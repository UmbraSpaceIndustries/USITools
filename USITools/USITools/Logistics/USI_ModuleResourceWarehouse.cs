namespace USITools
{
    [KSPModule("USI Warehouse")]
    public class USI_ModuleResourceWarehouse : PartModule
    {
        [KSPField(guiName = "Planetary Warehouse", guiActive = true, guiActiveEditor = true, isPersistant = true), UI_Toggle(disabledText = "Off", enabledText = "On")]
        public bool soiTransferEnabled = true;

        [KSPField(guiName = "Local Warehouse", guiActive = true, guiActiveEditor = true, isPersistant = true), UI_Toggle(disabledText = "Off", enabledText = "On")]
        public bool localTransferEnabled = true;

        // Info about the module in the Editor part list
        public override string GetInfo()
        {
            return "Stores shareable resources";
        }
    }
}
