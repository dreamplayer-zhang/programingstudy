using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Root_AOP01_Packing
{
    public class Certification : ModuleBase
    {
        Thread m_threadCheck;
        bool m_bThreadCheck;
        bool m_bUseCurtain = false;

        Run_RNR m_RNR;


        Axis[] _axisAll = new Axis[11];

        DIO_I _diEMS;
        DIO_I _diAllDoorLock;
        DIO_I[] _diDoorLock = new DIO_I[3];
        DIO_I[] _diLightCurtain = new DIO_I[2];
        DIO_I _diProtectionBar;

        DIO_O _doDoorLock;

        ALID _alid_EMS;
        ALID _alid_DoorOpen;
        ALID _alid_LightCurtain;
        ALID _alid_ProtectionBar;

        #region Property
        public Axis[] AxisAll
        {
            get => _axisAll;
            set => _axisAll = value;
        }
        public DIO_I DiEMS
        {
            get => _diEMS;
            set => _diEMS = value;
        }
        public DIO_O DODoorLock
        {
            get => _doDoorLock;
            set => _doDoorLock = value;
        }
        public DIO_I DIAllDoorLock
        {
            get => _diAllDoorLock;
            set => _diAllDoorLock = value;
        }
        public DIO_I[] DiDoorLock
        {
            get => _diDoorLock;
            set => _diDoorLock = value;
        }
        public DIO_I[] DiLightCurtain
        {
            get => _diLightCurtain;
            set => _diLightCurtain = value;
        }
        public DIO_I DiProtectionBar
        {
            get => _diProtectionBar;
            set => _diProtectionBar = value;
        }
        public ALID ALID_EMS
        {
            get => _alid_EMS;
            set => _alid_EMS = value;
        }
        public ALID ALID_DoorOpen
        {
            get => _alid_DoorOpen;
            set => _alid_DoorOpen = value;
        }
        public ALID ALID_LightCurtain
        {
            get => _alid_LightCurtain;
            set => _alid_LightCurtain = value;
        }
        public ALID ALID_ProtectionBar
        {
            get => _alid_ProtectionBar;
            set => _alid_ProtectionBar = value;
        }
        #endregion
        public Certification(string id, IEngineer engineer)
        {
            m_RNR = new Run_RNR(this);
            base.InitBase(id, engineer);
            InitThreadCheck();
        }

        public override void RunTree(Tree tree)
        {
            m_bUseCurtain = tree.Set(m_bUseCurtain, m_bUseCurtain, "Light Curtain", "Use Light Curtain");
        }
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetAxis(ref _axisAll[0], this, "Tape Stage Rotate");
            p_sInfo = m_toolBox.GetAxis(ref _axisAll[1], this, "Tape Cartridge X");
            p_sInfo = m_toolBox.GetAxis(ref _axisAll[2], this, "Loadport");
            p_sInfo = m_toolBox.GetAxis(ref _axisAll[3], this, "Unloadport");
            p_sInfo = m_toolBox.GetAxis(ref _axisAll[4], this, "Individual Elevator");
            p_sInfo = m_toolBox.GetAxis(ref _axisAll[5], this, "Vaccum Arm Width");
            p_sInfo = m_toolBox.GetAxis(ref _axisAll[6], this, "Vaccum Arm X");
            p_sInfo = m_toolBox.GetAxis(ref _axisAll[7], this, "Loader Plate X");
            p_sInfo = m_toolBox.GetAxis(ref _axisAll[8], this, "Picker X");
            p_sInfo = m_toolBox.GetAxis(ref _axisAll[9], this, "Picker Z");
            p_sInfo = m_toolBox.GetAxis(ref _axisAll[10], this, "Loader Pusher X");

            p_sInfo = m_toolBox.GetDIO(ref _diEMS, this, "EMS");

            p_sInfo = m_toolBox.GetDIO(ref _diAllDoorLock, this, "All Door Lock");

            p_sInfo = m_toolBox.GetDIO(ref _diDoorLock[0], this, "Main Panel Top left Door Lock");
            p_sInfo = m_toolBox.GetDIO(ref _diDoorLock[1], this, "Main Panel Bottom left Door Lock");
            p_sInfo = m_toolBox.GetDIO(ref _diDoorLock[2], this, "Main Panel Door");

            p_sInfo = m_toolBox.GetDIO(ref _diLightCurtain[0], this, "UnloadPort Light Curtain");
            p_sInfo = m_toolBox.GetDIO(ref _diLightCurtain[1], this, "LoadPort Light Curtain");
            p_sInfo = m_toolBox.GetDIO(ref _diProtectionBar, this, "Protection Bar");

            p_sInfo = m_toolBox.GetDIO(ref _doDoorLock, this, "Door Lock");

            _alid_EMS = m_gaf.GetALID(this, "EMS", "EMS ERROR");
            _alid_DoorOpen = m_gaf.GetALID(this, "DOOR", "DOOR ERROR");
            _alid_LightCurtain = m_gaf.GetALID(this, "Light Curtain", "LIGHT CURTAIN ERROR");
            _alid_ProtectionBar = m_gaf.GetALID(this, "Protection Bar", "PROTECTION BAR ERROR");
            m_RNR.GetTools(m_toolBox, bInit);
        }
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_RNR(this), true, "Run RNR");
        }
        public override void Reset()
        {
            //문열려있으면 리셋못하게막자
            //EMS 눌려있으면 풀어달라고 알람띄우자
            base.Reset();
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

        void InitThreadCheck()
        {
            m_threadCheck = new Thread(new ThreadStart(RunThreadCheck));
            m_threadCheck.Start();
        }
        void RunThreadCheck()
        {
            //m_bThreadCheck = true;
            //Thread.Sleep(5000);
            //while (m_bThreadCheck)
            //{
            //    //Thread.Sleep(5000);
            //    //this.p_eState = eState.Error;
            //    //EQ.p_bStop = true;
            //    //Debug.WriteLine("Run Thread EQ Stop");

            //    Thread.Sleep(10);

            //    if (_diEMS.p_bIn)
            //    {
            //        this.p_eState = eState.Error;
            //        EQ.p_bStop = true;
            //        _alid_EMS.Run(_diEMS.p_bIn, "Please Check the Emergency Buttons");
            //    }
            //    for (int i = 0; i < _diDoorLock.Length - 1; i++)
            //    {
            //        if (_diDoorLock[i].p_bIn)
            //        {
            //            this.p_eState = eState.Error;
            //            EQ.p_bStop = true;
            //            string strDoor = " ";
            //            if (i == 0)
            //                strDoor = "Main Panel Top Left Door";
            //            if (i == 1)
            //                strDoor = "Main Panel Bottom Left Door";
            //            if (i == 2)
            //                continue;
            //            //strDoor = "Main Panel Door";

            //            _alid_DoorOpen.Run(_diDoorLock[i].p_bIn, strDoor + "Opened");
            //        }
            //    }
            //    if (m_bUseCurtain)
            //    {
            //        for (int i = 0; i < _diLightCurtain.Length; i++)
            //        {
            //            if (_diLightCurtain[i].p_bIn)
            //            {
            //                this.p_eState = eState.Error;
            //                EQ.p_bStop = true;
            //                string strCurtain = " ";
            //                if (i == 0)
            //                    strCurtain = "Unloadport Light Curtain Error";
            //                if (i == 1)
            //                    strCurtain = "Loadport Light Curatin Error";


            //                _alid_LightCurtain.Run(_diLightCurtain[i].p_bIn, strCurtain);
            //                //Thread.Sleep(5000);

            //            }
            //        }
            //    }
            //    if (_diProtectionBar.p_bIn)
            //    {
            //        this.p_eState = eState.Error;
            //        EQ.p_bStop = true;
            //        _alid_ProtectionBar.Run(_diProtectionBar.p_bIn, "Protection Bar Error");
            //    }

            //    if (EQ.IsStop())
            //    {
            //        foreach (Axis axis in AxisAll)
            //        {
            //            axis.StopAxis();
            //        }
            //        p_eState = eState.Error;
            //    }
            //}
        }

        public class Run_RNR : ModuleRunBase
        {
            Certification m_module;
            int m_nRepeatCnt = 10;
            public Run_RNR(Certification module)
            {
                m_module = module;
                InitModuleRun(module);
            }
            public void GetTools(ToolBox toolBox, bool bInit)
            {

            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_nRepeatCnt = tree.Set(m_nRepeatCnt, m_nRepeatCnt, "Repeat", "Axis Repeat Count", bVisible);
            }
            public override ModuleRunBase Clone()
            {
                Run_RNR run = new Run_RNR(m_module);
                run.m_nRepeatCnt = m_nRepeatCnt;
                return run;
            }
            public override string Run()
            {
                m_module.DODoorLock.Write(true);
                for (int nRepeat = 0; nRepeat < m_nRepeatCnt; nRepeat++)
                {
                    //Thread.Sleep(1000);
                    if (EQ.IsStop())
                        return "EQ Stop";
                    for (int i = 0; i < m_module.AxisAll.Length; i++)
                    {
                        if (i == 2 || i == 3)
                        {
                            //m_module.AxisAll[i].StartMove(2500);
                            continue;
                        }

                        m_module.AxisAll[i].StartHome();
                        if (i == 0)
                            Debug.WriteLine("Axis" + i.ToString() + "Home");
                        else
                            Debug.WriteLine("Axis" + (i + 1).ToString() + "Home");
                    }
                    for (int i = 0; i < m_module.AxisAll.Length; i++)
                    {
                        //if (i == 2 || i == 3)
                        //    continue;

                        m_module.AxisAll[i].WaitReady();
                        if (i == 0)
                            Debug.WriteLine("Axis" + i.ToString() + "Home");
                        else
                            Debug.WriteLine("Axis" + (i + 1).ToString() + "Home Done");

                    }

                    //m_module.Run(m_module.AxisAll[0].StartMove(500000));
                    //m_module.Run(m_module.AxisAll[1].StartMove(150000));
                    ////m_module.Run(m_module.AxisAll[2].StartMove(4500));
                    ////m_module.Run(m_module.AxisAll[3].StartMove(4500));
                    //m_module.Run(m_module.AxisAll[4].StartMove(150000));
                    //m_module.Run(m_module.AxisAll[5].StartMove(20000));
                    //m_module.Run(m_module.AxisAll[6].StartMove(40000));
                    //m_module.Run(m_module.AxisAll[7].StartMove(40000));
                    //m_module.Run(m_module.AxisAll[8].StartMove(150000));
                    //m_module.Run(m_module.AxisAll[9].StartMove(20000));
                    //m_module.Run(m_module.AxisAll[10].StartMove(150000));

                    //m_module.Run(m_module.AxisAll[0].WaitReady());
                    //Debug.WriteLine("Axis 1" + "Move Done");
                    //m_module.Run(m_module.AxisAll[1].WaitReady());
                    //Debug.WriteLine("Axis 2" + "Move Done");
                    //m_module.Run(m_module.AxisAll[4].WaitReady());
                    //Debug.WriteLine("Axis 5" + "Move Done");
                    //m_module.Run(m_module.AxisAll[5].WaitReady());
                    //Debug.WriteLine("Axis 6" + "Move Done");
                    //m_module.Run(m_module.AxisAll[6].WaitReady());
                    //Debug.WriteLine("Axis 7" + "Move Done");
                    //m_module.Run(m_module.AxisAll[7].WaitReady());
                    //Debug.WriteLine("Axis 8" + "Move Done");
                    //m_module.Run(m_module.AxisAll[8].WaitReady());
                    //Debug.WriteLine("Axis 9" + "Move Done");
                    //m_module.Run(m_module.AxisAll[9].WaitReady());
                    //Debug.WriteLine("Axis 10" + "Move Done");
                    //m_module.Run(m_module.AxisAll[10].WaitReady());
                    //Debug.WriteLine("Axis 11" + "Move Done");

                    //Debug.WriteLine(nRepeat.ToString() + "번");    
                    //m_module.Run(m_module.AxisAll[0].StartMove(1000));
                    //m_module.Run(m_module.AxisAll[1].StartMove(1000));
                    //m_module.Run(m_module.AxisAll[4].StartMove(1000));
                    //m_module.Run(m_module.AxisAll[5].StartMove(1000));
                    //m_module.Run(m_module.AxisAll[6].StartMove(1000));
                    //m_module.Run(m_module.AxisAll[7].StartMove(1000));
                    //m_module.Run(m_module.AxisAll[8].StartMove(1000));
                    //m_module.Run(m_module.AxisAll[9].StartMove(1000));
                    //m_module.Run(m_module.AxisAll[10].StartMove(1000));

                    //m_module.Run(m_module.AxisAll[0].WaitReady());
                    //m_module.Run(m_module.AxisAll[1].WaitReady());
                    //m_module.Run(m_module.AxisAll[4].WaitReady());
                    //m_module.Run(m_module.AxisAll[5].WaitReady());
                    //m_module.Run(m_module.AxisAll[6].WaitReady());
                    //m_module.Run(m_module.AxisAll[7].WaitReady());
                    //m_module.Run(m_module.AxisAll[8].WaitReady());
                    //m_module.Run(m_module.AxisAll[9].WaitReady());
                    //m_module.Run(m_module.AxisAll[10].WaitReady());

                }
                //반복할거임
                //시작하면 Door lock I / O켜
                //EMS 눌리면 Alarm
                //문열리면 축 정지 Alarm
                //m_module의 I / O를보고 Alarm띄우면 됨
                return "OK";
            }
        }

    }
}
