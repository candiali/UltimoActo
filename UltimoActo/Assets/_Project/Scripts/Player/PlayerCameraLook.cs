using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UltimoActo.Player
{
    public class PlayerCameraLook : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("Transform de la cámara (hija del jugador)")]
        [SerializeField] private Transform cameraTransform;

        [Header("Sensibilidad")]
        [SerializeField] private float mouseSensitivity = 0.12f;

        [Header("Límites Verticales")]
        [SerializeField] private float minPitch = -85f;
        [SerializeField] private float maxPitch = 85f;

        private float cameraPitch = 0f;
        private bool isCursorLocked = true;

        public Transform CameraTransform => cameraTransform;

        private void Start()
        {
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            SetCursorLock(true);
        }

        private void Update()
        {
            HandleCursorToggle();

            if (!isCursorLocked) return;

            Vector2 lookInput = ReadLookDelta();

            // Rotación horizontal (Yaw): afecta a todo el cuerpo del jugador
            transform.Rotate(Vector3.up * (lookInput.x * mouseSensitivity));

            // Rotación vertical (Pitch): afecta sólo a la cámara
            cameraPitch -= lookInput.y * mouseSensitivity;
            cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

            if (cameraTransform != null)
            {
                cameraTransform.localEulerAngles = Vector3.right * cameraPitch;
            }
        }

        private Vector2 ReadLookDelta()
        {
            Vector2 delta = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null)
            {
                delta = mouse.delta.ReadValue();
            }
#else
            delta.x = Input.GetAxis("Mouse X") * 10f;
            delta.y = Input.GetAxis("Mouse Y") * 10f;
#endif
            return delta;
        }

        private void HandleCursorToggle()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                SetCursorLock(!isCursorLocked);
            }
            else if (mouse != null && mouse.leftButton.wasPressedThisFrame && !isCursorLocked)
            {
                SetCursorLock(true);
            }
#else
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetCursorLock(!isCursorLocked);
            }
            else if (Input.GetMouseButtonDown(0) && !isCursorLocked)
            {
                SetCursorLock(true);
            }
#endif
        }

        public void SetCursorLock(bool locked)
        {
            isCursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
