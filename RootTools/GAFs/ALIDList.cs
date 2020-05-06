using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Threading;

namespace RootTools.GAFs
{
    public class ALIDList : NotifyProperty
    {
        #region List ALID
        ObservableCollection<ALID> _aALID = new ObservableCollection<ALID>();
        public ObservableCollection<ALID> p_aALID
        {
            get { return _aALID; }
        }

        public void SaveFile(string sFile)
        {
            try
            {
                StreamWriter sw = new StreamWriter(new FileStream(sFile, FileMode.Create));
                foreach (ALID alid in p_aALID)
                {
                    alid.Save(sw);
                }
                sw.Close();
            }
            catch (Exception) { }
        }

        public void ClearALID()
        {
            foreach (ALID alid in p_aALID)
            {
                alid.p_bSet = false;
                alid.p_sMsg = "";
            }
            p_aSetALID.Clear();
        }
        #endregion

        #region UI Binding
        string _sInfo = "Last Error";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (_sInfo == value) return;
                _sInfo = value;
                OnPropertyChanged();
            }
        }

        Brush _brushAlarm = Brushes.White; 
        public Brush p_brushAlarm
        { 
            get { return _brushAlarm; }
            set
            {
                if (_brushAlarm == value) return;
                _brushAlarm = value;
                OnPropertyChanged(); 
            }
        }

        public void ShowPopup()
        {
            if (ALIDList_PopupUI.m_bShow) return; 
            ALIDList_PopupUI alidPopup = new ALIDList_PopupUI();
            alidPopup.Init(this);
            alidPopup.Show();
        }
        #endregion 

        #region List Set ALID
        ObservableCollection<ALID> _aSetALID = new ObservableCollection<ALID>();
        public ObservableCollection<ALID> p_aSetALID
        {
            get { return _aSetALID; }
        }

        DispatcherTimer m_timerSetALID = new DispatcherTimer();
        Queue<ALID> m_qSetALID = new Queue<ALID>();
        public void SetALID(ALID alid)
        {
            m_qSetALID.Enqueue(alid);
            p_sInfo = alid.p_sModule + " : " + alid.p_sDesc + ", " + alid.p_sMsg;
            p_brushAlarm = Brushes.Red;
        }

        private void M_timerSetALID_Tick(object sender, EventArgs e)
        {
            for (int n = p_aSetALID.Count - 1; n >= 0; n--)
            {
                if (p_aSetALID[n].p_bSet == false) p_aSetALID.RemoveAt(n); 
            }
            while (m_qSetALID.Count > 0)
            {
                ShowPopup(); 
                p_aSetALID.Add(m_qSetALID.Dequeue());
            }
            p_brushAlarm = (p_aSetALID.Count > 0) ? Brushes.Red : Brushes.White;
        }
        #endregion

        #region Tree
        private void M_tree_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            foreach (GAF.Group group in m_aGroup)
            {
                RunTree(m_treeRoot.GetTree(group.m_sGroup, false), group);
            }
        }

        void RunTree(Tree tree, GAF.Group group)
        {
            foreach (ALID alid in group.m_aALID)
            {
                alid.RunTree(tree, group.GetNextALID());
            }
        }
        #endregion

        string m_id;
        LogWriter m_log;
        List<GAF.Group> m_aGroup;
        public TreeRoot m_treeRoot;
        public void Init(string id, GAF gaf)
        {
            m_id = id;
            m_log = gaf.m_log;
            m_aGroup = gaf.m_aGroup;
            m_treeRoot = new TreeRoot(id, m_log);
            m_treeRoot.UpdateTree += M_tree_UpdateTree;
            RunTree(Tree.eMode.RegRead);
            ClearALID();
            m_timerSetALID.Interval = TimeSpan.FromMilliseconds(1);
            m_timerSetALID.Tick += M_timerSetALID_Tick;
            m_timerSetALID.Start();
        }

        public void ThreadStop()
        {
            m_timerSetALID.Stop();
        }

    }
}
