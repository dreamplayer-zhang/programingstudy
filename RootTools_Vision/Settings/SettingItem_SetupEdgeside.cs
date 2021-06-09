using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
	public class SettingItem_SetupEdgeside : SettingItem
	{
        public SettingItem_SetupEdgeside(string[] _treeViewPath) : base(_treeViewPath)
        {

        }

        [Category("Klarf")]
        [DisplayName("Use Klarf")]
        public bool UseKlarf
        {
            get => this.useKlarf;
            set => this.useKlarf = value;
        }
        private bool useKlarf = true;

        [Category("Klarf")]
        [DisplayName("Klarf Save Path")]
        public string KlarfSavePath
        {
            get
            {
                return klarfSavePath;
            }
            set
            {
                klarfSavePath = value;
            }
        }
        private string klarfSavePath = "D:\\Klarf";

        [Category("Klarf")]
        [DisplayName("Whole Circle Image Size Width")]
        public int OutputImageSizeWidth
        {
            get
            {
                return outputImageSizeWidth;
            }
            set
            {
                outputImageSizeWidth = value;
            }
        }
        private int outputImageSizeWidth = 1000;


        [Category("Klarf")]
        [DisplayName("Whole Circle Image Size Height")]
        public int OutputImageSizeHeight
        {
            get
            {
                return outputImageSizeHeight;
            }
            set
            {
                outputImageSizeHeight = value;
            }
        }
        private int outputImageSizeHeight = 1000;

        [Category("Common")]
        [DisplayName("Defect Image Path")]
        public string DefectImagePath
        {
            get
            {
                return defectImagePath;
            }
            set
            {
                defectImagePath = value;
            }
        }
        private string defectImagePath = "D:\\DefectImage";
    }
}
