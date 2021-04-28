using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Root_VEGA_P_Vision
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

        #region [Getter / Setter]
        public SelectMode ModeWindow { get => modeWindow; set => modeWindow = value; }
        public Setup SetupWindow { get => setupWindow; set => setupWindow = value; }
        public Grid MainPanel { get => mainPanel; set => mainPanel = value; }
        #endregion

        #region WPF member
        private Grid mainPanel;
        #endregion

        #region [UI]
        private SelectMode modeWindow;
        private Setup setupWindow;
        private Review_Panel reviewPanel;
        #endregion

        #region [ViewModel]
        private Setup_ViewModel setupViewModel;
        #endregion
        public bool Initialize()
        {
            InitModeSelect();
            InitSetupMode();
            InitReviewMode();
            return true;
        }
        void InitModeSelect()
        {
            modeWindow = new SelectMode();
        }
        void InitReviewMode()
        {
            reviewPanel = new Review_Panel();
        }
        void InitSetupMode()
        {
            setupWindow = new Setup();
            setupViewModel = new Setup_ViewModel();
            setupWindow.DataContext = setupViewModel;
        }
        void InitOperationMode()
        {
        }
        public void ChangeMainUI(UIElement window)
        {
            if (window == null) return;

            MainPanel.Children.Clear();
            MainPanel.Children.Add(window);
        }

        public void ChangeUIMode()
        {
            ChangeMainUI(modeWindow);
        }

        public void ChangeUIReview()
        {
            ChangeMainUI(reviewPanel);
        }

        public void ChangeUISetup()
        {
            ChangeMainUI(setupWindow);
        }

        public void ChangeUIRun()
        {

        }
    }
}
