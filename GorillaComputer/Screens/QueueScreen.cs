using GorillaComputer.Behaviours;
using GorillaComputer.Models;
using GorillaComputer.Utilities;
using System.Text;

namespace GorillaComputer.Screens
{
    internal class QueueScreen : ComputerScreen
    {
        public override string Title => "Queue";
        public override string Summary => "Use [OPTION 1-3] to set avaliable queue";

        public override string GetContent()
        {
            StringBuilder str = new();

            str.AppendLine($"Queue: '{ComputerUtils.Queue}'").AppendLine();

            if (!ComputerUtils.IsCompetitiveAllowed)
            {
                str.AppendLine("<color=red>The 'COMPETITIVE' queue requires the completion the obstacle course in City!</color>").AppendLine();
            }

            return str.ToString();
        }

        public override void ProcessScreen(KeyBinding key)
        {
            switch (key)
            {
                case KeyBinding.option1:
                    ComputerUtils.JoinQueue("DEFAULT");

                    UpdateScreen();
                    break;

                case KeyBinding.option2:
                    ComputerUtils.JoinQueue("MINIGAMES");

                    UpdateScreen();
                    break;

                case KeyBinding.option3:
                    if (!ComputerUtils.IsCompetitiveAllowed) return;
                    ComputerUtils.JoinQueue("COMPETITIVE");

                    UpdateScreen();
                    break;
            }
        }
    }
}
