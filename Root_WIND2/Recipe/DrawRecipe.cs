using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace Root_WIND2
{
    class DrawRecipe
    {
        /// <summary>
        /// 그려지는 Recipe 항목에 대한 Recipe의 상위 클래스.
        /// 오브젝트 항목들에 대한 컨트롤 기능
        /// BMP 생성 함수 구현
        /// </summary>
        /// 
        protected List<BasicShape> m_ObjectList; // Object List

        public DrawRecipe()
        {
        }
        public void SetData(List<BasicShape> _basicShapes)
        {
            m_ObjectList = _basicShapes;
        }

        // 각도형 클래스에 매핑.

        #region Object Control
        // ADD
        // Delete
        // Modify
        // COPY (ROI#1 -> ROI#2)
        // CUT OFF
        // Clear
        #endregion

    }
}
