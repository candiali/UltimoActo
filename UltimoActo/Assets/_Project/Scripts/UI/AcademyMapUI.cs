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

        // Límites del mundo representados en el plano (X: -68 a +96, Z: -38 a +154)
        private const float WorldMinX = -68f;
        private const float WorldMaxX = 96f;
        private const float WorldMinZ = -38f;
        private const float WorldMaxZ = 154f;

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

            // Sección 6: Camerinos y Backstage — Zona Restringida (Norte y Este)
            Color colSec6 = new Color(0.45f, 0.24f, 0.18f, 0.88f);
            AddRoom("z26_hall_e", "Galería Servicio", "Circulación Secundaria a Camerinos y Salida Exterior (Sección 6)", 39f, 36f, 6f, 42f, colSec6);
            AddRoom("z26_vest", "", "Vestíbulo de Camerinos Principales (Sección 6)", 26f, 78f, 19f, 5f, colSec6);
            AddRoom("z26", "26. Cam. Mara", "26. Camerino de Mara Bell (Sección 6)", 26f, 83f, 7.5f, 9f, colSec6);
            AddRoom("z27", "27. Cam. Elena", "27. Camerino de Elena Vesper — Cisne Negro (Sección 6)", 33.5f, 83f, 7.5f, 9f, colSec6);
            AddRoom("z28_conn_e", "", "Paso Este al Backstage (Sección 6)", 41f, 83f, 4f, 9f, colSec6);
            AddRoom("z28_link_s", "", "Conector Técnico al Backstage (Sección 6)", -4f, 84f, 12f, 8f, colSec6);
            AddRoom("z28", "28. Pasillo Backstage", "28. Pasillo Detrás del Escenario (Sección 6)", -12f, 92f, 57f, 6f, colSec6);
            AddRoom("z29", "29. Almacén Decorados", "29. Almacén de Decorados Escenográficos (Sección 6)", 10.6f, 98f, 27.4f, 16f, colSec6);
            AddRoom("z30", "30. Poleas [PUZLE]", "30. Sala de Mecanismos y Poleas — Abre el Escenario (Sección 6)", -28f, 84f, 16f, 20f, colSec6);

            // Sección 7: Teatro Principal — Clímax (Zonas 31 a 35)
            Color colSec7 = new Color(0.54f, 0.16f, 0.22f, 0.90f);
            Color colSec7Upper = new Color(0.28f, 0.36f, 0.48f, 0.88f);
            AddRoom("z32", "32. ESCENARIO PRINCIPAL", "32. Escenario Principal y Pasarela Superior (Sección 7)", -22f, 98f, 28f, 18f, colSec7);
            AddRoom("z33", "33. Foso Orquesta", "33. Foso de Orquesta (Sección 7)", -20f, 116f, 24f, 6f, new Color(0.38f, 0.28f, 0.16f, 0.90f));
            AddRoom("z31", "31. PATIO DE BUTACAS", "31. Patio de Butacas y Balcones — Gran Teatro (Sección 7)", -26f, 122f, 36f, 28f, colSec7);
            AddRoom("z34_stairs", "Esc. Cabina", "Escalera Técnica a Cabina y Pasarela (Sección 7)", -34f, 104f, 8f, 14f, colSec7Upper);
            AddRoom("z34", "34. Cabina Luces", "34. Cabina de Iluminación (Nivel Superior — Sección 7)", -34f, 118f, 12f, 14f, colSec7Upper);

            // Sección 8: Zona Secreta — La Verdad Oculta (Zonas 36 a 39)
            Color colSec8 = new Color(0.36f, 0.16f, 0.38f, 0.90f);
            AddRoom("z36", "36. Esc. Oculta", "36. Escalera Oculta (Sección 8 — Zona Secreta)", 10f, 114.2f, 19f, 3.8f, colSec8);
            AddRoom("z37", "37. Depósito", "37. Depósito Clausurado Subterráneo (Sección 8)", 29f, 114.2f, 13f, 12.3f, colSec8);
            AddRoom("z38", "38. Arch. Prohibidos", "38. Sala de Archivos Prohibidos (Sección 8)", 16f, 126.5f, 26f, 12f, colSec8);
            AddRoom("z39", "39. Cám. Escenario", "39. Cámara tras el Escenario — La Verdad Oculta (Sección 8)", 6f, 98.5f, 4.6f, 8.5f, colSec8);

            // Sección 9: Jardín de Invierno (Zona 40)
            Color colSec9 = new Color(0.18f, 0.33f, 0.22f, 0.90f);
            AddRoom("z40", "40. JARDÍN DE INVIERNO", "40. Jardín de Invierno — Gran Invernadero Clásico (Sección 9)", 45f, 22f, 48f, 70f, colSec9);

            // Sección 10: Sótano de Mantenimiento, Terraza y Azotea, Enfermería Antigua y Sala Conmemorativa (Zonas 41 a 44)
            Color colSec10Base = new Color(0.26f, 0.24f, 0.20f, 0.90f);
            Color colSec10Wing = new Color(0.24f, 0.36f, 0.38f, 0.90f);
            Color colSec10Memorial = new Color(0.38f, 0.24f, 0.28f, 0.90f);
            AddRoom("z41", "41. SÓTANO MANTENIMIENTO", "41. Sótano de Mantenimiento — Tuberías y Depósitos (Nivel Subterráneo)", 16f, -34f, 38f, 20f, colSec10Base);
            AddRoom("z41_stair", "Esc. Sótano", "41. Escalera Técnica al Sótano de Mantenimiento", 16f, -14f, 24f, 14f, colSec10Base);
            AddRoom("z42_hub", "42. Esc. Azotea", "Torreón del Ala Suroeste y Escalera a la Terraza y Azotea (Zona 42)", -25f, -4f, 10.5f, 16f, colSec10Wing);
            AddRoom("z43", "43. ENFERMERÍA", "43. Enfermería Antigua — Camillas y Registros Clínicos", -42f, -6f, 17f, 18f, colSec10Wing);
            AddRoom("z44", "44. SALA CONMEMORATIVA", "44. Sala Conmemorativa — Placas y Memoria de Alumnas", -38f, -22f, 23.5f, 18f, colSec10Memorial);

            // Sección 11: Gran Ala Occidental de Residencia y Talleres (Zonas 45 a 48)
            Color colSec11Dorm = new Color(0.28f, 0.26f, 0.38f, 0.90f);
            Color colSec11Work = new Color(0.36f, 0.26f, 0.20f, 0.90f);
            Color colSec11Dine = new Color(0.34f, 0.28f, 0.18f, 0.90f);
            Color colSec11Isadora = new Color(0.42f, 0.16f, 0.26f, 0.90f);
            AddRoom("z48_spy", "Celosía", "48. Galería de la Celosía de Vigilancia de Isadora Vale", -62f, 6f, 20f, 6f, colSec11Isadora);
            AddRoom("z48", "48. ESTANCIA ISADORA", "48. Estancia Privada de Isadora Vale y Celosía de Vigilancia", -62f, 12f, 24f, 16f, colSec11Isadora);
            AddRoom("z47_gal", "Gal. Patronato", "47. Galería de Enlace al Salón del Patronato", -38f, 44f, 18f, 6f, colSec11Dine);
            AddRoom("z47", "47. COMEDOR Y PATRONATO", "47. Comedor Antiguo y Salón del Patronato", -62f, 28f, 24f, 26f, colSec11Dine);
            AddRoom("z45", "45. DORMITORIO INTERNAS", "45. Dormitorio Común de Internas", -62f, 54f, 24f, 24f, colSec11Dorm);
            AddRoom("z46", "46. TALLER Y TINTORERÍA", "46. Taller de Zapatillas y Tintorería Escénica", -62f, 78f, 24f, 20f, colSec11Work);
            AddRoom("z46_east", "Patronaje", "46. Nave Este de Patronaje Escénico", -38f, 80f, 10f, 18f, colSec11Work);

            // Sección 12: Planta Alta, Red Subterránea y Exteriores Traseros (Zonas 49 a 59)
            Color colSec12Upper = new Color(0.30f, 0.38f, 0.48f, 0.90f);
            Color colSec12Sub = new Color(0.24f, 0.20f, 0.28f, 0.90f);
            Color colSec12Ext = new Color(0.22f, 0.32f, 0.26f, 0.90f);

            // Planta Alta (Y = 7.70m)
            AddRoom("z49", "49. MEZZANINE", "49. Galería Superior del Vestíbulo (Mezzanine — Nivel Superior)", -11f, 9f, 22f, 22f, colSec12Upper);
            AddRoom("z50", "50. TEORÍA ESCÉNICA", "50. Aulas de Teoría Escénica y Linterna Mágica (Nivel Superior)", -16f, 31f, 32f, 24f, colSec12Upper);
            AddRoom("z51", "51. RESIDENCIA PROF.", "51. Residencia de Profesores y del Maestro Viktor Krol (Nivel Superior)", -16f, 55f, 32f, 17f, colSec12Upper);
            AddRoom("z52", "52. TORRE DEL RELOJ", "52. Desván y Torre del Reloj de la Academia (Nivel Superior)", -12f, 72f, 24f, 16f, colSec12Upper);

            // Red Subterránea (Y = -3.20m)
            AddRoom("z53", "53. CALDERAS", "53. Sala de Calderas y Generadores Eléctricos (Nivel Subterráneo)", -15f, -34f, 31f, 20f, colSec12Sub);
            AddRoom("z54", "54. LAVANDERÍA", "54. Lavandería Industrial Subterránea y Montacargas (Nivel Subterráneo)", -65f, 18f, 27f, 18f, colSec12Sub);
            AddRoom("z56", "56. CRIPTA ANTIGUA", "56. Cripta y Cimientos del Siglo XVIII (Nivel Subterráneo)", -4f, -14f, 8f, 74f, colSec12Sub);
            AddRoom("z55", "55. UNDERSTAGE", "55. Foso Bajo el Escenario — Understage (Nivel Subterráneo)", -22f, 60f, 32f, 48f, colSec12Sub);

            // Exteriores Traseros (Y = 0.0m a 2.4m)
            AddRoom("z57", "57. COCHERA", "57. Cochera de Carruajes y Patio de Carga Norte (Exterior)", -65f, 98f, 35f, 27f, colSec12Ext);
            AddRoom("z58", "58. PABELLÓN GUARDA", "58. Pabellón del Guarda y Centralita Exterior (Exterior)", -65f, 125f, 17f, 20f, colSec12Ext);
            AddRoom("z59", "59. ESTANQUE CISNES", "59. Estanque Exterior Helado y Quiosco del Parque (Exterior)", 30f, 98f, 55f, 47f, colSec12Ext);
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
                new Vector2(33f, 36f),    // Depósito Instrumentos -> Taquillas y Vestuarios
                new Vector2(39f, 43.2f),  // Depósito Instrumentos -> Galería de Camerinos / Backstage (Sección 6)
                new Vector2(42f, 36f),    // Galería de Camerinos -> Puerta de Servicio Exterior
                new Vector2(26f, 80.5f),  // Sala del Piano -> Vestíbulo de Camerinos (Mara y Elena)
                new Vector2(29.75f, 83f), // Vestíbulo -> Camerino de Mara (Zona 26)
                new Vector2(37.25f, 83f), // Vestíbulo -> Camerino de Elena (Zona 27)
                new Vector2(43f, 92f),    // Conector Camerinos -> Pasillo Detrás del Escenario (Zona 28)
                new Vector2(0f, 84f),     // Zona Técnica -> Pasillo Detrás del Escenario (Zona 28)
                new Vector2(22f, 98f),    // Pasillo Backstage -> Almacén de Decorados (Zona 29)
                new Vector2(-12f, 95f),   // Pasillo Backstage -> Sala de Mecanismos y Poleas (Zona 30)
                new Vector2(-12f, 101f),  // Sala de Poleas -> Hombro Oeste del Escenario Principal (Zona 32)
                new Vector2(-2.5f, 98f),  // Pasillo Backstage -> Escenario Principal (Puertas Dobles)
                new Vector2(3.5f, 98f),   // Pasillo Backstage -> Acceso Lateral de Cortinas al Escenario
                new Vector2(-21f, 118.8f),// Escenario -> Escalera al Foso de Orquesta (Zona 33)
                new Vector2(-24.2f, 121f),// Escenario -> Patio de Butacas Oeste (Zona 31)
                new Vector2(8f, 121f),    // Escenario -> Patio de Butacas Este (Zona 31)
                new Vector2(-29.5f, 106f),// Escalera Técnica -> Cabina de Iluminación (34) y Pasarela Superior (35)
                new Vector2(10f, 116f),   // Panel Móvil (B-00) -> Escalera Oculta (Zona 36)
                new Vector2(29f, 116f),   // Escalera Oculta -> Depósito Clausurado (Zona 37)
                new Vector2(35.5f, 126.5f),// Depósito Clausurado -> Archivos Prohibidos (Zona 38)
                new Vector2(16f, 132.5f), // Archivos Prohibidos -> Pasaje a Cámara tras el Escenario (Zona 39)
                new Vector2(6f, 105.4f),  // Cámara tras el Escenario -> Escenario Principal
                new Vector2(45f, 80.5f),  // Pasillo de la Academia -> Jardín de Invierno (Entrada de Madera Oscura y Cristal)
                new Vector2(45f, 58f),    // Corredor de Mantenimiento -> Jardín de Invierno
                new Vector2(45f, 34.8f),  // Rampa del Patio Principal -> Jardín de Invierno
                new Vector2(19f, 0f),     // Lavabos y Duchas -> Escalera Norte del Sótano de Mantenimiento (Zona 41)
                new Vector2(42f, -12f),   // Sótano de Mantenimiento -> Escalera Este hacia Callejón de Servicio y Jardín
                new Vector2(14f, -10f),   // Patio de Entrada -> Paso de Servicio al Sótano y Jardín
                new Vector2(-22f, 12f),   // Recepción Antigua -> Torreón del Ala Suroeste (Zonas 42, 43 y 44)
                new Vector2(-17f, 11.5f), // Torreón -> Terraza y Azotea Mirador (Zona 42, Nivel Superior)
                new Vector2(-25f, 3f),    // Torreón -> Enfermería Antigua (Zona 43)
                new Vector2(-22.2f, -4f), // Torreón -> Sala Conmemorativa (Zona 44)
                new Vector2(-42f, 8.5f),  // Enfermería Antigua -> Galería de la Celosía de Vigilancia (Zona 48)
                new Vector2(-38f, 14.5f), // Oficina de Dirección -> Estancia Privada de Isadora Vale (Zona 48)
                new Vector2(-50f, 28f),   // Estancia de Isadora Vale -> Comedor Antiguo y Salón del Patronato (Zona 47)
                new Vector2(-20f, 47f),   // Pasillo de los Retratos -> Galería del Patronato (Zona 47)
                new Vector2(-38f, 47f),   // Galería del Patronato -> Comedor Antiguo y Salón del Patronato (Zona 47)
                new Vector2(-50f, 54f),   // Comedor Antiguo -> Dormitorio Común de Internas (Zona 45)
                new Vector2(-50f, 78f),   // Dormitorio Común -> Taller de Zapatillas y Tintorería Escénica (Zona 46)
                new Vector2(-28f, 85.5f), // Taller de Zapatillas y Tintorería -> Sala de Mecanismos y Poleas (Zona 30)
                // Conectores de Sección 12 (Zonas 49 a 59)
                new Vector2(-11f, 20f),   // Terraza / Azotea -> Mezzanine / Planta Alta (Zona 49)
                new Vector2(0f, 31f),     // Mezzanine -> Aulas de Teoría Escénica (Zona 50)
                new Vector2(0f, 55f),     // Aulas de Teoría -> Residencia de Profesores / Maestro Krol (Zona 51)
                new Vector2(0f, 72f),     // Residencia de Profesores -> Torre del Reloj (Zona 52)
                new Vector2(0f, 88f),     // Torre del Reloj -> Pasarela Superior del Teatro (Zona 35)
                new Vector2(16f, -21.5f), // Sótano de Mantenimiento -> Sala de Calderas (Zona 53)
                new Vector2(-15f, -24f),  // Sala de Calderas -> Lavandería Subterránea (Zona 54)
                new Vector2(0.5f, -14f),  // Sala de Calderas -> Cripta Fundacional del Siglo XVIII (Zona 56)
                new Vector2(0f, 60f),     // Cripta Fundacional -> Foso Bajo el Escenario / Understage (Zona 55)
                new Vector2(10f, 100f),   // Foso Bajo el Escenario -> Bóveda B-00 / Cámara tras el Escenario (Zona 39)
                new Vector2(-50f, 98f),   // Taller de Zapatillas -> Cochera de Carruajes (Zona 57)
                new Vector2(-56.5f, 125f),// Cochera de Carruajes -> Pabellón del Guarda y Verja Exterior (Zona 58)
                new Vector2(30f, 98f)     // Jardín de Invierno / Pasillo -> Estanque Exterior y Quiosco (Zona 59)
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

            // Marcador PUZLE DE POLEAS (Zona 30 -> Abre Escenario Principal)
            CreateMapPin("Pin_Stage_Pulley", new Vector2(-20f, 95f), "POLEAS", new Color(0.95f, 0.55f, 0.25f), out _);

            // Marcador CLÍMAX EN ESCENARIO PRINCIPAL (Zona 32)
            CreateMapPin("Pin_Stage_Climax", new Vector2(-8f, 109f), "CLÍMAX", new Color(0.95f, 0.32f, 0.38f), out _);

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

            // Detección de nivel vertical para Planta Alta (Y >= 6.0m), Nivel Subterráneo (Y <= -1.0m) y Exteriores
            if (pPos.y >= 6.0f)
            {
                if (new Rect(-13.0f, 7.0f, 26.0f, 25.0f).Contains(pt))
                    activeRoomTitle = "49. Galería Superior del Vestíbulo (Mezzanine) — Planta Alta";
                else if (new Rect(-18.0f, 30.0f, 36.0f, 26.0f).Contains(pt))
                    activeRoomTitle = "50. Aulas de Teoría Escénica y Linterna Mágica — Planta Alta";
                else if (new Rect(-18.0f, 54.0f, 36.0f, 19.0f).Contains(pt))
                    activeRoomTitle = "51. Residencia de Profesores y del Maestro Viktor Krol — Planta Alta";
                else if (new Rect(-14.0f, 71.0f, 28.0f, 18.0f).Contains(pt))
                    activeRoomTitle = "52. Desván y Torre del Reloj de la Academia — Planta Alta";
                else if (new Rect(-38.5f, 10.5f, 28.0f, 34.0f).Contains(pt))
                    activeRoomTitle = "42. Terraza y Azotea Mirador — Nivel Superior";
                else if (new Rect(-24.0f, 96.0f, 32.0f, 22.0f).Contains(pt))
                    activeRoomTitle = "35. Pasarela Superior del Teatro — Nivel Superior";
            }
            else if (pPos.y <= -1.0f)
            {
                if (new Rect(-16.0f, -36.0f, 34.0f, 23.0f).Contains(pt))
                    activeRoomTitle = "53. Sala de Calderas y Generadores Eléctricos — Nivel Subterráneo";
                else if (new Rect(-66.0f, -5.0f, 53.0f, 43.0f).Contains(pt))
                    activeRoomTitle = "54. Lavandería Industrial Subterránea y Montacargas — Nivel Subterráneo";
                else if (new Rect(-6.0f, -15.0f, 12.0f, 76.0f).Contains(pt))
                    activeRoomTitle = "56. Cripta y Cimientos del Siglo XVIII — Nivel Subterráneo";
                else if (new Rect(-24.0f, 59.0f, 36.0f, 50.0f).Contains(pt))
                    activeRoomTitle = "55. Foso Bajo el Escenario (Understage) — Nivel Subterráneo";
                else if (new Rect(15.5f, -34.5f, 39.0f, 35.0f).Contains(pt))
                    activeRoomTitle = "41. Sótano de Mantenimiento — Nivel Subterráneo";
                else if (new Rect(14.0f, 112.0f, 30.0f, 28.0f).Contains(pt))
                    activeRoomTitle = "37-38. Bóveda Subterránea B-00 y Archivos Prohibidos";
            }
            else if (new Rect(-66.0f, 97.0f, 38.0f, 30.0f).Contains(pt))
            {
                activeRoomTitle = "57. Cochera de Carruajes y Patio de Carga Norte — Exterior";
            }
            else if (new Rect(-66.0f, 124.0f, 20.0f, 23.0f).Contains(pt))
            {
                activeRoomTitle = "58. Pabellón del Guarda y Verja Exterior — Exterior";
            }
            else if (new Rect(28.0f, 97.0f, 59.0f, 50.0f).Contains(pt))
            {
                activeRoomTitle = "59. Estanque Exterior Helado y Quiosco de Música — Parque";
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
                    $"<b>GUÍA DE LLAVES Y ACCESOS:</b>\n" +
                    $"• <b>Llave 1 (Archivo):</b> {st1} -> Abre <b>8. Archivo</b>\n" +
                    $"• <b>Llave 2 (Hab. Elena):</b> {st2} -> Abre <b>14. Hab. Elena</b>\n" +
                    $"• <b>Escenario Principal:</b> <color=#FFA855>[En 30. Sala de Poleas — Palanca III]</color>\n" +
                    $"<color=#B8B09A>[M] Ampliar/Reducir Mapa   |   [H] Ocultar/Mostrar Guía</color>";
            }
        }
    }
}
