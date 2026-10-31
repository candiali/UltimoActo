using UnityEngine;
using UltimoActo.Interaction;

namespace UltimoActo.Puzzles
{
    /// <summary>
    /// Palanca o interruptor para reiniciar un puzle.
    /// Cumple la regla del GDD: 'Todo puzle importante debe tener mecanismo de reinicio para evitar softlocks'.
    /// </summary>
    public class PuzzleResetLever : MonoBehaviour, IInteractable
    {
        [SerializeField] private string id = "lever_reset_pedestals";
        [SerializeField] private PuzzleBase targetPuzzle;
        [SerializeField] private string prompt = "[E] Tirar de la palanca de reinicio";

        public string Id => id;
        public string Prompt => prompt;

        public bool CanInteract(GameObject player) => targetPuzzle != null;

        public void Interact(GameObject player)
        {
            if (targetPuzzle != null)
            {
                Debug.Log($"[Palanca] Reiniciando el puzle '{targetPuzzle.PuzzleId}'...");
                targetPuzzle.ResetPuzzle();
            }
        }
    }
}
