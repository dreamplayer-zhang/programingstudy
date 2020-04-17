using RootTools.Trees;
using System;
using System.Windows.Controls;

namespace RootTools.DMC
{
    /// <summary>
    /// DMCControl_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DMCControl_UI : UserControl
    {
        public DMCControl_UI()
        {
            InitializeComponent();
        }

        DMCControl m_dmc;
        public void Init(DMCControl dmc)
        {
            m_dmc = dmc;
            this.DataContext = dmc;
            commLogUI.Init(dmc.m_commLog);
            treeRootUI.Init(dmc.m_treeRoot);
            m_dmc.RunTree(Tree.eMode.Init);

            comboTeach.ItemsSource = Enum.GetValues(typeof(DMCControl.eTCRMode));
            comboTeach.SelectedValue = dmc.p_eSetTCRMode; 
            comboJogSpeed.ItemsSource = Enum.GetValues(typeof(DMCControl.eJogSpeed));
            comboJogSpeed.SelectedValue = dmc.p_eSetJogSpeed; 
            foreach (DMCAxis axis in dmc.m_aAxis) stackAxis.Children.Add(axis.p_ui);

            listDI_UI.Init(dmc.m_listDI);
            listDO_UI.Init(dmc.m_listDO); 
        }

        private void checkBoxServoOn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_dmc.p_bSetServo = !m_dmc.p_bGetServo;
            checkBoxServoOn.IsChecked = m_dmc.p_bGetServo; 
        }

        private void comboTeach_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboTeach.SelectedValue == null) return;
            m_dmc.p_eSetTCRMode = (DMCControl.eTCRMode)comboTeach.SelectedValue; 
        }

        private void comboJogSpeed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboJogSpeed.SelectedValue == null) return;
            m_dmc.p_eSetJogSpeed = (DMCControl.eJogSpeed)comboJogSpeed.SelectedValue;
        }
    }
}
