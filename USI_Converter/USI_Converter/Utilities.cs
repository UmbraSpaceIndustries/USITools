/**
 * Umbra Space Industries Resource Converter
 * 
 * This is a derivative work of Thunder Aerospace Corporation's library for  
 * the Kerbal Space Program, which is (c) 2013, Taranis Elsu, who retains the copyright for 
 * all unmodified portions of this work.  Enhancements and extensions are (c) 2014 Bob Palmer.  
 *  
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 * 
 * This code is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
 * creative commons license. See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>
 * for full details.
 * 
 * Attribution — You are free to modify this code, so long as you mention that the resulting
 * work is based upon or adapted from this code.
 * 
 * Non-commercial - You may not use this work for commercial purposes.
 * 
 * Share Alike — If you alter, transform, or build upon this work, you may distribute the
 * resulting work only under the same or similar license to the CC BY-NC-SA 3.0 license.
 * 
 * Note that Thunder Aerospace Corporation and Umbra Space Industries are ficticious entities 
 * created for entertainment purposes. It is in no way meant to represent a real entity.
 *  Any similarity to a real entity is purely coincidental.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace USI
{
    internal class Tuple<T1, T2>
    {
        internal T1 Item1 { get; set; }
        internal T2 Item2 { get; set; }

        internal Tuple(T1 item1, T2 item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }

        public Tuple()
        {
        }
    }

    internal class Tuple<T1, T2, T3> : Tuple<T1, T2>
    {
        internal T3 Item3 { get; set; }

        internal Tuple(T1 item1, T2 item2, T3 item3) : base(item1, item2)
        {
            this.Item3 = item3;
        }

        public Tuple()
        {
        }
    }

    public static class Utilities
    {
        private const int SecondsPerMinute = 60;
        private const int SecondsPerHour = 60*SecondsPerMinute;
        private const int SecondsPerDay = 6*SecondsPerHour;
        private const string ElectricChargeResName = "ElectricCharge";

        public static string Electricity
        {
            get { return ElectricChargeResName; }
        }

        public static double ElectricityMaxDeltaTime
        {
            get { return 1d*(1/TimeWarp.fixedDeltaTime); }
        }

        public static double MaxDeltaTime
        {
            get { return SecondsPerHour*(1/TimeWarp.fixedDeltaTime); }
        }

        private static IEnumerable<Part> FindAllFuelLineConnectedSourceParts(this Part refPart, List<Part> allParts, bool outRes)
        {
            return allParts.OfType<FuelLine>()
                           .Where(fl => fl.target != null && fl.parent != null && outRes ? fl.parent == refPart : fl.target == refPart)
                           .Select(fl => outRes ? fl.target : fl.parent);
        }

        public static List<Part> FindPartsInSameResStack(this Part refPart, List<Part> allParts, HashSet<Part> searchedParts, bool outRes)
        {
            var partList = new List<Part> {refPart};
            searchedParts.Add(refPart);
            foreach (var attachNode in refPart.attachNodes.Where(an => an.attachedPart != null && !searchedParts.Contains(an.attachedPart) && an.attachedPart.fuelCrossFeed && an.nodeType == AttachNode.NodeType.Stack))
            {
                partList.AddRange(attachNode.attachedPart.FindPartsInSameResStack(allParts, searchedParts, outRes));
            }
            foreach (var fuelLinePart in refPart.FindAllFuelLineConnectedSourceParts(allParts, outRes).Where(flp => !searchedParts.Contains(flp)))
            {
                partList.AddRange(fuelLinePart.FindPartsInSameResStack(allParts, searchedParts, outRes));
            }
            return partList;
        }

        public static List<Part> FindPartsInSameStage(this Part refPart, List<Part> allParts, bool outRes)
        {
            var partList = allParts.Where(vPart => vPart.inverseStage == refPart.inverseStage).ToList();
            partList.AddRange(refPart.FindAllFuelLineConnectedSourceParts(allParts, outRes));
            return partList;
        }

        public static string FormatValue(double ratio, int p, bool humanReadable)
        {
            const string unitPerSec = " U/sec";
            var digits = p;
            if (humanReadable)
            {
                if (ratio >= 1)
                {
                    return Math.Round(ratio, digits).ToString(CultureInfo.CurrentCulture) + unitPerSec;
                }
                var minRatio = ratio*SecondsPerMinute;
                if (minRatio >= 1)
                {
                    return string.Format("{0:F2} U/min", Math.Round(minRatio, digits));
                }
                var hourRatio = ratio*SecondsPerHour;
                if (hourRatio >= 1)
                {
                    return string.Format("{0:F2} U/hour", Math.Round(hourRatio, digits));
                }
                var dayRatio = ratio*SecondsPerDay;
                return string.Format("{0:F2} U/day", Math.Round(dayRatio, digits));
            }
            while (digits < 14 && (int) Math.Floor(ratio*Math.Pow(10, digits)) == 0)
            {
                digits++;
            }
            return Math.Round(ratio, digits).ToString(CultureInfo.CurrentCulture) + unitPerSec;
        }

        public static double GetValue(ConfigNode config, string name, double currentValue)
        {
            double newValue;
            if (config.HasValue(name) && double.TryParse(config.GetValue(name), out newValue))
            {
                return newValue;
            }
            return currentValue;
        }
    }
}