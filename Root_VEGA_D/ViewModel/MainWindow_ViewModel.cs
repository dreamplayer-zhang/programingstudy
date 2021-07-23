using Root_VEGA_D.Module;
using Root_VEGA_D.Module.Recipe;
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
        ADIRecipe m_recipe;

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

        BitmapSource m_bmpLeftTopAlignKeySrc;
        public BitmapSource p_bmpLeftTopAlignKeySrc
        {
            get => m_bmpLeftTopAlignKeySrc;
            set => SetProperty(ref m_bmpLeftTopAlignKeySrc, value);
        }
        BitmapSource m_bmpLeftBottomAlignKeySrc;
        public BitmapSource p_bmpLeftBottomAlignKeySrc
        {
            get => m_bmpLeftBottomAlignKeySrc;
            set => SetProperty(ref m_bmpLeftBottomAlignKeySrc, value);
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

        public MainWindow_ViewModel(MainWindow mainwindow, ADIRecipe recipe)
        {
            m_MainWindow = mainwindow;
            m_recipe = recipe;

            InitViewModel();
            DialogInit(m_MainWindow);
        }

        void InitViewModel()
        {
            p_recipeManager_ViewModel = new RecipeManager_VM(m_recipe);
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
                        m_MainWindow.Dispatcher.BeginInvoke(new ThreadStart(() =>
                        {
                            BitmapImage imgLeftTop = new BitmapImage(new Uri(moduleRun.m_grabMode.p_sTempLeftTopAlignKeyFile));
                            BitmapImage imgLeftBottom = new BitmapImage(new Uri(moduleRun.m_grabMode.p_sTempLeftBottomAlignKeyFile));

                            // Information의 AlignKey 표시
                            p_bmpLeftTopAlignKeySrc = imgLeftTop;
                            p_bmpLeftBottomAlignKeySrc = imgLeftBottom;

                            // RecipeWizard의 AlignKey 표시
                            p_recipeManager_ViewModel.p_bmpLeftTopAlignKeySrc = imgLeftTop;
                            p_recipeManager_ViewModel.p_bmpLeftBottomAlignKeySrc = imgLeftBottom;
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
                            p_SnapDispText = val.ToString("P0");
                        });
                    }
                    break;
                case Vision.LineScanStatus.LineInspCompleted:
                    {
                        int[] arrData = (int[])data;
                        int nTotalInspNum = arrData[0];
                        int nCurInspNum = arrData[1];
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            double val = (double)nCurInspNum / nTotalInspNum;
                            p_InspProgressValue = val;
                            p_InspDispText = val.ToString("P0");
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
