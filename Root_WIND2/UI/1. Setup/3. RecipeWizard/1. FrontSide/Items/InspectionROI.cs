

using RootTools;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;

namespace Root_WIND2
{
    public class InspectionROI : ObservableObject
    {
        public InspectionROI()
        {
        }

        public int p_Index
        {
            get
            {
                return m_Index;
            }
            set
            {
                SetProperty(ref m_Index, value);
            }

        }
        private int m_Index = 0;

        public List<PointLine> p_Data
        {
            get
            {
                return m_Data;
            }
            set
            {
                SetProperty(ref m_Data, value);
            }
        }
        private List<PointLine> m_Data = new List<PointLine>();

        public long p_Size
        {
            get
            {
                return m_Size;
            }
            set
            {
                SetProperty(ref m_Size, value);
            }
        }
        private long m_Size = 0;

        public Color p_Color
        {
            get
            {
                return m_Color;
            }
            set
            {
                SetProperty(ref m_Color, value);
            }
        }
        private Color m_Color;
    }
}
