using UnityEngine;
using UltimoActo.Core;
using UltimoActo.Interaction;

namespace UltimoActo.Puzzles
{
    /// <summary>
    /// Dial interactivo que controla una de las dos mitades de un CoopSplitKeyLockPuzzle.
    /// Ambos jugadores pueden ver el estado de los dos tambores en el cilindro compartido,
    /// y pueden girar los tambores tras comunicarse las mitades de la clave entre 1929 y Ruinas.
    /// </summary>
    public class CoopSplitKeyDialNode : MonoBehaviour, IInteractable
    {
        [SerializeField] private CoopSplitKeyLockPuzzle parentLockPuzzle;
        [SerializeField] private bool isRightHalfDial = false;
        [SerializeField] private string dialDisplayName = "Tambor Izquierdo (Mitad A — 1929)";
        [SerializeField] private EraVisibilityMode requiredInteractionEra = EraVisibilityMode.Both;

        public string Id => isRightHalfDial ? "coop_split_dial_right" : "coop_split_dial_left";

        public void Configure(
            CoopSplitKeyLockPuzzle lockPuzzle,
            bool rightHalf,
            string displayName,
            EraVisibilityMode eraRestriction = EraVisibilityMode.Both)
        {
            parentLockPuzzle = lockPuzzle;
            isRightHalfDial = rightHalf;
            dialDisplayName = displayName;
            requiredInteractionEra = eraRestriction;
        }

        public string Prompt
        {
            get
            {
                if (parentLockPuzzle == null) return string.Empty;
                if (parentLockPuzzle.IsSolved)
                {
                    return $"{dialDisplayName}: [{parentLockPuzzle.GetSymbolLabel(isRightHalfDial)}] (Cerrojo Desbloqueado)";
                }
                return $"[E] Girar {dialDisplayName} · Actual: [{parentLockPuzzle.GetSymbolLabel(isRightHalfDial)}]";
            }
        }

        public bool CanInteract(GameObject player)
        {
            return parentLockPuzzle != null && !parentLockPuzzle.IsSolved;
        }

        public void Interact(GameObject player)
        {
            if (parentLockPuzzle == null || parentLockPuzzle.IsSolved) return;
            parentLockPuzzle.RotateHalfDial(isRightHalfDial, requiredInteractionEra);
        }
    }
}
