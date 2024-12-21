using GorillaComputer.Utilities;
using HarmonyLib;

namespace GorillaComputer.Patches
{
    [HarmonyPatch(typeof(GorillaNetworking.GorillaComputer), nameof(GorillaNetworking.GorillaComputer.SetInVirtualStump))]
    public class VirtualStumpPatch
    {
        public static void Postfix(bool inVirtualStump)
        {
            ComputerUtils.SetInVStump(inVirtualStump);
        }
    }
}
