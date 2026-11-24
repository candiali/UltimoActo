using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UltimoActo.Core;
using UltimoActo.Player;

namespace UltimoActo.UI
{
    /// <summary>
    /// Paso 2 — Modo Solo y Entrada de Segundo Jugador:
    /// Menú de pausa y gestión de sala en tiempo real ([ESC]) durante la partida en la Academia Vesper.
    /// Permite a un jugador que empezó en solitario ("Jugar Solo"):
    ///  1. Abrir la sala en la red local (LAN) desde un punto seguro (inicio de una noche o menú de pausa)
    ///     SIN recargar la escena ni perder el progreso actual de noches, puertas, llaves o documentos.
    ///  2. Alternar su época visual ([Q]) mientras esté solo (1/2) y ver cómo se bloquea automáticamente
    ///     cuando se conecta el segundo jugador (2/2).
    ///  3. Consultar el estado de la investigación y volver al Menú Principal si lo desea.
    /// </summary>
    public class InGamePauseMenuUI : MonoBehaviour
    {
        public static InGamePauseMenuUI Instance { get; private set; }

        public bool IsOpen => isOpen;

        private bool isOpen;
        private GameObject pauseRootPanel;
        private TMP_InputField roomNameInput;
        private TextMeshProUGUI nightProgressLabel;
        private TextMeshProUGUI sessionStatusLabel;
        private TextMeshProUGUI eraStatusLabel;
        private TextMeshProUGUI feedbackMessageLabel;
        private Button openLanRoomButton;
        private TextMeshProUGUI openLanRoomButtonLabel;
        private Button toggleEraButton;
        private TextMeshProUGUI toggleEraButtonLabel;

        // Aviso de punto seguro al iniciar cada noche
        private CanvasGroup checkpointToastGroup;
        private TextMeshProUGUI checkpointToastText;
        private float checkpointToastTimer;

        private PlayerMovement cachedPlayerMovement;
        private PlayerCameraLook cachedPlayerLook;

        // Paleta Gran Ópera Vesper
        private static readonly Color ColBackdrop = new Color(0.03f, 0.02f, 0.04f, 0.84f);
        private static readonly Color ColCardBg = new Color(0.08f, 0.04f, 0.06f, 0.97f);
        private static readonly Color ColGoldBorder = new Color(0.85f, 0.71f, 0.41f, 0.85f);
        private static readonly Color ColGoldBright = new Color(0.95f, 0.84f, 0.58f, 1f);
        private static readonly Color ColIvoryText = new Color(0.96f, 0.94f, 0.90f, 1f);
        private static readonly Color ColMutedRose = new Color(0.76f, 0.67f, 0.65f, 1f);
        private static readonly Color ColBtnNormal = new Color(0.16f, 0.07f, 0.10f, 0.96f);
        private static readonly Color ColBtnHover = new Color(0.28f, 0.11f, 0.16f, 1f);
        private static readonly Color ColBtnDisabled = new Color(0.11f, 0.09f, 0.10f, 0.65f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrap()
        {
            if (Instance != null) return;
            if (FindFirstObjectByType<InGamePauseMenuUI>() != null) return;
            if (FindFirstObjectByType<PlayerMovement>() == null) return;

            GameObject go = new GameObject("InGamePauseMenuUI");
            go.AddComponent<InGamePauseMenuUI>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            cachedPlayerMovement = FindFirstObjectByType<PlayerMovement>();
            cachedPlayerLook = FindFirstObjectByType<PlayerCameraLook>();

            BuildPauseMenuCanvas();

            if (NightManager.Instance != null)
            {
                NightManager.Instance.OnNightStarted += OnNightStartedCheckpoint;
            }
            if (LanSessionManager.Instance != null)
            {
                LanSessionManager.Instance.OnConnectedPlayerCountChanged += OnPlayerCountChanged;
                LanSessionManager.Instance.OnRoomOpenedInGame += OnRoomOpenedInGameHandler;
            }
            if (EraVisualManager.Instance != null)
            {
                EraVisualManager.Instance.OnEraChanged += OnEraChangedHandler;
            }

            // Mostrar aviso de punto seguro al comenzar la partida si está en modo Solo
            StartCoroutine(InitialSafeCheckpointNoticeRoutine());
        }

        private void OnDestroy()
        {
            if (NightManager.Instance != null)
            {
                NightManager.Instance.OnNightStarted -= OnNightStartedCheckpoint;
            }
            if (LanSessionManager.Instance != null)
            {
                LanSessionManager.Instance.OnConnectedPlayerCountChanged -= OnPlayerCountChanged;
                LanSessionManager.Instance.OnRoomOpenedInGame -= OnRoomOpenedInGameHandler;
            }
            if (EraVisualManager.Instance != null)
            {
                EraVisualManager.Instance.OnEraChanged -= OnEraChangedHandler;
            }
            if (Instance == this) Instance = null;
        }

        private IEnumerator InitialSafeCheckpointNoticeRoutine()
        {
            yield return new WaitForSeconds(1.5f);
            bool isRoomOpen = LanSessionManager.Instance != null && LanSessionManager.Instance.IsRoomOpenOnLan;
            if (!isRoomOpen)
            {
                ShowSafeCheckpointToast("PUNTO SEGURO (INICIO DE NOCHE) · Pulsa [ESC] si deseas abrir la sala a un 2.º jugador en LAN sin perder progreso · Pulsa [Q] para alternar época (1929 / Ruinas)");
            }
        }

        private void OnNightStartedCheckpoint(NightDefinition def, int nightIdx)
        {
            bool isRoomOpen = LanSessionManager.Instance != null && LanSessionManager.Instance.IsRoomOpenOnLan;
            if (!isRoomOpen)
            {
                string roman = def != null ? def.roman : (nightIdx + 1).ToString();
                ShowSafeCheckpointToast($"PUNTO SEGURO — NOCHE {roman} · Puedes abrir la sala en LAN desde el menú de pausa [ESC] para que entre un 2.º jugador conservando todo tu avance");
            }
            else if (LanSessionManager.Instance != null && LanSessionManager.Instance.ConnectedPlayerCount < 2)
            {
                string roman = def != null ? def.roman : (nightIdx + 1).ToString();
                ShowSafeCheckpointToast($"SALA ABIERTA EN LAN (1/2) — NOCHE {roman} · Esperando al 2.º jugador (Época Ruinas) en la red local…");
            }
        }

        private void OnPlayerCountChanged(int count)
        {
            if (count >= 2)
            {
                ShowSafeCheckpointToast("2.º JUGADOR CONECTADO (2/2) · Anfitrión fijado en 1929 y Cliente en Ruinas · Tecla [Q] desactivada en cooperativo");
            }
            else
            {
                ShowSafeCheckpointToast("JUGADOR EN SOLITARIO (1/2) · Cambio de época con [Q] reactivado");
            }

            if (isOpen)
            {
                RefreshStatusTexts();
            }
        }

        private void OnRoomOpenedInGameHandler()
        {
            if (isOpen)
            {
                RefreshStatusTexts();
            }
        }

        private void OnEraChangedHandler(EraVisual newEra)
        {
            if (isOpen)
            {
                RefreshStatusTexts();
            }
        }

        private void Update()
        {
            if (checkpointToastGroup != null && checkpointToastGroup.alpha > 0f)
            {
                if (checkpointToastTimer > 0f)
                {
                    checkpointToastTimer -= Time.unscaledDeltaTime;
                }
                else
                {
                    checkpointToastGroup.alpha = Mathf.MoveTowards(checkpointToastGroup.alpha, 0f, Time.unscaledDeltaTime * 1.6f);
                }
            }

            bool escPressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                escPressed = true;
            }
#else
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                escPressed = true;
            }
#endif
            if (escPressed)
            {
                // Si el visor de documentos está abierto o se acaba de cerrar en este mismo frame con ESC, no abrir pausa
                if (DocumentUI.Instance != null && (DocumentUI.Instance.IsOpen || DocumentUI.Instance.LastClosedFrame == Time.frameCount))
                {
                    return;
                }
                if (NightManager.Instance != null && NightManager.Instance.IsTransitioning)
                {
                    return;
                }

                if (isOpen)
                {
                    ClosePauseMenu();
                }
                else
                {
                    OpenPauseMenu();
                }
            }
        }

        public void OpenPauseMenu()
        {
            if (pauseRootPanel == null) return;
            isOpen = true;
            pauseRootPanel.SetActive(true);

            if (cachedPlayerMovement == null) cachedPlayerMovement = FindFirstObjectByType<PlayerMovement>();
            if (cachedPlayerLook == null) cachedPlayerLook = FindFirstObjectByType<PlayerCameraLook>();

            if (cachedPlayerMovement != null) cachedPlayerMovement.enabled = false;
            if (cachedPlayerLook != null)
            {
                cachedPlayerLook.SetCursorLock(false);
                cachedPlayerLook.enabled = false;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            LanSessionManager mgr = LanSessionManager.EnsureInstance();
            if (roomNameInput != null && string.IsNullOrWhiteSpace(roomNameInput.text))
            {
                roomNameInput.text = mgr.ActiveRoomName;
            }

            if (feedbackMessageLabel != null)
            {
                feedbackMessageLabel.text = "";
            }

            RefreshStatusTexts();
        }

        public void ClosePauseMenu()
        {
            if (pauseRootPanel == null) return;
            isOpen = false;
            pauseRootPanel.SetActive(false);

            if (cachedPlayerMovement == null) cachedPlayerMovement = FindFirstObjectByType<PlayerMovement>();
            if (cachedPlayerLook == null) cachedPlayerLook = FindFirstObjectByType<PlayerCameraLook>();

            if (cachedPlayerMovement != null) cachedPlayerMovement.enabled = true;
            if (cachedPlayerLook != null)
            {
                cachedPlayerLook.enabled = true;
                cachedPlayerLook.SetCursorLock(true);
            }
        }

        private void RefreshStatusTexts()
        {
            LanSessionManager mgr = LanSessionManager.EnsureInstance();
            EraVisual currentEra = EraVisualManager.Instance != null ? EraVisualManager.Instance.CurrentEra : EraVisual.Glory1929;

            if (nightProgressLabel != null)
            {
                if (NightManager.Instance != null && NightManager.Instance.NightCount > 0)
                {
                    NightDefinition def = NightManager.Instance.Current;
                    int ev = NightManager.Instance.EvidenceThisNight;
                    int quota = def != null ? def.evidenceQuota : 6;
                    nightProgressLabel.text = $"NOCHE {def.roman} — {def.title}   ·   Evidencias reunidas: {ev} / {quota}";
                }
                else
                {
                    nightProgressLabel.text = "NOCHE I — LA LLEGADA   ·   Progreso activo en la Academia Vesper";
                }
            }

            if (sessionStatusLabel != null)
            {
                if (mgr.ConnectedPlayerCount >= 2)
                {
                    sessionStatusLabel.text = $"ESTADO DE RED: <color=#78C890><b>COOPERATIVO LAN ACTIVO (2/2 JUGADORES)</b></color> · Sala: \"{mgr.ActiveRoomName}\"";
                }
                else if (mgr.IsRoomOpenOnLan)
                {
                    string ipInfo = LanRoomDiscovery.Instance != null ? LanRoomDiscovery.Instance.GetLocalIPv4() : "LAN";
                    sessionStatusLabel.text = $"ESTADO DE RED: <color=#E5C168><b>SALA ABIERTA EN LAN (1/2)</b></color> · Anunciando \"{mgr.ActiveRoomName}\" ({ipInfo}:{mgr.ActivePort})";
                }
                else
                {
                    sessionStatusLabel.text = "ESTADO DE RED: <color=#D8C8B8><b>MODO SOLITARIO (SIN RED)</b></color> · Puedes abrir la sala aquí mismo sin perder progreso";
                }
            }

            if (eraStatusLabel != null)
            {
                string eraName = currentEra == EraVisual.Glory1929
                    ? "<color=#EED69A><b>ÉPOCA 1929 (PALACIO EN GLORIA)</b></color>"
                    : "<color=#9EC0E6><b>ÉPOCA ACTUAL (RUINAS DE TERROR)</b></color>";

                if (mgr.CanLocalPlayerToggleEraWithQ)
                {
                    eraStatusLabel.text = $"ÉPOCA VISUAL ACTUAL: {eraName}   ·   <color=#C8E2B8>Tecla [Q] habilitada (Modo 1 Jugador)</color>";
                }
                else
                {
                    eraStatusLabel.text = $"ÉPOCA VISUAL ACTUAL: {eraName}   ·   <color=#E08A8A>Tecla [Q] desactivada (2/2 jugadores conectados)</color>";
                }
            }

            if (openLanRoomButton != null && openLanRoomButtonLabel != null)
            {
                if (mgr.CurrentMode == LanSessionMode.Client)
                {
                    openLanRoomButton.interactable = false;
                    openLanRoomButtonLabel.text = "CONECTADO COMO INVITADO (ÉPOCA RUINAS)";
                }
                else if (mgr.ConnectedPlayerCount >= 2)
                {
                    openLanRoomButton.interactable = false;
                    openLanRoomButtonLabel.text = "SALA COMPLETA EN COOPERATIVO (2/2 JUGADORES)";
                }
                else if (mgr.IsRoomOpenOnLan)
                {
                    openLanRoomButton.interactable = true;
                    openLanRoomButtonLabel.text = "ACTUALIZAR NOMBRE Y REANUNCIAR SALA EN LAN (1/2)";
                }
                else
                {
                    openLanRoomButton.interactable = true;
                    openLanRoomButtonLabel.text = "ABRIR SALA A 2.º JUGADOR EN LAN (SIN PERDER PROGRESO)";
                }
            }

            if (toggleEraButton != null && toggleEraButtonLabel != null)
            {
                bool canToggle = mgr.CanLocalPlayerToggleEraWithQ;
                toggleEraButton.interactable = canToggle;
                if (canToggle)
                {
                    string targetName = currentEra == EraVisual.Glory1929 ? "ÉPOCA RUINAS" : "ÉPOCA 1929";
                    toggleEraButtonLabel.text = $"ALTERNAR A {targetName} [TECLA Q]";
                }
                else
                {
                    toggleEraButtonLabel.text = "CAMBIO DE ÉPOCA [Q] BLOQUEADO (2/2 JUGADORES)";
                }
            }
        }

        private void OnOpenLanRoomClicked()
        {
            LanSessionManager mgr = LanSessionManager.EnsureInstance();
            string requestedRoom = roomNameInput != null && !string.IsNullOrWhiteSpace(roomNameInput.text)
                ? roomNameInput.text.Trim()
                : "Academia Vesper";

            bool ok = mgr.OpenRoomFromSoloInPlace(requestedRoom, mgr.LocalPlayerName);
            if (ok)
            {
                string localIp = LanRoomDiscovery.Instance != null ? LanRoomDiscovery.Instance.GetLocalIPv4() : "127.0.0.1";
                if (feedbackMessageLabel != null)
                {
                    feedbackMessageLabel.color = new Color(0.55f, 0.88f, 0.65f, 1f);
                    feedbackMessageLabel.text = $"Sala \"{mgr.ActiveRoomName}\" abierta en LAN ({localIp}:{mgr.ActivePort}) conservando todo el progreso. " +
                                                "El 2.º jugador ya puede pulsar \"Unirse a Sala\" en su menú principal (entrará en la época Ruinas).";
                }
                ShowSafeCheckpointToast($"SALA ABIERTA EN LAN: \"{mgr.ActiveRoomName}\" ({localIp}) · Progreso intacto · Esperando al 2.º jugador…");
            }
            else
            {
                if (feedbackMessageLabel != null)
                {
                    feedbackMessageLabel.color = new Color(0.95f, 0.58f, 0.58f, 1f);
                    feedbackMessageLabel.text = "No se puede abrir la sala porque ya estás conectado como cliente invitado.";
                }
            }

            RefreshStatusTexts();
        }

        private void OnToggleEraInsideMenuClicked()
        {
            if (EraVisualManager.Instance == null) return;
            LanSessionManager mgr = LanSessionManager.EnsureInstance();
            if (!mgr.CanLocalPlayerToggleEraWithQ)
            {
                if (feedbackMessageLabel != null)
                {
                    feedbackMessageLabel.color = new Color(0.95f, 0.62f, 0.62f, 1f);
                    feedbackMessageLabel.text = "Hay 2 jugadores conectados (2/2): cada jugador permanece en su época asignada.";
                }
                RefreshStatusTexts();
                return;
            }

            EraVisual nextEra = EraVisualManager.Instance.CurrentEra == EraVisual.Glory1929
                ? EraVisual.Ruins
                : EraVisual.Glory1929;

            EraVisualManager.Instance.SetEra(nextEra, true);
            EraVisualManager.Instance.RefreshBottomEraStatusPill();
            RefreshStatusTexts();
        }

        private void OnReturnToMainMenuClicked()
        {
            isOpen = false;
            Time.timeScale = 1f;

            if (LanRoomDiscovery.Instance != null && LanSessionManager.Instance != null && LanSessionManager.Instance.CurrentMode == LanSessionMode.Client)
            {
                LanRoomDiscovery.Instance.NotifyHostClientLeaving(LanSessionManager.Instance.TargetHostIp);
            }
            if (LanSessionManager.Instance != null)
            {
                LanSessionManager.Instance.ShutdownNetworkSession();
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene("MainMenu");
        }

        public void ShowSafeCheckpointToast(string message)
        {
            if (checkpointToastGroup == null || checkpointToastText == null) return;
            checkpointToastText.text = message;
            checkpointToastGroup.alpha = 1f;
            checkpointToastTimer = 5.2f;
        }

        private void BuildPauseMenuCanvas()
        {
            GameObject canvasObj = new GameObject("InGamePause_Canvas");
            canvasObj.transform.SetParent(transform, false);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 120;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // 1. Toast de Punto Seguro (parte inferior central)
            GameObject toastObj = new GameObject("SafeCheckpointToast");
            toastObj.transform.SetParent(canvasObj.transform, false);
            RectTransform toastRt = toastObj.AddComponent<RectTransform>();
            toastRt.anchorMin = new Vector2(0.5f, 0f);
            toastRt.anchorMax = new Vector2(0.5f, 0f);
            toastRt.pivot = new Vector2(0.5f, 0f);
            toastRt.anchoredPosition = new Vector2(0f, 72f);
            toastRt.sizeDelta = new Vector2(980f, 48f);

            Image toastBorder = toastObj.AddComponent<Image>();
            toastBorder.color = ColGoldBorder;
            toastBorder.raycastTarget = false;

            checkpointToastGroup = toastObj.AddComponent<CanvasGroup>();
            checkpointToastGroup.alpha = 0f;
            checkpointToastGroup.blocksRaycasts = false;

            GameObject toastInner = new GameObject("Inner");
            toastInner.transform.SetParent(toastObj.transform, false);
            RectTransform toastInnerRt = toastInner.AddComponent<RectTransform>();
            toastInnerRt.anchorMin = Vector2.zero;
            toastInnerRt.anchorMax = Vector2.one;
            toastInnerRt.offsetMin = new Vector2(1.5f, 1.5f);
            toastInnerRt.offsetMax = new Vector2(-1.5f, -1.5f);

            Image toastBg = toastInner.AddComponent<Image>();
            toastBg.color = new Color(0.06f, 0.03f, 0.05f, 0.93f);
            toastBg.raycastTarget = false;

            GameObject toastTextObj = new GameObject("Text");
            toastTextObj.transform.SetParent(toastInner.transform, false);
            RectTransform toastTextRt = toastTextObj.AddComponent<RectTransform>();
            toastTextRt.anchorMin = Vector2.zero;
            toastTextRt.anchorMax = Vector2.one;
            toastTextRt.offsetMin = new Vector2(16f, 4f);
            toastTextRt.offsetMax = new Vector2(-16f, -4f);

            checkpointToastText = toastTextObj.AddComponent<TextMeshProUGUI>();
            checkpointToastText.fontSize = 15f;
            checkpointToastText.color = ColIvoryText;
            checkpointToastText.alignment = TextAlignmentOptions.Center;
            checkpointToastText.raycastTarget = false;

            // 2. Panel raíz del Menú de Pausa [ESC]
            pauseRootPanel = new GameObject("PauseMenu_Backdrop");
            pauseRootPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform rootRt = pauseRootPanel.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            Image backdropImg = pauseRootPanel.AddComponent<Image>();
            backdropImg.color = ColBackdrop;

            // Tarjeta central estilo Gran Ópera
            GameObject cardBorder = new GameObject("OperaPauseCard_Border");
            cardBorder.transform.SetParent(pauseRootPanel.transform, false);
            RectTransform cardBorderRt = cardBorder.AddComponent<RectTransform>();
            cardBorderRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardBorderRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardBorderRt.pivot = new Vector2(0.5f, 0.5f);
            cardBorderRt.sizeDelta = new Vector2(820f, 650f);

            Image borderImg = cardBorder.AddComponent<Image>();
            borderImg.color = ColGoldBorder;

            GameObject cardFill = new GameObject("OperaPauseCard_Fill");
            cardFill.transform.SetParent(cardBorder.transform, false);
            RectTransform cardFillRt = cardFill.AddComponent<RectTransform>();
            cardFillRt.anchorMin = Vector2.zero;
            cardFillRt.anchorMax = Vector2.one;
            cardFillRt.offsetMin = new Vector2(2f, 2f);
            cardFillRt.offsetMax = new Vector2(-2f, -2f);

            Image fillImg = cardFill.AddComponent<Image>();
            fillImg.color = ColCardBg;

            // Cabecera
            CreateLabel(cardFill.transform, "Eyebrow", "ACADEMIA VESPER · PAUSA Y SALA COOPERATIVA LAN", 14f, ColGoldBright, FontStyles.Bold,
                new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(740f, 24f));

            CreateLabel(cardFill.transform, "MainTitle", "GESTIÓN DE ÉPOCA Y SEGUNDO JUGADOR", 28f, ColIvoryText, FontStyles.Bold,
                new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(740f, 38f));

            // Línea divisoria dorada
            GameObject div = new GameObject("GoldDivider");
            div.transform.SetParent(cardFill.transform, false);
            RectTransform divRt = div.AddComponent<RectTransform>();
            divRt.anchorMin = new Vector2(0.5f, 1f);
            divRt.anchorMax = new Vector2(0.5f, 1f);
            divRt.pivot = new Vector2(0.5f, 0.5f);
            divRt.anchoredPosition = new Vector2(0f, -104f);
            divRt.sizeDelta = new Vector2(700f, 1.5f);
            Image divImg = div.AddComponent<Image>();
            divImg.color = new Color(0.85f, 0.71f, 0.41f, 0.45f);

            // Etiquetas de estado de Noche, Red y Época
            nightProgressLabel = CreateLabel(cardFill.transform, "NightProgress", "", 16.5f, ColGoldBright, FontStyles.Bold,
                new Vector2(0.5f, 1f), new Vector2(0f, -132f), new Vector2(730f, 28f));

            sessionStatusLabel = CreateLabel(cardFill.transform, "SessionStatus", "", 15.5f, ColIvoryText, FontStyles.Normal,
                new Vector2(0.5f, 1f), new Vector2(0f, -164f), new Vector2(730f, 28f));

            eraStatusLabel = CreateLabel(cardFill.transform, "EraStatus", "", 15.5f, ColIvoryText, FontStyles.Normal,
                new Vector2(0.5f, 1f), new Vector2(0f, -196f), new Vector2(730f, 28f));

            // Caja para abrir sala en LAN desde Modo Solo sin perder progreso
            GameObject lanBox = new GameObject("LanOpenBox");
            lanBox.transform.SetParent(cardFill.transform, false);
            RectTransform lanBoxRt = lanBox.AddComponent<RectTransform>();
            lanBoxRt.anchorMin = new Vector2(0.5f, 1f);
            lanBoxRt.anchorMax = new Vector2(0.5f, 1f);
            lanBoxRt.pivot = new Vector2(0.5f, 1f);
            lanBoxRt.anchoredPosition = new Vector2(0f, -238f);
            lanBoxRt.sizeDelta = new Vector2(720f, 152f);

            Image lanBoxBg = lanBox.AddComponent<Image>();
            lanBoxBg.color = new Color(0.12f, 0.06f, 0.08f, 0.92f);

            CreateLabel(lanBox.transform, "LanBoxTitle",
                "ENTRADA DE 2.º JUGADOR EN PUNTO SEGURO (CONSERVA NOCHE, LLAVES Y PUERTAS)",
                14f, ColGoldBright, FontStyles.Bold,
                new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(680f, 22f));

            CreateLabel(lanBox.transform, "RoomInputLabel", "Nombre de la Sala en LAN:", 14.5f, ColMutedRose, FontStyles.Normal,
                new Vector2(0f, 1f), new Vector2(24f, -56f), new Vector2(210f, 36f), TextAlignmentOptions.Left);

            roomNameInput = CreateInputField(lanBox.transform, "RoomNameInput", "Academia Vesper",
                new Vector2(0f, 1f), new Vector2(225f, -52f), new Vector2(470f, 38f));

            openLanRoomButton = CreateOperaButton(lanBox.transform, "BtnOpenLanRoom",
                "ABRIR SALA A 2.º JUGADOR EN LAN (SIN PERDER PROGRESO)",
                new Vector2(0.5f, 1f), new Vector2(0f, -104f), new Vector2(672f, 44f),
                out openLanRoomButtonLabel, OnOpenLanRoomClicked);

            // Mensaje de confirmación / feedback
            feedbackMessageLabel = CreateLabel(cardFill.transform, "FeedbackMsg", "", 14.5f, new Color(0.60f, 0.90f, 0.68f, 1f), FontStyles.Italic,
                new Vector2(0.5f, 1f), new Vector2(0f, -404f), new Vector2(720f, 42f));

            // Botones de acción inferiores
            toggleEraButton = CreateOperaButton(cardFill.transform, "BtnToggleEra",
                "ALTERNAR ÉPOCA (1929 / RUINAS) [TECLA Q]",
                new Vector2(0.5f, 1f), new Vector2(0f, -456f), new Vector2(672f, 46f),
                out toggleEraButtonLabel, OnToggleEraInsideMenuClicked);

            CreateOperaButton(cardFill.transform, "BtnResume",
                "REANUDAR INVESTIGACIÓN [ESC]",
                new Vector2(0.5f, 1f), new Vector2(0f, -514f), new Vector2(672f, 46f),
                out _, ClosePauseMenu);

            CreateOperaButton(cardFill.transform, "BtnMainMenu",
                "VOLVER AL MENÚ PRINCIPAL",
                new Vector2(0.5f, 1f), new Vector2(0f, -572f), new Vector2(672f, 44f),
                out _, OnReturnToMainMenuClicked);

            pauseRootPanel.SetActive(false);
        }

        private static TextMeshProUGUI CreateLabel(
            Transform parent,
            string name,
            string text,
            float fontSize,
            Color color,
            FontStyles style,
            Vector2 anchor,
            Vector2 anchoredPos,
            Vector2 size,
            TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(anchor.x, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static TMP_InputField CreateInputField(
            Transform parent,
            string name,
            string defaultText,
            Vector2 anchor,
            Vector2 anchoredPos,
            Vector2 size)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            RectTransform rt = root.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image bg = root.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.03f, 0.04f, 0.96f);

            GameObject textArea = new GameObject("TextArea");
            textArea.transform.SetParent(root.transform, false);
            RectTransform areaRt = textArea.AddComponent<RectTransform>();
            areaRt.anchorMin = Vector2.zero;
            areaRt.anchorMax = Vector2.one;
            areaRt.offsetMin = new Vector2(12f, 4f);
            areaRt.offsetMax = new Vector2(-12f, -4f);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(textArea.transform, false);
            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
            textComp.fontSize = 15.5f;
            textComp.color = ColIvoryText;
            textComp.alignment = TextAlignmentOptions.MidlineLeft;

            TMP_InputField input = root.AddComponent<TMP_InputField>();
            input.textViewport = areaRt;
            input.textComponent = textComp;
            input.text = defaultText;
            return input;
        }

        private static Button CreateOperaButton(
            Transform parent,
            string name,
            string caption,
            Vector2 anchor,
            Vector2 anchoredPos,
            Vector2 size,
            out TextMeshProUGUI labelOut,
            UnityEngine.Events.UnityAction onClick)
        {
            GameObject borderObj = new GameObject(name);
            borderObj.transform.SetParent(parent, false);
            RectTransform borderRt = borderObj.AddComponent<RectTransform>();
            borderRt.anchorMin = anchor;
            borderRt.anchorMax = anchor;
            borderRt.pivot = new Vector2(0.5f, 1f);
            borderRt.anchoredPosition = anchoredPos;
            borderRt.sizeDelta = size;

            Image borderImg = borderObj.AddComponent<Image>();
            borderImg.color = ColGoldBorder;

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(borderObj.transform, false);
            RectTransform fillRt = fillObj.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(1.5f, 1.5f);
            fillRt.offsetMax = new Vector2(-1.5f, -1.5f);

            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.color = ColBtnNormal;

            Button btn = borderObj.AddComponent<Button>();
            btn.targetGraphic = fillImg;
            ColorBlock cb = btn.colors;
            cb.normalColor = ColBtnNormal;
            cb.highlightedColor = ColBtnHover;
            cb.pressedColor = new Color(0.36f, 0.14f, 0.20f, 1f);
            cb.selectedColor = ColBtnHover;
            cb.disabledColor = ColBtnDisabled;
            btn.colors = cb;

            if (onClick != null)
            {
                btn.onClick.AddListener(onClick);
            }

            GameObject txtObj = new GameObject("Label");
            txtObj.transform.SetParent(fillObj.transform, false);
            RectTransform txtRt = txtObj.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(12f, 2f);
            txtRt.offsetMax = new Vector2(-12f, -2f);

            labelOut = txtObj.AddComponent<TextMeshProUGUI>();
            labelOut.text = caption;
            labelOut.fontSize = 15.5f;
            labelOut.fontStyle = FontStyles.Bold;
            labelOut.color = ColIvoryText;
            labelOut.alignment = TextAlignmentOptions.Center;
            labelOut.raycastTarget = false;

            return btn;
        }
    }
}
