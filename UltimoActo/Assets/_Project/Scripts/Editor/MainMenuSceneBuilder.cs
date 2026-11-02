#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using TMPro;
using UltimoActo.UI;

namespace UltimoActo.Editor
{
    public static class MainMenuSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/MainMenu.unity";
        private const string BackgroundTexturePath = "Assets/_Project/Textures/MainMenu_Background.jpg";

        [MenuItem("Tools/Último Acto/2. Construir Menú Principal (MainMenu)")]
        public static void BuildMainMenuScene()
        {
            // 1. Crear o reiniciar escena en Assets/Scenes/MainMenu.unity
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            // --- 2. Carga infalible de la textura del Gran Teatro de Ópera como Sprite ---
            Sprite bgSprite = LoadOperaBackgroundSprite();

            // --- 3. Cámara Principal Cinemática ---
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            cameraObj.transform.position = new Vector3(0f, 0f, -10f);
            cameraObj.transform.rotation = Quaternion.identity;

            Camera cam = cameraObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.03f, 0.02f, 0.03f); // Negro borgoña teatral
            cameraObj.AddComponent<AudioListener>();
            cameraObj.AddComponent<MenuCameraPan>();

            // Partículas sutiles de polvo teatral flotando en la luz
            CreateTheatricalDustParticles(cameraObj.transform);

            // --- 4. EventSystem (Esencial para interacción del ratón y New Input System) ---
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            InputSystemUIInputModule inputModule = eventSystemObj.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
#else
            eventSystemObj.AddComponent<StandaloneInputModule>();
#endif

            // --- 5. Canvas de Interfaz (16:9 PC) ---
            GameObject canvasObj = new GameObject("MainMenu_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
            MainMenuController menuController = canvasObj.AddComponent<MainMenuController>();

            // --- 6. Fondo Cinemático del Gran Teatro de Ópera ---
            GameObject bgObj = new GameObject("Cinematic_Opera_Background");
            bgObj.transform.parent = canvasObj.transform;
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.raycastTarget = false; // ¡NUNCA bloquea clics!
            if (bgSprite != null)
            {
                bgImage.sprite = bgSprite;
                bgImage.color = Color.white;
            }
            else
            {
                bgImage.color = new Color(0.12f, 0.04f, 0.06f);
            }

            // Sombra ambiental central sutil para resaltar texto y botones
            GameObject centerVignette = new GameObject("Atmospheric_Center_Shade");
            centerVignette.transform.parent = canvasObj.transform;
            RectTransform cvRect = centerVignette.AddComponent<RectTransform>();
            cvRect.anchorMin = new Vector2(0.2f, 0f);
            cvRect.anchorMax = new Vector2(0.8f, 1f);
            cvRect.offsetMin = Vector2.zero;
            cvRect.offsetMax = Vector2.zero;
            Image cvImage = centerVignette.AddComponent<Image>();
            cvImage.color = new Color(0.04f, 0.02f, 0.03f, 0.35f);
            cvImage.raycastTarget = false;

            // ========================================================
            // PANEL PRINCIPAL CENTRADO (Título + Frase + Botones)
            // ========================================================
            GameObject mainPanel = new GameObject("Main_Buttons_Panel");
            mainPanel.transform.parent = canvasObj.transform;
            RectTransform mainPanelRect = mainPanel.AddComponent<RectTransform>();
            mainPanelRect.anchorMin = Vector2.zero;
            mainPanelRect.anchorMax = Vector2.one;
            mainPanelRect.offsetMin = Vector2.zero;
            mainPanelRect.offsetMax = Vector2.zero;

            // --- Encabezado: Título y Frase Narrativa (PERFECTAMENTE CENTRADO) ---
            GameObject titleContainer = new GameObject("Title_Container");
            titleContainer.transform.parent = mainPanel.transform;
            RectTransform titleRect = titleContainer.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -95f);
            titleRect.sizeDelta = new Vector2(1200f, 170f);

            // Título: Ú L T I M O   A C T O (ancho amplio para evitar que se corte la 'O')
            GameObject titleTextObj = new GameObject("Title_Text");
            titleTextObj.transform.parent = titleContainer.transform;
            TextMeshProUGUI titleTMP = titleTextObj.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "Ú L T I M O   A C T O";
            titleTMP.fontSize = 58;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.color = new Color(0.97f, 0.95f, 0.91f); // Marfil
            titleTMP.characterSpacing = 16f;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.enableWordWrapping = false;
            titleTMP.raycastTarget = false;
            RectTransform ttRect = titleTextObj.GetComponent<RectTransform>();
            ttRect.anchorMin = new Vector2(0f, 1f);
            ttRect.anchorMax = new Vector2(1f, 1f);
            ttRect.pivot = new Vector2(0.5f, 1f);
            ttRect.anchoredPosition = new Vector2(0f, 0f);
            ttRect.sizeDelta = new Vector2(1200f, 75f);

            // Frase Narrativa: “La función terminó. La verdad, no.”
            GameObject quoteTextObj = new GameObject("Quote_Text");
            quoteTextObj.transform.parent = titleContainer.transform;
            TextMeshProUGUI quoteTMP = quoteTextObj.AddComponent<TextMeshProUGUI>();
            quoteTMP.text = "“La función terminó. La verdad, no.”";
            quoteTMP.fontSize = 22;
            quoteTMP.fontStyle = FontStyles.Italic;
            quoteTMP.color = new Color(0.85f, 0.72f, 0.44f); // Oro envejecido cálido
            quoteTMP.alignment = TextAlignmentOptions.Center;
            quoteTMP.enableWordWrapping = false;
            quoteTMP.raycastTarget = false;
            RectTransform qtRect = quoteTextObj.GetComponent<RectTransform>();
            qtRect.anchorMin = new Vector2(0f, 1f);
            qtRect.anchorMax = new Vector2(1f, 1f);
            qtRect.pivot = new Vector2(0.5f, 1f);
            qtRect.anchoredPosition = new Vector2(0f, -80f);
            qtRect.sizeDelta = new Vector2(1000f, 35f);

            // Línea divisoria central con detalle ornamental
            GameObject goldLine = new GameObject("Gold_Accent_Line");
            goldLine.transform.parent = titleContainer.transform;
            Image lineImg = goldLine.AddComponent<Image>();
            lineImg.color = new Color(0.85f, 0.72f, 0.44f, 0.55f);
            lineImg.raycastTarget = false;
            RectTransform lineRect = goldLine.GetComponent<RectTransform>();
            lineRect.anchorMin = new Vector2(0.5f, 1f);
            lineRect.anchorMax = new Vector2(0.5f, 1f);
            lineRect.pivot = new Vector2(0.5f, 1f);
            lineRect.anchoredPosition = new Vector2(0f, -125f);
            lineRect.sizeDelta = new Vector2(560f, 2f);

            // --- Columna de Botones Principales (CENTRADA EN PANTALLA) ---
            GameObject buttonColumn = new GameObject("Buttons_Column");
            buttonColumn.transform.parent = mainPanel.transform;
            RectTransform colRect = buttonColumn.AddComponent<RectTransform>();
            colRect.anchorMin = new Vector2(0.5f, 0.5f);
            colRect.anchorMax = new Vector2(0.5f, 0.5f);
            colRect.pivot = new Vector2(0.5f, 0.5f);
            colRect.anchoredPosition = new Vector2(0f, -70f);
            colRect.sizeDelta = new Vector2(380f, 320f);

            VerticalLayoutGroup vlg = buttonColumn.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 18f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            Button btnNewGame = CreateMenuButton("Btn_NewGame", buttonColumn.transform, "NUEVA PARTIDA", 52f);
            Button btnCoop = CreateMenuButton("Btn_Coop", buttonColumn.transform, "COOPERATIVO", 52f);
            Button btnHowTo = CreateMenuButton("Btn_HowToPlay", buttonColumn.transform, "CÓMO JUGAR", 52f);
            Button btnQuit = CreateMenuButton("Btn_Quit", buttonColumn.transform, "SALIR", 52f);

            // ========================================================
            // SUBMENÚ: COOPERATIVO (Modal Centrado)
            // ========================================================
            GameObject coopPanel = CreateModalPanel("Coop_Submenu_Panel", canvasObj.transform, new Vector2(540f, 440f));
            CreatePanelHeader(coopPanel.transform, "INVESTIGACIÓN COOPERATIVA", "Conexión LAN para 2 Jugadores");

            GameObject coopButtons = new GameObject("Coop_Buttons_Column");
            coopButtons.transform.parent = coopPanel.transform;
            RectTransform cBtnRect = coopButtons.AddComponent<RectTransform>();
            cBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
            cBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
            cBtnRect.pivot = new Vector2(0.5f, 0.5f);
            cBtnRect.anchoredPosition = new Vector2(0f, -25f);
            cBtnRect.sizeDelta = new Vector2(400f, 220f);

            VerticalLayoutGroup cVlg = coopButtons.AddComponent<VerticalLayoutGroup>();
            cVlg.spacing = 16f;
            cVlg.childAlignment = TextAnchor.MiddleCenter;
            cVlg.childControlWidth = true;
            cVlg.childControlHeight = false;
            cVlg.childForceExpandWidth = true;
            cVlg.childForceExpandHeight = false;

            Button btnCreateRoom = CreateMenuButton("Btn_CreateRoom", coopButtons.transform, "CREAR SALA (HOST)", 48f);
            Button btnJoinRoom = CreateMenuButton("Btn_JoinRoom", coopButtons.transform, "UNIRSE A SALA (CLIENTE)", 48f);
            Button btnBackCoop = CreateMenuButton("Btn_BackCoop", coopButtons.transform, "VOLVER", 48f);

            // ========================================================
            // SUBMENÚ: CÓMO JUGAR (Modal Centrado)
            // ========================================================
            GameObject howToPanel = CreateModalPanel("HowToPlay_Panel", canvasObj.transform, new Vector2(860f, 620f));
            CreatePanelHeader(howToPanel.transform, "MANUAL DE INVESTIGACIÓN", "Reglas y Controles en la Academia del Cisne");

            GameObject instructionsObj = new GameObject("Instructions_Text");
            instructionsObj.transform.parent = howToPanel.transform;
            TextMeshProUGUI instTMP = instructionsObj.AddComponent<TextMeshProUGUI>();
            instTMP.fontSize = 17;
            instTMP.lineSpacing = 12f;
            instTMP.color = new Color(0.93f, 0.91f, 0.87f);
            instTMP.raycastTarget = false;
            instTMP.text =
                "<b>PREMISA</b>\n" +
                "Dos investigadores reciben una invitación a la abandonada Academia del Cisne.\n" +
                "Ningún jugador posee habilidades exclusivas. La verdad se descubre compartiendo pistas y coordinando mecanismos.\n\n" +
                "<b>CONTROLES DE EXPLORACIÓN</b>\n" +
                "• <color=#D8B468>[W, A, S, D]</color> Desplazarse por los salones de ballet\n" +
                "• <color=#D8B468>[Ratón]</color> Observar el entorno e inspeccionar espejos\n" +
                "• <color=#D8B468>[E]</color> Interactuar con puertas, documentos y mecanismos\n" +
                "• <color=#D8B468>[Shift Izquierdo]</color> Correr\n" +
                "• <color=#D8B468>[Tab]</color> Consultar el Cuaderno de Investigación compartido\n\n" +
                "<b>REGLAS CLAVE</b>\n" +
                "• <b>Progreso Compartido:</b> Puertas, llaves y puzles tienen el mismo estado para ambos.\n" +
                "• <b>Puzles Deducibles:</b> Las pistas nunca se pierden. Si un mecanismo falla, utiliza la palanca de reinicio.";

            RectTransform instRect = instructionsObj.GetComponent<RectTransform>();
            instRect.anchorMin = new Vector2(0.5f, 0.5f);
            instRect.anchorMax = new Vector2(0.5f, 0.5f);
            instRect.pivot = new Vector2(0.5f, 0.5f);
            instRect.anchoredPosition = new Vector2(0f, 10f);
            instRect.sizeDelta = new Vector2(780f, 410f);

            Button btnCloseHowTo = CreateMenuButton("Btn_CloseHowTo", howToPanel.transform, "VOLVER AL MENÚ", 46f);
            RectTransform closeRect = btnCloseHowTo.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.5f, 0f);
            closeRect.anchorMax = new Vector2(0.5f, 0f);
            closeRect.pivot = new Vector2(0.5f, 0f);
            closeRect.anchoredPosition = new Vector2(0f, 30f);
            closeRect.sizeDelta = new Vector2(300f, 46f);

            // ========================================================
            // SERIALIZACIÓN Y VINCULACIÓN AL CONTROLADOR
            // ========================================================
            SerializedObject serMenu = new SerializedObject(menuController);
            serMenu.FindProperty("mainPanel").objectReferenceValue = mainPanel;
            serMenu.FindProperty("coopPanel").objectReferenceValue = coopPanel;
            serMenu.FindProperty("howToPlayPanel").objectReferenceValue = howToPanel;
            serMenu.FindProperty("targetSceneName").stringValue = "SampleScene";

            serMenu.FindProperty("btnNewGame").objectReferenceValue = btnNewGame;
            serMenu.FindProperty("btnCoop").objectReferenceValue = btnCoop;
            serMenu.FindProperty("btnHowTo").objectReferenceValue = btnHowTo;
            serMenu.FindProperty("btnQuit").objectReferenceValue = btnQuit;
            serMenu.FindProperty("btnCreateRoom").objectReferenceValue = btnCreateRoom;
            serMenu.FindProperty("btnJoinRoom").objectReferenceValue = btnJoinRoom;
            serMenu.FindProperty("btnBackCoop").objectReferenceValue = btnBackCoop;
            serMenu.FindProperty("btnCloseHowTo").objectReferenceValue = btnCloseHowTo;

            serMenu.ApplyModifiedProperties();

            coopPanel.SetActive(false);
            howToPanel.SetActive(false);

            // --- 7. Registrar escenas en Build Settings ---
            ConfigureBuildSettings();

            // Guardar y refrescar
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=green>[Último Acto]</color> ¡Menú Principal Centrado con Arte de Ópera y Efectos interactivos creado en {ScenePath}!");
            EditorUtility.DisplayDialog("Último Acto — Menú Principal",
                "¡Menú Principal reconstruido exitosamente!\n\n" +
                "• Fondo de Gran Teatro de Ópera con telones borgoña, palcos dorados y lámpara de araña.\n" +
                "• Título centrado (sin cortes de texto) y frase narrativa equilibrada.\n" +
                "• Botones centrados con micro-animaciones (zoom suave al pasar el ratón, resplandor vino/oro y sonido).\n" +
                "• EventSystem activo y operativo.\n\n" +
                "Presiona 'Play' para probar.", "¡A jugar!");
        }

        private static Sprite LoadOperaBackgroundSprite()
        {
            AssetDatabase.ImportAsset(BackgroundTexturePath, ImportAssetOptions.ForceUpdate);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundTexturePath);
            if (sprite != null) return sprite;

            // Respaldo dinámico directo desde Texture2D si el importer no devolvió sprite
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(BackgroundTexturePath);
            if (tex != null)
            {
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }

            Debug.LogWarning($"[MainMenu] No se pudo cargar el sprite en {BackgroundTexturePath}");
            return null;
        }

        private static void CreateTheatricalDustParticles(Transform parent)
        {
            GameObject dustObj = new GameObject("Theatrical_Dust_Particles");
            dustObj.transform.parent = parent;
            dustObj.transform.localPosition = new Vector3(0f, 0f, 8f);

            ParticleSystem ps = dustObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 8f;
            main.startSpeed = 0.15f;
            main.startSize = 0.08f;
            main.startColor = new Color(0.95f, 0.85f, 0.65f, 0.35f); // Partículas doradas suaves
            main.maxParticles = 60;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 8f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(14f, 8f, 4f);

            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.04f, 0.04f);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0.02f, 0.08f); // Flotan lentamente hacia arriba
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);
        }

        private static Button CreateMenuButton(string name, Transform parent, string label, float height)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.parent = parent;

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(380f, height);

            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.08f, 0.03f, 0.05f, 0.90f);
            img.raycastTarget = true; // El botón sí recibe clics

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(0.08f, 0.03f, 0.05f, 0.90f);
            cb.highlightedColor = new Color(0.38f, 0.09f, 0.16f, 0.98f); // Resplandor vino
            cb.pressedColor = new Color(0.55f, 0.14f, 0.24f, 1f);
            cb.selectedColor = new Color(0.24f, 0.06f, 0.11f, 0.95f);
            cb.fadeDuration = 0.1f;
            btn.colors = cb;

            // Borde sutil de oro envejecido en la base
            GameObject goldBorder = new GameObject("Gold_Accent_Border");
            goldBorder.transform.parent = btnObj.transform;
            Image bImg = goldBorder.AddComponent<Image>();
            bImg.color = new Color(0.85f, 0.72f, 0.44f, 0.55f);
            bImg.raycastTarget = false;
            RectTransform bRect = goldBorder.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0f, 0f);
            bRect.anchorMax = new Vector2(1f, 0f);
            bRect.pivot = new Vector2(0.5f, 0f);
            bRect.anchoredPosition = Vector2.zero;
            bRect.sizeDelta = new Vector2(0f, 2f);

            // Texto del Botón (Marfil)
            GameObject txtObj = new GameObject("Label");
            txtObj.transform.parent = btnObj.transform;
            TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 17;
            tmp.fontStyle = FontStyles.Bold;
            tmp.characterSpacing = 6f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.96f, 0.94f, 0.90f);
            tmp.raycastTarget = false;

            RectTransform tRect = txtObj.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;

            // Añadir el script de efectos dinámicos en hover/press
            btnObj.AddComponent<MenuButtonEffects>();

            return btn;
        }

        private static GameObject CreateModalPanel(string name, Transform parent, Vector2 size)
        {
            GameObject panel = new GameObject(name);
            panel.transform.parent = parent;
            RectTransform pr = panel.AddComponent<RectTransform>();
            pr.anchorMin = new Vector2(0.5f, 0.5f);
            pr.anchorMax = new Vector2(0.5f, 0.5f);
            pr.pivot = new Vector2(0.5f, 0.5f);
            pr.anchoredPosition = Vector2.zero;
            pr.sizeDelta = size;

            Image pImg = panel.AddComponent<Image>();
            pImg.color = new Color(0.06f, 0.03f, 0.04f, 0.97f);
            pImg.raycastTarget = true; // Bloquea clics detrás del modal

            GameObject border = new GameObject("Gold_Modal_Border");
            border.transform.parent = panel.transform;
            RectTransform bRect = border.AddComponent<RectTransform>();
            bRect.anchorMin = Vector2.zero;
            bRect.anchorMax = Vector2.one;
            bRect.offsetMin = new Vector2(-1.5f, -1.5f);
            bRect.offsetMax = new Vector2(1.5f, 1.5f);
            Image bImg = border.AddComponent<Image>();
            bImg.color = new Color(0.85f, 0.72f, 0.44f, 0.7f);
            bImg.raycastTarget = false;
            border.transform.SetAsFirstSibling();

            return panel;
        }

        private static void CreatePanelHeader(Transform parent, string title, string subtitle)
        {
            GameObject hObj = new GameObject("Header_Container");
            hObj.transform.parent = parent;
            RectTransform hr = hObj.AddComponent<RectTransform>();
            hr.anchorMin = new Vector2(0.5f, 1f);
            hr.anchorMax = new Vector2(0.5f, 1f);
            hr.pivot = new Vector2(0.5f, 1f);
            hr.anchoredPosition = new Vector2(0f, -25f);
            hr.sizeDelta = new Vector2(500f, 75f);

            GameObject tObj = new GameObject("Title");
            tObj.transform.parent = hObj.transform;
            TextMeshProUGUI tTmp = tObj.AddComponent<TextMeshProUGUI>();
            tTmp.text = title;
            tTmp.fontSize = 22;
            tTmp.fontStyle = FontStyles.Bold;
            tTmp.characterSpacing = 5f;
            tTmp.alignment = TextAlignmentOptions.Center;
            tTmp.color = new Color(0.96f, 0.94f, 0.90f);
            tTmp.raycastTarget = false;
            RectTransform tr = tObj.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0f, 1f);
            tr.anchorMax = new Vector2(1f, 1f);
            tr.pivot = new Vector2(0.5f, 1f);
            tr.anchoredPosition = Vector2.zero;
            tr.sizeDelta = new Vector2(0f, 35f);

            GameObject sObj = new GameObject("Subtitle");
            sObj.transform.parent = hObj.transform;
            TextMeshProUGUI sTmp = sObj.AddComponent<TextMeshProUGUI>();
            sTmp.text = subtitle;
            sTmp.fontSize = 14;
            sTmp.alignment = TextAlignmentOptions.Center;
            sTmp.color = new Color(0.85f, 0.72f, 0.44f);
            sTmp.raycastTarget = false;
            RectTransform sr = sObj.GetComponent<RectTransform>();
            sr.anchorMin = new Vector2(0f, 0f);
            sr.anchorMax = new Vector2(1f, 0f);
            sr.pivot = new Vector2(0.5f, 0f);
            sr.anchoredPosition = Vector2.zero;
            sr.sizeDelta = new Vector2(0f, 25f);
        }

        private static void ConfigureBuildSettings()
        {
            var scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Academy_Prototype.unity", true)
            };
            EditorBuildSettings.scenes = scenes;
        }
    }
}
#endif
