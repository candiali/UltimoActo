using UnityEngine;
using UltimoActo.Core;
using UltimoActo.Player;

namespace UltimoActo.Interaction
{
    public enum ItemType
    {
        Key,
        Document,
        Collectible,
        Fuse,
        Tool,
        Battery
    }

    public class WorldItem : MonoBehaviour, IInteractable
    {
        [Header("Datos del Objeto")]
        [SerializeField] private string id = "key_brass_rehearsal";
        [SerializeField] private string displayName = "Llave de la Sala de Ensayo";
        [SerializeField] private ItemType itemType = ItemType.Key;

        [Header("Feedback")]
        [SerializeField] private bool rotateInPlace = true;
        [SerializeField] private float rotationSpeed = 45f;

        public string Id => id;
        public string DisplayName => displayName;
        public ItemType Type => itemType;
        public string Prompt => $"[E] Recoger {displayName}";

        private void Update()
        {
            if (rotateInPlace)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            }
        }

        public bool CanInteract(GameObject player) => true;

        public void Interact(GameObject player)
        {
            var inventory = player.GetComponent<SimpleInventory>();
            if (inventory != null)
            {
                inventory.AddKey(id, displayName);
            }

            if (NightManager.Instance != null)
            {
                NightManager.Instance.ShowToast($"+ Objeto recogido: {displayName}", 3.2f);
            }

            // Si es la linterna, activarla directamente en el controlador del jugador
            if (id == "flashlight_brass_1924" || itemType == ItemType.Tool)
            {
                var flashlight = player.GetComponentInChildren<FlashlightController>();
                if (flashlight == null) flashlight = FindFirstObjectByType<FlashlightController>();
                if (flashlight != null)
                {
                    flashlight.UnlockFlashlight();
                }
            }

            Debug.Log($"[Objeto Recogido] {displayName} (ID: {id}) recogido por el jugador.");

            // Desactivamos el objeto en la escena
            gameObject.SetActive(false);
        }

        public void Configure(string itemId, string itemName, ItemType type = ItemType.Key)
        {
            id = itemId;
            displayName = itemName;
            itemType = type;
        }
    }
}
