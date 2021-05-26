using Root_Rinse_Loader.Module;
using RootTools;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace Root_Rinse_Loader.MainUI
{
    /// <summary>
    /// Magazine_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Magazine_UI : UserControl
    {
        public Magazine_UI()
        {
            InitializeComponent();
        }

        RinseL m_rinse; 
        Storage m_storage;
        public void Init(RinseL rinse, Storage storage)
        {
            m_rinse = rinse; 
            m_storage = storage;
            DataContext = storage;
            textBoxMagazineIndex.DataContext = rinse;
            comboBoxMagazine.DataContext = rinse;
            comboBoxMagazine.ItemsSource = Enum.GetValues(typeof(Storage.eMagazine)); 
            InitMagazine(); 
            InitTimer(); 
        }

        List<MagazineClamp_UI> m_aMagazine = new List<MagazineClamp_UI>(); 
        void InitMagazine()
        {
            gridMagazineLevels.Children.Clear();
            gridMagazineLevels.RowDefinitions.Clear();
            foreach (Storage.Magazine magazine in m_storage.m_aMagazine)
            {
                gridMagazineLevels.RowDefinitions.Add(new RowDefinition());
                MagazineClamp_UI ui = new MagazineClamp_UI();
                ui.Init(magazine);
                Grid.SetRow(ui, gridMagazineLevels.Children.Count);
                gridMagazineLevels.Children.Add(ui);
                m_aMagazine.Add(ui); 
            }
        }

        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(400);
            m_timer.Tick += M_timer_Tick; ;
            m_timer.Start();
        }

        bool m_bBlink = false; 
        private void M_timer_Tick(object sender, EventArgs e)
        {
            m_bBlink = !m_bBlink;
            foreach (MagazineClamp_UI ui in m_aMagazine) ui.OnTimer(m_bBlink);
            groupBoxMagazine.IsEnabled = (EQ.p_eState == EQ.eState.Ready);
            buttonLoadUp.IsEnabled = EQ.p_eState == EQ.eState.Ready;
            buttonLoadDown.IsEnabled = EQ.p_eState == EQ.eState.Ready; 
        }

        private void buttonNew_Click(object sender, RoutedEventArgs e)
        {
            m_rinse.p_eMagazine = Storage.eMagazine.Magazine4; 
            m_rinse.p_iMagazine = 0; 
        }

        private void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            DependencyProperty property = TextBox.TextProperty;
            BindingExpression binding = BindingOperations.GetBindingExpression((TextBox)sender, property);
            if (binding != null) binding.UpdateSource();
        }

        private void buttonMove_Click(object sender, RoutedEventArgs e)
        {
            m_storage.MoveMagazine(m_rinse.p_eMagazine, m_rinse.p_iMagazine, false); 
        }

        private void buttonLoadUp_Click(object sender, RoutedEventArgs e)
        {
            buttonLoadUp.IsEnabled = false;
            buttonLoadDown.IsEnabled = false; 
            m_storage.RunLoadUp(); 
        }

        private void buttonLoadDown_Click(object sender, RoutedEventArgs e)
        {
            buttonLoadUp.IsEnabled = false;
            buttonLoadDown.IsEnabled = false;
            m_storage.RunLoadDown();
        }
    }
}
