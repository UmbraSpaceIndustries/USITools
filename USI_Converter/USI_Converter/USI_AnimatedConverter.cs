using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using USI;

namespace USITools
{
    public class USI_AnimatedConverter : PartModule
    {
        private List<USI_Converter> _converters;

        private bool _isConverting;
        [KSPField] public string convertAnimationName = "Convert";

        public Animation ConvertAnimation
        {
            get
            {
                var anims = this.part.FindModelAnimators(this.convertAnimationName);
                if (anims.Any())
                {
                    return this.part.FindModelAnimators(this.convertAnimationName)[0];
                }
                return null;
            }
        }

        private void CheckForConverting()
        {
            if (this._converters.Any(c => c.converterEnabled))
            {
                if (this.ConvertAnimation != null)
                {
                    if (!this.ConvertAnimation.isPlaying)
                    {
                        this.ConvertAnimation[this.convertAnimationName].speed = 1;
                        this.ConvertAnimation.Play(this.convertAnimationName);
                    }
                }
            }
        }

        private void FindGenerators()
        {
            if (this._converters == null)
            {
                this._converters = new List<USI_Converter>();
            }
            if (this.vessel != null)
            {
                if (this.part.Modules.Contains("USI_ResourceConverter"))
                {
                    this._converters = this.part.Modules.OfType<USI_Converter>().ToList();
                }
            }
        }

        public override void OnAwake()
        {
            this.FindGenerators();
        }

        public override void OnLoad(ConfigNode node)
        {
            this.FindGenerators();
        }

        public override void OnStart(StartState state)
        {
            this.FindGenerators();
            if (this.ConvertAnimation != null)
            {
                this.ConvertAnimation[this.convertAnimationName].layer = 3;
            }
        }

        public override void OnUpdate()
        {
            this.FindGenerators();
            this.CheckForConverting();
            base.OnUpdate();
        }
    }
}