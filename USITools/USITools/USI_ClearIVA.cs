using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//NOTE:  This is based on the sfr command module code, which was released 
//       under a CC 3.0 Share Alike Attrib license.  

namespace USITools
{
  public class USI_ClearIVA : PartModule
  {
    private Transform kspParent;
    private Vector3 kspPosition;
    private bool isActive;
    private bool packed;


    public override void OnStart(StartState state)
    {
      base.OnStart(state);
      if (part.internalModel || state != StartState.Editor)
        return;
      try
      {
        part.CreateInternalModel();
        part.internalModel.transform.parent = part.transform;
        part.internalModel.transform.localRotation = new Quaternion(0.0f, 0.7f, -0.7f, 0.0f);
        part.internalModel.transform.localPosition = Vector3.zero;
        ChangeLayer(part.internalModel.transform, 0);
      }
      catch (Exception)
      {
          //Swallowed
      }
        if (state == StartState.Editor) return;
        packed = vessel.packed;
        isActive = vessel.isActiveVessel;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        Exception exception; 
        packed = vessel.packed;

        if (!part.internalModel)
        {
            part.CreateInternalModel();
            part.vessel.SetActiveInternalPart(part);
            kspParent = InternalCamera.Instance.transform.parent;
            if (part.internalModel)
            { 
                part.internalModel.SetVisible(true);
                part.internalModel.transform.parent = part.transform;
                part.internalModel.transform.localRotation = (new Quaternion(0.0f, 0.7f, -0.7f, 0.0f));
                part.internalModel.transform.localPosition = Vector3.zero;
                if (part.protoModuleCrew.Any())
                {
                    part.internalModel.Initialize(part);
                    part.internalModel.enabled = true;
                    using (var mCrew = part.protoModuleCrew.GetEnumerator())
                    {
                        while (mCrew.MoveNext())
                        {
                            var current = mCrew.Current;
                            if (current == null) continue;
                            if (current.seat && !part.vessel.packed)
                            {
                                current.seat.enabled = true;
                                current.seat.SpawnCrew();
                            }
                            if (!current.KerbalRef) continue;
                            current.KerbalRef.enabled = true;
                            current.KerbalRef.kerbalCam.cullingMask = 1;
                        }
                    }
                    using (var mSeat = part.internalModel.seats.GetEnumerator())
                    {
                        while (mSeat.MoveNext())
                        {
                            var current = mSeat.Current;
                            if (current == null) continue;
                            if (current.kerbalRef)
                                current.SpawnCrew();
                            if (current.portraitCamera != null)
                                current.portraitCamera.cullingMask = 65537;
                        }
                    }
                }
            }
        }

        isActive = part.vessel.isActiveVessel;
        if (isActive  && !packed)
        {
            if (part.internalModel && part.protoModuleCrew.Any())
            {
                part.vessel.SetActiveInternalPart(part);
                part.internalModel.Initialize(part);
                using (var seat = part.internalModel.seats.GetEnumerator())
                {
                    while (seat.MoveNext())
                    {
                        var current = seat.Current;
                        if (current == null) continue;
                        current.enabled = true;
                        if (current.portraitCamera)
                        current.portraitCamera.cullingMask = 1;
                    }
                }
                try
                {
                    using (var mCrew = part.protoModuleCrew.GetEnumerator())
                    {
                        while (mCrew.MoveNext())
                        {
                            var current = mCrew.Current;
                            if (current == null) continue;
                            if (current.seat && !part.vessel.packed)
                            {
                                current.seat.enabled = true;
                                current.seat.SpawnCrew();
                            }
                            if (!current.KerbalRef) continue;
                            current.KerbalRef.enabled = true;
                            current.KerbalRef.kerbalCam.cullingMask = 1;
                        }
                    }
                }
                
                catch (Exception ex)
                {
                    exception = ex;
                }
            }
        }
        try
        {
            if (FindCamera("InternalCamera") && part.vessel.isActiveVessel)
            {
                ChangeLayer(part.internalModel.transform, 16);
                part.internalModel.transform.position = (kspPosition);
                part.internalModel.transform.parent = (kspParent);
                part.internalModel.transform.localRotation = (new Quaternion(0.0f, 0.7f, -0.7f, 0.0f));
            }
            else
            {
                ChangeLayer(part.internalModel.transform, 0);
                part.internalModel.SetVisible(true);
                part.internalModel.gameObject.SetActive(true);
                part.internalModel.transform.parent = part.transform;
                part.internalModel.transform.localRotation = (new Quaternion(0.0f, 0.7f, -0.7f, 0.0f));
                part.internalModel.transform.localPosition = Vector3.zero;
            }
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        try
        {
            using (var seat = part.protoModuleCrew.GetEnumerator())
            {
                while (seat.MoveNext())
                {
                    var current = seat.Current;
                    if (current == null) continue;
                    if (current.seat)
                    {
                        current.seat.enabled = (true);
                        if (current.seat.portraitCamera)
                        {
                            (current.seat).portraitCamera.cullingMask = 65537;
                        }
                    }
                    if (!current.KerbalRef) continue;
                    current.KerbalRef.enabled = true;
                    current.KerbalRef.kerbalCam.cullingMask = 65537;
                    if (current.KerbalRef.kerbalCamOverlay)
                    current.KerbalRef.kerbalCamOverlay.layer = 16;
                }
            }
        }
        catch (Exception ex)
        {
            exception = ex;
        }
    }

    


    public static Camera FindCamera(string name)
    {
        return Camera.allCameras.FirstOrDefault(camera => camera.name == name);
    }

      public static void ChangeLayer(Transform part, int layer)
    {
        if (!part)
            return;
        if (!part.GetComponent<Camera>())
            part.gameObject.layer = layer;
        var enumerator = part.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
               ChangeLayer((Transform)enumerator.Current, layer);
        }
        finally
        {
            var disposable = enumerator as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
    }
  
  
  }
}
