using RootTools.Module;
using System.Windows.Controls;

namespace Root_Vega
{
    /// <summary>
    /// _1_1_Module.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class _1_1_Module : UserControl
    {
        public _1_1_Module()
        {
            InitializeComponent();
        }

        ModuleBase m_module; 
        public void Init(ModuleBase module)
        {
            m_module = module;
            this.DataContext = module; 
        }
    }
}
