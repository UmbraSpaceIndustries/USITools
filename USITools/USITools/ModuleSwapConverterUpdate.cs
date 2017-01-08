using System;
using System.Collections.Generic;
using System.Linq;

namespace USITools
{
    public class ModuleSwapConverterUpdate : VesselModule
    {
        private double lastCheck;
        private double checkTime = 5f;

        private List<ConverterPart> _converters;

        private void SetupParts()
        {
            _converters = new List<ConverterPart>();
            foreach (var p in vessel.Parts)
            {
                if (p.FindModulesImplementing<BaseConverter>().Any())
                    _converters.Add(new ConverterPart(p));
            }
        }

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            if (Planetarium.GetUniversalTime() - lastCheck < checkTime)
                return;

            if (_converters == null || _converters.Count == 0)
                SetupParts();

            lastCheck = Planetarium.GetUniversalTime();

            var Count = _converters.Count;
            for (int i = 0; i < Count; ++i)
            {
                var totBon = 1f;
                var conPart = _converters[i];
                var cCount = conPart.Converters.Count;
                for (int z = 0; z < cCount; ++z)
                {
                    var eBon = 1f;
                    if (conPart.SwapBays.Any())
                    {
                        float sameBays = conPart.SwapBays.Count(b => b.currentLoadout == z);
                        eBon = sameBays;
                    }
                    conPart.Converters[z].SetEfficiencyBonus("SwapBay", eBon);
                }
            }
        }

    }
}