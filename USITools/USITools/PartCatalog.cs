using KSP.UI.Screens;
using RUI.Icons.Selectable;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using KSP.Localization;

namespace USITools
{
    //[KSPAddon(KSPAddon.Startup.MainMenu, true)]
    //public class USI_RoverFilter : BaseFilter
    //{
    //    protected override string Manufacturer
    //    {
    //        get { return "USI - Rover Division"; }
    //        set { }
    //    }
    //    protected override string categoryTitle
    //    {
    //        get { return "Rovers"; }
    //        set { }
    //    }
    //}

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class USI_ManufacturingFilter : BaseFilter
    {
        protected override string Manufacturer
        {
            get { return "USI - Manufacturing Division"; }
            set { }
        }
        protected override string categoryTitle
        {
            get { return "Manufacturing"; }
            set { }
        }
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class USI_LogisticsFilter : BaseFilter
    {
        protected override string Manufacturer
        {
            get { return "USI - Logistics Division"; }
            set { }
        }
        protected override string categoryTitle
        {
            get { return "Logistics"; }
            set { }
        }
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class USI_ConstructionFilter : BaseFilter
    {
        protected override string Manufacturer
        {
            get { return "USI - Construction Division"; }
            set { }
        }
        protected override string categoryTitle
        {
            get { return "Construction"; }
            set { }
        }
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class USI_KolonizationFilter : BaseFilter
    {
        protected override string Manufacturer
        {
            get { return "USI - Kolonization Division"; }
            set { }
        }
        protected override string categoryTitle
        {
            get { return "Kolonization"; }//
            set { }
        }
    }


    //[KSPAddon(KSPAddon.Startup.MainMenu, true)]
    //public class USI_LifeSupportFilter : BaseFilter
    //{
    //    protected override string Manufacturer
    //    {
    //        get { return "USI - Life Support Division"; }
    //        set { }
    //    }
    //    protected override string categoryTitle
    //    {
    //        get { return "LifeSupport"; }
    //        set { }
    //    }
    //}

    public abstract class BaseFilter : MonoBehaviour
    {
        private readonly List<AvailablePart> parts = new List<AvailablePart>();
        internal string category = "#autoLOC_453547";
        internal bool filter = true;
        protected abstract string Manufacturer { get; set; }
        protected abstract string categoryTitle { get; set; }

        void Awake()
        {
            parts.Clear();
            var count = PartLoader.LoadedPartsList.Count;
            for(int i = 0; i < count; ++i)
            {
                var avPart = PartLoader.LoadedPartsList[i];
                if (!avPart.partPrefab) continue;
                if (avPart.manufacturer == Manufacturer)
                {
                    parts.Add(avPart);
                }
            }

            print(categoryTitle + "  Filter Count: " + parts.Count);
            if (parts.Count > 0)
                GameEvents.onGUIEditorToolbarReady.Add(SubCategories);
        }

        private bool EditorItemsFilter(AvailablePart avPart)
        {
            return parts.Contains(avPart);
        }

        private void SubCategories()
        {
            var icon = GenIcon(categoryTitle);
            var filter = PartCategorizer.Instance.filters.Find(f => f.button.categorydisplayName == category);
            PartCategorizer.AddCustomSubcategoryFilter(filter, categoryTitle, categoryTitle, icon, EditorItemsFilter);
        }

        private Icon GenIcon(string iconName)
        {
            var normIcon = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            var normIconFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), iconName + "_N.png");
            normIcon.LoadImage(File.ReadAllBytes(normIconFile));

            var selIcon = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            var selIconFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), iconName + "_S.png");
            selIcon.LoadImage(File.ReadAllBytes(selIconFile));

            print("*****Adding icon for " + categoryTitle);
            var icon = new Icon(iconName + "Icon", normIcon, selIcon);
            return icon;
        }
    }

}