using Root_Pine2.Module;
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
            InitMagazineEV_UI();
            InitLoaderUI(handler.m_loader0, gridLoader, 7);
            InitLoaderUI(handler.m_loader1, gridBoat, 7);
            InitLoaderUI(handler.m_loader2, gridBoat, 0);
            InitLoaderUI(handler.m_loader3, gridLoader, 0);
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
    }
}
