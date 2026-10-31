# Bitácora de Decisiones — Último Acto

## [2026-10-30] Fase 1 — Inicio de Prototipo Greybox
- **Motor y Render**: Unity 6 (6000.5.6f1) con Universal Render Pipeline (URP).
- **Sistema de Input**: Unity Input System Package con compatibilidad directa para teclado y ratón.
- **Estructura de Carpetas**: Se adopta el estándar `Assets/_Project/` para separar el código y prefabs propios de paquetes y dependencias del motor.
- **Sistema de Interacción**: Se implementa `IInteractable` y `PlayerInteraction` (Raycast centrado a 2.5m). En Fase 2 se integrará directamente con `NetworkInteractable` de Netcode for GameObjects.
- **Prototipo Greybox**: Implementación de figuras geométricas con paleta de materiales según el GDD (negro, marfil, vino tinto, latón dorado).
