using RootTools.Trees;
using SPIIPLUSCOM660Lib;
using System.Collections.Generic;
using System.Threading;

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
            axis.Init(this, m_channel, id, log);
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
            p_sInfo = sFunc + " ACS Error = " + uResult.ToString();
            m_log.Error(m_id + "." + p_sInfo);
            m_swAXM.Start();
            return uResult;
        }

        string Run(string sFunc, string sError)
        {
            if (sError == "OK") return sError;
            if (m_log == null) return sError;
            if (m_swAXM.ElapsedMilliseconds < 1000) return sError;
            p_sInfo = sFunc + " ACS Error = " + sError;
            m_log.Error(m_id + "." + p_sInfo);
            m_swAXM.Start();
            return sError;
        }
        #endregion

        #region Thread InitAxis
        public Queue<ACSAxis> m_qSetAxis = new Queue<ACSAxis>();
        int m_lAxisACS = 0;
        string InitAxis()
        {
            //AXM("AxmInfoGetAxisCount", CAXM.AxmInfoGetAxisCount(ref m_lAxisACS));
            if (m_bChannel == false) return "Init Axis Skip : AXL";
            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start();
            return "OK";
        }

        bool m_bThread = false;
        Thread m_thread;
        void RunThread()
        {
            m_bThread = true;
//            LoadMotFile();
            while (m_bThread)
            {
                Thread.Sleep(10);
                if (m_qSetAxis.Count > 0)
                {
                    ACSAxis axis = m_qSetAxis.Dequeue();
                    axis.GetAxisStatus();
                    axis.RunTreeSetting(Tree.eMode.Init);
                }
            }
        }
        #endregion

        #region MOT
        /*
                public void LoadMot()
                {
                    OpenFileDialog dlg = new OpenFileDialog();
                    dlg.Filter = "Mot Files (*.Mot)|*.Mot";
                    if (dlg.ShowDialog() == false) return;
                    uint nError = CAXM.AxmMotLoadParaAll(dlg.FileName);
                    if (nError > 0)
                    {
                        m_log.Error("AxmMotLoadParaAll Error : " + nError.ToString());
                        return;
                    }
                    foreach (ACSAxis axis in m_aAxis) m_qSetAxis.Enqueue(axis);
                }

                public void LoadMotFile()
                {
                    uint nError = CAXM.AxmMotLoadParaAll(m_strMotFile);
                    if (nError > 0) m_log.Error("AxmMotLoadParaAll Error : " + m_strMotFile + "  " + nError.ToString());
                }

                public void SaveMot()
                {
                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.Filter = "Mot Files (*.Mot)|*.Mot";
                    if (dlg.ShowDialog() == false) return;
                    AXM("AxmMotSaveParaAll", CAXM.AxmMotSaveParaAll(dlg.FileName));
                }
        */
        #endregion

        string m_id;
        Log m_log;
        IEngineer m_engineer;
        Channel m_channel; 
        bool m_bChannel = false;
//        string m_strMotFile = @"C:\VEGA\Init\VEGA.mot";
        public void Init(string id, IEngineer engineer, Channel channel, bool bChannel)
        {
            m_id = id;
            m_engineer = engineer;
            m_channel = channel; 
            m_bChannel = bChannel;
            m_log = LogView.GetLog(id);
            Run("Init Axis Error (ReStart SW) : ", InitAxis());
        }

        public void ThreadStop()
        {
            m_log.Info("ThreadStop Start");
            if (m_bThread)
            {
                m_bThread = false;
                m_thread.Join();
            }
            foreach (ACSAxis axis in m_aAxis) axis.ThreadStop();
            m_log.Info("ThreadStop Done");
        }

        public void RunTree(Tree tree)
        {
            tree.Set(m_lAxisACS, m_lAxisACS, "Detect", "Detected Axis Count", true, true);
        }

        public void RunEmergency()
        {
            foreach (ACSAxis axis in m_aAxis) axis.ServoOn(false);
        }
    }
}
