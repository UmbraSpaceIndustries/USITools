using Experience;
using KSP.Localization;

namespace USITools
{
    public class GeologySkill : ExperienceEffect
    {
        public GeologySkill(ExperienceTrait parent) : base(parent)
        {
        }

        public GeologySkill(ExperienceTrait parent, float[] modifiers) : base(parent, modifiers)
        {
        }

        protected override float GetDefaultValue()
        {
            return 0f;
        }

        protected override string GetDescription()
        {
            return Localizer.Format("#LOC_USI_Tools_Geology");//"Experience sifting valuable resources out of planetary regolit"
        }
    }
}