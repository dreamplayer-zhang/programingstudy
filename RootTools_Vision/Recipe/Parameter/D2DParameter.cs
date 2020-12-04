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
    public class D2DParameter : ParameterBase
    {
        public  D2DParameter() : base(typeof(D2D))
        {

        }

        // 검사 파라매터 적용 대상 셋팅


        #region [Parameters]
        private int intensity = 0;
        private int size = 0;
        private bool isBright = false;
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
        [Category("Option")]
        public bool IsBright
        {
            get => this.isBright;
            set
            {
                SetProperty<bool>(ref this.isBright, value);
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

        public override object Clone()
        {
            // string과 같이 new로 생성되는 변수가 있으면 MemberwiseClone을 사용하면안됩니다.
            // 현재 타입의 클래스를 생성해서 새로 값(객체)을 할당해주어야합니다.
            return this.MemberwiseClone(); ;
        }
    }
}
