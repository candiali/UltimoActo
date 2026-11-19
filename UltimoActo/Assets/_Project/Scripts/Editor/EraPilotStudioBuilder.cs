#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UltimoActo.Core;

namespace UltimoActo.Editor
{
    /// <summary>
    /// Constructor modular de las dos épocas (1929 Gloria vs. Actualidad Ruinas)
    /// e implementación con calidad final de la Zona Piloto: Zona 16 (Aula Principal de Ballet).
    /// Sigue fielmente la imagen de referencia:
    /// - 1929: Ivory #F3E9DC, Dorado #C9A24A, Vino #6B1E2E, Rosa-lavanda #C9A6C9,
    ///         ventanales arqueados con sol cálido, parqué pulido con reflejos, gran araña encendida y mural intacto.
    /// - Ruinas: Grises pardos #3A3532, Musgo #4A5240, Vino apagado #4A2A30, Luz lunar #8FA0B8,
    ///         ventanales rotos con hiedra, charcos reflectantes, escombros, yeso caído, araña oxidada y mural agrietado.
    /// </summary>
    public static class EraPilotStudioBuilder
    {
        private const string EraMatDir = "Assets/_Project/Materials/";
        private const string EraTexDir = "Assets/_Project/Textures/PBR/";

        // Paleta Oficial 1929 (Gloria)
        public static readonly Color Col1929Ivory = new Color(0.953f, 0.914f, 0.863f, 1f);        // #F3E9DC
        public static readonly Color Col1929AntiqueGold = new Color(0.788f, 0.635f, 0.290f, 1f);  // #C9A24A
        public static readonly Color Col1929Wine = new Color(0.420f, 0.118f, 0.180f, 1f);         // #6B1E2E
        public static readonly Color Col1929RoseLavender = new Color(0.788f, 0.651f, 0.788f, 1f); // #C9A6C9
        public static readonly Color Col1929WarmSun = new Color(0.98f, 0.93f, 0.86f, 1f);

        // Paleta Oficial Ruinas (Actualidad)
        public static readonly Color ColRuinsDesatBrown = new Color(0.227f, 0.208f, 0.196f, 1f);  // #3A3532
        public static readonly Color ColRuinsMoss = new Color(0.290f, 0.322f, 0.251f, 1f);        // #4A5240
        public static readonly Color ColRuinsDullWine = new Color(0.290f, 0.165f, 0.188f, 1f);    // #4A2A30
        public static readonly Color ColRuinsColdMoon = new Color(0.561f, 0.627f, 0.722f, 1f);    // #8FA0B8

        private struct EraPaletteMaterials
        {
            // 1929
            public Material parquet1929;
            public Material wallIvory1929;
            public Material boiserieCream1929;
            public Material antiqueGold1929;
            public Material mahoganyPolished1929;
            public Material velvetWine1929;
            public Material roseLavender1929;
            public Material mirrorIntact1929;
            public Material sunsetGlassEmit1929;
            public Material crystalBulbEmit1929;
            public Material sunBeamVolumetric1929;
            public Material muralFresco1929;

            // Ruinas
            public Material parquetRuins;
            public Material wallCrackedRuins;
            public Material exposedBrickRuins;
            public Material rustedIronRuins;
            public Material rottedWoodRuins;
            public Material tornWineVelvetRuins;
            public Material mossAndIvyRuins;
            public Material darkVineBarkRuins;
            public Material brokenMirrorRuins;
            public Material waterPuddleRuins;
            public Material moonBeamVolumetricRuins;
            public Material cobwebTranslucentRuins;
            public Material muralCrackedRuins;
        }

        [MenuItem("Tools/Último Acto/Épocas (1929 vs Ruinas)/Alternar Época Actual [F1]")]
        public static void MenuToggleEra()
        {
            if (EraVisualManager.Instance != null)
            {
                EraVisualManager.Instance.ToggleEra();
            }
            else
            {
                int l1929 = EraVisualManager.GetEra1929Layer();
                int lRuins = EraVisualManager.GetEraRuinsLayer();
                bool showing1929 = (Tools.visibleLayers & (1 << l1929)) != 0;
                if (showing1929)
                {
                    Tools.visibleLayers &= ~(1 << l1929);
                    Tools.visibleLayers |= (1 << lRuins);
                    Debug.Log("<color=#8FA0B8>[Último Acto]</color> Scene View cambiado a ÉPOCA RUINAS (Actualidad).");
                }
                else
                {
                    Tools.visibleLayers |= (1 << l1929);
                    Tools.visibleLayers &= ~(1 << lRuins);
                    Debug.Log("<color=#C9A24A>[Último Acto]</color> Scene View cambiado a ÉPOCA 1929 (Gloria).");
                }
                SceneView.RepaintAll();
            }
        }

        [MenuItem("Tools/Último Acto/Épocas (1929 vs Ruinas)/Enfocar Zona Piloto (Zona 16: Salón de Ensayo)")]
        public static void MenuFocusPilotZone16()
        {
            Vector3 studioCamPos = new Vector3(-21.2f, 4.4f, 52.8f);
            Vector3 studioLookTarget = new Vector3(-11.5f, 4.6f, 64.5f);
            Quaternion lookRot = Quaternion.LookRotation((studioLookTarget - studioCamPos).normalized, Vector3.up);

            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.LookAtDirect(studioLookTarget, lookRot, 8.5f);
                SceneView.lastActiveSceneView.Repaint();
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                player.transform.position = new Vector3(-20.5f, 3.4f, 53.5f);
                player.transform.rotation = Quaternion.Euler(0f, 42f, 0f);
                if (cc != null) cc.enabled = true;
            }
            Debug.Log("<color=#C9A24A>[Último Acto]</color> Cámara y Jugador situados en Zona 16 (Aula Principal de Ballet). Pulsa [F1] en Play Mode o usa el menú para alternar entre 1929 y Ruinas.");
        }

        /// <summary>
        /// Construye el gestor de épocas en la escena y remodela por completo la Zona 16
        /// (Aula Principal de Ballet) con sus grupos Decor_1929 y Decor_Ruins.
        /// </summary>
        public static void SetupEraSystemAndPilotZone16(Transform rootEnv, GameObject zone16, float vestY)
        {
            EnsureEraLayersInTagManager();
            EnsureDualEraProceduralTextures();
            EraPaletteMaterials mats = LoadOrCreateEraMaterials();

            // 1. Asegurar EraVisualManager y volúmenes URP en la escena
            GameObject eraMgrObj = GameObject.Find("EraVisualManager");
            if (eraMgrObj == null)
            {
                eraMgrObj = new GameObject("EraVisualManager");
            }
            if (eraMgrObj.GetComponent<EraVisualManager>() == null)
            {
                eraMgrObj.AddComponent<EraVisualManager>();
            }

            // Asegurar EraCameraBinder en la cámara del jugador
            Camera mainCam = Camera.main;
            if (mainCam != null && mainCam.GetComponent<EraCameraBinder>() == null)
            {
                mainCam.gameObject.AddComponent<EraCameraBinder>();
            }

            if (zone16 == null) return;

            // 2. Ocultar los adornos antiguos de una sola época en Zona 16 (conservando colliders estructurales, puertas, anomalías y documentos)
            PrepareZone16BaseGeometry(zone16.transform, mats);

            // 3. Crear los dos grupos hijos por época: Decor_1929 (Layer Era1929) y Decor_Ruins (Layer EraRuins)
            int layer1929 = EraVisualManager.GetEra1929Layer();
            int layerRuins = EraVisualManager.GetEraRuinsLayer();

            Transform existing1929 = zone16.transform.Find("Decor_1929");
            if (existing1929 != null) Object.DestroyImmediate(existing1929.gameObject);
            Transform existingRuins = zone16.transform.Find("Decor_Ruins");
            if (existingRuins != null) Object.DestroyImmediate(existingRuins.gameObject);

            GameObject decor1929 = new GameObject("Decor_1929");
            decor1929.transform.SetParent(zone16.transform, false);
            decor1929.layer = layer1929;
            decor1929.AddComponent<EraOnlyVisibility>().Configure(EraVisibilityMode.Only1929, false);

            GameObject decorRuins = new GameObject("Decor_Ruins");
            decorRuins.transform.SetParent(zone16.transform, false);
            decorRuins.layer = layerRuins;
            decorRuins.AddComponent<EraOnlyVisibility>().Configure(EraVisibilityMode.OnlyRuins, false);

            float studioH = 6.4f;

            // ====================================================================
            // CONSTRUCCIÓN MODULAR SIMÉTRICA EN AMBAS ÉPOCAS (1929 ↔ RUINAS)
            // ====================================================================

            // A. Boiserie arquitectónica, pilastras corintias, zócalos, cornisas y artesonado del techo
            BuildBoiserieAndCeiling(decor1929.transform, vestY, studioH, EraVisual.Glory1929, mats, layer1929);
            BuildBoiserieAndCeiling(decorRuins.transform, vestY, studioH, EraVisual.Ruins, mats, layerRuins);

            // B. Ventanales arqueados monumentales (con cortinajes, luz volumétrica y hiedra en ruinas)
            float[] windowX = { -23.0f, -15.5f, -11.5f };
            for (int i = 0; i < windowX.Length; i++)
            {
                BuildArchedWindowBay(decor1929.transform, i, windowX[i], vestY, 67.62f, EraVisual.Glory1929, mats, layer1929);
                BuildArchedWindowBay(decorRuins.transform, i, windowX[i], vestY, 67.62f, EraVisual.Ruins, mats, layerRuins);
            }

            // C. Espejos monumentales del muro sur y barras de ballet dobles con soportes de latón / óxido
            BuildMirrorsAndBalletBarres(decor1929.transform, vestY, EraVisual.Glory1929, mats, layer1929);
            BuildMirrorsAndBalletBarres(decorRuins.transform, vestY, EraVisual.Ruins, mats, layerRuins);

            // D. Gran Mural Fresco de la Primera Bailarina en el muro este (como en la imagen de referencia)
            BuildBallerinaMuralFresco(decor1929.transform, vestY, EraVisual.Glory1929, mats, layer1929);
            BuildBallerinaMuralFresco(decorRuins.transform, vestY, EraVisual.Ruins, mats, layerRuins);

            // E. Lámparas de araña imperiales de cristal (1929: encendidas y doradas ↔ Ruinas: oxidadas, inclinadas y apagadas)
            Vector3[] chandelierPositions =
            {
                new Vector3(-20.0f, vestY + 5.75f, 59.0f),
                new Vector3(-11.0f, vestY + 5.75f, 59.0f)
            };
            for (int c = 0; c < chandelierPositions.Length; c++)
            {
                BuildChandelier(decor1929.transform, $"Chandelier_1929_{c + 1}", chandelierPositions[c], EraVisual.Glory1929, mats, layer1929);
                BuildChandelier(decorRuins.transform, $"Chandelier_Ruins_{c + 1}", chandelierPositions[c], EraVisual.Ruins, mats, layerRuins);
            }

            // F. Rincón musical del piano vertical, banquetas, jarrones con flores frescas (1929) vs rotas/volcadas (Ruinas)
            BuildPianoAndFurnitureProps(decor1929.transform, vestY, EraVisual.Glory1929, mats, layer1929);
            BuildPianoAndFurnitureProps(decorRuins.transform, vestY, EraVisual.Ruins, mats, layerRuins);

            // G. Suelo detallado: brillo especular en 1929 vs charcos reflectantes, tablas levantadas, escombros y hojas en Ruinas
            BuildFloorDetailsAndDebris(decor1929.transform, vestY, EraVisual.Glory1929, mats, layer1929);
            BuildFloorDetailsAndDebris(decorRuins.transform, vestY, EraVisual.Ruins, mats, layerRuins);

            // H. Atmósfera: partículas de polvo dorado (1929) vs polvo frío y esporas en suspensión (Ruinas) + Reflection Probe
            BuildAtmosphereParticles(decor1929.transform, new Vector3(-15f, vestY + 3.0f, 59f), EraVisual.Glory1929, mats, layer1929);
            BuildAtmosphereParticles(decorRuins.transform, new Vector3(-15f, vestY + 3.0f, 59f), EraVisual.Ruins, mats, layerRuins);
            BuildStudioReflectionProbe(zone16.transform, new Vector3(-15f, vestY + 2.6f, 59f));

            // Aplicar recursivamente la capa y banderas estáticas a cada grupo visual
            AssignVisualLayerAndStaticFlags(decor1929.transform, layer1929);
            AssignVisualLayerAndStaticFlags(decorRuins.transform, layerRuins);
        }

        // ====================================================================
        // 1. PREPARACIÓN DE GEOMETRÍA BASE COMPARTIDA (COLLIDERS COMUNES)
        // ====================================================================
        private static void PrepareZone16BaseGeometry(Transform zone16, EraPaletteMaterials mats)
        {
            // El suelo, techo y muros principales mantienen su Collider en Default, pero cambian de material PBR según la época
            AttachSurfaceAdapter(zone16.Find("MainStudio_Floor"), mats.parquet1929, mats.parquetRuins);
            AttachSurfaceAdapter(zone16.Find("MainStudio_Ceiling"), mats.wallIvory1929, mats.wallCrackedRuins);
            AttachSurfaceAdapter(zone16.Find("MainStudio_Wall_East_Upper"), mats.wallIvory1929, mats.wallCrackedRuins);
            AttachSurfaceAdapter(zone16.Find("MainStudio_Wall_South_WestSeg"), mats.wallIvory1929, mats.wallCrackedRuins);
            AttachSurfaceAdapter(zone16.Find("MainStudio_Wall_South_Upper"), mats.wallIvory1929, mats.wallCrackedRuins);
            AttachSurfaceAdapter(zone16.Find("MainStudio_Wall_West_S"), mats.wallIvory1929, mats.wallCrackedRuins);
            AttachSurfaceAdapter(zone16.Find("MainStudio_Wall_West_N"), mats.wallIvory1929, mats.wallCrackedRuins);
            AttachSurfaceAdapter(zone16.Find("MainStudio_Wall_North_W"), mats.wallIvory1929, mats.wallCrackedRuins);
            AttachSurfaceAdapter(zone16.Find("MainStudio_Wall_North_Mid"), mats.wallIvory1929, mats.wallCrackedRuins);
            AttachSurfaceAdapter(zone16.Find("MainStudio_Wall_North_E"), mats.wallIvory1929, mats.wallCrackedRuins);

            // También adaptar los muros colindantes que dan al interior de Zona 16 (si existen en el padre)
            if (zone16.parent != null)
            {
                Transform mainHall = zone16.parent.Find("05_Main_Hallway");
                if (mainHall != null)
                {
                    foreach (Transform t in mainHall)
                    {
                        if (t.name.StartsWith("Hall_Wall_West"))
                        {
                            AttachSurfaceAdapter(t, mats.wallIvory1929, mats.wallCrackedRuins);
                        }
                    }
                }
            }

            // Eliminar decoraciones antiguas simples (sin lógica) para reemplazarlas por las versiones detalladas duales
            string[] legacyDecorPrefixes =
            {
                "Studio_Ceiling_Beam_",
                "Worn_Varnish_Patch_",
                "MainStudio_Mirror_",
                "High_MoonWindow_",
                "MoonWindow_Curtain_",
                "Moonlight_MainStudio_",
                "Arana_Aula_Principal_",
                "Cartel_Aula_Posiciones_1",
                "Foto_Clase_Antigua_1922",
                "Reloj_Detenido_Aula_Principal"
            };

            for (int i = zone16.childCount - 1; i >= 0; i--)
            {
                Transform child = zone16.GetChild(i);
                for (int p = 0; p < legacyDecorPrefixes.Length; p++)
                {
                    if (child.name.StartsWith(legacyDecorPrefixes[p]))
                    {
                        Object.DestroyImmediate(child.gameObject);
                        break;
                    }
                }
            }
        }

        private static void AttachSurfaceAdapter(Transform target, Material mat1929, Material matRuins)
        {
            if (target == null) return;
            Renderer r = target.GetComponent<Renderer>();
            if (r == null) return;
            EraSurfaceAdapter adapter = target.GetComponent<EraSurfaceAdapter>();
            if (adapter == null) adapter = target.gameObject.AddComponent<EraSurfaceAdapter>();
            adapter.Configure(r, mat1929, matRuins);
            r.sharedMaterial = mat1929;
        }

        // ====================================================================
        // 2. BOISERIE, PILASTRAS CORINTIAS, MOLDURAS Y TECHO ARTESONADO
        // ====================================================================
        private static void BuildBoiserieAndCeiling(Transform parent, float vestY, float studioH, EraVisual era, EraPaletteMaterials mats, int layer)
        {
            bool is1929 = era == EraVisual.Glory1929;
            Material wallMat = is1929 ? mats.boiserieCream1929 : mats.wallCrackedRuins;
            Material trimMat = is1929 ? mats.antiqueGold1929 : mats.rustedIronRuins;
            Material woodMat = is1929 ? mats.mahoganyPolished1929 : mats.rottedWoodRuins;

            GameObject archRoot = CreateFolder("Boiserie_And_CofferedCeiling", parent, layer);

            // Zócalo perimetral (Rodapié + Arrambadero de Boiserie de 1.15m de altura)
            // Muro Sur (Z = 50.26m, X: -25.6 a -4.4)
            VisPiece("Dado_South_LowerBase", PrimitiveType.Cube, archRoot.transform,
                new Vector3(-15f, vestY + 0.12f, 50.28f), new Vector3(21.4f, 0.24f, 0.10f), woodMat, layer);
            VisPiece("Dado_South_Panel", PrimitiveType.Cube, archRoot.transform,
                new Vector3(-15f, vestY + 0.58f, 50.26f), new Vector3(21.4f, 0.85f, 0.06f), wallMat, layer);
            VisPiece("Dado_South_ChairRail", PrimitiveType.Cube, archRoot.transform,
                new Vector3(-15f, vestY + 1.05f, 50.30f), new Vector3(21.4f, 0.10f, 0.12f), trimMat, layer);

            // Muro Este (X = -4.26m, Z: 50.4 a 67.6, dejando hueco puerta en Z = 56m)
            VisPiece("Dado_East_SouthSeg", PrimitiveType.Cube, archRoot.transform,
                new Vector3(-4.27f, vestY + 0.55f, 52.3f), new Vector3(0.08f, 1.10f, 4.1f), wallMat, layer);
            VisPiece("Dado_East_ChairRail_S", PrimitiveType.Cube, archRoot.transform,
                new Vector3(-4.30f, vestY + 1.05f, 52.3f), new Vector3(0.12f, 0.09f, 4.1f), trimMat, layer);
            VisPiece("Dado_East_NorthSeg", PrimitiveType.Cube, archRoot.transform,
                new Vector3(-4.27f, vestY + 0.55f, 62.6f), new Vector3(0.08f, 1.10f, 9.8f), wallMat, layer);
            VisPiece("Dado_East_ChairRail_N", PrimitiveType.Cube, archRoot.transform,
                new Vector3(-4.30f, vestY + 1.05f, 62.6f), new Vector3(0.12f, 0.09f, 9.8f), trimMat, layer);

            // Cornisa escalonada imperial bajo el techo (Y = vestY + 6.18m)
            VisPiece("Cornice_South", PrimitiveType.Cube, archRoot.transform,
                new Vector3(-15f, vestY + studioH - 0.18f, 50.34f), new Vector3(21.6f, 0.32f, 0.24f), wallMat, layer);
            VisPiece("Cornice_South_GoldFrieze", PrimitiveType.Cube, archRoot.transform,
                new Vector3(-15f, vestY + studioH - 0.36f, 50.32f), new Vector3(21.6f, 0.08f, 0.18f), trimMat, layer);

            VisPiece("Cornice_North", PrimitiveType.Cube, archRoot.transform,
                new Vector3(-15f, vestY + studioH - 0.18f, 67.66f), new Vector3(21.6f, 0.32f, 0.24f), wallMat, layer);
            VisPiece("Cornice_North_GoldFrieze", PrimitiveType.Cube, archRoot.transform,
                new Vector3(-15f, vestY + studioH - 0.36f, 67.68f), new Vector3(21.6f, 0.08f, 0.18f), trimMat, layer);

            VisPiece("Cornice_East", PrimitiveType.Cube, archRoot.transform,
                new Vector3(-4.34f, vestY + studioH - 0.18f, 59f), new Vector3(0.24f, 0.32f, 17.6f), wallMat, layer);
            VisPiece("Cornice_East_GoldFrieze", PrimitiveType.Cube, archRoot.transform,
                new Vector3(-4.32f, vestY + studioH - 0.36f, 59f), new Vector3(0.18f, 0.08f, 17.6f), trimMat, layer);

            VisPiece("Cornice_West", PrimitiveType.Cube, archRoot.transform,
                new Vector3(-25.66f, vestY + studioH - 0.18f, 59f), new Vector3(0.24f, 0.32f, 17.6f), wallMat, layer);

            // Pilastras Corintias estriadas a lo largo de los muros norte, sur y este
            float[] pilasterX = { -25.2f, -20.5f, -17.6f, -13.4f, -9.6f, -4.7f };
            for (int i = 0; i < pilasterX.Length; i++)
            {
                BuildCorinthianPilaster(archRoot.transform, $"Pilaster_North_{i + 1}",
                    new Vector3(pilasterX[i], vestY, 67.64f), studioH - 0.35f, wallMat, trimMat, is1929, layer);
                BuildCorinthianPilaster(archRoot.transform, $"Pilaster_South_{i + 1}",
                    new Vector3(pilasterX[i], vestY, 50.36f), studioH - 0.35f, wallMat, trimMat, is1929, layer);
            }

            float[] eastPilasterZ = { 50.8f, 54.4f, 60.8f, 66.8f };
            for (int i = 0; i < eastPilasterZ.Length; i++)
            {
                BuildCorinthianPilaster(archRoot.transform, $"Pilaster_East_{i + 1}",
                    new Vector3(-4.36f, vestY, eastPilasterZ[i]), studioH - 0.35f, wallMat, trimMat, is1929, layer);
            }

            // Techo Artesonado Clásico (vigas longitudinales y transversales con casetones y molduras doradas)
            float[] beamX = { -23.5f, -19.5f, -15.0f, -10.5f, -6.5f };
            for (int i = 0; i < beamX.Length; i++)
            {
                VisPiece($"Ceiling_MainBeam_X_{i + 1}", PrimitiveType.Cube, archRoot.transform,
                    new Vector3(beamX[i], vestY + studioH - 0.16f, 59f), new Vector3(0.38f, 0.28f, 17.4f), wallMat, layer);
                VisPiece($"Ceiling_BeamGoldTrim_X_{i + 1}", PrimitiveType.Cube, archRoot.transform,
                    new Vector3(beamX[i], vestY + studioH - 0.31f, 59f), new Vector3(0.16f, 0.04f, 17.2f), trimMat, layer);
            }
            float[] beamZ = { 52.5f, 56.5f, 61.5f, 65.5f };
            for (int i = 0; i < beamZ.Length; i++)
            {
                VisPiece($"Ceiling_CrossBeam_Z_{i + 1}", PrimitiveType.Cube, archRoot.transform,
                    new Vector3(-15f, vestY + studioH - 0.14f, beamZ[i]), new Vector3(21.2f, 0.24f, 0.34f), wallMat, layer);
            }

            // En RUINAS: Desconchones de yeso dejando ladrillo visto, manchas de humedad, musgo y vigas del techo rotas
            if (!is1929)
            {
                // Parches de ladrillo expuesto en las paredes (Decals 3D en relieve)
                VisPiece("ExposedBrick_Patch_East_1", PrimitiveType.Cube, archRoot.transform,
                    new Vector3(-4.26f, vestY + 4.4f, 63.2f), new Vector3(0.05f, 1.8f, 2.6f), mats.exposedBrickRuins, layer);
                VisPiece("ExposedBrick_Patch_South_1", PrimitiveType.Cube, archRoot.transform,
                    new Vector3(-16.8f, vestY + 3.8f, 50.28f), new Vector3(2.4f, 1.6f, 0.05f), mats.exposedBrickRuins, layer);
                VisPiece("ExposedBrick_Patch_West_1", PrimitiveType.Cube, archRoot.transform,
                    new Vector3(-25.72f, vestY + 2.5f, 53.5f), new Vector3(0.05f, 2.2f, 2.8f), mats.exposedBrickRuins, layer);

                // Manchas de musgo y humedad ascendente en las esquinas y bajo las ventanas
                VisPiece("Moss_WallStain_North_1", PrimitiveType.Cube, archRoot.transform,
                    new Vector3(-15.5f, vestY + 0.85f, 67.62f), new Vector3(4.5f, 1.6f, 0.06f), mats.mossAndIvyRuins, layer);
                VisPiece("Moss_WallStain_North_2", PrimitiveType.Cube, archRoot.transform,
                    new Vector3(-22.8f, vestY + 0.70f, 67.62f), new Vector3(3.8f, 1.3f, 0.06f), mats.mossAndIvyRuins, layer);

                // Hueco de techo colapsado con listones de madera rotos colgando
                VisPiece("Collapsed_Ceiling_Void", PrimitiveType.Cube, archRoot.transform,
                    new Vector3(-14.8f, vestY + studioH - 0.24f, 60.2f), new Vector3(3.8f, 0.08f, 3.4f), mats.exposedBrickRuins, layer);
                for (int l = 0; l < 5; l++)
                {
                    GameObject lath = VisPiece($"Broken_Ceiling_Lath_{l + 1}", PrimitiveType.Cube, archRoot.transform,
                        new Vector3(-16.2f + l * 0.7f, vestY + studioH - 0.45f, 60.2f), new Vector3(0.12f, 0.06f, 2.8f), mats.rottedWoodRuins, layer);
                    lath.transform.localRotation = Quaternion.Euler(14f + (l % 2) * 12f, l * 8f, -18f + l * 7f);
                }
            }
        }

        private static void BuildCorinthianPilaster(Transform parent, string name, Vector3 basePos, float height, Material shaftMat, Material capMat, bool is1929, int layer)
        {
            GameObject p = CreateFolder(name, parent, layer);
            p.transform.position = basePos;

            // Plinto inferior
            VisPiece("Plinth", PrimitiveType.Cube, p.transform,
                new Vector3(0f, 0.45f, 0f), new Vector3(0.58f, 0.90f, 0.36f), shaftMat, layer);
            VisPiece("Plinth_GoldMolding", PrimitiveType.Cube, p.transform,
                new Vector3(0f, 0.94f, 0f), new Vector3(0.64f, 0.08f, 0.42f), capMat, layer);

            // Fuste acanalado (3 nervios verticales)
            float shaftH = height - 1.55f;
            VisPiece("Shaft_Core", PrimitiveType.Cube, p.transform,
                new Vector3(0f, 0.98f + shaftH * 0.5f, 0f), new Vector3(0.46f, shaftH, 0.26f), shaftMat, layer);
            for (int f = -1; f <= 1; f++)
            {
                VisPiece($"Flute_{f + 2}", PrimitiveType.Cylinder, p.transform,
                    new Vector3(f * 0.13f, 0.98f + shaftH * 0.5f, 0f), new Vector3(0.08f, shaftH * 0.5f, 0.30f), shaftMat, layer);
            }

            // Capitel Corintio escalonado en dorado (1929) o erosionado (Ruinas)
            VisPiece("Capital_Neck", PrimitiveType.Cube, p.transform,
                new Vector3(0f, height - 0.52f, 0f), new Vector3(0.54f, 0.12f, 0.36f), capMat, layer);
            VisPiece("Capital_Volutes", PrimitiveType.Cube, p.transform,
                new Vector3(0f, height - 0.32f, 0f), new Vector3(0.66f, 0.28f, 0.44f), capMat, layer);
            VisPiece("Capital_Abacus", PrimitiveType.Cube, p.transform,
                new Vector3(0f, height - 0.10f, 0f), new Vector3(0.74f, 0.14f, 0.50f), shaftMat, layer);
        }

        // ====================================================================
        // 3. VENTANALES ARQUEADOS MONUMENTALES (SOL 1929 ↔ HIEDRA Y LUNA RUINAS)
        // ====================================================================
        private static void BuildArchedWindowBay(Transform parent, int index, float wx, float vestY, float wz, EraVisual era, EraPaletteMaterials mats, int layer)
        {
            bool is1929 = era == EraVisual.Glory1929;
            GameObject winRoot = CreateFolder($"ArchedWindowBay_{index + 1}", parent, layer);
            winRoot.transform.position = new Vector3(wx, vestY, wz);

            Material frameMat = is1929 ? mats.boiserieCream1929 : mats.rottedWoodRuins;
            Material goldMat = is1929 ? mats.antiqueGold1929 : mats.rustedIronRuins;
            Material curtainMat = is1929 ? mats.velvetWine1929 : mats.tornWineVelvetRuins;

            float winW = 2.85f;
            float winBaseY = 0.85f;
            float winRectH = 3.75f;
            float archCenterY = winBaseY + winRectH;

            // Alféizar y jambas laterales moldeadas
            VisPiece("Window_Sill", PrimitiveType.Cube, winRoot.transform,
                new Vector3(0f, winBaseY - 0.08f, -0.08f), new Vector3(winW + 0.45f, 0.16f, 0.34f), frameMat, layer);
            VisPiece("Jamb_Left", PrimitiveType.Cube, winRoot.transform,
                new Vector3(-winW * 0.5f, winBaseY + winRectH * 0.5f, -0.02f), new Vector3(0.22f, winRectH, 0.24f), frameMat, layer);
            VisPiece("Jamb_Right", PrimitiveType.Cube, winRoot.transform,
                new Vector3(winW * 0.5f, winBaseY + winRectH * 0.5f, -0.02f), new Vector3(0.22f, winRectH, 0.24f), frameMat, layer);

            // Arco de medio punto superior (segmentos radiales formando el arco clásico de la referencia)
            int archSegs = 9;
            float archRadius = winW * 0.5f;
            for (int a = 0; a < archSegs; a++)
            {
                float t0 = (float)a / archSegs * Mathf.PI;
                float t1 = (float)(a + 1) / archSegs * Mathf.PI;
                float tm = (t0 + t1) * 0.5f;
                float ax = Mathf.Cos(tm) * archRadius;
                float ay = archCenterY + Mathf.Sin(tm) * (archRadius * 0.72f);
                GameObject seg = VisPiece($"Arch_Crown_{a + 1}", PrimitiveType.Cube, winRoot.transform,
                    new Vector3(ax, ay, -0.03f), new Vector3(0.55f, 0.22f, 0.26f), frameMat, layer);
                seg.transform.localRotation = Quaternion.Euler(0f, 0f, (tm * Mathf.Rad2Deg) - 90f);
            }

            // Cuadrícula de parteluces (Muntin bars verticales y horizontales)
            float[] muntinX = { -0.88f, 0f, 0.88f };
            for (int m = 0; m < muntinX.Length; m++)
            {
                GameObject vBar = VisPiece($"Muntin_V_{m + 1}", PrimitiveType.Cube, winRoot.transform,
                    new Vector3(muntinX[m], winBaseY + winRectH * 0.5f + 0.35f, 0.02f), new Vector3(0.06f, winRectH + 0.7f, 0.08f), frameMat, layer);
                if (!is1929 && m == 1 && index == 1)
                {
                    vBar.transform.localRotation = Quaternion.Euler(0f, 0f, 8f); // Parteluz doblado en ruinas
                }
            }
            for (int h = 1; h <= 5; h++)
            {
                float hy = winBaseY + h * 0.74f;
                VisPiece($"Muntin_H_{h}", PrimitiveType.Cube, winRoot.transform,
                    new Vector3(0f, hy, 0.02f), new Vector3(winW - 0.1f, 0.05f, 0.08f), frameMat, layer);
            }

            if (is1929)
            {
                // 1929: Cristales intactos bañados por el sol dorado-rosado del atardecer
                VisPiece("Sunset_Glass_Pane", PrimitiveType.Cube, winRoot.transform,
                    new Vector3(0f, winBaseY + winRectH * 0.5f, 0.05f), new Vector3(winW - 0.15f, winRectH, 0.03f), mats.sunsetGlassEmit1929, layer);
                VisPiece("Sunset_Arch_Glass", PrimitiveType.Cylinder, winRoot.transform,
                    new Vector3(0f, archCenterY, 0.05f), new Vector3(winW * 0.92f, 0.02f, winW * 0.65f), mats.sunsetGlassEmit1929, layer)
                    .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

                // Cortinajes imperiales de terciopelo vino #6B1E2E con cenefa y abrazaderas doradas #C9A24A
                VisPiece("Pelmet_GoldBox", PrimitiveType.Cube, winRoot.transform,
                    new Vector3(0f, archCenterY + 1.08f, -0.14f), new Vector3(winW + 0.75f, 0.24f, 0.28f), goldMat, layer);
                for (int side = -1; side <= 1; side += 2)
                {
                    string sName = side < 0 ? "L" : "R";
                    VisPiece($"VelvetDrape_{sName}", PrimitiveType.Cylinder, winRoot.transform,
                        new Vector3(side * (winW * 0.5f + 0.16f), winBaseY + 2.35f, -0.16f), new Vector3(0.46f, 2.45f, 0.22f), curtainMat, layer);
                    VisPiece($"GoldTieback_{sName}", PrimitiveType.Cylinder, winRoot.transform,
                        new Vector3(side * (winW * 0.5f + 0.14f), winBaseY + 1.35f, -0.16f), new Vector3(0.52f, 0.05f, 0.28f), goldMat, layer);
                }

                // Luz direccional cálida por ventana + Haces volumétricos dorados sobre el parqué
                CreateEraSpotlight($"SunSpotlight_1929_{index + 1}", winRoot.transform,
                    new Vector3(0f, 4.4f, -0.35f), new Vector3(0f, 0.1f, -9.5f),
                    Col1929WarmSun, 4.8f, 58f, 18f, layer);

                GameObject beam = VisPiece($"VolumetricSunBeam_1929_{index + 1}", PrimitiveType.Cube, winRoot.transform,
                    new Vector3(0f, 2.35f, -4.4f), new Vector3(2.45f, 0.04f, 8.6f), mats.sunBeamVolumetric1929, layer);
                beam.transform.localRotation = Quaternion.Euler(25f, 0f, 0f);
                beam.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            }
            else
            {
                // RUINAS: Cristales rotos en fragmentos triangulares, hiedra densa trepando y cortinas desgarradas
                for (int s = 0; s < 5; s++)
                {
                    float sx = -1.1f + s * 0.55f;
                    float sy = winBaseY + 0.35f + (s % 3) * 1.15f;
                    GameObject shard = VisPiece($"BrokenGlass_Shard_{s + 1}", PrimitiveType.Cube, winRoot.transform,
                        new Vector3(sx, sy, 0.03f), new Vector3(0.42f, 0.38f, 0.02f), mats.brokenMirrorRuins, layer);
                    shard.transform.localRotation = Quaternion.Euler(0f, 0f, 18f + s * 27f);
                }

                // Cortina izquierda desgarrada e inclinada; derecha reducida a un jirón colgante
                GameObject tornL = VisPiece("TornDrape_L", PrimitiveType.Cube, winRoot.transform,
                    new Vector3(-winW * 0.5f - 0.08f, winBaseY + 1.95f, -0.15f), new Vector3(0.38f, 3.1f, 0.12f), curtainMat, layer);
                tornL.transform.localRotation = Quaternion.Euler(0f, 0f, -9f);
                VisPiece("TornDrape_Shred_R", PrimitiveType.Cube, winRoot.transform,
                    new Vector3(winW * 0.5f + 0.10f, winBaseY + 3.1f, -0.15f), new Vector3(0.28f, 1.45f, 0.10f), curtainMat, layer);

                // Hiedra salvaje y raíces (#4A5240) entrando por el arco roto y cayendo hacia el suelo
                BuildWindowIvyCluster(winRoot.transform, winW, winBaseY, archCenterY, mats, layer);

                // Luz lunar fría #8FA0B8 y haz polvoriento inclinado
                CreateEraSpotlight($"MoonSpotlight_Ruins_{index + 1}", winRoot.transform,
                    new Vector3(0f, 4.4f, -0.35f), new Vector3(0f, 0.1f, -9.5f),
                    ColRuinsColdMoon, 2.8f, 54f, 17f, layer);

                GameObject coldBeam = VisPiece($"VolumetricMoonBeam_Ruins_{index + 1}", PrimitiveType.Cube, winRoot.transform,
                    new Vector3(0f, 2.35f, -4.4f), new Vector3(2.25f, 0.04f, 8.4f), mats.moonBeamVolumetricRuins, layer);
                coldBeam.transform.localRotation = Quaternion.Euler(25f, 0f, 0f);
                coldBeam.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        private static void BuildWindowIvyCluster(Transform winRoot, float winW, float winBaseY, float archCenterY, EraPaletteMaterials mats, int layer)
        {
            GameObject ivyRoot = CreateFolder("Ivy_And_Roots_Overgrowth", winRoot, layer);

            // Tallos leñosos principales serpenteando por las jambas y parteluces
            for (int v = 0; v < 4; v++)
            {
                float vx = -winW * 0.45f + v * (winW * 0.30f);
                GameObject vine = VisPiece($"Vine_Stem_{v + 1}", PrimitiveType.Cylinder, ivyRoot.transform,
                    new Vector3(vx, winBaseY + 2.1f, -0.06f), new Vector3(0.07f, 1.85f, 0.07f), mats.darkVineBarkRuins, layer);
                vine.transform.localRotation = Quaternion.Euler(4f, 0f, -12f + v * 9f);
            }

            // Frondas de hojas de hiedra colgando desde el arco superior y el alféizar
            Vector3[] clusterOffsets =
            {
                new Vector3(-1.05f, archCenterY + 0.35f, -0.10f),
                new Vector3(-0.35f, archCenterY + 0.55f, -0.12f),
                new Vector3(0.45f, archCenterY + 0.40f, -0.10f),
                new Vector3(1.10f, archCenterY + 0.15f, -0.10f),
                new Vector3(-1.15f, winBaseY + 2.40f, -0.08f),
                new Vector3(0.95f, winBaseY + 1.85f, -0.08f),
                new Vector3(-0.65f, winBaseY + 0.25f, -0.14f),
                new Vector3(0.55f, winBaseY + 0.15f, -0.16f)
            };

            for (int c = 0; c < clusterOffsets.Length; c++)
            {
                GameObject leafCluster = VisPiece($"Ivy_Foliage_{c + 1}", PrimitiveType.Sphere, ivyRoot.transform,
                    clusterOffsets[c], new Vector3(0.78f, 0.52f, 0.28f), mats.mossAndIvyRuins, layer);
                leafCluster.transform.localRotation = Quaternion.Euler(c * 13f, c * 29f, c * 17f);

                // Guirnalda colgante vertical de hiedra
                VisPiece($"Hanging_Creeper_{c + 1}", PrimitiveType.Capsule, ivyRoot.transform,
                    clusterOffsets[c] + new Vector3(0f, -0.65f, -0.03f), new Vector3(0.16f, 0.58f, 0.12f), mats.mossAndIvyRuins, layer);
            }
        }

        // ====================================================================
        // 4. ESPEJOS DEL MURO SUR Y BARRAS DE BALLET (INTACTOS ↔ ROTOS)
        // ====================================================================
        private static void BuildMirrorsAndBalletBarres(Transform parent, float vestY, EraVisual era, EraPaletteMaterials mats, int layer)
        {
            bool is1929 = era == EraVisual.Glory1929;
            GameObject group = CreateFolder("Mirrors_And_BalletBarres", parent, layer);

            Material frameWood = is1929 ? mats.mahoganyPolished1929 : mats.rottedWoodRuins;
            Material trimGold = is1929 ? mats.antiqueGold1929 : mats.rustedIronRuins;
            Material mirrorMat = is1929 ? mats.mirrorIntact1929 : mats.brokenMirrorRuins;

            float[] mirrorX = { -23.0f, -13.2f, -9.8f, -6.4f };
            for (int i = 0; i < mirrorX.Length; i++)
            {
                float mx = mirrorX[i];
                GameObject mRoot = CreateFolder($"GrandMirror_{i + 1}", group.transform, layer);
                mRoot.transform.position = new Vector3(mx, vestY + 2.35f, 50.32f);

                // Marco exterior de caoba y moldura interior de pan de oro con remate superior
                VisPiece("OuterFrame", PrimitiveType.Cube, mRoot.transform,
                    Vector3.zero, new Vector3(2.96f, 3.88f, 0.08f), frameWood, layer);
                VisPiece("GoldBevelFrame", PrimitiveType.Cube, mRoot.transform,
                    new Vector3(0f, 0f, 0.03f), new Vector3(2.76f, 3.68f, 0.05f), trimGold, layer);
                VisPiece("CrownMedallion", PrimitiveType.Cylinder, mRoot.transform,
                    new Vector3(0f, 1.98f, 0.04f), new Vector3(0.65f, 0.04f, 0.42f), trimGold, layer)
                    .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

                if (is1929)
                {
                    // Cristal de espejo prístino y apliques cálidos laterales
                    VisPiece("MirrorGlass_Pristine", PrimitiveType.Cube, mRoot.transform,
                        new Vector3(0f, 0f, 0.05f), new Vector3(2.62f, 3.52f, 0.02f), mirrorMat, layer);
                }
                else
                {
                    // Espejo envejecido y fracturado con grietas diagonales y fragmentos caídos al suelo
                    VisPiece("MirrorGlass_Tarnished", PrimitiveType.Cube, mRoot.transform,
                        new Vector3(0f, 0f, 0.05f), new Vector3(2.62f, 3.52f, 0.02f), mirrorMat, layer);

                    for (int g = 0; g < 3; g++)
                    {
                        GameObject crack = VisPiece($"Mirror_Crack_{g + 1}", PrimitiveType.Cube, mRoot.transform,
                            new Vector3(-0.35f + g * 0.4f, -0.2f + g * 0.35f, 0.065f),
                            new Vector3(1.65f, 0.025f, 0.01f), mats.exposedBrickRuins, layer);
                        crack.transform.localRotation = Quaternion.Euler(0f, 0f, -38f + g * 33f);
                    }
                }
            }

            // Barras de ballet de doble altura a lo largo de los ventanales (como en la referencia)
            BuildDetailedBarreRun(group.transform, "WindowWall_BalletBarre",
                new Vector3(-16.8f, vestY, 66.85f), 14.5f, is1929, frameWood, trimGold, layer);

            // Detalle extra de Ruinas: un tramo de barra desprendido e inclinado sobre el suelo
            if (!is1929)
            {
                GameObject fallenRail = VisPiece("Fallen_Barre_Segment", PrimitiveType.Cylinder, group.transform,
                    new Vector3(-13.5f, vestY + 0.42f, 66.45f), new Vector3(0.055f, 2.2f, 0.055f), frameWood, layer);
                fallenRail.transform.localRotation = Quaternion.Euler(0f, 8f, 78f);
            }
        }

        private static void BuildDetailedBarreRun(Transform parent, string name, Vector3 centerBase, float length, bool is1929, Material woodMat, Material metalMat, int layer)
        {
            GameObject barre = CreateFolder(name, parent, layer);
            barre.transform.position = centerBase;

            // Barra superior (1.02m) y barra inferior (0.78m) con soportes curvos de latón/hierro
            GameObject topRail = VisPiece("Top_Rail", PrimitiveType.Cylinder, barre.transform,
                new Vector3(0f, 1.02f, 0f), new Vector3(0.055f, length * 0.5f, 0.055f), woodMat, layer);
            topRail.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            GameObject lowRail = VisPiece("Low_Rail", PrimitiveType.Cylinder, barre.transform,
                new Vector3(0f, 0.76f, 0f), new Vector3(0.048f, length * 0.5f, 0.048f), woodMat, layer);
            lowRail.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            int posts = Mathf.Max(3, Mathf.RoundToInt(length / 2.4f));
            for (int i = 0; i < posts; i++)
            {
                float px = -length * 0.5f + 0.4f + i * ((length - 0.8f) / (posts - 1));
                VisPiece($"Stanchion_Post_{i + 1}", PrimitiveType.Cylinder, barre.transform,
                    new Vector3(px, 0.51f, 0f), new Vector3(0.045f, 0.51f, 0.045f), metalMat, layer);
                VisPiece($"Stanchion_FloorFlange_{i + 1}", PrimitiveType.Cylinder, barre.transform,
                    new Vector3(px, 0.02f, 0f), new Vector3(0.18f, 0.02f, 0.18f), metalMat, layer);
                VisPiece($"WallBracket_{i + 1}", PrimitiveType.Cube, barre.transform,
                    new Vector3(px, 1.02f, 0.28f), new Vector3(0.06f, 0.06f, 0.56f), metalMat, layer);
            }
        }

        // ====================================================================
        // 5. GRAN MURAL FRESCO DE LA PRIMERA BAILARINA (MURO ESTE — REFERENCIA)
        // ====================================================================
        private static void BuildBallerinaMuralFresco(Transform parent, float vestY, EraVisual era, EraPaletteMaterials mats, int layer)
        {
            bool is1929 = era == EraVisual.Glory1929;
            GameObject mural = CreateFolder("Grand_Ballerina_WallMural", parent, layer);
            // Ubicado en el paño noble del muro este (X = -4.28m, Z = 61.8m)
            mural.transform.position = new Vector3(-4.28f, vestY + 3.25f, 61.8f);

            Material stuccoMat = is1929 ? mats.boiserieCream1929 : mats.wallCrackedRuins;
            Material goldMat = is1929 ? mats.antiqueGold1929 : mats.rustedIronRuins;
            Material canvasMat = is1929 ? mats.muralFresco1929 : mats.muralCrackedRuins;

            // Marco arquitectónico monumental (3.8m ancho × 4.0m alto)
            VisPiece("Mural_StuccoBacking", PrimitiveType.Cube, mural.transform,
                Vector3.zero, new Vector3(0.06f, 4.10f, 3.90f), stuccoMat, layer);
            VisPiece("Mural_GoldInnerFrame_Top", PrimitiveType.Cube, mural.transform,
                new Vector3(-0.04f, 1.92f, 0f), new Vector3(0.08f, 0.14f, 3.66f), goldMat, layer);
            VisPiece("Mural_GoldInnerFrame_Bottom", PrimitiveType.Cube, mural.transform,
                new Vector3(-0.04f, -1.92f, 0f), new Vector3(0.08f, 0.14f, 3.66f), goldMat, layer);
            VisPiece("Mural_GoldInnerFrame_Left", PrimitiveType.Cube, mural.transform,
                new Vector3(-0.04f, 0f, -1.78f), new Vector3(0.08f, 3.84f, 0.14f), goldMat, layer);
            VisPiece("Mural_GoldInnerFrame_Right", PrimitiveType.Cube, mural.transform,
                new Vector3(-0.04f, 0f, 1.78f), new Vector3(0.08f, 3.84f, 0.14f), goldMat, layer);

            // Lienzo del fresco de la bailarina
            VisPiece("Mural_Canvas_Fresco", PrimitiveType.Cube, mural.transform,
                new Vector3(-0.05f, 0f, 0f), new Vector3(0.03f, 3.68f, 3.42f), canvasMat, layer);

            if (is1929)
            {
                // Luz cálida de galería bañando el mural en 1929
                CreateEraSpotlight("Mural_GallerySpot_1929", mural.transform,
                    new Vector3(-1.4f, 2.25f, 0f), new Vector3(0f, -0.2f, 0f),
                    new Color(1.0f, 0.86f, 0.64f), 2.8f, 65f, 7f, layer);
            }
            else
            {
                // En Ruinas: grietas profundas cruzando el fresco, desconchón de ladrillo y moldura caída
                GameObject crack1 = VisPiece("Mural_DeepCrack_1", PrimitiveType.Cube, mural.transform,
                    new Vector3(-0.07f, 0.35f, -0.25f), new Vector3(0.02f, 2.65f, 0.06f), mats.exposedBrickRuins, layer);
                crack1.transform.localRotation = Quaternion.Euler(32f, 0f, 0f);

                VisPiece("Mural_PeelingBrickPatch", PrimitiveType.Cube, mural.transform,
                    new Vector3(-0.065f, -0.95f, 0.85f), new Vector3(0.02f, 1.15f, 1.05f), mats.exposedBrickRuins, layer);
                VisPiece("Mural_MossDrip", PrimitiveType.Cube, mural.transform,
                    new Vector3(-0.065f, 1.25f, -0.60f), new Vector3(0.02f, 1.20f, 0.75f), mats.mossAndIvyRuins, layer);
            }
        }

        // ====================================================================
        // 6. LÁMPARAS DE ARAÑA IMPERIALES (BRILLANTES ↔ OXIDADAS E INCLINADAS)
        // ====================================================================
        public static void BuildChandelier(Transform parent, string name, Vector3 pos, EraVisual era, object paletteObj, int layer)
        {
            EraPaletteMaterials mats = (EraPaletteMaterials)paletteObj;
            bool is1929 = era == EraVisual.Glory1929;

            GameObject root = CreateFolder(name, parent, layer);
            root.transform.position = pos;

            Material metalMat = is1929 ? mats.antiqueGold1929 : mats.rustedIronRuins;
            Material crystalMat = is1929 ? mats.crystalBulbEmit1929 : mats.brokenMirrorRuins;

            // Florón de techo fijo
            VisPiece("Ceiling_Rosette", PrimitiveType.Cylinder, root.transform,
                new Vector3(0f, 0.58f, 0f), new Vector3(0.75f, 0.05f, 0.75f), metalMat, layer);

            // Cuerpo colgante (en Ruinas está inclinado 14° con eslabones sueltos y telarañas)
            GameObject body = CreateFolder("Chandelier_SuspendedBody", root.transform, layer);
            if (!is1929)
            {
                body.transform.localRotation = Quaternion.Euler(12f, 18f, -14f);
                body.transform.localPosition = new Vector3(0.08f, -0.22f, -0.05f);
            }

            // Cadena y columna central torneada
            VisPiece("Suspension_Chain", PrimitiveType.Cylinder, body.transform,
                new Vector3(0f, 0.30f, 0f), new Vector3(0.05f, 0.32f, 0.05f), metalMat, layer);
            VisPiece("Central_Urn_Upper", PrimitiveType.Sphere, body.transform,
                new Vector3(0f, -0.05f, 0f), new Vector3(0.32f, 0.38f, 0.32f), metalMat, layer);
            VisPiece("Central_Spindle", PrimitiveType.Cylinder, body.transform,
                new Vector3(0f, -0.42f, 0f), new Vector3(0.14f, 0.45f, 0.14f), metalMat, layer);
            VisPiece("Central_Finial_Drop", PrimitiveType.Sphere, body.transform,
                new Vector3(0f, -0.95f, 0f), new Vector3(0.24f, 0.32f, 0.24f), crystalMat, layer);

            // Dos coronas concéntricas con 12 brazos curvos, platillos y velas
            int armCount = 10;
            float radiusOuter = 1.05f;
            VisPiece("Main_Crown_Ring", PrimitiveType.Cylinder, body.transform,
                new Vector3(0f, -0.48f, 0f), new Vector3(radiusOuter * 1.45f, 0.04f, radiusOuter * 1.45f), metalMat, layer);
            VisPiece("Upper_Tier_Ring", PrimitiveType.Cylinder, body.transform,
                new Vector3(0f, -0.18f, 0f), new Vector3(radiusOuter * 0.85f, 0.03f, radiusOuter * 0.85f), metalMat, layer);

            for (int a = 0; a < armCount; a++)
            {
                float ang = (360f / armCount) * a * Mathf.Deg2Rad;
                float cx = Mathf.Cos(ang) * radiusOuter;
                float cz = Mathf.Sin(ang) * radiusOuter;

                // Brazo radial
                GameObject arm = VisPiece($"ScrollArm_{a + 1}", PrimitiveType.Cube, body.transform,
                    new Vector3(cx * 0.52f, -0.52f, cz * 0.52f), new Vector3(0.045f, 0.055f, radiusOuter * 0.92f), metalMat, layer);
                arm.transform.localRotation = Quaternion.Euler(-8f, -a * (360f / armCount) + 90f, 0f);

                // Platillo recogegotas
                VisPiece($"Bobeche_Dish_{a + 1}", PrimitiveType.Cylinder, body.transform,
                    new Vector3(cx, -0.42f, cz), new Vector3(0.16f, 0.015f, 0.16f), metalMat, layer);

                // En Ruinas faltan algunas velas o están torcidas
                if (is1929 || (a % 3 != 0))
                {
                    GameObject candle = VisPiece($"CandleSleeve_{a + 1}", PrimitiveType.Cylinder, body.transform,
                        new Vector3(cx, -0.32f, cz), new Vector3(0.045f, 0.09f, 0.045f),
                        is1929 ? mats.wallIvory1929 : mats.rottedWoodRuins, layer);
                    if (!is1929) candle.transform.localRotation = Quaternion.Euler(18f, 0f, -15f);

                    if (is1929)
                    {
                        VisPiece($"FlameBulb_{a + 1}", PrimitiveType.Sphere, body.transform,
                            new Vector3(cx, -0.20f, cz), new Vector3(0.075f, 0.12f, 0.075f), crystalMat, layer);
                    }
                }

                // Prismas de cristal colgantes
                if (is1929 || (a % 2 == 0))
                {
                    VisPiece($"CrystalPrism_{a + 1}", PrimitiveType.Capsule, body.transform,
                        new Vector3(cx * 0.86f, -0.64f, cz * 0.86f), new Vector3(0.035f, 0.10f, 0.035f), crystalMat, layer);
                }
            }

            if (is1929)
            {
                // Luz cálida envolvente de la araña de 1929
                GameObject lightObj = new GameObject("Chandelier_WarmLight_1929");
                lightObj.transform.SetParent(body.transform, false);
                lightObj.transform.localPosition = new Vector3(0f, -0.45f, 0f);
                lightObj.layer = layer;
                Light l = lightObj.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = new Color(1.0f, 0.83f, 0.54f);
                l.intensity = 3.6f;
                l.range = 16.5f;
                l.shadows = LightShadows.Soft;
            }
            else
            {
                // Telarañas colgando entre los brazos oxidados de la araña en Ruinas
                for (int w = 0; w < 4; w++)
                {
                    GameObject web = VisPiece($"Chandelier_Cobweb_{w + 1}", PrimitiveType.Cube, body.transform,
                        new Vector3(0f, -0.35f, 0f), new Vector3(1.55f, 0.45f, 0.01f), mats.cobwebTranslucentRuins, layer);
                    web.transform.localRotation = Quaternion.Euler(0f, w * 45f, 0f);
                }
            }
        }

        // ====================================================================
        // 7. MOBILIARIO EQUIVALENTE: PIANO, BANQUETAS, JARRONES Y FLORES
        // ====================================================================
        private static void BuildPianoAndFurnitureProps(Transform parent, float vestY, EraVisual era, EraPaletteMaterials mats, int layer)
        {
            bool is1929 = era == EraVisual.Glory1929;
            GameObject props = CreateFolder("Furniture_And_Props", parent, layer);

            // Jarrón de porcelana con rosas frescas (1929) vs jarrón partido y flores secas en el suelo (Ruinas)
            // Ubicado sobre la mesa auxiliar junto al piano (-6.2f, vestY + 0.84f, 65.5f)
            if (is1929)
            {
                VisPiece("Porcelain_Vase_1929", PrimitiveType.Cylinder, props.transform,
                    new Vector3(-6.45f, vestY + 1.02f, 65.75f), new Vector3(0.18f, 0.18f, 0.18f), mats.wallIvory1929, layer);
                VisPiece("Vase_GoldRim_1929", PrimitiveType.Cylinder, props.transform,
                    new Vector3(-6.45f, vestY + 1.20f, 65.75f), new Vector3(0.20f, 0.02f, 0.20f), mats.antiqueGold1929, layer);
                for (int f = 0; f < 5; f++)
                {
                    float fx = -6.45f + Mathf.Cos(f * 1.25f) * 0.08f;
                    float fz = 65.75f + Mathf.Sin(f * 1.25f) * 0.08f;
                    VisPiece($"Fresh_RoseBloom_{f + 1}", PrimitiveType.Sphere, props.transform,
                        new Vector3(fx, vestY + 1.28f + (f % 2) * 0.04f, fz), new Vector3(0.09f, 0.08f, 0.09f),
                        (f % 2 == 0) ? mats.roseLavender1929 : mats.velvetWine1929, layer);
                }

                // Candelabros de latón encendidos sobre el piano vertical (-5.2f, vestY + 1.48f, 62.5f)
                for (int c = -1; c <= 1; c += 2)
                {
                    VisPiece($"Piano_Candelabra_{c}", PrimitiveType.Cylinder, props.transform,
                        new Vector3(-5.2f, vestY + 1.56f, 62.5f + c * 0.65f), new Vector3(0.08f, 0.14f, 0.08f), mats.antiqueGold1929, layer);
                    VisPiece($"Piano_CandleFlame_{c}", PrimitiveType.Sphere, props.transform,
                        new Vector3(-5.2f, vestY + 1.72f, 62.5f + c * 0.65f), new Vector3(0.05f, 0.08f, 0.05f), mats.crystalBulbEmit1929, layer);
                }

                // Cojines de terciopelo vino sobre las sillas de la maestra
                VisPiece("VelvetCushion_Chair1", PrimitiveType.Cube, props.transform,
                    new Vector3(-5.2f, vestY + 0.56f, 59.2f), new Vector3(0.68f, 0.08f, 0.68f), mats.velvetWine1929, layer);
                VisPiece("VelvetCushion_Chair2", PrimitiveType.Cube, props.transform,
                    new Vector3(-5.2f, vestY + 0.56f, 53.2f), new Vector3(0.68f, 0.08f, 0.68f), mats.velvetWine1929, layer);
            }
            else
            {
                // En Ruinas: el mismo jarrón volcado y roto al pie de la mesa con tallos secos
                GameObject brokenVase = VisPiece("Broken_Vase_Ruins", PrimitiveType.Cylinder, props.transform,
                    new Vector3(-6.75f, vestY + 0.07f, 65.25f), new Vector3(0.16f, 0.14f, 0.16f), mats.wallCrackedRuins, layer);
                brokenVase.transform.localRotation = Quaternion.Euler(82f, 35f, 0f);
                VisPiece("Withered_Flowers_Ruins", PrimitiveType.Sphere, props.transform,
                    new Vector3(-6.60f, vestY + 0.04f, 65.10f), new Vector3(0.28f, 0.05f, 0.22f), mats.tornWineVelvetRuins, layer);

                // Tapa frontal del piano desprendida e inclinada y candelabros caídos
                GameObject fallenLid = VisPiece("Piano_FallenFrontBoard", PrimitiveType.Cube, props.transform,
                    new Vector3(-5.68f, vestY + 0.62f, 62.5f), new Vector3(0.05f, 0.65f, 1.52f), mats.rottedWoodRuins, layer);
                fallenLid.transform.localRotation = Quaternion.Euler(0f, 0f, 19f);

                // Taburete/banqueta volcada junto al piano
                GameObject overturnedStool = VisPiece("Overturned_Bench_Ruins", PrimitiveType.Cube, props.transform,
                    new Vector3(-6.45f, vestY + 0.24f, 61.4f), new Vector3(0.85f, 0.42f, 0.45f), mats.rottedWoodRuins, layer);
                overturnedStool.transform.localRotation = Quaternion.Euler(68f, -24f, 12f);
            }
        }

        // ====================================================================
        // 8. DETALLE DE SUELO: BRILLO 1929 ↔ CHARCOS, TABLAS Y ESCOMBROS RUINAS
        // ====================================================================
        private static void BuildFloorDetailsAndDebris(Transform parent, float vestY, EraVisual era, EraPaletteMaterials mats, int layer)
        {
            bool is1929 = era == EraVisual.Glory1929;
            GameObject floorGroup = CreateFolder("Floor_Details", parent, layer);

            if (is1929)
            {
                // Cenefa perimetral incrustada en latón y caoba alrededor del parqué de gala
                VisPiece("Inlay_Border_North", PrimitiveType.Cube, floorGroup.transform,
                    new Vector3(-15f, vestY + 0.005f, 66.4f), new Vector3(19.5f, 0.008f, 0.14f), mats.antiqueGold1929, layer);
                VisPiece("Inlay_Border_South", PrimitiveType.Cube, floorGroup.transform,
                    new Vector3(-15f, vestY + 0.005f, 51.6f), new Vector3(19.5f, 0.008f, 0.14f), mats.antiqueGold1929, layer);
                VisPiece("Inlay_Border_East", PrimitiveType.Cube, floorGroup.transform,
                    new Vector3(-5.3f, vestY + 0.005f, 59.0f), new Vector3(0.14f, 0.008f, 14.9f), mats.antiqueGold1929, layer);
                VisPiece("Inlay_Border_West", PrimitiveType.Cube, floorGroup.transform,
                    new Vector3(-24.7f, vestY + 0.005f, 59.0f), new Vector3(0.14f, 0.008f, 14.9f), mats.antiqueGold1929, layer);
            }
            else
            {
                // En Ruinas: Grandes charcos de agua reflectantes bajo los ventanales (como en la imagen de referencia)
                Vector3[] puddleCenters =
                {
                    new Vector3(-21.8f, vestY + 0.008f, 63.2f),
                    new Vector3(-15.2f, vestY + 0.008f, 62.4f),
                    new Vector3(-11.4f, vestY + 0.008f, 64.1f),
                    new Vector3(-16.5f, vestY + 0.008f, 55.8f)
                };
                Vector3[] puddleScales =
                {
                    new Vector3(4.2f, 0.010f, 3.4f),
                    new Vector3(5.6f, 0.010f, 4.1f),
                    new Vector3(3.5f, 0.010f, 2.8f),
                    new Vector3(4.4f, 0.010f, 3.2f)
                };
                for (int p = 0; p < puddleCenters.Length; p++)
                {
                    VisPiece($"Rainwater_Puddle_{p + 1}", PrimitiveType.Cylinder, floorGroup.transform,
                        puddleCenters[p], puddleScales[p], mats.waterPuddleRuins, layer);
                }

                // Tablones de parqué levantados y astillados
                for (int b = 0; b < 9; b++)
                {
                    float bx = -22.5f + b * 1.95f;
                    float bz = 54.0f + (b % 4) * 3.1f;
                    GameObject plank = VisPiece($"Warped_FloorPlank_{b + 1}", PrimitiveType.Cube, floorGroup.transform,
                        new Vector3(bx, vestY + 0.04f, bz), new Vector3(0.24f, 0.04f, 1.85f), mats.rottedWoodRuins, layer);
                    plank.transform.localRotation = Quaternion.Euler(4f + (b % 3) * 3f, b * 23f, -3f);
                }

                // Cascotes de yeso caídos del techo, cristales rotos y hojas secas/musgo
                for (int r = 0; r < 18; r++)
                {
                    float rx = -23.5f + (r * 1.15f) % 18.0f;
                    float rz = 52.5f + ((r * 2.7f) % 14.0f);
                    Material rMat = (r % 3 == 0) ? mats.wallCrackedRuins : ((r % 3 == 1) ? mats.mossAndIvyRuins : mats.exposedBrickRuins);
                    GameObject rubble = VisPiece($"Ceiling_Rubble_{r + 1}", PrimitiveType.Cube, floorGroup.transform,
                        new Vector3(rx, vestY + 0.07f, rz),
                        new Vector3(0.28f + (r % 3) * 0.14f, 0.11f + (r % 2) * 0.08f, 0.24f + (r % 4) * 0.12f), rMat, layer);
                    rubble.transform.localRotation = Quaternion.Euler(r * 19f, r * 47f, r * 11f);
                }
            }
        }

        // ====================================================================
        // 9. PARTÍCULAS ATMOSFÉRICAS Y REFLECTION PROBE
        // ====================================================================
        private static void BuildAtmosphereParticles(Transform parent, Vector3 centerPos, EraVisual era, EraPaletteMaterials mats, int layer)
        {
            bool is1929 = era == EraVisual.Glory1929;
            GameObject pObj = new GameObject(is1929 ? "GoldenDustMotes_1929" : "ColdDustAndSpores_Ruins");
            pObj.transform.SetParent(parent, false);
            pObj.transform.position = centerPos;
            pObj.layer = layer;

            ParticleSystem ps = pObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(7f, 13f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.03f, 0.10f);
            main.startSize = is1929
                ? new ParticleSystem.MinMaxCurve(0.025f, 0.055f)
                : new ParticleSystem.MinMaxCurve(0.035f, 0.085f);
            main.startColor = is1929
                ? new Color(1.0f, 0.86f, 0.56f, 0.42f)
                : new Color(0.62f, 0.70f, 0.78f, 0.35f);
            main.maxParticles = is1929 ? 140 : 220;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = is1929 ? 16f : 26f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(19f, 5.2f, 15f);

            var psRenderer = pObj.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null)
            {
                psRenderer.sharedMaterial = is1929 ? mats.sunBeamVolumetric1929 : mats.moonBeamVolumetricRuins;
                psRenderer.shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        private static void BuildStudioReflectionProbe(Transform zone16, Vector3 pos)
        {
            Transform existing = zone16.Find("ReflectionProbe_Zone16");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            GameObject probeObj = new GameObject("ReflectionProbe_Zone16");
            probeObj.transform.SetParent(zone16, false);
            probeObj.transform.position = pos;

            ReflectionProbe probe = probeObj.AddComponent<ReflectionProbe>();
            probe.mode = ReflectionProbeMode.Realtime;
            probe.refreshMode = ReflectionProbeRefreshMode.OnAwake;
            probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
            probe.size = new Vector3(23f, 7.2f, 19f);
            probe.boxProjection = true;
            probe.intensity = 1.15f;
            probe.resolution = 256;
        }

        // ====================================================================
        // 10. TEXTURAS PROCEDURALES PBR ADICIONALES Y MATERIALES POR ÉPOCA
        // ====================================================================
        private static void EnsureDualEraProceduralTextures()
        {
            if (!Directory.Exists(EraTexDir)) Directory.CreateDirectory(EraTexDir);

            if (!File.Exists(EraTexDir + "Tex_Parquet_Albedo.png"))
            {
                ProceduralTextureGenerator.GenerateAllTextures();
            }

            // 1. Textura 1929: Estuco Veneciano y Damasco de Seda Imperial (sin cuadrados naranjas)
            string palaceWallAlb = EraTexDir + "Tex_PalaceWall1929_Albedo.png";
            string palaceWallNrm = EraTexDir + "Tex_PalaceWall1929_Normal.png";
            if (!File.Exists(palaceWallAlb) || !File.Exists(palaceWallNrm))
            {
                int size = 512;
                Texture2D albedo = new Texture2D(size, size, TextureFormat.RGBA32, true);
                Texture2D normal = new Texture2D(size, size, TextureFormat.RGBA32, true);
                float[,] hMap = new float[size, size];

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float u = (float)x / size;
                        float v = (float)y / size;

                        // Sutil tejido de damasco imperial vertical y estuco perlado
                        float stripe = Mathf.Sin(u * Mathf.PI * 16f) * 0.5f + 0.5f;
                        float damask = Mathf.Sin(u * Mathf.PI * 32f) * Mathf.Cos(v * Mathf.PI * 32f);
                        damask = Mathf.SmoothStep(-0.2f, 0.8f, damask);
                        float micro = Mathf.PerlinNoise(u * 24f, v * 24f) * 0.08f;

                        // Franja inferior de boiserie noble (v < 0.24)
                        float wainscot = v < 0.24f ? 1f : 0f;
                        float moldingLine = Mathf.SmoothStep(0.025f, 0.0f, Mathf.Abs(v - 0.24f));

                        hMap[x, y] = 0.5f + damask * 0.14f + stripe * 0.06f + moldingLine * 0.35f + micro;

                        Color silkIvory = new Color(0.95f, 0.91f, 0.85f);
                        Color silkChampagne = new Color(0.89f, 0.83f, 0.75f);
                        Color dadoWalnut = new Color(0.82f, 0.75f, 0.66f);
                        Color goldThread = new Color(0.78f, 0.63f, 0.32f);

                        Color wallCol = Color.Lerp(silkChampagne, silkIvory, damask * 0.65f + stripe * 0.25f);
                        wallCol = Color.Lerp(wallCol, dadoWalnut, wainscot * 0.55f);
                        wallCol = Color.Lerp(wallCol, goldThread, moldingLine * 0.85f);
                        albedo.SetPixel(x, y, wallCol);
                    }
                }

                BakeNormalMap(hMap, normal, 1.6f);
                SaveTexAsset(albedo, palaceWallAlb, false);
                SaveTexAsset(normal, palaceWallNrm, true);
            }

            // 2. Textura 1929: Parqué Imperial Miel-Dorado (cálido y rico en veta, sin reflejo azul)
            string palaceParquetAlb = EraTexDir + "Tex_PalaceParquet1929_Albedo.png";
            if (!File.Exists(palaceParquetAlb))
            {
                int size = 512;
                Texture2D albedo = new Texture2D(size, size, TextureFormat.RGBA32, true);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float plankW = 32f;
                        float plankL = 128f;
                        int row = Mathf.FloorToInt(y / plankW);
                        float xOff = (row % 2) * (plankL * 0.5f);
                        int col = Mathf.FloorToInt((x + xOff) / plankL);
                        float uP = ((x + xOff) % plankL) / plankL;
                        float vP = (y % plankW) / plankW;

                        float edge = Mathf.SmoothStep(0f, 0.06f, uP) * Mathf.SmoothStep(0f, 0.06f, 1f - uP)
                                   * Mathf.SmoothStep(0f, 0.10f, vP) * Mathf.SmoothStep(0f, 0.10f, 1f - vP);
                        float grain = Mathf.PerlinNoise((x + xOff) * 0.03f, y * 0.18f) * 0.22f;
                        float varTint = Mathf.Sin(row * 31f + col * 19f) * 0.06f;

                        Color warmOak = new Color(0.62f + varTint, 0.42f + varTint * 0.7f, 0.24f + varTint * 0.4f);
                        Color groove = new Color(0.24f, 0.14f, 0.07f);
                        albedo.SetPixel(x, y, Color.Lerp(groove, warmOak * (1f - grain * 0.6f), edge));
                    }
                }
                SaveTexAsset(albedo, palaceParquetAlb, false);
            }

            // 3. Textura Ruinas de Terror: Yeso Desgarrado, Grietas Negras Profundas, Moho y Humedad Sangrante
            string horrorPlasterAlb = EraTexDir + "Tex_HorrorRuinsWall_Albedo.png";
            string horrorPlasterNrm = EraTexDir + "Tex_HorrorRuinsWall_Normal.png";
            if (!File.Exists(horrorPlasterAlb) || !File.Exists(horrorPlasterNrm))
            {
                int size = 512;
                Texture2D albedo = new Texture2D(size, size, TextureFormat.RGBA32, true);
                Texture2D normal = new Texture2D(size, size, TextureFormat.RGBA32, true);
                float[,] hMap = new float[size, size];

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float u = (float)x / size;
                        float v = (float)y / size;

                        float baseNoise = Mathf.PerlinNoise(u * 8f, v * 8f);
                        float grit = Mathf.PerlinNoise(u * 36f, v * 36f) * 0.4f;

                        // Grietas negras profundas ramificadas
                        float w1 = Mathf.Abs(Mathf.Sin(u * 11f + Mathf.PerlinNoise(u * 16f, v * 16f) * 2.8f));
                        float w2 = Mathf.Abs(Mathf.Cos(v * 9f + Mathf.PerlinNoise(u * 12f + 4f, v * 12f) * 2.5f));
                        float crackMask = Mathf.Clamp01(Mathf.SmoothStep(0.09f, 0.0f, w1) + Mathf.SmoothStep(0.07f, 0.0f, w2));

                        // Chorretones verticales de humedad oscura desde el techo (v cerca de 1) y moho desde el suelo (v cerca de 0)
                        float dripNoise = Mathf.PerlinNoise(u * 22f, v * 3.5f);
                        float topDrip = Mathf.SmoothStep(0.45f, 0.95f, v + dripNoise * 0.45f);
                        float bottomMold = Mathf.SmoothStep(0.55f, 0.05f, v - dripNoise * 0.35f);

                        // Desconchones de ladrillo oscuro expuesto
                        float brickExpose = Mathf.SmoothStep(0.62f, 0.74f, Mathf.PerlinNoise(u * 5.5f + 2.1f, v * 5.5f + 3.7f));

                        float h = 0.70f + grit * 0.25f - crackMask * 0.65f - brickExpose * 0.35f;
                        hMap[x, y] = h;

                        Color deadPlaster = Color.Lerp(new Color(0.28f, 0.26f, 0.24f), new Color(0.18f, 0.17f, 0.16f), baseNoise);
                        Color darkBrick = new Color(0.19f, 0.11f, 0.10f);
                        Color blackMold = new Color(0.10f, 0.13f, 0.09f);
                        Color abyssCrack = new Color(0.04f, 0.04f, 0.05f);

                        Color pixel = Color.Lerp(deadPlaster, darkBrick, brickExpose * 0.85f);
                        pixel = Color.Lerp(pixel, blackMold, Mathf.Clamp01(topDrip * 0.7f + bottomMold * 0.85f));
                        pixel = Color.Lerp(pixel, abyssCrack, crackMask * 0.95f);
                        albedo.SetPixel(x, y, pixel);
                    }
                }

                BakeNormalMap(hMap, normal, 3.8f);
                SaveTexAsset(albedo, horrorPlasterAlb, false);
                SaveTexAsset(normal, horrorPlasterNrm, true);
            }
        }

        private static EraPaletteMaterials LoadOrCreateEraMaterials()
        {
            if (!Directory.Exists(EraMatDir)) Directory.CreateDirectory(EraMatDir);

            EraPaletteMaterials m = new EraPaletteMaterials();

            // 1929 (Gloria — Palacio Mágico y Misterioso)
            m.parquet1929 = BuildPBRMat("Mat_Era1929_ParquetGlory",
                new Color(0.95f, 0.82f, 0.65f), "Tex_PalaceParquet1929_Albedo.png", "Tex_Parquet_Normal.png",
                new Vector2(6f, 6f), 0.72f, 0.04f);

            m.wallIvory1929 = BuildPBRMat("Mat_Era1929_WallIvory",
                Col1929Ivory, "Tex_PalaceWall1929_Albedo.png", "Tex_PalaceWall1929_Normal.png",
                new Vector2(5f, 2f), 0.38f, 0.0f);

            m.boiserieCream1929 = BuildPBRMat("Mat_Era1929_BoiserieCream",
                new Color(0.94f, 0.89f, 0.82f), "Tex_PalaceWall1929_Albedo.png", "Tex_PalaceWall1929_Normal.png",
                new Vector2(3f, 1.5f), 0.46f, 0.02f);

            m.antiqueGold1929 = BuildPBRMat("Mat_Era1929_AntiqueGold",
                new Color(0.74f, 0.56f, 0.24f), null, "Tex_Marble_Normal.png",
                new Vector2(2f, 2f), 0.70f, 0.62f);

            m.mahoganyPolished1929 = BuildPBRMat("Mat_Era1929_PolishedMahogany",
                new Color(0.46f, 0.22f, 0.14f), "Tex_Mahogany_Albedo.png", "Tex_Mahogany_Normal.png",
                new Vector2(3f, 3f), 0.64f, 0.03f);

            m.velvetWine1929 = BuildPBRMat("Mat_Era1929_VelvetWine",
                Col1929Wine, "Tex_VelvetCarpet_Albedo.png", "Tex_VelvetCarpet_Normal.png",
                new Vector2(3f, 3f), 0.22f, 0.02f);

            m.roseLavender1929 = BuildPBRMat("Mat_Era1929_RoseLavender",
                Col1929RoseLavender, null, "Tex_VelvetCarpet_Normal.png",
                new Vector2(2f, 2f), 0.48f, 0.0f);

            m.mirrorIntact1929 = BuildPBRMat("Mat_Era1929_MirrorIntact",
                new Color(0.90f, 0.93f, 0.96f), null, null,
                Vector2.one, 0.96f, 0.92f);

            m.sunsetGlassEmit1929 = BuildEmissiveMat("Mat_Era1929_SunsetGlass",
                new Color(1.0f, 0.82f, 0.64f), new Color(1.0f, 0.68f, 0.42f) * 2.2f, 0.88f);

            m.crystalBulbEmit1929 = BuildEmissiveMat("Mat_Era1929_CrystalBulb",
                new Color(1.0f, 0.90f, 0.70f), new Color(1.0f, 0.78f, 0.42f) * 2.8f, 0.92f);

            m.sunBeamVolumetric1929 = BuildTransparentBeamMat("Mat_Era1929_SunBeamVolumetric",
                new Color(1.0f, 0.76f, 0.52f, 0.08f), new Color(1.0f, 0.70f, 0.42f) * 0.55f);

            m.muralFresco1929 = BuildPortraitFrescoMat("Mat_Era1929_MuralFresco",
                "Assets/_Project/Textures/Portraits/Elena_Vesper_Portrait.jpg",
                new Color(0.98f, 0.90f, 0.82f), 0.42f);

            // RUINAS (Actualidad — Película de Terror Psicológico)
            m.parquetRuins = BuildPBRMat("Mat_EraRuins_ParquetDecayed",
                new Color(0.19f, 0.17f, 0.15f), "Tex_Parquet_Albedo.png", "Tex_Parquet_Normal.png",
                new Vector2(7f, 7f), 0.22f, 0.02f);

            m.wallCrackedRuins = BuildPBRMat("Mat_EraRuins_CrackedPlaster",
                new Color(0.75f, 0.73f, 0.70f), "Tex_HorrorRuinsWall_Albedo.png", "Tex_HorrorRuinsWall_Normal.png",
                new Vector2(3.5f, 1.8f), 0.12f, 0.0f);

            m.exposedBrickRuins = BuildPBRMat("Mat_EraRuins_ExposedBrick",
                new Color(0.24f, 0.14f, 0.12f), "Tex_Cobble_Albedo.png", "Tex_Cobble_Normal.png",
                new Vector2(6f, 6f), 0.10f, 0.0f);

            m.rustedIronRuins = BuildPBRMat("Mat_EraRuins_RustedIron",
                new Color(0.22f, 0.18f, 0.16f), "Tex_LockerMetal_Albedo.png", "Tex_LockerMetal_Normal.png",
                new Vector2(3f, 3f), 0.20f, 0.62f);

            m.rottedWoodRuins = BuildPBRMat("Mat_EraRuins_RottedWood",
                ColRuinsDesatBrown, "Tex_Mahogany_Albedo.png", "Tex_Mahogany_Normal.png",
                new Vector2(3f, 3f), 0.15f, 0.0f);

            m.tornWineVelvetRuins = BuildPBRMat("Mat_EraRuins_TornWineVelvet",
                ColRuinsDullWine, "Tex_VelvetCarpet_Albedo.png", "Tex_VelvetCarpet_Normal.png",
                new Vector2(3f, 3f), 0.10f, 0.0f);

            m.mossAndIvyRuins = BuildPBRMat("Mat_EraRuins_MossAndIvy",
                ColRuinsMoss, "Tex_Facade_Albedo.png", "Tex_Facade_Normal.png",
                new Vector2(4f, 4f), 0.18f, 0.0f);

            m.darkVineBarkRuins = BuildPBRMat("Mat_EraRuins_VineBark",
                new Color(0.18f, 0.16f, 0.13f), "Tex_Mahogany_Albedo.png", "Tex_Mahogany_Normal.png",
                new Vector2(2f, 6f), 0.14f, 0.0f);

            m.brokenMirrorRuins = BuildPBRMat("Mat_EraRuins_BrokenMirror",
                new Color(0.45f, 0.50f, 0.54f), "Tex_Marble_Albedo.png", "Tex_Marble_Normal.png",
                new Vector2(2f, 2f), 0.72f, 0.68f);

            m.waterPuddleRuins = BuildPBRMat("Mat_EraRuins_WaterPuddle",
                new Color(0.10f, 0.13f, 0.16f), null, "Tex_Marble_Normal.png",
                new Vector2(2f, 2f), 0.97f, 0.22f);

            m.moonBeamVolumetricRuins = BuildTransparentBeamMat("Mat_EraRuins_MoonBeamVolumetric",
                new Color(0.56f, 0.65f, 0.76f, 0.07f), ColRuinsColdMoon * 0.42f);

            m.cobwebTranslucentRuins = BuildTransparentBeamMat("Mat_EraRuins_CobwebTranslucent",
                new Color(0.72f, 0.74f, 0.76f, 0.16f), Color.black);

            m.muralCrackedRuins = BuildPortraitFrescoMat("Mat_EraRuins_MuralCracked",
                "Assets/_Project/Textures/Portraits/Elena_Vesper_Portrait.jpg",
                new Color(0.46f, 0.44f, 0.42f), 0.14f);

            return m;
        }

        // ====================================================================
        // HELPERS DE MATERIALES, CAPAS Y PRIMITIVAS VISUALES (SIN COLLIDERS)
        // ====================================================================
        private static GameObject CreateFolder(string name, Transform parent, int layer)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.layer = layer;
            return go;
        }

        private static GameObject VisPiece(string name, PrimitiveType type, Transform parent, Vector3 localPos, Vector3 localScale, Material mat, int layer)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            Collider col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            go.layer = layer;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = localScale;

            if (mat != null)
            {
                Renderer r = go.GetComponent<Renderer>();
                if (r != null) r.sharedMaterial = mat;
            }
            return go;
        }

        private static Light CreateEraSpotlight(string name, Transform parent, Vector3 localPos, Vector3 targetLocalPos, Color color, float intensity, float spotAngle, float range, int layer)
        {
            GameObject lObj = new GameObject(name);
            lObj.transform.SetParent(parent, false);
            lObj.transform.localPosition = localPos;
            lObj.transform.localRotation = Quaternion.LookRotation((targetLocalPos - localPos).normalized, Vector3.up);
            lObj.layer = layer;

            Light l = lObj.AddComponent<Light>();
            l.type = LightType.Spot;
            l.color = color;
            l.intensity = intensity;
            l.spotAngle = spotAngle;
            l.innerSpotAngle = spotAngle * 0.55f;
            l.range = range;
            l.shadows = LightShadows.None;
            return l;
        }

        private static void AssignVisualLayerAndStaticFlags(Transform root, int layer)
        {
            if (root == null) return;
            root.gameObject.layer = layer;
            StaticEditorFlags flags = StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic;
            GameObjectUtility.SetStaticEditorFlags(root.gameObject, flags);

            foreach (Transform child in root)
            {
                AssignVisualLayerAndStaticFlags(child, layer);
            }
        }

        private static Material BuildPBRMat(string name, Color tint, string albedoFile, string normalFile, Vector2 tiling, float smoothness, float metallic)
        {
            string fullPath = EraMatDir + name + ".mat";
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

            mat.SetColor("_BaseColor", tint);
            mat.color = tint;
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);
            mat.enableInstancing = true;

            if (!string.IsNullOrEmpty(albedoFile))
            {
                Texture2D alb = AssetDatabase.LoadAssetAtPath<Texture2D>(EraTexDir + albedoFile);
                if (alb != null)
                {
                    mat.SetTexture("_BaseMap", alb);
                    mat.SetTextureScale("_BaseMap", tiling);
                }
            }
            if (!string.IsNullOrEmpty(normalFile))
            {
                Texture2D nrm = AssetDatabase.LoadAssetAtPath<Texture2D>(EraTexDir + normalFile);
                if (nrm != null)
                {
                    mat.SetTexture("_BumpMap", nrm);
                    mat.SetTextureScale("_BumpMap", tiling);
                    mat.EnableKeyword("_NORMALMAP");
                }
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material BuildEmissiveMat(string name, Color baseCol, Color emissionCol, float smoothness)
        {
            string fullPath = EraMatDir + name + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(fullPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, fullPath);
            }

            mat.SetColor("_BaseColor", baseCol);
            mat.color = baseCol;
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetColor("_EmissionColor", emissionCol);
            mat.EnableKeyword("_EMISSION");
            mat.enableInstancing = true;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material BuildTransparentBeamMat(string name, Color baseAlphaCol, Color emissionCol)
        {
            string fullPath = EraMatDir + name + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(fullPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");

            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, fullPath);
            }
            else
            {
                mat.shader = shader;
            }

            Color beamCol = new Color(
                Mathf.Clamp01(baseAlphaCol.r + emissionCol.r * 0.25f),
                Mathf.Clamp01(baseAlphaCol.g + emissionCol.g * 0.25f),
                Mathf.Clamp01(baseAlphaCol.b + emissionCol.b * 0.25f),
                baseAlphaCol.a);

            mat.SetColor("_BaseColor", beamCol);
            mat.color = beamCol;
            mat.SetFloat("_Surface", 1f); // Transparent en URP
            mat.SetFloat("_Blend", 0f);   // Alpha blend
            mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = (int)RenderQueue.Transparent;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.enableInstancing = true;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material BuildPortraitFrescoMat(string name, string texPath, Color tint, float smoothness)
        {
            string fullPath = EraMatDir + name + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(fullPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, fullPath);
            }

            mat.SetColor("_BaseColor", tint);
            mat.color = tint;
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", 0f);
            mat.enableInstancing = true;

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex != null) mat.SetTexture("_BaseMap", tex);

            Texture2D nrm = AssetDatabase.LoadAssetAtPath<Texture2D>(EraTexDir + "Tex_WallIvory_Normal.png");
            if (nrm != null)
            {
                mat.SetTexture("_BumpMap", nrm);
                mat.EnableKeyword("_NORMALMAP");
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void EnsureEraLayersInTagManager()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");
            if (layersProp == null) return;

            bool dirty = false;
            SerializedProperty l6 = layersProp.GetArrayElementAtIndex(EraVisualManager.FallbackLayer1929);
            if (l6 != null && l6.stringValue != EraVisualManager.LayerName1929)
            {
                l6.stringValue = EraVisualManager.LayerName1929;
                dirty = true;
            }
            SerializedProperty l7 = layersProp.GetArrayElementAtIndex(EraVisualManager.FallbackLayerRuins);
            if (l7 != null && l7.stringValue != EraVisualManager.LayerNameRuins)
            {
                l7.stringValue = EraVisualManager.LayerNameRuins;
                dirty = true;
            }

            if (dirty)
            {
                tagManager.ApplyModifiedProperties();
            }
        }

        private static void BakeNormalMap(float[,] heights, Texture2D normalTex, float strength)
        {
            int width = normalTex.width;
            int height = normalTex.height;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int xL = (x - 1 + width) % width;
                    int xR = (x + 1) % width;
                    int yD = (y - 1 + height) % height;
                    int yU = (y + 1) % height;
                    float dX = (heights[xR, y] - heights[xL, y]) * strength;
                    float dY = (heights[x, yU] - heights[x, yD]) * strength;
                    Vector3 n = new Vector3(-dX, -dY, 1f).normalized;
                    normalTex.SetPixel(x, y, new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f));
                }
            }
        }

        private static void SaveTexAsset(Texture2D tex, string fullPath, bool isNormal)
        {
            tex.Apply();
            File.WriteAllBytes(fullPath, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);
            TextureImporter imp = AssetImporter.GetAtPath(fullPath) as TextureImporter;
            if (imp != null)
            {
                imp.wrapMode = TextureWrapMode.Repeat;
                imp.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                if (!isNormal) imp.sRGBTexture = true;
                imp.SaveAndReimport();
            }
        }
    }
}
#endif
