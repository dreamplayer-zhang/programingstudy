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
    /// Warning.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Warning_UI : UserControl
    {
        public Warning_UI()
        {
            InitializeComponent();
        }

        public void Init(WindII_Warning warn)
        {
            DataContext = warn;
        }

    }
}
