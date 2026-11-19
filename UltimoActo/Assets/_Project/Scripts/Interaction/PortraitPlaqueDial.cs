using UnityEngine;

namespace UltimoActo.Interaction
{
    /// <summary>
    /// Componente interactivo colocado en cada una de las 4 placas de latón de la Galería de Retratos.
    /// Permite al jugador girar el dial y seleccionar una de las 4 estaciones de coronación.
    /// </summary>
    public class PortraitPlaqueDial : MonoBehaviour, IInteractable
    {
        [SerializeField] private PortraitGalleryPuzzleController puzzleController;
        [SerializeField] private int portraitIndex = 0;
        [SerializeField] private string primaDonnaName = "Beatrice Dupont (1898)";

        public string Id => $"portrait_dial_{portraitIndex}";
        public string Prompt
        {
            get
            {
                if (puzzleController != null && puzzleController.IsSolved)
                {
                    return $"Placa de {primaDonnaName} [Fijada]";
                }
                string currentSeason = puzzleController != null ? puzzleController.GetSeasonName(portraitIndex) : "I";
                return $"[E] Girar dial de {primaDonnaName} [Actual: {currentSeason}]";
            }
        }

        public bool CanInteract(GameObject player)
        {
            return puzzleController != null && !puzzleController.IsSolved;
        }

        public void Interact(GameObject player)
        {
            if (puzzleController != null)
            {
                puzzleController.RotateDial(portraitIndex);
            }
        }

        public void Configure(PortraitGalleryPuzzleController controller, int index, string dancerName)
        {
            puzzleController = controller;
            portraitIndex = index;
            primaDonnaName = dancerName;
        }
    }
}
