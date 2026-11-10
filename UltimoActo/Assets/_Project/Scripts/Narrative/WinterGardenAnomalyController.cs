using UnityEngine;
using UltimoActo.Interaction;
using UltimoActo.UI;

namespace UltimoActo.Narrative
{
    public enum WinterGardenNodeType
    {
        /// <summary>
        /// Fuente circular seca: reproduce un murmullo sutil de agua corriendo en una fuente seca
        /// que calla al acercarse al borde, y permite inspeccionar los cisnes de piedra.
        /// </summary>
        DryFountainWaterEchoAndInspect,

        /// <summary>
        /// Estatua conmemorativa de Elena Vesper: revela la inscripción con fecha posterior
        /// a la desaparición oficial y hace aparecer silenciosamente un objeto nuevo sobre la mesa de piedra.
        /// </summary>
        InteractiveElenaMemorialStatue,

        /// <summary>
        /// Rincón oculto tras la vegetación: losa de piedra desplazada y pequeña caja metálica
        /// parcialmente enterrada con fotografía antigua y fragmento de carta.
        /// </summary>
        InteractiveHiddenSlabAndMetalBox,

        /// <summary>
        /// Anomalía psicológica: estatuas de bailarinas que rotan sutilmente hacia el jugador
        /// únicamente cuando no están siendo observadas, más eco de piano distante.
        /// </summary>
        AnomalyWeepingBallerinaStatuesAndPiano
    }

    /// <summary>
    /// Controlador de interacciones narrativas y anomalías psicológicas para el
    /// JARDÍN DE INVIERNO (Zona 40):
    /// - Fuente circular seca con murmullo fantasma de agua.
    /// - Inscripción conmemorativa de Elena Vesper con fecha posterior al 14 de octubre de 1924.
    /// - Objeto que aparece sobre la pequeña mesa de piedra donde antes no había nada.
    /// - Losa desplazada y caja metálica semienterrada en el rincón oculto tras la vegetación.
    /// - Estatuas de bailarinas que cambian sutilmente de orientación al apartar la vista y melodía de piano lejana.
    /// </summary>
    public class WinterGardenAnomalyController : MonoBehaviour, IInteractable
    {
        [SerializeField] private WinterGardenNodeType nodeType;
        [SerializeField] private Transform primaryTarget;
        [SerializeField] private Transform secondaryTarget;
        [SerializeField] private GameObject appearingTableObject;

        private AudioSource audioSource;
        private Transform playerTransform;
        private Camera playerCamera;

        private bool isInspected;
        private float animTimer;
        private Vector3 slabInitialLocalPos;
        private Vector3 boxInitialLocalPos;
        private float statueRotateCooldown;
        private bool hasPlayedDistantPiano;

        public void Configure(
            WinterGardenNodeType type,
            Transform primary = null,
            Transform secondary = null,
            GameObject appearingObj = null)
        {
            nodeType = type;
            primaryTarget = primary;
            secondaryTarget = secondary;
            appearingTableObject = appearingObj;
        }

        private void Awake()
        {
            if (primaryTarget != null) slabInitialLocalPos = primaryTarget.localPosition;
            if (secondaryTarget != null) boxInitialLocalPos = secondaryTarget.localPosition;

            if (appearingTableObject != null && nodeType == WinterGardenNodeType.InteractiveElenaMemorialStatue)
            {
                appearingTableObject.SetActive(false);
            }

            SetupSynthesizedAudio();
        }

        private void SetupSynthesizedAudio()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 1.5f;
            audioSource.maxDistance = 16.0f;

            int sampleRate = 22050;
            float duration = (nodeType == WinterGardenNodeType.DryFountainWaterEchoAndInspect ||
                              nodeType == WinterGardenNodeType.AnomalyWeepingBallerinaStatuesAndPiano) ? 4.8f : 2.6f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float s = 0f;

                if (nodeType == WinterGardenNodeType.DryFountainWaterEchoAndInspect)
                {
                    // Murmullo cristalino y goteo de agua en una fuente que en realidad está seca
                    float env = Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
                    float trickle = (Mathf.PerlinNoise(t * 240f, 1.7f) - 0.5f) * 0.22f * env;
                    float drop1 = (t > 0.8f && t < 1.05f) ? Mathf.Sin(2f * Mathf.PI * 980f * (t - 0.8f)) * Mathf.Exp(-(t - 0.8f) * 18f) * 0.35f : 0f;
                    float drop2 = (t > 2.1f && t < 2.35f) ? Mathf.Sin(2f * Mathf.PI * 1120f * (t - 2.1f)) * Mathf.Exp(-(t - 2.1f) * 18f) * 0.32f : 0f;
                    float drop3 = (t > 3.4f && t < 3.65f) ? Mathf.Sin(2f * Mathf.PI * 890f * (t - 3.4f)) * Mathf.Exp(-(t - 3.4f) * 18f) * 0.28f : 0f;
                    s = trickle + drop1 + drop2 + drop3;
                }
                else if (nodeType == WinterGardenNodeType.AnomalyWeepingBallerinaStatuesAndPiano)
                {
                    // Melodía de piano muy distante (Mi4 - Sol4 - Si4 - Do5 - Si4) con reverberación melancólica
                    float[] notes = { 329.63f, 392.00f, 493.88f, 523.25f, 493.88f };
                    int idx = Mathf.Clamp(Mathf.FloorToInt(t / 0.92f), 0, notes.Length - 1);
                    float localT = t - (idx * 0.92f);
                    float env = Mathf.Exp(-localT * 2.6f) * Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
                    float f = notes[idx];
                    s = (Mathf.Sin(2f * Mathf.PI * f * t) * 0.24f +
                         Mathf.Sin(2f * Mathf.PI * f * 2f * t) * 0.08f) * env;
                }
                else if (nodeType == WinterGardenNodeType.InteractiveHiddenSlabAndMetalBox)
                {
                    // Roce de piedra húmeda desplazándose y pestillo metálico oxidado
                    float stoneSlide = (t < 1.3f) ? (Mathf.PerlinNoise(t * 75f, 0.3f) - 0.5f) * 0.45f * Mathf.Sin((t / 1.3f) * Mathf.PI) : 0f;
                    float metalLatch = (t >= 1.35f && t < 1.85f) ? Mathf.Sin(2f * Mathf.PI * 620f * (t - 1.35f)) * Mathf.Exp(-(t - 1.35f) * 14f) * 0.42f : 0f;
                    s = stoneSlide + metalLatch;
                }
                else
                {
                    // Acorde resonante grave y melancólico al leer la inscripción conmemorativa de Elena
                    float env = Mathf.Exp(-t * 1.3f);
                    s = (Mathf.Sin(2f * Mathf.PI * 261.63f * t) * 0.28f +
                         Mathf.Sin(2f * Mathf.PI * 329.63f * t) * 0.20f +
                         Mathf.Sin(2f * Mathf.PI * 392.00f * t) * 0.16f) * env;
                }

                samples[i] = Mathf.Clamp(s, -0.95f, 0.95f);
            }

            AudioClip clip = AudioClip.Create($"SFX_WinterGarden_{nodeType}", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            audioSource.clip = clip;
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                GameObject pObj = GameObject.FindGameObjectWithTag("Player");
                if (pObj != null) playerTransform = pObj.transform;
                playerCamera = Camera.main;
                if (playerTransform == null) return;
            }

            float dist = Vector3.Distance(playerTransform.position, transform.position);

            if (nodeType == WinterGardenNodeType.DryFountainWaterEchoAndInspect)
            {
                // Cuando el jugador está a media distancia (4.5m a 10.5m), se oye agua en la fuente seca;
                // si se acerca a menos de 3.2m para mirar el interior seco, el sonido de agua se corta.
                if (dist > 4.5f && dist < 10.5f)
                {
                    if (!audioSource.isPlaying)
                    {
                        audioSource.loop = true;
                        audioSource.volume = 0.42f;
                        audioSource.Play();
                    }
                }
                else if (dist < 3.3f || dist > 12.0f)
                {
                    if (audioSource.isPlaying && audioSource.loop)
                    {
                        audioSource.Stop();
                    }
                }
            }
            else if (nodeType == WinterGardenNodeType.AnomalyWeepingBallerinaStatuesAndPiano)
            {
                if (dist < 16f && !hasPlayedDistantPiano && !audioSource.isPlaying)
                {
                    hasPlayedDistantPiano = true;
                    audioSource.loop = false;
                    audioSource.volume = 0.38f;
                    audioSource.Play();
                }

                // Si el jugador está en el jardín pero de espaldas a las estatuas, estas rotan sutilmente hacia su posición
                if (dist > 3.5f && dist < 18.0f && playerCamera != null)
                {
                    statueRotateCooldown -= Time.deltaTime;
                    if (statueRotateCooldown <= 0f)
                    {
                        TryRotateStatueWhenUnseen(primaryTarget);
                        TryRotateStatueWhenUnseen(secondaryTarget);
                    }
                }
            }
            else if (nodeType == WinterGardenNodeType.InteractiveHiddenSlabAndMetalBox && isInspected)
            {
                if (animTimer < 1f)
                {
                    animTimer = Mathf.MoveTowards(animTimer, 1f, Time.deltaTime * 1.2f);
                    float smooth = Mathf.SmoothStep(0f, 1f, animTimer);
                    if (primaryTarget != null)
                    {
                        primaryTarget.localPosition = slabInitialLocalPos + new Vector3(0.42f * smooth, 0f, 0.18f * smooth);
                    }
                    if (secondaryTarget != null)
                    {
                        secondaryTarget.localPosition = boxInitialLocalPos + new Vector3(0f, 0.08f * smooth, 0f);
                    }
                }
            }
        }

        private void TryRotateStatueWhenUnseen(Transform statue)
        {
            if (statue == null || playerCamera == null || playerTransform == null) return;

            Vector3 dirToStatue = (statue.position - playerCamera.transform.position).normalized;
            float dot = Vector3.Dot(playerCamera.transform.forward, dirToStatue);

            // Solo gira cuando está fuera del cono de visión del jugador (dot < -0.15f)
            if (dot < -0.15f)
            {
                Vector3 lookDir = playerTransform.position - statue.position;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.2f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
                    statue.rotation = Quaternion.RotateTowards(statue.rotation, targetRot, 28f * Time.deltaTime);
                    statueRotateCooldown = 0.05f;
                }
            }
        }

        public string Id => $"winter_garden_{nodeType}";

        public string Prompt
        {
            get
            {
                switch (nodeType)
                {
                    case WinterGardenNodeType.DryFountainWaterEchoAndInspect:
                        return "[E] Examinar Fuente Circular Seca y Escultura de los Cisnes";
                    case WinterGardenNodeType.InteractiveElenaMemorialStatue:
                        return isInspected
                            ? "[E] Volver a leer Inscripción Conmemorativa de Elena Vesper (Revisar Mesa de Piedra)"
                            : "[E] Examinar Estatua e Inscripción Conmemorativa de Elena Vesper";
                    case WinterGardenNodeType.InteractiveHiddenSlabAndMetalBox:
                        return isInspected
                            ? "[E] Examinar Caja Metálica Desenterrada (Fotografía y Carta)"
                            : "[E] Apartar Losa de Piedra Desplazada y Abrir Caja Metálica Enterrada";
                    default:
                        return string.Empty;
                }
            }
        }

        public bool CanInteract(GameObject player)
        {
            return nodeType == WinterGardenNodeType.DryFountainWaterEchoAndInspect ||
                   nodeType == WinterGardenNodeType.InteractiveElenaMemorialStatue ||
                   nodeType == WinterGardenNodeType.InteractiveHiddenSlabAndMetalBox;
        }

        public void Interact(GameObject player)
        {
            switch (nodeType)
            {
                case WinterGardenNodeType.DryFountainWaterEchoAndInspect:
                    InspectDryFountain();
                    break;
                case WinterGardenNodeType.InteractiveElenaMemorialStatue:
                    InspectElenaMemorialStatue();
                    break;
                case WinterGardenNodeType.InteractiveHiddenSlabAndMetalBox:
                    InspectHiddenSlabAndMetalBox();
                    break;
            }
        }

        private void InspectDryFountain()
        {
            isInspected = true;
            if (audioSource.isPlaying && audioSource.loop)
            {
                audioSource.Stop();
            }

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "Fuente Circular de los Cisnes — El Sonido del Agua Ausente",
                    "Al asomarte al brocal de piedra gris cálida, el murmullo de agua que escuchabas desde los senderos cesa de golpe.\n\n" +
                    "• La taza circular de la fuente está **completamente seca**, cubierta de musgo oscuro, hojas marchitas y polvo acumulado durante años.\n" +
                    "• En el centro se alzan dos cisnes entrelazados tallados en piedra con detalles de latón oxidado. En la base del cisne negro hay una marca cincelada que apunta hacia el **sendero noreste (Estatua Conmemorativa)** y hacia el **rincón sureste oculto detrás de los helechos**.",
                    "Jardín de Invierno — Punto de referencia central");
            }
        }

        private void InspectElenaMemorialStatue()
        {
            isInspected = true;

            // Anomalía psicológica: al inspeccionar la inscripción de la estatua, aparece un objeto
            // sobre la pequeña mesa de piedra donde antes no había nada.
            if (appearingTableObject != null && !appearingTableObject.activeSelf)
            {
                appearingTableObject.SetActive(true);
            }

            if (!audioSource.isPlaying)
            {
                audioSource.loop = false;
                audioSource.volume = 0.65f;
                audioSource.Play();
            }

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "Inscripción Conmemorativa de Elena Vesper — [CONTRADICCIÓN TEMPORAL]",
                    "Al limpiar el musgo que cubre la placa de latón antiguo al pie del pedestal de piedra, descubres una inscripción grabada que contradice todos los registros públicos de la academia:\n\n" +
                    "«EN MEMORIA DE LA INTERPRETACIÓN ÍNTIMA DE **ELENA VESPER** EN ESTE JARDÍN DE INVIERNO\n" +
                    "*Variación del Cisne en Do Natural — Noche del **17 de Octubre de 1924***»\n\n" +
                    "• **Contradicción clave**: Según el Libro de Recepción y el acta oficial de Administración (Sección 2), Elena Vesper desapareció durante la función de *El Lago de las Sombras* el **14 de octubre de 1924**. Sin embargo, esta placa certifica que bailó aquí tres noches después, horas antes de la clausura definitiva del edificio.\n\n" +
                    "*(Al apartar la vista del pedestal, escuchas un leve roce sobre la **pequeña mesa de piedra** junto al banco oeste: algo que antes no estaba allí acaba de aparecer sobre la piedra)*.",
                    "Revisa la Pequeña Mesa de Piedra y el Rincón Oculto al Sureste");
            }
        }

        private void InspectHiddenSlabAndMetalBox()
        {
            isInspected = true;

            if (!audioSource.isPlaying)
            {
                audioSource.loop = false;
                audioSource.volume = 0.68f;
                audioSource.Play();
            }

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "Caja Metálica Enterrada bajo la Losa — Fotografía y Fragmento de Carta",
                    "Oculta tras los helechos y raíces del rincón sureste, apartas la losa de piedra desplazada y abres una pequeña caja metálica oxidada protegida con tela encerada:\n\n" +
                    "1. **Fotografía Antigua en Sepia (17 de Octubre de 1924)**: Muestra a Elena Vesper junto a la fuente seca de este Jardín de Invierno bajo la luz lunar, sosteniendo un violonchelo y acompañada por otra figura cuyo rostro quedó en sombra bajo los arcos de hierro. Al dorso lleva el sello del **Archivo Musical (Sección 5 — Estante III)**.\n" +
                    "2. **Fragmento de Carta Manuscrita**:\n" +
                    "   *«...Isadora firmó en Administración el acta de mi desaparición con fecha del día 14 para que nadie buscara en los sótanos del teatro. Si estás leyendo esto en el Jardín de Invierno, busca la copia sin expurgar en el Archivo de Expedientes (Sección 2) y la cinta oculta en la Cabina de Grabaciones (Sección 5). Nunca abandoné la academia aquella noche...»*",
                    "Pista conectada con Administración (Sección 2) y Archivo Musical (Sección 5)");
            }
        }
    }
}
