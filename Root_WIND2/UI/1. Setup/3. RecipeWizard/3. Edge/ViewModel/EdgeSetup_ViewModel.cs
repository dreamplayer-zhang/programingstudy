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
		private Setup_ViewModel setupVM;
		private DrawTool_ViewModel m_DrawTool_VM;
		public DrawTool_ViewModel p_DrawTool_VM
		{
			get
			{
				return m_DrawTool_VM;
			}
			set
			{
				SetProperty(ref m_DrawTool_VM, value);
			}
		}
		private WIND2_Engineer engineer;

		private int roiHeight;
		private int defectSizeMin;
		public int RoiHeight { get => roiHeight; set => roiHeight = value; }
		public int DefectSizeMin { get => defectSizeMin; set => defectSizeMin = value; }

		public EdgeSetup_ViewModel(Setup_ViewModel _setup)
		{
			setupVM = _setup;
			engineer = _setup.m_MainWindow.m_engineer;
		}

		public void Init()
		{
			p_DrawTool_VM = new DrawTool_ViewModel(setupVM.m_MainWindow.m_engineer.m_handler.m_edgesideVision.GetMemoryData(Module.EdgeSideVision.eMemData.EdgeTop), setupVM.m_MainWindow.dialogService);
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
			p_DrawTool_VM.Clear();

			IntPtr sharedBuf = new IntPtr();
			if (p_DrawTool_VM.p_ImageData.p_nByte == 3)
			{
				if (p_DrawTool_VM.p_eColorViewMode != RootViewer_ViewModel.eColorViewMode.All)
					sharedBuf = p_DrawTool_VM.p_ImageData.GetPtr((int)p_DrawTool_VM.p_eColorViewMode - 1);
				else // All 일때는 R채널로...
					sharedBuf = p_DrawTool_VM.p_ImageData.GetPtr(0);
			}
			else
			{
				sharedBuf = p_DrawTool_VM.p_ImageData.GetPtr();
			}

			setupVM.InspectionManagerEFEM.SharedBuffer = sharedBuf;
			setupVM.InspectionManagerEFEM.InspectionMode = InspectionManager_EFEM.InsepectionMode.EDGE;
			setupVM.InspectionManagerEFEM.SharedBufferByteCnt = p_DrawTool_VM.p_ImageData.p_nByte;
			setupVM.InspectionManagerEFEM.CreateInspection_Edgeside();
			setupVM.InspectionManagerEFEM.Start();
		}

	}
}
