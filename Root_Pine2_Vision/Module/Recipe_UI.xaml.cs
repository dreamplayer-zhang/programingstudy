using RootTools;
using RootTools.Trees;
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
            comboBoxOpen.ItemsSource = vision.p_asRecipe;
            treeRootAUI.Init(vision.m_UIRecipe[eWorks.A].m_treeRecipe);
            treeRootBUI.Init(vision.m_UIRecipe[eWorks.B].m_treeRecipe);
            vision.m_UIRecipe[eWorks.A].RunTreeRecipe(Tree.eMode.Init);
            vision.m_UIRecipe[eWorks.B].RunTreeRecipe(Tree.eMode.Init);
        }

        private void comboBoxOpen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string sRecipe = (string)comboBoxOpen.SelectedItem;
            textBoxRecipe.Text = sRecipe; 
            m_vision.m_UIRecipe[eWorks.A].RecipeOpen(sRecipe);
            m_vision.m_UIRecipe[eWorks.B].RecipeOpen(sRecipe);
            labelInfo.Content = "Recipe Open Done : " + sRecipe;
            m_vision.m_UIRecipe[eWorks.A].RunTreeRecipe(Tree.eMode.Init);
            m_vision.m_UIRecipe[eWorks.B].RunTreeRecipe(Tree.eMode.Init);
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxRecipe.Text == "") return; 
            m_vision.m_UIRecipe[eWorks.A].RecipeSave(textBoxRecipe.Text);
            m_vision.m_UIRecipe[eWorks.B].RecipeSave(textBoxRecipe.Text);
            comboBoxOpen.ItemsSource = m_vision.p_asRecipe;
            comboBoxOpen.SelectedItem = textBoxRecipe.Text;
            labelInfo.Content = "Recipe Save Done : " + textBoxRecipe.Text; 
        }

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
