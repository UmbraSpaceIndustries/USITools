using System.Collections.Generic;

namespace USITools
{
    /// <summary>
    /// Any class derived from <see cref="BaseConverter"/> that can accept
    ///   an <see cref="AbstractConverterAddon{T}"/>.
    /// </summary>
    /// <typeparam name="T">Any class derived from <see cref="BaseConverter"/>.</typeparam>
    public interface IConverterWithAddons<T>
        where T: BaseConverter
    {
        List<AbstractConverterAddon<T>> Addons { get; }
    }
}
