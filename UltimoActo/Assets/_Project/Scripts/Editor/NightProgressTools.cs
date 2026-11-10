#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UltimoActo.Core;

namespace UltimoActo.Editor
{
    /// <summary>Utilidades de edición para el sistema de noches y pruebas para la evaluación docente.</summary>
    public static class NightProgressTools
    {
        [MenuItem("Tools/Último Acto/Noches/Reiniciar a Noche I (La Llegada)")]
        public static void ResetNightProgress()
        {
            SetNightIndex(0);
        }

        [MenuItem("Tools/Último Acto/Noches/Saltar a Noche II (Las Aulas de Ensayo)")]
        public static void JumpToNight2() => SetNightIndex(1);

        [MenuItem("Tools/Último Acto/Noches/Saltar a Noche III (El Internado)")]
        public static void JumpToNight3() => SetNightIndex(2);

        [MenuItem("Tools/Último Acto/Noches/Saltar a Noche IV (Archivo y Parque Interior)")]
        public static void JumpToNight4() => SetNightIndex(3);

        [MenuItem("Tools/Último Acto/Noches/Saltar a Noche V (Lo Alto y lo Profundo)")]
        public static void JumpToNight5() => SetNightIndex(4);

        [MenuItem("Tools/Último Acto/Noches/Saltar a Noche VI (Bambalinas)")]
        public static void JumpToNight6() => SetNightIndex(5);

        [MenuItem("Tools/Último Acto/Noches/Saltar a Noche VII (El Lago de las Sombras)")]
        public static void JumpToNight7() => SetNightIndex(6);

        [MenuItem("Tools/Último Acto/Noches/Saltar a Noche VIII (Último Acto)")]
        public static void JumpToNight8() => SetNightIndex(7);

        private static void SetNightIndex(int index)
        {
            PlayerPrefs.SetInt(NightManager.PrefsKey, index);
            PlayerPrefs.Save();

            if (Application.isPlaying && NightManager.Instance != null)
            {
                NightManager.Instance.DevSkipNight();
            }
            else
            {
                Debug.Log($"<color=cyan>[Último Acto]</color> Noche fijada en {index + 1}. Al iniciar Play Mode comenzará en esa noche.");
            }
        }
    }
}
#endif
