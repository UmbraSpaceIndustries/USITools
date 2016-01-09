using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



namespace USITools
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class USI_KolonyFilter : MonoBehaviour
    {
        private static List<AvailablePart> kolonyParts = new List<AvailablePart>();
        internal string category = "Filter by Function";
        internal string subCategoryTitle = "Kolonization";
        internal string defaultTitle = "UKS";
        internal string iconName = "R&D_node_icon_start";
        internal bool filter = true;

        void Awake()
        {
            kolonyParts.Clear();
            foreach (AvailablePart avPart in PartLoader.LoadedPartsList)
            {
                if (!avPart.partPrefab) continue;
                if(avPart.manufacturer == "USI - Kolonization Division")
                {
                        kolonyParts.Add(avPart);
                }
            }

            if (kolonyParts.Count > 0)
            {
                GameEvents.onGUIEditorToolbarReady.Add(SubCategories);
            }

        }

        private bool EditorItemsFilter(AvailablePart avPart)
        {
            if (kolonyParts.Contains(avPart))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SubCategories()
        {
            RUI.Icons.Selectable.Icon icon = PartCategorizer.Instance.iconLoader.GetIcon(iconName);
            PartCategorizer.Category Filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == category);
            PartCategorizer.AddCustomSubcategoryFilter(Filter, subCategoryTitle, icon, p => EditorItemsFilter(p));

            RUIToggleButtonTyped button = Filter.button.activeButton;
            button.SetFalse(button, RUIToggleButtonTyped.ClickType.FORCED);
            button.SetTrue(button, RUIToggleButtonTyped.ClickType.FORCED);
        }
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class USI_FreightFilter : MonoBehaviour
    {
        private static List<AvailablePart> kolonyParts = new List<AvailablePart>();
        internal string category = "Filter by Function";
        internal string subCategoryTitle = "Freight";
        internal string defaultTitle = "FTT";
        internal string iconName = "RDicon_fuelSystems-highPerformance";
        internal bool filter = true;

        void Awake()
        {
            kolonyParts.Clear();
            foreach (AvailablePart avPart in PartLoader.LoadedPartsList)
            {
                if (!avPart.partPrefab) continue;
                if (avPart.manufacturer == "USI - Freight Division")
                {
                    kolonyParts.Add(avPart);
                }
            }

            if (kolonyParts.Count > 0)
            {
                GameEvents.onGUIEditorToolbarReady.Add(SubCategories);
            }
        }

        private bool EditorItemsFilter(AvailablePart avPart)
        {
            if (kolonyParts.Contains(avPart))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SubCategories()
        {
            RUI.Icons.Selectable.Icon icon = PartCategorizer.Instance.iconLoader.GetIcon(iconName);
            PartCategorizer.Category Filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == category);
            PartCategorizer.AddCustomSubcategoryFilter(Filter, subCategoryTitle, icon, p => EditorItemsFilter(p));

            RUIToggleButtonTyped button = Filter.button.activeButton;
            button.SetFalse(button, RUIToggleButtonTyped.ClickType.FORCED);
            button.SetTrue(button, RUIToggleButtonTyped.ClickType.FORCED);
        }
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class USI_KolonyLiteFilter : MonoBehaviour
    {
        private static List<AvailablePart> kolonyParts = new List<AvailablePart>();
        internal string category = "Filter by Function";
        internal string subCategoryTitle = "Kolonization Lite";
        internal string defaultTitle = "UKS-LITE";
        internal string iconName = "R&D_node_icon_start";
        internal bool filter = true;

        void Awake()
        {
            kolonyParts.Clear();
            foreach (AvailablePart avPart in PartLoader.LoadedPartsList)
            {
                if (!avPart.partPrefab) continue;
                if (avPart.manufacturer == "USI - Consumer Kolonization Division")
                {
                    kolonyParts.Add(avPart);
                }
            }

            if (kolonyParts.Count > 0)
            {
                GameEvents.onGUIEditorToolbarReady.Add(SubCategories);
            }

        }

        private bool EditorItemsFilter(AvailablePart avPart)
        {
            if (kolonyParts.Contains(avPart))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SubCategories()
        {
            RUI.Icons.Selectable.Icon icon = PartCategorizer.Instance.iconLoader.GetIcon(iconName);
            PartCategorizer.Category Filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == category);
            PartCategorizer.AddCustomSubcategoryFilter(Filter, subCategoryTitle, icon, p => EditorItemsFilter(p));

            RUIToggleButtonTyped button = Filter.button.activeButton;
            button.SetFalse(button, RUIToggleButtonTyped.ClickType.FORCED);
            button.SetTrue(button, RUIToggleButtonTyped.ClickType.FORCED);
        }
    }


    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class USI_RoverFilter : MonoBehaviour
    {
        private static List<AvailablePart> kolonyParts = new List<AvailablePart>();
        internal string category = "Filter by Function";
        internal string subCategoryTitle = "Rovers";
        internal string defaultTitle = "Rovers";
        internal string iconName = "R&D_node_icon_advancedmotors";
        internal bool filter = true;

        void Awake()
        {
            kolonyParts.Clear();
            foreach (AvailablePart avPart in PartLoader.LoadedPartsList)
            {
                if (!avPart.partPrefab) continue;
                if (avPart.manufacturer == "USI - Rover Division")
                {
                    kolonyParts.Add(avPart);
                }
            }

            if (kolonyParts.Count > 0)
            {
                GameEvents.onGUIEditorToolbarReady.Add(SubCategories);
            }
        }

        private bool EditorItemsFilter(AvailablePart avPart)
        {
            if (kolonyParts.Contains(avPart))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SubCategories()
        {
            RUI.Icons.Selectable.Icon icon = PartCategorizer.Instance.iconLoader.GetIcon(iconName);
            PartCategorizer.Category Filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == category);
            PartCategorizer.AddCustomSubcategoryFilter(Filter, subCategoryTitle, icon, p => EditorItemsFilter(p));

            RUIToggleButtonTyped button = Filter.button.activeButton;
            button.SetFalse(button, RUIToggleButtonTyped.ClickType.FORCED);
            button.SetTrue(button, RUIToggleButtonTyped.ClickType.FORCED);
        }
    }
}