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


        int m_dataCnt = 0;
        int m_materialDataCnt = 0;

        public bool m_useLog = false;
        SSLoggerNet m_sSLoggerNet { get; set; } = null;
        DataFormatter m_dataFormatter { get; set; } = null;
        MaterialFormatter m_materialFormatter { get; set; } = null;
        MarsLogManager()
        {
            m_sSLoggerNet = new SSLoggerNet();
            m_dataFormatter = new DataFormatter();
            m_materialFormatter = new MaterialFormatter();
            //m_sSLoggerNet.WritePRCLog(,);
            //m_dataFormatter.AddData(,);
        }

        public void WritePRC(int port, string device, PRC_EVENTID eventID, STATUS status, string stepName, int stepNum, string materialID = null)
        {
            if (!m_useLog)
                return;

            bool isExist = false;
            if (materialID != null)
                isExist = true;

            if (m_dataCnt > 0 && isExist)
                m_sSLoggerNet.WritePRCLog(port, device, eventID, status, stepName, stepNum, m_dataFormatter);
            else if (m_dataCnt > 0 && !isExist)
                m_sSLoggerNet.WritePRCLog(port, device, eventID, status, stepName, stepNum, m_dataFormatter, materialID);
            else if (m_dataCnt == 0 && isExist)
                m_sSLoggerNet.WritePRCLog(port, device, eventID, status, stepName, stepNum, materialID);
            else
                m_sSLoggerNet.WritePRCLog(port, device, eventID, status, stepName, stepNum);
        }

        public void WriteFNC(int port, string device, string eventID, STATUS status, MATERIAL_TYPE? type = null)
        {
            if (!m_useLog)
                return;

            bool isExist = false;
            if (type != null)
                isExist = true;
            if (m_dataCnt > 0 && isExist)
                m_sSLoggerNet.WriteFNCLog(port, device, eventID, status, m_dataFormatter, (MATERIAL_TYPE)type);
            else if (m_dataCnt > 0 && !isExist)
                m_sSLoggerNet.WriteFNCLog(port, device, eventID, status, m_dataFormatter);
            else if (m_dataCnt == 0 && isExist)
                m_sSLoggerNet.WriteFNCLog(port, device, eventID, status, (MATERIAL_TYPE)type);
            else
                m_sSLoggerNet.WriteFNCLog(port, device, eventID, status);
        }

        public void WriteXFR(string device, XFR_EVENTID eventID, STATUS stauts)
        {
            if (!m_useLog)
                return;
            //m_sSLoggerNet.WriteXFRLog(,);
        }

        public void WriteLEH()
        {
            if (!m_useLog)
                return;
        }

        public void WriteCFG()
        {
            if (!m_useLog)
                return;
        }

        public void ClearData()
        {
            if (!m_useLog)
                return;
            m_dataFormatter.ClearData();
            m_dataCnt = 0;
        }

        public void ClearMaterialData()
        {
            if (!m_useLog)
                return;
            m_materialFormatter.ClearData();
            m_materialDataCnt = 0;
        }

        //object GetTypeValue<T>(T value)
        //{
        //    var type = value.GetType();

        //    if (type == typeof(int))
        //    {
        //        return 
        //    }
        //    else if (type == typeof(double) || type == typeof(long))
        //    {

        //    }
        //    else if (type == typeof(string))
        //    { 

        //    }
        //}

        public void AddData(string key, dynamic value, string unit = null)
        {
            if(value.GetType() == typeof(bool))
            {
                value = value.ToString();
            }
            if (unit != null)
                m_dataFormatter.AddData(key, value, unit);
            else
                m_dataFormatter.AddData(key, value);

            m_dataCnt++;
        }

        //public void AddData(string key, int value, string unit = null)
        //{
        //    if (unit != null)
        //        m_dataFormatter.AddData(key, value, unit);
        //    else
        //        m_dataFormatter.AddData(key, value);

        //    m_dataCnt++;
        //}

        //public void AddData(string key, double value, string unit = null)
        //{
        //    if (unit != null)
        //        m_dataFormatter.AddData(key, value, unit);
        //    else
        //        m_dataFormatter.AddData(key, value);

        //    m_dataCnt++;
        //}

        //public void AddData(string key, string value, string unit = null)
        //{
        //    if (unit != null)
        //        m_dataFormatter.AddData(key, value, unit);
        //    else
        //        m_dataFormatter.AddData(key, value);

        //    m_dataCnt++;
        //}

        public void AddMaterialData(string lot, int slot)
        {
            m_materialFormatter.AddMaterial(lot, slot);

            m_materialDataCnt++;
        }

        public void AddTwoMaterialData(string firstLot, int firstSlot, string secondLot, int secondSlot)
        {
            m_materialFormatter.AddTwoMaterial(firstLot, firstSlot, secondLot, secondSlot);

            m_materialDataCnt++;
        }

    }
}
