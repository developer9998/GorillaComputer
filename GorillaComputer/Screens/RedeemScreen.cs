using GorillaComputer.Behaviours;
using GorillaComputer.Extension;
using GorillaComputer.Models;
using GorillaComputer.Utilities;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GorillaComputer.Screens
{
    internal class RedeemScreen : ComputerScreen
    {
        public override string Title => "Redeem";

        public override string Summary => "Press [ENTER] to submit redemption code";

        private bool increaseDots;
        private int dotCount = 1;
        private float dotTimer = 0;

        public override string GetContent()
        {
            StringBuilder str = new();

            var code = ComputerUtils.Computer.RedemptionCode;

            str.Append("Code: '").Append(code).Append("'").AppendLine().AppendLine();

            var status = ComputerUtils.Computer.RedemptionStatus;

            switch (status)
            {
                case GorillaNetworking.GorillaComputer.RedemptionResult.Invalid:
                    str.Append("<color=red>Invalid code!</color>");
                    increaseDots = false;
                    break;
                case GorillaNetworking.GorillaComputer.RedemptionResult.Checking:
                    str.Append("Validating").Append(string.Concat(Enumerable.Repeat(" .", dotCount)));
                    increaseDots = true;
                    break;
                case GorillaNetworking.GorillaComputer.RedemptionResult.AlreadyUsed:
                    str.Append("<color=red>Code already claimed!</color>");
                    increaseDots = false;
                    break;
                default:
                    increaseDots = false;
                    break;
            }

            return str.ToString();
        }

        public override void ProcessScreen(KeyBinding key)
        {
            var code = ComputerUtils.Computer.RedemptionCode;
            var status = ComputerUtils.Computer.RedemptionStatus;

            switch (key)
            {
                case KeyBinding.delete:
                    if (code.Length > 0)
                    {
                        code = code[..^1];
                    }
                    ComputerUtils.Computer.RedemptionCode = code;

                    UpdateScreen();
                    break;

                case KeyBinding.enter:
                    if (string.IsNullOrEmpty(code) && status != GorillaNetworking.GorillaComputer.RedemptionResult.Success)
                    {
                        ComputerUtils.Computer.RedemptionStatus = GorillaNetworking.GorillaComputer.RedemptionResult.Empty;
                        return;
                    }

                    if (code.Length < 8)
                    {
                        ComputerUtils.Computer.RedemptionStatus = GorillaNetworking.GorillaComputer.RedemptionResult.Invalid;
                        return;
                    }

                    if (status == GorillaNetworking.GorillaComputer.RedemptionResult.Checking)
                    {
                        return;
                    }

                    CodeRedemption.Instance.HandleCodeRedemption(code);
                    ComputerUtils.Computer.RedemptionStatus = GorillaNetworking.GorillaComputer.RedemptionResult.Checking;

                    UpdateScreen();
                    break;

                default:
                    if (key.IsFunctionKey() || code.Length >= 8) return;
                    code += key.GetKeyString();
                    ComputerUtils.Computer.RedemptionCode = code;

                    UpdateScreen();
                    break;
            }
        }

        public void Update()
        {
            if (increaseDots)
            {
                dotTimer += Time.unscaledDeltaTime;
                if (dotTimer > (1f / 6f))
                {
                    dotTimer = 0;
                    dotCount = dotCount >= 3 ? 1 : dotCount + 1;
                    UpdateScreen();
                }
            }
            else
            {
                dotCount = 1;
                dotTimer = 0;
            }
        }
    }
}
