using UnityEngine;
using UltimoActo.Core;
using UltimoActo.Interaction;

namespace UltimoActo.Puzzles
{
    /// <summary>
    /// Palanca o interruptor individual para un CoopSimultaneousSwitchPuzzle (Tipo 2).
    /// </summary>
    public class CoopSimultaneousSwitchNode : MonoBehaviour, IInteractable
    {
        [SerializeField] private CoopSimultaneousSwitchPuzzle parentPuzzle;
        [SerializeField] private bool isRuinsSwitch = false;
        [SerializeField] private string switchLabel = "Palanca de Sincronía";

        public string Id => isRuinsSwitch ? "coop_sim_switch_ruins" : "coop_sim_switch_1929";

        public void Configure(CoopSimultaneousSwitchPuzzle puzzle, bool ruinsEra, string label)
        {
            parentPuzzle = puzzle;
            isRuinsSwitch = ruinsEra;
            switchLabel = label;

            EraOnlyVisibility eov = GetComponent<EraOnlyVisibility>();
            if (eov == null) eov = gameObject.AddComponent<EraOnlyVisibility>();
            eov.Configure(ruinsEra ? EraVisibilityMode.OnlyRuins : EraVisibilityMode.Only1929, true);
        }

        public string Prompt
        {
            get
            {
                if (parentPuzzle == null) return string.Empty;
                if (parentPuzzle.IsSolved) return $"{switchLabel} [Sincronizado]";

                bool latched = isRuinsSwitch ? parentPuzzle.IsSwitchRuinsLatched : parentPuzzle.IsSwitch1929Latched;
                float rem = isRuinsSwitch ? parentPuzzle.RemainingRuins : parentPuzzle.Remaining1929;
                if (latched)
                {
                    return $"{switchLabel} [ACTIVO: {rem:0.0} s — Esperando a la otra época]";
                }
                string eraTag = isRuinsSwitch ? "[Época Ruinas]" : "[Época 1929]";
                return $"[E] Accionar {switchLabel} {eraTag}";
            }
        }

        public bool CanInteract(GameObject player)
        {
            return parentPuzzle != null && !parentPuzzle.IsSolved;
        }

        public void Interact(GameObject player)
        {
            if (parentPuzzle == null || parentPuzzle.IsSolved) return;
            parentPuzzle.TriggerSwitch(isRuinsSwitch);
        }
    }
}
