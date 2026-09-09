# Gravity Receipt

Prototipo coop PC: la gravedad sigue al objeto más caro de la sala.

## Requisitos

- Unity **6000.6.0f1**
- Licencia **Personal** activada en Unity Hub

## Abrir y jugar (ahora)

1. Abre el proyecto `GravityReceipt` en Unity (ya debería estar abriéndose).
2. Abre la escena: `Assets/_Project/Scenes/Office_Floor_A.unity`
3. Si la escena no aparece o quieres regenerarla: menú **GravityReceipt → Setup Office Floor A**
4. Pulsa **Play**

### Controles
| Acción | Input |
|--------|--------|
| Mover | WASD |
| Mirar | Ratón |
| Saltar | Space |
| Agarrar | Mantener **E** o clic izq (~0.4 s) |
| Soltar | **E** o clic der |

### Qué probar
1. La caja fuerte dorada ($80) debe mandar la gravedad al inicio.
2. Agarra y mueve valuables: al soltar el más caro / el último en empate, verás telegráfo ~1 s y el flip.
3. Si caes al vacío, respawneas.

## Seguimiento
Ver [`PROGRESO_Gravity_Receipt.md`](PROGRESO_Gravity_Receipt.md)

## Estructura
```
GravityReceipt/Assets/_Project/
  Scripts/Gravity|Player|Interaction|Mission|UI|Editor
  Scenes/Office_Floor_A.unity
```
