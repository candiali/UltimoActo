using UnityEngine;
using UnityEngine.Events;
using UltimoActo.Interaction;

namespace UltimoActo.Puzzles
{
    public enum PuzzleState : byte
    {
        Locked,
        Active,
        Solved
    }

    /// <summary>
    /// Clase base abstracta para todos los puzles de Último Acto.
    /// Diseñada según la skill 'ultimo-acto-puzles' para migrar limpiamente a NetworkPuzzle en la Fase 2.
    /// </summary>
    public abstract class PuzzleBase : MonoBehaviour
    {
        [Header("Configuración General")]
        [SerializeField] protected string puzzleId = "puz_sample";
        [SerializeField] protected PuzzleState state = PuzzleState.Active;

        [Header("Recompensas / Desbloqueos")]
        [Tooltip("Puerta opcional que se abrirá automáticamente al resolver el puzle")]
        [SerializeField] protected WorldDoor doorToUnlock;

        [Tooltip("ID de llave o ítem que se otorgará o activará")]
        [SerializeField] protected string rewardItemId = "";

        [Header("Eventos")]
        public UnityEvent onSolved;
        public UnityEvent onReset;

        public string PuzzleId => puzzleId;
        public PuzzleState State => state;
        public bool IsSolved => state == PuzzleState.Solved;

        public virtual void Solve()
        {
            if (state == PuzzleState.Solved) return; // Idempotente

            state = PuzzleState.Solved;
            Debug.Log($"<color=yellow>[Puzle Resuelto]</color> Puzle '{puzzleId}' completado exitosamente!");

            if (doorToUnlock != null)
            {
                doorToUnlock.SetLockState(false);
                Debug.Log($"[Puzle] Puerta '{doorToUnlock.Id}' desbloqueada por el puzle.");
            }

            onSolved?.Invoke();
        }

        public abstract void ResetPuzzle();
    }
}
