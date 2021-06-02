using Root_Pine2.Module;
using Root_Pine2_Vision.Module;
using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Root_Pine2.Engineer
{
    /// <summary>
    /// Pine2_Process_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Pine2_Process_UI : UserControl
    {
        public Pine2_Process_UI()
        {
            InitializeComponent();
        }

        Pine2_Handler m_handler; 
        public void Init(Pine2_Handler handler)
        {
            m_handler = handler;

            labelEQState.DataContext = EQ.m_EQ;
            checkBoxStop.DataContext = EQ.m_EQ;
            checkBoxPause.DataContext = EQ.m_EQ;
            checkBoxSimulate.DataContext = EQ.m_EQ;

            InitMagazineEV_UI();
            InitLoaderUI(handler.m_loader0, gridLoader, 6);
            InitLoaderUI(handler.m_loader1, gridBoat, 6);
            InitLoaderUI(handler.m_loader2, gridBoat, 0);
            InitLoaderUI(handler.m_loader3, gridLoader, 0);
            InitBoatsUI();
            InitTransferUI(); 
        }

        List<MagazineEV_UI> m_aMagazineUI = new List<MagazineEV_UI>(); 
        void InitMagazineEV_UI()
        {
            MagazineEVSet set = m_handler.m_magazineEV;
            foreach (InfoStrip.eMagazine eMagazine in Enum.GetValues(typeof(InfoStrip.eMagazine)))
            {
                MagazineEV_UI ui = new MagazineEV_UI();
                ui.Init(set.m_aEV[eMagazine]);
                Grid.SetColumn(ui, (int)eMagazine); 
                gridMagazineEV.Children.Add(ui);
                m_aMagazineUI.Add(ui);

                m_timer.Tick += M_timer_Tick;
                m_timer.Interval = TimeSpan.FromSeconds(0.2);
                m_timer.Start(); 
            }
        }

        List<Loader_UI> m_aLoaderUI = new List<Loader_UI>();
        void InitLoaderUI(dynamic loader, Grid grid, int nColumn)
        {
            Loader_UI ui = new Loader_UI();
            ui.Init(loader);
            Grid.SetColumn(ui, nColumn);
            grid.Children.Add(ui);
            m_aLoaderUI.Add(ui); 
        }

        List<Boats_UI> m_aBoatsUI = new List<Boats_UI>(); 
        void InitBoatsUI()
        {
            foreach (Vision.eVision eVision in Enum.GetValues(typeof(Vision.eVision)))
            {
                Boats_UI ui = new Boats_UI();
                ui.Init(m_handler.m_aBoats[eVision]);
                Grid.SetColumn(ui, 4 - (int)eVision);
                gridBoat.Children.Add(ui);
                m_aBoatsUI.Add(ui); 
            }
        }

        Transfer_UI m_transferUI; 
        void InitTransferUI()
        {
            Transfer_UI ui = new Transfer_UI();
            ui.Init(m_handler.m_transfer);
            Grid.SetColumn(ui, 3);
            gridLoader.Children.Add(ui);
            m_transferUI = ui; 
        }

        DispatcherTimer m_timer = new DispatcherTimer();
        private void M_timer_Tick(object sender, EventArgs e)
        {
            foreach (MagazineEV_UI ui in m_aMagazineUI) ui.OnTimer(); 
            foreach (Loader_UI ui in m_aLoaderUI) ui.OnTimer();
            foreach (Boats_UI ui in m_aBoatsUI) ui.OnTimer();
            m_transferUI.OnTimer(); 
        }
    }
}
