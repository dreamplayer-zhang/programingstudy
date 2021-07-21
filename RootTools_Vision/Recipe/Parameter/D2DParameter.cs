using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using RootTools;
using RootTools_Vision;

namespace RootTools_Vision
{

    public enum DiffFilterMethod
    {
        Average = 0,
        Gaussian,
        Median,
        Morphology,
        None,
    }
    public enum CreateRefImageMethod
    {
        Average = 0,
        MedianAverage,
        Median,
    }
    public enum RefImageUpdateFreq
    {
        Chip = 0,
        Chip_Trigger,
        Line,
        PreCreate,
    }
    public class D2DParameter : ParameterBase, IMaskInspection, IColorInspection, IDisplaySpecSummary, IFrontsideInspection
    {
        public  D2DParameter() : base(typeof(D2D))
        {

        }

        // 검사 파라매터 적용 대상 셋팅


        #region [Parameters]
        private int intensity = 0;
        private int size = 0;
        private int sizeLimit = 0;
        private bool isBright = false;
        private bool scaleMap = false;
        private bool histWeightMap = false;
        private DiffFilterMethod diffFilter = DiffFilterMethod.Average;
        private CreateRefImageMethod createRefImage = CreateRefImageMethod.Average;
        private RefImageUpdateFreq refImageUpdateFreq = RefImageUpdateFreq.Chip;
        #endregion

        #region [Getter Setter]
        [Category("Parameter")]
        public int Intensity
        {
            get => this.intensity;
            set
            {
                SetProperty<int>(ref this.intensity, value);
                Value = this.intensity;
            }
        }
        [Category("Parameter")]
        public int Size
        {
            get => this.size;
            set
            {
                SetProperty<int>(ref this.size, value);
            }
        }
        [Category("Parameter")]
        [DisplayName("Size Limit")]
        public int SizeLimit
        {
            get => this.sizeLimit;
            set
            {
                SetProperty<int>(ref this.sizeLimit, value);
            }
        }
        [Category("Option")]
        public bool IsBright
        {
            get => this.isBright;
            set
            {
                SetProperty<bool>(ref this.isBright, value);
            }
        }
        [Category("Option")]
        public bool ScaleMap
        {
            get => this.scaleMap;
            set
            {
                SetProperty<bool>(ref this.scaleMap, value);
            }
        }
        [Category("Option")]
        [DisplayName("Use Weight Map")]
        public bool HistWeightMap
        {
            get => this.histWeightMap;
            set
            {
                SetProperty<bool>(ref this.histWeightMap, value);
            }
        }
        [Category("Option")]
        [DisplayName("Diff Filter")]
        public DiffFilterMethod DiffFilter
        {
            get => this.diffFilter;
            set
            {
                SetProperty<DiffFilterMethod>(ref this.diffFilter, value);
            }
        }
        [Category("Option")]
        public CreateRefImageMethod CreateRefImage
        {
            get => this.createRefImage;
            set
            {
                SetProperty<CreateRefImageMethod>(ref this.createRefImage, value);
            }
        }
        [Category("Option")]
        public RefImageUpdateFreq RefImageUpdate
        {
            get => this.refImageUpdateFreq;
            set
            {
                SetProperty<RefImageUpdateFreq>(ref this.refImageUpdateFreq, value);
            }
        }


        // ROI
        [Browsable(false)]
        public int MaskIndex 
        {
            get;
            set;
        }

        [Browsable(false)]
        public IMAGE_CHANNEL IndexChannel
        { 
            get; 
            set; 
        }

        [Browsable(false)]
        public int Value 
        {
            get { return this.intensity; }
            set
            {
                RaisePropertyChanged("Value");
            }
        }
        #endregion

        public bool Save()
        {
            throw new NotImplementedException();
        }

        public bool Read()
        {
            throw new NotImplementedException();
        }

        //public override object Clone()
        //{
        //    // string과 같이 new로 생성되는 변수가 있으면 MemberwiseClone을 사용하면안됩니다.
        //    // 현재 타입의 클래스를 생성해서 새로 값(객체)을 할당해주어야합니다.
        //    return this.MemberwiseClone();
        //}
    }
}
