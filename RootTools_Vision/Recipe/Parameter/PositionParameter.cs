using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class PositionParameter : ParameterBase, IMaskInspection, IColorInspection
    {
        public PositionParameter() : base(typeof(Position))
        {

        }

        #region [Parameter]
        private int searchRangeX = 100;
        private int searchRangeY = 100;
        private int minScoreLimit = 60;
        #endregion


        #region [Getter Setter]
        public int SearchRangeX
        {
            get => searchRangeX;
            set
            {
                SetProperty<int>(ref searchRangeX, value);
            }
        }
        public int SearchRangeY
        {
            get => searchRangeY;
            set
            {
                SetProperty<int>(ref searchRangeY, value);
            }
        }
        public int MinScoreLimit
        {
            get => minScoreLimit;
            set
            {
                SetProperty<int>(ref minScoreLimit, value);
            }
        }

        [Browsable(false)]
        public INSPECTION_IMAGE_CHANNEL IndexChannel
        {
            get;
            set;
        }

        [Browsable(false)]
        public int MaskIndex 
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

        public override object Clone()
        {
            // string과 같이 new로 생성되는 변수가 있으면 MemberwiseClone을 사용하면안됩니다.
            // 현재 타입의 클래스를 생성해서 새로 값(객체)을 할당해주어야합니다.
            return this.MemberwiseClone(); ;
        }
    }
}
