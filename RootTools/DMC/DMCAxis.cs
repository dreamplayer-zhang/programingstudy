using RootTools.Trees;
using System.Windows.Controls;

namespace RootTools.DMC
{
    public class DMCAxis : NotifyProperty
    {
        #region Property
        #endregion

        #region Binding
        public string p_sID
        {
            get { return m_nAxis.ToString("00") + "." + m_sAxisID; }
        }

        float _fJoint = 0;
        public float p_fJoint
        {
            get { return _fJoint; }
            set
            {
                if (_fJoint == value) return;
                _fJoint = value;
                OnPropertyChanged();
            }
        }

        float _fCartesian = 0;
        public float p_fCartesian
        {
            get { return _fCartesian; }
            set
            {
                if (_fCartesian == value) return;
                _fCartesian = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region UI
        public UserControl p_ui
        {
            get
            {
                DMCAxis_UI ui = new DMCAxis_UI();
                ui.Init(this);
                return ui;
            }
        }
        #endregion

        #region Tree
        string m_sAxisID = "Axis"; 
        public void RunTree(Tree tree)
        {
            m_sAxisID = tree.Set(m_sAxisID, m_sAxisID, m_id, "Axis ID");
            if (tree.p_treeRoot.p_eMode == Tree.eMode.Update) OnPropertyChanged("p_sID"); 
        }
        #endregion

        #region Jog
        public bool m_bJog = false; 
        public void Jog_Minus_Move()
        {
            if (m_dmcControl.p_nRobot <= 0) return;
            m_bJog = true; 
            CoreMon.setJogOn(m_dmcControl.p_nRobot, m_nAxisBitID, 0);
        }
        
        public void Jog_Plus_Move()
        {
            if (m_dmcControl.p_nRobot <= 0) return;
            m_bJog = true;
            CoreMon.setJogOn(m_dmcControl.p_nRobot, 0, m_nAxisBitID);
        }
        
        public void Jog_Minus_Stop()
        {
            if (m_dmcControl.p_nRobot <= 0) return;
            CoreMon.setJogOff(m_dmcControl.p_nRobot, m_nAxisBitID, 0);
            m_bJog = false;
        }

        public void Jog_Plus_Stop()
        {
            if (m_dmcControl.p_nRobot <= 0) return;
            CoreMon.setJogOff(m_dmcControl.p_nRobot, 0, m_nAxisBitID);
            m_bJog = false;
        }
        #endregion

        string m_id;
        int m_nAxis = 0;
        uint m_nAxisBitID = 1;
        DMCControl m_dmcControl;
        public DMCAxis(int nAxis, DMCControl dmcControl)
        {
            m_id = nAxis.ToString("00") + ".Axis";
            m_nAxis = nAxis; 
            m_dmcControl = dmcControl;
            for (int n = 0; n < nAxis; n++) m_nAxisBitID *= 2; 
        }
    }
}
