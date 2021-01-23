﻿using System.Collections.Generic;

namespace RootTools.Control.Xenax
{
    public class XenaxListAxis : NotifyProperty
    {
        #region List Axis
        public delegate void dgOnChangeAxisList();
        public event dgOnChangeAxisList OnChangeAxisList;

        public List<XenaxAxis> m_aAxis = new List<XenaxAxis>();

        public Axis GetAxis(string id, Log log)
        {
            XenaxAxis axis = new XenaxAxis();
            axis.Init(this, id, log);
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

        string m_id;
        Log m_log;
        IEngineer m_engineer;
        public void Init(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = engineer;
            m_log = LogView.GetLog(id);
        }

        public void ThreadStop()
        {
            m_log.Info("ThreadStop Start");
            foreach (XenaxAxis axis in m_aAxis) axis.ThreadStop();
            m_log.Info("ThreadStop Done");
        }

        public void RunEmergency()
        {
            foreach (XenaxAxis axis in m_aAxis) axis.ServoOn(false);
        }
    }
}
