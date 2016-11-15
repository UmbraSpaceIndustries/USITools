using System.Collections.Generic;

namespace USITools
{
    public class ConverterInfo
    {
        public Part HostPart { get; set; }
        public List<BaseConverter> Converters { get; }
        public List<IEfficiencyBonusProvider> BonusProviders { get; }
        public List<ModuleSwappableConverter> SwapConverters { get; }
        public ConverterInfo(Part p)
        {
            Converters = new List<BaseConverter>();
            BonusProviders = new List<IEfficiencyBonusProvider>();
            SwapConverters = new List<ModuleSwappableConverter>();
            HostPart = p;
            Converters.AddRange(p.FindModulesImplementing<BaseConverter>());
            BonusProviders.AddRange(p.FindModulesImplementing<IEfficiencyBonusProvider>());
            SwapConverters.AddRange(p.FindModulesImplementing<ModuleSwappableConverter>());
        }
    }
}