---
name: ultimo-acto-puzles
description: Diseño e implementación de puzles cooperativos para Último Acto (observación, lógica, musicales, coreográficos, mecánicos, cooperativos): reglas de diseño, framework común sincronizado en red y recetas para espejos, partitura y última escena. Úsalo al diseñar, implementar o revisar cualquier puzle.
---

# Puzles

Antes: `ultimo-acto-contexto`, `ultimo-acto-interaccion`, `ultimo-acto-red-lan`.

## Familias
1. Observación (cambios en fotos, salas, reflejos)
2. Lógica (símbolos, fechas, objetos, testimonios)
3. Musicales (secuencias, partituras)
4. Coreográficos (poses de bailarines/maniquíes)
5. Mecánicos (palancas, espejos, luces, decorados)
6. Cooperativos (compartir pistas, coordinar, acciones simultáneas)

## Reglas de diseño (revisar cada puzle contra esto)
- **Deducible**: la solución sale de pistas presentes en el juego. Nada de adivinar ni pixel hunting.
- **Recuperable**: las pistas y la solución quedan en el Cuaderno. Si el jugador falla, puede releer.
- **Sin dependencia de rol**: ninguno requiere una habilidad exclusiva. La cooperación = posición distinta / información parcial / acción coordinada.
- **Nunca bloqueable** (softlock): si se gasta, se pierde o se rompe algo, hay forma de reiniciar el puzle. Todo puzle tiene `ResetPuzzle()`.
- **Fallo barato**: equivocarse = feedback + reintento. No castigar con pérdida de progreso.
- **Reutilizar** el sistema de interacción; variar las reglas, no el código.
- **Probar con gente nueva** antes de darlo por bueno.

## Framework común
```csharp
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public enum PuzzleState : byte { Locked, Active, Solved }

public abstract class NetworkPuzzle : NetworkBehaviour
{
    [SerializeField] string puzzleId;                 // estable y único
    [SerializeField] string rewardItemId;             // opcional: llave/objeto
    [SerializeField] DoorInteractable doorToOpen;     // opcional
    public UnityEvent onSolvedLocal;                  // efectos locales (sonido, luz)

    public NetworkVariable<PuzzleState> State = new(PuzzleState.Active);
    public string PuzzleId => puzzleId;

    public override void OnNetworkSpawn() => State.OnValueChanged += (_, s) => { if (s == PuzzleState.Solved) onSolvedLocal.Invoke(); };

    // SOLO servidor. Llamar cuando la lógica concreta detecte la solución.
    protected void Solve()
    {
        if (!IsServer || State.Value == PuzzleState.Solved) return;   // idempotente
        State.Value = PuzzleState.Solved;
        if (!string.IsNullOrEmpty(rewardItemId) && RewardLedger.Instance.TryClaim($"puzzle:{puzzleId}"))
            TeamInventory.Instance.TryAdd(rewardItemId);
        if (doorToOpen) doorToOpen.OpenFromServer();
        SaveManager.Instance?.NotifyProgress();
    }

    public abstract void ResetPuzzle();   // servidor
}
```
(`RewardLedger` y `SaveManager`: ver `ultimo-acto-guardado`.)

## Recetas

### Espejos (observación + mecánico + cooperativo)
- Jugador A ve el patrón objetivo desde una sala/ángulo; jugador B manipula los espejos desde otro. Deben comunicarse por voz.
- Cada espejo: `NetworkVariable<int> angleStep` (pasos discretos, p. ej. 8 posiciones). Rotar = RPC al servidor.
- Solución: array de pasos objetivo. El servidor comprueba tras cada cambio; al coincidir → `Solve()`.
- Pista en Cuaderno: foto del patrón + posición de cada espejo cuando se vea.
- Fallback: botón/palanca de reinicio de espejos.

### Partitura (musical + lógica)
- Fragmentos de partitura repartidos en salas (coleccionable `ScoreFragment`). Cada fragmento recogido va al Cuaderno.
- Puzle en el piano/caja de música: secuencia de notas objetivo = unir fragmentos en orden (el orden se deduce de números de compás/fechas).
- Servidor guarda la secuencia introducida; error → reinicia secuencia con sonido de desafinado, sin penalización.
- Subtítulo/indicador visual para cada nota (accesibilidad).

### La última escena (coreográfico + cooperativo, final)
- Colocar elementos del escenario (utilería, maniquíes en poses, luces) y activar mecanismos coordinados para reconstruir la escena.
- Estados intermedios guardados: cada elemento colocado es un estado compartido persistente (no se pierde si hay desconexión).
- Paso final con acción simultánea: ventana de tiempo generosa (≥ 3 s) y feedback claro de "el otro ya está listo". Implementar con `NetworkList<ulong>` de jugadores "listos" validado en servidor.

## Acción simultánea (patrón)
```csharp
// Servidor: cada jugador registra "listo" y lo mantiene; se resuelve si ambos están listos dentro de la ventana
HashSet<ulong> ready = new();
void SetReady(ulong id, bool v) { if (v) ready.Add(id); else ready.Remove(id); if (ready.Count == 2) Solve(); }
```
Evita requerir precisión de milisegundos; la latencia LAN es baja pero no cero.

## Plantilla para documentar cada puzle (rellenar antes de implementar)
```
ID: puz_xxx
Familia(s):
Capítulo / sala:
Objetivo del jugador:
Pistas (dónde están, cómo llegan al Cuaderno):
Solución:
Rol de cada jugador (sin habilidades exclusivas):
Cómo se reinicia:
Recompensa / desbloqueo:
Riesgo de softlock y mitigación:
```
