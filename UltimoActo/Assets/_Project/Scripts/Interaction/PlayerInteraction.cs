using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UltimoActo.Interaction
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Configuración de Raycast")]
        [Tooltip("Cámara desde donde se proyecta la mirada")]
        [SerializeField] private Camera playerCamera;

        [Tooltip("Distancia máxima de interacción en metros")]
        [SerializeField] private float maxDistance = 2.5f;

        [Tooltip("Capas con las que puede colisionar el rayo de interacción")]
        [SerializeField] private LayerMask interactionLayers = ~0;

        public event Action<IInteractable> OnFocusInteractable;
        public event Action OnLostFocusInteractable;

        private IInteractable currentInteractable;

        public IInteractable CurrentInteractable => currentInteractable;

        private void Awake()
        {
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
                if (playerCamera == null) playerCamera = Camera.main;
            }
        }

        private void Start()
        {
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
                if (playerCamera == null) playerCamera = Camera.main;
            }
        }

        private void Update()
        {
            PerformRaycast();
            CheckInteractionInput();
        }

        private void PerformRaycast()
        {
            if (playerCamera == null) return;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactionLayers))
            {
                IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    if (interactable != currentInteractable)
                    {
                        currentInteractable = interactable;
                        OnFocusInteractable?.Invoke(currentInteractable);
                    }
                    return;
                }
            }

            // Si no golpeó ningún interactuable
            if (currentInteractable != null)
            {
                currentInteractable = null;
                OnLostFocusInteractable?.Invoke();
            }
        }

        private void CheckInteractionInput()
        {
            if (currentInteractable == null) return;

            bool interactPressed = false;
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
            {
                interactPressed = true;
            }
#else
            if (Input.GetKeyDown(KeyCode.E))
            {
                interactPressed = true;
            }
#endif

            if (interactPressed && currentInteractable.CanInteract(gameObject))
            {
                currentInteractable.Interact(gameObject);
                // Forzar refresco del prompt tras interactuar
                OnFocusInteractable?.Invoke(currentInteractable);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (playerCamera == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * maxDistance);
        }
    }
}
