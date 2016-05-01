//using System;
//using UnityEngine;

//namespace USITools
//{
//    public class USI_Balloon : PartModule
//    {
//        [KSPField(isPersistant = true)]
//        public float GasDensity = 1f;

//        [KSPField]
//        public float BuoyancyRating = .005f;

//        [KSPField]
//        public float BurstingDensity = .0001f;

//        [KSPField]
//        public float DensityMultiplier = 1f;

//        [KSPField(guiActive = true, guiName = "Gas Density")]
//        public string DensityString = "";

//        [KSPField(guiActive = true, guiName = "Control Amount")]
//        public string ControlString = "";

//        [KSPField(isPersistant = true)]
//        public float ControlAmount = 0.01f;

//        public override void OnStart(StartState state)
//        {
//            part.force_activate();
//        }

//        [KSPEvent(guiActive = true, guiName = "Expand Gas (float up)")]
//        public void DecreaseDensity()
//        {
//            GasDensity -= ControlAmount;
//            if (GasDensity <= 0)
//                GasDensity = 0;
//        }

//        [KSPEvent(guiActive = true, guiName = "Compress Gas (float down)")]
//        public void IncreaseDensity()
//        {
//            GasDensity += ControlAmount;
//            if (GasDensity > 1)
//                GasDensity = 1;
//        }

//        [KSPEvent(guiActive = true, guiName = "Finer Control")]
//        public void IncreaseControl()
//        {
//            ControlAmount *= .1f;
//        }

//        [KSPEvent(guiActive = true, guiName = "Broader Control")]
//        public void DecreaseControl()
//        {
//            ControlAmount *= 10f;
//        }


//        [KSPAction("Expand Gas (float up)")]
//        public void DecreaseDensityAction(KSPActionParam param)
//        {
//            DecreaseDensity();
//        }

//        [KSPAction("Compress Gas (float down)")]
//        public void IncreaseDensityAction(KSPActionParam param)
//        {
//            IncreaseDensity();
//        }

//        [KSPAction("Finer Control")]
//        public void IncreaseControlAction(KSPActionParam param)
//        {
//            IncreaseControl();
//        }

//        [KSPAction("Broader Control")]
//        public void DecreaseControlAction(KSPActionParam param)
//        {
//            DecreaseControl();
//        }

//        public override void OnUpdate()
//        {
//            if (vessel != null)
//            {
//                try
//                {
//                    DensityString = String.Format("{0:0.00}%", GasDensity * 100);
//                    ControlString = String.Format("{0:0.00}%", ControlAmount * 100);
//                }
//                catch (Exception ex)
//                {
//                    print("[BALLOON] Error in OnUpdate - " + ex.Message);
//                }
//            }
//        }

//        public override void OnFixedUpdate()
//        {
//            try
//            {
//                //If we go too high, we explode.
//                if ((float)vessel.atmDensity <= BurstingDensity)
//                {
//                    print("Baloon exploded");
//                    if (part.parent != null)
//                    {
//                        part.decouple();
//                    }
//                    part.explode();
//                }

//                //Limit vertical accelleration
//                if (vessel.verticalSpeed > GetTerminalVelocity())
//                {
//                    return;
//                }

//                //Too heavy?
//                if (vessel.GetTotalMass() > BuoyancyRating)
//                    return;

//                //We first look at the difference between atmospheric density
//                //and the density in our balloon.  This will determine the upward thrust.
//                //If the inner density is higher than the outer density then
//                //our net is zero.
//                var adjDensity = (float)vessel.atmDensity * DensityMultiplier;
//                //A cap
//                if (adjDensity > 1f)
//                    adjDensity = 1f;

//                var netDensity = adjDensity - GasDensity;
//                if (netDensity <= 0)
//                {
//                    return; //No upward thrust)
//                }
//                //We should now have a float of 0-100 which would correspond to how many
//                //G's we can pull floating up.
//                //We now adjust that based on the flight characteristics.
//                var Buoyancy = 1 - (vessel.GetTotalMass() / BuoyancyRating);  //How many tons we can lift


//                //Otherwise, we just go up!
//                var gravity = (-FlightGlobals.getGeeForceAtPosition(part.rigidbody.worldCenterOfMass));
//                //0-1 G
//                var buoyantForce = gravity + (gravity*Buoyancy*adjDensity);
//                part.Rigidbody.AddForceAtPosition(buoyantForce, part.rigidbody.worldCenterOfMass, ForceMode.Force);
//            }
//            catch (Exception ex)
//            {
//                print("[BALLOON] Error in OnFixedUpdate - " + ex.Message);
//            }
//        }

//        public double GetTerminalVelocity()
//        {
//            if (vessel.mainBody == null) 
//                return 0;
//            var airDensity = vessel.atmDensity;
//            var mass = vessel.GetTotalMass();
//            var drag = part.maximum_drag*airDensity; //*FlightGlobals.DragMultiplier*airDensity);
//            var termVel = Math.Sqrt(2 * FlightGlobals.getGeeForceAtPosition(vessel.CoM).magnitude * mass / drag);
//            //print(String.Format("D:{0} DM:{1}", part.maximum_drag, FlightGlobals.DragMultiplier));
//            //print(String.Format("A:{0} M:{1} D:{2} TV:{3}", airDensity, mass, drag, termVel));
//            return termVel;
//        }
//    }
//}