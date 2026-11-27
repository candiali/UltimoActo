using UnityEngine;
using UltimoActo.Core;
using UltimoActo.Interaction;
using UltimoActo.UI;

namespace UltimoActo.Narrative
{
    /// <summary>
    /// BLOQUE 2 — Objeto interactivo en el mundo para un Coleccionable Narrativo o un Nodo de Secreto Especial.
    /// - Respeta la época asignada (EraVisibilityMode: Only1929, OnlyRuins o Both).
    /// - Si es un coleccionable estándar, al pulsar [E] lo registra en CollectibleAndSecretManager
    ///   (sincronizado con autoridad del Host), muestra su historia secundaria en el visor de documentos
    ///   (DocumentUI) y oculta la pieza física para evitar duplicados.
    /// - Si es un Secreto Especial Cooperativo de 2 pasos (1929 -> Ruinas), el nodo de Paso 1 (1929)
    ///   revela la pista de voz y habilita el nodo de Paso 2 (Ruinas) para desbloquear el recuerdo completo.
    /// </summary>
    public class WorldCollectibleItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private string collectibleId = "col_n1_ribbon_elena_1929";
        [SerializeField] private EraVisibilityMode eraVisibility = EraVisibilityMode.Both;
        [SerializeField] private bool isSecretStepOneNode = false;
        [SerializeField] private bool requiresSecretStepOneFirst = false;
        [SerializeField] private string stepOnePromptTitle = "Marca Oculta (1929)";
        [SerializeField] private string stepOneVoiceClueTitle = "Susurro Grabado en 1929";
        [TextArea(3, 6)]
        [SerializeField] private string stepOneVoiceClueBody = "";

        private bool stepOneInspectedLocally;

        public string CollectibleId => collectibleId;
        public string Id => isSecretStepOneNode ? $"secret_step1_{collectibleId}" : $"collectible_{collectibleId}";

        public void ConfigureCollectible(
            string id,
            EraVisibilityMode visibilityMode,
            bool secretStep1 = false,
            bool needsStep1First = false,
            string step1Prompt = "",
            string step1DocTitle = "",
            string step1DocBody = "")
        {
            collectibleId = id;
            eraVisibility = visibilityMode;
            isSecretStepOneNode = secretStep1;
            requiresSecretStepOneFirst = needsStep1First;
            stepOnePromptTitle = step1Prompt;
            stepOneVoiceClueTitle = step1DocTitle;
            stepOneVoiceClueBody = step1DocBody;

            if (visibilityMode != EraVisibilityMode.Both)
            {
                EraOnlyVisibility eov = GetComponent<EraOnlyVisibility>();
                if (eov == null) eov = gameObject.AddComponent<EraOnlyVisibility>();
                eov.Configure(visibilityMode, true);
            }
        }

        private bool IsVisibleInCurrentLocalEra()
        {
            if (eraVisibility == EraVisibilityMode.Both) return true;
            EraVisual current = EraVisualManager.Instance != null ? EraVisualManager.Instance.CurrentEra : EraVisual.Glory1929;
            if (eraVisibility == EraVisibilityMode.Only1929) return current == EraVisual.Glory1929;
            if (eraVisibility == EraVisibilityMode.OnlyRuins) return current == EraVisual.Ruins;
            return true;
        }

        public string Prompt
        {
            get
            {
                if (!IsVisibleInCurrentLocalEra()) return string.Empty;
                if (CollectibleAndSecretManager.Instance == null) return string.Empty;

                if (CollectibleAndSecretManager.Instance.IsUnlocked(collectibleId))
                {
                    return string.Empty;
                }

                string eraTag = eraVisibility == EraVisibilityMode.Only1929
                    ? "[Época 1929]"
                    : (eraVisibility == EraVisibilityMode.OnlyRuins ? "[Época Ruinas]" : "");

                if (isSecretStepOneNode)
                {
                    bool step1Done = stepOneInspectedLocally || CollectibleAndSecretManager.Instance.IsSecretStepOneDone(collectibleId);
                    return step1Done
                        ? $"[E] Releer {stepOnePromptTitle} {eraTag} (Pista enviada a Ruinas)"
                        : $"[E] Inspeccionar {stepOnePromptTitle} {eraTag}";
                }

                CollectibleDefinition def = CollectibleAndSecretManager.Instance.GetDefinition(collectibleId);
                if (def == null) return $"[E] Recoger Coleccionable {eraTag}";

                if (def.isSpecialSecret)
                {
                    if (requiresSecretStepOneFirst && !CollectibleAndSecretManager.Instance.IsSecretStepOneDone(collectibleId))
                    {
                        return $"[E] Examinar {def.title} {eraTag} (Requiere pista previa de 1929)";
                    }
                    return $"[E] Descubrir Secreto Especial: {def.title} {eraTag}";
                }

                return $"[E] Recoger Coleccionable ({CollectibleAndSecretManager.FormatTypeShort(def.type)}): {def.title} {eraTag}";
            }
        }

        public bool CanInteract(GameObject player)
        {
            if (!IsVisibleInCurrentLocalEra()) return false;
            if (CollectibleAndSecretManager.Instance == null) return false;
            return !CollectibleAndSecretManager.Instance.IsUnlocked(collectibleId);
        }

        public void Interact(GameObject player)
        {
            if (!CanInteract(player)) return;
            if (CollectibleAndSecretManager.Instance == null) return;

            if (isSecretStepOneNode)
            {
                stepOneInspectedLocally = true;
                CollectibleAndSecretManager.Instance.MarkSecretStepOneDone(collectibleId, true);

                if (DocumentUI.Instance != null)
                {
                    DocumentUI.Instance.ShowDocumentWithId(
                        $"sec_step1_{collectibleId}",
                        stepOneVoiceClueTitle,
                        stepOneVoiceClueBody,
                        "Paso 1 de Secreto Cooperativo [SOLO VISIBLE EN 1929] — Comunícaselo por voz a tu compañero en las Ruinas");
                }
                return;
            }

            if (requiresSecretStepOneFirst && !CollectibleAndSecretManager.Instance.IsSecretStepOneDone(collectibleId))
            {
                bool isSolo = LanSessionManager.Instance == null || LanSessionManager.Instance.ConnectedPlayerCount < 2;
                string hint = isSolo
                    ? "El resorte está trabado desde dentro. Pulsa [Q] para mirar este mismo punto en 1929 y encontrar dónde presionar."
                    : "El resorte oculto está bloqueado. Pídele por voz al jugador de 1929 que examine primero este mismo lugar en su época.";
                if (NightManager.Instance != null)
                {
                    NightManager.Instance.ShowToast(hint, 4.2f);
                }
                return;
            }

            CollectibleAndSecretManager.Instance.UnlockCollectibleOrSecret(collectibleId, true, true);
        }
    }
}
