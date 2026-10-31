---
name: ultimo-acto-arte-ui
description: Dirección de arte, sonido, interfaz y accesibilidad de Último Acto (academia de ballet deteriorada, paleta, iluminación, audio espacial, HUD discreto, subtítulos). Úsalo al crear materiales, iluminación, UI, menús, audio o ajustes de accesibilidad.
---

# Arte, sonido, UI y accesibilidad

Antes: `ultimo-acto-contexto`.

## Estética
Academia de ballet antigua, **elegante y deteriorada**.
- Paleta: negro, marfil, vino, dorado envejecido, tonos fríos de luz lunar.
- Elementos: espejos ornamentados, madera gastada, cortinas de terciopelo, vestuarios, máscaras, maniquíes, decorados teatrales.
- Contraste visual: calidez dorada/vino en interiores vs. azul frío de luna por ventanas.

## Iluminación (el 80% del miedo)
- Pipeline recomendado: URP. Iluminación mixta/horneada + pocas luces dinámicas (rendimiento en 2 instancias).
- Luces motivadas (lámparas, ventanas, velas), mucha penumbra pero **pistas siempre legibles**. Linterna/luz de apoyo si es necesaria; la oscuridad no puede ocultar solución de puzles.
- Niebla ligera, partículas de polvo en rayos de luz, reflejos en espejos (reflection probes / planar solo donde importa).
- Cambios de iluminación como parte de las anomalías (ver `ultimo-acto-anomalias`).

## Sonido
- Capas: ambiente (crujidos, viento) + melodía principal con **fragmentos incompletos** (se completa con la partitura) + pasos y sonidos 3D + silencios deliberados.
- Audio espacial 3D con oclusión simple; rolloff largo para melodías lejanas ("sala que no figura").
- Música adaptativa por capítulo: añadir/quitar capas, no reemplazar de golpe.
- Audio diegético importante en puzles = siempre con apoyo visual/subtítulo.

## Interfaz (discreta)
Elementos: indicador de interacción · inventario ligero · cuaderno de investigación · registro de coleccionables · menú de pausa y ajustes · subtítulos y accesibilidad.
- HUD mínimo: punto/icono de interacción y prompt contextual; sin barras ni contadores.
- Estilo UI: serif elegante, marfil sobre negro, acento dorado envejecido; papel/ilustración para el Cuaderno.
- **La pausa no pausa el juego en red** (el otro jugador sigue). Mostrar "Jugador X en el menú" si procede.
- Indicador del compañero (nombre/estado) discreto; opción de chat de voz externo (Discord) documentada: la cooperación depende de comunicación.

## Accesibilidad (requisito)
- Subtítulos para diálogos y para sonidos relevantes en puzles `[Suena una nota aguda]`; tamaño y fondo ajustables.
- Alternativa visual a señales sonoras; alternativa auditiva a señales visuales cuando el puzle lo permita.
- Remapeo de controles, sensibilidad de cámara, FOV, reducir balanceo de cámara (head bob), reducir/evitar parpadeos de luz.
- Opción intensidad de anomalías.
- Colores no como única señal (mirar daltonismo en símbolos de puzle).

## Rendimiento y escala
- Reutilizar módulos de sala (paredes, puertas, espejos) y variaciones de material.
- Habitaciones pequeñas y densas en detalles; sin espacios enormes vacíos.
- Cambio de aspecto de sala = variantes de decoración, no nueva geometría.
