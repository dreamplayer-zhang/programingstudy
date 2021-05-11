using Root_VEGA_P_Vision.Module;
using RootTools.Module;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Root_VEGA_P.Engineer
{
    /// <summary>
    /// Recipe_Module_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Recipe_Module_UI : UserControl
    {
        public Recipe_Module_UI()
        {
            InitializeComponent();
        }

        VEGA_P_Recipe m_recipe; 
        ModuleBase m_module; 
        public void Init(VEGA_P_Recipe recipe, ModuleBase module)
        {
            m_recipe = recipe; 
            m_module = module;
            comboBox.ItemsSource = module.m_asModuleRun;
            textBlockHeader.Text = module.p_id;
        }

        List<Recipe_InfoPod_UI> m_aInfoPod = new List<Recipe_InfoPod_UI>(); 
        public void AddInfoPod(string id, bool bExist, IRTRChild child)
        {
            Recipe_InfoPod_UI ui = new Recipe_InfoPod_UI();
            ui.Init(id, bExist, child);
            m_aInfoPod.Add(ui);
            stackPanelInfoPod.Children.Add(ui); 
        }

        private void buttonRun_Click(object sender, RoutedEventArgs e)
        {
            //forget
        }
    }
}
