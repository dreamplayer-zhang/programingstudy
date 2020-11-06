using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA
{
    class CamelliaState
    {
        public enum CamelliaMode
        {
            None,
            Hole,
            Die
        }
        public enum CamelliaSettingMode
        {
            None,
            Select,
            One,
            Line
        }

        public enum CamelliaSubMode
        {
            None,
            Hole,
            Die
        }
        public enum CamelliaReorderMode
        {
            None,
            Reorder
        }
        public enum CamelliaPointType
        {
            None,
            Blue,
            Yellow,
            Red
        }
        public enum CamelliaCenterEdge
        {
            None,
            Center,
            Edge,
            Blue // 예전 레시피 호환성 위해 임시로 추가
        }

        public enum CamelliaValueType
        {
            None,
            Variable,
            Constant
        }
    }
}
