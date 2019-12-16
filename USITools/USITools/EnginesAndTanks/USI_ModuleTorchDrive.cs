using System;
using System.Collections.Generic;
using KSP.UI.Screens.DebugToolbar.Screens.GamePlay;
using UnityEngine;

namespace USITools
{
    public class EngineWrapper
    {
        private float _maxThrust;
        private float _isp;
        private Func<double, double> _propFunc;
        private List<Transform> _thrustList;

        public void SetThrottle(float throttle)
        {
            _e.currentThrottle = (throttle/100f);
            _e.requestedThrottle = _e.currentThrottle;
            _e.UpdateThrottle();
        }

        public float GetThrottle()
        {
            return _e.currentThrottle;
        }

        public float MaxThrust
        {
            get { return _maxThrust; }
        }

        public float ThrustPercent
        {
            get { return _e.thrustPercentage; }
        }

        public double GetPropellant(double mass)
        {
            if (CheatOptions.InfinitePropellant)
                return 1d;

            return _propFunc(mass);
        }

        public List<Transform> GetTransforms
        {
            get { return _thrustList ?? (_thrustList = new List<Transform>()); }
        }

        public float GetISP()
        {
            return _e.realIsp;
        }

        public void UpdateFx(double throttle)
        {
            _e.currentThrottle = (float) (throttle/100d);
            _e.FXUpdate();
        }

        private ModuleEngines _e;
        private ModuleEnginesFX _x;

        public EngineWrapper(Part p)
        {
            _e = p.FindModuleImplementing<ModuleEngines>();
            _x = p.FindModuleImplementing<ModuleEnginesFX>();

            _maxThrust = _e.maxThrust;
            _thrustList = _e.thrustTransforms;

            if (_x == null)
            {
                _propFunc = _e.RequestPropellant;
            }
            else
            {
                _propFunc = _x.RequestPropellant;
            }
        }
    }

    public class USI_ModuleTorchDrive : PartModule
    {
        [KSPField(guiActive = true, guiName = "#LOC_USI_TorchDrive", isPersistant = true),//Torch Throttle (%)
         UI_FloatRange(controlEnabled = true, maxValue = 100f, minValue = 1f, stepIncrement = .01f,
             scene = UI_Scene.Flight)] public float torchThrottle = 0;

        private EngineWrapper _engine;

        public override void OnStart(StartState state)
        {
            _engine = new EngineWrapper(part);
            TimeWarp.GThreshold = double.MaxValue;
        }

        private Vector3d GetThrustVector()
        {
            var v = new Vector3d();
            foreach (Transform tf in _engine.GetTransforms)
            {
                v += -1 * tf.forward;
            }
            v = v.normalized;
            return v;
        }

        private const double GRAVITY = 9.82;

        private double GetFuelMass(Vector3d dv)
        {
            var mass = vessel.GetTotalMass() - 1 / ((Math.Exp(dv.magnitude / (GRAVITY * _engine.GetISP()))) / vessel.GetTotalMass());
            return mass;
        }

        public bool IsWarp()
        {
            return TimeWarp.WarpMode == TimeWarp.Modes.HIGH
                   && TimeWarp.CurrentRate > TimeWarp.MaxPhysicsRate;
        }

        public void FixedUpdate()
        {
            if (!IsWarp())
            {
                torchThrottle = _engine.GetThrottle()*100f;
                return;
            }

            _engine.SetThrottle(torchThrottle);
            _engine.UpdateFx(torchThrottle);

            var thrust = _engine.MaxThrust * (torchThrottle / 100f) * (_engine.ThrustPercent / 100f);
            var thrustVector = GetThrustVector()*thrust;
            var netAccel = thrustVector / vessel.GetTotalMass();
            var dV = netAccel * TimeWarp.deltaTime;

            double fuelMass = GetFuelMass(dV);
            double engineRequestResult = _engine.GetPropellant(fuelMass);
            if (engineRequestResult > 0.999) //99.9% of resources present to account for rounding issues
            {
                vessel.orbit.UpdateFromStateVectors(
                vessel.orbit.getRelativePositionAtUT(Planetarium.GetUniversalTime()),
                vessel.orbit.vel + dV.xzy,
                vessel.orbit.referenceBody,
                Planetarium.GetUniversalTime());
            }
        }
    }


    //public class ModuleEnginesTorchFX : ModuleEnginesFX
    //{
    //    public override void OnStart(StartState state)
    //    {
    //        base.OnStart(state);
    //        //Allow warping with throttle
    //        var idx = TimeWarp.fetch.warpRates.Length - 1;
    //        TimeWarp.fetch.maxPhysicsRate_index = idx;
    //    }

    //    private Vector3d GetThrustVector()
    //    {
    //        var v = new Vector3d();
    //        foreach (Transform tf in thrustTransforms)
    //        {
    //            v += -1 * tf.forward;
    //        }
    //        v = v.normalized;
    //        return v;
    //    }

    //    private const double GRAVITY = 9.82;

    //    private double GetFuelMass(Vector3d dv)
    //    {
    //        var mass = vessel.GetTotalMass() - 1 / (Math.Exp(dv.magnitude / (GRAVITY * realIsp)) / vessel.GetTotalMass());
    //        return mass;
    //    }

    //    public new bool TimeWarping()
    //    {
    //        return false;
    //    }

    //    public new void FixedUpdate()
    //    {
    //        base.FixedUpdate();
    //        ////Handle the cases when we are at warp
    //        //if (TimeWarping())
    //        //{
    //        //    FXUpdate();
    //        //    var thrust = maxThrust*currentThrottle*100;
    //        //    var thrustVector = GetThrustVector() * thrust;
    //        //    var netAccel = thrustVector / vessel.GetTotalMass();
    //        //    var dV = netAccel*TimeWarp.deltaTime;

    //        //    double fuelMass = GetFuelMass(dV);
    //        //    double engineRequestResult = RequestPropellant(fuelMass);
    //        //    if (engineRequestResult > 0.999) //99.9% of resources present to account for rounding issues
    //        //    {
    //        //        vessel.orbit.UpdateFromStateVectors(
    //        //        vessel.orbit.getRelativePositionAtUT(Planetarium.GetUniversalTime()),
    //        //        vessel.orbit.vel + dV.xzy,
    //        //        vessel.orbit.referenceBody,
    //        //        Planetarium.GetUniversalTime());
    //        //    }
    //        //}
    //    }

    //    void OnDestroy()
    //    {
            
    //    }           
    //}
}