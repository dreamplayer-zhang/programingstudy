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
        public delegate void FindEdgeDone(object dir);
        public event FindEdgeDone FindEdgeDoneEvent;
        public enum eDir
        {
            LT = 0,
            RT,
            RB
        };

        protected eDir _eDir = eDir.LT;
        public eDir p_Dir { get; set; }

        public List<Point> m_ptLT = new List<Point>();
        public List<Point> m_ptRT = new List<Point>();
        public List<Point> m_ptRB = new List<Point>();
        public RPoint m_ptCenter = new RPoint();
        public RPoint m_ptStageCenter = new RPoint();

        public void CalCenterPoint(CPoint ptROI, double resolutionX, double resolutionY, RPoint ptLTPulse, RPoint ptRTPulse, RPoint ptRBPulse)
        {
            Point ptAvgLT = new Point();
            Point ptAvgRT = new Point();
            Point ptAvgRB = new Point();
            int sumX = 0;
            int sumY = 0;
            for (int i = 0; i < m_ptLT.Count; i++)
            {
                sumX += m_ptLT[i].X;
                sumY += m_ptLT[i].Y;
            }
            ptAvgLT = new Point(sumX / m_ptLT.Count, sumY / m_ptLT.Count);

            sumX = 0;
            sumY = 0;
            for (int i = 0; i < m_ptRT.Count; i++)
            {
                sumX += m_ptRT[i].X;
                sumY += m_ptRT[i].Y;
            }
            ptAvgRT = new Point(sumX / m_ptRT.Count, sumY / m_ptRT.Count);

            sumX = 0;
            sumY = 0;
            for (int i = 0; i < m_ptRB.Count; i++)
            {
                sumX += m_ptRB[i].X;
                sumY += m_ptRB[i].Y;
            }
            ptAvgRB = new Point(sumX / m_ptRB.Count, sumY / m_ptRB.Count);

            double LTX = ptLTPulse.X + ((ptROI.X / 2) - ptAvgLT.X) * resolutionX * 10;
            double LTY = ptLTPulse.Y - ((ptROI.Y / 2) - ptAvgLT.Y) * resolutionX * 10;

            double RTX = ptRTPulse.X + ((ptROI.X / 2) - ptAvgRT.X) * resolutionX * 10;
            double RTY = ptRTPulse.Y - ((ptROI.Y / 2) - ptAvgRT.Y) * resolutionX * 10;

            double RBX = ptRBPulse.X + ((ptROI.X / 2) - ptAvgRB.X) * resolutionX * 10;
            double RBY = ptRBPulse.Y - ((ptROI.Y / 2) - ptAvgRB.Y) * resolutionX * 10;

            PointF ptLT = new PointF((float)LTX, (float)LTY);
            PointF ptRT = new PointF((float)RTX, (float)RTY);
            PointF ptRB = new PointF((float)RBX, (float)RBY);

            Get3PointCircle(ptLT, ptRT, ptRB, out double cx, out double cy, out double r);

            m_ptCenter.X = cx;
            m_ptCenter.Y = cy;


            string sPath = @"C:\Users\ATI\Desktop\CenteringTest\";
            string sLogFileName = "Centering.txt";
            string sTimeLog = DateTime.Now.ToString("HH:mm:ss") + ":" + DateTime.Now.Millisecond.ToString("000") + " : Center X =" + ((int)cx).ToString() + ", Center Y = " + ((int)cy).ToString();
            {
                string FullPath = sPath;
                DirectoryInfo di = new DirectoryInfo(FullPath);
                if (di.Exists == false)
                {
                    di.Create();
                }
                FullPath += sLogFileName;
                StreamWriter sw = new StreamWriter(FullPath, true); // append

                sw.WriteLine(sTimeLog);
                sw.Close();
            }
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

        public bool FindLTEdgeDone = false;
        public bool FindRTEdgeDone = false;
        public bool FindRBEdgeDone = false;
        public string ErrorString = "OK";
        public void FindEdge(object obj)
        {
            ImageData ImgData = ((CenteringParam)obj).img;
            CPoint ptROI = ((CenteringParam)obj).pt;
            eDir dir = ((CenteringParam)obj).dir;
            int nSearchRange = ((CenteringParam)obj).searchRange;
            int nSearchLevel = ((CenteringParam)obj).searchLevel;

            Mat matSrc = new Mat(new Size(ptROI.X, ptROI.Y), DepthType.Cv8U, 3, ImgData.GetPtr(), (int)ImgData.p_Stride * 3);
            Mat matTest = new Mat(new Size(ptROI.X, ptROI.Y), DepthType.Cv8U, 3, ImgData.GetPtr(), (int)ImgData.p_Stride * 3);
            Mat matInsp = new Mat();
            
            StopWatch stopWatch = new StopWatch();

            try
            {
                stopWatch.Start();
                matSrc.CopyTo(matInsp);
                CvInvoke.CvtColor(matInsp, matInsp, ColorConversion.Bgr2Gray);

                CvInvoke.MedianBlur(matInsp, matInsp, 7);

                Point vector = new Point();
                Point edge = new Point();
                var tempList = new List<Point>();
                PointF startPt = new PointF();
                switch (dir)
                {
                    case eDir.LT:
                        vector = new Point(0, 1);
                        tempList = m_ptLT;
                        startPt = new PointF(ptROI.X / 2, 0);
                        break;
                    case eDir.RT:
                        vector = new Point(0, 1);
                        tempList = m_ptRT;
                        startPt = new PointF(ptROI.X / 2, 0);
                        break;
                    case eDir.RB:
                        vector = new Point(0, -1);
                        tempList = m_ptRB;
                        startPt = new PointF(ptROI.X / 2, ptROI.Y);
                        break;
                }
                tempList.Clear();
                int cnt = 0;

                    edge = GetEdgePoint(matInsp, new PointF((float)startPt.X, startPt.Y), vector, nSearchRange, ptROI.Y, nSearchLevel);
                    if (edge.X == 0 && edge.Y == 0)
                    {
                        cnt++;
                    }
                    else
                    {
                        tempList.Add(edge);
                    }

                string test = "";
                if (dir == eDir.LT)
                {
                    test = "LT";
                }
                else if (dir == eDir.RT)
                {
                    test = "RT";
                }
                else if (dir == eDir.RB)
                {

                    test = "RB";
                }
                if (dir == eDir.LT)
                {
                    m_ptLT = tempList;
                    FindLTEdgeDone = true;
                }
                else if (dir == eDir.RT)
                {
                    m_ptRT = tempList;
                    FindRTEdgeDone = true;
                }
                else if (dir == eDir.RB)
                {
                    m_ptRB = tempList;
                    FindRBEdgeDone = true;
                }



                stopWatch.Stop();
                for (int i = 0; i < tempList.Count; i++)
                {
                    CvInvoke.Circle(matTest, tempList[i], 20, new MCvScalar(0, 0, 255),4);
                }
                matTest.Save(@"C:\Users\ATI\Desktop\CenteringTest\Image\" + DateTime.Now.ToString("yyMMddhhmmssff") + test + ".bmp");
            }
            catch(Exception ex)
            {
                ErrorString = "Error";
            }
        }

        public void FindEdgeInit()
        {
            FindLTEdgeDone = false;
            FindRTEdgeDone = false;
            FindRBEdgeDone = false;
            ErrorString = "OK";
            m_ptCenter = new RPoint();
        }

        public void FindEdge(ImageData ImgData, CPoint ptROI, int nSearchRange, int nSearchLength, int nSearchLevel, eDir dir)
        {


            Mat matSrc = new Mat(new Size(ptROI.X, ptROI.Y), DepthType.Cv8U, 3, ImgData.GetPtr(), (int)ImgData.p_Stride);
            Mat matInsp = new Mat();

            matSrc.CopyTo(matInsp);
            CvInvoke.CvtColor(matInsp, matInsp, ColorConversion.Bgr2Gray);

            CvInvoke.MedianBlur(matInsp, matInsp, 7);

            Point vector = new Point();
            var tempList = new List<Point>();
            PointF startPt = new PointF();
            switch (dir)

            {
                case eDir.LT:
                    vector = new Point(0, 1);
                    tempList = m_ptLT;
                    startPt = new PointF(ptROI.X / 2 - (nSearchRange / 2), ptROI.Y / 2 - (nSearchLength / 2));
                    break;
                case eDir.RT:
                    vector = new Point(0, 1);
                    tempList = m_ptRT;
                    startPt = new PointF(ptROI.X / 2 - (nSearchRange / 2), ptROI.Y / 2 - (nSearchLength / 2));
                    break;
                case eDir.RB:
                    vector = new Point(0, -1);
                    tempList = m_ptRB;
                    startPt = new PointF(ptROI.X / 2 - (nSearchRange / 2), ptROI.Y / 2 + (nSearchLength / 2));
                    break;
            }
            tempList.Clear();
            //for (int i = 0; i < nSearchRange; i++)
            //{
                Point edge = GetEdgePoint(matInsp, new PointF(startPt.X , startPt.Y), vector, nSearchRange, nSearchLength, nSearchLevel);
                tempList.Add(edge);
            //}

            
            //for (int i = 0; i < tempList.Count; i++)
            //{
            //    CvInvoke.Circle(matTest, tempList[i], 5, new MCvScalar(0, 255, 0));
            //}
            //Emgu.CV.UI.ImageViewer.Show(matTest);
        }

        public Point GetEdgePoint(Mat Image, PointF ptStart, Point vector, int nSearchRange, int nSearchLength, int nSearchLevel)
        {
            Point ptEdge = new Point();
            StopWatch sw = new StopWatch();

            sw.Start();
            double prox = GetProx(Image, ptStart, vector, nSearchRange, nSearchLength, nSearchLevel);
            if (prox < 10)
            {
                return ptEdge;
            }
            sw.Stop();
            sw.Start();
            ptEdge = GetEdgePoint(Image, ptStart, vector, prox, nSearchRange, nSearchLength);
            sw.Stop();
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

            x = (int)(point.X + 0.5);
            y = (int)(point.Y + 0.5);
            CheckPointRange(Image, ref x, ref y);

            prev = pImg[y * Image.Width + x];

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
            double vectorX = 0;
            double vectorY = 0;

            byte minValue = Byte.MaxValue;
            byte maxValue = Byte.MinValue;
            //Parallel.For(0, nSearchLength, i =>
            for (int i = 0; i < nSearchLength; i++)
            {
                int LineSum = 0;
                for (int j = 0; j < nSearchRange; j++)
                {
                    int x = (int)(point.X - (nSearchRange / 2) + j + 0.5);
                    int y = (int)(point.Y + vectorY + 0.5);
                    CheckPointRange(Image, ref x, ref y);
                    LineSum += pImg[y * Image.Width + x];
                }
                byte LineAvg = (byte)(LineSum / nSearchRange);
                if (LineAvg < minValue)
                {
                    minValue = LineAvg;
                }
                if (LineAvg > maxValue)
                {
                    maxValue = LineAvg;
                }
                vectorX += vector.X;
                vectorY += vector.Y;
            }//);

            min = minValue;
            max = maxValue;

        }

        public void CheckPointRange(Mat Image, ref int x, ref int y)
        {
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x >= Image.Width) x = Image.Width - 1;
            if (y >= Image.Height) y = Image.Height - 1;
        }
    }
    public class CenteringParam
    {
        public ImageData img;
        public CPoint pt;
        public int searchRange;
        public int searchLevel;
        public WaferCentering.eDir dir;

        public CenteringParam(ImageData img, CPoint pt, int searchRange, int searchLevel, WaferCentering.eDir dir)
        {
            this.img = img;
            this.pt = pt;
            this.searchRange = searchRange;
            this.searchLevel = searchLevel;
            this.dir = dir;
        }
    }
}
