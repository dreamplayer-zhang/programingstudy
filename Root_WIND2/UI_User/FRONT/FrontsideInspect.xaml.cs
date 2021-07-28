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

namespace Root_WIND2.UI_User
{
    /// <summary>
    /// FrontsideInspect.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FrontsideInspect : UserControl
    {
        public FrontsideInspect()
        {
            InitializeComponent();
        }

        private void PackIconMaterial_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RootTools.D2DSequenceViewer D2DInfo = new RootTools.D2DSequenceViewer();
            D2DInfo.Show();
            D2DInfo.Topmost = true;
        }
    }
}
