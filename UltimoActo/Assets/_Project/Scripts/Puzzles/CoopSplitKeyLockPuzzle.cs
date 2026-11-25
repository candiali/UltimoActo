using System;
using UnityEngine;
using UltimoActo.Core;
using UltimoActo.Interaction;

namespace UltimoActo.Puzzles
{
    /// <summary>
    /// BLOQUE B · Tipos 1 y 4 — Puzle Cooperativo de Clave Dividida (Dos Mitades entre 1929 y Ruinas):
    /// - La mitad A de la combinación solo es visible en la Época 1929.
    /// - La mitad B de la combinación solo es visible en la Época Ruinas.
    /// - Ambos jugadores deben comunicarse por voz (o alternar con [Q] en Modo Solo) para introducir
    ///   las dos mitades en la cerradura/cilindro compartido.
    /// - Desbloquea el logro cooperativo "Voz en la oscuridad" y registra la solución en el Diario [J].
    /// </summary>
    public class CoopSplitKeyLockPuzzle : CoopPuzzleBase
    {
        [Header("Símbolos / Valores de la Clave Dividida")]
        [SerializeField] private string[] symbolOptions =
        {
            "I — Cisne Dorado (1929)",
            "II — Rosa de Marfil",
            "III — Lira de Bronce (Ruinas)",
            "IV — Máscara de Ópera"
        };

        [SerializeField] private int currentLeftDial = 1;  // Arranca en II (incorrecto)
        [SerializeField] private int currentRightDial = 3; // Arranca en IV (incorrecto)
        [SerializeField] private int targetLeftDial = 0;   // Mitad A (1929): I — Cisne Dorado
        [SerializeField] private int targetRightDial = 2;  // Mitad B (Ruinas): III — Lira de Bronce

        [Header("Referencias Visuales de los Diales")]
        [SerializeField] private Transform leftDialVisual;
        [SerializeField] private Transform rightDialVisual;
        [SerializeField] private Light statusIndicatorLight;

        private AudioSource audioSource;

        public int CurrentLeftDial => currentLeftDial;
        public int CurrentRightDial => currentRightDial;

        public void ConfigureSplitLock(
            string id,
            string displayTitle,
            string[] options,
            int initialLeft,
            int initialRight,
            int correctLeft,
            int correctRight,
            Transform leftVisual,
            Transform rightVisual,
            Light indicatorLight,
            WorldDoor singleDoor,
            WorldDoubleDoor doubleDoor)
        {
            puzzleId = id;
            puzzleDisplayTitle = displayTitle;
            cooperationType = CoopCooperationType.SplitKeyTwoHalves;
            if (options != null && options.Length >= 2) symbolOptions = options;

            currentLeftDial = Mathf.Clamp(initialLeft, 0, symbolOptions.Length - 1);
            currentRightDial = Mathf.Clamp(initialRight, 0, symbolOptions.Length - 1);
            targetLeftDial = Mathf.Clamp(correctLeft, 0, symbolOptions.Length - 1);
            targetRightDial = Mathf.Clamp(correctRight, 0, symbolOptions.Length - 1);

            leftDialVisual = leftVisual;
            rightDialVisual = rightVisual;
            statusIndicatorLight = indicatorLight;
            doorToUnlock = singleDoor;
            doubleDoorToUnlock = doubleDoor;

            achievementId = "voz_en_la_oscuridad";
            achievementTitle = "Voz en la oscuridad";
            achievementDescription =
                "Resolvisteis un mecanismo de clave dividida comunicando las dos mitades de la combinación entre la Época 1929 y las Ruinas.";

            UpdateDialVisuals();
        }

        private void Awake()
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0.85f;
            }
            UpdateDialVisuals();
        }

        public string GetSymbolLabel(bool isRightDial)
        {
            int idx = isRightDial ? currentRightDial : currentLeftDial;
            if (symbolOptions == null || symbolOptions.Length == 0) return idx.ToString();
            return symbolOptions[Mathf.Clamp(idx, 0, symbolOptions.Length - 1)];
        }

        public void RotateHalfDial(bool isRightDial, EraVisibilityMode dialEraRestriction)
        {
            if (IsSolved) return;

            if (!CanInteractInEra(dialEraRestriction, out string deniedMsg))
            {
                if (NightManager.Instance != null)
                {
                    NightManager.Instance.ShowToast(deniedMsg, 3.5f);
                }
                return;
            }

            if (isRightDial)
            {
                currentRightDial = (currentRightDial + 1) % symbolOptions.Length;
            }
            else
            {
                currentLeftDial = (currentLeftDial + 1) % symbolOptions.Length;
            }

            PlayClickTone(isRightDial ? 1.15f : 0.95f);
            UpdateDialVisuals();

            DispatchCoopSubStateToNetwork($"DIAL|{currentLeftDial}|{currentRightDial}");
            EvaluateCombination(true);
        }

        private void EvaluateCombination(bool isLocalAction)
        {
            if (IsSolved) return;

            if (currentLeftDial == targetLeftDial && currentRightDial == targetRightDial)
            {
                PlayClickTone(1.45f);
                UpdateDialVisuals();

                string leftName = GetSymbolLabel(false);
                string rightName = GetSymbolLabel(true);
                string summary =
                    $"COMBINACIÓN DE CLAVE DIVIDIDA COMPLETADA ({puzzleDisplayTitle}):\n\n" +
                    $"• **Primera Mitad (Vista en 1929)**: {leftName}\n" +
                    $"• **Segunda Mitad (Vista en Ruinas)**: {rightName}\n\n" +
                    "Al alinear ambas mitades del sello imperial, el cerrojo cooperativo ha liberado el paso.";

                CompleteCoopPuzzle(summary, isLocalAction);

                if (NightManager.Instance != null)
                {
                    NightManager.Instance.ShowToast($"¡CLAVE DIVIDIDA RESUELTA! ({leftName} + {rightName}) — Paso desbloqueado.", 5.0f);
                }
            }
        }

        private void UpdateDialVisuals()
        {
            if (leftDialVisual != null)
            {
                leftDialVisual.localRotation = Quaternion.Euler(0f, 0f, currentLeftDial * (360f / Mathf.Max(1, symbolOptions.Length)));
            }
            if (rightDialVisual != null)
            {
                rightDialVisual.localRotation = Quaternion.Euler(0f, 0f, currentRightDial * (360f / Mathf.Max(1, symbolOptions.Length)));
            }
            if (statusIndicatorLight != null)
            {
                statusIndicatorLight.color = IsSolved
                    ? new Color(0.32f, 0.92f, 0.48f)
                    : new Color(0.92f, 0.68f, 0.28f);
            }
        }

        public override void ResetPuzzle()
        {
            if (IsSolved) return;
            currentLeftDial = 1;
            currentRightDial = 3;
            UpdateDialVisuals();
            DispatchCoopSubStateToNetwork($"DIAL|{currentLeftDial}|{currentRightDial}");
        }

        public override void ApplyNetworkCommand(string payload)
        {
            if (string.IsNullOrEmpty(payload)) return;

            if (payload == "SOLVED")
            {
                currentLeftDial = targetLeftDial;
                currentRightDial = targetRightDial;
                UpdateDialVisuals();
                EvaluateCombination(false);
                return;
            }

            string[] parts = payload.Split('|');
            if (parts.Length >= 3 && parts[0] == "DIAL")
            {
                if (int.TryParse(parts[1], out int l) && int.TryParse(parts[2], out int r))
                {
                    currentLeftDial = Mathf.Clamp(l, 0, symbolOptions.Length - 1);
                    currentRightDial = Mathf.Clamp(r, 0, symbolOptions.Length - 1);
                    PlayClickTone(1.05f);
                    UpdateDialVisuals();
                    EvaluateCombination(false);
                }
            }
        }

        public override string GetNetworkSnapshotPayload()
        {
            if (IsSolved) return "SOLVED";
            return $"DIAL|{currentLeftDial}|{currentRightDial}";
        }

        private void PlayClickTone(float pitch)
        {
            if (audioSource == null) return;
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(GetOrCreateClickClip());
        }

        private static AudioClip cachedClip;
        private static AudioClip GetOrCreateClickClip()
        {
            if (cachedClip != null) return cachedClip;
            int samples = 2400;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                data[i] = Mathf.Sin(2f * Mathf.PI * 1650f * t) * Mathf.Exp(-t * 32f) * 0.55f;
            }
            cachedClip = AudioClip.Create("CoopSplitLockClick", samples, 1, 44100, false);
            cachedClip.SetData(data, 0);
            return cachedClip;
        }
    }
}
