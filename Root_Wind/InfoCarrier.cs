using RootTools;
using RootTools.Gem;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Root_Wind
{
    public class InfoCarrier : GemCarrierBase, ITool
    {
        #region Property
        public DateTime m_timeLotStart = new DateTime();
        public DateTime m_timeLotEnd = new DateTime();

        public enum eState
        {
            Empty,
            Placed,
            Dock,
            Run,
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
        string _sModule = "";
        public string p_sModule 
        {
            get { return _sModule; }
            set { _sModule = value; } 
        }

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
            _eState = (eState)tree.Set(_eState, _eState, "State", "Pod State");
            p_id = tree.Set(p_id, p_id, "ID", "Pod ID");
            p_sModule = tree.Set(p_sModule, p_sModule, "Module", "Module ID (Loadport)");
        }
        #endregion

        #region IWTRChild
        public string IsRunOK()
        {
            switch (p_eState)
            {
                case eState.Dock:
                case eState.Run: return "OK";
            }
            return p_id + " eState = " + p_eState.ToString();
        }

        public string IsGetOK(int nID)
        {
            string sOK = IsRunOK();
            if (sOK != "OK") return sOK;
            if (GetInfoWafer(nID) == null) return p_id + " IsGetOK : InfoWafer not Exist";
            return "OK";
        }

        public string IsPutOK(int nID)
        {
            string sOK = IsRunOK();
            if (sOK != "OK") return sOK;
            if (GetInfoWafer(nID) != null) return p_id + " IsPutOK : InfoWafer Exist";
            return "OK";
        }

        public int GetWTRTeach()
        {
            return m_waferSize.GetData(p_eWaferSize).m_nTeachWTR;
        }
        #endregion

        #region Slot
        const int c_maxSlot = 25;
        void InitSlot()
        {
            for (int n = 0; n < c_maxSlot; n++)
            {
                InfoWafer newSlot = new InfoWafer(p_sModule + "." + (n + 1).ToString("00"), m_engineer);
                newSlot.p_eState = GemSlotBase.eState.Empty;
                newSlot.p_sCarrierID = p_sCarrierID;
                newSlot.p_sLocID = p_sLocID;
                m_aGemSlot.Add(newSlot);
                p_aInfoWafer.Add(null); 
            }
            InitChildID();
        }

        int _lWafer = c_maxSlot;
        public int p_lWafer
        {
            get { return _lWafer; }
            set
            {
                if (_lWafer == value) return;
                if (value > c_maxSlot) return;
                m_log.Info(p_id + " lWafer : " + _lWafer.ToString() + " -> " + value.ToString());
                _lWafer = value;
                InitChildID();
            }
        }

        public enum eSlotType
        {
            Up,
            Down
        }
        eSlotType _eSlotType = eSlotType.Up;
        public eSlotType p_eSlotType
        {
            get { return _eSlotType; }
            set
            {
                if (_eSlotType == value) return;
                m_log.Info(p_id + " SlotTyper : " + _eSlotType.ToString() + " -> " + value.ToString());
                _eSlotType = value;
                InitChildID();
            }
        }

        void InitChildID()
        {
            m_asInfoWafer.Clear();
            for (int n = 0; n < p_lWafer; n++)
            {
                switch (p_eSlotType)
                {
                    case eSlotType.Down: m_aGemSlot[n].p_id = p_sModule + "." + (p_lWafer - n).ToString("00"); break;
                    case eSlotType.Up: m_aGemSlot[n].p_id = p_sModule + "." + (n + 1).ToString("00"); break;
                }
                m_asInfoWafer.Add(m_aGemSlot[n].p_id);
            }
        }

        InfoWafer.eWaferSize _eWaferSize = InfoWafer.eWaferSize.eError;
        public InfoWafer.eWaferSize p_eWaferSize
        {
            get { return _eWaferSize; }
            set
            {
                if (_eWaferSize == value) return;
                WaferSize.Data data = m_waferSize.GetData(value);
                if (data.m_bEnable == false) return;
                m_log.Info(p_id + " eWaferSize : " + _eWaferSize.ToString() + " -> " + value.ToString());
                _eWaferSize = value;
                p_lWafer = data.m_lWafer;
            }
        }

        void RunTreeSlot(Tree tree)
        {
            p_eWaferSize = (InfoWafer.eWaferSize)tree.Set(p_eWaferSize, p_eWaferSize, "Size", "Wafer Size");
            p_lWafer = tree.Set(p_lWafer, p_lWafer, "Count", "Slot Count", true, true);
        }
        #endregion

        #region InfoWafer
        /// <summary> InfoWafer.p_id List </summary>
        public List<string> m_asInfoWafer = new List<string>();
        ObservableCollection<InfoWafer> _aInfoWafer = new ObservableCollection<InfoWafer>();
        /// <summary> InfoWafer List </summary>
        public ObservableCollection<InfoWafer> p_aInfoWafer
        {
            get { return _aInfoWafer; }
            set
            {
                if (_aInfoWafer == value) return;
                _aInfoWafer = value;
                OnPropertyChanged();
            }
        }

        InfoWafer GetInfoWafer(string sWafer)
        {
            foreach (InfoWafer infoWafer in p_aInfoWafer)
            {
                if (infoWafer != null)
                {
                    if (infoWafer.p_id == sWafer) return infoWafer; 
                }
            }
            return null; 
        }

        public void ClearInfoWafer()
        {
            for (int n = 0; n < p_aInfoWafer.Count; n++) SetInfoWafer(n, null); 
        }

        Registry m_reg = null;
        public void ReadInfoWafer_Registry()
        {
            ClearInfoWafer(); 
            m_reg = new Registry(p_id + ".Registry");
            p_lWafer = m_reg.Read("p_iWafer", c_maxSlot);
            for (int n = 0; n < p_lWafer; n++)
            {
                string sInfoWafer = m_reg.Read("sInfoWafer." + n.ToString("00"), "");
                if (sInfoWafer != "") p_aInfoWafer[n] = (InfoWafer)m_aGemSlot[n];
                m_aGemSlot[n].p_eState = (sInfoWafer != "") ? GemSlotBase.eState.Exist : GemSlotBase.eState.Empty; 
            }
            RunTreeWafer(Tree.eMode.Init); 
        }

        void SaveInfoWafer(int nID)
        {
            if (m_reg == null) return; 
            m_reg.Write("p_iWafer", p_lWafer);
            string sInfoWafer = (p_aInfoWafer[nID] == null) ? "" : p_aInfoWafer[nID].p_id;
            m_reg.Write("sInfoWafer." + nID.ToString("00"), sInfoWafer);
        }

        public InfoWafer GetInfoWafer(int nID)
        {
            if (nID < 0) return null;
            if (nID >= p_lWafer) return null;
            return p_aInfoWafer[nID];
        }

        public void SetInfoWafer(int nID, InfoWafer infoWafer)
        {
            if (nID < 0) return;
            if (nID >= p_lWafer) return;
            p_aInfoWafer[nID] = infoWafer;
            SaveInfoWafer(nID); 
        }

        public void SetInfoWafer(int nID)
        {
            if (nID < 0) return;
            if (nID >= p_lWafer) return;
            p_aInfoWafer[nID] = (InfoWafer)m_aGemSlot[nID];
            SaveInfoWafer(nID); 
        }
        #endregion

        #region Process
        public void StartProcess(string sWafer)
        {
            m_engineer.ClassHandler().AddSequence(GetInfoWafer(sWafer));
            m_engineer.ClassHandler().CalcSequence(); 
            RunTreeWafer(Tree.eMode.Init); 
        }

        public void StartAllProcess()
        {
            for (int n = 0; n < p_lWafer; n++)
            {
                if (p_aInfoWafer[n] != null) m_engineer.ClassHandler().AddSequence(p_aInfoWafer[n]);
            }
            m_engineer.ClassHandler().CalcSequence();
            RunTreeWafer(Tree.eMode.Init);
        }

        public void AfterHome()
        {
            p_eReqTransfer = eTransfer.TransferBlocked;
            for (int n = 0; n < 100; n++)
            {
                if (p_eTransfer != eTransfer.OutOfService) n = 200;
                else Thread.Sleep(10);
            }
            p_eReqTransfer = (p_ePresentSensor == ePresent.Exist) ? eTransfer.ReadyToUnload : eTransfer.ReadyToLoad;
            if (p_ePresentSensor == ePresent.Empty) m_gem.RemoveCarrierInfo(p_sLocID);
        }
        #endregion

        #region Mapping Function
        public string SetMapData(List<GemSlotBase.eState> aSlotState)
        {
            if (p_lWafer != aSlotState.Count) return "SetMapData Lendth Error";
            for (int n = 0; n < p_lWafer; n++)
            {
                GemSlotBase.eState state = aSlotState[n]; 
                if (state == GemSlotBase.eState.Exist) SetInfoWafer(n); 
                else SetInfoWafer(n, null);
            }
            return "OK";
        }
        #endregion

        #region UI
        public UserControl p_ui
        {
            get
            {
                InfoCarrier_UI ui = new InfoCarrier_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

        #region Tree
        public WaferSize m_waferSize;
        public void RunTreeSetup(Tree tree)
        {
            p_eSlotType = (eSlotType)tree.Set(p_eSlotType, p_eSlotType, "Slot Type", "Slot Numbering Type");
            m_waferSize.RunTree(tree.GetTree("Wafer Size", false), true);
        }

        public TreeRoot m_treeRootWafer;
        private void M_treeRootWafer_UpdateTree()
        {
            RunTreeWafer(Tree.eMode.Update);
            RunTreeWafer(Tree.eMode.RegWrite);
            RunTreeWafer(Tree.eMode.Init);
        }

        public void RunTreeWafer(Tree.eMode mode)
        {
            m_treeRootWafer.p_eMode = mode;
            for (int n = 0; n < p_aInfoWafer.Count; n++)
            {
                InfoWafer infoWafer = p_aInfoWafer[n];
                if (infoWafer != null) infoWafer.RunTree(m_treeRootWafer.GetTree(m_asInfoWafer[n], false));
                if (mode == Tree.eMode.RegWrite) SaveInfoWafer(n);
            }
        }

        public override void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeProperty(m_treeRoot.GetTree("Property"));
            RunTreeSlot(m_treeRoot.GetTree("Slot"));
            RunTreeGem(m_treeRoot.GetTree("Gem"));
        }
        #endregion

        public InfoCarrier(ModuleBase module, string sLocID, IEngineer engineer)
        {
            m_module = module;
            p_sModule = module.p_id;
            p_id = p_sModule + ".InfoCarrier";
            p_sCarrierID = p_sModule;
            p_sLocID = sLocID;
            m_engineer = engineer;
            m_gem = m_engineer.ClassGem();
            m_log = module.m_log;
            m_waferSize = new WaferSize(p_sModule, true, true);
            m_treeRootWafer = new TreeRoot(p_id, m_log);
            m_treeRootWafer.UpdateTree += M_treeRootWafer_UpdateTree;
            InitSlot();
            InitBase(); 
        }

        public void ThreadStop()
        {
        }
    }
}
