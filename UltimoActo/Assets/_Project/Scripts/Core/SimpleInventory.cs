using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimoActo.Core
{
    /// <summary>
    /// Inventario compartido entre las instancias de jugador y sincronizado en red (Paso 4 — Autoridad del Host).
    /// Registra llaves, fusibles y objetos recolectados para que ambos jugadores compartan progreso.
    /// </summary>
    public class SimpleInventory : MonoBehaviour
    {
        private static readonly HashSet<string> SharedKeys = new HashSet<string>();

        [Header("Debug / Vista Previa")]
        [SerializeField] private List<string> currentKeys = new List<string>();

        public static event Action<string, string> OnGlobalKeyAdded;
        public static event Action<string> OnGlobalKeyRemoved;
        public event Action<string> OnKeyAdded;

        public IReadOnlyCollection<string> Keys => currentKeys;
        public static IReadOnlyCollection<string> GlobalKeys => SharedKeys;

        private void Awake()
        {
            foreach (string k in SharedKeys)
            {
                if (!currentKeys.Contains(k)) currentKeys.Add(k);
            }
        }

        public bool HasKey(string keyId)
        {
            if (string.IsNullOrEmpty(keyId)) return true;
            return SharedKeys.Contains(keyId) || currentKeys.Contains(keyId);
        }

        public void AddKey(string keyId, string displayName = "")
        {
            AddKeyInternal(keyId, displayName, false);
        }

        public void AddKeyFromNetwork(string keyId, string displayName = "")
        {
            AddKeyInternal(keyId, displayName, true);
        }

        private void AddKeyInternal(string keyId, string displayName, bool fromNetwork)
        {
            if (string.IsNullOrEmpty(keyId)) return;

            bool addedNew = SharedKeys.Add(keyId);
            if (!currentKeys.Contains(keyId))
            {
                currentKeys.Add(keyId);
            }

            // Mantener sincronizadas todas las instancias de SimpleInventory en escena
            SimpleInventory[] allInventories = FindObjectsByType<SimpleInventory>(FindObjectsSortMode.None);
            for (int i = 0; i < allInventories.Length; i++)
            {
                if (allInventories[i] != null && !allInventories[i].currentKeys.Contains(keyId))
                {
                    allInventories[i].currentKeys.Add(keyId);
                }
            }

            if (addedNew)
            {
                Debug.Log($"[Inventario Compartido] Llave/Objeto añadido: {(string.IsNullOrEmpty(displayName) ? keyId : displayName)} (ID: {keyId})");
                OnKeyAdded?.Invoke(keyId);
                if (!fromNetwork)
                {
                    OnGlobalKeyAdded?.Invoke(keyId, displayName ?? string.Empty);
                }
            }
        }

        public void RemoveKey(string keyId)
        {
            RemoveKeyInternal(keyId, false);
        }

        public void RemoveKeyFromNetwork(string keyId)
        {
            RemoveKeyInternal(keyId, true);
        }

        private void RemoveKeyInternal(string keyId, bool fromNetwork)
        {
            if (string.IsNullOrEmpty(keyId)) return;

            bool removed = SharedKeys.Remove(keyId);
            if (currentKeys.Contains(keyId))
            {
                currentKeys.Remove(keyId);
            }

            SimpleInventory[] allInventories = FindObjectsByType<SimpleInventory>(FindObjectsSortMode.None);
            for (int i = 0; i < allInventories.Length; i++)
            {
                if (allInventories[i] != null && allInventories[i].currentKeys.Contains(keyId))
                {
                    allInventories[i].currentKeys.Remove(keyId);
                }
            }

            if (removed)
            {
                Debug.Log($"[Inventario Compartido] Llave/Objeto usado: {keyId}");
                if (!fromNetwork)
                {
                    OnGlobalKeyRemoved?.Invoke(keyId);
                }
            }
        }
    }
}
