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
        private const string NotAvailable = "n.a.";
        internal const short SlowConstraintInfoUpdate = 30;
        internal const short EditorConstraintInfoUpdate = 90;
        internal const short FastConstraintInfoUpdate = 5;
        private const double DemandActualDifferenceWarnLevel = 0.0000000001;
        private static readonly char[] Delimiters = {' ', ',', '\t', ';'};
        [KSPField] public bool HumanReadableInfoValues = true;
        internal Guid ID;
        private ConnectedPartCollection _connectedParts;
        private ConstraintInfo _constraintInfo;
        private short _constraintUpdateCounter = SlowConstraintInfoUpdate;

        [KSPField] public bool alwaysOn = false;
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Const.")] public string constraintDisplay;

        [KSPField] public float conversionRate = 1.0f;
        [KSPField(isPersistant = true)] public bool converterEnabled = false;
        [KSPField] public string converterName = "USI_Converter";

        [KSPField(guiActive = true, guiName = "Converter Status")] public string converterStatus = "Unknown";
        private List<ResourceRatio> inputResourceList;

        [KSPField] public string inputResources = "";
        private double lastUpdateTime;
        private List<ResourceRatio> outputResourceList;

        [KSPField] public string outputResources = "";
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Remaining")] public string remainingTimeDisplay;

        [KSPField] public bool requiresOxygenAtmo = false;

        [KSPField] public bool showRemainingTime = true;
        [KSPField] public bool shutdownIfAllOutputFull = false;

        internal bool ElectricityAffected
        {
            get
            {
                return this.outputResourceList.Any(or => or.resource.name == Utilities.Electricity)
                       || this.inputResourceList.Any(ir => ir.resource.name == Utilities.Electricity);
            }
        }

        [KSPEvent(active = false, guiActive = true, guiActiveEditor = true, guiName = "Activate Converter")]
        public void ActivateConverter()
        {
            this.converterEnabled = true;
            this.UpdateEvents();
        }

        internal void CollectResourceConstraintData(bool convEnabled, double deltaTime)
        {
            if (!this.showRemainingTime && !convEnabled)
            {
                return;
            }
            if (this._constraintUpdateCounter > 0 && !convEnabled)
            {
                this._constraintUpdateCounter--;
                return;
            }
            this._connectedParts = this._findConnectedParts();
            this._constraintUpdateCounter = HighLogic.LoadedSceneIsEditor ? EditorConstraintInfoUpdate : SlowConstraintInfoUpdate;
            var constraintData = new ResourceConstraintData();
            var cnt = 0;
            foreach (var output in this.outputResourceList)
            {
                List<PartResource> resParts;
                var amounts = this._getResourceAmounts(output.resource.name, true, out resParts);
                constraintData.AddConstraint(new ResourceConstraint(output.resource.name, true, amounts[1], amounts[0], output.ratio*this.conversionRate, output.allowExtra, resParts));
                cnt++;
            }
            foreach (var input in this.inputResourceList)
            {
                List<PartResource> resParts;
                var amounts = this._getResourceAmounts(input.resource.name, false, out resParts);
                constraintData.AddConstraint(new ResourceConstraint(input.resource.name, false, amounts[1], amounts[0], input.ratio*this.conversionRate, false, resParts));
                cnt++;
            }
            this._processResourceConstraintData(deltaTime, cnt > 0 ? constraintData : null);
        }

        [KSPEvent(active = false, guiActive = true, guiActiveEditor = true, guiName = "Deactivate Converter")]
        public void DeactivateConverter()
        {
            this.converterEnabled = false;
            this.UpdateEvents();
        }

        public void FixedUpdate()
        {
            if (Time.timeSinceLevelLoad < 1.0f || (!HighLogic.LoadedSceneIsEditor && !FlightGlobals.ready))
            {
                return;
            }

            if (!HighLogic.LoadedSceneHasPlanetarium)
            {
                this.CollectResourceConstraintData(this.converterEnabled, 1d);
                return;
            }

            if (Math.Abs(this.lastUpdateTime) <= double.Epsilon)
            {
                // Just started running
                this.lastUpdateTime = Planetarium.GetUniversalTime();
                return;
            }

            var deltaTime = Math.Min(Planetarium.GetUniversalTime() - this.lastUpdateTime, Utilities.MaxDeltaTime);
            var eDeltaTime = Math.Min(Planetarium.GetUniversalTime() - this.lastUpdateTime, Utilities.ElectricityMaxDeltaTime);

            if (deltaTime < Utilities.MaxDeltaTime)
            {
                return;
            }

            this.lastUpdateTime += deltaTime;
            this.CollectResourceConstraintData(this.converterEnabled, deltaTime);

            if (this.requiresOxygenAtmo && !this.vessel.mainBody.atmosphereContainsOxygen)
            {
                this.converterStatus = "No Oxygen";
                return;
            }

            if (!this.converterEnabled)
            {
                return;
            }

            
            if (this._constraintInfo.Convert)
            {
                var ratio = this._constraintInfo.EarliestConstraint.RateThisFrame(deltaTime)/this._constraintInfo.EarliestConstraint.RatePerSecond;
                var eRatio = this._constraintInfo.EarliestConstraint.RateThisFrame(eDeltaTime) / this._constraintInfo.EarliestConstraint.RatePerSecond; 
                foreach (var resourceConstraint in this._constraintInfo.Constraints.OrderBy(c => c.OutputResource))
                {
                    var demand = resourceConstraint.RatePerSecond * ratio;
                    //For EC we go with the EC ratio - what this means is that as we warp up, EC usage is going to drop as our
                    //time diffrence is greater than the max elec delta time.  The downside is that we use less EC.  The upside is that
                    //we do not kill batteries.  
                    if (resourceConstraint.ResourceName == Utilities.Electricity)
                    {
                        demand = resourceConstraint.RatePerSecond * eRatio;
                    }
                    var actual = resourceConstraint.RequestResource(demand);
                    if (Math.Abs(actual - demand) > (resourceConstraint.ResourceName == Utilities.Electricity ? 1d : DemandActualDifferenceWarnLevel*1000))
                    {
                        this.LogWarning(resourceConstraint.ResourceName + " demand = " + demand + " but " + actual + " transferred (rem. res. amount = " + resourceConstraint.RemainingAmount + ")");
                    }
                }
                this.converterStatus = "Running";
            }
            else
            {
                this.converterStatus = "Standby";
            }
        }

        public override string GetInfo()
        {
            var sb = new StringBuilder();
            sb.Append(this.converterName);
            sb.Append("\n\nInputs:");
            foreach (var input in this.inputResourceList)
            {
                double ratio = input.ratio*this.conversionRate;
                sb.Append("\n - ").Append(input.resource.name).Append(": ").Append(Utilities.FormatValue(ratio, 2, this.HumanReadableInfoValues));
            }
            sb.Append("\n\nOutputs: ");
            foreach (var output in this.outputResourceList)
            {
                double ratio = output.ratio*this.conversionRate;
                sb.Append("\n - ").Append(output.resource.name).Append(": ").Append(Utilities.FormatValue(ratio, 2, this.HumanReadableInfoValues));
            }
            sb.Append("\n");
            if (this.requiresOxygenAtmo)
            {
                sb.Append("\nRequires an atmosphere containing Oxygen.");
            }
            if (this.alwaysOn)
            {
                sb.Append("\nCannot be turned off.");
            }

            return sb.ToString();
        }

        public override void OnAwake()
        {
            this.Log("OnAwake");
            base.OnAwake();
            this.UpdateResourceLists();
        }

        public override void OnLoad(ConfigNode node)
        {
            this.Log("OnLoad: " + node);
            base.OnLoad(node);
            this.lastUpdateTime = Utilities.GetValue(node, "lastUpdateTime", this.lastUpdateTime);

            this.UpdateResourceLists();
            this.UpdateEvents();
        }

        public override void OnSave(ConfigNode node)
        {
            node.AddValue("lastUpdateTime", this.lastUpdateTime);
            this.Log("OnSave: " + node);
        }

        public override void OnStart(StartState state)
        {
            this.Log("OnStart: " + state);
            base.OnStart(state);

            if (state != StartState.Editor)
            {
                this.part.force_activate();
                this.ID = Guid.NewGuid();
            }

            if (!this.showRemainingTime)
            {
                var remTimeDisp = this.Fields["remainingTimeDisplay"];
                var constDisp = this.Fields["constraintDisplay"];
                remTimeDisp.guiActive = remTimeDisp.guiActiveEditor = constDisp.guiActive = constDisp.guiActiveEditor = false;
            }

            this.UpdateEvents();

            this.constraintDisplay = NotAvailable;
            this.remainingTimeDisplay = NotAvailable;
        }

        private void ParseInputResourceString(string resourceString, List<ResourceRatio> resources)
        {
            resources.Clear();

            string[] tokens = resourceString.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);

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

            string[] tokens = resourceString.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);

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

        [KSPAction("Toggle Converter")]
        public void ToggleConverter(KSPActionParam param)
        {
            this.converterEnabled = !this.converterEnabled;
            this.UpdateEvents();
        }

        private void UpdateEvents()
        {
            if (this.alwaysOn)
            {
                this.Events["ActivateConverter"].active = false;
                this.Events["DeactivateConverter"].active = false;
                this.converterEnabled = true;
                //BP - When we turn it on, let's also reset the time counter.
                this.lastUpdateTime = Planetarium.GetUniversalTime();
            }
            else
            {
                this.Events["ActivateConverter"].active = !this.converterEnabled;
                this.Events["DeactivateConverter"].active = this.converterEnabled;

                if (!this.converterEnabled)
                {
                    this.converterStatus = "Inactive";
                }
            }
        }

        private void UpdateResourceLists()
        {
            if (this.inputResourceList == null)
            {
                this.inputResourceList = new List<ResourceRatio>();
            }
            if (this.outputResourceList == null)
            {
                this.outputResourceList = new List<ResourceRatio>();
            }

            this.ParseInputResourceString(this.inputResources, this.inputResourceList);
            this.ParseOutputResourceString(this.outputResources, this.outputResourceList);

            this.Events["ActivateConverter"].guiName = "Activate " + this.converterName;
            this.Events["DeactivateConverter"].guiName = "Deactivate " + this.converterName;
            this.Actions["ToggleConverter"].guiName = "Toggle " + this.converterName;
            this.Fields["converterStatus"].guiName = this.converterName;
        }

        private ConnectedPartCollection _findConnectedParts()
        {
            var allParts = HighLogic.LoadedSceneIsEditor ? EditorLogic.fetch.ship.Parts : this.part.vessel.Parts;
            var allVessel = allParts;
            var noFlow = new List<Part> {this.part};
            var stagePrioIn = this.part.FindPartsInSameStage(allParts, false);
            var stagePrioOut = this.part.FindPartsInSameStage(allParts, true);
            var stackPrioIn = this.part.FindPartsInSameResStack(allParts, new HashSet<Part>(), false);
            var stackPrioOut = this.part.FindPartsInSameResStack(allParts, new HashSet<Part>(), true);
            return new ConnectedPartCollection(allVessel, noFlow, stagePrioIn, stagePrioOut, stackPrioIn, stackPrioOut, this.part);
        }

        private double[] _getResourceAmounts(string resourceName, bool outRes, out List<PartResource> pRes)
        {
            const string warnMsg = "[USI_Converter] unabled to retrieve resource amounts in editor";
            var amount = 0d;
            var maxAmount = 0d;
            var sourceParts = new List<Part>();
            pRes = new List<PartResource>();
            try
            {
                var resDef = PartResourceLibrary.Instance.GetDefinition(resourceName);
                switch (resDef.resourceFlowMode)
                {
                    case ResourceFlowMode.NO_FLOW:
                        sourceParts = this._connectedParts.NoFlow;
                        break;
                    case ResourceFlowMode.ALL_VESSEL:
                        sourceParts = this._connectedParts.AllVessel;
                        break;
                    case ResourceFlowMode.STAGE_PRIORITY_FLOW:
                        sourceParts = outRes ? this._connectedParts.StagePrioOutRes : this._connectedParts.StagePrioInRes;
                        break;
                    case ResourceFlowMode.STACK_PRIORITY_SEARCH:
                        sourceParts = outRes ? this._connectedParts.StackPrioOutRes : this._connectedParts.StackPrioInRes;
                        break;
                }
                var filtered = sourceParts
                    .Where(sp => sp.Resources.Contains(resDef.name))
                    .Select(sourcePart => sourcePart.Resources[resDef.name])
                    .Where(partRes => partRes != null && partRes.flowState).ToList();
                foreach (var partRes in filtered)
                {
                    amount += partRes.amount;
                    maxAmount += partRes.maxAmount;
                }
                pRes = filtered;
            }
            catch (NullReferenceException)
            {
                Debug.LogWarning(warnMsg);
            }
            catch (ArgumentException)
            {
                Debug.LogWarning(warnMsg);
            }
            catch (StackOverflowException)
            {
                Debug.LogWarning(warnMsg);
            }
            return new[] {amount, maxAmount};
        }

        private void _processResourceConstraintData(double deltaTime, ResourceConstraintData data = null)
        {
            if (data == null)
            {
                this.remainingTimeDisplay = NotAvailable;
                this.constraintDisplay = NotAvailable;
                return;
            }
            var info = data.GetConstraintInfo(ref this._constraintUpdateCounter, deltaTime);
            this.remainingTimeDisplay = info.InfoTexts[0];
            this.constraintDisplay = info.InfoTexts[1];
            this._constraintInfo = info;
        }

        private class ConnectedPartCollection
        {
            internal List<Part> AllVessel { get; private set; }
            internal List<Part> NoFlow { get; private set; }
            internal List<Part> StackPrioInRes { get; private set; }
            internal List<Part> StackPrioOutRes { get; private set; }
            internal List<Part> StagePrioInRes { get; private set; }
            internal List<Part> StagePrioOutRes { get; private set; }

            internal ConnectedPartCollection(List<Part> allVessel, List<Part> noFlow, List<Part> stagePrioIn, List<Part> stagePrioOut, List<Part> stackPrioIn, List<Part> stackPrioOut, Part convPart)
            {
                this.AllVessel = _sortByDistance(allVessel, convPart, true);
                this.NoFlow = _sortByDistance(noFlow, convPart, true);
                this.StackPrioInRes = _sortByDistance(stackPrioIn, convPart, false);
                this.StackPrioOutRes = _sortByDistance(stackPrioOut, convPart, true);
                this.StagePrioInRes = _sortByDistance(stagePrioIn, convPart, false);
                this.StagePrioOutRes = _sortByDistance(stagePrioOut, convPart, true);
            }

            private static List<Part> _sortByDistance(List<Part> startList, Part convPart, bool startClose)
            {
                var wrapperList = new List<PartDistanceWrapper>(startList.Count);
                wrapperList.AddRange(startList.Select(startPart => new PartDistanceWrapper(startPart, Vector3.Distance(startPart.transform.position, convPart.transform.position))));
                return (startClose ? wrapperList.OrderBy(i => i.Distance) : wrapperList.OrderByDescending(i => i.Distance)).Select(w => w.Part).ToList();
            }

            private struct PartDistanceWrapper
            {
                internal float Distance { get; private set; }
                internal Part Part { get; private set; }

                public PartDistanceWrapper(Part part, float dist) : this()
                {
                    this.Part = part;
                    this.Distance = dist;
                }
            }
        }

        private class ConstraintInfo
        {
            internal List<ResourceConstraint> Constraints { get; private set; }
            internal bool Convert { get; private set; }
            internal ResourceConstraint EarliestConstraint { get; private set; }
            internal string[] InfoTexts { get; private set; }

            internal ConstraintInfo(string remainingTime, string earliestConstraint, bool convert, ResourceConstraint earliest, List<ResourceConstraint> constraints)
            {
                this.Convert = convert;
                this.Constraints = constraints;
                this.InfoTexts = new[] {remainingTime, earliestConstraint};
                this.EarliestConstraint = earliest;
            }
        }

        private class ResourceConstraint
        {
            internal bool AllowsOverflow { get; private set; }
            private double Amount { get; set; }
            private double MaxAmount { get; set; }
            internal bool OutputResource { get; private set; }
            private List<PartResource> PartResources { get; set; }
            internal double RatePerSecond { get; private set; }

            internal double RemainingAmount
            {
                get { return this.OutputResource ? (this.AllowsOverflow ? double.MaxValue : this.MaxAmount - this.Amount) : this.Amount; }
            }

            internal double RemainingPercentage
            {
                get
                {
                    var percent = 0d;
                    if (this.MaxAmount > 0)
                    {
                        percent = this.Amount/this.MaxAmount;
                    }
                    return this.OutputResource ? 1d - percent : percent;
                }
            }

            internal double RemainingSeconds
            {
                get { return this.RemainingAmount/this.RatePerSecond; }
            }

            internal string ResourceName { get; private set; }

            internal ResourceConstraint(string name, bool output, double max, double avail, double rate, bool allowsOverflow, List<PartResource> partResources)
            {
                this.ResourceName = name;
                this.OutputResource = output;
                this.MaxAmount = max;
                this.Amount = avail;
                this.RatePerSecond = rate;
                this.AllowsOverflow = allowsOverflow;
                this.PartResources = partResources;
            }

            internal double RateThisFrame(double deltaTime)
            {
                return Math.Min(this.RatePerSecond*deltaTime, this.RemainingAmount);
            }

            internal double RequestResource(double demand)
            {
                var collected = 0d;
                foreach (var partResource in this.PartResources)
                {
                    if (collected >= demand)
                    {
                        break;
                    }
                    var avail = this.OutputResource ? partResource.maxAmount - partResource.amount : partResource.amount;
                    var transf = Math.Min(avail, demand - collected);
                    collected += transf;
                    if (!this.OutputResource)
                    {
                        transf *= -1;
                    }
                    partResource.amount += transf;
                    if (partResource.amount < 0)
                    {
                        partResource.amount = 0;
                    }
                    if (partResource.amount > partResource.maxAmount)
                    {
                        partResource.amount = partResource.maxAmount;
                    }
                }
                if (this.AllowsOverflow)
                {
                    if (collected < demand)
                    {
                        collected = demand;
                    }
                }
                return collected;
            }
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

            internal ConstraintInfo GetConstraintInfo(ref short counter, double deltaTime)
            {
                var earliestConstraint = this._findEarliestConstraint(deltaTime);
                if (earliestConstraint != null)
                {
                    var convInfos = _checkIfConvertingPossible(earliestConstraint, deltaTime);
                    return new ConstraintInfo(_convertRemainingToDisplayText(earliestConstraint.RemainingSeconds, earliestConstraint.RemainingPercentage, ref counter),
                                              earliestConstraint.ResourceName + (earliestConstraint.OutputResource ? " full" : " depleted"), convInfos, earliestConstraint, this._constraints);
                }
                return new ConstraintInfo(NotAvailable, NotAvailable, false, null, this._constraints);
            }

            private static bool _checkIfConvertingPossible(ResourceConstraint earliestConstraint, double deltaTime)
            {
                var rate = earliestConstraint.RateThisFrame(deltaTime);
                return rate > DemandActualDifferenceWarnLevel;
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

            private ResourceConstraint _findEarliestConstraint(double deltaTime)
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
    }
}