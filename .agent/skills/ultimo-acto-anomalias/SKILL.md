---
name: ultimo-acto-anomalias
description: Sistema de anomalías y terror psicológico de Último Acto: eventos sincronizados con efectos visuales/sonoros por jugador, catálogo de anomalías, ritmo, silencios y reglas para que no se confundan con errores de red ni borren progreso. Úsalo al crear sustos, eventos atmosféricos, reflejos, fotos cambiantes o sonidos.
---

# Anomalías y terror psicológico

Antes: `ultimo-acto-contexto`, `ultimo-acto-red-lan`.

## Principio clave
**Separar el evento (compartido) de su presentación (local).**
- El *evento* ("anomalía X ocurre, fase Y") lo decide el servidor y lo ven ambos clientes.
- La *presentación* puede diferir por jugador (uno ve una figura, el otro no) → es un efecto local, nunca afecta al progreso compartido.
- Así una anomalía asimétrica no se confunde con un bug de sincronización, y el progreso sigue siendo coherente.

## Estructura
```csharp
[CreateAssetMenu(menuName = "UltimoActo/Anomaly")]
public class AnomalyData : ScriptableObject
{
    public string id;                       // "anom_mirror_follow"
    public float durationSeconds = 6f;
    public AnomalyAudience audience;        // Both, OnlyPlayerA, OnlyPlayerB, RandomOne
    public bool affectsProgress = false;    // si true: DEBE ser un evento sincronizado y reversible
    public AudioClip sfx;
    public bool silenceBefore = true;       // corta ambiente 1–3 s antes
}
public enum AnomalyAudience { Both, OnlyPlayerA, OnlyPlayerB, RandomOne }
```
```csharp
public class AnomalyDirector : NetworkBehaviour
{
    // Servidor decide cuándo y a quién.
    public void Trigger(AnomalyData a)
    {
        if (!IsServer) return;
        ulong target = ChooseTarget(a);                    // clientId o ulong.MaxValue = ambos
        PlayAnomalyRpc(a.id, target, NetworkManager.ServerTime.Time);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void PlayAnomalyRpc(string anomalyId, ulong targetClient, double serverTime)
    {
        bool me = targetClient == ulong.MaxValue || targetClient == NetworkManager.LocalClientId;
        AnomalyPresenter.Play(anomalyId, presentFull: me);   // los demás reciben, como mucho, una versión mínima/neutra
    }
}
```
`AnomalyPresenter` busca el `AnomalyActor` local con ese id (reflejo, foto, luz, audio 3D) y lo reproduce.

## Catálogo inicial (del GDD)
| Anomalía | Implementación | Audiencia |
|---|---|---|
| Reflejo que sigue moviéndose al detenerse | Espejo con "doble" animado que desfasa unos frames | Uno o ambos |
| Aplausos tras telón vacío | AudioSource 3D detrás del telón, cola larga | Ambos |
| Foto que cambia al volver | Intercambio de textura al re-entrar a la sala (evento al salir/entrar, estado sincronizado) | Ambos |
| Melodía desde sala que no figura en planos | Audio 3D hacia sala oculta/pared falsa; pista de ruta secreta | Ambos |
| Figura que solo ve uno | Silueta/maniquí visible solo en el cliente objetivo | Uno |
| Cambio de aspecto de sala tras un hecho | Variante de sala (dos prefabs/estados), sincronizada por flag de historia | Ambos |

## Ritmo y reglas
- Breves (3–10 s) y variadas. Ninguna se repite seguida. Presupuesto: ≈1 anomalía menor por sala nueva, 1 mayor por capítulo.
- **Silencio** antes de algunas; luz y sonido espacial > criaturas.
- Sin monstruo persiguiendo continuamente. Persecución = opcional, puntual, con refugio garantizado y sin pérdida de progreso.
- Jamás: muerte con pérdida de progreso, ni quitar control durante más de unos segundos, ni jumpscares sin aviso sutil previo (tono = inquietud).
- Si `affectsProgress`: el estado resultante (p. ej. puerta que aparece) es un estado de servidor normal, persistente y guardable.
- Todo evento sonoro importante para un puzle debe tener equivalente visual/subtítulo (accesibilidad).
- Opción en ajustes: intensidad de anomalías (reducida / normal) y desactivar parpadeos fuertes de luz.

## Checklist
- [ ] Evento decidido por servidor, presentación local
- [ ] No altera estado compartido salvo `affectsProgress` y entonces vía servidor
- [ ] Duración corta, tiene fin garantizado (timeout)
- [ ] Accesible (subtítulo / alternativa visual)
- [ ] Probada con 2 jugadores: se entiende que es intencional, no un bug
