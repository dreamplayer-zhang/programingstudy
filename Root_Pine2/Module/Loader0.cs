using Root_Pine2.Engineer;
using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Root_Pine2.Module
{
    public class Loader0 : ModuleBase
    {
        #region ToolBox
        public Axis3D m_axis;
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axis, this, "Loader0");
            m_picker.GetTools(m_toolBox, this, bInit);
            m_keyence.GetTools(m_toolBox, this, bInit); 
            if (bInit) InitPosition();
        }

        public enum ePosTransfer
        {
            Transfer0,
            Transfer1,
            Transfer2,
            Transfer3,
            Transfer4,
            Transfer5,
            Transfer6,
            Transfer7,
        }
        public enum ePosTray
        {
            Tray0,
            Tray1,
            Tray2,
            Tray3,
            Tray4,
            Tray5,
            Tray6,
            Tray7,
        }
        public enum eUnloadVision
        {
            Top3D,
            Top2D,
        }
        const string c_sPosLoadEV = "LoadEV";
        const string c_sPosPaper = "Paper";
        const string c_sPosUp = "Up";
        const string c_sPosKeyence = "Keyence";
        void InitPosition()
        {
            m_axis.AddPos(c_sPosLoadEV);
            m_axis.AddPos(c_sPosPaper);
            m_axis.AddPos(c_sPosKeyence);
            m_axis.AddPos(ePosTransfer.Transfer0.ToString());
            m_axis.AddPos(ePosTransfer.Transfer7.ToString());
            m_axis.AddPos(GetPosString(eUnloadVision.Top3D, eWorks.A));
            m_axis.AddPos(GetPosString(eUnloadVision.Top3D, eWorks.B));
            m_axis.AddPos(GetPosString(eUnloadVision.Top2D, eWorks.A));
            m_axis.AddPos(GetPosString(eUnloadVision.Top2D, eWorks.B));
            m_axis.AddPos(ePosTray.Tray0.ToString());
            m_axis.AddPos(ePosTray.Tray7.ToString());
            m_axis.p_axisZ.AddPos(c_sPosUp);
        }
        string GetPosString(eUnloadVision eVision, eWorks eWorks)
        {
            return eVision.ToString() + eWorks.ToString(); 
        }
        #endregion

        #region Keyence SR-1000
        class Keyence
        {
            TCPAsyncClient m_tcpip;
            public void GetTools(ToolBox toolBox, ModuleBase module, bool bInit)
            {
                toolBox.GetComm(ref m_tcpip, module, "Keyence");
                if (bInit) m_tcpip.EventReceiveData += M_tcpip_EventReceiveData;
            }

            bool m_bRecieve = false; 
            public string m_sCode = "";
            private void M_tcpip_EventReceiveData(byte[] aBuf, int nSize, Socket socket)
            {
                string sMsg = Encoding.Default.GetString(aBuf, 0, nSize - 1);
                if ((sMsg == "OK,LON") || (sMsg == "OK,LOFF")) m_sCode = "";
                else m_sCode = sMsg;
                m_bRecieve = true; 
            }

            public string ReadCode()
            {
                if (m_tcpip.p_bConnect == false)
                {
                    m_tcpip.Connect();
                    Thread.Sleep(10);
                    if (m_tcpip.p_bConnect == false) return "Not Connected";
                }
                Thread.Sleep(10);
                m_bRecieve = false; 
                m_tcpip.Send("LON\r");
                StopWatch sw = new StopWatch();
                while (sw.ElapsedMilliseconds < 1000)
                {
                    Thread.Sleep(10); 
                    if (m_bRecieve)
                    {
                        if (m_sCode == "") m_tcpip.Send("LOFF\r"); 
                        return m_sCode; 
                    }
                }
                return m_sCode;
            }

            public void ThreadStop()
            {
                m_tcpip.ThreadStop(); 
            }
        }
        Keyence m_keyence = new Keyence(); 
        #endregion

        #region AvoidX
        public const double c_lAxisX = 1557800;
        Loader3 p_loader3 { get { return m_handler.m_loader3; } }
        string StartMoveX(string sPos, double dPos)
        {
            Axis axisX = p_loader3.m_axis.p_axisX;
            double fPos = m_axis.p_axisX.GetPosValue(sPos) + dPos;
            while ((fPos + axisX.m_posDst) > c_lAxisX)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
                if (p_loader3.IsBusy() == false)
                {
                    p_loader3.StartAvoidX(fPos);
                    Thread.Sleep(10);
                }
            }
            return m_axis.p_axisX.StartMove(fPos);
        }

        public string StartAvoidX(double fPos)
        {
            Run_AvoidX run = (Run_AvoidX)m_runAvoidX.Clone();
            run.m_fPos = c_lAxisX - fPos;
            return StartRun(run);
        }

        public string RunAvoidX(double fPos)
        {
            m_axis.p_axisX.StartMove(fPos);
            //m_axis.p_axisY.StartMove((p_infoStrip == null) ? ePosTransfer.Transfer7.ToString() : GetPosString(eUnloadVision.Top2D, eWorks.A)); 
            return m_axis.p_axisX.WaitReady();
        }
        #endregion

        #region AxisXY
        double GetXOffset(InfoStrip.eMagazine ePos)
        {
            double xScale = m_transfer.m_buffer.GetXScale(ePos);
            double p0 = m_axis.p_axisX.GetPosValue(ePosTransfer.Transfer0.ToString()); 
            double p7 = m_axis.p_axisX.GetPosValue(ePosTransfer.Transfer7.ToString());
            return xScale * (p7 - p0); 
        }

        public string RunMoveTransfer(ePosTransfer ePos, double xOffset, bool bWait = true)
        {
            xOffset += GetXOffset((InfoStrip.eMagazine)ePos);
            if (Run(StartMoveX(ePosTransfer.Transfer0.ToString(), xOffset))) return p_sInfo; 
            m_axis.p_axisY.StartMove(ePosTransfer.Transfer0);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveBoat(eUnloadVision eVision, eWorks eWorks, bool bWait = true)
        {
            string sPos = GetPosString(eVision, eWorks);
            if (Run(StartMoveX(sPos, 0))) return p_sInfo;
            m_axis.p_axisY.StartMove(sPos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveTray(ePosTray eTray, bool bWait = true)
        {
            double xOffset = GetXOffset((InfoStrip.eMagazine)eTray);
            if (Run(StartMoveX(ePosTray.Tray0.ToString(), xOffset))) return p_sInfo;
            m_axis.p_axisY.StartMove(ePosTray.Tray0);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        int m_pulsemm = 1000; 
        public string RunMoveLoadEV(bool bWait = true)
        {
            double dPos = m_pulsemm * (m_pine2.m_widthDefaultStrip - m_pine2.p_widthStrip);
            if (Run(StartMoveX(c_sPosLoadEV, dPos))) return p_sInfo;
            double xDst = m_axis.p_axisX.m_posDst;
            double yDst = m_axis.p_axisY.GetPosValue(ePosTransfer.Transfer7);
            while (Math.Abs(m_axis.p_axisX.p_posCommand - xDst) > Math.Abs(m_axis.p_axisY.p_posCommand - yDst)) Thread.Sleep(10); 
            m_axis.p_axisY.StartMove(c_sPosLoadEV);
            return bWait ? m_axis.WaitReady() : "OK"; 
        }

        public string RunMovePaper()
        {
            double dPos = m_pulsemm * (m_pine2.m_widthDefaultStrip - m_pine2.p_widthStrip);
            if (Run(StartMoveX(c_sPosPaper, dPos))) return p_sInfo;
            m_axis.p_axisY.StartMove(c_sPosLoadEV);
            return m_axis.WaitReady();
        }

        public string RunMove(string sPos, bool bWait = true)
        {
            if (Run(StartMoveX(sPos, 0))) return p_sInfo;
            m_axis.p_axisY.StartMove(sPos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        void RunTreeAxis(Tree tree)
        {
            m_pulsemm = tree.Set(m_pulsemm, m_pulsemm, "Pulse / mm", "Axis X (Pulse / mm)");
        }
        #endregion

        #region AxisZ
        double p_dZ
        {
            get { return 0;/* (m_pine2.m_thicknessDefault - m_pine2.p_thickness) * 10;*/ }
        }

        public string RunMoveZ(string sPos, double dPos, bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(sPos, dPos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveZ(ePosTransfer ePos, bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(ePosTransfer.Transfer7, p_dZ);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveZ(eUnloadVision eVision, eWorks eWorks, bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(GetPosString(eVision, eWorks), p_dZ);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveZPaper(ePosTray eTray, bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(ePosTray.Tray7);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveUp(bool bWait = true)
        {
            try
            {
                m_axis.p_axisZ.m_bCheckStop = false;
                m_axis.p_axisZ.StartMove(c_sPosUp);
                return bWait ? m_axis.WaitReady() : "OK";
            }
            finally { m_axis.p_axisZ.m_bCheckStop = true; }
        }

        public string RunShakeUp(int nShake, int dzPulse)
        {
            int iShake = 0; 
            while (iShake < nShake)
            {
                if (Run(RunShakeUp(-dzPulse))) return p_sInfo; 
                if (Run(RunShakeUp(0.9 * dzPulse))) return p_sInfo;
                iShake++;
            }
            return "OK"; 
        }

        string RunShakeUp(double dzPulse)
        {
            m_axis.p_axisZ.StartShift(dzPulse + p_dZ);
            return m_axis.WaitReady(); 
        }
        #endregion

        #region RunLoad
        public string RunLoadEV(int nShake, int dzShakeUp)
        {
            if (m_picker.p_infoStrip != null) return "InfoStrip != null";
            if (m_loadEV.p_bDone == false) return "LoadEV not Done";
            try
            {
                if (Run(RunMoveUp())) return p_sInfo;
                if (m_pine2.p_bUseKeyence)
                {
                    if (Run(RunMove(c_sPosKeyence))) return p_sInfo;
                    if (Run(RunMoveZ(c_sPosKeyence, 0))) return p_sInfo;
                    m_keyence.ReadCode();
                }
                if (Run(RunMoveLoadEV())) return p_sInfo;
                if (Run(RunMoveZ(c_sPosLoadEV, 0))) return p_sInfo;
                m_loadEV.p_bBlow = true;
                if (Run(m_picker.RunVacuum(true)))
                {
                    m_loadEV.p_bCycleStop = true; 
                    return "OK";
                }
                m_loadEV.p_eMove = LoadEV.eMove.Down; 
                if (Run(RunShakeUp(nShake, dzShakeUp))) return p_sInfo;
                m_loadEV.p_eMove = LoadEV.eMove.Stop;
                m_loadEV.p_bBlow = false;
                if (m_pine2.p_bCheckPaper)
                {
                    if (Run(RunMoveZ(c_sPosPaper, 0))) return p_sInfo;
                    if (Run(RunMovePaper())) return p_sInfo;
                }
                m_loadEV.CheckPaper();
                if (Run(RunMoveUp())) return p_sInfo;
                if (m_picker.IsVacuum() == false) return p_sInfo;
                m_picker.p_infoStrip = m_loadEV.GetNewInfoStrip();
                if (m_keyence.m_sCode != "") m_picker.p_infoStrip.p_id = m_keyence.m_sCode; 
                m_loadEV.StartLoad(); 
            }
            finally
            {
                m_loadEV.p_eMove = LoadEV.eMove.Stop;
                m_loadEV.p_bBlow = false;
                RunMoveUp();
            }
            return "OK";
        }

        string StartLoadTransfer() 
        {
            if (m_transfer.m_gripper.p_infoStrip == null) return "OK";
            Run_LoadTransfer run = (Run_LoadTransfer)m_runLoadTransfer.Clone();
            run.m_bCheckEnable = true; 
            return StartRun(run);
        }

        public string RunLoadTransfer(bool bCheckEnable)
        {
            Transfer.Gripper gripper = m_transfer.m_gripper;
            if (m_picker.p_infoStrip != null) return "InfoStrip != null";
            try
            {
                gripper.p_bLock = true;
                if (bCheckEnable && (gripper.p_bEnable == false)) return "OK";
                ePosTransfer ePos = (ePosTransfer)m_transfer.m_buffer.m_ePosDst;
                double xOffset = m_transfer.m_buffer.m_xOffset;
                //m_transfer.m_buffer.RunAlign(true);
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(RunMoveTransfer(ePos, -xOffset))) return p_sInfo;
                m_transfer.m_buffer.RunAlign(true);
                if (Run(RunMoveZ(ePos))) return p_sInfo;
                if (Run(m_picker.RunVacuum(true))) return p_sInfo;
                Thread.Sleep(300);
                if (Run(RunMoveUp())) return p_sInfo;
                if (m_picker.IsVacuum() == false) return p_sInfo;
                m_picker.p_infoStrip = gripper.p_infoStrip;
                gripper.p_infoStrip = null;
            }
            finally
            {
                RunMoveUp();
                gripper.p_bLock = false;
            }
            return "OK";
        }
        #endregion

        #region RunUnload
        public string StartUnloadStrip()
        {
            return StartRun(m_runUnloadStrip);
        }

        public string RunUnloadStrip()
        {
            if (p_infoStrip == null) return "OK";
            try
            {
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(RunMoveLoadEV())) return p_sInfo;
                if (Run(RunMoveZ(c_sPosLoadEV, 5000))) return p_sInfo;
                if (Run(m_picker.RunVacuum(false))) return p_sInfo;
                if (Run(RunMoveUp())) return p_sInfo;
                m_picker.p_infoStrip = null;
            }
            finally
            {
                RunMoveUp();
            }
            return "OK";
        }

        public string RunUnloadPaper()
        {
            if (m_picker.p_infoStrip != null) return "InfoStrip != null";
            try
            {
                if (Run(RunMoveUp())) return p_sInfo;
                ePosTray ePosTray = ePosTray.Tray7;
                if (Run(GetPaperTray(ref ePosTray))) return p_sInfo; 
                if (Run(RunMoveTray(ePosTray))) return p_sInfo;
                if (Run(RunMoveZPaper(ePosTray))) return p_sInfo;
                if (Run(m_picker.RunVacuum(false))) return p_sInfo;
                m_picker.p_infoStrip = null;
                MagazineEV magazine = m_handler.m_magazineEVSet.m_aEV[(InfoStrip.eMagazine)ePosTray];
                magazine.PutInfoStrip(null); 
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(RunMoveLoadEV())) return p_sInfo;
            }
            finally
            {
                RunMoveUp();
            }
            return "OK";
        }

        string GetPaperTray(ref ePosTray ePosTray)
        {
            MagazineEVSet magazine = m_handler.m_magazineEVSet;
            for (InfoStrip.eMagazine eMagazine = InfoStrip.eMagazine.Magazine7; eMagazine >= InfoStrip.eMagazine.Magazine0; eMagazine--)
            {
                if (magazine.IsEnableStack(eMagazine, InfoStrip.eResult.Paper, true))
                {
                    ePosTray = (ePosTray)eMagazine;
                    return "OK";
                }
            }
            for (InfoStrip.eMagazine eMagazine = InfoStrip.eMagazine.Magazine7; eMagazine >= InfoStrip.eMagazine.Magazine0; eMagazine--)
            {
                if (magazine.IsEnableStack(eMagazine, InfoStrip.eResult.Paper, false))
                {
                    ePosTray = (ePosTray)eMagazine;
                    return "OK";
                }
            }
            return "Paper Tray not Ready"; 
        }

        string StartUnloadBoat()
        {
            eVision eVision = m_pine2.p_b3D ? eVision.Top3D : eVision.Top2D;
            Boats boats = m_handler.m_aBoats[eVision];
            if ((boats.m_aBoat[eWorks.A].p_eStep == Boat.eStep.Ready) && (boats.m_aBoat[eWorks.A].p_infoStrip == null)) return StartUnloadBoat(eVision, eWorks.A);
            if ((boats.m_aBoat[eWorks.B].p_eStep == Boat.eStep.Ready) && (boats.m_aBoat[eWorks.B].p_infoStrip == null)) return StartUnloadBoat(eVision, eWorks.B);
            return "OK";
        }

        string StartUnloadBoat(eVision eVision, eWorks eWorks)
        {
            Run_UnloadBoat run = (Run_UnloadBoat)m_runUnloadBoat.Clone();
            run.m_eVision = (eVision == eVision.Top3D) ? eUnloadVision.Top3D : eUnloadVision.Top2D;
            run.m_eWorks = eWorks;
            return StartRun(run);
        }

        public string RunUnloadBoat(eUnloadVision eVision, eWorks eWorks)
        {
            if (m_picker.p_infoStrip == null) return "InfoStrip == null";
            Boats boats = GetBoats(eVision);
            Boat boat = boats.m_aBoat[eWorks];
            if (boat.p_eStep != Boat.eStep.Ready) return "Boat not Ready";
            try
            {
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(boats.RunMoveReady(eWorks))) return p_sInfo; 
                if (Run(RunMoveBoat(eVision, eWorks))) return p_sInfo;
                if (Run(RunMoveZ(eVision, eWorks))) return p_sInfo;
                boat.RunVacuum(true);
                if (Run(m_picker.RunVacuum(false))) return p_sInfo;
                if (Run(RunMoveUp(false))) return p_sInfo;
                Thread.Sleep(200);
                boat.StartClean();
                boat.p_infoStrip = m_picker.p_infoStrip;
                m_picker.p_infoStrip = null;
                boat.p_infoStrip.m_eWorks = eWorks;
                if (Run(m_axis.WaitReady())) return p_sInfo;
            }
            finally
            {
                RunMoveUp();
            }
            return "OK";
        }

        Boats GetBoats(eUnloadVision eUnloadVision)
        {
            switch (eUnloadVision)
            {
                case eUnloadVision.Top3D: return m_handler.m_aBoats[eVision.Top3D];
                case eUnloadVision.Top2D: return m_handler.m_aBoats[eVision.Top2D];
            }
            return null; 
        }
        #endregion

        #region PickerSet
        double m_mmPickerSetUp = 10;
        public string RunPickerSet()
        {
            double sec = 0;
            double pulseUp = m_pulsemm * m_mmPickerSetUp; 
            try
            {
                if (Run(RunMoveUp())) return p_sInfo; 
                string sPick = (m_pine2.p_eMode == Pine2.eRunMode.Stack) ? c_sPosLoadEV : ePosTransfer.Transfer7.ToString();
                switch (m_pine2.p_eMode)
                {
                    case Pine2.eRunMode.Stack: if (Run(RunMoveLoadEV())) return p_sInfo; break;
                    case Pine2.eRunMode.Magazine: if (Run(RunMoveTransfer(ePosTransfer.Transfer7, 0))) return p_sInfo; break; 
                }
                if (Run(m_picker.RunVacuum(false))) return p_sInfo;
                bool bUp = false; 
                while (true)
                {
                    if (Run(RunMoveZ(sPick, bUp ? pulseUp : 0))) return p_sInfo;
                    if (Run(m_picker.RunVacuum(bUp))) return p_sInfo;
                    if (Run(m_pine2.WaitPickerSet(ref sec))) return p_sInfo;
                    m_pine2.p_diPickerSet = false;
                    if (sec > 1)
                    {
                        RunMoveUp();
                        switch (m_pine2.p_eMode)
                        {
                            case Pine2.eRunMode.Stack: m_picker.p_infoStrip = m_loadEV.GetNewInfoStrip(); break;
                            case Pine2.eRunMode.Magazine: 
                                m_picker.p_infoStrip = m_transfer.m_gripper.p_infoStrip;
                                m_transfer.m_gripper.p_infoStrip = null;
                                break; 
                        }
                        return "OK";
                    }
                    bUp = !bUp; 
                }
            }
            finally
            {
                RunMoveUp(); 
            }
        }

        void RunTreePickerSet(Tree tree)
        {
            m_mmPickerSetUp = tree.Set(m_mmPickerSetUp, m_mmPickerSetUp, "Picker Up", "Picker Up (mm)");
        }
        #endregion

        #region override
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            p_sInfo = base.StateHome(m_axis.p_axisZ);
            if (p_sInfo != "OK") return p_sInfo;
            RunMoveUp(); 
            p_sInfo = base.StateHome(m_axis.p_axisX, m_axis.p_axisY);
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }

        public override string StateReady()
        {
            if (EQ.p_eState != EQ.eState.Run) return "OK";
            if (m_pine2.p_eMode == Pine2.eRunMode.Magazine)
            {
                double fPos = m_axis.p_axisY.GetPosValue(ePosTransfer.Transfer7) + 5000;
                if (m_axis.p_axisY.p_posCommand > fPos)
                {
                    m_axis.p_axisY.StartMove(fPos);
                    m_axis.p_axisY.WaitReady(); 
                }
            }
            if (m_picker.p_infoStrip != null)
            {
                if (m_picker.p_infoStrip.m_bPaper) return StartRun(m_runUnloadPaper); 
                return StartUnloadBoat();
            }
            else
            {
                switch (m_pine2.p_eMode)
                {
                    case Pine2.eRunMode.Stack: return (m_loadEV.p_bDone && (m_loadEV.p_bCycleStop == false)) ? StartRun(m_runLoadEV) : "OK";
                    case Pine2.eRunMode.Magazine: return m_transfer.m_gripper.p_bEnable ? StartLoadTransfer() : "OK";
                }
            }
            return "OK";
        }

        public override void Reset()
        {
            m_picker.m_dioVacuum.Write(false);
            m_picker.p_infoStrip = null;
            RunMoveUp(false); 
            base.Reset();
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            m_picker.RunTreeVacuum(tree.GetTree("Vacuum"));
            RunTreeAxis(tree.GetTree("Axis"));
            RunTreePickerSet(tree.GetTree("PickerSet")); 
        }
        #endregion

        public InfoStrip p_infoStrip { get { return m_picker.p_infoStrip; } }
        Picker m_picker = null;
        Pine2 m_pine2 = null;
        LoadEV m_loadEV = null;
        Transfer m_transfer = null;
        Pine2_Handler m_handler; 
        public Loader0(string id, IEngineer engineer, Pine2_Handler handler)
        {
            m_picker = new Picker(id);
            m_pine2 = handler.m_pine2;
            m_loadEV = handler.m_loadEV;
            m_transfer = handler.m_transfer;
            m_handler = handler; 
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            m_picker.ThreadStop();
            m_keyence.ThreadStop(); 
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runLoadEV;
        ModuleRunBase m_runUnloadStrip;
        ModuleRunBase m_runLoadTransfer;
        ModuleRunBase m_runUnloadPaper;
        ModuleRunBase m_runUnloadBoat;
        ModuleRunBase m_runAvoidX;
        protected override void InitModuleRuns()
        {
            m_runLoadEV = AddModuleRunList(new Run_LoadEV(this), true, "Load Strip from LoadEV");
            m_runUnloadStrip = AddModuleRunList(new Run_UnloadStrip(this), false, "Unload Strip to GetPosition");
            m_runLoadTransfer = AddModuleRunList(new Run_LoadTransfer(this), true, "Load Strip from Transfer");
            m_runUnloadPaper = AddModuleRunList(new Run_UnloadPaper(this), true, "Unload Paper to Tray");
            m_runUnloadBoat = AddModuleRunList(new Run_UnloadBoat(this), true, "Unload Paper to Boat");
            m_runAvoidX = AddModuleRunList(new Run_AvoidX(this), false, "Avoid Axis X");
            AddModuleRunList(new Run_PickerSet(this), false, "Picker Set");
        }

        public class Run_LoadEV : ModuleRunBase
        {
            Loader0 m_module;
            public Run_LoadEV(Loader0 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            int m_nShake = 2;
            int m_dzShakeUp = 5000; 
            public override ModuleRunBase Clone()
            {
                Run_LoadEV run = new Run_LoadEV(m_module);
                run.m_nShake = m_nShake;
                run.m_dzShakeUp = m_dzShakeUp;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_nShake = tree.Set(m_nShake, m_nShake, "Shake", "Shake Count", bVisible);
                m_dzShakeUp = tree.Set(m_dzShakeUp, m_dzShakeUp, "ShakeUp", "Shake Up (Pulse)", bVisible);
            }

            public override string Run()
            {
                return m_module.RunLoadEV(m_nShake, m_dzShakeUp);
            }
        }

        public class Run_UnloadStrip : ModuleRunBase
        {
            Loader0 m_module;
            public Run_UnloadStrip(Loader0 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_UnloadStrip run = new Run_UnloadStrip(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunUnloadStrip();
            }
        }

        public class Run_LoadTransfer : ModuleRunBase
        {
            Loader0 m_module;
            public Run_LoadTransfer(Loader0 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_LoadTransfer run = new Run_LoadTransfer(m_module);
                return run;
            }

            public bool m_bCheckEnable = false; 
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunLoadTransfer(m_bCheckEnable);
            }
        }

        public class Run_UnloadPaper : ModuleRunBase
        {
            Loader0 m_module;
            public Run_UnloadPaper(Loader0 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_UnloadPaper run = new Run_UnloadPaper(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunUnloadPaper(); 
            }
        }

        public class Run_UnloadBoat : ModuleRunBase
        {
            Loader0 m_module;
            public Run_UnloadBoat(Loader0 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public eUnloadVision m_eVision = eUnloadVision.Top3D;
            public eWorks m_eWorks = eWorks.A; 
            public override ModuleRunBase Clone()
            {
                Run_UnloadBoat run = new Run_UnloadBoat(m_module);
                run.m_eVision = m_eVision;
                run.m_eWorks = m_eWorks;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eVision = (eUnloadVision)tree.Set(m_eVision, m_eVision, "Vision", "Select Vision", bVisible);
                m_eWorks = (eWorks)tree.Set(m_eWorks, m_eWorks, "Boat", "Select Boat", bVisible);
            }

            public override string Run()
            {
                return m_module.RunUnloadBoat(m_eVision, m_eWorks); 
            }
        }

        public class Run_AvoidX : ModuleRunBase
        {
            Loader0 m_module;
            public Run_AvoidX(Loader0 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public double m_fPos = 0;
            public override ModuleRunBase Clone()
            {
                Run_AvoidX run = new Run_AvoidX(m_module);
                run.m_fPos = m_fPos;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_fPos = tree.Set(m_fPos, m_fPos, "Position", "Axis X Avoid Position", bVisible);
            }

            public override string Run()
            {
                return m_module.RunAvoidX(m_fPos);
            }
        }

        public class Run_PickerSet : ModuleRunBase
        {
            Loader0 m_module;
            public Run_PickerSet(Loader0 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_PickerSet run = new Run_PickerSet(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunPickerSet();
            }
        }
        #endregion
    }
}
