using GorillaComputer.Behaviours;
using GorillaComputer.Models;
using GorillaComputer.Utilities;
using System.Text;

namespace GorillaComputer.Screens
{
    internal class SupportScreen : ComputerScreen
    {
        public override string Title => "Support";
        public override string Summary => "Press [ENTER] to toggle account information";

        private bool displayAccountInfo;

        public override void OnScreenShow()
        {
            displayAccountInfo = false;
        }

        public override string GetContent()
        {
            StringBuilder str = new();

            if (displayAccountInfo)
            {
                ComputerUtils.SupportData.ForEach(a => str.AppendLine($"{a.Key}: {a.Value}").AppendLine());
            }
            else
            {
                str.Append("<color=red>Do not share account information with anyone other than Another Axiom support!</color>");
            }

            return str.ToString();
        }

        public override void ProcessScreen(KeyBinding key)
        {
            if (key == KeyBinding.enter)
            {
                displayAccountInfo ^= true;
                UpdateScreen();
            }
        }
    }
}
