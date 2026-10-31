---
name: ultimo-acto-qa-roadmap
description: Plan por fases, MVP y criterios de aceptación de Último Acto, con control de alcance y checklists de prueba/QA. Úsalo para decidir qué construir ahora, ordenar tareas, revisar si algo está listo o evaluar riesgos de alcance.
---

# Roadmap, MVP y QA

Antes: `ultimo-acto-contexto`.

## Fases (no saltarse el orden)
1. **Prototipo**: movimiento, cámara, interacción, una puerta, una habitación (`Academy_Prototype`).
2. **Cooperativo**: LAN, sincronización de ambos jugadores, estado compartido de objetos.
3. **Primer puzle**: pistas, solución, recompensa, desbloqueo de nueva habitación.
4. **Vertical slice**: 2–3 habitaciones conectadas, historia corta, 1 anomalía completa.
5. **Contenido completo**: capítulos, documentos, coleccionables, secretos, puzles.
6. **Pruebas y pulido**: sincronización, bugs, dificultad, guardado, duración.

Regla: empezar por UNA sala pequeña, no por toda la academia. No pasar de fase hasta cumplir los criterios de la actual.

## MVP (contenido mínimo viable) — checklist
- [ ] 2 jugadores conectados por LAN
- [ ] Recepción + una sala de ensayo
- [ ] Interacción con objetos, documentos y puertas
- [ ] Un puzle (espejos o símbolos)
- [ ] 1 carta, 1 fotografía, 1 anomalía
- [ ] Una llave como recompensa
- [ ] Una puerta que desbloquea la siguiente habitación
- [ ] Guardado básico
- **Criterio para ampliar**: se completa de principio a fin con dos jugadores y resulta comprensible y entretenido.

## Criterios de aceptación (antes de ampliar)
- [ ] Ambos jugadores entran en la misma partida
- [ ] Ambos exploran/interactúan sin roles exclusivos
- [ ] Puertas y mecanismos con estados coherentes
- [ ] Puzles completables sin quedar bloqueados
- [ ] Pistas importantes consultables de nuevo
- [ ] Recompensas no se duplican por errores de red
- [ ] Guardar y recuperar funciona
- [ ] Se entiende la historia principal sin explicaciones externas
- [ ] Eventos de terror no eliminan progreso injustamente

## Riesgos → medidas
| Riesgo | Medida |
|---|---|
| Multijugador complejo | Validar LAN desde el principio (fase 2 temprana, probar a diario con 2 instancias) |
| Demasiados sistemas | Reutilizar interacción básica; rechazar features fuera de fase |
| Puzles confusos | Playtest con personas nuevas, observar sin ayudar |
| Duración artificial | Secretos con significado; sin salas vacías |
| Anomalías vistas como bugs | Evento compartido vs. efecto local; feedback intencional |
| Pérdida de progreso | Guardar en puntos seguros; probar recuperación |

## Cómo trabajar una tarea
1. Identificar fase y requisito del GDD que cubre.
2. Implementar lo mínimo que lo cumple, reutilizando sistemas existentes.
3. Probar con 2 instancias (Multiplayer Play Mode o build + Editor).
4. Marcar el criterio de aceptación correspondiente.
5. Registrar problemas en `Docs/bugs.md` y decisiones en `Docs/decisiones.md`.

## Plantilla de playtest
```
Fecha / build:             Jugadores (nuevos / conocedores):
¿Entendieron el objetivo?  ¿Dónde se atascaron? (sala, puzle, min)
¿Usaron el Cuaderno?       ¿Alguna anomalía se tomó por bug?
Desync / errores de red:   Duración total:
¿Divertido? (1–5):         Cambios propuestos:
```

## Escenas
`MainMenu` (crear/unirse/ajustes/salir) · `Lobby` (2 jugadores) · `Academy_Prototype` · `Academy_Main` · `DisconnectRecovery` (solo si hace falta).
