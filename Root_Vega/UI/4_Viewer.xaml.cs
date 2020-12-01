using RootTools.Inspects;
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
using System.Windows.Threading;

namespace Root_Vega
{
	/// <summary>
	/// _4_Viewer.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class _4_Viewer : UserControl
	{
		public Dispatcher _Dispatcher;
		public _4_Viewer()
		{
			InitializeComponent();
			App.m_engineer.m_InspManager.AddChromeDefect += App_AddDefect;
			App.m_engineer.m_InspManager.ClearDefect += _ClearDefect;
			InspectionManager.RefreshDefect += InspectionManager_RefreshDefect;
		}
		~_4_Viewer()
		{
			App.m_engineer.m_InspManager.AddChromeDefect -= App_AddDefect;
			App.m_engineer.m_InspManager.ClearDefect -= _ClearDefect;
			InspectionManager.RefreshDefect -= InspectionManager_RefreshDefect;
		}
		private void InspectionManager_RefreshDefect()
		{
			try
			{
				viewer.Dispatcher.Invoke(new Action(delegate ()
				{
					viewer.RefreshDraw();
				}));
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
		}

		private void _ClearDefect()
		{
			try
			{
				viewer.Dispatcher.Invoke(new Action(delegate ()
				{
					viewer.ClearRect();
					viewer.RefreshDraw();
				}));
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
		}

		private void App_AddDefect(RootTools.DefectDataWrapper item)
		{
			if ((InspectionManager.GetInspectionType(item.nClassifyCode) == InspectionType.Strip) &&
				   InspectionManager.GetInspectionTarget(item.nClassifyCode) == InspectionTarget.Chrome)
			{
				try
				{
					viewer.Dispatcher.Invoke(new Action(delegate ()
					{
						int width = item.nWidth;
						int height = item.nHeight;
						viewer.AddBlock(item.fPosX, item.fPosY, width, height, Brushes.Red, new Pen(Brushes.Red, 1));
					}));
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex.Message);
				}
			}
		}

		//private void Button_Click(object sender, RoutedEventArgs e)
		//{
		//}
	}
}
