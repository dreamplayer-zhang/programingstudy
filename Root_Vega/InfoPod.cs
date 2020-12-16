using RootTools;
using RootTools.Gem;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Threading;
using System.Windows.Controls;

namespace Root_Vega
{
    public class InfoPod : GemCarrierBase, ITool
    {
        #region Property
        public DateTime m_timeLotStart = new DateTime();
        public DateTime m_timeLotEnd = new DateTime();

        public enum eState
        {
            Empty,
            Placed,
            Load,
        }
        eState _eState = eState.Empty;
        public eState p_eState
        {
            get { return _eState; }
            set
            {
                if (_eState == value) return;
                m_log.Info(p_id + " eState : " + _eState.ToString() + " -> " + value.ToString());
                _eState = value;
                if (_eState == eState.Load) m_timeLotStart = DateTime.Now;
                OnPropertyChanged();
            }
        }

        string _id = "";
        public string p_id
        {
            get { return _id; }
            set
            {
                if (_id == value) return;
                _id = value;
                if (p_infoReticle != null) p_infoReticle.p_sCarrierID = _id;
            }
        }

        string _sModule = "";
        public string p_sModule 
        {
            get { return _sModule; }
            set { _sModule = value; }
        }

        public string CheckPlaced(ePresent present, bool isLPLoad)
        {
            switch (p_eState)
            {
                case eState.Empty: 
                    if (present == ePresent.Exist)
                    {
                        if (isLPLoad) p_eState = eState.Load;
                        else p_eState = eState.Placed;
                    }
                    break;
                case eState.Placed: if (present == ePresent.Empty) p_eState = eState.Empty; break;
                default: 
                    if (present != p_ePresentSensor)
                    {
                        p_ePresentSensor = present;
                        return "Error"; 
                    }
                    break; 
            }
            p_ePresentSensor = present;
            return "OK"; 
        }

        void RunTreeProperty(Tree tree)
        {
            _eState = (eState)tree.Set(_eState, _eState, "State", "Pod State");
            p_id = tree.Set(p_id, p_id, "ID", "Pod ID");
            p_sModule = tree.Set(p_sModule, p_sModule, "Module", "Module ID (Loadport)");
        }
        #endregion

        #region IRobotChild
        public string IsRunOK()
        {
            switch (p_eState)
            {
                case eState.Load: return "OK";
            }
            return p_id + " eState = " + p_eState.ToString();
        }

        public string IsGetOK(ref int teachRobot)
        {
            string sOK = IsRunOK();
            if (sOK != "OK") return sOK;
            if (p_infoReticle == null) return p_id + " IsGetOK : InfoReticle not Exist";
            teachRobot = m_nTeachRobot;
            return "OK";
        }

        public string IsPutOK(ref int posRobot)
        {
            string sOK = IsRunOK();
            if (sOK != "OK") return sOK;
            if (p_infoReticle != null) return p_id + " IsPutOK : InfoReticle Exist";
            posRobot = m_nTeachRobot;
            return "OK";
        }
        #endregion

        #region InfoReticle
        const int c_maxSlot = 1;
        public void InitSlot()
        {
            for (int n = 0; n < c_maxSlot; n++)
            {
                InfoReticle newSlot = new InfoReticle(p_id + "." + n.ToString("00"), this, m_engineer);
                newSlot.p_eState = GemSlotBase.eState.Empty;
                newSlot.p_sCarrierID = p_sCarrierID;
                newSlot.p_sLocID = p_sLocID;
                m_aGemSlot.Add(newSlot);
            }
        }

        string m_sInfoReticle = "";
        InfoReticle _infoReticle = null;
        public InfoReticle p_infoReticle
        {
            get { return _infoReticle; }
            set
            {
                m_sInfoReticle = (value == null) ? "" : value.p_id;
                _infoReticle = value;
                if (m_reg != null) m_reg.Write("sInfoReticle", m_sInfoReticle);
                OnPropertyChanged();
            }
        }

        public void SetInfoReticleExist()
        {
            p_infoReticle = (InfoReticle)m_aGemSlot[0]; 
            p_infoReticle.p_eState = GemSlotBase.eState.Exist; 
        }

        Registry m_reg = null;
        public void ReadInfoReticle_Registry()
        {
            m_reg = new Registry(p_id + ".InfoReticle");
            m_sInfoReticle = m_reg.Read("sInfoReticle", m_sInfoReticle);
            if (m_sInfoReticle == "") return;
            p_infoReticle = m_engineer.ClassHandler().GetGemSlot(m_sInfoReticle);
            p_infoReticle.p_eState = GemSlotBase.eState.Exist; 
        }
        #endregion

        #region UI
        public UserControl p_ui
        {
            get
            {
                InfoPod_UI ui = new InfoPod_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

        #region Process
        public void StartProcess()
        {
            if (p_infoReticle == null) return;
            Vega_Handler handler = (Vega_Handler)m_engineer.ClassHandler();
            handler.AddSequence(p_infoReticle);
            handler.m_process.ReCalcSequence();
            RunTreeReticle(Tree.eMode.Init);
        }

        public void AfterHome()
        {
            p_eReqTransfer = eTransfer.TransferBlocked;
            for (int n = 0; n < 100; n++)
            {
                if (EQ.IsStop()) return; 
                if (p_eTransfer != eTransfer.OutOfService) n = 200;
                else Thread.Sleep(10);
            }
            p_eReqTransfer = (p_ePresentSensor == ePresent.Exist) ? eTransfer.ReadyToUnload : eTransfer.ReadyToLoad;
            if (p_ePresentSensor == ePresent.Empty && m_gem !=null) m_gem.RemoveCarrierInfo(p_sLocID);
        }
        #endregion

        #region Tree
        public TreeRoot m_treeRootReticle;
        private void M_treeRootReticle_UpdateTree()
        {
            RunTreeReticle(Tree.eMode.Update);
            if (p_infoReticle != null) p_infoReticle.RegWrite(); 
            RunTreeReticle(Tree.eMode.Init); 
        }

        public void RunTreeReticle(Tree.eMode mode)
        {
            if (m_treeRootReticle == null) return;
            m_treeRootReticle.p_eMode = mode; 
            if (p_infoReticle != null) p_infoReticle.RunTree(m_treeRootReticle.GetTree("InfoReticle")); 
        }

        public override void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeProperty(m_treeRoot.GetTree("Property"));
            RunTreeGem(m_treeRoot.GetTree("Gem"));
        }

        int m_nTeachRobot = 0;
        public void RunTreeTeach(Tree tree)
        {
            m_nTeachRobot = tree.Set(m_nTeachRobot, m_nTeachRobot, p_sModule, "Robot Teach Index");
        }
        #endregion

        public InfoPod(ModuleBase module, string sLocID, IEngineer engineer, string sLogGroup = "")
        {
            m_module = module; 
            p_sModule = module.p_id;
            p_id = p_sModule + ".InfoPod";
            p_sCarrierID = p_sModule; 
            p_sLocID = sLocID;
            m_engineer = engineer;
           // m_gem = m_engineer.ClassGem();
            m_log = module.m_log;
            m_treeRootReticle = new TreeRoot(p_id, m_log);
            m_treeRootReticle.UpdateTree += M_treeRootReticle_UpdateTree;
            InitSlot();
            InitBase();
        }

        public void ThreadStop()
        {
        }
    }
}
