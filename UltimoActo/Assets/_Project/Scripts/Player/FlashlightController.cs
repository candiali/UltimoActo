using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UltimoActo.Core;

namespace UltimoActo.Player
{
    /// <summary>
    /// Controlador de la linterna de dínamo / latón (1924).
    /// Proporciona iluminación en primera persona, gestión de batería/dínamo y
    /// parpadeo de interferencia sobrenatural ante anomalías psicológicas.
    /// </summary>
    public class FlashlightController : MonoBehaviour
    {
        public static FlashlightController Instance { get; private set; }

        [Header("Referencias")]
        [SerializeField] private Light spotLight;
        [SerializeField] private Transform cameraTransform;

        [Header("Estado")]
        [Tooltip("Indica si el jugador ya ha recogido la linterna en el mundo")]
        [SerializeField] private bool hasFlashlight = false;
        [SerializeField] private bool isOn = false;

        [Header("Batería y Dínamo")]
        [Range(0f, 100f)]
        [SerializeField] private float battery = 100f;
        [Tooltip("Porcentaje de consumo por minuto con la luz encendida")]
        [SerializeField] private float drainPerMinute = 6.0f; // ~16 minutos de luz continua
        [Tooltip("Carga que recupera la manivela de dínamo por cada pulsación de R")]
        [SerializeField] private float dynamoRechargePerCrank = 20f;

        [Header("Óptica")]
        [SerializeField] private float baseIntensity = 1.7f;
        [SerializeField] private float baseRange = 22f;
        [SerializeField] private float baseAngle = 52f;
        [SerializeField] private Color lightColor = new Color(0.96f, 0.93f, 0.84f);

        [Header("Interferencia Sobrenatural")]
        [Tooltip("Multiplicador de parpadeo activo (1 = normal, 0..1 en anomalías)")]
        private float anomalyFlickerMultiplier = 1.0f;
        private bool isAnomalyNearby = false;
        private Coroutine flickerRoutine;

        // UI
        private CanvasGroup batteryUIGroup;
        private RectTransform batteryFill;
        private TextMeshProUGUI batteryText;
        private AudioSource audioSource;

        public bool HasFlashlight => hasFlashlight;
        public bool IsOn => isOn;
        public float Battery => battery;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (spotLight == null)
            {
                spotLight = GetComponentInChildren<Light>();
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0.0f; // 2D UI / local sound
            }

            ConfigureLightProperties();
        }

        private void Start()
        {
            if (spotLight != null)
            {
                spotLight.enabled = hasFlashlight && isOn;
            }

            BuildBatteryHUD();
            UpdateHUD();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            HandleInput();
            HandleBatteryDrain();
            CheckAnomaliesProximity();
            ApplyLighting();
            UpdateHUD();
        }

        // ====================================================================
        // INPUT Y CONTROL
        // ====================================================================
        private void HandleInput()
        {
            if (!hasFlashlight) return;

            // Tecla F: Encender / Apagar
            bool toggleRequested = false;
            bool rechargeRequested = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.fKey.wasPressedThisFrame) toggleRequested = true;
                if (Keyboard.current.rKey.wasPressedThisFrame) rechargeRequested = true;
            }
            if (Gamepad.current != null)
            {
                if (Gamepad.current.buttonNorth.wasPressedThisFrame || Gamepad.current.dpad.up.wasPressedThisFrame) toggleRequested = true;
                if (Gamepad.current.buttonWest.wasPressedThisFrame) rechargeRequested = true;
            }
#else
            if (Input.GetKeyDown(KeyCode.F)) toggleRequested = true;
            if (Input.GetKeyDown(KeyCode.R)) rechargeRequested = true;
#endif

            if (toggleRequested)
            {
                Toggle();
            }

            if (rechargeRequested)
            {
                CrankDynamo();
            }
        }

        public void UnlockFlashlight()
        {
            hasFlashlight = true;
            isOn = true;
            battery = Mathf.Max(battery, 85f);
            PlayClickSound(1.2f);
            if (NightManager.Instance != null)
            {
                NightManager.Instance.ShowToast("LINTERNA DE LATÓN OBTENIDA — Pulsa [F] para alternar, [R] para accionar la dínamo", 5.0f);
            }
        }

        public void Toggle()
        {
            if (!hasFlashlight) return;

            if (!isOn && battery <= 0.05f)
            {
                PlayClickSound(0.7f);
                if (NightManager.Instance != null)
                {
                    NightManager.Instance.ShowToast("La dinamo está descargada. Pulsa [R] para girar la manivela", 2.5f);
                }
                return;
            }

            isOn = !isOn;
            PlayClickSound(isOn ? 1.0f : 0.85f);
        }

        public void CrankDynamo()
        {
            if (!hasFlashlight) return;

            battery = Mathf.Clamp(battery + dynamoRechargePerCrank, 0f, 100f);
            PlayDynamoWhirSound();

            if (!isOn && battery > 10f)
            {
                isOn = true;
            }
        }

        // ====================================================================
        // DRENAJE Y PARPADEO
        // ====================================================================
        private void HandleBatteryDrain()
        {
            if (!hasFlashlight || !isOn) return;

            float drainThisFrame = (drainPerMinute / 60.0f) * Time.deltaTime;
            battery = Mathf.Max(0f, battery - drainThisFrame);

            if (battery <= 0f)
            {
                isOn = false;
                PlayClickSound(0.6f);
            }
        }

        private void CheckAnomaliesProximity()
        {
            // Detectar si estamos a menos de 7.5 metros de cualquier anomalía o secreto activo
            isAnomalyNearby = false;
            Vector3 myPos = transform.position;

            Collider[] hits = Physics.OverlapSphere(myPos, 7.5f);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].GetComponentInParent<UltimoActo.Narrative.AcademyExpansionAnomalyController>() != null ||
                    hits[i].GetComponentInParent<UltimoActo.Narrative.MirrorAnomalyController>() != null ||
                    hits[i].name.Contains("Anomaly") || hits[i].name.Contains("Silhouette") || hits[i].name.Contains("Secreto"))
                {
                    isAnomalyNearby = true;
                    break;
                }
            }

            if (isAnomalyNearby && flickerRoutine == null && isOn)
            {
                flickerRoutine = StartCoroutine(AnomalyFlickerRoutine());
            }
        }

        private IEnumerator AnomalyFlickerRoutine()
        {
            while (isAnomalyNearby && isOn)
            {
                // Parpadeo errático e inquietante característico del cine de terror psicológico
                anomalyFlickerMultiplier = UnityEngine.Random.Range(0.15f, 0.95f);
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.04f, 0.16f));
                anomalyFlickerMultiplier = 1.0f;
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.2f, 1.2f));
            }
            anomalyFlickerMultiplier = 1.0f;
            flickerRoutine = null;
        }

        private void ApplyLighting()
        {
            if (spotLight == null) return;

            bool shouldBeLit = hasFlashlight && isOn && battery > 0f;
            spotLight.enabled = shouldBeLit;

            if (shouldBeLit)
            {
                // La luz se vuelve más tenue e inestable al agotarse la dínamo (<20%)
                float batteryFactor = battery < 20f ? Mathf.Lerp(0.35f, 1.0f, battery / 20f) : 1.0f;
                spotLight.intensity = baseIntensity * batteryFactor * anomalyFlickerMultiplier;
            }
        }

        private void ConfigureLightProperties()
        {
            if (spotLight == null) return;
            spotLight.type = LightType.Spot;
            spotLight.color = lightColor;
            spotLight.range = baseRange;
            spotLight.spotAngle = baseAngle;
            spotLight.intensity = baseIntensity;
            spotLight.shadows = LightShadows.None; // Optimización de sombras para rendimiento óptimo
        }

        // ====================================================================
        // EFECTOS DE AUDIO PROCEDURALES (Sin dependencias externas)
        // ====================================================================
        private void PlayClickSound(float pitch = 1.0f)
        {
            if (audioSource == null) return;
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(GenerateClickClip());
        }

        private void PlayDynamoWhirSound()
        {
            if (audioSource == null) return;
            audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.15f);
            audioSource.PlayOneShot(GenerateDynamoClip());
        }

        private static AudioClip cachedClickClip;
        private static AudioClip GenerateClickClip()
        {
            if (cachedClickClip != null) return cachedClickClip;
            int samples = 2000;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                data[i] = Mathf.Sin(2f * Mathf.PI * 1800f * t) * Mathf.Exp(-t * 35f);
            }
            cachedClickClip = AudioClip.Create("FlashlightClick", samples, 1, 44100, false);
            cachedClickClip.SetData(data, 0);
            return cachedClickClip;
        }

        private static AudioClip cachedDynamoClip;
        private static AudioClip GenerateDynamoClip()
        {
            if (cachedDynamoClip != null) return cachedDynamoClip;
            int samples = 6000;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(300f, 950f, t);
                data[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) + (UnityEngine.Random.value * 0.15f)) * Mathf.Sin(Mathf.PI * t);
            }
            cachedDynamoClip = AudioClip.Create("DynamoWhir", samples, 1, 44100, false);
            cachedDynamoClip.SetData(data, 0);
            return cachedDynamoClip;
        }

        // ====================================================================
        // HUD DE BATERÍA DE LA LINTERNA
        // ====================================================================
        private void BuildBatteryHUD()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject root = new GameObject("Flashlight_Battery_HUD");
            root.transform.SetParent(canvas.transform, false);

            batteryUIGroup = root.AddComponent<CanvasGroup>();
            batteryUIGroup.alpha = 0f;
            batteryUIGroup.blocksRaycasts = false;
            batteryUIGroup.interactable = false;

            RectTransform rt = root.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.anchoredPosition = new Vector2(36f, 36f);
            rt.sizeDelta = new Vector2(180f, 42f);

            // Fondo negro translúcido
            GameObject bgObj = new GameObject("Bg");
            bgObj.transform.SetParent(root.transform, false);
            RectTransform bgRT = bgObj.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.08f, 0.08f, 0.10f, 0.75f);
            bgImg.raycastTarget = false;

            // Barra interior
            GameObject barObj = new GameObject("BarFill");
            barObj.transform.SetParent(root.transform, false);
            batteryFill = barObj.AddComponent<RectTransform>();
            batteryFill.anchorMin = new Vector2(0f, 0f);
            batteryFill.anchorMax = new Vector2(1f, 1f);
            batteryFill.pivot = new Vector2(0f, 0.5f);
            batteryFill.offsetMin = new Vector2(4f, 4f);
            batteryFill.offsetMax = new Vector2(-4f, -4f);
            Image fillImg = barObj.AddComponent<Image>();
            fillImg.color = new Color(0.92f, 0.82f, 0.45f, 0.85f);
            fillImg.raycastTarget = false;

            // Texto descriptivo
            GameObject textObj = new GameObject("BatteryText");
            textObj.transform.SetParent(root.transform, false);
            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(10f, 0f);
            textRT.offsetMax = new Vector2(-10f, 0f);
            batteryText = textObj.AddComponent<TextMeshProUGUI>();
            batteryText.fontSize = 17f;
            batteryText.fontStyle = FontStyles.Bold;
            batteryText.alignment = TextAlignmentOptions.MidlineLeft;
            batteryText.color = new Color(0.95f, 0.95f, 0.90f);
            batteryText.raycastTarget = false;
        }

        private void UpdateHUD()
        {
            if (batteryUIGroup == null) return;

            batteryUIGroup.alpha = hasFlashlight ? 1f : 0f;
            if (!hasFlashlight) return;

            if (batteryFill != null)
            {
                batteryFill.anchorMax = new Vector2(battery / 100f, 1f);
            }

            if (batteryText != null)
            {
                string status = isOn ? "ON" : "OFF";
                batteryText.text = $"LINTERNA [{status}] {Mathf.CeilToInt(battery)}%";
            }
        }
    }
}
