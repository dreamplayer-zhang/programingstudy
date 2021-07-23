using Root_Pine2.Engineer;
using RootTools;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_Pine2.Module
{
    public class MagazineEV : ModuleBase
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            m_conveyor.GetTools(m_toolBox, this, bInit);
            m_elevator.GetTools(m_toolBox, this, bInit);
        }
        #endregion

        #region LED Display
        int m_nComm = 0; 
        int m_nUnitLED = 0; 
        string _sLED = "LED";
        public string p_sLED
        {
            get { return _sLED; }
            set
            {
                if (_sLED == value) return;
                _sLED = value;
                OnPropertyChanged(); 
                m_pine2.m_display.Write(m_nComm, m_nUnitLED, value); 
            }
        }

        void RunTreeLED(Tree tree)
        {
            m_nComm = tree.Set(m_nComm, m_nComm, "Comm", "Communication ID (0, 1)"); 
            m_nUnitLED = tree.Set(m_nUnitLED, m_nUnitLED, "Unit", "LED Display Modbus Unit ID");
        }
        #endregion

        #region Conveyor
        public class Conveyor
        {
            public enum eMove
            {
                Backward,
                Forward
            }
            public enum eCheck
            {
                Unload,
                Mid,
                Inside
            }
            DIO_IO m_dioSwitch;
            DIO_Os m_doMove;
            DIO_Is m_diCheck;
            public void GetTools(ToolBox toolBox, ModuleBase module, bool bInit)
            {
                toolBox.GetDIO(ref m_dioSwitch, module, "Switch", false);
                toolBox.GetDIO(ref m_doMove, module, "Move", Enum.GetNames(typeof(eMove)));
                toolBox.GetDIO(ref m_diCheck, module, "Check", Enum.GetNames(typeof(eCheck)));
                if (bInit) m_doMove.AllOff();
            }

            public bool m_bInv = false; 
            public void RunSwitch(int nBlink)
            {
                switch (m_magazineEV.p_eState)
                {
                    case eState.Run: m_dioSwitch.Write(nBlink % 2 == 0); break;
                    default: m_dioSwitch.Write(m_bInv ? (nBlink > 1) : (nBlink <= 1)); break;
                }
            }

            StopWatch m_swSwitch = new StopWatch(); 
            public bool CheckSwitch()
            {
                if (m_dioSwitch.p_bIn) return (m_swSwitch.ElapsedMilliseconds > 100);
                m_swSwitch.Restart();
                return false; 
            }

            public void RunMove(eMove eMove)
            {
                m_doMove.AllOff(); 
                Thread.Sleep(100);
                m_doMove.Write(eMove);
            }

            public void RunMoveStop()
            {
                m_doMove.AllOff(); 
            }

            public bool IsCheck(eCheck eCheck)
            {
                return m_diCheck.ReadDI((int)eCheck); 
            }

            public bool CheckExist()
            {
                for (int n = 0; n < 3; n++)
                {
                    if (m_diCheck.ReadDI(n)) return true; 
                }
                return false; 
            }

            public string WaitUnload(eCheck eCheck)
            {
                while (m_diCheck.ReadDI(eCheck) == false)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop"; 
                }
                while (m_diCheck.ReadDI(eCheck))
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                }
                return "OK"; 
            }

            MagazineEV m_magazineEV; 
            public Conveyor(MagazineEV magazineEV)
            {
                m_magazineEV = magazineEV; 
            }
        }
        public Conveyor m_conveyor;
        #endregion

        #region Elevator X Offset
        double[,] m_xOffset = new double[2, 2] { { 0, 0 }, { 0, 0 } };
        public double CalcXOffset(InfoStrip infoStrip)
        {
            int iPos = (int)infoStrip.p_eMagazinePos;
            int iSlot = infoStrip.p_iStrip; 
            return (iSlot * m_xOffset[iPos, 1] + (19 - iSlot) * m_xOffset[iPos, 0]) / 19; 
        }

        public double CalcXOffset()
        {
            int iPos = (m_elevator.m_ePosTransfer == Elevator.ePos.TransferDown) ? 1 : 0;
            int iSlot = m_elevator.m_iSlotTransfer;
            return (iSlot * m_xOffset[iPos, 1] + (19 - iSlot) * m_xOffset[iPos, 0]) / 19;
        }

        void RunTreeXOffset(Tree tree)
        {
            m_xOffset[0, 1] = tree.Set(m_xOffset[0, 1], m_xOffset[0, 1], "Up 19", "Transfer X Offset (pulse)");
            m_xOffset[1, 0] = tree.Set(m_xOffset[1, 0], m_xOffset[1, 0], "Down 0", "Transfer X Offset (pulse)");
            m_xOffset[1, 1] = tree.Set(m_xOffset[1, 1], m_xOffset[1, 1], "Down 19", "Transfer X Offset (pulse)");
        }
        #endregion

        #region Elevator
        public class Elevator
        {
            Axis m_axis;
            DIO_Os m_doAlign;
            DIO_Is m_diAlign;
            DIO_Is m_diProduct;
            public DIO_I m_diProtrude;
            public void GetTools(ToolBox toolBox, ModuleBase module, bool bInit)
            {
                toolBox.GetAxis(ref m_axis, module, "Elevator");
                toolBox.GetDIO(ref m_doAlign, module, "Align", new string[2] { "Backward", "Forward" });
                toolBox.GetDIO(ref m_diAlign, module, "Align", new string[4] { "Backward", "95", "74, 77", "Check" });
                toolBox.GetDIO(ref m_diProduct, module, "Product", Enum.GetNames(typeof(InfoStrip.eMagazinePos)));
                toolBox.GetDIO(ref m_diProtrude, module, "Protrude");
                if (bInit)
                {
                    InitPos();
                    InitALID(module);
                }
            }

            ALID m_alidProtrude; 
            void InitALID(ModuleBase module)
            {
                m_alidProtrude = module.m_gaf.GetALID(module, "Protrude", "Check Strip Protrude");
                m_alidProtrude.p_bEQError = false; 
            }

            #region Elevator
            public enum ePos
            {
                ConveyorUp,
                ConveyorDown,
                TransferUp,
                TransferDown,
                Stack
            }
            void InitPos()
            {
                m_axis.AddPos(Enum.GetNames(typeof(ePos)));
            }

            bool IsProtrude()
            {
                if (m_diProtrude.p_bIn == false) return false;
                m_alidProtrude.p_bSet = true;
                return true; 
            }

            string MoveElevator(Enum ePos, double fOffset = 0)
            {
                if (IsProtrude()) return "Strip Protrude";
                //string sRun = m_handler.m_transfer.IsPusherOff(); 
                //if (sRun != "OK") return sRun;
                if (m_bProduct[InfoStrip.eMagazinePos.Down])
                {
                    double fPos = m_axis.GetPosValue(ePos) + fOffset;
                    double fDown = m_axis.GetPosValue(Elevator.ePos.ConveyorDown) - 10000;
                    if ((m_axis.p_posCommand > fPos) && (fDown > fPos)) return "Can't Move Down cause Down Magazine";
                }
                m_axis.StartMove(ePos, fOffset);
                return "OK";
            }

            public string MoveToConveyor(InfoStrip.eMagazinePos eMagazinePos, double mmUp, bool bWait = true)
            {
                if (IsProtrude()) return "Strip Protrude";
                if (m_conveyor.IsCheck(Conveyor.eCheck.Inside)) return "Conveyer Inside Sensor Checked";
                m_infoStripPos = null;
                string sRun = MoveElevator((eMagazinePos == InfoStrip.eMagazinePos.Up) ? ePos.ConveyorUp : ePos.ConveyorDown, 1000 * mmUp);
                if (sRun != "OK") return sRun;
                if (bWait == false) return "OK";
                return m_axis.WaitReady();
            }

            public string MoveStack(bool bWait = true)
            {
                if (IsProtrude()) return "Strip Protrude";
                if (m_conveyor.IsCheck(Conveyor.eCheck.Inside)) return "Conveyer Inside Sensor Checked";
                m_infoStripPos = null;
                string sRun = MoveElevator(ePos.Stack);
                if (sRun != "OK") return sRun;
                if (bWait == false) return "OK";
                return m_axis.WaitReady();
            }

            double m_dSlot = 6000;
            public ePos m_ePosTransfer = ePos.TransferUp;
            public int m_iSlotTransfer = 0;
            public string MoveToTransfer(InfoStrip infoStrip)
            {
                if (IsProtrude()) return "Strip Protrude";
                if (m_conveyor.IsCheck(Conveyor.eCheck.Inside)) return "Conveyer Inside Sensor Checked";
                m_infoStripPos = null;
                m_ePosTransfer = (infoStrip.p_eMagazinePos == InfoStrip.eMagazinePos.Up) ? ePos.TransferUp : ePos.TransferDown;
                m_iSlotTransfer = infoStrip.p_iStrip; 
                string sRun = MoveElevator(m_ePosTransfer, -m_dSlot * m_iSlotTransfer);
                if (sRun != "OK") return sRun;
                string sMove = m_axis.WaitReady();
                if (sMove == "OK") m_infoStripPos = infoStrip;
                return sMove;
            }

            InfoStrip m_infoStripPos = null;
            public bool IsSamePos(InfoStrip infoStrip)
            {
                if (m_infoStripPos == null) return false;
                if (m_infoStripPos.p_iStrip != infoStrip.p_iStrip) return false;
                if (m_infoStripPos.p_eMagazinePos != infoStrip.p_eMagazinePos) return false;
                return true;
            }

            public bool IsMagazineUp()
            {
                return (m_axis.p_posCommand > m_axis.GetPosValue(ePos.Stack)); 
            }
            #endregion

            #region Align
            double m_secAlign = 5;
            public string RunAlign(bool bAlign)
            {
                m_doAlign.Write("Backward", !bAlign);
                m_doAlign.Write("Forward", bAlign);
                StopWatch sw = new StopWatch();
                int msAlign = (int)(1000 * m_secAlign);
                while (sw.ElapsedMilliseconds < msAlign)
                {
                    Thread.Sleep(10);
                    if (bAlign && IsAlignOn()) return "OK";
                    if (!bAlign && IsAlignOff()) return "OK";
                }
                return "Align Timeout";
            }

            bool IsAlignOn()
            {
                if (m_diAlign.ReadDI("Backward")) return false;
                if (m_diAlign.ReadDI("Check") == false) return false;
                return m_diAlign.ReadDI("95") || m_diAlign.ReadDI("74, 77");
            }

            bool IsAlignOff()
            {
                if (m_diAlign.ReadDI("95")) return false;
                if (m_diAlign.ReadDI("74, 77")) return false;
                if (m_diAlign.ReadDI("Check")) return false;
                return m_diAlign.ReadDI("Backward");
            }
            #endregion

            #region Product & Protrude
            public string WaitProduct(InfoStrip.eMagazinePos eMagazinePos)
            {
                while (true)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                    if (m_bProduct[eMagazinePos]) return "OK";
                }
            }

            public Dictionary<InfoStrip.eMagazinePos, bool> m_bProduct = new Dictionary<InfoStrip.eMagazinePos, bool>();
            public void ThreadCheck()
            {
                m_bProduct[InfoStrip.eMagazinePos.Up] = m_diProduct.ReadDI(InfoStrip.eMagazinePos.Up);
                m_bProduct[InfoStrip.eMagazinePos.Down] = m_diProduct.ReadDI(InfoStrip.eMagazinePos.Down);
            }
            #endregion

            public void RunTree(Tree tree)
            {
                m_dSlot = tree.Set(m_dSlot, m_dSlot, "Slot Interval", "Magazine Slot Interval (pulse)");
                m_secAlign = tree.Set(m_secAlign, m_secAlign, "Align Timeout", "Align Timeout (sec)");
            }

            Conveyor m_conveyor;
            Pine2_Handler m_handler;
            public Elevator(Conveyor conveyor, Pine2_Handler handler)
            {
                m_conveyor = conveyor;
                m_handler = handler; 
                m_bProduct.Add(InfoStrip.eMagazinePos.Up, false);
                m_bProduct.Add(InfoStrip.eMagazinePos.Down, false);
            }
        }
        public Elevator m_elevator;
        #endregion

        #region Stack
        public class Stack : NotifyProperty
        {
            InfoStrip.eResult _eResult = InfoStrip.eResult.Init;
            public InfoStrip.eResult p_eResult
            {
                get { return _eResult; }
                set
                {
                    if (_eResult == value) return;
                    _eResult = value;
                    OnPropertyChanged();
                    WriteResultLED();
                }
            }

            public void WriteResultLED()
            {
                switch (p_eResult)
                {
                    case InfoStrip.eResult.Init: m_magazineEV.p_sLED = "INIT"; break;
                    case InfoStrip.eResult.GOOD: m_magazineEV.p_sLED = "GOOD"; break;
                    case InfoStrip.eResult.DEF: m_magazineEV.p_sLED = "DEF "; break;
                    case InfoStrip.eResult.POS: m_magazineEV.p_sLED = "POS "; break;
                    case InfoStrip.eResult.BCD: m_magazineEV.p_sLED = "BCD "; break;
                    case InfoStrip.eResult.Paper: m_magazineEV.p_sLED = "PAPR"; break;
                }
            }

            int _nStack = 0;
            public int p_nStack
            {
                get { return _nStack; }
                set
                {
                    _nStack = value;
                    OnPropertyChanged();
                }
            }

            int p_lStack 
            { 
                get 
                {
                    if (p_eResult == InfoStrip.eResult.Paper) return m_magazineEV.m_pine2.p_lStackPaper; 
                    return m_magazineEV.m_pine2.p_lStack; 
                } 
            }

            int _iBundle = 0;
            public int p_iBundle
            {
                get { return _iBundle; }
                set
                {
                    _iBundle = value;
                    OnPropertyChanged();
                }
            }

            public void PutInfoStrip()
            {
                p_nStack++;
                if (p_nStack >= p_lStack) m_magazineEV.StartUnload(); 
            }

            MagazineEV m_magazineEV; 
            public Stack(MagazineEV magazineEV)
            {
                m_magazineEV = magazineEV;
            }
        }
        public Stack m_stack = null;
        #endregion

        #region Magazine
        public class Magazine : NotifyProperty
        {
            public List<InfoStrip> m_aStripRun = new List<InfoStrip>();
            private void InfoStrip_OnDispose(InfoStrip infoStrip)
            {
                m_aStripRun.Remove(infoStrip); 
            }

            public InfoStrip GetInfoStrip(bool bPeek)
            {
                if (m_qStripReady.Count == 0) return null;
                if (bPeek) return m_qStripReady.Peek(); 
                InfoStrip infoStrip = m_qStripReady.Dequeue();
                m_aStripRun.Add(infoStrip);
                return infoStrip; 
            }

            public int m_nStripCount = 0; 
            public void PutInfoStrip(InfoStrip infoStrip)
            {
                if (infoStrip != null) m_nStripCount++; 
                m_aStripRun.Remove(infoStrip);
                m_magazineEV.CheckMagazineDone(); 
            }

            public bool IsDone()
            {
                if (m_qStripReady.Count > 0) return false;
                if (m_aStripRun.Count > 0) return false;
                return true; 
            }

            int _iBundle = 0;
            public int p_iBundle
            {
                get { return _iBundle; }
                set
                {
                    _iBundle = value;
                    OnPropertyChanged();
                }
            }

            MagazineEV m_magazineEV; 
            public Queue<InfoStrip> m_qStripReady = new Queue<InfoStrip>(); 
            public Magazine(MagazineEV magazineEV, InfoStrip.eMagazinePos eMagazinePos, int iBundle)
            {
                p_iBundle = iBundle; 
                m_magazineEV = magazineEV; 
                for (int n = 0; n < 20; n++)
                {
                    InfoStrip infoStrip = new InfoStrip(magazineEV.p_eMagazine, eMagazinePos, iBundle, n);
                    infoStrip.OnDispose += InfoStrip_OnDispose;
                    m_qStripReady.Enqueue(infoStrip); 
                }
            }
        }
        public Dictionary<InfoStrip.eMagazinePos, Magazine> m_aMagazine = new Dictionary<InfoStrip.eMagazinePos, Magazine>(); 
        void InitMagazine()
        {
            m_aMagazine.Add(InfoStrip.eMagazinePos.Up, null);
            m_aMagazine.Add(InfoStrip.eMagazinePos.Down, null); 
        }
        #endregion

        #region InfoStrip
        public InfoStrip GetInfoStrip(bool bPeek)
        {
            switch (m_pine2.p_eMode)
            {
                case Pine2.eRunMode.Stack: return null;
                case Pine2.eRunMode.Magazine:
                    InfoStrip infoStrip = GetInfoStrip(m_aMagazine[InfoStrip.eMagazinePos.Down], bPeek);
                    if (infoStrip != null) return infoStrip; 
                    return GetInfoStrip(m_aMagazine[InfoStrip.eMagazinePos.Up], bPeek);
            }
            return null; 
        }

        InfoStrip GetInfoStrip(Magazine magazine, bool bPeek)
        {
            if (magazine == null) return null;
            return magazine.GetInfoStrip(bPeek); 
        }

        public void PutInfoStrip(InfoStrip infoStrip)
        {
            switch (m_pine2.p_eMode)
            {
                case Pine2.eRunMode.Stack: m_stack?.PutInfoStrip(); break;
                case Pine2.eRunMode.Magazine: m_aMagazine[infoStrip.p_eMagazinePos]?.PutInfoStrip(infoStrip); break;
            }
        }

        public void CheckMagazineDone()
        {
            if ((m_aMagazine[InfoStrip.eMagazinePos.Down] != null) && m_aMagazine[InfoStrip.eMagazinePos.Down].IsDone()) StartUnload();
            if ((m_aMagazine[InfoStrip.eMagazinePos.Up] != null) && m_aMagazine[InfoStrip.eMagazinePos.Up].IsDone()) StartUnload();
        }

        public bool IsMagazineBusy()
        {
            if ((m_aMagazine[InfoStrip.eMagazinePos.Down] != null) && (m_aMagazine[InfoStrip.eMagazinePos.Down].IsDone() == false)) return true;
            if ((m_aMagazine[InfoStrip.eMagazinePos.Up] != null) && (m_aMagazine[InfoStrip.eMagazinePos.Up].IsDone() == false)) return true;
            return false; 
        }
        #endregion

        #region override
        public override string StateReady()
        {
            switch (m_pine2.p_eMode)
            {
                case Pine2.eRunMode.Stack: 
                    if (m_stack == null)
                    {
                        return CheckStartLoad() ? StartLoad() : "OK"; 
                    }
                    return "OK"; 
                case Pine2.eRunMode.Magazine: 
                    if (m_aMagazine[InfoStrip.eMagazinePos.Up] == null)
                    {
                        return CheckStartLoad() ? StartLoad() : "OK";
                    }
                    if (m_aMagazine[InfoStrip.eMagazinePos.Down] == null)
                    {
                        return CheckStartLoad() ? StartLoad() : "OK";
                    }
                    return "OK"; 
            }
            return "OK";
        }

        bool CheckStartLoad()
        {
            return m_conveyor.CheckExist() && m_conveyor.CheckSwitch(); 
        }

        public override void Reset()
        {
            if (m_elevator.m_bProduct[InfoStrip.eMagazinePos.Down])
            {
                m_aMagazine[InfoStrip.eMagazinePos.Down] = null;
            }
            if (m_elevator.m_bProduct[InfoStrip.eMagazinePos.Up])
            {
                m_aMagazine[InfoStrip.eMagazinePos.Up] = null;
                m_stack = null; 
            }
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
            if ((m_aMagazine[InfoStrip.eMagazinePos.Down] != null) || m_elevator.m_bProduct[InfoStrip.eMagazinePos.Down]) return "Remove Down Magazine";
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            p_sLED = "MGZ" + ((int)p_eMagazine).ToString();
            //if (m_elevator.m_bProduct[InfoStrip.eMagazinePos.Down]) StartUnload();
            //if (m_elevator.m_bProduct[InfoStrip.eMagazinePos.Up]) StartUnload();
            return p_sInfo;
        }
        #endregion

        #region Load
        string StartLoad()
        {
            p_sLED = "LOAD";
            return StartRun(m_runLoad);
        }

        public string RunLoad()
        {
            switch (m_pine2.p_eMode)
            {
                case Pine2.eRunMode.Magazine:
                    InfoStrip.eMagazinePos pos = InfoStrip.eMagazinePos.Up;
                    if (m_aMagazine[pos] == null)
                    {
                        if (Run(RunLoad(pos))) return p_sInfo;
                        m_aMagazine[pos] = new Magazine(this, pos, m_pine2.p_iBundle++);
                        return "OK";
                    }
                    pos = InfoStrip.eMagazinePos.Down;
                    if (m_aMagazine[pos] == null)
                    {
                        if (Run(RunLoad(pos))) return p_sInfo;
                        m_aMagazine[pos] = new Magazine(this, pos, m_pine2.p_iBundle++);
                        return "OK";
                    }
                    break; 
                case Pine2.eRunMode.Stack:
                    if (Run(RunLoad(InfoStrip.eMagazinePos.Up))) return p_sInfo;
                    if (Run(m_elevator.MoveStack())) return p_sInfo;
                    m_stack = new Stack(this);
                    break;
            }
            return "OK";
        }

        double m_secProductDelay = 0.2; 
        string RunLoad(InfoStrip.eMagazinePos eMagazinePos)
        {
            try
            {
                if (m_elevator.m_bProduct[eMagazinePos]) return "Magazine Product Sensor Checked";
                if (Run(m_elevator.MoveToConveyor(eMagazinePos, (m_pine2.p_eMode == Pine2.eRunMode.Magazine) ? 0 : -7))) return p_sInfo;
                if (m_conveyor.CheckExist() == false) return "Conveyer Sensor not Detected";
                if (Run(m_elevator.RunAlign(false))) return p_sInfo;
                m_conveyor.RunMove(Conveyor.eMove.Forward);
                if (Run(m_elevator.WaitProduct(eMagazinePos))) return p_sInfo;
                Thread.Sleep((int)m_secProductDelay);
                m_conveyor.RunMoveStop();
                if (Run(m_elevator.MoveToConveyor(eMagazinePos, (m_pine2.p_eMode == Pine2.eRunMode.Magazine) ? 7 : 0))) return p_sInfo;
                if (Run(m_elevator.RunAlign(true))) return p_sInfo;
                m_magazineSet.MagazineLoaded(p_eMagazine, eMagazinePos); 
            }
            finally
            {
                m_conveyor.RunMoveStop();
            }
            return "OK";
        }
        #endregion

        #region Unload
        public void StartFinish()
        {
            if (m_elevator.m_bProduct[InfoStrip.eMagazinePos.Down]) StartRun(m_runUnload);
            if (m_elevator.m_bProduct[InfoStrip.eMagazinePos.Up]) StartRun(m_runUnload);
        }

        public string StartUnload()
        {
            p_sLED = "DONE"; 
            return StartRun(m_runUnload);
        }

        public string RunUnload()
        {
            string sRun; 
            Thread.Sleep(100); 
            switch (m_pine2.p_eMode)
            {
                case Pine2.eRunMode.Magazine:
                    InfoStrip.eMagazinePos ePos = InfoStrip.eMagazinePos.Down;
                    if ((m_aMagazine[ePos] != null) || m_elevator.m_bProduct[ePos])
                    {
                        sRun = RunUnload(ePos);
                        if (m_aMagazine[ePos] != null)
                        {
                            int nStrip = m_aMagazine[ePos].m_nStripCount; 
                            m_pine2.m_printer.AddPrint((int)p_eMagazine, m_aMagazine[ePos].p_iBundle, nStrip);
                        }
                        m_aMagazine[ePos] = null;
                        if (Run(m_elevator.RunAlign(true))) return p_sInfo;
                        return sRun;
                    }
                    ePos = InfoStrip.eMagazinePos.Up;
                    if ((m_aMagazine[ePos] != null) || m_elevator.m_bProduct[ePos])
                    {
                        sRun = RunUnload(ePos); 
                        if (m_aMagazine[ePos] != null)
                        {
                            int nStrip = m_aMagazine[ePos].m_nStripCount; 
                            m_pine2.m_printer.AddPrint((int)p_eMagazine, m_aMagazine[ePos].p_iBundle, nStrip);
                        }
                        m_aMagazine[ePos] = null;
                        return sRun;
                    }
                    break;
                case Pine2.eRunMode.Stack:
                    sRun = RunUnload(InfoStrip.eMagazinePos.Up); 
                    if (m_stack != null)
                    {
                        int nStrip = m_stack.p_nStack; 
                        if (nStrip > 0) m_pine2.m_printer.AddPrint((int)p_eMagazine, m_stack.p_iBundle, nStrip, m_stack.p_eResult); 
                        m_stack.WriteResultLED();
                    }
                    m_stack = null;
                    return sRun;
            }
            return "OK"; 
        }

        double m_secUnload = 4; 
        string RunUnload(InfoStrip.eMagazinePos eMagazinePos)
        {
            try
            {
                while (m_conveyor.CheckExist())
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                }
                m_conveyor.m_bInv = false;
                if (Run(m_elevator.RunAlign(true))) return p_sInfo;
                if (Run(m_elevator.MoveToConveyor(eMagazinePos, (m_pine2.p_eMode == Pine2.eRunMode.Magazine) ? 7 : 0))) return p_sInfo;
                if (Run(m_elevator.RunAlign(false))) return p_sInfo;
                if (Run(m_elevator.MoveToConveyor(eMagazinePos, (m_pine2.p_eMode == Pine2.eRunMode.Magazine) ? 0 : -7))) return p_sInfo;
                m_conveyor.RunMove(Conveyor.eMove.Backward);
                Thread.Sleep(1000);
                switch (m_pine2.p_eMode)
                {
                    case Pine2.eRunMode.Magazine:
                        if (Run(m_conveyor.WaitUnload(Conveyor.eCheck.Mid))) return p_sInfo;
                        Thread.Sleep(250);
                        m_conveyor.RunMoveStop();
                        m_magazineSet.MagazineUnloaded(p_eMagazine, eMagazinePos);
                        break;
                    case Pine2.eRunMode.Stack:
                        if (Run(m_conveyor.WaitUnload(Conveyor.eCheck.Inside))) return p_sInfo;
                        Thread.Sleep((int)(1000 * m_secUnload));
                        m_conveyor.RunMoveStop();
                        break; 
                }
            }
            finally
            {
                m_conveyor.RunMoveStop();
            }
            return "OK";
        }
        #endregion

        #region Move Transfer
        public string StartMoveTransfer(InfoStrip infoStrip)
        {
            if (m_elevator.IsSamePos(infoStrip)) return "OK";
            Run_MoveTransfer run = (Run_MoveTransfer)m_runMoveTransfer.Clone();
            run.m_infoStrip = infoStrip;
            return StartRun(run); 
        }

        public string RunMoveTransfer(InfoStrip infoStrip)
        {
            if (infoStrip == null) return "InfoStrip not Found";
            p_sLED = infoStrip.m_sLED; 
            return m_elevator.MoveToTransfer(infoStrip);
        }
        #endregion

        #region Check Thread
        Thread m_threadCheck;
        bool m_bThreadCheck = false; 
        void InitThread()
        {
            m_threadCheck = new Thread(new ThreadStart(RunThreadCheck));
            m_threadCheck.Start();
        }

        void RunThreadCheck()
        {
            m_bThreadCheck = true;
            Thread.Sleep(5000);
            while (m_bThreadCheck)
            {
                Thread.Sleep(10);
                m_elevator.ThreadCheck(); 
            }
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeLED(tree.GetTree("LED")); 
            m_elevator.RunTree(tree.GetTree("Elevator"));
            m_secProductDelay = tree.GetTree("Conveyer").Set(m_secProductDelay, m_secProductDelay, "Product Delay", "Product Sensor -> Conveyer Stop (sec)");
            m_secUnload = tree.GetTree("Conveyer").Set(m_secUnload, m_secUnload, "Stack Unload", "Stack Unload Delay (sec)");
            RunTreeXOffset(tree.GetTree("X Offset")); 
        }
        #endregion

        InfoStrip.eMagazine p_eMagazine { get; set; }
        Pine2 m_pine2;
        MagazineEVSet m_magazineSet; 
        public MagazineEV(InfoStrip.eMagazine eMagazine, IEngineer engineer, Pine2 pine2, MagazineEVSet magazineSet)
        {
            m_conveyor = new Conveyor(this);
            m_elevator = new Elevator(m_conveyor, (Pine2_Handler)engineer.ClassHandler());
            InitMagazine(); 
            p_eMagazine = eMagazine;
            string sID = eMagazine.ToString();
            p_id = sID.Substring(0, sID.Length - 1) + (char)(eMagazine + '0'); 
            m_nUnitLED = (int)eMagazine + 1; 
            m_pine2 = pine2;
            m_magazineSet = magazineSet; 
            base.InitBase(p_id, engineer);
            InitThread(); 
        }

        public override void ThreadStop()
        {
            if (m_bThreadCheck)
            {
                m_bThreadCheck = false;
                m_threadCheck.Join();
            }
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runLoad;
        ModuleRunBase m_runUnload;
        ModuleRunBase m_runMoveTransfer; 
        protected override void InitModuleRuns()
        {
            m_runLoad = AddModuleRunList(new Run_Load(this), false, "Load Magazine or Stack");
            m_runUnload = AddModuleRunList(new Run_Unload(this), false, "Unload Magazine or Stack");
            AddModuleRunList(new Run_DisplayLED(this), false, "Run Disply LED");
            AddModuleRunList(new Run_Align(this), false, "Run Align");
            AddModuleRunList(new Run_MoveStack(this), false, "Move Stack Position");
            m_runMoveTransfer = AddModuleRunList(new Run_MoveTransfer(this), false, "Move Transfer Position");
        }

        public class Run_Load : ModuleRunBase
        {
            MagazineEV m_module;
            public Run_Load(MagazineEV module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Load run = new Run_Load(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunLoad();
            }
        }

        public class Run_Unload : ModuleRunBase
        {
            MagazineEV m_module;
            public Run_Unload(MagazineEV module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Unload run = new Run_Unload(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunUnload();
            }
        }

        public class Run_DisplayLED : ModuleRunBase
        {
            MagazineEV m_module;
            public Run_DisplayLED(MagazineEV module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            string m_sLED = "Test";
            public override ModuleRunBase Clone()
            {
                Run_DisplayLED run = new Run_DisplayLED(m_module);
                run.m_sLED = m_sLED;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sLED = tree.Set(m_sLED, m_sLED, "LED", "Display LED", bVisible);
            }

            public override string Run()
            {
                m_module.p_sLED = m_sLED;
                return "OK";
            }
        }

        public class Run_Align : ModuleRunBase
        {
            MagazineEV m_module;
            public Run_Align(MagazineEV module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bAlign = false; 
            public override ModuleRunBase Clone()
            {
                Run_Align run = new Run_Align(m_module);
                run.m_bAlign = m_bAlign; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bAlign = tree.Set(m_bAlign, m_bAlign, "Align", "Run Align", bVisible); 
            }

            public override string Run()
            {
                return m_module.m_elevator.RunAlign(m_bAlign); 
            }
        }

        public class Run_MoveStack : ModuleRunBase
        {
            MagazineEV m_module;
            public Run_MoveStack(MagazineEV module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_MoveStack run = new Run_MoveStack(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.m_elevator.MoveStack();
            }
        }

        public class Run_MoveTransfer : ModuleRunBase
        {
            MagazineEV m_module;
            public Run_MoveTransfer(MagazineEV module)
            {
                m_module = module;
                m_infoStrip = new InfoStrip(module.p_eMagazine, InfoStrip.eMagazinePos.Up, 0, 0); 
                InitModuleRun(module);
            }

            public InfoStrip m_infoStrip;
            public override ModuleRunBase Clone()
            {
                Run_MoveTransfer run = new Run_MoveTransfer(m_module);
                run.m_infoStrip = m_infoStrip.Clone();
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_infoStrip.RunTreeMagazine(tree, bVisible);
            }

            public override string Run()
            {
                return m_module.RunMoveTransfer(m_infoStrip);
            }
        }
        #endregion
    }
}
