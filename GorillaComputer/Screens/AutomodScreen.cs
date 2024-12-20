using GorillaComputer.Behaviours;
using GorillaComputer.Models;
using GorillaComputer.Utilities;
using System.Text;

namespace GorillaComputer.Screens
{
    internal class AutomodScreen : ComputerScreen
    {
        public override string Title => "Automod";
        public override string Summary => "Use [OPTION 1/3] to select automod level";
        public override bool IsParentalLocked => true;

        public override string GetContent()
        {
            StringBuilder str = new();

            str.AppendLine("Automod automatically mutes players when they join your room if other players have them muted.").AppendLine();

            str.Append("Automod Level: ").Append(ComputerUtils.AutoMute);

            return str.ToString();
        }

        public override void ProcessScreen(KeyBinding key)
        {
            switch (key)
            {
                case KeyBinding.option1:
                    ComputerUtils.AutoMute = ComputerUtils.AutomodMode.Off;

                    UpdateScreen();
                    break;

                case KeyBinding.option2:
                    ComputerUtils.AutoMute = ComputerUtils.AutomodMode.Moderate;

                    UpdateScreen();
                    break;

                case KeyBinding.option3:
                    ComputerUtils.AutoMute = ComputerUtils.AutomodMode.Aggressive;

                    UpdateScreen();
                    break;
            }
        }
    }
}
