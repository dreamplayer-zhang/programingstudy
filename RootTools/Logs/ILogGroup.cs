using NLog.Config;
using System.Windows.Controls;

namespace RootTools
{
    public interface ILogGroup
    {
        string p_id { get; set; }
        UserControl p_ui { get; }
        void AddRule(LoggingConfiguration config); 
        void CalcData();
    }
}
