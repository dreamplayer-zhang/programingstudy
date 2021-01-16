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

namespace Root_WIND2
{
	class EBRSetup_ViewModel : ObservableObject
	{
		Recipe recipe;
		private WIND2_Engineer engineer;
		private Setup_ViewModel setupVM;
		private RootViewer_ViewModel drawToolVM;

		#region [Getter / Setter]
		public RootViewer_ViewModel DrawToolVM
		{
			get { return drawToolVM; }
			set { SetProperty(ref drawToolVM, value); }
		}
		#endregion

		public EBRSetup_ViewModel()
		{
			engineer = ProgramManager.Instance.Engineer;
		}

		public void Init(Setup_ViewModel _setup)
		{
			this.setupVM = _setup;
			this.recipe = _setup.Recipe;

			DrawToolVM = new RootViewer_ViewModel();
			DrawToolVM.init(ProgramManager.Instance.GetEdgeMemory(EdgeSideVision.EDGE_TYPE.EBR), ProgramManager.Instance.DialogService);
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

			Run_GrabEBR grab = (Run_GrabEBR)edgeSideVision.CloneModuleRun("GrabEBR");
			edgeSideVision.StartRun(grab);
		}

		public void Inspect()
		{
			setupVM.InsperctionMgrEBR.SharedBufferR_Gray = DrawToolVM.p_ImageData.GetPtr();
			setupVM.InsperctionMgrEBR.SharedBufferByteCnt = DrawToolVM.p_ImageData.p_nByte;
			setupVM.InsperctionMgrEBR.InspectionMode = InspectionManagerEBR.InsepectionMode.EBR;

			if (setupVM.InsperctionMgrEBR.CreateInspection() == false)
			{
				return;
			}
			setupVM.InsperctionMgrEBR.Start();
		}

		public void LoadParameter()
		{

		}
	}

}
