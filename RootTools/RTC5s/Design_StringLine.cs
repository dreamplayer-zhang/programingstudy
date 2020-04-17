using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.IO;

namespace RootTools.RTC5s
{
    public class Design_StringLine : DesignBase
    {
        #region Line Font
        class LineFont
        {
            public class Data
            {
                public char m_nCmd;
                public char m_x;
                public char m_y;
            }
            public List<Data> m_aData = new List<Data>();
            public void FileOpen(CPoint szFont, string sData, int nData, ref int idx)
            {
                m_aData.Clear();
                for (int n = 0; n < nData; n++)
                {
                    Data data = new Data();
                    data.m_nCmd = Convert.ToChar(sData.Substring(idx++, 1));
                    data.m_x = Convert.ToChar(sData.Substring(idx++, 1));
                    data.m_y = Convert.ToChar(sData.Substring(idx++, 1));
                    m_aData.Add(data);
                }
            }
        }
        List<LineFont> m_aFont = new List<LineFont>();
        void InitFont()
        {
            for (int nCh = 0; nCh < 128; nCh++)
            {
                LineFont lineFont = new LineFont();
                m_aFont.Add(lineFont);
            }
        }
        #endregion

        public string m_sCode = "ATI";
        double m_szDesign = 3;
        double m_fJagan = 0.1;
        public override void RunTree(Tree tree)
        {
            m_sCode = tree.Set(m_sCode, m_sCode, "Code", "String Line Code string");
            m_szDesign = tree.Set(m_szDesign, m_szDesign, "Size", "String Height (mm)");
            p_sFile = tree.SetFile(p_sFile, p_sFile, "lineFont", "Line Font File", "Font File Name");
            m_fJagan = tree.Set(m_fJagan, m_fJagan, "Char Space", "Between Char Space (mm)");
            base.RunTree(tree);
        }

        string _sFile = "";
        string p_sFile
        {
            get { return _sFile; }
            set
            {
                if (_sFile == value) return;
                _sFile = value;
                FontFileOpen();
            }
        }

        int m_nUseLower = 0;
        CPoint m_szFont = new CPoint();
        string FontFileOpen()
        {
            m_szFont.Y = 0;
            try
            {
                StreamReader sr = new StreamReader(new FileStream(p_sFile, FileMode.Open));
                string sData = sr.ReadLine();
                int idx = 0;
                m_nUseLower = GetInt(sData, ref idx);
                m_szFont.X = GetInt(sData, ref idx);
                m_szFont.Y = GetInt(sData, ref idx);
                for (int nCh = 32; nCh < 128; nCh++)
                {
                    int nData = GetInt(sData, ref idx);
                    m_aFont[nCh].FileOpen(m_szFont, sData, nData, ref idx);
                }
                sr.Close();
                return "OK";
            }
            catch (Exception)
            {
                m_szFont.Y = 0;
                return "Error";
            }
        }

        int GetInt(string sData, ref int idx)
        {
            try { return Convert.ToInt32(sData.Substring(idx, 3)); }
            finally { idx += 3; }
        }


        public override DesignBase MakeData(string sCode = "")
        {
            if (sCode != "") m_sCode = sCode;
            m_dataList.Clear();
            if (m_szFont.Y == 0) return null;
            m_fScale = m_szDesign / (m_szFont.Y - 1);
            m_xChar = 0;
            foreach (char ch in m_sCode) MakeData(ch);
            return base.MakeData(sCode);
        }

        double m_fScale;
        double m_xChar = 0;
        void MakeData(char ch)
        {
            LineFont lineFont = m_aFont[ch];
            foreach (LineFont.Data data in lineFont.m_aData)
            {
                double x = m_xChar + data.m_x * m_fScale;
                double y = -data.m_y * m_fScale;
                if (data.m_nCmd == 0) m_dataList.AddMove(x, y);
                else m_dataList.AddLine(x, y);
            }
            m_xChar += m_szDesign * m_szFont.X / m_szFont.Y + m_fJagan;
        }

        public Design_StringLine()
        {
            m_bEnableHatch = false;
            m_bEnableCode = true;
            InitFont();
        }
    }
}
