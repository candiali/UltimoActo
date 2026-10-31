---
name: ultimo-acto-narrativa
description: Narrativa y contenido de Último Acto: estructura de capítulos, ScriptableObjects para documentos, coleccionables y secretos, tono, reglas de pistas y contradicciones. Úsalo al escribir documentos/cartas/diarios, definir coleccionables, o estructurar la historia y los capítulos.
---

# Narrativa y coleccionables

Antes: `ultimo-acto-contexto`. **No inventar la verdad final**: si hace falta, proponer 2–3 opciones y dejar que decida el usuario. Los protagonistas A y B están sin definir: consultar antes de darles nombre/pasado.

## Capítulos
| Cap. | Título | Espacios | Función |
|---|---|---|---|
| I | La invitación | Recepción, entrada | Controles, primeras llaves, interacción, primera anomalía |
| II | Las alumnas ausentes | Salones de ensayo, vestuarios | Objetos personales, primeras historias |
| III | La música olvidada | Archivo, sala musical | Contradicciones en registros, parte de la partitura |
| IV | El Cisne Negro | Escenario secundario, camerino prohibido | Más anomalías, conexiones entre personajes |
| V | El último acto | Gran teatro | Reconstruir la función, puzle final, verdad |

Cada capítulo: un objetivo claro, 1–2 puzles principales, varios opcionales, coleccionables propios, al menos una contradicción de testimonio y una anomalía distinta.

## Tono
Elegante, melancólico, inquietante. Documentos con voz propia (alumnas, Isadora, partituras anotadas, telegramas, críticas de prensa). Pistas sutiles; la contradicción entre fuentes alimenta el misterio ("la versión oficial dice accidente").

## Reglas de pistas
- Cada revelación importante se apoya en ≥2 fuentes (una puede contradecir a la otra).
- Cada documento: pista concreta o dato de personaje. Nada de relleno.
- Texto breve (≤120 palabras para cartas; diarios por fragmentos). Todo en el Cuaderno.
- Mantener un **registro de continuidad** (fechas, nombres, quién sabe qué) en `Docs/continuidad.md`.

## Datos (ScriptableObjects)
```csharp
public enum CollectibleKind { Key, Emblem, Letter, Diary, Photo, ScoreFragment, PersonalItem, Secret }

[CreateAssetMenu(menuName = "UltimoActo/Collectible")]
public class CollectibleData : ScriptableObject
{
    public string id;                 // "col_letter_mara_03" (estable, único)
    public CollectibleKind kind;
    public int chapter;
    public string title;
    [TextArea(3, 12)] public string body;
    public Sprite image;              // foto, partitura, objeto
    public string[] revealsStoryFlags;
    public bool optional = true;
    public bool isSecret;
}
```
- Una herramienta/script de editor valida: IDs únicos, sin campos vacíos, conteo por capítulo.
- Presupuesto: **30–40 coleccionables narrativos + 5–8 secretos**. Ajustar tras playtests.
- Secretos con **significado** (escena, recuerdo, descubrimiento), no solo "ítem escondido": evita duración artificial.
- Secretos en rutas ocultas (habitaciones que cambian, melodía de una sala fuera de plano) recompensan la exploración.

## Plantilla de documento
```
ID: col_xxx          Cap.: N       Tipo:
Autor / destinatario / fecha (dentro del mundo):
Texto:
Qué revela:
Qué contradice o confirma (IDs):
Dónde se encuentra:
```

## Comprensión de la historia
Criterio de aceptación: los jugadores entienden la historia principal **sin explicaciones externas**. Probar dándoles solo el juego; si no entienden qué pasó con Elena, falta una pista en el camino crítico (no en los opcionales).
