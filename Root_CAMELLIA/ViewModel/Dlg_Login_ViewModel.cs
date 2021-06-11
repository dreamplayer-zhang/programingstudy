using Microsoft.Xaml.Behaviors;
using RootTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_CAMELLIA
{
    public class Dlg_Login_ViewModel : ObservableObject, IDialogRequestClose
    {
     
        public Dlg_Login_ViewModel(MainWindow_ViewModel main)
        {

        }

        #region Property

        string m_username = "";
        public string p_username
        {
            get
            {
                return m_username;
            }
            set
            {
                SetProperty(ref m_username, value);
            }
        }
        SecureString m_password;
        public SecureString p_password
        {
            get
            {
                return m_password;
            }
            set
            {
                SetProperty(ref m_password, value);
            }
        }
        #endregion

        #region Event
        public void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseRequested(this, new DialogCloseRequestedEventArgs(false));
            }
        }
        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if (Login())
                {
                    CloseRequested(this, new DialogCloseRequestedEventArgs(true));
                }
            }
            //else if(e.Key == Key.Escape)
            //{
            //    CloseRequested(this, new DialogCloseRequestedEventArgs(false));
            //}
        }
        #endregion

        #region Function
        public bool Login()
        {
            if (p_password == null)
            {
                CustomMessageBox.Show("Error", "Check ID or Password", MessageBoxButton.OK, CustomMessageBox.MessageBoxImage.Error );
                //MessageBox.Show("Check ID / Password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false ;
            }

            IntPtr pStr = Marshal.SecureStringToCoTaskMemUnicode(p_password);
            //Debug.WriteLine(Marshal.PtrToStringUni(pStr));
            string pass = Marshal.PtrToStringUni(pStr);
            if(App.m_engineer.m_login.CheckLogin(p_username, pass))
            {
                return true;
            }
            else
            {
                CustomMessageBox.Show("Error", "Check ID / Password", MessageBoxButton.OK, CustomMessageBox.MessageBoxImage.Error);
                return false;
            }
           
            //Marshal.ZeroFreeCoTaskMemUnicode(pStr);
            //string username = GeneralTools.SHA256Hash(p_username);
            //pass = GeneralTools.SHA256Hash(pass);
            //if (username != BaseDefine.USERNAME || pass != BaseDefine.PASSWORD)
            //{
            //    MessageBox.Show("Check ID / Password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return false;
            //}
        }
        #endregion

        #region Command
        public ICommand CmdSubmit
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (!Login())
                    {
                        return;
                    }  
                    CloseRequested(this, new DialogCloseRequestedEventArgs(true));
                });
            }
        }
        public ICommand CmdCancel
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CloseRequested(this, new DialogCloseRequestedEventArgs(false));
                });
            }
        }
        #endregion
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
    }

    public class PasswordBoxBehavior : Behavior<PasswordBox>
    {
        // BoundPassword
        public SecureString BoundPassword
        {
            get { return (SecureString)GetValue(BoundPasswordProperty); }
            set { SetValue(BoundPasswordProperty, value); }
        }
        public static readonly DependencyProperty BoundPasswordProperty = DependencyProperty.Register("BoundPassword",
            typeof(SecureString), typeof(PasswordBoxBehavior), new FrameworkPropertyMetadata(OnBoundPasswordChanged));
        protected override void OnAttached()
        {
            this.AssociatedObject.PasswordChanged += AssociatedObjectOnPasswordChanged;
            base.OnAttached();
        }
        /// <summary>
        /// Link up the intermediate SecureString (BoundPassword) to the UI instance
        /// </summary>
        private void AssociatedObjectOnPasswordChanged(object s, RoutedEventArgs e)
        {
            this.BoundPassword = this.AssociatedObject.SecurePassword;
        }
        /// <summary>
        /// Reacts to password reset on viewmodel (ViewModel.Password = new SecureString())
        /// </summary>
        private static void OnBoundPasswordChanged(object s, DependencyPropertyChangedEventArgs e)
        {
            var box = ((PasswordBoxBehavior)s).AssociatedObject;
            if (box != null)
            {
                if (((SecureString)e.NewValue).Length == 0)
                    box.Password = string.Empty;
            }
        }
    }
}
