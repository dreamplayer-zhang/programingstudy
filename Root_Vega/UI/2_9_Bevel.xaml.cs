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
	/// _2_9_Bevel.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class _2_9_Bevel : UserControl
	{
		public _2_9_Bevel()
		{
			InitializeComponent();
			App.m_engineer.m_InspManager.AddTopBevelDefect += App_AddTopDefect;
			App.m_engineer.m_InspManager.AddLeftBevelDefect += App_AddLeftDefect;
			App.m_engineer.m_InspManager.AddRightBevelDefect += App_AddRightDefect;
			App.m_engineer.m_InspManager.AddBotBevelDefect += App_AddBotDefect;

			App.m_engineer.m_InspManager.ClearDefect += _ClearDefect;
			InspectionManager.RefreshDefect += InspectionManager_RefreshDefect;
		}
		~_2_9_Bevel()
		{
			App.m_engineer.m_InspManager.AddTopBevelDefect -= App_AddTopDefect;
			App.m_engineer.m_InspManager.AddLeftBevelDefect -= App_AddLeftDefect;
			App.m_engineer.m_InspManager.AddRightBevelDefect -= App_AddRightDefect;
			App.m_engineer.m_InspManager.AddBotBevelDefect -= App_AddBotDefect;

			App.m_engineer.m_InspManager.ClearDefect -= _ClearDefect;
			InspectionManager.RefreshDefect -= InspectionManager_RefreshDefect;
		}

		private void InspectionManager_RefreshDefect()
		{
			viewer_top.Dispatcher.Invoke(new Action(delegate ()
			{
				viewer_top.RefreshDraw();
			}));
			viewer_bot.Dispatcher.Invoke(new Action(delegate ()
			{
				viewer_bot.RefreshDraw();
			}));
			viewer_left.Dispatcher.Invoke(new Action(delegate ()
			{
				viewer_left.RefreshDraw();
			}));
			viewer_right.Dispatcher.Invoke(new Action(delegate ()
			{
				viewer_right.RefreshDraw();
			}));
		}

		private void _ClearDefect()
		{
			try
			{
				viewer_top.Dispatcher.Invoke(new Action(delegate ()
				{
					viewer_top.ClearRect();
					viewer_top.RefreshDraw();
				}));
				viewer_bot.Dispatcher.Invoke(new Action(delegate ()
				{
					viewer_bot.ClearRect();
					viewer_bot.RefreshDraw();
				}));
				viewer_left.Dispatcher.Invoke(new Action(delegate ()
				{
					viewer_left.ClearRect();
					viewer_left.RefreshDraw();
				}));
				viewer_right.Dispatcher.Invoke(new Action(delegate ()
				{
					viewer_right.ClearRect();
					viewer_right.RefreshDraw();
				}));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private void App_AddTopDefect(RootTools.DefectDataWrapper item)
		{
			try
			{
				viewer_top.Dispatcher.Invoke(new Action(delegate ()
				{
					int width = item.nWidth;
					int height = item.nHeight;
					viewer_top.AddBlock(item.fPosX, item.fPosY, width, height, Brushes.Red, new Pen(Brushes.Red, 1));
				}));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
		private void App_AddBotDefect(RootTools.DefectDataWrapper item)
		{
			try
			{
				viewer_bot.Dispatcher.Invoke(new Action(delegate ()
				{
					int width = item.nWidth;
					int height = item.nHeight;
					viewer_bot.AddBlock(item.fPosX, item.fPosY, width, height, Brushes.Red, new Pen(Brushes.Red, 1));
				}));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
		private void App_AddLeftDefect(RootTools.DefectDataWrapper item)
		{
			try
			{
				viewer_left.Dispatcher.Invoke(new Action(delegate ()
				{
					int width = item.nWidth;
					int height = item.nHeight;
					viewer_left.AddBlock(item.fPosX, item.fPosY, width, height, Brushes.Red, new Pen(Brushes.Red, 1));
				}));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
		private void App_AddRightDefect(RootTools.DefectDataWrapper item)
		{
			try
			{
				viewer_right.Dispatcher.Invoke(new Action(delegate ()
				{
					int width = item.nWidth;
					int height = item.nHeight;
					viewer_right.AddBlock(item.fPosX, item.fPosY, width, height, Brushes.Red, new Pen(Brushes.Red, 1));
				}));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
}
