using Root_Vega.Module;
using RootTools;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_Vega
{
    public class Dialog_AutoFocus_ViewModel : ObservableObject, IDialogRequestClose
    {
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
        SideVision m_Vision;
        SideVision.Run_AutoFocus m_RunAutoFocus;
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
