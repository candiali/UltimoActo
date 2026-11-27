using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UltimoActo.Core;
using UltimoActo.Narrative;
using UltimoActo.Network;
using UltimoActo.Player;

namespace UltimoActo.UI
{
    /// <summary>
    /// 2) Interfaz del Diario / Archivo de Evidencias Compartido ([J] o [TAB]):
    /// - Muestra las 5 pestañas:
    ///   1. Documentos
    ///   2. Pistas de Puzles (incluye soluciones de puzles resueltos y logros cooperativos)
    ///   3. Fotografías
    ///   4. Partituras / Música
    ///   5. Notas Propias (con campo para escribir notas cortas que ve también el compañero en tiempo real)
    /// - Cada entrada muestra quién la encontró, en qué noche, en qué época se vio y si es
    ///   "SOLO VISIBLE EN 1929" o "SOLO VISIBLE EN RUINAS".
    /// - Permite previsualizar el texto o pulsar "ABRIR EN VISOR COMPLETO" reutilizando DocumentUI.
    /// </summary>
    public class EvidenceJournalUI : MonoBehaviour
    {
        public static EvidenceJournalUI Instance { get; private set; }

        public bool IsOpen => isOpen;
        public int LastClosedFrame { get; private set; } = -1;

        private bool isOpen;
        private JournalTabCategory currentTab = JournalTabCategory.Documents;
        private int currentPage = 0;
        private const int EntriesPerPage = 7;

        private GameObject journalRootPanel;
        private readonly Button[] tabButtons = new Button[6];
        private readonly TextMeshProUGUI[] tabButtonLabels = new TextMeshProUGUI[6];
        private readonly Button[] entrySlotButtons = new Button[EntriesPerPage];
        private readonly TextMeshProUGUI[] entrySlotTitles = new TextMeshProUGUI[EntriesPerPage];
        private readonly TextMeshProUGUI[] entrySlotSubtitles = new TextMeshProUGUI[EntriesPerPage];

        private TextMeshProUGUI pageCounterLabel;
        private TextMeshProUGUI detailTitleLabel;
        private TextMeshProUGUI detailMetadataLabel;
        private TextMeshProUGUI detailVisibilityBadgeLabel;
        private TextMeshProUGUI detailBodyLabel;
        private Button openFullViewerButton;

        // Sección de Notas Propias compartidas
        private GameObject personalNoteComposerBox;
        private TMP_InputField personalNoteInput;
        private TextMeshProUGUI saveStatusFooterLabel;

        // BLOQUE 2 — Vitrina visual de Coleccionables y Secretos por Sección dentro del Diario
        private GameObject collectiblesFilterBox;
        private TextMeshProUGUI collectiblesProgressHeaderLabel;
        private readonly Button[] sectionFilterButtons = new Button[6];
        private readonly TextMeshProUGUI[] sectionFilterLabels = new TextMeshProUGUI[6];
        private int activeCollectibleSectionFilter = 0; // 0=Todas, 1=Secc 1-3 (Noche I), 2=Secc 4-5, 3=Secc 6-8, 4=Secc 9-12, 5=Solo Secretos
        private CollectibleDefinition selectedCollectibleDef;

        private EvidenceJournalEntry selectedEntry;
        private PlayerMovement cachedPlayerMovement;
        private PlayerCameraLook cachedPlayerLook;

        // Paleta Gran Ópera Vesper
        private static readonly Color ColBackdrop = new Color(0.03f, 0.02f, 0.04f, 0.88f);
        private static readonly Color ColCardBg = new Color(0.08f, 0.04f, 0.06f, 0.98f);
        private static readonly Color ColGoldBorder = new Color(0.85f, 0.71f, 0.41f, 0.88f);
        private static readonly Color ColGoldBright = new Color(0.95f, 0.84f, 0.58f, 1f);
        private static readonly Color ColIvoryText = new Color(0.96f, 0.94f, 0.90f, 1f);
        private static readonly Color ColMutedRose = new Color(0.76f, 0.67f, 0.65f, 1f);
        private static readonly Color ColTabNormal = new Color(0.13f, 0.06f, 0.08f, 0.96f);
        private static readonly Color ColTabActive = new Color(0.32f, 0.12f, 0.18f, 1f);
        private static readonly Color ColSlotNormal = new Color(0.11f, 0.05f, 0.07f, 0.94f);
        private static readonly Color ColSlotSelected = new Color(0.26f, 0.10f, 0.15f, 0.98f);

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
            if (FindFirstObjectByType<EvidenceJournalUI>() != null) return;
            if (FindFirstObjectByType<PlayerMovement>() == null) return;

            GameObject go = new GameObject("EvidenceJournalUI");
            go.AddComponent<EvidenceJournalUI>();
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
            cachedPlayerMovement = FindFirstObjectByType<PlayerMovement>();
            cachedPlayerLook = FindFirstObjectByType<PlayerCameraLook>();

            BuildJournalCanvas();

            if (SharedEvidenceJournalManager.Instance != null)
            {
                SharedEvidenceJournalManager.Instance.OnJournalUpdated += OnJournalDataUpdated;
            }
            if (CollectibleAndSecretManager.Instance != null)
            {
                CollectibleAndSecretManager.Instance.OnCollectionUpdated += OnJournalDataUpdated;
            }
        }

        private void OnDestroy()
        {
            if (SharedEvidenceJournalManager.Instance != null)
            {
                SharedEvidenceJournalManager.Instance.OnJournalUpdated -= OnJournalDataUpdated;
            }
            if (CollectibleAndSecretManager.Instance != null)
            {
                CollectibleAndSecretManager.Instance.OnCollectionUpdated -= OnJournalDataUpdated;
            }
            if (Instance == this) Instance = null;
        }

        private void OnJournalDataUpdated()
        {
            if (isOpen)
            {
                RefreshJournalView();
            }
        }

        private void Update()
        {
            // No procesar [J] o [TAB] si el jugador está escribiendo en un TMP_InputField
            if (personalNoteInput != null && personalNoteInput.isFocused)
            {
                bool escWhileTyping = false;
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    escWhileTyping = true;
                }
#else
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    escWhileTyping = true;
                }
#endif
                if (escWhileTyping)
                {
                    CloseJournal();
                }
                return;
            }

            bool togglePressed = false;
            bool escPressed = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.jKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame)
                {
                    togglePressed = true;
                }
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    escPressed = true;
                }
            }
#else
            if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Tab))
            {
                togglePressed = true;
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                escPressed = true;
            }
#endif

            if (escPressed && isOpen)
            {
                if (DocumentUI.Instance != null && (DocumentUI.Instance.IsOpen || DocumentUI.Instance.LastClosedFrame == Time.frameCount))
                {
                    return;
                }
                CloseJournal();
                return;
            }

            if (togglePressed)
            {
                if (InGamePauseMenuUI.Instance != null && InGamePauseMenuUI.Instance.IsOpen) return;
                if (CoopDisconnectRecoveryUI.Instance != null && CoopDisconnectRecoveryUI.Instance.IsRecoveryModalOpen) return;
                if (DocumentUI.Instance != null && DocumentUI.Instance.IsOpen)
                {
                    DocumentUI.Instance.CloseDocument();
                }

                if (isOpen)
                {
                    CloseJournal();
                }
                else
                {
                    OpenJournal();
                }
            }
        }

        public void OpenJournal()
        {
            if (journalRootPanel == null) return;

            isOpen = true;
            journalRootPanel.SetActive(true);

            if (cachedPlayerMovement == null) cachedPlayerMovement = FindFirstObjectByType<PlayerMovement>();
            if (cachedPlayerLook == null) cachedPlayerLook = FindFirstObjectByType<PlayerCameraLook>();

            if (cachedPlayerMovement != null) cachedPlayerMovement.enabled = false;
            if (cachedPlayerLook != null)
            {
                cachedPlayerLook.SetCursorLock(false);
                cachedPlayerLook.enabled = false;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            RefreshJournalView();
        }

        public void CloseJournal()
        {
            if (journalRootPanel == null) return;

            isOpen = false;
            LastClosedFrame = Time.frameCount;
            journalRootPanel.SetActive(false);

            if (cachedPlayerMovement == null) cachedPlayerMovement = FindFirstObjectByType<PlayerMovement>();
            if (cachedPlayerLook == null) cachedPlayerLook = FindFirstObjectByType<PlayerCameraLook>();

            if (cachedPlayerMovement != null) cachedPlayerMovement.enabled = true;
            if (cachedPlayerLook != null)
            {
                cachedPlayerLook.enabled = true;
                cachedPlayerLook.SetCursorLock(true);
            }
        }

        public void SelectTab(JournalTabCategory category)
        {
            currentTab = category;
            currentPage = 0;
            selectedEntry = null;
            selectedCollectibleDef = null;
            RefreshJournalView();
        }

        private void RefreshJournalView()
        {
            SharedEvidenceJournalManager mgr = SharedEvidenceJournalManager.Instance;
            if (mgr == null) return;

            CollectibleAndSecretManager colMgr = CollectibleAndSecretManager.Instance;
            int colUnlocked = colMgr != null ? colMgr.UnlockedCollectiblesCount : 0;
            int colGoal = colMgr != null ? colMgr.TotalCollectiblesGoal : 32;
            int secUnlocked = colMgr != null ? colMgr.UnlockedSecretsCount : 0;
            int secGoal = colMgr != null ? colMgr.TotalSecretsGoal : 8;

            // Actualizar contadores en las 6 pestañas
            string[] tabBaseNames =
            {
                "DOCUMENTOS",
                "PISTAS Y PUZLES",
                "FOTOGRAFÍAS",
                "PARTITURAS",
                "NOTAS PROPIAS",
                "COLECCIÓN"
            };

            for (int t = 0; t < 6; t++)
            {
                JournalTabCategory cat = (JournalTabCategory)t;
                string labelText;
                if (cat == JournalTabCategory.CollectiblesAndSecrets)
                {
                    labelText = $"COLECCIÓN ({colUnlocked}/{colGoal} · ★{secUnlocked}/{secGoal})";
                }
                else
                {
                    int count = mgr.GetEntriesForTab(cat).Count;
                    labelText = $"{tabBaseNames[t]} ({count})";
                }

                if (tabButtonLabels[t] != null)
                {
                    tabButtonLabels[t].text = labelText;
                    tabButtonLabels[t].color = (cat == currentTab) ? ColGoldBright : ColIvoryText;
                }
                if (tabButtons[t] != null && tabButtons[t].targetGraphic != null)
                {
                    tabButtons[t].targetGraphic.color = (cat == currentTab) ? ColTabActive : ColTabNormal;
                }
            }

            if (personalNoteComposerBox != null)
            {
                personalNoteComposerBox.SetActive(currentTab == JournalTabCategory.PersonalNotes);
            }
            if (collectiblesFilterBox != null)
            {
                collectiblesFilterBox.SetActive(currentTab == JournalTabCategory.CollectiblesAndSecrets);
            }

            if (currentTab == JournalTabCategory.CollectiblesAndSecrets)
            {
                RefreshCollectiblesAndSecretsShowcase(colMgr, mgr);
                return;
            }

            List<EvidenceJournalEntry> tabEntries = mgr.GetEntriesForTab(currentTab);
            int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)tabEntries.Count / EntriesPerPage));
            currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);

            if (pageCounterLabel != null)
            {
                pageCounterLabel.text = $"Página {currentPage + 1} / {totalPages}  ({tabEntries.Count} registros)";
            }

            if (selectedEntry == null && tabEntries.Count > 0)
            {
                selectedEntry = tabEntries[0];
            }

            int startIndex = currentPage * EntriesPerPage;
            for (int i = 0; i < EntriesPerPage; i++)
            {
                int entryIdx = startIndex + i;
                if (entryIdx < tabEntries.Count)
                {
                    EvidenceJournalEntry e = tabEntries[entryIdx];
                    entrySlotButtons[i].gameObject.SetActive(true);

                    bool isSelected = selectedEntry != null && selectedEntry.entryId == e.entryId;
                    if (entrySlotButtons[i].targetGraphic != null)
                    {
                        entrySlotButtons[i].targetGraphic.color = isSelected ? ColSlotSelected : ColSlotNormal;
                    }

                    string prefix = e.isAchievement ? "★ " : (e.isPuzzleSolution ? "✓ " : "• ");
                    entrySlotTitles[i].text = prefix + e.title;
                    entrySlotTitles[i].color = e.isAchievement ? ColGoldBright : ColIvoryText;

                    string visShort = e.eraVisibilityRule == (int)EraVisibilityMode.Only1929
                        ? "<color=#F5D488>[Solo 1929]</color>"
                        : (e.eraVisibilityRule == (int)EraVisibilityMode.OnlyRuins
                            ? "<color=#92C2F4>[Solo Ruinas]</color>"
                            : "<color=#B8B0A4>[Ambas]</color>");

                    entrySlotSubtitles[i].text = $"Por: <b>{e.foundByPlayer}</b> · {e.nightLabel} · {visShort}";

                    int capturedIdx = entryIdx;
                    entrySlotButtons[i].onClick.RemoveAllListeners();
                    entrySlotButtons[i].onClick.AddListener(() =>
                    {
                        selectedEntry = tabEntries[capturedIdx];
                        RefreshJournalView();
                    });
                }
                else
                {
                    entrySlotButtons[i].gameObject.SetActive(false);
                }
            }

            RefreshSelectedEntryDetail();

            if (saveStatusFooterLabel != null)
            {
                bool isClient = LanSessionManager.Instance != null && LanSessionManager.Instance.CurrentMode == LanSessionMode.Client;
                int totalCount = mgr.AllEntries.Count;
                int achCount = mgr.UnlockedAchievements.Count;
                saveStatusFooterLabel.text = isClient
                    ? $"DIARIO COMPARTIDO EN RED ({totalCount} entradas · {colUnlocked}/{colGoal} coleccionables · ★{secUnlocked}/{secGoal} secretos) · Sincronizado con el Anfitrión"
                    : $"DIARIO COMPARTIDO ({totalCount} entradas · {colUnlocked}/{colGoal} coleccionables · ★{secUnlocked}/{secGoal} secretos) · Guardado en el Anfitrión";
            }
        }

        private void RefreshCollectiblesAndSecretsShowcase(CollectibleAndSecretManager colMgr, SharedEvidenceJournalManager journalMgr)
        {
            if (colMgr == null) return;

            for (int f = 0; f < sectionFilterButtons.Length; f++)
            {
                bool active = f == activeCollectibleSectionFilter;
                if (sectionFilterLabels[f] != null)
                {
                    sectionFilterLabels[f].color = active ? ColGoldBright : ColIvoryText;
                }
                if (sectionFilterButtons[f] != null && sectionFilterButtons[f].targetGraphic != null)
                {
                    sectionFilterButtons[f].targetGraphic.color = active ? ColTabActive : ColTabNormal;
                }
            }

            if (collectiblesProgressHeaderLabel != null)
            {
                collectiblesProgressHeaderLabel.text =
                    $"VITRINA POR SECCIÓN — Coleccionables: <color=#F3D692><b>{colMgr.UnlockedCollectiblesCount}/{colMgr.TotalCollectiblesGoal}</b></color>   ·   " +
                    $"Secretos Especiales: <color=#6EE88E><b>★ {colMgr.UnlockedSecretsCount}/{colMgr.TotalSecretsGoal}</b></color>   " +
                    "<i>(Las piezas que faltan muestran pistas vagas sin revelarse en el mapa)</i>";
            }

            List<CollectibleDefinition> filtered = new List<CollectibleDefinition>();
            var full = colMgr.FullCatalog;
            for (int i = 0; i < full.Count; i++)
            {
                var d = full[i];
                if (MatchesSectionFilter(d, activeCollectibleSectionFilter))
                {
                    filtered.Add(d);
                }
            }

            int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)filtered.Count / EntriesPerPage));
            currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);

            if (pageCounterLabel != null)
            {
                pageCounterLabel.text = $"Vitrina {currentPage + 1} / {totalPages}  ({filtered.Count} piezas)";
            }

            if (selectedCollectibleDef == null && filtered.Count > 0)
            {
                selectedCollectibleDef = filtered[0];
            }

            int startIndex = currentPage * EntriesPerPage;
            for (int i = 0; i < EntriesPerPage; i++)
            {
                int idx = startIndex + i;
                if (idx < filtered.Count)
                {
                    CollectibleDefinition d = filtered[idx];
                    bool unlocked = colMgr.IsUnlocked(d.id);
                    entrySlotButtons[i].gameObject.SetActive(true);

                    bool isSelected = selectedCollectibleDef != null && selectedCollectibleDef.id == d.id;
                    if (entrySlotButtons[i].targetGraphic != null)
                    {
                        entrySlotButtons[i].targetGraphic.color = isSelected ? ColSlotSelected : ColSlotNormal;
                    }

                    string eraShort = d.originEra == EraVisibilityMode.Only1929
                        ? "<color=#F5D488>[1929]</color>"
                        : (d.originEra == EraVisibilityMode.OnlyRuins ? "<color=#92C2F4>[Ruinas]</color>" : "<color=#6EE88E>[Coop 1929+Ruinas]</color>");

                    if (unlocked)
                    {
                        CollectibleUnlockRecord rec = colMgr.GetUnlockRecord(d.id);
                        string finder = rec != null ? rec.foundByPlayer : "Descubierto";
                        entrySlotTitles[i].text = (d.isSpecialSecret ? "★ " : "✓ ") + d.title;
                        entrySlotTitles[i].color = d.isSpecialSecret ? ColGoldBright : new Color(0.55f, 0.94f, 0.68f, 1f);
                        entrySlotSubtitles[i].text = $"{CollectibleAndSecretManager.FormatTypeShort(d.type)} · Noche {d.nightRoman} (Secc. {d.sectionIndex}) · Por <b>{finder}</b> {eraShort}";
                    }
                    else
                    {
                        entrySlotTitles[i].text = (d.isSpecialSecret ? "★ ??? — [Secreto Especial por Descubrir]" : "🔒 ??? — [Coleccionable por Descubrir]");
                        entrySlotTitles[i].color = new Color(0.72f, 0.66f, 0.62f, 0.90f);
                        entrySlotSubtitles[i].text = $"{CollectibleAndSecretManager.FormatTypeShort(d.type)} · Noche {d.nightRoman} · {d.sectionName} {eraShort}";
                    }

                    int captured = idx;
                    entrySlotButtons[i].onClick.RemoveAllListeners();
                    entrySlotButtons[i].onClick.AddListener(() =>
                    {
                        selectedCollectibleDef = filtered[captured];
                        RefreshJournalView();
                    });
                }
                else
                {
                    entrySlotButtons[i].gameObject.SetActive(false);
                }
            }

            RefreshSelectedCollectibleDetail(colMgr);
        }

        private static bool MatchesSectionFilter(CollectibleDefinition d, int filterIdx)
        {
            if (d == null) return false;
            switch (filterIdx)
            {
                case 1: return d.sectionIndex >= 1 && d.sectionIndex <= 3; // Secciones 1–3 (Noche I)
                case 2: return d.sectionIndex == 4 || d.sectionIndex == 5 || d.sectionIndex == 9 || d.sectionIndex == 10; // Ensayo, Archivo, Jardín, Enfermería
                case 3: return d.sectionIndex >= 6 && d.sectionIndex <= 8; // Camerinos, Teatro y Bóveda B-00
                case 4: return d.sectionIndex == 11 || d.sectionIndex == 12; // Internado, Planta Alta, Cripta y Exteriores
                case 5: return d.isSpecialSecret; // Solo los 8 Secretos Especiales
                default: return true;
            }
        }

        private void RefreshSelectedCollectibleDetail(CollectibleAndSecretManager colMgr)
        {
            if (selectedCollectibleDef == null || colMgr == null)
            {
                if (detailTitleLabel != null) detailTitleLabel.text = "Selecciona una pieza de la vitrina";
                if (detailMetadataLabel != null) detailMetadataLabel.text = "";
                if (detailVisibilityBadgeLabel != null) detailVisibilityBadgeLabel.text = "";
                if (detailBodyLabel != null) detailBodyLabel.text = "";
                if (openFullViewerButton != null) openFullViewerButton.gameObject.SetActive(false);
                return;
            }

            bool unlocked = colMgr.IsUnlocked(selectedCollectibleDef.id);
            string eraBadge = SharedEvidenceJournalManager.FormatVisibilityBadge((int)selectedCollectibleDef.originEra);

            if (unlocked)
            {
                CollectibleUnlockRecord rec = colMgr.GetUnlockRecord(selectedCollectibleDef.id);
                string finder = rec != null ? rec.foundByPlayer : "Investigador";

                if (detailTitleLabel != null)
                {
                    detailTitleLabel.text = (selectedCollectibleDef.isSpecialSecret ? "★ " : "✓ ") + selectedCollectibleDef.title;
                }
                if (detailMetadataLabel != null)
                {
                    detailMetadataLabel.text =
                        $"Tipo: <color=#F3D692><b>{CollectibleAndSecretManager.FormatTypeShort(selectedCollectibleDef.type)}</b></color>   |   " +
                        $"Ubicación: <b>{selectedCollectibleDef.zoneName} ({selectedCollectibleDef.sectionName})</b>   |   " +
                        $"Hallado por: <b>{finder}</b>";
                }
                if (detailVisibilityBadgeLabel != null)
                {
                    detailVisibilityBadgeLabel.text = $"{eraBadge} · Noche {selectedCollectibleDef.nightRoman} — Pieza registrada en la Colección Compartida";
                }
                if (detailBodyLabel != null)
                {
                    detailBodyLabel.text =
                        $"<b>DESCRIPCIÓN DE LA PIEZA:</b>\n{selectedCollectibleDef.description}\n\n" +
                        "────────────────────────────────────────────────────────\n" +
                        $"<b><color=#F3D692>HISTORIA SECUNDARIA / RECOMPENSA NARRATIVA (BIBLIA NARRATIVA):</color></b>\n" +
                        $"{selectedCollectibleDef.secondaryLoreReward}";
                }
                if (openFullViewerButton != null)
                {
                    openFullViewerButton.gameObject.SetActive(true);
                }
            }
            else
            {
                if (detailTitleLabel != null)
                {
                    detailTitleLabel.text = selectedCollectibleDef.isSpecialSecret
                        ? $"★ SECRETO ESPECIAL NO DESCUBIERTO (Noche {selectedCollectibleDef.nightRoman})"
                        : $"🔒 PIEZA DE COLECCIÓN NO DESCUBIERTA (Noche {selectedCollectibleDef.nightRoman})";
                }
                if (detailMetadataLabel != null)
                {
                    detailMetadataLabel.text =
                        $"Categoría: <color=#F3D692><b>{CollectibleAndSecretManager.FormatTypeShort(selectedCollectibleDef.type)}</b></color>   |   " +
                        $"Sección: <b>{selectedCollectibleDef.sectionName}</b>   |   " +
                        $"Noche: <b>Noche {selectedCollectibleDef.nightRoman}</b>";
                }
                if (detailVisibilityBadgeLabel != null)
                {
                    detailVisibilityBadgeLabel.text = $"{eraBadge} · No se marca en el mapa para preservar la exploración";
                }
                if (detailBodyLabel != null)
                {
                    detailBodyLabel.text =
                        "<b><color=#F3D692>PISTA VAGA PARA ENCONTRAR ESTA PIEZA:</color></b>\n\n" +
                        $"<i>«{selectedCollectibleDef.vagueMissingHint}»</i>\n\n" +
                        "────────────────────────────────────────────────────────\n" +
                        "<color=#B8B0A4>Consejo cooperativo: Recuerda que muchas piezas y secretos solo son visibles en una de las dos épocas (1929 o Ruinas), o requieren que un jugador observe una señal en 1929 y guíe por voz a su compañero en las Ruinas.</color>";
                }
                if (openFullViewerButton != null)
                {
                    openFullViewerButton.gameObject.SetActive(false);
                }
            }
        }

        private void RefreshSelectedEntryDetail()
        {
            if (selectedEntry == null)
            {
                if (detailTitleLabel != null) detailTitleLabel.text = "Ningún registro seleccionado en esta pestaña";
                if (detailMetadataLabel != null)
                {
                    detailMetadataLabel.text = currentTab == JournalTabCategory.PersonalNotes
                        ? "Escribe una nota en el recuadro superior y pulsa 'COMPARTIR NOTA' para que la vea tu compañero."
                        : "Explora la Academia Vesper y examina documentos, pistas, fotografías o partituras ([E]) para que se archiven automáticamente aquí.";
                }
                if (detailVisibilityBadgeLabel != null) detailVisibilityBadgeLabel.text = "";
                if (detailBodyLabel != null) detailBodyLabel.text = "";
                if (openFullViewerButton != null) openFullViewerButton.gameObject.SetActive(false);
                return;
            }

            if (detailTitleLabel != null)
            {
                detailTitleLabel.text = selectedEntry.title;
            }

            if (detailMetadataLabel != null)
            {
                detailMetadataLabel.text =
                    $"Descubierto por: <color=#F3D99B><b>{selectedEntry.foundByPlayer}</b></color>   |   " +
                    $"Noche: <b>{selectedEntry.nightLabel}</b>   |   " +
                    $"Visto en: <b>{selectedEntry.eraSeenLabel}</b>";
            }

            if (detailVisibilityBadgeLabel != null)
            {
                string badge = SharedEvidenceJournalManager.FormatVisibilityBadge(selectedEntry.eraVisibilityRule);
                string advice = selectedEntry.eraVisibilityRule == (int)EraVisibilityMode.Only1929
                    ? " — Pista exclusiva de 1929: comunícasela por voz a tu compañero en las Ruinas."
                    : (selectedEntry.eraVisibilityRule == (int)EraVisibilityMode.OnlyRuins
                        ? " — Pista exclusiva de las Ruinas: comunícasela por voz a tu compañero en 1929."
                        : " — Elemento compartido entre ambas épocas.");
                detailVisibilityBadgeLabel.text = badge + advice;
            }

            if (detailBodyLabel != null)
            {
                detailBodyLabel.text = selectedEntry.bodyContent;
            }

            if (openFullViewerButton != null)
            {
                openFullViewerButton.gameObject.SetActive(true);
            }
        }

        private void OnOpenInDocumentViewerClicked()
        {
            if (DocumentUI.Instance == null) return;

            if (currentTab == JournalTabCategory.CollectiblesAndSecrets)
            {
                if (selectedCollectibleDef == null || CollectibleAndSecretManager.Instance == null) return;
                if (!CollectibleAndSecretManager.Instance.IsUnlocked(selectedCollectibleDef.id)) return;

                string sub = $"{CollectibleAndSecretManager.FormatTypeShort(selectedCollectibleDef.type)} · {selectedCollectibleDef.sectionName} ({selectedCollectibleDef.zoneName}) · Noche {selectedCollectibleDef.nightRoman}";
                string body =
                    $"{selectedCollectibleDef.description}\n\n" +
                    "──────────────────────────────────────────────\n" +
                    $"<b>HISTORIA SECUNDARIA / RECOMPENSA NARRATIVA:</b>\n{selectedCollectibleDef.secondaryLoreReward}";

                DocumentUI.Instance.ShowDocumentWithId(
                    $"collectible_view_{selectedCollectibleDef.id}",
                    selectedCollectibleDef.title,
                    body,
                    sub,
                    skipJournalRecord: true);
                return;
            }

            if (selectedEntry == null) return;

            string fullSubtitle =
                $"{selectedEntry.foundByPlayer} · {selectedEntry.nightLabel} · {selectedEntry.eraSeenLabel} · " +
                SharedEvidenceJournalManager.FormatVisibilityBadge(selectedEntry.eraVisibilityRule);

            DocumentUI.Instance.ShowDocumentWithId(
                selectedEntry.entryId,
                selectedEntry.title,
                selectedEntry.bodyContent,
                fullSubtitle,
                skipJournalRecord: true);
        }

        private void OnAddPersonalNoteClicked()
        {
            if (personalNoteInput == null || SharedEvidenceJournalManager.Instance == null) return;
            string txt = personalNoteInput.text;
            if (string.IsNullOrWhiteSpace(txt)) return;

            SharedEvidenceJournalManager.Instance.AddPersonalNote(txt);
            personalNoteInput.text = "";
            SelectTab(JournalTabCategory.PersonalNotes);
        }

        private void BuildJournalCanvas()
        {
            GameObject canvasObj = new GameObject("EvidenceJournal_Canvas");
            canvasObj.transform.SetParent(transform, false);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 115;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            journalRootPanel = new GameObject("Journal_Backdrop");
            journalRootPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform rootRt = journalRootPanel.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            Image bgImg = journalRootPanel.AddComponent<Image>();
            bgImg.color = ColBackdrop;

            // Marco principal Gran Ópera (1460 x 880)
            GameObject cardBorder = new GameObject("JournalCard_Border");
            cardBorder.transform.SetParent(journalRootPanel.transform, false);
            RectTransform borderRt = cardBorder.AddComponent<RectTransform>();
            borderRt.anchorMin = new Vector2(0.5f, 0.5f);
            borderRt.anchorMax = new Vector2(0.5f, 0.5f);
            borderRt.pivot = new Vector2(0.5f, 0.5f);
            borderRt.sizeDelta = new Vector2(1460f, 880f);

            Image borderImg = cardBorder.AddComponent<Image>();
            borderImg.color = ColGoldBorder;

            GameObject cardFill = new GameObject("JournalCard_Fill");
            cardFill.transform.SetParent(cardBorder.transform, false);
            RectTransform fillRt = cardFill.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(2f, 2f);
            fillRt.offsetMax = new Vector2(-2f, -2f);

            Image fillImg = cardFill.AddComponent<Image>();
            fillImg.color = ColCardBg;

            // Cabecera
            CreateLabel(cardFill.transform, "Eyebrow",
                "ACADEMIA IMPERIAL VESPER · DIARIO Y ARCHIVO COMPARTIDO DE EVIDENCIAS (1929 ↔ RUINAS)",
                13.5f, ColGoldBright, FontStyles.Bold,
                new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(1200f, 22f));

            CreateLabel(cardFill.transform, "Title",
                "REGISTRO COOPERATIVO DE DOCUMENTOS, PISTAS, FOTOGRAFÍAS Y PARTITURAS",
                24f, ColIvoryText, FontStyles.Bold,
                new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(1250f, 34f));

            // Botón cerrar arriba a la derecha
            CreateSmallButton(cardFill.transform, "BtnCloseJournal", "CERRAR [J / TAB / ESC]",
                new Vector2(1f, 1f), new Vector2(-120f, -32f), new Vector2(200f, 38f),
                out _, CloseJournal);

            // Barra de 6 Pestañas (incluyendo COLECCIÓN con contador X/32 y Secretos Y/8)
            string[] defaultTabs =
            {
                "DOCUMENTOS (0)",
                "PISTAS Y PUZLES (0)",
                "FOTOGRAFÍAS (0)",
                "PARTITURAS (0)",
                "NOTAS PROPIAS (0)",
                "COLECCIÓN (0/32)"
            };
            for (int t = 0; t < 6; t++)
            {
                float xOffset = -585f + (t * 234f);
                int capturedTab = t;
                tabButtons[t] = CreateSmallButton(
                    cardFill.transform,
                    $"TabBtn_{t}",
                    defaultTabs[t],
                    new Vector2(0.5f, 1f),
                    new Vector2(xOffset, -94f),
                    new Vector2(226f, 42f),
                    out tabButtonLabels[t],
                    () => SelectTab((JournalTabCategory)capturedTab));
            }

            // Columna Izquierda: Lista paginada de entradas (540 px de ancho)
            GameObject leftCol = new GameObject("LeftListColumn");
            leftCol.transform.SetParent(cardFill.transform, false);
            RectTransform leftRt = leftCol.AddComponent<RectTransform>();
            leftRt.anchorMin = new Vector2(0f, 1f);
            leftRt.anchorMax = new Vector2(0f, 1f);
            leftRt.pivot = new Vector2(0f, 1f);
            leftRt.anchoredPosition = new Vector2(26f, -152f);
            leftRt.sizeDelta = new Vector2(540f, 670f);

            Image leftBg = leftCol.AddComponent<Image>();
            leftBg.color = new Color(0.05f, 0.03f, 0.04f, 0.94f);

            for (int i = 0; i < EntriesPerPage; i++)
            {
                float yPos = -14f - (i * 84f);
                entrySlotButtons[i] = CreateEntrySlotButton(
                    leftCol.transform,
                    $"EntrySlot_{i}",
                    new Vector2(270f, yPos),
                    new Vector2(512f, 76f),
                    out entrySlotTitles[i],
                    out entrySlotSubtitles[i]);
            }

            // Paginación inferior en columna izquierda
            CreateSmallButton(leftCol.transform, "BtnPrevPage", "◀ ANTERIOR",
                new Vector2(0f, 0f), new Vector2(85f, 28f), new Vector2(140f, 34f),
                out _, () =>
                {
                    currentPage = Mathf.Max(0, currentPage - 1);
                    RefreshJournalView();
                });

            pageCounterLabel = CreateLabel(leftCol.transform, "PageCounter", "Página 1 / 1",
                13.5f, ColMutedRose, FontStyles.Bold,
                new Vector2(0.5f, 0f), new Vector2(0f, 38f), new Vector2(220f, 26f));

            CreateSmallButton(leftCol.transform, "BtnNextPage", "SIGUIENTE ▶",
                new Vector2(1f, 0f), new Vector2(-85f, 28f), new Vector2(140f, 34f),
                out _, () =>
                {
                    currentPage++;
                    RefreshJournalView();
                });

            // Columna Derecha: Visor de detalle + Compositor de Notas Propias (845 px de ancho)
            GameObject rightCol = new GameObject("RightDetailColumn");
            rightCol.transform.SetParent(cardFill.transform, false);
            RectTransform rightRt = rightCol.AddComponent<RectTransform>();
            rightRt.anchorMin = new Vector2(0f, 1f);
            rightRt.anchorMax = new Vector2(0f, 1f);
            rightRt.pivot = new Vector2(0f, 1f);
            rightRt.anchoredPosition = new Vector2(586f, -152f);
            rightRt.sizeDelta = new Vector2(845f, 670f);

            Image rightBg = rightCol.AddComponent<Image>();
            rightBg.color = new Color(0.06f, 0.03f, 0.05f, 0.96f);

            // Caja de escritura de Notas Propias (visible en la pestaña Notas Propias y accesible siempre)
            personalNoteComposerBox = new GameObject("PersonalNoteComposerBox");
            personalNoteComposerBox.transform.SetParent(rightCol.transform, false);
            RectTransform noteBoxRt = personalNoteComposerBox.AddComponent<RectTransform>();
            noteBoxRt.anchorMin = new Vector2(0.5f, 1f);
            noteBoxRt.anchorMax = new Vector2(0.5f, 1f);
            noteBoxRt.pivot = new Vector2(0.5f, 1f);
            noteBoxRt.anchoredPosition = new Vector2(0f, -12f);
            noteBoxRt.sizeDelta = new Vector2(815f, 84f);

            Image noteBoxBg = personalNoteComposerBox.AddComponent<Image>();
            noteBoxBg.color = new Color(0.14f, 0.06f, 0.09f, 0.96f);

            CreateLabel(personalNoteComposerBox.transform, "NoteBoxHeader",
                "ESCRIBIR NOTA PERSONAL COMPARTIDA (Tu compañero la verá al instante en su Diario):",
                12.5f, ColGoldBright, FontStyles.Bold,
                new Vector2(0f, 1f), new Vector2(16f, -8f), new Vector2(780f, 20f), TextAlignmentOptions.Left);

            personalNoteInput = CreateNoteInputField(personalNoteComposerBox.transform,
                new Vector2(16f, -32f), new Vector2(585f, 40f));

            CreateSmallButton(personalNoteComposerBox.transform, "BtnAddNote", "COMPARTIR NOTA",
                new Vector2(1f, 1f), new Vector2(-108f, -32f), new Vector2(188f, 40f),
                out _, OnAddPersonalNoteClicked);

            // BLOQUE 2 — Vitrina visual y filtro por Sección (visible en la pestaña COLECCIÓN)
            collectiblesFilterBox = new GameObject("CollectiblesSectionFilterBox");
            collectiblesFilterBox.transform.SetParent(rightCol.transform, false);
            RectTransform colBoxRt = collectiblesFilterBox.AddComponent<RectTransform>();
            colBoxRt.anchorMin = new Vector2(0.5f, 1f);
            colBoxRt.anchorMax = new Vector2(0.5f, 1f);
            colBoxRt.pivot = new Vector2(0.5f, 1f);
            colBoxRt.anchoredPosition = new Vector2(0f, -12f);
            colBoxRt.sizeDelta = new Vector2(815f, 84f);

            Image colBoxBg = collectiblesFilterBox.AddComponent<Image>();
            colBoxBg.color = new Color(0.14f, 0.06f, 0.09f, 0.96f);

            collectiblesProgressHeaderLabel = CreateLabel(collectiblesFilterBox.transform, "ColBoxHeader",
                "VITRINA DE COLECCIONABLES Y SECRETOS POR SECCIÓN:",
                12.2f, ColGoldBright, FontStyles.Bold,
                new Vector2(0f, 1f), new Vector2(14f, -8f), new Vector2(785f, 20f), TextAlignmentOptions.Left);

            string[] filterCaptions =
            {
                "Todas (1-12)",
                "Secc. 1-3 (N. I)",
                "Secc. 4-5 y 9-10",
                "Secc. 6-8 (Teatro)",
                "Secc. 11-12",
                "★ Secretos (8)"
            };
            for (int f = 0; f < 6; f++)
            {
                float fx = -335f + (f * 134f);
                int capturedFilter = f;
                sectionFilterButtons[f] = CreateSmallButton(
                    collectiblesFilterBox.transform,
                    $"SecFilterBtn_{f}",
                    filterCaptions[f],
                    new Vector2(0.5f, 1f),
                    new Vector2(fx, -34f),
                    new Vector2(128f, 38f),
                    out sectionFilterLabels[f],
                    () =>
                    {
                        activeCollectibleSectionFilter = capturedFilter;
                        currentPage = 0;
                        selectedCollectibleDef = null;
                        RefreshJournalView();
                    });
            }
            collectiblesFilterBox.SetActive(false);

            // Cabecera de la entrada seleccionada
            detailTitleLabel = CreateLabel(rightCol.transform, "DetailTitle",
                "Selecciona una evidencia de la lista",
                21f, ColGoldBright, FontStyles.Bold,
                new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(795f, 34f), TextAlignmentOptions.Left);

            detailMetadataLabel = CreateLabel(rightCol.transform, "DetailMetadata",
                "", 14f, ColIvoryText, FontStyles.Normal,
                new Vector2(0.5f, 1f), new Vector2(0f, -148f), new Vector2(795f, 24f), TextAlignmentOptions.Left);

            detailVisibilityBadgeLabel = CreateLabel(rightCol.transform, "DetailVisibilityBadge",
                "", 14f, ColMutedRose, FontStyles.Bold,
                new Vector2(0.5f, 1f), new Vector2(0f, -176f), new Vector2(795f, 24f), TextAlignmentOptions.Left);

            // Línea divisoria
            GameObject div = new GameObject("DetailDivider");
            div.transform.SetParent(rightCol.transform, false);
            RectTransform divRt = div.AddComponent<RectTransform>();
            divRt.anchorMin = new Vector2(0.5f, 1f);
            divRt.anchorMax = new Vector2(0.5f, 1f);
            divRt.pivot = new Vector2(0.5f, 0.5f);
            divRt.anchoredPosition = new Vector2(0f, -208f);
            divRt.sizeDelta = new Vector2(795f, 1.5f);
            Image divImg = div.AddComponent<Image>();
            divImg.color = new Color(0.85f, 0.71f, 0.41f, 0.40f);

            // Texto completo de la evidencia
            detailBodyLabel = CreateLabel(rightCol.transform, "DetailBody",
                "", 16f, new Color(0.93f, 0.91f, 0.87f, 1f), FontStyles.Normal,
                new Vector2(0.5f, 1f), new Vector2(0f, -222f), new Vector2(795f, 375f), TextAlignmentOptions.TopLeft);
            detailBodyLabel.lineSpacing = 8f;
            detailBodyLabel.overflowMode = TextOverflowModes.Truncate;

            // Botón para abrir en el visor de documentos completo (reutilizando DocumentUI)
            openFullViewerButton = CreateSmallButton(rightCol.transform, "BtnOpenFullViewer",
                "ABRIR EN VISOR DE DOCUMENTOS COMPLETO",
                new Vector2(0.5f, 0f), new Vector2(0f, 52f), new Vector2(460f, 42f),
                out _, OnOpenInDocumentViewerClicked);

            // Pie de página con estado de sincronización y guardado del Host
            saveStatusFooterLabel = CreateLabel(cardFill.transform, "FooterSaveStatus",
                "DIARIO COMPARTIDO · Pulsa [J] o [TAB] en cualquier momento para consultar pistas entre épocas.",
                13.5f, ColMutedRose, FontStyles.Italic,
                new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(1380f, 24f));

            journalRootPanel.SetActive(false);
        }

        private static Button CreateEntrySlotButton(
            Transform parent,
            string name,
            Vector2 anchoredPos,
            Vector2 size,
            out TextMeshProUGUI titleOut,
            out TextMeshProUGUI subtitleOut)
        {
            GameObject borderObj = new GameObject(name);
            borderObj.transform.SetParent(parent, false);
            RectTransform rt = borderObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image borderImg = borderObj.AddComponent<Image>();
            borderImg.color = new Color(0.85f, 0.71f, 0.41f, 0.45f);

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(borderObj.transform, false);
            RectTransform fillRt = fillObj.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(1.5f, 1.5f);
            fillRt.offsetMax = new Vector2(-1.5f, -1.5f);

            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.color = ColSlotNormal;

            Button btn = borderObj.AddComponent<Button>();
            btn.targetGraphic = fillImg;

            titleOut = CreateLabel(fillObj.transform, "SlotTitle", "",
                15f, ColIvoryText, FontStyles.Bold,
                new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(size.x - 24f, 28f), TextAlignmentOptions.Left);
            titleOut.textWrappingMode = TextWrappingModes.NoWrap;
            titleOut.overflowMode = TextOverflowModes.Ellipsis;

            subtitleOut = CreateLabel(fillObj.transform, "SlotSub", "",
                12.5f, ColMutedRose, FontStyles.Normal,
                new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(size.x - 24f, 24f), TextAlignmentOptions.Left);
            subtitleOut.textWrappingMode = TextWrappingModes.NoWrap;
            subtitleOut.overflowMode = TextOverflowModes.Ellipsis;

            return btn;
        }

        private static TMP_InputField CreateNoteInputField(Transform parent, Vector2 anchoredPos, Vector2 size)
        {
            GameObject root = new GameObject("NoteInput");
            root.transform.SetParent(parent, false);
            RectTransform rt = root.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image bg = root.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.02f, 0.03f, 0.96f);

            GameObject textArea = new GameObject("TextArea");
            textArea.transform.SetParent(root.transform, false);
            RectTransform areaRt = textArea.AddComponent<RectTransform>();
            areaRt.anchorMin = Vector2.zero;
            areaRt.anchorMax = Vector2.one;
            areaRt.offsetMin = new Vector2(10f, 4f);
            areaRt.offsetMax = new Vector2(-10f, -4f);

            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(textArea.transform, false);
            RectTransform phRt = placeholderObj.AddComponent<RectTransform>();
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;
            phRt.offsetMin = Vector2.zero;
            phRt.offsetMax = Vector2.zero;

            TextMeshProUGUI phTmp = placeholderObj.AddComponent<TextMeshProUGUI>();
            phTmp.text = "Ej: 'En 1929 el emblema del pórtico marca Cisne Dorado - Lira de Bronce…'";
            phTmp.fontSize = 13.5f;
            phTmp.fontStyle = FontStyles.Italic;
            phTmp.color = new Color(0.65f, 0.58f, 0.56f, 0.75f);
            phTmp.alignment = TextAlignmentOptions.MidlineLeft;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(textArea.transform, false);
            RectTransform txtRt = textObj.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;

            TextMeshProUGUI txtTmp = textObj.AddComponent<TextMeshProUGUI>();
            txtTmp.fontSize = 14f;
            txtTmp.color = ColIvoryText;
            txtTmp.alignment = TextAlignmentOptions.MidlineLeft;

            TMP_InputField input = root.AddComponent<TMP_InputField>();
            input.textViewport = areaRt;
            input.textComponent = txtTmp;
            input.placeholder = phTmp;
            input.characterLimit = 220;
            return input;
        }

        private static TextMeshProUGUI CreateLabel(
            Transform parent,
            string name,
            string text,
            float fontSize,
            Color color,
            FontStyles style,
            Vector2 anchor,
            Vector2 anchoredPos,
            Vector2 size,
            TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(anchor.x, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button CreateSmallButton(
            Transform parent,
            string name,
            string caption,
            Vector2 anchor,
            Vector2 anchoredPos,
            Vector2 size,
            out TextMeshProUGUI labelOut,
            UnityEngine.Events.UnityAction onClick)
        {
            GameObject borderObj = new GameObject(name);
            borderObj.transform.SetParent(parent, false);
            RectTransform rt = borderObj.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image borderImg = borderObj.AddComponent<Image>();
            borderImg.color = ColGoldBorder;

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(borderObj.transform, false);
            RectTransform fillRt = fillObj.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(1.5f, 1.5f);
            fillRt.offsetMax = new Vector2(-1.5f, -1.5f);

            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.color = ColTabNormal;

            Button btn = borderObj.AddComponent<Button>();
            btn.targetGraphic = fillImg;
            if (onClick != null) btn.onClick.AddListener(onClick);

            labelOut = CreateLabel(fillObj.transform, "Label", caption,
                13.5f, ColIvoryText, FontStyles.Bold,
                new Vector2(0.5f, 1f), new Vector2(0f, -6f), new Vector2(size.x - 12f, size.y - 10f));

            return btn;
        }
    }
}
