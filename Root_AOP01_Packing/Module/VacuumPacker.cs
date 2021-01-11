using Root_EFEM;
using Root_EFEM.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_AOP01_Packing.Module
{
    public class VacuumPacker : ModuleBase, IWTRChild
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            m_wrapper.GetTools(m_toolBox, bInit); 
            m_stage.GetTools(m_toolBox, bInit);
            m_transfer.GetTools(m_toolBox, bInit);
            m_holder.GetTools(m_toolBox, bInit); 
            m_loader.GetTools(m_toolBox, bInit);
            m_heater.GetTools(m_toolBox, bInit);
        }
        #endregion

        #region Solvalue
        public List<DIO_I2O2> m_aSolvalve = new List<DIO_I2O2>();
        public void InitSolvalve(DIO_I2O2 sol)
        {
            if (sol == null) return;
            m_aSolvalve.Add(sol);
            sol.Write(false);
        }

        public List<string> p_asSol
        {
            get
            {
                List<string> asSol = new List<string>();
                foreach (DIO_I2O2 sol in m_aSolvalve) asSol.Add(sol.m_id);
                return asSol;
            }
        }

        public DIO_I2O2 GetSolvalve(string sSol)
        {
            foreach (DIO_I2O2 sol in m_aSolvalve)
            {
                if (sol.m_id == sSol) return sol;
            }
            return null;
        }
        #endregion

        #region Wrapper
        public class Wrapper
        {
            Axis m_axisMove;
            Axis m_axisPicker;
            DIO_IO[] m_dioVacuum = new DIO_IO[2];
            DIO_O[] m_doBlow = new DIO_O[2];
            DIO_I2O2 m_solPush;
            DIO_I m_diCheck;
            DIO_I m_diLevel;
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_packer.p_sInfo = toolBox.Get(ref m_axisMove, m_packer, m_id + ".Move");
                m_packer.p_sInfo = toolBox.Get(ref m_axisPicker, m_packer, m_id + ".Picker");
                m_packer.p_sInfo = toolBox.Get(ref m_dioVacuum[0], m_packer, m_id + ".Vacuum Center");
                m_packer.p_sInfo = toolBox.Get(ref m_dioVacuum[1], m_packer, m_id + ".Vacuum Side");
                m_packer.p_sInfo = toolBox.Get(ref m_doBlow[0], m_packer, m_id + ".Blow Center");
                m_packer.p_sInfo = toolBox.Get(ref m_doBlow[1], m_packer, m_id + ".Blow Side");
                m_packer.p_sInfo = toolBox.Get(ref m_solPush, m_packer, m_id + ".Push", "Back", "Push");
                m_packer.p_sInfo = toolBox.Get(ref m_diCheck, m_packer, m_id + ".Wrapper Check");
                m_packer.p_sInfo = toolBox.Get(ref m_diLevel, m_packer, m_id + ".Wrapper Level");
                if (bInit)
                {
                    m_packer.InitSolvalve(m_solPush);
                    InitPos();
                }
            }

            public enum ePosMove
            {
                Pick,
                Place,
                Push
            }
            public enum ePosPicker
            {
                Up,
                Down,
                Open,
                Push
            }
            void InitPos()
            {
                m_dioVacuum[0].Write(false);
                m_dioVacuum[1].Write(false);
                m_axisMove.AddPos(Enum.GetNames(typeof(ePosMove)));
                m_axisPicker.AddPos(Enum.GetNames(typeof(ePosPicker)));
            }

            public string RunMove(ePosMove ePos)
            {
                m_axisMove.StartMove(ePos);
                return m_axisMove.WaitReady();
            }

            public string RunMove(ePosPicker ePos)
            {
                m_axisPicker.StartMove(ePos);
                return m_axisPicker.WaitReady();
            }

            double m_secVac = 0.5;
            double m_secBlow = 0.5;
            public string RunVacOn()
            {
                m_dioVacuum[0].Write(true);
                m_dioVacuum[1].Write(true);
                int msVac = (int)(1000 * m_secVac);
                while (m_dioVacuum[0].m_swWrite.ElapsedMilliseconds < msVac)
                {
                    Thread.Sleep(10);
                    if (m_dioVacuum[0].p_bIn && m_dioVacuum[1].p_bIn) return "OK";
                    if (EQ.IsStop()) return m_id + " EQ Stop";
                }
                return "Vacuum Sensor On Timeout";
            }

            public string RunVacOff(bool bCenter)
            {
                int nID = bCenter ? 0 : 1;
                m_dioVacuum[nID].Write(false);
                if (m_dioVacuum[nID].p_bIn)
                {
                    m_doBlow[nID].Write(true);
                    Thread.Sleep((int)(1000 * m_secBlow));
                    m_doBlow[nID].Write(false);
                }
                return "OK";
            }

            public string RunPush()
            {
                try
                {
                    string sRun = m_solPush.RunSol(true);
                    if (sRun != "OK") return sRun;
                    return m_solPush.RunSol(false);
                }
                finally { m_solPush.Write(false); }
            }

            public bool IsWapperExist()
            {
                return (m_diCheck.p_bIn && m_diLevel.p_bIn); 
            }

            public void RunTree(Tree tree)
            {
                m_secVac = tree.Set(m_secVac, m_secVac, "Vacuum", "Vacuum On Wait (sec)");
                m_secBlow = tree.Set(m_secBlow, m_secBlow, "Blow", "Vacuum Off Blow Time (sec)");
            }

            string m_id;
            VacuumPacker m_packer;
            public Wrapper(string id, VacuumPacker packer)
            {
                m_id = id;
                m_packer = packer;
            }
        }
        public Wrapper m_wrapper;
        #endregion

        #region Stage
        public class Stage
        {
            DIO_O[] m_doVacuum = new DIO_O[2];
            DIO_O[] m_doBlow = new DIO_O[2];
            DIO_I2O2 m_solUp;
            DIO_I2O2 m_solRotate;
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_packer.p_sInfo = toolBox.Get(ref m_doVacuum[0], m_packer, m_id + ".Vacuum Center");
                m_packer.p_sInfo = toolBox.Get(ref m_doVacuum[1], m_packer, m_id + ".Vacuum Side");
                m_packer.p_sInfo = toolBox.Get(ref m_doBlow[0], m_packer, m_id + ".Blow Center");
                m_packer.p_sInfo = toolBox.Get(ref m_doBlow[1], m_packer, m_id + ".Blow Side");
                m_packer.p_sInfo = toolBox.Get(ref m_solUp, m_packer, m_id + ".Up", "Down", "Up");
                m_packer.p_sInfo = toolBox.Get(ref m_solRotate, m_packer, m_id + ".Rotate", "Home", "90");
                if (bInit)
                {
                    m_packer.InitSolvalve(m_solUp);
                    m_packer.InitSolvalve(m_solRotate);
                }
            }

            double m_secVac = 1;
            public string RunVac(bool bCenter, bool bSide)
            {
                m_doVacuum[0].Write(bCenter);
                m_doVacuum[1].Write(bSide);
                m_doBlow[0].Write(!bCenter);
                m_doBlow[1].Write(!bSide);
                Thread.Sleep((int)(1000 * m_secVac));
                m_doBlow[0].Write(false);
                m_doBlow[1].Write(false);
                return "OK";
            }

            public string RunUp(bool bUp)
            {
                return m_solUp.RunSol(bUp); 
            }

            public string RunRotate(bool bRotate)
            {
                if (m_solUp.p_bOut == false) return "State Down";
                return m_solRotate.RunSol(bRotate); 
            }

            public void RunTree(Tree tree)
            {
                m_secVac = tree.Set(m_secVac, m_secVac, "Vacuum", "Vacuum On & Off Delay (sec)");
            }

            string m_id;
            VacuumPacker m_packer;
            public Stage(string id, VacuumPacker packer)
            {
                m_id = id;
                m_packer = packer;
            }
        }

        public Stage m_stage;
        #endregion

        #region Transfer
        public class Transfer
        {
            Axis m_axis;
            DIO_I2O2 m_solDown;
            DIO_I2O2 m_solPush;
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_packer.p_sInfo = toolBox.Get(ref m_axis, m_packer, m_id);
                m_packer.p_sInfo = toolBox.Get(ref m_solDown, m_packer, m_id + ".Down", "Up", "Down");
                m_packer.p_sInfo = toolBox.Get(ref m_solPush, m_packer, m_id + ".Push", "Back", "Push");
                if (bInit)
                {
                    m_packer.InitSolvalve(m_solDown);
                    m_packer.InitSolvalve(m_solPush);
                    InitPos(); 
                }
            }

            public enum ePos
            {
                Ready,
                Push
            }
            void InitPos()
            {
                m_axis.AddPos(Enum.GetNames(typeof(ePos)));
            }

            public string RunMove(ePos ePos)
            {
                m_axis.StartMove(ePos);
                return m_axis.WaitReady();
            }

            public string RunDown(bool bDown)
            {
                return m_solDown.RunSol(bDown);
            }

            public string RunPush()
            {
                try
                {
                    string sRun = m_solPush.RunSol(true);
                    if (sRun != "OK") return sRun;
                    return m_solPush.RunSol(false);
                }
                finally { m_solPush.Write(false); }
            }

            string m_id;
            VacuumPacker m_packer;
            public Transfer(string id, VacuumPacker packer)
            {
                m_id = id;
                m_packer = packer;
            }
        }
        public Transfer m_transfer;
        #endregion

        #region Holder
        public class Holder
        {
            DIO_IO[] m_dioVacuum = new DIO_IO[2];
            DIO_O[] m_doBlow = new DIO_O[2];
            DIO_I2O2[] m_solHold = new DIO_I2O2[2];
            DIO_I2O2 m_solDown;
            DIO_I2O2 m_solForward;
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_packer.p_sInfo = toolBox.Get(ref m_dioVacuum[0], m_packer, m_id + ".UpHolder Vacuum");
                m_packer.p_sInfo = toolBox.Get(ref m_dioVacuum[1], m_packer, m_id + ".DownHolder Vacuum");
                m_packer.p_sInfo = toolBox.Get(ref m_doBlow[0], m_packer, m_id + ".UpHolder Blow");
                m_packer.p_sInfo = toolBox.Get(ref m_doBlow[1], m_packer, m_id + ".DownHolder Blow");
                m_packer.p_sInfo = toolBox.Get(ref m_solHold[0], m_packer, m_id + ".UpHolder", "Open", "Hold");
                m_packer.p_sInfo = toolBox.Get(ref m_solHold[1], m_packer, m_id + ".DownHolder", "Open", "Hold");
                m_packer.p_sInfo = toolBox.Get(ref m_solDown, m_packer, m_id + ".Down", "Up", "Down");
                m_packer.p_sInfo = toolBox.Get(ref m_solForward, m_packer, m_id + ".Forward", "Backward", "Forward");
                if (bInit)
                {
                    m_packer.InitSolvalve(m_solDown);
                    m_packer.InitSolvalve(m_solForward);
                    m_packer.InitSolvalve(m_solHold[0]);
                    m_packer.InitSolvalve(m_solHold[1]);
                }
            }

            double m_secVac = 1;
            double m_secBlow = 1;
            string RunVacuum(bool bOn)
            {
                m_dioVacuum[0].Write(bOn);
                m_dioVacuum[1].Write(bOn); 
                if (bOn)
                {
                    int msTimeout = (int)(1000 * m_secVac);
                    StopWatch sw = new StopWatch(); 
                    while (sw.ElapsedMilliseconds < msTimeout)
                    {
                        Thread.Sleep(10);
                        if (m_dioVacuum[0].p_bIn && m_dioVacuum[1].p_bIn) return "OK"; 
                    }
                    return "Vacuum On timeout"; 
                }
                else
                {
                    m_doBlow[0].Write(true);
                    m_doBlow[1].Write(true);
                    Thread.Sleep((int)(1000 * m_secBlow));
                    m_doBlow[0].Write(false);
                    m_doBlow[1].Write(false);
                }
                return "OK";
            }
            
            public string RunHold(bool bHold)
            {
                if (bHold == false) RunVacuum(false); 
                m_solHold[0].Write(bHold);
                m_solHold[1].Write(bHold);
                int msTimeout = (int)(1000 * m_solHold[0].m_secTimeout);
                StopWatch sw = new StopWatch(); 
                while (sw.ElapsedMilliseconds < msTimeout)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop"; 
                    if (m_solHold[0].p_bDone && m_solHold[1].p_bDone) return RunVacuum(true);
                }
                return "Hold Timeout"; 
            }

            public string RunForward(bool bForward)
            {
                if (m_packer.Run(RunHold(false))) return m_packer.p_sInfo; 
                if (bForward)
                {
                    if (m_packer.Run(m_solDown.RunSol(true))) return m_packer.p_sInfo;
                    if (m_packer.Run(m_solForward.RunSol(true))) return m_packer.p_sInfo;
                }
                else
                {
                    if (m_packer.Run(m_solForward.RunSol(false))) return m_packer.p_sInfo;
                    if (m_packer.Run(m_solDown.RunSol(false))) return m_packer.p_sInfo;
                }
                return "OK";
            }

            public void RunTree(Tree tree)
            {
                m_secVac = tree.Set(m_secVac, m_secVac, "Vacuum", "Vacuum On Wait (sec)");
                m_secBlow = tree.Set(m_secBlow, m_secBlow, "Blow", "Vacuum Off Blow Time (sec)");
            }

            string m_id;
            VacuumPacker m_packer;
            public Holder(string id, VacuumPacker packer)
            {
                m_id = id;
                m_packer = packer;
            }
        }
        public Holder m_holder;
        #endregion

        #region Loader
        public class Loader
        {
            Axis m_axisMove;
            Axis m_axisBridge;
            Axis m_axisWidth; 
            DIO_I2O2 m_solPodPush;
            DIO_I2O2 m_solRaise;
            DIO_I m_diPodCheck;
            DIO_O m_doVacuumPump;
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_packer.p_sInfo = toolBox.Get(ref m_axisMove, m_packer, m_id + ".Move");
                m_packer.p_sInfo = toolBox.Get(ref m_axisBridge, m_packer, m_id + ".Bridge");
                m_packer.p_sInfo = toolBox.Get(ref m_axisWidth, m_packer, m_id + ".Width");
                m_packer.p_sInfo = toolBox.Get(ref m_solPodPush, m_packer, m_id + ".PodPush", "Back", "Push");
                m_packer.p_sInfo = toolBox.Get(ref m_solRaise, m_packer, m_id + ".Raise", "Down", "Up");
                m_packer.p_sInfo = toolBox.Get(ref m_diPodCheck, m_packer, m_id + ".Pod Check");
                m_packer.p_sInfo = toolBox.Get(ref m_doVacuumPump, m_packer, m_id + ".Vacuum Pump");
                if (bInit)
                {
                    m_packer.InitSolvalve(m_solPodPush); 
                    m_packer.InitSolvalve(m_solRaise);
                    InitPos();
                }
            }
            
            public enum eSpeed
            {
                Slow,
                Fast
            }
            public enum ePosMove
            {
                Ready,
                Vacuum,
                Heatiing
            }
            public enum ePosBridge
            {
                Ready,
                Bridge
            }
            public enum ePosWidth
            {
                Ready,
                Open
            }
            void InitPos()
            {
                m_doVacuumPump.Write(false);
                m_axisMove.AddSpeed(Enum.GetNames(typeof(eSpeed))); 
                m_axisMove.AddPos(Enum.GetNames(typeof(ePosMove)));
                m_axisBridge.AddPos(Enum.GetNames(typeof(ePosBridge)));
                m_axisWidth.AddPos(Enum.GetNames(typeof(ePosWidth)));
            }

            public string RunMove(ePosMove ePos, eSpeed eSpeed)
            {
                m_axisMove.StartMove(ePos, 0, eSpeed);
                return m_axisMove.WaitReady();
            }

            public string RunBridge(ePosBridge ePos)
            {
                m_axisBridge.StartMove(ePos);
                return m_axisBridge.WaitReady();
            }

            public string RunMoveWidth(ePosWidth ePos)
            {
                m_axisWidth.StartMove(ePos);
                return m_axisWidth.WaitReady();
            }

            public string RunPush()
            {
                try
                {
                    if (m_packer.Run(m_solPodPush.RunSol(true))) return m_packer.p_sInfo;
                    Thread.Sleep(200);
                    return m_solPodPush.RunSol(false);
                }
                finally { m_solPodPush.Write(false); }
            }

            public string RunRaise(bool bUp)
            {
                return m_solRaise.RunSol(bUp);
            }

            public bool IsPodExist()
            {
                return m_diPodCheck.p_bIn; 
            }

            public int m_secVac = 10; 
            public string RunVacuumPump()
            {
                m_doVacuumPump.Write(true);
                int msVac = (int)(1000 * m_secVac);
                StopWatch sw = new StopWatch(); 
                while (sw.ElapsedMilliseconds < msVac)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop"; 
                }
                return "OK";
            }

            public void RunTree(Tree tree)
            {
                m_secVac = tree.Set(m_secVac, m_secVac, "Vacuum", "Vacuum On Wait (sec)");
            }

            string m_id;
            VacuumPacker m_packer;
            public Loader(string id, VacuumPacker packer)
            {
                m_id = id;
                m_packer = packer;
            }
        }
        public Loader m_loader; 
        #endregion

        #region Heater
        public class Heater
        {
            DIO_I2O2[] m_solSponge = new DIO_I2O2[2];
            DIO_I2O2[] m_solHeater = new DIO_I2O2[2];
            DIO_IO m_dioHeat;
            DIO_O m_doHeatTimeout;
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_packer.p_sInfo = toolBox.Get(ref m_solSponge[0], m_packer, m_id + ".UpSponge", "Up", "Down");
                m_packer.p_sInfo = toolBox.Get(ref m_solSponge[1], m_packer, m_id + ".DownSponge", "Down", "Up");
                m_packer.p_sInfo = toolBox.Get(ref m_solHeater[0], m_packer, m_id + ".HeaterUp", "Down", "Up");
                m_packer.p_sInfo = toolBox.Get(ref m_solHeater[1], m_packer, m_id + ".Silicone", "Up", "Down");
                m_packer.p_sInfo = toolBox.Get(ref m_dioHeat, m_packer, m_id + ".Heat");
                m_packer.p_sInfo = toolBox.Get(ref m_doHeatTimeout, m_packer, m_id + ".Heat Timeout");
                if (bInit)
                {
                    m_dioHeat.Write(false);
                    m_doHeatTimeout.Write(false); 
                    m_packer.InitSolvalve(m_solSponge[0]);
                    m_packer.InitSolvalve(m_solSponge[1]);
                    m_packer.InitSolvalve(m_solHeater[0]);
                    m_packer.InitSolvalve(m_solHeater[1]);
                }
            }

            public string RunSponge(bool bClose)
            {
                m_solSponge[0].Write(bClose);
                m_solSponge[1].Write(bClose);
                int msTimeout = (int)(1000 * m_solSponge[0].m_secTimeout);
                StopWatch sw = new StopWatch();
                while (sw.ElapsedMilliseconds < msTimeout)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                    if (m_solSponge[0].p_bDone && m_solSponge[1].p_bDone) return "OK";
                }
                return "Sponge Timeout";
            }

            public string RunHeater(bool bClose)
            {
                m_solHeater[0].Write(bClose);
                m_solHeater[1].Write(bClose);
                int msTimeout = (int)(1000 * m_solHeater[0].m_secTimeout);
                StopWatch sw = new StopWatch();
                while (sw.ElapsedMilliseconds < msTimeout)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                    if (m_solHeater[0].p_bDone && m_solHeater[1].p_bDone) return "OK";
                }
                return "HeaterClose Timeout";
            }

            public double m_secHeat = 3; 
            public string RunHeat()
            {
                try
                {
                    m_dioHeat.Write(true);
                    StopWatch sw = new StopWatch();
                    int msHeat = (int)(1000 * m_secHeat);
                    while (sw.ElapsedMilliseconds < msHeat)
                    {
                        Thread.Sleep(10);
                        if (EQ.IsStop()) return "EQ Stop";
                    }
                    return "OK";
                }
                finally { m_dioHeat.Write(false); }
            }

            public string RunHeatTimeout(bool bOn)
            {
                m_doHeatTimeout.Write(bOn);
                return "OK";
            }

            public void RunTree(Tree tree)
            {
                m_secHeat = tree.Set(m_secHeat, m_secHeat, "Vacuum", "Vacuum On Wait (sec)");
            }

            string m_id;
            VacuumPacker m_packer;
            public Heater(string id, VacuumPacker packer)
            {
                m_id = id;
                m_packer = packer;
            }
        }
        public Heater m_heater;
        #endregion

        #region Function
        public enum eStep
        {
            GetWrapper,
            HoldWrapper,
            BackWrapper,
            InsertCase,
            ReleaseWrapper,
            CloseWrapper,
            VacuumPump,
            Heating,
            Rotate,
            PushToLoader,
            Unload,
        }

        public string RunStep(eStep eStep)
        {
            switch (eStep)
            {
                case eStep.GetWrapper:
                    if (m_wrapper.IsWapperExist() == false) return "No Wrapper";
                    if (Run(m_wrapper.RunMove(Wrapper.ePosMove.Pick))) return p_sInfo;
                    if (Run(m_wrapper.RunMove(Wrapper.ePosPicker.Down))) return p_sInfo;
                    if (Run(m_wrapper.RunVacOn())) return p_sInfo;
                    if (Run(m_wrapper.RunMove(Wrapper.ePosPicker.Up))) return p_sInfo;
                    if (Run(m_wrapper.RunMove(Wrapper.ePosMove.Place))) return p_sInfo;
                    if (Run(m_wrapper.RunMove(Wrapper.ePosPicker.Down))) return p_sInfo;
                    if (Run(m_stage.RunVac(true, true))) return p_sInfo;
                    if (Run(m_wrapper.RunVacOff(false))) return p_sInfo;
                    if (Run(m_wrapper.RunMove(Wrapper.ePosPicker.Open))) return p_sInfo;
                    return "OK";
                case eStep.HoldWrapper:
                    if (Run(m_holder.RunForward(true))) return p_sInfo;
                    if (Run(m_holder.RunHold(true))) return p_sInfo;
                    return "OK";
                case eStep.BackWrapper:
                    if (Run(m_wrapper.RunVacOff(true))) return p_sInfo;
                    if (Run(m_wrapper.RunMove(Wrapper.ePosPicker.Up))) return p_sInfo;
                    if (Run(m_wrapper.RunMove(Wrapper.ePosMove.Pick))) return p_sInfo;
                    return "OK";
                case eStep.InsertCase:
                    try
                    {
                        if (Run(m_loader.RunMoveWidth(Loader.ePosWidth.Ready))) return p_sInfo;
                        if (Run(m_loader.RunBridge(Loader.ePosBridge.Bridge))) return p_sInfo;
                        if (Run(m_loader.RunMove(Loader.ePosMove.Vacuum, Loader.eSpeed.Slow))) return p_sInfo;
                        if (Run(m_transfer.RunDown(true))) return p_sInfo;
                        if (Run(m_transfer.RunMove(Transfer.ePos.Push))) return p_sInfo;
                        if (Run(m_transfer.RunPush())) return p_sInfo;
                    }
                    finally 
                    {
                        m_transfer.RunDown(false); 
                        m_transfer.RunMove(Transfer.ePos.Ready); 
                    }
                    return "OK";
                case eStep.ReleaseWrapper:
                    if (Run(m_holder.RunHold(false))) return p_sInfo;
                    if (Run(m_holder.RunForward(false))) return p_sInfo;
                    return "OK";
                case eStep.CloseWrapper:
                    if (Run(m_loader.RunMoveWidth(Loader.ePosWidth.Open))) return p_sInfo;
                    if (Run(m_heater.RunSponge(true))) return p_sInfo;
                    return "OK";
                case eStep.VacuumPump:
                    if (Run(m_loader.RunVacuumPump())) return p_sInfo;
                    if (Run(m_loader.RunMove(Loader.ePosMove.Heatiing, Loader.eSpeed.Fast))) return p_sInfo; 
                    return "OK";
                case eStep.Heating:
                    if (Run(m_heater.RunHeater(true))) return p_sInfo;
                    if (Run(m_heater.RunHeat())) return p_sInfo;
                    if (Run(m_heater.RunHeater(false))) return p_sInfo;
                    if (Run(m_loader.RunMove(Loader.ePosMove.Ready, Loader.eSpeed.Slow))) return p_sInfo;
                    return "OK";
                case eStep.Rotate:
                    if (Run(m_stage.RunVac(true, false))) return p_sInfo;
                    if (Run(m_stage.RunUp(true))) return p_sInfo;
                    if (Run(m_stage.RunRotate(true))) return p_sInfo;
                    if (Run(m_stage.RunUp(false))) return p_sInfo;
                    if (Run(m_stage.RunVac(true, true))) return p_sInfo;
                    return "OK";
                case eStep.PushToLoader:
                    if (Run(m_wrapper.RunMove(Wrapper.ePosPicker.Push))) return p_sInfo;
                    if (Run(m_stage.RunVac(false, false))) return p_sInfo;
                    if (Run(m_wrapper.RunMove(Wrapper.ePosMove.Push))) return p_sInfo;
                    if (Run(m_wrapper.RunPush())) return p_sInfo;
                    if (Run(m_wrapper.RunMove(Wrapper.ePosMove.Pick))) return p_sInfo;
                    if (Run(m_wrapper.RunMove(Wrapper.ePosPicker.Up))) return p_sInfo;
                    if (Run(m_stage.RunRotate(false))) return p_sInfo;
                    return "OK";
                case eStep.Unload:
                    if (Run(m_loader.RunMoveWidth(Loader.ePosWidth.Ready))) return p_sInfo;
                    if (Run(m_loader.RunMove(Loader.ePosMove.Ready, Loader.eSpeed.Slow))) return p_sInfo;
                    if (Run(m_loader.RunBridge(Loader.ePosBridge.Ready))) return p_sInfo;
                    return "OK";
            }
            return "OK"; 
        }

        eStep[] m_aStep =
        {
            eStep.GetWrapper,
            eStep.HoldWrapper,
            eStep.BackWrapper,
            eStep.InsertCase,
            eStep.ReleaseWrapper,
            eStep.CloseWrapper,
            eStep.VacuumPump,
            eStep.Heating,
            eStep.Rotate,
            eStep.PushToLoader,
        };
        public string RunPacking() //recover ???
        {
            foreach (eStep eStep in m_aStep)
            {
                if (Run(RunStep(eStep))) return p_sInfo; 
            }
            foreach (eStep eStep in m_aStep)
            {
                if (Run(RunStep(eStep))) return p_sInfo;
            }
            if (Run(RunStep(eStep.Unload))) return p_sInfo; 
            return "OK"; 
        }
        #endregion

        #region InfoWafer
        string m_sInfoWafer = "";
        InfoWafer _infoWafer = null;
        public InfoWafer p_infoWafer
        {
            get { return _infoWafer; }
            set
            {
                m_sInfoWafer = (value == null) ? "" : value.p_id;
                _infoWafer = value;
                if (m_reg != null) m_reg.Write("sInfoWafer", m_sInfoWafer);
                OnPropertyChanged();
            }
        }

        Registry m_reg = null;
        public void ReadInfoWafer_Registry()
        {
            m_reg = new Registry(p_id + ".InfoWafer");
            m_sInfoWafer = m_reg.Read("sInfoWafer", m_sInfoWafer);
            p_infoWafer = m_engineer.ClassHandler().GetGemSlot(m_sInfoWafer);
        }
        #endregion

        #region IWTRChild
        bool _bLock = false;
        public bool p_bLock
        {
            get { return _bLock; }
            set
            {
                if (_bLock == value) return;
                _bLock = value;
            }
        }

        bool IsLock()
        {
            for (int n = 0; n < 10; n++)
            {
                if (p_bLock == false) return false;
                Thread.Sleep(100);
            }
            return true;
        }

        public List<string> p_asChildSlot { get { return null; } }

        public InfoWafer GetInfoWafer(int nID)
        {
            return p_infoWafer;
        }

        public void SetInfoWafer(int nID, InfoWafer infoWafer)
        {
            p_infoWafer = infoWafer;
        }

        public string IsGetOK(int nID)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            if (p_infoWafer == null) return p_id + " IsGetOK - InfoWafer not Exist";
            return "OK";
        }

        public string IsPutOK(InfoWafer infoWafer, int nID)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            if (p_infoWafer != null) return p_id + " IsPutOK - InfoWafer Exist";
            if (m_waferSize.GetData(infoWafer.p_eSize).m_bEnable == false) return p_id + " not Enable Wafer Size";
            return "OK";
        }

        public int GetTeachWTR(InfoWafer infoWafer = null)
        {
            if (infoWafer == null) infoWafer = p_infoWafer;
            return m_waferSize.GetData(infoWafer.p_eSize).m_teachWTR;
        }

        public string BeforeGet(int nID)
        {
            //if (p_infoWafer == null) return m_id + " BeforeGet : InfoWafer = null";
            return CheckGetPut();
        }

        public string BeforePut(int nID)
        {
            if (p_infoWafer != null) return p_id + " BeforePut : InfoWafer != null";
            return CheckGetPut();
        }

        public string AfterGet(int nID)
        {
            return CheckGetPut();
        }

        public string AfterPut(int nID)
        {
            return "OK";
        }

        string CheckGetPut()
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            return "OK";
        }

        enum eCheckWafer
        {
            InfoWafer,
            Sensor
        }
        eCheckWafer m_eCheckWafer = eCheckWafer.InfoWafer;
        public bool IsWaferExist(int nID)
        {
            switch (m_eCheckWafer)
            {
                case eCheckWafer.Sensor: return false; // m_diWaferExist.p_bIn;
                default: return (p_infoWafer != null);
            }
        }

        InfoWafer.WaferSize m_waferSize;
        public void RunTreeTeach(Tree tree)
        {
            m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
        }
        #endregion

        #region Override
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            m_wrapper.RunTree(tree.GetTree("Wapper"));
            m_stage.RunTree(tree.GetTree("Stage"));
            m_holder.RunTree(tree.GetTree("Holder"));
            m_loader.RunTree(tree.GetTree("Loader"));
            m_heater.RunTree(tree.GetTree("Heater"));
        }

        public override void Reset()
        {
            base.Reset();
        }
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
        #endregion

        public VacuumPacker(string id, IEngineer engineer)
        {
            m_wrapper = new Wrapper("Wrapper", this);
            m_stage = new Stage("Stage", this); 
            m_transfer = new Transfer("Transfer", this);
            m_holder = new Holder("holder", this);
            m_loader = new Loader("Loader", this);
            m_heater = new Heater("Heater", this); 
            m_waferSize = new InfoWafer.WaferSize(id, false, false);
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Just Time Delay");
            AddModuleRunList(new Run_Solvalve(this), false, "Run Solvalve");
            AddModuleRunList(new Run_Step(this), false, "Run Step");
            AddModuleRunList(new Run_Packing(this), true, "Run Packing");
        }

        public class Run_Delay : ModuleRunBase
        {
            VacuumPacker m_module;
            public Run_Delay(VacuumPacker module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_secDelay = 2;
            public override ModuleRunBase Clone()
            {
                Run_Delay run = new Run_Delay(m_module);
                run.m_secDelay = m_secDelay;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Time Delay (sec)", bVisible);
            }

            public override string Run()
            {
                Thread.Sleep((int)(1000 * m_secDelay));
                return "OK";
            }
        }

        public class Run_Solvalve : ModuleRunBase
        {
            VacuumPacker m_module;
            public Run_Solvalve(VacuumPacker module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            string m_sSol = "";
            bool m_bOn = false;
            public override ModuleRunBase Clone()
            {
                Run_Solvalve run = new Run_Solvalve(m_module);
                run.m_sSol = m_sSol;
                run.m_bOn = m_bOn;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sSol = tree.Set(m_sSol, m_sSol, m_module.p_asSol, "SolValve", "Run SolValve", bVisible);
                m_bOn = tree.Set(m_bOn, m_bOn, "On", "Run SolValue On/Off", bVisible);
            }

            public override string Run()
            {
                DIO_I2O2 sol = m_module.GetSolvalve(m_sSol);
                if (sol == null) return "Invalid Solvalve Name";
                sol.Write(m_bOn);
                return sol.WaitDone();
            }
        }

        public class Run_Step : ModuleRunBase
        {
            VacuumPacker m_module;
            public Run_Step(VacuumPacker module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            eStep m_eStep = eStep.GetWrapper;
            public override ModuleRunBase Clone()
            {
                Run_Step run = new Run_Step(m_module);
                run.m_eStep = m_eStep;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eStep = (eStep)tree.Set(m_eStep, m_eStep, "Step", "Select Step", bVisible);
            }

            public override string Run()
            {
                return m_module.RunStep(m_eStep); 
            }
        }

        public class Run_Packing : ModuleRunBase
        {
            VacuumPacker m_module;
            public Run_Packing(VacuumPacker module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            int m_secVacuumPump = 5; 
            double m_secHeat = 3; 
            public override ModuleRunBase Clone()
            {
                Run_Packing run = new Run_Packing(m_module);
                run.m_secVacuumPump = m_secVacuumPump;
                run.m_secHeat = m_secHeat;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_secVacuumPump = tree.Set(m_secVacuumPump, m_secVacuumPump, "Vacuum Pump", "Vacuum Pump Time (sec)", bVisible);
                m_secHeat = tree.Set(m_secHeat, m_secHeat, "Heat", "Heat Time (sec)", bVisible);
            }

            public override string Run()
            {
                m_module.m_loader.m_secVac = m_secVacuumPump;
                m_module.m_heater.m_secHeat = m_secHeat; 
                return m_module.RunPacking();
            }
        }
        #endregion
    }
}
