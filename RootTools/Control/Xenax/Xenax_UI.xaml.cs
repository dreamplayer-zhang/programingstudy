using System.Windows.Controls;

namespace RootTools.Control.Xenax
{
    /// <summary>
    /// Xenax_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Xenax_UI : UserControl
    {
        public Xenax_UI()
        {
            InitializeComponent();
        }

        public void Init(Xenax xenax)
        {
            this.DataContext = xenax;
            xenaxListAxisUI.Init(xenax.m_listAxis);
        }

    }
}
