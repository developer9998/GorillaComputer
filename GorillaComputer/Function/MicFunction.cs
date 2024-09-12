using GorillaComputer.Model;
using GorillaComputer.Tool;
using GorillaNetworking;
using System.Text;
using GorillaComputer.Extension;

namespace GorillaComputer.Function
{
    internal class MicFunction : ComputerFunction
    {
        public override string Name => "Mic";

        public override string Description => "Use 'OPTION' keys to select microphone usage";

        public override string GetFunctionText()
        {
            StringBuilder str = new();

            str.AppendLine("Microphone usage will determine when your voice will be transmitted based on your input").AppendLine();

            str.AppendLine($"Current Mic Setting: {ComputerTool.PushToTalkType.ToString().ToPascalResolveCase()}").AppendLine();

            if (ComputerTool.PushToTalkType > 0)
            {
                str.AppendLine("'Push To Talk' & 'Push To Mute' work with any face button");
            }

            return str.ToString();
        }

        public override void OnKeyPressed(GorillaKeyboardBindings key)
        {
            switch (key)
            {
                case GorillaKeyboardBindings.option1:
                    ComputerTool.PushToTalkType = ComputerTool.EPTTMode.AllChat;
                    UpdateMonitor();
                    break;
                case GorillaKeyboardBindings.option2:
                    ComputerTool.PushToTalkType = ComputerTool.EPTTMode.PushToTalk;
                    UpdateMonitor();
                    break;
                case GorillaKeyboardBindings.option3:
                    ComputerTool.PushToTalkType = ComputerTool.EPTTMode.PushToMute;
                    UpdateMonitor();
                    break;
            }
        }

    }
}
