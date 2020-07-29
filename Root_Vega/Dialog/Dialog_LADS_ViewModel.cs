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
        SideVision.Run_LADS m_RunLADS;
        TreeRoot m_treeRoot = null;
        public TreeRoot p_treeRoot
        {
            get { return m_treeRoot; }
            set { SetProperty(ref m_treeRoot, value); }
        }

        public Dialog_LADS_ViewModel(SideVision vision, SideVision.Run_LADS lads)
        {
            m_Vision = vision;
            m_RunLADS = lads;
            p_treeRoot = new TreeRoot("LADS_ViewModel", vision.m_log);
            lads.RunTree(p_treeRoot, Tree.eMode.RegRead);
            lads.RunTree(p_treeRoot, Tree.eMode.Init);
            p_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
        }

        private void M_treeRoot_UpdateTree()
        {
            m_RunLADS.RunTree(p_treeRoot, Tree.eMode.Update);
            m_RunLADS.RunTree(p_treeRoot, Tree.eMode.Init);
            m_RunLADS.RunTree(p_treeRoot, Tree.eMode.RegWrite);
        }

        void Test()
        {
            m_Vision.p_eState = RootTools.Module.ModuleBase.eState.Run;
            m_Vision.StartRun(m_RunLADS);
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
