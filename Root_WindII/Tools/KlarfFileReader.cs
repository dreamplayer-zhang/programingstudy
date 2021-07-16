using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WindII
{
    public class KlarfFileReader
    {
        public static void OpenKlarfMapData(StreamReader stdFile, ref XMLData xmldata)
        {
            // [ 2021-06-15 ] : Imported by jhan from VisionWorks2

            string strNumber = "";
            string buffX;
            string buffY;
            string tmp;

            int[] SampleX = new int[10000];
            int[] SampleY = new int[10000];
            int Min_x = 99, Max_x = 0, Min_y = 99, Max_y = 0;
            int num = 0;

            tmp = stdFile.ReadLine();
            tmp.Trim();

            // Device
            while (tmp.IndexOf("DeviceID ") == -1)
            {
                tmp = stdFile.ReadLine();
            }
            tmp.Trim();
            tmp = tmp.Substring(tmp.IndexOf(' ') + 1, tmp.Length - tmp.IndexOf(' ') - 1);
            xmldata.Device = tmp.Substring(0, tmp.IndexOf(';'));

            // Die Pitch
            while (tmp.IndexOf("DiePitch ") == -1)
            {
                tmp = stdFile.ReadLine();
            }
            tmp.Trim();
            tmp = tmp.Substring(tmp.IndexOf(' ') + 1, tmp.Length - tmp.IndexOf(' ') - 1);
            xmldata.DiePitchX = Double.Parse(tmp.Substring(0, tmp.Length - tmp.IndexOf(' ') - 1));
            xmldata.DiePitchY = Double.Parse(tmp.Substring(tmp.IndexOf(' ') + 1, tmp.IndexOf(';') - tmp.IndexOf(' ') - 1));

            // Origin Die
            while (tmp.IndexOf("DieOrigin ") == -1)
            {
                tmp = stdFile.ReadLine();
            }
            tmp.Trim();
            tmp = tmp.Substring(tmp.IndexOf(' ') + 1, tmp.Length - tmp.IndexOf(' ') - 1);
            xmldata.OriginDieX = (Int32)Double.Parse(tmp.Substring(0, tmp.Length - tmp.IndexOf(' ') - 1));
            xmldata.OriginDieY = (Int32)Double.Parse(tmp.Substring(tmp.IndexOf(' ') + 1, tmp.IndexOf(';') - tmp.IndexOf(' ') - 1));

            while (tmp.IndexOf("SampleTestPlan ") == -1)
            {
                tmp = stdFile.ReadLine();
            }
            for (int i = 13; i < tmp.Length; i++)
            {
                char ch = tmp[i];
                if (ch >= '0' && ch <= '9')
                {
                    strNumber += ch;
                }
            }

            int n = Int32.Parse(strNumber);
            while (tmp.IndexOf(";") == -1)
            {
                tmp = stdFile.ReadLine();
                tmp.Trim();

                buffX = tmp.Substring(0, tmp.IndexOf(' '));
                buffY = tmp.Substring(tmp.IndexOf(' ') + 1, tmp.Length - tmp.IndexOf(' ') - 1);

                SampleX[num] = Int32.Parse(buffX);
                if (buffY.IndexOf(";") != -1)
                {
                    buffY = buffY.Substring(0, buffY.Length - 1);
                }
                SampleY[num] = Int32.Parse(buffY);
                num = num + 1;
                buffY = "";
                buffX = "";
            }

            for (int i = 0; i < n; i++)
            {
                if (SampleX[i] > Max_x)
                {
                    Max_x = SampleX[i];
                }
                if (SampleX[i] < Min_x)
                {
                    Min_x = SampleX[i];
                }
                if (SampleY[i] > Max_y)
                {
                    Max_y = SampleY[i];
                }
                if (SampleY[i] < Min_y)
                {
                    Min_y = SampleY[i];
                }
            }

            xmldata.MapSizeX = (Max_x - Min_x + 1);
            xmldata.MapSizeY = (Max_y - Min_y + 1);
            xmldata.MapData = new int[xmldata.MapSizeX * xmldata.MapSizeY];

            //mapSizeX = (Max_x - Min_x + 1);
            //mapSizeY = (Max_y - Min_y + 1);

            //mapdata = new int[mapSizeX * mapSizeY];
            int[] p = xmldata.MapData;

            int cnt = 0;
            int t = 0;
            for (int i = 0; i < n; i++)
            {
                t = (SampleX[cnt] - Min_x) + (xmldata.MapSizeX * (Max_y - SampleY[cnt]));
                p[t] = 1;
                cnt++;
            }

            int mapSizeX = xmldata.MapSizeX;
            int mapSizeY = xmldata.MapSizeY;
            int[] map = xmldata.MapData;
            List<System.Windows.Point> dieList = xmldata.DieList;
            dieList.Clear();

            for (int i = 0; i < mapSizeY; i++)
            {
                for(int j = 0; j<mapSizeX; j++)
                {
                    if(map[i * mapSizeX + j] == 1)
                    {
                        dieList.Add(new System.Windows.Point(j, i));
                    }
                }
            }
        }
    }
}
