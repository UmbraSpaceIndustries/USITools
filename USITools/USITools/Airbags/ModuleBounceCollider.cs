using System;
using UnityEngine;

public class ModuleBounceCollider : MonoBehaviour
{
    public float bounciness = 0.5f;
    public Part part;

    Vector3 lastVel = Vector3.zero;
    void FixedUpdate()
    {
        if (GetComponent<Rigidbody>())
            lastVel = GetComponent<Rigidbody>().velocity;
    }

    private void OnCollisionEnter(Collision col)
    {
        try
        {
            Vector3 normal = Vector3.zero;
            foreach (ContactPoint c in col.contacts)
                normal += c.normal;
            normal.Normalize();
            Vector3 inVelocity = lastVel;
            Vector3 outVelocity = bounciness * (-2f * (Vector3.Dot(inVelocity, normal) * normal) + inVelocity);
            GetComponent<Rigidbody>().velocity = outVelocity;
        }
        catch (Exception ex)
        {
            print("[AB] Error in OnCollissionEnter - " + ex.Message);
        }
    }
}