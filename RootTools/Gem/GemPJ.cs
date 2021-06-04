using RootTools.Trees;
using System.Collections.Generic;

namespace RootTools.Gem
{
    public class GemPJ
    {
        #region Property
        public enum eAutoStart
        {
            N,
            Y
        }
        public eAutoStart m_eAutoStart = eAutoStart.N;

        public enum eState
        {
            Queued,
            SettingUp,
            WaitingForStart,
            Processing,
            ProcessingComplete,
            Reserved,
            Pausing,
            Paused,
            Stopping,
            Aborting,
            Stopped,
            Aborted,
            JobCanceled,
            JobComplete = 17
        }
        eState _eState = eState.Stopped;
        public eState p_eState
        {
            get { return _eState; }
            set
            {
                if (_eState == value) return;
                m_log.Info(m_sPJobID + " PJ State Chaned : " + _eState.ToString() + " -> " + value.ToString());
                _eState = value;
            }
        }

     
        public enum eError
        {
            NO_ERROR,
            Unkown_Object,
            Unkown_TargetObjuctType,
            Unkown_ObjectInstance,
            Unkown_AttributeName,
            ReadOnly_AccessDenied,
            Unkown_ObjectType,
            Invalid_AttibuteValue,
            Syntax_Error,
            Verifacation_Error,
            Validataion_Error,
            ObjectIdentifier_InUse,
            ParametersImproperlySpecified,
            InsufficientParameters,
            Unsupported_Option,
            Busy,
        }

        public enum eCommand
        {
            None,
            Start,
            Pause,
            Resume,
            Stop,
            Abort,
            Cancel,
        }
        eCommand _eCommand = eCommand.None;
        public eCommand p_eCommand
        {
            get { return _eCommand; }
            set
            {
                if (_eCommand == value) return;
                _eCommand = value;
            }
        }
        #endregion

        #region CJ
        public GemCJ m_cj = null;
        #endregion

        #region GemCarrier
        public List<GemCarrierBase> m_aCarrier = new List<GemCarrierBase>();
        public List<List<GemSlotBase.eState>> m_aSlotState = new List<List<GemSlotBase.eState>>();

        public void SetSlotExist(GemCarrierBase carrier, int iSlot)
        {
            int iIndex = GetCarrierIndex(carrier);
            List<GemSlotBase.eState> aSlotState = m_aSlotState[iIndex];
            while (aSlotState.Count <= iSlot) aSlotState.Add(GemSlotBase.eState.Empty);
            aSlotState[iSlot] = GemSlotBase.eState.Exist;
            carrier.m_aGemSlot[iSlot].p_eState = GemSlotBase.eState.Select;
        }

        int GetCarrierIndex(GemCarrierBase carrier)
        {
            for (int n = 0; n < m_aCarrier.Count; n++)
            {
                if (carrier.p_sCarrierID == m_aCarrier[n].p_sCarrierID) return n;
            }
            m_aCarrier.Add(carrier);
            m_aSlotState.Add(new List<GemSlotBase.eState>());
            return m_aCarrier.Count - 1;
        }

        public void SettingUp()
        {
            for (int n = 0; n < m_aCarrier.Count; n++)
            {
                m_aCarrier[n].SetPJ(this, m_aSlotState[n]);
            }
        }
        #endregion

        #region Tree
        public void RunTree(Tree tree)
        {
            p_eState = (eState)tree.Set(p_eState, p_eState, "State", "ProcessJob State", true, true);
            m_sPJobID = tree.Set(m_sPJobID, m_sPJobID, "PJobID", "ProcessJob ID", true, true);
            m_sRecipeID = tree.Set(m_sRecipeID, m_sRecipeID, "Recipe", "ProcessJob Recipe ID", true, true); 
        }
        #endregion

        Log m_log;
        public string m_sPJobID = "";
        public string m_sRecipeID = "";

        public GemPJ(string sPJobID, eAutoStart autoStart, string sRcpID, Log log, string sRcpExt)
        {
            m_log = log;
            m_sPJobID = sPJobID;
            m_eAutoStart = autoStart;
            sRcpID = string.Format("C:\\Recipe\\{0}.{1}", sRcpID, sRcpExt);
            m_sRecipeID = sRcpID;
        }
    }
}
