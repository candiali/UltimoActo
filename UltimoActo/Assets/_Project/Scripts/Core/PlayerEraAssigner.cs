using System;
using UnityEngine;

namespace UltimoActo.Core
{
    /// <summary>
    /// Componente asignable a cada prefab/instancia de Jugador para la integración de red (2 jugadores):
    /// Jugador A (Host) = EraVisual.Glory1929, Jugador B (Client) = EraVisual.Ruins.
    /// </summary>
    public class PlayerEraAssigner : MonoBehaviour
    {
        [Header("Época Asignada a este Jugador (Red / Cooperativo)")]
        [SerializeField] private EraVisual era = EraVisual.Glory1929;
        [SerializeField] private bool isLocalPlayer = true;
        [SerializeField] private bool autoAssignFromNetworkRolePref = true;

        public EraVisual Era
        {
            get => era;
            set => SetEra(value);
        }

        private void Start()
        {
            if (autoAssignFromNetworkRolePref && PlayerPrefs.HasKey("NetworkRole"))
            {
                string role = PlayerPrefs.GetString("NetworkRole", "Host");
                era = string.Equals(role, "Client", StringComparison.OrdinalIgnoreCase)
                    ? EraVisual.Ruins
                    : EraVisual.Glory1929;
            }
            SetEra(era);
        }

        public void SetEra(EraVisual targetEra)
        {
            era = targetEra;
            EraCameraBinder binder = GetComponentInChildren<EraCameraBinder>(true);
            if (binder != null)
            {
                binder.SetCameraEra(era, !isLocalPlayer);
            }
            if (isLocalPlayer && EraVisualManager.Instance != null)
            {
                EraVisualManager.Instance.SetEra(era, false);
            }
        }
    }
}
