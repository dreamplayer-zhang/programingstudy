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
            Recipe_Module_UI ui = InitModule(m_handler.m_loadport, 0, 200);
            ui.AddInfoPod(InfoPod.ePod.EOP_Dome.ToString(), true, m_handler.m_loadport);
            ui.AddInfoPod(InfoPod.ePod.EIP_Cover.ToString(), true, m_handler.m_loadport);
            ui.AddInfoPod(InfoPod.ePod.EIP_Plate.ToString(), true, m_handler.m_loadport);
            ui.AddInfoPod(InfoPod.ePod.EOP_Door.ToString(), true, m_handler.m_loadport);
            ui = InitModule(m_handler.m_EIP_Cover, -250, -100);
            ui.AddInfoPod(InfoPod.ePod.EIP_Cover.ToString(), false, m_handler.m_EIP_Cover);
            ui = InitModule(m_handler.m_EIP_Plate, -250, 0);
            ui.AddInfoPod(InfoPod.ePod.EIP_Plate.ToString(), false, m_handler.m_EIP_Plate);
            ui = InitModule(m_handler.m_EOP, -250, 100);
            ui.AddInfoPod(InfoPod.ePod.EOP_Dome.ToString(), false, m_handler.m_EOP.m_dome);
            ui.AddInfoPod(InfoPod.ePod.EOP_Door.ToString(), false, m_handler.m_EOP.m_door);
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
        string m_sPath = "c:\\Recipe\\";
        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            string sModel = EQ.m_sModel;
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = m_sPath;
            dlg.DefaultExt = "." + sModel;
            dlg.Filter = sModel + " Recipe (." + sModel + ")|*." + sModel;
            if (dlg.ShowDialog() == true) m_moduleRunList.OpenJob(dlg.FileName);
            m_recipe.m_moduleRunList.RunTree(Tree.eMode.Init);
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            string sModel = EQ.m_sModel;
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.InitialDirectory = m_sPath;
            dlg.DefaultExt = "." + sModel;
            dlg.Filter = sModel + " Recipe (." + sModel + ")|*." + sModel;
            if (dlg.ShowDialog() == true) m_moduleRunList.SaveJob(dlg.FileName);
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
