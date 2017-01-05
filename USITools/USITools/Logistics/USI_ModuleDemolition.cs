using System;
using System.Collections.Generic;
using System.Linq;

namespace KolonyTools
{
    public class USI_ModuleDemolition : PartModule
    {
        //Another super hacky module.

        [KSPField]
        public float EVARange = 5f;

        [KSPField]
        public string ResourceName = "Recyclables";

        [KSPField]
        public string Menu = "Scrap";
        
        [KSPField] 
        public float Efficiency = 0.8f;

        [KSPEvent(active = true, guiActiveUnfocused = true, externalToEVAOnly = true, guiName = "Scrap part",
            unfocusedRange = 5f)]
        public void ScrapPart()
        {
            _demoParts = new List<Part>();
            if (part.parent != null)
            {
                _demoParts.Add(part.parent);
            }
            _demoParts.Add(part);
            DestroyParts();
        }

        private List<Part> _demoParts;
        
        [KSPEvent(active = true, guiActiveUnfocused = true, externalToEVAOnly = true, guiName = "Scrap section",
            unfocusedRange = 5f)]
        public void ScrapSection()
        {
            _demoParts = new List<Part>();

            if (part.parent != null)
            {
                AddRecursiveParts(part.parent, _demoParts);
            }
            else
            {
                _demoParts.Add(part);
            }
            DestroyParts();
        }

        [KSPEvent(active = true, guiActiveUnfocused = true, externalToEVAOnly = true, guiName = "Scrap vessel",
            unfocusedRange = 5f)]
        public void ScrapVessel()
        {
            _demoParts = new List<Part>();
            foreach (var p in vessel.parts)
            {
                _demoParts.Add(p);
            }
            DestroyParts();
        }

        private void AddRecursiveParts(Part part, List<Part> list)
        {
            if (part.children.Any())
            {
                foreach (var p in part.children)
                {
                    AddRecursiveParts(p, list);
                    list.Add(p);
                }
            }
            else
            {
                list.Add(part);
            }
        }

        private List<String> _blackList = new List<string> { "PotatoRoid", "UsiExplorationRock" };
        private void DestroyParts()
        {
            if (_demoParts == null)
                return;
            foreach (var part in _demoParts)
            {
                if (!_blackList.Contains(part.partName))
                {
                    var res = PartResourceLibrary.Instance.GetDefinition(ResourceName);
                    double resAmount = part.mass/res.density*Efficiency;
                    ScreenMessages.PostScreenMessage(
                        String.Format("You disassemble the {0} into {1:0.00} units of {2}", part.partInfo.title,
                            resAmount, ResourceName), 5f, ScreenMessageStyle.UPPER_CENTER);
                    PushResources(ResourceName, resAmount);
                }
                part.decouple();
                part.explode();

            }
        }
        
        public override void OnStart(StartState state)
        {
            Events["ScrapPart"].unfocusedRange = EVARange;
            Events["ScrapPart"].guiName = Menu + " part";
            Events["ScrapSection"].unfocusedRange = EVARange;
            Events["ScrapSection"].guiName = Menu + " attached parts"; 
            Events["ScrapVessel"].unfocusedRange = EVARange;
            Events["ScrapVessel"].guiName = Menu + " vessel"; 
            base.OnStart(state);
        }

        private void PushResources(string resourceName, double amount)
        {
            var vessels = LogisticsTools.GetNearbyVessels(2000, true, vessel, false);
            foreach (var v in vessels)
            {
                //Put recycled stuff into recycleable places
                foreach (var p in v.parts.Where(vp => vp != part && vp.Modules.Contains("USI_ModuleRecycleBin")))
                {
                    if (p.Resources.Contains(resourceName))
                    {
                        var partRes = p.Resources[resourceName];
                        var partNeed = partRes.maxAmount - partRes.amount;
                        if (partNeed > 0 && amount > 0)
                        {
                            if (partNeed > amount)
                            {
                                partNeed = amount;
                            }
                            partRes.amount += partNeed;
                            amount -= partNeed;
                        }
                    }
                }
            }
            if (amount > 1f)
            {
                ScreenMessages.PostScreenMessage(String.Format("{0:0} units of {1} were lost due to lack of recycle space", amount, ResourceName), 5f, ScreenMessageStyle.UPPER_CENTER);
            }
        }
    }
}