using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Root_WIND2
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
        public Setup SetupWindow { get => setupWindow; set => setupWindow = value; }
        public Root_WIND2.UI_User.Review ReviewWindow { get => reviewWindow; set => reviewWindow = value; }
        public Run RunWindow { get => runWindow; set => runWindow = value; }
        public Grid MainPanel { get => mainPanel; set => mainPanel = value; }
        public Setup_ViewModel SetupViewModel { get => setupViewModel; set => setupViewModel = value; }
        internal Root_WIND2.UI_User.Review_ViewModel ReviewViewModel { get => reviewViewModel; set => reviewViewModel = value; }
        internal Run_ViewModel RunViewModel { get => runViewModel; set => runViewModel = value; }
        public SettingDialog SettingDialog { get => settingDialog; set => settingDialog = value; }
        public SettingDialog_ViewModel SettingDialogViewModel { get => settingDialogViewModel; set => settingDialogViewModel = value; }
        public KlarfSettingDialog KlarfSettingDialog { get => klarfSettingDialog; set => klarfSettingDialog = value; }
        public KlarfSettingDialog_ViewModel KlarfSettingDialogViewModel { get => klarfSettingDialogViewModel; set => klarfSettingDialogViewModel = value; }


        #region WPF member
        private Grid mainPanel;
        #endregion

        #region UI
        private SelectMode modeWindow;
        private Setup setupWindow;

        private Root_WIND2.UI_User.Setup setupUserWindow;
        //private Review reviewWindow;
        private Root_WIND2.UI_User.Review reviewWindow;
        private Run runWindow;

        private SettingDialog settingDialog;
        private KlarfSettingDialog klarfSettingDialog;
        #endregion

        #region ViewModel
        private Root_WIND2.UI_User.Setup_ViewModel setupUserViewModel;

        private Setup_ViewModel setupViewModel;
        //private Review_ViewModel reviewViewModel;
        private Root_WIND2.UI_User.Review_ViewModel reviewViewModel;
        private Run_ViewModel runViewModel;

        private SettingDialog_ViewModel settingDialogViewModel;
        private KlarfSettingDialog_ViewModel klarfSettingDialogViewModel;
        #endregion

        public bool Initialize()
        {
            // Main UI
            InitModeSelect();
            InitSetupMode();
            InitReviewMode();
            InitRunMode();

            // 기타 UI
            InitSettingDialog();

            return true;
        }

        void InitModeSelect()
        {
            modeWindow = new SelectMode();
            modeWindow.Init();
        }
        void InitSetupMode()
        {
            setupWindow = new Setup();
            setupViewModel = new Setup_ViewModel();
            setupWindow.DataContext = SetupViewModel;

            setupUserWindow = new UI_User.Setup();
            setupUserViewModel = new UI_User.Setup_ViewModel();
            setupUserWindow.DataContext = setupUserViewModel;

        }
        void InitReviewMode()
        {
            reviewWindow = new Root_WIND2.UI_User.Review();
            reviewViewModel = new Root_WIND2.UI_User.Review_ViewModel(reviewWindow);
            reviewWindow.DataContext = ReviewViewModel;
        }
        void InitRunMode()
        {
            runWindow = new Run();
            runViewModel = new Run_ViewModel(setupViewModel);
            runWindow.DataContext = runViewModel;
        }

        void InitSettingDialog()
        {
            settingDialog = new SettingDialog();
            settingDialogViewModel = new SettingDialog_ViewModel(GlobalObjects.Instance.Get<Settings>());
            settingDialog.DataContext = settingDialogViewModel;

            klarfSettingDialog = new KlarfSettingDialog();
            klarfSettingDialogViewModel = new KlarfSettingDialog_ViewModel(GlobalObjects.Instance.Get<KlarfSetting>().SettingEdgeKlarf);    // 우선 Edge만
            klarfSettingDialog.DataContext = klarfSettingDialogViewModel;
        }

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

        }

        public void ChangUIReview()
        {
            ChangeMainUI((UIElement)reviewWindow);
        }

        public void ChangUIRun()
        {
            ChangeMainUI((UIElement)runWindow);
        }

        public void ChangUISetupUser()
        {
            ChangeMainUI((UIElement)setupUserWindow);
        }
    }
}
