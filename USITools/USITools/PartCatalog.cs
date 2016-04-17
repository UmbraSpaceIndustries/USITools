using KSP.UI.Screens;
using RUI.Icons.Selectable;
using System;
using System.Collections.Generic;
using System.Linq;
using KSP.UI;
using UnityEngine;
using UnityEngine.UI;



namespace USITools
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class USI_KolonyFilter : MonoBehaviour
    {
        private static readonly List<AvailablePart> kolonyParts = new List<AvailablePart>();
        internal string category = "Filter by Function";
        internal string subCategoryTitle = "Kolonization";
        internal string defaultTitle = "UKS";
        internal string iconName = "R&D_node_icon_start";
        internal bool filter = true;

        void Awake()
        {
            kolonyParts.Clear();
            foreach (var avPart in PartLoader.LoadedPartsList)
            {
                if (!avPart.partPrefab) continue;
                if (avPart.manufacturer == "USI - Kolonization Division")
                {
                    kolonyParts.Add(avPart);
                }
            }

            print("KolonyFilter Count: " + kolonyParts.Count);
            GameEvents.onGUIEditorToolbarReady.Add(SubCategories);
        }

        private static bool EditorItemsFilter(AvailablePart avPart)
        {
            return kolonyParts.Contains(avPart);
        }

        private void SubCategories()
        {
            print("*****Adding icon for " + subCategoryTitle);
            var icon = PartCategorizer.Instance.iconLoader.GetIcon(iconName);
            var filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == category);
            PartCategorizer.AddCustomSubcategoryFilter(filter, subCategoryTitle, icon, p => EditorItemsFilter(p));
        }
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class USI_FreightFilter : MonoBehaviour
    {
        private static readonly List<AvailablePart> kolonyParts = new List<AvailablePart>();
        internal string category = "Filter by Function";
        internal string subCategoryTitle = "Freight";
        internal string defaultTitle = "FTT";
        internal string iconName = "RDicon_fuelSystems-highPerformance";
        internal bool filter = true;

        void Awake()
        {
            kolonyParts.Clear();
            foreach (var avPart in PartLoader.LoadedPartsList)
            {
                if (!avPart.partPrefab) continue;
                if (avPart.manufacturer == "USI - Freight Division")
                {
                    kolonyParts.Add(avPart);
                }
            }

            print("FreightFilter Count: " + kolonyParts.Count);

            //if (kolonyParts.Count > 0)
            GameEvents.onGUIEditorToolbarReady.Add(SubCategories);
        }

        private static bool EditorItemsFilter(AvailablePart avPart)
        {
            return kolonyParts.Contains(avPart);
        }

        private void SubCategories()
        {
            print("*****Adding icon for " + subCategoryTitle);
            var icon = PartCategorizer.Instance.iconLoader.GetIcon(iconName);
            var filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == category);
            PartCategorizer.AddCustomSubcategoryFilter(filter, subCategoryTitle, icon, p => EditorItemsFilter(p));
        }
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class USI_KolonyLiteFilter : MonoBehaviour
    {
        private static readonly List<AvailablePart> kolonyParts = new List<AvailablePart>();
        internal string category = "Filter by Function";
        internal string subCategoryTitle = "Kolonization Lite";
        internal string defaultTitle = "UKS-LITE";
        internal string iconName = "R&D_node_icon_start";
        internal bool filter = true;

        void Awake()
        {
            kolonyParts.Clear();
            //Suppress if we have any UKS parts handy
            if (PartLoader.LoadedPartsList.Any(p => p.manufacturer == "USI - Kolonization Division"))
                return;

            foreach (var avPart in PartLoader.LoadedPartsList)
            {
                if (!avPart.partPrefab) continue;
                if (avPart.manufacturer == "USI - Consumer Kolonization Division")
                {
                    kolonyParts.Add(avPart);
                }
            }

            print("KolonyLiteFilter Count: " + kolonyParts.Count);

            //if (kolonyParts.Count > 0)
            GameEvents.onGUIEditorToolbarReady.Add(SubCategories);
        }

        private static bool EditorItemsFilter(AvailablePart avPart)
        {
            return kolonyParts.Contains(avPart);
        }

        private void SubCategories()
        {
            print("*****Adding icon for " + subCategoryTitle);
            var icon = PartCategorizer.Instance.iconLoader.GetIcon(iconName);
            var filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == category);
            PartCategorizer.AddCustomSubcategoryFilter(filter, subCategoryTitle, icon, p => EditorItemsFilter(p));
        }
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class USI_RoverFilter : MonoBehaviour
    {
        private static readonly List<AvailablePart> kolonyParts = new List<AvailablePart>();
        internal string category = "Filter by Function";
        internal string subCategoryTitle = "Rovers";
        internal string defaultTitle = "Rovers";
        internal string iconName = "R&D_node_icon_advancedmotors";
        internal bool filter = true;

        void Awake()
        {
            kolonyParts.Clear();
            foreach (var avPart in PartLoader.LoadedPartsList)
            {
                if (!avPart.partPrefab) continue;
                if (avPart.manufacturer == "USI - Rover Division")
                {
                    kolonyParts.Add(avPart);
                }
            }

            print("RoverFilter Count: " + kolonyParts.Count);
            //if (kolonyParts.Count > 0)
            //{
            GameEvents.onGUIEditorToolbarReady.Add(SubCategories);
            //}
        }

        private static bool EditorItemsFilter(AvailablePart avPart)
        {
            return kolonyParts.Contains(avPart);
        }

        private void SubCategories()
        {
            print("*****Adding icon for " + subCategoryTitle);
            var icon = PartCategorizer.Instance.iconLoader.GetIcon(iconName);
            var filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == category);
            PartCategorizer.AddCustomSubcategoryFilter(filter, subCategoryTitle, icon, p => EditorItemsFilter(p));
        }
    }
}