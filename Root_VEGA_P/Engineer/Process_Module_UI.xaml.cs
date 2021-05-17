using Root_VEGA_P.Module;
using Root_VEGA_P_Vision.Module;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Root_VEGA_P.Engineer
{
    /// <summary>
    /// Process_Module_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Process_Module_UI : UserControl
    {
        public Process_Module_UI()
        {
            InitializeComponent();
        }

        ModuleBase m_module; 
        public void Init(ModuleBase module)
        {
            m_module = module;
            textBlockHeader.Text = module.p_id;
            treeRootUI.Init(module.m_treeRootQueue);
            module.RunTreeQueue(Tree.eMode.Init);
            InitTimer(); 
        }

        List<Process_InfoPod_UI> m_aInfoPod = new List<Process_InfoPod_UI>(); 
        public void AddInfoPod(InfoPod.ePod ePod, IRTRChild child)
        {
            Process_InfoPod_UI ui = new Process_InfoPod_UI();
            ui.Init(ePod, child);
            m_aInfoPod.Add(ui);
            stackPanelInfoPod.Children.Add(ui); 
        }

        public void AddInfoPod(InfoPod.ePod ePod, RTR rtr)
        {
            Process_InfoPod_UI ui = new Process_InfoPod_UI();
            ui.Init(ePod, rtr);
            m_aInfoPod.Add(ui);
            stackPanelInfoPod.Children.Add(ui);
        }

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromSeconds(0.2);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        int[] m_nQueue = new int[2] { 0, 0 }; 
        private void M_timer_Tick(object sender, EventArgs e)
        {
            foreach (Process_InfoPod_UI ui in m_aInfoPod) ui.OnTimer(); 
            if ((m_module.m_qModuleRun.Count == m_nQueue[0]) && (m_module.m_qModuleRemote.Count == m_nQueue[1])) return;
            m_nQueue[0] = m_module.m_qModuleRun.Count;
            m_nQueue[1] = m_module.m_qModuleRemote.Count;
            m_module.RunTreeQueue(Tree.eMode.Init); 
        }
        #endregion
    }
}
