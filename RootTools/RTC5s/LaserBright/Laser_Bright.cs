using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RootTools.RTC5s.LaserBright
{
    public class Laser_Bright : NotifyProperty, ITool
    {
        #region DIO
        string _sInfo = "OK";
        string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (_sInfo == "OK") return;
                _sInfo = value;
            }
        }

        DIO_IO m_dioEnable;
        DIO_I m_diReady;
        DIO_O m_doStart;
        DIO_O m_doEmergency;
        public string GetTools(ModuleBase module)
        {
            ToolBox toolBox = module.m_toolBox;
            p_sInfo = toolBox.GetDIO(ref m_dioEnable, module, p_id + ".Enable");
            p_sInfo = toolBox.GetDIO(ref m_diReady, module, p_id + ".Ready");
            p_sInfo = toolBox.GetDIO(ref m_doStart, module, p_id + ".Start");
            p_sInfo = toolBox.GetDIO(ref m_doEmergency, module, p_id + ".Emergency");
            InitListDIO(module);
            return p_sInfo;
        }
        public ListDIO m_listDI = new ListDIO();
        public ListDIO m_listDO = new ListDIO();
        void InitListDIO(ModuleBase module)
        {
            m_listDI.Init(ListDIO.eDIO.Input);
            if (m_listDI.m_aDIO.Count == 0)
            {
                foreach (BitDI bit in module.m_listDI.m_aDIO)
                {
                    if (bit.p_sLongID.Contains(p_id)) m_listDI.m_aDIO.Add(bit);
                }
            }
            m_listDO.Init(ListDIO.eDIO.Output);
            if (m_listDO.m_aDIO.Count == 0)
            {
                foreach (BitDI bit in module.m_listDO.m_aDIO)
                {
                    if (bit.p_sLongID.Contains(p_id)) m_listDO.m_aDIO.Add(bit);
                }
            }
        }
        #endregion

        #region Property
        public bool p_bEnable { get { return m_dioEnable.p_bIn; } }

        public void LaserOff()
        {
            m_doStart.Write(false);
            m_dioEnable.Write(false);
            m_doEmergency.Write(false);
        }

        public void LaserOn()
        {
            if (p_bEnable) return;
            m_doEmergency.Write(false);
            m_doStart.Write(true);
        }
        #endregion

        #region Timer
        bool m_bReady = false;
        DispatcherTimer m_timer = new DispatcherTimer();
        private void M_timer_Tick(object sender, EventArgs e)
        {
            if (m_diReady.p_bIn && (m_bReady == false))
            {
                m_dioEnable.Write(true);
                m_bReady = true;
            }
            if (TimerPowerSave()) m_swPowerSave.Start();
        }
        #endregion

        #region Power Save
        bool m_bPowerSave = true;
        double m_secPowerSave = 30;
        void RunTreePowerSave(Tree tree)
        {
            m_bPowerSave = tree.Set(m_bPowerSave, m_bPowerSave, "Use", "Use Power Save Mode");
            m_secPowerSave = tree.Set(m_secPowerSave, m_secPowerSave, "Time", "Power Save Wait Time (sec)");
        }

        StopWatch m_swPowerSave = new StopWatch();
        bool TimerPowerSave()
        {
            if (p_bEnable == false) return true;
            if (m_bPowerSave == false) return true;
            //forget RTC5
            if (m_swPowerSave.ElapsedMilliseconds > (1000 * m_secPowerSave)) LaserOff();
            return false;
        }
        #endregion

        #region UI
        public UserControl p_ui
        {
            get
            {
                Laser_Bright_UI ui = new Laser_Bright_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }

        #endregion

        #region Tree
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreePowerSave(m_treeRoot.GetTree("Power Save"));
        }
        #endregion

        public string p_id { get; set; }

        Log m_log;
        public RTC5 m_RTC5;
        public TreeRoot m_treeRoot;
        public Laser_Bright(string id, Log log)
        {
            p_id = id;
            m_log = log;
            m_RTC5 = new RTC5(id, log);

            m_treeRoot = new TreeRoot(id, log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;

            m_timer.Interval = TimeSpan.FromMilliseconds(100);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        public void ThreadStop()
        {
            m_RTC5.ThreadStop();
        }
    }
}
