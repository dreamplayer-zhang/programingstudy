using Root_ASIS.Teachs;
using RootTools;
using RootTools.Memory;
using RootTools.Trees;
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
                    case eROI.Ready: p_brushROI = Brushes.DarkGreen; break;
                    case eROI.Active: p_brushROI = Brushes.Red; break;
                    case eROI.Done: p_brushROI = Brushes.DarkBlue; break; 
                }
            }
        }

        Brush _brushROI = Brushes.DarkGreen; 
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
            draw.AddText(brush, m_cp0, m_sDisplay); 
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

        public void LBD(bool bDown, CPoint cpImg)
        {
            if (bDown && (m_eShape == eShape.None))
            {
                m_cp0 = cpImg;
                m_eShape = eShape.Set; 
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
                case eShape.Set:
                    m_sz.X = cpImg.X - m_cp0.X;
                    m_sz.Y = cpImg.Y - m_cp0.Y;
                    break;
                default: break; 
            }
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
