using System.Windows; 
using System.Windows.Controls;
using System.Windows.Input;

namespace RootTools
{
    /// <summary>
    /// LogIn_MainUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LogIn_MainUI : UserControl
    {
        public LogIn_MainUI()
        {
            InitializeComponent();
        }

        Login m_login;
        public void Init(Login login)
        {
            m_login = login;
            DataContext = login;
            InvalidUI(); 
        }

        void InvalidUI()
        {
            bool bLogout = (m_login.p_user.m_eLevel == Login.eLevel.Logout);
            gridUser.Visibility = bLogout ? Visibility.Hidden : Visibility.Visible;
            bool bUseID = m_login.m_bCheckUserName;
            gridPassword.Visibility = bLogout && !bUseID ? Visibility.Visible : Visibility.Hidden;
            gridLogin.Visibility = bLogout && bUseID ? Visibility.Visible : Visibility.Hidden;
        }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (m_login.p_eLevel == Login.eLevel.Logout) return;
            m_login.Logout();
            InvalidUI();
        }

        private void passwordBoxPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            m_login.CheckLogin(passwordBoxPassword.Password);
            InvalidUI();
        }

        private void passwordBoxLogin_PasswordChanged(object sender, RoutedEventArgs e)
        {
            m_login.CheckLogin(passwordBoxLogin.Password);
            InvalidUI();
        }
    }
}
