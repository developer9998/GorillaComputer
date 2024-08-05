using GorillaComputer.Extension;
using GorillaComputer.Model;
using GorillaComputer.Tool;
using GorillaNetworking;
using GorillaTagScripts;
using System.Text;

namespace GorillaComputer.Function
{
    internal class RoomFunction : ComputerFunction
    {
        public override string Name => "Room";
        public override string Description => "Press 'ENTER' to join a room code - Press 'OPTION 1' to leave the current room";

        private string enteredRoomCode = "";

        public override string GetFunctionContent()
        {
            StringBuilder str = new();

            if (FriendshipGroupDetection.Instance.IsInParty)
            {
                str.AppendLine(ComputerTool.IsPartyWithinCollider ? "Your group will travel with you." : "<color=red>You will leave your party unless you gather them here first!</color>").AppendLine();
            }

            str.AppendLine(NetworkSystem.Instance.InRoom ? $"Players In Room: {NetworkSystem.Instance.RoomPlayerCount} / {PhotonNetworkController.Instance.GetRoomSize(NetworkSystem.Instance.GameModeString)}" : $"Players Online: {NetworkSystem.Instance.GlobalPlayerCount()}").AppendLine();

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
                str.AppendLine();

                str.AppendLine($"Current Room: {NetworkSystem.Instance.RoomName}").AppendLine();
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

                    SetFunctionContent();

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

                    SetFunctionContent();

                    break;
            }
        }
    }
}
