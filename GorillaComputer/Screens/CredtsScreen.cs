using GorillaComputer.Behaviours;
using GorillaComputer.Models;
using GorillaComputer.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorillaComputer.Screens
{
    internal class CredtsScreen : ComputerScreen
    {
        public override string Title => "Credits";
        public override string Summary => "Use [W/S] to navigate pages";

        public override string GetContent()
        {
            StringBuilder str = new();

            (string title, List<string> entries, bool continued) = ComputerUtils.CreditGetPage(ComputerUtils.CreditCurrentPage);

            str.AppendLine($"Gorilla Tag Credits - '{title}' {(continued ? "(continued)" : "")}").AppendLine();

            foreach (var entry in entries)
            {
                str.AppendLine(entry);
            }

            for (int i = 0; i < ComputerUtils.Computer.creditsView.pageSize - entries.Count(); i++)
            {
                str.AppendLine();
            }

            str.AppendLine().Append($"Page {ComputerUtils.CreditCurrentPage + 1}/{ComputerUtils.CreditPageCount}");

            return str.ToString();
        }

        public override void ProcessScreen(KeyBinding key)
        {
            if (key == KeyBinding.W)
            {
                ComputerUtils.CreditCurrentPage--;

                UpdateScreen();
            }

            if (key == KeyBinding.S)
            {
                ComputerUtils.CreditCurrentPage++;

                UpdateScreen();
            }
        }
    }
}
