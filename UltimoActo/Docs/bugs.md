# Registro de Bugs y Pruebas — Último Acto

## Estado Actual
- **Fase**: Fase 1 (Prototipo Core y Menú Principal)
- **Bugs Críticos**: 0
- **Bugs Menores**: 0

## Registro
| ID | Descripción | Estado | Solución |
|---|---|---|---|
| BUG-001 | `InvalidOperationException` al leer `Input.mousePosition` en `MenuCameraPan.cs` con el New Input System activo en Unity 6. | **Resuelto** | Se actualizó `MenuCameraPan.cs` para obtener las coordenadas del ratón a través de `Mouse.current.position.ReadValue()` bajo la directiva `#if ENABLE_INPUT_SYSTEM`. |
| BUG-002 | Los botones del Menú Principal no respondían a clics ni hovers de ratón en `MainMenu.unity`. | **Resuelto** | 1) Se añadió el GameObject `EventSystem` con el componente `InputSystemUIInputModule` indispensable para el New Input System. 2) Se desactivó `raycastTarget` en el gradiente de viñeteado y textos para evitar que bloqueen el rayo del ratón. 3) Se vincularon automáticamente los listeners de clic en el `Awake()` de `MainMenuController`. |
| BUG-003 | Error CS0103 en `MainMenuSceneBuilder.cs`: nombre de variable tipográfico (`howToPlayPanel` en lugar de `howToPanel`). | **Resuelto** | Corregido el nombre de la variable en la línea 330. Compilación limpia al 100%. |
