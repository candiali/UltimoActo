using System;
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

        public static event Action<WorldItem> OnWorldItemPickedUp;

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

        public bool CanInteract(GameObject player) => gameObject.activeSelf;

        public void Interact(GameObject player)
        {
            var inventory = player != null ? player.GetComponent<SimpleInventory>() : null;
            if (inventory == null) inventory = FindFirstObjectByType<SimpleInventory>();
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
                var flashlight = player != null ? player.GetComponentInChildren<FlashlightController>() : null;
                if (flashlight == null) flashlight = FindFirstObjectByType<FlashlightController>();
                if (flashlight != null)
                {
                    flashlight.UnlockFlashlight();
                }
            }

            Debug.Log($"[Objeto Recogido] {displayName} (ID: {id}) recogido por el jugador.");

            // Desactivamos el objeto en la escena y notificamos a la capa de sincronización LAN
            gameObject.SetActive(false);
            OnWorldItemPickedUp?.Invoke(this);
        }

        public void ApplyNetworkPickedUp(bool showToast = false)
        {
            SimpleInventory inv = FindFirstObjectByType<SimpleInventory>();
            if (inv != null)
            {
                inv.AddKeyFromNetwork(id, displayName);
            }

            if (id == "flashlight_brass_1924" || itemType == ItemType.Tool)
            {
                var flashlight = FindFirstObjectByType<FlashlightController>();
                if (flashlight != null && !flashlight.HasFlashlight)
                {
                    flashlight.UnlockFlashlight();
                }
            }

            if (showToast && NightManager.Instance != null)
            {
                NightManager.Instance.ShowToast($"[Cooperativo] Objeto recogido: {displayName}", 3.0f);
            }

            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        public void Configure(string itemId, string itemName, ItemType type = ItemType.Key)
        {
            id = itemId;
            displayName = itemName;
            itemType = type;
        }
    }
}
