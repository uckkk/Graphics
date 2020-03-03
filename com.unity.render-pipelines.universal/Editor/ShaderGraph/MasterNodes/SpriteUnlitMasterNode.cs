using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Drawing;
using UnityEditor.ShaderGraph.Drawing.Controls;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Internal;

namespace UnityEditor.Experimental.Rendering.Universal
{
    [Serializable]
    [Title("Master", "Sprite Unlit (Experimental)")]
    [FormerName("UnityEditor.Experimental.Rendering.LWRP.SpriteUnlitMasterNode")]
    class SpriteUnlitMasterNode : AbstractMaterialNode, IMasterNode, IMayRequirePosition, IMayRequireNormal, IMayRequireTangent
    {
        public const string PositionName = "Vertex Position";
        public const string NormalName = "Vertex Normal";
        public const string TangentName = "Vertex Tangent";
        public const string ColorSlotName = "Color";


        public const int PositionSlotId = 9;
        public const int ColorSlotId = 0;
        public const int VertNormalSlotId = 10;
        public const int VertTangentSlotId = 11;

        public SpriteUnlitMasterNode()
        {
            UpdateNodeAfterDeserialization();
            
            // TODO: Remove, temporary.
            RegisterCallback(OnNodeChanged);
        }

        // TODO: This should be based on callbacks/bindings to the Settings object
        // TODO: For now this data lived on the master node so we do this for sinplicity
        void OnNodeChanged(AbstractMaterialNode inNode, ModificationScope scope)
        {
            if(owner != null)
            {
                owner.UpdateSupportedBlocks();
            }
        }

        public sealed override void UpdateNodeAfterDeserialization()
        {
            base.UpdateNodeAfterDeserialization();
            name = "Sprite Unlit Master";

            AddSlot(new PositionMaterialSlot(PositionSlotId, PositionName, PositionName, CoordinateSpace.Object, ShaderStageCapability.Vertex));
            AddSlot(new NormalMaterialSlot(VertNormalSlotId, NormalName, NormalName, CoordinateSpace.Object, ShaderStageCapability.Vertex));
            AddSlot(new TangentMaterialSlot(VertTangentSlotId, TangentName, TangentName, CoordinateSpace.Object, ShaderStageCapability.Vertex));
            AddSlot(new ColorRGBAMaterialSlot(ColorSlotId, ColorSlotName, ColorSlotName, SlotType.Input, Color.white, ShaderStageCapability.Fragment));

            RemoveSlotsNameNotMatching(
                new[]
            {
                PositionSlotId,
                VertNormalSlotId,
                VertTangentSlotId,
                ColorSlotId,
            });
        }

        public string renderQueueTag => $"{RenderQueue.Transparent}";
        public string renderTypeTag => $"{RenderType.Transparent}";
        
        public ConditionalField[] GetConditionalFields(PassDescriptor pass)
        {
            return new ConditionalField[]
            {
                // Features
                new ConditionalField(Fields.GraphVertex,         IsSlotConnected(PBRMasterNode.PositionSlotId) || 
                                                                        IsSlotConnected(PBRMasterNode.VertNormalSlotId) || 
                                                                        IsSlotConnected(PBRMasterNode.VertTangentSlotId)),
                new ConditionalField(Fields.GraphPixel,          true),
                
                // Surface Type
                new ConditionalField(Fields.SurfaceTransparent,  true),
                
                // Blend Mode
                new ConditionalField(Fields.BlendAlpha,          true),
            };
        }

        public void ProcessPreviewMaterial(Material material)
        {

        }

        public NeededCoordinateSpace RequiresNormal(ShaderStageCapability stageCapability)
        {
            List<MaterialSlot> slots = new List<MaterialSlot>();
            GetSlots(slots);

            List<MaterialSlot> validSlots = new List<MaterialSlot>();
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].stageCapability != ShaderStageCapability.All && slots[i].stageCapability != stageCapability)
                    continue;

                validSlots.Add(slots[i]);
            }
            return validSlots.OfType<IMayRequireNormal>().Aggregate(NeededCoordinateSpace.None, (mask, node) => mask | node.RequiresNormal(stageCapability));
        }

        public NeededCoordinateSpace RequiresPosition(ShaderStageCapability stageCapability)
        {
            List<MaterialSlot> slots = new List<MaterialSlot>();
            GetSlots(slots);

            List<MaterialSlot> validSlots = new List<MaterialSlot>();
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].stageCapability != ShaderStageCapability.All && slots[i].stageCapability != stageCapability)
                    continue;

                validSlots.Add(slots[i]);
            }
            return validSlots.OfType<IMayRequirePosition>().Aggregate(NeededCoordinateSpace.None, (mask, node) => mask | node.RequiresPosition(stageCapability));
        }

        public NeededCoordinateSpace RequiresTangent(ShaderStageCapability stageCapability)
        {
            List<MaterialSlot> slots = new List<MaterialSlot>();
            GetSlots(slots);

            List<MaterialSlot> validSlots = new List<MaterialSlot>();
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].stageCapability != ShaderStageCapability.All && slots[i].stageCapability != stageCapability)
                    continue;

                validSlots.Add(slots[i]);
            }
            return validSlots.OfType<IMayRequireTangent>().Aggregate(NeededCoordinateSpace.None, (mask, node) => mask | node.RequiresTangent(stageCapability));
        }

        // TODO: Temporary
        // TODO: Required to prevent duplicate properties now they are also taken from Blocks
        public override void CollectShaderProperties(PropertyCollector properties, GenerationMode generationMode)
        {
            return;
        }

        // TODO: Temporary
        // TODO: Required to prevent duplicate properties now they are also taken from Blocks
        public override void CollectPreviewMaterialProperties(List<PreviewProperty> properties)
        {
            return;
        }
    }
}
