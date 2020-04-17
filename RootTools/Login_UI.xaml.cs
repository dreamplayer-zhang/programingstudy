using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RootTools
{
    /// <summary>
    /// Login_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Login_UI : UserControl
    {
        public Login_UI()
        {
            InitializeComponent();
        }

        #region UI Function
        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (m_login.p_eLevel == Login.eLevel.Logout) return;
            if (m_bEdit) m_bEdit = false;
            m_login.Logout();
            Resize(); 
        }

        bool m_bEdit = false; 
        private void Label_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (m_login.p_eLevel != Login.eLevel.Admin) return;
            m_bEdit = !m_bEdit;
            Resize();
            m_login.RunTree(Tree.eMode.Init); 
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            m_login.CheckLogin(passwordBox.Password);
            Resize(); 
        }
        #endregion

        Login m_login; 
        public void Init(Login login)
        {
            m_login = login; 
            this.DataContext = login;
            treeRootUI.Init(login.m_treeRoot);
            login.RunTree(Tree.eMode.Init); 
            Resize(); 
        }

        #region Resize
        void Resize()
        {
            bool bLogout = (m_login.p_eLevel == Login.eLevel.Logout);
            treeRootUI.Height = m_bEdit ? 400 : 0;
            comboBoxName.Width = (bLogout && m_login.m_bCheckUserName) ? 80 : 0;
            passwordBox.Width = bLogout ? 80 : 0;
            if (m_login.p_eLevel != Login.eLevel.Logout) passwordBox.Password = ""; 
        }
        #endregion
    }
}
