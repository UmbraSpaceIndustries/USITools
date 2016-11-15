using System;

namespace USITools
{
    public class ModuleResourceConverter_USI : ModuleResourceConverter
    {
        private float baseEfficiency;

        protected override void PreProcessing()
        {
            if (!IsActivated)
                return;

            baseEfficiency = EfficiencyBonus;
            EfficiencyBonus *= GetCrewBonus();
        }

        protected override void PostProcess(ConverterResults result, double deltaTime)
        {
             base.PostProcess(result, deltaTime);
            if (result.TimeFactor >= ResourceUtilities.FLOAT_TOLERANCE
                && !status.EndsWith("load"))
            {
                statusPercent = 0d; //Force a reset of the load display.
            }
            if(baseEfficiency > ResourceUtilities.FLOAT_TOLERANCE)
                EfficiencyBonus = baseEfficiency;
        }
    }
}