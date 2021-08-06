using RootTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace RootTools_Vision
{
    public class VerticalWireRecipe : RecipeItemBase
    {

        #region [Parameter]
        private List<byte[]> _rawData = new List<byte[]>();
        
        private List<Rect> _refCoord = new List<Rect>();
        private List<int> _refCoordArrange = new List<int>();

        private List<List<Point>> _inspPoint = new List<List<Point>>();
        private List<int> _inspROISelectedCoord = new List<int>();
        private List<int> _inspROIArrange = new List<int>();
        #endregion

        #region [Getter Setter]
        [XmlIgnore]
        public List<byte[]> RawData { get => _rawData; set => _rawData = value; }

        public List<List<Point>> InspPoint { get => _inspPoint; set => _inspPoint = value; }
        public List<int> InspROISelectedCoord { get => _inspROISelectedCoord; set => _inspROISelectedCoord = value; }
        public List<int> InspROIArrageMethod { get => _inspROIArrange; set => _inspROIArrange = value; }

        public List<Rect> RefCoord { get => _refCoord; set => _refCoord = value; }
        public List<int> RefCoordArrageMethod { get => _refCoordArrange; set => _refCoordArrange = value; }
        #endregion

        public VerticalWireRecipe()
        {

        }

        public override void Clear()
        {
            RawData.Clear();

            InspPoint.Clear();
            InspROISelectedCoord.Clear();
            InspROIArrageMethod.Clear();

            RefCoord.Clear();
            RefCoordArrageMethod.Clear();
        }

        public override bool Save(string recipePath)
        {
            for(int i = 0; i < RawData.Count; i++)
            {
                string path = recipePath + "VerticalWire_Feature_" + i.ToString() + ".bmp";
                Tools.SaveRawdataToBitmap(path, RawData[i], (int)RefCoord[i].Width, (int)RefCoord[i].Height, 3);
            }

            return true;
        }

        public override bool Read(string recipePath)
        {
            int idx = 0;
            foreach(Rect rt in this.RefCoord)
            {
                string rawDataPath = recipePath + "VerticalWire_Feature_" + idx.ToString() + ".bmp";
                if (File.Exists(rawDataPath))
                {
                    System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(rawDataPath);

                    if (bmp.Width != rt.Width || bmp.Height != rt.Height)
                        return false;

                    byte[] rawColorData = new byte[(int)rt.Width * (int)rt.Height * 3];

                    Tools.LoadBitmapToRawdata(rawDataPath, rawColorData, (int)rt.Width, (int)rt.Height, 3);

                    RawData.Add(rawColorData);
                }
                idx++;
            }    

            return true;
        }
    }
}
