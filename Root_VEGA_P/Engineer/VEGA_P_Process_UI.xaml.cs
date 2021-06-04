using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Root_VEGA_P.Engineer
{
    /// <summary>
    /// VEGA_P_Process_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class VEGA_P_Process_UI : UserControl
    {
        public VEGA_P_Process_UI()
        {
            InitializeComponent();
        }

        VEGA_P_Handler m_handler;
        VEGA_P_Process m_process; 
        public void Init(VEGA_P_Handler handler)
        {
            m_handler = handler; 
            m_process = handler.m_process;
            DataContext = m_process;

            labelEQState.DataContext = EQ.m_EQ;
            checkBoxStop.DataContext = EQ.m_EQ;
            checkBoxPause.DataContext = EQ.m_EQ;
            checkBoxSimulate.DataContext = EQ.m_EQ;

            treeRootUI.Init(m_process.m_treeRoot);
            m_process.RunTree(Tree.eMode.Init);

            InitModule(); 

            buttonSetRecover.IsEnabled = false;
            InitTimer();
        }

        void InitModule()
        {
            Process_Module_UI ui = InitModule(m_handler.m_loadport, 0, 300);
            ui.AddInfoPod(InfoPod.ePod.EOP_Dome, m_handler.m_loadport);
            ui.AddInfoPod(InfoPod.ePod.EIP_Cover, m_handler.m_loadport);
            ui.AddInfoPod(InfoPod.ePod.EIP_Plate, m_handler.m_loadport);
            ui.AddInfoPod(InfoPod.ePod.EOP_Door, m_handler.m_loadport);
            ui = InitModule(m_handler.m_rtr, 0, 0);
            ui.AddInfoPod(InfoPod.ePod.EOP_Dome, m_handler.m_rtr);
            ui.AddInfoPod(InfoPod.ePod.EIP_Cover, m_handler.m_rtr);
            ui.AddInfoPod(InfoPod.ePod.EIP_Plate, m_handler.m_rtr);
            ui.AddInfoPod(InfoPod.ePod.EOP_Door, m_handler.m_rtr);
            ui = InitModule(m_handler.m_EIP_Cover, -400, -20);
            ui.AddInfoPod(InfoPod.ePod.EIP_Cover, m_handler.m_EIP_Cover);
            ui = InitModule(m_handler.m_EIP_Plate, -400, 170);
            ui.AddInfoPod(InfoPod.ePod.EIP_Plate, m_handler.m_EIP_Plate);
            ui = InitModule(m_handler.m_EOP, -400, 360);
            ui.AddInfoPod(InfoPod.ePod.EOP_Dome, m_handler.m_EOP.m_dome);
            ui.AddInfoPod(InfoPod.ePod.EOP_Door, m_handler.m_EOP.m_door);
            ui = InitModule(m_handler.m_holder, 400, 20);
            ui.AddInfoPod(InfoPod.ePod.EIP_Cover, m_handler.m_holder);
            ui.AddInfoPod(InfoPod.ePod.EIP_Plate, m_handler.m_holder);
            ui = InitModule(m_handler.m_vision, 400, 250);
            ui.AddInfoPod(InfoPod.ePod.EIP_Cover, m_handler.m_vision);
            ui.AddInfoPod(InfoPod.ePod.EIP_Plate, m_handler.m_vision);

        }

        Process_Module_UI InitModule(ModuleBase module, int px, int py)
        {
            Process_Module_UI ui = new Process_Module_UI();
            ui.Init(module);
            ui.Margin = new Thickness(px, py, 0, 0);
            gridDrawing.Children.Add(ui);
            return ui; 
        }
        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();

        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromSeconds(0.2);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            m_process.OnTimer();
            bool bEQReady = (EQ.p_eState == EQ.eState.Ready);
            buttonClear.IsEnabled = bEQReady && (m_process.p_nQueue > 0);
            buttonSetRecover.IsEnabled = bEQReady && m_process.IsEnableRecover(); 
            buttonRecipeOpen.IsEnabled = bEQReady && !m_process.IsEnableRecover();
            buttonRunStep.IsEnabled = bEQReady && (m_process.p_nQueue > 0);
            buttonRun.IsEnabled = bEQReady && (m_process.p_nQueue > 0);
        }
        #endregion

        #region Recipe
        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            m_process.Clear(); 
        }

        private void buttonSetRecover_Click(object sender, RoutedEventArgs e)
        {
            m_process.CalcRecover(); 
        }

        private void buttonRecipeOpen_Click(object sender, RoutedEventArgs e)
        {
            m_process.RecipeOpen(); 
        }
        #endregion

        #region Run
        private void buttonRunStep_Click(object sender, RoutedEventArgs e)
        {
            m_process.StartRunStep(); 
        }

        private void buttonRun_Click(object sender, RoutedEventArgs e)
        {
            m_process.StartRunProcess(); 
        }

        private void checkBoxRnR_Click(object sender, RoutedEventArgs e)
        {
            m_process.StartRunRnR(checkBoxRnR.IsChecked == true);
        }
        #endregion

    }
}
