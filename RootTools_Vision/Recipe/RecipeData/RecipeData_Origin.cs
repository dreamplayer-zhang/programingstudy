using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace RootTools_Vision
{
    public class RecipeData_Origin
    {

        #region [Graphics XML Serialize 변수(레시피)]
        // 무조건 Public 선언되어야 함
        public CRect m_rtOrigin;
        public CPoint m_ptABSOrigin; // 메모리 기준 Origin 절대좌표 
        public CPoint m_ptRELOrigin; // Origin 상대좌표 
        public int m_nWidth; // Origin Width
        public int m_nHeight; // Origin Height
        #endregion

        public RecipeData_Origin()
        {
            m_nWidth = 0;
            m_nHeight = 0;
            m_ptABSOrigin = new CPoint(0, 0);
            m_ptRELOrigin = new CPoint(0, 0);
            m_rtOrigin = new CRect(0, 0, 10, 20);
        }

        public CPoint GetOriginPoint()
        {
            return m_ptABSOrigin;
        }

        public CRect GetOriginRect()
        {
            return m_rtOrigin;
        }

        public void SetOrigin(CPoint Origin, int nWidth, int nHeight)
        {
            m_ptABSOrigin = Origin;
            m_nWidth = nWidth;
            m_nHeight = nHeight;
            m_rtOrigin = new CRect(Origin, nWidth, nHeight);
        }

        public void SetOrigin(CRect rtOrigin)
        {
            m_ptABSOrigin = new CPoint(rtOrigin.Left, rtOrigin.Top);
            m_rtOrigin = rtOrigin;
            m_nWidth = rtOrigin.Width;
            m_nHeight = rtOrigin.Height;
        }
    }
}
