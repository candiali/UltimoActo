using UnityEngine;
using UltimoActo.Interaction;
using UltimoActo.UI;

namespace UltimoActo.Narrative
{
    public enum BackstageAnomalyType
    {
        /// <summary>
        /// Zona 26 (Camerino de Mara): El reflejo del espejo muestra brevemente una postura de ballet
        /// desfasada que nadie está realizando cuando el jugador se mueve frente al tocador.
        /// </summary>
        MaraMirrorDelayedPose,

        /// <summary>
        /// Zona 27 (Camerino de Elena): El espejo agrietado refleja momentáneamente una silueta detrás del jugador
        /// (que desaparece al girarse), y el maniquí del Cisne Negro cambia ligeramente de postura tras salir y regresar.
        /// </summary>
        ElenaMirrorAndCostumeShift,

        /// <summary>
        /// Zona 28 (Pasillo detrás del escenario): Pasos en tablas huecas, roce de cortinas o notas al otro lado
        /// del telón, y una lámpara de servicio al fondo que se enciende sola y se apaga antes de llegar.
        /// </summary>
        BackstageCurtainSoundsAndLight,

        /// <summary>
        /// Zona 29 (Almacén de decorados): Una cortina escénica se mece sin corriente de aire y una silueta
        /// parece cruzar brevemente detrás de un panel pintado de bosque nocturno.
        /// </summary>
        SceneryCurtainAndSilhouette,

        /// <summary>
        /// Zona 30 (Sala de mecanismos y poleas): Sonidos de cuerda tensándose, metal crujiendo y una polea
        /// elevada que gira lentamente sola cuando los jugadores no están cerca.
        /// </summary>
        PulleyTensionAndSelfTurn,

        /// <summary>
        /// Interacción Zona 27 (Camerino de Elena): Reconstruir los últimos preparativos secretos de Elena
        /// inspeccionando el vestuario del Cisne Negro y sus objetos personales.
        /// </summary>
        InteractiveElenaBlackSwanSuit,

        /// <summary>
        /// Interacción Zona 29 (Almacén de decorados): Comparar el panel pintado de 'El Lago de las Sombras'
        /// con las fotografías oficiales y descubrir cómo se modificó la escenografía.
        /// </summary>
        InteractiveSceneryComparison,

        /// <summary>
        /// Interacción / Puzle Zona 30 (Sala de mecanismos y poleas): Interpretar el plano técnico original
        /// y accionar las palancas de contrapeso para liberar el acceso al Escenario Principal y abrir el compartimento.
        /// </summary>
        InteractivePulleyMechanismPuzzle
    }

    /// <summary>
    /// Controlador de anomalías psicológicas e interacciones narrativas para la
    /// SECCIÓN 6: CAMERINOS Y BACKSTAGE — ZONA RESTRINGIDA (Zonas 26 a 30).
    /// </summary>
    public class BackstageAnomalyController : MonoBehaviour, IInteractable
    {
        [SerializeField] private BackstageAnomalyType anomalyType;
        [SerializeField] private Transform primaryMovingPart;
        [SerializeField] private Transform secondaryMovingPart;
        [SerializeField] private Light targetServiceLight;
        [SerializeField] private GameObject secretRevealObject;
        [SerializeField] private WorldDoubleDoor stageAccessDoubleDoor;

        private AudioSource audioSource;
        private AudioClip proceduralClip;

        private Quaternion initialRotPrimary;
        private Vector3 initialPosPrimary;
        private Quaternion initialRotSecondary;
        private Vector3 initialPosSecondary;
        private float baseLightIntensity = 1.6f;

        private float anomalyTimer;
        private float activePulseTimer;
        private bool hasEnteredElenaRoomOnce;
        private bool hasLeftElenaRoomAfterEntering;
        private bool hasShiftedBlackSwanCostume;

        private bool interactionResolved;
        private float mechanismAnimProgress;

        public void Configure(
            BackstageAnomalyType type,
            Transform primaryPart = null,
            Transform secondaryPart = null,
            Light lightRef = null,
            GameObject revealObj = null,
            WorldDoubleDoor stageDoor = null)
        {
            anomalyType = type;
            primaryMovingPart = primaryPart;
            secondaryMovingPart = secondaryPart;
            targetServiceLight = lightRef;
            secretRevealObject = revealObj;
            stageAccessDoubleDoor = stageDoor;
        }

        private void Awake()
        {
            if (primaryMovingPart != null)
            {
                initialRotPrimary = primaryMovingPart.localRotation;
                initialPosPrimary = primaryMovingPart.localPosition;
            }

            if (secondaryMovingPart != null)
            {
                initialRotSecondary = secondaryMovingPart.localRotation;
                initialPosSecondary = secondaryMovingPart.localPosition;
            }

            if (targetServiceLight != null)
            {
                baseLightIntensity = targetServiceLight.intensity;
            }

            if (secretRevealObject != null &&
                (anomalyType == BackstageAnomalyType.InteractiveElenaBlackSwanSuit ||
                 anomalyType == BackstageAnomalyType.InteractiveSceneryComparison ||
                 anomalyType == BackstageAnomalyType.InteractivePulleyMechanismPuzzle))
            {
                secretRevealObject.SetActive(false);
            }

            SetupAudio();
        }

        private void SetupAudio()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 1.8f;
            audioSource.maxDistance = 16.0f;
            audioSource.playOnAwake = false;

            switch (anomalyType)
            {
                case BackstageAnomalyType.MaraMirrorDelayedPose:
                case BackstageAnomalyType.ElenaMirrorAndCostumeShift:
                case BackstageAnomalyType.InteractiveElenaBlackSwanSuit:
                    proceduralClip = CreateSilkAndWhisperClip();
                    break;

                case BackstageAnomalyType.BackstageCurtainSoundsAndLight:
                case BackstageAnomalyType.SceneryCurtainAndSilhouette:
                case BackstageAnomalyType.InteractiveSceneryComparison:
                    proceduralClip = CreateBackstageFootstepsAndCurtainClip();
                    break;

                case BackstageAnomalyType.PulleyTensionAndSelfTurn:
                case BackstageAnomalyType.InteractivePulleyMechanismPuzzle:
                    proceduralClip = CreatePulleyRopeAndGearClip();
                    break;
            }

            audioSource.clip = proceduralClip;
        }

        private void Update()
        {
            Transform player = GetClosestPlayer(out float dist, out bool playerLookingAtMe);
            if (player == null) return;

            switch (anomalyType)
            {
                case BackstageAnomalyType.MaraMirrorDelayedPose:
                    UpdateMaraMirrorPose(dist, playerLookingAtMe);
                    break;

                case BackstageAnomalyType.ElenaMirrorAndCostumeShift:
                    UpdateElenaMirrorAndCostume(player, dist, playerLookingAtMe);
                    break;

                case BackstageAnomalyType.BackstageCurtainSoundsAndLight:
                    UpdateBackstageCorridorAnomaly(dist);
                    break;

                case BackstageAnomalyType.SceneryCurtainAndSilhouette:
                    UpdateSceneryStorageAnomaly(dist, playerLookingAtMe);
                    break;

                case BackstageAnomalyType.PulleyTensionAndSelfTurn:
                    UpdatePulleyRoomAnomaly(dist);
                    break;

                case BackstageAnomalyType.InteractivePulleyMechanismPuzzle:
                    UpdatePulleyPuzzleAnimation();
                    break;
            }
        }

        private void UpdateMaraMirrorPose(float dist, bool playerLookingAtMe)
        {
            // Muestra brevemente una silueta en postura de ballet en el espejo cuando el jugador está a media distancia
            anomalyTimer += Time.deltaTime;
            bool showDelayedReflection = dist > 2.2f && dist < 6.8f && Mathf.Sin(anomalyTimer * 1.3f) > 0.65f;

            if (primaryMovingPart != null)
            {
                primaryMovingPart.gameObject.SetActive(showDelayedReflection);
                if (showDelayedReflection)
                {
                    // Ligero retardo oscilante imitando un movimiento de brazos de ballet
                    float sway = Mathf.Sin(Time.time * 2.1f) * 8f;
                    primaryMovingPart.localRotation = initialRotPrimary * Quaternion.Euler(0f, sway, 0f);
                }
            }
        }

        private void UpdateElenaMirrorAndCostume(Transform player, float dist, bool playerLookingAtMe)
        {
            // 1. Control de entrada/salida del Camerino de Elena para cambiar ligeramente el Vestuario del Cisne Negro al regresar
            if (dist < 4.2f)
            {
                if (!hasEnteredElenaRoomOnce)
                {
                    hasEnteredElenaRoomOnce = true;
                }
                else if (hasLeftElenaRoomAfterEntering && !hasShiftedBlackSwanCostume)
                {
                    hasShiftedBlackSwanCostume = true;
                    if (secondaryMovingPart != null)
                    {
                        // El maniquí del Cisne Negro aparece ligeramente girado hacia la puerta y con el brazo elevado
                        secondaryMovingPart.localRotation = initialRotSecondary * Quaternion.Euler(0f, -28f, 4f);
                        secondaryMovingPart.localPosition = initialPosSecondary + new Vector3(0.08f, 0f, -0.10f);
                    }
                    if (!audioSource.isPlaying)
                    {
                        audioSource.volume = 0.35f;
                        audioSource.Play();
                    }
                }
            }
            else if (hasEnteredElenaRoomOnce && dist > 7.5f)
            {
                hasLeftElenaRoomAfterEntering = true;
            }

            // 2. Silueta fugaz detrás del jugador reflejada en el espejo agrietado (desaparece si el jugador se gira hacia atrás)
            if (primaryMovingPart != null)
            {
                Vector3 toMirror = (transform.position - player.position).normalized;
                float facingMirrorDot = Vector3.Dot(player.forward, toMirror);
                bool lookingTowardsMirror = facingMirrorDot > 0.55f;

                bool showSilhouetteBehind = dist > 1.8f && dist < 5.2f && lookingTowardsMirror && (Mathf.Sin(Time.time * 1.5f) > 0.58f);
                primaryMovingPart.gameObject.SetActive(showSilhouetteBehind);
            }
        }

        private void UpdateBackstageCorridorAnomaly(float dist)
        {
            // La luz de servicio del fondo se enciende cuando el jugador está lejos (7..20m) y se apaga antes de que llegue (< 5.2m)
            if (targetServiceLight != null)
            {
                bool shouldIlluminateDistant = dist > 5.4f && dist < 21.0f;
                float targetIntensity = shouldIlluminateDistant
                    ? baseLightIntensity * (0.85f + 0.15f * Mathf.Sin(Time.time * 6.0f))
                    : 0.04f;
                targetServiceLight.intensity = Mathf.Lerp(targetServiceLight.intensity, targetIntensity, Time.deltaTime * 5.0f);
            }

            // Sonidos de pasos y roce de pesados telones al otro lado de las cortinas
            anomalyTimer += Time.deltaTime;
            if (dist > 3.2f && dist < 14.5f && anomalyTimer > 9.0f)
            {
                anomalyTimer = 0f;
                if (!audioSource.isPlaying)
                {
                    audioSource.volume = 0.42f;
                    audioSource.Play();
                }
            }
        }

        private void UpdateSceneryStorageAnomaly(float dist, bool playerLookingAtMe)
        {
            // Una cortina escénica se mueve suavemente sin corriente de aire
            if (primaryMovingPart != null)
            {
                float breezeAngle = Mathf.Sin(Time.time * 1.8f) * 5.5f;
                primaryMovingPart.localRotation = initialRotPrimary * Quaternion.Euler(breezeAngle, 0f, breezeAngle * 0.5f);
            }

            // Una silueta parece pasar detrás de un panel pintado cuando el jugador está a media distancia y se oculta al acercarse
            if (secondaryMovingPart != null)
            {
                bool showPassingFigure = dist > 5.0f && dist < 13.5f && !playerLookingAtMe;
                secondaryMovingPart.gameObject.SetActive(showPassingFigure);
                if (showPassingFigure)
                {
                    float slideOffset = Mathf.PingPong(Time.time * 0.9f, 1.4f) - 0.7f;
                    secondaryMovingPart.localPosition = initialPosSecondary + new Vector3(slideOffset, 0f, 0f);
                }
            }
        }

        private void UpdatePulleyRoomAnomaly(float dist)
        {
            // Una polea gira lentamente sola y suena la cuerda tensándose cuando nadie está cerca (> 4.5m)
            if (dist > 4.5f && dist < 16.0f)
            {
                if (primaryMovingPart != null)
                {
                    primaryMovingPart.Rotate(Vector3.right, 28f * Time.deltaTime, Space.Self);
                }

                anomalyTimer += Time.deltaTime;
                if (anomalyTimer > 8.5f)
                {
                    anomalyTimer = 0f;
                    if (!audioSource.isPlaying)
                    {
                        audioSource.volume = 0.40f;
                        audioSource.Play();
                    }
                }
            }
        }

        private void UpdatePulleyPuzzleAnimation()
        {
            if (!interactionResolved) return;

            if (mechanismAnimProgress < 1f)
            {
                mechanismAnimProgress = Mathf.MoveTowards(mechanismAnimProgress, 1f, Time.deltaTime * 0.65f);

                // Palanca principal baja y el contrapeso sube/baja suavemente
                if (primaryMovingPart != null)
                {
                    primaryMovingPart.localRotation = Quaternion.Slerp(
                        initialRotPrimary,
                        initialRotPrimary * Quaternion.Euler(48f, 0f, 0f),
                        mechanismAnimProgress);
                }

                if (secondaryMovingPart != null)
                {
                    secondaryMovingPart.localPosition = Vector3.Lerp(
                        initialPosSecondary,
                        initialPosSecondary + new Vector3(0f, 1.45f, 0f),
                        mechanismAnimProgress);
                }
            }
        }

        public string Id => $"backstage_{anomalyType}_{gameObject.name}";

        public string Prompt
        {
            get
            {
                switch (anomalyType)
                {
                    case BackstageAnomalyType.InteractiveElenaBlackSwanSuit:
                        return interactionResolved
                            ? "[E] Volver a examinar el traje del Cisne Negro y la costura oculta"
                            : "[E] Examinar el Vestuario del Cisne Negro y reconstruir los preparativos de Elena";

                    case BackstageAnomalyType.InteractiveSceneryComparison:
                        return interactionResolved
                            ? "[E] Revisar análisis del decorado retirado de 'El Lago de las Sombras'"
                            : "[E] Comparar panel escenográfico con las fotografías oficiales";

                    case BackstageAnomalyType.InteractivePulleyMechanismPuzzle:
                        return interactionResolved
                            ? "[E] Inspeccionar mecanismo de contrapesos (Acceso al Escenario Principal desbloqueado)"
                            : "[E] Interpretar plano técnico y accionar palancas de contrapeso hacia el Escenario";

                    default:
                        return string.Empty;
                }
            }
        }

        public bool CanInteract(GameObject player)
        {
            return anomalyType == BackstageAnomalyType.InteractiveElenaBlackSwanSuit ||
                   anomalyType == BackstageAnomalyType.InteractiveSceneryComparison ||
                   anomalyType == BackstageAnomalyType.InteractivePulleyMechanismPuzzle;
        }

        public void Interact(GameObject player)
        {
            switch (anomalyType)
            {
                case BackstageAnomalyType.InteractiveElenaBlackSwanSuit:
                    ExecuteElenaSuitReconstruction();
                    break;

                case BackstageAnomalyType.InteractiveSceneryComparison:
                    ExecuteSceneryComparison();
                    break;

                case BackstageAnomalyType.InteractivePulleyMechanismPuzzle:
                    ExecutePulleyMechanismPuzzle();
                    break;
            }
        }

        private void ExecuteElenaSuitReconstruction()
        {
            interactionResolved = true;
            if (secretRevealObject != null) secretRevealObject.SetActive(true);
            if (!audioSource.isPlaying)
            {
                audioSource.volume = 0.55f;
                audioSource.Play();
            }

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "Reconstrucción — Los Últimos Preparativos de Elena Vesper",
                    "Al examinar de cerca el corpiño de terciopelo negro del traje del Cisne Negro y compararlo con la partitura doblada y las páginas arrancadas de su diario:\n\n" +
                    "1. En el forro interior del traje hay cosida una nota manuscrita de Elena:\n" +
                    "   «Sé que Isadora ordenó cambiar la música del tercer acto y bajar el bastidor falso en el compás 48 para sustituirme en la penumbra».\n\n" +
                    "2. Junto a las zapatillas de punta aparece un croquis de los mecanismos del escenario:\n" +
                    "   «Le pedí al técnico de tramoya que dejara preparado el Contrapeso III en la Sala de Poleas. Cuando suene el Do Natural de la partitura original, cruzaré detrás del decorado del lago para revelar la verdad ante todo el teatro».\n\n" +
                    "Pero algo salió mal durante la representación: Elena nunca llegó a salir por la puerta principal.",
                    "Pista clave conectada con el Archivo Musical y la Sala de Mecanismos y Poleas (Zona 30)");
            }
        }

        private void ExecuteSceneryComparison()
        {
            interactionResolved = true;
            if (secretRevealObject != null) secretRevealObject.SetActive(true);
            if (!audioSource.isPlaying)
            {
                audioSource.volume = 0.55f;
                audioSource.Play();
            }

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "Comparativa Escenográfica — El Decorado Oculto del Lago Nocturno",
                    "Al retirar parcialmente la lona polvorienta y comparar este gran panel pintado con las fotografías encontradas en Administración y Camerinos:\n\n" +
                    "• En las fotografías oficiales del estreno, el fondo del escenario muestra un muro cerrado de columnas neoclásicas.\n" +
                    "• Sin embargo, este panel original de «El Lago de las Sombras» tiene una abertura central practicable (el Paso del Cisne) diseñada para que la primera bailarina emergiera desde el fondo del escenario.\n\n" +
                    "• En el bastidor de madera hay una etiqueta de tramoya con fecha **13 DE OCTUBRE DE 1924 (22:45 h)** —el día ANTES de la desaparición de Elena—:\n" +
                    "  «Por orden de Dirección: sustituir el arco practicable por el panel ciego y bloquear el Contrapeso III en la Sala de Poleas».",
                    "Evidencia de manipulación previa a la última función");
            }
        }

        private void ExecutePulleyMechanismPuzzle()
        {
            interactionResolved = true;
            if (secretRevealObject != null) secretRevealObject.SetActive(true);
            if (!audioSource.isPlaying)
            {
                audioSource.volume = 0.70f;
                audioSource.Play();
            }

            // Desbloquea y abre las puertas dobles hacia el Escenario Principal en el Pasillo de Backstage
            if (stageAccessDoubleDoor != null)
            {
                stageAccessDoubleDoor.SetLockState(false);
                if (!stageAccessDoubleDoor.IsOpen)
                {
                    stageAccessDoubleDoor.ToggleDoor();
                }
            }

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "Sistema de Contrapesos Restablecido — Ruta al Escenario Principal Abierta",
                    "Siguiendo las anotaciones del plano técnico original, liberas el freno del **Contrapeso III (Telón de Guardia y Bastidor del Lago)** y alineas el tambor de poleas en su posición original.\n\n" +
                    "Las cuerdas de cáñamo se tensan en lo alto del peine escénico y escuchas cómo se levanta la barra de bloqueo mecánica de las **Puertas del Escenario Principal** en el pasillo detrás del escenario.\n\n" +
                    "Al desplazarse el contrapeso, queda al descubierto un compartimento técnico con el testimonio del jefe de tramoya:\n" +
                    "«La noche del 14 de octubre, alguien accionó la palanca de cierre desde esta misma plataforma justo cuando Elena cruzaba entre los bastidores del escenario. Cuando corrí a levantar el contrapeso, solo encontré una zapatilla junto a la trampilla del proscenio».",
                    "Las puertas hacia el Escenario Principal (Zona 28 -> Teatro) han quedado desbloqueadas");
            }
        }

        private Transform GetClosestPlayer(out float minDistance, out bool isLookingAtThis)
        {
            minDistance = float.MaxValue;
            isLookingAtThis = false;
            Transform closest = null;

            Camera[] cams = Camera.allCameras;
            for (int i = 0; i < cams.Length; i++)
            {
                Camera c = cams[i];
                if (c == null || !c.gameObject.activeInHierarchy) continue;

                float d = Vector3.Distance(c.transform.position, transform.position);
                if (d < minDistance)
                {
                    minDistance = d;
                    closest = c.transform;
                    Vector3 toObj = (transform.position - c.transform.position).normalized;
                    isLookingAtThis = Vector3.Dot(c.transform.forward, toObj) > 0.52f;
                }
            }

            return closest;
        }

        private static AudioClip CreateSilkAndWhisperClip()
        {
            int sampleRate = 22050;
            int samples = sampleRate * 2;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(Mathf.Clamp01(t / 2.0f) * Mathf.PI);
                float rustle = (Mathf.PerlinNoise(t * 95f, 1.7f) * 2f - 1f) * 0.14f;
                float chime = Mathf.Sin(2f * Mathf.PI * 523.25f * t) * Mathf.Exp(-t * 2.5f) * 0.08f;
                data[i] = (rustle + chime) * env;
            }
            AudioClip clip = AudioClip.Create("Backstage_SilkWhisper", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateBackstageFootstepsAndCurtainClip()
        {
            int sampleRate = 22050;
            int samples = sampleRate * 3;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float step1 = Mathf.Exp(-Mathf.Pow((t - 0.4f) * 18f, 2f)) * Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.35f;
                float step2 = Mathf.Exp(-Mathf.Pow((t - 1.1f) * 18f, 2f)) * Mathf.Sin(2f * Mathf.PI * 105f * t) * 0.30f;
                float step3 = Mathf.Exp(-Mathf.Pow((t - 1.8f) * 18f, 2f)) * Mathf.Sin(2f * Mathf.PI * 98f * t) * 0.24f;
                float curtain = (Mathf.PerlinNoise(t * 45f, 3.1f) * 2f - 1f) * Mathf.Exp(-Mathf.Pow((t - 2.3f) * 3f, 2f)) * 0.16f;
                data[i] = step1 + step2 + step3 + curtain;
            }
            AudioClip clip = AudioClip.Create("Backstage_StepsAndCurtain", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreatePulleyRopeAndGearClip()
        {
            int sampleRate = 22050;
            int samples = sampleRate * 3;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(Mathf.Clamp01(t / 3.0f) * Mathf.PI);
                float creak = Mathf.Sin(2f * Mathf.PI * (82f + 18f * Mathf.Sin(t * 4.5f)) * t) * 0.18f;
                float rope = (Mathf.PerlinNoise(t * 60f, 0.4f) * 2f - 1f) * 0.12f;
                float clank = Mathf.Exp(-Mathf.Pow((t - 1.5f) * 14f, 2f)) * Mathf.Sin(2f * Mathf.PI * 195f * t) * 0.32f;
                data[i] = (creak + rope + clank) * env;
            }
            AudioClip clip = AudioClip.Create("Backstage_PulleyGear", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
