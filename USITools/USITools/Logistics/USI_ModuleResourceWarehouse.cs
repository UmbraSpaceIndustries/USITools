using KSP.Localization;
namespace USITools
{
    [KSPModule("USI Warehouse")]
    public class USI_ModuleResourceWarehouse : PartModule
    {
        [KSPField(guiName = "#LOC_USI_PlanetaryWarehouse", guiActive = true, guiActiveEditor = true, isPersistant = true), UI_Toggle(disabledText = "#LOC_USI_UIOFF", enabledText = "#LOC_USI_UION")]//Planetary Warehouse//Off//On
        public bool soiTransferEnabled = false;

        [KSPField(guiName = "#LOC_USI_LocalWarehouse", guiActive = true, guiActiveEditor = true, isPersistant = true), UI_Toggle(disabledText = "#LOC_USI_UIOFF", enabledText = "#LOC_USI_UION")]//Local Warehouse
        public bool localTransferEnabled = true;

        // Info about the module in the Editor part list
        public override string GetInfo()
        {
            return Localizer.Format("#LOC_USI_Tools_MRW_info");//"Stores shareable resources"
        }
    }
}
