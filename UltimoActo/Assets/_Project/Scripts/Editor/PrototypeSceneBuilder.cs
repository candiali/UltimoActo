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
            // Crear o abrir nueva escena
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Asegurar directorio de materiales
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Materials"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Materials");
            }
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            }

            // 1. Crear Materiales Greybox
            Material matFloor = GetOrCreateMaterial("Mat_Floor", new Color(0.2f, 0.2f, 0.22f));
            Material matWall = GetOrCreateMaterial("Mat_Wall", new Color(0.85f, 0.83f, 0.8f));
            Material matDoor = GetOrCreateMaterial("Mat_Door", new Color(0.45f, 0.1f, 0.15f)); // Rojo vino
            Material matKey = GetOrCreateMaterial("Mat_Key", new Color(0.95f, 0.75f, 0.2f));  // Dorado
            Material matPedestal = GetOrCreateMaterial("Mat_Pedestal", new Color(0.15f, 0.15f, 0.15f));

            // 2. Iluminación
            GameObject dirLight = new GameObject("Directional Light");
            Light lightComp = dirLight.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            lightComp.color = new Color(0.8f, 0.85f, 0.95f);
            lightComp.intensity = 0.5f;
            dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject roomLight = new GameObject("Room Light");
            Light pointLight = roomLight.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = new Color(1f, 0.9f, 0.75f);
            pointLight.range = 15f;
            pointLight.intensity = 2f;
            roomLight.transform.position = new Vector3(0f, 3f, 0f);

            // 3. Habitación (Greybox 10x10x4)
            GameObject roomRoot = new GameObject("Environment_Greybox");

            // Suelo
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.parent = roomRoot.transform;
            floor.transform.position = new Vector3(0f, -0.25f, 0f);
            floor.transform.localScale = new Vector3(12f, 0.5f, 12f);
            floor.GetComponent<Renderer>().sharedMaterial = matFloor;

            // Pared Trasera (Z = -6)
            CreateWall("Wall_Back", roomRoot.transform, new Vector3(0f, 2f, -6f), new Vector3(12f, 4f, 0.4f), matWall);

            // Pared Izquierda (X = -6)
            CreateWall("Wall_Left", roomRoot.transform, new Vector3(-6f, 2f, 0f), new Vector3(0.4f, 4f, 12f), matWall);

            // Pared Derecha (X = 6)
            CreateWall("Wall_Right", roomRoot.transform, new Vector3(6f, 2f, 0f), new Vector3(0.4f, 4f, 12f), matWall);

            // Pared Frontal con hueco de puerta (Z = 6)
            CreateWall("Wall_Front_Left", roomRoot.transform, new Vector3(-3.5f, 2f, 6f), new Vector3(5f, 4f, 0.4f), matWall);
            CreateWall("Wall_Front_Right", roomRoot.transform, new Vector3(3.5f, 2f, 6f), new Vector3(5f, 4f, 0.4f), matWall);
            CreateWall("Wall_Front_Top", roomRoot.transform, new Vector3(0f, 3.25f, 6f), new Vector3(2f, 1.5f, 0.4f), matWall);

            // Habitación exterior / pasillo para ver que la puerta da a algún lado
            GameObject hallwayFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hallwayFloor.name = "Hallway_Floor";
            hallwayFloor.transform.parent = roomRoot.transform;
            hallwayFloor.transform.position = new Vector3(0f, -0.25f, 10f);
            hallwayFloor.transform.localScale = new Vector3(4f, 0.5f, 8f);
            hallwayFloor.GetComponent<Renderer>().sharedMaterial = matFloor;

            // 4. Puerta Interactuable
            GameObject doorFrame = new GameObject("Interactive_Door");
            doorFrame.transform.parent = roomRoot.transform;
            doorFrame.transform.position = new Vector3(-1f, 0f, 6f); // Posición de la bisagra

            GameObject doorMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorMesh.name = "DoorMesh";
            doorMesh.transform.parent = doorFrame.transform;
            doorMesh.transform.localPosition = new Vector3(1f, 1.25f, 0f); // Desplazado respecto a la bisagra
            doorMesh.transform.localScale = new Vector3(2f, 2.5f, 0.15f);
            doorMesh.GetComponent<Renderer>().sharedMaterial = matDoor;

            WorldDoor doorComponent = doorFrame.AddComponent<WorldDoor>();
            doorComponent.SetLockState(true, "key_brass_rehearsal");

            // 5. Pedestal y Llave Interactuable
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Pedestal";
            pedestal.transform.parent = roomRoot.transform;
            pedestal.transform.position = new Vector3(0f, 0.5f, 0f);
            pedestal.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);
            pedestal.GetComponent<Renderer>().sharedMaterial = matPedestal;

            GameObject keyItem = GameObject.CreatePrimitive(PrimitiveType.Cube);
            keyItem.name = "Key_Brass_Item";
            keyItem.transform.parent = roomRoot.transform;
            keyItem.transform.position = new Vector3(0f, 1.25f, 0f);
            keyItem.transform.localScale = new Vector3(0.2f, 0.2f, 0.4f);
            keyItem.GetComponent<Renderer>().sharedMaterial = matKey;

            WorldItem itemComponent = keyItem.AddComponent<WorldItem>();

            // 6. Jugador
            GameObject player = new GameObject("Player");
            player.transform.position = new Vector3(0f, 0f, -3f);
            player.tag = "Player";

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            PlayerMovement movement = player.AddComponent<PlayerMovement>();
            SimpleInventory inventory = player.AddComponent<SimpleInventory>();

            // Cámara del jugador
            GameObject cameraObj = new GameObject("FirstPersonCamera");
            cameraObj.transform.parent = player.transform;
            cameraObj.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            cameraObj.tag = "MainCamera";

            Camera cam = cameraObj.AddComponent<Camera>();
            cam.nearClipPlane = 0.1f;
            cameraObj.AddComponent<AudioListener>();

            PlayerCameraLook cameraLook = player.AddComponent<PlayerCameraLook>();
            PlayerInteraction interaction = player.AddComponent<PlayerInteraction>();

            // 7. UI Canvas (Retícula y Prompt)
            GameObject canvasObj = new GameObject("UI_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // Retícula (punto central)
            GameObject reticle = new GameObject("Reticle");
            reticle.transform.parent = canvasObj.transform;
            Image reticleImage = reticle.AddComponent<Image>();
            reticleImage.color = new Color(1f, 1f, 1f, 0.6f);
            RectTransform reticleRect = reticle.GetComponent<RectTransform>();
            reticleRect.anchoredPosition = Vector2.zero;
            reticleRect.sizeDelta = new Vector2(8f, 8f);

            // Prompt Text (TMP)
            GameObject promptObj = new GameObject("PromptText");
            promptObj.transform.parent = canvasObj.transform;
            TextMeshProUGUI promptText = promptObj.AddComponent<TextMeshProUGUI>();
            promptText.text = "[E] Interactuar";
            promptText.fontSize = 28;
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.color = Color.white;
            RectTransform promptRect = promptObj.GetComponent<RectTransform>();
            promptRect.anchoredPosition = new Vector2(0f, -60f);
            promptRect.sizeDelta = new Vector2(600f, 50f);

            // Script InteractionUI
            InteractionUI interactionUI = canvasObj.AddComponent<InteractionUI>();
            SerializedObject serializedUI = new SerializedObject(interactionUI);
            serializedUI.FindProperty("playerInteraction").objectReferenceValue = interaction;
            serializedUI.FindProperty("promptTMP").objectReferenceValue = promptText;
            serializedUI.FindProperty("crosshairImage").objectReferenceValue = reticleImage;
            serializedUI.ApplyModifiedProperties();

            // Guardar la escena
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=green>[Último Acto]</color> ¡Escena Prototipo creada con éxito en {ScenePath}!");
            EditorUtility.DisplayDialog("Último Acto - Fase 1", 
                "¡Sala Prototipo creada con éxito!\n\n1. Ya tienes el Jugador en 1ra persona con Cámara e Input.\n2. Llave dorada sobre el pedestal.\n3. Puerta cerrada con llave que conduce al pasillo.\n\nPresiona 'Play' para probar el prototipo.", "¡Entendido!");
        }

        private static GameObject CreateWall(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.parent = parent;
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().sharedMaterial = mat;
            return wall;
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
