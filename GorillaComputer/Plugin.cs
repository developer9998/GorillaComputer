using BepInEx;
using GorillaComputer.Tool;
using HarmonyLib;
using UnityEngine;

namespace GorillaComputer
{
    [BepInPlugin(Constants.GUID, Constants.Name, Constants.Version)]
    internal class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            LogTool.Logger = Logger;
            GorillaTagger.OnPlayerSpawned(Initialize);
            Harmony.CreateAndPatchAll(GetType().Assembly, Constants.GUID);
        }

        public void Initialize()
        {
            new GameObject($"{Constants.Name} ({Constants.GUID})").AddComponent<Main>();
        }
    }
}
