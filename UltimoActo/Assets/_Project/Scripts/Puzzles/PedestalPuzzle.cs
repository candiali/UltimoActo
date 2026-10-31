using UnityEngine;

namespace UltimoActo.Puzzles
{
    /// <summary>
    /// Puzle mecánico de símbolos/estatuas de la Academia.
    /// Comprueba la combinación de pasos en los pedestales giratorios.
    /// Al resolverse, desbloquea una puerta o mecanismo teatral.
    /// </summary>
    public class PedestalPuzzle : PuzzleBase
    {
        [Header("Pedestales y Combinación")]
        [Tooltip("Lista de pedestales involucrados en el puzle")]
        [SerializeField] private PedestalDial[] dials;

        [Tooltip("Secuencia de pasos objetivo (0=Cisne Blanco, 1=Telón, 2=Máscara, 3=Cisne Negro)")]
        [SerializeField] private int[] targetCombination = new int[] { 0, 3, 1 };

        [Header("Feedback Visual")]
        [SerializeField] private Light feedbackLight;
        [SerializeField] private Color unsolvedColor = new Color(0.8f, 0.2f, 0.1f);
        [SerializeField] private Color solvedColor = new Color(0.2f, 0.9f, 0.4f);

        private void Start()
        {
            if (feedbackLight != null)
            {
                feedbackLight.color = unsolvedColor;
            }

            // Suscribirse a los cambios de cada pedestal
            if (dials != null)
            {
                foreach (var dial in dials)
                {
                    if (dial != null)
                    {
                        dial.OnStepChanged += OnDialChanged;
                    }
                }
            }

            CheckCombination();
        }

        private void OnDestroy()
        {
            if (dials != null)
            {
                foreach (var dial in dials)
                {
                    if (dial != null)
                    {
                        dial.OnStepChanged -= OnDialChanged;
                    }
                }
            }
        }

        private void OnDialChanged(PedestalDial dial, int step)
        {
            if (state == PuzzleState.Solved) return;
            CheckCombination();
        }

        public void CheckCombination()
        {
            if (dials == null || targetCombination == null || dials.Length != targetCombination.Length)
            {
                return;
            }

            bool matches = true;
            for (int i = 0; i < dials.Length; i++)
            {
                if (dials[i] == null || dials[i].CurrentStep != targetCombination[i])
                {
                    matches = false;
                    break;
                }
            }

            if (matches)
            {
                Solve();
            }
        }

        public override void Solve()
        {
            base.Solve();

            if (feedbackLight != null)
            {
                feedbackLight.color = solvedColor;
                feedbackLight.intensity = 2.5f;
            }
        }

        public override void ResetPuzzle()
        {
            state = PuzzleState.Active;
            Debug.Log($"[Puzle] Reiniciando mecanismo '{puzzleId}'...");

            if (dials != null)
            {
                foreach (var dial in dials)
                {
                    if (dial != null)
                    {
                        dial.SetStep(0);
                    }
                }
            }

            if (feedbackLight != null)
            {
                feedbackLight.color = unsolvedColor;
                feedbackLight.intensity = 1f;
            }

            onReset?.Invoke();
        }
    }
}
