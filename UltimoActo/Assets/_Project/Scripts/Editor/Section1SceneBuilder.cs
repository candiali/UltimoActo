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
        private const string AutoBuildSessionKey = "UltimoActo_AutoBuild_Section1_2_3_V12";

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
                Debug.Log("<color=cyan>[Último Acto]</color> Construyendo automáticamente Mapa Completo (Secciones 1, 2 y 3)...");
                BuildFullAcademyMap();
            }
        }

        [MenuItem("Tools/Último Acto/1. Construir Mapa Completo (Sección 1 + Sección 2 + Sección 3 en SampleScene)")]
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
                // Cubre desde X = -60m hasta X = +60m, y Z = -60m hasta +130m
                CreatePrimitive("Bedrock_Safety_Ground", PrimitiveType.Cube, rootEnv.transform,
                    new Vector3(0f, -0.7f, 35f), new Vector3(120f, 1.0f, 190f), matCobble);

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
                CreateWall("Patio_Wall_R", zone2.transform, new Vector3(14f, 3f, -15.5f), new Vector3(1f, 6f, 21f), matFacade);

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

                // Muros del pasillo principal: con apertura ornamental en Z = 48m conectando con el Pasillo de los Retratos
                CreateWall("Pasillo_Wall_West_South", zone5.transform, new Vector3(-4f, hallY + (hallHeight * 0.5f), 39f), new Vector3(0.5f, hallHeight, 14f), matWallIvory);
                CreateWall("Pasillo_Wall_West_North", zone5.transform, new Vector3(-4f, hallY + (hallHeight * 0.5f), 64f), new Vector3(0.5f, hallHeight, 28f), matWallIvory);
                // Puerta doble hacia el Pasillo de los Retratos (hueco Z 46..50)
                CreateArchitecturalDoubleDoor("Doors_To_PortraitGallery", zone5.transform, new Vector3(-4f, hallY, 48f), Quaternion.Euler(0f, 90f, 0f), 3.7f, 3.6f, hallHeight, 0.5f, false, "", "PASILLO DE LOS RETRATOS", "", matMahogany, matGold, matWallIvory);

                // Muro este del pasillo principal: con puerta funcional en Z = 44m hacia la Sección 3 (Tocadores y Vestuarios)
                CreateWall("Pasillo_Wall_East_South", zone5.transform, new Vector3(4f, hallY + (hallHeight * 0.5f), 37.475f), new Vector3(0.5f, hallHeight, 10.95f), matWallIvory);
                CreateWall("Pasillo_Wall_East_North", zone5.transform, new Vector3(4f, hallY + (hallHeight * 0.5f), 61.525f), new Vector3(0.5f, hallHeight, 32.95f), matWallIvory);

                CreatePrimitive("Zocalo_Hall_W1", PrimitiveType.Cube, zone5.transform, new Vector3(-3.72f, hallY + 0.35f, 39f), new Vector3(0.1f, 0.7f, 14f), matMahogany);
                CreatePrimitive("Zocalo_Hall_W2", PrimitiveType.Cube, zone5.transform, new Vector3(-3.72f, hallY + 0.35f, 64f), new Vector3(0.1f, 0.7f, 28f), matMahogany);
                CreatePrimitive("Zocalo_Hall_E1", PrimitiveType.Cube, zone5.transform, new Vector3(3.72f, hallY + 0.35f, 37.4f), new Vector3(0.1f, 0.7f, 10.8f), matMahogany);
                CreatePrimitive("Zocalo_Hall_E2", PrimitiveType.Cube, zone5.transform, new Vector3(3.72f, hallY + 0.35f, 61.5f), new Vector3(0.1f, 0.7f, 32.8f), matMahogany);

                // Posiciones de las puertas de las salas (para no superponer paneles ni apliques)
                float[] hallDoorsW = { 40f, 48f, 56f };
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

                // Puerta real hacia Sección 3 (Tocadores y Vestuarios) en Z = 44m, y puertas futuras adosadas al muro
                CreateArchitecturalDoor("Door_Rehearsal_1", zone5.transform, new Vector3(-3.69f, hallY, 40f), Quaternion.Euler(0f, 90f, 0f), 1.6f, 3.0f, 0f, 0.12f, true, "", "SALA 1: ENSAYO PRINCIPAL", "Sala 1: Ensayo Principal — Cerrada con llave", matMahogany, matGold, matWallIvory, true);
                CreateArchitecturalDoor("Door_Costumes", zone5.transform, new Vector3(4f, hallY, 44f), Quaternion.Euler(0f, -90f, 0f), 1.8f, 3.2f, hallHeight, 0.5f, false, "", "TOCADORES Y VESTUARIOS", "", matMahogany, matGold, matWallIvory, false);
                CreateArchitecturalDoor("Door_Rehearsal_2", zone5.transform, new Vector3(-3.69f, hallY, 56f), Quaternion.Euler(0f, 90f, 0f), 1.6f, 3.0f, 0f, 0.12f, true, "", "SALA 2: COREOGRAFÍA", "Sala 2: Coreografía — Cerrada con llave", matMahogany, matGold, matWallIvory, true);
                CreateArchitecturalDoor("Door_Music_Archive", zone5.transform, new Vector3(3.69f, hallY, 62f), Quaternion.Euler(0f, -90f, 0f), 1.6f, 3.0f, 0f, 0.12f, true, "", "ARCHIVO MUSICAL", "Archivo Musical — Cerrado con llave", matMahogany, matGold, matWallIvory, true);

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

                CreateWall("Rec_Wall_South", zone6.transform, new Vector3(-17.5f, vestY + 2.5f, 12f), new Vector3(13f, 5.0f, 0.5f), matWallIvory);

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

                // Pared oeste: ventanal monumental con luz de luna y cortinas borgoña
                CreateWall("Office_Wall_West_S", zone7.transform, new Vector3(-38f, vestY + 2.5f, 14.5f), new Vector3(0.5f, 5.0f, 5f), matWallIvory);
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

                // Muros del pasillo de los retratos
                CreateWall("Portraits_Wall_West", zone10.transform, new Vector3(-20f, vestY + 2.5f, 43f), new Vector3(0.5f, 5.0f, 14f), matWallIvory);
                CreateWall("Portraits_Wall_North_1", zone10.transform, new Vector3(-17.5f, vestY + 2.5f, 50f), new Vector3(5f, 5.0f, 0.5f), matWallIvory);
                CreateWall("Portraits_Wall_North_2", zone10.transform, new Vector3(-9.5f, vestY + 2.5f, 50f), new Vector3(11f, 5.0f, 0.5f), matWallIvory);
                CreateWall("Portraits_Wall_South_Connector", zone10.transform, new Vector3(-9.5f, vestY + 2.5f, 46f), new Vector3(11f, 5.0f, 0.5f), matWallIvory);
                CreateWall("Portraits_Wall_East_S", zone10.transform, new Vector3(-15f, vestY + 2.5f, 41f), new Vector3(0.5f, 5.0f, 10f), matWallIvory);

                // Galería de retratos de bailarinas y directoras
                for (int p = 0; p < 4; p++)
                {
                    float pZ = 38f + (p * 3.5f);
                    CreateFramedArtPortrait($"Retrato_Galeria_{p + 1}", zone10.transform,
                        new Vector3(-19.7f, vestY + 2.6f, pZ), new Vector3(0.12f, 1.8f, 1.3f), matGold, matPortraitElena,
                        $"Retrato Histórico: Primera Bailarina {1905 + (p * 5)}");

                    CreateOrnateSconce($"Sconce_Galeria_{p + 1}", zone10.transform,
                        new Vector3(-19.7f, vestY + 3.0f, pZ + 1.2f), matGold, matLanternGlow, new Color(1f, 0.82f, 0.50f), 2.2f);
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

                // ====================================================================
                // GUARDAR Y FINALIZAR
                // ====================================================================
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

                Debug.Log($"<color=green>[Último Acto]</color> ¡MAPA COMPLETO (Secciones 1, 2 y 3: Vestuarios y Habitaciones de Alumnas) construido con éxito en {ScenePath}!");
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
            // Muro este de Tocadores (Z = 32 a 46)
            CreateWall("Vanity_Wall_East", zone12.transform, new Vector3(27f, vestY + 2.5f, 39f), new Vector3(0.5f, roomH, 14f), matWallIvory);

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
            CreateVanityStation("Tocador_3_Este_Elena_Anomalia", zone12.transform, new Vector3(25.8f, vestY, 35.0f), Quaternion.Euler(0f, -90f, 0f),
                matMahogany, matGold, matMirrorGlass, matVelvetDrapes, matLanternGlow, matPorcelain, matShadowFigure, true, true);
            CreateVanityStation("Tocador_4_Este", zone12.transform, new Vector3(25.8f, vestY, 39.2f), Quaternion.Euler(0f, -90f, 0f),
                matMahogany, matGold, matMirrorGlass, matVelvetDrapes, matLanternGlow, matPorcelain, matShadowFigure, false, false);
            CreateVanityStation("Tocador_5_Este", zone12.transform, new Vector3(25.8f, vestY, 43.2f), Quaternion.Euler(0f, -90f, 0f),
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

            // Muros perimetrales de la sala de taquillas
            CreateWall("Lockers_Wall_West_N", zone13.transform, new Vector3(27f, vestY + 2.5f, 34f), new Vector3(0.5f, roomH, 4f), matWallIvory);
            CreateWall("Lockers_Wall_South", zone13.transform, new Vector3(33f, vestY + 2.5f, 12f), new Vector3(12f, roomH, 0.5f), matWallIvory);
            CreateWall("Lockers_Wall_East", zone13.transform, new Vector3(39f, vestY + 2.5f, 24f), new Vector3(0.5f, roomH, 24f), matWallIvory);
            CreateWall("Lockers_Wall_North", zone13.transform, new Vector3(33f, vestY + 2.5f, 36f), new Vector3(12f, roomH, 0.5f), matWallIvory);

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

            // Muro sur de la habitación (Z = 52) con Puerta Cerrada con Llave (key_elena_room) en X = 19m
            CreateWall("Elena_Wall_South_W", zone14.transform, new Vector3(14.975f, vestY + 2.5f, 52f), new Vector3(5.95f, roomH, 0.5f), matWallIvory);
            CreateWall("Elena_Wall_South_E", zone14.transform, new Vector3(23.025f, vestY + 2.5f, 52f), new Vector3(5.95f, roomH, 0.5f), matWallIvory);
            CreateArchitecturalDoor("Door_To_Elena_Room", zone14.transform, new Vector3(19f, vestY, 52f), Quaternion.identity,
                1.8f, 3.2f, roomH, 0.5f, true, "key_elena_room", "HABITACIÓN DE ELENA VESPER",
                "Habitación Privada de Elena Vesper — Cerrada con llave (Buscar en Taquilla Nº 14)", matMahogany, matGold, matWallIvory);

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

            // Fotografías de ensayos e imagen de Elena con el vestuario de Cisne Negro en la pared
            CreateFramedArtPortrait("Retrato_Elena_CisneNegro_Hab", zone14.transform,
                new Vector3(25.68f, vestY + 2.6f, 61.5f), new Vector3(0.12f, 2.1f, 1.6f), matGold, matPortraitElena,
                "Retrato Personal: Elena Vesper con el Traje del Cisne Negro");
            CreateFramedArtPortrait("Foto_Ensayo_Hab_Elena", zone14.transform,
                new Vector3(19f, vestY + 2.5f, 52.32f), new Vector3(1.5f, 1.2f, 0.10f), matGold, matPhotoTroupe1923,
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

            // Muros perimetrales de lavabos y duchas
            CreateWall("Wash_Wall_West", zone15.transform, new Vector3(11f, vestY + 2.5f, 6f), new Vector3(0.5f, roomH, 12f), matWallIvory);
            CreateWall("Wash_Wall_South", zone15.transform, new Vector3(19f, vestY + 2.5f, 0f), new Vector3(16f, roomH, 0.5f), matWallIvory);
            CreateWall("Wash_Wall_East", zone15.transform, new Vector3(27f, vestY + 2.5f, 6f), new Vector3(0.5f, roomH, 12f), matWallIvory);

            // Revestimiento parcial de azulejos marfil con juntas oscurecidas por la humedad
            DecoPiece("Wash_Tile_Wainscot_W", PrimitiveType.Cube, zone15.transform, new Vector3(11.28f, vestY + 1.2f, 6f), new Vector3(0.06f, 2.4f, 11.4f), matCeramicTile);
            DecoPiece("Wash_Tile_Wainscot_E", PrimitiveType.Cube, zone15.transform, new Vector3(26.72f, vestY + 1.2f, 6f), new Vector3(0.06f, 2.4f, 11.4f), matCeramicTile);
            DecoPiece("Wash_Tile_Wainscot_S", PrimitiveType.Cube, zone15.transform, new Vector3(19f, vestY + 1.2f, 0.28f), new Vector3(15.4f, 2.4f, 0.06f), matCeramicTile);

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
        }

        private static void CreateBoiseriePanelSet(string name, Transform parent, Vector3 pos, float width, Material wallMat, Material trimMat)
        {
            GameObject boiserieRoot = new GameObject(name);
            boiserieRoot.transform.parent = parent;
            boiserieRoot.transform.position = pos;

            CreatePrimitive("Boiserie_Frame", PrimitiveType.Cube, boiserieRoot.transform,
                Vector3.zero, new Vector3(0.08f, 2.2f, width), trimMat);
            CreatePrimitive("Boiserie_Inner", PrimitiveType.Cube, boiserieRoot.transform,
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

            GameObject canvasObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canvasObj.name = "Oil_Canvas";
            canvasObj.transform.parent = frame.transform;
            canvasObj.transform.localPosition = new Vector3(scale.x > 0.5f ? 0f : 0.6f, 0f, 0f);
            canvasObj.transform.localScale = new Vector3(0.1f, 0.90f, 0.90f);
            canvasObj.GetComponent<Renderer>().sharedMaterial = artMat;
        }

        private static void CreatePictureSpotlight(string name, Transform parent, Vector3 pos, Vector3 targetPos, Color color, float intensity)
        {
            GameObject spotObj = new GameObject(name);
            spotObj.transform.parent = parent;
            spotObj.transform.position = pos;
            spotObj.transform.LookAt(targetPos);

            Light l = spotObj.AddComponent<Light>();
            l.type = LightType.Spot;
            l.color = color;
            l.range = 10f;
            l.spotAngle = 45f;
            l.intensity = intensity;
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
            Material woodMat, Material goldMat, Material wallMat, bool facade = false)
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
            door.Configure(hinge.transform, 90f, 0.9f, false, isLocked, requiredKeyId, name, lockDesc);
            SerializedObject serDoor = new SerializedObject(door);
            SerializedProperty pDHinge = serDoor.FindProperty("doorHinge");
            if (pDHinge != null) pDHinge.objectReferenceValue = hinge.transform;
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
