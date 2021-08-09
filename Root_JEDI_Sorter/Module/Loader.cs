using Root_JEDI_Sorter.Engineer;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_JEDI_Sorter.Module
{
    public class Loader : ModuleBase
    {
        #region Picker
        const int c_lPicker = 10;
        public class Picker : NotifyProperty
        {
            DIO_O m_doDown;
            DIO_I m_diUp;
            DIO_IO m_dioVacuum;
            DIO_O m_doBlow;
            public void GetTools(ToolBox toolBox, ModuleBase module, bool bInit)
            {
                toolBox.GetDIO(ref m_doDown, module, p_id + ".Down");
                toolBox.GetDIO(ref m_diUp, module, p_id + ".Up");
                toolBox.GetDIO(ref m_dioVacuum, module, p_id + ".Vacuum");
                toolBox.GetDIO(ref m_doBlow, module, p_id + ".Blow");
            }

            eResult _eResult = eResult.Empty; 
            public eResult p_eResult
            {
                get { return _eResult; }
                set
                {
                    _eResult = value;
                    OnPropertyChanged(); 
                }
            }

            public enum eState
            {
                Ready,
                Load,
                Loading,
                LoadFail,
                Unloading,
                UnloadFail
            }
            eState _eState = eState.Ready;
            public eState p_eState
            {
                get { return _eState; }
                set
                {
                    _eState = value;
                    OnPropertyChanged();
                }
            }

            public bool IsDone()
            {
                switch (p_eState)
                {
                    case eState.Loading: return false;
                    case eState.Unloading: return false;
                }
                return true;
            }

            public string CheckDone()
            {
                switch (p_eState)
                {
                    case eState.LoadFail:
                        p_eState = eState.Ready;
                        return p_id + " Load Fail";
                    case eState.UnloadFail:
                        p_eState = eState.Ready;
                        return p_id + " Unload Fail";
                }
                return "OK";
            }

            public string StartLoad(InfoTray infoTray, CPoint cpTray)
            {
                m_infoTray = infoTray;
                m_cpTray = cpTray; 
                if (p_eResult != eResult.Empty) return p_id + " Has InfoChip";
                if (p_eState != eState.Ready) return p_id + " State not Ready";
                p_eState = eState.Loading;
                return "OK";
            }

            public string StartUnload(InfoTray infoTray, CPoint cpTray)
            {
                m_infoTray = infoTray;
                m_cpTray = cpTray;
                if (p_eResult == eResult.Empty) return p_id + " Has not InfoChip";
                if (p_eState != eState.Load) return p_id + " State not Load";
                p_eState = eState.Unloading;
                return "OK";
            }

            bool m_bThread = false;
            Thread m_thread;
            void RunThread()
            {
                m_bThread = true;
                while (m_bThread)
                {
                    Thread.Sleep(10);
                    switch (p_eState)
                    {
                        case eState.Loading: p_eState = (RunLoadPicker() == "OK") ? eState.Load : eState.LoadFail; break;
                        case eState.Unloading: p_eState = (RunUnloadPicker() == "OK") ? eState.Ready : eState.UnloadFail; break;
                    }
                }
            }

            InfoTray m_infoTray;
            CPoint m_cpTray = new CPoint();  
            string RunLoadPicker()
            {
                try
                {
                    if (Run(RunUpDown(true))) return m_sInfo;
                    if (Run(RunVacuum(true))) return m_sInfo;
                    if (Run(RunUpDown(false))) return m_sInfo;
                    p_eResult = m_infoTray.m_aChip[m_cpTray.Y][m_cpTray.X];
                    m_infoTray.m_aChip[m_cpTray.Y][m_cpTray.X] = eResult.Empty;
                }
                finally { m_doDown.Write(false); }
                return "OK";
            }

            string RunUnloadPicker()
            {
                try
                {
                    if (Run(RunUpDown(true))) return m_sInfo;
                    if (Run(RunVacuum(false))) return m_sInfo;
                    if (Run(RunUpDown(false))) return m_sInfo;
                    m_infoTray.m_aChip[m_cpTray.Y][m_cpTray.X] = p_eResult;
                    p_eResult = eResult.Empty;
                }
                finally 
                { 
                    m_doDown.Write(false);
                    p_eResult = eResult.Empty;
                }
                return "OK";
            }

            double m_secWaitVacuum = 2;
            double m_secBlow = 0.5;
            string RunVacuum(bool bOn)
            {
                m_dioVacuum.Write(bOn);
                if (bOn == false)
                {
                    m_doBlow.Write(true);
                    Thread.Sleep((int)(500 * m_secBlow));
                    m_doBlow.Write(false);
                    return "OK";
                }
                int msVac = (int)(1000 * m_secWaitVacuum);
                while (m_dioVacuum.p_bIn != bOn)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return p_id + " EQ Stop";
                    if (m_dioVacuum.m_swWrite.ElapsedMilliseconds > msVac) return "Vacuum Sensor Timeout";
                }
                return "OK";
            }

            double m_secDown = 0.5;
            double m_secWaitUp = 2;
            string RunUpDown(bool bDown)
            {
                if (bDown)
                {
                    m_doDown.Write(true);
                    Thread.Sleep((int)(1000 * m_secDown));
                }
                else
                {
                    m_doDown.Write(false);
                    int msUp = (int)(1000 * m_secWaitUp);
                    StopWatch sw = new StopWatch();
                    while (m_diUp.p_bIn == false)
                    {
                        Thread.Sleep(10);
                        if (EQ.IsStop()) return p_id + " EQ Stop";
                        if (sw.ElapsedMilliseconds > msUp) return "Vacuum Sensor Timeout";
                    }
                }
                return "OK";
            }

            string m_sInfo = "OK";
            bool Run(string sInfo)
            {
                m_sInfo = sInfo;
                return sInfo == "OK";
            }

            public void RunTree(Tree tree)
            {
                m_secWaitVacuum = tree.Set(m_secWaitVacuum, m_secWaitVacuum, "Vacuum", "Vacuum On Wait Time (sec)");
                m_secBlow = tree.Set(m_secBlow, m_secBlow, "Blow", "Blow On Time (sec)");
                m_secWaitUp = tree.Set(m_secWaitUp, m_secWaitUp, "Up", "Picker Up Wait Time (sec)");
                m_secDown = tree.Set(m_secDown, m_secDown, "Down", "Picker Down Delay -> Vacuum On (sec)");
            }

            public string p_id { get; set; }
            public Picker(int iPicker)
            {
                p_id = "Picker" + (char)(iPicker + 'A');
                m_thread = new Thread(new ThreadStart(RunThread));
                m_thread.Start();
            }

            public void ThreadStop()
            {
                m_bThread = false;
                m_thread.Join();
            }
        }
        public List<Picker> m_picker = new List<Picker>();
        void InitPickers()
        {
            for (int n = 0; n < c_lPicker; n++) m_picker.Add(new Picker(n));
        }
        #endregion

        #region Run Picker
        public string PickerWaitDone()
        {
            while (IsPickerDone() == false)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
            }
            string sRun = "OK";
            foreach (Picker picker in m_picker)
            {
                string sInfo = picker.CheckDone();
                if ((sInfo != "OK") && (sRun == "OK")) sRun = sInfo;
            }
            return sRun;
        }

        bool IsPickerDone()
        {
            foreach (Picker picker in m_picker)
            {
                if (picker.IsDone() == false) return false;
            }
            return true;
        }
        #endregion

        #region ToolBox
        Axis m_axisWidth;
        AxisXZ m_axis;
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axisWidth, this, "Width");
            m_toolBox.GetAxis(ref m_axis, this, "Loader");
            foreach (Picker picker in m_picker) picker.GetTools(m_toolBox, this, bInit);
            if (bInit) InitPosition();
        }

        void InitPosition()
        {
            m_axisWidth.AddPos(Enum.GetNames(typeof(eWidth)));
            m_axis.AddPos(Enum.GetNames(typeof(ePos)));
        }
        #endregion

        #region Axis Width
        public enum eWidth
        {
            Min,
            Max,
        }
        Dictionary<eWidth, double> m_width = new Dictionary<eWidth, double>(); 
        void InitWidth()
        {
            m_width.Add(eWidth.Min, 10);
            m_width.Add(eWidth.Max, 20);
        }

        double m_fWidth = 10; 
        public string ChangeWidth(double fWidth, bool bWait = true)
        {
            m_fWidth = fWidth; 
            double pulseMin = m_axisWidth.GetPosValue(eWidth.Min);
            double pulseMax = m_axisWidth.GetPosValue(eWidth.Max);
            double dmm = m_width[eWidth.Max] - m_width[eWidth.Min]; 
            double dPos = (pulseMax - pulseMin) * (fWidth - m_width[eWidth.Min]) / dmm + pulseMin;
            m_axisWidth.StartMove(dPos);
            return bWait ? m_axisWidth.WaitReady() : "OK";
        }

        int m_incChip = 1;
        int m_incPicker = 1; 
        public string RunWidth()
        {
            double xDistance = Tray.m_distanceChip.X; 
            m_incChip = 1;
            while (m_width[eWidth.Min] > (m_incChip * xDistance)) m_incChip++; 
            m_incPicker = 1;
            while ((m_incPicker * m_width[eWidth.Min]) > xDistance) m_incPicker++;
            return ChangeWidth(m_incChip * xDistance / m_incPicker); 
        }

        void RunTreeWidth(Tree tree)
        {
            m_width[eWidth.Min] = tree.Set(m_width[eWidth.Min], m_width[eWidth.Min], "Min", "Picker Width (mm)");
            m_width[eWidth.Max] = tree.Set(m_width[eWidth.Max], m_width[eWidth.Max], "Max", "Picker Width (mm)");
        }
        #endregion

        #region Loader Axis
        public enum ePos
        {
            GoodA,
            GoodB,
            Reject,
            Rework
        }

        public string StartMoveX(ePos ePos, double fOffset)
        {
            m_axis.p_axisX.StartMove(ePos, fOffset);
            return "OK"; 
        }

        public string StartMoveZ(ePos ePos)
        {
            m_axis.p_axisZ.StartMove(ePos);
            return "OK";
        }
        #endregion

        #region Run Pick
        public string StartPick(Good.eGood eGood, Good.eStage eStage, eResult eResult, int maxPick)
        {
            Run_Pick run = (Run_Pick)m_runPick.Clone();
            run.m_eGood = eGood;
            run.m_eStage = eStage;
            run.m_eResult = eResult;
            run.m_maxPick = maxPick; 
            return StartRun(run); 
        }

        Good.eGood m_ePick = Good.eGood.GoodA; 
        public string RunPick(Good.eGood eGood, Good.eStage eStage, eResult eResult, int maxPick)
        {
            m_ePick = eGood; 
            Good good = m_handler.m_good[eGood];
            InfoTray infoTray = good.m_stage[eStage].p_infoTray;
            CPoint cpTray = infoTray.FindChip(eResult); 
            if (cpTray.X < 0) return "Can not Found " + eResult.ToString() + " in InfoTray";
            int iPicker = GetEmptyPicker();
            if (iPicker < 0) return "No Empty Picker";
            if (Run(StartMovePicker(eGood, iPicker, cpTray.X))) return p_sInfo;
            if (Run(good.MovePicker(eStage, 1000 * Tray.GetChipPosX(cpTray.Y)))) return p_sInfo; 
            if (Run(m_axis.WaitReady())) return p_sInfo; 
            for (int i = iPicker; i < c_lPicker; i += m_incPicker, cpTray.X += m_incChip) AddPick(i, infoTray, cpTray, eResult, ref maxPick);
            return PickerWaitDone(); 
        }

        int GetEmptyPicker()
        {
            for (int i = 0; i < c_lPicker; i += m_incPicker)
            {
                if (m_picker[i].p_eResult == eResult.Empty) return i; 
            }
            return -1; 
        }

        string StartMovePicker(Good.eGood eGood, int iPicker, int xChip)
        {
            double mmOffset = Tray.GetChipPosX(xChip) - m_fWidth * iPicker;
            ePos ePos = (eGood == Good.eGood.GoodA) ? ePos.GoodA : ePos.GoodB; 
            if (Run(StartMoveX(ePos, 1000 * mmOffset))) return p_sInfo;
            return StartMoveZ(ePos); 
        }

        void AddPick(int iPicker, InfoTray infoTray, CPoint cpTray, eResult eResult, ref int maxPick)
        {
            if (infoTray.m_aChip[cpTray.Y][cpTray.X] != eResult) return;
            if (maxPick <= 0) return; 
            m_picker[iPicker].StartLoad(infoTray, cpTray);
            maxPick--;  
        }
        #endregion

        #region Run Place
        public string StartPlace()
        {
            Run_Place run = (Run_Place)m_runPlace.Clone();
            return StartRun(run);
        }

        public string RunPlace()
        {
            eResult eResult = GetPickerResult(); 
            switch (eResult)
            {
                case eResult.Empty: return "All Picker Empty";
                case eResult.Good: return RunPlaceGood();
                default: return RunPlaceBad(eResult); 
            }
        }

        eResult GetPickerResult()
        {
            for (int n = 0; n < c_lPicker; n++)
            {
                if (m_picker[n].p_eResult != eResult.Empty) return m_picker[n].p_eResult; 
            }
            return eResult.Empty; 
        }

        string RunPlaceGood()
        {
            Good good = m_handler.m_good[m_ePick];
            InfoTray infoTray = good.m_stage[Good.eStage.Taker].p_infoTray;
            CPoint cpTray = infoTray.FindChip(eResult.Empty);
            if (cpTray.X < 0) return "Can not Found Empty in InfoTray";
            int iPicker = GetHoldPicker();
            if (iPicker < 0) return "No Hold Picker";
            if (Run(StartMovePicker(m_ePick, iPicker, cpTray.X))) return p_sInfo;
            if (Run(good.MovePicker(Good.eStage.Taker, 1000 * Tray.GetChipPosX(cpTray.Y)))) return p_sInfo;
            if (Run(m_axis.WaitReady())) return p_sInfo;
            for (int i = iPicker; i < c_lPicker; i += m_incPicker, cpTray.X += m_incChip) AddPlace(i, infoTray, cpTray);
            return PickerWaitDone();
        }

        string RunPlaceBad(eResult eResult)
        {
            Bad bad = m_handler.m_bad[(eResult == eResult.Reject) ? Bad.eBad.Reject : Bad.eBad.Rework];
            InfoTray infoTray = bad.m_stage.p_infoTray;
            CPoint cpTray = infoTray.FindChip(eResult.Empty);
            if (cpTray.X < 0) return "Can not Found Empty in InfoTray";
            int iPicker = GetHoldPicker();
            if (iPicker < 0) return "No Hold Picker";
            if (Run(StartMovePicker(m_ePick, iPicker, cpTray.X))) return p_sInfo;
            if (Run(bad.MoveStage(Bad.ePos.Picker, 1000 * Tray.GetChipPosX(cpTray.Y)))) return p_sInfo;
            if (Run(m_axis.WaitReady())) return p_sInfo;
            for (int i = iPicker; i < c_lPicker; i += m_incPicker, cpTray.X += m_incChip) AddPlace(i, infoTray, cpTray);
            return PickerWaitDone();
        }

        int GetHoldPicker()
        {
            for (int i = 0; i < c_lPicker; i += m_incPicker)
            {
                if (m_picker[i].p_eResult != eResult.Empty) return i;
            }
            return -1;
        }

        void AddPlace(int iPicker, InfoTray infoTray, CPoint cpTray)
        {
            if (infoTray.m_aChip[cpTray.Y][cpTray.X] != eResult.Empty) return;
            m_picker[iPicker].StartUnload(infoTray, cpTray);
        }
        #endregion

        #region override
        public override void RunTree(Tree tree)
        {
            RunTreeWidth(tree.GetTree("Width")); 
            foreach (Picker picker in m_picker) picker.RunTree(tree.GetTree(picker.p_id));
        }

        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            string sHome = StateHome(m_axis.p_axisZ, m_axisWidth);
            if (sHome != "OK") return sHome;
            sHome = StateHome(m_axis.p_axisX);
            if (sHome == "OK") p_eState = eState.Ready;
            return sHome;
        }

        public bool m_bBusy = false; 
        public Good.eGood m_eGoodRun = Good.eGood.GoodA; 
        public override string StateReady()
        {
            if (EQ.p_eState != EQ.eState.Run) return "OK";
            int iPicker = GetHoldPicker(); 
            if (iPicker >= 0) return StartPlace();
            Good.eGood eGood = m_eGoodRun;
            Good good = m_handler.m_good[eGood];
            if (m_bBusy == false) return "OK";

            InfoTray infoGiver = good.m_stage[Good.eStage.Giver].p_infoTray.CalcCount();
            InfoTray infoTaker = good.m_stage[Good.eStage.Taker].p_infoTray.CalcCount();
            int nTaker = GetChip(infoTaker, eResult.Empty);
            int nReject = GetChip(m_handler.m_bad[Bad.eBad.Reject].m_stage.p_infoTray.CalcCount(), eResult.Empty);
            int nRework = GetChip(m_handler.m_bad[Bad.eBad.Rework].m_stage.p_infoTray.CalcCount(), eResult.Empty);
            if ((GetChip(infoTaker, eResult.Reject) > 0) && (nReject > 0)) return StartPick(eGood, Good.eStage.Taker, eResult.Reject, nReject);
            if ((GetChip(infoTaker, eResult.Rework) > 0) && (nRework > 0)) return StartPick(eGood, Good.eStage.Taker, eResult.Rework, nRework);
            if ((GetChip(infoGiver, eResult.Reject) > 0) && (nReject > 0)) return StartPick(eGood, Good.eStage.Giver, eResult.Reject, nReject);
            if ((GetChip(infoGiver, eResult.Rework) > 0) && (nRework > 0)) return StartPick(eGood, Good.eStage.Giver, eResult.Rework, nRework);
            if ((GetChip(infoGiver, eResult.Good) > 0) && (nTaker > 0)) return StartPick(eGood, Good.eStage.Giver, eResult.Good, nTaker);
            m_bBusy = false; 
            return "OK";
        }

        int GetChip(InfoTray infoTray, eResult eResult)
        {
            if (infoTray == null) return 0;
            return infoTray.m_aCount[eResult]; 
        }
        #endregion

        JEDI_Sorter_Handler m_handler;
        public Loader(string id, IEngineer engineer)
        {
            m_handler = (JEDI_Sorter_Handler)engineer.ClassHandler();
            InitPickers();
            InitWidth(); 
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            foreach (Picker picker in m_picker) picker.ThreadStop();
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runPick;
        ModuleRunBase m_runPlace;
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            AddModuleRunList(new Run_ChangeWidth(this), true, "Change Picker Width");
            m_runPick = AddModuleRunList(new Run_Pick(this), true, "Run Pick Chip from Tray");
            m_runPlace = AddModuleRunList(new Run_Place(this), true, "Run Place Chip to Tray");
        }

        public class Run_Delay : ModuleRunBase
        {
            Loader m_module;
            public Run_Delay(Loader module)
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
                Thread.Sleep((int)(1000 * m_secDelay / 2));
                return "OK";
            }
        }

        public class Run_ChangeWidth : ModuleRunBase
        {
            Loader m_module;
            public Run_ChangeWidth(Loader module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_fWidth = 10;
            public override ModuleRunBase Clone()
            {
                Run_ChangeWidth run = new Run_ChangeWidth(m_module);
                run.m_fWidth = m_fWidth;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_fWidth = tree.Set(m_fWidth, m_fWidth, "Width", "Picker Width (mm)", bVisible);
            }

            public override string Run()
            {
                return m_module.ChangeWidth(m_fWidth);
            }
        }

        public class Run_Pick : ModuleRunBase
        {
            Loader m_module;
            public Run_Pick(Loader module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public Good.eGood m_eGood = Good.eGood.GoodA;
            public Good.eStage m_eStage = Good.eStage.Giver;
            public eResult m_eResult = eResult.Good;
            public int m_maxPick = c_lPicker; 
            public override ModuleRunBase Clone()
            {
                Run_Pick run = new Run_Pick(m_module);
                run.m_eGood = m_eGood;
                run.m_eStage = m_eStage;
                run.m_eResult = m_eResult;
                run.m_maxPick = m_maxPick; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eGood = (Good.eGood)tree.Set(m_eGood, m_eGood, "Boat", "Select Boat", bVisible);
                m_eStage = (Good.eStage)tree.Set(m_eStage, m_eStage, "Stage", "Select Boat", bVisible);
                m_eResult = (eResult)tree.Set(m_eResult, m_eResult, "Result", "Select Result", bVisible);
                m_maxPick = tree.Set(m_maxPick, m_maxPick, "Max Pick", "Max Pick Count", bVisible);
            }

            public override string Run()
            {
                return m_module.RunPick(m_eGood, m_eStage, m_eResult, m_maxPick);
            }
        }

        public class Run_Place : ModuleRunBase
        {
            Loader m_module;
            public Run_Place(Loader module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Pick run = new Run_Pick(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunPlace();
            }
        }
        #endregion
    }
}
