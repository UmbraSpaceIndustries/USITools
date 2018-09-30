using System;

namespace USITools
{
    [Obsolete("Use classes derived from AbstractSwapOption instead.")]
    public class LoadoutInfo
    {
        public int ModuleId { get; set; }
        public string LoadoutName { get; set; }
        public float BaseEfficiency { get; set; }
        public string DecalTexture { get; set; }
    }
}