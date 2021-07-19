using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace Root_WindII
{
    public class XMLCreator
    {
        public static bool CreateXMLFile(string filePath, XMLData xmldata)
        {
            try 
            {
                XmlDocument doc = new XmlDocument();
                XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "euc-kr", "yes");
                XmlComment comment = doc.CreateComment(@"uLoader Client XML Data");
                doc.AppendChild(declaration);
                doc.AppendChild(comment);

                XmlElement contents = doc.CreateElement("XMLCONTENTS");

                #region [BASIC_INFORMATION]
                XmlElement basic_information = doc.CreateElement("BASIC_INFORMATION");
                XmlElement recipe_name = doc.CreateElement("RECIPE_NAME");
                recipe_name.InnerText = xmldata.RecipeName;

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

                XmlElement die_list = XMLCreator.MaskDieList(doc, xmldata);

                basic_tab.AppendChild(device);
                basic_tab.AppendChild(die_pitch_x);
                basic_tab.AppendChild(die_pitch_y);
                basic_tab.AppendChild(scribe_line_x);
                basic_tab.AppendChild(scribe_line_y);
                basic_tab.AppendChild(shot_x);
                basic_tab.AppendChild(shot_y);
                basic_tab.AppendChild(map_offset_x);
                basic_tab.AppendChild(map_offset_y);
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
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return true;
        }

        private static XmlElement MaskDieList(XmlDocument doc, XMLData xmldata)
        {
            XmlElement die_list = doc.CreateElement("DIE_LIST");

            int mapSizeX = xmldata.MapSizeX;
            int mapSizeY = xmldata.MapSizeY;
            int[] map = xmldata.MapData;

            for (int i = 0; i < mapSizeY; i++)
            {
                for(int j = 0; j<mapSizeX; j++)
                {
                    if(map[i * mapSizeX + j] == 1)
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
    }
}
