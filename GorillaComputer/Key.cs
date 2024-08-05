using GorillaNetworking;
using System;
using UnityEngine;

namespace GorillaComputer
{
    [DisallowMultipleComponent]
    internal class Key : MonoBehaviour
    {
        public static Action<Key, bool> OnKeyClicked;

        private const float Debounce = 0.1f;

        private const float KeyBump = 0.15f;

        private const float ColliderBump = 9f / 8f;

        public GorillaKeyboardBindings Binding;

        public AudioClip ClickSound;

        private MeshRenderer renderer;

        private BoxCollider collider;

        private Vector3 colliderCentre;

        private Vector3 localPosition;

        private float _clickTime;

        public void Awake()
        {
            GorillaKeyboardButton keyButton = gameObject.GetComponent<GorillaKeyboardButton>();
            Binding = keyButton.Binding;
            Destroy(keyButton);

            gameObject.layer = (int)UnityLayer.GorillaInteractable;

            renderer = GetComponent<MeshRenderer>();
            renderer.material.color = Color.white;

            collider = GetComponent<BoxCollider>();
            collider.isTrigger = true;

            colliderCentre = collider.center;

            localPosition = transform.localPosition;
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (Time.realtimeSinceStartup > (_clickTime + Debounce) && collider.TryGetComponent(out GorillaTriggerColliderHandIndicator component))
            {
                _clickTime = Time.realtimeSinceStartup;

                GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);

                AnimateKey(true);

                OnKeyClicked?.Invoke(this, component.isLeftHand);
            }
        }

        public void OnTriggerExit(Collider collider)
        {
            if (collider.GetComponent<GorillaTriggerColliderHandIndicator>())
            {
                AnimateKey(false);
            }
        }

        private void AnimateKey(bool isBumped)
        {
            if (isBumped)
            {
                renderer.material.color = new Color(0.715f, 0.7f, 0.7f);
                transform.localPosition = localPosition - (Vector3.up * KeyBump);
                collider.center = colliderCentre - (Vector3.forward * KeyBump / ColliderBump);
            }
            else
            {
                renderer.material.color = Color.white;
                transform.localPosition = localPosition;
                collider.center = colliderCentre;
            }
        }
    }
}
