using DataMatrix.net;
using RootTools.Trees;
using System;
using System.Drawing;

namespace RootTools.RTC5s
{
    public class Design_DataMatrix : DesignBase
    {
        DmtxSymbolSize m_eSymbol = DmtxSymbolSize.DmtxSymbol12x12;
        public string m_sCode = "ATI";
        RPoint m_szDesign = new RPoint(3, 3);
        bool m_bDot = false;
        int m_nDot = 5;
        int m_usPeriod = 10;
        double m_fSize = 90;
        int m_nHatch = 0;
        public override void RunTree(Tree tree)
        {
            m_eSymbol = (DmtxSymbolSize)tree.Set(m_eSymbol, m_eSymbol, "Symbol", "Symbol Type");
            m_sCode = tree.Set(m_sCode, m_sCode, "Code", "DataMatrix Code");
            m_szDesign.Y = tree.Set(m_szDesign.Y, m_szDesign.Y, "Size", "DataMatrix Height (mm)");
            m_bDot = tree.Set(m_bDot, m_bDot, "Dot Marking", "Use Dot Marking");
            m_nDot = tree.Set(m_nDot, m_nDot, "Dot Count", "Dot Marking Count", m_bDot);
            m_usPeriod = tree.Set(m_usPeriod, m_usPeriod, "Period", "Dot Marking Period (us)", m_bDot);
            m_fSize = tree.Set(m_fSize, m_fSize, "Dot Size", "Data Matrix Dot Size (%)", !m_bDot);
            m_nHatch = tree.Set(m_nHatch, m_nHatch, "Dot Hatch", "Dot Hatch Count", !m_bDot);
            base.RunTree(tree);
        }

        public override DesignBase MakeData(string sCode = "")
        {
            if (sCode != "") m_sCode = sCode;
            m_dataList.Clear();
            Bitmap bitmap = MakeBitmap();
            if (bitmap == null) return null;
            CPoint szMatrix = MakeMatrix(bitmap);
            double dl = m_szDesign.Y / szMatrix.Y;
            double y = dl / 2;
            double fSize = dl * m_fSize / 200;
            for (int iy = 0; iy < szMatrix.Y; iy++, y += dl)
            {
                double x = dl / 2;
                for (int ix = 0; ix < szMatrix.X; ix++, x += dl)
                {
                    if (m_aDM[ix, iy] == 0) MakeData(x, y, fSize);
                }
            }
            return base.MakeData(sCode);
        }

        Bitmap MakeBitmap()
        {
            DmtxImageEncoder encoder = new DmtxImageEncoder();
            DmtxImageEncoderOptions options = new DmtxImageEncoderOptions();
            options.ModuleSize = 1;
            options.MarginSize = 0;
            options.BackColor = Color.White;
            options.ForeColor = Color.Black;
            options.SizeIdx = m_eSymbol;
            try
            {
                Bitmap bitmap = encoder.EncodeImage(m_sCode, options);
                return bitmap;
            }
            catch (Exception) { return null; }
        }

        byte[,] m_aDM;
        CPoint MakeMatrix(Bitmap bitmap)
        {
            CPoint szMatrix = new CPoint(bitmap.Width, bitmap.Height);
            m_aDM = new byte[szMatrix.X, szMatrix.Y];
            for (int x = 0; x < szMatrix.X; x++)
            {
                for (int y = 0, iy = szMatrix.Y - 1; y < szMatrix.Y; y++, iy--) m_aDM[x, iy] = bitmap.GetPixel(x, y).R;
            }
            m_szDesign.X = m_szDesign.Y * szMatrix.X / szMatrix.Y;
            return szMatrix;
        }

        void MakeData(double x, double y, double fSize)
        {
            if (m_bDot) m_dataList.AddDot(m_nDot, m_usPeriod, x, y);
            else
            {
                m_dataList.AddMove(x - fSize, y - fSize);
                m_dataList.AddLine(x - fSize, y + fSize);
                m_dataList.AddLine(x + fSize, y + fSize);
                m_dataList.AddLine(x + fSize, y - fSize);
                m_dataList.AddLine(x - fSize, y - fSize);
                double fHatch = 2 * fSize / (m_nHatch + 1);
                double yHatch = y - fSize + fHatch;
                for (int n = 0; n < m_nHatch; n++, yHatch += fHatch)
                {
                    m_dataList.AddMove(x - fSize, yHatch);
                    m_dataList.AddLine(x + fSize, yHatch);
                }
            }
        }

        public Design_DataMatrix()
        {
            m_bEnableHatch = false;
            m_bEnableCode = true;
        }
    }
}
