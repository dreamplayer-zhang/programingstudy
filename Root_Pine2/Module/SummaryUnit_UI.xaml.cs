using System.Windows.Controls;
using System.Windows.Media;

namespace Root_Pine2.Module
{
    /// <summary>
    /// SummaryUnit_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SummaryUnit_UI : UserControl
    {
        public SummaryUnit_UI()
        {
            InitializeComponent();
        }

        public void SetResult(Summary.Data.eVision eVision, Brush brush)
        {
            switch (eVision)
            {
                case Summary.Data.eVision.Total: gridTotal.Background = brush; break;
                case Summary.Data.eVision.Top3D: grid3D.Background = brush; break;
                case Summary.Data.eVision.Top2D: gridTop.Background = brush; break;
                case Summary.Data.eVision.Bottom: gridBottom.Background = brush; break;
            }
        }
    }
}
