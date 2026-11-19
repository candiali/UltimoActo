using UnityEngine;

namespace UltimoActo.Core
{
    /// <summary>
    /// Permite que una malla estructural compartida (con Collider común en Default)
    /// cambie automáticamente su material PBR entre 1929 y Ruinas por cámara.
    /// </summary>
    public class EraSurfaceAdapter : MonoBehaviour
    {
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Material material1929;
        [SerializeField] private Material materialRuins;

        public void Configure(Renderer rend, Material mat1929, Material matRuins)
        {
            targetRenderer = rend != null ? rend : GetComponent<Renderer>();
            material1929 = mat1929;
            materialRuins = matRuins;
            EraVisualManager.RegisterSurface(this);
            if (EraVisualManager.Instance != null)
            {
                ApplyEra(EraVisualManager.Instance.CurrentEra);
            }
        }

        private void OnEnable()
        {
            if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
            EraVisualManager.RegisterSurface(this);
            if (EraVisualManager.Instance != null)
            {
                ApplyEra(EraVisualManager.Instance.CurrentEra);
            }
        }

        private void OnDisable()
        {
            EraVisualManager.UnregisterSurface(this);
        }

        public void ApplyEra(EraVisual era)
        {
            if (targetRenderer == null) return;
            Material target = era == EraVisual.Glory1929 ? material1929 : materialRuins;
            if (target != null)
            {
                targetRenderer.sharedMaterial = target;
            }
        }
    }
}
