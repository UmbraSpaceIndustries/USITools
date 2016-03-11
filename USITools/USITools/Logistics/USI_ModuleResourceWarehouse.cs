namespace KolonyTools
{
    public class USI_ModuleResourceWarehouse : PartModule
    {       
        [KSPField(isPersistant = true)] 
        public bool transferEnabled = true;

        [KSPEvent(guiName = "Disable Warehouse", active = true, guiActive = true, guiActiveEditor = true)]
        public void DisableTransfer()
        {
            ToggleTransfer(false);
        }

        [KSPEvent(guiName = "Enable Warehouse", active = false, guiActive = true, guiActiveEditor = true)]
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
            return ".\n" + // Extra space at the top because the module name is so long...
                "Shares resources between nearby warehouses\n\n" +
                "Range: " + USITools.Logistics.LogisticsSetup.Instance.Config.ScavangeRange + "m";
        }
    }
}
