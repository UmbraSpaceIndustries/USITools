using UnityEngine;
using KSP.Localization;

namespace USITools
{
    /// <summary>
    /// Responsible for UI interactions related to swappable converters.
    /// </summary>
    /// <remarks>
    /// See <see cref="USI_SwapController"/> and <see cref="AbstractSwapOption"/>
    ///   for additional information.
    /// </remarks>
    public class USI_SwappableBay : PartModule
    {
        #region KSP Fields and Events
        [KSPField]
        public string bayName = "";

        [KSPField]
        public int moduleIndex = 0;

        [KSPField]
        public bool hasPermanentLoadout = false;

        [KSPField(isPersistant = true)]
        public int currentLoadout = 0;

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "#LOC_USI_Recipe")]//Recipe: 
        public string curTemplate = "???";

        [KSPEvent(active = true, guiActiveEditor = true, guiActiveUnfocused = true, guiName = "B1: Install [None]", unfocusedRange = 10f)]//
        public void LoadSetup()
        {
            if (!CheckResources())
                return;

            var oldTemplate = curTemplate;
            currentLoadout = displayLoadout;
            NextSetup();

            ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_USI_Tools_msg7", oldTemplate,curTemplate), 5f,
                ScreenMessageStyle.UPPER_CENTER);//"Reconfiguration from <<1>> to <<2>> completed."

            ConfigureLoadout();
        }

        [KSPEvent(active = true, guiActiveEditor = true, guiActiveUnfocused = true, guiName = "B1: Next Loadout", unfocusedRange = 10f)]//
        public void NextSetup()
        {
            if (_controller.Loadouts.Count < 2)
                return;

            displayLoadout++;
            if (displayLoadout >= _controller.Loadouts.Count)
            {
                displayLoadout = 0;
            }
            if (displayLoadout == currentLoadout)
            {
                NextSetup();
            }

            ChangeMenu();
        }

        [KSPEvent(active = true, guiActiveEditor = true, guiActiveUnfocused = true, guiName = "B1:  Prev. Loadout", unfocusedRange = 10f)]//
        public void PrevSetup()
        {
            if (_controller.Loadouts.Count < 2)
                return;

            displayLoadout--;
            if (displayLoadout < 0)
            {
                displayLoadout = _controller.Loadouts.Count - 1;
            }
            if (displayLoadout == currentLoadout)
            {
                PrevSetup();
            }

            ChangeMenu();
        }
        #endregion

        #region Fields and properties
        private bool _postLoad = false;
        private int displayLoadout;
        private USI_SwapController _controller;
        private bool _repairSkillRequired = true;
        #endregion

        public override void OnStart(StartState state)
        {
            _controller = part.FindModuleImplementing<USI_SwapController>();
            if (_controller == null)
            {
                Debug.LogError(string.Format("[USI] {0}: Part is misconfigured. USI_SwappableBay modules require a USI_SwapController module.", GetType().Name));
            }

            GameEvents.OnAnimationGroupStateChanged.Add(SetModuleState);
            displayLoadout = currentLoadout;

            _repairSkillRequired = USI_ConverterOptions.ConverterSwapRequiresRepairSkillEnabled;

            ConfigureLoadout();
            ConfigureMenus();

            // Disable the menus if there is only one swap option or if this bay has a permanent loadout.
            if (hasPermanentLoadout || _controller.Loadouts.Count < 2)
            {
                EnableMenus(false);
            }
        }

        private void SetModuleState(ModuleAnimationGroup module, bool enable)
        {
            if (module != null && module.part != part)
                return;

            if (HighLogic.LoadedSceneIsFlight && !hasPermanentLoadout)
            {
                EnableMenus(enable);
            }
        }

        private void ConfigureLoadout()
        {
            _controller.ApplyLoadout(currentLoadout, moduleIndex);
        }

        private void ConfigureMenus()
        {
            bool evaRequired = USI_ConverterOptions.ConverterSwapRequiresEVAEnabled;

            Events["NextSetup"].externalToEVAOnly = evaRequired;
            Events["NextSetup"].guiActive = !evaRequired;
            Events["PrevSetup"].externalToEVAOnly = evaRequired;
            Events["PrevSetup"].guiActive = !evaRequired;
            Events["LoadSetup"].externalToEVAOnly = evaRequired;
            Events["LoadSetup"].guiActive = !evaRequired;

            MonoUtilities.RefreshContextWindows(part);
        }

        private void EnableMenus(bool enable)
        {
            Events["NextSetup"].active = enable;
            Events["PrevSetup"].active = enable;
            Events["LoadSetup"].active = enable;
            Fields["curTemplate"].guiActive = enable;

            MonoUtilities.RefreshContextWindows(part);
        }

        public void Update()
        {
            if (!_postLoad)
            {
                if (_controller.Loadouts.Count > 1)
                {
                    _postLoad = true;
                    NextSetup();
                }
            }
        }

        public void ChangeMenu()
        {
            Events["NextSetup"].guiName = (bayName + " Next " + _controller.typeName).Trim();//
            Events["PrevSetup"].guiName = (bayName + " Prev. " + _controller.typeName).Trim();//
            Fields["curTemplate"].guiName = (bayName + " Recipe").Trim();//
            curTemplate = _controller.Loadouts[currentLoadout].ConverterName;
            Events["LoadSetup"].guiName =
                (bayName + " " + curTemplate + "=>" + _controller.Loadouts[displayLoadout].ConverterName).Trim();

            MonoUtilities.RefreshContextWindows(part);
        }

        private bool CheckResources()
        {
            if (HighLogic.LoadedSceneIsEditor)
                return true;

            if (USI_ConverterOptions.ConverterSwapRequiresRepairSkillEnabled)
            {
                if (USI_ConverterOptions.ConverterSwapRequiresEVAEnabled)
                {
                    var kerbal = FlightGlobals.ActiveVessel.rootPart.protoModuleCrew[0];
                    if (!kerbal.HasEffect("RepairSkill"))
                    {
                        ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_USI_Tools_msg1"), 5f,
                            ScreenMessageStyle.UPPER_CENTER);//"Only Kerbals with repair skills (e.g. engineers, mechanics) can reconfigure modules!"
                        return false;
                    }
                }
                else
                {
                    bool foundRepairSkill = false;
                    var crew = FlightGlobals.ActiveVessel.GetVesselCrew();
                    for (int i = 0; i < crew.Count; i++)
                    {
                        var kerbal = crew[i];
                        if (kerbal.HasEffect("RepairSkill"))
                        {
                            foundRepairSkill = true;
                            break;
                        }
                    }
                    if (!foundRepairSkill)
                    {
                        ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_USI_Tools_msg2"), 5f,//"A Kerbal with repair skills (e.g. engineer, mechanic) must be on board to reconfigure modules!"
                            ScreenMessageStyle.UPPER_CENTER);
                        return false;
                    }
                }
            }

            float costMultiplier = USI_ConverterOptions.ConverterSwapCostMultiplierValue;
            if (costMultiplier > ResourceUtilities.FLOAT_TOLERANCE)
            {
                var allResources = true;
                var missingResources = "";
                //Check that we have everything we need.
                var count = _controller.SwapCosts.Count;
                for (int i = 0; i < count; ++i)
                {
                    var resource = _controller.SwapCosts[i];
                    if (!HasResource(resource))
                    {
                        allResources = false;
                        missingResources += "\n" + (resource.Ratio * costMultiplier) + " " + resource.ResourceName;
                    }
                }
                if (!allResources)
                {
                    ScreenMessages.PostScreenMessage( Localizer.Format("#LOC_USI_Tools_msg3") + missingResources, 5f,//"Missing resources to change module:"
                        ScreenMessageStyle.UPPER_CENTER);
                    return false;
                }
                //Since everything is here...
                for (int i = 0; i < count; ++i)
                {
                    var resource = _controller.SwapCosts[i];
                    TakeResources(resource);
                }
            }
            return true;
        }

        private bool HasResource(ResourceRatio resInfo)
        {
            var resourceName = resInfo.ResourceName;
            var costMultiplier = USI_ConverterOptions.ConverterSwapCostMultiplierValue;

            if (costMultiplier <= ResourceUtilities.FLOAT_TOLERANCE)
            {
                return true;
            }

            var needed = resInfo.Ratio * costMultiplier;
            var whpList = LogisticsTools.GetRegionalWarehouses(vessel, "USI_ModuleResourceWarehouse");
            //EC we're a lot less picky...
            if (resInfo.ResourceName == "ElectricCharge")
            {
                whpList.AddRange(part.vessel.parts);
            }
            var count = whpList.Count;
            for (int i = 0; i < count; ++i)
            {
                var whp = whpList[i];
                if (whp == part)
                    continue;

                if (resInfo.ResourceName != "ElectricCharge")
                {
                    var wh = whp.FindModuleImplementing<USI_ModuleResourceWarehouse>();
                    if (wh != null)
                    {
                        if (!wh.localTransferEnabled)
                            continue;
                    }
                }
                if (whp.Resources.Contains(resourceName))
                {
                    var res = whp.Resources[resourceName];
                    if (res.amount >= needed)
                    {
                        needed = 0;
                        break;
                    }
                    else
                    {
                        needed -= res.amount;
                    }
                }
            }
            return (needed < ResourceUtilities.FLOAT_TOLERANCE);
        }

        private void TakeResources(ResourceRatio resInfo)
        {
            var resourceName = resInfo.ResourceName;
            var costMultiplier = USI_ConverterOptions.ConverterSwapCostMultiplierValue;

            if (costMultiplier > 0)
            {
                var needed = resInfo.Ratio * costMultiplier;
                //Pull in from warehouses

                var whpList = LogisticsTools.GetRegionalWarehouses(vessel, "USI_ModuleResourceWarehouse");
                var count = whpList.Count;
                for (int i = 0; i < count; ++i)
                {
                    var whp = whpList[i];
                    if (whp == part)
                        continue;
                    var wh = whp.FindModuleImplementing<USI_ModuleResourceWarehouse>();
                    if (wh != null)
                    {
                        if (!wh.localTransferEnabled)
                            continue;
                    }
                    if (whp.Resources.Contains(resourceName))
                    {
                        var res = whp.Resources[resourceName];
                        if (res.amount >= needed)
                        {
                            res.amount -= needed;
                            needed = 0;
                            break;
                        }
                        else
                        {
                            needed -= res.amount;
                            res.amount = 0;
                        }
                    }
                }
            }
        }

        public void OnDestroy()
        {
            GameEvents.OnAnimationGroupStateChanged.Remove(SetModuleState);
        }
    }
}
