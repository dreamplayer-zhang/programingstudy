using Microsoft.Win32;
using RootTools.Trees;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace RootTools.Module
{
    /// <summary>
    /// ModuleList_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ModuleList_UI : UserControl
    {
        public ModuleList_UI()
        {
            InitializeComponent();
        }

        #region Run
        private void buttonHome_Click(object sender, RoutedEventArgs e)
        {
            m_moduleList.p_visibleRnR = Visibility.Hidden;
            m_moduleList.p_iRun = 0;
            foreach (ModuleRunBase moduleRun in m_moduleList.p_moduleList)
            {
                m_moduleList.p_Percent = "0";
                moduleRun.p_eRunState = ModuleRunBase.eRunState.Ready;
            }
            
            EQ.p_eState = EQ.eState.Home;
        }

        private void ButtonRun_Click(object sender, RoutedEventArgs e)
        {
            ModuleListRun();
        }

        public void ModuleListRun()
        {
            foreach (ModuleRunBase moduleRun in m_moduleList.p_moduleList)
            {
                m_moduleList.p_Percent = "0";
                m_moduleList.p_nTotalRnR = 0;
                moduleRun.p_nProgress = 0;
                moduleRun.p_eRunState = ModuleRunBase.eRunState.Ready;
            }
            m_moduleList.p_visibleRnR = Visibility.Hidden;
            m_moduleList.p_sInfo = m_moduleList.ClickRun();
            m_moduleList.p_sNowProgress = "Module Run";
        }

        private void ButtonRunStep_Click(object sender, RoutedEventArgs e)
        {
            m_moduleList.p_visibleRnR = Visibility.Hidden;
            m_moduleList.p_sInfo = m_moduleList.ClickRunStep();
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            EQ.p_bPause = !EQ.p_bPause;
        }

        List<string> m_asModuleRun = new List<string>();
        void InitModuleRunNames()
        {
            m_asModuleRun.Clear();
            comboBoxRunStep.ItemsSource = null;
            foreach (ModuleRunBase moduleRun in m_moduleRunList.p_aModuleRun)
            {
                m_asModuleRun.Add(moduleRun.p_id);
            }
            if (m_asModuleRun.Count > 0) comboBoxRunStep.ItemsSource = m_asModuleRun;
            m_moduleRunList.RunTree(Tree.eMode.Init);
        }

        private void ComboBoxRunStep_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxRunStep.SelectedValue == null) return; 
            m_moduleList.p_sRunStep = comboBoxRunStep.SelectedValue.ToString();
        }

        private void buttonRnR_Click(object sender, RoutedEventArgs e)
        {
            m_moduleList.p_sInfo = m_moduleList.ClickRunRnR(); 
        }
        #endregion

        #region File
        string m_sPath = "c:\\Recipe\\";
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            string sModel = "Run" + EQ.m_sModel;
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.InitialDirectory = m_sPath;
            dlg.DefaultExt = "." + sModel;
            dlg.Filter = sModel + " ModuleRunList (." + sModel + ")|*." + sModel;
            if (dlg.ShowDialog() == true) m_moduleRunList.SaveJob(dlg.FileName);
        }

        public void ModuleListRunOpen()
        {
            m_moduleList.p_visibleRnR = Visibility.Hidden;
            string sModel = "Run" + EQ.m_sModel;
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = m_sPath;
            dlg.DefaultExt = "." + sModel;
            dlg.Filter = sModel + " ModuleRunList (." + sModel + ")|*." + sModel;
            if (dlg.ShowDialog() == true) m_moduleRunList.OpenJob(dlg.FileName);
            InitModuleRunNames();
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            ModuleListRunOpen();
        }
        #endregion

        #region ModuleRun
        string m_sModule = "";
        private void ComboBoxModule_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            buttonAdd.Visibility = Visibility.Hidden;
            comboBoxModuleRun.ItemsSource = null;
            if (comboBoxModule.SelectedValue == null) return;
            m_sModule = comboBoxModule.SelectedValue.ToString();
            comboBoxModuleRun.ItemsSource = m_moduleRunList.GetModuleRunNames(m_sModule);
        }

        string m_sModuleRun = "";
        private void ComboBoxModuleRun_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            buttonAdd.Visibility = Visibility.Hidden;
            if (comboBoxModuleRun.SelectedValue == null) return;
            m_sModuleRun = comboBoxModuleRun.SelectedValue.ToString();
            buttonAdd.Visibility = Visibility.Visible;
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            m_moduleList.p_visibleRnR = Visibility.Hidden;
            m_moduleRunList.Clear();
            InitModuleRunNames();
        }

        private void buttonUndo_Click(object sender, RoutedEventArgs e)
        {
            m_moduleList.p_visibleRnR = Visibility.Hidden;
            m_moduleRunList.Undo();
            InitModuleRunNames();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            m_moduleList.p_visibleRnR = Visibility.Hidden;
            m_moduleRunList.Add(m_sModule, m_sModuleRun);
            InitModuleRunNames();
        }
        #endregion

        ModuleList m_moduleList;
        ModuleRunList m_moduleRunList;
        public void Init(ModuleList moduleList)
        {
            if (moduleList == null) return; 
            m_moduleList = moduleList;
            m_moduleRunList = moduleList.m_moduleRunList;
            this.DataContext = moduleList;

            labelEQState.DataContext = EQ.m_EQ;
            checkBoxStop.DataContext = EQ.m_EQ;
            checkBoxPause.DataContext = EQ.m_EQ;
            checkBoxSimulate.DataContext = EQ.m_EQ;

            infoListUI.Init(moduleList.m_infoList);
            comboBoxModule.ItemsSource = moduleList.m_asModule;
            treeRootUI.Init(m_moduleRunList.m_treeRoot);
            m_moduleRunList.RunTree(Tree.eMode.Init);
        }
    }
}
