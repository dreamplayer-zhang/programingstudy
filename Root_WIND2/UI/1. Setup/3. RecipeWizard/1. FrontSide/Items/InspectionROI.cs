

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

        private List<PointLine> m_Data = new List<PointLine>();
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
        private int m_Size = 0;
        public int p_Size
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
        private Color m_Color;
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
    }
}
