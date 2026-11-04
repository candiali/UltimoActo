using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UltimoActo.UI;

namespace UltimoActo.Interaction
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Configuración de Raycast")]
        [Tooltip("Cámara desde donde se proyecta la mirada")]
        [SerializeField] private Camera playerCamera;

        [Tooltip("Distancia máxima de interacción en metros")]
        [SerializeField] private float maxDistance = 3.8f;

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
            if (DocumentUI.Instance != null && DocumentUI.Instance.IsOpen) return;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactionLayers))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable == null)
                {
                    var behaviours = hit.collider.GetComponentsInParent<MonoBehaviour>();
                    for (int i = 0; i < behaviours.Length; i++)
                    {
                        if (behaviours[i] is IInteractable it)
                        {
                            interactable = it;
                            break;
                        }
                    }
                }

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
            if (DocumentUI.Instance != null && (DocumentUI.Instance.IsOpen || Time.unscaledTime - DocumentUI.Instance.LastClosedTime < 0.35f || Time.frameCount == DocumentUI.Instance.LastClosedFrame)) return;

            bool interactPressed = false;
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if ((keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)) ||
                (mouse != null && mouse.leftButton.wasPressedThisFrame))
            {
                interactPressed = true;
            }
#endif
#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
            if (!interactPressed && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0)))
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
