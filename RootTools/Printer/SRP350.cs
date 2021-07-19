using System.Windows.Controls;

namespace RootTools.Printer
{
    public class SRP350 : NotifyProperty, ITool
    {
        #region Connect
        Registry m_reg;
        bool _bConnect = false;
        public bool p_bConnect
        {
            get { return _bConnect; }
            set
            {
                if (_bConnect == value) return;
                //RootTools/Printer/DLL/BXLPAPI.dll -> Copy to Project/bin/...
                if (value)
                {
                    int nConnect = BXLAPI.PrinterOpen(BXLAPI.IUsb, "", 0, 0, 0, 0, 0);
                    if (nConnect == BXLAPI.BXL_SUCCESS) _bConnect = true; 
                }
                else
                {
                    BXLAPI.PrinterClose();
                    _bConnect = false; 
                }
                OnPropertyChanged();
                m_reg.Write("Connect", p_bConnect);
            }
        }
        #endregion

        #region BXL Doc
        public string Start()
        {
            if (p_bConnect == false) p_bConnect = true; 
            int nStart = BXLAPI.TransactionStart();
            if (nStart != BXLAPI.BXL_SUCCESS) return "TransactionStart Error : " + nStart.ToString();
            int nInit = BXLAPI.InitializePrinter(); 
            if (nInit != BXLAPI.BXL_SUCCESS) return "InitializePrinter Error : " + nInit.ToString();
            BXLAPI.SetCharacterSet(BXLAPI.BXL_CS_WPC1252);
            BXLAPI.SetInterChrSet(BXLAPI.BXL_ICS_USA);
            return "OK"; 
        }

        public string End()
        {
            BXLAPI.CutPaper();
            //Only BK3-31 with presenter supported
            BXLAPI.PaperEject(BXLAPI.BXL_EJT_HOLD);
            int nEnd = BXLAPI.TransactionEnd(true, 5000 /* 3 seconds */);
            if (nEnd != BXLAPI.BXL_SUCCESS) return "TransactionEnd Error : " + nEnd.ToString();
            return "OK";
        }

        public enum eAlign
        {
            Left,
            Center,
            Right
        }
        int GetAlign(eAlign eAlign)
        {
            switch (eAlign)
            {
                case eAlign.Left: return BXLAPI.BXL_ALIGNMENT_LEFT; 
                case eAlign.Center: return BXLAPI.BXL_ALIGNMENT_CENTER;
                case eAlign.Right: return BXLAPI.BXL_ALIGNMENT_RIGHT; 
            }
            return BXLAPI.BXL_ALIGNMENT_LEFT;
        }

        public enum eAttribute
        {
            Default,
            FontB,
            Bold,
            UnderLine,
            UnderThick,
            Reverse,
            UpsideDown,
            FontC,
            Red
        }
        int GetAttribute(eAttribute eAttribute)
        {
            switch (eAttribute)
            {
                case eAttribute.Default: return BXLAPI.BXL_FT_DEFAULT;
                case eAttribute.FontB: return BXLAPI.BXL_FT_FONTB;
                case eAttribute.Bold: return BXLAPI.BXL_FT_BOLD;
                case eAttribute.UnderLine: return BXLAPI.BXL_FT_UNDERLINE;
                case eAttribute.UnderThick: return BXLAPI.BXL_FT_UNDERTHICK;
                case eAttribute.Reverse: return BXLAPI.BXL_FT_REVERSE;
                case eAttribute.UpsideDown: return BXLAPI.BXL_FT_UPSIDEDOWN;
                case eAttribute.FontC: return BXLAPI.BXL_FT_FONTC;
                case eAttribute.Red: return BXLAPI.BXL_FT_RED_COLOR;
            }
            return BXLAPI.BXL_FT_DEFAULT;
        }

        public void WriteText(string sWrite, eAlign eAlign = eAlign.Left, eAttribute eAttribute = eAttribute.Default)
        {
            BXLAPI.PrintText(sWrite, GetAlign(eAlign), GetAttribute(eAttribute), BXLAPI.BXL_TS_0WIDTH | BXLAPI.BXL_TS_0HEIGHT);
        }

        public void WriteQR(string sWrite, int nSize = 4)
        {
            BXLAPI.PrintQRCode(sWrite, BXLAPI.BXL_QRCODE_MODEL1, nSize, BXLAPI.BXL_QRCODE_ECC_LEVEL_L, BXLAPI.BXL_ALIGNMENT_LEFT);
        }
        #endregion

        #region ITool
        public string p_id { get; set; }

        public UserControl p_ui
        {
            get
            {
                SRP350_UI ui = new SRP350_UI();
                ui.Init(this);
                return ui;
            }
        }
        #endregion

        Log m_log; 
        public SRP350(string id, Log log)
        {
            p_id = id;
            m_log = log;
            m_reg = new Registry(id);
        }

        public void ThreadStop()
        {
            p_bConnect = false; 
        }
    }
}
