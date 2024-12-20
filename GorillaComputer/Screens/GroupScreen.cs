using GorillaComputer.Behaviours;
using GorillaComputer.Extension;
using GorillaComputer.Models;
using GorillaComputer.Utilities;
using GorillaNetworking;
using GorillaTagScripts;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GorillaComputer.Screens
{
    internal class GroupScreen : ComputerScreen
    {
        public override string Title => "Group";
        public override string Summary => "Use [0-9] to select destination\nPress [ENTER] to join public room";
        public override bool IsParentalLocked => true;

        public override string GetContent()
        {
            StringBuilder str = new();

            if (FriendshipGroupDetection.Instance.IsInParty)
            {
                ComputerUtils.Computer.primaryTriggersByZone.TryGetValue(ComputerUtils.AllowedMaps[Mathf.Min(ComputerUtils.AllowedMaps.Length - 1, ComputerUtils.Computer.groupMapJoinIndex)], out GorillaNetworkJoinTrigger component);

                bool canPartyJoin = (!component || component.CanPartyJoin()) && ComputerUtils.IsPartyWithinCollider;

                if (!canPartyJoin)
                {
                    str.AppendLine("You will be joined into an existing public room and bring your party with you.");
                }
                else
                {
                    str.AppendLine("All members of your party are required to gather here to group!");
                }
            }
            else if (NetworkSystem.Instance.InRoom && NetworkSystem.Instance.RoomPlayerCount > 1)
            {
                str.AppendLine("You will be joined into an existing public room and bring everyone in this room with you.");
            }
            else
            {
                str.AppendLine("You will be joined into an existing public room by yourself.");
            }

            str.AppendLine();

            AppendZoneInfo(str);

            return str.ToString();
        }

        public void AppendZoneInfo(StringBuilder str)
        {
            str.AppendLine($"Selected Zone: '{ComputerUtils.GroupMap}'").AppendLine();

            if (ComputerUtils.AllowedMaps.Length > 1)
            {
                str.AppendLine($"Destinations: {ComputerUtils.AllowedMaps.Select(z => $"'{z.ToUpper()}'").Join(", ")}").AppendLine();
            }
        }

        public override void ProcessScreen(KeyBinding key)
        {
            switch (key)
            {
                case KeyBinding.enter:
                    ComputerUtils.JoinGroupMap();
                    break;

                default:
                    if (!key.TryParseNumber(out int number)) return;
                    number--; // 1 = forest, 2 = cave, 3 = canyon, 4 = city, 5 = clouds

                    string map = ComputerUtils.AllowedMaps.ElementAtOrDefault(number) ?? null;
                    if (string.IsNullOrEmpty(map)) return;

                    ComputerUtils.SetGroupMap(map.ToUpper(), number);

                    UpdateScreen();
                    break;
            }
        }
    }
}
