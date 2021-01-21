using RootTools.Trees;
using System.Collections.Generic;
using System.Threading;

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
            m_qSetAxis.Enqueue(axis);
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

        StopWatch m_swAXM = new StopWatch();
        uint AXM(string sFunc, uint uResult)
        {
            if (uResult == 0) return uResult;
            if (m_log == null) return uResult;
            if (m_swAXM.ElapsedMilliseconds < 1000) return uResult;
            p_sInfo = sFunc + " Xenax Error = " + uResult.ToString();
            m_log.Error(m_id + "." + p_sInfo);
            m_swAXM.Start();
            return uResult;
        }

        string AXM(string sFunc, string sError)
        {
            if (sError == "OK") return sError;
            if (m_log == null) return sError;
            if (m_swAXM.ElapsedMilliseconds < 1000) return sError;
            p_sInfo = sFunc + " Xenax Error = " + sError;
            m_log.Error(m_id + "." + p_sInfo);
            m_swAXM.Start();
            return sError;
        }
        #endregion

        #region Thread InitAxis
        int m_lAxisXenax = 0;
        string InitAxis()
        {
            AXM("AxmInfoGetAxisCount", CAXM.AxmInfoGetAxisCount(ref m_lAxisXenax));
            return "OK";
        }

        #endregion

        string m_id;
        Log m_log;
        IEngineer m_engineer;
        bool m_bAXL = false;
        public void Init(string id, IEngineer engineer, bool bAXL)
        {
            m_id = id;
            m_engineer = engineer;
            m_bAXL = bAXL;
            m_log = LogView.GetLog(id);
            AXM("Init Axis Error (ReStart SW) : ", InitAxis());
        }

        public void ThreadStop()
        {
            m_log.Info("ThreadStop Start");
            if (m_bThread)
            {
                m_bThread = false;
                m_thread.Join();
            }
            foreach (XenaxAxis axis in m_aAxis) axis.ThreadStop();
            m_log.Info("ThreadStop Done");
        }

        public void RunTree(Tree tree)
        {
            tree.Set(m_lAxisXenax, m_lAxisXenax, "Detect", "Detected Axis Count", true, true);
        }

        public void RunEmergency()
        {
            foreach (XenaxAxis axis in m_aAxis) axis.ServoOn(false);
        }
    }
}
