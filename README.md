# Simulador de Misiones 2.5D - Unity

Un sistema sencillo de misiones de entrega/recogida en perspectiva 2.5D para Unity 2022.3 LTS.

## Características

- ✅ **Sistema de misiones flexible**: Entrega, recogida, y entrega con vuelta
- ✅ **Controlador 2.5D**: Movimiento en plano XZ con cámara isométrica
- ✅ **UI de misiones**: Panel con objetivo, temporizador y barra de progreso
- ✅ **Marcadores visuales**: Indicadores animados para recogida y entrega
- ✅ **Dadores de misiones**: NPCs/puntos que asignan misiones
- ✅ **Configuración rápida**: Script para generar escena de prueba completa

## Estructura del Proyecto

```
Assets/
├── Scripts/
│   ├── MissionSystem/
│   │   ├── MissionTypes.cs          # Enums y datos de misión
│   │   ├── MissionManager.cs        # Gestor principal
│   │   ├── MissionPickup.cs         # Detección de recogida
│   │   ├── MissionDeliveryZone.cs   # Zona de entrega
│   │   ├── MissionUI.cs             # Interfaz de usuario
│   │   └── MissionSetup.cs          # Configuración de escena prueba
│   ├── Player/
│   │   ├── PlayerController.cs      # Movimiento y interacción
│   │   └── Camera2D5.cs             # Cámara 2.5D/isométrica
│   └── World/
│       ├── MissionMarker.cs         # Marcadores visuales
│       └── MissionGiver.cs          # NPCs dadores de misiones
├── Prefabs/                         # Prefabs reutilizables
├── Scenes/                          # Escenas del juego
└── Resources/                       # Assets cargados en runtime
```

## Instalación

1. Abre Unity 2022.3 LTS
2. Crea un nuevo proyecto 3D (Core)
3. Copia la carpeta `Assets/Scripts` a tu proyecto
4. (Opcional) Copia `Assets/Resources` si usas el setup automático

## Uso Rápido

### 1. Configuración Automática (Escena de Prueba)

1. Crea una escena vacía
2. Crea un GameObject vacío llamado "Setup"
3. Añade el componente `MissionSetup`
4. En el Inspector, click en el menú del componente (⋮) → "Setup Test Scene"
5. ¡Presiona Play!

### 2. Configuración Manual

#### Crear MissionManager
```csharp
// En Awake de un GameObject persistente
GameObject manager = new GameObject("MissionManager");
manager.AddComponent<MissionManager>();
```

#### Crear Misión
```csharp
var mission = new DeliveryMissionData
{
    missionName = "Entrega Urgente",
    description = "Lleva el paquete a la zona marcada",
    type = MissionType.Delivery,
    pickupLocation = pickupTransform,
    deliveryLocation = deliveryTransform,
    itemPrefab = itemPrefab,
    timeLimit = 300f, // 5 minutos
    reward = 200
};

missionManager.AddMission(mission);
missionManager.StartMission(0); // Índice de la misión
```

#### Crear Jugador
1. GameObject con `CharacterController`
2. Añadir componente `PlayerController`
3. Asignar `MainCamera` y `ItemHoldPoint` (hijo del jugador en posición delantera)

#### Crear Cámara 2.5D
1. Cámara principal con componente `Camera2D5`
2. Asignar `target` al jugador
3. Ajustar `offset`, `zoomDistance`, `isometricAngle`

## Tipos de Misión

| Tipo | Descripción | Flujo |
|------|-------------|-------|
| `Delivery` | Recoger y entregar | Pickup → Delivery → Complete |
| `Pickup` | Solo recoger | Pickup → Complete |
| `DeliveryAndReturn` | Entregar y volver | Pickup → Delivery → Return → Complete |

## Controles

| Acción | Tecla |
|--------|-------|
| Mover | WASD / Flechas |
| Saltar | Espacio |
| Interactuar/Entregar | E |
| Zoom cámara | Scroll ratón |

## Personalización

### Añadir Nuevo Tipo de Misión
1. Añade valor a `MissionType` enum
2. Actualiza `MissionManager.CheckMissionProgress()`
3. Actualiza `MissionUI.GetObjectiveText()`

### Cambiar Estilo Visual
- Modifica `MissionMarker` para otros efectos
- Cambia materiales/colores en `MissionSetup.CreateMarker()`
- Personaliza `MissionUI` con tus propios prefabs UI

### Añadir Recompensas
Extiende `DeliveryMissionData`:
```csharp
public class MyMissionData : DeliveryMissionData
{
    public int experienceReward;
    public ItemData itemReward;
    public bool unlocksNextMission;
}
```

## Eventos Disponibles

```csharp
missionManager.missionEvents.OnMissionStarted.AddListener(OnMissionStart);
missionManager.missionEvents.OnMissionCompleted.AddListener(OnMissionComplete);
missionManager.missionEvents.OnMissionFailed.AddListener(OnMissionFail);
missionManager.missionEvents.OnObjectiveUpdated.AddListener(OnObjectiveUpdate);
```

## Requisitos

- Unity 2022.3 LTS o superior
- TextMeshPro (para UI) - Se instala via Package Manager
- Input System Package (opcional, usa Input Manager legacy por defecto)

## Solución de Problemas

**El jugador no recoge objetos:**
- Verifica que el objeto tenga collider `isTrigger = true`
- Verifica capa de colisión (Layer Collision Matrix)

**La cámara no sigue al jugador:**
- Asigna `target` en `Camera2D5` al transform del jugador
- Verifica que la cámara tenga tag "MainCamera"

**UI no aparece:**
- Verifica que `MissionUI` esté en un Canvas activo
- Comprueba referencias de textos/sliders en Inspector

## Extensiones Sugeridas

- [ ] Sistema de inventario
- [ ] Misiones en cadena (prerequisitos)
- [ ] Minimapa con marcadores
- [ ] Diálogos con opciones
- [ ] Sistema de reputación/facciones
- [ ] Misiones generadas proceduralmente

## Licencia

MIT - Libre para uso personal y comercial.

---

**¡Diviértete creando misiones!** 🎮📦