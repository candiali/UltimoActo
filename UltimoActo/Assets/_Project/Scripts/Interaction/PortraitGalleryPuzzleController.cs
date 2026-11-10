using System;
using System.Collections;
using UnityEngine;
using UltimoActo.Core;

namespace UltimoActo.Interaction
{
    /// <summary>
    /// Componente interactivo colocado en cada una de las 4 placas de latón de la Galería de Retratos.
    /// Permite al jugador girar el dial y seleccionar una de las 4 estaciones de coronación.
    /// </summary>
    public class PortraitPlaqueDial : MonoBehaviour, IInteractable
    {
        [SerializeField] private PortraitGalleryPuzzleController puzzleController;
        [SerializeField] private int portraitIndex = 0;
        [SerializeField] private string primaDonnaName = "Beatrice Dupont (1898)";

        public string Id => $"portrait_dial_{portraitIndex}";
        public string Prompt
        {
            get
            {
                if (puzzleController != null && puzzleController.IsSolved)
                {
                    return $"Placa de {primaDonnaName} [Fijada]";
                }
                string currentSeason = puzzleController != null ? puzzleController.GetSeasonName(portraitIndex) : "I";
                return $"[E] Girar dial de {primaDonnaName} [Actual: {currentSeason}]";
            }
        }

        public bool CanInteract(GameObject player)
        {
            return puzzleController != null && !puzzleController.IsSolved;
        }

        public void Interact(GameObject player)
        {
            if (puzzleController != null)
            {
                puzzleController.RotateDial(portraitIndex);
            }
        }

        public void Configure(PortraitGalleryPuzzleController controller, int index, string dancerName)
        {
            puzzleController = controller;
            portraitIndex = index;
            primaDonnaName = dancerName;
        }
    }

    /// <summary>
    /// Enigma del Pasillo de los Retratos (Zona 10, Noche I).
    /// Cuatro retratos de las primeras bailarinas con diales de bronce.
    /// La pista en la Sala de Espera / Despacho indica el orden de las coronaciones estacionales:
    /// Beatrice: Invierno (IV=3), Clara: Primavera (I=0), Teresa: Otoño (III=2), Elena: Verano (II=1).
    /// Al resolverlo, el retrato de Elena Vesper se abre revelando la Llave de las Aulas de Ensayo.
    /// </summary>
    public class PortraitGalleryPuzzleController : MonoBehaviour
    {
        public static PortraitGalleryPuzzleController Instance { get; private set; }

        public static readonly string[] SeasonNames = new string[]
        {
            "I — Primavera",
            "II — Verano",
            "III — Otoño",
            "IV — Invierno"
        };

        [Header("Estado")]
        [SerializeField] private int[] currentDialIndices = new int[] { 0, 0, 0, 0 };
        // Combinación correcta: Beatrice=Invierno(3), Clara=Primavera(0), Teresa=Otoño(2), Elena=Verano(1)
        [SerializeField] private int[] targetCombination = new int[] { 3, 0, 2, 1 };
        [SerializeField] private bool isSolved = false;

        [Header("Transformaciones Visuales")]
        [SerializeField] private Transform[] dialTransforms;
        [SerializeField] private Transform elenaPortraitHinge;
        [SerializeField] private GameObject secretNicheContents;

        private AudioSource audioSource;
        private Coroutine solveRoutine;

        public bool IsSolved => isSolved;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0.85f;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public string GetSeasonName(int portraitIndex)
        {
            if (portraitIndex < 0 || portraitIndex >= currentDialIndices.Length) return "I";
            int seasonIdx = currentDialIndices[portraitIndex];
            return SeasonNames[Mathf.Clamp(seasonIdx, 0, SeasonNames.Length - 1)];
        }

        public void RotateDial(int portraitIndex)
        {
            if (isSolved || solveRoutine != null) return;
            if (portraitIndex < 0 || portraitIndex >= currentDialIndices.Length) return;

            currentDialIndices[portraitIndex] = (currentDialIndices[portraitIndex] + 1) % 4;
            PlayDialClickSound();

            if (dialTransforms != null && portraitIndex < dialTransforms.Length && dialTransforms[portraitIndex] != null)
            {
                dialTransforms[portraitIndex].localRotation = Quaternion.Euler(0f, 0f, currentDialIndices[portraitIndex] * 90f);
            }

            if (CheckSolution())
            {
                solveRoutine = StartCoroutine(SolveSequence());
            }
        }

        private bool CheckSolution()
        {
            for (int i = 0; i < targetCombination.Length; i++)
            {
                if (currentDialIndices[i] != targetCombination[i]) return false;
            }
            return true;
        }

        private IEnumerator SolveSequence()
        {
            isSolved = true;
            yield return new WaitForSeconds(0.35f);

            // 1. Sonido de mecanismo secreto desatascándose
            PlaySecretLatchSound();
            yield return new WaitForSeconds(0.45f);

            // 2. Elena's portrait swings forward on its hinge
            if (elenaPortraitHinge != null)
            {
                float t = 0f;
                float duration = 1.6f;
                Quaternion startRot = elenaPortraitHinge.localRotation;
                Quaternion targetRot = startRot * Quaternion.Euler(0f, -80f, 0f);

                while (t < duration)
                {
                    t += Time.deltaTime;
                    elenaPortraitHinge.localRotation = Quaternion.Slerp(startRot, targetRot, Mathf.SmoothStep(0f, 1f, t / duration));
                    yield return null;
                }
                elenaPortraitHinge.localRotation = targetRot;
            }

            if (secretNicheContents != null)
            {
                secretNicheContents.SetActive(true);
            }

            // 3. Registrar evidencia y mostrar aviso
            if (NightManager.Instance != null)
            {
                NightManager.Instance.RegisterEvidence("evidencia_enigma_retratos_noche1");
                NightManager.Instance.ShowToast("COMPARTIMENTO SECRETO ABIERTO — El retrato de Elena Vesper ha cedido hacia adelante.", 6.5f);
            }

            solveRoutine = null;
        }

        public void Configure(Transform[] dials, Transform elenaHinge, GameObject nicheContents)
        {
            dialTransforms = dials;
            elenaPortraitHinge = elenaHinge;
            secretNicheContents = nicheContents;
        }

        // ====================================================================
        // AUDIO PROCEDURAL
        // ====================================================================
        private void PlayDialClickSound()
        {
            if (audioSource == null) return;
            audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.15f);
            audioSource.PlayOneShot(GenerateDialClip());
        }

        private void PlaySecretLatchSound()
        {
            if (audioSource == null) return;
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(GenerateLatchClip());
        }

        private static AudioClip cachedDialClip;
        private static AudioClip GenerateDialClip()
        {
            if (cachedDialClip != null) return cachedDialClip;
            int samples = 2200;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                data[i] = Mathf.Sin(2f * Mathf.PI * 3200f * t) * Mathf.Exp(-t * 50f);
            }
            cachedDialClip = AudioClip.Create("DialClick", samples, 1, 44100, false);
            cachedDialClip.SetData(data, 0);
            return cachedDialClip;
        }

        private static AudioClip cachedLatchClip;
        private static AudioClip GenerateLatchClip()
        {
            if (cachedLatchClip != null) return cachedLatchClip;
            int samples = 18000;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float scrape = (Mathf.Sin(2f * Mathf.PI * 220f * t) + Mathf.Sin(2f * Mathf.PI * 440f * t) * 0.5f) * (1f - t);
                data[i] = scrape;
            }
            cachedLatchClip = AudioClip.Create("SecretLatch", samples, 1, 44100, false);
            cachedLatchClip.SetData(data, 0);
            return cachedLatchClip;
        }
    }
}
