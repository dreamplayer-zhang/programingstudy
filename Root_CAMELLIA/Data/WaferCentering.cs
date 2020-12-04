using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using RootTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA.Data
{
    public class WaferCentering
    {
        public enum eDir
        {
            LT = 0,
            RT,
            RB
        };

        protected eDir _eDir = eDir.LT;
        public eDir p_Dir { get; set; }

        public Point m_ptLT = new Point();
        public Point m_ptRT = new Point();
        public Point m_ptRB = new Point();
        public RPoint m_ptCenter = new RPoint();
        //public void FindEdge(byte[] imgBuf, ePos pos)
        //{
        //    Mat matSrc = new Mat("emgutest.bmp");
        //    CvInvoke.Imshow("test", matSrc);

        //}
        //public void FindEdge(byte[] ImgBuf, ePos pos, CPoint ptROI)
        public void CalCenterPoint(CPoint ptROI, double resolutionX, double resolutionY, RPoint ptLTPulse, RPoint ptRTPulse, RPoint ptRBPulse)
        {
            double LTX = ptLTPulse.X - (((ptROI.X / 2) + m_ptLT.X) * resolutionX * 10);
            double LTY = ptLTPulse.Y + (((ptROI.Y / 2) - m_ptLT.Y) * resolutionY * 10);

            double RTX = ptRTPulse.X - (((ptROI.X / 2) + m_ptRT.X) * resolutionX * 10);
            double RTY = ptRTPulse.Y + (((ptROI.Y / 2) - m_ptRT.Y) * resolutionY * 10);

            double RBX = ptRBPulse.X - (((ptROI.X / 2) + m_ptRB.X) * resolutionX * 10);
            double RBY = ptRBPulse.Y + (((ptROI.Y / 2) - m_ptRB.Y) * resolutionY * 10);

            PointF ptLT = new PointF((float)LTX, (float)LTY);
            PointF ptRT = new PointF((float)RTX, (float)RTY);
            PointF ptRB = new PointF((float)RBX, (float)RBY);

            Get3PointCircle(ptLT, ptRT, ptRB, out double cx, out double cy, out double r);

            m_ptCenter.X = cx;
            m_ptCenter.Y = cy;
        }

        void Get3PointCircle(PointF p0, PointF p1, PointF p2, out double cx, out double cy, out double r)
        {
            double dy1, dy2, d, d2, yi;
            PointF P1 = new PointF((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2);
            PointF P2 = new PointF((p0.X + p2.X) / 2, (p0.Y + p2.Y) / 2);
            dy1 = p0.Y - p1.Y;
            dy2 = p0.Y - p2.Y;
            cx = 0;
            cy = 0;
            r = 0;
            if (dy1 != 0)
            {
                d = (p1.X - p0.X) / dy1;
                yi = P1.Y - d * P1.X;
                if (dy2 != 0)
                {
                    d2 = (p2.X - p0.X) / dy2;
                    if (d != d2) cx = (yi - (P2.Y - d2 * P2.X)) / (d2 - d);
                    else return;
                }
                else if (p2.X - p0.X == 0) return;
                else cx = P2.X;
            }
            else if (dy2 != 0 && p1.X - p0.X != 0)
            {
                d = (p2.X - p0.X) / dy2;
                yi = P2.Y - d * P2.X;
                cx = P1.X;
            }
            else return;
            cy = d * cx + yi;
            r = Math.Sqrt((p0.X - cx) * (p0.X - cx) + (p0.Y - cy) * (p0.Y - cy));
        }

        public void FindEdge(ImageData ImgData, CPoint ptROI, int nSearchRange, int nSearchLength, int nSearchLevel, eDir dir)
        {

            //            Bitmap bitmap = new Bitmap(@"C:\Users\cgkim\Desktop\ttt\tttt.bmp"); //This is your bitmap


            //byte[] result = null;
            //if (bitmap != null)
            //{
            //    MemoryStream stream = new MemoryStream();
            //    bitmap.Save(stream, bitmap.RawFormat);
            //    result = stream.ToArray();
            //} 

            Mat matSrc = new Mat();
             Mat matTest = new Mat(new Size(ptROI.X, ptROI.Y), DepthType.Cv8U, 3, ImgData.GetPtr(), (int)ImgData.p_Stride);
            Image<Gray, byte> imageGray = new Image<Gray, byte>(ptROI.X, ptROI.Y, (int)ImgData.p_Stride, ImgData.GetPtr());
            Mat matInsp = new Mat(ptROI.X, ptROI.Y, DepthType.Cv8U, 3);
            //Image<Gray, byte> imageGray = new Image<Gray, byte>(bitmap);

            //byte[] pImg = matSrc.GetRawData();
            matSrc = imageGray.Mat;

            matSrc.CopyTo(matInsp);
            CvInvoke.MedianBlur(matInsp, matInsp, 7);


            //Emgu.CV.UI.ImageViewer.Show(matSrc);
            //Emgu.CV.UI.ImageViewer.Show(matInsp);
            Point vector = new Point(0, 1);
            Point edge = new Point();

            switch (dir)
            {
                case eDir.LT:
                    vector = new Point(0, 1);
                    edge = GetEdgePoint(matInsp, new PointF(ptROI.X / 2, 0), vector, nSearchRange, ptROI.Y, nSearchLevel);
                    m_ptLT = edge;
                    break;
                case eDir.RT:
                    vector = new Point(0, 1);
                    edge = GetEdgePoint(matInsp, new PointF(ptROI.X / 2, 0), vector, nSearchRange, ptROI.Y, nSearchLevel);
                    m_ptRT = edge;
                    break;
                case eDir.RB:
                    vector = new Point(0, -1);
                    edge = GetEdgePoint(matInsp, new PointF(ptROI.X / 2, ptROI.Y), vector, nSearchRange, ptROI.Y, nSearchLevel);
                    m_ptRB = edge;
                    break;
            }





            CvInvoke.Circle(matTest, edge, 10, new MCvScalar(0,255,0));
            Emgu.CV.UI.ImageViewer.Show(matTest);
            //for(int i = 0; i < nSearchLength; i++)
            //{
            //    switch (dir)
            //    {
            //        case eDir.LT:

            //            break;
            //        case eDir.RT:
            //            break;
            //        case eDir.RB:
            //            break;
            //    }
            //}


            // Emgu.CV.UI.ImageViewer.Show(matInsp, "ttt");

            //Image<Gray, byte> imageGray = new Image<Gray, byte>(,);






            //Image<Gray, byte> imageGray = new Image<Gray, byte>(ptROI.X, ptROI.Y);
            //imageGray.Bytes = 
            //Image<Gray, byte> imageGray = new Image<Gray, byte>(bitmap);

            //Mat matSrc = imageGray.Mat;
            //Emgu.CV.UI.ImageViewer.Show(matSrc);

            //VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            ////imageGray = imageGray.SmoothMedian(3);
            ////matSrc = imageGray.Mat;
            ////Emgu.CV.UI.ImageViewer.Show(matSrc, "ttt");
            //imageGray = imageGray.ThresholdAdaptive(new Gray(255), AdaptiveThresholdType.GaussianC, ThresholdType.Otsu, 11, new Gray(0.5));

            //matSrc = imageGray.Mat;
            //Emgu.CV.UI.ImageViewer.Show(matSrc, "ttt");
            //CvInvoke.FindContours(imageGray, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

            //for (int i = 0; i < contours.Size; i++)
            //{

            //    CvInvoke.DrawContours(imageGray, contours, i, new MCvScalar(255, i, 0));

            //}
            //VectorOfPoint contour = contours[0];
            //for(int i = 0; i < contour.Size; i++)
            //{
            //    Console.WriteLine(contour[i]);
            //}

            ////Mat matdst = colorImage.Mat;

            //Emgu.CV.UI.ImageViewer.Show(matSrc, "ttt");

            //switch(p_ePos)
            //{
            //    case ePos.LT :
            //        m_ptLT = contour[contour.Size / 2];
            //        break;
            //    case ePos.RT :
            //        m_ptRT = contour[contour.Size / 2];
            //        break;
            //    case ePos.RB :
            //        m_ptRB = contour[contour.Size / 2];
            //        break;
            //}

            //for (Contour<Point> contours = grayImage.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST, storage); contours != null; contours = contours.HNext)
            //{
            //    Contour<Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.015, storage);
            //    if (currentContour.BoundingRectangle.Width > 20)
            //    {
            //        CvInvoke.cvDrawContours(color, contours, new MCvScalar(255), new MCvScalar(255), -1, 1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, new Point(0, 0));
            //        color.Draw(currentContour.BoundingRectangle, new Bgr(0, 255, 0), 1);
            //    }

            //    Point[] pts = currentContour.ToArray();
            //    foreach (Point p in pts)
            //    {
            //        //add points to listbox
            //        listBox1.Items.Add(p);
            //    }
            //}

        }

        public Point GetEdgePoint(Mat Image, PointF ptStart, Point vector, int nSearchRange, int nSearchLength, int nSearchLevel)
        {
            Point ptEdge = new Point();
            double prox = GetProx(Image, ptStart, vector, nSearchRange, nSearchLength, nSearchLevel);
            if (prox < 10)
            {
                return ptEdge;
            }
            ptEdge = GetEdgePoint(Image, ptStart, vector, prox, nSearchRange, nSearchLength);

            return ptEdge;
        }
        unsafe private Point GetEdgePoint(Mat Image, PointF point, PointF vector, double prox, int nSearchRange, int nSearchLength)
        {
            byte[] pImg = Image.GetRawData();

            Point result = new Point((int)point.X, (int)point.Y);


            // 찾는 방향 설정 (In to Out / Out to In)
            SetDirection(vector, out double positionX, out double positionY, out double vectorX, out double vectorY);

            byte prev = 0;
            byte current = 0;

            int x = 0;
            int y = 0;
            for (int i = 0; i < nSearchRange; i++)
            {
                x = (int)(point.X + positionX + 0.5);
                y = (int)(point.Y + positionY + 0.5);

                CheckPointRange(Image, ref x, ref y);
                prev += pImg[y * Image.Width + x];

                positionX += vectorX;
                positionY += vectorY;
            }


            prev /= (byte)nSearchRange;

            SetDirection(vector, out positionX, out positionY, out vectorX, out vectorY);

            for (int i = 0; i < nSearchLength; i++)
            {
                x = (int)(point.X + positionX + 0.5);
                y = (int)(point.Y + positionY + 0.5);

                CheckPointRange(Image, ref x, ref y);

                current = pImg[y * Image.Width + x];
                if ((current >= prox && prox > prev) ||
                   (current <= prox && prox < prev))
                {
                    result.X = x;
                    result.Y = y;
                    break;
                }

                prev = current;

                positionX += vectorX;
                positionY += vectorY;
            }

            return result;
        }

        private void SetDirection(PointF vector, out double positionX, out double positionY, out double vectorX, out double vectorY)
        {
            positionX = 0;
            positionY = 0;
            vectorX = vector.X;
            vectorY = vector.Y;
        }

        public double GetProx(Mat Image, PointF point, PointF vector, int nSearchRange, int nSearchLength, int nSearchLevel)
        {
            double dProx = 0;
            byte min, max;
            GetMinMax(Image, point, vector, nSearchRange, nSearchLength, out min, out max);

            if (max - min < 50)
            {
                dProx = 0;
            }
            else
            {
                dProx = (double)(min + (max - min) * nSearchLevel * 0.01);
            }
            return dProx;
        }

        public void GetMinMax(Mat Image, PointF point, PointF vector, int nSearchRange, int nSearchLength, out byte min, out byte max)
        {
            byte[] pImg = Image.GetRawData();

            min = Byte.MaxValue;
            max = Byte.MinValue;

            double vectorX = 0;
            double vectorY = 0;


            for (int i = 0; i < nSearchLength; i++)
            {
                int LineSum = 0;
                for (int j = 0; j < nSearchRange; j++)
                {
                    int x = (int)(point.X + j + 0.5);
                    int y = (int)(point.Y + vectorY + 0.5);
                    CheckPointRange(Image, ref x, ref y);
                    LineSum += pImg[y * Image.Width + x];
                }
                byte LineAvg = (byte)(LineSum / nSearchRange);
                if (LineAvg < min)
                {
                    min = LineAvg;
                }
                if (LineAvg > max)
                {
                    max = LineAvg;
                }
                vectorX += vector.X;
                vectorY += vector.Y;
            }

            //double vectorX = 0;
            //double vectorY = 0;
            //for(int i = 0; i < nSearchRange; i++)
            //{
            //    int x = (int)(point.X + vectorX + 0.5);
            //    int y = (int)(point.Y + vectorY + 0.5);
            //    CheckPointRange(Image, ref x, ref y);

            //    byte pix = pImg[y * Image.Width + x];
            //    if (pix < min)
            //        min = pix;
            //    if (pix > max)
            //        max = pix;

            //    vectorX += vector.X;
            //    vectorY += vector.Y;
            //}
        }

        public void CheckPointRange(Mat Image, ref int x, ref int y)
        {
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x >= Image.Width) x = Image.Width - 1;
            if (y >= Image.Height) y = Image.Height - 1;
        }
    }
}
