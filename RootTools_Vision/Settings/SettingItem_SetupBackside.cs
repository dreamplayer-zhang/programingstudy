using System.ComponentModel;

namespace RootTools_Vision
{
    public class SettingItem_SetupBackside : SettingItem
    {
        public SettingItem_SetupBackside(string[] _treeViewPath) : base(_treeViewPath)
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
        [DisplayName("Use Klarf Whole Wafer Image")]
        public bool UseKlarfWholeWaferImage
        {
            get
            {
                return useKlarfWholeWaferImage;
            }
            set
            {
                useKlarfWholeWaferImage = value;
            }
        }
        private bool useKlarfWholeWaferImage = false;

        [Category("Klarf")]
        [DisplayName("Whole Wafer Image Compression Rate")]
        public double WholeWaferImageCompressionRate
        {
            get
            {
                return wholeWaferImageCompressionRate;
            }
            set
            {
                wholeWaferImageCompressionRate = value;
            }
        }
        private double wholeWaferImageCompressionRate = 1;

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
