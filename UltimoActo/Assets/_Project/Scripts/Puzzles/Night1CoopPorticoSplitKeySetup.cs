using UnityEngine;
using UnityEngine.SceneManagement;
using UltimoActo.Core;
using UltimoActo.Interaction;
using UltimoActo.Player;

namespace UltimoActo.Puzzles
{
    /// <summary>
    /// BLOQUE B · Puzle Cooperativo de Ejemplo — Noche I ("El Cerrojo Dividido del Pórtico Imperial"):
    /// - Enseña la mecánica de cooperación entre épocas (Tipo 4: Dos mitades de una clave + Tipo 1: EraOnlyVisibility)
    ///   justo en el Pórtico de Entrada de la Academia (Zona 3, frente a las Grandes Puertas Dobles del Vestíbulo):
    ///   1. En la Época 1929 (Columna Oeste del Pórtico): Brilla el Emblema de Fundación de 1929
    ///      (SOLO VISIBLE EN 1929) que revela la Primera Mitad del Sello: «I — Cisne Dorado».
    ///   2. En la Época Ruinas (Columna Este del Pórtico): Entre la piedra agrietada aparece el Grabado de Clausura
    ///      (SOLO VISIBLE EN RUINAS) que revela la Segunda Mitad del Sello: «III — Lira de Bronce».
    ///   3. En el Pedestal Central junto a las puertas dobles (`Grand_Entrance_Doors`) se alza el Cilindro de
    ///      Doble Tambor Imperial (visible en ambas épocas). Al alinear el Tambor Izquierdo en «I — Cisne Dorado»
    ///      y el Tambor Derecho en «III — Lira de Bronce», las puertas monumentales se desbloquean y se abren,
    ///      registrando las pistas, la solución y el logro «Voz en la oscuridad» en el Diario Compartido ([J]/[TAB]).
    /// - Funciona tanto con 2 jugadores por voz como en Modo Solo alternando con [Q].
    /// </summary>
    public class Night1CoopPorticoSplitKeySetup : MonoBehaviour
    {
        public static Night1CoopPorticoSplitKeySetup Instance { get; private set; }

        private CoopSplitKeyLockPuzzle porticoSplitPuzzle;

        public CoopSplitKeyLockPuzzle PorticoPuzzle => porticoSplitPuzzle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoadedBootstrap;
            SceneManager.sceneLoaded += OnSceneLoadedBootstrap;
            TrySpawnInActiveScene();
        }

        private static void OnSceneLoadedBootstrap(Scene scene, LoadSceneMode mode)
        {
            TrySpawnInActiveScene();
        }

        private static void TrySpawnInActiveScene()
        {
            if (Instance != null) return;
            if (FindFirstObjectByType<Night1CoopPorticoSplitKeySetup>() != null) return;
            if (FindFirstObjectByType<PlayerMovement>() == null) return;

            GameObject go = new GameObject("Night1_CoopPuzzle_PorticoSplitKey");
            go.AddComponent<Night1CoopPorticoSplitKeySetup>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            BuildNight1PorticoCoopPuzzle();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void BuildNight1PorticoCoopPuzzle()
        {
            float porticoY = 2.4f;

            // Localizar las Grandes Puertas Dobles de la Entrada Principal (Z = 8.0m, Y = 2.4m)
            WorldDoubleDoor grandEntranceDoors = null;
            GameObject entranceObj = GameObject.Find("Grand_Entrance_Doors");
            if (entranceObj != null)
            {
                grandEntranceDoors = entranceObj.GetComponent<WorldDoubleDoor>();
            }

            if (grandEntranceDoors != null)
            {
                // Bloqueamos inicialmente la puerta principal con el cerrojo cooperativo de dos mitades
                grandEntranceDoors.SetLockState(true, "coop_split_key_portico_noche1");
            }

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            Material matBrass1929 = CreateMaterial(litShader, new Color(0.88f, 0.72f, 0.36f), 0.75f, 0.80f, new Color(0.55f, 0.42f, 0.14f));
            Material matStoneRuins = CreateMaterial(litShader, new Color(0.38f, 0.44f, 0.48f), 0.35f, 0.15f, new Color(0.22f, 0.34f, 0.48f));
            Material matPedestalDark = CreateMaterial(litShader, new Color(0.16f, 0.11f, 0.12f), 0.55f, 0.25f, Color.black);
            Material matGoldAccent = CreateMaterial(litShader, new Color(0.85f, 0.68f, 0.32f), 0.80f, 0.85f, new Color(0.35f, 0.26f, 0.08f));

            // 1. Controlador del Puzle Cooperativo Tipo 4 (Clave Dividida)
            porticoSplitPuzzle = gameObject.AddComponent<CoopSplitKeyLockPuzzle>();

            // 2. Pedestal Central del Cerrojo Dividido (en el Pórtico, X = 1.65m, Y = 2.4m, Z = 7.25m, justo a la derecha de la puerta principal)
            GameObject pedestalRoot = new GameObject("Pedestal_Cerrojo_Cooperativo_Noche1");
            pedestalRoot.transform.SetParent(transform, false);
            pedestalRoot.transform.position = new Vector3(1.65f, porticoY, 7.25f);

            CreateVisualPiece("Base_Pedestal", PrimitiveType.Cube, pedestalRoot.transform,
                new Vector3(0f, 0.55f, 0f), new Vector3(0.75f, 1.10f, 0.52f), matPedestalDark, false);

            CreateVisualPiece("Marco_Dorado_Superior", PrimitiveType.Cube, pedestalRoot.transform,
                new Vector3(0f, 1.22f, 0f), new Vector3(0.82f, 0.48f, 0.56f), matGoldAccent, false);

            // Luz indicadora sobre el pedestal
            GameObject lightObj = new GameObject("LockStatus_Light");
            lightObj.transform.SetParent(pedestalRoot.transform, false);
            lightObj.transform.localPosition = new Vector3(0f, 1.62f, -0.25f);
            Light statusLight = lightObj.AddComponent<Light>();
            statusLight.type = LightType.Point;
            statusLight.range = 4.5f;
            statusLight.intensity = 1.8f;
            statusLight.color = new Color(0.92f, 0.68f, 0.28f);

            // Tambor Izquierdo (Mitad A — 1929)
            GameObject leftDialObj = CreateVisualPiece("Tambor_Izquierdo_1929", PrimitiveType.Cylinder, pedestalRoot.transform,
                new Vector3(-0.20f, 1.24f, -0.28f), new Vector3(0.26f, 0.06f, 0.26f), matBrass1929, true);
            leftDialObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            CreateVisualPiece("Aguja_Izquierda", PrimitiveType.Cube, leftDialObj.transform,
                new Vector3(0f, 1.1f, 0.22f), new Vector3(0.12f, 0.4f, 0.45f), matPedestalDark, false);

            // Tambor Derecho (Mitad B — Ruinas)
            GameObject rightDialObj = CreateVisualPiece("Tambor_Derecho_Ruinas", PrimitiveType.Cylinder, pedestalRoot.transform,
                new Vector3(0.20f, 1.24f, -0.28f), new Vector3(0.26f, 0.06f, 0.26f), matStoneRuins, true);
            rightDialObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            CreateVisualPiece("Aguja_Derecha", PrimitiveType.Cube, rightDialObj.transform,
                new Vector3(0f, 1.1f, 0.22f), new Vector3(0.12f, 0.4f, 0.45f), matPedestalDark, false);

            string[] symbols =
            {
                "I — Cisne Dorado",
                "II — Rosa de Marfil",
                "III — Lira de Bronce",
                "IV — Máscara de Ópera"
            };

            porticoSplitPuzzle.ConfigureSplitLock(
                id: "noche1_portico_split_key",
                displayTitle: "Noche I — Cerrojo Dividido del Pórtico Imperial",
                options: symbols,
                initialLeft: 1,  // II — Rosa de Marfil
                initialRight: 3, // IV — Máscara de Ópera
                correctLeft: 0,  // I — Cisne Dorado (Mitad A en 1929)
                correctRight: 2, // III — Lira de Bronce (Mitad B en Ruinas)
                leftVisual: leftDialObj.transform,
                rightVisual: rightDialObj.transform,
                indicatorLight: statusLight,
                singleDoor: null,
                doubleDoor: grandEntranceDoors);

            CoopSplitKeyDialNode leftNode = leftDialObj.AddComponent<CoopSplitKeyDialNode>();
            leftNode.Configure(porticoSplitPuzzle, false, "Tambor Izquierdo (Mitad A · 1929)", EraVisibilityMode.Both);

            CoopSplitKeyDialNode rightNode = rightDialObj.AddComponent<CoopSplitKeyDialNode>();
            rightNode.Configure(porticoSplitPuzzle, true, "Tambor Derecho (Mitad B · Ruinas)", EraVisibilityMode.Both);

            // 3. PISTA MITAD A — SOLO VISIBLE EN ÉPOCA 1929 (Columna Oeste del Pórtico: X = -2.0m, Y = 3.85m, Z = 3.65m)
            GameObject plaque1929Obj = CreateVisualPiece("Emblema_MitadA_Solo1929", PrimitiveType.Cube, transform,
                new Vector3(-2.0f, porticoY + 1.45f, 3.62f), new Vector3(0.68f, 0.82f, 0.10f), matBrass1929, true);

            GameObject light1929Obj = new GameObject("Glow_1929_Clue");
            light1929Obj.transform.SetParent(plaque1929Obj.transform, false);
            light1929Obj.transform.localPosition = new Vector3(0f, 0f, -0.35f);
            Light l1929 = light1929Obj.AddComponent<Light>();
            l1929.type = LightType.Point;
            l1929.range = 3.5f;
            l1929.intensity = 1.6f;
            l1929.color = new Color(0.98f, 0.84f, 0.48f);
            l1929.cullingMask = 1 << EraVisualManager.GetEra1929Layer();

            CoopEraCluePlaque clue1929 = plaque1929Obj.AddComponent<CoopEraCluePlaque>();
            clue1929.Configure(
                puzzle: porticoSplitPuzzle,
                subId: "mitad_a_1929",
                visibilityMode: EraVisibilityMode.Only1929,
                interactPromptTitle: "Blasón Imperial de Inauguración (1.ª Mitad de la Clave)",
                docTitle: "Pista Noche I [SOLO VISIBLE EN 1929] — Blasón Imperial del Pórtico",
                docBody:
                    "BLASÓN DE GALA DE LA ACADEMIA VESPER (AÑO 1929)\n" +
                    "Estado de visibilidad: **SOLO VISIBLE EN LA ÉPOCA 1929**\n\n" +
                    "Sobre la columna occidental del pórtico reluce intacto el escudo de latón dorado instalado para la gala inaugural.\n\n" +
                    "En su inscripción superior se lee claramente la **PRIMERA MITAD DEL CERROJO DE ENTRADA**:\n" +
                    "• **Tambor Izquierdo (Mitad A — 1929): «I — Cisne Dorado»**\n\n" +
                    "*(La segunda mitad del blasón aún no había sido grabada en 1929. Comunica este símbolo por voz a tu compañero en las Ruinas, o pulsa [Q] si juegas en solitario, para buscar la segunda mitad en la columna oriental).*");

            // 4. PISTA MITAD B — SOLO VISIBLE EN ÉPOCA RUINAS (Columna Este del Pórtico: X = +2.0m, Y = 3.85m, Z = 3.65m)
            GameObject plaqueRuinsObj = CreateVisualPiece("Grabado_MitadB_SoloRuinas", PrimitiveType.Cube, transform,
                new Vector3(2.0f, porticoY + 1.45f, 3.62f), new Vector3(0.68f, 0.82f, 0.10f), matStoneRuins, true);

            GameObject lightRuinsObj = new GameObject("Glow_Ruins_Clue");
            lightRuinsObj.transform.SetParent(plaqueRuinsObj.transform, false);
            lightRuinsObj.transform.localPosition = new Vector3(0f, 0f, -0.35f);
            Light lRuins = lightRuinsObj.AddComponent<Light>();
            lRuins.type = LightType.Point;
            lRuins.range = 3.5f;
            lRuins.intensity = 1.8f;
            lRuins.color = new Color(0.56f, 0.78f, 0.98f);
            lRuins.cullingMask = 1 << EraVisualManager.GetEraRuinsLayer();

            CoopEraCluePlaque clueRuins = plaqueRuinsObj.AddComponent<CoopEraCluePlaque>();
            clueRuins.Configure(
                puzzle: porticoSplitPuzzle,
                subId: "mitad_b_ruinas",
                visibilityMode: EraVisibilityMode.OnlyRuins,
                interactPromptTitle: "Lápida de Clausura Agrietada (2.ª Mitad de la Clave)",
                docTitle: "Pista Noche I [SOLO VISIBLE EN RUINAS] — Lápida de Clausura en la Columna Este",
                docBody:
                    "INSCRIPCIÓN TALLADA TRAS EL ABANDONO DE LA ACADEMIA (ÉPOCA RUINAS)\n" +
                    "Estado de visibilidad: **SOLO VISIBLE EN LA ÉPOCA ACTUAL (RUINAS)**\n\n" +
                    "Bajo la hiedra seca y el yeso desprendido de la columna oriental del pórtico aparece una placa de bronce oxidado fijada décadas después de 1929 para sellar el edificio.\n\n" +
                    "En su relieve se conserva únicamente la **SEGUNDA MITAD DEL CERROJO DE ENTRADA**:\n" +
                    "• **Tambor Derecho (Mitad B — Ruinas): «III — Lira de Bronce»**\n\n" +
                    "*(La primera mitad de la placa está rota y arrancada en las Ruinas. Pregunta a tu compañero de 1929 qué símbolo ve en la columna occidental y alinead ambos tambores en el pedestal junto a la puerta principal).*");

            // Re-indexar en SharedWorldStateSync si ya había arrancado
            if (UltimoActo.Network.SharedWorldStateSync.Instance != null)
            {
                UltimoActo.Network.SharedWorldStateSync.Instance.IndexSceneInteractiveObjects();
            }
        }

        private static Material CreateMaterial(Shader shader, Color albedo, float smoothness, float metallic, Color emission)
        {
            Material m = new Material(shader);
            m.color = albedo;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", albedo);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            if (emission.maxColorComponent > 0.01f)
            {
                m.EnableKeyword("_EMISSION");
                if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", emission);
            }
            return m;
        }

        private static GameObject CreateVisualPiece(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 localPos,
            Vector3 localScale,
            Material mat,
            bool keepCollider)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;

            Renderer rend = go.GetComponent<Renderer>();
            if (rend != null && mat != null) rend.sharedMaterial = mat;

            if (!keepCollider)
            {
                Collider col = go.GetComponent<Collider>();
                if (col != null) Destroy(col);
            }
            return go;
        }
    }
}
