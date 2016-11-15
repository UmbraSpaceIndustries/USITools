namespace USITools
{
    public class ModuleResourceHarvester_USI : ModuleResourceHarvester
    {
        protected override void PostProcess(ConverterResults result, double deltaTime)
        {
            base.PostProcess(result, deltaTime);
            if (result.TimeFactor >= ResourceUtilities.FLOAT_TOLERANCE
                && !status.EndsWith("load"))
            {
                statusPercent = 0d; //Force a reset of the load display.
            }
        }
    }
}