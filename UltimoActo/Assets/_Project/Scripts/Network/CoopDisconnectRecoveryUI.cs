using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UltimoActo.Core;
using UltimoActo.Player;
using UltimoActo.UI;

namespace UltimoActo.Network
{
    /// <summary>
    /// Paso 6 — Desconexión y Reconexión Sin Perder Progreso:
    /// Si un jugador se cae en mitad de la partida:
    ///  1. El Host conserva el guardado de la noche y todo el progreso de llaves, puertas, documentos y puzles.
    ///  2. Se activa automáticamente una Pausa de Espera (overlay Gran Ópera) indicando qué ocurrió:
    ///     - Si eres el Host (1929): la sala sigue abierta y anunciándose en la LAN; en cuanto el 2.º jugador
    ///       vuelve a unirse, la pausa se cierra sola y el cliente recibe el snapshot completo del progreso actual.
    ///     - Si eres el Cliente (Ruinas): puedes pulsar "REINTENTAR RECONEXIÓN A LA SALA" (además de reintentos
    ///       automáticos periódicos), continuar en solitario en el mismo punto (con [Q] reactivado) o volver al Menú.
    /// </summary>
    public class CoopDisconnectRecoveryUI : MonoBehaviour
    {
        public static CoopDisconnectRecoveryUI Instance { get; private set; }

        public bool IsRecoveryModalOpen => isModalOpen;

        private bool isModalOpen;
        private GameObject modalBackdrop;
        private TextMeshProUGUI titleLabel;
        private TextMeshProUGUI reasonLabel;
        private TextMeshProUGUI preservedStateLabel;
        private TextMeshProUGUI waitingStatusLabel;
        private TextMeshProUGUI primaryActionBtnLabel;

        private float waitElapsedSeconds;
        private float nextAutoReconnectAttempt;
        private PlayerMovement cachedPlayerMovement;
        private PlayerCameraLook cachedPlayerLook;

        // Paleta Gran Ópera Vesper
        private static readonly Color ColBackdrop = new Color(0.02f, 0.01f, 0.03f, 0.88f);
        private static readonly Color ColCardBg = new Color(0.09f, 0.04f, 0.06f, 0.98f);
        private static readonly Color ColGoldBorder = new Color(0.85f, 0.71f, 0.41f, 0.88f);
        private static readonly Color ColGoldBright = new Color(0.95f, 0.84f, 0.58f, 1f);
        private static readonly Color ColIvoryText = new Color(0.96f, 0.94f, 0.90f, 1f);
        private static readonly Color ColBtnNormal = new Color(0.18f, 0.07f, 0.11f, 0.96f);
        private static readonly Color ColBtnHover = new Color(0.30f, 0.12f, 0.17f, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoadedBootstrap;
            SceneManager.sceneLoaded += OnSceneLoadedBootstrap;
            TrySpawnInActiveScene();
        }

        private static void OnSceneLoadedBootstrap(Scene scene, LoadSceneMode mode)
        {
            TrySpawnInActiveScene();
        }

        private static void TrySpawnInActiveScene()
        {
            if (Instance != null) return;
            if (FindFirstObjectByType<CoopDisconnectRecoveryUI>() != null) return;
            if (FindFirstObjectByType<PlayerMovement>() == null) return;

            GameObject go = new GameObject("CoopDisconnectRecoveryUI");
            go.AddComponent<CoopDisconnectRecoveryUI>();
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

            BuildRecoveryModalCanvas();

            if (LanSessionManager.Instance != null)
            {
                LanSessionManager.Instance.OnPartnerDisconnected += OnPartnerDisconnectedHandler;
                LanSessionManager.Instance.OnPartnerReconnected += OnPartnerReconnectedHandler;
            }
        }

        private void OnDestroy()
        {
            if (LanSessionManager.Instance != null)
            {
                LanSessionManager.Instance.OnPartnerDisconnected -= OnPartnerDisconnectedHandler;
                LanSessionManager.Instance.OnPartnerReconnected -= OnPartnerReconnectedHandler;
            }
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Atajo de prueba [F3] para simular la caída inesperada del otro jugador en un solo equipo
            bool f3Pressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame)
            {
                f3Pressed = true;
            }
#else
            if (Input.GetKeyDown(KeyCode.F3))
            {
                f3Pressed = true;
            }
#endif
            if (f3Pressed)
            {
                SimulateUnexpectedDisconnectForTesting();
            }
#endif

            if (!isModalOpen) return;

            waitElapsedSeconds += Time.unscaledDeltaTime;
            UpdateWaitingStatusAndAutoReconnect();
        }

        public void SimulateUnexpectedDisconnectForTesting()
        {
            LanSessionManager mgr = LanSessionManager.EnsureInstance();
            if (mgr.ConnectedPlayerCount < 2)
            {
                // Si estaba en 1/2, primero pasa a 2/2 y enseguida simula la caída para abrir la Pausa de Espera
                mgr.SetConnectedPlayerCount(2);
            }
            mgr.SetConnectedPlayerCount(1);
        }

        private void OnPartnerDisconnectedHandler(string reason)
        {
            // Asegurar que el Host persista el guardado inmediatamente
            if (NightManager.Instance != null)
            {
                NightManager.Instance.HostPersistCurrentNightSave();
            }

            // BLOQUE 3.6 — Si la secuencia final / amanecer / créditos está en curso, NO congelar la partida
            // ni bloquear la pantalla con el modal de espera: permitir que este jugador termine el final y vuelva al menú.
            if (UltimoActo.Narrative.UltimateFinaleAndCreditsController.IsFinaleRunning)
            {
                if (NightManager.Instance != null)
                {
                    NightManager.Instance.ShowToast("Tu compañero se ha desconectado durante el desenlace. Puedes terminar los créditos y volver al menú.", 4.5f);
                }
                return;
            }

            // Si el menú de pausa normal estaba abierto, cerrarlo para mostrar la pausa de desconexión
            if (InGamePauseMenuUI.Instance != null && InGamePauseMenuUI.Instance.IsOpen)
            {
                InGamePauseMenuUI.Instance.ClosePauseMenu();
            }

            OpenRecoveryModal(reason);
        }

        private void OnPartnerReconnectedHandler()
        {
            if (!isModalOpen) return;

            CloseRecoveryModal();

            // Si somos el Host, reenviar el snapshot completo de la partida al compañero reconectado
            if (SharedWorldStateSync.Instance != null)
            {
                SharedWorldStateSync.Instance.SendFullSnapshotFromHostToAllClients();
            }

            if (NightManager.Instance != null)
            {
                NightManager.Instance.ShowToast("RECONEXIÓN COMPLETADA (2/2) — Investigación cooperativa reanudada sin pérdida de progreso.", 5.0f);
            }
        }

        public void OpenRecoveryModal(string disconnectReason)
        {
            if (modalBackdrop == null) return;

            isModalOpen = true;
            waitElapsedSeconds = 0f;
            nextAutoReconnectAttempt = 4.0f;
            modalBackdrop.SetActive(true);

            // Pausar tiempo y liberar ratón para que el jugador no sufra ningún bloqueo mientras espera
            Time.timeScale = 0f;

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
            bool isClient = mgr.CurrentMode == LanSessionMode.Client;

            if (titleLabel != null)
            {
                titleLabel.text = isClient
                    ? "CONEXIÓN INTERRUMPIDA CON EL ANFITRIÓN (PAUSA DE ESPERA)"
                    : "COMPAÑERO DESCONECTADO — PAUSA DE ESPERA ACTIVA";
            }

            if (reasonLabel != null)
            {
                reasonLabel.text = string.IsNullOrEmpty(disconnectReason)
                    ? "Se ha interrumpido la comunicación en la red local."
                    : disconnectReason;
            }

            if (preservedStateLabel != null)
            {
                string nightInfo = "Noche I";
                int evCount = 0;
                int quota = 6;
                if (NightManager.Instance != null && NightManager.Instance.Current != null)
                {
                    nightInfo = $"Noche {NightManager.Instance.Current.roman} — {NightManager.Instance.Current.title}";
                    evCount = NightManager.Instance.EvidenceThisNight;
                    quota = NightManager.Instance.Current.evidenceQuota;
                }
                int keyCount = SimpleInventory.GlobalKeys != null ? SimpleInventory.GlobalKeys.Count : 0;

                preservedStateLabel.text =
                    $"PROGRESO CONSERVADO AL 100 %:  <b>{nightInfo}</b>   ·   Evidencias: <b>{evCount}/{quota}</b>   ·   Llaves/Objetos: <b>{keyCount}</b>\n" +
                    (isClient
                        ? "El anfitrión conserva el guardado. Si reconectas, recuperarás exactamente este estado."
                        : $"El guardado del anfitrión está seguro. La sala «{mgr.ActiveRoomName}» sigue abierta para que tu compañero vuelva a entrar.");
            }

            if (primaryActionBtnLabel != null)
            {
                primaryActionBtnLabel.text = isClient
                    ? "REINTENTAR RECONEXIÓN A LA SALA AHORA"
                    : "REANUNCIAR SALA EN LAN Y SEGUIR ESPERANDO";
            }

            UpdateWaitingStatusAndAutoReconnect();
        }

        public void CloseRecoveryModal()
        {
            if (modalBackdrop == null) return;

            isModalOpen = false;
            modalBackdrop.SetActive(false);
            Time.timeScale = 1f;

            if (cachedPlayerMovement == null) cachedPlayerMovement = FindFirstObjectByType<PlayerMovement>();
            if (cachedPlayerLook == null) cachedPlayerLook = FindFirstObjectByType<PlayerCameraLook>();

            if (cachedPlayerMovement != null) cachedPlayerMovement.enabled = true;
            if (cachedPlayerLook != null)
            {
                cachedPlayerLook.enabled = true;
                cachedPlayerLook.SetCursorLock(true);
            }
        }

        private void UpdateWaitingStatusAndAutoReconnect()
        {
            LanSessionManager mgr = LanSessionManager.EnsureInstance();
            bool isClient = mgr.CurrentMode == LanSessionMode.Client;
            int secs = Mathf.FloorToInt(waitElapsedSeconds);

            if (isClient)
            {
                if (waitElapsedSeconds >= nextAutoReconnectAttempt)
                {
                    nextAutoReconnectAttempt = waitElapsedSeconds + 5.0f;
                    mgr.TryReconnectClientInPlace();
                }

                if (waitingStatusLabel != null)
                {
                    int nextIn = Mathf.Max(1, Mathf.CeilToInt(nextAutoReconnectAttempt - waitElapsedSeconds));
                    waitingStatusLabel.text = $"Buscando de nuevo al anfitrión ({mgr.TargetHostIp}:{mgr.ActivePort})… Tiempo en espera: {secs} s (Próximo reintento automático en {nextIn} s)";
                }
            }
            else
            {
                if (!mgr.IsRoomOpenOnLan)
                {
                    mgr.OpenRoomFromSoloInPlace(mgr.ActiveRoomName, mgr.LocalPlayerName);
                }

                if (waitingStatusLabel != null)
                {
                    string ip = LanRoomDiscovery.Instance != null ? LanRoomDiscovery.Instance.GetLocalIPv4() : "127.0.0.1";
                    waitingStatusLabel.text = $"Esperando reconexión del 2.º investigador en \"{mgr.ActiveRoomName}\" ({ip}:{mgr.ActivePort})… Tiempo en espera: {secs} s";
                }
            }
        }

        private void OnPrimaryReconnectOrReannounceClicked()
        {
            LanSessionManager mgr = LanSessionManager.EnsureInstance();
            if (mgr.CurrentMode == LanSessionMode.Client)
            {
                nextAutoReconnectAttempt = waitElapsedSeconds + 5.0f;
                bool ok = mgr.TryReconnectClientInPlace();
                if (waitingStatusLabel != null && !ok)
                {
                    waitingStatusLabel.text = $"Intentando reconectar con {mgr.TargetHostIp}:{mgr.ActivePort}… Verifica que el anfitrión siga con la sala abierta.";
                }
            }
            else
            {
                mgr.OpenRoomFromSoloInPlace(mgr.ActiveRoomName, mgr.LocalPlayerName);
                if (waitingStatusLabel != null)
                {
                    string ip = LanRoomDiscovery.Instance != null ? LanRoomDiscovery.Instance.GetLocalIPv4() : "127.0.0.1";
                    waitingStatusLabel.text = $"Anuncio UDP enviado de nuevo en la red local ({ip}:{mgr.ActivePort}). Esperando a que el 2.º jugador pulse 'Unirse a Sala'…";
                }
            }
        }

        private void OnContinueSoloWithoutLosingProgressClicked()
        {
            LanSessionManager mgr = LanSessionManager.EnsureInstance();
            mgr.ContinueInSoloAfterDisconnect();
            CloseRecoveryModal();

            if (EraVisualManager.Instance != null)
            {
                EraVisualManager.Instance.RefreshBottomEraStatusPill();
            }

            if (NightManager.Instance != null)
            {
                NightManager.Instance.ShowToast("CONTINUANDO EN SOLITARIO (1/2) — Progreso conservado · Tecla [Q] de cambio de época reactivada.", 5.0f);
            }
        }

        private void OnReturnToMainMenuSavingProgressClicked()
        {
            if (NightManager.Instance != null)
            {
                NightManager.Instance.HostPersistCurrentNightSave();
            }

            isModalOpen = false;
            Time.timeScale = 1f;

            if (LanSessionManager.Instance != null)
            {
                LanSessionManager.Instance.ShutdownNetworkSession();
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene("MainMenu");
        }

        private void BuildRecoveryModalCanvas()
        {
            GameObject canvasObj = new GameObject("CoopDisconnectRecovery_Canvas");
            canvasObj.transform.SetParent(transform, false);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 145;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            modalBackdrop = new GameObject("DisconnectWait_Backdrop");
            modalBackdrop.transform.SetParent(canvasObj.transform, false);
            RectTransform rootRt = modalBackdrop.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            Image bgImg = modalBackdrop.AddComponent<Image>();
            bgImg.color = ColBackdrop;

            // Tarjeta central Gran Ópera
            GameObject cardBorder = new GameObject("RecoveryCard_Border");
            cardBorder.transform.SetParent(modalBackdrop.transform, false);
            RectTransform borderRt = cardBorder.AddComponent<RectTransform>();
            borderRt.anchorMin = new Vector2(0.5f, 0.5f);
            borderRt.anchorMax = new Vector2(0.5f, 0.5f);
            borderRt.pivot = new Vector2(0.5f, 0.5f);
            borderRt.sizeDelta = new Vector2(820f, 520f);

            Image borderImg = cardBorder.AddComponent<Image>();
            borderImg.color = ColGoldBorder;

            GameObject cardFill = new GameObject("RecoveryCard_Fill");
            cardFill.transform.SetParent(cardBorder.transform, false);
            RectTransform fillRt = cardFill.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(2f, 2f);
            fillRt.offsetMax = new Vector2(-2f, -2f);

            Image fillImg = cardFill.AddComponent<Image>();
            fillImg.color = ColCardBg;

            CreateLabel(cardFill.transform, "Eyebrow",
                "ACADEMIA VESPER · PROTECCIÓN DE SESIÓN COOPERATIVA LAN",
                13.5f, ColGoldBright, FontStyles.Bold,
                new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(740f, 22f));

            titleLabel = CreateLabel(cardFill.transform, "Title",
                "COMPAÑERO DESCONECTADO — PAUSA DE ESPERA ACTIVA",
                24f, ColIvoryText, FontStyles.Bold,
                new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(750f, 34f));

            reasonLabel = CreateLabel(cardFill.transform, "Reason",
                "El segundo investigador se ha desconectado de la sala.",
                15.5f, new Color(0.95f, 0.72f, 0.68f, 1f), FontStyles.Normal,
                new Vector2(0.5f, 1f), new Vector2(0f, -104f), new Vector2(740f, 28f));

            // Caja de resumen de progreso conservado
            GameObject stateBox = new GameObject("PreservedStateBox");
            stateBox.transform.SetParent(cardFill.transform, false);
            RectTransform boxRt = stateBox.AddComponent<RectTransform>();
            boxRt.anchorMin = new Vector2(0.5f, 1f);
            boxRt.anchorMax = new Vector2(0.5f, 1f);
            boxRt.pivot = new Vector2(0.5f, 1f);
            boxRt.anchoredPosition = new Vector2(0f, -144f);
            boxRt.sizeDelta = new Vector2(730f, 88f);

            Image boxBg = stateBox.AddComponent<Image>();
            boxBg.color = new Color(0.13f, 0.06f, 0.09f, 0.94f);

            preservedStateLabel = CreateLabel(stateBox.transform, "PreservedText",
                "", 14.5f, ColIvoryText, FontStyles.Normal,
                new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(690f, 64f));

            waitingStatusLabel = CreateLabel(cardFill.transform, "WaitingStatus",
                "Esperando reconexión en la red local…",
                14f, ColGoldBright, FontStyles.Italic,
                new Vector2(0.5f, 1f), new Vector2(0f, -248f), new Vector2(740f, 28f));

            // Botón 1: Reintentar reconexión / Reanunciar sala
            CreateOperaButton(cardFill.transform, "BtnReconnect",
                "REINTENTAR RECONEXIÓN A LA SALA",
                new Vector2(0.5f, 1f), new Vector2(0f, -296f), new Vector2(680f, 46f),
                out primaryActionBtnLabel, OnPrimaryReconnectOrReannounceClicked);

            // Botón 2: Continuar en solitario sin perder progreso (reactiva [Q])
            CreateOperaButton(cardFill.transform, "BtnContinueSolo",
                "CONTINUAR PARTIDA AQUÍ MISMO (SIN PERDER PROGRESO · REACTIVA [Q])",
                new Vector2(0.5f, 1f), new Vector2(0f, -356f), new Vector2(680f, 46f),
                out _, OnContinueSoloWithoutLosingProgressClicked);

            // Botón 3: Volver al menú principal conservando el guardado del Host
            CreateOperaButton(cardFill.transform, "BtnReturnMenu",
                "GUARDAR Y VOLVER AL MENÚ PRINCIPAL",
                new Vector2(0.5f, 1f), new Vector2(0f, -416f), new Vector2(680f, 44f),
                out _, OnReturnToMainMenuSavingProgressClicked);

            CreateLabel(cardFill.transform, "FooterHint",
                "La partida se reanudará automáticamente en cuanto el otro jugador vuelva a conectarse.",
                12.5f, new Color(0.76f, 0.72f, 0.68f, 0.90f), FontStyles.Normal,
                new Vector2(0.5f, 1f), new Vector2(0f, -476f), new Vector2(720f, 22f));

            modalBackdrop.SetActive(false);
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
            Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            return tmp;
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
            cb.pressedColor = new Color(0.38f, 0.14f, 0.20f, 1f);
            cb.selectedColor = ColBtnHover;
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
            labelOut.fontSize = 15f;
            labelOut.fontStyle = FontStyles.Bold;
            labelOut.color = ColIvoryText;
            labelOut.alignment = TextAlignmentOptions.Center;
            labelOut.raycastTarget = false;

            return btn;
        }
    }
}
