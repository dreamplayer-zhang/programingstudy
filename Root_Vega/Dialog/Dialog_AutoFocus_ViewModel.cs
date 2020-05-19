using Root_Vega.Module;
using RootTools;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Root_Vega
{
    public class Dialog_AutoFocus_ViewModel : ObservableObject, IDialogRequestClose
    {
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
        SideVision m_Vision;
        SideVision.Run_AutoFocus m_RunAutoFocus;

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

        SideVision.Run_AutoFocus.StepInfoList m_lstLeftStepInfo;
        public SideVision.Run_AutoFocus.StepInfoList p_lstLeftStepInfo
        {
            get
            {
                return m_lstLeftStepInfo;
            }
            set
            {
                SetProperty(ref m_lstLeftStepInfo, value);
            }
        }
        SideVision.Run_AutoFocus.StepInfoList m_lstRightStepInfo;
        public SideVision.Run_AutoFocus.StepInfoList p_lstRightStepInfo
        {
            get
            {
                return m_lstRightStepInfo;
            }
            set
            {
                SetProperty(ref m_lstRightStepInfo, value);
            }
        }
        ImageViewer_ViewModel m_ImageViewerLeft = new ImageViewer_ViewModel();
        public ImageViewer_ViewModel p_ImageViewerLeft
        {
            get
            {
                return m_ImageViewerLeft;
            }
            set
            {
                SetProperty(ref m_ImageViewerLeft, value);
                
            }
        }
        ImageViewer_ViewModel m_ImageViewerRight = new ImageViewer_ViewModel();
        public ImageViewer_ViewModel p_ImageViewerRight
        {
            get
            {
                return m_ImageViewerRight;
            }
            set
            {
                SetProperty(ref m_ImageViewerRight, value);
            }
        }
        string m_strLeftStatus;
        public string p_strLeftStatus
        {
            get
            {
                return m_strLeftStatus;
            }
            set
            {
                SetProperty(ref m_strLeftStatus, value);
            }
        }

        string m_strRightStatus;
        public string p_strRightStatus
        {
            get
            {
                return m_strRightStatus;
            }
            set
            {
                SetProperty(ref m_strRightStatus, value);
            }
        }

        public Dialog_AutoFocus_ViewModel(SideVision vision, SideVision.Run_AutoFocus af)
        {
            m_Vision = vision;
            m_RunAutoFocus = af;
            p_ImageViewerLeft.p_ImageData = af.m_imgDataLeft;
            p_ImageViewerRight.p_ImageData = af.m_imgDataRight;
            p_lstLeftStepInfo = af.m_lstLeftStepInfo;
            p_lstRightStepInfo = af.m_lstRightStepInfo;
        }

        public void OnOkButton()
        {
            m_Vision.StartRun(m_RunAutoFocus);
        }

        public void OnCancelButton()
        {
            CloseRequested(this, new DialogCloseRequestedEventArgs(false));
        }

        public void OnDoubleClick(object obj)
        {
            SideVision.Run_AutoFocus.StepInfo si = (SideVision.Run_AutoFocus.StepInfo)obj;
            p_bmpSrcLeftViewer = si.p_img;
            if (p_eLeftViewerVisibility == Visibility.Collapsed) p_eLeftViewerVisibility = Visibility.Visible;
            else p_eLeftViewerVisibility = Visibility.Collapsed;

            return;
        }

        public void OnImageDoubleClick()
        {
            if (p_eLeftViewerVisibility == Visibility.Collapsed) p_eLeftViewerVisibility = Visibility.Visible;
            else p_eLeftViewerVisibility = Visibility.Collapsed;

            return;
        }

        public RelayCommand ImageDoubleClick
        {
            get
            {
                return new RelayCommand(OnImageDoubleClick);
            }
        }

        public RelayCommandWithParameter DoubleClick
        {
            get
            {
                return new RelayCommandWithParameter(OnDoubleClick);
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
