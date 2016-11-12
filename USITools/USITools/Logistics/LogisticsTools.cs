using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using USITools.Logistics;

namespace KolonyTools
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
                foreach (var v in FlightGlobals.Vessels.Where(
                    x => x.mainBody == thisVessel.mainBody
                    && (x.Landed || !landedOnly || x == thisVessel)))
                {
                    if (v == thisVessel && !includeSelf) continue;
                    if (GetRange(thisVessel,v) < range)
                    {
                        vessels.Add(v);
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
            foreach (var v in vList)
            {
                foreach (var vp in v.parts.Where(p => p.Modules.Contains(module)))
                {
                    pList.Add(vp);
                }
            }
            return pList;
        }

        public static bool HasCrew(Vessel v, string skill)
        {
            return (v.GetVesselCrew().Any(c => c.experienceTrait.TypeName == skill));
        }

        public static bool NearbyCrew(Vessel v, float range, String skill)
        {
            List<Vessel> nearby = GetNearbyVessels(range, true, v, true);
            return nearby.Any(near=>near.GetVesselCrew().Any(c=>c.experienceTrait.TypeName==skill));
        }
    }
}
