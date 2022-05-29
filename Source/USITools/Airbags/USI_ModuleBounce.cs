using System.Collections;
using System.Collections.Generic;

//Public Domain by SQUAD

public class USI_ModuleBounce : PartModule
{
    [KSPField(isPersistant = false)]
    public float bounciness = 0.5f;

    public override void OnStart(PartModule.StartState state)
    {
        ModuleBounceCollider bounce = gameObject.GetComponent<ModuleBounceCollider>();

        if (bounce == null)
        {
            bounce = gameObject.AddComponent<ModuleBounceCollider>();
            bounce.bounciness = bounciness;
            bounce.part = part;
        }
    }
}