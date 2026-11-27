using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UltimoActo.Core;
using UltimoActo.Interaction;
using UltimoActo.Puzzles;
using UltimoActo.UI;

namespace UltimoActo.Narrative
{
    /// <summary>
    /// Configura automáticamente para las Noches II a VIII (Pasos D y E):
    /// 1) Los puzles cooperativos asimétricos (Tipos 1 a 5 de CoopPuzzleBase) sin alterar los puzles existentes.
    /// 2) Las listas y documentos complementarios divididos entre 1929 (Daniela) y Ruinas (Mateo)
    ///    para obligar a ambos jugadores a hablar por voz, cruzar datos y deducir cada misterio.
    /// 3) El catálogo de anomalías asimétricas (AnomalyEventData) de las Noches II a VIII.
    /// 4) La convergencia del Amanecer (06:00 h) en la Noche VIII donde ambas épocas se unen.
    /// </summary>
    public class Nights2To8NarrativeAndCoopSetup : MonoBehaviour
    {
        public static Nights2To8NarrativeAndCoopSetup Instance { get; private set; }

        private bool isInitialized;
        private bool dawnConvergenceTriggered;
        private GameObject dawnConvergenceVisualRoot;
        private CoopSimultaneousSwitchPuzzle finaleSimPuzzle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            TryInstallInActiveScene(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryInstallInActiveScene(scene);
        }

        private static void TryInstallInActiveScene(Scene scene)
        {
            if (scene.name == "MainMenu") return;
            if (Instance != null) return;

            GameObject go = new GameObject("Nights2To8NarrativeAndCoopSetup");
            Instance = go.AddComponent<Nights2To8NarrativeAndCoopSetup>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.95f);
            InitializeAllNights2To8();
        }

        private void Update()
        {
            if (!dawnConvergenceTriggered && finaleSimPuzzle != null && finaleSimPuzzle.IsSolved)
            {
                TriggerDawn0600Convergence("Dúo cooperativo de la Cámara tras el Escenario completado");
            }
        }

        private void OnDestroy()
        {
            TheatreClimaxController.OnClimaxPhaseActivated -= HandleTheatrePhaseAdvanced;
            if (Instance == this) Instance = null;
        }

        public void InitializeAllNights2To8()
        {
            if (isInitialized) return;
            isInitialized = true;

            CanonicalizeNights2To8DocumentsAndSplitComplementaryLists();

            SetupNight2CoopMirrorAndMannequins();
            SetupNight3CoopDressmakingAndLattice();
            SetupNight4CoopPowerSyncAndScoreCoda();
            SetupNight5CoopClockTowerClutchAndCryptMirror();
            SetupNight6CoopBackstageSceneryMirror();
            SetupNight7CoopVaultB00AndTheatreLists();
            SetupNight8CoopFinaleAndDawnConvergence();

            RegisterNights2To8AsymmetricAnomalies();

            TheatreClimaxController.OnClimaxPhaseActivated -= HandleTheatrePhaseAdvanced;
            TheatreClimaxController.OnClimaxPhaseActivated += HandleTheatrePhaseAdvanced;

            Debug.Log("[Nights2To8NarrativeAndCoopSetup] Noches II a VIII configuradas (Puzles Cooperativos, Listas Complementarias 1929/Ruinas, Anomalías Asimétricas y Amanecer 06:00 h).");
        }

        // =========================================================================================
        // 1. LISTAS COMPLEMENTARIAS Y DOCUMENTOS CANÓNICOS (NOCHES II A VIII)
        // =========================================================================================

        private void CanonicalizeNights2To8DocumentsAndSplitComplementaryLists()
        {
            WorldDocument[] allDocs = FindObjectsByType<WorldDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Dictionary<string, WorldDocument> byId = new Dictionary<string, WorldDocument>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < allDocs.Length; i++)
            {
                if (allDocs[i] != null && !string.IsNullOrEmpty(allDocs[i].Id))
                {
                    byId[allDocs[i].Id] = allDocs[i];
                }
            }

            // -------------------------------------------------------------------------------------
            // NOCHE II — LAS AULAS DE ENSAYO (Cuota: 8 evidencias — Listas complementarias)
            // -------------------------------------------------------------------------------------
            UpdateDoc(byId, "doc_s4_mirror_sequence_notes",
                "Cuaderno de Posiciones del Espejo n.º 3 — 12 de Octubre de 1929 (Mitad A: 1929)",
                "Zona 17 (Aula de Espejos) · Lista Complementaria 1929",
                "«ANOTACIÓN DE ENSAYO (Academia Vesper, 12 de octubre de 1929):\n" +
                "Isadora ha hecho instalar el nuevo cristal de azogue en el Panel n.º 3 del Aula de Espejos. Cuando Elena ensaya sola frente a él, su reflejo tarda medio compás en obedecer.\n\n" +
                "LISTA DE POSICIONES DEL DÚO (PARTE I — SOLO REGISTRADA EN EL CUADERNO DE 1929):\n" +
                "• 1.ª Posición (Apertura de Espejo): I — Plié de Sombra (Brazos cruzados al pecho).\n" +
                "• 2.ª Posición (Respuesta de Ala): II — Arabesque de Azogue (Brazo izquierdo extendido al cristal).\n" +
                "• 3.ª y 4.ª Posición: Arrancadas de este cuaderno. La costurera Madame Hélène las anotó con tiza en el reverso de la mesa de corte de la Sala de Maniquíes.»\n\n" +
                "[PISTA COOPERATIVA — HABLA CON TU COMPAÑERO]: Dile al jugador de Ruinas las posiciones 1.ª y 2.ª; él debe leerte las posiciones 3.ª y 4.ª halladas en la Sala de Maniquíes.");

            UpdateDoc(byId, "doc_s4_last_performance_fragment",
                "Informe Pericial sobre el Azogue del Panel n.º 3 — Expediente Actual (Mitad B: Ruinas)",
                "Zona 17 (Aula de Espejos) · Lista Complementaria Ruinas",
                "«INFORME DE INSPECCIÓN TÉCNICA (Ruinas de la Academia Vesper — Aula de Espejos):\n" +
                "El Panel n.º 3 no es un espejo ordinario de aula: oculta un bastidor hueco de resonancia conectado por tubo acústico con la cámara trasera del escenario.\n\n" +
                "DETALLE COMPLEMENTARIO PARA EL JUGADOR DE 1929:\n" +
                "Sobre el marco oxidado del espejo y en la mesa de maniquíes contigua sobrevivieron grabadas las dos últimas posturas que faltaban en el cuaderno de 1929:\n" +
                "• 3.ª Posición: III — Attitude de Luto (Giro de perfil hacia el bastidor norte).\n" +
                "• 4.ª Posición: IV — Reverencia Quebrada (Inclinación final hacia la sombra).\n\n" +
                "Si ambos jugadores alternan las 4 posiciones (I → II → III → IV) entre el Espejo n.º 3 y el Bastidor de Maniquíes, el pestillo oculto del taller se libera.»");

            UpdateDoc(byId, "doc_s4_costume_name_tags",
                "Etiquetas de Vestuario — Dos Trajes de Cisne Negro (13 de Octubre de 1929)",
                "Zona 18 (Sala de Maniquíes) · Registro de Sastrería",
                "«REGISTRO DE PRENDAS DE ESTRENO (Academia Vesper — Sala de Maniquíes):\n" +
                "• Maniquí I: \"Elena Vesper — Cisne Negro (Actos I y II)\". Talla de cintura: 56 cm. Forro interior intacto.\n" +
                "• Maniquí II: \"Cisne Negro — SOLO ESCENA DEL ESPEJO (Acto III)\". Talla de cintura: 61 cm (la medida exacta de Mara Bell, no la de Elena).\n\n" +
                "Nota de Madame Hélène: \"La directora Isadora Vale me prohibió preguntar por qué hay dos trajes de Cisne Negro con medidas distintas para la noche del 14 de octubre de 1929.\"»");

            UpdateDoc(byId, "doc_n2_dosage_card",
                "Tarjeta de Dosificación de la Botica — Enfermería (Lista Compartida 1929 / Ruinas)",
                "Zona 43 (Enfermería) · Pista de Botica",
                "«BOTICA DE LA ENFERMERÍA (Academia Vesper — Octubre de 1929):\n" +
                "Para abrir el armario de remedios del Dr. Morales sin activar el bloqueo, los tres frascos deben pulsarse en el orden exacto de preparación del sedante nocturno:\n\n" +
                "• 1.º Frasco I — Tintura de Valeriana (Base calmante prescrita a las internas a las 21:00 h).\n" +
                "• 2.º Frasco II — Bromuro de Potasio (Añadido por orden verbal de Isadora Vale).\n" +
                "• 3.º Frasco III — Éter Dulce de Cloral (Dosis concentrada servida únicamente en la copa del camerino de Elena Vesper el 14 de octubre de 1929).»");

            UpdateDoc(byId, "doc_n2_real_clinical_record",
                "Ficha Clínica Confidencial de Elena Vesper — 13 de Octubre de 1929",
                "Zona 43 (Enfermería) · Archivo Médico Reservado",
                "«FICHA MÉDICA N.º 1929-EV (Enfermería de la Academia Vesper):\n" +
                "Paciente: Elena Vesper (19 años, Primera Bailarina).\n" +
                "Observación del 13 de octubre de 1929 (víspera del estreno): La paciente presenta vértigo ligero y afirma que, durante el ensayo nocturno, su reflejo en el espejo central del escenario permaneció quieto mientras ella giraba.\n\n" +
                "ADVERTENCIA MANUSCRITA: \"Yo no receté las tres dosis de Éter Dulce retiradas hoy del armario bajo llave. La firma del vale de botica pertenece a Isadora Vale.\"»");

            UpdateDoc(byId, "doc_n2_mourning_book",
                "Libro de Duelo de la Sala Conmemorativa — Lista de Ofrendas (1929 vs. Actualidad)",
                "Zona 44 (Sala Conmemorativa) · Protocolo de Ofrendas",
                "«LIBRO DE CONDOLENCIAS Y PROTOCOLO DE OFRENDAS (Academia Vesper — Zona 44):\n" +
                "\"En memoria de Elena Vesper, desaparecida trágicamente en la noche del 14 de octubre de 1929 durante el estreno de El Lago de las Sombras.\"\n\n" +
                "ORDEN RITUAL DE LAS TRES OFRENDAS SOBRE EL PEDESTAL:\n" +
                "• 1.º Zapatillas de Satén Blanco (La inocencia del Primer Acto).\n" +
                "• 2.º Lirio Seco de Invierno (El silencio del Patronato).\n" +
                "• 3.º Pluma de Cisne Negro (La verdad oculta tras el telón).\n\n" +
                "Contradicción registrada: La página del 15 de octubre de 1929 lleva el sello de la funeraria tres horas ANTES de que la policía inspeccionara el escenario.»");

            UpdateDoc(byId, "doc_n2_erased_students_list",
                "Lista de Alumnas Silenciadas del Ala Oeste — Comparativa Cooperativa",
                "Zona 44 (Sala Conmemorativa) · Secreto del Ala Oeste",
                "«ARCHIVO RESERVADO DE INTERNAS (Academia Vesper — Noviembre de 1929):\n" +
                "Las tres alumnas del dormitorio oeste que declararon haber oído dos voces discutiendo en la cámara del espejo a las 22:18 h del 14 de octubre vieron sus becas revocadas al día siguiente:\n" +
                "1. Clara M. — Expulsada por \"delirio acústico\".\n" +
                "2. Sofía R. — Trasladada sin expediente.\n" +
                "3. Lucía V. — Dejó escondida bajo la tarima del dormitorio la cajita musical de Elena.»");

            // -------------------------------------------------------------------------------------
            // NOCHE III — EL INTERNADO (Cuota: 8 evidencias — Listas complementarias)
            // -------------------------------------------------------------------------------------
            UpdateDoc(byId, "doc_n3_isadora_private_diary",
                "Diario Privado de Isadora Vale — El Brindis de la Celosía (14 de Octubre de 1929)",
                "Zona 48 (Estancia Privada de Isadora) · Confesión de 1929",
                "«DIARIO DE ISADORA VALE (Estancia Privada, Zona 48 — 14 de octubre de 1929, 19:30 h):\n" +
                "\"El arte verdadero no admite titubeos. Elena ha descubierto las marcas del espejo de ilusión y amenaza con detener la función en el tercer acto y hablar ante la prensa.\n\n" +
                "Todo está dispuesto en la bandeja de cristal de mi antecámara. El mecanismo de la celosía de vigilancia solo se abre al pulsar las tres copas en la jerarquía del brindis de gala:\n" +
                "• 1.º Copa de Plata (El Patronato que financia y calla).\n" +
                "• 2.º Copa de Ámbar (Mara Bell, que ocupará el lugar del reflejo cuando caiga el telón).\n" +
                "• 3.º Copa Carmesí (Elena Vesper, con las tres gotas de la enfermería para que su voz no tiemble en la cámara trasera).\"»");

            // -------------------------------------------------------------------------------------
            // NOCHE IV — ARCHIVO Y PARQUE INTERIOR (Cuota: 8 evidencias — Listas complementarias)
            // -------------------------------------------------------------------------------------
            UpdateDoc(byId, "doc_s5_music_catalog",
                "Catálogo General del Archivo Musical (1927–1929) — Clave del Piano de Cola",
                "Zona 21 (Biblioteca Musical) · Clave del Piano",
                "«CATÁLOGO DE OBRAS — ACADEMIA VESPER (Sección 5, Biblioteca Musical):\n" +
                "Obra N.º 44: \"El Lago de las Sombras\" (Ballet en tres actos). Compositor: Julian Krol.\n" +
                "• Fecha de composición original: Octubre de 1928.\n" +
                "• Fecha de revisión forzosa para el estreno: 12 de octubre de 1929.\n\n" +
                "SECUENCIA DE APERTURA DEL ATRIL SECRETO DEL PIANO DE COLA:\n" +
                "\"Mi — Sol — Si — Do Natural — Mi\"\n" +
                "(Isadora Vale ordenó sustituir el cuarto tono por un acorde disonante en el Compás 48 para cubrir el ruido del cerrojo mecánico tras el espejo del escenario).»");

            UpdateDoc(byId, "doc_n4_restoration_note",
                "Guía de Restauración en Tres Capas — Compás 48 (Lista Complementaria 1929 / Ruinas)",
                "Zona 21 (Biblioteca Musical) · Mesa de Restauración",
                "«MESA DE RESTAURACIÓN DE PARTITURAS (Academia Vesper — Zona 21):\n" +
                "Para reconstruir el Compás 48 original de Julian Krol sobre el atril de restauración, deben superponerse los tres folios en orden cronológico:\n\n" +
                "• 1.º Folio I — Borrador a Lápiz de Julian Krol (Sin el calderón de silencio).\n" +
                "• 2.º Folio II — Calco de Tinta Roja de Elena Vesper (Marca los 4 pasos hacia el espejo).\n" +
                "• 3.º Folio III — Pliego Oficial Tachado por Isadora (14 de octubre de 1929).\n\n" +
                "[NOTA COOPERATIVA]: La válvula hidráulica de la Fuente del Cisne (Zona 40) y el Sótano de Mantenimiento (Zona 41) requieren que ambos jugadores mantengan sincronizados los interruptores de presión de ambas épocas.»");
        }

        private static void UpdateDoc(Dictionary<string, WorldDocument> byId, string id, string newTitle, string newSubtitle, string newBody)
        {
            if (byId.TryGetValue(id, out WorldDocument doc) && doc != null)
            {
                doc.ConfigureDocument(id, newTitle, newBody, newSubtitle);
            }
        }

        // =========================================================================================
        // 2. NOCHE II — PUZLE COOPERATIVO: ESPEJO N.º 3 (1929) Y MANIQUÍES DE ENSAYO (RUINAS)
        // =========================================================================================

        private void SetupNight2CoopMirrorAndMannequins()
        {
            if (GameObject.Find("Night2_CoopMirrorMannequinPuzzle") != null) return;

            GameObject root = new GameObject("Night2_CoopMirrorMannequinPuzzle");
            root.transform.position = new Vector3(-24.5f, 2.4f, 71.5f);

            CoopTurnSequencePuzzle turnPuzzle = root.AddComponent<CoopTurnSequencePuzzle>();

            GameObject rewardCabinet = CreateRewardPedestal(
                root.transform,
                "Night2_UnlockedPatternDrawer",
                new Vector3(-20.4f, 2.4f, 74.8f));

            turnPuzzle.ConfigureTurnSequence(
                "coop_night2_mirror_mannequin_poses",
                "El Reflejo y el Maniquí (Noche II — Espejos y Vestuario)",
                new int[] { 0, 1, 2, 3 },
                new EraVisibilityMode[]
                {
                    EraVisibilityMode.Only1929,
                    EraVisibilityMode.OnlyRuins,
                    EraVisibilityMode.Only1929,
                    EraVisibilityMode.OnlyRuins
                },
                new GameObject[] { rewardCabinet });

            // Placa 1 (SOLO 1929) junto al Panel 3 del Aula de Espejos (-36.85f, 3.65f, 59.0f)
            CreateEraCluePlaque(
                root.transform,
                turnPuzzle,
                "Night2_Mirror3_1929Plaque",
                new Vector3(-36.85f, 3.65f, 59.0f),
                Quaternion.Euler(0f, 90f, 0f),
                EraVisibilityMode.Only1929,
                "night2_mirror3_poses_part1_1929",
                "Inscripción de Azogue en el Panel n.º 3 (1929)",
                "Inscripción de Azogue en el Panel n.º 3 (1929) — Mitad I de la Coreografía",
                "En el cristal recién azogado de 1929 se distinguen las marcas de tiza blanca que Elena Vesper trazó antes del ensayo general:\n\n" +
                "• PASO 1 (En 1929 — Aula de Espejos): Posición I — «Plié de Sombra».\n" +
                "• PASO 2 (En Ruinas — Sala de Maniquíes): Posición II — «Arabesque de Azogue».\n\n" +
                "(Las marcas de los pasos 3 y 4 fueron borradas en 1929, pero quedaron talladas en la madera de la Sala de Maniquíes que ve tu compañero en las Ruinas).",
                new Color(0.95f, 0.80f, 0.42f));

            // Placa 2 (SOLO RUINAS) junto a la mesa central de la Sala de Maniquíes (-19.0f, 3.55f, 76.2f)
            CreateEraCluePlaque(
                root.transform,
                turnPuzzle,
                "Night2_Mannequin_RuinsPlaque",
                new Vector3(-19.0f, 3.55f, 76.2f),
                Quaternion.Euler(0f, 0f, 0f),
                EraVisibilityMode.OnlyRuins,
                "night2_mannequin_poses_part2_ruins",
                "Marcas de Alfiler en la Mesa de Maniquíes (Ruinas)",
                "Marcas de Alfiler en la Mesa de Maniquíes (Ruinas) — Mitad II de la Coreografía",
                "Sobre la madera agrietada de la Sala de Maniquíes, visibles únicamente bajo la capa de ruinas actuales, aparecen grabadas las dos últimas posiciones del dúo:\n\n" +
                "• PASO 3 (En 1929 — Aula de Espejos): Posición III — «Attitude de Luto».\n" +
                "• PASO 4 (En Ruinas — Sala de Maniquíes): Posición IV — «Reverencia Quebrada».\n\n" +
                "[COOPERACIÓN]: Alternad los 4 pasos por turnos (1.º en 1929 → 2.º en Ruinas → 3.º en 1929 → 4.º en Ruinas) para abrir el compartimento de patronaje.",
                new Color(0.55f, 0.78f, 0.95f));

            CreateTurnNode(root.transform, turnPuzzle, 0, "Posición I — Plié de Sombra (1929)", EraVisibilityMode.Only1929,
                new Vector3(-36.7f, 2.95f, 57.6f), new Color(0.92f, 0.76f, 0.35f));
            CreateTurnNode(root.transform, turnPuzzle, 1, "Posición II — Arabesque de Azogue (Ruinas)", EraVisibilityMode.OnlyRuins,
                new Vector3(-21.2f, 2.95f, 73.8f), new Color(0.48f, 0.75f, 0.95f));
            CreateTurnNode(root.transform, turnPuzzle, 2, "Posición III — Attitude de Luto (1929)", EraVisibilityMode.Only1929,
                new Vector3(-36.7f, 2.95f, 60.4f), new Color(0.92f, 0.76f, 0.35f));
            CreateTurnNode(root.transform, turnPuzzle, 3, "Posición IV — Reverencia Quebrada (Ruinas)", EraVisibilityMode.OnlyRuins,
                new Vector3(-17.2f, 2.95f, 73.8f), new Color(0.48f, 0.75f, 0.95f));
        }

        // =========================================================================================
        // 3. NOCHE III — PUZLE COOPERATIVO: PATRONAJE Y CELOSÍA DEL INTERNADO (ZONAS 45–48)
        // =========================================================================================

        private void SetupNight3CoopDressmakingAndLattice()
        {
            if (GameObject.Find("Night3_CoopInternadoLatticePuzzle") != null) return;

            GameObject root = new GameObject("Night3_CoopInternadoLatticePuzzle");
            root.transform.position = new Vector3(-48.0f, 2.4f, 48.0f);

            CoopTurnSequencePuzzle turnPuzzle = root.AddComponent<CoopTurnSequencePuzzle>();
            GameObject rewardBox = CreateRewardPedestal(
                root.transform,
                "Night3_SecretMusicBoxCompartment",
                new Vector3(-45.5f, 2.4f, 23.2f));

            turnPuzzle.ConfigureTurnSequence(
                "coop_night3_lattice_and_dye_workshop",
                "El Secreto de la Celosía y el Segundo Cisne Negro (Noche III)",
                new int[] { 0, 1, 2, 3 },
                new EraVisibilityMode[]
                {
                    EraVisibilityMode.Only1929,
                    EraVisibilityMode.OnlyRuins,
                    EraVisibilityMode.Only1929,
                    EraVisibilityMode.OnlyRuins
                },
                new GameObject[] { rewardBox });

            CreateEraCluePlaque(
                root.transform,
                turnPuzzle,
                "Night3_Lattice_1929List",
                new Vector3(-43.8f, 3.65f, 21.2f),
                Quaternion.Euler(0f, -90f, 0f),
                EraVisibilityMode.Only1929,
                "night3_internado_list_1929",
                "Registro de Rondas de la Celosía de Isadora (1929)",
                "Registro de Rondas de la Celosía de Isadora (1929) — Mitad A",
                "Junto a la mirilla oculta desde la que Isadora Vale vigilaba a las internas en 1929 figura la primera mitad del código de bastidores y del servicio nocturno:\n\n" +
                "• TURNO 1 (En 1929 — Estancia de Isadora): Activar el Tirador de Celosía I («Silencio de las 21:00 h»).\n" +
                "• TURNO 3 (En 1929 — Estancia de Isadora): Activar el Tirador de Celosía III («Copa Carmesí de Elena»).\n\n" +
                "¿Qué debe hacer tu compañero en las Ruinas en los Turnos 2 y 4? Pregúntale qué dice la tabla de tintes de Madame Hélène en el Taller de Costura (Zona 46).",
                new Color(0.95f, 0.78f, 0.38f));

            CreateEraCluePlaque(
                root.transform,
                turnPuzzle,
                "Night3_Workshop_RuinsList",
                new Vector3(-48.5f, 3.65f, 84.5f),
                Quaternion.Euler(0f, 180f, 0f),
                EraVisibilityMode.OnlyRuins,
                "night3_internado_list_ruins",
                "Tabla de Tintes y Alfileres de Madame Hélène (Ruinas)",
                "Tabla de Tintes y Alfileres de Madame Hélène (Ruinas) — Mitad B",
                "En las ruinas de la Tintorería (Zona 46), bajo el forro quemado del segundo tutú de Cisne Negro, aparece la mitad complementaria de la lista:\n\n" +
                "• TURNO 2 (En Ruinas — Taller de Costura): Girar la Bobina II («Azogue y Tinte Negro — Talla 61 cm de Mara»).\n" +
                "• TURNO 4 (En Ruinas — Taller de Costura): Girar la Bobina IV («Dobladillo de Plomo para el Espejo»).\n\n" +
                "[COOPERACIÓN]: Comunicando ambas listas por voz, completad la secuencia 1 (1929) → 2 (Ruinas) → 3 (1929) → 4 (Ruinas).",
                new Color(0.52f, 0.80f, 0.96f));

            CreateTurnNode(root.transform, turnPuzzle, 0, "Tirador I — Silencio de las 21:00 h (1929)", EraVisibilityMode.Only1929,
                new Vector3(-43.9f, 2.95f, 20.2f), new Color(0.95f, 0.78f, 0.38f));
            CreateTurnNode(root.transform, turnPuzzle, 1, "Bobina II — Tinte Negro Talla 61 cm (Ruinas)", EraVisibilityMode.OnlyRuins,
                new Vector3(-49.5f, 2.95f, 83.2f), new Color(0.52f, 0.80f, 0.96f));
            CreateTurnNode(root.transform, turnPuzzle, 2, "Tirador III — Copa Carmesí de Elena (1929)", EraVisibilityMode.Only1929,
                new Vector3(-43.9f, 2.95f, 22.0f), new Color(0.95f, 0.78f, 0.38f));
            CreateTurnNode(root.transform, turnPuzzle, 3, "Bobina IV — Dobladillo de Plomo (Ruinas)", EraVisibilityMode.OnlyRuins,
                new Vector3(-47.5f, 2.95f, 83.2f), new Color(0.52f, 0.80f, 0.96f));
        }

        // =========================================================================================
        // 4. NOCHE IV — PUZLE COOPERATIVO (TIPO 2: ACCIÓN SIMULTÁNEA): FUSIBLES Y CABINA DE AUDIO
        // =========================================================================================

        private void SetupNight4CoopPowerSyncAndScoreCoda()
        {
            if (GameObject.Find("Night4_CoopSimultaneousFusePuzzle") != null) return;

            GameObject root = new GameObject("Night4_CoopSimultaneousFusePuzzle");
            root.transform.position = new Vector3(34.0f, 2.4f, 56.0f);

            CoopSimultaneousSwitchPuzzle simPuzzle = root.AddComponent<CoopSimultaneousSwitchPuzzle>();
            simPuzzle.ConfigureSimultaneous(
                "coop_night4_simultaneous_power_relay",
                "Sincronía de Suministro: Cabina de Grabación y Sótano (Noche IV)",
                4.5f,
                14.0f);

            CreateEraCluePlaque(
                root.transform,
                simPuzzle,
                "Night4_Booth_1929Plaque",
                new Vector3(35.8f, 3.65f, 57.2f),
                Quaternion.Euler(0f, -90f, 0f),
                EraVisibilityMode.Only1929,
                "night4_simultaneous_power_clue_1929",
                "Esquema de Alimentación Acústica de Julian Krol (1929)",
                "Esquema de Alimentación Acústica de Julian Krol (1929) — Mitad A",
                "«CABINA DE GRABACIÓN MAGNÉTICA (14 de octubre de 1929):\n" +
                "Para alimentar el amplificador auxiliar que registró el Compás 48 sin cortes, el Regulador de Válvulas de 1929 (aquí en la Cabina de Grabaciones, Zona 24) debe mantenerse accionado AL MISMO TIEMPO que el Disyuntor Maestro (visible en las Ruinas).\n\n" +
                "LISTA COMPLEMENTARIA DEL ARCHIVO (PARTE 1929):\n" +
                "• Partitura de Restauración: 1.º Borrador de Krol → (pide el 2.º a tu compañero en Ruinas) → 3.º Pliego Tachado de Isadora.»",
                new Color(0.95f, 0.80f, 0.40f));

            CreateEraCluePlaque(
                root.transform,
                simPuzzle,
                "Night4_Basement_RuinsPlaque",
                new Vector3(35.8f, 3.65f, 54.4f),
                Quaternion.Euler(0f, -90f, 0f),
                EraVisibilityMode.OnlyRuins,
                "night4_simultaneous_power_clue_ruins",
                "Diagrama del Disyuntor Trifásico (Ruinas)",
                "Diagrama del Disyuntor Trifásico del Archivo y Sótano (Ruinas) — Mitad B",
                "«CONEXIÓN TRIFÁSICA DEL ARCHIVO Y SÓTANO (Ruinas):\n" +
                "El embrague eléctrico hacia el Archivo Musical requiere ACCIÓN SIMULTÁNEA (ventana de 4.5 segundos) entre ambos jugadores:\n" +
                "• Jugador en Ruinas: Accionar el Disyuntor de Suministro (Ruinas).\n" +
                "• Jugador en 1929: Accionar a la vez el Regulador de Válvulas en la Cabina de Grabaciones (1929).\n" +
                "(En Modo Solitario, el Eco-Retenedor mantiene la palanca durante 14 segundos para que alternes con [Q]).\n\n" +
                "LISTA COMPLEMENTARIA DEL ARCHIVO (PARTE RUINAS):\n" +
                "• El 2.º folio de la mesa de restauración es el Calco en Tinta Roja de Elena Vesper. Los sectores del generador son: I. Archivo Musical → II. Invernadero → III. Foso de Orquesta.»",
                new Color(0.50f, 0.82f, 0.96f));

            CreateSimultaneousSwitchNode(
                root.transform,
                simPuzzle,
                false,
                "Regulador de Válvulas de la Cabina (1929 — Zona 24)",
                new Vector3(36.1f, 2.95f, 57.5f),
                new Color(0.95f, 0.76f, 0.32f));

            CreateSimultaneousSwitchNode(
                root.transform,
                simPuzzle,
                true,
                "Disyuntor de Suministro del Archivo / Sótano (Ruinas)",
                new Vector3(36.1f, 2.95f, 53.8f),
                new Color(0.45f, 0.80f, 0.98f));
        }

        // =========================================================================================
        // 5. NOCHE V — PUZLE COOPERATIVO (TIPO 5): TORRE DEL RELOJ Y CRIPTA (ZONAS 49–56)
        // =========================================================================================

        private void SetupNight5CoopClockTowerClutchAndCryptMirror()
        {
            if (GameObject.Find("Night5_CoopClockAndCryptPuzzle") != null) return;

            GameObject root = new GameObject("Night5_CoopClockAndCryptPuzzle");
            root.transform.position = new Vector3(0f, -3.20f, 20.5f);

            CoopCrossEraMirrorPuzzle cryptMirror = root.AddComponent<CoopCrossEraMirrorPuzzle>();

            GameObject ruinsRubble = CreateRuinsRubbleBlocker(
                root.transform,
                "Night5_Crypt_RuinsRubbleBlocker",
                new Vector3(0.0f, -2.65f, 22.2f));

            GameObject revealedCenotaphProof = CreateRewardPedestal(
                root.transform,
                "Night5_Crypt_EmptyCoffinProof",
                new Vector3(0.0f, -3.20f, 22.2f));

            GameObject lever1929 = CreateCrossEraLeverVisual(
                root.transform,
                "Night5_Crypt_1929BronzeLatch",
                new Vector3(-1.4f, -2.25f, 21.2f));

            cryptMirror.ConfigureMirrorEffect(
                "coop_night5_crypt_cross_era_mirror",
                "Efecto Espejo en la Cripta: El Féretro Sin Cuerpo (Noche V)",
                EraVisibilityMode.Only1929,
                "Descorrer el Cerrojo de Bronce de la Reja Interior de la Cripta (1929)",
                lever1929.transform,
                new Vector3(0.45f, 0f, 0f),
                new GameObject[] { ruinsRubble },
                new GameObject[] { revealedCenotaphProof });

            CreateEraCluePlaque(
                root.transform,
                cryptMirror,
                "Night5_ClockTower_1929Plaque",
                new Vector3(-1.8f, -1.85f, 21.5f),
                Quaternion.Euler(0f, 0f, 0f),
                EraVisibilityMode.Only1929,
                "night5_clock_crypt_list_1929",
                "Cuaderno de Engranajes y Cripta (1929)",
                "Cuaderno de Engranajes de la Torre del Reloj y Cripta (1929) — Mitad A",
                "«TORRE DEL RELOJ Y CRIPTA DE FUNDADORES (14 de octubre de 1929 — 22:17 h):\n" +
                "Por orden de Isadora Vale, el péndulo mayor fue calzado a las 22:17 h para que la academia entera quedara congelada en el instante del tercer acto.\n\n" +
                "LISTAS COMPLEMENTARIAS PARA TU COMPAÑERO EN RUINAS:\n" +
                "• Linterna Mágica (Aula de Teoría, Zona 50): 1.ª Diapositiva = «El Cisne Frente al Espejo». (Tu compañero tiene la 2.ª en las Ruinas). 3.ª Diapositiva = «La Silueta Encerrada».\n" +
                "• Cripta de Fundadores (Zona 56): Si accionas este mecanismo en 1929, los escombros que bloquean el Féretro Vacío de Elena en las Ruinas desaparecerán por Efecto Espejo.»",
                new Color(0.95f, 0.80f, 0.42f));

            CreateEraCluePlaque(
                root.transform,
                cryptMirror,
                "Night5_Crypt_RuinsPlaque",
                new Vector3(1.8f, -1.85f, 21.5f),
                Quaternion.Euler(0f, 0f, 0f),
                EraVisibilityMode.OnlyRuins,
                "night5_clock_crypt_list_ruins",
                "Inscripción de la Cripta y Calderas (Ruinas)",
                "Inscripción de la Cripta y Calderas (Ruinas) — Mitad B",
                "«CRIPTA DE LOS FUNDADORES Y SUBTERRÁNEO (Zona 56 — Ruinas):\n" +
                "El acceso al cenotafio de Elena Vesper está sepultado bajo un derrumbe en la actualidad, pero en 1929 la reja original aún puede abrirse desde este mismo punto.\n\n" +
                "LISTAS COMPLEMENTARIAS PARA TU COMPAÑERO EN 1929:\n" +
                "• Linterna Mágica (Zona 50): La 2.ª Diapositiva es «El Doble Bajo el Telón».\n" +
                "• Hogar de la Caldera (Zona 53): El orden de recuperación entre las cenizas es: 1.º Hebilla de Satén → 2.º Fragmento de Partitura → 3.º Llave de Bronce Quemada.\n" +
                "[COOPERACIÓN]: Pídele al jugador de 1929 que descorra el Cerrojo de Bronce de la Cripta (Zona 56) para despejar el derrumbe en tu época.»",
                new Color(0.50f, 0.82f, 0.96f));
        }

        // =========================================================================================
        // 6. NOCHE VI — PUZLE COOPERATIVO (TIPO 5): ESCENOGRAFÍA Y CONTRAPESO III (ZONAS 26–30)
        // =========================================================================================

        private void SetupNight6CoopBackstageSceneryMirror()
        {
            if (GameObject.Find("Night6_CoopBackstageMirrorPuzzle") != null) return;

            GameObject root = new GameObject("Night6_CoopBackstageMirrorPuzzle");
            root.transform.position = new Vector3(-18.5f, 2.4f, 94.0f);

            CoopCrossEraMirrorPuzzle pulleyMirror = root.AddComponent<CoopCrossEraMirrorPuzzle>();

            GameObject collapsedBackdropRuins = CreateRuinsRubbleBlocker(
                root.transform,
                "Night6_CollapsedLakeBackdrop_Ruins",
                new Vector3(-19.5f, 2.95f, 93.2f));

            GameObject revealedShearedCableEvidence = CreateRewardPedestal(
                root.transform,
                "Night6_ShearedCableAndCufflinkProof",
                new Vector3(-19.5f, 2.4f, 93.2f));

            GameObject winch1929 = CreateCrossEraLeverVisual(
                root.transform,
                "Night6_Winch1929_CounterweightIII",
                new Vector3(-18.0f, 3.1f, 93.5f));

            pulleyMirror.ConfigureMirrorEffect(
                "coop_night6_backstage_pulley_mirror",
                "Efecto Espejo en Bambalinas: El Contrapeso III (Noche VI)",
                EraVisibilityMode.Only1929,
                "Izar y asegurar el Torno del Bastidor del Lago en la Sala de Poleas (1929)",
                winch1929.transform,
                new Vector3(0f, 0.45f, 0f),
                new GameObject[] { collapsedBackdropRuins },
                new GameObject[] { revealedShearedCableEvidence });

            CreateEraCluePlaque(
                root.transform,
                pulleyMirror,
                "Night6_Pulley_1929Plaque",
                new Vector3(-17.2f, 3.65f, 95.2f),
                Quaternion.Euler(0f, 0f, 0f),
                EraVisibilityMode.Only1929,
                "night6_backstage_order_1929",
                "Orden de Tramoya de Isadora Vale (13 de Octubre de 1929)",
                "Orden de Tramoya de Isadora Vale — 13 de Octubre de 1929 (Mitad A: 1929)",
                "«SALA DE POLEAS Y CONTRAPESOS (Academia Vesper — 13 de octubre de 1929, UN DÍA ANTES DEL ESTRENO):\n" +
                "\"Al maestro tramoyista Tomás: Mañana 14 de octubre, exactamente en el Compás 48 del Acto III, soltará usted el Contrapeso III para que el Telón de Hierro caiga de golpe y aísle la cámara del espejo central.\"\n\n" +
                "[ACCIÓN EN 1929]: Iza el Bastidor del Lago en este torno de 1929 para que los escombros del decorado desplomado en las Ruinas dejen libre el tambor de cables a tu compañero.»",
                new Color(0.95f, 0.80f, 0.40f));

            CreateEraCluePlaque(
                root.transform,
                pulleyMirror,
                "Night6_Pulley_RuinsPlaque",
                new Vector3(-21.2f, 3.65f, 95.2f),
                Quaternion.Euler(0f, 0f, 0f),
                EraVisibilityMode.OnlyRuins,
                "night6_backstage_police_report_ruins",
                "Peritaje Policial del Cable Cortado (Noviembre de 1929)",
                "Peritaje Policial del Cable Cortado — Noviembre de 1929 (Mitad B: Ruinas)",
                "«EXPEDIENTE POLICIAL ARCHIVADO (Ruinas de la Sala de Poleas — Zona 30):\n" +
                "Versión oficial publicada en prensa: \"El cable del Contrapeso III se rompió fortuitamente por fatiga del acero la noche del 14 de octubre de 1929.\"\n\n" +
                "CONTRADICCIÓN DEMOSTRADA AL HABLAR CON EL JUGADOR DE 1929:\n" +
                "1. En las Ruinas, el extremo del cable de acero presenta un corte limpio de cizalla, no rotura por tensión.\n" +
                "2. Tu compañero en 1929 tiene delante la orden escrita por Isadora el 13 de octubre (24 horas antes del supuesto accidente) mandando soltar el Contrapeso III en el Compás 48.»",
                new Color(0.50f, 0.82f, 0.96f));
        }

        // =========================================================================================
        // 7. NOCHE VII — PUZLE COOPERATIVO: BÓVEDA B-00 Y GUION DIVIDIDO DEL TEATRO (ZONAS 31–38)
        // =========================================================================================

        private void SetupNight7CoopVaultB00AndTheatreLists()
        {
            if (GameObject.Find("Night7_CoopVaultB00Puzzle") != null) return;

            GameObject root = new GameObject("Night7_CoopVaultB00Puzzle");
            root.transform.position = new Vector3(-8.0f, 2.4f, 118.0f);

            CoopTurnSequencePuzzle vaultPuzzle = root.AddComponent<CoopTurnSequencePuzzle>();
            GameObject vaultReward = CreateRewardPedestal(
                root.transform,
                "Night7_VaultB00_UnsealedPactDossier",
                new Vector3(-8.0f, 2.4f, 115.5f));

            vaultPuzzle.ConfigureTurnSequence(
                "coop_night7_theatre_and_vault_b00",
                "Reconstrucción del Estreno: El Pacto de los Dos Cisnes (Noche VII)",
                new int[] { 0, 1, 2, 3 },
                new EraVisibilityMode[]
                {
                    EraVisibilityMode.Only1929,
                    EraVisibilityMode.OnlyRuins,
                    EraVisibilityMode.Only1929,
                    EraVisibilityMode.OnlyRuins
                },
                new GameObject[] { vaultReward });

            CreateEraCluePlaque(
                root.transform,
                vaultPuzzle,
                "Night7_Stage_1929CueSheet",
                new Vector3(-11.5f, 3.65f, 118.2f),
                Quaternion.Euler(0f, 0f, 0f),
                EraVisibilityMode.Only1929,
                "night7_theatre_cues_part1_1929",
                "Guion de Dirección del Último Acto — Fases Impares (1929)",
                "Guion de Dirección del Último Acto — Fases Impares (1929 — Mitad A)",
                "«GRAN TEATRO DE LA ACADEMIA VESPER (14 de octubre de 1929 — Guion de Escena):\n" +
                "Para abrir la Cámara tras el Escenario (Zona 39) y liberar el Espejo Central, las 5 fases del teatro deben ejecutarse en orden. Este pliego de 1929 solo conserva las FASES IMPARES:\n\n" +
                "• FASE 1 (Foso de Orquesta — Zona 33): Alinear los 3 atriles con la Coda Original en Mi menor.\n" +
                "• FASE 2: [Arrancada — Pregunta a tu compañero en las Ruinas qué indica la marca de tiza del escenario].\n" +
                "• FASE 3 (Cabina de Luces — Zona 34): Enfocar los proyectores sobre el Espejo Central del Escenario.\n" +
                "• FASE 4: [Arrancada — Pregunta a tu compañero en las Ruinas qué contrapeso de la pasarela superior debe liberarse].\n" +
                "• FASE 5 (Escenario — Zona 32): Activar el Mecanismo del Espejo Central para abrir la Cámara Oculta (Zona 39).»",
                new Color(0.95f, 0.80f, 0.42f));

            CreateEraCluePlaque(
                root.transform,
                vaultPuzzle,
                "Night7_Stage_RuinsCueSheet",
                new Vector3(-4.5f, 3.65f, 118.2f),
                Quaternion.Euler(0f, 0f, 0f),
                EraVisibilityMode.OnlyRuins,
                "night7_theatre_cues_part2_ruins",
                "Bitácora de Escenario y Bóveda B-00 — Fases Pares (Ruinas)",
                "Bitácora de Reconstrucción del Escenario y Bóveda B-00 — Fases Pares (Ruinas — Mitad B)",
                "«GRAN TEATRO Y BÓVEDA SUBTERRÁNEA B-00 (Ruinas — Expediente de Mateo y Daniela):\n" +
                "Este informe actual contiene las FASES PARES que faltan en el guion de 1929:\n\n" +
                "• FASE 2 (Tablas del Escenario — Zona 32): Colocar las 3 posiciones de escena de Elena y Mara frente al bastidor de espejo.\n" +
                "• FASE 4 (Pasarela Técnica Superior — Zona 35): Desbloquear el puente de tramoya sobre el telón de fondo.\n\n" +
                "CONFESIÓN DE MARA BELL (Bóveda B-00, Zona 38):\n" +
                "\"Elena y yo descubrimos el plan de Isadora la tarde del 14 de octubre. Pactamos intercambiar nuestras posiciones en el Compás 48 para dejar en evidencia a la directora, pero Isadora adelantó la caída del telón y encerró a Elena en la cámara acústica tras el espejo.\"»",
                new Color(0.50f, 0.82f, 0.96f));

            CreateTurnNode(root.transform, vaultPuzzle, 0, "Hito I — 22:15 h: Coda Original de Krol (1929)", EraVisibilityMode.Only1929,
                new Vector3(-10.2f, 2.95f, 116.5f), new Color(0.95f, 0.80f, 0.42f));
            CreateTurnNode(root.transform, vaultPuzzle, 1, "Hito II — 22:16 h: Pacto de Elena y Mara (Ruinas)", EraVisibilityMode.OnlyRuins,
                new Vector3(-8.8f, 2.95f, 116.5f), new Color(0.50f, 0.82f, 0.96f));
            CreateTurnNode(root.transform, vaultPuzzle, 2, "Hito III — 22:17 h: Caída del Telón de Hierro (1929)", EraVisibilityMode.Only1929,
                new Vector3(-7.2f, 2.95f, 116.5f), new Color(0.95f, 0.80f, 0.42f));
            CreateTurnNode(root.transform, vaultPuzzle, 3, "Hito IV — 22:18 h: Encierro en la Cámara Trasera (Ruinas)", EraVisibilityMode.OnlyRuins,
                new Vector3(-5.8f, 2.95f, 116.5f), new Color(0.50f, 0.82f, 0.96f));
        }

        // =========================================================================================
        // 8. NOCHE VIII — PUZLE COOPERATIVO FINAL Y CONVERGENCIA DEL AMANECER (06:00 H)
        // =========================================================================================

        private void SetupNight8CoopFinaleAndDawnConvergence()
        {
            if (GameObject.Find("Night8_CoopDawnFinalePuzzle") != null) return;

            GameObject root = new GameObject("Night8_CoopDawnFinalePuzzle");
            root.transform.position = new Vector3(8.3f, 2.4f, 102.5f);

            finaleSimPuzzle = root.AddComponent<CoopSimultaneousSwitchPuzzle>();
            finaleSimPuzzle.ConfigureSimultaneous(
                "coop_night8_dawn_convergence_finale",
                "El Último Acto: Convergencia de las Dos Épocas al Amanecer (06:00 h)",
                5.0f,
                16.0f);

            CreateEraCluePlaque(
                root.transform,
                finaleSimPuzzle,
                "Night8_Chamber_1929FinalPlaque",
                new Vector3(7.1f, 3.65f, 103.4f),
                Quaternion.Euler(0f, 0f, 0f),
                EraVisibilityMode.Only1929,
                "night8_final_act_score_1929",
                "Última Página de Elena Vesper en la Cámara tras el Espejo (1929)",
                "Última Página de Elena Vesper en la Cámara tras el Espejo (1929 — Mitad A)",
                "«CÁMARA TRAS EL ESCENARIO (Zona 39 — Madrugada del 15 de octubre de 1929):\n" +
                "\"Si alguien escucha esta cajita musical al otro lado del espejo, no busquéis venganza contra Mara. Ella intentó abrir el cerrojo desde fuera hasta que nos separaron.\n\n" +
                "Para que el último acto termine y el reloj de la Academia Vesper alcance por fin las 06:00 de la mañana, quien vea mi noche de 1929 y quien vea el tiempo futuro deben sostener juntos los dos atriles de resonancia del espejo.\"»",
                new Color(0.98f, 0.84f, 0.45f));

            CreateEraCluePlaque(
                root.transform,
                finaleSimPuzzle,
                "Night8_Chamber_RuinsFinalPlaque",
                new Vector3(9.5f, 3.65f, 103.4f),
                Quaternion.Euler(0f, 0f, 0f),
                EraVisibilityMode.OnlyRuins,
                "night8_final_act_testament_ruins",
                "Carta Póstuma de Mara Bell a Daniela y Mateo (Ruinas)",
                "Carta Póstuma de Mara Bell a Daniela y Mateo (Ruinas — Mitad B)",
                "«CÁMARA OCULTA Y QUIOSCO DEL LAGO (Zona 39 / Zona 59 — Actualidad):\n" +
                "\"A mi nieta Daniela y a Mateo, nieto de Julian Krol:\n" +
                "Guardé silencio toda mi vida porque Isadora me amenazó con destruir el único recuerdo de Elena. La invitación anónima que os trajo aquí nació de la propia cajita musical que quedó sonando entre dos tiempos.\n\n" +
                "Accionad simultáneamente el Atril de 1929 y el Atril de las Ruinas en esta cámara para interpretar el Compás 48 completo y unir ambas épocas al amanecer (06:00 h).\"»",
                new Color(0.55f, 0.85f, 1.0f));

            CreateSimultaneousSwitchNode(
                root.transform,
                finaleSimPuzzle,
                false,
                "Atril de Resonancia de Elena Vesper — Compás 48 (1929)",
                new Vector3(7.3f, 2.95f, 102.5f),
                new Color(0.98f, 0.82f, 0.38f));

            CreateSimultaneousSwitchNode(
                root.transform,
                finaleSimPuzzle,
                true,
                "Atril de Resonancia de Mara Bell y Julian Krol — Compás 48 (Ruinas)",
                new Vector3(9.3f, 2.95f, 102.5f),
                new Color(0.52f, 0.84f, 0.98f));
        }

        private void HandleTheatrePhaseAdvanced(int phase)
        {
            if (phase >= 5)
            {
                TriggerDawn0600Convergence("Clímax del Gran Teatro completado (Fase 5)");
            }
        }

        /// <summary>
        /// Ejecuta el desenlace canónico de las 06:00 AM (Parte 1.7 de la Biblia Narrativa y Bloque 3):
        /// registra el epílogo en el Diario Compartido y dispara la secuencia final sincronizada
        /// (Amanecer 06:00 + Fusión de Épocas + Epílogo + Estadísticas + Créditos + Vuelta al Menú).
        /// </summary>
        public void TriggerDawn0600Convergence(string triggerSource)
        {
            if (dawnConvergenceTriggered) return;
            dawnConvergenceTriggered = true;

            int totalJournalEntries = (SharedEvidenceJournalManager.Instance != null && SharedEvidenceJournalManager.Instance.AllEntries != null)
                ? SharedEvidenceJournalManager.Instance.AllEntries.Count
                : 0;
            bool fullSecretUnlocked = totalJournalEntries >= 14;

            if (dawnConvergenceVisualRoot == null)
            {
                dawnConvergenceVisualRoot = new GameObject("Dawn0600_ConvergenceLighting");
                dawnConvergenceVisualRoot.transform.position = new Vector3(8.3f, 5.2f, 103.0f);

                Light dawnLightChamber = dawnConvergenceVisualRoot.AddComponent<Light>();
                dawnLightChamber.type = LightType.Point;
                dawnLightChamber.range = 28f;
                dawnLightChamber.intensity = 2.6f;
                dawnLightChamber.color = new Color(1.0f, 0.86f, 0.62f);

                GameObject stageDawnObj = new GameObject("StageDawnLight");
                stageDawnObj.transform.SetParent(dawnConvergenceVisualRoot.transform, false);
                stageDawnObj.transform.position = new Vector3(-8.0f, 6.5f, 112.0f);
                Light dawnLightStage = stageDawnObj.AddComponent<Light>();
                dawnLightStage.type = LightType.Point;
                dawnLightStage.range = 42f;
                dawnLightStage.intensity = 2.4f;
                dawnLightStage.color = new Color(1.0f, 0.84f, 0.58f);
            }

            string epilogueTitle = fullSecretUnlocked
                ? "06:00 AM — EL AMANECER DE LA ACADEMIA VESPER (EPÍLOGO VERDADERO: «DOS CISNES EN LIBERTAD»)"
                : "06:00 AM — EL AMANECER DE LA ACADEMIA VESPER (DESENLACE: «EL ÚLTIMO ACTO COMPLETADO»)";

            string epilogueBody =
                "El reloj monumental de la torre da por fin las seis campanadas de la mañana (06:00 h). Por primera vez desde el 14 de octubre de 1929, el bucle se rompe.\n\n" +
                "Las dos capas del tiempo —el esplendor dorado de 1929 que veía Daniela y la ruina silenciosa que veía Mateo— convergen en una única mañana serena. Ya ninguno de los dos es una silueta de eco para el otro: se ven cara a cara en el mismo espacio y en el mismo presente.\n\n" +
                "Sobre el pedestal de la Cámara tras el Escenario (Zona 39) descansan el objeto de la invitación anónima: la Cajita Musical de Cisne de Elena Vesper y la Partitura Original del Compás 48 firmada por Julian Krol, Elena Vesper y Mara Bell.";

            if (SharedEvidenceJournalManager.Instance != null)
            {
                SharedEvidenceJournalManager.Instance.RecordPuzzleSolution(
                    "epilogo_amanecer_0600_academia_vesper",
                    epilogueTitle,
                    epilogueBody);
            }

            if (UltimateFinaleAndCreditsController.Instance != null)
            {
                UltimateFinaleAndCreditsController.Instance.RequestStartSynchronizedFinale(triggerSource);
            }
        }

        // =========================================================================================
        // 9. CATÁLOGO DE ANOMALÍAS ASIMÉTRICAS PARA LAS NOCHES II A VIII (PARTE 3)
        // =========================================================================================

        private void RegisterNights2To8AsymmetricAnomalies()
        {
            AsymmetricAnomalyManager mgr = AsymmetricAnomalyManager.Instance;
            if (mgr == null) return;

            // --- NOCHE II: LAS AULAS DE ENSAYO ---
            AnomalyEventData n2MirrorReflection = AnomalyEventData.CreateRuntime(
                id: "anomaly_n2_panel3_autonomous_reflection",
                night: 1,
                zone: "Zona 17 — Aula de Espejos (Panel n.º 3)",
                pos: new Vector3(-34.5f, 2.4f, 59.0f),
                radius: 6.5f,
                trigger: AnomalyTriggerType.EnterZone,
                vis: AnomalyAsymmetricVisibility.BothDifferent,
                pattern: AnomalyCatalogPattern.AutonomousReflection,
                duration: 5.0f,
                linkedDocId: "doc_s4_mirror_sequence_notes",
                title: "Anomalía Noche II — El Reflejo Autónomo del Panel n.º 3",
                toast1929: "En 1929 el Panel n.º 3 muestra una bailarina ejecutando el Plié y el Arabesque medio compás después.",
                toastRuins: "En las Ruinas el cristal roto del Panel n.º 3 se empaña desde dentro y muestra las marcas III y IV.",
                journal1929: "vio en 1929 la silueta de una bailarina dentro del Panel n.º 3 del Aula de Espejos ejecutando dos posturas autónomas.",
                journalRuins: "vio en las Ruinas cómo el cristal roto del Panel n.º 3 se empañaba desde dentro con las marcas III y IV y sonido de respiración.",
                freq1929: 262.0f,
                freqRuins: 196.0f);
            mgr.RegisterAnomalyEvent(n2MirrorReflection);

            AnomalyEventData n2InfirmaryShadow = AnomalyEventData.CreateRuntime(
                id: "anomaly_n2_infirmary_screen_shadow",
                night: 1,
                zone: "Zona 43 — Enfermería (Biombo Clínico)",
                pos: new Vector3(-31.0f, 2.4f, 2.0f),
                radius: 5.5f,
                trigger: AnomalyTriggerType.ApproachObject,
                vis: AnomalyAsymmetricVisibility.Only1929,
                pattern: AnomalyCatalogPattern.SingleEraFigure,
                duration: 4.5f,
                linkedDocId: "doc_n2_real_clinical_record",
                title: "Anomalía Noche II — Sombra tras el Biombo de la Enfermería",
                toast1929: "Tras el biombo de 1929 se proyecta la sombra de Isadora vertiendo tres gotas de Éter Dulce en una copa.",
                toastRuins: "",
                journal1929: "vio tras el biombo blanco de la Enfermería (solo en 1929) la silueta de Isadora Vale contando tres gotas de Éter Dulce en una copa.",
                journalRuins: "",
                freq1929: 220.0f,
                freqRuins: 220.0f);
            mgr.RegisterAnomalyEvent(n2InfirmaryShadow);

            // --- NOCHE III: EL INTERNADO ---
            AnomalyEventData n3DormMusicBox = AnomalyEventData.CreateRuntime(
                id: "anomaly_n3_dormitory_music_box_and_lattice",
                night: 2,
                zone: "Zonas 45 y 48 — Dormitorio de Internas y Celosía de Isadora",
                pos: new Vector3(-48.0f, 2.4f, 62.0f),
                radius: 8.0f,
                trigger: AnomalyTriggerType.EnterZone,
                vis: AnomalyAsymmetricVisibility.BothDifferent,
                pattern: AnomalyCatalogPattern.MelodyFromUnmappedRoom,
                duration: 5.5f,
                linkedDocId: "doc_n3_isadora_private_diary",
                title: "Anomalía Noche III — La Cajita Musical y la Celosía Oculta",
                toast1929: "En 1929 se oye el roce de un vestido de seda y respiración contenida detrás de la celosía de madera.",
                toastRuins: "En las Ruinas suena una cajita musical de cuerda bajo el catre vacío n.º 4 del dormitorio.",
                journal1929: "escuchó en 1929 el roce de un vestido de seda y respiración detrás de la celosía desde la que Isadora vigilaba a las internas.",
                journalRuins: "escuchó en las Ruinas una cajita musical tocando el tema del Cisne Negro bajo el catre vacío del dormitorio.",
                freq1929: 330.0f,
                freqRuins: 440.0f);
            mgr.RegisterAnomalyEvent(n3DormMusicBox);

            // --- NOCHE IV: ARCHIVO Y PARQUE INTERIOR ---
            AnomalyEventData n4FountainBallerinas = AnomalyEventData.CreateRuntime(
                id: "anomaly_n4_winter_garden_swan_fountain",
                night: 3,
                zone: "Zona 40 — Jardín de Invierno (Fuente del Cisne)",
                pos: new Vector3(68.0f, 0.0f, 56.0f),
                radius: 9.0f,
                trigger: AnomalyTriggerType.EnterZone,
                vis: AnomalyAsymmetricVisibility.BothDifferent,
                pattern: AnomalyCatalogPattern.CrossEraDisplacedObject,
                duration: 5.0f,
                linkedDocId: "doc_n4_restoration_note",
                title: "Anomalía Noche IV — La Fuente del Cisne y las Estatuas Giradas",
                toast1929: "En 1929 la Fuente del Cisne murmura agua mezclada con la nota Do Natural de un violonchelo en el sótano.",
                toastRuins: "En las Ruinas la fuente está seca, pero las estatuas de piedra han girado la cabeza hacia el Gran Teatro.",
                journal1929: "escuchó en 1929 agua corriendo en la Fuente del Cisne junto con la nota grave Do Natural procedente del sótano.",
                journalRuins: "vio en las Ruinas cómo las estatuas de bailarinas del Jardín de Invierno habían girado la cabeza hacia la entrada del Gran Teatro.",
                freq1929: 130.81f,
                freqRuins: 261.63f);
            mgr.RegisterAnomalyEvent(n4FountainBallerinas);

            // --- NOCHE V: LO ALTO Y LO PROFUNDO ---
            AnomalyEventData n5ClockBellAnomaly = AnomalyEventData.CreateRuntime(
                id: "anomaly_n5_clock_tower_frozen_chime",
                night: 4,
                zone: "Zona 52 — Torre del Reloj (Planta Alta)",
                pos: new Vector3(0.0f, 7.70f, 78.0f),
                radius: 8.5f,
                trigger: AnomalyTriggerType.EnterZone,
                vis: AnomalyAsymmetricVisibility.BothDifferent,
                pattern: AnomalyCatalogPattern.AsymmetricDoorVisualState,
                duration: 5.0f,
                linkedDocId: "night5_clock_crypt_list_1929",
                title: "Anomalía Noche V — La Campanada Congelada de las 22:17 h",
                toast1929: "En 1929 el péndulo de la Torre del Reloj se clava en seco a las 22:17 h con el golpe de una cuña de bronce.",
                toastRuins: "En las Ruinas la campana oxidada y sin badajo resuena sola mientras las sombras de los engranajes giran hacia atrás.",
                journal1929: "vio en 1929 el péndulo de la Torre del Reloj detenerse exactamente a las 22:17 h por una cuña metálica.",
                journalRuins: "escuchó en las Ruinas la campana sin badajo de la torre resonar por sí sola mientras las agujas retrocedían.",
                freq1929: 146.83f,
                freqRuins: 110.0f);
            mgr.RegisterAnomalyEvent(n5ClockBellAnomaly);

            // --- NOCHE VI: BAMBALINAS ---
            AnomalyEventData n6DressingRoomMannequin = AnomalyEventData.CreateRuntime(
                id: "anomaly_n6_elena_dressing_room_mannequin_shift",
                night: 5,
                zone: "Zonas 26 y 27 — Camerinos de Mara y Elena",
                pos: new Vector3(33.5f, 2.4f, 87.5f),
                radius: 7.5f,
                trigger: AnomalyTriggerType.EnterZone,
                vis: AnomalyAsymmetricVisibility.BothDifferent,
                pattern: AnomalyCatalogPattern.CrossEraDisplacedObject,
                duration: 5.0f,
                linkedDocId: "night6_backstage_order_1929",
                title: "Anomalía Noche VI — El Maniquí Desplazado entre Camerinos",
                toast1929: "En 1929 el maniquí del Cisne Negro está junto al espejo de Elena y una voz susurra: «Cambiaremos en el Compás 48».",
                toastRuins: "En las Ruinas el camerino de Elena está vacío y ese mismo maniquí aparece frente al tocador de Mara Bell.",
                journal1929: "vio en 1929 el maniquí del Cisne Negro en el camerino de Elena Vesper mientras una voz susurraba el intercambio del Compás 48.",
                journalRuins: "vio en las Ruinas que el maniquí del Cisne Negro ya no estaba en el camerino de Elena, sino frente al espejo de Mara Bell.",
                freq1929: 246.94f,
                freqRuins: 196.0f);
            mgr.RegisterAnomalyEvent(n6DressingRoomMannequin);

            // --- NOCHE VII: EL LAGO DE LAS SOMBRAS ---
            AnomalyEventData n7EmptyCurtainApplause = AnomalyEventData.CreateRuntime(
                id: "anomaly_n7_auditorium_applause_behind_curtain",
                night: 6,
                zone: "Zonas 31 y 32 — Patio de Butacas y Escenario del Gran Teatro",
                pos: new Vector3(-8.0f, 2.4f, 124.0f),
                radius: 10.0f,
                trigger: AnomalyTriggerType.EnterZone,
                vis: AnomalyAsymmetricVisibility.BothDifferent,
                pattern: AnomalyCatalogPattern.ApplauseBehindEmptyCurtain,
                duration: 6.0f,
                linkedDocId: "night7_theatre_cues_part1_1929",
                title: "Anomalía Noche VII — Aplausos tras el Telón Vacío y Golpes en el Espejo",
                toast1929: "En 1929 el Patio de Butacas está lleno de siluetas de gala inmóviles mientras suenan dos golpes secos tras el espejo.",
                toastRuins: "En las Ruinas las butacas están vacías, pero resuenan aplausos tras el telón cerrado y un golpe dentro del Espejo Central.",
                journal1929: "vio en 1929 el Patio de Butacas lleno de figuras en traje de gala en silencio mientras sonaban dos golpes desde el interior del espejo.",
                journalRuins: "escuchó en las Ruinas una ovación procedente de detrás del telón vacío seguida de un golpe de auxilio dentro del Espejo Central.",
                freq1929: 196.0f,
                freqRuins: 164.81f);
            mgr.RegisterAnomalyEvent(n7EmptyCurtainApplause);

            // --- NOCHE VIII: ÚLTIMO ACTO ---
            AnomalyEventData n8FrozenPondFootsteps = AnomalyEventData.CreateRuntime(
                id: "anomaly_n8_frozen_pond_and_chamber_duet",
                night: 7,
                zone: "Zona 59 y Zona 39 — Estanque Helado y Cámara tras el Escenario",
                pos: new Vector3(8.3f, 2.4f, 104.0f),
                radius: 9.0f,
                trigger: AnomalyTriggerType.EnterZone,
                vis: AnomalyAsymmetricVisibility.Both,
                pattern: AnomalyCatalogPattern.AutonomousReflection,
                duration: 5.5f,
                linkedDocId: "night8_final_act_score_1929",
                title: "Anomalía Noche VIII — El Dúo Final en el Espejo de las 06:00 h",
                toast1929: "Las siluetas del Cisne Blanco y del Cisne Negro aguardan juntas frente a los dos atriles de la Cámara tras el Escenario.",
                toastRuins: "Las siluetas del Cisne Blanco y del Cisne Negro aguardan juntas frente a los dos atriles de la Cámara tras el Escenario.",
                journal1929: "vio en la Cámara tras el Escenario las siluetas de Elena Vesper y Mara Bell reflejadas juntas aguardando el acorde de las 06:00 h.",
                journalRuins: "vio en la Cámara tras el Escenario las siluetas de Elena Vesper y Mara Bell reflejadas juntas aguardando el acorde de las 06:00 h.",
                freq1929: 329.63f,
                freqRuins: 329.63f);
            mgr.RegisterAnomalyEvent(n8FrozenPondFootsteps);
        }

        // =========================================================================================
        // HELPERS VISUALES Y DE NODOS COOPERATIVOS
        // =========================================================================================

        private static void CreateEraCluePlaque(
            Transform parent,
            CoopPuzzleBase puzzle,
            string name,
            Vector3 worldPos,
            Quaternion worldRot,
            EraVisibilityMode visibilityMode,
            string clueSubId,
            string promptTitle,
            string docTitle,
            string docBody,
            Color accentColor)
        {
            GameObject plaqueObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plaqueObj.name = name;
            plaqueObj.transform.SetParent(parent, true);
            plaqueObj.transform.position = worldPos;
            plaqueObj.transform.rotation = worldRot;
            plaqueObj.transform.localScale = new Vector3(0.75f, 0.52f, 0.06f);

            Renderer rend = plaqueObj.GetComponent<Renderer>();
            if (rend != null)
            {
                Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                Material mat = new Material(sh);
                mat.color = accentColor * 0.65f;
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", accentColor * 0.25f);
                }
                rend.sharedMaterial = mat;
            }

            CoopEraCluePlaque plaque = plaqueObj.AddComponent<CoopEraCluePlaque>();
            plaque.Configure(
                puzzle,
                clueSubId,
                visibilityMode,
                promptTitle,
                docTitle,
                docBody);
        }

        private static void CreateTurnNode(
            Transform parent,
            CoopTurnSequencePuzzle puzzle,
            int stepIndex,
            string label,
            EraVisibilityMode eraMode,
            Vector3 worldPos,
            Color color)
        {
            GameObject nodeObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            nodeObj.name = $"TurnNode_Step{stepIndex + 1}_{(eraMode == EraVisibilityMode.OnlyRuins ? "Ruins" : "1929")}";
            nodeObj.transform.SetParent(parent, true);
            nodeObj.transform.position = worldPos;
            nodeObj.transform.localScale = new Vector3(0.32f, 0.45f, 0.32f);

            Renderer rend = nodeObj.GetComponent<Renderer>();
            if (rend != null)
            {
                Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                Material mat = new Material(sh);
                mat.color = color;
                rend.sharedMaterial = mat;
            }

            CoopTurnSequenceNode node = nodeObj.AddComponent<CoopTurnSequenceNode>();
            node.Configure(puzzle, stepIndex, eraMode, label);
        }

        private static void CreateSimultaneousSwitchNode(
            Transform parent,
            CoopSimultaneousSwitchPuzzle puzzle,
            bool isRuinsSwitch,
            string label,
            Vector3 worldPos,
            Color color)
        {
            GameObject switchObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            switchObj.name = $"SimultaneousSwitch_{(isRuinsSwitch ? "Ruins" : "1929")}";
            switchObj.transform.SetParent(parent, true);
            switchObj.transform.position = worldPos;
            switchObj.transform.localScale = new Vector3(0.36f, 0.58f, 0.24f);

            Renderer rend = switchObj.GetComponent<Renderer>();
            if (rend != null)
            {
                Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                Material mat = new Material(sh);
                mat.color = color;
                rend.sharedMaterial = mat;
            }

            GameObject handleObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handleObj.name = "LeverHandle";
            handleObj.transform.SetParent(switchObj.transform, false);
            handleObj.transform.localPosition = new Vector3(0f, 0.1f, 0.35f);
            handleObj.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
            handleObj.transform.localScale = new Vector3(0.22f, 0.55f, 0.22f);
            Collider handleCol = handleObj.GetComponent<Collider>();
            if (handleCol != null) Destroy(handleCol);

            CoopSimultaneousSwitchNode node = switchObj.AddComponent<CoopSimultaneousSwitchNode>();
            node.Configure(puzzle, isRuinsSwitch, label);
        }

        private static GameObject CreateRewardPedestal(
            Transform parent,
            string name,
            Vector3 worldPos)
        {
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pedestal.name = name;
            pedestal.transform.SetParent(parent, true);
            pedestal.transform.position = worldPos + new Vector3(0f, 0.55f, 0f);
            pedestal.transform.localScale = new Vector3(0.65f, 0.28f, 0.45f);

            Renderer rend = pedestal.GetComponent<Renderer>();
            if (rend != null)
            {
                Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                Material mat = new Material(sh);
                mat.color = new Color(0.85f, 0.68f, 0.28f);
                rend.sharedMaterial = mat;
            }

            pedestal.SetActive(false);
            return pedestal;
        }

        private static GameObject CreateRuinsRubbleBlocker(
            Transform parent,
            string name,
            Vector3 worldPos)
        {
            GameObject rubble = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rubble.name = name;
            rubble.transform.SetParent(parent, true);
            rubble.transform.position = worldPos;
            rubble.transform.localScale = new Vector3(1.35f, 1.15f, 0.85f);

            Renderer rend = rubble.GetComponent<Renderer>();
            if (rend != null)
            {
                Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                Material mat = new Material(sh);
                mat.color = new Color(0.32f, 0.30f, 0.28f);
                rend.sharedMaterial = mat;
            }

            return rubble;
        }

        private static GameObject CreateCrossEraLeverVisual(
            Transform parent,
            string name,
            Vector3 worldPos)
        {
            GameObject lever = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lever.name = name;
            lever.transform.SetParent(parent, true);
            lever.transform.position = worldPos;
            lever.transform.localScale = new Vector3(0.28f, 0.55f, 0.28f);

            Renderer rend = lever.GetComponent<Renderer>();
            if (rend != null)
            {
                Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                Material mat = new Material(sh);
                mat.color = new Color(0.88f, 0.68f, 0.28f);
                rend.sharedMaterial = mat;
            }

            Collider col = lever.GetComponent<Collider>();
            if (col != null) Destroy(col);

            return lever;
        }
    }
}
