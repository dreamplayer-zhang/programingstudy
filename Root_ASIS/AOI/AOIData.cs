using RootTools;
using RootTools.Memory;
using RootTools.Trees;
using System;
using System.Windows.Media;

namespace Root_ASIS.AOI
{
    public class AOIData : NotifyProperty
    {
        #region Property
        public string m_sDisplay = ""; 
        public bool m_bEnable = true;
        public bool m_bInspect = false; 
        public CPoint m_cp0 = new CPoint();
        public CPoint m_sz;
        public RPoint m_rpCenter = new RPoint();
        #endregion

        #region ROI State
        public enum eROI
        {
            Ready,
            Active,
            Done
        }
        eROI _eROI = eROI.Ready;
        public eROI p_eROI
        {
            get { return _eROI; }
            set
            {
                if (_eROI == value) return;
                _eROI = value;
                OnPropertyChanged(); 
                switch (_eROI)
                {
                    case eROI.Ready: p_brushROI = Brushes.Gray; break;
                    case eROI.Active: p_brushROI = Brushes.Red; break;
                    case eROI.Done: p_brushROI = Brushes.YellowGreen; break; 
                }
            }
        }

        Brush _brushROI = Brushes.Gray; 
        public Brush p_brushROI
        {
            get { return _brushROI; }
            set
            {
                if (_brushROI == value) return;
                _brushROI = value;
                OnPropertyChanged(); 
            }
        }
        #endregion

        #region Move State
        public enum eMove
        {
            None,
            Draw,
            Shift,
            Resize
        }
        public eMove m_eMove = eMove.None;
        #endregion

        #region Draw
        public enum eDraw
        {
            ROI,
            Inspect,
            All,
        };

        public void Draw(MemoryDraw draw, eDraw eDraw)
        {
            Brush brush = null; 
            switch (eDraw)
            {
                case eDraw.ROI:
                    if (p_eROI == eROI.Ready) return; 
                    brush = p_brushROI; 
                    break;
                case eDraw.Inspect:
                    if (m_bInspect == false) return;
                    brush = Brushes.Red;
                    break;
                case eDraw.All:
                    brush = Brushes.Green;
                    break; 
            }
            draw.AddRectangle(brush, m_cp0, m_cp0 + m_sz);
            draw.AddText(brush, m_cp0, p_id);
            draw.AddText(brush, m_cp0.X, m_cp0.Y + m_sz.Y, m_sDisplay);
            if (m_bInspect) draw.AddCross(brush, new CPoint(m_rpCenter), 16); 
        }
        #endregion

        #region Position
        public enum eShape
        {
            None,
            Set,
            Move,
            Resize,
        }
        eShape m_eShape = eShape.None;

        CPoint m_dpMove = new CPoint();
        CPoint m_dpResize = new CPoint(); 
        public void LBD(bool bDown, CPoint cpImg)
        {
            if (bDown && (m_eShape == eShape.None))
            {
                m_eShape = CheckLBD(cpImg); 
                switch (m_eShape)
                {
                    case eShape.Set: m_cp0 = cpImg; break;
                    case eShape.Move: m_dpMove = cpImg - m_cp0; break;
                    case eShape.Resize: m_dpResize = cpImg - m_sz; break; 
                }
            }
            else if (m_eShape != eShape.None)
            {
                m_eShape = eShape.None;
                p_eROI = eROI.Done; 
            }
        }

        public void MouseMove(CPoint cpImg)
        {
            switch (m_eShape)
            {
                case eShape.Set: m_sz = cpImg - m_cp0; break; 
                case eShape.Move: m_cp0 = cpImg - m_dpMove; break;
                case eShape.Resize: m_sz = cpImg - m_dpResize; break; 
                default: break; 
            }
        }

        eShape CheckLBD(CPoint cpImg)
        {
            if (IsInside(cpImg) == false) return eShape.Set;
            int dL = Math.Min(m_sz.X, m_sz.Y) / 3; 
            CPoint dp = cpImg - m_cp0;
            if ((dp.X < dL) && (dp.Y < dL)) return eShape.Move;
            dp = m_sz - cpImg;
            if ((dp.X < dL) && (dp.Y < dL)) return eShape.Resize;
            return eShape.Set;
        }

        bool IsInside(CPoint cpImg)
        {
            CPoint dp = cpImg - m_cp0;
            if ((dp.X < 0) || (dp.X > m_sz.X)) return false;
            if ((dp.Y < 0) || (dp.Y > m_sz.Y)) return false;
            return true; 
        }
        #endregion

        #region Tree
        public void RunTree(Tree treeParent, bool bVisible)
        {
            Tree tree = treeParent.GetTree(p_id, false, bVisible);
            p_eROI = (eROI)tree.Set(p_eROI, p_eROI, "eROI", "eROI", bVisible);
            m_cp0 = tree.Set(m_cp0, m_cp0, "cp0", "cp0", bVisible);
        }
        #endregion

        public AOIData Clone()
        {
            AOIData aoi = new AOIData(p_id, m_sz);
            aoi.m_cp0 = new CPoint(m_cp0);
            aoi.p_eROI = p_eROI;
            return aoi; 
        }

        public string p_id { get; set; }
        public AOIData(string id, CPoint sz)
        {
            p_id = id;
            m_sz = sz; 
        }
    }
}
