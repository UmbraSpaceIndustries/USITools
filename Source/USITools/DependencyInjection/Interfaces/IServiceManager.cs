using System;

namespace USITools
{
    public interface IServiceManager
    {
        object GetService(Type type);
        T GetService<T>() where T : class;
    }
}
