using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System.Threading;

namespace Root_ASIS.Module
{
    public class PickerFix : NotifyProperty
    {
        #region ToolBox
        public DIO_IO m_dioVacuum;
        DIO_O m_doBlow;
        public void GetTools(ModuleBase module, bool bInit)
        {
            module.p_sInfo = module.m_toolBox.Get(ref m_dioVacuum, module, "Vacuum");
            module.p_sInfo = module.m_toolBox.Get(ref m_doBlow, module, "Blow");
        }
        #endregion

        #region DIO Functions
        double m_secVac = 2;
        double m_secBlow = 0.5;
        public string RunVacuum(bool bOn)
        {
            m_dioVacuum.Write(bOn);
            if (bOn == false)
            {
                m_doBlow.Write(true);
                Thread.Sleep((int)(1000 * m_secBlow));
                m_doBlow.Write(false);
                return "OK";
            }
            int msVac = (int)(1000 * m_secVac);
            while (m_dioVacuum.p_bIn != bOn)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return p_id + " EQ Stop";
                if (m_dioVacuum.m_swWrite.ElapsedMilliseconds > msVac) return "Vacuum Sensor Timeout";
            }
            return "OK";
        }

        void RunTreeTimeout(Tree tree)
        {
            m_secVac = tree.Set(m_secVac, m_secVac, "Vacuum", "Vacuum Sensor Wait (sec)");
            m_secBlow = tree.Set(m_secBlow, m_secBlow, "Blow", "Blow Time (sec)");
        }
        #endregion

        #region Thread Vacuum Check
        bool m_bThread = false;
        Thread m_thread;
        void InitThread()
        {
            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start();
        }

        void RunThread()
        {
            m_bThread = true;
            Thread.Sleep(2000);
            while (m_bThread)
            {
                Thread.Sleep(10);
                p_bVacuumError = ((p_infoStrip != null) && m_dioVacuum.p_bOut && (m_dioVacuum.p_bIn == false));
            }
        }

        bool _bVacuumError = false;
        public bool p_bVacuumError
        {
            get { return _bVacuumError; }
            set
            {
                if (_bVacuumError == value) return;
                _bVacuumError = value;
                OnPropertyChanged();
                if (value)
                {
                    EQ.p_bStop = true;
                    m_module.p_sInfo = "Picker Vacuum Check Error";
                }
            }
        }
        #endregion

        #region InfoStrip
        InfoStrip _infoStrip = null;
        public InfoStrip p_infoStrip
        {
            get { return _infoStrip; }
            set
            {
                _infoStrip = value;
                OnPropertyChanged();
                m_reg.Write("iStrip", (value == null) ? -1 : value.p_iStrip);
            }
        }

        void InitStrip()
        {
            int iStrip = m_reg.Read("iStrip", -1);
            if (iStrip < 0) return;
            p_infoStrip = new InfoStrip(iStrip);
        }
        #endregion

        #region Tree & Reset
        public void RunTree(Tree tree)
        {
            RunTreeTimeout(tree.GetTree("Timeout", false));
        }

        public void Reset()
        {
            if (m_dioVacuum.p_bIn == false) p_infoStrip = null;
        }
        #endregion

        public string p_id { get; set; }
        ModuleBase m_module;
        Registry m_reg;
        public PickerFix(string id, ModuleBase module)
        {
            p_id = id;
            m_module = module;
            m_reg = new Registry(p_id);
            InitStrip();
            InitThread();
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                m_thread.Join();
            }
        }

    }
}
