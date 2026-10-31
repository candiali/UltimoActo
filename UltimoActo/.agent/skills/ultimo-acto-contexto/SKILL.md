---
name: ultimo-acto-contexto
description: Contexto base y reglas de diseño del juego "Último Acto" (cooperativo LAN para 2 jugadores en Unity, terror psicológico en una academia de ballet). Úsalo SIEMPRE antes de proponer o escribir código, escenas, puzles o contenido para este proyecto, y para decidir si algo entra o no en el alcance.
---

# Último Acto — Contexto y reglas del proyecto

## Qué es
Aventura cooperativa de 2 jugadores (LAN, Unity, primera persona) en la abandonada Academia del Cisne. Investigar, resolver puzles de ballet y reconstruir recuerdos. Terror psicológico por atmósfera, NO por monstruos ni combate.

## Historia (resumen para coherencia)
- Academia dirigida por **Isadora Vale**. Obra: *El Lago de las Sombras*.
- **Elena Vesper** (Cisne Negro) y **Mara Bell** (Cisne Blanco).
- Noche del estreno: la música se detuvo antes del último acto, golpe tras el telón, Elena desapareció. Versión oficial: accidente.
- Hoy: dos protagonistas (A y B, sin identidad definida aún, **sin habilidades exclusivas**) reciben una invitación anónima.
- Verdad definitiva solo al completar el último acto. No revelar antes de tiempo ni inventar la verdad final sin consultar al usuario.

## Pilares (todo diseño debe servir al menos a uno)
Misterio · Exploración · Cooperación · Puzles · Terror psicológico · Progresión.

## Reglas duras (no romper)
1. Ambos jugadores tienen **exactamente las mismas acciones**. Nada de clases/roles. La cooperación nace de la situación (posición, información distinta temporal).
2. **Progreso compartido**: puertas, mecanismos, objetos clave y puzles tienen el mismo estado para ambos. Solo los *efectos visuales/sonoros* de anomalías pueden diferir por jugador.
3. Todo puzle importante: solución **deducible** con las pistas del juego y **recuperable** en el Cuaderno. Nunca depende de que un jugador posea algo permanente.
4. Sin combate complejo. Persecuciones = opcionales, puntuales.
5. Los eventos de terror **nunca** borran progreso injustamente.
6. Recompensas **nunca** se duplican (idempotencia ante fallos de red).
7. Alcance: reutilizar sistemas; no construir salas enormes vacías.

## Alcance de contenido
- 5 capítulos: I La invitación · II Las alumnas ausentes · III La música olvidada · IV El Cisne Negro · V El último acto.
- 5 áreas principales + ~15–20 espacios secundarios.
- 30–40 coleccionables narrativos, 5–8 secretos especiales.
- Prioridades mecánicas — Esenciales: movimiento 1ª persona, interacción, inventario ligero, cuaderno compartido, puertas/llaves, puzles ambientales, coleccionables, anomalías. Opcional: persecuciones. Ampliación: restauración visual de salas.

## Stack y convenciones
- Unity (usar la versión del proyecto), C#, **Netcode for GameObjects + Unity Transport** (LAN, host = jugador anfitrión con autoridad).
- Idioma del código: identificadores en inglés; textos de juego, comentarios y docs en español.
- Datos de contenido en ScriptableObjects (ver skill `ultimo-acto-narrativa`).
- Escenas: `MainMenu`, `Lobby`, `Academy_Prototype`, `Academy_Main`, `DisconnectRecovery` (solo si hace falta).

## Cómo decidir
Ante cualquier tarea: (1) ¿respeta las reglas duras? (2) ¿está en el MVP o fase actual (ver `ultimo-acto-qa-roadmap`)? (3) ¿reutiliza un sistema existente? Si la respuesta a (2) es no, proponlo como "fuera de alcance por ahora" en vez de implementarlo.

## Skills relacionadas
`ultimo-acto-red-lan`, `ultimo-acto-interaccion`, `ultimo-acto-puzles`, `ultimo-acto-anomalias`, `ultimo-acto-guardado`, `ultimo-acto-narrativa`, `ultimo-acto-arte-ui`, `ultimo-acto-qa-roadmap`.
