using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UltimoActo.Core;
using UltimoActo.Interaction;
using UltimoActo.Puzzles;

namespace UltimoActo.UI
{
    /// <summary>
    /// BLOQUE 1 — MAPA DINÁMICO POR NOCHE Y ÉPOCA (AcademyMapUI, teclas [H] y [M]):
    /// Diseño limpio, compacto y legible sin superponer plantas ni saturar la pantalla:
    ///  - Modo Compacto (por defecto, 235x190 px arriba a la derecha): Radar local centrado en el jugador
    ///    que muestra ÚNICAMENTE la planta actual (Planta Principal, Planta Alta o Subterráneo) con zoom
    ///    cercano para que las salas se vean claras, separadas y sin amontonarse.
    ///  - Modo Plano Ampliado ([M], 520x420 px): Permite zoom con la rueda del ratón, desplazamiento
    ///    arrastrando con el ratón, selector de Planta (Baja / Alta / Sótano) y filtro por Sección (Todas / Noche Actual).
    ///  - Guía Dinámica ([H]): Generada 100% desde el estado real del juego (noche actual I–VIII, cuota de
    ///    evidencias, llaves obtenidas en inventario, estado real de puertas y puzles de la noche activa).
    ///  - Estética Dual por Época: Pergamino/plano dorado elegante en 1929 vs. plano deteriorado pizarra/musgo
    ///    con anotaciones de daños ("[Derrumbe]", "[Clausurado]") en las Ruinas.
    ///  - Niebla de Guerra Compartida: Solo las zonas visitadas por cualquiera de los dos jugadores muestran
    ///    su interior y rótulo; las no visitadas aparecen como silueta discreta ("?").
    ///  - Marcador opcional del último punto conocido del compañero (configurable para conservar el misterio).
    /// </summary>
    public class AcademyMapUI : MonoBehaviour
    {
        public static AcademyMapUI Instance { get; private set; }

        /// <summary>
        /// Evento disparado cuando el jugador local descubre una zona nueva para sincronizar la niebla de guerra con el compañero.
        /// </summary>
        public static event Action<string> OnLocalZoneVisitedForNetwork;

        public enum MapFloorLevel
        {
            MainFloor = 0,   // Y entre -1m y +6m (Zonas 1-33, 36, 39, 40, 43-48, 57-59)
            UpperFloor = 1,  // Y >= 6m (Zonas 34, 35, 42, 49-52)
            Subterranean = 2 // Y <= -1m (Zonas 37, 38, 41, 53-56)
        }

        [Serializable]
        private class ZoneMapNode
        {
            public string id;
            public int zoneNumber;
            public int sectionIndex;
            public int unlockNightIndex;
            public MapFloorLevel floor;
            public string label1929;
            public string labelRuins;
            public string fullName;
            public Rect worldXZ;
            public RectTransform rt;
            public Image fillImg;
            public Image borderImg;
            public TextMeshProUGUI labelTMP;
            public GameObject poiDotObj;
            public Image poiDotImg;
        }

        private RectTransform mapContainer;
        private Image outerBgImage;
        private Image outerBorderImage;
        private Image headerBgImage;
        private TextMeshProUGUI headerZoneTMP;
        private TextMeshProUGUI headerFloorBadgeTMP;

        // Área con máscara (RectMask2D) para que el zoom/paneo nunca se salga del recuadro
        private RectTransform viewportMaskRt;
        private Image viewportBgImage;
        private RectTransform mapCanvasContentRt;

        // Barra de controles (visible al ampliar con [M])
        private GameObject controlsToolbarObj;
        private TextMeshProUGUI btnFloorFilterTMP;
        private TextMeshProUGUI btnSectionFilterTMP;
        private TextMeshProUGUI btnPartnerToggleTMP;

        // Marcadores de jugador y compañero
        private RectTransform playerDot;
        private RectTransform playerDirectionLine;
        private RectTransform partnerDot;
        private Image partnerDotImg;

        // Tarjeta inferior compacta y dinámica ([H])
        private GameObject guidePanelObj;
        private Image guideBgImage;
        private Image guideBorderImage;
        private TextMeshProUGUI dynamicGuideTMP;

        private Transform playerTransform;
        private Transform cameraTransform;
        private SimpleInventory playerInventory;

        private readonly List<ZoneMapNode> zones = new List<ZoneMapNode>();
        private readonly HashSet<string> visitedZonesShared = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, WorldDoor> sceneDoorsById = new Dictionary<string, WorldDoor>(StringComparer.OrdinalIgnoreCase);

        private bool isExpanded = false;
        private bool isGuideVisible = true;
        private bool showPartnerLastKnownPos = true;
        private bool filterCurrentNightZonesOnly = false;
        private MapFloorLevel activePlayerFloor = MapFloorLevel.MainFloor;
        private MapFloorLevel manualViewedFloor = MapFloorLevel.MainFloor;
        private bool useManualFloorOverride = false;

        // Zoom y desplazamiento (Pan)
        private float compactLocalZoom = 2.65f; // En modo esquina hace zoom local limpio alrededor del jugador
        private float expandedZoom = 1.25f;
        private Vector2 expandedPanOffset = Vector2.zero;
        private bool isDraggingMap = false;
        private Vector2 lastMouseDragPos;

        // Última posición conocida del compañero (se actualiza periódicamente para mantener misterio)
        private Vector3 partnerLastKnownWorldPos;
        private float partnerLastSampleTime;
        private bool hasPartnerSample;

        // Límites globales del plano en XZ
        private const float WorldMinX = -68f;
        private const float WorldMaxX = 96f;
        private const float WorldMinZ = -38f;
        private const float WorldMaxZ = 154f;
        private const float BaseContentPixels = 360f;

        // Tamaños mucho más discretos y limpios (nunca invasivos)
        private readonly Vector2 compactSize = new Vector2(238f, 196f);
        private readonly Vector2 expandedSize = new Vector2(490f, 390f);

        private string currentRoomId = "z1_3";
        private float nextDoorCacheRefreshTime;

        private void Awake()
        {
            Instance = this;
            showPartnerLastKnownPos = PlayerPrefs.GetInt("UltimoActo_Map_ShowPartner", 1) == 1;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            FindPlayerReferences();
            EnsureMapUIHierarchy();
            VisitedZone("z1_3", false);
            VisitedZone("z4", false);
        }

        private void FindPlayerReferences()
        {
            if (UltimoActo.Network.NetworkCoopPlayer.LocalPlayer != null)
            {
                playerTransform = UltimoActo.Network.NetworkCoopPlayer.LocalPlayer.transform;
                playerInventory = playerTransform.GetComponent<SimpleInventory>();
            }
            else
            {
                GameObject pObj = GameObject.FindGameObjectWithTag("Player");
                if (pObj != null)
                {
                    playerTransform = pObj.transform;
                    playerInventory = pObj.GetComponent<SimpleInventory>();
                }
            }

            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        public void BindLocalPlayer(Transform localPlayer, Transform localCamera, SimpleInventory localInventory)
        {
            playerTransform = localPlayer;
            cameraTransform = localCamera;
            playerInventory = localInventory;
        }

        // =========================================================================================
        // NIEBLA DE GUERRA COMPARTIDA (Host <-> Cliente)
        // =========================================================================================

        public IReadOnlyCollection<string> VisitedZoneIds => visitedZonesShared;

        public void ApplyVisitedZoneFromNetwork(string zoneId)
        {
            if (string.IsNullOrEmpty(zoneId)) return;
            visitedZonesShared.Add(zoneId);
        }

        private void VisitedZone(string zoneId, bool notifyNetwork)
        {
            if (string.IsNullOrEmpty(zoneId)) return;
            if (visitedZonesShared.Add(zoneId))
            {
                if (notifyNetwork)
                {
                    OnLocalZoneVisitedForNetwork?.Invoke(zoneId);
                }
            }
        }

        // =========================================================================================
        // CONSTRUCCIÓN LIMPIA Y MINIMALISTA DE LA JERARQUÍA DEL MAPA
        // =========================================================================================

        private void EnsureMapUIHierarchy()
        {
            if (mapContainer != null) return;

            Transform existing = transform.Find("AcademyMap_HUD");
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }

            // Contenedor compacto en la esquina superior derecha
            GameObject containerObj = new GameObject("AcademyMap_HUD");
            containerObj.transform.SetParent(transform, false);
            mapContainer = containerObj.AddComponent<RectTransform>();
            mapContainer.anchorMin = new Vector2(1f, 1f);
            mapContainer.anchorMax = new Vector2(1f, 1f);
            mapContainer.pivot = new Vector2(1f, 1f);
            mapContainer.anchoredPosition = new Vector2(-16f, -16f);
            mapContainer.sizeDelta = compactSize;

            outerBgImage = containerObj.AddComponent<Image>();
            outerBgImage.color = new Color(0.08f, 0.06f, 0.05f, 0.90f);

            // Filete exterior
            GameObject borderObj = new GameObject("MapOuterBorder");
            borderObj.transform.SetParent(mapContainer, false);
            RectTransform borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-1.5f, -1.5f);
            borderRect.offsetMax = new Vector2(1.5f, 1.5f);
            outerBorderImage = borderObj.AddComponent<Image>();
            outerBorderImage.color = new Color(0.84f, 0.70f, 0.40f, 0.72f);
            outerBorderImage.raycastTarget = false;
            borderObj.transform.SetAsFirstSibling();

            // Cabecera compacta (26 px de alto) con nombre de la sala actual y planta
            GameObject headerObj = new GameObject("HeaderBar");
            headerObj.transform.SetParent(mapContainer, false);
            RectTransform headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = Vector2.zero;
            headerRect.sizeDelta = new Vector2(0f, 26f);
            headerBgImage = headerObj.AddComponent<Image>();
            headerBgImage.color = new Color(0.14f, 0.09f, 0.08f, 0.96f);
            headerBgImage.raycastTarget = false;

            headerZoneTMP = CreateMapTMP(headerRect, "CurrentZoneText", new Vector2(6f, 1f), new Vector2(-58f, -2f),
                11.5f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, new Color(0.96f, 0.88f, 0.66f));
            headerZoneTMP.overflowMode = TextOverflowModes.Ellipsis;
            headerZoneTMP.textWrappingMode = TextWrappingModes.NoWrap;

            headerFloorBadgeTMP = CreateMapTMP(headerRect, "FloorBadgeText", new Vector2(6f, 1f), new Vector2(-6f, -2f),
                10f, FontStyles.Bold, TextAlignmentOptions.MidlineRight, new Color(0.82f, 0.76f, 0.64f));

            // Viewport con RectMask2D para recortar limpiamente el zoom local o el plano ampliado
            GameObject viewportObj = new GameObject("MapViewportMask");
            viewportObj.transform.SetParent(mapContainer, false);
            viewportMaskRt = viewportObj.AddComponent<RectTransform>();
            viewportMaskRt.anchorMin = Vector2.zero;
            viewportMaskRt.anchorMax = Vector2.one;
            viewportMaskRt.offsetMin = new Vector2(5f, 5f);
            viewportMaskRt.offsetMax = new Vector2(-5f, -28f);

            viewportBgImage = viewportObj.AddComponent<Image>();
            viewportBgImage.color = new Color(0.06f, 0.05f, 0.05f, 0.65f);
            viewportObj.AddComponent<RectMask2D>();

            // Lienzo interno escalable/desplazable donde viven las salas
            GameObject contentObj = new GameObject("MapCanvasContent");
            contentObj.transform.SetParent(viewportMaskRt, false);
            mapCanvasContentRt = contentObj.AddComponent<RectTransform>();
            mapCanvasContentRt.anchorMin = new Vector2(0.5f, 0.5f);
            mapCanvasContentRt.anchorMax = new Vector2(0.5f, 0.5f);
            mapCanvasContentRt.pivot = new Vector2(0.5f, 0.5f);
            mapCanvasContentRt.sizeDelta = new Vector2(BaseContentPixels, BaseContentPixels);
            mapCanvasContentRt.anchoredPosition = Vector2.zero;

            BuildCleanZoneNodes();
            BuildPlayerAndPartnerMarkers();
            BuildExpandedToolbarControls();
            BuildCompactDynamicGuideCard();
        }

        private void BuildCleanZoneNodes()
        {
            zones.Clear();

            // --- PLANTA PRINCIPAL (MainFloor) ---
            // Sección 1 (Noche I)
            AddZone("z1_3", 1, 1, 0, MapFloorLevel.MainFloor, "1-3. Patio", "1-3. Patio [Verja]", "1-3. Portón y Patio de Entrada", -14f, -36f, 28f, 48f);
            AddZone("z4", 4, 1, 0, MapFloorLevel.MainFloor, "4. Vestíbulo", "4. Vestíbulo", "4. Vestíbulo de Recepción", -11f, 12f, 22f, 20f);
            AddZone("z5", 5, 1, 0, MapFloorLevel.MainFloor, "5. Pasillo", "5. Pasillo [Grietas]", "5. Pasillo Principal", -4f, 32f, 8f, 46f);

            // Sección 2 — Administración Oeste (Noche I)
            AddZone("z6", 6, 2, 0, MapFloorLevel.MainFloor, "6. Recepción", "6. Recepción [220V]", "6. Recepción Antigua y Cuadro Eléctrico", -24f, 12f, 13f, 12f);
            AddZone("z7", 7, 2, 0, MapFloorLevel.MainFloor, "7. Dirección", "7. Dirección", "7. Oficina de Dirección", -38f, 12f, 14f, 14f);
            AddZone("z8", 8, 2, 0, MapFloorLevel.MainFloor, "8. Archivo", "8. Archivo", "8. Archivo de Expedientes", -38f, 26f, 14f, 18f);
            AddZone("z9", 9, 2, 0, MapFloorLevel.MainFloor, "9. Espera", "9. Espera", "9. Sala de Espera", -24f, 24f, 13f, 12f);
            AddZone("z10", 10, 2, 0, MapFloorLevel.MainFloor, "10. Retratos", "10. Retratos", "10. Galería de los Retratos", -20f, 36f, 16f, 14f);

            // Sección 3 — Vestuarios Este (Noche I)
            AddZone("z11", 11, 3, 0, MapFloorLevel.MainFloor, "11. Vestuario", "11. Vestuario", "11. Vestuario Femenino", 11f, 12f, 16f, 20f);
            AddZone("z12", 12, 3, 0, MapFloorLevel.MainFloor, "12. Tocadores", "12. Tocadores", "12. Sala de Tocadores", 11f, 32f, 16f, 14f);
            AddZone("z13", 13, 3, 0, MapFloorLevel.MainFloor, "13. Taquillas", "13. Taquillas [Óxido]", "13. Taquillas Personales", 27f, 12f, 12f, 24f);
            AddZone("z14", 14, 3, 0, MapFloorLevel.MainFloor, "14. Hab. Elena", "14. Hab. Elena", "14. Habitación Privada de Elena Vesper", 12f, 52f, 14f, 14f);
            AddZone("z15", 15, 3, 0, MapFloorLevel.MainFloor, "15. Duchas", "15. Duchas [Humedad]", "15. Lavabos y Duchas", 11f, 0f, 16f, 12f);

            // Sección 4 y 10 — Aulas de Ensayo, Enfermería y Memorial (Noche II)
            AddZone("z16", 16, 4, 1, MapFloorLevel.MainFloor, "16. Aula Ballet", "16. Aula Ballet", "16. Aula Principal de Ballet", -26f, 50f, 22f, 18f);
            AddZone("z17", 17, 4, 1, MapFloorLevel.MainFloor, "17. Espejos", "17. Espejos [Roto]", "17. Aula de Espejos", -38f, 50f, 12f, 18f);
            AddZone("z18", 18, 4, 1, MapFloorLevel.MainFloor, "18. Maniquíes", "18. Maniquíes", "18. Sala de Maniquíes", -26f, 68f, 14f, 14f);
            AddZone("z19", 19, 4, 1, MapFloorLevel.MainFloor, "19. Individual", "19. Individual", "19. Sala de Entrenamiento Individual", -38f, 68f, 12f, 12f);
            AddZone("z20a", 20, 4, 1, MapFloorLevel.MainFloor, "20. Servicio", "20. Servicio", "20. Pasillo de Servicio", -12f, 68f, 8f, 16f);
            AddZone("z43", 43, 10, 1, MapFloorLevel.MainFloor, "43. Enfermería", "43. Enfermería", "43. Enfermería Antigua", -42f, -6f, 17f, 18f);
            AddZone("z44", 44, 10, 1, MapFloorLevel.MainFloor, "44. Memorial", "44. Memorial", "44. Sala Conmemorativa", -38f, -22f, 23.5f, 18f);

            // Sección 11 — Ala Occidental / Internado (Noche III)
            AddZone("z45", 45, 11, 2, MapFloorLevel.MainFloor, "45. Dormitorio", "45. Dormitorio", "45. Dormitorio de Internas", -62f, 54f, 24f, 24f);
            AddZone("z46", 46, 11, 2, MapFloorLevel.MainFloor, "46. Taller", "46. Tintorería", "46. Taller de Costura y Tintorería", -62f, 78f, 24f, 20f);
            AddZone("z47", 47, 11, 2, MapFloorLevel.MainFloor, "47. Comedor", "47. Patronato", "47. Comedor y Salón del Patronato", -62f, 28f, 24f, 26f);
            AddZone("z48", 48, 11, 2, MapFloorLevel.MainFloor, "48. Isadora", "48. Celosía", "48. Estancia de Isadora y Celosía", -62f, 12f, 24f, 16f);

            // Sección 5 y 9 — Archivo Musical y Jardín de Invierno (Noche IV)
            AddZone("z21", 21, 5, 3, MapFloorLevel.MainFloor, "21. Biblioteca", "21. Biblioteca", "21. Biblioteca Musical", 4f, 66f, 22f, 12f);
            AddZone("z22", 22, 5, 3, MapFloorLevel.MainFloor, "22. Piano", "22. Piano", "22. Sala del Piano de Cola", 8f, 78f, 18f, 14f);
            AddZone("z23", 23, 5, 3, MapFloorLevel.MainFloor, "23. Partituras", "23. Partituras", "23. Archivo de Partituras", 26f, 62f, 13f, 16f);
            AddZone("z24", 24, 5, 3, MapFloorLevel.MainFloor, "24. Cabina", "24. Cabina", "24. Cabina de Grabaciones", 30f, 50f, 9f, 12f);
            AddZone("z25", 25, 5, 3, MapFloorLevel.MainFloor, "25. Depósito", "25. Instrumentos", "25. Depósito de Instrumentos", 27f, 36f, 12f, 14f);
            AddZone("z40", 40, 9, 3, MapFloorLevel.MainFloor, "40. Jardín Invierno", "40. Invernadero [Seco]", "40. Jardín de Invierno", 45f, 22f, 44f, 66f);

            // Sección 6 — Camerinos y Bambalinas (Noche VI)
            AddZone("z26", 26, 6, 5, MapFloorLevel.MainFloor, "26. Mara", "26. Mara", "26. Camerino de Mara Bell", 26f, 83f, 7.5f, 9f);
            AddZone("z27", 27, 6, 5, MapFloorLevel.MainFloor, "27. Elena", "27. Elena", "27. Camerino de Elena Vesper", 33.5f, 83f, 7.5f, 9f);
            AddZone("z28", 28, 6, 5, MapFloorLevel.MainFloor, "28. Backstage", "28. Backstage", "28. Pasillo Detrás del Escenario", -12f, 92f, 53f, 6f);
            AddZone("z29", 29, 6, 5, MapFloorLevel.MainFloor, "29. Decorados", "29. Decorados [Caído]", "29. Almacén de Decorados", 10.6f, 98f, 27.4f, 16f);
            AddZone("z30", 30, 6, 5, MapFloorLevel.MainFloor, "30. Poleas", "30. Poleas [Cizalla]", "30. Sala de Mecanismos y Poleas", -28f, 84f, 16f, 20f);

            // Sección 7 y 8 — Gran Teatro y Cámara Oculta (Noches VII y VIII)
            AddZone("z32", 32, 7, 6, MapFloorLevel.MainFloor, "32. Escenario", "32. Escenario", "32. Escenario Principal", -22f, 98f, 28f, 18f);
            AddZone("z33", 33, 7, 6, MapFloorLevel.MainFloor, "33. Foso", "33. Foso", "33. Foso de Orquesta", -20f, 116f, 24f, 6f);
            AddZone("z31", 31, 7, 6, MapFloorLevel.MainFloor, "31. Patio Butacas", "31. Butacas [Polvo]", "31. Patio de Butacas del Gran Teatro", -26f, 122f, 36f, 26f);
            AddZone("z39", 39, 8, 7, MapFloorLevel.MainFloor, "39. Cám. Espejo", "39. Cám. Oculta", "39. Cámara tras el Escenario", 6f, 98.5f, 5.5f, 9.0f);

            // Exteriores Traseros (Noche VIII)
            AddZone("z57", 57, 12, 7, MapFloorLevel.MainFloor, "57. Cochera", "57. Cochera", "57. Cochera de Carruajes", -65f, 98f, 34f, 26f);
            AddZone("z58", 58, 12, 7, MapFloorLevel.MainFloor, "58. Guarda", "58. Pabellón", "58. Pabellón del Guarda", -65f, 125f, 18f, 20f);
            AddZone("z59", 59, 12, 7, MapFloorLevel.MainFloor, "59. Estanque", "59. Estanque [Hielo]", "59. Estanque Exterior y Quiosco", 38f, 98f, 44f, 42f);

            // --- PLANTA ALTA (UpperFloor, Y >= 6.0m) ---
            AddZone("z42", 42, 10, 1, MapFloorLevel.UpperFloor, "42. Azotea", "42. Azotea [Viento]", "42. Terraza y Azotea Mirador (Planta Alta)", -38f, 11.5f, 27f, 32f);
            AddZone("z49", 49, 12, 4, MapFloorLevel.UpperFloor, "49. Mezzanine", "49. Mezzanine", "49. Galería Superior Mezzanine (Planta Alta)", -11f, 9f, 22f, 22f);
            AddZone("z50", 50, 12, 4, MapFloorLevel.UpperFloor, "50. Teoría", "50. Linterna Mágica", "50. Aulas de Teoría Escénica (Planta Alta)", -16f, 31f, 32f, 24f);
            AddZone("z51", 51, 12, 4, MapFloorLevel.UpperFloor, "51. Res. Krol", "51. Res. Krol", "51. Residencia del Maestro Krol (Planta Alta)", -16f, 55f, 32f, 17f);
            AddZone("z52", 52, 12, 4, MapFloorLevel.UpperFloor, "52. Torre Reloj", "52. Reloj [22:17]", "52. Torre del Reloj (Planta Alta)", -12f, 72f, 24f, 16f);
            AddZone("z34", 34, 7, 6, MapFloorLevel.UpperFloor, "34. Cabina Luces", "34. Cabina Luces", "34. Cabina de Iluminación (Planta Alta)", -34f, 118f, 12f, 14f);
            AddZone("z35", 35, 7, 6, MapFloorLevel.UpperFloor, "35. Pasarela", "35. Pasarela", "35. Pasarela Técnica Superior (Planta Alta)", -22f, 98f, 28f, 18f);

            // --- NIVEL SUBTERRÁNEO (Subterranean, Y <= -1.0m) ---
            AddZone("z41", 41, 10, 3, MapFloorLevel.Subterranean, "41. Sótano Mant.", "41. Sótano [Fuga]", "41. Sótano de Mantenimiento (Subterráneo)", 16f, -34f, 38f, 20f);
            AddZone("z53", 53, 12, 4, MapFloorLevel.Subterranean, "53. Calderas", "53. Calderas [Ceniza]", "53. Sala de Calderas (Subterráneo)", -15f, -34f, 31f, 20f);
            AddZone("z54", 54, 12, 4, MapFloorLevel.Subterranean, "54. Lavandería", "54. Lavandería", "54. Lavandería Subterránea (Subterráneo)", -65f, 18f, 27f, 18f);
            AddZone("z56", 56, 12, 4, MapFloorLevel.Subterranean, "56. Cripta", "56. Cripta [Derrumbe]", "56. Cripta de los Fundadores (Subterráneo)", -6f, -14f, 12f, 74f);
            AddZone("z55", 55, 12, 4, MapFloorLevel.Subterranean, "55. Understage", "55. Understage", "55. Foso Bajo el Escenario (Subterráneo)", -22f, 60f, 32f, 48f);
            AddZone("z37", 37, 8, 6, MapFloorLevel.Subterranean, "37. Depósito B-00", "37. Depósito B-00", "37. Depósito Subterráneo B-00", 29f, 114.2f, 13f, 12.3f);
            AddZone("z38", 38, 8, 6, MapFloorLevel.Subterranean, "38. Arch. B-00", "38. Arch. Prohibidos", "38. Archivos Prohibidos B-00 (Subterráneo)", 16f, 126.5f, 26f, 12f);
        }

        private void AddZone(
            string id,
            int zoneNum,
            int sectionIdx,
            int unlockNight,
            MapFloorLevel floorLevel,
            string label1929,
            string labelRuins,
            string fullName,
            float xMin,
            float zMin,
            float width,
            float height)
        {
            GameObject roomObj = new GameObject($"Zone_{id}");
            roomObj.transform.SetParent(mapCanvasContentRt, false);
            RectTransform rt = roomObj.AddComponent<RectTransform>();

            Vector2 minNorm = WorldToNormalized(new Vector2(xMin, zMin));
            Vector2 maxNorm = WorldToNormalized(new Vector2(xMin + width, zMin + height));

            rt.anchorMin = minNorm;
            rt.anchorMax = maxNorm;
            rt.offsetMin = new Vector2(1.2f, 1.2f);
            rt.offsetMax = new Vector2(-1.2f, -1.2f);

            // Borde fino de plano arquitectónico
            Image borderImg = roomObj.AddComponent<Image>();
            borderImg.color = new Color(0.75f, 0.64f, 0.42f, 0.55f);
            borderImg.raycastTarget = false;

            // Relleno interior
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(rt, false);
            RectTransform fillRt = fillObj.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(1f, 1f);
            fillRt.offsetMax = new Vector2(-1f, -1f);

            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.color = new Color(0.18f, 0.14f, 0.12f, 0.85f);
            fillImg.raycastTarget = false;

            // Etiqueta limpia y sin desbordes
            TextMeshProUGUI lbl = CreateMapTMP(fillRt, "ZoneLabel", new Vector2(1.5f, 1.5f), new Vector2(-1.5f, -1.5f),
                7.5f, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.95f, 0.92f, 0.85f, 0.95f));
            lbl.enableAutoSizing = true;
            lbl.fontSizeMin = 5.0f;
            lbl.fontSizeMax = 8.5f;
            lbl.overflowMode = TextOverflowModes.Truncate;

            // Pequeño punto de interés (POI) que solo se enciende cuando la zona fue visitada y tiene objetivo/llave/puzle
            GameObject poiObj = new GameObject("POI_Dot");
            poiObj.transform.SetParent(fillRt, false);
            RectTransform poiRt = poiObj.AddComponent<RectTransform>();
            poiRt.anchorMin = new Vector2(1f, 1f);
            poiRt.anchorMax = new Vector2(1f, 1f);
            poiRt.pivot = new Vector2(1f, 1f);
            poiRt.anchoredPosition = new Vector2(-2f, -2f);
            poiRt.sizeDelta = new Vector2(5.5f, 5.5f);
            Image poiImg = poiObj.AddComponent<Image>();
            poiImg.color = new Color(0.98f, 0.80f, 0.25f, 1f);
            poiImg.raycastTarget = false;
            poiObj.SetActive(false);

            zones.Add(new ZoneMapNode
            {
                id = id,
                zoneNumber = zoneNum,
                sectionIndex = sectionIdx,
                unlockNightIndex = unlockNight,
                floor = floorLevel,
                label1929 = label1929,
                labelRuins = labelRuins,
                fullName = fullName,
                worldXZ = new Rect(xMin, zMin, width, height),
                rt = rt,
                fillImg = fillImg,
                borderImg = borderImg,
                labelTMP = lbl,
                poiDotObj = poiObj,
                poiDotImg = poiImg
            });
        }

        private void BuildPlayerAndPartnerMarkers()
        {
            // Marcador de último punto conocido del compañero
            GameObject partnerObj = new GameObject("PartnerLastKnownMarker");
            partnerObj.transform.SetParent(mapCanvasContentRt, false);
            partnerDot = partnerObj.AddComponent<RectTransform>();
            partnerDot.sizeDelta = new Vector2(8.5f, 8.5f);
            partnerDotImg = partnerObj.AddComponent<Image>();
            partnerDotImg.color = new Color(0.95f, 0.78f, 0.35f, 0.92f);
            partnerDotImg.raycastTarget = false;
            partnerObj.SetActive(false);

            // Marcador del jugador local
            GameObject pObj = new GameObject("PlayerMapMarker");
            pObj.transform.SetParent(mapCanvasContentRt, false);
            playerDot = pObj.AddComponent<RectTransform>();
            playerDot.sizeDelta = new Vector2(9.5f, 9.5f);
            Image pImg = pObj.AddComponent<Image>();
            pImg.color = new Color(0.22f, 0.96f, 1.0f, 1f);
            pImg.raycastTarget = false;

            GameObject dirObj = new GameObject("PlayerLookPointer");
            dirObj.transform.SetParent(playerDot, false);
            playerDirectionLine = dirObj.AddComponent<RectTransform>();
            playerDirectionLine.pivot = new Vector2(0.5f, 0f);
            playerDirectionLine.anchoredPosition = Vector2.zero;
            playerDirectionLine.sizeDelta = new Vector2(2.4f, 9.5f);
            Image dirImg = dirObj.AddComponent<Image>();
            dirImg.color = new Color(1f, 1f, 1f, 0.95f);
            dirImg.raycastTarget = false;
        }

        private void BuildExpandedToolbarControls()
        {
            // Barra discreta en la parte inferior del mapa que SOLO aparece cuando se amplía con [M]
            controlsToolbarObj = new GameObject("ExpandedMapToolbar");
            controlsToolbarObj.transform.SetParent(mapContainer, false);
            RectTransform tbRt = controlsToolbarObj.AddComponent<RectTransform>();
            tbRt.anchorMin = new Vector2(0f, 0f);
            tbRt.anchorMax = new Vector2(1f, 0f);
            tbRt.pivot = new Vector2(0.5f, 0f);
            tbRt.anchoredPosition = new Vector2(0f, 5f);
            tbRt.sizeDelta = new Vector2(-10f, 26f);

            Image tbBg = controlsToolbarObj.AddComponent<Image>();
            tbBg.color = new Color(0.06f, 0.04f, 0.05f, 0.94f);

            btnFloorFilterTMP = CreateMiniToolbarButton(tbRt, "BtnFloor", new Vector2(0.01f, 0.1f), new Vector2(0.33f, 0.9f),
                "Planta: Auto", CycleViewedFloor);

            btnSectionFilterTMP = CreateMiniToolbarButton(tbRt, "BtnSection", new Vector2(0.35f, 0.1f), new Vector2(0.67f, 0.9f),
                "Filtro: Visitadas", ToggleCurrentNightFilter);

            btnPartnerToggleTMP = CreateMiniToolbarButton(tbRt, "BtnPartner", new Vector2(0.69f, 0.1f), new Vector2(0.99f, 0.9f),
                showPartnerLastKnownPos ? "Compañero: ON" : "Compañero: OFF", TogglePartnerMarkerSetting);

            controlsToolbarObj.SetActive(false);
        }

        private TextMeshProUGUI CreateMiniToolbarButton(
            RectTransform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            string label,
            UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = new Color(0.20f, 0.12f, 0.14f, 0.95f);

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;
            if (onClick != null) btn.onClick.AddListener(onClick);

            TextMeshProUGUI tmp = CreateMapTMP(rt, "Lbl", new Vector2(2f, 1f), new Vector2(-2f, -1f),
                10f, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.95f, 0.88f, 0.72f));
            tmp.text = label;
            return tmp;
        }

        private void BuildCompactDynamicGuideCard()
        {
            guidePanelObj = new GameObject("DynamicNightGuide_Panel");
            guidePanelObj.transform.SetParent(mapContainer, false);
            RectTransform grt = guidePanelObj.AddComponent<RectTransform>();
            grt.anchorMin = new Vector2(0f, 0f);
            grt.anchorMax = new Vector2(1f, 0f);
            grt.pivot = new Vector2(0.5f, 1f);
            grt.anchoredPosition = new Vector2(0f, -5f);
            grt.sizeDelta = new Vector2(0f, 78f);

            guideBgImage = guidePanelObj.AddComponent<Image>();
            guideBgImage.color = new Color(0.07f, 0.05f, 0.06f, 0.92f);
            guideBgImage.raycastTarget = false;

            GameObject borderObj = new GameObject("GuideBorder");
            borderObj.transform.SetParent(grt, false);
            RectTransform brt = borderObj.AddComponent<RectTransform>();
            brt.anchorMin = Vector2.zero;
            brt.anchorMax = Vector2.one;
            brt.offsetMin = new Vector2(-1f, -1f);
            brt.offsetMax = new Vector2(1f, 1f);
            guideBorderImage = borderObj.AddComponent<Image>();
            guideBorderImage.color = new Color(0.82f, 0.68f, 0.38f, 0.48f);
            guideBorderImage.raycastTarget = false;
            borderObj.transform.SetAsFirstSibling();

            dynamicGuideTMP = CreateMapTMP(grt, "DynamicGuideText", new Vector2(8f, 4f), new Vector2(-8f, -4f),
                10.5f, FontStyles.Normal, TextAlignmentOptions.TopLeft, new Color(0.94f, 0.91f, 0.85f));
            dynamicGuideTMP.lineSpacing = 3f;
        }

        private static TextMeshProUGUI CreateMapTMP(
            RectTransform parent,
            string name,
            Vector2 offsetMin,
            Vector2 offsetMax,
            float fontSize,
            FontStyles style,
            TextAlignmentOptions align,
            Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.color = color;
            tmp.raycastTarget = false;
            return tmp;
        }

        // =========================================================================================
        // BUCLE DE ACTUALIZACIÓN: PLANTA ACTIVA, NIEBLA DE GUERRA, ZOOM Y GUÍA DINÁMICA
        // =========================================================================================

        private void Update()
        {
            if (playerTransform == null)
            {
                FindPlayerReferences();
                if (playerTransform == null) return;
            }

            RefreshDoorCacheIfNeeded();
            HandleMapHotkeysAndZoomPan();
            ApplyEraBlueprintTheme();
            UpdatePlayerFloorLocationAndFogOfWar();
            UpdateDynamicNightGuideText();
        }

        private void RefreshDoorCacheIfNeeded()
        {
            if (Time.unscaledTime < nextDoorCacheRefreshTime && sceneDoorsById.Count > 0) return;
            nextDoorCacheRefreshTime = Time.unscaledTime + 3.0f;

            sceneDoorsById.Clear();
            WorldDoor[] allDoors = FindObjectsByType<WorldDoor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < allDoors.Length; i++)
            {
                if (allDoors[i] != null && !string.IsNullOrEmpty(allDoors[i].Id))
                {
                    sceneDoorsById[allDoors[i].Id] = allDoors[i];
                }
            }
        }

        private void HandleMapHotkeysAndZoomPan()
        {
            // No procesar atajos si el jugador está escribiendo una nota en el Diario Compartido
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null &&
                EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null)
            {
                return;
            }

            bool toggleExpand = false;
            bool toggleGuide = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.mKey.wasPressedThisFrame) toggleExpand = true;
                if (Keyboard.current.hKey.wasPressedThisFrame) toggleGuide = true;
            }
#else
            if (Input.GetKeyDown(KeyCode.M)) toggleExpand = true;
            if (Input.GetKeyDown(KeyCode.H)) toggleGuide = true;
#endif

            if (toggleExpand && mapContainer != null)
            {
                isExpanded = !isExpanded;
                mapContainer.sizeDelta = isExpanded ? expandedSize : compactSize;
                if (controlsToolbarObj != null) controlsToolbarObj.SetActive(isExpanded);
                if (viewportMaskRt != null)
                {
                    viewportMaskRt.offsetMin = new Vector2(5f, isExpanded ? 34f : 5f);
                }
                if (!isExpanded)
                {
                    useManualFloorOverride = false;
                    expandedPanOffset = Vector2.zero;
                }
            }

            if (toggleGuide && guidePanelObj != null)
            {
                isGuideVisible = !isGuideVisible;
                guidePanelObj.SetActive(isGuideVisible);
            }

            // En Modo Ampliado ([M]), permitir zoom con la rueda del ratón y arrastrar el plano si el cursor está sobre el mapa
            if (isExpanded && viewportMaskRt != null)
            {
                Vector2 mousePos = GetMouseScreenPosition();
                if (RectTransformUtility.RectangleContainsScreenPoint(viewportMaskRt, mousePos, null))
                {
                    float scroll = GetMouseScrollDelta();
                    if (Mathf.Abs(scroll) > 0.01f)
                    {
                        expandedZoom = Mathf.Clamp(expandedZoom + scroll * 0.22f, 0.90f, 3.20f);
                    }

                    if (IsMouseLeftButtonDown())
                    {
                        isDraggingMap = true;
                        lastMouseDragPos = mousePos;
                    }
                }

                if (isDraggingMap)
                {
                    if (IsMouseLeftButtonHeld())
                    {
                        Vector2 delta = mousePos - lastMouseDragPos;
                        lastMouseDragPos = mousePos;
                        expandedPanOffset += delta;
                        float maxPan = 140f * expandedZoom;
                        expandedPanOffset.x = Mathf.Clamp(expandedPanOffset.x, -maxPan, maxPan);
                        expandedPanOffset.y = Mathf.Clamp(expandedPanOffset.y, -maxPan, maxPan);
                    }
                    else
                    {
                        isDraggingMap = false;
                    }
                }
            }
        }

        private void CycleViewedFloor()
        {
            if (!useManualFloorOverride)
            {
                useManualFloorOverride = true;
                manualViewedFloor = activePlayerFloor;
            }
            manualViewedFloor = (MapFloorLevel)(((int)manualViewedFloor + 1) % 3);
            if (btnFloorFilterTMP != null)
            {
                btnFloorFilterTMP.text = $"Planta: {GetFloorShortName(manualViewedFloor)}";
            }
        }

        private void ToggleCurrentNightFilter()
        {
            filterCurrentNightZonesOnly = !filterCurrentNightZonesOnly;
            if (btnSectionFilterTMP != null)
            {
                btnSectionFilterTMP.text = filterCurrentNightZonesOnly ? "Filtro: Noche Actual" : "Filtro: Visitadas";
            }
        }

        private void TogglePartnerMarkerSetting()
        {
            showPartnerLastKnownPos = !showPartnerLastKnownPos;
            PlayerPrefs.SetInt("UltimoActo_Map_ShowPartner", showPartnerLastKnownPos ? 1 : 0);
            PlayerPrefs.Save();
            if (btnPartnerToggleTMP != null)
            {
                btnPartnerToggleTMP.text = showPartnerLastKnownPos ? "Compañero: ON" : "Compañero: OFF";
            }
        }

        // =========================================================================================
        // ESTÉTICA POR ÉPOCA (1929 Pergamino Dorado vs. Ruinas Pizarra Deteriorada)
        // =========================================================================================

        private void ApplyEraBlueprintTheme()
        {
            EraVisual currentEra = EraVisualManager.Instance != null ? EraVisualManager.Instance.CurrentEra : EraVisual.Glory1929;
            bool is1929 = currentEra == EraVisual.Glory1929;

            if (outerBgImage != null)
            {
                outerBgImage.color = is1929
                    ? new Color(0.11f, 0.08f, 0.06f, 0.92f)  // Pergamino oscuro de ópera (1929)
                    : new Color(0.06f, 0.08f, 0.10f, 0.92f); // Pizarra fría deteriorada (Ruinas)
            }
            if (outerBorderImage != null)
            {
                outerBorderImage.color = is1929
                    ? new Color(0.86f, 0.71f, 0.40f, 0.78f)  // Pan de oro (1929)
                    : new Color(0.46f, 0.58f, 0.68f, 0.72f); // Acero oxidado / lunar (Ruinas)
            }
            if (headerBgImage != null)
            {
                headerBgImage.color = is1929
                    ? new Color(0.18f, 0.11f, 0.09f, 0.96f)
                    : new Color(0.09f, 0.12f, 0.15f, 0.96f);
            }
            if (viewportBgImage != null)
            {
                viewportBgImage.color = is1929
                    ? new Color(0.14f, 0.10f, 0.08f, 0.75f)
                    : new Color(0.07f, 0.09f, 0.11f, 0.78f);
            }
            if (guideBgImage != null)
            {
                guideBgImage.color = is1929
                    ? new Color(0.10f, 0.07f, 0.06f, 0.93f)
                    : new Color(0.06f, 0.08f, 0.10f, 0.93f);
            }
            if (guideBorderImage != null)
            {
                guideBorderImage.color = is1929
                    ? new Color(0.84f, 0.68f, 0.38f, 0.52f)
                    : new Color(0.44f, 0.56f, 0.66f, 0.52f);
            }
        }

        // =========================================================================================
        // SEPARACIÓN POR PLANTAS, NIEBLA DE GUERRA Y RADAR LOCAL CENTRADO
        // =========================================================================================

        private void UpdatePlayerFloorLocationAndFogOfWar()
        {
            Vector3 pPos = playerTransform.position;
            Vector2 pt = new Vector2(pPos.x, pPos.z);

            if (pPos.y >= 6.0f) activePlayerFloor = MapFloorLevel.UpperFloor;
            else if (pPos.y <= -1.0f) activePlayerFloor = MapFloorLevel.Subterranean;
            else activePlayerFloor = MapFloorLevel.MainFloor;

            MapFloorLevel displayedFloor = (isExpanded && useManualFloorOverride)
                ? manualViewedFloor
                : activePlayerFloor;

            Vector2 playerNorm = WorldToNormalized(pt);
            if (playerDot != null)
            {
                playerDot.anchorMin = playerNorm;
                playerDot.anchorMax = playerNorm;
                playerDot.anchoredPosition = Vector2.zero;
                playerDot.gameObject.SetActive(displayedFloor == activePlayerFloor);
            }

            if (playerDirectionLine != null && cameraTransform != null)
            {
                float yaw = cameraTransform.eulerAngles.y;
                playerDirectionLine.localRotation = Quaternion.Euler(0f, 0f, -yaw);
            }

            // Centrado automático en el jugador cuando está en Modo Compacto (radar limpio de cercanía)
            if (mapCanvasContentRt != null)
            {
                float activeZoom = isExpanded ? expandedZoom : compactLocalZoom;
                mapCanvasContentRt.localScale = new Vector3(activeZoom, activeZoom, 1f);

                if (!isExpanded)
                {
                    Vector2 centeredOffset = new Vector2(
                        (0.5f - playerNorm.x) * BaseContentPixels * activeZoom,
                        (0.5f - playerNorm.y) * BaseContentPixels * activeZoom);
                    mapCanvasContentRt.anchoredPosition = centeredOffset;
                }
                else
                {
                    mapCanvasContentRt.anchoredPosition = expandedPanOffset;
                }
            }

            // Último punto conocido del compañero (actualizado cada 3.5 s si está habilitado en ajustes)
            UpdatePartnerLastKnownMarker(displayedFloor);

            int currentNight = NightManager.Instance != null ? NightManager.Instance.CurrentNightIndex : 0;
            EraVisual currentEra = EraVisualManager.Instance != null ? EraVisualManager.Instance.CurrentEra : EraVisual.Glory1929;
            bool is1929 = currentEra == EraVisual.Glory1929;

            string activeRoomName = "Pasillo de la Academia";

            for (int i = 0; i < zones.Count; i++)
            {
                ZoneMapNode z = zones[i];
                bool isOnDisplayedFloor = z.floor == displayedFloor;
                bool insideThisZone = (z.floor == activePlayerFloor) && z.worldXZ.Contains(pt);

                if (insideThisZone)
                {
                    currentRoomId = z.id;
                    activeRoomName = $"{z.zoneNumber}. {(is1929 ? z.label1929 : z.labelRuins).Replace($"{z.zoneNumber}. ", "")}";
                    VisitedZone(z.id, true);
                }

                // Solo dibujamos las salas de la planta mostrada (nunca superponemos sótanos ni planta alta sobre planta baja)
                if (!isOnDisplayedFloor)
                {
                    if (z.rt.gameObject.activeSelf) z.rt.gameObject.SetActive(false);
                    continue;
                }

                bool isVisited = visitedZonesShared.Contains(z.id);
                bool isUnlockedThisNight = z.unlockNightIndex <= currentNight;
                bool isTargetOfCurrentNight = z.unlockNightIndex == currentNight;

                // En modo compacto o con filtro de noche, ocultar zonas de noches futuras muy lejanas que no han sido descubiertas
                bool shouldShowNode = isVisited || isUnlockedThisNight;
                if (filterCurrentNightZonesOnly && !isTargetOfCurrentNight && !insideThisZone)
                {
                    shouldShowNode = false;
                }

                if (z.rt.gameObject.activeSelf != shouldShowNode)
                {
                    z.rt.gameObject.SetActive(shouldShowNode);
                }
                if (!shouldShowNode) continue;

                // Estética de Niebla de Guerra:
                // - Si fue visitada: muestra color arquitectónico de la época y su nombre.
                // - Si aún NO fue visitada: se muestra como silueta en penumbra ("?") sin desvelar contenido.
                if (insideThisZone)
                {
                    z.fillImg.color = is1929
                        ? new Color(0.56f, 0.42f, 0.22f, 0.96f)
                        : new Color(0.28f, 0.46f, 0.56f, 0.96f);
                    z.borderImg.color = is1929
                        ? new Color(0.98f, 0.86f, 0.52f, 1f)
                        : new Color(0.68f, 0.88f, 0.98f, 1f);
                    z.labelTMP.text = is1929 ? z.label1929 : z.labelRuins;
                    z.labelTMP.color = new Color(1f, 0.98f, 0.92f, 1f);
                }
                else if (isVisited)
                {
                    z.fillImg.color = is1929
                        ? (isTargetOfCurrentNight ? new Color(0.32f, 0.23f, 0.16f, 0.88f) : new Color(0.22f, 0.17f, 0.14f, 0.82f))
                        : (isTargetOfCurrentNight ? new Color(0.18f, 0.26f, 0.28f, 0.88f) : new Color(0.13f, 0.17f, 0.20f, 0.82f));
                    z.borderImg.color = is1929
                        ? new Color(0.78f, 0.64f, 0.38f, 0.68f)
                        : new Color(0.45f, 0.58f, 0.66f, 0.68f);
                    z.labelTMP.text = is1929 ? z.label1929 : z.labelRuins;
                    z.labelTMP.color = new Color(0.92f, 0.88f, 0.80f, 0.92f);
                }
                else
                {
                    // Silueta de Niebla de Guerra
                    z.fillImg.color = new Color(0.08f, 0.08f, 0.10f, 0.55f);
                    z.borderImg.color = new Color(0.42f, 0.40f, 0.38f, 0.32f);
                    z.labelTMP.text = $"{z.zoneNumber} ?";
                    z.labelTMP.color = new Color(0.62f, 0.60f, 0.58f, 0.55f);
                }

                // Punto de interés (solo visible si la sala ya fue descubierta)
                bool showPoi = isVisited && HasDiscoveredInterestInZone(z, currentNight);
                if (z.poiDotObj != null && z.poiDotObj.activeSelf != showPoi)
                {
                    z.poiDotObj.SetActive(showPoi);
                }
            }

            if (headerZoneTMP != null)
            {
                headerZoneTMP.text = activeRoomName;
            }
            if (headerFloorBadgeTMP != null)
            {
                headerFloorBadgeTMP.text = GetFloorShortName(displayedFloor);
            }
        }

        private bool HasDiscoveredInterestInZone(ZoneMapNode z, int currentNight)
        {
            bool hasArchiveKey = HasInventoryKey("key_archives");
            bool hasElenaKey = HasInventoryKey("key_elena_room");

            if (z.id == "z7" && !hasArchiveKey) return true;
            if (z.id == "z13" && !hasElenaKey) return true;
            if (z.id == "z6" && AcademyFuseBoxController.Instance != null && !AcademyFuseBoxController.Instance.IsPowerRestored) return true;
            if (z.id == "z10" && PortraitGalleryPuzzleController.Instance != null && !PortraitGalleryPuzzleController.Instance.IsSolved) return true;

            return z.unlockNightIndex == currentNight;
        }

        private bool HasInventoryKey(string keyId)
        {
            if (playerInventory != null && playerInventory.HasKey(keyId)) return true;
            if (SimpleInventory.GlobalKeys != null)
            {
                foreach (string k in SimpleInventory.GlobalKeys)
                {
                    if (string.Equals(k, keyId, StringComparison.OrdinalIgnoreCase)) return true;
                }
            }
            return false;
        }

        private void UpdatePartnerLastKnownMarker(MapFloorLevel displayedFloor)
        {
            if (partnerDot == null) return;

            UltimoActo.Network.NetworkCoopPlayer partner = UltimoActo.Network.NetworkCoopPlayer.RemotePlayer;
            if (!showPartnerLastKnownPos || partner == null || partner == UltimoActo.Network.NetworkCoopPlayer.LocalPlayer)
            {
                if (partnerDot.gameObject.activeSelf) partnerDot.gameObject.SetActive(false);
                return;
            }

            // Actualizar "último punto conocido" cada 3.0 segundos para no restar misterio
            if (!hasPartnerSample || Time.unscaledTime - partnerLastSampleTime >= 3.0f)
            {
                partnerLastSampleTime = Time.unscaledTime;
                partnerLastKnownWorldPos = partner.transform.position;
                hasPartnerSample = true;
            }

            MapFloorLevel partnerFloor = MapFloorLevel.MainFloor;
            if (partnerLastKnownWorldPos.y >= 6.0f) partnerFloor = MapFloorLevel.UpperFloor;
            else if (partnerLastKnownWorldPos.y <= -1.0f) partnerFloor = MapFloorLevel.Subterranean;

            bool showOnThisFloor = partnerFloor == displayedFloor;
            if (partnerDot.gameObject.activeSelf != showOnThisFloor)
            {
                partnerDot.gameObject.SetActive(showOnThisFloor);
            }

            if (showOnThisFloor)
            {
                Vector2 rNorm = WorldToNormalized(new Vector2(partnerLastKnownWorldPos.x, partnerLastKnownWorldPos.z));
                partnerDot.anchorMin = rNorm;
                partnerDot.anchorMax = rNorm;
                partnerDot.anchoredPosition = Vector2.zero;
            }
        }

        // =========================================================================================
        // GUÍA DINÁMICA GENERADA DESDE LOS DATOS REALES DE LA PARTIDA (NO TEXTO FIJO)
        // =========================================================================================

        private void UpdateDynamicNightGuideText()
        {
            if (dynamicGuideTMP == null || !isGuideVisible) return;

            NightManager nm = NightManager.Instance;
            int nightIdx = nm != null ? nm.CurrentNightIndex : 0;
            string nightRoman = (nm != null && nm.Current != null) ? nm.Current.roman : "I";
            string nightTitle = (nm != null && nm.Current != null) ? nm.Current.title : "LA LLEGADA";
            int evCount = nm != null ? nm.EvidenceThisNight : 0;
            int evQuota = (nm != null && nm.Current != null) ? nm.Current.evidenceQuota : 6;

            string activeObjectiveLine = BuildDynamicObjectiveForNight(nightIdx);
            string keysAndDoorsLine = BuildDynamicKeysAndAccessState(nightIdx);

            dynamicGuideTMP.text =
                $"<b><color=#F3D692>Noche {nightRoman} ({nightTitle}):</color></b> Evidencias <b>{evCount}/{evQuota}</b> · Visitadas: <b>{visitedZonesShared.Count}/48</b>\n" +
                $"• <b>Objetivo activo:</b> {activeObjectiveLine}\n" +
                $"• <b>Accesos:</b> {keysAndDoorsLine}\n" +
                $"<size=9.5><color=#B8B09A>[M] {(isExpanded ? "Modo Radar Compacto" : "Abrir Plano / Plantas / Zoom")}  |  [H] Ocultar Guía</color></size>";
        }

        private string BuildDynamicObjectiveForNight(int nightIdx)
        {
            switch (nightIdx)
            {
                case 0: // Noche I
                    bool powerOn = AcademyFuseBoxController.Instance != null && AcademyFuseBoxController.Instance.IsPowerRestored;
                    int fuses = AcademyFuseBoxController.Instance != null ? AcademyFuseBoxController.Instance.InstalledFusesCount : 0;
                    bool portraitsOk = PortraitGalleryPuzzleController.Instance != null && PortraitGalleryPuzzleController.Instance.IsSolved;
                    if (!powerOn) return $"Restaurar Cuadro 220V en 6. Recepción ({fuses}/3 fusibles)";
                    if (!portraitsOk) return "Alinear los 4 retratos en 10. Galería e inspeccionar 14. Hab. Elena";
                    return "Completar cuota de evidencias en Vestíbulo, Archivo y Habitación de Elena";

                case 1: // Noche II
                    return "Explorar 16-18. Aulas de Ensayo (Espejo n.º 3 y Maniquíes) y 43-44. Enfermería/Memorial";

                case 2: // Noche III
                    return "Cruzar pistas entre 48. Celosía de Isadora (1929) y 46. Taller de Costura (Ruinas)";

                case 3: // Noche IV
                    return "Sincronizar 24. Cabina de Grabación / 41. Sótano y restaurar partitura en 21-23. Archivo";

                case 4: // Noche V
                    return "Subir a 49-52. Planta Alta (Torre del Reloj) y despejar 56. Cripta Subterránea";

                case 5: // Noche VI
                    return "Inspeccionar 26-27. Camerinos y accionar torno del Contrapeso III en 30. Sala de Poleas";

                case 6: // Noche VII
                    return "Completar guion de escena en 31-35. Gran Teatro y abrir 37-38. Bóveda B-00";

                default: // Noche VIII
                    return "Unir las dos épocas en 39. Cámara tras el Escenario y 59. Estanque hacia las 06:00 AM";
            }
        }

        private string BuildDynamicKeysAndAccessState(int nightIdx)
        {
            bool hasArchiveKey = HasInventoryKey("key_archives");
            bool hasElenaKey = HasInventoryKey("key_elena_room");

            if (nightIdx == 0)
            {
                string k1 = hasArchiveKey ? "<color=#6EE88E>Llave Archivo ✓</color>" : "<color=#F0C868>Llave Archivo (Pendiente)</color>";
                string k2 = hasElenaKey ? "<color=#6EE88E>Llave Elena ✓</color>" : "<color=#F0C868>Llave Elena (Pendiente)</color>";
                return $"{k1} · {k2}";
            }

            // En Noches II a VIII informa dinámicamente qué alas están desbloqueadas en la noche actual
            string[] nightWings =
            {
                "Ala Principal y Vestuarios",
                "Aulas de Ensayo (16-20) + Enfermería (43) y Memorial (44) abiertas",
                "Ala Occidental de Internado (45-48) desbloqueada",
                "Archivo Musical (21-25), Invernadero (40) y Sótano (41) abiertos",
                "Planta Alta (49-52) y Red Subterránea (53-56) abiertas",
                "Camerinos de Solistas y Sala de Poleas (26-30) abiertos",
                "Gran Teatro (31-35) y Bóveda Prohibida B-00 (36-38) abiertos",
                "Cámara Oculta (39) y Exteriores del Lago (57-59) abiertos"
            };

            int clamped = Mathf.Clamp(nightIdx, 0, nightWings.Length - 1);
            return $"<color=#6EE88E>{nightWings[clamped]}</color>";
        }

        private static string GetFloorShortName(MapFloorLevel floor)
        {
            switch (floor)
            {
                case MapFloorLevel.UpperFloor: return "PLANTA ALTA";
                case MapFloorLevel.Subterranean: return "SUBTERRÁNEO";
                default: return "PLANTA BAJA";
            }
        }

        private Vector2 WorldToNormalized(Vector2 worldXZ)
        {
            float nx = Mathf.InverseLerp(WorldMinX, WorldMaxX, worldXZ.x);
            float nz = Mathf.InverseLerp(WorldMinZ, WorldMaxZ, worldXZ.y);
            return new Vector2(nx, nz);
        }

        private static Vector2 GetMouseScreenPosition()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null) return Mouse.current.position.ReadValue();
            return Vector2.zero;
#else
            return Input.mousePosition;
#endif
        }

        private static float GetMouseScrollDelta()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null) return Mouse.current.scroll.ReadValue().y * 0.01f;
            return 0f;
#else
            return Input.mouseScrollDelta.y;
#endif
        }

        private static bool IsMouseLeftButtonDown()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        private static bool IsMouseLeftButtonHeld()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.isPressed;
#else
            return Input.GetMouseButton(0);
#endif
        }
    }
}
