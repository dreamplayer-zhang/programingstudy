using Root_Vega.Module;
using RootTools;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Root_Vega
{
    public class Dialog_LADS_ViewModel : ObservableObject, IDialogRequestClose
    {
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
        SideVision m_Vision;
        SideVision.Run_LADS m_RunLADS;
        public SideVision.Run_LADS p_RunLADS
        {
            get { return m_RunLADS; }
            set { SetProperty(ref m_RunLADS, value); }
        }
        TreeRoot m_treeRoot = null;
        public TreeRoot p_treeRoot
        {
            get { return m_treeRoot; }
            set { SetProperty(ref m_treeRoot, value); }
        }
        string m_strLeftSelectedInfo;
        public string p_strLeftSelectedInfo
        {
            get { return m_strLeftSelectedInfo; }
            set { SetProperty(ref m_strLeftSelectedInfo, value); }
        }
        string m_strRightSelectedInfo;
        public string p_strRightSelectedInfo
        {
            get { return m_strRightSelectedInfo; }
            set { SetProperty(ref m_strRightSelectedInfo, value); }
        }
        string m_strCenterSelectedInfo;
        public string p_strCenterSelectedInfo
        {
            get { return m_strCenterSelectedInfo; }
            set { SetProperty(ref m_strCenterSelectedInfo, value); }
        }
        BitmapSource m_bmpSrcLeftViewer;
        public BitmapSource p_bmpSrcLeftViewer
        {
            get { return m_bmpSrcLeftViewer; }
            set { SetProperty(ref m_bmpSrcLeftViewer, value); }
        }
        BitmapSource m_bmpSrcRightViewer;
        public BitmapSource p_bmpSrcRightViewer
        {
            get { return m_bmpSrcRightViewer; }
            set { SetProperty(ref m_bmpSrcRightViewer, value); }
        }
        BitmapSource m_bmpSrcCenterViewer;
        public BitmapSource p_bmpSrcCenterViewer
        {
            get { return m_bmpSrcCenterViewer; }
            set { SetProperty(ref m_bmpSrcCenterViewer, value); }
        }

        Visibility m_eLeftViewerVisibility = Visibility.Collapsed;
        public Visibility p_eLeftViewerVisibility
        {
            get { return m_eLeftViewerVisibility; }
            set { SetProperty(ref m_eLeftViewerVisibility, value); }
        }

        Visibility m_eRightViewerVisibility = Visibility.Collapsed;
        public Visibility p_eRightViewerVisibility
        {
            get { return m_eRightViewerVisibility; }
            set { SetProperty(ref m_eRightViewerVisibility, value); }
        }
        Visibility m_eCenterViewerVisibility = Visibility.Collapsed;
        public Visibility p_eCenterViewerVisibility
        {
            get { return m_eCenterViewerVisibility; }
            set { SetProperty(ref m_eCenterViewerVisibility, value); }
        }
        SideVision.CAutoFocusStatus m_afs;
        public SideVision.CAutoFocusStatus p_afs
        {
            get { return m_afs; }
            set { SetProperty(ref m_afs, value); }
        }
        ObservableCollection<SideVision.CStepInfo> m_lstLeftStepInfo;
        public ObservableCollection<SideVision.CStepInfo> p_lstLeftStepInfo
        {
            get { return m_lstLeftStepInfo; }
            set { SetProperty(ref m_lstLeftStepInfo, value); }
        }
        ObservableCollection<SideVision.CStepInfo> m_lstRightStepInfo;
        public ObservableCollection<SideVision.CStepInfo> p_lstRightStepInfo
        {
            get { return m_lstRightStepInfo; }
            set { SetProperty(ref m_lstRightStepInfo, value); }
        }
        ObservableCollection<SideVision.CStepInfo> m_lstCenterStepInfo;
        public ObservableCollection<SideVision.CStepInfo> p_lstCenterStepInfo
        {
            get { return m_lstCenterStepInfo; }
            set { SetProperty(ref m_lstCenterStepInfo, value); }
        }
        
        public Dialog_LADS_ViewModel(SideVision vision, SideVision.Run_LADS lads)
        {
            m_Vision = vision;
            p_RunLADS = lads;
            p_RunLADS._dispatcher = Dispatcher.CurrentDispatcher;
            p_lstLeftStepInfo = lads.p_lstLeftStepInfo;
            p_lstRightStepInfo = lads.p_lstRightStepInfo;
            p_lstCenterStepInfo = lads.p_lstCenterStepInfo;
            p_afs = lads.p_afs;
            p_bmpSrcLeftViewer = lads.p_bmpSrcLeftViewer;
            p_bmpSrcRightViewer = lads.p_bmpSrcRightViewer;
            p_bmpSrcCenterViewer = lads.p_bmpSrcCenterViewer;
            p_treeRoot = new TreeRoot("LADS_ViewModel", vision.m_log);
            lads.RunTree(p_treeRoot, Tree.eMode.RegRead);
            lads.RunTree(p_treeRoot, Tree.eMode.Init);
            p_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
        }

        private void M_treeRoot_UpdateTree()
        {
            p_RunLADS.RunTree(p_treeRoot, Tree.eMode.Update);
            p_RunLADS.RunTree(p_treeRoot, Tree.eMode.Init);
            p_RunLADS.RunTree(p_treeRoot, Tree.eMode.RegWrite);
        }

        public void OnOkButton()
        {
            m_Vision.StartRun(p_RunLADS);
            return;
        }

        public void OnCancelButton()
        {
            CloseRequested(this, new DialogCloseRequestedEventArgs(false));
        }

        public void OnLeftSideDoubleClick(object obj)
        {
            if (obj != null)
            {
                SideVision.CStepInfo si = (SideVision.CStepInfo)obj;
                p_bmpSrcLeftViewer = si.p_img;
                p_strLeftSelectedInfo = si.p_strInfo;
            }

            if (p_eLeftViewerVisibility == Visibility.Collapsed) p_eLeftViewerVisibility = Visibility.Visible;
            else p_eLeftViewerVisibility = Visibility.Collapsed;

            return;
        }

        public void OnRightSideDoubleClick(object obj)
        {
            if (obj != null)
            {
                SideVision.CStepInfo si = (SideVision.CStepInfo)obj;
                p_bmpSrcRightViewer = si.p_img;
                p_strRightSelectedInfo = si.p_strInfo;
            }

            if (p_eRightViewerVisibility == Visibility.Collapsed) p_eRightViewerVisibility = Visibility.Visible;
            else p_eRightViewerVisibility = Visibility.Collapsed;

            return;
        }

        public void OnCenterDoubleClick(object obj)
        {
            if (obj != null)
            {
                SideVision.CStepInfo si = (SideVision.CStepInfo)obj;
                p_bmpSrcCenterViewer = si.p_img;
                p_strCenterSelectedInfo = si.p_strInfo;
            }

            if (p_eCenterViewerVisibility == Visibility.Collapsed) p_eCenterViewerVisibility = Visibility.Visible;
            else p_eCenterViewerVisibility = Visibility.Collapsed;

            return;
        }

        public RelayCommandWithParameter LeftSideDoubleClick
        {
            get
            {
                return new RelayCommandWithParameter(OnLeftSideDoubleClick);
            }
        }

        public RelayCommandWithParameter RightSideDoubleClick
        {
            get
            {
                return new RelayCommandWithParameter(OnRightSideDoubleClick);
            }
        }

        public RelayCommandWithParameter CenterDoubleClick
        {
            get
            {
                return new RelayCommandWithParameter(OnCenterDoubleClick);
            }
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
