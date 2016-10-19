using System.Linq;

namespace USITools
{
    public class ModuleBallast : PartModule
    {
        [KSPField]
        public string ResourceName = "";

        [KSPField(guiName = "Ballast", isPersistant = true, guiActive = true, guiActiveEditor = true), UI_FloatRange(stepIncrement = 1f, maxValue = 100f, minValue = 0f)]
        public float ballastPercent = 0f;

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            var res = part.Resources.Get(ResourceName);
            if (res == null)
                return;
            
            vessel.checkSplashed();
            if (!vessel.Splashed)
            {
                res.amount = 0f;
                return;
            }

            if (res != null)
            {
                res.amount = res.maxAmount * ballastPercent / 100f;
            }

        }


    }
}