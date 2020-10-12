using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Module;
using RootTools.Printer;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading;

namespace Root_ASIS.Module
{
    public class Trays : ModuleBase
    {
        #region ToolBox
        DIO_I m_diFull;
        DIO_I m_diOpen;
        DIO_Is m_diProduct;
        SRP350 m_srp350; 
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_diFull, this, "Strip Full");
            p_sInfo = m_toolBox.Get(ref m_diOpen, this, "Tray Door Open");
            p_sInfo = m_toolBox.Get(ref m_diProduct, this, "Product Check", "Product", p_lTray);
            m_aLED[0].GetTools(m_toolBox, bInit);
            m_aLED[1].GetTools(m_toolBox, bInit);
            p_sInfo = m_toolBox.Get(ref m_srp350, this, "Printer"); 
            if (bInit) InitTools();
        }

        void InitTools() { }
        #endregion

        #region DI
        bool _bOpen = false; 
        public bool p_bOpen
        {
            get { return _bOpen; }
            set
            {
                if (_bOpen == value) return;
                _bOpen = value;
                OnPropertyChanged();
                if (EQ.p_eState == EQ.eState.Run)
                {
                    EQ.p_bStop = true;
                    p_sInfo = "Tray Open";
                }
            }
        }

        bool _bFull = false;
        public bool p_bFull
        {
            get { return _bFull; }
            set
            {
                if (_bFull == value) return;
                _bFull = value;
                OnPropertyChanged();
                if (EQ.p_eState == EQ.eState.Run)
                {
                    EQ.p_bStop = true;
                    p_sInfo = "Tray Full";
                }
            }
        }
        #endregion

        #region LED Display
        public class LED
        {
            public RS232 m_rs232; 
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_trays.p_sInfo = toolBox.Get(ref m_rs232, m_trays, m_id);
                if (bInit)
                {
                    m_rs232.OnRecieve += M_rs232_OnRecieve;
                    m_rs232.p_bConnect = true;
                    m_bgw.RunWorkerAsync(); 
                }
            }

            bool m_bSend = false; 
            private void M_rs232_OnRecieve(string sRead)
            {
                Thread.Sleep(10); 
                m_bSend = false; 
            }

            public Queue<string> m_qSend = new Queue<string>();
            bool m_bBGW = false; 
            BackgroundWorker m_bgw = new BackgroundWorker();
            private void M_bgw_DoWork(object sender, DoWorkEventArgs e)
            {
                m_bBGW = true; 
                while (m_bBGW)
                {
                    Thread.Sleep(10); 
                    if ((m_bSend == false) && (m_qSend.Count > 0))
                    {
                        m_bSend = true;
                        m_rs232.Send(m_qSend.Dequeue()); 
                    }
                }
            }

            string m_id; 
            Trays m_trays;
            public LED(string id, Trays trays)
            {
                m_id = id; 
                m_trays = trays;
                m_bgw.DoWork += M_bgw_DoWork;
            }

            public void ThreadStop()
            {
                m_bBGW = false;
                while (m_bgw.IsBusy) Thread.Sleep(10); 
            }
        }
        LED[] m_aLED = new LED[2]; 
        void InitLED()
        {
            m_aLED[0] = new LED("LED0", this);
            m_aLED[1] = new LED("LED1", this);
        }

        ushort[] c_aCRC = new ushort[256]
        {
            0x0000, 0xC0C1, 0xC181, 0x0140, 0xC301, 0x03C0, 0x0280, 0xC241,
            0xC601, 0x06C0, 0x0780, 0xC741, 0x0500, 0xC5C1, 0xC481, 0x0440,
            0xCC01, 0x0CC0, 0x0D80, 0xCD41, 0x0F00, 0xCFC1, 0xCE81, 0x0E40,
            0x0A00, 0xCAC1, 0xCB81, 0x0B40, 0xC901, 0x09C0, 0x0880, 0xC841,
            0xD801, 0x18C0, 0x1980, 0xD941, 0x1B00, 0xDBC1, 0xDA81, 0x1A40,
            0x1E00, 0xDEC1, 0xDF81, 0x1F40, 0xDD01, 0x1DC0, 0x1C80, 0xDC41,
            0x1400, 0xD4C1, 0xD581, 0x1540, 0xD701, 0x17C0, 0x1680, 0xD641,
            0xD201, 0x12C0, 0x1380, 0xD341, 0x1100, 0xD1C1, 0xD081, 0x1040,
            0xF001, 0x30C0, 0x3180, 0xF141, 0x3300, 0xF3C1, 0xF281, 0x3240,
            0x3600, 0xF6C1, 0xF781, 0x3740, 0xF501, 0x35C0, 0x3480, 0xF441,
            0x3C00, 0xFCC1, 0xFD81, 0x3D40, 0xFF01, 0x3FC0, 0x3E80, 0xFE41,
            0xFA01, 0x3AC0, 0x3B80, 0xFB41, 0x3900, 0xF9C1, 0xF881, 0x3840,
            0x2800, 0xE8C1, 0xE981, 0x2940, 0xEB01, 0x2BC0, 0x2A80, 0xEA41,
            0xEE01, 0x2EC0, 0x2F80, 0xEF41, 0x2D00, 0xEDC1, 0xEC81, 0x2C40,
            0xE401, 0x24C0, 0x2580, 0xE541, 0x2700, 0xE7C1, 0xE681, 0x2640,
            0x2200, 0xE2C1, 0xE381, 0x2340, 0xE101, 0x21C0, 0x2080, 0xE041,
            0xA001, 0x60C0, 0x6180, 0xA141, 0x6300, 0xA3C1, 0xA281, 0x6240,
            0x6600, 0xA6C1, 0xA781, 0x6740, 0xA501, 0x65C0, 0x6480, 0xA441,
            0x6C00, 0xACC1, 0xAD81, 0x6D40, 0xAF01, 0x6FC0, 0x6E80, 0xAE41,
            0xAA01, 0x6AC0, 0x6B80, 0xAB41, 0x6900, 0xA9C1, 0xA881, 0x6840,
            0x7800, 0xB8C1, 0xB981, 0x7940, 0xBB01, 0x7BC0, 0x7A80, 0xBA41,
            0xBE01, 0x7EC0, 0x7F80, 0xBF41, 0x7D00, 0xBDC1, 0xBC81, 0x7C40,
            0xB401, 0x74C0, 0x7580, 0xB541, 0x7700, 0xB7C1, 0xB681, 0x7640,
            0x7200, 0xB2C1, 0xB381, 0x7340, 0xB101, 0x71C0, 0x7080, 0xB041,
            0x5000, 0x90C1, 0x9181, 0x5140, 0x9301, 0x53C0, 0x5280, 0x9241,
            0x9601, 0x56C0, 0x5780, 0x9741, 0x5500, 0x95C1, 0x9481, 0x5440,
            0x9C01, 0x5CC0, 0x5D80, 0x9D41, 0x5F00, 0x9FC1, 0x9E81, 0x5E40,
            0x5A00, 0x9AC1, 0x9B81, 0x5B40, 0x9901, 0x59C0, 0x5880, 0x9841,
            0x8801, 0x48C0, 0x4980, 0x8941, 0x4B00, 0x8BC1, 0x8A81, 0x4A40,
            0x4E00, 0x8EC1, 0x8F81, 0x4F40, 0x8D01, 0x4DC0, 0x4C80, 0x8C41,
            0x4400, 0x84C1, 0x8581, 0x4540, 0x8701, 0x47C0, 0x4680, 0x8641,
            0x8201, 0x42C0, 0x4380, 0x8341, 0x4100, 0x81C1, 0x8081, 0x4040
        }; 

        byte[] m_aCode = new byte[13] { 0, 0x10, 0, 1, 0, 2, 4, 0x3f, 0x3f, 0x3f, 0x3f, 0, 0 }; 
        void AddLED(int nTray, string sLED)
        {
            int nLED = nTray / m_lLED;
            int nAdd = nTray % m_lLED;
            m_aCode[0] = (byte)nAdd;
            int i = 7;
            foreach (char ch in sLED) m_aCode[i++] = GetCode(ch);
            ushort wCRC = 0xffff; 
            for (int n = 0; n < 11; n++)
            {
                int nTemp = m_aCode[n] ^ wCRC;
                wCRC >>= 8;
                wCRC ^= c_aCRC[nTemp]; 
            }
            m_aCode[11] = (byte)(wCRC % 256); 
            m_aCode[12] = (byte)(wCRC / 256); 
            m_aLED[nLED].m_qSend.Enqueue(Encoding.ASCII.GetString(m_aCode, 0, 13));
        }

        byte GetCode(char ch)
        {
            if ((ch >= '0') && (ch <= '9')) return (byte)(ch - '0');
            if (ch >= 'a') return (byte)(ch - 'a' + 10);
            return (byte)(ch - 'A' + 10); 
        }

        int m_lLED = 30; 
        void RunTreeLED(Tree tree)
        {
            m_lLED = tree.Set(m_lLED, m_lLED, "lLED", "RS232 (0) Tray Count");
        }
        #endregion

        #region Tray
        public class Tray : NotifyProperty
        {
            int _nCount = 0;
            public int p_nCount
            {
                get { return _nCount; }
                set
                {
                    if (_nCount == value) return;
                    if ((m_count != null) && (value > _nCount)) m_count.p_nCount += (value - _nCount); 
                    _nCount = value;
                    OnPropertyChanged();
                }
            }
            public Count m_count;

            bool _bProduct = false; 
            public bool p_bProduct
            {
                get { return _bProduct; }
                set
                {
                    if (_bProduct == value) return;
                    _bProduct = value;
                    OnPropertyChanged(); 
                }
            }

            string _sTray = "Tray"; 
            public string p_sTray
            {
                get { return _sTray; }
                set
                {
                    _sTray = value;
                    OnPropertyChanged(); 
                }
            }
            public int m_nXout = -1; 

            string _sLED = "Tray";
            public string p_sLED
            {
                get { return _sLED; }
                set
                {
                    if (_sLED == value) return; 
                    _sLED = value;
                    OnPropertyChanged();
                    m_trays.AddLED(p_iTray, value); 
                }
            }

            public void Check(bool bProduct)
            {
                p_bProduct = bProduct; 
                if (p_nCount > 0)
                {
                    p_sLED = bProduct ? p_nCount.ToString("0000") : "EMPY"; 
                }
                else
                {
                    p_sLED = bProduct ? "CHEK" : p_sTray.Substring(0, 4); 
                }
            }

            public int p_iTray { get; set; }
            Trays m_trays;
            public Tray(int iTray, Trays trays)
            {
                p_iTray = iTray; 
                m_trays = trays; 
            }
        }
        public List<Tray> m_aTray = new List<Tray>(); 
        void InitTray()
        {
            while (m_aTray.Count < p_lTray) m_aTray.Add(new Tray(m_aTray.Count, this));
        }

        public void ClearTray()
        {
            foreach (Tray tray in m_aTray) tray.p_nCount = 0; 
        }
        #endregion

        #region Tray Size
        CPoint _szTray = new CPoint(0, 0);
        public CPoint p_szTray
        {
            get { return _szTray; }
            set
            {
                _szTray = value;
                OnPropertyChanged();
                m_reg.Write("szTray", _szTray);
                InitTray();
            }
        }

        int p_lTray { get { return p_szTray.X * p_szTray.Y; } }

        void InitBoxCount()
        {
            p_szTray = m_reg.Read("szTray", new CPoint(10, 5));
        }

        void RunTreeTray(Tree tree)
        {
            p_szTray = tree.Set(p_szTray, p_szTray, "Count", "Tray Count");
        }
        #endregion

        #region Sorting
        int m_lGood = 3;
        int[] m_alXout = new int[10] { 0, 2, 2, 1, 1, 1, 1, 1, 1, 1 };
        int m_lETC = 1;
        int m_lRework = 1;
        int m_lError = 1;

        int _nMaxSorting = 50; 
        public int p_nMaxSorting
        {
            get { return _nMaxSorting; }
            set
            {
                if (_nMaxSorting == value) return;
                _nMaxSorting = value;
                OnPropertyChanged(); 
            }
        }

        void RunTreeSorting(Tree tree)
        {
            RunTreeSortingTray(tree.GetTree("Tray", false));
        }

        void RunTreeSortingTray(Tree tree)
        {
            m_lGood = tree.Set(m_lGood, m_lGood, "Good", "Tray Count");
            for (int n = 1; n < 10; n++) m_alXout[n] = tree.Set(m_alXout[n], m_alXout[n], "XOut " + n.ToString(), "Tray Count");
            m_lETC = tree.Set(m_lETC, m_lETC, "ETC", "Tray Count");
            m_lRework = tree.Set(m_lRework, m_lRework, "Rework", "Tray Count");
            m_lError = tree.Set(m_lError, m_lError, "Error", "Tray Count");
            if (tree.IsUpdated())
            {
                InitSorting();
                InitCount();
            }
        }

        void InitSorting()
        {
            int nTray = 0;
            for (int n = 0; n < m_lGood; n++, nTray++)
            {
                m_aTray[nTray].p_sTray = GetTrayName(InfoStrip.eResult.Xout, 0);
                m_aTray[nTray].m_nXout = 0; 
            }
            for (int n = 1; n < 10; n++)
            {
                for (int i = 0; i < m_alXout[n]; i++, nTray++)
                {
                    m_aTray[nTray].p_sTray = GetTrayName(InfoStrip.eResult.Xout, n);
                    m_aTray[nTray].m_nXout = n;
                }
            }
            int lXout = p_lTray - nTray - m_lError - m_lRework - m_lETC;
            for (int n = 0; n < lXout; n++, nTray++)
            {
                m_aTray[nTray].p_sTray = GetTrayName(InfoStrip.eResult.Xout, n);
                m_aTray[nTray].m_nXout = n;
            }
            for (int n = 0; n < m_lETC; n++, nTray++) m_aTray[nTray].p_sTray = c_sEtc;
            for (int n = 0; n < m_lRework; n++, nTray++) m_aTray[nTray].p_sTray = GetTrayName(InfoStrip.eResult.Rework);
            for (int n = 0; n < m_lError; n++, nTray++) m_aTray[nTray].p_sTray = GetTrayName(InfoStrip.eResult.Error);
        }

        const string c_sEtc = "Etc "; 
        string GetTrayName(InfoStrip.eResult eResult, int nXOut = 0)
        {
            switch (eResult)
            {
                case InfoStrip.eResult.Xout: 
                    if (nXOut == 0) return "Good";
                    else return "X" + nXOut.ToString("000");
                case InfoStrip.eResult.Rework: return "Rewk";
                case InfoStrip.eResult.Error: return "Errr"; 
            }
            return c_sEtc; 
        }

        public CPoint GetTrayPosition(InfoStrip infoStip)
        {
            CPoint cpTray = new CPoint(0, 0); 
            string sTray = GetTrayName(infoStip.p_eResult, infoStip.m_nXout);
            if (GetTrayPosition(sTray, ref cpTray) == "OK") return cpTray;
            GetTrayPosition(c_sEtc, ref cpTray);
            return cpTray;
        }

        string GetTrayPosition(string sTray, ref CPoint cpTray)
        {
            for (int n = 0; n < p_lTray; n++)
            {
                if (m_aTray[n].p_sTray == sTray)
                {
                    cpTray.X = n % p_szTray.X;
                    cpTray.Y = n / p_szTray.Y; 
                    if (m_aTray[n].p_nCount < p_nMaxSorting) return "OK"; 
                }
            }
            return "Not found"; 
        }

        public void AddSort(CPoint cpTray)
        {
            int nTray = cpTray.X + cpTray.Y * p_szTray.X;
            m_aTray[nTray].p_nCount++;
        }
        #endregion

        #region Count
        public class Count : NotifyProperty
        {
            string _sName = ""; 
            public string p_sName
            {
                get { return _sName; }
                set
                {
                    _sName = value;
                    OnPropertyChanged(); 
                }
            }
            public int m_nXout = 0; 

            int _nCount = 0;
            public int p_nCount
            {
                get { return _nCount; }
                set
                {
                    _nCount = value;
                    OnPropertyChanged(); 
                }
            }

            public Count(string sName, int nXout)
            {
                p_sName = sName;
                m_nXout = nXout; 
                p_nCount = 0; 
            }
        }
        public ObservableCollection<Count> m_aCount = new ObservableCollection<Count>(); 
        void InitCount()
        {
            m_aCount.Clear(); 
            foreach (Tray tray in m_aTray) tray.m_count = GetCountList(tray);
        }

        Count GetCountList(Tray tray)
        {
            foreach (Count count in m_aCount)
            {
                if (count.p_sName == tray.p_sTray) return count; 
            }
            Count newCount = new Count(tray.p_sTray, tray.m_nXout); 
            m_aCount.Add(newCount);
            return newCount; 
        }

        public void ClearCount()
        {
            foreach (Count count in m_aCount) count.p_nCount = 0; 
        }
        #endregion

        #region Paper
        public CPoint m_cpNeedPaper = null;
        #endregion

        #region Override
        public override string StateReady()
        {
            p_bOpen = m_diOpen.p_bIn;
            p_bFull = m_diFull.p_bIn;
            if (p_bOpen) return "OK";
            for (int n = 0; n < p_lTray; n++) m_aTray[n].Check(m_diProduct.ReadDI(n)); 
            return "OK";
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            RunTreeTray(tree.GetTree("Tray", false));
            RunTreeLED(tree.GetTree("LED", false));
            RunTreeSorting(tree.GetTree("Sorting", false));
        }

        public override void Reset()
        {
            base.Reset();
        }
        #endregion

        Registry m_reg;
        public Trays(string id, IEngineer engineer)
        {
            m_reg = new Registry(id);
            InitBoxCount();
            InitLED(); 
            base.InitBase(id, engineer);
            InitSorting();
            InitCount(); 
        }

        public override void ThreadStop()
        {
            Reset();
            base.ThreadStop();
        }
    }
}
