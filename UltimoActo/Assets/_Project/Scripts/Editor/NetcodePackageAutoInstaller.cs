#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace UltimoActo.Editor
{
    /// <summary>
    /// Verifica e instala automáticamente 'Netcode for GameObjects' (com.unity.netcode.gameobjects)
    /// y 'Unity Transport' (com.unity.transport) mediante el Package Manager de Unity,
    /// y activa el símbolo de compilación ULTIMOACTO_NETCODE cuando el paquete está listo.
    /// </summary>
    [InitializeOnLoad]
    public static class NetcodePackageAutoInstaller
    {
        private const string NetcodePkgId = "com.unity.netcode.gameobjects";
        private const string TransportPkgId = "com.unity.transport";
        private const string NetcodeDefine = "ULTIMOACTO_NETCODE";
        private const string AutoInstallSessionKey = "UltimoActo_Netcode_AutoInstall_Checked_V1";

        private static AddAndRemoveRequest installRequest;

        static NetcodePackageAutoInstaller()
        {
            EditorApplication.delayCall += CheckOrInstallNetcode;
        }

        [MenuItem("Tools/Último Acto/Red LAN/Verificar o Instalar Netcode for GameObjects")]
        public static void ForceCheckOrInstallNetcode()
        {
            SessionState.SetBool(AutoInstallSessionKey, false);
            CheckOrInstallNetcode();
        }

        private static void CheckOrInstallNetcode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            var netcodeInfo = UnityEditor.PackageManager.PackageInfo.FindForPackageName(NetcodePkgId);
            if (netcodeInfo != null)
            {
                EnsureScriptingDefine(NetcodeDefine, true);
                return;
            }

            // Si aún no está instalado, quitar el define temporalmente para que todo compile limpio mientras se descarga
            EnsureScriptingDefine(NetcodeDefine, false);

            if (SessionState.GetBool(AutoInstallSessionKey, false)) return;
            SessionState.SetBool(AutoInstallSessionKey, true);

            Debug.Log("<color=cyan>[Último Acto · Red LAN]</color> Instalando 'Netcode for GameObjects' y 'Unity Transport' desde Package Manager...");
            installRequest = Client.AddAndRemove(new[] { NetcodePkgId, TransportPkgId }, null);
            EditorApplication.update += MonitorInstallProgress;
        }

        private static void MonitorInstallProgress()
        {
            if (installRequest == null || !installRequest.IsCompleted) return;

            EditorApplication.update -= MonitorInstallProgress;

            if (installRequest.Status == StatusCode.Success)
            {
                Debug.Log("<color=green>[Último Acto · Red LAN]</color> ¡Netcode for GameObjects y Unity Transport instalados correctamente!");
                EnsureScriptingDefine(NetcodeDefine, true);
            }
            else if (installRequest.Error != null)
            {
                Debug.LogWarning($"[Último Acto · Red LAN] No se pudo instalar automáticamente Netcode ({installRequest.Error.message}). Puedes instalarlo desde Window > Package Manager > Netcode for GameObjects.");
            }

            installRequest = null;
        }

        private static void EnsureScriptingDefine(string define, bool shouldBePresent)
        {
            NamedBuildTarget target = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (target == NamedBuildTarget.Unknown) target = NamedBuildTarget.Standalone;

            PlayerSettings.GetScriptingDefineSymbols(target, out string[] defines);
            List<string> list = new List<string>(defines);
            bool hasDefine = list.Contains(define);

            if (shouldBePresent && !hasDefine)
            {
                list.Add(define);
                PlayerSettings.SetScriptingDefineSymbols(target, list.ToArray());
                Debug.Log($"<color=green>[Último Acto · Red LAN]</color> Símbolo '{define}' activado.");
            }
            else if (!shouldBePresent && hasDefine)
            {
                list.Remove(define);
                PlayerSettings.SetScriptingDefineSymbols(target, list.ToArray());
            }
        }
    }
}
#endif
