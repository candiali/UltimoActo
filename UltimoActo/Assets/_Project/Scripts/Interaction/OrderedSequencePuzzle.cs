using System;
using System.Collections;
using UnityEngine;
using UltimoActo.Core;

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

    /// <summary>
    /// Puzzle genérico de secuencia ordenada. Si se pulsa un nodo incorrecto se reinicia.
    /// Al completarse: activa objetos de recompensa y registra evidencia en NightManager.
    /// Expone OnStateChanged(progreso) listo para enlazar con una NetworkVariable en la fase cooperativa.
    /// </summary>
    public class OrderedSequencePuzzle : MonoBehaviour
    {
        [SerializeField] private string puzzleId = "puzzle";
        [SerializeField] private int[] correctOrder = new int[0];
        [SerializeField] private GameObject[] rewardObjects = new GameObject[0];
        [SerializeField] private string solvedMessage = "Mecanismo resuelto.";
        [SerializeField] private string failMessage = "Orden incorrecto — el mecanismo se reinicia.";

        public event Action<int> OnStateChanged;

        private int progress;
        private bool solved;
        private AudioSource audioSource;

        public bool IsSolved => solved;
        public int Progress => progress;

        private void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.8f;
        }

        public void Configure(string id, int[] order, GameObject[] rewards, string okMsg, string badMsg)
        {
            puzzleId = id; correctOrder = order; rewardObjects = rewards;
            solvedMessage = okMsg; failMessage = badMsg;
            foreach (var r in rewardObjects) if (r != null) r.SetActive(false);
        }

        public void Press(int nodeIndex, Transform node)
        {
            if (solved) return;
            if (nodeIndex == correctOrder[progress])
            {
                progress++;
                Beep(1.0f + progress * 0.15f);
                StartCoroutine(Nudge(node));
                OnStateChanged?.Invoke(progress);
                if (progress >= correctOrder.Length) Solve();
            }
            else
            {
                progress = 0;
                Beep(0.55f);
                OnStateChanged?.Invoke(0);
                if (NightManager.Instance != null) NightManager.Instance.ShowToast(failMessage, 3.0f);
            }
        }

        private void Solve()
        {
            solved = true;
            foreach (var r in rewardObjects) if (r != null) r.SetActive(true);
            if (NightManager.Instance != null)
            {
                NightManager.Instance.RegisterEvidence("puzzle_" + puzzleId);
                NightManager.Instance.ShowToast(solvedMessage, 5.0f);
            }
        }

        private IEnumerator Nudge(Transform t)
        {
            if (t == null) yield break;
            Vector3 start = t.localPosition;
            Vector3 down = start + new Vector3(0f, -0.03f, 0f);
            float e = 0f;
            while (e < 0.12f) { e += Time.deltaTime; t.localPosition = Vector3.Lerp(start, down, e / 0.12f); yield return null; }
            e = 0f;
            while (e < 0.12f) { e += Time.deltaTime; t.localPosition = Vector3.Lerp(down, start, e / 0.12f); yield return null; }
            t.localPosition = start;
        }

        private void Beep(float pitch)
        {
            if (audioSource == null) return;
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(Clip());
        }

        private static AudioClip cached;
        private static AudioClip Clip()
        {
            if (cached != null) return cached;
            int n = 4000;
            float[] d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                d[i] = Mathf.Sin(2f * Mathf.PI * 1100f * t) * Mathf.Exp(-t * 18f) * 0.6f;
            }
            cached = AudioClip.Create("SeqBeep", n, 1, 44100, false);
            cached.SetData(d, 0);
            return cached;
        }
    }
}
