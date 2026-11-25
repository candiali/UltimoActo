using System.Collections;
using UnityEngine;
using UltimoActo.Core;
using UltimoActo.Interaction;

namespace UltimoActo.Puzzles
{
    /// <summary>
    /// BLOQUE B · Tipo 5 — Puzle Cooperativo de Efecto Espejo entre Épocas:
    /// - Una acción realizada en la Época 1929 (ej: desplazar un cortinaje pesado, asegurar un soporte
    ///   o mover un baúl antes del abandono) provoca una consecuencia directa en la Época Ruinas
    ///   (desaparece un bloqueo de escombros o se descubre un compartimento accesible en las Ruinas).
    /// - Desbloquea el logro cooperativo "Dos tiempos".
    /// </summary>
    public class CoopCrossEraMirrorPuzzle : CoopPuzzleBase, IInteractable
    {
        [Header("Efecto Espejo 1929 -> Ruinas")]
        [SerializeField] private EraVisibilityMode triggerEra = EraVisibilityMode.Only1929;
        [SerializeField] private string actionPromptText = "Descorrer cortinaje / Asegurar estructura en 1929";
        [SerializeField] private Transform objectToMoveIn1929;
        [SerializeField] private Vector3 moveDeltaIn1929 = new Vector3(1.4f, 0f, 0f);
        [SerializeField] private GameObject[] obstaclesClearedInRuins;
        [SerializeField] private GameObject[] objectsRevealedInRuins;

        private Vector3 initialPos1929;

        public string Id => $"coop_mirror_{puzzleId}";

        public void ConfigureMirrorEffect(
            string id,
            string displayTitle,
            EraVisibilityMode actionEra,
            string promptCaption,
            Transform movingObj1929,
            Vector3 delta1929,
            GameObject[] clearedInRuins,
            GameObject[] revealedInRuins,
            WorldDoor singleDoor = null)
        {
            puzzleId = id;
            puzzleDisplayTitle = displayTitle;
            cooperationType = CoopCooperationType.CrossEraMirrorEffect;
            triggerEra = actionEra;
            actionPromptText = promptCaption;
            objectToMoveIn1929 = movingObj1929;
            moveDeltaIn1929 = delta1929;
            obstaclesClearedInRuins = clearedInRuins;
            objectsRevealedInRuins = revealedInRuins;
            doorToUnlock = singleDoor;

            if (objectToMoveIn1929 != null)
            {
                initialPos1929 = objectToMoveIn1929.localPosition;
            }

            if (objectsRevealedInRuins != null)
            {
                for (int i = 0; i < objectsRevealedInRuins.Length; i++)
                {
                    if (objectsRevealedInRuins[i] != null) objectsRevealedInRuins[i].SetActive(false);
                }
            }

            achievementId = "dos_tiempos";
            achievementTitle = "Dos tiempos";
            achievementDescription =
                "Alterasteis un elemento en 1929 para despejar el camino o revelar un secreto décadas después en las Ruinas.";
        }

        public string Prompt
        {
            get
            {
                if (IsSolved) return $"{puzzleDisplayTitle} [Efecto espejo aplicado en las Ruinas]";
                string eraTag = triggerEra == EraVisibilityMode.Only1929 ? "[Acción en 1929]" : "[Acción en Ruinas]";
                return $"[E] {actionPromptText} {eraTag}";
            }
        }

        public bool CanInteract(GameObject player)
        {
            return !IsSolved;
        }

        public void Interact(GameObject player)
        {
            if (IsSolved) return;

            if (!CanInteractInEra(triggerEra, out string deniedMsg))
            {
                if (NightManager.Instance != null) NightManager.Instance.ShowToast(deniedMsg, 3.2f);
                return;
            }

            ApplyMirrorTransformation(true);
        }

        private void ApplyMirrorTransformation(bool isLocalAction)
        {
            if (IsSolved) return;

            if (objectToMoveIn1929 != null)
            {
                StartCoroutine(AnimateMoveRoutine(objectToMoveIn1929, initialPos1929 + moveDeltaIn1929));
            }

            if (obstaclesClearedInRuins != null)
            {
                for (int i = 0; i < obstaclesClearedInRuins.Length; i++)
                {
                    if (obstaclesClearedInRuins[i] != null) obstaclesClearedInRuins[i].SetActive(false);
                }
            }

            if (objectsRevealedInRuins != null)
            {
                for (int i = 0; i < objectsRevealedInRuins.Length; i++)
                {
                    if (objectsRevealedInRuins[i] != null) objectsRevealedInRuins[i].SetActive(true);
                }
            }

            string summary =
                $"EFECTO ESPEJO ENTRE ÉPOCAS ({puzzleDisplayTitle}):\n\n" +
                "El cambio realizado en la Época 1929 ha modificado el estado futuro de la sala en las Ruinas, despejando el acceso para el segundo investigador.";

            CompleteCoopPuzzle(summary, isLocalAction);

            if (NightManager.Instance != null)
            {
                NightManager.Instance.ShowToast($"¡EFECTO ESPEJO ACTIVADO! — El cambio en 1929 ha despejado el paso en las Ruinas.", 4.8f);
            }
        }

        private static IEnumerator AnimateMoveRoutine(Transform target, Vector3 endLocalPos)
        {
            if (target == null) yield break;
            Vector3 start = target.localPosition;
            float elapsed = 0f;
            float dur = 0.85f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                target.localPosition = Vector3.Lerp(start, endLocalPos, Mathf.SmoothStep(0f, 1f, elapsed / dur));
                yield return null;
            }
            target.localPosition = endLocalPos;
        }

        public override void ResetPuzzle()
        {
            if (IsSolved) return;
            if (objectToMoveIn1929 != null) objectToMoveIn1929.localPosition = initialPos1929;
        }

        public override void ApplyNetworkCommand(string payload)
        {
            if (payload == "SOLVED")
            {
                ApplyMirrorTransformation(false);
            }
        }

        public override string GetNetworkSnapshotPayload()
        {
            return IsSolved ? "SOLVED" : "ACTIVE";
        }
    }
}
