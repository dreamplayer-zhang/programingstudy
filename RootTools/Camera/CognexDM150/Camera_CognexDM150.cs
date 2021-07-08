using Cognex.DataMan.SDK;
using Cognex.DataMan.SDK.Discovery;
using RootTools.Memory;
using RootTools.Trees;
using System;
using System.Drawing;
using System.Net;
using System.Text;
using System.Windows.Controls;
using System.Xml;

namespace RootTools.Camera.CognexDM150
{
    public class Camera_CognexDM150 : NotifyProperty, ICamera
    {
        public event System.EventHandler Grabed;
        void _Dummy()
        {
            if (Grabed != null) Grabed(null, null);
        }

        #region Property
        public string p_id { get; set; }

        string _sInfo = "Info";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (_sInfo == value) return;
                _sInfo = value;
                OnPropertyChanged();
                if (value == "OK") return;
                p_sBCD = "Error";
            }
        }

        string _sBCD = "BCD";
        public string p_sBCD
        {
            get { return _sBCD; }
            set
            {
                _sBCD = value;
                OnPropertyChanged();
            }
        }

        public bool m_bReadDone = false; 
        #endregion

        #region RS232
        public class RS232
        {
            public SerSystemDiscoverer m_discover;
            public SerSystemConnector m_connect;

            Camera_CognexDM150 m_cam;
            public void Init(Camera_CognexDM150 cam)
            {
                m_cam = cam; 
                try
                {
                    m_discover = new SerSystemDiscoverer();
                    m_discover.SystemDiscovered += M_discover_SystemDiscovered;
                    m_discover.Discover();
                }
                catch (Exception e) { cam.m_log.Info(e.Message); }
            }

            public void ThreadStop()
            {
                m_discover.Dispose();
            }

            string m_sPort = "COM10";
            public void Connect()
            {
                m_connect = new SerSystemConnector(m_sPort);
            }

            public void RunTree(Tree tree)
            {
                m_sPort = tree.Set(m_sPort, m_sPort, "Port", "DM150 RS232 Port ID");
            }

            private void M_discover_SystemDiscovered(SerSystemDiscoverer.SystemInfo systemInfo)
            {
                if ((systemInfo.PortName != m_sPort) || (m_connect != null)) return;
                m_cam.Connect(); 
            }
        }
        #endregion

        #region TCPIP
        public class TCPIP
        {
            public EthSystemDiscoverer m_discover;
            public EthSystemConnector m_connect;

            Camera_CognexDM150 m_cam; 
            public void Init(Camera_CognexDM150 cam)
            {
                m_cam = cam; 
                try
                {
                    m_discover = new EthSystemDiscoverer();
                    m_discover.SystemDiscovered += M_discover_SystemDiscovered;
                    m_discover.Discover();

                }
                catch (Exception e) { cam.m_log.Info(e.Message); }
            }

            public void ThreadStop()
            {
                m_discover.Dispose();
            }

            string m_sIP = "192.0.0.0";
            public void Connect()
            {
                m_connect = new EthSystemConnector(IPAddress.Parse(m_sIP));
            }

            public void RunTree(Tree tree)
            {
                m_sIP = tree.Set(m_sIP, m_sIP, "IP Address", "DM150 TCPIP IP Address");
            }

            private void M_discover_SystemDiscovered(EthSystemDiscoverer.SystemInfo systemInfo)
            {
                if ((systemInfo.IPAddress.ToString() != m_sIP) || (m_connect != null)) return;
                m_cam.Connect(); 
            }
        }
        #endregion

        #region Communicate
        public enum eComm
        {
            RS232,
            TCPIP
        };
        eComm m_eComm = eComm.RS232;
        void RunTreeComm(Tree tree)
        {
            m_eComm = (eComm)tree.Set(m_eComm, m_eComm, "Communicate", "Communication Type"); 
        }

        RS232 m_rs232 = new RS232();
        TCPIP m_tcpip = new TCPIP(); 
        void InitComm()
        {
            switch (m_eComm)
            {
                case eComm.RS232: m_rs232.Init(this); break;
                case eComm.TCPIP: m_tcpip.Init(this); break; 
            }
        }
        #endregion

        #region Connect
        DataManSystem m_dm; 
        public bool Connect()
        {
            try
            {
                switch (m_eComm)
                {
                    case eComm.RS232:
                        m_rs232.Connect();
                        if (m_rs232.m_connect == null) return false;
                        m_dm = new DataManSystem(m_rs232.m_connect);
                        break;
                    case eComm.TCPIP:
                        m_tcpip.Connect();
                        if (m_tcpip.m_connect == null) return false;
                        m_dm = new DataManSystem(m_tcpip.m_connect);
                        break;
                }
                m_dm.SystemConnected += M_dm_SystemConnected;
                m_dm.SystemDisconnected += M_dm_SystemDisconnected;
                m_dm.XmlResultArrived += M_dm_XmlResultArrived;
                m_dm.ImageArrived += M_dm_ImageArrived;
                m_dm.Connect();
                m_dm.SetResultTypes(ResultTypes.ReadXml | ResultTypes.Image | ResultTypes.ImageGraphics);
                return true;
            }
            catch (Exception e) 
            { 
                m_log.Info(e.Message);
                return false; 
            }
        }

        private void M_dm_SystemConnected(object sender, EventArgs args)
        {
            m_log.Info("Connect OK");
        }

        private void M_dm_SystemDisconnected(object sender, EventArgs args)
        {
            m_log.Info("Disconnected");
            m_dm.Disconnect(); 
            m_dm.Dispose(); 
            switch (m_eComm)
            {
                case eComm.RS232:
                    m_rs232.m_connect.Disconnect();
                    m_rs232.m_connect.Dispose();
                    break;
                case eComm.TCPIP:
                    m_tcpip.m_connect.Disconnect();
                    m_tcpip.m_connect.Dispose();
                    break; 
            }
        }

        private void M_dm_XmlResultArrived(object sender, XmlResultArrivedEventArgs args)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(args.XmlResult); 
                XmlNode result = doc.SelectSingleNode("result/general/status");
                if (result == null || result.InnerText != "GOOD READ") return;
                XmlNode fullStringNode = doc.SelectSingleNode("result/general/full_string");
                if (fullStringNode == null) return;
                XmlAttribute encoding = fullStringNode.Attributes["encoding"];
                if (encoding != null && encoding.InnerText == "base64")
                {
                    byte[] code = Convert.FromBase64String(fullStringNode.InnerText);
                    p_sBCD = Encoding.ASCII.GetString(code);
                }
            }
            catch (Exception e)
            {
                m_log.Info(e.Message);
                m_bReadDone = true;
            }
        }

        //Bitmap m_image;
        private void M_dm_ImageArrived(object sender, ImageArrivedEventArgs args)
        {
            //m_image = new Bitmap(args.Image);
            //m_image.Save("D:\\BarcodeOrg.bmp");
        }
        #endregion

        #region Read BCD
        public string ReadBCD()
        {
            m_bReadDone = false;
            try
            {
                if (m_rs232.m_connect == null)
                    Connect();

                m_dm.SendCommand("TRIGGER ON");
            }
            catch(Exception e)
            {
                m_log.Info("Send Command Fail !!");
                return "Send Command Fail !!";
            }
            return "OK";
        }
        #endregion

        #region UI
        public UserControl p_ui
        {
            get
            {
                Camera_CognexDM150_UI ui = new Camera_CognexDM150_UI();
                ui.Init(this);
                return ui;
            }
        }
        #endregion

        #region Tree
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            p_treeRoot.p_eMode = mode;
            RunTree(p_treeRoot);
        }

        void RunTree(Tree treeRoot)
        {
            RunTreeComm(treeRoot.GetTree("Type"));
            switch (m_eComm)
            {
                case eComm.RS232: m_rs232.RunTree(treeRoot.GetTree("RS232")); break;
                case eComm.TCPIP: m_tcpip.RunTree(treeRoot.GetTree("TCPIP")); break;
            }
        }
        #endregion

        Log m_log;
        public TreeRoot p_treeRoot { get; set; }

        public Camera_CognexDM150(string id, Log log)
        {
            p_id = id;
            m_log = log;

            p_treeRoot = new TreeRoot(id, log);
            p_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);

            InitComm(); 
        }

        public void ThreadStop()
        {
            switch (m_eComm)
            {
                case eComm.RS232: m_rs232.ThreadStop(); break;
                case eComm.TCPIP: m_tcpip.ThreadStop(); break; 
            }
        }

        #region ICamera
        public bool bStopThread { get; set; }
        public int p_nGrabProgress { get; set; }
        public CPoint p_sz { get; set; }
        public CPoint GetRoiSize() { return null; }
        public double GetFps() { return 0; }
        public void GrabLineScan(MemoryData memory, CPoint cpScanOffset, int nLine, GrabData m_GrabData = null, bool bTest = false) { }
        public string StopGrab() { return "FALSE"; }
        public void GrabLineScanColor(MemoryData memory, CPoint cpScanOffset, int nLine, GrabData m_GrabData = null) { }
        #endregion

    }
}
