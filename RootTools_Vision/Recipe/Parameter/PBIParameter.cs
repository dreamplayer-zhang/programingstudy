using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class PBIParameter : ParameterBase
    {
        public PBIParameter() : base(typeof(PBI))
        {

        }

        // 측정 파라매터 적용 대상 셋팅
        #region [Parameters]
        #endregion

        #region [Getter Setter]
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