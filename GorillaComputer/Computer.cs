using GorillaComputer.Interface;
using GorillaComputer.Model;
using GorillaComputer.Tool;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaComputer
{
    public class Computer : MonoBehaviour
    {
        public IFunctionDatabase Database;

        public bool UseStartupMenu = true;

        public Action OnPopRequest = null;

        public GameObject StartupMenu, MainMenu;

        public Text[] FunctionLineText;
        public Text Main, Summary, FunctionArrows, FunctionPage, ModTitleHeader, TimeHeader, StartupTimeHeader, StartupDateHeader, StartupLabel, PingText;

        public Image PingImage;

        public Sprite Bar1, Bar2, Bar3, Bar4;

        private float Ping;

        private bool isSafeAccount;

        private int? _currentSelectionIndex = null;
        private float _textGlowValue = 0;

        private float _lastAutoRefresh;

        public void Awake()
        {
            enabled = false;
            isSafeAccount = PlayFabAuthenticator.instance.GetSafety();
        }

        public void OnDestroy()
        {
            OnPopRequest?.Invoke();
        }

        public async void Construct(ComputerAppearanceFlags appearance, bool startup)
        {
            transform.Find("Model/Stand").gameObject.AddComponent<GorillaSurfaceOverride>().overrideIndex = 262;
            transform.Find("Model/Display").gameObject.AddComponent<GorillaSurfaceOverride>().overrideIndex = 262;
            transform.Find("Model/Screen").gameObject.AddComponent<GorillaSurfaceOverride>().overrideIndex = 260;

            static T GetGraphic<T>(GameObject menu, string name) where T : Graphic
            {
                return menu.transform.Find(name).GetComponent<T>();
            }

            StartupMenu = transform.Find("Canvas/Startup Menu").gameObject;

            StartupTimeHeader = StartupMenu.transform.Find("TimeHeader").GetComponent<Text>();
            StartupDateHeader = StartupMenu.transform.Find("DateHeader").GetComponent<Text>();
            StartupLabel = StartupMenu.transform.Find("MainText").GetComponent<Text>();

            MainMenu = transform.Find("Canvas/Main Menu").gameObject;

            FunctionLineText = [GetGraphic<Text>(MainMenu, "Function1"), GetGraphic<Text>(MainMenu, "Function2"), GetGraphic<Text>(MainMenu, "Function3"), GetGraphic<Text>(MainMenu, "Function4"), GetGraphic<Text>(MainMenu, "Function5"), GetGraphic<Text>(MainMenu, "Function6")];

            Main = MainMenu.transform.Find("MainText").GetComponent<Text>();
            Summary = MainMenu.transform.Find("SummaryText").GetComponent<Text>();
            FunctionArrows = MainMenu.transform.Find("FunctionArrow").GetComponent<Text>();
            FunctionPage = MainMenu.transform.Find("FunctionPageText").GetComponent<Text>();
            ModTitleHeader = MainMenu.transform.Find("ModTitleHeader").GetComponent<Text>();
            TimeHeader = MainMenu.transform.Find("TimeHeader").GetComponent<Text>();
            PingText = MainMenu.transform.Find("PingText").GetComponent<Text>();
            PingImage = MainMenu.transform.Find("PingImage").GetComponent<Image>();
            PingImage.enabled = false;

            UpdateStartup(startup);
            UpdateAppearence(appearance);

            Bar1 = await AssetTool.LoadAsset<Sprite>("Bar1");
            Bar2 = await AssetTool.LoadAsset<Sprite>("Bar2");
            Bar3 = await AssetTool.LoadAsset<Sprite>("Bar3");
            Bar4 = await AssetTool.LoadAsset<Sprite>("Bar4");

            enabled = true;
        }

        public void Update()
        {
            if (UseStartupMenu)
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

            int recentUserBans = (int)AccessTools.Field(typeof(GorillaNetworking.GorillaComputer), "usersBanned").GetValue(ComputerTool.Computer);

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
                UpdateContent(Database.CurrentFunction.GetFunctionContent());
            }

            UpdateMainHeading();
        }

        private void UpdateMainHeading()
        {
            DateTime now = DateTime.Now;

            TimeHeader.text = now.ToString("hh:mm tt");

            int currentPing = PhotonTool.GetPing();
            bool inRoom = PhotonNetwork.InRoom;

            if (currentPing == -1 || !inRoom)
            {
                PingImage.enabled = false;
                PingText.enabled = false;
                Ping = 0;
                return;
            }

            if (!PingImage.enabled)
            {
                PingImage.enabled = true;
                PingText.enabled = true;
            }

            Ping = Mathf.MoveTowards(Ping, currentPing, 30f * Time.deltaTime);

            PingText.text = Mathf.FloorToInt(Ping).ToString();

            if (0 <= Ping && Ping <= 50)
            {
                PingImage.sprite = Bar4;
            }
            else if (50 <= Ping && Ping <= 60)
            {
                PingImage.sprite = Bar3;
            }
            else if (60 <= Ping && Ping <= 90)
            {
                PingImage.sprite = Bar2;
            }
            else if (90 <= Ping && Ping <= 120)
            {
                PingImage.sprite = Bar1;
            }
        }

        public void UpdateStartup(bool startup)
        {
            UseStartupMenu = startup;
            ActivateMenuObjects(UseStartupMenu);
        }

        public void UpdateAppearence(ComputerAppearanceFlags appearance, string content = null)
        {
            if (appearance.HasFlag(ComputerAppearanceFlags.Primary))
            {
                UpdateContent(content);
            }

            if (appearance.HasFlag(ComputerAppearanceFlags.Navigation))
            {
                UpdateNavigation();
            }
        }

        public void ActivateMenuObjects(bool useStartupMenu)
        {
            StartupMenu.SetActive(useStartupMenu);
            MainMenu.SetActive(!useStartupMenu);
        }

        public void UpdateContent(string content)
        {
            _lastAutoRefresh = Time.realtimeSinceStartup;

            if (Database.FunctionOverride != null)
            {
                Main.text = Database.FunctionOverride.Content;
                Summary.text = $"{Database.FunctionOverride.Name} - {Database.FunctionOverride.Description}";
                return;
            }

            Main.text = content ?? Database.CurrentFunction.GetFunctionContent();
            Summary.text = $"{Database.CurrentFunction.Name} - {Database.CurrentFunction.Description}";
        }

        public void UpdateNavigation()
        {
            if (Database.FunctionOverride != null)
            {
                FunctionLineText.ForEach(text => text.text = "");
                FunctionPage.text = "";
                FunctionArrows.enabled = false;
                return;
            }

            IEnumerable<string> functionNames = Database.FunctionRegistry.Select(function => function.Name).Skip(Mathf.FloorToInt(Database.CurrentFunctionIndex / (float)6) * 6).Take(6);
            int functionCount = functionNames.Count();

            int currentSelectionIndex = Database.CurrentFunctionIndex % Constants.PageCapacity;

            if (!_currentSelectionIndex.HasValue || _currentSelectionIndex.Value != Database.CurrentFunctionIndex)
            {
                _currentSelectionIndex = Database.CurrentFunctionIndex;
                _textGlowValue = 1f;
            }

            for (int i = 0; i < Constants.PageCapacity; i++)
            {
                Text text = FunctionLineText.ElementAtOrDefault(i);

                if (text == null)
                {
                    LogTool.Warning($"Function text at position {i} is null");
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

            FunctionPage.text = $"Page {Mathf.FloorToInt(Database.CurrentFunctionIndex / (float)6) + 1}/{Mathf.CeilToInt(Database.FunctionRegistry.Count / (float)6)}";
        }
    }
}
