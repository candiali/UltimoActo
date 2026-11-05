using System.Collections;
using UnityEngine;
using UltimoActo.Interaction;
using UltimoActo.UI;

namespace UltimoActo.Narrative
{
    public enum MusicArchiveAnomalyType
    {
        LibraryRustlingPages,
        PianoSelfPlayingWrongNote,
        EmptyCornerCelloNote,
        InteractiveGrandPianoSequence,
        InteractiveScoreSortingTable,
        InteractiveReelToReelRecorder
    }

    /// <summary>
    /// Controla tanto las anomalías acústicas/visuales sutiles como las interacciones de investigación
    /// musical (catálogo/secuencia del piano de cola, clasificación de partituras y grabadora de carrete abierto)
    /// de la Sección 5: Archivo Musical — Historia y Sonido (Zonas 21 a 25).
    /// </summary>
    public class MusicArchiveAnomalyController : MonoBehaviour, IInteractable
    {
        [SerializeField] private MusicArchiveAnomalyType mode;
        [SerializeField] private Transform primaryMovingPart;
        [SerializeField] private Transform secondaryMovingPart;
        [SerializeField] private Light flickeringLight;
        [SerializeField] private GameObject hiddenCompartmentReveal;

        private Transform playerCam;
        private AudioSource primaryAudio;
        private AudioSource secondaryBoothAudio; // Segundo punto de escucha en la cabina para cooperativo
        private float cooldownTimer = 3.0f;
        private float baseLightIntensity = 1.6f;
        private bool interactionCompleted = false;
        private bool isPlayingSequence = false;

        public string Id => $"music_archive_{mode}";

        public string Prompt
        {
            get
            {
                switch (mode)
                {
                    case MusicArchiveAnomalyType.InteractiveGrandPianoSequence:
                        return interactionCompleted
                            ? "[E] Volver a reproducir secuencia original en el Piano de Cola (Compartimento abierto)"
                            : "[E] Interpretar secuencia del Catálogo («Mi - Sol - Si - Do Natural - Mi») en el Piano de Cola";

                    case MusicArchiveAnomalyType.InteractiveScoreSortingTable:
                        return interactionCompleted
                            ? "[E] Examinar Partituras Ordenadas (1923–1924) y Sección Reconstruida"
                            : "[E] Ordenar Partituras por Fecha y Comparar Anotaciones de Elena y Mara";

                    case MusicArchiveAnomalyType.InteractiveReelToReelRecorder:
                        return isPlayingSequence
                            ? "[Reproduciendo Cinta Analógica de Ensayo...]"
                            : "[E] Reproducir y Comparar Cintas de Carrete Abierto (Ensayo Original vs. Cinta sin Etiqueta)";

                    default:
                        return string.Empty;
                }
            }
        }

        public void Configure(
            MusicArchiveAnomalyType anomalyMode,
            Transform primaryPart = null,
            Transform secondaryPart = null,
            Light targetLight = null,
            GameObject secretReveal = null)
        {
            mode = anomalyMode;
            primaryMovingPart = primaryPart;
            secondaryMovingPart = secondaryPart;
            flickeringLight = targetLight;
            hiddenCompartmentReveal = secretReveal;
        }

        public bool CanInteract(GameObject player)
        {
            return (mode == MusicArchiveAnomalyType.InteractiveGrandPianoSequence ||
                    mode == MusicArchiveAnomalyType.InteractiveScoreSortingTable ||
                    mode == MusicArchiveAnomalyType.InteractiveReelToReelRecorder) && !isPlayingSequence;
        }

        public void Interact(GameObject player)
        {
            if (!CanInteract(player)) return;

            switch (mode)
            {
                case MusicArchiveAnomalyType.InteractiveGrandPianoSequence:
                    StartCoroutine(PlayCorrectPianoSequenceAndOpenCompartment());
                    break;

                case MusicArchiveAnomalyType.InteractiveScoreSortingTable:
                    StartCoroutine(SortScoresAndRevealOriginalSection());
                    break;

                case MusicArchiveAnomalyType.InteractiveReelToReelRecorder:
                    StartCoroutine(PlayReelToReelComparison());
                    break;
            }
        }

        private void Start()
        {
            if (Camera.main != null) playerCam = Camera.main.transform;
            if (flickeringLight != null) baseLightIntensity = flickeringLight.intensity;
            if (hiddenCompartmentReveal != null && !interactionCompleted)
            {
                hiddenCompartmentReveal.SetActive(false);
            }

            primaryAudio = gameObject.AddComponent<AudioSource>();
            primaryAudio.spatialBlend = 1f;
            primaryAudio.playOnAwake = false;

            switch (mode)
            {
                case MusicArchiveAnomalyType.LibraryRustlingPages:
                    primaryAudio.minDistance = 1.5f;
                    primaryAudio.maxDistance = 12f;
                    primaryAudio.volume = 0.28f;
                    primaryAudio.clip = CreateRustlingPagesClip();
                    break;

                case MusicArchiveAnomalyType.PianoSelfPlayingWrongNote:
                    primaryAudio.minDistance = 2.5f;
                    primaryAudio.maxDistance = 18f;
                    primaryAudio.volume = 0.34f;
                    primaryAudio.clip = CreateDissonantPianoAnomalyClip();
                    break;

                case MusicArchiveAnomalyType.EmptyCornerCelloNote:
                    primaryAudio.minDistance = 2.0f;
                    primaryAudio.maxDistance = 15f;
                    primaryAudio.volume = 0.35f;
                    primaryAudio.clip = CreateBowedCelloNoteClip();
                    break;

                case MusicArchiveAnomalyType.InteractiveGrandPianoSequence:
                    primaryAudio.minDistance = 2.0f;
                    primaryAudio.maxDistance = 16f;
                    primaryAudio.volume = 0.42f;
                    primaryAudio.clip = CreateResolvedPianoSequenceClip();
                    break;

                case MusicArchiveAnomalyType.InteractiveScoreSortingTable:
                    primaryAudio.minDistance = 1.5f;
                    primaryAudio.maxDistance = 10f;
                    primaryAudio.volume = 0.32f;
                    primaryAudio.clip = CreateRustlingPagesClip();
                    break;

                case MusicArchiveAnomalyType.InteractiveReelToReelRecorder:
                    primaryAudio.minDistance = 1.5f;
                    primaryAudio.maxDistance = 12f;
                    primaryAudio.volume = 0.45f;
                    primaryAudio.clip = CreateReelTapeRecordingClip(false);

                    // Segundo altavoz en el extremo opuesto de la cabina para percepción cooperativa
                    if (secondaryMovingPart != null)
                    {
                        secondaryBoothAudio = secondaryMovingPart.gameObject.AddComponent<AudioSource>();
                        secondaryBoothAudio.spatialBlend = 1f;
                        secondaryBoothAudio.playOnAwake = false;
                        secondaryBoothAudio.minDistance = 1.2f;
                        secondaryBoothAudio.maxDistance = 8f;
                        secondaryBoothAudio.volume = 0.40f;
                        secondaryBoothAudio.clip = CreateReelTapeRecordingClip(true);
                    }
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

            switch (mode)
            {
                case MusicArchiveAnomalyType.LibraryRustlingPages:
                    UpdateLibraryPagesAnomaly(dist);
                    break;

                case MusicArchiveAnomalyType.PianoSelfPlayingWrongNote:
                    UpdateSelfPlayingPianoAnomaly(dist);
                    break;

                case MusicArchiveAnomalyType.EmptyCornerCelloNote:
                    UpdateEmptyCornerCelloAnomaly(dist);
                    break;
            }
        }

        private void UpdateLibraryPagesAnomaly(float dist)
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
                return;
            }

            if (dist >= 3.0f && dist <= 11.0f)
            {
                cooldownTimer = 24f;
                if (primaryAudio != null && primaryAudio.clip != null) primaryAudio.Play();
                if (primaryMovingPart != null)
                {
                    primaryMovingPart.localRotation = Quaternion.Euler(0f, Random.Range(-18f, 18f), 0f);
                }
            }
        }

        private void UpdateSelfPlayingPianoAnomaly(float dist)
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
                return;
            }

            // El piano suena solo cuando los jugadores están a media distancia (4.5m a 15m),
            // repitiendo la frase con una nota equivocada, y calla si se acercan demasiado (< 3.2m)
            if (dist >= 4.5f && dist <= 15.0f)
            {
                cooldownTimer = 26f;
                if (primaryAudio != null && primaryAudio.clip != null) primaryAudio.Play();
            }
            else if (dist < 3.0f && primaryAudio != null && primaryAudio.isPlaying)
            {
                primaryAudio.Stop();
            }
        }

        private void UpdateEmptyCornerCelloAnomaly(float dist)
        {
            // Parpadeo sutil e irregular de la lámpara de techo débil del depósito
            if (flickeringLight != null)
            {
                float flicker = Mathf.PerlinNoise(Time.time * 6.5f, 1.3f);
                flickeringLight.intensity = (flicker < 0.22f) ? baseLightIntensity * 0.35f : baseLightIntensity;
            }

            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
                return;
            }

            if (dist >= 3.2f && dist <= 11.5f)
            {
                cooldownTimer = 25f;
                if (primaryAudio != null && primaryAudio.clip != null) primaryAudio.Play();
            }
        }

        private IEnumerator PlayCorrectPianoSequenceAndOpenCompartment()
        {
            isPlayingSequence = true;
            if (primaryAudio != null && primaryAudio.clip != null) primaryAudio.Play();

            // Animación sutil del compartimento secreto del atril del piano de cola
            float elapsed = 0f;
            Quaternion startRot = primaryMovingPart != null ? primaryMovingPart.localRotation : Quaternion.identity;
            Quaternion endRot = Quaternion.Euler(-32f, 0f, 0f);

            while (elapsed < 1.6f)
            {
                elapsed += Time.deltaTime;
                if (primaryMovingPart != null)
                {
                    primaryMovingPart.localRotation = Quaternion.Slerp(startRot, endRot, elapsed / 1.6f);
                }
                yield return null;
            }

            interactionCompleted = true;
            if (hiddenCompartmentReveal != null) hiddenCompartmentReveal.SetActive(true);
            isPlayingSequence = false;

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "Compartimento Secreto del Piano de Cola — El Final Verdadero",
                    "Al interpretar la secuencia original registrada en el catálogo de 1922 («Mi - Sol - Si - Do Natural - Mi»), un resorte mecánico bajo el atril del piano de cola cede con un chasquido.\n\n" +
                    "En el hueco oculto aparece una carta manuscrita del compositor original:\n" +
                    "“Isadora Vale me obligó a sustituir el Do Natural del desenlace por un acorde disonante para justificar un cambio coreográfico de última hora. El manuscrito íntegro sin mutilar fue retirado al Archivo de Partituras contiguo.”");
            }
        }

        private IEnumerator SortScoresAndRevealOriginalSection()
        {
            isPlayingSequence = true;
            if (primaryAudio != null && primaryAudio.clip != null) primaryAudio.Play();

            if (primaryMovingPart != null)
            {
                primaryMovingPart.localPosition += new Vector3(0.08f, 0f, 0.04f);
            }

            yield return new WaitForSeconds(0.45f);
            interactionCompleted = true;
            if (hiddenCompartmentReveal != null) hiddenCompartmentReveal.SetActive(true);
            isPlayingSequence = false;

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "Investigación Completada — Reconstrucción Cronológica de Partituras (1922–1924)",
                    "Al ordenar las carpetas sobre la mesa de clasificación por fecha y comparar la caligrafía de ambas bailarinas:\n\n" +
                    "1. Versión Original (Octubre 1922): Compuesta para una única solista en el clímax.\n" +
                    "2. Partitura de Ensayo de Elena (10 de Octubre 1924): Conserva los compases originales en Mi menor, con una nota al margen de Elena: «Mara entra cuatro compases antes desde el bastidor izquierdo sin que la música lo indique».\n" +
                    "3. Versión Oficial de Dirección (15 de Octubre 1924): Las últimas páginas fueron arrancadas de la carpeta principal.\n\n" +
                    "[SECCIÓN MUSICAL OBTENIDA]: Has recuperado la Coda Original del Tercer Acto y una referencia escrita a lápiz: «La grabación magnética del ensayo sin cortes fue escondida detrás de la grabadora de carrete abierto en la Cabina de Grabaciones contigua».");
            }
        }

        private IEnumerator PlayReelToReelComparison()
        {
            isPlayingSequence = true;
            if (primaryAudio != null && primaryAudio.clip != null) primaryAudio.Play();
            if (secondaryBoothAudio != null && secondaryBoothAudio.clip != null) secondaryBoothAudio.Play();

            // Hacer girar los carretes de la grabadora analógica durante la reproducción
            float duration = 2.4f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (primaryMovingPart != null)
                {
                    primaryMovingPart.Rotate(Vector3.up, 180f * Time.deltaTime, Space.Self);
                }
                yield return null;
            }

            interactionCompleted = true;
            isPlayingSequence = false;

            if (DocumentUI.Instance != null)
            {
                DocumentUI.Instance.ShowDocument(
                    "Comparación de Cintas de Carrete Abierto — Grabación Oculta del 14 de Octubre",
                    "Al sincronizar la cinta oficial de ensayo con la cinta sin etiqueta hallada detrás del amplificador:\n\n" +
                    "• [00:00 - 00:18] Se escucha el piano interpretando la secuencia ausente en las partituras oficiales, salvo que falta una única nota grave de violonchelo (Do Natural) en el compás final.\n" +
                    "• [00:19 - 00:24] La música se interrumpe bruscamente. Se oyen pasos apresurados sobre la madera, el golpe seco de una puerta cerrándose con cerrojo y la respiración agitada de Elena cerca del micrófono.\n" +
                    "• [00:25] Una voz lejana desde el extremo opuesto de la sala exclama: «¡Baja el telón de hierro antes de que vean a—!» antes de que la cinta se corte en seco.\n\n" +
                    "[PISTA CLAVE PARA EL ACTO FINAL]: El mecanismo del telón y la iluminación del escenario responden a la cadencia completa cuyos instrumentos fueron retirados al Depósito de Instrumentos al final de esta sección.");
            }
        }

        private AudioClip CreateRustlingPagesClip()
        {
            int sampleRate = 22050;
            int samples = (int)(sampleRate * 0.75f);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float burst1 = Mathf.Exp(-Mathf.Pow((t - 0.18f) * 14f, 2f));
                float burst2 = Mathf.Exp(-Mathf.Pow((t - 0.46f) * 16f, 2f));
                float noise = (Mathf.PerlinNoise(i * 0.35f, 1.7f) - 0.5f) * 2f;
                data[i] = noise * (burst1 + burst2 * 0.8f) * 0.28f;
            }
            AudioClip clip = AudioClip.Create("Anomaly_RustlingPages", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateDissonantPianoAnomalyClip()
        {
            int sampleRate = 22050;
            int samples = sampleRate * 3;
            float[] data = new float[samples];
            // Secuencia con la cuarta nota equivocada/disonante (Fa4/tritono en lugar de Do4)
            float[] notes = { 164.81f, 196.00f, 246.94f, 233.08f, 164.81f, 233.08f };
            float noteLen = 0.5f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                int idx = Mathf.Min(notes.Length - 1, (int)(t / noteLen));
                float localT = t - (idx * noteLen);
                float freq = notes[idx];
                float env = Mathf.Exp(-localT * 4.2f);
                float wave = Mathf.Sin(2f * Mathf.PI * freq * localT) * 0.6f
                           + Mathf.Sin(2f * Mathf.PI * freq * 2f * localT) * 0.25f;
                // Si es la nota equivocada, añadir batimiento inquietante
                if (idx == 3 || idx == 5)
                {
                    wave += Mathf.Sin(2f * Mathf.PI * (freq * 1.03f) * localT) * 0.35f;
                }
                data[i] = wave * env * 0.28f;
            }
            AudioClip clip = AudioClip.Create("Anomaly_DissonantPiano", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateResolvedPianoSequenceClip()
        {
            int sampleRate = 22050;
            int samples = (int)(sampleRate * 2.5f);
            float[] data = new float[samples];
            // Secuencia original pura: Mi3 - Sol3 - Si3 - Do4 (Natural) - Mi4
            float[] notes = { 164.81f, 196.00f, 246.94f, 261.63f, 329.63f };
            float noteLen = 0.48f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                int idx = Mathf.Min(notes.Length - 1, (int)(t / noteLen));
                float localT = t - (idx * noteLen);
                float freq = notes[idx];
                float env = Mathf.Exp(-localT * 3.6f);
                data[i] = (Mathf.Sin(2f * Mathf.PI * freq * localT) * 0.65f
                         + Mathf.Sin(2f * Mathf.PI * freq * 2f * localT) * 0.25f) * env * 0.34f;
            }
            AudioClip clip = AudioClip.Create("Resolved_PianoSequence", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateBowedCelloNoteClip()
        {
            int sampleRate = 22050;
            int samples = (int)(sampleRate * 2.2f);
            float[] data = new float[samples];
            float freq = 130.81f; // Do3 (Nota profunda de violonchelo ausente en la grabación)

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float attack = Mathf.Clamp01(t / 0.35f);
                float decay = Mathf.Exp(-Mathf.Max(0f, t - 0.35f) * 1.4f);
                float vibrato = Mathf.Sin(2f * Mathf.PI * 5.2f * t) * 1.1f;
                float f = freq + vibrato;
                // Armónicos ricos de cuerda frotada
                float wave = Mathf.Sin(2f * Mathf.PI * f * t) * 0.55f
                           + Mathf.Sin(2f * Mathf.PI * f * 2f * t) * 0.28f
                           + Mathf.Sin(2f * Mathf.PI * f * 3f * t) * 0.14f;
                data[i] = wave * attack * decay * 0.32f;
            }
            AudioClip clip = AudioClip.Create("Anomaly_BowedCelloDo", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateReelTapeRecordingClip(bool secondarySpeakerChannel)
        {
            int sampleRate = 22050;
            int samples = (int)(sampleRate * 2.4f);
            float[] data = new float[samples];
            float[] notes = { 164.81f, 196.00f, 246.94f };

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                // Siseo analógico de cinta magnética
                float tapeHiss = (Mathf.PerlinNoise(i * 0.45f, 0.2f) - 0.5f) * 0.08f;
                float signal = 0f;

                if (t < 1.2f)
                {
                    int idx = Mathf.Min(notes.Length - 1, (int)(t / 0.4f));
                    float lt = t - idx * 0.4f;
                    signal = Mathf.Sin(2f * Mathf.PI * notes[idx] * lt) * Mathf.Exp(-lt * 4.5f) * 0.35f;
                }
                else if (t >= 1.3f && t < 1.75f)
                {
                    // Pasos apresurados y golpe seco de puerta cerrándose
                    float slam = Mathf.Exp(-(t - 1.55f) * 28f) * ((t >= 1.55f) ? 1f : 0f);
                    signal = slam * Mathf.Sin(2f * Mathf.PI * 95f * t) * 0.65f;
                }
                else if (t >= 1.80f && t < 2.25f)
                {
                    // Murmullo de voz lejana interrumpida (más clara en el altavoz secundario de la cabina)
                    float voiceGain = secondarySpeakerChannel ? 0.42f : 0.22f;
                    float formant = Mathf.Sin(2f * Mathf.PI * 210f * t) * Mathf.Sin(2f * Mathf.PI * 620f * t);
                    signal = formant * Mathf.Sin((t - 1.80f) / 0.45f * Mathf.PI) * voiceGain;
                }

                data[i] = tapeHiss + signal;
            }
            AudioClip clip = AudioClip.Create(secondarySpeakerChannel ? "ReelTape_SpeakerB" : "ReelTape_SpeakerA", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
