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

        public enum AngleDirection
		{
            Clockwise,
            CounterClockwise
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
        [DisplayName("Use Save Circle Image")]
        public bool UseSaveCircleImage
        {
            get => this.useSaveCircleImage;
            set => this.useSaveCircleImage = value;
        }
        private bool useSaveCircleImage = true;

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

        [Category("Klarf")]
        [DisplayName("Circle Thickness")]
        public int Thickness
        {
            get
            {
                return thickness;
            }
            set
            {
                thickness = value;
            }
        }
        private int thickness = 50;

        [Category("Klarf")]
        [DisplayName("Defect Image Size Height")]
        public int DefectImageHeight
        {
            get
            {
                return defectImageHeight;
            }
            set
            {
                defectImageHeight = value;
            }
        }
        private int defectImageHeight = 500;

        [Category("Klarf")]
        [DisplayName("Defect Size Interval (um)")]   // Rough bin에 올리는 Defect Size 간격(um) ex) 0~30um:1 / 30~60um:2 ...
        public int DefectSizeInterval
        {
            get
            {
                return defectSizeInterval;
            }
            set
            {
                defectSizeInterval = value;
            }
        }
        private int defectSizeInterval = 30;

        [Category("Klarf")]
        [DisplayName("Defect Size Interval Count")]   // Rough bin에 올리는 Defect Size 간격(um)의 개수 ex) 0~30um:1 / 30~60um:2 ... 5까지
        public int DefectSizeIntervalCnt
        {
            get
            {
                return defectSizeIntervalCnt;
            }
            set
            {
                defectSizeIntervalCnt = value;
            }
        }
        private int defectSizeIntervalCnt = 5;

        //[Category("Klarf")]
        //[DisplayName("Defect Size Standard Width (um)")]   // Find bin 올리는 Defect Size 기준(um) ex) 100um 보다 크면? 1:0
        //public int DefectSizeStandardWidth
        //{
        //    get
        //    {
        //        return defectSizeStandardWidth;
        //    }
        //    set
        //    {
        //        defectSizeStandardWidth = value;
        //    }
        //}
        //private int defectSizeStandardWidth = 100;

        //[Category("Klarf")]
        //[DisplayName("Defect Size Standard Height (um)")]   // Find bin 올리는 Defect Size 기준(um) ex) 100um 보다 크면? 1:0
        //public int DefectSizeStandardHeight
        //{
        //    get
        //    {
        //        return defectSizeStandardHeight;
        //    }
        //    set
        //    {
        //        defectSizeStandardHeight = value;
        //    }
        //}
        //private int defectSizeStandardHeight = 100;

        [Category("Klarf")]
        [DisplayName("Angle Direction")]  // Notch(6시방향) 기준 각도 표시
        public AngleDirection Angle
        {
            get
            {
                return angle;
            }
            set
            {
                angle = value;
            }
        }
        private AngleDirection angle = AngleDirection.Clockwise;

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
