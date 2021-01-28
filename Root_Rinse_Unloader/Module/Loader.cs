﻿using RootTools;
using RootTools.Control;
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
            p_sInfo = m_toolBox.Get(ref m_axis, this, "Loader");
            p_sInfo = m_toolBox.Get(ref m_dioPickerDown, this, "PickerDown", "Up", "Down");
            foreach (Picker picker in m_aPicker) picker.GetTools(m_toolBox);
            if (bInit)
            {
                InitPos();
            }
        }
        #endregion

        #region Picker
        public class Picker
        {
            public DIO_IO m_dioVacuum;
            public DIO_O m_doBlow;
            public void GetTools(ToolBox toolBox)
            {
                m_loader.p_sInfo = toolBox.Get(ref m_dioVacuum, m_loader, m_id + ".Vacuum");
                m_loader.p_sInfo = toolBox.Get(ref m_doBlow, m_loader, m_id + ".Blow");
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
                if (m_roller.m_bExist[n]) m_aPicker[n].m_dioVacuum.Write(bOn);
                Thread.Sleep(200); 
            }
            if (bOn)
            {
                StopWatch sw = new StopWatch();
                int msVac = (int)(1000 * m_secVac);
                int nExist = 4;
                int nVac = 0;
                while (nExist != nVac)
                {
                    Thread.Sleep(10);
                    nExist = 0;
                    nVac = 0; 
                    for (int n = 0; n < 4; n++)
                    {
                        if (m_roller.m_bExist[n])
                        {
                            nExist++;
                            if (m_aPicker[n].m_dioVacuum.p_bIn) nVac++; 
                        }
                    }
                    if (sw.ElapsedMilliseconds > msVac) return "Run Vacuum Timeout"; 
                }
            }
            else
            {
                foreach (Picker picker in m_aPicker) picker.m_doBlow.Write(true);
                Thread.Sleep((int)(1000 * m_secBlow));
                foreach (Picker picker in m_aPicker) picker.m_doBlow.Write(false);
            }
            return "OK";
        }

        void RunTreePicker(Tree tree)
        {
            m_secVac = tree.Set(m_secVac, m_secVac, "Vacuum", "Vacuum On Timeout (sec)");
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
            return m_dioPickerDown.WaitDone();
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
            m_axis.StartMove(ePos);
            return m_axis.WaitReady();
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
            if (Run(RunPickerDown(true))) return p_sInfo;
            if (Run(RunVacuum(true))) return p_sInfo;
            if (Run(RunPickerDown(false))) return p_sInfo;
            m_roller.p_eStep = Roller.eStep.Empty;
            if (Run(m_roller.RunRotate(true))) return p_sInfo;
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
                if (Run(RunPickerDown(true))) return p_sInfo;
                if (Run(RunVacuum(false))) return p_sInfo;
                if (Run(RunPickerDown(false))) return p_sInfo;
                m_storage.StartMoveStackReady();
                if (Run(MoveLoader(ePos.Roller))) return p_sInfo;
                return "OK";
            }
            finally
            {
                if (RunPickerDown(false) == "OK") MoveLoader(ePos.Roller); 
            }
        }

        public string RunRun()
        {
            if (m_bPickersetMode) return "OK";
            return p_bVacuum ? RunUnload() : RunLoad();
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
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            m_axis.StartMove(ePos.Roller); 
            return p_sInfo;
        }

        public override void Reset()
        {
            base.Reset();
        }
        #endregion

        #region PickerSet
        public bool m_bPickersetMode = false;
        string RunPickerSet()
        {
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
                RunPickerDown(false);
                MoveLoader(ePos.Roller);
                m_bPickersetMode = false;
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
                if (m_swPickerSet.ElapsedMilliseconds > 5000) return ePickerSet.Stop;
            }
            return (m_swPickerSet.ElapsedMilliseconds < 1000) ? ePickerSet.UpDown : ePickerSet.Vacuum;
        }

        public string m_sFilePickerSet = "";
        void RunTreePickerSet(Tree tree)
        {
            m_sFilePickerSet = tree.SetFile(m_sFilePickerSet, m_sFilePickerSet, "RunRinse_Loader", "ModuleRun", "PickerSet ModuleRun File");
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
        Roller m_roller;
        public Loader(string id, IEngineer engineer, RinseU rinse, Storage storage, Roller roller)
        {
            p_id = id;
            m_rinse = rinse;
            m_storage = storage;
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
        ModuleRunBase m_runPickerSet;
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Move(this), false, "Move Axis");
            AddModuleRunList(new Run_Load(this), false, "Load Strip");
            AddModuleRunList(new Run_Unload(this), false, "Unload Strip");
            m_runRun = AddModuleRunList(new Run_Run(this), false, "Move Strip");
            m_runPickerSet = AddModuleRunList(new Run_PickerSet(this), false, "PickerSet");
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
