using System.Linq;
using UnityEngine;

namespace USITools
{
    public static class NodeUtilities
    {
        public static void SpawnStructures(Part thisPart, AttachNode thisNode)
        {
            var structList = thisPart.FindModulesImplementing<ModuleStructuralNode>();
            foreach (var s in structList.Where(s => s.attachNodeName == thisNode.id))
            {
                s.SpawnStructure();
            }
        }

        public static void DetachPart(Part thisPart)
        {
            thisPart.parent = null;
            thisPart.attachJoint.DestroyJoint();
            thisPart.children.Clear();

            foreach (var an in thisPart.attachNodes)
            {
                an.attachedPart = null;
            }

            thisPart.topNode.attachedPart = null;
        }

        public static void MovePart(Part thisPart, Vector3 offset)
        {
            if (thisPart.Rigidbody != null)
                thisPart.transform.position += offset;

            thisPart.UpdateOrgPosAndRot(thisPart.vessel.rootPart);

            foreach (var p in thisPart.children)
                MovePart(p, offset);
        }

        public static void SwapLinks(Part thisPart, Part oldPart, Part newPart)
        {
            if (thisPart.parent == oldPart)
                thisPart.parent = newPart;

            if (thisPart.topNode.attachedPart == oldPart)
                thisPart.topNode.attachedPart = newPart;

            if (thisPart.attachJoint != null)
            {
                if (thisPart.attachJoint.Child == oldPart || thisPart.attachJoint.Parent == oldPart)
                {
                    thisPart.attachJoint.DestroyJoint();
                }
            }

            foreach (var an in thisPart.attachNodes.Where(an => an.attachedPart == oldPart))
            {
                an.attachedPart = newPart;
            }

            if (!thisPart.children.Contains(oldPart))
                return;

            thisPart.children.Remove(oldPart);
            thisPart.children.Add(newPart);
        }

        public static float GetPartThickness(Part thisPart)
        {
            var diff = thisPart.attachNodes[0].position - thisPart.attachNodes[1].position;
            return (diff).magnitude;
        }

        public static AttachNode GetLinkingNode(Part p1, Part p2)
        {
            return p1.attachNodes.FirstOrDefault(an => an.attachedPart == p2);
        }
    }
}