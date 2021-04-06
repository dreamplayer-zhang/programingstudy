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
using Root_VEGA_D.Engineer;
using RootTools;
namespace Root_VEGA_D
{
    /// <summary>
    /// RecipeWizard_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RecipeWizard_UI : UserControl
    {
        public RecipeWizard_UI()
        {
            InitializeComponent();
        }

        public RootViewer_ViewModel viewerVM;
        public VEGA_D_Engineer m_engineer;
        public bool init(VEGA_D_Engineer engineer)
        {
            m_engineer = engineer;
            viewerVM = new RootViewer_ViewModel();
            viewerVM.init(new ImageData(m_engineer.ClassMemoryTool().GetMemory("Vision.Memory", "Vision", "Main")));
            Viewer_UI.DataContext = viewerVM;
            return true;
        }
    }
}
