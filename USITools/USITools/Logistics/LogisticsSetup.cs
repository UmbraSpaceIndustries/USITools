using System;
using UnityEngine;

namespace USITools.Logistics
{
    public class LogisticsSetup : MonoBehaviour
    {
        // Static singleton instance
        private static LogisticsSetup instance;

        // Static singleton property
        public static LogisticsSetup Instance
        {
            get { return instance ?? (instance = new GameObject("LogisticsSetup").AddComponent<LogisticsSetup>()); }
        }

        //Static data holding variables
        private static LogisticsConfig _config;

        public LogisticsConfig Config
        {
            get { return _config ?? (_config = LoadConfig()); }
        }

        private LogisticsConfig LoadConfig()
        {
            var lsNodes = GameDatabase.Instance.GetConfigNodes("LOGISTICS_SETTINGS");
            var finalSettings = new LogisticsConfig
            {
                ScavangeRange = 150,
                LogisticsTime = 5,
                WarehouseTime = 10,
                MaintenanceRange = 150
            };
            foreach (var lsNode in lsNodes)
            {
                var settings = ResourceUtilities.LoadNodeProperties<LogisticsConfig>(lsNode);
                finalSettings.WarehouseTime = Math.Max(settings.WarehouseTime, finalSettings.WarehouseTime);
                finalSettings.LogisticsTime = Math.Max(settings.LogisticsTime, finalSettings.LogisticsTime);
                finalSettings.ScavangeRange = Math.Max(settings.ScavangeRange, finalSettings.ScavangeRange);
                finalSettings.MaintenanceRange = Math.Max(settings.MaintenanceRange, finalSettings.MaintenanceRange);
            }
            return finalSettings;
        }
    }
}