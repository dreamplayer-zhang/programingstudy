using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    /// <summary>
    /// Frontside - Spec 창에서 Inspection Item에 표시되는 데이터를 구현합니다.
    /// 항목은 Value와 Size입니다.
    /// </summary>
    public interface IDisplaySpecSummary
    {
        int Value { get; set; }
        int Size { get; set; }
    }
}
