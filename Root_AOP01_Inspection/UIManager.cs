using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Root_AOP01_Inspection
{
	public class UIManager
	{
		private UIManager()
		{
		}

		private static readonly Lazy<UIManager> instance = new Lazy<UIManager>(() => new UIManager());

		public static UIManager Instance
		{
			get
			{
				return instance.Value;
			}
		}

		public SelectMode ModeWindow { get => modeWindow; set => modeWindow = value; }
		public Setup_Panel SetupWindow { get => setupWindow; set => setupWindow = value; }
		public Review_Panel ReviewWindow { get => reviewWindow; set => reviewWindow = value; }
		public Run_Panel RunWindow { get => runWindow; set => runWindow = value; }
		public Grid MainPanel { get => mainPanel; set => mainPanel = value; }
		public Setup_ViewModel SetupViewModel { get => setupViewModel; set => setupViewModel = value; }
		//internal Review_ViewModel ReviewViewModel { get => reviewViewModel; set => reviewViewModel = value; }
		internal Run_ViewModel RunViewModel { get => runViewModel; set => runViewModel = value; }
		//public SettingDialog SettingDialog { get => settingDialog; set => settingDialog = value; }
		//public SettingDialog_ViewModel SettingDialogViewModel { get => settingDialogViewModel; set => settingDialogViewModel = value; }



		#region WPF member
		private Grid mainPanel;
		#endregion

		#region UI
		private SelectMode modeWindow;
		private Setup_Panel setupWindow;

		//private Root_WIND2.UI_Temp.Setup setupWindow2;
		private Review_Panel reviewWindow;
		private Run_Panel runWindow;

		//private SettingDialog settingDialog;
		#endregion

		#region ViewModel
		private Setup_ViewModel setupViewModel;
		//private Review_ViewModel reviewViewModel;
		private Run_ViewModel runViewModel;

		//private SettingDialog_ViewModel settingDialogViewModel;
		#endregion

		public bool Initialize()
		{
			// Main UI
			InitModeSelect();
			InitSetupMode();
			InitReviewMode();
			InitRunMode();

			// 기타 UI
			//InitSettingDialog();

			return true;
		}

		void InitModeSelect()
		{
			modeWindow = new SelectMode();
			modeWindow.Init();
		}
		void InitSetupMode()
		{
			setupWindow = new Setup_Panel();
			setupViewModel = new Setup_ViewModel();
			setupWindow.DataContext = SetupViewModel;

			//setupWindow2 = new UI_Temp.Setup();
			//setupViewModel2 = new UI_Temp.Setup_ViewModel();
			//setupWindow2.DataContext = setupViewModel2;

		}
		void InitReviewMode()
		{
			reviewWindow = new Review_Panel();
			//reviewViewModel = new Review_ViewModel(reviewWindow);
			//reviewWindow.DataContext = ReviewViewModel;
		}
		void InitRunMode()
		{
			runWindow = new Run_Panel();
			runViewModel = new Run_ViewModel();
			runWindow.DataContext = runViewModel;
		}

		//void InitSettingDialog()
		//{
		//	settingDialog = new SettingDialog();
		//	settingDialogViewModel = new SettingDialog_ViewModel();
		//	settingDialog.DataContext = settingDialogViewModel;
		//}

		public void ChangeMainUI(UIElement window)
		{
			if (window == null) return;

			MainPanel.Children.Clear();
			MainPanel.Children.Add((UIElement)window);
		}

		public void ChangeUIMode()
		{
			ChangeMainUI((UIElement)modeWindow);
		}

		public void ChangUISetup()
		{
			ChangeMainUI((UIElement)setupWindow);
			//ChangeMainUI((UIElement)setupWindow2);

		}

		public void ChangUIReview()
		{
			ChangeMainUI((UIElement)reviewWindow);
		}

		public void ChangUIRun()
		{
			ChangeMainUI((UIElement)runWindow);
		}
	}
}
