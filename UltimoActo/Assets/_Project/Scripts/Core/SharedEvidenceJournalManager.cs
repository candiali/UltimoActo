using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UltimoActo.Interaction;
using UltimoActo.Narrative;
using UltimoActo.UI;

namespace UltimoActo.Core
{
    public enum JournalTabCategory
    {
        Documents = 0,
        PuzzleClues = 1,
        Photographs = 2,
        ScoresAndMusic = 3,
        PersonalNotes = 4,
        CollectiblesAndSecrets = 5
    }

    [Serializable]
    public class EvidenceJournalEntry
    {
        public string entryId;
        public string title;
        public string subtitle;
        public string bodyContent;
        public int category;          // (int)JournalTabCategory
        public string foundByPlayer;  // Nombre del jugador que lo encontró o escribió
        public string nightLabel;     // Ej: "Noche I — La Llegada"
        public string eraSeenLabel;   // Ej: "Época 1929 (Gloria)" o "Época Actual (Ruinas)"
        public int eraVisibilityRule; // (int)EraVisibilityMode: 0=Ambas, 1=Solo 1929, 2=Solo Ruinas
        public bool isPuzzleSolution;
        public bool isAchievement;
    }

    [Serializable]
    public class EvidenceJournalSaveData
    {
        public List<EvidenceJournalEntry> entries = new List<EvidenceJournalEntry>();
        public List<string> unlockedAchievements = new List<string>();
    }

    /// <summary>
    /// 2) Diario / Archivo de Evidencias Compartido:
    /// - Registra automáticamente todo documento, pista de puzle, fotografía, partitura/música,
    ///   solución importante de puzle resuelto, logro cooperativo o nota propia escrita por cualquiera
    ///   de los dos jugadores.
    /// - Es 100% COMPARTIDO en red (lo que lee o anota uno aparece al instante en el diario de ambos)
    ///   y guarda quién lo encontró, en qué noche, en qué época se vio y si es "Solo visible en 1929"
    ///   o "Solo visible en Ruinas".
    /// - Persiste en el guardado del Host (JsonUtility en PlayerPrefs) sin duplicar entradas.
    /// </summary>
    public class SharedEvidenceJournalManager : MonoBehaviour
    {
        public const string SavePrefsKey = "UltimoActo_Host_EvidenceJournal_V1";

        public static SharedEvidenceJournalManager Instance { get; private set; }

        [SerializeField] private List<EvidenceJournalEntry> entries = new List<EvidenceJournalEntry>();
        [SerializeField] private List<string> unlockedAchievements = new List<string>();

        private readonly Dictionary<string, EvidenceJournalEntry> entriesById = new Dictionary<string, EvidenceJournalEntry>();

        /// <summary>
        /// Evento local cada vez que la lista del diario cambia (para refrescar EvidenceJournalUI).
        /// </summary>
        public event Action OnJournalUpdated;

        /// <summary>
        /// Evento disparado cuando este jugador añade una entrada nueva y debe enviarse por red al otro jugador.
        /// </summary>
        public static event Action<EvidenceJournalEntry> OnLocalEntryCreatedForNetwork;

        public IReadOnlyList<EvidenceJournalEntry> AllEntries => entries;
        public IReadOnlyList<string> UnlockedAchievements => unlockedAchievements;

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
            if (FindFirstObjectByType<SharedEvidenceJournalManager>() != null) return;
            if (FindFirstObjectByType<UltimoActo.Player.PlayerMovement>() == null) return;

            GameObject go = new GameObject("SharedEvidenceJournalManager");
            go.AddComponent<SharedEvidenceJournalManager>();
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
            LoadFromHostSave();
            SubscribeWorldAndPuzzleEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeWorldAndPuzzleEvents();
            if (Instance == this) Instance = null;
        }

        private void SubscribeWorldAndPuzzleEvents()
        {
            DocumentUI.OnDocumentDisplayed += OnDocumentDisplayedHandler;
            AcademyFuseBoxController.OnPowerRestoredEvent += OnFuseBoxSolvedHandler;
            PortraitGalleryPuzzleController.OnPortraitPuzzleSolved += OnPortraitGallerySolvedHandler;
            OrderedSequencePuzzle.OnAnySequencePuzzleUpdated += OnSequencePuzzleUpdatedHandler;
            TheatreClimaxController.OnClimaxPhaseActivated += OnTheatreClimaxPhaseHandler;
        }

        private void UnsubscribeWorldAndPuzzleEvents()
        {
            DocumentUI.OnDocumentDisplayed -= OnDocumentDisplayedHandler;
            AcademyFuseBoxController.OnPowerRestoredEvent -= OnFuseBoxSolvedHandler;
            PortraitGalleryPuzzleController.OnPortraitPuzzleSolved -= OnPortraitGallerySolvedHandler;
            OrderedSequencePuzzle.OnAnySequencePuzzleUpdated -= OnSequencePuzzleUpdatedHandler;
            TheatreClimaxController.OnClimaxPhaseActivated -= OnTheatreClimaxPhaseHandler;
        }

        // ====================================================================
        // CLASIFICACIÓN AUTOMÁTICA EN LAS 5 PESTAÑAS DEL DIARIO
        // ====================================================================
        public static JournalTabCategory ClassifyEntryCategory(string docId, string title, string body, bool isPersonalNote = false, bool isPuzzleClueOrSolution = false)
        {
            if (isPersonalNote) return JournalTabCategory.PersonalNotes;

            string combined = $"{docId} {title} {body}".ToLowerInvariant();

            if (combined.Contains("foto") || combined.Contains("photo") || combined.Contains("retrato") ||
                combined.Contains("portrait") || combined.Contains("placa de cristal") || combined.Contains("diapositiva") ||
                combined.Contains("imagen") || combined.Contains("cartel"))
            {
                return JournalTabCategory.Photographs;
            }

            if (combined.Contains("partitura") || combined.Contains("score") || combined.Contains("música") ||
                combined.Contains("musica") || combined.Contains("piano") || combined.Contains("cadencia") ||
                combined.Contains("compás") || combined.Contains("compas") || combined.Contains("cinta") ||
                combined.Contains("grabación") || combined.Contains("grabacion") || combined.Contains("violonchelo") ||
                combined.Contains("catálogo general del archivo musical") || combined.Contains("do natural"))
            {
                return JournalTabCategory.ScoresAndMusic;
            }

            if (isPuzzleClueOrSolution ||
                combined.Contains("pista") || combined.Contains("combinación") || combined.Contains("combinacion") ||
                combined.Contains("código") || combined.Contains("codigo") || combined.Contains("solución") ||
                combined.Contains("solucion") || combined.Contains("fusible") || combined.Contains("dial") ||
                combined.Contains("coronación") || combined.Contains("coronacion") || combined.Contains("secuencia") ||
                combined.Contains("contrapeso") || combined.Contains("palanca") || combined.Contains("clave dividida") ||
                combined.Contains("mitad"))
            {
                return JournalTabCategory.PuzzleClues;
            }

            return JournalTabCategory.Documents;
        }

        public static EraVisibilityMode DetectEraVisibilityForText(string docId, string title, string body)
        {
            // Detectar si existe un WorldDocument en escena con EraOnlyVisibility en sí mismo o sus padres
            if (!string.IsNullOrEmpty(docId))
            {
                WorldDocument[] allDocs = FindObjectsByType<WorldDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (int i = 0; i < allDocs.Length; i++)
                {
                    if (allDocs[i] != null && string.Equals(allDocs[i].Id, docId, StringComparison.OrdinalIgnoreCase))
                    {
                        EraOnlyVisibility eov = allDocs[i].GetComponentInParent<EraOnlyVisibility>();
                        if (eov != null && eov.Visibility != EraVisibilityMode.Both)
                        {
                            return eov.Visibility;
                        }
                        int layer = allDocs[i].gameObject.layer;
                        if (layer == EraVisualManager.GetEra1929Layer()) return EraVisibilityMode.Only1929;
                        if (layer == EraVisualManager.GetEraRuinsLayer()) return EraVisibilityMode.OnlyRuins;
                    }
                }
            }

            string combined = $"{docId} {title} {body}".ToLowerInvariant();
            if (combined.Contains("solo visible en 1929") || combined.Contains("[1929]") || combined.Contains("época 1929 (exclusivo)"))
            {
                return EraVisibilityMode.Only1929;
            }
            if (combined.Contains("solo visible en ruinas") || combined.Contains("[ruinas]") || combined.Contains("época ruinas (exclusivo)"))
            {
                return EraVisibilityMode.OnlyRuins;
            }
            return EraVisibilityMode.Both;
        }

        // ====================================================================
        // CAPTURA AUTOMÁTICA DE DOCUMENTOS, PUZLES, NOTAS Y LOGROS
        // ====================================================================
        private void OnDocumentDisplayedHandler(string docId, string title, string body, string subtitle)
        {
            JournalTabCategory cat = ClassifyEntryCategory(docId, title, body, false, false);
            EraVisibilityMode vis = DetectEraVisibilityForText(docId, title, body);

            RecordLocalEntry(
                entryId: string.IsNullOrEmpty(docId) ? $"doc_{SanitizeId(title)}" : docId,
                title: title,
                subtitle: subtitle,
                bodyContent: body,
                category: cat,
                visibilityRule: vis,
                isPuzzleSolution: false,
                isAchievement: false);
        }

        private void OnFuseBoxSolvedHandler()
        {
            RecordPuzzleSolution(
                puzzleId: "sol_cuadro_electrico_220v",
                puzzleTitle: "[SOLUCIÓN REGISTRADA] Cuadro Eléctrico Principal (220V)",
                solutionSummary:
                    "SOLUCIÓN DEL CUADRO ELÉCTRICO (NOCHE I — RECEPCIÓN ANTIGUA):\n\n" +
                    "• **Fusible Cerámico 10A**: Encontrado en el banco del Vestuario Femenino (Zona 11).\n" +
                    "• **Fusible Cerámico 15A**: Encontrado en la Taquilla Nº 07 de Mara Bell (Zona 13).\n" +
                    "• **Fusible Cerámico 25A**: Encontrado en el Despacho de Dirección (Zona 7).\n\n" +
                    "Resultado: Corriente de 220V restaurada y cierre electromagnético del Archivo de Expedientes (Zona 8) liberado.");
        }

        private void OnPortraitGallerySolvedHandler()
        {
            RecordPuzzleSolution(
                puzzleId: "sol_galeria_retratos_estaciones",
                puzzleTitle: "[SOLUCIÓN REGISTRADA] Enigma de los Retratos Estacionales (Zona 10)",
                solutionSummary:
                    "COMBINACIÓN DE CORONACIONES ESTACIONALES (PASILLO DE LOS RETRATOS):\n\n" +
                    "1. **Beatrice Dupont (1898)** -> **IV — Invierno**\n" +
                    "2. **Clara Valmont (1906)** -> **I — Primavera**\n" +
                    "3. **Teresa Vance (1914)** -> **III — Otoño**\n" +
                    "4. **Elena Vesper (1922)** -> **II — Verano**\n\n" +
                    "Resultado: El retrato de Elena Vesper ha cedido hacia adelante abriendo su compartimento secreto.");
        }

        private void OnSequencePuzzleUpdatedHandler(OrderedSequencePuzzle puzzle, int progress, bool solved)
        {
            if (!solved || puzzle == null) return;
            RecordPuzzleSolution(
                puzzleId: $"sol_seq_{puzzle.PuzzleId}",
                puzzleTitle: $"[SOLUCIÓN REGISTRADA] Mecanismo de Secuencia ({puzzle.PuzzleId})",
                solutionSummary:
                    $"El mecanismo de secuencia ordenada «{puzzle.PuzzleId}» ha sido resuelto en el orden correcto por los investigadores, " +
                    "revelando su compartimento o documento de recompensa correspondiente.");
        }

        private void OnTheatreClimaxPhaseHandler(int phaseIndex)
        {
            RecordPuzzleSolution(
                puzzleId: $"sol_climax_fase_{phaseIndex}",
                puzzleTitle: $"[SOLUCIÓN REGISTRADA] Clímax del Teatro Principal — Fase {phaseIndex}/5",
                solutionSummary:
                    $"Se ha completado y sincronizado la Fase {phaseIndex} de la reconstrucción final de «El Lago de las Sombras» en el Gran Teatro Vesper.");
        }

        public void RecordClueOrDocument(
            string entryId,
            string title,
            string subtitle,
            string bodyContent,
            JournalTabCategory category,
            EraVisibilityMode visibilityRule = EraVisibilityMode.Both)
        {
            RecordLocalEntry(entryId, title, subtitle, bodyContent, category, visibilityRule, false, false);
        }

        public void RecordPuzzleSolution(
            string puzzleId,
            string puzzleTitle,
            string solutionSummary,
            EraVisibilityMode visibilityRule = EraVisibilityMode.Both)
        {
            RecordLocalEntry(
                entryId: puzzleId,
                title: puzzleTitle,
                subtitle: "Solución verificada de puzle — Archivo Cooperativo Vesper",
                bodyContent: solutionSummary,
                category: JournalTabCategory.PuzzleClues,
                visibilityRule: visibilityRule,
                isPuzzleSolution: true,
                isAchievement: false);
        }

        public void UnlockCoopAchievement(string achievementId, string achievementTitle, string achievementDescription)
        {
            if (string.IsNullOrEmpty(achievementId)) return;
            if (!unlockedAchievements.Contains(achievementId))
            {
                unlockedAchievements.Add(achievementId);
            }

            RecordLocalEntry(
                entryId: $"ach_{achievementId}",
                title: $"★ LOGRO COOPERATIVO: {achievementTitle}",
                subtitle: "Hito Cooperativo Desbloqueado (Registrado para el Diario y Créditos)",
                bodyContent: achievementDescription,
                category: JournalTabCategory.PuzzleClues,
                visibilityRule: EraVisibilityMode.Both,
                isPuzzleSolution: true,
                isAchievement: true);

            if (NightManager.Instance != null)
            {
                NightManager.Instance.ShowToast($"★ LOGRO COOPERATIVO DESBLOQUEADO: «{achievementTitle}» — Guardado en el Diario [J]", 5.2f);
            }
        }

        public void AddPersonalNote(string noteText)
        {
            if (string.IsNullOrWhiteSpace(noteText)) return;
            string cleanText = noteText.Trim();
            string author = GetCurrentLocalPlayerName();
            string nightLabel = GetCurrentNightLabel();
            string eraLabel = GetCurrentEraLabel();
            string noteId = $"note_{DateTime.UtcNow.Ticks}_{UnityEngine.Random.Range(100, 999)}";

            EvidenceJournalEntry noteEntry = new EvidenceJournalEntry
            {
                entryId = noteId,
                title = $"Nota de {author} ({nightLabel})",
                subtitle = $"Escrita en {eraLabel}",
                bodyContent = cleanText,
                category = (int)JournalTabCategory.PersonalNotes,
                foundByPlayer = author,
                nightLabel = nightLabel,
                eraSeenLabel = eraLabel,
                eraVisibilityRule = (int)EraVisibilityMode.Both,
                isPuzzleSolution = false,
                isAchievement = false
            };

            if (TryInsertEntryWithoutDuplicate(noteEntry))
            {
                SaveToHostIfAuthoritative();
                OnJournalUpdated?.Invoke();
                OnLocalEntryCreatedForNetwork?.Invoke(noteEntry);
            }
        }

        private void RecordLocalEntry(
            string entryId,
            string title,
            string subtitle,
            string bodyContent,
            JournalTabCategory category,
            EraVisibilityMode visibilityRule,
            bool isPuzzleSolution,
            bool isAchievement)
        {
            if (string.IsNullOrEmpty(entryId)) return;

            string finder = GetCurrentLocalPlayerName();
            string nightLbl = GetCurrentNightLabel();
            string eraLbl = GetCurrentEraLabel();

            EvidenceJournalEntry entry = new EvidenceJournalEntry
            {
                entryId = entryId,
                title = string.IsNullOrEmpty(title) ? entryId : title,
                subtitle = subtitle ?? string.Empty,
                bodyContent = bodyContent ?? string.Empty,
                category = (int)category,
                foundByPlayer = finder,
                nightLabel = nightLbl,
                eraSeenLabel = eraLbl,
                eraVisibilityRule = (int)visibilityRule,
                isPuzzleSolution = isPuzzleSolution,
                isAchievement = isAchievement
            };

            if (TryInsertEntryWithoutDuplicate(entry))
            {
                SaveToHostIfAuthoritative();
                OnJournalUpdated?.Invoke();
                OnLocalEntryCreatedForNetwork?.Invoke(entry);
            }
        }

        public void ApplyEntryFromNetwork(EvidenceJournalEntry remoteEntry)
        {
            if (remoteEntry == null || string.IsNullOrEmpty(remoteEntry.entryId)) return;

            if (remoteEntry.isAchievement && !unlockedAchievements.Contains(remoteEntry.entryId))
            {
                unlockedAchievements.Add(remoteEntry.entryId);
            }

            if (TryInsertEntryWithoutDuplicate(remoteEntry))
            {
                SaveToHostIfAuthoritative();
                OnJournalUpdated?.Invoke();
            }
        }

        private bool TryInsertEntryWithoutDuplicate(EvidenceJournalEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.entryId)) return false;

            if (entriesById.ContainsKey(entry.entryId))
            {
                return false;
            }

            // Evitar duplicados por mismo título exacto en documentos estándar
            if (entry.category != (int)JournalTabCategory.PersonalNotes)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    if (string.Equals(entries[i].title, entry.title, StringComparison.OrdinalIgnoreCase) &&
                        entries[i].category == entry.category)
                    {
                        return false;
                    }
                }
            }

            entries.Add(entry);
            entriesById[entry.entryId] = entry;
            return true;
        }

        // ====================================================================
        // PERSISTENCIA EN EL GUARDADO DEL HOST (SIN DUPLICADOS)
        // ====================================================================
        public void SaveToHostIfAuthoritative()
        {
            bool isClient = LanSessionManager.Instance != null && LanSessionManager.Instance.CurrentMode == LanSessionMode.Client;
            if (isClient) return;

            try
            {
                EvidenceJournalSaveData data = new EvidenceJournalSaveData
                {
                    entries = new List<EvidenceJournalEntry>(entries),
                    unlockedAchievements = new List<string>(unlockedAchievements)
                };
                string json = JsonUtility.ToJson(data);
                PlayerPrefs.SetString(SavePrefsKey, json);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SharedEvidenceJournal] Error al guardar diario del Host: {ex.Message}");
            }
        }

        public void LoadFromHostSave()
        {
            bool isClient = LanSessionManager.Instance != null && LanSessionManager.Instance.CurrentMode == LanSessionMode.Client;
            if (isClient) return;
            if (!PlayerPrefs.HasKey(SavePrefsKey)) return;

            try
            {
                string json = PlayerPrefs.GetString(SavePrefsKey, "");
                if (string.IsNullOrEmpty(json)) return;

                EvidenceJournalSaveData data = JsonUtility.FromJson<EvidenceJournalSaveData>(json);
                if (data == null || data.entries == null) return;

                for (int i = 0; i < data.entries.Count; i++)
                {
                    TryInsertEntryWithoutDuplicate(data.entries[i]);
                }

                if (data.unlockedAchievements != null)
                {
                    for (int i = 0; i < data.unlockedAchievements.Count; i++)
                    {
                        string ach = data.unlockedAchievements[i];
                        if (!string.IsNullOrEmpty(ach) && !unlockedAchievements.Contains(ach))
                        {
                            unlockedAchievements.Add(ach);
                        }
                    }
                }

                OnJournalUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SharedEvidenceJournal] Error al cargar diario guardado: {ex.Message}");
            }
        }

        public void ClearSavedJournalForTesting()
        {
            entries.Clear();
            entriesById.Clear();
            unlockedAchievements.Clear();
            PlayerPrefs.DeleteKey(SavePrefsKey);
            PlayerPrefs.Save();
            OnJournalUpdated?.Invoke();
        }

        public List<EvidenceJournalEntry> GetEntriesForTab(JournalTabCategory tab)
        {
            List<EvidenceJournalEntry> list = new List<EvidenceJournalEntry>();
            int tabInt = (int)tab;
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                if (entries[i].category == tabInt)
                {
                    list.Add(entries[i]);
                }
            }
            return list;
        }

        public static string FormatVisibilityBadge(int visibilityRule)
        {
            EraVisibilityMode mode = (EraVisibilityMode)visibilityRule;
            switch (mode)
            {
                case EraVisibilityMode.Only1929:
                    return "<color=#F5D488>[SOLO VISIBLE EN 1929]</color>";
                case EraVisibilityMode.OnlyRuins:
                    return "<color=#92C2F4>[SOLO VISIBLE EN RUINAS]</color>";
                default:
                    return "<color=#B8B0A4>[VISIBLE EN AMBAS ÉPOCAS]</color>";
            }
        }

        private static string GetCurrentLocalPlayerName()
        {
            if (LanSessionManager.Instance != null && !string.IsNullOrWhiteSpace(LanSessionManager.Instance.LocalPlayerName))
            {
                return LanSessionManager.Instance.LocalPlayerName;
            }
            EraVisual era = EraVisualManager.Instance != null ? EraVisualManager.Instance.CurrentEra : EraVisual.Glory1929;
            return era == EraVisual.Glory1929 ? "Investigador (1929)" : "Investigador (Ruinas)";
        }

        private static string GetCurrentNightLabel()
        {
            if (NightManager.Instance != null && NightManager.Instance.Current != null)
            {
                return $"Noche {NightManager.Instance.Current.roman} — {NightManager.Instance.Current.title}";
            }
            return "Noche I — La Llegada";
        }

        private static string GetCurrentEraLabel()
        {
            EraVisual era = EraVisualManager.Instance != null ? EraVisualManager.Instance.CurrentEra : EraVisual.Glory1929;
            return era == EraVisual.Glory1929 ? "Época 1929 (Gloria)" : "Época Actual (Ruinas)";
        }

        private static string SanitizeId(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "entry";
            return raw.Trim().ToLowerInvariant().Replace(" ", "_").Replace("|", "_");
        }
    }
}
