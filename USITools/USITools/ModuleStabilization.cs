using System;

namespace USITools
{
    public class ModuleStabilization : VesselModule
    {
        [KSPField(isPersistant = true)]
        public double posX;
        [KSPField(isPersistant = true)]
        public double posY;
        [KSPField(isPersistant = true)]
        public double posZ;

        private Vector3d nuPos; 

        public static EventData<Vessel,bool,bool> onStabilizationToggle = new EventData<Vessel,bool,bool>("onStabilizationToggle");
        private bool _stabilized;

        public void Start()
        {
            onStabilizationToggle.Add(StabilizeVessel);
            nuPos = vessel.GetWorldPos3D();
            if (Math.Abs(posX + posY + posZ) < ResourceUtilities.FLOAT_TOLERANCE)
            {
                nuPos = new Vector3d(posX,posY,posZ);
            }
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

            var pos = vessel.GetWorldPos3D();
            posX = pos.x;
            posY = pos.y;
            posZ = pos.z;
            nuPos = new Vector3d(posX, posY, posZ);

            var newState = state;
            var msg = "Ground tether released!";
            if (newState)
                msg = "Ground tether attached!";

            if (!vessel.Landed)
            {
                newState = false;
                msg = "Must be landed for ground tether to operate!";
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


            vessel.SetPosition(nuPos);

            var c = vessel.parts.Count;
            for(int i = 0; i < c; ++i)
            {
                var r = vessel.parts[i].Rigidbody;
                r.angularVelocity *= 0;
                r.velocity *= 0;
            }
        }
    }
}