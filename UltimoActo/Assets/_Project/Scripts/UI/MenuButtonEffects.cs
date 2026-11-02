using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace UltimoActo.UI
{
    /// <summary>
    /// Añade vida, dinamismo y respuesta táctil a los botones del menú:
    /// escala suave (pop/zoom), resplandor de oro envejecido y vino,
    /// y micro-animación en hover y clic.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class MenuButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Animación de Escala")]
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float pressedScale = 0.98f;
        [SerializeField] private float animationSpeed = 12f;

        [Header("Colores y Resplandor")]
        [SerializeField] private Image buttonBackground;
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private Image goldAccentBorder;
        [SerializeField] private GameObject hoverIndicatorWings; // Decoradores laterales opcionales [ ▸ ◂ ]

        [Header("Paleta de Color")]
        [SerializeField] private Color normalBgColor = new Color(0.08f, 0.03f, 0.05f, 0.90f);
        [SerializeField] private Color hoverBgColor = new Color(0.36f, 0.08f, 0.15f, 0.96f); // Vino terciopelo
        [SerializeField] private Color normalTextColor = new Color(0.96f, 0.94f, 0.90f);     // Marfil
        [SerializeField] private Color hoverTextColor = new Color(1.0f, 0.92f, 0.68f);       // Oro brillante
        [SerializeField] private Color normalBorderColor = new Color(0.77f, 0.63f, 0.38f, 0.45f);
        [SerializeField] private Color hoverBorderColor = new Color(1.0f, 0.85f, 0.45f, 0.95f);

        private Vector3 baseScale;
        private Vector3 targetScale;
        private Coroutine animCoroutine;
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            baseScale = transform.localScale;
            targetScale = baseScale;

            if (buttonBackground == null) buttonBackground = GetComponent<Image>();
            if (labelText == null) labelText = GetComponentInChildren<TextMeshProUGUI>();

            ApplyVisualState(false, false);
        }

        private void OnEnable()
        {
            targetScale = baseScale;
            transform.localScale = baseScale;
            ApplyVisualState(false, false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;

            targetScale = baseScale * hoverScale;
            StartAnimation();
            ApplyVisualState(true, false);

            var menu = GetComponentInParent<MainMenuController>();
            if (menu != null) menu.PlayHoverSound();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            targetScale = baseScale;
            StartAnimation();
            ApplyVisualState(false, false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;

            targetScale = baseScale * pressedScale;
            StartAnimation();
            ApplyVisualState(true, true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            targetScale = baseScale;
            StartAnimation();
            ApplyVisualState(false, false);
        }

        private void StartAnimation()
        {
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(AnimateScale());
        }

        private IEnumerator AnimateScale()
        {
            while (Vector3.Distance(transform.localScale, targetScale) > 0.002f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);
                yield return null;
            }
            transform.localScale = targetScale;
            animCoroutine = null;
        }

        private void ApplyVisualState(bool isHover, bool isPressed)
        {
            if (buttonBackground != null)
            {
                buttonBackground.color = isHover ? hoverBgColor : normalBgColor;
            }

            if (labelText != null)
            {
                labelText.color = isHover ? hoverTextColor : normalTextColor;
            }

            if (goldAccentBorder != null)
            {
                goldAccentBorder.color = isHover ? hoverBorderColor : normalBorderColor;
            }

            if (hoverIndicatorWings != null)
            {
                hoverIndicatorWings.SetActive(isHover);
            }
        }
    }
}
