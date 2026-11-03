using System.Collections;
using UnityEngine;
using UltimoActo.Core;

namespace UltimoActo.Interaction
{
    /// <summary>
    /// Puerta doble monumental (típica de la entrada principal de la academia).
    /// Rota simultáneamente ambas hojas (izquierda hacia afuera/lado izquierdo, derecha hacia el lado derecho).
    /// </summary>
    public class WorldDoubleDoor : MonoBehaviour, IInteractable
    {
        [Header("Identificación")]
        [SerializeField] private string id = "door_grand_entrance";

        [Header("Hinges (Bisagras)")]
        [SerializeField] private Transform leftDoorHinge;
        [SerializeField] private Transform rightDoorHinge;

        [Header("Configuración de Apertura")]
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float animationDuration = 1.0f;

        [Header("Estado")]
        [SerializeField] private bool isLocked = false;
        [SerializeField] private string requiredKeyId = "";
        [SerializeField] private bool isOpen = false;

        private Quaternion leftClosedRot;
        private Quaternion leftOpenRot;
        private Quaternion rightClosedRot;
        private Quaternion rightOpenRot;
        private Coroutine animateCoroutine;

        public string Id => id;
        public bool IsOpen => isOpen;
        public bool IsLocked => isLocked;

        public string Prompt
        {
            get
            {
                if (isLocked) return "[E] Puertas atrancadas desde el interior";
                return isOpen ? "[E] Cerrar puertas dobles" : "[E] Abrir puertas de la Academia";
            }
        }

        private void Awake()
        {
            if (leftDoorHinge != null)
            {
                leftClosedRot = leftDoorHinge.localRotation;
                leftOpenRot = leftClosedRot * Quaternion.Euler(0f, -openAngle, 0f);
            }

            if (rightDoorHinge != null)
            {
                rightClosedRot = rightDoorHinge.localRotation;
                rightOpenRot = rightClosedRot * Quaternion.Euler(0f, openAngle, 0f);
            }
        }

        public bool CanInteract(GameObject player) => animateCoroutine == null;

        public void Interact(GameObject player)
        {
            if (isLocked)
            {
                var inventory = player.GetComponent<SimpleInventory>();
                if (inventory != null && inventory.HasKey(requiredKeyId))
                {
                    isLocked = false;
                    Debug.Log($"[Puertas Dobles] {id} desbloqueadas con la llave {requiredKeyId}!");
                    ToggleDoor();
                }
                else
                {
                    Debug.Log($"[Puertas Dobles] {id} están bloqueadas.");
                }
                return;
            }

            ToggleDoor();
        }

        public void ToggleDoor()
        {
            if (animateCoroutine != null) StopCoroutine(animateCoroutine);
            isOpen = !isOpen;
            animateCoroutine = StartCoroutine(AnimateDoubleDoors(isOpen));
        }

        private IEnumerator AnimateDoubleDoors(bool open)
        {
            float elapsed = 0f;
            Quaternion startLeft = leftDoorHinge != null ? leftDoorHinge.localRotation : Quaternion.identity;
            Quaternion targetLeft = open ? leftOpenRot : leftClosedRot;

            Quaternion startRight = rightDoorHinge != null ? rightDoorHinge.localRotation : Quaternion.identity;
            Quaternion targetRight = open ? rightOpenRot : rightClosedRot;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / animationDuration);

                if (leftDoorHinge != null)
                    leftDoorHinge.localRotation = Quaternion.Slerp(startLeft, targetLeft, t);

                if (rightDoorHinge != null)
                    rightDoorHinge.localRotation = Quaternion.Slerp(startRight, targetRight, t);

                yield return null;
            }

            if (leftDoorHinge != null) leftDoorHinge.localRotation = targetLeft;
            if (rightDoorHinge != null) rightDoorHinge.localRotation = targetRight;

            animateCoroutine = null;
        }

        public void SetLockState(bool locked, string keyId = "")
        {
            isLocked = locked;
            if (!string.IsNullOrEmpty(keyId)) requiredKeyId = keyId;
        }
    }
}
