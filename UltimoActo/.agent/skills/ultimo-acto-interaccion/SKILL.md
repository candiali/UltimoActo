---
name: ultimo-acto-interaccion
description: Sistema de interacción reutilizable para Último Acto en Unity: raycast en primera persona, interactuables en red, puertas y llaves, inventario ligero compartido, cuaderno compartido y documentos. Úsalo al implementar puertas, objetos recogibles, mecanismos, inventario o cuaderno.
---

# Interacción, puertas, inventario y cuaderno

Antes: `ultimo-acto-contexto` y `ultimo-acto-red-lan`. **Un solo sistema de interacción para todo** (reutilizar, regla de alcance).

## Flujo
1. Cliente local: raycast desde la cámara (distancia ~2.5 m, LayerMask `Interactable`). Si golpea un `NetworkInteractable`, muestra el indicador de interacción con su `Prompt`.
2. Al pulsar la tecla: `RequestInteract()` → RPC al servidor.
3. Servidor: `CanInteract(clientId)` → `OnServerInteract(clientId)` → cambia estado (NetworkVariable).
4. Todos los clientes reaccionan al cambio de estado (animación, sonido, UI).

## Base
```csharp
using Unity.Netcode;
using UnityEngine;

public abstract class NetworkInteractable : NetworkBehaviour
{
    [SerializeField] string id;            // ID estable y único (p. ej. "door_lobby_rehearsal")
    [SerializeField] string prompt = "Interactuar";
    [SerializeField] float maxDistance = 3f;
    public string Id => id;
    public virtual string Prompt => prompt;

    public void RequestInteract() => InteractRpc();

    // Si tu versión de NGO es 1.x: [ServerRpc(RequireOwnership = false)]
    [Rpc(SendTo.Server, RequireOwnership = false)]
    void InteractRpc(RpcParams rpcParams = default)
    {
        ulong sender = rpcParams.Receive.SenderClientId;
        if (!IsPlayerInRange(sender)) return;
        if (!CanInteract(sender)) return;
        OnServerInteract(sender);
    }

    bool IsPlayerInRange(ulong clientId)
    {
        var po = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        return po != null && Vector3.Distance(po.transform.position, transform.position) <= maxDistance + 1f;
    }

    protected virtual bool CanInteract(ulong clientId) => true;
    protected abstract void OnServerInteract(ulong clientId);   // SOLO servidor
}
```
Las subclases: `DoorInteractable`, `PickupInteractable`, `DocumentInteractable`, `LeverInteractable`, `MirrorInteractable`, etc. Los puzles las reutilizan (ver `ultimo-acto-puzles`).

## Inventario ligero COMPARTIDO
Llaves/emblemas/objetos de puzle son del *equipo*, no de un jugador (evita bloqueos si uno se desconecta).
```csharp
public class TeamInventory : NetworkBehaviour
{
    public static TeamInventory Instance { get; private set; }
    public NetworkList<Unity.Collections.FixedString64Bytes> Items = new();

    void Awake() => Instance = this;

    public bool Has(string itemId) => Items.Contains(itemId);

    // Solo servidor
    public bool TryAdd(string itemId)
    {
        if (!IsServer || Items.Contains(itemId)) return false;
        Items.Add(itemId);
        return true;
    }
    public bool TryConsume(string itemId)
    {
        if (!IsServer) return false;
        return Items.Remove(itemId);
    }
}
```
- Mostrar el inventario como lista corta (no cuadrícula compleja).
- Objetos recogidos: `PickupInteractable` hace `TryAdd`, y solo si tuvo éxito despawnea el objeto y registra el ID como recogido (idempotente).

## Puertas y llaves
```csharp
public class DoorInteractable : NetworkInteractable
{
    [SerializeField] string requiredItemId;       // vacío = abre libre
    [SerializeField] bool consumeKey = false;
    public NetworkVariable<bool> IsOpen = new(false);

    protected override bool CanInteract(ulong c) =>
        !IsOpen.Value && (string.IsNullOrEmpty(requiredItemId) || TeamInventory.Instance.Has(requiredItemId));

    protected override void OnServerInteract(ulong c)
    {
        if (consumeKey && !string.IsNullOrEmpty(requiredItemId)) TeamInventory.Instance.TryConsume(requiredItemId);
        IsOpen.Value = true;
    }
    // Una puerta también puede abrirse desde un puzle: método público OpenFromServer().
}
```
- Mensaje claro si falta la llave ("Está cerrada con llave"), vía ClientRpc solo al jugador que intentó.
- Las puertas controladas por la historia/puzles exponen `OpenFromServer()`.

## Cuaderno compartido
- Lista de `NoteEntry` (id de ScriptableObject) en `NetworkList` → ambos ven lo mismo.
- Se añade al leer un documento o al resolver una pista. Tipos: documento, pista de puzle, fotografía, testimonio.
- **Obligatorio**: toda solución/pista de puzle importante queda anotada y se puede releer. Mostrar también ilustración de la pista (ej. patrón de espejos).
- UI: pestañas simples (Documentos / Pistas / Fotos / Coleccionables). Pausa el movimiento local, NO el juego en red.

## Checklist
- [ ] Id único y estable en cada interactuable
- [ ] Toda mutación pasa por el servidor
- [ ] Reintento/doble pulsación no duplica nada
- [ ] Estado correcto para un cliente que se une tarde
- [ ] Feedback de audio/visual en todos los clientes
