using Root_EFEM;
using Root_EFEM.Module;
using RootTools;
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

            if (bInit)
            {
                InitALID();
            }
        }
        #endregion

        #region GAF
        public ALID alid_VacuumPacker;
        void InitALID()
        {
            alid_VacuumPacker = m_gaf.GetALID(this, "Vacuum Packer", "VACUUM PACKER ERROR");
        }
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
                foreach (DIO_I2O2 sol in m_aSolvalve)
                    asSol.Add(sol.m_id);
                return asSol;
            }
        }

        public DIO_I2O2 GetSolvalve(string sSol)
        {
            foreach (DIO_I2O2 sol in m_aSolvalve)
            {
                if (sol.m_id == sSol)
                    return sol;
            }
            return null;
        }
        #endregion

        #region Wrapper
        public class Wrapper
        {
            string m_id;
            VacuumPacker m_packer;
            public Wrapper(string id, VacuumPacker packer)
            {
                m_id = id;
                m_packer = packer;
            }
            public void RunTree(Tree tree)
            {
                m_secVac = tree.Set(m_secVac, m_secVac, "Vacuum", "Vacuum On Wait (sec)");
                m_secBlow = tree.Set(m_secBlow, m_secBlow, "Blow", "Vacuum Off Blow Time (sec)");
            }
            public string StateHome()
            {
                if (m_packer.Run(RunVacOff(true)))
                    return m_packer.p_sInfo;
                if (m_packer.Run(RunVacOff(false)))
                    return m_packer.p_sInfo;
                if (m_packer.Run(RunPush(false)))
                    return m_packer.p_sInfo;
                if (m_packer.Run(m_axisPickerX.StartHome()))
                    return m_packer.p_sInfo;

                double dist = m_axisPickerX.GetPosValue(ePosMove.Place) - m_axisPickerX.GetPosValue(ePosMove.Pick);
                while (dist / 2 < m_axisPickerX.p_posActual)
                {
                    Thread.Sleep(10);
                }
                if (m_packer.Run(m_axisPickerZ.StartHome()))
                    return m_packer.p_sInfo;
                if (m_packer.Run(m_axisPickerZ.WaitReady()))
                    return m_packer.p_sInfo;
                if (m_packer.Run(m_axisPickerX.WaitReady()))
                    return m_packer.p_sInfo;

                return "OK";
            }

            Axis m_axisPickerX; // PickerX, 9번축
            Axis m_axisPickerZ; // PickerZ, 10번축
            DIO_IO[] m_dioVacuum = new DIO_IO[2]; // [0] 가운데줄 Y72, [1] 사이드줄 Y74
            DIO_O[] m_doBlow = new DIO_O[2]; // [0] 가운데줄 Y73, [1] 사이드줄 Y74
            DIO_I2O2 m_solPush; // X98,99, Y70,71
            DIO_I m_diCheck; // X48
            DIO_I m_diLevel; // X49
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_packer.p_sInfo = toolBox.Get(ref m_axisPickerX, m_packer, m_id + ".Picker X");
                m_packer.p_sInfo = toolBox.Get(ref m_axisPickerZ, m_packer, m_id + ".Picker Z");
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
                Pick, //봉투잡는위치
                Place, // 봉투놓는위치
                Push // 푸셔 미는위치
            }
            public enum ePosPicker
            {
                Up, //위 위치
                Down, // 아래 위치
                Down2, // 아래 위치
                Open, // 봉투벌리는 높이
                Push // 푸셔 밀때의 높이
            }
            void InitPos()
            {
                m_dioVacuum[0].Write(false);
                m_dioVacuum[1].Write(false);
                m_axisPickerX.AddPos(Enum.GetNames(typeof(ePosMove)));
                m_axisPickerZ.AddPos(Enum.GetNames(typeof(ePosPicker)));
            }

            public string RunMoveX(ePosMove ePos, bool bWait = true)
            {
                if (bWait)
                {
                    m_axisPickerX.StartMove(ePos);
                    return m_axisPickerX.WaitReady();
                }
                else
                {
                    double dist = m_axisPickerX.GetPosValue(ePosMove.Place) - m_axisPickerX.GetPosValue(ePosMove.Pick);
                    m_axisPickerX.StartMove(ePos);
                    while (dist / 2 < m_axisPickerX.p_posActual)
                    {
                        Thread.Sleep(10);
                    }
                    return "OK";
                }
            }
            public string RunMoveZ(ePosPicker ePos)
            {
                m_axisPickerZ.StartMove(ePos);
                return m_axisPickerZ.WaitReady();
            }

            double m_secVac = 0.5;
            double m_secBlow = 0.5;
            public string RunVacOn()
            {
                m_dioVacuum[1].Write(true);
                m_dioVacuum[0].Write(true);
                int msVac = (int)(1000 * m_secVac);
                while (m_dioVacuum[0].m_swWrite.ElapsedMilliseconds < msVac)
                {
                    Thread.Sleep(10);
                    if (m_dioVacuum[0].p_bIn && m_dioVacuum[1].p_bIn)
                        return "OK";
                    if (EQ.IsStop())
                        return m_id + " EQ Stop";
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
            public string RunPush(bool bPush)
            {
                return m_solPush.RunSol(bPush);
            }
            public string RunPushBack()
            {
                try
                {
                    string sRun = m_solPush.RunSol(true);
                    if (sRun != "OK")
                        return sRun;
                    return m_solPush.RunSol(false);
                }
                finally { m_solPush.Write(false); }
            }
            public bool IsWapperExist()
            {
                return (m_diCheck.p_bIn && m_diLevel.p_bIn);
            }


        }
        public Wrapper m_wrapper;
        #endregion

        #region Stage
        public class Stage
        {
            string m_id;
            VacuumPacker m_packer;
            public Stage(string id, VacuumPacker packer)
            {
                m_id = id;
                m_packer = packer;
            }
            public void RunTree(Tree tree)
            {
                m_secVac = tree.Set(m_secVac, m_secVac, "Vacuum", "Vacuum On & Off Delay (sec)");
            }
            public string StateHome()
            {
                if (m_packer.Run(RunRollerUp(false)))
                    return m_packer.p_sInfo;
                if (m_packer.Run(RunUp(true)))
                    return m_packer.p_sInfo;
                if (m_packer.Run(RunRotate(false)))
                    return m_packer.p_sInfo;
                if (m_packer.Run(RunVac(false, false)))
                    return m_packer.p_sInfo;
                if (m_packer.Run(RunUp(false)))
                    return m_packer.p_sInfo;

                return "OK";
            }

            DIO_O[] m_doVacuum = new DIO_O[2]; // [0] 센터 Vac Y84 , [1] 사이드 Vac Y82
            DIO_O[] m_doBlow = new DIO_O[2]; // [0] 센터 Vac Y85 , [1] 사이드 Vac Y83
            DIO_I2O2 m_solUp; // 돌리기위해서 올려주는 솔
            DIO_I2O2 m_solRotate; // 90도 돌리는 솔
            DIO_I2O2 m_solRoller; // Roller Up:Down(X124:X125 / Y88:Y89)
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_packer.p_sInfo = toolBox.Get(ref m_doVacuum[0], m_packer, m_id + ".Vacuum Center");
                m_packer.p_sInfo = toolBox.Get(ref m_doVacuum[1], m_packer, m_id + ".Vacuum Side");
                m_packer.p_sInfo = toolBox.Get(ref m_doBlow[0], m_packer, m_id + ".Blow Center");
                m_packer.p_sInfo = toolBox.Get(ref m_doBlow[1], m_packer, m_id + ".Blow Side");
                m_packer.p_sInfo = toolBox.Get(ref m_solUp, m_packer, m_id + ".Up", "Down", "Up");
                m_packer.p_sInfo = toolBox.Get(ref m_solRotate, m_packer, m_id + ".Rotate", "Home", "90");
                m_packer.p_sInfo = toolBox.Get(ref m_solRoller, m_packer, m_id + ".Roller", "Up", "Down");
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
                return "OK";
            }

            public string RunUp(bool bUp)
            {
                return m_solUp.RunSol(bUp);
            }

            public string RunRotate(bool bRotate)
            {
                if (m_solUp.p_bOut == false)
                    return "State Down";
                return m_solRotate.RunSol(bRotate);
            }
            public string RunRollerUp(bool bUp)
            {
                return m_solRoller.RunSol(!bUp);
            }

        }
        public Stage m_stage;
        #endregion

        #region Transfer
        public class Transfer
        {
            string m_id;
            VacuumPacker m_packer;
            public Transfer(string id, VacuumPacker packer)
            {
                m_id = id;
                m_packer = packer;
            }
            public string StateHome()
            {
                if (m_packer.Run(m_axisTransfer.StartHome()))
                    return m_packer.p_sInfo;

                double dist = m_axisTransfer.GetPosValue(Transfer.ePos.PushStep2) - m_axisTransfer.GetPosValue(Transfer.ePos.Ready);
                while (dist / 2 < m_axisTransfer.p_posActual)
                {
                    Thread.Sleep(10);
                }
                if (m_packer.Run(RunDown(false)))
                    return m_packer.p_sInfo;
                if (m_packer.Run(RunPush(false)))
                    return m_packer.p_sInfo;
                if (m_packer.Run(m_axisTransfer.WaitReady()))
                    return m_packer.p_sInfo;
                return "OK";
            }

            Axis m_axisTransfer; //11번축, 로더에서 패킹스테이지로 밀어주는 축
            DIO_I2O2 m_solDown; // Y54,Y55 위아래 솔
            DIO_I2O2 m_solPush; // Y56,Y57 푸셔 솔
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_packer.p_sInfo = toolBox.Get(ref m_axisTransfer, m_packer, m_id);
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
                Ready, // 레디 약간 뒤임
                PushStep1, // Bridge Sol On 전까지 위치 (100,000)?
                PushStep2 // Bridge Sol On 이후 위치 (420,000)?
            }
            void InitPos()
            {
                m_axisTransfer.AddPos(Enum.GetNames(typeof(ePos)));
            }

            public string RunMove(ePos ePos, bool bWait = true)
            {
                if (bWait)
                {
                    m_axisTransfer.StartMove(ePos);
                    return m_axisTransfer.WaitReady();
                }
                else
                {
                    double dist = m_axisTransfer.GetPosValue(Transfer.ePos.PushStep2) - m_axisTransfer.GetPosValue(Transfer.ePos.Ready);
                    m_axisTransfer.StartMove(ePos);
                    while (dist / 2 < m_axisTransfer.p_posActual)
                    {
                        Thread.Sleep(10);
                    }
                    return "OK";
                }
            }

            public string RunDown(bool bDown)
            {
                return m_solDown.RunSol(bDown);
            }

            public string RunPush(bool bPush)
            {
                return m_solPush.RunSol(bPush);
            }

            public string RunPushBack()
            {
                try
                {
                    string sRun = m_solPush.RunSol(true);
                    if (sRun != "OK")
                        return sRun;
                    return m_solPush.RunSol(false);
                }
                finally { m_solPush.Write(false); }
            }
        }
        public Transfer m_transfer;
        #endregion

        #region Holder
        public class Holder
        {
            string m_id;
            VacuumPacker m_packer;
            public Holder(string id, VacuumPacker packer)
            {
                m_id = id;
                m_packer = packer;
            }
            public void RunTree(Tree tree)
            {
                m_secVac = tree.Set(m_secVac, m_secVac, "Vacuum", "Vacuum On Wait (sec)");
                m_secBlow = tree.Set(m_secBlow, m_secBlow, "Blow", "Vacuum Off Blow Time (sec)");
            }
            public string StateHome()
            {
                if (m_packer.Run(RunVacuum_Down(false)))
                    return m_packer.p_sInfo;
                if (m_packer.Run(RunHold_Both(false)))
                    return m_packer.p_sInfo;
                if (m_packer.Run(RunGuideX_Forward(false)))
                    return m_packer.p_sInfo;
                if (m_packer.Run(RunHold_Both(true)))
                    return m_packer.p_sInfo;
                if (m_packer.Run(RunGuideZ_Down(false)))
                    return m_packer.p_sInfo;

                return "OK";
            }

            // Y86 봉투 아래 홀더 공압
            DIO_IO[] m_dioHolderVac = new DIO_IO[2]; // Up/Down Holder Vac (X102:X93 / Y80:Y86)
            DIO_O[] m_doHolderBlow = new DIO_O[2];  // Holder Vac Blow
            DIO_I2O2[] m_solHold = new DIO_I2O2[2]; //Y46~Y49 [0]위 [1]아래 
            DIO_I2O2 m_solGuideZ; // Guide Z Up:down (X70:X71 / Y52:Y53) 
            DIO_I2O2 m_solGuideX; // Guide X Front:Back (X68:X69 / Y50:Y51) 
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_packer.p_sInfo = toolBox.Get(ref m_dioHolderVac[0], m_packer, m_id + ".UpHolder Vacuum");
                m_packer.p_sInfo = toolBox.Get(ref m_dioHolderVac[1], m_packer, m_id + ".DownHolder Vacuum");
                m_packer.p_sInfo = toolBox.Get(ref m_doHolderBlow[0], m_packer, m_id + ".UpHolder Blow");
                m_packer.p_sInfo = toolBox.Get(ref m_doHolderBlow[1], m_packer, m_id + ".DownHolder Blow");
                m_packer.p_sInfo = toolBox.Get(ref m_solHold[0], m_packer, m_id + ".UpHolder", "Open", "Hold");
                m_packer.p_sInfo = toolBox.Get(ref m_solHold[1], m_packer, m_id + ".DownHolder", "Open", "Hold");
                m_packer.p_sInfo = toolBox.Get(ref m_solGuideZ, m_packer, m_id + ".Guide Z", "Up", "Down");
                m_packer.p_sInfo = toolBox.Get(ref m_solGuideX, m_packer, m_id + ".Guide X", "Backward", "Forward");
                if (bInit)
                {
                    m_packer.InitSolvalve(m_solGuideZ);
                    m_packer.InitSolvalve(m_solGuideX);
                    m_packer.InitSolvalve(m_solHold[0]);
                    m_packer.InitSolvalve(m_solHold[1]);
                }
            }

            double m_secVac = 1;
            double m_secBlow = 1;
            public string RunVacuum_Down(bool bOn)
            {
                //m_dioHolderVac[0].Write(bOn);
                m_dioHolderVac[1].Write(bOn);
                if (bOn)
                {
                    int msTimeout = (int)(1000 * m_secVac);
                    StopWatch sw = new StopWatch();
                    while (sw.ElapsedMilliseconds < msTimeout)
                    {
                        Thread.Sleep(10);
                        if (/*m_dioHolderVac[0].p_bIn && */m_dioHolderVac[1].p_bIn)
                            return "OK";
                    }
                    return "Vacuum On timeout";
                }
                else
                {
                    //m_doHolderBlow[0].Write(true);
                    m_doHolderBlow[1].Write(true);
                    Thread.Sleep((int)(1000 * m_secBlow));
                    //m_doHolderBlow[0].Write(false);
                    m_doHolderBlow[1].Write(false);
                }
                return "OK";
            }
            public string RunHold_Up(bool bHold)
            {
                m_solHold[0].Write(bHold);
                int msTimeout = (int)(1000 * m_solHold[0].m_secTimeout);
                StopWatch sw = new StopWatch();
                while (sw.ElapsedMilliseconds < msTimeout)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop())
                        return "EQ Stop";
                    if (m_solHold[0].p_bDone)
                        return "OK";
                }
                return "Hold Timeout";
            }
            public string RunHold_Down(bool bHold)
            {
                m_solHold[1].Write(bHold);
                int msTimeout = (int)(1000 * m_solHold[0].m_secTimeout);
                StopWatch sw = new StopWatch();
                while (sw.ElapsedMilliseconds < msTimeout)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop())
                        return "EQ Stop";
                    if (m_solHold[0].p_bDone)
                        return "OK";
                }
                return "Hold Timeout";
            }
            public string RunHold_Both(bool bHold)
            {
                m_solHold[0].Write(bHold);
                m_solHold[1].Write(bHold);
                int msTimeout = (int)(1000 * m_solHold[0].m_secTimeout);
                StopWatch sw = new StopWatch();
                while (sw.ElapsedMilliseconds < msTimeout)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop())
                        return "EQ Stop";
                    if (m_solHold[0].p_bDone && m_solHold[1].p_bDone)
                        return "OK";
                }
                return "Hold Timeout";
            }
            public string RunGuideZ_Down(bool bDown)
            {
                if (m_packer.Run(m_solGuideZ.RunSol(bDown)))
                    return m_packer.p_sInfo;
                return "OK";
            }
            public string RunGuideX_Forward(bool bForward)
            {
                if (m_packer.Run(m_solGuideX.RunSol(bForward)))
                    return m_packer.p_sInfo;
                return "OK";
            }
            public string RunGuideDownForward(bool bDownForward)
            {
                //if (m_packer.Run(RunHold(false))) return m_packer.p_sInfo; 
                if (bDownForward)
                {
                    if (m_packer.Run(m_solGuideZ.RunSol(true)))
                        return m_packer.p_sInfo;
                    if (m_packer.Run(m_solGuideX.RunSol(true)))
                        return m_packer.p_sInfo;
                }
                else
                {
                    if (m_packer.Run(m_solGuideX.RunSol(false)))
                        return m_packer.p_sInfo;
                    if (m_packer.Run(m_solGuideZ.RunSol(false)))
                        return m_packer.p_sInfo;
                }
                return "OK";
            }



        }
        public Holder m_holder;
        #endregion

        #region Loader
        public class Loader
        {
            string m_id;
            VacuumPacker m_packer;
            public int m_secVac = 10;
            public Loader(string id, VacuumPacker packer)
            {
                m_id = id;
                m_packer = packer;
            }
            public void RunTree(Tree tree)
            {
                m_secVac = tree.Set(m_secVac, m_secVac, "Vacuum", "Vacuum On Wait (sec)");
            }
            public string StateHome()
            {
                m_doArmVacuumPump.Write(false);

                if (m_packer.Run(RunBridgeSol(false)))
                    return m_packer.p_sInfo;

                if (m_packer.Run(m_axisBridge.StartHome()))
                    return m_packer.p_sInfo;

                if (m_packer.Run(m_axisVacArmX.StartHome()))
                    return m_packer.p_sInfo;

                if (m_packer.Run(m_axisVacArmX.WaitReady()))
                    return m_packer.p_sInfo;

                if (m_packer.Run(m_axisVacArmX.StartMove(ePosArmX.Ready)))
                    return m_packer.p_sInfo;

                if (m_packer.Run(m_axisVacArmX.WaitReady()))
                    return m_packer.p_sInfo;

                if (m_packer.Run(m_axisVacArmWidth.StartHome()))
                    return m_packer.p_sInfo;

                if (m_packer.Run(m_axisVacArmWidth.WaitReady()))
                    return m_packer.p_sInfo;

                if (m_packer.Run(m_axisVacArmWidth.StartMove(ePosArmWidth.Ready)))
                    return m_packer.p_sInfo;

                if (m_packer.Run(m_axisVacArmWidth.WaitReady()))
                    return m_packer.p_sInfo;

                if (m_packer.Run(m_axisBridge.WaitReady()))
                    return m_packer.p_sInfo;

                return "OK";
            }

            Axis m_axisVacArmX; // 7번축 Vacuum Arm X
            Axis m_axisVacArmWidth; // 6번축 Vacuum Arm Width
            DIO_O m_doArmVacuumPump; //Y68, Y60 노즐 Vac

            Axis m_axisBridge; // 8번축 Loader
            DIO_I m_diPodCheck; // X92 Pod Check Sensor
            DIO_I2O2 m_solBridge; // Y29,30 이게멀까 // Bridge Sol
            //DIO_I2O2 m_solRaise; // 삭제

            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_packer.p_sInfo = toolBox.Get(ref m_axisVacArmX, m_packer, m_id + ".Arm X");
                m_packer.p_sInfo = toolBox.Get(ref m_axisBridge, m_packer, m_id + ".Bridge");
                m_packer.p_sInfo = toolBox.Get(ref m_axisVacArmWidth, m_packer, m_id + ".Arm Width");
                m_packer.p_sInfo = toolBox.Get(ref m_solBridge, m_packer, m_id + ".Bridge", "Back", "Push");
                //m_packer.p_sInfo = toolBox.Get(ref m_solRaise, m_packer, m_id + ".Raise", "Down", "Up");
                m_packer.p_sInfo = toolBox.Get(ref m_diPodCheck, m_packer, m_id + ".Pod Check");
                m_packer.p_sInfo = toolBox.Get(ref m_doArmVacuumPump, m_packer, m_id + ".Vacuum Pump");
                if (bInit)
                {
                    m_packer.InitSolvalve(m_solBridge);
                    //m_packer.InitSolvalve(m_solRaise);
                    InitPos();
                }
            }

            public enum eSpeed
            {
                //7번축 속도
                Slow,
                Fast
            }
            public enum ePosArmX
            {
                //7번축
                Ready, //맨뒤
                Vacuum, // Vac키는위치
                Heating // 히팅할때 회피할 위치
            }
            public enum ePosBridge
            {
                //8번축
                Ready,
                Bridge
            }
            public enum ePosArmWidth
            {
                //6번축
                Ready,
                IntoPack,
                Vacuum
            }
            void InitPos()
            {
                m_doArmVacuumPump.Write(false);
                m_axisVacArmX.AddSpeed(Enum.GetNames(typeof(eSpeed)));
                m_axisVacArmX.AddPos(Enum.GetNames(typeof(ePosArmX)));
                m_axisBridge.AddPos(Enum.GetNames(typeof(ePosBridge)));
                m_axisVacArmWidth.AddPos(Enum.GetNames(typeof(ePosArmWidth)));
            }

            public string RunMoveArmX(ePosArmX ePos, eSpeed eSpeed)
            {
                m_axisVacArmX.StartMove(ePos, 0, eSpeed);
                return m_axisVacArmX.WaitReady();
            }
            public string RunMoveArmWidth(ePosArmWidth ePos)
            {
                m_axisVacArmWidth.StartMove(ePos);
                return m_axisVacArmWidth.WaitReady();
            }

            public string RunBridgeAxis(ePosBridge ePos)
            {
                m_axisBridge.StartMove(ePos);
                return m_axisBridge.WaitReady();
            }

            public string RunBridgeSol(bool bRun)
            {
                return m_solBridge.RunSol(bRun);
            }
            public string RunBridgeSol()
            {
                try
                {
                    if (m_packer.Run(m_solBridge.RunSol(true)))
                        return m_packer.p_sInfo;
                    Thread.Sleep(200);
                    return m_solBridge.RunSol(false);
                }
                finally { m_solBridge.Write(false); }
            }

            //public string RunRaise(bool bUp)
            //{
            //    return m_solRaise.RunSol(bUp);
            //}

            public bool IsPodExist()
            {
                return m_diPodCheck.p_bIn;
            }


            public string RunVacPump()
            {
                m_doArmVacuumPump.Write(true);
                int msVac = (int)(1000 * m_secVac);
                StopWatch sw = new StopWatch();
                while (sw.ElapsedMilliseconds < msVac)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop())
                        return "EQ Stop";
                }
                return "OK";
            }
            public string RunVacPumpOff()
            {
                m_doArmVacuumPump.Write(false);
                return "OK";
            }




        }
        public Loader m_loader;
        #endregion

        #region Heater
        public class Heater
        {
            string m_id;
            VacuumPacker m_packer;
            public double m_secHeat = 3;
            public double m_secHeatBefore = 2;
            public Heater(string id, VacuumPacker packer)
            {
                m_id = id;
                m_packer = packer;
            }
            public void RunTree(Tree tree)
            {
                m_secHeat = tree.Set(m_secHeat, m_secHeat, "Heat", "Heat On Wait (sec)");
                m_secHeatBefore = tree.Set(m_secHeatBefore, m_secHeatBefore, "Heat Before", "Heat Before On Wait(sec)");
            }
            public string StateHome()
            {
                if (m_packer.Run(RunHeaterSol(false)))
                    return m_packer.p_sInfo;
                if (m_packer.Run(RunSpongeSol(false)))
                    return m_packer.p_sInfo;
                return "OK";
            }

            DIO_I2O2[] m_solSponge = new DIO_I2O2[2]; // [0] Y42,43 [1] Y34, 35 Sponge 위/아래 sol
            DIO_I2O2[] m_solHeater = new DIO_I2O2[2]; // [0] Y44,45 [1] Y32, 33 heater 위/아래 sol
            DIO_IO m_dioHeat; // Y11, X79 
            DIO_O m_doHeatTimeout; //Y12
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_packer.p_sInfo = toolBox.Get(ref m_solSponge[0], m_packer, m_id + ".UpSponge", "Up", "Down");
                m_packer.p_sInfo = toolBox.Get(ref m_solSponge[1], m_packer, m_id + ".DownSponge", "Up", "Down");
                m_packer.p_sInfo = toolBox.Get(ref m_solHeater[0], m_packer, m_id + ".UpSilicone", "Up", "Down");
                m_packer.p_sInfo = toolBox.Get(ref m_solHeater[1], m_packer, m_id + ".DownHeater", "Up", "Down");
                m_packer.p_sInfo = toolBox.Get(ref m_dioHeat, m_packer, m_id + ".Heat");
                m_packer.p_sInfo = toolBox.Get(ref m_doHeatTimeout, m_packer, m_id + ".Heat Timeout");
                if (bInit)
                {
                    m_dioHeat.Write(false);
                    m_doHeatTimeout.Write(false);
                    m_packer.m_holder.RunHold_Both(true);
                    m_packer.m_heater.RunHeaterSol(false);
                    m_packer.m_heater.RunSpongeSol(false);
                    //m_packer.InitSolvalve(m_solSponge[0]);
                    //m_packer.InitSolvalve(m_solSponge[1]);
                    //m_packer.InitSolvalve(m_solHeater[0]);
                    //m_packer.InitSolvalve(m_solHeater[1]);
                }
            }

            public string RunSpongeSol(bool bClose)
            {
                m_solSponge[0].Write(bClose);
                m_solSponge[1].Write(!bClose);
                //return "OK";
                int msTimeout = (int)(1000 * m_solSponge[0].m_secTimeout);
                StopWatch sw = new StopWatch();
                while (sw.ElapsedMilliseconds < msTimeout)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop())
                        return "EQ Stop";
                    if (m_solSponge[0].p_bDone && m_solSponge[1].p_bDone)
                        return "OK";
                }
                return "Sponge Timeout";
            }

            public string RunHeaterSol(bool bClose)
            {
                //m_solHeater[0].Write(true);
                m_solHeater[1].Write(!bClose);
                //m_solHeater[1].Write(!bClose);
                int msTimeout = (int)(1000 * m_solHeater[0].m_secTimeout);
                StopWatch sw = new StopWatch();
                while (sw.ElapsedMilliseconds < msTimeout)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop())
                        return "EQ Stop";
                    if (m_solHeater[1].p_bDone)
                        return "OK";
                    //if (m_solHeater[0].p_bDone && m_solHeater[1].p_bDone)
                    //    return "OK";
                }
                return "HeaterClose Timeout";
            }

            public string RunHeat(bool bBefore)
            {
                double sec = 0;
                if (bBefore)
                    sec = m_secHeat;
                else
                    sec = m_secHeatBefore;
                try
                {
                    m_dioHeat.Write(true);
                    StopWatch sw = new StopWatch();
                    int msHeat = (int)(1000 * sec);
                    while (sw.ElapsedMilliseconds < msHeat)
                    {
                        Thread.Sleep(10);
                        if (EQ.IsStop())
                            return "EQ Stop";
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


        }
        public Heater m_heater;
        #endregion


        #region Function
        public enum eStep
        {
            GetWrapper, // 봉투가져오기
            HoldWrapper, // 벌리고 홀더가잡기까지
            BackWrapper, // 피커 회피
            InsertCase, // 봉투에 넣기
            InputVacArm, // Vac Arm 넣기
            ReleaseWrapper, // 홀더놓기
            CloseWrapper, //스폰지닫기
            VacuumPump, //봉투에 vac하고 회피
            Heating, // 히팅
            Rotate, // 90도 돌리기
            PushToLoader, //Picker가 다시 밀어주기
            Unload,
            Heatingtest,
        }

        public string RunStep(eStep eStep)
        {
            switch (eStep)
            {
                case eStep.GetWrapper:
                    //if (m_wrapper.IsWapperExist() == false) return "No Wrapper";
                    if (Run(m_wrapper.RunMoveX(Wrapper.ePosMove.Pick)))
                        return p_sInfo;
                    if (Run(m_wrapper.RunMoveZ(Wrapper.ePosPicker.Down)))
                        return p_sInfo;
                    Thread.Sleep(10);
                    //Run(m_wrapper.RunVacOn());
                    if (Run(m_wrapper.RunVacOn()))
                    {
                        alid_VacuumPacker.Run(true, p_sInfo);
                        return p_sInfo;
                    }
                    if (Run(m_wrapper.RunMoveZ(Wrapper.ePosPicker.Open)))
                        return p_sInfo;
                    if (Run(m_wrapper.RunMoveX(Wrapper.ePosMove.Place)))
                        return p_sInfo;
                    if (Run(m_wrapper.RunMoveZ(Wrapper.ePosPicker.Down2)))
                        return p_sInfo;
                    if (Run(m_holder.RunVacuum_Down(true)))
                        return p_sInfo;
                    if (Run(m_wrapper.RunVacOff(false)))
                        return p_sInfo;
                    if (Run(m_wrapper.RunMoveZ(Wrapper.ePosPicker.Open)))
                        return p_sInfo;
                    return "OK";
                case eStep.HoldWrapper:
                    if (Run(m_holder.RunHold_Both(false)))
                        return p_sInfo;
                    if (Run(m_holder.RunGuideDownForward(true)))
                        return p_sInfo;
                    if (Run(m_holder.RunHold_Down(true)))
                        return p_sInfo;
                    Thread.Sleep(1000);
                    if (Run(m_holder.RunVacuum_Down(false)))
                        return p_sInfo;
                    if (Run(m_wrapper.RunMoveZ(Wrapper.ePosPicker.Up)))
                        return p_sInfo;
                    if (Run(m_holder.RunHold_Up(true)))
                        return p_sInfo;
                    Thread.Sleep(1000);
                    return "OK";
                case eStep.BackWrapper:
                    if (Run(m_wrapper.RunVacOff(true)))
                        return p_sInfo;
                    if (Run(m_wrapper.RunMoveX(Wrapper.ePosMove.Pick, false)))
                        return p_sInfo; //딜레이 제거 확인 필요
                    if (Run(m_holder.RunGuideZ_Down(false)))
                        return p_sInfo;
                    return "OK";
                case eStep.InsertCase:
                    try
                    {
                        if (Run(m_loader.RunMoveArmWidth(Loader.ePosArmWidth.Ready)))
                            return p_sInfo;
                        if (Run(m_loader.RunBridgeSol(false)))
                            return p_sInfo;
                        if (Run(m_loader.RunBridgeAxis(Loader.ePosBridge.Ready)))
                            return p_sInfo;
                        if (Run(m_transfer.RunMove(Transfer.ePos.Ready)))
                            return p_sInfo;

                        if (Run(m_loader.RunBridgeAxis(Loader.ePosBridge.Bridge)))
                            return p_sInfo;

                        if (Run(m_transfer.RunDown(true)))
                            return p_sInfo;
                        if (Run(m_transfer.RunMove(Transfer.ePos.PushStep1)))
                            return p_sInfo;

                        if (Run(m_loader.RunBridgeSol(true)))
                            return p_sInfo;

                        if (Run(m_transfer.RunMove(Transfer.ePos.PushStep2)))
                            return p_sInfo;
                        if (Run(m_transfer.RunPushBack()))
                            return p_sInfo;
                    }
                    finally
                    {
                        m_transfer.RunMove(Transfer.ePos.Ready, false); // 딜레이 제거 확인 필요
                        m_transfer.RunDown(false);
                        m_loader.RunBridgeSol(false);
                        m_loader.RunBridgeAxis(Loader.ePosBridge.Ready);
                    }
                    return "OK";
                case eStep.InputVacArm:
                    if (Run(m_loader.RunMoveArmWidth(Loader.ePosArmWidth.IntoPack)))
                        return p_sInfo;
                    if (Run(m_loader.RunMoveArmX(Loader.ePosArmX.Vacuum, Loader.eSpeed.Slow)))
                        return p_sInfo;
                    return "OK";
                case eStep.ReleaseWrapper:
                    if (Run(m_holder.RunHold_Both(false)))
                        return p_sInfo;
                    if (Run(m_holder.RunGuideX_Forward(false)))
                        return p_sInfo;
                    if (Run(m_holder.RunHold_Both(true)))
                        return p_sInfo;
                    return "OK";
                case eStep.CloseWrapper:
                    if (Run(m_loader.RunMoveArmWidth(Loader.ePosArmWidth.Vacuum)))
                        return p_sInfo;
                    if (Run(m_heater.RunSpongeSol(true)))
                        return p_sInfo;
                    Thread.Sleep(500);
                    return "OK";
                case eStep.VacuumPump:
                    if (Run(m_loader.RunVacPump()))
                        return p_sInfo;
                    if (Run(m_heater.RunHeat(true)))
                        return p_sInfo;
                    if (Run(m_loader.RunMoveArmX(Loader.ePosArmX.Heating, Loader.eSpeed.Fast)))
                        return p_sInfo;
                    if (Run(m_loader.RunVacPumpOff()))
                        return p_sInfo;
                    return "OK";
                case eStep.Heatingtest:
                    if (Run(m_heater.RunSpongeSol(true)))
                        return p_sInfo;
                    if (Run(m_heater.RunHeat(true)))
                        return p_sInfo;
                    if (Run(m_heater.RunHeaterSol(true)))
                        return p_sInfo;
                    if (Run(m_heater.RunHeat(false)))
                        return p_sInfo;
                    if (Run(m_heater.RunHeaterSol(false)))
                        return p_sInfo;
                    if (Run(m_heater.RunSpongeSol(false)))
                        return p_sInfo;
                    return "OK";
                case eStep.Heating:
                    if (Run(m_heater.RunSpongeSol(true)))
                        return p_sInfo;
                    if (Run(m_heater.RunHeaterSol(true)))
                        return p_sInfo;
                    if (Run(m_heater.RunHeat(false)))
                        return p_sInfo;
                    if (Run(m_heater.RunHeaterSol(false)))
                        return p_sInfo;
                    if (Run(m_heater.RunSpongeSol(false)))
                        return p_sInfo;
                    if (Run(m_loader.RunMoveArmX(Loader.ePosArmX.Ready, Loader.eSpeed.Slow)))
                        return p_sInfo;
                    if (Run(m_loader.RunMoveArmWidth(Loader.ePosArmWidth.Ready)))
                        return p_sInfo;
                    return "OK";
                case eStep.Rotate:
                    if (Run(m_stage.RunVac(true, false)))
                        return p_sInfo;
                    if (Run(m_stage.RunUp(true)))
                        return p_sInfo;
                    if (Run(m_stage.RunRotate(true)))
                        return p_sInfo;
                    //if (Run(m_stage.RunUp(false))) return p_sInfo;
                    Thread.Sleep(4000);
                    if (Run(m_stage.RunVac(false, false)))
                        return p_sInfo;
                    return "OK";
                case eStep.PushToLoader:
                    if (Run(m_holder.RunGuideZ_Down(true)))
                        return p_sInfo;
                    if (Run(m_holder.RunGuideX_Forward(true)))
                        return p_sInfo;
                    if (Run(m_holder.RunGuideZ_Down(false)))
                        return p_sInfo;
                    if (Run(m_loader.RunBridgeAxis(Loader.ePosBridge.Bridge)))
                        return p_sInfo;
                    if (Run(m_loader.RunBridgeSol(false)))
                        return p_sInfo;
                    if (Run(m_loader.RunBridgeSol(true)))
                        return p_sInfo;
                    if (Run(m_wrapper.RunMoveZ(Wrapper.ePosPicker.Push)))
                        return p_sInfo;
                    if (Run(m_stage.RunVac(false, false)))
                        return p_sInfo;
                    if (Run(m_wrapper.RunMoveX(Wrapper.ePosMove.Push)))
                        return p_sInfo;
                    if (Run(m_wrapper.RunPushBack()))
                        return p_sInfo;
                    if (Run(m_wrapper.RunMoveX(Wrapper.ePosMove.Pick, false)))
                        return p_sInfo;
                    if (Run(m_wrapper.RunMoveZ(Wrapper.ePosPicker.Open)))
                        return p_sInfo;
                    if (Run(m_stage.RunUp(false)))
                        return p_sInfo;
                    if (Run(m_loader.RunBridgeSol(false)))
                        return p_sInfo;
                    if (Run(m_loader.RunBridgeAxis(Loader.ePosBridge.Ready)))
                        return p_sInfo;
                    //if (Run(m_stage.RunUp(true))) return p_sInfo;
                    //if (Run(m_stage.RunRotate(false))) return p_sInfo;
                    //if (Run(m_stage.RunUp(false))) return p_sInfo;
                    return "OK";
                case eStep.Unload:
                    if (Run(m_loader.RunMoveArmWidth(Loader.ePosArmWidth.Ready)))
                        return p_sInfo;
                    if (Run(m_loader.RunMoveArmX(Loader.ePosArmX.Ready, Loader.eSpeed.Slow)))
                        return p_sInfo;
                    if (Run(m_loader.RunBridgeAxis(Loader.ePosBridge.Ready)))
                        return p_sInfo;
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
                if (Run(RunStep(eStep)))
                    return p_sInfo;
            }
            foreach (eStep eStep in m_aStep)
            {
                if (Run(RunStep(eStep)))
                    return p_sInfo;
            }
            if (Run(RunStep(eStep.Unload)))
                return p_sInfo;
            return "OK";
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

        public List<string> p_asChildSlot
        {
            get
            {
                return null;
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
            if (p_infoWafer == null)
                return p_id + " IsGetOK - InfoWafer not Exist";
            return "OK";
        }

        public string IsPutOK(InfoWafer infoWafer, int nID)
        {
            if (p_eState != eState.Ready)
                return p_id + " eState not Ready";
            if (p_infoWafer != null)
                return p_id + " IsPutOK - InfoWafer Exist";
            if (m_waferSize.GetData(infoWafer.p_eSize).m_bEnable == false)
                return p_id + " not Enable Wafer Size";
            return "OK";
        }

        public int GetTeachWTR(InfoWafer infoWafer = null)
        {
            if (infoWafer == null)
                infoWafer = p_infoWafer;
            return m_waferSize.GetData(infoWafer.p_eSize).m_teachWTR;
        }

        public string BeforeGet(int nID)
        {
            //0203
            if (p_infoWafer == null)
                return p_id + " BeforeGet : InfoWafer = null";
            return CheckGetPut();
        }

        public string BeforePut(int nID)
        {
            //0203
            if (p_infoWafer != null)
                return p_id + " BeforePut : InfoWafer != null";
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
            if (p_eState != eState.Ready)
                return p_id + " eState not Ready";
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
                case eCheckWafer.Sensor:
                    return false; // m_diWaferExist.p_bIn;
                default:
                    return (p_infoWafer != null);
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
            p_infoWafer = null;
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
            p_infoWafer = null;
            //p_sInfo = base.StateHome();
            if (Run(m_heater.StateHome()))
                return p_sInfo;
            if (Run(m_transfer.StateHome()))
                return p_sInfo;
            if (Run(m_wrapper.StateHome()))
                return p_sInfo;

            if (Run(m_loader.StateHome()))
                return p_sInfo;
            if (Run(m_holder.StateHome()))
                return p_sInfo;
            if (Run(m_stage.StateHome()))
                return p_sInfo;


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
                Thread.Sleep((int)(1000 * m_secDelay / 3));
                p_nProgress += 30;
                Thread.Sleep((int)(1000 * m_secDelay / 3));
                p_nProgress += 30;
                Thread.Sleep((int)(1000 * m_secDelay / 3));
                p_nProgress += 40;
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
                if (sol == null)
                    return "Invalid Solvalve Name";
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
                //p_id = p_id + "m_eStep";
            }

            public eStep m_eStep = eStep.GetWrapper;
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
                p_nProgress = 0;
                if (RunStep(m_eStep) != "OK")
                {
                    p_eRunState = eRunState.Error;
                    m_module.alid_VacuumPacker.Run(true, m_eStep.ToString() + ", " + p_sInfo);
                    return p_sInfo;
                }
                else
                {
                    return "OK";
                }
            }
            public string RunStep(eStep eStep)
            {
                switch (eStep)
                {
                    case eStep.GetWrapper:
                        if (m_module.m_wrapper.IsWapperExist() == false)
                            return "No Wrapper";
                        p_nProgress += 10;
                        if (m_module.Run(m_module.m_wrapper.RunMoveX(Wrapper.ePosMove.Pick)))
                            return p_sInfo;
                        p_nProgress += 10;
                        if (m_module.Run(m_module.m_wrapper.RunMoveZ(Wrapper.ePosPicker.Down)))
                            return p_sInfo;
                        p_nProgress += 10;
                        Thread.Sleep(1000);
                        if (m_module.Run(m_module.m_wrapper.RunVacOn()))
                        {
                            m_module.alid_VacuumPacker.Run(true, p_sInfo);
                            return p_sInfo;
                        }
                        p_nProgress += 10;
                        if (m_module.Run(m_module.m_wrapper.RunMoveZ(Wrapper.ePosPicker.Open)))
                            return p_sInfo;
                        p_nProgress += 10;
                        if (m_module.Run(m_module.m_wrapper.RunMoveX(Wrapper.ePosMove.Place)))
                            return p_sInfo;
                        p_nProgress += 10;
                        if (m_module.Run(m_module.m_wrapper.RunMoveZ(Wrapper.ePosPicker.Down2)))
                            return p_sInfo;
                        p_nProgress += 10;
                        if (m_module.Run(m_module.m_holder.RunVacuum_Down(true)))
                            return p_sInfo;
                        p_nProgress += 10;
                        if (m_module.Run(m_module.m_wrapper.RunVacOff(false)))
                            return p_sInfo;
                        p_nProgress += 10;
                        if (m_module.Run(m_module.m_wrapper.RunMoveZ(Wrapper.ePosPicker.Open)))
                            return p_sInfo;
                        p_nProgress += 10;
                        return "OK";
                    case eStep.HoldWrapper:
                        if (m_module.Run(m_module.m_holder.RunHold_Both(false)))
                            return p_sInfo;
                        p_nProgress += 16;
                        if (m_module.Run(m_module.m_holder.RunGuideDownForward(true)))
                            return p_sInfo;
                        p_nProgress += 17;
                        if (m_module.Run(m_module.m_holder.RunHold_Down(true)))
                            return p_sInfo;
                        p_nProgress += 16;
                        Thread.Sleep(1000);
                        if (m_module.Run(m_module.m_holder.RunVacuum_Down(false)))
                            return p_sInfo;
                        p_nProgress += 17;
                        if (m_module.Run(m_module.m_wrapper.RunMoveZ(Wrapper.ePosPicker.Up)))
                            return p_sInfo;
                        p_nProgress += 17;
                        if (m_module.Run(m_module.m_holder.RunHold_Up(true)))
                            return p_sInfo;
                        p_nProgress += 17;
                        Thread.Sleep(1000);
                        return "OK";
                    case eStep.BackWrapper:
                        if (m_module.Run(m_module.m_wrapper.RunVacOff(true)))
                            return p_sInfo;
                        p_nProgress += 33;
                        if (m_module.Run(m_module.m_wrapper.RunMoveX(Wrapper.ePosMove.Pick, false)))
                            return p_sInfo; //딜레이 제거 확인 필요
                        p_nProgress += 33;
                        if (m_module.Run(m_module.m_holder.RunGuideZ_Down(false)))
                            return p_sInfo;
                        p_nProgress += 34;
                        return "OK";
                    case eStep.InsertCase:
                        try
                        {
                            if (m_module.Run(m_module.m_loader.RunMoveArmWidth(Loader.ePosArmWidth.Ready)))
                                return p_sInfo;
                            p_nProgress += 8;
                            if (m_module.Run(m_module.m_loader.RunBridgeSol(false)))
                                return p_sInfo;
                            p_nProgress += 7;
                            if (m_module.Run(m_module.m_loader.RunBridgeAxis(Loader.ePosBridge.Ready)))
                                return p_sInfo;
                            p_nProgress += 8;
                            if (m_module.Run(m_module.m_transfer.RunMove(Transfer.ePos.Ready)))
                                return p_sInfo;
                            p_nProgress += 7;
                            if (m_module.Run(m_module.m_loader.RunBridgeAxis(Loader.ePosBridge.Bridge)))
                                return p_sInfo;
                            p_nProgress += 7;
                            if (m_module.Run(m_module.m_transfer.RunDown(true)))
                                return p_sInfo;
                            p_nProgress += 7;
                            if (m_module.Run(m_module.m_transfer.RunMove(Transfer.ePos.PushStep1)))
                                return p_sInfo;
                            p_nProgress += 7;
                            if (m_module.Run(m_module.m_loader.RunBridgeSol(true)))
                                return p_sInfo;
                            p_nProgress += 7;
                            if (m_module.Run(m_module.m_transfer.RunMove(Transfer.ePos.PushStep2)))
                                return p_sInfo;
                            p_nProgress += 7;
                            if (m_module.Run(m_module.m_transfer.RunPushBack()))
                                return p_sInfo;
                            p_nProgress += 7;
                        }
                        finally
                        {
                            m_module.m_transfer.RunMove(Transfer.ePos.Ready, false); // 딜레이 제거 확인 필요
                            p_nProgress += 7;
                            m_module.m_transfer.RunDown(false);
                            p_nProgress += 7;
                            m_module.m_loader.RunBridgeSol(false);
                            p_nProgress += 7;
                            m_module.m_loader.RunBridgeAxis(Loader.ePosBridge.Ready);
                            p_nProgress += 7;
                        }
                        return "OK";
                    case eStep.InputVacArm:
                        if (m_module.Run(m_module.m_loader.RunMoveArmWidth(Loader.ePosArmWidth.IntoPack)))
                            return p_sInfo;
                        p_nProgress += 50;
                        if (m_module.Run(m_module.m_loader.RunMoveArmX(Loader.ePosArmX.Vacuum, Loader.eSpeed.Slow)))
                            return p_sInfo;
                        p_nProgress += 50;
                        return "OK";
                    case eStep.ReleaseWrapper:
                        if (m_module.Run(m_module.m_holder.RunHold_Both(false)))
                            return p_sInfo;
                        p_nProgress += 33;
                        if (m_module.Run(m_module.m_holder.RunGuideX_Forward(false)))
                            return p_sInfo;
                        p_nProgress += 33;
                        if (m_module.Run(m_module.m_holder.RunHold_Both(true)))
                            return p_sInfo;
                        p_nProgress += 34;
                        return "OK";
                    case eStep.CloseWrapper:
                        if (m_module.Run(m_module.m_loader.RunMoveArmWidth(Loader.ePosArmWidth.Vacuum)))
                            return p_sInfo;
                        p_nProgress += 50;
                        if (m_module.Run(m_module.m_heater.RunSpongeSol(true)))
                            return p_sInfo;
                        Thread.Sleep(500);
                        p_nProgress += 50;
                        return "OK";
                    case eStep.VacuumPump:
                        if (m_module.Run(m_module.m_loader.RunVacPump()))
                            return p_sInfo;
                        p_nProgress += 25;
                        if (m_module.Run(m_module.m_heater.RunHeat(true)))
                            return p_sInfo;
                        p_nProgress += 25;
                        if (m_module.Run(m_module.m_loader.RunMoveArmX(Loader.ePosArmX.Heating, Loader.eSpeed.Fast)))
                            return p_sInfo;
                        p_nProgress += 25;
                        if (m_module.Run(m_module.m_loader.RunVacPumpOff()))
                            return p_sInfo;
                        p_nProgress += 25;
                        return "OK";
                    case eStep.Heatingtest:
                        if (m_module.Run(m_module.m_heater.RunSpongeSol(true)))
                            return p_sInfo;
                        p_nProgress += 17;
                        if (m_module.Run(m_module.m_heater.RunHeat(true)))
                            return p_sInfo;
                        p_nProgress += 17;
                        if (m_module.Run(m_module.m_heater.RunHeaterSol(true)))
                            return p_sInfo;
                        p_nProgress += 17;
                        if (m_module.Run(m_module.m_heater.RunHeat(false)))
                            return p_sInfo;
                        p_nProgress += 17;
                        if (m_module.Run(m_module.m_heater.RunHeaterSol(false)))
                            return p_sInfo;
                        p_nProgress += 16;
                        if (m_module.Run(m_module.m_heater.RunSpongeSol(false)))
                            return p_sInfo;
                        p_nProgress += 16;
                        return "OK";
                    case eStep.Heating:
                        if (m_module.Run(m_module.m_heater.RunSpongeSol(true)))
                            return p_sInfo;
                        p_nProgress += 14;
                        if (m_module.Run(m_module.m_heater.RunHeaterSol(true)))
                            return p_sInfo;
                        p_nProgress += 14;
                        if (m_module.Run(m_module.m_heater.RunHeat(false)))
                            return p_sInfo;
                        p_nProgress += 14;
                        if (m_module.Run(m_module.m_heater.RunHeaterSol(false)))
                            return p_sInfo;
                        p_nProgress += 14;
                        if (m_module.Run(m_module.m_heater.RunSpongeSol(false)))
                            return p_sInfo;
                        p_nProgress += 14;
                        if (m_module.Run(m_module.m_loader.RunMoveArmX(Loader.ePosArmX.Ready, Loader.eSpeed.Slow)))
                            return p_sInfo;
                        p_nProgress += 15;
                        if (m_module.Run(m_module.m_loader.RunMoveArmWidth(Loader.ePosArmWidth.Ready)))
                            return p_sInfo;
                        p_nProgress += 15;
                        return "OK";
                    case eStep.Rotate:
                        if (m_module.Run(m_module.m_stage.RunVac(true, false)))
                            return p_sInfo;
                        p_nProgress += 25;
                        if (m_module.Run(m_module.m_stage.RunUp(true)))
                            return p_sInfo;
                        p_nProgress += 25;
                        if (m_module.Run(m_module.m_stage.RunRotate(true)))
                            return p_sInfo;
                        p_nProgress += 25;
                        //if (m_module.Run(m_stage.RunUp(false))) return p_sInfo;
                        Thread.Sleep(4000);
                        if (m_module.Run(m_module.m_stage.RunVac(false, false)))
                            return p_sInfo;
                        p_nProgress += 25;
                        return "OK";
                    case eStep.PushToLoader:
                        if (m_module.Run(m_module.m_holder.RunGuideZ_Down(true)))
                            return p_sInfo;
                        p_nProgress += 7;
                        if (m_module.Run(m_module.m_holder.RunGuideX_Forward(true)))
                            return p_sInfo;
                        p_nProgress += 7;
                        if (m_module.Run(m_module.m_holder.RunGuideZ_Down(false)))
                            return p_sInfo;
                        p_nProgress += 7;
                        if (m_module.Run(m_module.m_loader.RunBridgeAxis(Loader.ePosBridge.Bridge)))
                            return p_sInfo;
                        p_nProgress += 7;
                        if (m_module.Run(m_module.m_loader.RunBridgeSol(false)))
                            return p_sInfo;
                        p_nProgress += 7;
                        if (m_module.Run(m_module.m_loader.RunBridgeSol(true)))
                            return p_sInfo;
                        p_nProgress += 7;
                        if (m_module.Run(m_module.m_wrapper.RunMoveZ(Wrapper.ePosPicker.Push)))
                            return p_sInfo;
                        p_nProgress += 7;
                        if (m_module.Run(m_module.m_stage.RunVac(false, false)))
                            return p_sInfo;
                        p_nProgress += 7;
                        if (m_module.Run(m_module.m_wrapper.RunMoveX(Wrapper.ePosMove.Push)))
                            return p_sInfo;
                        p_nProgress += 7;
                        if (m_module.Run(m_module.m_wrapper.RunPushBack()))
                            return p_sInfo;
                        p_nProgress += 7;
                        if (m_module.Run(m_module.m_wrapper.RunMoveX(Wrapper.ePosMove.Pick, false)))
                            return p_sInfo;
                        p_nProgress += 7;
                        if (m_module.Run(m_module.m_wrapper.RunMoveZ(Wrapper.ePosPicker.Open)))
                            return p_sInfo;
                        p_nProgress += 7;
                        if (m_module.Run(m_module.m_stage.RunRotate(false)))
                            return p_sInfo;
                        p_nProgress += 8;
                        if (m_module.Run(m_module.m_stage.RunUp(false)))
                            return p_sInfo;
                        p_nProgress += 8;

                        //if (m_module.Run(m_stage.RunUp(true))) return p_sInfo;
                        //if (m_module.Run(m_stage.RunRotate(false))) return p_sInfo;
                        //if (m_module.Run(m_stage.RunUp(false))) return p_sInfo;
                        return "OK";
                    case eStep.Unload:
                        if (m_module.Run(m_module.m_loader.RunBridgeSol(false)))
                            return p_sInfo;
                        p_nProgress += 50;
                        if (m_module.Run(m_module.m_loader.RunBridgeAxis(Loader.ePosBridge.Ready)))
                            return p_sInfo;
                        p_nProgress += 50;
                        return "OK";
                }
                return "OK";
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
