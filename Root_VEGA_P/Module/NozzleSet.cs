using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System.Collections.Generic;
using System.IO;

namespace Root_VEGA_P.Module
{
    public class NozzleSet : NotifyProperty
    {
        #region ToolBox
        DIO_Os m_doNozzle; 
        public string GetTools(ToolBox toolBox)
        {
            toolBox.GetDIO(ref m_doNozzle, m_module, m_sID + "Nozzle", m_asNozzle); 
            return "OK";
        }
        #endregion

        #region Property
        string[] m_asNozzle = new string[1] { "01" };
        int _nNozzle = 1;
        public int p_nNozzle
        {
            get { return _nNozzle; }
            set
            {
                if (_nNozzle == value) return;
                _nNozzle = value;
                m_asNozzle = new string[value];
                for (int n = 0; n < value; n++) m_asNozzle[n] = (n + 1).ToString("00");
                m_aOpen.Clear();
                for (int n = 0; n < value; n++) m_aOpen.Add(false);
                m_reg.Write(m_sID + "nNozzle", value);
            }
        }

        Registry m_reg;
        void InitNozzle(string id)
        {
            m_reg = new Registry(id);
            p_nNozzle = m_reg.Read(m_sID + "nNozzle", 1);
        }
        #endregion

        #region Atmospheric pressure
        double _hPa = 0;
        public double p_hPa
        {
            get { return _hPa; }
            set
            {
                if (_hPa == value) return;
                _hPa = value;
                OnPropertyChanged(); 
            }
        }
        public void RunTreePressure(Tree tree)
        {
            p_hPa = tree.Set(p_hPa, p_hPa, "hPa", "Atmospheric pressure (hPa)"); 
        }
        #endregion

        #region Nozzle Open
        public List<bool> m_aOpen = new List<bool>();

        public List<bool> GetCloneOpen()
        {
            List<bool> aOpen = new List<bool>();
            foreach (bool bOpen in m_aOpen) aOpen.Add(bOpen);
            return aOpen; 
        }

        public void RunTreeOpen(Tree tree)
        {
            for (int n = 0; n < p_nNozzle; n++)
            {
                m_aOpen[n] = tree.Set(m_aOpen[n], m_aOpen[n], (n + 1).ToString("00"), "Nozzle Open");
            }
        }
        #endregion

        #region File Save & Open
        TreeRoot m_treeRootJob; 
        void InitFileTree()
        {
            m_treeRootJob = new TreeRoot(m_sExt, m_module.m_log); 
        }

        public List<string> p_asFile
        {
            get
            {
                List<string> asFile = new List<string>();
                string sExt = "." + m_sExt; 
                string sPath = EQ.c_sPathRecipe + "\\Nozzle";
                DirectoryInfo directory = new DirectoryInfo(sPath); 
                foreach (FileInfo file in directory.GetFiles())
                {
                    if (file.Extension == sExt) asFile.Add(GetFileTitle(file.Name)); 
                }
                return asFile; 
            }
        }

        public void FileSave(string sFile)
        {
            string sPath = EQ.c_sPathRecipe + "\\Nozzle"; 
            Directory.CreateDirectory(sPath);
            Job job = new Job(sPath + "\\" + sFile + "." +m_sExt, true, m_module.m_log);
            m_treeRootJob.m_job = job;
            m_treeRootJob.p_eMode = Tree.eMode.JobSave;
            RunTreePressure(m_treeRootJob.GetTree("Pressure"));
            RunTreeOpen(m_treeRootJob.GetTree("Nozzle"));
            job.Close(); 
        }

        public void FileOpen(string sFile)
        {
            string sPath = EQ.c_sPathRecipe + "\\Nozzle";
            Directory.CreateDirectory(sPath);
            Job job = new Job(sPath + "\\" + sFile + "." + m_sExt, false, m_module.m_log);
            m_treeRootJob.m_job = job;
            m_treeRootJob.p_eMode = Tree.eMode.JobOpen;
            RunTreePressure(m_treeRootJob.GetTree("Pressure"));
            RunTreeOpen(m_treeRootJob.GetTree("Nozzle"));
            job.Close();
        }

        public void FileSave()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = "." + m_sExt;
            dlg.Filter = "Nozzle File (*." + m_sExt + ")|*." + m_sExt;
            dlg.InitialDirectory = EQ.c_sPathRecipe + "\\Nozzle";
            if (dlg.ShowDialog() == false) return;
            FileSave(GetFileTitle(dlg.SafeFileName)); 
        }

        public void FileOpen()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            string sExt = "." + m_sExt; 
            dlg.DefaultExt = sExt;
            dlg.Filter = "Nozzle File (*" + sExt + ")|*" + sExt;
            dlg.InitialDirectory = EQ.c_sPathRecipe + "\\Nozzle";
            if (dlg.ShowDialog() == false) return;
            FileOpen(GetFileTitle(dlg.SafeFileName));
        }

        string GetFileTitle(string sFile)
        {
            return sFile.Substring(0, sFile.Length - m_sExt.Length - 1); 
        }
        #endregion

        #region Nozzle Run
        public string RunNozzle(string sFile)
        {
            FileOpen(sFile);
            return RunNozzle(m_aOpen); 
        }

        public string RunNozzle(List<bool> aOpen)
        {
            for (int n = 0; n < p_nNozzle; n++) m_doNozzle.Write(n, aOpen[n]); 
            return "OK";
        }

        public string RunNozzle(int nNozzle)
        {
            for (int n = 0; n < p_nNozzle; n++) m_doNozzle.Write(n, n == nNozzle); 
            return "OK";
        }

        public string RunCloseAllNozzle()
        {
            for (int n = 0; n < p_nNozzle; n++) m_doNozzle.Write(n, false);
            return "OK";
        }
        #endregion

        #region Tree
        public void RunTreeSetup(Tree tree)
        {
            p_nNozzle = tree.Set(p_nNozzle, p_nNozzle, "Count", "Nozzle Count");
        }
        #endregion

        string m_sID;
        public string m_sExt; 
        ModuleBase m_module;
        public NozzleSet(ModuleBase module, string sID)
        {
            m_module = module;
            m_sID = sID;
            m_sExt = module.p_id;
            if (sID != "") m_sExt += sID.Substring(0, sID.Length - 1);
            InitFileTree(); 
            InitNozzle(module.p_id);
        }
    }
}
