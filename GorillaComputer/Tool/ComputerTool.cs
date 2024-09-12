using GorillaComputer.Extension;
using GorillaNetworking;
using GorillaTagScripts;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace GorillaComputer.Tool
{
    public static class ComputerTool
    {
        public static GorillaNetworking.GorillaComputer Computer => GorillaNetworking.GorillaComputer.instance;

        public static GorillaFriendCollider FriendCollider => Computer.friendJoinCollider;

        public static bool IsPartyWithinCollider => FriendshipGroupDetection.Instance.IsPartyWithinCollider(FriendCollider);

        public static bool IsNamePermitted(string name) => Computer.CheckAutoBanListForName(name);

        public static void JoinRoom(string roomCode)
        {
            if (roomCode == "" || roomCode.Length > 10 || !IsNamePermitted(roomCode)) return;

            if (FriendshipGroupDetection.Instance.IsInParty && !IsPartyWithinCollider)
            {
                FriendshipGroupDetection.Instance.LeaveParty();
            }

            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(roomCode, FriendshipGroupDetection.Instance.IsInParty ? JoinType.JoinWithParty : JoinType.Solo);
        }

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

        public static void InitializeMaterial(Color colour)
        {
            if (NetworkSystem.Instance.InRoom)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, colour.r, colour.g, colour.b);
            }
        }

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

                Computer.offlineVRRigNametagText.text = value;
                Computer.savedName = value;
                Computer.currentName = value;

                PlayerPrefs.SetString("playerName", value);
                PlayerPrefs.Save();

                InitializeMaterial(Colour);
            }
        }

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
                float r = value.r;
                float g = value.g;
                float b = value.b;

                PlayerPrefs.SetFloat("redValue", Mathf.Clamp01(r));
                PlayerPrefs.SetFloat("greenValue", Mathf.Clamp01(g));
                PlayerPrefs.SetFloat("blueValue", Mathf.Clamp01(b));
                PlayerPrefs.Save();

                GorillaTagger.Instance.UpdateColor(r, g, b);

                InitializeMaterial(value);
            }
        }

        public enum ETurnMode
        {
            Snap,
            Smooth,
            None
        }

        public static ETurnMode TurnType
        {
            get
            {
                return PlayerPrefs.GetString("stickTurning", "SNAP") switch
                {
                    "SNAP" => ETurnMode.Snap,
                    "SMOOTH" => ETurnMode.Smooth,
                    "NONE" => ETurnMode.None,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            set
            {
                string stickTurning = value.ToString().ToUpper();

                AccessTools.Field(Computer.GetType(), "turnType").SetValue(Computer, stickTurning);

                PlayerPrefs.SetString("stickTurning", stickTurning);
                PlayerPrefs.Save();

                GorillaTagger.Instance.GetComponent<GorillaSnapTurn>().ChangeTurnMode(stickTurning, (int)AccessTools.Field(Computer.GetType(), "turnValue").GetValue(Computer));
            }
        }

        public static int TurnValue
        {
            get
            {
                return PlayerPrefs.GetInt("turnFactor", 4);
            }
            set
            {
                var turnType = AccessTools.Field(Computer.GetType(), "turnType");
                var turnValue = AccessTools.Field(Computer.GetType(), "turnValue");

                turnValue.SetValue(Computer, value);

                PlayerPrefs.SetInt("turnFactor", value);
                PlayerPrefs.Save();

                GorillaTagger.Instance.GetComponent<GorillaSnapTurn>().ChangeTurnMode((string)turnType.GetValue(Computer), value);
            }
        }

        public enum EPTTMode
        {
            AllChat,
            PushToTalk,
            PushToMute
        }

        public static EPTTMode PushToTalkType
        {
            get
            {
                return Computer.pttType switch
                {
                    "ALL CHAT" => EPTTMode.AllChat,
                    "PUSH TO TALK" => EPTTMode.PushToTalk,
                    "PUSH TO MUTE" => EPTTMode.PushToMute,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            set
            {
                Computer.pttType = value switch
                {
                    EPTTMode.AllChat => "ALL CHAT",
                    EPTTMode.PushToTalk => "PUSH TO TALK",
                    EPTTMode.PushToMute => "PUSH TO MUTE",
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        public static string Queue
        {
            get
            {
                return Computer.currentQueue;
            }
            set
            {
                Computer.currentQueue = value;
            }
        }

        public static bool IsCompetitiveAllowed => Computer.allowedInCompetitive;

        public static string[] AllowedMaps => Computer.allowedMapsToJoin;

        public static string GroupMap => AllowedMaps.Length > 1 ? Computer.groupMapJoin : AllowedMaps.First().ToUpper();

        public static void SetGroupMap(string map, int index)
        {
            Computer.groupMapJoin = map;
            Computer.groupMapJoinIndex = index;

            PlayerPrefs.SetString("groupMapJoin", Computer.groupMapJoin);
            PlayerPrefs.SetInt("groupMapJoinIndex", Computer.groupMapJoinIndex);
            PlayerPrefs.Save();
        }

        public static void JoinGroupMap()
        {
            Computer.OnGroupJoinButtonPress(Mathf.Min(AllowedMaps.Length - 1, Computer.groupMapJoinIndex), FriendCollider);
        }

        public static void RefreshRigVoices()
        {
            RigContainer.RefreshAllRigVoices();
        }

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

        public enum EAutomodMode
        {
            Off,
            Moderate,
            Aggressive
        }

        public static EAutomodMode AutoMute
        {
            get
            {
                return (EAutomodMode)PlayerPrefs.GetInt("autoMute", 1);
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
                CreditsView creditsView = Computer.creditsView;
                return (int)creditsView.GetProperty("TotalPages").GetValue(creditsView);
            }
        }

        public static int CreditCurrentPage
        {
            get
            {
                CreditsView creditsView = Computer.creditsView;

                return (int)creditsView.GetField("currentPage").GetValue(creditsView);
            }
            set
            {
                CreditsView creditsView = Computer.creditsView;

                creditsView.GetField("currentPage").SetValue(creditsView, value < 0 ? CreditPageCount + value : value % CreditPageCount);
            }
        }

        public static (string Title, List<string> Entries, bool Continued) CreditGetPage(int page)
        {
            CreditsView creditsView = Computer.creditsView;

            object pageEntries = creditsView.GetMethod("GetPageEntries").Invoke(creditsView, [page]);

            object currentSection = pageEntries.GetField("Item1").GetValue(pageEntries);

            int currentSubPage = (int)pageEntries.GetField("Item2").GetValue(pageEntries);

            string title = (string)currentSection.GetProperty("Title").GetValue(currentSection);

            IEnumerable<string> entries = (IEnumerable<string>)creditsView.GetMethod("PageOfSection").Invoke(creditsView, [currentSection, currentSubPage]);

            return (Title: title, Entries: entries.ToList(), Continued: currentSubPage > 0);
        }

        public static Dictionary<string, string> SupportData
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {
                        "Player ID",
                        PlayFabAuthenticator.instance.GetPlayFabPlayerId()
                    },
                    {
                        "Platform",
                        $"{(string)AccessTools.Field(typeof(PlayFabAuthenticator), "platform").GetValue(PlayFabAuthenticator.instance)} (Modded)"
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
