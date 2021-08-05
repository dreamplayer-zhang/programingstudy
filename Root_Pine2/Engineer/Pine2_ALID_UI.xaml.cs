using RootTools;
using RootTools.GAFs;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_Pine2.Engineer
{
    /// <summary>
    /// Pine2_ALID.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Pine2_ALID_UI : UserControl
    {
        public Pine2_ALID_UI()
        {
            InitializeComponent();
        }

        ALIDList m_listALID;
        IEngineer m_engineer;
        GAF m_gaf;
        public void Init(ALIDList listALID, IEngineer engineer)
        {
            m_listALID = listALID;
            m_engineer = engineer;
            m_gaf = engineer.ClassGAF(); 
            DataContext = listALID;
            listViewALID.ItemsSource = listALID.p_aSetALID;

            m_timer.Interval = TimeSpan.FromSeconds(0.3);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        DispatcherTimer m_timer = new DispatcherTimer();
        private void M_timer_Tick(object sender, EventArgs e)
        {
            grid.Background = (m_gaf.m_listALID.p_aSetALID.Count > 0) ? Brushes.OrangeRed : Brushes.LightGreen;
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            m_listALID.ClearALID();
            if (EQ.p_eState == EQ.eState.Error) EQ.p_eState = (EQ.m_EQ.m_eStateOld != EQ.eState.Home) ? EQ.m_EQ.m_eStateOld : EQ.eState.Init;
            EQ.p_bStop = false; 
        }

    }
}
