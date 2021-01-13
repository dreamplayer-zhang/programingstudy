using Root_WIND2.Module;
using RootTools;
using RootTools.Module;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Root_WIND2
{
	public class EdgesideSetup_ViewModel : ObservableObject
	{
		Recipe recipe;
		private WIND2_Engineer engineer;
		private Setup_ViewModel setupVM;
		private RootViewer_ViewModel drawToolVM;

		private int roiHeight_Top;
		private int roiWidth_Top;
		private int threshold_Top;
		private int defectSizeMin_Top;
		private int mergeDist_Top;
		private int whiteIllum_Top;
		private int sideIllum_Top;
		
		private int roiHeight_Side;
		private int roiWidth_Side;
		private int threshold_Side;
		private int defectSizeMin_Side;
		private int mergeDist_Side;
		private int whiteIllum_Side;
		private int sideIllum_Side;

		private int roiHeight_Btm;
		private int roiWidth_Btm;
		private int threshold_Btm;
		private int defectSizeMin_Btm;
		private int mergeDist_Btm;
		private int whiteIllum_Btm;
		private int sideIllum_Btm;

		#region [Getter / Setter]
		public RootViewer_ViewModel DrawToolVM
		{
			get { return drawToolVM; }
			set { SetProperty(ref drawToolVM, value); }
		}

		public int ROIHeight_Top { get => roiHeight_Top; set => roiHeight_Top = value; }
		public int ROIWidth_Top { get => roiWidth_Top; set => roiWidth_Top = value; }
		public int Threshold_Top { get => threshold_Top; set => threshold_Top = value; }
		public int DefectSizeMin_Top { get => defectSizeMin_Top; set => defectSizeMin_Top = value; }
		public int MergeDist_Top { get => mergeDist_Top; set => mergeDist_Top = value; }
		public int WhiteIllum_Top { get => whiteIllum_Top; set => whiteIllum_Top = value; }
		public int SideIllum_Top { get => sideIllum_Top; set => sideIllum_Top = value; }

		public int ROIHeight_Side { get => roiHeight_Side; set => roiHeight_Side = value; }
		public int ROIWidth_Side { get => roiWidth_Side; set => roiWidth_Side = value; }
		public int Threshold_Side { get => threshold_Side; set => threshold_Side = value; }
		public int DefectSizeMin_Side { get => defectSizeMin_Side; set => defectSizeMin_Side = value; }
		public int MergeDist_Side { get => mergeDist_Side; set => mergeDist_Side = value; }

		public int WhiteIllum_Side { get => whiteIllum_Side; set => whiteIllum_Side = value; }
		public int SideIllum_Side { get => sideIllum_Side; set => sideIllum_Side = value; }

		public int ROIHeight_Btm { get => roiHeight_Btm; set => roiHeight_Btm = value; }
		public int ROIWidth_Btm { get => roiWidth_Btm; set => roiWidth_Btm = value; }
		public int Threshold_Btm { get => threshold_Btm; set => threshold_Btm = value; }
		public int DefectSizeMin_Btm { get => defectSizeMin_Btm; set => defectSizeMin_Btm = value; }
		public int MergeDist_Btm { get => mergeDist_Btm; set => mergeDist_Btm = value; }

		public int WhiteIllum_Btm { get => whiteIllum_Btm; set => whiteIllum_Btm = value; }
		public int SideIllum_Btm { get => sideIllum_Btm; set => sideIllum_Btm = value; }
		#endregion

		public ICommand btnTop { get { return new RelayCommand(() => ChangeViewer("Top")); }}
		public ICommand btnSide { get { return new RelayCommand(() => ChangeViewer("Side")); }}
		public ICommand btnBottom { get { return new RelayCommand(() => ChangeViewer("Bottom")); }}

		public EdgesideSetup_ViewModel()
		{
			engineer = ProgramManager.Instance.Engineer;
		}

		public void Init(Setup_ViewModel _setup, Recipe _recipe)
		{
			this.setupVM = _setup;
			this.recipe = _recipe;

			DrawToolVM = new RootViewer_ViewModel();
			DrawToolVM.init(ProgramManager.Instance.GetEdgeMemory(EdgeSideVision.EDGE_TYPE.EdgeTop));
		}

		public void Scan()
		{
			EQ.p_bStop = false;
			EdgeSideVision edgeSideVision = ((WIND2_Handler)engineer.ClassHandler()).p_EdgeSideVision;
			if (edgeSideVision.p_eState != ModuleBase.eState.Ready)
			{
				MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
				return;
			}

			Run_GrabEdge grab = (Run_GrabEdge)edgeSideVision.CloneModuleRun("GrabEdge");
			edgeSideVision.StartRun(grab);
		}

		public void Inspect()
		{
			IntPtr sharedBuf = new IntPtr();
			if (DrawToolVM.p_ImageData.p_nByte == 3)
			{
				if (DrawToolVM.p_eColorViewMode != RootViewer_ViewModel.eColorViewMode.All)
					sharedBuf = DrawToolVM.p_ImageData.GetPtr((int)DrawToolVM.p_eColorViewMode - 1);
				else // All 일때는 R채널로...
					sharedBuf = DrawToolVM.p_ImageData.GetPtr(0);
			}
			else
			{
				sharedBuf = DrawToolVM.p_ImageData.GetPtr();
			}

			setupVM.InspectionManagerEdge.SharedBufferR_Gray = sharedBuf;
			if (DrawToolVM.p_ImageData.p_nByte == 3)
			{
				setupVM.InspectionManagerEdge.SharedBufferG = DrawToolVM.p_ImageData.GetPtr(1);
				setupVM.InspectionManagerEdge.SharedBufferB = DrawToolVM.p_ImageData.GetPtr(2);
			}
			setupVM.InspectionManagerEdge.SharedBufferByteCnt = DrawToolVM.p_ImageData.p_nByte;
			setupVM.InspectionManagerEdge.InspectionMode = InspectionManagerEdge.InsepectionMode.EDGE;
			setupVM.InspectionManagerEdge.CreateInspection();
			setupVM.InspectionManagerEdge.Start();
		}

		private void ChangeViewer(string dataName)
		{
			if (dataName == "Top")
				DrawToolVM.init(ProgramManager.Instance.GetEdgeMemory(EdgeSideVision.EDGE_TYPE.EdgeTop), ProgramManager.Instance.DialogService);
			else if (dataName == "Side")
				DrawToolVM.init(ProgramManager.Instance.GetEdgeMemory(EdgeSideVision.EDGE_TYPE.EdgeSide), ProgramManager.Instance.DialogService);
			else if (dataName == "Bottom")
				DrawToolVM.init(ProgramManager.Instance.GetEdgeMemory(EdgeSideVision.EDGE_TYPE.EdgeBottom), ProgramManager.Instance.DialogService);
			
			SetParameter();
		}

		private void ClearDefectData()
		{


		}

		private void LoadParameter()
		{
			ROIHeight_Top = recipe.GetRecipe<EdgeSurfaceParameter>().RoiHeightTop;
			ROIWidth_Top = recipe.GetRecipe<EdgeSurfaceParameter>().RoiWidthTop;
			Threshold_Top = recipe.GetRecipe<EdgeSurfaceParameter>().ThesholdTop;
			DefectSizeMin_Top = recipe.GetRecipe<EdgeSurfaceParameter>().SizeMinTop;
			MergeDist_Top = recipe.GetRecipe<EdgeSurfaceParameter>().MergeDistTop;

			ROIHeight_Side = recipe.GetRecipe<EdgeSurfaceParameter>().RoiHeightSide;
			ROIWidth_Side = recipe.GetRecipe<EdgeSurfaceParameter>().RoiWidthSide;
			Threshold_Side = recipe.GetRecipe<EdgeSurfaceParameter>().ThesholdSide;
			DefectSizeMin_Side = recipe.GetRecipe<EdgeSurfaceParameter>().SizeMinSide;
			MergeDist_Side = recipe.GetRecipe<EdgeSurfaceParameter>().MergeDistSide;

			ROIHeight_Btm = recipe.GetRecipe<EdgeSurfaceParameter>().RoiHeightBtm;
			ROIWidth_Btm = recipe.GetRecipe<EdgeSurfaceParameter>().RoiWidthBtm;
			Threshold_Btm = recipe.GetRecipe<EdgeSurfaceParameter>().ThesholdBtm;
			DefectSizeMin_Btm = recipe.GetRecipe<EdgeSurfaceParameter>().SizeMinBtm;
			MergeDist_Btm = recipe.GetRecipe<EdgeSurfaceParameter>().MergeDistBtm;
		}

		private void SetParameter()
		{
			EdgeSurfaceParameter param = new EdgeSurfaceParameter();
			param.RoiHeightTop = ROIHeight_Top;
			param.RoiWidthTop = ROIWidth_Top;
			param.ThesholdTop = Threshold_Top;
			param.SizeMinTop = DefectSizeMin_Top;
			param.MergeDistTop = MergeDist_Top;

			param.RoiHeightSide = ROIHeight_Side;
			param.RoiWidthSide = ROIWidth_Side;
			param.ThesholdSide = Threshold_Side;
			param.SizeMinSide = DefectSizeMin_Side;
			param.MergeDistSide = MergeDist_Side;

			param.RoiHeightBtm = ROIHeight_Btm;
			param.RoiWidthBtm = ROIWidth_Btm;
			param.ThesholdBtm = Threshold_Btm;
			param.SizeMinBtm = DefectSizeMin_Btm;
			param.MergeDistBtm = MergeDist_Btm;

			recipe.ParameterItemList.Add(param);
		}
	}
}
