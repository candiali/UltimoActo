using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace UltimoActo.Core
{
    /// <summary>
    /// Información anunciada por una sala en la red local (LAN) mediante UDP broadcast.
    /// </summary>
    [Serializable]
    public class DiscoveredRoomInfo
    {
        public string roomId;
        public string roomName;
        public string hostPlayerName;
        public string hostIp;
        public int gamePort;
        public int nightIndex;
        public string nightDisplay;
        public string gameVersion;
        public int currentPlayers;
        public int maxPlayers;
        public float lastSeenRealtime;

        public bool IsFull => currentPlayers >= maxPlayers;
        public bool IsCompatibleVersion => string.Equals(gameVersion, LanRoomDiscovery.ProtocolGameVersion, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///Resultado de verificación previa al unirse a una sala LAN (handshake rápido UDP).
    /// </summary>
    public enum LanJoinVerifyStatus
    {
        Pending = 0,
        Approved = 1,
        RoomFull = 2,
        VersionMismatch = 3,
        TimeoutOrFirewall = 4
    }

    /// <summary>
    /// Sistema de descubrimiento y anuncio automático de salas en red local (LAN) por UDP Broadcast.
    /// No requiere que el jugador escriba IPs ni puertos en el uso normal, y funciona tanto
    /// entre dos equipos en la misma red Wi-Fi/Ethernet como entre dos instancias en el mismo PC.
    /// </summary>
    public class LanRoomDiscovery : MonoBehaviour
    {
        public const int DiscoveryHostPort = 47777;
        public const int DiscoveryClientPort = 47778;
        public const int DefaultGamePort = 7777;
        public const string MagicHeader = "ULTIMOACTO_LAN_V1";
        public static string ProtocolGameVersion => string.IsNullOrEmpty(Application.version) ? "1.0.0" : Application.version;

        public static LanRoomDiscovery Instance => LanSessionManager.Instance != null
            ? LanSessionManager.Instance.Discovery
            : FindFirstObjectByType<LanRoomDiscovery>();

        public event Action<List<DiscoveredRoomInfo>> OnRoomsUpdated;
        public event Action<LanJoinVerifyStatus, string, int, string> OnJoinVerifyResult;

        private UdpClient udpSocket;
        private int boundPort;
        private bool isBroadcastingHost;
        private bool isSearchingClient;

        // Datos de la sala cuando actuamos como Host
        private string hostRoomId;
        private string hostRoomName = "Academia Vesper";
        private string hostPlayerName = "Investigador (1929)";
        private int hostGamePort = DefaultGamePort;
        private int hostCurrentPlayers = 1;
        private int hostMaxPlayers = 2;

        private float nextBroadcastTime;
        private float nextPruneTime;
        private const float BroadcastInterval = 1.0f;
        private const float RoomExpireSeconds = 4.5f;

        // Verificación de conexión activa (handshake previo)
        private bool waitingJoinVerify;
        private string pendingJoinIp;
        private int pendingJoinPort;
        private float joinVerifyDeadline;

        private readonly Dictionary<string, DiscoveredRoomInfo> discoveredRooms = new Dictionary<string, DiscoveredRoomInfo>();
        private readonly ConcurrentQueue<ReceivedUdpPacket> incomingQueue = new ConcurrentQueue<ReceivedUdpPacket>();

        private struct ReceivedUdpPacket
        {
            public string text;
            public IPEndPoint sender;
        }

        public IReadOnlyCollection<DiscoveredRoomInfo> DiscoveredRooms => discoveredRooms.Values;
        public bool IsBroadcastingHost => isBroadcastingHost;
        public bool IsSearchingClient => isSearchingClient;

        public int HostCurrentPlayers
        {
            get => hostCurrentPlayers;
            set => hostCurrentPlayers = Mathf.Clamp(value, 1, hostMaxPlayers);
        }

        private void OnDisable()
        {
            StopAllDiscovery();
        }

        private void OnDestroy()
        {
            StopAllDiscovery();
        }

        /// <summary>
        /// Inicia el anuncio UDP broadcast de la sala creada por el Host.
        /// </summary>
        public bool StartHostBroadcast(string roomName, string playerName, int gamePort = DefaultGamePort)
        {
            StopAllDiscovery();

            hostRoomId = Guid.NewGuid().ToString("N").Substring(0, 8);
            hostRoomName = SanitizeField(string.IsNullOrWhiteSpace(roomName) ? "Academia Vesper" : roomName.Trim());
            hostPlayerName = SanitizeField(string.IsNullOrWhiteSpace(playerName) ? "Investigador 1929" : playerName.Trim());
            hostGamePort = gamePort;
            hostCurrentPlayers = 1;
            hostMaxPlayers = 2;

            if (!OpenUdpSocket(DiscoveryHostPort))
            {
                return false;
            }

            isBroadcastingHost = true;
            nextBroadcastTime = 0f;
            SendHostAnnouncement( null);
            Debug.Log($"<color=green>[LAN Discovery]</color> Anunciando sala '{hostRoomName}' de '{hostPlayerName}' en puerto UDP {boundPort} (Juego: {hostGamePort}).");
            return true;
        }

        /// <summary>
        /// Inicia la búsqueda automática de salas en la LAN como Cliente.
        /// </summary>
        public bool StartClientSearch()
        {
            StopAllDiscovery();
            discoveredRooms.Clear();
            NotifyRoomsChanged();

            if (!OpenUdpSocket(DiscoveryClientPort))
            {
                // Si el puerto de cliente está ocupado en la misma máquina, abrir puerto efímero
                if (!OpenUdpSocket(0))
                {
                    return false;
                }
            }

            isSearchingClient = true;
            SendClientDiscoveryProbe();
            Debug.Log($"<color=cyan>[LAN Discovery]</color> Buscando salas en la red local (escuchando en puerto UDP {boundPort})...");
            return true;
        }

        /// <summary>
        /// Fuerza un nuevo barrido de búsqueda LAN inmediata ("Buscar de nuevo").
        /// </summary>
        public void RefreshClientSearch()
        {
            if (!isSearchingClient || udpSocket == null)
            {
                StartClientSearch();
                return;
            }

            discoveredRooms.Clear();
            NotifyRoomsChanged();
            SendClientDiscoveryProbe();
        }

        /// <summary>
        /// Envía una solicitud rápida de verificación al Host antes de conectar Netcode,
        /// para detectar al instante: Sala llena, Versión distinta, o bloqueo de Firewall / IP inalcanzable.
        /// </summary>
        public void BeginVerifyAndJoin(string targetIp, int targetGamePort)
        {
            if (udpSocket == null)
            {
                OpenUdpSocket(0);
            }

            pendingJoinIp = string.IsNullOrWhiteSpace(targetIp) ? "127.0.0.1" : targetIp.Trim();
            pendingJoinPort = targetGamePort > 0 ? targetGamePort : DefaultGamePort;
            waitingJoinVerify = true;
            joinVerifyDeadline = Time.realtimeSinceStartup + 3.5f;

            string req = $"{MagicHeader}|JOIN_REQ|{ProtocolGameVersion}";
            byte[] data = Encoding.UTF8.GetBytes(req);

            try
            {
                if (IPAddress.TryParse(pendingJoinIp, out IPAddress parsedIp))
                {
                    IPEndPoint ep = new IPEndPoint(parsedIp, DiscoveryHostPort);
                    udpSocket?.Send(data, data.Length, ep);
                }
                else
                {
                    SendPacketToAllBroadcastTargets(data, DiscoveryHostPort);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LAN Discovery] Error al enviar solicitud de unión a {pendingJoinIp}: {ex.Message}");
            }
        }

        public void StopAllDiscovery()
        {
            isBroadcastingHost = false;
            isSearchingClient = false;
            waitingJoinVerify = false;

            if (udpSocket != null)
            {
                try
                {
                    udpSocket.Close();
                }
                catch { }
                udpSocket = null;
            }
        }

        public void NotifyHostClientLeaving(string hostIp)
        {
            string target = string.IsNullOrWhiteSpace(hostIp) ? "127.0.0.1" : hostIp.Trim();
            string msg = $"{MagicHeader}|LEAVE_NOTIFY";
            byte[] data = Encoding.UTF8.GetBytes(msg);

            try
            {
                using (UdpClient tempClient = new UdpClient())
                {
                    if (IPAddress.TryParse(target, out IPAddress parsed))
                    {
                        tempClient.Send(data, data.Length, new IPEndPoint(parsed, DiscoveryHostPort));
                    }
                    else
                    {
                        tempClient.EnableBroadcast = true;
                        tempClient.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, DiscoveryHostPort));
                    }
                }
            }
            catch { }
        }

        public string GetLocalIPv4()
        {
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up) continue;
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                    IPInterfaceProperties ipProps = ni.GetIPProperties();
                    foreach (UnicastIPAddressInformation ua in ipProps.UnicastAddresses)
                    {
                        if (ua.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ua.Address))
                        {
                            return ua.Address.ToString();
                        }
                    }
                }
            }
            catch { }
            return "127.0.0.1";
        }

        private void Update()
        {
            ProcessIncomingPackets();

            float now = Time.realtimeSinceStartup;

            if (isBroadcastingHost && now >= nextBroadcastTime)
            {
                nextBroadcastTime = now + BroadcastInterval;
                SendHostAnnouncement(null);
            }

            if (isSearchingClient && now >= nextPruneTime)
            {
                nextPruneTime = now + 1.0f;
                PruneExpiredRooms(now);
            }

            if (waitingJoinVerify && now >= joinVerifyDeadline)
            {
                waitingJoinVerify = false;
                OnJoinVerifyResult?.Invoke(
                    LanJoinVerifyStatus.TimeoutOrFirewall,
                    pendingJoinIp,
                    pendingJoinPort,
                    "No hubo respuesta de la sala. Verifica la IP o permite el juego en el Firewall de Windows (Redes privadas y públicas).");
            }
        }

        private bool OpenUdpSocket(int preferredPort)
        {
            try
            {
                udpSocket = new UdpClient();
                udpSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpSocket.ExclusiveAddressUse = false;
                udpSocket.EnableBroadcast = true;
                udpSocket.Client.Bind(new IPEndPoint(IPAddress.Any, preferredPort));
                boundPort = ((IPEndPoint)udpSocket.Client.LocalEndPoint).Port;
                udpSocket.BeginReceive(OnUdpDataReceived, udpSocket);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LAN Discovery] No se pudo enlazar puerto UDP {preferredPort}: {ex.Message}");
                udpSocket = null;
                return false;
            }
        }

        private void OnUdpDataReceived(IAsyncResult ar)
        {
            UdpClient client = ar.AsyncState as UdpClient;
            if (client == null || client != udpSocket) return;

            try
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                byte[] bytes = client.EndReceive(ar, ref sender);
                if (bytes != null && bytes.Length > 0)
                {
                    string text = Encoding.UTF8.GetString(bytes);
                    incomingQueue.Enqueue(new ReceivedUdpPacket { text = text, sender = sender });
                }
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch
            {
                // Ignorar paquetes corruptos o cierre de socket
            }

            try
            {
                if (udpSocket != null)
                {
                    udpSocket.BeginReceive(OnUdpDataReceived, udpSocket);
                }
            }
            catch { }
        }

        private void ProcessIncomingPackets()
        {
            while (incomingQueue.TryDequeue(out ReceivedUdpPacket pkt))
            {
                if (string.IsNullOrEmpty(pkt.text) || !pkt.text.StartsWith(MagicHeader)) continue;
                string[] parts = pkt.text.Split('|');
                if (parts.Length < 2) continue;

                string cmd = parts[1];

                // 1. Cliente solicita descubrir salas -> El Host responde directamente al emisor
                if (cmd == "DISCOVER_REQ" && isBroadcastingHost)
                {
                    SendHostAnnouncement(pkt.sender);
                }
                // 2. Host anuncia su sala -> El Cliente la añade/actualiza en su lista
                else if (cmd == "ROOM_ANNOUNCE" && isSearchingClient && parts.Length >= 11)
                {
                    HandleRoomAnnouncement(parts, pkt.sender);
                }
                // 3. Cliente solicita validación antes de entrar -> El Host verifica versión y cupo (1/2 vs 2/2)
                else if (cmd == "JOIN_REQ" && isBroadcastingHost && parts.Length >= 3)
                {
                    string clientVer = parts[2];
                    SendJoinAckToClient(pkt.sender, clientVer);
                }
                // 4. Respuesta del Host a la validación de unión del Cliente
                else if (cmd == "JOIN_ACK" && waitingJoinVerify && parts.Length >= 4)
                {
                    HandleJoinAck(parts, pkt.sender);
                }
                // 5. Notificación de salida del 2.º jugador
                else if (cmd == "LEAVE_NOTIFY" && isBroadcastingHost)
                {
                    hostCurrentPlayers = 1;
                    if (LanSessionManager.Instance != null)
                    {
                        LanSessionManager.Instance.SetConnectedPlayerCount(1);
                    }
                }
            }
        }

        private void SendHostAnnouncement(IPEndPoint directTarget)
        {
            if (udpSocket == null) return;

            GetCurrentNightMetadata(out int nightIdx, out string nightDisplay);

            // Formato: HEADER|ROOM_ANNOUNCE|roomId|roomName|hostPlayerName|gamePort|nightIdx|nightDisplay|gameVersion|currentPlayers|maxPlayers
            string msg = string.Join("|",
                MagicHeader,
                "ROOM_ANNOUNCE",
                hostRoomId,
                hostRoomName,
                hostPlayerName,
                hostGamePort.ToString(),
                nightIdx.ToString(),
                nightDisplay,
                ProtocolGameVersion,
                hostCurrentPlayers.ToString(),
                hostMaxPlayers.ToString());

            byte[] data = Encoding.UTF8.GetBytes(msg);

            if (directTarget != null)
            {
                try { udpSocket.Send(data, data.Length, directTarget); } catch { }
            }

            SendPacketToAllBroadcastTargets(data, DiscoveryClientPort);
        }

        private void SendClientDiscoveryProbe()
        {
            if (udpSocket == null) return;
            string probe = $"{MagicHeader}|DISCOVER_REQ|{ProtocolGameVersion}";
            byte[] data = Encoding.UTF8.GetBytes(probe);
            SendPacketToAllBroadcastTargets(data, DiscoveryHostPort);
        }

        private void SendJoinAckToClient(IPEndPoint clientEndPoint, string clientVersion)
        {
            if (udpSocket == null || clientEndPoint == null) return;

            string status;
            if (!string.Equals(clientVersion, ProtocolGameVersion, StringComparison.OrdinalIgnoreCase))
            {
                status = "VERSION";
            }
            else if (hostCurrentPlayers >= hostMaxPlayers)
            {
                status = "FULL";
            }
            else
            {
                status = "OK";
            }

            string ack = $"{MagicHeader}|JOIN_ACK|{status}|{hostGamePort}|{ProtocolGameVersion}|{hostRoomName}";
            byte[] data = Encoding.UTF8.GetBytes(ack);
            try
            {
                udpSocket.Send(data, data.Length, clientEndPoint);
            }
            catch { }

            if (status == "OK")
            {
                hostCurrentPlayers = 2;
                if (LanSessionManager.Instance != null)
                {
                    LanSessionManager.Instance.SetConnectedPlayerCount(2);
                }
            }
        }

        private void HandleJoinAck(string[] parts, IPEndPoint sender)
        {
            waitingJoinVerify = false;
            string status = parts[2];
            int hostPort = int.TryParse(parts[3], out int p) ? p : DefaultGamePort;
            string hostVer = parts.Length > 4 ? parts[4] : ProtocolGameVersion;
            string rName = parts.Length > 5 ? parts[5] : "Sala LAN";
            string resolvedIp = sender != null ? sender.Address.ToString() : pendingJoinIp;

            if (status == "OK")
            {
                OnJoinVerifyResult?.Invoke(LanJoinVerifyStatus.Approved, resolvedIp, hostPort, $"Conectando a '{rName}' ({resolvedIp}:{hostPort})...");
            }
            else if (status == "FULL")
            {
                OnJoinVerifyResult?.Invoke(LanJoinVerifyStatus.RoomFull, resolvedIp, hostPort, "Sala llena (2/2 investigadores ya están dentro).");
            }
            else if (status == "VERSION")
            {
                OnJoinVerifyResult?.Invoke(LanJoinVerifyStatus.VersionMismatch, resolvedIp, hostPort, $"Versión distinta (Sala: v{hostVer} · Tu juego: v{ProtocolGameVersion}).");
            }
        }

        private void HandleRoomAnnouncement(string[] parts, IPEndPoint sender)
        {
            string roomId = parts[2];
            string roomName = parts[3];
            string hostPlayer = parts[4];
            int gamePort = int.TryParse(parts[5], out int gp) ? gp : DefaultGamePort;
            int nightIdx = int.TryParse(parts[6], out int ni) ? ni : 0;
            string nightDisplay = parts[7];
            string ver = parts[8];
            int curPlayers = int.TryParse(parts[9], out int cp) ? cp : 1;
            int maxPlayers = int.TryParse(parts[10], out int mp) ? mp : 2;
            string ip = sender != null ? sender.Address.ToString() : "127.0.0.1";

            string key = $"{ip}:{gamePort}:{roomId}";
            bool isNew = !discoveredRooms.TryGetValue(key, out DiscoveredRoomInfo info);
            if (isNew)
            {
                info = new DiscoveredRoomInfo();
                discoveredRooms[key] = info;
            }

            bool changed = isNew ||
                           info.currentPlayers != curPlayers ||
                           info.nightIndex != nightIdx ||
                           info.roomName != roomName;

            info.roomId = roomId;
            info.roomName = roomName;
            info.hostPlayerName = hostPlayer;
            info.hostIp = ip;
            info.gamePort = gamePort;
            info.nightIndex = nightIdx;
            info.nightDisplay = nightDisplay;
            info.gameVersion = ver;
            info.currentPlayers = curPlayers;
            info.maxPlayers = maxPlayers;
            info.lastSeenRealtime = Time.realtimeSinceStartup;

            if (changed)
            {
                NotifyRoomsChanged();
            }
        }

        private void PruneExpiredRooms(float now)
        {
            List<string> expired = null;
            foreach (KeyValuePair<string, DiscoveredRoomInfo> kv in discoveredRooms)
            {
                if (now - kv.Value.lastSeenRealtime > RoomExpireSeconds)
                {
                    if (expired == null) expired = new List<string>();
                    expired.Add(kv.Key);
                }
            }

            if (expired != null && expired.Count > 0)
            {
                foreach (string k in expired) discoveredRooms.Remove(k);
                NotifyRoomsChanged();
            }
        }

        private void NotifyRoomsChanged()
        {
            List<DiscoveredRoomInfo> list = new List<DiscoveredRoomInfo>(discoveredRooms.Values);
            OnRoomsUpdated?.Invoke(list);
        }

        private void SendPacketToAllBroadcastTargets(byte[] data, int targetPort)
        {
            if (udpSocket == null || data == null) return;

            HashSet<IPAddress> targets = new HashSet<IPAddress>
            {
                IPAddress.Broadcast,
                IPAddress.Loopback
            };

            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up) continue;
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                    IPInterfaceProperties ipProps = ni.GetIPProperties();
                    foreach (UnicastIPAddressInformation ua in ipProps.UnicastAddresses)
                    {
                        if (ua.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                        IPAddress mask = ua.IPv4Mask;
                        if (mask == null) continue;

                        byte[] ipBytes = ua.Address.GetAddressBytes();
                        byte[] maskBytes = mask.GetAddressBytes();
                        if (ipBytes.Length == 4 && maskBytes.Length == 4)
                        {
                            byte[] bcast = new byte[4];
                            for (int i = 0; i < 4; i++)
                            {
                                bcast[i] = (byte)(ipBytes[i] | (maskBytes[i] ^ 255));
                            }
                            targets.Add(new IPAddress(bcast));
                        }
                    }
                }
            }
            catch { }

            foreach (IPAddress addr in targets)
            {
                try
                {
                    udpSocket.Send(data, data.Length, new IPEndPoint(addr, targetPort));
                }
                catch { }
            }
        }

        private static void GetCurrentNightMetadata(out int nightIdx, out string nightDisplay)
        {
            if (NightManager.Instance != null && NightManager.Instance.Current != null)
            {
                nightIdx = NightManager.Instance.CurrentNightIndex;
                NightDefinition d = NightManager.Instance.Current;
                nightDisplay = $"Noche {d.roman} — {d.title} ({d.clockTime})";
                return;
            }

            List<NightDefinition> defs = NightManager.BuildDefaultNights();
            nightIdx = Mathf.Clamp(PlayerPrefs.GetInt(NightManager.PrefsKey, 0), 0, defs.Count - 1);
            NightDefinition cur = defs[nightIdx];
            nightDisplay = $"Noche {cur.roman} — {cur.title} ({cur.clockTime})";
        }

        private static string SanitizeField(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Replace("|", "-").Replace("\n", " ").Replace("\r", "");
        }
    }
}
