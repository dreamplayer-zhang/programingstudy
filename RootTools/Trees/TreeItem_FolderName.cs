using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Input;

namespace RootTools.Trees
{
    class TreeItem_FolderName : Tree, ITreeItem
    {
        string _value = "";
        public string p_value
        {
            get { return _value; }
            set
            {
                SetValue(value);
                p_treeRoot.UpdateTreeItems();
            }
        }

        public void SetValue(dynamic value)
        {
            if (_value == value) return;
            if (m_log != null) m_log.Info(p_id + " : " + _value.ToString() + " -> " + value.ToString());
            _value = value;
            OnPropertyChanged("p_value");
        }

        public dynamic GetValue() { return p_value; }

        string m_sExt = "";
        public TreeItem_FolderName(string sName, Tree treeParent, string value, string sDesc, Log log)
        {
            p_sName = sName;
            p_treeParent = treeParent;
            p_treeRoot = treeParent.p_treeRoot;
            p_id = treeParent.p_id + "." + sName;
            p_sDescription = sDesc;
            _value = value;
            m_log = log;
        }

        public ICommand OpenFolderCommand
        {
            get 
            { 
                return new RelayCommand(OpenFolderDialog); 
            }
        }

        public void OpenFolderDialog()
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            if (dlg.ShowDialog() == CommonFileDialogResult.Cancel) return;
            p_value = dlg.FileName;
        }
    }
}
