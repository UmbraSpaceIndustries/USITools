namespace USITools
{
    public class ModuleFlexiLegController : PartModule
    {
        [KSPField(isPersistant = true)]
        public bool isActiveController = false;

        [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Enable Controller")]
        public void EnableController()
        {
            isActiveController = true;
            ToggleController();
        }

        [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "Disable Controller")]
        public void DisableController()
        {
            isActiveController = false;
            ToggleController();
        }

        public override void OnStart(StartState state)
        {
            ToggleController();
        }

        private void ToggleController()
        {
            Events["EnableController"].guiActive = !isActiveController;
            Events["DisableController"].guiActive = isActiveController;
            MonoUtilities.RefreshContextWindows(part);

            //We should also toggle ALL controllers on this vessel.
            var contList = vessel.FindPartModulesImplementing<ModuleFlexiLegController>();
            foreach (var c in contList)
            {
                c.isActiveController = isActiveController;
            }
        }
    }
}