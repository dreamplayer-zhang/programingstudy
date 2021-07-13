using Root_VEGA_D.Module;
using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Root_VEGA_D
{
    class MainWindow_ViewModel : ObservableObject
    {

        MainWindow m_MainWindow;

        #region Dialog
        public DialogService m_dialogService;
        #endregion

        #region ViewModel

        //RecipeWizard_VM m_recipeWizard_ViewModel;
        //public RecipeWizard_VM p_recipeWizard_ViewModel
        //{
        //    get
        //    {
        //        return m_recipeWizard_ViewModel;
        //    }
        //    set
        //    {
        //        SetProperty(ref m_recipeWizard_ViewModel, value);
        //    }
        //}
        RecipeManager_VM m_recipeManager_ViewModel;
        public RecipeManager_VM p_recipeManager_ViewModel
        {
            get
            {
                return m_recipeManager_ViewModel;
            }
            set
            {
                SetProperty(ref m_recipeManager_ViewModel, value);
            }
        }
        #endregion

        BitmapSource m_bitmapAlignKeySrc;
        public BitmapSource p_bitmapAlignKeySrc
        {
            get => m_bitmapAlignKeySrc;
            set => SetProperty(ref m_bitmapAlignKeySrc, value);
        }

        double m_SnapProgressValue = 0;
        public double p_SnapProgressValue
        {
            get => m_SnapProgressValue;
            set => SetProperty(ref m_SnapProgressValue, value);
        }

        string m_SnapDispText = "0%";
        public string p_SnapDispText
        {
            get => m_SnapDispText;
            set => SetProperty(ref m_SnapDispText, value);
        }

        double m_InspProgressValue = 0;
        public double p_InspProgressValue
        {
            get => m_InspProgressValue;
            set => SetProperty(ref m_InspProgressValue, value);
        }

        string m_InspDispText = "0%";
        public string p_InspDispText
        {
            get => m_InspDispText;
            set => SetProperty(ref m_InspDispText, value);
        }

        public MainWindow_ViewModel(MainWindow mainwindow)
        {
            m_MainWindow = mainwindow;

            InitViewModel();
            DialogInit(m_MainWindow);
        }

        void InitViewModel()
        {
            p_recipeManager_ViewModel = new RecipeManager_VM();
            //p_recipeWizard_ViewModel = new RecipeWizard_VM();
        }
        private void DialogInit(MainWindow main)
        {
            m_dialogService = new DialogService(main);
        }


        #region Command
        public ICommand CmdReview
        {
            get
            {
                return new RelayCommand(() =>
                {
                    MessageBox.Show("Review");
                });
            }
        }

        #endregion

        public void M_vision_LineScanStatusChanged(Vision vision, Run_GrabLineScan moduleRun, Vision.LineScanStatus status, object data)
        {
            switch (status)
            {
                case Vision.LineScanStatus.Init:
                    {
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            p_SnapProgressValue = 0;
                            p_SnapDispText = "0%";
                            p_InspProgressValue = 0;
                            p_InspDispText = "0%";
                        });
                    }
                    break;
                case Vision.LineScanStatus.AlignCompleted:
                    {
                        BitmapImage img = new BitmapImage(new Uri(moduleRun.m_grabMode.p_sTempAlignMarkerFile));
                        m_MainWindow.Dispatcher.BeginInvoke(new ThreadStart(() =>
                        {
                            // Information의 AlignKey 표시
                            p_bitmapAlignKeySrc = img;

                            // RecipeWizard의 AlignKey 표시
                            p_recipeManager_ViewModel.p_bitmapAlignKeySrc = img;
                        }));
                    }
                    break;
                case Vision.LineScanStatus.LineScanStarting:
                    break;
                case Vision.LineScanStatus.LineScanCompleted:
                    {
                        int[] arrData = (int[])data;
                        int nTotalScanNum = arrData[0];
                        int nCurScanNum = arrData[1];
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            double val = (double)nCurScanNum / nTotalScanNum;
                            p_SnapProgressValue = val;
                            p_SnapDispText = val.ToString(".0%");
                        });
                    }
                    break;
                case Vision.LineScanStatus.LineInspCompleted:
                    {
                        int[] arrData = (int[])data;
                        int nTotalScanNum = arrData[0];
                        int nCurScanNum = arrData[1];
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            double val = (double)nCurScanNum / nTotalScanNum;
                            p_SnapProgressValue = val;
                            p_SnapDispText = val.ToString(".0%");
                        });
                    }
                    break;
                case Vision.LineScanStatus.End:
                    {
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            p_SnapProgressValue = 100;
                            p_SnapDispText = "100%";
                        });
                    }
                    break;
                default: break;

            }
        }
    }
}
