using UnityEngine;

namespace USITools
{
    public class USI_DeployableMeshModule : PartModule
    {
        private GameObject _mesh;

        [KSPField]
        public string MeshName = string.Empty;

        [KSPField]
        public double ShowAtPercentage = 0d;

        public override void OnAwake()
        {
            base.OnAwake();

            if (ShowAtPercentage > 1d)
            {
                ShowAtPercentage = 1d;
            }
            if (ShowAtPercentage < 0d)
            {
                ShowAtPercentage = 0d;
            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                if (string.IsNullOrEmpty(MeshName))
                {
                    Debug.LogError($"[USITools] {ClassName}: {part.partInfo.title} is missing mesh name.");
                }
                else
                {
                    _mesh = part.gameObject.GetChild(MeshName);
                    if (_mesh == null)
                    {
                        Debug.LogError($"[USITools] {ClassName}: {part.partInfo.title} has no child game object named {MeshName}.");
                    }
                }
            }
        }

        public void SetActive(bool state)
        {
            if (_mesh != null)
            {
                if (_mesh.activeSelf != state)
                {
                    _mesh.SetActive(state);
                }
            }
        }
    }
}
