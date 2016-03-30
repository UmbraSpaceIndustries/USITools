using UnityEngine;

public class ModuleStructuralNode : PartModule
{
    public Transform structTransform;

    [KSPField]
    public string attachNodeName = "bottom";

    [KSPField]
    public string rootObject = "Fairing";

    [KSPField] 
    public bool spawnManually = false;

    [KSPField(isPersistant = true)] 
    public bool spawnState = false;

    [KSPField]
    public bool reverseVisibility = false;

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
            CheckDisplay();
        }
    }

    void LateUpdate()
    {
        if (!HighLogic.LoadedSceneIsEditor)
            return;
        if (structTransform == null)
            return;

        CheckDisplay();
    }

    public void SpawnStructure()
    {
        spawnState = true;
    }

    public void DespawnStructure()
    {
        spawnState = false;
    }

    private void CheckDisplay()
    {
        var attachNode = part.findAttachNode(attachNodeName);
        if (attachNode == null)
            return;

        bool showStructure;
        //We have two workflows.  Manual and Automatic spawning.
        if (spawnManually)
        {
            showStructure = spawnState;
        }
        else
        {
            showStructure = attachNode.attachedPart != null;
        }
        if (reverseVisibility)
            showStructure = !showStructure;

        structTransform.gameObject.SetActive(showStructure);        
    }
}

