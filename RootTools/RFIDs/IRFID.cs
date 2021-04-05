using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RootTools.RFIDs
{
    public interface IRFID
    {
        string Read(out string sRead);
        void RunTree(Tree tree);
        UserControl p_ui { get; }
        void ThreadStop(); 
    }
}
