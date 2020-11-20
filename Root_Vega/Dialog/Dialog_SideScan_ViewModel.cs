using Root_Vega.Module;
using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_Vega
{
    class Dialog_SideScan_ViewModel: ObservableObject, IDialogRequestClose
    {
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
        SideVision m_Vision;
        SideVision.Run_SideGrab m_RunSideGrab;
        int m_StartY = 0;
        public int p_StartLine
        {
            get
            {
                return m_StartY;
            }
            set
            {
                SetProperty(ref m_StartY, value);
            }
        }
        
        SideVision.Run_BevelGrab m_RunBevelGrab;
        
        int m_ScanNum = 0;
        public int p_ScanNum
        {
            get
            {
                return m_ScanNum;
            }
            set
            {
                SetProperty(ref m_ScanNum, value);
            }
        }

        int m_ScanWholeLine = 0;
        public int p_ScanWholeLine
        {
            get
            {
                return m_ScanWholeLine;
            }
            set
            {
                SetProperty(ref m_ScanWholeLine, value);
            }
        }

        ObservableCollection<GrabMode> m_GrabMode= new ObservableCollection<GrabMode>();
        public ObservableCollection<GrabMode> p_GrabMode
        {
            get{
                return m_GrabMode;
            }
            set{
                SetProperty(ref  m_GrabMode,value);
            }
        }
        GrabMode m_SelGrabMode = null;
        public GrabMode p_SelGrabMode
        {
            get
            {
                return m_SelGrabMode;
            }
            set
            {  
                SetProperty(ref m_SelGrabMode, value);
                p_StartLine = m_SelGrabMode.m_ScanStartLine;
                p_ScanNum = m_SelGrabMode.m_ScanLineNum;
                if (m_SelGrabMode.m_camera.p_sz.X != 0)
                    p_ScanWholeLine = (int)Math.Ceiling(m_RunSideGrab.m_xLine * 1000 / (m_SelGrabMode.m_camera.p_sz.X * m_RunSideGrab.m_dResY_um));
            }
        }

        public Dialog_SideScan_ViewModel(SideVision vision, SideVision.Run_SideGrab SideGrab, SideVision.Run_BevelGrab BevelGrab)
        {
            m_Vision = vision;
            m_RunSideGrab = SideGrab;
            m_RunBevelGrab = BevelGrab;
            p_GrabMode = m_Vision.m_aGrabMode;
            if (p_GrabMode.Count != 0)
            {
                p_SelGrabMode = p_GrabMode[0];
            }
            //p_StartY = Convert.ToInt32( GrabRun.m_rpAxis.X);
        }

        public void OnFullScanButton()
        {
            // Full Scan일 경우 Side/Bevel GrabMode를 모두 null로 Setting
            m_RunSideGrab.m_grabMode = null;
            m_RunBevelGrab.m_grabMode = null;
            CloseRequested(this, new DialogCloseRequestedEventArgs(true));
        }

        public void OnOkButton()
        {
            if (p_SelGrabMode == null)
                return;

            GrabMode grabModeTemp = GrabMode.Copy(p_SelGrabMode);
            grabModeTemp.m_ScanLineNum = p_ScanNum;
            grabModeTemp.m_ScanStartLine = p_StartLine;

            if(grabModeTemp.p_sName.IndexOf("Side") >=0)
            {
                m_RunSideGrab.m_grabMode = grabModeTemp;
                m_RunBevelGrab.m_grabMode = null;
            }
            else if(grabModeTemp.p_sName.IndexOf("Bevel")>=0)
            {
                m_RunBevelGrab.m_grabMode = grabModeTemp;
                m_RunSideGrab.m_grabMode = null;
            }
            CloseRequested(this, new DialogCloseRequestedEventArgs(true));
        }

        public void OnCancelButton()
        {
            CloseRequested(this, new DialogCloseRequestedEventArgs(false));
        }

        public void PlusScanNumber()
        {
            p_ScanNum++;
        }
        
        public void MinusScanNumber()
        {
            p_ScanNum--;
        }

        public void PlusScanStartLine()
        {
            p_StartLine++;
        }

        public void MinusScanStartLine()
        {
            p_StartLine--;
        }

        public RelayCommand PlusScanNumCommand
        {
            get
            {
                return new RelayCommand(PlusScanNumber);
            }
        }

        public RelayCommand MinusScanNumCommand
        {
            get
            {
                return new RelayCommand(MinusScanNumber);
            }
        }
        public RelayCommand PlusScanStartLineCommand
        {
            get
            {
                return new RelayCommand(PlusScanStartLine);
            }
        }
        public RelayCommand MinusScanStartLineCommand
        {
            get
            {
                return new RelayCommand(MinusScanStartLine);
            }
        }

        public RelayCommand FullScanCommand
        {
            get
            {
                return new RelayCommand(OnFullScanButton);
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
