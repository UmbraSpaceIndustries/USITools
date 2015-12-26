using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSPAchievements;
using UnityEngine;
using Random = System.Random;

namespace USITools
{
    public class USI_PulseDrive : PartModule
    {
        [KSPField]
        public string transformName = "thrustTransform";

        [KSPField(isPersistant =  true)]
        public int CurrentFuelIndex;


        [KSPField]
        public string shockAnimationName = "shockAnimation";

        [KSPField] 
        public int cartridgeYield = 1000000;

        [KSPField] 
        public int maxPulseTime = 3;

        [KSPField] 
        public double minPulseTime = 0.5;

        [KSPField] 
        public double particleLife = 0.12d;

        [KSPField]
        public double powerFactor = 2d;

        [KSPField]
        public double densityMultiplier = 250d;

        [KSPField] 
        public double heatMultiplier = 5;

        [KSPField]
        public float powerCurve = 1.0f;

        [KSPField] 
        public string fuelList;

        [KSPField]
        public float animationSpeed;

        [KSPField(guiActive = true)] 
        public string Fuel = "none";

        [KSPField(guiActive = true)]
        public string Thrust = "none";

        public bool pulseASAP = false;
        private bool pulseNow = false;

        private float lastThrottle;

        [KSPEvent(guiActive = true, active = true, guiName = "Previous Fuel")]
        public void PrevFuel()
        {
            CurrentFuelIndex -= 1;
            if (CurrentFuelIndex < 0)
                CurrentFuelIndex = Fuels.Count() -1;
            Fuel = Fuels[CurrentFuelIndex].name;
        }

        [KSPEvent(guiActive = true, active = true, guiName = "Next Fuel")]
        public void NextFuel()
        {
            CurrentFuelIndex += 1;
            if (CurrentFuelIndex >= Fuels.Count())
                CurrentFuelIndex = 0;
            Fuel = Fuels[CurrentFuelIndex].name;
        }

        [KSPEvent(guiActive = true, active = true, guiName = "Fire Manually")]
        public void ManualPulse()
        {
            pulseASAP=true;
        }
        [KSPAction("Fire Manually")] 
        public void ManualPulseAction(KSPActionParam param)
        {
            ManualPulse();
        }

        public double lastCheck;
        public double lastParticleCheck;
        private KSPParticleEmitter[] eList;
        public PartResourceDefinition[] Fuels;
        private ResourceBroker broker;

        
        
        public override void OnAwake()
        {
            eList = part.GetComponentsInChildren<KSPParticleEmitter>();
            broker = new ResourceBroker();
            var fl = fuelList.Split(';');
            Fuels = new PartResourceDefinition[fl.Count()];
            for(int i = 0; i < fl.Count(); i++)
            {
                var res = PartResourceLibrary.Instance.GetDefinition(fl[i]);
                if (res != null)
                    Fuels[i] = res;
            }
            CurrentFuelIndex = 0;
            Fuel = Fuels[CurrentFuelIndex].name;
            base.OnAwake();
        }

        public Animation ShockAnimation
        {
            get
            {
                return part.FindModelAnimators(shockAnimationName)[0];
            }
        }

        private void PlayAnimation()
        {
            ShockAnimation[shockAnimationName].speed = animationSpeed;
            ShockAnimation.Play(shockAnimationName);
        }

        private bool Deployed()
        {
            var ani = part.FindModuleImplementing<USIAnimation>();
            if (ani == null)
                return true;

            //Some special checks - if we're in atmo, automatically retract.
            //This is for the Medusa.

            if(vessel.atmDensity > ResourceUtilities.FLOAT_TOLERANCE && ani.isDeployed)
                ani.RetractModule();

            return ani.isDeployed;
        }

        public override void OnFixedUpdate()
        {
            // Don't surprise the user with a manual pulse lurking
            // eg because the engine was switched off
            pulseNow = pulseASAP; pulseASAP = false;

            if (!HighLogic.LoadedSceneIsFlight)
                return;

            if (!Deployed()) //For variants like the Medusa
                return;

            //Setup
            Transform t = part.FindModelTransform(transformName);
            if (lastCheck < ResourceUtilities.FLOAT_TOLERANCE)
                lastCheck = Planetarium.GetUniversalTime();

            var currentThrust = GetPulseThrust();
            var pulseInterval = GetPulseInterval(lastThrottle);

            //Check Emitters - turn them off if it's time.
            if (Planetarium.GetUniversalTime() > lastParticleCheck + particleLife)
                ToggleEmmitters(false);

            //Check for fuel
            if (!HasFuel())
                return;

            //See if it's time to fire.
            var eng = part.FindModuleImplementing<ModuleEngines>();
            if (Planetarium.GetUniversalTime() > (pulseNow ? lastCheck + minPulseTime : lastCheck + pulseInterval)) {
                lastThrottle = eng.currentThrottle;
                if (!eng.isActiveAndEnabled || (!pulseNow && lastThrottle < 0.01)) {
                    ToggleEmmitters(false);
                    return;
                }
                ConsumeFuel();
                lastCheck = Planetarium.GetUniversalTime();
                lastParticleCheck = lastCheck;
                ToggleEmmitters(true);
                PlayAnimation();
            } else {
                // Keep the pulse since it's viable but too early
                pulseASAP = pulseNow;
            }

            //Where are we in the curve?
            var pulseCurve = new FloatCurve();
            pulseCurve.Add(0, (1 - powerCurve) / 2);
            pulseCurve.Add(0.5f,  powerCurve);
            pulseCurve.Add(1, (1 - powerCurve) / 2);
            var curveTime = (float)(Planetarium.GetUniversalTime() - lastCheck) / (float)pulseInterval;
            var thrustAmount = (float)(currentThrust * pulseCurve.Evaluate(curveTime));

            part.rigidbody.AddForceAtPosition(-t.forward * thrustAmount, t.position, ForceMode.Force);
            part.AddThermalFlux(thrustAmount * heatMultiplier);
        }

        private double GetPulseInterval(float throttle)
        {
            var step = (1-throttle)*(maxPulseTime - minPulseTime);
            return step + minPulseTime;
        }

        private void ConsumeFuel()
        {
            var res = Fuels[CurrentFuelIndex];
            broker.RequestResource(part, res.name, 1, 1, "");
        }

        private bool HasFuel()
        {
            var res = Fuels[CurrentFuelIndex];
            return broker.AmountAvailable(part, res.name, 1, "") > 1;
        }

        private float GetPulseThrust()
        {
            var res = Fuels[CurrentFuelIndex];
            var thrust = Math.Pow(res.density * densityMultiplier, powerFactor) * cartridgeYield;
            Thrust = thrust.ToString();
            return (float)thrust;
        }

        private void ToggleEmmitters(bool state)
        {
            foreach (KSPParticleEmitter e in eList)
            {
                e.emit = state;
                e.enabled = state;
            }
        }
    }
}
