using GorillaComputer.Extension;
using GorillaComputer.Model;
using GorillaComputer.Tool;
using GorillaNetworking;
using GorillaTagScripts;
using Photon.Pun;
using System.Text;
using System;
using GorillaGameModes;
using System.Linq;

namespace GorillaComputer.Function
{
    internal class RoomFunction : ComputerFunction
    {
        public override string Name => "Room";
        public override string Description => "Press 'ENTER' to join a room code - Press 'OPTION 1' to leave the current room";

        private string enteredRoomCode = "";

        public override string GetFunctionText()
        {
            StringBuilder str = new();

            if (FriendshipGroupDetection.Instance.IsInParty)
            {
                str.AppendLine(ComputerTool.IsPartyWithinCollider ? "Your group will travel with you." : "<color=red>You will leave your party unless you gather them here first!</color>").AppendLine();
            }

            str.AppendLine(NetworkSystem.Instance.InRoom ? $"Players In Room: {NetworkSystem.Instance.RoomPlayerCount} / {PhotonNetworkController.Instance.GetRoomSize(NetworkSystem.Instance.GameModeString)}" : $"Players Online: {NetworkSystem.Instance.GlobalPlayerCount():n0}").AppendLine();

            bool isSafeAccount = PlayFabAuthenticator.instance.GetSafety();

            if (!isSafeAccount)
            {
                str.AppendLine($"Room Code: {enteredRoomCode}").AppendLine();

                if (GorillaNetworking.GorillaComputer.instance.roomFull)
                {
                    str.AppendLine("<color=red>Error: Room code is full!</color>");
                }

                if (GorillaNetworking.GorillaComputer.instance.roomNotAllowed)
                {
                    str.AppendLine($"<color=red>Error: Room cannot be entered in this map!</color>");
                }
            }

            if (NetworkSystem.Instance.InRoom)
            {
                str.AppendLine($"Current Room: {NetworkSystem.Instance.RoomName}").AppendLine();

                str.AppendLine($"Region: {PhotonNetwork.CloudRegion.Replace("/*", "").ToLower() switch
                {
                    // main regions
                    "us" => "USA (East)",
                    "usw" => "USA (West)",
                    "eu" => "Europe",
                    // other regions
                    "asia" => "Asia",
                    "au" => "Australia",
                    "cae" => "Canada, East",
                    "hk" => "Hong Kong",
                    "in" => "India",
                    "jp" => "Japan",
                    "za" => "South Africa",
                    "sa" => "South America",
                    "kr" => "South Korea",
                    "tr" => "Turkey",
                    "uae" => "United Arab Emirates",
                    "ussc" => "USA, South Central",
                    null => "Unknown",
                    _ => throw new ArgumentOutOfRangeException("CloudRegion")
                }}").AppendLine();

                string gameModeString = NetworkSystem.Instance.GameModeString;

                string gameMode = GameMode.gameModeNames.FirstOrDefault(gameModeString.Contains);

                str.AppendLine($"Game Mode: {gameMode?.ToLower()?.ToSentenceCase() ?? "None"}");
            }

            return str.ToString();
        }

        public override void OnKeyPressed(GorillaKeyboardBindings key)
        {
            switch (key)
            {
                case GorillaKeyboardBindings.delete:
                    if (enteredRoomCode.Length > 0)
                    {
                        enteredRoomCode = enteredRoomCode[..^1];
                    }

                    UpdateMonitor();

                    break;

                case GorillaKeyboardBindings.enter:
                    ComputerTool.JoinRoom(enteredRoomCode);
                    break;

                case GorillaKeyboardBindings.option1:
                    ComputerTool.LeaveRoom();
                    break;

                case GorillaKeyboardBindings.option2:

                case GorillaKeyboardBindings.option3:
                    break;

                default:
                    if (enteredRoomCode.Length >= 10) return;

                    enteredRoomCode += key.GetKeyString();

                    UpdateMonitor();

                    break;
            }
        }
    }
}
