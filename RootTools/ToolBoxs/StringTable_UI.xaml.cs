using Microsoft.Win32;
using RootTools.Trees;
using System.Windows.Controls;

namespace RootTools.ToolBoxs
{
    /// <summary>
    /// StringTable_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class StringTable_UI : UserControl
    {
        public StringTable_UI()
        {
            InitializeComponent();
        }

        _StringTable m_stringTable;
        public void Init(_StringTable stringTable)
        {
            m_stringTable = stringTable;
            this.DataContext = stringTable;
            treeUI.Init(stringTable.m_treeRoot);
            stringTable.RunTree(Tree.eMode.Init); 
        }

        private void Open_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".StringTable";
            dlg.Filter = "String Table (.StringTable)|*.StringTable";
            if (dlg.ShowDialog() == true) m_stringTable.FileOpen(dlg.FileName); 
        }

        private void Save_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = ".StringTable";
            dlg.Filter = "String Table (.StringTable)|*.StringTable";
            if (dlg.ShowDialog() == true) m_stringTable.FileSave(dlg.FileName);
        }
    }
}
