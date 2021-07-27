using RootTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace RootTools_Vision
{
    public struct VerticalWire_InspROI_Info
    {
        public List<TRect> _wirePoint;
        public int _refCoordNum;

        public VerticalWire_InspROI_Info(List<TRect> wirePoint, int refCoordNum)
        {
            this._wirePoint = wirePoint;
            this._refCoordNum = refCoordNum;
        }
    }

    public struct VerticalWire_RefCoord_Info
    {
        public int x;
        public int y;
        public int w;
        public int h;

        public VerticalWire_RefCoord_Info(int x, int y, int w, int h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
        }
    }

    public class VerticalWireRecipe : RecipeItemBase
    {

        //#region [Parameter]
        //private List<byte[]> _rawData = new List<byte[]>();
        //private List<VerticalWire_InspROI_Info> _inspROI = new List<VerticalWire_InspROI_Info>();
        //private List<VerticalWire_RefCoord_Info> _refCoord = new List<VerticalWire_RefCoord_Info>();

        //#endregion

        //#region [Getter Setter]
        //[XmlIgnore]
        //public List<byte[]> RawData { get => _rawData; set => _rawData = value; }

        //public List<VerticalWire_InspROI_Info> InspROI { get => _inspROI; set => _inspROI = value; }

        //public List<VerticalWire_RefCoord_Info> RefCoord { get => _refCoord; set => _refCoord = value; }

        //#endregion

        public VerticalWireRecipe()
        {

        }

        public override void Clear()
        {
            //RawData.Clear();
            //InspROI.Clear();
            //RefCoord.Clear();

        }

        public override bool Save(string recipePath)
        {
            return true;
        }

        public override bool Read(string recipePath)
        {
            //for(int i = 0; i < RefCoord.Count; i++)
            //{
            //    string rawDataPath = recipePath + "VerticalWire_RefFeature_" + i.ToString() +".bmp";
            //    if (File.Exists(rawDataPath))
            //    {
            //        Bitmap bmp = new Bitmap(rawDataPath);

            //        if(bmp.Width != this.RefCoord[i].w || bmp.Height != this.RefCoord[i].h)
            //            return false;

            //        byte[] rawColorData = new byte[this.RefCoord[i].w * this.RefCoord[i].h * 3];

            //        Tools.LoadBitmapToRawdata(rawDataPath, rawColorData, this.RefCoord[i].w, this.RefCoord[i].h, 3);

            //        RawData.Add(rawColorData);
            //    }
            //}
            

            return true;
        }
    }
}
