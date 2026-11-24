using System;
using UnityEngine;
#if ULTIMOACTO_NETCODE
using Unity.Netcode;
using Unity.Netcode.Components;
#endif

namespace UltimoActo.Network
{
    /// <summary>
    /// Paso 3 — Jugador en red:
    /// NetworkTransform con autoridad del cliente dueño (IsServerAuthoritative => false),
    /// para que tanto el Host (1929) como el Cliente (Ruinas) muevan su propio personaje
    /// con fluidez inmediata y sincronicen posición y rotación en tiempo real.
    /// </summary>
#if ULTIMOACTO_NETCODE
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
#else
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : MonoBehaviour
    {
    }
#endif
}
