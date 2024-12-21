using GorillaComputer.Extension;
using GorillaComputer.Tools;
using GorillaTagScripts;
using GorillaTagScripts.ModIO;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace GorillaComputer.Utilities
{
    /// <summary>
    /// Data based on the game computer
    /// </summary>
    public static class ComputerUtils
    {
        /// <summary>
        /// The GorillaComputer instance
        /// </summary>
        public static GorillaNetworking.GorillaComputer Computer => GorillaNetworking.GorillaComputer.instance;

        /// <summary>
        /// The active friend join collider
        /// </summary>
        public static GorillaFriendCollider FriendCollider => Computer.friendJoinCollider;

        /// <summary>
        /// Whether the local player is in a party
        /// </summary>
        public static bool InParty => FriendshipGroupDetection.Instance.IsInParty;

        /// <summary>
        /// Whether the party of the local player is in the friend join colliderr
        /// </summary>
        public static bool IsPartyWithinCollider => FriendshipGroupDetection.Instance.IsPartyWithinCollider(FriendCollider);

        /// <summary>
        /// Whether the local player is located in the Virtual Stump
        /// </summary>
        public static bool InVirtualStump => Computer.IsPlayerInVirtualStump();

        /// <summary>
        /// Whether a provided name is permitted for use based on blocked words
        /// </summary>
        public static bool IsNamePermitted(string name) => Computer.CheckAutoBanListForName(name);

        public static bool PlayerInVirtualStump => Computer.IsPlayerInVirtualStump();


        public static event Action<bool> SetInVirtualStump;

        public static void SetInVStump(bool inVStump) => SetInVirtualStump?.SafeInvoke(inVStump);


        /// <summary>
        /// Joins the room of the provided room code
        /// </summary>
        public static void JoinRoom(string roomCode)
        {
            if ((!InVirtualStump && roomCode == "") || (InVirtualStump && roomCode.Length == 1) || roomCode.Length > 10 || !IsNamePermitted(roomCode)) return;

            if (InParty && !IsPartyWithinCollider)
            {
                FriendshipGroupDetection.Instance.LeaveParty();
            }

            if (InVirtualStump)
            {
                CustomMapManager.UnloadMap(false);
            }

            GorillaNetworking.PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(roomCode, InParty ? GorillaNetworking.JoinType.JoinWithParty : GorillaNetworking.JoinType.Solo);
        }

        /// <summary>
        /// Leaves the current room
        /// </summary>
        public static async void LeaveRoom()
        {
            if (!NetworkSystem.Instance.InRoom) return;

            if (FriendshipGroupDetection.Instance.IsInParty)
            {
                FriendshipGroupDetection.Instance.LeaveParty();
                await Task.Delay(1000);
            }

            await NetworkSystem.Instance.ReturnToSinglePlayer();
        }

        /// <summary>
        /// Calls a method to initialize the colour of the local player's rig
        /// </summary>
        public static void InitializeMaterial(Color colour)
        {
            if (NetworkSystem.Instance.InRoom)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, colour.r, colour.g, colour.b);
            }
        }

        /// <summary>
        /// The name of the local player
        /// </summary>
        public static string Name
        {
            get
            {
                return NetworkSystem.Instance.GetMyNickName();
            }
            set
            {
                if (value == "" || value.Length > 12 || !IsNamePermitted(value)) return;

                NetworkSystem.Instance.SetMyNickName(value);
                ModIOMapsTerminal.RequestDriverNickNameRefresh();

                AccessTools.Method(Computer.GetType(), "SetNameTagText").Invoke(Computer, [value]);

                Computer.savedName = value;
                Computer.currentName = value;

                PlayerPrefs.SetString("playerName", value);
                PlayerPrefs.Save();

                InitializeMaterial(Colour);
            }
        }

        public static bool Nametags
        {
            get
            {
                return Computer.NametagsEnabled;
            }
            set
            {
                Computer.GetMethod("UpdateNametagSetting").Invoke(Computer, [value]);
            }
        }

        /// <summary>
        /// The color of the local player
        /// </summary>
        public static Color Color
        {
            get => Colour;
            set => Colour = value;
        }

        /// <summary>
        /// The colour of the local player
        /// </summary>
        public static Color Colour
        {
            get
            {
                float r = Mathf.Clamp01(PlayerPrefs.GetFloat("redValue", 0f));
                float g = Mathf.Clamp01(PlayerPrefs.GetFloat("greenValue", 0f));
                float b = Mathf.Clamp01(PlayerPrefs.GetFloat("blueValue", 0f));

                return new Color(r, g, b);
            }
            set
            {
                (float r, float g, float b) = (value.r, value.g, value.b);

                PlayerPrefs.SetFloat("redValue", Mathf.Clamp01(r));
                PlayerPrefs.SetFloat("greenValue", Mathf.Clamp01(g));
                PlayerPrefs.SetFloat("blueValue", Mathf.Clamp01(b));
                PlayerPrefs.Save();

                GorillaTagger.Instance.UpdateColor(r, g, b);

                InitializeMaterial(value);
            }
        }

        /// <summary>
        /// Acceptable turning modes for the computer
        /// </summary>
        public enum TurnMode
        {
            /// <summary>
            /// Player will turn in a sudden motion
            /// </summary>
            Snap,
            /// <summary>
            /// Player will turn over time slowly
            /// </summary>
            Smooth,
            /// <summary>
            /// Player will not turn
            /// </summary>
            None
        }

        /// <summary>
        /// The turn type of the local player (the mode of how the player turns, see the TurnMode enum)
        /// </summary>
        public static TurnMode TurnType
        {
            get
            {
                return PlayerPrefs.GetString("stickTurning", "SNAP") switch
                {
                    "SNAP" => TurnMode.Snap,
                    "SMOOTH" => TurnMode.Smooth,
                    "NONE" => TurnMode.None,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            set
            {
                string stickTurning = value.ToString().ToUpper();

                PlayerPrefs.SetString("stickTurning", stickTurning);
                PlayerPrefs.Save();

                AccessTools.Field(Computer.GetType(), "turnType").SetValue(Computer, stickTurning);

                GorillaTagger.Instance.GetComponent<GorillaSnapTurn>().ChangeTurnMode(stickTurning, (int)AccessTools.Field(Computer.GetType(), "turnValue").GetValue(Computer));
            }
        }

        /// <summary>
        /// The turn value of the local player (how much the player turns)
        /// </summary>
        public static int TurnValue
        {
            get
            {
                return PlayerPrefs.GetInt("turnFactor", 4);
            }
            set
            {
                PlayerPrefs.SetInt("turnFactor", value);
                PlayerPrefs.Save();

                (var turnType, var turnValue) = (AccessTools.Field(Computer.GetType(), "turnType"), AccessTools.Field(Computer.GetType(), "turnValue"));

                GorillaTagger.Instance.GetComponent<GorillaSnapTurn>().ChangeTurnMode((string)turnType.GetValue(Computer), value);

                turnValue.SetValue(Computer, value);
            }
        }

        /// <summary>
        /// Acceptable push-to-talk modes for the computer
        /// </summary>
        public enum PTTMode
        {
            /// <summary>
            /// Voice will always be transmitted
            /// </summary>
            AllChat,
            /// <summary>
            /// Voice will be transmitted if a face button is held
            /// </summary>
            PushToTalk,
            /// <summary>
            /// Voice will be transmitted if face buttons are released
            /// </summary>
            PushToMute
        }

        /// <summary>
        /// The push-to-talk type used by the local player (see the PTTMode enum)
        /// </summary>
        public static PTTMode PushToTalkType
        {
            get
            {
                return Computer.pttType switch
                {
                    "ALL CHAT" => PTTMode.AllChat,
                    "PUSH TO TALK" => PTTMode.PushToTalk,
                    "PUSH TO MUTE" => PTTMode.PushToMute,
                    _ => PTTMode.AllChat
                };
            }
            set
            {
                Computer.pttType = value switch
                {
                    PTTMode.AllChat => "ALL CHAT",
                    PTTMode.PushToTalk => "PUSH TO TALK",
                    PTTMode.PushToMute => "PUSH TO MUTE",
                    _ => "ADD CHAT"
                };
            }
        }

        /// <summary>
        /// The current queue of the local player (set the queue with the JoinQueue method to align with troop, recommended)
        /// </summary>
        public static string Queue
        {
            get
            {
                return Computer.currentQueue;
            }
            set
            {
                Computer.currentQueue = value;
                PlayerPrefs.SetString("currentQueue", Computer.currentQueue);
            }
        }

        /// <summary>
        /// The population of the local player's troop
        /// </summary>
        public static int TroopPopulation
        {
            get
            {
                return (int)Computer.GetField("currentTroopPopulation").GetValue(Computer);
            }
            set
            {
                Computer.GetField("currentTroopPopulation").SetValue(Computer, value);
            }
        }

        /// <summary>
        /// Whether the queue of local player's troop is active
        /// </summary>
        public static bool TroopQueueActive
        {
            get
            {
                return Computer.troopQueueActive;
            }
            set
            {
                Computer.troopQueueActive = value;
                PlayerPrefs.SetInt("troopQueueActive", value ? 1 : 0);
            }
        }

        /// <summary>
        /// The name of the local player's troop
        /// </summary>
        public static string TroopName
        {
            get
            {
                return Computer.troopName;
            }
            set
            {
                Computer.troopName = value;
                PlayerPrefs.SetString("troopName", Computer.troopName);
            }
        }

        /// <summary>
        /// The top five troops being used by players
        /// </summary>
        public static List<string> TopTroops
        {
            get
            {
                var list = (List<string>)Computer.GetField("topTroops").GetValue(Computer);
                return list.Skip(1).ToList();
            }
        }

        /// <summary>
        /// Whether the local player is in a troop
        /// </summary>
        public static bool InTroop => TroopName != "";

        /// <summary>
        /// Joins a queue with a provided name and if the name/queue is based on a troop
        /// </summary>
        public static void JoinQueue(string queueName, bool isTroopQueue = false)
        {
            Queue = queueName;
            TroopQueueActive = isTroopQueue;
            TroopPopulation = -1;
        }

        /// <summary>
        /// Whether a provided Troop name is valid, and can be used
        /// </summary>
        public static bool IsValidTroopName(string troop) => !string.IsNullOrEmpty(troop) && troop.Length <= 12 && (IsCompetitiveAllowed || troop != "COMPETITIVE") && IsNamePermitted(troop);

        /// <summary>
        /// Joins the provided Troop
        /// </summary>
        public static void JoinTroop(string troop)
        {
            if (!IsValidTroopName(troop))
            {
                Logging.Warning($"Troop name '{troop}' is not valid");
                return;
            }

            TroopPopulation = -1;
            TroopName = troop;
            RequestTroopPopulation();
            if (TroopQueueActive) Queue = troop;

            PlayerPrefs.Save();

            JoinTroopQueue();
        }

        /// <summary>
        /// Leaves the current Troop
        /// </summary>
        public static void LeaveTroop()
        {
            if (IsValidTroopName(TroopName))
            {
                Computer.troopToJoin = TroopName;
            }

            TroopPopulation = -1;
            TroopName = "";
            JoinQueue("DEFAULT", false);
        }

        /// <summary>
        /// Joins the current Troop queue
        /// </summary>
        public static void JoinTroopQueue()
        {
            if (!IsValidTroopName(TroopName))
            {
                Logging.Warning($"Troop name '{TroopName}' is not valid");
                return;
            }

            TroopPopulation = -1;
            JoinQueue(TroopName, true);
            RequestTroopPopulation();
        }

        /// <summary>
        /// Requests to update the population of the current Troop's playerbase
        /// </summary>
        public static void RequestTroopPopulation(bool forceUpdate = true)
        {
            Computer.GetField("troopPopulationCheckCooldown").SetValue(Computer, 0.5f);
            Computer.GetMethod("RequestTroopPopulation").Invoke(Computer, [forceUpdate]);
        }

        public static bool IsCompetitiveAllowed => Computer.allowedInCompetitive;

        public static string[] AllowedMaps
        {
            get
            {
                var maps = Computer.allowedMapsToJoin;
                if (maps.Length > 5) return maps.Take(5).ToArray();
                return maps;
            }
        }

        public static string GroupMap => AllowedMaps.Length > 1 ? Computer.groupMapJoin : AllowedMaps.First().ToUpper();

        public static void SetGroupMap(string map, int index)
        {
            Computer.groupMapJoin = map;
            Computer.groupMapJoinIndex = index;

            PlayerPrefs.SetString("groupMapJoin", Computer.groupMapJoin);
            PlayerPrefs.SetInt("groupMapJoinIndex", Computer.groupMapJoinIndex);
            PlayerPrefs.Save();
        }

        public static void JoinGroupMap() => Computer.OnGroupJoinButtonPress(Mathf.Min(AllowedMaps.Length - 1, Computer.groupMapJoinIndex), FriendCollider);

        public static void RefreshRigVoices() => RigContainer.RefreshAllRigVoices();

        public static bool UseVoiceChat
        {
            get
            {
                return Computer.voiceChatOn == "TRUE";
            }
            set
            {
                string useVc = value ? "TRUE" : "FALSE";

                Computer.voiceChatOn = useVc;

                PlayerPrefs.SetString("voiceChatOn", useVc);
                PlayerPrefs.Save();

                RefreshRigVoices();
            }
        }

        public static bool ItemParticles
        {
            get
            {
                return !Computer.disableParticles;
            }
            set
            {
                Computer.disableParticles = !value;

                PlayerPrefs.SetString("disableParticles", Computer.disableParticles ? "TRUE" : "FALSE");
                PlayerPrefs.Save();

                GorillaTagger.Instance.ShowCosmeticParticles(value);
            }
        }

        public static float InstrumentVolume
        {
            get
            {
                return Computer.instrumentVolume;
            }
            set
            {
                Computer.instrumentVolume = value;

                PlayerPrefs.SetFloat("instrumentVolume", value);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Acceptable auto mute modes for the computer
        /// </summary>
        public enum AutomodMode
        {
            /// <summary>
            /// No auto mute calculations are done
            /// </summary>
            Off,
            /// <summary>
            /// Moderate auto mute calculations are done
            /// </summary>
            Moderate,
            /// <summary>
            /// Aggressive auto mute calculations are done
            /// </summary>
            Aggressive
        }

        public static AutomodMode AutoMute
        {
            get
            {
                return (AutomodMode)PlayerPrefs.GetInt("autoMute", 1);
            }
            set
            {
                Computer.autoMuteType = value.ToString().ToUpper();

                PlayerPrefs.SetInt("autoMute", (int)value);
                PlayerPrefs.Save();

                RefreshRigVoices();
            }
        }

        public static int CreditPageCount
        {
            get
            {
                GorillaNetworking.CreditsView creditsView = Computer.creditsView;
                return (int)creditsView.GetProperty("TotalPages").GetValue(creditsView);
            }
        }

        public static int CreditCurrentPage
        {
            get
            {
                GorillaNetworking.CreditsView creditsView = Computer.creditsView;

                return (int)creditsView.GetField("currentPage").GetValue(creditsView);
            }
            set
            {
                GorillaNetworking.CreditsView creditsView = Computer.creditsView;

                creditsView.GetField("currentPage").SetValue(creditsView, value < 0 ? CreditPageCount + value : value % CreditPageCount);
            }
        }

        public static (string Title, List<string> Entries, bool Continue) CreditGetPage(int page)
        {
            GorillaNetworking.CreditsView creditsView = Computer.creditsView;

            object pageEntries = creditsView.GetMethod("GetPageEntries").Invoke(creditsView, [page]);

            object currentSection = pageEntries.GetField("Item1").GetValue(pageEntries);

            int currentSubPage = (int)pageEntries.GetField("Item2").GetValue(pageEntries);

            string title = (string)currentSection.GetProperty("Title").GetValue(currentSection);

            IEnumerable<string> entries = (IEnumerable<string>)creditsView.GetMethod("PageOfSection").Invoke(creditsView, [currentSection, currentSubPage]);

            return (title, entries.ToList(), currentSubPage > 0);
        }

        public static Dictionary<string, string> SupportData
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {
                        "Player ID",
                        GorillaNetworking.PlayFabAuthenticator.instance.GetPlayFabPlayerId()
                    },
                    {
                        "Platform",
                        $"{(PlatformTagJoin)AccessTools.Field(typeof(GorillaNetworking.PlayFabAuthenticator), "platform").GetValue(GorillaNetworking.PlayFabAuthenticator.instance)} (Modded)"
                    },
                    {
                        "Build Version",
                        Computer.version
                    },
                    {
                        "Build Date",
                        Computer.buildDate
                    }
                };
            }
        }
    }
}
