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

namespace Root_Vega
{
	/// <summary>
	/// _2_4_Surface.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class _2_5_MainVision : UserControl
	{
		public _2_5_MainVision()
		{
			InitializeComponent();
			App.m_engineer.m_InspManager.AddChromeDefect += App_AddDefect;
			App.m_engineer.m_InspManager.ClearDefect += _ClearDefect;
			InspectionManager.RefreshDefect += InspectionManager_RefreshDefect;
		}
		~_2_5_MainVision()
		{
			App.m_engineer.m_InspManager.AddChromeDefect -= App_AddDefect;
			App.m_engineer.m_InspManager.ClearDefect -= _ClearDefect;
			InspectionManager.RefreshDefect -= InspectionManager_RefreshDefect;
		}
		private void InspectionManager_RefreshDefect()
		{
			viewer.Dispatcher.Invoke(new Action(delegate ()
			{
				viewer.RefreshDraw();
			}));
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
				Console.WriteLine(ex.Message);
			}
		}

		private void App_AddDefect(RootTools.DefectDataWrapper item)
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
				Console.WriteLine(ex.Message);
			}
		}
	}
}
