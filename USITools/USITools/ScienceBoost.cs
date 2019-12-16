using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Experience;
using KSP.Localization;

namespace USITools
{
    public class ScienceBoost : ExperienceEffect
    {
        public ScienceBoost(ExperienceTrait parent) : base(parent)
        {
        }

        public ScienceBoost(ExperienceTrait parent, float[] modifiers) : base(parent, modifiers)
        {
        }

        protected override float GetDefaultValue()
        {
            return 0f;
        }

        protected override string GetDescription()
        {
            return Localizer.Format("#LOC_USI_Tools_Science");//"A Kolonist that increases science"
        }
    }
}
