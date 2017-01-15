namespace USITools
{
    
    public class USI_InertialDampener : PartModule
    {
        [KSPField(isPersistant = true)]
        public bool isActive = false;


        [KSPEvent(guiName = "Toggle Ground Tether", guiActive = true, externalToEVAOnly = true, guiActiveEditor = false, active = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public void ToggleDampen()
        {
            isActive = !isActive;
            ModuleStabilization.onStabilizationToggle.Fire(part.vessel,isActive,true);
            SyncDampeners();
        }

        public void Start()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            SyncDampeners();
            ModuleStabilization.onStabilizationToggle.Fire(part.vessel,isActive,false);
        }

        private void SyncDampeners()
        {
            var dList = vessel.FindPartModulesImplementing<USI_InertialDampener>();
            var c = dList.Count;
            for (int i = 0; i < c; ++i)
            {
                dList[i].isActive = isActive;
            }
        }
    }
}
