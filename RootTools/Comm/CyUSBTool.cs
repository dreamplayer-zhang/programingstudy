using CyUSB;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace RootTools
{
    public class CyUSBTool : NotifyProperty, ITool
    {
        #region Property
        string _sInfo = "OK"; 
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (value == _sInfo) return;
                _sInfo = value;
                OnPropertyChanged();
                if (value == "OK") return;
                m_log.Warn(value);
                AddCommLog(Brushes.Red, "p_sInfo = " + _sInfo); 
            }
        }

        public bool p_bOpen { get { return (m_device != null); } }

        int _nCh = 0;
        public int p_nCh
        {
            get { return _nCh; }
            set
            {
                if (_nCh == value) return;
                _nCh = value;
                OnPropertyChanged(); 
            }
        }

        double _fPower = 0;
        public double p_fPower
        {
            get { return _fPower; }
            set
            {
                if (_fPower == value) return;
                _fPower = value;
                OnPropertyChanged(); 
            }
        }

        int _msWriteSleep = 2; 
        public int p_msWriteSleep
        {
            get { return _msWriteSleep; }
            set
            {
                if (_msWriteSleep == value) return;
                _msWriteSleep = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region UI
        public UserControl p_ui 
        { 
            get 
            {
                CyUSBTool_UI ui = new CyUSBTool_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

        #region CyUSB
        USBDeviceList m_listDevice = null;
        CyUSBDevice m_device = null;
        CyUSBDevice m_device2 = null;
        string OpenUSB()
        {
            try
            {
                m_device = null;
                Thread.Sleep(10);
                m_listDevice = new USBDeviceList(CyConst.DEVICES_CYUSB);
                if (m_listDevice == null) return "Can't Fine CyUSB Device List";
               // m_device = m_listDevice[0x547, 0x1003] as CyUSBDevice;
                m_device = m_listDevice[0] as CyUSBDevice;
                if (m_listDevice.Count == 2)
                    m_device2 = m_listDevice[1] as CyUSBDevice;
                OnPropertyChanged("p_bOpen");
                if (m_device == null) return "Can't Find Light Board";
                return "OK";
            }
            catch (Exception e)
            {
                return "OpenUSB Error : " + e.Message;
            }
        }

        static readonly object m_csLock = new object();
        public string Write(int nCh, double fPower)
        {
            lock (m_csLock)
            {
                if (m_device == null) return "Write Error  : Device not Found";
                if (nCh < 12)
                {
                    p_nCh = nCh;
                    p_fPower = fPower;
                    return Write(m_device.ControlEndPt);
                }
                else
                {
                    p_nCh = nCh % 12;
                    p_fPower = fPower;
                    return Write(m_device2.ControlEndPt);
                }
            }
        }

        byte[] m_buf = new byte[4] { 0, 0, 0, 0 };
        string Write(CyControlEndPoint ep)
        {
            ep.Target = CyConst.TGT_DEVICE;
            ep.ReqType = CyConst.REQ_VENDOR;
            ep.Direction = CyConst.DIR_TO_DEVICE;
            ep.ReqCode = 0x80;
            ep.Index = (ushort)p_nCh;
            ep.Value = (ushort)Math.Round(p_fPower);
            Thread.Sleep(1);
            try
            {
                int nL = 0;
                AddCommLog(Brushes.Black);
                return ep.XferData(ref m_buf, ref nL) ? "OK" : "CyUSB Xfer Data Error";
            }
            catch (Exception e)
            {
                Thread.Sleep(1000);
                return "CyUSB Xfer Write Error : " + e.Message;
            }
            finally { Thread.Sleep(p_msWriteSleep); }
        }

        public string Read(int nCh, ref double fPower)
        {
            lock (m_csLock)
            {
                if (m_device == null)
                    return "Read Error  : Device not Found";
                if (nCh < 12)
                {
                    p_nCh = nCh;
                    return Read(m_device.ControlEndPt, ref fPower);
                }
                else
                {
                    p_nCh = nCh % 12;
                    return Read(m_device2.ControlEndPt, ref fPower);
                }
            }
        }

        string Read(CyControlEndPoint ep, ref double fPower)
        {
            ep.Target = CyConst.TGT_DEVICE;
            ep.ReqType = CyConst.REQ_VENDOR;
            ep.Direction = CyConst.DIR_FROM_DEVICE;
            ep.ReqCode = 0x81;
            ep.Index = (ushort)p_nCh;
            ep.Value = 0;
            Thread.Sleep(1);
            try
            {
                int nL = 0;
                fPower = ep.XferData(ref m_buf, ref nL) ? (double)m_buf[0] : 0;
                p_fPower = fPower; 
                AddCommLog(Brushes.Gray); 
                return "OK"; 
            }
            catch (Exception e)
            {
                Thread.Sleep(1000);
                return "CyUSB Xfer Read Error : " + e.Message;
            }
        }
        #endregion

        #region Comm Log
        public class CommLog
        {
            public string p_sMsg { get; set; }
            public Brush p_bColor { get; set; }

            public CommLog(Brush brush, string sMsg)
            {
                p_bColor = brush; 
                p_sMsg = sMsg;
            }
        }

        public Queue<CommLog> m_aCommLog = new Queue<CommLog>();
        void AddCommLog(Brush brush, string sMsg = "")
        {
            if (sMsg == "") sMsg = "Channel = " + p_nCh.ToString() + ", Power = " + p_fPower.ToString("0.0"); 
            m_aCommLog.Enqueue(new CommLog(brush, DateTime.Now.ToLongTimeString() + " " + sMsg));
        }
        #endregion

        public string p_id { get { return m_id; } }
        string m_id;
        LogWriter m_log;
        public CyUSBTool(string id, LogWriter log)
        {
            m_id = id;
            m_log = log;
            p_sInfo = OpenUSB();
        }

        public void ThreadStop()
        {
            if (m_device != null) m_device.Dispose();
            if (m_listDevice != null) m_listDevice.Dispose();
        }
    }
}
