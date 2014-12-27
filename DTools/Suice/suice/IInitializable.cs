namespace DTools.Suice
{
    /// <summary>
    /// Market for Injector to automatically call Initialize of a neweley created Dependency once it's created.
    /// 
    /// @author DisTurBinG
    /// </summary>
    public interface IInitializable
    {
        void Initialize();
    }
}