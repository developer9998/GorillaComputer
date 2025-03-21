using GorillaComputer.Behaviours;
using GorillaLocomotion;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GorillaComputer.Utilities
{
    internal static class YieldUtils
    {
        public static async Task YieldWebRequestAsync(UnityWebRequest webRequest)
        {
            var completionSource = new TaskCompletionSource<UnityWebRequest>();
            ((MonoBehaviour)(Main.HasInstance ? Main.Instance : GTPlayer.Instance)).StartCoroutine(AwaitWebRequestCoroutine(webRequest, completionSource));
            await completionSource.Task;
        }

        public static async Task YieldInstructionAsync(YieldInstruction instruction)
        {
            var completionSource = new TaskCompletionSource<YieldInstruction>();
            ((MonoBehaviour)(Main.HasInstance ? Main.Instance : GTPlayer.Instance)).StartCoroutine(AwaitInstructionCorouutine(instruction, completionSource));
            await completionSource.Task;
        }

        private static IEnumerator AwaitWebRequestCoroutine(UnityWebRequest webRequest, TaskCompletionSource<UnityWebRequest> completionSource)
        {
            yield return webRequest.SendWebRequest();
            completionSource.SetResult(webRequest);
        }

        private static IEnumerator AwaitInstructionCorouutine(YieldInstruction instruction, TaskCompletionSource<YieldInstruction> completionSource)
        {
            yield return instruction;
            completionSource.SetResult(instruction);
        }
    }
}
