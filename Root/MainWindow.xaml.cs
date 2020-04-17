using System.ComponentModel;
using System.Windows;
using RootTools.Trees;

namespace Root
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Window Event
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init(); 
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadStop();
        }
        #endregion

        #region TreeDemo
        TreeDemo m_treeDemo = new TreeDemo(); 
        void InitTreeDemo()
        {
            m_treeDemo.Init("TreeDemo", m_engineer.ClassLogView());
            treeRootUI.Init(m_treeDemo.m_treeRoot);
            m_treeDemo.RunTree(Tree.eMode.Init); 
        }
        #endregion

        Root_Engineer m_engineer = new Root_Engineer(); 
        void Init()
        {
            m_engineer.Init("Root");
            engineerUI.Init(m_engineer);
            InitTreeDemo();
        }

        void ThreadStop()
        {
            m_engineer.ThreadStop(); 
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            m_treeDemo.OpenTree(); 
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            m_treeDemo.SaveTree(); 
        }
    }
}
