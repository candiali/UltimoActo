#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UltimoActo.Core;

namespace UltimoActo.Editor
{
    /// <summary>Utilidades de edición para el sistema de noches.</summary>
    public static class NightProgressTools
    {
        [MenuItem("Tools/Último Acto/Reiniciar progreso de noches")]
        public static void ResetNightProgress()
        {
            PlayerPrefs.DeleteKey(NightManager.PrefsKey);
            PlayerPrefs.Save();
            Debug.Log("<color=cyan>[Último Acto]</color> Progreso de noches reiniciado: la próxima partida empezará en la Noche I.");
        }
    }
}
#endif
