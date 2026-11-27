using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ULTIMOACTO_NETCODE
using Unity.Collections;
using Unity.Netcode;
#endif
using UltimoActo.Core;
using UltimoActo.Interaction;
using UltimoActo.Narrative;
using UltimoActo.Puzzles;

namespace UltimoActo.Network
{
    /// <summary>
    /// Paso 4 — Estado Compartido con Autoridad del Host:
    /// Sincroniza en tiempo real entre el Anfitrión (Época 1929) y el Cliente (Época Ruinas):
    ///  1. Puertas simples (WorldDoor) y puertas dobles (WorldDoubleDoor): apertura, cierre y desbloqueo por llave.
    ///  2. Llaves, fusibles, objetos recogidos (WorldItem) e inventario compartido (SimpleInventory).
    ///  3. Documentos leídos y evidencias registradas (WorldDocument / DocumentUI / NightManager).
    ///  4. Interruptores y Puzles de las 8 Noches sin alterar su lógica interna:
    ///     - AcademyFuseBoxController (fusibles 10A/15A/25A y palanca de corriente 220V).
    ///     - PortraitGalleryPuzzleController (4 diales estacionales y apertura del retrato de Elena).
    ///     - OrderedSequencePuzzle (todos los puzles de secuencia ordenada de Noche I a Noche VIII).
    ///     - TheatreClimaxController (las 5 fases cooperativas del Gran Teatro).
    ///     - Nodos interactivos de Backstage, Archivo Musical, Zona Secreta, Jardín de Invierno y Expansión.
    ///  5. NightManager (noche actual, cuota compartida de evidencias, puertas por noche y guardado en el Host).
    /// </summary>
    public class SharedWorldStateSync : MonoBehaviour
    {
        public static SharedWorldStateSync Instance { get; private set; }

        public const string MsgClientActionToHost = "UltimoActo_SharedState_C2S";
        public const string MsgHostBroadcastToClients = "UltimoActo_SharedState_S2C";
        public const string MsgClientRequestSnapshot = "UltimoActo_SharedState_ReqSnap";

        private readonly Dictionary<string, WorldDoor> doorsById = new Dictionary<string, WorldDoor>();
        private readonly Dictionary<string, WorldDoubleDoor> doubleDoorsById = new Dictionary<string, WorldDoubleDoor>();
        private readonly Dictionary<string, WorldItem> itemsById = new Dictionary<string, WorldItem>();
        private readonly Dictionary<string, OrderedSequencePuzzle> seqPuzzlesById = new Dictionary<string, OrderedSequencePuzzle>();
        private readonly Dictionary<string, BackstageAnomalyController> backstageById = new Dictionary<string, BackstageAnomalyController>();
        private readonly Dictionary<string, MusicArchiveAnomalyController> musicById = new Dictionary<string, MusicArchiveAnomalyController>();
        private readonly Dictionary<string, SecretZoneAnomalyController> secretById = new Dictionary<string, SecretZoneAnomalyController>();
        private readonly Dictionary<string, WinterGardenAnomalyController> gardenById = new Dictionary<string, WinterGardenAnomalyController>();
        private readonly Dictionary<string, AcademyExpansionAnomalyController> expansionById = new Dictionary<string, AcademyExpansionAnomalyController>();
        private readonly Dictionary<string, CoopPuzzleBase> coopPuzzlesById = new Dictionary<string, CoopPuzzleBase>();

        private readonly HashSet<string> pickedItemIds = new HashSet<string>();
        private bool isApplyingRemoteState = false;
        private bool handlersRegistered = false;

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
            if (FindFirstObjectByType<SharedWorldStateSync>() != null) return;
            if (FindFirstObjectByType<UltimoActo.Player.PlayerMovement>() == null) return;

            GameObject go = new GameObject("SharedWorldStateSync_HostAuthority");
            go.AddComponent<SharedWorldStateSync>();
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
            IndexSceneInteractiveObjects();
            SubscribeLocalWorldEvents();
            TryRegisterNetcodeMessagingHandlers();

            if (LanSessionManager.Instance != null)
            {
                LanSessionManager.Instance.OnRoomOpenedInGame += OnRoomOpenedInGameHandler;
                LanSessionManager.Instance.OnConnectedPlayerCountChanged += OnPlayerCountChangedHandler;
            }
        }

        private void Update()
        {
            if (!handlersRegistered)
            {
                TryRegisterNetcodeMessagingHandlers();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeLocalWorldEvents();
            UnregisterNetcodeMessagingHandlers();

            if (LanSessionManager.Instance != null)
            {
                LanSessionManager.Instance.OnRoomOpenedInGame -= OnRoomOpenedInGameHandler;
                LanSessionManager.Instance.OnConnectedPlayerCountChanged -= OnPlayerCountChangedHandler;
            }
            if (Instance == this) Instance = null;
        }

        public void IndexSceneInteractiveObjects()
        {
            doorsById.Clear();
            foreach (WorldDoor d in FindObjectsByType<WorldDoor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (d == null) continue;
                string key = GetUniqueObjectKey(d.Id, d.transform);
                doorsById[key] = d;
                if (!string.IsNullOrEmpty(d.Id) && !doorsById.ContainsKey(d.Id))
                {
                    doorsById[d.Id] = d;
                }
            }

            doubleDoorsById.Clear();
            foreach (WorldDoubleDoor dd in FindObjectsByType<WorldDoubleDoor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (dd == null) continue;
                string key = GetUniqueObjectKey(dd.Id, dd.transform);
                doubleDoorsById[key] = dd;
                if (!string.IsNullOrEmpty(dd.Id) && !doubleDoorsById.ContainsKey(dd.Id))
                {
                    doubleDoorsById[dd.Id] = dd;
                }
            }

            itemsById.Clear();
            foreach (WorldItem item in FindObjectsByType<WorldItem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (item == null) continue;
                string key = GetUniqueObjectKey(item.Id, item.transform);
                itemsById[key] = item;
                if (!string.IsNullOrEmpty(item.Id) && !itemsById.ContainsKey(item.Id))
                {
                    itemsById[item.Id] = item;
                }
            }

            seqPuzzlesById.Clear();
            foreach (OrderedSequencePuzzle p in FindObjectsByType<OrderedSequencePuzzle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (p == null || string.IsNullOrEmpty(p.PuzzleId)) continue;
                seqPuzzlesById[p.PuzzleId] = p;
            }

            backstageById.Clear();
            foreach (BackstageAnomalyController b in FindObjectsByType<BackstageAnomalyController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (b == null) continue;
                backstageById[b.Id] = b;
            }

            musicById.Clear();
            foreach (MusicArchiveAnomalyController m in FindObjectsByType<MusicArchiveAnomalyController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (m == null) continue;
                musicById[m.Id] = m;
            }

            secretById.Clear();
            foreach (SecretZoneAnomalyController s in FindObjectsByType<SecretZoneAnomalyController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (s == null) continue;
                secretById[s.Id] = s;
            }

            gardenById.Clear();
            foreach (WinterGardenAnomalyController w in FindObjectsByType<WinterGardenAnomalyController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (w == null) continue;
                gardenById[w.Id] = w;
            }

            expansionById.Clear();
            foreach (AcademyExpansionAnomalyController e in FindObjectsByType<AcademyExpansionAnomalyController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (e == null) continue;
                expansionById[e.Id] = e;
            }

            coopPuzzlesById.Clear();
            foreach (CoopPuzzleBase cp in FindObjectsByType<CoopPuzzleBase>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (cp == null || string.IsNullOrEmpty(cp.PuzzleId)) continue;
                coopPuzzlesById[cp.PuzzleId] = cp;
            }
        }

        private static string GetUniqueObjectKey(string baseId, Transform t)
        {
            Vector3 p = t.position;
            return $"{baseId}@{Mathf.RoundToInt(p.x * 10f)}_{Mathf.RoundToInt(p.y * 10f)}_{Mathf.RoundToInt(p.z * 10f)}";
        }

        private void SubscribeLocalWorldEvents()
        {
            WorldDoor.OnDoorStateChanged += HandleLocalDoorChanged;
            WorldDoubleDoor.OnDoubleDoorStateChanged += HandleLocalDoubleDoorChanged;
            WorldItem.OnWorldItemPickedUp += HandleLocalItemPickedUp;
            SimpleInventory.OnGlobalKeyAdded += HandleLocalKeyAdded;
            SimpleInventory.OnGlobalKeyRemoved += HandleLocalKeyRemoved;
            AcademyFuseBoxController.OnFuseSlotInstalled += HandleLocalFuseInstalled;
            AcademyFuseBoxController.OnPowerRestoredEvent += HandleLocalPowerRestored;
            PortraitGalleryPuzzleController.OnDialRotated += HandleLocalPortraitDialRotated;
            OrderedSequencePuzzle.OnAnySequencePuzzleUpdated += HandleLocalSequencePuzzleUpdated;
            TheatreClimaxController.OnClimaxPhaseActivated += HandleLocalTheatrePhaseActivated;
            BackstageAnomalyController.OnBackstageNodeResolved += HandleLocalBackstageResolved;
            MusicArchiveAnomalyController.OnMusicArchiveNodeCompleted += HandleLocalMusicCompleted;
            SecretZoneAnomalyController.OnSecretZoneNodeActivated += HandleLocalSecretActivated;
            WinterGardenAnomalyController.OnWinterGardenNodeInspected += HandleLocalGardenInspected;
            AcademyExpansionAnomalyController.OnExpansionNodeInteracted += HandleLocalExpansionInteracted;
            SharedEvidenceJournalManager.OnLocalEntryCreatedForNetwork += HandleLocalJournalEntryCreated;
            CoopPuzzleBase.OnCoopPuzzleNetworkCommand += HandleLocalCoopPuzzleCommand;
            AsymmetricAnomalyManager.OnAnomalyTriggeredForNetwork += HandleLocalAnomalyTriggered;
            UltimateFinaleAndCreditsController.OnHostBroadcastFinalePacket += HandleHostBroadcastFinalePacket;
            UltimoActo.UI.AcademyMapUI.OnLocalZoneVisitedForNetwork += HandleLocalMapZoneVisited;
            CollectibleAndSecretManager.OnCollectibleNetworkEvent += HandleLocalCollectibleEvent;

            if (NightManager.Instance != null)
            {
                NightManager.Instance.OnEvidenceIdRegistered += HandleLocalEvidenceRegistered;
                NightManager.Instance.OnNightStarted += HandleLocalNightStarted;
            }
        }

        private void UnsubscribeLocalWorldEvents()
        {
            WorldDoor.OnDoorStateChanged -= HandleLocalDoorChanged;
            WorldDoubleDoor.OnDoubleDoorStateChanged -= HandleLocalDoubleDoorChanged;
            WorldItem.OnWorldItemPickedUp -= HandleLocalItemPickedUp;
            SimpleInventory.OnGlobalKeyAdded -= HandleLocalKeyAdded;
            SimpleInventory.OnGlobalKeyRemoved -= HandleLocalKeyRemoved;
            AcademyFuseBoxController.OnFuseSlotInstalled -= HandleLocalFuseInstalled;
            AcademyFuseBoxController.OnPowerRestoredEvent -= HandleLocalPowerRestored;
            PortraitGalleryPuzzleController.OnDialRotated -= HandleLocalPortraitDialRotated;
            OrderedSequencePuzzle.OnAnySequencePuzzleUpdated -= HandleLocalSequencePuzzleUpdated;
            TheatreClimaxController.OnClimaxPhaseActivated -= HandleLocalTheatrePhaseActivated;
            BackstageAnomalyController.OnBackstageNodeResolved -= HandleLocalBackstageResolved;
            MusicArchiveAnomalyController.OnMusicArchiveNodeCompleted -= HandleLocalMusicCompleted;
            SecretZoneAnomalyController.OnSecretZoneNodeActivated -= HandleLocalSecretActivated;
            WinterGardenAnomalyController.OnWinterGardenNodeInspected -= HandleLocalGardenInspected;
            AcademyExpansionAnomalyController.OnExpansionNodeInteracted -= HandleLocalExpansionInteracted;
            SharedEvidenceJournalManager.OnLocalEntryCreatedForNetwork -= HandleLocalJournalEntryCreated;
            CoopPuzzleBase.OnCoopPuzzleNetworkCommand -= HandleLocalCoopPuzzleCommand;
            AsymmetricAnomalyManager.OnAnomalyTriggeredForNetwork -= HandleLocalAnomalyTriggered;
            UltimateFinaleAndCreditsController.OnHostBroadcastFinalePacket -= HandleHostBroadcastFinalePacket;
            UltimoActo.UI.AcademyMapUI.OnLocalZoneVisitedForNetwork -= HandleLocalMapZoneVisited;
            CollectibleAndSecretManager.OnCollectibleNetworkEvent -= HandleLocalCollectibleEvent;

            if (NightManager.Instance != null)
            {
                NightManager.Instance.OnEvidenceIdRegistered -= HandleLocalEvidenceRegistered;
                NightManager.Instance.OnNightStarted -= HandleLocalNightStarted;
            }
        }

        private void OnRoomOpenedInGameHandler()
        {
            TryRegisterNetcodeMessagingHandlers();
        }

        private void OnPlayerCountChangedHandler(int count)
        {
            TryRegisterNetcodeMessagingHandlers();
            if (count >= 2 && IsHostAuthority())
            {
                // Enviar snapshot completo del estado de la partida al 2.º jugador que acaba de unirse
                SendFullSnapshotFromHostToAllClients();
            }
        }

        private static bool IsNetcodeActive()
        {
#if ULTIMOACTO_NETCODE
            return NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
#else
            return false;
#endif
        }

        private static bool IsHostAuthority()
        {
#if ULTIMOACTO_NETCODE
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                return NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost;
            }
#endif
            return LanSessionManager.Instance == null || LanSessionManager.Instance.CurrentMode != LanSessionMode.Client;
        }

        // ====================================================================
        // CAPTURA DE ACCIONES LOCALES -> ENVÍO AL HOST (ServerRpc) O CLIENTES (ClientRpc)
        // ====================================================================
        private void HandleLocalDoorChanged(WorldDoor door)
        {
            if (isApplyingRemoteState || door == null) return;
            string key = GetUniqueObjectKey(door.Id, door.transform);
            DispatchStatePacket($"DOOR|{key}|{(door.IsOpen ? 1 : 0)}|{(door.IsLocked ? 1 : 0)}|{door.Id}");
        }

        private void HandleLocalDoubleDoorChanged(WorldDoubleDoor ddoor)
        {
            if (isApplyingRemoteState || ddoor == null) return;
            string key = GetUniqueObjectKey(ddoor.Id, ddoor.transform);
            DispatchStatePacket($"DDOOR|{key}|{(ddoor.IsOpen ? 1 : 0)}|{(ddoor.IsLocked ? 1 : 0)}|{ddoor.Id}");
        }

        private void HandleLocalItemPickedUp(WorldItem item)
        {
            if (isApplyingRemoteState || item == null) return;
            string key = GetUniqueObjectKey(item.Id, item.transform);
            pickedItemIds.Add(key);
            pickedItemIds.Add(item.Id);
            DispatchStatePacket($"ITEM|{key}|{item.Id}|{Sanitize(item.DisplayName)}");
        }

        private void HandleLocalKeyAdded(string keyId, string displayName)
        {
            if (isApplyingRemoteState || string.IsNullOrEmpty(keyId)) return;
            DispatchStatePacket($"KEY_ADD|{keyId}|{Sanitize(displayName)}");
        }

        private void HandleLocalKeyRemoved(string keyId)
        {
            if (isApplyingRemoteState || string.IsNullOrEmpty(keyId)) return;
            DispatchStatePacket($"KEY_REM|{keyId}");
        }

        private void HandleLocalEvidenceRegistered(string evidenceId)
        {
            if (isApplyingRemoteState || string.IsNullOrEmpty(evidenceId)) return;
            DispatchStatePacket($"EVIDENCE|{Sanitize(evidenceId)}");
        }

        private void HandleLocalNightStarted(NightDefinition def, int nightIdx)
        {
            if (isApplyingRemoteState) return;
            if (!IsHostAuthority()) return;

            int evCount = NightManager.Instance != null ? NightManager.Instance.EvidenceThisNight : 0;
            NightManager.Instance?.HostPersistCurrentNightSave();
            DispatchStatePacket($"NIGHT|{nightIdx}|{evCount}");
        }

        private void HandleLocalFuseInstalled(int slotIdx)
        {
            if (isApplyingRemoteState) return;
            DispatchStatePacket($"FUSE_SLOT|{slotIdx}");
        }

        private void HandleLocalPowerRestored()
        {
            if (isApplyingRemoteState) return;
            DispatchStatePacket("FUSE_POWER|1");
        }

        private void HandleLocalPortraitDialRotated(int portraitIdx, int dialVal)
        {
            if (isApplyingRemoteState) return;
            DispatchStatePacket($"PORTRAIT_DIAL|{portraitIdx}|{dialVal}");
        }

        private void HandleLocalSequencePuzzleUpdated(OrderedSequencePuzzle puzzle, int progress, bool solved)
        {
            if (isApplyingRemoteState || puzzle == null) return;
            DispatchStatePacket($"SEQ_PUZZLE|{puzzle.PuzzleId}|{progress}|{(solved ? 1 : 0)}");
        }

        private void HandleLocalTheatrePhaseActivated(int phaseIndex)
        {
            if (isApplyingRemoteState) return;
            int mask = TheatreClimaxController.GetPhaseMaskSnapshot();
            DispatchStatePacket($"THEATRE_CLIMAX|{mask}");
        }

        private void HandleLocalBackstageResolved(string nodeId)
        {
            if (isApplyingRemoteState || string.IsNullOrEmpty(nodeId)) return;
            DispatchStatePacket($"NODE_BACKSTAGE|{nodeId}");
        }

        private void HandleLocalMusicCompleted(string nodeId)
        {
            if (isApplyingRemoteState || string.IsNullOrEmpty(nodeId)) return;
            DispatchStatePacket($"NODE_MUSIC|{nodeId}");
        }

        private void HandleLocalSecretActivated(string nodeId)
        {
            if (isApplyingRemoteState || string.IsNullOrEmpty(nodeId)) return;
            DispatchStatePacket($"NODE_SECRET|{nodeId}");
        }

        private void HandleLocalGardenInspected(string nodeId)
        {
            if (isApplyingRemoteState || string.IsNullOrEmpty(nodeId)) return;
            DispatchStatePacket($"NODE_GARDEN|{nodeId}");
        }

        private void HandleLocalExpansionInteracted(string nodeId)
        {
            if (isApplyingRemoteState || string.IsNullOrEmpty(nodeId)) return;
            DispatchStatePacket($"NODE_EXPANSION|{nodeId}");
        }

        private void HandleLocalJournalEntryCreated(EvidenceJournalEntry entry)
        {
            if (isApplyingRemoteState || entry == null) return;
            string json = JsonUtility.ToJson(entry);
            string b64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
            DispatchStatePacket($"JOURNAL_ENTRY|{b64}");
        }

        private void HandleLocalCoopPuzzleCommand(string puzzleId, string subPayload)
        {
            if (isApplyingRemoteState || string.IsNullOrEmpty(puzzleId) || string.IsNullOrEmpty(subPayload)) return;
            string b64Payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(subPayload));
            DispatchStatePacket($"COOP_PUZZLE|{puzzleId}|{b64Payload}");
        }

        private void HandleLocalAnomalyTriggered(string anomalyId)
        {
            if (isApplyingRemoteState || string.IsNullOrEmpty(anomalyId)) return;
            DispatchStatePacket($"ANOMALY_TRIG|{anomalyId}");
        }

        private void HandleHostBroadcastFinalePacket(string finalePayload)
        {
            if (isApplyingRemoteState || string.IsNullOrEmpty(finalePayload)) return;
            string b64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(finalePayload));
            DispatchStatePacket($"FINAL_DAWN_START|{b64}");
        }

        private void HandleLocalMapZoneVisited(string zoneId)
        {
            if (isApplyingRemoteState || string.IsNullOrEmpty(zoneId)) return;
            DispatchStatePacket($"MAP_VISIT|{zoneId}");
        }

        private void HandleLocalCollectibleEvent(string collectiblePayload)
        {
            if (isApplyingRemoteState || string.IsNullOrEmpty(collectiblePayload)) return;
            string b64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(collectiblePayload));
            DispatchStatePacket($"COLLECTIBLE_SYNC|{b64}");
        }

        // ====================================================================
        // TRANSPORTE NETCODE (CustomMessagingManager — ServerRpc / ClientRpc)
        // ====================================================================
        private void TryRegisterNetcodeMessagingHandlers()
        {
#if ULTIMOACTO_NETCODE
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening) return;
            if (NetworkManager.Singleton.CustomMessagingManager == null) return;

            var cmm = NetworkManager.Singleton.CustomMessagingManager;
            cmm.UnregisterNamedMessageHandler(MsgClientActionToHost);
            cmm.UnregisterNamedMessageHandler(MsgHostBroadcastToClients);
            cmm.UnregisterNamedMessageHandler(MsgClientRequestSnapshot);

            cmm.RegisterNamedMessageHandler(MsgClientActionToHost, OnHostReceivedClientAction);
            cmm.RegisterNamedMessageHandler(MsgHostBroadcastToClients, OnClientReceivedHostBroadcast);
            cmm.RegisterNamedMessageHandler(MsgClientRequestSnapshot, OnHostReceivedSnapshotRequest);

            handlersRegistered = true;

            // Si somos cliente recién conectado, solicitar el estado completo de la partida al Host
            if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                RequestFullSnapshotFromHost();
            }
#endif
        }

        private void UnregisterNetcodeMessagingHandlers()
        {
#if ULTIMOACTO_NETCODE
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.CustomMessagingManager != null)
            {
                var cmm = NetworkManager.Singleton.CustomMessagingManager;
                cmm.UnregisterNamedMessageHandler(MsgClientActionToHost);
                cmm.UnregisterNamedMessageHandler(MsgHostBroadcastToClients);
                cmm.UnregisterNamedMessageHandler(MsgClientRequestSnapshot);
            }
#endif
            handlersRegistered = false;
        }

        private void DispatchStatePacket(string packet)
        {
            if (string.IsNullOrEmpty(packet) || !IsNetcodeActive()) return;

#if ULTIMOACTO_NETCODE
            var nm = NetworkManager.Singleton;
            if (nm == null || nm.CustomMessagingManager == null) return;

            if (nm.IsServer || nm.IsHost)
            {
                BroadcastPacketFromHostToClients(packet);
            }
            else if (nm.IsClient)
            {
                SendPacketFromClientToHost(packet);
            }
#endif
        }

#if ULTIMOACTO_NETCODE
        private void SendPacketFromClientToHost(string packet)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || nm.CustomMessagingManager == null) return;

            byte[] utf8 = System.Text.Encoding.UTF8.GetBytes(packet);
            using (var writer = new FastBufferWriter(utf8.Length + 16, Allocator.Temp))
            {
                writer.WriteValueSafe(utf8.Length);
                writer.WriteBytesSafe(utf8, utf8.Length);
                nm.CustomMessagingManager.SendNamedMessage(
                    MsgClientActionToHost,
                    NetworkManager.ServerClientId,
                    writer,
                    NetworkDelivery.ReliableSequenced);
            }
        }

        private void BroadcastPacketFromHostToClients(string packet)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || nm.CustomMessagingManager == null || !nm.IsServer) return;

            byte[] utf8 = System.Text.Encoding.UTF8.GetBytes(packet);
            foreach (ulong clientId in nm.ConnectedClientsIds)
            {
                if (clientId == nm.LocalClientId) continue;
                using (var writer = new FastBufferWriter(utf8.Length + 16, Allocator.Temp))
                {
                    writer.WriteValueSafe(utf8.Length);
                    writer.WriteBytesSafe(utf8, utf8.Length);
                    nm.CustomMessagingManager.SendNamedMessage(
                        MsgHostBroadcastToClients,
                        clientId,
                        writer,
                        NetworkDelivery.ReliableSequenced);
                }
            }
        }

        private void RequestFullSnapshotFromHost()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || nm.CustomMessagingManager == null) return;

            using (var writer = new FastBufferWriter(16, Allocator.Temp))
            {
                writer.WriteValueSafe(1);
                nm.CustomMessagingManager.SendNamedMessage(
                    MsgClientRequestSnapshot,
                    NetworkManager.ServerClientId,
                    writer,
                    NetworkDelivery.ReliableSequenced);
            }
        }

        private void OnHostReceivedSnapshotRequest(ulong senderClientId, FastBufferReader reader)
        {
            if (!IsHostAuthority()) return;
            SendFullSnapshotFromHostToAllClients();
        }

        private void OnHostReceivedClientAction(ulong senderClientId, FastBufferReader reader)
        {
            if (!IsHostAuthority()) return;

            string packet = ReadPacketString(reader);
            if (string.IsNullOrEmpty(packet)) return;

            // 1. El Host autoritativo valida y aplica la acción en su propio mundo
            ApplyReceivedStatePacket(packet);

            // 2. El Host replica el estado confirmado a todos los clientes
            BroadcastPacketFromHostToClients(packet);
        }

        private void OnClientReceivedHostBroadcast(ulong senderClientId, FastBufferReader reader)
        {
            if (IsHostAuthority()) return;

            string packet = ReadPacketString(reader);
            if (string.IsNullOrEmpty(packet)) return;

            ApplyReceivedStatePacket(packet);
        }

        private static string ReadPacketString(FastBufferReader reader)
        {
            try
            {
                reader.ReadValueSafe(out int byteLen);
                if (byteLen <= 0 || byteLen > 262144) return string.Empty;
                byte[] buf = new byte[byteLen];
                reader.ReadBytesSafe(ref buf, byteLen);
                return System.Text.Encoding.UTF8.GetString(buf);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SharedWorldStateSync] Error al decodificar paquete de estado: {ex.Message}");
                return string.Empty;
            }
        }
#endif

        // ====================================================================
        // SNAPSHOT COMPLETO (Cuando un 2.º jugador se une a mitad de partida)
        // ====================================================================
        public void SendFullSnapshotFromHostToAllClients()
        {
            if (!IsHostAuthority() || !IsNetcodeActive()) return;

            // 1. Noche actual y evidencias
            if (NightManager.Instance != null)
            {
                DispatchStatePacket($"NIGHT|{NightManager.Instance.CurrentNightIndex}|{NightManager.Instance.EvidenceThisNight}");
                foreach (string evId in NightManager.Instance.GlobalEvidenceIds)
                {
                    DispatchStatePacket($"EVIDENCE|{Sanitize(evId)}");
                }
            }

            // 2. Llaves e inventario compartido
            foreach (string keyId in SimpleInventory.GlobalKeys)
            {
                DispatchStatePacket($"KEY_ADD|{keyId}|");
            }

            // 3. Objetos recogidos (desactivados en el mundo)
            foreach (var kv in itemsById)
            {
                if (kv.Value != null && !kv.Value.gameObject.activeSelf)
                {
                    DispatchStatePacket($"ITEM|{kv.Key}|{kv.Value.Id}|{Sanitize(kv.Value.DisplayName)}");
                }
            }

            // 4. Puertas abiertas o desbloqueadas
            foreach (var kv in doorsById)
            {
                WorldDoor d = kv.Value;
                if (d != null && (d.IsOpen || !d.IsLocked))
                {
                    DispatchStatePacket($"DOOR|{kv.Key}|{(d.IsOpen ? 1 : 0)}|{(d.IsLocked ? 1 : 0)}|{d.Id}");
                }
            }

            foreach (var kv in doubleDoorsById)
            {
                WorldDoubleDoor dd = kv.Value;
                if (dd != null && (dd.IsOpen || !dd.IsLocked))
                {
                    DispatchStatePacket($"DDOOR|{kv.Key}|{(dd.IsOpen ? 1 : 0)}|{(dd.IsLocked ? 1 : 0)}|{dd.Id}");
                }
            }

            // 5. Cuadro eléctrico (AcademyFuseBoxController)
            if (AcademyFuseBoxController.Instance != null)
            {
                if (AcademyFuseBoxController.Instance.IsPowerRestored)
                {
                    DispatchStatePacket("FUSE_POWER|1");
                }
            }

            // 6. Galería de Retratos (PortraitGalleryPuzzleController)
            if (PortraitGalleryPuzzleController.Instance != null)
            {
                int[] dials = PortraitGalleryPuzzleController.Instance.GetCurrentDialSnapshot();
                if (dials != null)
                {
                    for (int i = 0; i < dials.Length; i++)
                    {
                        if (dials[i] != 0)
                        {
                            DispatchStatePacket($"PORTRAIT_DIAL|{i}|{dials[i]}");
                        }
                    }
                }
            }

            // 7. Puzles de secuencia ordenada
            foreach (var kv in seqPuzzlesById)
            {
                OrderedSequencePuzzle p = kv.Value;
                if (p != null && (p.Progress > 0 || p.IsSolved))
                {
                    DispatchStatePacket($"SEQ_PUZZLE|{p.PuzzleId}|{p.Progress}|{(p.IsSolved ? 1 : 0)}");
                }
            }

            // 8. Secuencia Final del Teatro Principal (5 fases)
            int climaxMask = TheatreClimaxController.GetPhaseMaskSnapshot();
            if (climaxMask != 0)
            {
                DispatchStatePacket($"THEATRE_CLIMAX|{climaxMask}");
            }

            // 9. Nodos interactivos de secciones
            foreach (var kv in backstageById)
            {
                if (kv.Value != null && kv.Value.IsResolved) DispatchStatePacket($"NODE_BACKSTAGE|{kv.Key}");
            }
            foreach (var kv in musicById)
            {
                if (kv.Value != null && kv.Value.IsCompleted) DispatchStatePacket($"NODE_MUSIC|{kv.Key}");
            }
            foreach (var kv in secretById)
            {
                if (kv.Value != null && kv.Value.IsActivated) DispatchStatePacket($"NODE_SECRET|{kv.Key}");
            }
            foreach (var kv in gardenById)
            {
                if (kv.Value != null && kv.Value.IsInspected) DispatchStatePacket($"NODE_GARDEN|{kv.Key}");
            }
            foreach (var kv in expansionById)
            {
                if (kv.Value != null && kv.Value.HasInteracted) DispatchStatePacket($"NODE_EXPANSION|{kv.Key}");
            }

            // 10. Diario Compartido de Evidencias (SharedEvidenceJournalManager)
            if (SharedEvidenceJournalManager.Instance != null)
            {
                var allEntries = SharedEvidenceJournalManager.Instance.AllEntries;
                for (int i = 0; i < allEntries.Count; i++)
                {
                    string json = JsonUtility.ToJson(allEntries[i]);
                    string b64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
                    DispatchStatePacket($"JOURNAL_ENTRY|{b64}");
                }
            }

            // 11. Puzles Cooperativos (CoopPuzzleBase)
            foreach (var kv in coopPuzzlesById)
            {
                if (kv.Value != null)
                {
                    string snapPayload = kv.Value.GetNetworkSnapshotPayload();
                    if (!string.IsNullOrEmpty(snapPayload))
                    {
                        string b64Payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(snapPayload));
                        DispatchStatePacket($"COOP_PUZZLE|{kv.Key}|{b64Payload}");
                    }
                }
            }

            // 12. Niebla de Guerra del Mapa Compartido (AcademyMapUI)
            if (UltimoActo.UI.AcademyMapUI.Instance != null)
            {
                foreach (string zId in UltimoActo.UI.AcademyMapUI.Instance.VisitedZoneIds)
                {
                    DispatchStatePacket($"MAP_VISIT|{zId}");
                }
            }

            // 13. Coleccionables y Secretos Especiales (CollectibleAndSecretManager)
            if (CollectibleAndSecretManager.Instance != null)
            {
                List<string> colSnapshots = CollectibleAndSecretManager.Instance.BuildNetworkSnapshotPackets();
                for (int i = 0; i < colSnapshots.Count; i++)
                {
                    string b64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(colSnapshots[i]));
                    DispatchStatePacket($"COLLECTIBLE_SYNC|{b64}");
                }
            }
        }

        // ====================================================================
        // APLICACIÓN DEL ESTADO RECIBIDO EN EL MUNDO LOCAL
        // ====================================================================
        private void ApplyReceivedStatePacket(string packet)
        {
            string[] parts = packet.Split('|');
            if (parts.Length < 2) return;

            string cmd = parts[0];
            isApplyingRemoteState = true;

            try
            {
                switch (cmd)
                {
                    case "DOOR":
                        if (parts.Length >= 4)
                        {
                            string uniqueKey = parts[1];
                            bool open = parts[2] == "1";
                            bool locked = parts[3] == "1";
                            string fallbackId = parts.Length >= 5 ? parts[4] : uniqueKey;

                            if (doorsById.TryGetValue(uniqueKey, out WorldDoor door) && door != null)
                            {
                                door.ApplyNetworkState(open, locked, true);
                            }
                            else if (doorsById.TryGetValue(fallbackId, out WorldDoor doorById) && doorById != null)
                            {
                                doorById.ApplyNetworkState(open, locked, true);
                            }
                        }
                        break;

                    case "DDOOR":
                        if (parts.Length >= 4)
                        {
                            string uniqueKey = parts[1];
                            bool open = parts[2] == "1";
                            bool locked = parts[3] == "1";
                            string fallbackId = parts.Length >= 5 ? parts[4] : uniqueKey;

                            if (doubleDoorsById.TryGetValue(uniqueKey, out WorldDoubleDoor ddoor) && ddoor != null)
                            {
                                ddoor.ApplyNetworkState(open, locked, true);
                            }
                            else if (doubleDoorsById.TryGetValue(fallbackId, out WorldDoubleDoor ddoorById) && ddoorById != null)
                            {
                                ddoorById.ApplyNetworkState(open, locked, true);
                            }
                        }
                        break;

                    case "ITEM":
                        if (parts.Length >= 3)
                        {
                            string uniqueKey = parts[1];
                            string itemId = parts[2];

                            if (itemsById.TryGetValue(uniqueKey, out WorldItem item) && item != null)
                            {
                                item.ApplyNetworkPickedUp(true);
                            }
                            else if (itemsById.TryGetValue(itemId, out WorldItem itemById) && itemById != null)
                            {
                                itemById.ApplyNetworkPickedUp(true);
                            }
                        }
                        break;

                    case "KEY_ADD":
                        if (parts.Length >= 2)
                        {
                            string keyId = parts[1];
                            string disp = parts.Length >= 3 ? parts[2] : "";
                            SimpleInventory inv = FindFirstObjectByType<SimpleInventory>();
                            if (inv != null) inv.AddKeyFromNetwork(keyId, disp);
                        }
                        break;

                    case "KEY_REM":
                        if (parts.Length >= 2)
                        {
                            string keyId = parts[1];
                            SimpleInventory inv = FindFirstObjectByType<SimpleInventory>();
                            if (inv != null) inv.RemoveKeyFromNetwork(keyId);
                        }
                        break;

                    case "EVIDENCE":
                        if (parts.Length >= 2 && NightManager.Instance != null)
                        {
                            NightManager.Instance.RegisterEvidenceFromNetwork(parts[1]);
                        }
                        break;

                    case "NIGHT":
                        if (parts.Length >= 3 && NightManager.Instance != null)
                        {
                            if (int.TryParse(parts[1], out int nIdx) && int.TryParse(parts[2], out int evCount))
                            {
                                NightManager.Instance.ApplyAuthoritativeNightFromHost(nIdx, evCount);
                            }
                        }
                        break;

                    case "FUSE_SLOT":
                        if (parts.Length >= 2 && AcademyFuseBoxController.Instance != null)
                        {
                            if (int.TryParse(parts[1], out int slotIdx))
                            {
                                AcademyFuseBoxController.Instance.ApplyNetworkFuseSlot(slotIdx);
                            }
                        }
                        break;

                    case "FUSE_POWER":
                        if (AcademyFuseBoxController.Instance != null)
                        {
                            AcademyFuseBoxController.Instance.ApplyNetworkPowerRestored();
                        }
                        break;

                    case "PORTRAIT_DIAL":
                        if (parts.Length >= 3 && PortraitGalleryPuzzleController.Instance != null)
                        {
                            if (int.TryParse(parts[1], out int pIdx) && int.TryParse(parts[2], out int dVal))
                            {
                                PortraitGalleryPuzzleController.Instance.ApplyNetworkDialState(pIdx, dVal);
                            }
                        }
                        break;

                    case "SEQ_PUZZLE":
                        if (parts.Length >= 4)
                        {
                            string pId = parts[1];
                            if (int.TryParse(parts[2], out int prog))
                            {
                                bool solved = parts[3] == "1";
                                if (seqPuzzlesById.TryGetValue(pId, out OrderedSequencePuzzle puzzle) && puzzle != null)
                                {
                                    puzzle.ApplyNetworkState(prog, solved);
                                }
                            }
                        }
                        break;

                    case "THEATRE_CLIMAX":
                        if (parts.Length >= 2 && int.TryParse(parts[1], out int mask))
                        {
                            TheatreClimaxController.ApplyNetworkPhaseState(mask);
                        }
                        break;

                    case "NODE_BACKSTAGE":
                        if (parts.Length >= 2 && backstageById.TryGetValue(parts[1], out var bNode) && bNode != null)
                        {
                            bNode.ApplyNetworkResolved();
                        }
                        break;

                    case "NODE_MUSIC":
                        if (parts.Length >= 2 && musicById.TryGetValue(parts[1], out var mNode) && mNode != null)
                        {
                            mNode.ApplyNetworkCompleted();
                        }
                        break;

                    case "NODE_SECRET":
                        if (parts.Length >= 2 && secretById.TryGetValue(parts[1], out var sNode) && sNode != null)
                        {
                            sNode.ApplyNetworkActivated();
                        }
                        break;

                    case "NODE_GARDEN":
                        if (parts.Length >= 2 && gardenById.TryGetValue(parts[1], out var gNode) && gNode != null)
                        {
                            gNode.ApplyNetworkInspected();
                        }
                        break;

                    case "NODE_EXPANSION":
                        if (parts.Length >= 2 && expansionById.TryGetValue(parts[1], out var eNode) && eNode != null)
                        {
                            eNode.ApplyNetworkInteracted();
                        }
                        break;

                    case "JOURNAL_ENTRY":
                        if (parts.Length >= 2 && SharedEvidenceJournalManager.Instance != null)
                        {
                            try
                            {
                                byte[] bytes = Convert.FromBase64String(parts[1]);
                                string json = System.Text.Encoding.UTF8.GetString(bytes);
                                EvidenceJournalEntry entry = JsonUtility.FromJson<EvidenceJournalEntry>(json);
                                if (entry != null)
                                {
                                    SharedEvidenceJournalManager.Instance.ApplyEntryFromNetwork(entry);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"[SharedWorldStateSync] Error decodificando entrada del diario: {ex.Message}");
                            }
                        }
                        break;

                    case "COOP_PUZZLE":
                        if (parts.Length >= 3)
                        {
                            string pId = parts[1];
                            try
                            {
                                byte[] bytes = Convert.FromBase64String(parts[2]);
                                string subPayload = System.Text.Encoding.UTF8.GetString(bytes);
                                if (!coopPuzzlesById.ContainsKey(pId))
                                {
                                    IndexSceneInteractiveObjects();
                                }
                                if (coopPuzzlesById.TryGetValue(pId, out CoopPuzzleBase cp) && cp != null)
                                {
                                    cp.ApplyNetworkCommand(subPayload);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"[SharedWorldStateSync] Error decodificando puzle cooperativo '{pId}': {ex.Message}");
                            }
                        }
                        break;

                    case "ANOMALY_TRIG":
                        if (parts.Length >= 2 && AsymmetricAnomalyManager.Instance != null)
                        {
                            AsymmetricAnomalyManager.Instance.ApplyAnomalyTriggerFromNetwork(parts[1]);
                        }
                        break;

                    case "FINAL_DAWN_START":
                        if (parts.Length >= 2 && UltimateFinaleAndCreditsController.Instance != null)
                        {
                            try
                            {
                                byte[] bytes = Convert.FromBase64String(parts[1]);
                                string finalePayload = System.Text.Encoding.UTF8.GetString(bytes);
                                UltimateFinaleAndCreditsController.Instance.ApplyRemoteFinaleStartFromHost(finalePayload);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"[SharedWorldStateSync] Error decodificando inicio del final: {ex.Message}");
                            }
                        }
                        break;

                    case "MAP_VISIT":
                        if (parts.Length >= 2 && UltimoActo.UI.AcademyMapUI.Instance != null)
                        {
                            UltimoActo.UI.AcademyMapUI.Instance.ApplyVisitedZoneFromNetwork(parts[1]);
                        }
                        break;

                    case "COLLECTIBLE_SYNC":
                        if (parts.Length >= 2 && CollectibleAndSecretManager.Instance != null)
                        {
                            try
                            {
                                byte[] bytes = Convert.FromBase64String(parts[1]);
                                string colPayload = System.Text.Encoding.UTF8.GetString(bytes);
                                CollectibleAndSecretManager.Instance.ApplyNetworkPacket(colPayload);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"[SharedWorldStateSync] Error decodificando coleccionable: {ex.Message}");
                            }
                        }
                        break;
                }
            }
            finally
            {
                isApplyingRemoteState = false;
            }
        }

        private static string Sanitize(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Replace("|", " ").Replace("\n", " ").Replace("\r", "");
        }
    }
}
