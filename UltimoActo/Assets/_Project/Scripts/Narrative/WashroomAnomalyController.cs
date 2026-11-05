using UnityEngine;

namespace UltimoActo.Narrative
{
    /// <summary>
    /// Controla la anomalía psicológica acústica y visual en los Lavabos y Duchas Antiguas (Zona 15):
    /// Cuando el jugador se aleja del grifo o de la cabina de ducha vacía, se escucha el goteo
    /// o el murmullo breve del agua y cae una gota; al acercarse a inspeccionar, cesa en silencio.
    /// </summary>
    public class WashroomAnomalyController : MonoBehaviour
    {
        [SerializeField] private Transform dropletVisual;
        [SerializeField] private float silenceNearDistance = 3.2f;
        [SerializeField] private float maxHearDistance = 13.0f;
        [SerializeField] private float dripInterval = 3.6f;

        private Transform playerCam;
        private AudioSource audioSource;
        private AudioClip dripClip;
        private float timer = 1.5f;
        private Vector3 dropletStartLocalPos;
        private bool dropping = false;
        private float dropProgress = 0f;

        public void Configure(Transform droplet)
        {
            dropletVisual = droplet;
            if (dropletVisual != null) dropletStartLocalPos = dropletVisual.localPosition;
        }

        private void Start()
        {
            if (Camera.main != null) playerCam = Camera.main.transform;
            if (dropletVisual != null)
            {
                dropletStartLocalPos = dropletVisual.localPosition;
                dropletVisual.gameObject.SetActive(false);
            }

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 2.0f;
            audioSource.maxDistance = 14.0f;
            audioSource.playOnAwake = false;
            audioSource.volume = 0.35f;
            dripClip = CreateWaterDripClip();
        }

        private void Update()
        {
            if (playerCam == null)
            {
                if (Camera.main != null) playerCam = Camera.main.transform;
                else return;
            }

            if (dropping && dropletVisual != null)
            {
                dropProgress += Time.deltaTime * 2.6f;
                dropletVisual.localPosition = dropletStartLocalPos + new Vector3(0f, -dropProgress * 0.85f, 0f);
                if (dropProgress >= 1f)
                {
                    dropping = false;
                    dropletVisual.gameObject.SetActive(false);
                }
            }

            float dist = Vector3.Distance(playerCam.position, transform.position);

            // Solo ocurre cuando el jugador se ha alejado un poco (entre silenceNearDistance y maxHearDistance)
            if (dist > silenceNearDistance && dist < maxHearDistance)
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    timer = dripInterval + Random.Range(-0.6f, 1.2f);
                    TriggerDrip();
                }
            }
            else if (dist <= silenceNearDistance)
            {
                // Se detiene sutilmente cuando el jugador se acerca a comprobar la cabina/grifo
                if (dropping && dropletVisual != null)
                {
                    dropping = false;
                    dropletVisual.gameObject.SetActive(false);
                }
            }
        }

        private void TriggerDrip()
        {
            if (audioSource != null && dripClip != null)
            {
                audioSource.pitch = Random.Range(0.92f, 1.08f);
                audioSource.PlayOneShot(dripClip);
            }

            if (dropletVisual != null)
            {
                dropletVisual.localPosition = dropletStartLocalPos;
                dropletVisual.gameObject.SetActive(true);
                dropping = true;
                dropProgress = 0f;
            }
        }

        private AudioClip CreateWaterDripClip()
        {
            int sampleRate = 22050;
            int samples = sampleRate / 2; // 0.5s
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                // Gota resonante con barrido ascendente rápido de frecuencia + eco metálico de tubería
                float freq = Mathf.Lerp(650f, 1180f, Mathf.Clamp01(t * 14f));
                float env = Mathf.Exp(-t * 18f);
                float pipeEcho = Mathf.Sin(2f * Mathf.PI * 220f * t) * Mathf.Exp(-t * 6f) * 0.15f;
                data[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.55f) + pipeEcho;
            }

            AudioClip clip = AudioClip.Create("Anomaly_WaterDrip", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
