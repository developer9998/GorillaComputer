using HarmonyLib;

namespace GorillaComputer.Patch
{
    [HarmonyPatch(typeof(GorillaNetworking.GorillaComputer), "GeneralFailureMessage")]
    public class LazyWarningPatch
    {
        public static Watchable<string> CurrentFailureMessage = new("");

        public static void Postfix(string failMessage)
        {
            CurrentFailureMessage.value = failMessage;
        }
    }
}
