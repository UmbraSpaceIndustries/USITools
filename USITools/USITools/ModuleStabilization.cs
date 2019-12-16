using System;
using KSP.Localization;

namespace USITools
{
    public class ModuleStabilization : VesselModule
    {
        public static EventData<Vessel,bool,bool> onStabilizationToggle = new EventData<Vessel,bool,bool>("onStabilizationToggle");
        private bool _stabilized;

        public new void Start()
        {
            onStabilizationToggle.Add(StabilizeVessel);
        }

        public void OnDestroy()
        {
            onStabilizationToggle.Remove(StabilizeVessel);
        }

        private void StabilizeVessel(Vessel v, bool state, bool showMsg)
        {
            if (v != vessel)
                return;

            if (_stabilized == state)
                return;

            var newState = state;
            var msg = Localizer.Format("#LOC_USI_StabilizeReleased");//"Ground tether released!"
            if (newState)
                msg = Localizer.Format("#LOC_USI_StabilizeAttached");//"Ground tether attached!"

            if (!vessel.Landed)
            {
                newState = false;
                msg = Localizer.Format("#LOC_USI_StabilizeFail");//"Must be landed for ground tether to operate!"
            }

            if(showMsg)
                ScreenMessages.PostScreenMessage(msg, 5f,ScreenMessageStyle.UPPER_CENTER);

            _stabilized = newState;
        }

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            if (!_stabilized)
                return;

            if (!vessel.Landed)
                return;

            vessel.permanentGroundContact = true;

            var c = vessel.parts.Count;
            for (int i = 0; i < c; ++i)
            {
                var r = vessel.parts[i].Rigidbody;
                r.angularVelocity *= 0;
                r.velocity *= 0;
            }
        }
    }
}