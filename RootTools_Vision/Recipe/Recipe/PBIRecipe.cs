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
    public class PBIRecipe : RecipeItemBase
    {
        #region [Parameter]
        private List<byte[]> _rawData = new List<byte[]>();
        private List<Rect> _featureInfo = new List<Rect>();
        #endregion

        #region [Getter Setter]
        [XmlIgnore]
        public List<byte[]> RawData { get => _rawData; set => _rawData = value; }
        public List<Rect> FeatureInfo { get => _featureInfo; set => _featureInfo = value; }
        #endregion

        public PBIRecipe()
        {

        }

        public override void Clear()
        {
            RawData.Clear();
            FeatureInfo.Clear();
        }

        public override bool Save(string recipePath)
        {
            for (int i = 0; i < RawData.Count; i++)
            {
                string path = recipePath + "PBI_Feature_" + i.ToString() + ".bmp";
                Tools.SaveRawdataToBitmap(path, RawData[i], (int)FeatureInfo[i].Width, (int)FeatureInfo[i].Height, 3);
            }

            return true;
        }

        public override bool Read(string recipePath)
        {
            int idx = 0;
            foreach (Rect rt in this.FeatureInfo)
            {
                string rawDataPath = recipePath + "PBI_Feature_" + idx.ToString() + ".bmp";
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
