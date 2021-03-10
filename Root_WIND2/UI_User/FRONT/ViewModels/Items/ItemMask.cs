

using RootTools;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;

namespace Root_WIND2
{
    public class ItemMask : ObservableObject
    {
        public ItemMask()
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
                long sum = 0;
                foreach(PointLine pl in value)
                {
                    sum += pl.Width;
                }

                p_Size = sum;

                if (m_fullSize != 0)
                    p_AreaRatio = (double)p_Size / (double)p_FullSize * 100;

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

        public long p_FullSize
        {
            get
            {
                return m_fullSize;
            }
            set
            {
                SetProperty(ref m_fullSize, value);
            }
        }
        private long m_fullSize = 0;

        public double p_AreaRatio
        {
            get
            {
                return m_areaRatio;
            }
            set
            {
                SetProperty(ref m_areaRatio, value);
            }
        }
        private double m_areaRatio = 0;

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
