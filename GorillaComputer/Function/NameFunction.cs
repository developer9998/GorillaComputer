using GorillaComputer.Extension;
using GorillaComputer.Tool;
using GorillaNetworking;
using System.Text;

namespace GorillaComputer.Function
{
    internal class NameFunction : RoomFunction
    {
        public override string Name => "Name";
        public override string Description => "Press 'ENTER' to set a new name";
        public override bool IsParentalLocked => true;

        private string enteredNickName = "";

        public NameFunction()
        {
            enteredNickName = ComputerTool.Name;
        }

        public override string GetFunctionContent()
        {
            StringBuilder str = new();

            str.AppendLine($"Current Name: '{ComputerTool.Name}'").AppendLine();
            str.AppendLine($"New Name: {enteredNickName}");

            return str.ToString();
        }

        public override void OnKeyPressed(GorillaKeyboardBindings key)
        {
            switch (key)
            {
                case GorillaKeyboardBindings.delete:
                    if (enteredNickName.Length > 0)
                    {
                        enteredNickName = enteredNickName[..^1];
                    }

                    SetFunctionContent();

                    break;

                case GorillaKeyboardBindings.enter:
                    ComputerTool.Name = enteredNickName;

                    SetFunctionContent();

                    break;

                case GorillaKeyboardBindings.option1:
                case GorillaKeyboardBindings.option2:
                case GorillaKeyboardBindings.option3:
                    break;

                default:
                    if (enteredNickName.Length >= 12) return;

                    enteredNickName += key.GetKeyString();

                    SetFunctionContent();

                    break;
            }
        }
    }
}
