using System;
using System.Collections.Generic;
using UnityEngine;
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
    /// Paso 3 — Jugador en red:
    /// - Controla que movimiento (PlayerMovement), cámara (PlayerCameraLook, Camera, AudioListener)
    ///   e interacción (PlayerInteraction) estén activos SOLO para el dueño (IsOwner).
    /// - Sincroniza posición/rotación (junto con ClientNetworkTransform), inclinación vertical (pitch)
    ///   de la mirada, estado de la linterna (encendida/apagada y haz visible para el otro jugador),
    ///   y la época asignada al jugador (EraVisualManager.SetEra: Host = Glory1929, Client = Ruins).
    /// - Construye para el jugador remoto una silueta/presencia temporal acorde a su época
    ///   (usa los modelos 3D PF_Protagonista_A / PF_Protagonista_B o una silueta estilizada de época
    ///   con farol/linterna visible a través de los tiempos).
    /// </summary>
#if ULTIMOACTO_NETCODE
    public class NetworkCoopPlayer : NetworkBehaviour
#else
    public class NetworkCoopPlayer : MonoBehaviour
#endif
    {
        public static NetworkCoopPlayer LocalPlayer { get; private set; }
        public static NetworkCoopPlayer RemotePlayer { get; private set; }
        public static readonly List<NetworkCoopPlayer> ActivePlayers = new List<NetworkCoopPlayer>();

        [Header("Referencias del Jugador")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerCameraLook playerCameraLook;
        [SerializeField] private PlayerInteraction playerInteraction;
        [SerializeField] private FlashlightController localFlashlight;
        [SerializeField] private Camera firstPersonCamera;
        [SerializeField] private AudioListener audioListener;
        [SerializeField] private PlayerEraAssigner eraAssigner;

        // Visuales para cuando este objeto representa al OTRO jugador (remoto)
        private GameObject remotePresenceRoot;
        private Transform remoteHeadLightPivot;
        private Light remoteSpotLight;
        private Light remoteCloseFillLight;
        private Light remoteAuraLight;
        private Renderer[] remoteRenderers;
        private Material remotePresenceMat1929;
        private Material remotePresenceMatRuins;

        // Estado cacheado accesible tanto con Netcode como en simulación local
        private EraVisual assignedEra = EraVisual.Glory1929;
        private bool flashlightActive = true;
        private float syncedCameraPitch = 0f;
        private string syncedDisplayName = "Investigador";
        private bool isSimulatedPreviewPartner = false;

#if ULTIMOACTO_NETCODE
        private readonly NetworkVariable<int> netAssignedEra = new NetworkVariable<int>(
            (int)EraVisual.Glory1929,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private readonly NetworkVariable<bool> netFlashlightOn = new NetworkVariable<bool>(
            true,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private readonly NetworkVariable<float> netCameraPitch = new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private readonly NetworkVariable<FixedString64Bytes> netPlayerName = new NetworkVariable<FixedString64Bytes>(
            new FixedString64Bytes("Investigador"),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
#endif

        public EraVisual AssignedEra => assignedEra;
        public bool IsFlashlightActive => flashlightActive;
        public float SyncedCameraPitch => syncedCameraPitch;
        public string DisplayName => syncedDisplayName;
        public bool IsSimulatedPreview => isSimulatedPreviewPartner;

        // Interpolación suave para telemetría recibida por CustomMessagingManager
        private bool isNetworkRemotePeer = false;
        private Vector3 targetRemotePos;
        private Quaternion targetRemoteRot = Quaternion.identity;
        private bool hasRemotePoseSample = false;

        public bool IsLocalControlled
        {
            get
            {
                if (isSimulatedPreviewPartner || isNetworkRemotePeer) return false;
#if ULTIMOACTO_NETCODE
                if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && IsSpawned)
                {
                    return IsOwner;
                }
#endif
                return true;
            }
        }

        private void Awake()
        {
            CacheComponents();
            if (!ActivePlayers.Contains(this))
            {
                ActivePlayers.Add(this);
            }
        }

        private void Start()
        {
            if (isSimulatedPreviewPartner)
            {
                return;
            }

#if ULTIMOACTO_NETCODE
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && IsSpawned)
            {
                return;
            }
#endif
            // Modo solitario o antes de iniciar sesión Netcode: somos el jugador local
            ConfigureAsLocalOwner();
        }

#if ULTIMOACTO_NETCODE
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            CacheComponents();

            netAssignedEra.OnValueChanged += OnNetEraChanged;
            netFlashlightOn.OnValueChanged += OnNetFlashlightChanged;
            netCameraPitch.OnValueChanged += OnNetPitchChanged;
            netPlayerName.OnValueChanged += OnNetNameChanged;

            if (IsOwner)
            {
                ConfigureAsLocalOwner();

                EraVisual myEra = (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsHost && NetworkManager.Singleton.IsClient)
                    ? EraVisual.Ruins
                    : EraVisual.Glory1929;

                assignedEra = myEra;
                netAssignedEra.Value = (int)myEra;

                string pName = LanSessionManager.Instance != null
                    ? LanSessionManager.Instance.LocalPlayerName
                    : (myEra == EraVisual.Glory1929 ? "Investigador (1929)" : "Investigador (Ruinas)");
                syncedDisplayName = pName;
                netPlayerName.Value = new FixedString64Bytes(pName);

                bool flOn = localFlashlight != null ? (localFlashlight.HasFlashlight && localFlashlight.IsOn) : true;
                flashlightActive = flOn;
                netFlashlightOn.Value = flOn;

                if (EraVisualManager.Instance != null)
                {
                    EraVisualManager.Instance.SetEra(myEra, true);
                }
            }
            else
            {
                assignedEra = (EraVisual)netAssignedEra.Value;
                flashlightActive = netFlashlightOn.Value;
                syncedCameraPitch = netCameraPitch.Value;
                syncedDisplayName = netPlayerName.Value.ToString();

                ConfigureAsRemotePeer();
            }
        }

        public override void OnNetworkDespawn()
        {
            netAssignedEra.OnValueChanged -= OnNetEraChanged;
            netFlashlightOn.OnValueChanged -= OnNetFlashlightChanged;
            netCameraPitch.OnValueChanged -= OnNetPitchChanged;
            netPlayerName.OnValueChanged -= OnNetNameChanged;

            if (RemotePlayer == this) RemotePlayer = null;
            if (LocalPlayer == this) LocalPlayer = null;
            base.OnNetworkDespawn();
        }

        private void OnNetEraChanged(int prev, int current)
        {
            assignedEra = (EraVisual)current;
            ApplyRemoteEraStyling();
        }

        private void OnNetFlashlightChanged(bool prev, bool current)
        {
            flashlightActive = current;
            ApplyRemoteFlashlightState();
        }

        private void OnNetPitchChanged(float prev, float current)
        {
            syncedCameraPitch = current;
        }

        private void OnNetNameChanged(FixedString64Bytes prev, FixedString64Bytes current)
        {
            syncedDisplayName = current.ToString();
        }
#endif

        private void OnDestroy()
        {
            ActivePlayers.Remove(this);
            if (RemotePlayer == this) RemotePlayer = null;
            if (LocalPlayer == this) LocalPlayer = null;
        }

        private void CacheComponents()
        {
            if (characterController == null) characterController = GetComponent<CharacterController>();
            if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
            if (playerCameraLook == null) playerCameraLook = GetComponent<PlayerCameraLook>();
            if (playerInteraction == null) playerInteraction = GetComponent<PlayerInteraction>();
            if (eraAssigner == null) eraAssigner = GetComponent<PlayerEraAssigner>();
            if (firstPersonCamera == null) firstPersonCamera = GetComponentInChildren<Camera>(true);
            if (audioListener == null) audioListener = GetComponentInChildren<AudioListener>(true);
            if (localFlashlight == null) localFlashlight = GetComponentInChildren<FlashlightController>(true);
        }

        /// <summary>
        /// Activa movimiento, cámara, audio e interacción para el dueño de este jugador
        /// y vincula la UI de interacción, el mapa y el NightManager a este jugador local.
        /// </summary>
        public void ConfigureAsLocalOwner()
        {
            CacheComponents();
            LocalPlayer = this;
            gameObject.tag = "Player";

            if (characterController != null) characterController.enabled = true;
            if (playerMovement != null) playerMovement.enabled = true;
            if (playerCameraLook != null) playerCameraLook.enabled = true;
            if (playerInteraction != null) playerInteraction.enabled = true;

            if (firstPersonCamera != null)
            {
                firstPersonCamera.enabled = true;
                firstPersonCamera.gameObject.tag = "MainCamera";
            }
            if (audioListener != null)
            {
                audioListener.enabled = true;
            }
            if (localFlashlight != null)
            {
                localFlashlight.enabled = true;
            }

            if (remotePresenceRoot != null)
            {
                remotePresenceRoot.SetActive(false);
            }

            // Determinar época correspondiente al jugador local
            EraVisual targetEra = EraVisual.Glory1929;
            if (LanSessionManager.Instance != null && LanSessionManager.Instance.CurrentMode == LanSessionMode.Client)
            {
                targetEra = EraVisual.Ruins;
            }
            assignedEra = targetEra;

            if (eraAssigner != null)
            {
                eraAssigner.SetEra(targetEra);
            }
            else if (EraVisualManager.Instance != null)
            {
                EraVisualManager.Instance.SetEra(targetEra, false);
            }

            syncedDisplayName = LanSessionManager.Instance != null
                ? LanSessionManager.Instance.LocalPlayerName
                : (targetEra == EraVisual.Glory1929 ? "Investigador (1929)" : "Investigador (Ruinas)");

            RebindLocalSceneSystems();
        }

        /// <summary>
        /// Desactiva cámara local, movimiento e input de este objeto porque pertenece al OTRO jugador,
        /// y activa su silueta/presencia de época y su linterna sincronizada visible en nuestro mundo.
        /// </summary>
        public void ConfigureAsRemotePeer()
        {
            CacheComponents();
            RemotePlayer = this;

            // Evitar que FindGameObjectWithTag("Player") confunda al compañero remoto con el jugador local
            gameObject.tag = "Untagged";

            if (playerMovement != null) playerMovement.enabled = false;
            if (playerCameraLook != null) playerCameraLook.enabled = false;
            if (playerInteraction != null) playerInteraction.enabled = false;
            if (characterController != null) characterController.enabled = false;

            if (firstPersonCamera != null)
            {
                firstPersonCamera.enabled = false;
                firstPersonCamera.gameObject.tag = "Untagged";
            }
            if (audioListener != null)
            {
                audioListener.enabled = false;
            }
            if (localFlashlight != null)
            {
                localFlashlight.enabled = false;
                Light[] localLights = localFlashlight.GetComponentsInChildren<Light>(true);
                for (int i = 0; i < localLights.Length; i++)
                {
                    if (localLights[i] != null) localLights[i].enabled = false;
                }
            }

            EnsureRemotePresenceBuilt();
            if (remotePresenceRoot != null)
            {
                remotePresenceRoot.SetActive(true);
            }

            ApplyRemoteEraStyling();
            ApplyRemoteFlashlightState();
        }

        /// <summary>
        /// Crea o configura un compañero simulado (para probar la silueta, la linterna cruzada entre épocas
        /// y el indicador de posición en un solo equipo con [F2]).
        /// </summary>
        public void ConfigureAsSimulatedPreviewPartner(EraVisual partnerEra, string partnerName)
        {
            isSimulatedPreviewPartner = true;
            isNetworkRemotePeer = false;
            assignedEra = partnerEra;
            syncedDisplayName = partnerName;
            flashlightActive = true;
            ConfigureAsRemotePeer();
        }

        public void ApplyLiveNetworkTelemetry(
            Vector3 worldPos,
            float yawDegrees,
            float pitchDegrees,
            bool isFlashlightOn,
            EraVisual peerEra,
            string peerName)
        {
            if (!isNetworkRemotePeer)
            {
                isNetworkRemotePeer = true;
                isSimulatedPreviewPartner = false;
                ConfigureAsRemotePeer();
            }

            targetRemotePos = worldPos;
            targetRemoteRot = Quaternion.Euler(0f, yawDegrees, 0f);
            if (!hasRemotePoseSample)
            {
                hasRemotePoseSample = true;
                transform.SetPositionAndRotation(targetRemotePos, targetRemoteRot);
            }

            syncedCameraPitch = pitchDegrees;

            if (!string.IsNullOrEmpty(peerName))
            {
                syncedDisplayName = peerName;
            }

            if (assignedEra != peerEra)
            {
                assignedEra = peerEra;
                ApplyRemoteEraStyling();
            }

            if (flashlightActive != isFlashlightOn)
            {
                flashlightActive = isFlashlightOn;
                ApplyRemoteFlashlightState();
            }
        }

        private void RebindLocalSceneSystems()
        {
            InteractionUI ui = FindFirstObjectByType<InteractionUI>();
            if (ui != null && playerInteraction != null)
            {
                ui.BindToPlayerInteraction(playerInteraction);
            }

            AcademyMapUI map = FindFirstObjectByType<AcademyMapUI>();
            if (map != null)
            {
                map.BindLocalPlayer(transform, firstPersonCamera != null ? firstPersonCamera.transform : transform, GetComponent<SimpleInventory>());
            }
        }

        private void Update()
        {
            if (IsLocalControlled)
            {
                UpdateLocalOwnerState();
            }
            else
            {
                UpdateRemotePeerVisuals();
            }
        }

        private void UpdateLocalOwnerState()
        {
            if (EraVisualManager.Instance != null)
            {
                assignedEra = EraVisualManager.Instance.CurrentEra;
            }

            if (localFlashlight == null && FlashlightController.Instance != null)
            {
                localFlashlight = FlashlightController.Instance;
            }

            bool flNow = localFlashlight != null && localFlashlight.HasFlashlight && localFlashlight.IsOn && localFlashlight.Battery > 0f;
            float pitchNow = 0f;
            if (firstPersonCamera != null)
            {
                float rawX = firstPersonCamera.transform.localEulerAngles.x;
                pitchNow = rawX > 180f ? rawX - 360f : rawX;
            }

            flashlightActive = flNow;
            syncedCameraPitch = pitchNow;

#if ULTIMOACTO_NETCODE
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && IsSpawned && IsOwner)
            {
                if (netAssignedEra.Value != (int)assignedEra)
                {
                    netAssignedEra.Value = (int)assignedEra;
                }
                if (netFlashlightOn.Value != flNow)
                {
                    netFlashlightOn.Value = flNow;
                }
                if (Mathf.Abs(netCameraPitch.Value - pitchNow) > 0.4f)
                {
                    netCameraPitch.Value = pitchNow;
                }
            }
#endif
        }

        private void UpdateRemotePeerVisuals()
        {
            if (isNetworkRemotePeer && hasRemotePoseSample)
            {
                float dist = Vector3.Distance(transform.position, targetRemotePos);
                if (dist > 8.0f)
                {
                    transform.SetPositionAndRotation(targetRemotePos, targetRemoteRot);
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, targetRemotePos, Time.deltaTime * 16f);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRemoteRot, Time.deltaTime * 18f);
                }
            }

            if (remoteHeadLightPivot != null)
            {
                remoteHeadLightPivot.localRotation = Quaternion.Euler(syncedCameraPitch, 0f, 0f);
            }

            // Pulso sutil del aura temporal para que se perciba su presencia desde la otra época
            if (remoteAuraLight != null)
            {
                float pulse = 0.88f + 0.12f * Mathf.Sin(Time.time * 2.6f);
                remoteAuraLight.intensity = 1.65f * pulse;
            }
        }

        private void EnsureRemotePresenceBuilt()
        {
            if (remotePresenceRoot != null) return;

            remotePresenceRoot = new GameObject("RemoteTemporalPresence_Visual");
            remotePresenceRoot.transform.SetParent(transform, false);
            remotePresenceRoot.transform.localPosition = Vector3.zero;
            remotePresenceRoot.transform.localRotation = Quaternion.identity;

            // Importante: la presencia del otro jugador vive en la capa Default (0) para ser visible
            // desde AMBAS cámaras (tanto en 1929 como en Ruinas).
            remotePresenceRoot.layer = 0;

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            // Material de presencia cuando el compañero está en 1929 (eco dorado-marfil de la época de gloria)
            remotePresenceMat1929 = new Material(litShader);
            Color col1929 = new Color(0.95f, 0.82f, 0.54f, 1f);
            remotePresenceMat1929.color = col1929;
            if (remotePresenceMat1929.HasProperty("_BaseColor")) remotePresenceMat1929.SetColor("_BaseColor", col1929);
            if (remotePresenceMat1929.HasProperty("_Smoothness")) remotePresenceMat1929.SetFloat("_Smoothness", 0.65f);
            if (remotePresenceMat1929.HasProperty("_Metallic")) remotePresenceMat1929.SetFloat("_Metallic", 0.35f);
            remotePresenceMat1929.EnableKeyword("_EMISSION");
            if (remotePresenceMat1929.HasProperty("_EmissionColor"))
            {
                remotePresenceMat1929.SetColor("_EmissionColor", new Color(0.88f, 0.68f, 0.32f) * 1.35f);
            }

            // Material de presencia cuando el compañero está en las Ruinas (eco espectral azul-plata lunar)
            remotePresenceMatRuins = new Material(litShader);
            Color colRuins = new Color(0.62f, 0.78f, 0.96f, 1f);
            remotePresenceMatRuins.color = colRuins;
            if (remotePresenceMatRuins.HasProperty("_BaseColor")) remotePresenceMatRuins.SetColor("_BaseColor", colRuins);
            if (remotePresenceMatRuins.HasProperty("_Smoothness")) remotePresenceMatRuins.SetFloat("_Smoothness", 0.60f);
            if (remotePresenceMatRuins.HasProperty("_Metallic")) remotePresenceMatRuins.SetFloat("_Metallic", 0.25f);
            remotePresenceMatRuins.EnableKeyword("_EMISSION");
            if (remotePresenceMatRuins.HasProperty("_EmissionColor"))
            {
                remotePresenceMatRuins.SetColor("_EmissionColor", new Color(0.45f, 0.66f, 0.92f) * 1.35f);
            }

            // Construir silueta de investigador de época (gabardina Dark Academia, cabeza, brazos y linterna de latón en mano)
            GameObject bodyGroup = new GameObject("TemporalSilhouetteMesh");
            bodyGroup.transform.SetParent(remotePresenceRoot.transform, false);

            // Abrigo / Torso
            CreateSilhouettePart("Coat_Lower", PrimitiveType.Cylinder, bodyGroup.transform,
                new Vector3(0f, 0.58f, 0f), new Vector3(0.44f, 0.52f, 0.32f), Quaternion.identity);
            CreateSilhouettePart("Torso_Upper", PrimitiveType.Capsule, bodyGroup.transform,
                new Vector3(0f, 1.16f, 0f), new Vector3(0.42f, 0.36f, 0.28f), Quaternion.identity);
            // Cuello y Cabeza
            CreateSilhouettePart("Shoulders_Collar", PrimitiveType.Cube, bodyGroup.transform,
                new Vector3(0f, 1.40f, -0.02f), new Vector3(0.48f, 0.12f, 0.26f), Quaternion.identity);
            CreateSilhouettePart("Head", PrimitiveType.Sphere, bodyGroup.transform,
                new Vector3(0f, 1.62f, 0f), new Vector3(0.24f, 0.27f, 0.24f), Quaternion.identity);

            // Anillo de resonancia temporal en el suelo a los pies del compañero
            CreateSilhouettePart("TemporalFloorRing", PrimitiveType.Cylinder, bodyGroup.transform,
                new Vector3(0f, 0.03f, 0f), new Vector3(0.75f, 0.015f, 0.75f), Quaternion.identity);

            // Pivote de mirada y linterna del compañero (a la altura del pecho/ojos, sigue el pitch de su cámara)
            GameObject pivotObj = new GameObject("RemoteFlashlight_PitchPivot");
            pivotObj.transform.SetParent(remotePresenceRoot.transform, false);
            pivotObj.transform.localPosition = new Vector3(0.18f, 1.36f, 0.22f);
            remoteHeadLightPivot = pivotObj.transform;

            // Brazo derecho sosteniendo la linterna de latón
            CreateSilhouettePart("RightArm_Lantern", PrimitiveType.Capsule, remoteHeadLightPivot,
                new Vector3(0.02f, -0.06f, -0.10f), new Vector3(0.11f, 0.22f, 0.11f), Quaternion.Euler(78f, 0f, 0f));

            // Cuerpo de la linterna de dínamo del compañero
            CreateSilhouettePart("BrassFlashlight_Body", PrimitiveType.Cylinder, remoteHeadLightPivot,
                new Vector3(0f, 0f, 0.12f), new Vector3(0.09f, 0.14f, 0.09f), Quaternion.Euler(90f, 0f, 0f));

            // Haz de luz Spot del otro jugador (visible en la época local con alta intensidad URP)
            GameObject spotObj = new GameObject("RemotePlayer_SpotLight");
            spotObj.transform.SetParent(remoteHeadLightPivot, false);
            spotObj.transform.localPosition = new Vector3(0f, 0f, 0.26f);
            spotObj.transform.localRotation = Quaternion.identity;

            remoteSpotLight = spotObj.AddComponent<Light>();
            remoteSpotLight.type = LightType.Spot;
            remoteSpotLight.range = 40f;
            remoteSpotLight.spotAngle = 64f;
            remoteSpotLight.innerSpotAngle = 36f;
            remoteSpotLight.intensity = 32f;
            remoteSpotLight.cullingMask = ~0;
            remoteSpotLight.shadows = LightShadows.None;

            // Luz cercana del haz de la linterna del compañero
            GameObject closeObj = new GameObject("RemotePlayer_FlashlightCloseFill");
            closeObj.transform.SetParent(remoteHeadLightPivot, false);
            closeObj.transform.localPosition = new Vector3(0f, 0f, 0.30f);
            remoteCloseFillLight = closeObj.AddComponent<Light>();
            remoteCloseFillLight.type = LightType.Point;
            remoteCloseFillLight.range = 9.5f;
            remoteCloseFillLight.intensity = 3.2f;
            remoteCloseFillLight.cullingMask = ~0;
            remoteCloseFillLight.shadows = LightShadows.None;

            // Aura discreta que envuelve la silueta del compañero para distinguir su época
            GameObject auraObj = new GameObject("RemotePlayer_TemporalAuraLight");
            auraObj.transform.SetParent(remotePresenceRoot.transform, false);
            auraObj.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            remoteAuraLight = auraObj.AddComponent<Light>();
            remoteAuraLight.type = LightType.Point;
            remoteAuraLight.range = 4.2f;
            remoteAuraLight.intensity = 1.65f;
            remoteAuraLight.cullingMask = ~0;
            remoteAuraLight.shadows = LightShadows.None;

            remoteRenderers = bodyGroup.GetComponentsInChildren<Renderer>(true);
            List<Renderer> allRends = new List<Renderer>(remoteRenderers);
            allRends.AddRange(remoteHeadLightPivot.GetComponentsInChildren<Renderer>(true));
            remoteRenderers = allRends.ToArray();
        }

        private static GameObject CreateSilhouettePart(string name, PrimitiveType type, Transform parent, Vector3 localPos, Vector3 localScale, Quaternion localRot)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.layer = 0;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPos;
            part.transform.localRotation = localRot;
            part.transform.localScale = localScale;

            Collider col = part.GetComponent<Collider>();
            if (col != null)
            {
                Destroy(col);
            }
            return part;
        }

        private void ApplyRemoteEraStyling()
        {
            EnsureRemotePresenceBuilt();
            bool isPartner1929 = assignedEra == EraVisual.Glory1929;
            Material targetMat = isPartner1929 ? remotePresenceMat1929 : remotePresenceMatRuins;

            if (remoteRenderers != null && targetMat != null)
            {
                for (int i = 0; i < remoteRenderers.Length; i++)
                {
                    if (remoteRenderers[i] != null)
                    {
                        remoteRenderers[i].sharedMaterial = targetMat;
                    }
                }
            }

            if (remoteAuraLight != null)
            {
                remoteAuraLight.color = isPartner1929
                    ? new Color(0.96f, 0.80f, 0.46f)
                    : new Color(0.58f, 0.78f, 0.98f);
            }

            if (remoteSpotLight != null)
            {
                remoteSpotLight.color = isPartner1929
                    ? new Color(1.0f, 0.92f, 0.76f)
                    : new Color(0.88f, 0.94f, 1.0f);
            }

            if (remoteCloseFillLight != null && remoteSpotLight != null)
            {
                remoteCloseFillLight.color = remoteSpotLight.color;
            }
        }

        private void ApplyRemoteFlashlightState()
        {
            if (remoteSpotLight != null)
            {
                remoteSpotLight.enabled = flashlightActive;
            }
            if (remoteCloseFillLight != null)
            {
                remoteCloseFillLight.enabled = flashlightActive;
            }
        }

        /// <summary>
        /// Actualización manual para el compañero simulado con [F2] en pruebas de un solo equipo.
        /// </summary>
        public void SetSimulatedFlashlightAndPitch(bool isLit, float pitch)
        {
            flashlightActive = isLit;
            syncedCameraPitch = pitch;
            ApplyRemoteFlashlightState();
        }
    }
}
