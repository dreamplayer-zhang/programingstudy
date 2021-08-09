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
        public delegate void ParsingHandler();
        public static event ParsingHandler Parsing;
        static void ParsingDone()
        {
            if (Parsing != null)
                Parsing();
        }
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

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.EVEN_ODD.ToString());   // Even Odd
                if (xmlNodeList.Count > 0) xmlData.EvenOdd = xmlNodeList[0].InnerText;

                xmlNodeList = xmlDoc.GetElementsByTagName(eXMLData.DIE_LIST.ToString());   // Die List
                if (xmlNodeList.Count > 0)
                {
                    xmlNodeList = xmlDoc.SelectNodes("/XMLCONTENTS/BASIC_TAB/DIE_LIST/DIE_INFO");
                    xmlData.DieList.Clear();
                    foreach (XmlNode node in xmlNodeList)
                    {
                        Point pt = new Point();
                        pt.X = Convert.ToInt32(node["DIE_X"].InnerText);
                        pt.Y = Convert.ToInt32(node["DIE_Y"].InnerText);

                        xmlData.DieList.Add(pt);
                    }
                }

                int minX = 0, maxX = 0;
                int minY = 0, maxY = 0;

                foreach (Point pt in xmlData.DieList)
                {
                    int valX = (int)(pt.X - xmlData.OriginDieX);
                    int valY = (int)(pt.Y - xmlData.OriginDieY);

                    if (minX > valX) minX = valX;
                    if (maxX < valX) maxX = valX;

                    if (minY > valY) minY = valY;
                    if (maxY < valY) maxY = valY;
                }

                xmlData.UnitX = maxX - minX + 1;
                xmlData.UnitY = maxY - minY + 1;

                ParsingDone();
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

        static XmlElement MakeDieList(XmlDocument doc, XMLData xmldata)
        {
            XmlElement die_list = doc.CreateElement("DIE_LIST");

            int mapSizeX = xmldata.MapSizeX;
            int mapSizeY = xmldata.MapSizeY;
            int[] map = xmldata.MapData;

            for (int i = 0; i < mapSizeY; i++)
            {
                for (int j = 0; j < mapSizeX; j++)
                {
                    if (map[i * mapSizeX + j] == 1)
                    {
                        XmlElement dieInfo = doc.CreateElement("DIE_INFO");
                        XmlElement die_x = doc.CreateElement("DIE_X");
                        die_x.InnerText = j.ToString();

                        XmlElement die_y = doc.CreateElement("DIE_Y");
                        die_y.InnerText = i.ToString();

                        dieInfo.AppendChild(die_x);
                        dieInfo.AppendChild(die_y);

                        die_list.AppendChild(dieInfo);
                    }
                }
            }
            return die_list;
        }

        static public void CreateXMLData(string filePath, XMLData xmldata)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                //XMLData xmldata = RootTools_Vision.GlobalObjects.Instance.Get<XMLData>();
                XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "euc-kr", "yes");
                XmlComment comment = doc.CreateComment(@"uLoader Client XML Data");
                doc.AppendChild(declaration);
                doc.AppendChild(comment);

                XmlElement contents = doc.CreateElement("XMLCONTENTS");

                #region [BASIC_INFORMATION]
                XmlElement basic_information = doc.CreateElement("BASIC_INFORMATION");
                XmlElement recipe_name = doc.CreateElement("RECIPE_NAME");
                //recipe_name.InnerText = xmldata.RecipeName;
                recipe_name.InnerText = Path.GetFileNameWithoutExtension(filePath);
                XmlElement description = doc.CreateElement("DESCRIPTION");
                description.InnerText = xmldata.Description;

                basic_information.AppendChild(recipe_name);
                basic_information.AppendChild(description);
                #endregion

                #region [BASIC_TAB]
                XmlElement basic_tab = doc.CreateElement("BASIC_TAB");
                XmlElement device = doc.CreateElement("DEVICE");
                device.InnerText = xmldata.Device;

                XmlElement die_pitch_x = doc.CreateElement("DIE_PITCH_X");
                die_pitch_x.InnerText = xmldata.DiePitchX.ToString();

                XmlElement die_pitch_y = doc.CreateElement("DIE_PITCH_Y");
                die_pitch_y.InnerText = xmldata.DiePitchY.ToString();

                XmlElement scribe_line_x = doc.CreateElement("SCRIBE_LINE_X");
                scribe_line_x.InnerText = xmldata.ScribeLineX.ToString();

                XmlElement scribe_line_y = doc.CreateElement("SCRIBE_LINE_Y");
                scribe_line_y.InnerText = xmldata.ScribeLineY.ToString();

                XmlElement shot_x = doc.CreateElement("SHOT_X");
                shot_x.InnerText = xmldata.ShotX.ToString();

                XmlElement shot_y = doc.CreateElement("SHOT_Y");
                shot_y.InnerText = xmldata.ShotY.ToString();

                XmlElement map_offset_x = doc.CreateElement("MAP_OFFSET_X");
                map_offset_x.InnerText = xmldata.MapOffsetX.ToString();

                XmlElement map_offset_y = doc.CreateElement("MAP_OFFSET_Y");
                map_offset_y.InnerText = xmldata.MapOffsetY.ToString();

                XmlElement shot_offset_x = doc.CreateElement("SHOT_OFFSET_X");
                shot_offset_x.InnerText = xmldata.ShotOffsetX.ToString();

                XmlElement shot_offset_y = doc.CreateElement("SHOT_OFFSET_Y");
                shot_offset_y.InnerText = xmldata.ShotOffsetY.ToString();

                XmlElement smi_offset_x = doc.CreateElement("SMI_OFFSET_X");
                smi_offset_x.InnerText = xmldata.SMIOffsetX.ToString();

                XmlElement smi_offset_y = doc.CreateElement("SMI_OFFSET_Y");
                smi_offset_y.InnerText = xmldata.SMIOffsetY.ToString();

                XmlElement rotation = doc.CreateElement("ROTATION");
                rotation.InnerText = xmldata.Rotation.ToString();

                XmlElement origin_die_x = doc.CreateElement("ORIGIN_DIE_X");
                origin_die_x.InnerText = xmldata.OriginDieX.ToString();

                XmlElement origin_die_y = doc.CreateElement("ORIGIN_DIE_Y");
                origin_die_y.InnerText = xmldata.OriginDieY.ToString();

                XmlElement even_odd = doc.CreateElement("EVEN_ODD");
                even_odd.InnerText = xmldata.EvenOdd.ToString();

                XmlElement die_list = MakeDieList(doc, xmldata);

                basic_tab.AppendChild(device);
                basic_tab.AppendChild(die_pitch_x);
                basic_tab.AppendChild(die_pitch_y);
                basic_tab.AppendChild(scribe_line_x);
                basic_tab.AppendChild(scribe_line_y);
                basic_tab.AppendChild(shot_x);
                basic_tab.AppendChild(shot_y);
                basic_tab.AppendChild(map_offset_x);
                basic_tab.AppendChild(map_offset_y);
                basic_tab.AppendChild(shot_offset_x);
                basic_tab.AppendChild(shot_offset_y);
                basic_tab.AppendChild(smi_offset_x);
                basic_tab.AppendChild(smi_offset_y);
                basic_tab.AppendChild(rotation);
                basic_tab.AppendChild(origin_die_x);
                basic_tab.AppendChild(origin_die_y);
                basic_tab.AppendChild(even_odd);
                basic_tab.AppendChild(die_list);
                #endregion

                contents.AppendChild(basic_information);
                contents.AppendChild(basic_tab);

                doc.AppendChild(contents);

                doc.Save(filePath);
            }
            catch (Exception)
            {

            }
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
