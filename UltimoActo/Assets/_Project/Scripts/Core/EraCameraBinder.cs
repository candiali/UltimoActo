using UnityEngine;

namespace UltimoActo.Core
{
    /// <summary>
    /// Componente por cámara de jugador para renderizar la época asignada (1929 o Ruinas)
    /// mediante Culling Mask y cielo atmosférico acorde a la imagen de referencia.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class EraCameraBinder : MonoBehaviour
    {
        [SerializeField] private bool followLocalManager = true;
        [SerializeField] private EraVisual assignedEra = EraVisual.Glory1929;

        private Camera cam;

        public EraVisual AssignedEra => followLocalManager && EraVisualManager.Instance != null
            ? EraVisualManager.Instance.CurrentEra
            : assignedEra;

        public bool FollowLocalManager
        {
            get => followLocalManager;
            set => followLocalManager = value;
        }

        private void Awake()
        {
            cam = GetComponent<Camera>();
            EraVisualManager.RegisterCamera(this);
            RefreshCameraMask();
        }

        private void OnEnable()
        {
            if (cam == null) cam = GetComponent<Camera>();
            EraVisualManager.RegisterCamera(this);
            RefreshCameraMask();
        }

        private void OnDestroy()
        {
            EraVisualManager.UnregisterCamera(this);
        }

        public void SetCameraEra(EraVisual era, bool detachFromGlobalToggle = false)
        {
            assignedEra = era;
            if (detachFromGlobalToggle) followLocalManager = false;
            RefreshCameraMask();
        }

        public void RefreshCameraMask()
        {
            if (cam == null) cam = GetComponent<Camera>();
            if (cam == null) return;

            int layer1929 = EraVisualManager.GetEra1929Layer();
            int layerRuins = EraVisualManager.GetEraRuinsLayer();
            int bothEraMask = (1 << layer1929) | (1 << layerRuins);

            int commonMask = ~bothEraMask;
            int activeEraBit = AssignedEra == EraVisual.Glory1929 ? (1 << layer1929) : (1 << layerRuins);
            cam.cullingMask = commonMask | activeEraBit;

            // Cielo crepuscular rosa-lavanda luminoso en 1929 vs cielo tormentoso frío #8FA0B8 en Ruinas (como en la referencia)
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = AssignedEra == EraVisual.Glory1929
                ? new Color(0.56f, 0.47f, 0.54f, 1f)  // Cielo crepuscular rosa-lavanda de la referencia 1929
                : new Color(0.28f, 0.33f, 0.39f, 1f); // Cielo nublado frío #8FA0B8 de la referencia Ruinas
        }
    }
}
