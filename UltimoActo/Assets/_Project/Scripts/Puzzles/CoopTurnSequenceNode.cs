using UnityEngine;
using UltimoActo.Core;
using UltimoActo.Interaction;

namespace UltimoActo.Puzzles
{
    /// <summary>
    /// Nodo individual pulsable para un CoopTurnSequencePuzzle (Tipo 3).
    /// </summary>
    public class CoopTurnSequenceNode : MonoBehaviour, IInteractable
    {
        [SerializeField] private CoopTurnSequencePuzzle parentPuzzle;
        [SerializeField] private int nodeIndex = 0;
        [SerializeField] private EraVisibilityMode nodeEra = EraVisibilityMode.Only1929;
        [SerializeField] private string nodeLabel = "Tecla / Posición";

        public string Id => $"coop_turn_node_{nodeIndex}";

        public void Configure(CoopTurnSequencePuzzle puzzle, int index, EraVisibilityMode eraMode, string label)
        {
            parentPuzzle = puzzle;
            nodeIndex = index;
            nodeEra = eraMode;
            nodeLabel = label;

            EraOnlyVisibility eov = GetComponent<EraOnlyVisibility>();
            if (eov == null) eov = gameObject.AddComponent<EraOnlyVisibility>();
            eov.Configure(eraMode, true);
        }

        public string Prompt
        {
            get
            {
                if (parentPuzzle == null) return string.Empty;
                if (parentPuzzle.IsSolved) return $"{nodeLabel} [Completado]";
                string eraTag = nodeEra == EraVisibilityMode.Only1929
                    ? "[Turno 1929]"
                    : (nodeEra == EraVisibilityMode.OnlyRuins ? "[Turno Ruinas]" : "");
                return $"[E] Accionar {nodeLabel} {eraTag} (Progreso: {parentPuzzle.CurrentStepProgress}/{parentPuzzle.TotalSteps})";
            }
        }

        public bool CanInteract(GameObject player)
        {
            return parentPuzzle != null && !parentPuzzle.IsSolved;
        }

        public void Interact(GameObject player)
        {
            if (parentPuzzle == null || parentPuzzle.IsSolved) return;
            parentPuzzle.PressTurnNode(nodeIndex, nodeEra);
        }
    }
}
