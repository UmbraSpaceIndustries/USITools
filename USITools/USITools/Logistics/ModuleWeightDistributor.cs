using System.Collections.Generic;

namespace KolonyTools
{
    public class ModuleWeightDistributor : PartModule
    {
        [KSPField(isPersistant = true)]
        public bool transferEnabled = false;

        [KSPEvent(guiName = "Disable Weight Transfer", active = false, guiActive = true, guiActiveEditor = true)]
        public void DisableTransfer()
        {
            ToggleTransfer(false);
        }

        [KSPEvent(guiName = "Enable Weight Transfer", active = true, guiActive = true, guiActiveEditor = true)]
        public void EnableTransfer()
        {
            ToggleTransfer(true);
        }

        private void ToggleTransfer(bool state)
        {
            transferEnabled = state;
            Events["DisableTransfer"].active = state;
            Events["EnableTransfer"].active = !state;
            WeightTransfer();
            MonoUtilities.RefreshContextWindows(part);
        }

        private List<Part> _cargoParts;
        private int _childCount;

        public override void OnStart(StartState state)
        {
            _cargoParts = GetCargoParts();
            _childCount = part.children.Count;
            ToggleTransfer(transferEnabled);
        }

        public void Update()
        {
            var c = part.children.Count;
            if (c == _childCount)
                return;

            _childCount = c;
            _cargoParts = GetCargoParts();
            WeightTransfer();
        }

        private List<Part> GetCargoParts()
        {
            var c = part.children.Count;
            var parts = new List<Part>();
            for (int i = 0; i < c; ++i)
            {
                var p = part.children[i];
                if (p.children.Count == 0 
                    &&p.FindModuleImplementing<ModuleWeightDistributableCargo>() != null)
                    parts.Add(p);
            }
            return parts;
        }

        private void WeightTransfer()
        {
            var c = _cargoParts.Count;
            for (int i = 0; i < c; ++i)
            {
                var disablePhysics = transferEnabled;
                var p = _cargoParts[i];
                if (p.children.Count > 0)
                    disablePhysics = false;
                if (!p.Modules.Contains("ModuleWeightDistributableCargo"))
                    disablePhysics = false;

                if (disablePhysics)
                    p.physicalSignificance = Part.PhysicalSignificance.NONE;
                else
                    p.physicalSignificance = Part.PhysicalSignificance.FULL;
            }
        }
    }
}