using System.Collections.Generic;
using UnityEngine;

namespace USITools
{
    public abstract class AbstractSwapOption<T> : AbstractSwapOption
        where T : BaseConverter
    {
        public virtual void ApplyConverterChanges(T converter)
        {
            converter.ConverterName = ConverterName;
            converter.StartActionName = StartActionName;
            converter.UseSpecialistBonus = UseSpecialistBonus;
            converter.ExperienceEffect = ExperienceEffect;

            MonoUtilities.RefreshContextWindows(part);
        }
    }

    public abstract class AbstractSwapOption : PartModule
    {
        [KSPField]
        public float Efficiency = 1;

        [KSPField]
        public string ResourceName = "";

        [KSPField]
        public string ConverterName = "";

        [KSPField]
        public string StartActionName = "";

        [KSPField]
        public string StopActionName = "";

        [KSPField]
        public bool UseSpecialistBonus = false;

        [KSPField]
        public string ExperienceEffect = "";

        [KSPField]
        public bool UseBonus = true;

        public List<ResourceRatio> inputList { get; set; }
        public List<ResourceRatio> outputList { get; set; }
        public List<ResourceRatio> reqList { get; set; }

        public virtual ConversionRecipe PrepareRecipe(ConversionRecipe recipe)
        {
            return recipe;
        }

        public override void OnLoad(ConfigNode node)
        {
            if (inputList == null)
                inputList = new List<ResourceRatio>();
            if (outputList == null)
                outputList = new List<ResourceRatio>();
            if (reqList == null)
                reqList = new List<ResourceRatio>();

            if (node.HasNode("INPUT_RESOURCE")) inputList.Clear();
            if (node.HasNode("OUTPUT_RESOURCE")) outputList.Clear();
            if (node.HasNode("REQUIRED_RESOURCE")) reqList.Clear();

            var count = node.CountNodes;
            for (int i = 0; i < count; ++i)
            {
                var subNode = node.nodes[i];
                var newResource = new ResourceRatio() { FlowMode = ResourceFlowMode.NULL };
                if (!subNode.HasValue("ResourceName") && subNode.name.EndsWith("_RESOURCE"))
                {
                    Debug.Log("Resource must have value 'ResourceName'");
                    continue;
                }
                newResource.Load(subNode);
                switch (subNode.name)
                {
                    case "INPUT_RESOURCE":
                        inputList.Add(newResource);
                        break;
                    case "OUTPUT_RESOURCE":
                        outputList.Add(newResource);
                        break;
                    case "REQUIRED_RESOURCE":
                        reqList.Add(newResource);
                        break;
                }
            }
        }
    }
}
