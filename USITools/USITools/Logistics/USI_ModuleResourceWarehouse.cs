namespace KolonyTools
{
    [KSPModule("Resource Warehouse")]
    public class USI_ModuleResourceWarehouse : PartModule
    {       
        [KSPField(isPersistant = true)] 
        public bool transferEnabled = true;

        [KSPEvent(guiName = "Disable Warehouse", active = true, guiActive = true)]
        public void DisableTransfer()
        {
            ToggleTransfer(false);
        }

        public override string GetInfo()
        {
            return base.GetInfo() + "\nWarehouse full of resources\n";
        }

        [KSPEvent(guiName = "Enable Warehouse", active = true, guiActive = false)]
        public void EnableTransfer()
        {
            ToggleTransfer(true);
        }

        private void ToggleTransfer(bool state)
        {
            transferEnabled = state;
            Events["DisableTransfer"].guiActive = state;
            Events["EnableTransfer"].guiActive = !state; 
            MonoUtilities.RefreshContextWindows(part);
        }

        public override void OnStart(StartState state)
        {
            Events["DisableTransfer"].guiActive = transferEnabled;
            Events["EnableTransfer"].guiActive = !transferEnabled; 
           
        }
    }
}