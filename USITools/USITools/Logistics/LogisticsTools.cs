using System;
using System.Collections.Generic;
using UnityEngine;
using USITools.Logistics;

namespace USITools
{
    
    public class LogisticsTools
    {
        public const float PHYSICS_RANGE = 2000f;
        public static double GetRange(Vessel a, Vessel b)
        {
            var posCur = a.GetWorldPos3D();
            var posNext = b.GetWorldPos3D();
            return Vector3d.Distance(posCur, posNext);
        }

        public static List<Vessel> GetNearbyVessels(float range, bool includeSelf, Vessel thisVessel, bool landedOnly = true)
        {
            try
            {
                var vessels = new List<Vessel>();
                var count = FlightGlobals.Vessels.Count;
                for (int i = 0; i < count; ++i)
                {
                    var v = FlightGlobals.Vessels[i];
                    if (v.mainBody == thisVessel.mainBody
                        && (v.Landed || !landedOnly || v == thisVessel))
                    {
                        if (v == thisVessel && !includeSelf)
                            continue;

                        if (GetRange(thisVessel, v) < range)
                        {
                            vessels.Add(v);
                        }
                    }
                }
                return vessels;
            }
            catch (Exception ex)
            {
                Debug.Log(String.Format("[MKS] - ERROR in GetNearbyVessels - {0}", ex.Message));
                return new List<Vessel>();
            }
        }

        public static List<Part> GetRegionalWarehouses(Vessel vessel, string module)
        {
            var pList = new List<Part>();
            var vList = GetNearbyVessels((float)LogisticsSetup.Instance.Config.MaintenanceRange, true, vessel, false);
            var count = vList.Count;
            for(int i = 0; i < count; ++i)
            {
                var v = vList[i];
                var parts = v.parts;
                var pCount = parts.Count;
                for (int x = 0; x < pCount; ++x)
                {
                    Part p = parts[x];
                    if(p.Modules.Contains(module))
                        pList.Add(p);
                }
            }
            return pList;
        }

        public static bool HasCrew(Vessel v, string skill)
        {
            var crew = v.GetVesselCrew();
            var count = crew.Count;
            for (int i = 0; i < count; ++i)
            {
                if (crew[i].experienceTrait.TypeName == skill)
                    return true;
            }
            return false;
        }

        public static bool NearbyCrew(Vessel v, float range, String effect)
        {
            List<Vessel> nearby = GetNearbyVessels(range, true, v, true);
            var count = nearby.Count;
            for (int i = 0; i < count; ++i)
            {
                var vsl = nearby[i];
                var crew = vsl.GetVesselCrew();
                var cCount = crew.Count;
                for (int x = 0; x < cCount; ++x)
                {
                    if(crew[x].HasEffect(effect))
                        return true;
                }
            }
            return false;
        }
    }
}
