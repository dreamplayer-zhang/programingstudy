using System.Collections.Generic;
using System.Windows.Controls;

namespace RootTools.Lens
{
    public interface ILens
    {
        string p_id { get; set; }

        void ThreadStop();

        UserControl p_ui { get; }

        List<string> p_asPos { get; set; }

        string ChangePos(string sPos);

        string WaitReady(); 
    }
}
