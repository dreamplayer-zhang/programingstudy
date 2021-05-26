using System.ComponentModel;

namespace RootTools_Vision
{
    public class SettingItem_SetupFrontside : SettingItem
    {
        public SettingItem_SetupFrontside(string[] _treeViewPath): base(_treeViewPath)
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
        [DisplayName("TDI Review")]
        public bool UseTDIReview
        {
            get
            {
                return useTDIReview;
            }
            set
            {
                useTDIReview = value;
            }
        }


        private bool useTDIReview = false;

        [Category("Klarf")]
        [DisplayName("VRS Review")]
        public bool UseVrsReview
        {
            get
            {
                return useVrsReview;
            }
            set
            {
                useVrsReview = value;
            }
        }

        private bool useVrsReview = false;



        [Category("Klarf")]
        [DisplayName("Defect Sampling Number")]
        public int DefectSamplingNumber
        {
            get
            {
                return defectSamplingNumber;
            }
            set
            {
                defectSamplingNumber = value;
            }
        }
        private int defectSamplingNumber = 0;

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

        [Category("Common")]
        [DisplayName("Save Whole Wafer Image")]
        public string SaveWholeWaferImage
        {
            get
            {
                return saveWholeWaferImage;
            }
            set
            {
                saveWholeWaferImage = value;
            }
        }
        private string saveWholeWaferImage = "D:\\DefectImage";
    }
}
