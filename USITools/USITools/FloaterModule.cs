using System;
using UnityEngine;

namespace FloaterTools
{
    public class FloaterModule : PartModule
    {
        #region Fields
        [KSPField]
        public String deployAnimationName = "Deploy";

        [KSPField]
        public float deployAnimationSpeed = 1f;

        [KSPField(isPersistant = true)]
        public float maxBuoyancy = 50f;

        [KSPField]
        public float buoyancyChangeInterval = 10f;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "#LOC_USI_Buoyancy"), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 5f)]//Buoyancy
        public float buoyancyPercentageWhenDeployed = 100f;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "#LOC_USI_Pumpspeed"), UI_FloatRange(minValue = 10f, maxValue = 100f, stepIncrement = 10f)]//Pump speed
        public float pumpPower = 100f;

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "#LOC_USI_Currentbuoyancy")]//Current buoyancy
        public float totalCurrentBuoyancy = 0f;

        [KSPField(isPersistant = true)]
        public float buoyancyWhenStowed = 0f;

        [KSPField(isPersistant = true)]
        public bool isDeployed = false;

        [KSPField]
        public string FireSpitterBuoyancyFieldName = "buoyancyForce";

        [KSPField]
        public string FireSpitterBuoyancyModuleName = "FSbuoyancy";

        private float targetInflationPercentage
        {
            get { return isDeployed ? buoyancyPercentageWhenDeployed / 100f : 0f; }
        }

        #endregion

        #region Method overrides
        public override void OnStart(StartState state)
        {
            DeployAnimation.wrapMode = WrapMode.ClampForever;
            DeployAnimation.Play(deployAnimationName);

            DeployAnimation[deployAnimationName].normalizedTime = isDeployed ? targetInflationPercentage : 0f;

            UpdateEvents();

            base.OnStart(state);
        }


        public void FixedUpdate()
        {
            SetPumpSpeed();
            SyncFireSpitterBuoyancy();
        }

        #endregion

        #region Firespitter helper methods
        private void SyncFireSpitterBuoyancy()
        {
            totalCurrentBuoyancy = currentInflationPercentage * maxBuoyancy;

            //Don't wanna needlessly change physics
            if (FireSpitterBuoyancy != totalCurrentBuoyancy)
                FireSpitterBuoyancy = totalCurrentBuoyancy;
        }

        private float FireSpitterBuoyancy
        {
            get
            {
                return (float)part.Modules[FireSpitterBuoyancyModuleName].Fields.GetValue(FireSpitterBuoyancyFieldName);
            }
            set
            {
                part.Modules[FireSpitterBuoyancyModuleName].Fields.SetValue(FireSpitterBuoyancyFieldName, value);
            }
        }

        #endregion


        /// <summary>
        /// Set visibility of Events
        /// </summary>
        private void UpdateEvents()
        {
            //Firespitter slider should be disabled everywhere
            part.Modules[FireSpitterBuoyancyModuleName].Fields[FireSpitterBuoyancyFieldName].guiActiveEditor = false;
            part.Modules[FireSpitterBuoyancyModuleName].Fields[FireSpitterBuoyancyFieldName].guiActive = false;

            Events["DeployEvent"].active = !isDeployed;
            Events["RetractEvent"].active = isDeployed;
        }

        /// <summary>
        /// Sets a new deployed state
        /// </summary>
        private void SetDeployedState(bool newState)
        {
            isDeployed = newState;
            UpdateEvents();
        }

        private float currentInflationPercentage
        {
            get { return Mathf.Clamp01(DeployAnimation[deployAnimationName].normalizedTime); }
        }



        #region Animation support methods

        private Animation DeployAnimation
        {
            get { return part.FindModelAnimators(deployAnimationName)[0]; }
        }

        private void SetPumpSpeed()
        {
            var speed = deployAnimationSpeed * (pumpPower / 100f);

            var iTargetBuoyancy = (int)(targetInflationPercentage * 100);
            var iCurrentBuoyancy = (int)(currentInflationPercentage * 100);

            //Need to do this comparison with integers beacuse floating point can cause weirdness 
            if (iTargetBuoyancy == iCurrentBuoyancy)
                speed = 0f;             //current inflation level matches target. pump can be shut off
            else if (iTargetBuoyancy > iCurrentBuoyancy)
                speed = speed * -1;     //current inflation level is higher than target. pump needs to be reversed

            DeployAnimation[deployAnimationName].speed = speed;

            //normalizedTime will not stay between 0 and 1 by itself, and cause animation problems if not manually set
            DeployAnimation[deployAnimationName].normalizedTime = currentInflationPercentage;
        }
        #endregion

        #region Right click menu events
        [KSPEvent(active = false, guiActive = true, guiActiveEditor = true, guiActiveUnfocused = true, guiName = "Deploy", unfocusedRange = 5.0f)]
        public void DeployEvent()
        {
            SetDeployedState(true);
        }

        [KSPEvent(active = false, guiActive = true, guiActiveEditor = true, guiActiveUnfocused = true, guiName = "Retract", unfocusedRange = 5.0f)]
        public void RetractEvent()
        {
            SetDeployedState(false);
        }
        #endregion

        #region Bindable actions
        [KSPAction("Deploy")]
        public void DeployAction(KSPActionParam param)
        {
            SetDeployedState(true);
        }

        [KSPAction("Retract")]
        public void RetractAction(KSPActionParam param)
        {
            SetDeployedState(false);
        }

        [KSPAction("Toggle")]
        public void ToggleAction(KSPActionParam param)
        {
            SetDeployedState(!isDeployed);
        }

        [KSPAction("More Buoyancy")]
        public void IncreaseBuoyancyAction(KSPActionParam param)
        {
            buoyancyPercentageWhenDeployed = Math.Min(100f, buoyancyPercentageWhenDeployed + buoyancyChangeInterval);
        }
        [KSPAction("Less Buoyancy")]
        public void DecreaseBuoyancyAction(KSPActionParam param)
        {
            buoyancyPercentageWhenDeployed = Math.Max(0f, buoyancyPercentageWhenDeployed - buoyancyChangeInterval);
        }
        #endregion

    }
}
