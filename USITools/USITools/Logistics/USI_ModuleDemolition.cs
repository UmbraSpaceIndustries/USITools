using System;
using System.Collections.Generic;
using KSP.Localization;

namespace USITools
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

        [KSPEvent(active = true, guiActiveUnfocused = true, externalToEVAOnly = true, guiName = "#LOC_USI_Scrappart",
            unfocusedRange = 5f)]//Scrap part
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
        
        [KSPEvent(active = true, guiActiveUnfocused = true, externalToEVAOnly = true, guiName = "#LOC_USI_Scrapsection",
            unfocusedRange = 5f)]//Scrap section
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

        [KSPEvent(active = true, guiActiveUnfocused = true, externalToEVAOnly = true, guiName = "#LOC_USI_Scrapvessel",
            unfocusedRange = 5f)]//Scrap vessel
        public void ScrapVessel()
        {
            _demoParts = new List<Part>();
            var count = vessel.parts.Count;
            for (int i = 0; i < count; ++ i)
            {
                _demoParts.Add(vessel.parts[i]);
            }
            DestroyParts();
        }

        private void AddRecursiveParts(Part part, List<Part> list)
        {
            if (part.children.Count > 0)
            {
                var count = part.children.Count;
                for(int i = 0; i < count; ++i)
                {
                    var p = part.children[i];
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

            var count = _demoParts.Count;
            for (int i = 0; i < count; ++i)
            {
                var part = _demoParts[i];
                if (!_blackList.Contains(part.partName))
                {
                    var res = PartResourceLibrary.Instance.GetDefinition(ResourceName);
                    double resAmount = part.mass/res.density*Efficiency;
                    ScreenMessages.PostScreenMessage(
                        String.Format(Localizer.Format("#LOC_USI_Tools_msg8","{0}","{1:0.00}","{2}"), part.partInfo.title,
                            resAmount, ResourceName), 5f, ScreenMessageStyle.UPPER_CENTER);//You disassemble the "" into "" units of ""
                    PushResources(ResourceName, resAmount);
                }
                part.decouple();
                part.explosionPotential = 0f;
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
            var count = vessels.Count;
            for (int i = 0; i < count; ++i)
            {
                var v = vessels[i];
                //Put recycled stuff into recycleable places
                var parts = v.parts;
                var pCount = parts.Count;
                for(int x = 0; x < pCount; ++x)
                {
                    var p = parts[x];
                    if (p == part || !p.Modules.Contains("USI_ModuleRecycleBin"))
                        continue;

                    if (!p.Resources.Contains(resourceName))
                        continue;

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
            if (amount > 1f)
            {
                ScreenMessages.PostScreenMessage(String.Format(Localizer.Format("#LOC_USI_Tools_msg9", "{0:0}","{1}"), amount, ResourceName), 5f, ScreenMessageStyle.UPPER_CENTER);//""" units of "" were lost due to lack of recycle space"
            }
        }
    }
}