using RootTools;
using System;
using System.Collections.Generic;
using System.IO;
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

        Explorer_ViewModel m_summary = new Explorer_ViewModel(InitPath : @"C:\Camellia2\Summary");
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

        Explorer_ViewModel m_history = new Explorer_ViewModel(InitPath: @"C:\Camellia2\History");
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

            m_summary.DoubleClicked += DoubleClick;
            m_history.DoubleClicked += DoubleClick;

            InitExplorer();
        }

        string m_currentPath = @"C:\Camellia2\Summary\";
        public string p_currentPath
        {
            get
            {
                return m_currentPath;
            }
            set
            {
                SetProperty(ref m_currentPath, value);
            }
        }
        public void DoubleClick(object sender, EventArgs e)
        {
            string path = (string)sender;
            if(p_currentPath == path)
            {
                return;
            }
            p_currentPath = path;
            ReadCSV(path);
        }

        private void ReadCSV(string path)
        {
            try
            {
                var reader = new StreamReader(File.OpenRead(path));
                List<string> listA = new List<string>();
                List<string> listB = new List<string>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    listA.Add(values[0]);
                    listB.Add(values[1]);
                    
                }
                foreach (var coloumn1 in listA)
                {
                    Console.WriteLine(coloumn1);
                }
                foreach (var coloumn2 in listB)
                {
                    Console.WriteLine(coloumn2);
                }
            }
            catch (Exception)
            {

            }
            
        }

        int m_tabIdx = 0;
        public int p_tabIdx
        {
            get
            {
                return m_tabIdx;
            }
            set
            {
                SetProperty(ref m_tabIdx, value);
            }
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

        public ICommand CmdRefresh
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if(p_tabIdx == 0)
                    {
                        p_summary.RebuildTree(pIncludeFileChildren : true, InitPath: @"C:\Camellia2\Summary");
                    }
                    else
                    {
                        p_history.RebuildTree(pIncludeFileChildren: true, InitPath: @"C:\Camellia2\History");
                    }

                });
            }
        }


        #endregion


        //public override void OnMouseDoubleClick(object sender, System.Windows.Input.MouseEventArgs e)
        //{
        //    int test = 30;
        //}
    }
}
