using System;
using System.Collections.Generic;
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

namespace Root_Siltron.Icons
{
    /// <summary>
    /// AtiLogo.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AtiLogo : UserControl
    {
        public AtiLogo()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color",
            typeof(SolidColorBrush),
            typeof(AtiLogo),
            new PropertyMetadata(Brushes.Black, new PropertyChangedCallback(ColorChange))
        );

        private static void ColorChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AtiLogo logoControl = d as AtiLogo;
            logoControl.logo.Fill = (SolidColorBrush)e.NewValue;          
        }

        public SolidColorBrush LogoColor
        {
            get
            {
                return (SolidColorBrush)GetValue(ColorProperty);
            }
            set
            {
                SetValue(ColorProperty, value);
            }
        }
    }
    
}
