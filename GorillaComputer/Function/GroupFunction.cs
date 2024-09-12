using GorillaComputer.Extension;
using GorillaComputer.Model;
using GorillaComputer.Tool;
using GorillaNetworking;
using GorillaTagScripts;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GorillaComputer.Function
{
    internal class GroupFunction : ComputerFunction
    {
        public override string Name => "Group";
        public override string Description => "Use number keys to set map - Press 'ENTER' key to join map";
        public override bool IsParentalLocked => true;

        public override string GetFunctionText()
        {
            StringBuilder str = new();

            if (FriendshipGroupDetection.Instance.IsInParty)
            {
                ComputerTool.Computer.primaryTriggersByZone.TryGetValue(ComputerTool.AllowedMaps[Mathf.Min(ComputerTool.AllowedMaps.Length - 1, ComputerTool.Computer.groupMapJoinIndex)], out GorillaNetworkJoinTrigger component);
                
                bool canPartyJoin = (!component || component.CanPartyJoin()) && ComputerTool.IsPartyWithinCollider;

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
            str.AppendLine($"Active Zone: '{ComputerTool.GroupMap}'").AppendLine();

            if (ComputerTool.AllowedMaps.Length > 1)
            {
                str.AppendLine($"Destinations: {ComputerTool.AllowedMaps.Select(z => $"'{z.ToUpper()}'").Join(", ")}").AppendLine();
            }
        }

        public override void OnKeyPressed(GorillaKeyboardBindings key)
        {
            if (key == GorillaKeyboardBindings.enter)
            {
                ComputerTool.JoinGroupMap();
                return;
            }

            if (key.IsNumberKey())
            {
                int number = (int)key - 1;

                string map = ComputerTool.AllowedMaps.ElementAtOrDefault(number) ?? null;

                if (string.IsNullOrEmpty(map))
                {
                    return;
                }

                ComputerTool.SetGroupMap(map.ToUpper(), number);
                UpdateMonitor();
            }
        }
    }
}
