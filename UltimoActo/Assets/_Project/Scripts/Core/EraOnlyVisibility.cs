using UnityEngine;

namespace UltimoActo.Core
{
    /// <summary>
    /// Las dos épocas visuales simultáneas de la Academia Vesper.
    /// </summary>
    public enum EraVisual
    {
        Glory1929 = 0,
        Ruins = 1
    }

    /// <summary>
    /// Modo de visibilidad por época para pistas de puzles y objetos del cooperativo asimétrico.
    /// </summary>
    public enum EraVisibilityMode
    {
        Both = 0,
        Only1929 = 1,
        OnlyRuins = 2
    }

    /// <summary>
    /// Hook para objetos o pistas visuales que existen en ambas épocas o solo en una de ellas.
    /// No altera colliders ni lógica de puzles.
    /// </summary>
    public class EraOnlyVisibility : MonoBehaviour
    {
        [SerializeField] private EraVisibilityMode visibility = EraVisibilityMode.Both;
        [SerializeField] private bool applyLayerToVisualChildren = true;

        public EraVisibilityMode Visibility => visibility;

        public void Configure(EraVisibilityMode mode, bool applyLayer = true)
        {
            visibility = mode;
            applyLayerToVisualChildren = applyLayer;
            ApplyLayerAssignment();
        }

        private void Awake()
        {
            ApplyLayerAssignment();
        }

        public void ApplyLayerAssignment()
        {
            if (!applyLayerToVisualChildren || visibility == EraVisibilityMode.Both) return;

            int targetLayer = visibility == EraVisibilityMode.Only1929
                ? EraVisualManager.GetEra1929Layer()
                : EraVisualManager.GetEraRuinsLayer();

            SetLayerSafeKeepingCollidersOnDefault(transform, targetLayer);
        }

        private static void SetLayerSafeKeepingCollidersOnDefault(Transform root, int visualLayer)
        {
            if (root == null) return;
            Collider col = root.GetComponent<Collider>();
            if (col == null)
            {
                root.gameObject.layer = visualLayer;
            }
            foreach (Transform child in root)
            {
                SetLayerSafeKeepingCollidersOnDefault(child, visualLayer);
            }
        }
    }
}
