using MaterialDesignThemes.Wpf;
using Root_Vega.Module;
using RootTools;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

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

        ImageViewer_ViewModel m_ImageViewerLeft = new ImageViewer_ViewModel();
        public ImageViewer_ViewModel p_ImageViewerLeft
        {
            get { return m_ImageViewerLeft; }
            set { SetProperty(ref m_ImageViewerLeft, value); }
        }
        ImageViewer_ViewModel m_ImageViewerRight = new ImageViewer_ViewModel();
        public ImageViewer_ViewModel p_ImageViewerRight
        {
            get { return m_ImageViewerRight; }
            set { SetProperty(ref m_ImageViewerRight, value); }
        }

        public Dialog_LADS_ViewModel(SideVision vision, SideVision.Run_LADS lads)
        {
            m_Vision = vision;
            m_RunLADS = lads;
            m_RunLADS._dispatcher = Dispatcher.CurrentDispatcher;
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

        public void OnOkButton()
        {
            m_Vision.StartRun(m_RunLADS);
            return;
        }

        public void OnCancelButton()
        {
            CloseRequested(this, new DialogCloseRequestedEventArgs(false));
        }

        public RelayCommand OkCommand
        {
            get
            {
                return new RelayCommand(OnOkButton);
            }
        }

        public RelayCommand CancelCommand
        {
            get
            {
                return new RelayCommand(OnCancelButton);
            }
        }
    }
}
