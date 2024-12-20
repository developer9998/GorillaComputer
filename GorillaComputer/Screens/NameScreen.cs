using GorillaComputer.Extension;
using GorillaComputer.Models;
using GorillaComputer.Utilities;
using System.Text;

namespace GorillaComputer.Screens
{
    internal class NameScreen : RoomScreen
    {
        public override string Title => "Name";
        public override string Summary => "Press [ENTER] to set new new\nPress [OPTION 1] to toggle nametags";
        public override bool IsParentalLocked => true;

        public override string GetContent()
        {
            StringBuilder str = new();

            str.AppendLine($"Current Name: '{ComputerUtils.Name}'").AppendLine();

            var actualCurrentName = ComputerUtils.Computer.currentName;
            str.AppendLine($"New Name: '{actualCurrentName}'").AppendLine();

            bool nametags = ComputerUtils.Nametags;
            str.Append("Nametags: ").Append(nametags ? "On" : "Off");

            return str.ToString();
        }

        public override void ProcessScreen(KeyBinding key)
        {
            var currentName = ComputerUtils.Computer.currentName;

            switch (key)
            {
                case KeyBinding.delete:
                    if (currentName.Length > 0)
                    {
                        currentName = currentName[..^1];
                    }
                    ComputerUtils.Computer.currentName = currentName;

                    UpdateScreen();
                    break;

                case KeyBinding.enter:
                    ComputerUtils.Name = ComputerUtils.Computer.currentName;

                    UpdateScreen();
                    break;

                case KeyBinding.option1:
                    ComputerUtils.Nametags ^= true;

                    UpdateScreen();
                    break;

                default:
                    if (key.IsFunctionKey() || currentName.Length >= 12) return;
                    currentName += key.GetKeyString();
                    ComputerUtils.Computer.currentName = currentName;

                    UpdateScreen();
                    break;
            }
        }
    }
}
