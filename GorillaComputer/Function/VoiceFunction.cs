using GorillaComputer.Model;
using GorillaComputer.Tool;
using GorillaNetworking;
using System.Text;

namespace GorillaComputer.Function
{
    internal class VoiceFunction : ComputerFunction
    {
        public override string Name => "Voice";
        public override string Description => "Press 'OPTION' key to toggle the voice type";
        public override bool IsParentalLocked => true;

        public override string GetFunctionText()
        {
            StringBuilder str = new();

            str.AppendLine("Voice will change how you hear others and how they hear you").AppendLine();

            str.AppendLine($"Current Voice Type: {(ComputerTool.UseVoiceChat ? "Human" : "Monke")}");

            return str.ToString();
        }

        public override void OnKeyPressed(GorillaKeyboardBindings key)
        {
            switch (key)
            {
                case GorillaKeyboardBindings.enter:
                    ComputerTool.UseVoiceChat ^= true;
                    UpdateMonitor();
                    break;
            }
        }
    }
}
