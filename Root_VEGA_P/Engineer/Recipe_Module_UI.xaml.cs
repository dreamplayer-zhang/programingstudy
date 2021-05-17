using Root_VEGA_P_Vision.Module;
using RootTools.Module;
using RootTools.Trees;
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
            comboBox.ItemsSource = module.m_asRecipe;
            textBlockHeader.Text = module.p_id;
        }

        List<Recipe_InfoPod_UI> m_aInfoPod = new List<Recipe_InfoPod_UI>(); 
        public void AddInfoPod(InfoPod.ePod ePod, bool bExist, bool bRunFlip, IRTRChild child)
        {
            Recipe_InfoPod_UI ui = new Recipe_InfoPod_UI();
            ui.Init(ePod, bExist, bRunFlip, m_recipe, child);
            m_aInfoPod.Add(ui);
            stackPanelInfoPod.Children.Add(ui); 
        }

        public void ClearRecipe()
        {
            foreach (Recipe_InfoPod_UI ui in m_aInfoPod) ui.ClearRecipe(); 
        }

        private void buttonRun_Click(object sender, RoutedEventArgs e)
        {
            string sRun = (string)comboBox.SelectedItem;
            m_recipe.m_moduleRunList.Add(m_module, sRun);
            m_recipe.m_moduleRunList.RunTree(Tree.eMode.Init); 
        }
    }
}
