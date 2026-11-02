using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UltimoActo.UI
{
    /// <summary>
    /// Controlador del Menú Principal de 'Último Acto'.
    /// Gestiona la navegación de botones, apertura de submenús (Cooperativo, Cómo Jugar)
    /// y la transición a la escena oficial del juego (SampleScene).
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Paneles del Menú")]
        [Tooltip("Panel que contiene los 4 botones principales")]
        [SerializeField] private GameObject mainPanel;

        [Tooltip("Submenú con opciones de Crear / Unirse a Sala")]
        [SerializeField] private GameObject coopPanel;

        [Tooltip("Submenú con reglas, controles y filosofía del juego")]
        [SerializeField] private GameObject howToPlayPanel;

        [Header("Referencias de Botones")]
        [SerializeField] private Button btnNewGame;
        [SerializeField] private Button btnCoop;
        [SerializeField] private Button btnHowTo;
        [SerializeField] private Button btnQuit;
        [SerializeField] private Button btnCreateRoom;
        [SerializeField] private Button btnJoinRoom;
        [SerializeField] private Button btnBackCoop;
        [SerializeField] private Button btnCloseHowTo;

        [Header("Configuración de Escenas")]
        [Tooltip("Nombre de la escena oficial del juego")]
        [SerializeField] private string targetSceneName = "SampleScene";

        [Header("Audio (Opcional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip clickSound;

        private void Awake()
        {
            // Auto-vincular eventos al iniciar para garantizar funcionamiento al 100%
            if (btnNewGame != null) btnNewGame.onClick.AddListener(OnNewGameClicked);
            if (btnCoop != null) btnCoop.onClick.AddListener(OnCoopClicked);
            if (btnHowTo != null) btnHowTo.onClick.AddListener(OnHowToPlayClicked);
            if (btnQuit != null) btnQuit.onClick.AddListener(OnQuitClicked);

            if (btnCreateRoom != null) btnCreateRoom.onClick.AddListener(OnCreateRoomClicked);
            if (btnJoinRoom != null) btnJoinRoom.onClick.AddListener(OnJoinRoomClicked);
            if (btnBackCoop != null) btnBackCoop.onClick.AddListener(OnBackFromCoopClicked);

            if (btnCloseHowTo != null) btnCloseHowTo.onClick.AddListener(OnCloseHowToPlayClicked);
        }

        private void Start()
        {
            ShowMainPanel();
            
            // Asegurar que el cursor esté visible y desbloqueado en el menú
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        #region Botones Principales

        public void OnNewGameClicked()
        {
            PlayClickSound();
            Debug.Log($"[MainMenu] Iniciando Nueva Partida en '{targetSceneName}'...");
            LoadTargetScene();
        }

        public void OnCoopClicked()
        {
            PlayClickSound();
            if (mainPanel != null) mainPanel.SetActive(false);
            if (coopPanel != null) coopPanel.SetActive(true);
            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        }

        public void OnHowToPlayClicked()
        {
            PlayClickSound();
            if (mainPanel != null) mainPanel.SetActive(false);
            if (coopPanel != null) coopPanel.SetActive(false);
            if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
        }

        public void OnQuitClicked()
        {
            PlayClickSound();
            Debug.Log("[MainMenu] Saliendo del juego...");
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region Submenú Cooperativo

        public void OnCreateRoomClicked()
        {
            PlayClickSound();
            Debug.Log("[MainMenu] Creando sala como Host...");
            PlayerPrefs.SetString("NetworkRole", "Host");
            PlayerPrefs.Save();
            LoadTargetScene();
        }

        public void OnJoinRoomClicked()
        {
            PlayClickSound();
            Debug.Log("[MainMenu] Uniéndose a sala como Cliente...");
            PlayerPrefs.SetString("NetworkRole", "Client");
            PlayerPrefs.Save();
            LoadTargetScene();
        }

        public void OnBackFromCoopClicked()
        {
            PlayClickSound();
            ShowMainPanel();
        }

        #endregion

        #region Submenú Cómo Jugar

        public void OnCloseHowToPlayClicked()
        {
            PlayClickSound();
            ShowMainPanel();
        }

        #endregion

        #region Métodos Auxiliares

        public void ShowMainPanel()
        {
            if (mainPanel != null) mainPanel.SetActive(true);
            if (coopPanel != null) coopPanel.SetActive(false);
            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        }

        private void LoadTargetScene()
        {
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogError("[MainMenu] No se ha especificado el nombre de la escena de destino.");
            }
        }

        public void PlayHoverSound()
        {
            if (audioSource != null && hoverSound != null)
            {
                audioSource.PlayOneShot(hoverSound, 0.4f);
            }
        }

        private void PlayClickSound()
        {
            if (audioSource != null && clickSound != null)
            {
                audioSource.PlayOneShot(clickSound, 0.7f);
            }
        }

        #endregion
    }
}
