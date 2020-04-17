using Microsoft.Win32;
using RootTools.Trees;
using System.Collections.Generic;
using System.Threading;

namespace RootTools.Control.Ajin
{
    public class AjinListAxis : NotifyProperty
    {
        #region V Rate
        public static double s_vRate = 1;
        public double p_vRate
        {
            get { return s_vRate; }
            set
            {
                m_log.Info("V Rate Change : " + s_vRate.ToString() + " -> " + value.ToString());
                s_vRate = value;
                for (int n = 0; n < m_aAxis.Count; n++) m_aAxis[n].OverrideV();
            }
        }
        #endregion

        #region List Axis
        public delegate void dgOnChangeAxisList();
        public event dgOnChangeAxisList OnChangeAxisList; 

        int m_lAxis = 0;
        public List<IAxis> m_aAxis = new List<IAxis>();
        void InitAxisList()
        {
            while (m_aAxis.Count < m_lAxis)
            {
                AjinAxis axis = new AjinAxis();
                axis.Init(m_id, m_aAxis.Count, m_engineer, m_log, m_bEnable);
                m_aAxis.Add(axis);
            }
            if (OnChangeAxisList != null) OnChangeAxisList(); 
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
            p_sInfo = sFunc + " Ajin Error = " + uResult.ToString();
            m_log.Error(m_id + "." + p_sInfo);
            m_swAXM.Start();
            return uResult;
        }

        string AXM(string sFunc, string sError)
        {
            if (sError == "OK") return sError;
            if (m_log == null) return sError;
            if (m_swAXM.ElapsedMilliseconds < 1000) return sError;
            p_sInfo = sFunc + " Ajin Error = " + sError;
            m_log.Error(m_id + "." + p_sInfo);
            m_swAXM.Start();
            return sError;
        }
        #endregion

        #region InitAxis
        int m_lAxisAjin = 0;
        string InitAxis()
        {
            AXM("AxmInfoGetAxisCount", CAXM.AxmInfoGetAxisCount(ref m_lAxisAjin));
            if (m_bAXL == false) return "Init Axis Skip : AXL";
            m_bEnable = false;
            m_threadInitAxis = new Thread(new ThreadStart(RunThread_InitAxis));
            m_threadInitAxis.Start();
            return "OK";
        }

        bool m_bInitAxis = false;
        Thread m_threadInitAxis;
        void RunThread_InitAxis()
        {
            m_bInitAxis = true;
            for (int n = 0; n < m_lAxis; n++) ((AjinAxis)m_aAxis[n]).SetAxisStatus();
            m_bEnable = true;
            m_log.Info("RunThread_InitAxis - Done.");
        }
        #endregion

        #region MOT
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
            for (int n = 0; n < m_lAxis; n++) ((AjinAxis)m_aAxis[n]).GetAxisStatus();
        }

        public void LoadMotFile()
        {
            uint nError = CAXM.AxmMotLoadParaAll(m_strMotFile);
            if (nError > 0)
            {
                m_log.Error("AxmMotLoadParaAll Error : " + m_strMotFile + "  " + nError.ToString());
                return;
            }
            for (int n = 0; n < m_lAxis; n++) ((AjinAxis)m_aAxis[n]).GetAxisStatus();
        }

        public void SaveMot()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Mot Files (*.Mot)|*.Mot";
            if (dlg.ShowDialog() == false) return;
            AXM("AxmMotSaveParaAll", CAXM.AxmMotSaveParaAll(dlg.FileName));
        }
        #endregion

        string m_id;
        LogWriter m_log;
        IEngineer m_engineer;
        bool m_bAXL = false;
        bool m_bEnable = false;
        string m_strMotFile = @"C:\VEGA\Init\VEGA.mot";
        public void Init(string id, IEngineer engineer, bool bAXL)
        {
            m_id = id;
            m_engineer = engineer;
            m_bAXL = bAXL;
            m_log = engineer.ClassLogView().GetLog(LogView.eLogType.ENG, id);
            AXM("Init Axis Error (ReStart SW) : ", InitAxis());
        }

        public void ThreadStop()
        {
            m_log.Info("ThreadStop Start");
            if (m_bInitAxis)
            {
                m_bInitAxis = false;
                m_threadInitAxis.Join();
            }
            for (int n = 0; n < m_aAxis.Count; n++) ((AjinAxis)m_aAxis[n]).ThreadStop();
            m_log.Info("ThreadStop Done");
        }

        public void RunTree(Tree tree)
        {
            m_strMotFile = tree.SetFile(m_strMotFile, m_strMotFile, "mot", "MotFile", "Motor 설정  File 위치");
            p_vRate = tree.Set(p_vRate, 1, "V Rate", "All Axis V Rate (0.1 ~ 1)");
            if (p_vRate < 0.1) p_vRate = 0.1;
            if (p_vRate > 1) p_vRate = 1;
            RunCountTree(tree.GetTree("Count")); 
        }

        void RunCountTree(Tree tree)
        {
            m_lAxisAjin = tree.Set(m_lAxisAjin, m_lAxisAjin, "Detect", "Detected Axis Count", true, true);
            m_lAxis = tree.Set(m_lAxis, 0, "Set", "Axis Count Set");
            InitAxisList();
        }

        public void RunEmergency()
        {
            for (int n = 0; n < m_lAxis; n++) m_aAxis[n].ServoOn(false);
        }
    }
}
