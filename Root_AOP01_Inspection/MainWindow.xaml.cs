using RootTools;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Root_EFEM.Module;
using Root_AOP01_Inspection.Module;
using RootTools.GAFs;
using static Root_AOP01_Inspection.AOP01_Handler;
using System.Windows.Threading;
using System;
using System.Windows.Media;
using RootTools_Vision;
using RootTools.Memory;
using Root_AOP01_Inspection.Recipe;
using System.Collections.Generic;

namespace Root_AOP01_Inspection
{
	public class Dummy
	{
		public string a
		{
			get; set;
		}
		public string b
		{
			get; set;
		}
		public string c
		{
			get; set;
		}
		public string d
		{
			get; set;
		}
		public string e
		{
			get; set;
		}
	}

	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>

	public partial class MainWindow : Window
	{


		#region Title Bar
		private void MinimizeButton_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}
		private void MaximizeButton_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Maximized;
			NormalizeButton.Visibility = Visibility.Visible;
			MaximizeButton.Visibility = Visibility.Collapsed;
		}
		private void NormalizeButton_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Normal;
			MaximizeButton.Visibility = Visibility.Visible;
			NormalizeButton.Visibility = Visibility.Collapsed;
		}
		private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
			{
				if (this.WindowState == WindowState.Maximized)
				{
					this.WindowState = WindowState.Normal;
					MaximizeButton.Visibility = Visibility.Visible;
					NormalizeButton.Visibility = Visibility.Collapsed;
				}
				else
				{
					this.WindowState = WindowState.Maximized;
					NormalizeButton.Visibility = Visibility.Visible;
					MaximizeButton.Visibility = Visibility.Collapsed;
				}
			}
			else
			{
				this.DragMove();
			}
		}
		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
		#endregion

		#region Window Event
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (!Directory.Exists(@"C:\Recipe\AOP01")) Directory.CreateDirectory(@"C:\Recipe\AOP01");

			Init();

			if (this.WindowState == WindowState.Maximized)
			{
				MaximizeButton.Visibility = Visibility.Collapsed;
			}
			else
			{
				NormalizeButton.Visibility = Visibility.Collapsed;
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			ThreadStop();
		}
		#endregion

		#region Other Event
		private void ModeSelect_Click(object sender, RoutedEventArgs e)
		{
			MainPanel.Children.Clear();
			MainPanel.Children.Add(UIManager.Instance.ModeWindow);
		}
		#endregion

		//#region Mode UI
		//public SelectMode ModeSelect;
		//public Setup_Panel Setup;
		//public Review_Panel Review;
		//public Run_Panel Run;
		////public Dlg_Start Dlg;
		//#endregion

		#region ViewModel
		//private Dlg_ViewModel m_Dlg;
		#endregion

		public AOP01_Engineer m_engineer { get { return GlobalObjects.Instance.Get<AOP01_Engineer>(); } }
		public IDialogService dialogService;

		public MainWindow()
		{
			InitializeComponent();
		}
		public bool RegisterGlobalObjects()
		{
#if !DEBUG
			try
			{
#endif
			// Engineer
			AOP01_Engineer engineer = GlobalObjects.Instance.Register<AOP01_Engineer>();
				DialogService dialogService = GlobalObjects.Instance.Register<DialogService>(this);
				//WIND2_Warning warning = GlobalObjects.Instance.Register<WIND2_Warning>();
				engineer.Init("AOP01");

				MemoryTool memoryTool = engineer.ClassMemoryTool();

				//이미지 데이터 초기화 및 등록
				ImageData imageMain = GlobalObjects.Instance.RegisterNamed<ImageData>(App.MainRegName, memoryTool.GetMemory(App.mPool, App.mGroup, App.mMainMem));
				ImageData image45D = GlobalObjects.Instance.RegisterNamed<ImageData>(App.PellRegName, memoryTool.GetMemory(App.mPool, App.mGroup, App.m45DMem));

				ImageData imageSideLeft = GlobalObjects.Instance.RegisterNamed<ImageData>(App.SideLeftRegName, memoryTool.GetMemory(App.mPool, App.mGroup, App.mSideLeftMem));
				ImageData imageSideTop = GlobalObjects.Instance.RegisterNamed<ImageData>(App.SideTopRegName, memoryTool.GetMemory(App.mPool, App.mGroup, App.mSideTopMem));
				ImageData imageSideRight = GlobalObjects.Instance.RegisterNamed<ImageData>(App.SideRightRegName, memoryTool.GetMemory(App.mPool, App.mGroup, App.mSideRightMem));
				ImageData imageSideBottom = GlobalObjects.Instance.RegisterNamed<ImageData>(App.SideBotRegName, memoryTool.GetMemory(App.mPool, App.mGroup, App.mSideBotMem));

				//byte 업데이트
				if (imageMain.m_MemData != null) 
					imageMain.p_nByte = engineer.ClassMemoryTool().GetMemory(App.mPool, App.mGroup, App.mMainMem).p_nCount;
				if (image45D.m_MemData != null) 
					image45D.p_nByte = engineer.ClassMemoryTool().GetMemory(App.mPool, App.mGroup, App.m45DMem).p_nByte;

				if (imageSideLeft.m_MemData != null) 
					imageSideLeft.p_nByte = engineer.ClassMemoryTool().GetMemory(App.mPool, App.mGroup, App.mSideLeftMem).p_nCount;
				if (imageSideTop.m_MemData != null) 
					imageSideTop.p_nByte = engineer.ClassMemoryTool().GetMemory(App.mPool, App.mGroup, App.mSideTopMem).p_nCount;
				if (imageSideRight.m_MemData != null) 
					imageSideRight.p_nByte = engineer.ClassMemoryTool().GetMemory(App.mPool, App.mGroup, App.mSideRightMem).p_nCount;
				if (imageSideBottom.m_MemData != null) 
					imageSideBottom.p_nByte = engineer.ClassMemoryTool().GetMemory(App.mPool, App.mGroup, App.mSideBotMem).p_nCount;


				// Recipe 초기화 및 등록. 이름으로 찾도록 모두 수정해야함
				AOP_RecipeSurface frontSurface = GlobalObjects.Instance.RegisterNamed<AOP_RecipeSurface>(App.MainRecipeRegName);
				AOP_RecipeSurface sideSurface = GlobalObjects.Instance.RegisterNamed<AOP_RecipeSurface>(App.SideRecipeRegName);
				AOP_RecipeSurface pellSurface = GlobalObjects.Instance.RegisterNamed<AOP_RecipeSurface>(App.PellRecipeRegName);
				AOP_RecipeSurface backSurface = GlobalObjects.Instance.RegisterNamed<AOP_RecipeSurface>(App.BackRecipeRegName);

				if (imageMain.GetPtr() == IntPtr.Zero)
				{
					//MessageBox.Show("Front Inspection 생성 실패, 메모리 할당 없음");
				}
				else
				{
					// Inspection Manager
					var front_info = new SharedBufferInfo(imageMain.GetPtr(0), imageMain.p_Size.X, imageMain.p_Size.Y, imageMain.p_nByte, imageMain.GetPtr(1), imageMain.GetPtr(2));

					InspectionManager_AOP inspectionFront = GlobalObjects.Instance.RegisterNamed<InspectionManager_AOP>
						(App.MainInspMgRegName, frontSurface, front_info);
				}
				if (imageSideLeft.GetPtr() == IntPtr.Zero)
				{
					//MessageBox.Show("Front Inspection 생성 실패, 메모리 할당 없음");
				}
				else
				{
					// Inspection Manager
					var sideLeft_info = new SharedBufferInfo(imageSideLeft.GetPtr(0), imageSideLeft.p_Size.X, imageSideLeft.p_Size.Y, imageSideLeft.p_nByte, imageSideLeft.GetPtr(1), imageSideLeft.GetPtr(2));
					InspectionManager_AOP inspectionSideLeft = GlobalObjects.Instance.RegisterNamed<InspectionManager_AOP>
						(App.SideLeftInspMgRegName,
						sideSurface,
						sideLeft_info);
				}
				if (imageSideTop.GetPtr() == IntPtr.Zero)
				{
					//MessageBox.Show("Front Inspection 생성 실패, 메모리 할당 없음");
				}
				else
				{
					// Inspection Manager
					var sideTop_info = new SharedBufferInfo(imageSideTop.GetPtr(0), imageSideTop.p_Size.X, imageSideTop.p_Size.Y, imageSideTop.p_nByte, imageSideTop.GetPtr(1), imageSideTop.GetPtr(2));

					InspectionManager_AOP inspectionSideTop = GlobalObjects.Instance.RegisterNamed<InspectionManager_AOP>
						(App.SideTopInspMgRegName,
						sideSurface,
						sideTop_info);
				}
				if (imageSideRight.GetPtr() == IntPtr.Zero)
				{
					//MessageBox.Show("Front Inspection 생성 실패, 메모리 할당 없음");
				}
				else
				{
					// Inspection Manager
					var sideRight_info = new SharedBufferInfo(imageSideRight.GetPtr(0), imageSideRight.p_Size.X, imageSideRight.p_Size.Y, imageSideRight.p_nByte, imageSideRight.GetPtr(1), imageSideRight.GetPtr(2));

					InspectionManager_AOP inspectionSideRight = GlobalObjects.Instance.RegisterNamed<InspectionManager_AOP>
						(App.SideRightInspMgRegName, sideSurface, sideRight_info);
				}
				if (imageSideBottom.GetPtr() == IntPtr.Zero)
				{
					//MessageBox.Show("Front Inspection 생성 실패, 메모리 할당 없음");
				}
				else
				{
					// Inspection Manager
					var sideBot_info = new SharedBufferInfo(imageSideBottom.GetPtr(0), imageSideBottom.p_Size.X, imageSideBottom.p_Size.Y, imageSideBottom.p_nByte, imageSideBottom.GetPtr(1), imageSideBottom.GetPtr(2));

					InspectionManager_AOP inspectionSideBot = GlobalObjects.Instance.RegisterNamed<InspectionManager_AOP>
						(App.SideBotInspMgRegName, sideSurface, sideBot_info);
				}

				if (image45D.GetPtr() == IntPtr.Zero)
				{
					//MessageBox.Show("Front Inspection 생성 실패, 메모리 할당 없음");
				}
				else
				{
					// Inspection Manager
					var pell_info = new SharedBufferInfo(image45D.GetPtr(0), image45D.p_Size.X, image45D.p_Size.Y, image45D.p_nByte, image45D.GetPtr(1), image45D.GetPtr(2));

					InspectionManager_AOP inspectionPell = GlobalObjects.Instance.RegisterNamed<InspectionManager_AOP>
						(App.PellInspMgRegName, pellSurface, pell_info);
				}
				if (imageMain.GetPtr() == IntPtr.Zero)
				{
					//MessageBox.Show("Front Inspection 생성 실패, 메모리 할당 없음");
				}
				else
				{
					// Inspection Manager
					var back_info = new SharedBufferInfo(imageMain.GetPtr(0), imageMain.p_Size.X, imageMain.p_Size.Y, imageMain.p_nByte, imageMain.GetPtr(1), imageMain.GetPtr(2));//Main을 그대로 사용

					InspectionManager_AOP inspectionBack = GlobalObjects.Instance.RegisterNamed<InspectionManager_AOP>
						(App.BackInspMgRegName, backSurface, back_info);
				}

				// DialogService

				dialogService.Register<Dialog_ImageOpenViewModel, Dialog_ImageOpen>();
				//dialogService.Register<Dialog_Scan_ViewModel, Dialog_Scan>();
				//dialogService.Register<SettingDialog_ViewModel, SettingDialog>();
				dialogService.Register<TK4S, TK4SModuleUI>();
#if !DEBUG
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				return false;
			}
#endif
			return true;
		}
		void Init()
		{

			if (RegisterGlobalObjects() == false)
			{
				MessageBox.Show("Program Initialization fail");
				return;
			}

			if (UIManager.Instance.Initialize() == false)
			{
				MessageBox.Show("UI Initialization fail");
				return;
			}

			//// WPF 파라매터 연결
			UIManager.Instance.MainPanel = this.MainPanel;

			UIManager.Instance.ChangeUIMode();

			if (m_engineer.m_handler.m_aLoadportType[0] == eLoadport.Cymechs && m_engineer.m_handler.m_aLoadportType[1] == eLoadport.Cymechs)
			{
				UIManager.Instance.RunWindow.Init(m_engineer.m_handler.m_mainVision, m_engineer.m_handler.m_backsideVision, (RTRCleanUnit)m_engineer.m_handler.m_wtr, (Loadport_Cymechs)m_engineer.m_handler.m_aLoadport[0], (Loadport_Cymechs)m_engineer.m_handler.m_aLoadport[1], m_engineer, (RFID_Brooks)m_engineer.m_handler.m_aRFID[0], (RFID_Brooks)m_engineer.m_handler.m_aRFID[1]);
			}

			if (m_engineer.m_handler.m_FDC.m_aData.Count > 0)
			{
				try
				{
					FDCName1.DataContext = m_engineer.m_handler.m_FDC.m_aData[0];
					FDCValue1.DataContext = m_engineer.m_handler.m_FDC.m_aData[0];
					FDCName2.DataContext = m_engineer.m_handler.m_FDC.m_aData[1];
					FDCValue2.DataContext = m_engineer.m_handler.m_FDC.m_aData[1];
					FDCName3.DataContext = m_engineer.m_handler.m_FDC.m_aData[2];
					FDCValue3.DataContext = m_engineer.m_handler.m_FDC.m_aData[2];
					FDCName4.DataContext = m_engineer.m_handler.m_FDC.m_aData[3];
					FDCValue4.DataContext = m_engineer.m_handler.m_FDC.m_aData[3];
				}
				catch { }
			}

			///////시연용 임시코드
			//DatabaseManager.Instance.SetDatabase(1);
			//////
			//logView.Init(LogView.m_logView);
			//WarningUI.Init(GlobalObjects.Instance.Get<WIND2_Warning>());
			InitTimer();
			//dialogService = new DialogService(this);
			//dialogService.Register<Dialog_ImageOpenViewModel, Dialog_ImageOpen>();

			//Init_ViewModel();
			//Init_UI();
			
			//InitTimer();
		}
		DispatcherTimer m_timer = new DispatcherTimer();
		void InitTimer()
		{
			m_timer.Interval = TimeSpan.FromMilliseconds(20);
			m_timer.Tick += M_timer_Tick;
			m_timer.Start();
		}

		private void M_timer_Tick(object sender, EventArgs e)
		{
			if (m_engineer.m_handler.m_FDC.m_aData.Count > 0)
			{
				try
				{
					FDC1.Background = m_engineer.m_handler.m_FDC.m_aData[0].p_bAlarm == true ? Brushes.Red : Brushes.AliceBlue;
					FDC2.Background = m_engineer.m_handler.m_FDC.m_aData[1].p_bAlarm == true ? Brushes.Red : Brushes.AliceBlue;
					FDC3.Background = m_engineer.m_handler.m_FDC.m_aData[2].p_bAlarm == true ? Brushes.Red : Brushes.AliceBlue;
					FDC4.Background = m_engineer.m_handler.m_FDC.m_aData[3].p_bAlarm == true ? Brushes.Red : Brushes.AliceBlue;
				}
				catch { }
			}
		}
		void ThreadStop()
		{
			m_engineer.ThreadStop();
		}


		private void NaviRecipeSummary_Click(object sender, RoutedEventArgs e)
		{
			MainPanel.Children.Clear();
			MainPanel.Children.Add(UIManager.Instance.SetupWindow);
			UIManager.Instance.SetupViewModel.Set_RecipeSummary();
		}

		private void NaviRecipeSpec_Click(object sender, RoutedEventArgs e)
		{
			MainPanel.Children.Clear();
			MainPanel.Children.Add(UIManager.Instance.SetupWindow);
			UIManager.Instance.SetupViewModel.Set_RecipeSpec();
		}

		private void NaviRecipe45D_Click(object sender, RoutedEventArgs e)
		{
			MainPanel.Children.Clear();
			MainPanel.Children.Add(UIManager.Instance.SetupWindow);
			UIManager.Instance.SetupViewModel.Set_Recipe45DPanel();
		}

		private void NaviRecipeFrontside_Click(object sender, RoutedEventArgs e)
		{
			MainPanel.Children.Clear();
			MainPanel.Children.Add(UIManager.Instance.SetupWindow);
			UIManager.Instance.SetupViewModel.Set_RecipeFrontsidePanel();
		}

		private void NaviRecipeEdge_Click(object sender, RoutedEventArgs e)
		{
			MainPanel.Children.Clear();
			MainPanel.Children.Add(UIManager.Instance.SetupWindow);
			UIManager.Instance.SetupViewModel.Set_RecipeEdgePanel();
		}

		private void NaviRecipeLADS_Click(object sender, RoutedEventArgs e)
		{
			MainPanel.Children.Clear();
			MainPanel.Children.Add(UIManager.Instance.SetupWindow);
			UIManager.Instance.SetupViewModel.Set_RecipeLADSPanel();
		}

		private void NaviMaintenance_Click(object sender, RoutedEventArgs e)
		{
			MainPanel.Children.Clear();
			MainPanel.Children.Add(UIManager.Instance.SetupWindow);
			UIManager.Instance.SetupViewModel.Set_MaintenancePanel();
		}

		private void NaviGEM_Click(object sender, RoutedEventArgs e)
		{
			MainPanel.Children.Clear();
			MainPanel.Children.Add(UIManager.Instance.SetupWindow);
			UIManager.Instance.SetupViewModel.Set_GEMPanel();
		}

		private void NaviReview_Click(object sender, RoutedEventArgs e)
		{
			MainPanel.Children.Clear();
			MainPanel.Children.Add(UIManager.Instance.ReviewWindow);
		}

		private void NaviRun_click(object sender, RoutedEventArgs e)
		{
			MainPanel.Children.Clear();
			MainPanel.Children.Add(UIManager.Instance.RunWindow);
		}


		private void ViewMenuItem_Click(object sender, RoutedEventArgs e)
		{
			bool check = false;
			MenuItem item = sender as MenuItem;
			for (int i = 0; i < ViewMenu.Items.Count; i++)
			{
				check = ((MenuItem)ViewMenu.Items[i]).IsChecked || check;

				if (item == (MenuItem)ViewMenu.Items[i])
				{
					if (item.IsChecked)
					{
						viewTab.SelectedIndex = i;
						foreach (TabItem tab in viewTab.Items)
						{
							if (tab.Visibility == Visibility.Visible)
								viewTab.SelectedIndex = tab.TabIndex;
						}
					}
				}



			}
			if (check == false)
			{
				splitter.IsEnabled = false;
				ViewArea.Height = new GridLength(0);
			}
			else
			{
				splitter.IsEnabled = true;
				ViewArea.Height = new GridLength(200);
			}

		}
	}
}
