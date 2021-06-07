using Microsoft.Win32;
using RootTools;
using RootTools.Trees;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Root_Pine2_Vision.Module
{
    /// <summary>
    /// Vision_Snap_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Recipe_UI : UserControl, ITool
    {
        public Recipe_UI()
        {
            InitializeComponent();
        }

        Vision2D m_vision;
        public void Init(Vision2D vision)
        {
            m_vision = vision;
            DataContext = vision;
            SetRecipeList();
            treeRootAUI.Init(vision.m_recipe[Vision2D.eWorks.A].m_treeRecipe);
            treeRootBUI.Init(vision.m_recipe[Vision2D.eWorks.B].m_treeRecipe);
            vision.m_recipe[Vision2D.eWorks.A].RunTreeRecipe(Tree.eMode.Init);
            vision.m_recipe[Vision2D.eWorks.B].RunTreeRecipe(Tree.eMode.Init);
        }

        void SetRecipeList()
        {
            List<string> asRecipe = new List<string>(); 
            DirectoryInfo info = new DirectoryInfo(EQ.c_sPathRecipe);
            foreach (DirectoryInfo dir in info.GetDirectories()) asRecipe.Add(dir.Name);
            comboBoxOpen.ItemsSource = asRecipe; 
        }

        private void comboBoxOpen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string sRecipe = (string)comboBoxOpen.SelectedItem;
            textBoxRecipe.Text = sRecipe; 
            m_vision.m_recipe[Vision2D.eWorks.A].RecipeOpen(sRecipe);
            m_vision.m_recipe[Vision2D.eWorks.B].RecipeOpen(sRecipe);
            labelInfo.Content = "Recipe Open Done : " + sRecipe;
            m_vision.m_recipe[Vision2D.eWorks.A].RunTreeRecipe(Tree.eMode.Init);
            m_vision.m_recipe[Vision2D.eWorks.B].RunTreeRecipe(Tree.eMode.Init);
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxRecipe.Text == "") return; 
            m_vision.m_recipe[Vision2D.eWorks.A].RecipeSave(textBoxRecipe.Text);
            m_vision.m_recipe[Vision2D.eWorks.B].RecipeSave(textBoxRecipe.Text);
            SetRecipeList();
            comboBoxOpen.SelectedItem = textBoxRecipe.Text;
            labelInfo.Content = "Recipe Save Done : " + textBoxRecipe.Text; 
        }

        /*
                private void buttonOpen_Click(object sender, RoutedEventArgs e)
                {
                    OpenFileDialog dlg = new OpenFileDialog();
                    string sExt = "." + c_sExt;
                    dlg.DefaultExt = sExt;
                    dlg.Filter = "Recipe File (*" + sExt + ")|*" + sExt;
                    dlg.InitialDirectory = EQ.c_sPathRecipe;
                    if (dlg.ShowDialog() == false) return;
                    Job job = new Job(dlg.FileName, false, m_vision.m_log);
                    m_treeRoot.m_job = job;
                    m_treeRoot.p_eMode = Tree.eMode.JobOpen;
                    m_recipe.RunTree(m_treeRoot, true); 
                    job.Close();
                    RunTree(Tree.eMode.Init); 
                }

                private void buttonSave_Click(object sender, RoutedEventArgs e)
                {
                    SaveFileDialog dlg = new SaveFileDialog();
                    string sExt = "." + c_sExt;
                    dlg.DefaultExt = sExt;
                    dlg.Filter = "Pine2 Recipe File (*" + sExt + ")|*" + sExt;
                    dlg.InitialDirectory = EQ.c_sPathRecipe;
                    if (dlg.ShowDialog() == false) return;
                }
        */
        #region ITool
        public string p_id
        {
            get { return m_vision.p_id + ".Recipe"; }
            set { }
        }

        public UserControl p_ui { get { return this; } }

        public void ThreadStop() { }
        #endregion
    }
}
