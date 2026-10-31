using UnityEngine;
using UltimoActo.Interaction;
using UltimoActo.UI;

namespace UltimoActo.Narrative
{
    public class WorldDocument : MonoBehaviour, IInteractable
    {
        [Header("Datos de la Nota")]
        [SerializeField] private string id = "doc_isadora_clue";
        [SerializeField] private string title = "Nota de Isadora Vale";
        [SerializeField] private string subtitle = "Directora de la Academia (1974)";
        [TextArea(5, 10)]
        [SerializeField] private string content = 
            "\"Para que la coreografía de 'El Lago de las Sombras' cobre vida tras el telón, cada símbolo debe orientarse a su destino:\n\n" +
            "1. El Primer Pilar mira hacia la Luz del Alba: el Cisne Blanco.\n" +
            "2. El Segundo Pilar se inclina hacia la Oscuridad oculta: el Cisne Negro.\n" +
            "3. El Tercer Pilar aguarda frente al Escenario: el Telón Teatral.\n\n" +
            "Solo cuando los tres guarden silencio, el secreto del archivo se revelará.\"";

        public string Id => id;
        public string Title => title;
        public string Subtitle => subtitle;
        public string Content => content;

        public string Prompt => $"[E] Leer {title}";

        public bool CanInteract(GameObject player) => true;

        public void Interact(GameObject player)
        {
            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.OpenDocument(this);
            }
            else
            {
                Debug.Log($"[Documento] {title}:\n{content}");
            }
        }
    }
}
