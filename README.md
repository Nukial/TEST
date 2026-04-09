# TEST

Unity project source code published as open source.

## Requirements

- Unity Editor (version matching this project)
- Unity Entities package (`com.unity.entities`) 1.0+
- Unity Mathematics (`com.unity.mathematics`)
- Unity Burst (`com.unity.burst`)
- Unity Collections (`com.unity.collections`)
- Unity Physics (`com.unity.physics`) — optional, for physics-based controller

## Run

1. Open project folder in Unity Hub.
2. Add the folder `TEST`.
3. Open and run from Unity Editor.

## Project Structure

### Original OOP Layer (`Assets/Devion Games/`)

The existing MonoBehaviour-based RPG framework by DevionGames, using Singleton, ScriptableObject Data, Observer, Template Method, Strategy, and Chain of Responsibility patterns.

| Module | Namespace | Description |
|---|---|---|
| Inventory System | `DevionGames.InventorySystem` | Items, inventory, equipment, crafting, vendors |
| Stat System | `DevionGames.StatSystem` | Character stats, modifiers, effects, leveling |
| UI Widgets | `DevionGames.UIWidgets` | Reusable UI components: widgets, slots, tooltips |
| Graphs | `DevionGames.Graphs` | Node-based formula/visual scripting engine |
| Triggers | `DevionGames` | Trigger, action sequences, behavior triggers |
| Third Person Controller | `DevionGames` | Character movement, camera, motion state machine |
| Utilities | `DevionGames` | Blackboard, CallbackHandler, JSON, tweening |
| Module Manager | `DevionGames` | Plugin/module integration management |

### ECS Layer (`Assets/ECS/`)

A 1:1 Unity ECS (DOTS) rebuild of the core gameplay systems. Assembly: `RPG.ECS`.

```
Assets/ECS/
├── ECS.asmdef                          # Assembly definition
├── Components/
│   ├── Stats/
│   │   ├── StatData.cs                 # StatElement, AttributeElement, StatModifierElement, StatsHandlerTag, LevelData
│   │   └── StatEffectData.cs           # StatEffectElement, StatCallbackElement
│   ├── Inventory/
│   │   ├── ItemData.cs                 # ItemElement, InventoryContainerTag, ItemCollectionTag, InventoryManagerSingleton
│   │   ├── EquipmentData.cs            # EquipmentHandlerTag, EquipmentSlotElement, EquipRequest, UnequipRequest
│   │   └── InventoryRequests.cs        # AddItemRequest, RemoveItemRequest, UseItemRequest, CraftItemRequest, ItemOperationResult
│   ├── Controller/
│   │   └── ControllerData.cs           # InputData, MovementData, CharacterStateData, PhysicsData, CameraTargetData, PlayerTag
│   ├── Combat/
│   │   └── DamageData.cs               # DamageRequest, DamageResult, DamageProcessedTag
│   └── Triggers/
│       └── TriggerData.cs              # TriggerData, TriggerEventData, ActionSequenceData, ActionElement, BehaviorTriggerData, VendorData, CraftingData
├── Systems/
│   ├── Stats/
│   │   ├── StatCalculationSystem.cs    # Modifier stacking: Flat → PercentAdd → PercentMult
│   │   ├── AttributeSystem.cs          # Clamps CurrentValue to [0, max]
│   │   └── StatEffectSystem.cs         # Ticks effects + LevelUpSystem
│   ├── Inventory/
│   │   ├── InventorySystem.cs          # Add/Remove/Use items, cooldowns, cleanup
│   │   └── EquipmentSystem.cs          # Equip/Unequip + stat modifier propagation, crafting
│   ├── Controller/
│   │   └── ControllerSystems.cs        # InputGathering → Movement → CharacterState
│   ├── Combat/
│   │   └── DamageSystem.cs             # Damage calculation, critical strikes, knockback
│   └── Triggers/
│       └── TriggerSystems.cs           # Proximity detection, action sequences, cleanup
└── Authoring/
    ├── StatsHandlerAuthoring.cs        # Baker for StatsHandler entity
    ├── InventoryManagerAuthoring.cs    # Baker for inventory container entity
    ├── EquipmentHandlerAuthoring.cs    # Baker for equipment handler entity
    ├── ThirdPersonControllerAuthoring.cs # Baker for player controller entity
    └── TriggerAuthoring.cs             # Baker for trigger/vendor/crafting entities
```

#### OOP → ECS Mapping

| OOP Concept | ECS Equivalent |
|---|---|
| `ScriptableObject` data | `IComponentData` structs / `IBufferElementData` |
| `MonoBehaviour` | Systems (`ISystem`) + Authoring (`Baker`) |
| Class inheritance (`Item → UsableItem → EquipmentItem`) | `ItemType` enum + flat struct |
| Singleton (`InventoryManager.current`) | Singleton `IComponentData` |
| Observer/Events (`OnAddItem`, `onValueChange`) | One-frame event entities (`ItemOperationResult`, `TriggerEventData`) |
| State machine (`MotionState`) | `MotionStateType` enum in `CharacterStateData` |
| `Action` sequence | `ActionElement` buffer + `ActionSequenceData` |
| `StatModifier` stacking | `StatModifierElement` buffer processed by `StatCalculationSystem` |

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.
