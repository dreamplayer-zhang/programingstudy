using Microsoft.Win32;
using RootTools.Trees;
using System.Collections.Generic;
using System.Threading;

namespace RootTools.Control.Ajin
{
    public class AjinListAxis : NotifyProperty
    {
        #region List Axis
        public delegate void dgOnChangeAxisList();
        public event dgOnChangeAxisList OnChangeAxisList; 

        public List<AjinAxis> m_aAxis = new List<AjinAxis>();
        
        public Axis GetAxis(string id, Log log)
        {
            AjinAxis axis = new AjinAxis();
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

        #region Thread InitAxis
        public Queue<AjinAxis> m_qSetAxis = new Queue<AjinAxis>(); 
        int m_lAxisAjin = 0;
        string InitAxis()
        {
            AXM("AxmInfoGetAxisCount", CAXM.AxmInfoGetAxisCount(ref m_lAxisAjin));
            if (m_bAXL == false) return "Init Axis Skip : AXL";
            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start();
            return "OK";
        }

        bool m_bThread = false;
        Thread m_thread; 
        void RunThread()
        {
            m_bThread = true; 
            LoadMotFile();
            Thread.Sleep(5000);
            while (m_bThread)
            {
                Thread.Sleep(10); 
                if (m_qSetAxis.Count > 0)
                {
                    AjinAxis axis = m_qSetAxis.Dequeue();
                    axis.GetAxisStatus();
                    axis.RunTreeSetting(Tree.eMode.Init); 
                    axis.RunTreeSetting(Tree.eMode.RegWrite);
                    axis.RunTreeSetting(Tree.eMode.Update);
                }
            }
        }
        #endregion

        #region MOT
        Registry m_reg = new Registry("AjinListAxis"); 
        string _sMotFile = @"C:\Init\Motor.mot";
        string p_sMotFile
        {
            get { return _sMotFile; }
            set
            {
                _sMotFile = value;
                m_reg.Write("p_sMotFile", value); 
            }
        }

        public void LoadMot()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Mot Files (*.Mot)|*.Mot";
            if (dlg.ShowDialog() == false) return;
            p_sMotFile = dlg.FileName; 
            uint nError = CAXM.AxmMotLoadParaAll(dlg.FileName);
            if (nError > 0)
            {
                m_log.Error("AxmMotLoadParaAll Error : " + nError.ToString());
                return;
            }
            foreach (AjinAxis axis in m_aAxis) m_qSetAxis.Enqueue(axis); 
        }

        public void LoadMotFile()
        {
            uint nError = CAXM.AxmMotLoadParaAll(p_sMotFile);
            if (nError > 0) m_log.Error("AxmMotLoadParaAll Error : " + p_sMotFile + "  " + nError.ToString());
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
        public void Init(string id, IEngineer engineer, bool bAXL)
        {
            _sMotFile = m_reg.Read("p_sMotFile", _sMotFile); 
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
            foreach (AjinAxis axis in m_aAxis) axis.ThreadStop();
            m_log.Info("ThreadStop Done");
        }

        public void RunTree(Tree tree)
        {
            p_sMotFile = tree.SetFile(p_sMotFile, p_sMotFile, "mot", "MotFile", "Motor 설정  File 위치", true, false);
            tree.Set(m_lAxisAjin, m_lAxisAjin, "Detect", "Detected Axis Count", true, true);
        }

        public void RunEmergency()
        {
            foreach (AjinAxis axis in m_aAxis) axis.ServoOn(false); 
        }
    }
}
