using System;

namespace USITools
{
    public class USI_Falloff : PartModule
    {
        //a part falls off at velocity
        [KSPField] 
        public float threshold = 5f;

        public override void OnUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;
            if (vessel != null)
            {
                vessel.checkLanded();
                if(Math.Max(vessel.horizontalSrfSpeed,vessel.verticalSpeed) > threshold)
                {
                    if (part.parent != null)
                    {
                        part.decouple();
                    }
                }
            }
        }
    }


    public class USI_DropTank : PartModule
    {
        //tank decouples when out of resources.  May have hilarious results.
        [KSPField] 
        public bool explode = true;

        [KSPField] 
        public float threshold = 0.0001f;

        [KSPField(isPersistant =true)]
        public bool dropEnabled = true;


        [KSPEvent(guiActive = true, guiActiveEditor = true,  guiName = "Disable Auto-Drop")]
        public void DropToggleEvent()
        {
            dropEnabled = !dropEnabled;
            UpdateDropMenu();
        }


        protected void UpdateDropMenu()
        {
            if (dropEnabled)
                Events["DropToggleEvent"].guiName = "Disable Auto-Drop";
            else
                Events["DropToggleEvent"].guiName = "Enable Auto-Drop";
            MonoUtilities.RefreshContextWindows(part);
        }


        public override void OnStartFinished(StartState state)
        {
            base.OnStartFinished(state);
            UpdateDropMenu();
        }

        public override void OnUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;
            if (vessel != null)
            {
                bool drop = true;
                var rCount = part.Resources.Count;
                for (int i = 0; i < rCount; ++i)
                {
                    var res = part.Resources[i];

                    if (res.amount >= threshold || !dropEnabled)
                    {
                        drop = false;
                    }
                }
                if (drop)
                {
                    if (part.parent != null)
                    {
                        part.decouple();
                    }
                    if (explode)
                    {
                        part.explosionPotential = 0f;
                        part.explode();
                    } 

                }
            }
        }
    }
}
