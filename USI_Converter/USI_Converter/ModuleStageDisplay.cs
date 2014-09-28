using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace USI
{
    public class ModuleStageDisplay : PartModule
    {
        [KSPField(guiActive = true)]
        public string Stage = "n.a.";

        public void FixedUpdate()
        {
            this.Stage = string.Format("invStage = {0} stageIdx = {1}", this.part.inverseStage, this.part.inStageIndex);
        }
    }
}
