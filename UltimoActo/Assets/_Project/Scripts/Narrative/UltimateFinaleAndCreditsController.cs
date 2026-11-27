using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UltimoActo.Core;
using UltimoActo.Network;
using UltimoActo.Player;
using UltimoActo.UI;

namespace UltimoActo.Narrative
{
    /// <summary>
    /// BLOQUE 3 — FINAL DEFINITIVO, AMANECER (06:00) Y CRÉDITOS:
    /// Sustituye la tarjeta negra "(Continuará — final definitivo en desarrollo)" por la secuencia
    /// final completa y sincronizada entre el Anfitrión (1929) y el Cliente (Ruinas), o en Modo Solo:
    ///  1. Clímax sincronizado con autoridad del Host (con marca de tiempo UTC tolerante a latencia).
    ///  2. Secuencia cinematográfica del Amanecer (06:00 AM):
    ///     - Cámara controlada enfocando el Espejo Central y la Cámara tras el Escenario (Zona 32/39)
    ///       con las siluetas del Cisne Blanco (Mara) y del Cisne Negro (Elena).
    ///     - Cada jugador ve cómo su época se transforma en la otra (y en Modo Solo alterna automáticamente
    ///       entre ambas épocas) hasta que 1929 y las Ruinas se funden en la luz dorada de las 06:00 h.
    ///  3. Epílogo narrativo según la Biblia Narrativa con 3 variaciones según la cantidad de evidencias,
    ///     coleccionables, secretos y logros cooperativos encontrados (manteniendo el desenlace principal).
    ///  4. Pantalla de Estadísticas de la Partida (Tiempo total, Noches, Documentos/Coleccionables,
    ///     Secretos, Anomalías vistas y Logros Cooperativos).
    ///  5. Pantalla de Créditos desplazable (equipo, materia, curso, elenco, herramientas y licencias CC0)
    ///     que NO se salta al pulsar [ESC] una sola vez: requiere mantener pulsado [ESC] durante 1.8 s.
    ///  6. Botón "VOLVER AL MENÚ PRINCIPAL" que cierra la red limpiamente, desconecta la voz, marca la
    ///     partida como completada, desbloquea la pantalla de "Colección" y evita que "Continuar" lleve
    ///     a una noche inexistente.
    /// </summary>
    public class UltimateFinaleAndCreditsController : MonoBehaviour
    {
        public const string PrefsGameCompletedKey = "UltimoActo_GameCompleted";
        public const string PrefsBestEvidencesKey = "UltimoActo_BestEvidencesCount";
        public const string PrefsBestSecretsKey = "UltimoActo_BestSecretsCount";
        public const string PrefsBestAnomaliesKey = "UltimoActo_BestAnomaliesCount";
        public const string PrefsBestAchievementsKey = "UltimoActo_BestAchievementsCount";
        public const string PrefsLastRunTimeSecondsKey = "UltimoActo_LastRunTimeSeconds";
        public const string PrefsUnlockedEndingTierKey = "UltimoActo_UnlockedEndingTier";

        public static UltimateFinaleAndCreditsController Instance { get; private set; }

        /// <summary>
        /// Indica si la secuencia final del amanecer, epílogo, estadísticas o créditos está en curso.
        /// Cuando es true, una desconexión del compañero no bloquea ni congela la pantalla.
        /// </summary>
        public static bool IsFinaleRunning => Instance != null && Instance.isFinaleActive;

        public static event Action<string> OnHostBroadcastFinalePacket;

        private bool isFinaleActive;
        private bool isCreditsPhaseActive;
        private bool creditsFinishedOrSkipped;
        private float sessionStartRealtime;
        private float holdEscapeTimer;
        private const float HoldEscapeRequiredSeconds = 1.8f;

        // Estadísticas sincronizadas desde el Host al iniciar el final
        private int syncedEvidencesCount;
        private int syncedSecretsCount;
        private int syncedAnomaliesCount;
        private int syncedAchievementsCount;
        private float syncedRunDurationSeconds;

        // Cámara y escenario cinematográfico 06:00 AM
        private Camera playerMainCamera;
        private Transform savedCameraParent;
        private Vector3 savedCameraLocalPos;
        private Quaternion savedCameraLocalRot;
        private int savedCameraCullingMask;
        private PlayerMovement cachedPlayerMovement;
        private PlayerCameraLook cachedPlayerLook;
        private GameObject dawnStageSceneRoot;
        private Light dawnSunLight;
        private Light mirrorSpotLight;
        private Transform swanWhiteSilhouette;
        private Transform swanBlackSilhouette;

        // Audio procedural del amanecer y epílogo
        private AudioSource finaleAudioSource;

        // UI del Final Definitivo
        private Canvas finaleCanvas;
        private CanvasGroup veilCanvasGroup;
        private Image veilTintImage;

        // Banner superior durante la fusión de épocas (06:00 AM)
        private CanvasGroup dawnBannerGroup;
        private TextMeshProUGUI dawnClockLabel;
        private TextMeshProUGUI dawnPhaseTitleLabel;
        private TextMeshProUGUI dawnPhaseSubtitleLabel;

        // Panel de Epílogo Narrativo
        private GameObject epiloguePanel;
        private CanvasGroup epilogueCanvasGroup;
        private TextMeshProUGUI epilogueTierBadgeText;
        private TextMeshProUGUI epilogueHeadingText;
        private TextMeshProUGUI epilogueBodyText;
        private TextMeshProUGUI epiloguePageCounterText;
        private Button btnNextEpilogueSlide;
        private TextMeshProUGUI btnNextEpilogueLabel;
        private int currentEpilogueSlideIndex;
        private readonly List<string> epilogueSlides = new List<string>();
        private bool waitingForEpilogueContinue;

        // Panel de Estadísticas de la Partida
        private GameObject statsPanel;
        private CanvasGroup statsCanvasGroup;
        private TextMeshProUGUI statsBodyText;
        private TextMeshProUGUI statsAchievementsListText;
        private Button btnProceedToCredits;
        private bool waitingForStatsContinue;

        // Panel de Créditos Desplazables + Barra de mantener [ESC] + Botón Volver al Menú
        private GameObject creditsPanel;
        private CanvasGroup creditsCanvasGroup;
        private RectTransform creditsScrollContentRt;
        private TextMeshProUGUI holdEscapePromptText;
        private Image holdEscapeFillBar;
        private Button btnReturnToMainMenu;

        // Paleta Gran Teatro de Ópera & Amanecer 06:00 h
        private static readonly Color ColDarkOperaBg = new Color(0.04f, 0.018f, 0.028f, 0.96f);
        private static readonly Color ColCardBg = new Color(0.09f, 0.038f, 0.058f, 0.96f);
        private static readonly Color ColDawnGold = new Color(0.95f, 0.80f, 0.44f, 1f);
        private static readonly Color ColDawnGoldSoft = new Color(0.86f, 0.72f, 0.42f, 0.85f);
        private static readonly Color ColIvory = new Color(0.96f, 0.94f, 0.90f, 1f);
        private static readonly Color ColBtnWine = new Color(0.28f, 0.08f, 0.14f, 0.98f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            TryInstallInActiveScene(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryInstallInActiveScene(scene);
        }

        private static void TryInstallInActiveScene(Scene scene)
        {
            if (scene.name == "MainMenu") return;
            if (Instance != null) return;

            GameObject go = new GameObject("UltimateFinaleAndCreditsController");
            Instance = go.AddComponent<UltimateFinaleAndCreditsController>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            sessionStartRealtime = Time.realtimeSinceStartup;
        }

        private void Start()
        {
            BuildFinaleCanvasUI();
            SetupFinaleAudio();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (!isFinaleActive) return;

            // Durante la pantalla de créditos: detectar pulsación MANTENIDA de [ESC] (no salta con un solo toque)
            if (isCreditsPhaseActive && !creditsFinishedOrSkipped)
            {
                bool escHeld = IsEscapeOrSkipKeyHeld();
                if (escHeld)
                {
                    holdEscapeTimer += Time.unscaledDeltaTime;
                    if (holdEscapeFillBar != null)
                    {
                        holdEscapeFillBar.fillAmount = Mathf.Clamp01(holdEscapeTimer / HoldEscapeRequiredSeconds);
                    }
                    if (holdEscapePromptText != null)
                    {
                        holdEscapePromptText.text = $"Mantén pulsado [ESC] para saltar a la conclusión ({Mathf.Clamp01(holdEscapeTimer / HoldEscapeRequiredSeconds) * 100f:0}%)...";
                    }

                    if (holdEscapeTimer >= HoldEscapeRequiredSeconds)
                    {
                        creditsFinishedOrSkipped = true;
                    }
                }
                else
                {
                    holdEscapeTimer = Mathf.MoveTowards(holdEscapeTimer, 0f, Time.unscaledDeltaTime * 2.2f);
                    if (holdEscapeFillBar != null)
                    {
                        holdEscapeFillBar.fillAmount = Mathf.Clamp01(holdEscapeTimer / HoldEscapeRequiredSeconds);
                    }
                    if (holdEscapePromptText != null)
                    {
                        holdEscapePromptText.text = "Mantén pulsado [ESC] durante 2 s para saltar el desplazamiento · Rueda del ratón para desplazar";
                    }
                }

                // Permitir desplazar manualmente con la rueda del ratón
                float wheel = GetMouseScrollDeltaY();
                if (Mathf.Abs(wheel) > 0.01f && creditsScrollContentRt != null)
                {
                    Vector2 pos = creditsScrollContentRt.anchoredPosition;
                    pos.y = Mathf.Clamp(pos.y - wheel * 45f, 0f, 2350f);
                    creditsScrollContentRt.anchoredPosition = pos;
                }
            }
        }

        private static bool IsEscapeOrSkipKeyHeld()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                return Keyboard.current.escapeKey.isPressed;
            }
            return false;
#else
            return Input.GetKey(KeyCode.Escape);
#endif
        }

        private static float GetMouseScrollDeltaY()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.scroll.ReadValue().y * 0.05f;
            }
            return 0f;
#else
            return Input.mouseScrollDelta.y;
#endif
        }

        // =========================================================================================
        // 1. DISPARO SINCRONIZADO CON AUTORIDAD DEL HOST (Y TOLERANCIA A LATENCIA)
        // =========================================================================================

        /// <summary>
        /// Llamado por NightManager.EndingSequence(), TheatreClimaxController (Fase 5) o el puzle
        /// cooperativo final de la Noche VIII. Si somos el Host (o Modo Solo), calcula las estadísticas
        /// consolidadas, las transmite al Cliente con marca de tiempo UTC e inicia la secuencia.
        /// </summary>
        public void RequestStartSynchronizedFinale(string reason = "Clímax Noche VIII")
        {
            if (isFinaleActive) return;

            bool isClientOnly = LanSessionManager.Instance != null &&
                                LanSessionManager.Instance.CurrentMode == LanSessionMode.Client;

            GatherLocalSessionStatistics(
                out int evidences,
                out int secrets,
                out int anomalies,
                out int achievements,
                out float elapsedSec);

            long startUtcTicks = DateTime.UtcNow.AddMilliseconds(350).Ticks;

            if (!isClientOnly)
            {
                // Autoridad del Host: difundir orden sincronizada de inicio del final a todos los clientes
                string payload = $"{startUtcTicks}|{elapsedSec:0.0}|{evidences}|{secrets}|{anomalies}|{achievements}";
                OnHostBroadcastFinalePacket?.Invoke(payload);
            }

            StartCoroutine(ExecuteFullFinaleSequenceRoutine(
                startUtcTicks,
                elapsedSec,
                evidences,
                secrets,
                anomalies,
                achievements));
        }

        /// <summary>
        /// Recibido por el Cliente desde SharedWorldStateSync ("FINAL_DAWN_START|...").
        /// </summary>
        public void ApplyRemoteFinaleStartFromHost(string payload)
        {
            if (isFinaleActive) return;

            string[] parts = payload.Split('|');
            long startTicks = DateTime.UtcNow.Ticks;
            float elapsedSec = Mathf.Max(10f, Time.realtimeSinceStartup - sessionStartRealtime);
            int evidences = 16;
            int secrets = 3;
            int anomalies = 6;
            int achievements = 2;

            if (parts.Length >= 6)
            {
                long.TryParse(parts[0], out startTicks);
                float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out elapsedSec);
                int.TryParse(parts[2], out evidences);
                int.TryParse(parts[3], out secrets);
                int.TryParse(parts[4], out anomalies);
                int.TryParse(parts[5], out achievements);
            }

            StartCoroutine(ExecuteFullFinaleSequenceRoutine(
                startTicks,
                elapsedSec,
                evidences,
                secrets,
                anomalies,
                achievements));
        }

        private void GatherLocalSessionStatistics(
            out int evidencesCount,
            out int secretsCount,
            out int anomaliesCount,
            out int achievementsCount,
            out float durationSec)
        {
            durationSec = Mathf.Max(12f, Time.realtimeSinceStartup - sessionStartRealtime);
            evidencesCount = 0;
            secretsCount = 0;
            anomaliesCount = 0;
            achievementsCount = 0;

            if (SharedEvidenceJournalManager.Instance != null)
            {
                var all = SharedEvidenceJournalManager.Instance.AllEntries;
                evidencesCount = all != null ? all.Count : 0;
                achievementsCount = SharedEvidenceJournalManager.Instance.UnlockedAchievements != null
                    ? SharedEvidenceJournalManager.Instance.UnlockedAchievements.Count
                    : 0;

                if (all != null)
                {
                    for (int i = 0; i < all.Count; i++)
                    {
                        var e = all[i];
                        if (e == null) continue;
                        string idLower = (e.entryId ?? "").ToLowerInvariant();
                        string titleLower = (e.title ?? "").ToLowerInvariant();
                        if (idLower.Contains("anom") || titleLower.Contains("anomalía") || titleLower.Contains("anomalia"))
                        {
                            anomaliesCount++;
                        }
                        if (idLower.Contains("secret") || idLower.Contains("b00") || idLower.Contains("cripta") ||
                            titleLower.Contains("secreto") || titleLower.Contains("ocult") || e.isPuzzleSolution)
                        {
                            secretsCount++;
                        }
                    }
                }
            }

            if (NightManager.Instance != null && NightManager.Instance.GlobalEvidenceIds != null)
            {
                evidencesCount = Mathf.Max(evidencesCount, NightManager.Instance.GlobalEvidenceIds.Count);
            }

            secretsCount = Mathf.Clamp(Mathf.Max(secretsCount, evidencesCount / 4), 1, 8);
            anomaliesCount = Mathf.Max(anomaliesCount, 3);
        }

        // =========================================================================================
        // 2. SECUENCIA COMPLETA: AMANECER (06:00) -> EPÍLOGO -> ESTADÍSTICAS -> CRÉDITOS -> MENÚ
        // =========================================================================================

        private IEnumerator ExecuteFullFinaleSequenceRoutine(
            long scheduledUtcTicks,
            float runDurationSeconds,
            int evidencesCount,
            int secretsCount,
            int anomaliesCount,
            int achievementsCount)
        {
            isFinaleActive = true;
            Time.timeScale = 1f;

            syncedRunDurationSeconds = runDurationSeconds;
            syncedEvidencesCount = evidencesCount;
            syncedSecretsCount = secretsCount;
            syncedAnomaliesCount = anomaliesCount;
            syncedAchievementsCount = achievementsCount;

            // Persistir finalización de la partida y desbloquear la pantalla de Colección sin borrar logros
            SaveCompletedRunToPrefs();

            // Cerrar modales abiertos (documentos, diario, pausa o desconexión) para no tapar el final
            if (DocumentUI.Instance != null && DocumentUI.Instance.IsOpen)
            {
                DocumentUI.Instance.CloseDocument();
            }
            if (InGamePauseMenuUI.Instance != null && InGamePauseMenuUI.Instance.IsOpen)
            {
                InGamePauseMenuUI.Instance.ClosePauseMenu();
            }
            if (CoopDisconnectRecoveryUI.Instance != null && CoopDisconnectRecoveryUI.Instance.IsRecoveryModalOpen)
            {
                CoopDisconnectRecoveryUI.Instance.CloseRecoveryModal();
            }

            // Sincronización por marca de tiempo UTC (tolerante a latencia de red)
            double waitMs = (scheduledUtcTicks - DateTime.UtcNow.Ticks) / (double)TimeSpan.TicksPerMillisecond;
            if (waitMs > 15 && waitMs < 2500)
            {
                yield return new WaitForSecondsRealtime((float)(waitMs / 1000.0));
            }

            if (finaleCanvas != null)
            {
                finaleCanvas.gameObject.SetActive(true);
            }

            // Bloquear movimiento del jugador y tomar control cinematográfico de la cámara
            LockPlayerAndPrepareCinematicCamera();

            // -------------------------------------------------------------------------------------
            // FASE 1 y 2: SECUENCIA DEL AMANECER (06:00 AM) Y FUSIÓN DE LAS DOS ÉPOCAS
            // -------------------------------------------------------------------------------------
            yield return RunDawn0600EraFusionCinematicRoutine();

            // -------------------------------------------------------------------------------------
            // FASE 3: EPÍLOGO NARRATIVO CON VARIACIÓN SEGÚN COLECCIONABLES Y SECRETOS
            // -------------------------------------------------------------------------------------
            yield return RunNarrativeEpilogueSlidesRoutine();

            // -------------------------------------------------------------------------------------
            // FASE 4: ESTADÍSTICAS DE LA PARTIDA
            // -------------------------------------------------------------------------------------
            yield return RunSessionStatisticsScreenRoutine();

            // -------------------------------------------------------------------------------------
            // FASE 5 y 6: CRÉDITOS DESPLAZABLES (MANTENER [ESC] PARA SALTAR) Y VUELTA AL MENÚ
            // -------------------------------------------------------------------------------------
            yield return RunScrollingCreditsAndReturnMenuRoutine();
        }

        private void LockPlayerAndPrepareCinematicCamera()
        {
            if (NetworkCoopPlayer.LocalPlayer != null)
            {
                cachedPlayerMovement = NetworkCoopPlayer.LocalPlayer.GetComponent<PlayerMovement>();
                cachedPlayerLook = NetworkCoopPlayer.LocalPlayer.GetComponentInChildren<PlayerCameraLook>();
            }
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

            playerMainCamera = Camera.main;
            if (playerMainCamera == null) playerMainCamera = FindFirstObjectByType<Camera>();

            if (playerMainCamera != null)
            {
                savedCameraParent = playerMainCamera.transform.parent;
                savedCameraLocalPos = playerMainCamera.transform.localPosition;
                savedCameraLocalRot = playerMainCamera.transform.localRotation;
                savedCameraCullingMask = playerMainCamera.cullingMask;
            }
        }

        private IEnumerator RunDawn0600EraFusionCinematicRoutine()
        {
            EraVisual initialEra = EraVisualManager.Instance != null
                ? EraVisualManager.Instance.CurrentEra
                : EraVisual.Glory1929;
            EraVisual oppositeEra = initialEra == EraVisual.Glory1929
                ? EraVisual.Ruins
                : EraVisual.Glory1929;
            bool isSoloMode = LanSessionManager.Instance == null || LanSessionManager.Instance.ConnectedPlayerCount < 2;

            // Fundido breve hacia el escenario del Gran Teatro / Cámara del Espejo Central (Zona 32/39)
            yield return FadeVeilTo(new Color(0.05f, 0.02f, 0.04f, 1f), 1.0f);

            BuildDawnStageCinematicProps();

            // Colocar cámara controlada frente al Espejo Central del Escenario (Zona 32 mirando al arco y espejo en Z=103..107)
            Vector3 camStartPos = new Vector3(-8.0f, 4.35f, 114.8f);
            Vector3 camMidPos = new Vector3(-8.0f, 4.10f, 110.2f);
            Vector3 camFinalPos = new Vector3(-8.0f, 3.85f, 107.6f);
            Vector3 lookTarget = new Vector3(-8.0f, 3.70f, 103.5f);

            if (playerMainCamera != null)
            {
                playerMainCamera.transform.SetParent(null, true);
                playerMainCamera.transform.position = camStartPos;
                playerMainCamera.transform.rotation = Quaternion.LookRotation(lookTarget - camStartPos, Vector3.up);
            }

            if (dawnBannerGroup != null)
            {
                dawnBannerGroup.gameObject.SetActive(true);
                dawnBannerGroup.alpha = 1f;
            }

            // Compás 48 y las 6 campanadas de las 06:00 AM
            PlayDawnBellAndMusicBoxChord(0);

            string myEraLabel = initialEra == EraVisual.Glory1929 ? "1929 (Noche del Estreno)" : "Actualidad (Ruinas de la Academia)";
            string otherEraLabel = oppositeEra == EraVisual.Glory1929 ? "1929 (Noche del Estreno)" : "Actualidad (Ruinas de la Academia)";

            if (dawnClockLabel != null) dawnClockLabel.text = "05:59 → 06:00 AM";
            if (dawnPhaseTitleLabel != null) dawnPhaseTitleLabel.text = "EL COMPÁS 48 SE COMPLETA";
            if (dawnPhaseSubtitleLabel != null)
            {
                dawnPhaseSubtitleLabel.text = isSoloMode
                    ? "Modo Solitario: Las dos capas del tiempo (1929 y Ruinas) comienzan a resonar y alternarse en el Espejo Central..."
                    : $"Tu percepción ({myEraLabel}) comienza a transformarse en la época de tu compañero ({otherEraLabel})...";
            }

            yield return FadeVeilTo(new Color(0.05f, 0.02f, 0.04f, 0f), 1.2f);

            // Tramo 1 (3.0 s): Paneo suave mientras el jugador ve su época original
            float elapsed = 0f;
            while (elapsed < 3.0f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / 3.0f);
                if (playerMainCamera != null)
                {
                    playerMainCamera.transform.position = Vector3.Lerp(camStartPos, camMidPos, Mathf.SmoothStep(0f, 1f, t));
                    playerMainCamera.transform.rotation = Quaternion.LookRotation(lookTarget - playerMainCamera.transform.position, Vector3.up);
                }
                AnimateSwansBow(t * 0.35f);
                yield return null;
            }

            // Tramo 2 (3.5 s): Cada jugador ve cómo su época se transforma en la otra (y en Modo Solo alterna ambas épocas)
            PlayDawnBellAndMusicBoxChord(1);
            yield return FadeVeilTo(new Color(0.95f, 0.82f, 0.50f, 0.65f), 0.35f);

            if (EraVisualManager.Instance != null)
            {
                EraVisualManager.Instance.SetEra(oppositeEra, false);
            }
            if (dawnPhaseTitleLabel != null) dawnPhaseTitleLabel.text = "CRUCE DE PERCEPCIONES TEMPORALES";
            if (dawnPhaseSubtitleLabel != null)
            {
                dawnPhaseSubtitleLabel.text = isSoloMode
                    ? $"Alternando percepción hacia {otherEraLabel} antes de la convergencia definitiva..."
                    : $"Ahora contemplas la Academia Vesper a través de los ojos de tu compañero: {otherEraLabel}.";
            }

            yield return FadeVeilTo(new Color(0.95f, 0.82f, 0.50f, 0f), 0.55f);

            elapsed = 0f;
            while (elapsed < 2.8f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / 2.8f);

                // En Modo Solo, mostrar una pulsación adicional entre 1929 y Ruinas para evidenciar la fusión
                if (isSoloMode && Mathf.Abs(elapsed - 1.4f) < 0.04f && EraVisualManager.Instance != null)
                {
                    EraVisualManager.Instance.SetEra(initialEra, false);
                }

                if (playerMainCamera != null)
                {
                    playerMainCamera.transform.position = Vector3.Lerp(camMidPos, camFinalPos, Mathf.SmoothStep(0f, 1f, t));
                    playerMainCamera.transform.rotation = Quaternion.LookRotation(lookTarget - playerMainCamera.transform.position, Vector3.up);
                }
                AnimateSwansBow(0.35f + t * 0.35f);
                yield return null;
            }

            // Tramo 3 (4.2 s): 06:00 AM — FUSIÓN DEFINITIVA DE LAS DOS ÉPOCAS (Ambas capas visibles + Luz Dorada del Alba)
            PlayDawnBellAndMusicBoxChord(2);
            yield return FadeVeilTo(new Color(1.0f, 0.90f, 0.66f, 0.85f), 0.45f);

            ApplyGoldenDawnFusionAtmosphereAndBothLayers();

            if (dawnClockLabel != null) dawnClockLabel.text = "06:00 AM — AMANECER EN LA ACADEMIA VESPER";
            if (dawnPhaseTitleLabel != null) dawnPhaseTitleLabel.text = "LAS DOS ÉPOCAS SE FUNDEN EN UN SOLO PRESENTE";
            if (dawnPhaseSubtitleLabel != null)
            {
                dawnPhaseSubtitleLabel.text =
                    "El bucle del 14 de octubre de 1929 se rompe. Elena Vesper (Cisne Negro) y Mara Bell (Cisne Blanco) hacen su última reverencia juntas.";
            }

            yield return FadeVeilTo(new Color(1.0f, 0.90f, 0.66f, 0f), 0.95f);

            elapsed = 0f;
            while (elapsed < 3.6f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / 3.6f);
                AnimateSwansBow(0.70f + t * 0.30f);
                yield return null;
            }

            if (dawnBannerGroup != null)
            {
                dawnBannerGroup.alpha = 0f;
                dawnBannerGroup.gameObject.SetActive(false);
            }
        }

        private void ApplyGoldenDawnFusionAtmosphereAndBothLayers()
        {
            // Hacer visibles simultáneamente los objetos de 1929 y de Ruinas en la cámara principal
            if (playerMainCamera != null)
            {
                int l1929 = EraVisualManager.GetEra1929Layer();
                int lRuins = EraVisualManager.GetEraRuinsLayer();
                playerMainCamera.cullingMask |= (1 << l1929) | (1 << lRuins);
            }

            // Iluminación y atmósfera cálida de amanecer (06:00 AM)
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.82f, 0.70f, 0.54f, 1f);
            RenderSettings.fogDensity = 0.0022f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.96f, 0.86f, 0.68f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.84f, 0.70f, 0.56f, 1f);
            RenderSettings.ambientGroundColor = new Color(0.48f, 0.40f, 0.36f, 1f);

            if (dawnSunLight != null)
            {
                dawnSunLight.intensity = 3.2f;
                dawnSunLight.color = new Color(1.0f, 0.88f, 0.64f, 1f);
            }
        }

        private void BuildDawnStageCinematicProps()
        {
            if (dawnStageSceneRoot != null) return;

            dawnStageSceneRoot = new GameObject("Dawn0600_CinematicStage_Root");
            dawnStageSceneRoot.transform.position = new Vector3(-8.0f, 2.4f, 103.5f);

            // Luz solar de amanecer bañando el escenario y el Espejo Central
            GameObject sunObj = new GameObject("Dawn0600_GoldenLight");
            sunObj.transform.SetParent(dawnStageSceneRoot.transform, false);
            sunObj.transform.localPosition = new Vector3(0f, 4.5f, 3.2f);
            dawnSunLight = sunObj.AddComponent<Light>();
            dawnSunLight.type = LightType.Point;
            dawnSunLight.range = 38f;
            dawnSunLight.intensity = 2.0f;
            dawnSunLight.color = new Color(1.0f, 0.86f, 0.60f, 1f);

            GameObject spotObj = new GameObject("Dawn0600_MirrorSpotlight");
            spotObj.transform.SetParent(dawnStageSceneRoot.transform, false);
            spotObj.transform.localPosition = new Vector3(0f, 3.8f, 4.5f);
            spotObj.transform.localRotation = Quaternion.Euler(28f, 180f, 0f);
            mirrorSpotLight = spotObj.AddComponent<Light>();
            mirrorSpotLight.type = LightType.Spot;
            mirrorSpotLight.spotAngle = 58f;
            mirrorSpotLight.range = 18f;
            mirrorSpotLight.intensity = 3.5f;
            mirrorSpotLight.color = new Color(1.0f, 0.92f, 0.76f, 1f);

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            // Marco del Espejo Central de Escena (en Z = 103.5)
            Material goldFrameMat = new Material(litShader) { color = new Color(0.85f, 0.68f, 0.32f, 1f) };
            Material mirrorGlassMat = new Material(litShader) { color = new Color(0.78f, 0.88f, 0.96f, 1f) };
            if (mirrorGlassMat.HasProperty("_EmissionColor"))
            {
                mirrorGlassMat.EnableKeyword("_EMISSION");
                mirrorGlassMat.SetColor("_EmissionColor", new Color(0.45f, 0.40f, 0.28f, 1f));
            }

            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "CentralStageMirror_Frame";
            frame.transform.SetParent(dawnStageSceneRoot.transform, false);
            frame.transform.localPosition = new Vector3(0f, 1.85f, -0.4f);
            frame.transform.localScale = new Vector3(3.4f, 3.6f, 0.16f);
            Destroy(frame.GetComponent<Collider>());
            frame.GetComponent<Renderer>().sharedMaterial = goldFrameMat;

            GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glass.name = "CentralStageMirror_Surface";
            glass.transform.SetParent(dawnStageSceneRoot.transform, false);
            glass.transform.localPosition = new Vector3(0f, 1.85f, -0.30f);
            glass.transform.localScale = new Vector3(2.95f, 3.15f, 0.06f);
            Destroy(glass.GetComponent<Collider>());
            glass.GetComponent<Renderer>().sharedMaterial = mirrorGlassMat;

            // Siluetas de Elena Vesper (Cisne Negro) y Mara Bell (Cisne Blanco) frente al espejo
            Material blackSwanMat = new Material(litShader) { color = new Color(0.14f, 0.10f, 0.18f, 1f) };
            if (blackSwanMat.HasProperty("_EmissionColor"))
            {
                blackSwanMat.EnableKeyword("_EMISSION");
                blackSwanMat.SetColor("_EmissionColor", new Color(0.32f, 0.22f, 0.42f, 1f));
            }

            Material whiteSwanMat = new Material(litShader) { color = new Color(0.96f, 0.93f, 0.86f, 1f) };
            if (whiteSwanMat.HasProperty("_EmissionColor"))
            {
                whiteSwanMat.EnableKeyword("_EMISSION");
                whiteSwanMat.SetColor("_EmissionColor", new Color(0.55f, 0.48f, 0.32f, 1f));
            }

            swanBlackSilhouette = CreateSwanDancerFigure("ElenaVesper_BlackSwan", dawnStageSceneRoot.transform, new Vector3(-0.75f, 0f, 0.65f), blackSwanMat);
            swanWhiteSilhouette = CreateSwanDancerFigure("MaraBell_WhiteSwan", dawnStageSceneRoot.transform, new Vector3(0.75f, 0f, 0.65f), whiteSwanMat);
        }

        private static Transform CreateSwanDancerFigure(string name, Transform parent, Vector3 localPos, Material mat)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;

            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.transform.SetParent(root.transform, false);
            torso.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            torso.transform.localScale = new Vector3(0.30f, 0.48f, 0.22f);
            Destroy(torso.GetComponent<Collider>());
            torso.GetComponent<Renderer>().sharedMaterial = mat;

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.72f, 0f);
            head.transform.localScale = new Vector3(0.22f, 0.24f, 0.22f);
            Destroy(head.GetComponent<Collider>());
            head.GetComponent<Renderer>().sharedMaterial = mat;

            GameObject tutu = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tutu.transform.SetParent(root.transform, false);
            tutu.transform.localPosition = new Vector3(0f, 0.96f, 0f);
            tutu.transform.localScale = new Vector3(0.84f, 0.05f, 0.84f);
            Destroy(tutu.GetComponent<Collider>());
            tutu.GetComponent<Renderer>().sharedMaterial = mat;

            return root.transform;
        }

        private void AnimateSwansBow(float normalizedProgress)
        {
            float bowAngle = Mathf.SmoothStep(0f, 26f, Mathf.Clamp01((normalizedProgress - 0.45f) / 0.55f));
            float turnInward = Mathf.SmoothStep(0f, 18f, normalizedProgress);

            if (swanBlackSilhouette != null)
            {
                swanBlackSilhouette.localRotation = Quaternion.Euler(bowAngle, turnInward, 0f);
            }
            if (swanWhiteSilhouette != null)
            {
                swanWhiteSilhouette.localRotation = Quaternion.Euler(bowAngle, -turnInward, 0f);
            }
        }

        // =========================================================================================
        // 3. EPÍLOGO NARRATIVO CON 3 VARIACIONES SEGÚN COLECCIONABLES Y SECRETOS
        // =========================================================================================

        private int DetermineEndingTier(out string tierBadge, out string tierTitle)
        {
            int score = syncedEvidencesCount + (syncedSecretsCount * 2) + (syncedAchievementsCount * 3);

            if (score >= 26 || (syncedEvidencesCount >= 18 && syncedAchievementsCount >= 2))
            {
                tierBadge = "FINAL III DE III — EPÍLOGO ÍNTEGRO DE COLECCIONISTA (100% DE LA VERDAD REVELADA)";
                tierTitle = "«DOS CISNES EN LIBERTAD — LA CODA VERDADERA DEL COMPÁS 48»";
                return 3;
            }
            if (score >= 14 || syncedEvidencesCount >= 10)
            {
                tierBadge = "FINAL II DE III — EPÍLOGO EXTENDIDO (CONTRADICCIÓN DOCUMENTAL RESUELTA)";
                tierTitle = "«LA VERDAD DETRÁS DEL ESPEJO DE LA ACADEMIA VESPER»";
                return 2;
            }

            tierBadge = "FINAL I DE III — DESENLACE PRINCIPAL (EL AMANECER DE LAS 06:00 AM)";
            tierTitle = "«EL ÚLTIMO ACTO COMPLETADO»";
            return 1;
        }

        private void BuildEpilogueSlidesForCurrentTier(int tier)
        {
            epilogueSlides.Clear();

            // Diapositiva 1: La verdad de la noche del 14 de octubre de 1929 (Común a todos los finales según la Biblia Narrativa)
            epilogueSlides.Add(
                "<b>I. LA VERDAD DEL 14 DE OCTUBRE DE 1929</b>\n\n" +
                "Cuando la sexta campanada de las <b>06:00 AM</b> se apaga en la Torre del Reloj, la fractura que dividía la <b>Academia Vesper</b> desde hacía casi un siglo desaparece. Daniela y Mateo dejan de verse como siluetas de dos tiempos distintos y se encuentran cara a cara bajo la luz dorada del alba.\n\n" +
                "La versión oficial que habló de un «accidente fortuito» durante el estreno de <i>El Lago de las Sombras</i> queda desmentida para siempre:\n" +
                "<b>Elena Vesper (el Cisne Negro)</b> no huyó ni cayó por accidente. La directora <b>Isadora Vale</b>, obsesionada con lograr un tercer acto sobrecogedor y silenciar la amenaza de Elena de denunciar los abusos del Patronato, manipuló la partitura en el <b>Compás 48</b>, vertió tres gotas de Éter Dulce en la copa carmesí del camerino y ordenó soltar el <b>Contrapeso III</b> para encerrar a Elena dentro de la cámara acústica detrás del Espejo Central.");

            // Diapositiva 2: El secreto de Mara Bell y la invitación anónima
            epilogueSlides.Add(
                "<b>II. EL PACTO DEL CISNE BLANCO Y LA INVITACIÓN ANÓNIMA</b>\n\n" +
                "<b>Mara Bell (el Cisne Blanco)</b> jamás fue cómplice del crimen: aquella tarde había pactado en secreto con Elena vestir un segundo traje de Cisne Negro (de talla 61 cm, confeccionado en el taller de Madame Hélène) para intercambiar sus puestos frente al espejo y dejar en evidencia el montaje de Isadora.\n\n" +
                "Pero Isadora adelantó dos compases la caída del telón de hierro. Cuando Mara y el maestro <b>Julian Krol</b> lograron abrir la cámara oculta en la madrugada del 16 de octubre, ya era tarde. Bajo amenaza del Patronato, Mara guardó silencio toda su vida, custodiando la mitad de la partitura original hasta dejarla en manos de su nieta Daniela y de Mateo, nieto de Krol.\n\n" +
                "La invitación anónima que los convocó a la academia fue el último eco de la cajita musical de Elena, que necesitaba dos miradas —una en <b>1929</b> y otra en la <b>Actualidad</b>— para completar el último acto.");

            // Diapositiva 3: Variación según coleccionables, evidencias y secretos encontrados
            if (tier == 3)
            {
                epilogueSlides.Add(
                    "<b>III. EPÍLOGO ÍNTEGRO DE COLECCIONISTA — «DOS CISNES EN LIBERTAD»</b>\n\n" +
                    "<color=#F3D692>★ Gracias a que reunisteis las pruebas cruzadas de ambas épocas, los secretos ocultos y los logros de sincronía cooperativa:</color>\n\n" +
                    "Daniela y Mateo salen de la Academia Vesper no solo con la <b>Cajita Musical de Cisne</b> y la <b>Partitura Original a Dos Manos</b>, sino también con el expediente completo que prueba la premeditación de Isadora Vale: la orden de tramoya firmada el <b>13 de octubre de 1929</b> (un día antes del estreno), el acta del féretro vacío de la cripta, la ficha clínica del Dr. Morales y la cinta magnética sin cortes de Julian Krol.\n\n" +
                    "Tres meses después, la obra íntegra de Julian Krol se estrena por primera vez con su <b>Do Natural</b> verdadero en el Compás 48. El nombre de <b>Elena Vesper</b> queda restituido en la historia de la danza y la memoria de <b>Mara Bell</b> queda libre de toda sombra. En el cristal del espejo iluminado por el sol, las dos amigas hacen su última reverencia juntas y se desvanecen en paz.");
            }
            else if (tier == 2)
            {
                epilogueSlides.Add(
                    "<b>III. EPÍLOGO EXTENDIDO — «LA VERDAD DEL ESPEJO»</b>\n\n" +
                    "<color=#F3D692>◆ Gracias a las evidencias y listas complementarias que reconstruisteis entre 1929 y las Ruinas:</color>\n\n" +
                    "Daniela y Mateo recuperan la <b>Cajita Musical de Elena Vesper</b>, la <b>Partitura Original del Compás 48</b> y las pruebas documentales principales que demuestran que la orden de soltar el Contrapeso III fue firmada el 13 de octubre de 1929 y que el cable fue cortado con cizalla.\n\n" +
                    "La crónica histórica de la Academia Vesper se reescribe públicamente: la mentira de Isadora Vale cae y Mara Bell queda exonerada ante su familia. <i>(Aún quedan algunos secretos opcionales en la academia que podéis descubrir desde la pantalla de Colección o en una nueva partida para desbloquear el Epílogo Íntegro de Coleccionista).</i>");
            }
            else
            {
                epilogueSlides.Add(
                    "<b>III. DESENLACE ESTÁNDAR — «EL AMANECER TRAS EL TELÓN»</b>\n\n" +
                    "<color=#F3D692>• Habéis completado el Último Acto y unido las dos épocas a las 06:00 AM:</color>\n\n" +
                    "Daniela y Mateo toman del pedestal de la Cámara Oculta la <b>Cajita Musical de Cisne</b> y la <b>Partitura Original</b>. El eco de Elena Vesper queda por fin en libertad y las puertas de la Academia Vesper se abren hacia la luz de la mañana.\n\n" +
                    "Aunque habéis comprendido la tragedia de Elena, Mara e Isadora, parte de los documentos probatorios y secretos secundarios quedaron ocultos entre las sombras de las ocho noches. <i>(Podéis consultar vuestro progreso en la pantalla «Colección» del Menú Principal y rejugar para descubrir los 2 epílogos extendidos).</i>");
            }
        }

        private IEnumerator RunNarrativeEpilogueSlidesRoutine()
        {
            int tier = DetermineEndingTier(out string tierBadge, out string tierTitle);
            BuildEpilogueSlidesForCurrentTier(tier);

            if (epiloguePanel != null) epiloguePanel.SetActive(true);
            if (epilogueTierBadgeText != null) epilogueTierBadgeText.text = tierBadge;
            if (epilogueHeadingText != null) epilogueHeadingText.text = tierTitle;

            for (int i = 0; i < epilogueSlides.Count; i++)
            {
                currentEpilogueSlideIndex = i;
                if (epilogueBodyText != null) epilogueBodyText.text = epilogueSlides[i];
                if (epiloguePageCounterText != null)
                {
                    epiloguePageCounterText.text = $"Capítulo del Epílogo {i + 1} de {epilogueSlides.Count}";
                }
                if (btnNextEpilogueLabel != null)
                {
                    btnNextEpilogueLabel.text = (i + 1 < epilogueSlides.Count)
                        ? "SIGUIENTE CAPÍTULO DEL EPÍLOGO →"
                        : "VER ESTADÍSTICAS DE LA PARTIDA →";
                }

                PlayDawnBellAndMusicBoxChord(1);

                // Avanza al pulsar el botón o tras 18 segundos de lectura automática
                waitingForEpilogueContinue = true;
                float autoTimer = 0f;
                while (waitingForEpilogueContinue && autoTimer < 18.0f)
                {
                    autoTimer += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            if (epiloguePanel != null) epiloguePanel.SetActive(false);
        }

        // =========================================================================================
        // 4. PANTALLA DE ESTADÍSTICAS DE LA PARTIDA
        // =========================================================================================

        private IEnumerator RunSessionStatisticsScreenRoutine()
        {
            if (statsPanel != null) statsPanel.SetActive(true);

            TimeSpan span = TimeSpan.FromSeconds(Mathf.Max(1f, syncedRunDurationSeconds));
            string formattedTime = span.TotalHours >= 1.0
                ? $"{(int)span.TotalHours:00} h {span.Minutes:00} min {span.Seconds:00} s"
                : $"{span.Minutes:00} min {span.Seconds:00} s";

            string modeStr = (LanSessionManager.Instance != null && LanSessionManager.Instance.ConnectedPlayerCount >= 2)
                ? "Cooperativo Asimétrico en Red Local (2 Jugadores: 1929 + Ruinas)"
                : "Modo Solitario (Alternancia Temporal 1929 ↔ Ruinas con [Q])";

            DetermineEndingTier(out string tierBadge, out string tierTitle);

            if (statsBodyText != null)
            {
                statsBodyText.text =
                    $"<b>• Modalidad jugada:</b> <color=#F3D692>{modeStr}</color>\n" +
                    $"<b>• Tiempo de la sesión:</b> <color=#F3D692>{formattedTime}</color>\n" +
                    $"<b>• Noches superadas:</b> <color=#6EE88E>8 / 8 Noches (De las 20:15 h a las 06:00 AM)</color>\n" +
                    $"<b>• Evidencias, Documentos y Coleccionables en el Diario:</b> <color=#F3D692>{syncedEvidencesCount} registrados</color>\n" +
                    $"<b>• Secretos especiales y Revelaciones desbloqueadas:</b> <color=#F3D692>{syncedSecretsCount} / 8 descubiertos</color>\n" +
                    $"<b>• Anomalías asimétricas presenciadas:</b> <color=#F3D692>{syncedAnomaliesCount} registradas</color>\n" +
                    $"<b>• Logros cooperativos obtenidos:</b> <color=#6EE88E>{syncedAchievementsCount} desbloqueados</color>\n" +
                    $"<b>• Desenlace alcanzado:</b> <color=#F3D692>{tierTitle}</color>";
            }

            if (statsAchievementsListText != null)
            {
                List<string> achNames = new List<string>();
                if (SharedEvidenceJournalManager.Instance != null && SharedEvidenceJournalManager.Instance.UnlockedAchievements != null)
                {
                    foreach (string a in SharedEvidenceJournalManager.Instance.UnlockedAchievements)
                    {
                        achNames.Add(FormatAchievementFriendlyName(a));
                    }
                }
                if (achNames.Count == 0)
                {
                    achNames.Add("«El Último Acto» (Completar las 8 Noches y unir las épocas a las 06:00 AM)");
                }
                statsAchievementsListText.text =
                    "<b>HITOS Y LOGROS COOPERATIVOS REGISTRADOS EN ESTA PARTIDA:</b>\n" +
                    "• " + string.Join("\n• ", achNames);
            }

            waitingForStatsContinue = true;
            float autoStatsTimer = 0f;
            while (waitingForStatsContinue && autoStatsTimer < 22.0f)
            {
                autoStatsTimer += Time.unscaledDeltaTime;
                yield return null;
            }

            if (statsPanel != null) statsPanel.SetActive(false);
        }

        private static string FormatAchievementFriendlyName(string id)
        {
            if (string.IsNullOrEmpty(id)) return "Hito Cooperativo";
            string lower = id.ToLowerInvariant();
            if (lower.Contains("voz")) return "«Voz en la oscuridad» — Resolver un puzle de clave dividida comunicándose entre épocas";
            if (lower.Contains("sincron")) return "«Sincronía» — Ejecutar una acción simultánea perfecta entre 1929 y las Ruinas";
            if (lower.Contains("dos_tiempos") || lower.Contains("tiempos")) return "«Dos tiempos» — Despejar las Ruinas mediante un Efecto Espejo desde 1929";
            return $"«{id}»";
        }

        // =========================================================================================
        // 5. CRÉDITOS DESPLAZABLES (MANTENER [ESC] PARA SALTAR) Y VUELTA LIMPIA AL MENÚ
        // =========================================================================================

        private IEnumerator RunScrollingCreditsAndReturnMenuRoutine()
        {
            if (creditsPanel != null) creditsPanel.SetActive(true);
            isCreditsPhaseActive = true;
            creditsFinishedOrSkipped = false;
            holdEscapeTimer = 0f;

            if (creditsScrollContentRt != null)
            {
                creditsScrollContentRt.anchoredPosition = new Vector2(0f, 0f);
            }

            PlayDawnBellAndMusicBoxChord(2);

            float targetScrollY = 2150f;
            float scrollSpeed = 62f; // Desplazamiento suave y legible

            while (!creditsFinishedOrSkipped && creditsScrollContentRt != null && creditsScrollContentRt.anchoredPosition.y < targetScrollY)
            {
                Vector2 pos = creditsScrollContentRt.anchoredPosition;
                pos.y += scrollSpeed * Time.unscaledDeltaTime;
                creditsScrollContentRt.anchoredPosition = pos;
                yield return null;
            }

            // Al terminar el desplazamiento (o tras mantener [ESC] 1.8 s), llevar al bloque final de agradecimiento
            if (creditsScrollContentRt != null)
            {
                creditsScrollContentRt.anchoredPosition = new Vector2(0f, targetScrollY);
            }
            if (holdEscapePromptText != null)
            {
                holdEscapePromptText.text = "Fin de los créditos · La pantalla «COLECCIÓN» ha quedado desbloqueada en el Menú Principal";
            }
            if (holdEscapeFillBar != null)
            {
                holdEscapeFillBar.fillAmount = 1f;
            }
        }

        /// <summary>
        /// Botón "VOLVER AL MENÚ PRINCIPAL":
        /// Cierra la red limpiamente (Host y Cliente), detiene el micrófono/voz, restaura timeScale
        /// y carga la escena "MainMenu".
        /// </summary>
        public void OnReturnToMainMenuClicked()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Detener captura de micrófono de LanVoiceChatManager limpiamente
            if (LanVoiceChatManager.Instance != null)
            {
                LanVoiceChatManager.Instance.StopMicrophoneCapture();
            }

            // Cerrar red limpiamente (Host y Cliente) y volver al Menú Principal
            if (LanSessionManager.Instance != null)
            {
                LanSessionManager.Instance.ShutdownNetworkSession();
            }
            SceneManager.LoadScene("MainMenu");
        }

        private void SaveCompletedRunToPrefs()
        {
            int tier = DetermineEndingTier(out _, out _);

            PlayerPrefs.SetInt(PrefsGameCompletedKey, 1);
            PlayerPrefs.SetInt(PrefsBestEvidencesKey, Mathf.Max(PlayerPrefs.GetInt(PrefsBestEvidencesKey, 0), syncedEvidencesCount));
            PlayerPrefs.SetInt(PrefsBestSecretsKey, Mathf.Max(PlayerPrefs.GetInt(PrefsBestSecretsKey, 0), syncedSecretsCount));
            PlayerPrefs.SetInt(PrefsBestAnomaliesKey, Mathf.Max(PlayerPrefs.GetInt(PrefsBestAnomaliesKey, 0), syncedAnomaliesCount));
            PlayerPrefs.SetInt(PrefsBestAchievementsKey, Mathf.Max(PlayerPrefs.GetInt(PrefsBestAchievementsKey, 0), syncedAchievementsCount));
            PlayerPrefs.SetFloat(PrefsLastRunTimeSecondsKey, syncedRunDurationSeconds);
            PlayerPrefs.SetInt(PrefsUnlockedEndingTierKey, Mathf.Max(PlayerPrefs.GetInt(PrefsUnlockedEndingTierKey, 0), tier));

            // Asegurar que el índice de noche guardada no apunte a una noche 9 inexistente:
            // se reinicia a 0 (Noche I) para "Rejugar / Nueva Partida", conservando intacta la Colección.
            PlayerPrefs.SetInt(NightManager.PrefsKey, 0);
            PlayerPrefs.Save();

            if (SharedEvidenceJournalManager.Instance != null)
            {
                SharedEvidenceJournalManager.Instance.SaveToHostIfAuthoritative();
            }
        }

        // =========================================================================================
        // CONSTRUCCIÓN DE LA INTERFAZ DEL FINAL, EPÍLOGO, ESTADÍSTICAS Y CRÉDITOS
        // =========================================================================================

        private void BuildFinaleCanvasUI()
        {
            GameObject canvasObj = new GameObject("UltimateFinaleAndCredits_Canvas");
            canvasObj.transform.SetParent(transform, false);
            finaleCanvas = canvasObj.AddComponent<Canvas>();
            finaleCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            finaleCanvas.sortingOrder = 140; // Por encima del HUD y tarjetas de noche

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // 1. Velo de transición cinematográfica
            GameObject veilObj = new GameObject("CinematicVeil");
            veilObj.transform.SetParent(canvasObj.transform, false);
            RectTransform veilRt = veilObj.AddComponent<RectTransform>();
            veilRt.anchorMin = Vector2.zero;
            veilRt.anchorMax = Vector2.one;
            veilRt.offsetMin = Vector2.zero;
            veilRt.offsetMax = Vector2.zero;
            veilTintImage = veilObj.AddComponent<Image>();
            veilTintImage.color = new Color(0f, 0f, 0f, 1f);
            veilTintImage.raycastTarget = false;
            veilCanvasGroup = veilObj.AddComponent<CanvasGroup>();
            veilCanvasGroup.alpha = 0f;

            // 2. Banner cinematográfico durante la fusión del Amanecer 06:00 AM
            BuildDawnBannerUI(canvasObj.transform);

            // 3. Panel de Epílogo Narrativo
            BuildEpiloguePanelUI(canvasObj.transform);

            // 4. Panel de Estadísticas de la Partida
            BuildStatisticsPanelUI(canvasObj.transform);

            // 5. Panel de Créditos Desplazables + Botón Volver al Menú Principal
            BuildCreditsPanelUI(canvasObj.transform);

            finaleCanvas.gameObject.SetActive(false);
        }

        private void BuildDawnBannerUI(Transform parent)
        {
            GameObject bannerObj = new GameObject("Dawn0600_CinematicBanner");
            bannerObj.transform.SetParent(parent, false);
            RectTransform rt = bannerObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 44f);
            rt.sizeDelta = new Vector2(1180f, 150f);

            Image bg = bannerObj.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.02f, 0.03f, 0.88f);
            dawnBannerGroup = bannerObj.AddComponent<CanvasGroup>();
            dawnBannerGroup.alpha = 0f;

            dawnClockLabel = CreateLabel(bannerObj.transform, "DawnClock", new Vector2(0f, 46f), new Vector2(1100f, 32f),
                17f, FontStyles.Bold, ColDawnGold, TextAlignmentOptions.Center);
            dawnPhaseTitleLabel = CreateLabel(bannerObj.transform, "DawnTitle", new Vector2(0f, 12f), new Vector2(1100f, 38f),
                25f, FontStyles.Bold, ColIvory, TextAlignmentOptions.Center);
            dawnPhaseSubtitleLabel = CreateLabel(bannerObj.transform, "DawnSubtitle", new Vector2(0f, -34f), new Vector2(1100f, 52f),
                16.5f, FontStyles.Italic, new Color(0.92f, 0.88f, 0.80f, 1f), TextAlignmentOptions.Center);

            bannerObj.SetActive(false);
        }

        private void BuildEpiloguePanelUI(Transform parent)
        {
            epiloguePanel = CreateFullOverlayCard(parent, "Epilogue_OverlayPanel", new Vector2(1160f, 700f), out Transform cardRoot);
            epilogueCanvasGroup = epiloguePanel.GetComponent<CanvasGroup>();

            epilogueTierBadgeText = CreateLabel(cardRoot, "TierBadge", new Vector2(0f, 290f), new Vector2(1060f, 28f),
                14.5f, FontStyles.Bold, ColDawnGold, TextAlignmentOptions.Center);
            epilogueHeadingText = CreateLabel(cardRoot, "Heading", new Vector2(0f, 248f), new Vector2(1060f, 42f),
                26f, FontStyles.Bold, ColIvory, TextAlignmentOptions.Center);

            epilogueBodyText = CreateLabel(cardRoot, "BodyText", new Vector2(0f, 15f), new Vector2(1020f, 380f),
                18.5f, FontStyles.Normal, ColIvory, TextAlignmentOptions.TopLeft);
            epilogueBodyText.lineSpacing = 8f;

            epiloguePageCounterText = CreateLabel(cardRoot, "PageCounter", new Vector2(-320f, -278f), new Vector2(400f, 32f),
                15f, FontStyles.Italic, ColDawnGoldSoft, TextAlignmentOptions.MidlineLeft);

            btnNextEpilogueSlide = CreateActionButton(cardRoot, "BtnNextSlide", "SIGUIENTE CAPÍTULO DEL EPÍLOGO →",
                new Vector2(280f, -278f), new Vector2(420f, 48f), () => { waitingForEpilogueContinue = false; });
            btnNextEpilogueLabel = btnNextEpilogueSlide.GetComponentInChildren<TextMeshProUGUI>();

            epiloguePanel.SetActive(false);
        }

        private void BuildStatisticsPanelUI(Transform parent)
        {
            statsPanel = CreateFullOverlayCard(parent, "Stats_OverlayPanel", new Vector2(1160f, 720f), out Transform cardRoot);
            statsCanvasGroup = statsPanel.GetComponent<CanvasGroup>();

            CreateLabel(cardRoot, "StatsSubtitle", new Vector2(0f, 302f), new Vector2(1060f, 26f),
                14.5f, FontStyles.Bold, ColDawnGold, TextAlignmentOptions.Center).text =
                "ACADEMIA VESPER — EXPEDIENTE CERRADO A LAS 06:00 AM";

            CreateLabel(cardRoot, "StatsTitle", new Vector2(0f, 262f), new Vector2(1060f, 40f),
                27f, FontStyles.Bold, ColIvory, TextAlignmentOptions.Center).text =
                "ESTADÍSTICAS FINALES DE LA PARTIDA";

            statsBodyText = CreateLabel(cardRoot, "StatsBody", new Vector2(0f, 82f), new Vector2(1000f, 260f),
                18f, FontStyles.Normal, ColIvory, TextAlignmentOptions.TopLeft);
            statsBodyText.lineSpacing = 10f;

            statsAchievementsListText = CreateLabel(cardRoot, "StatsAchievements", new Vector2(0f, -135f), new Vector2(1000f, 150f),
                15.5f, FontStyles.Normal, new Color(0.92f, 0.86f, 0.72f, 1f), TextAlignmentOptions.TopLeft);
            statsAchievementsListText.lineSpacing = 6f;

            btnProceedToCredits = CreateActionButton(cardRoot, "BtnToCredits", "CONTINUAR A LOS CRÉDITOS FINALES →",
                new Vector2(0f, -285f), new Vector2(440f, 50f), () => { waitingForStatsContinue = false; });

            statsPanel.SetActive(false);
        }

        private void BuildCreditsPanelUI(Transform parent)
        {
            creditsPanel = CreateFullOverlayCard(parent, "Credits_OverlayPanel", new Vector2(1240f, 820f), out Transform cardRoot);
            creditsCanvasGroup = creditsPanel.GetComponent<CanvasGroup>();

            CreateLabel(cardRoot, "CreditsTopHeader", new Vector2(0f, 366f), new Vector2(1140f, 32f),
                16f, FontStyles.Bold, ColDawnGold, TextAlignmentOptions.Center).text =
                "ÚLTIMO ACTO — CRÉDITOS OFICIALES Y LICENCIAS";

            // Ventana de recorte (Mask) para el texto desplazable de los créditos
            GameObject viewportObj = new GameObject("CreditsViewport");
            viewportObj.transform.SetParent(cardRoot, false);
            RectTransform vpRt = viewportObj.AddComponent<RectTransform>();
            vpRt.anchorMin = new Vector2(0.5f, 0.5f);
            vpRt.anchorMax = new Vector2(0.5f, 0.5f);
            vpRt.pivot = new Vector2(0.5f, 0.5f);
            vpRt.anchoredPosition = new Vector2(0f, 28f);
            vpRt.sizeDelta = new Vector2(1120f, 580f);

            Image vpImg = viewportObj.AddComponent<Image>();
            vpImg.color = new Color(0.03f, 0.01f, 0.02f, 0.65f);
            Mask vpMask = viewportObj.AddComponent<Mask>();
            vpMask.showMaskGraphic = true;

            GameObject contentObj = new GameObject("CreditsScrollingContent");
            contentObj.transform.SetParent(viewportObj.transform, false);
            creditsScrollContentRt = contentObj.AddComponent<RectTransform>();
            creditsScrollContentRt.anchorMin = new Vector2(0.5f, 1f);
            creditsScrollContentRt.anchorMax = new Vector2(0.5f, 1f);
            creditsScrollContentRt.pivot = new Vector2(0.5f, 1f);
            creditsScrollContentRt.anchoredPosition = Vector2.zero;
            creditsScrollContentRt.sizeDelta = new Vector2(1040f, 2750f);

            TextMeshProUGUI creditsTmp = contentObj.AddComponent<TextMeshProUGUI>();
            creditsTmp.fontSize = 18f;
            creditsTmp.color = ColIvory;
            creditsTmp.alignment = TextAlignmentOptions.Top;
            creditsTmp.lineSpacing = 8f;
            creditsTmp.raycastTarget = false;
            creditsTmp.text = BuildCreditsFullSpanishText();

            // Barra de pulsación mantenida de [ESC] para saltar el scroll sin salto accidental al primer toque
            GameObject skipBarBg = new GameObject("HoldEscapeBarBg");
            skipBarBg.transform.SetParent(cardRoot, false);
            RectTransform sbRt = skipBarBg.AddComponent<RectTransform>();
            sbRt.anchorMin = new Vector2(0.5f, 0.5f);
            sbRt.anchorMax = new Vector2(0.5f, 0.5f);
            sbRt.pivot = new Vector2(0.5f, 0.5f);
            sbRt.anchoredPosition = new Vector2(-220f, -312f);
            sbRt.sizeDelta = new Vector2(640f, 10f);
            Image sbBgImg = skipBarBg.AddComponent<Image>();
            sbBgImg.color = new Color(0.22f, 0.16f, 0.18f, 0.9f);

            GameObject skipBarFill = new GameObject("HoldEscapeBarFill");
            skipBarFill.transform.SetParent(skipBarBg.transform, false);
            RectTransform sfRt = skipBarFill.AddComponent<RectTransform>();
            sfRt.anchorMin = Vector2.zero;
            sfRt.anchorMax = Vector2.one;
            sfRt.offsetMin = Vector2.zero;
            sfRt.offsetMax = Vector2.zero;
            holdEscapeFillBar = skipBarFill.AddComponent<Image>();
            holdEscapeFillBar.color = ColDawnGold;
            holdEscapeFillBar.type = Image.Type.Filled;
            holdEscapeFillBar.fillMethod = Image.FillMethod.Horizontal;
            holdEscapeFillBar.fillAmount = 0f;

            holdEscapePromptText = CreateLabel(cardRoot, "HoldEscapeText", new Vector2(-220f, -336f), new Vector2(660f, 28f),
                13.5f, FontStyles.Italic, ColDawnGoldSoft, TextAlignmentOptions.Center);
            holdEscapePromptText.text = "Mantén pulsado [ESC] durante 2 s para saltar el desplazamiento · Rueda del ratón para desplazar";

            // Botón siempre accesible "VOLVER AL MENÚ PRINCIPAL"
            btnReturnToMainMenu = CreateActionButton(cardRoot, "BtnReturnMainMenu", "VOLVER AL MENÚ PRINCIPAL",
                new Vector2(340f, -324f), new Vector2(380f, 52f), OnReturnToMainMenuClicked);

            creditsPanel.SetActive(false);
        }

        private static string BuildCreditsFullSpanishText()
        {
            return
                "\n<size=32><b><color=#F3D692>ÚLTIMO ACTO</color></b></size>\n" +
                "<size=19><i>«El Lago de las Sombras — Academia Vesper (1929 – Actualidad)»</i></size>\n\n" +
                "──────────────────────────────────────────────────────\n\n" +
                "<size=22><b><color=#F3D692>PROYECTO ACADÉMICO Y DESARROLLO</color></b></size>\n" +
                "<b>Materia:</b> Realidad Virtual (RV) — Proyecto Final\n" +
                "<b>Semestre y Curso:</b> 6.º Semestre — Ingeniería de Sistemas / Ciencias de la Computación\n" +
                "<b>Dirección, Diseño Narrativo, Arquitectura Dual y Programación en Red:</b>\n" +
                "Equipo de Desarrollo de <i>Último Acto</i> (Liz & Equipo RV)\n\n" +
                "──────────────────────────────────────────────────────\n\n" +
                "<size=22><b><color=#F3D692>DRAMATIS PERSONAE — ACADEMIA VESPER (1929)</color></b></size>\n" +
                "<b>Elena Vesper</b> — <i>El Cisne Negro (Primera Bailarina, desaparecida el 14 de octubre de 1929)</i>\n" +
                "<b>Mara Bell</b> — <i>El Cisne Blanco (Guardiana de la mitad de la partitura original)</i>\n" +
                "<b>Isadora Vale</b> — <i>Directora Artística de la Academia Vesper</i>\n" +
                "<b>Julian Krol</b> — <i>Maestro Compositor y Concertador del Compás 48</i>\n" +
                "<b>Madame Hélène</b> — <i>Maestra Costurera del Taller y Tintorería</i>\n" +
                "<b>Dr. Aurelio Morales</b> — <i>Médico de la Enfermería del Ala Oeste</i>\n" +
                "<b>Tomás</b> — <i>Maestro Tramoyista de la Sala de Poleas y Contrapesos</i>\n\n" +
                "<size=22><b><color=#F3D692>PROTAGONISTAS (INVESTIGADORES DEL PRESENTE)</color></b></size>\n" +
                "<b>Daniela Bell</b> — <i>Percepción de la Época 1929 (Anfitrión)</i>\n" +
                "<b>Mateo Krol</b> — <i>Percepción de las Ruinas Actuales (Invitado)</i>\n\n" +
                "──────────────────────────────────────────────────────\n\n" +
                "<size=22><b><color=#F3D692>TECNOLOGÍA, HERRAMIENTAS Y SISTEMAS</color></b></size>\n" +
                "• <b>Motor Gráfico:</b> Unity 6 (6000.5.6f1) — Universal Render Pipeline (URP 17.5.0)\n" +
                "• <b>Arquitectura Multijugador LAN:</b> Unity Netcode for GameObjects + Descubrimiento UDP Broadcast (Puertos 47777–47778)\n" +
                "• <b>Chat de Voz en Tiempo Real:</b> Captura y Transmisión PCM 16 kHz Mono con Eco Temporal por Época\n" +
                "• <b>Geometría y Escenografía:</b> Construcción Procedural Completa de 12 Secciones y 59 Zonas con Doble Capa Visual (Era1929 / EraRuins)\n" +
                "• <b>Banda Sonora y Efectos Acústicos:</b> Síntesis Procedural en Tiempo Real (Piano de Cola, Violonchelo en Do Natural, Cajita Musical y Campanas)\n\n" +
                "──────────────────────────────────────────────────────\n\n" +
                "<size=22><b><color=#F3D692>ASSETS, TIPOGRAFÍAS Y LICENCIAS</color></b></size>\n" +
                "• <b>Modelado Arquitectónico, Mobiliario y Materiales de Época:</b> Generación Procedural Propia en C# (Dominio del Proyecto / Libre de Regalías)\n" +
                "• <b>Efectos Sonoros y Síntesis Musical:</b> Síntesis Matemática PCM en Tiempo Real — Licencia <b>CC0 1.0 Universal (Dominio Público)</b>\n" +
                "• <b>Tipografía e Interfaz:</b> TextMeshPro (Unity Technologies) & Texturas de Retratos Generadas para el Proyecto\n\n" +
                "──────────────────────────────────────────────────────\n\n" +
                "<size=22><b><color=#F3D692>AGRADECIMIENTOS ESPECIALES</color></b></size>\n" +
                "A nuestro docente y compañeros de la asignatura de Realidad Virtual (6.º Semestre),\n" +
                "y a cada pareja de jugadores que cruzó sus voces entre 1929 y las Ruinas\n" +
                "para escuchar el último acorde de Elena Vesper y Mara Bell.\n\n" +
                "<size=24><b><color=#F3D692>¡GRACIAS POR JUGAR «ÚLTIMO ACTO»!</color></b></size>\n\n";
        }

        private static GameObject CreateFullOverlayCard(Transform parent, string name, Vector2 cardSize, out Transform cardInterior)
        {
            GameObject backdrop = new GameObject(name);
            backdrop.transform.SetParent(parent, false);
            RectTransform bgRt = backdrop.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            Image bgImg = backdrop.AddComponent<Image>();
            bgImg.color = ColDarkOperaBg;
            backdrop.AddComponent<CanvasGroup>();

            GameObject borderObj = new GameObject("GoldBorder");
            borderObj.transform.SetParent(backdrop.transform, false);
            RectTransform bRt = borderObj.AddComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.5f, 0.5f);
            bRt.anchorMax = new Vector2(0.5f, 0.5f);
            bRt.pivot = new Vector2(0.5f, 0.5f);
            bRt.anchoredPosition = Vector2.zero;
            bRt.sizeDelta = cardSize;

            Image borderImg = borderObj.AddComponent<Image>();
            borderImg.color = ColDawnGoldSoft;

            GameObject cardObj = new GameObject("CardInterior");
            cardObj.transform.SetParent(borderObj.transform, false);
            RectTransform cRt = cardObj.AddComponent<RectTransform>();
            cRt.anchorMin = Vector2.zero;
            cRt.anchorMax = Vector2.one;
            cRt.offsetMin = new Vector2(2f, 2f);
            cRt.offsetMax = new Vector2(-2f, -2f);

            Image cImg = cardObj.AddComponent<Image>();
            cImg.color = ColCardBg;

            cardInterior = cardObj.transform;
            return backdrop;
        }

        private static TextMeshProUGUI CreateLabel(
            Transform parent,
            string name,
            Vector2 anchoredPos,
            Vector2 size,
            float fontSize,
            FontStyles fontStyle,
            Color color,
            TextAlignmentOptions alignment)
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
            tmp.fontStyle = fontStyle;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button CreateActionButton(
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
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = ColBtnWine;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;
            if (onClick != null) btn.onClick.AddListener(onClick);

            GameObject border = new GameObject("GoldBottomLine");
            border.transform.SetParent(btnObj.transform, false);
            RectTransform brt = border.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0f, 0f);
            brt.anchorMax = new Vector2(1f, 0f);
            brt.pivot = new Vector2(0.5f, 0f);
            brt.anchoredPosition = Vector2.zero;
            brt.sizeDelta = new Vector2(0f, 2f);
            Image bImg = border.AddComponent<Image>();
            bImg.color = ColDawnGold;
            bImg.raycastTarget = false;

            GameObject txtObj = new GameObject("Label");
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform trt = txtObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(10f, 4f);
            trt.offsetMax = new Vector2(-10f, -4f);

            TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 15.5f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = ColIvory;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            return btn;
        }

        private IEnumerator FadeVeilTo(Color targetColor, float duration)
        {
            if (veilCanvasGroup == null || veilTintImage == null) yield break;

            Color startColor = veilTintImage.color;
            float startAlpha = veilCanvasGroup.alpha;
            float targetAlpha = targetColor.a;
            veilTintImage.color = new Color(targetColor.r, targetColor.g, targetColor.b, 1f);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
                veilTintImage.color = Color.Lerp(
                    new Color(startColor.r, startColor.g, startColor.b, 1f),
                    new Color(targetColor.r, targetColor.g, targetColor.b, 1f),
                    t);
                veilCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            veilCanvasGroup.alpha = targetAlpha;
        }

        // =========================================================================================
        // SÍNTESIS ACÚSTICA PROCEDURAL DEL AMANECER (06:00 AM) Y LA CAJITA MUSICAL DE ELENA
        // =========================================================================================

        private void SetupFinaleAudio()
        {
            finaleAudioSource = gameObject.AddComponent<AudioSource>();
            finaleAudioSource.spatialBlend = 0f;
            finaleAudioSource.playOnAwake = false;
            finaleAudioSource.volume = 0.48f;
        }

        private void PlayDawnBellAndMusicBoxChord(int progressionStage)
        {
            if (finaleAudioSource == null) return;

            int sampleRate = 22050;
            float duration = 2.8f;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];

            // Acordes del Compás 48 resuelto: Mi menor -> Do Natural Mayor -> Mi Mayor luminoso (Amanecer 06:00)
            float[] freqs = progressionStage == 0
                ? new float[] { 164.81f, 196.00f, 246.94f, 261.63f }
                : (progressionStage == 1
                    ? new float[] { 261.63f, 329.63f, 392.00f, 523.25f }
                    : new float[] { 164.81f, 246.94f, 329.63f, 415.30f, 659.25f });

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 1.15f) * Mathf.Clamp01(t / 0.04f);
                float sum = 0f;
                for (int n = 0; n < freqs.Length; n++)
                {
                    float noteDelay = n * 0.16f;
                    if (t >= noteDelay)
                    {
                        float lt = t - noteDelay;
                        float noteEnv = Mathf.Exp(-lt * 1.4f);
                        sum += Mathf.Sin(2f * Mathf.PI * freqs[n] * lt) * noteEnv * 0.22f;
                    }
                }
                data[i] = Mathf.Clamp(sum * env, -0.85f, 0.85f);
            }

            AudioClip clip = AudioClip.Create($"Dawn0600_Chord_{progressionStage}", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            finaleAudioSource.PlayOneShot(clip, 0.55f);
        }
    }
}
