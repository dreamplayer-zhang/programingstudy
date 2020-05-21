﻿using MvvmDialogs;
using Root_Vega.Module;
using RootTools;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media.Imaging;

namespace Root_Vega
{
    public class Dialog_AutoFocus_ViewModel : ObservableObject, IDialogRequestClose, IModalDialogViewModel
    {
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
        SideVision m_Vision;
        SideVision.Run_AutoFocus m_RunAutoFocus;
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
        SideVision.Run_AutoFocus.CAutoFocusStatus m_afs;
        public SideVision.Run_AutoFocus.CAutoFocusStatus p_afs
        {
            get { return m_afs; }
            set { SetProperty(ref m_afs, value); }
        }
        SideVision.Run_AutoFocus.CStepInfoList m_lstLeftStepInfo;
        public SideVision.Run_AutoFocus.CStepInfoList p_lstLeftStepInfo
        {
            get { return m_lstLeftStepInfo; }
            set { SetProperty(ref m_lstLeftStepInfo, value); }
        }
        SideVision.Run_AutoFocus.CStepInfoList m_lstRightStepInfo;
        public SideVision.Run_AutoFocus.CStepInfoList p_lstRightStepInfo
        {
            get { return m_lstRightStepInfo; }
            set { SetProperty(ref m_lstRightStepInfo, value); }
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
        
        public Dialog_AutoFocus_ViewModel(SideVision vision, SideVision.Run_AutoFocus af)
        {
            m_Vision = vision;
            m_RunAutoFocus = af;
            p_ImageViewerLeft.p_ImageData = af.m_imgDataLeft;
            p_ImageViewerRight.p_ImageData = af.m_imgDataRight;
            p_lstLeftStepInfo = af.m_lstLeftStepInfo;
            p_lstRightStepInfo = af.m_lstRightStepInfo;
            p_afs = af.m_afs;
            p_treeRoot = new TreeRoot("AutoFocus_ViewModel", vision.m_log);
            af.RunTree(p_treeRoot, Tree.eMode.RegRead);
            af.RunTree(p_treeRoot, Tree.eMode.Init);
            p_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
        }

        private void M_treeRoot_UpdateTree()
        {
            m_RunAutoFocus.RunTree(p_treeRoot, Tree.eMode.Update);
            m_RunAutoFocus.RunTree(p_treeRoot, Tree.eMode.Init);
            m_RunAutoFocus.RunTree(p_treeRoot, Tree.eMode.RegWrite);
        }

        public void OnOkButton()
        {
            
        }

        public void OnCancelButton()
        {
            
        }

        public void OnLeftSideDoubleClick(object obj)
        {
            if (obj != null)
            {
                SideVision.Run_AutoFocus.CStepInfo si = (SideVision.Run_AutoFocus.CStepInfo)obj;
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
                SideVision.Run_AutoFocus.CStepInfo si = (SideVision.Run_AutoFocus.CStepInfo)obj;
                p_bmpSrcRightViewer = si.p_img;
                p_strRightSelectedInfo = si.p_strInfo;
            }

            if (p_eRightViewerVisibility == Visibility.Collapsed) p_eRightViewerVisibility = Visibility.Visible;
            else p_eRightViewerVisibility = Visibility.Collapsed;

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

        public bool? DialogResult => this.DialogResult;
    }
}
