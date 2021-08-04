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

            InfoChip _infoChip = null;
            public InfoChip p_infoChip
            {
                get { return _infoChip; }
                set
                {
                    _infoChip = value;
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
                        p_infoChip = null;
                        return p_id + " Unload Fail";
                }
                return "OK";
            }

            public string StartLoad()
            {
                if (p_infoChip != null) return p_id + " Has InfoChip";
                if (p_eState != eState.Ready) return p_id + " State not Ready";
                p_eState = eState.Loading;
                return "OK";
            }

            public string StartUnload()
            {
                if (p_infoChip == null) return p_id + " Has not InfoChip";
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
                        case eState.Loading: p_eState = (RunPicker(true) == "OK") ? eState.Load : eState.LoadFail; break;
                        case eState.Unloading: p_eState = (RunPicker(false) == "OK") ? eState.Ready : eState.UnloadFail; break;
                    }
                }
            }

            string RunPicker(bool bLoading)
            {
                try
                {
                    if (Run(RunUpDown(true))) return m_sInfo;
                    if (Run(RunVacuum(bLoading))) return m_sInfo;
                    if (Run(RunUpDown(false))) return m_sInfo;
                }
                finally { m_doDown.Write(false); }
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
        AxisXY m_axis;
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
            mm75,
            mm95,
        }

        double m_dPos = 0;
        public string RunWidth(double fWidth, bool bWait = true)
        {
            double f75 = m_axisWidth.GetPosValue(eWidth.mm75);
            double f95 = m_axisWidth.GetPosValue(eWidth.mm95);
            double dPos = (f95 - f75) * (fWidth - 75) / 20 + f75;
            m_dPos = dPos;
            m_axisWidth.StartMove(dPos);
            return bWait ? m_axisWidth.WaitReady() : "OK";
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

        public string RunMoveX(ePos ePos, double fOffset, bool bWait = true)
        {
            m_axis.p_axisX.StartMove(ePos, fOffset);
            return bWait ? m_axis.p_axisX.WaitReady() : "OK"; 
        }

        public string RunMoveZ(ePos ePos, bool bWait = true)
        {
            m_axis.p_axisY.StartMove(ePos);
            return bWait ? m_axis.p_axisY.WaitReady() : "OK";
        }
        #endregion

        #region override
        public override void RunTree(Tree tree)
        {
            foreach (Picker picker in m_picker) picker.RunTree(tree.GetTree(picker.p_id));
        }
        #endregion

        JEDI_Sorter_Handler m_handler;
        public Loader(string id, IEngineer engineer)
        {
            m_handler = (JEDI_Sorter_Handler)engineer.ClassHandler();
            InitPickers();
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            foreach (Picker picker in m_picker) picker.ThreadStop();
            base.ThreadStop();
        }
    }
}
