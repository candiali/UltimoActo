using UnityEngine;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UltimoActo.Narrative;
using UltimoActo.Player;

namespace UltimoActo.UI
{
    public class DocumentUI : MonoBehaviour
    {
        public static DocumentUI Instance { get; set; }

        [Header("Elementos de UI")]
        [SerializeField] private GameObject documentPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private TextMeshProUGUI closeHintText;

        private bool isOpen = false;
        private float openTime = -1f;
        public float LastClosedTime { get; private set; } = -1f;
        public int LastClosedFrame { get; private set; } = -1;
        private PlayerCameraLook playerCameraLook;
        private PlayerMovement playerMovement;

        public bool IsOpen => isOpen;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            playerCameraLook = FindFirstObjectByType<PlayerCameraLook>();
            playerMovement = FindFirstObjectByType<PlayerMovement>();

            EnsureReferences();

            if (documentPanel != null)
            {
                documentPanel.SetActive(false);
            }
        }

        private void EnsureReferences()
        {
            if (documentPanel == null)
            {
                var child = transform.Find("DocumentViewer_Panel");
                if (child != null) documentPanel = child.gameObject;
            }

            if (documentPanel != null)
            {
                if (titleText == null)
                    titleText = documentPanel.transform.Find("Card/TitleText")?.GetComponent<TextMeshProUGUI>();
                if (bodyText == null)
                    bodyText = documentPanel.transform.Find("Card/ContentText")?.GetComponent<TextMeshProUGUI>();
                if (closeHintText == null)
                    closeHintText = documentPanel.transform.Find("Card/CloseHintText")?.GetComponent<TextMeshProUGUI>();
            }
        }

        private void Update()
        {
            if (!isOpen) return;

            // Cooldown de 0.35s para evitar que la pulsación de [E] que abrió el documento lo cierre en el mismo instante
            if (Time.unscaledTime - openTime < 0.35f) return;

            bool closePressed = false;
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
            {
                closePressed = true;
            }
            else if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                closePressed = true;
            }
#else
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                closePressed = true;
            }
#endif

            if (closePressed)
            {
                CloseDocument();
            }
        }

        public void OpenDocument(WorldDocument doc)
        {
            if (doc == null) return;
            ShowDocument(doc.Title, doc.Content, doc.Subtitle);
        }

        public void ShowDocument(string title, string body, string subtitle = "")
        {
            EnsureReferences();

            if (documentPanel == null)
            {
                Debug.LogWarning("[DocumentUI] No se encontró documentPanel asignado!");
                return;
            }

            isOpen = true;
            openTime = Time.unscaledTime;
            documentPanel.SetActive(true);

            if (titleText != null) titleText.text = title;
            if (subtitleText != null) subtitleText.text = subtitle;
            if (bodyText != null) bodyText.text = body;
            if (closeHintText != null) closeHintText.text = "Presiona [E] o [Esc] para cerrar";

            Debug.Log($"<color=cyan>[DocumentUI]</color> Abriendo documento en pantalla: {title}");

            // Pausar rotación de cámara para leer cómodamente
            if (playerCameraLook != null)
            {
                playerCameraLook.SetCursorLock(false);
                playerCameraLook.enabled = false;
            }
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
            }
        }

        public void CloseDocument()
        {
            if (!isOpen) return;

            isOpen = false;
            LastClosedFrame = Time.frameCount;
            LastClosedTime = Time.unscaledTime;

            if (documentPanel != null)
            {
                documentPanel.SetActive(false);
            }

            Debug.Log("<color=cyan>[DocumentUI]</color> Documento cerrado. Reanudando control.");

            // Reanudar controles del jugador
            if (playerCameraLook != null)
            {
                playerCameraLook.enabled = true;
                playerCameraLook.SetCursorLock(true);
            }
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }
        }
    }
}
