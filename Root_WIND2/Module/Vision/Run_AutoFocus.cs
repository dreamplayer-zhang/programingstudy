using RootTools.Camera.BaslerPylon;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using RootTools;
using System.Drawing;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using RootTools.Control;
using System.Threading;
using System.Collections.Concurrent;
using static RootTools.Control.Axis;

namespace Root_WIND2.Module
{
    public class Run_AutoFocus : ModuleRunBase
    {
        Vision m_module;
        Camera_Basler m_CamAutoFocus;

        RPoint m_ptVRSCamPos = new RPoint();

        int m_nStartFocusPosZ = 0;
        //int m_nEndFocusPosZ = 0;
        // 추후 모드 추가

        int m_nAFOffset = 0;
        int m_nRange = 1000;
        double m_dSpeed = 50000;
        double m_dAcc = 0.5f;
        double m_dDcc = 0.5f;

        bool m_bUse2Step = false;
        int m_nRange2 = 1000;
        double m_dSpeed2 = 50000;
        double m_dAcc2 = 0.5f;
        double m_dDcc2 = 0.5f;
        int m_nAFOffset2 = 0;


        public GrabMode m_grabMode = null;
        string m_sGrabMode = "";

        #region AutoFocus Param
        int nFrame = 0;
        List<Mat> m_buff = new List<Mat>();
        List<AF_Result> pos = new List<AF_Result>();
        class AF_Result
        {
            public double pos;
            public double score;
            public AF_Result(double p, double s)
            {
                this.pos = p;
                this.score = s;
            }
        }
        class CalcParameter
        {
            public int matPos;
            public double pos;

            public CalcParameter(int matPos, double pos)
            {
                this.matPos = matPos;
                this.pos = pos;
            }
        }
        #endregion
        public string p_sGrabMode
        {
            get { return m_sGrabMode; }
            set
            {
                m_sGrabMode = value;
                m_grabMode = m_module.GetGrabMode(value);
            }
        }
        public Run_AutoFocus(Vision module)
        {
            m_module = module;
            m_CamAutoFocus = m_module.CamAutoFocus;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_AutoFocus run = new Run_AutoFocus(m_module);
            run.m_nStartFocusPosZ = m_nStartFocusPosZ;
            run.m_nAFOffset = m_nAFOffset;
            run.m_nRange = m_nRange;
            run.m_dSpeed = m_dSpeed;
            run.m_dAcc = m_dAcc;
            run.m_dDcc = m_dDcc;
            run.m_bUse2Step = m_bUse2Step;
            run.m_nAFOffset2 = m_nAFOffset2;
            run.m_nRange2 = m_nRange2;
            run.m_dSpeed2 = m_dSpeed2;
            run.m_dAcc2 = m_dAcc2;
            run.m_dDcc2 = m_dDcc2;
            run.p_sGrabMode = p_sGrabMode;
            run.m_ptVRSCamPos = m_ptVRSCamPos;
            return run;
        }



        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            m_nStartFocusPosZ = tree.Set(m_nStartFocusPosZ, m_nStartFocusPosZ, "Auto Focus Start Pos", "Auto Focus Start Pos", bVisible);
            m_ptVRSCamPos = tree.Set(m_ptVRSCamPos, m_ptVRSCamPos, "VRS Cam Pos", "VRS Cam Pos", bVisible);
            p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);

            m_nRange = tree.Set(m_nRange, m_nRange, "Set Range", "Set Range", bVisible);
            m_dSpeed = tree.Set(m_dSpeed, m_dSpeed, "Set Speed", "Set Speed", bVisible);
            m_dAcc = tree.Set(m_dAcc, m_dAcc, "Set Acc 1 Step", "Set Acc 1 Step", bVisible);
            m_dDcc = tree.Set(m_dDcc, m_dDcc, "Set Dcc 1 Step", "Set Dcc 1 Step", bVisible);
            m_nAFOffset = tree.Set(m_nAFOffset, m_nAFOffset, "Auto Focus Offset", "Auto Focus Offset", bVisible);

            m_bUse2Step = tree.Set(m_bUse2Step, m_bUse2Step, "Use 2 Step", "Use 2 Step", bVisible);
            m_nRange2 = tree.Set(m_nRange2, m_nRange2, "Set Range 2 Step", "Set Range 2 Step", bVisible);
            m_dSpeed2 = tree.Set(m_dSpeed2, m_dSpeed2, "Set Speed 2 Step", "Set Speed 2 Step", bVisible);
            m_dAcc2 = tree.Set(m_dAcc2, m_dAcc2, "Set Acc 2 Step", "Set Acc 2 Step", bVisible);
            m_dDcc2 = tree.Set(m_dDcc2, m_dDcc2, "Set Dcc 2 Step", "Set Dcc 2 Step", bVisible);
            m_nAFOffset2 = tree.Set(m_nAFOffset2, m_nAFOffset2, "Auto Focus Offset 2 Step", "Auto Focus Offset 2 Step", bVisible);


        }

        public override string Run()
        {
            if (m_CamAutoFocus == null) return "Cam == null";

            m_CamAutoFocus.Connect();
            while (!m_CamAutoFocus.m_ConnectDone)
            {
                if (EQ.IsStop()) return "OK";
            }
            m_grabMode.SetLight(true);

            if (m_grabMode == null) return "Grab Mode == null";

            try
            {
                m_grabMode.SetLight(true);
                AxisXY axis = m_module.AxisXY;
                Axis axisZ = m_module.AxisZ;

                //axis.StartMove(new RPoint(4242328, 3575458));
                axis.StartMove(m_ptVRSCamPos);
                axis.WaitReady();

                axisZ.StartMove(m_nStartFocusPosZ - (m_nRange / 2));
                axisZ.WaitReady();

                int movePos = m_nStartFocusPosZ;
                int maxPos = movePos;


                ImageData img = m_CamAutoFocus.p_ImageViewer.p_ImageData;
                StopWatch sw = new StopWatch();

                m_CamAutoFocus.Grabed -= CamAutoFocus_Grabed;
                m_CamAutoFocus.Grabed += CamAutoFocus_Grabed;
                if (m_CamAutoFocus.p_CamInfo._IsGrabbing == false)
                {
                    m_CamAutoFocus.GrabContinuousShot(false);
                }
                else
                {
                    m_CamAutoFocus.StopGrab();
                    m_CamAutoFocus.GrabContinuousShot(false);
                }

                pos.Clear();
                nFrame = 0;

                while (nFrame == 0)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "OK";
                }

                Mat matSrc = new Mat(new Size(640, 480), DepthType.Cv8U, 3, img.GetPtr(), (int)img.p_Stride);
                axisZ.StartMove(m_nStartFocusPosZ + (m_nRange / 2), m_dSpeed, m_dAcc, m_dDcc);
                int nFrameDone = 0;
                sw.Start();
                nFrame = 0;
                while (Math.Abs(axisZ.p_posActual - (m_nStartFocusPosZ + (m_nRange / 2))) > 10)
                {
                    if (EQ.IsStop()) return "OK";

                    if (nFrameDone < nFrame)
                    {

                            double pos = axisZ.p_posActual;


                            m_buff.Add(matSrc.Clone());

                            CalcParameter param = new CalcParameter(nFrameDone, pos);

                            ThreadPool.QueueUserWorkItem(CalcAutoFocus, param);
                            nFrameDone++;
                            Thread.Sleep(1);
                    }
                }

                
                m_CamAutoFocus.StopGrab();
                while (pos.Count != nFrameDone)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "OK";
                }

                axisZ.WaitReady();
                List<AF_Result> orderList = pos.OrderBy(x => x.score).ToList();

                double resPos = orderList[0].pos + m_nAFOffset;
                axisZ.StartMove(resPos);
                axisZ.WaitReady();



                if (m_bUse2Step)
                {
                    pos.Clear();
                    axisZ.StartMove(resPos - (m_nRange2 / 2));
                    axisZ.WaitReady();

                    if (m_CamAutoFocus.p_CamInfo._IsGrabbing == false)
                    {
                        m_CamAutoFocus.GrabContinuousShot(false);
                    }
                    else
                    {
                        m_CamAutoFocus.StopGrab();
                        m_CamAutoFocus.GrabContinuousShot(false);
                    }
                       


                    nFrame = 0;

                    while (nFrame == 0)
                    {
                        Thread.Sleep(10);
                        if (EQ.IsStop()) return "OK";
                    }

                    axisZ.StartMove(resPos + (m_nRange2 / 2), m_dSpeed2, m_dAcc2, m_dDcc2);
                    nFrameDone = 0;
                    sw.Start();
                    nFrame = 0;
                    while (Math.Abs(axisZ.p_posActual - (resPos + (m_nRange2 / 2))) > 10)
                    {
                        if (EQ.IsStop()) return "OK";

                        if (nFrameDone < nFrame)
                        {
                            double pos = axisZ.p_posActual;


                            m_buff.Add(matSrc.Clone());

                            CalcParameter param = new CalcParameter(nFrameDone, pos);

                            ThreadPool.QueueUserWorkItem(CalcAutoFocus, param);
                            nFrameDone++;

                            Thread.Sleep(1);

                        }

                    }
                    m_CamAutoFocus.StopGrab();
                    while (pos.Count != nFrameDone)
                    {
                        Thread.Sleep(10);
                    }
                    axisZ.WaitReady();
                    orderList = pos.OrderBy(x => x.score).ToList();

                    resPos = orderList[0].pos + m_nAFOffset2;
                    axisZ.StartMove(resPos);
                    axisZ.WaitReady();
                }

                m_grabMode.m_dVRSFocusPos = resPos;
                m_module.RunTree(Tree.eMode.RegWrite);
                m_module.RunTree(Tree.eMode.Init);
                sw.Stop();
                //System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);
                return "OK";
            }
            finally
            {
                m_grabMode.SetLight(false);
                //m_grabMode.SetLight(false);
            }

        }

        private void CamAutoFocus_Grabed(object sender, EventArgs e)
        {
            nFrame++;
        }

        private void CalcAutoFocus(object obj)
        {
            Mat img = m_buff[m_buff.Count - 1];

            //StopWatch sw = new StopWatch();
            //sw.Start();
            double curPos = ((CalcParameter)obj).pos;

            VectorOfUMat rgb = new VectorOfUMat();
            VectorOfUMat rgb2 = new VectorOfUMat();

            Mat matLeft = new Mat(img, new Rectangle(10, 0, 310, 480));
            Mat matRight = new Mat(img, new Rectangle(320, 0, 310, 480));

            CvInvoke.Split(matLeft, rgb);
            CvInvoke.Split(matRight, rgb2);
            byte[] bt = rgb[2].Bytes;
            byte[] bt2 = rgb2[2].Bytes;

            int resPix = 0;
            int resPix2 = 0;
            int yPos = 0;
            int yPos2 = 0;


            var part = Partitioner.Create(0, matLeft.Height);
            int leftwidth = matLeft.Width;
            Parallel.ForEach(part, (i, state) =>
            {
                for (int a = i.Item1; a < i.Item2; a++)
                {
                    int pix = -1;
                    int pix2 = -1;
                    for (int j = 0; j < leftwidth; j++)
                    {
                        byte af = bt[(a * leftwidth) + j];
                        byte af2 = bt2[(a * leftwidth) + j];
                        pix += af;
                        pix2 += af2;
                    }
                    if (resPix < pix)
                    {
                        resPix = pix;
                        yPos = a;
                    }
                    if (resPix2 < pix2)
                    {
                        resPix2 = pix2;
                        yPos2 = a;
                    }

                }
            });
            Thread.Sleep(0);

            double res = Math.Abs(yPos - yPos2);

            pos.Add(new AF_Result(curPos, res));

           // sw.Stop();

        }

    }
}
