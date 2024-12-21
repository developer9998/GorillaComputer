using GorillaComputer.Behaviours;
using GorillaComputer.Extension;
using GorillaComputer.Models;
using GorillaComputer.Utilities;
using GorillaNetworking;
using System.Text;

namespace GorillaComputer.Screens
{
    internal class RoomScreen : ComputerScreen
    {
        public override string Title => "Room";
        public override string Summary => NetworkSystem.Instance.InRoom ? "Press [ENTER] to join room\nPress [OPTION 1] to leave current room" : "Press [ENTER] to join room";

        public void Awake()
        {
            NetworkSystem.Instance.OnMultiplayerStarted += UpdateScreen;
            NetworkSystem.Instance.OnReturnedToSinglePlayer += UpdateScreen;

            NetworkSystem.Instance.OnPlayerJoined += (player) => UpdateScreen();
            NetworkSystem.Instance.OnPlayerLeft += (player) => UpdateScreen();

            ComputerUtils.SetInVirtualStump += (inVStump) => UpdateScreen();
        }

        public override string GetContent()
        {
            StringBuilder str = new();

            if (ComputerUtils.InParty)
            {
                str.AppendLine(ComputerUtils.IsPartyWithinCollider ? "Your group will travel with you." : "<color=red>You will leave your party unless you gather them here first!</color>").AppendLine();
            }

            str.AppendLine(NetworkSystem.Instance.InRoom ? $"Players In Room: {NetworkSystem.Instance.RoomPlayerCount} / {PhotonNetworkController.Instance.GetRoomSize(NetworkSystem.Instance.GameModeString)}" : $"Players Online: {NetworkSystem.Instance.GlobalPlayerCount():n0}").AppendLine();

            bool isSafeAccount = PlayFabAuthenticator.instance.GetSafety();

            if (!isSafeAccount)
            {
                var roomToJoin = ComputerUtils.Computer.roomToJoin;

                str.AppendLine($"Room Code: {roomToJoin}").AppendLine();

                if (ComputerUtils.Computer.roomFull)
                {
                    str.AppendLine("<color=red>Error: Room code is full!</color>");
                }

                if (ComputerUtils.Computer.roomNotAllowed)
                {
                    str.AppendLine($"<color=red>Error: Room cannot be entered in this map!</color>");
                }
            }

            if (NetworkSystem.Instance.InRoom)
            {
                str.AppendLine($"Current Room: {NetworkSystem.Instance.RoomName}").AppendLine();
            }

            return str.ToString();
        }

        public override void ProcessScreen(KeyBinding key)
        {
            var roomToJoin = ComputerUtils.Computer.roomToJoin;

            switch (key)
            {
                case KeyBinding.delete:
                    if ((ComputerUtils.PlayerInVirtualStump && roomToJoin.Length > 1) || (!ComputerUtils.PlayerInVirtualStump && roomToJoin.Length > 0))
                    {
                        roomToJoin = roomToJoin[..^1];
                    }
                    ComputerUtils.Computer.roomToJoin = roomToJoin;

                    UpdateScreen();
                    break;

                case KeyBinding.enter:
                    if ((ComputerUtils.PlayerInVirtualStump && roomToJoin.Length <= 1) || (!ComputerUtils.PlayerInVirtualStump && roomToJoin.Length == 0)) return;
                    ComputerUtils.JoinRoom(roomToJoin);
                    break;

                case KeyBinding.option1:
                    if (!NetworkSystem.Instance.InRoom) return;
                    ComputerUtils.LeaveRoom();
                    break;

                default:
                    if (key.IsFunctionKey() || roomToJoin.Length >= 10) return;
                    roomToJoin += key.GetKeyString();
                    ComputerUtils.Computer.roomToJoin = roomToJoin;

                    UpdateScreen();
                    break;
            }
        }
    }
}
