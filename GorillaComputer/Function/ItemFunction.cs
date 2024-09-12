using GorillaComputer.Extension;
using GorillaComputer.Model;
using GorillaComputer.Tool;
using GorillaNetworking;
using System.Text;
using UnityEngine;

namespace GorillaComputer.Function
{
    internal class ItemFunction : ComputerFunction
    {
        public override string Name => "Items";
        public override string Description => "Use 'ENTER' key to toggle particles - Use number keys to set instrument volume";

        public override string GetFunctionText()
        {
            StringBuilder str = new();

            str.AppendLine($"Cosmetic Particles: {(ComputerTool.ItemParticles ? "Visible" : "Hidden")}").AppendLine();

            str.AppendLine($"Instrument Volume: {Mathf.CeilToInt(ComputerTool.InstrumentVolume * 50f)}");

            return str.ToString();
        }

        public override void OnKeyPressed(GorillaKeyboardBindings key)
        {
            if (key == GorillaKeyboardBindings.enter)
            {
                ComputerTool.ItemParticles ^= true;
                UpdateMonitor();
                return;
            }

            if (key.TryParseNumber(out int number))
            {
                ComputerTool.InstrumentVolume = number / 50f;
                UpdateMonitor();
            }
        }
    }
}
