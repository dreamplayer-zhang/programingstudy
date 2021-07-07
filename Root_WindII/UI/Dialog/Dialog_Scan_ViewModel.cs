using Root_EFEM;
using Root_EFEM.Module.FrontsideVision;
using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WindII
{
    public class Dialog_Scan_ViewModel : ObservableObject, IDialogRequestClose
    {
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
        Vision_Frontside m_Vision;
        Run_GrabLineScan m_RunGrab;

        #region Property
        int m_nStartLine = 0;
        public int p_nStartLine
        {
            get
            {
                return m_nStartLine;
            }
            set
            {
                SetProperty(ref m_nStartLine, value);
            }
        }
        int m_nScanNum = 0;
        public int p_nScanNum
        {
            get
            {
                return m_nScanNum;
            }
            set
            {
                SetProperty(ref m_nScanNum, value);
            }
        }
        int m_nScanWholeLine = 0;
        public int p_nScanWholeLine
        {
            get
            {
                return m_nScanWholeLine;
            }
            set
            {
                SetProperty(ref m_nScanWholeLine, value);
            }
        }
        ObservableCollection<GrabModeFront> m_GrabMode = new ObservableCollection<GrabModeFront>();
        public ObservableCollection<GrabModeFront> p_GrabMode
        {
            get
            {
                return m_GrabMode;
            }
            set
            {
                SetProperty(ref m_GrabMode, value);
            }
        }
        GrabModeFront m_SelGrabMode = null;
        public GrabModeFront p_SelGrabMode
        {
            get
            {
                return m_SelGrabMode;
            }

            set
            {
                SetProperty(ref m_SelGrabMode, value);
                p_nStartLine = m_SelGrabMode.m_ScanStartLine;
                p_nScanNum = m_SelGrabMode.m_ScanLineNum;
                //if (m_SelGrabMode.m_camera.p_sz.X != 0)
                //    p_nScanWholeLine = (int)Math.Ceiling(m_RunGrab.m_nWaferSize_mm * 1000 / (m_SelGrabMode.m_camera.p_sz.X * m_RunGrab.m_dResY_um));
            }
        }
        #endregion

        public Dialog_Scan_ViewModel(Vision_Frontside vision, Run_GrabLineScan grab)
        {
            m_Vision = vision;
            m_RunGrab = grab;

            p_GrabMode = m_Vision.m_aGrabMode;
            if (p_GrabMode.Count != 0)
            {
                p_SelGrabMode = p_GrabMode[0];
            }
        }

        #region RelayCommand
        public void OnOkButton()
        {
            if (p_SelGrabMode == null)
                return;
            p_SelGrabMode.m_ScanLineNum = p_nScanNum;
            p_SelGrabMode.m_ScanStartLine = p_nStartLine;
            m_RunGrab.m_grabMode = p_SelGrabMode;
            //m_RunGrab.m_rpAxis.X -= size.X * p_StartLine * 10;
            //m_RunGrab.m_cpMemory.X += size.X * p_StartLine; 
            CloseRequested(this, new DialogCloseRequestedEventArgs(true));
        }

        public void OnCancelButton()
        {
            CloseRequested(this, new DialogCloseRequestedEventArgs(false));
        }

        public void PlusScanNumber()
        {
            p_nScanNum++;
        }

        public void MinusScanNumber()
        {
            p_nScanNum--;
        }

        public void PlusScanStartLine()
        {
            p_nStartLine++;
        }

        public void MinusScanStartLine()
        {
            p_nStartLine--;
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
        #endregion
    }
}
