using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Root_Pine2.Module
{
    public class Transfer : ModuleBase
    {
        public enum ePos
        {
            Magazine0,
            Magazine1,
            Magazine2,
            Magazine3,
            Magazine4,
            Magazine5,
            Magazine6,
            Magazine7,
        }

        #region ToolBox
        public override void GetTools(bool bInit)
        {
            m_loaderPusher.GetTools(m_toolBox, this, bInit);
            m_buffer.GetTools(m_toolBox, this, bInit);
            m_gripper.GetTools(m_toolBox, this, bInit); 
            m_pusher.GetTools(m_toolBox, this, bInit); 
        }
        #endregion

        #region Loader Pusher
        public class LoaderPusher
        {
            #region ToolBox
            Axis[] m_axis = new Axis[2] { null, null };
            DIO_I2O[] m_dioPusher = new DIO_I2O[2] { null, null };
            DIO_I[] m_diOverload = new DIO_I[2] { null, null };
            public void GetTools(ToolBox toolBox, Transfer module, bool bInit)
            {
                toolBox.GetAxis(ref m_axis[0], module, "Loader Pusher 0");
                toolBox.GetAxis(ref m_axis[1], module, "Loader Pusher 1");
                toolBox.GetDIO(ref m_dioPusher[0], module, "Loader Pusher 0", "Backward", "Forward");
                toolBox.GetDIO(ref m_dioPusher[1], module, "Loader Pusher 1", "Backward", "Forward");
                toolBox.GetDIO(ref m_diOverload[0], module, "Loader Pusher 0 Overload");
                toolBox.GetDIO(ref m_diOverload[1], module, "Loader Pusher 1 Overload");
                if (bInit) InitPosition(); 
            }
            #endregion

            #region Axis
            void InitPosition()
            {
                foreach (ePos ePos in Enum.GetValues(typeof(ePos)))
                {
                    m_axis[GetAxisID(ePos)].AddPos(ePos.ToString()); 
                }
            }

            ePos m_ePosLeft = ePos.Magazine3;
            int GetAxisID(ePos ePos)
            {
                return (ePos <= m_ePosLeft) ? 0 : 1; 
            }

            public string RunMove(ePos ePos, bool bWait = true)
            {
                Axis axis = m_axis[GetAxisID(ePos)]; 
                axis.StartMove(ePos);
                return bWait ? axis.WaitReady() : "OK";
            }

            public void Reset()
            {
                m_axis[0].StartMove(0);
                m_axis[1].StartMove(0);
            }
            #endregion

            #region Pusher
            public string RunPusher(ePos ePos)
            {
                string sRun = RunMove(ePos);
                if (sRun != "OK") return sRun;
                DIO_I2O dioPusher = m_dioPusher[GetAxisID(ePos)]; 
                DIO_I diOverload = m_diOverload[GetAxisID(ePos)];
                try
                {
                    dioPusher.Write(true);
                    StopWatch sw = new StopWatch();
                    int msTimeout = (int)(1000 * dioPusher.m_secTimeout);
                    Thread.Sleep(100);
                    while (!dioPusher.p_bDone)
                    {
                        Thread.Sleep(10);
                        if (diOverload.p_bIn) return "Loader Pusher Overload Sensor Error";
                        if (sw.ElapsedMilliseconds > msTimeout) return "Run Loader Pusher Forward Timeout";
                    }
                    Thread.Sleep(100);
                    dioPusher.Write(false);
                    return dioPusher.WaitDone(); 
                }
                finally { dioPusher.Write(false); }
            }
            #endregion
        }
        LoaderPusher m_loaderPusher = new LoaderPusher();
        #endregion

        #region Buffer
        public class Buffer
        {
            #region ToolBox
            Axis m_axis;
            Axis m_axisWidth;
            public void GetTools(ToolBox toolBox, Transfer module, bool bInit)
            {
                toolBox.GetAxis(ref m_axis, module, "Buffer");
                toolBox.GetAxis(ref m_axisWidth, module, "Width");
                if (bInit) InitPosition();
            }

            void InitPosition()
            {
                m_axis.AddPos(Enum.GetNames(typeof(ePos)));
                m_axisWidth.AddPos(Enum.GetNames(typeof(eWidth)));
                
            }
            #endregion

            #region Axis
            double m_dPulse = 0; 
            public string RunMove(ePos ePos, bool bGripPos, bool bWait = true)
            {
                m_axis.StartMove(ePos, bGripPos ? 0 : m_dPulse); 
                return bWait ? m_axis.WaitReady() : "OK";
            }
            #endregion

            #region Width
            public enum eWidth
            {
                mm70,
                mm90,
            }
            public string RunWidth(double fWidth, bool bWait = true)
            {
                double f70 = m_axisWidth.GetPosValue(eWidth.mm70);
                double f90 = m_axisWidth.GetPosValue(eWidth.mm90);
                double dPos = (f90 - f70) * (fWidth - 70) / 10 + f70;
                m_axisWidth.StartMove(dPos);
                return bWait ? m_axisWidth.WaitReady() : "OK";
            }
            #endregion

            public void RunTree(Tree tree)
            {
                m_dPulse = tree.Set(m_dPulse, m_dPulse, "dPulse", "Distance between Buffer (pulse)"); 
            }
        }
        Buffer m_buffer = new Buffer();
        #endregion

        #region Gripper
        public class Gripper
        {
            Axis m_axis;
            DIO_I2O m_dioGripper;
            DIO_Is m_diCheck;
            public void GetTools(ToolBox toolBox, Transfer module, bool bInit)
            {
                toolBox.GetAxis(ref m_axis, module, "Gripper");
                toolBox.GetDIO(ref m_dioGripper, module, "Gripper", "Ungrip", "Grip");
                toolBox.GetDIO(ref m_diCheck, module, "Gripper Check", new string[] { "1", "2" });
                if (bInit) m_axis.AddPos(Enum.GetNames(typeof(eGripper)));
            }

            public enum eGripper
            {
                Ready,
                Ungrip,
                Grip
            }
            public string RunMoveGripper(eGripper eGripper, bool bWait = true)
            {
                m_axis.StartMove(eGripper);
                return bWait ? m_axis.WaitReady() : "OK";
            }

            public string RunGripper()
            {
                if (Run(RunGripper(false))) return m_sRun;
                if (Run(RunMoveGripper(eGripper.Grip))) return m_sRun;
                if (Run(RunGripper(true))) return m_sRun;
                if (Run(RunMoveGripper(eGripper.Ungrip))) return m_sRun;
                if (Run(RunGripper(false))) return m_sRun;
                if (Run(RunMoveGripper(eGripper.Ready))) return m_sRun;
                return "OK";
            }

            string RunGripper(bool bGrip)
            {
                m_dioGripper.Write(bGrip);
                return m_dioGripper.WaitDone();
            }

            string m_sRun = "OK";
            bool Run(string sRun)
            {
                m_sRun = sRun;
                return sRun != "OK";
            }

            public bool IsExist()
            {
                return m_diCheck.ReadDI(0) || m_diCheck.ReadDI(1);
            }

            public InfoStrip m_infoStrip = null; 
            public void Reset()
            {
                if (m_infoStrip == null) return;
                if (IsExist() == false) m_infoStrip = null; 
            }
        }
        Gripper m_gripper = new Gripper(); 
        #endregion

        #region Pusher
        public class Pusher
        {
            DIO_I2O m_dioPusher;
            DIO_I m_diOverload;
            DIO_Is m_diCheck;
            public void GetTools(ToolBox toolBox, Transfer module, bool bInit)
            {
                toolBox.GetDIO(ref m_dioPusher, module, "Pusher", "Backward", "Forward");
                toolBox.GetDIO(ref m_diOverload, module, "Pusher Overload");
                toolBox.GetDIO(ref m_diCheck, module, "Pusher Check", new string[] { "1", "2" });
            }

            public string RunPusher()
            {
                try
                {
                    m_dioPusher.Write(true);
                    StopWatch sw = new StopWatch();
                    int msTimeout = (int)(1000 * m_dioPusher.m_secTimeout);
                    Thread.Sleep(100);
                    while (!m_dioPusher.p_bDone)
                    {
                        Thread.Sleep(10);
                        if (m_diOverload.p_bIn) return "Pusher Overload Sensor Error";
                        if (sw.ElapsedMilliseconds > msTimeout) return "Run Pusher Forward Timeout";
                    }
                    Thread.Sleep(100);
                    m_dioPusher.Write(false);
                    return m_dioPusher.WaitDone();
                }
                finally { m_dioPusher.Write(false); }
            }

            public bool IsExist()
            {
                return m_diCheck.ReadDI(0) || m_diCheck.ReadDI(1); 
            }

            public InfoStrip m_infoStrip = null;
            public void Reset()
            {
                if (m_infoStrip == null) return;
                if (IsExist() == false) m_infoStrip = null;
            }

        }
        Pusher m_pusher = new Pusher();
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }

        public override void Reset()
        {
            m_loaderPusher.Reset();
            m_buffer.RunWidth(m_pine.p_widthStrip);
            m_gripper.Reset();
            m_pusher.Reset(); 
            base.Reset();
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            m_buffer.RunTree(tree.GetTree("Buffer"));
        }
        #endregion

        Pine2 m_pine; 
        public Transfer(string id, IEngineer engineer, Pine2 pine)
        {
            m_pine = pine; 
            base.InitBase(id, engineer); 
        }

        public override void ThreadStop()
        {
            Reset(); 
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {

        }
        #endregion
    }
}
