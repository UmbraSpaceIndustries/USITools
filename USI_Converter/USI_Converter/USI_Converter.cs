/**
 * Umbra Space Industries Resource Converter
 * 
 * This is a derivative work of Thunder Aerospace Corporation's library for  
 * the Kerbal Space Program, which is (c) 2013, Taranis Elsu, who retains the copyright for 
 * all unmodified portions of this work.  Enhancements and extensions are (c) 2014 Bob Palmer.  
 *  
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 * 
 * This code is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
 * creative commons license. See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>
 * for full details.
 * 
 * Attribution — You are free to modify this code, so long as you mention that the resulting
 * work is based upon or adapted from this code.
 * 
 * Non-commercial - You may not use this work for commercial purposes.
 * 
 * Share Alike — If you alter, transform, or build upon this work, you may distribute the
 * resulting work only under the same or similar license to the CC BY-NC-SA 3.0 license.
 * 
 * Note that Thunder Aerospace Corporation and Umbra Space Industries are ficticious entities 
 * created for entertainment purposes. It is in no way meant to represent a real entity.
 *  Any similarity to a real entity is purely coincidental.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace USI
{
    public class USI_Converter : PartModule
    {
        private static char[] delimiters = {' ', ',', '\t', ';'};

        [KSPField] public string converterName = "TAC Generic Converter";

        [KSPField(guiActive = true, guiName = "Converter Status")] public string converterStatus = "Unknown";

        [KSPField(isPersistant = true)] public bool converterEnabled = false;

        [KSPField] public bool alwaysOn = false;

        [KSPField] public float conversionRate = 1.0f;

        [KSPField] public string inputResources = "";

        [KSPField] public string outputResources = "";

        [KSPField] public bool requiresOxygenAtmo = false;

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Remaining")] public string RemainingTimeDisplay;
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Const.")] public string ConstraintDisplay;
        private const string NotAvailable = "n.a.";
        internal Guid ID;
        private short _constraintUpdateCounter = SlowConstraintInfoUpdate;
        internal const short SlowConstraintInfoUpdate = 30;
        internal const short FastConstraintInfoUpdate = 5;

        private double lastUpdateTime = 0.0f;

        private List<ResourceRatio> inputResourceList;
        private List<ResourceRatio> outputResourceList;

        public override void OnAwake()
        {
            this.Log("OnAwake");
            base.OnAwake();
            UpdateResourceLists();
        }

        public override void OnStart(PartModule.StartState state)
        {
            this.Log("OnStart: " + state);
            base.OnStart(state);

            if (state != StartState.Editor)
            {
                part.force_activate();
                ID = Guid.NewGuid();
            }

            UpdateEvents();

            ConstraintDisplay = NotAvailable;
            RemainingTimeDisplay = NotAvailable;
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            this.CollectResourceConstraintData();

            if (Time.timeSinceLevelLoad < 1.0f || !FlightGlobals.ready)
            {
                return;
            }

            if (lastUpdateTime == 0.0f)
            {
                // Just started running
                lastUpdateTime = Planetarium.GetUniversalTime();
                return;
            }

            double deltaTime = Math.Min(Planetarium.GetUniversalTime() - lastUpdateTime, Utilities.MaxDeltaTime);
            lastUpdateTime += deltaTime;

            if (converterEnabled)
            {
                if (requiresOxygenAtmo && !vessel.mainBody.atmosphereContainsOxygen)
                {
                    converterStatus = "Atmo lacks oxygen.";
                    return;
                }

                double desiredAmount = conversionRate*deltaTime;
                double maxElectricityDesired = Math.Min(desiredAmount, conversionRate*Math.Max(Utilities.ElectricityMaxDeltaTime, TimeWarp.fixedDeltaTime)); // Limit the max electricity consumed when reloading a vessel

                // Limit the resource amounts so that we do not produce more than we have room for, nor consume more than is available
                foreach (ResourceRatio output in outputResourceList)
                {
                    if (!output.allowExtra)
                    {
                        if (output.resource.id == Utilities.ElectricityId && desiredAmount > maxElectricityDesired)
                        {
                            // Special handling for electricity
                            double desiredElectricity = maxElectricityDesired*output.ratio;
                            double availableSpace = -part.IsResourceAvailable(output.resource, -desiredElectricity);
                            desiredAmount = desiredAmount*(availableSpace/desiredElectricity);
                        }
                        else
                        {
                            double availableSpace = -part.IsResourceAvailable(output.resource, -desiredAmount*output.ratio);
                            desiredAmount = availableSpace/output.ratio;
                        }
                        if (desiredAmount <= 0.000000001)
                        {
                            // Out of space, so no need to run
                            converterStatus = "No space for more " + output.resource.name;
                            return;
                        }
                    }
                }

                foreach (ResourceRatio input in inputResourceList)
                {
                    if (input.resource.id == Utilities.ElectricityId && desiredAmount > maxElectricityDesired)
                    {
                        // Special handling for electricity
                        double desiredElectricity = maxElectricityDesired*input.ratio;
                        double amountAvailable = part.IsResourceAvailable(input.resource, desiredElectricity);
                        desiredAmount = desiredAmount*(amountAvailable/desiredElectricity);
                    }
                    else
                    {
                        double amountAvailable = part.IsResourceAvailable(input.resource, desiredAmount*input.ratio);
                        desiredAmount = amountAvailable/input.ratio;
                    }
                    if (desiredAmount <= 0.000000001)
                    {
                        // Not enough input resources
                        converterStatus = "Not enough " + input.resource.name;
                        return;
                    }
                }

                foreach (ResourceRatio input in inputResourceList)
                {
                    double desired;
                    if (input.resource.id == Utilities.ElectricityId)
                    {
                        desired = Math.Min(desiredAmount, maxElectricityDesired)*input.ratio;
                    }
                    else
                    {
                        desired = desiredAmount*input.ratio;
                    }

                    double actual = part.TakeResource(input.resource, desired);

                    if (actual < (desired*0.999))
                    {
                        this.LogWarning("OnFixedUpdate: obtained less " + input.resource.name + " than expected: " + desired.ToString("0.000000000") + "/" + actual.ToString("0.000000000"));
                    }
                }

                foreach (ResourceRatio output in outputResourceList)
                {
                    double desired;
                    if (output.resource.id == Utilities.ElectricityId)
                    {
                        desired = Math.Min(desiredAmount, maxElectricityDesired)*output.ratio;
                    }
                    else
                    {
                        desired = desiredAmount*output.ratio;
                    }

                    double actual = -part.TakeResource(output.resource.id, -desired);

                    if (actual < (desired*0.999) && !output.allowExtra)
                    {
                        this.LogWarning("OnFixedUpdate: put less " + output.resource.name + " than expected: " + desired.ToString("0.000000000") + "/" + actual.ToString("0.000000000"));
                    }
                }

                converterStatus = "Running";
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            this.Log("OnLoad: " + node);
            base.OnLoad(node);
            lastUpdateTime = Utilities.GetValue(node, "lastUpdateTime", lastUpdateTime);

            UpdateResourceLists();
            UpdateEvents();
        }

        public override void OnSave(ConfigNode node)
        {
            node.AddValue("lastUpdateTime", lastUpdateTime);
            this.Log("OnSave: " + node);
        }

        public override string GetInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(converterName);
            sb.Append("\n\nInputs:");
            foreach (var input in inputResourceList)
            {
                double ratio = input.ratio*conversionRate;
                sb.Append("\n - ").Append(input.resource.name).Append(": ").Append(Utilities.FormatValue(ratio, 3)).Append("U/sec");
            }
            sb.Append("\n\nOutputs: ");
            foreach (var output in outputResourceList)
            {
                double ratio = output.ratio*conversionRate;
                sb.Append("\n - ").Append(output.resource.name).Append(": ").Append(Utilities.FormatValue(ratio, 3)).Append("U/sec");
            }
            sb.Append("\n");
            if (requiresOxygenAtmo)
            {
                sb.Append("\nRequires an atmosphere containing Oxygen.");
            }
            if (alwaysOn)
            {
                sb.Append("\nCannot be turned off.");
            }

            return sb.ToString();
        }

        [KSPEvent(active = false, guiActive = true, guiActiveEditor = true, guiName = "Activate Converter")]
        public void ActivateConverter()
        {
            converterEnabled = true;
            UpdateEvents();
        }

        [KSPEvent(active = false, guiActive = true, guiActiveEditor = true, guiName = "Deactivate Converter")]
        public void DeactivateConverter()
        {
            converterEnabled = false;
            UpdateEvents();
        }

        [KSPAction("Toggle Converter")]
        public void ToggleConverter(KSPActionParam param)
        {
            converterEnabled = !converterEnabled;
            UpdateEvents();
        }

        private void UpdateEvents()
        {
            if (alwaysOn)
            {
                Events["ActivateConverter"].active = false;
                Events["DeactivateConverter"].active = false;
                converterEnabled = true;
            }
            else
            {
                Events["ActivateConverter"].active = !converterEnabled;
                Events["DeactivateConverter"].active = converterEnabled;

                if (!converterEnabled)
                {
                    converterStatus = "Inactive";
                }
            }
        }

        private void UpdateResourceLists()
        {
            if (inputResourceList == null)
            {
                inputResourceList = new List<ResourceRatio>();
            }
            if (outputResourceList == null)
            {
                outputResourceList = new List<ResourceRatio>();
            }

            ParseInputResourceString(inputResources, inputResourceList);
            ParseOutputResourceString(outputResources, outputResourceList);

            Events["ActivateConverter"].guiName = "Activate " + converterName;
            Events["DeactivateConverter"].guiName = "Deactivate " + converterName;
            Actions["ToggleConverter"].guiName = "Toggle " + converterName;
            Fields["converterStatus"].guiName = converterName;
        }

        private void ParseInputResourceString(string resourceString, List<ResourceRatio> resources)
        {
            resources.Clear();

            string[] tokens = resourceString.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < (tokens.Length - 1); i += 2)
            {
                PartResourceDefinition resource = PartResourceLibrary.Instance.GetDefinition(tokens[i]);
                double ratio;
                if (resource != null && double.TryParse(tokens[i + 1], out ratio))
                {
                    resources.Add(new ResourceRatio(resource, ratio));
                }
                else
                {
                    this.Log("Cannot parse \"" + resourceString + "\", something went wrong.");
                }
            }

            var ratios = resources.Aggregate("", (result, value) => result + value.resource.name + ", " + value.ratio + ", ");
            this.Log("Input resources parsed: " + ratios + "\nfrom " + resourceString);
        }

        private void ParseOutputResourceString(string resourceString, List<ResourceRatio> resources)
        {
            resources.Clear();

            string[] tokens = resourceString.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < (tokens.Length - 2); i += 3)
            {
                PartResourceDefinition resource = PartResourceLibrary.Instance.GetDefinition(tokens[i]);
                double ratio;
                bool allowExtra;
                if (resource != null && double.TryParse(tokens[i + 1], out ratio) && bool.TryParse(tokens[i + 2], out allowExtra))
                {
                    resources.Add(new ResourceRatio(resource, ratio, allowExtra));
                }
                else
                {
                    this.Log("Cannot parse \"" + resourceString + "\", something went wrong.");
                }
            }

            var ratios = resources.Aggregate("", (result, value) => result + value.resource.name + ", " + value.ratio + ", ");
            this.Log("Output resources parsed: " + ratios + "\nfrom " + resourceString);
        }

        internal void CollectResourceConstraintData()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (_constraintUpdateCounter > 0)
                {
                    _constraintUpdateCounter--;
                    return;
                }
                this._constraintUpdateCounter = SlowConstraintInfoUpdate;
            }
            var constraintData = new ResourceConstraintData();
            var cnt = 0;
            foreach (var output in outputResourceList.Where(r => !r.allowExtra))
            {
                var amounts = this._getResourceAmounts(output.resource.name);
                constraintData.AddConstraint(new ResourceConstraint(output.resource.name, true, amounts[1], amounts[0], output.ratio*this.conversionRate));
                cnt++;
            }
            foreach (var input in inputResourceList)
            {
                var amounts = this._getResourceAmounts(input.resource.name);
                constraintData.AddConstraint(new ResourceConstraint(input.resource.name, false, amounts[1], amounts[0], input.ratio*this.conversionRate));
                cnt++;
            }
            this._processResourceConstraintData(cnt > 0 ? constraintData : null);
        }

        private void _processResourceConstraintData(ResourceConstraintData data = null)
        {
            if (data == null)
            {
                RemainingTimeDisplay = NotAvailable;
                ConstraintDisplay = NotAvailable;
                return;
            }
            var info = data.GetConstraintInfo(ref _constraintUpdateCounter);
            RemainingTimeDisplay = info[0];
            ConstraintDisplay = info[1];
        }

        private class ResourceConstraintData
        {
            private readonly List<ResourceConstraint> _constraints;

            internal ResourceConstraintData()
            {
                this._constraints = new List<ResourceConstraint>();
            }

            internal void AddConstraint(ResourceConstraint constraint)
            {
                this._constraints.Add(constraint);
            }

            internal string[] GetConstraintInfo(ref short counter)
            {
                var earliestConstraint = _findEarliestConstraint();
                if (earliestConstraint != null)
                {
                    return new[]
                           {
                               _convertRemainingToDisplayText(earliestConstraint.RemainingSeconds, earliestConstraint.RemainingPercentage, ref counter),
                               earliestConstraint.ResourceName + (earliestConstraint.OutputResource ? " full" : " depleted")
                           };
                }
                return new[] {NotAvailable, NotAvailable};
            }

            private static string _convertRemainingToDisplayText(double remainingSeconds, double remainingPercent, ref short counter)
            {
                string displayText;
                var days = remainingSeconds/21600;
                if (days > 1)
                {
                    displayText = string.Format("{0:#0.#} days", days);
                }
                else
                {
                    var timespan = TimeSpan.FromSeconds(remainingSeconds);
                    displayText = string.Format("{0:D2}:{1:D2}:{2:D2}", timespan.Hours, timespan.Minutes, timespan.Seconds);
                    counter = FastConstraintInfoUpdate;
                }
                return displayText + string.Format(" ({0:P2})", remainingPercent);
            }

            private ResourceConstraint _findEarliestConstraint()
            {
                var lowestRemaining = double.MaxValue;
                ResourceConstraint nearestConstraint = null;
                foreach (var constraint in this._constraints)
                {
                    var remSec = constraint.RemainingSeconds;
                    if (!(remSec < lowestRemaining))
                    {
                        continue;
                    }
                    nearestConstraint = constraint;
                    lowestRemaining = remSec;
                }
                return nearestConstraint;
            }
        }

        private class ResourceConstraint
        {
            internal string ResourceName { get; private set; }
            internal bool OutputResource { get; private set; }
            private double MaxAmount { get; set; }
            private double Amount { get; set; }
            private double RatePerSecond { get; set; }

            internal ResourceConstraint(string name, bool output, double max, double avail, double rate)
            {
                this.ResourceName = name;
                this.OutputResource = output;
                this.MaxAmount = max;
                this.Amount = avail;
                this.RatePerSecond = rate;
            }

            internal double RemainingSeconds
            {
                get
                {
                    var remAmount = OutputResource ? MaxAmount - Amount : Amount;
                    return remAmount/RatePerSecond;
                }
            }

            internal double RemainingPercentage
            {
                get
                {
                    var percent = 0d;
                    if (MaxAmount > 0)
                    {
                        percent = Amount/MaxAmount;
                    }
                    return OutputResource ? 1d - percent : percent;
                }
            }
        }

        private double[] _getResourceAmounts(String resourceName)
        {
            var resources = this._getConnectedResources(resourceName).ToList();
            var amount = resources.Sum(r => r.amount);
            var maxAmount = resources.Sum(r => r.maxAmount);
            return new[] {amount, maxAmount};
        }

        private IEnumerable<PartResource> _getConnectedResources(String resourceName)
        {
            var resources = new List<PartResource>();
            var resDef = PartResourceLibrary.Instance.GetDefinition(resourceName);
            if (resDef != null)
            {
                if (HighLogic.LoadedSceneIsEditor)
                {
                    if (resDef.resourceFlowMode == ResourceFlowMode.NO_FLOW)
                    {
                        if (this.part.Resources.Contains(resourceName))
                        {
                            resources.Add(this.part.Resources[resourceName]);
                        }
                    }
                    else if (resDef.resourceFlowMode != ResourceFlowMode.NULL)
                    {
                        var eParts = EditorLogic.fetch.ship.Parts;
                        resources.AddRange(from ePart in eParts
                                           where ePart.Resources.Contains(resourceName)
                                           select ePart.Resources[resourceName]);
                    }
                }
                if (HighLogic.LoadedSceneIsFlight)
                {
                    this.part.GetConnectedResources(resDef.id, resDef.resourceFlowMode, resources);
                }
            }
            return resources;
        }
    }
}