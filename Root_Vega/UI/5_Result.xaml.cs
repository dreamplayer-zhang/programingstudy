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

namespace Root_Vega
{
	/// <summary>
	/// _5_Result.xaml에 대한 상호 작용 논e
	/// </summary>
	public partial class _5_Result : UserControl
	{
		public _5_Result()
		{
			InitializeComponent();
		}

		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				var vm = (_5_ResultViewModel)this.DataContext;
				vm.StartSearch();
			}
		}

		private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var vm = (_5_ResultViewModel)this.DataContext;
			vm.OpenSelectedInspectionData();
		}
	}
}
