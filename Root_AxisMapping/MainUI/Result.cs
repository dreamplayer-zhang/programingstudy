using Root_AxisMapping.Module;
using RootTools;
using RootTools.Memory;
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

        public void Draw()
        {
            MemoryDraw draw = m_axisMapping.m_memoryPoolResult.m_viewer.p_memoryData.m_aDraw[0]; 
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
        }
    }
}
