using UnityEngine;
using UltimoActo.Interaction;
using UltimoActo.UI;

namespace UltimoActo.Narrative
{
    public enum SecretZoneNodeType
    {
        /// <summary>
        /// Zona 36 (Entrada a la Escalera Oculta): Panel móvil disimulado detrás de la estructura lateral
        /// del escenario que se desplaza al descubrir el mecanismo referenciado en las pistas B-00.
        /// </summary>
        InteractiveSecretPanelEntrance,

        /// <summary>
        /// Zona 36 (Escalera Oculta): Anomalía auditiva al descender — vibración lejana y apagada de la música
        /// del teatro y el sonido puntual de una puerta cerrándose desde el nivel inferior.
        /// </summary>
        AnomalyHiddenStairDoorShut,

        /// <summary>
        /// Zona 37 (Depósito Clausurado): Una tela protectora se mueve ligeramente cuando los jugadores pasan
        /// junto a ella, y una caja cambia de posición después de abandonar brevemente la habitación.
        /// </summary>
        AnomalySealedDepotClothAndCrate,

        /// <summary>
        /// Zona 37 (Depósito Clausurado): Examinar la caja metálica cerrada y el panel de la cavidad del muro
        /// para encontrar los documentos personales trasladados y desbloquear la Sala de Archivos Prohibidos.
        /// </summary>
        InteractiveSealedDepotInvestigation,

        /// <summary>
        /// Zona 38 (Sala de Archivos Prohibidos): Ordenar los documentos por fecha en la mesa central y relacionarlos
        /// con administración, camerinos y archivo musical para abrir el acceso a la Cámara tras el Escenario.
        /// </summary>
        InteractiveForbiddenArchivesTimeline,

        /// <summary>
        /// Zona 39 (Cámara tras el Escenario): Examinar la fotografía original, la carta personal y la partitura
        /// en la caja de madera abierta, activando el fragmento de música original desde el escenario seguido de silencio.
        /// </summary>
        InteractiveBehindStageFinalBox
    }

    /// <summary>
    /// Controlador de interacciones, puzles documentales y anomalías psicológicas para la
    /// SECCIÓN 8: ZONA SECRETA — LA VERDAD OCULTA (Zonas 36 a 39).
    /// </summary>
    public class SecretZoneAnomalyController : MonoBehaviour, IInteractable
    {
        [SerializeField] private SecretZoneNodeType nodeType;
        [SerializeField] private Transform primaryMovingTarget;
        [SerializeField] private Transform secondaryMovingTarget;
        [SerializeField] private WorldDoor linkedDoorToUnlock;
        [SerializeField] private GameObject hiddenRevealObject;

        private AudioSource audioSource;
        private Vector3 initialPosPrimary;
        private Quaternion initialRotPrimary;
        private Vector3 initialPosSecondary;

        private bool isActivated;
        private float animProgress;
        private bool hasTriggeredStairDoorSound;
        private bool hasEnteredDepotOnce;
        private bool hasLeftDepotAfterEntering;
        private bool hasShiftedDepotCrate;

        public void Configure(
            SecretZoneNodeType type,
            Transform primaryTarget = null,
            Transform secondaryTarget = null,
            WorldDoor doorToUnlock = null,
            GameObject revealObj = null)
        {
            nodeType = type;
            primaryMovingTarget = primaryTarget;
            secondaryMovingTarget = secondaryTarget;
            linkedDoorToUnlock = doorToUnlock;
            hiddenRevealObject = revealObj;
        }

        private void Awake()
        {
            if (primaryMovingTarget != null)
            {
                initialPosPrimary = primaryMovingTarget.localPosition;
                initialRotPrimary = primaryMovingTarget.localRotation;
            }

            if (secondaryMovingTarget != null)
            {
                initialPosSecondary = secondaryMovingTarget.localPosition;
            }

            if (hiddenRevealObject != null &&
                (nodeType == SecretZoneNodeType.InteractiveSealedDepotInvestigation ||
                 nodeType == SecretZoneNodeType.InteractiveForbiddenArchivesTimeline ||
                 nodeType == SecretZoneNodeType.InteractiveBehindStageFinalBox))
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
            audioSource.minDistance = 1.8f;
            audioSource.maxDistance = 18.0f;
            audioSource.playOnAwake = false;

            switch (nodeType)
            {
                case SecretZoneNodeType.InteractiveSecretPanelEntrance:
                case SecretZoneNodeType.AnomalyHiddenStairDoorShut:
                    audioSource.clip = CreateMuffledVibrationAndDoorShutClip();
                    break;

                case SecretZoneNodeType.InteractiveBehindStageFinalBox:
                    audioSource.clip = CreateDistantStageOriginalMusicThenSilenceClip();
                    break;

                default:
                    audioSource.clip = CreateArchivalPaperAndLatchClip();
                    break;
            }
        }

        private void Update()
        {
            Transform player = GetClosestPlayer(out float dist);
            if (player == null) return;

            switch (nodeType)
            {
                case SecretZoneNodeType.InteractiveSecretPanelEntrance:
                    UpdateSecretPanelSlideAnimation();
                    break;

                case SecretZoneNodeType.AnomalyHiddenStairDoorShut:
                    UpdateHiddenStairAnomaly(dist);
                    break;

                case SecretZoneNodeType.AnomalySealedDepotClothAndCrate:
                    UpdateSealedDepotAnomalies(dist);
                    break;
            }
        }

        private void UpdateSecretPanelSlideAnimation()
        {
            if (!isActivated || primaryMovingTarget == null) return;

            if (animProgress < 1f)
            {
                animProgress = Mathf.MoveTowards(animProgress, 1f, Time.deltaTime * 0.85f);
                // El panel móvil se retrae lateralmente dejando libre el paso hacia la Escalera Oculta
                primaryMovingTarget.localPosition = Vector3.Lerp(
                    initialPosPrimary,
                    initialPosPrimary + new Vector3(0f, 0f, -2.15f),
                    animProgress);
            }
        }

        private void UpdateHiddenStairAnomaly(float dist)
        {
            // Al descender por el tramo intermedio de la escalera oculta, suena una puerta cerrándose abajo
            // seguida de una vibración muy lejana de la música del teatro
            if (!hasTriggeredStairDoorSound && dist < 4.5f)
            {
                hasTriggeredStairDoorSound = true;
                if (!audioSource.isPlaying)
                {
                    audioSource.volume = 0.55f;
                    audioSource.Play();
                }
            }
            else if (hasTriggeredStairDoorSound && dist > 14.0f)
            {
                // Permite volver a percibir el efecto si regresan mucho después
                hasTriggeredStairDoorSound = false;
            }
        }

        private void UpdateSealedDepotAnomalies(float dist)
        {
            // 1. Una tela protectora se mueve ligeramente cuando los jugadores pasan junto a ella (< 3.8m)
            if (primaryMovingTarget != null)
            {
                float amp = (dist < 3.8f) ? 7.5f : 0.6f;
                float sway = Mathf.Sin(Time.time * 2.6f) * amp;
                primaryMovingTarget.localRotation = initialRotPrimary * Quaternion.Euler(sway, 0f, sway * 0.4f);
            }

            // 2. Una caja cambia de posición después de abandonar brevemente el depósito y regresar
            if (dist < 4.5f)
            {
                if (!hasEnteredDepotOnce)
                {
                    hasEnteredDepotOnce = true;
                }
                else if (hasLeftDepotAfterEntering && !hasShiftedDepotCrate)
                {
                    hasShiftedDepotCrate = true;
                    if (secondaryMovingTarget != null)
                    {
                        secondaryMovingTarget.localPosition = initialPosSecondary + new Vector3(-0.55f, 0f, 0.35f);
                        secondaryMovingTarget.localRotation = Quaternion.Euler(0f, 28f, 0f);
                    }
                }
            }
            else if (hasEnteredDepotOnce && dist > 8.5f)
            {
                hasLeftDepotAfterEntering = true;
            }
        }

        public string Id => $"secret_zone_{nodeType}_{gameObject.name}";

        public string Prompt
        {
            get
            {
                switch (nodeType)
                {
                    case SecretZoneNodeType.InteractiveSecretPanelEntrance:
                        return isActivated
                            ? "[E] Panel móvil abierto (Acceso a 36. Escalera Oculta — Archivo B-00)"
                            : "[E] Accionar pestillo oculto del panel «B-00» (Descubrir Escalera Oculta)";

                    case SecretZoneNodeType.InteractiveSealedDepotInvestigation:
                        return isActivated
                            ? "[E] Revisar Caja Metálica y Cavidad del Muro (Sala de Archivos Prohibidos abierta)"
                            : "[E] Examinar Caja Metálica Cerrada y Cavidad del Muro (Abrir Archivos Prohibidos)";

                    case SecretZoneNodeType.InteractiveForbiddenArchivesTimeline:
                        return isActivated
                            ? "[E] Revisar Cronología de los Archivos Prohibidos (Cámara tras el Escenario abierta)"
                            : "[E] Ordenar correspondencia y expedientes por fecha (Reconstruir la Verdad Oculta)";

                    case SecretZoneNodeType.InteractiveBehindStageFinalBox:
                        return isActivated
                            ? "[E] Volver a examinar la Caja de Madera Abierta (La Verdad Oculta de 'El Lago de las Sombras')"
                            : "[E] Examinar Fotografía Original, Carta Personal y Partitura en la Caja Abierta";

                    default:
                        return string.Empty;
                }
            }
        }

        public bool CanInteract(GameObject player)
        {
            return nodeType == SecretZoneNodeType.InteractiveSecretPanelEntrance ||
                   nodeType == SecretZoneNodeType.InteractiveSealedDepotInvestigation ||
                   nodeType == SecretZoneNodeType.InteractiveForbiddenArchivesTimeline ||
                   nodeType == SecretZoneNodeType.InteractiveBehindStageFinalBox;
        }

        public void Interact(GameObject player)
        {
            switch (nodeType)
            {
                case SecretZoneNodeType.InteractiveSecretPanelEntrance:
                    ExecuteSecretPanelOpen();
                    break;

                case SecretZoneNodeType.InteractiveSealedDepotInvestigation:
                    ExecuteSealedDepotInvestigation();
                    break;

                case SecretZoneNodeType.InteractiveForbiddenArchivesTimeline:
                    ExecuteForbiddenArchivesTimeline();
                    break;

                case SecretZoneNodeType.InteractiveBehindStageFinalBox:
                    ExecuteBehindStageFinalBox();
                    break;
            }
        }

        private void ExecuteSecretPanelOpen()
        {
            isActivated = true;
            if (!audioSource.isPlaying)
            {
                audioSource.volume = 0.65f;
                audioSource.Play();
            }

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "Mecanismo Oculto Activado — Acceso a la Escalera Oculta (Zona 36)",
                    "Al unir las referencias encontradas en Administración, el Archivo Musical, el Almacén de Decorados y la Sala de Poleas sobre el registro **«B-00»**, presionas el resorte disimulado tras la moldura de madera.\n\n" +
                    "El panel móvil se desplaza lentamente, dejando al descubierto una estrecha escalera de ladrillo envejecido y yeso deteriorado que desciende por el interior del muro antiguo del teatro hacia un nivel subterráneo que nunca estuvo al alcance de las alumnas ni del público.",
                    "SECCIÓN 8 DESBLOQUEADA: Zona Secreta — La Verdad Oculta");
            }
        }

        private void ExecuteSealedDepotInvestigation()
        {
            isActivated = true;
            if (hiddenRevealObject != null) hiddenRevealObject.SetActive(true);
            if (linkedDoorToUnlock != null)
            {
                linkedDoorToUnlock.SetLockState(false);
                if (!linkedDoorToUnlock.IsOpen) linkedDoorToUnlock.ToggleDoor();
            }

            if (!audioSource.isPlaying)
            {
                audioSource.volume = 0.60f;
                audioSource.Play();
            }

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "Depósito Clausurado — La Caja Metálica y la Cavidad del Muro",
                    "Al comparar las etiquetas de inventario parcialmente arrancadas, descubres un patrón de fechas imposible:\n\n" +
                    "• Varias cajas con pertenencias del camerino de Elena y fragmentos de escenografía tienen sello de entrada en este depósito a las **23:15 h del 14 de octubre de 1924** —apenas una hora después de interrumpirse la representación—.\n" +
                    "• Dentro de la **cavidad oculta tras el panel de madera** encuentras la llave y el registro de la **Caja Metálica Personal**, donde el antiguo archivero anotó:\n" +
                    "  «Todo lo que la Directora ordenó borrar de los expedientes oficiales fue guardado detrás de este depósito, en la Sala de Archivos Prohibidos».\n\n" +
                    "La puerta contigua hacia la **Sala de Archivos Prohibidos (Zona 38)** ha quedado abierta.",
                    "Acceso abierto hacia 38. Sala de Archivos Prohibidos");
            }
        }

        private void ExecuteForbiddenArchivesTimeline()
        {
            isActivated = true;
            if (hiddenRevealObject != null) hiddenRevealObject.SetActive(true);
            if (linkedDoorToUnlock != null)
            {
                linkedDoorToUnlock.SetLockState(false);
                if (!linkedDoorToUnlock.IsOpen) linkedDoorToUnlock.ToggleDoor();
            }

            if (!audioSource.isPlaying)
            {
                audioSource.volume = 0.65f;
                audioSource.Play();
            }

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "Reconstrucción Cronológica — El Secreto Administrativo de la Academia",
                    "Al ordenar por fecha las cartas breves, los informes y los expedientes de la mesa central, emerge una verdad mucho más humana, compleja y ambigua que la versión oficial:\n\n" +
                    "1. **5 de Octubre de 1924 (El Patronato y la Clausura)**: La academia estaba al borde de la ruina financiera. El mecenas principal exigió que su protegida, Mara Bell, protagonizara el acto final del estreno o retiraría toda la financiación.\n" +
                    "2. **11 de Octubre de 1924 (La Negativa de Mara y el Pacto con Elena)**: Contra lo que parecía en un principio, Mara NO quería robarle el puesto a Elena. En una carta privada, Mara intentó renunciar, pero Isadora Vale la amenazó con expulsar a ambas.\n" +
                    "3. **13 de Octubre de 1924 (El Plan de Elena y Mara)**: Elena y Mara acordaron en secreto cambiar sus posiciones en el compás 48 usando el paso del Arco del Lago para que Elena pudiera terminar la obra. Isadora descubrió parte del plan y ordenó bloquear el decorado del fondo, sin saber que Elena ya estaba dentro de la estructura cuando bajó el contrapeso.\n" +
                    "4. **La Última Página Arrancada**: El expediente final indica que la única prueba física de lo ocurrido aquella noche fue guardada en la **Cámara tras el Escenario (Zona 39)**.",
                    "Acceso desbloqueado hacia 39. Cámara tras el Escenario");
            }
        }

        private void ExecuteBehindStageFinalBox()
        {
            isActivated = true;
            if (hiddenRevealObject != null) hiddenRevealObject.SetActive(true);

            // Reproduce el fragmento de música original llegando desde el escenario seguido de un silencio absoluto
            if (!audioSource.isPlaying)
            {
                audioSource.volume = 0.80f;
                audioSource.Play();
            }

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "LA VERDAD OCULTA — La Cámara tras el Escenario (Revelación Secreta)",
                    "En el interior de la caja de madera cubierta de polvo descansan tres evidencias que nunca llegaron a los registros oficiales:\n\n" +
                    "1. **Fotografía Original de la Última Representación**: Tomada desde los bastidores en el compás 48. Muestra a Mara Bell en el proscenio extendiendo la mano hacia el Arco del Lago con expresión de horror mientras el telón de contrapeso cae, y detrás de la mirilla de esta misma cámara se distingue la silueta de alguien que observaba la función en silencio.\n" +
                    "2. **Partitura con Anotaciones Inéditas a Dos Manos**: Escrita conjuntamente por Elena Vesper y Mara Bell. Demuestra que el verdadero final de *El Lago de las Sombras* fue concebido como un dúo entre el Cisne Blanco y el Cisne Negro, no como una sustitución.\n" +
                    "3. **Carta Personal sin Enviar (15 de Octubre de 1924)**:\n" +
                    "   «Nos hicieron creer al mundo que Elena huyó por orgullo, porque admitir lo que ocurrió tras el telón habría destruido la academia y condenado a quienes callamos por miedo. Pero alguien conservó esta cámara abierta hacia el escenario... y la partitura aún tiene anotaciones recientes».\n\n" +
                    "Desde el otro lado de la mirilla, en el escenario vacío, resuena un breve compás de la música original... y luego, un silencio absoluto.",
                    "Has descubierto el secreto completo de ÚLTIMO ACTO (Sección 8 Completada)");
            }
        }

        private Transform GetClosestPlayer(out float minDistance)
        {
            minDistance = float.MaxValue;
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
                }
            }

            return closest;
        }

        private static AudioClip CreateMuffledVibrationAndDoorShutClip()
        {
            int sampleRate = 22050;
            int samples = sampleRate * 3;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                // Vibración grave apagada del teatro + golpe seco de una puerta cerrándose abajo en t = 1.6s
                float lowVib = Mathf.Sin(2f * Mathf.PI * 65.4f * t) * Mathf.Exp(-t * 0.7f) * 0.22f;
                float doorThud = Mathf.Exp(-Mathf.Pow((t - 1.6f) * 16f, 2f)) *
                                 (Mathf.Sin(2f * Mathf.PI * 78f * t) * 0.55f + (Mathf.PerlinNoise(t * 80f, 1.1f) * 2f - 1f) * 0.25f);
                data[i] = lowVib + doorThud;
            }
            AudioClip clip = AudioClip.Create("SecretStair_DoorShutBelow", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateArchivalPaperAndLatchClip()
        {
            int sampleRate = 22050;
            int samples = sampleRate * 2;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float rustle = (Mathf.PerlinNoise(t * 110f, 0.5f) * 2f - 1f) * Mathf.Exp(-t * 2.2f) * 0.22f;
                float latch = Mathf.Exp(-Mathf.Pow((t - 0.5f) * 22f, 2f)) * Mathf.Sin(2f * Mathf.PI * 190f * t) * 0.36f;
                data[i] = rustle + latch;
            }
            AudioClip clip = AudioClip.Create("SecretArchive_PaperAndLatch", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateDistantStageOriginalMusicThenSilenceClip()
        {
            int sampleRate = 22050;
            int samples = sampleRate * 5;
            float[] data = new float[samples];
            // Breve fragmento de la música original (0..3.2s) seguido de silencio completo (3.2..5.0s)
            float[] notes = { 329.63f, 392.00f, 493.88f, 523.25f };
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                if (t > 3.2f)
                {
                    data[i] = 0f;
                    continue;
                }
                int idx = Mathf.Clamp((int)(t / 0.8f), 0, notes.Length - 1);
                float localT = t - idx * 0.8f;
                float env = Mathf.Sin(Mathf.Clamp01(localT / 0.8f) * Mathf.PI) * Mathf.Clamp01((3.2f - t) / 0.6f);
                float f = notes[idx];
                data[i] = (Mathf.Sin(2f * Mathf.PI * f * t) * 0.22f + Mathf.Sin(2f * Mathf.PI * (f * 0.5f) * t) * 0.16f) * env;
            }
            AudioClip clip = AudioClip.Create("SecretChamber_MusicThenSilence", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
