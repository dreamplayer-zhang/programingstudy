using RootTools;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_Rinse_Unloader.Module
{
    public class Loader : ModuleBase
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetAxis(ref m_axis, this, "Loader");
            p_sInfo = m_toolBox.GetDIO(ref m_dioPickerDown, this, "PickerDown", "Up", "Down");
            p_sInfo = m_toolBox.GetDIO(ref m_diPickerSet, this, "PickerSet");
            foreach (Picker picker in m_aPicker) picker.GetTools(m_toolBox, bInit);
            if (bInit)
            {
                InitALID();
                InitPos();
            }
        }
        #endregion

        #region GAF
        ALID m_alidPickerDown;
        ALID m_alidRollerStripCheck;
        void InitALID()
        {
            m_alidPickerDown = m_gaf.GetALID(this, "PickerDown", "Picker Up & Down Error");
            m_alidRollerStripCheck = m_gaf.GetALID(this, "Roller Strip Check", "Roller Strip Check Error");
        }
        #endregion

        #region Picker
        public class Picker
        {
            public DIO_IO m_dioVacuum;
            public DIO_O m_doBlow;
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_loader.p_sInfo = toolBox.GetDIO(ref m_dioVacuum, m_loader, m_id + ".Vacuum");
                m_loader.p_sInfo = toolBox.GetDIO(ref m_doBlow, m_loader, m_id + ".Blow");
                if (bInit)
                {
                    m_dioVacuum.Write(false);
                    m_doBlow.Write(false);
                }
            }

            string m_id;
            Loader m_loader;
            public Picker(string id, Loader loader)
            {
                m_id = id;
                m_loader = loader;
            }
        }

        List<Picker> m_aPicker = new List<Picker>();
        void InitPickers()
        {
            for (int n = 0; n < 4; n++) m_aPicker.Add(new Picker("Picker" + n.ToString(), this));
        }

        bool _bVacuum = false;
        public bool p_bVacuum
        {
            get { return _bVacuum; }
            set
            {
                if (_bVacuum == value) return;
                _bVacuum = value;
                OnPropertyChanged();
            }
        }

        double m_secVac = 2;
        double m_secBlow = 0.5;
        public string RunVacuum(bool bOn)
        {
            p_bVacuum = bOn;
            for (int n = 0; n < 4; n++)
            {
                m_aPicker[n].m_dioVacuum.Write(bOn);
            }
            //if (!bOn) Thread.Sleep((int)(1000 * m_secBlow));
            Thread.Sleep(200);
            //if (bOn) Thread.Sleep((int)(1000 * m_secVac));
            //else
            //{
            //    foreach (Picker picker in m_aPicker) picker.m_doBlow.Write(true);
            //    Thread.Sleep((int)(1000 * m_secBlow));
            //    foreach (Picker picker in m_aPicker) picker.m_doBlow.Write(false);
            //}
            if (!bOn)
            {
                foreach (Picker picker in m_aPicker) picker.m_doBlow.Write(true);
                Thread.Sleep((int)(1000 * m_secBlow));
                foreach (Picker picker in m_aPicker) picker.m_doBlow.Write(false);
            }
            return "OK";
        }

        void RunTreePicker(Tree tree)
        {
            m_secVac = tree.Set(m_secVac, m_secVac, "Vacuum", "Vacuum On Time Delay (sec)");
            m_secBlow = tree.Set(m_secBlow, m_secBlow, "Blow", "Blow Time (sec)");
        }
        #endregion

        #region Picker Up & Down
        public bool m_bPickerDown = false;
        DIO_I2O2 m_dioPickerDown;
        public string RunPickerDown(bool bDown)
        {
            m_bPickerDown = bDown;
            m_dioPickerDown.Write(bDown);
            if (!EQ.IsStop())
            {
                string sRun = m_dioPickerDown.WaitDone();
                m_alidPickerDown.p_bSet = (sRun != "OK");
                return sRun;
            }
            return "OK";
        }
        #endregion

        #region Axis Move
        Axis m_axis;

        public enum ePos
        {
            Roller,
            Stotage,
        }
        void InitPos()
        {
            m_axis.AddPos(Enum.GetNames(typeof(ePos)));
        }

        public string MoveLoader(ePos ePos)
        {
            if ((m_rail.m_dioPusherDown.p_bOut == false) || (m_rail.m_dioPusherDown.p_bDone == false))  return "Check Pusher Down";
            if ((m_dioPickerDown.p_bOut) || (m_dioPickerDown.p_bDone == false)) return "Check Picker Down";
            m_axis.StartMove(ePos);
            return m_axis.WaitReady();
        }
        public bool IsLoaderDanger()
        {
            if (IsLoaderDanger(m_axis.p_posCommand)) return true;
            return IsLoaderDanger(m_axis.m_posDst);
        }

        bool IsLoaderDanger(double fPos)
        {
            double dPos = Math.Abs(fPos - m_axis.GetPosValue(ePos.Stotage));
            return (dPos < Math.Abs(fPos - m_axis.GetPosValue(ePos.Roller)));
        }
        #endregion

        #region Tact Time
        double _secTact = 0;
        public double p_secTact
        {
            get { return _secTact; }
            set
            {
                _secTact = value;
                OnPropertyChanged();
            }
        }

        double _secAveTact = 0;
        public double p_secAveTact
        {
            get { return _secAveTact; }
            set
            {
                _secAveTact = value;
                OnPropertyChanged();
            }
        }

        List<double> m_aTact = new List<double>();
        StopWatch m_swTact = new StopWatch();
        void CheckTact()
        {
            double secTact = m_swTact.ElapsedMilliseconds / 1000.0;
            m_swTact.Start();
            m_aTact.Add(secTact);
            if (m_aTact.Count <= 1) return;
            p_secTact = secTact;
            double secSum = 0;
            for (int n = 1; n < m_aTact.Count; n++) secSum += m_aTact[n];
            p_secAveTact = secSum / (m_aTact.Count - 1);
            while (m_aTact.Count > 4) m_aTact.RemoveAt(0);
        }
        #endregion

        #region Run Load
        public string RunLoad()
        {
            if (m_rinse.p_eMode != RinseU.eRunMode.Stack) return "Run mode is not Stack";
            if (Run(RunPickerDown(false))) return p_sInfo;
            if (Run(MoveLoader(ePos.Roller))) return p_sInfo;
            while (m_roller.p_eStep != Roller.eStep.Picker)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop"; 
            }
            if (Run(RunPickerLoad())) return p_sInfo; 
            if (RunCheckStrip() != "OK")
            {
                if (Run(RunPickerLoad())) return p_sInfo;
            }
            if (RunCheckStrip() != "OK")
            {
                m_alidRollerStripCheck.p_bSet = true;
                return "Load Strip Error"; 
            }
            m_roller.p_eStep = Roller.eStep.Empty;
            if (Run(m_roller.RunRotate(true))) return p_sInfo;
            return "OK";
        }

        string RunPickerLoad()
        {
            if (Run(RunVacuum(true))) return p_sInfo;
            if (Run(RunPickerDown(true))) return p_sInfo;
            Thread.Sleep((int)(1000 * m_secVac));
            if (Run(RunPickerDown(false))) return p_sInfo;
            return "OK"; 
        }

        public string RunUnload()
        {
            try
            {
                if (m_rinse.p_eMode != RinseU.eRunMode.Stack) return "Run mode is not Stack";
                if (Run(RunPickerDown(false))) return p_sInfo;
                while (m_storage.IsBusy())
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                }
                if (Run(MoveLoader(ePos.Stotage))) return p_sInfo;
                if (Run(RunVacuum(false))) return p_sInfo;
                m_rinse.CheckTact(); 
                if (Run(MoveLoader(ePos.Roller))) return p_sInfo;
                m_storage.StartMoveStackReady();
                return "OK";
            }
            finally
            {
                if (RunPickerDown(false) == "OK") MoveLoader(ePos.Roller); 
            }
        }

        public string RunRun()
        {
            if (EQ.p_bPickerSet) return "OK";
            return p_bVacuum ? RunUnload() : RunLoad();
        }

        public string RunCheckStrip()
        {
            string sResult = "OK";
            foreach (Roller.Line line in m_roller.m_aLine)
            {
                if (line.m_diCheck[2].p_bIn) return "Roller Check Strip";
            }
            return sResult;
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
            RunPickerDown(false);
            RunVacuum(false); 
            p_sInfo = base.StateHome();
            //p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            //m_axis.StartMove(ePos.Roller); 
            if(p_sInfo == "OK")
            {
                Thread.Sleep(200);
                //if (Run(m_axis.StartMove(ePos.Roller)))
                //{
                //    p_eState = eState.Error;
                //    return p_sInfo;
                //}
                p_eState = eState.Ready;
            }
            return p_sInfo;
        }

        public override void Reset()
        {
            base.Reset();
        }
        #endregion

        #region PickerSet
        string RunPickerSet()
        {
            EQ.p_bPickerSet = true; 
            try
            {
                if (Run(MoveLoader(ePos.Roller))) return p_sInfo;
                while (true)
                {
                    switch (CheckPickerSet())
                    {
                        case ePickerSet.Stop: return "OK";
                        case ePickerSet.UpDown:
                            if (Run(RunPickerDown(!m_bPickerDown))) return p_sInfo;
                            break;
                        case ePickerSet.Vacuum:
                            if (Run(RunVacuum(!p_bVacuum))) return p_sInfo;
                            break;
                    }
                }
            }
            finally
            {
                RunVacuum(false);
                RunPickerDown(false);
                MoveLoader(ePos.Roller);
                EQ.p_bPickerSet = false;
            }
        }

        enum ePickerSet
        {
            UpDown,
            Vacuum,
            Stop
        }

        DIO_I m_diPickerSet;
        StopWatch m_swPickerSet = new StopWatch();
        ePickerSet CheckPickerSet()
        {
            while (m_diPickerSet.p_bIn == false)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return ePickerSet.Stop;
            }
            m_swPickerSet.Start();
            while (m_diPickerSet.p_bIn)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return ePickerSet.Stop;
                if (m_swPickerSet.ElapsedMilliseconds > 3000) return ePickerSet.Stop;
            }
            return (m_swPickerSet.ElapsedMilliseconds < 600) ? ePickerSet.UpDown : ePickerSet.Vacuum;
        }

        public string m_sFilePickerSet = "";
        void RunTreePickerSet(Tree tree)
        {
            m_sFilePickerSet = tree.SetFile(m_sFilePickerSet, m_sFilePickerSet, "RunRinse_Unloader", "ModuleRun", "PickerSet ModuleRun File");
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreePicker(tree.GetTree("Picker", false));
            RunTreePickerSet(tree.GetTree("PickerSet"));
        }
        #endregion

        RinseU m_rinse;
        Storage m_storage;
        Rail m_rail; 
        Roller m_roller;
        public Loader(string id, IEngineer engineer, RinseU rinse, Storage storage, Rail rail, Roller roller)
        {
            p_id = id;
            m_rinse = rinse;
            m_storage = storage;
            m_rail = rail; 
            m_roller = roller;
            InitPickers();
            InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region StartRun
        public void StartRun()
        {
            switch (m_rinse.p_eMode)
            {
                case RinseU.eRunMode.Magazine:
                    RunPickerDown(false);
                    MoveLoader(ePos.Roller);
                    break;
                case RinseU.eRunMode.Stack:
                    StartRun(m_runRun.Clone());
                    break;
            }
        }
        #endregion

        #region ModuleRun
        ModuleRunBase m_runRun;
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Move(this), false, "Move Axis");
            AddModuleRunList(new Run_Load(this), false, "Load Strip");
            AddModuleRunList(new Run_Unload(this), false, "Unload Strip");
            m_runRun = AddModuleRunList(new Run_Run(this), false, "Move Strip");
            AddModuleRunList(new Run_PickerSet(this), false, "PickerSet");
        }

        public class Run_Move : ModuleRunBase
        {
            Loader m_module;
            public Run_Move(Loader module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            ePos m_ePos = ePos.Roller; 
            public override ModuleRunBase Clone()
            {
                Run_Move run = new Run_Move(m_module);
                run.m_ePos = m_ePos; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_ePos = (ePos)tree.Set(m_ePos, m_ePos, "Pos", "Axis Position", bVisible);
            }

            public override string Run()
            {
                return m_module.MoveLoader(m_ePos);
            }
        }

        public class Run_Load : ModuleRunBase
        {
            Loader m_module;
            public Run_Load(Loader module)
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
            Loader m_module;
            public Run_Unload(Loader module)
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

        public class Run_Run : ModuleRunBase
        {
            Loader m_module;
            public Run_Run(Loader module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Run run = new Run_Run(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                while (EQ.p_eState == EQ.eState.Run)
                {
                    Thread.Sleep(10);
                    if (m_module.Run(m_module.RunRun())) return p_sInfo;
                }
                return "OK";
            }
        }

        public class Run_PickerSet : ModuleRunBase
        {
            Loader m_module;
            public Run_PickerSet(Loader module)
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
