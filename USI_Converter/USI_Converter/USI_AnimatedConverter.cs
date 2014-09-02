using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using USI;

namespace USITools
{
    public class USI_AnimatedConverter : PartModule
    {
        [KSPField] 
        public string convertAnimationName = "Convert";

        private List<USI_Converter> _converters;
 
        private bool _isConverting;
        public Animation ConvertAnimation
        {
            get
            {
                var anims = part.FindModelAnimators(convertAnimationName);
                if (anims.Any())
                {
                    return part.FindModelAnimators(convertAnimationName)[0];
                }
                return null;
            }
        }

        public override void OnStart(PartModule.StartState state)
        {
            FindGenerators();
            if (ConvertAnimation != null)
            {
                ConvertAnimation[convertAnimationName].layer = 3;
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            FindGenerators();
        }

        public override void OnAwake()
        {
            FindGenerators();
        }

        public override void OnUpdate()
        {
            FindGenerators();
            CheckForConverting();
            base.OnUpdate();
        }

        private void FindGenerators()
        {
            if(_converters == null) _converters = new List<USI_Converter>();
            if (vessel != null)
            {
                if (part.Modules.Contains("USI_ResourceConverter"))
                {
                    _converters = part.Modules.OfType<USI_Converter>().ToList();
                } 
            }
        }

        private void CheckForConverting()
        {
            if (_converters.Any(c => c.converterEnabled))
            {
                if (ConvertAnimation != null)
                {
                    if (!ConvertAnimation.isPlaying)
                    {
                        ConvertAnimation[convertAnimationName].speed = 1;
                        ConvertAnimation.Play(convertAnimationName);
                    }
                }
            }
        }
    }
}
