using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Root_CAMELLIA
{
    public class Dlg_Review_ViewModel : ObservableObject, IDialogRequestClose
    {
        private MainWindow_ViewModel mainWindow_ViewModel;

        Explorer_ViewModel m_summary = new Explorer_ViewModel(@"C:\Camellia2\Summary\");
        public Explorer_ViewModel p_summary
        {
            get
            {
                return m_summary;
            }
            set
            {
                SetProperty(ref m_summary, value);
            }
        }

        Explorer_ViewModel m_history = new Explorer_ViewModel(@"C:\Camellia2\History\");
        public Explorer_ViewModel p_history
        {
            get
            {
                return m_history;
            }
            set
            {
                SetProperty(ref m_history, value);
            }
        }


        public Dlg_Review_ViewModel(MainWindow_ViewModel mainWindow_ViewModel)
        {
            this.mainWindow_ViewModel = mainWindow_ViewModel;

            

            InitExplorer();
        }

        private void InitExplorer()
        {
            //p_explorer.p_treeView = new System.Windows.Controls.TreeView();
            //m_explorer.p_treeView.SelectedItemChanged += ExplorerItemDoubleClick;
        }

        public void ExplorerItemDoubleClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Double click");
        }

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        #region Command
        public ICommand CmdClose
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CloseRequested(this, new DialogCloseRequestedEventArgs(true));
                });
            }
        }
        #endregion
    }
}
