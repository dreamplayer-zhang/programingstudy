using System.Windows.Controls;

namespace RootTools.Logs
{
    public class LogSet : ILog
    {
        #region ILogSet
        public string p_id { get; set; }

        public UserControl p_ui
        { 
            get
            {
                LogSet_UI ui = new LogSet_UI();
                ui.Init(this); 
                return ui; 
            }
        }
        #endregion

        public LogSet(string sLogSet)
        {
            p_id = sLogSet; 
        }
    }
}
