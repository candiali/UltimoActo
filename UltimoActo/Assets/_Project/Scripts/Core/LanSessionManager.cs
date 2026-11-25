using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ULTIMOACTO_NETCODE
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
#endif

namespace UltimoActo.Core
{
    public enum LanSessionMode
    {
        Solo = 0,
        Host = 1,
        Client = 2
    }

    /// <summary>
    /// Gestor persistente de sesión LAN y modo solitario para 'Último Acto'.
    /// - Jugar Solo: Época 1929, permite alternar entre 1929 y Ruinas con [Q], y abrir la sala
    ///   en cualquier punto seguro (inicio de noche o menú de pausa [ESC]) sin perder progreso.
    /// - Crear Sala (Host): Época 1929 (Glory1929) + anuncio automático por UDP Broadcast + Netcode Host.
    ///   Mientras está solo (1/2) puede usar [Q]; al conectarse el 2.º jugador (2/2), [Q] se bloquea
    ///   y el Host queda fijado en 1929.
    /// - Unirse a Sala (Client): Época Ruinas (Ruins) fija (2/2 jugadores).
    /// </summary>
    public class LanSessionManager : MonoBehaviour
    {
        public static LanSessionManager Instance { get; private set; }

        [Header("Estado de Sesión")]
        [SerializeField] private LanSessionMode sessionMode = LanSessionMode.Solo;
        [SerializeField] private EraVisual localPlayerEra = EraVisual.Glory1929;
        [SerializeField] private string localPlayerName = "Investigador";
        [SerializeField] private string activeRoomName = "Academia Vesper";
        [SerializeField] private string hostIpAddress = "127.0.0.1";
        [SerializeField] private int gamePort = LanRoomDiscovery.DefaultGamePort;
        [SerializeField] private int connectedPlayers = 1;

        private LanRoomDiscovery discovery;
        private string pendingTargetScene = "SampleScene";

        public LanSessionMode SessionMode => sessionMode;
        public LanSessionMode CurrentMode => sessionMode;
        public EraVisual LocalPlayerEra => localPlayerEra;
        public string LocalPlayerName => localPlayerName;
        public string ActiveRoomName => activeRoomName;
        public string TargetHostIp => hostIpAddress;
        public int ActivePort => gamePort;
        public LanRoomDiscovery Discovery => discovery;
        public int ConnectedPlayerCount => sessionMode == LanSessionMode.Client ? 2 : Mathf.Clamp(connectedPlayers, 1, 2);
        public bool IsRoomOpenOnLan => sessionMode == LanSessionMode.Host && discovery != null && discovery.IsBroadcastingHost;

        /// <summary>
        /// En solitario (o como Host mientras espera solo 1/2), el jugador puede alternar de época con [Q].
        /// Cuando hay 2 jugadores conectados (2/2), [Q] se desactiva automáticamente.
        /// </summary>
        public bool CanLocalPlayerToggleEraWithQ => ConnectedPlayerCount < 2 && sessionMode != LanSessionMode.Client;

        public event Action<string, bool> OnStatusMessage;
        public event Action<int> OnPlayerCountChanged;
        public event Action<int> OnConnectedPlayerCountChanged
        {
            add => OnPlayerCountChanged += value;
            remove => OnPlayerCountChanged -= value;
        }
        public event Action OnRoomOpenedInGame;
        public event Action<string> OnPartnerDisconnected;
        public event Action OnPartnerReconnected;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrapOnSceneLoad()
        {
            EnsureInstance();
        }

        public static LanSessionManager EnsureInstance()
        {
            if (Instance != null) return Instance;

            LanSessionManager existing = FindFirstObjectByType<LanSessionManager>();
            if (existing != null)
            {
                Instance = existing;
                return Instance;
            }

            GameObject go = new GameObject("LanSessionManager");
            Instance = go.AddComponent<LanSessionManager>();
            return Instance;
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

            discovery = GetComponent<LanRoomDiscovery>();
            if (discovery == null)
            {
                discovery = gameObject.AddComponent<LanRoomDiscovery>();
            }
            discovery.OnJoinVerifyResult += HandleJoinVerifyResult;

            // Sincronizar con PlayerPrefs si se entró directamente a SampleScene desde el Editor
            string savedRole = PlayerPrefs.GetString("NetworkRole", "Solo");
            if (string.Equals(savedRole, "Client", StringComparison.OrdinalIgnoreCase))
            {
                sessionMode = LanSessionMode.Client;
                localPlayerEra = EraVisual.Ruins;
                connectedPlayers = 2;
            }
            else if (string.Equals(savedRole, "Host", StringComparison.OrdinalIgnoreCase))
            {
                sessionMode = LanSessionMode.Host;
                localPlayerEra = EraVisual.Glory1929;
                connectedPlayers = 1;
            }
            else
            {
                sessionMode = LanSessionMode.Solo;
                localPlayerEra = EraVisual.Glory1929;
                connectedPlayers = 1;
            }
        }

        private void OnDestroy()
        {
            if (discovery != null)
            {
                discovery.OnJoinVerifyResult -= HandleJoinVerifyResult;
            }
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// 1. JUGAR SOLO (sin red, época inicial 1929 Glory1929, tecla [Q] habilitada).
        /// </summary>
        public void StartSoloSession(string targetScene = "SampleScene")
        {
            ShutdownNetworkIfActive();
            if (discovery != null) discovery.StopAllDiscovery();

            sessionMode = LanSessionMode.Solo;
            localPlayerEra = EraVisual.Glory1929;
            connectedPlayers = 1;
            PlayerPrefs.SetString("NetworkRole", "Solo");
            PlayerPrefs.Save();

            OnPlayerCountChanged?.Invoke(1);
            Debug.Log("<color=green>[LanSessionManager]</color> Iniciando partida en modo JUGAR SOLO (Época inicial: 1929 · [Q] para alternar época).");
            SceneManager.LoadScene(targetScene);
        }

        /// <summary>
        /// 2. CREAR SALA desde el Menú Principal (Host = Época 1929 Glory1929, anuncia en LAN por UDP broadcast).
        /// </summary>
        public void StartHostSession(string roomName, string playerName, string targetScene = "SampleScene")
        {
            ShutdownNetworkIfActive();

            sessionMode = LanSessionMode.Host;
            localPlayerEra = EraVisual.Glory1929;
            connectedPlayers = 1;
            activeRoomName = string.IsNullOrWhiteSpace(roomName) ? "Academia Vesper" : roomName.Trim();
            localPlayerName = string.IsNullOrWhiteSpace(playerName) ? "Investigador 1929" : playerName.Trim();
            gamePort = LanRoomDiscovery.DefaultGamePort;

            PlayerPrefs.SetString("NetworkRole", "Host");
            PlayerPrefs.SetString("LanRoomName", activeRoomName);
            PlayerPrefs.SetString("LanPlayerName", localPlayerName);
            PlayerPrefs.Save();

            StartHostServicesInternal(out string statusMsg, out bool isErr);
            OnStatusMessage?.Invoke(statusMsg, isErr);
            OnPlayerCountChanged?.Invoke(connectedPlayers);

            SceneManager.LoadScene(targetScene);
        }

        /// <summary>
        /// 2B. ABRIR SALA EN PARTIDA (Paso 2): Permite que un jugador que empezó en "Jugar Solo"
        /// abra la sala desde el menú de pausa [ESC] o al inicio de una noche SIN recargar la escena
        /// y SIN perder el progreso actual de noches, puertas, inventario ni puzles.
        /// </summary>
        public bool OpenRoomFromSoloInPlace(string roomName = null, string playerName = null)
        {
            if (sessionMode == LanSessionMode.Client)
            {
                OnStatusMessage?.Invoke("Solo el anfitrión puede abrir la sala.", true);
                return false;
            }

            if (IsRoomOpenOnLan)
            {
                OnStatusMessage?.Invoke($"La sala '{activeRoomName}' ya está abierta y anunciándose en la red local ({ConnectedPlayerCount}/2).", false);
                return true;
            }

            string sysUser = string.IsNullOrWhiteSpace(Environment.UserName) ? "Vesper" : Environment.UserName;
            activeRoomName = !string.IsNullOrWhiteSpace(roomName)
                ? roomName.Trim()
                : PlayerPrefs.GetString("LanRoomName", $"Academia Vesper — Sala de {sysUser}");
            localPlayerName = !string.IsNullOrWhiteSpace(playerName)
                ? playerName.Trim()
                : PlayerPrefs.GetString("LanPlayerName", sysUser);

            sessionMode = LanSessionMode.Host;
            localPlayerEra = EraVisual.Glory1929;
            connectedPlayers = 1;
            gamePort = LanRoomDiscovery.DefaultGamePort;

            PlayerPrefs.SetString("NetworkRole", "Host");
            PlayerPrefs.SetString("LanRoomName", activeRoomName);
            PlayerPrefs.SetString("LanPlayerName", localPlayerName);
            PlayerPrefs.Save();

            bool ok = StartHostServicesInternal(out string msg, out bool isErr);
            OnStatusMessage?.Invoke(msg, isErr);
            OnPlayerCountChanged?.Invoke(connectedPlayers);
            OnRoomOpenedInGame?.Invoke();

            if (NightManager.Instance != null)
            {
                NightManager.Instance.ShowToast($"SALA LAN ABIERTA: «{activeRoomName}» — Esperando 2.º investigador (progreso conservado)", 5.5f);
            }

            return ok;
        }

        private bool StartHostServicesInternal(out string statusMessage, out bool isError)
        {
            bool udpOk = discovery != null && discovery.StartHostBroadcast(activeRoomName, localPlayerName, gamePort);

#if ULTIMOACTO_NETCODE
            EnsureNetcodeManagerConfigured("0.0.0.0", (ushort)gamePort, true);
            if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsListening)
            {
                bool hostStarted = NetworkManager.Singleton.StartHost();
                if (!hostStarted)
                {
                    statusMessage = "No se pudo abrir el puerto 7777 del Host. Verifica Windows Firewall o que otra instancia no use ese puerto.";
                    isError = true;
                    return false;
                }
            }
#endif

            if (!udpOk)
            {
                statusMessage = $"Sala '{activeRoomName}' abierta (Puerto 7777), pero el puerto UDP 47777 de anuncio está ocupado en este PC. El 2.º jugador puede unirse con IP manual (127.0.0.1 o tu IP local).";
                isError = false;
                return true;
            }

            statusMessage = $"Sala '{activeRoomName}' abierta y anunciándose en la red local (progreso actual conservado).";
            isError = false;
            return true;
        }

        /// <summary>
        /// Notifica la entrada o salida del segundo jugador (llamado por Netcode o en pruebas locales),
        /// bloqueando [Q] y fijando al Host en 1929 cuando hay 2 jugadores conectados.
        /// </summary>
        public void SetConnectedPlayerCount(int count)
        {
            int clamped = Mathf.Clamp(count, 1, 2);
            if (connectedPlayers == clamped) return;

            int previousCount = connectedPlayers;
            connectedPlayers = clamped;
            if (discovery != null)
            {
                discovery.HostCurrentPlayers = connectedPlayers;
            }

            if (connectedPlayers >= 2)
            {
                // Con 2 jugadores, cada uno queda fijo en su época asignada (Host = 1929, Client = Ruinas)
                EraVisual lockedEra = sessionMode == LanSessionMode.Client ? EraVisual.Ruins : EraVisual.Glory1929;
                localPlayerEra = lockedEra;
                if (EraVisualManager.Instance != null && EraVisualManager.Instance.CurrentEra != lockedEra)
                {
                    EraVisualManager.Instance.SetEra(lockedEra, true);
                }

                if (NightManager.Instance != null)
                {
                    string eraLabel = lockedEra == EraVisual.Glory1929 ? "1929 (Gloria)" : "Ruinas (Actualidad)";
                    NightManager.Instance.ShowToast($"2.º INVESTIGADOR CONECTADO (2/2) — Cambio [Q] desactivado · Tu época es {eraLabel}", 5.5f);
                }

                if (previousCount < 2)
                {
                    OnPartnerReconnected?.Invoke();
                }
            }
            else
            {
                if (NightManager.Instance != null)
                {
                    NightManager.Instance.HostPersistCurrentNightSave();
                    if (sessionMode != LanSessionMode.Client)
                    {
                        NightManager.Instance.ShowToast("Investigador desconectado (1/2) — Progreso guardado por el anfitrión", 4.5f);
                    }
                }

                if (previousCount >= 2)
                {
                    string reason = sessionMode == LanSessionMode.Client
                        ? "Se ha perdido la conexión con el anfitrión de la sala."
                        : "El segundo investigador (Época Ruinas) se ha desconectado de la sala.";
                    OnPartnerDisconnected?.Invoke(reason);
                }
            }

            OnPlayerCountChanged?.Invoke(connectedPlayers);
        }

        /// <summary>
        /// Paso 6: Permite al Cliente intentar reconectar en caliente con la última IP/Puerto del Host
        /// sin recargar la escena ni perder el estado visual/progreso cargado.
        /// </summary>
        public bool TryReconnectClientInPlace()
        {
            string targetIp = string.IsNullOrWhiteSpace(hostIpAddress)
                ? PlayerPrefs.GetString("LanHostIp", "127.0.0.1")
                : hostIpAddress;

            if (discovery != null)
            {
                discovery.BeginVerifyAndJoin(targetIp, gamePort);
            }

#if ULTIMOACTO_NETCODE
            ShutdownNetworkIfActive();
            EnsureNetcodeManagerConfigured(targetIp, (ushort)gamePort, false);
            if (NetworkManager.Singleton != null)
            {
                bool started = NetworkManager.Singleton.StartClient();
                if (started)
                {
                    sessionMode = LanSessionMode.Client;
                    connectedPlayers = 2;
                    OnPartnerReconnected?.Invoke();
                    OnPlayerCountChanged?.Invoke(2);
                    return true;
                }
            }
#endif
            return false;
        }

        /// <summary>
        /// Paso 6: Si el otro jugador se desconecta definitivamente y el jugador local decide continuar
        /// la partida en solitario desde el mismo punto sin volver al menú ni perder progreso.
        /// </summary>
        public void ContinueInSoloAfterDisconnect()
        {
            if (NightManager.Instance != null)
            {
                NightManager.Instance.HostPersistCurrentNightSave();
            }

            if (sessionMode == LanSessionMode.Client)
            {
                ShutdownNetworkIfActive();
                if (discovery != null) discovery.StopAllDiscovery();
                sessionMode = LanSessionMode.Solo;
                connectedPlayers = 1;
                PlayerPrefs.SetString("NetworkRole", "Solo");
                PlayerPrefs.Save();
            }
            else
            {
                connectedPlayers = 1;
                if (discovery != null)
                {
                    discovery.HostCurrentPlayers = 1;
                }
            }

            OnPlayerCountChanged?.Invoke(1);
        }

        public void BeginSearchingLanRooms()
        {
            if (discovery == null) return;
            bool ok = discovery.StartClientSearch();
            if (ok)
            {
                OnStatusMessage?.Invoke("Buscando salas…", false);
            }
            else
            {
                OnStatusMessage?.Invoke(
                    "Windows Firewall o tu adaptador bloqueó el puerto UDP de búsqueda. Asegúrate de permitir el acceso en Redes privadas y públicas.",
                    true);
            }
        }

        public void RefreshLanSearch()
        {
            if (discovery == null) return;
            OnStatusMessage?.Invoke("Buscando salas…", false);
            discovery.RefreshClientSearch();
        }

        public void RequestJoinDiscoveredRoom(DiscoveredRoomInfo room, string playerName, string targetScene = "SampleScene")
        {
            if (room == null) return;

            if (!room.IsCompatibleVersion)
            {
                OnStatusMessage?.Invoke(
                    $"Versión distinta (Sala '{room.roomName}': v{room.gameVersion} · Tu juego: v{LanRoomDiscovery.ProtocolGameVersion}).",
                    true);
                return;
            }

            if (room.IsFull)
            {
                OnStatusMessage?.Invoke($"Sala llena ({room.currentPlayers}/{room.maxPlayers} investigadores en '{room.roomName}').", true);
                return;
            }

            localPlayerName = string.IsNullOrWhiteSpace(playerName) ? "Investigador Ruinas" : playerName.Trim();
            activeRoomName = room.roomName;
            pendingTargetScene = targetScene;

            OnStatusMessage?.Invoke($"Conectando… ({room.roomName})", false);
            discovery.BeginVerifyAndJoin(room.hostIp, room.gamePort);
        }

        public void RequestJoinByManualIp(string manualIp, string playerName, string targetScene = "SampleScene")
        {
            string cleanIp = string.IsNullOrWhiteSpace(manualIp) ? "127.0.0.1" : manualIp.Trim();
            localPlayerName = string.IsNullOrWhiteSpace(playerName) ? "Investigador Ruinas" : playerName.Trim();
            pendingTargetScene = targetScene;

            OnStatusMessage?.Invoke($"Conectando… ({cleanIp}:{LanRoomDiscovery.DefaultGamePort})", false);
            discovery.BeginVerifyAndJoin(cleanIp, LanRoomDiscovery.DefaultGamePort);
        }

        private void HandleJoinVerifyResult(LanJoinVerifyStatus status, string resolvedIp, int resolvedPort, string message)
        {
            if (status == LanJoinVerifyStatus.RoomFull ||
                status == LanJoinVerifyStatus.VersionMismatch ||
                status == LanJoinVerifyStatus.TimeoutOrFirewall)
            {
                OnStatusMessage?.Invoke(message, true);
                return;
            }

            ShutdownNetworkIfActive();
            if (discovery != null) discovery.StopAllDiscovery();

            sessionMode = LanSessionMode.Client;
            localPlayerEra = EraVisual.Ruins;
            connectedPlayers = 2;
            hostIpAddress = resolvedIp;
            gamePort = resolvedPort;

            PlayerPrefs.SetString("NetworkRole", "Client");
            PlayerPrefs.SetString("LanHostIp", hostIpAddress);
            PlayerPrefs.SetString("LanPlayerName", localPlayerName);
            PlayerPrefs.Save();

            OnStatusMessage?.Invoke(message, false);
            OnPlayerCountChanged?.Invoke(2);

#if ULTIMOACTO_NETCODE
            EnsureNetcodeManagerConfigured(hostIpAddress, (ushort)gamePort, false);
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.StartClient();
            }
#endif
            SceneManager.LoadScene(pendingTargetScene);
        }

        public void ShutdownNetworkSession()
        {
            ShutdownNetworkIfActive();
            if (discovery != null)
            {
                discovery.StopAllDiscovery();
            }
            sessionMode = LanSessionMode.Solo;
            localPlayerEra = EraVisual.Glory1929;
        }

        public void ShutdownNetworkIfActive()
        {
#if ULTIMOACTO_NETCODE
            if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsListening || NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost))
            {
                NetworkManager.Singleton.Shutdown();
            }
#endif
            connectedPlayers = 1;
        }

#if ULTIMOACTO_NETCODE
        private void EnsureNetcodeManagerConfigured(string address, ushort port, bool isHost)
        {
            NetworkManager nm = NetworkManager.Singleton;
            if (nm == null)
            {
                nm = FindFirstObjectByType<NetworkManager>();
            }
            if (nm == null)
            {
                GameObject nmObj = new GameObject("NetworkManager_UltimoActo");
                DontDestroyOnLoad(nmObj);
                UnityTransport utp = nmObj.AddComponent<UnityTransport>();
                nm = nmObj.AddComponent<NetworkManager>();
                nm.NetworkConfig = new NetworkConfig
                {
                    NetworkTransport = utp,
                    EnableSceneManagement = false,
                    ConnectionApproval = true
                };
            }

            UnityTransport transport = nm.GetComponent<UnityTransport>();
            if (transport == null)
            {
                transport = nm.gameObject.AddComponent<UnityTransport>();
            }
            if (nm.NetworkConfig == null)
            {
                nm.NetworkConfig = new NetworkConfig();
            }
            nm.NetworkConfig.NetworkTransport = transport;
            nm.NetworkConfig.ConnectionApproval = true;

            if (isHost)
            {
                transport.SetConnectionData("0.0.0.0", port, "0.0.0.0");
                nm.ConnectionApprovalCallback = OnNetcodeConnectionApproval;
            }
            else
            {
                transport.SetConnectionData(address, port);
            }

            nm.OnClientConnectedCallback -= OnNetcodeClientConnected;
            nm.OnClientConnectedCallback += OnNetcodeClientConnected;
            nm.OnClientDisconnectCallback -= OnNetcodeClientDisconnected;
            nm.OnClientDisconnectCallback += OnNetcodeClientDisconnected;
        }

        private void OnNetcodeConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            int connected = NetworkManager.Singleton != null ? NetworkManager.Singleton.ConnectedClientsIds.Count : 1;
            if (connected >= 2 && request.ClientNetworkId != NetworkManager.Singleton.LocalClientId)
            {
                response.Approved = false;
                response.Reason = "Sala llena (2/2 jugadores)";
                return;
            }

            response.Approved = true;
            response.CreatePlayerObject = false;
        }

        private void OnNetcodeClientConnected(ulong clientId)
        {
            if (NetworkManager.Singleton == null) return;
            if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
            {
                int count = Mathf.Clamp(NetworkManager.Singleton.ConnectedClientsIds.Count, 1, 2);
                SetConnectedPlayerCount(count);
            }
            else if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                SetConnectedPlayerCount(2);
            }
        }

        private void OnNetcodeClientDisconnected(ulong clientId)
        {
            if (NetworkManager.Singleton == null) return;
            if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
            {
                int count = Mathf.Clamp(NetworkManager.Singleton.ConnectedClientsIds.Count, 1, 2);
                SetConnectedPlayerCount(count);
            }
            else
            {
                // El cliente perdió la conexión con el Host
                SetConnectedPlayerCount(1);
            }
        }
#endif
    }
}
