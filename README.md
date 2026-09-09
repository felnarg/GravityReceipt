# Gravity Receipt

Prototipo coop PC: la gravedad sigue al objeto más caro de **la sala**.

## Requisitos

- Unity **6000.6.0f1**
- Licencia **Personal** activada en Unity Hub

## Abrir y jugar (obligatorio regenerar escena)

La escena commiteada puede estar **desactualizada**. Al pulsar **Play**, si no hay `MatchDirector` el piso se reconstruye solo (2p). Aun así, lo limpio es:

1. Abre el proyecto `GravityReceipt` en Unity.
2. Menú **GravityReceipt → Setup Office Floor A** (2 jugadores split) y guarda.  
   1P: **GravityReceipt → Setup Office Floor A (1 jugador)**
3. Pulsa **Play**

### Controles

| Acción | P1 (mitad superior, azul) | P2 (mitad inferior, naranja) |
|--------|---------------------------|------------------------------|
| Mover | WASD | Flechas o pad 1 |
| Mirar | Ratón | Numpad 4/6 (yaw) 8/5 (pitch) **con NumLock**, U/O Y/H, o stick der. |
| Saltar | Space | Right Ctrl / Keypad Enter / botón A |
| Agarrar (0.4 s) | Mantener **E** o clic izq | Right Shift / Keypad . / botón B |
| Soltar | **E** o clic der | Right Shift |
| Ping | **Q** | **/** |
| Sprint (Runner) | Left Shift | Right Alt |
| Ancla g 3 s (Anchor) | **F** | Keypad 0 |
| Cambiar rol | Tab | Keypad 7 |
| Rematch | **R** (al ganar/perder) | **R** |
| Liberar ratón | Escape | — |

### Qué probar (loop corto)

1. Cartel del Hub: **LA GRAVEDAD SIGUE LO MÁS CARO**.
2. Agarra el paquete **naranja**. En Archive, llévalo a la zona verde **ENCHUFA EL PAQUETE** (pared este).
3. Caja fuerte **dorada $80**: llévala cerca de una **pared** y suéltala. Tras ~1 s la gravedad de Archive tira hacia esa pared.
4. Cruza el pasillo (vacío a los lados). Si caes, respawneas en el último checkpoint.
5. Open Office: deja el paquete en la losa azul **ENTREGAR**.
6. Executive: paquete + jugador en la losa dorada, mantén agarrar para **SELLAR**.
7. Timer 10:00. 3 destrucciones del paquete (impactos fuertes) = derrota. **R** recarga la escena.

## Seguimiento
Ver [`PROGRESO_Gravity_Receipt.md`](PROGRESO_Gravity_Receipt.md)

## Estructura
```
GravityReceipt/Assets/_Project/
  Scripts/Gravity|Player|Interaction|Mission|UI|World|Editor
  Scenes/Office_Floor_A.unity
```
