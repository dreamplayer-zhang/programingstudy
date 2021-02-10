using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_WIND2.UI_User
{
    public class BacksideInspect_ViewModel : ObservableObject, IPage
    {
        #region [ColorDefines]
        private class ImageViewerColorDefines
        {
            public static SolidColorBrush MasterPosition = Brushes.Magenta;
            public static SolidColorBrush MasterPostionMove = Brushes.Yellow;
            public static SolidColorBrush ChipPosition = Brushes.Blue;
            public static SolidColorBrush ChipPositionMove = Brushes.Yellow;
            public static SolidColorBrush PostionFail = Brushes.Red;
            public static SolidColorBrush Defect = Brushes.Red;
        }

        private class MapViewerColorDefines
        {
            public static SolidColorBrush NoChip = Brushes.LightGray;
            public static SolidColorBrush Normal = Brushes.DimGray;
            public static SolidColorBrush Snap = Brushes.LightSkyBlue;
            public static SolidColorBrush Position = Brushes.SkyBlue;
            public static SolidColorBrush Inspection = Brushes.Gold;
            public static SolidColorBrush ProcessDefect = Brushes.YellowGreen;
            public static SolidColorBrush ProcessDefectWafer = Brushes.Green;

            public static SolidColorBrush GetWorkplaceStateColor(WORK_TYPE state)
            {
                switch (state)
                {
                    case WORK_TYPE.NONE:
                        return Normal;
                    case WORK_TYPE.SNAP:
                        return Snap;
                    case WORK_TYPE.ALIGNMENT:
                        return Position;
                    case WORK_TYPE.INSPECTION:
                        return Inspection;
                    case WORK_TYPE.DEFECTPROCESS:
                        return ProcessDefect;
                    case WORK_TYPE.DEFECTPROCESS_ALL:
                        return ProcessDefectWafer;
                    default:
                        return Normal;
                }
            }
        }
        #endregion

        public BacksideInspect_ViewModel()
        {

        }

        public void LoadRecipe()
        {

        }

        #region [Properties]
        private BacksideInspect_ImageViewer_ViewModel imageViewerVM;
        public BacksideInspect_ImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
            set
            {
                SetProperty<BacksideInspect_ImageViewer_ViewModel>(ref this.imageViewerVM, value);
            }
        }

        private MapViewer_ViewModel mapViewerVM;
        public MapViewer_ViewModel MapViewerVM
        {
            get => this.mapViewerVM;
            set
            {
                SetProperty<MapViewer_ViewModel>(ref this.mapViewerVM, value);
            }
        }
        #endregion

        #region [Command]
        public RelayCommand LoadedCommand
        {
            get => new RelayCommand(() =>
            {
                LoadRecipe();

                WorkEventManager.WorkplaceStateChanged += WorkplaceStateChanged_Callback;
            });
        }
        public RelayCommand UnloadedCommand
        {
            get => new RelayCommand(() =>
            {
                WorkEventManager.WorkplaceStateChanged -= WorkplaceStateChanged_Callback;
            });
        }

        public void WorkplaceStateChanged_Callback(object obj, WorkplaceStateChangedEventArgs args)
        {
            Workplace wp = args.workplace;

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                this.MapViewerVM.SetChipColor(wp.MapIndexX, wp.MapIndexY, MapViewerColorDefines.GetWorkplaceStateColor(wp.WorkState));
            }));
        }

        public RelayCommand btnStart
        {
            get => new RelayCommand(() =>
            {
                //this.ImageViewerVM.ClearObjects();
                GlobalObjects.Instance.Get<InspectionManagerBackside>().Start(WORK_TYPE.SNAP);
            });
        }

        public RelayCommand btnSnap
        {
            get => new RelayCommand(() =>
            {

            });
        }

        public RelayCommand btnStop
        {
            get => new RelayCommand(() =>
            {

            });
        }

        public RelayCommand btnClear
        {
            get => new RelayCommand(() =>
            {
                //this.ImageViewerVM.ClearObjects();
            });
        }

        public RelayCommand btnRemote
        {
            get => new RelayCommand(() =>
            {

            });
        }

        #endregion
    }
}
