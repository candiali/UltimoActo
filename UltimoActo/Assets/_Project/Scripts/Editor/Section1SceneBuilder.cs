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
    /// Generador arquitectónico integral de la SECCIÓN 1: Exterior y Entrada en SampleScene.unity.
    /// Reconstruye los 5 espacios físicamente conectados con alta fidelidad gótica-clásica:
    /// 1. Portón exterior (reja de hierro forjado, pilares de piedra, enredaderas, faroles)
    /// 2. Patio de entrada (adoquines, jardineras descuidadas, estatuas de bailarinas, charcos lunares)
    /// 3. Escalinata monumental y pórtico (12 peldaños con alfombra vino, rampa física invisible suave, columnas jónicas)
    /// 4. Vestíbulo de recepción (suelo de mármol veneciano con cenefa, boiserie, lámpara de araña, mostrador, retrato al óleo de Elena Vesper)
    /// 5. Pasillo principal (parqué envejecido en espiga, alfombra continua, apliques dorados, galería de retratos, puertas numeradas)
    /// </summary>
    [InitializeOnLoad]
    public static class Section1SceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";
        private const string MaterialsPath = "Assets/_Project/Materials/";
        private const string TexturesPBRPath = "Assets/_Project/Textures/PBR/";
        private const string ElenaPortraitPath = "Assets/_Project/Textures/Portraits/Elena_Vesper_Portrait.jpg";
        private const string AutoBuildSessionKey = "UltimoActo_AutoBuild_Section1_V7";

        static Section1SceneBuilder()
        {
            // Auto-ejecución inmediata al recargar el dominio de Unity para que el usuario no tenga que buscar el menú
            EditorApplication.delayCall += CheckAndAutoBuild;
        }

        private static void CheckAndAutoBuild()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // Si el editor está en Play mode, esperar a que el usuario salga de Play mode
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
                Debug.Log("<color=cyan>[Último Acto]</color> Reconstruyendo automáticamente Sección 1 con físicas y texturas PBR...");
                BuildSection1Scene();
            }
        }

        [MenuItem("Tools/Último Acto/3. Construir Sección 1 (Exterior y Entrada en SampleScene)")]
        public static void BuildSection1Scene()
        {
            try
            {
                // 1. Asegurar generación de texturas PBR en disco
                ProceduralTextureGenerator.GenerateAllTextures();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            if (!AssetDatabase.IsValidFolder("Assets/_Project/Materials"))
                AssetDatabase.CreateFolder("Assets/_Project", "Materials");
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            // ==========================================
            // 2. MATERIALES PBR VIBRANTES Y DETALLADOS
            // ==========================================
            Material matCobble = CreatePBRMaterial("Mat_S1_Cobblestone", "Tex_Cobble_Albedo.png", "Tex_Cobble_Normal.png", new Vector2(10f, 10f), 0.50f, 0.05f);
            Material matMarble = CreatePBRMaterial("Mat_S1_MarbleFloor", "Tex_Marble_Albedo.png", "Tex_Marble_Normal.png", new Vector2(6f, 6f), 0.88f, 0.15f);
            Material matParquet = CreatePBRMaterial("Mat_S1_Parquet", "Tex_Parquet_Albedo.png", "Tex_Parquet_Normal.png", new Vector2(4f, 16f), 0.70f, 0.05f);
            Material matCarpet = CreatePBRMaterial("Mat_S1_VelvetCarpet", "Tex_VelvetCarpet_Albedo.png", "Tex_VelvetCarpet_Normal.png", new Vector2(1f, 12f), 0.20f, 0.02f);
            Material matWallIvory = CreatePBRMaterial("Mat_S1_WallIvory", "Tex_WallIvory_Albedo.png", "Tex_WallIvory_Normal.png", new Vector2(4f, 2f), 0.25f, 0.02f);
            Material matFacade = CreatePBRMaterial("Mat_S1_Facade", "Tex_Facade_Albedo.png", "Tex_Facade_Normal.png", new Vector2(8f, 5f), 0.35f, 0.05f);
            Material matMahogany = CreatePBRMaterial("Mat_S1_Mahogany", "Tex_Mahogany_Albedo.png", "Tex_Mahogany_Normal.png", new Vector2(2f, 4f), 0.75f, 0.05f);

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
            Material matGreenGlass = GetOrCreateBasicMaterial("Mat_S1_BankerGlass", new Color(0.05f, 0.35f, 0.15f), 0.90f, 0.1f);

            // Retrato histórico al óleo de Elena Vesper
            Material matPortraitElena = CreatePortraitMaterial("Mat_S1_Portrait_Elena", ElenaPortraitPath, matMahogany);

            // ==========================================
            // 3. AMBIENTE, NIEBLA Y POST-PROCESADO
            // ==========================================
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.09f, 0.12f, 0.18f);
            RenderSettings.ambientEquatorColor = new Color(0.06f, 0.07f, 0.10f);
            RenderSettings.ambientGroundColor = new Color(0.02f, 0.02f, 0.03f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.007f;
            RenderSettings.fogColor = new Color(0.06f, 0.08f, 0.13f);

            // Luz de Luna fría azulada con sombras suaves
            GameObject moonObj = new GameObject("Moonlight_Cold");
            Light moonLight = moonObj.AddComponent<Light>();
            moonLight.type = LightType.Directional;
            moonLight.color = new Color(0.68f, 0.80f, 0.98f);
            moonLight.intensity = 0.85f;
            moonLight.shadows = LightShadows.Soft;
            moonObj.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

            CreatePostProcessVolume();

            GameObject rootEnv = new GameObject("Section1_Exterior_And_Entrance");

            // ====================================================================
            // 0. CIMENTACIÓN ANTICAÍDAS (SEGURIDAD ABSOLUTA CONTRA CAÍDAS AL VACÍO)
            // ====================================================================
            // Base sólida masiva que cubre toda la longitud y anchura del mapa
            CreatePrimitive("Bedrock_Safety_Ground", PrimitiveType.Cube, rootEnv.transform,
                new Vector3(0f, -0.7f, 25f), new Vector3(70f, 1.0f, 160f), matCobble);

            // ====================================================================
            // ZONA 1: PORTÓN EXTERIOR (Z: -38 a -26, Ancho: 28m)
            // ====================================================================
            GameObject zone1 = new GameObject("1_Exterior_Gate_Area");
            zone1.transform.parent = rootEnv.transform;

            CreatePrimitive("Exterior_Ground", PrimitiveType.Cube, zone1.transform,
                new Vector3(0f, -0.25f, -32f), new Vector3(28f, 0.5f, 12f), matCobble);

            CreateWall("Muro_Ext_Left", zone1.transform, new Vector3(-10f, 2.5f, -38f), new Vector3(10f, 5f, 1f), matStoneWall);
            CreateWall("Muro_Ext_Right", zone1.transform, new Vector3(10f, 2.5f, -38f), new Vector3(10f, 5f, 1f), matStoneWall);

            // Pilares de piedra monumentales en la entrada
            CreateClassicalColumn("Pilar_Porton_L", zone1.transform, new Vector3(-4.5f, 0f, -38f), 5.5f, 1.4f, matFacade);
            CreateClassicalColumn("Pilar_Porton_R", zone1.transform, new Vector3(4.5f, 0f, -38f), 5.5f, 1.4f, matFacade);

            CreateOrnateLantern("Farol_Porton_L", zone1.transform, new Vector3(-4.5f, 5.8f, -38f), matIron, matLanternGlow, new Color(1.0f, 0.82f, 0.50f), 2.8f);
            CreateOrnateLantern("Farol_Porton_R", zone1.transform, new Vector3(4.5f, 5.8f, -38f), matIron, matLanternGlow, new Color(1.0f, 0.82f, 0.50f), 2.8f);

            // Placa histórica de bronce en el pilar
            CreateHistoricalPlaque("Placa_Academia", zone1.transform, new Vector3(-3.7f, 2.2f, -37.2f), matGold);

            // Rejas de hierro forjado con remate de puntas
            CreateWroughtIronGate("Reja_Derecha", zone1.transform, new Vector3(2.1f, 2.5f, -38f), 3.8f, 4.8f, matIron, false);
            CreateWroughtIronGate("Reja_Izquierda_Entreabierta", zone1.transform, new Vector3(-2.8f, 2.5f, -36.5f), 3.8f, 4.8f, matIron, true);

            CreateWall("Muro_Lat_Ext_L", zone1.transform, new Vector3(-14f, 2.5f, -32f), new Vector3(1f, 5f, 12f), matStoneWall);
            CreateWall("Muro_Lat_Ext_R", zone1.transform, new Vector3(14f, 2.5f, -32f), new Vector3(1f, 5f, 12f), matStoneWall);

            // Enredaderas secas trepando por los muros
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

            // Bordillos de piedra labrada que delimitan el camino central
            CreatePrimitive("Bordillo_Path_L", PrimitiveType.Cube, zone2.transform,
                new Vector3(-3.85f, 0.05f, -15.5f), new Vector3(0.25f, 0.15f, 21f), matFacade);
            CreatePrimitive("Bordillo_Path_R", PrimitiveType.Cube, zone2.transform,
                new Vector3(3.85f, 0.05f, -15.5f), new Vector3(0.25f, 0.15f, 21f), matFacade);

            CreateWall("Patio_Wall_L", zone2.transform, new Vector3(-14f, 3f, -15.5f), new Vector3(1f, 6f, 21f), matFacade);
            CreateWall("Patio_Wall_R", zone2.transform, new Vector3(14f, 3f, -15.5f), new Vector3(1f, 6f, 21f), matFacade);

            // Jardineras simétricas descuidadas con vegetación invernal
            CreatePrimitive("Jardinera_Border_L", PrimitiveType.Cube, zone2.transform,
                new Vector3(-8f, 0.4f, -15.5f), new Vector3(4.5f, 0.8f, 14f), matStoneWall);
            CreatePrimitive("Vegetacion_L", PrimitiveType.Cube, zone2.transform,
                new Vector3(-8f, 0.9f, -15.5f), new Vector3(3.8f, 0.6f, 13.2f), matHedge);

            CreatePrimitive("Jardinera_Border_R", PrimitiveType.Cube, zone2.transform,
                new Vector3(8f, 0.4f, -15.5f), new Vector3(4.5f, 0.8f, 14f), matStoneWall);
            CreatePrimitive("Vegetacion_R", PrimitiveType.Cube, zone2.transform,
                new Vector3(8f, 0.9f, -15.5f), new Vector3(3.8f, 0.6f, 13.2f), matHedge);

            // Estatuas de bailarinas clásicas sobre pedestal
            CreateBallerinaStatue("Estatua_Bailarina_CisneBlanco", zone2.transform,
                new Vector3(-8f, 0f, -15.5f), matStatue, matStoneWall, "Estatua: El Cisne Blanco");
            CreateBallerinaStatue("Estatua_Bailarina_CisneNegro", zone2.transform,
                new Vector3(8f, 0f, -15.5f), matStatue, matStoneWall, "Estatua: El Cisne Negro");

            // Charcos de agua reflectantes
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

            // 1. 12 escalones arquitectónicos monumentales con alfombra roja central y barras doradas
            int stepCount = 12;
            float stepDepth = 0.65f; // 7.8 metros de longitud (de -5.0 a +2.8)
            float stepHeight = porticoY / stepCount; // 0.20m por escalón

            for (int i = 0; i < stepCount; i++)
            {
                float stepZ = -5.0f + (i * stepDepth) + (stepDepth * 0.5f);
                float stepY = (i * stepHeight) + (stepHeight * 0.5f);

                // Bloque de piedra sillar del escalón
                CreatePrimitive($"Escalon_Piedra_{i + 1}", PrimitiveType.Cube, zone3.transform,
                    new Vector3(0f, stepY, stepZ), new Vector3(14f, stepHeight, stepDepth), matFacade);

                // Alfombra de terciopelo vino damasco central en cada peldaño
                CreatePrimitive($"Escalon_Alfombra_{i + 1}", PrimitiveType.Cube, zone3.transform,
                    new Vector3(0f, stepY + 0.015f, stepZ), new Vector3(5.2f, stepHeight + 0.01f, stepDepth), matCarpet);

                // Barra de sujeción dorada clásica en la base del peldaño
                CreatePrimitive($"Barra_Dorada_{i + 1}", PrimitiveType.Cube, zone3.transform,
                    new Vector3(0f, (i * stepHeight) + 0.03f, -5.0f + (i * stepDepth) + 0.04f), new Vector3(5.4f, 0.04f, 0.04f), matGold);
            }

            // 2. RAMPA FÍSICA INVISIBLE CONTINUA
            // Conecta exactamente a nivel del suelo con el patio en Z: -5 (Y: 0m) y con el pórtico en Z: +2.8 (Y: 2.4m)
            GameObject stairRamp = new GameObject("Stair_Smooth_Physics_Incline");
            stairRamp.transform.parent = zone3.transform;
            stairRamp.transform.localPosition = new Vector3(0f, 0.91f, -1.01f);
            stairRamp.transform.localRotation = Quaternion.Euler(-17.10f, 0f, 0f);
            BoxCollider rampCol = stairRamp.AddComponent<BoxCollider>();
            rampCol.size = new Vector3(14.5f, 0.60f, 8.5f);

            // Balaustradas arquitectónicas laterales con zócalo, postes y pasamanos
            CreateArchitecturalBalustrade("Balaustrada_Left", zone3.transform, new Vector3(-7.2f, 0f, -1.1f), 8.5f, matFacade, matStoneWall);
            CreateArchitecturalBalustrade("Balaustrada_Right", zone3.transform, new Vector3(7.2f, 0f, -1.1f), 8.5f, matFacade, matStoneWall);

            // Descanso superior del pórtico (Y = 2.4m, Z: 2.8 a 8, Ancho: 18m)
            CreatePrimitive("Portico_Landing", PrimitiveType.Cube, zone3.transform,
                new Vector3(0f, porticoY - 0.25f, 5.4f), new Vector3(18f, 0.5f, 5.5f), matCobble);

            // Alfombra que continúa desde los escalones por el pórtico hasta las puertas
            CreatePrimitive("Portico_Carpet", PrimitiveType.Cube, zone3.transform,
                new Vector3(0f, porticoY + 0.015f, 5.4f), new Vector3(5.2f, 0.03f, 5.5f), matCarpet);

            // 4 Columnas jónicas clásicas monumentales con plinto, fuste y capitel
            CreateClassicalColumn("Columna_Portico_1", zone3.transform, new Vector3(-5.5f, porticoY, 4.2f), 5.5f, 0.9f, matFacade);
            CreateClassicalColumn("Columna_Portico_2", zone3.transform, new Vector3(-2f, porticoY, 4.2f), 5.5f, 0.9f, matFacade);
            CreateClassicalColumn("Columna_Portico_3", zone3.transform, new Vector3(2f, porticoY, 4.2f), 5.5f, 0.9f, matFacade);
            CreateClassicalColumn("Columna_Portico_4", zone3.transform, new Vector3(5.5f, porticoY, 4.2f), 5.5f, 0.9f, matFacade);

            // Entablamento y frontón clásico con molduras
            CreatePrimitive("Entablamento_Portico", PrimitiveType.Cube, zone3.transform,
                new Vector3(0f, porticoY + 5.8f, 4.8f), new Vector3(16f, 0.8f, 3.5f), matFacade);
            CreatePrimitive("Fronton_Triangular", PrimitiveType.Cube, zone3.transform,
                new Vector3(0f, porticoY + 6.8f, 4.8f), new Vector3(12f, 1.2f, 1.5f), matFacade);

            // Fachada Principal
            float facadeY = porticoY + 4f;
            CreateWall("Fachada_L", zone3.transform, new Vector3(-8f, facadeY, 8f), new Vector3(12f, 8f, 0.8f), matFacade);
            CreateWall("Fachada_R", zone3.transform, new Vector3(8f, facadeY, 8f), new Vector3(12f, 8f, 0.8f), matFacade);
            CreateWall("Fachada_Top", zone3.transform, new Vector3(0f, porticoY + 6f, 8f), new Vector3(4f, 4f, 0.8f), matFacade);

            // Grandes Puertas Dobles de Entrada Principal (caoba con herrajes dorados y aldabas)
            WorldDoubleDoor entranceDoors = CreateGrandDoubleDoor("Grand_Entrance_Doors", zone3.transform,
                new Vector3(0f, porticoY, 8f), matMahogany, matGold);

            CreateOrnateSconce("Farol_Puerta_L", zone3.transform, new Vector3(-2.8f, porticoY + 2.5f, 7.5f), matGold, matLanternGlow, new Color(1f, 0.85f, 0.55f), 2.5f);
            CreateOrnateSconce("Farol_Puerta_R", zone3.transform, new Vector3(2.8f, porticoY + 2.5f, 7.5f), matGold, matLanternGlow, new Color(1f, 0.85f, 0.55f), 2.5f);

            // ====================================================================
            // ZONA 4: VESTÍBULO DE RECEPCIÓN (Z: +8 a +32, Y: 2.4, 22m × 24m)
            // ====================================================================
            GameObject zone4 = new GameObject("4_Reception_Vestibule");
            zone4.transform.parent = rootEnv.transform;

            float vestY = porticoY;
            float vestHeight = 7.2f;

            // Suelo de mármol veneciano pulido con cenefa perimetral de mármol negro
            CreatePrimitive("Vestibulo_Marble_Floor", PrimitiveType.Cube, zone4.transform,
                new Vector3(0f, vestY - 0.25f, 20f), new Vector3(22f, 0.5f, 24f), matMarble);

            // Cenefa perimetral oscura en el mármol
            CreatePrimitive("Cenefa_Marble_N", PrimitiveType.Cube, zone4.transform,
                new Vector3(0f, vestY - 0.22f, 31f), new Vector3(21.5f, 0.5f, 0.6f), matDarkMarbleBorder);
            CreatePrimitive("Cenefa_Marble_S", PrimitiveType.Cube, zone4.transform,
                new Vector3(0f, vestY - 0.22f, 9f), new Vector3(21.5f, 0.5f, 0.6f), matDarkMarbleBorder);
            CreatePrimitive("Cenefa_Marble_W", PrimitiveType.Cube, zone4.transform,
                new Vector3(-10.2f, vestY - 0.22f, 20f), new Vector3(0.6f, 0.5f, 23.5f), matDarkMarbleBorder);
            CreatePrimitive("Cenefa_Marble_E", PrimitiveType.Cube, zone4.transform,
                new Vector3(10.2f, vestY - 0.22f, 20f), new Vector3(0.6f, 0.5f, 23.5f), matDarkMarbleBorder);

            // Gran alfombra pasillera de terciopelo vino damasco
            CreatePrimitive("Vestibulo_Carpet", PrimitiveType.Cube, zone4.transform,
                new Vector3(0f, vestY + 0.02f, 20f), new Vector3(5.5f, 0.05f, 24f), matCarpet);

            // Techo artesonado con vigas oscuras
            CreatePrimitive("Vestibulo_Ceiling", PrimitiveType.Cube, zone4.transform,
                new Vector3(0f, vestY + vestHeight, 20f), new Vector3(22f, 0.5f, 24f), matWallIvory);

            for (int b = 0; b < 5; b++)
            {
                float beamZ = 10f + (b * 5.0f);
                CreatePrimitive($"Ceiling_Beam_{b + 1}", PrimitiveType.Cube, zone4.transform,
                    new Vector3(0f, vestY + vestHeight - 0.25f, beamZ), new Vector3(22f, 0.5f, 0.6f), matMahogany);
            }

            // Paredes con molduras marfil (Boiserie) y zócalos de caoba
            CreateWall("Vest_Wall_West_1", zone4.transform, new Vector3(-11f, vestY + (vestHeight * 0.5f), 13f), new Vector3(0.6f, vestHeight, 10f), matWallIvory);
            CreateWall("Vest_Wall_West_2", zone4.transform, new Vector3(-11f, vestY + (vestHeight * 0.5f), 27f), new Vector3(0.6f, vestHeight, 10f), matWallIvory);
            CreateWall("Vest_Wall_West_ArchTop", zone4.transform, new Vector3(-11f, vestY + 5.5f, 20f), new Vector3(0.6f, 3.4f, 4f), matWallIvory);
            CreateDoorwayArch("Arch_Administration", zone4.transform, new Vector3(-10.8f, vestY + 1.8f, 20f), "ADMINISTRACIÓN Y ARCHIVO", matMahogany, matGold, matVelvetDrapes);

            CreateWall("Vest_Wall_East_1", zone4.transform, new Vector3(11f, vestY + (vestHeight * 0.5f), 13f), new Vector3(0.6f, vestHeight, 10f), matWallIvory);
            CreateWall("Vest_Wall_East_2", zone4.transform, new Vector3(11f, vestY + (vestHeight * 0.5f), 27f), new Vector3(0.6f, vestHeight, 10f), matWallIvory);
            CreateWall("Vest_Wall_East_ArchTop", zone4.transform, new Vector3(11f, vestY + 5.5f, 20f), new Vector3(0.6f, 3.4f, 4f), matWallIvory);
            CreateDoorwayArch("Arch_DressingRooms", zone4.transform, new Vector3(10.8f, vestY + 1.8f, 20f), "VESTUARIOS Y SALONES", matMahogany, matGold, matVelvetDrapes);

            // Marcos ornamentales Boiserie en relieve 3D sobre las paredes del vestíbulo
            CreateBoiseriePanelSet("Boiserie_Vest_W1", zone4.transform, new Vector3(-10.65f, vestY + 2.6f, 13f), 8f, matWallIvory, matMahogany);
            CreateBoiseriePanelSet("Boiserie_Vest_W2", zone4.transform, new Vector3(-10.65f, vestY + 2.6f, 27f), 8f, matWallIvory, matMahogany);
            CreateBoiseriePanelSet("Boiserie_Vest_E1", zone4.transform, new Vector3(10.65f, vestY + 2.6f, 13f), 8f, matWallIvory, matMahogany);
            CreateBoiseriePanelSet("Boiserie_Vest_E2", zone4.transform, new Vector3(10.65f, vestY + 2.6f, 27f), 8f, matWallIvory, matMahogany);

            // Portal hacia el pasillo principal con telones de terciopelo vino
            CreateWall("Vest_Wall_North_L", zone4.transform, new Vector3(-7.5f, vestY + (vestHeight * 0.5f), 32f), new Vector3(7f, vestHeight, 0.6f), matWallIvory);
            CreateWall("Vest_Wall_North_R", zone4.transform, new Vector3(7.5f, vestY + (vestHeight * 0.5f), 32f), new Vector3(7f, vestHeight, 0.6f), matWallIvory);
            CreateWall("Vest_Wall_North_Top", zone4.transform, new Vector3(0f, vestY + 5.5f, 32f), new Vector3(8f, 3.4f, 0.6f), matWallIvory);

            // Telones teatrales recogidos a los lados del gran portal
            CreatePrimitive("Curtain_Arch_L", PrimitiveType.Cube, zone4.transform,
                new Vector3(-3.8f, vestY + 3f, 31.8f), new Vector3(1.2f, 5.5f, 0.4f), matVelvetDrapes);
            CreatePrimitive("Curtain_Arch_R", PrimitiveType.Cube, zone4.transform,
                new Vector3(3.8f, vestY + 3f, 31.8f), new Vector3(1.2f, 5.5f, 0.4f), matVelvetDrapes);

            // Mostrador de Recepción Antiguo con detalles de ballet y libro interactivo
            CreateReceptionDesk("Reception_Counter", zone4.transform, new Vector3(-5.5f, vestY, 14f),
                matMahogany, matGold, matClothCover, matGreenGlass);

            // Retrato histórico al óleo de Elena Vesper en el muro sur del vestíbulo
            CreateFramedArtPortrait("Retrato_Elena_Vesper_CisneNegro", zone4.transform,
                new Vector3(5.5f, vestY + 3.2f, 8.4f), new Vector3(2.4f, 3.2f, 0.2f), matGold, matPortraitElena,
                "Retrato de Elena Vesper — El Cisne Negro (1924)");

            // Foco de iluminación dedicado sobre el retrato de Elena Vesper
            CreatePictureSpotlight("Spotlight_Elena_Portrait", zone4.transform,
                new Vector3(5.5f, vestY + 5.5f, 11f), new Vector3(5.5f, vestY + 3.2f, 8.4f), new Color(1.0f, 0.90f, 0.75f), 3.5f);

            // Sofás cubiertos con sábanas de polvo
            CreateCoveredFurniture("Sofa_Cubierto_Polvo", zone4.transform, new Vector3(7.5f, vestY, 25f), matClothCover, matMahogany);

            // Gran Lámpara de Araña central deteriorada con luz cálida y sombras suaves
            CreateChandelier("Arana_Cristal_Vestibulo", zone4.transform, new Vector3(0f, vestY + 5.5f, 20f),
                matGold, new Color(1f, 0.85f, 0.55f), 4.8f);

            // ====================================================================
            // ZONA 5: PASILLO PRINCIPAL (Z: +32 a +78, Y: 2.4, Ancho: 8m, Largo: 46m)
            // ====================================================================
            GameObject zone5 = new GameObject("5_Grand_Hallway");
            zone5.transform.parent = rootEnv.transform;

            float hallY = vestY;
            float hallHeight = 5.2f;

            // Suelo de parqué envejecido en espiga (Chevron)
            CreatePrimitive("Pasillo_Parquet_Floor", PrimitiveType.Cube, zone5.transform,
                new Vector3(0f, hallY - 0.25f, 55f), new Vector3(8f, 0.5f, 46f), matParquet);

            // Alfombra de damasco vino continua que guía la mirada hacia las profundidades
            CreatePrimitive("Pasillo_Carpet_Runner", PrimitiveType.Cube, zone5.transform,
                new Vector3(0f, hallY + 0.02f, 55f), new Vector3(3.2f, 0.05f, 46f), matCarpet);

            // Techo del pasillo con molduras perimetrales
            CreatePrimitive("Pasillo_Ceiling", PrimitiveType.Cube, zone5.transform,
                new Vector3(0f, hallY + hallHeight, 55f), new Vector3(8f, 0.5f, 46f), matWallIvory);

            // Muros laterales del corredor
            CreateWall("Pasillo_Wall_West", zone5.transform, new Vector3(-4f, hallY + (hallHeight * 0.5f), 55f), new Vector3(0.5f, hallHeight, 46f), matWallIvory);
            CreateWall("Pasillo_Wall_East", zone5.transform, new Vector3(4f, hallY + (hallHeight * 0.5f), 55f), new Vector3(0.5f, hallHeight, 46f), matWallIvory);

            // Zócalos de caoba en el pasillo
            CreatePrimitive("Zocalo_Hall_W", PrimitiveType.Cube, zone5.transform,
                new Vector3(-3.72f, hallY + 0.35f, 55f), new Vector3(0.1f, 0.7f, 46f), matMahogany);
            CreatePrimitive("Zocalo_Hall_E", PrimitiveType.Cube, zone5.transform,
                new Vector3(3.72f, hallY + 0.35f, 55f), new Vector3(0.1f, 0.7f, 46f), matMahogany);

            // Boiserie ornamental a lo largo del pasillo
            for (float pz = 36f; pz <= 72f; pz += 6f)
            {
                CreateBoiseriePanelSet($"Boiserie_Hall_W_{pz}", zone5.transform, new Vector3(-3.73f, hallY + 2.2f, pz), 4.5f, matWallIvory, matMahogany);
                CreateBoiseriePanelSet($"Boiserie_Hall_E_{pz}", zone5.transform, new Vector3(3.73f, hallY + 2.2f, pz), 4.5f, matWallIvory, matMahogany);
            }

            // Puertas cerradas numeradas hacia futuras salas de ensayo
            CreateHallwayDoor("Door_Rehearsal_1", zone5.transform, new Vector3(-3.8f, hallY, 40f), true, "SALA 1: ENSAYO PRINCIPAL", matMahogany, matGold);
            CreateHallwayDoor("Door_Costumes", zone5.transform, new Vector3(3.8f, hallY, 44f), false, "SALA DE VESTUARIO", matMahogany, matGold);
            CreateHallwayDoor("Door_Rehearsal_2", zone5.transform, new Vector3(-3.8f, hallY, 56f), true, "SALA 2: COREOGRAFÍA", matMahogany, matGold);
            CreateHallwayDoor("Door_Music_Archive", zone5.transform, new Vector3(3.8f, hallY, 62f), false, "ARCHIVO MUSICAL", matMahogany, matGold);

            // Galería de retratos de bailarinas en el pasillo
            CreateFramedArtPortrait("Portrait_Ballerina_1", zone5.transform,
                new Vector3(-3.7f, hallY + 2.6f, 48f), new Vector3(0.15f, 1.8f, 1.3f), matGold, matPortraitElena, "Retrato Histórico I");
            CreateFramedArtPortrait("Portrait_Ballerina_2", zone5.transform,
                new Vector3(3.7f, hallY + 2.6f, 52f), new Vector3(0.15f, 1.8f, 1.3f), matGold, matPortraitElena, "Retrato Histórico II");
            CreateFramedArtPortrait("Portrait_Ballerina_3", zone5.transform,
                new Vector3(-3.7f, hallY + 2.6f, 68f), new Vector3(0.15f, 1.8f, 1.3f), matGold, matPortraitElena, "Retrato Histórico III");

            // Apliques de luz cálida espaciados a lo largo del pasillo
            for (float lz = 36f; lz <= 74f; lz += 8f)
            {
                CreateOrnateSconce($"Sconce_Hall_W_{lz}", zone5.transform,
                    new Vector3(-3.7f, hallY + 2.9f, lz), matGold, matLanternGlow, new Color(1f, 0.82f, 0.50f), 2.2f);
                CreateOrnateSconce($"Sconce_Hall_E_{lz}", zone5.transform,
                    new Vector3(3.7f, hallY + 2.9f, lz), matGold, matLanternGlow, new Color(1f, 0.82f, 0.50f), 2.2f);
            }

            // Fondo del Pasillo: Gran arco con telón entreabierto hacia la oscuridad
            CreateWall("Pasillo_Wall_End", zone5.transform, new Vector3(0f, hallY + (hallHeight * 0.5f), 78f), new Vector3(8f, hallHeight, 0.6f), matWallIvory);
            CreateDoorwayArch("Arch_GrandTheatre_Entrance", zone5.transform, new Vector3(0f, hallY + 1.8f, 77.8f), "HACIA EL GRAN TEATRO", matMahogany, matGold, matVelvetDrapes);

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
            cc.stepOffset = 0.40f; // Sube escalones suavemente
            cc.slopeLimit = 60f;   // Sube la rampa con fluidez absoluta
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
            serLook.FindProperty("cameraTransform").objectReferenceValue = cameraObj.transform;
            serLook.ApplyModifiedProperties();

            PlayerInteraction interaction = player.AddComponent<PlayerInteraction>();
            SerializedObject serInteract = new SerializedObject(interaction);
            serInteract.FindProperty("playerCamera").objectReferenceValue = cam;
            serInteract.ApplyModifiedProperties();

            // Linterna sutil de apoyo
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
            serializedUI.FindProperty("playerInteraction").objectReferenceValue = interaction;
            serializedUI.FindProperty("promptTMP").objectReferenceValue = promptText;
            serializedUI.FindProperty("crosshairImage").objectReferenceValue = reticleImage;
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

            Debug.Log($"<color=green>[Último Acto]</color> ¡SECCIÓN 1 (Exterior y Entrada) construida con éxito en {ScenePath}!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"<color=red>[Último Acto]</color> Error crítico al construir Sección 1: {ex.Message}\n{ex.StackTrace}");
            }
        }

        #region Métodos de Creación PBR y Arquitectura

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

            // CRUCIAL: Color base en blanco para que la textura brille con su contraste original
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

            // Plinto cuadrado inferior
            CreatePrimitive("Plinto_Base", PrimitiveType.Cube, colRoot.transform,
                new Vector3(0f, plinthH * 0.5f, 0f), new Vector3(radius * 1.5f, plinthH, radius * 1.5f), mat);

            // Fuste estriado cilíndrico
            CreatePrimitive("Fuste_Shaft", PrimitiveType.Cylinder, colRoot.transform,
                new Vector3(0f, plinthH + (shaftH * 0.5f), 0f), new Vector3(radius, shaftH * 0.5f, radius), mat);

            // Capitel con volutas
            CreatePrimitive("Capitel_Top", PrimitiveType.Cube, colRoot.transform,
                new Vector3(0f, totalHeight - (capH * 0.5f), 0f), new Vector3(radius * 1.4f, capH, radius * 1.4f), mat);
        }

        private static void CreateArchitecturalBalustrade(string name, Transform parent, Vector3 centerPos, float length, Material stoneMat, Material baseMat)
        {
            GameObject balRoot = new GameObject(name);
            balRoot.transform.parent = parent;
            balRoot.transform.position = centerPos;

            // Zócalo base inclinado
            CreatePrimitive("Balustrade_Base", PrimitiveType.Cube, balRoot.transform,
                new Vector3(0f, 0.8f, 0f), new Vector3(0.5f, 0.4f, length), baseMat);

            // Pasamanos superior
            CreatePrimitive("Balustrade_Rail", PrimitiveType.Cube, balRoot.transform,
                new Vector3(0f, 1.8f, 0f), new Vector3(0.6f, 0.25f, length), stoneMat);

            // Cuerpo sólido lateral de protección contra caídas
            CreatePrimitive("Balustrade_Wall", PrimitiveType.Cube, balRoot.transform,
                new Vector3(0f, 1.3f, 0f), new Vector3(0.4f, 1.2f, length), stoneMat);
        }

        private static void CreateWroughtIronGate(string name, Transform parent, Vector3 pos, float width, float height, Material ironMat, bool open)
        {
            GameObject gate = new GameObject(name);
            gate.transform.parent = parent;
            gate.transform.position = pos;
            if (open) gate.transform.rotation = Quaternion.Euler(0f, 35f, 0f);

            // Marco de hierro
            CreatePrimitive("Gate_Frame", PrimitiveType.Cube, gate.transform,
                Vector3.zero, new Vector3(width, height, 0.15f), ironMat);

            // Puntas de lanza ornamentales
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

            // Marco ornamental en relieve (moldura rectangular)
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
            GameObject doorRoot = new GameObject(name);
            doorRoot.transform.parent = parent;
            doorRoot.transform.position = basePos;

            float doorWidth = 1.8f;
            float doorHeight = 4.0f;
            float doorThick = 0.2f;

            GameObject leftHinge = new GameObject("Left_Hinge");
            leftHinge.transform.parent = doorRoot.transform;
            leftHinge.transform.localPosition = new Vector3(-doorWidth, 0f, 0f);

            GameObject leftMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftMesh.name = "Left_Door_Mesh";
            leftMesh.transform.parent = leftHinge.transform;
            leftMesh.transform.localPosition = new Vector3(doorWidth * 0.5f, doorHeight * 0.5f, 0f);
            leftMesh.transform.localScale = new Vector3(doorWidth, doorHeight, doorThick);
            leftMesh.GetComponent<Renderer>().sharedMaterial = woodMat;

            GameObject leftHandle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftHandle.name = "Left_Handle";
            leftHandle.transform.parent = leftMesh.transform;
            leftHandle.transform.localPosition = new Vector3(0.35f, 0f, -0.6f);
            leftHandle.transform.localScale = new Vector3(0.12f, 0.4f, 0.2f);
            leftHandle.GetComponent<Renderer>().sharedMaterial = goldMat;

            GameObject rightHinge = new GameObject("Right_Hinge");
            rightHinge.transform.parent = doorRoot.transform;
            rightHinge.transform.localPosition = new Vector3(doorWidth, 0f, 0f);

            GameObject rightMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightMesh.name = "Right_Door_Mesh";
            rightMesh.transform.parent = rightHinge.transform;
            rightMesh.transform.localPosition = new Vector3(-doorWidth * 0.5f, doorHeight * 0.5f, 0f);
            rightMesh.transform.localScale = new Vector3(doorWidth, doorHeight, doorThick);
            rightMesh.GetComponent<Renderer>().sharedMaterial = woodMat;

            GameObject rightHandle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightHandle.name = "Right_Handle";
            rightHandle.transform.parent = rightMesh.transform;
            rightHandle.transform.localPosition = new Vector3(-0.35f, 0f, -0.6f);
            rightHandle.transform.localScale = new Vector3(0.12f, 0.4f, 0.2f);
            rightHandle.GetComponent<Renderer>().sharedMaterial = goldMat;

            WorldDoubleDoor doubleDoor = doorRoot.AddComponent<WorldDoubleDoor>();
            SerializedObject ser = new SerializedObject(doubleDoor);
            ser.FindProperty("leftDoorHinge").objectReferenceValue = leftHinge.transform;
            ser.FindProperty("rightDoorHinge").objectReferenceValue = rightHinge.transform;
            ser.FindProperty("openAngle").floatValue = 95f;
            ser.FindProperty("animationDuration").floatValue = 1.2f;
            ser.ApplyModifiedProperties();

            return doubleDoor;
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

            // Mostrador curvo/en L de madera noble
            CreatePrimitive("Desk_Main", PrimitiveType.Cube, desk.transform,
                new Vector3(0f, 0.6f, 0f), new Vector3(4.8f, 1.2f, 1.4f), woodMat);
            CreatePrimitive("Desk_CounterTop", PrimitiveType.Cube, desk.transform,
                new Vector3(0f, 1.25f, 0f), new Vector3(5.1f, 0.1f, 1.6f), woodMat);

            // Libro de Registro Antiguo interactivo
            GameObject bookObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bookObj.name = "Libro_Registro_Recepcion";
            bookObj.transform.parent = desk.transform;
            bookObj.transform.localPosition = new Vector3(-0.8f, 1.35f, 0f);
            bookObj.transform.localScale = new Vector3(0.55f, 0.08f, 0.42f);
            bookObj.transform.localRotation = Quaternion.Euler(0f, 12f, 0f);
            bookObj.GetComponent<Renderer>().sharedMaterial = goldMat;

            WorldDocument doc = bookObj.AddComponent<WorldDocument>();
            SerializedObject serDoc = new SerializedObject(doc);
            SerializedProperty propTitle = serDoc.FindProperty("title");
            if (propTitle != null) propTitle.stringValue = "Libro de Huéspedes — Último Ensayo General";
            SerializedProperty propContent = serDoc.FindProperty("content");
            if (propContent != null) propContent.stringValue =
                "14 de Octubre de 1924\n\n" +
                "El ensayo final de 'El Lago de los Cisnes' dará comienzo a las 20:00 h en el Gran Teatro.\n" +
                "Elena Vesper (Cisne Negro) ha manifestado malestar y se niega a ensayar con el resto del elenco.\n" +
                "Se escucharon gritos cerca de la sala de espejos antes del cierre.\n\n" +
                "[Una nota apresurada añadida al pie con tinta corrida]:\n" +
                "“La música se detuvo. Elena no está en su camerino. Las puertas del teatro han sido cerradas con llave desde adentro.”";
            serDoc.ApplyModifiedProperties();

            // Campana de latón antigua
            CreatePrimitive("Campanilla_Recepcion", PrimitiveType.Cylinder, desk.transform,
                new Vector3(0.6f, 1.35f, 0.2f), new Vector3(0.18f, 0.1f, 0.18f), goldMat);

            // Lámpara de banquero clásica con pantalla verde
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

        private static void CreateHallwayDoor(string name, Transform parent, Vector3 pos, bool onLeftWall, string label, Material woodMat, Material goldMat)
        {
            GameObject doorFrame = new GameObject(name);
            doorFrame.transform.parent = parent;
            doorFrame.transform.position = pos;
            if (!onLeftWall) doorFrame.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            GameObject doorMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorMesh.name = "DoorMesh";
            doorMesh.transform.parent = doorFrame.transform;
            doorMesh.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            doorMesh.transform.localScale = new Vector3(0.15f, 3.0f, 1.8f);
            doorMesh.GetComponent<Renderer>().sharedMaterial = woodMat;

            GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = "Brass_Sign";
            sign.transform.parent = doorMesh.transform;
            sign.transform.localPosition = new Vector3(onLeftWall ? 0.6f : -0.6f, 0.35f, 0f);
            sign.transform.localScale = new Vector3(0.08f, 0.15f, 0.7f);
            sign.GetComponent<Renderer>().sharedMaterial = goldMat;

            WorldDoor doorComp = doorFrame.AddComponent<WorldDoor>();
            doorComp.SetLockState(true, "");
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

            // Cortinaje de terciopelo decorativo a los lados
            CreatePrimitive("Drape_L", PrimitiveType.Cube, arch.transform,
                new Vector3(0f, -1.2f, -1.1f), new Vector3(0.3f, 2.4f, 0.4f), drapeMat);
            CreatePrimitive("Drape_R", PrimitiveType.Cube, arch.transform,
                new Vector3(0f, -1.2f, 1.1f), new Vector3(0.3f, 2.4f, 0.4f), drapeMat);
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
            l.shadows = LightShadows.None; // Apliques usan iluminación difusa para optimizar el atlas de sombras en URP
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

        private static void CreateDocumentViewerModal(GameObject canvasObj)
        {
            GameObject docPanel = new GameObject("DocumentViewer_Panel");
            docPanel.transform.parent = canvasObj.transform;
            RectTransform panelRect = docPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image bg = docPanel.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.04f, 0.06f, 0.94f);

            GameObject card = new GameObject("Card");
            card.transform.parent = docPanel.transform;
            RectTransform cardRect = card.AddComponent<RectTransform>();
            cardRect.anchoredPosition = Vector2.zero;
            cardRect.sizeDelta = new Vector2(860f, 620f);
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.12f, 0.11f, 0.12f, 0.98f);

            GameObject border = new GameObject("Gold_Card_Border");
            border.transform.parent = card.transform;
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
            titleObj.transform.parent = card.transform;
            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.fontSize = 28;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = new Color(0.95f, 0.85f, 0.55f);
            RectTransform tRect = titleObj.GetComponent<RectTransform>();
            tRect.anchoredPosition = new Vector2(0f, 250f);
            tRect.sizeDelta = new Vector2(800f, 45f);

            GameObject contentObj = new GameObject("ContentText");
            contentObj.transform.parent = card.transform;
            TextMeshProUGUI contentTMP = contentObj.AddComponent<TextMeshProUGUI>();
            contentTMP.fontSize = 20;
            contentTMP.lineSpacing = 16f;
            contentTMP.alignment = TextAlignmentOptions.TopLeft;
            contentTMP.color = new Color(0.90f, 0.88f, 0.84f);
            RectTransform cRect = contentObj.GetComponent<RectTransform>();
            cRect.anchoredPosition = new Vector2(0f, -20f);
            cRect.sizeDelta = new Vector2(760f, 440f);

            GameObject hintObj = new GameObject("CloseHintText");
            hintObj.transform.parent = card.transform;
            TextMeshProUGUI hintTMP = hintObj.AddComponent<TextMeshProUGUI>();
            hintTMP.text = "[E] o [Escape] para cerrar documento";
            hintTMP.fontSize = 17;
            hintTMP.alignment = TextAlignmentOptions.Center;
            hintTMP.color = new Color(0.70f, 0.65f, 0.55f);
            RectTransform hRect = hintObj.GetComponent<RectTransform>();
            hRect.anchoredPosition = new Vector2(0f, -270f);
            hRect.sizeDelta = new Vector2(800f, 35f);

            DocumentUI docUI = docPanel.AddComponent<DocumentUI>();
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

            docPanel.SetActive(false);
        }

        private static GameObject CreatePrimitive(string name, PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.parent = parent;
            obj.transform.localPosition = pos;
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
