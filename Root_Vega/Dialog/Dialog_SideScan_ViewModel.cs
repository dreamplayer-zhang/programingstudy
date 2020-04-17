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
        SideVision.Run_SideGrab m_RunGrab;
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

        eScanPos m_eSelScanPos = eScanPos.Bottom;
        public eScanPos p_eSelScanPos
        {
            get
            {
                return m_eSelScanPos;
            }
            set
            {
                SetProperty(ref m_eSelScanPos, value);
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
                if (m_SelGrabMode.m_camera.GetRoiSize().X != 0)
                    p_ScanWholeLine = (int)Math.Ceiling(m_RunGrab.m_xLine * 1000 / (m_SelGrabMode.m_camera.GetRoiSize().X * m_RunGrab.m_fRes));
            }
        }

        public Dialog_SideScan_ViewModel(SideVision vision, SideVision.Run_SideGrab grab)
        {
            m_Vision = vision;
            m_RunGrab = grab;
            p_GrabMode = m_Vision.m_aGrabMode;
            if (p_GrabMode.Count != 0)
            {
                p_SelGrabMode = p_GrabMode[0];
            }
            //p_StartY = Convert.ToInt32( GrabRun.m_rpAxis.X);
        }

        public void OnOkButton()
        {
            if (p_SelGrabMode == null)
                return;
            p_SelGrabMode.m_ScanLineNum = p_ScanNum;
            p_SelGrabMode.m_ScanStartLine = p_StartLine;
            m_RunGrab.m_grabMode = p_SelGrabMode;
            m_RunGrab.m_eScanPos = p_eSelScanPos;
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
