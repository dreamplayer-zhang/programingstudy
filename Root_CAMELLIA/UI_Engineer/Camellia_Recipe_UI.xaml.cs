using Microsoft.Win32;
using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;

namespace Root_CAMELLIA
{
    /// <summary>
    /// Camellia_Recipe_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Camellia_Recipe_UI : UserControl
    {
        public Camellia_Recipe_UI()
        {
            InitializeComponent();
        }

        CAMELLIA_Recipe m_recipe;
        ModuleRunList m_moduleRunList;
        public void Init(CAMELLIA_Recipe recipe)
        {
            m_recipe = recipe;
            m_moduleRunList = recipe.m_moduleRunList;
            this.DataContext = recipe;
            comboBoxModule.ItemsSource = recipe.m_asModule;
            treeRootUI.Init(m_moduleRunList.m_treeRoot);
            m_moduleRunList.RunTree(Tree.eMode.Init);
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
            dlg.Filter = sModel + " Recipe (." + sModel + ")|*." + sModel;
            if (dlg.ShowDialog() == true) m_moduleRunList.SaveJob(dlg.FileName);
            m_recipe.m_moduleRunList.RunTree(Tree.eMode.Init);
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            m_moduleRunList.Clear();
            m_recipe.m_moduleRunList.RunTree(Tree.eMode.Init);
        }
        #endregion

        #region ModuleRun
        string m_sModule = "";
        private void comboBoxModule_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            buttonAdd.Visibility = Visibility.Hidden;
            comboBoxModuleRun.ItemsSource = null;
            if (comboBoxModule.SelectedValue == null) return;
            m_sModule = comboBoxModule.SelectedValue.ToString();
            comboBoxModuleRun.ItemsSource = m_moduleRunList.GetRecipeRunNames(m_sModule);
        }
        string m_sModuleRun = "";
        private void comboBoxModuleRun_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            buttonAdd.Visibility = Visibility.Hidden;
            if (comboBoxModuleRun.SelectedValue == null) return;
            m_sModuleRun = comboBoxModuleRun.SelectedValue.ToString();
            buttonAdd.Visibility = Visibility.Visible;
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            m_moduleRunList.Add(m_sModule, m_sModuleRun);
            m_recipe.m_moduleRunList.RunTree(Tree.eMode.Init);
        }
        #endregion
    }
}
