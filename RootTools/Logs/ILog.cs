using System.Windows.Controls;

namespace RootTools.Logs
{
    public interface ILog
    {
        string p_id { get; set; }
        UserControl p_ui { get; }
    }
}
