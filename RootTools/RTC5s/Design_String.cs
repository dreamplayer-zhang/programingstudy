using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace RootTools.RTC5s
{
    public class Design_String : DesignBase
    {
        #region Font
        List<string> p_asFont
        {
            get
            {
                List<string> asFont = new List<string>();
                foreach (FontFamily fontFamily in FontFamily.Families)
                {
                    asFont.Add(fontFamily.Name);
                }
                return asFont;
            }
        }
        Font m_font = null;
        string m_sFont = "";
        void RunTreeFont(Tree tree)
        {
            m_sFont = tree.Set(m_sFont, m_sFont, p_asFont, "Font", "Font Name");
        }
        #endregion

        public string m_sCode = "ATI";
        double m_hCode = 5;
        public override void RunTree(Tree tree)
        {
            m_sCode = tree.Set(m_sCode, m_sCode, "Code", "String Code");
            m_hCode = tree.Set(m_hCode, m_hCode, "Height", "String Height");
            RunTreeFont(tree);
            base.RunTree(tree);
        }

        double m_fScale = 1;
        DesignOutline m_outLine = new DesignOutline();
        public override DesignBase MakeData(string sCode = "")
        {
            if (sCode != "") m_sCode = sCode;
            m_dataList.Clear();
            CPoint szDC = CalcSize();
            if (szDC.X == 0) return null;
            Bitmap bitmap = MakeImage(szDC);
            m_outLine.MakeOutline(bitmap, m_dataList);
            foreach (DataList.Data data in m_dataList.m_aData)
            {
                data.m_x *= m_fScale;
                data.m_y *= m_fScale;
            }
            return base.MakeData(sCode);
        }

        int c_fMaxSize = 4194304;
        CPoint CalcSize(int hFont = 300)
        {
            Bitmap bitmap = new Bitmap(1, 1);
            Graphics graphics = Graphics.FromImage(bitmap);
            m_font = new Font(m_sFont, hFont);
            m_fScale = m_hCode / hFont;
            if (m_font == null) return new CPoint(0, 0);
            SizeF szF = graphics.MeasureString(m_sCode, m_font);
            double fSize = szF.Width * szF.Height;
            if (fSize <= c_fMaxSize)
            {
                CPoint sz = new CPoint();
                sz.X = (int)Math.Ceiling(szF.Width) + 10;
                sz.Y = (int)Math.Ceiling(szF.Height) + 10;
                return sz;
            }
            else
            {
                double fScale = Math.Sqrt(fSize / c_fMaxSize);
                return CalcSize((int)(hFont / fScale) - 1);
            }
        }

        Bitmap MakeImage(CPoint szDC)
        {
            Bitmap bitmap = new Bitmap(szDC.X, szDC.Y);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.FillRectangle(Brushes.Black, 0, 0, szDC.X, szDC.Y);
            graphics.DrawString(m_sCode, m_font, Brushes.White, 0, 0);
            return bitmap;
        }

        public Design_String()
        {
            m_bEnableHatch = true;
            m_bEnableCode = true;
        }
    }
}
