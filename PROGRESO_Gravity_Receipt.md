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
| **Fase** | 1 — Proto: una sala, una regla |
| **Semana del plan** | 1–2 (en curso) |
| **Última actualización** | 2026-09-08 |
| **En curso ahora** | Usuario debe regenerar escena y re-playtestear flip |
| **Hecho relevante** | Fix OOB + gravedad por eje + outline + sala sellada |
| **Siguiente acción concreta** | Menú GravityReceipt → Setup Office Floor A → Play → llevar caja a una pared |
| **Build jugable** | Sí (Editor Play Mode, 1p local) |
| **Online 4p** | No |
| **Bloqueadores** | Ninguno |

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
| 0.7 | Escena `Office_Floor_A` (blockout vacío) | [x] | Sala Archive + 2 valuables + player |
| 0.8 | README corto del proyecto | [x] | `README.md` en raíz |
| 0.9 | Scripts stub gravedad / player / grab | [x] | + `VoidKillZone`, `SetupOfficeFloorA`, `GravityHud` |

---

# SEMANAS 1–2 — Proto: una sala, una regla

**Objetivo:** 2 personas entienden la regla en ≤30 s sin explicación hablada.

| ID | Tarea | Estado | Notas |
|----|--------|--------|-------|
| 1.1 | Player: move / look / jump | [x] | `PlayerMotor` (Input Manager) |
| 1.2 | Agarrar / soltar props | [x] | Wind-up 0.4 s |
| 1.3 | `ValuableItem` con precio `$` | [x] | |
| 1.4 | `GravityManager` por sala | [x] | Stub; telegráfo 1 s |
| 1.5 | Aplicar vector de gravedad | [x] | Snap a ejes mundo hacia el valuable |
| 1.6 | Telegráfo 1.0 s antes del flip | [x] | + HUD texto |
| 1.7 | Inercia corta al cambiar g | [x] | Reset parcial de velocidad + ground raycast |
| 1.8 | Outline del valuable dominante | [x] | `DominantValuableOutline` (pulso/color) |
| 1.9 | Escena mínima: 1 sala + 2 valuables | [x] | Sala sellada + respawn OOB |
| 1.10 | Playtest local 2p (misma máquina o builds) | [ ] | Re-playtest 1p tras fix OOB |
| 1.11 | Vídeo mudo 8–10 s del flip | [ ] | |

**Gate 1–2:** si no se entiende en 30 s → simplificar UI, no añadir features.

---

# SEMANAS 3–4 — Feel + objetivo 1

**Objetivo:** completar Sala A en coop local sin tutorial hablado.

| ID | Tarea | Estado | Notas |
|----|--------|--------|-------|
| 2.1 | `MissionPackage` (abolladuras / 3 vidas) | [ ] | |
| 2.2 | Objetivo “enchufar” en Sala A | [ ] | |
| 2.3 | Checkpoint al completar objetivo | [ ] | |
| 2.4 | Grab wind-up 0.4 s | [x] | Ya en 1.2 |
| 2.5 | Ping radial (“¡no toques eso!”) | [ ] | |
| 2.6 | Tutorial de 1 frase en pared del hub | [ ] | Texto: la gravedad sigue lo más caro |
| 2.7 | Playtest Sala A completa | [ ] | |

**Gate 3–4:** run de Archive estable y divertida.

---

# SEMANAS 5–6 — Mapa completo offline

**Objetivo:** 3 partidas seguidas con rematch voluntario.

| ID | Tarea | Estado | Notas |
|----|--------|--------|-------|
| 3.1 | Blockout Pasillo + vacío lateral | [ ] | |
| 3.2 | Blockout Sala B Open Office | [ ] | 3 valuables |
| 3.3 | Blockout Sala C Executive | [ ] | 4 valuables |
| 3.4 | Objetivo 2: entregar paquete | [ ] | |
| 3.5 | Objetivo 3: sellar contrato | [ ] | |
| 3.6 | Timer de partida ~10:00 | [ ] | |
| 3.7 | Roles stub: Runner (sprint) / Anchor (fix 3 s) | [ ] | Clerk/Intern opcionales |
| 3.8 | Muerte por vacío + respawn en checkpoint | [ ] | |
| 3.9 | Moment of the Match (mayor caída) | [ ] | Local primero |
| 3.10 | Flujo rematch <15 s | [ ] | |

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
| 5.5 | Ajustar telegráfo / FOV / velocidad de flip | [ ] | |
| 5.6 | Pass siluetas/colores valuables | [ ] | |
| 5.7 | Emotes (4) + whoosh final | [ ] | |
| 5.8 | Decisión go / no-go | [ ] | Ver métricas abajo |

**Gate go:** rematch ≥ 55% **o** ≥ 40% de grupos envían clip solos.  
**Si no:** recortar scope / retocar feel; no añadir sistemas nuevos.

---

# SEMANAS 11–12 — Vertical slice vendible

**Objetivo:** build privada + vídeo que vende el juego en silencio.

| ID | Tarea | Estado | Notas |
|----|--------|--------|-------|
| 6.1 | Pass de arte (no final, salir de gris total) | [ ] | |
| 6.2 | Pulido UI mínima | [ ] | |
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
        │   │   └── Networking/     (sem 7+)
        │   └── Settings/
        └── ...
```

Cuando exista el proyecto, marcar `0.5`–`0.7` y pegar aquí la ruta real si cambia.

---

## Componentes / sistemas (checklist técnica)

| Sistema | Script(s) previstos | Estado |
|---------|---------------------|--------|
| Gravedad por sala | `GravityManager` | [x] | Stub inicial |
| Objeto de valor | `ValuableItem` | [x] | Stub inicial |
| Agarre | `PlayerInteractor` | [x] | Stub (falta `Grabbable` dedicado) |
| Paquete de misión | `MissionPackage` | [ ] |
| Objetivos | `ObjectiveTrigger` | [ ] |
| Roles | `PlayerRole` + abilities | [ ] |
| UI telegráfo / outline | `GravityHud` | [x] | Texto HUD; falta outline |
| Moment of the Match | `MatchHighlightRecorder` | [ ] |
| Red (host auth g) | por definir en 4.1 | [ ] |
| Player move | `PlayerMotor` | [x] | Input Manager |
| Void / respawn | `VoidKillZone` | [x] | |
| Setup escena | `SetupOfficeFloorA` | [x] | Menú GravityReceipt |

---

## Layout `Office_Floor_A` (referencia)

```
[HUB spawn] → [SALA A: Archive] → [PASILLO] → [SALA B: Open Office] → [SALA C: Executive]
                 2 valuables        vacío L/R         3 valuables                4 valuables
                 Obj: enchufar                        Obj: entregar               Obj: sellar
```

| Sala | Valuables (MVP) | Estado blockout |
|------|-----------------|-----------------|
| Hub | — | [~] | Solo Archive por ahora |
| A Archive | Caja fuerte $80, Archivador $40 | [x] | Escena generada |
| Pasillo | Ninguno (g hereda) | [ ] |
| B Open Office | Monitor $120, Planta $60, Cafetera $90 | [ ] |
| C Executive | Maletín $200, Trofeo $150, Server $110, Planta oro $95 | [ ] |

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

---

## Checklist del día (copiar al empezar una sesión)

```
Fecha:
Enfoque de hoy (1–3 IDs del plan):
Hecho:
Pendiente al cerrar:
Bloqueadores:
Actualicé "Estado actual": sí / no
```
