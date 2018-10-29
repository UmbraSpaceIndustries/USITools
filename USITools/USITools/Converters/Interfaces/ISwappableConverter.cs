namespace USITools
{
    public interface ISwappableConverter
    {
        bool IsStandalone { get; }
        void Swap(AbstractSwapOption swapOption);
    }
}
