using GorillaComputer.Behaviours;
using GorillaComputer.Extension;
using GorillaComputer.Models;
using GorillaComputer.Utilities;
using System.Text;

namespace GorillaComputer.Screens
{
    internal class VoiceScreen : ComputerScreen
    {
        public override string Title => "Voice";
        public override string Summary => "Use [OPTION 1-3] to select mic setting\nPress [ENTER] to toggle voice type";
        public override bool IsParentalLocked => true;

        public override string GetContent()
        {
            StringBuilder str = new();

            str.AppendLine($"Voice Type: '{(ComputerUtils.UseVoiceChat ? "HUMAN" : "MONKE")}'").AppendLine();
            str.AppendLine($"Microphone Setting: '{ComputerUtils.PushToTalkType.ToString().ToPascalResolveCase().ToUpper()}'").AppendLine();

            if (ComputerUtils.PushToTalkType > 0)
            {
                str.AppendLine("'PUSH TO TALK' & 'PUSH TO MUTE' work with any face button");
            }

            return str.ToString();
        }

        public override void ProcessScreen(KeyBinding key)
        {
            switch (key)
            {
                case KeyBinding.enter:
                    ComputerUtils.UseVoiceChat ^= true;
                    UpdateScreen();
                    break;

                case KeyBinding.option1:
                    ComputerUtils.PushToTalkType = ComputerUtils.PTTMode.AllChat;
                    UpdateScreen();
                    break;

                case KeyBinding.option2:
                    ComputerUtils.PushToTalkType = ComputerUtils.PTTMode.PushToTalk;
                    UpdateScreen();
                    break;

                case KeyBinding.option3:
                    ComputerUtils.PushToTalkType = ComputerUtils.PTTMode.PushToMute;
                    UpdateScreen();
                    break;
            }
        }
    }
}
