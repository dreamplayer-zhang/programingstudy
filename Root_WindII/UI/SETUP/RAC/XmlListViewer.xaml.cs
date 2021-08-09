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

namespace Root_WindII
{
    /// <summary>
    /// XmlListViewer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class XmlListViewer : UserControl
    {
        public XmlListViewer()
        {
            InitializeComponent();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchBox.Focus();
        }

        private void TextBlock_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.IBeam;
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }
    }
}
