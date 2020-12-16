using RootTools.Module;

namespace Root_EFEM.Module
{
    public interface ILoadport
    {
        ModuleRunBase GetRunLoad();
        ModuleRunBase GetRunUnload(); 
    }
}
