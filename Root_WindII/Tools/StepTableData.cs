using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Xml;

namespace Root_WindII
{
    public class StepTableData //: ObservableObject
    {
        #region [Variables]
        public string m_sMasterPartID { get; set; }
        public string m_sSlavePartID { get; set; }
        public string m_sMasterLayer { get; set; }
        public string m_sSlaveLayer { get; set; }
        //public List<int> m_ROI { get; set; }
        public string m_sROI { get; set; }
        public string m_sSide { get; set; }
        #endregion
        
        public StepTableData()
        {
            m_sMasterPartID = string.Empty;
            m_sSlavePartID = string.Empty;
            m_sMasterLayer = string.Empty;
            m_sSlaveLayer = string.Empty;
            m_sROI = string.Empty;
            m_sSide = "FS";
        }

        public StepTableData(string masterPartID, string slavePartID, string master, string slave, string roi, string side)
        {
            m_sMasterPartID = masterPartID;
            m_sSlavePartID = slavePartID;
            m_sMasterLayer = master;
            m_sSlaveLayer = slave;
            m_sROI = roi;
            m_sSide = side;
        }

        static private XmlDocument StepTableDoc = new XmlDocument();
        public static string FindStepMatchingTable(string partID, string StepMatchingFilePath)
        {
            if(StepMatchingFilePath == "")
                return "";

            DirectoryInfo diretory = new DirectoryInfo(StepMatchingFilePath);

            foreach (var fileinfo in diretory.GetFiles())
            {
                //if ((partID + ".XML").ToUpper() == file.Name.ToUpper()) // 기존 파일 이름 비교
                //    return file.FullName;



                // PartID 비교
                StepTableDoc.Load(fileinfo.FullName);

                XmlNodeList xmlNodeList = null;
                xmlNodeList = StepTableDoc.GetElementsByTagName("RecipeCreationData");

                if (xmlNodeList.Count == 0) continue;

                string sSlave = xmlNodeList[0].FirstChild.Attributes["DestinationElement"].Value;
                
                if(sSlave.ToUpper() == partID.ToUpper())
                {
                    return fileinfo.FullName;
                }
            }
            return string.Empty;
        }

    }

}
