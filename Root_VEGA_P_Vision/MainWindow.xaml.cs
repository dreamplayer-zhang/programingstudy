using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Root_VEGA_P_Vision.Engineer;
using RootTools;
using RootTools.Database;
using RootTools.Memory;
using RootTools_Vision;
using RootTools_Vision.WorkManager3;
namespace Root_VEGA_P_Vision
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
		public VEGA_P_Vision_Engineer m_engineer { get => GlobalObjects.Instance.Get<VEGA_P_Vision_Engineer>(); }
		public IDialogService dialogService;
        public MainWindow()
        {
            InitializeComponent();
			DataContext = new MainVM(this);
        }

		#region Title Bar
		private void MinimizeButton_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}
		private void MaximizeButton_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Maximized;
			NormalizeButton.Visibility = Visibility.Visible;
			MaximizeButton.Visibility = Visibility.Collapsed;
		}
		private void NormalizeButton_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Normal;
			MaximizeButton.Visibility = Visibility.Visible;
			NormalizeButton.Visibility = Visibility.Collapsed;
		}
		private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
			{
				if (WindowState == WindowState.Maximized)
				{
					WindowState = WindowState.Normal;
                    MaximizeButton.Visibility = Visibility.Visible;
                    NormalizeButton.Visibility = Visibility.Collapsed;
                }
				else
				{
					WindowState = WindowState.Maximized;
                    NormalizeButton.Visibility = Visibility.Visible;
                    MaximizeButton.Visibility = Visibility.Collapsed;
                }
			}
			else
			{
				DragMove();
			}
		}
		#endregion

		#region Window Event
		public void Window_Loaded()
		{
			if (!Directory.Exists(@"C:\Recipe\Vega_P")) Directory.CreateDirectory(@"C:\Recipe\Vega_P");

			Init();
			if (WindowState == WindowState.Maximized)
			{
				MaximizeButton.Visibility = Visibility.Collapsed;
			}
			else
			{
				NormalizeButton.Visibility = Visibility.Collapsed;
			}
		}

		void Init()
        {
			CreateGlobalPaths();

			if(!RegisterGlobalObjects())
            {
				MessageBox.Show("Program Init Fail");
				return;
            }
			if (!UIManager.Instance.Initialize())
            {
				MessageBox.Show("UI Init Fail");
				return;
            }

			UIManager.Instance.MainPanel = MainPanel;
			UIManager.Instance.ChangeUIMode();

			DatabaseManager.Instance.SetDatabase(1);
			DatabaseManager.Instance.ValidateDatabase();

			logView.Init(LogView._logView);
            InitTimer();
        }
		void CreateGlobalPaths()
		{
			Type t = typeof(Constants.RootPath);
			FieldInfo[] fields = t.GetFields(BindingFlags.Static | BindingFlags.Public);
			foreach (FieldInfo field in fields)
				Directory.CreateDirectory(field.GetValue(null).ToString());
		}

		bool RegisterGlobalObjects()
        {
            try
            {
				Settings settings = GlobalObjects.Instance.Register<Settings>();
				VEGA_P_Vision_Engineer engineer = GlobalObjects.Instance.Register<VEGA_P_Vision_Engineer>();
				DialogService dialogService = GlobalObjects.Instance.Register<DialogService>(this);
				engineer.Init("Vega-P");

				MemoryTool memoryTool = engineer.ClassMemoryTool();
				Type partstype = typeof(Module.Vision.eParts);
				Type InspType = typeof(Module.Vision.MainOptic.eInsp);
				Type UpdownType = typeof(Module.Vision.eUpDown);
				Type SideType = typeof(Module.Vision.SideOptic.eSide);
				int sidelen = Enum.GetValues(SideType).Length;
				int partslen = Enum.GetValues(partstype).Length;
				int updownlen = Enum.GetValues(UpdownType).Length;
				int insplen = Enum.GetValues(InspType).Length;

				RecipeCoverFront recipeCoverFront = GlobalObjects.Instance.Register<RecipeCoverFront>();
				RecipeCoverBack recipeCoverBack = GlobalObjects.Instance.Register<RecipeCoverBack>();
				RecipePlateFront recipePlateFront = GlobalObjects.Instance.Register<RecipePlateFront>();
				RecipePlateBack recipePlateBack = GlobalObjects.Instance.Register<RecipePlateBack>();

				GlobalObjects.Instance.RegisterNamed<ImageData>(App.mMaskLayer, memoryTool.GetMemory(App.mPool, App.mGroup, App.mMaskLayer));
				GlobalObjects.Instance.Register<PodIDInfo>();
				foreach (var v in Enum.GetValues(partstype))
                    foreach (var v2 in Enum.GetValues(InspType))
                        foreach (var v3 in Enum.GetValues(UpdownType))
                        {
							List<IntPtr> li = new List<IntPtr>();
                            string memstr = v.ToString() + "." + v2.ToString() + "." + v3.ToString();
							MemoryData memData = engineer.ClassMemoryTool().GetMemory(App.mPool, App.mGroup, memstr);
                            ImageData Data = GlobalObjects.Instance.RegisterNamed<ImageData>(memstr, memoryTool.GetMemory(App.mPool,App.mGroup,memstr));
                            if (Data.m_MemData!=null)
                            {
								Data.p_nByte = memData.p_nByte;
								Data.p_nPlane = memData.p_nCount;
                            }

							for(int i=0;i<Data.p_nPlane;i++)
								li.Add(Data.GetPtr(i));

							if(Data.GetPtr()!=IntPtr.Zero)
                            {
								WorkManager Insp = GlobalObjects.Instance.RegisterNamed<WorkManager>(memstr + ".Inspection",4);
								RecipeBase recipe = recipeCoverBack;

								switch(v)
                                {
									case Module.Vision.eParts.EIP_Cover:
										recipe = v3.Equals(Module.Vision.eUpDown.Front) ? recipeCoverFront : recipeCoverBack;
										break;
									case Module.Vision.eParts.EIP_Plate:
										recipe = v3.Equals(Module.Vision.eUpDown.Front) ? recipePlateFront : recipePlateBack;
										break;
                                }
									

								Insp.SetRecipe(recipe);
								Insp.SetSharedBuffer(new SharedBufferInfo(
									Data.p_Size.X,
									Data.p_Size.Y,
									1,
									li));
                            }
						}


				foreach(var v in Enum.GetValues(partstype))
					foreach(var v2 in Enum.GetValues(SideType))
                    {
						string memstr = v.ToString() + "." + v2.ToString();
	
						MemoryData memData = engineer.ClassMemoryTool().GetMemory(App.mPool, App.mGroup, memstr);
                        ImageData Data = GlobalObjects.Instance.RegisterNamed<ImageData>(memstr, memoryTool.GetMemory(App.mPool, App.mGroup, memstr));
                        List<IntPtr> li = new List<IntPtr>();
						if(Data.m_MemData!=null)
                        {
							Data.p_nByte = memData.p_nByte;
							Data.p_nPlane = memData.p_nCount;
                        }
						for (int i = 0; i < Data.p_nPlane; i++)
							li.Add(Data.GetPtr(i));

						if(Data.GetPtr()!=IntPtr.Zero)
                        {
							WorkManager Insp = GlobalObjects.Instance.RegisterNamed<WorkManager>(memstr + ".Inspection", 4);
							RecipeBase recipe = recipeCoverFront;

							switch (v)
							{
								case Module.Vision.eParts.EIP_Cover:
									recipe =  recipeCoverFront;
									break;
								case Module.Vision.eParts.EIP_Plate:
									recipe = recipePlateFront;
									break;
							}

							Insp.SetRecipe(recipe);
							Insp.SetSharedBuffer(new SharedBufferInfo(
								Data.p_Size.X,
								Data.p_Size.Y,
								1,
								li)) ;
                        }
                    }
				//DialogService
				dialogService.Register<Dialog_ImageOpenViewModel, Dialog_ImageOpen>();
			}
			catch (Exception ex)
            {
				MessageBox.Show(ex.Message);
				return false;
            }
			return true;
        }

		#region [Timer]
		DispatcherTimer m_timer = new DispatcherTimer();
		private void InitTimer()
        {
			m_timer.Interval = TimeSpan.FromMilliseconds(20);
			m_timer.Tick += M_timer_Tick;
			m_timer.Start();
        }
		private void M_timer_Tick(object sender,EventArgs e)
        {

        }
        #endregion
        public void Window_Closing()
		{
			GlobalObjects.Instance.Get<VEGA_P_Vision_Engineer>().ThreadStop();
			GlobalObjects.Instance.Clear();
			Application.Current.Shutdown();
		}
		#endregion
	}
}
