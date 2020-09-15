using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace Root_WIND2
{
    public class RecipeData_Origin
    {
        CPoint m_ptOrigin;
        CRect m_rtOrigin;
        int m_nWidth;
        int m_nHeight;

        public RecipeData_Origin()
        {

        }

        public CPoint GetOriginPoint()
        {
            return m_ptOrigin;
        }

        public CRect GetOriginRect()
        {
            return m_rtOrigin;
        }

        public void SetOrigin(CPoint Origin, int nWidth, int nHeight)
        {
            m_ptOrigin = Origin;
            m_nWidth = nWidth;
            m_nHeight = nHeight;
            m_rtOrigin = new CRect(Origin, nWidth, nHeight);
        }

        public void SetOrigin(CRect rtOrigin)
        {
            m_ptOrigin = new CPoint(rtOrigin.Left, rtOrigin.Top);
            m_rtOrigin = rtOrigin;
            m_nWidth = rtOrigin.Width;
            m_nHeight = rtOrigin.Height;
        }
    }
}
