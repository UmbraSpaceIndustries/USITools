using System.Collections.Generic;
using System.Text;
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
            converter.StopActionName = StopActionName;
            converter.UseSpecialistBonus = UseSpecialistBonus;
            converter.ExperienceEffect = ExperienceEffect;

            MonoUtilities.RefreshContextWindows(part);
        }

        public virtual void PostProcess(T Converter, ConverterResults result, double deltaTime)
        {
            PostProcess(result, deltaTime);
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
                output.AppendLine("<color=#99FF00>Inputs:</color>");
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
                output.AppendLine("<color=#99FF00>Outputs:</color>");
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
                output.AppendLine("<color=#99FF00>Requirements:</color>");
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

            // 60 seconds X 60 minutes X 6 hours = 21600 seconds per Kerbin day
            return string.Format("{0:F2}/day", ratio * 21600);
        }

        public virtual void PostProcess(ConverterResults result, double deltaTime)
        {
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
