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
			//string[] keywords = new string[] { "Main", "SideLeft", "SideTop", "SideBot", "SideRight" };
			//현재 ViewModel에 있는 edgebox를 저장한다.
			throw new System.Exception();
			//MainVision mainVision = ((AOP01_Handler)m_Engineer.ClassHandler()).m_mainVision;
			//MainVision.Run_SurfaceInspection surfaceInspection = (MainVision.Run_SurfaceInspection)mainVision.CloneModuleRun("MainSurfaceInspection");
			//if (m_ImageViewerLeft_VM.TRectList.Count == 6)
			//{
			//	surfaceInspection.mainEdgeList[1] = new List<TRect>(m_ImageViewerLeft_VM.TRectList).ToArray();
			//}
			//if (m_ImageViewerTop_VM.TRectList.Count == 6)
			//{
			//	surfaceInspection.mainEdgeList[2] = new List<TRect>(m_ImageViewerTop_VM.TRectList).ToArray();
			//}
			//if (m_ImageViewerBot_VM.TRectList.Count == 6)
			//{
			//	surfaceInspection.mainEdgeList[3] = new List<TRect>(m_ImageViewerBot_VM.TRectList).ToArray();
			//}
			//if (m_ImageViewerRight_VM.TRectList.Count == 6)
			//{
			//	surfaceInspection.mainEdgeList[4] = new List<TRect>(m_ImageViewerRight_VM.TRectList).ToArray();
			//}
			//surfaceInspection.UpdateTree();
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
