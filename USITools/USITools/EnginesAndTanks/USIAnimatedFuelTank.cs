using System;
using UnityEngine;

namespace USITools
{
    public class USIAnimatedFuelTank : PartModule
    {
        [KSPField] public string inflateAnimationName = "Inflate";

        public Animation InflateAnimation
        {
            get { return part.FindModelAnimators(inflateAnimationName)[0]; }
        }

        public override void OnStart(StartState state)
        {
            try
            {
                InflateAnimation[inflateAnimationName].layer = 2;
                base.OnStart(state);
            }
            catch (Exception ex)
            {
                print("ERROR IN USIAnimatedFuelTank - " + ex.Message);
            }
        }

        public void FixedUpdate()
        {
            var totCap = 0d;
            var usedCap = 0d;

            foreach (var r in part.Resources.list)
            {
                totCap += r.maxAmount;
                usedCap += r.amount;
            }

            var aniPercent = usedCap/totCap;
            InflateAnimation[inflateAnimationName].speed = 0;
            InflateAnimation[inflateAnimationName].normalizedTime = (float)aniPercent;
            InflateAnimation.Play((inflateAnimationName));
        }
    }
}