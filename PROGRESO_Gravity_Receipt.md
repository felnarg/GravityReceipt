# Gravity Receipt — Seguimiento paso a paso

Documento vivo del prototipo.  
**Úsalo siempre** para saber: qué hay ahora, qué se está haciendo y qué falta.

---

## Cómo leer este archivo

| Marcador | Significado |
|----------|-------------|
| `[ ]` | Pendiente |
| `[~]` | En curso |
| `[x]` | Hecho |
| `[!]` | Bloqueado (anotar motivo en Notas) |
| `[–]` | Descartado / fuera de scope |

**Regla del agente:** al terminar cada tarea, actualizar este archivo en la misma sesión (marcar `[x]`, mover el foco en “Estado actual” y anotar la fecha).

**Regla del humano:** si cambias de prioridad o pruebas algo fuera del plan, anótalo en “Registro de cambios” para no perder el hilo.

---

## Estado actual (actualizar siempre)

| Campo | Valor |
|-------|--------|
| **Fase** | 1–2 proto cerrado en código + 3–6 mapa offline + pulido de feel/regla |
| **Semana del plan** | 3–6 (código listo; falta playtest humano) |
| **Última actualización** | 2026-09-09 ~00:00 COT (sesión overnight Cloud Agent, bloque 5) |
| **En curso ahora** | Playtest humano 2p + vídeo del flip (1.10 / 1.11 / 2.7) |
| **Hecho relevante** | Split 2p, gravedad por sala, misión, mapa Hub→Executive, rematch, HUD de regla, flip feel, curbs del pasillo, valuables vuelven a casa |
| **Siguiente acción concreta** | Unity: GravityReceipt → Setup Office Floor A → Play → enchufar paquete + flip con caja dorada a una pared |
| **Build jugable** | Sí (Editor Play Mode, 1p o 2p local). **Hay que regenerar la escena con el menú Setup.** |
| **Online 4p** | No (no empezar hasta que el playtest offline sea sólido) |
| **Bloqueadores** | Este entorno Linux no tiene Unity Editor: no se pudo Play Mode ni grabar el vídeo 1.11 |

---

## Cierre de sesión (2026-09-09 overnight)

### HECHO
- Input por jugador + split-screen 2p (P1 WASD+ratón, P2 flechas+numpad/pad; P2 también I/K para pitch).
- `Grabbable` exclusivo (un holder a la vez) + drop al Warp/disable.
- Gravedad **por sala** (`RoomVolume` + `GravityBody`; `Physics.gravity = 0`).
- `MissionPackage` (3 vidas / 3 destrucciones), 3 objetivos, checkpoints, timer 10:00.
- Roles stub Runner (sprint) / Anchor (fija g 3 s). Tab / KP7 para cambiar.
- Mapa: Hub → Archive → Pasillo con vacío → Open Office → Executive.
- Tutorial en pared del Hub + carteles de pista (caja a la pared, entregar, sellar).
- HUD: splash 9 s de la regla, banner **¡FLIP!** durante telegráfo, rematch con **R**.
- Moment of the Match local (mayor caída).
- Feel de flip: FOV punch + shake + whoosh procedural.
- Dominante: pulso + etiqueta **¡ESTE TIRA DE G!**.
- Pasillo: bordillos 0.58 m (ya no se camina al vacío sin querer).
- Valuables que caen al vacío vuelven a su spawn.
- Losas Entregar/Sellar cubren el paquete agarrado; Sellar también si llevas el paquete.
- Menú Setup 2p (default) y 1p.

### A MEDIAS
- Escena `Office_Floor_A.unity` **commiteada sigue siendo la Archive v1**. En Play, si no hay `MatchDirector`, el factory reconstruye el piso 2p automáticamente. Para guardarla: menú Setup.
- 1.10 / 2.7 playtest: código listo, **cero playtests reales** (no hay Unity aquí).
- Outline dominante sigue siendo pulso de escala + tint, no un outline URP de verdad.
- Whoosh de flip listo; emotes (5.7) no.

### FALTA
- 1.11 Vídeo mudo 8–10 s del flip (grabar en Unity local).
- Playtest 2 personas: comprensión ≤30 s, OOB, pasillo, 3 objetivos, rematch.
- Ajustes de feel según ese playtest (telegráfo, mareo, tamaño de puertas, catwalk).
- Sem 7–8 online: **no tocar** hasta que el offline esté sólido.

### Cómo probar mañana en Unity local (pasos exactos)

1. Abre Unity Hub → proyecto **GravityReceipt** con Editor **6000.6.0f1**.
2. Espera a que compile (scripts nuevos en `Assets/_Project/Scripts/...`).
3. Opción A (recomendada): menú **GravityReceipt → Setup Office Floor A** → OK.  
   Opción B: pulsa **Play** directo; si la escena es la vieja, se reconstruye sola (2p).
4. Pulsa **Play**. Debes ver split: P1 arriba (cápsula azul), P2 abajo (naranja). Los valuables muestran `$` encima. Splash 9 s: “LA GRAVEDAD SIGUE AL OBJETO MÁS CARO”.
5. **P1:** WASD + ratón. Agarra el cubo naranja (mantener E ~0.4 s).
6. Entra a Archive (norte). Zona verde en la pared este: paquete dentro → objetivo Enchufar.
7. Agarra la **caja dorada** (etiqueta ¡ESTE TIRA DE G! si es la dominante) y déjala pegada a una **pared**. Espera ~1 s: banner **¡FLIP!**, whoosh, FOV, g cambia.
8. Cruza el pasillo (bordillos rojos altos; vacío a los lados). Si caes, respawneas. Si un valuable cae, vuelve a su sitio.
9. Losa azul Open Office = Entregar. Losa dorada Executive + paquete en manos o mantener E = Sellar.
10. Al ganar/perder: pulsa **R**. Debería recargar en <15 s. **F5** reinicia siempre.
11. (Opcional) **GravityReceipt → Setup Office Floor A (1 jugador)** para probar solo.
12. Para 1.11: **F8** captura un PNG (carpeta del proyecto en Editor). Graba 8–10 s mudos del paso 7, o dispara F8 durante el FLIP.

---

## Resumen del juego (referencia rápida)

- **Nombre:** Gravity Receipt  
- **Género:** Coop party caótico (PC / Steam)  
- **Hook:** la gravedad sigue al objeto más caro de la sala  
- **Jugadores MVP:** 4 · **Partida:** 8–12 min  
- **Motor:** Unity 6 (URP) · **Netcode (más adelante):** Fish-Net o Photon Fusion  
- **Mapa MVP:** `Office_Floor_A` (Hub → Archive → Pasillo → Open Office → Executive)

### Pilares
1. Gravedad = objeto de valor  
2. Caos cooperativo  
3. El fallo es el contenido (clips)

### Win / Lose
- **Ganar:** completar 3 objetivos antes del timer (~10:00)  
- **Perder:** timer a 0 **o** el paquete se destruye 3 veces  

---

## Mapa mental del progreso

```
[Fase 0 Doc] → [Sem 1–2 Proto regla] → [Sem 3–4 Feel + Obj1]
        → [Sem 5–6 Mapa offline] → [Sem 7–8 Online 4p]
        → [Sem 9–10 Playtests] → [Sem 11–12 Vertical slice]
```

---

# FASE 0 — Documentación y repo

| ID | Tarea | Estado | Notas |
|----|--------|--------|-------|
| 0.1 | Acordar concepto y GDD | [x] | Gravity Receipt elegido |
| 0.2 | Crear `PROGRESO_Gravity_Receipt.md` | [x] | Este archivo |
| 0.3 | Autorización para editar/crear proyecto | [x] | Usuario 2026-09-08 |
| 0.4 | Inicializar git (si se pide) | [ ] | Solo si el usuario lo solicita |
| 0.5a | Instalar Unity Hub | [x] | winget `Unity.UnityHub` 3.21.1 |
| 0.5b | Instalar Unity Editor 6000.6.0f1 | [x] | winget `Unity.Unity.6000` → `C:\Program Files\Unity 6000.6.0f1` |
| 0.5c | Activar licencia Personal (login Hub) | [x] | Usuario confirmó |
| 0.5d | Abrir/validar proyecto en Editor | [x] | ProjectSettings reparados desde proyecto fresco |
| 0.6 | Estructura de carpetas del repo | [x] | `Assets/_Project/...` creada |
| 0.7 | Escena `Office_Floor_A` (blockout vacío) | [x] | Regenerar con Setup (mapa completo en factory) |
| 0.8 | README corto del proyecto | [x] | `README.md` en raíz |
| 0.9 | Scripts stub gravedad / player / grab | [x] | Sistemas de misión + World factory |

---

# SEMANAS 1–2 — Proto: una sala, una regla

**Objetivo:** 2 personas entienden la regla en ≤30 s sin explicación hablada.

| ID | Tarea | Estado | Notas |
|----|--------|--------|-------|
| 1.1 | Player: move / look / jump | [x] | `PlayerMotor` + `LocalPlayerInput` |
| 1.2 | Agarrar / soltar props | [x] | Wind-up 0.4 s + `Grabbable` |
| 1.3 | `ValuableItem` con precio `$` | [x] | |
| 1.4 | `GravityManager` por sala | [x] | Ya no es global: un manager por sala |
| 1.5 | Aplicar vector de gravedad | [x] | Snap a ejes; `GravityBody` por rigidbody |
| 1.6 | Telegráfo 1.0 s antes del flip | [x] | HUD + flecha 3D en la sala |
| 1.7 | Inercia corta al cambiar g | [x] | Reset parcial de velocidad + ground raycast |
| 1.8 | Outline del valuable dominante | [x] | Pulso/color (no outline URP) |
| 1.9 | Escena mínima: 1 sala + 2 valuables | [x] | Archive sigue existiendo dentro del piso |
| 1.10 | Playtest local 2p (misma máquina o builds) | [~] | Split-screen implementado; falta que 2 personas lo jueguen |
| 1.11 | Vídeo mudo 8–10 s del flip | [~] | F8 screenshot + auto PNG en el **primer** flip; falta vídeo humano |

**Gate 1–2:** si no se entiende en 30 s → simplificar UI, no añadir features.

---

# SEMANAS 3–4 — Feel + objetivo 1

**Objetivo:** completar Sala A en coop local sin tutorial hablado.

| ID | Tarea | Estado | Notas |
|----|--------|--------|-------|
| 2.1 | `MissionPackage` (abolladuras / 3 vidas) | [x] | 3 dents → destrucción; 3 destrucciones = lose |
| 2.2 | Objetivo “enchufar” en Sala A | [x] | Zona verde pared este Archive |
| 2.3 | Checkpoint al completar objetivo | [x] | `CheckpointSystem` avanza spawn |
| 2.4 | Grab wind-up 0.4 s | [x] | Ya en 1.2 |
| 2.5 | Ping radial (“¡no toques eso!”) | [x] | Q / slash |
| 2.6 | Tutorial de 1 frase en pared del hub | [x] | “LA GRAVEDAD SIGUE LO MÁS CARO” |
| 2.7 | Playtest Sala A completa | [ ] | Humano; no se pudo en el agente |

**Gate 3–4:** run de Archive estable y divertida.

---

# SEMANAS 5–6 — Mapa completo offline

**Objetivo:** 3 partidas seguidas con rematch voluntario.

| ID | Tarea | Estado | Notas |
|----|--------|--------|-------|
| 3.1 | Blockout Pasillo + vacío lateral | [x] | Catwalk 3.6 m, bordillos 0.58 m, sin paredes laterales |
| 3.2 | Blockout Sala B Open Office | [x] | Monitor $120, Planta $60, Cafetera $90 |
| 3.3 | Blockout Sala C Executive | [x] | Maletín $200, Trofeo $150, Server $110, Planta oro $95 |
| 3.4 | Objetivo 2: entregar paquete | [x] | Losa azul Open Office |
| 3.5 | Objetivo 3: sellar contrato | [x] | Losa dorada + hold E ~1 s |
| 3.6 | Timer de partida ~10:00 | [x] | `MatchDirector` 600 s |
| 3.7 | Roles stub: Runner (sprint) / Anchor (fix 3 s) | [x] | P1 Runner / P2 Anchor por defecto |
| 3.8 | Muerte por vacío + respawn en checkpoint | [x] | `VoidKillZone` multi-jugador + paquete |
| 3.9 | Moment of the Match (mayor caída) | [x] | Local; se muestra al terminar |
| 3.10 | Flujo rematch <15 s | [x] | R recarga la escena activa |

**Gate 5–6:** rematch voluntario en playtests internos.

---

# SEMANAS 7–8 — Online 4 jugadores

**Objetivo:** 4 jugadores terminan una run sin desync grave de gravedad.

| ID | Tarea | Estado | Notas |
|----|--------|--------|-------|
| 4.1 | Elegir netcode (Fish-Net **o** Photon Fusion) | [ ] | No cambiar a mitad |
| 4.2 | Lobby host / join | [ ] | |
| 4.3 | Sync posiciones jugadores | [ ] | |
| 4.4 | Ownership de props / grabs | [ ] | |
| 4.5 | Host-authoritative: DominantValuable + GravityVector | [ ] | Clientes no deciden g |
| 4.6 | Test 2 PCs / LAN | [ ] | |
| 4.7 | Test 4 jugadores | [ ] | |
| 4.8 | Grabar 5 clips; elegir 1 “héroe” | [ ] | |

**Gate 7–8:** run online completa sin softlock/desync P0.

---

# SEMANAS 9–10 — Juicio de viralidad

**Objetivo:** decidir go / no-go con datos.

| ID | Tarea | Estado | Notas |
|----|--------|--------|-------|
| 5.1 | Playtests con 8–12 grupos externos | [ ] | |
| 5.2 | Medir rematch % | [ ] | Meta ≥ 55% |
| 5.3 | Medir mareo % | [ ] | Meta < 10% |
| 5.4 | Medir comprensión de la regla | [ ] | Meta ≥ 80% en 1ª partida |
| 5.5 | Ajustar telegráfo / FOV / velocidad de flip | [~] | FOV punch + shake + banner FLIP; falta ajustar con mareo real |
| 5.6 | Pass siluetas/colores valuables | [~] | Primitivas distintas (caja/cápsula/cilindro) + masa por precio; falta arte |
| 5.7 | Emotes (4) + whoosh final | [~] | Whoosh procedural en cada flip; emotes no |
| 5.8 | Decisión go / no-go | [ ] | Ver métricas abajo |

**Gate go:** rematch ≥ 55% **o** ≥ 40% de grupos envían clip solos.  
**Si no:** recortar scope / retocar feel; no añadir sistemas nuevos.

---

# SEMANAS 11–12 — Vertical slice vendible

**Objetivo:** build privada + vídeo que vende el juego en silencio.

| ID | Tarea | Estado | Notas |
|----|--------|--------|-------|
| 6.1 | Pass de arte (no final, salir de gris total) | [ ] | |
| 6.2 | Pulido UI mínima | [~] | Splash regla, banner FLIP, toast de objetivo |
| 6.3 | Build Steam o itch privada | [ ] | |
| 6.4 | Trailer 15–20 s del mejor clip | [ ] | |
| 6.5 | Lista bugs P0 cerrada | [ ] | Gravedad, softlock, desync |
| 6.6 | Steam page (cuando el clip mudo funcione) | [ ] | No antes del gate 9–10 |

---

## Fuera de scope (no hacer hasta después del gate 9–10)

| Idea | Estado |
|------|--------|
| PvP 2v2 oficinas rivales | [–] |
| Battle pass / seasons | [–] |
| Voice chat nativo | [–] |
| Más de 1 mapa | [–] |
| Cosméticos complejos | [–] |
| Editor de niveles | [–] |
| Ranked | [–] |

---

## Estructura de carpetas prevista (Unity)

```
Juego/
├── PROGRESO_Gravity_Receipt.md    ← este archivo
├── README.md
└── GravityReceipt/                ← proyecto Unity
    └── Assets/
        ├── _Project/
        │   ├── Art/
        │   ├── Audio/
        │   ├── Prefabs/
        │   ├── Scenes/
        │   │   └── Office_Floor_A.unity
        │   ├── ScriptableObjects/
        │   ├── Scripts/
        │   │   ├── Gravity/
        │   │   ├── Player/
        │   │   ├── Interaction/
        │   │   ├── Mission/
        │   │   ├── UI/
        │   │   ├── World/
        │   │   └── Networking/     (sem 7+)
        │   └── Settings/
        └── ...
```

Cuando exista el proyecto, marcar `0.5`–`0.7` y pegar aquí la ruta real si cambia.

---

## Componentes / sistemas (checklist técnica)

| Sistema | Script(s) previstos | Estado |
|---------|---------------------|--------|
| Gravedad por sala | `GravityManager` + `RoomVolume` | [x] | Un manager por sala; pasillo hereda |
| Cuerpo con g custom | `GravityBody` | [x] | Rigidbodies; Physics.gravity = 0 |
| Objeto de valor | `ValuableItem` | [x] | Cambia de manager al cruzar salas |
| Agarre | `PlayerInteractor` + `Grabbable` | [x] | |
| Paquete de misión | `MissionPackage` | [x] | |
| Objetivos | `ObjectiveTrigger` | [x] | 3 en el piso |
| Roles | `PlayerRole` | [x] | Runner / Anchor stub |
| UI telegráfo / outline | `GravityHud` + `DominantValuableOutline` | [x] | Splash + banner FLIP; beacon “TIRA DE G” |
| Moment of the Match | `MatchHighlightRecorder` | [x] | Local |
| Red (host auth g) | por definir en 4.1 | [ ] | |
| Player move | `PlayerMotor` + `LocalPlayerInput` | [x] | 2p |
| Void / respawn | `VoidKillZone` + `CheckpointSystem` | [x] | |
| Setup escena | `SetupOfficeFloorA` + `OfficeFloorFactory` | [x] | 1p y 2p |
| Partida / rematch | `MatchDirector` | [x] | Estado Playing/Won/Lost |

---

## Layout `Office_Floor_A` (referencia)

```
[HUB spawn] → [SALA A: Archive] → [PASILLO] → [SALA B: Open Office] → [SALA C: Executive]
                 2 valuables        vacío L/R         3 valuables                4 valuables
                 Obj: enchufar                        Obj: entregar               Obj: sellar
```

Eje +Z (metros aprox.): Hub z=-4..4 → Archive 4..16 → Pasillo 16..27 → Office 27..43 → Executive 43..57.

| Sala | Valuables (MVP) | Estado blockout |
|------|-----------------|-----------------|
| Hub | Taza $15 (tutorial) | [x] | Tutorial + spawn + paquete + props sin $ |
| A Archive | Caja fuerte $80, Archivador $40 | [x] | Enchufar este |
| Pasillo | Ninguno (g hereda) | [x] | Catwalk + vacío |
| B Open Office | Monitor $120, Planta $60, Cafetera $90 | [x] | Entregar |
| C Executive | Maletín $200, Trofeo $150, Server $110, Planta oro $95 | [x] | Sellar |

---

## Métricas (rellenar en playtests)

| Métrica | Meta | Última medición | Fecha |
|---------|------|-----------------|-------|
| Rematch rate (misma sesión) | ≥ 55% | — | — |
| % partidas con clip/highlight | ≥ 40% | — | — |
| Comprensión regla sin tutorial hablado | ≥ 80% | — | — |
| Mareo reportado | < 10% | — | — |
| Duración media de sesión | ≥ 35 min | — | — |

---

## Registro de cambios (append-only)

| Fecha | Qué pasó | Qué falta ahora |
|-------|----------|-----------------|
| 2026-09-08 | Concepto Gravity Receipt + GDD + autorización de código | Crear este MD (hecho) y esqueleto Unity |
| 2026-09-08 | Creado `PROGRESO_Gravity_Receipt.md` | Fase 0.5–0.8: proyecto Unity, carpetas, escena, README |
| 2026-09-08 | Búsqueda de Unity Hub/Editor: no encontrado | Instalar Unity 6 LTS (URP) para desbloquear 0.5 |
| 2026-09-08 | Instalados Hub + Unity 6000.6.0f1 vía winget | Activar licencia en Hub (0.5c) |
| 2026-09-08 | `-createProject` falló exit 198 (sin licencia) | Scaffold manual de carpeta + scripts |
| 2026-09-08 | Carpeta `GravityReceipt`, README, .gitignore, stubs | 0.5c login + 0.5d abrir proyecto + 0.7 escena |
| 2026-09-08 | Licencia OK; ProjectSettings reparados; escena `Office_Floor_A` creada | Playtest 1p + outline dominante (1.8) + vídeo (1.11) |
| 2026-09-08 | Playtest: usuario agarró y salió de la escena | Fix OOB + gravedad snap + outline + regenerar Setup |
| 2026-09-08 | Push a GitHub `felnarg/GravityReceipt` (commit proto) | Lanzar Cloud Agent overnight con prompt del plan |
| 2026-09-09 | Cloud Agent: 2p split + mapa offline + misión/timer/roles/rematch | Playtest humano + vídeo flip; no Unity en el cloud |
| 2026-09-09 | Pulido overnight: curbs, grab exclusivo, HUD regla/FLIP, FOV+whoosh, valuables home | Playtest Unity local + vídeo 1.11; no online |

---

## Log de sesiones del agente

Formato: cada vez que el agente trabaje en el repo, añadir una entrada breve.

### 2026-09-08 — Sesión inicial
- Creado este archivo de seguimiento.

### 2026-09-08 — Instalación Unity + scaffold
- Hub + Editor 6000; scaffold inicial.

### 2026-09-08 — Post-licencia: escena jugable
- Escena Archive v1 generada.

### 2026-09-08 — Fix caída + plan 1.5–1.8
- Gravedad: dirección = eje hacia el valuable (con bias vertical).
- PlayerMotor: ground raycast, inercia al flip, Escape libera ratón.
- VoidKillZone: respawn por distancia (no solo trigger).
- Outline del dominante + sala sellada más gruesa.
- **Usuario:** `GravityReceipt → Setup Office Floor A` → Play → llevar caja dorada a una **pared** y soltar.

### 2026-09-09 — Overnight: offline 2p + mapa
- `LocalPlayerInput` / split-screen 2p.
- Gravedad por sala (`GravityBody`, `RoomVolume`, evento de g por instancia).
- `MatchDirector` (máquina Playing/Won/Lost) + timer 10:00 + rematch R.
- `MissionPackage`, 3 `ObjectiveTrigger`, `CheckpointSystem`, ping, tutorial Hub.
- `OfficeFloorFactory`: Hub → Archive → Pasillo vacío → Open Office → Executive.
- Roles Runner/Anchor stub. Highlight de mayor caída.
- Play en escena vieja ahora **auto-reconstruye** el piso 2p (`OfficeFloorPlayGuard`).
- Chevrones amarillos de ruta, marcos de puerta, etiquetas `$` en valuables y “PAQUETE”.
- Cámara lenta al ganar/perder + cursor libre para pulsar R.
- Luces puntuales por sala.
- Robustez: `== null` de Unity en managers/paquete/outline; .meta del outline; dents ignorados si el paquete está agarrado.
- Hard stop programado 08:00 America/Bogota; esta entrega es el avance de código.

### 2026-09-09 — Overnight bloque 2 (feel + bugs 2p)
- Bordillos del pasillo 0.58 m (stepOffset 0.28): ya no se camina al vacío.
- Valuables OOB vuelven a casa; volúmenes de sala se solapan en puertas.
- Grab exclusivo 2p; drop al Warp; Sellar con paquete en manos.
- HUD splash 9 s + banner ¡FLIP!; carteles de pista en Hub/Archive/Office/Executive.
- `GravityFlipSfx` whoosh + FOV punch + shake en el motor.
- Beacon “¡ESTE TIRA DE G!” en el dominante. P2 mira con I/K.
- FindPlayer ya no atribuye P1 al slot P2 en 1p.
- Unity fake-null: `== null` en Text/Collider/Renderer/Rigidbody/managers.

### 2026-09-09 — Overnight bloque 3 (siluetas + toast)
- Valuables: monitor plano, plantas/trofeo cápsula, cafetera cilindro; masa escala con `$`.
- Paquete no recibe dents de jugadores (caminar/empujar).
- HUD toast 2.4 s al completar Enchufar/Entregar/Sellar.

### 2026-09-09 — Overnight bloque 4 (regla en el Hub)
- Taza $15 en Hub para el primer flip local.
- Props grises (caja/silla/mesa) **sin** `$`: se pueden agarrar pero no tiran de g.
- `SpawnHome` los devuelve si caen al vacío.

### 2026-09-09 — Overnight bloque 5 (juice)
- Ding al completar objetivo. HUD muestra cooldown de Anchor.
- Moment of the Match cuenta flips además de caídas.
- FOV extra al sprint Runner; cruceta crece al wind-up de agarre.

---

## Checklist del día (copiar al empezar una sesión)

```
Fecha: 2026-09-09
Enfoque de hoy (1–3 IDs del plan): pulido 1.10/2.7 (sin Unity) + 5.5 feel + bugs 2p
Hecho: curbs, HUD regla/FLIP, whoosh/FOV, grab exclusivo, valuables home
Pendiente al cerrar: playtest Unity local, vídeo 1.11, no online
Bloqueadores: Unity Editor ausente en el cloud agent
Actualicé "Estado actual": sí
```
