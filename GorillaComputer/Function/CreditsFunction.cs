using GorillaComputer.Model;
using GorillaComputer.Tool;
using GorillaNetworking;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorillaComputer.Function
{
    internal class CreditsFunction : ComputerFunction
    {
        public override string Name => "Credits";
        public override string Description => "Use 'W' & 'S' to switch credit pages";

        public override string GetFunctionText()
        {
            StringBuilder str = new();

            (string Title, List<string> Entries, bool Continued) = ComputerTool.CreditGetPage(ComputerTool.CreditCurrentPage);

            str.AppendLine($"Credits - '{Title}' {(Continued ? "(continued)" : "")}").AppendLine();

            foreach (var entry in Entries)
            {
                str.AppendLine(entry);
            }          

            for (int i = 0; i < ComputerTool.Computer.creditsView.pageSize - Entries.Count(); i++) // TODO: implement pageSize to ComputerUtil
            {
                str.AppendLine();
            }

            str.AppendLine().Append($"Page {ComputerTool.CreditCurrentPage + 1} / {ComputerTool.CreditPageCount}");

            return str.ToString();
        }

        public override void OnKeyPressed(GorillaKeyboardBindings key)
        {
            if (key == GorillaKeyboardBindings.W)
            {
                ComputerTool.CreditCurrentPage--;
                UpdateMonitor();
            }

            if (key == GorillaKeyboardBindings.S)
            {
                ComputerTool.CreditCurrentPage++;
                UpdateMonitor();
            }
        }
    }
}
