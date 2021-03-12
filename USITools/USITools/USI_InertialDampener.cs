namespace USITools
{
    
    public class USI_InertialDampener : PartModule
    {
        [KSPField(isPersistant = true)]
        public bool isActive = false;

        #region KSP actions and events
        [KSPAction(guiName = "Disable ground tether")]
        public void DisableDampenAction(KSPActionParam param)
        {
            if (isActive)
            {
                ToggleDampen(false);
            }
        }

        [KSPAction(guiName = "Enable ground tether")]
        public void EnableDampenAction(KSPActionParam param)
        {
            if (!isActive)
            {
                ToggleDampen(true);
            }
        }

        [KSPAction(guiName = "Toggle ground tether")]
        public void ToggleDampenAction(KSPActionParam param)
        {
            ToggleDampen(!isActive);
        }

        [KSPEvent(
            guiName = "Ground Tether",
            guiActive = true,
            externalToEVAOnly = true,
            guiActiveEditor = false,
            active = true,
            guiActiveUnfocused = true,
            unfocusedRange = 3.0f)]
        public void ToggleDampenEvent()
        {
            ToggleDampen(!isActive);
        }
        #endregion

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (!HighLogic.LoadedSceneIsFlight)
                return;

            SetActive(isActive);

            SyncDampeners();
            ModuleStabilization.onStabilizationToggle
                .Fire(part.vessel, isActive, false);
        }

        public void SetActive(bool isActive)
        {
            this.isActive = isActive;

            Events[nameof(ToggleDampenEvent)].guiName
                = $"Ground tether: {(isActive ? "On" : "Off")}";
            MonoUtilities.RefreshContextWindows(part);
        }

        private void SyncDampeners()
        {
            var dampeners = vessel.FindPartModulesImplementing<USI_InertialDampener>();
            for (int i = 0; i < dampeners.Count; ++i)
            {
                dampeners[i].SetActive(isActive);
            }
        }

        private void ToggleDampen(bool isActive)
        {
            SetActive(isActive);

            ModuleStabilization.onStabilizationToggle
                .Fire(part.vessel, isActive, true);
            SyncDampeners();
        }
    }
}
