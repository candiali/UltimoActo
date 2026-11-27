using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UltimoActo.Core;
using UltimoActo.UI;

namespace UltimoActo.Narrative
{
    public enum CollectibleItemType
    {
        LettersAndDiaries = 0,       // Cartas y Diarios
        Photographs = 1,             // Fotografías y Placas
        MusicalScores = 2,           // Partituras y Apuntes Musicales
        DancerPersonalItems = 3,     // Objetos Personales de las Bailarinas
        EmblemsAndOptionalKeys = 4,  // Emblemas, Medallas y Llaves Opcionales
        SpecialSecret = 5            // Secretos Especiales (1 a 8)
    }

    [Serializable]
    public class CollectibleDefinition
    {
        public string id;
        public string title;
        public CollectibleItemType type;
        public EraVisibilityMode originEra;
        public int nightIndex;       // 0 = Noche I .. 7 = Noche VIII
        public string nightRoman;    // "I" .. "VIII"
        public int sectionIndex;     // 1 .. 12
        public string sectionName;
        public string zoneName;
        public string description;
        public string secondaryLoreReward;
        public string vagueMissingHint; // Pista vaga mostrada cuando falta, sin marcarla en el mapa
        public bool isSpecialSecret;
    }

    [Serializable]
    public class CollectibleUnlockRecord
    {
        public string id;
        public string foundByPlayer;
        public string nightFoundLabel;
        public string eraFoundLabel;
    }

    [Serializable]
    public class CollectiblesSaveData
    {
        public List<CollectibleUnlockRecord> unlockedItems = new List<CollectibleUnlockRecord>();
        public List<string> secretStepOneDoneIds = new List<string>();
    }

    /// <summary>
    /// BLOQUE 2 (Paso C) — Gestor de Coleccionables Narrativos (32 piezas) y Secretos Especiales (8 secretos):
    /// - Define el catálogo completo repartido por las 8 Noches y las 12 Secciones de la Academia Vesper,
    ///   con su ID, título, tipo, época de origen, noche, sección, zona, descripción, recompensa/historia
    ///   secundaria enlazada a la Biblia Narrativa y pista vaga cuando aún no se ha encontrado.
    /// - Instancia físicamente en la escena las primeras piezas de la Noche I (4 Coleccionables + 1 Secreto
    ///   Especial Cooperativo en la Sala de Tocadores Zona 12).
    /// - Estado 100% compartido entre Host y Cliente con autoridad del Host y persistencia en PlayerPrefs
    ///   ("UltimoActo_Host_Collectibles_V1") sin duplicar recompensas ni coleccionables.
    /// </summary>
    public class CollectibleAndSecretManager : MonoBehaviour
    {
        public const string SavePrefsKey = "UltimoActo_Host_Collectibles_V1";

        public static CollectibleAndSecretManager Instance { get; private set; }

        /// <summary>
        /// Evento local para refrescar la pestaña "Colección" del Diario Compartido y la pantalla del Menú Principal.
        /// </summary>
        public event Action OnCollectionUpdated;

        /// <summary>
        /// Evento de red disparado cuando se desbloquea un coleccionable/secreto o el Paso 1 de un secreto cooperativo.
        /// Formato: "UNLOCK|id|foundBy" o "STEP1|id"
        /// </summary>
        public static event Action<string> OnCollectibleNetworkEvent;

        private readonly List<CollectibleDefinition> catalog = new List<CollectibleDefinition>();
        private readonly Dictionary<string, CollectibleDefinition> catalogById = new Dictionary<string, CollectibleDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, CollectibleUnlockRecord> unlockedById = new Dictionary<string, CollectibleUnlockRecord>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> secretStepOneCompleted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, GameObject> spawnedWorldVisualsById = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyList<CollectibleDefinition> FullCatalog => catalog;
        public int TotalCollectiblesGoal { get; private set; }
        public int TotalSecretsGoal { get; private set; }
        public int UnlockedCollectiblesCount { get; private set; }
        public int UnlockedSecretsCount { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            TryInstallInActiveScene(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryInstallInActiveScene(scene);
        }

        private static void TryInstallInActiveScene(Scene scene)
        {
            if (Instance != null) return;
            GameObject go = new GameObject("CollectibleAndSecretManager");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<CollectibleAndSecretManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            BuildCanonicalCatalog();
            LoadFromHostPrefs();
        }

        private void Start()
        {
            SceneManager.sceneLoaded -= OnGameplaySceneLoaded;
            SceneManager.sceneLoaded += OnGameplaySceneLoaded;

            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                SpawnNight1PhysicalCollectiblesAndSecret();
                SpawnNights2To8PhysicalCollectiblesAndSecrets();
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnGameplaySceneLoaded;
            if (Instance == this) Instance = null;
        }

        private void OnGameplaySceneLoaded(Scene scene, LoadSceneMode mode)
        {
            spawnedWorldVisualsById.Clear();
            LoadFromHostPrefs();
            if (scene.name != "MainMenu")
            {
                SpawnNight1PhysicalCollectiblesAndSecret();
                SpawnNights2To8PhysicalCollectiblesAndSecrets();
            }
        }

        // =========================================================================================
        // CONSULTA Y DESBLOQUEO COMPARTIDO (SIN DUPLICADOS)
        // =========================================================================================

        public CollectibleDefinition GetDefinition(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            catalogById.TryGetValue(id, out CollectibleDefinition def);
            return def;
        }

        public bool IsUnlocked(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            return unlockedById.ContainsKey(id);
        }

        public CollectibleUnlockRecord GetUnlockRecord(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            unlockedById.TryGetValue(id, out CollectibleUnlockRecord rec);
            return rec;
        }

        public bool IsSecretStepOneDone(string secretId)
        {
            if (string.IsNullOrEmpty(secretId)) return false;
            return secretStepOneCompleted.Contains(secretId);
        }

        public void MarkSecretStepOneDone(string secretId, bool broadcastToNetwork)
        {
            if (string.IsNullOrEmpty(secretId)) return;
            if (secretStepOneCompleted.Add(secretId))
            {
                SaveToHostPrefsIfAuthoritative();
                if (broadcastToNetwork)
                {
                    OnCollectibleNetworkEvent?.Invoke($"STEP1|{secretId}");
                }
                if (NightManager.Instance != null)
                {
                    NightManager.Instance.ShowToast("[Secreto Cooperativo 1/2] Pista de 1929 activada — Avísale por voz a tu compañero en las Ruinas.", 4.2f);
                }
            }
        }

        public void UnlockCollectibleOrSecret(string id, bool showViewerAndToast, bool broadcastToNetwork, string remoteFinderName = null)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (!catalogById.TryGetValue(id, out CollectibleDefinition def) || def == null) return;

            // Evitar duplicar recompensas o coleccionables ya desbloqueados
            if (unlockedById.ContainsKey(id)) return;

            string finder = !string.IsNullOrEmpty(remoteFinderName)
                ? remoteFinderName
                : GetLocalPlayerDisplayName();

            string nightLabel = $"Noche {def.nightRoman}";
            string eraLabel = def.originEra == EraVisibilityMode.Only1929
                ? "Época 1929 (Gloria)"
                : (def.originEra == EraVisibilityMode.OnlyRuins ? "Época Actual (Ruinas)" : "Ambas Épocas");

            CollectibleUnlockRecord record = new CollectibleUnlockRecord
            {
                id = id,
                foundByPlayer = finder,
                nightFoundLabel = nightLabel,
                eraFoundLabel = eraLabel
            };

            unlockedById[id] = record;
            RecountTotals();
            SaveToHostPrefsIfAuthoritative();

            // Ocultar el objeto 3D en el mundo si ya fue recogido
            if (spawnedWorldVisualsById.TryGetValue(id, out GameObject worldProp) && worldProp != null)
            {
                worldProp.SetActive(false);
            }

            // Registrar también en el Diario Compartido de Evidencias y actualizar contadores para el Epílogo
            if (SharedEvidenceJournalManager.Instance != null)
            {
                string prefix = def.isSpecialSecret ? "★ [SECRETO ESPECIAL] " : $"[COLECCIONABLE · {FormatTypeShort(def.type)}] ";
                string fullBody =
                    $"<b>Ubicación:</b> {def.zoneName} ({def.sectionName} · Noche {def.nightRoman})\n" +
                    $"<b>Época de origen:</b> {eraLabel}\n\n" +
                    $"{def.description}\n\n" +
                    $"<b><color=#F3D692>HISTORIA SECUNDARIA / RECOMPENSA NARRATIVA:</color></b>\n{def.secondaryLoreReward}";

                SharedEvidenceJournalManager.Instance.RecordClueOrDocument(
                    entryId: $"collectible_journal_{def.id}",
                    title: prefix + def.title,
                    subtitle: $"{def.sectionName} · Hallado por {finder}",
                    bodyContent: fullBody,
                    category: MapCollectibleTypeToJournalTab(def.type),
                    visibilityRule: def.originEra);
            }

            PlayerPrefs.SetInt(
                UltimateFinaleAndCreditsController.PrefsBestEvidencesKey,
                Mathf.Max(PlayerPrefs.GetInt(UltimateFinaleAndCreditsController.PrefsBestEvidencesKey, 0), UnlockedCollectiblesCount));
            PlayerPrefs.SetInt(
                UltimateFinaleAndCreditsController.PrefsBestSecretsKey,
                Mathf.Max(PlayerPrefs.GetInt(UltimateFinaleAndCreditsController.PrefsBestSecretsKey, 0), UnlockedSecretsCount));
            PlayerPrefs.Save();

            if (broadcastToNetwork)
            {
                OnCollectibleNetworkEvent?.Invoke($"UNLOCK|{id}|{finder}");
            }

            if (showViewerAndToast)
            {
                if (NightManager.Instance != null)
                {
                    string toastMsg = def.isSpecialSecret
                        ? $"★ SECRETO ESPECIAL DESCUBIERTO ({UnlockedSecretsCount}/{TotalSecretsGoal}): «{def.title}»"
                        : $"COLECCIONABLE HALLADO ({UnlockedCollectiblesCount}/{TotalCollectiblesGoal}): «{def.title}»";
                    NightManager.Instance.ShowToast(toastMsg, 4.2f);
                }

                if (DocumentUI.Instance != null)
                {
                    string headerTitle = def.isSpecialSecret
                        ? $"★ SECRETO ESPECIAL ({UnlockedSecretsCount}/{TotalSecretsGoal}) — {def.title}"
                        : $"COLECCIONABLE ({UnlockedCollectiblesCount}/{TotalCollectiblesGoal}) — {def.title}";
                    string sub = $"{FormatTypeShort(def.type)} · {def.sectionName} ({def.zoneName}) · Noche {def.nightRoman}";
                    string body =
                        $"{def.description}\n\n" +
                        "──────────────────────────────────────────────\n" +
                        $"<b>HISTORIA SECUNDARIA DESBLOQUEADA EN COLECCIÓN:</b>\n{def.secondaryLoreReward}";

                    DocumentUI.Instance.ShowDocumentWithId(
                        $"collectible_view_{def.id}",
                        headerTitle,
                        body,
                        sub,
                        skipJournalRecord: true);
                }
            }
            else if (NightManager.Instance != null)
            {
                string toastRemote = def.isSpecialSecret
                    ? $"[Compañero: {finder}] ★ Secreto Especial desbloqueado ({UnlockedSecretsCount}/{TotalSecretsGoal}): «{def.title}»"
                    : $"[Compañero: {finder}] Coleccionable hallado ({UnlockedCollectiblesCount}/{TotalCollectiblesGoal}): «{def.title}»";
                NightManager.Instance.ShowToast(toastRemote, 4.0f);
            }

            OnCollectionUpdated?.Invoke();
        }

        public void ApplyNetworkPacket(string packet)
        {
            if (string.IsNullOrEmpty(packet)) return;
            string[] parts = packet.Split('|');
            if (parts.Length < 2) return;

            if (parts[0] == "STEP1")
            {
                MarkSecretStepOneDone(parts[1], false);
            }
            else if (parts[0] == "UNLOCK")
            {
                string id = parts[1];
                string finder = parts.Length >= 3 ? parts[2] : "Compañero";
                UnlockCollectibleOrSecret(id, false, false, finder);
            }
        }

        public List<string> BuildNetworkSnapshotPackets()
        {
            List<string> list = new List<string>();
            foreach (string s1 in secretStepOneCompleted)
            {
                list.Add($"STEP1|{s1}");
            }
            foreach (var kv in unlockedById)
            {
                list.Add($"UNLOCK|{kv.Key}|{kv.Value.foundByPlayer}");
            }
            return list;
        }

        // =========================================================================================
        // PRIMERAS PIEZAS FÍSICAS EN ESCENA — NOCHE I (4 COLECCIONABLES + 1 SECRETO COOPERATIVO)
        // =========================================================================================

        public void SpawnNight1PhysicalCollectiblesAndSecret()
        {
            float vestY = 2.4f;
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            Material matSilkRibbon1929 = new Material(litShader) { color = new Color(0.18f, 0.10f, 0.16f, 1f) };
            Material matBronzeMedalRuins = new Material(litShader) { color = new Color(0.55f, 0.48f, 0.32f, 1f) };
            Material matScorePaper1929 = new Material(litShader) { color = new Color(0.95f, 0.88f, 0.72f, 1f) };
            Material matPhotoPlateRuins = new Material(litShader) { color = new Color(0.52f, 0.68f, 0.82f, 1f) };
            Material matSecretLatch = new Material(litShader) { color = new Color(0.88f, 0.72f, 0.36f, 1f) };

            // 1. Coleccionable 1 (Solo 1929 — Zona 11 Vestuario Femenino): Cinta de Satén Negro con Iniciales "E. V."
            SpawnCollectibleProp(
                id: "col_n1_ribbon_elena_1929",
                worldPos: new Vector3(16.8f, vestY + 0.92f, 18.2f),
                scale: new Vector3(0.32f, 0.08f, 0.24f),
                mat: matSilkRibbon1929,
                vis: EraVisibilityMode.Only1929);

            // 2. Coleccionable 2 (Solo Ruinas — Zona 6 Recepción Antigua): Medalla Oxidada del Patronato Fundador (1929)
            SpawnCollectibleProp(
                id: "col_n1_emblem_patronato_ruins",
                worldPos: new Vector3(-16.2f, vestY + 1.05f, 16.5f),
                scale: new Vector3(0.26f, 0.08f, 0.26f),
                mat: matBronzeMedalRuins,
                vis: EraVisibilityMode.OnlyRuins);

            // 3. Coleccionable 3 (Solo 1929 — Zona 9 Sala de Espera): Apunte de Preludio en Mi Menor de Julian Krol
            SpawnCollectibleProp(
                id: "col_n1_score_prelude_krol_1929",
                worldPos: new Vector3(-18.5f, vestY + 0.95f, 28.5f),
                scale: new Vector3(0.38f, 0.06f, 0.28f),
                mat: matScorePaper1929,
                vis: EraVisibilityMode.Only1929);

            // 4. Coleccionable 4 (Solo Ruinas — Zona 3/4 Pórtico y Escalera): Negativo Velado de la Marquesina (14 de Octubre de 1929)
            SpawnCollectibleProp(
                id: "col_n1_photo_opening_gala_ruins",
                worldPos: new Vector3(2.85f, vestY + 1.12f, 6.2f),
                scale: new Vector3(0.34f, 0.06f, 0.26f),
                mat: matPhotoPlateRuins,
                vis: EraVisibilityMode.OnlyRuins);

            // 5. SECRETO ESPECIAL 1 DE LA NOCHE I (Cooperativo 1929 -> Ruinas en Zona 12 Sala de Tocadores):
            // "El Susurro tras el Espejo Nº 3"
            if (!IsUnlocked("sec_n1_whispering_mirror_duet"))
            {
                // Nodo Paso 1 (SOLO VISIBLE EN 1929) en el Tocador Nº 3 de Elena (Zona 12: X=23.6, Z=34.4)
                if (GameObject.Find("SecretN1_Step1_1929_Tocador3") == null)
                {
                    GameObject step1Obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    step1Obj.name = "SecretN1_Step1_1929_Tocador3";
                    step1Obj.transform.position = new Vector3(23.6f, vestY + 1.18f, 34.4f);
                    step1Obj.transform.localScale = new Vector3(0.28f, 0.22f, 0.12f);
                    step1Obj.GetComponent<Renderer>().sharedMaterial = matSecretLatch;

                    WorldCollectibleItem itemStep1 = step1Obj.AddComponent<WorldCollectibleItem>();
                    itemStep1.ConfigureCollectible(
                        id: "sec_n1_whispering_mirror_duet",
                        visibilityMode: EraVisibilityMode.Only1929,
                        secretStep1: true,
                        needsStep1First: false,
                        step1Prompt: "Muesca Grabada por Elena en el Tocador Nº 3",
                        step1DocTitle: "Secreto de la Noche I (Parte 1/2 — 1929): La Muesca del Tocador Nº 3",
                        step1DocBody:
                            "En el borde inferior del espejo dorado del Tocador Nº 3 (visible solo en 1929), Elena Vesper grabó con la punta de una horquilla:\n\n" +
                            "«Mara: Si algo me ocurre esta noche, he escondido nuestro juramento detrás del listón derecho de este mismo Tocador Nº 3. Quien pulse el resorte oculto bajo el marco verá lo que Isadora quiso callar».\n\n" +
                            "[COMUNICACIÓN POR VOZ]: Avísale a tu compañero en las Ruinas que ahora puede pulsar el Resorte Oculto del Tocador Nº 3 en la Sala de Tocadores (Zona 12).");
                }

                // Nodo Paso 2 (SOLO VISIBLE EN RUINAS) en el mismo Tocador Nº 3 (Zona 12: X=24.9, Z=34.4)
                SpawnCollectibleProp(
                    id: "sec_n1_whispering_mirror_duet",
                    worldPos: new Vector3(24.9f, vestY + 1.18f, 34.4f),
                    scale: new Vector3(0.30f, 0.24f, 0.14f),
                    mat: matPhotoPlateRuins,
                    vis: EraVisibilityMode.OnlyRuins,
                    needsSecretStep1: true);
            }
        }

        // =========================================================================================
        // RESTO DE PIEZAS FÍSICAS EN ESCENA — NOCHES II A VIII (28 COLECCIONABLES + 7 SECRETOS)
        // =========================================================================================

        public void SpawnNights2To8PhysicalCollectiblesAndSecrets()
        {
            float vestY = 2.4f;
            float roofY = 7.70f;
            float subY = -3.20f;
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            Material matGold1929 = new Material(litShader) { color = new Color(0.92f, 0.78f, 0.42f, 1f) };
            Material matPaper1929 = new Material(litShader) { color = new Color(0.95f, 0.89f, 0.74f, 1f) };
            Material matBlueRuins = new Material(litShader) { color = new Color(0.50f, 0.72f, 0.90f, 1f) };
            Material matBronzeRuins = new Material(litShader) { color = new Color(0.58f, 0.50f, 0.36f, 1f) };

            // --- NOCHE II: LAS AULAS DE ENSAYO (Zonas 17, 18, 42, 43, 44) ---
            SpawnCollectibleProp("col_n2_chalk_box_mirror3_1929", new Vector3(-36.4f, vestY + 0.92f, 56.4f), new Vector3(0.30f, 0.10f, 0.22f), matPaper1929, EraVisibilityMode.Only1929);
            SpawnCollectibleProp("col_n2_sewing_thimble_helene_ruins", new Vector3(-18.2f, vestY + 0.95f, 75.8f), new Vector3(0.22f, 0.14f, 0.22f), matBlueRuins, EraVisibilityMode.OnlyRuins);
            SpawnCollectibleProp("col_n2_amber_vial_morales_1929", new Vector3(-31.8f, vestY + 0.95f, 6.2f), new Vector3(0.22f, 0.24f, 0.22f), matGold1929, EraVisibilityMode.Only1929);
            SpawnCollectibleProp("col_n2_removed_memorial_plaque_ruins", new Vector3(-26.0f, vestY + 0.90f, -15.5f), new Vector3(0.42f, 0.08f, 0.28f), matBronzeRuins, EraVisibilityMode.OnlyRuins);

            SpawnTwoStepCoopSecret(
                secretId: "sec_n2_rooftop_binocular_echo",
                step1Pos1929: new Vector3(-22.8f, roofY + 1.05f, 41.8f),
                step2PosRuins: new Vector3(-21.2f, roofY + 1.05f, 41.8f),
                mat1929: matGold1929,
                matRuins: matBlueRuins,
                step1Prompt: "Catalejo de Latón del Mirador Norte (Azotea 1929)",
                step1DocTitle: "Secreto II (Parte 1/2 — 1929): La Vela en la Claraboya del Teatro",
                step1DocBody:
                    "Al mirar por el catalejo del mirador norte de la azotea en la madrugada de 1929, se distingue luz de vela moviéndose tras el tragaluz de la Cámara tras el Escenario a las 02:15 h.\n\n" +
                    "[COMUNICACIÓN POR VOZ]: Dile a tu compañero en las Ruinas que examine la barandilla derecha del mirador de la Azotea (Zona 42) para desbloquear el Secreto II.");

            // --- NOCHE III: EL INTERNADO (Zonas 45, 46, 47, 48) ---
            SpawnCollectibleProp("col_n3_intern_letter_unsent_1929", new Vector3(-51.0f, vestY + 0.88f, 64.0f), new Vector3(0.34f, 0.06f, 0.26f), matPaper1929, EraVisibilityMode.Only1929);
            SpawnCollectibleProp("col_n3_lead_hem_weights_ruins", new Vector3(-50.5f, vestY + 0.95f, 86.0f), new Vector3(0.28f, 0.12f, 0.24f), matBronzeRuins, EraVisibilityMode.OnlyRuins);
            SpawnCollectibleProp("col_n3_patronato_menu_gala_1929", new Vector3(-50.0f, vestY + 0.95f, 40.0f), new Vector3(0.36f, 0.06f, 0.26f), matPaper1929, EraVisibilityMode.Only1929);
            SpawnCollectibleProp("col_n3_isadora_spyglass_lens_ruins", new Vector3(-46.0f, vestY + 0.95f, 18.5f), new Vector3(0.24f, 0.14f, 0.24f), matBlueRuins, EraVisibilityMode.OnlyRuins);

            SpawnTwoStepCoopSecret(
                secretId: "sec_n3_dumbwaiter_confession",
                step1Pos1929: new Vector3(-45.2f, vestY + 1.05f, 25.5f),
                step2PosRuins: new Vector3(-45.2f, vestY + 1.05f, 29.5f),
                mat1929: matGold1929,
                matRuins: matBlueRuins,
                step1Prompt: "Torno del Montacargas de Servicio (Antecámara 1929)",
                step1DocTitle: "Secreto III (Parte 1/2 — 1929): El Montacargas entre Isadora y el Comedor",
                step1DocBody:
                    "Al soltar el freno del montacargas desde la antecámara de Isadora en 1929, la bandeja oculta desciende hasta el registro del Comedor del Patronato.\n\n" +
                    "[COMUNICACIÓN POR VOZ]: Avísale a tu compañero en las Ruinas que abra la trampilla del montacargas en el muro sur del Comedor (Zona 47).");

            // --- NOCHE IV: ARCHIVO Y PARQUE INTERIOR (Zonas 22, 24, 25, 40) ---
            SpawnCollectibleProp("col_n4_tuning_fork_c_natural_1929", new Vector3(18.2f, vestY + 0.98f, 85.0f), new Vector3(0.28f, 0.10f, 0.18f), matGold1929, EraVisibilityMode.Only1929);
            SpawnCollectibleProp("col_n4_spliced_magnetic_tape_ruins", new Vector3(36.2f, vestY + 0.95f, 55.2f), new Vector3(0.30f, 0.08f, 0.24f), matBlueRuins, EraVisibilityMode.OnlyRuins);
            SpawnCollectibleProp("col_n4_glass_orchid_token_1929", new Vector3(66.5f, 1.05f, 56.0f), new Vector3(0.26f, 0.10f, 0.26f), matGold1929, EraVisibilityMode.Only1929);
            SpawnCollectibleProp("col_n4_cello_bridge_elena_note_ruins", new Vector3(32.5f, vestY + 0.95f, 42.0f), new Vector3(0.28f, 0.12f, 0.20f), matBronzeRuins, EraVisibilityMode.OnlyRuins);

            SpawnTwoStepCoopSecret(
                secretId: "sec_n4_hidden_harmonic_resonance",
                step1Pos1929: new Vector3(68.0f, 1.15f, 54.5f),
                step2PosRuins: new Vector3(68.0f, 1.15f, 57.5f),
                mat1929: matGold1929,
                matRuins: matBlueRuins,
                step1Prompt: "Válvula Acústica del Brocal del Cisne (Invernadero 1929)",
                step1DocTitle: "Secreto IV (Parte 1/2 — 1929): El Tubo de Resonancia de la Fuente",
                step1DocBody:
                    "Al girar la figura del cisne en 1929, se abre una cámara estanca en el brocal norte de la fuente del Jardín de Invierno.\n\n" +
                    "[COMUNICACIÓN POR VOZ]: Pídele a tu compañero en las Ruinas que registre el brocal norte de la Fuente del Cisne (Zona 40).");

            // --- NOCHE V: LO ALTO Y LO PROFUNDO (Zonas 50, 51, 52, 53, 56) ---
            SpawnCollectibleProp("col_n5_magic_lantern_glass_slide_1929", new Vector3(1.5f, roofY + 0.95f, 42.5f), new Vector3(0.28f, 0.18f, 0.12f), matGold1929, EraVisibilityMode.Only1929);
            SpawnCollectibleProp("col_n5_krol_pocket_metronome_1929", new Vector3(2.0f, roofY + 0.95f, 62.0f), new Vector3(0.24f, 0.28f, 0.24f), matGold1929, EraVisibilityMode.Only1929);
            SpawnCollectibleProp("col_n5_clock_pendulum_wedge_ruins", new Vector3(1.2f, roofY + 0.95f, 79.5f), new Vector3(0.32f, 0.14f, 0.22f), matBronzeRuins, EraVisibilityMode.OnlyRuins);
            SpawnCollectibleProp("col_n5_scorched_satin_buckle_ruins", new Vector3(2.2f, subY + 0.95f, -24.0f), new Vector3(0.26f, 0.10f, 0.22f), matBlueRuins, EraVisibilityMode.OnlyRuins);

            SpawnTwoStepCoopSecret(
                secretId: "sec_n5_empty_cenotaph_ballast",
                step1Pos1929: new Vector3(-1.6f, subY + 1.05f, 23.5f),
                step2PosRuins: new Vector3(1.6f, subY + 1.05f, 23.5f),
                mat1929: matGold1929,
                matRuins: matBlueRuins,
                step1Prompt: "Clavija del Sello Funerario (Cripta 1929)",
                step1DocTitle: "Secreto V (Parte 1/2 — 1929): El Pestillo Interior del Cenotafio",
                step1DocBody:
                    "Al retirar la clavija de latón detrás del altar de la Cripta en 1929, la tapa lateral del cenotafio queda suelta para quien la inspeccione en las Ruinas.\n\n" +
                    "[COMUNICACIÓN POR VOZ]: Dile a tu compañero en las Ruinas que examine el costado derecho del cenotafio en la Cripta (Zona 56).");

            // --- NOCHE VI: BAMBALINAS (Zonas 26, 27, 29, 30) ---
            SpawnCollectibleProp("col_n6_mara_white_feather_brooch_1929", new Vector3(29.7f, vestY + 0.95f, 88.2f), new Vector3(0.26f, 0.08f, 0.22f), matPaper1929, EraVisibilityMode.Only1929);
            SpawnCollectibleProp("col_n6_elena_broken_hairpin_ruins", new Vector3(37.2f, vestY + 0.95f, 88.2f), new Vector3(0.24f, 0.08f, 0.20f), matBlueRuins, EraVisibilityMode.OnlyRuins);
            SpawnCollectibleProp("col_n6_tomas_rigging_ledger_1929", new Vector3(24.0f, vestY + 0.95f, 105.0f), new Vector3(0.36f, 0.08f, 0.28f), matPaper1929, EraVisibilityMode.Only1929);
            SpawnCollectibleProp("col_n6_sheared_steel_strand_ruins", new Vector3(-20.8f, vestY + 0.95f, 92.5f), new Vector3(0.34f, 0.10f, 0.22f), matBronzeRuins, EraVisibilityMode.OnlyRuins);

            SpawnTwoStepCoopSecret(
                secretId: "sec_n6_twin_swans_dressing_mirror",
                step1Pos1929: new Vector3(33.1f, vestY + 1.15f, 87.5f),
                step2PosRuins: new Vector3(33.9f, vestY + 1.15f, 87.5f),
                mat1929: matGold1929,
                matRuins: matBlueRuins,
                step1Prompt: "Rejilla Acústica del Tabique (Camerino de Mara — 1929)",
                step1DocTitle: "Secreto VI (Parte 1/2 — 1929): El Conducto entre los Dos Camerinos",
                step1DocBody:
                    "Desde el camerino de Mara Bell en 1929 empujas el cilindro oculto dentro de la rejilla que atraviesa el tabique hacia el camerino de Elena Vesper.\n\n" +
                    "[COMUNICACIÓN POR VOZ]: Dile a tu compañero en las Ruinas que recoja el mensaje en el muro divisorio del Camerino de Elena (Zona 27).");

            // --- NOCHE VII: EL LAGO DE LAS SOMBRAS (Zonas 32, 33, 34, 37, 38) ---
            SpawnCollectibleProp("col_n7_ivory_baton_krol_1929", new Vector3(-8.0f, 2.15f, 118.8f), new Vector3(0.36f, 0.08f, 0.14f), matPaper1929, EraVisibilityMode.Only1929);
            SpawnCollectibleProp("col_n7_amber_spotlight_filter_1929", new Vector3(-28.0f, roofY + 0.95f, 123.5f), new Vector3(0.30f, 0.24f, 0.10f), matGold1929, EraVisibilityMode.Only1929);
            SpawnCollectibleProp("col_n7_b00_wax_seal_stamp_ruins", new Vector3(34.5f, subY + 0.95f, 120.0f), new Vector3(0.24f, 0.16f, 0.24f), matBronzeRuins, EraVisibilityMode.OnlyRuins);
            SpawnCollectibleProp("col_n7_mara_confession_plate_ruins", new Vector3(29.0f, subY + 0.95f, 132.0f), new Vector3(0.34f, 0.08f, 0.28f), matBlueRuins, EraVisibilityMode.OnlyRuins);

            SpawnTwoStepCoopSecret(
                secretId: "sec_n7_proscenium_prompt_box_diary",
                step1Pos1929: new Vector3(-9.2f, vestY + 0.95f, 115.0f),
                step2PosRuins: new Vector3(-6.8f, vestY + 0.95f, 115.0f),
                mat1929: matGold1929,
                matRuins: matBlueRuins,
                step1Prompt: "Pestillo de la Concha del Apuntador (Proscenio 1929)",
                step1DocTitle: "Secreto VII (Parte 1/2 — 1929): El Doble Fondo de la Concha del Apuntador",
                step1DocBody:
                    "En el borde delantero del escenario en 1929 desbloqueas el pestillo bajo el atril del apuntador.\n\n" +
                    "[COMUNICACIÓN POR VOZ]: Avísale a tu compañero en las Ruinas que abra el doble fondo de la concha del apuntador en el Proscenio (Zona 32).");

            // --- NOCHE VIII: ÚLTIMO ACTO (Zonas 39, 57, 58, 59) ---
            SpawnCollectibleProp("col_n8_fake_departure_carriage_ticket_1929", new Vector3(-48.0f, 0.95f, 110.0f), new Vector3(0.32f, 0.06f, 0.22f), matPaper1929, EraVisibilityMode.Only1929);
            SpawnCollectibleProp("col_n8_gatekeeper_switchboard_log_ruins", new Vector3(-56.5f, 0.95f, 134.5f), new Vector3(0.34f, 0.08f, 0.24f), matBlueRuins, EraVisibilityMode.OnlyRuins);
            SpawnCollectibleProp("col_n8_gazebo_music_cylinder_1929", new Vector3(57.5f, 0.95f, 121.5f), new Vector3(0.26f, 0.16f, 0.26f), matGold1929, EraVisibilityMode.Only1929);
            SpawnCollectibleProp("col_n8_dual_hand_original_coda_ruins", new Vector3(8.3f, vestY + 0.95f, 104.2f), new Vector3(0.38f, 0.08f, 0.28f), matBlueRuins, EraVisibilityMode.OnlyRuins);

            SpawnTwoStepCoopSecret(
                secretId: "sec_n8_dawn_mirror_reconciliation",
                step1Pos1929: new Vector3(7.4f, vestY + 1.15f, 104.5f),
                step2PosRuins: new Vector3(9.2f, vestY + 1.15f, 104.5f),
                mat1929: matGold1929,
                matRuins: matBlueRuins,
                step1Prompt: "Inscripción en el Reverso del Espejo Central (Cámara 1929)",
                step1DocTitle: "Secreto VIII (Parte 1/2 — 1929): El Último Compás Tras el Azogue",
                step1DocBody:
                    "En el reverso del Espejo Central dentro de la Cámara Oculta (Zona 39) en 1929, Elena Vesper dejó encajada la mitad izquierda del medallón del cisne.\n\n" +
                    "[COMUNICACIÓN POR VOZ]: Pídele a tu compañero en las Ruinas que una la mitad derecha en el reverso del Espejo Central (Zona 39) para desbloquear el Secreto VIII.");
        }

        private void SpawnTwoStepCoopSecret(
            string secretId,
            Vector3 step1Pos1929,
            Vector3 step2PosRuins,
            Material mat1929,
            Material matRuins,
            string step1Prompt,
            string step1DocTitle,
            string step1DocBody)
        {
            if (IsUnlocked(secretId)) return;

            string step1Name = $"SecretStep1_1929_{secretId}";
            if (GameObject.Find(step1Name) == null)
            {
                GameObject s1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                s1.name = step1Name;
                s1.transform.position = step1Pos1929;
                s1.transform.localScale = new Vector3(0.28f, 0.22f, 0.14f);
                s1.GetComponent<Renderer>().sharedMaterial = mat1929;

                WorldCollectibleItem item1 = s1.AddComponent<WorldCollectibleItem>();
                item1.ConfigureCollectible(
                    id: secretId,
                    visibilityMode: EraVisibilityMode.Only1929,
                    secretStep1: true,
                    needsStep1First: false,
                    step1Prompt: step1Prompt,
                    step1DocTitle: step1DocTitle,
                    step1DocBody: step1DocBody);
            }

            SpawnCollectibleProp(
                id: secretId,
                worldPos: step2PosRuins,
                scale: new Vector3(0.30f, 0.24f, 0.16f),
                mat: matRuins,
                vis: EraVisibilityMode.OnlyRuins,
                needsSecretStep1: true);
        }

        private void SpawnCollectibleProp(
            string id,
            Vector3 worldPos,
            Vector3 scale,
            Material mat,
            EraVisibilityMode vis,
            bool needsSecretStep1 = false)
        {
            if (IsUnlocked(id)) return;
            string objName = $"WorldCollectible_{id}";
            if (GameObject.Find(objName) != null) return;

            GameObject prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prop.name = objName;
            prop.transform.position = worldPos;
            prop.transform.localScale = scale;

            Renderer rend = prop.GetComponent<Renderer>();
            if (rend != null && mat != null)
            {
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", mat.color * 0.32f);
                }
                rend.sharedMaterial = mat;
            }

            WorldCollectibleItem wc = prop.AddComponent<WorldCollectibleItem>();
            wc.ConfigureCollectible(id, vis, false, needsSecretStep1);
            spawnedWorldVisualsById[id] = prop;
        }

        // =========================================================================================
        // CATÁLOGO COMPLETO DE 32 COLECCIONABLES Y 8 SECRETOS ESPECIALES (NOCHES I A VIII)
        // =========================================================================================

        private void BuildCanonicalCatalog()
        {
            catalog.Clear();
            catalogById.Clear();

            // -------------------------------------------------------------------------------------
            // NOCHE I — LA LLEGADA (Secciones 1, 2 y 3: 4 Coleccionables + 1 Secreto Especial)
            // -------------------------------------------------------------------------------------
            RegisterDef(
                "col_n1_ribbon_elena_1929",
                "Cinta de Satén Negro con Iniciales «E. V.»",
                CollectibleItemType.DancerPersonalItems,
                EraVisibilityMode.Only1929,
                0, "I", 3, "Sección 3 — Vestuarios y Alumnas", "Zona 11 — Vestuario Femenino",
                "Una cinta de satén negro para zapatilla de punta, cosida a mano con hilo de plata y las iniciales «E. V.» en el reverso.",
                "Elena Vesper cosió dentro del forro de sus cintas la combinación de su taquilla personal (Nº 14) porque temía que alguien registrara su camerino antes del estreno del 14 de octubre de 1929.",
                "Un lazo de satén negro olvidado sobre los bancos donde las bailarinas se calzaban en 1929 (Sección 3).",
                false);

            RegisterDef(
                "col_n1_emblem_patronato_ruins",
                "Medalla Oxidada del Patronato Fundador (1929)",
                CollectibleItemType.EmblemsAndOptionalKeys,
                EraVisibilityMode.OnlyRuins,
                0, "I", 2, "Sección 2 — Administración", "Zona 6 — Recepción Antigua",
                "Insignia de bronce con el emblema del Cisne y la Lira que llevaban los miembros de la Junta del Patronato la noche del estreno.",
                "Al dorso figura grabado el lema de Isadora Vale: «La perfección sobrevive al silencio». Tres patronos entregaron sus medallas en noviembre de 1929 tras enterarse de que el acta policial había sido alterada.",
                "Una insignia metálica caída junto al mostrador donde se controlaba la electricidad y la recepción en las Ruinas (Sección 2).",
                false);

            RegisterDef(
                "col_n1_score_prelude_krol_1929",
                "Apunte de Preludio en Mi Menor de Julian Krol",
                CollectibleItemType.MusicalScores,
                EraVisibilityMode.Only1929,
                0, "I", 2, "Sección 2 — Administración", "Zona 9 — Sala de Espera",
                "Hoja de pentagrama doblada en cuatro partes con anotaciones a lápiz del maestro concertador Julian Krol.",
                "Krol escribió en el margen inferior: «Isadora me pidió hoy que el timbal del tercer acto suene tres decibelios más fuerte para tapar el ruido de un cerrojo metálico detrás del escenario».",
                "Un borrador musical olvidado por el compositor en la antesala donde aguardaban las visitas en 1929 (Sección 2).",
                false);

            RegisterDef(
                "col_n1_photo_opening_gala_ruins",
                "Negativo Velado de la Marquesina (14 de Octubre de 1929)",
                CollectibleItemType.Photographs,
                EraVisibilityMode.OnlyRuins,
                0, "I", 1, "Sección 1 — Exterior y Vestíbulo", "Zona 3 — Pórtico Monumental",
                "Placa fotográfica de acetato hallada entre las baldosas agrietadas de la entrada principal.",
                "Muestra la llegada de los carruajes a las 20:00 h del 14 de octubre de 1929. En la ventana superior del ala oeste se distingue la silueta de Isadora Vale cerrando las contraventanas por dentro.",
                "Una placa fotográfica caída al pie de las columnas de entrada en las Ruinas (Sección 1).",
                false);

            RegisterDef(
                "sec_n1_whispering_mirror_duet",
                "Secreto I — El Juramento Oculto del Tocador Nº 3",
                CollectibleItemType.SpecialSecret,
                EraVisibilityMode.Both,
                0, "I", 3, "Sección 3 — Vestuarios y Alumnas", "Zona 12 — Sala de Tocadores",
                "Un compartimento oculto en el marco del Tocador Nº 3 que solo se abre cuando el investigador de 1929 localiza la muesca grabada por Elena y guía por voz a su compañero en las Ruinas.",
                "Contiene un billete manuscrito firmado por Elena Vesper y Mara Bell el 13 de octubre de 1929: «Si Isadora intenta encerrar a una de las dos durante el estreno, la otra guardará la mitad de la partitura hasta que alguien venga a terminar el último acto».",
                "★ [SECRETO COOPERATIVO]: En los tocadores de las alumnas (Sección 3), un espejo guarda en 1929 la indicación para abrir un resorte oculto en las Ruinas.",
                true);

            // -------------------------------------------------------------------------------------
            // NOCHE II — LAS AULAS DE ENSAYO (Secciones 4 y 10: 4 Coleccionables + 1 Secreto)
            // -------------------------------------------------------------------------------------
            RegisterDef(
                "col_n2_chalk_box_mirror3_1929",
                "Estuche de Tiza Blanca de Posiciones Escénicas",
                CollectibleItemType.DancerPersonalItems,
                EraVisibilityMode.Only1929,
                1, "II", 4, "Sección 4 — Aulas de Ensayo", "Zona 17 — Aula de Espejos",
                "Pequeña caja de madera con tizas numeradas del I al IV utilizadas para marcar las plantas de los pies frente a los espejos.",
                "Confirma que el Panel n.º 3 fue calibrado con un ángulo de refracción distinto al resto de espejos de la academia para ensayar la ilusión óptica del tercer acto.",
                "Un estuche de tiza dejado frente a los espejos de ensayo postural en 1929 (Sección 4).",
                false);

            RegisterDef(
                "col_n2_sewing_thimble_helene_ruins",
                "Dedal de Plata de Madame Hélène (Talla 61 cm)",
                CollectibleItemType.DancerPersonalItems,
                EraVisibilityMode.OnlyRuins,
                1, "II", 4, "Sección 4 — Aulas de Ensayo", "Zona 18 — Sala de Maniquíes",
                "Dedal de sastrería grabado con una aguja cruzada y una cinta métrica detenida en la marca de 61 centímetros.",
                "Madame Hélène anotó en su libreta que aquel dedal se le cayó la noche en que cosió a escondidas el segundo traje de Cisne Negro con las medidas de Mara Bell.",
                "Un utensilio de plata perdido entre los maniquíes de costura en las Ruinas (Sección 4).",
                false);

            RegisterDef(
                "col_n2_amber_vial_morales_1929",
                "Frasco Ámbar Vacío de Éter Dulce (Dr. Morales)",
                CollectibleItemType.DancerPersonalItems,
                EraVisibilityMode.Only1929,
                1, "II", 10, "Sección 10 — Enfermería y Memorial", "Zona 43 — Enfermería Antigua",
                "Frasco cuentagotas de cristal ámbar con etiqueta de la botica interna fechada el 14 de octubre de 1929.",
                "El Dr. Aurelio Morales dejó constancia de que faltaban exactamente tres dosis del armario clínico dos horas antes de que comenzara la función.",
                "Un frasco de botica olvidado junto a las camillas de la enfermería en 1929 (Sección 10).",
                false);

            RegisterDef(
                "col_n2_removed_memorial_plaque_ruins",
                "Placa Conmemorativa Retirada de Lucía V.",
                CollectibleItemType.EmblemsAndOptionalKeys,
                EraVisibilityMode.OnlyRuins,
                1, "II", 10, "Sección 10 — Enfermería y Memorial", "Zona 44 — Sala Conmemorativa",
                "Placa de latón arrancada del muro sur de la Sala Conmemorativa y oculta tras un pedestal.",
                "Lleva el nombre de las tres alumnas becadas que escucharon golpes detrás del muro del escenario a las 22:18 h y fueron expulsadas a la mañana siguiente.",
                "Una placa de latón desprendida en el salón dedicado a la memoria de las alumnas en las Ruinas (Sección 10).",
                false);

            RegisterDef(
                "sec_n2_rooftop_binocular_echo",
                "Secreto II — La Ventana Iluminada desde la Azotea",
                CollectibleItemType.SpecialSecret,
                EraVisibilityMode.Both,
                1, "II", 10, "Sección 10 — Enfermería y Memorial", "Zona 42 — Terraza y Azotea Mirador",
                "Recuerdo cruzado desde el mirador norte de la azotea: en 1929 se observa luz de vela dentro de la cámara trasera del teatro horas después de cerrarse el telón.",
                "Demuestra que a las 02:15 h de la madrugada del 15 de octubre de 1929 aún había alguien con vida dentro de la Cámara tras el Escenario.",
                "★ [SECRETO DE OBSERVACIÓN]: En el mirador más alto al aire libre (Sección 10), la vista hacia el tejado del teatro revela una luz que no debería existir.",
                true);

            // -------------------------------------------------------------------------------------
            // NOCHE III — EL INTERNADO (Sección 11: 4 Coleccionables + 1 Secreto)
            // -------------------------------------------------------------------------------------
            RegisterDef(
                "col_n3_intern_letter_unsent_1929",
                "Carta No Enviada de la Interna Clara M.",
                CollectibleItemType.LettersAndDiaries,
                EraVisibilityMode.Only1929,
                2, "III", 11, "Sección 11 — Ala Occidental e Internado", "Zona 45 — Dormitorio de Internas",
                "Carta escondida bajo el colchón n.º 4 del dormitorio común, confiscada antes de llegar al correo.",
                "Clara describe cómo la noche del 14 de octubre les sirvieron la infusión nocturna media hora antes de lo habitual para que ninguna interna saliera al pasillo.",
                "Una carta doblada bajo las camas del dormitorio de internas en 1929 (Sección 11).",
                false);

            RegisterDef(
                "col_n3_lead_hem_weights_ruins",
                "Plomadas de Dobladillo del Segundo Cisne Negro",
                CollectibleItemType.DancerPersonalItems,
                EraVisibilityMode.OnlyRuins,
                2, "III", 11, "Sección 11 — Ala Occidental e Internado", "Zona 46 — Taller y Tintorería",
                "Juego de pequeñas pesas de plomo cosidas al borde del tutú escénico para imitar la caída del traje de Elena.",
                "Su peso exacto permitía que, bajo la luz tenue del tercer acto, el público no notara la diferencia de estatura entre Mara Bell y Elena Vesper.",
                "Unas pesas de costura olvidadas entre las tinas de tinte negro en las Ruinas (Sección 11).",
                false);

            RegisterDef(
                "col_n3_patronato_menu_gala_1929",
                "Menú del Banquete Privado del Patronato (14-X-1929)",
                CollectibleItemType.LettersAndDiaries,
                EraVisibilityMode.Only1929,
                2, "III", 11, "Sección 11 — Ala Occidental e Internado", "Zona 47 — Comedor y Salón del Patronato",
                "Tarjeta impresa en papel verjurado con el orden de los brindis de medianoche tras el estreno.",
                "Al dorso hay una nota financiera: el presidente del Patronato prometió duplicar la subvención si la prensa no publicaba ningún escándalo sobre las internas.",
                "Un menú de gala sobre la mesa del salón donde cenaba el Patronato en 1929 (Sección 11).",
                false);

            RegisterDef(
                "col_n3_isadora_spyglass_lens_ruins",
                "Lente Esmerilada de la Celosía de Isadora Vale",
                CollectibleItemType.EmblemsAndOptionalKeys,
                EraVisibilityMode.OnlyRuins,
                2, "III", 11, "Sección 11 — Ala Occidental e Internado", "Zona 48 — Estancia Privada de Isadora",
                "Pequeño prisma óptico engarzado en latón que permitía observar el pasillo de internas sin ser vista.",
                "Tiene grabada la cifra «22:17» — la hora exacta en que Isadora ordenó detener el reloj de la torre durante el tercer acto.",
                "Una lente óptica oculta tras la mirilla de vigilancia de la directora en las Ruinas (Sección 11).",
                false);

            RegisterDef(
                "sec_n3_dumbwaiter_confession",
                "Secreto III — El Mensaje en el Montacargas del Internado",
                CollectibleItemType.SpecialSecret,
                EraVisibilityMode.Both,
                2, "III", 11, "Sección 11 — Ala Occidental e Internado", "Zona 47 y 48 — Antecámara y Comedor",
                "En el hueco del servicio que comunica el comedor con la antecámara de Isadora apareció la servilleta con la que Mara limpió la copa carmesí.",
                "Confirma que Mara intentó vaciar la copa de Elena antes del tercer acto, pero solo alcanzó a derramar la mitad del sedante.",
                "★ [SECRETO DE EXPLORACIÓN]: Entre el comedor del Patronato y la estancia de Isadora (Sección 11), la bandeja de servicio guarda la prueba del brindis.",
                true);

            // -------------------------------------------------------------------------------------
            // NOCHE IV — ARCHIVO Y PARQUE INTERIOR (Secciones 5, 9 y 10: 4 Coleccionables + 1 Secreto)
            // -------------------------------------------------------------------------------------
            RegisterDef(
                "col_n4_tuning_fork_c_natural_1929",
                "Diapasón de Acero en Do Natural de Julian Krol",
                CollectibleItemType.DancerPersonalItems,
                EraVisibilityMode.Only1929,
                3, "IV", 5, "Sección 5 — Archivo Musical", "Zona 22 — Sala del Piano de Cola",
                "Diapasón afinado exactamente a 261.63 Hz (Do4 Natural), guardado junto al atril del piano de cola.",
                "Julian Krol lo utilizaba para afinar el resorte acústico que abría el compartimento secreto del piano donde ocultó su carta de protesta contra Isadora.",
                "Un diapasón metálico junto al piano de cola en 1929 (Sección 5).",
                false);

            RegisterDef(
                "col_n4_spliced_magnetic_tape_ruins",
                "Recorte de Cinta Magnética del Compás 48",
                CollectibleItemType.MusicalScores,
                EraVisibilityMode.OnlyRuins,
                3, "IV", 5, "Sección 5 — Archivo Musical", "Zona 24 — Cabina de Grabaciones",
                "Trozo de cinta de carrete abierto de 4 segundos cortado con tijera y escondido bajo el zócalo del amplificador.",
                "En esos cuatro segundos eliminados de la cinta oficial se oye con claridad el chasquido del cerrojo exterior cerrándose sobre Elena Vesper.",
                "Un fragmento de cinta cortada en la cabina de grabación en las Ruinas (Sección 5).",
                false);

            RegisterDef(
                "col_n4_glass_orchid_token_1929",
                "Medallón de Cristal del Invernadero de Invierno",
                CollectibleItemType.EmblemsAndOptionalKeys,
                EraVisibilityMode.Only1929,
                3, "IV", 9, "Sección 9 — Jardín de Invierno", "Zona 40 — Jardín de Invierno",
                "Disco de vidrio soplado con la silueta de dos cisnes enfrentados junto a la Fuente del Cisne.",
                "Era la prenda que Elena y Mara usaban como señal secreta en el banco del invernadero cuando necesitaban reunirse lejos de los ojos de Isadora.",
                "Un medallón de vidrio junto a la fuente del invernadero en 1929 (Sección 9).",
                false);

            RegisterDef(
                "col_n4_cello_bridge_elena_note_ruins",
                "Puente de Violonchelo con Anotación en Tinta Roja",
                CollectibleItemType.MusicalScores,
                EraVisibilityMode.OnlyRuins,
                3, "IV", 5, "Sección 5 — Archivo Musical", "Zona 25 — Depósito de Instrumentos",
                "Puente de madera de arce retirado del violonchelo solista la noche del 14 de octubre de 1929.",
                "Lleva escrita con tinta roja la indicación: «Compás 48: cuatro pasos hacia el cristal cuando suene el Do grave».",
                "Una pieza de violonchelo guardada en el depósito de instrumentos en las Ruinas (Sección 5).",
                false);

            RegisterDef(
                "sec_n4_hidden_harmonic_resonance",
                "Secreto IV — El Eco del Violonchelo en la Fuente del Cisne",
                CollectibleItemType.SpecialSecret,
                EraVisibilityMode.Both,
                3, "IV", 9, "Sección 9 — Jardín de Invierno", "Zona 40 — Fuente del Cisne",
                "Conducto acústico subterráneo que une el depósito de instrumentos con el brocal de la Fuente del Cisne.",
                "Revela que Julian Krol escondió una copia del final original dentro del cilindro estanco de la fuente antes de abandonar la academia en 1930.",
                "★ [SECRETO ACÚSTICO]: La fuente central del Jardín de Invierno (Sección 9) responde a la nota que falta en el Archivo Musical.",
                true);

            // -------------------------------------------------------------------------------------
            // NOCHE V — LO ALTO Y LO PROFUNDO (Sección 12: 4 Coleccionables + 1 Secreto)
            // -------------------------------------------------------------------------------------
            RegisterDef(
                "col_n5_magic_lantern_glass_slide_1929",
                "Placa de Cristal n.º 0 de la Linterna Mágica",
                CollectibleItemType.Photographs,
                EraVisibilityMode.Only1929,
                4, "V", 12, "Sección 12 — Planta Alta y Subterráneo", "Zona 50 — Aulas de Teoría Escénica",
                "Diapositiva de vidrio pintada a mano para el proyector de teoría escénica de la Planta Alta.",
                "Muestra el esquema técnico del Espejo Central con una cámara hueca de 1.80 metros de fondo diseñada originalmente para trucos de desaparición.",
                "Una diapositiva de cristal junto al proyector de teoría en la Planta Alta de 1929 (Sección 12).",
                false);

            RegisterDef(
                "col_n5_krol_pocket_metronome_1929",
                "Metrónomo de Bolsillo de Julian Krol (66 BPM)",
                CollectibleItemType.DancerPersonalItems,
                EraVisibilityMode.Only1929,
                4, "V", 12, "Sección 12 — Planta Alta y Subterráneo", "Zona 51 — Residencia del Maestro Krol",
                "Metrónomo mecánico de latón con la varilla soldada en Andante Sostenuto (66 pulsos por minuto).",
                "Dentro de la tapa hay un retrato diminuto de su nieto recién nacido y una promesa: «Algún día un Krol devolverá este compás a su sitio».",
                "Un metrónomo de latón en los aposentos de profesores de la Planta Alta en 1929 (Sección 12).",
                false);

            RegisterDef(
                "col_n5_clock_pendulum_wedge_ruins",
                "Cuña de Bronce del Péndulo de las 22:17 h",
                CollectibleItemType.EmblemsAndOptionalKeys,
                EraVisibilityMode.OnlyRuins,
                4, "V", 12, "Sección 12 — Planta Alta y Subterráneo", "Zona 52 — Torre del Reloj",
                "Calzo triangular de bronce forjado con las iniciales del taller de mantenimiento de la academia.",
                "Fue encajado manualmente en el escape del reloj monumental la noche del estreno para detener todas las esferas del edificio a las 22:17 h.",
                "Una pieza de bronce encajada entre los engranajes de la Torre del Reloj en las Ruinas (Sección 12).",
                false);

            RegisterDef(
                "col_n5_scorched_satin_buckle_ruins",
                "Hebilla de Satén Calcinada del Hogar de la Caldera",
                CollectibleItemType.DancerPersonalItems,
                EraVisibilityMode.OnlyRuins,
                4, "V", 12, "Sección 12 — Planta Alta y Subterráneo", "Zona 53 — Sala de Calderas",
                "Hebilla plateada ennegrecida por el hollín, rescatada de entre las cenizas frías de la caldera principal.",
                "Pertenece al abrigo de calle de Elena Vesper: Isadora mandó quemar su abrigo y su maleta en la caldera para fingir que Elena había abandonado el edificio.",
                "Un objeto metálico entre las cenizas de las calderas subterráneas en las Ruinas (Sección 12).",
                false);

            RegisterDef(
                "sec_n5_empty_cenotaph_ballast",
                "Secreto V — Las Piedras de Lastre del Féretro Vacío",
                CollectibleItemType.SpecialSecret,
                EraVisibilityMode.Both,
                4, "V", 12, "Sección 12 — Planta Alta y Subterráneo", "Zona 56 — Cripta de los Fundadores",
                "Al abrir la reja de 1929 y despejar el derrumbe en las Ruinas, el cenotafio sellado de la cripta revela su interior.",
                "El ataúd oficial enviado en secreto en noviembre de 1929 nunca contuvo restos humanos: solo seis adoquines de lastre y un velo negro.",
                "★ [SECRETO COOPERATIVO]: En lo más profundo de la cripta (Sección 12), un cerrojo de 1929 despeja la verdad sepultada en las Ruinas.",
                true);

            // -------------------------------------------------------------------------------------
            // NOCHE VI — BAMBALINAS (Sección 6: 4 Coleccionables + 1 Secreto)
            // -------------------------------------------------------------------------------------
            RegisterDef(
                "col_n6_mara_white_feather_brooch_1929",
                "Broche de Pluma de Cisne Blanco de Mara Bell",
                CollectibleItemType.DancerPersonalItems,
                EraVisibilityMode.Only1929,
                5, "VI", 6, "Sección 6 — Camerinos y Bambalinas", "Zona 26 — Camerino de Mara Bell",
                "Prendedor de nácar y plata que Mara Bell debía lucir en el tercer acto de El Lago de las Sombras.",
                "Mara lo dejó sobre su tocador a las 21:55 h cuando se vistió con el segundo traje de Cisne Negro para intentar salvar a Elena.",
                "Un broche de nácar sobre el tocador del Cisne Blanco en 1929 (Sección 6).",
                false);

            RegisterDef(
                "col_n6_elena_broken_hairpin_ruins",
                "Horquilla Doblada en el Cerrojo del Camerino de Elena",
                CollectibleItemType.DancerPersonalItems,
                EraVisibilityMode.OnlyRuins,
                5, "VI", 6, "Sección 6 — Camerinos y Bambalinas", "Zona 27 — Camerino de Elena Vesper",
                "Horquilla de acero para moño de ballet con la punta torcida dentro del ojo de la cerradura.",
                "Muestra que Elena fue encerrada primero en su propio camerino hasta diez minutos antes del tercer acto para impedir que hablara con la prensa.",
                "Una horquilla torcida junto a la puerta del camerino del Cisne Negro en las Ruinas (Sección 6).",
                false);

            RegisterDef(
                "col_n6_tomas_rigging_ledger_1929",
                "Libreta de Guardias del Tramoyista Tomás (13-X-1929)",
                CollectibleItemType.LettersAndDiaries,
                EraVisibilityMode.Only1929,
                5, "VI", 6, "Sección 6 — Camerinos y Bambalinas", "Zona 29 — Almacén de Decorados",
                "Cuaderno de cuero engrasado donde el jefe de tramoya anotaba la tensión de las cuerdas de cáñamo y cables de acero.",
                "El 13 de octubre Tomás anotó: «El cable del Contrapeso III es nuevo y soporta 800 kg; jamás podría romperse solo, como pretende la señora Vale».",
                "Una libreta técnica entre los bastidores pintados del almacén en 1929 (Sección 6).",
                false);

            RegisterDef(
                "col_n6_sheared_steel_strand_ruins",
                "Cabo de Acero Cortado con Cizalla (Contrapeso III)",
                CollectibleItemType.EmblemsAndOptionalKeys,
                EraVisibilityMode.OnlyRuins,
                5, "VI", 6, "Sección 6 — Camerinos y Bambalinas", "Zona 30 — Sala de Mecanismos y Poleas",
                "Extremo del cable maestro del Contrapeso III conservado bajo el tambor del torno oeste.",
                "Sus hebras muestran el bisel limpio de una cizalla industrial, refutando definitivamente la tesis del accidente por fatiga del metal.",
                "Un trozo de cable de acero bajo los tornos de la Sala de Poleas en las Ruinas (Sección 6).",
                false);

            RegisterDef(
                "sec_n6_twin_swans_dressing_mirror",
                "Secreto VI — El Pasadizo Acústico entre los Dos Camerinos",
                CollectibleItemType.SpecialSecret,
                EraVisibilityMode.Both,
                5, "VI", 6, "Sección 6 — Camerinos y Bambalinas", "Zonas 26 y 27 — Tabique de Camerinos",
                "Rejilla de ventilación oculta tras los espejos contiguos de Mara Bell y Elena Vesper.",
                "A través de aquella rejilla Elena y Mara acordaron la señal de los cuatro pasos del Compás 48 sin que Isadora pudiera oírlas desde el pasillo.",
                "★ [SECRETO COOPERATIVO]: Entre los camerinos de las dos solistas (Sección 6), el muro que las separa conserva su última conversación.",
                true);

            // -------------------------------------------------------------------------------------
            // NOCHE VII — EL LAGO DE LAS SOMBRAS (Secciones 7 y 8: 4 Coleccionables + 1 Secreto)
            // -------------------------------------------------------------------------------------
            RegisterDef(
                "col_n7_ivory_baton_krol_1929",
                "Batuta de Marfil Quebrada de Julian Krol",
                CollectibleItemType.DancerPersonalItems,
                EraVisibilityMode.Only1929,
                6, "VII", 7, "Sección 7 — Gran Teatro", "Zona 33 — Foso de Orquesta",
                "Batuta de dirección partida por la mitad sobre el atril del director en el Foso de Orquesta.",
                "Julian Krol la partió contra el atril a las 22:17 h en el instante exacto en que cayó el telón de hierro y comprendió la trampa de Isadora.",
                "Una batuta partida en el foso frente al escenario en 1929 (Sección 7).",
                false);

            RegisterDef(
                "col_n7_amber_spotlight_filter_1929",
                "Gelatina Ámbar n.º 14 de la Cabina de Luces",
                CollectibleItemType.EmblemsAndOptionalKeys,
                EraVisibilityMode.Only1929,
                6, "VII", 7, "Sección 7 — Gran Teatro", "Zona 34 — Cabina de Iluminación",
                "Filtro óptico montado en el proyector central para volver translúcido el cristal de azogue del escenario.",
                "Solo cuando aquel filtro iluminaba el Espejo Central desde 35 grados se volvía invisible lo que ocurría dentro de la cámara trasera.",
                "Un filtro de foco teatral en la cabina alta de iluminación en 1929 (Sección 7).",
                false);

            RegisterDef(
                "col_n7_b00_wax_seal_stamp_ruins",
                "Sello de Lacre de la Bóveda Subterránea B-00",
                CollectibleItemType.EmblemsAndOptionalKeys,
                EraVisibilityMode.OnlyRuins,
                6, "VII", 8, "Sección 8 — Zona Secreta B-00", "Zona 37 — Depósito Subterráneo B-00",
                "Matriz de bronce utilizada para lacrar los expedientes prohibidos retirados del archivo general.",
                "Lleva las iniciales «I. V. — Archivo Reservado 1929» y abría los cajones donde se ocultaron las declaraciones originales de los músicos.",
                "Un sello de lacre oculto en el depósito clausurado bajo el teatro en las Ruinas (Sección 8).",
                false);

            RegisterDef(
                "col_n7_mara_confession_plate_ruins",
                "Retrato Doble de Elena y Mara (Sin Retocar, 1929)",
                CollectibleItemType.Photographs,
                EraVisibilityMode.OnlyRuins,
                6, "VII", 8, "Sección 8 — Zona Secreta B-00", "Zona 38 — Archivos Prohibidos B-00",
                "Fotografía de plata en gelatina donde Elena Vesper y Mara Bell posan juntas vestidas ambas de Cisne Negro.",
                "Al dorso, la letra de Elena dice: «Dos cisnes negros para un solo espejo. Nadie podrá encerrar la verdad para siempre».",
                "Una fotografía prohibida archivada en la bóveda más profunda bajo el escenario en las Ruinas (Sección 8).",
                false);

            RegisterDef(
                "sec_n7_proscenium_prompt_box_diary",
                "Secreto VII — El Testimonio del Apuntador en la Concha del Proscenio",
                CollectibleItemType.SpecialSecret,
                EraVisibilityMode.Both,
                6, "VII", 7, "Sección 7 — Gran Teatro", "Zona 32 — Proscenio del Escenario Principal",
                "Bajo la concha acústica del apuntador, en el borde delantero del escenario, sobrevivió el libreto anotado a un metro del espejo.",
                "El apuntador registró las últimas palabras de Elena antes de que cayera el telón de hierro: «¡Mara, no entres, el cerrojo se cierra desde arriba!».",
                "★ [SECRETO ESCÉNICO]: En el borde delantero de las tablas del Gran Teatro (Sección 7), la concha del apuntador guarda lo que el público no oyó.",
                true);

            // -------------------------------------------------------------------------------------
            // NOCHE VIII — ÚLTIMO ACTO (Secciones 8 y 12: 4 Coleccionables + 1 Secreto Final)
            // -------------------------------------------------------------------------------------
            RegisterDef(
                "col_n8_fake_departure_carriage_ticket_1929",
                "Billete de Carruaje Sin Usar a Nombre de Elena Vesper",
                CollectibleItemType.LettersAndDiaries,
                EraVisibilityMode.Only1929,
                7, "VIII", 12, "Sección 12 — Exteriores Traseros", "Zona 57 — Cochera de Carruajes",
                "Billete del expreso nocturno de las 23:30 h del 14 de octubre de 1929 comprado por la secretaría de Isadora.",
                "Nunca fue picado ni utilizado: Isadora lo compró por adelantado para fabricar la coartada de que Elena había tomado el tren tras la función.",
                "Un billete de viaje intacto en la cochera de carruajes del patio norte en 1929 (Sección 12).",
                false);

            RegisterDef(
                "col_n8_gatekeeper_switchboard_log_ruins",
                "Cinta Telegráfica de la Centralita del Guarda (23:40 h)",
                CollectibleItemType.LettersAndDiaries,
                EraVisibilityMode.OnlyRuins,
                7, "VIII", 12, "Sección 12 — Exteriores Traseros", "Zona 58 — Pabellón del Guarda",
                "Tira de papel impresa de la centralita telefónica exterior de la Academia Vesper.",
                "Demuestra que la llamada a la policía fue bloqueada desde el despacho de dirección hasta las 08:00 h de la mañana siguiente.",
                "Un registro telefónico en la caseta del guarda junto a la verja norte en las Ruinas (Sección 12).",
                false);

            RegisterDef(
                "col_n8_gazebo_music_cylinder_1929",
                "Cilindro de Latón de la Cajita Musical del Lago",
                CollectibleItemType.MusicalScores,
                EraVisibilityMode.Only1929,
                7, "VIII", 12, "Sección 12 — Exteriores Traseros", "Zona 59 — Estanque Exterior y Quiosco",
                "Cilindro de púas microscópicas que contiene los ocho compases de la coda de Elena y Mara.",
                "Fue grabado por Julian Krol para que, al insertarse en la cajita musical de la cámara oculta, emitiera la frecuencia exacta del Compás 48.",
                "Un cilindro musical de latón en el quiosco junto al estanque helado en 1929 (Sección 12).",
                false);

            RegisterDef(
                "col_n8_dual_hand_original_coda_ruins",
                "Manuscrito Original a Dos Manos (Elena Vesper & Mara Bell)",
                CollectibleItemType.MusicalScores,
                EraVisibilityMode.OnlyRuins,
                7, "VIII", 8, "Sección 8 — Cámara tras el Escenario", "Zona 39 — Cámara tras el Escenario",
                "Pliego final firmado conjuntamente por Elena Vesper, Mara Bell y Julian Krol en octubre de 1929.",
                "Es el objeto sagrado que motivó la invitación anónima a Daniela y Mateo, uniendo a las dos familias casi un siglo después.",
                "El manuscrito definitivo custodiado tras el Espejo Central del escenario en las Ruinas (Sección 8).",
                false);

            RegisterDef(
                "sec_n8_dawn_mirror_reconciliation",
                "Secreto VIII — La Reverencia de las 06:00 AM en el Espejo Central",
                CollectibleItemType.SpecialSecret,
                EraVisibilityMode.Both,
                7, "VIII", 8, "Sección 8 — Cámara tras el Escenario", "Zona 39 — Cámara tras el Escenario",
                "Cuando ambos investigadores sostienen al unísono los dos atriles de resonancia de la Cámara tras el Escenario, el azogue del espejo refleja ambas épocas unidas.",
                "Elena Vesper y Mara Bell aparecen reflejadas sin barrotes ni sombras entre ellas, completando el paso final que quedó interrumpido en 1929.",
                "★ [SECRETO FINAL]: En la cámara oculta tras el espejo del teatro (Sección 8), la sincronía de ambos tiempos desvela la última reverencia.",
                true);

            RecountTotals();
        }

        private void RegisterDef(
            string id,
            string title,
            CollectibleItemType type,
            EraVisibilityMode era,
            int nightIdx,
            string nightRoman,
            int sectionIdx,
            string sectionName,
            string zoneName,
            string desc,
            string rewardLore,
            string vagueHint,
            bool specialSecret)
        {
            CollectibleDefinition def = new CollectibleDefinition
            {
                id = id,
                title = title,
                type = type,
                originEra = era,
                nightIndex = nightIdx,
                nightRoman = nightRoman,
                sectionIndex = sectionIdx,
                sectionName = sectionName,
                zoneName = zoneName,
                description = desc,
                secondaryLoreReward = rewardLore,
                vagueMissingHint = vagueHint,
                isSpecialSecret = specialSecret
            };
            catalog.Add(def);
            catalogById[id] = def;
        }

        private void RecountTotals()
        {
            int colGoal = 0;
            int secGoal = 0;
            int colUnl = 0;
            int secUnl = 0;

            for (int i = 0; i < catalog.Count; i++)
            {
                var d = catalog[i];
                bool unl = unlockedById.ContainsKey(d.id);
                if (d.isSpecialSecret)
                {
                    secGoal++;
                    if (unl) secUnl++;
                }
                else
                {
                    colGoal++;
                    if (unl) colUnl++;
                }
            }

            TotalCollectiblesGoal = colGoal;
            TotalSecretsGoal = secGoal;
            UnlockedCollectiblesCount = colUnl;
            UnlockedSecretsCount = secUnl;
        }

        // =========================================================================================
        // PERSISTENCIA EN EL GUARDADO DEL HOST (SIN DUPLICADOS)
        // =========================================================================================

        public void SaveToHostPrefsIfAuthoritative()
        {
            bool isClient = LanSessionManager.Instance != null && LanSessionManager.Instance.CurrentMode == LanSessionMode.Client;
            if (isClient) return;

            try
            {
                CollectiblesSaveData data = new CollectiblesSaveData();
                foreach (var kv in unlockedById)
                {
                    data.unlockedItems.Add(kv.Value);
                }
                foreach (string s1 in secretStepOneCompleted)
                {
                    data.secretStepOneDoneIds.Add(s1);
                }

                string json = JsonUtility.ToJson(data);
                PlayerPrefs.SetString(SavePrefsKey, json);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[CollectibleAndSecretManager] Error al guardar colección: {ex.Message}");
            }
        }

        public void LoadFromHostPrefs()
        {
            bool isClient = LanSessionManager.Instance != null && LanSessionManager.Instance.CurrentMode == LanSessionMode.Client;
            if (isClient) return;
            if (!PlayerPrefs.HasKey(SavePrefsKey))
            {
                RecountTotals();
                return;
            }

            try
            {
                string json = PlayerPrefs.GetString(SavePrefsKey, "");
                if (!string.IsNullOrEmpty(json))
                {
                    CollectiblesSaveData data = JsonUtility.FromJson<CollectiblesSaveData>(json);
                    if (data != null)
                    {
                        if (data.unlockedItems != null)
                        {
                            for (int i = 0; i < data.unlockedItems.Count; i++)
                            {
                                var rec = data.unlockedItems[i];
                                if (rec != null && !string.IsNullOrEmpty(rec.id) && !unlockedById.ContainsKey(rec.id))
                                {
                                    unlockedById[rec.id] = rec;
                                }
                            }
                        }
                        if (data.secretStepOneDoneIds != null)
                        {
                            for (int i = 0; i < data.secretStepOneDoneIds.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(data.secretStepOneDoneIds[i]))
                                {
                                    secretStepOneCompleted.Add(data.secretStepOneDoneIds[i]);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[CollectibleAndSecretManager] Error al cargar colección: {ex.Message}");
            }

            RecountTotals();
            OnCollectionUpdated?.Invoke();
        }

        public static string FormatTypeShort(CollectibleItemType type)
        {
            switch (type)
            {
                case CollectibleItemType.LettersAndDiaries: return "Carta / Diario";
                case CollectibleItemType.Photographs: return "Fotografía";
                case CollectibleItemType.MusicalScores: return "Partitura";
                case CollectibleItemType.DancerPersonalItems: return "Objeto Personal";
                case CollectibleItemType.EmblemsAndOptionalKeys: return "Emblema / Reliquia";
                case CollectibleItemType.SpecialSecret: return "★ Secreto Especial";
                default: return "Coleccionable";
            }
        }

        private static JournalTabCategory MapCollectibleTypeToJournalTab(CollectibleItemType type)
        {
            switch (type)
            {
                case CollectibleItemType.Photographs: return JournalTabCategory.Photographs;
                case CollectibleItemType.MusicalScores: return JournalTabCategory.ScoresAndMusic;
                case CollectibleItemType.SpecialSecret: return JournalTabCategory.PuzzleClues;
                default: return JournalTabCategory.Documents;
            }
        }

        private static string GetLocalPlayerDisplayName()
        {
            if (LanSessionManager.Instance != null && !string.IsNullOrWhiteSpace(LanSessionManager.Instance.LocalPlayerName))
            {
                return LanSessionManager.Instance.LocalPlayerName;
            }
            EraVisual era = EraVisualManager.Instance != null ? EraVisualManager.Instance.CurrentEra : EraVisual.Glory1929;
            return era == EraVisual.Glory1929 ? "Daniela (1929)" : "Mateo (Ruinas)";
        }
    }
}
