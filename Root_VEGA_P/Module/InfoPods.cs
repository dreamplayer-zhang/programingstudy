using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Gem;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace Root_VEGA_P.Module
{
    public class InfoPods : GemCarrierBase
    {
        #region Property
        public DateTime m_timeLotStart = new DateTime();
        public DateTime m_timeLotEnd = new DateTime();

        public enum eState
        {
            Empty,
            Placed,
            Dock,
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
                if (_eState == eState.Dock) m_timeLotStart = DateTime.Now;
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
                if (m_aGemSlot == null) return;
                foreach (GemSlotBase slot in m_aGemSlot) slot.p_sCarrierID = value;
            }
        }

        public string p_sModule { get; set; }

        public string CheckPlaced(ePresent present)
        {
            switch (p_eState)
            {
                case eState.Empty: if (present == ePresent.Exist) p_eState = eState.Placed; break;
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
            _eState = (eState)tree.Set(_eState, _eState, "State", "Carrier State");
            p_id = tree.Set(p_id, p_id, "ID", "Carrier ID");
            p_sModule = tree.Set(p_sModule, p_sModule, "Module", "Module ID (Loadport)");
        }
        #endregion

        #region InfoPod
        Stack<InfoPod> m_infoPod = new Stack<InfoPod>();
        public InfoPod p_infoPod
        {
            get
            {
                if (m_infoPod.Count == 0) return null;
                return m_infoPod.Peek();
            }
            set
            {
                int nPod = (value != null) ? (int)value.p_ePod : -1;
                if (value == null) m_infoPod.Pop();
                else m_infoPod.Push(value);
                m_reg.Write("InfoPod", nPod);
                value?.WriteReg();
                OnPropertyChanged();
            }
        }

        Registry m_reg = null;
        public void ReadPod_Registry()
        {
            m_reg = new Registry("InfoPod");
            int nPod = m_reg.Read(p_id, -1);
            if (nPod < 0) return;
            NewInfoPod(nPod + 1);
            p_infoPod.ReadReg();
        }

        public int GetPodCount()
        {
            return m_infoPod.Count; 
        }

        public void NewInfoPod(int nPod)
        {
            ClearInfoPod();
            for (int n = 0; n < nPod; n++) m_infoPod.Push(new InfoPod((InfoPod.ePod)n));
        }

        public void ClearInfoPod()
        {
            m_infoPod.Clear();
        }
        #endregion

        #region Tree
        public override void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeProperty(m_treeRoot.GetTree("Property"));
            RunTreeGem(m_treeRoot.GetTree("Gem"));
        }
        #endregion

        public InfoPods(ModuleBase module, string sLocID, IEngineer engineer)
        {
            m_module = module;
            p_sModule = module.p_id;
            p_id = p_sModule + ".InfoPod";
            p_sLocID = sLocID;
            m_engineer = engineer;
            m_gem = engineer.ClassGem();
            m_log = LogView.GetLog(module.p_id);
            InitBase(); 
        }

        public void ThreadStop()
        {
            //foreach (InfoCarrier_UI ui in m_aCarrierUI) ui.m_timer.Stop();
        }

    }
}
