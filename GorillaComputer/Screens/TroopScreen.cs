using GorillaComputer.Behaviours;
using GorillaComputer.Extension;
using GorillaComputer.Models;
using GorillaComputer.Utilities;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GorillaComputer.Screens
{
    internal class TroopScreen : ComputerScreen
    {
        public override string Title => "Troop";
        public override string Summary => ComputerUtils.InTroop ? "Press [OPTION 1] to toggle troop queue\nPress [OPTION 2] to leave troop" : "Press [ENTER] to join troop";

        private float populationCheckTimer = 0;

        private bool increaseDots;
        private int dotCount = 1;
        private float dotTimer = 0;

        public override string GetContent()
        {
            var str = new StringBuilder();

            string troopName = ComputerUtils.TroopName;

            if (!ComputerUtils.InTroop)
            {
                var troopToJoin = ComputerUtils.Computer.troopToJoin;

                str.Append("New Troop: '").Append(troopToJoin).Append("'").AppendLine().AppendLine();

                str.Append("Top Troops: ");

                var topTroops = ComputerUtils.TopTroops;

                for (int i = 0; i < topTroops.Count; i++)
                {
                    string displayIndex = $"#{i + 1}";
                    string topTroop = topTroops[i];
                    str.AppendLine().Append(displayIndex).Append(": '").Append(topTroop).Append("'");
                }

                return str.ToString();
            }

            str.Append("Current Troop: '").Append(troopName).Append("'").AppendLine().AppendLine();

            int population = ComputerUtils.TroopPopulation;

            if (ComputerUtils.TroopQueueActive)
            {
                str.AppendLine("Troop queue active");

                increaseDots = population == -1;
                if (population == -1)
                {
                    str.AppendLine().Append("Population: Loading").Append(string.Concat(Enumerable.Repeat(" .", dotCount))).AppendLine().AppendLine();
                }
                else
                {
                    str.AppendLine().Append("Population: ").Append(Mathf.Max(1, population)).AppendLine().AppendLine();
                }
            }
            else
            {
                str.Append("<color=grey>Troop queue inactive</color>");
            }

            return str.ToString();
        }

        public override void ProcessScreen(KeyBinding key)
        {
            string troopToJoin = ComputerUtils.Computer.troopToJoin;

            switch (key)
            {
                case KeyBinding.delete:
                    if (troopToJoin.Length > 0)
                    {
                        troopToJoin = troopToJoin[..^1];
                    }
                    ComputerUtils.Computer.troopToJoin = troopToJoin;

                    UpdateScreen();
                    break;

                case KeyBinding.enter:
                    if (ComputerUtils.InTroop) return;
                    ComputerUtils.JoinTroop(troopToJoin);

                    UpdateScreen();
                    break;

                case KeyBinding.option1:
                    if (!ComputerUtils.InTroop) return;

                    if (ComputerUtils.TroopQueueActive) ComputerUtils.JoinQueue("DEFAULT");
                    else ComputerUtils.JoinTroopQueue();

                    UpdateScreen();
                    break;

                case KeyBinding.option2:
                    if (!ComputerUtils.InTroop) return;
                    ComputerUtils.LeaveTroop();

                    UpdateScreen();
                    break;

                default:
                    if (key.IsFunctionKey() || troopToJoin.Length >= 12 || ComputerUtils.InTroop) return;

                    troopToJoin += key.GetKeyString();
                    ComputerUtils.Computer.troopToJoin = troopToJoin;

                    UpdateScreen();
                    break;
            }
        }

        // When our troop queue is active, every couple seconds, a request is sent to evaluate the population of our troop
        public void Update()
        {
            if (increaseDots)
            {
                dotTimer += Time.unscaledDeltaTime;
                if (dotTimer > (1f / 6f))
                {
                    dotTimer = 0;
                    dotCount = dotCount >= 3 ? 1 : dotCount + 1;
                    UpdateScreen();
                }
            }
            else
            {
                dotCount = 1;
                dotTimer = 0;
            }

            if (ComputerUtils.TroopQueueActive)
            {
                populationCheckTimer += Time.unscaledDeltaTime;

                if (populationCheckTimer < 8f) return;
                populationCheckTimer = 0;

                ComputerUtils.RequestTroopPopulation();
            }
        }
    }
}
