using Microsoft.Win32;
using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;

namespace Root_VEGA_P.Engineer
{
    /// <summary>
    /// VEGA_P_Recipe_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class VEGA_P_Recipe_UI : UserControl
    {
        public VEGA_P_Recipe_UI()
        {
            InitializeComponent();
        }

        VEGA_P_Recipe m_recipe;
        VEGA_P_Handler m_handler; 
        ModuleRunList m_moduleRunList;
        public void Init(VEGA_P_Handler handler)
        {
            m_handler = handler;
            m_recipe = handler.m_recipe; 
            m_moduleRunList = m_recipe.m_moduleRunList;
            this.DataContext = m_recipe;
            treeRootUI.Init(m_moduleRunList.m_treeRoot);
            m_moduleRunList.RunTree(Tree.eMode.Init);
            InitModule(); 
        }

        void InitModule()
        {
            Recipe_Module_UI ui = InitModule(m_handler.m_loadport, 0, 300);
            ui.AddInfoPod(InfoPod.ePod.EOP_Dome, true, false, m_handler.m_loadport);
            ui.AddInfoPod(InfoPod.ePod.EIP_Cover, true, false, m_handler.m_loadport);
            ui.AddInfoPod(InfoPod.ePod.EIP_Plate, true, false, m_handler.m_loadport);
            ui.AddInfoPod(InfoPod.ePod.EOP_Door, true, false, m_handler.m_loadport);
            ui = InitModule(m_handler.m_EIP_Cover, -300, -20);
            ui.AddInfoPod(InfoPod.ePod.EIP_Cover, false, false, m_handler.m_EIP_Cover);
            ui = InitModule(m_handler.m_EIP_Plate, -300, 100);
            ui.AddInfoPod(InfoPod.ePod.EIP_Plate, false, false, m_handler.m_EIP_Plate);
            ui = InitModule(m_handler.m_EOP, -300, 220);
            ui.AddInfoPod(InfoPod.ePod.EOP_Dome, false, false, m_handler.m_EOP.m_dome);
            ui.AddInfoPod(InfoPod.ePod.EOP_Door, false, false, m_handler.m_EOP.m_door);
            ui = InitModule(m_handler.m_holder, 300, 0);
            ui.AddInfoPod(InfoPod.ePod.EIP_Cover, false, true, m_handler.m_holder);
            ui.AddInfoPod(InfoPod.ePod.EIP_Plate, false, true, m_handler.m_holder);
            ui = InitModule(m_handler.m_vision, 300, 150);
            ui.AddInfoPod(InfoPod.ePod.EIP_Cover, false, false, m_handler.m_vision);
            ui.AddInfoPod(InfoPod.ePod.EIP_Plate, false, false, m_handler.m_vision);
        }

        Recipe_Module_UI InitModule(ModuleBase module, int px, int py)
        {
            Recipe_Module_UI ui = new Recipe_Module_UI();
            ui.Init(m_recipe, module);
            ui.Margin = new Thickness(px, py, 0, 0);
            gridDrawing.Children.Add(ui); 
            return ui; 
        }

        #region Job
        
        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            m_recipe.RecipeOpen();
            m_recipe.m_moduleRunList.RunTree(Tree.eMode.Init);
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            m_recipe.RecipeSave(); 
            m_recipe.m_moduleRunList.RunTree(Tree.eMode.Init);
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            m_moduleRunList.Clear();
            m_recipe.m_moduleRunList.RunTree(Tree.eMode.Init);
            InitModule(); 
        }
        #endregion
    }
}
