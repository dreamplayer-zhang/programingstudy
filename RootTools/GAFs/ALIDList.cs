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
        public List<ALID> m_aALID = new List<ALID>();

        public void SaveFile(string sFile)
        {
            try
            {
                StreamWriter sw = new StreamWriter(new FileStream(sFile, FileMode.Create));
                foreach (ALID alid in m_aALID)
                {
                    alid.Save(sw);
                }
                sw.Close();
            }
            catch (Exception) { }
        }

        public void ClearALID()
        {
            foreach (ALID alid in m_aALID)
            {
                alid.p_sMsg = "";
                alid.p_bSet = false;
            }
            p_alarmBlink = false;
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
        private bool _alarmBlink = false;
        public bool p_alarmBlink
        {
            get
            {
                return _alarmBlink;
            }
            set
            {
                if (_alarmBlink == value) return;
                _alarmBlink = value;
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
        public ObservableCollection<ALID> p_aSetALID { get; set; }

        DispatcherTimer m_timerSetALID = new DispatcherTimer();
        public void SetALID(ALID alid)
        {
            p_sInfo = alid.p_sModule + " : " + alid.p_sDesc + ", " + alid.p_sMsg;
            p_brushAlarm = Brushes.Red;
            p_alarmBlink = true;
        }

        private void M_timerSetALID_Tick(object sender, EventArgs e)
        {
            for (int n = p_aSetALID.Count - 1; n >= 0; n--)
            {
                if (p_aSetALID[n].p_bSet == false) p_aSetALID.RemoveAt(n);
            }
            foreach (ALID alid in m_aALID)
            {
                if (alid.p_bSet && (IsExistSetALID(alid) == false))
                {
                    ShowPopup();
                    p_aSetALID.Add(alid);
                }
            }
            p_brushAlarm = (p_aSetALID.Count > 0) ? Brushes.Red : Brushes.White;
        }

        bool IsExistSetALID(ALID alid)
        {
            foreach (ALID al in p_aSetALID)
            {
                if (al.m_sID == alid.m_sID) return true;
            }
            return false;
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
        Log m_log;
        List<GAF.Group> m_aGroup;
        public TreeRoot m_treeRoot;
        public void Init(string id, GAF gaf)
        {
            p_aSetALID = new ObservableCollection<ALID>();
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
