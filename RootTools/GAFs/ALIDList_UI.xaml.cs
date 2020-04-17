using Microsoft.Win32;
using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;

namespace RootTools.GAFs
{
    /// <summary>
    /// ALIDList_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ALIDList_UI : UserControl
    {
        public ALIDList_UI()
        {
            InitializeComponent();
        }

        ALIDList m_listALID;
        public void Init(ALIDList listALID)
        {
            m_listALID = listALID;
            this.DataContext = listALID;
            treeUI.Init(listALID.m_treeRoot);
            listALID.RunTree(Tree.eMode.Init);
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
            m_listALID.SaveFile(dlg.FileName);
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            m_listALID.ClearALID();
        }
    }
}
