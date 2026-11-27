using System;
using UnityEngine;
using UltimoActo.Core;

namespace UltimoActo.Narrative
{
    public enum AnomalyTriggerType
    {
        EnterZone = 0,
        ApproachObject = 1,
        TimeInDarkness = 2,
        PuzzleMilestone = 3,
        ControlledRandom = 4
    }

    public enum AnomalyAsymmetricVisibility
    {
        Both = 0,
        Only1929 = 1,
        OnlyRuins = 2,
        BothDifferent = 3
    }

    public enum AnomalyCatalogPattern
    {
        AutonomousReflection = 0,
        SingleEraFigure = 1,
        ApplauseBehindEmptyCurtain = 2,
        ChangingPhotographOnReturn = 3,
        MelodyFromUnmappedRoom = 4,
        CrossEraDisplacedObject = 5,
        CrossingBallerinaShadow = 6,
        AsymmetricDoorVisualState = 7
    }

    /// <summary>
    /// PARTE 3 — Datos de un Evento de Anomalía Asimétrica (ScriptableObject / Data-Driven):
    /// Define ID, noche, zona, disparador, duración, visibilidad por época (Both, Only1929, OnlyRuins
    /// o BothDifferent con variante para Julián Vesper en 1929 y Valeria Bell en las Ruinas),
    /// efectos (visual, sonoro, luz/linterna, objeto), enlace al documento/revelación que refuerza,
    /// y la entrada que deja en el Diario Compartido ("El jugador X vio en 1929 / Ruinas…")
    /// para impulsar la comunicación por voz entre ambos jugadores.
    /// </summary>
    [CreateAssetMenu(fileName = "AnomalyEvent_New", menuName = "Último Acto/Narrativa/Anomaly Event Data")]
    public class AnomalyEventData : ScriptableObject
    {
        [Header("Identificación y Ubicación")]
        public string anomalyId = "anom_n1_sample";
        [Tooltip("Índice de noche (0 = Noche I .. 7 = Noche VIII)")]
        public int nightIndex = 0;
        public string zoneName = "Zona 12 — Sala de Tocadores";
        public Vector3 worldPosition = Vector3.zero;
        public float triggerRadius = 4.5f;

        [Header("Disparador y Duración")]
        public AnomalyTriggerType triggerType = AnomalyTriggerType.ApproachObject;
        [Tooltip("ID de hito de puzle o documento si triggerType == PuzzleMilestone")]
        public string requiredMilestoneId = "";
        public float silencePrefaceSeconds = 0.75f;
        public float durationSeconds = 4.2f;
        public float cooldownSeconds = 35f;
        public bool triggerOncePerNight = false;

        [Header("Visibilidad Asimétrica y Catálogo")]
        public AnomalyAsymmetricVisibility visibility = AnomalyAsymmetricVisibility.BothDifferent;
        public AnomalyCatalogPattern catalogPattern = AnomalyCatalogPattern.AutonomousReflection;
        public bool affectsSharedProgress = false;

        [Header("Efectos y Ganchos de Audio")]
        public bool pulsePlayerFlashlight = true;
        public bool dimNearbyRoomLights = true;
        public string audioHookId1929 = "sfx_anomaly_whisper_1929";
        public string audioHookIdRuins = "sfx_anomaly_scrape_ruins";
        public float synthesizedToneHz1929 = 440f;
        public float synthesizedToneHzRuins = 220f;

        [Header("Narrativa y Registro en el Diario Compartido")]
        public string linkedDocumentOrRevelationId = "doc_s3_mirror_back_note";
        public string journalEntryTitle = "Anomalía Registrada en los Espejos";

        [TextArea(2, 5)]
        public string toastObservation1929 =
            "Tu reflejo en el espejo dorado ha tardado un segundo más en bajar los brazos.";

        [TextArea(2, 5)]
        public string toastObservationRuins =
            "En el espejo agrietado ha aparecido la silueta de una segunda bailarina vestida de Cisne Negro detrás de ti.";

        [TextArea(3, 6)]
        public string journalDescription1929 =
            "observó en la Época 1929 cómo el espejo intacto repetía una postura de ballet que nadie estaba ejecutando frente al tocador.";

        [TextArea(3, 6)]
        public string journalDescriptionRuins =
            "observó en la Época Ruinas una figura inmóvil con el traje del Cisne Negro reflejada detrás del tocador roto, donde en 1929 solo se veía un reflejo desfasado.";

        public static AnomalyEventData CreateRuntime(
            string id,
            int night,
            string zone,
            Vector3 pos,
            float radius,
            AnomalyTriggerType trigger,
            AnomalyAsymmetricVisibility vis,
            AnomalyCatalogPattern pattern,
            float duration,
            string linkedDocId,
            string title,
            string toast1929,
            string toastRuins,
            string journal1929,
            string journalRuins,
            float freq1929 = 392f,
            float freqRuins = 196f,
            string milestoneId = "")
        {
            AnomalyEventData data = CreateInstance<AnomalyEventData>();
            data.name = id;
            data.anomalyId = id;
            data.nightIndex = night;
            data.zoneName = zone;
            data.worldPosition = pos;
            data.triggerRadius = radius;
            data.triggerType = trigger;
            data.requiredMilestoneId = milestoneId;
            data.visibility = vis;
            data.catalogPattern = pattern;
            data.durationSeconds = duration;
            data.linkedDocumentOrRevelationId = linkedDocId;
            data.journalEntryTitle = title;
            data.toastObservation1929 = toast1929;
            data.toastObservationRuins = toastRuins;
            data.journalDescription1929 = journal1929;
            data.journalDescriptionRuins = journalRuins;
            data.synthesizedToneHz1929 = freq1929;
            data.synthesizedToneHzRuins = freqRuins;
            return data;
        }
    }
}
