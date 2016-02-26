namespace USITools
{
    public class ModuleAquaticEngine : PartModule
    {
        public ModuleEnginesFX engine;

        public override void OnAwake()
        {
            engine = part.FindModuleImplementing<ModuleEnginesFX>();
        }

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return; 
            
            if (engine != null)
            {
                vessel.checkSplashed();
                if (!vessel.Splashed && engine.EngineIgnited)
                {
                    engine.Shutdown();
                }
            }
        }
    }
}