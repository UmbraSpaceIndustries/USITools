using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using USITools.Helpers;

namespace USITools
{
    public abstract class AbstractDeployModule : PartModule
    {
        protected static string DEPOSIT_MADE_MESSAGE = "#LOC_USI_Deployable_DepositMadeMessage";
        protected static string PART_IS_CREWED_ERROR_MESSAGE = "#LOC_USI_Deployable_PartIsCrewedErrorMessage";

        protected const string PAW_GROUP_NAME = "usi-deployable";

        protected readonly List<PartModule> _affectedModules = new List<PartModule>();
        protected bool _isInitialized = false;
        protected List<ResourceRatio> _resourceCosts;

        #region KSP fields
        [KSPField]
        public string AffectedPartModules;

        [KSPField]
        public int CrewCapacity = 0;

        [KSPField]
        public string DeployGuiName;

        /// <summary>
        /// A value from 0 - 1 representing the percentage of the deploy resources paid so far.
        /// </summary>
        [KSPField(
            isPersistant = true,
            groupName = PAW_GROUP_NAME,
            groupDisplayName = "#LOC_USI_Deployable_PAWGroupDisplayName",
            guiActive = true,
            guiActiveEditor = false,
            guiName = "#LOC_USI_Deployable_PercentPaidDisplayName",
            guiFormat = "P2")]
        public double PartialDeployPercentage = 0d;

        [KSPField]
        public string PartInfoTitle;

        [KSPField]
        public string PAWGroupDisplayName;

        /// <summary>
        /// Comma-separated list of resources and amounts (ex. MaterialKits,200,SpecializedParts,20)
        /// </summary>
        [KSPField]
        public string ResourceCosts;

        [KSPField]
        public double ResourceMultiplier = 1d;

        [KSPField]
        public string RetractGuiName;

        [KSPField(isPersistant = true)]
        public bool StartDeployed = false;

        [KSPField]
        public string ToggleGuiName;
        #endregion

        #region KSP actions and events
        [KSPAction(guiName = "#LOC_USI_Deployable_DeployDisplayName")]
        public void DeployAction(KSPActionParam param)
        {
            Deploy();
        }

        [KSPEvent(
            guiName = "#LOC_USI_Deployable_DeployDisplayName",
            groupName = PAW_GROUP_NAME,
            groupDisplayName = "#LOC_USI_Deployable_PAWGroupDisplayName",
            guiActive = false,
            guiActiveEditor = true,
            guiActiveUnfocused = true,
            unfocusedRange = 10f)]
        public void DeployEvent()
        {
            Deploy();
        }

        [KSPAction(guiName = "#LOC_USI_Deployable_PayDisplayName")]
        public void PayAction(KSPActionParam param)
        {
            DepositResources();
        }

        [KSPEvent(
            guiName = "#LOC_USI_Deployable_PayDisplayName",
            groupName = PAW_GROUP_NAME,
            groupDisplayName = "#LOC_USI_Deployable_PAWGroupDisplayName",
            guiActive = true,
            guiActiveEditor = false,
            guiActiveUnfocused = true,
            unfocusedRange = 10f)]
        public void PayEvent()
        {
            DepositResources();
        }

        [KSPAction(guiName = "#LOC_USI_Deployable_RetractDisplayName")]
        public void RetractAction(KSPActionParam param)
        {
            Retract();
        }

        [KSPEvent(
            guiName = "#LOC_USI_Deployable_RetractDisplayName",
            groupName = PAW_GROUP_NAME,
            groupDisplayName = "#LOC_USI_Deployable_PAWGroupDisplayName",
            guiActive = false,
            guiActiveEditor = true,
            guiActiveUnfocused = true,
            unfocusedRange = 10f)]
        public void RetractEvent()
        {
            Retract();
        }

        [KSPAction(guiName = "#LOC_USI_Deployable_ToggleDisplayName")]
        public void ToggleAction(KSPActionParam param)
        {
            if (StartDeployed)
            {
                Retract();
            }
            else
            {
                Deploy();
            }
        }
        #endregion

        private void CompressResourceCapacity()
        {
            try
            {
                foreach (var resource in part.Resources)
                {
                    if (resource.maxAmount > ResourceMultiplier)
                    {
                        resource.maxAmount /= ResourceMultiplier;
                    }
                    if (resource.amount > resource.maxAmount)
                    {
                        resource.amount = resource.maxAmount;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[USITools] {ClassName}: Error compressing resources. {ex.Message}");
            }
        }

        private void ConsumeResource(ResourceRatio resourceRatio, double percentage)
        {
            var resourceName = resourceRatio.ResourceName;
            var needed = resourceRatio.Ratio * percentage;

            var sourceParts = LogisticsTools.GetRegionalWarehouses(vessel, nameof(USI_ModuleResourceWarehouse));
            foreach (var sourcePart in sourceParts)
            {
                if (sourcePart != part)
                {
                    var warehouse = sourcePart.FindModuleImplementing<USI_ModuleResourceWarehouse>();
                    if (warehouse != null &&
                        warehouse.localTransferEnabled &&
                        sourcePart.Resources.Contains(resourceName))
                    {
                        var resource = sourcePart.Resources[resourceName];
                        if (resource.flowState)
                        {
                            if (resource.amount >= needed)
                            {
                                resource.amount -= needed;
                                needed = 0;
                                break;
                            }
                            else
                            {
                                needed -= resource.amount;
                                resource.amount = 0;
                            }
                        }
                    }
                }
            }
        }

        private void ConsumeResources(double percentage)
        {
            if (_resourceCosts == null || _resourceCosts.Count < 1)
            {
                return;
            }
            foreach (var resource in _resourceCosts)
            {
                ConsumeResource(resource, percentage);
            }
        }

        public virtual void Deploy()
        {
            if (PartialDeployPercentage >= 1d)
            {
                ExpandResourceCapacity();
                EnableModules();
                StartDeployed = true;
                RefreshPAW();
            }
        }

        protected virtual void DepositResources()
        {
            if (HighLogic.LoadedSceneIsEditor ||
               PartialDeployPercentage >= 1d ||
                _resourceCosts == null ||
                _resourceCosts.Count < 1)
            {
                return;
            }

            if (PartialDeployPercentage < 0d)
            {
                PartialDeployPercentage = 0d;
            }

            var needed = 1d - PartialDeployPercentage;
            var available = FindResourcesMinimumAvailablePercentage();
            if (available >= needed)
            {
                ConsumeResources(needed);
                PartialDeployPercentage = 1d;
            }
            else
            {
                ConsumeResources(available);
                PartialDeployPercentage = Math.Min(1d, PartialDeployPercentage + available);
            }

            var percentage = $"{100 * PartialDeployPercentage:F1}";
            var message = string.Format(DEPOSIT_MADE_MESSAGE, percentage, part.partInfo.title);
            ScreenMessages.PostScreenMessage(message, 5f);

            RefreshPAW();
        }

        protected void DisableModules()
        {
            foreach (var module in _affectedModules)
            {
                if (module is BaseConverter)
                {
                    (module as BaseConverter).DisableModule();
                }
                else
                {
                    module.isEnabled = false;
                }
            }
        }

        protected void EnableModules()
        {
            foreach (var module in _affectedModules)
            {
                if (module is BaseConverter)
                {
                    (module as BaseConverter).EnableModule();
                }
                else
                {
                    module.isEnabled = true;
                }
            }
        }

        protected void ExpandCrewCapacity()
        {
            if (CrewCapacity > 0)
            {
                part.CrewCapacity = CrewCapacity;
                part.CheckTransferDialog();
                MonoUtilities.RefreshContextWindows(part);
            }
        }

        protected void ExpandResourceCapacity()
        {
            try
            {
                if (ResourceMultiplier > 1d)
                {
                    foreach (var resource in part.Resources)
                    {
                        resource.maxAmount *= ResourceMultiplier;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[USITools] {ClassName}: Error expanding resources. {ex.Message}");
            }
        }

        protected double FindResourcesMinimumAvailablePercentage()
        {
            if (_resourceCosts == null || _resourceCosts.Count < 1)
            {
                return 0d;
            }

            return _resourceCosts.Select(FindResourceAvailablePercentage).Min();
        }

        protected double FindResourceAvailablePercentage(ResourceRatio resource)
        {
            var resourceName = resource.ResourceName;
            var needed = resource.Ratio;
            if (needed < ResourceUtilities.FLOAT_TOLERANCE)
            {
                return 1d;
            }
            var available = 0d;
            var warehouses = LogisticsTools.GetRegionalModules<USI_ModuleResourceWarehouse>(vessel);

            if (warehouses == null || warehouses.Count < 1)
            {
                return available;
            }

            foreach (var warehouse in warehouses)
            {
                // TODO - change this so that it ignores localTransferEnabled for the active vessel
                if (!warehouse.localTransferEnabled && resourceName != "ElectricCharge")
                {
                    continue;
                }
                if (warehouse.part.Resources.Contains(resourceName))
                {
                    var partResource = warehouse.part.Resources[resourceName];
                    if (partResource.flowState)
                    {
                        available += warehouse.part.Resources[resourceName].amount;
                        if (available >= needed)
                        {
                            return 1d;
                        }
                    }
                }
            }
            return available / needed;
        }

        protected void GetAffectedPartModules()
        {
            _affectedModules.Clear();

            var sanitized = string.IsNullOrEmpty(AffectedPartModules) ?
                string.Empty :
                Regex.Replace(AffectedPartModules, @"\s+", "");
            var tokens = sanitized.Split(',');
            foreach (var module in part.Modules)
            {
                if (module is BaseConverter || tokens.Contains(module.moduleName))
                {
                    _affectedModules.Add(module);
                }
            }
        }

        public override string GetInfo()
        {
            if (_resourceCosts == null || _resourceCosts.Count < 1)
            {
                return base.GetInfo();
            }
            var builder = new StringBuilder();
            builder
                .AppendLine(PartInfoTitle)
                .AppendLine();
            foreach (var resource in _resourceCosts)
            {
                builder
                    .AppendFormat("{0} {1}", resource.Ratio, resource.ResourceName)
                    .AppendLine();
            }

            return builder.ToString();
        }

        protected void GetLocalizedDisplayNames()
        {
            Localizer.TryGetStringByTag(
                "#LOC_USI_Deployable_DepositMadeMessage",
                out DEPOSIT_MADE_MESSAGE);
            Localizer.TryGetStringByTag(
                "#LOC_USI_Deployable_PartIsCrewedErrorMessage",
                out PART_IS_CREWED_ERROR_MESSAGE);

            if (Localizer.TryGetStringByTag(
                "#LOC_USI_Deployable_PercentPaidDisplayName",
                out var percentPaidDisplayName))
            {
                Fields[nameof(PartialDeployPercentage)].guiName = percentPaidDisplayName;
            }
            if (Localizer.TryGetStringByTag(
                "#LOC_USI_Deployable_PayDisplayName",
                out var payDisplayName))
            {
                Actions[nameof(PayAction)].guiName = payDisplayName;
                Events[nameof(PayEvent)].guiName = payDisplayName;
            }

            // The remaining values can be customized via the part config
            if (string.IsNullOrEmpty(PartInfoTitle))
            {
                if (!Localizer.TryGetStringByTag("#LOC_USI_Deployable_PartInfoTitle", out PartInfoTitle))
                {
                    PartInfoTitle = "#LOC_USI_Deployable_PartInfoTitle";
                }
            }

            if (!string.IsNullOrEmpty(PAWGroupDisplayName))
            {
                Fields[nameof(PartialDeployPercentage)].group.displayName = PAWGroupDisplayName;
            }
            else if (Localizer.TryGetStringByTag(
                "#LOC_USI_Deployable_PAWGroupDisplayName",
                out var pawGroupDisplayName))
            {
                Fields[nameof(PartialDeployPercentage)].group.displayName = pawGroupDisplayName;
            }

            if (!string.IsNullOrEmpty(DeployGuiName))
            {
                Actions[nameof(DeployAction)].guiName = DeployGuiName;
                Events[nameof(DeployEvent)].guiName = DeployGuiName;
            }
            else if (Localizer.TryGetStringByTag(
                "#LOC_USI_Deployable_DeployDisplayName",
                out var deployDisplayName))
            {
                Actions[nameof(DeployAction)].guiName = deployDisplayName;
                Events[nameof(DeployEvent)].guiName = deployDisplayName;
            }

            if (!string.IsNullOrEmpty(RetractGuiName))
            {
                Actions[nameof(RetractAction)].guiName = RetractGuiName;
                Events[nameof(RetractEvent)].guiName = RetractGuiName;
            }
            else if (Localizer.TryGetStringByTag(
                "#LOC_USI_Deployable_RetractDisplayName",
                out var retractDisplayName))
            {
                Actions[nameof(RetractAction)].guiName = retractDisplayName;
                Events[nameof(RetractEvent)].guiName = retractDisplayName;
            }

            if (!string.IsNullOrEmpty(ToggleGuiName))
            {
                Actions[nameof(ToggleAction)].guiName = ToggleGuiName;
            }
            else if (Localizer.TryGetStringByTag(
                "#LOC_USI_Deployable_ToggleDisplayName",
                out var toggleDisplayName))
            {
                Actions[nameof(ToggleAction)].guiName = toggleDisplayName;
            }
        }

        protected void GetResourceCosts()
        {
            if (string.IsNullOrEmpty(ResourceCosts))
            {
                return;
            }

            try
            {
                _resourceCosts = ResourceHelpers.DeserializeResourceRatios(ResourceCosts);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[USITools] {ClassName}: {ex.Message}");
                _resourceCosts = null;
            }
        }
        
        public virtual void Initialize()
        {
            try
            {
                _isInitialized = true;
                GetAffectedPartModules();
                GetLocalizedDisplayNames();
                GetResourceCosts();
                RefreshPAW();
                if (StartDeployed)
                {
                    Deploy();
                }
                else
                {
                    Retract();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[USITools] {ClassName}: Could not initialize. {ex.Message}");
                enabled = false;
            }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            Initialize();
        }

        protected virtual void RefreshPAW()
        {
            if (_resourceCosts != null &&
                _resourceCosts.Count > 0 &&
                PartialDeployPercentage < 1d)
            {
                Actions[nameof(DeployAction)].active = false;
                Actions[nameof(RetractAction)].active = false;
                Actions[nameof(ToggleAction)].active = false;

                Events[nameof(DeployEvent)].guiActive = false;
                Events[nameof(RetractEvent)].guiActive = false;
            }
            else
            {
                Actions[nameof(PayAction)].active = false;
                Actions[nameof(DeployAction)].active = !StartDeployed;
                Actions[nameof(RetractAction)].active = StartDeployed;

                Events[nameof(PayEvent)].guiActive = false;
                Events[nameof(DeployEvent)].guiActive = !StartDeployed;
                Events[nameof(RetractEvent)].guiActive = StartDeployed;

                Fields[nameof(PartialDeployPercentage)].guiActive = false;
            }

            MonoUtilities.RefreshContextWindows(part);
        }

        public virtual void Retract()
        {
            if (part.protoModuleCrew.Count > 0)
            {
                var message = string.Format(PART_IS_CREWED_ERROR_MESSAGE, part.partInfo.title);
                ScreenMessages.PostScreenMessage(message, 5f);
                return;
            }
            part.CrewCapacity = 0;
            if (ResourceMultiplier > 1d)
            {
                CompressResourceCapacity();
            }
            DisableModules();
            SetControlSurfaceState(false);
            StartDeployed = false;
            RefreshPAW();
        }

        protected void SetControlSurfaceState(bool isEnabled)
        {
            var controlSurface = part.FindModuleImplementing<ModuleControlSurface>();
            if (controlSurface == null)
            {
                return;
            }

            controlSurface.isEnabled = isEnabled;
            controlSurface.ignorePitch = !isEnabled;
            controlSurface.ignoreRoll = !isEnabled;
            controlSurface.ignoreYaw = !isEnabled;
        }
    }
}
