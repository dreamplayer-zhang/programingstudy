using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.IO;

namespace RootTools.RTC5s
{
    public class Design_StringDot : DesignBase
    {
        #region Dot Font
        class DotFont
        {
            public List<CPoint> m_aDot = new List<CPoint>();
            public void FileOpen(CPoint szFont, string sData, ref int idx)
            {
                m_aDot.Clear();
                for (int y = 0; y < szFont.Y; y++)
                {
                    for (int x = 0; x < szFont.X; x++)
                    {
                        if (Convert.ToChar(sData.Substring(idx, 1)) == '1') m_aDot.Add(new CPoint(x, y));
                        idx++;
                    }
                }
            }
        }
        List<DotFont> m_aFont = new List<DotFont>();
        void InitFont()
        {
            for (int nCh = 0; nCh < 128; nCh++)
            {
                DotFont dotFont = new DotFont();
                m_aFont.Add(dotFont);
            }
        }
        #endregion

        public string m_sCode = "ATI";
        double m_szDesign = 3;
        double m_fJagan = 0.1;
        bool m_bDot = true;
        int m_nDot = 5;
        int m_usPeriod = 10;
        public override void RunTree(Tree tree)
        {
            m_sCode = tree.Set(m_sCode, m_sCode, "Code", "String Dot Code string");
            m_szDesign = tree.Set(m_szDesign, m_szDesign, "Size", "String Height (mm)");
            p_sFile = tree.SetFile(p_sFile, p_sFile, "dotFont", "Dot Font File", "Font File Name");
            m_fJagan = tree.Set(m_fJagan, m_fJagan, "Char Space", "Between Char Space (mm)");
            m_nDot = tree.Set(m_nDot, m_nDot, "Dot Count", "Dot Marking Count", m_bDot);
            m_usPeriod = tree.Set(m_usPeriod, m_usPeriod, "Period", "Dot Marking Period (us)", m_bDot);
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
                for (int nCh = 32; nCh < 128; nCh++) m_aFont[nCh].FileOpen(m_szFont, sData, ref idx);
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
            m_dDot = m_szDesign / (m_szFont.Y - 1);
            m_xChar = 0;
            foreach (char ch in m_sCode) MakeData(ch);
            return base.MakeData(sCode);
        }

        double m_dDot;
        double m_xChar = 0;
        void MakeData(char ch)
        {
            DotFont dotFont = m_aFont[ch];
            foreach (CPoint cp in dotFont.m_aDot)
            {
                double x = m_xChar + cp.X * m_dDot;
                double y = -cp.Y * m_dDot;
                m_dataList.AddDot(m_nDot, m_usPeriod, x, y);
            }
            m_xChar += m_szDesign * m_szFont.X / m_szFont.Y + m_fJagan;
        }

        public Design_StringDot()
        {
            m_bEnableHatch = false;
            m_bEnableCode = true;
            InitFont();
        }
    }
}
