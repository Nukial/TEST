using Unity.Entities;
using UnityEngine;
using RPG.ECS.Triggers;

namespace RPG.ECS.Authoring
{
    /// <summary>
    /// Authoring component for converting Trigger/BehaviorTrigger MonoBehaviours to ECS.
    /// Mirrors DevionGames.BaseTrigger and DevionGames.BehaviorTrigger.
    /// </summary>
    public class TriggerAuthoring : MonoBehaviour
    {
        [Tooltip("Trigger name/identifier.")]
        public string triggerName = "Trigger";

        [Tooltip("Distance at which the trigger can be activated.")]
        public float useDistance = 3f;

        [Tooltip("Input type required to activate.")]
        public TriggerInputType inputType = TriggerInputType.Key;

        [Header("Behavior")]
        [Tooltip("Whether the action sequence can be interrupted.")]
        public bool interruptable = true;

        [Tooltip("Actions to execute when trigger is used.")]
        public ActionDefinition[] onUsedActions;

        [Tooltip("Actions to execute when trigger interaction ends.")]
        public ActionDefinition[] onUnusedActions;

        [Header("Inventory Trigger (Optional)")]
        [Tooltip("If set, this trigger opens an inventory window.")]
        public string inventoryWindowName;

        [Header("Vendor (Optional)")]
        [Tooltip("If > 0, this trigger acts as a vendor.")]
        public float buyPriceFactor = 0f;
        public float sellPriceFactor = 0f;

        [Header("Crafting (Optional)")]
        [Tooltip("If set, this trigger acts as a crafting station.")]
        public string ingredientsWindowName;

        [System.Serializable]
        public struct ActionDefinition
        {
            public string actionTypeName;
            public float duration;
            public string targetName;
            public float value;
            [Tooltip("0 = Self, 1 = Player")]
            public byte targetType;
        }

        public class Baker : Baker<TriggerAuthoring>
        {
            public override void Bake(TriggerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Core trigger data
                AddComponent(entity, new TriggerData
                {
                    NameHash = authoring.triggerName.GetHashCode(),
                    UseDistance = authoring.useDistance,
                    InputType = authoring.inputType,
                    InRange = false,
                    InUse = false,
                    IsStarted = false,
                    CurrentUser = Entity.Null
                });

                // Behavior trigger data - create action sequences
                if (authoring.onUsedActions != null && authoring.onUsedActions.Length > 0)
                {
                    var actionBuffer = AddBuffer<ActionElement>(entity);

                    foreach (var actionDef in authoring.onUsedActions)
                    {
                        actionBuffer.Add(new ActionElement
                        {
                            ActionTypeHash = actionDef.actionTypeName.GetHashCode(),
                            Status = ActionStatus.NotStarted,
                            FloatParam = actionDef.duration,
                            IntParam = 0,
                            TargetType = actionDef.targetType,
                            TargetNameHash = actionDef.targetName != null ?
                                actionDef.targetName.GetHashCode() : 0
                        });
                    }

                    AddComponent(entity, new ActionSequenceData
                    {
                        CurrentActionIndex = 0,
                        TotalActions = authoring.onUsedActions.Length,
                        Status = ActionSequenceStatus.Inactive,
                        Interruptable = authoring.interruptable
                    });
                }

                // Inventory trigger tag
                if (!string.IsNullOrEmpty(authoring.inventoryWindowName))
                {
                    AddComponent(entity, new InventoryTriggerTag
                    {
                        WindowNameHash = authoring.inventoryWindowName.GetHashCode()
                    });
                }

                // Vendor data
                if (authoring.buyPriceFactor > 0f || authoring.sellPriceFactor > 0f)
                {
                    AddComponent(entity, new VendorData
                    {
                        BuyPriceFactor = authoring.buyPriceFactor,
                        SellPriceFactor = authoring.sellPriceFactor
                    });
                }

                // Crafting data
                if (!string.IsNullOrEmpty(authoring.ingredientsWindowName))
                {
                    AddComponent(entity, new CraftingData
                    {
                        IngredientsWindowHash = authoring.ingredientsWindowName.GetHashCode()
                    });
                }
            }
        }
    }
}
