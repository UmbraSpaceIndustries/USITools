namespace USITools
{
    public class ModuleSwapOption : PartModule
    {
        [KSPField]
        public float Efficiency = 1;

        [KSPField]
        public string ResourceName = "";

        [KSPField]
        public string ConverterName = "";

        [KSPField]
        public string StartActionName = "";

        [KSPField]
        public string StopActionName = "";
    }
}