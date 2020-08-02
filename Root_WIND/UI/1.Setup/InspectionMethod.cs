using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using RootTools;

namespace Root_WIND
{
    public class InspectionMethod : ObservableObject
    {
        public delegate void ChangeMode(object e);
        public event ChangeMode changeMode;

        public InspectionMethod()
        {
            init();
        }
        private void init()
        {
            m_Surface = new Surface(this);
            m_D2D = new D2D(this);
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

        private InspectionMode m_inspMode = InspectionMode.Surface;
        public InspectionMode p_inspMode
        {
            get
            {
                return m_inspMode;
            }
            set
            {
                p_nValue = 0;
                p_nSize = 0;
                SetProperty(ref m_inspMode, value);
                changeMode(this);
            }
        }

        private int m_nValue;
        public int p_nValue
        {
            get
            {
                return m_nValue;
            }
            set
            {
                SetProperty(ref m_nValue, value);
            }
        }

        private int m_nSize;
        public int p_nSize
        {
            get
            {
                return m_nSize;
            }
            set
            {
                SetProperty(ref m_nSize, value);
            }
        }


        public Surface m_Surface;
        public D2D m_D2D;


    }
    public class Surface : ObservableObject
    {
        InspectionMethod inspItem;
        public Surface(InspectionMethod item)
        {
            inspItem = item;
        }
        private bool m_nAbsGV = true;
        private SurfaceMode m_surfaceMode = SurfaceMode.Dark;
        private int m_nPitGV = 0;
        private int m_nSize = 0;


        [Category("Inspection Item")]
        [DisplayName("Name")]
        public string p_sName
        {
            get
            {
                return inspItem.p_sName;
            }
            set
            {
                inspItem.p_sName = value;
            }
        }
        [Category("Inspection Item")]
        [DisplayName("Mode")]
        public InspectionMode p_inspMode
        {
            get
            {
                return inspItem.p_inspMode;
            }
            set
            {
                p_nPitGV = 0;
                p_nSize = 0;
                inspItem.p_inspMode = value;
            }
        }

        [Category("Parameter")]
        [DisplayName("Use Absolute GV")]
        public bool p_nAbsGV
        {
            get
            {
                return m_nAbsGV;
            }
            set
            {
                SetProperty(ref m_nAbsGV, value);
            }
        }
        [Category("Parameter")]
        [DisplayName("Surface Mode")]
        public SurfaceMode p_surfaceMode
        {
            get
            {
                return m_surfaceMode;
            }
            set
            {
                SetProperty(ref m_surfaceMode, value);
            }
        }
        [Category("Parameter")]
        [DisplayName("Pit Level(GV)")]
        public int p_nPitGV
        {
            get
            {
                return m_nPitGV;
            }
            set
            {
                inspItem.p_nValue = value;
                SetProperty(ref m_nPitGV, value);
            }
        }
        [Category("Parameter")]
        [DisplayName("Pit Size(Pxl)")]
        public int p_nSize
        {
            get
            {
                return m_nSize;
            }
            set
            {
                inspItem.p_nSize = value;
                SetProperty(ref m_nSize, value);
            }
        }

        public enum SurfaceMode
        {
            Bright,
            Dark,
        }

    }
    public class D2D : ObservableObject
    {
        InspectionMethod inspItem;
        public D2D(InspectionMethod item)
        {
            inspItem = item;
        }
        private int m_nIntensity = 0;
        private int m_nSize = 0;

        [Category("Inspection Item")]
        [DisplayName("Name")]
        public string p_sName
        {
            get
            {
                return inspItem.p_sName;
            }
            set
            {
                inspItem.p_sName = value;
            }
        }
        [Category("Inspection Item")]
        [DisplayName("Mode")]
        public InspectionMode p_inspMode
        {
            get
            {
                return inspItem.p_inspMode;
            }
            set
            {
                p_nIntentsity = 0;
                p_nsize = 0;
                inspItem.p_inspMode = value;
            }
        }
        [Category("Parameter")]
        [DisplayName("Intensity")]
        public int p_nIntentsity
        {
            get
            {
                return inspItem.p_nValue;
            }
            set
            {
                inspItem.p_nValue = value;
                SetProperty(ref m_nIntensity, value);
            }
        }
        [Category("Parameter")]
        [DisplayName("Size(Pxl)")]
        public int p_nsize
        {
            get
            {
                return inspItem.p_nSize;
            }
            set
            {
                inspItem.p_nSize = value;
                SetProperty(ref m_nSize, value);
            }
        }
    }
    public enum InspectionMode
    {
        Surface,
        D2D,
    }



}
