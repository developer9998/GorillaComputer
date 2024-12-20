using GorillaComputer.Tools;
using GorillaComputer.Utilities;
using GorillaNetworking;
using GorillaTag;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaComputer.Behaviours
{
    internal class Computer : GorillaSliceableSimple
    {
        // public bool UseStartupMenu = true;

        public Action OnPopRequest = null;

        public GameObject StartupMenu, MainMenu;

        public List<Text> FunctionLineText = [];

        public Text Main, Summary, FunctionArrows, FunctionPage, ModTitleHeader, TimeHeader, StartupTimeHeader, StartupDateHeader, StartupLabel;

        private bool isSafeAccount;

        private int? _currentSelectionIndex = null;
        private float _textGlowValue = 0;

        private float _lastAutoRefresh;

        public void Awake()
        {
            enabled = false;
            isSafeAccount = PlayFabAuthenticator.instance.GetSafety();
        }

        public override void OnEnable()
        {
            step = GorillaSlicerSimpleManager.UpdateStep.Update;
            base.OnEnable();
        }

        public void OnDestroy()
        {
            OnPopRequest?.Invoke();
        }

        public void Initiate(Sprite sprite, bool startup)
        {
            var thread = new Thread(() => Initialize(sprite, startup));

            thread.Start();
            Logging.Info($"Computer {gameObject.scene.name} initalizing");

            thread.Join();
            Logging.Info("Initialized");
        }

        private void Initialize(Sprite sprite, bool startup)
        {
            // Computer assets

            transform.Find("Model/Stand").gameObject.AddComponent<GorillaSurfaceOverride>().overrideIndex = 262;
            transform.Find("Model/Display").gameObject.AddComponent<GorillaSurfaceOverride>().overrideIndex = 262;
            transform.Find("Model/Screen").gameObject.AddComponent<GorillaSurfaceOverride>().overrideIndex = 260;

            // Computer menus

            StartupMenu = transform.Find("Canvas/Startup Menu").gameObject;
            StartupMenu.SetActive(false);

            StartupTimeHeader = StartupMenu.transform.Find("TimeHeader").GetComponent<Text>();
            StartupDateHeader = StartupMenu.transform.Find("DateHeader").GetComponent<Text>();
            StartupLabel = StartupMenu.transform.Find("MainText").GetComponent<Text>();

            MainMenu = transform.Find("Canvas/Main Menu").gameObject;
            MainMenu.SetActive(false);

            // Navigation area

            FunctionArrows = MainMenu.transform.Find("FunctionArrow").GetComponent<Text>();
            FunctionPage = MainMenu.transform.Find("FunctionPageText").GetComponent<Text>();

            FunctionLineText.Capacity = Constants.PageCapacity;

            foreach (Transform child in MainMenu.transform)
            {
                string name = child.name;
                if (name.StartsWith("Function") && char.IsDigit(name.Last()))
                {
                    FunctionLineText.Add(child.GetComponent<Text>());
                    continue;
                }
            }

            // Client area

            Main = MainMenu.transform.Find("MainText").GetComponent<Text>();
            Summary = MainMenu.transform.Find("SummaryText").GetComponent<Text>();

            // Title bar

            TimeHeader = MainMenu.transform.Find("TimeHeader").GetComponent<Text>();
            ModTitleHeader = MainMenu.transform.Find("ModTitleHeader").GetComponent<Text>();
            ModTitleHeader.text = Constants.Name;

            if (!startup)
            {
                UpdateScreen();
                UpdateNavigation();
            }

            UpdateWallpaper(sprite);

            RevealStartup(startup);

            enabled = true;
        }

        public override void SliceUpdate()
        {
            if (GTAppState.isQuitting) return;

            if (Singleton<Main>.Instance.InStartupMenu)
            {
                UpdateStartupMenu();
            }
            else
            {
                UpdateMainMenu();
            }
        }

        private void UpdateStartupMenu()
        {
            DateTime now = DateTime.Now;

            StartupTimeHeader.text = now.ToString("hh:mm");
            StartupDateHeader.text = now.ToString("dddd, MMMM d");

            NetworkRegionInfo[] regionData = (NetworkRegionInfo[])AccessTools.Field(typeof(NetworkSystemPUN), "regionData").GetValue(NetworkSystem.Instance);

            int recentUserBans = (int)AccessTools.Field(typeof(GorillaNetworking.GorillaComputer), "usersBanned").GetValue(ComputerUtils.Computer);

            int globalPlayerCount = regionData == null || regionData.Length == 0 ? 0 : regionData.Select(rd => rd.playersInRegion).Sum();

            StartupLabel.text = $"Players Online: {globalPlayerCount:n0} - Recent Bans: {recentUserBans:n0}{(isSafeAccount ? "\n<color=red>Managed Account: Some settings may be disabled</color>" : "")}";
        }

        private void UpdateMainMenu()
        {
            if (_currentSelectionIndex.HasValue && _textGlowValue > 0)
            {
                FunctionLineText[_currentSelectionIndex.Value % Constants.PageCapacity].GetComponent<Outline>().effectColor = new Color(1f, 1f, 1f, _textGlowValue * 0.25f);
                _textGlowValue = Mathf.MoveTowards(_textGlowValue, 0f, 3f * Time.deltaTime);
            }

            if (Time.realtimeSinceStartup > _lastAutoRefresh + 1f)
            {
                UpdateScreen(null);
            }

            UpdateMainHeading();
        }

        private void UpdateMainHeading()
        {
            DateTime now = DateTime.Now;
            TimeHeader.text = now.ToString("hh:mm tt");
        }

        public void RevealStartup(bool startup)
        {
            Logging.Info($"RevealStartup ({startup})");

            Singleton<Main>.Instance.InStartupMenu = startup;
            ActivateMenuObjects(startup);
        }

        public void UpdateWallpaper(Sprite sprite)
        {
            if (sprite == null)
            {
                Logging.Error("Null wallpaper sprite");
                return;
            }

            transform.Find("Canvas/Persistent Menu/Backdrop").GetComponent<Image>().sprite = sprite;
        }

        public void ActivateMenuObjects(bool useStartupMenu)
        {
            StartupMenu.SetActive(useStartupMenu);
            MainMenu.SetActive(!useStartupMenu);
        }

        public void UpdateScreen(string content = null)
        {
            if (StartupMenu.activeSelf) return;

            _lastAutoRefresh = Time.realtimeSinceStartup;

            var functionOverride = Singleton<Main>.Instance.ScreenOverride;

            if (functionOverride != null)
            {
                Logging.Info("Database.FunctionOverride");

                Main.text = functionOverride.Content;
                Summary.text = $"{functionOverride.Title} - {functionOverride.Summary}";

                return;
            }

            var function = Singleton<Main>.Instance.ActiveScreen;

            Main.text = content ?? function.GetContent();
            Summary.text = $"{function.Title} - {function.Summary}";
        }

        public void UpdateNavigation()
        {
            if (StartupMenu.activeSelf) return;

            if (Singleton<Main>.Instance.ScreenOverride != null)
            {
                FunctionLineText.ForEach(text => text.text = "");

                FunctionPage.text = "";

                FunctionArrows.enabled = false;

                return;
            }

            IEnumerable<string> functionNames = Singleton<Main>.Instance.ScreenRegistry.Select(function => function.Title).Skip(Mathf.FloorToInt(Singleton<Main>.Instance.ActiveScreenIndex.Value / (float)Constants.PageCapacity) * Constants.PageCapacity).Take(Constants.PageCapacity);

            int functionCount = functionNames.Count();

            int currentSelectionIndex = Singleton<Main>.Instance.ActiveScreenIndex.Value % Constants.PageCapacity;

            if (!_currentSelectionIndex.HasValue || _currentSelectionIndex.Value != Singleton<Main>.Instance.ActiveScreenIndex.Value)
            {
                _currentSelectionIndex = Singleton<Main>.Instance.ActiveScreenIndex.Value;
                _textGlowValue = 1f;
            }

            for (int i = 0; i < Constants.PageCapacity; i++)
            {
                Text text = FunctionLineText.ElementAtOrDefault(i);

                if (text == null)
                {
                    Logging.Warning($"Function text at position {i} is null");
                    continue;
                }

                if (i >= functionCount)
                {
                    text.text = "";
                    continue;
                }

                text.text = functionNames.ElementAt(i);

                if (text != FunctionLineText[_currentSelectionIndex.Value % Constants.PageCapacity])
                {
                    text.GetComponent<Outline>().effectColor = new Color(1f, 1f, 1f, 0f);
                    continue;
                }

                text.GetComponent<Outline>().effectColor = new Color(1f, 1f, 1f, 0.25f);

                FunctionArrows.enabled = true;
                FunctionArrows.transform.localPosition = new Vector3(FunctionArrows.transform.localPosition.x, text.transform.localPosition.y + 0.55f, 0f);
            }

            FunctionPage.text = $"Page {Mathf.FloorToInt(Singleton<Main>.Instance.ActiveScreenIndex.Value / (float)Constants.PageCapacity) + 1}/{Mathf.CeilToInt(Singleton<Main>.Instance.ScreenRegistry.Count / (float)Constants.PageCapacity)}";
        }
    }
}
