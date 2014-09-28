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

using System.Collections.Generic;
using System.Linq;

namespace USI
{
    public static class PartExtensions
    {
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
    }
}