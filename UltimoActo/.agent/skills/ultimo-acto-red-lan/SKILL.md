---
name: ultimo-acto-red-lan
description: Arquitectura de red LAN para 2 jugadores en Unity con Netcode for GameObjects (host autoritativo, estado compartido con NetworkVariable/NetworkList, RPCs, lobby, desconexiones). Úsalo al crear o modificar cualquier cosa que deba sincronizarse entre los dos jugadores en Último Acto.
---

# Red LAN cooperativa (Netcode for GameObjects)

Antes: leer `ultimo-acto-contexto`. Verifica la versión instalada de `com.unity.netcode.gameobjects` (la sintaxis de RPC cambió entre 1.x y 2.x) y adáptate a ella.

## Modelo
- **Host-client**: un jugador es host (servidor + jugador). Máximo 2 conexiones.
- **Servidor autoritativo para el progreso**: el cliente *pide* (RPC al servidor), el servidor *valida y cambia* el estado, el estado se replica por `NetworkVariable`/`NetworkList`.
- **Estado compartido** (puertas, mecanismos, puzles resueltos, inventario, cuaderno, coleccionables): SIEMPRE en el servidor.
- **Efectos locales** (anomalías visuales, susurros, parpadeos de luz): solo cliente, disparados por un evento sincronizado (ver `ultimo-acto-anomalias`).

## Reglas
1. Nunca cambiar estado compartido desde el cliente. Cliente → `Rpc(SendTo.Server)` → validar → modificar.
2. Validar en servidor: ¿existe el objeto? ¿ya está resuelto? ¿el jugador está a distancia razonable? ¿tiene la llave?
3. Operaciones **idempotentes**: pulsar dos veces o reintentar no duplica recompensas (ver `ultimo-acto-guardado`).
4. Objetos de escena con estado = `NetworkObject` en escena (spawn automático) con `NetworkBehaviour`.
5. Usar IDs estables (string en ScriptableObject, p. ej. `"door_rehearsal_01"`), nunca índices de jerarquía.
6. Pocos mensajes: sincronizar estado, no cada frame. La posición del jugador va con `NetworkTransform` (autoridad de cliente aceptable en cooperativo LAN; documenta la decisión).

## Conexión (LAN directa por IP)
```csharp
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class LanConnection : MonoBehaviour
{
    const ushort Port = 7777;

    public void StartHost()
    {
        var nm = NetworkManager.Singleton;
        var utp = nm.GetComponent<UnityTransport>();
        utp.SetConnectionData("0.0.0.0", Port, "0.0.0.0"); // escucha en todas las interfaces
        nm.NetworkConfig.ConnectionApproval = true;
        nm.ConnectionApprovalCallback = (req, res) =>
        {
            bool hayHueco = nm.ConnectedClientsIds.Count < 2;
            res.Approved = hayHueco;
            res.CreatePlayerObject = true;
            res.Reason = hayHueco ? "" : "Partida llena";
        };
        nm.StartHost();
    }

    public void StartClient(string hostIp)
    {
        var nm = NetworkManager.Singleton;
        nm.GetComponent<UnityTransport>().SetConnectionData(hostIp, Port);
        nm.StartClient();
    }
}
```
- Mostrar al host su IP local en el Lobby (`System.Net.Dns` / `NetworkInterface`), y permitir que el cliente la teclee. Descubrimiento automático por UDP broadcast = mejora posterior, no MVP.
- Firewall: avisar al usuario si el cliente no conecta (puerto UDP 7777).

## Estado compartido: patrón base
```csharp
public class SharedBool : NetworkBehaviour
{
    public NetworkVariable<bool> Value = new(false,
        NetworkVariable<bool>.DefaultReadPerm, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        Value.OnValueChanged += OnChanged;
        OnChanged(false, Value.Value); // aplica el estado actual al unirse tarde
    }
    public override void OnNetworkDespawn() => Value.OnValueChanged -= OnChanged;
    protected virtual void OnChanged(bool prev, bool now) { }
}
```
Importante: aplicar el estado en `OnNetworkSpawn` para que un cliente que se une o reconecta vea el estado correcto.

## Desconexiones
- Cliente se desconecta: el host sigue; mostrar aviso; permitir reconectar y recibir el estado completo (NetworkVariables se resincronizan solas).
- Host se desconecta: la partida termina; ofrecer volver al menú. Escena `DisconnectRecovery` solo si hace falta restaurar desde el último guardado.
- Nunca perder progreso por una desconexión: el guardado vive en el host (ver `ultimo-acto-guardado`).

## Pruebas (obligatorias antes de dar algo por hecho)
- Dos instancias en la misma máquina (build + Editor, o *Multiplayer Play Mode*).
- Probar: unirse tarde, reconectar, doble pulsación simultánea, ambos jugadores interactuando con el mismo objeto a la vez.
