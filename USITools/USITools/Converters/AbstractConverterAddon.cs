namespace USITools
{
    public abstract class AbstractConverterAddon<T>
        where T: BaseConverter
    {
        protected T Converter { get; private set; }
        public bool IsActive
        {
            get { return Converter.IsActivated; }
        }

        public AbstractConverterAddon(T converter)
        {
            Converter = converter;
        }

        public virtual void PreProcessing()
        {
        }

        public virtual void PostProcess(ConverterResults result, double deltaTime)
        {
        }
    }
}
