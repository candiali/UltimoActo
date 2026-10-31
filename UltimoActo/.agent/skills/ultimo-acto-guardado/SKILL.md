---
name: ultimo-acto-guardado
description: Sistema de guardado en puntos seguros para Último Acto, gestionado por el anfitrión LAN: formato de datos, guardado atómico, registro de recompensas para no duplicarlas, carga y restauración del estado compartido. Úsalo al implementar guardar/cargar, checkpoints o recompensas únicas.
---

# Guardado y recompensas únicas

Antes: `ultimo-acto-contexto`, `ultimo-acto-red-lan`.

## Reglas
- **El host gestiona el guardado** (primera versión LAN). El cliente nunca escribe partida.
- Guardar en **puntos seguros** (checkpoints al abrir una zona, resolver un puzle, cambiar de capítulo) y al salir limpiamente desde el menú de pausa. No guardar a mitad de una anomalía ni de una acción simultánea.
- **Idempotencia**: una recompensa (llave, documento, coleccionable) se concede una sola vez, aunque haya reintentos de RPC, doble pulsación, reconexión o recarga.
- Guardado **atómico**: escribir a archivo temporal y reemplazar; conservar copia de seguridad.
- Versionar el formato (`version`) para migrar partidas.

## Datos
```csharp
[System.Serializable]
public class SaveData
{
    public int version = 1;
    public string lastCheckpointId;
    public int chapter;
    public string savedAtUtc;
    public System.Collections.Generic.List<string> claimedRewards = new();   // "puzzle:puz_mirrors_01", "pickup:key_rehearsal"
    public System.Collections.Generic.List<string> openedDoors = new();
    public System.Collections.Generic.List<string> solvedPuzzles = new();
    public System.Collections.Generic.List<string> teamItems = new();
    public System.Collections.Generic.List<string> notebookEntries = new();
    public System.Collections.Generic.List<string> collectibles = new();
    public System.Collections.Generic.List<string> storyFlags = new();       // "room_dressing_changed", etc.
}
```
Todo se identifica por **ID estable** (string), no por referencias de escena.

## Ledger de recompensas
```csharp
public class RewardLedger : MonoBehaviour   // solo tiene sentido en servidor
{
    public static RewardLedger Instance { get; private set; }
    readonly HashSet<string> claimed = new();
    void Awake() => Instance = this;

    /// true = primera vez (conceder). false = ya concedida (ignorar).
    public bool TryClaim(string rewardKey) => claimed.Add(rewardKey);
    public IEnumerable<string> All => claimed;
    public void Load(IEnumerable<string> keys) { claimed.Clear(); foreach (var k in keys) claimed.Add(k); }
}
```

## Escritura atómica
```csharp
using System.IO; using UnityEngine;

public static class SaveIO
{
    static string Path(string slot) => System.IO.Path.Combine(Application.persistentDataPath, $"save_{slot}.json");

    public static void Write(string slot, SaveData data)
    {
        string path = Path(slot), tmp = path + ".tmp", bak = path + ".bak";
        File.WriteAllText(tmp, JsonUtility.ToJson(data, true));
        if (File.Exists(path)) File.Replace(tmp, path, bak); else File.Move(tmp, path);
    }

    public static SaveData Read(string slot)
    {
        foreach (var p in new[] { Path(slot), Path(slot) + ".bak" })
        {
            try { if (File.Exists(p)) return JsonUtility.FromJson<SaveData>(File.ReadAllText(p)); }
            catch (System.Exception e) { Debug.LogWarning($"Save corrupto {p}: {e.Message}"); }
        }
        return null;
    }
}
```

## SaveManager (servidor)
- `NotifyProgress()`: marca "sucio"; guarda en el siguiente punto seguro (debounce, p. ej. 2 s).
- `Capture()`: recorre `NetworkInteractable`/`NetworkPuzzle`/`TeamInventory`/notebook y rellena `SaveData`.
- `ApplyOnHostStart(SaveData)`: tras cargar la escena y hacer spawn, aplicar IDs: abrir puertas, marcar puzles resueltos (sin re-conceder recompensas: `RewardLedger.Load` primero), repoblar inventario/cuaderno, no re-spawnear pickups ya recogidos.
- Al reconectar un cliente: no hace falta guardar nada; recibe el estado por NetworkVariables.

## Pruebas obligatorias
- Resolver puzle → matar proceso → cargar: la puerta sigue abierta y la llave NO se duplica.
- Doble pulsación / RPC repetido: una sola recompensa.
- Cliente cae y vuelve: estado idéntico al del host.
- Guardado corrupto (editar JSON): cae a `.bak` sin crashear.
- Guardar dentro de una anomalía o acción simultánea: bloqueado hasta terminar.
