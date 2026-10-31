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
        public static DocumentUI Instance { get; private set; }

        [Header("Elementos de UI")]
        [SerializeField] private GameObject documentPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private TextMeshProUGUI closeHintText;

        private bool isOpen = false;
        private PlayerCameraLook playerCameraLook;
        private PlayerMovement playerMovement;

        public bool IsOpen => isOpen;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            playerCameraLook = FindFirstObjectByType<PlayerCameraLook>();
            playerMovement = FindFirstObjectByType<PlayerMovement>();

            if (documentPanel != null)
            {
                documentPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (!isOpen) return;

            bool closePressed = false;
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
            {
                closePressed = true;
            }
#else
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
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
            if (doc == null || documentPanel == null) return;

            isOpen = true;
            documentPanel.SetActive(true);

            if (titleText != null) titleText.text = doc.Title;
            if (subtitleText != null) subtitleText.text = doc.Subtitle;
            if (bodyText != null) bodyText.text = doc.Content;
            if (closeHintText != null) closeHintText.text = "Presiona [E] o [Esc] para cerrar";

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
            if (documentPanel != null)
            {
                documentPanel.SetActive(false);
            }

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
