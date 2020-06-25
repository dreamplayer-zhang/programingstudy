using Microsoft.Win32;
using RootTools.Trees;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace RootTools.Control.Ajin
{
    public class AjinListAxis : NotifyProperty
    {
        #region List Axis
        public delegate void dgOnChangeAxisList();
        public event dgOnChangeAxisList OnChangeAxisList; 

        public List<AjinAxis> m_aAxis = new List<AjinAxis>();
/*        void InitAxisList()
        {
            while (m_aAxis.Count < m_lAxis)
            {
                AjinAxis axis = new AjinAxis();
                axis.Init(m_id, this, m_engineer, m_bEnable);
                m_aAxis.Add(axis);
            }
            if (OnChangeAxisList != null) OnChangeAxisList();
        } */ //forget
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
        BackgroundWorker m_bgwInit = new BackgroundWorker();
        int m_lAxisAjin = 0;
        string InitAxis()
        {
            AXM("AxmInfoGetAxisCount", CAXM.AxmInfoGetAxisCount(ref m_lAxisAjin));
            if (m_bAXL == false) return "Init Axis Skip : AXL";
            m_bgwInit.DoWork += M_bgwInit_DoWork;
            m_bgwInit.RunWorkerCompleted += M_bgwInit_RunWorkerCompleted;
            return "OK";
        }

        private void M_bgwInit_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int n = 0; n < m_lAxisAjin; n++) (m_aAxis[n]).SetAxisStatus();
        }

        private void M_bgwInit_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        void RunThread_InitAxis()
        {
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
        Log m_log;
        IEngineer m_engineer;
        bool m_bAXL = false;
        string m_strMotFile = @"C:\VEGA\Init\VEGA.mot";
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
