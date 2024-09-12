using GorillaComputer.Extension;
using GorillaComputer.Model;
using GorillaComputer.Tool;
using GorillaNetworking;
using System.Text;

namespace GorillaComputer.Function
{
    internal class QueueFunction : ComputerFunction
    {
        public override string Name => "Queue";
        public override string Description => "Use 'OPTION' keys to set the avaliable queue";

        public override string GetFunctionText()
        {
            StringBuilder str = new();

            str.AppendLine("Queue will filter you into public rooms with a certain demographic of players").AppendLine();

            str.AppendLine($"Current Queue: {ComputerTool.Queue.ToString().ToSentenceCase()}").AppendLine();

            if (!ComputerTool.IsCompetitiveAllowed)
            {
                str.AppendLine("<color=red>The Competitive queue requires the player to complete an obstacle course in City</color>").AppendLine();
            }

            return str.ToString();
        }

        public override void OnKeyPressed(GorillaKeyboardBindings key)
        {
            switch (key)
            {
                case GorillaKeyboardBindings.option1:
                    ComputerTool.Queue = "DEFAULT";
                    break;
                case GorillaKeyboardBindings.option2:
                    ComputerTool.Queue = "MINIGAMES";
                    break;
                case GorillaKeyboardBindings.option3:
                    if (ComputerTool.IsCompetitiveAllowed)
                    {
                        ComputerTool.Queue = "COMPETITIVE";
                        break;
                    }
                    return;
                default:
                    return;
            }
            UpdateMonitor();
        }
    }
}
