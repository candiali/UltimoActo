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
