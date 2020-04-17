using RootTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Root_Vega.Dialog
{
	/// <summary>
	/// Dialog_InspResultView.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class Dialog_InspResultView : Window, IDialog
	{
		public Dialog_InspResultView()
		{
			InitializeComponent();

			App.Current.MainWindow.Closed += (sender, args) => this.Close();
		}
		private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.DragMove();
		}
	}
}
