# Gravity Receipt

Coop caótico de oficina: **llevas un paquete naranja a 3 zonas** antes de que se destruya o se acabe el tiempo. El twist es que **el objeto con más `$` de cada sala voltea la gravedad** hacia donde lo dejes.

La gracia no es leer carteles. Es que un compañero lleva el paquete por el pasillo y el otro pone la caja `$80` en una pared: la sala se cae de lado, el paquete vuela, y sale el clip.

## Cómo se gana (una frase)

Paquete naranja → zona **VERDE** → losa **AZUL** → losa **DORADA** (mantén E). El `$` más caro es la herramienta (y el caos), no el objetivo.

## Arte (gratis, CC0)

El piso ya no es solo cubos grises:

- **Muebles 3D** (sillas, escritorios, estanterías, sofá, plantas, lámparas): [Kenney Furniture Kit](https://kenney.nl/assets/furniture-kit) (CC0). Se cargan en Play desde `Assets/StreamingAssets/Kenney/` (no hace falta importar FBX en el Editor).
- **Suelos y paredes** (madera, alfombra, yeso): [ambientCG](https://ambientcg.com) WoodFloor051 / Carpet008 / Plaster001 (CC0), en `StreamingAssets/Textures/`.

Tras **Setup Office Floor A**, Archive se lee de madera, Hub/Office de alfombra, Executive de madera + sofá. El paquete y los `$` siguen siendo siluetas propias (tienen que leerse al vuelo).

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
| Ocultar chrome HUD | **F9** (también etiquetas 3D) | **F9** |
| Pausa | **P** | **P** |
| Comfort (menos mareo) | **F10** | **F10** |
| Unstuck (cheat) | **F3** | **F3** |
| Warp checkpoint (cheat) | **F4** | **F4** |
| Respawn paquete (cheat) | **F7** | **F7** |

Cheat de iteración (no sale en el HUD): **F3** desatasca · **F4** teleporta al checkpoint (salta el splash) · **F6** completa el siguiente objetivo y teleporta (salta el splash) · **F7** respawnea el paquete · **F10** comfort (menos shake/FOV).

### Qué probar (loop corto)

1. Play. Splash corto: **paquete a 3 zonas** + **el $ voltea la sala**. La barra de arriba dice `PAQUETE → zona VERDE`. La flecha apunta al paquete naranja (orbe).
2. Agarra el paquete (**E**). Llévalo a Archive, pared este, cubo **verde** que pulsa (`AQUÍ`).
3. (Opcional, el caos) Caja **`$80`**: llévala a una **pared**. Espera ~1 s → la sala voltea. Pad cian = nueva ABAJO. El paquete sigue siendo la misión.
4. Pasillo (vacío a los lados) → losa **azul** → losa **dorada** (mantén E).
5. **P** pausa (ahí están los controles). **F10** si marea. **R** rematch.

Cubos **grises** no tienen `$` y no voltean nada. Cada sala tiene su propio `$` máximo.

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
