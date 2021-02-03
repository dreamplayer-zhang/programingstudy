using Root_EFEM.Module;
using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_AOP01_Packing.Module
{
    public class TapePacker : ModuleBase, IWTRChild
    {

        public TapePacker(string id, IEngineer engineer)
        {
            InitChildSlot();
            m_cartridge = new Cartridge("Cartridge", this);
            m_roller = new Roller("Roller", this);
            m_head = new Head("Head", this);
            m_stage = new Stage("Stage", this);
            m_camera = new Camera("Camera", this);
            m_waferSize = new InfoWafer.WaferSize(id, false, false);
            base.InitBase(id, engineer);
            InitALID();
        }

        #region ToolBox
        public override void GetTools(bool bInit)
        {
            m_cartridge.GetTools(m_toolBox, bInit);
            m_roller.GetTools(m_toolBox, bInit);
            m_head.GetTools(m_toolBox, bInit);
            m_stage.GetTools(m_toolBox, bInit);
            m_camera.GetTools(m_toolBox, bInit);
        }
        #endregion

        #region ALID
        ALID m_alidTape;

        void InitALID()
        {
            m_alidTape = m_gaf.GetALID(this, "Taping Module", "Taping Module Error");
        }
        #endregion

        #region Camera
        public class Camera
        {
            Camera_Basler m_reticleVRS;
            Camera_Basler m_CheckRemainVRS;
            Camera_Basler m_CheckEndVRS;

            TapePacker m_packer;
            public Camera(string id, TapePacker packer)
            {
                m_packer = packer;
            }
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_packer.p_sInfo = toolBox.Get(ref m_reticleVRS, m_packer, "Check Reticle");
                m_packer.p_sInfo = toolBox.Get(ref m_CheckRemainVRS, m_packer, "Check Remain Tape");
                m_packer.p_sInfo = toolBox.Get(ref m_CheckEndVRS, m_packer, "Check End Tape");
            }
            public string RunCheckTapeVRS()
            {
                if (m_CheckRemainVRS.Grab() == "OK")
                {
                    //검사?
                }
                return "OK";
            }
            public string RunCheckEndVRS()
            {
                if (m_CheckEndVRS.Grab() == "OK")
                {
                    //검사?
                }
                return "OK";
            }
            public string RunReticleVRS()
            {
                if (m_reticleVRS.Grab() == "OK")
                {
                    //검사?
                }
                return "OK";
            }
        }
        public Camera m_camera;
        #endregion

        #region Cartridge
        public class Cartridge
        {
            Axis m_axis;  // Axis 2
            DIO_O m_doCutter; // Y17 (false:ON / true:OFF)
            DIO_I[] m_diCheckCutter = new DIO_I[2]; // X20:ON / X21:OFF
            DIO_I2O2 m_solLock; // Y19:unlock / Y20:lock / X22:lock / X23:unlock
            DIO_I[] m_diCheckPlaced = new DIO_I[2]; // X26:placed left / X27:placed right
            DIO_I2O2 m_solTapeStoper; // Y21:stop / Y22:release / X24:up / X25:down

            string m_id;
            TapePacker m_packer;
            public Cartridge(string id, TapePacker packer)
            {
                m_id = id;
                m_packer = packer;
            }

            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_packer.p_sInfo = toolBox.Get(ref m_axis, m_packer, m_id);
                m_packer.p_sInfo = toolBox.Get(ref m_doCutter, m_packer, m_id + ".Cutter Off");
                m_packer.p_sInfo = toolBox.Get(ref m_diCheckCutter[0], m_packer, m_id + ".Cutter ON");
                m_packer.p_sInfo = toolBox.Get(ref m_diCheckCutter[1], m_packer, m_id + ".Cutter OFF");
                m_packer.p_sInfo = toolBox.Get(ref m_solLock, m_packer, m_id + ".Lock", "Unlock", "Lock");
                m_packer.p_sInfo = toolBox.Get(ref m_diCheckPlaced[0], m_packer, m_id + ".Placed Left");
                m_packer.p_sInfo = toolBox.Get(ref m_diCheckPlaced[1], m_packer, m_id + ".Placed Right");
                m_packer.p_sInfo = toolBox.Get(ref m_solTapeStoper, m_packer, m_id + ".Stopper", "Stop", "Release");

                if (bInit)
                {
                    m_packer.InitSolvalve(m_solLock);
                    m_packer.InitSolvalve(m_solTapeStoper);
                    InitPos();
                }
            }

            public string RunMove(ePos ePos)
            {
                m_axis.StartMove(ePos);
                return m_axis.WaitReady();
            }
            public string RunCutter()
            {
                m_doCutter.Write(false);
                Thread.Sleep(500);
                if (!m_diCheckCutter[0].p_bIn)
                    return "Cutter doesn't work";
                m_doCutter.Write(true);
                Thread.Sleep(500);
                if (!m_diCheckCutter[1].p_bIn)
                    return "Cutter doesn't work";

                return "OK";
            }
            public string RunLock(bool bLock)
            {
                if (bLock && IsCheck() == false) return "Cartridge Not Placed";
                return m_solLock.RunSol(bLock);
            }
            public string RunStopper(bool bStop)
            {
                //true면 지금 release인데..
                return m_solTapeStoper.RunSol(!bStop);
            }
            public bool IsCheck()
            {
                return m_diCheckPlaced[0].p_bIn && m_diCheckPlaced[1].p_bIn;
            }
            void InitPos()
            {
                m_axis.AddPos(Enum.GetNames(typeof(ePos)));
            }
            public enum ePos
            {
                Ready,
                CheckTape,
                Attach,
                Taping,
                Cutting,
                Run
            }

        }
        public Cartridge m_cartridge;
        #endregion

        #region Roller
        public class Roller
        {
            DIO_I2O2 m_solRollerUp; // Y25:up / Y26:down / X30:up / X31:down
            DIO_I2O2 m_solRollerPush; // Y23:push / Y24:pull / X28:push / X29:down

            string m_id;
            TapePacker m_packer;
            public Roller(string id, TapePacker packer)
            {
                m_id = id;
                m_packer = packer;
            }

            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_packer.p_sInfo = toolBox.Get(ref m_solRollerUp, m_packer, m_id + ".UpDown", "Down", "Up");
                m_packer.p_sInfo = toolBox.Get(ref m_solRollerPush, m_packer, m_id + ".Push", "Back", "Push");
                if (bInit)
                {
                    m_packer.InitSolvalve(m_solRollerPush);
                    m_packer.InitSolvalve(m_solRollerUp);
                }
            }
            public string RunRollerPushUp(bool bPush)
            {
                if (m_packer.Run(m_solRollerPush.RunSol(false)))
                    return m_packer.p_sInfo;
                if (bPush)
                {
                    if (m_packer.Run(m_solRollerUp.RunSol(true)))
                        return m_packer.p_sInfo;

                    return m_solRollerPush.RunSol(true);
                }
                else
                {
                    return m_solRollerUp.RunSol(false);
                }
            }
        }
        public Roller m_roller;
        #endregion

        #region Head
        public class Head
        {
            DIO_I2O2 m_solHead; // Y13:up / Y14:down / X13:up / X14:down
            DIO_I2O2 m_solPicker; // Y15:up / Y16:down / X15:up / X16:down
            DIO_IO m_dioVacuum; // Y66 (true:ON/false:OFF) / X96
            DIO_O m_doBlow; // Y67 (true:ON/false:OFF)
            DIO_I m_diOverload; // X17 // 케이스 틀어짐확인 센서
            DIO_I m_diTopCheck; // X18 (false:none / true:checked )

            string m_id;
            TapePacker m_packer;
            public Head(string id, TapePacker packer)
            {
                m_id = id;
                m_packer = packer;
            }

            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_packer.p_sInfo = toolBox.Get(ref m_solHead, m_packer, m_id + ".Head", "Up", "Down");
                m_packer.p_sInfo = toolBox.Get(ref m_solPicker, m_packer, m_id + ".Picker", "Up", "Down");
                m_packer.p_sInfo = toolBox.Get(ref m_diOverload, m_packer, m_id + ".Overload");
                m_packer.p_sInfo = toolBox.Get(ref m_diTopCheck, m_packer, m_id + ".Check Case Top");
                m_packer.p_sInfo = toolBox.Get(ref m_dioVacuum, m_packer, m_id + ".Vacuum");
                m_packer.p_sInfo = toolBox.Get(ref m_doBlow, m_packer, m_id + ".Blow");
                if (bInit)
                {
                    m_packer.InitSolvalve(m_solHead);
                    m_packer.InitSolvalve(m_solPicker);
                }
            }

            double m_secVac = 0.5;
            double m_secBlow = 0.5;
            public void RunTree(Tree tree)
            {
                m_secVac = tree.Set(m_secVac, m_secVac, "Vacuum", "Vacuum On Wait (sec)");
                m_secBlow = tree.Set(m_secBlow, m_secBlow, "Blow", "Vacuum Off Blow Time (sec)");
            }

            public string RunSol(bool bHeadDown, bool bPickerDown)
            {
                m_solPicker.Write(bPickerDown);
                string sRun = RunHeadDown(bHeadDown);
                if (sRun != "OK")
                {
                    m_solPicker.Write(false);
                    RunHeadDown(false);
                    return sRun;
                }
                return m_solPicker.WaitDone();
            }
            public string RunHeadDown(bool bDown)
            {
                if (bDown == false)
                    return m_solHead.RunSol(bDown);
                m_solHead.Write(bDown);
                Thread.Sleep(100);
                int msWait = (int)(1000 * m_solHead.m_secTimeout);
                while (m_solHead.p_bDone != true)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return m_id + " EQ Stop";
                    if (m_solHead.m_swWrite.ElapsedMilliseconds > msWait) return m_solHead.m_id + " Solvalve Move Timeout";
                }
                if (m_diOverload.p_bIn)
                {
                    m_solHead.Write(false);
                    return "Overload Sensor Checked";
                }
                return "OK";
            }
            public string RunVacuum(bool bOn)
            {
                m_dioVacuum.Write(bOn);
                if (bOn == false)
                {
                    m_doBlow.Write(true);
                    Thread.Sleep((int)(500 * m_secBlow));
                    m_doBlow.Write(false);
                    return "OK";
                }
                int msVac = (int)(1000 * m_secVac);
                while (m_dioVacuum.p_bIn != bOn)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop())
                        return m_id + " EQ Stop";
                    if (m_dioVacuum.m_swWrite.ElapsedMilliseconds > msVac)
                        return "Vacuum Sensor Timeout";
                }
                return "OK";
            }
        }
        public Head m_head;
        #endregion

        #region Stage
        public class Stage
        {
            public Axis m_axis; // Axis 0

            DIO_I m_diCheck; // X19 (false:none / true:checked)

            string m_id;
            TapePacker m_packer;
            public Stage(string id, TapePacker packer)
            {
                m_id = id;
                m_packer = packer;
            }

            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_packer.p_sInfo = toolBox.Get(ref m_axis, m_packer, m_id + ".Rotate");
                m_packer.p_sInfo = toolBox.Get(ref m_diCheck, m_packer, m_id + ".Check");
                if (bInit)
                {
                }
            }

            public bool IsCheck()
            {
                return m_diCheck.p_bIn;
            }

            const double c_fPpR = 2621440;
            public double m_degReady = 0;
            //public double m_degAttach = 13; 
            int m_nRound = 2;
            public double m_degCut = 10;
            double m_vDeg = 90;
            double m_secAcc = 0.7;
            public void RunTree(Tree tree)
            {
                m_nRound = tree.Set(m_nRound, m_nRound, "Round", "Taping Rotate Round");
                m_degReady = tree.Set(m_degReady, m_degReady, "Ready", "Ready Position (Deg)");
                //m_degAttach = tree.Set(m_degAttach, m_degAttach, "Attach", "Attach Position (Deg)");
                m_degCut = tree.Set(m_degCut, m_degCut, "Cut", "Cut Position (Deg)");
                m_vDeg = tree.Set(m_vDeg, m_vDeg, "Speed", "Taping Rotate Speed (Deg / sec)");
                m_secAcc = tree.Set(m_secAcc, m_secAcc, "Acc", "Taping Rotate Acceleration Time (sec)");
            }

            public string RunMove(double fDeg)
            {
                double fNow = m_axis.p_posCommand;
                double fPulse = fDeg * c_fPpR / 360;
                while ((fPulse - fNow) > c_fPpR)
                    fNow += c_fPpR;
                while ((fNow - fPulse) > c_fPpR)
                    fNow -= c_fPpR;
                if (Math.Abs(fPulse - fNow) > c_fPpR / 2)
                {
                    if (fPulse > fNow)
                        fNow += c_fPpR;
                    else
                        fNow -= c_fPpR;
                }
                m_axis.SetCommandPosition(fNow);
                m_axis.StartMove(fPulse);
                return m_axis.WaitReady();
            }
            public string RunTaping()
            {
                double fPulse = m_degCut * c_fPpR / 360;
                double vPulse = m_vDeg * c_fPpR / 360;
                m_axis.StartMove(fPulse + c_fPpR * m_nRound, vPulse, m_secAcc, m_secAcc);
                string sRun = m_axis.WaitReady();
                m_axis.SetCommandPosition(fPulse);
                return sRun;
            }
            public string RunRotate(double fDeg)
            {
                double fPulse = fDeg * c_fPpR / 360;
                double vPulse = m_vDeg * c_fPpR / 360;
                double fNow = m_axis.p_posCommand;
                m_axis.StartMove(fNow - fPulse, vPulse, m_secAcc, m_secAcc);
                return m_axis.WaitReady();
            }
            public string BeforeHome()
            {
                m_axis.ServoOn(true);
                Thread.Sleep(100);
                m_axis.Jog(0.3);
                Thread.Sleep(1200);
                m_axis.StopAxis();
                Thread.Sleep(100);
                return "OK";
            }
        }
        public Stage m_stage;
        #endregion

        #region Solvalue
        public List<DIO_I2O2> m_aSolvalve = new List<DIO_I2O2>();
        public void InitSolvalve(DIO_I2O2 sol)
        {
            if (sol == null)
                return;
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

        #region InfoWafer
        string m_sInfoWafer = "";
        InfoWafer _infoWafer = null;
        public InfoWafer p_infoWafer
        {
            get
            {
                return _infoWafer;
            }
            set
            {
                m_sInfoWafer = (value == null) ? "" : value.p_id;
                _infoWafer = value;
                if (m_reg != null)
                    m_reg.Write("sInfoWafer", m_sInfoWafer);
                OnPropertyChanged();
            }
        }

        Registry m_reg = null;
        public void ReadInfoWafer_Registry()
        {
            m_reg = new Registry(p_id + ".InfoWafer");
            m_sInfoWafer = m_reg.Read("sInfoWafer", m_sInfoWafer);
            p_eProcess = (eProcess)m_reg.Read("Process", (int)p_eProcess);
            p_infoWafer = m_engineer.ClassHandler().GetGemSlot(m_sInfoWafer);
        }
        #endregion

        #region IWTRChild
        bool _bLock = false;
        public bool p_bLock
        {
            get
            {
                return _bLock;
            }
            set
            {
                if (_bLock == value)
                    return;
                _bLock = value;
            }
        }

        bool IsLock()
        {
            for (int n = 0; n < 10; n++)
            {
                if (p_bLock == false)
                    return false;
                Thread.Sleep(100);
            }
            return true;
        }

        List<string> _asChildSlot = new List<string>();
        void InitChildSlot()
        {
            _asChildSlot.Add("Case");
            _asChildSlot.Add("Reticle");
        }
        public List<string> p_asChildSlot
        {
            get
            {
                return _asChildSlot;
            }
        }

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
            if (p_eState != eState.Ready)
                return p_id + " eState not Ready";
            //return "OK"; //0202 

            //0203 inforwafer 확인
            if (p_infoWafer == null)
                return p_id + " IsGetOK - InfoWafer not Exist";
            //이 아래는 맞는거 같음
            switch (p_eProcess)
            {
                case eProcess.Case:
                    if (nID == 0)
                        return "OK";
                    break;
                case eProcess.Reticle:
                    if (nID == 1)
                        return "OK";
                    break;
                case eProcess.Done:
                    if (nID == 0)
                        return "OK";
                    break;
            }
            return p_id + " IsGetOK - Process " + p_eProcess.ToString();
        }

        public string IsPutOK(InfoWafer infoWafer, int nID)
        {
            //0203 디버그 p_infowafer 확인
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            //return "OK"; // 0202
            if (p_infoWafer != null) return p_id + " IsPutOK - InfoWafer Exist";
            //p_eProcess 확인
            
            switch (p_eProcess)
            {
                case eProcess.Empty:
                    if (nID == 0) 
                        return "OK"; 
                    break;
                case eProcess.Opened: if (nID == 1) 
                        return "OK"; 
                    break;
            }
            return p_id + " IsPutOK - Process " + p_eProcess.ToString();
        }

        public int GetTeachWTR(InfoWafer infoWafer = null)
        {
            if (infoWafer == null)
                infoWafer = p_infoWafer;
            return m_waferSize.GetData(infoWafer.p_eSize).m_teachWTR;
        }

        public string BeforeGet(int nID)
        {
            //0203 nID로 Rotate 각도 조절할 수 있는지 확인
            
            //if (Run(m_stage.RunRotate(30))) return p_sInfo;
            if (p_infoWafer == null) return p_id + " BeforeGet : InfoWafer = null";
            return CheckGetPut();
        }

        public string BeforePut(int nID)
        {
            //nID따라서 Rotate 각도 조절할 수 있는지 확인

            //if (Run(m_stage.RunRotate(30))) return p_sInfo;
            if (p_infoWafer != null) return p_id + " BeforePut : InfoWafer != null";
            return CheckGetPut();
        }

        public string AfterGet(int nID)
        {
            //return "OK"; // 0202

            //0203 nID로 AfterGet 후 각도 동작하고
            //if (Run(m_stage.RunRotate(-30))) return p_sInfo;

            //여기도 맞는거 같음
            switch (p_eProcess)
            {
                case eProcess.Case: if (nID == 0) p_eProcess = eProcess.Empty; break;
                case eProcess.Done: if (nID == 0) p_eProcess = eProcess.Empty; break;
                case eProcess.Reticle: if (nID == 1) p_eProcess = eProcess.Opened; break;
            }
            return CheckGetPut();
        }

        public string AfterPut(int nID)
        {
            //0203 nID로 구분해서 p_eProcess 바꺼? //p_eProcess = eProcess.Case; or p_eProcess = eProcess.Reticle;

            //return "OK"; //0202 

            //0203 nID로 AfterGet 후 각도 동작
            //if (Run(m_stage.RunRotate(-30))) return p_sInfo;
            switch (p_eProcess)
            {
                case eProcess.Empty: if (nID == 0) p_eProcess = eProcess.Case; break;
                case eProcess.Opened: if (nID == 1) p_eProcess = eProcess.Reticle; break;
            }
            return "OK";
        }

        string CheckGetPut()
        {
            if (p_eState != eState.Ready)
                return p_id + " eState not Ready";
            return "OK";
        }

        public bool IsWaferExist(int nID)
        {       
            return (p_infoWafer != null);
        }

        InfoWafer.WaferSize m_waferSize;
        public void RunTreeTeach(Tree tree)
        {
            m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
        }
        #endregion

        #region Process State
        public enum eProcess
        {
            Empty, // 기본
            Case, // After Put
            Opening, // RunCoverOpen Start
            Opened, // RunCoverOpen Done
            Reticle, // After Put, Only Opened
            Closing, // RunCoverClose Start
            Closed, // RunCoverClose Done
            Taping, // RunTaping
            Done, // RungTaping Done
            Error
        }
        eProcess _eProcess = eProcess.Empty;
        public eProcess p_eProcess
        {
            get
            {
                return _eProcess;
            }
            set
            {
                if (_eProcess == value)
                    return;
                _eProcess = value;
                if (m_reg != null) m_reg.Write("Process", (int)value);
                OnPropertyChanged();
            }
        }
        #endregion

        #region Functions
        public string RunCoverOpen()
        {
            //0203  p_eProcess = eProcess.Case
            //p_eProcess = eProcess.Case; 0203 삭제
            if (p_eProcess != eProcess.Case) return "Process not Case : " + p_eProcess.ToString();
            p_eProcess = eProcess.Opening;
            if (Run(m_head.RunVacuum(false)))
                return p_sInfo;
            if (Run(m_head.RunSol(false, false)))
                return p_sInfo;
            if (Run(m_head.RunSol(true, true)))
                return p_sInfo;
            if (Run(m_head.RunVacuum(true)))
                return p_sInfo;
            if (Run(m_head.RunSol(false, true)))
                return p_sInfo;
            p_eProcess = eProcess.Opened;
            //p_eProcess = eProcess.Opened
            return "OK";
        }
        public string RunCoverClose()
        {
            //0203 p_eProcess = eProcess.Reticle
            //p_eProcess = eProcess.Reticle; 0203 삭제
            if (p_eProcess != eProcess.Reticle) return "Process not Reticle : " + p_eProcess.ToString();
            p_eProcess = eProcess.Closing;
            if (Run(m_head.RunSol(true, true)))
                return p_sInfo;
            if (Run(m_head.RunVacuum(false)))
                return p_sInfo;
            if (Run(m_head.RunSol(true, false)))
                return p_sInfo;
            p_eProcess = eProcess.Closed;
            //0203 p_eProcess = eProcess.Closed
            return "OK";
        }
        public string RunHeadUp()
        {
            if (Run(m_head.RunVacuum(false)))
                return p_sInfo;
            if (Run(m_head.RunSol(false, false)))
                return p_sInfo;
            return "OK";
        }
        
        public string RunTaping()
        {
            //Thread.Sleep(10);
            //0203 Closed 상태 면 테이핑 ㄱㄱ
            if (p_eProcess != eProcess.Closed) return "Process not Closed : " + p_eProcess.ToString();
            p_eProcess = eProcess.Taping;
            if (Run(m_head.RunSol(true, false))) return p_sInfo; // Overload는 여기서 체크
            if (Run(m_roller.RunRollerPushUp(true))) return p_sInfo;
            //여기서 Check Top Sensor 체크해야될듯 근데 안들어오네
            // 0203 Check Case Top Sensor 시퀀스 추가 필요

            if (Run(m_stage.RunMove(m_stage.m_degReady))) return p_sInfo;
            if (Run(m_cartridge.RunMove(Cartridge.ePos.Attach))) return p_sInfo;
            if (Run(m_stage.RunRotate(15))) return p_sInfo;
            if (Run(m_cartridge.RunStopper(false))) return p_sInfo;

            if (Run(m_cartridge.RunMove(Cartridge.ePos.Taping))) return p_sInfo;
            if (Run(m_stage.RunRotate(705))) return p_sInfo;

            if (Run(m_stage.RunRotate(33))) return p_sInfo;
            if (Run(m_cartridge.RunMove(Cartridge.ePos.Cutting))) return p_sInfo;
            if (Run(m_cartridge.RunCutter())) return p_sInfo;

            // 0203 Tape 잔량 시퀀스 추가 필요
            if (Run(m_cartridge.RunMove(Cartridge.ePos.CheckTape))) return p_sInfo;
            //if (Run(m_camera.RunCheckEndVRS())) return p_sInfo;

            if (Run(m_stage.RunRotate(327))) return p_sInfo;
            if (Run(m_stage.RunMove(m_stage.m_degReady))) return p_sInfo;

            if (Run(m_head.RunHeadDown(false))) return p_sInfo;
            if (Run(m_roller.RunRollerPushUp(false))) return p_sInfo;
            p_eProcess = eProcess.Done;
            return "OK";
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
            m_head.RunTree(tree.GetTree("Head"));
            m_stage.RunTree(tree.GetTree("Stage"));
        }
        public override void Reset()
        {
            m_roller.RunRollerPushUp(false);
            //여기가 프로세스별 Recovery 진행하거나 알람 띄우면 되나
            base.Reset();
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            //뚜껑 닫고 그냥 축 홈 잡자
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            p_sInfo = base.StateHome();
            if (Run(m_head.RunSol(true, true)))
                return p_sInfo;
            if (Run(m_roller.RunRollerPushUp(false)))
                return p_sInfo;
            Thread.Sleep(1000);
            if (Run(m_head.RunVacuum(false)))
                return p_sInfo;
            if (Run(m_head.RunSol(false, false)))
                return p_sInfo;
            if (Run(m_roller.RunRollerPushUp(false)))
                return p_sInfo;

            //m_stage.BeforeHome(); 


            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            p_eProcess = (p_sInfo == "OK") ? p_eProcess : eProcess.Error;
            if (p_eState == eState.Ready)
                m_stage.RunMove(m_stage.m_degReady);
            return p_sInfo;
        }
        #endregion

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Just Time Delay");
            AddModuleRunList(new Run_Solvalve(this), false, "Run Solvalve");
            AddModuleRunList(new Run_Rotate(this), false, "Run Rotate");
            AddModuleRunList(new Run_Cover(this), true, "Run Cover Open, Close, Head Up");
            AddModuleRunList(new Run_Taping(this), true, "Run Taping");
        }

        public class Run_Delay : ModuleRunBase
        {
            TapePacker m_module;
            public Run_Delay(TapePacker module)
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
            TapePacker m_module;
            public Run_Solvalve(TapePacker module)
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

        public class Run_Rotate : ModuleRunBase
        {
            TapePacker m_module;
            public Run_Rotate(TapePacker module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_fDeg = 0;
            public override ModuleRunBase Clone()
            {
                Run_Rotate run = new Run_Rotate(m_module);
                run.m_fDeg = m_fDeg;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_fDeg = tree.Set(m_fDeg, m_fDeg, "Rotate", "Rotate Degree (Degree)", bVisible);
            }

            public override string Run()
            {
                return m_module.m_stage.RunRotate(m_fDeg);
                //return m_module.m_stage.RunMove(m_fDeg); 
            }
        }

        public class Run_Cover : ModuleRunBase
        {
            TapePacker m_module;
            public Run_Cover(TapePacker module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            enum eCover
            {
                Open,
                Close,
                HeadUp
            }
            eCover m_eCover = eCover.Open;
            public override ModuleRunBase Clone()
            {
                Run_Cover run = new Run_Cover(m_module);
                run.m_eCover = m_eCover;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eCover = (eCover)tree.Set(m_eCover, m_eCover, "Cover", "Run Cover", bVisible);
            }

            public override string Run()
            {
                switch (m_eCover)
                {
                    case eCover.Open:
                        return m_module.RunCoverOpen();
                    case eCover.Close:
                        return m_module.RunCoverClose();
                    case eCover.HeadUp:
                        return m_module.RunHeadUp();
                }
                return "OK";
            }
        }

        public class Run_Taping : ModuleRunBase
        {
            TapePacker m_module;
            public Run_Taping(TapePacker module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Taping run = new Run_Taping(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunTaping();
            }
        }
        #endregion
    }
}

