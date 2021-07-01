using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Root_VEGA_D.Engineer;

namespace Root_VEGA_D
{
	/// <summary>
	/// Login_UI.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class Login_UI : Window
	{
		VEGA_D_Engineer m_engineer;
        public Login_UI(VEGA_D_Engineer engineer)
        {
			InitializeComponent();
			m_engineer = engineer;
			textBlockLoginLevel.DataContext = m_engineer.m_login;
			textBlockLoginLevel.Foreground = m_engineer.m_login.p_sUserName == "Logout" ? Brushes.Red : Brushes.Green;

			textBoxID.Focus();
		}

		private void Login_Click(object sender, RoutedEventArgs e)
		{
			m_engineer.m_login.p_sComboName = textBoxID.Text;
			m_engineer.m_login.CheckLogin(textBoxPW.Text);
			textBlockLoginLevel.Foreground = m_engineer.m_login.p_sUserName == "Logout" ? Brushes.Red : Brushes.Green;
		}

        private void textBoxID_KeyDown(object sender, KeyEventArgs e)
        {
			if(e.Key == Key.Enter)
            {
				m_engineer.m_login.p_sComboName = textBoxID.Text;
				m_engineer.m_login.CheckLogin(textBoxPW.Text);
				textBlockLoginLevel.Foreground = m_engineer.m_login.p_sUserName == "Logout" ? Brushes.Red : Brushes.Green;
			}

			if(e.Key == Key.Escape)
            {
				Close();
            }
        }
    }
}
