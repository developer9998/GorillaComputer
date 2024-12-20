using GorillaComputer.Behaviours;
using GorillaComputer.Extension;
using GorillaComputer.Models;
using GorillaComputer.Utilities;
using System.Text;

namespace GorillaComputer.Screens
{
    internal class TurnScreen : ComputerScreen
    {
        public override string Title => "Turn";

        public override string Summary => "Use [OPTION 1/3] keys to select turn mode\nUse [0-9] to set turn speed";

        public override string GetContent()
        {
            StringBuilder str = new();

            str.AppendLine($"Turn Type: {ComputerUtils.TurnType}").AppendLine();

            str.AppendLine($"Turn Value: {ComputerUtils.TurnValue}").AppendLine();

            return str.ToString();
        }

        public override void ProcessScreen(KeyBinding key)
        {
            switch (key)
            {
                case KeyBinding.option1:
                    ComputerUtils.TurnType = ComputerUtils.TurnMode.Snap;
                    UpdateScreen();
                    break;

                case KeyBinding.option2:
                    ComputerUtils.TurnType = ComputerUtils.TurnMode.Smooth;
                    UpdateScreen();
                    break;

                case KeyBinding.option3:
                    ComputerUtils.TurnType = ComputerUtils.TurnMode.None;
                    UpdateScreen();
                    break;

                default:
                    if (key.TryParseNumber(out int number))
                    {
                        ComputerUtils.TurnValue = number;
                        UpdateScreen();
                    }
                    break;
            }
        }
    }
}
