namespace USITools
{
    [KSPModule("USI Warehouse")]
    public class USI_ModuleResourceWarehouse : PartModule
    {       
        [KSPField(isPersistant = true)] 
        public bool transferEnabled = true;

        [KSPEvent(guiName = "Disable Warehouse", active = false, guiActive = true, guiActiveEditor = true)]
        public void DisableTransfer()
        {
            ToggleTransfer(false);
        }

        [KSPEvent(guiName = "Enable Warehouse", active = true, guiActive = true, guiActiveEditor = true)]
        public void EnableTransfer()
        {
            ToggleTransfer(true);
        }

        private void ToggleTransfer(bool state)
        {
            transferEnabled = state;
            Events["DisableTransfer"].active = state;
            Events["EnableTransfer"].active = !state; 
            MonoUtilities.RefreshContextWindows(part);
        }

        public override void OnStart(StartState state)
        {
            Events["DisableTransfer"].active = transferEnabled;
            Events["EnableTransfer"].active = !transferEnabled; 
        }

        // Info about the module in the Editor part list
        public override string GetInfo()
        {
            return "Stores shareable resources";
        }
    }
}
