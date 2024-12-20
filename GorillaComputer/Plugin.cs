using BepInEx;
using BepInEx.Logging;
using GorillaComputer.Behaviours;
using HarmonyLib;
using UnityEngine;

namespace GorillaComputer
{
    [BepInPlugin(Constants.GUID, Constants.Name, Constants.Version)]
    internal class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource TiedLogger;

        public void Awake()
        {
            TiedLogger = Logger;

            GorillaTagger.OnPlayerSpawned(() => new GameObject(typeof(Main).FullName).AddComponent<Main>());
            Harmony.CreateAndPatchAll(GetType().Assembly, Constants.GUID);
        }
    }
}
