using Root_VEGA_P_Vision.Engineer;
using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Memory;
using RootTools.Module;
using RootTools_CLR;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{
    public class PositionViewerTab_ViewModel:ObservableObject
    {
        public PositionViewerTab_Panel Main;
        public PositionFeature curTab;
        PositionImageViewer_ViewModel mEIPCoverTop, mEIPCoverBtm, mEIPBaseTop, mEIPBaseBtm;
        #region Property
        public PositionImageViewer_ViewModel p_EIPCoverTop
        {
            get => mEIPCoverTop;
            set => SetProperty(ref mEIPCoverTop, value);
        }
        public PositionImageViewer_ViewModel p_EIPCoverBtm
        {
            get => mEIPCoverBtm;
            set => SetProperty(ref mEIPCoverBtm, value);
        }
        public PositionImageViewer_ViewModel p_EIPBaseTop
        {
            get => mEIPBaseTop;
            set => SetProperty(ref mEIPBaseTop, value);
        }
        public PositionImageViewer_ViewModel p_EIPBaseBtm
        {
            get => mEIPBaseBtm;
            set => SetProperty(ref mEIPBaseBtm, value);
        }
        #endregion

        public PositionViewerTab_ViewModel()
        { 
            Main = new PositionViewerTab_Panel();
            Main.DataContext = this;
            mEIPCoverTop = new PositionImageViewer_ViewModel("EIP_Cover.Main.Front");
            mEIPCoverBtm = new PositionImageViewer_ViewModel("EIP_Cover.Main.Front");
            mEIPBaseTop = new PositionImageViewer_ViewModel("EIP_Cover.Main.Front");
            mEIPBaseBtm = new PositionImageViewer_ViewModel("EIP_Cover.Main.Front");

            //ImageData CoverTopimage = GlobalObjects.Instance.GetNamed<ImageData>("EIP_Cover.Main.Front");
            //ImageData CoverBtmimage = GlobalObjects.Instance.GetNamed<ImageData>("EIP_Cover.Main.Back");
            //ImageData BaseTopimage = GlobalObjects.Instance.GetNamed<ImageData>("EIP_Plate.Main.Front");
            //ImageData BaseBtmimage = GlobalObjects.Instance.GetNamed<ImageData>("EIP_Plate.Main.Back");

            //p_EIPCoverTop.init(CoverTopimage,GlobalObjects.Instance.Get<DialogService>());
            //p_EIPCoverBtm.init(CoverBtmimage, GlobalObjects.Instance.Get<DialogService>());
            //p_EIPBaseTop.init(BaseTopimage, GlobalObjects.Instance.Get<DialogService>());
            //p_EIPBaseBtm.init(BaseBtmimage, GlobalObjects.Instance.Get<DialogService>());

            selectedViewer = mEIPCoverTop;
            curTab = PositionFeature.COVERTOP;
        }

        private void MEIPBaseBtm_FeatureBoxDone(object e)
        {
         
            MessageBox.Show(e.ToString());
        }

        public void UpdateOriginBox()
        {
            p_EIPCoverTop.RedrawOriginBox();
            p_EIPCoverBtm.RedrawOriginBox();
            p_EIPBaseTop.RedrawOriginBox();
            p_EIPBaseBtm.RedrawOriginBox();
        }

        #region ICommand
        public PositionImageViewer_ViewModel selectedViewer { get; set; } = new PositionImageViewer_ViewModel("EIP_Cover.Main.Front");
        int selectedIdx;
        public ICommand TabChanged
        {
            get => new RelayCommand(() => {
                curTab = (PositionFeature)Main.ViewerTab.SelectedIndex;
                switch (curTab)
                {
                    case PositionFeature.COVERTOP:
                        selectedViewer = p_EIPCoverTop;
                        break;
                    case PositionFeature.COVERBTM:
                        selectedViewer = p_EIPCoverBtm;
                        break;
                    case PositionFeature.BASETOP:
                        selectedViewer = p_EIPBaseTop;
                        break;
                    case PositionFeature.BASEBTM:
                        selectedViewer = p_EIPBaseBtm;
                        break;
                }
            });
        }
        public ICommand ImageOpen
        {
            get => new RelayCommand(() => selectedViewer._openImage());
        }
        public ICommand ImageSave
        {
            get => new RelayCommand(() => selectedViewer._saveImage());
        }
        public ICommand ImageClear
        {
            get => new RelayCommand(() => selectedViewer._clearImage());
        }
        public ICommand btnSnap
        {
            get => new RelayCommand(() => Snap());
        }
        public ICommand btnAlignTest
        {
            get => new RelayCommand(() => ManualAlign());
        }

        public class Result
        {
            public CPoint Pos;
            public double Score;

            public Result(CPoint Pos, double Score)
            {
                this.Pos = Pos;
                this.Score = Score;
            }
        }

        unsafe double ManualAlign()
        {
            EUVPositionRecipe positionRecipe = GlobalObjects.Instance.Get<RecipeVision>().GetItem<EUVPositionRecipe>();
            ImageData mem = GlobalObjects.Instance.GetNamed<ImageData>("EIP_Cover.Main.Front");
            byte* srcPtr = (byte*)mem.GetPtr().ToPointer();

            List<Result> resli = new List<Result>();
            int idx = 0;

            int trigger = 50;
            int firstedge=0, secondEdge=0;
            CPoint Abspt = new CPoint(0,0);
            int[] arr = new int[positionRecipe.EIPCoverTopFeature.ListAlignFeature.Count];
            foreach (RecipeType_ImageData template in positionRecipe.EIPCoverTopFeature.ListAlignFeature)
            {
                idx++;
                int posX = 0, posY = 0; //results
                template.Save(@"C:\AlignFeature\"+idx+".bmp");
                Abspt = new CPoint(template.PositionX, template.PositionY);
                //double result = CLR_IP.Cpp_TemplateMatching(srcPtr, template.RawData, &posX, &posY,
                //    mem.p_Size.X, mem.p_Size.Y, template.Width, template.Height, Abspt.X - trigger, Abspt.Y - trigger, Abspt.X + template.Width +trigger, Abspt.Y+template.Height+trigger, 5, 1, 0);

                firstedge = CLR_IP.Cpp_FindEdge(template.RawData, template.Width, template.Height, Abspt.X - trigger, Abspt.Y - trigger, Abspt.X + template.Width + trigger, Abspt.Y + template.Height + trigger, 0, 50);

                arr[idx] = firstedge;
            }

            return CalcAngle(new CPoint(arr[0], Abspt.Y), new CPoint(arr[1], Abspt.Y));
        }
        public CPoint ConvertRelToAbs(CPoint ptRel)
        {
            EUVOriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeVision>().GetItem<EUVOriginRecipe>();

            return new CPoint(originRecipe.TDIOrigin.Origin.X + ptRel.X, originRecipe.TDIOrigin.Origin.Y - originRecipe.TDIOrigin.OriginSize.Y + ptRel.Y);
        }
        private double CalcAngle(CPoint firstPos, CPoint secondPos)
        {
            double radian = Math.Atan2(secondPos.Y - firstPos.Y, secondPos.X - firstPos.X);
            double angle = radian * (180 / Math.PI);
            double resAngle;
            if (secondPos.Y - firstPos.Y < 0)
                resAngle = angle + 180;
            else
                resAngle = angle - 180;

            return resAngle;
        }

        void Snap()
        {
            EQ.p_bStop = false;
            Vision vision = GlobalObjects.Instance.Get<VEGA_P_Vision_Engineer>().m_handler.m_vision;
            if (vision.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
        }
        #endregion
    }
}
