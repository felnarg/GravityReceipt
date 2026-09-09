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
| Mirar | Ratón | J/L (yaw) + I/K o Y/H (pitch), Numpad 4/6/8/5 **con NumLock**, U/O, o stick der. |
| Saltar | Space | Right Ctrl / Keypad Enter / botón A |
| Agarrar (0.4 s) | Mantener **E** o clic izq | Right Shift / Keypad . / botón B |
| Soltar | **E** o clic der | Right Shift |
| Emote (OK / NO / ? / ¡AQUÍ!) | **1 2 3 4** | Keypad **1 2 3 9** |
| Ping | **Q** | **/** |
| Sprint (Runner) | Left Shift | Right Alt |
| Ancla g 3 s (Anchor) | **F** | Keypad 0 |
| Cambiar rol | Tab | Keypad 7 |
| Rematch | **R** al terminar / **F5** siempre | igual |
| Screenshot PNG | **F8** | **F8** |
| Ocultar chrome HUD | **F9** | **F9** |
| Pausa | **P** | **P** |
| Unstuck (cheat) | **F3** | **F3** |
| Warp checkpoint (cheat) | **F4** | **F4** |
| Respawn paquete (cheat) | **F7** | **F7** |

Cheat de iteración (no sale en el HUD): **F3** desatasca · **F4** teleporta al checkpoint · **F6** completa el siguiente objetivo y teleporta · **F7** respawnea el paquete.

### Qué probar (loop corto)

1. Cartel del Hub: **LA GRAVEDAD SIGUE LO MÁS CARO**. Cubos grises **no** tienen `$` (no tiran de g). Taza `$15` sí. El paquete lleva un orbe naranja encima.
2. Agarra el paquete **naranja**. Sigue la flecha a los pies / columna del objetivo. En Archive, llévalo a la zona verde **ENCHUFA EL PAQUETE** (pared este).
3. Caja fuerte **dorada $80**: llévala a una **pared** (puedes **mantenerla en las manos**). Tras ~1 s la gravedad de Archive tira hacia esa pared. Si la sueltas, espera a que se detenga.
4. Cruza el pasillo (vacío a los lados; bordillos rojos). Si caes, respawneas en el último checkpoint.
5. Open Office: deja el paquete en la losa azul **ENTREGAR** (también cuenta si lo llevas en las manos sobre la losa).
6. Executive: paquete + jugador en la losa dorada. Mantén **E** **o** quédate encima con el paquete agarrado = **SELLAR**.
7. Timer 10:00 (no baja durante el splash de 9 s). 3 destrucciones del paquete (impactos fuertes) = derrota. **R** recarga la escena.
8. **Gravedad por sala:** Archive puede estar “de lado” mientras Office sigue normal.

### Si algo falla
- **No hay split / mapa viejo:** menú Setup Office Floor A y guarda la escena.
- **P2 no mira con numpad:** NumLock ON, o usa J/L + I/K.
- **P2 no aparece:** Setup 2p (no el ítem “1 jugador”).
- **Caes al vacío andando:** los bordillos son de 0.58 m; hay que saltar o un flip. **F5** si te atascas.
- **Quieres saltar objetivos:** **F6** (cheat).
- **Te atascaste en un flip:** **F3** o **P** para mirar.

## Seguimiento
Ver [`PROGRESO_Gravity_Receipt.md`](PROGRESO_Gravity_Receipt.md)

## Estructura
```
GravityReceipt/Assets/_Project/
  Scripts/Gravity|Player|Interaction|Mission|UI|World|Editor
  Scenes/Office_Floor_A.unity
```
