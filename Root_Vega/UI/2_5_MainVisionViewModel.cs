using ATI;
using Emgu.CV.Structure;
using Livet.Converters;
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
using DPoint = System.Drawing.Point;
using MBrushes = System.Windows.Media.Brushes;

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

		bool refEnabled;
		bool alignEnabled;

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

				m_RefFeatureDrawer = new SimpleShapeDrawerVM(m_ImageViewer);
				m_RefFeatureDrawer.RectangleKeyValue = Key.D1;
				refEnabled = false;

				_AlignFeatureDrawer = new SimpleShapeDrawerVM(m_ImageViewer);
				_AlignFeatureDrawer.m_Stroke = MBrushes.BlueViolet;
				_AlignFeatureDrawer.RectangleKeyValue = Key.D1;
				alignEnabled = false;

				_SetRefDreawer();

				p_ImageViewer.SetDrawer(p_RefFeatureDrawer);
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

//			DBConnector connector = new DBConnector("localhost", "Inspections", "root", "`ati5344");
//			if (connector.Open())
//			{
//				string query = "SELECT COUNT(*) FROM inspections.tempdata;";
//				string temp = string.Empty;
//				var result = connector.SendQuery(query, ref temp);
//#if DEBUG
//				//Debug.WriteLine(string.Format("tempdata Row Count CODE : {0}", result));
//				//Debug.WriteLine(string.Format("tempdata Row Result : {0}", temp));
//#endif
//				int count;
//				if (int.TryParse(temp, out count))
//				{
//					if (currentTotalIdx < count)
//					{
//						//current index부터 count까지 defect정보를 가져와 UI에 update하고 current index를 업데이트한다

//						//countQurey = string.Format("SELECT * FROM inspections.tempdata WHERE ");//우선순위를 낮춘다. SQLite파일하고 이미지 출력하는게 먼저

//						currentTotalIdx = count;
//					}
//				}
//			}
//			connector.Close();
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
					p_PatternReferenceList = new ObservableCollection<Reference>(value.Position.ReferenceList);
					p_PatternAlignList = new ObservableCollection<AlignData>(value.Position.AlignList);
				}
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

		#region p_PatternReferenceList

		ObservableCollection<Reference> _PatternReferenceList;
		public ObservableCollection<Reference> p_PatternReferenceList
		{
			get { return _PatternReferenceList; }
			set
			{
				SetProperty(ref _PatternReferenceList, value);
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


		#region p_PatternAlignList

		ObservableCollection<AlignData> _PatternAlignList;
		public ObservableCollection<AlignData> p_PatternAlignList
		{
			get { return _PatternAlignList; }
			set
			{
				SetProperty(ref _PatternAlignList, value);
			}
		}
		#endregion

		#region SelectedAlign
		AlignData _SelectedAlign;
		public AlignData SelectedAlign
		{
			get { return this._SelectedAlign; }
			set
			{
				SetProperty(ref _SelectedAlign, value);
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

		#region p_RefFeatureDrawer
		private SimpleShapeDrawerVM m_RefFeatureDrawer;
		public SimpleShapeDrawerVM p_RefFeatureDrawer
		{
			get
			{
				return m_RefFeatureDrawer;
			}
			set
			{
				SetProperty(ref m_RefFeatureDrawer, value);
			}
		}
		#endregion

		#region p_AlignFeatureDrawer
		private SimpleShapeDrawerVM _AlignFeatureDrawer;
		public SimpleShapeDrawerVM p_AlignFeatureDrawer
		{
			get
			{
				return _AlignFeatureDrawer;
			}
			set
			{
				SetProperty(ref _AlignFeatureDrawer, value);
			}
		}
		#endregion

		#endregion

		#region Func
		private void _SetAlignDrawer()
		{
			p_ImageViewer.SetDrawer(p_AlignFeatureDrawer);
			alignEnabled = true;
			refEnabled = false;
		}

		private void _SetRefDreawer()
		{
			p_ImageViewer.SetDrawer(p_RefFeatureDrawer);
			alignEnabled = false;
			refEnabled = true;
		}
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
			connector.Close();
		}

		void ClearDrawList()
		{
			if (refEnabled)
			{
				p_RefFeatureDrawer.Clear();
			}
			if (alignEnabled)
			{
				p_AlignFeatureDrawer.Clear();
			}
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
			for (int k = 0; k < p_PatternRoiList.Count; k++)
			{
				var currentRoi = p_PatternRoiList[k];
				//ROI 개수만큼 회전하면서 검사영역을 생성한다
				for (int j = 0; j < currentRoi.Strip.ParameterList.Count; j++)
				{
					//검사영역 생성 기준
					//1. 등록된 feature를 탐색한다. 지정된 score에 부합하는 feature가 없을 경우 2차, 3차로 넘어갈 수도 있다. 
					//1.1. 만약 등록된 Feature가 없는 경우 기준 위치는 0,0으로한다
					CPoint standardPos = new CPoint(0, 0);
					int refStartXOffset = 0;
					int refStartYOffset = 0;

					foreach (var feature in currentRoi.Position.ReferenceList)
					{
						//TODO : Align과 중복되므로 나중에 별도 메소드로 만들어서 코드 중복을 최소화
						var bmp = feature.m_Feature.GetRectImage(new CRect(0, 0, feature.m_Feature.p_Size.X, feature.m_Feature.p_Size.Y));
						Emgu.CV.Image<Gray, byte> featureImage = new Emgu.CV.Image<Gray, byte>(bmp);
						var laplaceFeature = featureImage.Laplace(1);
						//laplaceFeature.Save(@"D:\Test\feature.bmp");

						CRect targetRect = new CRect(
							new Point(feature.RoiRect.Center().X - (feature.FeatureFindArea + feature.RoiRect.Width) / 2.0, feature.RoiRect.Center().Y - (feature.FeatureFindArea + feature.RoiRect.Height) / 2.0),
							new Point(feature.RoiRect.Center().X + (feature.FeatureFindArea + feature.RoiRect.Width) / 2.0, feature.RoiRect.Center().Y + (feature.FeatureFindArea + feature.RoiRect.Height) / 2.0));
						Emgu.CV.Image<Gray, byte> sourceImage = new Emgu.CV.Image<Gray, byte>(p_ImageViewer.p_ImageData.GetRectImage(targetRect));
						var laplaceSource = sourceImage.Laplace(1);
						//laplaceSource.Save(@"D:\Test\source.bmp");

						var resultImage = laplaceSource.MatchTemplate(laplaceFeature, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed);

						int widthDiff = laplaceSource.Width - resultImage.Width;
						int heightDiff = laplaceSource.Height - resultImage.Height;

						float[,,] matches = resultImage.Data;

						Point maxRelativePoint = new Point();//상대위치

						bool foundFeature = false;
						float maxScore = float.MinValue;

						for (int x = 0; x < matches.GetLength(1); x++)
						{
							for (int y = 0; y < matches.GetLength(0); y++)
							{
								if (maxScore < matches[y, x, 0] && feature.FeatureTargetScore <= matches[y, x, 0])
								{
									maxScore = matches[y, x, 0];
									maxRelativePoint.X = x;
									maxRelativePoint.Y = y;
									foundFeature = true;
								}
								//matches[y, x, 0] *= 256;
							}
						}
						resultImage.Data = matches;
						//resultImage.Save(@"D:\Test\result.bmp");
						if (foundFeature)
						{
							//2. feature 중심위치가 확보되면 해당 좌표를 저장
							standardPos.X = targetRect.Left + (int)maxRelativePoint.X + widthDiff / 2;
							standardPos.Y = targetRect.Top + (int)maxRelativePoint.Y + heightDiff / 2;
							refStartXOffset = feature.PatternDistX;
							refStartYOffset = feature.PatternDistY;
							DrawCross(new DPoint(standardPos.X, standardPos.Y), MBrushes.Red);

							break;//찾았으니 중단
						}
						else
						{
							continue;//못 찾았으면 다음 Feature값으로 이동
						}
					}

					//3. 등록된 Align Key 2개를 탐색한다. feature의 위치 정보도 참조하여 회전 보정 시에 들어갈 값을 준비해둔다
					List<CPoint> alignKeyList = new List<CPoint>();

					if (currentRoi.Position.AlignList.Count == 2)
					{
						for (int n = 0; n < 2; n++)
						{
							//TODO : Reference와 중복되므로 나중에 별도 메소드로 만들어서 코드 중복을 최소화
							var align = currentRoi.Position.AlignList[n];
							var bmp = align.m_Feature.GetRectImage(new CRect(0, 0, align.m_Feature.p_Size.X, align.m_Feature.p_Size.Y));
							Emgu.CV.Image<Gray, byte> featureImage = new Emgu.CV.Image<Gray, byte>(bmp);
							var laplaceFeature = featureImage.Laplace(1);

							CRect targetRect = new CRect(
								new Point(align.RoiRect.Center().X - align.FeatureFindArea / 2.0, align.RoiRect.Center().Y - align.FeatureFindArea / 2.0),
								new Point(align.RoiRect.Center().X + align.FeatureFindArea / 2.0, align.RoiRect.Center().Y + align.FeatureFindArea / 2.0));
							Emgu.CV.Image<Gray, byte> sourceImage = new Emgu.CV.Image<Gray, byte>(p_ImageViewer.p_ImageData.GetRectImage(targetRect));
							var laplaceSource = sourceImage.Laplace(1);

							var resultImage = laplaceSource.MatchTemplate(laplaceFeature, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed);

							int widthDiff = laplaceSource.Width - resultImage.Width;
							int heightDiff = laplaceSource.Height - resultImage.Height;

							float[,,] matches = resultImage.Data;

							Point maxRelativePoint = new Point();//상대위치

							bool foundFeature = false;
							float maxScore = float.MinValue;

							for (int x = 0; x < matches.GetLength(1); x++)
							{
								for (int y = 0; y < matches.GetLength(0); y++)
								{
									if (maxScore < matches[y, x, 0] && align.FeatureTargetScore <= matches[y, x, 0])
									{
										maxScore = matches[y, x, 0];
										maxRelativePoint.X = x;
										maxRelativePoint.Y = y;
										foundFeature = true;
									}
									//matches[y, x, 0] *= 256;
								}
							}
							if (foundFeature)
							{
								//2. feature 중심위치가 확보되면 해당 좌표를 저장
								CPoint tempPos = new CPoint();
								tempPos.X = targetRect.Left + (int)maxRelativePoint.X + widthDiff / 2;
								tempPos.Y = targetRect.Top + (int)maxRelativePoint.Y + heightDiff / 2;
								DrawCross(new DPoint(tempPos.X, tempPos.Y), MBrushes.Crimson);
								alignKeyList.Add(tempPos);
							}
						}
					}
					//TODO : 회전보정은 나중에하기
					if (alignKeyList.Count != 2)
					{
						//align 실패. 에러를 띄우거나 회전 좌표 보정을 하지 않음
					}
					else
					{
						//align 탐색 성공. 좌표 보정 계산 시작
					}

					//4. 저장된 좌표를 기준으로 PatternDistX, PatternDistY만큼 더한다. 이 좌표가 Start Position이 된다
					var startPos = new Point(standardPos.X + refStartXOffset, standardPos.Y + refStartYOffset);
					//5. Start Position에 InspAreaWidth와 InspAreaHeight만큼 더해준다. 이 좌표가 End Position이 된다
					var endPos = new Point(startPos.X + (int)currentRoi.Strip.ParameterList[j].InspAreaWidth, startPos.Y + (int)currentRoi.Strip.ParameterList[j].InspAreaHeight);
					//6. Start Postiion과 End Position, Inspection Offset을 이용하여 검사 영역을 생성한다. 우선은 일괄 생성을 대상으로 한다
					var inspRect = new CRect(startPos, endPos);

					var temp = new UIElementInfo(new Point(inspRect.Left, inspRect.Top), new Point(inspRect.Right, inspRect.Bottom));

					System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
					rect.Width = inspRect.Width;
					rect.Height = inspRect.Height;
					System.Windows.Controls.Canvas.SetLeft(rect, inspRect.Left);
					System.Windows.Controls.Canvas.SetTop(rect, inspRect.Top);
					rect.StrokeThickness = 3;
					rect.Stroke = MBrushes.Orange;

					p_RefFeatureDrawer.m_ListShape.Add(rect);
					p_RefFeatureDrawer.m_Element.Add(rect);
					p_RefFeatureDrawer.m_ListRect.Add(temp);

					p_ImageViewer.SetRoiRect();

					int nDefectCode = InspectionManager.MakeDefectCode(InspectionTarget.Chrome, InspectionType.Strip, k);

					m_Engineer.m_InspManager.SetStandardPos(nDefectCode, standardPos);

					m_Engineer.m_InspManager.CreateInspArea(App.sPatternPool, App.sPatternGroup, App.sPatternmem, m_Engineer.GetMemory(App.sPatternPool, App.sPatternGroup, App.sPatternmem).GetMBOffset(),
							m_Engineer.GetMemory(App.sPatternPool, App.sPatternGroup, App.sPatternmem).p_sz.X,
							m_Engineer.GetMemory(App.sPatternPool, App.sPatternGroup, App.sPatternmem).p_sz.Y,
							inspRect, 500, currentRoi.Strip.ParameterList[j], nDefectCode, m_Engineer.m_recipe.VegaRecipeData.UseDefectMerge, m_Engineer.m_recipe.VegaRecipeData.MergeDistance);
					//7. Strip검사를 시작한다
				}
			}
			m_Engineer.m_InspManager.StartInspection();//검사 시작!
		}
		void DrawCross(System.Drawing.Point pt, System.Windows.Media.SolidColorBrush brsColor)
		{
			DPoint ptLT = new DPoint(pt.X - 10, pt.Y - 10);
			DPoint ptRB = new DPoint(pt.X + 10, pt.Y + 10);
			DPoint ptLB = new DPoint(pt.X - 10, pt.Y + 10);
			DPoint ptRT = new DPoint(pt.X + 10, pt.Y - 10);

			DrawLine(ptLT, ptRB, brsColor);
			DrawLine(ptLB, ptRT, brsColor);
		}
		void DrawLine(System.Drawing.Point pt1, System.Drawing.Point pt2, System.Windows.Media.SolidColorBrush brsColor)
		{
			// implement
			Line myLine = new Line();
			myLine.Stroke = brsColor;
			myLine.X1 = pt1.X;
			myLine.X2 = pt2.X;
			myLine.Y1 = pt1.Y;
			myLine.Y2 = pt2.Y;
			myLine.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
			myLine.VerticalAlignment = VerticalAlignment.Center;
			myLine.StrokeThickness = 2;


			m_ImageViewer.SelectedTool.m_ListShape.Add(myLine);
			UIElementInfo uei = new UIElementInfo(new System.Windows.Point(myLine.X1, myLine.Y1), new System.Windows.Point(myLine.X2, myLine.Y2));
			m_ImageViewer.SelectedTool.m_ListRect.Add(uei);
			m_ImageViewer.SelectedTool.m_Element.Add(myLine);
		}
		void _SaveAlign()
		{
			if (!alignEnabled)
				return;

			if (p_AlignFeatureDrawer.m_ListRect.Count >= 2)
			{
				for (int i = 0; i < 2; i++)
				{
					var featureArea = p_RefFeatureDrawer.m_ListRect[0];
					var featureRect = new CRect(featureArea.StartPos, featureArea.EndPos);
					var featureImageArr = p_ImageViewer.p_ImageData.GetRectByteArray(featureRect);
					var targetName = string.Format("{0}_Align_{1}.bmp", SelectedROI.Name, SelectedROI.Position.ReferenceList.Count);
					//TODO 이상하게 구현함. 나중에 수정 필요
					Emgu.CV.Image<Gray, byte> temp = new Emgu.CV.Image<Gray, byte>(featureRect.Width, featureRect.Height);
					temp.Bytes = featureImageArr;
					temp.Save(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(SelectedRecipe.RecipePath), targetName));

					AlignData tempFeature = new AlignData();
					tempFeature.Name = targetName;
					tempFeature.RoiRect = new CRect(featureArea.StartPos, featureArea.EndPos);
					tempFeature.m_Feature = new ImageData(featureRect.Width, featureRect.Height);
					tempFeature.m_Feature.LoadImageSync(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(SelectedRecipe.RecipePath), targetName), new CPoint(0, 0));
					SelectedROI.Position.AlignList.Add(tempFeature);
				}

				p_PatternAlignList = new ObservableCollection<AlignData>(SelectedROI.Position.AlignList);
			}
		}
		void _SaveFeature()
		{
			//그려진 첫번째 빨간색영역 내의 이미지를 Feature 정보로 저장한다
			//그린 정보는 SelectedFeature가 변경되면 Update되어야한다
			if (!refEnabled)
				return;

			if (p_RefFeatureDrawer.m_ListRect.Count >= 1)
			{
				var featureArea = p_RefFeatureDrawer.m_ListRect[0];
				var featureRect = new CRect(featureArea.StartPos, featureArea.EndPos);
				var featureImageArr = p_ImageViewer.p_ImageData.GetRectByteArray(featureRect);
				var targetName = string.Format("{0}_Ref_{1}.bmp", SelectedROI.Name, SelectedROI.Position.ReferenceList.Count);
				//TODO 이상하게 구현함. 나중에 수정 필요
				Emgu.CV.Image<Gray, byte> temp = new Emgu.CV.Image<Gray, byte>(featureRect.Width, featureRect.Height);
				temp.Bytes = featureImageArr;
				temp.Save(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(SelectedRecipe.RecipePath), targetName));

				Reference tempFeature = new Reference();
				tempFeature.Name = targetName;
				tempFeature.RoiRect = new CRect(featureArea.StartPos, featureArea.EndPos);
				tempFeature.m_Feature = new ImageData(featureRect.Width, featureRect.Height);
				tempFeature.m_Feature.LoadImageSync(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(SelectedRecipe.RecipePath), targetName), new CPoint(0, 0));
				SelectedROI.Position.ReferenceList.Add(tempFeature);

				p_PatternReferenceList = new ObservableCollection<Reference>(SelectedROI.Position.ReferenceList);
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
		public ICommand ClearDrawingCommand
		{
			get
			{
				return new RelayCommand(ClearDrawList);
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
		public ICommand SaveFeatureCommand
		{
			get
			{
				return new RelayCommand(_SaveFeature);
			}
		}
		public ICommand SaveAlignCommand
		{
			get
			{
				return new RelayCommand(_SaveAlign);
			}
		}
		public ICommand ChangeToolForAlign
		{
			get
			{
				return new RelayCommand(_SetAlignDrawer);
			}
		}
		public ICommand ChangeToolForRef
		{
			get
			{
				return new RelayCommand(_SetRefDreawer);
			}
		}
		#endregion
	}
}
