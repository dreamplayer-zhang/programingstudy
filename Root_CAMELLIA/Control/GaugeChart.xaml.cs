using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Root_CAMELLIA.Control
{
    /// <summary>
    /// GaugeChart.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GaugeChart : UserControl, INotifyPropertyChanged

    {
        public GaugeChart()
        {
            InitializeComponent();
        }

        double m_from = 0;
        public double p_from
        {
            get
            {
                return m_from;
            }
            set
            {
                m_from = value;
                OnPropertyChanged("p_from");
            }
        }

        string m_unit = "MPa";
        public string p_unit
        {
            get
            {
                return m_unit;
            }
            set
            {
                m_unit = value;
                OnPropertyChanged("p_unit");
            }
        }

        double m_to = 100;
        public double p_to
        {
            get
            {
                return m_to;
            }
            set
            {
                m_to = value;
                OnPropertyChanged("p_to");
            }
        }

        double m_value = 50;
        public double p_value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
                OnPropertyChanged("p_value");
            }
        }

        string m_name = "null";
        public string p_name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
                OnPropertyChanged("p_name");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
