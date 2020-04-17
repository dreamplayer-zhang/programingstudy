using Microsoft.Win32;
using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;

namespace RootTools.GAFs
{
    /// <summary>
    /// CEIDList_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CEIDList_UI : UserControl
    {
        public CEIDList_UI()
        {
            InitializeComponent();
        }

        CEIDList m_listCEID;
        public void Init(CEIDList listCEID)
        {
            m_listCEID = listCEID;
            this.DataContext = listCEID;
            treeUI.Init(listCEID.m_treeRoot);
            m_listCEID.RunTree(Tree.eMode.Init);
        }

        private void CheckBoxSetup_Click(object sender, RoutedEventArgs e)
        {
            treeUI.Visibility = (checkBoxSetup.IsChecked == true) ? Visibility.Visible : Visibility.Hidden;
            dataGrid.Visibility = (checkBoxSetup.IsChecked == false) ? Visibility.Visible : Visibility.Hidden;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = ".csv";
            dlg.Filter = "Save File  (.csv)|*.csv";
            if (dlg.ShowDialog() == false) return;
            m_listCEID.SaveFile(dlg.FileName);
        }
    }
}
