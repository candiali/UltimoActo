using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UltimoActo.Core
{
    /// <summary>
    /// Singleton que gestiona las dos épocas visuales de la Academia Vesper:
    /// - Glory1929 (1929: palacio mágico en marfil #F3E9DC, dorado #C9A24A, vino #6B1E2E y rosa-lavanda #C9A6C9,
    ///   sin saturación naranja ni sombras negras).
    /// - Ruins (Actualidad: película de terror en gris pardo #3A3532, musgo #4A5240 y luz lunar fría #8FA0B8,
    ///   con visibilidad total de la arquitectura destruida, grietas, hiedra y escombros).
    /// </summary>
    public class EraVisualManager : MonoBehaviour
    {
        public const string LayerName1929 = "Era1929";
        public const string LayerNameRuins = "EraRuins";
        public const int FallbackLayer1929 = 6;
        public const int FallbackLayerRuins = 7;

        public static EraVisualManager Instance { get; private set; }

        [Header("Estado de Época (Asignable por Jugador / Red)")]
        [SerializeField] private EraVisual era = EraVisual.Glory1929;

        [Header("Volúmenes Post-Procesado URP por Época")]
        [SerializeField] private Volume volume1929;
        [SerializeField] private Volume volumeRuins;

        public EraVisual CurrentEra => era;
        public event Action<EraVisual> OnEraChanged;

        private static readonly List<EraSurfaceAdapter> RegisteredSurfaces = new List<EraSurfaceAdapter>();
        private static readonly List<EraCameraBinder> RegisteredCameras = new List<EraCameraBinder>();
        private readonly List<Light> lights1929 = new List<Light>();
        private readonly List<Light> lightsRuins = new List<Light>();
        private readonly List<Light> baseCommonLights = new List<Light>();
        private readonly List<ReflectionProbe> probes = new List<ReflectionProbe>();

        private Light directionalSunMoon;
        private CanvasGroup eraBannerGroup;
        private TextMeshProUGUI eraBannerTitle;
        private TextMeshProUGUI eraBannerSubtitle;
        private float bannerTimer;

        // Paleta atmosférica 1929 (Fiel a la fila superior de la imagen de referencia: Marfil luminoso y Rosa-Lavanda suave, SIN naranja saturado)
        private static readonly Color Fog1929 = new Color(0.56f, 0.48f, 0.54f, 1f);        // Bruma crepuscular rosa-lavanda suave
        private static readonly Color AmbientSky1929 = new Color(0.78f, 0.74f, 0.76f, 1f); // Marfil perlado luminoso #F3E9DC
        private static readonly Color AmbientEq1929 = new Color(0.64f, 0.58f, 0.62f, 1f);  // Rosa-lavanda suave #C9A6C9
        private static readonly Color AmbientGnd1929 = new Color(0.46f, 0.42f, 0.44f, 1f); // Sombras claras y legibles (nunca negras)
        private static readonly Color SunColor1929 = new Color(0.98f, 0.94f, 0.90f, 1f);   // Luz natural marfil-champán (nunca naranja)

        // Paleta atmosférica Ruinas (Fiel a la fila inferior de la imagen de referencia: Luz lunar fría #8FA0B8 y musgo #4A5240 totalmente visibles)
        private static readonly Color FogRuins = new Color(0.28f, 0.33f, 0.39f, 1f);       // Niebla fría plateada-azulada visible
        private static readonly Color AmbientSkyRuins = new Color(0.48f, 0.55f, 0.64f, 1f);// Cielo nublado frío #8FA0B8
        private static readonly Color AmbientEqRuins = new Color(0.38f, 0.42f, 0.38f, 1f); // Musgo desaturado #4A5240
        private static readonly Color AmbientGndRuins = new Color(0.26f, 0.28f, 0.30f, 1f);// Sombras frías visibles al 100%
        private static readonly Color MoonColorRuins = new Color(0.72f, 0.80f, 0.92f, 1f); // Luz lunar fría clara #8FA0B8

        public static int GetEra1929Layer()
        {
            int l = LayerMask.NameToLayer(LayerName1929);
            return l >= 0 ? l : FallbackLayer1929;
        }

        public static int GetEraRuinsLayer()
        {
            int l = LayerMask.NameToLayer(LayerNameRuins);
            return l >= 0 ? l : FallbackLayerRuins;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrap()
        {
            if (Instance != null) return;
            if (FindFirstObjectByType<EraVisualManager>() != null) return;
            if (FindFirstObjectByType<UltimoActo.Player.PlayerMovement>() == null) return;

            GameObject host = new GameObject("EraVisualManager");
            host.AddComponent<EraVisualManager>();
        }

        public static void RegisterSurface(EraSurfaceAdapter surface)
        {
            if (surface != null && !RegisteredSurfaces.Contains(surface))
            {
                RegisteredSurfaces.Add(surface);
            }
        }

        public static void UnregisterSurface(EraSurfaceAdapter surface)
        {
            RegisteredSurfaces.Remove(surface);
        }

        public static void RegisterCamera(EraCameraBinder binder)
        {
            if (binder != null && !RegisteredCameras.Contains(binder))
            {
                RegisteredCameras.Add(binder);
            }
        }

        public static void UnregisterCamera(EraCameraBinder binder)
        {
            RegisteredCameras.Remove(binder);
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

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            RenderSettings.skybox = null;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.reflectionIntensity = 0.30f;

            // Desactivar cualquier Volume antiguo de la escena base para que no oscurezca ni interfiera
            DisableConflictingBaseVolumes();

            EnsureCameraBinders();
            EnsurePostProcessingVolumes();
            CacheEraLightsAndProbes();
            BuildEraFeedbackHud();

            if (NightManager.Instance != null)
            {
                NightManager.Instance.OnNightStarted += OnNightStartedHandler;
            }

            SetEra(era, false);
        }

        private void DisableConflictingBaseVolumes()
        {
            GameObject baseVol = GameObject.Find("Global_PostProcess_Volume");
            if (baseVol != null)
            {
                Volume v = baseVol.GetComponent<Volume>();
                if (v != null) v.enabled = false;
            }
        }

        private void OnNightStartedHandler(NightDefinition def, int nightIdx)
        {
            ApplyAtmosphereAndLightsForEra(era);
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            bool f1Pressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
            {
                f1Pressed = true;
            }
#else
            if (Input.GetKeyDown(KeyCode.F1))
            {
                f1Pressed = true;
            }
#endif
            if (f1Pressed)
            {
                ToggleEra();
            }
#endif

            if (eraBannerGroup != null && eraBannerGroup.alpha > 0f)
            {
                if (bannerTimer > 0f)
                {
                    bannerTimer -= Time.unscaledDeltaTime;
                }
                else
                {
                    eraBannerGroup.alpha = Mathf.MoveTowards(eraBannerGroup.alpha, 0f, Time.unscaledDeltaTime * 1.8f);
                }
            }
        }

        private void LateUpdate()
        {
            ApplyAtmosphereValuesOnly(era);
        }

        public void ToggleEra()
        {
            SetEra(era == EraVisual.Glory1929 ? EraVisual.Ruins : EraVisual.Glory1929, true);
        }

        public void SetEra(EraVisual targetEra)
        {
            SetEra(targetEra, true);
        }

        public void SetEra(EraVisual targetEra, bool showBanner)
        {
            era = targetEra;

            RenderSettings.skybox = null;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.reflectionIntensity = 0.30f;

            DisableConflictingBaseVolumes();
            EnsureCameraBinders();
            foreach (EraCameraBinder binder in RegisteredCameras)
            {
                if (binder != null) binder.RefreshCameraMask();
            }

            for (int i = RegisteredSurfaces.Count - 1; i >= 0; i--)
            {
                if (RegisteredSurfaces[i] != null)
                {
                    RegisteredSurfaces[i].ApplyEra(era);
                }
            }

            ApplyAtmosphereAndLightsForEra(era);
            RefreshReflectionProbes();

#if UNITY_EDITOR
            int l1929 = GetEra1929Layer();
            int lRuins = GetEraRuinsLayer();
            if (era == EraVisual.Glory1929)
            {
                UnityEditor.Tools.visibleLayers |= (1 << l1929);
                UnityEditor.Tools.visibleLayers &= ~(1 << lRuins);
            }
            else
            {
                UnityEditor.Tools.visibleLayers |= (1 << lRuins);
                UnityEditor.Tools.visibleLayers &= ~(1 << l1929);
            }
            UnityEditor.SceneView.RepaintAll();
#endif

            if (showBanner)
            {
                ShowEraBanner(era);
            }

            OnEraChanged?.Invoke(era);
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera renderingCamera)
        {
            if (renderingCamera == null) return;

            EraCameraBinder binder = renderingCamera.GetComponent<EraCameraBinder>();
            EraVisual camEra = binder != null ? binder.AssignedEra : era;

            ApplyAtmosphereValuesOnly(camEra);
            if (binder != null && !binder.FollowLocalManager)
            {
                for (int i = 0; i < RegisteredSurfaces.Count; i++)
                {
                    if (RegisteredSurfaces[i] != null) RegisteredSurfaces[i].ApplyEra(camEra);
                }
            }
        }

        private void EnsureCameraBinders()
        {
            Camera[] allCams = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (Camera c in allCams)
            {
                if (c == null) continue;
                if (c.GetComponent<EraCameraBinder>() == null)
                {
                    c.gameObject.AddComponent<EraCameraBinder>();
                }
            }
        }

        public void CacheEraLightsAndProbes()
        {
            lights1929.Clear();
            lightsRuins.Clear();
            baseCommonLights.Clear();
            probes.Clear();

            int l1929 = GetEra1929Layer();
            int lRuins = GetEraRuinsLayer();

            GameObject moonObj = GameObject.Find("Moonlight_Cold");
            if (moonObj != null)
            {
                directionalSunMoon = moonObj.GetComponent<Light>();
                // Orientar la luz direccional para que ilumine frontalmente la fachada y el pórtico (sin dejarlos a contraluz oscuro)
                moonObj.transform.rotation = Quaternion.Euler(38f, 152f, 0f);
            }

            Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light l in allLights)
            {
                if (l == null) continue;
                if (l.type != LightType.Directional && l.shadows != LightShadows.None)
                {
                    l.shadows = LightShadows.None;
                }

                if (l.gameObject.layer == l1929)
                {
                    lights1929.Add(l);
                }
                else if (l.gameObject.layer == lRuins)
                {
                    lightsRuins.Add(l);
                }
                else if (l.type == LightType.Point && !l.gameObject.name.Contains("Flashlight"))
                {
                    baseCommonLights.Add(l);
                }
            }

            probes.AddRange(FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None));
        }

        private void ApplyAtmosphereAndLightsForEra(EraVisual activeEra)
        {
            if (lights1929.Count == 0 && lightsRuins.Count == 0)
            {
                CacheEraLightsAndProbes();
            }

            bool is1929 = activeEra == EraVisual.Glory1929;

            for (int i = 0; i < lights1929.Count; i++)
            {
                if (lights1929[i] != null) lights1929[i].enabled = is1929;
            }
            for (int i = 0; i < lightsRuins.Count; i++)
            {
                if (lightsRuins[i] != null) lightsRuins[i].enabled = !is1929;
            }

            // Mantener todas las luces base encendidas en ambas épocas, cambiando su color entre marfil-champán suave (1929) y luz lunar fría (Ruinas)
            for (int i = 0; i < baseCommonLights.Count; i++)
            {
                Light cl = baseCommonLights[i];
                if (cl == null) continue;
                cl.enabled = true;
                cl.color = is1929
                    ? new Color(0.98f, 0.93f, 0.85f)  // Blanco marfil cálido natural (NUNCA naranja saturado)
                    : new Color(0.72f, 0.82f, 0.93f); // Luz lunar pálida espectral (100% visible)
            }

            if (volume1929 != null) volume1929.weight = is1929 ? 1f : 0f;
            if (volumeRuins != null) volumeRuins.weight = is1929 ? 0f : 1f;

            ApplyAtmosphereValuesOnly(activeEra);
        }

        private void ApplyAtmosphereValuesOnly(EraVisual activeEra)
        {
            bool is1929 = activeEra == EraVisual.Glory1929;

            float nightFactor = 0f;
            if (NightManager.Instance != null && NightManager.Instance.Current != null)
            {
                nightFactor = Mathf.Clamp01(NightManager.Instance.CurrentNightIndex / 7f);
            }

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.ambientMode = AmbientMode.Trilight;

            if (is1929)
            {
                // 1929: Luminoso, marfil perlado y rosa-lavanda suave (como en la fila superior de la referencia)
                RenderSettings.fogColor = Color.Lerp(Fog1929, Fog1929 * 0.82f, nightFactor * 0.35f);
                RenderSettings.fogDensity = 0.0020f;
                RenderSettings.ambientSkyColor = Color.Lerp(AmbientSky1929, AmbientSky1929 * 0.88f, nightFactor * 0.3f);
                RenderSettings.ambientEquatorColor = Color.Lerp(AmbientEq1929, AmbientEq1929 * 0.88f, nightFactor * 0.3f);
                RenderSettings.ambientGroundColor = AmbientGnd1929;

                if (directionalSunMoon != null)
                {
                    directionalSunMoon.color = SunColor1929;
                    directionalSunMoon.intensity = Mathf.Lerp(1.35f, 1.10f, nightFactor);
                }
            }
            else
            {
                // Ruinas: Luz lunar fría #8FA0B8 clara y niebla plateada (como en la fila inferior de la referencia, 100% visible)
                RenderSettings.fogColor = Color.Lerp(FogRuins, FogRuins * 0.85f, nightFactor * 0.35f);
                RenderSettings.fogDensity = 0.0042f;
                RenderSettings.ambientSkyColor = Color.Lerp(AmbientSkyRuins, AmbientSkyRuins * 0.85f, nightFactor * 0.3f);
                RenderSettings.ambientEquatorColor = Color.Lerp(AmbientEqRuins, AmbientEqRuins * 0.85f, nightFactor * 0.3f);
                RenderSettings.ambientGroundColor = AmbientGndRuins;

                if (directionalSunMoon != null)
                {
                    directionalSunMoon.color = MoonColorRuins;
                    directionalSunMoon.intensity = Mathf.Lerp(1.25f, 0.98f, nightFactor);
                }
            }
        }

        private void RefreshReflectionProbes()
        {
            int l1929 = GetEra1929Layer();
            int lRuins = GetEraRuinsLayer();
            int bothEraMask = (1 << l1929) | (1 << lRuins);
            int activeBit = era == EraVisual.Glory1929 ? (1 << l1929) : (1 << lRuins);

            foreach (ReflectionProbe probe in probes)
            {
                if (probe == null) continue;
                probe.cullingMask = (~bothEraMask) | activeBit;
                if (Application.isPlaying && probe.mode == ReflectionProbeMode.Realtime)
                {
                    probe.RenderProbe();
                }
            }
        }

        private void EnsurePostProcessingVolumes()
        {
            if (volume1929 == null)
            {
                GameObject v1929Obj = GameObject.Find("GlobalVolume_Era1929");
                if (v1929Obj == null)
                {
                    v1929Obj = new GameObject("GlobalVolume_Era1929");
                    v1929Obj.transform.SetParent(transform, false);
                }
                volume1929 = v1929Obj.GetComponent<Volume>();
                if (volume1929 == null) volume1929 = v1929Obj.AddComponent<Volume>();
                volume1929.isGlobal = true;
                volume1929.priority = 50f;
                volume1929.sharedProfile = BuildProfile1929();
            }

            if (volumeRuins == null)
            {
                GameObject vRuinsObj = GameObject.Find("GlobalVolume_EraRuins");
                if (vRuinsObj == null)
                {
                    vRuinsObj = new GameObject("GlobalVolume_EraRuins");
                    vRuinsObj.transform.SetParent(transform, false);
                }
                volumeRuins = vRuinsObj.GetComponent<Volume>();
                if (volumeRuins == null) volumeRuins = vRuinsObj.AddComponent<Volume>();
                volumeRuins.isGlobal = true;
                volumeRuins.priority = 50f;
                volumeRuins.sharedProfile = BuildProfileRuins();
            }
        }

        private static VolumeProfile BuildProfile1929()
        {
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "Profile_Era1929_PalaceIvory";

            Bloom bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(0.92f);
            bloom.intensity.Override(0.80f);
            bloom.scatter.Override(0.72f);
            bloom.tint.Override(new Color(0.98f, 0.92f, 0.95f)); // Brillo perlado-rosado suave

            ColorAdjustments colorAdj = profile.Add<ColorAdjustments>(true);
            colorAdj.postExposure.Override(0.38f); // Luminoso y claro como en la imagen de referencia
            colorAdj.contrast.Override(8f);
            colorAdj.colorFilter.Override(new Color(0.98f, 0.96f, 0.97f)); // Marfil limpio, sin tinte naranja
            colorAdj.saturation.Override(-6f); // Saturación contenida y elegante (cero naranja chillón)

            WhiteBalance wb = profile.Add<WhiteBalance>(true);
            wb.temperature.Override(2f); // Temperatura casi neutra para conservar el blanco marfil #F3E9DC
            wb.tint.Override(6f);        // Sutil matiz rosa-lavanda #C9A6C9

            Vignette vig = profile.Add<Vignette>(true);
            vig.color.Override(new Color(0.16f, 0.08f, 0.12f));
            vig.intensity.Override(0.24f);
            vig.smoothness.Override(0.45f);

            FilmGrain grain = profile.Add<FilmGrain>(true);
            grain.type.Override(FilmGrainLookup.Thin1);
            grain.intensity.Override(0.10f);

            return profile;
        }

        private static VolumeProfile BuildProfileRuins()
        {
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "Profile_EraRuins_ColdHorror";

            Bloom bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(0.95f);
            bloom.intensity.Override(0.55f);
            bloom.scatter.Override(0.68f);
            bloom.tint.Override(new Color(0.72f, 0.82f, 0.94f));

            ColorAdjustments colorAdj = profile.Add<ColorAdjustments>(true);
            colorAdj.postExposure.Override(0.32f); // Exposición positiva para que todas las ruinas se vean con total nitidez
            colorAdj.contrast.Override(18f);
            colorAdj.colorFilter.Override(new Color(0.88f, 0.92f, 0.98f)); // Tono frío lunar #8FA0B8
            colorAdj.saturation.Override(-34f); // Desaturado cinematográfico de terror

            WhiteBalance wb = profile.Add<WhiteBalance>(true);
            wb.temperature.Override(-14f);
            wb.tint.Override(-4f);

            Vignette vig = profile.Add<Vignette>(true);
            vig.color.Override(new Color(0.04f, 0.05f, 0.07f));
            vig.intensity.Override(0.36f);
            vig.smoothness.Override(0.52f);

            FilmGrain grain = profile.Add<FilmGrain>(true);
            grain.type.Override(FilmGrainLookup.Medium3);
            grain.intensity.Override(0.28f);

            ChromaticAberration ca = profile.Add<ChromaticAberration>(true);
            ca.intensity.Override(0.08f);

            return profile;
        }

        private void BuildEraFeedbackHud()
        {
            GameObject canvasObj = new GameObject("EraVisual_HUD_Canvas");
            canvasObj.transform.SetParent(transform, false);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 65;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject bannerObj = new GameObject("EraBanner");
            bannerObj.transform.SetParent(canvasObj.transform, false);
            RectTransform rt = bannerObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -24f);
            rt.sizeDelta = new Vector2(600f, 74f);

            Image bg = bannerObj.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.05f, 0.07f, 0.86f);
            bg.raycastTarget = false;

            eraBannerGroup = bannerObj.AddComponent<CanvasGroup>();
            eraBannerGroup.alpha = 0f;
            eraBannerGroup.blocksRaycasts = false;

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(bannerObj.transform, false);
            RectTransform titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 0.42f);
            titleRt.anchorMax = new Vector2(1f, 0.94f);
            titleRt.offsetMin = new Vector2(16f, 0f);
            titleRt.offsetMax = new Vector2(-16f, 0f);

            eraBannerTitle = titleObj.AddComponent<TextMeshProUGUI>();
            eraBannerTitle.fontSize = 22f;
            eraBannerTitle.fontStyle = FontStyles.Bold;
            eraBannerTitle.alignment = TextAlignmentOptions.Center;
            eraBannerTitle.raycastTarget = false;

            GameObject subObj = new GameObject("Subtitle");
            subObj.transform.SetParent(bannerObj.transform, false);
            RectTransform subRt = subObj.AddComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0f, 0.08f);
            subRt.anchorMax = new Vector2(1f, 0.44f);
            subRt.offsetMin = new Vector2(16f, 0f);
            subRt.offsetMax = new Vector2(-16f, 0f);

            eraBannerSubtitle = subObj.AddComponent<TextMeshProUGUI>();
            eraBannerSubtitle.fontSize = 14f;
            eraBannerSubtitle.alignment = TextAlignmentOptions.Center;
            eraBannerSubtitle.raycastTarget = false;
        }

        private void ShowEraBanner(EraVisual current)
        {
            if (eraBannerGroup == null) return;

            if (current == EraVisual.Glory1929)
            {
                eraBannerTitle.text = "ÉPOCA 1929 — PALACIO IMPERIAL VESPER";
                eraBannerTitle.color = new Color(0.95f, 0.88f, 0.72f);
                eraBannerSubtitle.text = "[F1] Cambiar a Ruinas · Esplendor marfil, oro viejo y crepúsculo";
                eraBannerSubtitle.color = new Color(0.95f, 0.91f, 0.88f, 0.88f);
            }
            else
            {
                eraBannerTitle.text = "ÉPOCA ACTUAL — RUINAS DE LA ACADEMIA";
                eraBannerTitle.color = new Color(0.68f, 0.78f, 0.90f);
                eraBannerSubtitle.text = "[F1] Cambiar a 1929 · Abandono, hiedra, grietas y luz lunar fría";
                eraBannerSubtitle.color = new Color(0.78f, 0.84f, 0.90f, 0.88f);
            }

            eraBannerGroup.alpha = 1f;
            bannerTimer = 2.6f;
        }
    }
}
