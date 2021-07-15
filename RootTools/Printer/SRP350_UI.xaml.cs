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

namespace RootTools.Printer
{
    /// <summary>
    /// SRP350_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SRP350_UI : UserControl
    {
        public SRP350_UI()
        {
            InitializeComponent();
        }

        SRP350 m_srp350; 
        public void Init(SRP350 srp350)
        {
            m_srp350 = srp350;
            DataContext = srp350;
        }

        private void buttonWrite_Click(object sender, RoutedEventArgs e)
        {
            m_srp350.Start();
            m_srp350.WriteText("Test");
            m_srp350.WriteText("Test", SRP350.eAlign.Center);
            m_srp350.WriteQR("1234567890ABCDEFG");
            m_srp350.End(); 
        }
    }
}
