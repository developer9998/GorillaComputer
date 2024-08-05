using GorillaComputer.Model;
using GorillaComputer.Tool;
using GorillaNetworking;
using System.Text;

namespace GorillaComputer.Function
{
    internal class VoiceFunction : ComputerFunction
    {
        public override string Name => "Voice";
        public override string Description => "Use 'OPTION' keys to set voice type (HUMAN / MONKE)";
        public override bool IsParentalLocked => true;

        public override string GetFunctionContent()
        {
            StringBuilder str = new();

            str.AppendLine($"Voice Type '{(ComputerTool.UseVoiceChat ? "HUMAN" : "MONKE")}'");

            return str.ToString();
        }

        public override void OnKeyPressed(GorillaKeyboardBindings key)
        {
            switch (key)
            {
                case GorillaKeyboardBindings.option1:
                    ComputerTool.UseVoiceChat = true;
                    SetFunctionContent();
                    break;
                case GorillaKeyboardBindings.option2:
                    ComputerTool.UseVoiceChat = false;
                    SetFunctionContent();
                    break;
            }
        }
    }
}
