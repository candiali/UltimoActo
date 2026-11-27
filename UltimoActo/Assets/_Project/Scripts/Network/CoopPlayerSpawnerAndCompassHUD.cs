using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ULTIMOACTO_NETCODE
using Unity.Collections;
using Unity.Netcode;
#endif
using UltimoActo.Core;
using UltimoActo.Interaction;
using UltimoActo.Player;
using UltimoActo.UI;

namespace UltimoActo.Network
{
    /// <summary>
    /// Paso 3 — Gestor de Spawning de Jugadores en Red e Indicador de Presencia del Compañero:
    /// - Asegura que el Player de la escena (o prefab) tenga NetworkObject, ClientNetworkTransform y NetworkCoopPlayer.
    /// - En modo Host (1929): spawnea al jugador anfitrión como PlayerObject del Host y, cuando se conecta el
    ///   segundo jugador (Cliente = Ruinas), instancia y spawnea automáticamente su PlayerObject en una posición
    ///   segura junto al anfitrión.
    /// - En modo Cliente (Ruinas): desactiva el Player local de plantilla hasta recibir su NetworkObject con IsOwner.
    /// - Muestra un indicador discreto en pantalla (HUD) de dónde está el otro jugador (nombre, época, distancia en metros
    ///   y flecha direccional cuando está fuera del campo visual), además de actualizar el punto del compañero en el mapa.
    /// - Incluye soporte para prueba rápida en un solo equipo ([F2]) creando un compañero simulado en la otra época
    ///   con su silueta temporal y su linterna activa.
    /// </summary>
    public class CoopPlayerSpawnerAndCompassHUD : MonoBehaviour
    {
        public static CoopPlayerSpawnerAndCompassHUD Instance { get; private set; }

        public const string MsgCoopPlayerPose = "UltimoActo_CoopPlayerPose_Msg";

        private GameObject runtimePlayerPrefabTemplate;
        private NetworkCoopPlayer simulatedPartnerInstance;
        private NetworkCoopPlayer liveRemotePartnerInstance;
        private float simulatedOrbitAngle;
        private float nextPoseBroadcastTime;
        private bool poseHandlerRegistered;

        // HUD discreto de ubicación del otro jugador
        private Canvas compassCanvas;
        private CanvasGroup markerCanvasGroup;
        private RectTransform markerRect;
        private Image markerOuterBorder;
        private Image markerInnerFill;
        private Image markerDirectionArrow;
        private TextMeshProUGUI markerTopTitle;
        private TextMeshProUGUI markerBottomInfo;

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
            if (FindFirstObjectByType<CoopPlayerSpawnerAndCompassHUD>() != null) return;
            if (FindFirstObjectByType<PlayerMovement>() == null) return;

            GameObject go = new GameObject("CoopPlayerSpawnerAndCompassHUD");
            go.AddComponent<CoopPlayerSpawnerAndCompassHUD>();
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
            BuildPartnerCompassHUD();
            SetupScenePlayerAndNetworkCallbacks();

            if (LanSessionManager.Instance != null)
            {
                LanSessionManager.Instance.OnConnectedPlayerCountChanged += OnPlayerCountChangedHandler;
                LanSessionManager.Instance.OnRoomOpenedInGame += OnRoomOpenedInGameHandler;
            }
        }

        private void OnDestroy()
        {
            if (LanSessionManager.Instance != null)
            {
                LanSessionManager.Instance.OnConnectedPlayerCountChanged -= OnPlayerCountChangedHandler;
                LanSessionManager.Instance.OnRoomOpenedInGame -= OnRoomOpenedInGameHandler;
            }

#if ULTIMOACTO_NETCODE
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnNetcodeClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnNetcodeClientDisconnected;
            }
#endif
            if (runtimePlayerPrefabTemplate != null)
            {
                Destroy(runtimePlayerPrefabTemplate);
            }
            if (Instance == this) Instance = null;
        }

        private void SetupScenePlayerAndNetworkCallbacks()
        {
            PlayerMovement existingMove = FindFirstObjectByType<PlayerMovement>();
            if (existingMove == null) return;

            GameObject scenePlayer = existingMove.gameObject;
            EnsurePlayerHasNetworkComponents(scenePlayer);

            LanSessionManager session = LanSessionManager.EnsureInstance();

#if ULTIMOACTO_NETCODE
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnNetcodeClientConnected;
                NetworkManager.Singleton.OnClientConnectedCallback += OnNetcodeClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnNetcodeClientDisconnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnNetcodeClientDisconnected;
            }
#endif

            // Configurar el Player de la escena como jugador local (Host/Solo = 1929, Cliente = Ruinas)
            NetworkCoopPlayer coop = scenePlayer.GetComponent<NetworkCoopPlayer>();
            if (coop != null)
            {
                coop.ConfigureAsLocalOwner();
            }

            if (session.CurrentMode == LanSessionMode.Client && EraVisualManager.Instance != null)
            {
                EraVisualManager.Instance.SetEra(EraVisual.Ruins, true);
            }

            TryRegisterPoseMessagingHandler();
        }

        private static void EnsurePlayerHasNetworkComponents(GameObject playerObj)
        {
            if (playerObj == null) return;

#if ULTIMOACTO_NETCODE
            // Importante: NO añadir NetworkObject en runtime a un GameObject de escena sin GlobalObjectIdHash serializado,
            // ya que Netcode emite el error "Detected NetworkObject GlobalObjectIdHash value of 0!".
            // La posición, linterna y época se sincronizan limpiamente por CustomMessagingManager.
            NetworkObject preExistingNetObj = playerObj.GetComponent<NetworkObject>();
            if (preExistingNetObj != null && !preExistingNetObj.IsSpawned)
            {
                DestroyImmediate(preExistingNetObj);
            }
#endif
            if (playerObj.GetComponent<NetworkCoopPlayer>() == null)
            {
                playerObj.AddComponent<NetworkCoopPlayer>();
            }
        }

        private void OnRoomOpenedInGameHandler()
        {
            TryRegisterPoseMessagingHandler();
        }

#if ULTIMOACTO_NETCODE
        private void OnNetcodeClientConnected(ulong clientId)
        {
            TryRegisterPoseMessagingHandler();
            if (NetworkManager.Singleton == null) return;
            if (clientId != NetworkManager.Singleton.LocalClientId)
            {
                DestroySimulatedPartnerIfPresent();
            }
        }

        private void OnNetcodeClientDisconnected(ulong clientId)
        {
            if (NetworkManager.Singleton == null) return;
            if (clientId == NetworkManager.Singleton.LocalClientId) return;

            if (liveRemotePartnerInstance != null)
            {
                Destroy(liveRemotePartnerInstance.gameObject);
                liveRemotePartnerInstance = null;
            }
        }
#endif

        private void TryRegisterPoseMessagingHandler()
        {
#if ULTIMOACTO_NETCODE
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening) return;
            if (NetworkManager.Singleton.CustomMessagingManager == null) return;

            var cmm = NetworkManager.Singleton.CustomMessagingManager;
            cmm.UnregisterNamedMessageHandler(MsgCoopPlayerPose);
            cmm.RegisterNamedMessageHandler(MsgCoopPlayerPose, OnReceivedRemotePlayerPose);
            poseHandlerRegistered = true;
#endif
        }

#if ULTIMOACTO_NETCODE
        private void OnReceivedRemotePlayerPose(ulong senderClientId, FastBufferReader reader)
        {
            if (NetworkManager.Singleton == null || senderClientId == NetworkManager.Singleton.LocalClientId) return;

            try
            {
                reader.ReadValueSafe(out float px);
                reader.ReadValueSafe(out float py);
                reader.ReadValueSafe(out float pz);
                reader.ReadValueSafe(out float yaw);
                reader.ReadValueSafe(out float pitch);
                reader.ReadValueSafe(out byte flags);
                reader.ReadValueSafe(out FixedString64Bytes peerNameFs);

                bool flOn = (flags & 1) != 0;
                EraVisual peerEra = ((flags & 2) != 0) ? EraVisual.Ruins : EraVisual.Glory1929;
                string peerName = peerNameFs.ToString();

                DestroySimulatedPartnerIfPresent();

                if (liveRemotePartnerInstance == null)
                {
                    GameObject remObj = new GameObject($"CoopPartner_NetPeer_{senderClientId}");
                    liveRemotePartnerInstance = remObj.AddComponent<NetworkCoopPlayer>();
                }

                liveRemotePartnerInstance.ApplyLiveNetworkTelemetry(
                    new Vector3(px, py, pz),
                    yaw,
                    pitch,
                    flOn,
                    peerEra,
                    peerName);
            }
            catch { }
        }
#endif

        private void BroadcastLocalPlayerPoseIfConnected()
        {
#if ULTIMOACTO_NETCODE
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
            {
                poseHandlerRegistered = false;
                return;
            }
            if (!poseHandlerRegistered)
            {
                TryRegisterPoseMessagingHandler();
            }
            if (NetworkCoopPlayer.LocalPlayer == null) return;
            if (Time.unscaledTime < nextPoseBroadcastTime) return;
            nextPoseBroadcastTime = Time.unscaledTime + 0.04f; // 25 Hz

            var cmm = NetworkManager.Singleton.CustomMessagingManager;
            if (cmm == null) return;

            NetworkCoopPlayer local = NetworkCoopPlayer.LocalPlayer;
            Vector3 pos = local.transform.position;
            float yaw = local.transform.eulerAngles.y;
            float pitch = local.SyncedCameraPitch;
            byte flags = 0;
            if (local.IsFlashlightActive) flags |= 1;
            if (local.AssignedEra == EraVisual.Ruins) flags |= 2;
            FixedString64Bytes nameFs = new FixedString64Bytes(
                string.IsNullOrEmpty(local.DisplayName) ? "Investigador" : local.DisplayName);

            using FastBufferWriter writer = new FastBufferWriter(128, Unity.Collections.Allocator.Temp);
            writer.WriteValueSafe(pos.x);
            writer.WriteValueSafe(pos.y);
            writer.WriteValueSafe(pos.z);
            writer.WriteValueSafe(yaw);
            writer.WriteValueSafe(pitch);
            writer.WriteValueSafe(flags);
            writer.WriteValueSafe(nameFs);

            if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
            {
                cmm.SendNamedMessageToAll(MsgCoopPlayerPose, writer, NetworkDelivery.Unreliable);
            }
            else
            {
                cmm.SendNamedMessage(MsgCoopPlayerPose, NetworkManager.ServerClientId, writer, NetworkDelivery.Unreliable);
            }
#endif
        }

        /// <summary>
        /// Cuando el usuario pulsa [F2] en el Editor (o cuando cambia el número de jugadores a 2 sin un segundo cliente Netcode),
        /// crea o elimina un compañero simulado en la otra época para poder verificar en un solo equipo:
        /// 1. La silueta temporal del otro jugador acorde a su época.
        /// 2. Su linterna iluminando en tiempo real desde la otra época.
        /// 3. El indicador discreto en pantalla y en el mapa de dónde está.
        /// </summary>
        private void OnPlayerCountChangedHandler(int count)
        {
            if (count >= 2)
            {
                bool hasRealRemote = NetworkCoopPlayer.RemotePlayer != null && !NetworkCoopPlayer.RemotePlayer.IsSimulatedPreview;
                if (!hasRealRemote)
                {
                    SpawnOrUpdateSimulatedPartner();
                }
            }
            else
            {
                DestroySimulatedPartnerIfPresent();
            }
        }

        private void SpawnOrUpdateSimulatedPartner()
        {
            if (simulatedPartnerInstance != null) return;
            if (NetworkCoopPlayer.LocalPlayer == null) return;

            Transform localT = NetworkCoopPlayer.LocalPlayer.transform;
            Vector3 spawnPos = localT.position + localT.forward * 2.8f + localT.right * 1.1f;
            Quaternion spawnRot = Quaternion.LookRotation(-localT.forward, Vector3.up);

            GameObject simObj = new GameObject("CoopPartner_TemporalPresence_Ruins");
            simObj.transform.SetPositionAndRotation(spawnPos, spawnRot);

            simulatedPartnerInstance = simObj.AddComponent<NetworkCoopPlayer>();
            EraVisual localEra = EraVisualManager.Instance != null ? EraVisualManager.Instance.CurrentEra : EraVisual.Glory1929;
            EraVisual partnerEra = localEra == EraVisual.Glory1929 ? EraVisual.Ruins : EraVisual.Glory1929;
            string partnerName = partnerEra == EraVisual.Ruins ? "Compañero (Ruinas)" : "Compañero (1929)";

            simulatedPartnerInstance.ConfigureAsSimulatedPreviewPartner(partnerEra, partnerName);
            simulatedOrbitAngle = 0f;
        }

        private void DestroySimulatedPartnerIfPresent()
        {
            if (simulatedPartnerInstance != null)
            {
                Destroy(simulatedPartnerInstance.gameObject);
                simulatedPartnerInstance = null;
            }
        }

        private void Update()
        {
            BroadcastLocalPlayerPoseIfConnected();
            UpdateSimulatedPartnerMotion();
            UpdatePartnerCompassIndicator();
        }

        private void UpdateSimulatedPartnerMotion()
        {
            if (simulatedPartnerInstance == null || NetworkCoopPlayer.LocalPlayer == null) return;

            Transform localT = NetworkCoopPlayer.LocalPlayer.transform;
            Vector3 toLocal = localT.position - simulatedPartnerInstance.transform.position;
            toLocal.y = 0f;
            float dist = toLocal.magnitude;

            // Si el jugador local se aleja a otra sala (> 6.5 m), el compañero simulado camina suavemente tras él
            if (dist > 6.5f)
            {
                Vector3 targetPos = localT.position - localT.forward * 2.6f + localT.right * 1.2f;
                simulatedPartnerInstance.transform.position = Vector3.MoveTowards(
                    simulatedPartnerInstance.transform.position,
                    targetPos,
                    3.2f * Time.deltaTime);
            }

            // Gira suavemente explorando con su linterna hacia donde mira o donde está el jugador local
            simulatedOrbitAngle += Time.deltaTime * 0.85f;
            Vector3 lookDir = (localT.position + localT.forward * 4.5f) - simulatedPartnerInstance.transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.05f)
            {
                Quaternion baseLook = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
                Quaternion sway = Quaternion.Euler(0f, Mathf.Sin(simulatedOrbitAngle) * 28f, 0f);
                simulatedPartnerInstance.transform.rotation = Quaternion.Slerp(
                    simulatedPartnerInstance.transform.rotation,
                    baseLook * sway,
                    Time.deltaTime * 3.5f);
            }

            float pitchWave = Mathf.Sin(simulatedOrbitAngle * 1.3f) * 10f;
            simulatedPartnerInstance.SetSimulatedFlashlightAndPitch(true, pitchWave);
        }

        private void UpdatePartnerCompassIndicator()
        {
            if (markerCanvasGroup == null || markerRect == null) return;

            NetworkCoopPlayer remote = NetworkCoopPlayer.RemotePlayer;
            NetworkCoopPlayer local = NetworkCoopPlayer.LocalPlayer;
            Camera cam = Camera.main;

            if (remote == null || local == null || remote == local || cam == null)
            {
                markerCanvasGroup.alpha = 0f;
                return;
            }

            if (DocumentUI.Instance != null && DocumentUI.Instance.IsOpen)
            {
                markerCanvasGroup.alpha = 0f;
                return;
            }

            markerCanvasGroup.alpha = 0.92f;

            Vector3 worldHead = remote.transform.position + new Vector3(0f, 1.92f, 0f);
            float distanceMeters = Vector3.Distance(local.transform.position, remote.transform.position);

            bool partnerIs1929 = remote.AssignedEra == EraVisual.Glory1929;
            string eraShort = partnerIs1929 ? "ÉPOCA 1929" : "ÉPOCA RUINAS";
            string flashState = remote.IsFlashlightActive ? "Linterna ON" : "Linterna OFF";
            bool partnerTalking = LanVoiceChatManager.Instance != null && LanVoiceChatManager.Instance.IsRemoteSpeaking;
            string speakTag = partnerTalking ? "  ·  <color=#78E89A>● HABLANDO</color>" : "";

            if (markerTopTitle != null)
            {
                markerTopTitle.text = $"{remote.DisplayName} · {eraShort}";
                markerTopTitle.color = partnerIs1929
                    ? new Color(0.96f, 0.86f, 0.62f, 1f)
                    : new Color(0.72f, 0.86f, 0.98f, 1f);
            }

            if (markerBottomInfo != null)
            {
                markerBottomInfo.text = $"{distanceMeters:0.0} m  ·  {flashState}{speakTag}";
            }

            if (markerOuterBorder != null)
            {
                markerOuterBorder.color = partnerIs1929
                    ? new Color(0.85f, 0.68f, 0.34f, 0.85f)
                    : new Color(0.52f, 0.72f, 0.92f, 0.85f);
            }

            // Proyectar posición 3D a coordenadas de pantalla con sujeción elegante a los bordes
            Vector3 screenPt = cam.WorldToScreenPoint(worldHead);
            bool isBehind = screenPt.z < 0f;
            if (isBehind)
            {
                screenPt.x = Screen.width - screenPt.x;
                screenPt.y = Screen.height - screenPt.y;
            }

            float padX = 135f;
            float padY = 85f;
            float clampedX = Mathf.Clamp(screenPt.x, padX, Screen.width - padX);
            float clampedY = Mathf.Clamp(screenPt.y, padY, Screen.height - padY);
            bool isClamped = isBehind ||
                             Mathf.Abs(clampedX - screenPt.x) > 1f ||
                             Mathf.Abs(clampedY - screenPt.y) > 1f;

            if (isBehind)
            {
                // Si está detrás, empujar al borde izquierdo o derecho más cercano
                Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                Vector2 dir = (new Vector2(screenPt.x, screenPt.y) - center).normalized;
                if (dir.sqrMagnitude < 0.01f) dir = Vector2.right;
                clampedX = Mathf.Clamp(center.x + dir.x * Screen.width * 0.45f, padX, Screen.width - padX);
                clampedY = Mathf.Clamp(center.y + dir.y * Screen.height * 0.40f, padY, Screen.height - padY);
            }

            markerRect.position = new Vector3(clampedX, clampedY, 0f);

            if (markerDirectionArrow != null)
            {
                markerDirectionArrow.gameObject.SetActive(isClamped);
                if (isClamped)
                {
                    Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                    Vector2 dir = (new Vector2(clampedX, clampedY) - screenCenter).normalized;
                    float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                    markerDirectionArrow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, ang);
                }
            }
        }

        private void BuildPartnerCompassHUD()
        {
            GameObject canvasObj = new GameObject("CoopPartnerCompass_Canvas");
            canvasObj.transform.SetParent(transform, false);
            compassCanvas = canvasObj.AddComponent<Canvas>();
            compassCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            compassCanvas.sortingOrder = 58;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject markerObj = new GameObject("PartnerPresenceIndicator");
            markerObj.transform.SetParent(canvasObj.transform, false);
            markerRect = markerObj.AddComponent<RectTransform>();
            markerRect.sizeDelta = new Vector2(215f, 44f);

            markerCanvasGroup = markerObj.AddComponent<CanvasGroup>();
            markerCanvasGroup.alpha = 0f;
            markerCanvasGroup.blocksRaycasts = false;
            markerCanvasGroup.interactable = false;

            markerOuterBorder = markerObj.AddComponent<Image>();
            markerOuterBorder.color = new Color(0.52f, 0.72f, 0.92f, 0.85f);
            markerOuterBorder.raycastTarget = false;

            GameObject fillObj = new GameObject("InnerFill");
            fillObj.transform.SetParent(markerObj.transform, false);
            RectTransform fillRt = fillObj.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(1.5f, 1.5f);
            fillRt.offsetMax = new Vector2(-1.5f, -1.5f);

            markerInnerFill = fillObj.AddComponent<Image>();
            markerInnerFill.color = new Color(0.05f, 0.04f, 0.07f, 0.86f);
            markerInnerFill.raycastTarget = false;

            // Rombo / Flecha discreta superior para indicar dirección cuando está fuera de pantalla
            GameObject arrowObj = new GameObject("DirectionDiamond");
            arrowObj.transform.SetParent(markerObj.transform, false);
            RectTransform arrowRt = arrowObj.AddComponent<RectTransform>();
            arrowRt.anchorMin = new Vector2(0.5f, 1f);
            arrowRt.anchorMax = new Vector2(0.5f, 1f);
            arrowRt.pivot = new Vector2(0.5f, 0.5f);
            arrowRt.anchoredPosition = new Vector2(0f, 8f);
            arrowRt.sizeDelta = new Vector2(10f, 10f);

            markerDirectionArrow = arrowObj.AddComponent<Image>();
            markerDirectionArrow.color = new Color(0.95f, 0.84f, 0.58f, 0.95f);
            markerDirectionArrow.raycastTarget = false;

            // Línea 1: Nombre y época del compañero
            GameObject topObj = new GameObject("PartnerTitle");
            topObj.transform.SetParent(fillObj.transform, false);
            RectTransform topRt = topObj.AddComponent<RectTransform>();
            topRt.anchorMin = new Vector2(0f, 0.48f);
            topRt.anchorMax = new Vector2(1f, 0.96f);
            topRt.offsetMin = new Vector2(8f, 0f);
            topRt.offsetMax = new Vector2(-8f, 0f);

            markerTopTitle = topObj.AddComponent<TextMeshProUGUI>();
            markerTopTitle.fontSize = 12.5f;
            markerTopTitle.fontStyle = FontStyles.Bold;
            markerTopTitle.alignment = TextAlignmentOptions.Center;
            markerTopTitle.textWrappingMode = TextWrappingModes.NoWrap;
            markerTopTitle.overflowMode = TextOverflowModes.Ellipsis;
            markerTopTitle.raycastTarget = false;

            // Línea 2: Distancia en metros y estado de su linterna
            GameObject botObj = new GameObject("PartnerDistanceAndLight");
            botObj.transform.SetParent(fillObj.transform, false);
            RectTransform botRt = botObj.AddComponent<RectTransform>();
            botRt.anchorMin = new Vector2(0f, 0.04f);
            botRt.anchorMax = new Vector2(1f, 0.50f);
            botRt.offsetMin = new Vector2(8f, 0f);
            botRt.offsetMax = new Vector2(-8f, 0f);

            markerBottomInfo = botObj.AddComponent<TextMeshProUGUI>();
            markerBottomInfo.fontSize = 11.5f;
            markerBottomInfo.color = new Color(0.88f, 0.86f, 0.82f, 0.95f);
            markerBottomInfo.alignment = TextAlignmentOptions.Center;
            markerBottomInfo.raycastTarget = false;
        }
    }
}
