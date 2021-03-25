using System.Windows.Controls;

namespace Root_VEGA_P_Vision
{
    /// <summary>
    /// SelectMode.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SelectMode : UserControl
    {
        public SelectMode()
        {
            InitializeComponent();
            DataContext = new MainVM(this);
        }
    }
}
