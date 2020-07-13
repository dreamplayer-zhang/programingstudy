using ATI;
using Microsoft.Win32;
using RootTools;
using RootTools.Comm;
using RootTools.Inspects;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Root_Vega
{
	class _2_5_MainVisionViewModel : ObservableObject
	{
		/// <summary>
		/// 외부 Thread에서 UI를 Update하기 위한 Dispatcher
		/// </summary>
		protected Dispatcher _dispatcher;
		System.Threading.Timer resultTimer;
		Vega_Engineer m_Engineer;
		MemoryTool m_MemoryModule;
		ImageData m_Image;
		int currentTotalIdx;

		public _2_5_MainVisionViewModel(Vega_Engineer engineer, IDialogService dialogService)
		{
			_dispatcher = Dispatcher.CurrentDispatcher;
			m_Engineer = engineer;
			Init(engineer, dialogService);
		}
		~_2_5_MainVisionViewModel()
		{
			resultTimer.Dispose();
		}
		void Init(Vega_Engineer engineer, IDialogService dialogService)
		{
			m_MemoryModule = m_Engineer.ClassMemoryTool();
			m_ImageViewer = new ImageViewer_ViewModel(new ImageData(m_MemoryModule.GetMemory(App.sPatternPool, App.sPatternGroup, App.sPatternmem)), dialogService);

			if (m_MemoryModule != null)
			{

				m_DrawHistoryWorker = new DrawHistoryWorker();
				m_SimpleShapeDrawer = new SimpleShapeDrawerVM(m_ImageViewer);
				m_SimpleShapeDrawer.RectangleKeyValue = Key.D1;

				p_ImageViewer.SetDrawer(p_SimpleShapeDrawer);
				p_ImageViewer.m_HistoryWorker = m_DrawHistoryWorker;

				p_InformationDrawer = new InformationDrawer(p_ImageViewer);
			}
			m_Engineer.m_recipe.LoadComplete += () =>
			{
				SelectedRecipe = m_Engineer.m_recipe;
				p_PatternRoiList = new ObservableCollection<Roi>(m_Engineer.m_recipe.VegaRecipeData.RoiList.Where(x => x.RoiType == Roi.Item.ReticlePattern));
				StripParamList = new ObservableCollection<StripParamData>();

				_SelectedROI = null;

				SelectedParam = new StripParamData();//UI 초기화를 위한 코드
				SelectedParam = null;
			};

			m_Engineer.m_InspManager.nInspectionCount = 0;//Inspection total count 초기화. 임시 db에서 데이터값을 정상적으로 끌어올때 사용한다

			resultTimer = new System.Threading.Timer(checkAddDefect);
			resultTimer.Change(0, 1000);

			return;
		}
		private void checkAddDefect(object state)
		{
			//_dispatcher
			//concept
			//db에 주기적으로 접근하여 tempTable의 최대 개수를 확인
			//최대개수와 currentDefectIndex의 차이가 발생한다면 currentDefectIndex와 최대 개수 사이의 defect UI를 갱신하고 currentDefectIndex를 최대 개수로 변경한다

			DBConnector connector = new DBConnector("localhost", "Inspections", "root", "`ati5344");
			if (connector.Open())
			{
				string query = "SELECT COUNT(*) FROM inspections.tempdata;";
				string temp = string.Empty;
				var result = connector.SendQuery(query, ref temp);
#if DEBUG
				Debug.WriteLine(string.Format("tempdata Row Count CODE : {0}", result));
				Debug.WriteLine(string.Format("tempdata Row Result : {0}", temp));
#endif
				int count;
				if (int.TryParse(temp, out count))
				{
					if (currentTotalIdx < count)
					{
						//current index부터 count까지 defect정보를 가져와 UI에 update하고 current index를 업데이트한다

						//countQurey = string.Format("SELECT * FROM inspections.tempdata WHERE ");//우선순위를 낮춘다. SQLite파일하고 이미지 출력하는게 먼저

						currentTotalIdx = count;
					}
				}
			}
		}

		#region Property

		public DrawHistoryWorker m_DrawHistoryWorker;

		#region SelectedRecipe

		VegaRecipe _SelectedRecipe;
		public VegaRecipe SelectedRecipe
		{
			get { return this._SelectedRecipe; }
			set
			{
				SetProperty(ref _SelectedRecipe, value);
			}
		}
		#endregion

		#region p_PatternRoiList

		ObservableCollection<Roi> _p_PatternRoiList;
		public ObservableCollection<Roi> p_PatternRoiList
		{
			get { return _p_PatternRoiList; }
			set
			{
				SetProperty(ref _p_PatternRoiList, value);
			}
		}
		#endregion

		#region p_PatternFeatureList

		ObservableCollection<Feature> _PatternFeatureList;
		public ObservableCollection<Feature> p_PatternFeatureList
		{
			get { return _PatternFeatureList; }
			set
			{
				SetProperty(ref _PatternFeatureList, value);
			}
		}
		#endregion

		#region StripParamList

		ObservableCollection<StripParamData> _StripParamList;
		public ObservableCollection<StripParamData> StripParamList
		{
			get { return _StripParamList; }
			set
			{
				SetProperty(ref _StripParamList, value);
			}
		}
		#endregion

		#region SelectedROI
		Roi _SelectedROI;
		public Roi SelectedROI
		{
			get { return _SelectedROI; }
			set
			{
				SetProperty(ref _SelectedROI, value);

				if (value != null)
				{
					StripParamList = new ObservableCollection<StripParamData>(value.Strip.ParameterList);
					p_PatternFeatureList = new ObservableCollection<Feature>(value.Position.FeatureList);
				}
			}
		}
		#endregion

		#region SelectedFeature
		Feature _SelectedFeature;
		public Feature SelectedFeature
		{
			get { return this._SelectedFeature; }
			set
			{
				SetProperty(ref _SelectedFeature, value);
			}
		}
		#endregion

		#region SelectedParam
		StripParamData _SelectedParam;
		public StripParamData SelectedParam
		{
			get { return _SelectedParam; }
			set
			{
				if (value != null)
				{
					SetProperty(ref _SelectedParam, value);
				}
			}
		}
		#endregion

		#region p_InformationDrawer

		private InformationDrawer informationDrawer;
		public InformationDrawer p_InformationDrawer
		{
			get
			{
				return informationDrawer;
			}
			set
			{
				SetProperty(ref informationDrawer, value);
			}
		}
		#endregion

		#region p_ImageViewer

		private ImageViewer_ViewModel m_ImageViewer;
		public ImageViewer_ViewModel p_ImageViewer
		{
			get
			{
				return m_ImageViewer;
			}
			set
			{
				SetProperty(ref m_ImageViewer, value);
			}
		}
		#endregion

		#region p_SimpleShapeDrawer
		private SimpleShapeDrawerVM m_SimpleShapeDrawer;
		public SimpleShapeDrawerVM p_SimpleShapeDrawer
		{
			get
			{
				return m_SimpleShapeDrawer;
			}
			set
			{
				SetProperty(ref m_SimpleShapeDrawer, value);
			}
		}
		#endregion

		#endregion

		#region Func
		void _addParam()
		{
			if (!m_Engineer.m_recipe.Loaded)
				return;

			int paramCount = SelectedROI.Strip.ParameterList.Count;
			string defaultName = string.Format("StripParam #{0}", paramCount);

			StripParamData temp = new StripParamData();
			temp.Name = defaultName;

			SelectedROI.Strip.ParameterList.Add(temp);

			StripParamList = new ObservableCollection<StripParamData>(SelectedROI.Strip.ParameterList);
		}
		void _addRoi()
		{
			if (!m_Engineer.m_recipe.Loaded)
				return;

			int roiCount = m_Engineer.m_recipe.VegaRecipeData.RoiList.Where(x => x.RoiType == Roi.Item.ReticlePattern).Count();
			string defaultName = string.Format("Pattern ROI #{0}", roiCount);

			Roi temp = new Roi(defaultName, Roi.Item.ReticlePattern);
			m_Engineer.m_recipe.VegaRecipeData.RoiList.Add(temp);

			p_PatternRoiList = new ObservableCollection<Roi>(m_Engineer.m_recipe.VegaRecipeData.RoiList.Where(x => x.RoiType == Roi.Item.ReticlePattern));
		}
		void _clearInspReslut()
		{
			currentTotalIdx = 0;

			DBConnector connector = new DBConnector("localhost", "Inspections", "root", "`ati5344");
			if (connector.Open())
			{
				string dropQuery = "DROP TABLE Inspections.tempdata";
				var result = connector.SendNonQuery(dropQuery);
				Debug.WriteLine(string.Format("tempdata Table Drop : {0}", result));
				result = connector.SendNonQuery("INSERT INTO inspections.inspstatus (idx, inspStatusNum) VALUES ('0', '0') ON DUPLICATE KEY UPDATE idx='0', inspStatusNum='0';");
				Debug.WriteLine(string.Format("Status Clear : {0}", result));
			}
		}

		void ClearDrawList()
		{
			p_SimpleShapeDrawer.Clear();
			p_InformationDrawer.Clear();

			p_ImageViewer.SetRoiRect();
			p_InformationDrawer.Redrawing();
		}
		private void _startInsp()
		{
			//TODO EdgeboxViewModel과 SideViewModel에 중복된 메소드가 존재하므로 통합이 가능한경우 정리하도록 합시다
			if (!m_Engineer.m_recipe.Loaded)
				return;

			//0. 개수 초기화 및 Table Drop
			_clearInspReslut();

			ClearDrawList();

			//2. 획득한 영역을 기준으로 검사영역을 생성하고 검사를 시작한다
			for (int i = 0; i < 4; i++)
			{
				for (int k = 0; k < p_PatternRoiList.Count; k++)
				{
					//ROI 개수만큼 회전하면서 검사영역을 생성한다
				}
			}
			m_Engineer.m_InspManager.StartInspection();
		}
		void _DrawFeature()
		{
			//그려진 첫번째 빨간색영역 내의 이미지를 Feature 정보로 저장한다
			//그린 정보는 SelectedFeature가 변경되면 Update되어야한다
			if (p_SimpleShapeDrawer.m_ListRect.Count > 0)
			{
				var featureArea = p_SimpleShapeDrawer.m_ListRect[0];
				var featureImage = p_ImageViewer.p_ImageData.GetRectImage(new CRect(featureArea.StartPos, featureArea.EndPos));
				var targetName = string.Format("{0}_{1}.bmp", SelectedROI.Name, SelectedROI.Position.FeatureList.Count);

				featureImage.Save(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(SelectedRecipe.RecipePath), targetName), ImageFormat.Bmp);

				Feature tempFeature = new Feature(targetName);
				tempFeature.RoiRect = new CRect(featureArea.StartPos, featureArea.EndPos);
				tempFeature.m_Feature = new ImageData(featureImage.Width, featureImage.Height);
				tempFeature.m_Feature.LoadImageSync(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(SelectedRecipe.RecipePath), targetName), new CPoint(0, 0));
				SelectedROI.Position.FeatureList.Add(tempFeature);

				p_PatternFeatureList = new ObservableCollection<Feature>(SelectedROI.Position.FeatureList);
			}
		}
		#endregion

		#region Command
		public ICommand AddROICommand
		{
			get
			{
				return new RelayCommand(_addRoi);
			}
		}
		public ICommand ClearResultCommand
		{
			get
			{
				return new RelayCommand(_clearInspReslut);
			}
		}
		public ICommand CommandStartInsp
		{
			get
			{
				return new RelayCommand(_startInsp);
			}
		}
		public ICommand CommandAddParam
		{
			get
			{
				return new RelayCommand(_addParam);
			}
		}
		public ICommand DrawFeatureCommand
		{
			get
			{
				return new RelayCommand(_DrawFeature);
			}
		}
		#endregion
	}
}
