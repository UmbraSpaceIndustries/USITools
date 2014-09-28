using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace USITools
{
    public class USI_WiggleEngine : PartModule
    {
        [KSPField] 
        public int vectorAdjustment = 0;

        [KSPField] 
        public int thrustAdjustment = 0;


        public override void OnFixedUpdate()
        {
            //We will adjust two things - the thrust limiter, and add some random force to the part.
            var r = new Random();
            float t = ((float)r.Next(-thrustAdjustment, thrustAdjustment)) / 100f;
            float eThrust = 0;

            var modE = part.FindModuleImplementing<ModuleEngines>();
            if (modE != null && modE.EngineIgnited)
            {
                var adj = modE.thrustPercentage + (t * modE.thrustPercentage);
                if (adj > 100) adj = 100;
                if (adj < 0) adj = 0;
                modE.thrustPercentage += adj;
                eThrust = modE.finalThrust;
            }

            var modFx = part.FindModuleImplementing<ModuleEngines>();
            if (modFx != null && modFx.EngineIgnited)
            {
                var adj = modFx.thrustPercentage + (t * modFx.thrustPercentage);
                if (adj > 100) adj = 100;
                if (adj < 0) adj = 0;
                modFx.thrustPercentage += adj;
                eThrust = modFx.finalThrust;
            }

            float spd = (float)Math.Max(vessel.horizontalSrfSpeed, vessel.verticalSpeed);

            if (spd > 0 && eThrust > 0)
            {
                float x = ((float)r.Next(-vectorAdjustment, vectorAdjustment)) / 10000f * spd * eThrust;
                float y = ((float)r.Next(-vectorAdjustment, vectorAdjustment)) / 10000f * spd * eThrust;
                float z = ((float)r.Next(-vectorAdjustment, vectorAdjustment)) / 10000f * spd * eThrust;
                part.rigidbody.AddForce(new Vector3(x, y, z));

            }
        }
    }
}
