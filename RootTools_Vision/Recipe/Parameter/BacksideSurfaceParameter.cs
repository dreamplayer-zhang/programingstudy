using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class BacksideSurfaceParameter : ParameterBase, IMaskInspection, IColorInspection, IDisplaySpecSummary, IBackInspection
    {
        public BacksideSurfaceParameter() : base(typeof(BacksideSurface))
        {

        }
        #region [Parameter]
        private int intensity = 30;
        private int size = 10;
        private bool isBright = false;
        private bool isAdaptiveIntensity = false;
        private int adaptiveOffset = 10;
        private DiffFilterMethod diffFilter = DiffFilterMethod.Average;
        #endregion

        #region [Getter Setter]
        [Category("Parameter")]
        public int Intensity
        {
            get => this.intensity;
            set
            {
                SetProperty<int>(ref this.intensity, value);
                RaisePropertyChanged("Value");
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
        //[DisplayName("Pattern Intensity <-> Param Intensity Offset")]
        [DisplayName("Adaptive Intensity")]
        public int AdaptiveOffset
        {
            get => this.adaptiveOffset;
            set
            {
                SetProperty<int>(ref this.adaptiveOffset, value);
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
        [DisplayName("Adaptive Intensity Mode")]
        public bool IsAdaptiveIntensity
        {
            get => this.isAdaptiveIntensity;
            set
            {
                SetProperty<bool>(ref this.isAdaptiveIntensity, value);
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
            get
            {
                return this.intensity;
            }
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
        //    return this.MemberwiseClone(); ;
        //}
    }
}
