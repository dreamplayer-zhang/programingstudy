using System.Windows.Controls;

namespace RootTools
{
    public interface ITool
    {
        string p_id { get; }
        UserControl p_ui { get; }
        void ThreadStop();
    }
}
