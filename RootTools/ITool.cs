using System.Windows.Controls;

namespace RootTools
{
    public interface ITool
    {
        string p_id { get; set; }
        UserControl p_ui { get; }
        void ThreadStop();
    }
}
