using UnityEngine;
using UltimoActo.Core;
using UltimoActo.Interaction;

namespace UltimoActo.Puzzles
{
    /// <summary>
    /// BLOQUE B · Tipo 3 — Puzle Cooperativo de Secuencia por Turnos:
    /// - Una secuencia musical o coreográfica donde cada paso debe ser ejecutado por la época
    ///   correspondiente (ej: Paso 1 en 1929 -> Paso 2 en Ruinas -> Paso 3 en 1929 -> Paso 4 en Ruinas).
    /// - Si alguien pulsa fuera de su turno o el nodo equivocado, se reinicia con aviso claro.
    /// </summary>
    public class CoopTurnSequencePuzzle : CoopPuzzleBase
    {
        [SerializeField] private int[] expectedNodeIndices = { 0, 1, 2, 3 };
        [SerializeField] private EraVisibilityMode[] expectedStepEras =
        {
            EraVisibilityMode.Only1929,
            EraVisibilityMode.OnlyRuins,
            EraVisibilityMode.Only1929,
            EraVisibilityMode.OnlyRuins
        };
        [SerializeField] private GameObject[] rewardObjectsToEnable;

        private int currentStepProgress = 0;

        public int CurrentStepProgress => currentStepProgress;
        public int TotalSteps => expectedNodeIndices != null ? expectedNodeIndices.Length : 0;

        public void ConfigureTurnSequence(
            string id,
            string displayTitle,
            int[] nodeOrder,
            EraVisibilityMode[] stepEras,
            GameObject[] rewards,
            WorldDoor singleDoor = null,
            WorldDoubleDoor doubleDoor = null)
        {
            puzzleId = id;
            puzzleDisplayTitle = displayTitle;
            cooperationType = CoopCooperationType.TurnBasedSequence;
            expectedNodeIndices = nodeOrder;
            expectedStepEras = stepEras;
            rewardObjectsToEnable = rewards;
            doorToUnlock = singleDoor;
            doubleDoorToUnlock = doubleDoor;

            if (rewardObjectsToEnable != null)
            {
                for (int i = 0; i < rewardObjectsToEnable.Length; i++)
                {
                    if (rewardObjectsToEnable[i] != null) rewardObjectsToEnable[i].SetActive(false);
                }
            }
        }

        public void PressTurnNode(int nodeIndex, EraVisibilityMode nodeEra)
        {
            if (IsSolved || expectedNodeIndices == null || expectedNodeIndices.Length == 0) return;

            if (!CanInteractInEra(nodeEra, out string deniedReason))
            {
                if (NightManager.Instance != null) NightManager.Instance.ShowToast(deniedReason, 3.2f);
                return;
            }

            EraVisibilityMode expectedEra = (expectedStepEras != null && currentStepProgress < expectedStepEras.Length)
                ? expectedStepEras[currentStepProgress]
                : EraVisibilityMode.Both;

            bool correctNode = expectedNodeIndices[currentStepProgress] == nodeIndex;
            bool correctEra = expectedEra == EraVisibilityMode.Both || expectedEra == nodeEra;

            if (correctNode && correctEra)
            {
                currentStepProgress++;
                DispatchCoopSubStateToNetwork($"TURN_STEP|{currentStepProgress}");

                if (currentStepProgress >= expectedNodeIndices.Length)
                {
                    MarkTurnSequenceSolved(true);
                }
                else if (NightManager.Instance != null)
                {
                    EraVisibilityMode nextEra = (expectedStepEras != null && currentStepProgress < expectedStepEras.Length)
                        ? expectedStepEras[currentStepProgress]
                        : EraVisibilityMode.Both;
                    string nextWho = nextEra == EraVisibilityMode.Only1929
                        ? "Turno de 1929"
                        : (nextEra == EraVisibilityMode.OnlyRuins ? "Turno de las Ruinas" : "Siguiente paso");
                    NightManager.Instance.ShowToast($"Paso {currentStepProgress}/{expectedNodeIndices.Length} correcto — Ahora: {nextWho}", 3.2f);
                }
            }
            else
            {
                currentStepProgress = 0;
                DispatchCoopSubStateToNetwork("TURN_STEP|0");
                if (NightManager.Instance != null)
                {
                    NightManager.Instance.ShowToast($"[{puzzleDisplayTitle}] Orden o turno incorrecto — la secuencia se reinicia (0/{expectedNodeIndices.Length}).", 3.5f);
                }
            }
        }

        private void MarkTurnSequenceSolved(bool isLocalAction)
        {
            if (IsSolved) return;

            if (rewardObjectsToEnable != null)
            {
                for (int i = 0; i < rewardObjectsToEnable.Length; i++)
                {
                    if (rewardObjectsToEnable[i] != null) rewardObjectsToEnable[i].SetActive(true);
                }
            }

            string summary =
                $"SECUENCIA POR TURNOS COMPLETADA ({puzzleDisplayTitle}):\n\n" +
                "Ambos investigadores alternaron sus acciones entre la Época 1929 y las Ruinas en el orden exacto de la partitura/coreografía.";

            CompleteCoopPuzzle(summary, isLocalAction);

            if (NightManager.Instance != null)
            {
                NightManager.Instance.ShowToast($"¡SECUENCIA POR TURNOS RESUELTA! — {puzzleDisplayTitle} completado.", 4.5f);
            }
        }

        public override void ResetPuzzle()
        {
            if (IsSolved) return;
            currentStepProgress = 0;
            DispatchCoopSubStateToNetwork("TURN_STEP|0");
        }

        public override void ApplyNetworkCommand(string payload)
        {
            if (string.IsNullOrEmpty(payload)) return;
            if (payload == "SOLVED")
            {
                currentStepProgress = TotalSteps;
                MarkTurnSequenceSolved(false);
                return;
            }

            string[] parts = payload.Split('|');
            if (parts.Length >= 2 && parts[0] == "TURN_STEP" && int.TryParse(parts[1], out int step))
            {
                currentStepProgress = Mathf.Clamp(step, 0, TotalSteps);
                if (currentStepProgress >= TotalSteps && TotalSteps > 0)
                {
                    MarkTurnSequenceSolved(false);
                }
            }
        }

        public override string GetNetworkSnapshotPayload()
        {
            return IsSolved ? "SOLVED" : $"TURN_STEP|{currentStepProgress}";
        }
    }
}
