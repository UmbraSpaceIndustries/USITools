using System;

namespace USITools
{
    [Obsolete("Use a class derived from AbstractSwapOption instead.")]
    public class ModuleSwapOption : ModuleHarvesterSwapOption { }

    public class ModuleHarvesterSwapOption
        : AbstractSwapOption<ModuleResourceHarvester_USI>
    {
        public override void ApplyConverterChanges(ModuleResourceHarvester_USI converter)
        {
            converter.Efficiency = Efficiency;
            converter.ResourceName = ResourceName;

            base.ApplyConverterChanges(converter);
        }
    }
}
