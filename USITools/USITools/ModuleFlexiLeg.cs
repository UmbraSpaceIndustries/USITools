using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace USITools
{
    public class ModuleFlexiLegServo : PartModule
    {
        [KSPField] 
        public string menuName = "Angle";

        //A series of transforms and their increments
        [KSPField] 
        public string transformConfig;

        [KSPField(guiName = "Angle", isPersistant = true, guiActive = true, guiActiveEditor = false), UI_FloatRange(stepIncrement = 1f, maxValue = 100f, minValue = 0f)]
        public float servoPercent = 0f;

        [KSPField]
        public float servoSpeed = 0.1f;

        [KSPField] 
        public float minValue = 0f;

        [KSPField] 
        public float maxValue = 150f;

        public float currentValue = 0f;

        public void FixedUpdate()
        {
            CheckRotations(servoSpeed);
        }

        private void CheckRotations(float speed)
        {
            var range = GetRange();
            var goalAngle = minValue + (range * (servoPercent / 100f));
            var currentAngle = minValue + currentValue;

            var netChange = goalAngle - currentAngle;
            if (Math.Abs(netChange) < ResourceUtilities.FLOAT_TOLERANCE)
                return;

            var changeAmount = 0f;
            if (netChange > 0)
            {
                changeAmount = Math.Min(netChange, speed);
            }
            else
            {
                changeAmount = Math.Max(netChange, speed * -1f);
            }
            currentValue += changeAmount;
            SetTransforms(changeAmount);
            //Check for a controller
            UpdateLinkedParts();
        }

        private void UpdateLinkedParts()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;
            var ctr = part.FindModuleImplementing<ModuleFlexiLegController>();
            if (ctr == null)
                return;
            if (ctr.isActiveController)
            {
                var servos = vessel.FindPartModulesImplementing<ModuleFlexiLegServo>();
                foreach (var s in servos)
                {
                    var partVal = GetServoValue(s.menuName);
                    s.servoPercent = partVal;
                }
            }
        }

        private float GetServoValue(string servoName)
        {
            var servos = part.FindModulesImplementing<ModuleFlexiLegServo>();
            foreach (var s in servos)
            {
                if (s.menuName == servoName)
                    return s.servoPercent;
            }
            return 0f;
        }

        private float GetRange()
        {
            return Math.Abs((minValue + 360) - (maxValue + 360));
        }

        public override void OnStart(StartState state)
        {
            transformKeys = new Dictionary<string, Vector3>();
            Fields["servoPercent"].guiName = menuName;
            MonoUtilities.RefreshContextWindows(part);

            var tList = transformConfig.Split(',');
            for (int i = 0; i < tList.Count(); i+=4)
            {
                transformKeys.Add(tList[i], new Vector3(float.Parse(tList[i + 1]), float.Parse(tList[i + 2]), float.Parse(tList[i + 3])));
            }
            currentValue = 0;
            CheckRotations(1000);
        }

        public Dictionary<string, Vector3> transformKeys;

        private void SetTransforms(float m)
        {
            foreach (var t in transformKeys)
            {
                var v = t.Value*m;
                var g = part.FindModelTransform(t.Key);
                g.transform.Rotate(v.x,v.y,v.z);
            }    
        }
    }
}