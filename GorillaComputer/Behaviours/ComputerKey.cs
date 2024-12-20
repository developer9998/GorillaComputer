using GorillaComputer.Models;
using GorillaNetworking;
using System;
using UnityEngine;

namespace GorillaComputer.Behaviours
{
    [DisallowMultipleComponent]
    internal class ComputerKey : MonoBehaviour
    {
        public Computer Computer;

        public static Action<ComputerKey, bool> OnKeyClicked;

        private const float Debounce = 0.1f;

        private const float KeyBump = 0.15f;

        private const float ColliderBump = 9f / 8f;

        public KeyBinding Binding;

        public AudioClip ClickSound;

        private MeshRenderer Renderer => GetComponent<MeshRenderer>();

        private BoxCollider Collider => GetComponent<BoxCollider>();

        private Vector3 centre;

        private Vector3 localPosition;

        private float _clickTime;

        public void Awake()
        {
            if (TryGetComponent(out GorillaKeyboardButton button))
            {
                Binding = (KeyBinding)Enum.Parse(typeof(KeyBinding), button.Binding.ToString());
                Destroy(button);
            }

            gameObject.layer = (int)UnityLayer.GorillaInteractable;

            Renderer.material.color = Color.white;

            Collider.isTrigger = true;
            centre = Collider.center;

            localPosition = transform.localPosition;
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (Time.realtimeSinceStartup > _clickTime + Debounce && collider.TryGetComponent(out GorillaTriggerColliderHandIndicator component))
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
                Renderer.material.color = new Color(0.715f, 0.7f, 0.7f);
                transform.localPosition = localPosition - Vector3.up * KeyBump;
                Collider.center = centre - Vector3.forward * KeyBump / ColliderBump;
            }
            else
            {
                Renderer.material.color = Color.white;
                transform.localPosition = localPosition;
                Collider.center = centre;
            }
        }
    }
}
