using MaterialDesignThemes.Wpf;
using Root_Vega.Module;
using RootTools;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_Vega
{
    public class Dialog_LADS_ViewModel : ObservableObject, IDialogRequestClose
    {
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
        SideVision m_Vision;
        Run_LADS m_RunLADS;
        TreeRoot m_treeRoot = null;
        public TreeRoot p_treeRoot
        {
            get { return m_treeRoot; }
            set { SetProperty(ref m_treeRoot, value); }
        }

        public Dialog_LADS_ViewModel(SideVision vision, Run_LADS lads)
        {
            m_Vision = vision;
            m_RunLADS = lads;
            p_treeRoot = new TreeRoot("LADS_ViewModel", vision.m_log);
        }

        void Test()
        {
            return;
        }

        public RelayCommand CommandTest
        {
            get
            {
                return new RelayCommand(Test);
            }
        }
    }
}
