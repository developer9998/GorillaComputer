using HarmonyLib;

namespace GorillaComputer.Patches
{
    [HarmonyPatch(typeof(GorillaNetworking.GorillaComputer), "GeneralFailureMessage")]
    public class FailureMessagePatch
    {
        public static Watchable<string> CurrentFailureMessage = new("");

        public static void Postfix(string failMessage)
        {
            CurrentFailureMessage.value = failMessage;
        }
    }
}
