namespace USITools
{
    public class ModuleAquaticRCS : PartModule
    {
        public ModuleRCS RCS;

        public override void OnAwake()
        {
            RCS = part.FindModuleImplementing<ModuleRCS>();
        }

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return; 
            
            if (RCS != null)
            {
                vessel.checkSplashed();
                if (!vessel.Splashed && RCS.rcsEnabled)
                {
                    RCS.Disable();
                }
            }
        }
    }
}