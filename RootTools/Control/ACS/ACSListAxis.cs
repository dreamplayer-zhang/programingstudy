using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace RootTools.Control.ACS
{
    public class ACSListAxis : NotifyProperty
    {
        #region List Axis
        public delegate void dgOnChangeAxisList();
        public event dgOnChangeAxisList OnChangeAxisList;

        public List<ACSAxis> m_aAxis = new List<ACSAxis>();

        public Axis GetAxis(string id, Log log)
        {
            ACSAxis axis = new ACSAxis();
            axis.Init(this, id, m_acs);
            m_aAxis.Add(axis);
            if (OnChangeAxisList != null) OnChangeAxisList();
            return axis;
        }

        public AxisXY GetAxisXY(string id, Log log)
        {
            AxisXY axisXY = new AxisXY();
            axisXY.p_axisX = GetAxis(id + "X", log);
            axisXY.p_axisY = GetAxis(id + "Y", log);
            return axisXY;
        }

        public AxisXZ GetAxisXZ(string id, Log log)
        {
            AxisXZ axisXZ = new AxisXZ();
            axisXZ.p_axisX = GetAxis(id + "X", log);
            axisXZ.p_axisZ = GetAxis(id + "Z", log);
            return axisXZ;
        }

        public Axis3D GetAxis3D(string id, Log log)
        {
            Axis3D axis = new Axis3D();
            axis.p_axisX = GetAxis(id + "X", log);
            axis.p_axisY = GetAxis(id + "Y", log);
            axis.p_axisZ = GetAxis(id + "Z", log);
            return axis;
        }
        #endregion

        #region AXM Info
        string _sInfo = "Last Error";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (_sInfo == value) return;
                _sInfo = value;
                OnPropertyChanged();
                if (value == "OK") return;
                m_log.Error("p_sInfo = " + value);
            }
        }
        #endregion

        #region Thread Check
        public void RunThreadCheck()
        {
            try
            {
                Array aLimit = m_acs.m_channel.ReadVariableAsVector("FAULT", -1, 0, m_lAxis); 
                foreach (ACSAxis axis in m_aAxis) axis.RunThreadCheck(aLimit);
            }
            catch (Exception e) { LogError(m_id + "ReadVariableAsVector Error : " + e.Message); }
        }

        StopWatch m_swError = new StopWatch();
        void LogError(string sError)
        {
            if (m_swError.ElapsedMilliseconds < 5000) return;
            m_swError.Restart();
            p_sInfo = sError;
        }
        #endregion

        string m_id;
        Log m_log;
        IEngineer m_engineer;
        ACS m_acs; 
        public void Init(string id, IEngineer engineer, ACS acs)
        {
            m_id = id;
            m_engineer = engineer;
            m_acs = acs; 
            m_log = LogView.GetLog(id);
        }

        public void ThreadStop()
        {
            m_log.Info("ThreadStop Start");
            foreach (ACSAxis axis in m_aAxis) axis.ThreadStop();
            m_log.Info("ThreadStop Done");
        }

        public int m_lAxis = 0;
        public int m_lAxisDetect = 0;
        public void RunTree(Tree tree)
        {
            tree.Set(m_lAxisDetect, m_lAxisDetect, "Detect", "Detected Axis Count", true, true);
            m_lAxis = tree.Set(m_lAxis, m_lAxis, "Set", "Set Axis Count"); 
        }

        public void RunEmergency()
        {
            foreach (ACSAxis axis in m_aAxis) axis.ServoOn(false);
        }
    }
}
