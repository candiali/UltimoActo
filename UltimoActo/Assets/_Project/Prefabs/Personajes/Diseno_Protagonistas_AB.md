# Diseño de Personajes — Protagonistas A y B (*Último Acto*)

Documento de referencia para modelado 3D, texturizado, rigging y prefabs dentro de `Assets/_Project/Prefabs/Personajes/`.

---

## 1. Concepto Narrativo y Estética Compartida

- **Perfil:** Dos compañeras de escuela/universidad (~19–21 años), amigas cercanas con afición por lo paranormal, las leyendas urbanas y los misterios sin resolver.
- **Estilo visual:** **Dark Academia** clásico con toques de investigación de campo nocturna (años 90 / atemporal analógico, sin tecnología moderna intrusiva).
- **Regla de diseño cooperativo:** Como ambas tienen exactamente las mismas mecánicas y habilidades en el juego, deben **distinguirse de inmediato en salas oscuras** mediante:
  1. **Silueta:** Abrigo largo (A) vs. chaqueta corta + bufanda/boina (B).
  2. **Acento de color dentro de la paleta:** Carbón/Negro + Dorado envejecido (A) vs. Vino/Borgoña + Marfil (B).
  3. **Accesorios de investigación:** Libreta de campo y linterna de latón (A) vs. cámara analógica/grabadora EVP (B).

---

## 2. Protagonista A — *"La Analítica"* (Quien recibe la invitación)

### Personalidad visual
Metódica, observadora, siempre lleva apuntes y herramientas básicas para documentar pistas.

### Vestuario y detalles de modelado
- **Cabello y rostro:** Cabello oscuro ondulado recogido en un moño bajo algo despeinado, con mechones sueltos enmarcando el rostro. **Gafas redondas de montura metálica fina** (dorado envejecido).
- **Prenda exterior:** Gabardina / abrigo largo de lana **negro carbón** (`#1D1E22`), con solapas anchas y botones de latón oscuro. Llega por debajo de la rodilla.
- **Prendas interiores:** Suéter de cuello alto **marfil** (`#E6DEC8`), falda plisada gris oscuro/carbón, medias opacas negras y zapatos tipo **Oxford de cuero gastado** (`#261B16`).
- **Accesorios paranormales (clave para silueta):**
  - **Bandolera de cuero oscuro** cruzada al pecho con hebillas en **dorado envejecido** (`#B39149`).
  - **Linterna vintage de latón** enganchada a la correa de la bandolera.
  - **Libreta de cuero** asomando del bolsillo del abrigo o sujeta en el costado.

---

## 3. Protagonista B — *"La Curiosa"* (Segunda investigadora)

### Personalidad visual
Más impulsiva y valiente, amante de las leyendas de la Academia del Cisne y de capturar evidencia visual/sonora.

### Vestuario y detalles de modelado
- **Cabello y rostro:** Cabello castaño oscuro a la altura de los hombros con flequillo ligero. **Boina oscura** (`#2A1419` o negro) ligeramente ladeada y horquillas metálicas.
- **Prenda exterior:** Chaqueta académica estructurada (estilo tweed/terciopelo) en tono **vino oscuro** (`#4E1621`) con botones oscuros y coderas discretas.
- **Prendas interiores:** Blusa **marfil envejecido** (`#EAE3D2`) con cuello clásico y lazo/cinta negra al cuello, pantalón de pinzas oscuro de tiro alto y botines de cuero acordonados.
- **Accesorios paranormales (clave para silueta):**
  - **Bufanda tejida marfil** rodeando el cuello (rompe la silueta del torso y la hace reconocible de espaldas).
  - **Cámara fotográfica analógica** o **grabadora de casetes (EVP)** colgada a la cadera con correa de cuero.
  - **Guantes sin dedos** (mitones) tejidos en tono oscuro.

---

## 4. Especificaciones Técnicas para Blender y Unity (URP)

### Escala y compatibilidad con `Section1SceneBuilder`
- **Altura total del modelo:** `1.70 m – 1.74 m` (encaja dentro del `CharacterController` configurado a `height = 1.8f`, `radius = 0.4f`, `center = (0, 0.9, 0)`).
- **Altura de ojos / cámara FPS:** `1.60 m` sobre el suelo (`Y = 1.60f`), coincidiendo exactamente con `FirstPersonCamera`.
- **Pivote (Origin):** En `(0, 0, 0)` justo entre los pies, mirando hacia `+Z` (eje frontal de Unity).

### Presupuesto de geometría y texturas
- **Triángulos:** `18,000 – 28,000 tris` por personaje (suficiente para pliegues de abrigo, gafas, bufanda y manos expresivas).
- **Materiales URP (`Universal Render Pipeline/Lit`):**
  - `2K` para cuerpo/ropa principal, `1K` para accesorios pequeños.
  - Mapas recomendados: `BaseMap` (Albedo), `NormalMap`, `Metallic/Smoothness` (muy útil para diferenciar tela mate de cuero, gafas y latón dorado).

### Rigging y Primera Persona + Espejos
- Configurar el `.fbx` en Unity como **Animation Type: Humanoid** para compartir animaciones entre Protagonista A, Protagonista B y futuros NPCs/maniquíes.
- **Separación de cabeza para FPS:** Mantener la malla de la cabeza/cabello/gafas/boina en un subobjeto separado (`HeadGroup`) o con un material/capa independiente:
  - El jugador local oculta su propia `HeadGroup` a su cámara principal (para no ver el interior de la cabeza), pero la mantiene visible para **sombras** y **reflejos en espejos** (puzles y anomalías).
  - El compañero en red LAN ve el modelo completo al 100%.

---

## 5. Prefabs Base en Unity

Puedes generar los prefabs prototipo (con proporciones reales, materiales URP y accesorios diferenciados) desde el menú de Unity:
`Tools > Último Acto > Personajes > Generar Prefabs Protagonistas A y B`

Esto creará:
- `Assets/_Project/Prefabs/Personajes/PF_Protagonista_A.prefab`
- `Assets/_Project/Prefabs/Personajes/PF_Protagonista_B.prefab`

Cuando tengas los modelos `.fbx` definitivos de Blender, solo debes reemplazar las mallas dentro de `VisualRoot` manteniendo la misma escala y jerarquía.
