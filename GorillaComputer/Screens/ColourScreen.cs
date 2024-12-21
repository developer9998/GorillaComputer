using GorillaComputer.Behaviours;
using GorillaComputer.Extension;
using GorillaComputer.Models;
using GorillaComputer.Utilities;
using System.Text;
using UnityEngine;

namespace GorillaComputer.Screens
{
    internal class ColourScreen : ComputerScreen
    {
        public override string Title => "Colour";
        public override string Summary => "Use [OPTION 1/3] to select component\nUse [0-9] to set value";

        private int cursorLine;

        public void Awake()
        {
            GorillaTagger.Instance.offlineVRRig.OnColorChanged += (colour) => UpdateScreen();
        }

        public override string GetContent()
        {
            StringBuilder str = new();

            Color colour = ComputerUtils.Colour;
            str.Append("  Red: ").Append(Mathf.FloorToInt(colour.r * 9f)).Append(cursorLine == 0 ? " <" : " ").AppendLine().AppendLine();
            str.Append("Green: ").Append(Mathf.FloorToInt(colour.g * 9f)).Append(cursorLine == 1 ? " <" : " ").AppendLine().AppendLine();
            str.Append(" Blue: ").Append(Mathf.FloorToInt(colour.b * 9f)).Append(cursorLine == 2 ? " <" : " ").AppendLine().AppendLine();

            return str.ToString();
        }

        public override void ProcessScreen(KeyBinding key)
        {
            switch (key)
            {
                case KeyBinding.option1:
                    cursorLine = 0;

                    UpdateScreen();
                    break;

                case KeyBinding.option2:
                    cursorLine = 1;

                    UpdateScreen();
                    break;

                case KeyBinding.option3:
                    cursorLine = 2;

                    UpdateScreen();
                    break;

                default:
                    if (key.TryParseNumber(out int number))
                    {
                        Color playerColour = ComputerUtils.Colour;

                        switch (cursorLine)
                        {
                            case 0:
                                playerColour.r = number / 9f;
                                break;
                            case 1:
                                playerColour.g = number / 9f;
                                break;
                            case 2:
                                playerColour.b = number / 9f;
                                break;
                        }

                        ComputerUtils.Colour = playerColour;
                        UpdateScreen();
                    }
                    break;
            }
        }
    }
}
