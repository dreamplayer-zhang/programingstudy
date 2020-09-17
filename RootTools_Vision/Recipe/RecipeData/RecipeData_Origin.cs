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
        CPoint m_ptABSOrigin; // 메모리 기준 Origin 절대좌표 
        CPoint m_ptRELOrigin; // Origin 상대좌표 (0, 0)
        CRect m_rtOrigin;
        int m_nWidth;
        int m_nHeight;

        public RecipeData_Origin()
        {

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
