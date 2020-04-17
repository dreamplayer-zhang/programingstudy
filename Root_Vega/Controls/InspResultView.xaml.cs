using Root_Vega.Dialog;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Root_Vega.Controls
{
	/// <summary>
	/// InspResultView.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class InspResultView : UserControl
	{
		public InspResultView()
		{
			InitializeComponent();
		}

		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				var vm = (InspResultViewModel)this.DataContext;
				vm.OnStartSearchButton();
			}
		}
	}
}
