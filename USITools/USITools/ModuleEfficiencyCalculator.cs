using System;
using System.Collections.Generic;
using System.Linq;

namespace USITools
{
    public class ModuleEfficiencyCalculator : VesselModule
    {
        private double lastCheck;
        private double checkTime = 5f;

        private List<ConverterInfo> _converters;

        private void SetupParts()
        {
            _converters = new List<ConverterInfo>();
            foreach (var p in vessel.Parts)
            {
                if(p.FindModulesImplementing<BaseConverter>().Any())
                    _converters.Add(new ConverterInfo(p));
            }
        }

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            if (Math.Abs(lastCheck - Planetarium.GetUniversalTime()) < checkTime)
                return;

            if(_converters == null)
                SetupParts();

            lastCheck = Planetarium.GetUniversalTime();

            var Count = _converters.Count;
            for (int i = 0; i < Count; ++i)
            {
                var totBon = 1f;
                var conPart = _converters[i];
                foreach (var bon in conPart.BonusProviders)
                {
                    totBon *= bon.GetEfficiencyBonus();
                }
                var cCount = conPart.Converters.Count;
                for(int z = 0; z < cCount; ++z)
                {
                    var eBon = 1f;
                    if (conPart.SwapConverters.Any())
                    {
                        float sameBays = conPart.SwapConverters.Count(b => b.currentLoadout == z);
                        eBon = sameBays;
                    }
                    conPart.Converters[z].EfficiencyBonus = totBon * eBon;
                }
            }
        }
    }
}