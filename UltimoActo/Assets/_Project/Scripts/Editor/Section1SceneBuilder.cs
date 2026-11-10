#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UltimoActo.Interaction;
using UltimoActo.Core;
using UltimoActo.Narrative;
using UltimoActo.Player;
using UltimoActo.UI;

namespace UltimoActo.Editor
{
    /// <summary>
    /// Generador arquitectónico integral de ÚLTIMO ACTO en SampleScene.unity.
    /// Reconstruye las 10 zonas físicas continuas e interconectadas:
    /// --- SECCIÓN 1: EXTERIOR Y ENTRADA ---
    /// 1. Portón exterior (reja de hierro forjado, pilares neoclásicos, faroles, enredaderas)
    /// 2. Patio de entrada (adoquines, jardineras descuidadas, estatuas de bailarinas, charcos lunares)
    /// 3. Escalinata monumental y pórtico (12 peldaños con alfombra vino, rampa suave a ras de suelo, columnas jónicas)
    /// 4. Vestíbulo de recepción (mármol veneciano con cenefa negra, boiserie 3D, lámpara de araña, mostrador, cuadro de Elena Vesper)
    /// 5. Pasillo principal (parqué en espiga, alfombra continua, apliques, galería de retratos, puertas numeradas)
    /// --- SECCIÓN 2: RECEPCIÓN Y ADMINISTRACIÓN ---
    /// 6. Recepción antigua (mostrador en L, timbre de latón, teléfono de disco, libro de visitas, reloj detenido a las 20:15)
    /// 7. Oficina de administración (despacho privado de Isadora Vale, escritorio caoba, sillón de cuero, retrato de la directora, cartas confidenciales)
    /// 8. Archivo de expedientes (estanterías altas, cajas caídas, mesa de examen, expedientes confidenciales de Elena Vesper y Mara Bell)
    /// 9. Sala de espera (sillones de terciopelo verde y borgoña, mesa baja con folletos de 1923, foto del elenco con hueco fantasmal)
    /// 10. Pasillo de los retratos (galería de 8 primeras bailarinas, cuadro de 'El Lago de las Sombras', conexión circular con el pasillo principal)
    ///
    /// --- SECCIÓN 3: VESTUARIOS Y HABITACIONES DE ALUMNAS ---
    /// 11. Vestuario femenino (bancos de madera, percheros metálicos con tutús, armarios, zapatillas con nombres, lista de alumnas, reloj detenido)
    /// 12. Tocadores con espejos (tocadores de caoba con espejos ovalados, bombillas antiguas, anomalía psicológica de silueta en reflejo, nota tras espejo)
    /// 13. Taquillas personales (filas de taquillas metálicas oxidadas bajo luz fría fluorescente, taquilla Nº 14 de Elena Vesper con llave de su habitación)
    /// 14. Habitación de Elena (cama marfil, escritorio con cajón de fondo falso, joyero, carta del 16 de octubre, foto sin retocar que contradice el retrato)
    /// 15. Lavabos y duchas antiguas (lavabos de porcelana, espejos, tuberías vistas, cabinas de ducha con anomalía acústica de goteo al alejarse)
    ///
    /// --- SECCIÓN 4: AULAS DE ENSAYO ---
    /// 16. Aula principal de ballet (techos altos 6.4m, grandes espejos, barras de ballet, ventanales lunares, piano vertical, marcas de tiza, anomalía de reflejo lejano)
    /// 17. Aula de espejos (paneles verticales de espejo entre columnas, tarimas de postura, focos, objeto con reflejo desplazado y anomalía en un solo panel)
    /// 18. Sala de maniquíes y vestuario escénico (maniquíes en pose de ballet, tutús y telas, mesa de costura, biombo en penumbra, maniquí que gira al apartar la vista)
    /// 19. Sala de entrenamiento individual (sala aislada con barra, espejo, botella y toalla, diario de práctica, anomalía de zapatillas deslizándose sobre madera)
    /// 20. Pasillo de servicio (tuberías vistas, carros de carga, escaleras plegables, conexiones a Archivo Musical, Teatro y atajo al Vestíbulo, anomalía de piano lejano)
    ///
    /// --- SECCIÓN 5: ARCHIVO MUSICAL — HISTORIA Y SONIDO ---
    /// 21. Biblioteca musical (estanterías de madera oscura, mesas de lectura con lámparas amarillentas, catálogo musical, versiones de 'El Lago de las Sombras', páginas que pasan solas)
    /// 22. Sala del piano (piano de cola antiguo frente a cortinas borgoña, metrónomo, interacción de secuencia musical que abre compartimento secreto, notas autónomas disonantes)
    /// 23. Archivo de partituras (archivadores etiquetados, escalera móvil, mesa de clasificación interactiva por fechas, carpeta vacía del final original)
    /// 24. Cabina de grabaciones (aislamiento acústico, grabadora de carrete abierto interactiva, cinta sin etiqueta oculta, doble altavoz cooperativo con pasos y voz cortada)
    /// 25. Depósito de instrumentos (violonchelo cubierto por tela oscura, estuches de violín, timbal, lámpara parpadeante, anomalía de nota Do Natural ausente en la grabación, salida a camerinos)
    ///
    /// --- SECCIÓN 6: CAMERINOS Y BACKSTAGE — ZONA RESTRINGIDA ---
    /// 26. Camerino de Mara (tocador ordenado con bombillas cálidas, fotos con Elena, carta escondida en cajón, anomalía de reflejo con postura desfasada)
    /// 27. Camerino de Elena (espejo agrietado, vestuario del Cisne Negro en maniquí que cambia al salir y regresar, silueta en el espejo, reconstrucción de preparativos secretos)
    /// 28. Pasillo detrás del escenario (paredes oscuras técnicas, tuberías, cortinas pesadas, aberturas de observación al escenario, señales, anomalía de pasos tras telón y luz al fondo)
    /// 29. Almacén de decorados (techo alto 6.8m, paneles pintados del lago nocturno, estructura bajo lona, comparativa con fotos oficiales, anomalía de cortina y silueta tras panel)
    /// 30. Sala de mecanismos y poleas (techo alto 7.2m, peine de poleas, cuerdas, contrapesos, plano técnico y puzle ambiental que desbloquea el acceso al Escenario Principal)
    ///
    /// --- SECCIÓN 7: TEATRO PRINCIPAL — CLÍMAX ---
    /// 31. Patio de butacas (auditorio monumental con butacas de terciopelo borgoña, balcones laterales, programas de mano, figuras inmóviles a distancia y aplausos lejanos)
    /// 32. Escenario principal (escenografía física de 'El Lago de las Sombras', marcas de Elena y Mara, silueta de bailarina bajo foco, reconstrucción del Clímax en 5 fases)
    /// 33. Foso de orquesta (foso hundido entre escenario y butacas, atril del director, espacio vacío del violonchelo solista, Fase 1 musical y anomalía de instrumentos vacíos)
    /// 34. Cabina de iluminación (cabina elevada con ventanal al escenario, consola de luces interactiva para la Fase 3, plano de iluminación y foco autónomo)
    /// 35. Pasarela superior del teatro (pasarela metálica elevada sobre telones y poleas, control de suspensión para la Fase 4 y perspectiva de altura)
    ///
    /// --- SECCIÓN 8: ZONA SECRETA — LA VERDAD OCULTA ---
    /// 36. Escalera oculta (acceso tras panel móvil en el hombro este del escenario, ladrillo envejecido, descansillo intermedio, anomalía de puerta cerrándose abajo)
    /// 37. Depósito clausurado (sala subterránea húmeda, cajas de utilería retiradas, fotos no oficiales, hueco oculto tras panel de madera, anomalías de tela y caja)
    /// 38. Sala de archivos prohibidos (archivo secreto ordenado, 6 expedientes prohibidos sobre Elena, Mara e Isadora, puzle cronológico en la mesa central)
    /// 39. Cámara tras el escenario (cámara íntima oculta tras el muro este del escenario principal, mirilla hacia el escenario, caja abierta con partitura a dos manos y secuencia musical final)
    ///
    /// --- SECCIÓN 9: JARDÍN DE INVIERNO (ZONA 40) ---
    /// 40. Jardín de Invierno (invernadero clásico europeo con bóveda de cristal y hierro oxidado, entrada de madera oscura y cristal desde el edificio principal y corredor de mantenimiento, fuente circular seca con cisnes y murmullo de agua fantasma, senderos ramificados, galería de estatuas que giran al apartar la vista, estatua conmemorativa de Elena Vesper con contradicción de fecha, mesa de piedra con aparición de objeto y rincón oculto tras la vegetación con losa desplazada y caja metálica semienterrada)
    /// </summary>
    [InitializeOnLoad]
    public static class Section1SceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";
        private const string MaterialsPath = "Assets/_Project/Materials/";
        private const string TexturesPBRPath = "Assets/_Project/Textures/PBR/";
        private const string ElenaPortraitPath = "Assets/_Project/Textures/Portraits/Elena_Vesper_Portrait.jpg";
        private const string IsadoraPortraitPath = "Assets/_Project/Textures/Portraits/Isadora_Vale_Portrait.jpg";
        private const string TroupePhotoPath = "Assets/_Project/Textures/Portraits/Troupe_1923_Photo.jpg";
        private const string AutoBuildSessionKey = "UltimoActo_AutoBuild_Section1_To_12_V34";

        static Section1SceneBuilder()
        {
            EditorApplication.delayCall += CheckAndAutoBuild;
        }

        private static void CheckAndAutoBuild()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.playModeStateChanged += OnPlayModeChanged;
                return;
            }

            ExecuteAutoBuild();
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;
                EditorApplication.delayCall += ExecuteAutoBuild;
            }
        }

        private static void ExecuteAutoBuild()
        {
            if (!SessionState.GetBool(AutoBuildSessionKey, false))
            {
                SessionState.SetBool(AutoBuildSessionKey, true);
                Debug.Log("<color=cyan>[Último Acto]</color> Construyendo automáticamente Mapa Completo (Secciones 1 a 9 — 40 Zonas + Jardín de Invierno)...");
                BuildFullAcademyMap();
            }
        }

        [MenuItem("Tools/Último Acto/1. Construir Mapa Completo (Secciones 1 a 9 en SampleScene)")]
        public static void BuildFullAcademyMap()
        {
            try
            {
                ProceduralTextureGenerator.GenerateAllTextures();

                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                if (!AssetDatabase.IsValidFolder("Assets/_Project/Materials"))
                    AssetDatabase.CreateFolder("Assets/_Project", "Materials");
                if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                    AssetDatabase.CreateFolder("Assets", "Scenes");

                // ==========================================
                // 1. MATERIALES PBR VIBRANTES
                // ==========================================
                Material matCobble = CreatePBRMaterial("Mat_S1_Cobblestone", "Tex_Cobble_Albedo.png", "Tex_Cobble_Normal.png", new Vector2(10f, 10f), 0.50f, 0.05f);
                Material matMarble = CreatePBRMaterial("Mat_S1_MarbleFloor", "Tex_Marble_Albedo.png", "Tex_Marble_Normal.png", new Vector2(6f, 6f), 0.88f, 0.15f);
                Material matParquet = CreatePBRMaterial("Mat_S1_Parquet", "Tex_Parquet_Albedo.png", "Tex_Parquet_Normal.png", new Vector2(4f, 16f), 0.70f, 0.05f);
                Material matCarpet = CreatePBRMaterial("Mat_S1_VelvetCarpet", "Tex_VelvetCarpet_Albedo.png", "Tex_VelvetCarpet_Normal.png", new Vector2(1f, 12f), 0.20f, 0.02f);
                Material matWallIvory = CreatePBRMaterial("Mat_S1_WallIvory", "Tex_WallIvory_Albedo.png", "Tex_WallIvory_Normal.png", new Vector2(4f, 2f), 0.25f, 0.02f);
                Material matFacade = CreatePBRMaterial("Mat_S1_Facade", "Tex_Facade_Albedo.png", "Tex_Facade_Normal.png", new Vector2(8f, 5f), 0.35f, 0.05f);
                Material matMahogany = CreatePBRMaterial("Mat_S1_Mahogany", "Tex_Mahogany_Albedo.png", "Tex_Mahogany_Normal.png", new Vector2(2f, 4f), 0.75f, 0.05f);
                Material matGreenVelvet = CreatePBRMaterial("Mat_S2_GreenVelvet", "Tex_GreenVelvet_Albedo.png", "Tex_GreenVelvet_Normal.png", new Vector2(4f, 4f), 0.25f, 0.02f);
                Material matLeather = CreatePBRMaterial("Mat_S2_LeatherDark", "Tex_Leather_Albedo.png", "Tex_Leather_Normal.png", new Vector2(3f, 3f), 0.45f, 0.05f);
                Material matCeramicTile = CreatePBRMaterial("Mat_S3_CeramicTile", "Tex_CeramicTile_Albedo.png", "Tex_CeramicTile_Normal.png", new Vector2(6f, 6f), 0.78f, 0.05f);
                Material matRustedLocker = CreatePBRMaterial("Mat_S3_RustedLocker", "Tex_LockerMetal_Albedo.png", "Tex_LockerMetal_Normal.png", new Vector2(2f, 3f), 0.48f, 0.72f);

                Material matStoneWall = GetOrCreateBasicMaterial("Mat_S1_StoneWall", new Color(0.35f, 0.33f, 0.31f), 0.30f, 0.0f);
                Material matDarkMarbleBorder = GetOrCreateBasicMaterial("Mat_S1_DarkMarbleBorder", new Color(0.08f, 0.08f, 0.10f), 0.85f, 0.1f);
                Material matIron = GetOrCreateBasicMaterial("Mat_S1_Iron", new Color(0.12f, 0.12f, 0.14f), 0.45f, 0.80f);
                Material matStatue = GetOrCreateBasicMaterial("Mat_S1_StatueStone", new Color(0.65f, 0.63f, 0.58f), 0.40f, 0.05f);
                Material matPuddle = GetOrCreateBasicMaterial("Mat_S1_Puddle", new Color(0.10f, 0.13f, 0.18f), 0.98f, 0.15f);
                Material matHedge = GetOrCreateBasicMaterial("Mat_S1_DryHedge", new Color(0.16f, 0.18f, 0.12f), 0.15f, 0.0f);
                Material matGold = GetOrCreateBasicMaterial("Mat_S1_AntiqueGold", new Color(0.85f, 0.70f, 0.38f), 0.75f, 0.75f);
                Material matClothCover = GetOrCreateBasicMaterial("Mat_S1_ClothCover", new Color(0.72f, 0.70f, 0.65f), 0.15f, 0.0f);
                Material matVelvetDrapes = GetOrCreateBasicMaterial("Mat_S1_VelvetDrapes", new Color(0.38f, 0.06f, 0.12f), 0.20f, 0.02f);
                Material matLanternGlow = GetOrCreateEmissiveMaterial("Mat_S1_LanternGlow", new Color(1f, 0.85f, 0.50f));
                Material matColdFluorescent = GetOrCreateEmissiveMaterial("Mat_S3_ColdFluorescent", new Color(0.72f, 0.88f, 1.0f));
                Material matGreenGlass = GetOrCreateBasicMaterial("Mat_S1_BankerGlass", new Color(0.05f, 0.35f, 0.15f), 0.90f, 0.1f);
                Material matMetalArchive = GetOrCreateBasicMaterial("Mat_S2_MetalArchive", new Color(0.20f, 0.24f, 0.22f), 0.55f, 0.65f);
                Material matWoodArchive = GetOrCreateBasicMaterial("Mat_S2_WoodArchive", new Color(0.28f, 0.18f, 0.12f), 0.35f, 0.05f);
                Material matCardboard = GetOrCreateBasicMaterial("Mat_S2_CardboardBox", new Color(0.55f, 0.45f, 0.35f), 0.15f, 0.0f);
                Material matMirrorGlass = GetOrCreateBasicMaterial("Mat_S3_MirrorGlass", new Color(0.74f, 0.78f, 0.82f), 0.96f, 0.88f);
                Material matPorcelain = GetOrCreateBasicMaterial("Mat_S3_Porcelain", new Color(0.88f, 0.86f, 0.81f), 0.85f, 0.05f);
                Material matTutuWhite = GetOrCreateBasicMaterial("Mat_S3_TutuWhite", new Color(0.85f, 0.82f, 0.76f), 0.22f, 0.0f);
                Material matTutuBlack = GetOrCreateBasicMaterial("Mat_S3_TutuBlack", new Color(0.09f, 0.08f, 0.11f), 0.25f, 0.05f);
                Material matSatinRibbon = GetOrCreateBasicMaterial("Mat_S3_SatinPink", new Color(0.76f, 0.58f, 0.56f), 0.60f, 0.05f);
                Material matShadowFigure = GetOrCreateBasicMaterial("Mat_S3_ShadowFigure", new Color(0.04f, 0.04f, 0.06f), 0.15f, 0.0f);

                // Cuadros artísticos al óleo y fotografías históricas
                Material matPortraitElena = CreatePortraitMaterial("Mat_S1_Portrait_Elena", ElenaPortraitPath, matMahogany);
                Material matPortraitIsadora = CreatePortraitMaterial("Mat_S2_Portrait_Isadora", IsadoraPortraitPath, matMahogany);
                Material matPhotoTroupe1923 = CreatePortraitMaterial("Mat_S2_Photo_Troupe1923", TroupePhotoPath, matMahogany);

                // ==========================================
                // 2. ILUMINACIÓN, NIEBLA Y POST-PROCESO
                // ==========================================
                RenderSettings.ambientMode = AmbientMode.Trilight;
                RenderSettings.ambientSkyColor = new Color(0.09f, 0.12f, 0.18f);
                RenderSettings.ambientEquatorColor = new Color(0.06f, 0.07f, 0.10f);
                RenderSettings.ambientGroundColor = new Color(0.02f, 0.02f, 0.03f);

                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogDensity = 0.007f;
                RenderSettings.fogColor = new Color(0.06f, 0.08f, 0.13f);

                GameObject moonObj = new GameObject("Moonlight_Cold");
                Light moonLight = moonObj.AddComponent<Light>();
                moonLight.type = LightType.Directional;
                moonLight.color = new Color(0.68f, 0.80f, 0.98f);
                moonLight.intensity = 0.85f;
                moonLight.shadows = LightShadows.Soft;
                moonObj.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

                CreatePostProcessVolume();

                GameObject rootEnv = new GameObject("Section1_2_3_Complete_Academy");

                // ====================================================================
                // 0. CIMENTACIÓN ANTICAÍDAS (PROTECCIÓN ABSOLUTA PARA TODA LA ACADEMIA)
                // ====================================================================
                // CIMENTACIÓN ANTICAÍDAS Y BASAMENTOS SUBTERRÁNEOS Y EXTERIORES
                // Cubre el Ala Oeste extendida hasta X = -95m y el Ala Norte hasta Z = 160m
                CreatePrimitive("Bedrock_Safety_Ground_West", PrimitiveType.Cube, rootEnv.transform,
                    new Vector3(-45.0f, -0.7f, 30f), new Vector3(120f, 1.0f, 180f), matCobble);
                CreatePrimitive("Bedrock_Safety_Ground_NorthEast", PrimitiveType.Cube, rootEnv.transform,
                    new Vector3(62.5f, -0.7f, 48.25f), new Vector3(95f, 1.0f, 95.5f), matCobble);
                CreatePrimitive("Bedrock_Subterranean_Safety_Ground", PrimitiveType.Cube, rootEnv.transform,
                    new Vector3(0f, -4.2f, 60f), new Vector3(230f, 1.0f, 240f), matCobble);

                // ====================================================================
                // ZONA 1: PORTÓN EXTERIOR (Z: -38 a -26, Ancho: 28m)
                // ====================================================================
                GameObject zone1 = new GameObject("1_Exterior_Gate_Area");
                zone1.transform.parent = rootEnv.transform;

                CreatePrimitive("Exterior_Ground", PrimitiveType.Cube, zone1.transform,
                    new Vector3(0f, -0.25f, -32f), new Vector3(28f, 0.5f, 12f), matCobble);

                CreateWall("Muro_Ext_Left", zone1.transform, new Vector3(-10f, 2.5f, -38f), new Vector3(10f, 5f, 1f), matStoneWall);
                CreateWall("Muro_Ext_Right", zone1.transform, new Vector3(10f, 2.5f, -38f), new Vector3(10f, 5f, 1f), matStoneWall);

                CreateClassicalColumn("Pilar_Porton_L", zone1.transform, new Vector3(-4.5f, 0f, -38f), 5.5f, 1.4f, matFacade);
                CreateClassicalColumn("Pilar_Porton_R", zone1.transform, new Vector3(4.5f, 0f, -38f), 5.5f, 1.4f, matFacade);

                CreateOrnateLantern("Farol_Porton_L", zone1.transform, new Vector3(-4.5f, 5.8f, -38f), matIron, matLanternGlow, new Color(1.0f, 0.82f, 0.50f), 2.8f);
                CreateOrnateLantern("Farol_Porton_R", zone1.transform, new Vector3(4.5f, 5.8f, -38f), matIron, matLanternGlow, new Color(1.0f, 0.82f, 0.50f), 2.8f);

                CreateHistoricalPlaque("Placa_Academia", zone1.transform, new Vector3(-3.7f, 2.2f, -37.2f), matGold);
                CreateWroughtIronGate("Reja_Derecha", zone1.transform, new Vector3(2.1f, 2.5f, -38f), 3.8f, 4.8f, matIron, false);
                CreateWroughtIronGate("Reja_Izquierda_Entreabierta", zone1.transform, new Vector3(-2.8f, 2.5f, -36.5f), 3.8f, 4.8f, matIron, true);

                CreateWall("Muro_Lat_Ext_L", zone1.transform, new Vector3(-14f, 2.5f, -32f), new Vector3(1f, 5f, 12f), matStoneWall);
                CreateWall("Muro_Lat_Ext_R", zone1.transform, new Vector3(14f, 2.5f, -32f), new Vector3(1f, 5f, 12f), matStoneWall);

                CreatePrimitive("Enredadera_Ext_L", PrimitiveType.Cube, zone1.transform,
                    new Vector3(-9.5f, 2.2f, -37.3f), new Vector3(6f, 3.8f, 0.2f), matHedge);

                // ====================================================================
                // ZONA 2: PATIO DE ENTRADA (Z: -26 a -5, Ancho: 28m, Largo: 21m)
                // ====================================================================
                GameObject zone2 = new GameObject("2_Entrance_Courtyard");
                zone2.transform.parent = rootEnv.transform;

                CreatePrimitive("Patio_Stone_Ground", PrimitiveType.Cube, zone2.transform,
                    new Vector3(0f, -0.25f, -15.5f), new Vector3(28f, 0.5f, 21f), matCobble);

                CreatePrimitive("Patio_Central_Pathway", PrimitiveType.Cube, zone2.transform,
                    new Vector3(0f, -0.22f, -15.5f), new Vector3(7.5f, 0.5f, 21f), matCobble);

                CreatePrimitive("Bordillo_Path_L", PrimitiveType.Cube, zone2.transform,
                    new Vector3(-3.85f, 0.05f, -15.5f), new Vector3(0.25f, 0.15f, 21f), matFacade);
                CreatePrimitive("Bordillo_Path_R", PrimitiveType.Cube, zone2.transform,
                    new Vector3(3.85f, 0.05f, -15.5f), new Vector3(0.25f, 0.15f, 21f), matFacade);

                CreateWall("Patio_Wall_L", zone2.transform, new Vector3(-14f, 3f, -15.5f), new Vector3(1f, 6f, 21f), matFacade);
                // Muro este del Patio de Entrada con puerta de servicio en Z = -10.0m hacia el Callejón Técnico y Sótano de Mantenimiento
                CreateWall("Patio_Wall_R_S", zone2.transform, new Vector3(14f, 3f, -18.525f), new Vector3(1f, 6f, 14.95f), matFacade);
                CreateWall("Patio_Wall_R_N", zone2.transform, new Vector3(14f, 3f, -6.975f), new Vector3(1f, 6f, 3.95f), matFacade);
                CreateArchitecturalDoor("Door_Patio_To_ServiceAlley", zone2.transform,
                    new Vector3(14f, 0f, -10f), Quaternion.Euler(0f, 90f, 0f),
                    1.8f, 3.2f, 6.0f, 1.0f, false, "",
                    "PASO DE SERVICIO — SÓTANO DE MANTENIMIENTO Y JARDÍN DE INVIERNO", "", matIron, matGold, matFacade);

                CreatePrimitive("Jardinera_Border_L", PrimitiveType.Cube, zone2.transform,
                    new Vector3(-8f, 0.4f, -15.5f), new Vector3(4.5f, 0.8f, 14f), matStoneWall);
                CreatePrimitive("Vegetacion_L", PrimitiveType.Cube, zone2.transform,
                    new Vector3(-8f, 0.9f, -15.5f), new Vector3(3.8f, 0.6f, 13.2f), matHedge);

                CreatePrimitive("Jardinera_Border_R", PrimitiveType.Cube, zone2.transform,
                    new Vector3(8f, 0.4f, -15.5f), new Vector3(4.5f, 0.8f, 14f), matStoneWall);
                CreatePrimitive("Vegetacion_R", PrimitiveType.Cube, zone2.transform,
                    new Vector3(8f, 0.9f, -15.5f), new Vector3(3.8f, 0.6f, 13.2f), matHedge);

                CreateBallerinaStatue("Estatua_Bailarina_CisneBlanco", zone2.transform,
                    new Vector3(-8f, 0f, -15.5f), matStatue, matStoneWall, "Estatua: El Cisne Blanco");
                CreateBallerinaStatue("Estatua_Bailarina_CisneNegro", zone2.transform,
                    new Vector3(8f, 0f, -15.5f), matStatue, matStoneWall, "Estatua: El Cisne Negro");

                CreatePrimitive("Puddle_1", PrimitiveType.Cube, zone2.transform,
                    new Vector3(-1.8f, 0.02f, -20f), new Vector3(2.8f, 0.04f, 2.2f), matPuddle);
                CreatePrimitive("Puddle_2", PrimitiveType.Cube, zone2.transform,
                    new Vector3(2.2f, 0.02f, -10f), new Vector3(3.2f, 0.04f, 2.4f), matPuddle);

                CreateStandingLamp("Farol_Patio_1", zone2.transform, new Vector3(-4.5f, 0f, -22f), matIron, matLanternGlow, new Color(1.0f, 0.80f, 0.48f), 2.5f);
                CreateStandingLamp("Farol_Patio_2", zone2.transform, new Vector3(4.5f, 0f, -22f), matIron, matLanternGlow, new Color(1.0f, 0.80f, 0.48f), 2.5f);
                CreateStandingLamp("Farol_Patio_3", zone2.transform, new Vector3(-4.5f, 0f, -8f), matIron, matLanternGlow, new Color(1.0f, 0.80f, 0.48f), 2.5f);
                CreateStandingLamp("Farol_Patio_4", zone2.transform, new Vector3(4.5f, 0f, -8f), matIron, matLanternGlow, new Color(1.0f, 0.80f, 0.48f), 2.5f);

                // ====================================================================
                // ZONA 3: ESCALINATA MONUMENTAL Y PÓRTICO (Z: -5 a +8, Y: 2.4m)
                // ====================================================================
                GameObject zone3 = new GameObject("3_Grand_Staircase_And_Portico");
                zone3.transform.parent = rootEnv.transform;

                float porticoY = 2.4f;

                // 12 escalones arquitectónicos con alfombra roja y varillas doradas
                int stepCount = 12;
                float stepDepth = 0.65f;
                float stepHeight = porticoY / stepCount; // 0.20m por escalón

                for (int i = 0; i < stepCount; i++)
                {
                    float stepZ = -5.0f + (i * stepDepth) + (stepDepth * 0.5f);
                    float stepY = (i * stepHeight) + (stepHeight * 0.5f);

                    CreatePrimitive($"Escalon_Piedra_{i + 1}", PrimitiveType.Cube, zone3.transform,
                        new Vector3(0f, stepY, stepZ), new Vector3(14f, stepHeight, stepDepth), matFacade);

                    CreatePrimitive($"Escalon_Alfombra_{i + 1}", PrimitiveType.Cube, zone3.transform,
                        new Vector3(0f, stepY + 0.015f, stepZ), new Vector3(5.2f, stepHeight + 0.01f, stepDepth), matCarpet);

                    CreatePrimitive($"Barra_Dorada_{i + 1}", PrimitiveType.Cube, zone3.transform,
                        new Vector3(0f, (i * stepHeight) + 0.03f, -5.0f + (i * stepDepth) + 0.04f), new Vector3(5.4f, 0.04f, 0.04f), matGold);
                }

                // Rampa física continua que arranca a ras del patio (Y: 0m en Z: -5m) hasta el pórtico (Y: 2.4m en Z: +2.8m)
                GameObject stairRamp = new GameObject("Stair_Smooth_Physics_Incline");
                stairRamp.transform.parent = zone3.transform;
                stairRamp.transform.localPosition = new Vector3(0f, 0.91f, -1.01f);
                stairRamp.transform.localRotation = Quaternion.Euler(-17.10f, 0f, 0f);
                BoxCollider rampCol = stairRamp.AddComponent<BoxCollider>();
                rampCol.size = new Vector3(14.5f, 0.60f, 8.5f);

                CreateArchitecturalBalustrade("Balaustrada_Left", zone3.transform, new Vector3(-7.2f, 0f, -1.1f), 8.5f, matFacade, matStoneWall);
                CreateArchitecturalBalustrade("Balaustrada_Right", zone3.transform, new Vector3(7.2f, 0f, -1.1f), 8.5f, matFacade, matStoneWall);

                CreatePrimitive("Portico_Landing", PrimitiveType.Cube, zone3.transform,
                    new Vector3(0f, porticoY - 0.25f, 5.4f), new Vector3(18f, 0.5f, 5.5f), matCobble);

                CreatePrimitive("Portico_Carpet", PrimitiveType.Cube, zone3.transform,
                    new Vector3(0f, porticoY + 0.015f, 5.4f), new Vector3(5.2f, 0.03f, 5.5f), matCarpet);

                CreateClassicalColumn("Columna_Portico_1", zone3.transform, new Vector3(-5.5f, porticoY, 4.2f), 5.5f, 0.9f, matFacade);
                CreateClassicalColumn("Columna_Portico_2", zone3.transform, new Vector3(-2f, porticoY, 4.2f), 5.5f, 0.9f, matFacade);
                CreateClassicalColumn("Columna_Portico_3", zone3.transform, new Vector3(2f, porticoY, 4.2f), 5.5f, 0.9f, matFacade);
                CreateClassicalColumn("Columna_Portico_4", zone3.transform, new Vector3(5.5f, porticoY, 4.2f), 5.5f, 0.9f, matFacade);

                CreatePrimitive("Entablamento_Portico", PrimitiveType.Cube, zone3.transform,
                    new Vector3(0f, porticoY + 5.8f, 4.8f), new Vector3(16f, 0.8f, 3.5f), matFacade);
                CreatePrimitive("Fronton_Triangular", PrimitiveType.Cube, zone3.transform,
                    new Vector3(0f, porticoY + 6.8f, 4.8f), new Vector3(12f, 1.2f, 1.5f), matFacade);

                float facadeY = porticoY + 4f;
                CreateWall("Fachada_L", zone3.transform, new Vector3(-8f, facadeY, 8f), new Vector3(12f, 8f, 0.8f), matFacade);
                CreateWall("Fachada_R", zone3.transform, new Vector3(8f, facadeY, 8f), new Vector3(12f, 8f, 0.8f), matFacade);
                CreateWall("Fachada_Top", zone3.transform, new Vector3(0f, porticoY + 6f, 8f), new Vector3(4f, 4f, 0.8f), matFacade);

                WorldDoubleDoor entranceDoors = CreateGrandDoubleDoor("Grand_Entrance_Doors", zone3.transform,
                    new Vector3(0f, porticoY, 8f), matMahogany, matGold);

                // Placa oficial con documento interactivo en el pórtico junto a la puerta
                CreateInteractiveWorldDoc("Placa_Oficial_Entrada", zone3.transform,
                    new Vector3(-2.2f, porticoY + 2.0f, 7.8f), new Vector3(0.6f, 0.8f, 0.12f), matGold,
                    "doc_entrance_edict_1924",
                    "Edicto Oficial de Clausura (Octubre 1924)",
                    "ACADEMIA IMPERIAL DE BALLET\n\nPor disposición gubernamental y de la Dirección artística de Isadora Vale, este edificio queda CLAUSURADO indefinidamente tras la desaparición de la primera bailarina Elena Vesper durante el ensayo general de 'El Lago de las Sombras'.\n\nQueda prohibido el paso a prensa y curiosos.\n\nTodo el archivo documental, libros de registro y expedientes de las alumnas han quedado bajo custodia en el ala de ADMINISTRACIÓN (ala oeste del vestíbulo).\n\nLos investigadores autorizados deben registrar su ingreso en la recepción antes de continuar la inspección.");

                CreateOrnateSconce("Farol_Puerta_L", zone3.transform, new Vector3(-2.8f, porticoY + 2.5f, 7.5f), matGold, matLanternGlow, new Color(1f, 0.85f, 0.55f), 2.5f);
                CreateOrnateSconce("Farol_Puerta_R", zone3.transform, new Vector3(2.8f, porticoY + 2.5f, 7.5f), matGold, matLanternGlow, new Color(1f, 0.85f, 0.55f), 2.5f);

                // ====================================================================
                // ZONA 4: VESTÍBULO DE RECEPCIÓN (Z: +8 a +32, Y: 2.4, 22m × 24m)
                // ====================================================================
                GameObject zone4 = new GameObject("4_Reception_Vestibule");
                zone4.transform.parent = rootEnv.transform;

                float vestY = porticoY;
                float vestHeight = 7.2f;

                CreatePrimitive("Vestibulo_Marble_Floor", PrimitiveType.Cube, zone4.transform,
                    new Vector3(0f, vestY - 0.25f, 20f), new Vector3(22f, 0.5f, 24f), matMarble);

                CreatePrimitive("Cenefa_Marble_N", PrimitiveType.Cube, zone4.transform,
                    new Vector3(0f, vestY - 0.25f, 31f), new Vector3(21.5f, 0.5f, 0.6f), matDarkMarbleBorder);
                CreatePrimitive("Cenefa_Marble_S", PrimitiveType.Cube, zone4.transform,
                    new Vector3(0f, vestY - 0.25f, 9f), new Vector3(21.5f, 0.5f, 0.6f), matDarkMarbleBorder);
                CreatePrimitive("Cenefa_Marble_W", PrimitiveType.Cube, zone4.transform,
                    new Vector3(-10.2f, vestY - 0.22f, 20f), new Vector3(0.6f, 0.5f, 23.5f), matDarkMarbleBorder);
                CreatePrimitive("Cenefa_Marble_E", PrimitiveType.Cube, zone4.transform,
                    new Vector3(10.2f, vestY - 0.22f, 20f), new Vector3(0.6f, 0.5f, 23.5f), matDarkMarbleBorder);

                CreatePrimitive("Vestibulo_Carpet", PrimitiveType.Cube, zone4.transform,
                    new Vector3(0f, vestY + 0.02f, 20f), new Vector3(5.5f, 0.05f, 24f), matCarpet);

                CreatePrimitive("Vestibulo_Ceiling", PrimitiveType.Cube, zone4.transform,
                    new Vector3(0f, vestY + vestHeight, 20f), new Vector3(22f, 0.5f, 24f), matWallIvory);

                for (int b = 0; b < 5; b++)
                {
                    float beamZ = 10f + (b * 5.0f);
                    CreatePrimitive($"Ceiling_Beam_{b + 1}", PrimitiveType.Cube, zone4.transform,
                        new Vector3(0f, vestY + vestHeight - 0.25f, beamZ), new Vector3(22f, 0.5f, 0.6f), matMahogany);
                }

                // Pared oeste: arco abierto de 4 metros hacia la SECCIÓN 2 (Recepción Antigua)
                CreateWall("Vest_Wall_West_1", zone4.transform, new Vector3(-11f, vestY + (vestHeight * 0.5f), 13f), new Vector3(0.6f, vestHeight, 10f), matWallIvory);
                CreateWall("Vest_Wall_West_2", zone4.transform, new Vector3(-11f, vestY + (vestHeight * 0.5f), 27f), new Vector3(0.6f, vestHeight, 10f), matWallIvory);
                CreateArchitecturalDoubleDoor("Doors_Administration", zone4.transform, new Vector3(-11f, vestY, 20f), Quaternion.Euler(0f, 90f, 0f), 3.7f, 4.2f, vestHeight, 0.6f, false, "", "ADMINISTRACIÓN Y ARCHIVO", "", matMahogany, matGold, matWallIvory);

                // Pared este: grandes puertas dobles hacia la SECCIÓN 3 (Vestuario Femenino y Habitaciones de Alumnas)
                CreateWall("Vest_Wall_East_1", zone4.transform, new Vector3(11f, vestY + (vestHeight * 0.5f), 13f), new Vector3(0.6f, vestHeight, 10f), matWallIvory);
                CreateWall("Vest_Wall_East_2", zone4.transform, new Vector3(11f, vestY + (vestHeight * 0.5f), 27f), new Vector3(0.6f, vestHeight, 10f), matWallIvory);
                CreateArchitecturalDoubleDoor("Doors_DressingRooms", zone4.transform, new Vector3(11f, vestY, 20f), Quaternion.Euler(0f, 90f, 0f), 3.7f, 4.2f, vestHeight, 0.6f, false, "", "VESTUARIOS Y HABITACIONES", "", matMahogany, matGold, matWallIvory);

                CreateBoiseriePanelSet("Boiserie_Vest_W1", zone4.transform, new Vector3(-10.65f, vestY + 2.6f, 13f), 8f, matWallIvory, matMahogany);
                CreateBoiseriePanelSet("Boiserie_Vest_W2", zone4.transform, new Vector3(-10.65f, vestY + 2.6f, 27f), 8f, matWallIvory, matMahogany);
                CreateBoiseriePanelSet("Boiserie_Vest_E1", zone4.transform, new Vector3(10.65f, vestY + 2.6f, 13f), 8f, matWallIvory, matMahogany);
                CreateBoiseriePanelSet("Boiserie_Vest_E2", zone4.transform, new Vector3(10.65f, vestY + 2.6f, 27f), 8f, matWallIvory, matMahogany);

                // Portal norte hacia el pasillo principal
                CreateWall("Vest_Wall_North_L", zone4.transform, new Vector3(-7.5f, vestY + (vestHeight * 0.5f), 32f), new Vector3(7f, vestHeight, 0.6f), matWallIvory);
                CreateWall("Vest_Wall_North_R", zone4.transform, new Vector3(7.5f, vestY + (vestHeight * 0.5f), 32f), new Vector3(7f, vestHeight, 0.6f), matWallIvory);
                CreateWall("Vest_Wall_North_Top", zone4.transform, new Vector3(0f, vestY + 5.5f, 32f), new Vector3(8f, 3.4f, 0.6f), matWallIvory);

                GameObject curL = CreatePrimitive("Curtain_Arch_L", PrimitiveType.Cube, zone4.transform,
                    new Vector3(-3.8f, vestY + 3f, 31.8f), new Vector3(1.2f, 5.5f, 0.4f), matVelvetDrapes);
                Collider cLCol = curL.GetComponent<Collider>();
                if (cLCol != null) Object.DestroyImmediate(cLCol);

                GameObject curR = CreatePrimitive("Curtain_Arch_R", PrimitiveType.Cube, zone4.transform,
                    new Vector3(3.8f, vestY + 3f, 31.8f), new Vector3(1.2f, 5.5f, 0.4f), matVelvetDrapes);
                Collider cRCol = curR.GetComponent<Collider>();
                if (cRCol != null) Object.DestroyImmediate(cRCol);

                CreateReceptionDesk("Reception_Counter", zone4.transform, new Vector3(-5.5f, vestY, 14f),
                    matMahogany, matGold, matClothCover, matGreenGlass);

                CreateFramedArtPortrait("Retrato_Elena_Vesper_CisneNegro", zone4.transform,
                    new Vector3(5.5f, vestY + 3.2f, 8.4f), new Vector3(2.4f, 3.2f, 0.2f), matGold, matPortraitElena,
                    "Retrato de Elena Vesper — El Cisne Negro (1924)");

                CreatePictureSpotlight("Spotlight_Elena_Portrait", zone4.transform,
                    new Vector3(5.5f, vestY + 5.5f, 11f), new Vector3(5.5f, vestY + 3.2f, 8.4f), new Color(1.0f, 0.90f, 0.75f), 3.5f);

                CreateCoveredFurniture("Sofa_Cubierto_Polvo", zone4.transform, new Vector3(7.5f, vestY, 25f), matClothCover, matMahogany);

                CreateChandelier("Arana_Cristal_Vestibulo", zone4.transform, new Vector3(0f, vestY + 5.5f, 20f),
                    matGold, new Color(1f, 0.85f, 0.55f), 4.8f);

                // ====================================================================
                // ZONA 5: PASILLO PRINCIPAL (Z: +32 a +78, Y: 2.4, Ancho: 8m, Largo: 46m)
                // ====================================================================
                GameObject zone5 = new GameObject("5_Grand_Hallway");
                zone5.transform.parent = rootEnv.transform;

                float hallY = vestY;
                float hallHeight = 5.2f;

                CreatePrimitive("Pasillo_Parquet_Floor", PrimitiveType.Cube, zone5.transform,
                    new Vector3(0f, hallY - 0.25f, 55f), new Vector3(8f, 0.5f, 46f), matParquet);

                CreatePrimitive("Pasillo_Carpet_Runner", PrimitiveType.Cube, zone5.transform,
                    new Vector3(0f, hallY + 0.02f, 55f), new Vector3(3.2f, 0.05f, 46f), matCarpet);

                CreatePrimitive("Pasillo_Ceiling", PrimitiveType.Cube, zone5.transform,
                    new Vector3(0f, hallY + hallHeight, 55f), new Vector3(8f, 0.5f, 46f), matWallIvory);

                // Muros oeste del pasillo principal:
                // - Puerta en Z = 40m (Atajo lateral de servicio hacia el Vestíbulo)
                // - Puerta doble en Z = 48m (Pasillo de los Retratos)
                // - Puerta en Z = 56m (Aula Principal de Ballet — Sección 4)
                // - Puerta en Z = 73m (Pasillo de Servicio — Sección 4)
                CreateWall("Pasillo_Wall_West_South_1", zone5.transform, new Vector3(-4f, hallY + (hallHeight * 0.5f), 35.475f), new Vector3(0.5f, hallHeight, 6.95f), matWallIvory);
                CreateWall("Pasillo_Wall_West_South_2", zone5.transform, new Vector3(-4f, hallY + (hallHeight * 0.5f), 43.525f), new Vector3(0.5f, hallHeight, 4.95f), matWallIvory);
                CreateWall("Pasillo_Wall_West_North_1", zone5.transform, new Vector3(-4f, hallY + (hallHeight * 0.5f), 52.475f), new Vector3(0.5f, hallHeight, 4.95f), matWallIvory);
                CreateWall("Pasillo_Wall_West_North_2", zone5.transform, new Vector3(-4f, hallY + (hallHeight * 0.5f), 64.50f), new Vector3(0.5f, hallHeight, 14.90f), matWallIvory);
                CreateWall("Pasillo_Wall_West_North_3", zone5.transform, new Vector3(-4f, hallY + (hallHeight * 0.5f), 76.025f), new Vector3(0.5f, hallHeight, 3.95f), matWallIvory);

                // Puerta doble hacia el Pasillo de los Retratos (hueco Z 46..50)
                CreateArchitecturalDoubleDoor("Doors_To_PortraitGallery", zone5.transform, new Vector3(-4f, hallY, 48f), Quaternion.Euler(0f, 90f, 0f), 3.7f, 3.6f, hallHeight, 0.5f, false, "", "PASILLO DE LOS RETRATOS", "", matMahogany, matGold, matWallIvory);

                // Muro este del pasillo principal:
                // - Puerta en Z = 44m hacia la Sección 3 (Tocadores y Vestuarios)
                // - Puerta en Z = 62m hacia la Sección 5 (Biblioteca y Archivo Musical)
                CreateWall("Pasillo_Wall_East_South", zone5.transform, new Vector3(4f, hallY + (hallHeight * 0.5f), 37.475f), new Vector3(0.5f, hallHeight, 10.95f), matWallIvory);
                CreateWall("Pasillo_Wall_East_Mid", zone5.transform, new Vector3(4f, hallY + (hallHeight * 0.5f), 53.00f), new Vector3(0.5f, hallHeight, 15.90f), matWallIvory);
                CreateWall("Pasillo_Wall_East_North", zone5.transform, new Vector3(4f, hallY + (hallHeight * 0.5f), 70.525f), new Vector3(0.5f, hallHeight, 14.95f), matWallIvory);

                DecoPiece("Zocalo_Hall_W1a", PrimitiveType.Cube, zone5.transform, new Vector3(-3.72f, hallY + 0.35f, 35.4f), new Vector3(0.1f, 0.7f, 6.8f), matMahogany);
                DecoPiece("Zocalo_Hall_W1b", PrimitiveType.Cube, zone5.transform, new Vector3(-3.72f, hallY + 0.35f, 43.5f), new Vector3(0.1f, 0.7f, 4.8f), matMahogany);
                DecoPiece("Zocalo_Hall_W2a", PrimitiveType.Cube, zone5.transform, new Vector3(-3.72f, hallY + 0.35f, 52.4f), new Vector3(0.1f, 0.7f, 4.8f), matMahogany);
                DecoPiece("Zocalo_Hall_W2b", PrimitiveType.Cube, zone5.transform, new Vector3(-3.72f, hallY + 0.35f, 64.5f), new Vector3(0.1f, 0.7f, 14.7f), matMahogany);
                DecoPiece("Zocalo_Hall_W2c", PrimitiveType.Cube, zone5.transform, new Vector3(-3.72f, hallY + 0.35f, 76.0f), new Vector3(0.1f, 0.7f, 3.8f), matMahogany);
                DecoPiece("Zocalo_Hall_E1", PrimitiveType.Cube, zone5.transform, new Vector3(3.72f, hallY + 0.35f, 37.4f), new Vector3(0.1f, 0.7f, 10.8f), matMahogany);
                DecoPiece("Zocalo_Hall_E2a", PrimitiveType.Cube, zone5.transform, new Vector3(3.72f, hallY + 0.35f, 53.0f), new Vector3(0.1f, 0.7f, 15.7f), matMahogany);
                DecoPiece("Zocalo_Hall_E2b", PrimitiveType.Cube, zone5.transform, new Vector3(3.72f, hallY + 0.35f, 70.5f), new Vector3(0.1f, 0.7f, 14.7f), matMahogany);

                // Posiciones de las puertas de las salas (para no superponer paneles ni apliques)
                float[] hallDoorsW = { 40f, 48f, 56f, 73f };
                float[] hallDoorsE = { 44f, 62f };
                System.Func<float[], float, float, bool> nearDoor = (arr, z, dist) =>
                {
                    foreach (float d in arr) if (Mathf.Abs(z - d) < dist) return true;
                    return false;
                };

                for (float pz = 36f; pz <= 72f; pz += 6f)
                {
                    if (!nearDoor(hallDoorsW, pz, 3.7f))
                        CreateBoiseriePanelSet($"Boiserie_Hall_W_{pz}", zone5.transform, new Vector3(-3.73f, hallY + 2.2f, pz), 4.5f, matWallIvory, matMahogany);
                    if (!nearDoor(hallDoorsE, pz, 3.7f))
                        CreateBoiseriePanelSet($"Boiserie_Hall_E_{pz}", zone5.transform, new Vector3(3.73f, hallY + 2.2f, pz), 4.5f, matWallIvory, matMahogany);
                }

                // Puertas reales hacia Sección 3 y 5 (Este) y Sección 4: Aulas de Ensayo y Pasillo de Servicio (Oeste)
                CreateArchitecturalDoor("Door_Rehearsal_1", zone5.transform, new Vector3(-4f, hallY, 40f), Quaternion.Euler(0f, 90f, 0f), 1.8f, 3.2f, hallHeight, 0.5f, false, "", "PASILLO DE SERVICIO — ATAJO AL VESTÍBULO", "", matMahogany, matGold, matWallIvory, false);
                CreateArchitecturalDoor("Door_Costumes", zone5.transform, new Vector3(4f, hallY, 44f), Quaternion.Euler(0f, -90f, 0f), 1.8f, 3.2f, hallHeight, 0.5f, false, "", "TOCADORES Y VESTUARIOS", "", matMahogany, matGold, matWallIvory, false);
                CreateArchitecturalDoor("Door_Rehearsal_2", zone5.transform, new Vector3(-4f, hallY, 56f), Quaternion.Euler(0f, 90f, 0f), 1.8f, 3.2f, hallHeight, 0.5f, false, "", "AULA PRINCIPAL DE BALLET", "", matMahogany, matGold, matWallIvory, false);
                CreateArchitecturalDoor("Door_Service_Corridor_Hall", zone5.transform, new Vector3(-4f, hallY, 73f), Quaternion.Euler(0f, 90f, 0f), 1.8f, 3.2f, hallHeight, 0.5f, false, "", "PASILLO DE SERVICIO — ZONA TÉCNICA", "", matMahogany, matGold, matWallIvory, false);
                CreateArchitecturalDoor("Door_Music_Archive", zone5.transform, new Vector3(4f, hallY, 62f), Quaternion.Euler(0f, -90f, 0f), 1.8f, 3.2f, hallHeight, 0.5f, false, "", "BIBLIOTECA Y ARCHIVO MUSICAL", "", matMahogany, matGold, matWallIvory, false);

                CreateFramedArtPortrait("Portrait_Ballerina_1", zone5.transform,
                    new Vector3(3.7f, hallY + 2.6f, 38f), new Vector3(0.15f, 1.8f, 1.3f), matGold, matPortraitElena, "Retrato Histórico I");
                CreateFramedArtPortrait("Portrait_Ballerina_2", zone5.transform,
                    new Vector3(3.7f, hallY + 2.6f, 52f), new Vector3(0.15f, 1.8f, 1.3f), matGold, matPortraitElena, "Retrato Histórico II");
                CreateFramedArtPortrait("Portrait_Ballerina_3", zone5.transform,
                    new Vector3(-3.7f, hallY + 2.6f, 68f), new Vector3(0.15f, 1.8f, 1.3f), matGold, matPortraitElena, "Retrato Histórico III");

                for (float lz = 36f; lz <= 74f; lz += 8f)
                {
                    if (!nearDoor(hallDoorsW, lz, 2.0f))
                        CreateOrnateSconce($"Sconce_Hall_W_{lz}", zone5.transform,
                            new Vector3(-3.7f, hallY + 2.9f, lz), matGold, matLanternGlow, new Color(1f, 0.82f, 0.50f), 2.2f);
                    if (!nearDoor(hallDoorsE, lz, 2.0f))
                        CreateOrnateSconce($"Sconce_Hall_E_{lz}", zone5.transform,
                            new Vector3(3.7f, hallY + 2.9f, lz), matGold, matLanternGlow, new Color(1f, 0.82f, 0.50f), 2.2f);
                }

                // Final del pasillo principal: Gran portal cerrado hacia el Gran Teatro (de piso a techo, imposible saltar por encima)
                CreateWall("Pasillo_Wall_End_L", zone5.transform, new Vector3(-2.95f, hallY + (hallHeight * 0.5f), 78f), new Vector3(2.1f, hallHeight, 0.6f), matWallIvory);
                CreateWall("Pasillo_Wall_End_R", zone5.transform, new Vector3(2.95f, hallY + (hallHeight * 0.5f), 78f), new Vector3(2.1f, hallHeight, 0.6f), matWallIvory);
                CreateArchitecturalDoubleDoor("Doors_GrandTheatre", zone5.transform, new Vector3(0f, hallY, 78f), Quaternion.identity, 3.5f, 4.0f, hallHeight, 0.6f, true, "", "GRAN TEATRO DE LA ÓPERA", "Gran Teatro de la Ópera — Cerrado con cerrojo desde el interior", matMahogany, matGold, matWallIvory);

                // ====================================================================
                // SECCIÓN 2: RECEPCIÓN Y ADMINISTRACIÓN (Área Oeste)
                // ====================================================================

                // ====================================================================
                // ZONA 6: RECEPCIÓN ANTIGUA (X: -11 a -24, Z: 12 a 24, Y: 2.4m)
                // ====================================================================
                GameObject zone6 = new GameObject("6_Old_Reception");
                zone6.transform.parent = rootEnv.transform;

                CreatePrimitive("Rec_Floor", PrimitiveType.Cube, zone6.transform,
                    new Vector3(-17.5f, vestY - 0.25f, 18f), new Vector3(13f, 0.5f, 12f), matParquet);
                CreatePrimitive("Rec_Ceiling", PrimitiveType.Cube, zone6.transform,
                    new Vector3(-17.5f, vestY + 5.0f, 18f), new Vector3(13f, 0.5f, 12f), matWallIvory);

                // Pared sur de Recepción con puerta en X = -22.0m hacia el Ala Suroeste (Terraza y Azotea, Enfermería Antigua y Sala Conmemorativa)
                CreateWall("Rec_Wall_South_W", zone6.transform, new Vector3(-23.525f, vestY + 2.5f, 12f), new Vector3(0.95f, 5.0f, 0.5f), matWallIvory);
                CreateWall("Rec_Wall_South_E", zone6.transform, new Vector3(-15.975f, vestY + 2.5f, 12f), new Vector3(9.95f, 5.0f, 0.5f), matWallIvory);
                CreateArchitecturalDoor("Door_Reception_To_SouthWestWing", zone6.transform,
                    new Vector3(-22.0f, vestY, 12f), Quaternion.identity,
                    1.8f, 3.2f, 5.0f, 0.5f, false, "",
                    "ALA SUROESTE — ENFERMERÍA ANTIGUA, SALA CONMEMORATIVA Y AZOTEA", "", matMahogany, matGold, matWallIvory);

                // Pared oeste: con puerta de madera noble hacia la oficina de administración en Z = 18m
                CreateWall("Rec_Wall_West_S", zone6.transform, new Vector3(-24f, vestY + 2.5f, 14.475f), new Vector3(0.5f, 5.0f, 4.95f), matWallIvory);
                CreateWall("Rec_Wall_West_N", zone6.transform, new Vector3(-24f, vestY + 2.5f, 21.525f), new Vector3(0.5f, 5.0f, 4.95f), matWallIvory);
                CreateArchitecturalDoor("Door_Reception_To_Office", zone6.transform, new Vector3(-24f, vestY, 18f), Quaternion.Euler(0f, 90f, 0f), 1.8f, 3.2f, 5.0f, 0.5f, false, "", "OFICINA DE DIRECCIÓN", "", matMahogany, matGold, matWallIvory);

                // Pared norte: con puerta hacia la sala de espera en X = -17.5m
                CreateWall("Rec_Wall_North_W", zone6.transform, new Vector3(-21.275f, vestY + 2.5f, 24f), new Vector3(5.45f, 5.0f, 0.5f), matWallIvory);
                CreateWall("Rec_Wall_North_E", zone6.transform, new Vector3(-13.725f, vestY + 2.5f, 24f), new Vector3(5.45f, 5.0f, 0.5f), matWallIvory);
                CreateArchitecturalDoor("Door_Reception_To_WaitingRoom", zone6.transform, new Vector3(-17.5f, vestY, 24f), Quaternion.identity, 1.8f, 3.2f, 5.0f, 0.5f, false, "", "SALA DE ESPERA", "", matMahogany, matGold, matWallIvory);

                // Mostrador de recepción en L
                CreatePrimitive("Counter_Front", PrimitiveType.Cube, zone6.transform,
                    new Vector3(-16.5f, vestY + 0.6f, 16f), new Vector3(6.5f, 1.2f, 0.9f), matMahogany);
                CreatePrimitive("Counter_Top", PrimitiveType.Cube, zone6.transform,
                    new Vector3(-16.5f, vestY + 1.22f, 16f), new Vector3(6.8f, 0.08f, 1.1f), matMahogany);
                CreatePrimitive("Counter_Side", PrimitiveType.Cube, zone6.transform,
                    new Vector3(-13.5f, vestY + 0.6f, 14.5f), new Vector3(0.9f, 1.2f, 2.2f), matMahogany);

                // Timbre de servicio de latón
                CreatePrimitive("Timbre_Recepcion_Antigua", PrimitiveType.Cylinder, zone6.transform,
                    new Vector3(-15.5f, vestY + 1.3f, 16f), new Vector3(0.18f, 0.08f, 0.18f), matGold);

                // Teléfono de sobremesa antiguo de disco
                CreateVintagePhone("Telefono_Recepcion", zone6.transform, new Vector3(-14.2f, vestY + 1.26f, 16f), matIron, matGold);

                // Libro de Registro Diario de 1924 (Documento interactivo)
                CreateInteractiveWorldDoc("Libro_Visitas_1924", zone6.transform, new Vector3(-17.2f, vestY + 1.28f, 16f),
                    new Vector3(0.5f, 0.06f, 0.4f), matGold, "doc_reception_log_1924",
                    "Libro Diario de Recepción — 12 de Octubre de 1924",
                    "Directora Vale ordenó prohibir el paso a extraños después de las 18:00 h.\n\n" +
                    "Anotación manuscrita del recepcionista:\n" +
                    "“Elena Vesper insiste en que las sombras en el escenario no siguen su movimiento.\n" +
                    "Se niega a devolver la llave maestra del sótano y asegura que el último ensayo general no terminará nunca.”");

                // Reloj de pared antiguo detenido en las 20:15
                CreateWallClock("Reloj_Detenido_2015", zone6.transform, new Vector3(-17.5f, vestY + 3.6f, 12.3f), matMahogany, matGold);

                // Calendario de pared abierto en Octubre de 1924
                CreateHistoricalPlaque("Calendario_Octubre_1924", zone6.transform, new Vector3(-15.2f, vestY + 3.2f, 12.3f), matGold);

                // Casilleros de correo y archivadores de madera tras el mostrador
                CreatePrimitive("Casilleros_Correo", PrimitiveType.Cube, zone6.transform,
                    new Vector3(-17.5f, vestY + 1.8f, 12.4f), new Vector3(5.5f, 2.4f, 0.5f), matMahogany);

                CreateOrnateSconce("Sconce_Rec_1", zone6.transform, new Vector3(-21.5f, vestY + 3.2f, 23.7f), matGold, matLanternGlow, new Color(1f, 0.82f, 0.50f), 2.5f);
                CreateOrnateSconce("Sconce_Rec_2", zone6.transform, new Vector3(-23.7f, vestY + 3.2f, 15f), matGold, matLanternGlow, new Color(1f, 0.82f, 0.50f), 2.5f);

                // ====================================================================
                // ZONA 7: OFICINA DE ADMINISTRACIÓN (X: -24 a -38, Z: 12 a 26, Y: 2.4m)
                // ====================================================================
                GameObject zone7 = new GameObject("7_Administration_Office");
                zone7.transform.parent = rootEnv.transform;

                CreatePrimitive("Office_Floor", PrimitiveType.Cube, zone7.transform,
                    new Vector3(-31f, vestY - 0.25f, 19f), new Vector3(14f, 0.5f, 14f), matParquet);
                CreatePrimitive("Office_Rug", PrimitiveType.Cube, zone7.transform,
                    new Vector3(-31f, vestY + 0.02f, 19f), new Vector3(8.5f, 0.04f, 8.5f), matCarpet);
                CreatePrimitive("Office_Ceiling", PrimitiveType.Cube, zone7.transform,
                    new Vector3(-31f, vestY + 5.0f, 19f), new Vector3(14f, 0.5f, 14f), matWallIvory);

                CreateWall("Office_Wall_South", zone7.transform, new Vector3(-31f, vestY + 2.5f, 12f), new Vector3(14f, 5.0f, 0.5f), matWallIvory);

                // Pared oeste: ventanal monumental con luz de luna y cortinas borgoña + Puerta Privada hacia la Estancia de Isadora Vale (Zona 48) en Z = 14.5m
                CreateWall("Office_Wall_West_S1", zone7.transform, new Vector3(-38f, vestY + 2.5f, 12.725f), new Vector3(0.5f, 5.0f, 1.45f), matWallIvory);
                CreateWall("Office_Wall_West_S2", zone7.transform, new Vector3(-38f, vestY + 2.5f, 16.275f), new Vector3(0.5f, 5.0f, 1.45f), matWallIvory);
                CreateArchitecturalDoor("Door_Office_To_IsadoraSuite", zone7.transform,
                    new Vector3(-38f, vestY, 14.5f), Quaternion.Euler(0f, 90f, 0f),
                    1.8f, 3.2f, 5.0f, 0.5f, false, "",
                    "ESTANCIA PRIVADA DE ISADORA VALE — DIRECCIÓN (ZONA 48)", "", matMahogany, matGold, matWallIvory);
                CreateWall("Office_Wall_West_N", zone7.transform, new Vector3(-38f, vestY + 2.5f, 23.5f), new Vector3(0.5f, 5.0f, 5f), matWallIvory);
                CreateWall("Office_Wall_West_Top", zone7.transform, new Vector3(-38f, vestY + 4.3f, 19f), new Vector3(0.5f, 1.4f, 4f), matWallIvory);
                CreateWall("Office_Wall_West_Bottom", zone7.transform, new Vector3(-38f, vestY + 0.5f, 19f), new Vector3(0.5f, 1.0f, 4f), matWallIvory);

                // Luz lunar fría azulada entrando por el ventanal del despacho
                CreatePictureSpotlight("Spotlight_Moon_Window", zone7.transform,
                    new Vector3(-37.5f, vestY + 4f, 19f), new Vector3(-31f, vestY + 1f, 19f), new Color(0.68f, 0.80f, 0.98f), 3.0f);

                // Cortinas borgoña descoloridas enmarcando la ventana
                CreatePrimitive("Office_Curtain_L", PrimitiveType.Cube, zone7.transform,
                    new Vector3(-37.6f, vestY + 2.6f, 17f), new Vector3(0.3f, 3.4f, 0.6f), matVelvetDrapes);
                CreatePrimitive("Office_Curtain_R", PrimitiveType.Cube, zone7.transform,
                    new Vector3(-37.6f, vestY + 2.6f, 21f), new Vector3(0.3f, 3.4f, 0.6f), matVelvetDrapes);

                // Pared norte: con puerta sellada con llave hacia el archivo de expedientes en X = -31m (muro completo sin rendijas)
                CreateWall("Office_Wall_North_W", zone7.transform, new Vector3(-35.025f, vestY + 2.5f, 26f), new Vector3(5.95f, 5.0f, 0.5f), matWallIvory);
                CreateWall("Office_Wall_North_E", zone7.transform, new Vector3(-26.975f, vestY + 2.5f, 26f), new Vector3(5.95f, 5.0f, 0.5f), matWallIvory);
                CreateArchitecturalDoor("Door_To_Archives", zone7.transform, new Vector3(-31f, vestY, 26f), Quaternion.identity, 1.8f, 3.2f, 5.0f, 0.5f, true, "key_archives", "ARCHIVO DE EXPEDIENTES", "Archivo de Expedientes — Cerrado con llave de la Directora", matMahogany, matGold, matWallIvory);

                // Gran escritorio presidencial de la Directora Isadora Vale
                CreatePrimitive("Desk_Director", PrimitiveType.Cube, zone7.transform,
                    new Vector3(-31f, vestY + 0.65f, 19f), new Vector3(3.6f, 1.3f, 1.8f), matMahogany);

                // Sillón de cuero envejecido para la Directora
                CreatePrimitive("Sillon_Director", PrimitiveType.Cube, zone7.transform,
                    new Vector3(-31f, vestY + 0.8f, 20.6f), new Vector3(1.2f, 1.6f, 1.0f), matLeather);

                // Sillas de visita
                CreatePrimitive("Silla_Visita_1", PrimitiveType.Cube, zone7.transform,
                    new Vector3(-32.2f, vestY + 0.6f, 17.4f), new Vector3(0.9f, 1.2f, 0.8f), matLeather);
                CreatePrimitive("Silla_Visita_2", PrimitiveType.Cube, zone7.transform,
                    new Vector3(-29.8f, vestY + 0.6f, 17.4f), new Vector3(0.9f, 1.2f, 0.8f), matLeather);

                // Lámpara de banquero con luz cálida sobre el escritorio
                CreatePrimitive("Office_Lamp_Base", PrimitiveType.Cylinder, zone7.transform,
                    new Vector3(-32f, vestY + 1.35f, 19f), new Vector3(0.18f, 0.05f, 0.18f), matGold);
                CreatePrimitive("Office_Lamp_Glass", PrimitiveType.Cube, zone7.transform,
                    new Vector3(-32f, vestY + 1.65f, 19f), new Vector3(0.35f, 0.12f, 0.22f), matGreenGlass);

                GameObject offLampObj = new GameObject("Office_Lamp_Light");
                offLampObj.transform.parent = zone7.transform;
                offLampObj.transform.localPosition = new Vector3(-32f, vestY + 1.60f, 19f);
                Light offLampLight = offLampObj.AddComponent<Light>();
                offLampLight.type = LightType.Point;
                offLampLight.color = new Color(1f, 0.88f, 0.65f);
                offLampLight.range = 8f;
                offLampLight.intensity = 2.4f;

                // Documento interactivo en el escritorio: Carta Confidencial de Isadora Vale
                CreateInteractiveWorldDoc("Carta_Confidencial_Isadora", zone7.transform, new Vector3(-30.6f, vestY + 1.34f, 19f),
                    new Vector3(0.45f, 0.04f, 0.35f), matGold, "doc_isadora_letter",
                    "Carta Confidencial — Dirección de la Academia",
                    "A los miembros de la Junta Directiva:\n\n" +
                    "Los incidentes durante los ensayos de 'El Lago de las Sombras' ya no pueden ocultarse bajo el pretexto del agotamiento.\n" +
                    "Mara Bell asumirá el papel del Cisne Blanco y del Cisne Negro si Elena continúa manifestando estos delirios.\n\n" +
                    "He ordenado custodiar los expedientes médicos originales en el archivo bajo llave. Nadie debe inspeccionar las fichas del elenco sin mi autorización expresa.\n\n" +
                    "— Isadora Vale, Directora");

                // Llave interactiva de latón del Archivo de Expedientes sobre el escritorio de la Directora
                GameObject keyArchives = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                keyArchives.name = "Key_Archives";
                keyArchives.transform.parent = zone7.transform;
                keyArchives.transform.position = new Vector3(-31.6f, vestY + 1.34f, 18.4f);
                keyArchives.transform.localScale = new Vector3(0.10f, 0.03f, 0.24f);
                keyArchives.transform.rotation = Quaternion.Euler(0f, 30f, 90f);
                keyArchives.GetComponent<Renderer>().sharedMaterial = matGold;
                WorldItem keyItem = keyArchives.AddComponent<WorldItem>();
                keyItem.Configure("key_archives", "Llave del Archivo de Expedientes", ItemType.Key);

                // Retrato al óleo solemne de Isadora Vale en la pared
                CreateFramedArtPortrait("Retrato_Isadora_Vale", zone7.transform,
                    new Vector3(-35.5f, vestY + 3.0f, 25.7f), new Vector3(2.2f, 2.8f, 0.15f), matGold, matPortraitIsadora,
                    "Retrato Oficial: Isadora Vale (Directora de la Academia, 1924)");

                // Estanterías con libros de contabilidad encuadernados en negro
                CreatePrimitive("Estanteria_Oficina_1", PrimitiveType.Cube, zone7.transform,
                    new Vector3(-31f, vestY + 2.0f, 12.4f), new Vector3(6.5f, 3.8f, 0.6f), matWoodArchive);
                CreatePrimitive("Archivador_Metal_Oficina", PrimitiveType.Cube, zone7.transform,
                    new Vector3(-25f, vestY + 1.4f, 14f), new Vector3(1.2f, 2.6f, 0.8f), matMetalArchive);

                // ====================================================================
                // ZONA 8: ARCHIVO DE EXPEDIENTES (X: -24 a -38, Z: 26 a 44, Y: 2.4m)
                // ====================================================================
                GameObject zone8 = new GameObject("8_Record_Archives");
                zone8.transform.parent = rootEnv.transform;

                CreatePrimitive("Archive_Floor", PrimitiveType.Cube, zone8.transform,
                    new Vector3(-31f, vestY - 0.25f, 35f), new Vector3(14f, 0.5f, 18f), matParquet);
                CreatePrimitive("Archive_Ceiling", PrimitiveType.Cube, zone8.transform,
                    new Vector3(-31f, vestY + 5.0f, 35f), new Vector3(14f, 0.5f, 18f), matWallIvory);

                CreateWall("Archive_Wall_West", zone8.transform, new Vector3(-38f, vestY + 2.5f, 35f), new Vector3(0.5f, 5.0f, 18f), matWallIvory);
                CreateWall("Archive_Wall_North", zone8.transform, new Vector3(-31f, vestY + 2.5f, 44f), new Vector3(14f, 5.0f, 0.5f), matWallIvory);

                // Pared este: con puerta hacia la Sala de Espera en Z = 32m (hueco Z 30.95..33.05, sin rendijas)
                CreateWall("Archive_Wall_East_S", zone8.transform, new Vector3(-24f, vestY + 2.5f, 28.475f), new Vector3(0.5f, 5.0f, 4.95f), matWallIvory);
                CreateWall("Archive_Wall_East_N", zone8.transform, new Vector3(-24f, vestY + 2.5f, 38.525f), new Vector3(0.5f, 5.0f, 10.95f), matWallIvory);
                CreateArchitecturalDoor("Door_Archive_To_WaitingRoom", zone8.transform, new Vector3(-24f, vestY, 32f), Quaternion.Euler(0f, 90f, 0f), 1.8f, 3.2f, 5.0f, 0.5f, false, "", "SALA DE ESPERA", "", matMahogany, matGold, matWallIvory);

                // Estanterías monumentales de archivo formando pasillos estrechos
                for (int r = 0; r < 3; r++)
                {
                    float rackX = -35f + (r * 3.8f);
                    CreatePrimitive($"Archive_Rack_{r + 1}", PrimitiveType.Cube, zone8.transform,
                        new Vector3(rackX, vestY + 1.8f, 35f), new Vector3(1.2f, 3.6f, 12f), matWoodArchive);

                    // Cajas de archivo y carpetas sobre las estanterías
                    CreatePrimitive($"Boxes_Rack_{r + 1}_A", PrimitiveType.Cube, zone8.transform,
                        new Vector3(rackX, vestY + 3.7f, 33f), new Vector3(0.9f, 0.6f, 2.5f), matCardboard);
                    CreatePrimitive($"Boxes_Rack_{r + 1}_B", PrimitiveType.Cube, zone8.transform,
                        new Vector3(rackX, vestY + 3.7f, 37f), new Vector3(0.9f, 0.6f, 2.5f), matCardboard);
                }

                // Estantería inclinada / caída con cajas en el suelo (sensación de abandono apresurado)
                GameObject tiltedShelf = CreatePrimitive("Archive_Tilted_Shelf", PrimitiveType.Cube, zone8.transform,
                    new Vector3(-27f, vestY + 1.4f, 40f), new Vector3(1.2f, 2.8f, 3.5f), matWoodArchive);
                tiltedShelf.transform.rotation = Quaternion.Euler(0f, 12f, 14f);

                CreatePrimitive("Scattered_Box_1", PrimitiveType.Cube, zone8.transform,
                    new Vector3(-28.5f, vestY + 0.2f, 39f), new Vector3(0.7f, 0.4f, 0.5f), matCardboard);
                CreatePrimitive("Scattered_Box_2", PrimitiveType.Cube, zone8.transform,
                    new Vector3(-28.2f, vestY + 0.15f, 41f), new Vector3(0.6f, 0.3f, 0.6f), matCardboard);

                // Mesa de examen documental central con lámpara de lectura
                CreatePrimitive("Archive_Exam_Table", PrimitiveType.Cube, zone8.transform,
                    new Vector3(-31f, vestY + 0.5f, 35f), new Vector3(2.4f, 1.0f, 1.4f), matWoodArchive);

                // Expediente interactivo 1: Elena Vesper
                CreateInteractiveWorldDoc("Expediente_Elena_Vesper", zone8.transform, new Vector3(-31.6f, vestY + 1.05f, 35f),
                    new Vector3(0.45f, 0.04f, 0.35f), matGold, "doc_elena_file",
                    "Expediente Confidencial: Elena Vesper (Alumna Nº 142)",
                    "Ingreso: 1919. Condición: Primera Bailarina / Solista.\n\n" +
                    "Informe médico (28 Septiembre 1924):\n" +
                    "Pérdida severa de peso, insomnio agudo y episodios de disociación.\n" +
                    "Afirma escuchar melodías tras los espejos del salón de ensayo y asegura que el reflejo del Cisne Negro se mueve con un retraso deliberado.\n\n" +
                    "Dictamen médico: Suspensión inmediata de funciones.\n\n" +
                    "[Anotación tachada al margen con tinta roja]:\n" +
                    "“La obra se estrenará el 14 de octubre cueste lo que cueste. Elena no se irá de esta academia.”");

                // Expediente interactivo 2: Mara Bell (Suplente)
                CreateInteractiveWorldDoc("Expediente_Mara_Bell", zone8.transform, new Vector3(-30.4f, vestY + 1.05f, 35f),
                    new Vector3(0.45f, 0.04f, 0.35f), matGold, "doc_mara_bell",
                    "Informe de Ensayo: Mara Bell (Suplente del Cisne)",
                    "Anotación de la bailarina Mara Bell:\n\n" +
                    "“He visto a Elena descender al archivo a oscuras. No busca partituras; oculta una libreta encuadernada en terciopelo negro tras los archivadores del fondo.\n\n" +
                    "Dice que el libreto original tiene un final que nadie conoce, y que si la música no se detiene a tiempo, el teatro nos consumirá a todas.”");

                CreateOrnateSconce("Sconce_Archive_1", zone8.transform, new Vector3(-31f, vestY + 3.2f, 43.7f), matGold, matLanternGlow, new Color(1f, 0.82f, 0.50f), 2.2f);
                CreateOrnateSconce("Sconce_Archive_2", zone8.transform, new Vector3(-31f, vestY + 3.2f, 26.3f), matGold, matLanternGlow, new Color(1f, 0.82f, 0.50f), 2.2f);

                // ====================================================================
                // ZONA 9: SALA DE ESPERA (X: -11 a -24, Z: 24 a 36, Y: 2.4m)
                // ====================================================================
                GameObject zone9 = new GameObject("9_Waiting_Room");
                zone9.transform.parent = rootEnv.transform;

                CreatePrimitive("Waiting_Floor", PrimitiveType.Cube, zone9.transform,
                    new Vector3(-17.5f, vestY - 0.25f, 30f), new Vector3(13f, 0.5f, 12f), matParquet);
                CreatePrimitive("Waiting_Rug", PrimitiveType.Cube, zone9.transform,
                    new Vector3(-17.5f, vestY + 0.02f, 30f), new Vector3(8.5f, 0.04f, 8.5f), matCarpet);
                CreatePrimitive("Waiting_Ceiling", PrimitiveType.Cube, zone9.transform,
                    new Vector3(-17.5f, vestY + 5.0f, 30f), new Vector3(13f, 0.5f, 12f), matWallIvory);

                CreateWall("Waiting_Wall_East", zone9.transform, new Vector3(-11f, vestY + 2.5f, 30f), new Vector3(0.5f, 5.0f, 12f), matWallIvory);
                // Pared oeste compartida con el archivo: hueco para la puerta del archivo (Z 30.95..33.05)
                CreateWall("Waiting_Wall_West_S", zone9.transform, new Vector3(-24f, vestY + 2.5f, 27.475f), new Vector3(0.5f, 5.0f, 6.95f), matWallIvory);
                CreateWall("Waiting_Wall_West_N", zone9.transform, new Vector3(-24f, vestY + 2.5f, 34.525f), new Vector3(0.5f, 5.0f, 2.95f), matWallIvory);

                // Pared norte: con puerta hacia el Pasillo de los Retratos en X = -17.5m (muro completo sin rendijas)
                CreateWall("Waiting_Wall_North_W", zone9.transform, new Vector3(-21.275f, vestY + 2.5f, 36f), new Vector3(5.45f, 5.0f, 0.5f), matWallIvory);
                CreateWall("Waiting_Wall_North_E", zone9.transform, new Vector3(-13.725f, vestY + 2.5f, 36f), new Vector3(5.45f, 5.0f, 0.5f), matWallIvory);
                CreateArchitecturalDoor("Door_Waiting_To_Portraits", zone9.transform, new Vector3(-17.5f, vestY, 36f), Quaternion.identity, 1.8f, 3.2f, 5.0f, 0.5f, false, "", "PASILLO DE LOS RETRATOS", "", matMahogany, matGold, matWallIvory);

                // Mobiliario de espera elegante pero inquietante
                // Sofá de terciopelo verde bosque
                CreatePrimitive("Sofa_Verde_Espera", PrimitiveType.Cube, zone9.transform,
                    new Vector3(-17.5f, vestY + 0.6f, 33f), new Vector3(3.2f, 1.2f, 1.2f), matGreenVelvet);

                // Sillón orejero de terciopelo borgoña
                CreatePrimitive("Sillon_Burgundy_1", PrimitiveType.Cube, zone9.transform,
                    new Vector3(-14.2f, vestY + 0.6f, 30f), new Vector3(1.2f, 1.3f, 1.1f), matVelvetDrapes);

                // Detalle psicológico perturbador: una silla de madera tallada colocada orientada de frente contra una pared vacía
                GameObject chairFacedWall = CreatePrimitive("Silla_Mirando_Pared", PrimitiveType.Cube, zone9.transform,
                    new Vector3(-12f, vestY + 0.55f, 26f), new Vector3(0.8f, 1.1f, 0.8f), matWoodArchive);
                chairFacedWall.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

                // Mesa baja con servicio de té y folletos de la academia
                CreatePrimitive("Coffee_Table", PrimitiveType.Cube, zone9.transform,
                    new Vector3(-17.5f, vestY + 0.3f, 30f), new Vector3(2.2f, 0.6f, 1.4f), matMahogany);

                // Perchero de latón con un abrigo oscuro olvidado
                CreatePrimitive("Coat_Rack_Pole", PrimitiveType.Cylinder, zone9.transform,
                    new Vector3(-12.5f, vestY + 1.2f, 34.5f), new Vector3(0.12f, 1.2f, 0.12f), matGold);
                CreatePrimitive("Forgotten_Coat", PrimitiveType.Cube, zone9.transform,
                    new Vector3(-12.5f, vestY + 1.4f, 34.5f), new Vector3(0.5f, 1.2f, 0.4f), matLeather);

                // Fotografía conmemorativa oficial del elenco de 1923 (con el hueco misterioso entre bailarinas)
                CreateFramedArtPortrait("Foto_Elenco_1923", zone9.transform,
                    new Vector3(-11.3f, vestY + 2.8f, 30f), new Vector3(0.15f, 2.0f, 2.8f), matGold, matPhotoTroupe1923,
                    "Fotografía Conmemorativa del Elenco (Temporada 1923)");

                CreateInteractiveWorldDoc("Nota_Foto_1923", zone9.transform, new Vector3(-17.5f, vestY + 0.64f, 30f),
                    new Vector3(0.4f, 0.04f, 0.3f), matGold, "doc_photo_note",
                    "Folleto de la Academia — Temporada 1923",
                    "Folleto publicitario de la prestigiosa Academia Vesper.\n\n" +
                    "Al examinar la fotografía enmarcada en la pared:\n" +
                    "En la primera fila del cuerpo de baile se observa un espacio inexplicable entre dos alumnas sonrientes, con una sombra distorsionada sobre las tablas del escenario.\n\n" +
                    "Alguien ha rayado furiosamente uno de los nombres del pie de foto con un alfiler.");

                CreateStandingLamp("Farol_Sala_Espera", zone9.transform, new Vector3(-22f, vestY, 26f), matIron, matLanternGlow, new Color(1f, 0.85f, 0.55f), 2.5f);

                // ====================================================================
                // ZONA 10: PASILLO DE LOS RETRATOS (X: -24 a -4, Z: 36 a 52, Y: 2.4m)
                // ====================================================================
                GameObject zone10 = new GameObject("10_Portrait_Gallery_Hallway");
                zone10.transform.parent = rootEnv.transform;

                // Tramo principal Norte-Sur (X: -17.5m, Z: 36 a 50m)
                CreatePrimitive("Portraits_Floor_NS", PrimitiveType.Cube, zone10.transform,
                    new Vector3(-17.5f, vestY - 0.25f, 43f), new Vector3(5f, 0.5f, 14f), matParquet);
                CreatePrimitive("Portraits_Carpet_NS", PrimitiveType.Cube, zone10.transform,
                    new Vector3(-17.5f, vestY + 0.02f, 43f), new Vector3(2.4f, 0.04f, 14f), matCarpet);
                CreatePrimitive("Portraits_Ceiling_NS", PrimitiveType.Cube, zone10.transform,
                    new Vector3(-17.5f, vestY + 5.0f, 43f), new Vector3(5f, 0.5f, 14f), matWallIvory);

                // Tramo conector Este-Oeste que conecta directamente con el Pasillo Principal (Z: 48m, X: -15m a -4m)
                CreatePrimitive("Portraits_Floor_EW", PrimitiveType.Cube, zone10.transform,
                    new Vector3(-9.5f, vestY - 0.25f, 48f), new Vector3(11f, 0.5f, 4f), matParquet);
                CreatePrimitive("Portraits_Carpet_EW", PrimitiveType.Cube, zone10.transform,
                    new Vector3(-9.5f, vestY + 0.02f, 48f), new Vector3(11f, 0.04f, 2.4f), matCarpet);
                CreatePrimitive("Portraits_Ceiling_EW", PrimitiveType.Cube, zone10.transform,
                    new Vector3(-9.5f, vestY + 5.0f, 48f), new Vector3(11f, 0.5f, 4f), matWallIvory);

                // Muros del pasillo de los retratos (con conexión al norte en X = -17.5, Z = 50 hacia Aula Principal de Ballet,
                // al oeste en X = -20, Z = 47 hacia la Galería del Patronato y Comedor Antiguo [Zona 47],
                // y al este en X = -15, Z = 40 hacia el Atajo Lateral de Servicio al Vestíbulo)
                CreateWall("Portraits_Wall_West_S", zone10.transform, new Vector3(-20f, vestY + 2.5f, 40.975f), new Vector3(0.5f, 5.0f, 9.95f), matWallIvory);
                CreateWall("Portraits_Wall_West_N", zone10.transform, new Vector3(-20f, vestY + 2.5f, 49.025f), new Vector3(0.5f, 5.0f, 1.95f), matWallIvory);
                CreateArchitecturalDoor("Door_Portraits_To_PatronageGallery", zone10.transform,
                    new Vector3(-20f, vestY, 47.0f), Quaternion.Euler(0f, 90f, 0f),
                    1.8f, 3.2f, 5.0f, 0.5f, false, "",
                    "GALERÍA DEL PATRONATO Y COMEDOR ANTIGUO (ZONA 47)", "", matMahogany, matGold, matWallIvory);
                CreateWall("Portraits_Wall_North_1W", zone10.transform, new Vector3(-19.275f, vestY + 2.5f, 50f), new Vector3(1.45f, 5.0f, 0.5f), matWallIvory);
                CreateWall("Portraits_Wall_North_1E", zone10.transform, new Vector3(-15.725f, vestY + 2.5f, 50f), new Vector3(1.45f, 5.0f, 0.5f), matWallIvory);
                CreateArchitecturalDoor("Door_Portraits_To_MainStudio", zone10.transform, new Vector3(-17.5f, vestY, 50f), Quaternion.identity, 1.8f, 3.2f, 5.0f, 0.5f, false, "", "AULA PRINCIPAL DE BALLET (ACCESO SUR)", "", matMahogany, matGold, matWallIvory);

                CreateWall("Portraits_Wall_North_2", zone10.transform, new Vector3(-9.5f, vestY + 2.5f, 50f), new Vector3(11f, 5.0f, 0.5f), matWallIvory);
                CreateWall("Portraits_Wall_South_Connector", zone10.transform, new Vector3(-9.5f, vestY + 2.5f, 46f), new Vector3(11f, 5.0f, 0.5f), matWallIvory);
                CreateWall("Portraits_Wall_East_S1", zone10.transform, new Vector3(-15f, vestY + 2.5f, 37.475f), new Vector3(0.5f, 5.0f, 2.95f), matWallIvory);
                CreateWall("Portraits_Wall_East_S2", zone10.transform, new Vector3(-15f, vestY + 2.5f, 43.525f), new Vector3(0.5f, 5.0f, 4.95f), matWallIvory);
                CreateArchitecturalDoor("Door_Portraits_To_Shortcut", zone10.transform, new Vector3(-15f, vestY, 40f), Quaternion.Euler(0f, 90f, 0f), 1.8f, 3.2f, 5.0f, 0.5f, false, "", "ATAJO DE SERVICIO — VESTÍBULO", "", matMahogany, matGold, matWallIvory);

                // Galería de retratos de bailarinas y directoras (entre Z = 37.2m y Z = 44.8m para dejar despejada la puerta en Z = 47.0m)
                for (int p = 0; p < 4; p++)
                {
                    float pZ = 37.2f + (p * 2.4f);
                    CreateFramedArtPortrait($"Retrato_Galeria_{p + 1}", zone10.transform,
                        new Vector3(-19.7f, vestY + 2.6f, pZ), new Vector3(0.12f, 1.8f, 1.3f), matGold, matPortraitElena,
                        $"Retrato Histórico: Primera Bailarina {1905 + (p * 5)}");

                    CreateOrnateSconce($"Sconce_Galeria_{p + 1}", zone10.transform,
                        new Vector3(-19.7f, vestY + 3.0f, pZ + 0.95f), matGold, matLanternGlow, new Color(1f, 0.82f, 0.50f), 2.2f);
                }

                // Retratos en el muro conector hacia el pasillo principal
                CreateFramedArtPortrait("Retrato_Lago_Sombras", zone10.transform,
                    new Vector3(-11f, vestY + 2.6f, 49.7f), new Vector3(2.6f, 1.8f, 0.12f), matGold, matPortraitElena,
                    "Óleo Representativo: El Lago de las Sombras (1924)");

                // Evidencia de reinterpretación tras visitar la Habitación de Elena (Sección 3 -> regreso a Sección 2)
                CreateInteractiveWorldDoc("Inscripcion_Reverso_Lago_Sombras", zone10.transform,
                    new Vector3(-11f, vestY + 1.45f, 49.68f), new Vector3(0.45f, 0.18f, 0.05f), matGold, "doc_portrait_reinterpretation",
                    "Reverso del Bastidor — 'El Lago de las Sombras'",
                    "Al inspeccionar el borde inferior del lienzo con lo descubierto en la habitación privada de Elena:\n\n" +
                    "Se distingue una capa gruesa de óleo oscuro aplicada apresuradamente sobre una segunda bailarina que originalmente aparecía detrás de Elena.\n\n" +
                    "Grabado a punzón en la madera del bastidor:\n" +
                    "“Retocado por orden de Dirección el 15 de octubre de 1924 — Borrar a la suplente de la escena principal.”");

                CreateOrnateSconce("Sconce_Galeria_EW", zone10.transform,
                    new Vector3(-9.5f, vestY + 3.0f, 46.3f), matGold, matLanternGlow, new Color(1f, 0.82f, 0.50f), 2.2f);

                // ====================================================================
                // SECCIÓN 3: VESTUARIOS Y HABITACIONES DE ALUMNAS (Ala Este, Y: 2.4m)
                // ====================================================================
                BuildSection3DressingAndDormitories(rootEnv.transform, vestY,
                    matParquet, matCarpet, matWallIvory, matMahogany, matGold, matIron,
                    matClothCover, matVelvetDrapes, matLanternGlow, matColdFluorescent,
                    matGreenVelvet, matLeather, matWoodArchive, matMetalArchive, matCardboard,
                    matCeramicTile, matRustedLocker, matMirrorGlass, matPorcelain,
                    matTutuWhite, matTutuBlack, matSatinRibbon, matShadowFigure,
                    matPortraitElena, matPhotoTroupe1923);

                // ====================================================================
                // SECCIÓN 4: AULAS DE ENSAYO (Ala Noroeste y Corredores de Servicio, Y: 2.4m)
                // ====================================================================
                BuildSection4RehearsalStudios(rootEnv.transform, vestY,
                    matParquet, matCarpet, matWallIvory, matMahogany, matGold, matIron,
                    matClothCover, matVelvetDrapes, matLanternGlow, matColdFluorescent,
                    matGreenVelvet, matLeather, matWoodArchive, matMetalArchive, matCardboard,
                    matStoneWall, matRustedLocker, matMirrorGlass, matPorcelain,
                    matTutuWhite, matTutuBlack, matSatinRibbon, matShadowFigure,
                    matPortraitElena, matPhotoTroupe1923);

                // ====================================================================
                // SECCIÓN 5: ARCHIVO MUSICAL — HISTORIA Y SONIDO (Ala Noreste, Y: 2.4m)
                // ====================================================================
                BuildSection5MusicArchive(rootEnv.transform, vestY,
                    matParquet, matCarpet, matWallIvory, matMahogany, matGold, matIron,
                    matClothCover, matVelvetDrapes, matLanternGlow, matColdFluorescent,
                    matGreenVelvet, matLeather, matWoodArchive, matMetalArchive, matCardboard,
                    matStoneWall, matRustedLocker, matMirrorGlass, matPorcelain,
                    matTutuWhite, matTutuBlack, matSatinRibbon, matShadowFigure,
                    matPortraitElena, matPhotoTroupe1923);

                // ====================================================================
                // SECCIÓN 6: CAMERINOS Y BACKSTAGE — ZONA RESTRINGIDA (Norte y Este, Y: 2.4m)
                // ====================================================================
                BuildSection6BackstageAndDressingRooms(rootEnv.transform, vestY,
                    matParquet, matCarpet, matWallIvory, matMahogany, matGold, matIron,
                    matClothCover, matVelvetDrapes, matLanternGlow, matColdFluorescent,
                    matGreenVelvet, matLeather, matWoodArchive, matMetalArchive, matCardboard,
                    matStoneWall, matRustedLocker, matMirrorGlass, matPorcelain,
                    matTutuWhite, matTutuBlack, matSatinRibbon, matShadowFigure,
                    matPortraitElena, matPhotoTroupe1923, matCobble);

                // ====================================================================
                // SECCIÓN 7: TEATRO PRINCIPAL — CLÍMAX (Zonas 31 a 35)
                // ====================================================================
                BuildSection7MainTheatreClimax(rootEnv.transform, vestY,
                    matParquet, matCarpet, matWallIvory, matMahogany, matGold, matIron,
                    matClothCover, matVelvetDrapes, matLanternGlow, matColdFluorescent,
                    matGreenVelvet, matLeather, matWoodArchive, matMetalArchive, matCardboard,
                    matStoneWall, matRustedLocker, matMirrorGlass, matPorcelain,
                    matTutuWhite, matTutuBlack, matSatinRibbon, matShadowFigure,
                    matPortraitElena, matPhotoTroupe1923);

                // ====================================================================
                // SECCIÓN 8: ZONA SECRETA — LA VERDAD OCULTA (Zonas 36 a 39)
                // ====================================================================
                BuildSection8SecretZone(rootEnv.transform, vestY,
                    matParquet, matCarpet, matWallIvory, matMahogany, matGold, matIron,
                    matClothCover, matVelvetDrapes, matLanternGlow, matColdFluorescent,
                    matGreenVelvet, matLeather, matWoodArchive, matMetalArchive, matCardboard,
                    matStoneWall, matRustedLocker, matMirrorGlass, matPorcelain,
                    matTutuWhite, matTutuBlack, matSatinRibbon, matShadowFigure,
                    matPortraitElena, matPhotoTroupe1923);

                // ====================================================================
                // SECCIÓN 9: JARDÍN DE INVIERNO (Zona 40)
                // ====================================================================
                BuildSection9WinterGarden(rootEnv.transform, vestY,
                    matParquet, matCarpet, matWallIvory, matMahogany, matGold, matIron,
                    matClothCover, matVelvetDrapes, matLanternGlow, matColdFluorescent,
                    matGreenVelvet, matLeather, matWoodArchive, matMetalArchive, matCardboard,
                    matStoneWall, matRustedLocker, matMirrorGlass, matPorcelain,
                    matTutuWhite, matTutuBlack, matSatinRibbon, matShadowFigure,
                    matPortraitElena, matPhotoTroupe1923, matCobble, matStatue, matHedge);

                // ====================================================================
                // SECCIÓN 10: SÓTANO DE MANTENIMIENTO, TERRAZA Y AZOTEA, ENFERMERÍA ANTIGUA Y SALA CONMEMORATIVA (Zonas 41 a 44)
                // ====================================================================
                BuildSection10ExpansionZones(rootEnv.transform, vestY,
                    matParquet, matCarpet, matWallIvory, matMahogany, matGold, matIron,
                    matClothCover, matVelvetDrapes, matLanternGlow, matColdFluorescent,
                    matGreenVelvet, matLeather, matWoodArchive, matMetalArchive, matCardboard,
                    matStoneWall, matRustedLocker, matMirrorGlass, matPorcelain,
                    matTutuWhite, matTutuBlack, matSatinRibbon, matShadowFigure,
                    matPortraitElena, matPhotoTroupe1923, matCobble, matStatue, matHedge,
                    matCeramicTile, matFacade, matGreenGlass);

                // ====================================================================
                // JUGADOR EN PRIMERA PERSONA
                // ====================================================================
                GameObject player = new GameObject("Player");
                player.transform.position = new Vector3(0f, 0.1f, -34f);
                player.tag = "Player";

                CharacterController cc = player.AddComponent<CharacterController>();
                cc.height = 1.8f;
                cc.radius = 0.4f;
                cc.center = new Vector3(0f, 0.9f, 0f);
                cc.stepOffset = 0.40f;
                cc.slopeLimit = 60f;
                cc.skinWidth = 0.05f;

                PlayerMovement movement = player.AddComponent<PlayerMovement>();
                SimpleInventory inventory = player.AddComponent<SimpleInventory>();

                GameObject cameraObj = new GameObject("FirstPersonCamera");
                cameraObj.transform.parent = player.transform;
                cameraObj.transform.localPosition = new Vector3(0f, 1.6f, 0f);
                cameraObj.tag = "MainCamera";

                Camera cam = cameraObj.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.Skybox;
                cam.cullingMask = ~0;
                cam.depth = 0;
                cam.nearClipPlane = 0.08f;
                cam.farClipPlane = 1000f;

                UniversalAdditionalCameraData camData = cameraObj.GetComponent<UniversalAdditionalCameraData>();
                if (camData == null)
                {
                    camData = cameraObj.AddComponent<UniversalAdditionalCameraData>();
                }
                camData.renderPostProcessing = true;

                cameraObj.AddComponent<AudioListener>();

                PlayerCameraLook cameraLook = player.AddComponent<PlayerCameraLook>();
                SerializedObject serLook = new SerializedObject(cameraLook);
                SerializedProperty pCamTrans = serLook.FindProperty("cameraTransform");
                if (pCamTrans != null) pCamTrans.objectReferenceValue = cameraObj.transform;
                serLook.ApplyModifiedProperties();

                PlayerInteraction interaction = player.AddComponent<PlayerInteraction>();
                SerializedObject serInteract = new SerializedObject(interaction);
                SerializedProperty pCamRef = serInteract.FindProperty("playerCamera");
                if (pCamRef != null) pCamRef.objectReferenceValue = cam;
                serInteract.ApplyModifiedProperties();

                GameObject flashlightObj = new GameObject("Flashlight_Support");
                flashlightObj.transform.parent = cameraObj.transform;
                flashlightObj.transform.localPosition = Vector3.zero;
                Light flashlight = flashlightObj.AddComponent<Light>();
                flashlight.type = LightType.Spot;
                flashlight.color = new Color(0.95f, 0.92f, 0.85f);
                flashlight.range = 18f;
                flashlight.spotAngle = 55f;
                flashlight.intensity = 1.5f;

                // ====================================================================
                // EVENT SYSTEM Y CANVAS DE UI
                // ====================================================================
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
                InputSystemUIInputModule inputModule = eventSystemObj.AddComponent<InputSystemUIInputModule>();
                inputModule.AssignDefaultActions();
#else
                eventSystemObj.AddComponent<StandaloneInputModule>();
#endif

                GameObject canvasObj = new GameObject("UI_Canvas");
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvasObj.AddComponent<GraphicRaycaster>();

                GameObject reticle = new GameObject("Reticle");
                reticle.transform.parent = canvasObj.transform;
                Image reticleImage = reticle.AddComponent<Image>();
                reticleImage.color = new Color(1f, 1f, 1f, 0.65f);
                reticleImage.raycastTarget = false;
                RectTransform reticleRect = reticle.GetComponent<RectTransform>();
                reticleRect.anchoredPosition = Vector2.zero;
                reticleRect.sizeDelta = new Vector2(8f, 8f);

                GameObject promptObj = new GameObject("PromptText");
                promptObj.transform.parent = canvasObj.transform;
                TextMeshProUGUI promptText = promptObj.AddComponent<TextMeshProUGUI>();
                promptText.text = "[E] Interactuar";
                promptText.fontSize = 26;
                promptText.fontStyle = FontStyles.Bold;
                promptText.alignment = TextAlignmentOptions.Center;
                promptText.color = new Color(0.96f, 0.94f, 0.90f);
                promptText.raycastTarget = false;
                RectTransform promptRect = promptObj.GetComponent<RectTransform>();
                promptRect.anchoredPosition = new Vector2(0f, -60f);
                promptRect.sizeDelta = new Vector2(800f, 50f);

                InteractionUI interactionUI = canvasObj.AddComponent<InteractionUI>();
                SerializedObject serializedUI = new SerializedObject(interactionUI);
                SerializedProperty pUIInteract = serializedUI.FindProperty("playerInteraction");
                if (pUIInteract != null) pUIInteract.objectReferenceValue = interaction;
                SerializedProperty pUIPrompt = serializedUI.FindProperty("promptTMP");
                if (pUIPrompt != null) pUIPrompt.objectReferenceValue = promptText;
                SerializedProperty pUICross = serializedUI.FindProperty("crosshairImage");
                if (pUICross != null) pUICross.objectReferenceValue = reticleImage;
                serializedUI.ApplyModifiedProperties();

                CreateDocumentViewerModal(canvasObj);
                canvasObj.AddComponent<AcademyMapUI>();

                // ====================================================================
                // GUARDAR Y FINALIZAR
                // ====================================================================
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

                Debug.Log($"<color=green>[Último Acto]</color> ¡MAPA COMPLETO (Secciones 1 a 9 — Las 40 Zonas de la Academia + Jardín de Invierno) construido con éxito en {ScenePath}!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"<color=red>[Último Acto]</color> Error crítico al construir el mapa: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // ====================================================================
        // CONSTRUCCIÓN DE LA SECCIÓN 3: VESTUARIOS Y HABITACIONES DE ALUMNAS
        // ====================================================================
        private static void BuildSection3DressingAndDormitories(
            Transform rootEnv, float vestY,
            Material matParquet, Material matCarpet, Material matWallIvory, Material matMahogany,
            Material matGold, Material matIron, Material matClothCover, Material matVelvetDrapes,
            Material matLanternGlow, Material matColdFluorescent, Material matGreenVelvet,
            Material matLeather, Material matWoodArchive, Material matMetalArchive, Material matCardboard,
            Material matCeramicTile, Material matRustedLocker, Material matMirrorGlass, Material matPorcelain,
            Material matTutuWhite, Material matTutuBlack, Material matSatinRibbon, Material matShadowFigure,
            Material matPortraitElena, Material matPhotoTroupe1923)
        {
            float roomH = 5.0f;

            // ====================================================================
            // ZONA 11: VESTUARIO FEMENINO (X: 11 a 27, Z: 12 a 32, Y: 2.4m)
            // ====================================================================
            GameObject zone11 = new GameObject("11_Female_Dressing_Room");
            zone11.transform.parent = rootEnv;

            CreatePrimitive("Dressing_Floor", PrimitiveType.Cube, zone11.transform,
                new Vector3(19f, vestY - 0.25f, 22f), new Vector3(16f, 0.5f, 20f), matParquet);
            CreatePrimitive("Dressing_Ceiling", PrimitiveType.Cube, zone11.transform,
                new Vector3(19f, vestY + roomH, 22f), new Vector3(16f, 0.5f, 20f), matWallIvory);

            // Pared sur (Z = 12): puerta hacia Zona 15 (Lavabos y Duchas Antiguas) en X = 19m (hueco X 17.95..20.05)
            CreateWall("Dressing_Wall_South_W", zone11.transform, new Vector3(14.475f, vestY + 2.5f, 12f), new Vector3(6.95f, roomH, 0.5f), matWallIvory);
            CreateWall("Dressing_Wall_South_E", zone11.transform, new Vector3(23.525f, vestY + 2.5f, 12f), new Vector3(6.95f, roomH, 0.5f), matWallIvory);
            CreateArchitecturalDoor("Door_Dressing_To_Showers", zone11.transform, new Vector3(19f, vestY, 12f), Quaternion.identity, 1.8f, 3.2f, roomH, 0.5f, false, "", "LAVABOS Y DUCHAS", "", matMahogany, matGold, matWallIvory);

            // Pared este (X = 27): puerta hacia Zona 13 (Taquillas Personales) en Z = 22m (hueco Z 20.95..23.05)
            CreateWall("Dressing_Wall_East_S", zone11.transform, new Vector3(27f, vestY + 2.5f, 16.475f), new Vector3(0.5f, roomH, 8.95f), matWallIvory);
            CreateWall("Dressing_Wall_East_N", zone11.transform, new Vector3(27f, vestY + 2.5f, 27.525f), new Vector3(0.5f, roomH, 8.95f), matWallIvory);
            CreateArchitecturalDoor("Door_Dressing_To_Lockers", zone11.transform, new Vector3(27f, vestY, 22f), Quaternion.Euler(0f, 90f, 0f), 1.8f, 3.2f, roomH, 0.5f, false, "", "TAQUILLAS PERSONALES", "", matMahogany, matGold, matWallIvory);

            // Pared norte (Z = 32): puertas dobles hacia Zona 12 (Tocadores con Espejos) en X = 19m (hueco X 17..21)
            CreateWall("Dressing_Wall_North_W", zone11.transform, new Vector3(14.0f, vestY + 2.5f, 32f), new Vector3(6.0f, roomH, 0.5f), matWallIvory);
            CreateWall("Dressing_Wall_North_E", zone11.transform, new Vector3(24.0f, vestY + 2.5f, 32f), new Vector3(6.0f, roomH, 0.5f), matWallIvory);
            CreateArchitecturalDoubleDoor("Doors_Dressing_To_Vanity", zone11.transform, new Vector3(19f, vestY, 32f), Quaternion.identity, 3.7f, 3.6f, roomH, 0.5f, false, "", "SALA DE TOCADORES", "", matMahogany, matGold, matWallIvory);

            // Ventanas altas con luz azulada de luna en la parte superior del muro
            for (int w = 0; w < 2; w++)
            {
                float wz = (w == 0) ? 16f : 28f;
                DecoPiece($"High_Window_Frame_{w}", PrimitiveType.Cube, zone11.transform,
                    new Vector3(26.7f, vestY + 4.1f, wz), new Vector3(0.12f, 1.1f, 2.4f), matMahogany);
                DecoPiece($"High_Window_Glass_{w}", PrimitiveType.Cube, zone11.transform,
                    new Vector3(26.73f, vestY + 4.1f, wz), new Vector3(0.06f, 0.9f, 2.1f), matColdFluorescent);
                CreatePictureSpotlight($"Moonlight_Dressing_{w}", zone11.transform,
                    new Vector3(26.2f, vestY + 4.1f, wz), new Vector3(19f, vestY + 0.8f, wz), new Color(0.55f, 0.72f, 0.96f), 2.2f);
            }

            // Bancos largos de madera con bolsos, cintas de satén y zapatillas abandonadas
            float[] benchZ = { 17.5f, 26.5f };
            for (int b = 0; b < benchZ.Length; b++)
            {
                float bz = benchZ[b];
                CreatePrimitive($"Banco_Madera_{b + 1}_Top", PrimitiveType.Cube, zone11.transform,
                    new Vector3(19f, vestY + 0.46f, bz), new Vector3(5.2f, 0.08f, 0.65f), matMahogany);
                CreatePrimitive($"Banco_Madera_{b + 1}_LegL", PrimitiveType.Cube, zone11.transform,
                    new Vector3(16.8f, vestY + 0.22f, bz), new Vector3(0.15f, 0.44f, 0.55f), matMahogany);
                CreatePrimitive($"Banco_Madera_{b + 1}_LegR", PrimitiveType.Cube, zone11.transform,
                    new Vector3(21.2f, vestY + 0.22f, bz), new Vector3(0.15f, 0.44f, 0.55f), matMahogany);

                // Bolso de cuero olvidado y cintas de zapatillas sobre el banco
                CreatePrimitive($"Bolso_Abandonado_{b + 1}", PrimitiveType.Cube, zone11.transform,
                    new Vector3(17.5f + b * 2.2f, vestY + 0.62f, bz), new Vector3(0.42f, 0.26f, 0.24f), matLeather);
                DecoPiece($"Cinta_Saten_{b + 1}", PrimitiveType.Cube, zone11.transform,
                    new Vector3(18.6f, vestY + 0.51f, bz + 0.1f), new Vector3(0.55f, 0.015f, 0.06f), matSatinRibbon);
            }

            // Percheros metálicos envejecidos con tutús (Cisne Blanco y Cisne Negro) y prendas polvorientas
            float[] rackZ = { 15.5f, 28.5f };
            for (int r = 0; r < rackZ.Length; r++)
            {
                float rz = rackZ[r];
                CreateGarmentRackWithTutus($"Perchero_Vestuario_{r + 1}", zone11.transform,
                    new Vector3(24.2f, vestY, rz), matIron, matGold, matTutuWhite, matTutuBlack, matClothCover);
            }

            // Cestas de mimbre/madera olvidadas con ropa de ensayo
            CreatePrimitive("Cesta_Olvidada_1", PrimitiveType.Cylinder, zone11.transform,
                new Vector3(24.5f, vestY + 0.35f, 19.5f), new Vector3(0.75f, 0.35f, 0.75f), matCardboard);
            CreatePrimitive("Cesta_Olvidada_2", PrimitiveType.Cylinder, zone11.transform,
                new Vector3(13.2f, vestY + 0.30f, 28.8f), new Vector3(0.65f, 0.30f, 0.65f), matCardboard);

            // Armarios de madera en el muro oeste (con uno interactivo abrible)
            CreateInteractiveWardrobe("Armario_Alumnas_1", zone11.transform, new Vector3(12.2f, vestY, 15.2f), Quaternion.Euler(0f, 90f, 0f), matMahogany, matGold, matTutuWhite, true);
            CreateInteractiveWardrobe("Armario_Alumnas_2", zone11.transform, new Vector3(12.2f, vestY, 25.5f), Quaternion.Euler(0f, 90f, 0f), matMahogany, matGold, matClothCover, false);

            // Carteles antiguos con ilustraciones de posiciones de ballet
            CreateFramedArtPortrait("Cartel_Posiciones_Ballet_1", zone11.transform,
                new Vector3(14.5f, vestY + 2.6f, 12.32f), new Vector3(1.6f, 1.9f, 0.08f), matGold, matPhotoTroupe1923,
                "Cartel Didáctico: Las Cinco Posiciones Clásicas del Ballet (1912)");
            CreateFramedArtPortrait("Cartel_Posiciones_Ballet_2", zone11.transform,
                new Vector3(23.5f, vestY + 2.6f, 31.68f), new Vector3(1.6f, 1.9f, 0.08f), matGold, matPortraitElena,
                "Cartel de Ensayo: Técnica de Puntas y Port de Bras");

            // Reloj detenido y calendario antiguo
            CreateWallClock("Reloj_Detenido_Vestuario", zone11.transform, new Vector3(14.5f, vestY + 3.6f, 31.7f), matMahogany, matGold);
            CreateHistoricalPlaque("Calendario_Vestuario_1924", zone11.transform, new Vector3(16.0f, vestY + 2.8f, 31.7f), matGold);

            // Lámparas de techo cálidas muy tenues
            CreateOrnateLantern("Lampara_Vestuario_Sur", zone11.transform, new Vector3(19f, vestY + 4.3f, 17.5f), matGold, matLanternGlow, new Color(1f, 0.80f, 0.48f), 1.9f);
            CreateOrnateLantern("Lampara_Vestuario_Norte", zone11.transform, new Vector3(19f, vestY + 4.3f, 26.5f), matGold, matLanternGlow, new Color(1f, 0.80f, 0.48f), 1.9f);

            // ELEMENTOS NARRATIVOS DE LA ZONA 11 (VESTUARIO FEMENINO)
            CreateInteractiveWorldDoc("Doc_Zapatillas_Elena", zone11.transform,
                new Vector3(19.2f, vestY + 0.54f, 17.5f), new Vector3(0.32f, 0.08f, 0.22f), matSatinRibbon,
                "doc_s3_ballet_slippers",
                "Zapatillas de Punta con Nombre Escrito a Mano",
                "Un par de zapatillas de satén rosa desgastadas por el uso intenso. En la suela de cuero aparece escrito a mano con tinta negra:\n" +
                "«Elena Vesper — Solista Principal».\n\n" +
                "Las cintas de sujeción no se desataron: fueron cortadas limpiamente con tijeras.\n" +
                "Dentro de la puntera izquierda hay un trozo de papel doblado: «Mara volvió a registrar mi bolso durante el descanso. He tenido que esconder mis cosas importantes bajo llave.»");

            CreateInteractiveWorldDoc("Doc_Lista_Alumnas_Vestuario", zone11.transform,
                new Vector3(12.25f, vestY + 1.15f, 15.2f), new Vector3(0.35f, 0.04f, 0.28f), matGold,
                "doc_s3_student_list",
                "Lista Oficial de Alumnas y Taquillas (Octubre de 1924)",
                "REGISTRO INTERNO DE VESTUARIO — ACADEMIA VESPER\n\n" +
                "• Taquilla Nº 03 — Sofía Lindqvist (Cuerpo de baile)\n" +
                "• Taquilla Nº 07 — Mara Bell (Primera Suplente)\n" +
                "• Taquilla Nº 11 — Clara Valmont (Corifeo)\n" +
                "• Taquilla Nº 14 — Elena Vesper (Primera Bailarina — Cisne Negro)\n\n" +
                "Nota de la Gobernanta:\n" +
                "“Se recuerda a las alumnas que las llaves de las habitaciones privadas del ala norte son responsabilidad exclusiva de cada solista.”");

            CreateInteractiveWorldDoc("Doc_Nota_Olvidada_Vestuario", zone11.transform,
                new Vector3(20.1f, vestY + 0.54f, 26.5f), new Vector3(0.34f, 0.04f, 0.26f), matGold,
                "doc_s3_forgotten_bench_note",
                "Nota Olvidada entre las Pertenencias del Banco",
                "“Clara, ¿has visto cómo miraba Mara a Elena frente a los espejos del tocador antes del ensayo?\n\n" +
                "Cuando apagaron las lámparas del vestuario a las nueve, juraría que vi a alguien entrar con una linterna hacia los lavabos del fondo. El reloj de la pared se detuvo esa misma noche y nadie ha venido a recoger sus abrigos.”");

            // ====================================================================
            // ZONA 12: TOCADORES CON ESPEJOS (X: 11 a 27, Z: 32 a 46, Y: 2.4m)
            // + Corredor conector hacia el Pasillo Principal (X: 4 a 11, Z: 42 a 46)
            // ====================================================================
            GameObject zone12 = new GameObject("12_Vanity_Mirror_Room");
            zone12.transform.parent = rootEnv;

            CreatePrimitive("Vanity_Floor", PrimitiveType.Cube, zone12.transform,
                new Vector3(19f, vestY - 0.25f, 39f), new Vector3(16f, 0.5f, 14f), matParquet);
            CreatePrimitive("Vanity_Ceiling", PrimitiveType.Cube, zone12.transform,
                new Vector3(19f, vestY + roomH, 39f), new Vector3(16f, 0.5f, 14f), matWallIvory);
            CreatePrimitive("Vanity_Rug", PrimitiveType.Cube, zone12.transform,
                new Vector3(19f, vestY + 0.02f, 39f), new Vector3(7.5f, 0.04f, 10.5f), matCarpet);

            // Corredor de conexión oeste con el Pasillo Principal (Z: 42..46, X: 4..11)
            CreatePrimitive("Vanity_Conn_Floor", PrimitiveType.Cube, zone12.transform,
                new Vector3(7.5f, vestY - 0.25f, 44f), new Vector3(7f, 0.5f, 4f), matParquet);
            CreatePrimitive("Vanity_Conn_Carpet", PrimitiveType.Cube, zone12.transform,
                new Vector3(7.5f, vestY + 0.02f, 44f), new Vector3(7f, 0.04f, 2.2f), matCarpet);
            CreatePrimitive("Vanity_Conn_Ceiling", PrimitiveType.Cube, zone12.transform,
                new Vector3(7.5f, vestY + roomH, 44f), new Vector3(7f, 0.5f, 4f), matWallIvory);
            CreateWall("Vanity_Conn_Wall_South", zone12.transform, new Vector3(7.5f, vestY + 2.5f, 42f), new Vector3(7f, roomH, 0.5f), matWallIvory);
            CreateWall("Vanity_Conn_Wall_North", zone12.transform, new Vector3(7.5f, vestY + 2.5f, 46f), new Vector3(7f, roomH, 0.5f), matWallIvory);

            // Muro oeste de Tocadores (de Z = 32 a Z = 42, dejando abierto Z 42..46 hacia el corredor del pasillo)
            CreateWall("Vanity_Wall_West_S", zone12.transform, new Vector3(11f, vestY + 2.5f, 37f), new Vector3(0.5f, roomH, 10f), matWallIvory);
            // Muro este de Tocadores (Z = 32 a 46): con puerta de conexión en Z = 41m hacia el Depósito de Instrumentos (Sección 5)
            CreateWall("Vanity_Wall_East_S", zone12.transform, new Vector3(27f, vestY + 2.5f, 35.975f), new Vector3(0.5f, roomH, 7.95f), matWallIvory);
            CreateWall("Vanity_Wall_East_N", zone12.transform, new Vector3(27f, vestY + 2.5f, 44.025f), new Vector3(0.5f, roomH, 3.95f), matWallIvory);
            CreateArchitecturalDoor("Door_InstrumentStorage_To_Vanity", zone12.transform, new Vector3(27f, vestY, 41f), Quaternion.Euler(0f, 90f, 0f), 1.8f, 3.2f, roomH, 0.5f, false, "", "DEPÓSITO DE INSTRUMENTOS — ARCHIVO MUSICAL", "", matMahogany, matGold, matWallIvory);

            // Muro norte (Z = 46): puerta hacia el corredor privado de la Habitación de Elena en X = 19m (hueco X 17.95..20.05)
            CreateWall("Vanity_Wall_North_W", zone12.transform, new Vector3(14.475f, vestY + 2.5f, 46f), new Vector3(6.95f, roomH, 0.5f), matWallIvory);
            CreateWall("Vanity_Wall_North_E", zone12.transform, new Vector3(23.525f, vestY + 2.5f, 46f), new Vector3(6.95f, roomH, 0.5f), matWallIvory);
            CreateArchitecturalDoor("Door_Vanity_To_ElenaCorridor", zone12.transform, new Vector3(19f, vestY, 46f), Quaternion.identity, 1.8f, 3.2f, roomH, 0.5f, false, "", "CORREDOR PRIVADO — SOLISTA", "", matMahogany, matGold, matWallIvory);

            // Cortinas borgoña descoloridas en los rincones de la sala de tocadores
            DecoPiece("Vanity_Curtain_NW", PrimitiveType.Cube, zone12.transform, new Vector3(11.4f, vestY + 2.5f, 41.3f), new Vector3(0.35f, 4.4f, 0.9f), matVelvetDrapes);
            DecoPiece("Vanity_Curtain_NE", PrimitiveType.Cube, zone12.transform, new Vector3(26.6f, vestY + 2.5f, 45.2f), new Vector3(0.35f, 4.4f, 0.9f), matVelvetDrapes);

            // Tocadores con espejos ovalados, bombillas antiguas, frascos de perfume y sillas borgoña
            CreateVanityStation("Tocador_1_Oeste", zone12.transform, new Vector3(12.2f, vestY, 35.0f), Quaternion.Euler(0f, 90f, 0f),
                matMahogany, matGold, matMirrorGlass, matVelvetDrapes, matLanternGlow, matPorcelain, matShadowFigure, true, false);
            CreateVanityStation("Tocador_2_Oeste_Mara", zone12.transform, new Vector3(12.2f, vestY, 39.2f), Quaternion.Euler(0f, 90f, 0f),
                matMahogany, matGold, matMirrorGlass, matVelvetDrapes, matLanternGlow, matPorcelain, matShadowFigure, true, false);

            // Tocador 3 (Este): Tocador de Elena Vesper CON ANOMALÍA PSICOLÓGICA DE REFLEJO Y DISCREPANCIA DE OBJETO
            CreateVanityStation("Tocador_3_Este_Elena_Anomalia", zone12.transform, new Vector3(25.8f, vestY, 34.4f), Quaternion.Euler(0f, -90f, 0f),
                matMahogany, matGold, matMirrorGlass, matVelvetDrapes, matLanternGlow, matPorcelain, matShadowFigure, true, true);
            CreateVanityStation("Tocador_4_Este", zone12.transform, new Vector3(25.8f, vestY, 37.8f), Quaternion.Euler(0f, -90f, 0f),
                matMahogany, matGold, matMirrorGlass, matVelvetDrapes, matLanternGlow, matPorcelain, matShadowFigure, false, false);
            CreateVanityStation("Tocador_5_Este", zone12.transform, new Vector3(25.8f, vestY, 44.0f), Quaternion.Euler(0f, -90f, 0f),
                matMahogany, matGold, matMirrorGlass, matVelvetDrapes, matLanternGlow, matPorcelain, matShadowFigure, true, false);

            // ELEMENTOS NARRATIVOS DE LA ZONA 12 (TOCADORES CON ESPEJOS)
            CreateInteractiveWorldDoc("Doc_Foto_Elena_Y_Mara", zone12.transform,
                new Vector3(12.35f, vestY + 0.88f, 39.2f), new Vector3(0.28f, 0.04f, 0.24f), matGold,
                "doc_s3_elena_mara_photo",
                "Fotografía Pequeña: Elena Vesper y Mara Bell en los Tocadores",
                "Una fotografía en blanco y negro tomada frente a estos mismos espejos meses antes del estreno.\n" +
                "Elena está sentada frente al espejo mientras Mara, de pie detrás de ella, apoya las manos sobre sus hombros.\n\n" +
                "El rostro de Elena fue rasgado levemente con la punta de una horquilla. En el reverso se lee:\n" +
                "“Mara ensaya mis gestos frente al espejo cuando cree que nadie la ve. No quiere compartir el papel; quiere ocupar mi lugar.”");

            CreateInteractiveWorldDoc("Doc_Nota_Reverso_Espejo", zone12.transform,
                new Vector3(25.65f, vestY + 0.88f, 35.0f), new Vector3(0.32f, 0.04f, 0.26f), matGold,
                "doc_s3_mirror_back_note",
                "Nota Oculta en el Reverso del Espejo de Elena (Tocador Nº 3)",
                "Escrito con lápiz de ojos negro en la madera trasera del marco ovalado:\n\n" +
                "“13 de Octubre — Anoche las bombillas del tocador parpadearon y mi reflejo tardó un segundo más que yo en bajar los brazos.\n\n" +
                "He cerrado con llave mi habitación privada al fondo del corredor norte. Guardé la llave dentro de mi taquilla personal (Taquilla Nº 14, en la sala de taquillas junto al vestuario) para que ni Mara ni la Directora puedan registrar mis cuadernos.”");

            // ====================================================================
            // ZONA 13: TAQUILLAS PERSONALES (X: 27 a 39, Z: 12 a 36, Y: 2.4m)
            // ====================================================================
            GameObject zone13 = new GameObject("13_Personal_Lockers");
            zone13.transform.parent = rootEnv;

            CreatePrimitive("Lockers_Floor", PrimitiveType.Cube, zone13.transform,
                new Vector3(33f, vestY - 0.25f, 24f), new Vector3(12f, 0.5f, 24f), matParquet);
            CreatePrimitive("Lockers_Ceiling", PrimitiveType.Cube, zone13.transform,
                new Vector3(33f, vestY + roomH, 24f), new Vector3(12f, 0.5f, 24f), matWallIvory);

            // Muros perimetrales de la sala de taquillas (con puerta norte en X = 33m, Z = 36m hacia el Depósito de Instrumentos)
            CreateWall("Lockers_Wall_West_N", zone13.transform, new Vector3(27f, vestY + 2.5f, 34f), new Vector3(0.5f, roomH, 4f), matWallIvory);
            CreateWall("Lockers_Wall_South", zone13.transform, new Vector3(33f, vestY + 2.5f, 12f), new Vector3(12f, roomH, 0.5f), matWallIvory);
            CreateWall("Lockers_Wall_East", zone13.transform, new Vector3(39f, vestY + 2.5f, 24f), new Vector3(0.5f, roomH, 24f), matWallIvory);
            CreateWall("Lockers_Wall_North_W", zone13.transform, new Vector3(29.475f, vestY + 2.5f, 36f), new Vector3(4.95f, roomH, 0.5f), matWallIvory);
            CreateWall("Lockers_Wall_North_E", zone13.transform, new Vector3(36.525f, vestY + 2.5f, 36f), new Vector3(4.95f, roomH, 0.5f), matWallIvory);
            CreateArchitecturalDoor("Door_InstrumentStorage_To_Lockers", zone13.transform, new Vector3(33f, vestY, 36f), Quaternion.identity, 1.8f, 3.2f, roomH, 0.5f, false, "", "DEPÓSITO DE INSTRUMENTOS — SECCIÓN 5", "", matMahogany, matGold, matWallIvory);

            // Iluminación fluorescente fría e irregular (contraste con los tonos cálidos del vestuario)
            CreateColdFluorescentFixture("Fluorescente_Taquillas_Sur", zone13.transform, new Vector3(33f, vestY + 4.75f, 18f), matIron, matColdFluorescent, 2.2f);
            CreateColdFluorescentFixture("Fluorescente_Taquillas_Norte", zone13.transform, new Vector3(33f, vestY + 4.75f, 30f), matIron, matColdFluorescent, 1.8f);

            // Filas de taquillas metálicas antiguas con amplio espacio para dos jugadores
            string[] lockerNamesW = { "Nº 01 - A. Rousseau", "Nº 03 - S. Lindqvist", "Nº 05 - V. Morozova", "Nº 07 - M. Bell", "Nº 09 - L. Chevalier", "Nº 11 - C. Valmont" };
            for (int i = 0; i < lockerNamesW.Length; i++)
            {
                float lz = 14.5f + i * 3.4f;
                if (Mathf.Abs(lz - 22f) < 1.8f) lz += 2.2f; // Respetar la puerta de entrada en Z = 22m
                bool openDoor = (i == 1 || i == 3 || i == 5);
                CreateMetalLockerUnit($"Taquilla_Oeste_{i + 1}", zone13.transform, new Vector3(28.0f, vestY, lz), Quaternion.Euler(0f, 90f, 0f),
                    lockerNamesW[i], openDoor, false, matRustedLocker, matGold, matCardboard);
            }

            string[] lockerNamesE = { "Nº 12 - H. Berg", "Nº 13 - I. Kovacs", "Nº 14 - E. Vesper", "Nº 15 - N. Volkova", "Nº 16 - M. Laurent", "Nº 18 - E. Novak" };
            for (int i = 0; i < lockerNamesE.Length; i++)
            {
                float lz = 15.0f + i * 3.6f;
                bool isElenaLocker = (i == 2); // Taquilla Nº 14 - Elena Vesper (Z = 22.2m, justo frente a la entrada)
                CreateMetalLockerUnit($"Taquilla_Este_{i + 1}", zone13.transform, new Vector3(38.0f, vestY, lz), Quaternion.Euler(0f, -90f, 0f),
                    lockerNamesE[i], true, isElenaLocker, matRustedLocker, matGold, matCardboard);

                if (isElenaLocker)
                {
                    // Marcas recientes en el polvo frente a la Taquilla Nº 14
                    DecoPiece("Marcas_Polvo_Taquilla14", PrimitiveType.Cube, zone13.transform,
                        new Vector3(36.6f, vestY + 0.015f, lz), new Vector3(1.4f, 0.01f, 1.2f), matClothCover);

                    // LLAVE DE LA HABITACIÓN PRIVADA DE ELENA dentro de la Taquilla Nº 14
                    GameObject keyElena = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    keyElena.name = "Key_Elena_Room";
                    keyElena.transform.parent = zone13.transform;
                    keyElena.transform.position = new Vector3(37.75f, vestY + 1.08f, lz - 0.15f);
                    keyElena.transform.localScale = new Vector3(0.12f, 0.03f, 0.26f);
                    keyElena.transform.rotation = Quaternion.Euler(0f, 45f, 90f);
                    keyElena.GetComponent<Renderer>().sharedMaterial = matGold;
                    WorldItem keyElenaComp = keyElena.AddComponent<WorldItem>();
                    keyElenaComp.Configure("key_elena_room", "Llave de la Habitación de Elena (Nº 14)", ItemType.Key);
                    EditorUtility.SetDirty(keyElenaComp);

                    // Documento en la Taquilla Nº 14 de Elena
                    CreateInteractiveWorldDoc("Doc_Taquilla_14_Elena", zone13.transform,
                        new Vector3(37.75f, vestY + 1.42f, lz + 0.12f), new Vector3(0.26f, 0.04f, 0.24f), matGold,
                        "doc_s3_locker14_elena",
                        "Taquilla Nº 14 (Elena Vesper) — Fotografía Colocada al Revés",
                        "A diferencia de las taquillas vacías, en el suelo frente a la Taquilla Nº 14 hay huellas recientes sobre el polvo.\n\n" +
                        "En el estante superior hay una fotografía colocada deliberadamente boca abajo y una nota escrita de puño y letra de Elena:\n" +
                        "“Si encuentras esta llave, entra en mi habitación al final del corredor norte y abre el compartimento del fondo de mi escritorio. Allí dejé la prueba de que la fotografía del pasillo fue alterada.”");
                }
            }

            // Documentos adicionales en otras taquillas abiertas (Mara Bell y Clara Valmont)
            CreateInteractiveWorldDoc("Doc_Taquilla_07_Mara", zone13.transform,
                new Vector3(28.25f, vestY + 1.08f, 26.9f), new Vector3(0.26f, 0.04f, 0.22f), matGold,
                "doc_s3_locker07_mara",
                "Taquilla Nº 07 (Mara Bell) — Carta entre Alumnas",
                "Una carta doblada escondida bajo unas cintas de ballet:\n\n" +
                "“12 de Octubre de 1924\n" +
                "Mara: La Directora Vale me confirmó hoy en su despacho que tú bailarás el acto final el día 14. Dijo que Elena ‘no estará en condiciones de subir al escenario’.\n" +
                "¿Cómo puede estar tan segura dos días antes del estreno? Ten cuidado, lo que planean en dirección no es una simple sustitución.”");

            CreateInteractiveWorldDoc("Doc_Taquilla_11_Recorte", zone13.transform,
                new Vector3(28.25f, vestY + 1.08f, 33.7f), new Vector3(0.26f, 0.04f, 0.22f), matGold,
                "doc_s3_locker11_clipping",
                "Taquilla Nº 11 (Clara Valmont) — Recorte de Periódico",
                "Un recorte amarillento de la crónica cultural local:\n\n" +
                "«CRISIS EN LA ACADEMIA VESPER: Rumores de clausura tras la misteriosa cancelación de 'El Lago de las Sombras'. Los inspectores encontraron las taquillas de varias alumnas completamente vaciadas durante la madrugada del 17 de octubre.»");

            // Restos de papel y polvo en el suelo del corredor de taquillas
            DecoPiece("Papel_Suelo_1", PrimitiveType.Cube, zone13.transform, new Vector3(31.5f, vestY + 0.02f, 19.2f), new Vector3(0.28f, 0.01f, 0.22f), matClothCover);
            DecoPiece("Papel_Suelo_2", PrimitiveType.Cube, zone13.transform, new Vector3(34.2f, vestY + 0.02f, 28.4f), new Vector3(0.25f, 0.01f, 0.30f), matClothCover);

            // ====================================================================
            // ZONA 14: HABITACIÓN DE ELENA (Corredor Z: 46..52, Habitación X: 12..26, Z: 52..66)
            // ====================================================================
            GameObject zone14 = new GameObject("14_Elena_Private_Room");
            zone14.transform.parent = rootEnv;

            // Pequeño corredor privado de acceso (X: 17 a 21, Z: 46 a 52)
            CreatePrimitive("Elena_Corr_Floor", PrimitiveType.Cube, zone14.transform,
                new Vector3(19f, vestY - 0.25f, 49f), new Vector3(4f, 0.5f, 6f), matParquet);
            CreatePrimitive("Elena_Corr_Carpet", PrimitiveType.Cube, zone14.transform,
                new Vector3(19f, vestY + 0.02f, 49f), new Vector3(2.0f, 0.04f, 6f), matCarpet);
            CreatePrimitive("Elena_Corr_Ceiling", PrimitiveType.Cube, zone14.transform,
                new Vector3(19f, vestY + roomH, 49f), new Vector3(4f, 0.5f, 6f), matWallIvory);
            CreateWall("Elena_Corr_Wall_West", zone14.transform, new Vector3(17f, vestY + 2.5f, 49f), new Vector3(0.5f, roomH, 6f), matWallIvory);
            CreateWall("Elena_Corr_Wall_East", zone14.transform, new Vector3(21f, vestY + 2.5f, 49f), new Vector3(0.5f, roomH, 6f), matWallIvory);
            CreateOrnateSconce("Sconce_Elena_Corr", zone14.transform, new Vector3(17.3f, vestY + 2.9f, 49f), matGold, matLanternGlow, new Color(1f, 0.80f, 0.48f), 1.6f);

            // Habitación privada de Elena Vesper (X: 12 a 26, Z: 52 a 66)
            CreatePrimitive("Elena_Room_Floor", PrimitiveType.Cube, zone14.transform,
                new Vector3(19f, vestY - 0.25f, 59f), new Vector3(14f, 0.5f, 14f), matParquet);
            CreatePrimitive("Elena_Room_Ceiling", PrimitiveType.Cube, zone14.transform,
                new Vector3(19f, vestY + roomH, 59f), new Vector3(14f, 0.5f, 14f), matWallIvory);
            CreatePrimitive("Elena_Room_Rug", PrimitiveType.Cube, zone14.transform,
                new Vector3(19f, vestY + 0.02f, 59f), new Vector3(7.5f, 0.04f, 7.5f), matCarpet);

            // Muro sur de la habitación (Z = 52) con Puerta Cerrada con Llave (key_elena_room) en X = 19m (abre hacia adentro de la habitación)
            CreateWall("Elena_Wall_South_W", zone14.transform, new Vector3(14.975f, vestY + 2.5f, 52f), new Vector3(5.95f, roomH, 0.5f), matWallIvory);
            CreateWall("Elena_Wall_South_E", zone14.transform, new Vector3(23.025f, vestY + 2.5f, 52f), new Vector3(5.95f, roomH, 0.5f), matWallIvory);
            CreateArchitecturalDoor("Door_To_Elena_Room", zone14.transform, new Vector3(19f, vestY, 52f), Quaternion.identity,
                1.8f, 3.2f, roomH, 0.5f, true, "key_elena_room", "HABITACIÓN DE ELENA VESPER",
                "Habitación Privada de Elena Vesper — Cerrada con llave (Buscar en Taquilla Nº 14)", matMahogany, matGold, matWallIvory, false, -90f);

            // Muros oeste, este y norte de la habitación de Elena
            CreateWall("Elena_Wall_West", zone14.transform, new Vector3(12f, vestY + 2.5f, 59f), new Vector3(0.5f, roomH, 14f), matWallIvory);
            CreateWall("Elena_Wall_East", zone14.transform, new Vector3(26f, vestY + 2.5f, 59f), new Vector3(0.5f, roomH, 14f), matWallIvory);
            CreateWall("Elena_Wall_North", zone14.transform, new Vector3(19f, vestY + 2.5f, 66f), new Vector3(14f, roomH, 0.5f), matWallIvory);

            // Ventana norte con cortinas borgoña parcialmente abiertas y luz lunar fría
            DecoPiece("Elena_Window_Frame", PrimitiveType.Cube, zone14.transform, new Vector3(19f, vestY + 2.7f, 65.72f), new Vector3(3.2f, 2.8f, 0.12f), matMahogany);
            DecoPiece("Elena_Window_Glass", PrimitiveType.Cube, zone14.transform, new Vector3(19f, vestY + 2.7f, 65.74f), new Vector3(2.8f, 2.4f, 0.06f), matColdFluorescent);
            DecoPiece("Elena_Curtain_L", PrimitiveType.Cube, zone14.transform, new Vector3(17.3f, vestY + 2.6f, 65.55f), new Vector3(0.85f, 3.6f, 0.25f), matVelvetDrapes);
            DecoPiece("Elena_Curtain_R", PrimitiveType.Cube, zone14.transform, new Vector3(20.7f, vestY + 2.6f, 65.55f), new Vector3(0.85f, 3.6f, 0.25f), matVelvetDrapes);
            CreatePictureSpotlight("Spotlight_Moon_ElenaRoom", zone14.transform,
                new Vector3(19f, vestY + 3.8f, 65.0f), new Vector3(19f, vestY + 0.8f, 58.5f), new Color(0.62f, 0.78f, 0.98f), 2.8f);

            // Cama individual con ropa de cama marfil envejecida y mesita de noche
            CreatePrimitive("Elena_Bed_Frame", PrimitiveType.Cube, zone14.transform, new Vector3(14.2f, vestY + 0.35f, 62.5f), new Vector3(2.6f, 0.5f, 1.5f), matMahogany);
            CreatePrimitive("Elena_Bed_Headboard", PrimitiveType.Cube, zone14.transform, new Vector3(13.0f, vestY + 0.85f, 62.5f), new Vector3(0.2f, 1.3f, 1.5f), matMahogany);
            CreatePrimitive("Elena_Bed_Mattress", PrimitiveType.Cube, zone14.transform, new Vector3(14.3f, vestY + 0.62f, 62.5f), new Vector3(2.3f, 0.22f, 1.35f), matClothCover);
            CreatePrimitive("Elena_Bed_Pillow", PrimitiveType.Cube, zone14.transform, new Vector3(13.45f, vestY + 0.78f, 62.5f), new Vector3(0.45f, 0.14f, 0.85f), matTutuWhite);
            CreatePrimitive("Elena_Nightstand", PrimitiveType.Cube, zone14.transform, new Vector3(13.2f, vestY + 0.45f, 60.5f), new Vector3(0.7f, 0.9f, 0.7f), matMahogany);

            // Armario personal y pequeña estantería con libros de Elena
            CreateInteractiveWardrobe("Armario_Personal_Elena", zone14.transform, new Vector3(13.2f, vestY, 55.5f), Quaternion.Euler(0f, 90f, 0f), matMahogany, matGold, matTutuBlack, true);
            CreatePrimitive("Estanteria_Libros_Elena", PrimitiveType.Cube, zone14.transform, new Vector3(24.8f, vestY + 1.4f, 55.5f), new Vector3(0.7f, 2.8f, 2.8f), matWoodArchive);

            // Escritorio de madera oscura de Elena, silla, joyero abierto y compartimento de fondo falso
            CreatePrimitive("Elena_Desk_Main", PrimitiveType.Cube, zone14.transform, new Vector3(23.8f, vestY + 0.6f, 61.5f), new Vector3(1.5f, 1.2f, 2.8f), matMahogany);
            CreatePrimitive("Elena_Desk_Chair", PrimitiveType.Cube, zone14.transform, new Vector3(22.3f, vestY + 0.55f, 61.5f), new Vector3(0.8f, 1.1f, 0.8f), matVelvetDrapes);

            // Pequeño joyero abierto sobre el escritorio
            CreatePrimitive("Joyero_Abierto_Base", PrimitiveType.Cube, zone14.transform, new Vector3(23.6f, vestY + 1.25f, 60.6f), new Vector3(0.32f, 0.12f, 0.24f), matMahogany);
            DecoPiece("Joyero_Interior_Terciopelo", PrimitiveType.Cube, zone14.transform, new Vector3(23.6f, vestY + 1.32f, 60.6f), new Vector3(0.28f, 0.03f, 0.20f), matVelvetDrapes);

            // Compartimento oculto / cajón de fondo falso interactivo junto al escritorio
            GameObject secretDrawer = new GameObject("Cajon_Fondo_Falso_Elena");
            secretDrawer.transform.parent = zone14.transform;
            secretDrawer.transform.position = new Vector3(23.0f, vestY + 0.95f, 62.4f);
            GameObject drawerHinge = new GameObject("Drawer_Hinge");
            drawerHinge.transform.parent = secretDrawer.transform;
            drawerHinge.transform.localPosition = Vector3.zero;
            CreatePrimitive("Drawer_Front", PrimitiveType.Cube, drawerHinge.transform, new Vector3(0f, 0f, 0.25f), new Vector3(0.45f, 0.22f, 0.55f), matMahogany);
            WorldDoor drawerDoor = secretDrawer.AddComponent<WorldDoor>();
            drawerDoor.Configure(drawerHinge.transform, 75f, 0.7f, false, false, "", "Cajon_Fondo_Falso_Elena", "");
            EditorUtility.SetDirty(drawerDoor);

            // Fotografías de ensayos e imagen de Elena con el vestuario de Cisne Negro en la pared (a la izquierda de la puerta, nunca bloqueando el paso)
            CreateFramedArtPortrait("Retrato_Elena_CisneNegro_Hab", zone14.transform,
                new Vector3(25.68f, vestY + 2.6f, 61.5f), new Vector3(0.12f, 2.1f, 1.6f), matGold, matPortraitElena,
                "Retrato Personal: Elena Vesper con el Traje del Cisne Negro");
            CreateFramedArtPortrait("Foto_Ensayo_Hab_Elena", zone14.transform,
                new Vector3(15.0f, vestY + 2.5f, 52.32f), new Vector3(1.5f, 1.2f, 0.10f), matGold, matPhotoTroupe1923,
                "Fotografía de Ensayos Privados (Otoño de 1924)");

            CreateOrnateSconce("Sconce_Elena_Room", zone14.transform, new Vector3(12.35f, vestY + 2.9f, 59f), matGold, matLanternGlow, new Color(1f, 0.82f, 0.52f), 1.8f);

            // ELEMENTOS NARRATIVOS Y REVELACIÓN PRINCIPAL EN LA HABITACIÓN DE ELENA (ZONA 14)
            CreateInteractiveWorldDoc("Doc_Carta_Incompleta_Elena", zone14.transform,
                new Vector3(23.5f, vestY + 1.24f, 61.5f), new Vector3(0.38f, 0.04f, 0.30f), matGold,
                "doc_s3_elena_incomplete_letter",
                "Carta Incompleta de Elena Vesper — [REVELACIÓN PRINCIPAL]",
                "Manuscrito con fecha del 16 DE OCTUBRE DE 1924 (dos días DESPUÉS de su supuesta desaparición oficial el 14 de octubre):\n\n" +
                "“Querida hermana:\n" +
                "Si algún día logras leer esto, no creas ni una palabra de lo que Isadora Vale escribió en el libro de recepción. Anotaron que desaparecí durante el ensayo general de la noche del 14 de octubre, pero hoy es 16 de octubre y sigo aquí.\n\n" +
                "Me encerraron en el ala privada después de que descubriera lo que ocurrió realmente tras el telón del teatro. La cronología oficial es una farsa para encubrir lo que pasó la noche del estreno.”");

            CreateInteractiveWorldDoc("Doc_Foto_Contradictoria_Elena", zone14.transform,
                new Vector3(23.15f, vestY + 1.12f, 62.45f), new Vector3(0.34f, 0.04f, 0.28f), matSatinRibbon,
                "doc_s3_contradictory_photo",
                "Fotografía Original sin Retocar — Contradicción del Pasillo de los Retratos",
                "Oculta en el doble fondo del escritorio de Elena:\n\n" +
                "Es la fotografía original que sirvió de modelo para el cuadro de 'El Lago de las Sombras' expuesto en el Pasillo de los Retratos (Sección 2).\n\n" +
                "¡CONTRADICCIÓN CLAVE!: En el cuadro del pasillo, Elena aparece completamente sola sobre el escenario. Sin embargo, en esta fotografía original sin retocar, otra figura vestida con un traje idéntico de Cisne Negro está de pie justo detrás de ella en la penumbra.\n\n" +
                "Anotación al dorso: «Regresa al Pasillo de los Retratos y examina el reverso del lienzo de 'El Lago de las Sombras'. Pintaron encima el 15 de octubre para borrarla.»");

            CreateInteractiveWorldDoc("Doc_Partitura_Incompleta_Elena", zone14.transform,
                new Vector3(13.2f, vestY + 0.94f, 60.5f), new Vector3(0.35f, 0.04f, 0.26f), matGold,
                "doc_s3_incomplete_score",
                "Partitura Incompleta — Fragmento del Archivo Musical",
                "Una hoja de pentagrama arrancada con varias notas tachadas violentamente:\n\n" +
                "«Coda en Mi Menor — Variación del Espejo: Mi - Sol - Si - Do# - Re»\n\n" +
                "Nota al margen de Elena:\n" +
                "“El maestro de orquesta arrancó la segunda mitad de esta partitura y la encerró en el Archivo Musical del pasillo principal. Cuando tocan esos compases en el piano del ensayo, las luces del teatro responden.”");

            // ====================================================================
            // ZONA 15: LAVABOS Y DUCHAS ANTIGUAS (X: 11 a 27, Z: 0 a 12, Y: 2.4m)
            // ====================================================================
            GameObject zone15 = new GameObject("15_Vintage_Washrooms_And_Showers");
            zone15.transform.parent = rootEnv;

            CreatePrimitive("Wash_Floor", PrimitiveType.Cube, zone15.transform,
                new Vector3(19f, vestY - 0.25f, 6f), new Vector3(16f, 0.5f, 12f), matCeramicTile);
            CreatePrimitive("Wash_Ceiling", PrimitiveType.Cube, zone15.transform,
                new Vector3(19f, vestY + roomH, 6f), new Vector3(16f, 0.5f, 12f), matWallIvory);

            // Muros perimetrales de lavabos y duchas (con puerta en el muro sur X = 19.0m, Z = 0.0m hacia el Sótano de Mantenimiento)
            CreateWall("Wash_Wall_West", zone15.transform, new Vector3(11f, vestY + 2.5f, 6f), new Vector3(0.5f, roomH, 12f), matWallIvory);
            CreateWall("Wash_Wall_South_W", zone15.transform, new Vector3(14.475f, vestY + 2.5f, 0f), new Vector3(6.95f, roomH, 0.5f), matWallIvory);
            CreateWall("Wash_Wall_South_E", zone15.transform, new Vector3(23.525f, vestY + 2.5f, 0f), new Vector3(6.95f, roomH, 0.5f), matWallIvory);
            CreateArchitecturalDoor("Door_Washroom_To_MaintenanceBasement", zone15.transform,
                new Vector3(19f, vestY, 0f), Quaternion.identity,
                1.8f, 3.2f, roomH, 0.5f, false, "",
                "SÓTANO DE MANTENIMIENTO — ESCALERA TÉCNICA SUBTERRÁNEA (ZONA 41)", "", matIron, matGold, matWallIvory);
            CreateWall("Wash_Wall_East", zone15.transform, new Vector3(27f, vestY + 2.5f, 6f), new Vector3(0.5f, roomH, 12f), matWallIvory);

            // Revestimiento parcial de azulejos marfil con juntas oscurecidas por la humedad
            DecoPiece("Wash_Tile_Wainscot_W", PrimitiveType.Cube, zone15.transform, new Vector3(11.28f, vestY + 1.2f, 6f), new Vector3(0.06f, 2.4f, 11.4f), matCeramicTile);
            DecoPiece("Wash_Tile_Wainscot_E", PrimitiveType.Cube, zone15.transform, new Vector3(26.72f, vestY + 1.2f, 6f), new Vector3(0.06f, 2.4f, 11.4f), matCeramicTile);
            DecoPiece("Wash_Tile_Wainscot_S_W", PrimitiveType.Cube, zone15.transform, new Vector3(14.5f, vestY + 1.2f, 0.28f), new Vector3(6.4f, 2.4f, 0.06f), matCeramicTile);
            DecoPiece("Wash_Tile_Wainscot_S_E", PrimitiveType.Cube, zone15.transform, new Vector3(23.5f, vestY + 1.2f, 0.28f), new Vector3(6.4f, 2.4f, 0.06f), matCeramicTile);

            // Tuberías antiguas oxidadas recorriendo paredes y techo
            DecoPiece("Pipe_Ceiling_Main", PrimitiveType.Cylinder, zone15.transform,
                new Vector3(19f, vestY + 4.65f, 6f), new Vector3(0.14f, 5.8f, 0.14f), matRustedLocker).transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            DecoPiece("Pipe_Wall_West", PrimitiveType.Cylinder, zone15.transform,
                new Vector3(11.38f, vestY + 3.8f, 6f), new Vector3(0.11f, 5.8f, 0.11f), matRustedLocker).transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            DecoPiece("Pipe_Vertical_Corner", PrimitiveType.Cylinder, zone15.transform,
                new Vector3(11.4f, vestY + 2.4f, 1.2f), new Vector3(0.12f, 2.4f, 0.12f), matRustedLocker);

            // Ventana pequeña alta en el muro sur con tenue luz azulada lunar
            DecoPiece("Wash_Small_Window", PrimitiveType.Cube, zone15.transform, new Vector3(19f, vestY + 3.9f, 0.28f), new Vector3(1.8f, 0.9f, 0.10f), matColdFluorescent);
            CreatePictureSpotlight("Spotlight_Moon_Washroom", zone15.transform,
                new Vector3(19f, vestY + 3.8f, 0.8f), new Vector3(19f, vestY + 0.5f, 6.5f), new Color(0.52f, 0.70f, 0.95f), 2.1f);

            // Fila de 4 lavabos de porcelana envejecida, grifos metálicos y espejos rectangulares en el muro oeste
            for (int s = 0; s < 4; s++)
            {
                float sz = 2.2f + s * 2.4f;
                bool hasDripAnomaly = (s == 0); // El último lavabo del fondo gotea cuando te alejas
                CreatePorcelainWashbasin($"Lavabo_Porcelana_{s + 1}", zone15.transform,
                    new Vector3(11.75f, vestY, sz), matPorcelain, matIron, matMirrorGlass, matColdFluorescent, hasDripAnomaly);
            }

            // 4 Cabinas de ducha en el lado este con tabiques, cortinas descoloridas y puertas entreabiertas
            for (int c = 0; c < 4; c++)
            {
                float cz = 2.0f + c * 2.5f;
                bool hasShowerAnomaly = (c == 2); // Cabina 3 con anomalía acústica de agua/goteo cuando los jugadores se alejan
                CreateShowerStall($"Cabina_Ducha_{c + 1}", zone15.transform,
                    new Vector3(24.8f, vestY, cz), matCeramicTile, matRustedLocker, matClothCover, matColdFluorescent, hasShowerAnomaly);
            }

            // Luz tenue húmeda en el techo
            CreateOrnateSconce("Sconce_Washroom", zone15.transform, new Vector3(15.5f, vestY + 3.4f, 11.65f), matGold, matLanternGlow, new Color(0.95f, 0.82f, 0.58f), 1.4f);

            // ELEMENTOS NARRATIVOS DE LA ZONA 15 (LAVABOS Y DUCHAS ANTIGUAS)
            CreateInteractiveWorldDoc("Doc_Inscripcion_Lavabo", zone15.transform,
                new Vector3(11.8f, vestY + 0.95f, 2.2f), new Vector3(0.32f, 0.04f, 0.24f), matPorcelain,
                "doc_s3_washbasin_inscription",
                "Inscripción Parcialmente Borrada en el Azulejo del Lavabo",
                "Grabado con un objeto punzante en la junta húmeda bajo el espejo del último lavabo:\n\n" +
                "«18 de Octubre — El agua helada no quita el olor a humo ni la pintura negra de las manos.\n" +
                "Cerraron las verjas de la calle creyendo que ya no quedaba nadie adentro, pero alguien sigue abriendo las duchas cada madrugada.»");

            CreateInteractiveWorldDoc("Doc_Objeto_Olvidado_Ducha", zone15.transform,
                new Vector3(24.6f, vestY + 0.12f, 7.0f), new Vector3(0.28f, 0.06f, 0.22f), matGold,
                "doc_s3_shower_forgotten_item",
                "Medallón Oxidado y Rastro Reciente en la Cabina de Ducha Nº 3",
                "Junto al desagüe de la tercera cabina de ducha hay un medallón de latón empañado por la humedad y restos de barro seco que no pertenecen al suelo de parquet del vestuario.\n\n" +
                "Dentro del medallón se conserva una miniatura borrosa de Elena y una inscripción:\n" +
                "«Quien estuvo aquí después de la clausura venía desde los niveles inferiores buscando agua limpia.»");
        }

        private static void CreateGarmentRackWithTutus(string name, Transform parent, Vector3 pos, Material ironMat, Material goldMat, Material whiteTutuMat, Material blackTutuMat, Material clothMat)
        {
            GameObject rack = new GameObject(name);
            rack.transform.parent = parent;
            rack.transform.position = pos;

            CreatePrimitive("Rack_Base_L", PrimitiveType.Cube, rack.transform, new Vector3(0f, 0.05f, -1.4f), new Vector3(0.7f, 0.08f, 0.12f), ironMat);
            CreatePrimitive("Rack_Base_R", PrimitiveType.Cube, rack.transform, new Vector3(0f, 0.05f, 1.4f), new Vector3(0.7f, 0.08f, 0.12f), ironMat);
            CreatePrimitive("Rack_Pole_L", PrimitiveType.Cylinder, rack.transform, new Vector3(0f, 1.0f, -1.4f), new Vector3(0.06f, 0.95f, 0.06f), ironMat);
            CreatePrimitive("Rack_Pole_R", PrimitiveType.Cylinder, rack.transform, new Vector3(0f, 1.0f, 1.4f), new Vector3(0.06f, 0.95f, 0.06f), ironMat);
            DecoPiece("Rack_Top_Bar", PrimitiveType.Cylinder, rack.transform, new Vector3(0f, 1.92f, 0f), new Vector3(0.05f, 1.45f, 0.05f), goldMat).transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // Tutús colgados y prendas cubiertas de polvo
            float[] zOffsets = { -0.9f, -0.25f, 0.4f, 1.0f };
            for (int i = 0; i < zOffsets.Length; i++)
            {
                Material tutuMat = (i == 1) ? blackTutuMat : ((i % 2 == 0) ? whiteTutuMat : clothMat);
                DecoPiece($"Hanger_{i}", PrimitiveType.Cube, rack.transform, new Vector3(0f, 1.82f, zOffsets[i]), new Vector3(0.35f, 0.04f, 0.03f), goldMat);
                DecoPiece($"Bodice_{i}", PrimitiveType.Cylinder, rack.transform, new Vector3(0f, 1.48f, zOffsets[i]), new Vector3(0.26f, 0.28f, 0.18f), tutuMat);
                DecoPiece($"Tutu_Skirt_{i}", PrimitiveType.Cylinder, rack.transform, new Vector3(0f, 1.18f, zOffsets[i]), new Vector3(0.78f, 0.09f, 0.78f), tutuMat);
            }
        }

        private static void CreateInteractiveWardrobe(string name, Transform parent, Vector3 pos, Quaternion rot, Material woodMat, Material goldMat, Material insideClothMat, bool interactiveDoor)
        {
            GameObject w = new GameObject(name);
            w.transform.parent = parent;
            w.transform.position = pos;
            w.transform.rotation = rot;

            // Cuerpo hueco del armario (trasera, laterales, techo y base)
            CreatePrimitive("Wardrobe_Back", PrimitiveType.Cube, w.transform, new Vector3(0f, 1.25f, -0.36f), new Vector3(1.5f, 2.5f, 0.08f), woodMat);
            CreatePrimitive("Wardrobe_Side_L", PrimitiveType.Cube, w.transform, new Vector3(-0.71f, 1.25f, 0f), new Vector3(0.08f, 2.5f, 0.8f), woodMat);
            CreatePrimitive("Wardrobe_Side_R", PrimitiveType.Cube, w.transform, new Vector3(0.71f, 1.25f, 0f), new Vector3(0.08f, 2.5f, 0.8f), woodMat);
            CreatePrimitive("Wardrobe_Top", PrimitiveType.Cube, w.transform, new Vector3(0f, 2.46f, 0f), new Vector3(1.58f, 0.12f, 0.86f), woodMat);
            CreatePrimitive("Wardrobe_Bottom", PrimitiveType.Cube, w.transform, new Vector3(0f, 0.08f, 0f), new Vector3(1.5f, 0.16f, 0.8f), woodMat);
            DecoPiece("Wardrobe_Inside_Garment", PrimitiveType.Cylinder, w.transform, new Vector3(-0.25f, 1.35f, -0.05f), new Vector3(0.52f, 0.45f, 0.42f), insideClothMat);

            // Puerta derecha fija entreabierta y puerta izquierda interactiva
            CreatePrimitive("Wardrobe_Door_R", PrimitiveType.Cube, w.transform, new Vector3(0.35f, 1.25f, 0.38f), new Vector3(0.66f, 2.26f, 0.05f), woodMat);

            GameObject doorHinge = new GameObject("Wardrobe_Door_L_Hinge");
            doorHinge.transform.parent = w.transform;
            doorHinge.transform.localPosition = new Vector3(-0.68f, 0f, 0.38f);
            doorHinge.transform.localRotation = Quaternion.identity;

            CreatePrimitive("Wardrobe_Door_L_Leaf", PrimitiveType.Cube, doorHinge.transform, new Vector3(0.33f, 1.25f, 0f), new Vector3(0.66f, 2.26f, 0.05f), woodMat);
            DecoPiece("Wardrobe_Handle_L", PrimitiveType.Sphere, doorHinge.transform, new Vector3(0.58f, 1.20f, 0.05f), new Vector3(0.06f, 0.06f, 0.06f), goldMat);

            if (interactiveDoor)
            {
                WorldDoor wd = w.AddComponent<WorldDoor>();
                wd.Configure(doorHinge.transform, -95f, 0.8f, false, false, "", name, "");
                EditorUtility.SetDirty(wd);
            }
        }

        private static void CreateVanityStation(string name, Transform parent, Vector3 pos, Quaternion rot,
            Material woodMat, Material goldMat, Material mirrorMat, Material velvetMat, Material glowMat,
            Material porcelainMat, Material shadowMat, bool bulbLit, bool isAnomalyStation)
        {
            GameObject station = new GameObject(name);
            station.transform.parent = parent;
            station.transform.position = pos;
            station.transform.rotation = rot;

            // Mesa de tocador de madera oscura
            CreatePrimitive("Vanity_Table_Top", PrimitiveType.Cube, station.transform, new Vector3(0f, 0.80f, 0f), new Vector3(1.7f, 0.10f, 0.75f), woodMat);
            CreatePrimitive("Vanity_Leg_L", PrimitiveType.Cube, station.transform, new Vector3(-0.75f, 0.38f, 0f), new Vector3(0.12f, 0.76f, 0.65f), woodMat);
            CreatePrimitive("Vanity_Leg_R", PrimitiveType.Cube, station.transform, new Vector3(0.75f, 0.38f, 0f), new Vector3(0.12f, 0.76f, 0.65f), woodMat);

            // Silla tapizada en terciopelo borgoña
            CreatePrimitive("Vanity_Stool_Seat", PrimitiveType.Cylinder, station.transform, new Vector3(0f, 0.46f, 0.68f), new Vector3(0.52f, 0.08f, 0.52f), velvetMat);
            CreatePrimitive("Vanity_Stool_Base", PrimitiveType.Cylinder, station.transform, new Vector3(0f, 0.22f, 0.68f), new Vector3(0.14f, 0.22f, 0.14f), woodMat);

            // Marco ovalado ornamentado y espejo antiguo
            DecoPiece("Mirror_Frame_Oval", PrimitiveType.Cylinder, station.transform,
                new Vector3(0f, 1.62f, -0.30f), new Vector3(1.15f, 0.04f, 1.48f), goldMat).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            DecoPiece("Mirror_Glass_Surface", PrimitiveType.Cylinder, station.transform,
                new Vector3(0f, 1.62f, -0.27f), new Vector3(1.02f, 0.03f, 1.34f), mirrorMat).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            // Bombillas antiguas alrededor del espejo (algunas encendidas, otras apagadas/rotas)
            for (int b = -2; b <= 2; b++)
            {
                float angle = b * 0.48f;
                float bx = Mathf.Sin(angle) * 0.56f;
                float by = 1.62f + Mathf.Cos(angle) * 0.65f;
                bool broken = (!bulbLit && b % 2 == 0) || (b == 1);
                DecoPiece($"Vanity_Bulb_{b}", PrimitiveType.Sphere, station.transform,
                    new Vector3(bx, by, -0.22f), new Vector3(0.07f, 0.07f, 0.07f), broken ? woodMat : glowMat);
            }

            Light vanityPointLight = null;
            if (bulbLit)
            {
                GameObject lObj = new GameObject("Vanity_Warm_Light");
                lObj.transform.parent = station.transform;
                lObj.transform.localPosition = new Vector3(0f, 1.85f, 0.15f);
                vanityPointLight = lObj.AddComponent<Light>();
                vanityPointLight.type = LightType.Point;
                vanityPointLight.color = new Color(1.0f, 0.82f, 0.52f);
                vanityPointLight.range = 6.5f;
                vanityPointLight.intensity = 1.45f;
            }

            // Frascos de perfume, polveras y cepillos sobre la mesa
            DecoPiece("Perfume_Bottle_Real", PrimitiveType.Cylinder, station.transform,
                new Vector3(-0.48f, 0.90f, 0.05f), new Vector3(0.10f, 0.09f, 0.10f), goldMat);
            DecoPiece("Powder_Compact", PrimitiveType.Cylinder, station.transform,
                new Vector3(0.42f, 0.87f, 0.08f), new Vector3(0.16f, 0.03f, 0.16f), porcelainMat);

            if (isAnomalyStation)
            {
                // Discrepancia sutil de objeto: un frasco caído a la izquierda en la mesa real, pero erguido al lado opuesto tras el cristal
                GameObject fallenBottle = DecoPiece("Fallen_Perfume_Discrepancy", PrimitiveType.Cylinder, station.transform,
                    new Vector3(-0.25f, 0.88f, -0.08f), new Vector3(0.08f, 0.11f, 0.08f), porcelainMat);
                fallenBottle.transform.localRotation = Quaternion.Euler(0f, 0f, 82f);

                // Silueta fantasmal de bailarina vestida de Cisne Negro que aparece un instante en el reflejo
                GameObject silhouette = new GameObject("Mirror_Anomaly_Silhouette");
                silhouette.transform.parent = station.transform;
                silhouette.transform.localPosition = new Vector3(0f, 0f, -0.21f);
                silhouette.transform.localRotation = Quaternion.identity;

                DecoPiece("Silhouette_Head", PrimitiveType.Sphere, silhouette.transform, new Vector3(0f, 1.88f, 0f), new Vector3(0.22f, 0.26f, 0.04f), shadowMat);
                DecoPiece("Silhouette_Torso", PrimitiveType.Capsule, silhouette.transform, new Vector3(0f, 1.50f, 0f), new Vector3(0.28f, 0.34f, 0.04f), shadowMat);
                DecoPiece("Silhouette_BlackTutu", PrimitiveType.Cylinder, silhouette.transform, new Vector3(0f, 1.32f, 0f), new Vector3(0.75f, 0.06f, 0.08f), shadowMat);

                MirrorAnomalyController anomaly = station.AddComponent<MirrorAnomalyController>();
                anomaly.Configure(silhouette, vanityPointLight);
                SerializedObject serAnom = new SerializedObject(anomaly);
                SerializedProperty pSil = serAnom.FindProperty("silhouetteRoot");
                if (pSil != null) pSil.objectReferenceValue = silhouette;
                SerializedProperty pLit = serAnom.FindProperty("vanityLight");
                if (pLit != null) pLit.objectReferenceValue = vanityPointLight;
                serAnom.ApplyModifiedProperties();
                EditorUtility.SetDirty(anomaly);
            }
        }

        private static void CreateColdFluorescentFixture(string name, Transform parent, Vector3 pos, Material housingMat, Material tubeGlowMat, float intensity)
        {
            GameObject fix = new GameObject(name);
            fix.transform.parent = parent;
            fix.transform.position = pos;

            DecoPiece("Fluorescent_Housing", PrimitiveType.Cube, fix.transform, new Vector3(0f, 0.08f, 0f), new Vector3(0.45f, 0.08f, 2.8f), housingMat);
            DecoPiece("Fluorescent_Tube", PrimitiveType.Cylinder, fix.transform, Vector3.zero, new Vector3(0.10f, 1.3f, 0.10f), tubeGlowMat).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            GameObject lObj = new GameObject("Cold_Light");
            lObj.transform.parent = fix.transform;
            lObj.transform.localPosition = new Vector3(0f, -0.15f, 0f);
            Light l = lObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.72f, 0.88f, 1.0f);
            l.range = 14f;
            l.intensity = intensity;
        }

        private static void CreateMetalLockerUnit(string name, Transform parent, Vector3 pos, Quaternion rot, string plateLabel, bool openable, bool startSlightlyOpen, Material metalMat, Material goldMat, Material insideMat)
        {
            GameObject locker = new GameObject(name);
            locker.transform.parent = parent;
            locker.transform.position = pos;
            locker.transform.rotation = rot;

            // Caja metálica con estantes interiores visibles
            CreatePrimitive("Locker_Back", PrimitiveType.Cube, locker.transform, new Vector3(0f, 1.15f, -0.32f), new Vector3(0.95f, 2.3f, 0.05f), metalMat);
            CreatePrimitive("Locker_Side_L", PrimitiveType.Cube, locker.transform, new Vector3(-0.45f, 1.15f, 0f), new Vector3(0.05f, 2.3f, 0.68f), metalMat);
            CreatePrimitive("Locker_Side_R", PrimitiveType.Cube, locker.transform, new Vector3(0.45f, 1.15f, 0f), new Vector3(0.05f, 2.3f, 0.68f), metalMat);
            CreatePrimitive("Locker_Top", PrimitiveType.Cube, locker.transform, new Vector3(0f, 2.27f, 0f), new Vector3(0.95f, 0.06f, 0.68f), metalMat);
            CreatePrimitive("Locker_Bottom", PrimitiveType.Cube, locker.transform, new Vector3(0f, 0.05f, 0f), new Vector3(0.95f, 0.10f, 0.68f), metalMat);
            CreatePrimitive("Locker_Shelf_Mid", PrimitiveType.Cube, locker.transform, new Vector3(0f, 1.02f, 0f), new Vector3(0.86f, 0.03f, 0.60f), metalMat);
            CreatePrimitive("Locker_Shelf_Upper", PrimitiveType.Cube, locker.transform, new Vector3(0f, 1.38f, 0f), new Vector3(0.86f, 0.03f, 0.60f), metalMat);

            // Bisagra y puerta metálica con placa de identificación
            GameObject hinge = new GameObject("Locker_Door_Hinge");
            hinge.transform.parent = locker.transform;
            hinge.transform.localPosition = new Vector3(-0.43f, 0f, 0.32f);
            hinge.transform.localRotation = startSlightlyOpen ? Quaternion.Euler(0f, -65f, 0f) : Quaternion.identity;

            CreatePrimitive("Locker_Door_Panel", PrimitiveType.Cube, hinge.transform, new Vector3(0.43f, 1.15f, 0f), new Vector3(0.86f, 2.18f, 0.04f), metalMat);
            DecoPiece("Locker_NamePlate", PrimitiveType.Cube, hinge.transform, new Vector3(0.43f, 1.85f, 0.03f), new Vector3(0.42f, 0.11f, 0.015f), goldMat);
            DecoPiece("Locker_Handle", PrimitiveType.Cube, hinge.transform, new Vector3(0.76f, 1.15f, 0.04f), new Vector3(0.04f, 0.18f, 0.04f), goldMat);

            if (openable)
            {
                WorldDoor doorComp = locker.AddComponent<WorldDoor>();
                doorComp.Configure(hinge.transform, startSlightlyOpen ? 65f : -85f, 0.65f, startSlightlyOpen, false, "", $"Taquilla_{plateLabel}", "");
                SerializedObject serDoor = new SerializedObject(doorComp);
                SerializedProperty pH = serDoor.FindProperty("doorHinge");
                if (pH != null) pH.objectReferenceValue = hinge.transform;
                SerializedProperty pO = serDoor.FindProperty("isOpen");
                if (pO != null) pO.boolValue = startSlightlyOpen;
                SerializedProperty pId = serDoor.FindProperty("id");
                if (pId != null) pId.stringValue = $"Taquilla ({plateLabel})";
                serDoor.ApplyModifiedProperties();
                EditorUtility.SetDirty(doorComp);
            }
        }

        private static void CreatePorcelainWashbasin(string name, Transform parent, Vector3 pos, Material porcelainMat, Material metalMat, Material mirrorMat, Material waterMat, bool addDripAnomaly)
        {
            GameObject sink = new GameObject(name);
            sink.transform.parent = parent;
            sink.transform.position = pos;

            // Pedestal y lavabo de porcelana envejecida
            CreatePrimitive("Sink_Pedestal", PrimitiveType.Cylinder, sink.transform, new Vector3(0f, 0.42f, 0f), new Vector3(0.24f, 0.42f, 0.24f), porcelainMat);
            CreatePrimitive("Sink_Basin", PrimitiveType.Cube, sink.transform, new Vector3(0f, 0.88f, 0f), new Vector3(0.62f, 0.16f, 0.78f), porcelainMat);
            DecoPiece("Sink_Faucet", PrimitiveType.Cylinder, sink.transform, new Vector3(-0.18f, 1.04f, 0f), new Vector3(0.06f, 0.10f, 0.06f), metalMat);
            DecoPiece("Sink_Spout", PrimitiveType.Cube, sink.transform, new Vector3(-0.08f, 1.10f, 0f), new Vector3(0.18f, 0.04f, 0.04f), metalMat);

            // Espejo rectangular sobre el lavabo
            DecoPiece("Sink_Mirror_Frame", PrimitiveType.Cube, sink.transform, new Vector3(-0.42f, 1.62f, 0f), new Vector3(0.05f, 1.05f, 0.72f), metalMat);
            DecoPiece("Sink_Mirror_Glass", PrimitiveType.Cube, sink.transform, new Vector3(-0.39f, 1.62f, 0f), new Vector3(0.02f, 0.94f, 0.62f), mirrorMat);

            if (addDripAnomaly)
            {
                GameObject droplet = DecoPiece("Water_Droplet_Anomaly", PrimitiveType.Sphere, sink.transform,
                    new Vector3(0.0f, 1.05f, 0f), new Vector3(0.045f, 0.065f, 0.045f), waterMat);
                WashroomAnomalyController anom = sink.AddComponent<WashroomAnomalyController>();
                anom.Configure(droplet.transform);
                SerializedObject ser = new SerializedObject(anom);
                SerializedProperty pDrop = ser.FindProperty("dropletVisual");
                if (pDrop != null) pDrop.objectReferenceValue = droplet.transform;
                ser.ApplyModifiedProperties();
                EditorUtility.SetDirty(anom);
            }
        }

        private static void CreateShowerStall(string name, Transform parent, Vector3 pos, Material tileMat, Material pipeMat, Material curtainMat, Material waterMat, bool addAnomaly)
        {
            GameObject stall = new GameObject(name);
            stall.transform.parent = parent;
            stall.transform.position = pos;

            // Tabiques divisorios de azulejo
            CreatePrimitive("Shower_Partition_S", PrimitiveType.Cube, stall.transform, new Vector3(0f, 1.25f, -1.15f), new Vector3(2.2f, 2.5f, 0.12f), tileMat);
            CreatePrimitive("Shower_Partition_N", PrimitiveType.Cube, stall.transform, new Vector3(0f, 1.25f, 1.15f), new Vector3(2.2f, 2.5f, 0.12f), tileMat);

            // Tubería vertical y cabezal de ducha antiguo
            DecoPiece("Shower_Pipe", PrimitiveType.Cylinder, stall.transform, new Vector3(1.65f, 1.45f, 0f), new Vector3(0.06f, 1.25f, 0.06f), pipeMat);
            DecoPiece("Shower_Head", PrimitiveType.Cylinder, stall.transform, new Vector3(1.42f, 2.35f, 0f), new Vector3(0.22f, 0.05f, 0.22f), pipeMat).transform.localRotation = Quaternion.Euler(0f, 0f, 25f);

            // Cortina descolorida y puerta de cabina entreabierta
            DecoPiece("Shower_Curtain_Faded", PrimitiveType.Cube, stall.transform, new Vector3(-0.95f, 1.20f, -0.55f), new Vector3(0.06f, 2.1f, 0.95f), curtainMat);

            GameObject doorHinge = new GameObject("Shower_Door_Hinge");
            doorHinge.transform.parent = stall.transform;
            doorHinge.transform.localPosition = new Vector3(-1.05f, 0f, 1.05f);
            doorHinge.transform.localRotation = Quaternion.Euler(0f, 38f, 0f);
            CreatePrimitive("Shower_Cabin_Door", PrimitiveType.Cube, doorHinge.transform, new Vector3(0f, 1.15f, -0.48f), new Vector3(0.06f, 2.0f, 0.96f), pipeMat);

            WorldDoor cabinDoor = stall.AddComponent<WorldDoor>();
            cabinDoor.Configure(doorHinge.transform, -75f, 0.8f, false, false, "", name, "");
            EditorUtility.SetDirty(cabinDoor);

            if (addAnomaly)
            {
                GameObject drop = DecoPiece("Shower_Drip_Visual", PrimitiveType.Sphere, stall.transform,
                    new Vector3(1.38f, 2.25f, 0f), new Vector3(0.05f, 0.08f, 0.05f), waterMat);
                WashroomAnomalyController anom = stall.AddComponent<WashroomAnomalyController>();
                anom.Configure(drop.transform);
                SerializedObject ser = new SerializedObject(anom);
                SerializedProperty pDrop = ser.FindProperty("dropletVisual");
                if (pDrop != null) pDrop.objectReferenceValue = drop.transform;
                ser.ApplyModifiedProperties();
                EditorUtility.SetDirty(anom);
            }
        }

        // ====================================================================
        // CONSTRUCCIÓN DE LA SECCIÓN 4: AULAS DE ENSAYO (Zonas 16 a 20)
        // ====================================================================
        private static void BuildSection4RehearsalStudios(
            Transform rootEnv, float vestY,
            Material matParquet, Material matCarpet, Material matWallIvory, Material matMahogany,
            Material matGold, Material matIron, Material matClothCover, Material matVelvetDrapes,
            Material matLanternGlow, Material matColdFluorescent, Material matGreenVelvet,
            Material matLeather, Material matWoodArchive, Material matMetalArchive, Material matCardboard,
            Material matStoneWall, Material matRustedLocker, Material matMirrorGlass, Material matPorcelain,
            Material matTutuWhite, Material matTutuBlack, Material matSatinRibbon, Material matShadowFigure,
            Material matPortraitElena, Material matPhotoTroupe1923)
        {
            Material matWornWood = GetOrCreateBasicMaterial("Mat_S4_WornWoodPatch", new Color(0.26f, 0.20f, 0.15f), 0.12f, 0.0f);
            Material matSoloLightWood = GetOrCreateBasicMaterial("Mat_S4_SoloLightWood", new Color(0.56f, 0.43f, 0.29f), 0.55f, 0.02f);
            Material matChalkMark = GetOrCreateBasicMaterial("Mat_S4_ChalkMark", new Color(0.92f, 0.90f, 0.84f), 0.10f, 0.0f);
            Material matTarnishedMirror = GetOrCreateBasicMaterial("Mat_S4_TarnishedMirror", new Color(0.55f, 0.60f, 0.64f), 0.82f, 0.78f);
            Material matBurgundyCostume = GetOrCreateBasicMaterial("Mat_S4_BurgundyCostume", new Color(0.38f, 0.07f, 0.12f), 0.28f, 0.02f);

            // ====================================================================
            // ZONA 16: AULA PRINCIPAL DE BALLET (X: -26 a -4, Z: 50 a 68, Y: 2.4m, Altura: 6.4m)
            // ====================================================================
            GameObject zone16 = new GameObject("16_Main_Ballet_Studio");
            zone16.transform.parent = rootEnv;
            float studioH = 6.4f;

            CreatePrimitive("MainStudio_Floor", PrimitiveType.Cube, zone16.transform,
                new Vector3(-15f, vestY - 0.25f, 59f), new Vector3(22f, 0.5f, 18f), matParquet);
            CreatePrimitive("MainStudio_Ceiling", PrimitiveType.Cube, zone16.transform,
                new Vector3(-15f, vestY + studioH, 59f), new Vector3(22f, 0.5f, 18f), matWallIvory);

            // Vigas clásicas en el techo alto (6.4m)
            for (float bx = -22f; bx <= -8f; bx += 4.5f)
            {
                DecoPiece($"Studio_Ceiling_Beam_{bx}", PrimitiveType.Cube, zone16.transform,
                    new Vector3(bx, vestY + studioH - 0.18f, 59f), new Vector3(0.35f, 0.32f, 17.6f), matMahogany);
            }

            // Zonas del suelo donde el barniz ha desaparecido por años de ensayo
            DecoPiece("Worn_Varnish_Patch_1", PrimitiveType.Cube, zone16.transform,
                new Vector3(-15f, vestY + 0.008f, 57.5f), new Vector3(6.5f, 0.012f, 4.2f), matWornWood);
            DecoPiece("Worn_Varnish_Patch_2", PrimitiveType.Cube, zone16.transform,
                new Vector3(-19.5f, vestY + 0.008f, 62.0f), new Vector3(4.2f, 0.012f, 3.8f), matWornWood);

            // Marcas de posiciones coreográficas en el suelo (comparables con las anotaciones de la clase interrumpida)
            Vector3[] corpsPositions =
            {
                new Vector3(-19.0f, vestY + 0.015f, 60.5f),
                new Vector3(-16.5f, vestY + 0.015f, 61.8f),
                new Vector3(-13.5f, vestY + 0.015f, 61.8f),
                new Vector3(-11.0f, vestY + 0.015f, 60.5f),
                new Vector3(-15.0f, vestY + 0.015f, 58.2f) // Posición central reclamada por la suplente
            };
            for (int m = 0; m < corpsPositions.Length; m++)
            {
                CreateChoreographyFloorMark($"Marca_Coreografia_Cuerpo_{m + 1}", zone16.transform, corpsPositions[m], matChalkMark, false);
            }
            // Marca aislada de Elena desplazada junto al espejo sur
            CreateChoreographyFloorMark("Marca_Coreografia_Solista_Elena", zone16.transform, new Vector3(-11.5f, vestY + 0.015f, 52.8f), matChalkMark, true);

            // Cerramiento superior del muro este (de 5.2m a 6.4m sobre el Pasillo Principal)
            CreateWall("MainStudio_Wall_East_Upper", zone16.transform, new Vector3(-4f, vestY + 5.8f, 59f), new Vector3(0.5f, 1.2f, 18f), matWallIvory);

            // Muro sur (Z = 50): tramo oeste completo (-26 a -20) + cerramiento superior sobre Galería (-20 a -4, de 5.0m a 6.4m)
            CreateWall("MainStudio_Wall_South_WestSeg", zone16.transform, new Vector3(-23f, vestY + (studioH * 0.5f), 50f), new Vector3(6f, studioH, 0.5f), matWallIvory);
            CreateWall("MainStudio_Wall_South_Upper", zone16.transform, new Vector3(-12f, vestY + 5.7f, 50f), new Vector3(16f, 1.4f, 0.5f), matWallIvory);

            // Muro oeste (X = -26, Z: 50 a 68): puertas dobles hacia Zona 17 (Aula de Espejos) en Z = 59m (hueco Z 57..61)
            CreateWall("MainStudio_Wall_West_S", zone16.transform, new Vector3(-26f, vestY + (studioH * 0.5f), 53.5f), new Vector3(0.5f, studioH, 7.0f), matWallIvory);
            CreateWall("MainStudio_Wall_West_N", zone16.transform, new Vector3(-26f, vestY + (studioH * 0.5f), 64.5f), new Vector3(0.5f, studioH, 7.0f), matWallIvory);
            CreateArchitecturalDoubleDoor("Doors_MainStudio_To_MirrorRoom", zone16.transform, new Vector3(-26f, vestY, 59f), Quaternion.Euler(0f, 90f, 0f), 3.7f, 3.8f, studioH, 0.5f, false, "", "AULA DE ESPEJOS — CORRECCIÓN DE POSTURA", "", matMahogany, matGold, matWallIvory);

            // Muro norte (Z = 68, X: -26 a -4):
            // - Puerta en X = -19m hacia Zona 18 (Sala de Maniquíes y Vestuario Escénico)
            // - Puerta en X = -8m hacia Zona 20 (Pasillo de Servicio)
            CreateWall("MainStudio_Wall_North_W", zone16.transform, new Vector3(-23.025f, vestY + (studioH * 0.5f), 68f), new Vector3(5.95f, studioH, 0.5f), matWallIvory);
            CreateWall("MainStudio_Wall_North_Mid", zone16.transform, new Vector3(-13.50f, vestY + (studioH * 0.5f), 68f), new Vector3(8.90f, studioH, 0.5f), matWallIvory);
            CreateWall("MainStudio_Wall_North_E", zone16.transform, new Vector3(-5.475f, vestY + (studioH * 0.5f), 68f), new Vector3(2.95f, studioH, 0.5f), matWallIvory);
            CreateArchitecturalDoor("Door_MainStudio_To_Mannequins", zone16.transform, new Vector3(-19f, vestY, 68f), Quaternion.identity, 1.8f, 3.2f, studioH, 0.5f, false, "", "SALA DE MANIQUÍES Y VESTUARIO ESCÉNICO", "", matMahogany, matGold, matWallIvory);
            CreateArchitecturalDoor("Door_MainStudio_To_Service", zone16.transform, new Vector3(-8f, vestY, 68f), Quaternion.identity, 1.8f, 3.2f, studioH, 0.5f, false, "", "PASILLO DE SERVICIO", "", matMahogany, matGold, matWallIvory);

            // Grandes espejos antiguos en la pared sur (Z = 50.32m) y barras de ballet de madera con soportes metálicos
            float[] mainMirrorX = { -23.0f, -13.2f, -9.8f, -6.4f };
            for (int i = 0; i < mainMirrorX.Length; i++)
            {
                float mx = mainMirrorX[i];
                DecoPiece($"MainStudio_Mirror_Frame_{i + 1}", PrimitiveType.Cube, zone16.transform,
                    new Vector3(mx, vestY + 2.3f, 50.28f), new Vector3(2.9f, 3.8f, 0.08f), matMahogany);
                DecoPiece($"MainStudio_Mirror_GoldTrim_{i + 1}", PrimitiveType.Cube, zone16.transform,
                    new Vector3(mx, vestY + 2.3f, 50.31f), new Vector3(2.74f, 3.64f, 0.05f), matGold);
                CreatePrimitive($"MainStudio_Mirror_Glass_{i + 1}", PrimitiveType.Cube, zone16.transform,
                    new Vector3(mx, vestY + 2.3f, 50.34f), new Vector3(2.62f, 3.52f, 0.02f), (i == 1) ? matTarnishedMirror : matMirrorGlass);
            }

            // Barras de ballet frente a los espejos y en el lateral norte
            CreateBalletBarre("Barra_Ballet_Sur_Principal", zone16.transform, new Vector3(-9.8f, vestY, 51.05f), Quaternion.identity, 8.8f, matMahogany, matIron);
            CreateBalletBarre("Barra_Ballet_Sur_Oeste", zone16.transform, new Vector3(-23.0f, vestY, 51.05f), Quaternion.identity, 4.4f, matMahogany, matIron);
            CreateBalletBarre("Barra_Ballet_Norte_Central", zone16.transform, new Vector3(-13.5f, vestY, 67.2f), Quaternion.identity, 6.8f, matMahogany, matIron);

            // Ventanales altos con luz lunar azulada y cortinas borgoña parcialmente descolgadas
            float[] windowX = { -23.0f, -15.5f, -11.5f };
            for (int w = 0; w < windowX.Length; w++)
            {
                float wx = windowX[w];
                DecoPiece($"High_MoonWindow_Frame_{w + 1}", PrimitiveType.Cube, zone16.transform,
                    new Vector3(wx, vestY + 4.85f, 67.70f), new Vector3(2.8f, 2.1f, 0.14f), matMahogany);
                DecoPiece($"High_MoonWindow_Glass_{w + 1}", PrimitiveType.Cube, zone16.transform,
                    new Vector3(wx, vestY + 4.85f, 67.74f), new Vector3(2.5f, 1.85f, 0.06f), matColdFluorescent);

                // Cortinas borgoña: una caída en diagonal para sugerir abandono repentino
                GameObject curtainL = DecoPiece($"MoonWindow_Curtain_L_{w + 1}", PrimitiveType.Cube, zone16.transform,
                    new Vector3(wx - 1.25f, vestY + 4.2f, 67.58f), new Vector3(0.42f, 3.1f, 0.16f), matVelvetDrapes);
                if (w == 1) curtainL.transform.localRotation = Quaternion.Euler(0f, 0f, -14f);

                DecoPiece($"MoonWindow_Curtain_R_{w + 1}", PrimitiveType.Cube, zone16.transform,
                    new Vector3(wx + 1.25f, vestY + 4.35f, 67.58f), new Vector3(0.42f, 2.9f, 0.16f), matVelvetDrapes);

                CreatePictureSpotlight($"Moonlight_MainStudio_{w + 1}", zone16.transform,
                    new Vector3(wx, vestY + 5.1f, 66.8f), new Vector3(wx, vestY + 0.2f, 56.5f), new Color(0.56f, 0.74f, 0.98f), 2.8f);
            }

            // Piano vertical antiguo en el extremo este de la sala, con taburete, sillas y mesa de partituras
            CreateUprightPiano("Piano_Vertical_Ensayo", zone16.transform, new Vector3(-5.2f, vestY, 62.5f), Quaternion.Euler(0f, -90f, 0f), matMahogany, matGold, matPorcelain, matIron);

            // Mesa auxiliar con partituras y sillas de observación junto al piano
            CreatePrimitive("Mesa_Partituras_Ensayo", PrimitiveType.Cube, zone16.transform,
                new Vector3(-6.2f, vestY + 0.42f, 65.5f), new Vector3(1.4f, 0.84f, 0.9f), matMahogany);
            CreatePrimitive("Silla_Maestra_1", PrimitiveType.Cube, zone16.transform,
                new Vector3(-5.2f, vestY + 0.52f, 59.2f), new Vector3(0.75f, 1.04f, 0.75f), matMahogany);
            CreatePrimitive("Silla_Maestra_2", PrimitiveType.Cube, zone16.transform,
                new Vector3(-5.2f, vestY + 0.52f, 53.2f), new Vector3(0.75f, 1.04f, 0.75f), matMahogany);

            // Carteles antiguos de posiciones de ballet, fotografías de clases y reloj detenido
            CreateFramedArtPortrait("Cartel_Aula_Posiciones_1", zone16.transform,
                new Vector3(-4.32f, vestY + 2.8f, 60.0f), new Vector3(0.10f, 2.0f, 1.5f), matGold, matPhotoTroupe1923,
                "Lámina Académica: Arabesque y Attitude (Escuela Imperial, 1910)");
            CreateFramedArtPortrait("Foto_Clase_Antigua_1922", zone16.transform,
                new Vector3(-4.32f, vestY + 2.8f, 52.8f), new Vector3(0.10f, 1.8f, 2.2f), matGold, matPhotoTroupe1923,
                "Fotografía de Clase Colectiva — Cuerpo de Baile (1922)");
            CreateWallClock("Reloj_Detenido_Aula_Principal", zone16.transform, new Vector3(-13.5f, vestY + 4.3f, 67.68f), matMahogany, matGold);

            // Lámparas cálidas tenues de techo en el aula principal
            CreateChandelier("Arana_Aula_Principal_Oeste", zone16.transform, new Vector3(-20f, vestY + 5.6f, 59f), matGold, new Color(1f, 0.82f, 0.52f), 2.4f);
            CreateChandelier("Arana_Aula_Principal_Este", zone16.transform, new Vector3(-10f, vestY + 5.6f, 59f), matGold, new Color(1f, 0.82f, 0.52f), 2.4f);

            // ANOMALÍA PSICOLÓGICA ZONA 16: Figura en posición de ballet reflejada al fondo cuando el jugador se aleja de los espejos
            GameObject studioMirrorAnomalyHost = new GameObject("Anomaly_MainStudio_MirrorPose");
            studioMirrorAnomalyHost.transform.parent = zone16.transform;
            studioMirrorAnomalyHost.transform.position = new Vector3(-11.5f, vestY, 50.45f);

            GameObject balletPoseSilhouette = CreateBalletSilhouetteFigure("Silhouette_BalletPose_MainStudio", studioMirrorAnomalyHost.transform,
                new Vector3(0f, 0.05f, -0.06f), matShadowFigure, true);
            Light mirrorGlintLight = CreateSubtleAnomalyLight("MainStudio_Mirror_Glint", studioMirrorAnomalyHost.transform,
                new Vector3(0f, 2.1f, 0.5f), new Color(0.62f, 0.78f, 0.98f), 1.6f);

            RehearsalAnomalyController anom16 = studioMirrorAnomalyHost.AddComponent<RehearsalAnomalyController>();
            anom16.Configure(RehearsalAnomalyType.MainStudioMirrorPose, balletPoseSilhouette, null, null, null, mirrorGlintLight);
            EditorUtility.SetDirty(anom16);

            // ELEMENTOS NARRATIVOS DE LA ZONA 16 (AULA PRINCIPAL DE BALLET)
            CreateInteractiveWorldDoc("Doc_Anotaciones_Clase_Interrumpida", zone16.transform,
                new Vector3(-6.2f, vestY + 0.88f, 65.5f), new Vector3(0.42f, 0.04f, 0.32f), matGold,
                "doc_s4_interrupted_class_notes",
                "Anotaciones de una Clase Interrumpida — Cuaderno de Coreografía",
                "DIAGRAMA DE POSICIONES — ACTO FINAL («EL LAGO DE LAS SOMBRAS»)\n\n" +
                "En la página abierta hay un croquis de las marcas de tiza trazadas sobre el suelo del aula:\n" +
                "• Las cuatro cruces en semicírculo corresponden al cuerpo de baile.\n" +
                "• El círculo central pertenece a la Primera Bailarina (Elena Vesper).\n\n" +
                "Sin embargo, al comparar el dibujo con las marcas reales en la madera del suelo, alguien borró a medias la marca central y trazó una nueva cruz aislada a menos de dos metros del espejo sur.\n\n" +
                "Nota al margen del maestro de baile:\n" +
                "“17 de octubre, 20:10 h — Se suspende el ensayo inmediatamente. Elena asegura que la figura reflejada en el espejo central no repite los pasos del resto de la clase.”");

            CreateInteractiveWorldDoc("Doc_Lista_Asistencia_Incompleta", zone16.transform,
                new Vector3(-5.2f, vestY + 1.08f, 59.2f), new Vector3(0.36f, 0.04f, 0.28f), matGold,
                "doc_s4_incomplete_attendance",
                "Lista de Asistencia Incompleta — Ensayo Nocturno",
                "CONTROL DE ASISTENCIA OBLIGATORIA — SALA PRINCIPAL\n\n" +
                "1. Sofía Lindqvist — Presente\n" +
                "2. Clara Valmont — Presente\n" +
                "3. Mara Bell — Presente (Asignada al puesto central por orden de Dirección)\n" +
                "4. Elena Vesper — [Casilla en blanco, tachada con trazo tembloroso]\n\n" +
                "Observaciones:\n" +
                "“Aunque Elena no firmó la hoja de entrada a las 19:00 h, sus zapatillas de ensayo y su partitura anotada ya estaban colocadas frente al espejo antes de que abriéramos el aula.”");

            CreateInteractiveWorldDoc("Doc_Foto_Ultimo_Ensayo", zone16.transform,
                new Vector3(-5.2f, vestY + 1.48f, 62.5f), new Vector3(0.38f, 0.04f, 0.30f), matSatinRibbon,
                "doc_s4_last_rehearsal_photo",
                "Fotografía de las Alumnas durante el Último Ensayo",
                "Una fotografía en blanco y negro tomada desde la esquina del piano vertical pocos días antes del cierre.\n\n" +
                "Las alumnas mantienen la quinta posición en el centro del aula mirando hacia los espejos del sur.\n" +
                "Al observar el reflejo capturado en el cristal del fondo, todas las bailarinas aparecen de frente salvo una figura en el extremo izquierdo cuyo reflejo permanece de espaldas, mirando fijamente hacia el interior del espejo.");

            // ====================================================================
            // ZONA 17: AULA DE ESPEJOS (X: -38 a -26, Z: 50 a 68, Y: 2.4m, Altura: 6.0m)
            // ====================================================================
            GameObject zone17 = new GameObject("17_Mirror_Posture_Studio");
            zone17.transform.parent = rootEnv;
            float mirrorRoomH = 6.0f;

            CreatePrimitive("MirrorRoom_Floor", PrimitiveType.Cube, zone17.transform,
                new Vector3(-32f, vestY - 0.25f, 59f), new Vector3(12f, 0.5f, 18f), matParquet);
            CreatePrimitive("MirrorRoom_Ceiling", PrimitiveType.Cube, zone17.transform,
                new Vector3(-32f, vestY + mirrorRoomH, 59f), new Vector3(12f, 0.5f, 18f), matWallIvory);

            // Muros sur (Z = 50) y oeste (X = -38)
            CreateWall("MirrorRoom_Wall_South", zone17.transform, new Vector3(-32f, vestY + (mirrorRoomH * 0.5f), 50f), new Vector3(12f, mirrorRoomH, 0.5f), matWallIvory);
            CreateWall("MirrorRoom_Wall_West", zone17.transform, new Vector3(-38f, vestY + (mirrorRoomH * 0.5f), 59f), new Vector3(0.5f, mirrorRoomH, 18f), matWallIvory);

            // Muro norte (Z = 68): puerta reconocible hacia Zona 19 (Sala de Entrenamiento Individual) en X = -32m (hueco X -33.05..-30.95)
            CreateWall("MirrorRoom_Wall_North_W", zone17.transform, new Vector3(-35.525f, vestY + (mirrorRoomH * 0.5f), 68f), new Vector3(4.95f, mirrorRoomH, 0.5f), matWallIvory);
            CreateWall("MirrorRoom_Wall_North_E", zone17.transform, new Vector3(-28.475f, vestY + (mirrorRoomH * 0.5f), 68f), new Vector3(4.95f, mirrorRoomH, 0.5f), matWallIvory);
            CreateArchitecturalDoor("Door_MirrorRoom_To_SoloRoom", zone17.transform, new Vector3(-32f, vestY, 68f), Quaternion.identity, 1.8f, 3.2f, mirrorRoomH, 0.5f, false, "", "SALA DE ENTRENAMIENTO INDIVIDUAL", "", matMahogany, matGold, matWallIvory);

            // Grandes paneles de espejos verticales en las paredes oeste y este, separados por columnas ornamentadas y marcos metálicos
            for (int p = 0; p < 5; p++)
            {
                float pz = 52.2f + (p * 3.4f);

                // Columna clásica separadora en el muro oeste
                CreateClassicalColumn($"MirrorRoom_Col_W_{p + 1}", zone17.transform, new Vector3(-37.55f, vestY, pz - 1.7f), mirrorRoomH - 0.2f, 0.28f, matWallIvory);

                // Panel de espejo vertical en el muro oeste (alternando espejos limpios y espejos con manchas/bordes oscurecidos)
                DecoPiece($"MirrorPanel_Frame_W_{p + 1}", PrimitiveType.Cube, zone17.transform,
                    new Vector3(-37.68f, vestY + 2.4f, pz), new Vector3(0.08f, 4.2f, 2.5f), matIron);
                CreatePrimitive($"MirrorPanel_Glass_W_{p + 1}", PrimitiveType.Cube, zone17.transform,
                    new Vector3(-37.63f, vestY + 2.4f, pz), new Vector3(0.02f, 4.0f, 2.3f), (p % 2 == 1) ? matTarnishedMirror : matMirrorGlass);

                // Focos de pared en hilera sobre cada panel para dar claridad sin confundir las puertas
                CreateOrnateSconce($"Sconce_MirrorRoom_W_{p + 1}", zone17.transform,
                    new Vector3(-37.55f, vestY + 4.75f, pz), matGold, matLanternGlow, new Color(0.95f, 0.84f, 0.62f), 2.0f);
            }

            // Paneles de espejo en el muro este (a ambos lados de la puerta doble hacia el Aula Principal)
            float[] eastPanelZ = { 53.2f, 64.8f };
            for (int ep = 0; ep < eastPanelZ.Length; ep++)
            {
                float ez = eastPanelZ[ep];
                DecoPiece($"MirrorPanel_Frame_E_{ep + 1}", PrimitiveType.Cube, zone17.transform,
                    new Vector3(-26.32f, vestY + 2.4f, ez), new Vector3(0.08f, 4.2f, 3.8f), matIron);
                CreatePrimitive($"MirrorPanel_Glass_E_{ep + 1}", PrimitiveType.Cube, zone17.transform,
                    new Vector3(-26.37f, vestY + 2.4f, ez), new Vector3(0.02f, 4.0f, 3.6f), matMirrorGlass);
            }

            // Barras de ballet laterales y pequeñas plataformas de práctica (tarimas de corrección de postura)
            CreateBalletBarre("Barra_AulaEspejos_Sur", zone17.transform, new Vector3(-32f, vestY, 51.0f), Quaternion.identity, 7.5f, matMahogany, matIron);
            CreateBalletBarre("Barra_AulaEspejos_Este_Norte", zone17.transform, new Vector3(-26.95f, vestY, 64.8f), Quaternion.Euler(0f, 90f, 0f), 3.6f, matMahogany, matIron);

            // Plataformas de práctica de madera con reborde de latón
            CreatePrimitive("Plataforma_Practica_1", PrimitiveType.Cube, zone17.transform,
                new Vector3(-34.2f, vestY + 0.12f, 55.6f), new Vector3(2.4f, 0.24f, 2.4f), matMahogany);
            DecoPiece("Plataforma_Practica_1_Trim", PrimitiveType.Cube, zone17.transform,
                new Vector3(-34.2f, vestY + 0.245f, 55.6f), new Vector3(2.44f, 0.02f, 2.44f), matGold);

            CreatePrimitive("Plataforma_Practica_2", PrimitiveType.Cube, zone17.transform,
                new Vector3(-34.2f, vestY + 0.12f, 62.4f), new Vector3(2.4f, 0.24f, 2.4f), matMahogany);
            DecoPiece("Plataforma_Practica_2_Trim", PrimitiveType.Cube, zone17.transform,
                new Vector3(-34.2f, vestY + 0.245f, 62.4f), new Vector3(2.44f, 0.02f, 2.44f), matGold);

            // Marcas de tiza sobre el suelo indicando ángulos de corrección y una línea oblicua hacia el Panel Nº 3 (Z = 59.0m)
            CreateChoreographyFloorMark("Marca_Tiza_Espejos_1", zone17.transform, new Vector3(-34.2f, vestY + 0.25f, 55.6f), matChalkMark, false);
            CreateChoreographyFloorMark("Marca_Tiza_Espejos_2", zone17.transform, new Vector3(-34.2f, vestY + 0.25f, 62.4f), matChalkMark, true);
            DecoPiece("Marca_Tiza_Angulo_Oblicuo", PrimitiveType.Cube, zone17.transform,
                new Vector3(-35.6f, vestY + 0.015f, 59.0f), new Vector3(2.2f, 0.01f, 0.06f), matChalkMark).transform.localRotation = Quaternion.Euler(0f, -22f, 0f);

            // Objeto real frente al Panel 3 (taburete con metrónomo) y su reflejo desplazado varios centímetros
            CreatePrimitive("Taburete_Real_Espejo3", PrimitiveType.Cylinder, zone17.transform,
                new Vector3(-36.7f, vestY + 0.35f, 58.2f), new Vector3(0.45f, 0.35f, 0.45f), matMahogany);
            DecoPiece("Metronomo_Real_Espejo3", PrimitiveType.Cube, zone17.transform,
                new Vector3(-36.7f, vestY + 0.82f, 58.2f), new Vector3(0.18f, 0.26f, 0.18f), matGold);

            // ANOMALÍA PSICOLÓGICA ZONA 17: Figura en un único panel de espejo (Panel 3, Z = 59m) mientras los demás están vacíos + objeto reflejado desplazado
            GameObject mirrorPanel3Host = new GameObject("Anomaly_MirrorRoom_Panel3");
            mirrorPanel3Host.transform.parent = zone17.transform;
            mirrorPanel3Host.transform.position = new Vector3(-37.55f, vestY, 59.0f);

            GameObject singlePanelFigure = CreateBalletSilhouetteFigure("Silhouette_SinglePanel3_Figure", mirrorPanel3Host.transform,
                new Vector3(-0.04f, 0.08f, 0f), matShadowFigure, false);
            singlePanelFigure.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            GameObject displacedReflectedMetronome = DecoPiece("Metronomo_Reflejo_Desplazado", PrimitiveType.Cube, mirrorPanel3Host.transform,
                new Vector3(-0.03f, 0.82f, -0.45f), new Vector3(0.18f, 0.26f, 0.18f), matGold);
            Light panel3PulseLight = CreateSubtleAnomalyLight("Panel3_Anomaly_Light", mirrorPanel3Host.transform,
                new Vector3(0.45f, 2.2f, 0f), new Color(0.70f, 0.85f, 1.0f), 1.8f);

            RehearsalAnomalyController anom17 = mirrorPanel3Host.AddComponent<RehearsalAnomalyController>();
            anom17.Configure(RehearsalAnomalyType.MirrorRoomSinglePanelFigure,
                singlePanelFigure, null, null, displacedReflectedMetronome.transform, panel3PulseLight);
            EditorUtility.SetDirty(anom17);

            // ELEMENTOS NARRATIVOS DE LA ZONA 17 (AULA DE ESPEJOS)
            CreateInteractiveWorldDoc("Doc_Secuencia_Movimientos_Espejos", zone17.transform,
                new Vector3(-34.2f, vestY + 0.28f, 55.6f), new Vector3(0.38f, 0.04f, 0.28f), matGold,
                "doc_s4_mirror_sequence_notes",
                "Anotaciones sobre Secuencia de Movimientos y Reflejos",
                "CUADERNO DE CORRECCIÓN POSTURAL — SALA DE ESPEJOS\n\n" +
                "«Secuencia para el Acto III: Treinta y dos giros frente al tercer panel vertical.\n" +
                "La mirada debe fijarse en un punto exacto a la altura de los ojos para no perder el eje.»\n\n" +
                "Debajo, escrito con caligrafía apresurada por una alumna:\n" +
                "“No puedo practicar en esta sala sola. Cuando te detienes en seco tras el giro número doce, el reflejo del tercer panel tarda medio segundo más en frenar. Si miras desde la marca de tiza oblicua, los objetos junto al cristal tampoco coinciden con su posición real.”");

            CreateInteractiveWorldDoc("Doc_Fragmento_Ultima_Representacion", zone17.transform,
                new Vector3(-36.2f, vestY + 0.04f, 59.0f), new Vector3(0.30f, 0.03f, 0.24f), matSatinRibbon,
                "doc_s4_last_performance_fragment",
                "Fragmento de Papel — Referencia a la Última Representación",
                "Un recorte arrancado de las instrucciones escénicas para la noche del estreno de «El Lago de las Sombras»:\n\n" +
                "“...la Directora Vale ha ordenado que el vestuario original de Elena sea trasladado a la sala contigua de maniquíes antes del tercer acto.\n" +
                "Oficialmente se dirá que Elena abandonó la academia por voluntad propia antes del ensayo general, pero su tutú de escena aún conserva las agujas clavadas del ajuste de esta tarde.”");

            // ====================================================================
            // ZONA 18: SALA DE MANIQUÍES Y VESTUARIO ESCÉNICO (X: -26 a -12, Z: 68 a 82, Y: 2.4m, Altura: 5.4m)
            // ====================================================================
            GameObject zone18 = new GameObject("18_Mannequin_And_Stage_Costumes");
            zone18.transform.parent = rootEnv;
            float costumeRoomH = 5.4f;

            CreatePrimitive("MannequinRoom_Floor", PrimitiveType.Cube, zone18.transform,
                new Vector3(-19f, vestY - 0.25f, 75f), new Vector3(14f, 0.5f, 14f), matParquet);
            CreatePrimitive("MannequinRoom_Ceiling", PrimitiveType.Cube, zone18.transform,
                new Vector3(-19f, vestY + costumeRoomH, 75f), new Vector3(14f, 0.5f, 14f), matWallIvory);

            // Muro norte completo (Z = 82, X: -26 a -12)
            CreateWall("MannequinRoom_Wall_North", zone18.transform, new Vector3(-19f, vestY + (costumeRoomH * 0.5f), 82f), new Vector3(14f, costumeRoomH, 0.5f), matWallIvory);

            // Muro oeste (X = -26, Z: 68 a 82): puerta en Z = 74m hacia Zona 19 (Sala de Entrenamiento Individual)
            CreateWall("MannequinRoom_Wall_West_S", zone18.transform, new Vector3(-26f, vestY + (costumeRoomH * 0.5f), 70.475f), new Vector3(0.5f, costumeRoomH, 4.95f), matWallIvory);
            CreateWall("MannequinRoom_Wall_West_N", zone18.transform, new Vector3(-26f, vestY + (costumeRoomH * 0.5f), 78.525f), new Vector3(0.5f, costumeRoomH, 6.95f), matWallIvory);
            CreateArchitecturalDoor("Door_SoloRoom_To_Mannequins", zone18.transform, new Vector3(-26f, vestY, 74f), Quaternion.Euler(0f, 90f, 0f), 1.8f, 3.2f, costumeRoomH, 0.5f, false, "", "SALA DE ENTRENAMIENTO INDIVIDUAL", "", matMahogany, matGold, matWallIvory);

            // Muro este (X = -12, Z: 68 a 82): puerta en Z = 75m hacia Zona 20 (Pasillo de Servicio)
            CreateWall("MannequinRoom_Wall_East_S", zone18.transform, new Vector3(-12f, vestY + (costumeRoomH * 0.5f), 70.975f), new Vector3(0.5f, costumeRoomH, 5.95f), matWallIvory);
            CreateWall("MannequinRoom_Wall_East_N", zone18.transform, new Vector3(-12f, vestY + (costumeRoomH * 0.5f), 79.025f), new Vector3(0.5f, costumeRoomH, 5.95f), matWallIvory);
            CreateArchitecturalDoor("Door_Mannequins_To_Service", zone18.transform, new Vector3(-12f, vestY, 75f), Quaternion.Euler(0f, 90f, 0f), 1.8f, 3.2f, costumeRoomH, 0.5f, false, "", "PASILLO DE SERVICIO", "", matMahogany, matGold, matWallIvory);

            // Mesa de costura central con cintas, agujas, carretes y bocetos de vestuario
            CreatePrimitive("Mesa_Costura_Central", PrimitiveType.Cube, zone18.transform,
                new Vector3(-19f, vestY + 0.45f, 75f), new Vector3(3.2f, 0.90f, 1.6f), matMahogany);
            DecoPiece("Cinta_Costura_1", PrimitiveType.Cube, zone18.transform,
                new Vector3(-18.2f, vestY + 0.92f, 74.7f), new Vector3(0.85f, 0.015f, 0.07f), matSatinRibbon);
            DecoPiece("Carrete_Hilo_1", PrimitiveType.Cylinder, zone18.transform,
                new Vector3(-19.8f, vestY + 0.96f, 75.3f), new Vector3(0.10f, 0.06f, 0.10f), matGold);
            DecoPiece("Alfiletero_Terciopelo", PrimitiveType.Sphere, zone18.transform,
                new Vector3(-19.5f, vestY + 0.95f, 75.1f), new Vector3(0.16f, 0.10f, 0.16f), matBurgundyCostume);

            // Lámpara de trabajo colgante que ilumina la mesa central y deja las esquinas en penumbra inquietante
            DecoPiece("Cable_Lampara_Costura", PrimitiveType.Cylinder, zone18.transform,
                new Vector3(-19f, vestY + 4.4f, 75f), new Vector3(0.03f, 0.95f, 0.03f), matIron);
            DecoPiece("Pantalla_Lampara_Costura", PrimitiveType.Cylinder, zone18.transform,
                new Vector3(-19f, vestY + 3.4f, 75f), new Vector3(0.65f, 0.12f, 0.65f), matIron);
            Light sewingWorkLight = CreateSubtleAnomalyLight("Luz_Trabajo_Costura", zone18.transform,
                new Vector3(-19f, vestY + 3.25f, 75f), new Color(1f, 0.80f, 0.48f), 2.6f);
            sewingWorkLight.gameObject.SetActive(true);
            sewingWorkLight.range = 10.5f;

            // Maniquíes de cuerpo entero en distintas posturas de ballet (en tonos borgoña, marfil y negro, y cubiertos con telas claras)
            CreateStageMannequin("Maniqui_1_CisneBlanco", zone18.transform, new Vector3(-16.2f, vestY, 72.0f), Quaternion.Euler(0f, -35f, 0f), matWoodArchive, matIron, matTutuWhite, false);
            CreateStageMannequin("Maniqui_2_Borgoña", zone18.transform, new Vector3(-22.2f, vestY, 71.5f), Quaternion.Euler(0f, 40f, 0f), matWoodArchive, matIron, matBurgundyCostume, false);
            CreateStageMannequin("Maniqui_3_CubiertoTela", zone18.transform, new Vector3(-15.0f, vestY, 78.8f), Quaternion.Euler(0f, -140f, 0f), matWoodArchive, matIron, matClothCover, true);
            CreateStageMannequin("Maniqui_4_CisneNegro", zone18.transform, new Vector3(-17.2f, vestY, 78.2f), Quaternion.Euler(0f, 165f, 0f), matWoodArchive, matIron, matTutuBlack, false);

            // Maniquí Nº 5 (el que gira sutilmente cuando el jugador aparta la vista)
            GameObject turningMannequin = CreateStageMannequin("Maniqui_5_AnomaliaGiro", zone18.transform,
                new Vector3(-21.5f, vestY, 77.2f), Quaternion.Euler(0f, 15f, 0f), matWoodArchive, matIron, matTutuBlack, false);

            // Biombo alto en la esquina noroeste ocultando parcialmente el rincón en penumbra
            GameObject screenRoot = new GameObject("Biombo_Alto_Esquina_NO");
            screenRoot.transform.parent = zone18.transform;
            screenRoot.transform.position = new Vector3(-23.2f, vestY, 79.4f);
            CreatePrimitive("Biombo_Panel_1", PrimitiveType.Cube, screenRoot.transform, new Vector3(-0.75f, 1.35f, 0.25f), new Vector3(0.95f, 2.7f, 0.08f), matMahogany).transform.localRotation = Quaternion.Euler(0f, 28f, 0f);
            CreatePrimitive("Biombo_Panel_2", PrimitiveType.Cube, screenRoot.transform, new Vector3(0.0f, 1.35f, 0.0f), new Vector3(0.95f, 2.7f, 0.08f), matVelvetDrapes).transform.localRotation = Quaternion.Euler(0f, -15f, 0f);
            CreatePrimitive("Biombo_Panel_3", PrimitiveType.Cube, screenRoot.transform, new Vector3(0.75f, 1.35f, -0.25f), new Vector3(0.95f, 2.7f, 0.08f), matMahogany).transform.localRotation = Quaternion.Euler(0f, 22f, 0f);

            // Silueta cubierta por una tela clara junto al biombo que parece respirar levemente
            GameObject breathingClothFigure = CreateStageMannequin("Maniqui_6_TelaRespirando", zone18.transform,
                new Vector3(-24.4f, vestY, 80.5f), Quaternion.Euler(0f, 135f, 0f), matWoodArchive, matIron, matClothCover, true);

            // Percheros con trajes, cajas de sombrerería/accesorios y máscaras teatrales
            CreateGarmentRackWithTutus("Perchero_Escenico_1", zone18.transform, new Vector3(-13.6f, vestY, 71.2f), matIron, matGold, matBurgundyCostume, matTutuBlack, matClothCover);
            CreatePrimitive("Caja_Accesorios_1", PrimitiveType.Cube, zone18.transform, new Vector3(-13.8f, vestY + 0.35f, 80.4f), new Vector3(1.1f, 0.7f, 0.85f), matCardboard);
            CreatePrimitive("Caja_Accesorios_2", PrimitiveType.Cube, zone18.transform, new Vector3(-15.1f, vestY + 0.25f, 80.6f), new Vector3(0.85f, 0.5f, 0.75f), matCardboard);
            DecoPiece("Mascara_Teatral_1", PrimitiveType.Sphere, zone18.transform, new Vector3(-13.8f, vestY + 0.82f, 80.4f), new Vector3(0.22f, 0.28f, 0.14f), matPorcelain);

            // ANOMALÍA PSICOLÓGICA ZONA 18: Giro del maniquí al apartar la vista + tela que respira levemente
            RehearsalAnomalyController anom18 = turningMannequin.AddComponent<RehearsalAnomalyController>();
            anom18.Configure(RehearsalAnomalyType.MannequinTurnAndClothBreath,
                null, turningMannequin.transform, breathingClothFigure.transform, null, null);
            EditorUtility.SetDirty(anom18);

            // ELEMENTOS NARRATIVOS DE LA ZONA 18 (SALA DE MANIQUÍES Y VESTUARIO ESCÉNICO)
            CreateInteractiveWorldDoc("Doc_Etiquetas_Vestuario_Escenico", zone18.transform,
                new Vector3(-16.2f, vestY + 1.25f, 72.0f), new Vector3(0.28f, 0.14f, 0.18f), matGold,
                "doc_s4_costume_name_tags",
                "Etiquetas de Sastrería Escénica — Solistas",
                "REGISTRO DE VESTUARIO DE REPRESENTACIÓN — TEMPORADA 1924\n\n" +
                "• Maniquí I (Marfil y Oro): «Odette — Cisne Blanco. Asignado a: Elena Vesper».\n" +
                "• Maniquí II (Terciopelo Borgoña): «Dama de la Corte — Acto I. Asignado a: Clara Valmont».\n" +
                "• Maniquí V (Tul Negro y Azabache): «Odile — Cisne Negro».\n\n" +
                "Nota de la jefa de sastrería:\n" +
                "“No entiendo por qué los maniquíes de ensayo aparecen movidos cada mañana hacia la puerta del taller. He pedido a las alumnas que no entren sin luz.”");

            CreateInteractiveWorldDoc("Doc_Bocetos_Trajes_Representacion", zone18.transform,
                new Vector3(-19.0f, vestY + 0.93f, 75.0f), new Vector3(0.44f, 0.04f, 0.34f), matGold,
                "doc_s4_costume_sketches",
                "Bocetos de los Trajes para la Representación Final",
                "Láminas de carboncillo y acuarela sobre la mesa de costura.\n\n" +
                "El diseño del Cisne Negro muestra un corpiño con trece broches de plata en la espalda que requería la ayuda de otra persona para abrocharse o quitarse.\n\n" +
                "Anotación escrita con lápiz rojo:\n" +
                "“Imposible que una bailarina se desvista sola de este traje en menos de diez minutos. Quienquiera que dejó el tutú negro escondido tras el biombo tuvo que contar con ayuda dentro del taller.”");

            CreateInteractiveWorldDoc("Doc_Prenda_Contradictoria", zone18.transform,
                new Vector3(-24.1f, vestY + 0.42f, 79.8f), new Vector3(0.40f, 0.12f, 0.32f), matSatinRibbon,
                "doc_s4_contradictory_garment",
                "Corpiño Escénico Oculto tras el Biombo (Contradicción de Registros)",
                "Oculto detrás del biombo de esquina se encuentra el corpiño original del Cisne Negro confeccionado a la medida exacta de Elena Vesper.\n\n" +
                "La etiqueta interior con el nombre «ELENA VESPER» fue arrancada a tirones y sobre ella se cosió con hilo negro una etiqueta nueva: «MARA BELL — ESTRENO 15 DE OCTUBRE».\n\n" +
                "¡Esto contradice directamente el registro oficial de la Directora Vale, que aseguraba que el cambio de reparto solo se decidió tras la desaparición de Elena el 17 de octubre! El reemplazo ya estaba preparado días antes.");

            // ====================================================================
            // ZONA 19: SALA DE ENTRENAMIENTO INDIVIDUAL (X: -38 a -26, Z: 68 a 80, Y: 2.4m, Altura: 5.4m)
            // ====================================================================
            GameObject zone19 = new GameObject("19_Solo_Training_Room");
            zone19.transform.parent = rootEnv;
            float soloRoomH = 5.4f;

            CreatePrimitive("SoloRoom_Floor", PrimitiveType.Cube, zone19.transform,
                new Vector3(-32f, vestY - 0.25f, 74f), new Vector3(12f, 0.5f, 12f), matSoloLightWood);
            CreatePrimitive("SoloRoom_Ceiling", PrimitiveType.Cube, zone19.transform,
                new Vector3(-32f, vestY + soloRoomH, 74f), new Vector3(12f, 0.5f, 12f), matWallIvory);

            // Muros norte (Z = 80) y oeste (X = -38)
            CreateWall("SoloRoom_Wall_North", zone19.transform, new Vector3(-32f, vestY + (soloRoomH * 0.5f), 80f), new Vector3(12f, soloRoomH, 0.5f), matWallIvory);
            CreateWall("SoloRoom_Wall_West", zone19.transform, new Vector3(-38f, vestY + (soloRoomH * 0.5f), 74f), new Vector3(0.5f, soloRoomH, 12f), matWallIvory);

            // Ventana alta en el muro oeste cubierta por cortinas pesadas borgoña
            DecoPiece("SoloRoom_HighWindow_Frame", PrimitiveType.Cube, zone19.transform,
                new Vector3(-37.70f, vestY + 3.8f, 74f), new Vector3(0.12f, 1.8f, 3.2f), matMahogany);
            DecoPiece("SoloRoom_HeavyCurtain_L", PrimitiveType.Cube, zone19.transform,
                new Vector3(-37.58f, vestY + 3.1f, 73.1f), new Vector3(0.24f, 3.4f, 1.55f), matVelvetDrapes);
            DecoPiece("SoloRoom_HeavyCurtain_R", PrimitiveType.Cube, zone19.transform,
                new Vector3(-37.58f, vestY + 3.1f, 74.9f), new Vector3(0.24f, 3.4f, 1.55f), matVelvetDrapes);

            // Barra de ballet individual y espejo de cuerpo entero en el muro norte
            CreateBalletBarre("Barra_Entrenamiento_Individual", zone19.transform, new Vector3(-34.2f, vestY, 78.9f), Quaternion.identity, 4.6f, matMahogany, matIron);

            DecoPiece("SoloMirror_Frame", PrimitiveType.Cube, zone19.transform,
                new Vector3(-29.5f, vestY + 2.0f, 79.68f), new Vector3(2.2f, 3.4f, 0.08f), matMahogany);
            DecoPiece("SoloMirror_GoldTrim", PrimitiveType.Cube, zone19.transform,
                new Vector3(-29.5f, vestY + 2.0f, 79.65f), new Vector3(2.04f, 3.24f, 0.05f), matGold);
            CreatePrimitive("SoloMirror_Glass", PrimitiveType.Cube, zone19.transform,
                new Vector3(-29.5f, vestY + 2.0f, 79.62f), new Vector3(1.92f, 3.12f, 0.02f), matMirrorGlass);

            // En una esquina: silla, pequeña mesa auxiliar, botella de agua vacía y toalla antigua
            CreatePrimitive("SoloRoom_Chair", PrimitiveType.Cube, zone19.transform,
                new Vector3(-36.4f, vestY + 0.50f, 70.2f), new Vector3(0.75f, 1.0f, 0.75f), matMahogany);
            CreatePrimitive("SoloRoom_SideTable", PrimitiveType.Cube, zone19.transform,
                new Vector3(-36.4f, vestY + 0.40f, 71.6f), new Vector3(0.95f, 0.80f, 0.85f), matMahogany);
            DecoPiece("Botella_Agua_Vacia", PrimitiveType.Cylinder, zone19.transform,
                new Vector3(-36.25f, vestY + 0.94f, 71.45f), new Vector3(0.10f, 0.14f, 0.10f), matColdFluorescent);
            DecoPiece("Toalla_Antigua_Doblada", PrimitiveType.Cube, zone19.transform,
                new Vector3(-36.55f, vestY + 0.83f, 71.75f), new Vector3(0.38f, 0.05f, 0.28f), matClothCover);

            // Esquemas de postura en la pared y pequeño armario donde se guardaban materiales de práctica
            CreateFramedArtPortrait("Esquema_Postura_Solo_1", zone19.transform,
                new Vector3(-37.68f, vestY + 2.3f, 70.5f), new Vector3(0.08f, 1.4f, 1.1f), matGold, matPortraitElena,
                "Esquema Anatómico de Equilibrio en Punta");
            CreateFramedArtPortrait("Esquema_Postura_Solo_2", zone19.transform,
                new Vector3(-37.68f, vestY + 2.3f, 77.5f), new Vector3(0.08f, 1.4f, 1.1f), matGold, matPhotoTroupe1923,
                "Diagrama de Giros Individuales — Variación Acto III");

            CreatePrimitive("Armario_Materiales_Practica", PrimitiveType.Cube, zone19.transform,
                new Vector3(-27.2f, vestY + 1.15f, 78.8f), new Vector3(1.4f, 2.3f, 0.75f), matMahogany);
            CreateOrnateSconce("Sconce_SoloRoom_1", zone19.transform,
                new Vector3(-32f, vestY + 3.4f, 79.65f), matGold, matLanternGlow, new Color(1f, 0.82f, 0.52f), 1.85f);

            // ANOMALÍA PSICOLÓGICA ZONA 19: Sonido de zapatillas deslizándose sobre la madera seguido de silencio + sombra fugaz en el espejo
            GameObject soloAnomalyHost = new GameObject("Anomaly_SoloRoom_SlippersAndShadow");
            soloAnomalyHost.transform.parent = zone19.transform;
            soloAnomalyHost.transform.position = new Vector3(-29.5f, vestY, 79.55f);

            GameObject fleetingMirrorShadow = CreateBalletSilhouetteFigure("Silhouette_SoloMirror_Fleeting", soloAnomalyHost.transform,
                new Vector3(0f, 0.06f, 0.04f), matShadowFigure, true);
            RehearsalAnomalyController anom19 = soloAnomalyHost.AddComponent<RehearsalAnomalyController>();
            anom19.Configure(RehearsalAnomalyType.SoloRoomSlippersAndShadow,
                fleetingMirrorShadow, null, null, null, null);
            EditorUtility.SetDirty(anom19);

            // ELEMENTOS NARRATIVOS DE LA ZONA 19 (SALA DE ENTRENAMIENTO INDIVIDUAL)
            CreateInteractiveWorldDoc("Doc_Diario_Practica_Presion", zone19.transform,
                new Vector3(-36.4f, vestY + 0.84f, 71.55f), new Vector3(0.36f, 0.04f, 0.26f), matGold,
                "doc_s4_solo_practice_diary",
                "Diario de Práctica Individual — La Presión del Último Acto",
                "SALA DE ENTRENAMIENTO PRIVADO — ANOTACIONES DE OCTUBRE\n\n" +
                "«11 de octubre: La Directora Vale cerró la puerta por fuera durante tres horas hasta que completé la variación sin un solo temblor en el tobillo.\n" +
                "14 de octubre: Mara me observa desde la rendija del aula de espejos. Dice que mi sombra ya no tiene fuerza para sostener la corona del Cisne.\n" +
                "16 de octubre: Ya no estoy sola cuando ensayo de madrugada. Cada vez que detengo los pies junto a la barra, escucho otras zapatillas deslizarse un paso más sobre la madera detrás de mí.»");

            CreateInteractiveWorldDoc("Doc_Secuencia_Movimientos_Elena", zone19.transform,
                new Vector3(-27.2f, vestY + 1.25f, 78.38f), new Vector3(0.38f, 0.08f, 0.24f), matSatinRibbon,
                "doc_s4_elena_movement_sequence",
                "Anotación Secreta de Elena — Secuencia de Pasos y Compases",
                "Guardada en el interior del armario de práctica hay una tarjeta manuscrita por Elena Vesper:\n\n" +
                "«SECUENCIA PERSONAL DE ENSAYO — COMPASES CLAVE:\n" +
                "I. Primer Arabesque — 4 tiempos\n" +
                "II. Pirueta en Attitude — 2 tiempos\n" +
                "III. Reverencia hacia el espejo — 7 tiempos\n\n" +
                "Si algo me ocurre durante el ensayo general en el escenario, buscad las partituras originales en el Archivo Musical entrando por el pasillo de servicio.»");

            // ====================================================================
            // ZONA 20: PASILLO DE SERVICIO (Tramo Norte Técnico + Atajo Sur al Vestíbulo)
            // ====================================================================
            GameObject zone20 = new GameObject("20_Service_Corridor");
            zone20.transform.parent = rootEnv;
            float serviceH = 5.0f;

            // --- 20A: TRAMO NORTE PRINCIPAL (X: -12 a -4, Z: 68 a 84) + RAMAL ESTE TRAS EL TEATRO (X: -4 a +8, Z: 78 a 84) ---
            CreatePrimitive("Service_North_Floor_Wood", PrimitiveType.Cube, zone20.transform,
                new Vector3(-8f, vestY - 0.25f, 76f), new Vector3(8f, 0.5f, 16f), matParquet);
            // Zonas de losas de piedra combinadas con la madera desgastada
            DecoPiece("Service_Stone_Slab_1", PrimitiveType.Cube, zone20.transform,
                new Vector3(-8f, vestY + 0.01f, 71.5f), new Vector3(5.6f, 0.02f, 4.2f), matStoneWall);
            DecoPiece("Service_Stone_Slab_2", PrimitiveType.Cube, zone20.transform,
                new Vector3(-8f, vestY + 0.01f, 80.5f), new Vector3(5.6f, 0.02f, 4.2f), matStoneWall);
            CreatePrimitive("Service_North_Ceiling", PrimitiveType.Cube, zone20.transform,
                new Vector3(-8f, vestY + serviceH, 76f), new Vector3(8f, 0.5f, 16f), matWallIvory);

            // Ramal Este de Servicio (X: -4 a +8, Z: 78 a 84) que conecta hacia la Zona Técnica del Teatro y el Archivo Musical
            CreatePrimitive("Service_EastWing_Floor", PrimitiveType.Cube, zone20.transform,
                new Vector3(2f, vestY - 0.25f, 81f), new Vector3(12f, 0.5f, 6f), matStoneWall);
            CreatePrimitive("Service_EastWing_Ceiling", PrimitiveType.Cube, zone20.transform,
                new Vector3(2f, vestY + serviceH, 81f), new Vector3(12f, 0.5f, 6f), matWallIvory);

            // Muro oeste del tramo norte (de Z = 82 a 84, ya que Z 68..82 lo cubre el muro este de Zona 18)
            CreateWall("Service_Wall_West_NorthEnd", zone20.transform, new Vector3(-12f, vestY + 2.5f, 83f), new Vector3(0.5f, serviceH, 2f), matWallIvory);

            // Muro norte (Z = 84, X: -12 a +8):
            // - Puerta Auxiliar cerrada en X = -8m (donde suena el piano lejano)
            // - Puerta a Zona Técnica del Teatro en X = 0m
            CreateWall("Service_Wall_North_1", zone20.transform, new Vector3(-10.525f, vestY + 2.5f, 84f), new Vector3(2.95f, serviceH, 0.5f), matWallIvory);
            CreateWall("Service_Wall_North_2", zone20.transform, new Vector3(-4.0f, vestY + 2.5f, 84f), new Vector3(5.90f, serviceH, 0.5f), matWallIvory);
            CreateWall("Service_Wall_North_3", zone20.transform, new Vector3(4.525f, vestY + 2.5f, 84f), new Vector3(6.95f, serviceH, 0.5f), matWallIvory);

            CreateArchitecturalDoor("Door_Auxiliary_Piano_Room", zone20.transform, new Vector3(-8f, vestY, 84f), Quaternion.identity, 1.8f, 3.2f, serviceH, 0.5f, true, "key_aux_rehearsal", "SALA AUXILIAR DE REPETICIÓN", "Sala Auxiliar de Repetición — Cerrada por dentro", matMahogany, matGold, matWallIvory);
            CreateArchitecturalDoor("Door_Service_To_TheatreTech", zone20.transform, new Vector3(0f, vestY, 84f), Quaternion.identity, 1.8f, 3.2f, serviceH, 0.5f, false, "", "PASILLO DETRÁS DEL ESCENARIO (ZONA 28)", "", matMahogany, matGold, matWallIvory);

            // Muro sur del ramal este (Z = 78, X: 4 a 8): con puerta directa en X = 6m hacia la Biblioteca Musical (Zona 21)
            CreateWall("Service_Wall_EastWing_South_W", zone20.transform, new Vector3(4.475f, vestY + 2.5f, 78f), new Vector3(0.95f, serviceH, 0.5f), matWallIvory);
            CreateWall("Service_Wall_EastWing_South_E", zone20.transform, new Vector3(7.525f, vestY + 2.5f, 78f), new Vector3(0.95f, serviceH, 0.5f), matWallIvory);
            CreateArchitecturalDoor("Door_Library_To_TechService", zone20.transform, new Vector3(6f, vestY, 78f), Quaternion.identity, 1.8f, 3.2f, serviceH, 0.5f, false, "", "ZONA TÉCNICA DEL ESCENARIO — SERVICIO", "", matMahogany, matGold, matWallIvory);

            // Muro extremo este del ramal (X = 8, Z: 78 a 84): Puerta abierta hacia la Sala del Piano (Zona 22) en Z = 81m
            CreateWall("Service_Wall_EastEnd_S", zone20.transform, new Vector3(8f, vestY + 2.5f, 78.975f), new Vector3(0.5f, serviceH, 1.95f), matWallIvory);
            CreateWall("Service_Wall_EastEnd_N", zone20.transform, new Vector3(8f, vestY + 2.5f, 83.025f), new Vector3(0.5f, serviceH, 1.95f), matWallIvory);
            CreateArchitecturalDoor("Door_Service_To_MusicArchive", zone20.transform, new Vector3(8f, vestY, 81f), Quaternion.Euler(0f, 90f, 0f), 1.8f, 3.2f, serviceH, 0.5f, false, "", "SALA DEL PIANO — ARCHIVO MUSICAL", "", matMahogany, matGold, matWallIvory);

            // Tuberías vistas a lo largo de las paredes y techo del pasillo de servicio
            DecoPiece("Service_Pipe_Overhead_1", PrimitiveType.Cylinder, zone20.transform,
                new Vector3(-11.4f, vestY + 4.35f, 76f), new Vector3(0.12f, 7.8f, 0.12f), matRustedLocker).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            DecoPiece("Service_Pipe_Overhead_2", PrimitiveType.Cylinder, zone20.transform,
                new Vector3(-11.1f, vestY + 4.10f, 76f), new Vector3(0.08f, 7.8f, 0.08f), matIron).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            DecoPiece("Service_Pipe_EastWing", PrimitiveType.Cylinder, zone20.transform,
                new Vector3(1.5f, vestY + 4.35f, 83.4f), new Vector3(0.12f, 6.2f, 0.12f), matRustedLocker).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            // Señalización interna para el personal
            CreateHistoricalPlaque("Placa_Servicio_Teatro_Archivo", zone20.transform, new Vector3(-4.32f, vestY + 2.4f, 76.0f), matGold);
            CreateHistoricalPlaque("Placa_Servicio_Vestuario", zone20.transform, new Vector3(-11.68f, vestY + 2.4f, 72.5f), matGold);

            // Carros de transporte, cajas de madera, escaleras plegables y objetos cubiertos con lonas
            CreateServiceCart("Carro_Transporte_Escenografia", zone20.transform, new Vector3(-6.2f, vestY, 70.5f), Quaternion.Euler(0f, 12f, 0f), matWoodArchive, matIron, matCardboard);
            CreateFoldingLadder("Escalera_Plegable_Servicio", zone20.transform, new Vector3(-10.8f, vestY, 79.2f), Quaternion.Euler(0f, 25f, 0f), matWoodArchive, matIron);

            CreatePrimitive("Caja_Madera_Servicio_1", PrimitiveType.Cube, zone20.transform, new Vector3(4.2f, vestY + 0.45f, 79.4f), new Vector3(1.2f, 0.9f, 1.0f), matWoodArchive);
            CreatePrimitive("Objeto_Cubierto_Lona_1", PrimitiveType.Cube, zone20.transform, new Vector3(5.6f, vestY + 0.65f, 79.5f), new Vector3(1.3f, 1.3f, 1.1f), matClothCover);

            // Lámparas metálicas sencillas en el pasillo de servicio
            CreateColdFluorescentFixture("Lampara_Servicio_Norte_1", zone20.transform, new Vector3(-8f, vestY + 4.75f, 72f), matIron, matLanternGlow, 1.6f);
            CreateColdFluorescentFixture("Lampara_Servicio_Norte_2", zone20.transform, new Vector3(-8f, vestY + 4.75f, 80f), matIron, matLanternGlow, 1.6f);
            CreateColdFluorescentFixture("Lampara_Servicio_RamalEste", zone20.transform, new Vector3(2.5f, vestY + 4.75f, 81f), matIron, matLanternGlow, 1.5f);

            // ANOMALÍA PSICOLÓGICA ZONA 20: Sonido lejano de piano detrás de la puerta auxiliar cerrada que calla al acercarse
            GameObject distantPianoAnomalyHost = new GameObject("Anomaly_ServiceCorridor_DistantPiano");
            distantPianoAnomalyHost.transform.parent = zone20.transform;
            distantPianoAnomalyHost.transform.position = new Vector3(-8f, vestY + 1.2f, 83.8f);

            RehearsalAnomalyController anom20 = distantPianoAnomalyHost.AddComponent<RehearsalAnomalyController>();
            anom20.Configure(RehearsalAnomalyType.ServiceCorridorDistantPiano,
                null, null, null, null, null);
            EditorUtility.SetDirty(anom20);

            // --- 20B: TRAMO SUR — ACCESO LATERAL DE REGRESO AL VESTÍBULO (X: -15 a -4, Z: 38 a 42) ---
            CreatePrimitive("Service_Shortcut_Floor", PrimitiveType.Cube, zone20.transform,
                new Vector3(-9.5f, vestY - 0.25f, 40f), new Vector3(11f, 0.5f, 4f), matParquet);
            DecoPiece("Service_Shortcut_StoneSlab", PrimitiveType.Cube, zone20.transform,
                new Vector3(-9.5f, vestY + 0.01f, 40f), new Vector3(6.5f, 0.02f, 2.2f), matStoneWall);
            CreatePrimitive("Service_Shortcut_Ceiling", PrimitiveType.Cube, zone20.transform,
                new Vector3(-9.5f, vestY + serviceH, 40f), new Vector3(11f, 0.5f, 4f), matWallIvory);

            CreateWall("Service_Shortcut_Wall_South", zone20.transform, new Vector3(-9.5f, vestY + 2.5f, 38f), new Vector3(11f, serviceH, 0.5f), matWallIvory);
            CreateWall("Service_Shortcut_Wall_North", zone20.transform, new Vector3(-9.5f, vestY + 2.5f, 42f), new Vector3(11f, serviceH, 0.5f), matWallIvory);

            DecoPiece("Service_Shortcut_Pipe", PrimitiveType.Cylinder, zone20.transform,
                new Vector3(-9.5f, vestY + 4.35f, 41.55f), new Vector3(0.10f, 5.2f, 0.10f), matRustedLocker).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            CreatePrimitive("Caja_Servicio_Atajo", PrimitiveType.Cube, zone20.transform,
                new Vector3(-9.5f, vestY + 0.40f, 38.75f), new Vector3(1.1f, 0.80f, 0.70f), matWoodArchive);
            CreateOrnateSconce("Sconce_Service_Shortcut", zone20.transform,
                new Vector3(-9.5f, vestY + 3.0f, 41.68f), matIron, matLanternGlow, new Color(1f, 0.82f, 0.52f), 1.8f);

            // ELEMENTOS NARRATIVOS DE LA ZONA 20 (PASILLO DE SERVICIO)
            CreateInteractiveWorldDoc("Doc_Registro_Traslado_Material", zone20.transform,
                new Vector3(4.2f, vestY + 0.93f, 79.4f), new Vector3(0.38f, 0.04f, 0.28f), matGold,
                "doc_s4_service_transfer_log",
                "Albarán de Traslado Interno — Personal de Tramoya y Servicio",
                "REGISTRO DE MOVIMIENTO DE MATERIAL ESCÉNICO (17 DE OCTUBRE, 21:30 H)\n\n" +
                "• Baúl Nº 04 (Partituras originales de 'El Lago de las Sombras'): Trasladado al Archivo Musical y cerrado con llave.\n" +
                "• Atrezo del Tercer Acto: Retirado del escenario principal hacia la Zona Técnica del Teatro.\n\n" +
                "Nota del encargado de turno:\n" +
                "“Nos ordenaron despejar el aula principal antes de las diez. Cuando pasamos con los carros frente a la Sala Auxiliar de Repetición, el piano de adentro seguía sonando a pesar de que la puerta llevaba cerrada con candado desde el mediodía.”");

            CreateInteractiveWorldDoc("Doc_Nota_Mantenimiento_Servicio", zone20.transform,
                new Vector3(-9.5f, vestY + 0.83f, 38.75f), new Vector3(0.34f, 0.04f, 0.26f), matGold,
                "doc_s4_maintenance_shortcut_note",
                "Aviso para el Personal — Corredor Lateral hacia el Vestíbulo",
                "COMUNICACIÓN INTERNA DE MANTENIMIENTO\n\n" +
                "“Este corredor de servicio permanece habilitado como paso directo entre las Aulas de Ensayo del ala norte y la salida hacia el Vestíbulo Principal.\n\n" +
                "Se recuerda a los tramoyistas no dejar carros bloqueando las puertas dobles ni responder si escuchan música procedente de las salas clausuradas después del toque de queda.”");
        }

        private static void CreateBalletBarre(string name, Transform parent, Vector3 pos, Quaternion rot, float length, Material woodMat, Material metalMat)
        {
            GameObject barre = new GameObject(name);
            barre.transform.parent = parent;
            barre.transform.position = pos;
            barre.transform.rotation = rot;

            // Barra horizontal superior e inferior de madera pulida
            GameObject topRail = CreatePrimitive("Barre_Wood_Top", PrimitiveType.Cylinder, barre.transform,
                new Vector3(0f, 1.05f, 0f), new Vector3(0.06f, length * 0.5f, 0.06f), woodMat);
            topRail.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            GameObject lowRail = DecoPiece("Barre_Wood_Lower", PrimitiveType.Cylinder, barre.transform,
                new Vector3(0f, 0.78f, 0f), new Vector3(0.05f, length * 0.5f, 0.05f), woodMat);
            lowRail.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            // Soportes metálicos envejecidos
            int postCount = Mathf.Max(2, Mathf.RoundToInt(length / 2.2f) + 1);
            for (int i = 0; i < postCount; i++)
            {
                float t = (postCount == 1) ? 0.5f : (float)i / (postCount - 1);
                float px = Mathf.Lerp(-length * 0.46f, length * 0.46f, t);
                CreatePrimitive($"Barre_Post_{i + 1}", PrimitiveType.Cylinder, barre.transform,
                    new Vector3(px, 0.52f, 0f), new Vector3(0.05f, 0.52f, 0.05f), metalMat);
                DecoPiece($"Barre_Base_{i + 1}", PrimitiveType.Cylinder, barre.transform,
                    new Vector3(px, 0.02f, 0f), new Vector3(0.18f, 0.02f, 0.18f), metalMat);
            }
        }

        private static void CreateChoreographyFloorMark(string name, Transform parent, Vector3 pos, Material chalkMat, bool isSoloistCross)
        {
            GameObject mark = new GameObject(name);
            mark.transform.parent = parent;
            mark.transform.position = pos;

            DecoPiece("Chalk_Line_A", PrimitiveType.Cube, mark.transform, Vector3.zero, new Vector3(0.55f, 0.008f, 0.05f), chalkMat).transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            DecoPiece("Chalk_Line_B", PrimitiveType.Cube, mark.transform, Vector3.zero, new Vector3(0.55f, 0.008f, 0.05f), chalkMat).transform.localRotation = Quaternion.Euler(0f, -45f, 0f);

            if (isSoloistCross)
            {
                DecoPiece("Chalk_Soloist_Ring", PrimitiveType.Cylinder, mark.transform, new Vector3(0f, -0.002f, 0f), new Vector3(0.75f, 0.004f, 0.75f), chalkMat);
            }
        }

        private static void CreateUprightPiano(string name, Transform parent, Vector3 pos, Quaternion rot, Material woodMat, Material brassMat, Material ivoryKeysMat, Material ironMat)
        {
            GameObject piano = new GameObject(name);
            piano.transform.parent = parent;
            piano.transform.position = pos;
            piano.transform.rotation = rot;

            // Cuerpo principal del piano vertical antiguo
            CreatePrimitive("Piano_Body_Upper", PrimitiveType.Cube, piano.transform, new Vector3(0f, 0.72f, 0f), new Vector3(1.55f, 1.44f, 0.62f), woodMat);
            CreatePrimitive("Piano_Keybed", PrimitiveType.Cube, piano.transform, new Vector3(0f, 0.76f, 0.42f), new Vector3(1.48f, 0.12f, 0.34f), woodMat);
            DecoPiece("Piano_WhiteKeys", PrimitiveType.Cube, piano.transform, new Vector3(0f, 0.83f, 0.44f), new Vector3(1.36f, 0.04f, 0.24f), ivoryKeysMat);
            DecoPiece("Piano_BlackKeys", PrimitiveType.Cube, piano.transform, new Vector3(0f, 0.86f, 0.38f), new Vector3(1.28f, 0.03f, 0.12f), ironMat);
            DecoPiece("Piano_MusicStand", PrimitiveType.Cube, piano.transform, new Vector3(0f, 1.08f, 0.33f), new Vector3(0.75f, 0.28f, 0.03f), woodMat);
            DecoPiece("Piano_Pedal_L", PrimitiveType.Cube, piano.transform, new Vector3(-0.14f, 0.08f, 0.34f), new Vector3(0.05f, 0.04f, 0.16f), brassMat);
            DecoPiece("Piano_Pedal_R", PrimitiveType.Cube, piano.transform, new Vector3(0.14f, 0.08f, 0.34f), new Vector3(0.05f, 0.04f, 0.16f), brassMat);

            // Banqueta de piano frente al teclado
            CreatePrimitive("Piano_Stool", PrimitiveType.Cube, piano.transform, new Vector3(0f, 0.26f, 0.92f), new Vector3(0.75f, 0.52f, 0.42f), woodMat);
        }

        private static GameObject CreateStageMannequin(string name, Transform parent, Vector3 pos, Quaternion rot, Material standWoodMat, Material metalMat, Material dressMat, bool clothCovered)
        {
            GameObject man = new GameObject(name);
            man.transform.parent = parent;
            man.transform.position = pos;
            man.transform.rotation = rot;

            // Pedestal y mástil metálico
            CreatePrimitive("Mannequin_Base", PrimitiveType.Cylinder, man.transform, new Vector3(0f, 0.04f, 0f), new Vector3(0.45f, 0.04f, 0.45f), standWoodMat);
            CreatePrimitive("Mannequin_Pole", PrimitiveType.Cylinder, man.transform, new Vector3(0f, 0.65f, 0f), new Vector3(0.05f, 0.65f, 0.05f), metalMat);

            // Torso, cabeza y postura de brazos en ballet
            CreatePrimitive("Mannequin_Torso", PrimitiveType.Capsule, man.transform, new Vector3(0f, 1.32f, 0f), new Vector3(0.32f, 0.36f, 0.24f), clothCovered ? dressMat : standWoodMat);
            DecoPiece("Mannequin_Head", PrimitiveType.Sphere, man.transform, new Vector3(0f, 1.78f, 0f), new Vector3(0.20f, 0.25f, 0.20f), clothCovered ? dressMat : standWoodMat);

            // Falda/tutú escénico o sábana cubriendo la figura
            if (clothCovered)
            {
                CreatePrimitive("Mannequin_DrapedSheet", PrimitiveType.Cylinder, man.transform, new Vector3(0f, 0.98f, 0f), new Vector3(0.68f, 0.68f, 0.68f), dressMat);
            }
            else
            {
                CreatePrimitive("Mannequin_TutuSkirt", PrimitiveType.Cylinder, man.transform, new Vector3(0f, 1.05f, 0f), new Vector3(0.88f, 0.08f, 0.88f), dressMat);
                DecoPiece("Mannequin_Bodice", PrimitiveType.Cylinder, man.transform, new Vector3(0f, 1.30f, 0f), new Vector3(0.34f, 0.22f, 0.26f), dressMat);
                DecoPiece("Mannequin_Arm_L", PrimitiveType.Capsule, man.transform, new Vector3(-0.26f, 1.48f, 0.10f), new Vector3(0.07f, 0.28f, 0.07f), standWoodMat).transform.localRotation = Quaternion.Euler(-25f, 0f, -38f);
                DecoPiece("Mannequin_Arm_R", PrimitiveType.Capsule, man.transform, new Vector3(0.26f, 1.56f, 0.08f), new Vector3(0.07f, 0.28f, 0.07f), standWoodMat).transform.localRotation = Quaternion.Euler(-15f, 0f, 45f);
            }

            return man;
        }

        private static GameObject CreateBalletSilhouetteFigure(string name, Transform parent, Vector3 localPos, Material shadowMat, bool arabesquePose)
        {
            GameObject fig = new GameObject(name);
            fig.transform.parent = parent;
            fig.transform.localPosition = localPos;
            fig.transform.localRotation = Quaternion.identity;

            DecoPiece("Sil_Head", PrimitiveType.Sphere, fig.transform, new Vector3(0f, 1.72f, 0f), new Vector3(0.20f, 0.24f, 0.05f), shadowMat);
            DecoPiece("Sil_Torso", PrimitiveType.Capsule, fig.transform, new Vector3(0f, 1.34f, 0f), new Vector3(0.26f, 0.34f, 0.05f), shadowMat);
            DecoPiece("Sil_Tutu", PrimitiveType.Cylinder, fig.transform, new Vector3(0f, 1.06f, 0f), new Vector3(0.82f, 0.06f, 0.10f), shadowMat);
            DecoPiece("Sil_StandingLeg", PrimitiveType.Capsule, fig.transform, new Vector3(-0.06f, 0.55f, 0f), new Vector3(0.09f, 0.48f, 0.05f), shadowMat);

            if (arabesquePose)
            {
                DecoPiece("Sil_ArabesqueLeg", PrimitiveType.Capsule, fig.transform, new Vector3(0.32f, 1.02f, 0f), new Vector3(0.08f, 0.44f, 0.05f), shadowMat).transform.localRotation = Quaternion.Euler(0f, 0f, -68f);
                DecoPiece("Sil_Arm_Forward", PrimitiveType.Capsule, fig.transform, new Vector3(-0.34f, 1.52f, 0f), new Vector3(0.06f, 0.34f, 0.04f), shadowMat).transform.localRotation = Quaternion.Euler(0f, 0f, 75f);
            }
            else
            {
                DecoPiece("Sil_SecondLeg", PrimitiveType.Capsule, fig.transform, new Vector3(0.08f, 0.55f, 0f), new Vector3(0.09f, 0.48f, 0.05f), shadowMat);
                DecoPiece("Sil_Arm_Crown_L", PrimitiveType.Capsule, fig.transform, new Vector3(-0.22f, 1.62f, 0f), new Vector3(0.06f, 0.28f, 0.04f), shadowMat).transform.localRotation = Quaternion.Euler(0f, 0f, -32f);
                DecoPiece("Sil_Arm_Crown_R", PrimitiveType.Capsule, fig.transform, new Vector3(0.22f, 1.62f, 0f), new Vector3(0.06f, 0.28f, 0.04f), shadowMat).transform.localRotation = Quaternion.Euler(0f, 0f, 32f);
            }

            return fig;
        }

        private static Light CreateSubtleAnomalyLight(string name, Transform parent, Vector3 localPos, Color col, float intensity)
        {
            GameObject lObj = new GameObject(name);
            lObj.transform.parent = parent;
            lObj.transform.localPosition = localPos;
            Light l = lObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = col;
            l.range = 6.5f;
            l.intensity = intensity;
            return l;
        }

        private static void CreateServiceCart(string name, Transform parent, Vector3 pos, Quaternion rot, Material woodMat, Material ironMat, Material boxMat)
        {
            GameObject cart = new GameObject(name);
            cart.transform.parent = parent;
            cart.transform.position = pos;
            cart.transform.rotation = rot;

            CreatePrimitive("Cart_Bed", PrimitiveType.Cube, cart.transform, new Vector3(0f, 0.28f, 0f), new Vector3(1.1f, 0.10f, 1.8f), woodMat);
            CreatePrimitive("Cart_HandleBar", PrimitiveType.Cube, cart.transform, new Vector3(0f, 0.68f, -0.86f), new Vector3(1.05f, 0.75f, 0.06f), ironMat);
            CreatePrimitive("Cart_CargoBox", PrimitiveType.Cube, cart.transform, new Vector3(0f, 0.58f, 0.15f), new Vector3(0.85f, 0.50f, 0.95f), boxMat);
        }

        private static void CreateFoldingLadder(string name, Transform parent, Vector3 pos, Quaternion rot, Material woodMat, Material ironMat)
        {
            GameObject ladder = new GameObject(name);
            ladder.transform.parent = parent;
            ladder.transform.position = pos;
            ladder.transform.rotation = rot;

            CreatePrimitive("Ladder_Side_A", PrimitiveType.Cube, ladder.transform, new Vector3(0f, 1.05f, 0.28f), new Vector3(0.65f, 2.15f, 0.06f), woodMat).transform.localRotation = Quaternion.Euler(14f, 0f, 0f);
            CreatePrimitive("Ladder_Side_B", PrimitiveType.Cube, ladder.transform, new Vector3(0f, 1.05f, -0.28f), new Vector3(0.65f, 2.15f, 0.06f), woodMat).transform.localRotation = Quaternion.Euler(-14f, 0f, 0f);
            DecoPiece("Ladder_TopHinge", PrimitiveType.Cube, ladder.transform, new Vector3(0f, 2.08f, 0f), new Vector3(0.70f, 0.06f, 0.22f), ironMat);
        }

        // ====================================================================
        // CONSTRUCCIÓN DE LA SECCIÓN 5: ARCHIVO MUSICAL — HISTORIA Y SONIDO (Zonas 21 a 25)
        // ====================================================================
        private static void BuildSection5MusicArchive(
            Transform rootEnv, float vestY,
            Material matParquet, Material matCarpet, Material matWallIvory, Material matMahogany,
            Material matGold, Material matIron, Material matClothCover, Material matVelvetDrapes,
            Material matLanternGlow, Material matColdFluorescent, Material matGreenVelvet,
            Material matLeather, Material matWoodArchive, Material matMetalArchive, Material matCardboard,
            Material matStoneWall, Material matRustedLocker, Material matMirrorGlass, Material matPorcelain,
            Material matTutuWhite, Material matTutuBlack, Material matSatinRibbon, Material matShadowFigure,
            Material matPortraitElena, Material matPhotoTroupe1923)
        {
            Material matAcousticPanel = GetOrCreateBasicMaterial("Mat_S5_AcousticFoamAged", new Color(0.28f, 0.24f, 0.20f), 0.10f, 0.0f);
            Material matDarkCoverCloth = GetOrCreateBasicMaterial("Mat_S5_DarkInstrumentCover", new Color(0.11f, 0.09f, 0.12f), 0.15f, 0.0f);
            Material matYellowedPaper = GetOrCreateBasicMaterial("Mat_S5_YellowedPaper", new Color(0.86f, 0.80f, 0.65f), 0.18f, 0.0f);

            // ====================================================================
            // ZONA 21: BIBLIOTECA MUSICAL (Galería X: 4..12, Z: 58..66 + Nave Principal X: 4..26, Z: 66..78)
            // ====================================================================
            GameObject zone21 = new GameObject("21_Music_Library");
            zone21.transform.parent = rootEnv;
            float libH = 5.4f;

            // 21A: Galería de entrada y orientación desde el Pasillo Principal (X: 4..12, Z: 58..66)
            CreatePrimitive("Library_Entry_Floor", PrimitiveType.Cube, zone21.transform,
                new Vector3(8f, vestY - 0.25f, 62f), new Vector3(8f, 0.5f, 8f), matParquet);
            CreatePrimitive("Library_Entry_Carpet", PrimitiveType.Cube, zone21.transform,
                new Vector3(8f, vestY + 0.02f, 62f), new Vector3(5.5f, 0.04f, 3.0f), matCarpet);
            CreatePrimitive("Library_Entry_Ceiling", PrimitiveType.Cube, zone21.transform,
                new Vector3(8f, vestY + libH, 62f), new Vector3(8f, 0.5f, 8f), matWallIvory);
            CreateWall("Library_Entry_Wall_South", zone21.transform,
                new Vector3(8f, vestY + (libH * 0.5f), 58f), new Vector3(8f, libH, 0.5f), matWallIvory);

            // 21B: Nave principal alargada de la Biblioteca Musical (X: 4..26, Z: 66..78)
            CreatePrimitive("Library_Main_Floor", PrimitiveType.Cube, zone21.transform,
                new Vector3(15f, vestY - 0.25f, 72f), new Vector3(22f, 0.5f, 12f), matParquet);
            CreatePrimitive("Library_Main_Carpet", PrimitiveType.Cube, zone21.transform,
                new Vector3(15f, vestY + 0.02f, 72f), new Vector3(16f, 0.04f, 4.2f), matCarpet);
            CreatePrimitive("Library_Main_Ceiling", PrimitiveType.Cube, zone21.transform,
                new Vector3(15f, vestY + libH, 72f), new Vector3(22f, 0.5f, 12f), matWallIvory);

            // Muro norte de la Biblioteca (Z = 78, X: 8 a 26; X: 4 a 8 ya tiene la puerta a Zona Técnica en X = 6m):
            // Puertas dobles en X = 17m hacia Zona 22 (Sala del Piano)
            CreateWall("Library_Wall_North_W", zone21.transform, new Vector3(11.5f, vestY + (libH * 0.5f), 78f), new Vector3(7.0f, libH, 0.5f), matWallIvory);
            CreateWall("Library_Wall_North_E", zone21.transform, new Vector3(22.5f, vestY + (libH * 0.5f), 78f), new Vector3(7.0f, libH, 0.5f), matWallIvory);
            CreateArchitecturalDoubleDoor("Doors_Library_To_PianoRoom", zone21.transform, new Vector3(17f, vestY, 78f), Quaternion.identity,
                3.7f, 3.6f, libH, 0.5f, false, "", "SALA DEL PIANO DE COLA", "", matMahogany, matGold, matWallIvory);

            // Muro este de la Biblioteca (X = 26, Z: 66 a 78): puerta en Z = 72m hacia Zona 23 (Archivo de Partituras)
            CreateWall("Library_Wall_East_S", zone21.transform, new Vector3(26f, vestY + (libH * 0.5f), 68.475f), new Vector3(0.5f, libH, 4.95f), matWallIvory);
            CreateWall("Library_Wall_East_N", zone21.transform, new Vector3(26f, vestY + (libH * 0.5f), 75.525f), new Vector3(0.5f, libH, 4.95f), matWallIvory);
            CreateArchitecturalDoor("Door_Library_To_ScoreArchive", zone21.transform, new Vector3(26f, vestY, 72f), Quaternion.Euler(0f, 90f, 0f),
                1.8f, 3.2f, libH, 0.5f, false, "", "ARCHIVO DE PARTITURAS", "", matMahogany, matGold, matWallIvory);

            // Ventana alta con entrada de luz fría lunar en la Biblioteca Musical
            DecoPiece("Library_HighWindow_Frame", PrimitiveType.Cube, zone21.transform,
                new Vector3(11.5f, vestY + 4.1f, 77.70f), new Vector3(2.8f, 1.6f, 0.12f), matMahogany);
            DecoPiece("Library_HighWindow_Glass", PrimitiveType.Cube, zone21.transform,
                new Vector3(11.5f, vestY + 4.1f, 77.74f), new Vector3(2.5f, 1.35f, 0.06f), matColdFluorescent);
            CreatePictureSpotlight("Moonlight_MusicLibrary", zone21.transform,
                new Vector3(11.5f, vestY + 4.2f, 77.0f), new Vector3(13.5f, vestY + 0.5f, 70.5f), new Color(0.58f, 0.76f, 0.98f), 2.5f);

            // Estanterías alargadas de madera oscura y armarios para partituras
            float[] shelfX = { 6.2f, 10.5f, 21.5f, 24.4f };
            for (int s = 0; s < shelfX.Length; s++)
            {
                CreatePrimitive($"Library_Bookshelf_{s + 1}", PrimitiveType.Cube, zone21.transform,
                    new Vector3(shelfX[s], vestY + 1.85f, 67.2f), new Vector3(2.6f, 3.7f, 0.75f), matMahogany);
            }
            CreatePrimitive("Library_Bookshelf_SouthEntry", PrimitiveType.Cube, zone21.transform,
                new Vector3(8.0f, vestY + 1.85f, 58.65f), new Vector3(5.2f, 3.7f, 0.75f), matWoodArchive);
            CreatePrimitive("Library_SheetCabinet_West", PrimitiveType.Cube, zone21.transform,
                new Vector3(4.65f, vestY + 1.55f, 72.0f), new Vector3(0.75f, 3.1f, 6.2f), matMahogany);

            // Mesas de lectura con lámparas antiguas de luz amarillenta y libros de teoría musical
            float[] tableX = { 11.5f, 18.5f };
            for (int t = 0; t < tableX.Length; t++)
            {
                float tx = tableX[t];
                CreatePrimitive($"Reading_Table_{t + 1}", PrimitiveType.Cube, zone21.transform,
                    new Vector3(tx, vestY + 0.44f, 72.0f), new Vector3(3.2f, 0.88f, 1.5f), matMahogany);
                CreatePrimitive($"Reading_Chair_{t + 1}_S", PrimitiveType.Cube, zone21.transform,
                    new Vector3(tx, vestY + 0.50f, 70.8f), new Vector3(0.75f, 1.0f, 0.75f), matLeather);
                CreatePrimitive($"Reading_Chair_{t + 1}_N", PrimitiveType.Cube, zone21.transform,
                    new Vector3(tx, vestY + 0.50f, 73.2f), new Vector3(0.75f, 1.0f, 0.75f), matLeather);

                // Lámpara antigua de lectura con luz amarillenta cálida
                DecoPiece($"Reading_Lamp_Base_{t + 1}", PrimitiveType.Cylinder, zone21.transform,
                    new Vector3(tx - 0.8f, vestY + 0.94f, 72.0f), new Vector3(0.20f, 0.05f, 0.20f), matGold);
                DecoPiece($"Reading_Lamp_Shade_{t + 1}", PrimitiveType.Cylinder, zone21.transform,
                    new Vector3(tx - 0.8f, vestY + 1.22f, 72.0f), new Vector3(0.36f, 0.10f, 0.36f), matLanternGlow);
                Light lampL = CreateSubtleAnomalyLight($"Reading_Lamp_Light_{t + 1}", zone21.transform,
                    new Vector3(tx - 0.8f, vestY + 1.18f, 72.0f), new Color(1.0f, 0.84f, 0.46f), 2.3f);
                lampL.range = 8.5f;
            }

            // Atril vacío en la biblioteca y compartimento discreto en el mueble del fondo
            CreateMusicStand("Atril_Vacio_Biblioteca", zone21.transform, new Vector3(15.0f, vestY, 69.2f), Quaternion.Euler(0f, 15f, 0f), matMahogany, matGold);
            CreatePrimitive("Compartimento_Discreto_Mueble", PrimitiveType.Cube, zone21.transform,
                new Vector3(23.2f, vestY + 0.55f, 76.8f), new Vector3(1.6f, 1.1f, 0.85f), matMahogany);

            // Hojas de partitura desordenadas por el suelo cubiertas de polvo
            Vector3[] scatteredSheets =
            {
                new Vector3(9.5f, vestY + 0.025f, 64.2f),
                new Vector3(13.8f, vestY + 0.025f, 70.4f),
                new Vector3(16.4f, vestY + 0.025f, 73.6f),
                new Vector3(21.2f, vestY + 0.025f, 71.1f)
            };
            for (int sh = 0; sh < scatteredSheets.Length; sh++)
            {
                DecoPiece($"Hoja_Partitura_Suelo_{sh + 1}", PrimitiveType.Cube, zone21.transform,
                    scatteredSheets[sh], new Vector3(0.32f, 0.008f, 0.24f), matYellowedPaper).transform.localRotation = Quaternion.Euler(0f, sh * 37f, 0f);
            }

            // ANOMALÍA AUDITIVA ZONA 21: Páginas que se pasan solas y sonido procedente de una estantería vacía
            GameObject rustlingHost = new GameObject("Anomaly_Library_RustlingPages");
            rustlingHost.transform.parent = zone21.transform;
            rustlingHost.transform.position = new Vector3(21.5f, vestY + 1.3f, 67.6f);
            GameObject movingBook = DecoPiece("Libro_Partitura_Movil", PrimitiveType.Cube, rustlingHost.transform,
                Vector3.zero, new Vector3(0.34f, 0.06f, 0.26f), matYellowedPaper);
            MusicArchiveAnomalyController anom21 = rustlingHost.AddComponent<MusicArchiveAnomalyController>();
            anom21.Configure(MusicArchiveAnomalyType.LibraryRustlingPages, movingBook.transform);
            EditorUtility.SetDirty(anom21);

            // DOCUMENTOS DE LA ZONA 21 (BIBLIOTECA MUSICAL)
            CreateInteractiveWorldDoc("Doc_Catalogo_Musical_Abierto", zone21.transform,
                new Vector3(11.5f, vestY + 0.92f, 72.0f), new Vector3(0.48f, 0.06f, 0.36f), matGold,
                "doc_s5_music_catalog",
                "Catálogo General del Archivo Musical (1918–1924)",
                "REGISTRO DE COMPOSICIONES ESCÉNICAS — ACADEMIA VESPER\n\n" +
                "• Ref. M-104 (Octubre 1922): «El Lago de las Sombras — Versión Original del Maestro K. Vesper».\n" +
                "  Cadencia del Acto Final escrita en Mi menor pura: «Mi - Sol - Si - Do Natural - Mi».\n\n" +
                "• Ref. M-104/B (Septiembre 1924): « Primera Revisión por orden de Isadora Vale».\n" +
                "  Se acorta el solo del Cisne Blanco.\n\n" +
                "• Ref. M-104/C (12 de Octubre 1924): «Versión Oficial de Estreno — Modificada por Isadora Vale».\n" +
                "  Nota al margen del bibliotecario: «La Directora ordenó alterar la cuarta nota de la coda en el piano de cola y retirar la partitura original de 1922 al Archivo de Partituras. El atril del piano de cola solo se desbloquea al tocar la cadencia original de 1922»");

            CreateInteractiveWorldDoc("Doc_Libros_Teoria_Y_Registros", zone21.transform,
                new Vector3(18.5f, vestY + 0.92f, 72.0f), new Vector3(0.42f, 0.05f, 0.32f), matGold,
                "doc_s5_theory_and_versions",
                "Tratado de Armonía Escénica y Registro de Modificaciones",
                "Entre las páginas de un tratado de teoría musical aparece una ficha comparativa de puño y letra de Elena Vesper:\n\n" +
                "“Isadora cree que nadie nota por qué cambió los últimos dieciséis compases de 'El Lago de las Sombras'.\n" +
                "En la versión original, la solista permanece en el centro del proscenio mientras sube la luz. En la versión modificada por Vale el 12 de octubre, la música introduce un acorde falso para dar entrada a una segunda bailarina desde la sombra y apagar las candilejas.”");

            CreateInteractiveWorldDoc("Doc_Compartimento_Discreto_Biblio", zone21.transform,
                new Vector3(23.2f, vestY + 1.14f, 76.8f), new Vector3(0.36f, 0.04f, 0.26f), matSatinRibbon,
                "doc_s5_discreet_compartment_note",
                "Nota Guardada en el Compartimento Discreto de la Biblioteca",
                "“Al archivero jefe:\n\n" +
                "He guardado las tres carpetas de ensayo en el Archivo de Partituras (puerta este) y las cintas magnéticas de la última semana en la Cabina de Grabaciones.\n" +
                "Por favor, si escuchas pasar páginas en la estantería del fondo cuando la biblioteca está cerrada, no enciendas las luces altas: el aire que sube desde el foso de orquesta mueve los papeles... o eso prefiero creer.”");

            // ====================================================================
            // ZONA 22: SALA DEL PIANO (X: 8 a 26, Z: 78 a 92, Y: 2.4m, Altura: 5.6m)
            // ====================================================================
            GameObject zone22 = new GameObject("22_Grand_Piano_Room");
            zone22.transform.parent = rootEnv;
            float pianoRoomH = 5.6f;

            CreatePrimitive("PianoRoom_Floor", PrimitiveType.Cube, zone22.transform,
                new Vector3(17f, vestY - 0.25f, 85f), new Vector3(18f, 0.5f, 14f), matParquet);
            CreatePrimitive("PianoRoom_Rug", PrimitiveType.Cube, zone22.transform,
                new Vector3(17f, vestY + 0.02f, 85f), new Vector3(9.5f, 0.04f, 8.0f), matCarpet);
            CreatePrimitive("PianoRoom_Ceiling", PrimitiveType.Cube, zone22.transform,
                new Vector3(17f, vestY + pianoRoomH, 85f), new Vector3(18f, 0.5f, 14f), matWallIvory);

            // Muros perimetrales de la Sala del Piano (Z = 78 ya está construido por Zona 21; X = 8 de Z 78..84 por Zona 20)
            CreateWall("PianoRoom_Wall_West_N", zone22.transform, new Vector3(8f, vestY + (pianoRoomH * 0.5f), 88f), new Vector3(0.5f, pianoRoomH, 8f), matWallIvory);
            CreateWall("PianoRoom_Wall_North", zone22.transform, new Vector3(17f, vestY + (pianoRoomH * 0.5f), 92f), new Vector3(18f, pianoRoomH, 0.5f), matWallIvory);
            // Muro este (X = 26, Z: 78 a 92) con puerta en Z = 80.5m hacia el Vestíbulo de Camerinos (Zonas 26 y 27)
            CreateWall("PianoRoom_Wall_East_S", zone22.transform, new Vector3(26f, vestY + (pianoRoomH * 0.5f), 78.725f), new Vector3(0.5f, pianoRoomH, 1.45f), matWallIvory);
            CreateWall("PianoRoom_Wall_East_N", zone22.transform, new Vector3(26f, vestY + (pianoRoomH * 0.5f), 86.775f), new Vector3(0.5f, pianoRoomH, 10.45f), matWallIvory);
            CreateArchitecturalDoor("Door_PianoRoom_To_DressingHall", zone22.transform, new Vector3(26f, vestY, 80.5f), Quaternion.Euler(0f, 90f, 0f),
                1.8f, 3.2f, pianoRoomH, 0.5f, false, "", "CAMERINOS DE MARA Y ELENA — BACKSTAGE", "", matMahogany, matGold, matWallIvory);

            // Pared norte cubierta con pesadas cortinas de terciopelo borgoña hacia donde mira el piano de cola
            for (int c = 0; c < 5; c++)
            {
                float cx = 10.6f + (c * 3.2f);
                DecoPiece($"PianoRoom_BurgundyDrape_{c + 1}", PrimitiveType.Cube, zone22.transform,
                    new Vector3(cx, vestY + 2.6f, 91.58f), new Vector3(2.6f, 4.8f, 0.22f), matVelvetDrapes);
            }

            // Piano de cola antiguo en el centro de la sala, orientado hacia las cortinas borgoña
            GameObject grandPiano = CreateGrandPiano("Piano_De_Cola_Antiguo", zone22.transform,
                new Vector3(17f, vestY, 85.2f), Quaternion.identity,
                matMahogany, matGold, matPorcelain, matIron, matSatinRibbon);

            // Atriles, sillas, escritorio con anotaciones y metrónomo antiguo
            CreateMusicStand("Atril_SalaPiano_1", zone22.transform, new Vector3(14.0f, vestY, 87.2f), Quaternion.Euler(0f, 25f, 0f), matMahogany, matGold);
            CreateMusicStand("Atril_SalaPiano_2", zone22.transform, new Vector3(20.0f, vestY, 87.2f), Quaternion.Euler(0f, -25f, 0f), matMahogany, matGold);

            CreatePrimitive("Silla_Camara_1", PrimitiveType.Cube, zone22.transform,
                new Vector3(13.5f, vestY + 0.50f, 88.0f), new Vector3(0.75f, 1.0f, 0.75f), matMahogany);
            CreatePrimitive("Silla_Camara_2", PrimitiveType.Cube, zone22.transform,
                new Vector3(20.5f, vestY + 0.50f, 88.0f), new Vector3(0.75f, 1.0f, 0.75f), matMahogany);

            // Escritorio del maestro repetidor con metrónomo antiguo
            CreatePrimitive("Escritorio_Maestro_Piano", PrimitiveType.Cube, zone22.transform,
                new Vector3(23.8f, vestY + 0.45f, 82.0f), new Vector3(1.4f, 0.90f, 2.4f), matMahogany);
            DecoPiece("Metronomo_Antiguo_SalaPiano", PrimitiveType.Cube, zone22.transform,
                new Vector3(23.6f, vestY + 1.04f, 81.3f), new Vector3(0.22f, 0.32f, 0.22f), matGold);

            CreateChandelier("Arana_Sala_Piano", zone22.transform, new Vector3(17f, vestY + 4.9f, 85f), matGold, new Color(1f, 0.82f, 0.50f), 2.5f);

            // ANOMALÍA PSICOLÓGICA ZONA 22: El piano produce notas por sí solo con una nota equivocada cuando los jugadores están a distancia
            GameObject selfPlayingHost = new GameObject("Anomaly_GrandPiano_WrongNote");
            selfPlayingHost.transform.parent = zone22.transform;
            selfPlayingHost.transform.position = new Vector3(17f, vestY + 1.0f, 85.2f);
            MusicArchiveAnomalyController anom22 = selfPlayingHost.AddComponent<MusicArchiveAnomalyController>();
            anom22.Configure(MusicArchiveAnomalyType.PianoSelfPlayingWrongNote);
            EditorUtility.SetDirty(anom22);

            // DOCUMENTOS DE LA ZONA 22 (SALA DEL PIANO)
            CreateInteractiveWorldDoc("Doc_Partitura_Piano_Cola", zone22.transform,
                new Vector3(17.9f, vestY + 1.02f, 84.3f), new Vector3(0.42f, 0.04f, 0.32f), matYellowedPaper,
                "doc_s5_grand_piano_incomplete_score",
                "Partitura Incompleta de 'El Lago de las Sombras' (Sobre el Piano de Cola)",
                "CODA DEL TERCER ACTO — EJEMPLAR DE REPETICIÓN\n\n" +
                "Sobre la madera del piano de cola descansa la partitura de ensayo con correcciones superpuestas en tinta roja y lápiz negro.\n\n" +
                "Donde la versión oficial exige tocar un acorde cortante y bajar el telón, una anotación manuscrita al pie indica:\n" +
                "«El final original nunca debía interrumpirse así. Si reproduces en el teclado la secuencia original anotada en el Catálogo de la Biblioteca (Mi - Sol - Si - Do Natural - Mi), el compartimento del atril se abrirá»");

            CreateInteractiveWorldDoc("Doc_Escritorio_Sala_Piano", zone22.transform,
                new Vector3(23.6f, vestY + 0.94f, 82.2f), new Vector3(0.38f, 0.04f, 0.28f), matGold,
                "doc_s5_repetiteur_desk_notes",
                "Cuaderno del Maestro Repetidor — La Nota Equivocada",
                "“15 de Octubre de 1924\n\n" +
                "Cada vez que intento ensayar la nueva versión que me impuso la Directora Vale, mis dedos se detienen en el cuarto compás.\n" +
                "Anoche cerré la tapa del piano de cola a las diez en punto. Al cruzar la biblioteca hacia el pasillo, escuché el piano sonar detrás de mí a puerta cerrada... repitiendo una y otra vez esa nota falsa que destruye la armonía de Elena.”");

            // ====================================================================
            // ZONA 23: ARCHIVO DE PARTITURAS (X: 26 a 39, Z: 62 a 78, Y: 2.4m, Altura: 5.2m)
            // ====================================================================
            GameObject zone23 = new GameObject("23_Sheet_Music_Archive");
            zone23.transform.parent = rootEnv;
            float scoreArchH = 5.2f;

            CreatePrimitive("ScoreArchive_Floor", PrimitiveType.Cube, zone23.transform,
                new Vector3(32.5f, vestY - 0.25f, 70f), new Vector3(13f, 0.5f, 16f), matParquet);
            CreatePrimitive("ScoreArchive_Ceiling", PrimitiveType.Cube, zone23.transform,
                new Vector3(32.5f, vestY + scoreArchH, 70f), new Vector3(13f, 0.5f, 16f), matWallIvory);

            // Muros norte (Z = 78) y este (X = 39)
            CreateWall("ScoreArchive_Wall_North", zone23.transform, new Vector3(32.5f, vestY + (scoreArchH * 0.5f), 78f), new Vector3(13f, scoreArchH, 0.5f), matWallIvory);
            CreateWall("ScoreArchive_Wall_East", zone23.transform, new Vector3(39f, vestY + (scoreArchH * 0.5f), 70f), new Vector3(0.5f, scoreArchH, 16f), matWallIvory);

            // Muro sur (Z = 62, X: 26 a 39):
            // - Puerta en X = 28.0m hacia el Corredor del Depósito de Instrumentos (Zona 25)
            // - Puerta en X = 34.5m hacia la Cabina de Grabaciones (Zona 24)
            CreateWall("ScoreArchive_Wall_South_W", zone23.transform, new Vector3(26.475f, vestY + (scoreArchH * 0.5f), 62f), new Vector3(0.95f, scoreArchH, 0.5f), matWallIvory);
            CreateWall("ScoreArchive_Wall_South_Mid", zone23.transform, new Vector3(31.25f, vestY + (scoreArchH * 0.5f), 62f), new Vector3(4.40f, scoreArchH, 0.5f), matWallIvory);
            CreateWall("ScoreArchive_Wall_South_E", zone23.transform, new Vector3(37.275f, vestY + (scoreArchH * 0.5f), 62f), new Vector3(3.45f, scoreArchH, 0.5f), matWallIvory);

            CreateArchitecturalDoor("Door_ScoreArchive_To_InstrumentPass", zone23.transform, new Vector3(28.0f, vestY, 62f), Quaternion.identity,
                1.8f, 3.2f, scoreArchH, 0.5f, false, "", "DEPÓSITO DE INSTRUMENTOS", "", matMahogany, matGold, matWallIvory);
            CreateArchitecturalDoor("Door_ScoreArchive_To_RecordingBooth", zone23.transform, new Vector3(34.5f, vestY, 62f), Quaternion.identity,
                1.8f, 3.2f, scoreArchH, 0.5f, false, "", "CABINA DE GRABACIONES", "", matMahogany, matGold, matWallIvory);

            // Archivadores de madera ordenados con cajones etiquetados
            for (int cab = 0; cab < 4; cab++)
            {
                float cz = 65.0f + (cab * 3.4f);
                CreatePrimitive($"Score_DrawerCabinet_E_{cab + 1}", PrimitiveType.Cube, zone23.transform,
                    new Vector3(38.1f, vestY + 1.9f, cz), new Vector3(0.9f, 3.8f, 2.8f), matWoodArchive);
                DecoPiece($"Score_DrawerLabel_Low_{cab + 1}", PrimitiveType.Cube, zone23.transform,
                    new Vector3(37.62f, vestY + 1.2f, cz), new Vector3(0.02f, 0.14f, 0.45f), matGold);
                DecoPiece($"Score_DrawerLabel_High_{cab + 1}", PrimitiveType.Cube, zone23.transform,
                    new Vector3(37.62f, vestY + 2.6f, cz), new Vector3(0.02f, 0.14f, 0.45f), matGold);
            }

            CreatePrimitive("Score_Cabinet_North_1", PrimitiveType.Cube, zone23.transform,
                new Vector3(30.0f, vestY + 1.9f, 77.1f), new Vector3(4.2f, 3.8f, 0.9f), matWoodArchive);
            CreatePrimitive("Score_Cabinet_North_2", PrimitiveType.Cube, zone23.transform,
                new Vector3(35.0f, vestY + 1.9f, 77.1f), new Vector3(4.2f, 3.8f, 0.9f), matWoodArchive);

            // Escalera móvil para alcanzar los compartimentos superiores de los archivadores
            GameObject rollingLadder = new GameObject("Escalera_Movil_Archivadores");
            rollingLadder.transform.parent = zone23.transform;
            rollingLadder.transform.position = new Vector3(37.2f, vestY, 71.5f);
            DecoPiece("Ladder_Rail_Top", PrimitiveType.Cylinder, zone23.transform,
                new Vector3(37.55f, vestY + 3.7f, 70.0f), new Vector3(0.05f, 6.2f, 0.05f), matIron).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            CreatePrimitive("RollingLadder_Frame", PrimitiveType.Cube, rollingLadder.transform,
                new Vector3(0f, 1.85f, 0f), new Vector3(0.12f, 3.8f, 0.65f), matMahogany).transform.localRotation = Quaternion.Euler(0f, 0f, -12f);

            // Mesa central de clasificación con interacción para ordenar partituras por fecha
            GameObject sortingTable = CreatePrimitive("Mesa_Central_Clasificacion_Partituras", PrimitiveType.Cube, zone23.transform,
                new Vector3(32.5f, vestY + 0.45f, 70.0f), new Vector3(3.6f, 0.90f, 2.0f), matMahogany);

            GameObject folderStack = DecoPiece("Carpetas_Por_Ordenar", PrimitiveType.Cube, sortingTable.transform,
                new Vector3(-0.2f, 0.54f, 0f), new Vector3(0.25f, 0.06f, 0.35f), matYellowedPaper);
            GameObject reconstructedReveal = DecoPiece("Seccion_Musical_Reconstruida_Visual", PrimitiveType.Cube, sortingTable.transform,
                new Vector3(0.25f, 0.55f, 0f), new Vector3(0.22f, 0.05f, 0.32f), matGold);

            MusicArchiveAnomalyController sortingInteract = sortingTable.AddComponent<MusicArchiveAnomalyController>();
            sortingInteract.Configure(MusicArchiveAnomalyType.InteractiveScoreSortingTable, folderStack.transform, null, null, reconstructedReveal);
            EditorUtility.SetDirty(sortingInteract);

            CreateOrnateLantern("Lampara_Archivo_Partituras", zone23.transform, new Vector3(32.5f, vestY + 4.5f, 70f), matGold, matLanternGlow, new Color(1f, 0.84f, 0.52f), 2.2f);

            // DOCUMENTOS DE LA ZONA 23 (ARCHIVO DE PARTITURAS)
            CreateInteractiveWorldDoc("Doc_Carpeta_Vacia_Final_Original", zone23.transform,
                new Vector3(30.0f, vestY + 1.15f, 76.55f), new Vector3(0.42f, 0.04f, 0.30f), matCardboard,
                "doc_s5_empty_original_ending_folder",
                "Carpeta Vacía — «Final Original de la Composición (Octubre 1922)»",
                "CARPETA ARCHIVADORA Nº 01 — SELLO DE DIRECCIÓN\n\n" +
                "La carpeta que debería contener el manuscrito íntegro del desenlace original de «El Lago de las Sombras» está completamente vacía.\n" +
                "En la solapa interior solo queda el rastro de lacre roto y un recibo de retirada firmado el 14 de octubre a las 19:40 h.\n\n" +
                "Usa la Mesa Central de Clasificación ([E]) para ordenar cronológicamente los borradores restantes y reconstruir la sección arrancada.");

            CreateInteractiveWorldDoc("Doc_Discrepancias_Elena_Mara_Partituras", zone23.transform,
                new Vector3(35.0f, vestY + 1.15f, 76.55f), new Vector3(0.40f, 0.04f, 0.30f), matYellowedPaper,
                "doc_s5_elena_mara_score_discrepancies",
                "Partituras Anotadas por Elena Vesper y Mara Bell (Comparativa)",
                "Dos ejemplares de ensayo colocados uno junto al otro muestran una contradicción evidente:\n\n" +
                "• En la partitura de Elena Vesper: las indicaciones de escena dicen «La solista permanece sola junto al espejo central hasta el último acorde en Do Natural».\n" +
                "• En la partitura de Mara Bell (fechada antes de la desaparición de Elena): alguien escribió con tinta de Dirección «En el compás 48, cortar la luz del proscenio y cerrar el paso lateral antes de que termine la coda».");

            // ====================================================================
            // ZONA 24: CABINA DE GRABACIONES (X: 30 a 39, Z: 50 a 62, Y: 2.4m, Altura: 4.8m)
            // ====================================================================
            GameObject zone24 = new GameObject("24_Recording_Booth");
            zone24.transform.parent = rootEnv;
            float boothH = 4.8f;

            CreatePrimitive("Booth_Floor", PrimitiveType.Cube, zone24.transform,
                new Vector3(34.5f, vestY - 0.25f, 56f), new Vector3(9f, 0.5f, 12f), matParquet);
            CreatePrimitive("Booth_Ceiling", PrimitiveType.Cube, zone24.transform,
                new Vector3(34.5f, vestY + boothH, 56f), new Vector3(9f, 0.5f, 12f), matAcousticPanel);

            // Muros este (X = 39), sur (Z = 50) y oeste (X = 30, con puerta en Z = 56m hacia el corredor del Depósito de Instrumentos)
            CreateWall("Booth_Wall_East", zone24.transform, new Vector3(39f, vestY + (boothH * 0.5f), 56f), new Vector3(0.5f, boothH, 12f), matMahogany);
            CreateWall("Booth_Wall_South", zone24.transform, new Vector3(34.5f, vestY + (boothH * 0.5f), 50f), new Vector3(9f, boothH, 0.5f), matMahogany);
            CreateWall("Booth_Wall_West_S", zone24.transform, new Vector3(30f, vestY + (boothH * 0.5f), 52.475f), new Vector3(0.5f, boothH, 4.95f), matMahogany);
            CreateWall("Booth_Wall_West_N", zone24.transform, new Vector3(30f, vestY + (boothH * 0.5f), 59.525f), new Vector3(0.5f, boothH, 4.95f), matMahogany);
            CreateArchitecturalDoor("Door_RecordingBooth_To_InstrumentPass", zone24.transform, new Vector3(30f, vestY, 56f), Quaternion.Euler(0f, 90f, 0f),
                1.8f, 3.2f, boothH, 0.5f, false, "", "PASILLO AL DEPÓSITO DE INSTRUMENTOS", "", matMahogany, matGold, matMahogany);

            // Revestimiento acústico absorbente envejecido en las paredes de la cabina
            for (int ap = 0; ap < 4; ap++)
            {
                float az = 51.8f + (ap * 2.8f);
                DecoPiece($"Acoustic_Baffle_E_{ap + 1}", PrimitiveType.Cube, zone24.transform,
                    new Vector3(38.65f, vestY + 2.2f, az), new Vector3(0.14f, 3.2f, 2.2f), matAcousticPanel);
            }
            for (int sp = 0; sp < 3; sp++)
            {
                float sx = 31.8f + (sp * 2.6f);
                DecoPiece($"Acoustic_Baffle_S_{sp + 1}", PrimitiveType.Cube, zone24.transform,
                    new Vector3(sx, vestY + 2.2f, 50.35f), new Vector3(2.1f, 3.2f, 0.14f), matAcousticPanel);
            }

            // Mesa técnica de grabación, grabadora analógica de carrete abierto, auriculares antiguos, amplificador y segundo monitor cooperativo
            CreatePrimitive("Mesa_Equipo_Grabacion", PrimitiveType.Cube, zone24.transform,
                new Vector3(36.8f, vestY + 0.44f, 56.0f), new Vector3(1.6f, 0.88f, 3.8f), matMahogany);
            CreatePrimitive("Silla_Tecnico_Sonido", PrimitiveType.Cube, zone24.transform,
                new Vector3(35.3f, vestY + 0.50f, 56.0f), new Vector3(0.75f, 1.0f, 0.75f), matLeather);

            // Altavoz monitor secundario en el extremo suroeste de la cabina (para que dos jugadores en cooperativo escuchen desde dos puntos)
            GameObject coopSpeaker = CreatePrimitive("Altavoz_Monitor_Cooperativo_Oeste", PrimitiveType.Cube, zone24.transform,
                new Vector3(31.2f, vestY + 1.15f, 51.4f), new Vector3(0.65f, 1.1f, 0.55f), matMetalArchive);
            DecoPiece("Altavoz_Cono_Bronce", PrimitiveType.Cylinder, coopSpeaker.transform,
                new Vector3(0.28f, 0.15f, 0f), new Vector3(0.35f, 0.04f, 0.35f), matGold).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            // Grabadora analógica de carrete abierto interactiva
            CreateReelToReelRecorder("Grabadora_Carrete_Abierto", zone24.transform,
                new Vector3(36.7f, vestY + 0.90f, 56.0f), Quaternion.Euler(0f, -90f, 0f),
                matMetalArchive, matIron, matGold, coopSpeaker.transform);

            // Pequeño amplificador de válvulas, auriculares antiguos y cajas de cintas etiquetadas
            CreatePrimitive("Amplificador_Valvulas", PrimitiveType.Cube, zone24.transform,
                new Vector3(36.8f, vestY + 1.08f, 54.6f), new Vector3(0.55f, 0.36f, 0.75f), matMetalArchive);
            DecoPiece("Auriculares_Antiguos", PrimitiveType.Cylinder, zone24.transform,
                new Vector3(36.4f, vestY + 0.93f, 57.1f), new Vector3(0.26f, 0.05f, 0.26f), matIron);
            DecoPiece("Cintas_Etiquetadas_Lote", PrimitiveType.Cube, zone24.transform,
                new Vector3(36.9f, vestY + 0.94f, 57.4f), new Vector3(0.35f, 0.12f, 0.35f), matCardboard);

            CreateOrnateSconce("Luz_Cabina_Grabacion", zone24.transform, new Vector3(34.5f, vestY + 3.4f, 50.45f), matGold, matLanternGlow, new Color(1f, 0.78f, 0.42f), 1.7f);

            // DOCUMENTOS DE LA ZONA 24 (CABINA DE GRABACIONES)
            CreateInteractiveWorldDoc("Doc_Cuaderno_Cabina_Grabacion", zone24.transform,
                new Vector3(36.4f, vestY + 0.92f, 55.1f), new Vector3(0.36f, 0.04f, 0.28f), matYellowedPaper,
                "doc_s5_booth_observations_notebook",
                "Cuaderno de Observaciones Técnicas — Sesiones de Grabación",
                "CABINA ACÚSTICA — REGISTRO DEL INGENIERO DE SONIDO\n\n" +
                "• Cinta 01 (10 de Octubre): Ensayo general con orquesta de cámara. Elena clava el tempo a la perfección.\n" +
                "• Cinta 02 (12 de Octubre): Grabación de la versión modificada por Isadora Vale. Se suprime el pasaje de violonchelo solista.\n" +
                "• 14 de Octubre (20:05 h): «Dejé el micrófono de ambiente abierto durante la discusión en el ensayo privado. Antes de que Dirección confiscara los carretes, escondí la cinta sin etiqueta detrás del amplificador de válvulas».\n\n" +
                "Pulsa [E] sobre la Grabadora de Carrete Abierto para reproducir y comparar las cintas (en cooperativo, un jugador puede escuchar junto a la mesa y otro junto al monitor de la esquina opuesta).");

            CreateInteractiveWorldDoc("Doc_Cinta_Sin_Etiqueta_Oculta", zone24.transform,
                new Vector3(37.35f, vestY + 0.94f, 54.45f), new Vector3(0.28f, 0.04f, 0.28f), matGold,
                "doc_s5_unlabeled_hidden_tape",
                "Cinta Magnética sin Etiqueta (Escondida detrás del Amplificador)",
                "Un carrete metálico sin rotular oculto en el hueco trasero entre el amplificador y el panel acústico.\n\n" +
                "En el borde interior del carrete hay una inscripción grabada con alfiler: «14-OCT — NO BORRAR».\n\n" +
                "Al montarla en la grabadora contigua ([E]), se escucha la secuencia musical ausente en las partituras oficiales —a la que solo le falta una nota grave de violonchelo—, seguida de pasos precipitados, una puerta cerrándose de golpe y una voz lejana que se interrumpe antes de pronunciar el nombre de quien estaba en el escenario.");

            // ====================================================================
            // ZONA 25: DEPÓSITO DE INSTRUMENTOS (Corredor X: 26..30, Z: 50..62 + Depósito X: 27..39, Z: 36..50)
            // ====================================================================
            GameObject zone25 = new GameObject("25_Instrument_Storage");
            zone25.transform.parent = rootEnv;
            float storageH = 5.0f;

            // 25A: Pasillo conector norte-sur (X: 26 a 30, Z: 50 a 62)
            CreatePrimitive("InstrumentPass_Floor", PrimitiveType.Cube, zone25.transform,
                new Vector3(28.0f, vestY - 0.25f, 56f), new Vector3(4f, 0.5f, 12f), matParquet);
            CreatePrimitive("InstrumentPass_Ceiling", PrimitiveType.Cube, zone25.transform,
                new Vector3(28.0f, vestY + storageH, 56f), new Vector3(4f, 0.5f, 12f), matWallIvory);
            // Cierre del tramo de 2m entre Z = 50 y Z = 52 en X = 26m y esquina X 26..27 en Z = 50m
            CreateWall("InstrumentPass_Wall_West_S", zone25.transform, new Vector3(26.0f, vestY + 2.5f, 51f), new Vector3(0.5f, storageH, 2f), matWallIvory);
            CreateWall("InstrumentPass_Wall_Corner", zone25.transform, new Vector3(26.5f, vestY + 2.5f, 50f), new Vector3(1.0f, storageH, 0.5f), matWallIvory);

            // 25B: Nave principal del Depósito de Instrumentos (X: 27 a 39, Z: 36 a 50)
            CreatePrimitive("Storage_Floor", PrimitiveType.Cube, zone25.transform,
                new Vector3(33f, vestY - 0.25f, 43f), new Vector3(12f, 0.5f, 14f), matParquet);
            CreatePrimitive("Storage_Ceiling", PrimitiveType.Cube, zone25.transform,
                new Vector3(33f, vestY + storageH, 43f), new Vector3(12f, 0.5f, 14f), matWallIvory);

            // Muro este (X = 39, Z: 36 a 50) con puerta en Z = 43.2m hacia la Galería de Servicio de Camerinos (Sección 6)
            // y tramo norte-oeste de X = 27 (Z: 46 a 50; Z: 36 a 46 ya lo cubre Zona 12 con su puerta a Camerinos en Z = 41m)
            CreateWall("Storage_Wall_East_S", zone25.transform, new Vector3(39f, vestY + 2.5f, 39.075f), new Vector3(0.5f, storageH, 6.15f), matWallIvory);
            CreateWall("Storage_Wall_East_N", zone25.transform, new Vector3(39f, vestY + 2.5f, 47.125f), new Vector3(0.5f, storageH, 5.75f), matWallIvory);
            CreateArchitecturalDoor("Door_InstrumentStorage_To_Backstage", zone25.transform, new Vector3(39f, vestY, 43.2f), Quaternion.Euler(0f, 90f, 0f),
                1.8f, 3.2f, storageH, 0.5f, false, "", "CAMERINOS Y PASILLO DE BACKSTAGE (SECCIÓN 6)", "", matMahogany, matGold, matWallIvory);
            CreateWall("Storage_Wall_West_NorthSeg", zone25.transform, new Vector3(27f, vestY + 2.5f, 48f), new Vector3(0.5f, storageH, 4f), matWallIvory);

            // Estanterías robustas, cajas de madera, soportes para instrumentos, estuches de violines, atriles plegados y percusión
            CreatePrimitive("Estanteria_Instrumentos_Este_1", PrimitiveType.Cube, zone25.transform,
                new Vector3(37.9f, vestY + 1.6f, 39.2f), new Vector3(1.1f, 3.2f, 3.6f), matWoodArchive);
            CreatePrimitive("Estanteria_Instrumentos_Este_2", PrimitiveType.Cube, zone25.transform,
                new Vector3(37.9f, vestY + 1.6f, 47.2f), new Vector3(1.1f, 3.2f, 3.6f), matWoodArchive);

            // Estuches de violines colocados directamente sobre las dos estanterías (sin flotar frente a la puerta de Z = 43.2m)
            float[] violinShelfZ = { 38.4f, 40.0f, 46.4f, 48.0f };
            for (int v = 0; v < violinShelfZ.Length; v++)
            {
                DecoPiece($"Estuche_Violin_{v + 1}", PrimitiveType.Cube, zone25.transform,
                    new Vector3(37.75f, vestY + 1.05f + (v % 2) * 0.9f, violinShelfZ[v]), new Vector3(0.36f, 0.18f, 0.82f), matLeather);
            }

            // Cajas de madera de transporte orquestal y timbal sinfónico de percusión
            CreatePrimitive("Caja_Orquesta_1", PrimitiveType.Cube, zone25.transform,
                new Vector3(29.2f, vestY + 0.45f, 47.2f), new Vector3(1.3f, 0.9f, 1.1f), matWoodArchive);
            CreatePrimitive("Timbal_Sinfonico_Percusion", PrimitiveType.Cylinder, zone25.transform,
                new Vector3(30.8f, vestY + 0.52f, 47.0f), new Vector3(0.95f, 0.52f, 0.95f), matGold);
            DecoPiece("Timbal_Parche", PrimitiveType.Cylinder, zone25.transform,
                new Vector3(30.8f, vestY + 1.05f, 47.0f), new Vector3(0.90f, 0.02f, 0.90f), matYellowedPaper);

            // Atriles plegados apilados
            for (int st = 0; st < 3; st++)
            {
                CreateMusicStand($"Atril_Plegado_Deposito_{st + 1}", zone25.transform,
                    new Vector3(28.5f + st * 0.45f, vestY, 44.5f), Quaternion.Euler(0f, 40f * st, 0f), matIron, matIron);
            }

            // Violonchelo cubierto por una tela oscura junto al rincón del fondo
            CreateDrapedCello("Violonchelo_Cubierto_TelaOscura", zone25.transform,
                new Vector3(35.8f, vestY, 47.8f), Quaternion.Euler(0f, -35f, 0f), matMahogany, matDarkCoverCloth, matGold);

            // Lámpara de techo débil que parpadea ocasionalmente + Anomalía acústica de la nota ausente (Do Natural) desde el rincón vacío
            GameObject weakLampObj = new GameObject("Lampara_Debil_Parpadeante_Deposito");
            weakLampObj.transform.parent = zone25.transform;
            weakLampObj.transform.position = new Vector3(33f, vestY + 4.4f, 43f);
            DecoPiece("WeakLamp_Cord", PrimitiveType.Cylinder, weakLampObj.transform, new Vector3(0f, 0.35f, 0f), new Vector3(0.03f, 0.35f, 0.03f), matIron);
            DecoPiece("WeakLamp_Bulb", PrimitiveType.Sphere, weakLampObj.transform, Vector3.zero, new Vector3(0.18f, 0.22f, 0.18f), matLanternGlow);
            Light weakFlickerLight = CreateSubtleAnomalyLight("Weak_Flicker_Light", weakLampObj.transform, Vector3.zero, new Color(1.0f, 0.78f, 0.44f), 1.65f);
            weakFlickerLight.range = 12f;

            GameObject emptyCornerAnomalyHost = new GameObject("Anomaly_EmptyCorner_CelloNote");
            emptyCornerAnomalyHost.transform.parent = zone25.transform;
            emptyCornerAnomalyHost.transform.position = new Vector3(36.8f, vestY + 1.0f, 48.5f);
            MusicArchiveAnomalyController anom25 = emptyCornerAnomalyHost.AddComponent<MusicArchiveAnomalyController>();
            anom25.Configure(MusicArchiveAnomalyType.EmptyCornerCelloNote, null, null, weakFlickerLight, null);
            EditorUtility.SetDirty(anom25);

            // DOCUMENTOS DE LA ZONA 25 (DEPÓSITO DE INSTRUMENTOS)
            CreateInteractiveWorldDoc("Doc_Etiquetas_Instrumentos_Contradictorias", zone25.transform,
                new Vector3(29.2f, vestY + 0.94f, 47.2f), new Vector3(0.38f, 0.04f, 0.28f), matGold,
                "doc_s5_contradictory_instrument_tags",
                "Etiquetas de Inventario de Instrumentos — Fechas Contradictorias",
                "ETIQUETAS DE DEPÓSITO ORQUESTAL — ACADEMIA VESPER\n\n" +
                "• Estuches de Violín I–IV: «Retirados del foso de orquesta el 18 de octubre tras la clausura oficial».\n" +
                "• Violonchelo Solista (Cubierto con lona negra): «Retirado por orden directa de Isadora Vale el 13 DE OCTUBRE DE 1924».\n\n" +
                "¡CONTRADICCIÓN!: Según los registros oficiales, el ensayo general con orquesta completa iba a celebrarse la noche del 14 de octubre. Sin embargo, el violonchelo principal que daba la señal de entrada a Elena ya había sido confiscado y escondido aquí el día anterior.");

            CreateInteractiveWorldDoc("Doc_Nota_Ausente_Deposito", zone25.transform,
                new Vector3(35.8f, vestY + 0.85f, 47.4f), new Vector3(0.34f, 0.05f, 0.24f), matSatinRibbon,
                "doc_s5_missing_recording_note_discovery",
                "Diapasón de Concierto y Partitura del Violonchelo Cubierto",
                "Prendido bajo la tela oscura que cubre el violonchelo hay un diapasón de acero calibrado y una hoja de anotaciones:\n\n" +
                "«Compás Final — Nota de Resolución: DO NATURAL (130.8 Hz)».\n\n" +
                "Al escuchar la nota profunda que resuena desde este rincón del depósito cuando parpadea la lámpara, compruebas que coincide exactamente con la nota que faltaba en la grabación de la Cabina de Grabaciones.\n\n" +
                "Al dorso se lee: «Con la cadencia completa reconstruida (Mi - Sol - Si - Do Natural - Mi), el mecanismo acústico de la Zona Técnica del Escenario responderá cuando lleguemos al Gran Teatro. Las puertas oeste y sur de este depósito conducen directamente de regreso a los Camerinos y Taquillas»");
        }

        private static GameObject CreateGrandPiano(string name, Transform parent, Vector3 pos, Quaternion rot, Material woodMat, Material goldMat, Material ivoryMat, Material ironMat, Material docMat)
        {
            GameObject piano = new GameObject(name);
            piano.transform.parent = parent;
            piano.transform.position = pos;
            piano.transform.rotation = rot;

            // Caja de resonancia de cola (cuerpo rectangular delantero + cola curva trasera)
            CreatePrimitive("Grand_Body_Front", PrimitiveType.Cube, piano.transform, new Vector3(0f, 0.82f, -0.35f), new Vector3(1.65f, 0.36f, 1.35f), woodMat);
            CreatePrimitive("Grand_Body_Tail", PrimitiveType.Cylinder, piano.transform, new Vector3(0f, 0.82f, 0.55f), new Vector3(1.55f, 0.18f, 1.85f), woodMat);
            DecoPiece("Grand_GoldHarp_Interior", PrimitiveType.Cylinder, piano.transform, new Vector3(0f, 0.99f, 0.35f), new Vector3(1.38f, 0.03f, 1.55f), goldMat);

            // Tapa superior entreabierta sostenida por varilla de latón
            GameObject lid = DecoPiece("Grand_Open_Lid", PrimitiveType.Cube, piano.transform, new Vector3(-0.15f, 1.32f, 0.15f), new Vector3(1.55f, 0.04f, 2.05f), woodMat);
            lid.transform.localRotation = Quaternion.Euler(0f, 0f, -24f);
            DecoPiece("Grand_Lid_PropStick", PrimitiveType.Cylinder, piano.transform, new Vector3(0.52f, 1.15f, -0.15f), new Vector3(0.03f, 0.28f, 0.03f), goldMat).transform.localRotation = Quaternion.Euler(0f, 0f, 18f);

            // Teclado de marfil y ébano en el frente sur (-Z)
            CreatePrimitive("Grand_Keybed", PrimitiveType.Cube, piano.transform, new Vector3(0f, 0.76f, -1.12f), new Vector3(1.52f, 0.14f, 0.38f), woodMat);
            DecoPiece("Grand_WhiteKeys", PrimitiveType.Cube, piano.transform, new Vector3(0f, 0.84f, -1.16f), new Vector3(1.40f, 0.04f, 0.24f), ivoryMat);
            DecoPiece("Grand_BlackKeys", PrimitiveType.Cube, piano.transform, new Vector3(0f, 0.86f, -1.08f), new Vector3(1.32f, 0.03f, 0.12f), ironMat);

            // Tres patas torneadas y lira de pedales
            CreatePrimitive("Grand_Leg_FL", PrimitiveType.Cylinder, piano.transform, new Vector3(-0.68f, 0.35f, -0.85f), new Vector3(0.12f, 0.35f, 0.12f), woodMat);
            CreatePrimitive("Grand_Leg_FR", PrimitiveType.Cylinder, piano.transform, new Vector3(0.68f, 0.35f, -0.85f), new Vector3(0.12f, 0.35f, 0.12f), woodMat);
            CreatePrimitive("Grand_Leg_Back", PrimitiveType.Cylinder, piano.transform, new Vector3(0f, 0.35f, 1.18f), new Vector3(0.12f, 0.35f, 0.12f), woodMat);
            DecoPiece("Grand_PedalLyre", PrimitiveType.Cube, piano.transform, new Vector3(0f, 0.38f, -0.82f), new Vector3(0.28f, 0.65f, 0.06f), goldMat);

            // Banqueta de concierto
            CreatePrimitive("Grand_Bench", PrimitiveType.Cube, piano.transform, new Vector3(0f, 0.26f, -1.68f), new Vector3(0.88f, 0.52f, 0.44f), woodMat);

            // Atril con compartimento secreto que se abre al reproducir la secuencia correcta
            GameObject musicDeskHinge = new GameObject("Grand_SecretDesk_Hinge");
            musicDeskHinge.transform.parent = piano.transform;
            musicDeskHinge.transform.localPosition = new Vector3(0f, 1.02f, -0.92f);
            DecoPiece("Grand_MusicDesk_Panel", PrimitiveType.Cube, musicDeskHinge.transform, new Vector3(0f, 0.16f, 0f), new Vector3(0.85f, 0.32f, 0.04f), woodMat);

            GameObject secretRevealObj = DecoPiece("Compartimento_Secreto_Nota_Dorada", PrimitiveType.Cube, piano.transform,
                new Vector3(0f, 1.04f, -0.82f), new Vector3(0.32f, 0.04f, 0.22f), docMat);

            // Interacción sobre el teclado del piano de cola
            GameObject interactKeyboard = CreatePrimitive("Teclado_Interactivo_Secuencia", PrimitiveType.Cube, piano.transform,
                new Vector3(0f, 0.85f, -1.12f), new Vector3(1.48f, 0.22f, 0.42f), ivoryMat);
            interactKeyboard.GetComponent<Renderer>().enabled = false;
            MusicArchiveAnomalyController seqInteract = interactKeyboard.AddComponent<MusicArchiveAnomalyController>();
            seqInteract.Configure(MusicArchiveAnomalyType.InteractiveGrandPianoSequence, musicDeskHinge.transform, null, null, secretRevealObj);
            EditorUtility.SetDirty(seqInteract);

            return piano;
        }

        private static void CreateReelToReelRecorder(string name, Transform parent, Vector3 pos, Quaternion rot, Material bodyMat, Material reelMat, Material goldMat, Transform secondarySpeaker)
        {
            GameObject rec = new GameObject(name);
            rec.transform.parent = parent;
            rec.transform.position = pos;
            rec.transform.rotation = rot;

            CreatePrimitive("Recorder_Chassis", PrimitiveType.Cube, rec.transform, new Vector3(0f, 0.26f, 0f), new Vector3(0.72f, 0.52f, 0.36f), bodyMat);
            DecoPiece("Recorder_VU_Meter_L", PrimitiveType.Cube, rec.transform, new Vector3(-0.16f, 0.14f, 0.19f), new Vector3(0.14f, 0.09f, 0.02f), goldMat);
            DecoPiece("Recorder_VU_Meter_R", PrimitiveType.Cube, rec.transform, new Vector3(0.16f, 0.14f, 0.19f), new Vector3(0.14f, 0.09f, 0.02f), goldMat);

            // Carretes magnéticos izquierdo y derecho
            GameObject reelSpinner = new GameObject("Reels_Spinner");
            reelSpinner.transform.parent = rec.transform;
            reelSpinner.transform.localPosition = new Vector3(0f, 0.36f, 0.20f);

            DecoPiece("Magnetic_Reel_Left", PrimitiveType.Cylinder, reelSpinner.transform, new Vector3(-0.18f, 0f, 0f), new Vector3(0.28f, 0.02f, 0.28f), reelMat).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            DecoPiece("Magnetic_Reel_Right", PrimitiveType.Cylinder, reelSpinner.transform, new Vector3(0.18f, 0f, 0f), new Vector3(0.28f, 0.02f, 0.28f), goldMat).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            MusicArchiveAnomalyController tapeInteract = rec.AddComponent<MusicArchiveAnomalyController>();
            tapeInteract.Configure(MusicArchiveAnomalyType.InteractiveReelToReelRecorder, reelSpinner.transform, secondarySpeaker, null, null);
            EditorUtility.SetDirty(tapeInteract);
        }

        private static void CreateDrapedCello(string name, Transform parent, Vector3 pos, Quaternion rot, Material woodMat, Material darkClothMat, Material goldMat)
        {
            GameObject cello = new GameObject(name);
            cello.transform.parent = parent;
            cello.transform.position = pos;
            cello.transform.rotation = rot;

            // Soporte de madera y silueta del violonchelo cubierto parcialmente por tela oscura
            CreatePrimitive("Cello_Stand_Base", PrimitiveType.Cube, cello.transform, new Vector3(0f, 0.06f, 0f), new Vector3(0.65f, 0.12f, 0.65f), woodMat);
            CreatePrimitive("Cello_Draped_Body", PrimitiveType.Capsule, cello.transform, new Vector3(0f, 0.82f, 0f), new Vector3(0.58f, 0.52f, 0.34f), darkClothMat);
            DecoPiece("Cello_Neck_Scroll", PrimitiveType.Cylinder, cello.transform, new Vector3(0f, 1.52f, 0f), new Vector3(0.08f, 0.32f, 0.08f), woodMat);
            DecoPiece("Cello_Endpin", PrimitiveType.Cylinder, cello.transform, new Vector3(0f, 0.20f, 0f), new Vector3(0.03f, 0.18f, 0.03f), goldMat);
        }

        private static void CreateMusicStand(string name, Transform parent, Vector3 pos, Quaternion rot, Material poleMat, Material deskMat)
        {
            GameObject stand = new GameObject(name);
            stand.transform.parent = parent;
            stand.transform.position = pos;
            stand.transform.rotation = rot;

            DecoPiece("Stand_Base", PrimitiveType.Cylinder, stand.transform, new Vector3(0f, 0.03f, 0f), new Vector3(0.36f, 0.03f, 0.36f), poleMat);
            DecoPiece("Stand_Pole", PrimitiveType.Cylinder, stand.transform, new Vector3(0f, 0.62f, 0f), new Vector3(0.04f, 0.60f, 0.04f), poleMat);
            DecoPiece("Stand_Bookplate", PrimitiveType.Cube, stand.transform, new Vector3(0f, 1.22f, 0f), new Vector3(0.52f, 0.34f, 0.03f), deskMat).transform.localRotation = Quaternion.Euler(28f, 0f, 0f);
        }

        // ====================================================================
        // CONSTRUCCIÓN DE LA SECCIÓN 6: CAMERINOS Y BACKSTAGE — ZONA RESTRINGIDA (Zonas 26 a 30)
        // ====================================================================
        private static void BuildSection6BackstageAndDressingRooms(
            Transform rootEnv, float vestY,
            Material matParquet, Material matCarpet, Material matWallIvory, Material matMahogany,
            Material matGold, Material matIron, Material matClothCover, Material matVelvetDrapes,
            Material matLanternGlow, Material matColdFluorescent, Material matGreenVelvet,
            Material matLeather, Material matWoodArchive, Material matMetalArchive, Material matCardboard,
            Material matStoneWall, Material matRustedLocker, Material matMirrorGlass, Material matPorcelain,
            Material matTutuWhite, Material matTutuBlack, Material matSatinRibbon, Material matShadowFigure,
            Material matPortraitElena, Material matPhotoTroupe1923, Material matCobble)
        {
            Material matBackstageDarkWall = GetOrCreateBasicMaterial("Mat_S6_BackstageDarkWall", new Color(0.16f, 0.14f, 0.15f), 0.18f, 0.02f);
            Material matCrackedMirror = GetOrCreateBasicMaterial("Mat_S6_CrackedMirror", new Color(0.68f, 0.74f, 0.78f), 0.92f, 0.80f);
            Material matPaintedLake = GetOrCreateBasicMaterial("Mat_S6_PaintedLakeBackdrop", new Color(0.12f, 0.19f, 0.28f), 0.22f, 0.0f);
            Material matHempRope = GetOrCreateBasicMaterial("Mat_S6_HempRope", new Color(0.58f, 0.48f, 0.34f), 0.12f, 0.0f);
            Material matBlueprintPaper = GetOrCreateBasicMaterial("Mat_S6_BlueprintYellowed", new Color(0.84f, 0.78f, 0.58f), 0.20f, 0.0f);

            // ====================================================================
            // CIRCULACIÓN SECUNDARIA DE CAMERINOS + CONEXIÓN AL DEPÓSITO DE INSTRUMENTOS Y SALIDA DE SERVICIO EXTERIOR
            // - Corredor Este (X: 39 a 45, Z: 36 a 78) + Callejón Exterior de Servicio (X: 39 a 45, Z: 12 a 36)
            // - Vestíbulo de Camerinos (X: 26 a 45, Z: 78 a 83) + Paso al Backstage (X: 41 a 45, Z: 83 a 92)
            // ====================================================================
            GameObject dressingHall = new GameObject("26_27_Dressing_Secondary_Circulation");
            dressingHall.transform.parent = rootEnv;
            float hallH = 5.0f;

            // 1. Corredor lateral este que regresa al Depósito de Instrumentos (Zona 25) y a la Puerta de Servicio Exterior (X: 39..45, Z: 36..78)
            CreatePrimitive("EastServiceHall_Floor", PrimitiveType.Cube, dressingHall.transform,
                new Vector3(42f, vestY - 0.25f, 57f), new Vector3(6f, 0.5f, 42f), matParquet);
            CreatePrimitive("EastServiceHall_Carpet", PrimitiveType.Cube, dressingHall.transform,
                new Vector3(42f, vestY + 0.02f, 57f), new Vector3(2.6f, 0.04f, 38f), matCarpet);
            CreatePrimitive("EastServiceHall_Ceiling", PrimitiveType.Cube, dressingHall.transform,
                new Vector3(42f, vestY + hallH, 57f), new Vector3(6f, 0.5f, 42f), matWallIvory);

            // Muro este del Corredor de Mantenimiento (X = 45, Z: 36 a 78) dividido en Z = 58.0m para conectar con el Jardín de Invierno (Zona 40)
            CreateWall("EastServiceHall_Wall_East_S", dressingHall.transform,
                new Vector3(45f, vestY + 2.5f, 46.475f), new Vector3(0.5f, hallH, 20.95f), matWallIvory);
            CreateWall("EastServiceHall_Wall_East_N", dressingHall.transform,
                new Vector3(45f, vestY + 2.5f, 68.525f), new Vector3(0.5f, hallH, 18.95f), matWallIvory);
            CreateArchitecturalDoor("Door_MaintenanceCorridor_To_WinterGarden", dressingHall.transform,
                new Vector3(45f, vestY, 58f), Quaternion.Euler(0f, 90f, 0f),
                1.8f, 3.2f, hallH, 0.5f, false, "",
                "JARDÍN DE INVIERNO — ACCESO DESDE CORREDOR DE MANTENIMIENTO (ZONA 40)", "", matMahogany, matGold, matWallIvory);

            // Muro sur (Z = 36, X: 39 a 45) con Puerta de Servicio Exterior hacia el exterior de la academia (X = 42m)
            CreateWall("EastServiceHall_Wall_South_W", dressingHall.transform,
                new Vector3(39.975f, vestY + 2.5f, 36f), new Vector3(1.95f, hallH, 0.5f), matWallIvory);
            CreateWall("EastServiceHall_Wall_South_E", dressingHall.transform,
                new Vector3(44.025f, vestY + 2.5f, 36f), new Vector3(1.95f, hallH, 0.5f), matWallIvory);
            CreateArchitecturalDoor("Door_Backstage_Exterior_Service", dressingHall.transform,
                new Vector3(42f, vestY, 36f), Quaternion.identity,
                1.8f, 3.2f, hallH, 0.5f, false, "", "SALIDA DE SERVICIO EXTERIOR — PATIO DE LA ACADEMIA", "", matMahogany, matGold, matWallIvory);

            // Callejón y rampa exterior de servicio (X: 39..45, Z: -12..36) que conecta la puerta de servicio exterior con el Patio de Entrada (Sección 1)
            CreatePrimitive("Exterior_Service_Platform", PrimitiveType.Cube, dressingHall.transform,
                new Vector3(42f, vestY - 0.25f, 33f), new Vector3(6f, 0.5f, 6f), matCobble);
            GameObject extRamp = CreatePrimitive("Exterior_Service_Ramp", PrimitiveType.Cube, dressingHall.transform,
                new Vector3(42f, 1.15f, 22f), new Vector3(5.8f, 0.3f, 16.5f), matCobble);
            extRamp.transform.localRotation = Quaternion.Euler(-8.5f, 0f, 0f);
            CreatePrimitive("Exterior_Service_Alley_Ground", PrimitiveType.Cube, dressingHall.transform,
                new Vector3(42f, -0.25f, 1f), new Vector3(6f, 0.5f, 26f), matCobble);
            CreatePrimitive("Exterior_Service_Court_Link", PrimitiveType.Cube, dressingHall.transform,
                new Vector3(29.5f, -0.25f, -10f), new Vector3(31f, 0.5f, 6f), matCobble);

            // Basamento de piedra bajo el Ala Este (Lavabos y Taquillas, Y: 0 a 2.4m) y muros del callejón exterior para que ningún volumen flote
            CreateWall("EastWing_Foundation_Plinth_Lockers", dressingHall.transform,
                new Vector3(33f, 1.2f, 24f), new Vector3(12f, 2.4f, 24f), matStoneWall);
            CreateWall("EastWing_Foundation_Plinth_Washrooms", dressingHall.transform,
                new Vector3(19f, 1.2f, 6f), new Vector3(16f, 2.4f, 12f), matStoneWall);
            CreateWall("Exterior_Service_WestWall_Lower", dressingHall.transform,
                new Vector3(39f, 2.4f, 2.5f), new Vector3(0.5f, 4.8f, 19f), matStoneWall);
            CreateWall("Exterior_Service_Court_Wall_South", dressingHall.transform,
                new Vector3(26.75f, 2.4f, -13f), new Vector3(25.5f, 4.8f, 0.5f), matStoneWall);
            CreateWall("Exterior_Service_Court_Wall_North", dressingHall.transform,
                new Vector3(26.75f, 2.4f, -7f), new Vector3(25.5f, 4.8f, 0.5f), matStoneWall);

            // Muro exterior este del callejón (Z: -12 a 22; en Z >= 22 comienza el muro oeste del Jardín de Invierno)
            CreateWall("Exterior_Service_OuterWall_E", dressingHall.transform,
                new Vector3(45f, 2.2f, 5.0f), new Vector3(0.5f, 4.8f, 34f), matStoneWall);
            CreateOrnateLantern("Farol_Salida_Servicio_Exterior", dressingHall.transform,
                new Vector3(43.8f, vestY + 3.2f, 35.2f), matIron, matLanternGlow, new Color(1f, 0.78f, 0.45f), 2.1f);

            // 2. Vestíbulo de Circulación Secundaria frente a los Camerinos de Mara y Elena (X: 26..45, Z: 78..83)
            // En X = 45.0, Z = 80.5 se abre la Entrada Principal de Madera Oscura con Paneles de Cristal al Jardín de Invierno
            CreatePrimitive("DressingVestibule_Floor", PrimitiveType.Cube, dressingHall.transform,
                new Vector3(35.5f, vestY - 0.25f, 80.5f), new Vector3(19f, 0.5f, 5f), matParquet);
            CreatePrimitive("DressingVestibule_Carpet", PrimitiveType.Cube, dressingHall.transform,
                new Vector3(35.5f, vestY + 0.02f, 80.5f), new Vector3(16f, 0.04f, 2.4f), matCarpet);
            CreatePrimitive("DressingVestibule_Ceiling", PrimitiveType.Cube, dressingHall.transform,
                new Vector3(35.5f, vestY + hallH, 80.5f), new Vector3(19f, 0.5f, 5f), matWallIvory);

            // 3. Conector Norte-Sur desde el Vestíbulo de Camerinos hasta el Pasillo Detrás del Escenario (X: 41..45, Z: 83..92)
            CreatePrimitive("DressingToBackstage_Floor", PrimitiveType.Cube, dressingHall.transform,
                new Vector3(43f, vestY - 0.25f, 87.5f), new Vector3(4f, 0.5f, 9f), matParquet);
            CreatePrimitive("DressingToBackstage_Carpet", PrimitiveType.Cube, dressingHall.transform,
                new Vector3(43f, vestY + 0.02f, 87.5f), new Vector3(2.2f, 0.04f, 8.5f), matCarpet);
            CreatePrimitive("DressingToBackstage_Ceiling", PrimitiveType.Cube, dressingHall.transform,
                new Vector3(43f, vestY + hallH, 87.5f), new Vector3(4f, 0.5f, 9f), matWallIvory);
            CreateWall("DressingToBackstage_Wall_East", dressingHall.transform,
                new Vector3(45f, vestY + 2.5f, 87.5f), new Vector3(0.5f, hallH, 9f), matWallIvory);

            CreateOrnateSconce("Luz_Circulacion_Camerinos_1", dressingHall.transform, new Vector3(33.5f, vestY + 3.1f, 78.35f), matGold, matLanternGlow, new Color(1f, 0.82f, 0.52f), 2.0f);
            CreateOrnateSconce("Luz_Circulacion_Camerinos_2", dressingHall.transform, new Vector3(44.6f, vestY + 3.1f, 56.0f), matGold, matLanternGlow, new Color(1f, 0.82f, 0.52f), 1.9f);
            CreateOrnateSconce("Luz_Circulacion_Camerinos_3", dressingHall.transform, new Vector3(44.6f, vestY + 3.1f, 43.2f), matGold, matLanternGlow, new Color(1f, 0.82f, 0.52f), 1.9f);

            // ====================================================================
            // ZONA 26: CAMERINO DE MARA (X: 26 a 33.5, Z: 83 a 92, Y: 2.4m, Altura: 4.8m)
            // ====================================================================
            GameObject zone26 = new GameObject("26_Mara_Dressing_Room");
            zone26.transform.parent = rootEnv;
            float dressRoomH = 4.8f;

            CreatePrimitive("MaraRoom_Floor", PrimitiveType.Cube, zone26.transform,
                new Vector3(29.75f, vestY - 0.25f, 87.5f), new Vector3(7.5f, 0.5f, 9f), matParquet);
            CreatePrimitive("MaraRoom_Rug", PrimitiveType.Cube, zone26.transform,
                new Vector3(29.75f, vestY + 0.02f, 87.5f), new Vector3(4.8f, 0.04f, 5.6f), matCarpet);
            CreatePrimitive("MaraRoom_Ceiling", PrimitiveType.Cube, zone26.transform,
                new Vector3(29.75f, vestY + dressRoomH, 87.5f), new Vector3(7.5f, 0.5f, 9f), matWallIvory);

            // Muro sur (Z = 83, X: 26 a 33.5) con puerta independiente en X = 29.75m
            CreateWall("MaraRoom_Wall_South_W", zone26.transform, new Vector3(27.35f, vestY + 2.4f, 83f), new Vector3(2.70f, dressRoomH, 0.5f), matWallIvory);
            CreateWall("MaraRoom_Wall_South_E", zone26.transform, new Vector3(32.15f, vestY + 2.4f, 83f), new Vector3(2.70f, dressRoomH, 0.5f), matWallIvory);
            CreateArchitecturalDoor("Door_DressingRoom_Mara", zone26.transform, new Vector3(29.75f, vestY, 83f), Quaternion.identity,
                1.8f, 3.2f, dressRoomH, 0.5f, false, "", "CAMERINO I — MARA BELL", "", matMahogany, matGold, matWallIvory);

            // Muro divisorio entre el Camerino de Mara y el Camerino de Elena (X = 33.5, Z: 83 a 92)
            CreateWall("Mara_Elena_Partition_Wall", zone26.transform, new Vector3(33.5f, vestY + 2.4f, 87.5f), new Vector3(0.5f, dressRoomH, 9f), matWallIvory);
            // Muro norte (Z = 92, X: 26 a 33.5) compartido con el Pasillo Detrás del Escenario
            CreateWall("MaraRoom_Wall_North", zone26.transform, new Vector3(29.75f, vestY + 2.4f, 92f), new Vector3(7.5f, dressRoomH, 0.5f), matWallIvory);

            // Tocador de madera oscura elegante con espejo envejecido y bombillas cálidas alrededor del marco
            CreatePrimitive("Tocador_Madera_Mara", PrimitiveType.Cube, zone26.transform,
                new Vector3(29.75f, vestY + 0.44f, 91.1f), new Vector3(2.6f, 0.88f, 0.85f), matMahogany);
            DecoPiece("Marco_Espejo_Mara", PrimitiveType.Cube, zone26.transform,
                new Vector3(29.75f, vestY + 1.85f, 91.55f), new Vector3(1.85f, 1.75f, 0.10f), matGold);
            DecoPiece("Cristal_Espejo_Mara", PrimitiveType.Cube, zone26.transform,
                new Vector3(29.75f, vestY + 1.85f, 91.50f), new Vector3(1.62f, 1.52f, 0.04f), matMirrorGlass);

            // Bombillas cálidas alrededor del marco del espejo de Mara
            for (int b = 0; b < 6; b++)
            {
                float bx = 28.90f + (b % 3) * 0.85f;
                float by = vestY + (b < 3 ? 2.65f : 1.15f);
                DecoPiece($"Bombilla_Tocador_Mara_{b + 1}", PrimitiveType.Sphere, zone26.transform,
                    new Vector3(bx, by, 91.45f), new Vector3(0.09f, 0.09f, 0.09f), matLanternGlow);
            }
            Light maraVanityLight = CreateSubtleAnomalyLight("Luz_Calida_Tocador_Mara", zone26.transform,
                new Vector3(29.75f, vestY + 2.1f, 90.6f), new Color(1.0f, 0.84f, 0.54f), 2.3f);
            maraVanityLight.range = 8.5f;

            // Silla tapizada con prendas abandonadas sobre el respaldo y el asiento
            CreatePrimitive("Silla_Tapizada_Mara", PrimitiveType.Cube, zone26.transform,
                new Vector3(29.75f, vestY + 0.48f, 89.8f), new Vector3(0.75f, 0.96f, 0.75f), matVelvetDrapes);
            DecoPiece("Prenda_Abandonada_Silla_Mara", PrimitiveType.Cube, zone26.transform,
                new Vector3(29.85f, vestY + 0.65f, 89.75f), new Vector3(0.55f, 0.18f, 0.45f), matTutuWhite).transform.localRotation = Quaternion.Euler(12f, 24f, -8f);

            // Perchero ordenado con prendas de ballet cuidadosamente colgadas y pequeño armario personal
            CreatePrimitive("Armario_Personal_Mara", PrimitiveType.Cube, zone26.transform,
                new Vector3(26.85f, vestY + 1.45f, 88.5f), new Vector3(0.95f, 2.9f, 1.8f), matMahogany);
            DecoPiece("Perchero_Barra_Mara", PrimitiveType.Cylinder, zone26.transform,
                new Vector3(32.5f, vestY + 1.55f, 88.2f), new Vector3(0.05f, 1.1f, 0.05f), matGold).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            DecoPiece("Tutu_Colgado_Mara_1", PrimitiveType.Cylinder, zone26.transform,
                new Vector3(32.5f, vestY + 1.15f, 87.7f), new Vector3(0.65f, 0.25f, 0.65f), matTutuWhite);
            DecoPiece("Tutu_Colgado_Mara_2", PrimitiveType.Cylinder, zone26.transform,
                new Vector3(32.5f, vestY + 1.15f, 88.7f), new Vector3(0.65f, 0.25f, 0.65f), matSatinRibbon);

            // Objetos sobre el tocador de Mara: maquillaje, horquillas, caja de accesorios y fotografía con Elena
            DecoPiece("Caja_Accesorios_Danza_Mara", PrimitiveType.Cube, zone26.transform,
                new Vector3(28.85f, vestY + 0.93f, 91.05f), new Vector3(0.32f, 0.12f, 0.24f), matGold);
            DecoPiece("Maquillaje_Y_Horquillas_Mara", PrimitiveType.Cylinder, zone26.transform,
                new Vector3(29.35f, vestY + 0.92f, 90.95f), new Vector3(0.14f, 0.06f, 0.14f), matPorcelain);
            CreateFramedArtPortrait("Foto_Mara_Junto_A_Elena", zone26.transform,
                new Vector3(26.35f, vestY + 2.3f, 85.4f), new Vector3(0.10f, 1.1f, 1.4f), matGold, matPhotoTroupe1923,
                "Fotografía de Mara Bell y Elena Vesper (Otoño de 1922, antes de su rivalidad)");

            // ANOMALÍA PSICOLÓGICA ZONA 26: El reflejo del espejo muestra brevemente una postura de ballet que nadie está realizando
            GameObject maraMirrorAnomalyHost = new GameObject("Anomaly_MaraMirror_DelayedPose");
            maraMirrorAnomalyHost.transform.parent = zone26.transform;
            maraMirrorAnomalyHost.transform.position = new Vector3(29.75f, vestY + 1.2f, 91.42f);
            GameObject maraMirrorPoseFig = CreateBalletSilhouetteFigure("Reflejo_Postura_Desfasada_Mara", maraMirrorAnomalyHost.transform,
                new Vector3(0f, -0.45f, 0f), matShadowFigure, true);
            BackstageAnomalyController anom26 = maraMirrorAnomalyHost.AddComponent<BackstageAnomalyController>();
            anom26.Configure(BackstageAnomalyType.MaraMirrorDelayedPose, maraMirrorPoseFig.transform);
            EditorUtility.SetDirty(anom26);

            // DOCUMENTOS E INTERACCIÓN NARRATIVA DE LA ZONA 26 (CAMERINO DE MARA)
            CreateInteractiveWorldDoc("Doc_Carta_Oculta_Cajon_Mara", zone26.transform,
                new Vector3(30.45f, vestY + 0.88f, 90.75f), new Vector3(0.36f, 0.04f, 0.26f), matBlueprintPaper,
                "doc_s6_mara_hidden_drawer_letter",
                "Carta Parcialmente Escondida entre los Cajones del Tocador de Mara",
                "CONFIDENCIAL — DESPACHO DE DIRECCIÓN (13 DE OCTUBRE DE 1924)\n\n" +
                "«Mara:\n" +
                "Mañana durante el tercer acto de 'El Lago de las Sombras', el telón intermedio bajará en el compás 48 cuando suene el acorde modificado. Tú entrarás por el lateral derecho para asumir el cierre de la obra.\n\n" +
                "Elena no debe saber que el decorado del fondo ha sido bloqueado ni que el jefe de tramoya recibió instrucciones de accionar el contrapeso antes de la coda.\n" +
                "— Isadora Vale»\n\n" +
                "Al dorso, Mara escribió con trazo tembloroso: «Solo acepté ocupar su puesto en el acto final, nunca que la encerraran detrás del escenario. ¿Qué hicieron con ella cuando bajó el telón?»");

            CreateInteractiveWorldDoc("Doc_Notas_Ensayo_Y_Foto_Mara", zone26.transform,
                new Vector3(29.75f, vestY + 0.92f, 91.05f), new Vector3(0.38f, 0.04f, 0.28f), matGold,
                "doc_s6_mara_rehearsal_notes",
                "Fotografías Antiguas y Notas de Ensayo de Mara Bell",
                "Sobre el tocador descansan horquillas, polveras y una fotografía de 1922 donde Elena y Mara posan abrazadas tras su primer recital juntas.\n\n" +
                "Junto a la foto hay una libreta de ensayo abierta el 14 de octubre a las 19:50 h:\n" +
                "«Escuché a Elena discutir con el técnico de sonido y después encerrarse en su camerino (al lado del mío) con el traje del Cisne Negro. Dijo que había descubierto lo que ocultaban en el Almacén de Decorados y que iba a usar la música original para desenmascarar a Isadora en pleno escenario».");

            // ====================================================================
            // ZONA 27: CAMERINO DE ELENA (X: 33.5 a 41, Z: 83 a 92, Y: 2.4m, Altura: 4.8m)
            // ====================================================================
            GameObject zone27 = new GameObject("27_Elena_Dressing_Room");
            zone27.transform.parent = rootEnv;

            CreatePrimitive("ElenaDressRoom_Floor", PrimitiveType.Cube, zone27.transform,
                new Vector3(37.25f, vestY - 0.25f, 87.5f), new Vector3(7.5f, 0.5f, 9f), matParquet);
            CreatePrimitive("ElenaDressRoom_Rug", PrimitiveType.Cube, zone27.transform,
                new Vector3(37.25f, vestY + 0.02f, 87.5f), new Vector3(4.8f, 0.04f, 5.6f), matCarpet);
            CreatePrimitive("ElenaDressRoom_Ceiling", PrimitiveType.Cube, zone27.transform,
                new Vector3(37.25f, vestY + dressRoomH, 87.5f), new Vector3(7.5f, 0.5f, 9f), matWallIvory);

            // Muro sur (Z = 83, X: 33.5 a 41) con puerta independiente en X = 37.25m
            CreateWall("ElenaDressRoom_Wall_South_W", zone27.transform, new Vector3(34.85f, vestY + 2.4f, 83f), new Vector3(2.70f, dressRoomH, 0.5f), matWallIvory);
            CreateWall("ElenaDressRoom_Wall_South_E", zone27.transform, new Vector3(39.65f, vestY + 2.4f, 83f), new Vector3(2.70f, dressRoomH, 0.5f), matWallIvory);
            CreateArchitecturalDoor("Door_DressingRoom_Elena", zone27.transform, new Vector3(37.25f, vestY, 83f), Quaternion.identity,
                1.8f, 3.2f, dressRoomH, 0.5f, false, "", "CAMERINO PRINCIPAL — ELENA VESPER", "", matMahogany, matGold, matWallIvory);

            // Muro este (X = 41, Z: 83 a 92) y muro norte (Z = 92, X: 33.5 a 41)
            CreateWall("ElenaDressRoom_Wall_East", zone27.transform, new Vector3(41f, vestY + 2.4f, 87.5f), new Vector3(0.5f, dressRoomH, 9f), matWallIvory);
            CreateWall("ElenaDressRoom_Wall_North", zone27.transform, new Vector3(37.25f, vestY + 2.4f, 92f), new Vector3(7.5f, dressRoomH, 0.5f), matWallIvory);

            // Tocador antiguo con cajones abiertos y espejo alto con pequeñas grietas
            CreatePrimitive("Tocador_Antiguo_Camerino_Elena", PrimitiveType.Cube, zone27.transform,
                new Vector3(37.25f, vestY + 0.44f, 91.1f), new Vector3(2.6f, 0.88f, 0.85f), matMahogany);
            CreatePrimitive("Cajon_Abierto_Elena_1", PrimitiveType.Cube, zone27.transform,
                new Vector3(36.35f, vestY + 0.68f, 90.52f), new Vector3(0.65f, 0.18f, 0.45f), matWoodArchive);
            CreatePrimitive("Cajon_Abierto_Elena_2", PrimitiveType.Cube, zone27.transform,
                new Vector3(38.15f, vestY + 0.52f, 90.48f), new Vector3(0.65f, 0.18f, 0.48f), matWoodArchive);

            DecoPiece("Marco_Espejo_Alto_Elena", PrimitiveType.Cube, zone27.transform,
                new Vector3(37.25f, vestY + 2.05f, 91.55f), new Vector3(1.65f, 2.25f, 0.10f), matGold);
            DecoPiece("Cristal_Espejo_Agrietado_Elena", PrimitiveType.Cube, zone27.transform,
                new Vector3(37.25f, vestY + 2.05f, 91.50f), new Vector3(1.45f, 2.05f, 0.04f), matCrackedMirror);
            // Grietas diagonales en el espejo alto de Elena
            DecoPiece("Grieta_Espejo_Elena_1", PrimitiveType.Cube, zone27.transform,
                new Vector3(37.05f, vestY + 2.25f, 91.47f), new Vector3(0.75f, 0.015f, 0.02f), matIron).transform.localRotation = Quaternion.Euler(0f, 0f, 34f);
            DecoPiece("Grieta_Espejo_Elena_2", PrimitiveType.Cube, zone27.transform,
                new Vector3(37.42f, vestY + 1.85f, 91.47f), new Vector3(0.55f, 0.012f, 0.02f), matIron).transform.localRotation = Quaternion.Euler(0f, 0f, -52f);

            // Armario parcialmente bloqueado por prendas y perchero desordenado
            CreatePrimitive("Armario_Bloqueado_Elena", PrimitiveType.Cube, zone27.transform,
                new Vector3(34.45f, vestY + 1.45f, 88.8f), new Vector3(0.95f, 2.9f, 1.9f), matMahogany).transform.localRotation = Quaternion.Euler(0f, 6f, 0f);
            CreatePrimitive("Prendas_Bloqueando_Armario", PrimitiveType.Cube, zone27.transform,
                new Vector3(35.05f, vestY + 0.55f, 88.6f), new Vector3(0.65f, 1.1f, 1.2f), matClothCover);
            DecoPiece("Perchero_Desordenado_Elena", PrimitiveType.Cylinder, zone27.transform,
                new Vector3(39.9f, vestY + 1.50f, 89.2f), new Vector3(0.05f, 1.1f, 0.05f), matIron).transform.localRotation = Quaternion.Euler(84f, 0f, 0f);

            // ELEMENTO VISUAL CENTRAL ZONA 27: Maniquí con el Vestuario del Cisne Negro
            GameObject blackSwanMannequin = CreateStageMannequin("Maniqui_Vestuario_Cisne_Negro", zone27.transform,
                new Vector3(37.25f, vestY, 87.6f), Quaternion.Euler(0f, 180f, 0f), matMahogany, matGold, matTutuBlack, false);

            // Zapatillas de punta antiguas al pie del maniquí
            DecoPiece("Zapatilla_Punta_Antigua_1", PrimitiveType.Capsule, zone27.transform,
                new Vector3(36.75f, vestY + 0.05f, 87.3f), new Vector3(0.10f, 0.14f, 0.08f), matSatinRibbon).transform.localRotation = Quaternion.Euler(90f, 22f, 0f);
            DecoPiece("Zapatilla_Punta_Antigua_2", PrimitiveType.Capsule, zone27.transform,
                new Vector3(36.95f, vestY + 0.05f, 87.15f), new Vector3(0.10f, 0.14f, 0.08f), matSatinRibbon).transform.localRotation = Quaternion.Euler(90f, -40f, 0f);

            Light elenaMoodLight = CreateSubtleAnomalyLight("Luz_Inquietante_Camerino_Elena", zone27.transform,
                new Vector3(37.25f, vestY + 2.9f, 87.6f), new Color(0.92f, 0.72f, 0.48f), 1.85f);
            elenaMoodLight.range = 8.0f;

            // Interacción principal en el traje del Cisne Negro para reconstruir los últimos preparativos de Elena
            GameObject hiddenBodiceNoteVisual = DecoPiece("Nota_Cosida_Corpiño_CisneNegro", PrimitiveType.Cube, blackSwanMannequin.transform,
                new Vector3(0f, 1.32f, 0.18f), new Vector3(0.16f, 0.12f, 0.03f), matGold);
            BackstageAnomalyController blackSwanInteract = blackSwanMannequin.AddComponent<BackstageAnomalyController>();
            blackSwanInteract.Configure(BackstageAnomalyType.InteractiveElenaBlackSwanSuit, null, null, null, hiddenBodiceNoteVisual, null);
            EditorUtility.SetDirty(blackSwanInteract);

            // ANOMALÍA PSICOLÓGICA ZONA 27: Silueta detrás del jugador en el espejo agrietado + cambio sutil del traje al salir y regresar
            GameObject elenaMirrorAnomalyHost = new GameObject("Anomaly_ElenaMirror_And_CostumeShift");
            elenaMirrorAnomalyHost.transform.parent = zone27.transform;
            elenaMirrorAnomalyHost.transform.position = new Vector3(37.25f, vestY + 1.2f, 91.42f);
            GameObject silhouetteBehindPlayer = CreateBalletSilhouetteFigure("Silueta_Espejo_Detras_Jugador", elenaMirrorAnomalyHost.transform,
                new Vector3(0.18f, -0.45f, 0f), matShadowFigure, false);
            BackstageAnomalyController anom27 = elenaMirrorAnomalyHost.AddComponent<BackstageAnomalyController>();
            anom27.Configure(BackstageAnomalyType.ElenaMirrorAndCostumeShift, silhouetteBehindPlayer.transform, blackSwanMannequin.transform);
            EditorUtility.SetDirty(anom27);

            // DOCUMENTOS DE LA ZONA 27 (CAMERINO DE ELENA)
            CreateInteractiveWorldDoc("Doc_Partitura_Doblada_Camerino_Elena", zone27.transform,
                new Vector3(36.45f, vestY + 0.92f, 91.05f), new Vector3(0.36f, 0.04f, 0.26f), matBlueprintPaper,
                "doc_s6_elena_folded_score",
                "Partitura Doblada con Anotaciones de Escena y Tramoya",
                "Un pliego doblado en cuatro partes que combina el compás 48 de la versión original de «El Lago de las Sombras» con un esquema técnico del escenario:\n\n" +
                "• Contradicción con Administración: Mientras el informe oficial afirma que Elena abandonó la academia antes de la función, esta hoja tiene anotaciones de última hora hechas con lápiz de camerino a las **20:10 h**.\n" +
                "• Nota de Elena: «Cuando el violonchelo toque el Do Natural de la partitura original, el Contrapeso III de la Sala de Poleas debe estar arriba para mantener abierto el paso central del decorado del lago».\n\n" +
                "Examina también el **Vestuario del Cisne Negro ([E])** en el centro del camerino para reconstruir sus preparativos.");

            CreateInteractiveWorldDoc("Doc_Paginas_Arrancadas_Diario_Elena", zone27.transform,
                new Vector3(38.10f, vestY + 0.92f, 91.05f), new Vector3(0.34f, 0.04f, 0.24f), matSatinRibbon,
                "doc_s6_elena_torn_diary_pages",
                "Páginas Arrancadas del Diario de Elena Vesper (14 de Octubre)",
                "“Si alguien encuentra estas páginas en mi camerino es porque no me dejaron terminar el último acto.\n\n" +
                "Isadora retiró ayer el decorado original al Almacén de Decorados y ordenó modificar las cuerdas de la Sala de Mecanismos y Poleas. No quiere sustituirme únicamente en el cartel: quiere que el telón de contrapeso caiga antes de que yo pueda cruzar hacia el proscenio.\n\n" +
                "He dejado la prueba cosida dentro del traje del Cisne Negro.”");

            // ====================================================================
            // ZONA 28: PASILLO DETRÁS DEL ESCENARIO (X: -12 a 45, Z: 92 a 98 + Conector X: -4 a 8, Z: 84 a 92)
            // ====================================================================
            GameObject zone28 = new GameObject("28_Backstage_Corridor");
            zone28.transform.parent = rootEnv;
            float backstageH = 5.4f;

            // Eje principal Este-Oeste detrás del escenario (X: -12 a 45, Z: 92 a 98)
            CreatePrimitive("Backstage_Main_Floor_Wood", PrimitiveType.Cube, zone28.transform,
                new Vector3(16.5f, vestY - 0.25f, 95f), new Vector3(57f, 0.5f, 6f), matParquet);
            // Zonas de suelo técnico resistente en las áreas de carga y trabajo
            DecoPiece("Backstage_WorkFloor_Slab_W", PrimitiveType.Cube, zone28.transform,
                new Vector3(-4.0f, vestY + 0.01f, 95f), new Vector3(14f, 0.02f, 4.8f), matStoneWall);
            DecoPiece("Backstage_WorkFloor_Slab_E", PrimitiveType.Cube, zone28.transform,
                new Vector3(22.0f, vestY + 0.01f, 95f), new Vector3(14f, 0.02f, 4.8f), matStoneWall);
            CreatePrimitive("Backstage_Main_Ceiling", PrimitiveType.Cube, zone28.transform,
                new Vector3(16.5f, vestY + backstageH, 95f), new Vector3(57f, 0.5f, 6f), matBackstageDarkWall);

            // Tramo conector Norte-Sur (X: -4 a 8, Z: 84 a 92) que enlaza con la Zona 20 (Door_Service_To_TheatreTech en X = 0, Z = 84)
            CreatePrimitive("Backstage_SouthLink_Floor", PrimitiveType.Cube, zone28.transform,
                new Vector3(2f, vestY - 0.25f, 88f), new Vector3(12f, 0.5f, 8f), matStoneWall);
            CreatePrimitive("Backstage_SouthLink_Ceiling", PrimitiveType.Cube, zone28.transform,
                new Vector3(2f, vestY + backstageH, 88f), new Vector3(12f, 0.5f, 8f), matBackstageDarkWall);
            CreateWall("Backstage_SouthLink_Wall_West", zone28.transform,
                new Vector3(-4f, vestY + 2.7f, 88f), new Vector3(0.5f, backstageH, 8f), matBackstageDarkWall);

            // Muro sur del Pasillo Detrás del Escenario en el tramo X: -12 a -4 (Z = 92) y puerta en X = 43m hacia los Camerinos
            CreateWall("Backstage_Wall_South_WestSeg", zone28.transform,
                new Vector3(-8f, vestY + 2.7f, 92f), new Vector3(8f, backstageH, 0.5f), matBackstageDarkWall);
            CreateWall("Backstage_Wall_South_EastDoorL", zone28.transform,
                new Vector3(41.475f, vestY + 2.7f, 92f), new Vector3(0.95f, backstageH, 0.5f), matBackstageDarkWall);
            CreateWall("Backstage_Wall_South_EastDoorR", zone28.transform,
                new Vector3(44.525f, vestY + 2.7f, 92f), new Vector3(0.95f, backstageH, 0.5f), matBackstageDarkWall);
            CreateArchitecturalDoor("Door_Backstage_To_DressingHall", zone28.transform,
                new Vector3(43f, vestY, 92f), Quaternion.identity,
                1.8f, 3.2f, backstageH, 0.5f, false, "", "PASILLO DE CAMERINOS (MARA / ELENA)", "", matMahogany, matGold, matBackstageDarkWall);

            // Muro extremo este del pasillo (X = 45, Z: 92 a 98)
            CreateWall("Backstage_Wall_EastEnd", zone28.transform,
                new Vector3(45f, vestY + 2.7f, 95f), new Vector3(0.5f, backstageH, 6f), matBackstageDarkWall);

            // Muro extremo oeste del pasillo (X = -12, Z: 92 a 98) con puerta técnica reforzada en Z = 95m hacia la Sala de Mecanismos y Poleas (Zona 30)
            CreateWall("Backstage_Wall_West_S", zone28.transform,
                new Vector3(-12f, vestY + 2.7f, 92.975f), new Vector3(0.5f, backstageH, 1.95f), matBackstageDarkWall);
            CreateWall("Backstage_Wall_West_N", zone28.transform,
                new Vector3(-12f, vestY + 2.7f, 97.025f), new Vector3(0.5f, backstageH, 1.95f), matBackstageDarkWall);
            CreateArchitecturalDoor("Door_Backstage_To_PulleyRoom", zone28.transform,
                new Vector3(-12f, vestY, 95f), Quaternion.Euler(0f, 90f, 0f),
                1.8f, 3.2f, backstageH, 0.5f, false, "", "SALA DE MECANISMOS Y POLEAS (ZONA 30)", "", matMahogany, matGold, matBackstageDarkWall);

            // Muro norte del Pasillo Detrás del Escenario (Z = 98, X: -12 a 45):
            // - En X = -8.2m: Abertura de observación entre bastidores con vista al Escenario Principal (Zona 32)
            // - En X = -2.5m: Puertas dobles reforzadas hacia el Escenario Principal (desbloqueadas desde la Sala de Poleas)
            // - En X = +3.5m: Acceso lateral de bastidores y cortinas hacia el Escenario Principal
            // - En X = +22.0m: Entrada amplia de puertas dobles (3.7m) hacia el Almacén de Decorados (Zona 29)
            CreateWall("Backstage_Wall_North_Seg1W", zone28.transform, new Vector3(-10.8f, vestY + 2.7f, 98f), new Vector3(2.4f, backstageH, 0.5f), matBackstageDarkWall);
            // Abertura de observación en X: -9.6 a -6.8 (pecho y dintel)
            CreateWall("Backstage_Wall_North_WindowSill", zone28.transform, new Vector3(-8.2f, vestY + 0.55f, 98f), new Vector3(2.8f, 1.1f, 0.5f), matBackstageDarkWall);
            CreateWall("Backstage_Wall_North_WindowHeader", zone28.transform, new Vector3(-8.2f, vestY + 4.25f, 98f), new Vector3(2.8f, 2.3f, 0.5f), matBackstageDarkWall);
            CreateWall("Backstage_Wall_North_Seg1E", zone28.transform, new Vector3(-5.6f, vestY + 2.7f, 98f), new Vector3(2.4f, backstageH, 0.5f), matBackstageDarkWall);

            CreateWall("Backstage_Wall_North_BetweenStageDoors", zone28.transform, new Vector3(0.925f, vestY + 2.7f, 98f), new Vector3(3.05f, backstageH, 0.5f), matBackstageDarkWall);
            CreateWall("Backstage_Wall_North_Seg2", zone28.transform, new Vector3(12.275f, vestY + 2.7f, 98f), new Vector3(15.45f, backstageH, 0.5f), matBackstageDarkWall);
            CreateWall("Backstage_Wall_North_Seg3", zone28.transform, new Vector3(34.5f, vestY + 2.7f, 98f), new Vector3(21.0f, backstageH, 0.5f), matBackstageDarkWall);

            GameObject mainStageDoubleDoorObj = CreateArchitecturalDoubleDoor(
                "Doors_Backstage_To_MainStage", zone28.transform, new Vector3(-2.5f, vestY, 98f), Quaternion.identity,
                3.5f, 3.8f, backstageH, 0.5f, true, "key_stage_counterweight_lock",
                "ESCENARIO PRINCIPAL — ACCESO CENTRAL DE BASTIDORES",
                "Acceso Principal al Escenario — Bloqueado por el freno del Contrapeso III (Sala de Mecanismos y Poleas)",
                matMahogany, matGold, matBackstageDarkWall);
            WorldDoubleDoor mainStageDoubleDoor = mainStageDoubleDoorObj.GetComponent<WorldDoubleDoor>();

            CreateArchitecturalDoor(
                "Door_Backstage_StageWing_East", zone28.transform, new Vector3(3.5f, vestY, 98f), Quaternion.identity,
                1.8f, 3.2f, backstageH, 0.5f, false, "",
                "ACCESO LATERAL DE CORTINAS — ESCENARIO PRINCIPAL (ZONA 32)", "",
                matMahogany, matGold, matBackstageDarkWall);

            CreateArchitecturalDoubleDoor(
                "Doors_Backstage_To_SceneryStorage", zone28.transform, new Vector3(22f, vestY, 98f), Quaternion.identity,
                3.7f, 3.8f, backstageH, 0.5f, false, "",
                "ALMACÉN DE DECORADOS ESCENOGRÁFICOS (ZONA 29)", "",
                matMahogany, matGold, matBackstageDarkWall);

            // Marcos de observación y cortinas pesadas a lo largo del pasillo detrás del escenario
            float[] curtainX = { -9.5f, -6.9f, 14.0f, 31.0f, 39.0f };
            for (int c = 0; c < curtainX.Length; c++)
            {
                DecoPiece($"Cortina_Pesada_Backstage_{c + 1}", PrimitiveType.Cube, zone28.transform,
                    new Vector3(curtainX[c], vestY + 2.5f, 97.55f), new Vector3(2.2f, 4.8f, 0.28f), matVelvetDrapes);
            }

            // Tuberías, cableado antiguo y soportes metálicos a lo largo del techo y paredes del pasillo
            DecoPiece("Tuberia_Backstage_Principal_1", PrimitiveType.Cylinder, zone28.transform,
                new Vector3(16.5f, vestY + 4.85f, 97.35f), new Vector3(0.14f, 28.0f, 0.14f), matRustedLocker).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            DecoPiece("Cableado_Antiguo_Backstage_2", PrimitiveType.Cylinder, zone28.transform,
                new Vector3(16.5f, vestY + 4.55f, 97.42f), new Vector3(0.06f, 28.0f, 0.06f), matIron).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            // Carros para transportar vestuario y señales antiguas de orientación
            CreateServiceCart("Carro_Vestuario_Backstage_1", zone28.transform, new Vector3(9.5f, vestY, 93.4f), Quaternion.Euler(0f, 8f, 0f), matWoodArchive, matIron, matVelvetDrapes);
            CreateServiceCart("Carro_Vestuario_Backstage_2", zone28.transform, new Vector3(33.5f, vestY, 96.4f), Quaternion.Euler(0f, -14f, 0f), matWoodArchive, matIron, matClothCover);

            CreateHistoricalPlaque("Senal_Direccion_Camerinos", zone28.transform, new Vector3(38.0f, vestY + 2.5f, 92.32f), matGold);
            CreateHistoricalPlaque("Senal_Direccion_Almacen_Decorados", zone28.transform, new Vector3(19.2f, vestY + 2.5f, 97.68f), matGold);
            CreateHistoricalPlaque("Senal_Direccion_Sala_Mecanismos", zone28.transform, new Vector3(-9.5f, vestY + 2.5f, 92.32f), matGold);
            CreateHistoricalPlaque("Senal_Direccion_Escenario_Principal", zone28.transform, new Vector3(-5.2f, vestY + 2.5f, 97.68f), matGold);

            // Pequeñas lámparas de servicio (con la luz del fondo vinculada a la anomalía psicológica)
            CreateColdFluorescentFixture("Lampara_Servicio_Backstage_Oeste", zone28.transform, new Vector3(-6f, vestY + 5.1f, 95f), matIron, matLanternGlow, 1.5f);
            CreateColdFluorescentFixture("Lampara_Servicio_Backstage_Centro", zone28.transform, new Vector3(14f, vestY + 5.1f, 95f), matIron, matLanternGlow, 1.5f);
            Light distantBackstageLight = CreateSubtleAnomalyLight("Luz_Servicio_Fondo_Anomalia", zone28.transform,
                new Vector3(37f, vestY + 4.6f, 95f), new Color(1.0f, 0.78f, 0.42f), 2.4f);
            distantBackstageLight.range = 13f;

            // ANOMALÍA AUDITIVA Y LUMÍNICA ZONA 28: Pasos y roce de telas tras las cortinas + luz de servicio que se apaga al acercarse
            GameObject backstageAnomalyHost = new GameObject("Anomaly_Backstage_StepsAndDistantLight");
            backstageAnomalyHost.transform.parent = zone28.transform;
            backstageAnomalyHost.transform.position = new Vector3(35f, vestY + 1.5f, 95f);
            BackstageAnomalyController anom28 = backstageAnomalyHost.AddComponent<BackstageAnomalyController>();
            anom28.Configure(BackstageAnomalyType.BackstageCurtainSoundsAndLight, null, null, distantBackstageLight);
            EditorUtility.SetDirty(anom28);

            // DOCUMENTO DE ORIENTACIÓN Y NARRATIVA ZONA 28
            CreateInteractiveWorldDoc("Doc_Tablon_Regiduria_Backstage", zone28.transform,
                new Vector3(2.5f, vestY + 1.35f, 92.35f), new Vector3(0.55f, 0.42f, 0.06f), matGold,
                "doc_s6_backstage_callsheet",
                "Tablón de Regiduría — Orden de Escena de la Última Representación",
                "PASILLO DETRÁS DEL ESCENARIO — GUÍA DE CONEXIONES Y REGISTRO DE FUNCIÓN\n\n" +
                "• Al Este (→): Camerino de Mara (Zona 26), Camerino de Elena (Zona 27), retorno al Depósito de Instrumentos y Puerta de Servicio Exterior.\n" +
                "• Al Norte-Centro (↑): Almacén de Decorados (Zona 29) y Puertas de Acceso al Escenario Principal.\n" +
                "• Al Oeste (←): Sala de Mecanismos y Poleas (Zona 30).\n\n" +
                "Anotación del regidor de escena (14 de Octubre, 20:14 h):\n" +
                "«Alguien ha echado el cierre mecánico de las puertas del Escenario Principal desde la Sala de Mecanismos y Poleas. Para abrir el paso al escenario hay que liberar el Contrapeso III en la Sala de Poleas».");

            // ====================================================================
            // ZONA 29: ALMACÉN DE DECORADOS (X: 10.6 a 38, Z: 98 a 114, Y: 2.4m, Altura: 6.8m)
            // Deja el vano oculto X: 6.0 a 10.6 detrás del muro este del escenario para la Cámara Secreta (Zona 39)
            // ====================================================================
            GameObject zone29 = new GameObject("29_Scenery_Storage");
            zone29.transform.parent = rootEnv;
            float sceneryH = 6.8f;

            CreatePrimitive("Scenery_Floor", PrimitiveType.Cube, zone29.transform,
                new Vector3(24.3f, vestY - 0.25f, 106f), new Vector3(27.4f, 0.5f, 16f), matParquet);
            CreatePrimitive("Scenery_Ceiling", PrimitiveType.Cube, zone29.transform,
                new Vector3(24.3f, vestY + sceneryH, 106f), new Vector3(27.4f, 0.5f, 16f), matBackstageDarkWall);

            // Muros perimetrales del Almacén de Decorados (Sur Z = 98 ya está cerrado por Zona 28 con sus puertas dobles en X = 22m)
            CreateWall("Scenery_Wall_North", zone29.transform, new Vector3(24.3f, vestY + 3.4f, 114f), new Vector3(27.4f, sceneryH, 0.5f), matBackstageDarkWall);
            CreateWall("Scenery_Wall_West", zone29.transform, new Vector3(10.6f, vestY + 3.4f, 106f), new Vector3(0.5f, sceneryH, 16f), matBackstageDarkWall);
            CreateWall("Scenery_Wall_East", zone29.transform, new Vector3(38f, vestY + 3.4f, 106f), new Vector3(0.5f, sceneryH, 16f), matBackstageDarkWall);

            // Paneles pintados de 'El Lago de las Sombras' (lago nocturno, árboles oscuros y arquitectura escénica)
            // dejando corredores transitables entre estructuras
            CreatePrimitive("Panel_Pintado_Bosque_Oeste", PrimitiveType.Cube, zone29.transform,
                new Vector3(13.2f, vestY + 2.2f, 103.5f), new Vector3(5.5f, 4.4f, 0.22f), matPaintedLake).transform.localRotation = Quaternion.Euler(0f, 14f, 0f);
            CreatePrimitive("Panel_Pintado_Arquitectura_Este", PrimitiveType.Cube, zone29.transform,
                new Vector3(31.5f, vestY + 2.2f, 103.5f), new Vector3(5.5f, 4.4f, 0.22f), matMahogany).transform.localRotation = Quaternion.Euler(0f, -12f, 0f);

            // Panel central del Lago Nocturno que difiere de las fotografías oficiales (Interactivo)
            GameObject alteredLakePanel = CreatePrimitive("Panel_Lago_Nocturno_Modificado", PrimitiveType.Cube, zone29.transform,
                new Vector3(22.0f, vestY + 2.4f, 111.5f), new Vector3(7.2f, 4.8f, 0.26f), matPaintedLake);
            DecoPiece("Marco_Dorado_Panel_Lago", PrimitiveType.Cube, alteredLakePanel.transform,
                new Vector3(0f, 0f, -0.15f), new Vector3(0.95f, 0.92f, 0.12f), matGold);

            // Estructura alta cubierta por una lona junto al panel
            CreatePrimitive("Estructura_Cubierta_Lona_Almacen", PrimitiveType.Cube, zone29.transform,
                new Vector3(14.8f, vestY + 1.85f, 110.5f), new Vector3(2.8f, 3.7f, 2.2f), matClothCover);

            // Marcos de ventanas falsas, columnas decorativas, cortinas enrolladas y cajas de utilería etiquetadas
            CreatePrimitive("Marco_Ventana_Falsa_1", PrimitiveType.Cube, zone29.transform,
                new Vector3(11.3f, vestY + 1.8f, 108.0f), new Vector3(0.25f, 3.6f, 2.4f), matMahogany);
            CreateClassicalColumn("Columna_Escenografica_1", zone29.transform, new Vector3(28.5f, vestY, 110.5f), 4.8f, 0.38f, matWallIvory);
            CreateClassicalColumn("Columna_Escenografica_2", zone29.transform, new Vector3(30.2f, vestY, 111.2f), 4.8f, 0.38f, matWallIvory);
            DecoPiece("Cortina_Enrollada_1", PrimitiveType.Cylinder, zone29.transform,
                new Vector3(35.2f, vestY + 0.35f, 109.5f), new Vector3(0.45f, 2.6f, 0.45f), matVelvetDrapes).transform.localRotation = Quaternion.Euler(0f, 20f, 90f);

            CreatePrimitive("Caja_Utileria_Etiquetada_1", PrimitiveType.Cube, zone29.transform,
                new Vector3(17.2f, vestY + 0.55f, 106.5f), new Vector3(1.5f, 1.1f, 1.3f), matWoodArchive);
            CreatePrimitive("Caja_Utileria_Etiquetada_2", PrimitiveType.Cube, zone29.transform,
                new Vector3(26.8f, vestY + 0.55f, 106.5f), new Vector3(1.5f, 1.1f, 1.3f), matWoodArchive);

            // Interacción principal en el panel pintado de 'El Lago de las Sombras'
            GameObject revealedStageTag = DecoPiece("Etiqueta_Tramoya_13_Octubre", PrimitiveType.Cube, alteredLakePanel.transform,
                new Vector3(0.35f, -0.15f, -0.22f), new Vector3(0.12f, 0.08f, 0.05f), matGold);
            BackstageAnomalyController sceneryCompareInteract = alteredLakePanel.AddComponent<BackstageAnomalyController>();
            sceneryCompareInteract.Configure(BackstageAnomalyType.InteractiveSceneryComparison, null, null, null, revealedStageTag, null);
            EditorUtility.SetDirty(sceneryCompareInteract);

            // ANOMALÍA BREVE ZONA 29: Una cortina colgada se mueve sin corriente de aire y una silueta pasa detrás de un panel
            GameObject swayingCurtain = DecoPiece("Cortina_Colgada_Anomalia_Almacen", PrimitiveType.Cube, zone29.transform,
                new Vector3(16.0f, vestY + 2.6f, 103.8f), new Vector3(1.8f, 4.6f, 0.20f), matVelvetDrapes);
            GameObject silhouetteBehindPanel = CreateBalletSilhouetteFigure("Silueta_Detras_Panel_Almacen", zone29.transform,
                new Vector3(12.5f, vestY, 104.8f), matShadowFigure, true);
            GameObject sceneryAnomalyHost = new GameObject("Anomaly_SceneryStorage_CurtainAndSilhouette");
            sceneryAnomalyHost.transform.parent = zone29.transform;
            sceneryAnomalyHost.transform.position = new Vector3(14.5f, vestY + 1.5f, 104.2f);
            BackstageAnomalyController anom29 = sceneryAnomalyHost.AddComponent<BackstageAnomalyController>();
            anom29.Configure(BackstageAnomalyType.SceneryCurtainAndSilhouette, swayingCurtain.transform, silhouetteBehindPanel.transform);
            EditorUtility.SetDirty(anom29);

            CreateColdFluorescentFixture("Luz_Almacen_Decorados_1", zone29.transform, new Vector3(15f, vestY + 6.2f, 106f), matIron, matLanternGlow, 1.8f);
            CreateColdFluorescentFixture("Luz_Almacen_Decorados_2", zone29.transform, new Vector3(29f, vestY + 6.2f, 106f), matIron, matLanternGlow, 1.8f);

            // DOCUMENTOS DE LA ZONA 29 (ALMACÉN DE DECORADOS)
            CreateInteractiveWorldDoc("Doc_Etiqueta_Contradictoria_Decorado", zone29.transform,
                new Vector3(17.2f, vestY + 1.14f, 106.5f), new Vector3(0.42f, 0.04f, 0.30f), matBlueprintPaper,
                "doc_s6_scenery_contradictory_label",
                "Registro de Almacén Escenográfico — Contradicción de Fechas",
                "INVENTARIO DE ESCENOGRAFÍA — «EL LAGO DE LAS SOMBRAS»\n\n" +
                "• Registro Oficial de Dirección: «El decorado original del Lago Nocturno con arco central permaneció montado hasta la clausura del 18 de octubre».\n" +
                "• Etiqueta física clavada en la caja de utilería Nº 09: **«Retirado del escenario principal la noche del 13 de octubre de 1924 por orden directa de Isadora Vale»**.\n\n" +
                "Inspecciona el gran **Panel del Lago Nocturno al fondo del almacén ([E])** para comparar su estructura con las fotografías oficiales.");

            CreateInteractiveWorldDoc("Doc_Nota_Estructura_Bajo_Lona", zone29.transform,
                new Vector3(26.8f, vestY + 1.14f, 106.5f), new Vector3(0.38f, 0.04f, 0.28f), matGold,
                "doc_s6_tarp_structure_note",
                "Nota Técnica sobre el Bastidor de Cierre",
                "Sobre la caja contigua hay un croquis de los carpinteros de teatro:\n\n" +
                "«Nos hicieron construir un panel ciego idéntico al arco del lago pero sin salida trasera. Si alguien baja el Contrapeso III desde la Sala de Poleas en mitad de la función, la bailarina que esté en el fondo del escenario queda incomunicada de los bastidores».");

            // ====================================================================
            // ZONA 30: SALA DE MECANISMOS Y POLEAS (X: -28 a -12, Z: 84 a 104, Y: 2.4m, Altura: 7.2m)
            // ====================================================================
            GameObject zone30 = new GameObject("30_Pulley_Mechanism_Room");
            zone30.transform.parent = rootEnv;
            float pulleyH = 7.2f;

            CreatePrimitive("PulleyRoom_Floor", PrimitiveType.Cube, zone30.transform,
                new Vector3(-20f, vestY - 0.25f, 94f), new Vector3(16f, 0.5f, 20f), matStoneWall);
            CreatePrimitive("PulleyRoom_Ceiling", PrimitiveType.Cube, zone30.transform,
                new Vector3(-20f, vestY + pulleyH, 94f), new Vector3(16f, 0.5f, 20f), matBackstageDarkWall);

            // Muros perimetrales de la Sala de Mecanismos y Poleas
            CreateWall("PulleyRoom_Wall_South", zone30.transform, new Vector3(-20f, vestY + 3.6f, 84f), new Vector3(16f, pulleyH, 0.5f), matBackstageDarkWall);
            CreateWall("PulleyRoom_Wall_North", zone30.transform, new Vector3(-20f, vestY + 3.6f, 104f), new Vector3(16f, pulleyH, 0.5f), matBackstageDarkWall);
            // Muro Oeste (X = -28m, Z: 84..104) con puerta en Z = 85.5m hacia el Taller de Zapatillas y Tintorería Escénica (Zona 46)
            CreateWall("PulleyRoom_Wall_West_S", zone30.transform, new Vector3(-28f, vestY + 3.6f, 84.225f), new Vector3(0.5f, pulleyH, 0.45f), matBackstageDarkWall);
            CreateWall("PulleyRoom_Wall_West_N", zone30.transform, new Vector3(-28f, vestY + 3.6f, 95.275f), new Vector3(0.5f, pulleyH, 17.45f), matBackstageDarkWall);
            CreateArchitecturalDoor("Door_PulleyRoom_To_Workshop", zone30.transform,
                new Vector3(-28f, vestY, 85.5f), Quaternion.Euler(0f, 90f, 0f),
                1.8f, 3.2f, pulleyH, 0.5f, false, "",
                "TALLER DE ZAPATILLAS Y TINTORERÍA ESCÉNICA (ZONA 46)", "", matMahogany, matGold, matBackstageDarkWall);
            CreateWall("PulleyRoom_Wall_East_S", zone30.transform, new Vector3(-12f, vestY + 3.6f, 88.0f), new Vector3(0.5f, pulleyH, 8.0f), matBackstageDarkWall);
            CreateWall("PulleyRoom_Wall_East_N1", zone30.transform, new Vector3(-12f, vestY + 3.6f, 98.975f), new Vector3(0.5f, pulleyH, 1.95f), matBackstageDarkWall);
            CreateWall("PulleyRoom_Wall_East_N2", zone30.transform, new Vector3(-12f, vestY + 3.6f, 103.025f), new Vector3(0.5f, pulleyH, 1.95f), matBackstageDarkWall);
            CreateArchitecturalDoor("Door_PulleyRoom_To_StageWing", zone30.transform,
                new Vector3(-12f, vestY, 101f), Quaternion.Euler(0f, 90f, 0f),
                1.8f, 3.2f, pulleyH, 0.5f, false, "", "HOMBRO OESTE DEL ESCENARIO PRINCIPAL (ZONA 32)", "", matMahogany, matGold, matBackstageDarkWall);

            // Estructura funcional y plausible de poleas, vigas de suspensión, cuerdas, contrapesos, engranajes y palancas
            for (int g = 0; g < 4; g++)
            {
                float gz = 87.5f + g * 4.2f;
                // Vigas metálicas superiores del peine de tramoya
                DecoPiece($"FlyGrid_Beam_{g + 1}", PrimitiveType.Cube, zone30.transform,
                    new Vector3(-20f, vestY + 6.1f, gz), new Vector3(14.5f, 0.28f, 0.28f), matIron);

                // Soportes verticales de guía de contrapesos en la pared oeste
                CreatePrimitive($"Counterweight_GuideRail_{g + 1}", PrimitiveType.Cylinder, zone30.transform,
                    new Vector3(-26.8f, vestY + 3.1f, gz), new Vector3(0.12f, 3.1f, 0.12f), matIron);

                // Poleas superiores fijadas a la estructura y cuerdas verticales comprensibles
                DecoPiece($"Overhead_Pulley_Wheel_{g + 1}", PrimitiveType.Cylinder, zone30.transform,
                    new Vector3(-24.5f, vestY + 5.85f, gz), new Vector3(0.65f, 0.08f, 0.65f), matRustedLocker).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                DecoPiece($"Vertical_HempRope_{g + 1}", PrimitiveType.Cylinder, zone30.transform,
                    new Vector3(-24.5f, vestY + 3.2f, gz), new Vector3(0.05f, 2.7f, 0.05f), matHempRope);
            }

            // Contrapesos de hierro (el Contrapeso III es el que se desplaza al resolver el puzle)
            CreatePrimitive("Contrapeso_Hierro_I", PrimitiveType.Cube, zone30.transform,
                new Vector3(-26.2f, vestY + 2.4f, 87.5f), new Vector3(0.65f, 1.4f, 0.65f), matIron);
            CreatePrimitive("Contrapeso_Hierro_II", PrimitiveType.Cube, zone30.transform,
                new Vector3(-26.2f, vestY + 3.1f, 91.7f), new Vector3(0.65f, 1.4f, 0.65f), matIron);
            GameObject counterweightIII = CreatePrimitive("Contrapeso_Hierro_III_Escenario", PrimitiveType.Cube, zone30.transform,
                new Vector3(-26.2f, vestY + 1.2f, 95.9f), new Vector3(0.70f, 1.5f, 0.70f), matRustedLocker);

            // Banco central de palancas, manivelas y engranajes con marcas de uso recientes
            GameObject leverBank = CreatePrimitive("Banco_Controles_Tramoya", PrimitiveType.Cube, zone30.transform,
                new Vector3(-21.5f, vestY + 0.55f, 95.9f), new Vector3(1.6f, 1.1f, 2.8f), matIron);
            DecoPiece("Engranaje_Principal_Tramoya", PrimitiveType.Cylinder, leverBank.transform,
                new Vector3(-0.52f, 0.35f, 0f), new Vector3(0.65f, 0.12f, 0.65f), matGold).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            GameObject masterLeverArm = DecoPiece("Palanca_Contrapeso_III_Reciente", PrimitiveType.Cylinder, leverBank.transform,
                new Vector3(0.25f, 0.65f, 0f), new Vector3(0.08f, 0.48f, 0.08f), matGold);
            masterLeverArm.transform.localRotation = Quaternion.Euler(-24f, 0f, 0f);

            // Compartimento técnico oculto que se descubre al accionar el mecanismo
            GameObject hiddenTechCompartment = DecoPiece("Compartimento_Oculto_JefeTramoya", PrimitiveType.Cube, leverBank.transform,
                new Vector3(0.35f, 0.58f, 0.32f), new Vector3(0.24f, 0.08f, 0.22f), matBlueprintPaper);

            BackstageAnomalyController pulleyPuzzle = leverBank.AddComponent<BackstageAnomalyController>();
            pulleyPuzzle.Configure(
                BackstageAnomalyType.InteractivePulleyMechanismPuzzle,
                masterLeverArm.transform,
                counterweightIII.transform,
                null,
                hiddenTechCompartment,
                mainStageDoubleDoor);
            EditorUtility.SetDirty(pulleyPuzzle);

            // Mesa de trabajo con planos técnicos amarillentos, herramientas colgadas y panel de instrucciones
            CreatePrimitive("Mesa_Trabajo_Mecanismos", PrimitiveType.Cube, zone30.transform,
                new Vector3(-16.5f, vestY + 0.45f, 88.5f), new Vector3(2.8f, 0.90f, 1.4f), matWoodArchive);
            CreateHistoricalPlaque("Panel_Instrucciones_Operacion", zone30.transform,
                new Vector3(-16.5f, vestY + 2.2f, 84.35f), matGold);
            DecoPiece("Herramienta_Colgada_1", PrimitiveType.Cube, zone30.transform,
                new Vector3(-14.2f, vestY + 1.8f, 84.32f), new Vector3(0.18f, 0.65f, 0.05f), matIron);
            DecoPiece("Herramienta_Colgada_2", PrimitiveType.Cube, zone30.transform,
                new Vector3(-13.8f, vestY + 1.8f, 84.32f), new Vector3(0.14f, 0.52f, 0.05f), matRustedLocker);

            // ANOMALÍA PSICOLÓGICA ZONA 30: Polea que gira lentamente cuando nadie está cerca y crujido de cuerdas
            GameObject autonomousPulleyWheel = DecoPiece("Polea_Giro_Autonomo_Anomalia", PrimitiveType.Cylinder, zone30.transform,
                new Vector3(-21.5f, vestY + 3.8f, 100.1f), new Vector3(0.85f, 0.08f, 0.85f), matGold);
            autonomousPulleyWheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            GameObject pulleyAnomalyHost = new GameObject("Anomaly_PulleyRoom_SelfTurn");
            pulleyAnomalyHost.transform.parent = zone30.transform;
            pulleyAnomalyHost.transform.position = new Vector3(-21.5f, vestY + 2.0f, 100.1f);
            BackstageAnomalyController anom30 = pulleyAnomalyHost.AddComponent<BackstageAnomalyController>();
            anom30.Configure(BackstageAnomalyType.PulleyTensionAndSelfTurn, autonomousPulleyWheel.transform);
            EditorUtility.SetDirty(anom30);

            CreateColdFluorescentFixture("Luz_Sala_Poleas_1", zone30.transform, new Vector3(-20f, vestY + 6.5f, 89f), matIron, matColdFluorescent, 1.7f);
            CreateColdFluorescentFixture("Luz_Sala_Poleas_2", zone30.transform, new Vector3(-20f, vestY + 6.5f, 99f), matIron, matLanternGlow, 1.8f);

            // DOCUMENTOS DE LA ZONA 30 (SALA DE MECANISMOS Y POLEAS)
            CreateInteractiveWorldDoc("Doc_Plano_Disposicion_Original_Decorados", zone30.transform,
                new Vector3(-16.9f, vestY + 0.94f, 88.5f), new Vector3(0.52f, 0.04f, 0.36f), matBlueprintPaper,
                "doc_s6_original_stage_rigging_blueprint",
                "Plano Técnico Amarillento — Disposición Original de Decorados y Contrapesos",
                "PLANO DE TRAMOYA Y SUSPENSIÓN ESCÉNICA — GRAN TEATRO VESPER\n\n" +
                "• Línea I (Contrapeso Norte): Telón de Boca de Terciopelo Borgoña.\n" +
                "• Línea II (Contrapeso Central): Bambalinas y Puente de Luces de Proscenio.\n" +
                "• Línea III (Contrapeso Sur — Palanca Dorada): **Arco Practicable del Lago y Cerrojo Mecánico de Acceso al Escenario Principal**.\n\n" +
                "Instrucción de seguridad: «Si la Palanca III se deja abatida hacia atrás, el telón ciego desciende y bloquea las puertas de acceso al Escenario Principal. Para abrir el paso al escenario, acciona el **Banco de Controles de Tramoya ([E])** en el centro de la sala».");

            CreateInteractiveWorldDoc("Doc_Registro_Tecnico_Manipulacion", zone30.transform,
                new Vector3(-16.0f, vestY + 0.94f, 88.5f), new Vector3(0.38f, 0.04f, 0.28f), matGold,
                "doc_s6_technician_manipulation_log",
                "Anotaciones del Técnico de Escena — Manipulación en la Última Función",
                "“14 de Octubre de 1924 — 20:30 h\n\n" +
                "Al revisar los tambores de cable después del tercer acto, encontré la empuñadura de latón de la Palanca III limpia de polvo y con marcas de haberse accionado con fuerza durante la representación.\n\n" +
                "Los elementos del escenario fueron movidos en un orden inverso al previsto en el libreto: alguien bajó el cierre del fondo mientras Elena estaba en escena, justo en el compás en que la música fue alterada.”");
        }

        // ====================================================================
        // CONSTRUCCIÓN DE LA SECCIÓN 7: TEATRO PRINCIPAL — CLÍMAX (Zonas 31 a 35)
        // ====================================================================
        private static void BuildSection7MainTheatreClimax(
            Transform rootEnv, float vestY,
            Material matParquet, Material matCarpet, Material matWallIvory, Material matMahogany,
            Material matGold, Material matIron, Material matClothCover, Material matVelvetDrapes,
            Material matLanternGlow, Material matColdFluorescent, Material matGreenVelvet,
            Material matLeather, Material matWoodArchive, Material matMetalArchive, Material matCardboard,
            Material matStoneWall, Material matRustedLocker, Material matMirrorGlass, Material matPorcelain,
            Material matTutuWhite, Material matTutuBlack, Material matSatinRibbon, Material matShadowFigure,
            Material matPortraitElena, Material matPhotoTroupe1923)
        {
            Material matTheatreDarkWood = GetOrCreateBasicMaterial("Mat_S7_TheatreStageWood", new Color(0.18f, 0.12f, 0.09f), 0.42f, 0.04f);
            Material matLakeWaterSurface = GetOrCreateBasicMaterial("Mat_S7_StageLakeWater", new Color(0.09f, 0.18f, 0.28f), 0.88f, 0.35f);
            Material matMistArch = GetOrCreateBasicMaterial("Mat_S7_MistArchStone", new Color(0.32f, 0.35f, 0.40f), 0.25f, 0.02f);
            Material matCatwalkSteel = GetOrCreateBasicMaterial("Mat_S7_CatwalkSteel", new Color(0.22f, 0.24f, 0.27f), 0.45f, 0.65f);
            Material matProgramPaper = GetOrCreateBasicMaterial("Mat_S7_ProgramPaper", new Color(0.86f, 0.79f, 0.62f), 0.18f, 0.0f);

            float stageY = vestY;            // Nivel del Escenario Principal: 2.4m
            float pitY = vestY - 1.2f;       // Nivel hundido del Foso de Orquesta: 1.2m
            float audY = vestY - 0.8f;       // Nivel del Patio de Butacas: 1.6m
            float upperY = vestY + 5.2f;     // Nivel de la Cabina de Iluminación y Pasarela Superior: 7.6m
            float roofY = vestY + 11.5f;     // Techo monumental del Gran Teatro: 13.9m

            // ====================================================================
            // ZONA 32: ESCENARIO PRINCIPAL (X: -22 a +6, Z: 98 a 116, Y: 2.4m)
            // ====================================================================
            GameObject zone32 = new GameObject("32_Main_Stage");
            zone32.transform.parent = rootEnv;

            // Tablas de madera oscura del escenario principal y techo de la caja escénica
            CreatePrimitive("MainStage_Floor", PrimitiveType.Cube, zone32.transform,
                new Vector3(-8f, stageY - 0.25f, 107f), new Vector3(28f, 0.5f, 18f), matTheatreDarkWood);
            CreatePrimitive("MainStage_Roof", PrimitiveType.Cube, zone32.transform,
                new Vector3(-8f, roofY, 107f), new Vector3(28f, 0.5f, 18f), matMahogany);

            // Frente bajo del proscenio (Z = 116) que separa el escenario elevado del foso hundido
            CreateWall("Stage_Apron_FrontWall", zone32.transform,
                new Vector3(-8f, pitY + 0.6f, 116f), new Vector3(24f, 1.2f, 0.4f), matMahogany);
            DecoPiece("Stage_Footlights_Trim", PrimitiveType.Cube, zone32.transform,
                new Vector3(-8f, stageY + 0.06f, 115.75f), new Vector3(20f, 0.12f, 0.25f), matGold);

            // Muro sur del hombro oeste del escenario (Z = 98, X: -22 a -12; el tramo -12..+6 ya tiene las puertas de Zona 28)
            // No ponemos muro en X: -22..-12, Z = 98..104 porque ahí limita con el muro norte de Zona 30 (Z = 104)
            // Muro este del escenario (X = 6, Z: 114 a 116; Z: 98..114 lo cubre el muro oeste de Zona 29)
            CreateWall("Stage_Wall_East_Upper", zone32.transform,
                new Vector3(6f, stageY + 8.0f, 107f), new Vector3(0.5f, 7.0f, 18f), matMahogany);
            CreateWall("Stage_Wall_South_Upper", zone32.transform,
                new Vector3(-8f, stageY + 8.4f, 98f), new Vector3(28f, 6.2f, 0.5f), matMahogany);

            // Grandes telones de boca de terciopelo borgoña en el arco de proscenio (Z = 115.4)
            CreatePrimitive("Proscenium_GrandDrape_West", PrimitiveType.Cube, zone32.transform,
                new Vector3(-18.5f, stageY + 5.2f, 115.4f), new Vector3(6.5f, 10.4f, 0.55f), matVelvetDrapes);
            CreatePrimitive("Proscenium_GrandDrape_East", PrimitiveType.Cube, zone32.transform,
                new Vector3(2.5f, stageY + 5.2f, 115.4f), new Vector3(6.5f, 10.4f, 0.55f), matVelvetDrapes);
            DecoPiece("Proscenium_UpperValance_Gold", PrimitiveType.Cube, zone32.transform,
                new Vector3(-8f, stageY + 9.6f, 115.4f), new Vector3(28f, 2.2f, 0.65f), matVelvetDrapes);
            DecoPiece("Proscenium_GoldArchMold", PrimitiveType.Cube, zone32.transform,
                new Vector3(-8f, stageY + 8.45f, 115.55f), new Vector3(15.5f, 0.28f, 0.72f), matGold);

            // Bastidores laterales (patas escénicas) que ocultan los laterales del escenario
            float[] wingZ = { 103.5f, 108.0f, 112.5f };
            for (int w = 0; w < wingZ.Length; w++)
            {
                DecoPiece($"Stage_WingCurtain_W_{w + 1}", PrimitiveType.Cube, zone32.transform,
                    new Vector3(-16.2f, stageY + 4.2f, wingZ[w]), new Vector3(2.8f, 8.4f, 0.28f), matVelvetDrapes).transform.localRotation = Quaternion.Euler(0f, 15f, 0f);
                DecoPiece($"Stage_WingCurtain_E_{w + 1}", PrimitiveType.Cube, zone32.transform,
                    new Vector3(0.2f, stageY + 4.2f, wingZ[w]), new Vector3(2.8f, 8.4f, 0.28f), matVelvetDrapes).transform.localRotation = Quaternion.Euler(0f, -15f, 0f);
            }

            // Escenografía física envejecida de 'El Lago de las Sombras':
            // 1. Superficie que sugiere agua nocturna en la mitad posterior del escenario
            DecoPiece("Escenografia_Superficie_Lago", PrimitiveType.Cylinder, zone32.transform,
                new Vector3(-8f, stageY + 0.015f, 104.2f), new Vector3(11.5f, 0.015f, 7.5f), matLakeWaterSurface);

            // 2. Árboles oscuros tallados en madera escénica
            Vector3[] treePositions =
            {
                new Vector3(-13.8f, stageY, 102.5f),
                new Vector3(-2.2f, stageY, 102.5f),
                new Vector3(-14.5f, stageY, 106.8f),
                new Vector3(-1.5f, stageY, 106.8f)
            };
            for (int t = 0; t < treePositions.Length; t++)
            {
                CreateStageTreeProp($"Arbol_Oscuro_Escenario_{t + 1}", zone32.transform, treePositions[t], matTheatreDarkWood, matShadowFigure);
            }

            // 3. Estructuras arquitectónicas neoclásicas que surgen de la niebla al fondo (El Arco del Lago)
            GameObject lakeArch = new GameObject("Arco_Escenico_Lago_De_Las_Sombras");
            lakeArch.transform.parent = zone32.transform;
            lakeArch.transform.position = new Vector3(-8f, stageY, 101.2f);
            CreateClassicalColumn("Columna_Lago_Izq", lakeArch.transform, new Vector3(-2.6f, 0f, 0f), 4.6f, 0.36f, matMistArch);
            CreateClassicalColumn("Columna_Lago_Der", lakeArch.transform, new Vector3(2.6f, 0f, 0f), 4.6f, 0.36f, matMistArch);
            CreatePrimitive("Arquitrabe_Arco_Lago", PrimitiveType.Cube, lakeArch.transform,
                new Vector3(0f, 4.8f, 0f), new Vector3(6.2f, 0.55f, 0.65f), matMistArch);
            DecoPiece("Niebla_Baja_Lago_1", PrimitiveType.Cube, lakeArch.transform,
                new Vector3(0f, 0.12f, 0.8f), new Vector3(8.5f, 0.18f, 3.2f), matClothCover);

            // Telón secundario intermedio falso que oculta el Arco del Lago hasta que se acciona la Pasarela Superior (Fase 4)
            GameObject falseMidStageCurtain = DecoPiece("Telon_Secundario_Falso_Lago", PrimitiveType.Cube, zone32.transform,
                new Vector3(-8f, stageY + 2.4f, 102.3f), new Vector3(6.4f, 4.8f, 0.18f), matVelvetDrapes);

            // ANOMALÍA VISUAL PRINCIPAL ZONA 32: Silueta femenina al fondo del escenario bajo luz teatral en postura de ballet
            GameObject stageSilhouetteHost = new GameObject("Anomaly_Stage_BallerinaSilhouette");
            stageSilhouetteHost.transform.parent = zone32.transform;
            stageSilhouetteHost.transform.position = new Vector3(-8f, stageY, 103.4f);
            GameObject stageBallerinaFig = CreateBalletSilhouetteFigure("Silueta_Elena_Bajo_Foco_Escenario", stageSilhouetteHost.transform,
                Vector3.zero, matShadowFigure, true);
            Light stageColdBacklight = CreatePictureSpotlight("Foco_Frio_Silueta_Escenario", zone32.transform,
                new Vector3(-8f, stageY + 6.8f, 106.5f), new Vector3(-8f, stageY + 1.0f, 103.4f), new Color(0.62f, 0.80f, 1.0f), 3.6f);
            TheatreClimaxController anom32 = stageSilhouetteHost.AddComponent<TheatreClimaxController>();
            anom32.Configure(TheatreClimaxNodeType.AnomalyStageBallerinaSilhouette, stageBallerinaFig.transform, null, stageColdBacklight);
            EditorUtility.SetDirty(anom32);

            // Marcas de posición en el suelo del escenario (Elena en el centro, Mara en el lateral derecho)
            CreateChoreographyFloorMark("Marca_Suelo_Elena_Proscenio", zone32.transform, new Vector3(-8f, stageY + 0.02f, 110.0f), matGold, true);
            CreateChoreographyFloorMark("Marca_Suelo_Mara_Lateral", zone32.transform, new Vector3(-3.2f, stageY + 0.02f, 110.8f), matSatinRibbon, false);
            GameObject hiddenUpstageMark = DecoPiece("Marca_Oculta_Arco_Lago", PrimitiveType.Cylinder, zone32.transform,
                new Vector3(-8f, stageY + 0.03f, 101.2f), new Vector3(1.2f, 0.02f, 1.2f), matGold);

            // FASE 2 DE LA SECUENCIA FINAL: Reconstruir posiciones escénicas en las marcas laterales del escenario
            GameObject stagePositionsInteractObj = CreatePrimitive("Marcas_Interactivas_Posiciones_Escenario", PrimitiveType.Cube, zone32.transform,
                new Vector3(-3.2f, stageY + 0.08f, 110.8f), new Vector3(1.1f, 0.16f, 1.1f), matGold);
            TheatreClimaxController phase2Ctrl = stagePositionsInteractObj.AddComponent<TheatreClimaxController>();
            phase2Ctrl.Configure(TheatreClimaxNodeType.InteractivePhase2StagePositions, null, null, null, null, hiddenUpstageMark);
            EditorUtility.SetDirty(phase2Ctrl);

            // FASE 5 / CLÍMAX EN EL CENTRO DEL ESCENARIO: Atril dorado de proscenio para ejecutar la secuencia completa
            GameObject climaxCenterStand = new GameObject("Pedestal_Climax_Centro_Escenario");
            climaxCenterStand.transform.parent = zone32.transform;
            climaxCenterStand.transform.position = new Vector3(-8f, stageY, 110.0f);
            CreateMusicStand("Atril_Climax_Ultimo_Acto", climaxCenterStand.transform, Vector3.zero, Quaternion.identity, matGold, matMahogany);
            GameObject climaxTriggerBox = CreatePrimitive("Trigger_Ejecutar_Secuencia_Final", PrimitiveType.Cube, climaxCenterStand.transform,
                new Vector3(0f, 1.05f, 0f), new Vector3(0.85f, 1.4f, 0.85f), matGold);
            climaxTriggerBox.GetComponent<Renderer>().enabled = false;
            GameObject finalTestimonyReveal = DecoPiece("Testimonio_Final_Arco_Lago", PrimitiveType.Cube, zone32.transform,
                new Vector3(-8f, stageY + 0.95f, 101.2f), new Vector3(0.48f, 0.12f, 0.36f), matGold);
            TheatreClimaxController phase5Ctrl = climaxTriggerBox.AddComponent<TheatreClimaxController>();
            phase5Ctrl.Configure(TheatreClimaxNodeType.InteractivePhase5FinalStageClimax, null, null, stageColdBacklight, null, finalTestimonyReveal);
            EditorUtility.SetDirty(phase5Ctrl);

            // DOCUMENTOS DE LA ZONA 32 (ESCENARIO PRINCIPAL)
            CreateInteractiveWorldDoc("Doc_Partitura_Incompleta_Escenario", zone32.transform,
                new Vector3(-7.3f, stageY + 0.04f, 110.4f), new Vector3(0.42f, 0.04f, 0.30f), matProgramPaper,
                "doc_s7_stage_incomplete_score",
                "Partitura de Escena y Guía de la Secuencia Final (Clímax)",
                "RECONSTRUCCIÓN DE «EL LAGO DE LAS SOMBRAS» — 14 DE OCTUBRE DE 1924\n\n" +
                "Sobre las tablas del escenario convergen todas las pistas de la academia. Para reconstruir lo ocurrido en el último acto:\n" +
                "1. **Foso de Orquesta (Zona 33)**: Ordena los fragmentos en el Atril del Director para confirmar la cadencia en *Do Natural*.\n" +
                "2. **Marcas del Escenario (Zona 32)**: Inspecciona las marcas de posición de Elena y Mara a la derecha del proscenio (`[E]`).\n" +
                "3. **Cabina de Iluminación (Zona 34)**: Sube por la escalera técnica oeste y activa la secuencia de focos en la consola.\n" +
                "4. **Pasarela Superior (Zona 35)**: Acciona el control superior sobre el escenario para elevar el telón falso del Lago.\n" +
                "5. **Ejecución Final**: Pulsa `[E]` en el **Atril Central del Escenario** para desencadenar la revelación final.");

            CreateInteractiveWorldDoc("Doc_Notas_Posiciones_Elena_Mara_Escenario", zone32.transform,
                new Vector3(-3.2f, stageY + 0.05f, 111.5f), new Vector3(0.38f, 0.04f, 0.28f), matSatinRibbon,
                "doc_s7_stage_elena_mara_positions",
                "Anotación de Regiduría sobre las Marcas del Suelo",
                "Junto a las marcas de tiza y latón del lateral derecho hay un par de cintas de raso cortadas:\n\n" +
                "«Posición A (Centro): Elena Vesper — Coda original.\n" +
                "Posición B (Hombro Derecho): Mara Bell — Entrada añadida el 13 de octubre.\n" +
                "Posición C (Bajo el Arco del Lago): Punto donde Elena debía emerger al sonar el Do Natural, justo debajo del bastidor conectado al Contrapeso III».");

            // ====================================================================
            // CIRCULACIONES LATERALES Y ESCALERAS MULTI-NIVEL DEL TEATRO
            // Conecta Escenario (2.4m) <-> Foso (1.2m) <-> Patio de Butacas (1.6m) <-> Cabina y Pasarela (7.6m)
            // ====================================================================
            GameObject theatreLinks = new GameObject("Theatre_Vertical_And_Lateral_Links");
            theatreLinks.transform.parent = rootEnv;

            // Plataforma técnica oeste junto al escenario (X: -28 a -22, Z: 104 a 122, Y: 2.4m)
            CreatePrimitive("WestTech_StageLanding", PrimitiveType.Cube, theatreLinks.transform,
                new Vector3(-25f, stageY - 0.25f, 111f), new Vector3(6f, 0.5f, 14f), matStoneWall);
            CreateWall("WestTech_OuterWall_W", theatreLinks.transform,
                new Vector3(-34f, stageY + 5.75f, 117f), new Vector3(0.5f, 11.5f, 26f), matMahogany);
            CreateWall("WestTech_SouthWall_Closure", theatreLinks.transform,
                new Vector3(-31f, stageY + 5.75f, 104f), new Vector3(6f, 11.5f, 0.5f), matMahogany);

            // 1. Escalera/rampa suave desde el lateral oeste del escenario hacia el Foso de Orquesta (de Y = 2.4m a Y = 1.2m)
            GameObject pitRamp = CreatePrimitive("Escalera_Lateral_Acceso_Foso", PrimitiveType.Cube, theatreLinks.transform,
                new Vector3(-21.0f, 1.75f, 118.8f), new Vector3(2.4f, 0.3f, 5.2f), matTheatreDarkWood);
            pitRamp.transform.localRotation = Quaternion.Euler(14.5f, 0f, 0f);

            // 2. Escalera/rampa oeste desde el escenario hacia el pasillo oeste del Patio de Butacas (de Y = 2.4m a Y = 1.6m)
            GameObject westAudSteps = CreatePrimitive("Escalera_Oeste_A_PatioButacas", PrimitiveType.Cube, theatreLinks.transform,
                new Vector3(-24.2f, 1.95f, 120.5f), new Vector3(3.2f, 0.3f, 4.8f), matTheatreDarkWood);
            westAudSteps.transform.localRotation = Quaternion.Euler(10.0f, 0f, 0f);

            // 3. Escalera/rampa este desde el hombro este del escenario hacia el pasillo este del Patio de Butacas (X: 6..10, Z: 114..122)
            CreatePrimitive("EastStage_Landing", PrimitiveType.Cube, theatreLinks.transform,
                new Vector3(8f, stageY - 0.25f, 116f), new Vector3(4f, 0.5f, 4f), matTheatreDarkWood);
            GameObject eastAudSteps = CreatePrimitive("Escalera_Este_A_PatioButacas", PrimitiveType.Cube, theatreLinks.transform,
                new Vector3(8f, 1.95f, 120.0f), new Vector3(3.6f, 0.3f, 4.8f), matTheatreDarkWood);
            eastAudSteps.transform.localRotation = Quaternion.Euler(10.0f, 0f, 0f);
            // Muro este de EastStage_Landing (X = 10, Z: 114..122) dividido para dejar el vano oculto en Z: 114.9..117.1 (Acceso a Zona 36: Escalera Oculta)
            CreateWall("EastStage_OuterWall_E_South", theatreLinks.transform,
                new Vector3(10f, stageY + 1.7f, 114.45f), new Vector3(0.5f, 3.4f, 0.9f), matMahogany);
            CreateWall("EastStage_OuterWall_E_North", theatreLinks.transform,
                new Vector3(10f, stageY + 1.7f, 119.55f), new Vector3(0.5f, 3.4f, 4.9f), matMahogany);
            CreateWall("EastStage_OuterWall_E_Upper", theatreLinks.transform,
                new Vector3(10f, stageY + 7.45f, 118f), new Vector3(0.5f, 8.1f, 8f), matMahogany);
            CreateWall("EastStage_SouthClosure", theatreLinks.transform,
                new Vector3(8f, stageY + 5.75f, 114f), new Vector3(4f, 11.5f, 0.5f), matMahogany);

            // 4. Escalera técnica de dos tramos hacia la Cabina de Iluminación y la Pasarela Superior (de Y = 2.4m a Y = 7.6m)
            // Tramo 1 (X = -29.5, Z: 106 -> 116, sube de Y = 2.4m a Y = 5.0m)
            GameObject stairFlight1 = CreatePrimitive("Escalera_Cabina_Tramo1", PrimitiveType.Cube, theatreLinks.transform,
                new Vector3(-29.5f, 3.65f, 110.5f), new Vector3(2.8f, 0.35f, 9.6f), matCatwalkSteel);
            stairFlight1.transform.localRotation = Quaternion.Euler(-15.8f, 0f, 0f);
            // Descansillo intermedio (X: -33.5 a -27.5, Z: 115 a 119, Y = 5.0m)
            CreatePrimitive("Escalera_Cabina_Descansillo", PrimitiveType.Cube, theatreLinks.transform,
                new Vector3(-30.8f, 4.85f, 117.0f), new Vector3(5.8f, 0.35f, 4.2f), matCatwalkSteel);
            // Tramo 2 (X = -32.0, Z: 115 -> 105, o hacia Z = 120 a Y = 7.6m)
            GameObject stairFlight2 = CreatePrimitive("Escalera_Cabina_Tramo2", PrimitiveType.Cube, theatreLinks.transform,
                new Vector3(-31.8f, 6.25f, 111.0f), new Vector3(2.6f, 0.35f, 9.6f), matCatwalkSteel);
            stairFlight2.transform.localRotation = Quaternion.Euler(15.8f, 0f, 0f);
            // Plataforma superior de llegada en Y = 7.6m (X: -33.8 a -22.5, Z: 104 a 118) que une directamente la Pasarela (Z = 104..118) y la Cabina (Z = 118..132)
            CreatePrimitive("Upper_Landing_To_BoothAndCatwalk", PrimitiveType.Cube, theatreLinks.transform,
                new Vector3(-28.0f, upperY - 0.20f, 105.5f), new Vector3(11.0f, 0.40f, 3.6f), matCatwalkSteel);
            CreatePrimitive("Upper_Corridor_To_LightingBooth", PrimitiveType.Cube, theatreLinks.transform,
                new Vector3(-24.2f, upperY - 0.20f, 111.5f), new Vector3(3.6f, 0.40f, 13.0f), matCatwalkSteel);

            // ====================================================================
            // ZONA 33: FOSO DE ORQUESTA (X: -20 a +4, Z: 116 a 122, Y: 1.2m)
            // ====================================================================
            GameObject zone33 = new GameObject("33_Orchestra_Pit");
            zone33.transform.parent = rootEnv;

            CreatePrimitive("OrchestraPit_Floor", PrimitiveType.Cube, zone33.transform,
                new Vector3(-8f, pitY - 0.25f, 119f), new Vector3(24f, 0.5f, 6f), matTheatreDarkWood);
            // Murete de antepecho de caoba y latón que separa el foso del patio de butacas (Z = 122)
            CreateWall("OrchestraPit_Parapet_North", zone33.transform,
                new Vector3(-8f, pitY + 0.75f, 122f), new Vector3(24f, 1.5f, 0.35f), matMahogany);
            DecoPiece("OrchestraPit_BrassRail", PrimitiveType.Cube, zone33.transform,
                new Vector3(-8f, pitY + 1.55f, 122f), new Vector3(24f, 0.08f, 0.42f), matGold);
            CreateWall("OrchestraPit_Wall_East", zone33.transform,
                new Vector3(4f, pitY + 0.75f, 119f), new Vector3(0.4f, 1.5f, 6f), matMahogany);

            // Atriles antiguos, sillas de músicos, percusión y partituras deterioradas en el foso
            float[] musicianX = { -15.5f, -12.0f, -4.0f, -0.5f, 2.2f };
            for (int m = 0; m < musicianX.Length; m++)
            {
                float mx = musicianX[m];
                CreateMusicStand($"Atril_Foso_Orquesta_{m + 1}", zone33.transform,
                    new Vector3(mx, pitY, 118.6f), Quaternion.Euler(0f, 180f, 0f), matIron, matMahogany);
                CreatePrimitive($"Silla_Musico_Foso_{m + 1}", PrimitiveType.Cube, zone33.transform,
                    new Vector3(mx, pitY + 0.45f, 119.6f), new Vector3(0.65f, 0.90f, 0.65f), matMahogany);
            }

            // Elemento de percusión en el extremo este del foso
            CreatePrimitive("Timbal_Foso_Orquesta", PrimitiveType.Cylinder, zone33.transform,
                new Vector3(2.2f, pitY + 0.48f, 117.5f), new Vector3(0.88f, 0.48f, 0.88f), matGold);

            // Espacio vacío donde debería encontrarse el violonchelo solista (retirado al Depósito de Instrumentos)
            DecoPiece("Marca_Espacio_Vacio_Violonchelo_Solista", PrimitiveType.Cylinder, zone33.transform,
                new Vector3(-6.2f, pitY + 0.02f, 118.8f), new Vector3(0.95f, 0.02f, 0.95f), matGold);
            CreateMusicStand("Atril_Solista_Violonchelo_Ausente", zone33.transform,
                new Vector3(-6.2f, pitY, 118.2f), Quaternion.Euler(0f, 165f, 0f), matGold, matMahogany);

            // Atril principal del director de orquesta con marcas de uso e interacción de Fase 1
            GameObject conductorPod = CreatePrimitive("Podio_Director_Foso", PrimitiveType.Cube, zone33.transform,
                new Vector3(-8f, pitY + 0.12f, 120.2f), new Vector3(1.6f, 0.24f, 1.4f), matMahogany);
            CreateMusicStand("Atril_Principal_Director", conductorPod.transform,
                new Vector3(0f, 0.12f, -0.35f), Quaternion.Euler(0f, 180f, 0f), matGold, matGold);

            TheatreClimaxController phase1PitCtrl = conductorPod.AddComponent<TheatreClimaxController>();
            phase1PitCtrl.Configure(TheatreClimaxNodeType.InteractivePhase1OrchestraStand);
            EditorUtility.SetDirty(phase1PitCtrl);

            // ANOMALÍA AUDITIVA ZONA 33: Instrumentos que parecen sonar desde posiciones vacías y nota repetida
            GameObject pitAnomalyHost = new GameObject("Anomaly_OrchestraPit_PhantomNotes");
            pitAnomalyHost.transform.parent = zone33.transform;
            pitAnomalyHost.transform.position = new Vector3(-6.2f, pitY + 1.0f, 118.8f);
            TheatreClimaxController anom33 = pitAnomalyHost.AddComponent<TheatreClimaxController>();
            anom33.Configure(TheatreClimaxNodeType.AnomalyOrchestraPitPhantomMusic);
            EditorUtility.SetDirty(anom33);

            // Iluminación fría tenue procedente del escenario sobre los atriles del foso
            Light pitSpillLight = CreateSubtleAnomalyLight("Luz_Tenue_Foso_Orquesta", zone33.transform,
                new Vector3(-8f, pitY + 2.2f, 118.5f), new Color(0.65f, 0.80f, 0.98f), 1.9f);
            pitSpillLight.range = 11f;

            // DOCUMENTOS DE LA ZONA 33 (FOSO DE ORQUESTA)
            CreateInteractiveWorldDoc("Doc_Carpeta_Incompleta_Foso", zone33.transform,
                new Vector3(-6.2f, pitY + 0.05f, 118.8f), new Vector3(0.38f, 0.04f, 0.28f), matProgramPaper,
                "doc_s7_pit_empty_cello_space",
                "Carpeta Musical Incompleta y Espacio Vacío del Violonchelo Principal",
                "FOSO DE ORQUESTA — PUESTO DEL PRIMER VIOLONCHELO\n\n" +
                "En el suelo de madera quedan las marcas del punzón del violonchelo solista, pero ni la silla ni el instrumento están aquí: es el mismo violonchelo cubierto con tela negra que encontraste en el Depósito de Instrumentos (retirado el 13 de octubre).\n\n" +
                "En la carpeta caída al pie del atril se lee:\n" +
                "«Sin el Do Natural del primer violonchelo en el compás 48, Elena no sabrá que han adelantado la caída del telón de contrapeso. Interactúa con el **Podio del Director ([E])** en el centro del foso para reconstruir la secuencia musical»");

            // ====================================================================
            // ZONA 31: PATIO DE BUTACAS (X: -26 a +10, Z: 122 a 150, Y: 1.6m, Techo: 13.9m)
            // ====================================================================
            GameObject zone31 = new GameObject("31_Auditorium_Seating");
            zone31.transform.parent = rootEnv;

            CreatePrimitive("Auditorium_Floor", PrimitiveType.Cube, zone31.transform,
                new Vector3(-8f, audY - 0.25f, 136f), new Vector3(36f, 0.5f, 28f), matParquet);
            CreatePrimitive("Auditorium_CenterCarpet", PrimitiveType.Cube, zone31.transform,
                new Vector3(-8f, audY + 0.02f, 136f), new Vector3(3.4f, 0.04f, 26f), matCarpet);
            CreatePrimitive("Auditorium_Roof", PrimitiveType.Cube, zone31.transform,
                new Vector3(-8f, roofY, 136f), new Vector3(36f, 0.5f, 28f), matWallIvory);

            // Muros perimetrales ornamentados del Patio de Butacas
            CreateWall("Auditorium_Wall_North_Back", zone31.transform,
                new Vector3(-8f, audY + 6.15f, 150f), new Vector3(36f, 12.3f, 0.6f), matWallIvory);
            CreateWall("Auditorium_Wall_West", zone31.transform,
                new Vector3(-26f, audY + 6.15f, 136f), new Vector3(0.6f, 12.3f, 28f), matWallIvory);
            CreateWall("Auditorium_Wall_East", zone31.transform,
                new Vector3(10f, audY + 6.15f, 136f), new Vector3(0.6f, 12.3f, 28f), matWallIvory);

            // Balcones laterales clásicos con escaleras de acceso desde el fondo del auditorio
            CreatePrimitive("Balcon_Lateral_Oeste_Piso", PrimitiveType.Cube, zone31.transform,
                new Vector3(-23.8f, audY + 3.4f, 135f), new Vector3(3.8f, 0.35f, 18f), matMahogany);
            CreatePrimitive("Balcon_Lateral_Oeste_Baranda", PrimitiveType.Cube, zone31.transform,
                new Vector3(-22.0f, audY + 4.1f, 135f), new Vector3(0.25f, 1.1f, 18f), matGold);
            GameObject westBalconyStair = CreatePrimitive("Escalera_Balcon_Oeste", PrimitiveType.Cube, zone31.transform,
                new Vector3(-23.8f, audY + 1.7f, 146.5f), new Vector3(2.4f, 0.3f, 6.2f), matMahogany);
            westBalconyStair.transform.localRotation = Quaternion.Euler(-28f, 0f, 0f);

            CreatePrimitive("Balcon_Lateral_Este_Piso", PrimitiveType.Cube, zone31.transform,
                new Vector3(7.8f, audY + 3.4f, 135f), new Vector3(3.8f, 0.35f, 18f), matMahogany);
            CreatePrimitive("Balcon_Lateral_Este_Baranda", PrimitiveType.Cube, zone31.transform,
                new Vector3(6.0f, audY + 4.1f, 135f), new Vector3(0.25f, 1.1f, 18f), matGold);
            GameObject eastBalconyStair = CreatePrimitive("Escalera_Balcon_Este", PrimitiveType.Cube, zone31.transform,
                new Vector3(7.8f, audY + 1.7f, 146.5f), new Vector3(2.4f, 0.3f, 6.2f), matMahogany);
            eastBalconyStair.transform.localRotation = Quaternion.Euler(-28f, 0f, 0f);

            // Filas de butacas antiguas de terciopelo borgoña orientadas hacia el escenario (-Z)
            // Dejando pasillo central amplio (X = -8) y pasillos laterales transitables
            GameObject phantomSpectatorsGroup = new GameObject("Phantom_Spectators_Group");
            phantomSpectatorsGroup.transform.parent = zone31.transform;
            phantomSpectatorsGroup.transform.position = new Vector3(-8f, audY, 134f);

            for (int row = 0; row < 6; row++)
            {
                float rz = 125.5f + row * 3.4f;
                // Bloque oeste de butacas (X: -19 a -11) y bloque este (X: -5 a +3)
                float[] bankCenterX = { -15.0f, -1.0f };
                for (int b = 0; b < bankCenterX.Length; b++)
                {
                    float bx = bankCenterX[b];
                    bool displaced = (row == 2 && b == 1);
                    float yaw = displaced ? 14f : 0f;
                    CreateAuditoriumSeatRow($"Fila_Butacas_R{row + 1}_B{b + 1}", zone31.transform,
                        new Vector3(bx, audY, rz), Quaternion.Euler(0f, yaw, 0f), 6.8f,
                        (row == 4 && b == 0) ? matClothCover : matVelvetDrapes, matMahogany, matGold);
                }

                // En las filas 2 y 3 añadimos siluetas sentadas inmóviles que solo se ven desde el escenario
                if (row == 2 || row == 3)
                {
                    float sx = (row == 2) ? -14.2f : -1.8f;
                    DecoPiece($"Espectador_Inmovil_Torso_{row}", PrimitiveType.Capsule, phantomSpectatorsGroup.transform,
                        new Vector3(sx + 8f, 0.95f, rz - 134f), new Vector3(0.38f, 0.42f, 0.28f), matShadowFigure);
                    DecoPiece($"Espectador_Inmovil_Cabeza_{row}", PrimitiveType.Sphere, phantomSpectatorsGroup.transform,
                        new Vector3(sx + 8f, 1.52f, rz - 134f), new Vector3(0.24f, 0.26f, 0.24f), matShadowFigure);
                }
            }

            // ANOMALÍA PSICOLÓGICA ZONA 31: Figuras inmóviles en las butacas vistas desde el escenario + aplausos lejanos
            GameObject auditoriumAnomalyHost = new GameObject("Anomaly_Auditorium_SpectatorsAndApplause");
            auditoriumAnomalyHost.transform.parent = zone31.transform;
            auditoriumAnomalyHost.transform.position = new Vector3(-8f, audY + 1.0f, 134f);
            TheatreClimaxController anom31 = auditoriumAnomalyHost.AddComponent<TheatreClimaxController>();
            anom31.Configure(TheatreClimaxNodeType.AnomalyAuditoriumSpectatorsAndApplause, phantomSpectatorsGroup.transform);
            EditorUtility.SetDirty(anom31);

            // Lámparas de araña parcialmente apagadas en el techo alto del auditorio
            CreateChandelier("Arana_Patio_Butacas_1", zone31.transform, new Vector3(-14f, roofY - 1.2f, 132f), matGold, new Color(1f, 0.78f, 0.45f), 1.4f);
            CreateChandelier("Arana_Patio_Butacas_2", zone31.transform, new Vector3(-2f, roofY - 1.2f, 140f), matGold, new Color(1f, 0.78f, 0.45f), 1.2f);

            // DOCUMENTOS DE LA ZONA 31 (PATIO DE BUTACAS)
            CreateInteractiveWorldDoc("Doc_Programa_Mano_Ultima_Funcion", zone31.transform,
                new Vector3(-11.2f, audY + 0.56f, 128.9f), new Vector3(0.38f, 0.04f, 0.28f), matProgramPaper,
                "doc_s7_auditorium_handbill",
                "Programa de Mano Antiguo — Gran Estreno del 14 de Octubre de 1924",
                "GRAN TEATRO DE LA ACADEMIA VESPER — PROGRAMA OFICIAL\n\n" +
                "«EL LAGO DE LAS SOMBRAS — Ballet en Tres Actos»\n" +
                "• Actos I y II (Primera Bailarina): **Elena Vesper**.\n" +
                "• Acto III — El Cierre del Lago: Se observa una tira de papel impresa pegada a última hora sobre el nombre de Elena con el nombre de **Mara Bell**.\n\n" +
                "Al comparar este programa con las cintas del Archivo Musical, compruebas que el cambio de bailarina en el Acto III ya estaba impreso ANTES de que comenzara la función en la que supuestamente Elena desapareció de forma imprevista.");

            CreateInteractiveWorldDoc("Doc_Entrada_Ultima_Funcion_1924", zone31.transform,
                new Vector3(-4.5f, audY + 0.56f, 135.7f), new Vector3(0.32f, 0.04f, 0.22f), matGold,
                "doc_s7_auditorium_ticket_stub",
                "Entrada de Palco y Anotación de un Testigo entre las Butacas",
                "FILA 4, BUTACA 12 — 14 DE OCTUBRE DE 1924 (20:00 H)\n\n" +
                "En el reverso de la entrada alguien escribió con lápiz apresurado antes de desalojar la sala:\n" +
                "«En mitad del tercer acto, la música del foso tocó un acorde discordante y los focos de la Cabina de Iluminación apagaron de golpe el fondo del escenario. Escuchamos un golpe seco de contrapesos detrás del telón intermedio y un grito ahogado, pero el director obligó a la orquesta a tocar más fuerte mientras salía la segunda bailarina»");

            // ====================================================================
            // ZONA 34: CABINA DE ILUMINACIÓN (X: -34 a -22, Z: 118 a 132, Y: 7.6m)
            // ====================================================================
            GameObject zone34 = new GameObject("34_Lighting_Booth");
            zone34.transform.parent = rootEnv;
            float boothH = 4.2f;

            CreatePrimitive("LightingBooth_Floor", PrimitiveType.Cube, zone34.transform,
                new Vector3(-28f, upperY - 0.20f, 125f), new Vector3(12f, 0.40f, 14f), matStoneWall);
            CreatePrimitive("LightingBooth_Ceiling", PrimitiveType.Cube, zone34.transform,
                new Vector3(-28f, upperY + boothH, 125f), new Vector3(12f, 0.40f, 14f), matMahogany);

            CreateWall("LightingBooth_Wall_North", zone34.transform,
                new Vector3(-28f, upperY + 2.1f, 132f), new Vector3(12f, boothH, 0.4f), matMahogany);
            CreateWall("LightingBooth_Wall_West", zone34.transform,
                new Vector3(-34f, upperY + 2.1f, 125f), new Vector3(0.4f, boothH, 14f), matMahogany);
            // Muro sur (Z = 118) con abertura amplia de paso hacia la escalera técnica y la Pasarela Superior (Zona 35)
            CreateWall("LightingBooth_Wall_South_W", zone34.transform,
                new Vector3(-31.5f, upperY + 2.1f, 118f), new Vector3(5.0f, boothH, 0.4f), matMahogany);

            // Gran ventanal panorámico orientado hacia el escenario, el foso y el patio de butacas (X = -22, Z: 118 a 132)
            CreateWall("LightingBooth_Window_LowWall", zone34.transform,
                new Vector3(-22.2f, upperY + 0.55f, 125f), new Vector3(0.35f, 1.1f, 14f), matMahogany);
            CreateWall("LightingBooth_Window_HighHeader", zone34.transform,
                new Vector3(-22.2f, upperY + 3.65f, 125f), new Vector3(0.35f, 1.1f, 14f), matMahogany);

            // Consola de iluminación antigua con interruptores físicos, reguladores de intensidad y cableado
            GameObject lightConsole = CreatePrimitive("Consola_Iluminacion_Principal", PrimitiveType.Cube, zone34.transform,
                new Vector3(-23.4f, upperY + 0.52f, 124.5f), new Vector3(1.5f, 1.04f, 4.6f), matMetalArchive);
            for (int sw = 0; sw < 5; sw++)
            {
                float sz = -1.6f + sw * 0.8f;
                DecoPiece($"Regulador_Intensidad_{sw + 1}", PrimitiveType.Cube, lightConsole.transform,
                    new Vector3(0.15f, 0.60f, sz), new Vector3(0.28f, 0.22f, 0.08f), matGold).transform.localRotation = Quaternion.Euler(0f, 0f, -22f + sw * 8f);
            }

            // Focos teatrales controlados desde la Cabina de Iluminación (Fase 3 + Anomalía de foco autónomo)
            Light boothSpot1 = CreatePictureSpotlight("Foco_Cabina_Hacia_ArcoLago", zone34.transform,
                new Vector3(-21.8f, upperY + 2.2f, 122.0f), new Vector3(-8f, stageY + 1.2f, 101.5f), new Color(1.0f, 0.86f, 0.55f), 4.0f);
            Light boothSpot2 = CreatePictureSpotlight("Foco_Cabina_Hacia_Proscenio", zone34.transform,
                new Vector3(-21.8f, upperY + 2.2f, 126.0f), new Vector3(-8f, stageY + 0.5f, 110.0f), new Color(0.65f, 0.85f, 1.0f), 3.5f);

            TheatreClimaxController phase3BoothCtrl = lightConsole.AddComponent<TheatreClimaxController>();
            phase3BoothCtrl.Configure(TheatreClimaxNodeType.InteractivePhase3LightingConsole, null, null, boothSpot1, boothSpot2, hiddenUpstageMark);
            EditorUtility.SetDirty(phase3BoothCtrl);

            // ANOMALÍA DE ILUMINACIÓN ZONA 34: Foco que se enciende brevemente solo revelando detalles ocultos en el escenario
            GameObject boothAnomalyHost = new GameObject("Anomaly_LightingBooth_AutonomousSpot");
            boothAnomalyHost.transform.parent = zone34.transform;
            boothAnomalyHost.transform.position = new Vector3(-23.4f, upperY + 1.5f, 124.5f);
            TheatreClimaxController anom34 = boothAnomalyHost.AddComponent<TheatreClimaxController>();
            anom34.Configure(TheatreClimaxNodeType.AnomalyLightingBoothAutonomousSpot, null, null, boothSpot1, boothSpot2, hiddenUpstageMark);
            EditorUtility.SetDirty(anom34);

            CreateColdFluorescentFixture("Luz_Interna_Cabina_Iluminacion", zone34.transform,
                new Vector3(-28f, upperY + 3.8f, 125f), matIron, matLanternGlow, 1.7f);

            // DOCUMENTO DE LA ZONA 34 (CABINA DE ILUMINACIÓN)
            CreateInteractiveWorldDoc("Doc_Plano_Iluminacion_Ultima_Funcion", zone34.transform,
                new Vector3(-23.4f, upperY + 1.08f, 126.2f), new Vector3(0.45f, 0.04f, 0.34f), matProgramPaper,
                "doc_s7_lighting_booth_plot",
                "Plano de Iluminación de la Última Representación (Anotado a Mano)",
                "CABINA TÉCNICA DE ILUMINACIÓN — SECUENCIA DE LUCES DEL ACTO III\n\n" +
                "• **Guion Oficial Impreso**: Mantener encendido el Circuito I (Proscenio) y apagar por completo el fondo del escenario en el compás 48.\n" +
                "• **Corrección Manuscrita del Técnico de Luces**:\n" +
                "  «Me ordenaron dejar a oscuras el Arco del Lago en el compás 48 para que el público no viera caer el bastidor de contrapeso sobre la posición de Elena. Si activas la **Consola de Iluminación ([E])** en el orden original (Proscenio -> Lago -> Contraluz del Arco), la luz revelará desde aquí arriba la estructura oculta detrás del telón falso».");

            // ====================================================================
            // ZONA 35: PASARELA SUPERIOR DEL TEATRO (Y: 7.6m sobre el perímetro del Escenario Principal)
            // ====================================================================
            GameObject zone35 = new GameObject("35_Upper_Catwalk");
            zone35.transform.parent = rootEnv;

            // 1. Tramo Oeste de la Pasarela Superior (X: -22.4 a -19.2, Z: 98.5 a 115.5, Y: 7.6m) conectado con la Cabina de Iluminación
            CreatePrimitive("Catwalk_West_Deck", PrimitiveType.Cube, zone35.transform,
                new Vector3(-20.8f, upperY - 0.15f, 107.0f), new Vector3(3.2f, 0.30f, 17.0f), matCatwalkSteel);
            CreatePrimitive("Catwalk_West_InnerRail", PrimitiveType.Cube, zone35.transform,
                new Vector3(-19.25f, upperY + 0.65f, 107.0f), new Vector3(0.12f, 1.30f, 13.8f), matIron);

            // 2. Puente Norte sobre el Proscenio y Telones (X: -19.2 a +4.5, Z: 112.5 a 115.5, Y: 7.6m)
            CreatePrimitive("Catwalk_NorthBridge_Deck", PrimitiveType.Cube, zone35.transform,
                new Vector3(-7.35f, upperY - 0.15f, 114.0f), new Vector3(23.7f, 0.30f, 3.0f), matCatwalkSteel);
            CreatePrimitive("Catwalk_NorthBridge_SouthRail", PrimitiveType.Cube, zone35.transform,
                new Vector3(-7.35f, upperY + 0.65f, 112.55f), new Vector3(23.7f, 1.30f, 0.12f), matIron);
            CreatePrimitive("Catwalk_NorthBridge_NorthRail", PrimitiveType.Cube, zone35.transform,
                new Vector3(-7.35f, upperY + 0.65f, 115.45f), new Vector3(23.7f, 1.30f, 0.12f), matIron);

            // 3. Pasarela Posterior sobre el Arco del Lago y Peine de Poleas (X: -19.2 a +4.5, Z: 98.5 a 101.5, Y: 7.6m)
            CreatePrimitive("Catwalk_RearBridge_Deck", PrimitiveType.Cube, zone35.transform,
                new Vector3(-7.35f, upperY - 0.15f, 100.0f), new Vector3(23.7f, 0.30f, 3.0f), matCatwalkSteel);
            CreatePrimitive("Catwalk_RearBridge_NorthRail", PrimitiveType.Cube, zone35.transform,
                new Vector3(-7.35f, upperY + 0.65f, 101.45f), new Vector3(23.7f, 1.30f, 0.12f), matIron);
            CreatePrimitive("Catwalk_RearBridge_SouthRail", PrimitiveType.Cube, zone35.transform,
                new Vector3(-7.35f, upperY + 0.65f, 98.55f), new Vector3(23.7f, 1.30f, 0.12f), matIron);

            // Cuerdas, poleas superiores y puntos de suspensión de los decorados visibles desde la pasarela
            for (int p = 0; p < 5; p++)
            {
                float px = -16.0f + p * 4.2f;
                DecoPiece($"Pasarela_Polea_Suspension_{p + 1}", PrimitiveType.Cylinder, zone35.transform,
                    new Vector3(px, upperY + 1.4f, 102.2f), new Vector3(0.55f, 0.08f, 0.55f), matGold).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                DecoPiece($"Pasarela_Cable_Telon_{p + 1}", PrimitiveType.Cylinder, zone35.transform,
                    new Vector3(px, upperY - 1.2f, 102.2f), new Vector3(0.04f, 2.6f, 0.04f), matIron);
            }

            // FASE 4 DE LA SECUENCIA FINAL: Control accesible en la Pasarela Superior para elevar el telón falso del Lago
            GameObject catwalkWinchControl = CreatePrimitive("Control_Mecanismo_Pasarela_Superior", PrimitiveType.Cube, zone35.transform,
                new Vector3(-8.0f, upperY + 0.55f, 101.25f), new Vector3(1.4f, 1.1f, 0.75f), matIron);
            DecoPiece("Volante_Laton_Pasarela", PrimitiveType.Cylinder, catwalkWinchControl.transform,
                new Vector3(0f, 0.58f, 0f), new Vector3(0.55f, 0.06f, 0.55f), matGold);

            TheatreClimaxController phase4CatwalkCtrl = catwalkWinchControl.AddComponent<TheatreClimaxController>();
            phase4CatwalkCtrl.Configure(TheatreClimaxNodeType.InteractivePhase4CatwalkRigging, falseMidStageCurtain.transform);
            EditorUtility.SetDirty(phase4CatwalkCtrl);

            // ANOMALÍA DE ALTURA Y PERSPECTIVA ZONA 35
            GameObject catwalkAnomalyHost = new GameObject("Anomaly_Catwalk_PerspectiveShift");
            catwalkAnomalyHost.transform.parent = zone35.transform;
            catwalkAnomalyHost.transform.position = new Vector3(-8.0f, upperY + 1.0f, 114.0f);
            TheatreClimaxController anom35 = catwalkAnomalyHost.AddComponent<TheatreClimaxController>();
            anom35.Configure(TheatreClimaxNodeType.AnomalyCatwalkPerspectiveEcho, stageBallerinaFig.transform);
            EditorUtility.SetDirty(anom35);

            CreateColdFluorescentFixture("Luz_Fria_Pasarela_Superior_1", zone35.transform,
                new Vector3(-8f, upperY + 3.4f, 114.0f), matIron, matColdFluorescent, 1.6f);
            CreateColdFluorescentFixture("Luz_Fria_Pasarela_Superior_2", zone35.transform,
                new Vector3(-8f, upperY + 3.4f, 100.0f), matIron, matColdFluorescent, 1.6f);

            // DOCUMENTO DE LA ZONA 35 (PASARELA SUPERIOR DEL TEATRO)
            CreateInteractiveWorldDoc("Doc_Etiquetas_Suspension_Pasarela", zone35.transform,
                new Vector3(-9.3f, upperY + 0.72f, 101.25f), new Vector3(0.40f, 0.04f, 0.28f), matProgramPaper,
                "doc_s7_catwalk_rigging_tags",
                "Etiquetas de Suspensión Escénica — Conexión con la Sala de Poleas",
                "PASARELA DE MANTENIMIENTO SUPERIOR — PEINE DE BAMBALINAS\n\n" +
                "Las etiquetas metálicas remachadas a la barandilla coinciden con el plano de la Sala de Mecanismos y Poleas (Zona 30):\n" +
                "• **Vara Superior Nº 3 (Telón Secundario del Lago)**: «Una vez liberado el Contrapeso III en la Sala de Poleas e iluminado el fondo desde la Cabina de Luces, acciona el **Control de Suspensión de esta Pasarela ([E])** para elevar el telón falso y dejar al descubierto el Arco Original de Elena Vesper».");
        }

        private static void CreateStageTreeProp(string name, Transform parent, Vector3 pos, Material trunkMat, Material canopyMat)
        {
            GameObject tree = new GameObject(name);
            tree.transform.parent = parent;
            tree.transform.position = pos;

            CreatePrimitive("Tree_Trunk", PrimitiveType.Cylinder, tree.transform, new Vector3(0f, 2.2f, 0f), new Vector3(0.42f, 2.2f, 0.42f), trunkMat);
            DecoPiece("Tree_Branch_L", PrimitiveType.Cube, tree.transform, new Vector3(-0.75f, 3.6f, 0f), new Vector3(1.6f, 0.18f, 0.14f), trunkMat).transform.localRotation = Quaternion.Euler(0f, 0f, 28f);
            DecoPiece("Tree_Branch_R", PrimitiveType.Cube, tree.transform, new Vector3(0.75f, 3.8f, 0f), new Vector3(1.6f, 0.18f, 0.14f), trunkMat).transform.localRotation = Quaternion.Euler(0f, 0f, -24f);
            DecoPiece("Tree_DarkFoliageFlat", PrimitiveType.Cube, tree.transform, new Vector3(0f, 4.3f, 0f), new Vector3(2.8f, 1.4f, 0.12f), canopyMat);
        }

        private static void CreateAuditoriumSeatRow(string name, Transform parent, Vector3 pos, Quaternion rot, float rowWidth, Material velvetMat, Material woodMat, Material brassMat)
        {
            GameObject row = new GameObject(name);
            row.transform.parent = parent;
            row.transform.position = pos;
            row.transform.rotation = rot;

            // Banco corrido de butacas tapizadas en terciopelo borgoña con respaldos y apoyabrazos de madera oscura
            CreatePrimitive("Seats_CushionBase", PrimitiveType.Cube, row.transform, new Vector3(0f, 0.28f, 0f), new Vector3(rowWidth, 0.26f, 0.62f), velvetMat);
            CreatePrimitive("Seats_Backrest", PrimitiveType.Cube, row.transform, new Vector3(0f, 0.66f, 0.24f), new Vector3(rowWidth, 0.62f, 0.14f), velvetMat);
            DecoPiece("Seats_WoodTopRail", PrimitiveType.Cube, row.transform, new Vector3(0f, 0.99f, 0.24f), new Vector3(rowWidth + 0.08f, 0.06f, 0.16f), woodMat);
            DecoPiece("Seats_SideArm_L", PrimitiveType.Cube, row.transform, new Vector3(-rowWidth * 0.5f, 0.45f, 0f), new Vector3(0.10f, 0.55f, 0.66f), woodMat);
            DecoPiece("Seats_SideArm_R", PrimitiveType.Cube, row.transform, new Vector3(rowWidth * 0.5f, 0.45f, 0f), new Vector3(0.10f, 0.55f, 0.66f), woodMat);
            DecoPiece("Seats_BrassNumberPlate", PrimitiveType.Cube, row.transform, new Vector3(-rowWidth * 0.5f - 0.06f, 0.58f, 0f), new Vector3(0.02f, 0.10f, 0.16f), brassMat);
        }

        // ====================================================================
        // CONSTRUCCIÓN DE LA SECCIÓN 8: ZONA SECRETA — LA VERDAD OCULTA (Zonas 36 a 39)
        // 36. Escalera oculta (X: 10.0 a 29.0, Z: 114.2 a 117.8, desciende de Y: 2.4m a Y: -3.2m)
        // 37. Depósito clausurado (X: 29.0 a 42.0, Z: 114.2 a 126.5, Y: -3.2m)
        // 38. Sala de archivos prohibidos (X: 16.0 a 42.0, Z: 126.5 a 138.5, Y: -3.2m)
        // 39. Cámara tras el escenario (X: 6.0 a 10.6, Z: 98.5 a 107.0, Y: 2.4m, tras el muro este del Escenario)
        // ====================================================================
        private static void BuildSection8SecretZone(
            Transform rootEnv, float vestY,
            Material matParquet, Material matCarpet, Material matWallIvory, Material matMahogany,
            Material matGold, Material matIron, Material matClothCover, Material matVelvetDrapes,
            Material matLanternGlow, Material matColdFluorescent, Material matGreenVelvet,
            Material matLeather, Material matWoodArchive, Material matMetalArchive, Material matCardboard,
            Material matStoneWall, Material matRustedLocker, Material matMirrorGlass, Material matPorcelain,
            Material matTutuWhite, Material matTutuBlack, Material matSatinRibbon, Material matShadowFigure,
            Material matPortraitElena, Material matPhotoTroupe1923)
        {
            Material matAgedBrick = GetOrCreateBasicMaterial("Mat_S8_AgedBrickWall", new Color(0.26f, 0.16f, 0.14f), 0.18f, 0.02f);
            Material matDampPlaster = GetOrCreateBasicMaterial("Mat_S8_DampPlaster", new Color(0.22f, 0.23f, 0.21f), 0.15f, 0.0f);
            Material matDarkCement = GetOrCreateBasicMaterial("Mat_S8_SubterraneanCement", new Color(0.17f, 0.18f, 0.19f), 0.22f, 0.04f);
            Material matSecretPaper = GetOrCreateBasicMaterial("Mat_S8_ArchivalPaper", new Color(0.84f, 0.77f, 0.60f), 0.16f, 0.0f);

            float stageY = vestY;            // Nivel del Escenario Principal: 2.4m
            float midLandingY = -0.4f;       // Descansillo intermedio de la Escalera Oculta: -0.4m
            float subY = -3.2f;              // Nivel subterráneo del Depósito y Archivos Prohibidos: -3.2m
            float subH = 3.6f;               // Altura libre subterránea (techo en Y = +0.4m, bajo todos los suelos superiores)

            // ====================================================================
            // PISTAS DISTRIBUIDAS EN SECCIONES ANTERIORES PARA DESCUBRIR LA ZONA SECRETA (REF. B-00)
            // ====================================================================
            GameObject distributedCluesGroup = new GameObject("Section8_Distributed_Discovery_Clues");
            distributedCluesGroup.transform.parent = rootEnv;

            // 1. Pista en Administración (Sección 2 — sobre el mostrador de Recepción / mesa de Archivo)
            CreateInteractiveWorldDoc("Doc_Pista_Admin_Ref_B00", distributedCluesGroup.transform,
                new Vector3(-18.6f, vestY + 1.28f, 16.0f), new Vector3(0.34f, 0.04f, 0.24f), matSecretPaper,
                "doc_s8_clue_admin_b00",
                "Memorándum Reservado — Traslado al Depósito Subterráneo B-00",
                "DIRECCIÓN GENERAL — NOTA DE ARCHIVO INTERNO (17 DE OCTUBRE DE 1924)\n\n" +
                "«Por orden de Isadora Vale, toda la correspondencia privada del Patronato y el acta médica real de la noche del 14 de octubre han sido retiradas de este archivo administrativo y depositadas bajo llave en la **Bóveda Estructural B-00**, cuyo acceso se oculta tras el panel de caoba del hombro este del Escenario Principal».");

            // 2. Pista en Archivo Musical (Sección 5 — sobre la Mesa de Lectura 2 de la Biblioteca Musical, Zona 21)
            CreateInteractiveWorldDoc("Doc_Pista_Musica_Ref_B00", distributedCluesGroup.transform,
                new Vector3(19.3f, vestY + 0.91f, 72.0f), new Vector3(0.34f, 0.04f, 0.24f), matSecretPaper,
                "doc_s8_clue_music_b00",
                "Ficha de Expurgo Musical — Cámara Oculta tras el Escenario",
                "ARCHIVO MUSICAL — REGISTRO DE PARTITURAS RETIRADAS\n\n" +
                "«El manuscrito íntegro a dos manos de *El Lago de las Sombras* (anotado conjuntamente por Elena Vesper y Mara Bell) no fue destruido: fue guardado en la **Cámara tras el Escenario**, accesible únicamente descendiendo por la Escalera Oculta (`Ref. B-00`) situada en el lateral este del proscenio».");

            // 3. Pista en Almacén de Decorados (Sección 6 — fijada al muro oeste en X = 10.9m, Zona 29)
            CreateInteractiveWorldDoc("Doc_Pista_Decorados_Ref_B00", distributedCluesGroup.transform,
                new Vector3(10.90f, vestY + 1.45f, 105.0f), new Vector3(0.05f, 0.28f, 0.36f), matSecretPaper,
                "doc_s8_clue_scenery_b00",
                "Marca de los Carpinteros en el Muro Oeste del Almacén",
                "ALMACÉN DE DECORADOS — ANOTACIÓN TRAS EL BASTIDOR OESTE\n\n" +
                "«Detrás de este muro oeste (`X = 10.6m`) existe un vano hueco de cuatro metros y medio que nos prohibieron abrir desde el almacén. Su única entrada baja desde el **panel móvil del descansillo este del escenario (`X = 10, Z = 116`)** y vuelve a subir hasta una cámara privada pegada al escenario principal».");

            // 4. Pista en Sala de Mecanismos y Poleas (Sección 6 — sobre la Mesa de Trabajo en X = -15.5m, Z = 88.5m, Zona 30)
            CreateInteractiveWorldDoc("Doc_Pista_Poleas_Ref_B00", distributedCluesGroup.transform,
                new Vector3(-15.4f, vestY + 0.94f, 88.5f), new Vector3(0.38f, 0.04f, 0.28f), matSecretPaper,
                "doc_s8_clue_pulleys_b00",
                "Anexo Estructural — Plano del Muro Este y Escalera Oculta (B-00)",
                "SALA DE MECANISMOS — PLANO DE CIMIENTOS DEL TEATRO\n\n" +
                "El plano de cimentación muestra una galería subterránea bajo el ala noreste del teatro:\n" +
                "• **Acceso Oculto (`B-00`)**: En el descansillo este entre el Escenario Principal y el Patio de Butacas (`X = 10m, Z = 116m`), un panel de moldura clásica se desliza al presionarlo (`[E]`), revelando la **Escalera Oculta (Zona 36)**.");

            // ====================================================================
            // ZONA 36: ESCALERA OCULTA (X: 10.0 a 29.0, Z: 114.2 a 117.8, desciende de Y = 2.4m a Y = -3.2m)
            // ====================================================================
            GameObject zone36 = new GameObject("36_Hidden_Staircase");
            zone36.transform.parent = rootEnv;

            // Panel móvil disimulado en el muro este del descansillo del escenario (X = 10.0m, Z = 116.0m, Y = 2.4m)
            GameObject secretPanelHost = new GameObject("Panel_Movil_Escalera_Oculta_Host");
            secretPanelHost.transform.parent = zone36.transform;
            secretPanelHost.transform.position = new Vector3(10.0f, stageY, 116.0f);

            GameObject slidingPanelMesh = CreatePrimitive("Panel_Movil_Escalera_Oculta", PrimitiveType.Cube, secretPanelHost.transform,
                new Vector3(0f, 1.7f, 0f), new Vector3(0.32f, 3.35f, 2.25f), matMahogany);
            DecoPiece("Moldura_Disimulada_Panel_Secreto", PrimitiveType.Cube, slidingPanelMesh.transform,
                new Vector3(-0.18f, 0f, 0f), new Vector3(0.06f, 0.82f, 0.82f), matGold);
            DecoPiece("Placa_Resorte_B00", PrimitiveType.Cube, slidingPanelMesh.transform,
                new Vector3(-0.19f, -0.08f, -0.35f), new Vector3(0.04f, 0.08f, 0.14f), matGold);

            SecretZoneAnomalyController panelCtrl = slidingPanelMesh.AddComponent<SecretZoneAnomalyController>();
            panelCtrl.Configure(SecretZoneNodeType.InteractiveSecretPanelEntrance, slidingPanelMesh.transform);
            EditorUtility.SetDirty(panelCtrl);

            // Pista 5 junto al propio panel móvil en el Hombro Este del Escenario (Zona 32 / 36)
            CreateInteractiveWorldDoc("Doc_Inscripcion_Junto_Panel_Secreto", zone36.transform,
                new Vector3(9.55f, stageY + 1.25f, 114.35f), new Vector3(0.28f, 0.24f, 0.05f), matGold,
                "doc_s8_clue_stage_panel_b00",
                "Inscripción Técnica Discreta — Acceso a la Escalera Oculta (B-00)",
                "HOMBRO ESTE DEL ESCENARIO — JUNTO AL PANEL DE CAOBA\n\n" +
                "Grabado en una pequeña chapa de latón junto a la moldura:\n" +
                "«GALERÍA DE INSPECCIÓN Y ARCHIVO RESERVADO B-00 — Presiona el **Panel Móvil de Caoba ([E])** a tu derecha para correr el cierre y descender por la Escalera Oculta».");

            // 1. Rellano superior de entrada tras el panel móvil (X: 10.0 a 12.8, Z: 114.2 a 117.8, Y = 2.4m)
            CreatePrimitive("HiddenStair_TopLanding_Floor", PrimitiveType.Cube, zone36.transform,
                new Vector3(11.4f, stageY - 0.20f, 116.0f), new Vector3(2.8f, 0.40f, 3.6f), matMahogany);
            CreatePrimitive("HiddenStair_TopLanding_Ceiling", PrimitiveType.Cube, zone36.transform,
                new Vector3(14.5f, stageY + 3.4f, 116.0f), new Vector3(9.0f, 0.40f, 3.6f), matAgedBrick);

            // 2. Primer tramo descendente de madera oscura (X: 12.6 a 18.8, desciende de Y = 2.4m a Y = -0.4m)
            GameObject stairFlight1 = CreatePrimitive("HiddenStair_Flight1_Ramp", PrimitiveType.Cube, zone36.transform,
                new Vector3(15.7f, 0.85f, 116.0f), new Vector3(6.8f, 0.35f, 3.2f), matMahogany);
            stairFlight1.transform.localRotation = Quaternion.Euler(0f, 0f, -24.2f);

            // Peldaños visuales de madera oscura sobre el Tramo 1 y barandilla metálica oxidada
            for (int s = 0; s < 8; s++)
            {
                float sx = 13.0f + s * 0.75f;
                float sy = stageY - 0.18f - s * 0.34f;
                DecoPiece($"Peldaño_Madera_T1_{s + 1}", PrimitiveType.Cube, zone36.transform,
                    new Vector3(sx, sy, 116.0f), new Vector3(0.45f, 0.08f, 3.1f), matWoodArchive);
            }
            DecoPiece("Barandilla_Oxidada_Tramo1", PrimitiveType.Cube, zone36.transform,
                new Vector3(15.7f, 1.85f, 114.55f), new Vector3(6.6f, 0.08f, 0.08f), matRustedLocker).transform.localRotation = Quaternion.Euler(0f, 0f, -24.2f);

            // 3. Descansillo intermedio (X: 18.6 a 21.6, Z: 114.2 a 117.8, Y = -0.4m)
            CreatePrimitive("HiddenStair_MidLanding_Floor", PrimitiveType.Cube, zone36.transform,
                new Vector3(20.1f, midLandingY - 0.20f, 116.0f), new Vector3(3.2f, 0.40f, 3.6f), matDarkCement);
            CreatePrimitive("HiddenStair_MidLanding_Ceiling", PrimitiveType.Cube, zone36.transform,
                new Vector3(23.8f, midLandingY + 3.4f, 116.0f), new Vector3(10.5f, 0.40f, 3.6f), matAgedBrick);

            // Placa técnica antigua y manchas de humedad en el descansillo intermedio
            CreateHistoricalPlaque("Placa_Tecnica_Descansillo_B00", zone36.transform,
                new Vector3(20.1f, midLandingY + 1.65f, 117.55f), matRustedLocker);
            DecoPiece("Mancha_Humedad_Escalera_1", PrimitiveType.Cube, zone36.transform,
                new Vector3(19.5f, midLandingY + 1.2f, 114.42f), new Vector3(1.8f, 1.4f, 0.04f), matDampPlaster);

            CreateInteractiveWorldDoc("Doc_Placa_Descansillo_Escalera_Oculta", zone36.transform,
                new Vector3(20.1f, midLandingY + 1.35f, 117.52f), new Vector3(0.42f, 0.28f, 0.06f), matGold,
                "doc_s8_stair_technical_plate",
                "Placa Técnica del Muro Interior — Galería Subterránea B-00",
                "INSCRIPCIÓN TÉCNICA EN EL DESCANSILLO INTERMEDIO (ZONA 36)\n\n" +
                "«MURO DE CARGA ORIENTAL — CONSTRUCCIÓN ORIGINAL DE 1898\n" +
                "Nivel Inferior (-3.2m): Depósito de Custodia (Zona 37) y Sala de Archivos Reservados de Dirección (Zona 38).\n" +
                "Acceso clausurado al alumnado y al público por orden del Patronato el 17 de octubre de 1924».");

            // ANOMALÍA AUDITIVA ZONA 36: Vibraciones apagadas del teatro y sonido de una puerta cerrándose abajo
            GameObject stairAnomalyHost = new GameObject("Anomaly_HiddenStair_DoorShutBelow");
            stairAnomalyHost.transform.parent = zone36.transform;
            stairAnomalyHost.transform.position = new Vector3(20.1f, midLandingY + 1.0f, 116.0f);
            SecretZoneAnomalyController anom36 = stairAnomalyHost.AddComponent<SecretZoneAnomalyController>();
            anom36.Configure(SecretZoneNodeType.AnomalyHiddenStairDoorShut);
            EditorUtility.SetDirty(anom36);

            // 4. Segundo tramo descendente (X: 21.5 a 27.6, desciende de Y = -0.4m a Y = -3.2m)
            GameObject stairFlight2 = CreatePrimitive("HiddenStair_Flight2_Ramp", PrimitiveType.Cube, zone36.transform,
                new Vector3(24.55f, -1.95f, 116.0f), new Vector3(6.8f, 0.35f, 3.2f), matMahogany);
            stairFlight2.transform.localRotation = Quaternion.Euler(0f, 0f, -24.2f);

            for (int s = 0; s < 8; s++)
            {
                float sx = 21.9f + s * 0.75f;
                float sy = midLandingY - 0.18f - s * 0.34f;
                DecoPiece($"Peldaño_Madera_T2_{s + 1}", PrimitiveType.Cube, zone36.transform,
                    new Vector3(sx, sy, 116.0f), new Vector3(0.45f, 0.08f, 3.1f), matWoodArchive);
            }
            DecoPiece("Barandilla_Oxidada_Tramo2", PrimitiveType.Cube, zone36.transform,
                new Vector3(24.55f, -0.95f, 114.55f), new Vector3(6.6f, 0.08f, 0.08f), matRustedLocker).transform.localRotation = Quaternion.Euler(0f, 0f, -24.2f);

            // Rellano inferior de llegada en Y = -3.2m (X: 27.2 a 29.2, Z: 114.2 a 117.8)
            CreatePrimitive("HiddenStair_BottomLanding_Floor", PrimitiveType.Cube, zone36.transform,
                new Vector3(28.2f, subY - 0.20f, 116.0f), new Vector3(2.4f, 0.40f, 3.6f), matDarkCement);

            // Paredes laterales de ladrillo envejecido y yeso deteriorado que encierran toda la Escalera Oculta (X: 10 a 29)
            CreateWall("HiddenStair_Wall_South", zone36.transform,
                new Vector3(19.5f, 1.2f, 114.2f), new Vector3(19.0f, 9.6f, 0.4f), matAgedBrick);
            CreateWall("HiddenStair_Wall_North", zone36.transform,
                new Vector3(19.5f, 1.2f, 117.8f), new Vector3(19.0f, 9.6f, 0.4f), matAgedBrick);

            // Lámparas de servicio muy tenues que dejan gran parte del recorrido en penumbra
            CreateColdFluorescentFixture("Luz_Servicio_Escalera_Alta", zone36.transform,
                new Vector3(12.2f, stageY + 2.95f, 116.0f), matRustedLocker, matLanternGlow, 1.2f);
            CreateColdFluorescentFixture("Luz_Servicio_Escalera_Descansillo", zone36.transform,
                new Vector3(20.1f, midLandingY + 2.95f, 116.0f), matRustedLocker, matLanternGlow, 1.1f);

            // ====================================================================
            // ZONA 37: DEPÓSITO CLAUSURADO (X: 29.0 a 42.0, Z: 114.2 a 126.5, Y = -3.2m, Altura: 3.6m)
            // ====================================================================
            GameObject zone37 = new GameObject("37_Sealed_Storage_Depot");
            zone37.transform.parent = rootEnv;

            CreatePrimitive("SealedDepot_Floor", PrimitiveType.Cube, zone37.transform,
                new Vector3(35.5f, subY - 0.20f, 120.35f), new Vector3(13.0f, 0.40f, 12.3f), matDarkCement);
            CreatePrimitive("SealedDepot_Ceiling", PrimitiveType.Cube, zone37.transform,
                new Vector3(35.5f, subY + subH, 120.35f), new Vector3(13.0f, 0.40f, 12.3f), matAgedBrick);

            // Muro oeste (X = 29.0) con puerta antigua desde el final de la Escalera Oculta en Z = 116.0m
            CreateWall("SealedDepot_Wall_West_N", zone37.transform,
                new Vector3(29.0f, subY + 1.8f, 121.8f), new Vector3(0.45f, subH, 9.4f), matAgedBrick);
            CreateArchitecturalDoor("Door_HiddenStair_To_SealedDepot", zone37.transform,
                new Vector3(29.0f, subY, 116.0f), Quaternion.Euler(0f, 90f, 0f),
                1.8f, 3.0f, subH, 0.45f, false, "",
                "DEPÓSITO CLAUSURADO (ZONA 37)", "", matWoodArchive, matRustedLocker, matAgedBrick);

            // Muros sur (Z = 114.2) y este (X = 42.0) de ladrillo con manchas de humedad
            CreateWall("SealedDepot_Wall_South", zone37.transform,
                new Vector3(35.5f, subY + 1.8f, 114.2f), new Vector3(13.0f, subH, 0.45f), matAgedBrick);
            CreateWall("SealedDepot_Wall_East", zone37.transform,
                new Vector3(42.0f, subY + 1.8f, 120.35f), new Vector3(0.45f, subH, 12.3f), matAgedBrick);

            // Muro norte (Z = 126.5) con Puerta Blindada hacia la Sala de Archivos Prohibidos (Zona 38) en X = 35.5m
            CreateWall("SealedDepot_Wall_North_W", zone37.transform,
                new Vector3(31.725f, subY + 1.8f, 126.5f), new Vector3(5.45f, subH, 0.45f), matAgedBrick);
            CreateWall("SealedDepot_Wall_North_E", zone37.transform,
                new Vector3(39.275f, subY + 1.8f, 126.5f), new Vector3(5.45f, subH, 0.45f), matAgedBrick);
            GameObject doorToForbiddenArchivesObj = CreateArchitecturalDoor(
                "Door_SealedDepot_To_ForbiddenArchives", zone37.transform,
                new Vector3(35.5f, subY, 126.5f), Quaternion.identity,
                1.8f, 3.0f, subH, 0.45f, true, "key_forbidden_archives_cavity",
                "SALA DE ARCHIVOS PROHIBIDOS (ZONA 38)",
                "Sala de Archivos Prohibidos — Cerrada (Examina las cajas y el panel hueco de madera en el Depósito Clausurado)",
                matMahogany, matGold, matAgedBrick);
            WorldDoor doorToForbiddenArchives = doorToForbiddenArchivesObj.GetComponent<WorldDoor>();

            // Manchas de humedad en paredes y suelo del depósito
            DecoPiece("Mancha_Humedad_Deposito_1", PrimitiveType.Cube, zone37.transform,
                new Vector3(41.72f, subY + 1.4f, 118.5f), new Vector3(0.04f, 2.2f, 3.4f), matDampPlaster);
            DecoPiece("Mancha_Humedad_Deposito_2", PrimitiveType.Cube, zone37.transform,
                new Vector3(34.5f, subY + 1.3f, 114.45f), new Vector3(3.8f, 2.0f, 0.04f), matDampPlaster);

            // Estanterías metálicas oxidadas, cajas de madera con etiquetas arrancadas y trajes en fundas protectoras
            CreatePrimitive("Estanteria_Metalica_Deposito_1", PrimitiveType.Cube, zone37.transform,
                new Vector3(31.2f, subY + 1.35f, 124.2f), new Vector3(2.8f, 2.7f, 0.9f), matRustedLocker);
            CreatePrimitive("Estanteria_Metalica_Deposito_2", PrimitiveType.Cube, zone37.transform,
                new Vector3(40.8f, subY + 1.35f, 122.5f), new Vector3(0.9f, 2.7f, 3.6f), matRustedLocker);

            CreateGarmentRackWithTutus("Perchero_Fundas_Protectoras_Deposito", zone37.transform,
                new Vector3(39.5f, subY, 116.5f), matRustedLocker, matIron, matClothCover, matTutuBlack, matClothCover);

            // Cajas de madera con etiquetas de inventario parcialmente arrancadas y caja metálica cerrada en el centro
            CreatePrimitive("Caja_Madera_Retirada_1", PrimitiveType.Cube, zone37.transform,
                new Vector3(34.2f, subY + 0.50f, 119.5f), new Vector3(1.4f, 1.0f, 1.2f), matWoodArchive);
            CreatePrimitive("Caja_Madera_Retirada_2", PrimitiveType.Cube, zone37.transform,
                new Vector3(36.2f, subY + 0.42f, 119.8f), new Vector3(1.2f, 0.84f, 1.1f), matWoodArchive);
            CreatePrimitive("Caja_Metalica_Cerrada_Personal", PrimitiveType.Cube, zone37.transform,
                new Vector3(34.2f, subY + 1.12f, 119.5f), new Vector3(0.58f, 0.24f, 0.42f), matMetalArchive);

            // ANOMALÍA VISUAL SUTIL ZONA 37: Tela colgada que se mueve al pasar + caja que cambia de posición al salir y volver
            GameObject hangingClothAnom = DecoPiece("Tela_Colgada_Anomalia_Deposito", PrimitiveType.Cube, zone37.transform,
                new Vector3(33.0f, subY + 1.85f, 117.5f), new Vector3(0.18f, 2.5f, 1.4f), matClothCover);
            GameObject shiftingCrateAnom = CreatePrimitive("Caja_Desplazable_Anomalia_Deposito", PrimitiveType.Cube, zone37.transform,
                new Vector3(31.8f, subY + 0.38f, 120.5f), new Vector3(0.85f, 0.76f, 0.85f), matCardboard);

            GameObject depotAnomalyHost = new GameObject("Anomaly_SealedDepot_ClothAndCrate");
            depotAnomalyHost.transform.parent = zone37.transform;
            depotAnomalyHost.transform.position = new Vector3(33.0f, subY + 1.0f, 118.2f);
            SecretZoneAnomalyController anom37 = depotAnomalyHost.AddComponent<SecretZoneAnomalyController>();
            anom37.Configure(SecretZoneNodeType.AnomalySealedDepotClothAndCrate, hangingClothAnom.transform, shiftingCrateAnom.transform);
            EditorUtility.SetDirty(anom37);

            // INTERACCIÓN PRINCIPAL ZONA 37: Panel de madera en el muro este (X = 41.6, Z = 118.0) que oculta un hueco con llave y documentos
            GameObject woodenCavityPanel = CreatePrimitive("Panel_Madera_Hueco_Oculto_Deposito", PrimitiveType.Cube, zone37.transform,
                new Vector3(41.65f, subY + 1.35f, 118.2f), new Vector3(0.16f, 1.45f, 1.35f), matMahogany);
            DecoPiece("Hueco_Secreto_Pared_Deposito", PrimitiveType.Cube, zone37.transform,
                new Vector3(41.75f, subY + 1.35f, 118.2f), new Vector3(0.08f, 0.85f, 0.85f), matShadowFigure);

            SecretZoneAnomalyController depotInvestigateCtrl = woodenCavityPanel.AddComponent<SecretZoneAnomalyController>();
            depotInvestigateCtrl.Configure(SecretZoneNodeType.InteractiveSealedDepotInvestigation, woodenCavityPanel.transform, null, doorToForbiddenArchives);
            EditorUtility.SetDirty(depotInvestigateCtrl);

            CreateColdFluorescentFixture("Luz_Tenue_Deposito_Clausurado", zone37.transform,
                new Vector3(35.5f, subY + 3.25f, 120.35f), matRustedLocker, matLanternGlow, 1.45f);

            // DOCUMENTOS DE LA ZONA 37 (DEPÓSITO CLAUSURADO)
            CreateInteractiveWorldDoc("Doc_Etiquetas_Arrancadas_Deposito", zone37.transform,
                new Vector3(36.2f, subY + 0.86f, 119.8f), new Vector3(0.38f, 0.04f, 0.28f), matSecretPaper,
                "doc_s8_depot_torn_inventory_tags",
                "Etiquetas de Inventario Parcialmente Arrancadas — Objetos Retirados tras el Incidente",
                "DEPÓSITO SUBTERRÁNEO CLAUSURADO (ZONA 37)\n\n" +
                "Las etiquetas adheridas a las cajas de madera revelan que este lugar no era un almacén ordinario:\n" +
                "• **Lote B-01 (15 de Octubre, 03:20 h)**: «Objetos personales, cuadernos de ensayo y trajes escénicos retirados de las aulas y del escenario antes de la inspección policial».\n" +
                "• **Lote B-02**: «Fotografías sin retocar del ensayo general del 13 de octubre. Prohibida su exhibición».\n\n" +
                "Nota al margen: *«La llave del Archivo Prohibido contiguo está escondida tras el **Panel de Madera Suelto en la pared este ([E])**»*.");

            CreateInteractiveWorldDoc("Doc_Fotos_No_Oficiales_Deposito", zone37.transform,
                new Vector3(31.2f, subY + 1.15f, 123.7f), new Vector3(0.36f, 0.04f, 0.28f), matSatinRibbon,
                "doc_s8_depot_unofficial_photos",
                "Fotografías No Oficiales de Ensayos — Elena Vesper y Mara Bell",
                "FOTOGRAFÍAS DE ENSAYO CONFISCADAS (OCTUBRE DE 1924)\n\n" +
                "A diferencia de los retratos oficiales de la galería donde la dirección fomentaba la rivalidad entre ambas alumnas, estas fotografías instantáneas muestran a **Elena Vesper y Mara Bell ensayando juntas en secreto después de medianoche**, corrigiéndose mutuamente la postura con absoluta complicidad y afecto.\n\n" +
                "En el reverso de la última foto se lee con letra de Elena:\n" +
                "«Mara no es mi rival; es la única persona dentro de esta academia que intenta ayudarme a sacar a la luz lo que Isadora Vale oculta».");

            // ====================================================================
            // ZONA 38: SALA DE ARCHIVOS PROHIBIDOS (X: 16.0 a 42.0, Z: 126.5 a 138.5, Y = -3.2m, Altura: 3.6m)
            // ====================================================================
            GameObject zone38 = new GameObject("38_Forbidden_Archives_Room");
            zone38.transform.parent = rootEnv;

            CreatePrimitive("ForbiddenArchives_Floor", PrimitiveType.Cube, zone38.transform,
                new Vector3(29.0f, subY - 0.20f, 132.5f), new Vector3(26.0f, 0.40f, 12.0f), matParquet);
            CreatePrimitive("ForbiddenArchives_Carpet", PrimitiveType.Cube, zone38.transform,
                new Vector3(28.5f, subY + 0.02f, 132.5f), new Vector3(10.0f, 0.04f, 6.0f), matCarpet);
            CreatePrimitive("ForbiddenArchives_Ceiling", PrimitiveType.Cube, zone38.transform,
                new Vector3(29.0f, subY + subH, 132.5f), new Vector3(26.0f, 0.40f, 12.0f), matWallIvory);

            // Muros perimetrales de la Sala de Archivos Prohibidos
            CreateWall("ForbiddenArchives_Wall_North", zone38.transform,
                new Vector3(29.0f, subY + 1.8f, 138.5f), new Vector3(26.0f, subH, 0.45f), matMahogany);
            CreateWall("ForbiddenArchives_Wall_East", zone38.transform,
                new Vector3(42.0f, subY + 1.8f, 132.5f), new Vector3(0.45f, subH, 12.0f), matMahogany);
            // Tramo sur oeste (X: 16 a 29, Z = 126.5; el tramo X: 29 a 42 ya está cerrado por el muro norte de Zona 37)
            CreateWall("ForbiddenArchives_Wall_South_WestExt", zone38.transform,
                new Vector3(22.5f, subY + 1.8f, 126.5f), new Vector3(13.0f, subH, 0.45f), matMahogany);

            // Muro oeste (X = 16.0, Z: 126.5 a 138.5) con Puerta Secreta hacia el pasaje ascendente de la Cámara tras el Escenario (Zona 39) en Z = 132.5m
            CreateWall("ForbiddenArchives_Wall_West_S", zone38.transform,
                new Vector3(16.0f, subY + 1.8f, 128.975f), new Vector3(0.45f, subH, 4.95f), matMahogany);
            CreateWall("ForbiddenArchives_Wall_West_N", zone38.transform,
                new Vector3(16.0f, subY + 1.8f, 136.025f), new Vector3(0.45f, subH, 4.95f), matMahogany);
            GameObject doorToBehindStageChamberObj = CreateArchitecturalDoor(
                "Door_ForbiddenArchives_To_BehindStageChamber", zone38.transform,
                new Vector3(16.0f, subY, 132.5f), Quaternion.Euler(0f, 90f, 0f),
                1.8f, 3.0f, subH, 0.45f, true, "key_behind_stage_timeline",
                "PASAJE A LA CÁMARA TRAS EL ESCENARIO (ZONA 39)",
                "Cámara tras el Escenario — Bloqueada (Ordena los documentos por fecha en la Mesa Central de los Archivos Prohibidos)",
                matMahogany, matGold, matMahogany);
            WorldDoor doorToBehindStageChamber = doorToBehindStageChamberObj.GetComponent<WorldDoor>();

            // Archivadores de madera oscura, estanterías metálicas y cajas de documentos ordenadas
            float[] cabX = { 20.5f, 24.5f, 32.5f, 36.5f };
            for (int c = 0; c < cabX.Length; c++)
            {
                CreatePrimitive($"Archivador_Prohibido_Norte_{c + 1}", PrimitiveType.Cube, zone38.transform,
                    new Vector3(cabX[c], subY + 1.35f, 137.4f), new Vector3(2.4f, 2.7f, 0.85f), matWoodArchive);
                DecoPiece($"Placa_Archivador_Prohibido_{c + 1}", PrimitiveType.Cube, zone38.transform,
                    new Vector3(cabX[c], subY + 2.1f, 136.95f), new Vector3(0.45f, 0.14f, 0.04f), matGold);
            }

            CreatePrimitive("Estanteria_Metal_Prohibidos_Sur", PrimitiveType.Cube, zone38.transform,
                new Vector3(22.5f, subY + 1.35f, 127.4f), new Vector3(5.5f, 2.7f, 0.85f), matMetalArchive);

            // Pequeño escritorio lateral con cajones cerrados (X = 39.5, Z = 132.5)
            CreatePrimitive("Escritorio_Auxiliar_Archivos_Prohibidos", PrimitiveType.Cube, zone38.transform,
                new Vector3(39.6f, subY + 0.55f, 132.5f), new Vector3(1.4f, 1.1f, 2.4f), matMahogany);
            CreatePrimitive("Silla_Escritorio_Archivos_Prohibidos", PrimitiveType.Cube, zone38.transform,
                new Vector3(38.2f, subY + 0.50f, 132.5f), new Vector3(0.75f, 1.0f, 0.75f), matLeather);

            // MESA CENTRAL CON LÁMPARA ANTIGUA E INTERACCIÓN DE CRONOLOGÍA (X = 28.5, Z = 132.5)
            GameObject centralArchiveTable = CreatePrimitive("Mesa_Central_Archivos_Prohibidos", PrimitiveType.Cube, zone38.transform,
                new Vector3(28.5f, subY + 0.48f, 132.5f), new Vector3(4.2f, 0.96f, 2.2f), matMahogany);
            CreateOrnateLantern("Lampara_Antigua_Mesa_Prohibidos", zone38.transform,
                new Vector3(28.5f, subY + 1.18f, 132.5f), matGold, matLanternGlow, new Color(1.0f, 0.82f, 0.46f), 2.8f);

            SecretZoneAnomalyController archiveTimelineCtrl = centralArchiveTable.AddComponent<SecretZoneAnomalyController>();
            archiveTimelineCtrl.Configure(SecretZoneNodeType.InteractiveForbiddenArchivesTimeline, null, null, doorToBehindStageChamber);
            EditorUtility.SetDirty(archiveTimelineCtrl);

            // LOS 6 DOCUMENTOS CLAVE DE LA SALA DE ARCHIVOS PROHIBIDOS (ZONA 38)
            // 1. Correspondencia privada de la dirección
            CreateInteractiveWorldDoc("Doc_Prohibido_1_Correspondencia_Direccion", zone38.transform,
                new Vector3(26.9f, subY + 0.99f, 131.8f), new Vector3(0.38f, 0.04f, 0.28f), matSecretPaper,
                "doc_s8_forbidden_1_correspondence",
                "1. Correspondencia Privada de la Dirección (28 de Septiembre de 1924)",
                "ARCHIVO PROHIBIDO — CARPETA I (CORRESPONDENCIA RESERVADA)\n\n" +
                "Carta del Presidente del Patronato a la Directora Isadora Vale:\n" +
                "«Si Elena Vesper cumple su amenaza de entregar a la prensa las cartas de su hermana fallecida en 1922, la Academia cerrará y seremos procesados. Debe usted impedir que Elena abandone el edificio tras la gala del 14 de octubre, cueste lo que cueste».");

            // 2. Informes sobre cambios realizados en El Lago de las Sombras
            CreateInteractiveWorldDoc("Doc_Prohibido_2_Cambios_Lago", zone38.transform,
                new Vector3(27.9f, subY + 0.99f, 131.8f), new Vector3(0.38f, 0.04f, 0.28f), matSecretPaper,
                "doc_s8_forbidden_2_lake_changes",
                "2. Informe Reservado — Modificación Técnica de «El Lago de las Sombras» (11 de Octubre)",
                "ARCHIVO PROHIBIDO — CARPETA II (MODIFICACIÓN ESCÉNICA)\n\n" +
                "Informe firmado por Isadora Vale:\n" +
                "«Se ordena sustituir el final del Acto III: se retirará el primer violonchelo del foso para suprimir la señal en Do Natural y se bajará el bastidor ciego del Contrapeso III a las 20:15 h, aislando a la primera bailarina en la parte posterior del escenario mientras Mara Bell concluye la obra ante el público».");

            // 3. Registros que contradicen la versión oficial de la desaparición de Elena Vesper
            CreateInteractiveWorldDoc("Doc_Prohibido_3_Contradiccion_Oficial", zone38.transform,
                new Vector3(29.1f, subY + 0.99f, 131.8f), new Vector3(0.38f, 0.04f, 0.28f), matSecretPaper,
                "doc_s8_forbidden_3_contradiction",
                "3. Parte Médico Interno — Contradicción con el Acta Oficial (14 de Octubre, 20:30 h)",
                "ARCHIVO PROHIBIDO — CARPETA III (REGISTRO NO ENTREGADO A LA POLICÍA)\n\n" +
                "«20:30 h del 14 de octubre de 1924: Contrariamente al comunicado redactado para las autoridades donde se afirma que Elena Vesper huyó del teatro durante la función, la alumna fue atendida por contusiones leves tras la caída del bastidor de contrapeso y trasladada bajo llave al ala privada de dirección».");

            // 4. Anotaciones sobre decisiones tomadas por Isadora Vale
            CreateInteractiveWorldDoc("Doc_Prohibido_4_Decisiones_Isadora", zone38.transform,
                new Vector3(26.9f, subY + 0.99f, 133.2f), new Vector3(0.38f, 0.04f, 0.28f), matSecretPaper,
                "doc_s8_forbidden_4_isadora_notes",
                "4. Diario de Decisiones de Isadora Vale (15 y 16 de Octubre de 1924)",
                "ARCHIVO PROHIBIDO — CARPETA IV (MANUSCRITO DE ISADORA VALE)\n\n" +
                "«16 de octubre: Elena se niega a firmar el acuerdo de silencio en su habitación. He ordenado trasladarla esta medianoche a través de la Cámara tras el Escenario. Sin embargo, sospecho que Mara Bell ha copiado la llave del archivo subterráneo».");

            // 5. Documentos que revelan información desconocida sobre el vínculo entre Elena y Mara
            CreateInteractiveWorldDoc("Doc_Prohibido_5_Vinculo_Elena_Mara", zone38.transform,
                new Vector3(27.9f, subY + 0.99f, 133.2f), new Vector3(0.38f, 0.04f, 0.28f), matSatinRibbon,
                "doc_s8_forbidden_5_elena_mara_bond",
                "5. Declaración Conjunta de Elena Vesper y Mara Bell (Secreto entre Alumnas)",
                "ARCHIVO PROHIBIDO — CARPETA V (CONFISCADA A MARA BELL)\n\n" +
                "«Nos hicieron fingir ante toda la academia que éramos enemigas para que la Directora Vale confiara en Mara el papel sustituto del tercer acto. Ese era nuestro plan desde el principio: mientras Isadora creía encerrar a Elena, Mara tenía preparada la salida oculta detrás del escenario».");

            // 6. Expediente incompleto al que le falta su última página
            CreateInteractiveWorldDoc("Doc_Prohibido_6_Expediente_Incompleto", zone38.transform,
                new Vector3(29.1f, subY + 0.99f, 133.2f), new Vector3(0.38f, 0.04f, 0.28f), matGold,
                "doc_s8_forbidden_6_incomplete_dossier",
                "6. Expediente Especial Nº 00 — [FALTA LA ÚLTIMA PÁGINA]",
                "ARCHIVO PROHIBIDO — EXPEDIENTE Nº 00 (17 DE OCTUBRE DE 1924)\n\n" +
                "La última página del expediente ha sido arrancada del broche metálico. En el margen inferior queda una línea escrita:\n" +
                "«La última página de este expediente, junto con la fotografía real de la función y la partitura a dos manos, permanece en la **Cámara tras el Escenario (Zona 39)**.\n" +
                "Interactúa con la **Mesa Central ([E])** para ordenar los 6 registros cronológicamente y desbloquear la puerta oeste».");

            // ====================================================================
            // CORREDOR Y ESCALERA OCULTA ASCENDENTE DE ZONA 38 A ZONA 39
            // Conecta X = 16.0, Z = 132.5, Y = -3.2m -> X = 8.3, Z = 107.0, Y = 2.4m sin solapar ninguna sala
            // ====================================================================
            GameObject secretPass38To39 = new GameObject("Secret_Passage_Zone38_To_Zone39");
            secretPass38To39.transform.parent = rootEnv;

            // Tramo horizontal oeste en Y = -3.2m (X: 6.2 a 16.0, Z: 130.5 a 134.5)
            CreatePrimitive("Pass38_WestLeg_Floor", PrimitiveType.Cube, secretPass38To39.transform,
                new Vector3(11.1f, subY - 0.20f, 132.5f), new Vector3(9.8f, 0.40f, 3.6f), matDarkCement);
            CreatePrimitive("Pass38_WestLeg_Ceiling", PrimitiveType.Cube, secretPass38To39.transform,
                new Vector3(11.1f, subY + subH, 132.5f), new Vector3(9.8f, 0.40f, 3.6f), matAgedBrick);
            CreateWall("Pass38_WestLeg_Wall_North", secretPass38To39.transform,
                new Vector3(11.1f, subY + 1.8f, 134.3f), new Vector3(9.8f, subH, 0.4f), matAgedBrick);
            CreateWall("Pass38_WestLeg_Wall_South", secretPass38To39.transform,
                new Vector3(13.1f, subY + 1.8f, 130.7f), new Vector3(5.8f, subH, 0.4f), matAgedBrick);

            // Tramo horizontal sur en Y = -3.2m bajo la galería este (X: 6.2 a 10.4, Z: 114.0 a 134.5, techo en Y = +0.4m)
            CreatePrimitive("Pass38_SouthLeg_Floor", PrimitiveType.Cube, secretPass38To39.transform,
                new Vector3(8.3f, subY - 0.20f, 124.25f), new Vector3(4.2f, 0.40f, 20.5f), matDarkCement);
            CreatePrimitive("Pass38_SouthLeg_Ceiling", PrimitiveType.Cube, secretPass38To39.transform,
                new Vector3(8.3f, subY + subH, 124.25f), new Vector3(4.2f, 0.40f, 20.5f), matAgedBrick);
            CreateWall("Pass38_SouthLeg_Wall_West", secretPass38To39.transform,
                new Vector3(6.2f, subY + 1.8f, 120.5f), new Vector3(0.4f, subH, 28.0f), matAgedBrick);
            CreateWall("Pass38_SouthLeg_Wall_East", secretPass38To39.transform,
                new Vector3(10.4f, subY + 1.8f, 118.8f), new Vector3(0.4f, subH, 23.8f), matAgedBrick);

            // Rampa/escalera oculta ascendente desde Z = 114.0 (Y = -3.2m) hasta Z = 107.0 (Y = +2.4m) dentro del doble muro (X: 6.2 a 10.4)
            GameObject climbToChamberRamp = CreatePrimitive("Pass38_AscendingStair_To_Zone39", PrimitiveType.Cube, secretPass38To39.transform,
                new Vector3(8.3f, -0.55f, 110.5f), new Vector3(3.6f, 0.35f, 9.0f), matMahogany);
            climbToChamberRamp.transform.localRotation = Quaternion.Euler(38.6f, 0f, 0f);

            for (int st = 0; st < 12; st++)
            {
                float sz = 113.8f - st * 0.56f;
                float sy = subY + 0.22f + st * 0.46f;
                DecoPiece($"Peldaño_Ascenso_Camara_{st + 1}", PrimitiveType.Cube, secretPass38To39.transform,
                    new Vector3(8.3f, sy, sz), new Vector3(3.4f, 0.08f, 0.42f), matWoodArchive);
            }

            CreateColdFluorescentFixture("Luz_Pasaje_Secreto_A_Camara", secretPass38To39.transform,
                new Vector3(8.3f, subY + 3.1f, 122.0f), matRustedLocker, matLanternGlow, 1.3f);

            // ====================================================================
            // ZONA 39: CÁMARA TRAS EL ESCENARIO (X: 6.0 a 10.6, Z: 98.5 a 107.0, Y = 2.4m, Altura: 3.8m)
            // Situada físicamente detrás de la pared este del Escenario Principal (Zona 32)
            // ====================================================================
            GameObject zone39 = new GameObject("39_Chamber_Behind_Stage");
            zone39.transform.parent = rootEnv;
            float chamberH = 3.8f;

            CreatePrimitive("ChamberBehindStage_Floor", PrimitiveType.Cube, zone39.transform,
                new Vector3(8.3f, stageY - 0.20f, 102.75f), new Vector3(4.6f, 0.40f, 8.5f), matParquet);
            CreatePrimitive("ChamberBehindStage_Ceiling", PrimitiveType.Cube, zone39.transform,
                new Vector3(8.3f, stageY + chamberH, 106.25f), new Vector3(4.6f, 0.40f, 15.5f), matMahogany);

            // Muro oeste de la Cámara (X = 6.0, Z: 98.5 a 107.0) que da directamente al Escenario Principal (Zona 32):
            // Incluye la MIRILLA / REJILLA DE OBSERVACIÓN en Z = 102.8m y una puerta oculta de retorno directo al escenario en Z = 105.4m
            CreateWall("Chamber_Wall_West_SouthSeg", zone39.transform,
                new Vector3(6.0f, stageY + 1.9f, 100.1f), new Vector3(0.35f, chamberH, 3.2f), matMahogany);
            // Vano de la Mirilla de Observación en Z: 101.7 a 103.9 (antepecho bajo, dintel superior y lamas de madera oscura)
            CreateWall("Chamber_Peephole_LowWall", zone39.transform,
                new Vector3(6.0f, stageY + 0.68f, 102.8f), new Vector3(0.35f, 1.36f, 2.2f), matMahogany);
            CreateWall("Chamber_Peephole_HighWall", zone39.transform,
                new Vector3(6.0f, stageY + 2.85f, 102.8f), new Vector3(0.35f, 1.90f, 2.2f), matMahogany);
            DecoPiece("Mirilla_Marco_Laton", PrimitiveType.Cube, zone39.transform,
                new Vector3(6.18f, stageY + 1.62f, 102.8f), new Vector3(0.06f, 0.58f, 1.85f), matGold);
            DecoPiece("Mirilla_Lama_Horizontal_1", PrimitiveType.Cube, zone39.transform,
                new Vector3(6.0f, stageY + 1.54f, 102.8f), new Vector3(0.18f, 0.03f, 1.75f), matIron);
            DecoPiece("Mirilla_Lama_Horizontal_2", PrimitiveType.Cube, zone39.transform,
                new Vector3(6.0f, stageY + 1.70f, 102.8f), new Vector3(0.18f, 0.03f, 1.75f), matIron);

            // Puerta disimulada en X = 6.0, Z = 105.4m para salir directamente al Escenario Principal tras descubrir la verdad
            CreateArchitecturalDoor("Door_SecretChamber_To_MainStage", zone39.transform,
                new Vector3(6.0f, stageY, 105.4f), Quaternion.Euler(0f, 90f, 0f),
                1.6f, 2.9f, chamberH, 0.35f, false, "",
                "SALIDA OCULTA AL ESCENARIO PRINCIPAL (ZONA 32)", "", matMahogany, matGold, matMahogany);

            // Mesa pequeña cubierta de polvo en el centro de la Cámara tras el Escenario (X = 8.3, Z = 102.8)
            CreatePrimitive("Mesa_Polvorienta_Camara_Secreta", PrimitiveType.Cube, zone39.transform,
                new Vector3(8.3f, stageY + 0.45f, 102.8f), new Vector3(1.5f, 0.90f, 2.1f), matMahogany);

            // CAJA DE MADERA ABIERTA INTERACTIVA (Fotografía original, carta personal y partitura a dos manos)
            GameObject openWoodenBox = CreatePrimitive("Caja_Madera_Abierta_Verdad_Oculta", PrimitiveType.Cube, zone39.transform,
                new Vector3(8.3f, stageY + 0.98f, 102.8f), new Vector3(0.62f, 0.16f, 0.46f), matWoodArchive);
            DecoPiece("Tapa_Abierta_Caja_Secreta", PrimitiveType.Cube, openWoodenBox.transform,
                new Vector3(0.28f, 0.65f, 0f), new Vector3(0.08f, 1.1f, 0.96f), matGold).transform.localRotation = Quaternion.Euler(0f, 0f, -25f);
            DecoPiece("Interior_Terciopelo_Caja_Secreta", PrimitiveType.Cube, openWoodenBox.transform,
                new Vector3(0f, 0.52f, 0f), new Vector3(0.88f, 0.05f, 0.88f), matVelvetDrapes);

            // Luz escénica cálida que ilumina el Arco del Lago vista a través de la mirilla durante la secuencia final
            Light stageEchoLight = CreatePictureSpotlight("Foco_Eco_Original_Desde_Mirilla", zone39.transform,
                new Vector3(5.6f, stageY + 2.4f, 102.8f), new Vector3(-8.0f, stageY + 1.2f, 101.2f), new Color(1.0f, 0.86f, 0.58f), 4.5f);

            SecretZoneAnomalyController finalBoxCtrl = openWoodenBox.AddComponent<SecretZoneAnomalyController>();
            finalBoxCtrl.Configure(SecretZoneNodeType.InteractiveBehindStageFinalBox, openWoodenBox.transform, null, null, stageEchoLight.gameObject);
            EditorUtility.SetDirty(finalBoxCtrl);

            // Iluminación tenue y silenciosa de la Cámara tras el Escenario
            CreateOrnateSconce("Aplique_Tenue_Camara_Secreta", zone39.transform,
                new Vector3(10.25f, stageY + 2.2f, 102.8f), matGold, matLanternGlow, new Color(1.0f, 0.80f, 0.46f), 1.35f);

            // DOCUMENTOS DE LA ZONA 39 (CÁMARA TRAS EL ESCENARIO)
            CreateInteractiveWorldDoc("Doc_Mirilla_Observacion_Escenario", zone39.transform,
                new Vector3(6.35f, stageY + 1.38f, 102.8f), new Vector3(0.26f, 0.04f, 0.34f), matGold,
                "doc_s8_behind_stage_peephole",
                "Mecanismo de Observación Disimulado (Mirilla hacia el Escenario Principal)",
                "CÁMARA TRAS EL ESCENARIO — ÁNGULO OCULTO DE OBSERVACIÓN\n\n" +
                "A través de las lamas horizontales de esta rejilla tienes una vista directa del **Arco del Lago y del centro del Escenario Principal (Zona 32)**.\n\n" +
                "Desde este ángulo oculto se comprende toda la escena: la noche del 14 de octubre, cuando bajó el bastidor ciego para aislar el fondo del escenario, Mara Bell abrió desde aquí el cerrojo interior para que Elena Vesper pudiera cruzar a esta cámara sin ser vista desde el patio de butacas ni desde la cabina de luces.\n\n" +
                "Examina la **Caja de Madera Abierta sobre la mesa ([E])** a tus espaldas para revelar el desenlace completo.");

            CreateInteractiveWorldDoc("Doc_Ultima_Pagina_Expediente_00", zone39.transform,
                new Vector3(8.3f, stageY + 0.93f, 101.95f), new Vector3(0.36f, 0.04f, 0.26f), matSecretPaper,
                "doc_s8_missing_last_page",
                "Última Página del Expediente Prohibido y Partitura a Dos Manos",
                "LA VERDAD OCULTA DE LA ACADEMIA — PÁGINA FINAL\n\n" +
                "Junto a la caja de madera reposa la página arrancada del Archivo Prohibido:\n" +
                "• **La Verdad sobre Isadora Vale**: Clausuró el teatro el 18 de octubre para ocultar que intentó retener ilegalmente a Elena Vesper tras descubrir que Elena poseía las pruebas de la negligencia que causó la muerte de su hermana en 1922.\n" +
                "• **La Verdad sobre Elena y Mara**: Nunca fueron enemigas. Simularon su rivalidad para que Mara fuera elegida sustituta en el Acto III y pudiera rescatar a Elena a través de esta cámara oculta.\n\n" +
                "Pulsa **`[E]` en la Caja de Madera Abierta** en el centro de la mesa para inspeccionar la fotografía original, la carta de despedida y escuchar el último eco musical del escenario.");
        }

        // ====================================================================
        // CONSTRUCCIÓN DE LA SECCIÓN 9: JARDÍN DE INVIERNO MONUMENTAL (ZONA 40)
        // Gran Invernadero Clásico Europeo (48m x 70m -> X: 45.0 a 93.0, Z: 22.0 a 92.0, Y: 2.4m, Altura bóveda: 10.2m)
        // Conectado físicamente con:
        // 1. Entrada de madera oscura con paneles de cristal desde el Pasillo de la Academia (X = 45.0, Z = 80.5)
        // 2. Corredor de Mantenimiento y Servicio Este (X = 45.0, Z = 58.0)
        // 3. Acceso Sur directo desde la rampa del Patio de Entrada (X = 45.0, Z = 33.5)
        // ====================================================================
        private static void BuildSection9WinterGarden(
            Transform rootEnv, float vestY,
            Material matParquet, Material matCarpet, Material matWallIvory, Material matMahogany,
            Material matGold, Material matIron, Material matClothCover, Material matVelvetDrapes,
            Material matLanternGlow, Material matColdFluorescent, Material matGreenVelvet,
            Material matLeather, Material matWoodArchive, Material matMetalArchive, Material matCardboard,
            Material matStoneWall, Material matRustedLocker, Material matMirrorGlass, Material matPorcelain,
            Material matTutuWhite, Material matTutuBlack, Material matSatinRibbon, Material matShadowFigure,
            Material matPortraitElena, Material matPhotoTroupe1923, Material matCobble, Material matStatue, Material matHedge)
        {
            // Paleta visual específica del Jardín de Invierno:
            // verde oliva oscuro, musgo, marrón envejecido, piedra gris cálida, hierro negro oxidado, marfil desgastado y latón antiguo
            Material matOliveDark = GetOrCreateBasicMaterial("Mat_S9_OliveFoliageDark", new Color(0.14f, 0.21f, 0.13f), 0.14f, 0.0f);
            Material matMossGreen = GetOrCreateBasicMaterial("Mat_S9_MossStone", new Color(0.19f, 0.26f, 0.15f), 0.18f, 0.0f);
            Material matAgedEarth = GetOrCreateBasicMaterial("Mat_S9_AgedSoilBrown", new Color(0.18f, 0.13f, 0.10f), 0.10f, 0.0f);
            Material matWarmGreyStone = GetOrCreateBasicMaterial("Mat_S9_WarmGreyStone", new Color(0.46f, 0.44f, 0.40f), 0.32f, 0.04f);
            Material matOxidizedBlackIron = GetOrCreateBasicMaterial("Mat_S9_OxidizedBlackIron", new Color(0.11f, 0.12f, 0.12f), 0.38f, 0.78f);
            Material matConservatoryGlass = GetOrCreateBasicMaterial("Mat_S9_ConservatoryGlass", new Color(0.42f, 0.56f, 0.68f), 0.92f, 0.25f);
            Material matDryLeaves = GetOrCreateBasicMaterial("Mat_S9_DryAutumnLeaves", new Color(0.36f, 0.24f, 0.14f), 0.12f, 0.0f);

            float gardenY = vestY;            // 2.4m (enrasado con el edificio principal y el corredor de mantenimiento)
            float wallH = 6.8f;               // Altura de cornisa perimetral: 6.8m (Y = 9.2m)
            float glassApexY = gardenY + 10.2f;// Cumbrera de la gran cubierta de cristal: Y = 12.6m

            GameObject zone40 = new GameObject("40_Winter_Garden_Conservatory");
            zone40.transform.parent = rootEnv;

            // Suelo base ampliado de tierra marrón envejecida y musgo (48m de ancho x 70m de largo: X: 45 a 93, Z: 22 a 92)
            CreatePrimitive("WinterGarden_Earth_Base", PrimitiveType.Cube, zone40.transform,
                new Vector3(69f, gardenY - 0.25f, 57f), new Vector3(48f, 0.50f, 70f), matAgedEarth);

            // ====================================================================
            // 1. ENTRADAS DE MADERA OSCURA CON PANELES DE CRISTAL Y MUROS PERIMETRALES
            // ====================================================================
            // Entrada 1 (Noroeste, X = 45.0, Z = 80.5): Conectada físicamente con el Pasillo de la Academia (Sala del Piano / Camerinos)
            CreateWall("WG_WestEntry_GlassFrame_S", zone40.transform,
                new Vector3(45f, gardenY + 2.5f, 78.525f), new Vector3(0.45f, 5.0f, 1.05f), matMahogany);
            CreateWall("WG_WestEntry_GlassFrame_N", zone40.transform,
                new Vector3(45f, gardenY + 2.5f, 82.475f), new Vector3(0.45f, 5.0f, 1.05f), matMahogany);
            DecoPiece("WG_Entry_GlassPane_L", PrimitiveType.Cube, zone40.transform,
                new Vector3(45.02f, gardenY + 2.1f, 78.55f), new Vector3(0.12f, 2.8f, 0.72f), matConservatoryGlass);
            DecoPiece("WG_Entry_GlassPane_R", PrimitiveType.Cube, zone40.transform,
                new Vector3(45.02f, gardenY + 2.1f, 82.45f), new Vector3(0.12f, 2.8f, 0.72f), matConservatoryGlass);

            CreateArchitecturalDoubleDoor(
                "Doors_MainAcademyHall_To_WinterGarden", zone40.transform,
                new Vector3(45f, gardenY, 80.5f), Quaternion.Euler(0f, 90f, 0f),
                2.8f, 3.5f, 5.0f, 0.45f, false, "",
                "JARDÍN DE INVIERNO — ENTRADA PRINCIPAL DE MADERA OSCURA Y CRISTAL (ZONA 40)", "",
                matMahogany, matGold, matMahogany);

            // Entrada 2 (Centro-Oeste, X = 45.0, Z = 58.0): Conectada con el Corredor de Mantenimiento
            DecoPiece("WG_MaintDoor_GlassTransom", PrimitiveType.Cube, zone40.transform,
                new Vector3(45.26f, gardenY + 3.65f, 58.0f), new Vector3(0.10f, 0.75f, 1.85f), matConservatoryGlass);

            // Entrada 3 (Suroeste, X = 45.0, Z = 33.5): Conectada directamente con la rampa empedrada desde el Patio de Entrada (Sección 1)
            CreateWall("WG_SouthWestEntry_Wall_S", zone40.transform,
                new Vector3(45f, gardenY + 2.5f, 26.5f), new Vector3(0.45f, 5.0f, 9.0f), matMahogany);
            CreateWall("WG_SouthWestEntry_Wall_N", zone40.transform,
                new Vector3(45f, gardenY + 2.5f, 35.475f), new Vector3(0.45f, 5.0f, 1.05f), matMahogany);
            DecoPiece("WG_SouthWestEntry_GlassPane_S", PrimitiveType.Cube, zone40.transform,
                new Vector3(45.02f, gardenY + 2.1f, 31.55f), new Vector3(0.12f, 2.6f, 0.65f), matConservatoryGlass);
            DecoPiece("WG_SouthWestEntry_GlassPane_N", PrimitiveType.Cube, zone40.transform,
                new Vector3(45.02f, gardenY + 2.1f, 35.45f), new Vector3(0.12f, 2.6f, 0.65f), matConservatoryGlass);
            CreateArchitecturalDoor(
                "Door_CourtyardRamp_To_WinterGarden", zone40.transform,
                new Vector3(45f, gardenY, 33.5f), Quaternion.Euler(0f, 90f, 0f),
                1.8f, 3.2f, 5.0f, 0.45f, false, "",
                "JARDÍN DE INVIERNO — ACCESO SUR DESDE EL PATIO DE ENTRADA (ZONA 40)", "",
                matMahogany, matGold, matMahogany);

            // Muros perimetrales clásicos de marfil desgastado, piedra gris cálida y ventanales con montantes de hierro oxidado
            CreateWall("WG_Wall_West_UpperCornice", zone40.transform,
                new Vector3(45f, gardenY + 5.9f, 57f), new Vector3(0.45f, 1.8f, 70f), matWallIvory);
            CreateWall("WG_Wall_North", zone40.transform,
                new Vector3(69f, gardenY + 3.4f, 92f), new Vector3(48f, wallH, 0.5f), matWallIvory);
            CreateWall("WG_Wall_South", zone40.transform,
                new Vector3(69f, gardenY + 3.4f, 22f), new Vector3(48f, wallH, 0.5f), matWallIvory);
            CreateWall("WG_Wall_East", zone40.transform,
                new Vector3(93f, gardenY + 3.4f, 57f), new Vector3(0.5f, wallH, 70f), matWallIvory);

            // Columnas interiores de hierro negro oxidado que sostienen la gran nave acristalada (X = 56m y X = 82m)
            for (float cz = 28f; cz <= 86f; cz += 14.5f)
            {
                CreateClassicalColumn($"Columna_Hierro_Invernadero_Oeste_{cz:F0}", zone40.transform,
                    new Vector3(56.0f, gardenY, cz), wallH, 0.34f, matOxidizedBlackIron);
                CreateClassicalColumn($"Columna_Hierro_Invernadero_Este_{cz:F0}", zone40.transform,
                    new Vector3(82.0f, gardenY, cz), wallH, 0.34f, matOxidizedBlackIron);
            }

            // Pilastras de hierro negro oxidado y grandes ventanales de cristal en los muros este, norte y sur
            for (float pz = 26f; pz <= 88f; pz += 8.8f)
            {
                DecoPiece($"Pilastra_Hierro_Este_{pz:F0}", PrimitiveType.Cube, zone40.transform,
                    new Vector3(92.65f, gardenY + 3.4f, pz), new Vector3(0.28f, wallH, 0.34f), matOxidizedBlackIron);
                DecoPiece($"Ventanal_Cristal_Este_{pz:F0}", PrimitiveType.Cube, zone40.transform,
                    new Vector3(92.70f, gardenY + 3.5f, pz), new Vector3(0.08f, 4.8f, 6.2f), matConservatoryGlass);
                DecoPiece($"Pilastra_Hierro_Oeste_{pz:F0}", PrimitiveType.Cube, zone40.transform,
                    new Vector3(45.35f, gardenY + 3.4f, pz), new Vector3(0.28f, wallH, 0.34f), matOxidizedBlackIron);
            }

            // ====================================================================
            // 2. GRAN CUBIERTA DE CRISTAL CON ESTRUCTURA METÁLICA Y PANELES AGRIETADOS/AUSENTES
            // ====================================================================
            GameObject roofStructure = new GameObject("WinterGarden_Glass_And_Iron_Roof");
            roofStructure.transform.parent = zone40.transform;

            // Cerchas transversales monumentales de hierro negro oxidado (48m de luz)
            for (float rz = 24.0f; rz <= 90.0f; rz += 7.3f)
            {
                DecoPiece($"Cercha_Hierro_Horizontal_{rz:F1}", PrimitiveType.Cube, roofStructure.transform,
                    new Vector3(69f, gardenY + wallH, rz), new Vector3(48f, 0.28f, 0.28f), matOxidizedBlackIron);
                DecoPiece($"Arco_Inclinado_Oeste_{rz:F1}", PrimitiveType.Cube, roofStructure.transform,
                    new Vector3(57f, gardenY + wallH + 1.6f, rz), new Vector3(24.5f, 0.22f, 0.24f), matOxidizedBlackIron).transform.localRotation = Quaternion.Euler(0f, 0f, 15.8f);
                DecoPiece($"Arco_Inclinado_Este_{rz:F1}", PrimitiveType.Cube, roofStructure.transform,
                    new Vector3(81f, gardenY + wallH + 1.6f, rz), new Vector3(24.5f, 0.22f, 0.24f), matOxidizedBlackIron).transform.localRotation = Quaternion.Euler(0f, 0f, -15.8f);
            }

            // Viga de cumbrera longitudinal en Y = 12.6m
            DecoPiece("Cumbrera_Central_Hierro", PrimitiveType.Cube, roofStructure.transform,
                new Vector3(69f, glassApexY, 57f), new Vector3(0.36f, 0.36f, 70f), matOxidizedBlackIron);

            // Paneles del techo de cristal (dejando huecos rotos/ausentes sobre la Fuente Central Z=58 y el Pabellón de Elena Z=79)
            float[] roofBayZ = { 27.5f, 34.8f, 42.1f, 49.4f, 65.5f, 72.2f, 86.5f };
            for (int b = 0; b < roofBayZ.Length; b++)
            {
                float bz = roofBayZ[b];
                CreatePrimitive($"Panel_Techo_Cristal_Oeste_{b + 1}", PrimitiveType.Cube, roofStructure.transform,
                    new Vector3(57f, gardenY + wallH + 1.6f, bz), new Vector3(24.4f, 0.12f, 6.4f), matConservatoryGlass).transform.localRotation = Quaternion.Euler(0f, 0f, 15.8f);
                CreatePrimitive($"Panel_Techo_Cristal_Este_{b + 1}", PrimitiveType.Cube, roofStructure.transform,
                    new Vector3(81f, gardenY + wallH + 1.6f, bz), new Vector3(24.4f, 0.12f, 6.4f), matConservatoryGlass).transform.localRotation = Quaternion.Euler(0f, 0f, -15.8f);
            }

            // Cristales rotos colgando y ramas exteriores asomando sobre los huecos abiertos (Z = 58 y Z = 79)
            DecoPiece("Rama_Exterior_Sombra_Fuente_1", PrimitiveType.Cylinder, roofStructure.transform,
                new Vector3(68.5f, glassApexY - 0.4f, 58.2f), new Vector3(0.18f, 3.8f, 0.18f), matMahogany).transform.localRotation = Quaternion.Euler(78f, 25f, 12f);
            DecoPiece("Rama_Exterior_Sombra_Estatua_2", PrimitiveType.Cylinder, roofStructure.transform,
                new Vector3(81.5f, glassApexY - 0.6f, 79.0f), new Vector3(0.16f, 3.2f, 0.16f), matMahogany).transform.localRotation = Quaternion.Euler(82f, -35f, 0f);

            // Hilos finos de lluvia nocturna cayendo por las aberturas del techo de cristal sobre la fuente y el musgo
            for (int r = 0; r < 14; r++)
            {
                float rx = 66.2f + (r % 7) * 0.95f;
                float rz = 56.2f + (r / 7) * 3.4f;
                DecoPiece($"Estela_Lluvia_Techo_Roto_{r + 1}", PrimitiveType.Cylinder, zone40.transform,
                    new Vector3(rx, gardenY + 5.2f, rz), new Vector3(0.014f, 3.4f, 0.014f), matConservatoryGlass);
            }

            // ====================================================================
            // 3. GRAN FUENTE CIRCULAR DE PIEDRA COMPLETAMENTE SECA CON CISNES (X = 69.0, Z = 58.0)
            // ====================================================================
            GameObject dryFountain = new GameObject("Fuente_Circular_Seca_Cisnes");
            dryFountain.transform.parent = zone40.transform;
            dryFountain.transform.position = new Vector3(69.0f, gardenY, 58.0f);

            // Brocal circular monumental de piedra gris cálida (6.8m de diámetro) y lecho interior seco con musgo y hojas caídas
            CreatePrimitive("Fuente_Brocal_Exterior", PrimitiveType.Cylinder, dryFountain.transform,
                new Vector3(0f, 0.42f, 0f), new Vector3(6.8f, 0.42f, 6.8f), matWarmGreyStone);
            CreatePrimitive("Fuente_Interior_Seco_Musgo", PrimitiveType.Cylinder, dryFountain.transform,
                new Vector3(0f, 0.44f, 0f), new Vector3(5.8f, 0.41f, 5.8f), matMossGreen);
            DecoPiece("Fuente_Hojas_Secas_1", PrimitiveType.Cylinder, dryFountain.transform,
                new Vector3(-1.35f, 0.86f, 0.95f), new Vector3(1.6f, 0.02f, 1.35f), matDryLeaves);
            DecoPiece("Fuente_Hojas_Secas_2", PrimitiveType.Cylinder, dryFountain.transform,
                new Vector3(1.45f, 0.86f, -0.85f), new Vector3(1.7f, 0.02f, 1.25f), matDryLeaves);

            // Pedestal central escalonado y ornamentación de dos cisnes tallados en piedra y latón oxidado
            CreatePrimitive("Fuente_Pedestal_Base", PrimitiveType.Cylinder, dryFountain.transform,
                new Vector3(0f, 0.95f, 0f), new Vector3(2.2f, 0.45f, 2.2f), matWarmGreyStone);
            CreatePrimitive("Fuente_Pedestal_Central", PrimitiveType.Cylinder, dryFountain.transform,
                new Vector3(0f, 1.55f, 0f), new Vector3(1.35f, 0.75f, 1.35f), matWarmGreyStone);
            DecoPiece("Cisne_Piedra_Cuerpo_1", PrimitiveType.Sphere, dryFountain.transform,
                new Vector3(-0.34f, 2.45f, 0f), new Vector3(0.62f, 0.46f, 0.88f), matStatue);
            DecoPiece("Cisne_Piedra_Cuello_1", PrimitiveType.Cylinder, dryFountain.transform,
                new Vector3(-0.34f, 2.98f, 0.28f), new Vector3(0.14f, 0.42f, 0.14f), matStatue).transform.localRotation = Quaternion.Euler(22f, 0f, 0f);
            DecoPiece("Cisne_Piedra_Cuerpo_2", PrimitiveType.Sphere, dryFountain.transform,
                new Vector3(0.34f, 2.45f, 0f), new Vector3(0.62f, 0.46f, 0.88f), matOxidizedBlackIron);
            DecoPiece("Cisne_Piedra_Cuello_2", PrimitiveType.Cylinder, dryFountain.transform,
                new Vector3(0.34f, 2.98f, -0.28f), new Vector3(0.14f, 0.42f, 0.14f), matOxidizedBlackIron).transform.localRotation = Quaternion.Euler(-22f, 0f, 0f);
            DecoPiece("Corona_Laton_Fuente", PrimitiveType.Cylinder, dryFountain.transform,
                new Vector3(0f, 2.22f, 0f), new Vector3(1.55f, 0.06f, 1.55f), matGold);

            WinterGardenAnomalyController fountainCtrl = dryFountain.AddComponent<WinterGardenAnomalyController>();
            fountainCtrl.Configure(WinterGardenNodeType.DryFountainWaterEchoAndInspect);
            EditorUtility.SetDirty(fountainCtrl);

            // ====================================================================
            // 4. SENDEROS DE PIEDRA QUE SE RAMIFICAN DESDE LA FUENTE Y SE RECONECTAN ENTRE SÍ
            // ====================================================================
            GameObject stonePaths = new GameObject("WinterGarden_Interconnected_Stone_Paths");
            stonePaths.transform.parent = zone40.transform;

            // Gran rotonda de piedra alrededor de la fuente seca (16.5m de diámetro)
            DecoPiece("Sendero_Anillo_Central_Fuente", PrimitiveType.Cylinder, stonePaths.transform,
                new Vector3(69.0f, gardenY + 0.015f, 58.0f), new Vector3(16.5f, 0.02f, 16.5f), matWarmGreyStone);

            // Avenida Central Oeste-Este (Desde la puerta del Corredor de Mantenimiento X=45, Z=58 pasando por la Fuente hasta X=86)
            DecoPiece("Sendero_Avenida_Central_OE", PrimitiveType.Cube, stonePaths.transform,
                new Vector3(65.5f, gardenY + 0.02f, 58.0f), new Vector3(41.0f, 0.03f, 3.8f), matWarmGreyStone);

            // Gran Paseo Longitudinal Central Norte-Sur (X = 69.0, Z: 27.0 a 86.0)
            DecoPiece("Sendero_Paseo_Central_NS", PrimitiveType.Cube, stonePaths.transform,
                new Vector3(69.0f, gardenY + 0.02f, 56.5f), new Vector3(3.8f, 0.03f, 59.0f), matWarmGreyStone);

            // Avenida Noroeste (Desde la Entrada Principal de Madera Oscura y Cristal X=45, Z=80.5 hasta el Circuito Oriental X=83.0)
            DecoPiece("Sendero_Avenida_Norte_OE", PrimitiveType.Cube, stonePaths.transform,
                new Vector3(64.0f, gardenY + 0.02f, 80.5f), new Vector3(38.0f, 0.03f, 3.6f), matWarmGreyStone);

            // Avenida Suroeste (Desde el Acceso Sur del Patio X=45, Z=33.5 hasta el Circuito Oriental X=83.0)
            DecoPiece("Sendero_Avenida_Sur_OE", PrimitiveType.Cube, stonePaths.transform,
                new Vector3(64.0f, gardenY + 0.02f, 33.5f), new Vector3(38.0f, 0.03f, 3.6f), matWarmGreyStone);

            // Circuito Longitudinal Occidental (X = 56.0, Z: 27.0 a 86.0) y Circuito Longitudinal Oriental (X = 82.0, Z: 27.0 a 86.0)
            DecoPiece("Sendero_Circuito_Occidental_NS", PrimitiveType.Cube, stonePaths.transform,
                new Vector3(56.0f, gardenY + 0.02f, 56.5f), new Vector3(3.2f, 0.03f, 59.0f), matWarmGreyStone);
            DecoPiece("Sendero_Circuito_Oriental_NS", PrimitiveType.Cube, stonePaths.transform,
                new Vector3(82.0f, gardenY + 0.02f, 56.5f), new Vector3(3.2f, 0.03f, 59.0f), matWarmGreyStone);

            // Senderos Transversales intermedios en Z = 45.5 y Z = 69.5 que conectan los tres paseos longitudinales
            DecoPiece("Sendero_Transversal_Sur_Intermedio", PrimitiveType.Cube, stonePaths.transform,
                new Vector3(69.0f, gardenY + 0.02f, 45.5f), new Vector3(29.0f, 0.03f, 2.8f), matWarmGreyStone);
            DecoPiece("Sendero_Transversal_Norte_Intermedio", PrimitiveType.Cube, stonePaths.transform,
                new Vector3(69.0f, gardenY + 0.02f, 69.5f), new Vector3(29.0f, 0.03f, 2.8f), matWarmGreyStone);

            // ====================================================================
            // 5. BANCOS DE HIERRO OXIDADO Y PEQUEÑA MESA DE PIEDRA (CON OBJETO QUE APARECE)
            // ====================================================================
            CreateWinterGardenIronBench("Banco_Hierro_Oxidado_Rotonda_NO", zone40.transform,
                new Vector3(63.2f, gardenY, 63.8f), Quaternion.Euler(0f, 135f, 0f), matOxidizedBlackIron, matMahogany);
            CreateWinterGardenIronBench("Banco_Hierro_Oxidado_Rotonda_SO", zone40.transform,
                new Vector3(63.2f, gardenY, 52.2f), Quaternion.Euler(0f, 45f, 0f), matOxidizedBlackIron, matMahogany);
            CreateWinterGardenIronBench("Banco_Hierro_Oxidado_Rotonda_NE", zone40.transform,
                new Vector3(74.8f, gardenY, 63.8f), Quaternion.Euler(0f, -135f, 0f), matOxidizedBlackIron, matMahogany);
            CreateWinterGardenIronBench("Banco_Hierro_Oxidado_Rotonda_SE", zone40.transform,
                new Vector3(74.8f, gardenY, 52.2f), Quaternion.Euler(0f, -45f, 0f), matOxidizedBlackIron, matMahogany);
            CreateWinterGardenIronBench("Banco_Hierro_Oxidado_Paseo_Oeste_1", zone40.transform,
                new Vector3(53.5f, gardenY, 65.5f), Quaternion.Euler(0f, 90f, 0f), matOxidizedBlackIron, matMahogany);
            CreateWinterGardenIronBench("Banco_Hierro_Oxidado_Paseo_Oeste_2", zone40.transform,
                new Vector3(53.5f, gardenY, 40.0f), Quaternion.Euler(0f, 90f, 0f), matOxidizedBlackIron, matMahogany);
            CreateWinterGardenIronBench("Banco_Hierro_Oxidado_Paseo_Este_1", zone40.transform,
                new Vector3(84.5f, gardenY, 65.5f), Quaternion.Euler(0f, -90f, 0f), matOxidizedBlackIron, matMahogany);
            CreateWinterGardenIronBench("Banco_Hierro_Oxidado_Paseo_Este_2", zone40.transform,
                new Vector3(84.5f, gardenY, 40.0f), Quaternion.Euler(0f, -90f, 0f), matOxidizedBlackIron, matMahogany);

            // Pequeña mesa circular de piedra gris cálida junto al banco del paseo oeste (X = 56.0, Z = 65.5)
            GameObject smallStoneTable = new GameObject("Mesa_Pequena_De_Piedra");
            smallStoneTable.transform.parent = zone40.transform;
            smallStoneTable.transform.position = new Vector3(56.0f, gardenY, 65.5f);
            CreatePrimitive("Mesa_Piedra_Columna", PrimitiveType.Cylinder, smallStoneTable.transform,
                new Vector3(0f, 0.42f, 0f), new Vector3(0.45f, 0.42f, 0.45f), matWarmGreyStone);
            CreatePrimitive("Mesa_Piedra_Tablero", PrimitiveType.Cylinder, smallStoneTable.transform,
                new Vector3(0f, 0.86f, 0f), new Vector3(1.25f, 0.06f, 1.25f), matWarmGreyStone);

            // Objeto que aparece sobre la mesa de piedra donde antes no estaba (tras leer la inscripción de la estatua de Elena)
            GameObject appearingRelicGroup = new GameObject("Objeto_Aparecido_Sobre_Mesa_Piedra");
            appearingRelicGroup.transform.parent = smallStoneTable.transform;
            appearingRelicGroup.transform.localPosition = new Vector3(0f, 0.93f, 0f);
            DecoPiece("Relicario_Laton_Aparecido", PrimitiveType.Cylinder, appearingRelicGroup.transform,
                new Vector3(-0.18f, 0.02f, 0.05f), new Vector3(0.18f, 0.02f, 0.18f), matGold);
            DecoPiece("Cinta_Saten_Aparecida", PrimitiveType.Cube, appearingRelicGroup.transform,
                new Vector3(0.12f, 0.015f, -0.08f), new Vector3(0.32f, 0.015f, 0.06f), matSatinRibbon);

            CreateInteractiveWorldDoc("Doc_Objeto_Aparecido_Mesa_Piedra", appearingRelicGroup.transform,
                new Vector3(0f, 0.03f, 0f), new Vector3(0.38f, 0.04f, 0.28f), matGold,
                "doc_s9_stone_table_appearing_object",
                "Relicario y Nota Aparecidos en la Mesa de Piedra — Conexión con Archivo Musical y Administración",
                "MESA DE PIEDRA DEL JARDÍN DE INVIERNO — [ANOMALÍA DE PRESENCIA]\n\n" +
                "Hace apenas un momento esta mesa de piedra estaba vacía. Ahora reposa sobre ella un relicario de latón con una nota doblada:\n\n" +
                "«La noche del **17 de octubre de 1924**, tres días después de que Isadora Vale registrara en **Administración (Sección 2)** la supuesta desaparición de Elena, nos reunimos en este Jardín de Invierno.\n" +
                "Elena tocó por última vez el violonchelo que hoy está oculto bajo una tela negra en el **Depósito de Instrumentos del Archivo Musical (Sección 5, Zona 25)** antes de partir al amanecer.\n" +
                "La caja con la fotografía de aquella noche está enterrada bajo la **losa desplazada en el rincón sureste de este jardín (`X = 86.5, Z = 27.5`)**, detrás de los helechos».");

            // ====================================================================
            // 6. GALERÍA DE ESTATUAS DE BAILARINAS Y TEMPLETE CON LA ESTATUA CONMEMORATIVA DE ELENA VESPER
            // ====================================================================
            // Dos estatuas cercanas a la rotonda que cambian sutilmente de orientación cuando el jugador aparta la vista
            GameObject statueNW = new GameObject("Estatua_Bailarina_Jardin_NO");
            statueNW.transform.parent = zone40.transform;
            statueNW.transform.position = new Vector3(60.5f, gardenY, 69.5f);
            CreateBallerinaStatue("Mesh_Estatua_NO", statueNW.transform, Vector3.zero, matStatue, matWarmGreyStone,
                "Estatua Clásica I — Arabesque en Piedra Gris");

            GameObject statueSW = new GameObject("Estatua_Bailarina_Jardin_SO");
            statueSW.transform.parent = zone40.transform;
            statueSW.transform.position = new Vector3(60.5f, gardenY, 45.5f);
            CreateBallerinaStatue("Mesh_Estatua_SO", statueSW.transform, Vector3.zero, matStatue, matWarmGreyStone,
                "Estatua Clásica II — Attitude bajo la Hiedra");

            // Otras estatuas de la galería monumental de bailarinas sobre pedestales envejecidos
            CreateBallerinaStatue("Estatua_Bailarina_Jardin_SE", zone40.transform,
                new Vector3(77.5f, gardenY, 45.5f), matStatue, matWarmGreyStone,
                "Estatua Clásica III — Variación de Otoño");
            CreateBallerinaStatue("Estatua_Bailarina_Jardin_NorteCentro", zone40.transform,
                new Vector3(69.0f, gardenY, 84.5f), matStatue, matWarmGreyStone,
                "Estatua Clásica IV — Reverencia del Primer Acto");
            CreateBallerinaStatue("Estatua_Bailarina_Jardin_SurCentro", zone40.transform,
                new Vector3(69.0f, gardenY, 28.5f), matStatue, matWarmGreyStone,
                "Estatua Clásica V — El Cisne en Reposo");
            CreateBallerinaStatue("Estatua_Bailarina_Jardin_EsteCentro", zone40.transform,
                new Vector3(86.5f, gardenY, 58.0f), matStatue, matWarmGreyStone,
                "Estatua Clásica VI — Pas de Deux Nocturno");

            // Controlador de anomalía de estatuas que giran al apartar la vista + melodía de piano distante
            GameObject statueAnomalyHost = new GameObject("Anomaly_WinterGarden_StatuesAndDistantPiano");
            statueAnomalyHost.transform.parent = zone40.transform;
            statueAnomalyHost.transform.position = new Vector3(62.5f, gardenY + 1.5f, 58.0f);
            WinterGardenAnomalyController statuesAnomCtrl = statueAnomalyHost.AddComponent<WinterGardenAnomalyController>();
            statuesAnomCtrl.Configure(WinterGardenNodeType.AnomalyWeepingBallerinaStatuesAndPiano, statueNW.transform, statueSW.transform);
            EditorUtility.SetDirty(statuesAnomCtrl);

            // ESTATUA CONMEMORATIVA DE ELENA VESPER EN EL PABELLÓN NORESTE (X = 82.0, Z = 79.0)
            GameObject elenaMemorialStatue = new GameObject("Estatua_Conmemorativa_Elena_Vesper");
            elenaMemorialStatue.transform.parent = zone40.transform;
            elenaMemorialStatue.transform.position = new Vector3(82.0f, gardenY, 79.0f);
            CreateBallerinaStatue("Mesh_Estatua_Elena_Vesper", elenaMemorialStatue.transform, Vector3.zero, matStatue, matWarmGreyStone,
                "Estatua Conmemorativa — Elena Vesper");
            DecoPiece("Plataforma_Pabellon_Elena", PrimitiveType.Cylinder, elenaMemorialStatue.transform,
                new Vector3(0f, 0.08f, 0f), new Vector3(4.8f, 0.08f, 4.8f), matWarmGreyStone);

            // Placa conmemorativa de latón antiguo al pie de la estatua de Elena (Interactiva)
            GameObject memorialPlaqueTrigger = CreatePrimitive("Placa_Conmemorativa_Elena_Jardin", PrimitiveType.Cube, elenaMemorialStatue.transform,
                new Vector3(-0.92f, 1.05f, 0f), new Vector3(0.18f, 0.56f, 0.84f), matGold);
            WinterGardenAnomalyController memorialCtrl = memorialPlaqueTrigger.AddComponent<WinterGardenAnomalyController>();
            memorialCtrl.Configure(WinterGardenNodeType.InteractiveElenaMemorialStatue, null, null, appearingRelicGroup);
            EditorUtility.SetDirty(memorialCtrl);

            // ====================================================================
            // 7. RINCÓN OCULTO TRAS LA VEGETACIÓN (X = 86.5, Z = 27.5): LOSA DESPLAZADA Y CAJA METÁLICA SEMIENTERRADA
            // ====================================================================
            GameObject hiddenCorner = new GameObject("Rincon_Oculto_Losa_Y_Caja_Enterrada");
            hiddenCorner.transform.parent = zone40.transform;
            hiddenCorner.transform.position = new Vector3(86.5f, gardenY, 27.5f);

            // Hueco en la tierra húmeda, losa de piedra desplazada y pequeña caja metálica parcialmente enterrada
            DecoPiece("Hueco_Tierra_Excavada", PrimitiveType.Cube, hiddenCorner.transform,
                new Vector3(0f, 0.02f, 0f), new Vector3(1.25f, 0.04f, 1.05f), matShadowFigure);
            GameObject displacedSlab = CreatePrimitive("Losa_Piedra_Desplazada", PrimitiveType.Cube, hiddenCorner.transform,
                new Vector3(0.32f, 0.08f, 0.16f), new Vector3(1.18f, 0.15f, 0.94f), matWarmGreyStone);
            displacedSlab.transform.localRotation = Quaternion.Euler(0f, 18f, 0f);

            GameObject buriedMetalBox = CreatePrimitive("Caja_Metalica_Parcialmente_Enterrada", PrimitiveType.Cube, hiddenCorner.transform,
                new Vector3(-0.15f, 0.12f, -0.05f), new Vector3(0.52f, 0.24f, 0.38f), matOxidizedBlackIron);
            buriedMetalBox.transform.localRotation = Quaternion.Euler(8f, -12f, 5f);
            DecoPiece("Detalle_Laton_Caja_Enterrada", PrimitiveType.Cube, buriedMetalBox.transform,
                new Vector3(0f, 0.52f, 0f), new Vector3(0.82f, 0.08f, 0.25f), matGold);

            WinterGardenAnomalyController hiddenCornerCtrl = buriedMetalBox.AddComponent<WinterGardenAnomalyController>();
            hiddenCornerCtrl.Configure(WinterGardenNodeType.InteractiveHiddenSlabAndMetalBox, displacedSlab.transform, buriedMetalBox.transform);
            EditorUtility.SetDirty(hiddenCornerCtrl);

            CreateInteractiveWorldDoc("Doc_Foto_Y_Carta_Rincon_Oculto", hiddenCorner.transform,
                new Vector3(-0.15f, 0.28f, 0.38f), new Vector3(0.38f, 0.04f, 0.28f), matGold,
                "doc_s9_buried_photo_and_letter",
                "Fotografía del 17 de Octubre y Fragmento de Carta — Contradicción con la Desaparición Oficial",
                "RINCÓN OCULTO DEL JARDÍN DE INVIERNO — PRUEBA FÍSICA\n\n" +
                "• **Fotografía Antigua**: Tomada junto a la fuente de los cisnes la noche del **17 de octubre de 1924** (tres días después de la fecha oficial de desaparición). Al dorso figura la referencia del **Archivo Musical (Sección 5)** y de **Administración (Sección 2)**.\n" +
                "• **Fragmento de Carta**:\n" +
                "  «Enterramos esta caja aquí antes de que tapiaran las puertas del teatro. La inscripción que Mara grabó al pie de la estatua del noreste dice la verdad: el 17 de octubre Elena seguía con vida dentro de la academia»");

            // ====================================================================
            // 8. VEGETACIÓN ABUNDANTE PERO CONTROLADA: ÁRBOLES DE INVERNADERO, HIEDRA, HELECHOS, RAÍCES Y ARBUSTOS
            // ====================================================================
            GameObject vegetationGroup = new GameObject("WinterGarden_Controlled_Vegetation");
            vegetationGroup.transform.parent = zone40.transform;

            // Árboles antiguos de invernadero en los cuatro cuadrantes ajardinados
            Vector3[] gardenTrees =
            {
                new Vector3(62.5f, gardenY, 75.0f),
                new Vector3(75.5f, gardenY, 75.0f),
                new Vector3(62.5f, gardenY, 39.0f),
                new Vector3(75.5f, gardenY, 39.0f),
                new Vector3(88.5f, gardenY, 68.0f),
                new Vector3(88.5f, gardenY, 46.0f)
            };
            for (int gt = 0; gt < gardenTrees.Length; gt++)
            {
                CreateStageTreeProp($"Arbol_Invernadero_{gt + 1}", vegetationGroup.transform, gardenTrees[gt], matMahogany, matOliveDark);
            }

            // Hiedra y plantas trepadoras subiendo por los muros perimetrales
            float[] ivyZ = { 28f, 40f, 52f, 66f, 78f, 87f };
            for (int v = 0; v < ivyZ.Length; v++)
            {
                DecoPiece($"Hiedra_Muro_Este_{v + 1}", PrimitiveType.Cube, vegetationGroup.transform,
                    new Vector3(92.55f, gardenY + 2.8f, ivyZ[v]), new Vector3(0.18f, 4.8f, 4.2f), matOliveDark);
                DecoPiece($"Hiedra_Muro_Oeste_{v + 1}", PrimitiveType.Cube, vegetationGroup.transform,
                    new Vector3(45.45f, gardenY + 2.6f, ivyZ[v] - 1.5f), new Vector3(0.18f, 4.4f, 3.6f), matMossGreen);
            }

            // Arbustos, helechos y macizos vegetales que enmarcan los paseos y ocultan el rincón sureste (X = 86.5, Z = 27.5)
            Vector3[] bushCoords =
            {
                new Vector3(83.2f, gardenY + 0.72f, 30.8f),
                new Vector3(86.2f, gardenY + 0.78f, 31.4f),
                new Vector3(89.5f, gardenY + 0.74f, 30.2f),
                new Vector3(89.8f, gardenY + 0.70f, 25.8f),
                new Vector3(50.2f, gardenY + 0.68f, 27.5f),
                new Vector3(50.2f, gardenY + 0.68f, 46.5f),
                new Vector3(50.2f, gardenY + 0.68f, 69.5f),
                new Vector3(50.2f, gardenY + 0.68f, 86.5f),
                new Vector3(62.5f, gardenY + 0.65f, 64.0f),
                new Vector3(75.5f, gardenY + 0.65f, 64.0f),
                new Vector3(62.5f, gardenY + 0.65f, 51.5f),
                new Vector3(75.5f, gardenY + 0.65f, 51.5f),
                new Vector3(88.5f, gardenY + 0.72f, 84.5f),
                new Vector3(76.0f, gardenY + 0.68f, 86.5f)
            };
            for (int b = 0; b < bushCoords.Length; b++)
            {
                DecoPiece($"Arbusto_Oliva_{b + 1}", PrimitiveType.Sphere, vegetationGroup.transform,
                    bushCoords[b], new Vector3(2.6f, 1.55f, 2.4f), (b % 2 == 0) ? matOliveDark : matHedge);
            }

            // Raíces retorcidas que invaden los bordes de los senderos de piedra
            float[] rootZ = { 36.5f, 49.0f, 62.5f, 75.0f, 83.0f };
            for (int rt = 0; rt < rootZ.Length; rt++)
            {
                DecoPiece($"Raiz_Sendero_{rt + 1}", PrimitiveType.Cylinder, vegetationGroup.transform,
                    new Vector3(67.2f + (rt % 2) * 3.6f, gardenY + 0.05f, rootZ[rt]), new Vector3(0.14f, 1.8f, 0.14f), matMahogany).transform.localRotation = Quaternion.Euler(86f, 32f * rt, 0f);
            }

            // ====================================================================
            // 9. ILUMINACIÓN PRINCIPALMENTE FRÍA Y LUNAR + LUCES CÁLIDAS DÉBILES DE LA ACADEMIA
            // ====================================================================
            CreatePictureSpotlight("Luz_Lunar_Sobre_Fuente_Seca", zone40.transform,
                new Vector3(69.0f, glassApexY - 0.5f, 58.0f), new Vector3(69.0f, gardenY + 0.5f, 58.0f),
                new Color(0.60f, 0.78f, 0.98f), 4.2f);
            CreatePictureSpotlight("Luz_Lunar_Sobre_Estatua_Elena", zone40.transform,
                new Vector3(82.0f, glassApexY - 0.5f, 79.0f), new Vector3(82.0f, gardenY + 1.0f, 79.0f),
                new Color(0.62f, 0.80f, 1.0f), 3.6f);
            CreatePictureSpotlight("Luz_Lunar_Rincon_Oculto_SE", zone40.transform,
                new Vector3(86.5f, glassApexY - 0.8f, 27.5f), new Vector3(86.5f, gardenY + 0.2f, 27.5f),
                new Color(0.55f, 0.74f, 0.96f), 2.8f);

            // Luces cálidas débiles provenientes de las entradas de la academia en el muro oeste (X = 45.4)
            CreateOrnateSconce("Luz_Calida_Debil_Entrada_Academia_N", zone40.transform,
                new Vector3(45.4f, gardenY + 3.1f, 79.0f), matOxidizedBlackIron, matLanternGlow, new Color(1.0f, 0.78f, 0.44f), 1.45f);
            CreateOrnateSconce("Luz_Calida_Debil_Entrada_Mantenimiento", zone40.transform,
                new Vector3(45.4f, gardenY + 3.1f, 59.4f), matOxidizedBlackIron, matLanternGlow, new Color(1.0f, 0.78f, 0.44f), 1.35f);
            CreateOrnateSconce("Luz_Calida_Debil_Entrada_Sur", zone40.transform,
                new Vector3(45.4f, gardenY + 3.1f, 34.8f), matOxidizedBlackIron, matLanternGlow, new Color(1.0f, 0.78f, 0.44f), 1.35f);
        }

        private static void CreateWinterGardenIronBench(string name, Transform parent, Vector3 pos, Quaternion rot, Material ironMat, Material woodMat)
        {
            GameObject bench = new GameObject(name);
            bench.transform.parent = parent;
            bench.transform.position = pos;
            bench.transform.rotation = rot;

            CreatePrimitive("Bench_Seat", PrimitiveType.Cube, bench.transform, new Vector3(0f, 0.44f, 0f), new Vector3(1.85f, 0.08f, 0.58f), woodMat);
            CreatePrimitive("Bench_Backrest", PrimitiveType.Cube, bench.transform, new Vector3(0f, 0.82f, -0.24f), new Vector3(1.85f, 0.48f, 0.08f), ironMat);
            DecoPiece("Bench_Arm_L", PrimitiveType.Cube, bench.transform, new Vector3(-0.88f, 0.48f, 0f), new Vector3(0.08f, 0.62f, 0.60f), ironMat);
            DecoPiece("Bench_Arm_R", PrimitiveType.Cube, bench.transform, new Vector3(0.88f, 0.48f, 0f), new Vector3(0.08f, 0.62f, 0.60f), ironMat);
        }

        // ====================================================================
        // CONSTRUCCIÓN DE LA SECCIÓN 10: EXPANSIÓN ARQUITECTÓNICA DE LA ACADEMIA (ZONAS 41 A 44)
        // - 41. Sótano de Mantenimiento (Nivel subterráneo Y = -3.2m, tuberías acústicas, depósitos de agua, ruta alternativa y secreto del consumo del teatro)
        // - 42. Terraza y Azotea Mirador (Nivel superior Y = 7.70m, balaustradas, macetas secas, vistas a patios/teatro y secreto de la ventana que no figura en planos)
        // - 43. Enfermería Antigua (Ala Suroeste Y = 2.4m, camillas, biombos, botica médica y secreto del registro clínico de Elena con página arrancada)
        // - 44. Sala Conmemorativa (Ala Suroeste Y = 2.4m, bancos de madera, flores secas, retratos, placas con nombres y secreto de la placa retirada de la pared)
        // ====================================================================
        private static void BuildSection10ExpansionZones(
            Transform rootEnv, float vestY,
            Material matParquet, Material matCarpet, Material matWallIvory, Material matMahogany,
            Material matGold, Material matIron, Material matClothCover, Material matVelvetDrapes,
            Material matLanternGlow, Material matColdFluorescent, Material matGreenVelvet,
            Material matLeather, Material matWoodArchive, Material matMetalArchive, Material matCardboard,
            Material matStoneWall, Material matRustedLocker, Material matMirrorGlass, Material matPorcelain,
            Material matTutuWhite, Material matTutuBlack, Material matSatinRibbon, Material matShadowFigure,
            Material matPortraitElena, Material matPhotoTroupe1923, Material matCobble, Material matStatue, Material matHedge,
            Material matCeramicTile, Material matFacade, Material matGreenGlass)
        {
            Material matAgedBrick = GetOrCreateBasicMaterial("Mat_S10_AgedBrick", new Color(0.27f, 0.17f, 0.14f), 0.18f, 0.02f);
            Material matCopperPipe = GetOrCreateBasicMaterial("Mat_S10_AgedCopperPipe", new Color(0.56f, 0.34f, 0.22f), 0.62f, 0.78f);
            Material matBoilerTank = GetOrCreateBasicMaterial("Mat_S10_BoilerIronTank", new Color(0.18f, 0.20f, 0.21f), 0.42f, 0.82f);
            Material matDryFlowers = GetOrCreateBasicMaterial("Mat_S10_DriedRosesAndLaurel", new Color(0.38f, 0.24f, 0.18f), 0.12f, 0.0f);
            Material matWallScar = GetOrCreateBasicMaterial("Mat_S10_RemovedPlaqueWallScar", new Color(0.13f, 0.08f, 0.05f), 0.12f, 0.0f);
            Material matAmberBottle = GetOrCreateBasicMaterial("Mat_S10_AmberMedicalBottle", new Color(0.52f, 0.28f, 0.08f), 0.88f, 0.10f);
            Material matWarmWindowGlow = GetOrCreateEmissiveMaterial("Mat_S10_UnmappedWindowGlow", new Color(1.0f, 0.72f, 0.32f));

            // Materiales adicionales para la Sección 12 (Zonas 49 a 59) — mismos assets que en las secciones 1, 7 y 8
            Material matSecretPaper = GetOrCreateBasicMaterial("Mat_S8_ArchivalPaper", new Color(0.84f, 0.77f, 0.60f), 0.16f, 0.0f);
            Material matDarkCement = GetOrCreateBasicMaterial("Mat_S8_SubterraneanCement", new Color(0.17f, 0.18f, 0.19f), 0.22f, 0.04f);
            Material matCatwalkSteel = GetOrCreateBasicMaterial("Mat_S7_CatwalkSteel", new Color(0.22f, 0.24f, 0.27f), 0.45f, 0.65f);
            Material matTheatreDarkWood = GetOrCreateBasicMaterial("Mat_S7_TheatreStageWood", new Color(0.18f, 0.12f, 0.09f), 0.42f, 0.04f);
            Material matPuddle = GetOrCreateBasicMaterial("Mat_S1_Puddle", new Color(0.10f, 0.13f, 0.18f), 0.98f, 0.15f);
            Material matMarble = GetOrCreateBasicMaterial("Mat_S12_FoundersMarble", new Color(0.78f, 0.76f, 0.70f), 0.70f, 0.10f);

            // ====================================================================
            // ZONA 41: SÓTANO DE MANTENIMIENTO (NIVEL SUBTERRÁNEO Y = -3.2m)
            // Tres subáreas interconectadas + Doble ruta vertical (Lavabos Y=2.4m <-> Sótano Y=-3.2m <-> Callejón Servicio/Jardín de Invierno Y=0.0m)
            // ====================================================================
            GameObject zone41 = new GameObject("41_Maintenance_Basement_Subterranean");
            zone41.transform.parent = rootEnv;
            float baseFloorY = -3.2f;
            float baseH = 4.4f; // Techo en Y = +1.2m

            // 1. Descansillo Superior Norte (X: 16..22, Z: -6.5..0, Y = 2.4m) conectado a Door_Washroom_To_MaintenanceBasement (19, 2.4, 0)
            CreatePrimitive("Basement_NorthUpperLanding_Floor", PrimitiveType.Cube, zone41.transform,
                new Vector3(19.0f, vestY - 0.25f, -3.25f), new Vector3(6.0f, 0.5f, 6.5f), matStoneWall);
            CreatePrimitive("Basement_NorthUpperLanding_Ceiling", PrimitiveType.Cube, zone41.transform,
                new Vector3(26.5f, vestY + 4.8f, -3.25f), new Vector3(21.0f, 0.5f, 6.5f), matStoneWall);
            CreateWall("Basement_NorthStair_Wall_West", zone41.transform,
                new Vector3(16.0f, vestY + 2.2f, -3.25f), new Vector3(0.5f, 5.2f, 6.5f), matAgedBrick);
            CreateWall("Basement_NorthStair_Wall_SouthUpper", zone41.transform,
                new Vector3(26.5f, 2.0f, -6.5f), new Vector3(21.0f, 10.5f, 0.5f), matAgedBrick);
            CreateWall("Basement_NorthStair_Wall_NorthEastUpper", zone41.transform,
                new Vector3(31.5f, 2.0f, 0.0f), new Vector3(11.0f, 10.5f, 0.5f), matAgedBrick);

            // Escalera Norte (X: 22.0m a 35.0m, Z = -3.25m): baja de Y = 2.4m a Y = -3.2m (desnivel 5.6m en 13.0m)
            int nSteps = 24;
            float nStepLen = 13.0f / nSteps;
            float nStepDrop = (vestY - baseFloorY) / nSteps;
            for (int s = 0; s < nSteps; s++)
            {
                float sx = 22.0f + (s + 0.5f) * nStepLen;
                float sy = vestY - (s + 1) * nStepDrop;
                CreatePrimitive($"Peldaño_Sotano_Norte_{s + 1}", PrimitiveType.Cube, zone41.transform,
                    new Vector3(sx, sy - 0.12f, -3.25f), new Vector3(nStepLen + 0.06f, 0.25f, 4.8f), matStoneWall);
            }
            GameObject northBasementRamp = new GameObject("Basement_NorthStair_SmoothRamp");
            northBasementRamp.transform.parent = zone41.transform;
            northBasementRamp.transform.localPosition = new Vector3(28.5f, (vestY + baseFloorY) * 0.5f - 0.1f, -3.25f);
            northBasementRamp.transform.localRotation = Quaternion.Euler(0f, 0f, -23.3f);
            BoxCollider nbRampCol = northBasementRamp.AddComponent<BoxCollider>();
            nbRampCol.size = new Vector3(14.4f, 0.35f, 4.8f);

            // Descansillo inferior de la Escalera Norte (X: 35..40, Z: -14..0, Y = -3.2m)
            CreatePrimitive("Basement_NorthLowerHall_Floor", PrimitiveType.Cube, zone41.transform,
                new Vector3(37.0f, baseFloorY - 0.25f, -7.0f), new Vector3(6.0f, 0.5f, 14.0f), matCobble);
            CreatePrimitive("Basement_NorthLowerHall_Ceiling", PrimitiveType.Cube, zone41.transform,
                new Vector3(37.0f, baseFloorY + baseH, -10.25f), new Vector3(6.0f, 0.5f, 7.5f), matStoneWall);
            CreateWall("Basement_NorthLowerHall_Wall_W", zone41.transform,
                new Vector3(34.0f, baseFloorY + 2.2f, -10.25f), new Vector3(0.5f, baseH, 7.5f), matAgedBrick);
            CreateWall("Basement_NorthLowerHall_Wall_E", zone41.transform,
                new Vector3(40.0f, 1.0f, -7.0f), new Vector3(0.5f, 9.0f, 14.0f), matAgedBrick);
            CreateWall("Basement_NorthLowerHall_Wall_N", zone41.transform,
                new Vector3(37.0f, 1.0f, 0.0f), new Vector3(6.0f, 9.0f, 0.5f), matAgedBrick);

            // 2. Planta Subterránea Principal del Sótano de Mantenimiento (X: 16.0m a 54.0m, Z: -34.0m a -14.0m, Y = -3.2m)
            // Dimensiones: 38m de ancho x 20m de fondo = 760 m² divididos en 3 cámaras funcionales conectadas
            CreatePrimitive("Basement_Main_Floor", PrimitiveType.Cube, zone41.transform,
                new Vector3(35.0f, baseFloorY - 0.25f, -24.0f), new Vector3(38.0f, 0.5f, 20.0f), matCobble);
            // El techo cubre todo excepto el hueco de la Escalera Este (X: 39.5..44.5, Z: -22..-14)
            CreatePrimitive("Basement_Ceiling_WestAndCenter", PrimitiveType.Cube, zone41.transform,
                new Vector3(27.75f, baseFloorY + baseH, -24.0f), new Vector3(23.5f, 0.5f, 20.0f), matStoneWall);
            CreatePrimitive("Basement_Ceiling_SouthEast", PrimitiveType.Cube, zone41.transform,
                new Vector3(46.75f, baseFloorY + baseH, -28.0f), new Vector3(14.5f, 0.5f, 12.0f), matStoneWall);
            CreatePrimitive("Basement_Ceiling_FarEast", PrimitiveType.Cube, zone41.transform,
                new Vector3(49.25f, baseFloorY + baseH, -18.0f), new Vector3(9.5f, 0.5f, 8.0f), matStoneWall);

            // Muros perimetrales del Sótano de Mantenimiento
            // Conexión Este (X = 16.0m, Z: -24..-14) hacia la Red Subterránea (Zonas 53 a 56)
            CreateWall("Basement_OuterWall_West_S", zone41.transform,
                new Vector3(16.0f, baseFloorY + 2.2f, -29.0f), new Vector3(0.6f, baseH, 10.0f), matAgedBrick);
            CreateWall("Basement_OuterWall_West_N", zone41.transform,
                new Vector3(16.0f, baseFloorY + 2.2f, -16.5f), new Vector3(0.6f, baseH, 5.0f), matAgedBrick);
            CreateArchitecturalDoor("Door_Basement_To_BoilerAndCrypt", zone41.transform,
                new Vector3(16.0f, baseFloorY, -21.5f), Quaternion.Euler(0f, 90f, 0f),
                2.0f, 3.2f, baseH, 0.6f, false, "",
                "RED SUBTERRÁNEA — SALA DE CALDERAS Y CRIPTA (ZONAS 53 A 56)", "", matIron, matGold, matAgedBrick);
            CreateWall("Basement_OuterWall_South", zone41.transform,
                new Vector3(35.0f, baseFloorY + 2.2f, -34.0f), new Vector3(38.0f, baseH, 0.6f), matAgedBrick);
            CreateWall("Basement_OuterWall_East", zone41.transform,
                new Vector3(54.0f, baseFloorY + 2.2f, -24.0f), new Vector3(0.6f, baseH, 20.0f), matAgedBrick);
            // Muro Norte del sótano (Z = -14.0m), dejando abiertos:
            // - Paso a la Escalera Norte en X: 34.5..39.5
            // - Paso a la Escalera Este en X: 39.5..44.5
            CreateWall("Basement_OuterWall_North_W", zone41.transform,
                new Vector3(25.0f, baseFloorY + 2.2f, -14.0f), new Vector3(18.0f, baseH, 0.6f), matAgedBrick);
            CreateWall("Basement_OuterWall_North_E", zone41.transform,
                new Vector3(49.25f, baseFloorY + 2.2f, -14.0f), new Vector3(9.5f, baseH, 0.6f), matAgedBrick);

            // 3. Escalera Este de Ruta Alternativa (X: 39.5..44.5, Z: -22.0m en Y = -3.2m hasta Z = -12.0m en Y = 0.0m)
            // Conecta el Sótano directamente con el Callejón de Servicio Exterior, el Patio de Entrada y el Jardín de Invierno
            int eSteps = 16;
            float eStepLen = 10.0f / eSteps;
            float eStepRise = (0.0f - baseFloorY) / eSteps;
            for (int s = 0; s < eSteps; s++)
            {
                float sz = -22.0f + (s + 0.5f) * eStepLen;
                float sy = baseFloorY + (s + 0.5f) * eStepRise;
                CreatePrimitive($"Peldaño_Sotano_Este_{s + 1}", PrimitiveType.Cube, zone41.transform,
                    new Vector3(42.0f, sy - 0.1f, sz), new Vector3(4.6f, 0.22f, eStepLen + 0.05f), matStoneWall);
            }
            GameObject eastBasementRamp = new GameObject("Basement_EastStair_SmoothRamp");
            eastBasementRamp.transform.parent = zone41.transform;
            eastBasementRamp.transform.localPosition = new Vector3(42.0f, baseFloorY * 0.5f, -17.0f);
            eastBasementRamp.transform.localRotation = Quaternion.Euler(-17.75f, 0f, 0f);
            BoxCollider ebRampCol = eastBasementRamp.AddComponent<BoxCollider>();
            ebRampCol.size = new Vector3(4.6f, 0.35f, 10.8f);

            // Muretes de protección alrededor de la boca de la Escalera Este en superficie (Y = 0.0m)
            CreateWall("Basement_EastStair_Guard_W", zone41.transform,
                new Vector3(39.5f, -0.4f, -17.0f), new Vector3(0.4f, 3.6f, 10.0f), matAgedBrick);
            CreateWall("Basement_EastStair_Guard_E", zone41.transform,
                new Vector3(44.5f, -0.4f, -17.0f), new Vector3(0.4f, 3.6f, 10.0f), matAgedBrick);

            // Tabiques interiores con amplios pasos arqueados entre las 3 subáreas del Sótano:
            // - Subárea 41A (Galería de Tuberías Acústicas: X 16..29)
            // - Subárea 41B (Sala de Depósitos de Agua y Estanterías: X 29..43)
            // - Subárea 41C (Cuarto de Guardia del Ingeniero y Contadores del Teatro: X 43..54)
            CreateWall("Basement_Partition_41A_41B_N", zone41.transform,
                new Vector3(29.0f, baseFloorY + 2.2f, -18.0f), new Vector3(0.5f, baseH, 8.0f), matAgedBrick);
            CreateWall("Basement_Partition_41A_41B_S", zone41.transform,
                new Vector3(29.0f, baseFloorY + 2.2f, -30.5f), new Vector3(0.5f, baseH, 7.0f), matAgedBrick);
            CreateWall("Basement_Partition_41B_41C_S", zone41.transform,
                new Vector3(43.0f, baseFloorY + 2.2f, -30.5f), new Vector3(0.5f, baseH, 7.0f), matAgedBrick);

            // --- SUBÁREA 41A: CORREDOR DE TUBERÍAS ACÚSTICAS Y RESONANCIA DE LA ACADEMIA (X: 16..29, Z: -34..-14) ---
            for (int p = 0; p < 4; p++)
            {
                float pz = -17.0f - p * 4.5f;
                // Tuberías horizontales en el techo que conducen el sonido de las salas superiores
                DecoPiece($"Tuberia_Techo_Cobre_{p + 1}", PrimitiveType.Cylinder, zone41.transform,
                    new Vector3(28.0f, baseFloorY + 3.85f, pz), new Vector3(0.24f, 11.5f, 0.24f), matCopperPipe).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                DecoPiece($"Tuberia_Techo_Hierro_{p + 1}", PrimitiveType.Cylinder, zone41.transform,
                    new Vector3(28.0f, baseFloorY + 3.50f, pz + 0.55f), new Vector3(0.32f, 11.5f, 0.32f), matRustedLocker).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }

            // Colector vertical acústico en el muro oeste (X = 17.2m, Z = -24.0m) con volante de válvula interactivo
            GameObject acousticManifold = new GameObject("Colector_Acustico_Paredes_Academia");
            acousticManifold.transform.parent = zone41.transform;
            acousticManifold.transform.position = new Vector3(17.2f, baseFloorY, -24.0f);

            for (int c = -2; c <= 2; c++)
            {
                DecoPiece($"Conducto_Vertical_Acustico_{c + 3}", PrimitiveType.Cylinder, acousticManifold.transform,
                    new Vector3(0f, 2.2f, c * 1.1f), new Vector3(0.28f, 2.2f, 0.28f), (c % 2 == 0) ? matCopperPipe : matBoilerTank);
            }
            GameObject valveWheel = CreatePrimitive("Volante_Valvula_Resonancia", PrimitiveType.Cylinder, acousticManifold.transform,
                new Vector3(0.35f, 1.45f, 0f), new Vector3(0.65f, 0.06f, 0.65f), matGold);
            valveWheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            AcademyExpansionAnomalyController pipeAnomalyCtrl = valveWheel.AddComponent<AcademyExpansionAnomalyController>();
            pipeAnomalyCtrl.Configure(ExpansionNodeType.BasementAcousticPipesAnomaly, valveWheel.transform);
            EditorUtility.SetDirty(pipeAnomalyCtrl);

            CreateInteractiveWorldDoc("Doc_Cuaderno_Acustica_Sotano", zone41.transform,
                new Vector3(21.5f, baseFloorY + 0.96f, -16.2f), new Vector3(0.38f, 0.05f, 0.28f), matGold,
                "doc_s10_basement_pipe_acoustics",
                "Nota Técnica — Transmisión de Sonidos por los Conductos de Calefacción",
                "Anotación del operario de calderas:\n\n" +
                "«Las tuberías de cobre que suben desde este muro oeste atraviesan directamente las aulas de ballet, la sala del piano y el foso del teatro.\n" +
                "De noche, cuando el edificio queda en silencio, basta con acercarse al colector para oír con total nitidez lo que ocurre arriba: pasos de ensayo sobre la madera, compases del piano y voces apagadas detrás de los muros.»");
            CreatePrimitive("Mesa_Inspeccion_Tuberias", PrimitiveType.Cube, zone41.transform,
                new Vector3(21.5f, baseFloorY + 0.46f, -16.2f), new Vector3(1.8f, 0.92f, 0.9f), matWoodArchive);

            // --- SUBÁREA 41B: SALA DE GRANDES DEPÓSITOS DE AGUA, ESTANTERÍAS Y CAJAS (X: 29..43, Z: -34..-14) ---
            float[] tankZ = { -19.5f, -25.0f, -30.5f };
            for (int t = 0; t < tankZ.Length; t++)
            {
                GameObject waterTank = new GameObject($"Deposito_Agua_Antiguo_{t + 1}");
                waterTank.transform.parent = zone41.transform;
                waterTank.transform.position = new Vector3(33.2f, baseFloorY, tankZ[t]);

                CreatePrimitive("Bancada_Piedra_Deposito", PrimitiveType.Cube, waterTank.transform,
                    new Vector3(0f, 0.25f, 0f), new Vector3(3.4f, 0.5f, 3.4f), matStoneWall);
                CreatePrimitive("Cilindro_Deposito_Hierro", PrimitiveType.Cylinder, waterTank.transform,
                    new Vector3(0f, 2.0f, 0f), new Vector3(3.0f, 1.55f, 3.0f), matBoilerTank);
                DecoPiece("Anillo_Laton_Superior", PrimitiveType.Cylinder, waterTank.transform,
                    new Vector3(0f, 3.1f, 0f), new Vector3(3.08f, 0.08f, 3.08f), matGold);
                DecoPiece("Manometro_Presion", PrimitiveType.Cylinder, waterTank.transform,
                    new Vector3(1.55f, 1.65f, 0f), new Vector3(0.32f, 0.05f, 0.32f), matPorcelain).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }

            // Estanterías metálicas y cajas de suministros en la zona central del sótano
            for (int sh = 0; sh < 3; sh++)
            {
                float sz = -18.5f - sh * 5.2f;
                CreatePrimitive($"Estanteria_Mantenimiento_{sh + 1}", PrimitiveType.Cube, zone41.transform,
                    new Vector3(39.8f, baseFloorY + 1.35f, sz), new Vector3(1.1f, 2.7f, 3.2f), matMetalArchive);
                CreatePrimitive($"Caja_Repuestos_Sotano_{sh + 1}", PrimitiveType.Cube, zone41.transform,
                    new Vector3(38.6f, baseFloorY + 0.42f, sz + 0.8f), new Vector3(0.85f, 0.84f, 0.85f), matCardboard);
            }

            // --- SUBÁREA 41C: CUARTO DE GUARDIA DEL INGENIERO Y SECRETO DEL REGISTRO DE MANTENIMIENTO (X: 43..54, Z: -34..-14) ---
            // Panel electromecánico de suministro al Teatro Principal en el muro este (X = 53.3m, Z = -25.0m)
            CreatePrimitive("Panel_Suministro_Teatro_Base", PrimitiveType.Cube, zone41.transform,
                new Vector3(53.35f, baseFloorY + 1.65f, -25.0f), new Vector3(0.45f, 2.8f, 4.6f), matBoilerTank);
            for (int m = 0; m < 4; m++)
            {
                DecoPiece($"Contador_Electrico_Sector_{m + 1}", PrimitiveType.Cube, zone41.transform,
                    new Vector3(53.08f, baseFloorY + 2.1f, -26.8f + m * 1.2f), new Vector3(0.12f, 0.48f, 0.42f), matGold);
            }

            // Escritorio del Ingeniero Jefe con el SECRETO DEL SÓTANO DE MANTENIMIENTO
            CreatePrimitive("Mesa_Guardia_Ingeniero", PrimitiveType.Cube, zone41.transform,
                new Vector3(48.8f, baseFloorY + 0.48f, -25.0f), new Vector3(2.2f, 0.96f, 1.4f), matWoodArchive);
            CreatePrimitive("Silla_Guardia_Ingeniero", PrimitiveType.Cube, zone41.transform,
                new Vector3(48.8f, baseFloorY + 0.45f, -26.2f), new Vector3(0.75f, 0.90f, 0.75f), matIron);

            GameObject secretMaintLedger = CreatePrimitive("Registro_Secreto_Mantenimiento_Teatro", PrimitiveType.Cube, zone41.transform,
                new Vector3(48.8f, baseFloorY + 1.00f, -24.85f), new Vector3(0.58f, 0.08f, 0.44f), matGold);
            AcademyExpansionAnomalyController secretMaintCtrl = secretMaintLedger.AddComponent<AcademyExpansionAnomalyController>();
            secretMaintCtrl.Configure(ExpansionNodeType.BasementSecretMaintenanceLog);
            EditorUtility.SetDirty(secretMaintCtrl);

            // Iluminación funcional cálida e industrial del Sótano de Mantenimiento
            CreateOrnateLantern("Luz_Sotano_Escalera_Norte", zone41.transform,
                new Vector3(28.5f, vestY + 3.8f, -3.25f), matIron, matLanternGlow, new Color(1.0f, 0.76f, 0.42f), 2.2f);
            CreateOrnateLantern("Luz_Sotano_Tuberias_41A", zone41.transform,
                new Vector3(22.5f, baseFloorY + 3.9f, -24.0f), matIron, matLanternGlow, new Color(1.0f, 0.74f, 0.38f), 2.4f);
            CreateOrnateLantern("Luz_Sotano_Depositos_41B", zone41.transform,
                new Vector3(36.0f, baseFloorY + 3.9f, -24.0f), matIron, matLanternGlow, new Color(1.0f, 0.78f, 0.45f), 2.5f);
            CreateOrnateLantern("Luz_Sotano_Ingeniero_41C", zone41.transform,
                new Vector3(48.8f, baseFloorY + 3.9f, -25.0f), matGold, matLanternGlow, new Color(1.0f, 0.82f, 0.50f), 2.6f);

            // ====================================================================
            // TORREÓN Y VESTÍBULO DEL ALA SUROESTE (X: -25.0m a -14.5m, Z: -4.0m a 12.0m, Y = 2.4m)
            // Conecta 6. Recepción Antigua con:
            // - 42. Terraza y Azotea Mirador (subiendo por la escalera del torreón a Y = 7.70m)
            // - 43. Enfermería Antigua (al oeste)
            // - 44. Sala Conmemorativa (al sur)
            // ====================================================================
            GameObject swTower = new GameObject("42_43_44_SouthWest_Wing_Hub_And_StairTower");
            swTower.transform.parent = rootEnv;
            float wingH = 5.0f;
            float roofY = 7.70f;

            CreatePrimitive("SW_Hub_Floor", PrimitiveType.Cube, swTower.transform,
                new Vector3(-19.75f, vestY - 0.25f, 4.0f), new Vector3(10.5f, 0.5f, 16.0f), matParquet);
            CreatePrimitive("SW_Hub_Carpet", PrimitiveType.Cube, swTower.transform,
                new Vector3(-22.2f, vestY + 0.02f, 4.0f), new Vector3(2.8f, 0.04f, 14.5f), matCarpet);
            // Techo sobre el corredor inferior oeste (X: -25..-19.35) y el rellano sur de entrada a la escalera (Z: -4..-1.2)
            CreatePrimitive("SW_Hub_Ceiling_West", PrimitiveType.Cube, swTower.transform,
                new Vector3(-22.175f, vestY + wingH, 4.0f), new Vector3(5.65f, 0.5f, 16.0f), matWallIvory);
            // Techo alto del Torreón de la Escalera (en Y = 11.2m) cubriendo toda la caja de escalera (X: -19.6..-14.2, Z: -4.2..12.2)
            CreatePrimitive("SW_Tower_HighCeiling", PrimitiveType.Cube, swTower.transform,
                new Vector3(-16.9f, roofY + 3.5f, 4.0f), new Vector3(5.4f, 0.5f, 16.5f), matFacade);

            // Muros completos de piso a techo del Torreón de la Escalera (Y = 2.4m a Y = 11.2m) para que no se vea ningún borde de techo ni cielo abierto:
            // 1. Muro Este del Torreón (X = -14.5m)
            CreateWall("SW_Tower_Wall_East", swTower.transform,
                new Vector3(-14.5f, 6.8f, 4.0f), new Vector3(0.5f, 8.8f, 16.0f), matFacade);
            // 2. Muro Sur del Torreón (Z = -4.0m, X: -19.5m a -14.5m, sube desde Y = 2.4m hasta Y = 11.2m)
            CreateWall("SW_Tower_Wall_South_Full", swTower.transform,
                new Vector3(-17.0f, 6.8f, -4.0f), new Vector3(5.2f, 8.8f, 0.5f), matFacade);
            // 3. Muro Oeste de la Caja de Escalera (X = -19.35m): deja paso de 2.8m en el extremo sur (Z: -4.0 a -1.2) en planta baja y cierra de Z = -1.2 a 12.0 hasta Y = 11.2m
            CreateWall("SW_Stair_Wall_West_Full", swTower.transform,
                new Vector3(-19.35f, 6.8f, 5.4f), new Vector3(0.4f, 8.8f, 13.2f), matWallIvory);
            CreateWall("SW_Stair_Wall_West_SouthLintel", swTower.transform,
                new Vector3(-19.35f, 8.6f, -2.6f), new Vector3(0.4f, 5.2f, 2.8f), matWallIvory);

            // 4. Muro Norte superior del Torreón (Z = 12.0m, X: -19.35..-14.5) con puerta en Y = 7.70m hacia la Terraza y Azotea
            CreateWall("SW_Tower_UpperNorth_W", swTower.transform,
                new Vector3(-18.75f, roofY + 1.75f, 11.5f), new Vector3(1.5f, 3.5f, 0.5f), matFacade);
            CreateWall("SW_Tower_UpperNorth_E", swTower.transform,
                new Vector3(-15.25f, roofY + 1.75f, 11.5f), new Vector3(1.5f, 3.5f, 0.5f), matFacade);
            CreateArchitecturalDoor("Door_Tower_To_RooftopTerrace", swTower.transform,
                new Vector3(-17.0f, roofY, 11.5f), Quaternion.identity,
                1.8f, 3.1f, 3.5f, 0.5f, false, "",
                "TERRAZA Y AZOTEA MIRADOR — VISTA PANORÁMICA (ZONA 42)", "", matMahogany, matGold, matFacade);

            // Escalera Real del Torreón hacia la Terraza y Azotea (X = -16.9m, Z: -1.0m en Y = 2.4m hasta Z = 10.2m en Y = 7.70m)
            int rSteps = 22;
            float rStepLen = 11.2f / rSteps;
            float rStepRise = (roofY - vestY) / rSteps;
            for (int s = 0; s < rSteps; s++)
            {
                float sz = -1.0f + (s + 0.5f) * rStepLen;
                float sy = vestY + (s + 0.5f) * rStepRise;
                CreatePrimitive($"Escalon_Azotea_{s + 1}", PrimitiveType.Cube, swTower.transform,
                    new Vector3(-16.9f, sy - 0.1f, sz), new Vector3(4.2f, 0.24f, rStepLen + 0.05f), matFacade);
                CreatePrimitive($"Alfombra_Escalon_Azotea_{s + 1}", PrimitiveType.Cube, swTower.transform,
                    new Vector3(-16.9f, sy + 0.03f, sz), new Vector3(2.2f, 0.02f, rStepLen), matCarpet);
            }
            GameObject roofStairRamp = new GameObject("Rooftop_Stair_Smooth_Incline");
            roofStairRamp.transform.parent = swTower.transform;
            roofStairRamp.transform.localPosition = new Vector3(-16.9f, (vestY + roofY) * 0.5f, 4.6f);
            roofStairRamp.transform.localRotation = Quaternion.Euler(-25.3f, 0f, 0f);
            BoxCollider rRampCol = roofStairRamp.AddComponent<BoxCollider>();
            rRampCol.size = new Vector3(4.2f, 0.35f, 12.5f);

            // Plataforma superior antes de la puerta de la azotea (Z: 10.2..11.8, Y = 7.70m)
            CreatePrimitive("Rooftop_UpperLanding_Floor", PrimitiveType.Cube, swTower.transform,
                new Vector3(-16.9f, roofY - 0.2f, 10.9f), new Vector3(4.6f, 0.4f, 1.8f), matCobble);

            CreateOrnateSconce("Sconce_SW_Hub_1", swTower.transform,
                new Vector3(-19.65f, vestY + 3.0f, 6.0f), matGold, matLanternGlow, new Color(1.0f, 0.82f, 0.50f), 2.1f);
            CreateOrnateSconce("Sconce_SW_Stair_Upper", swTower.transform,
                new Vector3(-14.85f, vestY + 4.8f, 5.5f), matGold, matLanternGlow, new Color(1.0f, 0.82f, 0.50f), 2.1f);

            // ====================================================================
            // ZONA 42: TERRAZA Y AZOTEA MIRADOR (Y = 7.70m, X: -38.0m a -11.0m, Z: 11.5m a 44.0m)
            // Tres subáreas: 42A. Pérgola Sur y Mirador al Patio | 42B. Jardín Seco de Balaustradas y Claraboyas | 42C. Mirador Norte y Ventana Secreta
            // ====================================================================
            GameObject zone42 = new GameObject("42_Rooftop_Terrace_And_Overlook");
            zone42.transform.parent = rootEnv;

            // Pavimento de piedra clásica sobre la azotea del Ala Oeste (27m x 32.5m)
            CreatePrimitive("Rooftop_Stone_Deck", PrimitiveType.Cube, zone42.transform,
                new Vector3(-24.5f, roofY - 0.15f, 27.75f), new Vector3(27.0f, 0.30f, 32.5f), matCobble);

            // Senderos de losa clara en cruz sobre la azotea
            CreatePrimitive("Rooftop_Walkway_NS", PrimitiveType.Cube, zone42.transform,
                new Vector3(-22.0f, roofY + 0.02f, 27.75f), new Vector3(3.6f, 0.04f, 30.5f), matFacade);
            CreatePrimitive("Rooftop_Walkway_EW", PrimitiveType.Cube, zone42.transform,
                new Vector3(-24.5f, roofY + 0.02f, 28.0f), new Vector3(24.0f, 0.04f, 3.4f), matFacade);

            // Balaustradas clásicas perimetrales + barrera de seguridad invisible para no caer por accidente mientras se observa el paisaje
            for (int bz = 0; bz < 3; bz++)
            {
                float segZ = 16.5f + bz * 10.0f;
                CreateArchitecturalBalustrade($"Rooftop_Balustrade_East_{bz + 1}", zone42.transform,
                    new Vector3(-11.2f, roofY, segZ), 9.5f, matFacade, matStoneWall);
                CreateArchitecturalBalustrade($"Rooftop_Balustrade_West_{bz + 1}", zone42.transform,
                    new Vector3(-37.8f, roofY, segZ), 9.5f, matFacade, matStoneWall);
            }
            // Muretes de antepecho clásico en el perímetro de la terraza
            // Muretes de antepecho clásico en el perímetro de la terraza (con vano y puerta en X = -11.0m, Z = 20.0m hacia la Mezzanine / Planta Alta [Zona 49])
            CreateWall("Rooftop_Parapet_East_S", zone42.transform,
                new Vector3(-11.0f, roofY + 0.55f, 15.225f), new Vector3(0.45f, 1.1f, 7.45f), matFacade);
            CreateWall("Rooftop_Parapet_East_N", zone42.transform,
                new Vector3(-11.0f, roofY + 0.55f, 33.275f), new Vector3(0.45f, 1.1f, 21.45f), matFacade);
            CreateArchitecturalDoor("Door_Rooftop_To_Mezzanine", zone42.transform,
                new Vector3(-11.0f, roofY, 20.0f), Quaternion.Euler(0f, 90f, 0f),
                1.8f, 3.2f, 4.2f, 0.45f, false, "",
                "GALERÍA SUPERIOR DEL VESTÍBULO — PLANTA ALTA (ZONA 49)", "", matMahogany, matGold, matFacade);
            CreateWall("Rooftop_Parapet_West", zone42.transform,
                new Vector3(-38.0f, roofY + 0.55f, 27.75f), new Vector3(0.45f, 1.1f, 32.5f), matFacade);
            CreateWall("Rooftop_Parapet_North", zone42.transform,
                new Vector3(-24.5f, roofY + 0.55f, 44.0f), new Vector3(27.0f, 1.1f, 0.45f), matFacade);
            CreateWall("Rooftop_Parapet_South_W", zone42.transform,
                new Vector3(-28.75f, roofY + 0.55f, 11.5f), new Vector3(18.5f, 1.1f, 0.45f), matFacade);
            CreateWall("Rooftop_Parapet_South_E", zone42.transform,
                new Vector3(-12.75f, roofY + 0.55f, 11.5f), new Vector3(3.5f, 1.1f, 0.45f), matFacade);

            // Pilares esquineros y faroles de hierro forjado en la azotea
            Vector3[] roofLanterns =
            {
                new Vector3(-11.4f, roofY, 13.0f),
                new Vector3(-11.4f, roofY, 28.0f),
                new Vector3(-11.4f, roofY, 43.2f),
                new Vector3(-37.4f, roofY, 13.0f),
                new Vector3(-37.4f, roofY, 28.0f),
                new Vector3(-37.4f, roofY, 43.2f),
                new Vector3(-22.0f, roofY, 43.2f)
            };
            for (int rl = 0; rl < roofLanterns.Length; rl++)
            {
                CreateStandingLamp($"Farol_Terraza_{rl + 1}", zone42.transform, roofLanterns[rl],
                    matIron, matLanternGlow, new Color(1.0f, 0.82f, 0.50f), 2.2f);
            }

            // --- SUBÁREA 42A: MIRADOR ESTE SOBRE EL PATIO DE ENTRADA Y JARDÍN DE INVIERNO (Z: 12..24) ---
            CreateWinterGardenIronBench("Banco_Terraza_Mirador_Patio_1", zone42.transform,
                new Vector3(-13.2f, roofY, 17.5f), Quaternion.Euler(0f, -90f, 0f), matIron, matMahogany);
            CreateWinterGardenIronBench("Banco_Terraza_Mirador_Patio_2", zone42.transform,
                new Vector3(-13.2f, roofY, 23.5f), Quaternion.Euler(0f, -90f, 0f), matIron, matMahogany);

            CreateInteractiveWorldDoc("Doc_Boceto_Orientacion_Terraza", zone42.transform,
                new Vector3(-13.1f, roofY + 0.52f, 17.5f), new Vector3(0.38f, 0.04f, 0.28f), matGold,
                "doc_s10_rooftop_courtyard_sketch",
                "Cuaderno de Orientación en la Azotea — Vista sobre los Patios",
                "Un cuaderno de dibujo olvidado sobre el banco que mira hacia el Patio de Entrada:\n\n" +
                "«Subir a la terraza después de horas de ensayo era nuestra única forma de respirar aire libre.\n" +
                "Desde esta balaustrada este se domina todo el **Patio de Entrada** y, al fondo a la derecha, brilla la gran bóveda acristalada del **Jardín de Invierno**.\n" +
                "Pero desde la semana del estreno nadie quiere acercarse a la balaustrada del norte: dicen que de madrugada se enciende una luz en una parte del tejado del teatro donde no debería existir ninguna habitación.»");

            // --- SUBÁREA 42B: JARDINERAS CON MACETAS SECAS, PÉRGOLA Y CLARABOYAS (Z: 22..35) ---
            Vector3[] dryPlanters =
            {
                new Vector3(-16.5f, roofY, 21.0f),
                new Vector3(-27.5f, roofY, 21.0f),
                new Vector3(-16.5f, roofY, 34.0f),
                new Vector3(-27.5f, roofY, 34.0f),
                new Vector3(-33.5f, roofY, 25.0f),
                new Vector3(-33.5f, roofY, 31.0f)
            };
            for (int dp = 0; dp < dryPlanters.Length; dp++)
            {
                CreatePrimitive($"Maceta_Piedra_Terraza_{dp + 1}", PrimitiveType.Cube, zone42.transform,
                    dryPlanters[dp] + new Vector3(0f, 0.42f, 0f), new Vector3(1.6f, 0.84f, 1.6f), matStoneWall);
                DecoPiece($"Arbusto_Seco_Maceta_{dp + 1}", PrimitiveType.Sphere, zone42.transform,
                    dryPlanters[dp] + new Vector3(0f, 1.05f, 0f), new Vector3(1.35f, 1.15f, 1.35f), (dp % 2 == 0) ? matDryFlowers : matHedge);
            }

            // Dos claraboyas antiguas de hierro y cristal sobre la cubierta de Administración y Archivo
            float[] skylightZ = { 19.0f, 35.0f };
            for (int sk = 0; sk < skylightZ.Length; sk++)
            {
                CreatePrimitive($"Claraboya_Marco_{sk + 1}", PrimitiveType.Cube, zone42.transform,
                    new Vector3(-31.0f, roofY + 0.35f, skylightZ[sk]), new Vector3(4.4f, 0.70f, 5.2f), matIron);
                CreatePrimitive($"Claraboya_Cristal_{sk + 1}", PrimitiveType.Cube, zone42.transform,
                    new Vector3(-31.0f, roofY + 0.72f, skylightZ[sk]), new Vector3(4.0f, 0.08f, 4.8f), matMirrorGlass);
            }

            // Mesa circular de piedra y bancos en el centro de la terraza
            CreatePrimitive("Mesa_Piedra_Terraza_Base", PrimitiveType.Cylinder, zone42.transform,
                new Vector3(-22.0f, roofY + 0.42f, 28.0f), new Vector3(0.55f, 0.42f, 0.55f), matStoneWall);
            CreatePrimitive("Mesa_Piedra_Terraza_Tablero", PrimitiveType.Cylinder, zone42.transform,
                new Vector3(-22.0f, roofY + 0.86f, 28.0f), new Vector3(1.75f, 0.06f, 1.75f), matFacade);
            CreateWinterGardenIronBench("Banco_Terraza_Centro_Oeste", zone42.transform,
                new Vector3(-24.8f, roofY, 28.0f), Quaternion.Euler(0f, 90f, 0f), matIron, matMahogany);

            // --- SUBÁREA 42C: MIRADOR NORTE HACIA EL TEJADO DEL TEATRO Y SECRETO DE LA VENTANA NO REGISTRADA (Z: 35..44) ---
            // Construimos en lo alto del ala trasera noreste del Teatro Principal (X = 18.5, Y = 10.2, Z = 106.0) el Torreón Secreto con la Ventana Iluminada
            // que se divisa desde este Mirador Norte de la Azotea pero NO aparece en los planos de Administración
            GameObject unmappedWindowStructure = new GameObject("Torreon_Oculto_Teatro_Ventana_No_Registrada");
            unmappedWindowStructure.transform.parent = zone42.transform;
            unmappedWindowStructure.transform.position = new Vector3(18.5f, 10.2f, 106.0f);

            CreatePrimitive("Muro_Alto_Secreto_Teatro", PrimitiveType.Cube, unmappedWindowStructure.transform,
                Vector3.zero, new Vector3(8.5f, 5.5f, 4.5f), matAgedBrick);
            DecoPiece("Marco_Ventana_Prohibida_Teatro", PrimitiveType.Cube, unmappedWindowStructure.transform,
                new Vector3(-1.2f, 0.4f, -2.28f), new Vector3(1.8f, 2.6f, 0.16f), matIron);
            GameObject secretWindowPane = DecoPiece("Cristal_Iluminado_Ventana_Secreta", PrimitiveType.Cube, unmappedWindowStructure.transform,
                new Vector3(-1.2f, 0.4f, -2.32f), new Vector3(1.45f, 2.25f, 0.08f), matWarmWindowGlow);
            Light unmappedWindowLight = CreatePictureSpotlight("Luz_Ventana_Oculta_No_En_Planos", unmappedWindowStructure.transform,
                new Vector3(-1.2f, 0.4f, -2.6f), new Vector3(-12.0f, 8.5f, 65.0f), new Color(1.0f, 0.72f, 0.30f), 2.6f);

            // Pedestal de observación con catalejo de latón y planos desplegados en el Mirador Norte (X = -22.0, Z = 42.6)
            GameObject northOverlook = new GameObject("Mirador_Norte_Catalejo_Y_Planos");
            northOverlook.transform.parent = zone42.transform;
            northOverlook.transform.position = new Vector3(-22.0f, roofY, 42.6f);

            CreatePrimitive("Pedestal_Mirador_Norte", PrimitiveType.Cube, northOverlook.transform,
                new Vector3(0f, 0.55f, 0f), new Vector3(1.4f, 1.1f, 0.9f), matFacade);
            GameObject brassTelescope = CreatePrimitive("Catalejo_Laton_Mirador_Teatro", PrimitiveType.Cylinder, northOverlook.transform,
                new Vector3(0f, 1.32f, 0f), new Vector3(0.16f, 0.55f, 0.16f), matGold);
            brassTelescope.transform.localRotation = Quaternion.Euler(82f, 34f, 0f);

            AcademyExpansionAnomalyController rooftopSecretCtrl = brassTelescope.AddComponent<AcademyExpansionAnomalyController>();
            rooftopSecretCtrl.Configure(ExpansionNodeType.RooftopSecretUnmappedWindow, brassTelescope.transform, null, secretWindowPane, unmappedWindowLight);
            EditorUtility.SetDirty(rooftopSecretCtrl);

            // ====================================================================
            // ZONA 43: ENFERMERÍA ANTIGUA (ALA SUROESTE, X: -42.0m a -25.0m, Z: -6.0m a 12.0m, Y = 2.4m)
            // Tres subáreas: 43A. Camillas y Biombos | 43B. Botica y Armarios Médicos | 43C. Consultorio y Registro de Alumnas con Página Arrancada
            // ====================================================================
            GameObject zone43 = new GameObject("43_Old_Infirmary");
            zone43.transform.parent = rootEnv;

            CreatePrimitive("Infirmary_Floor", PrimitiveType.Cube, zone43.transform,
                new Vector3(-33.5f, vestY - 0.25f, 3.0f), new Vector3(17.0f, 0.5f, 18.0f), matCeramicTile);
            CreatePrimitive("Infirmary_Ceiling", PrimitiveType.Cube, zone43.transform,
                new Vector3(-33.5f, vestY + wingH, 3.0f), new Vector3(17.0f, 0.5f, 18.0f), matWallIvory);

            // Muros perimetrales de la Enfermería Antigua (con puerta reservada en X = -42.0m, Z = 8.5m hacia la Galería de Vigilancia de Isadora Vale [Zona 48])
            CreateWall("Infirmary_Wall_West_S", zone43.transform,
                new Vector3(-42.0f, vestY + 2.5f, 0.725f), new Vector3(0.5f, wingH, 13.45f), matWallIvory);
            CreateWall("Infirmary_Wall_West_N", zone43.transform,
                new Vector3(-42.0f, vestY + 2.5f, 10.775f), new Vector3(0.5f, wingH, 2.45f), matWallIvory);
            CreateArchitecturalDoor("Door_Infirmary_To_IsadoraSpyGallery", zone43.transform,
                new Vector3(-42.0f, vestY, 8.5f), Quaternion.Euler(0f, 90f, 0f),
                1.8f, 3.2f, wingH, 0.5f, false, "",
                "PASADIZO DE SERVICIO Y CELOSÍA DE VIGILANCIA — DIRECCIÓN (ZONA 48)", "", matMahogany, matGold, matWallIvory);
            CreateWall("Infirmary_Wall_North", zone43.transform,
                new Vector3(-33.5f, vestY + 2.5f, 12.0f), new Vector3(17.0f, wingH, 0.5f), matWallIvory);
            CreateWall("Infirmary_Wall_South", zone43.transform,
                new Vector3(-33.5f, vestY + 2.5f, -6.0f), new Vector3(17.0f, wingH, 0.5f), matWallIvory);
            // Muro Este (X = -25.0m) con puerta hacia el Vestíbulo del Ala Suroeste en Z = 3.0m
            CreateWall("Infirmary_Wall_East_S", zone43.transform,
                new Vector3(-25.0f, vestY + 2.5f, -2.025f), new Vector3(0.5f, wingH, 7.95f), matWallIvory);
            CreateWall("Infirmary_Wall_East_N", zone43.transform,
                new Vector3(-25.0f, vestY + 2.5f, 8.025f), new Vector3(0.5f, wingH, 7.95f), matWallIvory);
            CreateArchitecturalDoor("Door_Tower_To_Infirmary", zone43.transform,
                new Vector3(-25.0f, vestY, 3.0f), Quaternion.Euler(0f, 90f, 0f),
                1.8f, 3.2f, wingH, 0.5f, false, "",
                "ENFERMERÍA ANTIGUA — REGISTROS CLÍNICOS DE ALUMNAS (ZONA 43)", "", matMahogany, matGold, matWallIvory);

            // Tabique interior semiabierto que separa el Consultorio y Archivo Clínico (Norte, Z: 5.5..12) del Pabellón de Camillas y Botica (Sur, Z: -6..5.5)
            CreateWall("Infirmary_Consult_Partition_W", zone43.transform,
                new Vector3(-38.0f, vestY + 2.5f, 5.5f), new Vector3(8.0f, wingH, 0.4f), matWallIvory);
            CreateWall("Infirmary_Consult_Partition_E", zone43.transform,
                new Vector3(-28.0f, vestY + 2.5f, 5.5f), new Vector3(6.0f, wingH, 0.4f), matWallIvory);

            // --- SUBÁREA 43A: SALA DE CAMILLAS Y BIOMBOS DE OBSERVACIÓN (X: -42..-32, Z: -6..5.5) ---
            float[] cotZ = { -3.8f, -0.2f, 3.2f };
            for (int c = 0; c < cotZ.Length; c++)
            {
                float cz = cotZ[c];
                GameObject cot = new GameObject($"Camilla_Medica_{c + 1}");
                cot.transform.parent = zone43.transform;
                cot.transform.position = new Vector3(-39.2f, vestY, cz);

                CreatePrimitive("Camilla_Bastidor", PrimitiveType.Cube, cot.transform,
                    new Vector3(0f, 0.55f, 0f), new Vector3(2.3f, 0.10f, 1.05f), matPorcelain);
                CreatePrimitive("Camilla_Colchon", PrimitiveType.Cube, cot.transform,
                    new Vector3(0f, 0.66f, 0f), new Vector3(2.2f, 0.14f, 0.96f), matClothCover);
                CreatePrimitive("Camilla_Almohada", PrimitiveType.Cube, cot.transform,
                    new Vector3(-0.82f, 0.78f, 0f), new Vector3(0.42f, 0.12f, 0.72f), matTutuWhite);
                CreatePrimitive("Camilla_Cabecero", PrimitiveType.Cube, cot.transform,
                    new Vector3(-1.12f, 0.85f, 0f), new Vector3(0.08f, 0.95f, 1.05f), matPorcelain);

                // Mesilla auxiliar con vendas y frascos
                CreatePrimitive($"Mesilla_Clinica_{c + 1}", PrimitiveType.Cube, cot.transform,
                    new Vector3(-0.6f, 0.44f, 0.82f), new Vector3(0.55f, 0.88f, 0.45f), matPorcelain);
                DecoPiece($"Frasco_Medicina_{c + 1}", PrimitiveType.Cylinder, cot.transform,
                    new Vector3(-0.6f, 0.96f, 0.82f), new Vector3(0.12f, 0.12f, 0.12f), matAmberBottle);

                // Biombo plegable junto a cada camilla
                GameObject screen = new GameObject($"Biombo_Medico_{c + 1}");
                screen.transform.parent = zone43.transform;
                screen.transform.position = new Vector3(-36.8f, vestY, cz);

                CreatePrimitive("Biombo_Panel_Central", PrimitiveType.Cube, screen.transform,
                    new Vector3(0f, 1.05f, 0f), new Vector3(0.08f, 2.0f, 1.25f), matClothCover);
                CreatePrimitive("Biombo_Marco_Madera", PrimitiveType.Cube, screen.transform,
                    new Vector3(0f, 1.05f, 0f), new Vector3(0.10f, 2.08f, 1.32f), matMahogany);

                if (c == 0)
                {
                    // Anomalía en la Camilla Nº 1 del fondo (Z = -3.8m): Silueta sentada tras el biombo que desaparece al acercarse o interactuar
                    GameObject seatedSilhouette = DecoPiece("Silueta_Alumna_Tras_Biombo", PrimitiveType.Capsule, cot.transform,
                        new Vector3(0.15f, 1.18f, 0f), new Vector3(0.46f, 0.56f, 0.36f), matShadowFigure);
                    BoxCollider scCol = screen.AddComponent<BoxCollider>();
                    scCol.center = new Vector3(0f, 1.05f, 0f);
                    scCol.size = new Vector3(0.35f, 2.1f, 1.35f);
                    AcademyExpansionAnomalyController screenCtrl = screen.AddComponent<AcademyExpansionAnomalyController>();
                    screenCtrl.Configure(ExpansionNodeType.InfirmaryScreenShadowAnomaly, screen.transform, null, seatedSilhouette);
                    EditorUtility.SetDirty(screenCtrl);
                }
            }

            // --- SUBÁREA 43B: BOTICA, LAVABO CLÍNICO Y ARMARIOS MÉDICOS ANTIGUOS (X: -32..-25, Z: -6..5.5) ---
            for (int cab = 0; cab < 2; cab++)
            {
                float cx = -30.5f + cab * 3.2f;
                GameObject medCab = new GameObject($"Armario_Medico_Antiguo_{cab + 1}");
                medCab.transform.parent = zone43.transform;
                medCab.transform.position = new Vector3(cx, vestY, -5.2f);

                CreatePrimitive("Vitrina_Cuerpo", PrimitiveType.Cube, medCab.transform,
                    new Vector3(0f, 1.25f, 0f), new Vector3(2.1f, 2.5f, 0.65f), matPorcelain);
                DecoPiece("Vitrina_Cristal", PrimitiveType.Cube, medCab.transform,
                    new Vector3(0f, 1.35f, 0.34f), new Vector3(1.85f, 1.95f, 0.04f), matMirrorGlass);
                for (int b = -2; b <= 2; b++)
                {
                    DecoPiece($"Frasco_Botica_{b + 3}", PrimitiveType.Cylinder, medCab.transform,
                        new Vector3(b * 0.32f, 1.45f, 0.15f), new Vector3(0.12f, 0.14f, 0.12f), (b % 2 == 0) ? matAmberBottle : matGreenGlass);
                }
            }

            CreateInteractiveWorldDoc("Doc_Botica_Sedantes_Direccion", zone43.transform,
                new Vector3(-28.8f, vestY + 1.02f, -4.5f), new Vector3(0.36f, 0.04f, 0.28f), matGold,
                "doc_s10_infirmary_sedatives_note",
                "Nota de Farmacia — Suministro Reservado de Hidrato de Cloral (15 de Octubre de 1924)",
                "INVENTARIO DE SUSTANCIAS CONTROLADAS — ENFERMERÍA\n\n" +
                "«Faltan tres frascos de sedante suave y dos rollos de venda quirúrgica del armario nº 2.\n" +
                "La Directora Isadora Vale retiró personalmente las llaves del botiquín la madrugada del 15 de octubre alegando que debía atender a una persona en las dependencias privadas detrás del escenario.»");

            // --- SUBÁREA 43C: CONSULTORIO Y ARCHIVO DE ALUMNAS CON EL SECRETO DE LA PÁGINA ARRANCADA (X: -42..-25, Z: 5.5..12) ---
            CreatePrimitive("Escritorio_Doctora_Enfermeria", PrimitiveType.Cube, zone43.transform,
                new Vector3(-34.0f, vestY + 0.52f, 9.2f), new Vector3(2.8f, 1.04f, 1.5f), matMahogany);
            CreatePrimitive("Silla_Doctora_Enfermeria", PrimitiveType.Cube, zone43.transform,
                new Vector3(-34.0f, vestY + 0.50f, 10.4f), new Vector3(0.85f, 1.0f, 0.85f), matLeather);

            // Ficheros de historiales médicos de alumnas en el muro norte
            for (int f = 0; f < 3; f++)
            {
                CreatePrimitive($"Archivador_Clinico_Alumnas_{f + 1}", PrimitiveType.Cube, zone43.transform,
                    new Vector3(-39.5f + f * 2.2f, vestY + 1.25f, 11.2f), new Vector3(1.6f, 2.5f, 0.75f), matMetalArchive);
            }

            CreateInteractiveWorldDoc("Doc_Ficha_Medica_Mara_Y_Alumnas", zone43.transform,
                new Vector3(-35.0f, vestY + 1.06f, 9.1f), new Vector3(0.36f, 0.04f, 0.28f), matPorcelain,
                "doc_s10_infirmary_student_files",
                "Fichas Clínicas de Alumnas — Temporada de Otoño de 1924",
                "EXTRACTO DEL ARCHIVO MÉDICO DE LA ACADEMIA\n\n" +
                "• **Mara Bell (08 Oct. 1924)**: «Inflamación severa en ambos tendones de Aquiles por exceso de repeticiones de la variación del Cisne Negro. La alumna confiesa entre lágrimas que la Directora la obliga a ensayar a medianoche para sustituir a Elena.»\n" +
                "• **Clara Valmont (11 Oct. 1924)**: «Insomnio y episodios de pánico; afirma haber oído voces discutidas bajo el suelo del escenario principal.»");

            // SECRETO DE LA ENFERMERÍA: Registro de Ingresos con la Noche Importante de Elena y la Página Siguiente Arrancada
            GameObject tornMedicalRegister = CreatePrimitive("Registro_Enfermeria_Pagina_Arrancada_Elena", PrimitiveType.Cube, zone43.transform,
                new Vector3(-33.2f, vestY + 1.08f, 9.1f), new Vector3(0.56f, 0.08f, 0.42f), matGold);
            DecoPiece("Resto_Pagina_Arrancada_Blanco", PrimitiveType.Cube, tornMedicalRegister.transform,
                new Vector3(0.55f, 0.1f, 0f), new Vector3(0.22f, 0.35f, 0.85f), matTutuWhite);
            AcademyExpansionAnomalyController infirmarySecretCtrl = tornMedicalRegister.AddComponent<AcademyExpansionAnomalyController>();
            infirmarySecretCtrl.Configure(ExpansionNodeType.InfirmarySecretTornMedicalRegister);
            EditorUtility.SetDirty(infirmarySecretCtrl);

            // Iluminación de la Enfermería (contraste entre fluorescente clínico pálido en camillas y luz cálida en consultorio)
            CreateColdFluorescentFixture("Luz_Clinica_Camillas", zone43.transform,
                new Vector3(-36.5f, vestY + 4.75f, -0.2f), matIron, matColdFluorescent, 2.0f);
            CreateOrnateLantern("Luz_Consultorio_Enfermeria", zone43.transform,
                new Vector3(-34.0f, vestY + 4.2f, 9.2f), matGold, matLanternGlow, new Color(1.0f, 0.82f, 0.52f), 2.3f);

            // ====================================================================
            // ZONA 44: SALA CONMEMORATIVA (ALA SUROESTE, X: -38.0m a -14.5m, Z: -22.0m a -4.0m, Y = 2.4m)
            // Tres subáreas: 44A. Nave de Bancos de Madera y Flores Secas | 44B. Galería de Fotografías | 44C. Muro de Placas y Secreto de la Placa Retirada
            // ====================================================================
            GameObject zone44 = new GameObject("44_Memorial_Hall");
            zone44.transform.parent = rootEnv;

            CreatePrimitive("Memorial_Floor", PrimitiveType.Cube, zone44.transform,
                new Vector3(-26.25f, vestY - 0.25f, -13.0f), new Vector3(23.5f, 0.5f, 18.0f), matParquet);
            CreatePrimitive("Memorial_Carpet_Central", PrimitiveType.Cube, zone44.transform,
                new Vector3(-24.5f, vestY + 0.02f, -13.0f), new Vector3(9.5f, 0.04f, 14.0f), matCarpet);
            CreatePrimitive("Memorial_Ceiling", PrimitiveType.Cube, zone44.transform,
                new Vector3(-26.25f, vestY + wingH, -13.0f), new Vector3(23.5f, 0.5f, 18.0f), matWallIvory);

            // Muros perimetrales de la Sala Conmemorativa
            CreateWall("Memorial_Wall_West", zone44.transform,
                new Vector3(-38.0f, vestY + 2.5f, -13.0f), new Vector3(0.5f, wingH, 18.0f), matWallIvory);
            CreateWall("Memorial_Wall_East", zone44.transform,
                new Vector3(-14.5f, vestY + 2.5f, -13.0f), new Vector3(0.5f, wingH, 18.0f), matWallIvory);
            CreateWall("Memorial_Wall_South", zone44.transform,
                new Vector3(-26.25f, vestY + 2.5f, -22.0f), new Vector3(23.5f, wingH, 0.5f), matWallIvory);

            // Muro Norte (Z = -4.0m) con puerta hacia el Vestíbulo del Ala Suroeste centrada en X = -22.2m (lejos del muro de la escalera)
            CreateWall("Memorial_Wall_North_W", zone44.transform,
                new Vector3(-30.625f, vestY + 2.5f, -4.0f), new Vector3(14.75f, wingH, 0.5f), matWallIvory);
            CreateWall("Memorial_Wall_North_E", zone44.transform,
                new Vector3(-17.825f, vestY + 2.5f, -4.0f), new Vector3(6.65f, wingH, 0.5f), matWallIvory);
            CreateArchitecturalDoor("Doors_Tower_To_MemorialRoom", zone44.transform,
                new Vector3(-22.2f, vestY, -4.0f), Quaternion.identity,
                1.8f, 3.2f, wingH, 0.5f, false, "",
                "SALA CONMEMORATIVA — MEMORIA PÚBLICA DE LA ACADEMIA (ZONA 44)", "", matMahogany, matGold, matWallIvory);

            // --- SUBÁREA 44A: NAVE CENTRAL DE BANCOS DE MADERA NOBLE Y CORONAS DE FLORES SECAS ---
            float[] pewZ = { -9.5f, -14.5f };
            float[] pewX = { -21.5f, -27.5f };
            for (int r = 0; r < pewZ.Length; r++)
            {
                for (int col = 0; col < pewX.Length; col++)
                {
                    GameObject pew = new GameObject($"Banco_Conmemorativo_Madera_{r + 1}_{col + 1}");
                    pew.transform.parent = zone44.transform;
                    pew.transform.position = new Vector3(pewX[col], vestY, pewZ[r]);

                    CreatePrimitive("Pew_Seat", PrimitiveType.Cube, pew.transform,
                        new Vector3(0f, 0.45f, 0f), new Vector3(3.8f, 0.08f, 0.65f), matMahogany);
                    CreatePrimitive("Pew_Backrest", PrimitiveType.Cube, pew.transform,
                        new Vector3(0f, 0.85f, 0.26f), new Vector3(3.8f, 0.72f, 0.08f), matMahogany);
                    CreatePrimitive("Pew_Side_L", PrimitiveType.Cube, pew.transform,
                        new Vector3(-1.82f, 0.52f, 0f), new Vector3(0.12f, 1.04f, 0.72f), matMahogany);
                    CreatePrimitive("Pew_Side_R", PrimitiveType.Cube, pew.transform,
                        new Vector3(1.82f, 0.52f, 0f), new Vector3(0.12f, 1.04f, 0.72f), matMahogany);
                }
            }

            // Pedestales con jarrones y coronas de flores secas alrededor de la sala
            Vector3[] dryFlowerPedestals =
            {
                new Vector3(-17.2f, vestY, -8.5f),
                new Vector3(-17.2f, vestY, -16.5f),
                new Vector3(-31.8f, vestY, -8.5f),
                new Vector3(-31.8f, vestY, -16.5f),
                new Vector3(-21.0f, vestY, -20.6f),
                new Vector3(-28.0f, vestY, -20.6f)
            };
            for (int df = 0; df < dryFlowerPedestals.Length; df++)
            {
                CreatePrimitive($"Pedestal_Flores_Secas_{df + 1}", PrimitiveType.Cylinder, zone44.transform,
                    dryFlowerPedestals[df] + new Vector3(0f, 0.55f, 0f), new Vector3(0.56f, 0.55f, 0.56f), matStoneWall);
                DecoPiece($"Jarron_Bronce_{df + 1}", PrimitiveType.Cylinder, zone44.transform,
                    dryFlowerPedestals[df] + new Vector3(0f, 1.28f, 0f), new Vector3(0.36f, 0.22f, 0.36f), matGold);
                DecoPiece($"Ramo_Flores_Secas_{df + 1}", PrimitiveType.Sphere, zone44.transform,
                    dryFlowerPedestals[df] + new Vector3(0f, 1.62f, 0f), new Vector3(0.78f, 0.58f, 0.78f), matDryFlowers);
            }

            // --- SUBÁREA 44B: GALERÍA DE FOTOGRAFÍAS DE ALUMNAS Y DISCURSO OFICIAL (OESTE, X: -38..-32) ---
            for (int fp = 0; fp < 4; fp++)
            {
                float fz = -6.8f - fp * 3.6f;
                CreateFramedArtPortrait($"Foto_Conmemorativa_Alumnas_{fp + 1}", zone44.transform,
                    new Vector3(-37.65f, vestY + 2.5f, fz), new Vector3(0.12f, 1.65f, 2.1f),
                    matGold, (fp % 2 == 0) ? matPhotoTroupe1923 : matPortraitElena,
                    $"Archivo Fotográfico de Honor — Promoción {1918 + fp * 2}");
            }

            CreateInteractiveWorldDoc("Doc_Libro_Homenaje_Sala_Conmemorativa", zone44.transform,
                new Vector3(-34.5f, vestY + 1.05f, -12.5f), new Vector3(0.44f, 0.06f, 0.34f), matGold,
                "doc_s10_memorial_public_tribute",
                "Libro de Actos Conmemorativos — Cómo la Academia Decidió Recordar a sus Alumnas",
                "MEMORIA PÚBLICA DE LA ACADEMIA VESPER (NOVIEMBRE DE 1924)\n\n" +
                "«Esta Sala Conmemorativa fue remodelada apresuradamente por orden de Isadora Vale una semana después de la tragedia.\n" +
                "En las fotografías oficiales y en los discursos impresos para la prensa, la dirección presentó la noche del 14 de octubre como un fatal accidente fortuito, elogiando el sacrificio artístico de sus solistas.\n" +
                "Sin embargo, varias exalumnas dejaron flores secas negras junto al banco del fondo y denunciaron que una de las placas recién instaladas fue arrancada a martillazos durante la noche.»");
            CreatePrimitive("Atril_Libro_Conmemorativo", PrimitiveType.Cube, zone44.transform,
                new Vector3(-34.5f, vestY + 0.52f, -12.5f), new Vector3(0.9f, 1.04f, 0.7f), matMahogany);

            // --- SUBÁREA 44C: MURO DE HONOR DE LAS PLACAS Y SECRETO DE LA PLACA RETIRADA (SUR, Z = -21.7m) ---
            // Gran revestimiento de caoba noble en el Muro de Honor Sur
            CreatePrimitive("Panel_Boiserie_Muro_Honor", PrimitiveType.Cube, zone44.transform,
                new Vector3(-24.5f, vestY + 2.4f, -21.68f), new Vector3(14.5f, 3.8f, 0.12f), matMahogany);

            // Filas de placas de latón con nombres de alumnas + EL HUECO VACÍO CENTRAL CON LA MARCA EN LA PARED
            for (int row = 0; row < 2; row++)
            {
                for (int col = -2; col <= 2; col++)
                {
                    float px = -24.5f + col * 2.2f;
                    float py = vestY + 1.95f + row * 0.95f;

                    if (row == 1 && col == 0)
                    {
                        // SECRETO DE LA SALA CONMEMORATIVA: La placa fue retirada y su marca oscura + 4 agujeros de tornillos permanece en la pared
                        GameObject wallScar = CreatePrimitive("Marca_Pared_Placa_Retirada_Elena", PrimitiveType.Cube, zone44.transform,
                            new Vector3(px, py, -21.58f), new Vector3(1.38f, 0.62f, 0.04f), matWallScar);
                        DecoPiece("Cerco_Oxido_Placa_Retirada", PrimitiveType.Cube, wallScar.transform,
                            new Vector3(0f, 0f, 0.35f), new Vector3(0.92f, 0.84f, 0.25f), matAgedBrick);

                        // La placa arrancada escondida en el suelo tras el banco inferior (se revela/destaca al inspeccionar la huella)
                        GameObject hiddenRemovedPlaque = CreatePrimitive("Placa_Laton_Arrancada_Oculta_Tras_Banco", PrimitiveType.Cube, zone44.transform,
                            new Vector3(px, vestY + 0.06f, -19.85f), new Vector3(1.32f, 0.04f, 0.58f), matGold);
                        hiddenRemovedPlaque.transform.localRotation = Quaternion.Euler(0f, 14f, 0f);

                        AcademyExpansionAnomalyController plaqueSecretCtrl = wallScar.AddComponent<AcademyExpansionAnomalyController>();
                        plaqueSecretCtrl.Configure(ExpansionNodeType.MemorialRemovedPlaqueSecret, wallScar.transform, null, hiddenRemovedPlaque);
                        EditorUtility.SetDirty(plaqueSecretCtrl);
                    }
                    else
                    {
                        DecoPiece($"Placa_Honor_Alumna_{row}_{col + 3}", PrimitiveType.Cube, zone44.transform,
                            new Vector3(px, py, -21.58f), new Vector3(1.32f, 0.56f, 0.05f), matGold);
                    }
                }
            }

            // Banco de madera justo frente al Muro de Honor donde está la marca de la placa retirada
            CreateWinterGardenIronBench("Banco_Frente_Muro_De_Placas", zone44.transform,
                new Vector3(-24.5f, vestY, -19.4f), Quaternion.Euler(0f, 180f, 0f), matIron, matMahogany);

            // Iluminación solemne y silenciosa de la Sala Conmemorativa
            CreatePictureSpotlight("Foco_Muro_De_Honor_Y_Placa_Retirada", zone44.transform,
                new Vector3(-24.5f, vestY + 4.4f, -17.5f), new Vector3(-24.5f, vestY + 2.4f, -21.6f),
                new Color(1.0f, 0.84f, 0.55f), 3.2f);
            CreateOrnateLantern("Lampara_Sala_Conmemorativa_N", zone44.transform,
                new Vector3(-24.5f, vestY + 4.3f, -8.5f), matGold, matLanternGlow, new Color(1.0f, 0.80f, 0.46f), 1.9f);
            CreateOrnateLantern("Lampara_Sala_Conmemorativa_S", zone44.transform,
                new Vector3(-24.5f, vestY + 4.3f, -16.0f), matGold, matLanternGlow, new Color(1.0f, 0.80f, 0.46f), 1.9f);

            // ====================================================================
            // SECCIÓN 11: GRAN ALA OCCIDENTAL DE RESIDENCIA Y TALLERES (ZONAS 45 A 48)
            // ZONA 48: ESTANCIA PRIVADA DE ISADORA VALE Y CELOSÍA DE VIGILANCIA
            // ZONA 47: COMEDOR ANTIGUO Y SALÓN DEL PATRONATO (+ GALERÍA DE ENLACE)
            // ZONA 45: DORMITORIO COMÚN DE INTERNAS
            // ZONA 46: TALLER DE ZAPATILLAS Y TINTORERÍA ESCÉNICA
            // ====================================================================

            // Basamento exterior de sillería bajo toda la Gran Ala Occidental (X: -62.5m a -27.5m, Z: 5.5m a 98.5m, Y: 0m a 2.4m)
            CreatePrimitive("WestWing_Foundation_Plinth_Main", PrimitiveType.Cube, rootEnv,
                new Vector3(-50.0f, 1.1f, 52.0f), new Vector3(24.5f, 2.2f, 92.5f), matStoneWall);
            CreatePrimitive("WestWing_Foundation_Plinth_EastGallery", PrimitiveType.Cube, rootEnv,
                new Vector3(-29.0f, 1.1f, 47.0f), new Vector3(18.0f, 2.2f, 6.5f), matStoneWall);
            CreatePrimitive("WestWing_Foundation_Plinth_WorkshopEast", PrimitiveType.Cube, rootEnv,
                new Vector3(-33.0f, 1.1f, 89.0f), new Vector3(10.5f, 2.2f, 18.5f), matStoneWall);

            // --------------------------------------------------------------------
            // ZONA 48: ESTANCIA PRIVADA DE ISADORA VALE Y CELOSÍA DE VIGILANCIA
            // Tramo Norte (Salón y Alcoba de la Directora): X: -62.0m a -38.0m, Z: 12.0m a 28.0m, Y = 2.4m
            // Tramo Sur (Galería Oculta de la Celosía tras Enfermería): X: -62.0m a -42.0m, Z: 6.0m a 12.0m, Y = 2.4m
            // --------------------------------------------------------------------
            GameObject zone48 = new GameObject("48_Isadora_Private_Suite_And_Spy_Lattice");
            zone48.transform.parent = rootEnv;

            // Suelos, alfombras y techos de la Zona 48
            CreatePrimitive("IsadoraSuite_Floor_Main", PrimitiveType.Cube, zone48.transform,
                new Vector3(-50.0f, vestY - 0.25f, 20.0f), new Vector3(24.0f, 0.5f, 16.0f), matParquet);
            CreatePrimitive("IsadoraSuite_Ceiling_Main", PrimitiveType.Cube, zone48.transform,
                new Vector3(-50.0f, vestY + wingH, 20.0f), new Vector3(24.0f, 0.5f, 16.0f), matWallIvory);
            CreatePrimitive("IsadoraSuite_Carpet_Salon", PrimitiveType.Cube, zone48.transform,
                new Vector3(-44.5f, vestY + 0.02f, 20.0f), new Vector3(9.5f, 0.04f, 12.0f), matCarpet);
            CreatePrimitive("IsadoraSuite_Carpet_Bedroom", PrimitiveType.Cube, zone48.transform,
                new Vector3(-56.0f, vestY + 0.02f, 20.5f), new Vector3(8.5f, 0.04f, 11.0f), matCarpet);

            CreatePrimitive("IsadoraSpyGallery_Floor_South", PrimitiveType.Cube, zone48.transform,
                new Vector3(-52.0f, vestY - 0.25f, 9.0f), new Vector3(20.0f, 0.5f, 6.0f), matParquet);
            CreatePrimitive("IsadoraSpyGallery_Ceiling_South", PrimitiveType.Cube, zone48.transform,
                new Vector3(-52.0f, vestY + wingH, 9.0f), new Vector3(20.0f, 0.5f, 6.0f), matWallIvory);
            CreatePrimitive("IsadoraSpyGallery_Runner", PrimitiveType.Cube, zone48.transform,
                new Vector3(-50.0f, vestY + 0.02f, 8.8f), new Vector3(14.5f, 0.04f, 2.2f), matCarpet);

            // Muros perimetrales de la Zona 48
            // Muro Oeste continuo (X = -62.0m, Z: 6.0m a 28.0m)
            CreateWall("IsadoraSuite_Wall_West", zone48.transform,
                new Vector3(-62.0f, vestY + 2.5f, 17.0f), new Vector3(0.5f, wingH, 22.0f), matWallIvory);
            // Muro Sur de la Galería de Vigilancia (Z = 6.0m, X: -62.0m a -42.0m)
            CreateWall("IsadoraSpyGallery_Wall_South", zone48.transform,
                new Vector3(-52.0f, vestY + 2.5f, 6.0f), new Vector3(20.0f, wingH, 0.5f), matWallIvory);
            // Cierre del escalón Este-Oeste en Z = 12.0m (X: -42.0m a -38.0m, ya cubierto por Infirmary_Wall_North, añadimos moldura interior)
            // Muro Este entre Z = 26.0m y Z = 28.0m (X = -38.0m, colindante con el extremo sur del Archivo de Expedientes)
            CreateWall("IsadoraSuite_Wall_East_NorthStub", zone48.transform,
                new Vector3(-38.0f, vestY + 2.5f, 27.0f), new Vector3(0.5f, wingH, 2.0f), matWallIvory);

            // Muro Norte compartido con 47. Comedor Antiguo y Salón del Patronato (Z = 28.0m, X: -62.0m a -38.0m) con puerta doble en X = -50.0m
            CreateWall("IsadoraSuite_Wall_North_W", zone48.transform,
                new Vector3(-56.75f, vestY + 2.5f, 28.0f), new Vector3(10.5f, wingH, 0.5f), matWallIvory);
            CreateWall("IsadoraSuite_Wall_North_E", zone48.transform,
                new Vector3(-43.25f, vestY + 2.5f, 28.0f), new Vector3(10.5f, wingH, 0.5f), matWallIvory);
            CreateArchitecturalDoubleDoor("Doors_IsadoraSuite_To_DiningPatronage", zone48.transform,
                new Vector3(-50.0f, vestY, 28.0f), Quaternion.identity,
                3.0f, 3.6f, wingH, 0.5f, false, "",
                "COMEDOR ANTIGUO Y SALÓN DEL PATRONATO (ZONA 47)", "", matMahogany, matGold, matWallIvory);

            // Tabique interior N-S en X = -50.0m (Z: 12.0m a 28.0m) que divide el Gabinete de Trofeos y Gramófono (Este) de la Alcoba con Dosel (Oeste), con paso en Z = 20.0m
            CreateWall("IsadoraSuite_InteriorWall_S", zone48.transform,
                new Vector3(-50.0f, vestY + 2.5f, 15.25f), new Vector3(0.4f, wingH, 6.5f), matWallIvory);
            CreateWall("IsadoraSuite_InteriorWall_N", zone48.transform,
                new Vector3(-50.0f, vestY + 2.5f, 24.75f), new Vector3(0.4f, wingH, 6.5f), matWallIvory);
            CreateWall("IsadoraSuite_InteriorWall_Lintel", zone48.transform,
                new Vector3(-50.0f, vestY + 4.2f, 20.0f), new Vector3(0.4f, 1.6f, 3.0f), matWallIvory);

            // Tabique de la Galería Oculta de Vigilancia en Z = 12.0m (X: -62.0m a -42.0m) con arco de paso en X = -46.0m
            CreateWall("IsadoraSpyGallery_Partition_W", zone48.transform,
                new Vector3(-55.0f, vestY + 2.5f, 12.0f), new Vector3(14.0f, wingH, 0.4f), matWallIvory);
            CreateWall("IsadoraSpyGallery_Partition_E", zone48.transform,
                new Vector3(-43.25f, vestY + 2.5f, 12.0f), new Vector3(2.5f, wingH, 0.4f), matWallIvory);

            // Sub-área 48A: Gabinete de Trofeos, Retratos y Gramófono (X: -50.0m a -38.0m, Z: 12.0m a 28.0m)
            CreateFramedArtPortrait("Retrato_Isadora_Juventud_Imperial_1898", zone48.transform,
                new Vector3(-49.7f, vestY + 2.6f, 24.5f), new Vector3(0.12f, 2.2f, 1.6f), matGold, matPhotoTroupe1923,
                "Retrato Oficial: «Isadora Vale — Primera Bailarina del Teatro Imperial (1898)»");
            CreatePrimitive("Escritorio_Privado_Nocturno_Isadora", PrimitiveType.Cube, zone48.transform,
                new Vector3(-44.0f, vestY + 0.52f, 24.2f), new Vector3(2.6f, 1.04f, 1.4f), matMahogany);
            DecoPiece("Tapete_Cuero_Escritorio_Isadora", PrimitiveType.Cube, zone48.transform,
                new Vector3(-44.0f, vestY + 1.05f, 24.2f), new Vector3(1.6f, 0.02f, 0.9f), matLeather);
            CreatePrimitive("Sillon_Privado_Isadora", PrimitiveType.Cube, zone48.transform,
                new Vector3(-44.0f, vestY + 0.55f, 25.8f), new Vector3(0.95f, 1.1f, 0.95f), matLeather);
            CreatePrimitive("Divan_Terciopelo_Isadora_Base", PrimitiveType.Cube, zone48.transform,
                new Vector3(-41.5f, vestY + 0.36f, 19.5f), new Vector3(1.1f, 0.52f, 2.8f), matVelvetDrapes);
            CreatePrimitive("Divan_Terciopelo_Isadora_Respaldo", PrimitiveType.Cube, zone48.transform,
                new Vector3(-41.0f, vestY + 0.72f, 19.5f), new Vector3(0.22f, 0.85f, 2.8f), matMahogany);

            // Vitrinas de coronas de plata y programas antiguos de ópera
            for (int tc = 0; tc < 2; tc++)
            {
                float tcZ = 16.5f + tc * 6.5f;
                CreatePrimitive($"Vitrina_Trofeos_Isadora_{tc + 1}", PrimitiveType.Cube, zone48.transform,
                    new Vector3(-49.2f, vestY + 1.15f, tcZ), new Vector3(0.75f, 2.3f, 2.2f), matMahogany);
                DecoPiece($"Cristal_Vitrina_Trofeos_{tc + 1}", PrimitiveType.Cube, zone48.transform,
                    new Vector3(-48.80f, vestY + 1.35f, tcZ), new Vector3(0.04f, 1.5f, 1.9f), matMirrorGlass);
                DecoPiece($"Corona_Laurel_Plata_{tc + 1}", PrimitiveType.Cylinder, zone48.transform,
                    new Vector3(-49.15f, vestY + 1.40f, tcZ), new Vector3(0.38f, 0.08f, 0.38f), matGold);
            }

            // Sub-área 48B: Alcoba con Dosel de la Directora (X: -62.0m a -50.0m, Z: 12.0m a 28.0m)
            // Cama señorial con cuatro columnas de caoba y cortinajes borgoña
            CreatePrimitive("Cama_Dosel_Isadora_Base", PrimitiveType.Cube, zone48.transform,
                new Vector3(-58.5f, vestY + 0.38f, 21.5f), new Vector3(3.2f, 0.55f, 2.4f), matMahogany);
            CreatePrimitive("Cama_Dosel_Isadora_Colchon", PrimitiveType.Cube, zone48.transform,
                new Vector3(-58.4f, vestY + 0.72f, 21.5f), new Vector3(2.9f, 0.28f, 2.2f), matVelvetDrapes);
            CreatePrimitive("Cama_Dosel_Isadora_Cabecero", PrimitiveType.Cube, zone48.transform,
                new Vector3(-60.0f, vestY + 1.65f, 21.5f), new Vector3(0.25f, 2.6f, 2.5f), matMahogany);
            CreatePrimitive("Cama_Dosel_Isadora_Techo", PrimitiveType.Cube, zone48.transform,
                new Vector3(-58.5f, vestY + 2.95f, 21.5f), new Vector3(3.3f, 0.18f, 2.5f), matMahogany);
            float[] postOffsetsX = { -60.0f, -57.0f };
            float[] postOffsetsZ = { 20.35f, 22.65f };
            for (int px = 0; px < 2; px++)
            {
                for (int pz = 0; pz < 2; pz++)
                {
                    DecoPiece($"Poste_Dosel_{px}_{pz}", PrimitiveType.Cylinder, zone48.transform,
                        new Vector3(postOffsetsX[px], vestY + 1.55f, postOffsetsZ[pz]), new Vector3(0.14f, 1.45f, 0.14f), matMahogany);
                }
            }
            // Tocador personal y armario ropero de la directora
            CreateVanityStation("Tocador_Privado_Isadora", zone48.transform,
                new Vector3(-56.0f, vestY, 26.8f), Quaternion.Euler(0f, 180f, 0f),
                matMahogany, matGold, matMirrorGlass, matVelvetDrapes, matLanternGlow, matPorcelain, matShadowFigure, true, false);
            CreatePrimitive("Armario_Ropero_Isadora", PrimitiveType.Cube, zone48.transform,
                new Vector3(-56.0f, vestY + 1.35f, 13.2f), new Vector3(2.8f, 2.7f, 1.1f), matMahogany);

            // Sub-área 48C: Galería Oculta de la Celosía de Vigilancia (X: -62.0m a -42.0m, Z: 6.0m a 12.0m)
            // Mecanismo de rejilla de bronce, mirilla óptica hacia la Enfermería/Dirección y pupitre de anotación nocturna
            CreatePrimitive("Pupitre_Espionaje_Isadora", PrimitiveType.Cube, zone48.transform,
                new Vector3(-47.5f, vestY + 0.52f, 7.2f), new Vector3(2.2f, 1.04f, 0.95f), matMahogany);
            CreatePrimitive("Rejilla_Celosia_Vigilancia_Bronce", PrimitiveType.Cube, zone48.transform,
                new Vector3(-42.22f, vestY + 1.65f, 10.6f), new Vector3(0.12f, 0.85f, 1.4f), matGold);
            DecoPiece("Tubo_Acustico_Interfono_Vigilancia", PrimitiveType.Cylinder, zone48.transform,
                new Vector3(-42.35f, vestY + 1.45f, 11.4f), new Vector3(0.10f, 0.95f, 0.10f), matGold);

            // SECRETO DE ZONA 48: Bitácora de Vigilancia Nocturna de Isadora Vale sobre el pupitre de la celosía
            GameObject spyLatticeTrigger = CreatePrimitive("Bitacora_Celosia_Vigilancia_Isadora_Secreto", PrimitiveType.Cube, zone48.transform,
                new Vector3(-47.5f, vestY + 1.08f, 7.2f), new Vector3(0.55f, 0.08f, 0.42f), matLeather);
            DecoPiece("Lente_Mirilla_Laton_Sobre_Bitacora", PrimitiveType.Cylinder, zone48.transform,
                new Vector3(-47.15f, vestY + 1.12f, 7.25f), new Vector3(0.14f, 0.06f, 0.14f), matGold);
            AcademyExpansionAnomalyController spyCtrl = spyLatticeTrigger.AddComponent<AcademyExpansionAnomalyController>();
            spyCtrl.Configure(ExpansionNodeType.IsadoraSpyLatticeSecret);
            EditorUtility.SetDirty(spyCtrl);

            CreateOrnateLantern("Lampara_Salon_Isadora", zone48.transform,
                new Vector3(-44.5f, vestY + 4.3f, 20.0f), matGold, matLanternGlow, new Color(1.0f, 0.78f, 0.45f), 2.1f);
            CreateOrnateLantern("Lampara_Alcoba_Isadora", zone48.transform,
                new Vector3(-56.0f, vestY + 4.3f, 20.5f), matGold, matLanternGlow, new Color(1.0f, 0.72f, 0.42f), 1.8f);
            CreatePictureSpotlight("Foco_Celosia_Vigilancia_Z48", zone48.transform,
                new Vector3(-47.5f, vestY + 3.8f, 8.8f), new Vector3(-47.5f, vestY + 1.08f, 7.2f),
                new Color(1.0f, 0.82f, 0.50f), 2.8f);

            // --------------------------------------------------------------------
            // ZONA 47: COMEDOR ANTIGUO Y SALÓN DEL PATRONATO (+ GALERÍA DE ENLACE)
            // Cuerpo Principal: X: -62.0m a -38.0m, Z: 28.0m a 54.0m, Y = 2.4m
            // Galería de Enlace con 10. Pasillo de los Retratos: X: -38.0m a -20.0m, Z: 44.0m a 50.0m, Y = 2.4m
            // --------------------------------------------------------------------
            GameObject zone47 = new GameObject("47_Old_Dining_Hall_And_Patronage_Salon");
            zone47.transform.parent = rootEnv;

            // Galería de Enlace al Patronato (X: -38.0m a -20.0m, Z: 44.0m a 50.0m, encajada entre 8. Archivo [Z <= 44] y 16/17. Aulas [Z >= 50])
            CreatePrimitive("PatronageGallery_Floor", PrimitiveType.Cube, zone47.transform,
                new Vector3(-29.0f, vestY - 0.25f, 47.0f), new Vector3(18.0f, 0.5f, 6.0f), matParquet);
            CreatePrimitive("PatronageGallery_Ceiling", PrimitiveType.Cube, zone47.transform,
                new Vector3(-29.0f, vestY + wingH, 47.0f), new Vector3(18.0f, 0.5f, 6.0f), matWallIvory);
            CreatePrimitive("PatronageGallery_Carpet", PrimitiveType.Cube, zone47.transform,
                new Vector3(-29.0f, vestY + 0.02f, 47.0f), new Vector3(17.0f, 0.04f, 2.8f), matCarpet);
            CreateWall("PatronageGallery_Wall_South", zone47.transform,
                new Vector3(-29.0f, vestY + 2.5f, 44.0f), new Vector3(18.0f, wingH, 0.4f), matWallIvory);
            CreateWall("PatronageGallery_Wall_North", zone47.transform,
                new Vector3(-29.0f, vestY + 2.5f, 50.0f), new Vector3(18.0f, wingH, 0.4f), matWallIvory);

            // Placas de mecenas y benefactores en la Galería del Patronato
            for (int pg = 0; pg < 3; pg++)
            {
                float pgX = -34.5f + pg * 5.5f;
                CreateOrnateSconce($"Sconce_Patronato_Gal_{pg + 1}", zone47.transform,
                    new Vector3(pgX, vestY + 3.0f, 49.7f), matGold, matLanternGlow, new Color(1.0f, 0.82f, 0.50f), 1.9f);
                CreateFramedArtPortrait($"Cuadro_Benefactor_Patronato_{pg + 1}", zone47.transform,
                    new Vector3(pgX, vestY + 2.5f, 44.3f), new Vector3(1.3f, 1.7f, 0.12f), matGold, matPhotoTroupe1923,
                    $"Consejo de Mecenas del Patronato — Temporada {1918 + pg * 3}");
            }

            // Suelo, techo y alfombras del Cuerpo Principal del Comedor y Salón del Patronato (X: -62.0m a -38.0m, Z: 28.0m a 54.0m)
            CreatePrimitive("DiningHall_Floor", PrimitiveType.Cube, zone47.transform,
                new Vector3(-50.0f, vestY - 0.25f, 41.0f), new Vector3(24.0f, 0.5f, 26.0f), matParquet);
            CreatePrimitive("DiningHall_Ceiling", PrimitiveType.Cube, zone47.transform,
                new Vector3(-50.0f, vestY + wingH, 41.0f), new Vector3(24.0f, 0.5f, 26.0f), matWallIvory);
            CreatePrimitive("PatronageSalon_Rug", PrimitiveType.Cube, zone47.transform,
                new Vector3(-44.0f, vestY + 0.02f, 40.5f), new Vector3(9.5f, 0.04f, 18.0f), matCarpet);

            // Muros perimetrales de la Zona 47
            CreateWall("DiningHall_Wall_West", zone47.transform,
                new Vector3(-62.0f, vestY + 2.5f, 41.0f), new Vector3(0.5f, wingH, 26.0f), matWallIvory);
            // Muro Este (X = -38.0m, Z: 28.0m a 54.0m) con puerta en Z = 47.0m hacia la Galería del Patronato
            CreateWall("DiningHall_Wall_East_S", zone47.transform,
                new Vector3(-38.0f, vestY + 2.5f, 37.025f), new Vector3(0.5f, wingH, 17.95f), matWallIvory);
            CreateWall("DiningHall_Wall_East_N", zone47.transform,
                new Vector3(-38.0f, vestY + 2.5f, 50.975f), new Vector3(0.5f, wingH, 6.05f), matWallIvory);
            CreateArchitecturalDoor("Door_PatronageGallery_To_DiningHall", zone47.transform,
                new Vector3(-38.0f, vestY, 47.0f), Quaternion.Euler(0f, 90f, 0f),
                1.8f, 3.2f, wingH, 0.5f, false, "",
                "COMEDOR ANTIGUO Y SALÓN DEL PATRONATO (ZONA 47)", "", matMahogany, matGold, matWallIvory);

            // Muro Norte compartido con 45. Dormitorio Común de Internas (Z = 54.0m, X: -62.0m a -38.0m) con puerta doble en X = -50.0m
            CreateWall("DiningHall_Wall_North_W", zone47.transform,
                new Vector3(-56.75f, vestY + 2.5f, 54.0f), new Vector3(10.5f, wingH, 0.5f), matWallIvory);
            CreateWall("DiningHall_Wall_North_E", zone47.transform,
                new Vector3(-43.25f, vestY + 2.5f, 54.0f), new Vector3(10.5f, wingH, 0.5f), matWallIvory);
            CreateArchitecturalDoubleDoor("Doors_DiningHall_To_Dormitory", zone47.transform,
                new Vector3(-50.0f, vestY, 54.0f), Quaternion.identity,
                3.0f, 3.6f, wingH, 0.5f, false, "",
                "DORMITORIO COMÚN DE INTERNAS (ZONA 45)", "", matMahogany, matGold, matWallIvory);

            // Columnata interior (X = -49.0m) que separa el Refectorio de Alumnas (Oeste) del Salón Reservado del Patronato (Este)
            for (int dc = 0; dc < 3; dc++)
            {
                float colZ = 33.5f + dc * 7.5f;
                CreateClassicalColumn($"Columna_Comedor_Patronato_{dc + 1}", zone47.transform,
                    new Vector3(-49.0f, vestY, colZ), wingH, 0.65f, matFacade);
            }

            // Sub-área 47A: Gran Refectorio de Internas (X: -62.0m a -49.5m, Z: 34.0m a 52.0m)
            // Dos largas mesas de roble con bancos corridos y pizarrón del régimen de pesaje
            for (int rt = 0; rt < 2; rt++)
            {
                float tableX = -58.2f + rt * 4.8f;
                CreatePrimitive($"Mesa_Refectorio_Alumnas_{rt + 1}", PrimitiveType.Cube, zone47.transform,
                    new Vector3(tableX, vestY + 0.42f, 43.5f), new Vector3(1.5f, 0.84f, 11.5f), matWoodArchive);
                CreatePrimitive($"Banco_Refectorio_W_{rt + 1}", PrimitiveType.Cube, zone47.transform,
                    new Vector3(tableX - 1.15f, vestY + 0.24f, 43.5f), new Vector3(0.45f, 0.48f, 11.0f), matWoodArchive);
                CreatePrimitive($"Banco_Refectorio_E_{rt + 1}", PrimitiveType.Cube, zone47.transform,
                    new Vector3(tableX + 1.15f, vestY + 0.24f, 43.5f), new Vector3(0.45f, 0.48f, 11.0f), matWoodArchive);

                // Platos de porcelana y jarras de agua alineados con precisión sobre las mesas
                for (int pl = 0; pl < 5; pl++)
                {
                    float plateZ = 39.0f + pl * 2.2f;
                    DecoPiece($"Plato_Refectorio_{rt}_{pl}", PrimitiveType.Cylinder, zone47.transform,
                        new Vector3(tableX - 0.38f, vestY + 0.86f, plateZ), new Vector3(0.28f, 0.02f, 0.28f), matPorcelain);
                    DecoPiece($"Plato_Refectorio_E_{rt}_{pl}", PrimitiveType.Cylinder, zone47.transform,
                        new Vector3(tableX + 0.38f, vestY + 0.86f, plateZ), new Vector3(0.28f, 0.02f, 0.28f), matPorcelain);
                }
            }

            CreateInteractiveWorldDoc("Doc_Pizarron_Regimen_Y_Pesaje_Refectorio", zone47.transform,
                new Vector3(-61.55f, vestY + 1.65f, 43.5f), new Vector3(0.08f, 0.85f, 1.25f), matGold,
                "doc_s11_refectory_weighing_regime",
                "Tablón del Refectorio — Régimen Estricto y Control de Peso (Octubre 1924)",
                "ORDEN DE LA DIRECCIÓN — RÉGIMEN DE INTERNAS PREVIO AL ESTRENO:\n\n" +
                "«Queda terminantemente prohibido servir pan, lácteos o caldo con grasa a las integrantes del cuerpo de baile durante las dos semanas previas a 'El Lago de las Sombras'.\n\n" +
                "Toda alumna cuyo peso exceda en más de trescientos gramos el baremo fijado por Madame Vale será trasladada a vigilancia médica en la Enfermería Antigua y perderá su puesto en la fila principal.»");

            // Sub-área 47B: Oficio de Servicio y Montacargas al Subsuelo (Esquina Suroeste, X: -62.0m a -53.0m, Z: 28.5m a 33.5m)
            CreatePrimitive("Mostrador_Oficio_Montacargas", PrimitiveType.Cube, zone47.transform,
                new Vector3(-58.0f, vestY + 0.55f, 31.0f), new Vector3(5.5f, 1.1f, 1.1f), matMahogany);
            CreatePrimitive("Torre_Montacargas_Servicio_Subsuelo", PrimitiveType.Cube, zone47.transform,
                new Vector3(-60.8f, vestY + 1.85f, 29.3f), new Vector3(1.6f, 3.7f, 1.4f), matIron);
            DecoPiece("Bandeja_Plata_Cubierta_Montacargas", PrimitiveType.Cylinder, zone47.transform,
                new Vector3(-57.5f, vestY + 1.15f, 31.0f), new Vector3(0.55f, 0.16f, 0.55f), matGold);

            // Sub-área 47C: Salón de Té y Mesa Presidencial del Patronato (Mitad Este, X: -48.0m a -38.0m, Z: 29.0m a 52.0m)
            CreatePrimitive("Mesa_Presidencial_Patronato", PrimitiveType.Cube, zone47.transform,
                new Vector3(-43.5f, vestY + 0.44f, 39.5f), new Vector3(2.4f, 0.88f, 9.5f), matMahogany);
            DecoPiece("Mantel_Bordado_Patronato", PrimitiveType.Cube, zone47.transform,
                new Vector3(-43.5f, vestY + 0.89f, 39.5f), new Vector3(2.1f, 0.02f, 9.1f), matVelvetDrapes);
            // Candelabros de bronce y copas de cristal volcadas de la medianoche del 14 de octubre
            for (int cnd = 0; cnd < 3; cnd++)
            {
                float cndZ = 36.5f + cnd * 3.0f;
                DecoPiece($"Candelabro_Patronato_{cnd + 1}", PrimitiveType.Cylinder, zone47.transform,
                    new Vector3(-43.5f, vestY + 1.15f, cndZ), new Vector3(0.22f, 0.26f, 0.22f), matGold);
            }

            // SECRETO DE ZONA 47: Acta Reservada de Medianoche del Patronato y Registro del Montacargas en la cabecera de la Mesa Presidencial
            GameObject patronageSecretTrigger = CreatePrimitive("Acta_Secreta_Patronato_Y_Montacargas_1924", PrimitiveType.Cube, zone47.transform,
                new Vector3(-43.5f, vestY + 0.94f, 35.4f), new Vector3(0.62f, 0.08f, 0.46f), matLeather);
            DecoPiece("Copa_Cristal_Volcada_Patronato", PrimitiveType.Cylinder, zone47.transform,
                new Vector3(-43.0f, vestY + 0.95f, 35.6f), new Vector3(0.10f, 0.14f, 0.10f), matMirrorGlass).transform.localRotation = Quaternion.Euler(0f, 25f, 82f);
            AcademyExpansionAnomalyController patronageCtrl = patronageSecretTrigger.AddComponent<AcademyExpansionAnomalyController>();
            patronageCtrl.Configure(ExpansionNodeType.PatronageDinnerSecretMinutes);
            EditorUtility.SetDirty(patronageCtrl);

            CreateOrnateLantern("Lampara_Comedor_Refectorio", zone47.transform,
                new Vector3(-55.5f, vestY + 4.3f, 42.0f), matGold, matLanternGlow, new Color(1.0f, 0.80f, 0.48f), 2.2f);
            CreateOrnateLantern("Lampara_Salon_Patronato", zone47.transform,
                new Vector3(-43.5f, vestY + 4.3f, 40.0f), matGold, matLanternGlow, new Color(1.0f, 0.78f, 0.45f), 2.4f);
            CreatePictureSpotlight("Foco_Mesa_Presidencial_Patronato", zone47.transform,
                new Vector3(-43.5f, vestY + 3.9f, 37.0f), new Vector3(-43.5f, vestY + 0.94f, 35.4f),
                new Color(1.0f, 0.84f, 0.52f), 2.9f);

            // --------------------------------------------------------------------
            // ZONA 45: DORMITORIO COMÚN DE INTERNAS
            // X: -62.0m a -38.0m, Z: 54.0m a 78.0m, Y = 2.4m
            // --------------------------------------------------------------------
            GameObject zone45 = new GameObject("45_Boarding_Students_Dormitory");
            zone45.transform.parent = rootEnv;

            CreatePrimitive("Dormitory_Floor", PrimitiveType.Cube, zone45.transform,
                new Vector3(-50.0f, vestY - 0.25f, 66.0f), new Vector3(24.0f, 0.5f, 24.0f), matParquet);
            CreatePrimitive("Dormitory_Ceiling", PrimitiveType.Cube, zone45.transform,
                new Vector3(-50.0f, vestY + wingH, 66.0f), new Vector3(24.0f, 0.5f, 24.0f), matWallIvory);
            CreatePrimitive("Dormitory_CenterRunner", PrimitiveType.Cube, zone45.transform,
                new Vector3(-50.0f, vestY + 0.02f, 66.0f), new Vector3(3.6f, 0.04f, 22.0f), matCarpet);

            // Muros perimetrales del Dormitorio Común
            CreateWall("Dormitory_Wall_West", zone45.transform,
                new Vector3(-62.0f, vestY + 2.5f, 66.0f), new Vector3(0.5f, wingH, 24.0f), matWallIvory);
            CreateWall("Dormitory_Wall_East", zone45.transform,
                new Vector3(-38.0f, vestY + 2.5f, 66.0f), new Vector3(0.5f, wingH, 24.0f), matWallIvory);

            // Muro Norte compartido con 46. Taller de Zapatillas y Tintorería Escénica (Z = 78.0m, X: -62.0m a -38.0m) con puerta doble en X = -50.0m
            CreateWall("Dormitory_Wall_North_W", zone45.transform,
                new Vector3(-56.75f, vestY + 2.5f, 78.0f), new Vector3(10.5f, wingH, 0.5f), matWallIvory);
            CreateWall("Dormitory_Wall_North_E", zone45.transform,
                new Vector3(-43.25f, vestY + 2.5f, 78.0f), new Vector3(10.5f, wingH, 0.5f), matWallIvory);
            CreateArchitecturalDoubleDoor("Doors_Dormitory_To_Workshop", zone45.transform,
                new Vector3(-50.0f, vestY, 78.0f), Quaternion.identity,
                3.0f, 3.6f, wingH, 0.5f, false, "",
                "TALLER DE ZAPATILLAS Y TINTORERÍA ESCÉNICA (ZONA 46)", "", matMahogany, matGold, matWallIvory);

            // Sub-área 45A: Nave de Camas y Baúles de Internas (2 filas de 5 camas de hierro y madera a cada lado del pasillo central)
            GameObject maraTrunkLid = null;
            for (int row = 0; row < 2; row++)
            {
                float bedX = (row == 0) ? -58.2f : -41.8f;
                float trunkX = (row == 0) ? -55.4f : -44.6f;
                float headboardX = (row == 0) ? -60.0f : -40.0f;

                for (int b = 0; b < 5; b++)
                {
                    float bedZ = 57.5f + b * 4.2f;
                    int bedNumber = (row * 5) + b + 1;

                    // Armazón y colchón de la cama de interna
                    CreatePrimitive($"Cama_Interna_{bedNumber}_Base", PrimitiveType.Cube, zone45.transform,
                        new Vector3(bedX, vestY + 0.32f, bedZ), new Vector3(2.8f, 0.42f, 1.35f), matIron);
                    CreatePrimitive($"Cama_Interna_{bedNumber}_Colchon", PrimitiveType.Cube, zone45.transform,
                        new Vector3(bedX, vestY + 0.56f, bedZ), new Vector3(2.65f, 0.22f, 1.25f), matClothCover);
                    CreatePrimitive($"Cama_Interna_{bedNumber}_Cabecero", PrimitiveType.Cube, zone45.transform,
                        new Vector3(headboardX, vestY + 0.78f, bedZ), new Vector3(0.12f, 1.15f, 1.40f), matIron);

                    // Mesilla de noche junto a la cama
                    CreatePrimitive($"Mesilla_Interna_{bedNumber}", PrimitiveType.Cube, zone45.transform,
                        new Vector3(headboardX + (row == 0 ? 0.55f : -0.55f), vestY + 0.38f, bedZ + 1.15f), new Vector3(0.65f, 0.76f, 0.55f), matMahogany);

                    // Baúl personal de madera y latón al pie de cada cama
                    CreatePrimitive($"Baul_Interna_{bedNumber}_Cuerpo", PrimitiveType.Cube, zone45.transform,
                        new Vector3(trunkX, vestY + 0.26f, bedZ), new Vector3(0.75f, 0.52f, 1.15f), matWoodArchive);
                    GameObject lid = CreatePrimitive($"Baul_Interna_{bedNumber}_Tapa", PrimitiveType.Cube, zone45.transform,
                        new Vector3(trunkX, vestY + 0.55f, bedZ), new Vector3(0.78f, 0.08f, 1.18f), matMahogany);

                    // Cama N.º 7 (row = 1, b = 1 -> Z = 61.7m): Cama de Mara Bell con su caja de música de bailarina y baúl con cintas ensangrentadas
                    if (bedNumber == 7)
                    {
                        maraTrunkLid = lid;
                        GameObject musicBoxObj = CreatePrimitive("Caja_De_Musica_Mara_Bell_Anomalia", PrimitiveType.Cube, zone45.transform,
                            new Vector3(headboardX - 0.55f, vestY + 0.82f, bedZ + 1.15f), new Vector3(0.32f, 0.14f, 0.26f), matGold);
                        GameObject ballerinaFig = DecoPiece("Figurilla_Bailarina_Porcelana_Mara", PrimitiveType.Cylinder, zone45.transform,
                            new Vector3(headboardX - 0.55f, vestY + 0.96f, bedZ + 1.15f), new Vector3(0.08f, 0.12f, 0.08f), matPorcelain);

                        AcademyExpansionAnomalyController maraMusicCtrl = musicBoxObj.AddComponent<AcademyExpansionAnomalyController>();
                        maraMusicCtrl.Configure(
                            ExpansionNodeType.DormitoryMusicBoxAndTrunkAnomaly,
                            ballerinaFig.transform,
                            null,
                            maraTrunkLid);
                        EditorUtility.SetDirty(maraMusicCtrl);
                    }
                }
            }

            // Sub-área 45B: Rincón de Estudio Nocturno y Estufa de Carbón (Centro-Sur del Dormitorio, X: -52.5m a -47.5m, Z: 55.5m a 60.5m)
            CreatePrimitive("Estufa_Hierro_Fundido_Dormitorio", PrimitiveType.Cylinder, zone45.transform,
                new Vector3(-50.0f, vestY + 0.85f, 58.0f), new Vector3(0.85f, 0.85f, 0.85f), matIron);
            DecoPiece("Tubo_Humo_Estufa_Dormitorio", PrimitiveType.Cylinder, zone45.transform,
                new Vector3(-50.0f, vestY + 3.1f, 58.0f), new Vector3(0.22f, 1.85f, 0.22f), matIron);
            CreatePrimitive("Mesa_Lectura_Internas", PrimitiveType.Cube, zone45.transform,
                new Vector3(-50.0f, vestY + 0.42f, 64.5f), new Vector3(1.8f, 0.84f, 3.2f), matMahogany);

            CreateInteractiveWorldDoc("Doc_Reglamento_Silencio_Nocturno_Internado", zone45.transform,
                new Vector3(-50.0f, vestY + 0.86f, 64.5f), new Vector3(0.42f, 0.04f, 0.32f), matGold,
                "doc_s11_dormitory_curfew_rules",
                "Reglamento del Internado — Toque de Queda y Clausura Nocturna (1924)",
                "REGLAMENTO DE CONVIVENCIA DEL INTERNADO:\n\n" +
                "«1. A las 21:30 horas se apagarán todas las lámparas del Dormitorio Común y se cerrarán con llave las puertas hacia el Comedor y los Talleres.\n" +
                "2. Ninguna interna podrá abandonar su cama bajo pretexto de escuchar música o pasos procedentes del Teatro o de la Enfermería.\n" +
                "3. Toda conversación sobre alumnas dadas de baja o trasladadas será sancionada con reclusión inmediata.»");

            // Sub-área 45C y SECRETO DE ZONA 45: Nicho del Baúl Oculto tras el Tabique de Ropa Blanca (Esquina Noroeste, X: -61.2m a -56.5m, Z: 74.5m a 77.5m)
            CreatePrimitive("Estanteria_Ropa_Blanca_Dormitorio", PrimitiveType.Cube, zone45.transform,
                new Vector3(-57.8f, vestY + 1.25f, 75.2f), new Vector3(3.2f, 2.5f, 0.65f), matWoodArchive);
            CreatePrimitive("Tabima_Madera_Falso_Fondo_Dormitorio", PrimitiveType.Cube, zone45.transform,
                new Vector3(-60.4f, vestY + 0.14f, 76.4f), new Vector3(2.2f, 0.28f, 1.8f), matWoodArchive);

            GameObject collectiveDiaryTrigger = CreatePrimitive("Baul_Oculto_Diario_Colectivo_Internas_Secreto", PrimitiveType.Cube, zone45.transform,
                new Vector3(-60.4f, vestY + 0.48f, 76.4f), new Vector3(0.82f, 0.42f, 0.58f), matLeather);
            DecoPiece("Candado_Laton_Forzado_Diario_Colectivo", PrimitiveType.Cube, zone45.transform,
                new Vector3(-60.0f, vestY + 0.52f, 76.4f), new Vector3(0.12f, 0.16f, 0.14f), matGold);
            AcademyExpansionAnomalyController diaryCtrl = collectiveDiaryTrigger.AddComponent<AcademyExpansionAnomalyController>();
            diaryCtrl.Configure(ExpansionNodeType.DormitoryCollectiveDiarySecret);
            EditorUtility.SetDirty(diaryCtrl);

            CreateOrnateLantern("Lampara_Dormitorio_Sur", zone45.transform,
                new Vector3(-50.0f, vestY + 4.3f, 60.5f), matGold, matLanternGlow, new Color(1.0f, 0.78f, 0.46f), 2.1f);
            CreateOrnateLantern("Lampara_Dormitorio_Norte", zone45.transform,
                new Vector3(-50.0f, vestY + 4.3f, 72.0f), matGold, matLanternGlow, new Color(1.0f, 0.78f, 0.46f), 2.1f);
            CreatePictureSpotlight("Foco_Baul_Diario_Colectivo_Z45", zone45.transform,
                new Vector3(-59.2f, vestY + 3.6f, 76.2f), new Vector3(-60.4f, vestY + 0.48f, 76.4f),
                new Color(0.85f, 0.90f, 1.0f), 2.8f);

            // --------------------------------------------------------------------
            // ZONA 46: TALLER DE ZAPATILLAS Y TINTORERÍA ESCÉNICA
            // Cuerpo Principal: X: -62.0m a -38.0m, Z: 78.0m a 98.0m, Y = 2.4m
            // Nave Este de Patronaje (conexión directa con 30. Sala de Poleas en X = -28.0m, Z = 85.5m): X: -38.0m a -28.0m, Z: 80.0m a 98.0m, Y = 2.4m
            // --------------------------------------------------------------------
            GameObject zone46 = new GameObject("46_Cobbler_And_Scenic_Dye_Workshop");
            zone46.transform.parent = rootEnv;

            CreatePrimitive("Workshop_Floor_Main", PrimitiveType.Cube, zone46.transform,
                new Vector3(-50.0f, vestY - 0.25f, 88.0f), new Vector3(24.0f, 0.5f, 20.0f), matCeramicTile);
            CreatePrimitive("Workshop_Ceiling_Main", PrimitiveType.Cube, zone46.transform,
                new Vector3(-50.0f, vestY + wingH, 88.0f), new Vector3(24.0f, 0.5f, 20.0f), matWallIvory);
            CreatePrimitive("Workshop_Floor_EastBay", PrimitiveType.Cube, zone46.transform,
                new Vector3(-33.0f, vestY - 0.25f, 89.0f), new Vector3(10.0f, 0.5f, 18.0f), matCeramicTile);
            CreatePrimitive("Workshop_Ceiling_EastBay", PrimitiveType.Cube, zone46.transform,
                new Vector3(-33.0f, vestY + wingH, 89.0f), new Vector3(10.0f, 0.5f, 18.0f), matWallIvory);

            // Muros perimetrales de la Zona 46
            CreateWall("Workshop_Wall_West", zone46.transform,
                new Vector3(-62.0f, vestY + 2.5f, 88.0f), new Vector3(0.5f, wingH, 20.0f), matWallIvory);
            // Muro Norte de Zona 46 con puerta en X = -50.0m hacia el Patio Trasero y Cochera (Zonas 57 a 59)
            CreateWall("Workshop_Wall_North_W", zone46.transform,
                new Vector3(-56.75f, vestY + 2.5f, 98.0f), new Vector3(10.5f, wingH, 0.5f), matWallIvory);
            CreateWall("Workshop_Wall_North_E", zone46.transform,
                new Vector3(-33.25f, vestY + 2.5f, 98.0f), new Vector3(23.5f, wingH, 0.5f), matWallIvory);
            CreateArchitecturalDoor("Door_Workshop_To_CarriageCourtyard", zone46.transform,
                new Vector3(-50.0f, vestY, 98.0f), Quaternion.identity,
                2.2f, 3.4f, wingH, 0.5f, false, "",
                "COCHERA DE CARRUAJES Y PATIO NORTE (ZONAS 57 A 59)", "", matIron, matGold, matWallIvory);
            // Escalón Este-Oeste entre Z = 78.0m y Z = 80.0m (X = -38.0m) y Muro Sur de la Nave Este en Z = 80.0m (X: -38.0m a -28.0m, colindante con 19. Sala Individual)
            CreateWall("Workshop_Wall_EastStep_Z78_80", zone46.transform,
                new Vector3(-38.0f, vestY + 2.5f, 79.0f), new Vector3(0.5f, wingH, 2.0f), matWallIvory);
            CreateWall("Workshop_EastBay_Wall_South", zone46.transform,
                new Vector3(-33.0f, vestY + 2.5f, 80.0f), new Vector3(10.0f, wingH, 0.5f), matWallIvory);
            // Tramo Este entre Z = 80.0m y Z = 84.0m (X = -28.0m, colindante con el Pasillo Técnico; de Z = 84 a 104 ya cierra PulleyRoom_Wall_West con su puerta en Z = 85.5m)
            CreateWall("Workshop_EastBay_Wall_East_S", zone46.transform,
                new Vector3(-28.0f, vestY + 2.5f, 82.0f), new Vector3(0.5f, wingH, 4.0f), matWallIvory);

            // Sub-área 46A: Banco de Hormas de Madera Talladas a Medida (Oeste, X: -61.5m a -51.0m, Z: 80.0m a 96.0m)
            for (int sh = 0; sh < 3; sh++)
            {
                float shZ = 82.5f + sh * 5.0f;
                CreatePrimitive($"Estanteria_Hormas_Madera_{sh + 1}", PrimitiveType.Cube, zone46.transform,
                    new Vector3(-60.8f, vestY + 1.35f, shZ), new Vector3(0.85f, 2.7f, 3.6f), matWoodArchive);

                // Pares de hormas de madera de haya numeradas en los estantes
                for (int h = 0; h < 4; h++)
                {
                    float hZ = shZ - 1.2f + h * 0.8f;
                    DecoPiece($"Horma_Madera_Izq_{sh}_{h}", PrimitiveType.Capsule, zone46.transform,
                        new Vector3(-60.65f, vestY + 1.48f, hZ), new Vector3(0.16f, 0.14f, 0.28f), matMahogany).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    DecoPiece($"Horma_Madera_Der_{sh}_{h}", PrimitiveType.Capsule, zone46.transform,
                        new Vector3(-60.65f, vestY + 1.88f, hZ), new Vector3(0.16f, 0.14f, 0.28f), matMahogany).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                }
            }

            CreatePrimitive("Banco_Maestro_Zapatero", PrimitiveType.Cube, zone46.transform,
                new Vector3(-55.0f, vestY + 0.46f, 88.5f), new Vector3(2.2f, 0.92f, 6.5f), matWoodArchive);
            DecoPiece("Prensa_Cuero_Y_Suelas_Hierro", PrimitiveType.Cylinder, zone46.transform,
                new Vector3(-55.0f, vestY + 1.18f, 86.5f), new Vector3(0.45f, 0.32f, 0.45f), matIron);
            DecoPiece("Tarro_Resina_Colofonia_Ambar", PrimitiveType.Cylinder, zone46.transform,
                new Vector3(-54.6f, vestY + 1.04f, 90.2f), new Vector3(0.24f, 0.14f, 0.24f), matGold);

            // Sub-área 46B: Tintorería Escénica y Cubas de Tinte Negro Azabache (Centro-Norte, X: -49.0m a -39.0m, Z: 88.0m a 97.0m)
            CreatePrimitive("Plataforma_Piedra_Tintoreria", PrimitiveType.Cube, zone46.transform,
                new Vector3(-44.0f, vestY + 0.10f, 93.5f), new Vector3(8.5f, 0.20f, 6.5f), matStoneWall);
            for (int vat = 0; vat < 2; vat++)
            {
                float vatX = -46.2f + vat * 4.4f;
                CreatePrimitive($"Cuba_Tinte_Negro_Azabache_{vat + 1}", PrimitiveType.Cylinder, zone46.transform,
                    new Vector3(vatX, vestY + 0.75f, 94.0f), new Vector3(1.85f, 0.65f, 1.85f), matIron);
                DecoPiece($"Liquido_Tinte_Negro_Cisne_{vat + 1}", PrimitiveType.Cylinder, zone46.transform,
                    new Vector3(vatX, vestY + 1.32f, 94.0f), new Vector3(1.68f, 0.04f, 1.68f), matTutuBlack);
            }
            // Bastidor superior de secado con tules negros suspendidos sobre las cubas
            DecoPiece("Barra_Secado_Tules_Negros", PrimitiveType.Cylinder, zone46.transform,
                new Vector3(-44.0f, vestY + 2.85f, 94.0f), new Vector3(0.08f, 3.8f, 0.08f), matIron).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            DecoPiece("Tul_Cisne_Negro_Escurriendo_1", PrimitiveType.Cube, zone46.transform,
                new Vector3(-45.5f, vestY + 2.25f, 94.0f), new Vector3(0.85f, 1.15f, 0.12f), matTutuBlack);
            DecoPiece("Tul_Cisne_Negro_Escurriendo_2", PrimitiveType.Cube, zone46.transform,
                new Vector3(-42.5f, vestY + 2.25f, 94.0f), new Vector3(0.85f, 1.15f, 0.12f), matTutuBlack);

            // Sub-área 46C y SECRETO DE ZONA 46: Nave Este de Patronaje y Encargo de los Dos Trajes Idénticos del Cisne Negro (X: -38.0m a -28.0m, Z: 80.0m a 98.0m)
            CreatePrimitive("Mesa_Patronaje_Cisne_Negro", PrimitiveType.Cube, zone46.transform,
                new Vector3(-33.5f, vestY + 0.45f, 90.5f), new Vector3(2.4f, 0.90f, 5.5f), matMahogany);

            // Dos maniquíes gemelos vestidos con el traje idéntico del Cisne Negro (Odile) junto a la mesa de patronaje
            for (int mq = 0; mq < 2; mq++)
            {
                float mqZ = 88.8f + mq * 3.4f;
                DecoPiece($"Pedestal_Maniqui_CisneNegro_Gemelo_{mq + 1}", PrimitiveType.Cylinder, zone46.transform,
                    new Vector3(-36.2f, vestY + 0.45f, mqZ), new Vector3(0.45f, 0.45f, 0.45f), matMahogany);
                DecoPiece($"Torso_Maniqui_CisneNegro_Gemelo_{mq + 1}", PrimitiveType.Capsule, zone46.transform,
                    new Vector3(-36.2f, vestY + 1.35f, mqZ), new Vector3(0.42f, 0.52f, 0.32f), matTutuBlack);
                DecoPiece($"Faldellin_Tutu_Negro_Gemelo_{mq + 1}", PrimitiveType.Cylinder, zone46.transform,
                    new Vector3(-36.2f, vestY + 1.08f, mqZ), new Vector3(0.95f, 0.06f, 0.95f), matTutuBlack);
            }

            // SECRETO DE ZONA 46: Libro de Encargos Reservados de Sastrería y Hormas Modificadas N.º 14 y N.º 7 sobre la Mesa de Patronaje
            GameObject workshopSecretTrigger = CreatePrimitive("Libro_Encargos_Dos_Cisnes_Negros_Y_Alzas_Secreto", PrimitiveType.Cube, zone46.transform,
                new Vector3(-33.5f, vestY + 0.94f, 90.5f), new Vector3(0.65f, 0.08f, 0.48f), matLeather);
            DecoPiece("Horma_Modificada_Con_Alzas_Mara_N7", PrimitiveType.Capsule, zone46.transform,
                new Vector3(-33.0f, vestY + 0.98f, 91.2f), new Vector3(0.18f, 0.16f, 0.32f), matMahogany).transform.localRotation = Quaternion.Euler(90f, -25f, 0f);
            AcademyExpansionAnomalyController workshopCtrl = workshopSecretTrigger.AddComponent<AcademyExpansionAnomalyController>();
            workshopCtrl.Configure(ExpansionNodeType.WorkshopTwinBlackSwanOrderSecret);
            EditorUtility.SetDirty(workshopCtrl);

            CreateOrnateLantern("Lampara_Taller_Zapateria_Oeste", zone46.transform,
                new Vector3(-54.0f, vestY + 4.3f, 88.5f), matGold, matLanternGlow, new Color(1.0f, 0.80f, 0.48f), 2.1f);
            CreateOrnateLantern("Lampara_Taller_Patronaje_Este", zone46.transform,
                new Vector3(-33.5f, vestY + 4.3f, 89.0f), matGold, matLanternGlow, new Color(1.0f, 0.80f, 0.48f), 2.1f);
            CreatePictureSpotlight("Foco_Mesa_Patronaje_Secreto_Z46", zone46.transform,
                new Vector3(-33.5f, vestY + 3.9f, 90.5f), new Vector3(-33.5f, vestY + 0.94f, 90.5f),
                new Color(1.0f, 0.84f, 0.55f), 3.0f);

            // ====================================================================
            // SECCIÓN 12: PLANTA ALTA, RED SUBTERRÁNEA Y EXTERIORES TRASEROS (ZONAS 49 A 59)
            // ====================================================================

            // --------------------------------------------------------------------
            // BLOQUE A: PLANTA ALTA CLÁSICA (Y = 7.70m, ZONAS 49 A 52)
            // Conectada con la Terraza (Zona 42) en X = -11.0m, Z = 20.0m y con la Pasarela (Zona 35) en Z = 78.0m
            // --------------------------------------------------------------------
            GameObject blockUpperFloor = new GameObject("Section12_UpperFloor_Zones_49_To_52");
            blockUpperFloor.transform.parent = rootEnv;
            float upperFloorH = 4.2f;

            // ZONA 49: Galería Superior del Vestíbulo (Mezzanine / Balcón Imperial) (X: -11.0m a +11.0m, Z: 9.0m a 31.0m, Y = 7.70m)
            // Diseñada como una galería en forma de U con vano central abierto con barandilla que mira hacia el Vestíbulo inferior (Y = 2.4m)
            GameObject zone49 = new GameObject("49_Mezzanine_Imperial_Gallery");
            zone49.transform.parent = blockUpperFloor.transform;

            // Galería Oeste (X: -11..-6, Z: 9..31)
            CreatePrimitive("Mezzanine_Floor_West", PrimitiveType.Cube, zone49.transform,
                new Vector3(-8.5f, roofY - 0.2f, 20.0f), new Vector3(5.0f, 0.4f, 22.0f), matParquet);
            CreatePrimitive("Mezzanine_Carpet_West", PrimitiveType.Cube, zone49.transform,
                new Vector3(-8.5f, roofY + 0.02f, 20.0f), new Vector3(2.4f, 0.04f, 20.0f), matCarpet);
            // Galería Este (X: 6..11, Z: 9..31)
            CreatePrimitive("Mezzanine_Floor_East", PrimitiveType.Cube, zone49.transform,
                new Vector3(8.5f, roofY - 0.2f, 20.0f), new Vector3(5.0f, 0.4f, 22.0f), matParquet);
            CreatePrimitive("Mezzanine_Carpet_East", PrimitiveType.Cube, zone49.transform,
                new Vector3(8.5f, roofY + 0.02f, 20.0f), new Vector3(2.4f, 0.04f, 20.0f), matCarpet);
            // Galería Sur (X: -6..6, Z: 9..14)
            CreatePrimitive("Mezzanine_Floor_South", PrimitiveType.Cube, zone49.transform,
                new Vector3(0.0f, roofY - 0.2f, 11.5f), new Vector3(7.0f, 0.4f, 5.0f), matParquet);
            // Galería Norte (X: -6..6, Z: 26..31)
            CreatePrimitive("Mezzanine_Floor_North", PrimitiveType.Cube, zone49.transform,
                new Vector3(0.0f, roofY - 0.2f, 28.5f), new Vector3(7.0f, 0.4f, 5.0f), matParquet);

            // Balaustrada dorada interior que asoma al vacío del Vestíbulo
            CreateArchitecturalBalustrade("Mezzanine_Balustrade_Inner_W", zone49.transform,
                new Vector3(-6.0f, roofY, 20.0f), 12.0f, matGold, matMahogany);
            CreateArchitecturalBalustrade("Mezzanine_Balustrade_Inner_E", zone49.transform,
                new Vector3(6.0f, roofY, 20.0f), 12.0f, matGold, matMahogany);

            // Muros exteriores de la Mezzanine
            CreateWall("Mezzanine_Wall_West", zone49.transform,
                new Vector3(-11.0f, roofY + 2.1f, 20.0f), new Vector3(0.5f, upperFloorH, 22.0f), matWallIvory);
            CreateWall("Mezzanine_Wall_East", zone49.transform,
                new Vector3(11.0f, roofY + 2.1f, 20.0f), new Vector3(0.5f, upperFloorH, 22.0f), matWallIvory);
            CreateWall("Mezzanine_Wall_South", zone49.transform,
                new Vector3(0.0f, roofY + 2.1f, 9.0f), new Vector3(22.0f, upperFloorH, 0.5f), matWallIvory);
            CreateWall("Mezzanine_Wall_North_W", zone49.transform,
                new Vector3(-6.75f, roofY + 2.1f, 31.0f), new Vector3(8.5f, upperFloorH, 0.5f), matWallIvory);
            CreateWall("Mezzanine_Wall_North_E", zone49.transform,
                new Vector3(6.75f, roofY + 2.1f, 31.0f), new Vector3(8.5f, upperFloorH, 0.5f), matWallIvory);

            // Puerta Norte de la Mezzanine hacia las Aulas de Teoría (Zona 50)
            CreateArchitecturalDoubleDoor("Doors_Mezzanine_To_TheoryClass", zone49.transform,
                new Vector3(0.0f, roofY, 31.0f), Quaternion.identity,
                3.0f, 3.4f, upperFloorH, 0.5f, false, "",
                "AULAS DE TEORÍA ESCÉNICA Y ANATOMÍA (ZONA 50)", "", matMahogany, matGold, matWallIvory);

            // Secreto / Punto de Observación de Zona 49
            GameObject mezzanineTrigger = CreatePrimitive("Atril_Protocolo_Mezzanine_Z49", PrimitiveType.Cube, zone49.transform,
                new Vector3(0.0f, roofY + 0.52f, 14.5f), new Vector3(1.2f, 1.04f, 0.8f), matMahogany);
            DecoPiece("Libro_Protocolo_Ceremonial_Z49", PrimitiveType.Cube, zone49.transform,
                new Vector3(0.0f, roofY + 1.08f, 14.5f), new Vector3(0.54f, 0.08f, 0.38f), matLeather);
            AcademyExpansionAnomalyController mezzanineCtrl = mezzanineTrigger.AddComponent<AcademyExpansionAnomalyController>();
            mezzanineCtrl.Configure(ExpansionNodeType.MezzanineImperialBalconyOverview);
            EditorUtility.SetDirty(mezzanineCtrl);

            // ZONA 50: Aulas de Teoría Escénica, Anatomía y Linterna Mágica (X: -16.0m a +16.0m, Z: 31.0m a 55.0m, Y = 7.70m)
            GameObject zone50 = new GameObject("50_Theory_And_Anatomy_Amphitheatre");
            zone50.transform.parent = blockUpperFloor.transform;

            CreatePrimitive("Theory_Floor", PrimitiveType.Cube, zone50.transform,
                new Vector3(0.0f, roofY - 0.2f, 43.0f), new Vector3(32.0f, 0.4f, 24.0f), matParquet);
            CreatePrimitive("Theory_Ceiling", PrimitiveType.Cube, zone50.transform,
                new Vector3(0.0f, roofY + upperFloorH, 43.0f), new Vector3(32.0f, 0.4f, 24.0f), matWallIvory);
            CreateWall("Theory_Wall_West", zone50.transform,
                new Vector3(-16.0f, roofY + 2.1f, 43.0f), new Vector3(0.5f, upperFloorH, 24.0f), matWallIvory);
            CreateWall("Theory_Wall_East", zone50.transform,
                new Vector3(16.0f, roofY + 2.1f, 43.0f), new Vector3(0.5f, upperFloorH, 24.0f), matWallIvory);
            CreateWall("Theory_Wall_North_W", zone50.transform,
                new Vector3(-9.25f, roofY + 2.1f, 55.0f), new Vector3(13.5f, upperFloorH, 0.5f), matWallIvory);
            CreateWall("Theory_Wall_North_E", zone50.transform,
                new Vector3(9.25f, roofY + 2.1f, 55.0f), new Vector3(13.5f, upperFloorH, 0.5f), matWallIvory);

            // Puerta Norte hacia la Residencia de Profesores (Zona 51)
            CreateArchitecturalDoubleDoor("Doors_Theory_To_ProfessorsResidence", zone50.transform,
                new Vector3(0.0f, roofY, 55.0f), Quaternion.identity,
                3.0f, 3.4f, upperFloorH, 0.5f, false, "",
                "RESIDENCIA DE PROFESORES Y DEL MAESTRO VIKTOR KROL (ZONA 51)", "", matMahogany, matGold, matWallIvory);

            // Mobiliario: Anfiteatro escalonado con pupitres y proyector de Linterna Mágica
            for (int r = 0; r < 3; r++)
            {
                float tierZ = 35.0f + r * 3.5f;
                float tierY = roofY + (r * 0.35f);
                CreatePrimitive($"Grada_Teoria_{r + 1}", PrimitiveType.Cube, zone50.transform,
                    new Vector3(0.0f, tierY - 0.17f, tierZ), new Vector3(18.0f, 0.35f, 3.0f), matWoodArchive);
                CreatePrimitive($"Pupitre_Teoria_{r + 1}", PrimitiveType.Cube, zone50.transform,
                    new Vector3(0.0f, tierY + 0.42f, tierZ), new Vector3(16.0f, 0.84f, 0.9f), matMahogany);
            }
            // Pantalla de proyección y pizarra anatómica al frente (Norte)
            CreatePrimitive("Pantalla_Proyeccion_Lienzo", PrimitiveType.Cube, zone50.transform,
                new Vector3(0.0f, roofY + 2.2f, 54.5f), new Vector3(8.5f, 3.2f, 0.1f), matClothCover);

            // Linterna mágica / Proyector de placas de cristal (SECRETO ZONA 50)
            GameObject projectorObj = CreatePrimitive("Proyector_Linterna_Magica_Laton", PrimitiveType.Cube, zone50.transform,
                new Vector3(0.0f, roofY + 0.75f, 33.5f), new Vector3(0.85f, 0.70f, 1.2f), matGold);
            DecoPiece("Tubo_Optico_Proyector", PrimitiveType.Cylinder, projectorObj.transform,
                new Vector3(0.0f, 0.1f, 0.75f), new Vector3(0.24f, 0.45f, 0.24f), matGold).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Light projectorBeam = CreatePictureSpotlight("Luz_Haz_Proyector_Cristal", zone50.transform,
                new Vector3(0.0f, roofY + 1.2f, 34.0f), new Vector3(0.0f, roofY + 2.2f, 54.5f),
                new Color(1.0f, 0.92f, 0.75f), 3.2f);
            AcademyExpansionAnomalyController projectorCtrl = projectorObj.AddComponent<AcademyExpansionAnomalyController>();
            projectorCtrl.Configure(ExpansionNodeType.TheoryGlassSlideProjectorSecret, projectorObj.transform, null, null, projectorBeam);
            EditorUtility.SetDirty(projectorCtrl);

            // ZONA 51: Residencia de Profesores y del Maestro Viktor Krol (X: -16.0m a +16.0m, Z: 55.0m a 72.0m, Y = 7.70m)
            GameObject zone51 = new GameObject("51_Professors_And_Maestro_Krol_Residence");
            zone51.transform.parent = blockUpperFloor.transform;

            CreatePrimitive("Residence_Floor", PrimitiveType.Cube, zone51.transform,
                new Vector3(0.0f, roofY - 0.2f, 63.5f), new Vector3(32.0f, 0.4f, 17.0f), matParquet);
            CreatePrimitive("Residence_Ceiling", PrimitiveType.Cube, zone51.transform,
                new Vector3(0.0f, roofY + upperFloorH, 63.5f), new Vector3(32.0f, 0.4f, 17.0f), matWallIvory);
            CreateWall("Residence_Wall_West", zone51.transform,
                new Vector3(-16.0f, roofY + 2.1f, 63.5f), new Vector3(0.5f, upperFloorH, 17.0f), matWallIvory);
            CreateWall("Residence_Wall_East", zone51.transform,
                new Vector3(16.0f, roofY + 2.1f, 63.5f), new Vector3(0.5f, upperFloorH, 17.0f), matWallIvory);
            CreateWall("Residence_Wall_North_W", zone51.transform,
                new Vector3(-9.25f, roofY + 2.1f, 72.0f), new Vector3(13.5f, upperFloorH, 0.5f), matWallIvory);
            CreateWall("Residence_Wall_North_E", zone51.transform,
                new Vector3(9.25f, roofY + 2.1f, 72.0f), new Vector3(13.5f, upperFloorH, 0.5f), matWallIvory);

            // Puerta Norte hacia la Torre del Reloj (Zona 52)
            CreateArchitecturalDoor("Door_Residence_To_ClockTower", zone51.transform,
                new Vector3(0.0f, roofY, 72.0f), Quaternion.identity,
                2.0f, 3.2f, upperFloorH, 0.5f, false, "",
                "DESVÁN Y TORRE DEL RELOJ DE LA ACADEMIA (ZONA 52)", "", matMahogany, matIron, matWallIvory);

            // Estudio y Armonio del Maestro Viktor Krol
            CreatePrimitive("Armonio_Viktor_Krol", PrimitiveType.Cube, zone51.transform,
                new Vector3(-8.5f, roofY + 0.65f, 68.0f), new Vector3(2.4f, 1.3f, 1.2f), matMahogany);
            CreatePrimitive("Atril_Partitura_Maestro_Krol", PrimitiveType.Cube, zone51.transform,
                new Vector3(-8.5f, roofY + 1.25f, 67.8f), new Vector3(0.9f, 0.2f, 0.3f), matGold);

            // SECRETO ZONA 51: Partitura original corregida del Acto III
            GameObject krolScoreTrigger = CreatePrimitive("Partitura_Acto3_Anotada_Krol_Secreto", PrimitiveType.Cube, zone51.transform,
                new Vector3(-8.5f, roofY + 1.35f, 67.8f), new Vector3(0.48f, 0.06f, 0.36f), matSecretPaper);
            AcademyExpansionAnomalyController krolCtrl = krolScoreTrigger.AddComponent<AcademyExpansionAnomalyController>();
            krolCtrl.Configure(ExpansionNodeType.MaestroKrolAlteredScoreSecret);
            EditorUtility.SetDirty(krolCtrl);

            // ZONA 52: Desván y Torre del Reloj de la Academia (X: -12.0m a +12.0m, Z: 72.0m a 88.0m, Y = 7.70m, conecta con Pasarela Zona 35 en Z = 88.0m)
            GameObject zone52 = new GameObject("52_Clock_Tower_And_High_Attic");
            zone52.transform.parent = blockUpperFloor.transform;

            CreatePrimitive("ClockTower_Floor", PrimitiveType.Cube, zone52.transform,
                new Vector3(0.0f, roofY - 0.2f, 80.0f), new Vector3(24.0f, 0.4f, 16.0f), matDarkCement);
            CreatePrimitive("ClockTower_Ceiling", PrimitiveType.Cube, zone52.transform,
                new Vector3(0.0f, roofY + 6.5f, 80.0f), new Vector3(24.0f, 0.4f, 16.0f), matAgedBrick);
            CreateWall("ClockTower_Wall_West", zone52.transform,
                new Vector3(-12.0f, roofY + 3.25f, 80.0f), new Vector3(0.5f, 6.5f, 16.0f), matAgedBrick);
            CreateWall("ClockTower_Wall_East", zone52.transform,
                new Vector3(12.0f, roofY + 3.25f, 80.0f), new Vector3(0.5f, 6.5f, 16.0f), matAgedBrick);

            // Muro Norte (Z = 88.0m) con paso hacia el puente de la Pasarela Superior (Zona 35)
            CreateWall("ClockTower_Wall_North_W", zone52.transform,
                new Vector3(-7.25f, roofY + 3.25f, 88.0f), new Vector3(9.5f, 6.5f, 0.5f), matAgedBrick);
            CreateWall("ClockTower_Wall_North_E", zone52.transform,
                new Vector3(7.25f, roofY + 3.25f, 88.0f), new Vector3(9.5f, 6.5f, 0.5f), matAgedBrick);
            CreateArchitecturalDoor("Door_ClockTower_To_TheatreCatwalk", zone52.transform,
                new Vector3(0.0f, roofY, 88.0f), Quaternion.identity,
                2.0f, 3.2f, 6.5f, 0.5f, false, "",
                "PASARELA SUPERIOR DEL GRAN TEATRO (ZONA 35)", "", matCatwalkSteel, matGold, matAgedBrick);

            // Gran maquinaria de bronce del reloj maestro detenido a las 20:15 (SECRETO ZONA 52)
            GameObject clockGearMaster = CreatePrimitive("Engranaje_Maestro_Reloj_Torre_Z52", PrimitiveType.Cylinder, zone52.transform,
                new Vector3(0.0f, roofY + 2.5f, 81.5f), new Vector3(3.2f, 0.35f, 3.2f), matGold);
            clockGearMaster.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            GameObject jammedWedge = DecoPiece("Cuña_Hierro_Sabotaje_Reloj_2015", PrimitiveType.Cube, clockGearMaster.transform,
                new Vector3(1.2f, 0.0f, 0.8f), new Vector3(0.35f, 0.55f, 0.35f), matIron);
            AcademyExpansionAnomalyController clockCtrl = clockGearMaster.AddComponent<AcademyExpansionAnomalyController>();
            clockCtrl.Configure(ExpansionNodeType.ClockTowerJammedGearSecret, clockGearMaster.transform);
            EditorUtility.SetDirty(clockCtrl);

            // --------------------------------------------------------------------
            // BLOQUE B: RED SUBTERRÁNEA COMPLETA (Y = -3.20m, ZONAS 53 A 56)
            // Conectada al Sótano de Mantenimiento (Zona 41) en X = 16.0m, Z = -21.5m
            // --------------------------------------------------------------------
            GameObject blockSubterranean = new GameObject("Section12_Subterranean_Zones_53_To_56");
            blockSubterranean.transform.parent = rootEnv;
            float subFloorY = -3.2f;
            float subVaultH = 4.2f;

            // ZONA 53: Sala de Calderas y Generadores Eléctricos (X: -15.0m a +16.0m, Z: -34.0m a -14.0m, Y = -3.20m)
            GameObject zone53 = new GameObject("53_Boiler_Room_And_Electrical_Generators");
            zone53.transform.parent = blockSubterranean.transform;

            CreatePrimitive("Boiler_Floor", PrimitiveType.Cube, zone53.transform,
                new Vector3(0.5f, subFloorY - 0.2f, -24.0f), new Vector3(31.0f, 0.4f, 20.0f), matDarkCement);
            CreatePrimitive("Boiler_Ceiling", PrimitiveType.Cube, zone53.transform,
                new Vector3(0.5f, subFloorY + subVaultH, -24.0f), new Vector3(31.0f, 0.4f, 20.0f), matAgedBrick);
            CreateWall("Boiler_Wall_South", zone53.transform,
                new Vector3(0.5f, subFloorY + 2.1f, -34.0f), new Vector3(31.0f, subVaultH, 0.5f), matAgedBrick);
            CreateWall("Boiler_Wall_North_W", zone53.transform,
                new Vector3(-8.5f, subFloorY + 2.1f, -14.0f), new Vector3(13.0f, subVaultH, 0.5f), matAgedBrick);
            CreateWall("Boiler_Wall_North_E", zone53.transform,
                new Vector3(9.5f, subFloorY + 2.1f, -14.0f), new Vector3(13.0f, subVaultH, 0.5f), matAgedBrick);

            // Puerta Norte hacia la Cripta Fundacional (Zona 56)
            CreateArchitecturalDoor("Door_Boiler_To_AncientCrypt", zone53.transform,
                new Vector3(0.5f, subFloorY, -14.0f), Quaternion.identity,
                2.0f, 3.2f, subVaultH, 0.5f, false, "",
                "CRIPTA Y CIMIENTOS ANTIGUOS DEL SIGLO XVIII (ZONA 56)", "", matIron, matGold, matAgedBrick);

            // Puerta Oeste hacia la Lavandería Subterránea (Zona 54) en X = -15.0m, Z = -24.0m
            CreateWall("Boiler_Wall_West_S", zone53.transform,
                new Vector3(-15.0f, subFloorY + 2.1f, -29.5f), new Vector3(0.5f, subVaultH, 9.0f), matAgedBrick);
            CreateWall("Boiler_Wall_West_N", zone53.transform,
                new Vector3(-15.0f, subFloorY + 2.1f, -18.5f), new Vector3(0.5f, subVaultH, 9.0f), matAgedBrick);
            CreateArchitecturalDoor("Door_Boiler_To_Laundry", zone53.transform,
                new Vector3(-15.0f, subFloorY, -24.0f), Quaternion.Euler(0f, 90f, 0f),
                2.0f, 3.2f, subVaultH, 0.5f, false, "",
                "LAVANDERÍA SUBTERRÁNEA Y MONTACARGAS (ZONA 54)", "", matIron, matGold, matAgedBrick);

            // Batería de 3 grandes calderas cilíndricas de carbón remachadas
            GameObject boiler3Door = null;
            for (int b = 0; b < 3; b++)
            {
                float bx = -8.0f + b * 8.5f;
                CreatePrimitive($"Caldera_Cilindro_Remachado_{b + 1}", PrimitiveType.Cylinder, zone53.transform,
                    new Vector3(bx, subFloorY + 1.8f, -28.5f), new Vector3(3.2f, 1.8f, 3.2f), matBoilerTank);
                GameObject door = CreatePrimitive($"Puerta_Cenicero_Caldera_{b + 1}", PrimitiveType.Cube, zone53.transform,
                    new Vector3(bx, subFloorY + 0.65f, -26.8f), new Vector3(1.1f, 0.85f, 0.25f), matIron);
                if (b == 2) boiler3Door = door;
            }

            // SECRETO ZONA 53: La Caldera N.º 3 con restos del tutú incinerado y frascos de sedante
            AcademyExpansionAnomalyController boilerCtrl = boiler3Door.AddComponent<AcademyExpansionAnomalyController>();
            boilerCtrl.Configure(ExpansionNodeType.BoilerIncinerationFurnaceSecret, boiler3Door.transform);
            EditorUtility.SetDirty(boilerCtrl);

            // ZONA 54: Lavandería Industrial Subterránea y Pozo del Montacargas (X: -65.0m a -15.0m, Z: 18.0m a 36.0m, Y = -3.20m)
            // Conecta en la vertical con el Oficio de Comedor (Zona 47 en X = -60.8m, Z = 29.3m)
            GameObject zone54 = new GameObject("54_Subterranean_Laundry_And_Dumbwaiter");
            zone54.transform.parent = blockSubterranean.transform;

            // Galería conector subterránea de X: -15m a -38m, Z: -26m a 20m
            CreatePrimitive("Laundry_Tunnel_Floor", PrimitiveType.Cube, zone54.transform,
                new Vector3(-26.5f, subFloorY - 0.2f, -3.0f), new Vector3(23.0f, 0.4f, 44.0f), matDarkCement);
            CreatePrimitive("Laundry_Tunnel_Ceiling", PrimitiveType.Cube, zone54.transform,
                new Vector3(-26.5f, subFloorY + subVaultH, -3.0f), new Vector3(23.0f, 0.4f, 44.0f), matAgedBrick);
            // Nave principal de lavado (X: -65..-38, Z: 18..36)
            CreatePrimitive("Laundry_Main_Floor", PrimitiveType.Cube, zone54.transform,
                new Vector3(-51.5f, subFloorY - 0.2f, 27.0f), new Vector3(27.0f, 0.4f, 18.0f), matDarkCement);
            CreatePrimitive("Laundry_Main_Ceiling", PrimitiveType.Cube, zone54.transform,
                new Vector3(-51.5f, subFloorY + subVaultH, 27.0f), new Vector3(27.0f, 0.4f, 18.0f), matAgedBrick);
            CreateWall("Laundry_Wall_West", zone54.transform,
                new Vector3(-65.0f, subFloorY + 2.1f, 27.0f), new Vector3(0.5f, subVaultH, 18.0f), matAgedBrick);
            CreateWall("Laundry_Wall_North", zone54.transform,
                new Vector3(-51.5f, subFloorY + 2.1f, 36.0f), new Vector3(27.0f, subVaultH, 0.5f), matAgedBrick);

            // Pozo y Terminal Inferior del Montacargas (SECRETO ZONA 54)
            GameObject dumbwaiterShaft = CreatePrimitive("Pozo_Montacargas_Subterraneo_Terminal_Z54", PrimitiveType.Cube, zone54.transform,
                new Vector3(-60.8f, subFloorY + 1.85f, 29.3f), new Vector3(1.8f, 3.7f, 1.6f), matIron);
            DecoPiece("Caja_Montacargas_Inferior", PrimitiveType.Cube, dumbwaiterShaft.transform,
                new Vector3(0f, -0.4f, 0f), new Vector3(1.4f, 1.4f, 1.2f), matWoodArchive);
            AcademyExpansionAnomalyController laundryCtrl = dumbwaiterShaft.AddComponent<AcademyExpansionAnomalyController>();
            laundryCtrl.Configure(ExpansionNodeType.SubterraneanLaundryDumbwaiterSecret);
            EditorUtility.SetDirty(laundryCtrl);

            // ZONA 56: Cripta y Cimientos del Siglo XVIII (X: -15.0m a +15.0m, Z: -14.0m a 60.0m, Y = -3.20m)
            // Galería central abovedada que cruza bajo toda la academia
            GameObject zone56 = new GameObject("56_Ancient_Crypt_And_Founders_Vault");
            zone56.transform.parent = blockSubterranean.transform;

            CreatePrimitive("Crypt_Floor", PrimitiveType.Cube, zone56.transform,
                new Vector3(0.0f, subFloorY - 0.2f, 23.0f), new Vector3(8.0f, 0.4f, 74.0f), matStoneWall);
            CreatePrimitive("Crypt_Vault_Ceiling", PrimitiveType.Cube, zone56.transform,
                new Vector3(0.0f, subFloorY + subVaultH, 23.0f), new Vector3(8.0f, 0.4f, 74.0f), matAgedBrick);
            CreateWall("Crypt_Wall_West", zone56.transform,
                new Vector3(-4.0f, subFloorY + 2.1f, 23.0f), new Vector3(0.5f, subVaultH, 74.0f), matStoneWall);
            CreateWall("Crypt_Wall_East", zone56.transform,
                new Vector3(4.0f, subFloorY + 2.1f, 23.0f), new Vector3(0.5f, subVaultH, 74.0f), matStoneWall);

            // Altar del siglo XVIII y Lápida Fundacional en el centro de la cripta (SECRETO ZONA 56)
            GameObject cryptAltar = CreatePrimitive("Altar_Secular_Cripta_Fundacional_Z56", PrimitiveType.Cube, zone56.transform,
                new Vector3(0.0f, subFloorY + 0.65f, 23.0f), new Vector3(2.4f, 1.3f, 1.6f), matStoneWall);
            DecoPiece("Lapida_Fundacional_1768", PrimitiveType.Cube, cryptAltar.transform,
                new Vector3(0.0f, 0.55f, 0.0f), new Vector3(1.9f, 0.15f, 1.2f), matMarble);
            AcademyExpansionAnomalyController cryptCtrl = cryptAltar.AddComponent<AcademyExpansionAnomalyController>();
            cryptCtrl.Configure(ExpansionNodeType.AncientCryptFoundersPassageSecret);
            EditorUtility.SetDirty(cryptCtrl);

            // Puerta Norte de la Cripta hacia el Foso Bajo el Escenario (Zona 55) en Z = 60.0m
            CreateArchitecturalDoor("Door_Crypt_To_Understage", zone56.transform,
                new Vector3(0.0f, subFloorY, 60.0f), Quaternion.identity,
                2.0f, 3.2f, subVaultH, 0.5f, false, "",
                "FOSO BAJO EL ESCENARIO — UNDERSTAGE (ZONA 55)", "", matIron, matGold, matStoneWall);

            // ZONA 55: Foso Bajo el Escenario (Understage y Trampilla del Cisne) (X: -22.0m a +10.0m, Z: 60.0m a 108.0m, Y = -3.20m)
            // Justo debajo del Escenario Principal (Zona 32 en Y = 2.4m)
            GameObject zone55 = new GameObject("55_Understage_Pit_And_Trapdoor");
            zone55.transform.parent = blockSubterranean.transform;

            CreatePrimitive("Understage_Floor", PrimitiveType.Cube, zone55.transform,
                new Vector3(-6.0f, subFloorY - 0.2f, 84.0f), new Vector3(32.0f, 0.4f, 48.0f), matDarkCement);
            CreatePrimitive("Understage_Ceiling", PrimitiveType.Cube, zone55.transform,
                new Vector3(-6.0f, subFloorY + 5.4f, 84.0f), new Vector3(32.0f, 0.4f, 48.0f), matTheatreDarkWood);
            CreateWall("Understage_Wall_West", zone55.transform,
                new Vector3(-22.0f, subFloorY + 2.7f, 84.0f), new Vector3(0.5f, 5.4f, 48.0f), matAgedBrick);
            CreateWall("Understage_Wall_North", zone55.transform,
                new Vector3(-6.0f, subFloorY + 2.7f, 108.0f), new Vector3(32.0f, 5.4f, 0.5f), matAgedBrick);

            // Conexión Este (X = 10.0m, Z = 100.0m) hacia la Zona Secreta B-00 (Zonas 36 a 39)
            CreateWall("Understage_Wall_East_S", zone55.transform,
                new Vector3(10.0f, subFloorY + 2.7f, 78.0f), new Vector3(0.5f, 5.4f, 36.0f), matAgedBrick);
            CreateWall("Understage_Wall_East_N", zone55.transform,
                new Vector3(10.0f, subFloorY + 2.7f, 104.5f), new Vector3(0.5f, 5.4f, 7.0f), matAgedBrick);
            CreateArchitecturalDoor("Door_Understage_To_SecretVaultB00", zone55.transform,
                new Vector3(10.0f, subFloorY, 100.0f), Quaternion.Euler(0f, 90f, 0f),
                2.0f, 3.2f, 5.4f, 0.5f, false, "",
                "BÓVEDA ESTRUCTURAL B-00 Y CÁMARA TRAS EL ESCENARIO (SECCIÓN 8)", "", matIron, matGold, matAgedBrick);

            // Bosque de puntales de madera bajo las tablas del escenario
            for (int px = -18; px <= 6; px += 6)
            {
                for (int pz = 66; pz <= 102; pz += 9)
                {
                    DecoPiece($"Puntal_Madera_Understage_{px}_{pz}", PrimitiveType.Cube, zone55.transform,
                        new Vector3(px, subFloorY + 2.7f, pz), new Vector3(0.35f, 5.4f, 0.35f), matWoodArchive);
                }
            }

            // Mecanismo inferior de la Trampilla de Apariciones (SECRETO ZONA 55)
            GameObject trapdoorMechanism = CreatePrimitive("Mecanismo_Trampilla_Escotillon_Z55", PrimitiveType.Cube, zone55.transform,
                new Vector3(-8.0f, subFloorY + 3.8f, 100.0f), new Vector3(2.6f, 0.45f, 2.6f), matIron);
            DecoPiece("Palanca_Cerrojo_Saboteado_Z55", PrimitiveType.Cylinder, trapdoorMechanism.transform,
                new Vector3(0.8f, -0.6f, 0.0f), new Vector3(0.12f, 0.9f, 0.12f), matGold);
            AcademyExpansionAnomalyController understageCtrl = trapdoorMechanism.AddComponent<AcademyExpansionAnomalyController>();
            understageCtrl.Configure(ExpansionNodeType.UnderstageTrapdoorMechanismSecret, trapdoorMechanism.transform);
            EditorUtility.SetDirty(understageCtrl);

            // --------------------------------------------------------------------
            // BLOQUE C: RECINTO EXTERIOR TRASERO Y PARQUE (Y = 0.0m a 2.4m, ZONAS 57 A 59)
            // Conectado al Taller de Zapatillas (Zona 46) en X = -50.0m, Z = 98.0m
            // --------------------------------------------------------------------
            GameObject blockExteriors = new GameObject("Section12_Exteriors_Zones_57_To_59");
            blockExteriors.transform.parent = rootEnv;

            // ZONA 57: Cochera de Carruajes y Patio de Carga Norte (X: -65.0m a -30.0m, Z: 98.0m a 125.0m, Y = 0.0m)
            GameObject zone57 = new GameObject("57_Carriage_House_And_North_Courtyard");
            zone57.transform.parent = blockExteriors.transform;

            CreatePrimitive("Carriage_Ground", PrimitiveType.Cube, zone57.transform,
                new Vector3(-47.5f, -0.2f, 111.5f), new Vector3(35.0f, 0.4f, 27.0f), matCobble);
            CreateWall("Carriage_Wall_West", zone57.transform,
                new Vector3(-65.0f, 2.5f, 111.5f), new Vector3(0.6f, 5.0f, 27.0f), matStoneWall);
            CreateWall("Carriage_Wall_North", zone57.transform,
                new Vector3(-47.5f, 2.5f, 125.0f), new Vector3(35.0f, 5.0f, 0.6f), matStoneWall);

            // Carruaje negro de gala histórico
            GameObject carriageObj = CreatePrimitive("Carruaje_Gala_Negro_Historico", PrimitiveType.Cube, zone57.transform,
                new Vector3(-55.0f, 1.4f, 112.0f), new Vector3(2.8f, 2.4f, 4.8f), matLeather);
            for (int w = 0; w < 4; w++)
            {
                float wx = (w % 2 == 0) ? -56.5f : -53.5f;
                float wz = (w < 2) ? 110.2f : 113.8f;
                DecoPiece($"Rueda_Carruaje_{w + 1}", PrimitiveType.Cylinder, zone57.transform,
                    new Vector3(wx, 0.6f, wz), new Vector3(1.2f, 0.14f, 1.2f), matIron).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }

            // SECRETO ZONA 57: El baúl confiscado con billetes hacia Viena a nombre de Elena y Mara
            GameObject carriageTrunk = CreatePrimitive("Baul_Viaje_Confiscado_Elena_Z57", PrimitiveType.Cube, zone57.transform,
                new Vector3(-58.5f, 0.45f, 112.0f), new Vector3(0.85f, 0.65f, 1.4f), matWoodArchive);
            AcademyExpansionAnomalyController carriageCtrl = carriageTrunk.AddComponent<AcademyExpansionAnomalyController>();
            carriageCtrl.Configure(ExpansionNodeType.CarriageHouseConfiscatedTrunkSecret);
            EditorUtility.SetDirty(carriageCtrl);

            // ZONA 58: Pabellón del Guarda y Centralita Telefónica Exterior (X: -65.0m a -48.0m, Z: 125.0m a 145.0m, Y = 0.0m)
            GameObject zone58 = new GameObject("58_Gatehouse_And_Exterior_Switchboard");
            zone58.transform.parent = blockExteriors.transform;

            CreatePrimitive("Gatehouse_Floor", PrimitiveType.Cube, zone58.transform,
                new Vector3(-56.5f, -0.2f, 135.0f), new Vector3(17.0f, 0.4f, 20.0f), matCobble);
            CreatePrimitive("Gatehouse_Roof", PrimitiveType.Cube, zone58.transform,
                new Vector3(-56.5f, 4.2f, 135.0f), new Vector3(17.0f, 0.4f, 20.0f), matStoneWall);
            CreateWall("Gatehouse_Wall_West", zone58.transform,
                new Vector3(-65.0f, 2.0f, 135.0f), new Vector3(0.5f, 4.0f, 20.0f), matStoneWall);
            CreateWall("Gatehouse_Wall_North", zone58.transform,
                new Vector3(-56.5f, 2.0f, 145.0f), new Vector3(17.0f, 4.0f, 0.5f), matStoneWall);

            // Gran verja exterior de salida al bosque
            CreateWroughtIronGate("Verja_Hierro_Salida_Exterior_Z58", zone58.transform,
                new Vector3(-56.5f, 0.0f, 145.0f), 5.5f, 4.2f, matIron, false);

            // Centralita telefónica de clavijas con cables cortados (SECRETO ZONA 58)
            GameObject switchboardObj = CreatePrimitive("Centralita_Telefonica_Cables_Cortados_Z58", PrimitiveType.Cube, zone58.transform,
                new Vector3(-62.0f, 1.25f, 136.0f), new Vector3(0.85f, 2.2f, 2.2f), matMahogany);
            DecoPiece("Cables_Cobre_Cortados_Centralita", PrimitiveType.Cylinder, switchboardObj.transform,
                new Vector3(0.35f, -0.2f, 0.0f), new Vector3(0.12f, 0.4f, 0.6f), matCopperPipe);
            AcademyExpansionAnomalyController gatehouseCtrl = switchboardObj.AddComponent<AcademyExpansionAnomalyController>();
            gatehouseCtrl.Configure(ExpansionNodeType.GatehouseCutSwitchboardSecret);
            EditorUtility.SetDirty(gatehouseCtrl);

            // ZONA 59: Estanque Exterior Helado y Quiosco de Música del Parque (X: 30.0m a 85.0m, Z: 98.0m a 145.0m, Y = 0.0m)
            // Justo al norte del Jardín de Invierno
            GameObject zone59 = new GameObject("59_Frozen_Swan_Pond_And_Park_Gazebo");
            zone59.transform.parent = blockExteriors.transform;

            CreatePrimitive("Park_Ground", PrimitiveType.Cube, zone59.transform,
                new Vector3(57.5f, -0.25f, 121.5f), new Vector3(55.0f, 0.4f, 47.0f), matCobble);

            // Gran estanque helado (espejo exterior del Lago de las Sombras)
            CreatePrimitive("Estanque_Cisnes_Agua_Helada", PrimitiveType.Cube, zone59.transform,
                new Vector3(57.5f, -0.1f, 121.5f), new Vector3(42.0f, 0.15f, 32.0f), matPuddle);
            for (int r = 0; r < 4; r++)
            {
                // r = 0/1 -> lados Este/Oeste (a lo largo de Z); r = 2/3 -> lados Norte/Sur (a lo largo de X, girados 90°)
                bool alongX = r >= 2;
                float side = (r % 2 == 0) ? 1f : -1f;
                Vector3 balPos = alongX
                    ? new Vector3(57.5f, 0.0f, 121.5f + side * 17.0f)
                    : new Vector3(57.5f + side * 22.0f, 0.0f, 121.5f);
                float balLen = alongX ? 42.0f : 32.0f;
                string balName = $"Balustrada_Estanque_{r + 1}";
                CreateArchitecturalBalustrade(balName, zone59.transform, balPos, balLen, matFacade, matStoneWall);
                if (alongX)
                {
                    Transform balT = zone59.transform.Find(balName);
                    if (balT != null) balT.rotation = Quaternion.Euler(0f, 90f, 0f);
                }
            }

            // Quiosco de música / Templete de piedra en el centro del estanque
            GameObject gazeboObj = CreatePrimitive("Templete_Quiosco_Musica_Estanque", PrimitiveType.Cylinder, zone59.transform,
                new Vector3(57.5f, 0.4f, 121.5f), new Vector3(8.5f, 0.8f, 8.5f), matStoneWall);
            for (int c = 0; c < 6; c++)
            {
                float ang = c * 60f * Mathf.Deg2Rad;
                Vector3 colPos = new Vector3(57.5f + Mathf.Cos(ang) * 3.6f, 0.8f, 121.5f + Mathf.Sin(ang) * 3.6f);
                CreateClassicalColumn($"Columna_Quiosco_{c + 1}", zone59.transform, colPos, 4.2f, 0.55f, matFacade);
            }
            CreatePrimitive("Cupula_Quiosco_Musica", PrimitiveType.Sphere, zone59.transform,
                new Vector3(57.5f, 5.4f, 121.5f), new Vector3(8.8f, 2.5f, 8.8f), matFacade);

            // SECRETO ZONA 59: El estuche sumergido en el agua helada con el cilindro de cera original y la carta de Mara
            GameObject pondSecretTrigger = CreatePrimitive("Estuche_Grabacion_Sumergido_Estanque_Z59", PrimitiveType.Cube, zone59.transform,
                new Vector3(57.5f, 0.85f, 118.0f), new Vector3(0.65f, 0.18f, 0.42f), matLeather);
            AcademyExpansionAnomalyController pondCtrl = pondSecretTrigger.AddComponent<AcademyExpansionAnomalyController>();
            pondCtrl.Configure(ExpansionNodeType.FrozenSwanPondGazeboSecret);
            EditorUtility.SetDirty(pondCtrl);
        }

        #region Métodos de Creación PBR, Muebles y Narrativa

        private static Material CreatePBRMaterial(string name, string albedoFile, string normalFile, Vector2 tiling, float smoothness, float metallic)
        {
            string fullPath = $"{MaterialsPath}{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(fullPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, fullPath);
            }
            else
            {
                mat.shader = shader;
            }

            mat.SetColor("_BaseColor", Color.white);
            mat.color = Color.white;

            string albedoPath = $"{TexturesPBRPath}{albedoFile}";
            string normalPath = $"{TexturesPBRPath}{normalFile}";

            Texture2D albedoTex = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
            Texture2D normalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);

            if (albedoTex != null)
            {
                mat.SetTexture("_BaseMap", albedoTex);
                mat.SetTextureScale("_BaseMap", tiling);
            }

            if (normalTex != null)
            {
                mat.SetTexture("_BumpMap", normalTex);
                mat.SetTextureScale("_BumpMap", tiling);
                mat.EnableKeyword("_NORMALMAP");
            }

            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);
            EditorUtility.SetDirty(mat);

            return mat;
        }

        private static Material CreatePortraitMaterial(string name, string texturePath, Material fallbackMat)
        {
            string fullPath = $"{MaterialsPath}{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(fullPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, fullPath);
            }

            mat.SetColor("_BaseColor", Color.white);
            mat.color = Color.white;

            Texture2D portraitTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (portraitTex != null)
            {
                mat.SetTexture("_BaseMap", portraitTex);
            }
            else
            {
                mat.color = new Color(0.25f, 0.15f, 0.18f);
            }

            mat.SetFloat("_Smoothness", 0.45f);
            mat.SetFloat("_Metallic", 0.0f);
            EditorUtility.SetDirty(mat);

            return mat;
        }

        private static void CreateClassicalColumn(string name, Transform parent, Vector3 basePos, float totalHeight, float radius, Material mat)
        {
            GameObject colRoot = new GameObject(name);
            colRoot.transform.parent = parent;
            colRoot.transform.position = basePos;

            float plinthH = 0.5f;
            float capH = 0.5f;
            float shaftH = totalHeight - plinthH - capH;

            CreatePrimitive("Plinto_Base", PrimitiveType.Cube, colRoot.transform,
                new Vector3(0f, plinthH * 0.5f, 0f), new Vector3(radius * 1.5f, plinthH, radius * 1.5f), mat);

            CreatePrimitive("Fuste_Shaft", PrimitiveType.Cylinder, colRoot.transform,
                new Vector3(0f, plinthH + (shaftH * 0.5f), 0f), new Vector3(radius, shaftH * 0.5f, radius), mat);

            CreatePrimitive("Capitel_Top", PrimitiveType.Cube, colRoot.transform,
                new Vector3(0f, totalHeight - (capH * 0.5f), 0f), new Vector3(radius * 1.4f, capH, radius * 1.4f), mat);
        }

        private static void CreateArchitecturalBalustrade(string name, Transform parent, Vector3 centerPos, float length, Material stoneMat, Material baseMat)
        {
            GameObject balRoot = new GameObject(name);
            balRoot.transform.parent = parent;
            balRoot.transform.position = centerPos;

            CreatePrimitive("Balustrade_Base", PrimitiveType.Cube, balRoot.transform,
                new Vector3(0f, 0.8f, 0f), new Vector3(0.5f, 0.4f, length), baseMat);

            CreatePrimitive("Balustrade_Rail", PrimitiveType.Cube, balRoot.transform,
                new Vector3(0f, 1.8f, 0f), new Vector3(0.6f, 0.25f, length), stoneMat);

            CreatePrimitive("Balustrade_Wall", PrimitiveType.Cube, balRoot.transform,
                new Vector3(0f, 1.3f, 0f), new Vector3(0.4f, 1.2f, length), stoneMat);
        }

        private static void CreateWroughtIronGate(string name, Transform parent, Vector3 pos, float width, float height, Material ironMat, bool open)
        {
            GameObject gate = new GameObject(name);
            gate.transform.parent = parent;
            gate.transform.position = pos;
            if (open) gate.transform.rotation = Quaternion.Euler(0f, 35f, 0f);

            CreatePrimitive("Gate_Frame", PrimitiveType.Cube, gate.transform,
                Vector3.zero, new Vector3(width, height, 0.15f), ironMat);

            for (float x = -width * 0.45f; x <= width * 0.45f; x += 0.4f)
            {
                CreatePrimitive($"Spike_{x:F1}", PrimitiveType.Cylinder, gate.transform,
                    new Vector3(x, (height * 0.5f) + 0.25f, 0f), new Vector3(0.08f, 0.25f, 0.08f), ironMat);
            }
        }

        private static void CreateHistoricalPlaque(string name, Transform parent, Vector3 pos, Material goldMat)
        {
            GameObject plaque = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plaque.name = name;
            plaque.transform.parent = parent;
            plaque.transform.position = pos;
            plaque.transform.localScale = new Vector3(0.1f, 0.6f, 0.9f);
            plaque.GetComponent<Renderer>().sharedMaterial = goldMat;
            RemoveCollider(plaque);
        }

        private static void CreateBoiseriePanelSet(string name, Transform parent, Vector3 pos, float width, Material wallMat, Material trimMat)
        {
            GameObject boiserieRoot = new GameObject(name);
            boiserieRoot.transform.parent = parent;
            boiserieRoot.transform.position = pos;

            DecoPiece("Boiserie_Frame", PrimitiveType.Cube, boiserieRoot.transform,
                Vector3.zero, new Vector3(0.08f, 2.2f, width), trimMat);
            DecoPiece("Boiserie_Inner", PrimitiveType.Cube, boiserieRoot.transform,
                new Vector3(0.02f, 0f, 0f), new Vector3(0.06f, 1.9f, width - 0.3f), wallMat);
        }

        private static void CreateFramedArtPortrait(string name, Transform parent, Vector3 pos, Vector3 scale, Material frameMat, Material artMat, string title)
        {
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = name;
            frame.transform.parent = parent;
            frame.transform.position = pos;
            frame.transform.localScale = scale;
            frame.GetComponent<Renderer>().sharedMaterial = frameMat;
            RemoveCollider(frame);

            GameObject canvasObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canvasObj.name = "Oil_Canvas";
            canvasObj.transform.parent = frame.transform;
            if (scale.x > scale.z)
            {
                canvasObj.transform.localPosition = Vector3.zero;
                canvasObj.transform.localScale = new Vector3(0.90f, 0.90f, 1.15f);
            }
            else
            {
                canvasObj.transform.localPosition = Vector3.zero;
                canvasObj.transform.localScale = new Vector3(1.15f, 0.90f, 0.90f);
            }
            canvasObj.GetComponent<Renderer>().sharedMaterial = artMat;
            RemoveCollider(canvasObj);
        }

        private static Light CreatePictureSpotlight(string name, Transform parent, Vector3 pos, Vector3 targetPos, Color color, float intensity)
        {
            GameObject spotObj = new GameObject(name);
            spotObj.transform.parent = parent;
            spotObj.transform.position = pos;
            spotObj.transform.LookAt(targetPos);

            Light l = spotObj.AddComponent<Light>();
            l.type = LightType.Spot;
            l.color = color;
            l.range = 24f;
            l.spotAngle = 45f;
            l.intensity = intensity;
            return l;
        }

        private static void CreatePostProcessVolume()
        {
            GameObject volumeObj = new GameObject("Global_PostProcess_Volume");
            Volume vol = volumeObj.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.priority = 1;

            string profilePath = "Assets/Settings/SampleSceneProfile.asset";
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if (profile != null)
            {
                vol.sharedProfile = profile;
            }
        }

        private static WorldDoubleDoor CreateGrandDoubleDoor(string name, Transform parent, Vector3 basePos, Material woodMat, Material goldMat)
        {
            // Hueco de la fachada: X -2..2 (4 m), altura 4 m, muro de 0.8 m. Abre hacia el vestíbulo.
            GameObject doubleDoorObj = CreateArchitecturalDoubleDoor(name, parent, basePos, Quaternion.Euler(0f, 180f, 0f),
                3.7f, 3.82f, 4.0f, 0.8f, false, "", "", "", woodMat, goldMat, woodMat, true, 95f);
            return doubleDoorObj.GetComponent<WorldDoubleDoor>();
        }

        private static void CreateBallerinaStatue(string name, Transform parent, Vector3 pos, Material statueMat, Material pedestalMat, string statueTitle)
        {
            GameObject statueObj = new GameObject(name);
            statueObj.transform.parent = parent;
            statueObj.transform.position = pos;

            CreatePrimitive("Base_Pedestal", PrimitiveType.Cube, statueObj.transform,
                new Vector3(0f, 0.4f, 0f), new Vector3(1.6f, 0.8f, 1.6f), pedestalMat);
            CreatePrimitive("Fuste_Pedestal", PrimitiveType.Cylinder, statueObj.transform,
                new Vector3(0f, 1.2f, 0f), new Vector3(1.2f, 0.8f, 1.2f), pedestalMat);
            CreatePrimitive("Cornisa_Pedestal", PrimitiveType.Cube, statueObj.transform,
                new Vector3(0f, 2.05f, 0f), new Vector3(1.5f, 0.25f, 1.5f), pedestalMat);

            CreatePrimitive("Statue_Torso", PrimitiveType.Capsule, statueObj.transform,
                new Vector3(0f, 3.0f, 0f), new Vector3(0.45f, 0.7f, 0.45f), statueMat);
            CreatePrimitive("Statue_Head", PrimitiveType.Sphere, statueObj.transform,
                new Vector3(0f, 3.9f, 0f), new Vector3(0.35f, 0.4f, 0.35f), statueMat);
            CreatePrimitive("Statue_Tutu", PrimitiveType.Cylinder, statueObj.transform,
                new Vector3(0f, 2.8f, 0f), new Vector3(1.1f, 0.15f, 1.1f), statueMat);

            GameObject armL = CreatePrimitive("Statue_Arm_L", PrimitiveType.Capsule, statueObj.transform,
                new Vector3(-0.35f, 3.8f, 0f), new Vector3(0.12f, 0.5f, 0.12f), statueMat);
            armL.transform.rotation = Quaternion.Euler(0f, 0f, 35f);

            GameObject armR = CreatePrimitive("Statue_Arm_R", PrimitiveType.Capsule, statueObj.transform,
                new Vector3(0.35f, 3.8f, 0f), new Vector3(0.12f, 0.5f, 0.12f), statueMat);
            armR.transform.rotation = Quaternion.Euler(0f, 0f, -35f);
        }

        private static void CreateReceptionDesk(string name, Transform parent, Vector3 pos, Material woodMat, Material goldMat, Material clothMat, Material lampGlassMat)
        {
            GameObject desk = new GameObject(name);
            desk.transform.parent = parent;
            desk.transform.position = pos;

            CreatePrimitive("Desk_Main", PrimitiveType.Cube, desk.transform,
                new Vector3(0f, 0.6f, 0f), new Vector3(4.8f, 1.2f, 1.4f), woodMat);
            CreatePrimitive("Desk_CounterTop", PrimitiveType.Cube, desk.transform,
                new Vector3(0f, 1.25f, 0f), new Vector3(5.1f, 0.1f, 1.6f), woodMat);

            CreateInteractiveWorldDoc("Libro_Registro_Recepcion", desk.transform,
                new Vector3(-0.8f, 1.35f, 0f), new Vector3(0.55f, 0.08f, 0.42f), goldMat, "doc_vestibule_registry",
                "Libro de Huéspedes — Último Ensayo General",
                "14 de Octubre de 1924\n\n" +
                "El ensayo final de 'El Lago de los Cisnes' dará comienzo a las 20:00 h en el Gran Teatro.\n" +
                "Elena Vesper (Cisne Negro) ha manifestado malestar y se niega a ensayar con el resto del elenco.\n" +
                "Se escucharon gritos cerca de la sala de espejos antes del cierre.\n\n" +
                "[Una nota apresurada añadida al pie con tinta corrida]:\n" +
                "“La música se detuvo. Elena no está en su camerino. Las puertas del teatro han sido cerradas con llave desde adentro.”");

            CreatePrimitive("Campanilla_Recepcion", PrimitiveType.Cylinder, desk.transform,
                new Vector3(0.6f, 1.35f, 0.2f), new Vector3(0.18f, 0.1f, 0.18f), goldMat);

            CreatePrimitive("Lamp_Base", PrimitiveType.Cylinder, desk.transform,
                new Vector3(1.4f, 1.33f, -0.2f), new Vector3(0.2f, 0.05f, 0.2f), goldMat);
            CreatePrimitive("Lamp_Stem", PrimitiveType.Cylinder, desk.transform,
                new Vector3(1.4f, 1.55f, -0.2f), new Vector3(0.04f, 0.22f, 0.04f), goldMat);
            CreatePrimitive("Lamp_Shade", PrimitiveType.Cube, desk.transform,
                new Vector3(1.4f, 1.75f, -0.2f), new Vector3(0.35f, 0.12f, 0.22f), lampGlassMat);

            GameObject lampLightObj = new GameObject("Desk_Light");
            lampLightObj.transform.parent = desk.transform;
            lampLightObj.transform.localPosition = new Vector3(1.4f, 1.70f, -0.2f);
            Light deskLight = lampLightObj.AddComponent<Light>();
            deskLight.type = LightType.Point;
            deskLight.color = new Color(0.9f, 1.0f, 0.75f);
            deskLight.range = 5f;
            deskLight.intensity = 1.8f;
        }

        private static void CreateCoveredFurniture(string name, Transform parent, Vector3 pos, Material clothMat, Material woodMat)
        {
            GameObject furniture = new GameObject(name);
            furniture.transform.parent = parent;
            furniture.transform.position = pos;

            CreatePrimitive("Sofa_Base", PrimitiveType.Cube, furniture.transform,
                new Vector3(0f, 0.45f, 0f), new Vector3(2.4f, 0.9f, 1.1f), clothMat);
            CreatePrimitive("Sofa_Back", PrimitiveType.Cube, furniture.transform,
                new Vector3(0f, 0.95f, 0.45f), new Vector3(2.4f, 1.0f, 0.35f), clothMat);
        }

        // ====================================================================
        // PUERTAS ARQUITECTÓNICAS (marco completo, molduras, hoja con paneles,
        // muro superior hasta el techo y bisagra lateral real)
        // ====================================================================
        private const float DoorJamb = 0.15f;
        private const float DoorLintel = 0.18f;

        private static void RemoveCollider(GameObject go)
        {
            Collider c = go.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);
        }

        private static GameObject DecoPiece(string name, PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject go = CreatePrimitive(name, type, parent, pos, scale, mat);
            RemoveCollider(go);
            return go;
        }

        /// <summary>
        /// Marco de la puerta: jambas, dintel, molduras (arquitrabe) en ambas caras,
        /// cornisa superior y muro macizo desde el dintel hasta el techo.
        /// openingWidth = ancho libre de la hoja(s). El hueco en el muro debe ser openingWidth + 2*DoorJamb.
        /// </summary>
        private static void BuildDoorFrame(Transform root, float openingWidth, float doorHeight, float wallThickness,
            float ceilingTop, bool facade, Material woodMat, Material wallMat)
        {
            float halfW = openingWidth * 0.5f;
            float jambDepth = wallThickness + 0.06f;
            float frameTop = doorHeight + DoorLintel;

            CreatePrimitive("Frame_Jamb_L", PrimitiveType.Cube, root,
                new Vector3(-halfW - DoorJamb * 0.5f, frameTop * 0.5f, 0f), new Vector3(DoorJamb, frameTop, jambDepth), woodMat);
            CreatePrimitive("Frame_Jamb_R", PrimitiveType.Cube, root,
                new Vector3(halfW + DoorJamb * 0.5f, frameTop * 0.5f, 0f), new Vector3(DoorJamb, frameTop, jambDepth), woodMat);
            CreatePrimitive("Frame_Lintel", PrimitiveType.Cube, root,
                new Vector3(0f, doorHeight + DoorLintel * 0.5f, 0f), new Vector3(openingWidth, DoorLintel, jambDepth), woodMat);

            // Muro macizo hasta el techo: nadie puede saltar por encima
            float wallAboveH = ceilingTop - frameTop;
            if (!facade && wallAboveH > 0.02f)
            {
                CreatePrimitive("Wall_Above_Door", PrimitiveType.Cube, root,
                    new Vector3(0f, frameTop + wallAboveH * 0.5f, 0f),
                    new Vector3(openingWidth + DoorJamb * 2f, wallAboveH, wallThickness), wallMat);
            }

            // Molduras clásicas (arquitrabe) en ambas caras del muro
            float casingW = 0.16f;
            float casingT = 0.04f;
            float outerX = halfW + DoorJamb + casingW * 0.5f;
            float casingH = frameTop + casingW;
            int faces = facade ? 1 : 2;
            for (int f = 0; f < faces; f++)
            {
                float side = (f == 0) ? 1f : -1f;
                float z = side * (wallThickness * 0.5f + casingT * 0.5f);
                string s = (f == 0) ? "A" : "B";
                DecoPiece($"Casing_L_{s}", PrimitiveType.Cube, root, new Vector3(-outerX, casingH * 0.5f, z), new Vector3(casingW, casingH, casingT), woodMat);
                DecoPiece($"Casing_R_{s}", PrimitiveType.Cube, root, new Vector3(outerX, casingH * 0.5f, z), new Vector3(casingW, casingH, casingT), woodMat);
                DecoPiece($"Casing_Top_{s}", PrimitiveType.Cube, root, new Vector3(0f, frameTop + casingW * 0.5f, z), new Vector3(outerX * 2f + casingW, casingW, casingT), woodMat);
                // Cornisa superior saliente
                DecoPiece($"Cornice_{s}", PrimitiveType.Cube, root,
                    new Vector3(0f, frameTop + casingW + 0.05f, side * (wallThickness * 0.5f + 0.05f)),
                    new Vector3(outerX * 2f + casingW + 0.14f, 0.10f, 0.10f), woodMat);
                // Zócalos (plinths) en la base de las molduras
                DecoPiece($"Plinth_L_{s}", PrimitiveType.Cube, root, new Vector3(-outerX, 0.12f, side * (wallThickness * 0.5f + 0.03f)), new Vector3(casingW + 0.03f, 0.24f, 0.06f), woodMat);
                DecoPiece($"Plinth_R_{s}", PrimitiveType.Cube, root, new Vector3(outerX, 0.12f, side * (wallThickness * 0.5f + 0.03f)), new Vector3(casingW + 0.03f, 0.24f, 0.06f), woodMat);
            }
        }

        /// <summary>
        /// Hoja de puerta clásica con paneles en relieve, manilla, placa de latón y protector inferior.
        /// handleSide: +1 si la manilla queda a la derecha (local +X) de la hoja, -1 a la izquierda.
        /// </summary>
        private static GameObject BuildPanelledLeaf(string name, Transform hinge, float centerX, float width, float height,
            float handleSide, string label, Material woodMat, Material goldMat)
        {
            float t = 0.07f;
            GameObject leaf = new GameObject(name);
            leaf.transform.parent = hinge;
            leaf.transform.localPosition = new Vector3(centerX, height * 0.5f, 0f);
            leaf.transform.localRotation = Quaternion.identity;
            leaf.transform.localScale = Vector3.one;

            // Cuerpo sólido (único collider: bloquea el paso y recibe la interacción)
            CreatePrimitive("Leaf_Body", PrimitiveType.Cube, leaf.transform, Vector3.zero, new Vector3(width - 0.004f, height - 0.004f, t), woodMat);

            // Paneles en relieve: 2 columnas x 2 filas por cara
            float margin = 0.13f;
            float gap = 0.12f;
            int cols = width > 1.1f ? 2 : 1;
            float panelW = (width - margin * 2f - gap * (cols - 1)) / cols;
            float usableH = height - margin * 2f - gap;
            float lowerH = usableH * 0.38f;
            float upperH = usableH - lowerH;
            float lowerY = -height * 0.5f + margin + lowerH * 0.5f;
            float upperY = -height * 0.5f + margin + lowerH + gap + upperH * 0.5f;

            for (int face = 0; face < 2; face++)
            {
                float side = face == 0 ? 1f : -1f;
                float zPanel = side * (t * 0.5f + 0.012f);
                for (int c = 0; c < cols; c++)
                {
                    float x = -width * 0.5f + margin + panelW * 0.5f + c * (panelW + gap);
                    DecoPiece($"Panel_Low_{face}_{c}", PrimitiveType.Cube, leaf.transform, new Vector3(x, lowerY, zPanel), new Vector3(panelW, lowerH, 0.024f), woodMat);
                    DecoPiece($"Panel_Up_{face}_{c}", PrimitiveType.Cube, leaf.transform, new Vector3(x, upperY, zPanel), new Vector3(panelW, upperH, 0.024f), woodMat);
                }

                // Protector inferior de latón
                DecoPiece($"Kick_Plate_{face}", PrimitiveType.Cube, leaf.transform,
                    new Vector3(0f, -height * 0.5f + 0.09f, side * (t * 0.5f + 0.004f)), new Vector3(width - 0.12f, 0.14f, 0.008f), goldMat);

                // Escudo y pomo de latón (a 1 m del suelo)
                float hx = handleSide * (width * 0.5f - 0.12f);
                float hy = 1.0f - height * 0.5f;
                DecoPiece($"Escutcheon_{face}", PrimitiveType.Cube, leaf.transform,
                    new Vector3(hx, hy, side * (t * 0.5f + 0.006f)), new Vector3(0.07f, 0.24f, 0.012f), goldMat);
                DecoPiece($"Knob_{face}", PrimitiveType.Sphere, leaf.transform,
                    new Vector3(hx, hy + 0.03f, side * (t * 0.5f + 0.045f)), new Vector3(0.075f, 0.075f, 0.075f), goldMat);

                // Placa con el nombre de la sala
                if (!string.IsNullOrEmpty(label))
                {
                    DecoPiece($"Name_Plate_{face}", PrimitiveType.Cube, leaf.transform,
                        new Vector3(0f, 1.65f - height * 0.5f, side * (t * 0.5f + 0.03f)), new Vector3(Mathf.Min(width * 0.55f, 0.6f), 0.13f, 0.01f), goldMat);
                }
            }

            return leaf;
        }

        /// <summary>
        /// Puerta simple completa. pos = centro del hueco a nivel del suelo y en el centro del grosor del muro.
        /// El hueco del muro debe medir doorWidth + 0.30 m. facade = puerta decorativa adosada a un muro macizo.
        /// </summary>
        private static GameObject CreateArchitecturalDoor(
            string name, Transform parent, Vector3 pos, Quaternion rotation,
            float doorWidth, float doorHeight, float ceilingTop, float wallThickness,
            bool isLocked, string requiredKeyId, string label, string lockDesc,
            Material woodMat, Material goldMat, Material wallMat, bool facade = false, float openAngle = 90f)
        {
            GameObject root = new GameObject(name);
            root.transform.parent = parent;
            root.transform.position = pos;
            root.transform.rotation = rotation;

            BuildDoorFrame(root.transform, doorWidth, doorHeight, wallThickness, ceilingTop, facade, woodMat, wallMat);

            GameObject hinge = new GameObject("Door_Hinge");
            hinge.transform.parent = root.transform;
            hinge.transform.localPosition = new Vector3(-doorWidth * 0.5f, 0f, 0f);
            hinge.transform.localRotation = Quaternion.identity;

            BuildPanelledLeaf("Door_Leaf", hinge.transform, doorWidth * 0.5f, doorWidth, doorHeight, 1f, label, woodMat, goldMat);

            WorldDoor door = root.AddComponent<WorldDoor>();
            door.Configure(hinge.transform, openAngle, 0.9f, false, isLocked, requiredKeyId, name, lockDesc);
            SerializedObject serDoor = new SerializedObject(door);
            SerializedProperty pDHinge = serDoor.FindProperty("doorHinge");
            if (pDHinge != null) pDHinge.objectReferenceValue = hinge.transform;
            SerializedProperty pDAngle = serDoor.FindProperty("openAngle");
            if (pDAngle != null) pDAngle.floatValue = openAngle;
            SerializedProperty pDLocked = serDoor.FindProperty("isLocked");
            if (pDLocked != null) pDLocked.boolValue = isLocked;
            SerializedProperty pDKey = serDoor.FindProperty("requiredKeyId");
            if (pDKey != null) pDKey.stringValue = requiredKeyId;
            SerializedProperty pDId = serDoor.FindProperty("id");
            if (pDId != null) pDId.stringValue = name;
            SerializedProperty pDDesc = serDoor.FindProperty("lockedDescription");
            if (pDDesc != null) pDDesc.stringValue = lockDesc;
            serDoor.ApplyModifiedProperties();
            EditorUtility.SetDirty(door);
            return root;
        }

        /// <summary>
        /// Puerta doble completa. El hueco del muro debe medir totalWidth + 0.30 m.
        /// </summary>
        private static GameObject CreateArchitecturalDoubleDoor(
            string name, Transform parent, Vector3 pos, Quaternion rotation,
            float totalWidth, float doorHeight, float ceilingTop, float wallThickness,
            bool isLocked, string requiredKeyId, string label, string lockDesc,
            Material woodMat, Material goldMat, Material wallMat,
            bool startOpen = false, float openAngle = 90f)
        {
            GameObject root = new GameObject(name);
            root.transform.parent = parent;
            root.transform.position = pos;
            root.transform.rotation = rotation;

            BuildDoorFrame(root.transform, totalWidth, doorHeight, wallThickness, ceilingTop, false, woodMat, wallMat);

            float leafW = totalWidth * 0.5f;

            GameObject leftHinge = new GameObject("Left_Hinge");
            leftHinge.transform.parent = root.transform;
            leftHinge.transform.localPosition = new Vector3(-leafW, 0f, 0f);
            leftHinge.transform.localRotation = Quaternion.identity;
            BuildPanelledLeaf("Left_Leaf", leftHinge.transform, leafW * 0.5f, leafW, doorHeight, 1f, "", woodMat, goldMat);

            GameObject rightHinge = new GameObject("Right_Hinge");
            rightHinge.transform.parent = root.transform;
            rightHinge.transform.localPosition = new Vector3(leafW, 0f, 0f);
            rightHinge.transform.localRotation = Quaternion.identity;
            BuildPanelledLeaf("Right_Leaf", rightHinge.transform, -leafW * 0.5f, leafW, doorHeight, -1f, "", woodMat, goldMat);

            if (startOpen)
            {
                leftHinge.transform.localRotation = Quaternion.Euler(0f, openAngle, 0f);
                rightHinge.transform.localRotation = Quaternion.Euler(0f, -openAngle, 0f);
            }

            // Placa de latón con el nombre sobre el dintel (ambas caras)
            if (!string.IsNullOrEmpty(label))
            {
                for (int f = 0; f < 2; f++)
                {
                    float side = f == 0 ? 1f : -1f;
                    DecoPiece($"Lintel_Plate_{f}", PrimitiveType.Cube, root.transform,
                        new Vector3(0f, doorHeight + DoorLintel + 0.45f, side * (wallThickness * 0.5f + 0.015f)),
                        new Vector3(Mathf.Min(totalWidth * 0.55f, 2.0f), 0.28f, 0.02f), goldMat);
                }
            }

            WorldDoubleDoor dd = root.AddComponent<WorldDoubleDoor>();
            dd.Configure(leftHinge.transform, rightHinge.transform, openAngle, 1.1f, startOpen, isLocked, requiredKeyId, name, lockDesc);
            SerializedObject serDD = new SerializedObject(dd);
            SerializedProperty pLHinge = serDD.FindProperty("leftDoorHinge");
            if (pLHinge != null) pLHinge.objectReferenceValue = leftHinge.transform;
            SerializedProperty pRHinge = serDD.FindProperty("rightDoorHinge");
            if (pRHinge != null) pRHinge.objectReferenceValue = rightHinge.transform;
            SerializedProperty pDDLocked = serDD.FindProperty("isLocked");
            if (pDDLocked != null) pDDLocked.boolValue = isLocked;
            SerializedProperty pDDKey = serDD.FindProperty("requiredKeyId");
            if (pDDKey != null) pDDKey.stringValue = requiredKeyId;
            SerializedProperty pDDId = serDD.FindProperty("id");
            if (pDDId != null) pDDId.stringValue = name;
            SerializedProperty pDDDesc = serDD.FindProperty("lockedDescription");
            if (pDDDesc != null) pDDDesc.stringValue = lockDesc;
            serDD.ApplyModifiedProperties();
            EditorUtility.SetDirty(dd);
            return root;
        }

        private static void CreateDoorwayArch(string name, Transform parent, Vector3 pos, string labelText, Material frameMat, Material goldMat, Material drapeMat)
        {
            GameObject arch = new GameObject(name);
            arch.transform.parent = parent;
            arch.transform.position = pos;

            GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = "Arch_Sign";
            sign.transform.parent = arch.transform;
            sign.transform.localPosition = Vector3.zero;
            sign.transform.localScale = new Vector3(0.1f, 0.45f, 2.5f);
            sign.GetComponent<Renderer>().sharedMaterial = goldMat;
            Collider signCol = sign.GetComponent<Collider>();
            if (signCol != null) Object.DestroyImmediate(signCol);

            GameObject drapeL = CreatePrimitive("Drape_L", PrimitiveType.Cube, arch.transform,
                new Vector3(0f, -1.2f, -1.1f), new Vector3(0.3f, 2.4f, 0.4f), drapeMat);
            Collider dlCol = drapeL.GetComponent<Collider>();
            if (dlCol != null) Object.DestroyImmediate(dlCol);

            GameObject drapeR = CreatePrimitive("Drape_R", PrimitiveType.Cube, arch.transform,
                new Vector3(0f, -1.2f, 1.1f), new Vector3(0.3f, 2.4f, 0.4f), drapeMat);
            Collider drCol = drapeR.GetComponent<Collider>();
            if (drCol != null) Object.DestroyImmediate(drCol);
        }

        private static void CreateChandelier(string name, Transform parent, Vector3 pos, Material mat, Color lightColor, float intensity)
        {
            GameObject chandelier = new GameObject(name);
            chandelier.transform.parent = parent;
            chandelier.transform.position = pos;

            CreatePrimitive("Chandelier_Chain", PrimitiveType.Cylinder, chandelier.transform,
                new Vector3(0f, 0.8f, 0f), new Vector3(0.1f, 0.8f, 0.1f), mat);
            CreatePrimitive("Chandelier_Ring_Upper", PrimitiveType.Cylinder, chandelier.transform,
                new Vector3(0f, 0.2f, 0f), new Vector3(1.6f, 0.15f, 1.6f), mat);
            CreatePrimitive("Chandelier_Ring_Lower", PrimitiveType.Cylinder, chandelier.transform,
                new Vector3(0f, -0.4f, 0f), new Vector3(2.4f, 0.15f, 2.4f), mat);

            GameObject lightObj = new GameObject("Light_Chandelier");
            lightObj.transform.parent = chandelier.transform;
            lightObj.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = lightColor;
            l.range = 24f;
            l.intensity = intensity;
            l.shadows = LightShadows.Soft;
        }

        private static void CreateOrnateSconce(string name, Transform parent, Vector3 pos, Material goldMat, Material glowMat, Color color, float intensity)
        {
            GameObject sconce = new GameObject(name);
            sconce.transform.parent = parent;
            sconce.transform.position = pos;

            CreatePrimitive("Sconce_Bracket", PrimitiveType.Cube, sconce.transform,
                Vector3.zero, new Vector3(0.15f, 0.35f, 0.25f), goldMat);

            CreatePrimitive("Sconce_Bulb", PrimitiveType.Sphere, sconce.transform,
                new Vector3(0f, 0.1f, 0.2f), new Vector3(0.15f, 0.2f, 0.15f), glowMat);

            GameObject lightObj = new GameObject("Sconce_Light");
            lightObj.transform.parent = sconce.transform;
            lightObj.transform.localPosition = new Vector3(0f, 0.1f, 0.25f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.range = 10f;
            l.intensity = intensity;
            l.shadows = LightShadows.None;
        }

        private static void CreateStandingLamp(string name, Transform parent, Vector3 pos, Material mat, Material glowMat, Color color, float intensity)
        {
            GameObject lamp = new GameObject(name);
            lamp.transform.parent = parent;
            lamp.transform.position = pos;

            CreatePrimitive("Lamp_Pole", PrimitiveType.Cylinder, lamp.transform,
                new Vector3(0f, 1.8f, 0f), new Vector3(0.18f, 1.8f, 0.18f), mat);
            CreatePrimitive("Lamp_Head", PrimitiveType.Cube, lamp.transform,
                new Vector3(0f, 3.8f, 0f), new Vector3(0.6f, 0.6f, 0.6f), mat);
            CreatePrimitive("Lamp_Glass", PrimitiveType.Sphere, lamp.transform,
                new Vector3(0f, 3.8f, 0f), new Vector3(0.35f, 0.35f, 0.35f), glowMat);

            GameObject lightObj = new GameObject("Lamp_Light");
            lightObj.transform.parent = lamp.transform;
            lightObj.transform.localPosition = new Vector3(0f, 3.8f, 0f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.range = 12f;
            l.intensity = intensity;
            l.shadows = LightShadows.Soft;
        }

        private static void CreateOrnateLantern(string name, Transform parent, Vector3 pos, Material mat, Material glowMat, Color color, float intensity)
        {
            GameObject lantern = new GameObject(name);
            lantern.transform.parent = parent;
            lantern.transform.position = pos;

            CreatePrimitive("Lantern_Cage", PrimitiveType.Cube, lantern.transform,
                Vector3.zero, new Vector3(0.5f, 0.7f, 0.5f), mat);
            CreatePrimitive("Lantern_Core", PrimitiveType.Sphere, lantern.transform,
                Vector3.zero, new Vector3(0.25f, 0.35f, 0.25f), glowMat);

            GameObject lObj = new GameObject("Lantern_Light");
            lObj.transform.parent = lantern.transform;
            lObj.transform.localPosition = Vector3.zero;
            Light l = lObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.range = 12f;
            l.intensity = intensity;
            l.shadows = LightShadows.Soft;
        }

        private static void CreateVintagePhone(string name, Transform parent, Vector3 pos, Material bodyMat, Material goldMat)
        {
            GameObject phone = new GameObject(name);
            phone.transform.parent = parent;
            phone.transform.position = pos;

            CreatePrimitive("Phone_Base", PrimitiveType.Cylinder, phone.transform,
                new Vector3(0f, 0.05f, 0f), new Vector3(0.25f, 0.05f, 0.25f), bodyMat);
            CreatePrimitive("Phone_Stem", PrimitiveType.Cylinder, phone.transform,
                new Vector3(0f, 0.22f, 0f), new Vector3(0.06f, 0.20f, 0.06f), goldMat);
            CreatePrimitive("Phone_Mouthpiece", PrimitiveType.Cylinder, phone.transform,
                new Vector3(0f, 0.38f, 0.08f), new Vector3(0.12f, 0.06f, 0.12f), goldMat);
            CreatePrimitive("Phone_Earpiece", PrimitiveType.Cylinder, phone.transform,
                new Vector3(-0.12f, 0.25f, 0f), new Vector3(0.08f, 0.15f, 0.08f), bodyMat);
        }

        private static void CreateWallClock(string name, Transform parent, Vector3 pos, Material woodMat, Material goldMat)
        {
            GameObject clock = new GameObject(name);
            clock.transform.parent = parent;
            clock.transform.position = pos;

            CreatePrimitive("Clock_Frame", PrimitiveType.Cylinder, clock.transform,
                Vector3.zero, new Vector3(0.85f, 0.08f, 0.85f), woodMat).transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            CreatePrimitive("Clock_Face", PrimitiveType.Cylinder, clock.transform,
                new Vector3(0f, 0f, -0.05f), new Vector3(0.70f, 0.04f, 0.70f), goldMat).transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            CreatePrimitive("Clock_Hands", PrimitiveType.Cube, clock.transform,
                new Vector3(0.08f, 0.06f, -0.08f), new Vector3(0.04f, 0.25f, 0.02f), woodMat).transform.rotation = Quaternion.Euler(0f, 0f, 45f);
        }

        private static void CreateInteractiveWorldDoc(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, string docId, string docTitle, string docContent)
        {
            GameObject docObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            docObj.name = name;
            docObj.transform.parent = parent;
            docObj.transform.localPosition = pos;
            docObj.transform.localScale = scale;
            if (mat != null) docObj.GetComponent<Renderer>().sharedMaterial = mat;

            WorldDocument doc = docObj.AddComponent<WorldDocument>();
            SerializedObject ser = new SerializedObject(doc);
            SerializedProperty pId = ser.FindProperty("id");
            if (pId != null) pId.stringValue = docId;
            SerializedProperty pTitle = ser.FindProperty("title");
            if (pTitle != null) pTitle.stringValue = docTitle;
            SerializedProperty pContent = ser.FindProperty("content");
            if (pContent != null) pContent.stringValue = docContent;
            ser.ApplyModifiedProperties();
        }

        private static void CreateDocumentViewerModal(GameObject canvasObj)
        {
            GameObject docPanel = new GameObject("DocumentViewer_Panel");
            docPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform panelRect = docPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image bg = docPanel.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.04f, 0.06f, 0.94f);

            GameObject card = new GameObject("Card");
            card.transform.SetParent(docPanel.transform, false);
            RectTransform cardRect = card.AddComponent<RectTransform>();
            cardRect.anchoredPosition = Vector2.zero;
            cardRect.sizeDelta = new Vector2(860f, 620f);
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.12f, 0.11f, 0.12f, 0.98f);

            GameObject border = new GameObject("Gold_Card_Border");
            border.transform.SetParent(card.transform, false);
            RectTransform bRect = border.AddComponent<RectTransform>();
            bRect.anchorMin = Vector2.zero;
            bRect.anchorMax = Vector2.one;
            bRect.offsetMin = new Vector2(-2f, -2f);
            bRect.offsetMax = new Vector2(2f, 2f);
            Image bImg = border.AddComponent<Image>();
            bImg.color = new Color(0.85f, 0.72f, 0.44f, 0.75f);
            bImg.raycastTarget = false;
            border.transform.SetAsFirstSibling();

            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(card.transform, false);
            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.fontSize = 28;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = new Color(0.95f, 0.85f, 0.55f);
            RectTransform tRect = titleObj.GetComponent<RectTransform>();
            tRect.anchoredPosition = new Vector2(0f, 250f);
            tRect.sizeDelta = new Vector2(800f, 45f);

            GameObject contentObj = new GameObject("ContentText");
            contentObj.transform.SetParent(card.transform, false);
            TextMeshProUGUI contentTMP = contentObj.AddComponent<TextMeshProUGUI>();
            contentTMP.fontSize = 20;
            contentTMP.lineSpacing = 16f;
            contentTMP.alignment = TextAlignmentOptions.TopLeft;
            contentTMP.color = new Color(0.90f, 0.88f, 0.84f);
            RectTransform cRect = contentObj.GetComponent<RectTransform>();
            cRect.anchoredPosition = new Vector2(0f, -20f);
            cRect.sizeDelta = new Vector2(760f, 440f);

            GameObject hintObj = new GameObject("CloseHintText");
            hintObj.transform.SetParent(card.transform, false);
            TextMeshProUGUI hintTMP = hintObj.AddComponent<TextMeshProUGUI>();
            hintTMP.text = "[E] o [Escape] para cerrar documento";
            hintTMP.fontSize = 17;
            hintTMP.alignment = TextAlignmentOptions.Center;
            hintTMP.color = new Color(0.70f, 0.65f, 0.55f);
            RectTransform hRect = hintObj.GetComponent<RectTransform>();
            hRect.anchoredPosition = new Vector2(0f, -270f);
            hRect.sizeDelta = new Vector2(800f, 35f);

            DocumentUI docUI = canvasObj.AddComponent<DocumentUI>();
            SerializedObject serUI = new SerializedObject(docUI);
            SerializedProperty pPanel = serUI.FindProperty("documentPanel");
            if (pPanel != null) pPanel.objectReferenceValue = docPanel;
            SerializedProperty pTitle = serUI.FindProperty("titleText");
            if (pTitle != null) pTitle.objectReferenceValue = titleTMP;
            SerializedProperty pBody = serUI.FindProperty("bodyText");
            if (pBody != null) pBody.objectReferenceValue = contentTMP;
            SerializedProperty pHint = serUI.FindProperty("closeHintText");
            if (pHint != null) pHint.objectReferenceValue = hintTMP;
            serUI.ApplyModifiedProperties();
            EditorUtility.SetDirty(docUI);

            docPanel.SetActive(false);
        }

        private static GameObject CreatePrimitive(string name, PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.parent = parent;
            obj.transform.localPosition = pos;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = scale;
            if (mat != null) obj.GetComponent<Renderer>().sharedMaterial = mat;
            return obj;
        }

        private static GameObject CreateWall(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            return CreatePrimitive(name, PrimitiveType.Cube, parent, pos, scale, mat);
        }

        private static Material GetOrCreateBasicMaterial(string name, Color color, float smoothness = 0.5f, float metallic = 0f)
        {
            string fullPath = $"{MaterialsPath}{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(fullPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            if (mat == null)
            {
                mat = new Material(shader);
                mat.color = color;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
                AssetDatabase.CreateAsset(mat, fullPath);
            }
            else
            {
                mat.color = color;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            }
            return mat;
        }

        private static Material GetOrCreateEmissiveMaterial(string name, Color emitColor)
        {
            string fullPath = $"{MaterialsPath}{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(fullPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, fullPath);
            }

            mat.SetColor("_BaseColor", emitColor);
            mat.SetColor("_EmissionColor", emitColor * 2.5f);
            mat.EnableKeyword("_EMISSION");
            EditorUtility.SetDirty(mat);
            return mat;
        }

        #endregion
    }
}
#endif
