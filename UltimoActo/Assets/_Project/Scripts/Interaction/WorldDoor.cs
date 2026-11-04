using System.Collections;
using UnityEngine;
using UltimoActo.Core;

namespace UltimoActo.Interaction
{
    public class WorldDoor : MonoBehaviour, IInteractable
    {
        [Header("Identificación")]
        [SerializeField] private string id = "door_sample";

        [Header("Configuración de la Puerta")]
        [Tooltip("Transform que rotará (bisagra/hinge). Si se deja vacío, rota este mismo GameObject")]
        [SerializeField] private Transform doorHinge;

        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float animationDuration = 0.8f;

        [Header("Estado y Llave")]
        [SerializeField] private bool isLocked = false;
        [SerializeField] private string requiredKeyId = "";
        [SerializeField] private bool isOpen = false;
        [SerializeField] private string lockedDescription = "Cerrada con llave";

        private Quaternion closedRotation;
        private Quaternion openRotation;
        private Coroutine rotateCoroutine;

        public string Id => id;
        public bool IsOpen => isOpen;
        public bool IsLocked => isLocked;

        public string Prompt
        {
            get
            {
                if (isLocked)
                {
                    if (!string.IsNullOrEmpty(lockedDescription))
                        return $"[E] {lockedDescription}";
                    return "[E] Cerrada con llave";
                }
                return isOpen ? "[E] Cerrar puerta" : "[E] Abrir puerta";
            }
        }

        private void Awake()
        {
            InitRotations();
        }

        private void Start()
        {
            InitRotations();
        }

        public void InitRotations()
        {
            if (doorHinge == null) doorHinge = transform;

            if (isOpen)
            {
                openRotation = doorHinge.localRotation;
                closedRotation = openRotation * Quaternion.Euler(0f, -openAngle, 0f);
            }
            else
            {
                closedRotation = doorHinge.localRotation;
                openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
            }
        }

        public bool CanInteract(GameObject player)
        {
            return rotateCoroutine == null;
        }

        public void Interact(GameObject player)
        {
            if (isLocked)
            {
                var inventory = player.GetComponent<SimpleInventory>();
                if (inventory != null && inventory.HasKey(requiredKeyId))
                {
                    isLocked = false;
                    Debug.Log($"[Puerta] {id} desbloqueada usando la llave {requiredKeyId}!");
                    ToggleDoor();
                }
                else
                {
                    Debug.Log($"[Puerta] {id} está cerrada. Se necesita la llave '{requiredKeyId}'.");
                }
                return;
            }

            ToggleDoor();
        }

        private void ToggleDoor()
        {
            if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
            isOpen = !isOpen;
            rotateCoroutine = StartCoroutine(AnimateDoor(isOpen ? openRotation : closedRotation));
        }

        private IEnumerator AnimateDoor(Quaternion targetRotation)
        {
            Quaternion startRotation = doorHinge.localRotation;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / animationDuration);
                doorHinge.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }

            doorHinge.localRotation = targetRotation;
            rotateCoroutine = null;
        }

        public void SetLockState(bool locked, string keyId = "")
        {
            isLocked = locked;
            if (!string.IsNullOrEmpty(keyId)) requiredKeyId = keyId;
        }

        public void Configure(Transform hinge, float angle = 90f, float duration = 0.8f, bool startOpen = false, bool locked = false, string keyId = "", string doorId = "", string lockDesc = "Cerrada con llave")
        {
            doorHinge = hinge;
            openAngle = angle;
            animationDuration = duration;
            isOpen = startOpen;
            isLocked = locked;
            requiredKeyId = keyId;
            if (!string.IsNullOrEmpty(doorId)) id = doorId;
            if (!string.IsNullOrEmpty(lockDesc)) lockedDescription = lockDesc;
            InitRotations();
        }
    }
}
