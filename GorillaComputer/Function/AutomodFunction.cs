using GorillaComputer.Model;
using GorillaComputer.Tool;
using GorillaNetworking;
using System.Text;

namespace GorillaComputer.Function
{
    internal class AutomodFunction : ComputerFunction
    {
        public override string Name => "Automod";
        public override string Description => "Use 'OPTION' keys to select the automod level";
        public override bool IsParentalLocked => true;

        public override string GetFunctionText()
        {
            StringBuilder str = new();

            str.AppendLine("Automod automatically mutes players when they join your room if other players have them muted.").AppendLine();

            str.AppendLine($"Current Automod Level: {ComputerTool.AutoMute}");

            return str.ToString();
        }

        public override void OnKeyPressed(GorillaKeyboardBindings key)
        {
            switch (key)
            {
                case GorillaKeyboardBindings.option1:
                    ComputerTool.AutoMute = ComputerTool.EAutomodMode.Aggressive;
                    UpdateMonitor();
                    break;
                case GorillaKeyboardBindings.option2:
                    ComputerTool.AutoMute = ComputerTool.EAutomodMode.Moderate;
                    UpdateMonitor();
                    break;
                case GorillaKeyboardBindings.option3:
                    ComputerTool.AutoMute = ComputerTool.EAutomodMode.Off;
                    UpdateMonitor();
                    break;
            }
        }
    }
}
