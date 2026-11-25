using UnityEngine;
using UltimoActo.Core;
using UltimoActo.Interaction;
using UltimoActo.UI;

namespace UltimoActo.Puzzles
{
    /// <summary>
    /// Componente pequeño y reutilizable para pistas u objetos de inspección que solo existen
    /// en una época (Tipo 1 y Tipo 4: Pista de 1929 o Pista de las Ruinas).
    /// - Asegura que sus mallas visuales tengan la capa Era1929 o EraRuins (mediante EraOnlyVisibility).
    /// - Solo permite interactuar cuando el jugador está viendo la época correspondiente
    ///   (o indica cambiar con [Q] en Modo Solo).
    /// - Al examinarse, abre el visor de documentos (DocumentUI) y archiva automáticamente la pista
    ///   en el Diario Compartido ([J]/[TAB]) con la etiqueta "[SOLO VISIBLE EN 1929]" o "[SOLO VISIBLE EN RUINAS]".
    /// </summary>
    public class CoopEraCluePlaque : MonoBehaviour, IInteractable
    {
        [Header("Puzle Asociado y Época")]
        [SerializeField] private CoopPuzzleBase parentCoopPuzzle;
        [SerializeField] private string clueSubId = "half_a";
        [SerializeField] private EraVisibilityMode eraVisibility = EraVisibilityMode.Only1929;

        [Header("Contenido de la Pista")]
        [SerializeField] private string promptTitle = "Inscripción de Época";
        [SerializeField] private string documentTitle = "Inscripción de 1929";
        [TextArea(4, 8)]
        [SerializeField] private string documentBody = "";

        private bool hasBeenRead;

        public string Id => $"coop_clue_plaque_{clueSubId}";

        public void Configure(
            CoopPuzzleBase puzzle,
            string subId,
            EraVisibilityMode visibilityMode,
            string interactPromptTitle,
            string docTitle,
            string docBody)
        {
            parentCoopPuzzle = puzzle;
            clueSubId = subId;
            eraVisibility = visibilityMode;
            promptTitle = interactPromptTitle;
            documentTitle = docTitle;
            documentBody = docBody;

            EraOnlyVisibility eov = GetComponent<EraOnlyVisibility>();
            if (eov == null) eov = gameObject.AddComponent<EraOnlyVisibility>();
            eov.Configure(visibilityMode, true);
        }

        private bool IsVisibleInCurrentLocalEra()
        {
            if (eraVisibility == EraVisibilityMode.Both) return true;
            EraVisual current = EraVisualManager.Instance != null ? EraVisualManager.Instance.CurrentEra : EraVisual.Glory1929;
            if (eraVisibility == EraVisibilityMode.Only1929) return current == EraVisual.Glory1929;
            if (eraVisibility == EraVisibilityMode.OnlyRuins) return current == EraVisual.Ruins;
            return true;
        }

        public string Prompt
        {
            get
            {
                if (!IsVisibleInCurrentLocalEra())
                {
                    return string.Empty;
                }
                string eraTag = eraVisibility == EraVisibilityMode.Only1929
                    ? "[Época 1929]"
                    : (eraVisibility == EraVisibilityMode.OnlyRuins ? "[Época Ruinas]" : "");
                string readTag = hasBeenRead ? " (Archivada en Diario [J])" : "";
                return $"[E] Examinar {promptTitle} {eraTag}{readTag}";
            }
        }

        public bool CanInteract(GameObject player)
        {
            return IsVisibleInCurrentLocalEra();
        }

        public void Interact(GameObject player)
        {
            if (!IsVisibleInCurrentLocalEra()) return;

            hasBeenRead = true;

            if (parentCoopPuzzle != null)
            {
                parentCoopPuzzle.RegisterCoopClueInJournal(
                    clueSubId,
                    documentTitle,
                    documentBody,
                    eraVisibility,
                    JournalTabCategory.PuzzleClues);
            }

            if (DocumentUI.Instance != null)
            {
                string visSubtitle = eraVisibility == EraVisibilityMode.Only1929
                    ? "Pista Cooperativa — SOLO VISIBLE EN 1929 (Comunícasela a tu compañero)"
                    : (eraVisibility == EraVisibilityMode.OnlyRuins
                        ? "Pista Cooperativa — SOLO VISIBLE EN RUINAS (Comunícasela a tu compañero)"
                        : "Pista Cooperativa — Archivo Vesper");

                DocumentUI.Instance.ShowDocumentWithId(
                    docId: parentCoopPuzzle != null ? $"coop_clue_{parentCoopPuzzle.PuzzleId}_{clueSubId}" : Id,
                    title: documentTitle,
                    body: documentBody,
                    subtitle: visSubtitle,
                    skipJournalRecord: true);
            }
        }
    }
}
