using RootTools;
using RootTools.Control;
using RootTools.GAFs;
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
            p_sInfo = m_toolBox.GetAxis(ref m_axis, this, "Loader");
            p_sInfo = m_toolBox.GetDIO(ref m_dioPickerDown, this, "PickerDown", "Up", "Down", true, true);
            p_sInfo = m_toolBox.GetDIO(ref m_dioPickerSet, this, "PickerSet");
            foreach (Picker picker in m_aPicker) picker.GetTools(m_toolBox);
            if (bInit)
            {
                InitALID();
                InitPos();
            }
        }
        #endregion

        #region GAF
        ALID m_alidPickerDown;
        ALID m_alidPickerDrop;
        void InitALID()
        {
            m_alidPickerDown = m_gaf.GetALID(this, "PickerDown", "Picker Up & Down Error");
            m_alidPickerDrop = m_gaf.GetALID(this, "PickerDrop", "Picker Drop Strip");
        }
        #endregion

        #region Picker
        public class Picker
        {
            public DIO_IO m_dioVacuum;
            public DIO_O m_doBlow;
            public void GetTools(ToolBox toolBox)
            {
                m_loader.p_sInfo = toolBox.GetDIO(ref m_dioVacuum, m_loader, m_id + ".Vacuum");
                m_loader.p_sInfo = toolBox.GetDIO(ref m_doBlow, m_loader, m_id + ".Blow");
            }

            public bool IsDrop()
            {
                if (m_dioVacuum.p_bOut == false) return false;
                return (m_dioVacuum.p_bIn == false); 
            }

            public string m_id;
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
            try
            {
                p_bVacuum = bOn;
                for (int n = 0; n < 4; n++)
                {
                    m_aPicker[n].m_dioVacuum.Write(bOn && m_storage.m_stack.m_diCheck[n].p_bIn);
                }
                if (bOn)
                {
                    Thread.Sleep((int)(1000 * m_secVac));
                    foreach (Picker picker in m_aPicker) picker.m_dioVacuum.Write(picker.m_dioVacuum.p_bIn);
                }
                else
                {
                    m_bCheckStrip = false;
                    foreach (Picker picker in m_aPicker) picker.m_doBlow.Write(true);
                    Thread.Sleep((int)(1000 * m_secBlow));
                }
                foreach (Picker picker in m_aPicker) picker.m_doBlow.Write(false);
                return "OK";
            }
            finally
            {
                foreach (Picker picker in m_aPicker) picker.m_doBlow.Write(false);
            }
        }

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

        int m_nShake = 0;
        double[] m_secShake = new double[2] { 0.3, 0.2 }; 
        public string RunShakeUp()
        {
            try
            {
                for (int n = 0; n < m_nShake; n++)
                {
                    m_dioPickerDown.Write(false);
                    Thread.Sleep((int)(1000 * m_secShake[0]));
                    m_dioPickerDown.Write(true);
                    Thread.Sleep((int)(1000 * m_secShake[1]));
                }
                return RunPickerDown(false);
            }
            finally
            {
                string sSend = ""; 
                foreach (Picker picker in m_aPicker)
                {
                    if (picker.m_dioVacuum.p_bIn == false) picker.m_dioVacuum.Write(false);
                    sSend += picker.m_dioVacuum.p_bIn ? 'O' : '.'; 
                }
                Thread.Sleep(100); 
            }
        }

        void RunTreePicker(Tree tree)
        {
            m_secVac = tree.Set(m_secVac, m_secVac, "Vacuum", "Vacuum Sensor Wait (sec)");
            m_secBlow = tree.Set(m_secBlow, m_secBlow, "Blow", "Blow Time (sec)");
            m_nShake = tree.Set(m_nShake, m_nShake, "Shake", "Shake Up Count");
            RunTreePickerShake(tree.GetTree("Shake Delay", true, m_nShake > 0), m_nShake > 0); 
        }

        void RunTreePickerShake(Tree tree, bool bVisible)
        {
            m_secShake[0] = tree.Set(m_secShake[0], m_secShake[0], "Up", "Shake Up Time (sec)", bVisible);
            m_secShake[1] = tree.Set(m_secShake[1], m_secShake[1], "Down", "Shake Down Time (sec)", bVisible);
        }
        #endregion

        #region Axis Move
        Axis m_axis; 
        public enum ePos
        {
            Stotage,
            Rail,
            Roller
        }
        void InitPos()
        {
            m_axis.AddPos(Enum.GetNames(typeof(ePos)));
        }

        public string MoveLoader(ePos ePos, bool bWait = true)
        {
            if ((ePos == ePos.Stotage) && m_storage.IsHighPos()) return "Check Storage Position"; 
            m_axis.StartMove(ePos);
            if (bWait == false) return "OK"; 
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
            return (dPos < Math.Abs(fPos - m_axis.GetPosValue(ePos.Rail))); 
        }
        #endregion

        #region PickerSet
        DIO_IO m_dioPickerSet;
        string RunPickerSet()
        {
            int n = 0;
            bool bOn = true; 
            m_dioPickerSet.Write(bOn); 
            while (m_dioPickerSet.p_bIn == false)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
                n++; 
                if (n >= 50)
                {
                    n = 0;
                    bOn = !bOn;
                    m_dioPickerSet.Write(bOn);
                }
            }
            m_dioPickerSet.Write(false);
            return "OK"; 
        }

        public string m_sFilePickerSet = "";
        void RunTreePickerSet(Tree tree)
        {
            m_sFilePickerSet = tree.SetFile(m_sFilePickerSet, m_sFilePickerSet, "RunRinse_Loader", "ModuleRun", "PickerSet ModuleRun File"); 
        }
        #endregion

        #region Run Load & Unload
        public string RunLoad()
        {
            if (m_storage.m_stack.p_bCheck == false)
            {
                Thread.Sleep(10000); 
                if (Run(m_roller.RunRotate(false))) return p_sInfo;
                m_rinse.SendFinish(); 
                EQ.p_eState = EQ.eState.Ready;
                return "OK";
            }
            if (m_rinse.p_eMode != RinseL.eRunMode.Stack) return "Run mode is not Stack"; 
            if (Run(RunPickerDown(false))) return p_sInfo;
            if (Run(MoveLoader(ePos.Stotage))) return p_sInfo;
            if (m_storage.p_bIsEnablePick == false)
            {
                if (Run(m_storage.MoveStackReady())) return p_sInfo;
            }
            if (Run(RunPickerDown(true))) return p_sInfo;
            Thread.Sleep(200);
            if (Run(RunVacuum(true))) return p_sInfo;
            Thread.Sleep(200);
            //m_storage.StartStackDown();
            if (Run(m_storage.MoveStack_Down())) return p_sInfo;
            Thread.Sleep(100);
            if (Run(RunShakeUp())) return p_sInfo;
            m_bCheckStrip = true; 
            if (Run(MoveLoader(ePos.Roller))) return p_sInfo;
            return "OK";
        }

        public string RunUnload()
        {
            if (m_rinse.p_eMode != RinseL.eRunMode.Stack) return "Run mode is not Stack";
            if (m_roller.IsEmpty() == false) return "OK";
            if (Run(RunPickerDown(false))) return p_sInfo;
            if (Run(MoveLoader(ePos.Roller))) return p_sInfo;
            if (Run(m_roller.RunRotate(false))) return p_sInfo;
            Thread.Sleep(100);
            m_bCheckStrip = false;
            if (Run(RunPickerDown(true))) return p_sInfo;
            Thread.Sleep(200);
            if (Run(RunVacuum(false))) return p_sInfo;
            if (Run(RunPickerDown(false))) return p_sInfo;
            m_rinse.CheckTact(); 
            if (Run(m_roller.RunRotate(true))) return p_sInfo;
            if (m_storage.m_stack.p_bCheck) return "OK"; 
            if (Run(MoveLoader(ePos.Rail))) return p_sInfo;
            return "OK";
        }

        public string RunRun()
        {
            if (EQ.p_bPickerSet) return "OK";
            //if (m_rinse.p_eStateRinse != RinseL.eRinseRun.Run) return "Rinse State not Run";
            //if (m_rinse.p_eStateUnloader != EQ.eState.Run) return "Rinse Unloader State not Run";
            return p_bVacuum ? RunUnload() : RunLoad(); 
        }
        #endregion

        #region Thread Check
        bool m_bThreadCheck = false;
        Thread m_threadCheck; 
        void InitThreadCheck()
        {
            m_threadCheck = new Thread(new ThreadStart(RunThreadCheck));
            m_threadCheck.Start();
        }

        bool m_bCheckStrip = false; 
        void RunThreadCheck()
        {
            m_bThreadCheck = true;
            Thread.Sleep(1000); 
            while (m_bThreadCheck)
            {
                Thread.Sleep(10); 
                if (m_bCheckStrip)
                {
                    foreach (Picker picker in m_aPicker)
                    {
                        if (picker.IsDrop())
                        {
                            m_alidPickerDrop.p_bSet = true; 
                            picker.m_dioVacuum.Write(false); 
                            p_sInfo = picker.m_id + " : Picker drop Strip";
                            m_bCheckStrip = false; 
                        }
                    }
                }
            }
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
            RunVacuum(false); 
            base.Reset();
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreePicker(tree.GetTree("Picker"));
            RunTreePickerSet(tree.GetTree("PickerSet")); 
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
            InitThreadCheck(); 
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

        #region StartRun
        public void StartRun()
        {
            if (EQ.p_bPickerSet) return; 
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
            AddModuleRunList(new Run_Move(this), false, "Move Axis");
            AddModuleRunList(new Run_Load(this), false, "Load Strip");
            AddModuleRunList(new Run_Unload(this), false, "Unload Strip");
            m_runRun = AddModuleRunList(new Run_Run(this), true, "Run");
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
