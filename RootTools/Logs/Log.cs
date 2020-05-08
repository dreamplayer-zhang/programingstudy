using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RootTools.Logs
{
    public class Log : NLog.Logger, ILog
    {
        #region ILogSet
        public string p_id { get; set; }

        public UserControl p_ui
        {
            get
            {
                Log_UI ui = new Log_UI();
                ui.Init(this);
                return ui;
            }
        }
        #endregion
    }
}
