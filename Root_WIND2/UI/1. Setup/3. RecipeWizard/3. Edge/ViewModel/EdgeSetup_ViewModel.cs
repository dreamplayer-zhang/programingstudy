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

		private DrawTool_ViewModel drawToolVM;
		public DrawTool_ViewModel DrawToolVM
		{
			get { return drawToolVM; }
			set { SetProperty(ref drawToolVM, value); }
		}
		
		public ICommand btnTop { get { return new RelayCommand(() => ChangeViewer("Top")); }}
		public ICommand btnSide { get { return new RelayCommand(() => ChangeViewer("Side")); }}
		public ICommand btnBottom { get { return new RelayCommand(() => ChangeViewer("Bottom")); }}

		private int roiHeight;
		public int RoiHeight { get => roiHeight; set => roiHeight = value; }
		private int defectSizeMin;
		public int DefectSizeMin { get => defectSizeMin; set => defectSizeMin = value; }

		public EdgeSetup_ViewModel(Setup_ViewModel _setup)
		{
			setupVM = _setup;
			engineer = _setup.m_MainWindow.m_engineer;
		}

		public void Init()
		{
			DrawToolVM = new DrawTool_ViewModel(setupVM.m_MainWindow.m_engineer.m_handler.m_edgesideVision.GetMemoryData(Module.EdgeSideVision.eMemData.EdgeTop), setupVM.m_MainWindow.dialogService);
		}

		public void Scan()
		{
			EQ.p_bStop = false;
			EdgeSideVision edgeSideVision = ((WIND2_Handler)engineer.ClassHandler()).m_edgesideVision;
			if (edgeSideVision.p_eState != ModuleBase.eState.Ready)
			{
				MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
				return;
			}

			EdgeSideVision.Run_GrabEdge grab = (EdgeSideVision.Run_GrabEdge)edgeSideVision.CloneModuleRun("GrabEdge");
			edgeSideVision.StartRun(grab);
		}

		public void Inspect()
		{
			DrawToolVM.Clear();

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

			setupVM.InspectionManagerEFEM.SharedBuffer = sharedBuf;
			setupVM.InspectionManagerEFEM.InspectionMode = InspectionManager_EFEM.InsepectionMode.EDGE;
			setupVM.InspectionManagerEFEM.SharedBufferByteCnt = DrawToolVM.p_ImageData.p_nByte;
			setupVM.InspectionManagerEFEM.CreateInspection_Edgeside();
			setupVM.InspectionManagerEFEM.Start();
		}

		private void ChangeViewer(string dataName)
		{
			if (dataName == "Top")
				DrawToolVM.ChangeImageData(setupVM.m_MainWindow.m_engineer.m_handler.m_edgesideVision.GetMemoryData(Module.EdgeSideVision.eMemData.EdgeTop), setupVM.m_MainWindow.dialogService);
			else if (dataName == "Side")
				DrawToolVM.ChangeImageData(setupVM.m_MainWindow.m_engineer.m_handler.m_edgesideVision.GetMemoryData(Module.EdgeSideVision.eMemData.EdgeSide), setupVM.m_MainWindow.dialogService);
			else if (dataName == "Bottom")
				DrawToolVM.ChangeImageData(setupVM.m_MainWindow.m_engineer.m_handler.m_edgesideVision.GetMemoryData(Module.EdgeSideVision.eMemData.EdgeBottom), setupVM.m_MainWindow.dialogService);
		}

		private void ClearDefectData()
		{

		}
	}
}
