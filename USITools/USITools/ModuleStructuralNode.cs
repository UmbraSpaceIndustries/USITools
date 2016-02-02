using UnityEngine;

public class ModuleStructuralNode : PartModule
{
    public Transform structTransform;

    [KSPField]
    public string attachNodeName = "bottom";

    [KSPField]
    public string rootObject = "Fairing";

    public override void OnStart(StartState state)
    {
        structTransform = part.FindModelTransform(rootObject);
        if (structTransform == null)
            return;

        if (state == StartState.Editor)
        {
            structTransform.gameObject.SetActive(false);
        }
        else
        {
            AttachNode node = part.findAttachNode(attachNodeName);
            structTransform.gameObject.SetActive(node.attachedPart != null);
        }
    }

    void LateUpdate()
    {
        if (!HighLogic.LoadedSceneIsEditor)
            return;
        if (structTransform == null)
            return;

        var node = part.findAttachNode(attachNodeName);
        structTransform.gameObject.SetActive(node.attachedPart != null);
    }
}

