using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UltimoActo.UI
{
    /// <summary>
    /// Añade un movimiento sutil y atmosférico a la cámara del menú principal:
    /// una respiración lenta y un leve paneo cinemático reactivo a la posición del ratón.
    /// Compatible 100% con el nuevo Input System de Unity.
    /// </summary>
    public class MenuCameraPan : MonoBehaviour
    {
        [Header("Balanceo Cinemático")]
        [SerializeField] private float swayAmount = 0.08f;
        [SerializeField] private float swaySpeed = 0.35f;

        [Header("Efecto Parallax del Ratón")]
        [SerializeField] private float mouseInfluence = 0.05f;
        [SerializeField] private float mouseSmoothing = 2.5f;

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Vector2 smoothMouse;

        private void Start()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }

        private void Update()
        {
            // Movimiento suave y lento tipo respiración
            float sinX = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
            float cosY = Mathf.Cos(Time.time * swaySpeed * 0.8f) * (swayAmount * 0.6f);

            // Posición del ratón compatible con el New Input System
            Vector2 mousePos = GetMousePosition();
            Vector2 mouseNorm = new Vector2(
                (mousePos.x / Screen.width) - 0.5f,
                (mousePos.y / Screen.height) - 0.5f
            );

            smoothMouse = Vector2.Lerp(smoothMouse, mouseNorm, Time.deltaTime * mouseSmoothing);

            Vector3 targetPos = initialPosition + new Vector3(
                sinX + (smoothMouse.x * mouseInfluence),
                cosY + (smoothMouse.y * mouseInfluence * 0.5f),
                0f
            );

            transform.position = targetPos;

            // Rotación leve hacia el punto de mira
            float rotY = smoothMouse.x * 1.5f;
            float rotX = -smoothMouse.y * 1.2f;
            transform.rotation = initialRotation * Quaternion.Euler(rotX, rotY, 0f);
        }

        private Vector2 GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null)
            {
                return mouse.position.ReadValue();
            }
            return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
#else
            return Input.mousePosition;
#endif
        }
    }
}
