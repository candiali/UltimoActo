using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UltimoActo.Core;

namespace UltimoActo.UI
{
    /// <summary>
    /// Mapa arquitectónico interactivo en tiempo real en la esquina superior de la pantalla.
    /// Muestra las 20 zonas de la Academia Vesper (Secciones 1 a 4), las conexiones de puertas,
    /// la ubicación exacta del jugador, en qué cuarto se encuentra y dónde están las llaves.
    /// Permite ampliar el mapa con [M] o mostrar/ocultar la guía con [H].
    /// </summary>
    public class AcademyMapUI : MonoBehaviour
    {
        [System.Serializable]
        private struct RoomZoneDef
        {
            public string id;
            public string shortLabel;
            public string fullName;
            public Rect worldXZ; // xMin, zMin, width, height
            public Color baseColor;
            public Image uiBox;
        }

        private RectTransform mapContainer;
        private RectTransform mapBoard;
        private RectTransform playerDot;
        private RectTransform playerDirectionLine;
        private RectTransform keyArchivesMarker;
        private RectTransform keyElenaMarker;
        private Image keyArchivesBg;
        private Image keyElenaBg;
        private TextMeshProUGUI currentZoneTMP;
        private TextMeshProUGUI keyGuideTMP;
        private GameObject guidePanelObj;

        private Transform playerTransform;
        private Transform cameraTransform;
        private SimpleInventory playerInventory;

        private List<RoomZoneDef> rooms = new List<RoomZoneDef>();
        private bool isExpanded = false;
        private bool isGuideVisible = true;

        // Límites del mundo representados en el plano (X: -42 a +42, Z: -38 a +95)
        private const float WorldMinX = -42f;
        private const float WorldMaxX = 42f;
        private const float WorldMinZ = -38f;
        private const float WorldMaxZ = 95f;

        private readonly Vector2 compactSize = new Vector2(390f, 360f);
        private readonly Vector2 expandedSize = new Vector2(720f, 660f);

        private void Start()
        {
            FindPlayerReferences();
            EnsureMapUIHierarchy();
        }

        private void FindPlayerReferences()
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null)
            {
                playerTransform = pObj.transform;
                playerInventory = pObj.GetComponent<SimpleInventory>();
            }
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void EnsureMapUIHierarchy()
        {
            if (mapContainer != null) return;

            Transform existing = transform.Find("AcademyMap_HUD");
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }

            // Contenedor principal arriba a la derecha
            GameObject containerObj = new GameObject("AcademyMap_HUD");
            containerObj.transform.SetParent(transform, false);
            mapContainer = containerObj.AddComponent<RectTransform>();
            mapContainer.anchorMin = new Vector2(1f, 1f);
            mapContainer.anchorMax = new Vector2(1f, 1f);
            mapContainer.pivot = new Vector2(1f, 1f);
            mapContainer.anchoredPosition = new Vector2(-18f, -18f);
            mapContainer.sizeDelta = compactSize;

            Image outerBg = containerObj.AddComponent<Image>();
            outerBg.color = new Color(0.05f, 0.05f, 0.07f, 0.90f);
            outerBg.raycastTarget = false;

            // Borde dorado elegante
            GameObject borderObj = new GameObject("GoldBorder");
            borderObj.transform.SetParent(mapContainer, false);
            RectTransform borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-2f, -2f);
            borderRect.offsetMax = new Vector2(2f, 2f);
            Image borderImg = borderObj.AddComponent<Image>();
            borderImg.color = new Color(0.82f, 0.68f, 0.38f, 0.65f);
            borderImg.raycastTarget = false;
            borderObj.transform.SetAsFirstSibling();

            // Cabecera con el cuarto actual
            GameObject headerObj = new GameObject("HeaderBar");
            headerObj.transform.SetParent(mapContainer, false);
            RectTransform headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = Vector2.zero;
            headerRect.sizeDelta = new Vector2(0f, 34f);
            Image headerBg = headerObj.AddComponent<Image>();
            headerBg.color = new Color(0.14f, 0.11f, 0.14f, 0.96f);
            headerBg.raycastTarget = false;

            GameObject zoneTextObj = new GameObject("CurrentZoneText");
            zoneTextObj.transform.SetParent(headerRect, false);
            currentZoneTMP = zoneTextObj.AddComponent<TextMeshProUGUI>();
            currentZoneTMP.fontSize = 14f;
            currentZoneTMP.fontStyle = FontStyles.Bold;
            currentZoneTMP.alignment = TextAlignmentOptions.Center;
            currentZoneTMP.color = new Color(0.96f, 0.88f, 0.62f);
            currentZoneTMP.text = "PLANO DE LA ACADEMIA";
            currentZoneTMP.raycastTarget = false;
            RectTransform ztRect = zoneTextObj.GetComponent<RectTransform>();
            ztRect.anchorMin = Vector2.zero;
            ztRect.anchorMax = Vector2.one;
            ztRect.offsetMin = new Vector2(6f, 2f);
            ztRect.offsetMax = new Vector2(-6f, -2f);

            // Tablero donde se dibujan los cuartos proporcionalmente
            GameObject boardObj = new GameObject("MapBoard");
            boardObj.transform.SetParent(mapContainer, false);
            mapBoard = boardObj.AddComponent<RectTransform>();
            mapBoard.anchorMin = Vector2.zero;
            mapBoard.anchorMax = Vector2.one;
            mapBoard.offsetMin = new Vector2(8f, 8f);
            mapBoard.offsetMax = new Vector2(-8f, -38f);

            BuildRoomsOnBoard();
            BuildDoorwayConnectorsOnBoard();
            BuildSpecialMarkersOnBoard();
            BuildKeyAndRoomGuidePanel();
        }

        private void BuildRoomsOnBoard()
        {
            rooms.Clear();

            Color colSec1 = new Color(0.22f, 0.28f, 0.38f, 0.85f); // Exterior, Vestíbulo y Pasillo
            Color colSec2 = new Color(0.16f, 0.34f, 0.28f, 0.85f); // Administración (Oeste)
            Color colSec3 = new Color(0.42f, 0.18f, 0.24f, 0.85f); // Vestuarios y Alumnas (Este)
            Color colSec4 = new Color(0.38f, 0.30f, 0.16f, 0.85f); // Aulas de Ensayo (Noroeste)
            Color colSec5 = new Color(0.32f, 0.22f, 0.42f, 0.85f); // Archivo Musical (Noreste)

            // Sección 1
            AddRoom("z1_3", "1-3. Patio Entrada", "1-3. Portón y Patio de Entrada (Sección 1)", -14f, -36f, 28f, 48f, colSec1);
            AddRoom("z4", "4. Vestíbulo", "4. Vestíbulo de Recepción (Sección 1)", -11f, 12f, 22f, 20f, colSec1);
            AddRoom("z5", "5. Pasillo Central", "5. Pasillo Principal (Sección 1)", -4f, 32f, 8f, 46f, colSec1);

            // Sección 2 (Oeste)
            AddRoom("z6", "6. Recepción", "6. Recepción Antigua (Sección 2)", -24f, 12f, 13f, 12f, colSec2);
            AddRoom("z7", "7. Dirección [LLAVE]", "7. Oficina de Dirección — Aquí está la Llave del Archivo", -38f, 12f, 14f, 14f, colSec2);
            AddRoom("z8", "8. Archivo [CERRADO]", "8. Archivo de Expedientes (Sección 2)", -38f, 26f, 14f, 18f, colSec2);
            AddRoom("z9", "9. Sala Espera", "9. Sala de Espera (Sección 2)", -24f, 24f, 13f, 12f, colSec2);
            AddRoom("z10", "10. Retratos", "10. Pasillo de los Retratos (Sección 2)", -20f, 36f, 16f, 14f, colSec2);

            // Sección 3 (Este)
            AddRoom("z15", "15. Duchas", "15. Lavabos y Duchas Antiguas (Sección 3)", 11f, 0f, 16f, 12f, colSec3);
            AddRoom("z11", "11. Vestuario", "11. Vestuario Femenino (Sección 3)", 11f, 12f, 16f, 20f, colSec3);
            AddRoom("z13", "13. Taquillas [LLAVE]", "13. Taquillas Personales — Llave en Taquilla Nº 14", 27f, 12f, 12f, 24f, colSec3);
            AddRoom("z12", "12. Tocadores", "12. Sala de Tocadores con Espejos (Sección 3)", 11f, 32f, 16f, 14f, colSec3);
            AddRoom("z12_conn", "", "Corredor a Tocadores", 4f, 42f, 7f, 4f, colSec3);
            AddRoom("z14_corr", "", "Corredor Privado de Solista", 17f, 46f, 4f, 6f, colSec3);
            AddRoom("z14", "14. Hab. Elena [CERRADA]", "14. Habitación Privada de Elena Vesper (Sección 3)", 12f, 52f, 14f, 14f, colSec3);

            // Sección 4 (Noroeste)
            AddRoom("z20b", "20B. Atajo", "20. Atajo de Servicio hacia el Vestíbulo", -15f, 38f, 11f, 4f, colSec4);
            AddRoom("z16", "16. Aula Ballet", "16. Aula Principal de Ballet (Sección 4)", -26f, 50f, 22f, 18f, colSec4);
            AddRoom("z17", "17. Aula Espejos", "17. Aula de Espejos — Corrección Postural (Sección 4)", -38f, 50f, 12f, 18f, colSec4);
            AddRoom("z19", "19. Sala Individual", "19. Sala de Entrenamiento Individual (Sección 4)", -38f, 68f, 12f, 12f, colSec4);
            AddRoom("z18", "18. Maniquíes", "18. Sala de Maniquíes y Vestuario Escénico (Sección 4)", -26f, 68f, 14f, 14f, colSec4);
            AddRoom("z20a", "20. Servicio", "20. Pasillo de Servicio y Acceso Técnico (Sección 4)", -12f, 68f, 8f, 16f, colSec4);
            AddRoom("z20_east", "Zona Técnica", "20. Ramal Técnico del Escenario", -4f, 78f, 12f, 6f, colSec4);

            // Sección 5: Archivo Musical — Historia y Sonido (Noreste)
            AddRoom("z21_entry", "", "21. Galería de Entrada — Biblioteca Musical (Sección 5)", 4f, 58f, 8f, 8f, colSec5);
            AddRoom("z21", "21. Biblioteca Musical", "21. Biblioteca Musical — Catálogo y Versiones (Sección 5)", 4f, 66f, 22f, 12f, colSec5);
            AddRoom("z22", "22. Sala del Piano", "22. Sala del Piano de Cola (Sección 5)", 8f, 78f, 18f, 14f, colSec5);
            AddRoom("z23", "23. Partituras", "23. Archivo de Partituras (Sección 5)", 26f, 62f, 13f, 16f, colSec5);
            AddRoom("z24", "24. Grabaciones", "24. Cabina de Grabaciones Acústica (Sección 5)", 30f, 50f, 9f, 12f, colSec5);
            AddRoom("z25_pass", "", "25. Pasillo al Depósito de Instrumentos", 26f, 50f, 4f, 12f, colSec5);
            AddRoom("z25", "25. Instrumentos", "25. Depósito de Instrumentos (Sección 5)", 27f, 36f, 12f, 14f, colSec5);
        }

        private void AddRoom(string id, string shortLabel, string fullName, float xMin, float zMin, float width, float height, Color color)
        {
            GameObject roomObj = new GameObject($"MapRoom_{id}");
            roomObj.transform.SetParent(mapBoard, false);
            RectTransform rt = roomObj.AddComponent<RectTransform>();

            Vector2 minNorm = WorldToNormalized(new Vector2(xMin, zMin));
            Vector2 maxNorm = WorldToNormalized(new Vector2(xMin + width, zMin + height));

            rt.anchorMin = minNorm;
            rt.anchorMax = maxNorm;
            rt.offsetMin = new Vector2(1f, 1f);
            rt.offsetMax = new Vector2(-1f, -1f);

            Image img = roomObj.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;

            if (!string.IsNullOrEmpty(shortLabel))
            {
                GameObject lblObj = new GameObject("RoomLabel");
                lblObj.transform.SetParent(rt, false);
                TextMeshProUGUI tmp = lblObj.AddComponent<TextMeshProUGUI>();
                tmp.text = shortLabel;
                tmp.fontSize = 9.5f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = new Color(0.95f, 0.93f, 0.88f, 0.92f);
                tmp.enableWordWrapping = true;
                tmp.raycastTarget = false;

                RectTransform lrt = lblObj.GetComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = new Vector2(1f, 1f);
                lrt.offsetMax = new Vector2(-1f, -1f);
            }

            rooms.Add(new RoomZoneDef
            {
                id = id,
                shortLabel = shortLabel,
                fullName = fullName,
                worldXZ = new Rect(xMin, zMin, width, height),
                baseColor = color,
                uiBox = img
            });
        }

        private void BuildDoorwayConnectorsOnBoard()
        {
            // Pequeños indicadores luminosos en cada puerta para que se vea por dónde se pasa entre salas
            Vector2[] doorPositions =
            {
                new Vector2(0f, 12f),     // Entrada al Vestíbulo
                new Vector2(-11f, 18f),   // Vestíbulo -> Recepción Antigua
                new Vector2(11f, 20f),    // Vestíbulo -> Vestuario Femenino
                new Vector2(0f, 32f),     // Vestíbulo -> Pasillo Principal
                new Vector2(-24f, 18f),   // Recepción -> Oficina Dirección
                new Vector2(-17.5f, 24f), // Recepción -> Sala de Espera
                new Vector2(-31f, 26f),   // Dirección -> Archivo (Con llave)
                new Vector2(-17.5f, 36f), // Sala Espera -> Galería Retratos
                new Vector2(-4f, 48f),    // Pasillo -> Galería Retratos
                new Vector2(-4f, 40f),    // Pasillo -> Atajo Servicio
                new Vector2(-15f, 40f),   // Galería -> Atajo Servicio
                new Vector2(-17.5f, 50f), // Galería -> Aula Ballet
                new Vector2(-4f, 56f),    // Pasillo -> Aula Ballet
                new Vector2(-4f, 73f),    // Pasillo -> Pasillo Servicio
                new Vector2(4f, 44f),     // Pasillo -> Tocadores
                new Vector2(19f, 12f),    // Vestuario -> Duchas
                new Vector2(27f, 22f),    // Vestuario -> Taquillas
                new Vector2(19f, 32f),    // Vestuario -> Tocadores
                new Vector2(19f, 46f),    // Tocadores -> Corredor Elena
                new Vector2(19f, 52f),    // Corredor -> Habitación Elena (Con llave)
                new Vector2(-26f, 59f),   // Aula Ballet -> Aula Espejos
                new Vector2(-19f, 68f),   // Aula Ballet -> Maniquíes
                new Vector2(-8f, 68f),    // Aula Ballet -> Pasillo Servicio
                new Vector2(-32f, 68f),   // Aula Espejos -> Sala Individual
                new Vector2(-26f, 74f),   // Sala Individual -> Maniquíes
                new Vector2(-12f, 75f),   // Maniquíes -> Pasillo Servicio
                new Vector2(4f, 62f),     // Pasillo -> Biblioteca Musical (Sección 5)
                new Vector2(6f, 78f),     // Biblioteca -> Zona Técnica
                new Vector2(8f, 81f),     // Zona Técnica -> Sala del Piano
                new Vector2(17f, 78f),    // Biblioteca -> Sala del Piano
                new Vector2(26f, 72f),    // Biblioteca -> Archivo de Partituras
                new Vector2(34.5f, 62f),  // Archivo Partituras -> Cabina Grabaciones
                new Vector2(28f, 62f),    // Archivo Partituras -> Pasillo Depósito Instrumentos
                new Vector2(30f, 56f),    // Cabina Grabaciones -> Pasillo Depósito Instrumentos
                new Vector2(27f, 41f),    // Depósito Instrumentos -> Sala de Tocadores / Camerinos
                new Vector2(33f, 36f)     // Depósito Instrumentos -> Taquillas y Vestuarios
            };

            foreach (Vector2 dp in doorPositions)
            {
                GameObject dDot = new GameObject("DoorTick");
                dDot.transform.SetParent(mapBoard, false);
                RectTransform drt = dDot.AddComponent<RectTransform>();
                Vector2 norm = WorldToNormalized(dp);
                drt.anchorMin = norm;
                drt.anchorMax = norm;
                drt.anchoredPosition = Vector2.zero;
                drt.sizeDelta = new Vector2(5f, 5f);

                Image dImg = dDot.AddComponent<Image>();
                dImg.color = new Color(0.95f, 0.85f, 0.55f, 0.90f);
                dImg.raycastTarget = false;
            }
        }

        private void BuildSpecialMarkersOnBoard()
        {
            // Marcador LLAVE 1: Oficina de Dirección (-31, 19)
            keyArchivesMarker = CreateMapPin("Pin_Key_Archives", new Vector2(-30f, 19f), "LLAVE 1", new Color(0.98f, 0.78f, 0.18f), out keyArchivesBg);

            // Marcador LLAVE 2: Taquilla Nº 14 de Elena (37.5, 22.2)
            keyElenaMarker = CreateMapPin("Pin_Key_Elena", new Vector2(35f, 22.2f), "LLAVE 2", new Color(0.98f, 0.78f, 0.18f), out keyElenaBg);

            // Marcador del Jugador (Punto brillante + línea de dirección de mirada)
            GameObject pObj = new GameObject("PlayerMapMarker");
            pObj.transform.SetParent(mapBoard, false);
            playerDot = pObj.AddComponent<RectTransform>();
            playerDot.sizeDelta = new Vector2(11f, 11f);
            Image pImg = pObj.AddComponent<Image>();
            pImg.color = new Color(0.20f, 0.95f, 1.0f, 1f);
            pImg.raycastTarget = false;

            GameObject dirObj = new GameObject("PlayerLookPointer");
            dirObj.transform.SetParent(playerDot, false);
            playerDirectionLine = dirObj.AddComponent<RectTransform>();
            playerDirectionLine.pivot = new Vector2(0.5f, 0f);
            playerDirectionLine.anchoredPosition = Vector2.zero;
            playerDirectionLine.sizeDelta = new Vector2(3f, 12f);
            Image dirImg = dirObj.AddComponent<Image>();
            dirImg.color = new Color(1f, 1f, 1f, 0.95f);
            dirImg.raycastTarget = false;
        }

        private RectTransform CreateMapPin(string name, Vector2 worldXZ, string text, Color bgCol, out Image bgImage)
        {
            GameObject pin = new GameObject(name);
            pin.transform.SetParent(mapBoard, false);
            RectTransform rt = pin.AddComponent<RectTransform>();
            Vector2 norm = WorldToNormalized(worldXZ);
            rt.anchorMin = norm;
            rt.anchorMax = norm;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(46f, 15f);

            bgImage = pin.AddComponent<Image>();
            bgImage.color = bgCol;
            bgImage.raycastTarget = false;

            GameObject txtObj = new GameObject("PinText");
            txtObj.transform.SetParent(rt, false);
            TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 8.5f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.08f, 0.06f, 0.04f, 1f);
            tmp.raycastTarget = false;

            RectTransform trt = txtObj.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            return rt;
        }

        private void BuildKeyAndRoomGuidePanel()
        {
            guidePanelObj = new GameObject("KeyAndRoomGuide_Panel");
            guidePanelObj.transform.SetParent(mapContainer, false);
            RectTransform grt = guidePanelObj.AddComponent<RectTransform>();
            grt.anchorMin = new Vector2(0f, 0f);
            grt.anchorMax = new Vector2(1f, 0f);
            grt.pivot = new Vector2(0.5f, 1f);
            grt.anchoredPosition = new Vector2(0f, -6f);
            grt.sizeDelta = new Vector2(0f, 112f);

            Image gBg = guidePanelObj.AddComponent<Image>();
            gBg.color = new Color(0.06f, 0.06f, 0.09f, 0.92f);
            gBg.raycastTarget = false;

            GameObject txtObj = new GameObject("GuideText");
            txtObj.transform.SetParent(grt, false);
            keyGuideTMP = txtObj.AddComponent<TextMeshProUGUI>();
            keyGuideTMP.fontSize = 12.5f;
            keyGuideTMP.alignment = TextAlignmentOptions.TopLeft;
            keyGuideTMP.color = new Color(0.92f, 0.90f, 0.85f);
            keyGuideTMP.lineSpacing = 6f;
            keyGuideTMP.raycastTarget = false;

            RectTransform trt = txtObj.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(10f, 6f);
            trt.offsetMax = new Vector2(-10f, -6f);
        }

        private Vector2 WorldToNormalized(Vector2 worldXZ)
        {
            float nx = Mathf.InverseLerp(WorldMinX, WorldMaxX, worldXZ.x);
            float nz = Mathf.InverseLerp(WorldMinZ, WorldMaxZ, worldXZ.y);
            return new Vector2(nx, nz);
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                FindPlayerReferences();
                if (playerTransform == null) return;
            }

            HandleMapHotkeys();
            UpdatePlayerMarkerAndCurrentRoom();
            UpdateKeysAndGuideText();
        }

        private void HandleMapHotkeys()
        {
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
            }

            if (toggleGuide && guidePanelObj != null)
            {
                isGuideVisible = !isGuideVisible;
                guidePanelObj.SetActive(isGuideVisible);
            }
        }

        private void UpdatePlayerMarkerAndCurrentRoom()
        {
            Vector3 pPos = playerTransform.position;
            Vector2 norm = WorldToNormalized(new Vector2(pPos.x, pPos.z));

            if (playerDot != null)
            {
                playerDot.anchorMin = norm;
                playerDot.anchorMax = norm;
                playerDot.anchoredPosition = Vector2.zero;
            }

            if (playerDirectionLine != null && cameraTransform != null)
            {
                float yaw = cameraTransform.eulerAngles.y;
                playerDirectionLine.localRotation = Quaternion.Euler(0f, 0f, -yaw);
            }

            string activeRoomTitle = "Explorando la Academia";
            Vector2 pt = new Vector2(pPos.x, pPos.z);

            for (int i = 0; i < rooms.Count; i++)
            {
                RoomZoneDef r = rooms[i];
                bool inside = r.worldXZ.Contains(pt);
                if (r.uiBox != null)
                {
                    r.uiBox.color = inside
                        ? new Color(
                            Mathf.Min(1f, r.baseColor.r + 0.22f),
                            Mathf.Min(1f, r.baseColor.g + 0.22f),
                            Mathf.Min(1f, r.baseColor.b + 0.22f),
                            0.98f)
                        : r.baseColor;
                }
                if (inside && !string.IsNullOrEmpty(r.fullName))
                {
                    activeRoomTitle = r.fullName;
                }
            }

            if (currentZoneTMP != null)
            {
                currentZoneTMP.text = $"UBICACIÓN: {activeRoomTitle}";
            }
        }

        private void UpdateKeysAndGuideText()
        {
            bool hasArchiveKey = playerInventory != null && playerInventory.HasKey("key_archives");
            bool hasElenaKey = playerInventory != null && playerInventory.HasKey("key_elena_room");

            float pulse = 0.85f + Mathf.Sin(Time.unscaledTime * 3.5f) * 0.15f;

            if (keyArchivesBg != null)
            {
                keyArchivesBg.color = hasArchiveKey
                    ? new Color(0.22f, 0.78f, 0.38f, 0.95f)
                    : new Color(0.98f * pulse, 0.78f * pulse, 0.18f, 0.95f);
            }

            if (keyElenaBg != null)
            {
                keyElenaBg.color = hasElenaKey
                    ? new Color(0.22f, 0.78f, 0.38f, 0.95f)
                    : new Color(0.98f * pulse, 0.78f * pulse, 0.18f, 0.95f);
            }

            if (keyGuideTMP != null)
            {
                string st1 = hasArchiveKey ? "<color=#55EE77>[OBTENIDA]</color>" : "<color=#FFD447>[En 7. Oficina Dirección — Escritorio]</color>";
                string st2 = hasElenaKey ? "<color=#55EE77>[OBTENIDA]</color>" : "<color=#FFD447>[En 13. Taquillas — Taquilla Nº 14]</color>";

                keyGuideTMP.text =
                    $"<b>GUÍA DE LLAVES Y CUARTOS:</b>\n" +
                    $"• <b>Llave 1 (Archivo):</b> {st1} -> Abre <b>8. Archivo</b>\n" +
                    $"• <b>Llave 2 (Hab. Elena):</b> {st2} -> Abre <b>14. Hab. Elena</b>\n" +
                    $"<color=#B8B09A>[M] Ampliar/Reducir Mapa   |   [H] Ocultar/Mostrar Guía   |   Punto Azul = Tú</color>";
            }
        }
    }
}
