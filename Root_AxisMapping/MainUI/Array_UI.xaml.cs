using System.Windows.Controls;
using System.Windows.Input;

namespace Root_AxisMapping.MainUI
{
    /// <summary>
    /// Array_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Array_UI : UserControl
    {
        public Array_UI()
        {
            InitializeComponent();
        }

        Array m_array; 
        public void Init(Array array)
        {
            m_array = array; 
            DataContext = array;
        }

        private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_array.OnSelect(); 
        }
    }
}
