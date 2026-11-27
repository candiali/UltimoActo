using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UltimoActo.Core;
using UltimoActo.Interaction;
using UltimoActo.Player;

namespace UltimoActo.Narrative
{
    /// <summary>
    /// PASO C — Noche I Completa ("La Llegada"): Documentos Complementarios, Anomalías Asimétricas y Revelación.
    /// - Configura todos los documentos y listas de la Noche I (1929 vs. Actualidad/Ruinas) con el canon oficial
    ///   de la Biblia Narrativa (Academia Vesper, Octubre de 1929, Julián Vesper y Valeria Bell).
    /// - Implementa las LISTAS COMPLEMENTARIAS entre épocas para que los dos jugadores tengan que hablarse
    ///   entre sí por voz para entender y resolver cada misterio:
    ///   1. Lista de Taquillas y Fusibles dividida: En 1929 el cuadro de recepción indica los números de taquilla
    ///      (Nº 07 y Nº 14) y el amperaje de los fusibles pero con seudónimos; en las Ruinas el registro del
    ///      vestuario asigna los nombres reales (Mara Bell = Nº 07, Elena Vesper = Nº 14).
    ///   2. Lista de las Cuatro Prima Donnas dividida: En 1929 el Programa Litúrgico conserva legibles las
    ///      estaciones de Beatrice Dupont (IV — Invierno) y Clara Valmont (I — Primavera); en las Ruinas,
    ///      una nota restaurada conserva la mitad inferior con Teresa Vance (III — Otoño) y Elena Vesper (II — Verano).
    ///   3. Contradicción central de la Noche I: El Libro de Recepción de 1929 certifica que Elena desapareció y
    ///      abandonó el edificio la noche del 14 de octubre de 1929, mientras que en su Habitación Privada en las
    ///      Ruinas aparece su carta manuscrita fechada el 16 de octubre de 1929 ("Hoy es 16 de octubre y sigo aquí").
    /// - Registra en AsymmetricAnomalyManager las 3 anomalías asimétricas de la Noche I con sus elementos visuales
    ///   diferenciados por época y su entrada automática en el Diario Compartido ([J]/[TAB]).
    /// </summary>
    public class Night1NarrativeAndAnomaliesSetup : MonoBehaviour
    {
        public static Night1NarrativeAndAnomaliesSetup Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoadedBootstrap;
            SceneManager.sceneLoaded += OnSceneLoadedBootstrap;
            TrySpawnInActiveScene();
        }

        private static void OnSceneLoadedBootstrap(Scene scene, LoadSceneMode mode)
        {
            TrySpawnInActiveScene();
        }

        private static void TrySpawnInActiveScene()
        {
            if (Instance != null) return;
            if (FindFirstObjectByType<Night1NarrativeAndAnomaliesSetup>() != null) return;
            if (FindFirstObjectByType<PlayerMovement>() == null) return;

            GameObject go = new GameObject("Night1_NarrativeAndAnomalies_Setup");
            go.AddComponent<Night1NarrativeAndAnomaliesSetup>();
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

        private void Start()
        {
            ConfigureNight1DocumentsAndComplementaryLists();
            RegisterNight1AsymmetricAnomalies();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ====================================================================
        // 1. DOCUMENTOS Y LISTAS COMPLEMENTARIAS DE LA NOCHE I (1929 ↔ RUINAS)
        // ====================================================================
        private void ConfigureNight1DocumentsAndComplementaryLists()
        {
            float vestY = 2.4f;
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            Material matPaper1929 = new Material(litShader) { color = new Color(0.95f, 0.89f, 0.74f) };
            Material matPaperRuins = new Material(litShader) { color = new Color(0.62f, 0.66f, 0.72f) };

            // A. ZONA 3 (PÓRTICO): Edicto Oficial de Clausura (Visible en Ruinas) + Programa de Mano de Gala (Visible en 1929)
            UpdateExistingDoc(
                docId: "doc_entrance_edict_1924",
                newTitle: "Edicto Oficial de Clausura — Academia Vesper (Noviembre 1929)",
                newSubtitle: "Zona 3 (Pórtico Monumental) · Documento de la Actualidad / Ruinas",
                newContent:
                    "ORDEN MUNICIPAL Y DE LA JUNTA DEL PATRONATO — ACADEMIA VESPER\n\n" +
                    "Por disposición oficial y de la Directora Artística Isadora Vale, la Academia Vesper queda CLAUSURADA tras la interrupción del estreno de «El Lago de las Sombras» la noche del 14 de octubre de 1929 y la desaparición de la primera bailarina Elena Vesper.\n\n" +
                    "Los inspectores no hallaron rastro de la solista fuera del recinto. Todo el archivo documental y los registros del elenco han quedado bajo llave en el Ala de Administración (oeste) y los Vestuarios (este).\n\n" +
                    "— Nota manuscrita en el reverso de tu invitación anónima:\n" +
                    "«Julián (1929) y Valeria (Ruinas): Cada uno ve solo la mitad de los registros. Hablad entre vosotros para cruzar las listas».",
                visibility: EraVisibilityMode.Both);

            // Crear Programa de Mano de la Gala de Estreno (Solo visible en 1929, en el Pórtico junto a la entrada)
            CreateEraSpecificWorldDoc(
                name: "Doc_N1_Programa_Gala_1929",
                pos: new Vector3(-2.85f, vestY + 1.15f, 6.2f),
                scale: new Vector3(0.42f, 0.05f, 0.32f),
                mat: matPaper1929,
                docId: "doc_n1_programa_gala_1929",
                title: "Programa de Mano — Gran Estreno de «El Lago de las Sombras» (14 de Octubre de 1929)",
                subtitle: "Zona 3 (Pórtico Monumental) · SOLO VISIBLE EN 1929 (Julián Vesper)",
                content:
                    "ACADEMIA VESPER — TEMPORADA DE OTOÑO DE 1929 (20:15 H)\n" +
                    "Dirección y Coreografía: Isadora Vale · Maestro Concertador: Viktor Krol\n\n" +
                    "• **Cisne Negro (Solista Principal):** Elena Vesper (Alumna Nº 142)\n" +
                    "• **Cisne Blanco (Primera Suplente):** Mara Bell (Alumna Nº 108)\n\n" +
                    "NOTA ESCENOGRÁFICA DE LA VELADA:\n" +
                    "«Para el Acto Final se estrena el gran Arco de Espejo Escénico en el fondo del proscenio. A las 20:15 h en punto se cerrarán las puertas del vestíbulo».",
                visibility: EraVisibilityMode.Only1929);

            // B. ZONA 6 (RECEPCIÓN ANTIGUA): Libro Diario de Recepción (1929) — Primera mitad de la contradicción de fechas
            // + Parte 1 de la Lista de Fusibles y Taquillas (por código de alumna, sin nombres propios)
            UpdateExistingDoc(
                docId: "doc_reception_log_1924",
                newTitle: "Libro Oficial de Recepción — Noche del Estreno (14 de Octubre de 1929) [Mitad 1929]",
                newSubtitle: "Zona 6 (Recepción Antigua) · SOLO VISIBLE EN 1929 · Compáralo con lo que vea tu compañero en Ruinas",
                newContent:
                    "REGISTRO DE GUARDIA DE RECEPCIÓN — 14 DE OCTUBRE DE 1929 (22:30 H)\n\n" +
                    "1. **Declaración Oficial firmada por Isadora Vale:**\n" +
                    "   «Hago constar que a las 22:08 h, tras un golpe accidental en el bastidor del escenario antes del último acto, la primera bailarina **Elena Vesper abandonó el edificio y desapareció en la noche del 14 de octubre**. Nadie queda ya en las habitaciones privadas».\n\n" +
                    "2. **Cuadro de Mantenimiento Eléctrico (Lista de 1929 — Parte A):**\n" +
                    "   Para restaurar los 220V del cierre del Archivo se requieren 3 fusibles cerámicos:\n" +
                    "   • **Fusible de 10A:** Guardado en el banco central del Vestuario Femenino.\n" +
                    "   • **Fusible de 15A:** Custodiado en la **Taquilla de la Primera Suplente (Cisne Blanco)**.\n" +
                    "   • **Fusible de 25A:** Custodiado en el archivador del Despacho de Dirección.\n" +
                    "   *(En este libro de 1929 no figura qué número de taquilla corresponde a cada bailarina; pregúntale a tu compañero en las Ruinas qué dice la lista del vestuario).* ",
                visibility: EraVisibilityMode.Only1929);

            // Crear en Zona 6 (Recepción en Ruinas) el Informe Forense de Recepción (Solo visible en Ruinas)
            CreateEraSpecificWorldDoc(
                name: "Doc_N1_Acta_Inspeccion_Recepcion_Ruins",
                pos: new Vector3(-15.0f, vestY + 1.28f, 16.0f),
                scale: new Vector3(0.46f, 0.05f, 0.34f),
                mat: matPaperRuins,
                docId: "doc_n1_inspeccion_recepcion_ruins",
                title: "Acta de Inspección tras la Clausura — Recepción (Noviembre de 1929) [Mitad Ruinas]",
                subtitle: "Zona 6 (Recepción Antigua) · SOLO VISIBLE EN RUINAS (Valeria Bell)",
                content:
                    "INFORME DE LA COMISIÓN DE CLAUSURA — ACADEMIA VESPER\n\n" +
                    "• Sobre el mostrador quedó abierto el Libro de Recepción donde la Directora Vale afirmó que Elena Vesper huyó a la calle a las 22:08 h del **14 de octubre de 1929**.\n" +
                    "• Sin embargo, el guarda del portón exterior juró que **las verjas estuvieron cerradas con candado desde las 20:15 h hasta la mañana del día 15** y que nadie salió a la calle.\n\n" +
                    "• **Nota sobre las llaves internas:** La llave de la Habitación Privada de la Solista nunca apareció en Dirección; los inspectores sospechan que sigue oculta en la **Taquilla Personal de Elena Vesper** (en la Sala 13).",
                visibility: EraVisibilityMode.OnlyRuins);

            // C. ZONA 11 (VESTUARIO FEMENINO): Lista Oficial de Alumnas y Taquillas (SOLO VISIBLE EN RUINAS)
            // Complementa al Libro de Recepción de 1929 diciendo qué número de taquilla tiene Mara (15A) y cuál tiene Elena (Llave 2)
            UpdateExistingDoc(
                docId: "doc_s3_student_list",
                newTitle: "Asignación de Taquillas del Vestuario (Octubre de 1929) [Mitad Ruinas]",
                newSubtitle: "Zona 11 (Vestuario Femenino) · SOLO VISIBLE EN RUINAS · Cruza esta lista con la de Recepción de 1929",
                newContent:
                    "TABLÓN INTERNO DE VESTUARIO — ACADEMIA VESPER (OCTUBRE 1929)\n" +
                    "*(Parte B de la lista — Comunícale estos números por voz a tu compañero de 1929)*:\n\n" +
                    "• **Taquilla Nº 03** -> Sofía Lindqvist (Cuerpo de baile)\n" +
                    "• **Taquilla Nº 07** -> **Mara Bell (Primera Suplente — Cisne Blanco)**\n" +
                    "• **Taquilla Nº 11** -> Clara Valmont (Corifeo)\n" +
                    "• **Taquilla Nº 14** -> **Elena Vesper (Primera Bailarina — Cisne Negro)**\n\n" +
                    "Observación anotada a lápiz en las ruinas:\n" +
                    "«Si tu compañero en 1929 leyó en Recepción qué guardaba la Primera Suplente y qué guardaba la Solista Principal, ya sabéis exactamente qué buscar en las **Taquillas Nº 07 y Nº 14** de la Sala 13».",
                visibility: EraVisibilityMode.OnlyRuins);

            // Coleccionable personal opcional en Zona 11: Zapatillas de punta de Elena
            UpdateExistingDoc(
                docId: "doc_s3_ballet_slippers",
                newTitle: "[Coleccionable Personal] Zapatillas de Punta de Elena Vesper (1929)",
                newSubtitle: "Zona 11 (Vestuario Femenino) · Recuerdo de las Bailarinas",
                newContent:
                    "Un par de zapatillas de satén desgastadas por los ensayos de «El Lago de las Sombras». En la suela de cuero se lee: «Elena Vesper — Cisne Negro (Octubre 1929)».\n\n" +
                    "Las cintas de sujeción fueron cortadas limpiamente con tijeras antes del último acto. Dentro de la puntera izquierda hay una esquela doblada:\n" +
                    "«Alguien entró a mi tocador mientras ensayábamos en el escenario. He cerrado mi habitación del ala norte con llave y la dejé en mi taquilla personal».",
                visibility: EraVisibilityMode.Both);

            // D. ZONA 7 (OFICINA DE DIRECCIÓN) y ZONA 8 (ARCHIVO DE EXPEDIENTES):
            UpdateExistingDoc(
                docId: "doc_isadora_letter",
                newTitle: "Carta Confidencial de Isadora Vale al Patronato (12 de Octubre de 1929)",
                newSubtitle: "Zona 7 (Oficina de Dirección) · Visible en Ambas Épocas",
                newContent:
                    "A la Junta Reservada de la Academia Vesper:\n\n" +
                    "«Las anomalías durante los ensayos nocturnos de 'El Lago de las Sombras' inquietan al elenco: las alumnas aseguran que los espejos devuelven figuras que no están en la sala y sus recuerdos del último acto no coinciden.\n\n" +
                    "Para el estreno del 14 de octubre he dispuesto el mecanismo definitivo tras el espejo del escenario. Si Elena Vesper intenta alterar mi desenlace, Mara Bell ocupará su lugar bajo el foco y los expedientes quedarán sellados en el Archivo».\n\n" +
                    "— Isadora Vale, Directora Artística (1929)",
                visibility: EraVisibilityMode.Both);

            UpdateExistingDoc(
                docId: "doc_elena_file",
                newTitle: "Expediente Confidencial Nº 142: Elena Vesper (1929) [Mitad 1929]",
                newSubtitle: "Zona 8 (Archivo de Expedientes) · SOLO VISIBLE EN 1929 (Julián Vesper)",
                newContent:
                    "ARCHIVO DE DIRECCIÓN — ACADEMIA VESPER (OCTUBRE DE 1929)\n\n" +
                    "• **Alumna:** Elena Vesper · **Papel:** Cisne Negro.\n" +
                    "• **Observación de Isadora Vale (11 de Octubre de 1929):**\n" +
                    "  «Elena sospecha que hemos modificado el compás 48 de la partitura y el bastidor del espejo escénico. Afirma que su reflejo en el Tocador Nº 3 se mueve con retraso».\n\n" +
                    "• **Orden:** Retirar su placa de coronación y mantener bajo llave la puerta del Aula de Ballet tras el Pasillo de los Retratos.",
                visibility: EraVisibilityMode.Only1929);

            UpdateExistingDoc(
                docId: "doc_mara_bell",
                newTitle: "Declaración de Mara Bell sobre los Ensayos (Octubre de 1929) [Mitad Ruinas]",
                newSubtitle: "Zona 8 (Archivo de Expedientes) · SOLO VISIBLE EN RUINAS (Valeria Bell)",
                newContent:
                    "CARPETA ANEXA AL EXPEDIENTE — RECUPERADA EN LAS RUINAS DEL ARCHIVO\n\n" +
                    "Testimonio manuscrito de Mara Bell (Cisne Blanco):\n" +
                    "«Dicen que yo quería quitarle el puesto a Elena, pero mienten. En los ensayos del 10 de octubre ninguna de nosotras recordaba lo mismo al salir del escenario: yo recordaba que la música se paraba antes del cuarto acto, y Elena juraba haber oído un golpe detrás del espejo.\n\n" +
                    "Elena escondió la verdad sobre la fotografía alterada del pasillo en el doble fondo del escritorio de su Habitación Privada (Zona 14)».",
                visibility: EraVisibilityMode.OnlyRuins);

            // E. ZONA 9 (SALA DE ESPERA) y ZONA 10 (PASILLO DE LOS RETRATOS):
            // LISTA COMPLEMENTARIA DE LAS 4 PRIMA DONNAS (Mitad A en 1929 + Mitad B en Ruinas)
            UpdateExistingDoc(
                docId: "doc_s1_seasons_portrait_clue",
                newTitle: "Registro de Coronaciones Estacionales — Parte I (1898–1912) [Solo 1929]",
                newSubtitle: "Zona 9 (Sala de Espera) · SOLO VISIBLE EN 1929 · Tu compañero en Ruinas tiene la Parte II",
                newContent:
                    "LIBRO DE HONOR DE LAS PRIMA DONNAS — ACADEMIA VESPER (PARTE I: 1929)\n" +
                    "*(Esta página intacta de 1929 contiene las dos primeras coronaciones del Pasillo de los Retratos. Díselas por voz a tu compañero y pídele las dos restantes)*:\n\n" +
                    "1. ** Primer Retrato — Beatrice Dupont (1898):**\n" +
                    "   Coronada bajo la escarcha del **INVIERNO [Dial en posición IV]**.\n\n" +
                    "2. **Segundo Retrato — Clara Henschel (1912):**\n" +
                    "   Coronada en el florecer de la **PRIMAVERA [Dial en posición I]**.\n\n" +
                    "3. **Tercer Retrato — Teresa Vane (1920):** *[Página aún sin encuadernar en esta edición — Consultar archivo en las Ruinas]*.\n" +
                    "4. **Cuarto Retrato — Elena Vesper (1929):** *[Consultar archivo en las Ruinas]*.",
                visibility: EraVisibilityMode.Only1929);

            // Crear la Parte II de la lista de las Prima Donnas (SOLO VISIBLE EN RUINAS, en la Sala de Espera)
            CreateEraSpecificWorldDoc(
                name: "Doc_N1_Coronaciones_Parte2_Ruins",
                pos: new Vector3(-16.8f, vestY + 0.64f, 29.8f),
                scale: new Vector3(0.36f, 0.04f, 0.28f),
                mat: matPaperRuins,
                docId: "doc_n1_seasons_portrait_clue_part2_ruins",
                title: "Registro de Coronaciones Estacionales — Parte II (1920–1929) [Solo Ruinas]",
                subtitle: "Zona 9 (Sala de Espera) · SOLO VISIBLE EN RUINAS · Tu compañero en 1929 tiene la Parte I",
                content:
                    "HOJA DESPRENDIDA DEL LIBRO DE HONOR — ACADEMIA VESPER (PARTE II: RUINAS)\n" +
                    "*(La mitad superior del pliego fue destruida por la humedad, pero se conservan intactas las dos últimas coronaciones del Pasillo de los Retratos)*:\n\n" +
                    "1. **Primer Retrato — Beatrice Dupont (1898):** *[Ilegible por el moho — Pregunta a tu compañero en 1929]*.\n" +
                    "2. **Segundo Retrato — Clara Henschel (1912):** *[Ilegible por el moho — Pregunta a tu compañero en 1929]*.\n\n" +
                    "3. **Tercer Retrato — Teresa Vane (1920):**\n" +
                    "   Coronada entre las hojas doradas del **OTOÑO [Dial en posición III]**.\n\n" +
                    "4. **Cuarto Retrato — Elena Vesper (1929):**\n" +
                    "   Coronada en la noche del Solsticio de **VERANO [Dial en posición II]**.",
                visibility: EraVisibilityMode.OnlyRuins);

            // F. ZONA 12 (TOCADORES) y ZONA 14 (HABITACIÓN DE ELENA):
            // La Revelación Principal y Contradicción Central de la Noche I (14 de Octubre vs 16 de Octubre de 1929)
            UpdateExistingDoc(
                docId: "doc_s3_mirror_back_note",
                newTitle: "Nota Oculta tras el Espejo del Tocador Nº 3 (13 de Octubre de 1929)",
                newSubtitle: "Zona 12 (Sala de Tocadores) · Visible en Ambas Épocas",
                newContent:
                    "Escrito con lápiz negro detrás del marco ovalado del tocador de Elena Vesper:\n\n" +
                    "«13 de Octubre de 1929 — Anoche las bombillas del tocador parpadearon y mi reflejo en el cristal tardó un segundo más que yo en bajar los brazos.\n\n" +
                    "He guardado la llave de mi Habitación Privada dentro de mi taquilla personal (consulta la lista combinada de Recepción y Vestuario para saber mi número)».",
                visibility: EraVisibilityMode.Both);

            UpdateExistingDoc(
                docId: "doc_s3_elena_incomplete_letter",
                newTitle: "Carta Manuscrita de Elena Vesper (16 de Octubre de 1929) — [REVELACIÓN NOCHE I]",
                newSubtitle: "Zona 14 (Habitación Privada de Elena) · contradice el Libro de Recepción del 14 de Octubre",
                newContent:
                    "MANUSCRITO FECHADO EL **16 DE OCTUBRE DE 1929** (Dos días DESPUÉS de su supuesta desaparición oficial el 14 de octubre):\n\n" +
                    "«Si tienes esta carta en tus manos y tu compañero ha leído el Libro de Recepción de 1929, ya habéis descubierto la primera gran mentira de Isadora Vale.\n\n" +
                    "Isadora escribió en recepción que desaparecí y hui del edificio la noche del estreno (14 de octubre a las 22:08 h). **Pero hoy es 16 de octubre de 1929 y sigo encerrada dentro de la Academia Vesper**.\n\n" +
                    "La noche del estreno la música se detuvo antes del último acto y se oyó aquel golpe detrás del telón. No fue un accidente. En el cajón de fondo falso de este escritorio dejé la fotografía original que demuestra lo que había detrás de mí en el escenario».",
                visibility: EraVisibilityMode.Both);

            UpdateExistingDoc(
                docId: "doc_s3_contradictory_photo",
                newTitle: "Placa Fotográfica sin Retocar (1929) — Los Dos Cisnes Negros",
                newSubtitle: "Zona 14 (Cajón Secreto de Elena) · Fotografía de Evidencia",
                newContent:
                    "Oculta en el doble fondo del escritorio de Elena Vesper:\n\n" +
                    "Es la placa fotográfica original tomada durante el ensayo general de «El Lago de las Sombras» en octubre de 1929.\n\n" +
                    "• **En el cuadro oficial de 1929:** Elena aparece bailando completamente sola.\n" +
                    "• **En esta fotografía original sin retocar:** Detrás de Elena, reflejada en el gran espejo del escenario, **hay una segunda figura vestida con un traje idéntico de Cisne Negro**.\n\n" +
                    "Anotación al dorso: «Isadora mandó pintar encima el 15 de octubre para borrar el espejo y a la segunda figura».",
                visibility: EraVisibilityMode.Both);
        }

        private static void UpdateExistingDoc(
            string docId,
            string newTitle,
            string newSubtitle,
            string newContent,
            EraVisibilityMode visibility)
        {
            WorldDocument[] allDocs = FindObjectsByType<WorldDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < allDocs.Length; i++)
            {
                if (allDocs[i] != null && string.Equals(allDocs[i].Id, docId, StringComparison.OrdinalIgnoreCase))
                {
                    allDocs[i].ConfigureDocument(docId, newTitle, newContent, newSubtitle);
                    if (visibility != EraVisibilityMode.Both)
                    {
                        EraOnlyVisibility eov = allDocs[i].GetComponent<EraOnlyVisibility>();
                        if (eov == null) eov = allDocs[i].gameObject.AddComponent<EraOnlyVisibility>();
                        eov.Configure(visibility, true);
                    }
                    break;
                }
            }
        }

        private void CreateEraSpecificWorldDoc(
            string name,
            Vector3 pos,
            Vector3 scale,
            Material mat,
            string docId,
            string title,
            string subtitle,
            string content,
            EraVisibilityMode visibility)
        {
            GameObject docObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            docObj.name = name;
            docObj.transform.SetParent(transform, false);
            docObj.transform.position = pos;
            docObj.transform.localScale = scale;

            Renderer rend = docObj.GetComponent<Renderer>();
            if (rend != null && mat != null) rend.sharedMaterial = mat;

            WorldDocument wd = docObj.AddComponent<WorldDocument>();
            wd.ConfigureDocument(docId, title, content, subtitle);

            if (visibility != EraVisibilityMode.Both)
            {
                EraOnlyVisibility eov = docObj.AddComponent<EraOnlyVisibility>();
                eov.Configure(visibility, true);
            }
        }

        // ====================================================================
        // 2. CATÁLOGO DE ANOMALÍAS ASIMÉTRICAS DE LA NOCHE I
        // ====================================================================
        private void RegisterNight1AsymmetricAnomalies()
        {
            AsymmetricAnomalyManager mgr = AsymmetricAnomalyManager.Instance;
            if (mgr == null) return;

            float vestY = 2.4f;
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            Material matGoldGhost = new Material(litShader) { color = new Color(0.95f, 0.82f, 0.48f) };
            matGoldGhost.EnableKeyword("_EMISSION");
            if (matGoldGhost.HasProperty("_EmissionColor")) matGoldGhost.SetColor("_EmissionColor", new Color(0.75f, 0.58f, 0.22f));

            Material matDarkShadow = new Material(litShader) { color = new Color(0.08f, 0.10f, 0.14f) };

            // ANOMALÍA 1 (Zona 12 — Tocador Nº 3 de Elena): Reflejo Desfasado en 1929 vs. Silueta de Cisne Negro en Ruinas
            GameObject fig1929Tocador = CreateSimpleSilhouetteProp("Anom1_Pose1929", new Vector3(25.3f, vestY, 34.4f), matGoldGhost, EraVisibilityMode.Only1929);
            GameObject figRuinsTocador = CreateSimpleSilhouetteProp("Anom1_ShadowRuins", new Vector3(24.6f, vestY, 34.4f), matDarkShadow, EraVisibilityMode.OnlyRuins);
            fig1929Tocador.SetActive(false);
            figRuinsTocador.SetActive(false);

            AnomalyEventData anomMirror = AnomalyEventData.CreateRuntime(
                id: "anom_n1_tocador_elena_asimetrico",
                night: 0,
                zone: "Zona 12 — Sala de Tocadores (Tocador Nº 3 de Elena)",
                pos: new Vector3(24.5f, vestY + 1.2f, 34.4f),
                radius: 4.2f,
                trigger: AnomalyTriggerType.ApproachObject,
                vis: AnomalyAsymmetricVisibility.BothDifferent,
                pattern: AnomalyCatalogPattern.AutonomousReflection,
                duration: 4.0f,
                linkedDocId: "doc_s3_mirror_back_note",
                title: "Anomalía Noche I — El Reflejo Dividido del Tocador Nº 3",
                toast1929: "En el espejo intacto de 1929 aparece una bailarina dorada manteniendo la quinta posición un segundo después de ti.",
                toastRuins: "En el espejo roto de las Ruinas se alza detrás de ti una silueta vestida de Cisne Negro que no está en la habitación.",
                journal1929: "vio en el Tocador Nº 3 (Época 1929) una figura luminosa en quinta posición reflejada con un segundo de retraso bajo las bombillas intactas.",
                journalRuins: "vio en ese mismo Tocador Nº 3 (Época Ruinas) una segunda silueta oscura con el tutú del Cisne Negro parada detrás del asiento.",
                freq1929: 523.25f,
                freqRuins: 246.94f);

            mgr.RegisterAnomalyEvent(anomMirror, fig1929Tocador, figRuinsTocador);

            // ANOMALÍA 2 (Zona 9 / Zona 10 — Retratos y Fotografía que cambia al descubrir la carta del 16 de octubre):
            GameObject extraShadowInPhotoRuins = CreateSimpleSilhouetteProp("Anom2_PhotoSecondSwan", new Vector3(-11.45f, vestY + 2.2f, 30.0f), matDarkShadow, EraVisibilityMode.OnlyRuins);
            extraShadowInPhotoRuins.transform.localScale = new Vector3(0.15f, 0.85f, 0.45f);
            extraShadowInPhotoRuins.SetActive(false);

            AnomalyEventData anomPhotoChange = AnomalyEventData.CreateRuntime(
                id: "anom_n1_fotografia_cambiante_elenco",
                night: 0,
                zone: "Zona 9 y 10 — Sala de Espera y Pasillo de los Retratos",
                pos: new Vector3(-14.5f, vestY + 1.5f, 32.5f),
                radius: 5.0f,
                trigger: AnomalyTriggerType.EnterZone,
                vis: AnomalyAsymmetricVisibility.BothDifferent,
                pattern: AnomalyCatalogPattern.ChangingPhotographOnReturn,
                duration: 4.5f,
                linkedDocId: "doc_s3_contradictory_photo",
                title: "Anomalía Noche I — La Segunda Sombra en el Retrato",
                toast1929: "Bajo el foco cálido de 1929, el retrato muestra a Elena Vesper completamente sola sobre el escenario.",
                toastRuins: "Al observar el marco en las Ruinas, una segunda silueta de Cisne Negro se dibuja nítidamente detrás de Elena.",
                journal1929: "confirmó que en el lienzo expuesto en 1929 Elena Vesper aparece bailando sola sin ningún espejo ni figura detrás.",
                journalRuins: "descubrió que en las Ruinas la capa de pintura superficial se ha desprendido, dejando ver la silueta de un segundo Cisne Negro detrás de Elena.",
                freq1929: 440f,
                freqRuins: 220f);

            mgr.RegisterAnomalyEvent(anomPhotoChange, null, extraShadowInPhotoRuins);

            // ANOMALÍA 3 (Zona 4 / Zona 5 — Sombra de bailarina que cruza hacia el Norte al restaurar la corriente del Cuadro Eléctrico):
            GameObject crossingShadow = CreateSimpleSilhouetteProp("Anom3_CrossingBallerina", new Vector3(0f, vestY, 28.5f), matGoldGhost, EraVisibilityMode.Both);
            crossingShadow.SetActive(false);

            AnomalyEventData anomPowerMilestone = AnomalyEventData.CreateRuntime(
                id: "anom_n1_sombra_vestibulo_energia",
                night: 0,
                zone: "Zona 4 — Vestíbulo de Recepción",
                pos: new Vector3(0f, vestY + 1.2f, 24.0f),
                radius: 18.0f,
                trigger: AnomalyTriggerType.PuzzleMilestone,
                vis: AnomalyAsymmetricVisibility.BothDifferent,
                pattern: AnomalyCatalogPattern.CrossingBallerinaShadow,
                duration: 3.6f,
                linkedDocId: "doc_s3_elena_incomplete_letter",
                title: "Anomalía Noche I — Eco en el Arco del Vestíbulo tras Restaurar 220V",
                toast1929: "Al volver la corriente en 1929, la silueta de una bailarina cruza el arco norte hacia el interior del palacio.",
                toastRuins: "Al activarse el relé en las Ruinas, se oyen pasos rápidos alejándose hacia el corredor de la Habitación de Elena.",
                journal1929: "vio al encenderse la araña del Vestíbulo en 1929 cómo una figura cruzaba hacia el pasillo interior (nunca hacia la salida de la calle).",
                journalRuins: "escuchó en ese mismo instante pasos sobre la madera dirigiéndose hacia la Habitación Privada de Elena (Zona 14), confirmando que no salió del edificio.",
                freq1929: 587.33f,
                freqRuins: 293.66f,
                milestoneId: "evidencia_energia_restaurada_noche1");

            mgr.RegisterAnomalyEvent(anomPowerMilestone, crossingShadow, crossingShadow);
        }

        private GameObject CreateSimpleSilhouetteProp(string name, Vector3 worldPos, Material mat, EraVisibilityMode vis)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(transform, false);
            root.transform.position = worldPos;

            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Torso";
            torso.transform.SetParent(root.transform, false);
            torso.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            torso.transform.localScale = new Vector3(0.32f, 0.48f, 0.22f);
            Destroy(torso.GetComponent<Collider>());
            torso.GetComponent<Renderer>().sharedMaterial = mat;

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.72f, 0f);
            head.transform.localScale = new Vector3(0.22f, 0.25f, 0.22f);
            Destroy(head.GetComponent<Collider>());
            head.GetComponent<Renderer>().sharedMaterial = mat;

            GameObject tutu = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tutu.name = "Tutu";
            tutu.transform.SetParent(root.transform, false);
            tutu.transform.localPosition = new Vector3(0f, 0.98f, 0f);
            tutu.transform.localScale = new Vector3(0.78f, 0.05f, 0.78f);
            Destroy(tutu.GetComponent<Collider>());
            tutu.GetComponent<Renderer>().sharedMaterial = mat;

            if (vis != EraVisibilityMode.Both)
            {
                EraOnlyVisibility eov = root.AddComponent<EraOnlyVisibility>();
                eov.Configure(vis, true);
            }
            return root;
        }
    }
}
