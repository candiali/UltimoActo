using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UltimoActo.Core;

namespace UltimoActo.Interaction
{
    /// <summary>
    /// Controlador del Cuadro Eléctrico Principal de la Academia (Noche I).
    /// Requiere tres fusibles cerámicos distribuidos por la planta baja para activar
    /// la palanca de cuchilla, restaurar la corriente de las lámparas del vestíbulo
    /// y liberar el cierre electromagnético del Archivo de Expedientes.
    /// </summary>
    public class AcademyFuseBoxController : MonoBehaviour, IInteractable
    {
        public static AcademyFuseBoxController Instance { get; private set; }

        [Header("Identificación")]
        [SerializeField] private string id = "fuse_box_reception";

        [Header("Requisitos de Fusibles")]
        [SerializeField] private string[] requiredFuseIds = new string[]
        {
            "fuse_ceramic_10a", // Vestuario femenino
            "fuse_ceramic_15a", // Taquilla Nº 7 de Mara Bell
            "fuse_ceramic_25a"  // Despacho de Dirección
        };

        [SerializeField] private string[] requiredFuseNames = new string[]
        {
            "Fusible Cerámico 10A",
            "Fusible Cerámico 15A",
            "Fusible Cerámico 25A"
        };

        [SerializeField] private bool[] slotInstalled = new bool[] { false, false, false };

        [Header("Estado de la Red")]
        [SerializeField] private bool isPowerRestored = false;

        [Header("Componentes Mecánicos y Visuales")]
        [SerializeField] private Transform knifeSwitchLever;
        [SerializeField] private Renderer[] slotIndicators;
        [SerializeField] private Material redIndicatorMat;
        [SerializeField] private Material greenIndicatorMat;

        [Header("Efectos al Activar")]
        [SerializeField] private List<Light> lightsToPowerOn = new List<Light>();
        [SerializeField] private WorldDoor targetElectromagneticDoor;

        private AudioSource audioSource;
        private Coroutine powerUpRoutine;

        public string Id => id;
        public bool IsPowerRestored => isPowerRestored;
        public int InstalledFusesCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < slotInstalled.Length; i++) if (slotInstalled[i]) count++;
                return count;
            }
        }

        public string Prompt
        {
            get
            {
                if (isPowerRestored)
                {
                    return "Cuadro Eléctrico: Corriente restaurada (220V)";
                }

                if (AllFusesInstalled())
                {
                    return "[E] Bajar palanca de cuchilla para restaurar la corriente";
                }

                // Comprobar si el jugador tiene alguno de los fusibles necesarios
                int nextSlot = GetNextMissingFusePlayerHas();
                if (nextSlot >= 0)
                {
                    return $"[E] Instalar {requiredFuseNames[nextSlot]} en el cuadro";
                }

                return $"[E] Cuadro Eléctrico sin fusibles ({InstalledFusesCount}/3 instalados)";
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0.85f;
            }

            UpdateIndicators();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public bool CanInteract(GameObject player)
        {
            return !isPowerRestored && powerUpRoutine == null;
        }

        public void Interact(GameObject player)
        {
            if (isPowerRestored || powerUpRoutine != null) return;

            var inventory = player.GetComponent<SimpleInventory>();
            if (inventory == null) return;

            if (AllFusesInstalled())
            {
                // Bajar la palanca y restaurar la corriente
                powerUpRoutine = StartCoroutine(RestorePowerSequence());
                return;
            }

            // Intentar colocar un fusible que tenga el jugador
            int slotToFill = GetNextMissingFusePlayerHas(inventory);
            if (slotToFill >= 0)
            {
                string fuseId = requiredFuseIds[slotToFill];
                inventory.RemoveKey(fuseId);
                slotInstalled[slotToFill] = true;
                UpdateIndicators();
                PlayPorcelainSnapSound();

                int total = InstalledFusesCount;
                if (NightManager.Instance != null)
                {
                    if (total >= 3)
                    {
                        NightManager.Instance.ShowToast("¡Tres fusibles colocados! Acciona la palanca de cuchilla.", 4.0f);
                    }
                    else
                    {
                        NightManager.Instance.ShowToast($"Fusible instalado correctamente ({total}/3).", 3.0f);
                    }
                }
            }
            else
            {
                // Mensaje informativo
                if (NightManager.Instance != null)
                {
                    NightManager.Instance.ShowToast($"Faltan fusibles cerámicos ({InstalledFusesCount}/3). Busca en Vestuarios, Taquillas y Administración.", 4.5f);
                }
            }
        }

        public bool AllFusesInstalled()
        {
            for (int i = 0; i < slotInstalled.Length; i++)
            {
                if (!slotInstalled[i]) return false;
            }
            return true;
        }

        private int GetNextMissingFusePlayerHas()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return -1;
            var inventory = player.GetComponent<SimpleInventory>();
            return GetNextMissingFusePlayerHas(inventory);
        }

        private int GetNextMissingFusePlayerHas(SimpleInventory inventory)
        {
            if (inventory == null) return -1;
            for (int i = 0; i < requiredFuseIds.Length; i++)
            {
                if (!slotInstalled[i] && inventory.HasKey(requiredFuseIds[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        private void UpdateIndicators()
        {
            if (slotIndicators == null) return;
            for (int i = 0; i < slotIndicators.Length; i++)
            {
                if (slotIndicators[i] != null)
                {
                    Material mat = slotInstalled[i] ? greenIndicatorMat : redIndicatorMat;
                    if (mat != null) slotIndicators[i].sharedMaterial = mat;
                }
            }
        }

        private IEnumerator RestorePowerSequence()
        {
            // 1. Sonido y animación mecánica de la palanca de cuchilla
            PlayKnifeSwitchClackSound();
            if (knifeSwitchLever != null)
            {
                float t = 0f;
                Quaternion startRot = knifeSwitchLever.localRotation;
                Quaternion targetRot = startRot * Quaternion.Euler(55f, 0f, 0f);
                while (t < 0.25f)
                {
                    t += Time.deltaTime;
                    knifeSwitchLever.localRotation = Quaternion.Slerp(startRot, targetRot, t / 0.25f);
                    yield return null;
                }
                knifeSwitchLever.localRotation = targetRot;
            }

            yield return new WaitForSeconds(0.4f);

            // 2. Parpadeo inicial de la red eléctrica
            PlayTransformerHumSound();

            for (int flicker = 0; flicker < 4; flicker++)
            {
                SetLightsIntensity(0f);
                yield return new WaitForSeconds(0.08f);
                SetLightsIntensity(UnityEngine.Random.Range(0.4f, 1.2f));
                yield return new WaitForSeconds(0.06f);
            }

            // 3. Luces plenamente encendidas
            SetLightsIntensity(1.0f);
            isPowerRestored = true;

            // 4. Liberar cerrojo electromagnético del Archivo de Expedientes
            if (targetElectromagneticDoor != null)
            {
                targetElectromagneticDoor.UnlockDoor();
            }

            // 5. Registrar evidencia en NightManager
            if (NightManager.Instance != null)
            {
                NightManager.Instance.RegisterEvidence("evidencia_energia_restaurada_noche1");
                NightManager.Instance.ShowToast("ENERGÍA RESTAURADA — El relé electromagnético del Archivo se ha desbloqueado.", 6.0f);
            }

            powerUpRoutine = null;
        }

        private void SetLightsIntensity(float multiplier)
        {
            if (lightsToPowerOn == null) return;
            for (int i = 0; i < lightsToPowerOn.Count; i++)
            {
                if (lightsToPowerOn[i] != null)
                {
                    lightsToPowerOn[i].enabled = multiplier > 0.05f;
                    lightsToPowerOn[i].intensity = Mathf.Max(0.5f, lightsToPowerOn[i].intensity) * multiplier;
                }
            }
        }

        public void Configure(
            Transform lever, Renderer[] indicators, Material redMat, Material greenMat,
            List<Light> targetLights, WorldDoor archiveDoor)
        {
            knifeSwitchLever = lever;
            slotIndicators = indicators;
            redIndicatorMat = redMat;
            greenIndicatorMat = greenMat;
            lightsToPowerOn = targetLights ?? new List<Light>();
            targetElectromagneticDoor = archiveDoor;
        }

        // ====================================================================
        // AUDIO PROCEDURAL
        // ====================================================================
        private void PlayPorcelainSnapSound()
        {
            if (audioSource == null) return;
            audioSource.pitch = UnityEngine.Random.Range(1.1f, 1.3f);
            audioSource.PlayOneShot(GenerateSnapClip());
        }

        private void PlayKnifeSwitchClackSound()
        {
            if (audioSource == null) return;
            audioSource.pitch = 0.9f;
            audioSource.PlayOneShot(GenerateClackClip());
        }

        private void PlayTransformerHumSound()
        {
            if (audioSource == null) return;
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(GenerateHumClip());
        }

        private static AudioClip cachedSnapClip;
        private static AudioClip GenerateSnapClip()
        {
            if (cachedSnapClip != null) return cachedSnapClip;
            int samples = 3000;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                data[i] = Mathf.Sin(2f * Mathf.PI * 2600f * t) * Mathf.Exp(-t * 40f);
            }
            cachedSnapClip = AudioClip.Create("PorcelainSnap", samples, 1, 44100, false);
            cachedSnapClip.SetData(data, 0);
            return cachedSnapClip;
        }

        private static AudioClip cachedClackClip;
        private static AudioClip GenerateClackClip()
        {
            if (cachedClackClip != null) return cachedClackClip;
            int samples = 5000;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                data[i] = Mathf.Sin(2f * Mathf.PI * 180f * t) * Mathf.Exp(-t * 22f);
            }
            cachedClackClip = AudioClip.Create("KnifeSwitchClack", samples, 1, 44100, false);
            cachedClackClip.SetData(data, 0);
            return cachedClackClip;
        }

        private static AudioClip cachedHumClip;
        private static AudioClip GenerateHumClip()
        {
            if (cachedHumClip != null) return cachedHumClip;
            int samples = 14000;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                data[i] = (Mathf.Sin(2f * Mathf.PI * 60f * t) * 0.6f + Mathf.Sin(2f * Mathf.PI * 120f * t) * 0.4f) * Mathf.Exp(-t * 3.5f);
            }
            cachedHumClip = AudioClip.Create("TransformerHum", samples, 1, 44100, false);
            cachedHumClip.SetData(data, 0);
            return cachedHumClip;
        }
    }
}
