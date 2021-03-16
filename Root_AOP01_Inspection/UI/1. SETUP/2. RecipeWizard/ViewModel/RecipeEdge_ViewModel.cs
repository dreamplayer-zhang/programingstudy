using System.Collections.Generic;
using System.Windows.Input;
using Root_AOP01_Inspection.Module;
using RootTools;
using RootTools_Vision;

namespace Root_AOP01_Inspection
{
	public class RecipeEdge_ViewModel : ObservableObject
	{
		Setup_ViewModel m_Setup;
		AOP01_Engineer m_Engineer;



		private RecipeEdge_Viewer_ViewModel m_ImageViewerLeft_VM;
		public RecipeEdge_Viewer_ViewModel p_ImageViewerLeft_VM
		{
			get
			{
				return m_ImageViewerLeft_VM;
			}
			set
			{
				SetProperty(ref m_ImageViewerLeft_VM, value);
			}
		}
		private RecipeEdge_Viewer_ViewModel m_ImageViewerTop_VM;
		public RecipeEdge_Viewer_ViewModel p_ImageViewerTop_VM
		{
			get
			{
				return m_ImageViewerTop_VM;
			}
			set
			{
				SetProperty(ref m_ImageViewerTop_VM, value);
			}
		}
		private RecipeEdge_Viewer_ViewModel m_ImageViewerRight_VM;
		public RecipeEdge_Viewer_ViewModel p_ImageViewerRight_VM
		{
			get
			{
				return m_ImageViewerRight_VM;
			}
			set
			{
				SetProperty(ref m_ImageViewerRight_VM, value);
			}
		}
		private RecipeEdge_Viewer_ViewModel m_ImageViewerBot_VM;
		public RecipeEdge_Viewer_ViewModel p_ImageViewerBot_VM
		{
			get
			{
				return m_ImageViewerBot_VM;
			}
			set
			{
				SetProperty(ref m_ImageViewerBot_VM, value);
			}
		}
		#region EdgeDrawMode
		private bool _EdgeDrawMode;
		public bool EdgeDrawMode
		{
			get
			{
				return _EdgeDrawMode;
			}
			set
			{
				if (_EdgeDrawMode == value)
					return;

				if (m_ImageViewerLeft_VM != null)
				{
					m_ImageViewerLeft_VM.EdgeDrawMode = value;
					if (value)
					{
						m_ImageViewerLeft_VM.Clear();
					}
				}
				if (m_ImageViewerRight_VM != null)
				{
					m_ImageViewerRight_VM.EdgeDrawMode = value;
					if (value)
					{
						m_ImageViewerRight_VM.Clear();
					}
				}
				if (m_ImageViewerTop_VM != null)
				{
					m_ImageViewerTop_VM.EdgeDrawMode = value;
					if (value)
					{
						m_ImageViewerTop_VM.Clear();
					}
				}
				if (m_ImageViewerBot_VM != null)
				{
					m_ImageViewerBot_VM.EdgeDrawMode = value;
					if (value)
					{
						m_ImageViewerBot_VM.Clear();
					}
				}
				SetProperty(ref _EdgeDrawMode, value);
			}
		}

		#endregion

		public RecipeEdge_ViewModel(Setup_ViewModel setup)
		{
			m_Setup = setup;
			m_Engineer = GlobalObjects.Instance.Get<AOP01_Engineer>();

			p_ImageViewerTop_VM = new RecipeEdge_Viewer_ViewModel(App.SideTopRegName);
			p_ImageViewerTop_VM.init(GlobalObjects.Instance.GetNamed<ImageData>(App.SideTopRegName), GlobalObjects.Instance.Get<DialogService>());
			p_ImageViewerLeft_VM = new RecipeEdge_Viewer_ViewModel(App.SideLeftRegName);
			p_ImageViewerLeft_VM.init(GlobalObjects.Instance.GetNamed<ImageData>(App.SideLeftRegName), GlobalObjects.Instance.Get<DialogService>());
			p_ImageViewerRight_VM = new RecipeEdge_Viewer_ViewModel(App.SideRightRegName);
			p_ImageViewerRight_VM.init(GlobalObjects.Instance.GetNamed<ImageData>(App.SideRightRegName), GlobalObjects.Instance.Get<DialogService>());
			p_ImageViewerBot_VM = new RecipeEdge_Viewer_ViewModel(App.SideBotRegName);
			p_ImageViewerBot_VM.init(GlobalObjects.Instance.GetNamed<ImageData>(App.SideBotRegName), GlobalObjects.Instance.Get<DialogService>());
		}

		private void saveCurrentEdge()
		{
			MainVision mainVision = ((AOP01_Handler)m_Engineer.ClassHandler()).m_mainVision;

			//bot
			if (m_ImageViewerBot_VM.TRectList.Count == 6)
			{
				mainVision.SetRectInfo(m_ImageViewerBot_VM.TRectList, App.SideBotModuleName);
			}
			//top
			if (m_ImageViewerTop_VM.TRectList.Count == 6)
			{
				mainVision.SetRectInfo(m_ImageViewerTop_VM.TRectList, App.SideTopModuleName);
			}
			//left
			if (m_ImageViewerLeft_VM.TRectList.Count == 6)
			{
				mainVision.SetRectInfo(m_ImageViewerLeft_VM.TRectList, App.SideLeftModuleName);
			}
			//right
			if (m_ImageViewerRight_VM.TRectList.Count == 6)
			{
				mainVision.SetRectInfo(m_ImageViewerRight_VM.TRectList, App.SideRightModuleName);
			}
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
					MainVision.Run_GrabSideScan grab = (MainVision.Run_GrabSideScan)mainVision.CloneModuleRun("Run Side Scan");
					mainVision.StartRun(grab);
				});
			}
		}
		public ICommand commandSaveEdgeBox
		{
			get
			{
				return new RelayCommand(saveCurrentEdge);
			}
		}
	}
}
