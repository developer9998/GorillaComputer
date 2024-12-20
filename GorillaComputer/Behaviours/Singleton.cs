using GorillaComputer.Tools;
using UnityEngine;

namespace GorillaComputer.Behaviours
{
    internal class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        public static bool HasInstance => Instance;

        private T SelfInstance => gameObject.GetComponent<T>();

        public void Awake()
        {
            if (HasInstance && Instance != SelfInstance)
            {
                Debug.Log("DEAD");
                Destroy(SelfInstance);
            }

            Instance = SelfInstance;
            Initialize();
        }

        protected virtual void Initialize()
        {
            Logging.Info($"Initializing {typeof(T).Name} singleton");
        }
    }
}
