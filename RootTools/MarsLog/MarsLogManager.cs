using SSLNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools
{
    public class MarsLogManager
    {
        private static MarsLogManager instance;
        public static MarsLogManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MarsLogManager();
                }
                return instance;
            }   
            private set
            {
                instance = value;
            }
        }

        public bool m_useLog = false;
        SSLoggerNet m_sSLoggerNet { get; set; } = null;

        MarsLogManager()
        {
            try
            {
                m_sSLoggerNet = new SSLoggerNet();
            }
            catch (Exception e)
            {
                string ess = e.ToString();
            }
        }
        public void ChangeMaterial(int port, int slot, string lot, string foup, string recipe)
        {
            m_sSLoggerNet.ChangeMaterial(port, slot, lot, foup, recipe);
        }

        public void ChangeMaterialSlot(int port, int slot)
        {
            m_sSLoggerNet.ChangeMaterialSlot(port, slot);
        }

        public void WritePRC(int port, string device, PRC_EVENTID eventID, STATUS status, string stepName, int stepNum, DataFormatter dataFormatter = null, string materialID = null)
        {
            if (!m_useLog)
                return;

            bool isExist = false;
            if (materialID != null)
                isExist = true;

            bool isDataExist = CheckDataExist(dataFormatter);

            if (isDataExist && isExist)
                m_sSLoggerNet.WritePRCLog(port, device, eventID, status, stepName, stepNum, dataFormatter);
            else if (isDataExist && !isExist)
                m_sSLoggerNet.WritePRCLog(port, device, eventID, status, stepName, stepNum, dataFormatter, materialID);
            else if (!isDataExist && isExist)
                m_sSLoggerNet.WritePRCLog(port, device, eventID, status, stepName, stepNum, materialID);
            else
                m_sSLoggerNet.WritePRCLog(port, device, eventID, status, stepName, stepNum);
        }

        public void WriteFNC(int port, string device, string eventID, STATUS status, DataFormatter dataFormatter = null, MATERIAL_TYPE? type = MATERIAL_TYPE.WAFER)
        {
            if (!m_useLog)
                return;

            bool isExist = false;
            if (type != null)
                isExist = true;

            bool isDataExist = CheckDataExist(dataFormatter);

            if (isDataExist && isExist)
                m_sSLoggerNet.WriteFNCLog(port, device, eventID, status, dataFormatter, (MATERIAL_TYPE)type);
            else if (isDataExist && !isExist)
                m_sSLoggerNet.WriteFNCLog(port, device, eventID, status, dataFormatter);
            else if (!isDataExist && isExist)
                m_sSLoggerNet.WriteFNCLog(port, device, eventID, status, (MATERIAL_TYPE)type);
            else
                m_sSLoggerNet.WriteFNCLog(port, device, eventID, status);
        }

        public void WriteXFR(string device, XFR_EVENTID eventID, STATUS stauts, FlowData fromDevice, FlowData fromSlot, FlowData toDevice, FlowData toSlot, DataFormatter dataFormatter = null, MaterialFormatter materialFormatter = null)
        {
            if (!m_useLog)
                return;

            bool isDataExist = CheckDataExist(dataFormatter);

            bool isMaterialDataExist = CheckMaterialDataExist(materialFormatter);

            if (isDataExist && isMaterialDataExist)
                m_sSLoggerNet.WriteXFRLog(device, eventID, stauts, fromDevice, fromSlot, toDevice, toSlot, dataFormatter, materialFormatter);
            else if (isDataExist && !isMaterialDataExist)
                m_sSLoggerNet.WriteXFRLog(device, eventID, stauts, fromDevice, fromSlot, toDevice, toSlot, dataFormatter);
            else if (!isDataExist && isMaterialDataExist)
                m_sSLoggerNet.WriteXFRLog(device, eventID, stauts, fromDevice, fromSlot, toDevice, toSlot, materialFormatter);
            else
                m_sSLoggerNet.WriteXFRLog(device, eventID, stauts, fromDevice, fromSlot, toDevice, toSlot);
        }

        public void WriteLEH(int port, string device, LEH_EVENTID eventID, FlowData flowInfo, DataFormatter dataFormatter = null)
        {
            if (!m_useLog)
                return;

            bool isDataExist = CheckDataExist(dataFormatter);

            if (isDataExist)
                m_sSLoggerNet.WriteLEHLog(port, device, eventID, flowInfo, dataFormatter);
            else
                m_sSLoggerNet.WriteLEHLog(port, device, eventID, flowInfo);
        }

        bool CheckDataExist(DataFormatter data)
        {
            return data == null ? false : true;
        }

        bool CheckMaterialDataExist(MaterialFormatter materialData)
        {
            return materialData == null ? false : true;
        }

        public void WriteCFG(string device, string eventID, DataFormatter dataFormatter = null)
        {
            if (!m_useLog)
                return;

            bool isDataExist = CheckDataExist(dataFormatter);

            if (isDataExist)
                m_sSLoggerNet.WriteCFGLog(device, eventID, dataFormatter);
            else
                m_sSLoggerNet.WriteCFGLog(device, eventID);
        }

        //public DataFormatter GetDataFormatter()
        //{
        //    return new DataFormatter();
        //}

        //public MaterialFormatter GetMaterialFormatter()
        //{
        //    return new MaterialFormatter();
        //}

        //public FlowData GetFlowData()
        //{
        //    return new FlowData();
        //}

        //public void AddData(string key, dynamic value, string unit = null)
        //{
        //    if(value.GetType() == typeof(bool))
        //    {
        //        value = value.ToString();
        //    }
        //    if (unit != null)
        //        m_dataFormatter.AddData(key, value, unit);
        //    else
        //        m_dataFormatter.AddData(key, value);

        //    m_dataCnt++;
        //}

        ////public void AddData(string key, int value, string unit = null)
        ////{
        ////    if (unit != null)
        ////        m_dataFormatter.AddData(key, value, unit);
        ////    else
        ////        m_dataFormatter.AddData(key, value);

        ////    m_dataCnt++;
        ////}

        ////public void AddData(string key, double value, string unit = null)
        ////{
        ////    if (unit != null)
        ////        m_dataFormatter.AddData(key, value, unit);
        ////    else
        ////        m_dataFormatter.AddData(key, value);

        ////    m_dataCnt++;
        ////}

        ////public void AddData(string key, string value, string unit = null)
        ////{
        ////    if (unit != null)
        ////        m_dataFormatter.AddData(key, value, unit);
        ////    else
        ////        m_dataFormatter.AddData(key, value);

        ////    m_dataCnt++;
        ////}
        //public void AddFlowData(FlowDataInfo flowData, dynamic value)
        //{
        //    switch (flowData)
        //    {
        //        case FlowDataInfo.FromDevice:
        //            m_fromDevice.AddData(value);
        //            break;
        //        case FlowDataInfo.FromSlot:
        //            m_fromSlot.AddData(value);
        //            break;
        //        case FlowDataInfo.ToDevice:
        //            m_toDevice.AddData(value);
        //            break;
        //        case FlowDataInfo.ToSlot:
        //            m_toSlot.AddData(value);
        //            break;
        //        case FlowDataInfo.FlowInfo:
        //            m_flowInfo.AddData(value);
        //            break;
        //    }
        //}

        //public void ClearFlowData(FlowDataInfo flowData = FlowDataInfo.All)
        //{
        //    switch (flowData)
        //    {
        //        case FlowDataInfo.FromDevice:
        //            m_fromDevice.ClearData();
        //            break;
        //        case FlowDataInfo.FromSlot:
        //            m_fromSlot.ClearData();
        //            break;
        //        case FlowDataInfo.ToDevice:
        //            m_toDevice.ClearData();
        //            break;
        //        case FlowDataInfo.ToSlot:
        //            m_toSlot.ClearData();
        //            break;
        //        case FlowDataInfo.FlowInfo:
        //            m_flowInfo.ClearData();
        //            break;
        //        case FlowDataInfo.All:
        //            ClearAllFlowData();
        //            break;
        //    }
        //}

        //void ClearAllFlowData()
        //{
        //    m_fromDevice.ClearData();
        //    m_fromSlot.ClearData();
        //    m_toDevice.ClearData();
        //    m_toSlot.ClearData();
        //    m_flowInfo.ClearData();
        //}

        //public void AddMaterialData(string lot, int slot)
        //{
        //    m_materialFormatter.AddMaterial(lot, slot);

        //    m_materialDataCnt++;
        //}

        //public void AddTwoMaterialData(string firstLot, int firstSlot, string secondLot, int secondSlot)
        //{
        //    m_materialFormatter.AddTwoMaterial(firstLot, firstSlot, secondLot, secondSlot);

        //    m_materialDataCnt++;
        //}

    }
}
