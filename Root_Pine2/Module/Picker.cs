using RootTools;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System.Threading;

namespace Root_Pine2.Module
{
    public class Picker : NotifyProperty
    {
        #region ToolBox
        ALID m_alidDrop; 
        public DIO_IO m_dioVacuum;
        DIO_O m_doBlow;
        public void GetTools(ToolBox toolBox, ModuleBase module, bool bInit)
        {
            module.p_sInfo = toolBox.GetDIO(ref m_dioVacuum, module, p_id + ".Vacuum");
            module.p_sInfo = toolBox.GetDIO(ref m_doBlow, module, p_id + ".Blow");
            if (bInit) m_alidDrop = module.m_gaf.GetALID(module, "Strip Drop", "Picker Strip Drop"); 
        }
        #endregion

        #region Run Vacuum
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

        public bool IsVacuum()
        {
            return m_dioVacuum.p_bIn; 
        }

        public void RunTreeVacuum(Tree tree)
        {
            m_secVac = tree.Set(m_secVac, m_secVac, "Vacuum", "Vacuum Sensor Wait (sec)");
            m_secBlow = tree.Set(m_secBlow, m_secBlow, "Blow", "Blow Time (sec)");
            m_secDrop = tree.Set(m_secDrop, m_secDrop, "Drop", "Strip Drop Time (sec)");
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
            }
        }
        #endregion

        #region Thread Check
        bool m_bThread = false;
        Thread m_threadCheck;
        void InitThreadCheck()
        {
            m_threadCheck = new Thread(new ThreadStart(RunThreadCheck));
            m_threadCheck.Start();
        }

        double m_secDrop = 0.2;
        void RunThreadCheck()
        {
            StopWatch sw = new StopWatch(); 
            m_bThread = true;
            Thread.Sleep(5000);
            while (m_bThread)
            {
                Thread.Sleep(10);
                if ((p_infoStrip == null) || (m_dioVacuum.p_bOut == false) || m_dioVacuum.p_bIn) sw.Start(); 
                m_alidDrop.p_bSet = (sw.ElapsedMilliseconds > (int)(1000 * m_secDrop));
            }
        }
        #endregion

        public string p_id { get; set; }
        public Picker(string id)
        {
            p_id = id;
            InitThreadCheck(); 
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                m_threadCheck.Join(); 
            }
        }
    }
}
