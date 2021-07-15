using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class SurfaceParameter : ParameterBase, IMaskInspection, IColorInspection, IFrontsideInspection
    {
        public SurfaceParameter() : base(typeof(Surface))
        {

        }


        #region [Parameter]
        private int intensity = 0;
        private int size = 0;
        private int sizeLimit = 0;
        private bool isBright = false;
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
