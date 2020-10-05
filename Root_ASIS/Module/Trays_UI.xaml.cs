using RootTools;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_ASIS.Module
{
    /// <summary>
    /// Trays_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Trays_UI : UserControl
    {
        public Trays_UI()
        {
            InitializeComponent();
        }

        Trays m_trays; 
        public void Init(Trays trays)
        {
            m_trays = trays;
            DataContext = trays;
            InitTimer(); 
        }

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer(); 
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromSeconds(0.3);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start(); 
        }

        bool m_bBlink = false; 
        private void M_timer_Tick(object sender, EventArgs e)
        {
            buttonOpen.Foreground = m_bBlink && m_trays.p_bOpen ? Brushes.Red : Brushes.Green; 
            buttonFull.Foreground = m_bBlink && m_trays.p_bFull ? Brushes.Red : Brushes.Green;
            InitTraysUI();
            TimerTray(); 
            m_bBlink = !m_bBlink;
        }
        #endregion

        #region Tray
        CPoint m_szTray = new CPoint(0, 0);
        void InitTraysUI()
        {
            if (m_szTray == m_trays.p_szTray) return;
            m_szTray = m_trays.p_szTray;
            gridTray.Children.Clear();
            gridTray.ColumnDefinitions.Clear();
            for (int y = 0; y < m_szTray.Y; y++) gridTray.RowDefinitions.Add(new RowDefinition());
            for (int x = 0; x < m_szTray.X; x++) gridTray.ColumnDefinitions.Add(new ColumnDefinition());
            int nTray = 0;
            for (int y = 0; y < m_szTray.Y; y++)
            {
                for (int x = 0; x < m_szTray.X; x++, nTray++)
                {
                    Trays_Tray_UI ui = new Trays_Tray_UI();
                    ui.Init(m_trays.m_aTray[nTray]);
                    Grid.SetColumn(ui, x);
                    Grid.SetRow(ui, m_szTray.Y - y - 1);
                    gridTray.Children.Add(ui);
                }
            }
        }

        void TimerTray()
        {
            foreach (Trays_Tray_UI ui in gridTray.Children)
            {
                if (ui.m_tray.p_bProduct) ui.Background = m_bBlink && (ui.m_tray.p_nCount == 0) ? Brushes.Red : Brushes.LightGreen;
                else ui.Background = m_bBlink && (ui.m_tray.p_nCount > 0) ? Brushes.Purple : Brushes.AliceBlue; 
            }
        }
        #endregion

        private void buttonClear_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_trays.ClearTray(); 
        }
    }
}
