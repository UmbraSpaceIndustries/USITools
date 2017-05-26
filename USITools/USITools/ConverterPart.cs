using System.Collections.Generic;

namespace USITools
{
    public class ConverterPart
    {
        public Part HostPart { get; set; }
        public List<IEfficiencyBonusConsumer> Converters { get; set; }
        public List<ModuleSwappableConverter> SwapBays { get; set; }
        public ConverterPart(Part p)
        {
            Converters = new List<IEfficiencyBonusConsumer>();
            SwapBays = new List<ModuleSwappableConverter>();
            HostPart = p;
            Converters.AddRange(p.FindModulesImplementing<IEfficiencyBonusConsumer>());
            SwapBays.AddRange(p.FindModulesImplementing<ModuleSwappableConverter>());
        }
    }
}