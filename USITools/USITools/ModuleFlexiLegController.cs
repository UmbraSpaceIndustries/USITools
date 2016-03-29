using System.Collections.Generic;

namespace USITools
{
    public class ModuleFlexiLegController : PartModule
    {
        [KSPField(isPersistant = true)]
        public bool isActiveController = false;

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Enable Controller")]
        public void EnableController()
        {
            isActiveController = true;
            ToggleController();
        }

        [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "Disable Controller")]
        public void DisableController()
        {
            isActiveController = false;
            ToggleController();
        }

        public override void OnStart(StartState state)
        {
            ToggleController();
        }

        private void ToggleController()
        {
            Events["EnableController"].guiActive = !isActiveController;
            Events["DisableController"].guiActive = isActiveController;
            Events["EnableController"].guiActiveEditor = !isActiveController;
            Events["DisableController"].guiActiveEditor = isActiveController;
            MonoUtilities.RefreshContextWindows(part);

            //We should also toggle ALL controllers on this vessel.
            var contList = GetLegControllers();
            foreach (var c in contList)
            {
                c.isActiveController = isActiveController;
            }
        }

        private List<ModuleFlexiLegController> GetLegControllers()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (EditorLogic.fetch.ship.parts.Count > 0)
                {
                    var ctr = new List<ModuleFlexiLegController>();
                    foreach (var p in EditorLogic.fetch.ship.parts)
                    {
                        var mod = p.FindModuleImplementing<ModuleFlexiLegController>();
                        if (mod == null)
                            continue;

                        ctr.Add(mod);
                    }
                    return ctr;
                }
                return new List<ModuleFlexiLegController>();
            }                
            return vessel.FindPartModulesImplementing<ModuleFlexiLegController>();
        }
    }
}