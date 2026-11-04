# Bitácora de Decisiones — Último Acto

## [2026-10-30] Fase 1 — Inicio de Prototipo Greybox
- **Motor y Render**: Unity 6 (6000.5.6f1) con Universal Render Pipeline (URP).
- **Sistema de Input**: Unity Input System Package con compatibilidad directa para teclado y ratón.
- **Estructura de Carpetas**: Se adopta el estándar `Assets/_Project/` para separar el código y prefabs propios de paquetes y dependencias del motor.
- **Sistema de Interacción**: Se implementa `IInteractable` y `PlayerInteraction` (Raycast centrado a 2.5m). En Fase 2 se integrará directamente con `NetworkInteractable` de Netcode for GameObjects.
- **Prototipo Greybox**: Implementación de figuras geométricas con paleta de materiales según el GDD (negro, marfil, vino tinto, latón dorado).

## [2026-11-03] Sección 1 — Exterior y Entrada (SampleScene)
- **Escena Principal**: Se construye la Sección 1 en `SampleScene.unity`, conectando físicamente 5 áreas: Portón exterior, Patio de entrada, Escalinata monumental, Vestíbulo de recepción y Pasillo principal.
- **Escala Espacial**: Espacios amplios (más de 100 metros de recorrido lineal) con proporciones arquitectónicas de academia de ballet europea clásica.
- **Interacción Arquitectónica**: Implementación de `WorldDoubleDoor` para las puertas dobles monumentales de entrada y `WorldDocument` para el Libro de Registro de 1974 sobre el mostrador de recepción.
- **Atmósfera Lumínica**: Luz fría lunar cenital en exteriores complementada por luz cálida ámbar de faroles, apliques dorados y chandelier interior.
- **Priorización**: Se aplaza el multijugador para consolidar primero la escala y la inmersión de los escenarios de exploración.

## [2026-11-03] Optimización PBR, Física de Escaleras y Arquitectura Neoclásica
- **Física de Escaleras**: Integración de rampa invisible de precisión a -17.1° combinada con 12 peldaños sólidos y placa base anti-abismo `Bedrock_Safety_Ground` de 70m × 160m. Movimiento unificado de un solo paso en `PlayerMovement.cs` para evitar cualquier bloqueo o caída.
- **Texturizado y Relieves**: Materiales PBR con albedo en base blanca pura, normal maps generados y elementos arquitectónicos 3D: alfombra roja damasco con varillas doradas en cada escalón, paneles Boiserie en relieve, molduras de zócalo en caoba, cenefa de mármol negro y lámpara de araña deteriorada.
- **Auto-Rebuild en Editor**: Marcado `[InitializeOnLoad]` en `Section1SceneBuilder.cs` para garantizar que la escena se actualice automáticamente en el editor sin depender de ejecuciones manuales.

## [2026-11-03] Sección 2 — Recepción y Administración
- **Integración Arquitectónica Continua**: Se conecta el Vestíbulo de la Sección 1 (`Arch_Administration` a $X = -11\text{m}$, $Z = 20\text{m}$) con las 5 áreas de administración y se cierra el circuito circular desembocando de nuevo en el Pasillo Principal a $X = -4\text{m}$, $Z = 48\text{m}$.
- **Salto y Fluidez de Movimiento**: Se añade la acción de salto (`jumpHeight = 1.6m`) con barra espaciadora (`[Space]`) y se retira el muro de colisión residual `Stairs_Sub_Base` de la escalinata para garantizar ascenso suave tanto caminando como saltando.
- **Áreas Ambientadas y Narrativa Ambiental**:
  1. *Recepción Antigua*: Mostrador en L de caoba, timbre de bronce, teléfono de disco, reloj parado a las 20:15, calendario de 1924 y Libro de Visitas interactivo.
  2. *Oficina de Administración*: Gran escritorio de caoba con cartas confidenciales sin enviar, sillón de cuero capitoné, ventanal con cortinas borgoña y retrato al óleo de la directora Isadora Vale.
  3. *Archivo de Expedientes*: Estanterías metálicas altas, cajas de archivo desordenadas, estante volcado, mesa de examen y expedientes confidenciales interactivos de Elena Vesper y Mara Bell.
  4. *Sala de Espera*: Sillones de terciopelo verde esmeralda y borgoña, mesita baja con folletos de la academia, fotografía histórica de la compañía de 1923 con un espacio vacío inexplicable y una silla solitaria encarada a la pared.
  5. *Pasillo de los Retratos*: Galería clásica con molduras doradas, 8 retratos enmarcados de antiguas primeras bailarinas con placas de latón y cuadro central de *El Lago de las Sombras*.
- **Modal de Lectura de Documentos**: Interfaz UI en pantalla con fondo de viñeta oscura, tarjeta ornamental con marco dorado, tipografía nítida y cierre interactivo con `[E]` o `[Escape]`.
