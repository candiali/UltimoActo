using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UltimoActo.Core;

namespace UltimoActo.UI
{
    /// <summary>
    /// Controlador del Menú Principal de 'Último Acto' (Paso 1 — Multijugador LAN y Modo Solo).
    /// Conserva la estética del Gran Teatro de Ópera (negro borgoña, pan de oro envejecido y marfil)
    /// e integra:
    ///  - "Jugar solo"
    ///  - "Crear Sala" (Host = Época 1929, anuncio automático por UDP broadcast en la LAN)
    ///  - "Unirse a Sala" (Cliente = Época Ruinas, lista automática de salas LAN con 1 clic,
    ///    botón "Buscar de nuevo", respaldo de IP manual y avisos de estado / Firewall de Windows).
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Paneles del Menú")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject coopPanel; // Panel "Crear Sala" o submenú heredado
        [SerializeField] private GameObject joinLanPanel;
        [SerializeField] private GameObject howToPlayPanel;
        [SerializeField] private GameObject firewallHelpBox;

        [Header("Botones Principales")]
        [SerializeField] private Button btnNewGame;     // "JUGAR SOLO"
        [SerializeField] private Button btnCoop;        // "CREAR SALA"
        [SerializeField] private Button btnJoinLanMenu; // "UNIRSE A SALA"
        [SerializeField] private Button btnHowTo;
        [SerializeField] private Button btnQuit;

        [Header("Panel Crear Sala (Host - Época 1929)")]
        [SerializeField] private TMP_InputField inputHostRoomName;
        [SerializeField] private TMP_InputField inputHostPlayerName;
        [SerializeField] private TextMeshProUGUI hostNightAndVersionInfoTMP;
        [SerializeField] private TextMeshProUGUI hostStatusTMP;
        [SerializeField] private Button btnCreateRoom;
        [SerializeField] private Button btnBackCoop;

        [Header("Panel Unirse a Sala (Cliente - Época Ruinas)")]
        [SerializeField] private TMP_InputField inputClientPlayerName;
        [SerializeField] private TextMeshProUGUI joinStatusTMP;
        [SerializeField] private RectTransform roomListContainer;
        [SerializeField] private Button btnRefreshRooms;
        [SerializeField] private Button btnFirewallHelp;
        [SerializeField] private TMP_InputField inputManualIp;
        [SerializeField] private Button btnJoinRoom; // Botón "Conectar por IP" (respaldo)
        [SerializeField] private Button btnBackJoinLan;

        [Header("Submenú Cómo Jugar")]
        [SerializeField] private Button btnCloseHowTo;

        [Header("Configuración de Escenas")]
        [SerializeField] private string targetSceneName = "SampleScene";

        [Header("Audio (Opcional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip clickSound;

        private readonly List<GameObject> spawnedRoomCards = new List<GameObject>();
        private float searchStartTime;
        private bool isSearchingLan;
        private bool shownNoRoomsYet;

        // Colores de la estética del Gran Teatro de Ópera
        private static readonly Color ColOperaDarkBg = new Color(0.06f, 0.025f, 0.035f, 0.97f);
        private static readonly Color ColOperaCardBg = new Color(0.11f, 0.04f, 0.06f, 0.94f);
        private static readonly Color ColOperaGold = new Color(0.85f, 0.72f, 0.44f, 1f);
        private static readonly Color ColOperaIvory = new Color(0.96f, 0.94f, 0.90f, 1f);
        private static readonly Color ColOperaWineHighlight = new Color(0.38f, 0.09f, 0.16f, 0.98f);
        private static readonly Color ColStatusError = new Color(0.96f, 0.48f, 0.44f, 1f);
        private static readonly Color ColStatusOk = new Color(0.88f, 0.78f, 0.50f, 1f);

        private void Awake()
        {
            EnsureLanMenuStructure();
            BindAllButtons();
        }

        private void Start()
        {
            ShowMainPanel();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            LanSessionManager session = LanSessionManager.EnsureInstance();
            session.OnStatusMessage -= OnSessionStatusMessage;
            session.OnStatusMessage += OnSessionStatusMessage;

            if (session.Discovery != null)
            {
                session.Discovery.OnRoomsUpdated -= OnDiscoveredRoomsUpdated;
                session.Discovery.OnRoomsUpdated += OnDiscoveredRoomsUpdated;
            }

            PopulateDefaultInputs();
        }

        private void OnDestroy()
        {
            if (LanSessionManager.Instance != null)
            {
                LanSessionManager.Instance.OnStatusMessage -= OnSessionStatusMessage;
                if (LanSessionManager.Instance.Discovery != null)
                {
                    LanSessionManager.Instance.Discovery.OnRoomsUpdated -= OnDiscoveredRoomsUpdated;
                }
            }
        }

        private void Update()
        {
            if (isSearchingLan && !shownNoRoomsYet && Time.realtimeSinceStartup - searchStartTime > 3.2f)
            {
                if (LanSessionManager.Instance != null &&
                    LanSessionManager.Instance.Discovery != null &&
                    LanSessionManager.Instance.Discovery.DiscoveredRooms.Count == 0)
                {
                    shownNoRoomsYet = true;
                    SetJoinStatus(
                        "No se encontraron salas en la red local. Pulsa «Buscar de nuevo», verifica que el anfitrión haya abierto la sala o revisa el aviso de Firewall.",
                        false);
                }
            }
        }

        private void BindAllButtons()
        {
            if (btnNewGame != null)
            {
                btnNewGame.onClick.RemoveAllListeners();
                btnNewGame.onClick.AddListener(OnPlaySoloClicked);
            }
            if (btnCoop != null)
            {
                btnCoop.onClick.RemoveAllListeners();
                btnCoop.onClick.AddListener(OnOpenCreateRoomPanelClicked);
            }
            if (btnJoinLanMenu != null)
            {
                btnJoinLanMenu.onClick.RemoveAllListeners();
                btnJoinLanMenu.onClick.AddListener(OnOpenJoinRoomPanelClicked);
            }
            if (btnHowTo != null)
            {
                btnHowTo.onClick.RemoveAllListeners();
                btnHowTo.onClick.AddListener(OnHowToPlayClicked);
            }
            if (btnQuit != null)
            {
                btnQuit.onClick.RemoveAllListeners();
                btnQuit.onClick.AddListener(OnQuitClicked);
            }

            if (btnCreateRoom != null)
            {
                btnCreateRoom.onClick.RemoveAllListeners();
                btnCreateRoom.onClick.AddListener(OnConfirmCreateRoomClicked);
            }
            if (btnBackCoop != null)
            {
                btnBackCoop.onClick.RemoveAllListeners();
                btnBackCoop.onClick.AddListener(OnBackFromCoopClicked);
            }

            if (btnRefreshRooms != null)
            {
                btnRefreshRooms.onClick.RemoveAllListeners();
                btnRefreshRooms.onClick.AddListener(OnRefreshRoomsClicked);
            }
            if (btnFirewallHelp != null)
            {
                btnFirewallHelp.onClick.RemoveAllListeners();
                btnFirewallHelp.onClick.AddListener(OnToggleFirewallHelpClicked);
            }
            if (btnJoinRoom != null)
            {
                btnJoinRoom.onClick.RemoveAllListeners();
                btnJoinRoom.onClick.AddListener(OnJoinByManualIpClicked);
            }
            if (btnBackJoinLan != null)
            {
                btnBackJoinLan.onClick.RemoveAllListeners();
                btnBackJoinLan.onClick.AddListener(OnBackFromCoopClicked);
            }

            if (btnCloseHowTo != null)
            {
                btnCloseHowTo.onClick.RemoveAllListeners();
                btnCloseHowTo.onClick.AddListener(OnCloseHowToPlayClicked);
            }
        }

        private void PopulateDefaultInputs()
        {
            string sysUser = string.IsNullOrWhiteSpace(Environment.UserName) ? "Vesper" : Environment.UserName;
            string savedRoom = PlayerPrefs.GetString("LanRoomName", $"Academia Vesper — Sala de {sysUser}");
            string savedPlayer = PlayerPrefs.GetString("LanPlayerName", sysUser);

            if (inputHostRoomName != null && string.IsNullOrEmpty(inputHostRoomName.text))
            {
                inputHostRoomName.text = savedRoom;
            }
            if (inputHostPlayerName != null && string.IsNullOrEmpty(inputHostPlayerName.text))
            {
                inputHostPlayerName.text = savedPlayer;
            }
            if (inputClientPlayerName != null && string.IsNullOrEmpty(inputClientPlayerName.text))
            {
                inputClientPlayerName.text = savedPlayer;
            }
            if (inputManualIp != null && string.IsNullOrEmpty(inputManualIp.text))
            {
                inputManualIp.text = PlayerPrefs.GetString("LanHostIp", "");
            }

            RefreshHostNightSummary();
        }

        private void RefreshHostNightSummary()
        {
            if (hostNightAndVersionInfoTMP == null) return;

            var nights = NightManager.BuildDefaultNights();
            int nIdx = Mathf.Clamp(PlayerPrefs.GetInt(NightManager.PrefsKey, 0), 0, nights.Count - 1);
            NightDefinition cur = nights[nIdx];
            string ver = LanRoomDiscovery.ProtocolGameVersion;

            hostNightAndVersionInfoTMP.text =
                $"• <b>Época del Anfitrión:</b> <color=#E8C97A>1929 (Academia en su Gloria)</color>\n" +
                $"• <b>Progreso actual a anunciar:</b> Noche {cur.roman} — {cur.title} ({cur.clockTime})\n" +
                $"• <b>Versión del protocolo:</b> v{ver}   ·   <b>Cupo:</b> 2 Jugadores en Red Local (UDP Broadcast)";
        }

        #region Acciones de Botones Principales

        /// <summary>
        /// Botón "JUGAR SOLO": Inicia partida individual en Época 1929 sin requerir conexión.
        /// </summary>
        public void OnPlaySoloClicked()
        {
            PlayClickSound();
            LanSessionManager.EnsureInstance().StartSoloSession(targetSceneName);
        }

        // Alias heredado por compatibilidad con escenas previas
        public void OnNewGameClicked() => OnPlaySoloClicked();

        /// <summary>
        /// Botón "CREAR SALA": Abre el modal de creación de sala LAN (Host = Época 1929).
        /// </summary>
        public void OnOpenCreateRoomPanelClicked()
        {
            PlayClickSound();
            isSearchingLan = false;
            if (LanSessionManager.Instance != null && LanSessionManager.Instance.Discovery != null)
            {
                LanSessionManager.Instance.Discovery.StopAllDiscovery();
            }

            RefreshHostNightSummary();
            if (hostStatusTMP != null)
            {
                hostStatusTMP.text = "Al pulsar «Abrir Sala», tu partida se anunciará automáticamente por UDP en la red local.";
                hostStatusTMP.color = ColStatusOk;
            }

            if (mainPanel != null) mainPanel.SetActive(false);
            if (coopPanel != null) coopPanel.SetActive(true);
            if (joinLanPanel != null) joinLanPanel.SetActive(false);
            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        }

        public void OnCoopClicked() => OnOpenCreateRoomPanelClicked();

        /// <summary>
        /// Botón "UNIRSE A SALA": Abre el buscador automático de salas en la LAN.
        /// </summary>
        public void OnOpenJoinRoomPanelClicked()
        {
            PlayClickSound();
            if (mainPanel != null) mainPanel.SetActive(false);
            if (coopPanel != null) coopPanel.SetActive(false);
            if (joinLanPanel != null) joinLanPanel.SetActive(true);
            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
            if (firewallHelpBox != null) firewallHelpBox.SetActive(false);

            StartAutomaticLanSearch();
        }

        public void OnHowToPlayClicked()
        {
            PlayClickSound();
            isSearchingLan = false;
            if (mainPanel != null) mainPanel.SetActive(false);
            if (coopPanel != null) coopPanel.SetActive(false);
            if (joinLanPanel != null) joinLanPanel.SetActive(false);
            if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
        }

        public void OnQuitClicked()
        {
            PlayClickSound();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region Acciones de Crear Sala y Unirse a Sala (LAN)

        public void OnConfirmCreateRoomClicked()
        {
            PlayClickSound();
            string rName = inputHostRoomName != null ? inputHostRoomName.text : "Academia Vesper";
            string pName = inputHostPlayerName != null ? inputHostPlayerName.text : "Investigador 1929";

            LanSessionManager.EnsureInstance().StartHostSession(rName, pName, targetSceneName);
        }

        public void OnCreateRoomClicked() => OnConfirmCreateRoomClicked();

        private void StartAutomaticLanSearch()
        {
            ClearRoomCards();
            searchStartTime = Time.realtimeSinceStartup;
            isSearchingLan = true;
            shownNoRoomsYet = false;

            SetJoinStatus("Buscando salas…", false);
            LanSessionManager.EnsureInstance().BeginSearchingLanRooms();
        }

        public void OnRefreshRoomsClicked()
        {
            PlayClickSound();
            ClearRoomCards();
            searchStartTime = Time.realtimeSinceStartup;
            isSearchingLan = true;
            shownNoRoomsYet = false;

            SetJoinStatus("Buscando salas…", false);
            LanSessionManager.EnsureInstance().RefreshLanSearch();
        }

        public void OnToggleFirewallHelpClicked()
        {
            PlayClickSound();
            if (firewallHelpBox != null)
            {
                firewallHelpBox.SetActive(!firewallHelpBox.activeSelf);
            }
        }

        public void OnJoinByManualIpClicked()
        {
            PlayClickSound();
            string ip = inputManualIp != null ? inputManualIp.text : "";
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = "127.0.0.1";
                if (inputManualIp != null) inputManualIp.text = ip;
            }
            string pName = inputClientPlayerName != null ? inputClientPlayerName.text : "Investigador Ruinas";
            SetJoinStatus($"Conectando a {ip}…", false);
            LanSessionManager.EnsureInstance().RequestJoinByManualIp(ip, pName, targetSceneName);
        }

        public void OnJoinRoomClicked() => OnJoinByManualIpClicked();

        private void OnRoomCardClicked(DiscoveredRoomInfo room)
        {
            PlayClickSound();
            if (room == null) return;

            string pName = inputClientPlayerName != null ? inputClientPlayerName.text : "Investigador Ruinas";
            LanSessionManager.EnsureInstance().RequestJoinDiscoveredRoom(room, pName, targetSceneName);
        }

        public void OnBackFromCoopClicked()
        {
            PlayClickSound();
            isSearchingLan = false;
            if (LanSessionManager.Instance != null && LanSessionManager.Instance.Discovery != null)
            {
                LanSessionManager.Instance.Discovery.StopAllDiscovery();
            }
            ShowMainPanel();
        }

        public void OnCloseHowToPlayClicked()
        {
            PlayClickSound();
            ShowMainPanel();
        }

        #endregion

        #region Actualización de Lista de Salas y Mensajes de Estado

        private void OnSessionStatusMessage(string message, bool isError)
        {
            if (joinLanPanel != null && joinLanPanel.activeSelf)
            {
                SetJoinStatus(message, isError);
            }
            if (coopPanel != null && coopPanel.activeSelf && hostStatusTMP != null)
            {
                hostStatusTMP.text = message;
                hostStatusTMP.color = isError ? ColStatusError : ColStatusOk;
            }
        }

        private void SetJoinStatus(string msg, bool isError)
        {
            if (joinStatusTMP == null) return;
            joinStatusTMP.text = msg;
            joinStatusTMP.color = isError ? ColStatusError : ColStatusOk;
        }

        private void OnDiscoveredRoomsUpdated(List<DiscoveredRoomInfo> rooms)
        {
            if (roomListContainer == null) return;

            ClearRoomCards();

            if (rooms == null || rooms.Count == 0)
            {
                if (Time.realtimeSinceStartup - searchStartTime > 3.0f)
                {
                    SetJoinStatus("No se encontraron salas en la red local. Pulsa «Buscar de nuevo» o verifica el Firewall de Windows.", false);
                }
                return;
            }

            shownNoRoomsYet = true;
            SetJoinStatus($"Salas encontradas en la red local ({rooms.Count}) — Haz clic en una sala para entrar:", false);

            int maxDisplayed = Mathf.Min(rooms.Count, 4);
            for (int i = 0; i < maxDisplayed; i++)
            {
                CreateRoomCardUI(rooms[i]);
            }
        }

        private void ClearRoomCards()
        {
            for (int i = 0; i < spawnedRoomCards.Count; i++)
            {
                if (spawnedRoomCards[i] != null)
                {
                    Destroy(spawnedRoomCards[i]);
                }
            }
            spawnedRoomCards.Clear();
        }

        private void CreateRoomCardUI(DiscoveredRoomInfo room)
        {
            if (roomListContainer == null || room == null) return;

            GameObject cardObj = new GameObject($"RoomCard_{room.roomId}");
            cardObj.transform.SetParent(roomListContainer, false);
            spawnedRoomCards.Add(cardObj);

            RectTransform rt = cardObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(680f, 64f);

            Image bg = cardObj.AddComponent<Image>();
            bg.color = ColOperaCardBg;

            Button btn = cardObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = ColOperaCardBg;
            cb.highlightedColor = ColOperaWineHighlight;
            cb.pressedColor = new Color(0.52f, 0.14f, 0.22f, 1f);
            cb.selectedColor = ColOperaWineHighlight;
            btn.colors = cb;

            // Filete inferior dorado
            GameObject border = new GameObject("GoldLine");
            border.transform.SetParent(cardObj.transform, false);
            RectTransform brt = border.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0f, 0f);
            brt.anchorMax = new Vector2(1f, 0f);
            brt.pivot = new Vector2(0.5f, 0f);
            brt.anchoredPosition = Vector2.zero;
            brt.sizeDelta = new Vector2(0f, 1.5f);
            Image bImg = border.AddComponent<Image>();
            bImg.color = new Color(ColOperaGold.r, ColOperaGold.g, ColOperaGold.b, 0.55f);
            bImg.raycastTarget = false;

            // Texto izquierdo: Nombre de Sala + Anfitrión + Noche
            GameObject leftTxtObj = new GameObject("RoomInfo");
            leftTxtObj.transform.SetParent(cardObj.transform, false);
            RectTransform lrt = leftTxtObj.AddComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0f, 0f);
            lrt.anchorMax = new Vector2(0.70f, 1f);
            lrt.offsetMin = new Vector2(16f, 6f);
            lrt.offsetMax = new Vector2(-8f, -6f);

            TextMeshProUGUI leftTmp = leftTxtObj.AddComponent<TextMeshProUGUI>();
            leftTmp.fontSize = 15.5f;
            leftTmp.alignment = TextAlignmentOptions.MidlineLeft;
            leftTmp.color = ColOperaIvory;
            leftTmp.raycastTarget = false;
            leftTmp.text =
                $"<b>{room.roomName}</b>   <color=#D8B468>·   Anfitrión: {room.hostPlayerName}</color>\n" +
                $"<size=13><color=#C8C0B4>{room.nightDisplay}   ·   Jugadores: {room.currentPlayers}/{room.maxPlayers}   ·   v{room.gameVersion}</color></size>";

            // Etiqueta derecha: Acción / Estado
            GameObject rightTxtObj = new GameObject("JoinBadge");
            rightTxtObj.transform.SetParent(cardObj.transform, false);
            RectTransform rrt = rightTxtObj.AddComponent<RectTransform>();
            rrt.anchorMin = new Vector2(0.70f, 0f);
            rrt.anchorMax = new Vector2(1f, 1f);
            rrt.offsetMin = new Vector2(4f, 6f);
            rrt.offsetMax = new Vector2(-16f, -6f);

            TextMeshProUGUI rightTmp = rightTxtObj.AddComponent<TextMeshProUGUI>();
            rightTmp.fontSize = 13.5f;
            rightTmp.fontStyle = FontStyles.Bold;
            rightTmp.alignment = TextAlignmentOptions.MidlineRight;
            rightTmp.raycastTarget = false;

            if (!room.IsCompatibleVersion)
            {
                rightTmp.text = "VERSIÓN DISTINTA";
                rightTmp.color = ColStatusError;
            }
            else if (room.IsFull)
            {
                rightTmp.text = "SALA LLENA (2/2)";
                rightTmp.color = ColStatusError;
            }
            else
            {
                rightTmp.text = "ENTRAR (RUINAS) →";
                rightTmp.color = ColOperaGold;
            }

            DiscoveredRoomInfo captured = room;
            btn.onClick.AddListener(() => OnRoomCardClicked(captured));
            cardObj.AddComponent<MenuButtonEffects>();
        }

        #endregion

        #region Auto-Aseguramiento Dinámico de la UI del Menú (Si se abre sin reconstruir)

        private void EnsureLanMenuStructure()
        {
            // 1. Renombrar botones principales a "JUGAR SOLO", "CREAR SALA", "UNIRSE A SALA" manteniendo estética
            if (btnNewGame != null) SetButtonLabel(btnNewGame, "JUGAR SOLO");
            if (btnCoop != null) SetButtonLabel(btnCoop, "CREAR SALA");

            // Si la escena aún no tenía el 3er botón "UNIRSE A SALA" en la columna principal, lo insertamos con la misma estética
            if (btnJoinLanMenu == null && btnCoop != null && btnCoop.transform.parent != null)
            {
                Transform col = btnCoop.transform.parent;
                RectTransform colRt = col.GetComponent<RectTransform>();
                if (colRt != null && colRt.sizeDelta.y < 380f)
                {
                    colRt.sizeDelta = new Vector2(colRt.sizeDelta.x, 390f);
                }

                GameObject clone = Instantiate(btnCoop.gameObject, col);
                clone.name = "Btn_JoinLanMenu";
                clone.transform.SetSiblingIndex(btnCoop.transform.GetSiblingIndex() + 1);
                btnJoinLanMenu = clone.GetComponent<Button>();
                SetButtonLabel(btnJoinLanMenu, "UNIRSE A SALA");
            }

            // Si aún no existe joinLanPanel (escena previa al Paso 1), lo construimos en tiempo de ejecución con la estética del teatro
            if (joinLanPanel == null)
            {
                BuildRuntimeLanPanels();
            }
        }

        private static void SetButtonLabel(Button btn, string text)
        {
            if (btn == null) return;
            TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null) tmp.text = text;
        }

        private void BuildRuntimeLanPanels()
        {
            Transform canvasRoot = transform;

            // Actualizar coopPanel para ser el modal de "CREAR SALA" con campos de Nombre de Sala y Nombre de Jugador
            if (coopPanel != null && inputHostRoomName == null)
            {
                RectTransform crt = coopPanel.GetComponent<RectTransform>();
                if (crt != null) crt.sizeDelta = new Vector2(680f, 540f);

                Transform oldBtns = coopPanel.transform.Find("Coop_Buttons_Column");
                if (oldBtns != null) oldBtns.gameObject.SetActive(false);

                inputHostRoomName = CreateStyledInputField("Input_HostRoomName", coopPanel.transform,
                    "Nombre de la Sala:", "Academia Vesper — Sala LAN", new Vector2(0f, 105f));
                inputHostPlayerName = CreateStyledInputField("Input_HostPlayerName", coopPanel.transform,
                    "Nombre del Anfitrión:", "Investigador 1929", new Vector2(0f, 25f));

                hostNightAndVersionInfoTMP = CreateStyledText("Host_InfoSummary", coopPanel.transform,
                    new Vector2(0f, -65f), new Vector2(580f, 75f), 14f, TextAlignmentOptions.TopLeft, ColOperaIvory);

                hostStatusTMP = CreateStyledText("Host_StatusAndFirewall", coopPanel.transform,
                    new Vector2(0f, -135f), new Vector2(580f, 48f), 13f, TextAlignmentOptions.Center, ColStatusOk);

                btnCreateRoom = CreateStyledButton("Btn_ConfirmCreateRoom", coopPanel.transform,
                    "ABRIR SALA Y ENTRAR (1929)", new Vector2(-145f, -210f), new Vector2(270f, 46f));
                btnBackCoop = CreateStyledButton("Btn_BackCreateRoom", coopPanel.transform,
                    "VOLVER", new Vector2(165f, -210f), new Vector2(230f, 46f));
            }

            // Crear joinLanPanel ("UNIRSE A SALA") con lista automática LAN, Buscar de nuevo, Firewall y respaldo IP
            joinLanPanel = new GameObject("JoinLan_Submenu_Panel");
            joinLanPanel.transform.SetParent(canvasRoot, false);
            RectTransform jrt = joinLanPanel.AddComponent<RectTransform>();
            jrt.anchorMin = new Vector2(0.5f, 0.5f);
            jrt.anchorMax = new Vector2(0.5f, 0.5f);
            jrt.pivot = new Vector2(0.5f, 0.5f);
            jrt.anchoredPosition = Vector2.zero;
            jrt.sizeDelta = new Vector2(820f, 650f);

            Image jBg = joinLanPanel.AddComponent<Image>();
            jBg.color = ColOperaDarkBg;

            GameObject border = new GameObject("Gold_Modal_Border");
            border.transform.SetParent(joinLanPanel.transform, false);
            RectTransform bRect = border.AddComponent<RectTransform>();
            bRect.anchorMin = Vector2.zero;
            bRect.anchorMax = Vector2.one;
            bRect.offsetMin = new Vector2(-1.5f, -1.5f);
            bRect.offsetMax = new Vector2(1.5f, 1.5f);
            Image bImg = border.AddComponent<Image>();
            bImg.color = new Color(ColOperaGold.r, ColOperaGold.g, ColOperaGold.b, 0.7f);
            bImg.raycastTarget = false;
            border.transform.SetAsFirstSibling();

            CreateStyledText("Join_Title", joinLanPanel.transform, new Vector2(0f, 282f), new Vector2(740f, 36f),
                22f, TextAlignmentOptions.Center, ColOperaIvory, FontStyles.Bold).text = "UNIRSE A SALA EN RED LOCAL";
            CreateStyledText("Join_Subtitle", joinLanPanel.transform, new Vector2(0f, 252f), new Vector2(740f, 26f),
                14f, TextAlignmentOptions.Center, ColOperaGold).text = "Invitado: Época Actual (Ruinas de la Academia) · Haz clic en una sala para entrar";

            inputClientPlayerName = CreateStyledInputField("Input_ClientPlayerName", joinLanPanel.transform,
                "Tu Nombre:", "Investigador Ruinas", new Vector2(0f, 196f));

            joinStatusTMP = CreateStyledText("Join_StatusBanner", joinLanPanel.transform,
                new Vector2(0f, 142f), new Vector2(720f, 36f), 14.5f, TextAlignmentOptions.Center, ColStatusOk, FontStyles.Bold);

            // Contenedor de la lista de salas encontradas en LAN
            GameObject listObj = new GameObject("DiscoveredRooms_List");
            listObj.transform.SetParent(joinLanPanel.transform, false);
            roomListContainer = listObj.AddComponent<RectTransform>();
            roomListContainer.anchorMin = new Vector2(0.5f, 0.5f);
            roomListContainer.anchorMax = new Vector2(0.5f, 0.5f);
            roomListContainer.pivot = new Vector2(0.5f, 0.5f);
            roomListContainer.anchoredPosition = new Vector2(0f, -10f);
            roomListContainer.sizeDelta = new Vector2(700f, 250f);

            Image listBg = listObj.AddComponent<Image>();
            listBg.color = new Color(0.03f, 0.015f, 0.02f, 0.75f);

            VerticalLayoutGroup vlg = listObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Fila de respaldo por IP manual
            inputManualIp = CreateStyledInputField("Input_ManualIpFallback", joinLanPanel.transform,
                "IP Manual (Respaldo):", "", new Vector2(-115f, -172f), 460f);
            if (inputManualIp.placeholder is TextMeshProUGUI ph)
            {
                ph.text = "Opcional: ej. 192.168.1.50 o 127.0.0.1";
            }
            btnJoinRoom = CreateStyledButton("Btn_ConnectManualIp", joinLanPanel.transform,
                "CONECTAR IP", new Vector2(235f, -184f), new Vector2(210f, 40f));

            // Caja desplegable de ayuda de Firewall
            firewallHelpBox = new GameObject("Firewall_Help_Box");
            firewallHelpBox.transform.SetParent(joinLanPanel.transform, false);
            RectTransform fhrt = firewallHelpBox.AddComponent<RectTransform>();
            fhrt.anchorMin = new Vector2(0.5f, 0.5f);
            fhrt.anchorMax = new Vector2(0.5f, 0.5f);
            fhrt.pivot = new Vector2(0.5f, 0.5f);
            fhrt.anchoredPosition = new Vector2(0f, -10f);
            fhrt.sizeDelta = new Vector2(700f, 250f);
            Image fhBg = firewallHelpBox.AddComponent<Image>();
            fhBg.color = new Color(0.08f, 0.03f, 0.04f, 0.98f);
            TextMeshProUGUI fhTxt = CreateStyledText("Firewall_Help_Text", firewallHelpBox.transform,
                Vector2.zero, new Vector2(660f, 220f), 14f, TextAlignmentOptions.MidlineLeft, ColOperaIvory);
            fhTxt.text =
                "<b><color=#D8B468>¿CÓMO PERMITIR LA CONEXIÓN EN EL FIREWALL DE WINDOWS?</color></b>\n\n" +
                "1. Cuando abras o busques sala por primera vez, Windows Defender mostrará una ventana emergente.\n" +
                "2. Marca <b>ambas casillas</b>: <color=#E8C97A>«Redes privadas»</color> y <color=#E8C97A>«Redes públicas»</color> y haz clic en <b>«Permitir acceso»</b>.\n" +
                "3. Si cerraste la ventana sin permitirlo: abre <i>Seguridad de Windows > Firewall y protección de red > Permitir una aplicación a través del firewall</i> y habilita <b>UltimoActo</b> (o <b>Unity Editor</b>) en Privada y Pública.\n" +
                "4. Puertos locales usados automáticamente: <b>UDP 47777–47778</b> (anuncio LAN) y <b>UDP 7777</b> (Netcode).";
            firewallHelpBox.SetActive(false);

            // Botones inferiores: Buscar de nuevo | Ayuda de Firewall | Volver
            btnRefreshRooms = CreateStyledButton("Btn_RefreshRooms", joinLanPanel.transform,
                "BUSCAR DE NUEVO", new Vector2(-235f, -265f), new Vector2(220f, 44f));
            btnFirewallHelp = CreateStyledButton("Btn_FirewallHelp", joinLanPanel.transform,
                "AVISO DE FIREWALL", new Vector2(0f, -265f), new Vector2(220f, 44f));
            btnBackJoinLan = CreateStyledButton("Btn_BackJoinLan", joinLanPanel.transform,
                "VOLVER", new Vector2(235f, -265f), new Vector2(220f, 44f));

            joinLanPanel.SetActive(false);
        }

        private static TextMeshProUGUI CreateStyledText(
            string name, Transform parent, Vector2 anchoredPos, Vector2 size,
            float fontSize, TextAlignmentOptions align, Color color, FontStyles style = FontStyles.Normal)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.color = color;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button CreateStyledButton(string name, Transform parent, string label, Vector2 anchoredPos, Vector2 size)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.08f, 0.03f, 0.05f, 0.94f);

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(0.08f, 0.03f, 0.05f, 0.94f);
            cb.highlightedColor = ColOperaWineHighlight;
            cb.pressedColor = new Color(0.55f, 0.14f, 0.24f, 1f);
            cb.selectedColor = ColOperaWineHighlight;
            btn.colors = cb;

            GameObject goldBorder = new GameObject("Gold_Accent_Border");
            goldBorder.transform.SetParent(btnObj.transform, false);
            Image bImg = goldBorder.AddComponent<Image>();
            bImg.color = new Color(ColOperaGold.r, ColOperaGold.g, ColOperaGold.b, 0.65f);
            bImg.raycastTarget = false;
            RectTransform bRect = goldBorder.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0f, 0f);
            bRect.anchorMax = new Vector2(1f, 0f);
            bRect.pivot = new Vector2(0.5f, 0f);
            bRect.anchoredPosition = Vector2.zero;
            bRect.sizeDelta = new Vector2(0f, 2f);

            GameObject txtObj = new GameObject("Label");
            txtObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 14.5f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.characterSpacing = 4f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = ColOperaIvory;
            tmp.raycastTarget = false;

            RectTransform tRect = txtObj.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;

            btnObj.AddComponent<MenuButtonEffects>();
            return btn;
        }

        private static TMP_InputField CreateStyledInputField(
            string name, Transform parent, string labelText, string defaultVal, Vector2 anchoredPos, float totalWidth = 580f)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            RectTransform rrt = root.AddComponent<RectTransform>();
            rrt.anchorMin = new Vector2(0.5f, 0.5f);
            rrt.anchorMax = new Vector2(0.5f, 0.5f);
            rrt.pivot = new Vector2(0.5f, 0.5f);
            rrt.anchoredPosition = anchoredPos;
            rrt.sizeDelta = new Vector2(totalWidth, 62f);

            CreateStyledText("FieldLabel", root.transform, new Vector2(0f, 21f), new Vector2(totalWidth, 20f),
                13f, TextAlignmentOptions.MidlineLeft, ColOperaGold, FontStyles.Bold).text = labelText;

            GameObject boxObj = new GameObject("InputBox");
            boxObj.transform.SetParent(root.transform, false);
            RectTransform brt = boxObj.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0f, 0f);
            brt.anchorMax = new Vector2(1f, 0f);
            brt.pivot = new Vector2(0.5f, 0f);
            brt.anchoredPosition = Vector2.zero;
            brt.sizeDelta = new Vector2(0f, 38f);

            Image boxBg = boxObj.AddComponent<Image>();
            boxBg.color = new Color(0.14f, 0.05f, 0.08f, 0.95f);

            GameObject textArea = new GameObject("TextArea");
            textArea.transform.SetParent(boxObj.transform, false);
            RectTransform tart = textArea.AddComponent<RectTransform>();
            tart.anchorMin = Vector2.zero;
            tart.anchorMax = Vector2.one;
            tart.offsetMin = new Vector2(12f, 4f);
            tart.offsetMax = new Vector2(-12f, -4f);

            GameObject phObj = new GameObject("Placeholder");
            phObj.transform.SetParent(textArea.transform, false);
            RectTransform phrt = phObj.AddComponent<RectTransform>();
            phrt.anchorMin = Vector2.zero;
            phrt.anchorMax = Vector2.one;
            phrt.offsetMin = Vector2.zero;
            phrt.offsetMax = Vector2.zero;
            TextMeshProUGUI phTmp = phObj.AddComponent<TextMeshProUGUI>();
            phTmp.text = "Escribe aquí...";
            phTmp.fontSize = 15f;
            phTmp.fontStyle = FontStyles.Italic;
            phTmp.alignment = TextAlignmentOptions.MidlineLeft;
            phTmp.color = new Color(0.65f, 0.58f, 0.54f, 0.65f);

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(textArea.transform, false);
            RectTransform trt = txtObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            TextMeshProUGUI txtTmp = txtObj.AddComponent<TextMeshProUGUI>();
            txtTmp.fontSize = 15.5f;
            txtTmp.alignment = TextAlignmentOptions.MidlineLeft;
            txtTmp.color = ColOperaIvory;

            TMP_InputField input = boxObj.AddComponent<TMP_InputField>();
            input.textViewport = tart;
            input.textComponent = txtTmp;
            input.placeholder = phTmp;
            input.text = defaultVal;

            return input;
        }

        #endregion

        #region Métodos Auxiliares

        public void ShowMainPanel()
        {
            if (mainPanel != null) mainPanel.SetActive(true);
            if (coopPanel != null) coopPanel.SetActive(false);
            if (joinLanPanel != null) joinLanPanel.SetActive(false);
            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
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
