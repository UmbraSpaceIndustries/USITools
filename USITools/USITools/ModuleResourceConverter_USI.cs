using System;

namespace USITools
{
    public class ModuleResourceConverter_USI : ModuleResourceConverter
    {
        private float baseEfficiency;

        protected override void PreProcessing()
        {
            baseEfficiency = EfficiencyBonus;
            print($"BASE: {Efficiency:0.00000000}");
            EfficiencyBonus *= GetCrewBonus();
            print($"BONUS: {GetCrewBonus():0.00000000}");
            print($"EFFICIENCY: {GetCrewBonus():0.00000000}");
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