using Microsoft.Win32;
using Root_ASIS.Module;
using RootTools.Trees;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Root_ASIS
{
    /// <summary>
    /// Teach_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainTeach_UI : UserControl
    {
        public MainTeach_UI()
        {
            InitializeComponent();
        }

        ASIS_Engineer m_engineer = null;
        ASIS_Handler m_handler;
        MainTeach m_teach; 
        public void Init(MainTeach teach, ASIS_Engineer engineer)
        {
            m_engineer = engineer; 
            m_handler = (ASIS_Handler)engineer.ClassHandler();
            DataContext = teach; 
            m_teach = teach; 
            teach0UI.Init(m_handler.m_aBoat[Boat.eBoat.Boat0].m_teach);
            teach1UI.Init(m_handler.m_aBoat[Boat.eBoat.Boat1].m_teach);
            treeUI.Init(m_teach.m_treeRoot);
            m_teach.RunTree(Tree.eMode.Init);
            InitTimer(); 
        }

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer(); 
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromSeconds(0.1);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start(); 
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            buttonOpen.IsEnabled = !m_teach.m_bgwRecipeSave.IsBusy; 
            buttonSave.IsEnabled = !m_teach.m_bgwRecipeSave.IsBusy;
        }
        #endregion

        #region File
        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = m_teach.m_sEXT;
            dlg.Filter = "Open Recipe File (*" + m_teach.m_sEXT + ")|*" + m_teach.m_sEXT;
            if (dlg.ShowDialog() == false) return;
            buttonOpen.IsEnabled = false; 
            m_teach.OpenRecipe(dlg.FileName);
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = m_teach.m_sEXT;
            dlg.Filter = "Save Recipe File (*" + m_teach.m_sEXT + ")|*" + m_teach.m_sEXT;
            if (dlg.ShowDialog() == false) return;
            buttonSave.IsEnabled = false; 
            m_teach.SaveRecipe(dlg.FileName); 
        }
        #endregion
    }
}
