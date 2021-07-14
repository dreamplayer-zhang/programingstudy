using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace RootTools.Printer
{
    public class SRP350 : NotifyProperty, ITool
    {
        #region Printer
        public string[] m_asPriner =
        {
            "BIXOLON SRP-350II",
            "BIXOLON SRP-350III"
        };

        string _sPrinter = "BIXOLON SRP-350III"; 
        public string p_sPrinter
        {
            get { return _sPrinter; }
            set
            {
                if (_sPrinter == value) return; 
                _sPrinter = value;
                OnPropertyChanged(); 
                m_reg.Write("Printer", p_sPrinter);
            }
        }

        Registry m_reg; 
        void InitPrinter(string id)
        {
            m_reg = new Registry(id);
            p_sPrinter = m_reg.Read("Printer", p_sPrinter);
            p_bConnect = m_reg.Read("Connect", p_bConnect);
        }
        #endregion

        #region BXL API
        static class BXL
        {
            [DllImport("BXLPDC_x64.dll")]
            public static extern bool ConnectPrinterW([MarshalAs(UnmanagedType.LPWStr)] string szPrinterName);

            [DllImport("BXLPDC_x64.dll")]
            public static extern void DisconnectPrinter();

            [DllImport("BXLPDC_x64.dll")]
            public static extern bool Start_DocW([MarshalAs(UnmanagedType.LPWStr)] string szDocName);

            [DllImport("BXLPDC_x64.dll")]
            public static extern bool Start_Page();

            [DllImport("BXLPDC_x64.dll")]
            public static extern void End_Page();

            [DllImport("BXLPDC_x64.dll")]
            public static extern void End_Doc();

            [DllImport("BXLPDC_x64.dll")]
            public static extern int PrintDeviceFontW(int nPositionX, int nPositionY, [MarshalAs(UnmanagedType.LPWStr)] string szFontName, int nFontSize, [MarshalAs(UnmanagedType.LPWStr)] string szData);

            [DllImport("BXLPDC_x64.dll")]
            public static extern int PrintTrueFontW(int nPositionX, int nPositionY, [MarshalAs(UnmanagedType.LPWStr)] string szFontName, int nFontSize, [MarshalAs(UnmanagedType.LPWStr)] string szData, bool bBold, int nRotation, bool bItalic, bool bUnderline);

            [DllImport("BXLPDC_x64.dll")]
            public static extern int PrintBitmap(int nPositionX, int nPositionY, string bitmapFile);

        }
        #endregion

        #region Connect
        bool _bConnect = false;
        public bool p_bConnect
        {
            get { return _bConnect; }
            set
            {
                if (_bConnect == value) return;
                //RootTools/Printer/DLL/BXLPDC_x64.dll -> Copy to Project/bin/...
                if (value) _bConnect = BXL.ConnectPrinterW(p_sPrinter.Trim());
                else BXL.DisconnectPrinter();
                OnPropertyChanged();
                m_reg.Write("Connect", p_bConnect);
            }
        }
        #endregion

        #region Font
        public enum eFontA
        {
            FontA1x1,
            FontA1x2,
            FontA2x1,
            FontA2x2,
            FontA2x4,
            FontA4x2,
            FontA4x4,
            FontA4x8,
            FontA8x4,
            FontA8x8,
        }
        public enum eFontB
        {
            FontB1x1,
            FontB1x2,
            FontB2x1,
            FontB2x2,
            FontB2x4,
            FontB4x2,
            FontB4x4,
            FontB4x8,
            FontB8x4,
            FontB8x8,
        }
        public enum eFontC
        {
            FontC1x1,
            FontC1x2,
            FontC2x1,
            FontC2x2,
            FontC2x4,
            FontC4x2,
            FontC4x4,
            FontC4x8,
            FontC8x4,
            FontC8x8,
        }
        public enum eFontKoean
        {
            Korean1x1,
            Korean1x2,
            Korean2x1,
            Korean2x2,
            Korean2x4,
            Korean4x2,
            Korean4x4,
            Korean4x8,
            Korean8x4,
            Korean8x8,
        }
        public enum eFontChinese
        {
            Chinese2312_1x1,
            Chinese2312_1x2,
            Chinese2312_2x1,
            Chinese2312_2x2,
            Chinese2312_2x4,
            Chinese2312_4x2,
            Chinese2312_4x4,
            Chinese2312_4x8,
            Chinese2312_8x4,
            Chinese2312_8x8,
            ChineseBIG5_1x1,
            ChineseBIG5_1x2,
            ChineseBIG5_2x1,
            ChineseBIG5_2x2,
            ChineseBIG5_2x4,
            ChineseBIG5_4x2,
            ChineseBIG5_4x4,
            ChineseBIG5_4x8,
            ChineseBIG5_8x4,
            ChineseBIG5_8x8,
        }
        public enum eFontJapanese
        {
            Japanese1x1,
            Japanese1x2,
            Japanese2x1,
            Japanese2x2,
            Japanese2x4,
            Japanese4x2,
            Japanese4x4,
            Japanese4x8,
            Japanese8x4,
            Japanese8x8,
        }
        #endregion

        #region BXL Doc
        int m_yDoc = 0; 
        public string Start(string sDoc)
        {
            if (BXL.Start_DocW(sDoc) == false) return "Start Doc Error";
            m_yDoc = 0;
            BXL.Start_Page();
            return "OK"; 
        }

        public void End()
        {
            BXL.End_Page();
            BXL.End_Doc();
        }

        public void Write(int x, int dy, eFontA font, int szFont, string sWrite) { Write(x, dy, font.ToString(), szFont, sWrite); }
        public void Write(int x, int dy, eFontB font, int szFont, string sWrite) { Write(x, dy, font.ToString(), szFont, sWrite); }
        public void Write(int x, int dy, eFontC font, int szFont, string sWrite) { Write(x, dy, font.ToString(), szFont, sWrite); }
        public void Write(int x, int dy, eFontKoean font, int szFont, string sWrite) { Write(x, dy, font.ToString(), szFont, sWrite); }
        public void Write(int x, int dy, eFontChinese font, int szFont, string sWrite) { Write(x, dy, font.ToString(), szFont, sWrite); }
        public void Write(int x, int dy, eFontJapanese font, int szFont, string sWrite) { Write(x, dy, font.ToString(), szFont, sWrite); }
        void Write(int x, int dy, string sFont, int szFont, string sWrite)
        {
            m_yDoc += dy; 
            m_yDoc += BXL.PrintDeviceFontW(x, m_yDoc, sFont, szFont, sWrite);
        }

        public void Write(int x, int dy, string sFont, int szFont, string sWrite, bool bBold, int nRotate, bool bItalic, bool bUnderline)
        {
            m_yDoc += dy;
            m_yDoc += BXL.PrintTrueFontW(x, m_yDoc, sFont, szFont, sWrite, bBold, nRotate, bItalic, bUnderline); 
        }

        public void Write(int x, int dy, string sFileNameBMP)
        {
            m_yDoc += dy;
            m_yDoc += BXL.PrintBitmap(x, m_yDoc, sFileNameBMP);
        }
        #endregion

        #region Cut
        bool _bCutFeeding = true; 
        public bool p_bCutFeeding
        {
            get { return _bCutFeeding; }
            set
            {
                _bCutFeeding = value;
                OnPropertyChanged(); 
            }
        }

        public string Cut(bool bFeeding = true)
        {
            p_bCutFeeding = bFeeding; 
            if (bFeeding) return CutnFeeding();
            return CutNoFeeding(); 
        }

        string CutnFeeding()
        {
            if (Start("Partial Cut") != "OK") return "Partial Cut StartDoc Error";
            Write(0, 0, "FontControl", 9, "P");
            End();
            return "OK"; 
        }

        string CutNoFeeding()
        {
            if (Start("Partial Cut without Feeding") != "OK") return "Partial Cut without Feeding StartDoc Error";
            Write(0, 0, "FontControl", 9, "g");
            End();
            return "OK";
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
            InitPrinter(id); 
        }

        public void ThreadStop()
        {
            p_bConnect = false; 
        }
    }
}
