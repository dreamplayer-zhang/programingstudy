using Root_AxisMapping.Module;
using RootTools;
using RootTools.Memory;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Root_AxisMapping.MainUI
{
    public class Result : NotifyProperty
    {
        #region Mapping Property
        public int p_xArray {  get { return m_mapping.p_xArray; } }
        public int p_yArray { get { return m_mapping.p_yArray; } }
        public Array[,] p_aArray {  get { return m_mapping.m_aArray; } }
        #endregion

        #region Draw
        List<int> m_aPos = new List<int>(); 
        void InitPos()
        {
            for (int n = 0; n < Math.Max(p_xArray, p_yArray); n++) m_aPos.Add(1024 * (n + 1));
        }

        RPoint m_rpCenter = new RPoint(); 
        public void Draw()
        {
            MemoryDraw draw = m_axisMapping.m_memoryPoolResult.m_viewer.p_memoryData.m_aDraw[0];
            draw.Clear(); 
            DrawBase(draw, Brushes.LightGray);
            m_rpCenter = p_aArray[p_xArray / 2, p_yArray / 2].m_rpCenter; 
            DrawResult(draw, Brushes.Red);
            draw.InvalidDraw(); 
        }

        void DrawBase(MemoryDraw draw, Brush brush)
        {
            for (int iy = 0; iy < p_yArray; iy++)
            {
                int y = m_aPos[iy];
                draw.AddLine(brush, new CPoint(m_aPos[0], y), new CPoint(m_aPos[p_xArray - 1], y)); 
            }
            for (int ix = 0; ix < p_yArray; ix++)
            {
                int x = m_aPos[ix];
                draw.AddLine(brush, new CPoint(x, m_aPos[0]), new CPoint(x, m_aPos[p_xArray - 1]));
            }
        }

        double m_fScale = 1; 
        void DrawResult(MemoryDraw draw, Brush brush)
        {
            for (int x = 0; x < p_xArray; x++) DrawResultX(draw, brush, x);
            for (int y = 0; y < p_xArray; y++) DrawResultY(draw, brush, y);
        }

        void DrawResultX(MemoryDraw draw, Brush brush, int ix)
        {
            int iy = 0;
            while ((iy < p_yArray) && (p_aArray[ix, iy].p_eState == Array.eState.Empty)) iy++;
            CPoint cp0 = GetResult(draw, brush, ix, iy); 
            while ((iy < p_yArray) && (p_aArray[ix, iy].p_eState != Array.eState.Empty))
            {
                CPoint cp1 = GetResult(draw, brush, ix, iy);
                draw.AddLine(brush, cp0, cp1); 
                iy++;
                cp0 = cp1; 
            }
        }

        void DrawResultY(MemoryDraw draw, Brush brush, int iy)
        {
            int ix = 0;
            while ((ix < p_yArray) && (p_aArray[ix, iy].p_eState == Array.eState.Empty)) ix++;
            CPoint cp0 = GetResult(draw, brush, ix, iy);
            while ((ix < p_yArray) && (p_aArray[ix, iy].p_eState != Array.eState.Empty))
            {
                CPoint cp1 = GetResult(draw, brush, ix, iy);
                draw.AddLine(brush, cp0, cp1);
                ix++;
                cp0 = cp1;
            }
        }

        double m_yPeriod = 20;
        double m_yPixelSize = 1.2;
        double p_yGap { get { return 1000 * m_yPeriod / m_yPixelSize; } }
        CPoint GetResult(MemoryDraw draw, Brush brush, int x, int y)
        {
            RPoint dp = p_aArray[x, y].m_rpCenter - m_rpCenter;
            dp.Y += p_yGap * (p_yArray / 2 - y); 
            int xp = (int)(Math.Round(dp.X * m_fScale / p_yGap * 1024 + m_aPos[x]));
            int yp = (int)(Math.Round(dp.Y * m_fScale / p_yGap * 1024 + m_aPos[y]));
            CPoint cpDraw = new CPoint(xp, yp);
            draw.AddText(brush, cpDraw, (m_yPixelSize * dp.X).ToString("0.0, ") + (m_yPixelSize * dp.Y).ToString("0.0")); 
            return cpDraw; 
        }

        void RunTreeDraw(Tree tree)
        {
            m_fScale = tree.Set(m_fScale, m_fScale, "Scale", "Draw Scale");
            m_yPeriod = tree.Set(m_yPeriod, m_yPeriod, "Y Period", "Y Period (mm)");
            m_yPixelSize = tree.Set(m_yPixelSize, m_yPixelSize, "Y Pixel", "Y Pixel Size (um)");
        }
        #endregion

        #region Tree
        public TreeRoot m_treeRoot; 
        void InitTree()
        {
            m_treeRoot = new TreeRoot(m_id, m_log);
            RunTree(Tree.eMode.RegRead);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite); 
        }

        public void RunTree(Tree.eMode eMode)
        {
            m_treeRoot.p_eMode = eMode;
            RunTreeDraw(m_treeRoot.GetTree("Draw")); 
        }
        #endregion

        string m_id; 
        Mapping m_mapping;
        AxisMapping_Engineer m_engineer;
        public AxisMapping m_axisMapping;
        Log m_log;
        public Result(string id, Mapping mapping, AxisMapping_Engineer engineer)
        {
            m_id = id;
            m_mapping = mapping; 
            m_engineer = engineer;
            m_axisMapping = ((AxisMapping_Handler)engineer.ClassHandler()).m_axisMapping;
            m_log = LogView.GetLog(id);
            InitPos();
            InitTree();
        }
    }
}
