using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using static RootTools.Gem.XGem.XGem;

namespace RootTools.Gem
{
    public class GemCarrierBase : NotifyProperty
    {
        #region GemState
        public enum eGemState
        {
            NotRead,
            WaitForHost,
            VerificationOK,
            VerificationFailed
        }

        eGemState _eStateCarrierID = eGemState.NotRead;
        public eGemState p_eStateCarrierID
        {
            get { return _eStateCarrierID; }
            set
            {
                if (_eStateCarrierID == value) return;
                m_log.Info("p_eState " + _eStateCarrierID.ToString() + " -> " + value.ToString());
                _eStateCarrierID = value;
            }
        }

        eGemState _eStateSlotMap = eGemState.NotRead;
        public eGemState p_eStateSlotMap
        {
            get { return _eStateSlotMap; }
            set
            {
                if (_eStateSlotMap == value) return;
                m_log.Info("p_eStateSlotMap " + _eStateSlotMap.ToString() + " -> " + value.ToString());
                _eStateSlotMap = value;
            }
        }

        public enum ePresent
        {
            Empty,
            Exist,
            Unknown
        }
        ePresent _ePresentSensor = ePresent.Empty;
        public ePresent p_ePresentSensor 
        {
            get { return _ePresentSensor; }
            set
            {
                if (_ePresentSensor == value) return;
                m_log.Info("p_ePresentSensor " + _ePresentSensor.ToString() + " -> " + value.ToString());
                _ePresentSensor = value;
                if (m_gem != null && !m_gem.p_bOffline)
                {
                    switch (value)
                    {
                        case ePresent.Empty: m_module.p_sInfo = m_gem.SendCarrierPresentSensor(this, false); break;
                        case ePresent.Exist: m_module.p_sInfo = m_gem.SendCarrierPresentSensor(this, true); break;
                    }
                    
                    SendCarrierOn();
                }
                
            }
        }

        public enum eAssociated
        {
            NotAssociated,
            Associated
        }
        eAssociated _eAssociated = eAssociated.NotAssociated;

        public eAssociated p_eAssociated
        {
            get { return _eAssociated; }
            set 
            {
                if (_eAssociated == value) return;
                m_log.Info("p_eAssociated State " + _eAssociated.ToString() + " -> " + value.ToString());
                _eAssociated = value;
            }
        }

        eAssociated _eReqAssociated = eAssociated.NotAssociated;

        public eAssociated p_eReqAssociated
        {
            get { return _eReqAssociated; }
            set 
            {
                if (_eReqAssociated == value) return;
                m_log.Info("p_eAssociated State " + _eReqAssociated.ToString() + " -> " + value.ToString());
                _eReqAssociated = value;                
            }
        }

        public void SetReqAssociated(eAssociated value)
        {
            p_eReqAssociated = value;
            m_gem.SendLPInfo(this);
        }

        public void RunTreeAssociated(Tree tree)
        {
            p_eReqAssociated = (eAssociated)tree.Set(p_eReqAssociated, p_eReqAssociated, "Request", "Request Asocciated State");
            tree.Set(p_eAssociated, p_eAssociated, "State", "Associated State", true, true);
        }
        #endregion

        #region Carrier
        bool _bCarrierOn = false; 
        public bool p_bCarrierOn
        {
            get { return _bCarrierOn; }
            set
            {
                if (_bCarrierOn == value) return; 
                if (m_gem == null) return;
                m_log.Info("CarrierOn : " + _bCarrierOn.ToString() + " -> " + value.ToString()); 
                _bCarrierOn = value;
                m_gem.SendCarrierOn(this, value); 
            }
        }

        void SendCarrierOn()
        {
            switch (p_eTransfer)
            {
                case eTransfer.TransferBlocked:
                if (p_ePresentSensor == ePresent.Exist)
                {
                    if (m_gem != null) p_bCarrierOn = true;
                    //m_bReqReadCarrierID = true;//jws 210414 check 필요 여기서 rfid 리드 요청함
                }
            
                if (p_ePresentSensor == ePresent.Empty) 
                {
                    if (m_gem != null) p_bCarrierOn = false;
                }
                break;
            }
        }

        void CMSDelCarrierInfo()
        {
            m_gem?.CMSDelCarrierInfo(this);
        }

        void RunTreeCarrier(Tree tree)
        {
            p_sCarrierID = tree.Set(p_sCarrierID, p_sCarrierID, "Carrier ID", "Carrier ID"); 
            p_bCarrierOn = tree.Set(p_bCarrierOn, p_bCarrierOn, "CarrierOn", "CMS Set Carrier On");
            tree.SetButton(new RelayCommand(CMSDelCarrierInfo), "Delete", "Delete", "Delete Carrier Info");
        }
        #endregion
          
        #region Carrier Tranfer
        public enum eTransfer
        {
            OutOfService,
            TransferBlocked,
            ReadyToLoad,
            ReadyToUnload
        }
        eTransfer _eReqTransfer = eTransfer.ReadyToLoad;
        public eTransfer p_eReqTransfer
        {
            get { return _eReqTransfer; }
            set
            { 
                if (_eReqTransfer == value) return;
                if ((m_gem == null) || m_gem.p_bOffline) return;
                m_log.Info("p_eTransfer " + _eReqTransfer.ToString() + " -> " + value.ToString());
                p_eReqTransfer = value;
                if (EQ.p_bSimulate) p_eTransfer = value; 
                switch (_eReqTransfer)
                {
                    case eTransfer.ReadyToLoad:
                        if (m_gem != null) m_module.p_sInfo = m_gem.CMSSetReadyToLoad(this);
                        break;
                    case eTransfer.ReadyToUnload:
                        if (m_gem != null) m_module.p_sInfo = m_gem.CMSSetReadyToUnload(this);
                        break;
                    default:
                        m_gem.SendLPInfo(this);
                        break;
                }
                RunTree(Tree.eMode.Init);
            }
        }

        eTransfer _eTransfer = eTransfer.OutOfService;
        public eTransfer eBeforTransfer= eTransfer.OutOfService;
        public eTransfer p_eTransfer
        {
            get { return _eTransfer; }
            set
            {
                if (_eTransfer == value) return;
                eBeforTransfer = value;
                m_log.Info("p_eTransfer " + _eTransfer.ToString() + " -> " + value.ToString());
                _eTransfer = value;
                OnPropertyChanged();
                _eReqTransfer = value; 
                SendCarrierOn();
                RunTree(Tree.eMode.Init); 
            }
        }

        public void RunTreeTransfer(Tree tree)
        {
            p_eReqTransfer = (eTransfer)tree.Set(p_eReqTransfer, p_eReqTransfer, "Request", "Request Trasfer State");
            tree.Set(p_eTransfer, p_eTransfer, "State", "Transfer State", true, true);
        }
        #endregion

        #region Remote Variable
        public bool m_bReqReadCarrierID = false;
        public bool m_bReqLoad = false;
        public bool m_bReqUnload = false;
        public bool m_bReqGem = false;
        #endregion

        #region Carrier Access
        public enum eAccess
        {
            NotAccessed,
            InAccessed,
            CarrierCompleted,
            CarrierStoped
        }
        eAccess _eReqAccess = eAccess.NotAccessed; 
        public eAccess p_eReqAccess
        {
            get { return _eReqAccess; }
            set
            {
                if (_eReqAccess == value) return;
                if ((m_gem == null) || m_gem.p_bOffline) return;
                _eReqAccess = value;
                m_gem?.SendCarrierAccessing(this, _eReqAccess);
            }
        }
        eAccess _eAccess = eAccess.NotAccessed; 
        public eAccess p_eAccess
        {
            get { return _eAccess; }
            set
            {
                if (_eAccess == value) return;
                m_log.Info("Loadpert Access = " + value.ToString());
                _eAccess = value;
                _eReqAccess = value; 
                switch (_eAccess)
                {

                    case eAccess.CarrierCompleted: 
                        //m_bReqUnload = true; 
                        break; 
                }
            }
        }

        public enum eAccessLP
        {
            Manual,
            Auto
        }
        eAccessLP _eReqAccessLP = eAccessLP.Manual; 
        public eAccessLP p_eReqAccessLP
        {
            get { return _eReqAccessLP; }
            set
            {
                OnPropertyChanged("p_bAccessLP_Auto");
                OnPropertyChanged("p_bAccessLP_Manual");
                if (_eReqAccessLP == value) return;
                m_log.Info("Loadpert Req Access mode = " + value.ToString());
                _eReqAccessLP = value;
                if (m_gem != null) m_module.p_sInfo = m_gem.SendCarrierAccessLP(this, value);
                RunTree(Tree.eMode.Init);
            }
        }

        eAccessLP _eAccessLP = eAccessLP.Manual;
        public eAccessLP p_eAccessLP
        {
            get { return _eAccessLP; }
            set
            {
                if (_eAccessLP == value) return;
                m_log.Info("Loadpert Access mode = " + value.ToString());
                _eAccessLP = value;
                RunTree(Tree.eMode.Init);
                OnPropertyChanged();
                OnPropertyChanged("p_eAccessLP");
                OnPropertyChanged("p_bAccessLP_Auto");
                OnPropertyChanged("p_bAccessLP_Manual");
            }
        }

        public void RunTreeAccessLP(Tree tree)
        {
            p_eReqAccessLP = (eAccessLP)tree.Set(p_eReqAccessLP, p_eReqAccessLP, "Request", "Request Carrier AccessLP State");
            tree.Set(p_eAccessLP, p_eAccessLP, "State", "Carrier AccessLP State", true, true);
        }
        #endregion
        
        #region CarrierID
        public void SendCarrierID(string sCarrierID)
        {
            if (m_gem == null)
            {
                p_eStateCarrierID = eGemState.VerificationOK;
                p_eTransfer = eTransfer.TransferBlocked;
                return;
            }
            if (m_gem.p_eControl != eControl.ONLINEREMOTE)
            {
                p_eStateCarrierID = eGemState.VerificationOK;
                p_eReqTransfer = eTransfer.TransferBlocked;
                return;
            }

            m_gem.SendCarrierID(this, sCarrierID);

            p_sCarrierID = sCarrierID;
            m_log.Info("Send CarrierID : " + sCarrierID);
        }

        string _sCarrierID = "";
        public string p_sCarrierID
        {
            get { return _sCarrierID; }
            set
            {
                if (_sCarrierID == value) return;
                m_log?.Info("p_sCarrierID " + _sCarrierID + " -> " + value);
                _sCarrierID = value;
                OnPropertyChanged(); 
            }
        }
        #endregion

        #region Slot & PJ
        /// <summary> InfoWafer 와 비슷하지만 Wafer를 빼가도 null로 바꾸지 않는다. </summary>
        public List<GemSlotBase> m_aGemSlot { get; set; } = new List<GemSlotBase>();

        public void SendSlotMap()
        {
            if (m_gem == null || m_gem.p_eControl != eControl.ONLINEREMOTE)
            {
                p_eStateSlotMap = eGemState.VerificationOK;
                return;
            }
            List<GemSlotBase.eState> aMap = new List<GemSlotBase.eState>();
            foreach (GemSlotBase slot in m_aGemSlot) aMap.Add(slot.p_eState);
            p_eStateSlotMap = eGemState.WaitForHost;
            m_gem.SendSlotMap(this, aMap);
            m_log.Info("Send Carrier Slotmap : " + aMap);
        }

        public string SetSlotInfo(int nIndex, GemSlotBase.eState state, string sLotID, string sSlotID) 
        {
            if (nIndex < 0) return "Invalid Index : " + nIndex.ToString();
            if (nIndex >= m_aGemSlot.Count) return "Invalid Index : " + nIndex.ToString();
            m_aGemSlot[nIndex].p_eState = state;
            m_aGemSlot[nIndex].p_sLotID = sLotID;
            m_aGemSlot[nIndex].p_sSlotID = sSlotID;
            return "OK";
        }

        public string PJReqVerifySlot(string sSlotInfo) 
        {
            if (sSlotInfo.Length != m_aGemSlot.Count) return "Slot Count Missmatch"; 
            for (int n = 0; n < sSlotInfo.Length; n++)
            {
                if (sSlotInfo[n] == '3')
                {
                    if (m_aGemSlot[n].p_eState != GemSlotBase.eState.Exist) return "Slot state is not Exist";
                }
            }
            return "OK";
        }

        public void SetPJ(GemPJ pj, List<GemSlotBase.eState> aSlotState)
        {
            int l = Math.Min(aSlotState.Count, m_aGemSlot.Count); 
            for (int n = 0; n < l; n++)
            {
                if (aSlotState[n] == GemSlotBase.eState.Exist) m_aGemSlot[n].AddPJ(pj);  
            }
        }
        #endregion

        #region Property
        public string p_sLocID { get; set; }

        string _sLotID = "";
        public string p_sLotID 
        {
            get { return _sLotID; }
            set
            {
                if (_sLotID == value) return;
                _sLotID = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Tree Gem
        public TreeRoot m_treeRoot;
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public virtual void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeGem(m_treeRoot.GetTree("Gem")); 
        }

        protected void RunTreeGem(Tree tree)
        {
            RunTreeTransfer(tree.GetTree("Transfer"));
            RunTreeAccessLP(tree.GetTree("AccessLP"));
            RunTreeAssociated(tree.GetTree("Associated"));
            RunTreeCarrier(tree.GetTree("Carrier"));
        }
        #endregion

        protected IEngineer m_engineer;
        protected ModuleBase m_module; 
        protected Log m_log;
        public IGem m_gem;
        public void InitBase()
        {
            m_gem?.AddGemCarrier(this);
            m_treeRoot = new TreeRoot(p_sLocID + ".Gem", m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            
            OnPropertyChanged("p_bAccessLP_Auto");
            OnPropertyChanged("p_bAccessLP_Manual");
        }
    }
}

