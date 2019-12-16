using Experience;
using KSP.Localization;

namespace USITools
{
    public class FundsBoost : ExperienceEffect
    {
        public FundsBoost(ExperienceTrait parent) : base(parent)
        {
        }

        public FundsBoost(ExperienceTrait parent, float[] modifiers) : base(parent, modifiers)
        {
        }

        protected override float GetDefaultValue()
        {
            return 0f;
        }

        protected override string GetDescription()
        {
            return Localizer.Format("#LOC_USI_Tools_Funds");//"A Kolonist that increases funds"
        }
    }
}