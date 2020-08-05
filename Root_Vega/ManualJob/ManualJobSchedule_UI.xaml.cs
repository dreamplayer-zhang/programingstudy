using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace Root_Vega.ManualJob
{
    /// <summary>
    /// ManualJobSchedule.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ManualJobSchedule_UI : Window
    {
        static public bool m_bShow = false;
        DispatcherTimer m_UIBackTimer = new DispatcherTimer();
        Color UIBackColorT = Color.FromArgb(255, 67, 67, 122);
        Brush UIBackBrushT;
        Color UIBackColorF = Color.FromArgb(255, 45, 45, 48);
        Brush UIBackBrushF;
        public ManualJobSchedule_UI()
        {
            m_bShow = true;
            InitializeComponent();

            UIBackBrushT = new SolidColorBrush(UIBackColorT);
            UIBackBrushF = new SolidColorBrush(UIBackColorF);

            m_UIBackTimer.Interval = TimeSpan.FromMilliseconds(150);
            m_UIBackTimer.Tick += m_UIBackTimer_Tick;
            m_UIBackTimer.Start();
        }

        int m_iTimer = 0; 
        private void m_UIBackTimer_Tick(object sender, EventArgs e)
        {
            buttonRun.Visibility = (comboRecipeID.SelectedValue == null) ? Visibility.Hidden : Visibility.Visible; 
            if (m_iTimer >= 5) gridMain.Background = UIBackBrushT;
            else gridMain.Background = UIBackBrushF;
            m_iTimer = (m_iTimer + 1) % 10; 
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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            m_bShow = false;
        }

        #region Recipe
        void InitRecipeList()
        {
            DirectoryInfo info = new DirectoryInfo("c:\\Vega\\Recipe");
            FileInfo[] files = info.GetFiles("*.Vega");
            List<string> asRecipeFile = new List<string>(); 
            foreach (FileInfo fileInfo in files) asRecipeFile.Add(fileInfo.FullName);
            comboRecipeID.ItemsSource = asRecipeFile;
        }
        private void comboRecipeID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string sRecipe = (string)comboRecipeID.SelectedValue;
            if (sRecipe == null) return;
            if (m_JobSchedule.m_loadport.m_infoPod.p_infoReticle == null) return; 
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
