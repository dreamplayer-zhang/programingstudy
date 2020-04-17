using Microsoft.Win32;
using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;

namespace RootTools.GAFs
{
    /// <summary>
    /// SVIDList_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SVIDList_UI : UserControl
    {
        public SVIDList_UI()
        {
            InitializeComponent();
        }

        SVIDList m_listSVID;
        public void Init(SVIDList listSVID)
        {
            m_listSVID = listSVID;
            this.DataContext = listSVID;
            treeUI.Init(listSVID.m_treeRoot);
            listSVID.RunTree(Tree.eMode.Init);
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
            m_listSVID.SaveFile(dlg.FileName);
        }
    }
}
