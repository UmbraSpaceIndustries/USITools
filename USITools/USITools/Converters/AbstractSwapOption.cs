using System.Collections.Generic;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace USITools
{
    /// <summary>
    /// Swap options are loadouts that can be applied to a converter to
    ///   alter its behavior, like changing its recipe or giving it side effects.
    /// </summary>
    /// <remarks>
    /// See <see cref="USI_SwapController"/> and <see cref="USI_SwappableBay"/> for
    ///   details on how these loadouts are applied.
    /// </remarks>
    /// <typeparam name="T">Any converter type derived from the base game's <see cref="BaseConverter"/> class.</typeparam>
    public abstract class AbstractSwapOption<T> : AbstractSwapOption
        where T : BaseConverter, IConverterWithAddons<T>
    {
        public virtual void ApplyConverterChanges(T converter)
        {
            converter.ConverterName = ConverterName;
            converter.StartActionName = StartActionName;
            converter.StopActionName = StopActionName;
            converter.UseSpecialistBonus = UseSpecialistBonus;
            converter.ExperienceEffect = ExperienceEffect;

            MonoUtilities.RefreshContextWindows(part);
        }

        public virtual ConversionRecipe PrepareRecipe(ConversionRecipe recipe)
        {
            return recipe;
        }

        public virtual void PreProcessing(T converter)
        {
            if (converter.Addons.Count > 0)
            {
                for (int i = 0; i < converter.Addons.Count; i++)
                {
                    var addon = converter.Addons[i];
                    addon.PreProcessing();
                }
            }
        }

        public virtual void PostProcess(T converter, ConverterResults result, double deltaTime)
        {
            if (converter.Addons.Count > 0)
            {
                for (int i = 0; i < converter.Addons.Count; i++)
                {
                    var addon = converter.Addons[i];
                    addon.PostProcess(result, deltaTime);
                }
            }
        }
    }

    public abstract class AbstractSwapOption : PartModule
    {
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

        public List<ResourceRatio> inputList;
        public List<ResourceRatio> outputList;
        public List<ResourceRatio> reqList;

        public override string GetInfo()
        {
            StringBuilder output = new StringBuilder();
            output
                .AppendLine(ConverterName)
                .AppendLine();

            if (inputList.Count > 0)
            {
                output.AppendLine("<color=#99FF00>"+Localizer.Format("#LOC_USI_Tools_ASO_Info1") +"</color>");//Inputs:
                foreach (var resource in inputList)
                {
                    output
                        .Append(" - ")
                        .Append(resource.ResourceName)
                        .Append(": ");

                    if (resource.ResourceName == "ElectricCharge")
                        output
                            .AppendFormat("{0:F2}/sec", resource.Ratio)
                            .AppendLine();
                    else
                        output.AppendLine(ParseResourceRatio(resource.Ratio));
                }
            }
            if (outputList.Count > 0)
            {
                output.AppendLine("<color=#99FF00>"+Localizer.Format("#LOC_USI_Tools_ASO_Info2") +"</color>");//Outputs:
                foreach (var resource in outputList)
                {
                    output
                        .Append(" - ")
                        .Append(resource.ResourceName)
                        .Append(": ");

                    if (resource.ResourceName == "ElectricCharge")
                        output
                            .AppendFormat("{0:F2}/sec", resource.Ratio)
                            .AppendLine();
                    else
                        output.AppendLine(ParseResourceRatio(resource.Ratio));
                }
            }
            if (reqList.Count > 0)
            {
                output.AppendLine("<color=#99FF00>"+Localizer.Format("#LOC_USI_Tools_ASO_Info3") +"</color>");//Requirements:
                foreach (var resource in reqList)
                {
                    output
                        .Append(" - ")
                        .Append(resource.ResourceName)
                        .Append(": ")
                        .AppendFormat("{0:F2}", resource.Ratio)
                        .AppendLine();
                }
            }

            return output.ToString();
        }

        public override string GetModuleDisplayName()
        {
            string displayName = GetType().Name;

            var idx = displayName.IndexOf('_');
            if (idx >= 0)
            {
                displayName = displayName.Substring(idx + 1);
            }
            displayName = displayName.Replace("Swap", " ");

            return displayName;
        }

        private string ParseResourceRatio(double ratio)
        {
            //string units = "sec";
            //if (ratio < 0.001)
            //{
            //    ratio *= 60;
            //    units = "min";
            //}
            //if (ratio < 0.001)
            //{
            //    ratio *= 60;
            //    units = "hr";
            //}
            //if (ratio < 0.001)
            //{
            //    ratio *= 6;
            //    units = "day";
            //}

            return string.Format("{0:F2}/day", ratio * KSPUtil.dateTimeFormatter.Day);
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
