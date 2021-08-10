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
    public enum CreateDiffMethod
    {
        Absolute = 0,
        Bright,
        Dark,
    }

    [TypeConverter(typeof(GridConverterEnum))]
    public enum UpdateFreq
    {
        Chip = 0,
        Line,
        [Description("Pre-Created Image")]
        PreCreate,
        [Description("None [MC Mode Only]")]
        None, // D2D 4.0 Only
    }

    public class D2DParameter : ParameterBase, IMaskInspection, IColorInspection, IDisplaySpecSummary, IFrontsideInspection
    {
        public  D2DParameter() : base(typeof(D2D))
        {

        }

        // 검사 파라매터 적용 대상 셋팅


        #region [Parameters]

        // Inspection Parameter
        private int sensitivity = 0;
        private int lsl = 0;
        private int usl = 0;
        private int defectMaxNum = 100;

        //Create Diff Image Option
        private CreateDiffMethod createDiffMethod = CreateDiffMethod.Absolute;
        private bool useScaleMap = false;
        private bool useHistWeightMap = false;
        private DiffFilterMethod diffFilter = DiffFilterMethod.Median;

        // Create Golden Image Option
        private CreateRefImageMethod createRefImage = CreateRefImageMethod.MedianAverage;
        private UpdateFreq refImageUpdateFreq = UpdateFreq.Chip;

        // MC Method
        private bool useMC = false;
        private int windowSize_ME = 1;
        #endregion

        #region [Getter Setter]

        [Category("Parameter")]
        [DisplayName("Sensitivity")]
        public int Sensitivity
        {
            get => this.sensitivity;
            set
            {
                SetProperty<int>(ref this.sensitivity, value);
                Value = this.sensitivity;
            }
        }

        [Category("Parameter")]
        [DisplayName("LSL(Lower Specification Limit)")]
        public int LSL
        {
            get => this.lsl;
            set
            {
                SetProperty<int>(ref this.lsl, value);
            }
        }

        [Category("Parameter")]
        [DisplayName("USL(Upper Specification Limit)")]
        public int USL
        {
            get => this.usl;
            set
            {
                SetProperty<int>(ref this.usl, value);
            }
        }

        [Category("Parameter")]
        [DisplayName("Maximum Defect Num (Chip)")]
        public int DefectMaxNum
        {
            get => this.defectMaxNum;
            set
            {
                SetProperty<int>(ref this.defectMaxNum, value);
            }
        }
        [Category("Diff Image Option")]
        [DisplayName("Create Method")]
        public CreateDiffMethod CreateDiffMethod
        {
            get => this.createDiffMethod;
            set
            {
                SetProperty<CreateDiffMethod>(ref this.createDiffMethod, value);
            }
        }
        [Category("Diff Image Option")]
        [DisplayName("Use Scale Map")]
        public bool UseScaleMap
        {
            get => this.useScaleMap;
            set
            {
                SetProperty<bool>(ref this.useScaleMap, value);
            }
        }
        [Category("Diff Image Option")]
        [DisplayName("Use Weight Map")]
        public bool UseHistWeightMap
        {
            get => this.useHistWeightMap;
            set
            {
                SetProperty<bool>(ref this.useHistWeightMap, value);
            }
        }
        [Category("Diff Image Option")]
        [DisplayName("Filter")]
        public DiffFilterMethod DiffFilter
        {
            get => this.diffFilter;
            set
            {
                SetProperty<DiffFilterMethod>(ref this.diffFilter, value);
            }
        }
        [Category("Golden Image Option")]
        [DisplayName("Create Method")]
        public CreateRefImageMethod CreateRefImage
        {
            get => this.createRefImage;
            set
            {
                SetProperty<CreateRefImageMethod>(ref this.createRefImage, value);
            }
        }
        [Category("Golden Image Option")]
        [DisplayName("Update Frequency")]
        public UpdateFreq RefImageUpdateFreq
        {
            get => this.refImageUpdateFreq;
            set
            {
                SetProperty<UpdateFreq>(ref this.refImageUpdateFreq, value);
            }
        }
        [Category("Motion Compensation Option")]
        [DisplayName("Use Motion Compensation")]
        public bool UseMC
        {
            get => useMC;
            set
            {
                SetProperty<bool>(ref this.useMC, value);
            }
        }

        [Category("Motion Compensation Option")]
        [DisplayName("Window Size")]
        public int WindowSize_ME
        {
            get => this.windowSize_ME;
            set
            {
                SetProperty<int>(ref this.windowSize_ME, value);
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
            get { return this.sensitivity; }
            set
            {
                RaisePropertyChanged("Value");
            }
        }

        [Browsable(false)]
        public int Size
        {
            get { return this.lsl; }
            set
            {
                RaisePropertyChanged("Size");
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

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EnumDisplayNameAttribute : Attribute
    {
        public EnumDisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }

        public string DisplayName { get; set; }
    }
    public class EnumMapper
    {
        public EnumMapper(object enumValue, string enumDescription)
        {
            Enum = enumValue;
            Description = enumDescription;
        }

        public object Enum { get; private set; }
        public string Description { get; private set; }
    }
}
