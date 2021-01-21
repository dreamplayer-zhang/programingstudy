﻿using RootTools_Vision;
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
        private int chipSearchRangeX = 100;
        private int chipSearchRangeY = 100;
        private int chipMinScoreLimit = 60;
        private int waferSearchRangeX = 1000;
        private int waferSearchRangeY = 1000;
        private int waferMinScoreLimit = 60;
        #endregion


        #region [Getter Setter]

        [DisplayName("Chip Search Range X")]
        public int ChipSearchRangeX
        {
            get => chipSearchRangeX;
            set
            {
                SetProperty<int>(ref chipSearchRangeX, value);
            }
        }
        [DisplayName("Chip Search Range Y")]
        public int ChipSearchRangeY
        {
            get => chipSearchRangeY;
            set
            {
                SetProperty<int>(ref chipSearchRangeY, value);
            }
        }

        [DisplayName("Chip Score Limit")]
        public int ChipMinScoreLimit
        {
            get => chipMinScoreLimit;
            set
            {
                SetProperty<int>(ref chipMinScoreLimit, value);
            }
        }

        [DisplayName("Wafer Search Range X")]
        public int WaferSearchRangeX
        {
            get => waferSearchRangeX;
            set
            {
                SetProperty<int>(ref waferSearchRangeX, value);
            }
        }

        [DisplayName("Wafer Search Range Y")]
        public int WaferSearchRangeY
        {
            get => waferSearchRangeY;
            set
            {
                SetProperty<int>(ref waferSearchRangeY, value);
            }
        }

        [DisplayName("Wafer Score Limit")]
        public int WaferMinScoreLimit
        {
            get => waferMinScoreLimit;
            set
            {
                SetProperty<int>(ref waferMinScoreLimit, value);
            }
        }

        [Browsable(false)]
        public IMAGE_CHANNEL IndexChannel
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

        //public override object Clone()
        //{
        //    // string과 같이 new로 생성되는 변수가 있으면 MemberwiseClone을 사용하면안됩니다.
        //    // 현재 타입의 클래스를 생성해서 새로 값(객체)을 할당해주어야합니다.
        //    return this.MemberwiseClone(); ;
        //}
    }
}
