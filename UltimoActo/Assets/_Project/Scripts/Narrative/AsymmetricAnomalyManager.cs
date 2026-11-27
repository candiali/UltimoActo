using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UltimoActo.Core;
using UltimoActo.Network;
using UltimoActo.Player;

namespace UltimoActo.Narrative
{
    /// <summary>
    /// PARTE 3 — Gestor del Sistema de Anomalías Asimétricas Basado en Datos:
    /// - Evalúa el catálogo de AnomalyEventData filtrado por la noche actual (NightManager).
    /// - Regla de sincronía: El Host autoritativo decide cuándo se dispara una anomalía que afecta al progreso
    ///   o a un hito compartido, mientras que los efectos visuales, sonoros y de observación se adaptan a la
    ///   época que percibe cada jugador (Julián Vesper en 1929 vs. Valeria Bell en las Ruinas).
    /// - En Modo Solo, las anomalías por época se muestran según la época activa al alternar con [Q].
    /// - Cada anomalía comienza con un breve silencio/atenuación previa y una señal sutil (pulso en la linterna
    ///   de dínamo y bajada suave de lámparas cercanas), sin sustos constantes ni monstruo perseguidor.
    /// - Registra automáticamente en el Diario Compartido ([J]/[TAB]) lo que vio cada jugador
    ///   ("Julián Vesper [1929] vio…" / "Valeria Bell [Ruinas] vio…") para empujar la comunicación por voz.
    /// </summary>
    public class AsymmetricAnomalyManager : MonoBehaviour
    {
        public static AsymmetricAnomalyManager Instance { get; private set; }

        /// <summary>
        /// Evento estático para que SharedWorldStateSync replique el disparo de una anomalía desde el Host.
        /// </summary>
        public static event Action<string> OnAnomalyTriggeredForNetwork;

        /// <summary>
        /// Gancho público de audio: (audioHookId, worldPosition, era). Permite conectar clips personalizados
        /// cuando estén disponibles; mientras tanto reproduce audio procedural atmosférico.
        /// </summary>
        public event Action<string, Vector3, EraVisual> OnAudioHookRequested;

        [SerializeField] private List<AnomalyEventData> registeredAnomalies = new List<AnomalyEventData>();

        private readonly Dictionary<string, AnomalyEventData> anomalyById = new Dictionary<string, AnomalyEventData>();
        private readonly Dictionary<string, float> lastTriggerTime = new Dictionary<string, float>();
        private readonly HashSet<string> triggeredOnceThisSession = new HashSet<string>();

        // Elementos visuales dinámicos instanciados por época para cada anomalía
        private readonly Dictionary<string, GameObject> visualProp1929ById = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, GameObject> visualPropRuinsById = new Dictionary<string, GameObject>();

        private float timeInDarknessCounter;
        private float nextControlledRandomCheck;

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
            if (FindFirstObjectByType<AsymmetricAnomalyManager>() != null) return;
            if (FindFirstObjectByType<PlayerMovement>() == null) return;

            GameObject go = new GameObject("AsymmetricAnomalyManager");
            go.AddComponent<AsymmetricAnomalyManager>();
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
            if (NightManager.Instance != null)
            {
                NightManager.Instance.OnEvidenceIdRegistered += OnMilestoneOrEvidenceRegistered;
            }
        }

        private void OnDestroy()
        {
            if (NightManager.Instance != null)
            {
                NightManager.Instance.OnEvidenceIdRegistered -= OnMilestoneOrEvidenceRegistered;
            }
            if (Instance == this) Instance = null;
        }

        public void RegisterAnomalyEvent(
            AnomalyEventData data,
            GameObject optionalProp1929 = null,
            GameObject optionalPropRuins = null)
        {
            if (data == null || string.IsNullOrEmpty(data.anomalyId)) return;

            if (!anomalyById.ContainsKey(data.anomalyId))
            {
                registeredAnomalies.Add(data);
            }
            anomalyById[data.anomalyId] = data;

            if (optionalProp1929 != null)
            {
                visualProp1929ById[data.anomalyId] = optionalProp1929;
            }
            if (optionalPropRuins != null)
            {
                visualPropRuinsById[data.anomalyId] = optionalPropRuins;
            }
        }

        private void OnMilestoneOrEvidenceRegistered(string milestoneId)
        {
            if (string.IsNullOrEmpty(milestoneId)) return;

            for (int i = 0; i < registeredAnomalies.Count; i++)
            {
                AnomalyEventData a = registeredAnomalies[i];
                if (a == null || a.triggerType != AnomalyTriggerType.PuzzleMilestone) continue;
                if (string.Equals(a.requiredMilestoneId, milestoneId, StringComparison.OrdinalIgnoreCase) ||
                    milestoneId.IndexOf(a.requiredMilestoneId, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    TryTriggerAnomaly(a.anomalyId, true);
                }
            }
        }

        private void Update()
        {
            Transform playerT = GetLocalPlayerTransform();
            if (playerT == null) return;

            int currentNight = NightManager.Instance != null ? NightManager.Instance.CurrentNightIndex : 0;

            // Contador de tiempo en la oscuridad (linterna apagada)
            bool flashLit = FlashlightController.Instance != null &&
                            FlashlightController.Instance.HasFlashlight &&
                            FlashlightController.Instance.IsOn &&
                            FlashlightController.Instance.Battery > 0f;
            if (!flashLit)
            {
                timeInDarknessCounter += Time.deltaTime;
            }
            else
            {
                timeInDarknessCounter = 0f;
            }

            Vector3 pPos = playerT.position;
            float now = Time.time;

            for (int i = 0; i < registeredAnomalies.Count; i++)
            {
                AnomalyEventData a = registeredAnomalies[i];
                if (a == null) continue;

                // Permitir las anomalías de la noche actual (o de noches ya desbloqueadas)
                if (a.nightIndex > currentNight) continue;

                if (a.triggerOncePerNight && triggeredOnceThisSession.Contains(a.anomalyId))
                {
                    continue;
                }

                if (lastTriggerTime.TryGetValue(a.anomalyId, out float lastT) && (now - lastT) < a.cooldownSeconds)
                {
                    continue;
                }

                float dist = Vector3.Distance(pPos, a.worldPosition);

                switch (a.triggerType)
                {
                    case AnomalyTriggerType.EnterZone:
                    case AnomalyTriggerType.ApproachObject:
                        if (dist <= a.triggerRadius && IsAnomalyVisibleInLocalEra(a))
                        {
                            TryTriggerAnomaly(a.anomalyId, true);
                        }
                        break;

                    case AnomalyTriggerType.TimeInDarkness:
                        if (timeInDarknessCounter >= 12f && dist <= a.triggerRadius * 2.5f && IsAnomalyVisibleInLocalEra(a))
                        {
                            timeInDarknessCounter = 0f;
                            TryTriggerAnomaly(a.anomalyId, true);
                        }
                        break;

                    case AnomalyTriggerType.ControlledRandom:
                        if (now >= nextControlledRandomCheck && dist <= a.triggerRadius * 1.8f && IsAnomalyVisibleInLocalEra(a))
                        {
                            nextControlledRandomCheck = now + 22f;
                            if (UnityEngine.Random.value < 0.45f)
                            {
                                TryTriggerAnomaly(a.anomalyId, true);
                            }
                        }
                        break;
                }
            }
        }

        private static Transform GetLocalPlayerTransform()
        {
            if (NetworkCoopPlayer.LocalPlayer != null) return NetworkCoopPlayer.LocalPlayer.transform;
            PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
            return pm != null ? pm.transform : null;
        }

        public bool IsAnomalyVisibleInLocalEra(AnomalyEventData data)
        {
            if (data == null) return false;
            if (data.visibility == AnomalyAsymmetricVisibility.Both ||
                data.visibility == AnomalyAsymmetricVisibility.BothDifferent)
            {
                return true;
            }

            EraVisual localEra = EraVisualManager.Instance != null ? EraVisualManager.Instance.CurrentEra : EraVisual.Glory1929;
            if (data.visibility == AnomalyAsymmetricVisibility.Only1929) return localEra == EraVisual.Glory1929;
            if (data.visibility == AnomalyAsymmetricVisibility.OnlyRuins) return localEra == EraVisual.Ruins;
            return true;
        }

        public void TryTriggerAnomaly(string anomalyId, bool broadcastIfShared)
        {
            if (string.IsNullOrEmpty(anomalyId)) return;
            if (!anomalyById.TryGetValue(anomalyId, out AnomalyEventData data) || data == null) return;

            float now = Time.time;
            if (lastTriggerTime.TryGetValue(anomalyId, out float lastT) && (now - lastT) < data.cooldownSeconds)
            {
                return;
            }

            lastTriggerTime[anomalyId] = now;
            if (data.triggerOncePerNight)
            {
                triggeredOnceThisSession.Add(anomalyId);
            }

            if (broadcastIfShared && (data.affectsSharedProgress ||
                data.visibility == AnomalyAsymmetricVisibility.Both ||
                data.visibility == AnomalyAsymmetricVisibility.BothDifferent))
            {
                OnAnomalyTriggeredForNetwork?.Invoke(anomalyId);
            }

            StartCoroutine(ExecuteAnomalySequenceRoutine(data));
        }

        public void ApplyAnomalyTriggerFromNetwork(string anomalyId)
        {
            if (string.IsNullOrEmpty(anomalyId)) return;
            if (!anomalyById.TryGetValue(anomalyId, out AnomalyEventData data) || data == null) return;

            float now = Time.time;
            if (lastTriggerTime.TryGetValue(anomalyId, out float lastT) && (now - lastT) < 3.0f)
            {
                return;
            }

            lastTriggerTime[anomalyId] = now;
            if (data.triggerOncePerNight)
            {
                triggeredOnceThisSession.Add(anomalyId);
            }

            StartCoroutine(ExecuteAnomalySequenceRoutine(data));
        }

        private IEnumerator ExecuteAnomalySequenceRoutine(AnomalyEventData data)
        {
            if (!IsAnomalyVisibleInLocalEra(data))
            {
                yield break;
            }

            EraVisual localEra = EraVisualManager.Instance != null ? EraVisualManager.Instance.CurrentEra : EraVisual.Glory1929;
            bool is1929 = localEra == EraVisual.Glory1929;

            // 1. Silencio previo y señal sutil de inicio (atenuación de lámparas cercanas + pulso en la linterna)
            List<KeyValuePair<Light, float>> dimmedLights = null;
            if (data.dimNearbyRoomLights)
            {
                dimmedLights = DimNearbyPointLights(data.worldPosition, 9.5f, 0.35f);
            }

            if (data.pulsePlayerFlashlight && FlashlightController.Instance != null)
            {
                Transform pT = GetLocalPlayerTransform();
                if (pT != null && Vector3.Distance(pT.position, data.worldPosition) <= data.triggerRadius * 2.4f)
                {
                    FlashlightController.Instance.TriggerBriefAnomalyFlicker(1.35f);
                }
            }

            if (data.silencePrefaceSeconds > 0.05f)
            {
                yield return new WaitForSeconds(data.silencePrefaceSeconds);
            }

            // 2. Activar manifestación visual asimétrica y sonido de época
            GameObject activeProp = is1929
                ? (visualProp1929ById.TryGetValue(data.anomalyId, out GameObject p19) ? p19 : null)
                : (visualPropRuinsById.TryGetValue(data.anomalyId, out GameObject pRu) ? pRu : null);

            bool restoreActiveFalseAfter = false;
            if (activeProp != null)
            {
                if (data.catalogPattern == AnomalyCatalogPattern.SingleEraFigure ||
                    data.catalogPattern == AnomalyCatalogPattern.AutonomousReflection ||
                    data.catalogPattern == AnomalyCatalogPattern.CrossingBallerinaShadow)
                {
                    activeProp.SetActive(true);
                    restoreActiveFalseAfter = true;
                }
                else if (data.catalogPattern == AnomalyCatalogPattern.ChangingPhotographOnReturn ||
                         data.catalogPattern == AnomalyCatalogPattern.CrossEraDisplacedObject)
                {
                    activeProp.SetActive(true);
                }
            }

            string hookId = is1929 ? data.audioHookId1929 : data.audioHookIdRuins;
            OnAudioHookRequested?.Invoke(hookId, data.worldPosition, localEra);
            PlayProceduralAnomalyCue(data, is1929);

            // 3. Mostrar aviso sutil en pantalla y archivar observación en el Diario Compartido [J]
            string toastMsg = is1929 ? data.toastObservation1929 : data.toastObservationRuins;
            if (!string.IsNullOrEmpty(toastMsg) && NightManager.Instance != null)
            {
                string eraPrefix = is1929 ? "[Anomalía 1929]" : "[Anomalía Ruinas]";
                NightManager.Instance.ShowToast($"{eraPrefix} {toastMsg}", 4.5f);
            }

            RecordAsymmetricObservationInJournal(data, is1929);

            // 4. Duración de la manifestación breve
            float dur = Mathf.Max(1.0f, data.durationSeconds);
            float elapsed = 0f;
            Vector3 initialPropPos = activeProp != null ? activeProp.transform.localPosition : Vector3.zero;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                if (activeProp != null && data.catalogPattern == AnomalyCatalogPattern.CrossingBallerinaShadow)
                {
                    float t = Mathf.Clamp01(elapsed / dur);
                    activeProp.transform.localPosition = initialPropPos + new Vector3(Mathf.Lerp(-1.2f, 1.2f, t), 0f, 0f);
                }
                yield return null;
            }

            if (activeProp != null && restoreActiveFalseAfter)
            {
                activeProp.transform.localPosition = initialPropPos;
                activeProp.SetActive(false);
            }

            // 5. Restaurar intensidad de las lámparas cercanas
            if (dimmedLights != null)
            {
                for (int i = 0; i < dimmedLights.Count; i++)
                {
                    if (dimmedLights[i].Key != null)
                    {
                        dimmedLights[i].Key.intensity = dimmedLights[i].Value;
                    }
                }
            }
        }

        private static void RecordAsymmetricObservationInJournal(AnomalyEventData data, bool seenIn1929)
        {
            if (SharedEvidenceJournalManager.Instance == null || data == null) return;

            string protagonistName = seenIn1929 ? "Julián Vesper (1929)" : "Valeria Bell (Ruinas)";
            string eraSuffix = seenIn1929 ? "1929" : "ruins";
            string entryId = $"anom_obs_{data.anomalyId}_{eraSuffix}";

            string obsDetail = seenIn1929 ? data.journalDescription1929 : data.journalDescriptionRuins;
            string oppositeHint = seenIn1929
                ? "Comunica por voz a Valeria Bell (Ruinas) lo que acabas de presenciar en este punto para compararlo con el estado actual de la sala."
                : "Comunica por voz a Julián Vesper (1929) lo que acabas de presenciar en este punto para compararlo con la noche del estreno.";

            string fullBody =
                $"REGISTRO DE ANOMALÍA ASIMÉTRICA — {data.zoneName.ToUpperInvariant()}\n\n" +
                $"• **Testigo:** {protagonistName} {obsDetail}\n\n" +
                $"• **Vínculo con la Investigación:** Refuerza la evidencia «{data.linkedDocumentOrRevelationId}».\n\n" +
                $"• **Nota Cooperativa:** {oppositeHint}";

            EraVisibilityMode visRule = seenIn1929 ? EraVisibilityMode.Only1929 : EraVisibilityMode.OnlyRuins;
            if (data.visibility == AnomalyAsymmetricVisibility.Both)
            {
                visRule = EraVisibilityMode.Both;
            }

            string titleWithEra = $"{data.journalEntryTitle} ({(seenIn1929 ? "Visto en 1929" : "Visto en Ruinas")})";

            SharedEvidenceJournalManager.Instance.RecordClueOrDocument(
                entryId: entryId,
                title: titleWithEra,
                subtitle: $"Anomalía en {data.zoneName} — Testigo: {protagonistName}",
                bodyContent: fullBody,
                category: JournalTabCategory.PuzzleClues,
                visibilityRule: visRule);
        }

        private static List<KeyValuePair<Light, float>> DimNearbyPointLights(Vector3 center, float radius, float factor)
        {
            List<KeyValuePair<Light, float>> list = new List<KeyValuePair<Light, float>>();
            Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < allLights.Length; i++)
            {
                Light l = allLights[i];
                if (l == null || l.type != LightType.Point || !l.enabled) continue;
                if (l.name.Contains("Flashlight") || l.name.Contains("RemotePlayer")) continue;

                if (Vector3.Distance(l.transform.position, center) <= radius)
                {
                    list.Add(new KeyValuePair<Light, float>(l, l.intensity));
                    l.intensity *= factor;
                }
            }
            return list;
        }

        private static void PlayProceduralAnomalyCue(AnomalyEventData data, bool is1929)
        {
            float baseFreq = is1929 ? data.synthesizedToneHz1929 : data.synthesizedToneHzRuins;
            float dur = Mathf.Clamp(data.durationSeconds * 0.65f, 1.4f, 3.2f);
            int sampleRate = 22050;
            int sampleCount = Mathf.RoundToInt(sampleRate * dur);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float timeSec = (float)i / sampleRate;
                // Silencio inicial suave seguido de armónico misterioso
                float env = Mathf.Sin(t * Mathf.PI) * Mathf.Exp(-1.6f * t);
                float detune = is1929 ? 1.003f : 0.942f;
                float w1 = Mathf.Sin(2f * Mathf.PI * baseFreq * timeSec);
                float w2 = Mathf.Sin(2f * Mathf.PI * (baseFreq * 1.498f * detune) * timeSec) * 0.45f;
                float whisper = (Mathf.PerlinNoise(timeSec * 85f, is1929 ? 1.2f : 3.7f) * 2f - 1f) * 0.12f;
                samples[i] = (w1 * 0.45f + w2 + whisper) * env * 0.36f;
            }

            AudioClip clip = AudioClip.Create($"AnomCue_{data.anomalyId}", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            AudioSource.PlayClipAtPoint(clip, data.worldPosition, 0.55f);
        }
    }
}
