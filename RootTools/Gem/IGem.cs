using RootTools.GAFs;
using System.Collections.Generic;
using static RootTools.Gem.XGem.XGem;

namespace RootTools.Gem
{
    public delegate void dgGemRemoteCommand(string sCmd, Dictionary<string, string> dicParam, long[] pnResult);
    public delegate void dgGemSECSMessageReceived(long nObjectID, long nStream, long nFunction, long nSysbyte);

    public interface IGem
    {
        event dgGemRemoteCommand OnGemRemoteCommand;
        event dgGemSECSMessageReceived OnGemSECSMessageReceived;

        bool p_bOffline { get; }

        long SetSV(SVID sv, dynamic value);

        long SetCEID(CEID ecv);

        long SetCEID(long nCEID);

        long SetAlarm(ALID alid, bool bSet);

        void AddGemCarrier(GemCarrierBase carrier);

        void SendLPInfo(GemCarrierBase carrier);

        string SendCarrierPresentSensor(GemCarrierBase carrier, bool bPresent);

        void SendCarrierOn(GemCarrierBase carrier, bool bOn);

        void SendCarrierID(GemCarrierBase carrier, string sCarrierID);

        void SendSlotMap(GemCarrierBase carrier, List<GemSlotBase.eState> aMap);

        string CMSSetReadyToLoad(GemCarrierBase carrier); 

        string CMSSetReadyToUnload(GemCarrierBase carrier); 

        void CMSDelCarrierInfo(GemCarrierBase carrier);

        void SendCarrierAccessing(GemCarrierBase carrier, GemCarrierBase.eAccess access);

        string SendCarrierAccessLP(GemCarrierBase carrier, GemCarrierBase.eAccessLP accessLP);

        void RemoveCarrierInfo(string sLocID); 

        GemCJ p_cjRun { get; set; }
        
        void SendPJComplete(string sPJobID); 

        bool p_bUseSTS { get; set; }

        void STSSetTransport(string sLocID, GemSlotBase gemSlot, GemSlotBase.eSTS state);

        void STSSetProcessing(GemSlotBase gemSlot, GemSlotBase.eSTSProcess process);

        void DeleteAllJobInfo();

        eControl p_eControl { get; set; }

        eControl p_eReqControl { get; set; }

        void MakeObject(ref long nObject);

        void SetListItem(long nObject, int listCnt);

        void SetStringItem(long nObject, string strItem);

        void SetInt4Item(long nObject, int nitem);

        void SetFloat4Item(long nObject, float fItem);

        void GEMSetVariables(long nObject, long nVid);
    }
}
