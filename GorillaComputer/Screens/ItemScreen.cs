using GorillaComputer.Behaviours;
using GorillaComputer.Extension;
using GorillaComputer.Models;
using GorillaComputer.Utilities;
using System.Text;
using UnityEngine;

namespace GorillaComputer.Screens
{
    internal class ItemScreen : ComputerScreen
    {
        public override string Title => "Items";
        public override string Summary => "Press [ENTER] to toggle particles\nUse [0-9] to set instrument volume";

        public override string GetContent()
        {
            StringBuilder str = new();

            str.AppendLine($"Cosmetic Particles: {(ComputerUtils.ItemParticles ? "Visible" : "Hidden")}").AppendLine();

            str.AppendLine($"Instrument Volume: {Mathf.CeilToInt(ComputerUtils.InstrumentVolume * 50f)}");

            return str.ToString();
        }

        public override void ProcessScreen(KeyBinding key)
        {
            if (key == KeyBinding.enter)
            {
                ComputerUtils.ItemParticles ^= true;
                UpdateScreen();
                return;
            }

            if (key.TryParseNumber(out int number))
            {
                ComputerUtils.InstrumentVolume = number / 50f;
                UpdateScreen();
            }
        }
    }
}
