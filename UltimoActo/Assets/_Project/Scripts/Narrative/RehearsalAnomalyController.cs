using UnityEngine;

namespace UltimoActo.Narrative
{
    public enum RehearsalAnomalyType
    {
        MainStudioMirrorPose,
        MirrorRoomSinglePanelFigure,
        MannequinTurnAndClothBreath,
        SoloRoomSlippersAndShadow,
        ServiceCorridorDistantPiano
    }

    /// <summary>
    /// Controla las anomalías psicológicas sutiles de la Sección 4 (Aulas de Ensayo: Zonas 16 a 20).
    /// Todas las anomalías son breves, controladas y atmosféricas, sin interferir con el movimiento
    /// ni bloquear la navegación cooperativa.
    /// </summary>
    public class RehearsalAnomalyController : MonoBehaviour
    {
        [SerializeField] private RehearsalAnomalyType anomalyType;
        [SerializeField] private Transform targetTransform;
        [SerializeField] private Transform secondaryTransform;
        [SerializeField] private Transform displacedReflectionObject;
        [SerializeField] private Light anomalyLight;

        private Transform playerCam;
        private AudioSource audioSource;
        private bool hasTurnedOnce = false;
        private float cooldownTimer = 2.0f;
        private float activeTimer = 0f;
        private bool isAnomalyActive = false;
        private Vector3 baseScale = Vector3.one;
        private Vector3 displacedBaseLocalPos = Vector3.zero;

        public void Configure(RehearsalAnomalyType type, Transform primary, Transform secondary = null)
        {
            anomalyType = type;
            targetTransform = primary;
            secondaryTransform = secondary;
        }

        public void Configure(
            RehearsalAnomalyType type,
            GameObject silhouetteObj,
            Transform mannequinTarget,
            Transform clothBreathTarget,
            Transform displacedObj,
            Light pulseLight)
        {
            anomalyType = type;
            targetTransform = silhouetteObj != null ? silhouetteObj.transform : mannequinTarget;
            secondaryTransform = clothBreathTarget;
            displacedReflectionObject = displacedObj;
            anomalyLight = pulseLight;
        }

        private void Start()
        {
            if (Camera.main != null) playerCam = Camera.main.transform;
            if (secondaryTransform != null) baseScale = secondaryTransform.localScale;
            if (displacedReflectionObject != null) displacedBaseLocalPos = displacedReflectionObject.localPosition;
            if (anomalyLight != null) anomalyLight.enabled = false;

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.playOnAwake = false;

            switch (anomalyType)
            {
                case RehearsalAnomalyType.MainStudioMirrorPose:
                case RehearsalAnomalyType.MirrorRoomSinglePanelFigure:
                    if (targetTransform != null) targetTransform.gameObject.SetActive(false);
                    audioSource.minDistance = 2f;
                    audioSource.maxDistance = 14f;
                    audioSource.volume = 0.22f;
                    audioSource.clip = CreateGlassResonanceClip();
                    break;

                case RehearsalAnomalyType.SoloRoomSlippersAndShadow:
                    if (targetTransform != null) targetTransform.gameObject.SetActive(false);
                    audioSource.minDistance = 1.5f;
                    audioSource.maxDistance = 11f;
                    audioSource.volume = 0.32f;
                    audioSource.clip = CreateSlipperSlideClip();
                    break;

                case RehearsalAnomalyType.ServiceCorridorDistantPiano:
                    audioSource.minDistance = 2.0f;
                    audioSource.maxDistance = 16f;
                    audioSource.volume = 0.28f;
                    audioSource.loop = true;
                    audioSource.clip = CreateDistantPianoClip();
                    break;
            }
        }

        private void Update()
        {
            if (playerCam == null)
            {
                if (Camera.main != null) playerCam = Camera.main.transform;
                else return;
            }

            float dist = Vector3.Distance(playerCam.position, transform.position);
            Vector3 toTarget = (transform.position - playerCam.position).normalized;
            float lookDot = Vector3.Dot(playerCam.forward, toTarget);

            switch (anomalyType)
            {
                case RehearsalAnomalyType.MainStudioMirrorPose:
                    UpdateMainStudioAnomaly(dist, lookDot);
                    break;

                case RehearsalAnomalyType.MirrorRoomSinglePanelFigure:
                    UpdateMirrorRoomAnomaly(dist, lookDot);
                    break;

                case RehearsalAnomalyType.MannequinTurnAndClothBreath:
                    UpdateMannequinAnomaly(dist, lookDot);
                    break;

                case RehearsalAnomalyType.SoloRoomSlippersAndShadow:
                    UpdateSoloRoomAnomaly(dist, lookDot);
                    break;

                case RehearsalAnomalyType.ServiceCorridorDistantPiano:
                    UpdateServicePianoAnomaly(dist);
                    break;
            }
        }

        private void UpdateMainStudioAnomaly(float dist, float lookDot)
        {
            if (targetTransform == null) return;

            // Al alejarse de los espejos (entre 7m y 16m) aparece brevemente en posición de ballet;
            // al mirar directamente o acercarse, el aula vuelve a estar vacía.
            if (dist >= 7.0f && dist <= 16.0f && lookDot > 0.35f && lookDot < 0.82f)
            {
                if (!isAnomalyActive && cooldownTimer <= 0f)
                {
                    isAnomalyActive = true;
                    activeTimer = 2.2f;
                    targetTransform.gameObject.SetActive(true);
                    if (anomalyLight != null) anomalyLight.enabled = true;
                    if (audioSource != null && audioSource.clip != null) audioSource.Play();
                }
            }

            if (isAnomalyActive)
            {
                activeTimer -= Time.deltaTime;
                if (activeTimer <= 0f || dist < 5.5f || lookDot >= 0.90f)
                {
                    isAnomalyActive = false;
                    cooldownTimer = 28f;
                    targetTransform.gameObject.SetActive(false);
                    if (anomalyLight != null) anomalyLight.enabled = false;
                }
            }
            else if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }
        }

        private void UpdateMirrorRoomAnomaly(float dist, float lookDot)
        {
            if (targetTransform == null) return;

            if (displacedReflectionObject != null)
            {
                float shift = Mathf.Sin(Time.time * 1.4f) * 0.04f;
                displacedReflectionObject.localPosition = displacedBaseLocalPos + new Vector3(0f, 0f, shift);
            }

            if (dist >= 2.8f && dist <= 9.5f && lookDot > 0.65f)
            {
                if (!isAnomalyActive && cooldownTimer <= 0f)
                {
                    isAnomalyActive = true;
                    activeTimer = 1.6f;
                    targetTransform.gameObject.SetActive(true);
                    if (anomalyLight != null) anomalyLight.enabled = true;
                    if (audioSource != null && audioSource.clip != null) audioSource.Play();
                }
            }

            if (isAnomalyActive)
            {
                activeTimer -= Time.deltaTime;
                // Pequeño retraso lateral simulando reflejo desincronizado
                targetTransform.localPosition = new Vector3(-0.04f, 0.08f, Mathf.Sin(Time.time * 2.5f) * 0.08f);
                if (activeTimer <= 0f || dist < 1.8f)
                {
                    isAnomalyActive = false;
                    cooldownTimer = 30f;
                    targetTransform.gameObject.SetActive(false);
                    if (anomalyLight != null) anomalyLight.enabled = false;
                }
            }
            else if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }
        }

        private void UpdateMannequinAnomaly(float dist, float lookDot)
        {
            // 1. Ilusión óptica de respiración suave en la silueta cubierta por tela cuando se observa de lejos
            if (secondaryTransform != null)
            {
                if (dist > 2.8f && dist < 12f)
                {
                    float breath = 1f + Mathf.Sin(Time.time * 1.9f) * 0.025f;
                    secondaryTransform.localScale = new Vector3(baseScale.x * breath, baseScale.y * (1f + (breath - 1f) * 0.5f), baseScale.z * breath);
                }
                else
                {
                    secondaryTransform.localScale = baseScale;
                }
            }

            // 2. El maniquí gira discretamente una sola vez cuando el jugador está en la sala pero mira hacia otro lado
            if (!hasTurnedOnce && targetTransform != null && dist > 3.0f && dist < 9.0f && lookDot < -0.25f)
            {
                hasTurnedOnce = true;
                Vector3 dir = playerCam.position - targetTransform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.1f)
                {
                    targetTransform.rotation = Quaternion.LookRotation(dir.normalized);
                }
            }
        }

        private void UpdateSoloRoomAnomaly(float dist, float lookDot)
        {
            if (dist >= 2.2f && dist <= 7.5f && lookDot > 0.60f)
            {
                if (!isAnomalyActive && cooldownTimer <= 0f)
                {
                    isAnomalyActive = true;
                    activeTimer = 1.4f;
                    if (targetTransform != null) targetTransform.gameObject.SetActive(true);
                    if (audioSource != null && audioSource.clip != null) audioSource.Play();
                }
            }

            if (isAnomalyActive)
            {
                activeTimer -= Time.deltaTime;
                if (activeTimer <= 0f || dist < 1.6f)
                {
                    isAnomalyActive = false;
                    cooldownTimer = 32f;
                    if (targetTransform != null) targetTransform.gameObject.SetActive(false);
                }
            }
            else if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }
        }

        private void UpdateServicePianoAnomaly(float dist)
        {
            if (audioSource == null || audioSource.clip == null) return;

            // Se escucha música lejana de ensayo cuando el jugador está a media distancia (3.5m a 14m),
            // pero enmudece de golpe al acercarse a la puerta (< 3.0m)
            if (dist > 3.2f && dist < 14.5f)
            {
                if (!audioSource.isPlaying) audioSource.Play();
                float t = Mathf.InverseLerp(3.2f, 6.5f, dist);
                audioSource.volume = Mathf.Lerp(0.02f, 0.26f, t);
            }
            else
            {
                if (audioSource.isPlaying) audioSource.Stop();
            }
        }

        private AudioClip CreateGlassResonanceClip()
        {
            int sampleRate = 22050;
            int samples = sampleRate;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 3.5f);
                data[i] = (Mathf.Sin(2f * Mathf.PI * 587.33f * t) * 0.5f + Mathf.Sin(2f * Mathf.PI * 880f * t) * 0.3f) * env * 0.25f;
            }
            AudioClip clip = AudioClip.Create("Anomaly_MirrorResonance", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateSlipperSlideClip()
        {
            int sampleRate = 22050;
            int samples = (int)(sampleRate * 0.8f);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                // Fricción suave de satén sobre madera seguida de un leve apoyo de punta
                float slideEnv = Mathf.Sin(Mathf.Clamp01(t / 0.55f) * Mathf.PI);
                float noise = (Mathf.PerlinNoise(i * 0.18f, 0.5f) - 0.5f) * 2f;
                float tap = (t > 0.60f && t < 0.72f) ? Mathf.Exp(-(t - 0.60f) * 45f) * Mathf.Sin(2f * Mathf.PI * 190f * t) * 0.6f : 0f;
                data[i] = (noise * slideEnv * 0.22f) + tap;
            }
            AudioClip clip = AudioClip.Create("Anomaly_SlipperSlide", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateDistantPianoClip()
        {
            int sampleRate = 22050;
            int samples = sampleRate * 4;
            float[] data = new float[samples];
            // Arpegio lento y melancólico en Mi menor (Mi3 - Sol3 - Si3 - Do4 - Si3 - Sol3)
            float[] notes = { 164.81f, 196.00f, 246.94f, 261.63f, 246.94f, 196.00f, 164.81f, 146.83f };
            float noteLen = 0.5f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                int idx = Mathf.Clamp((int)(t / noteLen), 0, notes.Length - 1);
                float localT = t - (idx * noteLen);
                float env = Mathf.Exp(-localT * 4.2f);
                float f = notes[idx];
                float wave = Mathf.Sin(2f * Mathf.PI * f * localT) * 0.6f
                           + Mathf.Sin(2f * Mathf.PI * (f * 2f) * localT) * 0.25f;
                data[i] = wave * env * 0.30f;
            }
            AudioClip clip = AudioClip.Create("Anomaly_DistantRehearsalPiano", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
