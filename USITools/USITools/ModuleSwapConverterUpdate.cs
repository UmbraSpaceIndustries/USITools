
using System.Collections.Generic;
using Assets._UI5.Rendering.Scripts;


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

            if (Planetarium.GetUniversalTime() - lastCheck < checkTime)
                return;

            if (_converters == null || _converters.Count == 0)
                SetupParts();

            lastCheck = Planetarium.GetUniversalTime();

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
                        var sameBays = 0;
                        var bCount = conPart.SwapBays.Count;
                        for (int q = 0; q < bCount; ++q)
                        {
                            if (conPart.SwapBays[q].currentLoadout == z)
                                sameBays++;
                        }
                        eBon = sameBays;
                    }
                    conPart.Converters[z].SetEfficiencyBonus("SwapBay", eBon);
                }
            }
        }

    }
}