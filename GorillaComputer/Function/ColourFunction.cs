using GorillaComputer.Extension;
using GorillaComputer.Model;
using GorillaComputer.Tool;
using GorillaNetworking;
using System.Text;
using UnityEngine;

namespace GorillaComputer.Function
{
    internal class ColourFunction : ComputerFunction
    {
        public override string Name => "Colour";
        public override string Description => "Use 'OPTION' keys to select component - Use number keys to set value";

        private int cursorLine;

        public override string GetFunctionText()
        {
            Color playerColour = ComputerTool.Colour;

            StringBuilder str = new();

            str.AppendLine($"Red: {Mathf.FloorToInt(playerColour.r * 9f)} {(cursorLine == 0 ? "<" : "")}").AppendLine();

            str.AppendLine($"Green: {Mathf.FloorToInt(playerColour.g * 9f)} {(cursorLine == 1 ? "<" : "")}").AppendLine();

            str.AppendLine($"Blue: {Mathf.FloorToInt(playerColour.b * 9f)} {(cursorLine == 2 ? "<" : "")}").AppendLine();

            return str.ToString();
        }

        public override void OnKeyPressed(GorillaKeyboardBindings key)
        {
            switch (key)
            {
                case GorillaKeyboardBindings.option1:
                    cursorLine = 0;
                    UpdateMonitor();
                    break;

                case GorillaKeyboardBindings.option2:
                    cursorLine = 1;
                    UpdateMonitor();
                    break;

                case GorillaKeyboardBindings.option3:
                    cursorLine = 2;
                    UpdateMonitor();
                    break;

                default:
                    if (key.TryParseNumber(out int number))
                    {
                        Color playerColour = ComputerTool.Colour;

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

                        ComputerTool.Colour = playerColour;
                        UpdateMonitor();
                    }
                    break;
            }
        }
    }
}
