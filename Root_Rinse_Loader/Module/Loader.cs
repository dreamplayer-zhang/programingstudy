using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_Rinse_Loader.Module
{
    public class Loader : ModuleBase
    {
        #region ToolBox

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axis, this, "Loader");
            p_sInfo = m_toolBox.Get(ref m_dioPickerDown, this, "PickerDown", "Up", "Down");
            p_sInfo = m_toolBox.Get(ref m_diPickerSet, this, "PickerSet");
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
            foreach (Picker picker in m_aPicker) picker.m_dioVacuum.Write(bOn);
            if (bOn)
            {
                Thread.Sleep((int)(1000 * m_secVac));
                p_bVacuum = true; 
                return "OK";
            }
            else
            {
                foreach (Picker picker in m_aPicker) picker.m_doBlow.Write(true);
                Thread.Sleep((int)(1000 * m_secBlow));
                foreach (Picker picker in m_aPicker) picker.m_doBlow.Write(false);
                p_bVacuum = false; 
            }
            return "OK";
        }

        void RunTreePicker(Tree tree)
        {
            m_secVac = tree.Set(m_secVac, m_secVac, "Vacuum", "Vacuum Sensor Wait (sec)");
            m_secBlow = tree.Set(m_secBlow, m_secBlow, "Blow", "Blow Time (sec)");
        }
        #endregion

        #region Picker Up & Down
        DIO_I2O2 m_dioPickerDown;
        public string RunPickerDown(bool bDown)
        {
            m_dioPickerDown.Write(bDown);
            return m_dioPickerDown.WaitDone();
        }
        #endregion

        #region Axis Move
        Axis m_axis; 

        public enum ePos
        {
            Stotage,
            Roller
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

        #region PickerSet
        public bool m_bPickersetMode = false;
        public string StartPickerSet()
        {
            StartRun(m_runPickerSet.Clone());
            return "OK";
        }

        string RunPickerSet()
        {
            try
            {
                if (Run(MoveLoader(ePos.Stotage))) return p_sInfo;
                bool bDown = false;
                bool bVacuum = false;
                while (true)
                {
                    switch (CheckPickerSet())
                    {
                        case ePickerSet.Stop: return "OK";
                        case ePickerSet.UpDown:
                            bDown = !bDown;
                            if (Run(RunPickerDown(bDown))) return p_sInfo;
                            break;
                        case ePickerSet.Vacuum:
                            bVacuum = !bVacuum;
                            if (Run(RunVacuum(bVacuum))) return p_sInfo;
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
                if (m_swPickerSet.ElapsedMilliseconds > 3000) return ePickerSet.Stop;
            }
            return (m_swPickerSet.ElapsedMilliseconds < 1000) ? ePickerSet.UpDown : ePickerSet.Vacuum;
        }
        #endregion

        #region Run Load & Unload
        public string RunLoad()
        {
            if (m_rinse.p_eMode != RinseL.eRunMode.Stack) return "Run mode is not Stack"; 
            if (Run(RunPickerDown(false))) return p_sInfo; 
            if (m_storage.p_bIsEnablePick == false)
            {
                if (Run(m_storage.StartMoveStackReady())) return p_sInfo;
                while (m_storage.IsBusy())
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                }
            }
            if (Run(MoveLoader(ePos.Stotage))) return p_sInfo;
            if (Run(RunPickerDown(true))) return p_sInfo;
            if (Run(RunVacuum(true))) return p_sInfo;
            if (Run(RunPickerDown(false))) return p_sInfo;
            if (Run(m_storage.StartMoveStackReady())) return p_sInfo;
            if (Run(MoveLoader(ePos.Roller))) return p_sInfo;
            return "OK";
        }

        public string RunUnload()
        {
            if (m_rinse.p_eMode != RinseL.eRunMode.Stack) return "Run mode is not Stack";
            if (Run(RunPickerDown(false))) return p_sInfo;
            if (Run(MoveLoader(ePos.Roller))) return p_sInfo;
            //forget Check Stip Exist -> Error ??
            if (Run(m_roller.RunRotate(false))) return p_sInfo;
            Thread.Sleep(100); //forget Delete
            if (Run(RunPickerDown(true))) return p_sInfo;
            if (Run(RunVacuum(false))) return p_sInfo;
            if (Run(RunPickerDown(false))) return p_sInfo;
            if (Run(m_roller.RunRotate(true))) return p_sInfo;
            return "OK";
        }

        public string RunRun()
        {
            if (m_bPickersetMode) return "OK";
            if (m_rinse.p_eStateRinse != RinseL.eRinseRun.Run) return "Rinse State not Run";
            if (m_rinse.p_eStateUnloader != EQ.eState.Run) return "Rinse Unloader State not Run";
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
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }

        public override void Reset()
        {
            base.Reset();
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreePicker(tree.GetTree("Picker", false));
        }
        #endregion

        RinseL m_rinse;
        Storage m_storage;
        Roller m_roller; 
        public Loader(string id, IEngineer engineer, RinseL rinse, Storage storage, Roller roller)
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
            if (m_bPickersetMode) return; 
            switch (m_rinse.p_eMode)
            {
                case RinseL.eRunMode.Magazine:
                    RunPickerDown(false);
                    MoveLoader(ePos.Roller);
                    break;
                case RinseL.eRunMode.Stack:
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
            AddModuleRunList(new Run_Load(this), false, "Load Strip");
            AddModuleRunList(new Run_Unload(this), false, "Unload Strip");
            m_runRun = AddModuleRunList(new Run_Run(this), true, "Run");
            m_runPickerSet = AddModuleRunList(new Run_PickerSet(this), false, "PickerSet");
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
