using System;
using System.Collections;
using System.Text;

namespace USITools
{
    public class ModuleSwappableConverterNew : PartModule
    {
        [KSPField]
        public bool autoActivate = true;

        [KSPField]
        public string bayName = "";

        [KSPField]
        public bool isConverter = false;

        [KSPField]
        public int moduleIndex = 0;

        [KSPField(isPersistant = true)]
        public int currentLoadout = 0;

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Active: ")]
        public string curTemplate = "???";

        [KSPEvent(active = true, guiActiveEditor = true, guiActiveUnfocused = true, externalToEVAOnly = true, guiName = "B1: Install [None]",unfocusedRange = 10f)]
        public void LoadSetup()
        {
            if (!CheckResources())
                return;
            var oldTemplate = curTemplate;
            currentLoadout = displayLoadout;
            NextSetup();
            ScreenMessages.PostScreenMessage("Reconfiguration from " + oldTemplate + " to " + curTemplate + " completed.", 5f,
                ScreenMessageStyle.UPPER_CENTER);
            ConfigureLoadout();
        }

        [KSPEvent(active = true, guiActiveEditor = true, guiActiveUnfocused = true, externalToEVAOnly = true, guiName = "B1: Next Loadout",unfocusedRange = 10f)]
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

        [KSPEvent(active = true, guiActiveEditor = true, guiActiveUnfocused = true, externalToEVAOnly = true, guiName = "B1:  Prev. Loadout", unfocusedRange = 10f)]
        public void PrevSetup()
        {
            if (_controller.Loadouts.Count < 2)
                return;
            
            displayLoadout--;
            if (displayLoadout < 0)
            {
                displayLoadout = _controller.Loadouts.Count-1;
            }
            if (displayLoadout == currentLoadout)
            {
                PrevSetup();
            }
            ChangeMenu();
        }

        private bool CheckResources()
        {
            if (HighLogic.LoadedSceneIsEditor)
                return true;
            var kerbal = FlightGlobals.ActiveVessel.rootPart.protoModuleCrew[0];
            if (!kerbal.HasEffect("RepairSkill"))
            {
                ScreenMessages.PostScreenMessage("Only Kerbals with repair skills (engineers, mechanics) can reconfigure modules!", 5f,
                    ScreenMessageStyle.UPPER_CENTER);
                return false;
            }

            var allResources = true;
            var missingResources = "";
            //Check that we have everything we need.
            var count = _controller.ResCosts.Count;
            for(int i = 0; i < count; ++i)
            {
                var r = _controller.ResCosts[i];
                if (!HasResource(r))
                {
                    allResources = false;
                    missingResources += "\n" + r.Ratio + " " + r.ResourceName;
                }
            }
            if (!allResources)
            {
                ScreenMessages.PostScreenMessage("Missing resources to change module:" + missingResources, 5f,
                    ScreenMessageStyle.UPPER_CENTER);
                return false;
            }
            //Since everything is here...
            for (int i = 0; i < count; ++i)
            {
                var r = _controller.ResCosts[i];
                TakeResources(r);
            }
            return true;
        }

        private bool HasResource(ResourceRatio resInfo)
        {
            var resourceName = resInfo.ResourceName;
            var needed = resInfo.Ratio;
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
                if(whp == part)
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
            var needed = resInfo.Ratio;
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

        private bool _postLoad = false;

        public void FixedUpdate()
        {
            if (!_postLoad)
            {
                if (_controller.Loadouts.Count > 2)
                {
                    _postLoad = true;
                    NextSetup();
                }
            }
        }

        public void ChangeMenu()
        {
            Events["NextSetup"].guiName = (bayName + " Next " + _controller.typeName).Trim();
            Events["PrevSetup"].guiName = (bayName + " Prev. " + _controller.typeName).Trim();
            Fields["curTemplate"].guiName = (bayName + " Active " + _controller.typeName).Trim();
            curTemplate = _controller.Loadouts[currentLoadout].ConverterName;
            Events["LoadSetup"].guiName =
                (bayName + " " + curTemplate + "=>" + _controller.Loadouts[displayLoadout].ConverterName).Trim();

            MonoUtilities.RefreshContextWindows(part);
        }

        private int displayLoadout;
        private ModuleSwapControllerNew _controller;

        public override void OnStart(StartState state)
        {
            _controller = part.FindModuleImplementing<ModuleSwapControllerNew>();
            GameEvents.OnAnimationGroupStateChanged.Add(SetModuleState);
            displayLoadout = currentLoadout;
            ConfigureLoadout();
        }

        public void OnDestroy()
        {
            GameEvents.OnAnimationGroupStateChanged.Remove(SetModuleState);
        }


        private void SetModuleState(ModuleAnimationGroup module, bool enable)
        {
            if (module != null && module.part != part)
                return;

            if (HighLogic.LoadedSceneIsFlight)
            {
                EnableMenus(enable);
            }
        }

        private void EnableMenus(bool enable)
        {
            Events["NextSetup"].active = enable;
            Events["PrevSetup"].active = enable;
            Events["LoadSetup"].active = enable;
            MonoUtilities.RefreshContextWindows(part);
        }

        private void ConfigureLoadout()
        {
            _controller.ApplyLoadout(currentLoadout, moduleIndex,isConverter);
        }
    }
}