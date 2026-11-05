using UnityEngine;

namespace UltimoActo.Narrative
{
    /// <summary>
    /// Controla la anomalía psicológica sutil en la Sala de Tocadores (Zona 12):
    /// Cuando el jugador observa el espejo de Elena a cierta distancia, aparece brevemente
    /// la silueta de una bailarina detrás del cristal durante un instante y se desvanece
    /// si el jugador se acerca demasiado o tras 1.8 segundos.
    /// </summary>
    public class MirrorAnomalyController : MonoBehaviour
    {
        [SerializeField] private GameObject silhouetteRoot;
        [SerializeField] private Light vanityLight;
        [SerializeField] private float minTriggerDist = 2.0f;
        [SerializeField] private float maxTriggerDist = 6.0f;
        [SerializeField] private float anomalyDuration = 1.8f;
        [SerializeField] private float cooldownSeconds = 35f;

        private Transform playerCam;
        private float activeTimer = 0f;
        private float cooldownTimer = 3f;
        private bool isShowing = false;
        private float baseLightIntensity = 1.4f;
        private AudioSource audioSource;

        public void Configure(GameObject silhouette, Light bulbLight)
        {
            silhouetteRoot = silhouette;
            vanityLight = bulbLight;
            if (vanityLight != null) baseLightIntensity = vanityLight.intensity;
            if (silhouetteRoot != null) silhouetteRoot.SetActive(false);
        }

        private void Start()
        {
            if (Camera.main != null) playerCam = Camera.main.transform;
            if (vanityLight != null) baseLightIntensity = vanityLight.intensity;
            if (silhouetteRoot != null) silhouetteRoot.SetActive(false);

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 1.5f;
            audioSource.maxDistance = 10f;
            audioSource.playOnAwake = false;
            audioSource.volume = 0.25f;
            audioSource.clip = CreateMusicBoxChordClip();
        }

        private void Update()
        {
            if (playerCam == null)
            {
                if (Camera.main != null) playerCam = Camera.main.transform;
                else return;
            }

            if (isShowing)
            {
                activeTimer -= Time.deltaTime;

                // Parpadeo sutil de la bombilla del tocador durante la anomalía
                if (vanityLight != null)
                {
                    float flicker = Mathf.PerlinNoise(Time.time * 18f, 0.5f);
                    vanityLight.intensity = baseLightIntensity * Mathf.Lerp(0.25f, 1.15f, flicker);
                }

                float dist = Vector3.Distance(playerCam.position, transform.position);
                if (activeTimer <= 0f || dist < 1.5f)
                {
                    EndAnomaly();
                }
                return;
            }

            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
                return;
            }

            float d = Vector3.Distance(playerCam.position, transform.position);
            if (d >= minTriggerDist && d <= maxTriggerDist)
            {
                Vector3 toMirror = (transform.position - playerCam.position).normalized;
                float dot = Vector3.Dot(playerCam.forward, toMirror);
                // El jugador mira en dirección al espejo
                if (dot > 0.72f)
                {
                    StartAnomaly();
                }
            }
        }

        private void StartAnomaly()
        {
            isShowing = true;
            activeTimer = anomalyDuration;
            if (silhouetteRoot != null) silhouetteRoot.SetActive(true);
            if (audioSource != null && audioSource.clip != null) audioSource.Play();
        }

        private void EndAnomaly()
        {
            isShowing = false;
            cooldownTimer = cooldownSeconds;
            if (silhouetteRoot != null) silhouetteRoot.SetActive(false);
            if (vanityLight != null) vanityLight.intensity = baseLightIntensity;
        }

        private AudioClip CreateMusicBoxChordClip()
        {
            int sampleRate = 22050;
            int samples = sampleRate * 2;
            float[] data = new float[samples];
            // Acorde menor melancólico (Mi4, Sol4, Si4) con decaimiento tipo caja de música
            float f1 = 329.63f;
            float f2 = 392.00f;
            float f3 = 493.88f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 2.4f);
                float wave = Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.4f
                           + Mathf.Sin(2f * Mathf.PI * f2 * t) * 0.3f
                           + Mathf.Sin(2f * Mathf.PI * f3 * t) * 0.3f;
                data[i] = wave * env * 0.35f;
            }

            AudioClip clip = AudioClip.Create("Anomaly_MusicBox_Chord", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
