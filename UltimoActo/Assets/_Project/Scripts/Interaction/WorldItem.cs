using UnityEngine;
using UltimoActo.Core;

namespace UltimoActo.Interaction
{
    public enum ItemType
    {
        Key,
        Document,
        Collectible
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
            if (itemType == ItemType.Key)
            {
                var inventory = player.GetComponent<SimpleInventory>();
                if (inventory != null)
                {
                    inventory.AddKey(id, displayName);
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
