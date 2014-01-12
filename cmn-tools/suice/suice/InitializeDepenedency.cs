using System;

namespace CmnTools.Suice
{
    /// <summary>
    /// Dependencies implement this if require initialization
    /// 
    /// @author DisTurBinG
    /// </summary>
    public interface InitializeDependency
    {
        void Initialize();
    }
}
