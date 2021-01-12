using Root_WIND2.Module;
using RootTools;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Root_WIND2
{
	public class EdgeSetup_ViewModel : ObservableObject
	{
		private WIND2_Engineer engineer;
		private Setup_ViewModel setupVM;

		private RootViewer_ViewModel drawToolVM;
		public RootViewer_ViewModel DrawToolVM
		{
			get { return drawToolVM; }
			set { SetProperty(ref drawToolVM, value); }
		}
		
		public ICommand btnTop { get { return new RelayCommand(() => ChangeViewer("Top")); }}
		public ICommand btnSide { get { return new RelayCommand(() => ChangeViewer("Side")); }}
		public ICommand btnBottom { get { return new RelayCommand(() => ChangeViewer("Bottom")); }}

		private int roiHeight;
		public int ROIHeight { get => roiHeight; set => roiHeight = value; }
		private int roiWidth;
		public int ROIWidth { get => roiWidth; set => roiWidth = value; }
		private int threshold;
		public int Threshold { get => threshold; set => threshold = value; }
		private int defectSizeMin;
		public int DefectSizeMin { get => defectSizeMin; set => defectSizeMin = value; }

		public EdgeSetup_ViewModel(Setup_ViewModel _setup)
		{
			setupVM = _setup;
			engineer = ProgramManager.Instance.Engineer;
		}

		public void Init()
		{
			DrawToolVM = new RootViewer_ViewModel();
			DrawToolVM.init(ProgramManager.Instance.GetEdgeMemory(EdgeSideVision.EDGE_TYPE.EdgeTop));
			//DrawToolVM.SetImageData(ProgramManager.Instance.GetEdgeMemory(EdgeSideVision.EDGE_TYPE.EdgeTop));
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
			setupVM.InspectionManagerEdge.InspectionMode = InspectionManagerEdge.InsepectionMode.EDGE;
			setupVM.InspectionManagerEdge.SharedBufferByteCnt = DrawToolVM.p_ImageData.p_nByte;
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
		}

		private void ClearDefectData()
		{

		}
	}
}
