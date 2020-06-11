using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_Vega.ManualJob
{
    /// <summary>
    /// ManualJobSchedule.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ManualJobSchedule_UI : Window
    {
        static public bool m_bShow = false;
        public ManualJobSchedule_UI()
        {
            m_bShow = true;
            InitializeComponent();
        }

        ManualJobSchedule m_JobSchedule;
        public void Init(ManualJobSchedule jobschdule)
        {
            m_JobSchedule = jobschdule;
            this.DataContext = jobschdule.m_loadport.m_infoPod;
            textBoxLotID.DataContext = jobschdule.m_loadport.m_infoPod.m_aGemSlot[0];
            textBoxSlotID.DataContext = jobschdule.m_loadport.m_infoPod.m_aGemSlot[0];
            InitRecipeList();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_bShow = false;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        #region Recipe
        void InitRecipeList()
        {
            string[] asRecipeFile = Directory.GetFiles("c:\\Recipe");
            comboRecipeID.ItemsSource = asRecipeFile;
        }
        private void comboRecipeID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string sRecipe = (string)comboRecipeID.SelectedValue;
            if (sRecipe == null) return;
            m_JobSchedule.m_loadport.m_infoPod.p_infoReticle.m_sManualRecipe = sRecipe;
            m_JobSchedule.m_loadport.m_infoPod.p_infoReticle.RecipeOpen(sRecipe);
        }
        #endregion

        private void ButtonRun_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
