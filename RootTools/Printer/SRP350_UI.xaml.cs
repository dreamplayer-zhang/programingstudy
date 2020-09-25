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
            comboPrinter.ItemsSource = srp350.m_asPriner; 
        }

        private void buttonCut_Click(object sender, RoutedEventArgs e)
        {
            m_srp350.Cut(m_srp350.p_bCutFeeding); 
        }

        private void buttonWrite_Click(object sender, RoutedEventArgs e)
        {
            m_srp350.Start("Test Print");
            m_srp350.Write(0, 0, SRP350.eFontA.FontA2x2, 9, "FontA2*2 9");
            m_srp350.Write(0, 0, SRP350.eFontKoean.Korean2x2, 9, "KoreanA2*2 9");
            m_srp350.Write(0, 0, "Arial", 9, "Arial 9 Bold", true, 10, false, false);
            m_srp350.End(); 
        }
    }
}
