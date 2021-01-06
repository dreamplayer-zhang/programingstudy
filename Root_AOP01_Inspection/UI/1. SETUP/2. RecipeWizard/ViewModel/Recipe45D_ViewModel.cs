using System;
using System.Windows.Input;
using Root_AOP01_Inspection.Module;
using RootTools;

namespace Root_AOP01_Inspection
{
	class Recipe45D_ViewModel : ObservableObject
	{
		Setup_ViewModel m_Setup;
		AOP01_Engineer m_Engineer;


		private Recipe45D_ImageViewer_ViewModel m_ImageViewer_VM;
		public Recipe45D_ImageViewer_ViewModel p_ImageViewer_VM
		{
			get
			{
				return m_ImageViewer_VM;
			}
			set
			{
				SetProperty(ref m_ImageViewer_VM, value);
			}
		}
		public Recipe45D_ViewModel(Setup_ViewModel setup)
		{
			m_Setup = setup;
			m_Engineer = setup.m_MainWindow.m_engineer;

			p_ImageViewer_VM = new Recipe45D_ImageViewer_ViewModel();
			p_ImageViewer_VM.init(ProgramManager.Instance.Image);
			p_ImageViewer_VM.DrawDone += DrawDone_Callback;
		}

		private void DrawDone_Callback(CPoint leftTop, CPoint rightBottom)
		{
			p_ImageViewer_VM.Clear();
			this.m_ImageViewer_VM.DrawRect(leftTop, rightBottom,  Recipe45D_ImageViewer_ViewModel.ColorType.Defect);
		}
		public ICommand commandInspTest
		{
			get
			{
				return new RelayCommand(startTestInsp);
			}
		}

		private void startTestInsp()
		{
			//ProgramManager.Instance.Engineer.InspectionManager.Recipe
			var test = new RootTools_Vision.Recipe();
			test.WaferMap = new RootTools_Vision.RecipeType_WaferMap(40, 40, new int[40*40]);
			ProgramManager.Instance.Engineer.InspectionManager.Recipe = test;
			ProgramManager.Instance.Engineer.InspectionManager.CreateInspecion();
		}

		public ICommand btnBack
		{
			get
			{
				return new RelayCommand(() =>
				{
					m_Setup.Set_RecipeWizardPanel();
				});
			}
		}
		public ICommand btnSnap
		{
			get
			{
				return new RelayCommand(() =>
				{
					MainVision mainVision = ((AOP01_Handler)m_Engineer.ClassHandler()).m_mainVision;
					MainVision.Run_Grab45 grab = (MainVision.Run_Grab45)mainVision.CloneModuleRun("Run Grab 45");
					mainVision.StartRun(grab);
				});
			}
		}
	}
}
