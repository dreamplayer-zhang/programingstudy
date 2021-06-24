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

        public bool UseTDIReview
        {
            get => this.useTDIReview;
            set => this.useTDIReview = value;
        }
        private bool useTDIReview = true;

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

        [Category("Klarf")]
        [DisplayName("Whole Wafer Image Start X")]
        public int WholeWaferImageStartX
        {
            get
            {
                return wholeWaferImageStartX;
            }
            set
            {
                wholeWaferImageStartX = value;
            }
        }
        private int wholeWaferImageStartX = 0;

        [Category("Klarf")]
        [DisplayName("Whole Wafer Image Start Y")]
        public int WholeWaferImageStartY
        {
            get
            {
                return wholeWaferImageStartY;
            }
            set
            {
                wholeWaferImageStartY = value;
            }
        }
        private int wholeWaferImageStartY = 0;

        [Category("Klarf")]
        [DisplayName("Whole Wafer Image End X")]
        public int WholeWaferImageEndX
        {
            get
            {
                return wholeWaferImageEndX;
            }
            set
            {
                wholeWaferImageEndX = value;
            }
        }
        private int wholeWaferImageEndX = 1000;

        [Category("Klarf")]
        [DisplayName("Whole Wafer Image End Y")]
        public int WholeWaferImageEndY
        {
            get
            {
                return wholeWaferImageEndY;
            }
            set
            {
                wholeWaferImageEndY = value;
            }
        }
        private int wholeWaferImageEndY = 1000;



        [Category("Klarf")]
        [DisplayName("Output Image Size Y")]
        public int OutputImageSizeY
        {
            get
            {
                return outputImageSizeY;
            }
            set
            {
                outputImageSizeY = value;
            }
        }
        private int outputImageSizeY = 3000;


        [Category("Klarf")]
        [DisplayName("Output Image Size X")]
        public int OutputImageSizeX
        {
            get
            {
                return outputImageSizeX;
            }
            set
            {
                outputImageSizeX = value;
            }
        }
        private int outputImageSizeX = 3000;


        [Category("Klarf")]
        [DisplayName("Save Image Wafer Size (mm)")]
        public int SaveWaferSize
        {
            get
            {
                return saveWaferSize;
            }
            set
            {
                saveWaferSize = value;
            }
        }
        private int saveWaferSize = 280;

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
