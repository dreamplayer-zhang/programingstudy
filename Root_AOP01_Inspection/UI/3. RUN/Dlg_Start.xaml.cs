using System.Windows;
using RootTools;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.IO;
using Root_AOP01_Inspection.UI._3._RUN;
using System.ComponentModel;
using Root_AOP01_Inspection.UI_UserControl;
using RootTools.Gem;

namespace Root_AOP01_Inspection
{
    /// <summary>
    /// Dlg_Start.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Dlg_Start : Window
    {
        static public bool m_bShow = false;
        AOP01_Engineer m_engineer;
        AOP01_Handler m_handler;
        AOP01_Recipe m_recipe;
        InfoCarrier m_infoCarrier = null;
        public Dlg_Start(InfoCarrier infoCarrier)
        {
            InitializeComponent();
            m_infoCarrier = infoCarrier;
        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            m_bShow = false;
        }
        public void Init(AOP01_Engineer engineer)
        {
            m_engineer = engineer;
            m_handler = engineer.m_handler;
            m_recipe = m_handler.m_recipe;
        }
        ManualJobSchedule m_JobSchedule;
        public void Init(ManualJobSchedule jobschdule)
        {
            m_aRecipe = new ObservableCollection<Recipe>();
            listviewRCP.ItemsSource = m_aRecipe;
            m_JobSchedule = jobschdule;
            this.DataContext = jobschdule;
            LoadportNum.Text = Loadport_UI.sLoadportNum;
        }   
        #region Recipe List
        public class Recipe : NotifyProperty
        {
            int _nNumber = 1;
            public int p_nNumber
            {
                get { return _nNumber; }
                set
                {
                    _nNumber = value;
                    OnPropertyChanged();
                }
            }
            public string p_sRecipeName { get; set; }
            public string p_sDate { get; set; }

            public Recipe(int nNumber, string sRecipe, string sDate)
            {
                p_nNumber = nNumber + 1;
                p_sRecipeName = sRecipe;
                p_sDate = sDate;
            }
        }
        public ObservableCollection<Recipe> m_aRecipe { get; set; }

        public void ClearRecipe()
        {
            m_aRecipe.Clear();
        }

        public void AddRecipe(string sRecipe, string sDate)
        {
            Recipe recipe = new Recipe(m_aRecipe.Count, sRecipe, sDate);
            m_aRecipe.Add(recipe);
        }
        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearRecipe();
            string[] Getfiles = Directory.GetFiles(m_recipe.m_sPath, "*.AOP01");
            foreach (string files in Getfiles)
            {
                FileInfo file = new FileInfo(files);
                string rcpname = file.Name;
                AddRecipe(rcpname, file.LastWriteTime.ToString());
            }
        }
        public string sRecipeName = "";
        public string sRecipe = "";
        void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {  
            Recipe typeItem = (Recipe)listviewRCP.SelectedItem;
            sRecipeName = typeItem.p_sRecipeName.ToString();
            RecipeID.Text = sRecipeName;
            sRecipe = m_recipe.m_sPath + sRecipeName;   
        }
        #endregion


        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            m_handler.bInit = true;
            InfoWafer infoWafer = m_infoCarrier.GetInfoWafer(0);
            if (infoWafer != null)
            {
                infoWafer.RecipeOpen(sRecipe);
                m_handler.AddSequence(infoWafer);
                m_handler.CalcSequence();
            }

            this.DialogResult = true;
        }
    }

}
