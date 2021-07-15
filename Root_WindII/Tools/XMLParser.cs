using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace Root_WindII
{
    public partial class XMLParser
    {
        //static public void SelectedXmlFile(string filePath, XMLData xmlData, StepTableData stepData)    // Find PartID, StepID in the selected xml file
        //{
        //    string fileName;
        //    string[] temp;
        //    char sp = '.';

        //    fileName = Path.GetFileName(filePath);
        //    temp = fileName.Split(sp);
        //    if (temp != null)
        //    {
        //        xmlData.PartID = stepData.PartID = temp[0];
        //        xmlData.StepID = stepData.StepID = temp[1];
        //    }
        //}

        static private XmlDocument xmlDoc = new XmlDocument();
        static public bool ParseMapInfo(string filePath, XMLData xmlData)
        {
            if (filePath == string.Empty) return false;

            try
            {
                xmlDoc.Load(filePath);

                XmlNodeList xmlNodeList = null;

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.RECIPE_NAME.ToString());     // Recipe Name       
                if (xmlNodeList.Count > 0) xmlData.RecipeName = xmlNodeList[0].InnerText;

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.DESCRIPTION.ToString());     // Description
                if (xmlNodeList.Count > 0) xmlData.Description = xmlNodeList[0].InnerText;

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.DEVICE.ToString());          // Device
                if (xmlNodeList.Count > 0) xmlData.Device = xmlNodeList[0].InnerText;

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.DIE_PITCH_X.ToString());     // Die Pitch X
                if (xmlNodeList.Count > 0) xmlData.DiePitchX = Convert.ToDouble(xmlNodeList[0].InnerText);

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.DIE_PITCH_Y.ToString());     // Die Pitch Y
                if (xmlNodeList.Count > 0) xmlData.DiePitchY = Convert.ToDouble(xmlNodeList[0].InnerText);

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.SCRIBE_LINE_X.ToString());   // Scribe Line X
                if (xmlNodeList.Count > 0) xmlData.ScribeLineX = Convert.ToDouble(xmlNodeList[0].InnerText);

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.SCRIBE_LINE_Y.ToString());   // Scribe Line Y
                if (xmlNodeList.Count > 0) xmlData.ScribeLineY = Convert.ToDouble(xmlNodeList[0].InnerText);

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.SHOT_X.ToString());          // Shot X
                if (xmlNodeList.Count > 0) xmlData.ShotX = Convert.ToInt32(xmlNodeList[0].InnerText);

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.SHOT_Y.ToString());          // Shot Y
                if (xmlNodeList.Count > 0) xmlData.ShotY = Convert.ToInt32(xmlNodeList[0].InnerText);

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.MAP_OFFSET_X.ToString());    // Map Offset X
                if (xmlNodeList.Count > 0) xmlData.MapOffsetX = Convert.ToDouble(xmlNodeList[0].InnerText);

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.MAP_OFFSET_Y.ToString());    // Map Offset Y
                if (xmlNodeList.Count > 0) xmlData.MapOffsetY = Convert.ToDouble(xmlNodeList[0].InnerText);

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.SHOT_OFFSET_X.ToString());   // Shot Offset X
                if (xmlNodeList.Count > 0) xmlData.ShotOffsetX = Convert.ToDouble(xmlNodeList[0].InnerText);

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.SHOT_OFFSET_Y.ToString());   // Shot Offset Y
                if (xmlNodeList.Count > 0) xmlData.ShotOffsetY = Convert.ToDouble(xmlNodeList[0].InnerText);

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.SMI_OFFSET_X.ToString());   // SMI Offset X
                if (xmlNodeList.Count > 0) xmlData.SMIOffsetX = Convert.ToDouble(xmlNodeList[0].InnerText);

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.SMI_OFFSET_Y.ToString());   // SMI Offset Y
                if (xmlNodeList.Count > 0) xmlData.SMIOffsetY = Convert.ToDouble(xmlNodeList[0].InnerText);

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.ROTATION.ToString());   // Rotation
                if (xmlNodeList.Count > 0) xmlData.Rotation = Convert.ToInt32(xmlNodeList[0].InnerText);

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.ORIGIN_DIE_X.ToString());   // Origin Die X
                if (xmlNodeList.Count > 0) xmlData.OriginDieX = Convert.ToInt32(xmlNodeList[0].InnerText);

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.ORIGIN_DIE_Y.ToString());   // Origin Die Y
                if (xmlNodeList.Count > 0) xmlData.OriginDieY = Convert.ToInt32(xmlNodeList[0].InnerText);

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.ORIGIN_DIE_Y.ToString());   // Even Odd
                if (xmlNodeList.Count > 0) xmlData.EvenOdd = xmlNodeList[0].InnerText;

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.DIE_LIST.ToString());   // Die List
                if (xmlNodeList.Count > 0)
                {
                    xmlNodeList = xmlDoc.SelectNodes("/XMLCONTENTS/BASIC_TAB/DIE_LIST/DIE_INFO");
                    foreach (XmlNode node in xmlNodeList)
                    {
                        Point pt = new Point();
                        pt.X = Convert.ToInt32(node["DIE_X"].InnerText);
                        pt.Y = Convert.ToInt32(node["DIE_Y"].InnerText);

                        xmlData.DieList.Add(pt);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("XML Parsing Error : 입력된 XML Data가 잘못되었습니다.");
                return false;
            }

            // Backside
            xmlData.MapOffsetX_Backside = xmlData.MapOffsetX == 0 ? xmlData.ScribeLineX : xmlData.DiePitchX - xmlData.MapOffsetX;
            xmlData.ShotOffsetX_Backside = -xmlData.ShotOffsetX; // Scribelane 더해야할 수 있음
            xmlData.SMIOffsetX_Backside = xmlData.ShotX * xmlData.DiePitchX - xmlData.SMIOffsetX;

            return true;
        }

        static public string GetMapInfoPartId(string filePath)
        {
            string sRst = "";
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(filePath);
                //doc.SelectNodes("XMLCONTENTS/BASIC_TAB");
                XmlNodeList xmlNodeList = doc.GetElementsByTagName(eXMLData.DEVICE.ToString()); ;
                if (xmlNodeList.Count > 0) sRst = xmlNodeList[0].InnerText;
            }
            catch (Exception ex)
            {
                MessageBox.Show("XML Parsing Error : 입력된 XML Data가 잘못되었습니다.");
                sRst = "";
            }
            return sRst;
        }


        static private XmlDocument StepTableDoc = new XmlDocument();
        static public string[] GetMasterLayerNames(string filePath)
        {
            List<string> Names = new List<string>();
            if (filePath == string.Empty)
            {
                return null;
            }
            try
            {
                StepTableDoc.Load(filePath);

                XmlNodeList xmlNodeList = null;
                xmlNodeList = StepTableDoc.GetElementsByTagName("LayerCnt");

                if (xmlNodeList.Count > 0)
                {
                    for (int i = 0; i < xmlNodeList.Count; i++)
                    {
                        Names.Add(xmlNodeList[i].ChildNodes[0].InnerText);
                    }
                }
                return Names.ToArray();
            }
            catch (Exception)
            {
                MessageBox.Show("XML Parsing Error : 입력된 XML Data가 잘못되었습니다.");
                return null;
            }
        }
        static public string GetMasterProductName(string filePath)
        {
            if (filePath == string.Empty)
            {
                return null;
            }
            try
            {
                StepTableDoc.Load(filePath);

                XmlNodeList xmlNodeList = null;
                xmlNodeList = StepTableDoc.GetElementsByTagName("Product");
                string sMaster = xmlNodeList[0].ChildNodes[0].InnerText;
                string sSlave = xmlNodeList[0].ChildNodes[1].InnerText;
                return sMaster;
            }
            catch (Exception e)
            {
                MessageBox.Show("Invalid XML File", "RAC SERVER", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            return null;

        }
        //static public bool StepMatchingTableParse(string filePath, StepTableData stepData)  // StepMatchingFilePath : StepMatchingTable file (ex) ~~\Product_B.xml
        //{
        //    if (filePath == string.Empty)
        //    {
        //        return false;
        //    }

        //    try
        //    {
        //        StepTableDoc.Load(filePath);

        //        XmlNodeList xmlNodeList = null;
        //        xmlNodeList = StepTableDoc.GetElementsByTagName("LayerCnt");

        //        if (xmlNodeList.Count > 0)
        //        {
        //            for (int i = 0; i < xmlNodeList.Count; i++)
        //            {
        //                if (xmlNodeList[i].ChildNodes[1].InnerText == stepData.StepID)
        //                {
        //                    stepData.MasterLayer = xmlNodeList[i].ChildNodes[0].InnerText;
        //                    stepData.SlaveLayer = xmlNodeList[i].ChildNodes[1].InnerText;
        //                    AddListInt(xmlNodeList[i].ChildNodes[2].InnerText, stepData.ROI);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        MessageBox.Show("XML Parsing Error : 입력된 XML Data가 잘못되었습니다.");
        //        return false;
        //    }
        //    return true;
        //}

        static private List<int> AddListInt(string str, List<int> list)
        {
            string[] temp;
            char sp = ',';
            list.Clear();
            temp = str.Split(sp);
            for (int i = 0; i < temp.Count(); i++)
            {
                list.Add(Convert.ToInt32(temp[i]));
            }
            return list;
            //return temp = str.Split(sp);
        }

        static public bool StepMatchingTableParse(string filePath, List<StepTableData> stepDataList)  // StepMatchingFilePath : StepMatchingTable file (ex) ~~\Product_B.xml
        {
            string sMaster = string.Empty;
            string sSlave = string.Empty;
            List<int> list = new List<int>();

            if (filePath == string.Empty)
            {
                return false;
            }

            try
            {
                StepTableDoc.Load(filePath);

                XmlNodeList xmlNodeList = null;
                xmlNodeList = StepTableDoc.GetElementsByTagName("Product");
                sMaster = xmlNodeList[0].ChildNodes[0].InnerText;
                sSlave = xmlNodeList[0].ChildNodes[1].InnerText;

                xmlNodeList = StepTableDoc.GetElementsByTagName("LayerCnt");
                if (xmlNodeList.Count > 0)
                {
                    //list.Clear();
                    for (int i = 0; i < xmlNodeList.Count; i++)
                    {
                        stepDataList.Add(new StepTableData(sMaster, sSlave
                                                           , xmlNodeList[i].ChildNodes[0].InnerText
                                                           , xmlNodeList[i].ChildNodes[1].InnerText
                                                           , xmlNodeList[i].ChildNodes[2].InnerText
                                                           , xmlNodeList[i].ChildNodes[3].InnerText));
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("XML Parsing Error : 입력된 XML Data가 잘못되었습니다.");
                return false;
            }
            return true;
        }

        static public bool StepMatchingTableParse_New(string filePath, List<StepTableData> stepDataList)  // StepMatchingFilePath : StepMatchingTable file (ex) ~~\Product_B.xml
        {
            string sMaster = string.Empty;
            string sSlave = string.Empty;
            string sRegionName = string.Empty;
            List<int> list = new List<int>();

            if (filePath == string.Empty)
            {
                return false;
            }

            try
            {
                StepTableDoc.Load(filePath);

                XmlNodeList xmlNodeList = null;
                xmlNodeList = StepTableDoc.GetElementsByTagName("RecipeCreationData");
                sMaster = xmlNodeList[0].FirstChild.Attributes["SourceElement"].Value;
                sSlave = xmlNodeList[0].FirstChild.Attributes["DestinationElement"].Value;
                sRegionName = xmlNodeList[0].FirstChild.Attributes["RegionNames"].Value;

                xmlNodeList = StepTableDoc.GetElementsByTagName("Layers")[0].ChildNodes;

                if (xmlNodeList.Count > 0)
                {
                    for (int i = 0; i < xmlNodeList.Count; i++)
                    {
                        stepDataList.Add(new StepTableData(sMaster, sSlave
                                                           , xmlNodeList[i].Attributes["SourceElement"].Value
                                                           , xmlNodeList[i].Attributes["DestinationElement"].Value
                                                           , xmlNodeList[i].Attributes["RegionNames"].Value
                                                           , xmlNodeList[i].Attributes["Side"].Value));
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("XML Parsing Error : 입력된 XML Data가 잘못되었습니다.");
                return false;
            }
            return true;
        }
    }
}
