using System.Collections.Generic;

namespace USITools
{
    // For a basic deployable part, the concepts of "deployed" and "retracted"
    //  are simplified as "paid in full" and "partially paid" respectively.
    //  So we can automate all of the deploy/retract actions normally exposed to
    //  and triggered by the player and simplify the UI as a result.
    public class USI_BasicDeployableModule : AbstractDeployModule
    {
        protected List<USI_DeployableMeshModule> _meshes;

        public override void Deploy()
        {
            base.Deploy();
            if (HighLogic.LoadedSceneIsEditor)
            {
                SetMeshVisibility();
            }
        }

        protected override void DepositResources()
        {
            base.DepositResources();

            SetMeshVisibility();
            if (PartialDeployPercentage >= 1d)
            {
                Deploy();
            }
        }

        public override void OnStart(StartState state)
        {
            GetResourceCosts();
            if (_resourceCosts == null || _resourceCosts.Count < 1)
            {
                PartialDeployPercentage = 1d;
            }

            StartDeployed = PartialDeployPercentage >= 1d;

            _meshes = part.FindModulesImplementing<USI_DeployableMeshModule>();
            SetMeshVisibility();

            base.OnStart(state);
        }

        protected override void RefreshPAW()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                base.RefreshPAW();
            }
            else
            {
                Actions[nameof(DeployAction)].active = false;
                Actions[nameof(RetractAction)].active = false;
                Actions[nameof(ToggleAction)].active = false;

                Events[nameof(DeployEvent)].guiActive = false;
                Events[nameof(DeployEvent)].guiActiveEditor = false;
                Events[nameof(RetractEvent)].guiActive = false;
                Events[nameof(RetractEvent)].guiActiveEditor = false;

                if (PartialDeployPercentage >= 1d)
                {
                    Actions[nameof(PayAction)].active = false;
                    Events[nameof(PayEvent)].guiActive = false;
                    Fields[nameof(PartialDeployPercentage)].guiActive = false;
                }

                MonoUtilities.RefreshContextWindows(part);
            }
        }

        public override void Retract()
        {
            base.Retract();
            if (HighLogic.LoadedSceneIsEditor)
            {
                SetMeshVisibility();
            }
        }

        protected void SetMeshVisibility()
        {
            if (_meshes != null && _meshes.Count > 0)
            {
                foreach (var mesh in _meshes)
                {
                    mesh.SetActive(
                        _isDeployedInEditor ||
                        mesh.ShowAtPercentage <= PartialDeployPercentage);
                }
            }
        }
    }
}
