using RootTools.Gem;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;

namespace RootTools
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

        #region IWTRChild
        public string IsRunOK()
        {
            switch (p_eState)
            {
                case eState.Dock: return "OK";
            }
            return p_id + " eState = " + p_eState.ToString();
        }

        public int GetTeachWTR(InfoWafer infoWafer = null)
        {
            return m_waferSize.GetData(p_eWaferSize).m_teachWTR;
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
        #endregion

        #region GemSlot
        const int c_maxSlot = 25;
        /// <summary> m_GemSlot.p_id List </summary>
        void InitSlot()
        {
            for (int n = 0; n < c_maxSlot; n++)
            {
                InfoWafer newSlot = new InfoWafer(p_sModule, n, m_engineer);
                newSlot.p_eState = GemSlotBase.eState.Empty;
                newSlot.p_sCarrierID = p_sCarrierID;
                newSlot.p_sLocID = p_sLocID;
                m_aGemSlot.Add(newSlot);
                m_aInfoWafer.Add(null);
            }
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
            }
        }

        public List<string> p_asGemSlot
        {
            get
            {
                List<string> asGemSlot = new List<string>();
                for (int n = 0; n < p_lWafer; n++) asGemSlot.Add(m_aGemSlot[n].p_id);
                return asGemSlot; 
            }
        }

        InfoWafer.eWaferSize _eWaferSize = InfoWafer.eWaferSize.e300mm;
        public InfoWafer.eWaferSize p_eWaferSize
        {
            get { return _eWaferSize; }
            set
            {
                if (_eWaferSize == value) return;
                InfoWafer.WaferSize.Data data = m_waferSize.GetData(value);
                if (data.m_bEnable == false) return;
                m_log.Info(p_id + " eWaferSize : " + _eWaferSize.ToString() + " -> " + value.ToString());
                _eWaferSize = value;
                p_lWafer = data.m_lWafer;
            }
        }

        void RunTreeSlot(Tree tree)
        {
            p_eWaferSize = (InfoWafer.eWaferSize)tree.Set(p_eWaferSize, p_eWaferSize, "Size", "Wafer Size");
            if (m_waferSize.m_bUseCount == false) p_lWafer = 1; 
            else p_lWafer = tree.Set(p_lWafer, p_lWafer, "Count", "Slot Count", true, true);
        }
        #endregion

        #region InfoWafer
        public List<InfoWafer> m_aInfoWafer = new List<InfoWafer>();
        InfoWafer GetInfoWafer(string sWafer)
        {
            foreach (InfoWafer infoWafer in m_aInfoWafer)
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
            for (int n = 0; n < m_aInfoWafer.Count; n++) SetInfoWafer(n, null);
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
                if (sInfoWafer != "") m_aInfoWafer[n] = (InfoWafer)m_aGemSlot[n];
                m_aGemSlot[n].p_eState = (sInfoWafer != "") ? GemSlotBase.eState.Exist : GemSlotBase.eState.Empty;
            }
            RunTreeWafer(Tree.eMode.Init);
        }

        void SaveInfoWafer(int nID)
        {
            if (m_reg == null) return;
            m_reg.Write("p_iWafer", p_lWafer);
            string sInfoWafer = (m_aInfoWafer[nID] == null) ? "" : m_aInfoWafer[nID].p_id;
            m_reg.Write("sInfoWafer." + nID.ToString("00"), sInfoWafer);
        }

        public InfoWafer GetInfoWafer(int nID)
        {
            if (nID < 0) return null;
            if (nID >= p_lWafer) return null;
            //if (m_aInfoWafer[nID] == null) m_aInfoWafer[nID] = (InfoWafer)m_aGemSlot[nID]; //lyj del
            return m_aInfoWafer[nID];
        }

        public void SetInfoWafer(int nID, InfoWafer infoWafer)
        {
            if (nID < 0) return;
            if (nID >= p_lWafer) return;
            if (EQ.p_nRnR < 1)
            {
                if (infoWafer == null && m_aInfoWafer[nID] != null) m_aInfoWafer[nID].ClearInfo(); //? 왜 클리어?
            }
            m_aInfoWafer[nID] = infoWafer;
            SaveInfoWafer(nID);
        }

        public void SetInfoWafer(int nID)
        {
            if (nID < 0) return;
            if (nID >= p_lWafer) return;
            m_aInfoWafer[nID] = new InfoWafer((InfoWafer)m_aGemSlot[nID]);
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
                if (m_aInfoWafer[n] != null) m_engineer.ClassHandler().AddSequence(m_aInfoWafer[n]);
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
            if ((p_ePresentSensor == ePresent.Empty) && (m_gem != null) && m_gem.p_bOffline == false)
                m_gem.RemoveCarrierInfo(p_sLocID);
        }
        #endregion

        #region Mapping Function
        public string SetMapData(List<GemSlotBase.eState> aSlotState)
        {
            if (p_lWafer > aSlotState.Count) return "SetMapData Lendth Error";
            for (int n = 0; n < p_lWafer; n++)
            {
                m_aGemSlot[n].p_eState = aSlotState[n];
                if (aSlotState[n] == GemSlotBase.eState.Exist) SetInfoWafer(n);
                else SetInfoWafer(n, null);
            }
            return "OK";
        }

        public string GetMapData()
        {
            string map = "";
            foreach(GemSlotBase slot in m_aGemSlot)
            {
                switch (slot.p_eState)
                {
                    case GemSlotBase.eState.Empty:
                        map += "0";
                        break;
                    case GemSlotBase.eState.Exist:
                        map += "1";
                        break;
                    case GemSlotBase.eState.Double:
                        map += "D";
                        break;
                    case GemSlotBase.eState.Cross:
                        map += "C";
                        break;
                    default:
                        map += "-";
                        break;
                }
            }
            //if (p_lWafer > aSlotState.Count) return "SetMapData Lendth Error";
            //for (int n = 0; n < p_lWafer; n++)
            //{
            //    m_aGemSlot[n].p_eState = aSlotState[n];
            //    if (aSlotState[n] == GemSlotBase.eState.Exist) SetInfoWafer(n);
            //    else SetInfoWafer(n, null);
            //}
            return map;
        }

        public string SetSelectMapData(InfoCarrier infoCarrier)
        {
            //if (p_lWafer > aSlotState.Count) return "SetMapData Lendth Error";
            for (int n = 0; n < p_lWafer; n++)
            {
                //if (aSlotState[n] == GemSlotBase.eState.Select) SetInfoWafer(n);
                if (GetInfoWafer(n) != null)
                {
                    if (GetInfoWafer(n).p_eState != GemSlotBase.eState.Select) SetInfoWafer(n, null);
                }
            }
            return "OK";
        }
        #endregion

        #region UI
        List<InfoCarrier_UI> m_aCarrierUI = new List<InfoCarrier_UI>(); 
        public UserControl p_ui
        {
            get
            {
                InfoCarrier_UI ui = new InfoCarrier_UI();
                ui.Init(this);
                m_aCarrierUI.Add(ui); 
                return ui;
            }
        }
        #endregion

        #region Tree
        public InfoWafer.WaferSize m_waferSize;
        public TreeRoot m_treeRootWafer;
        private void M_treeRootWafer_UpdateTree()
        {
            RunTreeWafer(Tree.eMode.Update);
            RunTreeWafer(Tree.eMode.RegWrite);
            RunTreeWafer(Tree.eMode.Init);
        }

        public void RunTreeWafer(Tree.eMode mode)
        {
            if ((mode == Tree.eMode.Init) && m_treeRootWafer.m_bFocus) return; 
            m_treeRootWafer.p_eMode = mode;
            for (int n = 0; n < m_aInfoWafer.Count; n++)
            {
                InfoWafer infoWafer = m_aInfoWafer[n];
                if (infoWafer != null) infoWafer.RunTree(m_treeRootWafer.GetTree(m_aGemSlot[n].p_id, false));
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

        public InfoCarrier(ModuleBase module, string sLocID, IEngineer engineer, bool bEnableWaferSize, bool bEnableWaferCount)
        {
            m_module = module;
            p_sModule = module.p_id;
            p_id = p_sModule + ".InfoCarrier";
            //p_sCarrierID = p_sModule; //LYJ Carrier ID Loadport 들어가는거 삭제
            p_sLocID = sLocID;
            m_engineer = engineer;
            m_gem = m_engineer.ClassGem();
            m_log = LogView.GetLog(module.p_id, module.p_id);
            m_waferSize = new InfoWafer.WaferSize(p_sModule, bEnableWaferSize, bEnableWaferCount);
            m_treeRootWafer = new TreeRoot(p_id, m_log);
            m_treeRootWafer.UpdateTree += M_treeRootWafer_UpdateTree;
            InitSlot();
            InitBase();
        }

        public void ThreadStop()
        {
            foreach (InfoCarrier_UI ui in m_aCarrierUI) ui.m_timer.Stop(); 
        }
    }
}
