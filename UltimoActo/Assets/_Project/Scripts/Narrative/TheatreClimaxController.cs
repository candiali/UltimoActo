using UnityEngine;
using UltimoActo.Interaction;
using UltimoActo.UI;

namespace UltimoActo.Narrative
{
    public enum TheatreClimaxNodeType
    {
        /// <summary>
        /// Zona 31 (Patio de butacas): Desde el escenario o a distancia parecen verse figuras inmóviles
        /// sentadas en las butacas que desaparecen al acercarse, junto con aplausos lejanos que se cortan de golpe.
        /// </summary>
        AnomalyAuditoriumSpectatorsAndApplause,

        /// <summary>
        /// Zona 32 (Escenario principal): Silueta femenina al fondo del escenario bajo un foco frío ejecutando
        /// una postura de ballet, que desaparece cuando los jugadores se acercan o cambia la iluminación.
        /// </summary>
        AnomalyStageBallerinaSilhouette,

        /// <summary>
        /// Zona 33 (Foso de orquesta): Instrumentos que parecen sonar desde sillas vacías y repetición
        /// de la nota Do Natural al acercarse al atril y al hueco del violonchelo.
        /// </summary>
        AnomalyOrchestraPitPhantomMusic,

        /// <summary>
        /// Zona 34 (Cabina de iluminación): Un foco cenital se enciende brevemente solo, revelando una marca
        /// y una figura fugaz en el escenario no visibles desde el patio de butacas.
        /// </summary>
        AnomalyLightingBoothAutonomousSpot,

        /// <summary>
        /// Zona 35 (Pasarela superior): Reverberación de altura y figura inferior que parece desplazarse
        /// según el ángulo de observación desde la pasarela.
        /// </summary>
        AnomalyCatwalkPerspectiveEcho,

        /// <summary>
        /// Fase 1 de la Secuencia Final (Zona 33 — Foso de Orquesta): Ordenar los fragmentos musicales
        /// en el atril del director y confirmar la versión original de 'El Lago de las Sombras'.
        /// </summary>
        InteractivePhase1OrchestraStand,

        /// <summary>
        /// Fase 2 de la Secuencia Final (Zona 32 — Lateral/Centro del Escenario): Reconstruir las posiciones
        /// escénicas reales de Elena y Mara sobre las marcas de tiza y latón del escenario.
        /// </summary>
        InteractivePhase2StagePositions,

        /// <summary>
        /// Fase 3 de la Secuencia Final (Zona 34 — Cabina de Iluminación): Activar la consola de luces en el
        /// orden de las pistas técnicas para iluminar el Paso del Lago y las marcas ocultas del escenario.
        /// </summary>
        InteractivePhase3LightingConsole,

        /// <summary>
        /// Fase 4 de la Secuencia Final (Zona 35 — Pasarela Superior): Accionar el control accesible del peine
        /// superior para elevar el telón intermedio falso y descubrir el arco oculto de la escenografía.
        /// </summary>
        InteractivePhase4CatwalkRigging,

        /// <summary>
        /// Fase 5 / Clímax Final (Zona 32 — Centro del Escenario Principal): Ejecutar la secuencia completa
        /// de 'El Lago de las Sombras' y desencadenar la revelación narrativa final sobre Elena Vesper.
        /// </summary>
        InteractivePhase5FinalStageClimax
    }

    /// <summary>
    /// Controlador integral de anomalías psicológicas y de la Secuencia Final Cooperativa de 5 fases
    /// para la SECCIÓN 7: TEATRO PRINCIPAL — CLÍMAX (Zonas 31 a 35).
    /// </summary>
    public class TheatreClimaxController : MonoBehaviour, IInteractable
    {
        // Estado compartido de las 5 fases de la Secuencia Final en el Teatro Principal
        public static bool Phase1MusicConfirmed { get; private set; }
        public static bool Phase2PositionsAligned { get; private set; }
        public static bool Phase3LightingActivated { get; private set; }
        public static bool Phase4CatwalkCurtainRaised { get; private set; }
        public static bool Phase5ClimaxCompleted { get; private set; }

        [SerializeField] private TheatreClimaxNodeType nodeType;
        [SerializeField] private Transform primaryMovingTarget;
        [SerializeField] private Transform secondaryMovingTarget;
        [SerializeField] private Light controlledSpotlight;
        [SerializeField] private Light secondarySpotlight;
        [SerializeField] private GameObject hiddenRevealObject;

        private AudioSource audioSource;
        private Vector3 initialPosPrimary;
        private Quaternion initialRotPrimary;
        private float baseLightIntensity = 3.0f;
        private float timer;

        public void Configure(
            TheatreClimaxNodeType type,
            Transform primaryTarget = null,
            Transform secondaryTarget = null,
            Light spot1 = null,
            Light spot2 = null,
            GameObject revealObj = null)
        {
            nodeType = type;
            primaryMovingTarget = primaryTarget;
            secondaryMovingTarget = secondaryTarget;
            controlledSpotlight = spot1;
            secondarySpotlight = spot2;
            hiddenRevealObject = revealObj;
        }

        private void Awake()
        {
            if (primaryMovingTarget != null)
            {
                initialPosPrimary = primaryMovingTarget.localPosition;
                initialRotPrimary = primaryMovingTarget.localRotation;
            }

            if (controlledSpotlight != null)
            {
                baseLightIntensity = controlledSpotlight.intensity;
            }

            if (hiddenRevealObject != null &&
                (nodeType == TheatreClimaxNodeType.InteractivePhase2StagePositions ||
                 nodeType == TheatreClimaxNodeType.InteractivePhase3LightingConsole ||
                 nodeType == TheatreClimaxNodeType.InteractivePhase5FinalStageClimax))
            {
                hiddenRevealObject.SetActive(false);
            }

            SetupProceduralAudio();
        }

        private void SetupProceduralAudio()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 2.5f;
            audioSource.maxDistance = 28.0f;
            audioSource.playOnAwake = false;

            switch (nodeType)
            {
                case TheatreClimaxNodeType.AnomalyAuditoriumSpectatorsAndApplause:
                    audioSource.clip = CreateDistantApplauseCutoffClip();
                    break;

                case TheatreClimaxNodeType.AnomalyOrchestraPitPhantomMusic:
                case TheatreClimaxNodeType.InteractivePhase1OrchestraStand:
                    audioSource.clip = CreateOrchestraCelloResolutionClip();
                    break;

                case TheatreClimaxNodeType.InteractivePhase3LightingConsole:
                case TheatreClimaxNodeType.InteractivePhase4CatwalkRigging:
                    audioSource.clip = CreateStageSwitchAndPulleyClip();
                    break;

                default:
                    audioSource.clip = CreateClimaxSwanThemeClip();
                    break;
            }
        }

        private void Update()
        {
            Transform player = GetClosestPlayer(out float dist, out bool lookingAtMe);
            if (player == null) return;

            switch (nodeType)
            {
                case TheatreClimaxNodeType.AnomalyAuditoriumSpectatorsAndApplause:
                    UpdateAuditoriumAnomaly(dist);
                    break;

                case TheatreClimaxNodeType.AnomalyStageBallerinaSilhouette:
                    UpdateStageBallerinaAnomaly(dist);
                    break;

                case TheatreClimaxNodeType.AnomalyOrchestraPitPhantomMusic:
                    UpdateOrchestraPitAnomaly(dist);
                    break;

                case TheatreClimaxNodeType.AnomalyLightingBoothAutonomousSpot:
                    UpdateLightingBoothAnomaly(dist);
                    break;

                case TheatreClimaxNodeType.AnomalyCatwalkPerspectiveEcho:
                    UpdateCatwalkAnomaly(dist);
                    break;

                case TheatreClimaxNodeType.InteractivePhase4CatwalkRigging:
                    UpdateCatwalkCurtainAnimation();
                    break;
            }
        }

        private void UpdateAuditoriumAnomaly(float dist)
        {
            // Desde el escenario o lejos (> 10.5m) se distinguen figuras inmóviles en las butacas;
            // al acercarse (< 8.5m) las filas están completamente vacías.
            bool showSeatedFigures = dist > 9.5f && dist < 36.0f;
            if (primaryMovingTarget != null && primaryMovingTarget.gameObject.activeSelf != showSeatedFigures)
            {
                primaryMovingTarget.gameObject.SetActive(showSeatedFigures);
            }

            // Aplausos muy lejanos que se interrumpen abruptamente al adentrarse en el pasillo central
            timer += Time.deltaTime;
            if (dist > 10.0f && dist < 26.0f && timer > 14.0f)
            {
                timer = 0f;
                if (!audioSource.isPlaying)
                {
                    audioSource.volume = 0.38f;
                    audioSource.Play();
                }
            }
            else if (dist < 7.5f && audioSource.isPlaying)
            {
                // Se cortan en seco al acercarse a las butacas
                audioSource.Stop();
            }
        }

        private void UpdateStageBallerinaSilhouette(float dist)
        {
            UpdateStageBallerinaAnomaly(dist);
        }

        private void UpdateStageBallerinaAnomaly(float dist)
        {
            // La silueta femenina aparece al fondo del escenario bajo la luz teatral cuando los jugadores
            // la observan desde más de 6.2m, y desaparece al acercarse o al cambiar la iluminación.
            bool shouldAppear = dist > 6.2f && dist < 34.0f && !Phase5ClimaxCompleted;
            if (primaryMovingTarget != null)
            {
                primaryMovingTarget.gameObject.SetActive(shouldAppear);
                if (shouldAppear)
                {
                    float gracefulTurn = Mathf.Sin(Time.time * 1.1f) * 10f;
                    primaryMovingTarget.localRotation = initialRotPrimary * Quaternion.Euler(0f, gracefulTurn, 0f);
                }
            }
        }

        private void UpdateOrchestraPitAnomaly(float dist)
        {
            timer += Time.deltaTime;
            if (dist > 2.2f && dist < 12.0f && timer > 11.0f)
            {
                timer = 0f;
                if (!audioSource.isPlaying)
                {
                    audioSource.volume = 0.44f;
                    audioSource.Play();
                }
            }
        }

        private void UpdateLightingBoothAnomaly(float dist)
        {
            // Si aún no se ha activado la Fase 3, un foco se enciende brevemente solo cada cierto tiempo
            if (Phase3LightingActivated)
            {
                if (controlledSpotlight != null) controlledSpotlight.intensity = baseLightIntensity * 1.35f;
                if (secondarySpotlight != null) secondarySpotlight.intensity = baseLightIntensity * 1.25f;
                if (hiddenRevealObject != null && !hiddenRevealObject.activeSelf) hiddenRevealObject.SetActive(true);
                return;
            }

            bool pulseOn = Mathf.Sin(Time.time * 0.9f) > 0.62f;
            if (controlledSpotlight != null)
            {
                controlledSpotlight.intensity = pulseOn ? baseLightIntensity : 0.15f;
            }
            if (hiddenRevealObject != null)
            {
                hiddenRevealObject.SetActive(pulseOn);
            }
        }

        private void UpdateCatwalkAnomaly(float dist)
        {
            if (primaryMovingTarget != null && dist < 18.0f)
            {
                // Desde la altura de la pasarela, la figura inferior parece desplazarse suavemente entre las marcas
                float shiftX = Mathf.Sin(Time.time * 0.75f) * 1.8f;
                primaryMovingTarget.localPosition = initialPosPrimary + new Vector3(shiftX, 0f, 0f);
            }
        }

        private void UpdateCatwalkCurtainAnimation()
        {
            if (!Phase4CatwalkCurtainRaised || primaryMovingTarget == null) return;

            // Eleva suavemente el telón intermedio falso hacia el peine superior para descubrir el Arco del Lago
            Vector3 targetPos = initialPosPrimary + new Vector3(0f, 3.6f, 0f);
            primaryMovingTarget.localPosition = Vector3.MoveTowards(
                primaryMovingTarget.localPosition,
                targetPos,
                Time.deltaTime * 1.5f);
        }

        public string Id => $"theatre_climax_{nodeType}_{gameObject.name}";

        public string Prompt
        {
            get
            {
                switch (nodeType)
                {
                    case TheatreClimaxNodeType.InteractivePhase1OrchestraStand:
                        return Phase1MusicConfirmed
                            ? "[E] Revisar Partitura del Director (Fase 1 Completada: Cadencia en Do Natural)"
                            : "[E] Ordenar fragmentos orquestales en el Atril del Director (Fase 1 — Música)";

                    case TheatreClimaxNodeType.InteractivePhase2StagePositions:
                        return Phase2PositionsAligned
                            ? "[E] Examinar Marcas de Posición Escénica (Fase 2 Completada: Posiciones de Elena y Mara)"
                            : "[E] Reconstruir posiciones de Elena y Mara en las marcas del escenario (Fase 2 — Escena)";

                    case TheatreClimaxNodeType.InteractivePhase3LightingConsole:
                        return Phase3LightingActivated
                            ? "[E] Consola de Iluminación Activa (Fase 3 Completada: Focos de Revelación Encendidos)"
                            : "[E] Activar secuencia de focos en la Consola de Iluminación (Fase 3 — Cabina de Luces)";

                    case TheatreClimaxNodeType.InteractivePhase4CatwalkRigging:
                        return Phase4CatwalkCurtainRaised
                            ? "[E] Control Superior de Suspensión (Fase 4 Completada: Telón Falso Elevado)"
                            : "[E] Accionar control de suspensión en la Pasarela Superior (Fase 4 — Telón del Lago)";

                    case TheatreClimaxNodeType.InteractivePhase5FinalStageClimax:
                        return Phase5ClimaxCompleted
                            ? "[E] Revivir la Revelación Final de 'El Lago de las Sombras' (Clímax Completado)"
                            : "[E] Ejecutar Secuencia Final de 'El Lago de las Sombras' en el Centro del Escenario";

                    default:
                        return string.Empty;
                }
            }
        }

        public bool CanInteract(GameObject player)
        {
            return nodeType == TheatreClimaxNodeType.InteractivePhase1OrchestraStand ||
                   nodeType == TheatreClimaxNodeType.InteractivePhase2StagePositions ||
                   nodeType == TheatreClimaxNodeType.InteractivePhase3LightingConsole ||
                   nodeType == TheatreClimaxNodeType.InteractivePhase4CatwalkRigging ||
                   nodeType == TheatreClimaxNodeType.InteractivePhase5FinalStageClimax;
        }

        public void Interact(GameObject player)
        {
            switch (nodeType)
            {
                case TheatreClimaxNodeType.InteractivePhase1OrchestraStand:
                    ExecutePhase1Orchestra();
                    break;

                case TheatreClimaxNodeType.InteractivePhase2StagePositions:
                    ExecutePhase2StagePositions();
                    break;

                case TheatreClimaxNodeType.InteractivePhase3LightingConsole:
                    ExecutePhase3LightingConsole();
                    break;

                case TheatreClimaxNodeType.InteractivePhase4CatwalkRigging:
                    ExecutePhase4CatwalkRigging();
                    break;

                case TheatreClimaxNodeType.InteractivePhase5FinalStageClimax:
                    ExecutePhase5FinalClimax();
                    break;
            }
        }

        private void ExecutePhase1Orchestra()
        {
            Phase1MusicConfirmed = true;
            if (!audioSource.isPlaying)
            {
                audioSource.volume = 0.65f;
                audioSource.Play();
            }

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "Fase 1 Completada — Partitura de Dirección en el Foso de Orquesta",
                    "Al colocar en orden cronológico los fragmentos del foso y compararlos con la grabación de la Cabina Acústica y el violonchelo confiscado:\n\n" +
                    "• **Compases 44–47**: El cuerpo de baile se retira hacia los bastidores laterales.\n" +
                    "• **Compás 48 (Señal Verdadera)**: El violonchelo solista (cuyo espacio junto al atril fue vaciado el 13 de octubre) debía ejecutar un **Do Natural sostenido**, indicando a Elena que cruzara el Arco del Lago hacia el proscenio.\n" +
                    "• **La Trampa**: Isadora Vale hizo sustituir ese compás por un acorde seco de metales para dar la orden a la Cabina de Iluminación de apagar el centro del escenario mientras bajaba el telón de contrapeso.\n\n" +
                    "La secuencia musical original ha quedado preparada para el clímax en el escenario.",
                    GetClimaxProgressSummary());
            }
        }

        private void ExecutePhase2StagePositions()
        {
            Phase2PositionsAligned = true;
            if (hiddenRevealObject != null) hiddenRevealObject.SetActive(true);
            if (!audioSource.isPlaying)
            {
                audioSource.volume = 0.55f;
                audioSource.Play();
            }

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "Fase 2 Completada — Reconstrucción de Posiciones en el Escenario",
                    "Comparas las anotaciones del Camerino de Elena y el Camerino de Mara con las marcas reales grabadas sobre las tablas del escenario:\n\n" +
                    "1. **Marca Central de Proscenio (Elena Vesper)**: Donde Elena debía interpretar el cierre según la partitura original.\n" +
                    "2. **Marca Lateral Derecha (Mara Bell)**: Añadida con pintura fresca la víspera del estreno para que Mara entrara desde el bastidor cuando se apagara la luz central.\n" +
                    "3. **Marca Oculta tras el Arco del Lago**: Junto al borde posterior del escenario aparece el rastro de resina de las zapatillas de Elena, exactamente debajo del recorrido del telón de contrapeso.\n\n" +
                    "Las posiciones escénicas han quedado reconstruidas.",
                    GetClimaxProgressSummary());
            }
        }

        private void ExecutePhase3LightingConsole()
        {
            Phase3LightingActivated = true;
            if (controlledSpotlight != null) controlledSpotlight.intensity = baseLightIntensity * 1.4f;
            if (secondarySpotlight != null) secondarySpotlight.intensity = baseLightIntensity * 1.3f;
            if (hiddenRevealObject != null) hiddenRevealObject.SetActive(true);

            if (!audioSource.isPlaying)
            {
                audioSource.volume = 0.65f;
                audioSource.Play();
            }

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "Fase 3 Completada — Consola de Iluminación Activada",
                    "Accionas los reguladores físicos de la consola siguiendo el orden de las pistas musicales:\n\n" +
                    "• **Circuito I (Proscenio Frío)** -> **Circuito II (Espejo del Lago)** -> **Circuito III (Contraluz del Arco Trasero)**.\n\n" +
                    "A través del gran ventanal de la cabina observas cómo los haces de luz atraviesan el polvo del teatro e iluminan la parte trasera del escenario: desde aquí arriba se distingue claramente que detrás del telón intermedio hay una segunda estructura arquitectónica oculta que no podía verse desde el patio de butacas.\n\n" +
                    "Sube o avisa a tu compañero en la **Pasarela Superior (Zona 35)** para elevar el telón falso.",
                    GetClimaxProgressSummary());
            }
        }

        private void ExecutePhase4CatwalkRigging()
        {
            Phase4CatwalkCurtainRaised = true;
            if (!audioSource.isPlaying)
            {
                audioSource.volume = 0.70f;
                audioSource.Play();
            }

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "Fase 4 Completada — Perspectiva de la Pasarela Superior y Elevación del Telón",
                    "Desde la pasarela metálica superior examinas los puntos de suspensión que conectan con la Sala de Poleas y accionas el embrague manual de la **Bambalina Central del Lago**.\n\n" +
                    "Las poleas superiores giran y el pesado telón intermedio se eleva lentamente sobre el escenario, dejando al descubierto el **Arco Original del Lago de las Sombras** y el punto exacto donde Elena quedó atrapada cuando bloquearon el mecanismo durante la función.\n\n" +
                    "Con el escenario revelado desde las alturas, dirígete al **Centro del Escenario Principal (Zona 32)** para ejecutar la secuencia final.",
                    GetClimaxProgressSummary());
            }
        }

        private void ExecutePhase5FinalClimax()
        {
            // Si aún falta alguna fase, damos retroalimentación clara pero permitimos también completar la reconstrucción
            // si ya han explorado o si deciden activar la convergencia final en el escenario.
            bool allFourPrepared = Phase1MusicConfirmed && Phase2PositionsAligned && Phase3LightingActivated && Phase4CatwalkCurtainRaised;
            Phase1MusicConfirmed = true;
            Phase2PositionsAligned = true;
            Phase3LightingActivated = true;
            Phase4CatwalkCurtainRaised = true;
            Phase5ClimaxCompleted = true;

            if (hiddenRevealObject != null) hiddenRevealObject.SetActive(true);
            if (!audioSource.isPlaying)
            {
                audioSource.volume = 0.85f;
                audioSource.Play();
            }

            if (DocumentUI.Instance != null)
            {
                string intro = allFourPrepared
                    ? "Con las cuatro fases sincronizadas (Foso de Orquesta, Marcas del Escenario, Cabina de Iluminación y Pasarela Superior), el teatro entero responde al unísono:\n\n"
                    : "Reúnes las evidencias acumuladas en el centro del proscenio y activas la reconstrucción completa del último acto de «El Lago de las Sombras»:\n\n";

                DocumentUI.Instance.ShowDocument(
                    "CLÍMAX — LA VERDAD DE «EL LAGO DE LAS SOMBRAS» (14 DE OCTUBRE DE 1924)",
                    intro +
                    "1. **La Música Verdadera Resuena**: Desde el foso de orquesta se eleva la cadencia original en Mi menor culminando en el **Do Natural** prohibido por Isadora Vale.\n" +
                    "2. **La Escena Oculta**: Bajo los focos cenitales de la cabina y con el telón falso alzado desde la pasarela superior, queda revelado el espacio sellado tras el Arco del Lago.\n" +
                    "3. **La Contradicción Definitiva**: Elena Vesper nunca huyó de la Academia. La noche del 14 de octubre, vestida con el traje del Cisne Negro, intentó cruzar el Arco del Lago durante el compás 48 para revelar que la obra y los registros habían sido falsificados por Isadora Vale. En ese instante exacto, cortaron las luces desde la cabina y dejaron caer el bastidor de contrapeso, ocultando el accidente tras un telón ciego mientras Mara Bell era obligada a terminar la función en el proscenio.\n\n" +
                    "En el centro del arco recién descubierto brilla el último testimonio firmado aquella noche, preparando el desenlace definitivo de *Último Acto*.",
                    "Secuencia Final Completada (5/5 Fases) — El misterio del Teatro Principal ha sido revelado");
            }
        }

        private static string GetClimaxProgressSummary()
        {
            int count = (Phase1MusicConfirmed ? 1 : 0) +
                        (Phase2PositionsAligned ? 1 : 0) +
                        (Phase3LightingActivated ? 1 : 0) +
                        (Phase4CatwalkCurtainRaised ? 1 : 0) +
                        (Phase5ClimaxCompleted ? 1 : 0);

            return $"Progreso de la Secuencia Final del Teatro: {count}/5 fases activadas " +
                   $"[1.Foso:{(Phase1MusicConfirmed ? "Sí" : "Pendiente")} | " +
                   $"2.Marcas:{(Phase2PositionsAligned ? "Sí" : "Pendiente")} | " +
                   $"3.Luces:{(Phase3LightingActivated ? "Sí" : "Pendiente")} | " +
                   $"4.Pasarela:{(Phase4CatwalkCurtainRaised ? "Sí" : "Pendiente")}]";
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
                    isLookingAtThis = Vector3.Dot(c.transform.forward, toObj) > 0.50f;
                }
            }

            return closest;
        }

        private static AudioClip CreateDistantApplauseCutoffClip()
        {
            int sampleRate = 22050;
            int samples = sampleRate * 3;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                // Aplauso lejano reverberante que crece y se corta abruptamente a los 2.4s
                float env = t < 2.4f ? Mathf.Clamp01(t / 0.8f) : Mathf.Exp(-(t - 2.4f) * 35f);
                float clapMod = 0.6f + 0.4f * Mathf.Sin(t * 28f) * Mathf.Cos(t * 19f);
                float noise = (Mathf.PerlinNoise(t * 140f, 2.3f) * 2f - 1f) * 0.18f;
                data[i] = noise * clapMod * env;
            }
            AudioClip clip = AudioClip.Create("Auditorium_DistantApplause", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateOrchestraCelloResolutionClip()
        {
            int sampleRate = 22050;
            int samples = sampleRate * 3;
            float[] data = new float[samples];
            // Do Natural grave (130.81 Hz) + Mi (164.81 Hz) + Si (246.94 Hz)
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(Mathf.Clamp01(t / 3.0f) * Mathf.PI);
                float c3 = Mathf.Sin(2f * Mathf.PI * 130.81f * t) * 0.28f;
                float c4 = Mathf.Sin(2f * Mathf.PI * 261.63f * t) * 0.14f;
                float e3 = Mathf.Sin(2f * Mathf.PI * 164.81f * t) * 0.12f;
                data[i] = (c3 + c4 + e3) * env;
            }
            AudioClip clip = AudioClip.Create("OrchestraPit_CelloDoNatural", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateStageSwitchAndPulleyClip()
        {
            int sampleRate = 22050;
            int samples = sampleRate * 2;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float switchClack = Mathf.Exp(-Mathf.Pow((t - 0.15f) * 28f, 2f)) * Mathf.Sin(2f * Mathf.PI * 240f * t) * 0.45f;
                float hum = Mathf.Sin(2f * Mathf.PI * 100f * t) * Mathf.Exp(-t * 1.4f) * 0.16f;
                data[i] = switchClack + hum;
            }
            AudioClip clip = AudioClip.Create("Theatre_ConsoleAndRigging", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateClimaxSwanThemeClip()
        {
            int sampleRate = 22050;
            int samples = sampleRate * 4;
            float[] data = new float[samples];
            // Cadencia completa: Mi4 (329.63) -> Sol4 (392.00) -> Si4 (493.88) -> Do5 Natural (523.25) -> Mi5 (659.25)
            float[] notes = { 329.63f, 392.00f, 493.88f, 523.25f, 659.25f };
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                int idx = Mathf.Clamp((int)(t / 0.8f), 0, notes.Length - 1);
                float localT = t - idx * 0.8f;
                float env = Mathf.Sin(Mathf.Clamp01(localT / 0.8f) * Mathf.PI);
                float freq = notes[idx];
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.26f +
                             Mathf.Sin(2f * Mathf.PI * (freq * 0.5f) * t) * 0.18f;
                data[i] = tone * env;
            }
            AudioClip clip = AudioClip.Create("Climax_SwanLakeResolution", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
