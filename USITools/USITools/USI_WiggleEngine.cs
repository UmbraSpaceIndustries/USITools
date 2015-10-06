using System;
using System.Collections.Generic;
using System.Text;
using KSPAchievements;
using UnityEngine;
using Random = System.Random;

namespace USITools
{
    public class USI_WiggleEngine : PartModule
    {
        [KSPField] 
        public int vectorAdjustment = 0;

        public override void OnFixedUpdate()
        {
            //add some random force to the part.
            var r = new Random();
            float eThrust = 0;

            var modE = part.FindModuleImplementing<ModuleEngines>();
            if (modE != null && modE.EngineIgnited)
            {
                eThrust = modE.finalThrust;
            }

            var modFx = part.FindModuleImplementing<ModuleEngines>();
            if (modFx != null && modFx.EngineIgnited)
            {
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


