using QRCoder; 
using RootTools.Trees;
using System;
using System.Drawing;

namespace RootTools.RTC5s
{
    public class Design_QRcode : DesignBase
    {
        public string m_sCode = "ATI";
        double m_szDesign = 10;
        double m_fSize = 90;
        int m_nHatch = 0;
        public override void RunTree(Tree tree)
        {
            m_sCode = tree.Set(m_sCode, m_sCode, "Code", "QR Code string");
            m_szDesign = tree.Set(m_szDesign, m_szDesign, "Size", "DataMatrix Height (mm)");
            m_fSize = tree.Set(m_fSize, m_fSize, "Dot Size", "Data Matrix Dot Size (%)");
            m_nHatch = tree.Set(m_nHatch, m_nHatch, "Dot Hatch", "Dot Hatch Count");
            base.RunTree(tree);
        }

        public override DesignBase MakeData(string sCode = "")
        {
            if (sCode != "") m_sCode = sCode;
            m_dataList.Clear();
            Bitmap bitmap = MakeBitmap();
            if (bitmap == null) return null;
            CPoint szMatrix = MakeMatrix(bitmap);
            double dl = m_szDesign / szMatrix.Y;
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
            try
            {
                QRCodeGenerator generator = new QRCodeGenerator(); 
                QRCodeData codeData = generator.CreateQrCode(m_sCode, QRCodeGenerator.ECCLevel.Q);
                QRCode qr = new QRCode(codeData);
                Bitmap bitmap = qr.GetGraphic(1, Color.Black, Color.White, false);
                bitmap.Save("d:\\QR.bmp");
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
            return szMatrix;
        }

        void MakeData(double x, double y, double fSize)
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

        public Design_QRcode()
        {
            m_bEnableHatch = false;
            m_bEnableCode = true;
        }
    }
}
