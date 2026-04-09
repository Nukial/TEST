using Unity.Entities;
using UnityEngine;
using RPG.ECS.Inventory;

namespace RPG.ECS.Authoring
{
    /// <summary>
    /// Authoring component for converting EquipmentHandler MonoBehaviour to ECS.
    /// Mirrors DevionGames.InventorySystem.EquipmentHandler with bone mapping
    /// and equipment region slots.
    /// </summary>
    public class EquipmentHandlerAuthoring : MonoBehaviour
    {
        [Tooltip("Name of the equipment UI window.")]
        public string windowName = "Equipment";

        [Tooltip("Equipment region definitions for this character.")]
        public EquipmentRegionDef[] regions = new EquipmentRegionDef[]
        {
            new EquipmentRegionDef { regionName = "Head" },
            new EquipmentRegionDef { regionName = "Chest" },
            new EquipmentRegionDef { regionName = "Legs" },
            new EquipmentRegionDef { regionName = "Feet" },
            new EquipmentRegionDef { regionName = "MainHand" },
            new EquipmentRegionDef { regionName = "OffHand" },
        };

        [System.Serializable]
        public struct EquipmentRegionDef
        {
            public string regionName;
        }

        public class Baker : Baker<EquipmentHandlerAuthoring>
        {
            public override void Bake(EquipmentHandlerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new EquipmentHandlerTag
                {
                    WindowNameHash = authoring.windowName.GetHashCode()
                });

                var slotBuffer = AddBuffer<EquipmentSlotElement>(entity);

                foreach (var region in authoring.regions)
                {
                    slotBuffer.Add(new EquipmentSlotElement
                    {
                        RegionHash = region.regionName.GetHashCode(),
                        EquippedItemHash = 0,
                        VisualInstance = Entity.Null,
                        IsOccupied = false
                    });
                }
            }
        }
    }
}
