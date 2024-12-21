using GorillaComputer.Extension;
using GorillaComputer.Models;
using GorillaComputer.Patches;
using GorillaComputer.Screens;
using GorillaComputer.Tools;
using GorillaComputer.Utilities;
using GorillaExtensions;
using GorillaNetworking;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GorillaComputer.Behaviours
{
    internal class Main : Singleton<Main>
    {
        /// <summary>
        /// Queue of terminals to upgraded
        /// </summary>
        public static readonly UniqueQueue<GorillaComputerTerminal> TerminalQueue = [];

        /// <summary>
        /// List of terminals that have been included in our queue, less so "upgraded"
        /// </summary>
        private static readonly List<GorillaComputerTerminal> UpgradedTerminals = [];

        // Computers

        /// <summary>
        /// The active computer being used (should only be used in TryGetComputer, then cached)
        /// </summary>
        private Computer ActiveComputer => ComputerStack.TryPeek(out Computer component) ? component : null;

        /// <summary>
        /// The generic stack of initialized computers, the computer at the top represents our active computer
        /// </summary>
        private readonly Stack<Computer> ComputerStack = [];

        // Screens

        /// <summary>
        /// The active screen being used, if there is one (based on a nullable integer)
        /// </summary>
        public ComputerScreen ActiveScreen => ActiveScreenIndex.HasValue ? ScreenRegistry[ActiveScreenIndex.Value] : null;

        public int? ActiveScreenIndex;

        /// <summary>
        /// List of all screen behaviours added to this object
        /// </summary>
        public List<ComputerScreen> ScreenRegistry = [];

        /// <summary>
        /// The override screen, often used to provide insight with a message (connection issues, moderation actions, etc.)
        /// </summary>
        public ScreenOverride ScreenOverride = null;

        /// <summary>
        /// Whether we are currently booted into our start menu
        /// </summary>
        public bool InStartupMenu = true;

        // Assets
        public Sprite Wallpaper;
        public AudioClip KeyClickSmall, KeyClickLarge;

        protected async override void Initialize()
        {
            base.Initialize();

            enabled = false;

            // Assets

            var wallpaperTexture = await AssetLoader.GetWallpaperTexture();
            Wallpaper = Sprite.Create(wallpaperTexture, new Rect(0, 0, wallpaperTexture.width, wallpaperTexture.height), Vector2.zero);

            KeyClickSmall = await AssetLoader.LoadAsset<AudioClip>("Click");
            KeyClickLarge = await AssetLoader.LoadAsset<AudioClip>("ClickLarge");

            // Screens

            var builtinScreens = new List<Type>()
            {
                typeof(RoomScreen),
                typeof(NameScreen),
                typeof(ColourScreen),
                typeof(TurnScreen),
                typeof(QueueScreen),
                typeof(TroopScreen),
                typeof(GroupScreen),
                typeof(VoiceScreen),

                typeof(AutomodScreen),
                typeof(ItemScreen),
                typeof(RedeemScreen),
                typeof(CredtsScreen),
                typeof(SupportScreen),
                typeof(ModsScreen)
            };

            builtinScreens.ForEach(RegisterScreen);

            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                assemblies.Where(assembly => assembly != null && assembly.GetCustomAttribute<ComputerScannableAttribute>() != null).ForEach(assembly =>
                {
                    try
                    {
                        Logging.Info($"Searching assembly {assembly.GetName().Name}");

                        var types = assembly.GetTypes();
                        types.Where(page => page.GetCustomAttribute<ComputerCustomScreenAttribute>() != null).ForEach(RegisterScreen);
                    }
                    catch (Exception ex)
                    {
                        Logging.Fatal($"Exception thrown when searching assembly {assembly.GetName().Name}");
                        Logging.Error(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Logging.Fatal("Exception thrown when performing initial assembly check");
                Logging.Error(ex);
            }

            ComputerScreen.UpdateScreenAction += delegate (ComputerScreen screen, string content)
            {
                if (!screen.enabled || !TryGetComputer(out var computer)) return;

                if (ActiveScreen == screen)
                {
                    computer.UpdateScreen(content);
                }
            };

            // Failure message

            FailureMessagePatch.CurrentFailureMessage.AddCallback(OnFailureRecieved);
            OnFailureRecieved(FailureMessagePatch.CurrentFailureMessage.value);

            // Computer

            SceneIndex.MonkeBlocks.AddCallbackOnSceneLoad(() => CheckScene(SceneManager.GetSceneByBuildIndex((int)SceneIndex.MonkeBlocks), null));

            ComputerKey.OnKeyClicked = PressButton;

            enabled = true;
            ComputerUtils.Computer.enabled = false;
        }

        public void Update()
        {
            if (TerminalQueue.Count > 0)
            {
                var queuedTerminal = TerminalQueue.Dequeue();
                InitializeComputer(queuedTerminal);
            }
        }

        public void RegisterScreen(Type type)
        {
            try
            {
                ComputerScreen screen = gameObject.AddComponent(type) as ComputerScreen;
                screen.enabled = false;
                RegisterScreen(screen);
            }
            catch (Exception ex)
            {
                Logging.Fatal($"Exception thrown when registering screen from type {type.FullName}");
                Logging.Error(ex);
            }
        }

        public void RegisterScreen(ComputerScreen newScreen)
        {
            if (newScreen.IsParentalLocked && PlayFabAuthenticator.instance.GetSafety())
            {
                Logging.Warning($"Screen {newScreen.GetType().Name} cannot be registered with parental lock");
                return;
            }

            if (ScreenRegistry.Contains(newScreen))
            {
                Logging.Warning($"Screen {newScreen.GetType().Name} is already included in GorillaComputer registry");
                return;
            }

            ScreenRegistry.Add(newScreen);
        }

        public void SwitchScreen(int index)
        {
            ComputerScreen screen = ScreenRegistry.ElementAtOrDefault(index);

            try
            {
                var lastScreen = ActiveScreen;
                if (lastScreen) lastScreen.enabled = false; // Disable our old screen - if one exists

                ActiveScreenIndex = index;

                var newScreen = ActiveScreen;
                newScreen.enabled = true; // Enable our new screen

                Logging.Info($"Current screen is set to {ActiveScreen.Title} of type {ActiveScreen.GetType().Name}");

                screen.OnScreenShow();
            }
            catch (Exception ex)
            {
                Logging.Fatal($"Exception thrown when showing screen #{index}");
                Logging.Error(ex);
            }
        }

        public void PressButton(ComputerKey key, bool isLeftHand)
        {
            var handPlayer = isLeftHand ? GorillaTagger.Instance.offlineVRRig.leftHandPlayer : GorillaTagger.Instance.offlineVRRig.rightHandPlayer;
            handPlayer.PlayOneShot(key.ClickSound, 0.8f);

            InitiateBindingAction(key.Binding);
        }

        public void InitiateBindingAction(KeyBinding binding)
        {
            if (!TryGetComputer(out var computer)) return;

            if (InStartupMenu)
            {
                InStartupMenu = false;

                SwitchScreen(0);
                computer.RevealStartup(false);
                computer.UpdateScreen();
                computer.UpdateNavigation();

                return;
            }

            if (binding == KeyBinding.up)
            {
                SwitchScreen(ActiveScreenIndex.Value > 0 ? ActiveScreenIndex.Value - 1 : ScreenRegistry.Count - 1);
                computer.UpdateScreen();
                computer.UpdateNavigation();

                return;
            }

            if (binding == KeyBinding.down)
            {
                SwitchScreen(ActiveScreenIndex.Value >= ScreenRegistry.Count - 1 ? 0 : ActiveScreenIndex.Value + 1);
                computer.UpdateScreen();
                computer.UpdateNavigation();

                return;
            }

            ActiveScreen.ProcessScreen(binding);
        }

        public async void InitializeComputer(GorillaComputerTerminal terminal)
        {
            Transform computerUI = terminal.transform.Find("ComputerUI");
            Transform computerTerminalScreen = computerUI ? (computerUI.Find("monitor") ?? terminal.monitorMesh.transform) : terminal.monitorMesh.transform;

            GameObject monitor = Instantiate(await AssetLoader.LoadAsset<GameObject>("Monitor"));
            monitor.name = "GorillaComputer";

            Transform transform = monitor.transform;
            transform.SetParent(computerTerminalScreen);
            transform.localPosition = new Vector3(-0.0345f, -0.109f, 0.205f);
            transform.localEulerAngles = new Vector3(270f, 180f, 0f);
            transform.localScale = Vector3.one * 0.63f;
            transform.SetParent(terminal.transform);

            Computer component = monitor.AddComponent<Computer>();

            ComputerStack.Push(component); // PUSH!

            component.OnPopRequest = () =>
            {
                // Active computer, pre-pop
                TryGetComputer(out var computer);

                // Compare the computer requesting to pop to our active computer
                if (computer == component)
                {
                    ComputerStack.Pop(); // POP!

                    // Active computer, post-pop
                    TryGetComputer(out computer);

                    computer.enabled = true;

                    computer.UpdateWallpaper(Wallpaper);

                    computer.UpdateScreen();

                    computer.UpdateNavigation();

                    computer.RevealStartup(InStartupMenu);
                }
            };

            // Initiates an initialization on the computer, done on a different thread to not get in the way of other operations (i.e, cosmetics, other mods)
            component.Initiate(Wallpaper, InStartupMenu);

            // These fields in the terminal have to be assigned for the game to build
            // (see GorillaComputerTerminal.BuildValidationCheck)
            terminal.myFunctionText.gameObject.SetActive(false);
            terminal.myScreenText.gameObject.SetActive(false);

            if (computerUI)
            {
                computerUI.GetChild(0)?.gameObject?.SetActive(false);
                InitializeKeyboard(component, computerUI.GetChild(1).Find("Buttons/Keys"));
                return;
            }

            foreach (Transform t in terminal.transform)
            {
                if (t.name.StartsWith("monitor"))
                {
                    // Hide any monitor objects
                    t.gameObject.SetActive(false);
                }

                if (t.name.StartsWith("keyboard"))
                {
                    InitializeKeyboard(component, t.Find("Buttons/Keys"));
                }
            }
        }

        private void InitializeKeyboard(Computer computer, Transform buttonParent)
        {
            if (buttonParent == null)
            {
                Logging.Error($"Computer has a null keyboard that cannot be initialized");
                return;
            }

            var keyMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

            GorillaKeyboardButton[] keyboardButtons = buttonParent.GetComponentsInChildren<GorillaKeyboardButton>();

            if (keyboardButtons == null || keyboardButtons.Length == 0)
            {
                Logging.Error($"Computer has no keys to replace");
                return;
            }

            for (int i = 0; i < keyboardButtons.Length; i++)
            {
                var button = keyboardButtons[i];
                button.GetComponent<MeshFilter>().mesh = keyMesh;

                var key = button.AddComponent<ComputerKey>();
                key.Computer = computer;
                key.ClickSound = key.Binding.IsFunctionKey() ? KeyClickLarge : KeyClickSmall;
            }
        }

        public void OnFailureRecieved(string failMessage)
        {
            if (failMessage == null || failMessage == "") return;

            failMessage = failMessage.ToSentenceCase().Replace("steam", "Steam").Replace("gorilla tag", "Gorilla Tag");

            Logging.Warning($"howdy: {failMessage}");

            ScreenOverride = new ScreenOverride()
            {
                Title = "Warning",
                Summary = "Please read the entirety of the following message:",
                Content = failMessage
            };

            if (TryGetComputer(out var computer))
            {
                computer.UpdateScreen();
                computer.UpdateNavigation();
            }
        }

        public bool TryGetComputer(out Computer computer)
        {
            computer = ActiveComputer;
            return computer;
        }

        public static void QueueTerminal(GorillaComputerTerminal terminal)
        {
            if (UpgradedTerminals.Contains(terminal)) return;
            UpgradedTerminals.Add(terminal);

            if (CheckScene(terminal.gameObject.scene, terminal))
            {
                TerminalQueue.Enqueue(terminal);
            }
        }

        public static bool CheckScene(Scene scene, GorillaComputerTerminal terminal)
        {
            int buildIndex = scene.buildIndex;

            if (!Enum.IsDefined(typeof(SceneIndex), buildIndex))
            {
                Logging.Info("Valid custom terminal");
                return true;
            }

            var sceneIndex = (SceneIndex)buildIndex;

            if (sceneIndex == SceneIndex.MonkeBlocks && !terminal)
            {
                terminal = scene.GetComponentInHierarchy<GorillaComputerTerminal>(true);
                if (terminal)
                {
                    terminal.transform.parent.gameObject.SetActive(true);
                    Logging.Info("Valid 'MonkeBlocks' terminal");
                    return true;
                }
                return false;
            }

            var path = terminal.gameObject.GetPath();

            if (sceneIndex == SceneIndex.GT && (path.Contains("MonkeBlocksRoomPersistent") || path.Contains("VirtualStump"))) // CHANGE LAST BIT FOR WHEN I GET A COMPUTER MODEL FOR VSTUMP!
            {
                terminal.transform.parent.gameObject.SetActive(false);
                Logging.Info("Invalid duplicate terminal");
                return false;
            }

            Logging.Info($"Valid '{sceneIndex}' terminal {terminal.name}");
            return true;
        }
    }
}
