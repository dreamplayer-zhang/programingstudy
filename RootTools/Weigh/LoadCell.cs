using RootTools.Comm;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RootTools.Weigh
{
    public class LoadCell:ObservableObject
    {
        IEngineer engineer;
        Log m_log;
        double weight = 0;

        public void GetTools(ToolBox toolBox,bool bInit)
        {
            //toolBox.GetComm(ref m_rs232,)
        }
        public LoadCell(string id,IEngineer engineer)
        {
            p_id = id;
            this.engineer = engineer;
            m_log = LogView.GetLog(id);

            InitRS232();
            InitTreeSetup();
        }
        #region [Property]
        public string p_id { get; set; }
        string _sInfo = "OK";
        public string p_sInfo
        {
            get => _sInfo;
            set
            {
                if (_sInfo == value) return;
                _sInfo = value;
                RaisePropertyChanged();
                if (value == "OK") return;
                //m_tcpip.m_commLog.Add(CommLog.eType.Info, value);
                //m_log.Warn(value);
            }
        }
        #endregion
        #region [UI]
        public UserControl p_ui
        {
            get
            {
                LoadCell_UI ui = new LoadCell_UI();
                ui.Init(this);
                return ui;
            }
        }
        #endregion
        #region [Serial]
        public enum eSerial
        {
            RS232,
            RS485
        }
        public eSerial m_eSerial = eSerial.RS232;
        double m_secWaitReply = 1;
        void RunTreeSerial(Tree tree)
        {
            m_eSerial = (eSerial)tree.Set(m_eSerial, m_eSerial, "Type", "Serial Communication Type");
            m_secWaitReply = tree.Set(m_secWaitReply, m_secWaitReply, "Wait Reply", "Wait Reply Time (sec)");
        }
        #region [RS232]
        RS232 m_rs232;
        public RS232 p_rs232
        {
            get => m_rs232;
            set => SetProperty(ref m_rs232, value);
        }
        void InitRS232()
        {
            m_rs232 = new RS232(p_id, m_log);
            m_rs232.OnReceive += M_rs232_OnReceive;
        }

        private void M_rs232_OnReceive(string sRead)
        {
            if (sRead.Length < 9) return;
            p_rs232.m_commLog.Add(CommLog.eType.Receive, "CAS Receive = " + sRead.Trim());
        }
        #endregion
        #endregion
        #region [Tree]
        public TreeRoot m_treeRootSetup;
        void InitTreeSetup()
        {
            m_treeRootSetup = new TreeRoot(p_id + ".Setup", m_log);
            m_treeRootSetup.UpdateTree += M_treeRootSetup_UpdateTree; ;
            RunTreeSetup(Tree.eMode.RegRead);
        }

        private void M_treeRootSetup_UpdateTree()
        {
            RunTreeSetup(Tree.eMode.Update);
            RunTreeSetup(Tree.eMode.Init);
            RunTreeSetup(Tree.eMode.RegWrite);
        }

        public void RunTreeSetup(Tree.eMode eMode)
        {
            m_treeRootSetup.p_eMode = eMode;
            RunTreeSerial(m_treeRootSetup.GetTree("Serial"));
        }
        #endregion
    }
}
