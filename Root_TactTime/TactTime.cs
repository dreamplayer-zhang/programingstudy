using Microsoft.Win32;
using Root_TactTime.Mold;
using Root_TactTime.Pine2;
using RootTools;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_TactTime
{
    public class TactTime : NotifyProperty
    {
        #region Time
        double _secRun = 0;
        public double p_secRun
        {
            get { return _secRun; }
            set
            {
                _secRun = Math.Round(100 * value) / 100;
                OnPropertyChanged();
                foreach (Module module in m_aModule) module.OnPropertyChanged("p_fProgress"); 
            }
        }
        #endregion

        #region Unload & TactTime
        int m_nUnload = 0; 
        double m_secUnload0 = 0;
        public void Unload(double secUnload)
        {
            if (m_nUnload > 0) p_secTact = (secUnload - m_secUnload0) / m_nUnload;
            else
            {
                m_secUnload0 = secUnload;
                p_secTact = 0; 
            }
            m_nUnload++;
        }

        double _secTact = 0; 
        public double p_secTact
        {
            get { return _secTact; }
            set
            {
                _secTact = Math.Round(100 * value) / 100;
                OnPropertyChanged(); 
            }
        }
        #endregion

        #region Sequence
        public class Sequence
        {
            public string m_sFrom = "";
            public string m_sTo = ""; 
            public Sequence(string sFrom, string sTo)
            {
                m_sFrom = sFrom;
                m_sTo = sTo; 
            }
        }
        public List<Sequence> m_aSequence = new List<Sequence>(); 
        public void AddSequence(string sFrom, string sTo)
        {
            m_aSequence.Add(new Sequence(sFrom, sTo)); 
        }

        public void ClearSequence(bool bClearSequence)
        {
            p_secRun = 0;
            m_iStrip = 0;
            foreach (Module module in m_aModule) module.Clear();
            foreach (Loader loader in m_aLoader) loader.Clear();
            m_nUnload = 0;
            p_secTact = 0; 
            if (bClearSequence) m_aSequence.Clear();
        }

        public void SaveSequence()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Sequence Files (*.Sequence)|*.Sequence";
            if (dlg.ShowDialog() == false) return;
            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                fs = new FileStream(dlg.FileName, FileMode.Create);
                sw = new StreamWriter(fs);
                foreach (Sequence sequence in m_aSequence)
                {
                    sw.WriteLine(sequence.m_sFrom + "\t" + sequence.m_sTo); 
                }
            }
            finally
            {
                sw.Close();
                fs.Close();
            }
        }

        public void OpenSequence()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Sequence Files (*.Sequence)|*.Sequence";
            if (dlg.ShowDialog() == false) return;
            m_aSequence.Clear(); 
            FileStream fs = null;
            StreamReader sr = null;
            try
            {
                fs = new FileStream(dlg.FileName, FileMode.Open);
                sr = new StreamReader(fs);
                string sRead = sr.ReadLine(); 
                while (sRead != null)
                {
                    string[] asRead = sRead.Split('\t');
                    AddSequence(asRead[0], asRead[1]);
                    sRead = sr.ReadLine();
                }
            }
            finally
            {
                sr.Close();
                fs.Close();
            }
        }

        public void Undo()
        {
            m_aSequence.RemoveAt(m_aSequence.Count - 1);
            m_timer.Interval = TimeSpan.FromSeconds(0.001);
            StartSimulation(); 
            m_timer.Interval = TimeSpan.FromSeconds(m_secSimul);
        }
        #endregion

        #region Run Simulation
        public Queue<Sequence> m_qSequence = new Queue<Sequence>();
        public void StartSimulation()
        {
            if (m_qSequence.Count > 0) return;
            ClearSequence(false);
            foreach (Sequence sequence in m_aSequence) m_qSequence.Enqueue(sequence);
            m_timer.Start();
        }

        DispatcherTimer m_timer = new DispatcherTimer();
        void initTimer()
        {
            m_timer.Interval = TimeSpan.FromSeconds(m_secSimul);
            m_timer.Tick += M_timer_Tick;
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            if (m_qSequence.Count == 0)
            {
                m_timer.Stop();
                return; 
            }
            Sequence sequence = m_qSequence.Dequeue();
            Module module = GetModule(sequence.m_sTo);
            if (module != null) module.MoveFrom(GetPicker(sequence.m_sFrom), false); 
            else
            {
                Picker picker = GetPicker(sequence.m_sTo);
                module = GetModule(sequence.m_sFrom);
                if (module != null) picker.MoveFrom(module, false);
            }
        }

        double m_secSimul = 1; 
        void RunTreeSimul(Tree tree)
        {
            m_secSimul = tree.Set(m_secSimul, m_secSimul, "Interval", "Simul Interval (sec)");
            m_timer.Interval = TimeSpan.FromSeconds(m_secSimul); 
        }
        #endregion

        #region Color
        public enum eColor
        {
            None,
            From,
            To
        }
        public Dictionary<eColor, Brush> m_aColor = new Dictionary<eColor, Brush>(); 
        void InitColor()
        {
            m_aColor.Add(eColor.None, Brushes.Beige);
            m_aColor.Add(eColor.From, Brushes.LightGreen);
            m_aColor.Add(eColor.To, Brushes.LightPink);
        }

        public void ClearColor()
        {
            foreach (Module module in m_aModule) module.p_eColor = eColor.None;
            foreach (Picker picker in m_aPicker) picker.p_eColor = eColor.None; 
        }
        #endregion

        #region Module, Loader, Picker
        public List<Module> m_aModule = new List<Module>();
        Module GetModule(string id)
        {
            foreach (Module module in m_aModule)
            {
                if (module.p_id == id) return module; 
            }
            return null; 
        }

        public List<Loader> m_aLoader = new List<Loader>(); 

        public List<Picker> m_aPicker = new List<Picker>();
        Picker GetPicker(string id)
        {
            foreach (Picker picker in m_aPicker)
            {
                if (picker.p_id == id) return picker;
            }
            return null;
        }
        #endregion

        #region Tree
        public TreeRoot m_treeRoot;
        void InitTree()
        {
            m_treeRoot = new TreeRoot("TactTime", null);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);

        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode eMode)
        {
            m_treeRoot.p_eMode = eMode;
            RunTreeSimul(m_treeRoot.GetTree("Simulation"));
            foreach (Loader loader in m_aLoader) loader.RunTree(m_treeRoot.GetTree(loader.p_id));
            RunTreeRunTime(m_treeRoot.GetTree("ModuleRun")); 
        }

        void RunTreeRunTime(Tree tree)
        {
            foreach (Module module in m_aModule)
            {
                module.m_secRun[1] = tree.Set(module.m_secRun[1], module.m_secRun[1], module.p_id, "RunTime (sec)"); 
            }
        }
        #endregion

        dynamic m_model = null; 
        public int m_iStrip = 0; 
        public TactTime()
        {
            p_secRun = 0;
            m_model = new Pine2_Picker(this); 

            InitColor(); 
            InitTree();
            initTimer();
        }
    }
}
