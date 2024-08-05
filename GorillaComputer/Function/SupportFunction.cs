using GorillaComputer.Model;
using GorillaComputer.Tool;
using GorillaNetworking;
using System;
using System.Collections.Generic;
using System.Text;

namespace GorillaComputer.Function
{
    internal class SupportFunction : ComputerFunction
    {
        public override string Name => "Support";
        public override string Description => "Press 'ENTER' to show account information";

        private bool displaySupport;

        public override void OnFunctionOpened()
        {
            displaySupport = false;
        }

        public override string GetFunctionContent()
        {
            StringBuilder str = new();

            if (displaySupport)
            {
                ComputerTool.SupportData.ForEach(a => str.AppendLine($"{a.Key}: {a.Value}").AppendLine());
            }
            else
            {
                str.Append("<color=red>Do not share account information with anyone other than Another Axiom support!</color>");
            }

            return str.ToString();
        }

        public override void OnKeyPressed(GorillaKeyboardBindings key)
        {
            if (key == GorillaKeyboardBindings.enter)
            {
                displaySupport = true;
                SetFunctionContent();
            }
        }
    }
}
