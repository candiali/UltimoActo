using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimoActo.Core
{
    /// <summary>
    /// Inventario ligero para la Fase 1.
    /// Registra llaves y objetos recolectados. En Fase 2 se sincronizará a través de NetworkList/NetworkVariable.
    /// </summary>
    public class SimpleInventory : MonoBehaviour
    {
        [Header("Debug / Vista Previa")]
        [SerializeField] private List<string> currentKeys = new List<string>();

        public event Action<string> OnKeyAdded;

        public bool HasKey(string keyId)
        {
            if (string.IsNullOrEmpty(keyId)) return true;
            return currentKeys.Contains(keyId);
        }

        public void AddKey(string keyId, string displayName = "")
        {
            if (string.IsNullOrEmpty(keyId)) return;

            if (!currentKeys.Contains(keyId))
            {
                currentKeys.Add(keyId);
                Debug.Log($"[Inventario] Llave añadida: {(string.IsNullOrEmpty(displayName) ? keyId : displayName)} (ID: {keyId})");
                OnKeyAdded?.Invoke(keyId);
            }
        }

        public void RemoveKey(string keyId)
        {
            if (currentKeys.Contains(keyId))
            {
                currentKeys.Remove(keyId);
                Debug.Log($"[Inventario] Llave usada/removida: {keyId}");
            }
        }
    }
}
