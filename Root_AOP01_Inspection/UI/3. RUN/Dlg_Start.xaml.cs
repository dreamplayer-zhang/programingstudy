using System.Windows;
using RootTools;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.IO;
using Root_AOP01_Inspection.UI._3._RUN;


namespace Root_AOP01_Inspection
{
    /// <summary>
    /// Dlg_Start.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Dlg_Start : Window
    {
        static public bool m_bShow = false;
        AOP01_Handler m_handler;
        AOP01_Recipe m_recipe;
        RNR m_aRnR;
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
            //m_bShow = false;
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            m_bShow = false;
        }
        public void Init(AOP01_Handler handler)
        {
            //m_aRnR = new RNR();
            //m_aRecipe = new ObservableCollection<Recipe>();
            m_handler = handler;
            m_recipe = m_handler.m_recipe;
            //listviewRCP.ItemsSource = m_aRecipe;
            //RNRset.DataContext = m_aRnR;
        }
        ManualJobSchedule m_JobSchedule;
        public void Init(ManualJobSchedule jobschdule)
        {
            m_aRnR = new RNR();
            m_aRecipe = new ObservableCollection<Recipe>();
            listviewRCP.ItemsSource = m_aRecipe;
            RNRset.DataContext = m_aRnR;
            m_JobSchedule = jobschdule;
            this.DataContext = jobschdule;
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
        public string sRecipe = "";
        void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {  
            Recipe typeItem = (Recipe)listviewRCP.SelectedItem;
            string sRecipeName = typeItem.p_sRecipeName.ToString();
            sRecipe = m_recipe.m_sPath + sRecipeName;

            ////////////////////////////////////////////////////////


            //m_recipe.m_moduleRunList.OpenJob(sRecipe);
            //m_recipe.m_moduleRunList.RunTree(Tree.eMode.Init);
        }
        #endregion

        #region RnR Property
        public class RNR : NotifyProperty
        {

            bool _bRnR = false;
            public bool p_bRnR
            {
                get { return _bRnR; }
                set
                {
                    _bRnR = value;
                    OnPropertyChanged();
                }
            }

            int _nRnR = 1;
            public int p_nRnR
            {
                get { return _nRnR; }
                set
                {
                    _nRnR = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            InfoWafer infoWafer = m_infoCarrier.GetInfoWafer(0);
            if (infoWafer != null)
            {
                infoWafer.RecipeOpen(sRecipe);
                m_handler.AddSequence(infoWafer);
                m_handler.CalcSequence();
                //m_infoCarrier.StartProcess(infoWafer.p_id);
            }
            this.DialogResult = true;
        }
    }

}
