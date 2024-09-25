using GorillaComputer.Function;
using GorillaComputer.Interface;
using GorillaComputer.Model;
using GorillaComputer.Patch;
using GorillaComputer.Tool;
using GorillaExtensions;
using GorillaNetworking;
using System.Collections.Generic;
using GorillaComputer.Extension;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Reflection;
using System.IO;

namespace GorillaComputer
{
    internal class Main : MonoBehaviourPunCallbacks, IFunctionDatabase
    {
        public List<ComputerFunction> FunctionRegistry { get; set; }
        public ComputerFunction CurrentFunction { get; set; }
        public int CurrentFunctionIndex { get; set; }
        public LazyFunctionOverride FunctionOverride { get; set; }

        private bool InStartupMenu = true;

        private Computer CurrentComputer => ComputerStack != null && ComputerStack.Count > 0 && ComputerStack.TryPeek(out Computer component) ? component : null;

        private Stack<Computer> ComputerStack = [];

        private Sprite Wallpaper;

        private AudioClip genericClick, functionClick;

        private Dictionary<string, ComputerSceneLocation> ComputerLocationDict = new()
        {
            {
                "GorillaTag",
                new ComputerSceneLocation()
                {
                    ComputerName = "StumpComputer",
                    ComputerLocation = "Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/GorillaComputerObject",
                    UseFallbackMethod = true
                }
            },
            {
                "Beach",
                new ComputerSceneLocation()
                {
                    ComputerName = "BeachComputer",
                    ComputerLocation = "Beach/BeachComputer (1)/GorillaComputerObject/",
                    UseFallbackMethod = true
                }
            },
            {
                "Cave",
                new ComputerSceneLocation()
                {
                    ComputerName = "CaveComputer",
                    ComputerLocation = "Cave_Main_Prefab/CaveComputer/-- Caves PhysicalComputer UI --",
                    UseFallbackMethod = true
                }
            },
            {
                "Basement",
                new ComputerSceneLocation()
                {
                    ComputerName = "BasementComputer",
                    ComputerLocation = "Basement/DungeonRoomAnchor/BasementComputer/GorillaComputerObject",
                    UseFallbackMethod = true
                }
            },
            {
                "Mountain",
                new ComputerSceneLocation()
                {
                    ComputerName = "MountainComputer",
                    ComputerLocation = "Mountain/Geometry/goodigloo/GorillaComputerObject",
                    UseFallbackMethod = true
                }
            },
            {
                "Skyjungle",
                new ComputerSceneLocation()
                {
                    ComputerName = "CloudsComputer",
                    ComputerLocation = "skyjungle/UI/GorillaComputerObject",
                    UseFallbackMethod = true
                }
            },
            {
                "Rotating",
                new ComputerSceneLocation()
                {
                    ComputerName = "RotatingComputer",
                    ComputerLocation = "RotatingPermanentEntrance/UI (1)/-- Rotating PhysicalComputer UI --",
                    UseFallbackMethod = true
                }
            },
            {
                "Metropolis",
                new ComputerSceneLocation()
                {
                    ComputerName = "MetroComputer",
                    ComputerLocation = "MetroMain/ComputerArea/GorillaComputerObject",
                    UseFallbackMethod = true
                }
            }
        };

        public async void Awake()
        {
            enabled = false;

            LogTool.Info("Initializing computer");

            await Task.Delay(100);

            LogTool.Info("Setup failure");

            LazyWarningPatch.CurrentFailureMessage.AddCallback(OnFailureRecieved);
            OnFailureRecieved(LazyWarningPatch.CurrentFailureMessage.value);

            #region Keyboard

            LogTool.Info("Keyboard");

            Key.OnKeyClicked = PressButton;

            genericClick = await AssetTool.LoadAsset<AudioClip>("Click");
            functionClick = await AssetTool.LoadAsset<AudioClip>("ClickLarge");

            #endregion

            #region Computer

            LogTool.Info("Computer");

            InitializeComputer(SceneManager.GetActiveScene(), ComputerLocationDict["GorillaTag"]);

            SceneManager.sceneLoaded += async delegate (Scene scene, LoadSceneMode loadMode)
            {
                LogTool.Info("Scene loaded");

                if (ComputerLocationDict.TryGetValue(scene.name, out ComputerSceneLocation location) && loadMode == LoadSceneMode.Additive)
                {
                    await Task.Delay(100); // small delay seems needed
                    InitializeComputer(scene, location);
                }
            };

            #endregion

            #region Function

            LogTool.Info("Function");

            FunctionRegistry = [];
            CurrentFunction = null;
            FunctionOverride = null;

            AddFunction(new RoomFunction());
            AddFunction(new NameFunction());
            AddFunction(new ColourFunction());
            AddFunction(new TurnFunction());
            AddFunction(new MicFunction());
            AddFunction(new QueueFunction());

            AddFunction(new GroupFunction());
            AddFunction(new VoiceFunction());
            AddFunction(new AutomodFunction());
            AddFunction(new ItemFunction());
            AddFunction(new CreditsFunction());
            AddFunction(new SupportFunction());

            AddFunction(new ModsFunction());

            LogTool.Info("Assemblies");

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            assemblies.Where(assembly => assembly != null && assembly.GetCustomAttribute<AutoRegisterAttribute>() != null).ForEach(assembly =>
            {
                try
                {
                    LogTool.Info($"Searching assembly {assembly.GetName().Name}");

                    var functionTypes = assembly.GetTypes().Where(page => page.GetCustomAttribute<AutoRegisterAttribute>() != null).ToArray();
                    if (functionTypes != null && functionTypes.Any())
                    {
                        foreach(var function in functionTypes)
                        {
                            try
                            {
                                LogTool.Info($"Adding function of type {function.FullName}");

                                ComputerFunction computerFunction = Activator.CreateInstance(function) as ComputerFunction ?? throw new InvalidCastException();
                                AddFunction(computerFunction);
                            }
                            catch(Exception ex)
                            {
                                LogTool.Error($"Error when adding function of type {function.FullName}: {ex}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogTool.Error($"Error when searching assembly {assembly.GetName().Name}: {ex}");
                }
            });

            #endregion

            #region Wallpaper

            LogTool.Info("Wallpaper");

            var wallpaperTex = await AssetTool.GetWallpaperTexture();

            Wallpaper = Sprite.Create(wallpaperTex, new Rect(0, 0, wallpaperTex.width, wallpaperTex.height), Vector2.zero);

            #endregion

            LogTool.Info("Initialize complete");

            enabled = true;
            ComputerTool.Computer.enabled = false;
        }

        public void AddFunction(ComputerFunction function)
        {
            if (function.IsParentalLocked && PlayFabAuthenticator.instance.GetSafety())
            {
                LogTool.Warning($"Function {function.GetType().Name} cannot be registered with parental lock");
                return;
            }

            if (FunctionRegistry.Contains(function))
            {
                LogTool.Warning($"Function {function.GetType().Name} is already included in GorillaComputer registry");
                return;
            }

            FunctionRegistry.Add(function);

            ComputerFunction.RequestUpdateMonitor += delegate (ComputerFunction currentFunction, string content)
            {
                if (currentFunction == function && CurrentFunction == function)
                {
                    CurrentComputer.UpdateAppearence(ComputerAppearanceFlags.Primary, content);
                }
            };
        }

        public void DefineFunction(int index)
        {
            ComputerFunction function = FunctionRegistry.ElementAtOrDefault(index);

            if (function == null)
            {
                LogTool.Error($"Function in registry at position {index} is not allocated for and cannot be set to");
                return;
            }

            CurrentFunction = function;
            CurrentFunctionIndex = index;

            LogTool.Info($"Current function is set to {CurrentFunction.Name} of type {CurrentFunction.GetType().Name}");

            function.OnFunctionOpened();
        }

        public void PressButton(Key pressedKey, bool isLeftHand)
        {
            if (!enabled)
            {
                LogTool.Warning("PressButton attempt while mod has not initialized");
                return;
            }

            var handPlayer = isLeftHand ? GorillaTagger.Instance.offlineVRRig.leftHandPlayer : GorillaTagger.Instance.offlineVRRig.rightHandPlayer;

            handPlayer.PlayOneShot(pressedKey.ClickSound, 0.8f);

            GorillaKeyboardBindings pressedBind = pressedKey.Binding;

            if (InStartupMenu)
            {
                InStartupMenu = false;

                DefineFunction(0);

                CurrentComputer.UpdateStartup(InStartupMenu);
                CurrentComputer.UpdateAppearence(ComputerAppearanceFlags.All);

                return;
            }

            if (pressedBind == GorillaKeyboardBindings.up)
            {
                DefineFunction(CurrentFunctionIndex > 0 ? CurrentFunctionIndex - 1 : FunctionRegistry.Count - 1);

                CurrentComputer.UpdateAppearence(ComputerAppearanceFlags.All);

                return;
            }

            if (pressedBind == GorillaKeyboardBindings.down)
            {
                DefineFunction(CurrentFunctionIndex >= FunctionRegistry.Count - 1 ? 0 : CurrentFunctionIndex + 1);

                CurrentComputer.UpdateAppearence(ComputerAppearanceFlags.All);

                return;
            }

            CurrentFunction.OnKeyPressed(pressedBind);
        }

        public async void InitializeComputer(Scene scene, ComputerSceneLocation location)
        {
            GameObject baseComputer = GameObject.Find(location.ComputerLocation);

            if (baseComputer == null && !location.UseFallbackMethod)
            {
                LogTool.Warning($"Computer {location.ComputerName} for scene {scene.name} could not be found, the fallback method is not permitted");
                return;
            }

            bool isUsingFallback = baseComputer == null;
            baseComputer ??= scene.GetComponentInHierarchy<GorillaComputerTerminal>(false)?.gameObject ?? null;

            if (baseComputer == null || !baseComputer.TryGetComponent(out GorillaComputerTerminal gct))
            {
                LogTool.Warning($"Computer {location.ComputerName} for scene {scene.name} could not be found, the fallback method was permitted and {(isUsingFallback ? "was" : "wasn't")} in use");
                return;
            }

            Transform computerUI = gct.transform.Find("ComputerUI");
            Transform computerTerminalScreen = computerUI ? computerUI.Find("monitor") : gct.monitorMesh.transform;

            GameObject monitor = Instantiate(await AssetTool.LoadAsset<GameObject>("Monitor"));
            monitor.name = location.ComputerName;

            Transform transform = monitor.transform;
            transform.SetParent(computerTerminalScreen);
            transform.localPosition = new Vector3(-0.0345f, -0.1091f, 0.207f);
            transform.localEulerAngles = new Vector3(270f, 180f, 0f);
            transform.localScale = Vector3.one * 0.63f;
            transform.SetParent(baseComputer.transform);

            Computer component = monitor.AddComponent<Computer>();

            if (CurrentComputer)
            {
                CurrentComputer.enabled = false;
            }

            ComputerStack.Push(component);

            component.Database = this;
            component.OnPopRequest = () =>
            {
                if (CurrentComputer == component)
                {
                    ComputerStack.Pop();

                    CurrentComputer.enabled = true;

                    CurrentComputer.UpdateWallpaper(Wallpaper);

                    CurrentComputer.UpdateAppearence(ComputerAppearanceFlags.All);

                    CurrentComputer.UpdateStartup(InStartupMenu);
                }
            };
            component.Construct(Wallpaper, InStartupMenu ? ComputerAppearanceFlags.None : ComputerAppearanceFlags.All, InStartupMenu);

            gct.myFunctionText?.gameObject?.SetActive(false);
            gct.myScreenText?.gameObject?.SetActive(false);

            if (computerUI)
            {
                computerUI.GetChild(0)?.gameObject?.SetActive(false);
                InitializeKeyboard(location, computerUI.GetChild(1).Find("Buttons/Keys"));
                return;
            }

            foreach (Transform t in gct.transform)
            {
                if (t.name.StartsWith("monitor"))
                {
                    t.gameObject.SetActive(false);
                }

                if (t.name.StartsWith("keyboard"))
                {
                    InitializeKeyboard(location, t.Find("Buttons/Keys"));
                }
            }
        }
        
        private void InitializeKeyboard(ComputerSceneLocation location, Transform buttonParent)
        {
            if (buttonParent == null)
            {
                LogTool.Error($"Computer {location.ComputerName} has a null keyboard that cannot be initialized");
                return;
            }

            var keyMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

            GorillaKeyboardButton[] keyboardButtons = buttonParent.GetComponentsInChildren<GorillaKeyboardButton>();

            if (keyboardButtons == null || keyboardButtons.Length == 0)
            {
                LogTool.Error($"Computer {location.ComputerName} has no keys to replace");
                return;
            }

            for(int i = 0; i < keyboardButtons.Length; i++)
            {
                var button = keyboardButtons[i];
                button.GetComponent<MeshFilter>().mesh = keyMesh;
                var component = button.AddComponent<Key>();
                component.ClickSound = component.Binding.IsFunctionKey() ? functionClick : genericClick;
            }
        }

        public void OnFailureRecieved(string failMessage)
        {
            if (failMessage == null || failMessage == "") return;

            failMessage = failMessage.ToSentenceCase().Replace("steam", "Steam").Replace("gorilla tag", "Gorilla Tag");

            LogTool.Warning($"howdy: {failMessage}");

            FunctionOverride = new LazyFunctionOverride()
            {
                Name = "Warning",
                Description = "Please read the entirety of the following message:",
                Content = failMessage
            };

            if (CurrentComputer)
            {
                CurrentComputer.UpdateAppearence(ComputerAppearanceFlags.Primary);
            }
        }

        public override void OnJoinedRoom() => ForceUpdateComputer();

        public override void OnLeftRoom() => ForceUpdateComputer();

        public override void OnPlayerEnteredRoom(Player newPlayer) => ForceUpdateComputer();

        public override void OnPlayerLeftRoom(Player otherPlayer) => ForceUpdateComputer();

        public void ForceUpdateComputer()
        {
            try
            {
                if (CurrentComputer && !InStartupMenu)
                {
                    CurrentComputer.UpdateAppearence(ComputerAppearanceFlags.Primary);
                }
            }
            catch
            {

            }
        }
    }
}
