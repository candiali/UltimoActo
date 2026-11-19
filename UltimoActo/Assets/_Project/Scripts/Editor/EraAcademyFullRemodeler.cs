#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UltimoActo.Core;
using UltimoActo.Interaction;
using UltimoActo.Narrative;

namespace UltimoActo.Editor
{
    /// <summary>
    /// Remodelador visual integral de las 12 Secciones y 59 Zonas de la Academia Vesper
    /// en las dos épocas simultáneas:
    ///   - Época 1929 (Gloria): Palacio Imperial mágico y misterioso (claroscuro de velas doradas,
    ///     damasco de seda marfil #F3E9DC, bronce dorado envejecido #C9A24A, terciopelo vino #6B1E2E,
    ///     rosa-lavanda #C9A6C9, tapetes bordados, libros encuadernados y apliques cálidos).
    ///   - Época Ruinas (Actualidad): Película de terror psicológico (penumbra abismal, luces que
    ///     parpadean y se apagan, grietas negras profundas, ladrillo sangrante de humedad, vigas
    ///     derrumbadas, figuras cubiertas por sudarios en las esquinas, raíces, charcos y telarañas).
    /// No altera colisiones, puertas, puzles ni la lógica de NightManager.
    /// </summary>
    public static class EraAcademyFullRemodeler
    {
        private const string EraMatDir = "Assets/_Project/Materials/";
        private const string EraTexDir = "Assets/_Project/Textures/PBR/";

        public struct MasterEraPalette
        {
            // 1929 (Palacio Mágico y Misterioso)
            public Material parquet1929;
            public Material marble1929;
            public Material travertine1929;
            public Material porcelainTile1929;
            public Material wallIvory1929;
            public Material boiserieCream1929;
            public Material antiqueGold1929;
            public Material mahogany1929;
            public Material velvetWine1929;
            public Material velvetEmerald1929;
            public Material roseLavender1929;
            public Material mirrorIntact1929;
            public Material warmBulbEmit1929;
            public Material sunsetGlassEmit1929;
            public Material sunBeamVolumetric1929;
            public Material crystalWater1929;
            public Material fireGlowEmit1929;

            // Ruinas (Película de Terror Psicológico)
            public Material parquetRuins;
            public Material marbleCrackedRuins;
            public Material mossyCobbleRuins;
            public Material crackedTileRuins;
            public Material wallCrackedRuins;
            public Material exposedBrickRuins;
            public Material abyssCrackRuins;
            public Material rustedIronRuins;
            public Material rottedWoodRuins;
            public Material tornWineVelvetRuins;
            public Material tornGreenVelvetRuins;
            public Material shroudClothRuins;
            public Material mossAndIvyRuins;
            public Material darkVineBarkRuins;
            public Material brokenMirrorRuins;
            public Material waterPuddleRuins;
            public Material moonBeamVolumetricRuins;
            public Material cobwebTranslucentRuins;
        }

        private static readonly string[] All59ZoneRootNames =
        {
            "1_Exterior_Gate_Area",
            "2_Entrance_Courtyard",
            "3_Grand_Staircase_And_Portico",
            "4_Reception_Vestibule",
            "5_Grand_Hallway",
            "6_Old_Reception",
            "7_Administration_Office",
            "8_Record_Archives",
            "9_Waiting_Room",
            "10_Portrait_Gallery_Hallway",
            "11_Female_Dressing_Room",
            "12_Vanity_Mirror_Room",
            "13_Personal_Lockers",
            "14_Elena_Private_Room",
            "15_Vintage_Washrooms_And_Showers",
            "16_Main_Ballet_Studio",
            "17_Mirror_Posture_Studio",
            "18_Mannequin_And_Stage_Costumes",
            "19_Solo_Training_Room",
            "20_Service_Corridor",
            "21_Music_Library",
            "22_Grand_Piano_Room",
            "23_Sheet_Music_Archive",
            "24_Recording_Booth",
            "25_Instrument_Storage",
            "26_Mara_Dressing_Room",
            "27_Elena_Dressing_Room",
            "28_Backstage_Corridor",
            "29_Scenery_Storage",
            "30_Pulley_Mechanism_Room",
            "31_Auditorium_Seating",
            "32_Main_Stage",
            "33_Orchestra_Pit",
            "34_Lighting_Booth",
            "35_Upper_Catwalk",
            "36_Hidden_Staircase",
            "37_Sealed_Storage_Depot",
            "38_Forbidden_Archives_Room",
            "39_Chamber_Behind_Stage",
            "40_Winter_Garden_Conservatory",
            "41_Maintenance_Basement_Subterranean",
            "42_Rooftop_Terrace_And_Overlook",
            "43_Old_Infirmary",
            "44_Memorial_Hall",
            "45_Boarding_Students_Dormitory",
            "46_Cobbler_And_Scenic_Dye_Workshop",
            "47_Old_Dining_Hall_And_Patronage_Salon",
            "48_Isadora_Private_Suite_And_Spy_Lattice",
            "49_Mezzanine_Imperial_Gallery",
            "50_Theory_And_Anatomy_Amphitheatre",
            "51_Professors_And_Maestro_Krol_Residence",
            "52_Clock_Tower_And_High_Attic",
            "53_Boiler_Room_And_Electrical_Generators",
            "54_Subterranean_Laundry_And_Dumbwaiter",
            "55_Understage_Pit_And_Trapdoor",
            "56_Ancient_Crypt_And_Founders_Vault",
            "57_Carriage_House_And_North_Courtyard",
            "58_Gatehouse_And_Exterior_Switchboard",
            "59_Frozen_Swan_Pond_And_Park_Gazebo"
        };

        [MenuItem("Tools/Último Acto/Épocas (1929 vs Ruinas)/1. Enfocar Fachada y Patio (Sección 1 — Zonas 1-5)")]
        public static void FocusSection1Facade() => FocusSceneCamera(new Vector3(0f, 3.2f, -24.5f), new Vector3(0f, 5.5f, -4.0f));

        [MenuItem("Tools/Último Acto/Épocas (1929 vs Ruinas)/2. Enfocar Vestíbulo y Administración (Sección 2 — Zonas 6-10)")]
        public static void FocusSection2Admin() => FocusSceneCamera(new Vector3(0f, 4.4f, 11.5f), new Vector3(0f, 4.2f, 25.0f));

        [MenuItem("Tools/Último Acto/Épocas (1929 vs Ruinas)/3. Enfocar Vestuarios y Tocadores (Sección 3 — Zonas 11-15)")]
        public static void FocusSection3Dressing() => FocusSceneCamera(new Vector3(19.0f, 4.1f, 34.0f), new Vector3(19.0f, 3.8f, 44.0f));

        [MenuItem("Tools/Último Acto/Épocas (1929 vs Ruinas)/4. Enfocar Salón de Ensayo Piloto (Sección 4 — Zonas 16-20)")]
        public static void FocusSection4Rehearsal() => EraPilotStudioBuilder.MenuFocusPilotZone16();

        [MenuItem("Tools/Último Acto/Épocas (1929 vs Ruinas)/5. Enfocar Archivo Musical y Biblioteca (Sección 5 — Zonas 21-25)")]
        public static void FocusSection5Music() => FocusSceneCamera(new Vector3(9.5f, 4.1f, 70.5f), new Vector3(17.5f, 3.8f, 73.0f));

        [MenuItem("Tools/Último Acto/Épocas (1929 vs Ruinas)/6. Enfocar Camerinos de Mara y Elena (Sección 6 — Zonas 26-30)")]
        public static void FocusSection6Backstage() => FocusSceneCamera(new Vector3(35.2f, 4.1f, 84.8f), new Vector3(37.5f, 4.0f, 91.2f));

        [MenuItem("Tools/Último Acto/Épocas (1929 vs Ruinas)/7. Enfocar Gran Teatro y Escenario (Sección 7 — Zonas 31-35)")]
        public static void FocusSection7Theatre() => FocusSceneCamera(new Vector3(-8.0f, 4.8f, 136.5f), new Vector3(-8.0f, 5.2f, 108.0f));

        [MenuItem("Tools/Último Acto/Épocas (1929 vs Ruinas)/8. Enfocar Jardín de Invierno (Sección 9 — Zona 40)")]
        public static void FocusSection9WinterGarden() => FocusSceneCamera(new Vector3(58.0f, 4.5f, 44.0f), new Vector3(69.0f, 4.8f, 57.0f));

        private static void FocusSceneCamera(Vector3 camPos, Vector3 lookTarget)
        {
            Quaternion rot = Quaternion.LookRotation((lookTarget - camPos).normalized, Vector3.up);
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.LookAtDirect(lookTarget, rot, Vector3.Distance(camPos, lookTarget));
                SceneView.lastActiveSceneView.Repaint();
            }
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                player.transform.position = camPos;
                Vector3 flatDir = lookTarget - camPos;
                flatDir.y = 0f;
                if (flatDir.sqrMagnitude > 0.01f) player.transform.rotation = Quaternion.LookRotation(flatDir.normalized, Vector3.up);
                if (cc != null) cc.enabled = true;
            }
        }

        public static void RemodelAllSectionsAndZones(Transform rootEnv, float vestY)
        {
            if (rootEnv == null) return;

            // Desactivar el skybox azul por defecto en la escena guardada para evitar reflejos azules en suelos y mesas
            RenderSettings.skybox = null;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.reflectionIntensity = 0.35f;

            MasterEraPalette mats = BuildMasterPalette();
            int layer1929 = EraVisualManager.GetEra1929Layer();
            int layerRuins = EraVisualManager.GetEraRuinsLayer();

            // Recopilar las posiciones de TODAS las puertas de la academia para no colocar jamás molduras o props sobre una puerta
            List<Vector3> allDoorCenters = CollectAllDoorPositions(rootEnv);

            Dictionary<string, Transform> zoneMap = new Dictionary<string, Transform>();
            foreach (string zName in All59ZoneRootNames)
            {
                Transform zTrans = FindTransformRecursive(rootEnv, zName);
                if (zTrans != null)
                {
                    zoneMap[zName] = zTrans;
                }
            }

            int zoneNumber = 0;
            foreach (string zName in All59ZoneRootNames)
            {
                zoneNumber++;
                if (!zoneMap.TryGetValue(zName, out Transform zTrans) || zTrans == null) continue;

                if (zName == "16_Main_Ballet_Studio")
                {
                    EnhanceZoneInteractablesDualEra(zTrans, mats, layer1929, layerRuins);
                    continue;
                }

                RemodelZoneBaseAndPerimeter(zTrans, zoneNumber, allDoorCenters, mats, layer1929, layerRuins);
            }

            // Escenografía Heroica por Sección (Secciones 1 a 12)
            BuildSection1ExteriorAndEntranceHero(zoneMap, vestY, mats, layer1929, layerRuins);
            BuildSection2ReceptionAndAdminHero(zoneMap, vestY, mats, layer1929, layerRuins);
            BuildSection3StudentDressingAndRoomsHero(zoneMap, vestY, mats, layer1929, layerRuins);
            BuildSection4RehearsalStudiosHero(zoneMap, vestY, mats, layer1929, layerRuins);
            BuildSection5MusicArchiveHero(zoneMap, vestY, mats, layer1929, layerRuins);
            BuildSection6DressingRoomsAndBackstageHero(zoneMap, vestY, mats, layer1929, layerRuins);
            BuildSection7GrandTheatreClimaxHero(zoneMap, vestY, mats, layer1929, layerRuins);
            BuildSection8SecretVaultHero(zoneMap, vestY, mats, layer1929, layerRuins);
            BuildSection9WinterGardenHero(zoneMap, vestY, mats, layer1929, layerRuins);
            BuildSection10SouthwestAndBasementHero(zoneMap, vestY, mats, layer1929, layerRuins);
            BuildSection11WestBoardingWingHero(zoneMap, vestY, mats, layer1929, layerRuins);
            BuildSection12UpperSubterraneanAndExteriorsHero(zoneMap, vestY, mats, layer1929, layerRuins);

            AdaptRemainingSharedRenderers(rootEnv, mats);

            foreach (KeyValuePair<string, Transform> kv in zoneMap)
            {
                if (kv.Value == null) continue;
                Transform d1929 = kv.Value.Find("Decor_1929");
                Transform dRuins = kv.Value.Find("Decor_Ruins");
                if (d1929 != null) SetLayerAndStaticRecursive(d1929, layer1929);
                if (dRuins != null) SetLayerAndStaticRecursive(dRuins, layerRuins);
            }
        }

        private static List<Vector3> CollectAllDoorPositions(Transform rootEnv)
        {
            List<Vector3> list = new List<Vector3>();
            foreach (WorldDoor wd in rootEnv.GetComponentsInChildren<WorldDoor>(true))
            {
                if (wd != null) list.Add(wd.transform.position);
            }
            foreach (WorldDoubleDoor wdd in rootEnv.GetComponentsInChildren<WorldDoubleDoor>(true))
            {
                if (wdd != null) list.Add(wdd.transform.position);
            }
            return list;
        }

        private static bool IsNearAnyDoor(Vector3 worldPos, List<Vector3> doorCenters, float minRadius = 2.35f)
        {
            float rSq = minRadius * minRadius;
            for (int i = 0; i < doorCenters.Count; i++)
            {
                Vector3 d = doorCenters[i];
                float dx = worldPos.x - d.x;
                float dz = worldPos.z - d.z;
                if (dx * dx + dz * dz < rSq && Mathf.Abs(worldPos.y - d.y) < 4.5f)
                {
                    return true;
                }
            }
            return false;
        }

        // ====================================================================
        // PASE UNIVERSAL POR ZONA: PALACIO MISTERIOSO 1929 ↔ TERROR EN RUINAS
        // ====================================================================
        private static void RemodelZoneBaseAndPerimeter(
            Transform zoneRoot, int zoneIdx, List<Vector3> allDoorCenters,
            MasterEraPalette mats, int layer1929, int layerRuins)
        {
            Transform old1929 = zoneRoot.Find("Decor_1929");
            if (old1929 != null) Object.DestroyImmediate(old1929.gameObject);
            Transform oldRuins = zoneRoot.Find("Decor_Ruins");
            if (oldRuins != null) Object.DestroyImmediate(oldRuins.gameObject);

            GameObject decor1929 = new GameObject("Decor_1929");
            decor1929.transform.SetParent(zoneRoot, false);
            decor1929.layer = layer1929;
            decor1929.AddComponent<EraOnlyVisibility>().Configure(EraVisibilityMode.Only1929, false);

            GameObject decorRuins = new GameObject("Decor_Ruins");
            decorRuins.transform.SetParent(zoneRoot, false);
            decorRuins.layer = layerRuins;
            decorRuins.AddComponent<EraOnlyVisibility>().Configure(EraVisibilityMode.OnlyRuins, false);

            bool isExterior = zoneIdx == 1 || zoneIdx == 2 || zoneIdx == 3 || zoneIdx == 40 || zoneIdx == 42 || zoneIdx >= 57;
            bool isSubterranean = zoneIdx == 36 || zoneIdx == 37 || zoneIdx == 38 || zoneIdx == 41 || (zoneIdx >= 53 && zoneIdx <= 56);
            bool isTiledWetRoom = zoneIdx == 15 || zoneIdx == 43;

            Bounds zoneBounds = new Bounds(zoneRoot.position, Vector3.zero);
            bool hasBounds = false;
            float floorY = 2.4f;
            float ceilingY = 7.4f;
            bool foundFloor = false;
            bool foundCeiling = false;

            List<Transform> wallSegments = new List<Transform>();
            List<Transform> furnitureTables = new List<Transform>();
            List<Transform> furnitureShelves = new List<Transform>();
            List<Light> existingLights = new List<Light>();

            Renderer[] renderers = zoneRoot.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                if (r == null || r.transform.IsChildOf(decor1929.transform) || r.transform.IsChildOf(decorRuins.transform))
                    continue;

                if (r.GetComponent<WorldDocument>() != null || r.GetComponent<SequenceNode>() != null || r.GetComponent<WorldItem>() != null)
                    continue;

                if (!hasBounds)
                {
                    zoneBounds = r.bounds;
                    hasBounds = true;
                }
                else
                {
                    zoneBounds.Encapsulate(r.bounds);
                }

                string n = r.gameObject.name.ToLowerInvariant();
                ClassifyAndAttachSurfaceAdapter(r, n, isExterior, isSubterranean, isTiledWetRoom, mats);

                if (n.Contains("floor") || n.Contains("ground") || n.Contains("suelo") || n.Contains("deck"))
                {
                    float topY = r.bounds.max.y;
                    if (!foundFloor || topY < floorY)
                    {
                        floorY = topY;
                        foundFloor = true;
                    }
                }
                else if (n.Contains("ceiling") || n.Contains("roof") || n.Contains("techo"))
                {
                    float botY = r.bounds.min.y;
                    if (!foundCeiling || botY > ceilingY)
                    {
                        ceilingY = botY;
                        foundCeiling = true;
                    }
                }
                else if (n.Contains("wall") || n.Contains("muro"))
                {
                    Vector3 sz = r.bounds.size;
                    if (sz.y >= 2.2f && (sz.x >= 2.6f || sz.z >= 2.6f))
                    {
                        wallSegments.Add(r.transform);
                    }
                }
                else if (n.Contains("table") || n.Contains("mesa") || n.Contains("desk") || n.Contains("escritorio"))
                {
                    if (r.bounds.size.x >= 0.7f && r.bounds.size.z >= 0.6f)
                    {
                        furnitureTables.Add(r.transform);
                    }
                }
                else if (n.Contains("bookshelf") || n.Contains("estanteria") || n.Contains("sheetcabinet"))
                {
                    if (r.bounds.size.y >= 1.8f)
                    {
                        furnitureShelves.Add(r.transform);
                    }
                }
            }

            if (!hasBounds) return;
            if (!foundFloor) floorY = zoneBounds.min.y + 0.25f;
            if (!foundCeiling) ceilingY = Mathf.Max(floorY + 4.2f, zoneBounds.max.y - 0.25f);

            // 1. Boiserie Palaciega (1929) vs Destrucción Estructural de Terror (Ruinas) respetando puertas
            BuildAutomatedWallTrimAndDecay(wallSegments, allDoorCenters, decor1929.transform, decorRuins.transform,
                floorY, ceilingY, isExterior, isSubterranean, mats, layer1929, layerRuins);

            // 2. Detallado de Mesas, Escritorios y Estanterías (adiós a bloques lisos)
            UpgradeZoneFurnitureDualEra(furnitureTables, furnitureShelves, decor1929.transform, decorRuins.transform,
                mats, layer1929, layerRuins);

            // 3. Iluminación Dual: Claroscuro de velas mágicas (1929) vs Penumbra y parpadeo de terror (Ruinas)
            existingLights.AddRange(zoneRoot.GetComponentsInChildren<Light>(true));
            BuildDualEraZoneLighting(existingLights, zoneBounds, floorY, ceilingY, zoneIdx, isExterior, isSubterranean,
                decor1929.transform, decorRuins.transform, mats, layer1929, layerRuins);

            // 4. Escenografía de Terror en Ruinas (vigas derrumbadas, figuras bajo sudarios, charcos negros, raíces)
            //    y Misterio Palaciego en 1929 (cortinajes de rincón, alfombras y candelabros)
            BuildAutomatedFloorAndHorrorAtmosphere(zoneBounds, floorY, ceilingY, zoneIdx, isExterior, isSubterranean,
                allDoorCenters, decor1929.transform, decorRuins.transform, mats, layer1929, layerRuins);

            // 5. Partículas atmosféricas
            Vector3 centerAir = new Vector3(zoneBounds.center.x, Mathf.Lerp(floorY, ceilingY, 0.45f), zoneBounds.center.z);
            Vector3 boxSize = new Vector3(
                Mathf.Clamp(zoneBounds.size.x * 0.8f, 4f, 26f),
                Mathf.Clamp(ceilingY - floorY - 0.8f, 2.5f, 8f),
                Mathf.Clamp(zoneBounds.size.z * 0.8f, 4f, 26f));
            BuildZoneParticles(decor1929.transform, centerAir, boxSize, true, mats, layer1929);
            BuildZoneParticles(decorRuins.transform, centerAir, boxSize, false, mats, layerRuins);

            // 6. Realce sutil de interactuables
            EnhanceZoneInteractablesDualEra(zoneRoot, mats, layer1929, layerRuins);
        }

        private static void ClassifyAndAttachSurfaceAdapter(Renderer r, string lowerName, bool isExterior, bool isSubterranean, bool isTiledWetRoom, MasterEraPalette mats)
        {
            Material m1929 = null;
            Material mRuins = null;

            if (lowerName.Contains("mirror") || lowerName.Contains("espejo") || lowerName.Contains("cristal_espejo"))
            {
                m1929 = mats.mirrorIntact1929;
                mRuins = mats.brokenMirrorRuins;
            }
            else if (lowerName.Contains("carpet") || lowerName.Contains("rug") || lowerName.Contains("alfombra") ||
                     lowerName.Contains("drape") || lowerName.Contains("curtain") || lowerName.Contains("cortina") ||
                     lowerName.Contains("telon") || lowerName.Contains("velvet") || lowerName.Contains("butaca") ||
                     lowerName.Contains("seat") || lowerName.Contains("colcha"))
            {
                m1929 = mats.velvetWine1929;
                mRuins = mats.tornWineVelvetRuins;
            }
            else if (lowerName.Contains("floor") || lowerName.Contains("ground") || lowerName.Contains("suelo") || lowerName.Contains("step") || lowerName.Contains("peldano"))
            {
                if (isExterior || lowerName.Contains("cobble") || lowerName.Contains("courtyard") || lowerName.Contains("park"))
                {
                    m1929 = mats.travertine1929;
                    mRuins = mats.mossyCobbleRuins;
                }
                else if (isTiledWetRoom || lowerName.Contains("tile") || lowerName.Contains("washroom") || lowerName.Contains("infirmary"))
                {
                    m1929 = mats.porcelainTile1929;
                    mRuins = mats.crackedTileRuins;
                }
                else if (lowerName.Contains("vestibule") || lowerName.Contains("marble") || lowerName.Contains("memorial") || lowerName.Contains("stair"))
                {
                    m1929 = mats.marble1929;
                    mRuins = mats.marbleCrackedRuins;
                }
                else if (isSubterranean)
                {
                    m1929 = mats.travertine1929;
                    mRuins = mats.mossyCobbleRuins;
                }
                else
                {
                    m1929 = mats.parquet1929;
                    mRuins = mats.parquetRuins;
                }
            }
            else if (lowerName.Contains("wall") || lowerName.Contains("muro") || lowerName.Contains("ceiling") ||
                     lowerName.Contains("roof") || lowerName.Contains("techo") || lowerName.Contains("column") ||
                     lowerName.Contains("pillar") || lowerName.Contains("balustrade") || lowerName.Contains("arch"))
            {
                if (isSubterranean)
                {
                    m1929 = mats.travertine1929;
                    mRuins = mats.exposedBrickRuins;
                }
                else if (isTiledWetRoom && lowerName.Contains("wall"))
                {
                    m1929 = mats.porcelainTile1929;
                    mRuins = mats.crackedTileRuins;
                }
                else
                {
                    m1929 = mats.wallIvory1929;
                    mRuins = mats.wallCrackedRuins;
                }
            }
            else if (lowerName.Contains("gold") || lowerName.Contains("brass") || lowerName.Contains("marco") ||
                     lowerName.Contains("plaque") || lowerName.Contains("chandelier") || lowerName.Contains("arana") ||
                     lowerName.Contains("bell") || lowerName.Contains("trophy") || lowerName.Contains("busto"))
            {
                m1929 = mats.antiqueGold1929;
                mRuins = mats.rustedIronRuins;
            }
            else if (lowerName.Contains("locker") || lowerName.Contains("taquilla") || lowerName.Contains("pipe") ||
                     lowerName.Contains("tuberia") || lowerName.Contains("boiler") || lowerName.Contains("caldera") ||
                     lowerName.Contains("gate") || lowerName.Contains("reja") || lowerName.Contains("catwalk") ||
                     lowerName.Contains("pulley") || lowerName.Contains("polea") || lowerName.Contains("contrapeso"))
            {
                m1929 = mats.antiqueGold1929;
                mRuins = mats.rustedIronRuins;
            }
            else if (lowerName.Contains("desk") || lowerName.Contains("escritorio") || lowerName.Contains("table") ||
                     lowerName.Contains("mesa") || lowerName.Contains("shelf") || lowerName.Contains("estanteria") ||
                     lowerName.Contains("cabinet") || lowerName.Contains("armario") || lowerName.Contains("tocador") ||
                     lowerName.Contains("bench") || lowerName.Contains("banco") || lowerName.Contains("piano") ||
                     lowerName.Contains("chair") || lowerName.Contains("silla") || lowerName.Contains("counter") ||
                     lowerName.Contains("mostrador") || lowerName.Contains("bed") || lowerName.Contains("cama") ||
                     lowerName.Contains("baul"))
            {
                m1929 = mats.mahogany1929;
                mRuins = mats.rottedWoodRuins;
            }
            else if (lowerName.Contains("hedge") || lowerName.Contains("seto") || lowerName.Contains("bush") || lowerName.Contains("plant"))
            {
                m1929 = mats.velvetEmerald1929;
                mRuins = mats.mossAndIvyRuins;
            }
            else if (lowerName.Contains("statue") || lowerName.Contains("estatua") || lowerName.Contains("fountain") || lowerName.Contains("fuente"))
            {
                m1929 = mats.boiserieCream1929;
                mRuins = mats.wallCrackedRuins;
            }

            if (m1929 != null && mRuins != null)
            {
                EraSurfaceAdapter adapter = r.GetComponent<EraSurfaceAdapter>();
                if (adapter == null) adapter = r.gameObject.AddComponent<EraSurfaceAdapter>();
                adapter.Configure(r, m1929, mRuins);
                r.sharedMaterial = m1929;
            }
        }

        private static void BuildAutomatedWallTrimAndDecay(
            List<Transform> walls, List<Vector3> allDoorCenters,
            Transform decor1929, Transform decorRuins,
            float floorY, float ceilingY, bool isExterior, bool isSubterranean,
            MasterEraPalette mats, int layer1929, int layerRuins)
        {
            Material trim1929 = mats.antiqueGold1929;
            Material dado1929 = isSubterranean ? mats.travertine1929 : mats.boiserieCream1929;
            Material wood1929 = mats.mahogany1929;

            int wallIdx = 0;
            foreach (Transform w in walls)
            {
                if (w == null) continue;
                Renderer wr = w.GetComponent<Renderer>();
                if (wr == null) continue;
                Bounds wb = wr.bounds;
                Vector3 sz = wb.size;
                Vector3 c = wb.center;

                bool alongX = sz.x >= sz.z;
                float len = alongX ? sz.x : sz.z;
                float thick = alongX ? Mathf.Min(sz.z, 0.60f) : Mathf.Min(sz.x, 0.60f);
                if (len < 2.6f) continue;

                float wBot = Mathf.Max(floorY, wb.min.y);
                float wTop = Mathf.Min(ceilingY, wb.max.y);
                if (wTop - wBot < 2.2f) continue;

                wallIdx++;

                // Cornisa superior imperial en 1929 (siempre por encima de las puertas)
                if (!isExterior)
                {
                    Vector3 corniceScale = alongX
                        ? new Vector3(len, 0.24f, thick + 0.12f)
                        : new Vector3(thick + 0.12f, 0.24f, len);
                    VisWorld("CrownCornice_1929_" + wallIdx, PrimitiveType.Cube, decor1929,
                        new Vector3(c.x, wTop - 0.12f, c.z), corniceScale, wood1929, layer1929);

                    Vector3 friezeScale = alongX
                        ? new Vector3(len, 0.06f, thick + 0.15f)
                        : new Vector3(thick + 0.15f, 0.06f, len);
                    VisWorld("CrownFrieze_Gold_1929_" + wallIdx, PrimitiveType.Cube, decor1929,
                        new Vector3(c.x, wTop - 0.26f, c.z), friezeScale, trim1929, layer1929);
                }

                // Si el centro de este tramo de muro está libre de puertas, colocamos arrambadero de caoba noble + pilastras en 1929
                // y grietas estructurales + ladrillo roto + tablones clavados en Ruinas
                Vector3 floorCheck = new Vector3(c.x, wBot, c.z);
                if (!IsNearAnyDoor(floorCheck, allDoorCenters, 2.5f))
                {
                    float safeLen = Mathf.Max(1.6f, len - 0.8f);

                    if (!isExterior)
                    {
                        // 1929: Arrambadero bajo de caoba noble (0..0.85m) y moldura fina de bronce bruñido (sin barra amarilla chillona)
                        Vector3 dadoScale = alongX
                            ? new Vector3(safeLen, 0.82f, thick + 0.04f)
                            : new Vector3(thick + 0.04f, 0.82f, safeLen);
                        VisWorld("PalaceWainscot_1929_" + wallIdx, PrimitiveType.Cube, decor1929,
                            new Vector3(c.x, wBot + 0.41f, c.z), dadoScale, wood1929, layer1929);

                        Vector3 capScale = alongX
                            ? new Vector3(safeLen + 0.04f, 0.05f, thick + 0.07f)
                            : new Vector3(thick + 0.07f, 0.05f, safeLen + 0.04f);
                        VisWorld("WainscotBronzeCap_1929_" + wallIdx, PrimitiveType.Cube, decor1929,
                            new Vector3(c.x, wBot + 0.84f, c.z), capScale, trim1929, layer1929);

                        // En Ruinas: el mismo arrambadero en madera podrida (sin placas ni cajas sobresaliendo del muro)
                        VisWorld("DecayedWainscot_Ruins_" + wallIdx, PrimitiveType.Cube, decorRuins,
                            new Vector3(c.x, wBot + 0.41f, c.z), dadoScale, mats.rottedWoodRuins, layerRuins);
                    }
                }
            }
        }

        // ====================================================================
        // DETALLADO DE MESAS, ESCRITORIOS Y ESTANTERÍAS (1929 PALACIO ↔ RUINAS TERROR)
        // ====================================================================
        private static void UpgradeZoneFurnitureDualEra(
            List<Transform> tables, List<Transform> shelves,
            Transform decor1929, Transform decorRuins,
            MasterEraPalette mats, int layer1929, int layerRuins)
        {
            int tIdx = 0;
            foreach (Transform t in tables)
            {
                if (t == null) continue;
                Renderer r = t.GetComponent<Renderer>();
                if (r == null) continue;
                Bounds b = r.bounds;
                Vector3 c = b.center;
                Vector3 sz = b.size;
                float topY = b.max.y;
                tIdx++;

                // 1929: Tablero biselado de caoba noble, cantoneras de bronce y tapete imperial de terciopelo vino
                VisWorld($"TableBevelTop_1929_{tIdx}", PrimitiveType.Cube, decor1929,
                    new Vector3(c.x, topY - 0.02f, c.z), new Vector3(sz.x + 0.08f, 0.05f, sz.z + 0.08f), mats.mahogany1929, layer1929);
                VisWorld($"TableGoldTrim_1929_{tIdx}", PrimitiveType.Cube, decor1929,
                    new Vector3(c.x, topY - 0.05f, c.z), new Vector3(sz.x + 0.10f, 0.02f, sz.z + 0.10f), mats.antiqueGold1929, layer1929);
                VisWorld($"TableVelvetRunner_1929_{tIdx}", PrimitiveType.Cube, decor1929,
                    new Vector3(c.x, topY + 0.004f, c.z), new Vector3(sz.x * 0.72f, 0.008f, sz.z * 0.78f), mats.velvetWine1929, layer1929);

                // RUINAS: Tablones partidos sobre la mesa, tela desgarrada colgando y telaraña hasta el suelo
                GameObject splitPlank = VisWorld($"TableBrokenPlank_Ruins_{tIdx}", PrimitiveType.Cube, decorRuins,
                    new Vector3(c.x, topY + 0.015f, c.z), new Vector3(sz.x * 0.85f, 0.03f, sz.z * 0.45f), mats.rottedWoodRuins, layerRuins);
                splitPlank.transform.localRotation = Quaternion.Euler(2f, (tIdx % 2 == 0) ? 7f : -9f, 3f);
            }

            int sIdx = 0;
            foreach (Transform s in shelves)
            {
                if (s == null) continue;
                Renderer r = s.GetComponent<Renderer>();
                if (r == null) continue;
                Bounds b = r.bounds;
                Vector3 c = b.center;
                Vector3 sz = b.size;
                sIdx++;

                bool alongX = sz.x >= sz.z;
                float len = alongX ? sz.x : sz.z;
                float depth = alongX ? sz.z : sz.x;

                // 1929: 4 baldas con hileras de tomos encuadernados en cuero vino, esmeralda y lomos dorados + cornisa superior
                VisWorld($"ShelfCrown_1929_{sIdx}", PrimitiveType.Cube, decor1929,
                    new Vector3(c.x, b.max.y + 0.06f, c.z),
                    alongX ? new Vector3(sz.x + 0.12f, 0.12f, sz.z + 0.10f) : new Vector3(sz.x + 0.10f, 0.12f, sz.z + 0.12f),
                    mats.antiqueGold1929, layer1929);

                for (int row = 0; row < 4; row++)
                {
                    float ry = b.min.y + 0.45f + row * ((sz.y - 0.7f) / 3.5f);
                    Material bookMat = (row % 3 == 0) ? mats.velvetWine1929 : ((row % 3 == 1) ? mats.velvetEmerald1929 : mats.antiqueGold1929);
                    Vector3 rowScale = alongX
                        ? new Vector3(len * 0.88f, 0.34f, depth + 0.05f)
                        : new Vector3(depth + 0.05f, 0.34f, len * 0.88f);
                    VisWorld($"ShelfBookRow_1929_{sIdx}_{row + 1}", PrimitiveType.Cube, decor1929,
                        new Vector3(c.x, ry, c.z), rowScale, bookMat, layer1929);
                }

                // RUINAS: Baldas hundidas en diagonal
                for (int row = 0; row < 3; row++)
                {
                    float ry = b.min.y + 0.65f + row * 0.85f;
                    Vector3 brokenShelfScale = alongX
                        ? new Vector3(len * 0.85f, 0.06f, depth + 0.08f)
                        : new Vector3(depth + 0.08f, 0.06f, len * 0.85f);
                    GameObject bs = VisWorld($"CollapsedShelfPlank_Ruins_{sIdx}_{row + 1}", PrimitiveType.Cube, decorRuins,
                        new Vector3(c.x, ry, c.z), brokenShelfScale, mats.rottedWoodRuins, layerRuins);
                    bs.transform.localRotation = alongX
                        ? Quaternion.Euler(0f, 0f, (row % 2 == 0) ? 11f : -14f)
                        : Quaternion.Euler((row % 2 == 0) ? 11f : -14f, 0f, 0f);
                }
            }
        }

        // ====================================================================
        // ILUMINACIÓN DUAL: PALACIO MISTERIOSO 1929 ↔ PARPADEO DE TERROR RUINAS
        // ====================================================================
        private static void BuildDualEraZoneLighting(
            List<Light> existingLights, Bounds zoneBounds, float floorY, float ceilingY,
            int zoneIdx, bool isExterior, bool isSubterranean,
            Transform decor1929, Transform decorRuins, MasterEraPalette mats, int layer1929, int layerRuins)
        {
            List<Vector3> lampPositions = new List<Vector3>();
            foreach (Light l in existingLights)
            {
                if (l == null || l.type != LightType.Point) continue;
                if (l.transform.IsChildOf(decor1929) || l.transform.IsChildOf(decorRuins)) continue;
                if (l.gameObject.name.Contains("Anomaly") || l.gameObject.name.Contains("Glint")) continue;

                lampPositions.Add(l.transform.position);
            }

            if (lampPositions.Count == 0 && !isExterior)
            {
                lampPositions.Add(new Vector3(zoneBounds.center.x, ceilingY - 0.65f, zoneBounds.center.z));
            }

            int maxFixtures = Mathf.Min(lampPositions.Count, 4);
            for (int i = 0; i < maxFixtures; i++)
            {
                Vector3 lp = lampPositions[i];
                float fixtureY = isExterior ? lp.y : Mathf.Clamp(lp.y, floorY + 2.6f, ceilingY - 0.35f);
                Vector3 pos = new Vector3(lp.x, fixtureY, lp.z);

                // Solo colocar plafones de techo físicos en salas interiores (nunca sobre farolas de exterior)
                if (!isExterior)
                {
                    VisWorld($"LampFixture_1929_{i + 1}", PrimitiveType.Cylinder, decor1929,
                        pos + new Vector3(0f, 0.25f, 0f), new Vector3(0.48f, 0.04f, 0.48f), mats.antiqueGold1929, layer1929);
                    VisWorld($"LampGlobe_1929_{i + 1}", PrimitiveType.Sphere, decor1929,
                        pos, new Vector3(0.30f, 0.24f, 0.30f), mats.warmBulbEmit1929, layer1929);

                    GameObject fixRuins = VisWorld($"LampFixture_Ruins_{i + 1}", PrimitiveType.Cylinder, decorRuins,
                        pos + new Vector3(0.04f, 0.18f, 0f), new Vector3(0.45f, 0.04f, 0.45f), mats.rustedIronRuins, layerRuins);
                    fixRuins.transform.localRotation = Quaternion.Euler(14f, i * 25f, -16f);
                }

                GameObject l1929Obj = new GameObject($"EraLight_1929_{i + 1}");
                l1929Obj.transform.SetParent(decor1929, false);
                l1929Obj.transform.position = pos - new Vector3(0f, 0.15f, 0f);
                l1929Obj.layer = layer1929;
                Light l1929 = l1929Obj.AddComponent<Light>();
                l1929.type = LightType.Point;
                l1929.color = new Color(0.98f, 0.93f, 0.86f);
                l1929.intensity = isSubterranean ? 2.1f : 2.5f;
                l1929.range = 16.0f;
                l1929.shadows = LightShadows.None;

                GameObject lRuinsObj = new GameObject($"HorrorLight_Ruins_{zoneIdx}_{i + 1}");
                lRuinsObj.transform.SetParent(decorRuins, false);
                lRuinsObj.transform.position = pos - new Vector3(0f, 0.25f, 0f);
                lRuinsObj.layer = layerRuins;
                Light lRuins = lRuinsObj.AddComponent<Light>();
                lRuins.type = LightType.Point;
                lRuins.color = new Color(0.70f, 0.80f, 0.92f);
                lRuins.intensity = 1.75f;
                lRuins.range = 15.0f;
                lRuins.shadows = LightShadows.None;

                if (i == 0)
                {
                    HorrorLightFlicker flicker = lRuinsObj.AddComponent<HorrorLightFlicker>();
                    flicker.Configure(lRuins, 1.75f, 7.5f + (zoneIdx % 5) * 1.2f, zoneIdx * 13.7f);
                }
            }
        }

        // ====================================================================
        // ESCENOGRAFÍA DE TERROR EN RUINAS Y MISTERIO PALACIEGO EN 1929 POR ZONA
        // ====================================================================
        private static void BuildAutomatedFloorAndHorrorAtmosphere(
            Bounds zoneBounds, float floorY, float ceilingY, int zoneIdx, bool isExterior, bool isSubterranean,
            List<Vector3> allDoorCenters,
            Transform decor1929, Transform decorRuins, MasterEraPalette mats, int layer1929, int layerRuins)
        {
            Vector3 c = zoneBounds.center;
            float sx = Mathf.Max(3.5f, zoneBounds.size.x - 2.4f);
            float sz = Mathf.Max(3.5f, zoneBounds.size.z - 2.4f);

            // 1. EN 1929 (Palacio Mágico con Misterio):
            //    Cortinajes de terciopelo vino #6B1E2E en las esquinas de la estancia que proyectan siluetas teatrales
            if (!isExterior && sx >= 5.0f && sz >= 5.0f)
            {
                float drapeH = Mathf.Clamp(ceilingY - floorY - 0.4f, 3.0f, 5.8f);
                for (int corner = 0; corner < 2; corner++)
                {
                    float cx = c.x + ((corner == 0) ? -1f : 1f) * (sx * 0.46f);
                    float cz = c.z + ((corner == 0) ? 1f : -1f) * (sz * 0.46f);
                    Vector3 cornerPos = new Vector3(cx, floorY, cz);
                    if (!IsNearAnyDoor(cornerPos, allDoorCenters, 2.2f))
                    {
                        VisWorld($"PalaceCornerDrape_1929_{corner + 1}", PrimitiveType.Cylinder, decor1929,
                            new Vector3(cx, floorY + drapeH * 0.5f, cz), new Vector3(0.42f, drapeH * 0.5f, 0.42f), mats.velvetWine1929, layer1929);
                        VisWorld($"PalaceDrapeGoldTie_1929_{corner + 1}", PrimitiveType.Cylinder, decor1929,
                            new Vector3(cx, floorY + 1.35f, cz), new Vector3(0.48f, 0.04f, 0.48f), mats.antiqueGold1929, layer1929);
                    }
                }
            }

            // 2. EN RUINAS (Película de Terror Psicológico):
            // A) Grandes charcos negros de agua estancada que reflejan la linterna
            int puddleCount = Mathf.Clamp(Mathf.RoundToInt((sx * sz) / 70f), 2, 5);
            for (int p = 0; p < puddleCount; p++)
            {
                float px = c.x + ((p % 2 == 0) ? -1f : 1f) * (sx * (0.16f + (p % 3) * 0.08f));
                float pz = c.z + ((p < 2) ? -1f : 1f) * (sz * (0.18f + (p % 2) * 0.09f));
                VisWorld($"HorrorPuddle_Ruins_{p + 1}", PrimitiveType.Cylinder, decorRuins,
                    new Vector3(px, floorY + 0.008f, pz),
                    new Vector3(2.8f + (p % 2) * 1.1f, 0.009f, 2.2f + (p % 3) * 0.8f),
                    mats.waterPuddleRuins, layerRuins);
            }

            // B) Vigas pesadas del techo derrumbadas en diagonal hacia el suelo (sin bloquear puertas)
            if (!isExterior && sx >= 5.5f && sz >= 5.5f)
            {
                int beamCount = (sx * sz > 140f) ? 3 : 2;
                for (int b = 0; b < beamCount; b++)
                {
                    float bx = c.x + (b - 0.5f) * (sx * 0.28f);
                    float bz = c.z + ((b % 2 == 0) ? 1f : -1f) * (sz * 0.26f);
                    Vector3 bPos = new Vector3(bx, floorY, bz);
                    if (!IsNearAnyDoor(bPos, allDoorCenters, 2.6f))
                    {
                        GameObject fallenBeam = VisWorld($"CollapsedRoofBeam_Ruins_{b + 1}", PrimitiveType.Cube, decorRuins,
                            new Vector3(bx, floorY + 1.35f, bz), new Vector3(0.24f, 2.9f, 0.22f), mats.rottedWoodRuins, layerRuins);
                        fallenBeam.transform.localRotation = Quaternion.Euler(48f + b * 12f, zoneIdx * 29f + b * 75f, 18f);
                    }
                }
            }

            // C) Silueta inquietante cubierta por un sudario / maniquí roto en un rincón oscuro de la sala
            if (!isExterior && sx >= 5.0f && sz >= 5.0f)
            {
                Vector3 cornerFigPos = new Vector3(c.x - sx * 0.42f, floorY, c.z - sz * 0.42f);
                if (!IsNearAnyDoor(cornerFigPos, allDoorCenters, 2.4f))
                {
                    BuildHorrorShroudedFigure(decorRuins, $"HorrorShroudedFigure_Z{zoneIdx}", cornerFigPos, zoneIdx, mats, layerRuins);
                }
            }

            // D) Tablones levantados y cascotes de mampostería en el suelo
            int rubbleCount = Mathf.Clamp(Mathf.RoundToInt((sx * sz) / 28f), 4, 16);
            for (int r = 0; r < rubbleCount; r++)
            {
                float tX = ((r * 37 + zoneIdx * 13) % 100) / 100f - 0.5f;
                float tZ = ((r * 61 + zoneIdx * 19) % 100) / 100f - 0.5f;
                float rx = c.x + tX * sx * 0.82f;
                float rz = c.z + tZ * sz * 0.82f;
                Material rMat = (r % 3 == 0) ? mats.wallCrackedRuins : ((r % 3 == 1) ? mats.exposedBrickRuins : mats.rottedWoodRuins);
                GameObject rub = VisWorld($"HorrorRubble_Ruins_{r + 1}", PrimitiveType.Cube, decorRuins,
                    new Vector3(rx, floorY + 0.05f, rz),
                    new Vector3(0.32f + (r % 3) * 0.14f, 0.09f + (r % 2) * 0.07f, 0.28f + (r % 4) * 0.14f),
                    rMat, layerRuins);
                rub.transform.localRotation = Quaternion.Euler(r * 19f, r * 43f, r * 11f);
            }
        }

        private static void BuildHorrorShroudedFigure(Transform parent, string name, Vector3 floorPos, int seedIdx, MasterEraPalette mats, int layer)
        {
            GameObject fig = new GameObject(name);
            fig.transform.SetParent(parent, false);
            fig.transform.position = floorPos;
            fig.transform.rotation = Quaternion.Euler(0f, (seedIdx * 53f) % 360f, 0f);
            fig.layer = layer;

            // Figura vertical cubierta por un sudario grisáceo desgarrado (como una bailarina o maniquí tapado en la penumbra)
            VisLocal("Shroud_LowerDraped", PrimitiveType.Cylinder, fig.transform,
                new Vector3(0f, 0.55f, 0f), new Vector3(0.48f, 0.55f, 0.42f), mats.shroudClothRuins, layer);
            GameObject torso = VisLocal("Shroud_UpperTorso", PrimitiveType.Capsule, fig.transform,
                new Vector3(0f, 1.25f, 0f), new Vector3(0.36f, 0.42f, 0.28f), mats.shroudClothRuins, layer);
            torso.transform.localRotation = Quaternion.Euler(8f, 0f, -7f);

            // Cabeza inclinada de forma inquietante (o maniquí sin cabeza en zonas impares)
            if (seedIdx % 2 == 0)
            {
                GameObject head = VisLocal("Shroud_TiltedHead", PrimitiveType.Sphere, fig.transform,
                    new Vector3(0.05f, 1.72f, 0.04f), new Vector3(0.21f, 0.25f, 0.21f), mats.shroudClothRuins, layer);
                head.transform.localRotation = Quaternion.Euler(18f, 0f, -24f);
            }
            else
            {
                VisLocal("BrokenNeck_Peg", PrimitiveType.Cylinder, fig.transform,
                    new Vector3(0f, 1.66f, 0f), new Vector3(0.05f, 0.10f, 0.05f), mats.rustedIronRuins, layer);
            }
        }

        private static void EnhanceZoneInteractablesDualEra(Transform zoneRoot, MasterEraPalette mats, int layer1929, int layerRuins)
        {
            Transform d1929 = zoneRoot.Find("Decor_1929");
            Transform dRuins = zoneRoot.Find("Decor_Ruins");
            if (d1929 == null || dRuins == null) return;

            WorldDocument[] docs = zoneRoot.GetComponentsInChildren<WorldDocument>(true);
            int idx = 0;
            foreach (WorldDocument doc in docs)
            {
                if (doc == null) continue;
                idx++;
                Vector3 p = doc.transform.position;
                Vector3 s = doc.transform.lossyScale;

                VisWorld($"DocTray_1929_{idx}", PrimitiveType.Cube, d1929,
                    p - new Vector3(0f, 0.012f, 0f), new Vector3(s.x + 0.06f, 0.015f, s.z + 0.06f), mats.antiqueGold1929, layer1929);
                VisWorld($"DocTray_Ruins_{idx}", PrimitiveType.Cube, dRuins,
                    p - new Vector3(0f, 0.012f, 0f), new Vector3(s.x + 0.05f, 0.015f, s.z + 0.05f), mats.rustedIronRuins, layerRuins);
            }
        }

        // ====================================================================
        // ESCENOGRAFÍA HEROICA POR SECCIÓN (SECCIONES 1 A 12)
        // ====================================================================

        private static void BuildSection1ExteriorAndEntranceHero(
            Dictionary<string, Transform> zoneMap, float vestY, MasterEraPalette mats, int l1929, int lRuins)
        {
            if (zoneMap.TryGetValue("2_Entrance_Courtyard", out Transform z2))
            {
                Transform d1929 = z2.Find("Decor_1929");
                Transform dRuins = z2.Find("Decor_Ruins");
                Vector3 fPos = new Vector3(0f, 0f, -15.5f);

                VisWorld("CourtyardFountain_OuterRim_1929", PrimitiveType.Cylinder, d1929,
                    fPos + new Vector3(0f, 0.35f, 0f), new Vector3(5.6f, 0.35f, 5.6f), mats.boiserieCream1929, l1929);
                VisWorld("CourtyardFountain_Water_1929", PrimitiveType.Cylinder, d1929,
                    fPos + new Vector3(0f, 0.62f, 0f), new Vector3(5.1f, 0.05f, 5.1f), mats.crystalWater1929, l1929);
                VisWorld("CourtyardFountain_Pedestal_1929", PrimitiveType.Cylinder, d1929,
                    fPos + new Vector3(0f, 1.15f, 0f), new Vector3(1.35f, 0.85f, 1.35f), mats.boiserieCream1929, l1929);
                BuildBallerinaSculpture(d1929, "Courtyard_BallerinaStatue_1929", fPos + new Vector3(0f, 2.0f, 0f), mats.boiserieCream1929, mats.antiqueGold1929, false, l1929);

                float[] roseX = { -5.2f, 5.2f };
                float[] roseZ = { -21.0f, -15.5f, -10.0f };
                foreach (float rx in roseX)
                {
                    foreach (float rz in roseZ)
                    {
                        VisWorld($"RoseBush_Foliage_1929_{rx}_{rz}", PrimitiveType.Sphere, d1929,
                            new Vector3(rx, 0.55f, rz), new Vector3(1.45f, 0.95f, 1.45f), mats.velvetEmerald1929, l1929);
                        for (int b = 0; b < 5; b++)
                        {
                            VisWorld($"RoseBloom_1929_{rx}_{rz}_{b}", PrimitiveType.Sphere, d1929,
                                new Vector3(rx + Mathf.Cos(b * 1.3f) * 0.48f, 0.85f + (b % 2) * 0.15f, rz + Mathf.Sin(b * 1.3f) * 0.48f),
                                new Vector3(0.18f, 0.16f, 0.18f), (b % 2 == 0) ? mats.roseLavender1929 : mats.velvetWine1929, l1929);
                        }
                    }
                }

                VisWorld("CourtyardFountain_OuterRim_Ruins", PrimitiveType.Cylinder, dRuins,
                    fPos + new Vector3(0f, 0.34f, 0f), new Vector3(5.6f, 0.34f, 5.6f), mats.wallCrackedRuins, lRuins);
                VisWorld("CourtyardFountain_StagnantWater_Ruins", PrimitiveType.Cylinder, dRuins,
                    fPos + new Vector3(0f, 0.42f, 0f), new Vector3(4.9f, 0.04f, 4.9f), mats.waterPuddleRuins, lRuins);
                VisWorld("CourtyardFountain_MossRing_Ruins", PrimitiveType.Cylinder, dRuins,
                    fPos + new Vector3(0f, 0.69f, 0f), new Vector3(5.65f, 0.03f, 5.65f), mats.mossAndIvyRuins, lRuins);
                VisWorld("CourtyardFountain_Pedestal_Ruins", PrimitiveType.Cylinder, dRuins,
                    fPos + new Vector3(0f, 1.15f, 0f), new Vector3(1.35f, 0.85f, 1.35f), mats.wallCrackedRuins, lRuins);
                BuildBallerinaSculpture(dRuins, "Courtyard_BallerinaStatue_Ruins", fPos + new Vector3(0f, 1.98f, 0f), mats.wallCrackedRuins, mats.mossAndIvyRuins, true, lRuins);

                foreach (float rx in roseX)
                {
                    foreach (float rz in roseZ)
                    {
                        VisWorld($"DeadBush_Ruins_{rx}_{rz}", PrimitiveType.Sphere, dRuins,
                            new Vector3(rx, 0.45f, rz), new Vector3(1.55f, 0.75f, 1.45f), mats.mossAndIvyRuins, lRuins);
                    }
                }
            }

            if (zoneMap.TryGetValue("3_Grand_Staircase_And_Portico", out Transform z3))
            {
                Transform d1929 = z3.Find("Decor_1929");
                Transform dRuins = z3.Find("Decor_Ruins");

                float[] bannerX = { -7.8f, -3.6f, 3.6f, 7.8f };
                for (int i = 0; i < bannerX.Length; i++)
                {
                    float bx = bannerX[i];
                    VisWorld($"Facade_RoyalBanner_1929_{i + 1}", PrimitiveType.Cube, d1929,
                        new Vector3(bx, vestY + 4.2f, 7.55f), new Vector3(1.45f, 4.4f, 0.08f), mats.velvetWine1929, l1929);
                    VisWorld($"Facade_BannerGoldBar_1929_{i + 1}", PrimitiveType.Cylinder, d1929,
                        new Vector3(bx, vestY + 6.45f, 7.55f), new Vector3(0.08f, 0.85f, 0.08f), mats.antiqueGold1929, l1929)
                        .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    VisWorld($"Facade_BannerCrest_1929_{i + 1}", PrimitiveType.Cylinder, d1929,
                        new Vector3(bx, vestY + 4.5f, 7.49f), new Vector3(0.65f, 0.04f, 0.65f), mats.antiqueGold1929, l1929)
                        .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

                    GameObject tornBanner = VisWorld($"Facade_TornBanner_Ruins_{i + 1}", PrimitiveType.Cube, dRuins,
                        new Vector3(bx, vestY + 4.8f, 7.55f), new Vector3(1.25f, 2.2f, 0.06f), mats.tornWineVelvetRuins, lRuins);
                    tornBanner.transform.localRotation = Quaternion.Euler(0f, 0f, -8f + i * 5f);

                    VisWorld($"Facade_IvyMass_Ruins_{i + 1}", PrimitiveType.Sphere, dRuins,
                        new Vector3(bx, vestY + 3.6f, 7.48f), new Vector3(2.2f, 3.8f, 0.45f), mats.mossAndIvyRuins, lRuins);
                }
            }

            if (zoneMap.TryGetValue("4_Reception_Vestibule", out Transform z4))
            {
                BuildHeroChandelierPair(z4, new Vector3(0f, vestY + 5.8f, 20.0f), mats, l1929, lRuins);
            }

            if (zoneMap.TryGetValue("5_Grand_Hallway", out Transform z5))
            {
                BuildHeroChandelierPair(z5, new Vector3(0f, vestY + 4.6f, 40.0f), mats, l1929, lRuins);
                BuildHeroChandelierPair(z5, new Vector3(0f, vestY + 4.6f, 55.0f), mats, l1929, lRuins);
                BuildHeroChandelierPair(z5, new Vector3(0f, vestY + 4.6f, 70.0f), mats, l1929, lRuins);
            }
        }

        private static void BuildSection2ReceptionAndAdminHero(
            Dictionary<string, Transform> zoneMap, float vestY, MasterEraPalette mats, int l1929, int lRuins)
        {
            if (zoneMap.TryGetValue("7_Administration_Office", out Transform z7))
            {
                // Ventanal en X = -37.55, Z = 21.0 (lejos de la puerta a la suite de Isadora en Z = 14.5)
                BuildHeroWindowPair(z7, new Vector3(-37.55f, vestY, 21.0f), Quaternion.Euler(0f, 90f, 0f), mats, l1929, lRuins);
                BuildHeroChandelierPair(z7, new Vector3(-31.0f, vestY + 4.5f, 19.0f), mats, l1929, lRuins);
            }
            if (zoneMap.TryGetValue("9_Waiting_Room", out Transform z9))
            {
                BuildHeroChandelierPair(z9, new Vector3(-17.5f, vestY + 4.5f, 30.0f), mats, l1929, lRuins);
            }
            if (zoneMap.TryGetValue("10_Portrait_Gallery_Hallway", out Transform z10))
            {
                BuildHeroChandelierPair(z10, new Vector3(-14.0f, vestY + 4.5f, 44.0f), mats, l1929, lRuins);
            }
        }

        // --- SECCIÓN 3: VESTUARIOS Y TOCADORES (Corregido: nada sobre la puerta de Z = 46) ---
        private static void BuildSection3StudentDressingAndRoomsHero(
            Dictionary<string, Transform> zoneMap, float vestY, MasterEraPalette mats, int l1929, int lRuins)
        {
            if (zoneMap.TryGetValue("11_Female_Dressing_Room", out Transform z11))
            {
                BuildHeroChandelierPair(z11, new Vector3(19.0f, vestY + 4.4f, 22.0f), mats, l1929, lRuins);
                BuildTutuRackPair(z11, new Vector3(15.5f, vestY, 20.5f), mats, l1929, lRuins);
            }
            if (zoneMap.TryGetValue("12_Vanity_Mirror_Room", out Transform z12))
            {
                // Araña central sobre la alfombra de la Sala de Tocadores (19.0, Z = 39.0)
                BuildHeroChandelierPair(z12, new Vector3(19.0f, vestY + 4.4f, 39.0f), mats, l1929, lRuins);

                // Adornar los tocadores reales en los muros Oeste (X = 12.2) y Este (X = 25.8), nunca en la puerta norte (19, 46)
                BuildHollywoodVanityPair(z12, new Vector3(12.2f, vestY, 35.0f), Quaternion.Euler(0f, -90f, 0f), mats, l1929, lRuins);
                BuildHollywoodVanityPair(z12, new Vector3(25.8f, vestY, 37.8f), Quaternion.Euler(0f, 90f, 0f), mats, l1929, lRuins);
            }
            if (zoneMap.TryGetValue("14_Elena_Private_Room", out Transform z14))
            {
                BuildHeroWindowPair(z14, new Vector3(19.0f, vestY, 65.6f), Quaternion.identity, mats, l1929, lRuins);
                BuildHeroChandelierPair(z14, new Vector3(19.0f, vestY + 4.4f, 59.0f), mats, l1929, lRuins);
            }
        }

        private static void BuildSection4RehearsalStudiosHero(
            Dictionary<string, Transform> zoneMap, float vestY, MasterEraPalette mats, int l1929, int lRuins)
        {
            if (zoneMap.TryGetValue("17_Mirror_Posture_Studio", out Transform z17))
            {
                BuildHeroChandelierPair(z17, new Vector3(-32.0f, vestY + 5.2f, 59.0f), mats, l1929, lRuins);
                BuildHeroWindowPair(z17, new Vector3(-37.6f, vestY, 59.0f), Quaternion.Euler(0f, 90f, 0f), mats, l1929, lRuins);
            }
            if (zoneMap.TryGetValue("18_Mannequin_And_Stage_Costumes", out Transform z18))
            {
                BuildTutuRackPair(z18, new Vector3(-19.0f, vestY, 75.0f), mats, l1929, lRuins);
            }
        }

        private static void BuildSection5MusicArchiveHero(
            Dictionary<string, Transform> zoneMap, float vestY, MasterEraPalette mats, int l1929, int lRuins)
        {
            if (zoneMap.TryGetValue("21_Music_Library", out Transform z21))
            {
                BuildHeroChandelierPair(z21, new Vector3(15.0f, vestY + 4.8f, 72.0f), mats, l1929, lRuins);
            }
            if (zoneMap.TryGetValue("22_Grand_Piano_Room", out Transform z22))
            {
                BuildHeroChandelierPair(z22, new Vector3(17.0f, vestY + 4.8f, 85.0f), mats, l1929, lRuins);
                BuildHeroWindowPair(z22, new Vector3(25.6f, vestY, 85.0f), Quaternion.Euler(0f, -90f, 0f), mats, l1929, lRuins);
            }
        }

        private static void BuildSection6DressingRoomsAndBackstageHero(
            Dictionary<string, Transform> zoneMap, float vestY, MasterEraPalette mats, int l1929, int lRuins)
        {
            if (zoneMap.TryGetValue("26_Mara_Dressing_Room", out Transform z26))
            {
                // Ventanal en el muro lateral oeste (X = 26.35), tocador en el muro norte (29.75, 91.1)
                BuildHeroWindowPair(z26, new Vector3(26.35f, vestY, 86.2f), Quaternion.Euler(0f, 90f, 0f), mats, l1929, lRuins);
                BuildHollywoodVanityPair(z26, new Vector3(29.75f, vestY, 91.25f), Quaternion.identity, mats, l1929, lRuins);
                BuildHeroChandelierPair(z26, new Vector3(29.75f, vestY + 4.2f, 87.5f), mats, l1929, lRuins, 0.85f);
            }
            if (zoneMap.TryGetValue("27_Elena_Dressing_Room", out Transform z27))
            {
                // Ventanal en el muro lateral este (X = 40.65), tocador en el muro norte (37.25, 91.1)
                BuildHeroWindowPair(z27, new Vector3(40.65f, vestY, 86.2f), Quaternion.Euler(0f, -90f, 0f), mats, l1929, lRuins);
                BuildHollywoodVanityPair(z27, new Vector3(37.25f, vestY, 91.25f), Quaternion.identity, mats, l1929, lRuins);
                BuildTutuRackPair(z27, new Vector3(39.6f, vestY, 88.5f), mats, l1929, lRuins);
                BuildHeroChandelierPair(z27, new Vector3(37.25f, vestY + 4.2f, 87.5f), mats, l1929, lRuins, 0.85f);
            }
        }

        private static void BuildSection7GrandTheatreClimaxHero(
            Dictionary<string, Transform> zoneMap, float vestY, MasterEraPalette mats, int l1929, int lRuins)
        {
            if (zoneMap.TryGetValue("31_Auditorium_Seating", out Transform z31))
            {
                Transform d1929 = z31.Find("Decor_1929");
                Transform dRuins = z31.Find("Decor_Ruins");
                float audY = vestY - 0.8f;
                float roofY = vestY + 11.5f;

                BuildHeroChandelierPair(z31, new Vector3(-8.0f, roofY - 1.4f, 134.0f), mats, l1929, lRuins, 1.85f);

                float[] boxZ = { 126.5f, 134.0f, 141.5f };
                float[] boxLevels = { audY + 3.2f, audY + 6.4f };
                foreach (float bz in boxZ)
                {
                    foreach (float by in boxLevels)
                    {
                        for (int side = -1; side <= 1; side += 2)
                        {
                            float bx = side < 0 ? -24.2f : 8.2f;
                            VisWorld($"OperaBox_Balcony_1929_{side}_{bz}_{by}", PrimitiveType.Cube, d1929,
                                new Vector3(bx, by, bz), new Vector3(2.2f, 1.15f, 5.2f), mats.boiserieCream1929, l1929);
                            VisWorld($"OperaBox_GoldFrieze_1929_{side}_{bz}_{by}", PrimitiveType.Cube, d1929,
                                new Vector3(bx - side * 1.12f, by + 0.35f, bz), new Vector3(0.12f, 0.28f, 5.3f), mats.antiqueGold1929, l1929);
                            VisWorld($"OperaBox_VelvetRail_1929_{side}_{bz}_{by}", PrimitiveType.Cube, d1929,
                                new Vector3(bx - side * 1.10f, by + 0.62f, bz), new Vector3(0.24f, 0.14f, 5.3f), mats.velvetWine1929, l1929);
                            VisWorld($"OperaBox_WarmLamp_1929_{side}_{bz}_{by}", PrimitiveType.Sphere, d1929,
                                new Vector3(bx - side * 1.18f, by + 0.95f, bz), new Vector3(0.22f, 0.28f, 0.22f), mats.warmBulbEmit1929, l1929);

                            VisWorld($"OperaBox_Balcony_Ruins_{side}_{bz}_{by}", PrimitiveType.Cube, dRuins,
                                new Vector3(bx, by, bz), new Vector3(2.2f, 1.15f, 5.2f), mats.wallCrackedRuins, lRuins);
                            VisWorld($"OperaBox_RustedFrieze_Ruins_{side}_{bz}_{by}", PrimitiveType.Cube, dRuins,
                                new Vector3(bx - side * 1.12f, by + 0.35f, bz), new Vector3(0.12f, 0.28f, 5.3f), mats.rustedIronRuins, lRuins);
                        }
                    }
                }

                Vector3 holeCenter = new Vector3(-8.0f, roofY - 0.3f, 132.0f);
                VisWorld("CollapsedDome_SkyAperture_Ruins", PrimitiveType.Cylinder, dRuins,
                    holeCenter, new Vector3(11.5f, 0.15f, 9.5f), mats.moonBeamVolumetricRuins, lRuins);
                for (int r = 0; r < 10; r++)
                {
                    float ang = r * 36f * Mathf.Deg2Rad;
                    Vector3 rPos = holeCenter + new Vector3(Mathf.Cos(ang) * 5.2f, -0.35f, Mathf.Sin(ang) * 4.4f);
                    GameObject rafter = VisWorld($"CollapsedDome_BrokenRafter_{r + 1}", PrimitiveType.Cube, dRuins,
                        rPos, new Vector3(0.24f, 0.22f, 3.8f), mats.rottedWoodRuins, lRuins);
                    rafter.transform.localRotation = Quaternion.Euler(24f, r * 36f, 12f);
                }

                GameObject domeBeam = VisWorld("CollapsedDome_LunarShaft_Ruins", PrimitiveType.Cylinder, dRuins,
                    new Vector3(-8.0f, audY + 6.2f, 127.5f), new Vector3(8.5f, 6.4f, 7.5f), mats.moonBeamVolumetricRuins, lRuins);
                domeBeam.transform.localRotation = Quaternion.Euler(14f, 0f, 0f);
                domeBeam.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            }

            if (zoneMap.TryGetValue("32_Main_Stage", out Transform z32))
            {
                Transform d1929 = z32.Find("Decor_1929");
                Transform dRuins = z32.Find("Decor_Ruins");

                for (int f = -7; f <= 7; f++)
                {
                    float fx = -8.0f + f * 1.25f;
                    VisWorld($"StageFootlight_Bulb_1929_{f + 8}", PrimitiveType.Sphere, d1929,
                        new Vector3(fx, vestY + 0.14f, 115.65f), new Vector3(0.14f, 0.12f, 0.14f), mats.warmBulbEmit1929, l1929);
                }
                VisWorld("Proscenium_GoldBullionFringe_1929", PrimitiveType.Cube, d1929,
                    new Vector3(-8.0f, vestY + 8.45f, 115.35f), new Vector3(22.0f, 0.28f, 0.72f), mats.antiqueGold1929, l1929);

                for (int d = 0; d < 16; d++)
                {
                    float dx = -16.5f + d * 1.15f;
                    float dz = 111.5f + (d % 4) * 1.1f;
                    GameObject stageDebris = VisWorld($"StageApron_CollapsedRubble_{d + 1}", PrimitiveType.Cube, dRuins,
                        new Vector3(dx, vestY + 0.22f, dz),
                        new Vector3(0.85f + (d % 3) * 0.35f, 0.32f + (d % 2) * 0.22f, 0.75f),
                        (d % 2 == 0) ? mats.wallCrackedRuins : mats.rottedWoodRuins, lRuins);
                    stageDebris.transform.localRotation = Quaternion.Euler(d * 17f, d * 31f, d * 13f);
                }
            }
        }

        private static void BuildSection8SecretVaultHero(
            Dictionary<string, Transform> zoneMap, float vestY, MasterEraPalette mats, int l1929, int lRuins)
        {
            if (zoneMap.TryGetValue("38_Forbidden_Archives_Room", out Transform z38))
            {
                BuildHeroChandelierPair(z38, new Vector3(29.0f, -3.2f + 3.1f, 132.5f), mats, l1929, lRuins, 0.85f);
            }
        }

        private static void BuildSection9WinterGardenHero(
            Dictionary<string, Transform> zoneMap, float vestY, MasterEraPalette mats, int l1929, int lRuins)
        {
            if (zoneMap.TryGetValue("40_Winter_Garden_Conservatory", out Transform z40))
            {
                Transform d1929 = z40.Find("Decor_1929");
                Transform dRuins = z40.Find("Decor_Ruins");
                Vector3 fCenter = new Vector3(69.0f, vestY, 57.0f);

                VisWorld("WinterGarden_FountainWater_1929", PrimitiveType.Cylinder, d1929,
                    fCenter + new Vector3(0f, 0.55f, 0f), new Vector3(6.8f, 0.06f, 6.8f), mats.crystalWater1929, l1929);
                GameObject sunVaultBeam = VisWorld("WinterGarden_SunsetVaultBeam_1929", PrimitiveType.Cylinder, d1929,
                    fCenter + new Vector3(0f, 4.5f, 0f), new Vector3(9.5f, 4.5f, 9.5f), mats.sunBeamVolumetric1929, l1929);
                sunVaultBeam.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;

                for (int v = 0; v < 12; v++)
                {
                    float ang = v * 30f * Mathf.Deg2Rad;
                    Vector3 vp = fCenter + new Vector3(Mathf.Cos(ang) * 9.5f, 5.8f, Mathf.Sin(ang) * 14.5f);
                    VisWorld($"Conservatory_GiantHangingVine_{v + 1}", PrimitiveType.Capsule, dRuins,
                        vp, new Vector3(0.45f, 2.6f, 0.45f), mats.mossAndIvyRuins, lRuins);
                }
            }
        }

        private static void BuildSection10SouthwestAndBasementHero(
            Dictionary<string, Transform> zoneMap, float vestY, MasterEraPalette mats, int l1929, int lRuins)
        {
            if (zoneMap.TryGetValue("43_Old_Infirmary", out Transform z43))
            {
                BuildHeroChandelierPair(z43, new Vector3(-33.5f, vestY + 4.2f, 3.0f), mats, l1929, lRuins);
            }
            if (zoneMap.TryGetValue("44_Memorial_Hall", out Transform z44))
            {
                BuildHeroChandelierPair(z44, new Vector3(-26.2f, vestY + 4.8f, -13.0f), mats, l1929, lRuins);
            }
        }

        private static void BuildSection11WestBoardingWingHero(
            Dictionary<string, Transform> zoneMap, float vestY, MasterEraPalette mats, int l1929, int lRuins)
        {
            if (zoneMap.TryGetValue("45_Boarding_Students_Dormitory", out Transform z45))
            {
                BuildHeroChandelierPair(z45, new Vector3(-51.5f, vestY + 4.4f, 54.0f), mats, l1929, lRuins);
                BuildHeroWindowPair(z45, new Vector3(-64.5f, vestY, 54.0f), Quaternion.Euler(0f, 90f, 0f), mats, l1929, lRuins);
            }
            if (zoneMap.TryGetValue("47_Old_Dining_Hall_And_Patronage_Salon", out Transform z47))
            {
                BuildHeroChandelierPair(z47, new Vector3(-51.5f, vestY + 4.6f, 25.0f), mats, l1929, lRuins);
            }
            if (zoneMap.TryGetValue("48_Isadora_Private_Suite_And_Spy_Lattice", out Transform z48))
            {
                BuildHeroChandelierPair(z48, new Vector3(-51.5f, vestY + 4.5f, 2.0f), mats, l1929, lRuins);
            }
        }

        private static void BuildSection12UpperSubterraneanAndExteriorsHero(
            Dictionary<string, Transform> zoneMap, float vestY, MasterEraPalette mats, int l1929, int lRuins)
        {
            float upperY = 7.70f;
            float subY = -3.20f;

            if (zoneMap.TryGetValue("49_Mezzanine_Imperial_Gallery", out Transform z49))
            {
                BuildHeroChandelierPair(z49, new Vector3(0f, upperY + 4.2f, 20.0f), mats, l1929, lRuins);
            }
            if (zoneMap.TryGetValue("50_Theory_And_Anatomy_Amphitheatre", out Transform z50))
            {
                BuildHeroChandelierPair(z50, new Vector3(0f, upperY + 4.4f, 43.0f), mats, l1929, lRuins);
            }
            if (zoneMap.TryGetValue("52_Clock_Tower_And_High_Attic", out Transform z52))
            {
                Transform d1929 = z52.Find("Decor_1929");
                Transform dRuins = z52.Find("Decor_Ruins");
                VisWorld("ClockTower_SunDialGlow_1929", PrimitiveType.Cylinder, d1929,
                    new Vector3(0f, upperY + 3.2f, 87.5f), new Vector3(4.2f, 0.05f, 4.2f), mats.sunsetGlassEmit1929, l1929)
                    .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                VisWorld("ClockTower_BrokenLunarBeam_Ruins", PrimitiveType.Cylinder, dRuins,
                    new Vector3(0f, upperY + 2.2f, 82.5f), new Vector3(3.4f, 4.5f, 3.4f), mats.moonBeamVolumetricRuins, lRuins)
                    .transform.localRotation = Quaternion.Euler(70f, 0f, 0f);
            }
            if (zoneMap.TryGetValue("53_Boiler_Room_And_Electrical_Generators", out Transform z53))
            {
                Transform d1929 = z53.Find("Decor_1929");
                VisWorld("Boiler_FireboxGlow_1929", PrimitiveType.Sphere, d1929,
                    new Vector3(0f, subY + 1.2f, -24.0f), new Vector3(1.6f, 1.1f, 1.6f), mats.fireGlowEmit1929, l1929);
            }
        }

        // ====================================================================
        // MÓDULOS REUTILIZABLES DE ESCENOGRAFÍA (TOCADOR, PERCHERO, VENTANAL, ARAÑA)
        // ====================================================================
        private static void BuildHollywoodVanityPair(Transform zoneRoot, Vector3 basePos, Quaternion rot, MasterEraPalette mats, int l1929, int lRuins)
        {
            Transform d1929 = zoneRoot.Find("Decor_1929");
            Transform dRuins = zoneRoot.Find("Decor_Ruins");
            if (d1929 == null || dRuins == null) return;

            GameObject v1929 = new GameObject("HollywoodVanity_1929");
            v1929.transform.SetParent(d1929, false);
            v1929.transform.position = basePos;
            v1929.transform.rotation = rot;
            v1929.layer = l1929;

            // Marco barroco dorado propio del espejo para que las bombillas siempre estén ancladas a un marco físico real
            VisLocal("Vanity_GoldMirrorFrame_1929", PrimitiveType.Cube, v1929.transform,
                new Vector3(0f, 1.92f, 0.22f), new Vector3(1.64f, 1.96f, 0.08f), mats.antiqueGold1929, l1929);
            VisLocal("Vanity_MirrorGlass_1929", PrimitiveType.Cube, v1929.transform,
                new Vector3(0f, 1.92f, 0.17f), new Vector3(1.42f, 1.74f, 0.03f), mats.mirrorIntact1929, l1929);

            for (int side = -1; side <= 1; side += 2)
            {
                for (int b = 0; b < 5; b++)
                {
                    VisLocal($"Bulb_Side_{side}_{b}", PrimitiveType.Sphere, v1929.transform,
                        new Vector3(side * 0.75f, 1.10f + b * 0.38f, 0.15f), new Vector3(0.07f, 0.07f, 0.07f), mats.warmBulbEmit1929, l1929);
                }
            }
            for (int t = -2; t <= 2; t++)
            {
                VisLocal($"Bulb_Top_{t + 3}", PrimitiveType.Sphere, v1929.transform,
                    new Vector3(t * 0.30f, 2.84f, 0.15f), new Vector3(0.07f, 0.07f, 0.07f), mats.warmBulbEmit1929, l1929);
            }

            // Jarrón apoyado sobre la esquina de la mesa de tocador (Y = 0.96m)
            VisLocal("Vanity_RoseVase_1929", PrimitiveType.Cylinder, v1929.transform,
                new Vector3(-0.78f, 0.98f, 0f), new Vector3(0.14f, 0.12f, 0.14f), mats.boiserieCream1929, l1929);
            for (int r = 0; r < 5; r++)
            {
                VisLocal($"Vanity_RoseBloom_{r + 1}", PrimitiveType.Sphere, v1929.transform,
                    new Vector3(-0.78f + Mathf.Cos(r * 1.2f) * 0.06f, 1.14f + (r % 2) * 0.03f, Mathf.Sin(r * 1.2f) * 0.06f),
                    new Vector3(0.08f, 0.07f, 0.08f), (r % 2 == 0) ? mats.roseLavender1929 : mats.velvetWine1929, l1929);
            }

            // RUINAS: Espejo pesado con marco barroco oxidado inclinado, cristal partido y hiedra
            GameObject vRuins = new GameObject("HollywoodVanity_Ruins");
            vRuins.transform.SetParent(dRuins, false);
            vRuins.transform.position = basePos;
            vRuins.transform.rotation = rot;
            vRuins.layer = lRuins;

            GameObject tiltedFrame = VisLocal("TiltedBaroqueMirror_Ruins", PrimitiveType.Cube, vRuins.transform,
                new Vector3(0f, 1.88f, 0.20f), new Vector3(1.64f, 1.96f, 0.09f), mats.rustedIronRuins, lRuins);
            tiltedFrame.transform.localRotation = Quaternion.Euler(-11f, 4f, -4f);
            VisLocal("OvergrownIvy_VanityTop_Ruins", PrimitiveType.Sphere, vRuins.transform,
                new Vector3(-0.62f, 2.75f, 0.16f), new Vector3(1.10f, 0.78f, 0.32f), mats.mossAndIvyRuins, lRuins);
        }

        private static void BuildTutuRackPair(Transform zoneRoot, Vector3 basePos, MasterEraPalette mats, int l1929, int lRuins)
        {
            Transform d1929 = zoneRoot.Find("Decor_1929");
            Transform dRuins = zoneRoot.Find("Decor_Ruins");
            if (d1929 == null || dRuins == null) return;

            GameObject r1929 = new GameObject("TutuGarmentRack_1929");
            r1929.transform.SetParent(d1929, false);
            r1929.transform.position = basePos;
            r1929.layer = l1929;

            VisLocal("Rack_TopBar_1929", PrimitiveType.Cylinder, r1929.transform,
                new Vector3(0f, 1.85f, 0f), new Vector3(0.04f, 0.95f, 0.04f), mats.antiqueGold1929, l1929)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            for (int t = -1; t <= 1; t++)
            {
                Material dressMat = (t == 0) ? mats.roseLavender1929 : mats.boiserieCream1929;
                VisLocal($"HangingTutu_Bodice_{t + 2}", PrimitiveType.Cylinder, r1929.transform,
                    new Vector3(0f, 1.45f, t * 0.52f), new Vector3(0.24f, 0.28f, 0.18f), dressMat, l1929);
                VisLocal($"HangingTutu_Skirt_{t + 2}", PrimitiveType.Cylinder, r1929.transform,
                    new Vector3(0f, 1.02f, t * 0.52f), new Vector3(0.62f, 0.24f, 0.52f), dressMat, l1929);
            }

            GameObject rRuins = new GameObject("TutuGarmentRack_Ruins");
            rRuins.transform.SetParent(dRuins, false);
            rRuins.transform.position = basePos;
            rRuins.layer = lRuins;

            GameObject fallenBar = VisLocal("Rack_CollapsedBar_Ruins", PrimitiveType.Cylinder, rRuins.transform,
                new Vector3(-0.25f, 0.65f, 0f), new Vector3(0.04f, 0.95f, 0.04f), mats.rustedIronRuins, lRuins);
            fallenBar.transform.localRotation = Quaternion.Euler(68f, 15f, 22f);
            VisLocal("TornTutu_Pile_Ruins", PrimitiveType.Sphere, rRuins.transform,
                new Vector3(-0.32f, 0.18f, 0.15f), new Vector3(0.95f, 0.28f, 1.15f), mats.shroudClothRuins, lRuins);
        }

        private static void BuildHeroWindowPair(Transform zoneRoot, Vector3 basePos, Quaternion rot, MasterEraPalette mats, int l1929, int lRuins)
        {
            Transform d1929 = zoneRoot.Find("Decor_1929");
            Transform dRuins = zoneRoot.Find("Decor_Ruins");
            if (d1929 == null || dRuins == null) return;

            GameObject w1929 = new GameObject("HeroArchedWindow_1929");
            w1929.transform.SetParent(d1929, false);
            w1929.transform.position = basePos;
            w1929.transform.rotation = rot;
            w1929.layer = l1929;

            VisLocal("SunsetGlass_1929", PrimitiveType.Cube, w1929.transform,
                new Vector3(0f, 2.6f, 0f), new Vector3(2.2f, 3.6f, 0.05f), mats.sunsetGlassEmit1929, l1929);
            VisLocal("VelvetCurtain_L_1929", PrimitiveType.Cylinder, w1929.transform,
                new Vector3(-1.18f, 2.5f, -0.14f), new Vector3(0.38f, 2.1f, 0.20f), mats.velvetWine1929, l1929);
            VisLocal("VelvetCurtain_R_1929", PrimitiveType.Cylinder, w1929.transform,
                new Vector3(1.18f, 2.5f, -0.14f), new Vector3(0.38f, 2.1f, 0.20f), mats.velvetWine1929, l1929);
            GameObject warmShaft = VisLocal("WarmSunShaft_1929", PrimitiveType.Cube, w1929.transform,
                new Vector3(0f, 2.0f, -3.2f), new Vector3(2.0f, 0.04f, 6.2f), mats.sunBeamVolumetric1929, l1929);
            warmShaft.transform.localRotation = Quaternion.Euler(24f, 0f, 0f);
            warmShaft.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;

            GameObject wRuins = new GameObject("HeroArchedWindow_Ruins");
            wRuins.transform.SetParent(dRuins, false);
            wRuins.transform.position = basePos;
            wRuins.transform.rotation = rot;
            wRuins.layer = lRuins;

            VisLocal("TornCurtain_L_Ruins", PrimitiveType.Cube, wRuins.transform,
                new Vector3(-1.15f, 2.3f, -0.14f), new Vector3(0.32f, 2.6f, 0.10f), mats.tornWineVelvetRuins, lRuins)
                .transform.localRotation = Quaternion.Euler(0f, 0f, -11f);
            for (int v = 0; v < 5; v++)
            {
                VisLocal($"WindowIvyCluster_Ruins_{v + 1}", PrimitiveType.Sphere, wRuins.transform,
                    new Vector3(-0.9f + v * 0.45f, 3.6f - (v % 2) * 0.5f, -0.12f),
                    new Vector3(0.75f, 0.65f, 0.32f), mats.mossAndIvyRuins, lRuins);
                VisLocal($"WindowHangingVine_Ruins_{v + 1}", PrimitiveType.Capsule, wRuins.transform,
                    new Vector3(-0.9f + v * 0.45f, 2.5f - (v % 3) * 0.35f, -0.14f),
                    new Vector3(0.16f, 0.85f, 0.14f), mats.mossAndIvyRuins, lRuins);
            }
            GameObject coldShaft = VisLocal("ColdMoonShaft_Ruins", PrimitiveType.Cube, wRuins.transform,
                new Vector3(0f, 2.0f, -3.2f), new Vector3(1.85f, 0.04f, 6.0f), mats.moonBeamVolumetricRuins, lRuins);
            coldShaft.transform.localRotation = Quaternion.Euler(24f, 0f, 0f);
            coldShaft.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        private static void BuildHeroChandelierPair(Transform zoneRoot, Vector3 pos, MasterEraPalette mats, int l1929, int lRuins, float scaleMult = 1.0f)
        {
            Transform d1929 = zoneRoot.Find("Decor_1929");
            Transform dRuins = zoneRoot.Find("Decor_Ruins");
            if (d1929 == null || dRuins == null) return;

            BuildScaledChandelier(d1929, "HeroChandelier_1929", pos, scaleMult, true, mats, l1929);
            BuildScaledChandelier(dRuins, "HeroChandelier_Ruins", pos, scaleMult, false, mats, lRuins);
        }

        private static void BuildScaledChandelier(Transform parent, string name, Vector3 pos, float scaleMult, bool is1929, MasterEraPalette mats, int layer)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.position = pos;
            root.transform.localScale = Vector3.one * scaleMult;
            root.layer = layer;
            if (!is1929) root.transform.localRotation = Quaternion.Euler(16f, 24f, -19f);

            Material metal = is1929 ? mats.antiqueGold1929 : mats.rustedIronRuins;
            Material bulb = is1929 ? mats.warmBulbEmit1929 : mats.brokenMirrorRuins;

            VisLocal("Chain", PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.35f, 0f), new Vector3(0.05f, 0.35f, 0.05f), metal, layer);
            VisLocal("CrownRing", PrimitiveType.Cylinder, root.transform, new Vector3(0f, -0.35f, 0f), new Vector3(1.45f, 0.04f, 1.45f), metal, layer);
            VisLocal("CoreUrn", PrimitiveType.Sphere, root.transform, new Vector3(0f, -0.25f, 0f), new Vector3(0.36f, 0.42f, 0.36f), metal, layer);

            int arms = 8;
            for (int a = 0; a < arms; a++)
            {
                float ang = a * (360f / arms) * Mathf.Deg2Rad;
                Vector3 cp = new Vector3(Mathf.Cos(ang) * 0.85f, -0.22f, Mathf.Sin(ang) * 0.85f);
                VisLocal($"Candle_{a + 1}", PrimitiveType.Sphere, root.transform, cp, new Vector3(0.09f, 0.14f, 0.09f), bulb, layer);
            }

            if (is1929)
            {
                GameObject lObj = new GameObject("ChandelierWarmLight");
                lObj.transform.SetParent(root.transform, false);
                lObj.transform.localPosition = new Vector3(0f, -0.4f, 0f);
                lObj.layer = layer;
                Light l = lObj.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = new Color(0.98f, 0.93f, 0.86f);
                l.intensity = 2.6f * scaleMult;
                l.range = 16f * scaleMult;
                l.shadows = LightShadows.None;
            }
            else
            {
                VisLocal("ChandelierCobweb", PrimitiveType.Cube, root.transform,
                    new Vector3(0f, -0.25f, 0f), new Vector3(1.55f, 0.52f, 0.01f), mats.cobwebTranslucentRuins, layer);
            }
        }

        private static void BuildBallerinaSculpture(Transform parent, string name, Vector3 basePos, Material bodyMat, Material accentMat, bool ruined, int layer)
        {
            GameObject s = new GameObject(name);
            s.transform.SetParent(parent, false);
            s.transform.position = basePos;
            s.layer = layer;
            if (ruined) s.transform.localRotation = Quaternion.Euler(11f, 18f, -14f);

            VisLocal("Supporting_Leg", PrimitiveType.Capsule, s.transform, new Vector3(0f, 0.48f, 0f), new Vector3(0.11f, 0.48f, 0.11f), bodyMat, layer);
            GameObject arabesqueLeg = VisLocal("Arabesque_Leg", PrimitiveType.Capsule, s.transform, new Vector3(0f, 0.92f, -0.45f), new Vector3(0.10f, 0.44f, 0.10f), bodyMat, layer);
            arabesqueLeg.transform.localRotation = Quaternion.Euler(78f, 0f, 0f);
            VisLocal("Tutu_Skirt", PrimitiveType.Cylinder, s.transform, new Vector3(0f, 0.96f, 0f), new Vector3(0.78f, 0.05f, 0.78f), accentMat, layer);
            VisLocal("Torso", PrimitiveType.Capsule, s.transform, new Vector3(0f, 1.32f, 0.08f), new Vector3(0.22f, 0.34f, 0.18f), bodyMat, layer);
            if (!ruined)
            {
                VisLocal("Head", PrimitiveType.Sphere, s.transform, new Vector3(0f, 1.78f, 0.14f), new Vector3(0.20f, 0.23f, 0.20f), bodyMat, layer);
                VisLocal("Crown", PrimitiveType.Cylinder, s.transform, new Vector3(0f, 1.91f, 0.14f), new Vector3(0.12f, 0.03f, 0.12f), accentMat, layer);
            }
        }

        private static void BuildZoneParticles(Transform parent, Vector3 pos, Vector3 boxScale, bool is1929, MasterEraPalette mats, int layer)
        {
            GameObject pObj = new GameObject(is1929 ? "ZoneDust_1929" : "ZoneSpores_Ruins");
            pObj.transform.SetParent(parent, false);
            pObj.transform.position = pos;
            pObj.layer = layer;

            ParticleSystem ps = pObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 11f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
            main.startSize = is1929 ? new ParticleSystem.MinMaxCurve(0.02f, 0.045f) : new ParticleSystem.MinMaxCurve(0.035f, 0.085f);
            main.startColor = is1929 ? new Color(1.0f, 0.84f, 0.52f, 0.36f) : new Color(0.56f, 0.66f, 0.78f, 0.34f);
            main.maxParticles = 65;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var em = ps.emission;
            em.rateOverTime = 9f;

            var sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Box;
            sh.scale = boxScale;

            var psr = pObj.GetComponent<ParticleSystemRenderer>();
            if (psr != null)
            {
                psr.sharedMaterial = is1929 ? mats.sunBeamVolumetric1929 : mats.moonBeamVolumetricRuins;
                psr.shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        private static void AdaptRemainingSharedRenderers(Transform rootEnv, MasterEraPalette mats)
        {
            Renderer[] allRenderers = rootEnv.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in allRenderers)
            {
                if (r == null || r.GetComponent<EraSurfaceAdapter>() != null) continue;
                if (r.gameObject.layer == EraVisualManager.GetEra1929Layer() || r.gameObject.layer == EraVisualManager.GetEraRuinsLayer()) continue;
                if (r.GetComponent<WorldDocument>() != null || r.GetComponent<SequenceNode>() != null || r.GetComponent<WorldItem>() != null) continue;

                ClassifyAndAttachSurfaceAdapter(r, r.gameObject.name.ToLowerInvariant(), false, false, false, mats);
            }
        }

        // ====================================================================
        // PALETA DE MATERIALES PBR COMPARTIDOS CON GPU INSTANCING
        // ====================================================================
        private static MasterEraPalette BuildMasterPalette()
        {
            MasterEraPalette p = new MasterEraPalette();

            // 1929 (Palacio Mágico y Misterioso)
            p.parquet1929 = LoadOrMakePBR("Mat_Era1929_ParquetGlory", new Color(0.95f, 0.82f, 0.65f), "Tex_PalaceParquet1929_Albedo.png", "Tex_Parquet_Normal.png", new Vector2(6f, 6f), 0.72f, 0.04f);
            p.marble1929 = LoadOrMakePBR("Mat_Era1929_VenetianMarble", new Color(0.94f, 0.90f, 0.84f), "Tex_Marble_Albedo.png", "Tex_Marble_Normal.png", new Vector2(5f, 5f), 0.78f, 0.06f);
            p.travertine1929 = LoadOrMakePBR("Mat_Era1929_CleanTravertine", new Color(0.88f, 0.82f, 0.74f), "Tex_Facade_Albedo.png", "Tex_Facade_Normal.png", new Vector2(6f, 6f), 0.48f, 0.02f);
            p.porcelainTile1929 = LoadOrMakePBR("Mat_Era1929_PorcelainTile", new Color(0.95f, 0.92f, 0.88f), "Tex_CeramicTile_Albedo.png", "Tex_CeramicTile_Normal.png", new Vector2(8f, 8f), 0.76f, 0.04f);
            p.wallIvory1929 = LoadOrMakePBR("Mat_Era1929_WallIvory", EraPilotStudioBuilder.Col1929Ivory, "Tex_PalaceWall1929_Albedo.png", "Tex_PalaceWall1929_Normal.png", new Vector2(5f, 2f), 0.38f, 0f);
            p.boiserieCream1929 = LoadOrMakePBR("Mat_Era1929_BoiserieCream", new Color(0.94f, 0.89f, 0.82f), "Tex_PalaceWall1929_Albedo.png", "Tex_PalaceWall1929_Normal.png", new Vector2(3f, 1.5f), 0.46f, 0.02f);
            p.antiqueGold1929 = LoadOrMakePBR("Mat_Era1929_AntiqueGold", new Color(0.74f, 0.56f, 0.24f), null, "Tex_Marble_Normal.png", new Vector2(2f, 2f), 0.70f, 0.62f);
            p.mahogany1929 = LoadOrMakePBR("Mat_Era1929_PolishedMahogany", new Color(0.46f, 0.22f, 0.14f), "Tex_Mahogany_Albedo.png", "Tex_Mahogany_Normal.png", new Vector2(3f, 3f), 0.64f, 0.03f);
            p.velvetWine1929 = LoadOrMakePBR("Mat_Era1929_VelvetWine", EraPilotStudioBuilder.Col1929Wine, "Tex_VelvetCarpet_Albedo.png", "Tex_VelvetCarpet_Normal.png", new Vector2(3f, 3f), 0.22f, 0.02f);
            p.velvetEmerald1929 = LoadOrMakePBR("Mat_Era1929_VelvetEmerald", new Color(0.16f, 0.34f, 0.23f), "Tex_GreenVelvet_Albedo.png", "Tex_GreenVelvet_Normal.png", new Vector2(3f, 3f), 0.24f, 0.02f);
            p.roseLavender1929 = LoadOrMakePBR("Mat_Era1929_RoseLavender", EraPilotStudioBuilder.Col1929RoseLavender, null, "Tex_VelvetCarpet_Normal.png", new Vector2(2f, 2f), 0.48f, 0f);
            p.mirrorIntact1929 = LoadOrMakePBR("Mat_Era1929_MirrorIntact", new Color(0.90f, 0.93f, 0.96f), null, null, Vector2.one, 0.96f, 0.92f);
            p.warmBulbEmit1929 = LoadAssetMat("Mat_Era1929_CrystalBulb", new Color(1.0f, 0.88f, 0.62f), true);
            p.sunsetGlassEmit1929 = LoadAssetMat("Mat_Era1929_SunsetGlass", new Color(1.0f, 0.82f, 0.62f), true);
            p.sunBeamVolumetric1929 = LoadAssetMat("Mat_Era1929_SunBeamVolumetric", new Color(1.0f, 0.78f, 0.54f, 0.09f), false);
            p.crystalWater1929 = LoadOrMakePBR("Mat_Era1929_CrystalWater", new Color(0.32f, 0.54f, 0.66f), null, "Tex_Marble_Normal.png", new Vector2(3f, 3f), 0.94f, 0.15f);
            p.fireGlowEmit1929 = LoadAssetMat("Mat_Era1929_BoilerFireGlow", new Color(1.0f, 0.45f, 0.12f), true);

            // Ruinas (Película de Terror Psicológico)
            p.parquetRuins = LoadOrMakePBR("Mat_EraRuins_ParquetDecayed", new Color(0.19f, 0.17f, 0.15f), "Tex_Parquet_Albedo.png", "Tex_Parquet_Normal.png", new Vector2(7f, 7f), 0.22f, 0.02f);
            p.marbleCrackedRuins = LoadOrMakePBR("Mat_EraRuins_CrackedMarble", new Color(0.22f, 0.21f, 0.20f), "Tex_Marble_Albedo.png", "Tex_Marble_Normal.png", new Vector2(5f, 5f), 0.24f, 0.04f);
            p.mossyCobbleRuins = LoadOrMakePBR("Mat_EraRuins_MossyCobble", new Color(0.20f, 0.22f, 0.18f), "Tex_Cobble_Albedo.png", "Tex_Cobble_Normal.png", new Vector2(6f, 6f), 0.18f, 0f);
            p.crackedTileRuins = LoadOrMakePBR("Mat_EraRuins_CrackedTile", new Color(0.26f, 0.27f, 0.24f), "Tex_CeramicTile_Albedo.png", "Tex_CeramicTile_Normal.png", new Vector2(8f, 8f), 0.20f, 0f);
            p.wallCrackedRuins = LoadOrMakePBR("Mat_EraRuins_CrackedPlaster", new Color(0.75f, 0.73f, 0.70f), "Tex_HorrorRuinsWall_Albedo.png", "Tex_HorrorRuinsWall_Normal.png", new Vector2(3.5f, 1.8f), 0.12f, 0f);
            p.exposedBrickRuins = LoadOrMakePBR("Mat_EraRuins_ExposedBrick", new Color(0.24f, 0.14f, 0.12f), "Tex_Cobble_Albedo.png", "Tex_Cobble_Normal.png", new Vector2(6f, 6f), 0.10f, 0f);
            p.abyssCrackRuins = LoadOrMakePBR("Mat_EraRuins_AbyssCrack", new Color(0.04f, 0.04f, 0.05f), null, "Tex_Cobble_Normal.png", new Vector2(2f, 2f), 0.05f, 0f);
            p.rustedIronRuins = LoadOrMakePBR("Mat_EraRuins_RustedIron", new Color(0.22f, 0.18f, 0.16f), "Tex_LockerMetal_Albedo.png", "Tex_LockerMetal_Normal.png", new Vector2(3f, 3f), 0.20f, 0.62f);
            p.rottedWoodRuins = LoadOrMakePBR("Mat_EraRuins_RottedWood", new Color(0.18f, 0.16f, 0.14f), "Tex_Mahogany_Albedo.png", "Tex_Mahogany_Normal.png", new Vector2(3f, 3f), 0.12f, 0f);
            p.tornWineVelvetRuins = LoadOrMakePBR("Mat_EraRuins_TornWineVelvet", EraPilotStudioBuilder.ColRuinsDullWine, "Tex_VelvetCarpet_Albedo.png", "Tex_VelvetCarpet_Normal.png", new Vector2(3f, 3f), 0.08f, 0f);
            p.tornGreenVelvetRuins = LoadOrMakePBR("Mat_EraRuins_TornGreenVelvet", EraPilotStudioBuilder.ColRuinsMoss, "Tex_GreenVelvet_Albedo.png", "Tex_GreenVelvet_Normal.png", new Vector2(3f, 3f), 0.08f, 0f);
            p.shroudClothRuins = LoadOrMakePBR("Mat_EraRuins_HorrorShroudCloth", new Color(0.36f, 0.35f, 0.33f), "Tex_HorrorRuinsWall_Albedo.png", "Tex_VelvetCarpet_Normal.png", new Vector2(2f, 2f), 0.10f, 0f);
            p.mossAndIvyRuins = LoadOrMakePBR("Mat_EraRuins_MossAndIvy", EraPilotStudioBuilder.ColRuinsMoss, "Tex_Facade_Albedo.png", "Tex_Facade_Normal.png", new Vector2(4f, 4f), 0.14f, 0f);
            p.darkVineBarkRuins = LoadOrMakePBR("Mat_EraRuins_VineBark", new Color(0.12f, 0.11f, 0.09f), "Tex_Mahogany_Albedo.png", "Tex_Mahogany_Normal.png", new Vector2(2f, 6f), 0.10f, 0f);
            p.brokenMirrorRuins = LoadOrMakePBR("Mat_EraRuins_BrokenMirror", new Color(0.38f, 0.44f, 0.48f), "Tex_Marble_Albedo.png", "Tex_Marble_Normal.png", new Vector2(2f, 2f), 0.68f, 0.65f);
            p.waterPuddleRuins = LoadOrMakePBR("Mat_EraRuins_WaterPuddle", new Color(0.06f, 0.08f, 0.10f), null, "Tex_Marble_Normal.png", new Vector2(2f, 2f), 0.96f, 0.25f);
            p.moonBeamVolumetricRuins = LoadAssetMat("Mat_EraRuins_MoonBeamVolumetric", new Color(0.56f, 0.65f, 0.76f, 0.07f), false);
            p.cobwebTranslucentRuins = LoadAssetMat("Mat_EraRuins_CobwebTranslucent", new Color(0.72f, 0.74f, 0.76f, 0.18f), false);

            return p;
        }

        private static Material LoadOrMakePBR(string name, Color tint, string albedoFile, string normalFile, Vector2 tiling, float smoothness, float metallic)
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

        private static Material LoadAssetMat(string name, Color fallbackCol, bool emissive)
        {
            string fullPath = EraMatDir + name + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(fullPath);
            if (mat != null) return mat;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            mat = new Material(shader);
            mat.SetColor("_BaseColor", fallbackCol);
            mat.color = fallbackCol;
            if (emissive)
            {
                mat.SetColor("_EmissionColor", fallbackCol * 2.5f);
                mat.EnableKeyword("_EMISSION");
            }
            mat.enableInstancing = true;
            AssetDatabase.CreateAsset(mat, fullPath);
            return mat;
        }

        private static GameObject VisWorld(string name, PrimitiveType type, Transform parent, Vector3 worldPos, Vector3 scale, Material mat, int layer)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            Collider col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            go.layer = layer;
            go.transform.SetParent(parent, true);
            go.transform.position = worldPos;
            go.transform.localScale = scale;

            if (mat != null)
            {
                Renderer r = go.GetComponent<Renderer>();
                if (r != null) r.sharedMaterial = mat;
            }
            return go;
        }

        private static GameObject VisLocal(string name, PrimitiveType type, Transform parent, Vector3 localPos, Vector3 localScale, Material mat, int layer)
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

        private static Transform FindTransformRecursive(Transform root, string targetName)
        {
            if (root == null) return null;
            if (root.name == targetName) return root;
            foreach (Transform child in root)
            {
                Transform found = FindTransformRecursive(child, targetName);
                if (found != null) return found;
            }
            return null;
        }

        private static void SetLayerAndStaticRecursive(Transform root, int layer)
        {
            if (root == null) return;
            root.gameObject.layer = layer;
            GameObjectUtility.SetStaticEditorFlags(root.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
            foreach (Transform child in root)
            {
                SetLayerAndStaticRecursive(child, layer);
            }
        }
    }
}
#endif
