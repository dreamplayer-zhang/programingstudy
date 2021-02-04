using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_WIND2.UI_User
{
    public class FrontsideInspect_ViewModel : ObservableObject, IPage
    {
        #region [ColorDefines]
        private class ColorDefines
        {
            public static SolidColorBrush MasterPosition = Brushes.Magenta;
            public static SolidColorBrush MasterPostionMove = Brushes.Yellow;
            public static SolidColorBrush ChipPosition = Brushes.Blue;
            public static SolidColorBrush ChipPositionMove = Brushes.Yellow;
            public static SolidColorBrush PostionFail = Brushes.Red;
            public static SolidColorBrush Defect = Brushes.Red;
        }
        #endregion

        private FrontsideInspect_ImageViewer_ViewModel imageViewerVM;
        public FrontsideInspect_ImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
        }

        public FrontsideInspect_ViewModel()
        {
            this.imageViewerVM = new FrontsideInspect_ImageViewer_ViewModel();
            this.imageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("FrontImage"), GlobalObjects.Instance.Get<DialogService>());


            WorkEventManager.PositionDone += PositionDone_Callback;
            WorkEventManager.InspectionDone += InspectionDone_Callback;
            //WorkEventManager.ProcessDefectDone += ProcessDefectDone_Callback;
        }

        public void SetPage()
        {
            LoadRecipe();
        }

        public void LoadRecipe()
        {
            // 맵 불러오기
        }

        #region [Command]

        public RelayCommand btnStart
        {
            get => new RelayCommand(() =>
            {
                this.ImageViewerVM.ClearObjects();
                GlobalObjects.Instance.Get<InspectionManagerFrontside>().Start(WORK_TYPE.SNAP);
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
                GlobalObjects.Instance.Get<InspectionManagerFrontside>().Stop();
            });
        }

        public RelayCommand btnClear
        {
            get => new RelayCommand(() =>
            {
                this.ImageViewerVM.ClearObjects();
            });
        }
        #endregion

        #region [Callback]
        object lockObj = new object();
        private void PositionDone_Callback(object obj, PositionDoneEventArgs args)
        {
            Workplace workplace = obj as Workplace;
            lock (this.lockObj)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    String test = "";
                    if (workplace.Index == 0)
                    {
                        test += "Trans : {" + workplace.OffsetX.ToString() + ", " + workplace.OffsetX.ToString() + "}" + "\n";
                        DrawRectMasterFeature(args.ptOldStart, args.ptOldEnd, args.ptNewStart, args.ptNewEnd, test, args.bSuccess);
                    }
                    else
                    {
                        test += "Trans : {" + workplace.TransX.ToString() + ", " + workplace.TransY.ToString() + "}" + "\n";
                        DrawRectChipFeature(args.ptOldStart, args.ptOldEnd, args.ptNewStart, args.ptNewEnd, test, args.bSuccess);
                    }
                }));
            }
        }
        private void InspectionDone_Callback(object obj, InspectionDoneEventArgs args)
        {
            Workplace workplace = obj as Workplace;
            List<String> textList = new List<String>();
            List<CRect> rectList = new List<CRect>();


            foreach (RootTools.Database.Defect defectInfo in workplace.DefectList)
            {
                String text = "";

                rectList.Add(new CRect((int)defectInfo.p_rtDefectBox.Left, (int)defectInfo.p_rtDefectBox.Top, (int)defectInfo.p_rtDefectBox.Right, (int)defectInfo.p_rtDefectBox.Bottom));
                textList.Add(text);
            }

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                DrawRectDefect(rectList, textList, args.reDraw);
            }));
        }
        #endregion

        #region [ImageView Draw Method]

        public void DrawRectMasterFeature(CPoint ptOldStart, CPoint ptOldEnd, CPoint ptNewStart, CPoint ptNewEnd, String text, bool bSuccess)
        {
            ImageViewerVM.AddDrawRect(ptOldStart, ptOldEnd, ColorDefines.MasterPosition);
            ImageViewerVM.AddDrawRect(ptNewStart, ptNewEnd, bSuccess ? ColorDefines.MasterPostionMove: ColorDefines.PostionFail);
            //ImageViewerVM.DrawText(ptNew)
        }

        public void DrawRectChipFeature(CPoint ptOldStart, CPoint ptOldEnd, CPoint ptNewStart, CPoint ptNewEnd, String text, bool bSuccess)
        {
            ImageViewerVM.AddDrawRect(ptOldStart, ptOldEnd, ColorDefines.ChipPosition);
            ImageViewerVM.AddDrawRect(ptNewStart, ptNewEnd, bSuccess ? ColorDefines.ChipPositionMove : ColorDefines.PostionFail);
        }
        public void DrawRectDefect(List<CRect> rectList, List<String> text, bool reDraw = false)
        {
            ImageViewerVM.AddDrawRectList(rectList, ColorDefines.Defect);
        }
        #endregion
    }
}
