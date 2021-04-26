using System.Collections.Generic;
using System.Linq;

namespace USITools
{
    public static class ConverterExtensionMethods
    {
        /// <summary>
        /// Find all converter addons of the specified type on the <see cref="Vessel"/>.
        /// </summary>
        /// <typeparam name="T">The converter addon type.</typeparam>
        /// <typeparam name="T1">The converter type.</typeparam>
        /// <param name="vessel"></param>
        /// <returns></returns>
        public static List<T> FindConverterAddonsImplementing<T, T1>(this Vessel vessel)
            where T : AbstractConverterAddon<T1>
            where T1: BaseConverter
        {
            var converters = vessel.FindPartModulesImplementing<IConverterWithAddons<T1>>();

            return FindConverterAddons<T, T1>(converters);
        }

        /// <summary>
        /// Find all converter addons of the specified type on the <see cref="Vessel"/>.
        /// </summary>
        /// <remarks>
        /// This overload assumes that you're looking for addons for a <see cref="USI_Converter"/>,
        /// since that will be the case 95% of the time.
        /// </remarks>
        /// <typeparam name="T">The converter addon type.</typeparam>
        /// <param name="vessel"></param>
        /// <returns></returns>
        public static List<T> FindConverterAddonsImplementing<T>(this Vessel vessel)
            where T : AbstractConverterAddon<USI_Converter>
        {
            return FindConverterAddonsImplementing<T, USI_Converter>(vessel);
        }

        /// <summary>
        /// Find all converter addons of the specified type on the <see cref="Part"/>.
        /// </summary>
        /// <typeparam name="T">The converter addon type.</typeparam>
        /// <typeparam name="T1">The converter type.</typeparam>
        /// <param name="part"></param>
        /// <returns></returns>
        public static List<T> FindConverterAddonsImplementing<T, T1>(this Part part)
            where T : AbstractConverterAddon<T1>
            where T1 : BaseConverter
        {
            var converters = part.FindModulesImplementing<IConverterWithAddons<T1>>();

            return FindConverterAddons<T, T1>(converters);
        }

        /// <summary>
        /// Find all converter addons of the specified type on the <see cref="Part"/>.
        /// </summary>
        /// <remarks>
        /// This overload assumes that you're looking for addons for a <see cref="USI_Converter"/>,
        /// since that will be the case 95% of the time.
        /// </remarks>
        /// <typeparam name="T">The converter addon type.</typeparam>
        /// <param name="part"></param>
        /// <returns></returns>
        public static List<T> FindConverterAddonsImplementing<T>(this Part part)
            where T : AbstractConverterAddon<USI_Converter>
        {
            return FindConverterAddonsImplementing<T, USI_Converter>(part);
        }

        private static List<T> FindConverterAddons<T, T1>(List<IConverterWithAddons<T1>> converters)
            where T : AbstractConverterAddon<T1>
            where T1 : BaseConverter
        {
            var addons = converters
                .SelectMany(c => c.Addons
                    .Where(a => a is T))
                .ToList();

            return addons.Select(a => (T)a).ToList();
        }
    }
}
