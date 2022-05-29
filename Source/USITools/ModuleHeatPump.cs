namespace USITools
{
    public class ModuleHeatPump : ModuleActiveRadiator
    {
        //A very simple module.  When landed, flux is dumped to a geothermal well

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!IsCooling)
                return;

            if (!vessel.LandedOrSplashed && vessel.radarAltitude > 500d)
            {
                //ScreenMessages.PostScreenMessage("Heat Pumps must be landed to use geothermal wells!", 5f,
                //    ScreenMessageStyle.UPPER_CENTER);
                //Shutdown();
               return;
            }

            //Dump our heat into the geothermal well.
            part.thermalInternalFlux = 0d;
        }
    }
}