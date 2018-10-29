using System;
using System.Collections.Generic;
using UnityEngine;

namespace USITools
{
    /// <summary>
    /// Wrapper class for the base game's <see cref="ModuleResourceHarvester"/> that
    ///   allows us to do things like swap drill heads, give drills side effects
    ///   via <see cref="AbstractConverterAddon{T}"/>, etc.
    /// </summary>
    public class USI_Harvester :
        ModuleResourceHarvester,
        IConverterWithAddons<USI_Harvester>,
        ISwappableConverter
    {
        #region Fields and properties
        private AbstractSwapOption<USI_Harvester> _swapOption;

        public List<AbstractConverterAddon<USI_Harvester>> Addons { get; private set; } =
            new List<AbstractConverterAddon<USI_Harvester>>();

        [KSPField]
        protected bool IsStandaloneHarvester = false;

        /// <summary>
        /// This allows standalone harvesters to co-exist with swappable harvesters on the same part.
        /// </summary>
        /// <remarks>
        /// Wrapping the KSPField in a property is necessary in order to expose it via <see cref="ISwappableConverter"/>
        /// and thus allow <see cref="USI_SwapController"/> to ignore standalone harvesters.
        /// </remarks>
        public bool IsStandalone
        {
            get { return IsStandaloneHarvester; }
        }
        #endregion

        public void Swap(AbstractSwapOption swapOption)
        {
            Swap(swapOption as AbstractSwapOption<USI_Harvester>);
        }

        public void Swap(AbstractSwapOption<USI_Harvester> swapOption)
        {
            _swapOption = swapOption;
            Addons.Clear();

            try
            {
                _swapOption.ApplyConverterChanges(this);
            }
            catch (Exception ex)
            {
                Debug.LogError("[USI] USI_Harvester: Could not apply harvester changes. " + ex.Message);
            }
        }

        protected override ConversionRecipe PrepareRecipe(double deltaTime)
        {
            var recipe = base.PrepareRecipe(deltaTime);

            if (_swapOption != null)
            {
                recipe = _swapOption.PrepareRecipe(recipe);
            }

            return recipe;
        }

        protected override void PreProcessing()
        {
            base.PreProcessing();

            if (_swapOption != null)
            {
                _swapOption.PreProcessing(this);
            }
        }

        protected override void PostProcess(ConverterResults result, double deltaTime)
        {
            base.PostProcess(result, deltaTime);

            var hasLoad = false;
            if (status != null)
            {
                hasLoad = status.EndsWith("Load");
            }

            if (result.TimeFactor >= ResourceUtilities.FLOAT_TOLERANCE
                && !hasLoad)
            {
                statusPercent = 0d; //Force a reset of the load display.
            }

            if (_swapOption != null)
            {
                _swapOption.PostProcess(this, result, deltaTime);
            }
        }

        public override string GetInfo()
        {
            if (_swapOption == null)
                return base.GetInfo();

            return string.Empty;
        }
    }
}
