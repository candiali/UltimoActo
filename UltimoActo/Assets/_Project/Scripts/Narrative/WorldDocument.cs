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

        public void ConfigureDocument(string docId, string docTitle, string docContent, string docSubtitle = "")
        {
            if (!string.IsNullOrEmpty(docId)) id = docId;
            if (!string.IsNullOrEmpty(docTitle)) title = docTitle;
            content = docContent ?? string.Empty;
            if (!string.IsNullOrEmpty(docSubtitle)) subtitle = docSubtitle;
        }

        public string Prompt
        {
            get
            {
                UltimoActo.Core.EraOnlyVisibility eov = GetComponentInParent<UltimoActo.Core.EraOnlyVisibility>();
                if (eov != null && eov.Visibility != UltimoActo.Core.EraVisibilityMode.Both)
                {
                    var curEra = UltimoActo.Core.EraVisualManager.Instance != null
                        ? UltimoActo.Core.EraVisualManager.Instance.CurrentEra
                        : UltimoActo.Core.EraVisual.Glory1929;
                    if (eov.Visibility == UltimoActo.Core.EraVisibilityMode.Only1929 && curEra != UltimoActo.Core.EraVisual.Glory1929)
                        return string.Empty;
                    if (eov.Visibility == UltimoActo.Core.EraVisibilityMode.OnlyRuins && curEra != UltimoActo.Core.EraVisual.Ruins)
                        return string.Empty;
                }
                return $"[E] Leer {title}";
            }
        }

        public bool CanInteract(GameObject player)
        {
            UltimoActo.Core.EraOnlyVisibility eov = GetComponentInParent<UltimoActo.Core.EraOnlyVisibility>();
            if (eov != null && eov.Visibility != UltimoActo.Core.EraVisibilityMode.Both)
            {
                var curEra = UltimoActo.Core.EraVisualManager.Instance != null
                    ? UltimoActo.Core.EraVisualManager.Instance.CurrentEra
                    : UltimoActo.Core.EraVisual.Glory1929;
                if (eov.Visibility == UltimoActo.Core.EraVisibilityMode.Only1929 && curEra != UltimoActo.Core.EraVisual.Glory1929)
                    return false;
                if (eov.Visibility == UltimoActo.Core.EraVisibilityMode.OnlyRuins && curEra != UltimoActo.Core.EraVisual.Ruins)
                    return false;
            }
            return true;
        }

        public void Interact(GameObject player)
        {
            DocumentUI ui = DocumentUI.Instance;
            if (ui == null)
            {
                ui = Object.FindFirstObjectByType<DocumentUI>(FindObjectsInactive.Include);
            }

            if (ui != null)
            {
                ui.OpenDocument(this);
            }
            else
            {
                Debug.LogWarning($"[WorldDocument] No se encontró DocumentUI en la escena para '{title}'. Mostrando en consola:\n{content}");
            }
        }
    }
}
