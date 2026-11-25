using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
#if ULTIMOACTO_NETCODE
using Unity.Collections;
using Unity.Netcode;
#endif
using UltimoActo.Core;

namespace UltimoActo.Network
{
    /// <summary>
    /// Paso 5 — Chat de Voz en LAN (Implementación propia dentro de Netcode, sin Vivox ni Photon):
    /// - Captura con Microphone.Start (mono, 16 kHz) en bloques de 20 ms (320 muestras).
    /// - Compresión PCM 16-bit firmada con reducción a 16 kHz (640 bytes por paquete de 20 ms).
    /// - Envío por mensajes personalizados de Netcode (CustomMessagingManager) en modo no fiable / sin orden
    ///   (NetworkDelivery.Unreliable), con jitter buffer circular pequeño en el receptor.
    /// - Reproducción continua en AudioSource con AudioClip en streaming.
    /// - Opciones completas: Pulsar para hablar [V] y micrófono abierto, selección de micrófono,
    ///   volumen de entrada y salida, silenciar al otro jugador.
    /// - Indicadores: icono discreto de "Hablando" para cada jugador y aviso si no hay micrófono
    ///   o si Windows deniega el permiso.
    /// - Audio espacial / efecto sutil por época opcional (eco/resonancia lejana en Ruinas, desactivable;
    ///   por defecto la voz se oye clara en 2D para comunicación perfecta).
    /// - Supresión básica de eco (puerta de ruido + atenuación adaptativa cuando el otro jugador habla)
    ///   y recomendación de usar auriculares en el menú.
    /// </summary>
    public class LanVoiceChatManager : MonoBehaviour
    {
        public static LanVoiceChatManager Instance { get; private set; }

        public const string MsgVoiceFrame = "UltimoActo_LanVoice_PCM16";
        public const int SampleRate = 16000;
        public const int FrameSamples = 320; // 20 ms a 16 kHz
        public const int CaptureBufferSeconds = 2;
        public const int PlaybackRingSeconds = 4;
        public const int JitterBufferStartSamples = 960; // 60 ms de pre-buffer para absorber jitter de red

        [Header("Opciones de Voz")]
        [SerializeField] private bool pushToTalkMode = true; // true = Pulsar [V], false = Micrófono abierto
        [SerializeField] private int selectedMicIndex = 0;
        [Range(0f, 2f)]
        [SerializeField] private float inputGain = 1.15f;
        [Range(0f, 2f)]
        [SerializeField] private float outputVolume = 1.0f;
        [SerializeField] private bool muteRemotePlayer = false;
        [SerializeField] private bool experimentalEraEffect = false; // Por defecto false (comunicación clara)
        [SerializeField] private bool basicEchoSuppression = true;
        [SerializeField] private bool loopbackTestMode = false; // Permite probar el propio micrófono en un solo PC

        // Captura de micrófono
        private readonly List<string> availableMics = new List<string>();
        private string activeMicDeviceName;
        private AudioClip micCaptureClip;
        private int lastMicReadPos;
        private bool isMicCapturing;
        private bool hasMicrophoneHardware;
        private bool micPermissionDeniedOrFailed;
        private string micDiagnosticMessage = "";
        private readonly float[] captureFrameFloat = new float[FrameSamples];
        private readonly byte[] encodedPcm16Bytes = new byte[FrameSamples * 2];

        // Estado de habla ("Hablando")
        private bool isLocalSpeaking;
        private float localVoiceLevel;
        private float localSpeakingHoldTimer;

        private bool isRemoteSpeaking;
        private float remoteVoiceLevel;
        private float remoteSpeakingHoldTimer;

        // Reproducción y Jitter Buffer en el receptor
        private AudioSource playbackAudioSource;
        private AudioEchoFilter eraEchoFilter;
        private AudioLowPassFilter eraLowPassFilter;
        private AudioClip streamingPlaybackClip;
        private float[] jitterRingBuffer;
        private int jitterWriteIndex;
        private int jitterReadIndex;
        private int jitterBufferedSamples;
        private bool jitterPrimed;
        private ushort outgoingSequence;

        // Mensajería Netcode
        private bool voiceHandlerRegistered;

        // HUD discreto de indicadores de "Hablando" y avisos de micrófono
        private Canvas voiceHudCanvas;
        private GameObject localSpeakBadge;
        private Image localSpeakBorder;
        private TextMeshProUGUI localSpeakText;
        private RectTransform localLevelBar;

        private GameObject remoteSpeakBadge;
        private Image remoteSpeakBorder;
        private TextMeshProUGUI remoteSpeakText;
        private RectTransform remoteLevelBar;

        private GameObject micWarningBadge;
        private TextMeshProUGUI micWarningText;

        public bool IsPushToTalk => pushToTalkMode;
        public string ActiveMicName => string.IsNullOrEmpty(activeMicDeviceName) ? "Ninguno" : activeMicDeviceName;
        public int AvailableMicCount => availableMics.Count;
        public float InputGain => inputGain;
        public float OutputVolume => outputVolume;
        public bool IsRemoteMuted => muteRemotePlayer;
        public bool IsExperimentalEraEffectEnabled => experimentalEraEffect;
        public bool IsBasicEchoSuppressionEnabled => basicEchoSuppression;
        public bool IsLoopbackTestEnabled => loopbackTestMode;
        public bool IsLocalSpeaking => isLocalSpeaking;
        public bool IsRemoteSpeaking => isRemoteSpeaking && !muteRemotePlayer;
        public bool HasMicrophoneIssue => !hasMicrophoneHardware || micPermissionDeniedOrFailed;
        public string MicStatusDescription => micDiagnosticMessage;

        public event Action OnVoiceSettingsChanged;

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
            if (FindFirstObjectByType<LanVoiceChatManager>() != null) return;
            if (FindFirstObjectByType<UltimoActo.Player.PlayerMovement>() == null) return;

            GameObject go = new GameObject("LanVoiceChatManager");
            go.AddComponent<LanVoiceChatManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            jitterRingBuffer = new float[SampleRate * PlaybackRingSeconds];
            LoadSavedVoicePreferences();
        }

        private void Start()
        {
            SetupStreamingPlaybackAudioSource();
            BuildVoiceIndicatorsHUD();
            RefreshMicrophoneDeviceList();
            StartMicrophoneCapture();
            TryRegisterNetcodeVoiceHandler();
        }

        private void OnDestroy()
        {
            StopMicrophoneCapture();
            UnregisterNetcodeVoiceHandler();
            if (Instance == this) Instance = null;
        }

        private void LoadSavedVoicePreferences()
        {
            pushToTalkMode = PlayerPrefs.GetInt("UltimoActo_Voice_PTT", 1) == 1;
            inputGain = Mathf.Clamp(PlayerPrefs.GetFloat("UltimoActo_Voice_InGain", 1.15f), 0.2f, 2.0f);
            outputVolume = Mathf.Clamp(PlayerPrefs.GetFloat("UltimoActo_Voice_OutVol", 1.0f), 0.0f, 2.0f);
            muteRemotePlayer = PlayerPrefs.GetInt("UltimoActo_Voice_MuteRemote", 0) == 1;
            experimentalEraEffect = PlayerPrefs.GetInt("UltimoActo_Voice_EraFX", 0) == 1;
            basicEchoSuppression = PlayerPrefs.GetInt("UltimoActo_Voice_EchoSup", 1) == 1;
        }

        private void SaveVoicePreferences()
        {
            PlayerPrefs.SetInt("UltimoActo_Voice_PTT", pushToTalkMode ? 1 : 0);
            PlayerPrefs.SetFloat("UltimoActo_Voice_InGain", inputGain);
            PlayerPrefs.SetFloat("UltimoActo_Voice_OutVol", outputVolume);
            PlayerPrefs.SetInt("UltimoActo_Voice_MuteRemote", muteRemotePlayer ? 1 : 0);
            PlayerPrefs.SetInt("UltimoActo_Voice_EraFX", experimentalEraEffect ? 1 : 0);
            PlayerPrefs.SetInt("UltimoActo_Voice_EchoSup", basicEchoSuppression ? 1 : 0);
            PlayerPrefs.Save();
        }

        // ====================================================================
        // API DE OPCIONES DE VOZ (Para el Menú de Pausa [ESC])
        // ====================================================================
        public void TogglePushToTalkMode()
        {
            pushToTalkMode = !pushToTalkMode;
            SaveVoicePreferences();
            OnVoiceSettingsChanged?.Invoke();
        }

        public void CycleMicrophoneDevice()
        {
            RefreshMicrophoneDeviceList();
            if (availableMics.Count == 0)
            {
                OnVoiceSettingsChanged?.Invoke();
                return;
            }

            selectedMicIndex = (selectedMicIndex + 1) % availableMics.Count;
            StartMicrophoneCapture();
            OnVoiceSettingsChanged?.Invoke();
        }

        public void CycleInputGain()
        {
            // Cicla entre 60% -> 100% -> 130% -> 160% -> 200%
            float[] steps = { 0.60f, 1.00f, 1.30f, 1.60f, 2.00f };
            int nextIdx = 0;
            for (int i = 0; i < steps.Length; i++)
            {
                if (inputGain < steps[i] - 0.05f)
                {
                    nextIdx = i;
                    break;
                }
            }
            inputGain = steps[nextIdx];
            SaveVoicePreferences();
            OnVoiceSettingsChanged?.Invoke();
        }

        public void CycleOutputVolume()
        {
            // Cicla entre 40% -> 80% -> 100% -> 140% -> 180%
            float[] steps = { 0.40f, 0.80f, 1.00f, 1.40f, 1.80f };
            int nextIdx = 0;
            for (int i = 0; i < steps.Length; i++)
            {
                if (outputVolume < steps[i] - 0.05f)
                {
                    nextIdx = i;
                    break;
                }
            }
            outputVolume = steps[nextIdx];
            if (playbackAudioSource != null)
            {
                playbackAudioSource.volume = Mathf.Clamp01(outputVolume * 0.75f);
            }
            SaveVoicePreferences();
            OnVoiceSettingsChanged?.Invoke();
        }

        public void ToggleMuteRemotePlayer()
        {
            muteRemotePlayer = !muteRemotePlayer;
            if (muteRemotePlayer)
            {
                ClearJitterBuffer();
                isRemoteSpeaking = false;
            }
            SaveVoicePreferences();
            OnVoiceSettingsChanged?.Invoke();
        }

        public void ToggleExperimentalEraEffect()
        {
            experimentalEraEffect = !experimentalEraEffect;
            ApplyEraSpatialEffectSettings();
            SaveVoicePreferences();
            OnVoiceSettingsChanged?.Invoke();
        }

        public void ToggleLoopbackTestMode()
        {
            loopbackTestMode = !loopbackTestMode;
            if (!loopbackTestMode)
            {
                ClearJitterBuffer();
                isRemoteSpeaking = false;
            }
            OnVoiceSettingsChanged?.Invoke();
        }

        // ====================================================================
        // CAPTURA CON MICROPHONE.START (16 kHz, Bloques de 20 ms)
        // ====================================================================
        public void RefreshMicrophoneDeviceList()
        {
            availableMics.Clear();
            try
            {
                string[] devs = Microphone.devices;
                if (devs != null && devs.Length > 0)
                {
                    availableMics.AddRange(devs);
                    hasMicrophoneHardware = true;
                }
                else
                {
                    hasMicrophoneHardware = false;
                }
            }
            catch (Exception ex)
            {
                hasMicrophoneHardware = false;
                micPermissionDeniedOrFailed = true;
                micDiagnosticMessage = $"Permiso de micrófono bloqueado por Windows: {ex.Message}";
            }

            if (!hasMicrophoneHardware)
            {
                activeMicDeviceName = string.Empty;
                micDiagnosticMessage = "No se detectó ningún micrófono conectado o Windows denegó el acceso (Configuración > Privacidad > Micrófono).";
            }
        }

        public void StartMicrophoneCapture()
        {
            StopMicrophoneCapture();
            RefreshMicrophoneDeviceList();

            if (!hasMicrophoneHardware || availableMics.Count == 0)
            {
                UpdateVoiceHUD();
                return;
            }

            selectedMicIndex = Mathf.Clamp(selectedMicIndex, 0, availableMics.Count - 1);
            activeMicDeviceName = availableMics[selectedMicIndex];

            try
            {
                micCaptureClip = Microphone.Start(activeMicDeviceName, true, CaptureBufferSeconds, SampleRate);
                lastMicReadPos = 0;

                if (micCaptureClip == null)
                {
                    micPermissionDeniedOrFailed = true;
                    isMicCapturing = false;
                    micDiagnosticMessage = "Windows denegó el permiso del micrófono. Actívalo en Configuración de Windows > Privacidad y seguridad > Micrófono.";
                }
                else
                {
                    micPermissionDeniedOrFailed = false;
                    isMicCapturing = true;
                    micDiagnosticMessage = $"Micrófono activo ({SampleRate / 1000} kHz PCM 16-bit · Bloques 20 ms)";
                }
            }
            catch (Exception ex)
            {
                micPermissionDeniedOrFailed = true;
                isMicCapturing = false;
                micDiagnosticMessage = $"No se pudo abrir el micrófono '{activeMicDeviceName}': Verifica permisos en Windows ({ex.Message}).";
            }

            UpdateVoiceHUD();
        }

        public void StopMicrophoneCapture()
        {
            if (isMicCapturing && !string.IsNullOrEmpty(activeMicDeviceName))
            {
                try
                {
                    if (Microphone.IsRecording(activeMicDeviceName))
                    {
                        Microphone.End(activeMicDeviceName);
                    }
                }
                catch { }
            }
            isMicCapturing = false;
            micCaptureClip = null;
            lastMicReadPos = 0;
        }

        private void Update()
        {
            if (!voiceHandlerRegistered)
            {
                TryRegisterNetcodeVoiceHandler();
            }

            ProcessMicrophoneCaptureFrames();
            UpdateSpeakingTimers();
            UpdateDynamicSpatialFollow();
            UpdateVoiceHUD();
        }

        private void ProcessMicrophoneCaptureFrames()
        {
            if (!isMicCapturing || micCaptureClip == null || string.IsNullOrEmpty(activeMicDeviceName))
            {
                return;
            }

            int currentPos = 0;
            try
            {
                currentPos = Microphone.GetPosition(activeMicDeviceName);
            }
            catch
            {
                micPermissionDeniedOrFailed = true;
                isMicCapturing = false;
                micDiagnosticMessage = "Se perdió el acceso al micrófono. Revisa la conexión o permisos de Windows.";
                return;
            }

            if (currentPos < 0) return;

            int clipTotalSamples = micCaptureClip.samples;
            int available = currentPos - lastMicReadPos;
            if (available < 0)
            {
                available += clipTotalSamples;
            }

            // Si el buffer saltó demasiado (p.ej. pausa larga del SO), re-sincronizar
            if (available > SampleRate)
            {
                lastMicReadPos = (currentPos - FrameSamples + clipTotalSamples) % clipTotalSamples;
                available = FrameSamples;
            }

            bool wantsToTransmit = IsLocalVoiceTransmissionActive();

            while (available >= FrameSamples)
            {
                micCaptureClip.GetData(captureFrameFloat, lastMicReadPos);
                lastMicReadPos = (lastMicReadPos + FrameSamples) % clipTotalSamples;
                available -= FrameSamples;

                if (!wantsToTransmit)
                {
                    continue;
                }

                // Calcular energía RMS del bloque de 20 ms
                float sumSq = 0f;
                float effectiveGain = inputGain;

                // Cancelación/supresión básica de eco: si el compañero está hablando por nuestros altavoces,
                // elevamos el umbral de la puerta de ruido y atenuamos ligeramente la entrada
                if (basicEchoSuppression && isRemoteSpeaking && !loopbackTestMode)
                {
                    effectiveGain *= 0.65f;
                }

                for (int i = 0; i < FrameSamples; i++)
                {
                    float s = Mathf.Clamp(captureFrameFloat[i] * effectiveGain, -1f, 1f);
                    captureFrameFloat[i] = s;
                    sumSq += s * s;
                }

                float rms = Mathf.Sqrt(sumSq / FrameSamples);
                float gateThreshold = (!pushToTalkMode)
                    ? (basicEchoSuppression && isRemoteSpeaking ? 0.024f : 0.012f)
                    : 0.004f;

                if (rms >= gateThreshold || pushToTalkMode)
                {
                    if (rms >= 0.005f || pushToTalkMode)
                    {
                        localVoiceLevel = Mathf.Clamp01(rms * 8.5f);
                        isLocalSpeaking = true;
                        localSpeakingHoldTimer = 0.22f;
                    }

                    // Compresión PCM 16-bit firmada (16 kHz mono, 640 bytes por paquete de 20 ms)
                    EncodePcm16(captureFrameFloat, encodedPcm16Bytes);
                    TransmitVoiceFrame(encodedPcm16Bytes);
                }
            }
        }

        private bool IsLocalVoiceTransmissionActive()
        {
            if (!pushToTalkMode)
            {
                // Micrófono abierto
                return true;
            }

            // Pulsar para hablar con [V]
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.vKey.isPressed)
            {
                return true;
            }
#else
            if (Input.GetKey(KeyCode.V))
            {
                return true;
            }
#endif
            return false;
        }

        private static void EncodePcm16(float[] samples, byte[] outBytes)
        {
            int idx = 0;
            for (int i = 0; i < FrameSamples; i++)
            {
                short val = (short)Mathf.RoundToInt(Mathf.Clamp(samples[i], -1f, 1f) * 32767f);
                outBytes[idx++] = (byte)(val & 0xFF);
                outBytes[idx++] = (byte)((val >> 8) & 0xFF);
            }
        }

        private static void DecodePcm16ToJitterBuffer(
            byte[] inBytes,
            int byteOffset,
            int sampleCount,
            float volumeScale,
            float[] ringBuffer,
            ref int writeIdx,
            ref int bufferedCount,
            ref int readIdx,
            out float rmsOut)
        {
            int ringLen = ringBuffer.Length;
            float sumSq = 0f;
            int b = byteOffset;

            for (int i = 0; i < sampleCount; i++)
            {
                short val = (short)(inBytes[b] | (inBytes[b + 1] << 8));
                b += 2;
                float s = Mathf.Clamp((val / 32767f) * volumeScale, -1f, 1f);
                sumSq += s * s;

                ringBuffer[writeIdx] = s;
                writeIdx = (writeIdx + 1) % ringLen;

                if (bufferedCount < ringLen)
                {
                    bufferedCount++;
                }
                else
                {
                    // Si el buffer circular se llena, avanzar el puntero de lectura para mantener baja latencia
                    readIdx = (readIdx + 1) % ringLen;
                }
            }

            rmsOut = sampleCount > 0 ? Mathf.Sqrt(sumSq / sampleCount) : 0f;
        }

        // ====================================================================
        // ENVÍO Y RECEPCIÓN POR NETCODE (CustomMessagingManager — Unreliable)
        // ====================================================================
        private void TryRegisterNetcodeVoiceHandler()
        {
#if ULTIMOACTO_NETCODE
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening) return;
            if (NetworkManager.Singleton.CustomMessagingManager == null) return;

            var cmm = NetworkManager.Singleton.CustomMessagingManager;
            cmm.UnregisterNamedMessageHandler(MsgVoiceFrame);
            cmm.RegisterNamedMessageHandler(MsgVoiceFrame, OnNetcodeVoiceFrameReceived);
            voiceHandlerRegistered = true;
#endif
        }

        private void UnregisterNetcodeVoiceHandler()
        {
#if ULTIMOACTO_NETCODE
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.CustomMessagingManager != null)
            {
                NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(MsgVoiceFrame);
            }
#endif
            voiceHandlerRegistered = false;
        }

        private void TransmitVoiceFrame(byte[] pcm16Data)
        {
            outgoingSequence++;

            // Si está activo el modo de prueba local (Loopback), enviamos también a nuestro propio jitter buffer
            if (loopbackTestMode)
            {
                ReceiveEncodedVoiceFrame(pcm16Data, FrameSamples);
            }

#if ULTIMOACTO_NETCODE
            var nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsListening || nm.CustomMessagingManager == null) return;

            int payloadLen = pcm16Data.Length;
            if (nm.IsServer || nm.IsHost)
            {
                foreach (ulong clientId in nm.ConnectedClientsIds)
                {
                    if (clientId == nm.LocalClientId) continue;
                    using (var writer = new FastBufferWriter(payloadLen + 16, Allocator.Temp))
                    {
                        writer.WriteValueSafe(outgoingSequence);
                        writer.WriteValueSafe((ushort)FrameSamples);
                        writer.WriteBytesSafe(pcm16Data, payloadLen);
                        nm.CustomMessagingManager.SendNamedMessage(
                            MsgVoiceFrame,
                            clientId,
                            writer,
                            NetworkDelivery.Unreliable);
                    }
                }
            }
            else if (nm.IsClient)
            {
                using (var writer = new FastBufferWriter(payloadLen + 16, Allocator.Temp))
                {
                    writer.WriteValueSafe(outgoingSequence);
                    writer.WriteValueSafe((ushort)FrameSamples);
                    writer.WriteBytesSafe(pcm16Data, payloadLen);
                    nm.CustomMessagingManager.SendNamedMessage(
                        MsgVoiceFrame,
                        NetworkManager.ServerClientId,
                        writer,
                        NetworkDelivery.Unreliable);
                }
            }
#endif
        }

#if ULTIMOACTO_NETCODE
        private void OnNetcodeVoiceFrameReceived(ulong senderClientId, FastBufferReader reader)
        {
            if (NetworkManager.Singleton != null && senderClientId == NetworkManager.Singleton.LocalClientId)
            {
                return;
            }

            try
            {
                reader.ReadValueSafe(out ushort seq);
                reader.ReadValueSafe(out ushort sampleCount);
                if (sampleCount == 0 || sampleCount > 1024) return;

                int byteLen = sampleCount * 2;
                byte[] buffer = new byte[byteLen];
                reader.ReadBytesSafe(ref buffer, byteLen);

                ReceiveEncodedVoiceFrame(buffer, sampleCount);
            }
            catch { }
        }
#endif

        private void ReceiveEncodedVoiceFrame(byte[] pcm16Bytes, int sampleCount)
        {
            if (muteRemotePlayer) return;

            DecodePcm16ToJitterBuffer(
                pcm16Bytes,
                0,
                sampleCount,
                outputVolume,
                jitterRingBuffer,
                ref jitterWriteIndex,
                ref jitterBufferedSamples,
                ref jitterReadIndex,
                out float rms);

            if (jitterBufferedSamples >= JitterBufferStartSamples)
            {
                jitterPrimed = true;
            }

            // Si el buffer acumula más de 220 ms por un tirón de red, recortamos al valor óptimo para evitar retraso
            int maxAllowedLatencySamples = SampleRate / 4; // 250 ms
            if (jitterBufferedSamples > maxAllowedLatencySamples)
            {
                int drop = jitterBufferedSamples - JitterBufferStartSamples;
                jitterReadIndex = (jitterReadIndex + drop) % jitterRingBuffer.Length;
                jitterBufferedSamples = JitterBufferStartSamples;
            }

            remoteVoiceLevel = Mathf.Clamp01(rms * 8.5f);
            isRemoteSpeaking = true;
            remoteSpeakingHoldTimer = 0.26f;
        }

        // ====================================================================
        // REPRODUCCIÓN EN AUDIOSOURCE CON AUDIOCLIP EN STREAMING + EFECTO POR ÉPOCA
        // ====================================================================
        private void SetupStreamingPlaybackAudioSource()
        {
            GameObject speakerObj = new GameObject("RemoteVoice_StreamingSpeaker");
            speakerObj.transform.SetParent(transform, false);

            playbackAudioSource = speakerObj.AddComponent<AudioSource>();
            playbackAudioSource.playOnAwake = false;
            playbackAudioSource.loop = true;
            playbackAudioSource.spatialBlend = 0f; // Comunicación clara 2D por defecto
            playbackAudioSource.volume = Mathf.Clamp01(outputVolume * 0.85f);

            // Filtros para la opción experimental de efecto sutil por época (eco y lejanía en las Ruinas)
            eraLowPassFilter = speakerObj.AddComponent<AudioLowPassFilter>();
            eraLowPassFilter.cutoffFrequency = 5200f;
            eraLowPassFilter.enabled = false;

            eraEchoFilter = speakerObj.AddComponent<AudioEchoFilter>();
            eraEchoFilter.delay = 115f;
            eraEchoFilter.decayRatio = 0.28f;
            eraEchoFilter.wetMix = 0.24f;
            eraEchoFilter.dryMix = 0.88f;
            eraEchoFilter.enabled = false;

            ApplyEraSpatialEffectSettings();

            streamingPlaybackClip = AudioClip.Create(
                "LanVoice_JitterStream",
                SampleRate * PlaybackRingSeconds,
                1,
                SampleRate,
                true,
                OnStreamingAudioRead);

            playbackAudioSource.clip = streamingPlaybackClip;
            playbackAudioSource.Play();
        }

        private void OnStreamingAudioRead(float[] data)
        {
            int count = data.Length;
            if (muteRemotePlayer || !jitterPrimed || jitterBufferedSamples <= 0)
            {
                Array.Clear(data, 0, count);
                if (jitterBufferedSamples <= 0)
                {
                    jitterPrimed = false;
                }
                return;
            }

            int ringLen = jitterRingBuffer.Length;
            for (int i = 0; i < count; i++)
            {
                if (jitterBufferedSamples > 0)
                {
                    data[i] = jitterRingBuffer[jitterReadIndex];
                    jitterReadIndex = (jitterReadIndex + 1) % ringLen;
                    jitterBufferedSamples--;
                }
                else
                {
                    data[i] = 0f;
                    jitterPrimed = false;
                }
            }
        }

        private void ClearJitterBuffer()
        {
            jitterBufferedSamples = 0;
            jitterReadIndex = 0;
            jitterWriteIndex = 0;
            jitterPrimed = false;
        }

        private void ApplyEraSpatialEffectSettings()
        {
            if (playbackAudioSource == null) return;

            if (!experimentalEraEffect)
            {
                // Comunicación clara normal por defecto
                playbackAudioSource.spatialBlend = 0f;
                if (eraEchoFilter != null) eraEchoFilter.enabled = false;
                if (eraLowPassFilter != null) eraLowPassFilter.enabled = false;
            }
            else
            {
                // Efecto experimental por época: eco cavernoso sutil en Ruinas y espacialización suave
                bool inRuins = EraVisualManager.Instance != null && EraVisualManager.Instance.CurrentEra == EraVisual.Ruins;
                playbackAudioSource.spatialBlend = 0.35f;
                playbackAudioSource.minDistance = 4f;
                playbackAudioSource.maxDistance = 45f;

                if (eraEchoFilter != null)
                {
                    eraEchoFilter.enabled = true;
                    eraEchoFilter.delay = inRuins ? 135f : 75f;
                    eraEchoFilter.decayRatio = inRuins ? 0.32f : 0.18f;
                    eraEchoFilter.wetMix = inRuins ? 0.28f : 0.14f;
                }
                if (eraLowPassFilter != null)
                {
                    eraLowPassFilter.enabled = inRuins;
                    eraLowPassFilter.cutoffFrequency = 4600f;
                }
            }
        }

        private void UpdateDynamicSpatialFollow()
        {
            if (playbackAudioSource == null) return;

            if (experimentalEraEffect)
            {
                ApplyEraSpatialEffectSettings();
                if (NetworkCoopPlayer.RemotePlayer != null)
                {
                    playbackAudioSource.transform.position = NetworkCoopPlayer.RemotePlayer.transform.position + Vector3.up * 1.6f;
                }
            }
        }

        private void UpdateSpeakingTimers()
        {
            if (localSpeakingHoldTimer > 0f)
            {
                localSpeakingHoldTimer -= Time.unscaledDeltaTime;
                if (localSpeakingHoldTimer <= 0f)
                {
                    isLocalSpeaking = false;
                    localVoiceLevel = 0f;
                }
            }

            if (remoteSpeakingHoldTimer > 0f)
            {
                remoteSpeakingHoldTimer -= Time.unscaledDeltaTime;
                if (remoteSpeakingHoldTimer <= 0f)
                {
                    isRemoteSpeaking = false;
                    remoteVoiceLevel = 0f;
                }
            }
        }

        // ====================================================================
        // HUD DISCRETO DE INDICADORES "HABLANDO" Y AVISO DE MICRÓFONO
        // ====================================================================
        private void BuildVoiceIndicatorsHUD()
        {
            GameObject canvasObj = new GameObject("LanVoiceChat_HUD_Canvas");
            canvasObj.transform.SetParent(transform, false);
            voiceHudCanvas = canvasObj.AddComponent<Canvas>();
            voiceHudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            voiceHudCanvas.sortingOrder = 66;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            // 1. Indicador de Jugador Local Hablando (abajo a la izquierda, encima de la barra de época)
            localSpeakBadge = CreateSpeakingBadge(
                canvasObj.transform,
                "LocalPlayer_SpeakingBadge",
                new Vector2(205f, 68f),
                out localSpeakBorder,
                out localSpeakText,
                out localLevelBar);

            // 2. Indicador del Compañero Hablando (justo a la derecha del indicador local)
            remoteSpeakBadge = CreateSpeakingBadge(
                canvasObj.transform,
                "RemotePlayer_SpeakingBadge",
                new Vector2(475f, 68f),
                out remoteSpeakBorder,
                out remoteSpeakText,
                out remoteLevelBar);

            // 3. Aviso discreto si no hay micrófono o si Windows deniega el permiso
            micWarningBadge = new GameObject("MicHardwareOrPermission_Warning");
            micWarningBadge.transform.SetParent(canvasObj.transform, false);
            RectTransform warnRt = micWarningBadge.AddComponent<RectTransform>();
            warnRt.anchorMin = new Vector2(0f, 0f);
            warnRt.anchorMax = new Vector2(0f, 0f);
            warnRt.pivot = new Vector2(0f, 0f);
            warnRt.anchoredPosition = new Vector2(205f, 68f);
            warnRt.sizeDelta = new Vector2(540f, 32f);

            Image warnBorder = micWarningBadge.AddComponent<Image>();
            warnBorder.color = new Color(0.85f, 0.42f, 0.38f, 0.85f);
            warnBorder.raycastTarget = false;

            GameObject warnInner = new GameObject("Inner");
            warnInner.transform.SetParent(micWarningBadge.transform, false);
            RectTransform warnInRt = warnInner.AddComponent<RectTransform>();
            warnInRt.anchorMin = Vector2.zero;
            warnInRt.anchorMax = Vector2.one;
            warnInRt.offsetMin = new Vector2(1.5f, 1.5f);
            warnInRt.offsetMax = new Vector2(-1.5f, -1.5f);

            Image warnBg = warnInner.AddComponent<Image>();
            warnBg.color = new Color(0.14f, 0.05f, 0.06f, 0.92f);
            warnBg.raycastTarget = false;

            GameObject warnTxtObj = new GameObject("Text");
            warnTxtObj.transform.SetParent(warnInner.transform, false);
            RectTransform warnTxtRt = warnTxtObj.AddComponent<RectTransform>();
            warnTxtRt.anchorMin = Vector2.zero;
            warnTxtRt.anchorMax = Vector2.one;
            warnTxtRt.offsetMin = new Vector2(10f, 2f);
            warnTxtRt.offsetMax = new Vector2(-10f, -2f);

            micWarningText = warnTxtObj.AddComponent<TextMeshProUGUI>();
            micWarningText.fontSize = 12f;
            micWarningText.color = new Color(0.96f, 0.78f, 0.74f, 1f);
            micWarningText.alignment = TextAlignmentOptions.MidlineLeft;
            micWarningText.textWrappingMode = TextWrappingModes.NoWrap;
            micWarningText.overflowMode = TextOverflowModes.Ellipsis;
            micWarningText.raycastTarget = false;

            localSpeakBadge.SetActive(false);
            remoteSpeakBadge.SetActive(false);
            micWarningBadge.SetActive(false);
        }

        private static GameObject CreateSpeakingBadge(
            Transform parent,
            string name,
            Vector2 anchoredPos,
            out Image borderOut,
            out TextMeshProUGUI labelOut,
            out RectTransform levelFillOut)
        {
            GameObject badge = new GameObject(name);
            badge.transform.SetParent(parent, false);
            RectTransform rt = badge.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(260f, 34f);

            borderOut = badge.AddComponent<Image>();
            borderOut.color = new Color(0.85f, 0.71f, 0.41f, 0.85f);
            borderOut.raycastTarget = false;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(badge.transform, false);
            RectTransform fillRt = fill.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(1.5f, 1.5f);
            fillRt.offsetMax = new Vector2(-1.5f, -1.5f);

            Image bg = fill.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.04f, 0.06f, 0.90f);
            bg.raycastTarget = false;

            // Barra inferior de nivel de voz
            GameObject barObj = new GameObject("VoiceLevelBar");
            barObj.transform.SetParent(fill.transform, false);
            levelFillOut = barObj.AddComponent<RectTransform>();
            levelFillOut.anchorMin = new Vector2(0f, 0f);
            levelFillOut.anchorMax = new Vector2(0.5f, 0.14f);
            levelFillOut.offsetMin = Vector2.zero;
            levelFillOut.offsetMax = Vector2.zero;

            Image barImg = barObj.AddComponent<Image>();
            barImg.color = new Color(0.45f, 0.88f, 0.58f, 0.90f);
            barImg.raycastTarget = false;

            GameObject txtObj = new GameObject("Label");
            txtObj.transform.SetParent(fill.transform, false);
            RectTransform txtRt = txtObj.AddComponent<RectTransform>();
            txtRt.anchorMin = new Vector2(0f, 0.14f);
            txtRt.anchorMax = new Vector2(1f, 1f);
            txtRt.offsetMin = new Vector2(10f, 0f);
            txtRt.offsetMax = new Vector2(-10f, 0f);

            labelOut = txtObj.AddComponent<TextMeshProUGUI>();
            labelOut.fontSize = 12.5f;
            labelOut.fontStyle = FontStyles.Bold;
            labelOut.color = new Color(0.95f, 0.93f, 0.88f, 1f);
            labelOut.alignment = TextAlignmentOptions.MidlineLeft;
            labelOut.textWrappingMode = TextWrappingModes.NoWrap;
            labelOut.overflowMode = TextOverflowModes.Ellipsis;
            labelOut.raycastTarget = false;

            return badge;
        }

        private void UpdateVoiceHUD()
        {
            if (micWarningBadge != null)
            {
                bool showWarning = HasMicrophoneIssue;
                if (micWarningBadge.activeSelf != showWarning)
                {
                    micWarningBadge.SetActive(showWarning);
                }
                if (showWarning && micWarningText != null)
                {
                    micWarningText.text = $"AVISO DE VOZ: {micDiagnosticMessage}";
                }
            }

            if (localSpeakBadge != null)
            {
                bool showLocal = !HasMicrophoneIssue && isLocalSpeaking;
                if (localSpeakBadge.activeSelf != showLocal)
                {
                    localSpeakBadge.SetActive(showLocal);
                }
                if (showLocal)
                {
                    string modeTag = pushToTalkMode ? "[V]" : "[MIC ABIERTO]";
                    localSpeakText.text = $"● TÚ HABLANDO {modeTag}";
                    if (localLevelBar != null)
                    {
                        localLevelBar.anchorMax = new Vector2(Mathf.Clamp(localVoiceLevel, 0.1f, 1f), 0.14f);
                    }
                }
            }

            if (remoteSpeakBadge != null)
            {
                bool showRemote = isRemoteSpeaking && !muteRemotePlayer;
                if (remoteSpeakBadge.activeSelf != showRemote)
                {
                    remoteSpeakBadge.SetActive(showRemote);
                }
                if (showRemote)
                {
                    string partnerLabel = loopbackTestMode
                        ? "PRUEBA DE MICRO"
                        : (NetworkCoopPlayer.RemotePlayer != null ? NetworkCoopPlayer.RemotePlayer.DisplayName.ToUpperInvariant() : "COMPAÑERO");
                    string fxTag = experimentalEraEffect ? " · [ECO ÉPOCA]" : "";
                    remoteSpeakText.text = $"● {partnerLabel} HABLANDO{fxTag}";
                    if (remoteLevelBar != null)
                    {
                        remoteLevelBar.anchorMax = new Vector2(Mathf.Clamp(remoteVoiceLevel, 0.1f, 1f), 0.14f);
                    }
                }
            }
        }
    }
}
