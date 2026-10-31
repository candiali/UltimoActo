using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UltimoActo.Interaction;

namespace UltimoActo.UI
{
    public class InteractionUI : MonoBehaviour
    {
        [Header("Referencias de Jugador")]
        [SerializeField] private PlayerInteraction playerInteraction;

        [Header("Elementos de UI")]
        [Tooltip("Texto que muestra la acción (ej: '[E] Abrir puerta')")]
        [SerializeField] private TextMeshProUGUI promptTMP;
        [SerializeField] private Text promptLegacyText;

        [Tooltip("Icono o punto central de la retícula")]
        [SerializeField] private Image crosshairImage;

        [Header("Estilo Visual")]
        [SerializeField] private Color normalCrosshairColor = new Color(1f, 1f, 1f, 0.4f);
        [SerializeField] private Color activeCrosshairColor = new Color(1f, 0.85f, 0.3f, 0.9f);

        private void Start()
        {
            if (playerInteraction == null)
            {
                playerInteraction = FindFirstObjectByType<PlayerInteraction>();
            }

            if (playerInteraction != null)
            {
                playerInteraction.OnFocusInteractable += ShowPrompt;
                playerInteraction.OnLostFocusInteractable += HidePrompt;
            }

            HidePrompt();
        }

        private void OnDestroy()
        {
            if (playerInteraction != null)
            {
                playerInteraction.OnFocusInteractable -= ShowPrompt;
                playerInteraction.OnLostFocusInteractable -= HidePrompt;
            }
        }

        private void ShowPrompt(IInteractable interactable)
        {
            if (interactable == null)
            {
                HidePrompt();
                return;
            }

            SetPromptText(interactable.Prompt);

            if (crosshairImage != null)
            {
                crosshairImage.color = activeCrosshairColor;
                crosshairImage.transform.localScale = Vector3.one * 1.3f;
            }
        }

        private void HidePrompt()
        {
            SetPromptText(string.Empty);

            if (crosshairImage != null)
            {
                crosshairImage.color = normalCrosshairColor;
                crosshairImage.transform.localScale = Vector3.one;
            }
        }

        private void SetPromptText(string text)
        {
            if (promptTMP != null)
            {
                promptTMP.text = text;
                promptTMP.gameObject.SetActive(!string.IsNullOrEmpty(text));
            }
            else if (promptLegacyText != null)
            {
                promptLegacyText.text = text;
                promptLegacyText.gameObject.SetActive(!string.IsNullOrEmpty(text));
            }
        }
    }
}
