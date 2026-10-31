#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UltimoActo.Core;
using UltimoActo.Player;
using UltimoActo.Interaction;
using UltimoActo.Puzzles;
using UltimoActo.Narrative;
using UltimoActo.UI;

namespace UltimoActo.Editor
{
    public static class PrototypeSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Academy_Prototype.unity";
        private const string MaterialsPath = "Assets/_Project/Materials/";

        [MenuItem("Tools/Último Acto/1. Construir Sala Prototipo Greybox (Fase 1)")]
        public static void BuildPrototypeScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            if (!AssetDatabase.IsValidFolder("Assets/_Project/Materials"))
                AssetDatabase.CreateFolder("Assets/_Project", "Materials");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes"))
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");

            // --- 1. Materiales Greybox Temáticos ---
            Material matFloor = GetOrCreateMaterial("Mat_Floor", new Color(0.18f, 0.18f, 0.2f));
            Material matParquet = GetOrCreateMaterial("Mat_Parquet", new Color(0.35f, 0.25f, 0.18f)); // Madera
            Material matWall = GetOrCreateMaterial("Mat_Wall", new Color(0.85f, 0.83f, 0.8f));        // Marfil
            Material matDoor1 = GetOrCreateMaterial("Mat_Door1", new Color(0.45f, 0.1f, 0.15f));      // Vino tinto
            Material matDoor2 = GetOrCreateMaterial("Mat_Door2", new Color(0.15f, 0.35f, 0.45f));     // Azul noche
            Material matKey = GetOrCreateMaterial("Mat_Key", new Color(0.95f, 0.75f, 0.2f));          // Dorado
            Material matDarkProp = GetOrCreateMaterial("Mat_DarkProp", new Color(0.12f, 0.12f, 0.12f));
            Material matIndicator = GetOrCreateMaterial("Mat_Indicator", new Color(0.9f, 0.85f, 0.6f));

            // --- 2. Iluminación Ambiental ---
            GameObject dirLight = new GameObject("Directional Light");
            Light lightComp = dirLight.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            lightComp.color = new Color(0.75f, 0.8f, 0.9f);
            lightComp.intensity = 0.4f;
            dirLight.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

            GameObject rootEnv = new GameObject("Environment_Academy");

            // ==========================================
            // SALA 1: RECEPCIÓN Y ARCHIVO (Z: -6 a 6)
            // ==========================================
            GameObject floor1 = CreatePrimitive("Floor_Reception", PrimitiveType.Cube, rootEnv.transform,
                new Vector3(0f, -0.25f, 0f), new Vector3(12f, 0.5f, 12f), matFloor);

            // Paredes Sala 1
            CreateWall("Wall_Rec_Back", rootEnv.transform, new Vector3(0f, 2f, -6f), new Vector3(12f, 4f, 0.4f), matWall);
            CreateWall("Wall_Rec_Left", rootEnv.transform, new Vector3(-6f, 2f, 0f), new Vector3(0.4f, 4f, 12f), matWall);
            CreateWall("Wall_Rec_Right", rootEnv.transform, new Vector3(6f, 2f, 0f), new Vector3(0.4f, 4f, 12f), matWall);
            CreateWall("Wall_Rec_Front_L", rootEnv.transform, new Vector3(-3.5f, 2f, 6f), new Vector3(5f, 4f, 0.4f), matWall);
            CreateWall("Wall_Rec_Front_R", rootEnv.transform, new Vector3(3.5f, 2f, 6f), new Vector3(5f, 4f, 0.4f), matWall);
            CreateWall("Wall_Rec_Front_Top", rootEnv.transform, new Vector3(0f, 3.25f, 6f), new Vector3(2f, 1.5f, 0.4f), matWall);

            // Luz Sala 1
            CreatePointLight("Light_Reception", rootEnv.transform, new Vector3(0f, 3.2f, 0f), new Color(1f, 0.9f, 0.75f), 12f, 1.8f);

            // Pedestal con Llave Inicial
            GameObject ped1 = CreatePrimitive("Pedestal_Key", PrimitiveType.Cylinder, rootEnv.transform,
                new Vector3(0f, 0.5f, 0f), new Vector3(0.8f, 0.5f, 0.8f), matDarkProp);

            GameObject keyObj = CreatePrimitive("Key_Brass_Item", PrimitiveType.Cube, rootEnv.transform,
                new Vector3(0f, 1.25f, 0f), new Vector3(0.2f, 0.2f, 0.4f), matKey);
            keyObj.AddComponent<WorldItem>();

            // Escritorio con Documento (Pista de Isadora)
            GameObject desk = CreatePrimitive("Desk_Document", PrimitiveType.Cube, rootEnv.transform,
                new Vector3(-4f, 0.5f, -3f), new Vector3(2f, 1f, 1.2f), matDarkProp);

            GameObject docObj = CreatePrimitive("Doc_Isadora_Item", PrimitiveType.Cube, rootEnv.transform,
                new Vector3(-4f, 1.05f, -3f), new Vector3(0.4f, 0.05f, 0.5f), matWall);
            docObj.AddComponent<WorldDocument>();

            // Puerta 1 (Recepción -> Pasillo)
            WorldDoor door1 = CreateInteractiveDoor("Door_Reception_To_Rehearsal", rootEnv.transform,
                new Vector3(-1f, 0f, 6f), matDoor1, "key_brass_rehearsal", true);

            // ==========================================
            // PASILLO CONECTOR (Z: 6 a 14)
            // ==========================================
            CreatePrimitive("Floor_Corridor", PrimitiveType.Cube, rootEnv.transform,
                new Vector3(0f, -0.25f, 10f), new Vector3(4f, 0.5f, 8f), matFloor);
            CreateWall("Wall_Corridor_Left", rootEnv.transform, new Vector3(-2f, 2f, 10f), new Vector3(0.4f, 4f, 8f), matWall);
            CreateWall("Wall_Corridor_Right", rootEnv.transform, new Vector3(2f, 2f, 10f), new Vector3(0.4f, 4f, 8f), matWall);
            CreatePointLight("Light_Corridor", rootEnv.transform, new Vector3(0f, 3.2f, 10f), new Color(0.6f, 0.75f, 0.9f), 10f, 1.2f);

            // ==========================================
            // SALA 2: SALA DE ENSAYO DE BALLET (Z: 14 a 30)
            // ==========================================
            CreatePrimitive("Floor_Rehearsal", PrimitiveType.Cube, rootEnv.transform,
                new Vector3(0f, -0.25f, 22f), new Vector3(16f, 0.5f, 16f), matParquet);

            // Paredes Sala 2
            CreateWall("Wall_Reh_Back_L", rootEnv.transform, new Vector3(-4.5f, 2f, 14f), new Vector3(7f, 4f, 0.4f), matWall);
            CreateWall("Wall_Reh_Back_R", rootEnv.transform, new Vector3(4.5f, 2f, 14f), new Vector3(7f, 4f, 0.4f), matWall);
            CreateWall("Wall_Reh_Back_Top", rootEnv.transform, new Vector3(0f, 3.25f, 14f), new Vector3(2f, 1.5f, 0.4f), matWall);

            CreateWall("Wall_Reh_Left", rootEnv.transform, new Vector3(-8f, 2f, 22f), new Vector3(0.4f, 4f, 16f), matWall);
            CreateWall("Wall_Reh_Right", rootEnv.transform, new Vector3(8f, 2f, 22f), new Vector3(0.4f, 4f, 16f), matWall);

            CreateWall("Wall_Reh_Front_L", rootEnv.transform, new Vector3(-4.5f, 2f, 30f), new Vector3(7f, 4f, 0.4f), matWall);
            CreateWall("Wall_Reh_Front_R", rootEnv.transform, new Vector3(4.5f, 2f, 30f), new Vector3(7f, 4f, 0.4f), matWall);
            CreateWall("Wall_Reh_Front_Top", rootEnv.transform, new Vector3(0f, 3.25f, 30f), new Vector3(2f, 1.5f, 0.4f), matWall);

            // Luz de ambiente Sala 2
            CreatePointLight("Light_Rehearsal_Main", rootEnv.transform, new Vector3(0f, 3.5f, 22f), new Color(1f, 0.95f, 0.85f), 18f, 1.5f);

            // Puerta 2 (Sala de Ensayo -> Gran Teatro) - Bloqueada por el Puzle
            WorldDoor door2 = CreateInteractiveDoor("Door_Rehearsal_To_Stage", rootEnv.transform,
                new Vector3(-1f, 0f, 30f), matDoor2, "", true);

            // ==========================================
            // PUZLE MECÁNICO DE PILARES DE BALLET
            // ==========================================
            GameObject puzzleRoot = new GameObject("Puzzle_BalletPedestals");
            puzzleRoot.transform.parent = rootEnv.transform;
            puzzleRoot.transform.position = new Vector3(0f, 0f, 22f);

            // Luz de Feedback sobre el puzle
            GameObject feedbackLightObj = new GameObject("Light_PuzzleFeedback");
            feedbackLightObj.transform.parent = puzzleRoot.transform;
            feedbackLightObj.transform.position = new Vector3(0f, 3f, 0f);
            Light feedbackLight = feedbackLightObj.AddComponent<Light>();
            feedbackLight.type = LightType.Point;
            feedbackLight.range = 10f;
            feedbackLight.intensity = 1.8f;
            feedbackLight.color = new Color(0.85f, 0.2f, 0.15f); // Rojo inicialmente

            // 3 Pedestales Giratorios
            PedestalDial dial1 = CreatePedestalDial("Pilar_1_Alba", puzzleRoot.transform, new Vector3(-3f, 0f, 0f), matDarkProp, matIndicator, "Pilar del Alba");
            PedestalDial dial2 = CreatePedestalDial("Pilar_2_Sombra", puzzleRoot.transform, new Vector3(0f, 0f, 0f), matDarkProp, matIndicator, "Pilar de la Sombra");
            PedestalDial dial3 = CreatePedestalDial("Pilar_3_Escenario", puzzleRoot.transform, new Vector3(3f, 0f, 0f), matDarkProp, matIndicator, "Pilar del Escenario");

            // Palanca de Reinicio (evita softlocks)
            GameObject leverObj = CreatePrimitive("Lever_Reset_Puzzle", PrimitiveType.Cube, puzzleRoot.transform,
                new Vector3(-6.5f, 1.2f, 0f), new Vector3(0.3f, 0.8f, 0.3f), matDoor1);
            PuzzleResetLever resetLever = leverObj.AddComponent<PuzzleResetLever>();

            // Controlador del Puzle
            PedestalPuzzle puzzleComp = puzzleRoot.AddComponent<PedestalPuzzle>();
            SerializedObject serPuzzle = new SerializedObject(puzzleComp);
            serPuzzle.FindProperty("puzzleId").stringValue = "puz_ballet_pillars";
            serPuzzle.FindProperty("doorToUnlock").objectReferenceValue = door2;
            serPuzzle.FindProperty("feedbackLight").objectReferenceValue = feedbackLight;

            SerializedProperty dialsProp = serPuzzle.FindProperty("dials");
            dialsProp.arraySize = 3;
            dialsProp.GetArrayElementAtIndex(0).objectReferenceValue = dial1;
            dialsProp.GetArrayElementAtIndex(1).objectReferenceValue = dial2;
            dialsProp.GetArrayElementAtIndex(2).objectReferenceValue = dial3;

            // Combinación correcta según la nota:
            // 1. Alba = Cisne Blanco (Paso 0)
            // 2. Sombra = Cisne Negro (Paso 3)
            // 3. Escenario = Telón Teatral (Paso 1)
            SerializedProperty combProp = serPuzzle.FindProperty("targetCombination");
            combProp.arraySize = 3;
            combProp.GetArrayElementAtIndex(0).intValue = 0;
            combProp.GetArrayElementAtIndex(1).intValue = 3;
            combProp.GetArrayElementAtIndex(2).intValue = 1;

            serPuzzle.ApplyModifiedProperties();

            // Asignar puzle a la palanca
            SerializedObject serLever = new SerializedObject(resetLever);
            serLever.FindProperty("targetPuzzle").objectReferenceValue = puzzleComp;
            serLever.ApplyModifiedProperties();

            // ==========================================
            // JUGADOR (FIRST PERSON)
            // ==========================================
            GameObject player = new GameObject("Player");
            player.transform.position = new Vector3(0f, 0f, -3f);
            player.tag = "Player";

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            PlayerMovement movement = player.AddComponent<PlayerMovement>();
            SimpleInventory inventory = player.AddComponent<SimpleInventory>();

            GameObject cameraObj = new GameObject("FirstPersonCamera");
            cameraObj.transform.parent = player.transform;
            cameraObj.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            cameraObj.tag = "MainCamera";

            Camera cam = cameraObj.AddComponent<Camera>();
            cam.nearClipPlane = 0.1f;
            cameraObj.AddComponent<AudioListener>();

            PlayerCameraLook cameraLook = player.AddComponent<PlayerCameraLook>();
            PlayerInteraction interaction = player.AddComponent<PlayerInteraction>();

            // ==========================================
            // UI CANVAS (RETÍCULA, PROMPTS Y DOCUMENT VIEWER)
            // ==========================================
            GameObject canvasObj = new GameObject("UI_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // Retícula central
            GameObject reticle = new GameObject("Reticle");
            reticle.transform.parent = canvasObj.transform;
            Image reticleImage = reticle.AddComponent<Image>();
            reticleImage.color = new Color(1f, 1f, 1f, 0.6f);
            RectTransform reticleRect = reticle.GetComponent<RectTransform>();
            reticleRect.anchoredPosition = Vector2.zero;
            reticleRect.sizeDelta = new Vector2(8f, 8f);

            // Texto Prompt Interacción
            GameObject promptObj = new GameObject("PromptText");
            promptObj.transform.parent = canvasObj.transform;
            TextMeshProUGUI promptText = promptObj.AddComponent<TextMeshProUGUI>();
            promptText.text = "[E] Interactuar";
            promptText.fontSize = 28;
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.color = Color.white;
            RectTransform promptRect = promptObj.GetComponent<RectTransform>();
            promptRect.anchoredPosition = new Vector2(0f, -60f);
            promptRect.sizeDelta = new Vector2(800f, 50f);

            InteractionUI interactionUI = canvasObj.AddComponent<InteractionUI>();
            SerializedObject serializedUI = new SerializedObject(interactionUI);
            serializedUI.FindProperty("playerInteraction").objectReferenceValue = interaction;
            serializedUI.FindProperty("promptTMP").objectReferenceValue = promptText;
            serializedUI.FindProperty("crosshairImage").objectReferenceValue = reticleImage;
            serializedUI.ApplyModifiedProperties();

            // Panel de Lectura de Documentos
            CreateDocumentUIPanel(canvasObj);

            // Guardar y refrescar
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=green>[Último Acto]</color> ¡Escena ampliada con Puzle generada en {ScenePath}!");
            EditorUtility.DisplayDialog("Último Acto - Prototipo Ampliado",
                "¡Escena ampliada creada con éxito!\n\n" +
                "1. Recepción con Llave inicial y Documento/Pista de Isadora Vale.\n" +
                "2. Puerta Vino Tinto hacia el Pasillo.\n" +
                "3. Sala de Ensayo de Ballet con el Puzle de los 3 Pilares y Palanca de reinicio.\n" +
                "4. Puerta del Escenario desbloqueable al resolver la combinación correcta.\n\n" +
                "Presiona 'Play' para jugar.", "¡A probar!");
        }

        private static void CreateDocumentUIPanel(GameObject canvasObj)
        {
            GameObject docPanel = new GameObject("DocumentViewer_Panel");
            docPanel.transform.parent = canvasObj.transform;
            RectTransform panelRect = docPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Fondo oscuro semitransparente
            Image bg = docPanel.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.08f, 0.92f);

            // Contenedor de la carta
            GameObject card = new GameObject("Card");
            card.transform.parent = docPanel.transform;
            RectTransform cardRect = card.AddComponent<RectTransform>();
            cardRect.anchoredPosition = Vector2.zero;
            cardRect.sizeDelta = new Vector2(850f, 600f);
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.12f, 0.11f, 0.12f, 0.98f);

            // Título
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.parent = card.transform;
            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.fontSize = 32;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = new Color(0.95f, 0.8f, 0.35f);
            RectTransform tRect = titleObj.GetComponent<RectTransform>();
            tRect.anchoredPosition = new Vector2(0f, 240f);
            tRect.sizeDelta = new Vector2(800f, 50f);

            // Subtítulo
            GameObject subObj = new GameObject("SubtitleText");
            subObj.transform.parent = card.transform;
            TextMeshProUGUI subTMP = subObj.AddComponent<TextMeshProUGUI>();
            subTMP.fontSize = 18;
            subTMP.alignment = TextAlignmentOptions.Center;
            subTMP.color = new Color(0.7f, 0.7f, 0.7f);
            RectTransform sRect = subObj.GetComponent<RectTransform>();
            sRect.anchoredPosition = new Vector2(0f, 205f);
            sRect.sizeDelta = new Vector2(800f, 30f);

            // Cuerpo
            GameObject bodyObj = new GameObject("BodyText");
            bodyObj.transform.parent = card.transform;
            TextMeshProUGUI bodyTMP = bodyObj.AddComponent<TextMeshProUGUI>();
            bodyTMP.fontSize = 22;
            bodyTMP.lineSpacing = 15;
            bodyTMP.color = new Color(0.92f, 0.9f, 0.88f);
            RectTransform bRect = bodyObj.GetComponent<RectTransform>();
            bRect.anchoredPosition = new Vector2(0f, 0f);
            bRect.sizeDelta = new Vector2(750f, 340f);

            // Hint cerrar
            GameObject hintObj = new GameObject("CloseHintText");
            hintObj.transform.parent = card.transform;
            TextMeshProUGUI hintTMP = hintObj.AddComponent<TextMeshProUGUI>();
            hintTMP.fontSize = 18;
            hintTMP.alignment = TextAlignmentOptions.Center;
            hintTMP.color = new Color(0.6f, 0.6f, 0.6f);
            RectTransform hRect = hintObj.GetComponent<RectTransform>();
            hRect.anchoredPosition = new Vector2(0f, -250f);
            hRect.sizeDelta = new Vector2(800f, 40f);

            DocumentUI docUI = canvasObj.AddComponent<DocumentUI>();
            SerializedObject serUI = new SerializedObject(docUI);
            serUI.FindProperty("documentPanel").objectReferenceValue = docPanel;
            serUI.FindProperty("titleText").objectReferenceValue = titleTMP;
            serUI.FindProperty("subtitleText").objectReferenceValue = subTMP;
            serUI.FindProperty("bodyText").objectReferenceValue = bodyTMP;
            serUI.FindProperty("closeHintText").objectReferenceValue = hintTMP;
            serUI.ApplyModifiedProperties();

            docPanel.SetActive(false);
        }

        private static PedestalDial CreatePedestalDial(string name, Transform parent, Vector3 localPos, Material baseMat, Material indicatorMat, string displayName)
        {
            GameObject dialRoot = new GameObject(name);
            dialRoot.transform.parent = parent;
            dialRoot.transform.localPosition = localPos;

            // Base estática
            GameObject baseCol = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseCol.name = "DialBase";
            baseCol.transform.parent = dialRoot.transform;
            baseCol.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            baseCol.transform.localScale = new Vector3(1.2f, 0.5f, 1.2f);
            baseCol.GetComponent<Renderer>().sharedMaterial = baseMat;

            // Parte giratoria superior
            GameObject rotatingHead = new GameObject("RotatingHead");
            rotatingHead.transform.parent = dialRoot.transform;
            rotatingHead.transform.localPosition = new Vector3(0f, 1f, 0f);

            // Cubo central del dial
            GameObject headCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            headCube.name = "HeadCube";
            headCube.transform.parent = rotatingHead.transform;
            headCube.transform.localPosition = Vector3.zero;
            headCube.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            headCube.GetComponent<Renderer>().sharedMaterial = baseMat;

            // Puntero / Flecha de dirección (apunta hacia Z+)
            GameObject pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pointer.name = "DirectionPointer";
            pointer.transform.parent = rotatingHead.transform;
            pointer.transform.localPosition = new Vector3(0f, 0.2f, 0.5f);
            pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.4f);
            pointer.GetComponent<Renderer>().sharedMaterial = indicatorMat;

            PedestalDial dialComp = dialRoot.AddComponent<PedestalDial>();
            SerializedObject serDial = new SerializedObject(dialComp);
            serDial.FindProperty("dialId").stringValue = name.ToLower();
            serDial.FindProperty("pedestalName").stringValue = displayName;
            serDial.FindProperty("rotatingPart").objectReferenceValue = rotatingHead.transform;
            serDial.ApplyModifiedProperties();

            return dialComp;
        }

        private static WorldDoor CreateInteractiveDoor(string name, Transform parent, Vector3 hingePos, Material mat, string keyId, bool locked)
        {
            GameObject doorFrame = new GameObject(name);
            doorFrame.transform.parent = parent;
            doorFrame.transform.position = hingePos;

            GameObject doorMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorMesh.name = "DoorMesh";
            doorMesh.transform.parent = doorFrame.transform;
            doorMesh.transform.localPosition = new Vector3(1f, 1.25f, 0f);
            doorMesh.transform.localScale = new Vector3(2f, 2.5f, 0.15f);
            doorMesh.GetComponent<Renderer>().sharedMaterial = mat;

            WorldDoor door = doorFrame.AddComponent<WorldDoor>();
            door.SetLockState(locked, keyId);
            return door;
        }

        private static GameObject CreatePrimitive(string name, PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.parent = parent;
            obj.transform.position = pos;
            obj.transform.localScale = scale;
            if (mat != null) obj.GetComponent<Renderer>().sharedMaterial = mat;
            return obj;
        }

        private static GameObject CreateWall(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            return CreatePrimitive(name, PrimitiveType.Cube, parent, pos, scale, mat);
        }

        private static GameObject CreatePointLight(string name, Transform parent, Vector3 pos, Color color, float range, float intensity)
        {
            GameObject lightObj = new GameObject(name);
            lightObj.transform.parent = parent;
            lightObj.transform.position = pos;
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.range = range;
            l.intensity = intensity;
            return lightObj;
        }

        private static Material GetOrCreateMaterial(string name, Color color)
        {
            string fullPath = $"{MaterialsPath}{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(fullPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                mat = new Material(shader);
                mat.color = color;
                AssetDatabase.CreateAsset(mat, fullPath);
            }
            return mat;
        }
    }
}
#endif
