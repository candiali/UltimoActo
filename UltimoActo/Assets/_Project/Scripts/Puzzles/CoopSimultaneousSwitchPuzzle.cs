using UnityEngine;
using UltimoActo.Core;
using UltimoActo.Interaction;

namespace UltimoActo.Puzzles
{
    /// <summary>
    /// BLOQUE B · Tipo 2 — Puzle Cooperativo de Acción Simultánea:
    /// - Dos interruptores/palancas en lugares o épocas distintas (uno en 1929 y otro en Ruinas)
    ///   que deben mantenerse accionados a la vez dentro de una ventana de sincronía con tolerancia
    ///   por latencia validada por el Host.
    /// - MODO SOLO: Cuando el jugador está solo (IsSoloFallbackActive), al accionar la primera palanca
    ///   se activa un "Eco-Retenedor Temporal" (temporizador generoso de 14 segundos) para que pueda
    ///   cambiar de época con [Q] y accionar la segunda palanca sin romper la dificultad.
    /// - Desbloquea el logro cooperativo "Sincronía".
    /// </summary>
    public class CoopSimultaneousSwitchPuzzle : CoopPuzzleBase
    {
        [Header("Ventanas de Sincronía")]
        [SerializeField] private float coopSyncWindowSeconds = 4.5f;
        [SerializeField] private float soloEchoLatchSeconds = 14.0f;

        private float timerSwitch1929;
        private float timerSwitchRuins;

        public bool IsSwitch1929Latched => timerSwitch1929 > 0f || IsSolved;
        public bool IsSwitchRuinsLatched => timerSwitchRuins > 0f || IsSolved;
        public float Remaining1929 => Mathf.Max(0f, timerSwitch1929);
        public float RemainingRuins => Mathf.Max(0f, timerSwitchRuins);

        public void ConfigureSimultaneous(
            string id,
            string displayTitle,
            float coopWindow,
            float soloWindow,
            WorldDoor singleDoor = null,
            WorldDoubleDoor doubleDoor = null)
        {
            puzzleId = id;
            puzzleDisplayTitle = displayTitle;
            cooperationType = CoopCooperationType.SimultaneousAction;
            coopSyncWindowSeconds = Mathf.Max(2.5f, coopWindow);
            soloEchoLatchSeconds = Mathf.Max(10f, soloWindow);
            doorToUnlock = singleDoor;
            doubleDoorToUnlock = doubleDoor;

            achievementId = "sincronia";
            achievementTitle = "Sincronía";
            achievementDescription =
                "Accionasteis al unísono un mecanismo de sincronía temporal entre 1929 y las Ruinas.";
        }

        private void Update()
        {
            if (IsSolved) return;

            bool had1929 = timerSwitch1929 > 0f;
            bool hadRuins = timerSwitchRuins > 0f;

            if (timerSwitch1929 > 0f) timerSwitch1929 -= Time.deltaTime;
            if (timerSwitchRuins > 0f) timerSwitchRuins -= Time.deltaTime;

            if ((had1929 && timerSwitch1929 <= 0f) || (hadRuins && timerSwitchRuins <= 0f))
            {
                if (NightManager.Instance != null)
                {
                    NightManager.Instance.ShowToast($"[{puzzleDisplayTitle}] El retén temporal se ha liberado antes de completar la sincronía.", 3.0f);
                }
            }
        }

        public void TriggerSwitch(bool isRuinsSwitch)
        {
            if (IsSolved) return;

            EraVisibilityMode req = isRuinsSwitch ? EraVisibilityMode.OnlyRuins : EraVisibilityMode.Only1929;
            if (!CanInteractInEra(req, out string deniedMsg))
            {
                if (NightManager.Instance != null) NightManager.Instance.ShowToast(deniedMsg, 3.2f);
                return;
            }

            float holdDuration = IsSoloFallbackActive ? soloEchoLatchSeconds : coopSyncWindowSeconds;
            if (isRuinsSwitch)
            {
                timerSwitchRuins = holdDuration;
            }
            else
            {
                timerSwitch1929 = holdDuration;
            }

            DispatchCoopSubStateToNetwork($"SIM_TRIG|{(isRuinsSwitch ? 1 : 0)}|{holdDuration:0.0}");

            if (timerSwitch1929 > 0f && timerSwitchRuins > 0f)
            {
                ResolveSimultaneousSuccess(true);
            }
            else if (NightManager.Instance != null)
            {
                string otherEra = isRuinsSwitch ? "1929" : "Ruinas";
                string hint = IsSoloFallbackActive
                    ? $"Eco-retenedor activo durante {holdDuration:0} s — Pulsa [Q] y acciona la palanca de {otherEra}."
                    : $"Manteniendo contacto ({holdDuration:0.0} s) — ¡Que tu compañero accione la palanca de {otherEra} ahora!";
                NightManager.Instance.ShowToast(hint, 3.8f);
            }
        }

        private void ResolveSimultaneousSuccess(bool isLocalAction)
        {
            if (IsSolved) return;

            string summary =
                $"SINCRONÍA TEMPORAL COMPLETADA ({puzzleDisplayTitle}):\n\n" +
                "Ambos contactos (el interruptor de 1929 y el interruptor de las Ruinas) fueron accionados dentro de la misma ventana de resonancia, liberando el enclavamiento.";

            CompleteCoopPuzzle(summary, isLocalAction);

            if (NightManager.Instance != null)
            {
                NightManager.Instance.ShowToast($"¡SINCRONÍA PERFECTA! — {puzzleDisplayTitle} desbloqueado.", 4.5f);
            }
        }

        public override void ResetPuzzle()
        {
            if (IsSolved) return;
            timerSwitch1929 = 0f;
            timerSwitchRuins = 0f;
        }

        public override void ApplyNetworkCommand(string payload)
        {
            if (string.IsNullOrEmpty(payload)) return;
            if (payload == "SOLVED")
            {
                ResolveSimultaneousSuccess(false);
                return;
            }

            string[] parts = payload.Split('|');
            if (parts.Length >= 3 && parts[0] == "SIM_TRIG")
            {
                bool isRuins = parts[1] == "1";
                if (!float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float dur))
                {
                    dur = coopSyncWindowSeconds;
                }
                if (isRuins) timerSwitchRuins = dur;
                else timerSwitch1929 = dur;

                if (timerSwitch1929 > 0f && timerSwitchRuins > 0f)
                {
                    ResolveSimultaneousSuccess(false);
                }
            }
        }

        public override string GetNetworkSnapshotPayload()
        {
            return IsSolved ? "SOLVED" : "ACTIVE";
        }
    }
}
