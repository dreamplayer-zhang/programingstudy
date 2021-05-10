using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RootTools;

namespace Root_WindII
{
    public class Mask : ObservableObject
    {
        private Mask m_mask;
        public Mask p_mask
        {
            get
            {
                return m_mask;
            }
            set
            {
                SetProperty(ref m_mask, value);
            }
        }
        private string m_sName;
        public string p_sName
        {
            get
            {
                return m_sName;
            }
            set
            {
                SetProperty(ref m_sName, value);
            }
        }

        private System.Windows.Point m_PtMask;
        public System.Windows.Point p_PtMask
        {
            get
            {
                return m_PtMask;
            }
            set
            {
                SetProperty(ref m_PtMask, value);
            }
        }

        private double m_dWidth;
        public double p_dWidth
        {
            get
            {
                return m_dWidth;
            }
            set
            {
                SetProperty(ref m_dWidth, value);
            }
        }

        private double m_dHeight;
        public double p_dHeight
        {
            get
            {
                return m_dHeight;
            }
            set
            {
                SetProperty(ref m_dHeight, value);
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
