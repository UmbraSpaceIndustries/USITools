using System;
using System.Collections.Generic;

namespace USITools.Helpers
{
    public static class ResourceHelpers
    {
        /// <summary>
        /// Convert a comma-separated list of resource value pairs into a list of <see cref="ResourceRatio"/>.
        /// </summary>
        /// <param name="serializedResourceRatios">Comma-separated list of key,value pairs (ex. MaterialKits,200,SpecializedParts,20)</param>
        /// <returns></returns>
        public static List<ResourceRatio> DeserializeResourceRatios(string serializedResourceRatios)
        {
            var tokens = serializedResourceRatios.Split(',');
            if (tokens.Length % 2 != 0)
            {
                throw new Exception($"Invalid resource string '{serializedResourceRatios}'");
            }

            var resources = new List<ResourceRatio>();
            for (int i = 0; i < tokens.Length; i += 2)
            {
                var sanitizedResourceName = tokens[i].Trim();
                var sanitizedAmount = tokens[i + 1].Trim();
                if (double.TryParse(sanitizedResourceName, out _))
                {
                    throw new Exception($"Resource pair {tokens[i]},{tokens[i + 1]} is invalid. (expected ResourceName,Amount)");
                }
                if (!double.TryParse(sanitizedAmount, out double amount))
                {
                    throw new Exception($"Resource pair {tokens[i]},{tokens[i + 1]} is invalid. (expected ResourceName,Amount)");
                }
                resources.Add(new ResourceRatio(sanitizedResourceName, amount, false));
            }

            return resources;
        }
    }
}
