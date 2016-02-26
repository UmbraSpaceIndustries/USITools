namespace USITools
{
    public class ModuleAquaticSAS : PartModule
    {
        public ModuleReactionWheel SAS;

        public override void OnAwake()
        {
            SAS = part.FindModuleImplementing<ModuleReactionWheel>();
        }

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            if (SAS != null)
            {
                vessel.checkSplashed();
                if (!vessel.Splashed && SAS.operational)
                {
                    SAS.Deactivate(null);
                }
            }
        }
    }
}