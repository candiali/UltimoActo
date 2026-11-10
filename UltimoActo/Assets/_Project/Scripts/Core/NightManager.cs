using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UltimoActo.Interaction;
using UltimoActo.Player;
using UltimoActo.UI;

namespace UltimoActo.Core
{
    /// <summary>
    /// Definición de una noche de la investigación: título, objetivo, atmósfera y puertas que se abren.
    /// </summary>
    [Serializable]
    public class NightDefinition
    {
        public string roman;
        public string title;
        public string clockTime;
        public string subtitle;
        public string objective;
        [Tooltip("Evidencias nuevas (documentos / secretos leídos por primera vez) necesarias para terminar la noche")]
        public int evidenceQuota = 6;

        [Header("Atmósfera")]
        public float fogDensity = 0.007f;
        public Color fogColor = new Color(0.06f, 0.08f, 0.13f);
        public Color ambientSky = new Color(0.09f, 0.12f, 0.18f);
        public Color moonColor = new Color(0.68f, 0.80f, 0.98f);
        public float moonIntensity = 0.85f;
        [Tooltip("Multiplicador de la intensidad de las lámparas (luces puntuales). Baja con las noches")]
        public float lampScale = 1.0f;

        [Header("Accesos")]
        [Tooltip("Ids de puertas que dejan de estar selladas a partir de esta noche")]
        public string[] doorsUnlockedThisNight = new string[0];
    }

    /// <summary>
    /// Sistema de noches (Fase 1 del plan de duración):
    ///  - 8 noches consecutivas, cada una con su objetivo, su atmósfera y sus accesos.
    ///  - Las puertas de las alas todavía no abiertas permanecen selladas hasta su noche.
    ///  - Cada documento / secreto leído por primera vez cuenta como evidencia (DocumentUI.ShowDocument).
    ///  - Al llegar a la cuota, "amanece": fundido a negro, tarjeta de título y regreso al vestíbulo.
    ///
    /// Se crea solo al cargar una escena que contenga al jugador (no requiere cambios en el builder).
    /// El estado está pensado para sincronizarse más adelante con la capa de red (Fase cooperativa).
    /// </summary>
    public class NightManager : MonoBehaviour
    {
        public const string PrefsKey = "UltimoActo_Night_Index";

        public static NightManager Instance { get; private set; }

        [Header("Progreso")]
        [Tooltip("Si está activo, la noche actual se recuerda entre sesiones (requiere sistema de guardado de llaves/objetos)")]
        [SerializeField] private bool persistProgress = false;
        [SerializeField] private int currentNightIndex = 0;

        [Header("Noches")]
        [SerializeField] private List<NightDefinition> nights = new List<NightDefinition>();

        public event Action<NightDefinition, int> OnNightStarted;
        public event Action<int, int> OnEvidenceChanged;
        public event Action<int> OnNightCompleted;

        public int CurrentNightIndex => currentNightIndex;
        public int NightCount => nights.Count;
        public NightDefinition Current => nights[Mathf.Clamp(currentNightIndex, 0, nights.Count - 1)];
        public int EvidenceThisNight => evidenceThisNight;
        public bool IsTransitioning => isTransitioning;

        private readonly HashSet<string> globalEvidence = new HashSet<string>();
        private readonly Dictionary<string, int> doorUnlockNight = new Dictionary<string, int>();
        private readonly Dictionary<Light, float> baseLampIntensity = new Dictionary<Light, float>();
        private readonly List<WorldDoor> doors = new List<WorldDoor>();
        private readonly List<WorldDoubleDoor> doubleDoors = new List<WorldDoubleDoor>();

        private int evidenceThisNight;
        private bool awaitingDawn;
        private bool isTransitioning;
        private bool gameFinished;
        private Coroutine dawnRoutine;
        private Coroutine toastRoutine;

        private Transform playerTransform;
        private CharacterController playerController;
        private PlayerMovement playerMovement;
        private PlayerCameraLook playerLook;
        private Vector3 startPos;
        private float startYaw;
        private bool hasStartPose;
        private Light moonLight;

        // --- HUD ---
        private RectTransform panelRoot;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI bodyText;
        private RectTransform barFill;
        private CanvasGroup toastGroup;
        private TextMeshProUGUI toastText;
        private Image fadeImage;
        private CanvasGroup cardGroup;
        private TextMeshProUGUI cardRoman;
        private TextMeshProUGUI cardTitle;
        private TextMeshProUGUI cardSubtitle;
        private TextMeshProUGUI cardClock;

        // ====================================================================
        // ARRANQUE AUTOMÁTICO
        // ====================================================================
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            if (FindFirstObjectByType<PlayerMovement>() == null) return;
            new GameObject("NightManager").AddComponent<NightManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (nights == null || nights.Count == 0)
            {
                nights = BuildDefaultNights();
            }

            for (int i = 0; i < nights.Count; i++)
            {
                string[] ids = nights[i].doorsUnlockedThisNight;
                if (ids == null) continue;
                foreach (string id in ids)
                {
                    if (!string.IsNullOrEmpty(id)) doorUnlockNight[id] = i;
                }
            }

            if (persistProgress)
            {
                currentNightIndex = Mathf.Clamp(PlayerPrefs.GetInt(PrefsKey, 0), 0, nights.Count - 1);
            }
            else
            {
                currentNightIndex = 0;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
                if (pm != null) playerObj = pm.gameObject;
            }
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                playerController = playerObj.GetComponent<CharacterController>();
                playerMovement = playerObj.GetComponent<PlayerMovement>();
                playerLook = playerObj.GetComponentInChildren<PlayerCameraLook>();
                if (playerLook == null) playerLook = FindFirstObjectByType<PlayerCameraLook>();
                startPos = playerTransform.position;
                startYaw = playerTransform.eulerAngles.y;
                hasStartPose = true;
            }
            if (playerLook == null) playerLook = FindFirstObjectByType<PlayerCameraLook>();
            if (playerMovement == null) playerMovement = FindFirstObjectByType<PlayerMovement>();

            GameObject moonObj = GameObject.Find("Moonlight_Cold");
            if (moonObj != null) moonLight = moonObj.GetComponent<Light>();

            foreach (Light l in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (l.type == LightType.Point) baseLampIntensity[l] = l.intensity;
            }
            doors.AddRange(FindObjectsByType<WorldDoor>(FindObjectsSortMode.None));
            doubleDoors.AddRange(FindObjectsByType<WorldDoubleDoor>(FindObjectsSortMode.None));

            BuildHud();
            StartCoroutine(TransitionToNight(currentNightIndex, true));
        }

        // ====================================================================
        // API PÚBLICA
        // ====================================================================
        public bool IsNightLockedDoor(string doorId, out string reason)
        {
            reason = string.Empty;
            if (string.IsNullOrEmpty(doorId)) return false;
            if (!doorUnlockNight.TryGetValue(doorId, out int unlockNight)) return false;
            if (unlockNight <= currentNightIndex) return false;
            reason = $"Sellada — la academia no te dejará pasar hasta la Noche {nights[unlockNight].roman}";
            return true;
        }

        /// <summary>Registra una evidencia (documento, secreto, anomalía). Solo cuenta la primera vez que se lee.</summary>
        public void RegisterEvidence(string evidenceId)
        {
            if (string.IsNullOrEmpty(evidenceId) || gameFinished || isTransitioning) return;
            if (!globalEvidence.Add(evidenceId)) return;
            if (awaitingDawn) return;

            evidenceThisNight++;
            int quota = Current.evidenceQuota;
            OnEvidenceChanged?.Invoke(evidenceThisNight, quota);
            RefreshHud();

            if (evidenceThisNight >= quota)
            {
                OnNightCompleted?.Invoke(currentNightIndex);
                if (dawnRoutine != null) StopCoroutine(dawnRoutine);
                dawnRoutine = StartCoroutine(DawnSequence());
            }
            else
            {
                ShowToast($"Nueva evidencia registrada  ({evidenceThisNight}/{quota})", 3.0f);
            }
        }

        public void ShowToast(string message, float seconds = 3.5f)
        {
            if (toastText == null || toastGroup == null) return;
            toastText.text = message;
            if (toastRoutine != null) StopCoroutine(toastRoutine);
            toastRoutine = StartCoroutine(ToastRoutine(seconds));
        }

        /// <summary>Salta a la siguiente noche (depuración / presentación).</summary>
        public void DevSkipNight()
        {
            if (isTransitioning || gameFinished) return;
            if (dawnRoutine != null) StopCoroutine(dawnRoutine);
            dawnRoutine = null;
            AdvanceToNextNight();
        }

        // ====================================================================
        // FLUJO DE NOCHES
        // ====================================================================
        private IEnumerator DawnSequence()
        {
            awaitingDawn = true;
            RefreshHud();
            ShowToast("OBJETIVO CUMPLIDO — Las campanas de la torre anuncian el final de la noche...", 6.5f);
            yield return new WaitForSecondsRealtime(7.0f);
            dawnRoutine = null;
            AdvanceToNextNight();
        }

        private void AdvanceToNextNight()
        {
            if (currentNightIndex + 1 < nights.Count)
            {
                currentNightIndex++;
                if (persistProgress)
                {
                    PlayerPrefs.SetInt(PrefsKey, currentNightIndex);
                    PlayerPrefs.Save();
                }
                StartCoroutine(TransitionToNight(currentNightIndex, false));
            }
            else
            {
                StartCoroutine(EndingSequence());
            }
        }

        private IEnumerator TransitionToNight(int index, bool firstLoad)
        {
            isTransitioning = true;
            CloseOpenDocument();
            SetPlayerControl(false);

            if (firstLoad)
            {
                SetFade(1f);
            }
            else
            {
                yield return FadeTo(1f, 1.4f);
            }

            evidenceThisNight = 0;
            awaitingDawn = false;
            ApplyNight(index);
            if (!firstLoad) TeleportPlayerToNightSpawn(index);
            RefreshHud();
            OnNightStarted?.Invoke(nights[index], index);

            // Tarjeta de título
            NightDefinition d = nights[index];
            cardRoman.text = $"NOCHE {d.roman}";
            cardTitle.text = d.title;
            cardSubtitle.text = d.subtitle;
            cardClock.text = d.clockTime;

            yield return FadeCard(1f, 1.2f);
            yield return new WaitForSecondsRealtime(3.4f);

            SetPlayerControl(true);
            isTransitioning = false;

            StartCoroutine(FadeCard(0f, 1.6f));
            yield return FadeTo(0f, 1.6f);
        }

        private IEnumerator EndingSequence()
        {
            isTransitioning = true;
            gameFinished = true;
            CloseOpenDocument();
            SetPlayerControl(false);
            yield return FadeTo(1f, 2.0f);

            cardRoman.text = "FIN DE LAS OCHO NOCHES";
            cardTitle.text = "El telón desciende";
            cardSubtitle.text = "Has recorrido la Academia Vesper de principio a fin.\n(Continuará — final definitivo en desarrollo)";
            cardClock.text = "";
            yield return FadeCard(1f, 2.0f);
        }

        private void ApplyNight(int index)
        {
            NightDefinition d = nights[index];

            // Puertas selladas hasta su noche
            foreach (WorldDoor door in doors)
            {
                if (door == null) continue;
                if (IsNightLockedDoor(door.Id, out string reason)) door.SetNightLock(true, reason);
                else door.SetNightLock(false, string.Empty);
            }
            foreach (WorldDoubleDoor door in doubleDoors)
            {
                if (door == null) continue;
                if (IsNightLockedDoor(door.Id, out string reason)) door.SetNightLock(true, reason);
                else door.SetNightLock(false, string.Empty);
            }

            // Atmósfera
            RenderSettings.fog = true;
            RenderSettings.fogDensity = d.fogDensity;
            RenderSettings.fogColor = d.fogColor;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = d.ambientSky;
            RenderSettings.ambientEquatorColor = d.ambientSky * 0.66f;
            RenderSettings.ambientGroundColor = d.ambientSky * 0.22f;

            if (moonLight != null)
            {
                moonLight.color = d.moonColor;
                moonLight.intensity = d.moonIntensity;
            }

            foreach (KeyValuePair<Light, float> kv in baseLampIntensity)
            {
                if (kv.Key != null) kv.Key.intensity = kv.Value * d.lampScale;
            }
        }

        private void TeleportPlayerToNightSpawn(int index)
        {
            if (playerTransform == null) return;

            Vector3 pos;
            float yaw;
            if (index == 0 && hasStartPose)
            {
                pos = startPos;
                yaw = startYaw;
            }
            else
            {
                // Centro del Vestíbulo de Recepción (suelo en Y = 2.4m), mirando al norte
                float floorY = 2.4f;
                float pivotOffset = playerController != null
                    ? (playerController.height * 0.5f - playerController.center.y + playerController.skinWidth + 0.05f)
                    : 0.95f;
                pos = new Vector3(0f, floorY + pivotOffset, 16f);
                yaw = 0f;
            }

            if (playerController != null) playerController.enabled = false;
            playerTransform.position = pos;
            playerTransform.rotation = Quaternion.Euler(0f, yaw, 0f);
            if (playerController != null) playerController.enabled = true;
        }

        private void CloseOpenDocument()
        {
            if (DocumentUI.Instance != null && DocumentUI.Instance.IsOpen)
            {
                DocumentUI.Instance.CloseDocument();
            }
        }

        private void SetPlayerControl(bool enabledState)
        {
            if (playerMovement != null) playerMovement.enabled = enabledState;
            if (playerLook != null) playerLook.enabled = enabledState;
        }

        // ====================================================================
        // ENTRADA (teclas)
        // ====================================================================
        private void Update()
        {
            bool toggleHud = false;
            bool skip = false;
#if ENABLE_INPUT_SYSTEM
            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                toggleHud = kb.nKey.wasPressedThisFrame;
                skip = kb.f10Key.wasPressedThisFrame;
            }
#else
            toggleHud = Input.GetKeyDown(KeyCode.N);
            skip = Input.GetKeyDown(KeyCode.F10);
#endif
            if (toggleHud && panelRoot != null) panelRoot.gameObject.SetActive(!panelRoot.gameObject.activeSelf);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (skip) DevSkipNight();
#endif
        }

        // ====================================================================
        // HUD
        // ====================================================================
        private void RefreshHud()
        {
            if (titleText == null || nights.Count == 0) return;
            NightDefinition d = Current;
            titleText.text = $"NOCHE {d.roman} — {d.title}   ·   {d.clockTime}";

            if (awaitingDawn)
            {
                bodyText.text = "Objetivo cumplido. Esperando el amanecer...";
            }
            else
            {
                bodyText.text = $"{d.objective}\nEvidencias: {evidenceThisNight} / {d.evidenceQuota}";
            }

            if (barFill != null)
            {
                float t = d.evidenceQuota > 0 ? Mathf.Clamp01((float)evidenceThisNight / d.evidenceQuota) : 1f;
                barFill.anchorMax = new Vector2(t, 1f);
            }
        }

        private void BuildHud()
        {
            GameObject canvasObj = new GameObject("NightHUD_Canvas");
            canvasObj.transform.SetParent(transform, false);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            // Panel de objetivo (arriba a la izquierda)
            panelRoot = MakeRect("ObjectivePanel", canvasObj.transform,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(26f, -26f), new Vector2(780f, 128f));
            Image panelBg = panelRoot.gameObject.AddComponent<Image>();
            panelBg.color = new Color(0.04f, 0.04f, 0.06f, 0.74f);
            panelBg.raycastTarget = false;

            RectTransform border = MakeRect("GoldLine", panelRoot, Vector2.zero, new Vector2(0f, 1f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(4f, 0f));
            Image borderImg = border.gameObject.AddComponent<Image>();
            borderImg.color = new Color(0.85f, 0.70f, 0.38f, 0.9f);
            borderImg.raycastTarget = false;

            titleText = MakeText("Title", panelRoot, 24f, FontStyles.Bold, new Color(0.96f, 0.86f, 0.58f), TextAlignmentOptions.TopLeft);
            SetStretch(titleText.rectTransform, new Vector2(18f, 78f), new Vector2(-10f, -8f));

            bodyText = MakeText("Body", panelRoot, 20f, FontStyles.Normal, new Color(0.92f, 0.90f, 0.85f), TextAlignmentOptions.TopLeft);
            SetStretch(bodyText.rectTransform, new Vector2(18f, 16f), new Vector2(-10f, -42f));

            RectTransform barBg = MakeRect("BarBg", panelRoot, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(-8f, 7f));
            Image barBgImg = barBg.gameObject.AddComponent<Image>();
            barBgImg.color = new Color(0.20f, 0.18f, 0.16f, 0.9f);
            barBgImg.raycastTarget = false;

            barFill = MakeRect("BarFill", barBg, Vector2.zero, new Vector2(0f, 1f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
            barFill.offsetMin = Vector2.zero;
            barFill.offsetMax = Vector2.zero;
            Image barFillImg = barFill.gameObject.AddComponent<Image>();
            barFillImg.color = new Color(0.85f, 0.70f, 0.38f, 0.95f);
            barFillImg.raycastTarget = false;

            // Aviso inferior
            RectTransform toastRect = MakeRect("Toast", canvasObj.transform,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 190f), new Vector2(1300f, 60f));
            toastGroup = toastRect.gameObject.AddComponent<CanvasGroup>();
            toastGroup.alpha = 0f;
            toastGroup.blocksRaycasts = false;
            toastGroup.interactable = false;
            toastText = MakeText("ToastText", toastRect, 26f, FontStyles.Bold, new Color(0.96f, 0.90f, 0.72f), TextAlignmentOptions.Center);
            SetStretch(toastText.rectTransform, Vector2.zero, Vector2.zero);

            // Fundido a negro
            RectTransform fadeRect = MakeRect("Fade", canvasObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            fadeRect.offsetMin = Vector2.zero;
            fadeRect.offsetMax = Vector2.zero;
            fadeImage = fadeRect.gameObject.AddComponent<Image>();
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
            fadeImage.raycastTarget = false;

            // Tarjeta de título de noche
            RectTransform cardRect = MakeRect("TitleCard", canvasObj.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;
            cardGroup = cardRect.gameObject.AddComponent<CanvasGroup>();
            cardGroup.alpha = 0f;
            cardGroup.blocksRaycasts = false;
            cardGroup.interactable = false;

            cardRoman = MakeText("CardRoman", cardRect, 34f, FontStyles.Bold, new Color(0.85f, 0.70f, 0.38f), TextAlignmentOptions.Center);
            PlaceCentered(cardRoman.rectTransform, 120f, 70f, 1000f);
            cardTitle = MakeText("CardTitle", cardRect, 74f, FontStyles.Bold, new Color(0.96f, 0.93f, 0.86f), TextAlignmentOptions.Center);
            PlaceCentered(cardTitle.rectTransform, 30f, 110f, 1600f);
            cardSubtitle = MakeText("CardSubtitle", cardRect, 28f, FontStyles.Italic, new Color(0.80f, 0.78f, 0.74f), TextAlignmentOptions.Center);
            PlaceCentered(cardSubtitle.rectTransform, -80f, 120f, 1500f);
            cardClock = MakeText("CardClock", cardRect, 30f, FontStyles.Normal, new Color(0.70f, 0.78f, 0.92f), TextAlignmentOptions.Center);
            PlaceCentered(cardClock.rectTransform, -190f, 50f, 600f);
        }

        private static RectTransform MakeRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        private static TextMeshProUGUI MakeText(string name, Transform parent, float size, FontStyles style, Color color, TextAlignmentOptions align)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = align;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void SetStretch(RectTransform rt, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        private static void PlaceCentered(RectTransform rt, float y, float height, float width)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(width, height);
        }

        private void SetFade(float alpha)
        {
            if (fadeImage == null) return;
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
        }

        private IEnumerator FadeTo(float target, float duration)
        {
            if (fadeImage == null) yield break;
            float start = fadeImage.color.a;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                SetFade(Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t / duration)));
                yield return null;
            }
            SetFade(target);
        }

        private IEnumerator FadeCard(float target, float duration)
        {
            if (cardGroup == null) yield break;
            float start = cardGroup.alpha;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                cardGroup.alpha = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t / duration));
                yield return null;
            }
            cardGroup.alpha = target;
        }

        private IEnumerator ToastRoutine(float seconds)
        {
            toastGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(seconds);
            float t = 0f;
            while (t < 0.8f)
            {
                t += Time.unscaledDeltaTime;
                toastGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.8f);
                yield return null;
            }
            toastGroup.alpha = 0f;
            toastRoutine = null;
        }

        // ====================================================================
        // DATOS POR DEFECTO: 8 NOCHES
        // ====================================================================
        private static NightDefinition Night(
            string roman, string title, string clock, string subtitle, string objective, int quota,
            float fog, Color fogColor, Color sky, Color moon, float moonIntensity, float lampScale,
            params string[] doorsUnlocked)
        {
            return new NightDefinition
            {
                roman = roman,
                title = title,
                clockTime = clock,
                subtitle = subtitle,
                objective = objective,
                evidenceQuota = quota,
                fogDensity = fog,
                fogColor = fogColor,
                ambientSky = sky,
                moonColor = moon,
                moonIntensity = moonIntensity,
                lampScale = lampScale,
                doorsUnlockedThisNight = doorsUnlocked
            };
        }

        public static List<NightDefinition> BuildDefaultNights()
        {
            return new List<NightDefinition>
            {
                Night("I", "LA LLEGADA", "20:15",
                    "Recepción, Administración y Vestuarios.\nEntiende qué ocurrió la noche del estreno.",
                    "Reúne evidencias en la planta baja del edificio principal.", 6,
                    0.0070f, new Color(0.06f, 0.08f, 0.13f), new Color(0.09f, 0.12f, 0.18f), new Color(0.68f, 0.80f, 0.98f), 0.85f, 1.00f),

                Night("II", "LAS AULAS DE ENSAYO", "21:40",
                    "Las aulas de ballet, la Enfermería y la Sala Conmemorativa\nrevelan cómo se recordó —y cómo se olvidó— a las alumnas.",
                    "Investiga las aulas, la Enfermería y el Torreón del Ala Suroeste.", 8,
                    0.0075f, new Color(0.06f, 0.08f, 0.13f), new Color(0.085f, 0.115f, 0.17f), new Color(0.66f, 0.79f, 0.97f), 0.80f, 0.95f,
                    "Door_Rehearsal_1", "Door_Rehearsal_2", "Door_Portraits_To_MainStudio", "Door_Portraits_To_Shortcut",
                    "Door_Reception_To_SouthWestWing"),

                Night("III", "EL INTERNADO", "23:05",
                    "El Ala Occidental se abre: dormitorio, comedor, taller\ny la estancia privada de Isadora Vale.",
                    "Descubre la vida —y la vigilancia— dentro del internado.", 8,
                    0.0082f, new Color(0.06f, 0.09f, 0.12f), new Color(0.08f, 0.12f, 0.16f), new Color(0.64f, 0.80f, 0.94f), 0.75f, 0.90f,
                    "Door_Office_To_IsadoraSuite", "Door_Portraits_To_PatronageGallery", "Door_Infirmary_To_IsadoraSpyGallery"),

                Night("IV", "ARCHIVO Y PARQUE INTERIOR", "00:30",
                    "La Biblioteca Musical, el Jardín de Invierno\ny el Sótano de Mantenimiento.",
                    "Sigue el rastro de la música y del agua bajo la academia.", 8,
                    0.0090f, new Color(0.07f, 0.09f, 0.12f), new Color(0.075f, 0.10f, 0.15f), new Color(0.62f, 0.78f, 0.95f), 0.70f, 0.84f,
                    "Door_Music_Archive", "Door_Service_Corridor_Hall", "Door_Service_To_MusicArchive",
                    "Door_InstrumentStorage_To_Vanity", "Door_InstrumentStorage_To_Lockers", "Door_PianoRoom_To_DressingHall",
                    "Door_Patio_To_ServiceAlley", "Door_MaintenanceCorridor_To_WinterGarden", "Door_Washroom_To_MaintenanceBasement"),

                Night("V", "LO ALTO Y LO PROFUNDO", "01:50",
                    "La Planta Alta con la Torre del Reloj\ny la red subterránea de Calderas, Lavandería y Cripta.",
                    "Recorre los dos extremos verticales de la academia.", 8,
                    0.0100f, new Color(0.07f, 0.08f, 0.11f), new Color(0.07f, 0.09f, 0.13f), new Color(0.62f, 0.75f, 0.94f), 0.62f, 0.78f,
                    "Door_Tower_To_RooftopTerrace", "Door_Basement_To_BoilerAndCrypt"),

                Night("VI", "BAMBALINAS", "03:10",
                    "Camerinos, backstage y Sala de Poleas:\nla maquinaria detrás de la función.",
                    "Reconstruye lo que ocurría entre bastidores.", 8,
                    0.0110f, new Color(0.08f, 0.07f, 0.10f), new Color(0.08f, 0.08f, 0.13f), new Color(0.70f, 0.72f, 0.95f), 0.55f, 0.72f,
                    "Door_Service_To_TheatreTech", "Door_Library_To_TechService", "Door_InstrumentStorage_To_Backstage",
                    "Door_Backstage_To_DressingHall", "Door_Backstage_To_PulleyRoom", "Door_Backstage_Exterior_Service",
                    "Door_PulleyRoom_To_Workshop"),

                Night("VII", "EL LAGO DE LAS SOMBRAS", "04:30",
                    "El Gran Teatro, el Foso y la Cámara Secreta\ntras el Escenario.",
                    "Llega al escenario y a la verdad oculta bajo sus tablas.", 6,
                    0.0120f, new Color(0.09f, 0.06f, 0.09f), new Color(0.09f, 0.07f, 0.12f), new Color(0.75f, 0.72f, 0.95f), 0.48f, 0.66f,
                    "Door_PulleyRoom_To_StageWing", "Door_ClockTower_To_TheatreCatwalk", "Door_Understage_To_SecretVaultB00"),

                Night("VIII", "ÚLTIMO ACTO", "05:45",
                    "Los exteriores traseros: cochera, pabellón del guarda\ny el estanque. Falta muy poco para el amanecer.",
                    "Cierra el último capítulo de Elena Vesper.", 4,
                    0.0135f, new Color(0.10f, 0.06f, 0.07f), new Color(0.10f, 0.07f, 0.09f), new Color(0.85f, 0.70f, 0.80f), 0.40f, 0.60f,
                    "Door_Workshop_To_CarriageCourtyard")
            };
        }
    }
}
