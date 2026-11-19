using UnityEngine;

namespace UltimoActo.Core
{
    /// <summary>
    /// Componente de parpadeo inestable para luces en la Época Ruinas (película de terror),
    /// manteniendo siempre un umbral mínimo de visibilidad para no dejar la sala a oscuras.
    /// </summary>
    public class HorrorLightFlicker : MonoBehaviour
    {
        [SerializeField] private Light targetLight;
        [SerializeField] private float baseIntensity = 1.65f;
        [SerializeField] private float flickerSpeed = 8.5f;
        [SerializeField] private float seed;

        public void Configure(Light l, float intensity, float speed, float randomSeed)
        {
            targetLight = l;
            baseIntensity = intensity;
            flickerSpeed = speed;
            seed = randomSeed;
        }

        private void Awake()
        {
            if (targetLight == null) targetLight = GetComponent<Light>();
            if (Mathf.Approximately(seed, 0f)) seed = Random.Range(0f, 100f);
        }

        private void Update()
        {
            if (targetLight == null || !targetLight.enabled) return;

            float t = Time.time * flickerSpeed + seed;
            float n1 = Mathf.PerlinNoise(t, seed * 0.37f);
            float n2 = Mathf.PerlinNoise(t * 2.4f, seed * 1.13f);
            float dip = (n1 < 0.18f) ? 0.42f : 1.0f;
            float pulse = Mathf.Lerp(0.75f, 1.30f, n2);
            targetLight.intensity = baseIntensity * dip * pulse;
        }
    }
}
