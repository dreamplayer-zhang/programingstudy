using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Root_EFEM.Module.FrontsideVision.Centering;

namespace Root_EFEM.Module.FrontsideVision
{
    public class Run_Centering : ModuleRunBase
    {
        Vision_Frontside m_module;
        Camera_Basler m_CamAlign;

        RPoint waferLT = new RPoint();
        RPoint waferRT = new RPoint();
        RPoint waferRB = new RPoint();
        int focusPosZ = 0;
        int edgeSearchRange = 20;
        int edgeSearchLevel = 30;

        GrabModeFront m_grabMode = null;

        string m_sGrabMode;
        public string p_sGrabMode
        {
            get { return m_sGrabMode; }
            set
            {
                m_sGrabMode = value;
                m_grabMode = m_module.GetGrabMode(value);
            }
        }

        public Run_Centering(Vision_Frontside module)
        {
            m_module = module;
            m_CamAlign = m_module.CamAlign;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_Centering run = new Run_Centering(m_module);
            run.waferLT = waferLT;
            run.waferRT = waferRT;
            run.waferRB = waferRB;

            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            waferLT = tree.Set(waferLT, waferLT, "Wafer LT", "Wafer LT", bVisible);
            waferRT = tree.Set(waferRT, waferRT, "Wafer RT", "Wafer RT", bVisible);
            waferRB = tree.Set(waferRB, waferRB, "Wafer RB", "Wafer RB", bVisible);
            focusPosZ = tree.Set(focusPosZ, focusPosZ, "Focus Z", "Focus Z", bVisible);
            p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
            edgeSearchRange = tree.Set(edgeSearchRange, edgeSearchRange, "Search Range", "Search Range", bVisible);
            edgeSearchLevel = tree.Set(edgeSearchLevel, edgeSearchLevel, "Search Level", "Search Level", bVisible);
        }

        public override string Run()
        {
            if (m_CamAlign == null) return "Cam == null";


            StopWatch sw = new StopWatch();
            m_CamAlign.Connect();

            sw.Start();

            while (!m_CamAlign.m_ConnectDone)
            {
                if(sw.ElapsedMilliseconds > 15000)
                {
                    return "VRS Cam Connect Error";
                }
                if (EQ.IsStop()) return "OK";
            }
            sw.Stop();
            
            try
            {
                if (m_grabMode == null)
                {
                    return "Grab Mode == null";
                }

                m_grabMode.SetLight(true);


                Centering centering = new Centering();


                AxisXY axisXY = m_module.AxisXY;
                Axis axisZ = m_module.AxisZ;

                axisZ.StartMove(focusPosZ);
                if (axisZ.WaitReady() == "OK")
                    return "axis Z Move Error";


                if (m_module.Run(axisXY.StartMove(waferLT)))
                    return p_sInfo;
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;


                ImageData img = m_CamAlign.p_ImageData;

                m_CamAlign.GrabOneShot();

                CenteringParam param = new CenteringParam(img, m_CamAlign.GetRoiSize(), edgeSearchRange, edgeSearchLevel, eDir.LT);
                ThreadPool.QueueUserWorkItem(centering.FindEdge, param);


                if (m_module.Run(axisXY.StartMove(waferRT)))
                    return p_sInfo;
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;


                m_CamAlign.GrabOneShot();
                param = new CenteringParam(img, m_CamAlign.GetRoiSize(), edgeSearchRange, edgeSearchLevel, eDir.LT);
                ThreadPool.QueueUserWorkItem(centering.FindEdge, param);


                if (m_module.Run(axisXY.StartMove(waferRB)))
                    return p_sInfo;
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;


                m_CamAlign.GrabOneShot();
                param = new CenteringParam(img, m_CamAlign.GetRoiSize(), edgeSearchRange, edgeSearchLevel, eDir.LT);
                ThreadPool.QueueUserWorkItem(centering.FindEdge, param);






            }
            finally
            {
                m_grabMode.SetLight(false);
            }

            return "OK";
        }
    }

    public class Centering
    {
        public enum eDir
        {
            LT = 0,
            RT,
            RB
        };

        protected eDir _eDir = eDir.LT;
        public eDir p_Dir { get; set; }


        public void FindEdge(object obj)
        {
            ImageData ImgData = ((CenteringParam)obj).img;
            CPoint ptROI = ((CenteringParam)obj).pt;
            eDir dir = ((CenteringParam)obj).dir;
            int nSearchRange = ((CenteringParam)obj).searchRange;
            int nSearchLevel = ((CenteringParam)obj).searchLevel;

            //Mat matSrc = new Mat(new Size(ptROI.X, ptROI.Y), DepthType.Cv8U, 3, ImgData.GetPtr(), (int)ImgData.p_Stride * 3);
            //Mat matTest = new Mat(new Size(ptROI.X, ptROI.Y), DepthType.Cv8U, 3, ImgData.GetPtr(), (int)ImgData.p_Stride * 3);
            //Mat matInsp = new Mat();

        }

        public Point GetEdgePoint(ImageData Image, PointF ptStart, Point vector, int nSearchRange, int nSearchLength, int nSearchLevel)
        {
            Point ptEdge = new Point();
            StopWatch sw = new StopWatch();

            sw.Start();
            double prox = GetProx(Image, ptStart, vector, nSearchRange, nSearchLength, nSearchLevel);
            //if (prox < 10)
            //{
            //    return ptEdge;
            //}
            //sw.Stop();
            //sw.Start();
            //ptEdge = GetEdgePoint(Image, ptStart, vector, prox, nSearchRange, nSearchLength);
            //sw.Stop();
            return ptEdge;
        }

        public double GetProx(ImageData Image, PointF point, PointF vector, int nSearchRange, int nSearchLength, int nSearchLevel)
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

        public void GetMinMax(ImageData Image, PointF point, PointF vector, int nSearchRange, int nSearchLength, out byte min, out byte max)
        {
            byte[] pImg = Image.GetByteArray();
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
                    LineSum += pImg[y * Image.p_Size.X + x];
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

        public void CheckPointRange(ImageData Image, ref int x, ref int y)
        {
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x >= Image.p_Size.X) x = Image.p_Size.X - 1;
            if (y >= Image.p_Size.Y) y = Image.p_Size.Y - 1;
        }

    }
    public class CenteringParam
    {
        public ImageData img;
        public CPoint pt;
        public int searchRange;
        public int searchLevel;
        public eDir dir;

        public CenteringParam(ImageData img, CPoint pt, int searchRange, int searchLevel, eDir dir)
        {
            this.img = img;
            this.pt = pt;
            this.searchRange = searchRange;
            this.searchLevel = searchLevel;
            this.dir = dir;
        }
    }
}
