using UnityEngine;

namespace UltimoActo.Interaction
{
    /// <summary>
    /// Nodo pulsable de un puzzle de secuencia ordenada (frascos, jarrones, botones...).
    /// </summary>
    public class SequenceNode : MonoBehaviour, IInteractable
    {
        [SerializeField] private OrderedSequencePuzzle puzzle;
        [SerializeField] private int nodeIndex;
        [SerializeField] private string label = "Nodo";

        public string Id => $"seq_node_{nodeIndex}";
        public string Prompt => puzzle != null && puzzle.IsSolved ? label : $"[E] {label}";
        public bool CanInteract(GameObject player) => puzzle != null && !puzzle.IsSolved;
        public void Interact(GameObject player) { if (puzzle != null) puzzle.Press(nodeIndex, transform); }

        public void Configure(OrderedSequencePuzzle p, int index, string nodeLabel)
        {
            puzzle = p; nodeIndex = index; label = nodeLabel;
        }
    }
}
