using SSLNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
        public FlowData m_flowDataA;
        public FlowData m_flowDataB;
        public DataFormatter m_dataFormatterA;
        public DataFormatter m_dataFormatterB;
        MarsLogManager()
        {
            try
            {
                m_sSLoggerNet = new SSLoggerNet();
                m_flowDataA = new FlowData();
                m_flowDataB = new FlowData();
                m_dataFormatterA = new DataFormatter();
                m_dataFormatterB = new DataFormatter();
            }
            catch (Exception e)
            {
                //string ess = e.ToString();
            }
        }
        public void ChangeMaterial(int port, int slot, string lot, string foup, string recipe)
        {
            if (!m_useLog)
                return;
            m_sSLoggerNet.ChangeMaterial(port, slot, lot, foup, recipe);
        }

        public void ChangeMaterialSlot(int port, int slot)
        {
            m_sSLoggerNet.ChangeMaterialSlot(port, slot);
        }

        public void WritePRC(int port, string device, PRC_EVENTID eventID, STATUS status, MATERIAL_TYPE materialType, string stepName = "", int stepNum = -1, int stageNum = -1, DataFormatter dataFormatter = null)
        {
            if (!m_useLog)
                return;

            bool isDataExist = CheckDataExist(dataFormatter);

            bool isExist = false;
            if (stageNum != -1)
                isExist = true;

            if (isDataExist && isExist)
                m_sSLoggerNet.WritePRCLog(port, device, eventID, status, materialType, stepName, stepNum, stageNum, dataFormatter);
            else if (isDataExist && !isExist)
                m_sSLoggerNet.WritePRCLog(port, device, eventID, status, materialType, stepName, stepNum, dataFormatter);
            else if (!isDataExist && isExist)
                m_sSLoggerNet.WritePRCLog(port, device, eventID, status, materialType, stepName, stepNum, stageNum);
            else if (!isDataExist && !isExist && stepName != "" && stepNum != -1)
                m_sSLoggerNet.WritePRCLog(port, device, eventID, status, materialType, stepName, stepNum);
            else
                m_sSLoggerNet.WritePRCLog(port, device, eventID, status, materialType);
        }

        public void WriteFNC(int port, string device, string eventID, STATUS status, MATERIAL_TYPE type = MATERIAL_TYPE.WAFER, DataFormatter dataFormatter = null)
        {
            if (!m_useLog)
                return;

            bool isDataExist = CheckDataExist(dataFormatter);

            if (isDataExist)
                m_sSLoggerNet.WriteFNCLog(port, device, eventID, status, type, dataFormatter);
            else
                m_sSLoggerNet.WriteFNCLog(port, device, eventID, status, type);
        }

        public void WriteXFR(int port, string device, XFR_EVENTID eventID, STATUS stauts, FlowData fromDevice, FlowData fromSlot, FlowData toDevice, FlowData toSlot, MaterialFormatter materialFormatter, DataFormatter dataFormatter = null)
        {
            if (!m_useLog)
                return;

            bool isDataExist = CheckDataExist(dataFormatter);

            if (isDataExist)
                m_sSLoggerNet.WriteXFRLog(port, device, eventID, stauts, fromDevice, fromSlot, toDevice, toSlot, materialFormatter, dataFormatter);
            else
                m_sSLoggerNet.WriteXFRLog(port, device, eventID, stauts, fromDevice, fromSlot, toDevice, toSlot, materialFormatter);
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

        public void WriteCFG(string device, string categoryID, string cfgID, dynamic value, DataFormatter dataFormatter = null, string unit = "", string ecid = "")
        {
            if (!m_useLog)
                return;

            bool isDataExist = CheckDataExist(dataFormatter);

            if(isDataExist && (unit == "" || ecid == ""))
            {
                MessageBox.Show("dataFormatter가 존재 할 시 unit, ecid는 공백이면 안됩니다.");
                return;
            }
            try
            {
                if (isDataExist)
                    m_sSLoggerNet.WriteCFGLog(device, categoryID, cfgID, value, dataFormatter, unit, ecid);
                else if (!isDataExist && unit != "" && ecid != "")
                    m_sSLoggerNet.WriteCFGLog(device, categoryID, cfgID, value, unit, ecid);
                else
                    m_sSLoggerNet.WriteCFGLog(device, categoryID, cfgID, value);
            }
            catch (Exception e)
            {

            }

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
