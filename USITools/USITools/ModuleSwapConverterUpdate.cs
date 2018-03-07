
using System.Collections.Generic;
using Assets._UI5.Rendering.Scripts;
using UnityEngine;


namespace USITools
{
    public class ModuleSwapConverterUpdate : VesselModule
    {
        private double lastCheck;
        private double checkTime = 5f;
        private int loadout = -1;

        private List<ConverterPart> _converters;

        private void SetupParts()
        {
            _converters = new List<ConverterPart>();
            var count = vessel.parts.Count;
            for(int i = 0; i < count; ++i)
            {
                var p = vessel.parts[i];
                if (p.FindModulesImplementing<BaseConverter>().Count > 0)
                    _converters.Add(new ConverterPart(p));
            }
        }

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            if (Time.timeSinceLevelLoad - lastCheck < checkTime)
                return;

            //Only do a converter check if the situation has changed.
            //One would be the completed load of all modules.  Just don't
            //thrash this too many times.
            if (_converters == null || _converters.Count == 0)
            {
                SetupParts();
                lastCheck = Time.timeSinceLevelLoad;
                CalculateStatus();
            }
        }


        public void CalculateStatus()
        {
            var cCount = _converters.Count;
            for (int i = 0; i < cCount; ++i)
            {
                var conPart = _converters[i];
                var pCount = conPart.Converters.Count;
                for (int z = 0; z < pCount; ++z)
                {
                    var eBon = 1f;
                    if (conPart.SwapBays.Count > 0)
                    {
                        var loadout = 0;
                        var sameBays = 0;
                        var bCount = conPart.SwapBays.Count;
                        for (int q = 0; q < bCount; ++q)
                        {
                            loadout = conPart.SwapBays[q].currentLoadout;
                            if (conPart.SwapBays[q].currentLoadout == z)
                                sameBays++;
                            conPart.Converters[loadout].EnableModule();
                        }
                        if(z!= loadout)
                            conPart.Converters[z].DisableModule();
                        eBon = sameBays;
                    }
                    conPart.Converters[z].SetEfficiencyBonus("SwapBay", eBon);
                }
            }
        }
    }
}