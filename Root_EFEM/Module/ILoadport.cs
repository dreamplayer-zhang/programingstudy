using RootTools;

namespace Root_EFEM.Module
{
    public interface ILoadport
    {
        string p_id { get; set; }

        string RunDocking();

        string RunUndocking();

        InfoCarrier p_infoCarrier { get; set; }
    }
}
