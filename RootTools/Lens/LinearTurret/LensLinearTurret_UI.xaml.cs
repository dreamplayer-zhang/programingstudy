using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;

namespace RootTools.Lens.LinearTurret
{
    /// <summary>
    /// Lens_LinearTurret_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LensLinearTurret_UI : UserControl
    {
        public LensLinearTurret_UI()
        {
            InitializeComponent();
        }

        LensLinearTurret m_lens;
        public void Init(LensLinearTurret lens)
        {
            m_lens = lens;
            DataContext = lens;
            treeUI.Init(lens.p_treeRoot);
            lens.RunTree(Tree.eMode.Init); 
            comboBoxPos.ItemsSource = lens.p_asPos; 
            tabItemAxis.Content = lens.m_axis.p_ui; 
        }

        private void buttonChange_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxPos.SelectedValue is string)
            {
                m_lens.ChangePos((string)comboBoxPos.SelectedValue);
            }
        }

    }
}
