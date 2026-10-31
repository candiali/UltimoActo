using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UltimoActo.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Velocidad de Movimiento")]
        [Tooltip("Velocidad estándar de caminata en m/s")]
        [SerializeField] private float walkSpeed = 3.5f;

        [Tooltip("Velocidad al correr en m/s")]
        [SerializeField] private float sprintSpeed = 5.5f;

        [Header("Física y Gravedad")]
        [SerializeField] private float gravity = -15f;
        [SerializeField] private float groundedGravity = -2f;

        private CharacterController controller;
        private Vector3 verticalVelocity;
        private bool isSprinting;

        public bool IsGrounded => controller != null && controller.isGrounded;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Vector2 input = ReadMoveInput();
            isSprinting = ReadSprintInput();

            float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

            // Movimiento relativo a la orientación horizontal del jugador
            Vector3 moveDirection = transform.right * input.x + transform.forward * input.y;
            if (moveDirection.sqrMagnitude > 1f)
            {
                moveDirection.Normalize();
            }

            controller.Move(moveDirection * currentSpeed * Time.deltaTime);

            // Gravedad
            if (controller.isGrounded && verticalVelocity.y < 0)
            {
                verticalVelocity.y = groundedGravity;
            }
            else
            {
                verticalVelocity.y += gravity * Time.deltaTime;
            }

            controller.Move(verticalVelocity * Time.deltaTime);
        }

        private Vector2 ReadMoveInput()
        {
            Vector2 input = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
            }
#else
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
#endif
            return input;
        }

        private bool ReadSprintInput()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
#else
            return Input.GetKey(KeyCode.LeftShift);
#endif
        }
    }
}
