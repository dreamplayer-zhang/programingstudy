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

        //public void FindEdge(byte[] imgBuf, ePos pos)
        //{
        //    Mat matSrc = new Mat("emgutest.bmp");
        //    CvInvoke.Imshow("test", matSrc);

        //}
        //public void FindEdge(byte[] ImgBuf, ePos pos, CPoint ptROI)
        public void FindEdge(ImageData ImgData, CPoint ptROI, int nSearchRange, int nSearchLength, int nSearchLevel, eDir dir)
        {

            Bitmap bitmap = new Bitmap(@"C:\Users\cgkim\Desktop\ttt\tttt.bmp"); //This is your bitmap


            //byte[] result = null;
            //if (bitmap != null)
            //{
            //    MemoryStream stream = new MemoryStream();
            //    bitmap.Save(stream, bitmap.RawFormat);
            //    result = stream.ToArray();
            //} 
            

            Mat matSrc = new Mat(new Size(ptROI.X, ptROI.Y), DepthType.Cv8U, 3, ImgData.GetPtr(), (int)ImgData.p_Stride);
            Mat matInsp = new Mat(ptROI.X, ptROI.Y, DepthType.Cv8U, 3);
            //Image<Gray, byte> imageGray = new Image<Gray, byte>(bitmap);

            //byte[] pImg = matSrc.GetRawData();

            matSrc.CopyTo(matInsp);
            CvInvoke.MedianBlur(matInsp, matInsp, 5);

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
            if(prox < 10)
            {
                return ptEdge;
            }
          //  ptEdge =  GetEdgePoint(Image, ptStart, vector, )

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
            for(int i = 0; i < nSearchRange; i++)
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

            byte LineSum = 0;
            for (int i = 0; i < nSearchLength; i++)
            {
                for(int j = 0; j < nSearchRange; j++)
                {
                    int x = (int)(point.X + vectorX + 0.5);
                    int y = (int)(point.Y + vectorY + 0.5);
                    CheckPointRange(Image, ref x, ref y);
                    LineSum += pImg[y * Image.Width + x];
                }
                byte LineAvg = (byte)(LineSum / nSearchRange);
                if (LineAvg < min)
                {
                    min = LineAvg;
                }
                if(LineAvg > max)
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
