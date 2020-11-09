using Root_ASIS.Teachs;
using RootTools;
using RootTools.Inspects;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Threading;

namespace Root_ASIS
{
    public class MainTeach : NotifyProperty
    {
        #region Property
        string _sInfo = "OK"; 
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (_sInfo == value) return;
                _sInfo = value;
                OnPropertyChanged();
                if (value != "OK") m_log.Info("p_sInfo = " + value.ToString()); 
            }
        }
        #endregion

        #region Recipe
        public string m_sEXT = ".ASIS";
        string _sRecipe = "Recipe"; 
        public string p_sRecipe
        {
            get { return _sRecipe; }
            set
            {
                _sRecipe = value;
                OnPropertyChanged(); 
            }
        }

        public void SaveRecipe(string sFile)
        {
            Job job = new Job(sFile, true, m_log);
            if (job == null) p_sInfo = "Recipe File Save Error : " + sFile;
            else
            {
                p_sRecipe = sFile;
                m_bgwRecipeSave.RunWorkerAsync(job);
                m_bProgress = true; 
                m_bgwRecipeProgress.RunWorkerAsync(m_msSave); 
            }
        }

        public void OpenRecipe(string sFile)
        {
            Job job = new Job(sFile, false, m_log);
            if (job == null) p_sInfo = "Recipe File Open Error : " + sFile;
            else
            {
                p_sRecipe = sFile;
                m_bgwRecipeOpen.RunWorkerAsync(job);
                m_bProgress = true; 
                m_bgwRecipeProgress.RunWorkerAsync(m_msOpen);
            }
        }

        int _progressFile = 0; 
        public int p_progressFile
        {
            get { return _progressFile; }
            set
            {
                _progressFile = value;
                OnPropertyChanged(); 
            }
        }

        Registry m_reg; 
        public BackgroundWorker m_bgwRecipeSave = new BackgroundWorker();
        public BackgroundWorker m_bgwRecipeOpen = new BackgroundWorker();
        public BackgroundWorker m_bgwRecipeProgress = new BackgroundWorker();
        void InitRecipe()
        {
            m_reg = new Registry("MainTeachReg");
            m_msSave = m_reg.Read("Save", m_msSave);
            m_msOpen = m_reg.Read("Open", m_msOpen);
            m_bgwRecipeSave.DoWork += M_bgwRecipeSave_DoWork;
            m_bgwRecipeOpen.DoWork += M_bgwRecipeOpen_DoWork;
            m_bgwRecipeOpen.RunWorkerCompleted += M_bgwRecipeOpen_RunWorkerCompleted;
            m_bgwRecipeProgress.DoWork += M_bgwRecipeProgress_DoWork;
        }

        int m_msSave = 5000; 
        private void M_bgwRecipeSave_DoWork(object sender, DoWorkEventArgs e)
        {
            StopWatch sw = new StopWatch(); 
            string sRecipe = p_sRecipe.Replace(".ASIS", "");
            Job job = (Job)e.Argument;
            m_treeRoot.m_job = job;
            RunTree(Tree.eMode.JobSave);
            job.Close();
            m_aTeach[0].SaveRecipe(sRecipe + "0.Teach");
            m_aTeach[1].SaveRecipe(sRecipe + "1.Teach");
            m_aTeach[0].m_memoryPool.m_viewer.p_memoryData.FileSaveBMP(sRecipe + "0.bmp", 0);
            m_aTeach[1].m_memoryPool.m_viewer.p_memoryData.FileSaveBMP(sRecipe + "1.bmp", 0);
            p_sInfo = "Save Recipe File Finished";
            m_msSave = (m_msSave + (int)sw.ElapsedMilliseconds) / 2;
            m_reg.Write("Save", m_msSave);
            m_bProgress = false; 
        }

        int m_msOpen = 5000; 
        private void M_bgwRecipeOpen_DoWork(object sender, DoWorkEventArgs e)
        {
            StopWatch sw = new StopWatch();
            string sRecipe = p_sRecipe.Replace(".ASIS", "");
            Job job = (Job)e.Argument;
            m_treeRoot.m_job = job;
            RunTree(Tree.eMode.JobOpen);
            job.Close();
            m_aTeach[0].m_memoryPool.m_viewer.p_memoryData.FileOpenBMP(sRecipe + "0.bmp", 0);
            m_aTeach[1].m_memoryPool.m_viewer.p_memoryData.FileOpenBMP(sRecipe + "1.bmp", 0);
            m_msOpen = (m_msOpen + (int)sw.ElapsedMilliseconds) / 2;
            m_reg.Write("Open", m_msOpen);
            m_bProgress = false; 
        }

        private void M_bgwRecipeOpen_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string sRecipe = p_sRecipe.Replace(".ASIS", "");
            m_aTeach[0].OpenRecipe(sRecipe + "0.Teach");
            m_aTeach[1].OpenRecipe(sRecipe + "1.Teach");
            RunTree(Tree.eMode.Init);
            m_aTeach[0].RunTreeAOI(Tree.eMode.Init);
            m_aTeach[1].RunTreeAOI(Tree.eMode.Init);
            p_sInfo = "Open Recipe File Finished";
        }

        bool m_bProgress = false; 
        private void M_bgwRecipeProgress_DoWork(object sender, DoWorkEventArgs e)
        {
            int msProgress = (int)e.Argument; 
            StopWatch sw = new StopWatch(); 
            while (m_bProgress)
            {
                p_progressFile = (int)Math.Min((100 * sw.ElapsedMilliseconds / msProgress), 95); 
                Thread.Sleep(10); 
            }
            p_progressFile = 100; 
        }
        #endregion

        #region Tree
        public TreeRoot m_treeRoot;
        void InitTree()
        {
            m_treeRoot = new TreeRoot(m_id, m_log);
            RunTree(Tree.eMode.RegRead);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode eMode)
        {
            m_treeRoot.p_eMode = eMode;
            RunTreeArray(m_treeRoot.GetTree("Array")); 
        }

        public void RunTreeArray(Tree tree)
        {
            Strip.p_szBlock = tree.Set(Strip.p_szBlock, Strip.p_szBlock, "Block", "Number of Block");
            Strip.p_szUnit = tree.Set(Strip.p_szUnit, Strip.p_szUnit, "Unit", "Number of Unit");
            Strip.p_eUnitOrder = (Strip.eUnitOrder)tree.Set(Strip.p_eUnitOrder, Strip.p_eUnitOrder, "Order", "Unit Numbering Order");
            List<string> asOrder = new List<string>();
            string sOrder = Strip.p_eUnitSecondOrder.ToString(); 
            switch (Strip.p_eUnitOrder)
            {
                case Strip.eUnitOrder.Left:
                case Strip.eUnitOrder.Right:
                    asOrder.Add(Strip.eUnitOrder.Down.ToString());
                    asOrder.Add(Strip.eUnitOrder.Up.ToString());
                    break;
                case Strip.eUnitOrder.Down:
                case Strip.eUnitOrder.Up:
                    asOrder.Add(Strip.eUnitOrder.Left.ToString());
                    asOrder.Add(Strip.eUnitOrder.Right.ToString());
                    break;
            }
            sOrder = tree.Set(sOrder, sOrder, asOrder, "Second Order", "Unit Numbering Order");
            foreach (Strip.eUnitOrder order in Enum.GetValues(typeof(Strip.eUnitOrder)))
            {
                if (order.ToString() == sOrder) Strip.p_eUnitSecondOrder = order; 
            }
        }
        #endregion

        string m_id; 
        ASIS_Engineer m_engineer;
        Log m_log; 
        Teach[] m_aTeach = new Teach[2];
        public MainTeach(string id, ASIS_Engineer engineer)
        {
            m_id = id;
            m_engineer = engineer;
            m_log = LogView.GetLog(id); 
            ASIS_Handler handler = (ASIS_Handler)m_engineer.ClassHandler();
            m_aTeach[0] = handler.m_aBoat[Module.Boat.eBoat.Boat0].m_teach; 
            m_aTeach[1] = handler.m_aBoat[Module.Boat.eBoat.Boat1].m_teach;
            InitRecipe();
            InitTree();
        }
    }
}
