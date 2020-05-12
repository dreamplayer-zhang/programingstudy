using System.Collections.Generic;
using System.Windows.Controls;

namespace RootTools.Logs
{
    public class Log_Set : ILog
    {
        #region ILog
        public string p_id { get; set; }

        public UserControl p_ui
        { 
            get
            {
                Log_Set_UI ui = new Log_Set_UI();
                ui.Init(this); 
                return ui; 
            }
        }

        public void CalcData()
        {
            foreach (Log_Group group in m_aGroup) group.CalcData(); 
        }
        #endregion

        #region List Log Group
        public List<Log_Group> m_aGroup = new List<Log_Group>(); 
        #endregion

        public Log_Set(string sLogSet)
        {
            p_id = sLogSet; 
        }
    }
}
