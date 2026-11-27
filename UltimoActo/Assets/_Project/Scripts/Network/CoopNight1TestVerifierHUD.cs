using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UltimoActo.Core;
using UltimoActo.Interaction;
using UltimoActo.Player;
using UltimoActo.Puzzles;
using UltimoActo.UI;

namespace UltimoActo.Network
{
    /// <summary>
    /// Paso 7 — Pruebas Integrales de la Noche I Completa (En un solo equipo y entre dos equipos):
    /// - Panel de verificación interactivo en tiempo real (mostrar/ocultar con [F4]) que monitoriza
    ///   todos los hitos de la Noche I ("La Llegada"), el estado de la conexión LAN (1/2 o 2/2),
    ///   las dos épocas visuales (1929 / Ruinas), la presencia/linterna del compañero y el chat de voz.
    /// - Incluye botones de navegación rápida por los puntos clave de la Noche I (Portón/Patio,
    ///   Vestíbulo/Cuadro Eléctrico, Fusible 10A en Vestuario, Fusible 15A y Llave 2 en Taquillas,
    ///   Fusible 25A y Llave 1 en Oficina de Dirección, Archivo de Expedientes y Galería de Retratos)
    ///   para poder recorrer y validar toda la Noche I cómodamente en 2 instancias.
    /// </summary>
    public class CoopNight1TestVerifierHUD : MonoBehaviour
    {
        public static CoopNight1TestVerifierHUD Instance { get; private set; }

        private GameObject verifierPanel;
        private TextMeshProUGUI checklistBodyText;
        private bool isVisible = false;

        private static readonly Color ColCardBg = new Color(0.06f, 0.03f, 0.05f, 0.94f);
        private static readonly Color ColGoldBorder = new Color(0.85f, 0.71f, 0.41f, 0.85f);
        private static readonly Color ColGoldBright = new Color(0.95f, 0.84f, 0.58f, 1f);
        private static readonly Color ColIvoryText = new Color(0.96f, 0.94f, 0.90f, 1f);
        private static readonly Color ColBtnNormal = new Color(0.16f, 0.07f, 0.10f, 0.95f);

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
            if (FindFirstObjectByType<CoopNight1TestVerifierHUD>() != null) return;
            if (FindFirstObjectByType<PlayerMovement>() == null) return;

            GameObject go = new GameObject("CoopNight1TestVerifierHUD");
            go.AddComponent<CoopNight1TestVerifierHUD>();
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
            BuildVerifierCanvas();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            bool f4Pressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.f4Key.wasPressedThisFrame)
            {
                f4Pressed = true;
            }
#else
            if (Input.GetKeyDown(KeyCode.F4))
            {
                f4Pressed = true;
            }
#endif
            if (f4Pressed && verifierPanel != null)
            {
                isVisible = !isVisible;
                verifierPanel.SetActive(isVisible);
            }

            if (isVisible)
            {
                RefreshChecklistText();
            }
        }

        public void ToggleVerifierPanel()
        {
            if (verifierPanel == null) return;
            isVisible = !isVisible;
            verifierPanel.SetActive(isVisible);
            if (isVisible) RefreshChecklistText();
        }

        private void RefreshChecklistText()
        {
            if (checklistBodyText == null) return;

            LanSessionManager session = LanSessionManager.EnsureInstance();
            EraVisual currentEra = EraVisualManager.Instance != null ? EraVisualManager.Instance.CurrentEra : EraVisual.Glory1929;
            LanVoiceChatManager voice = LanVoiceChatManager.Instance;
            NightManager nightMgr = NightManager.Instance;
            AcademyFuseBoxController fuseBox = AcademyFuseBoxController.Instance;
            PortraitGalleryPuzzleController portraitPuzzle = PortraitGalleryPuzzleController.Instance;

            bool hasPartner = NetworkCoopPlayer.RemotePlayer != null;
            bool hasKeyArchive = SimpleInventory.GlobalKeys != null && ContainsKey("key_archives");
            bool hasKeyElena = SimpleInventory.GlobalKeys != null && ContainsKey("key_elena_room");
            int fusesCount = fuseBox != null ? fuseBox.InstalledFusesCount : 0;
            bool powerOn = fuseBox != null && fuseBox.IsPowerRestored;
            bool portraitsSolved = portraitPuzzle != null && portraitPuzzle.IsSolved;
            bool porticoCoopSolved = Night1CoopPorticoSplitKeySetup.Instance != null &&
                                     Night1CoopPorticoSplitKeySetup.Instance.PorticoPuzzle != null &&
                                     Night1CoopPorticoSplitKeySetup.Instance.PorticoPuzzle.IsSolved;
            int journalEntries = SharedEvidenceJournalManager.Instance != null
                ? SharedEvidenceJournalManager.Instance.AllEntries.Count
                : 0;
            int achievementsCount = SharedEvidenceJournalManager.Instance != null
                ? SharedEvidenceJournalManager.Instance.UnlockedAchievements.Count
                : 0;

            int nightIdx = nightMgr != null ? nightMgr.CurrentNightIndex : 0;
            string nightRoman = (nightMgr != null && nightMgr.Current != null) ? nightMgr.Current.roman : "I";
            int evCount = nightMgr != null ? nightMgr.EvidenceThisNight : 0;
            int evQuota = (nightMgr != null && nightMgr.Current != null) ? nightMgr.Current.evidenceQuota : 6;

            string chkMode = session.ConnectedPlayerCount >= 2
                ? "<color=#6EE88E>[OK 2/2]</color>"
                : $"<color=#F0C868>[{session.CurrentMode} 1/2]</color>";

            string chkEra = currentEra == EraVisual.Glory1929
                ? "<color=#F3D692>1929 (Gloria)</color>"
                : "<color=#9EC4EC>Ruinas (Actualidad)</color>";

            string chkPartner = hasPartner
                ? $"<color=#6EE88E>[OK] {NetworkCoopPlayer.RemotePlayer.DisplayName}</color>"
                : "<color=#C4B0A4>[Sin 2.º jugador — Pulsa F2 para simular]</color>";

            string chkVoice = (voice != null && !voice.HasMicrophoneIssue)
                ? (voice.IsLocalSpeaking
                    ? "<color=#6EE88E>[TRANSMITIENDO VOZ ●]</color>"
                    : $"<color=#6EE88E>[LISTO: {(voice.IsPushToTalk ? "Tecla V" : "Mic Abierto")}]</color>")
                : "<color=#F08078>[Revisar Micrófono]</color>";

            string chkPorticoCoop = porticoCoopSolved
                ? "<color=#6EE88E>[RESUELTO: I Cisne + III Lira]</color>"
                : "<color=#F0C868>[Pendiente: Pórtico (Columna 1929 + Columna Ruinas)]</color>";

            int colUnl = UltimoActo.Narrative.CollectibleAndSecretManager.Instance != null
                ? UltimoActo.Narrative.CollectibleAndSecretManager.Instance.UnlockedCollectiblesCount
                : 0;
            int secUnl = UltimoActo.Narrative.CollectibleAndSecretManager.Instance != null
                ? UltimoActo.Narrative.CollectibleAndSecretManager.Instance.UnlockedSecretsCount
                : 0;

            string chkJournal = $"<color=#6EE88E>[{journalEntries} Entradas · Colección {colUnl}/32 · ★{secUnl}/8 · J/TAB]</color>";

            string chkFuses = powerOn
                ? "<color=#6EE88E>[COMPLETADO: 220V Restaurados]</color>"
                : $"<color=#F0C868>[{fusesCount}/3 Fusibles Instalados]</color>";

            string chkKey1 = hasKeyArchive ? "<color=#6EE88E>[OBTENIDA]</color>" : "<color=#C4B0A4>[Pendiente: 7. Dirección]</color>";
            string chkKey2 = hasKeyElena ? "<color=#6EE88E>[OBTENIDA]</color>" : "<color=#C4B0A4>[Pendiente: 13. Taquilla Nº14]</color>";
            string chkPortraits = portraitsSolved
                ? "<color=#6EE88E>[RESUELTO: IV-I-III-II]</color>"
                : "<color=#C4B0A4>[Pendiente: 10. Galería Retratos]</color>";
            string chkNight1 = (nightIdx > 0 || evCount >= evQuota)
                ? "<color=#6EE88E>[NOCHE I COMPLETADA]</color>"
                : $"<color=#F0C868>[Evidencias: {evCount} / {evQuota}]</color>";

            checklistBodyText.text =
                $"<b>1. Red y Sesión LAN:</b> {chkMode} · Sala: \"{session.ActiveRoomName}\"\n" +
                $"<b>2. Época Visual Activa:</b> {chkEra} · [Q]: {(session.CanLocalPlayerToggleEraWithQ ? "Habilitado" : "Bloqueado 2/2")}\n" +
                $"<b>3. Diario Compartido [J/TAB]:</b> {chkJournal}\n" +
                $"<b>4. Puzle Coop Noche I (Pórtico):</b> {chkPorticoCoop}\n" +
                $"<b>5. Compañero y Chat de Voz:</b> {chkPartner} · {chkVoice}\n" +
                $"<b>6. Puzle Cuadro Eléctrico:</b> {chkFuses}\n" +
                $"<b>7. Llave 1:</b> {chkKey1}  |  <b>Llave 2:</b> {chkKey2}\n" +
                $"<b>8. Puzle Retratos:</b> {chkPortraits}  |  <b>Noche {nightRoman}:</b> {chkNight1}\n\n" +
                "<color=#D8C49E><b>ATAJOS DE PRUEBA RÁPIDA (TECLADO):</b></color>\n" +
                "• <b>[J] o [TAB]</b> Abrir Diario Compartido   |   <b>[ESC]</b> Menú Pausa / Sala LAN\n" +
                "• <b>[Q]</b> Cambiar Época (1929 ↔ Ruinas en modo 1 jugador)\n" +
                "• <b>[V]</b> Hablar por Micrófono   |   <b>[F2]</b> Simular 2.º Jugador   |   <b>[F4]</b> Panel";
        }

        private static bool ContainsKey(string keyId)
        {
            foreach (string k in SimpleInventory.GlobalKeys)
            {
                if (string.Equals(k, keyId, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private static void TeleportLocalPlayerTo(Vector3 worldPos, float yawDegrees)
        {
            PlayerMovement pm = null;
            if (NetworkCoopPlayer.LocalPlayer != null)
            {
                pm = NetworkCoopPlayer.LocalPlayer.GetComponent<PlayerMovement>();
            }
            if (pm == null) pm = FindFirstObjectByType<PlayerMovement>();
            if (pm == null) return;

            CharacterController cc = pm.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            pm.transform.position = worldPos;
            pm.transform.rotation = Quaternion.Euler(0f, yawDegrees, 0f);
            if (cc != null) cc.enabled = true;
        }

        private void BuildVerifierCanvas()
        {
            GameObject canvasObj = new GameObject("CoopNight1Verifier_Canvas");
            canvasObj.transform.SetParent(transform, false);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 95;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            verifierPanel = new GameObject("VerifierCard_Border");
            verifierPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform rt = verifierPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(22f, -130f);
            rt.sizeDelta = new Vector2(540f, 650f);

            Image borderImg = verifierPanel.AddComponent<Image>();
            borderImg.color = ColGoldBorder;

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(verifierPanel.transform, false);
            RectTransform fillRt = fillObj.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(1.5f, 1.5f);
            fillRt.offsetMax = new Vector2(-1.5f, -1.5f);

            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.color = ColCardBg;

            // Título
            GameObject titleObj = new GameObject("Header");
            titleObj.transform.SetParent(fillObj.transform, false);
            RectTransform titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -10f);
            titleRt.sizeDelta = new Vector2(-24f, 26f);

            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "VERIFICADOR INTEGRAL COOPERATIVO — NOCHES I A VIII [F4]";
            titleTmp.fontSize = 14.5f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = ColGoldBright;
            titleTmp.alignment = TextAlignmentOptions.Left;
            titleTmp.raycastTarget = false;

            // Cuerpo del checklist
            GameObject bodyObj = new GameObject("ChecklistBody");
            bodyObj.transform.SetParent(fillObj.transform, false);
            RectTransform bodyRt = bodyObj.AddComponent<RectTransform>();
            bodyRt.anchorMin = new Vector2(0f, 1f);
            bodyRt.anchorMax = new Vector2(1f, 1f);
            bodyRt.pivot = new Vector2(0.5f, 1f);
            bodyRt.anchoredPosition = new Vector2(0f, -40f);
            bodyRt.sizeDelta = new Vector2(-24f, 290f);

            checklistBodyText = bodyObj.AddComponent<TextMeshProUGUI>();
            checklistBodyText.fontSize = 12.2f;
            checklistBodyText.color = ColIvoryText;
            checklistBodyText.alignment = TextAlignmentOptions.TopLeft;
            checklistBodyText.lineSpacing = 3.5f;
            checklistBodyText.raycastTarget = false;

            // Botones de navegación rápida por la Noche I y Noches II–VIII para pruebas ágiles en 2 instancias
            CreateQuickTeleportButton(fillObj.transform, "BtnTpPortico", "Noche I: Pórtico (Llave Dividida)",
                new Vector2(14f, -338f), new Vector2(248f, 30f), () => TeleportLocalPlayerTo(new Vector3(0f, 3.4f, 4.2f), 0f));

            CreateQuickTeleportButton(fillObj.transform, "BtnTpDireccion", "Noche I: 7. Dirección (Llave 1 + 25A)",
                new Vector2(274f, -338f), new Vector2(248f, 30f), () => TeleportLocalPlayerTo(new Vector3(-31f, 3.4f, 18f), -90f));

            CreateQuickTeleportButton(fillObj.transform, "BtnTpVestuario", "Noche I: 11. Vestuario + 13. Taquillas",
                new Vector2(14f, -372f), new Vector2(248f, 30f), () => TeleportLocalPlayerTo(new Vector3(19f, 3.4f, 19.5f), 180f));

            CreateQuickTeleportButton(fillObj.transform, "BtnTpRetratos", "Noche I: 10. Galería Retratos + 14. Elena",
                new Vector2(274f, -372f), new Vector2(248f, 30f), () => TeleportLocalPlayerTo(new Vector3(-12f, 3.4f, 42f), 0f));

            CreateQuickTeleportButton(fillObj.transform, "BtnTpN2Mirror", "Noche II: Espejo n.º 3 (1929)",
                new Vector2(14f, -410f), new Vector2(248f, 30f), () => TeleportLocalPlayerTo(new Vector3(-34.8f, 3.4f, 59.0f), -90f));

            CreateQuickTeleportButton(fillObj.transform, "BtnTpN2Mannequin", "Noche II: Sala Maniquíes (Ruinas)",
                new Vector2(274f, -410f), new Vector2(248f, 30f), () => TeleportLocalPlayerTo(new Vector3(-19.0f, 3.4f, 72.5f), 0f));

            CreateQuickTeleportButton(fillObj.transform, "BtnTpN3Lattice", "Noche III: Celosía Isadora (1929)",
                new Vector2(14f, -444f), new Vector2(248f, 30f), () => TeleportLocalPlayerTo(new Vector3(-45.5f, 3.4f, 21.0f), 90f));

            CreateQuickTeleportButton(fillObj.transform, "BtnTpN3Workshop", "Noche III: Taller Costura (Ruinas)",
                new Vector2(274f, -444f), new Vector2(248f, 30f), () => TeleportLocalPlayerTo(new Vector3(-48.5f, 3.4f, 81.5f), 0f));

            CreateQuickTeleportButton(fillObj.transform, "BtnTpN4Booth", "Noche IV: Sincronía Cabina Audio",
                new Vector2(14f, -478f), new Vector2(248f, 30f), () => TeleportLocalPlayerTo(new Vector3(34.5f, 3.4f, 55.8f), 90f));

            CreateQuickTeleportButton(fillObj.transform, "BtnTpN5Crypt", "Noche V: Cripta Efecto Espejo",
                new Vector2(274f, -478f), new Vector2(248f, 30f), () => TeleportLocalPlayerTo(new Vector3(0.0f, -2.2f, 19.5f), 0f));

            CreateQuickTeleportButton(fillObj.transform, "BtnTpN6Pulley", "Noche VI: Poleas Contrapeso III",
                new Vector2(14f, -512f), new Vector2(248f, 30f), () => TeleportLocalPlayerTo(new Vector3(-19.5f, 3.4f, 91.5f), 0f));

            CreateQuickTeleportButton(fillObj.transform, "BtnTpN7Theatre", "Noche VII: Gran Teatro y Guion B-00",
                new Vector2(274f, -512f), new Vector2(248f, 30f), () => TeleportLocalPlayerTo(new Vector3(-8.0f, 3.4f, 114.5f), 0f));

            CreateQuickTeleportButton(fillObj.transform, "BtnTpN8Chamber", "Noche VIII: Cámara Final y Amanecer (06:00)",
                new Vector2(14f, -548f), new Vector2(248f, 32f), () => TeleportLocalPlayerTo(new Vector3(8.3f, 3.4f, 100.8f), 0f));

            CreateQuickTeleportButton(fillObj.transform, "BtnTpN1Secret", "Noche I: Tocador Nº 3 (★ Secreto Coop I)",
                new Vector2(274f, -548f), new Vector2(248f, 32f), () => TeleportLocalPlayerTo(new Vector3(23.5f, 3.4f, 32.8f), 0f));

            CreateQuickTeleportButton(fillObj.transform, "BtnAdvanceNight", "Avanzar Noche (I → VIII)",
                new Vector2(14f, -586f), new Vector2(210f, 32f), () =>
                {
                    if (NightManager.Instance != null)
                    {
                        NightManager.Instance.DevSkipNight();
                    }
                });

            CreateQuickTeleportButton(fillObj.transform, "BtnTriggerFinale", "★ PROBAR FINAL DEFINITIVO 06:00 + CRÉDITOS",
                new Vector2(232f, -586f), new Vector2(290f, 32f), () =>
                {
                    isVisible = false;
                    if (verifierPanel != null) verifierPanel.SetActive(false);
                    if (UltimoActo.Narrative.UltimateFinaleAndCreditsController.Instance != null)
                    {
                        UltimoActo.Narrative.UltimateFinaleAndCreditsController.Instance.RequestStartSynchronizedFinale("Prueba directa desde Panel [F4]");
                    }
                });

            verifierPanel.SetActive(false);
        }

        private static void CreateQuickTeleportButton(
            Transform parent,
            string name,
            string label,
            Vector2 anchoredPos,
            Vector2 size,
            UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = ColBtnNormal;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;
            if (onClick != null) btn.onClick.AddListener(onClick);

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRt = txtObj.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(6f, 2f);
            txtRt.offsetMax = new Vector2(-6f, -2f);

            TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 11.5f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = ColIvoryText;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
        }
    }
}
