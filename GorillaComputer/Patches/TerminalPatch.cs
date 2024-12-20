using GorillaComputer.Behaviours;
using HarmonyLib;

namespace GorillaComputer.Patches
{
    [HarmonyPatch(typeof(GorillaComputerTerminal), "OnEnable")]
    public class TerminalPatch
    {
        public static void Postfix(GorillaComputerTerminal __instance)
        {
            Main.QueueTerminal(__instance);
        }
    }
}
