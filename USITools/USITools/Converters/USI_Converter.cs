using System;
using System.Collections.Generic;
using UnityEngine;

namespace USITools
{
    /// <summary>
    /// Wrapper class for the base game's <see cref="ModuleResourceConverter"/> that
    ///   allows us to do things like swap converter recipes, give converters side effects
    ///   via <see cref="AbstractConverterAddon{T}"/>, etc.
    /// </summary>
    /// <remarks>
    public class USI_Converter :
        ModuleResourceConverter,
        IConverterWithAddons<USI_Converter>,
        ISwappableConverter
    {
        #region Fields and properties
        private AbstractSwapOption<USI_Converter> _swapOption;

        public List<AbstractConverterAddon<USI_Converter>> Addons { get; private set; } =
            new List<AbstractConverterAddon<USI_Converter>>();

        [KSPField]
        protected bool IsStandaloneConverter = false;

        /// <summary>
        /// This allows standalone converters to co-exist with swappable converters on the same part.
        /// </summary>
        /// <remarks>
        /// Wrapping the KSPField in a property is necessary in order to expose it via <see cref="ISwappableConverter"/>
        /// and thus allow <see cref="USI_SwapController"/> to ignore standalone converters.
        /// </remarks>
        public bool IsStandalone
        {
            get { return IsStandaloneConverter; }
        }
        #endregion

        public void Swap(AbstractSwapOption swapOption)
        {
            Swap(swapOption as AbstractSwapOption<USI_Converter>);
        }

        public void Swap(AbstractSwapOption<USI_Converter> swapOption)
        {
            _swapOption = swapOption;
            Addons.Clear();

            try
            {
                _swapOption.ApplyConverterChanges(this);
            }
            catch (Exception ex)
            {
                Debug.LogError("[USI] USI_Converter: Could not apply converter changes. " + ex.Message);
            }
        }

        protected override ConversionRecipe PrepareRecipe(double deltatime)
        {
            var recipe = base.PrepareRecipe(deltatime);
            if (recipe == null)
            {
                recipe = Recipe;
            }

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

            if (!hasLoad && result.TimeFactor >= ResourceUtilities.FLOAT_TOLERANCE)
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
            if (IsStandaloneConverter)
                return base.GetInfo();

            return string.Empty;
        }
    }
}
