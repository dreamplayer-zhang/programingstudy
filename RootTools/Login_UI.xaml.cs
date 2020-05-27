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
            m_login.Logout();
            Resize(); 
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            m_login.CheckLogin(passwordBox.Password);
            Resize();
            m_login.RunTree(Tree.eMode.Init);
        }
        #endregion

        Login m_login; 
        public void Init(Login login)
        {
            m_login = login; 
            DataContext = login;
            treeRootUI.Init(login.m_treeRoot);
            login.RunTree(Tree.eMode.Init); 
            Resize(); 
        }

        #region Resize
        void Resize()
        {
            bool bLogout = (m_login.p_eLevel == Login.eLevel.Logout);
            comboBoxName.Visibility = (bLogout && m_login.m_bCheckUserName) ? Visibility.Visible : Visibility.Hidden;
            passwordBox.Visibility = bLogout ? Visibility.Visible : Visibility.Hidden;
            stackAddUser.Visibility = (m_login.p_eLevel >= Login.eLevel.Admin) ? Visibility.Visible : Visibility.Collapsed;
            treeRootUI.Visibility = (m_login.p_eLevel >= Login.eLevel.Admin) ? Visibility.Visible : Visibility.Collapsed;
            if (m_login.p_eLevel != Login.eLevel.Logout) passwordBox.Password = ""; 
        }
        #endregion

        private void buttonAddUser_Click(object sender, RoutedEventArgs e)
        {
            m_login.AddUser(textBoxAddUser.Text); 
        }

        private void buttonDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            m_login.DeleteUser(textBoxAddUser.Text);
        }
    }
}
