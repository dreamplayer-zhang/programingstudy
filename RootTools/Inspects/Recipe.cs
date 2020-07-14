using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;

namespace RootTools.Inspects
{
	public class VegaRecipe : Recipe
	{
		VegaRecipeData vegaRecipeData = new VegaRecipeData();
		public VegaRecipeData VegaRecipeData
		{
			get { return vegaRecipeData; }
			set { SetProperty(ref vegaRecipeData, value); }
		}
		public override void Init()
		{
			base.Init();
			this.VegaRecipeData = new VegaRecipeData();
		}
		public override bool Save(string recipeDir)
		{
			if (File.Exists(recipeDir))
			{
				//파일 경로가 들어오면 안됩니다!
				return false;
			}
			this.RecipeName = recipeDir.Split('\\').Last();
			if (!Directory.Exists(recipeDir))
			{
				Directory.CreateDirectory(recipeDir);
			}

			string paramPath = Path.Combine(recipeDir, "Parameter.VegaVision");

			//Feature 저장
			if (VegaRecipeData != null)
			{
				foreach (var roi in VegaRecipeData.RoiList)
				{
					int featureIdx = 0;
					if (roi != null)
					{
						foreach (var feature in roi.Position.ReferenceList)
						{
							if (feature != null)
							{
								feature.Name = roi.Name + "_" + featureIdx.ToString() + ".bmp";
								feature.m_Feature.SaveImageSync(Path.Combine(recipeDir, feature.Name));
								featureIdx++;
							}
						}
					}
				}
			}
			if (MapData != null)
			{
				MapData.Save(recipeDir, "MapData.csv");
			}

			//Serialize가능한 내용을 xml로 저장
			using (StreamWriter wr = new StreamWriter(paramPath))
			{
				XmlSerializer xs = new XmlSerializer(typeof(VegaRecipe));
				xs.Serialize(wr, this);
			}
			return true;
		}
		public override void Load(string filePath)
		{
			Loaded = false;
			RecipePath = filePath;

			XmlSerializer serializer = new XmlSerializer(typeof(VegaRecipe));
			VegaRecipe result = new VegaRecipe();

			using (Stream reader = new FileStream(filePath, FileMode.Open))
			{
				// Call the Deserialize method to restore the object's state.
				result = (VegaRecipe)serializer.Deserialize(reader);
			}
			this.MapData = result.MapData;
			this.m_SI = result.m_SI;
			this.VegaRecipeData = result.VegaRecipeData;
			this.RecipeName = Path.GetDirectoryName(filePath).Split('\\').Last();

			//feature data load
			foreach (var roi in result.vegaRecipeData.RoiList)
			{
				foreach (var feature in roi.Position.ReferenceList)
				{
					string imageTargetPath = System.IO.Path.Combine(Path.GetDirectoryName(filePath), feature.Name);
					//TODO : Image 정보를 별도로 넣는 것이 효율적일 것으로 보임
					feature.m_Feature = new ImageData(feature.RoiRect.Width, feature.RoiRect.Height);
					feature.m_Feature.LoadImageSync(imageTargetPath, new CPoint(0, 0));
				}
			}

			//map data load
			var currentName = Path.GetDirectoryName(filePath).Split('\\').Last();
			result.RecipeName = currentName;

			if (result.MapData != null)
			{
				result.MapData.Load(Path.GetDirectoryName(filePath));
			}
			else
			{
				result.MapData = new MapData(1, 1);
			}

			if (LoadComplete != null)
			{
				LoadComplete();
			}

			Loaded = true;
		}
	}
	public class Recipe : ObservableObject
	{
		#region EventHandler
		/// <summary>
		/// 이벤트 핸들러
		/// </summary>
		public delegate void EventHandler();
		[XmlIgnore] public EventHandler LoadComplete;
		#endregion

		[XmlIgnore] public string RecipeName { get; set; }
		[XmlIgnore] public string RecipePath { get; set; }
		RecipeData recipeData = new RecipeData();
		public RecipeData RecipeData
		{
			get { return recipeData; }
			set { SetProperty(ref recipeData, value); }
		}

		[XmlIgnore] public Result m_SI;
		public MapData MapData;
		/// <summary>
		/// 레시피가 로드된 상태를 표시한다
		/// </summary>
		[XmlIgnore] public bool Loaded { get; protected set; }
		virtual public void Init()
		{
			this.RecipeName = "";
			RecipeData = new RecipeData();
			MapData = new MapData();
		}
		/// <summary>
		/// 레시피를 저장한다
		/// </summary>
		/// <param name="recipeDir">레시피가 저장될 폴더명</param>
		virtual public bool Save(string recipeDir)
		{
			return true;
		}

		/// <summary>
		/// VEGA Vision Recipe를 로드한다
		/// </summary>
		/// <param name="filePath">.VegaVision File경로</param>
		/// <returns></returns>
		virtual public void Load(string filePath)
		{

		}
	}
	public class Result
	{
		//DateTime m_StartTime;
		//DateTime m_EndTime;
		public List<DefectInfo> DefectInfoList;

		Result()
		{
			DefectInfoList = new List<DefectInfo>();
		}

		public void MakeCluster(List<DefectInfo> DD, int nRange = 10)
		{
			int cid = 0;
			for (int n = 0; n < DD.Count; n++)
			{
				DefectInfo Curr = DD[n];
				if (Curr.nClusterID != DefectInfo.NONE)
				{
					Curr.nClusterID = cid++;
					for (int m = 0; m < DD.Count; m++)
					{
						if (Curr.rtArea.Center().Distance(DD[m].rtArea.Center()) < nRange)
						{
							DD[m].nClusterID = Curr.nClusterID;
						}
					}
				}
			}
		}

	}


	public class DefectInfo
	{
		public const int NONE = -1;
		public RPoint ptPos; // Center 
		public double dArea; // Count of points 
		public CPoint ptUnit; // chip die
		public RPoint ptSize; // w h 
		public int nClusterID = NONE; // 이웃
		public CRect rtArea;  //외각
		public string sDefectName;
		public int sDCode;
	}
	public class VegaRecipeData : RecipeData
	{
		public VegaRecipeData()
		{
			//일단 pixel 단위로 지정
			_SideInspMargin = new Param<int>(100, 0, int.MaxValue);

			_SideTopUpperOffset = new Param<int>(10000, 0, int.MaxValue);
			_SideTopCenterOffset = new Param<int>(30000, 0, int.MaxValue);
			_SideTopUnderOffset = new Param<int>(10000, 0, int.MaxValue);

			_SideLeftUpperOffset = new Param<int>(10000, 0, int.MaxValue);
			_SideLeftCenterOffset = new Param<int>(30000, 0, int.MaxValue);
			_SideLeftUnderOffset = new Param<int>(10000, 0, int.MaxValue);

			_SideRightUpperOffset = new Param<int>(10000, 0, int.MaxValue);
			_SideRightCenterOffset = new Param<int>(30000, 0, int.MaxValue);
			_SideRightUnderOffset = new Param<int>(10000, 0, int.MaxValue);

			_SideBottomUpperOffset = new Param<int>(10000, 0, int.MaxValue);
			_SideBottomCenterOffset = new Param<int>(30000, 0, int.MaxValue);
			_SideBottomUnderOffset = new Param<int>(10000, 0, int.MaxValue);
		}

		#region SideInspMargin

		Param<int> _SideInspMargin;
		public int SideInspMargin
		{
			get
			{
				return _SideInspMargin._value;
			}
			set
			{
				if (_SideInspMargin._value == value)
					return;
				_SideInspMargin._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion


		#region SideTopUpperOffset

		Param<int> _SideTopUpperOffset;
		public int SideTopUpperOffset
		{
			get
			{
				return _SideTopUpperOffset._value;
			}
			set
			{
				if (_SideTopUpperOffset._value == value)
					return;
				_SideTopUpperOffset._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region SideTopCenterOffset

		Param<int> _SideTopCenterOffset;
		public int SideTopCenterOffset
		{
			get
			{
				return _SideTopCenterOffset._value;
			}
			set
			{
				if (_SideTopCenterOffset._value == value)
					return;
				_SideTopCenterOffset._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region SideTopUnderOffset

		Param<int> _SideTopUnderOffset;
		public int SideTopUnderOffset
		{
			get
			{
				return _SideTopUnderOffset._value;
			}
			set
			{
				if (_SideTopUnderOffset._value == value)
					return;
				_SideTopUnderOffset._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region SideLeftUpperOffset

		Param<int> _SideLeftUpperOffset;
		public int SideLeftUpperOffset
		{
			get
			{
				return _SideLeftUpperOffset._value;
			}
			set
			{
				if (_SideLeftUpperOffset._value == value)
					return;
				_SideLeftUpperOffset._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region SideLeftCenterOffset

		Param<int> _SideLeftCenterOffset;
		public int SideLeftCenterOffset
		{
			get
			{
				return _SideLeftCenterOffset._value;
			}
			set
			{
				if (_SideLeftCenterOffset._value == value)
					return;
				_SideLeftCenterOffset._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region SideLeftUnderOffset

		Param<int> _SideLeftUnderOffset;
		public int SideLeftUnderOffset
		{
			get
			{
				return _SideLeftUnderOffset._value;
			}
			set
			{
				if (_SideLeftUnderOffset._value == value)
					return;
				_SideLeftUnderOffset._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region SideRightUpperOffset

		Param<int> _SideRightUpperOffset;
		public int SideRightUpperOffset
		{
			get
			{
				return _SideRightUpperOffset._value;
			}
			set
			{
				if (_SideRightUpperOffset._value == value)
					return;
				_SideRightUpperOffset._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region SideRightCenterOffset

		Param<int> _SideRightCenterOffset;
		public int SideRightCenterOffset
		{
			get
			{
				return _SideRightCenterOffset._value;
			}
			set
			{
				if (_SideRightCenterOffset._value == value)
					return;
				_SideRightCenterOffset._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region SideRightUnderOffset

		Param<int> _SideRightUnderOffset;
		public int SideRightUnderOffset
		{
			get
			{
				return _SideRightUnderOffset._value;
			}
			set
			{
				if (_SideRightUnderOffset._value == value)
					return;
				_SideRightUnderOffset._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region SideBottomUpperOffset

		Param<int> _SideBottomUpperOffset;
		public int SideBottomUpperOffset
		{
			get
			{
				return _SideBottomUpperOffset._value;
			}
			set
			{
				if (_SideBottomUpperOffset._value == value)
					return;
				_SideBottomUpperOffset._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region SideBottomCenterOffset

		Param<int> _SideBottomCenterOffset;
		public int SideBottomCenterOffset
		{
			get
			{
				return _SideBottomCenterOffset._value;
			}
			set
			{
				if (_SideBottomCenterOffset._value == value)
					return;
				_SideBottomCenterOffset._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region SideBottomUnderOffset

		Param<int> _SideBottomUnderOffset;
		public int SideBottomUnderOffset
		{
			get
			{
				return _SideBottomUnderOffset._value;
			}
			set
			{
				if (_SideBottomUnderOffset._value == value)
					return;
				_SideBottomUnderOffset._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion
	}
	public class RecipeData : ObservableObject
	{
		public RecipeData()
		{
			_useDefectMerge = false;
			_mergeDistance = new Param<int>(10, 0, int.MaxValue);//최대치는 나중에 정하기
			_roiList = new ObservableCollection<Roi>();
		}

		#region p_Roi

		ObservableCollection<Roi> _roiList;
		public ObservableCollection<Roi> RoiList
		{
			get
			{
				return _roiList;
			}
			set
			{
				SetProperty(ref _roiList, value);
			}
		}
		#endregion

		#region UseDefectMerge
		bool _useDefectMerge;
		public bool UseDefectMerge
		{
			get { return _useDefectMerge; }
			set
			{
				SetProperty(ref _useDefectMerge, value);
			}
		}
		#endregion

		#region MergeDistance
		Param<int> _mergeDistance;
		public int MergeDistance
		{
			get
			{
				return _mergeDistance._value;
			}
			set
			{
				if (_mergeDistance._value == value)
					return;
				_mergeDistance._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion
	}
	public class Param<T> where T : IComparable
	{
		T __value;
		public T _value
		{
			get { return __value; }
			set
			{
				if (_min.CompareTo(value) > 0)
				{
					__value = _min;
					return;
				}
				if (_max.CompareTo(value) < 0)
				{
					__value = _max;
					return;
				}
				__value = value;
			}
		}
		T _min;
		T _max;
		public Param(T Value, T Min, T Max)
		{
			_min = Min;
			_max = Max;
			_value = Value;


		}
	}
	public class BaseParamData : ObservableObject
	{
		#region TargetGV
		protected Param<int> _targetGV;
		public int TargetGV
		{
			get
			{
				return _targetGV._value;
			}
			set
			{
				if (_targetGV._value == value)
					return;
				_targetGV._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region DefectSize
		protected Param<int> _defectSize;
		public int DefectSize
		{
			get
			{
				return _defectSize._value;
			}
			set
			{
				if (_defectSize._value == value)
					return;
				_defectSize._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region TargetLabelText
		protected string _TargetLabelText;
		public string TargetLabelText
		{
			get
			{
				return _TargetLabelText;
			}
			set
			{
				if (_TargetLabelText != value)
				{
					_TargetLabelText = value;
					RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region Name
		protected string _Name;
		public string Name
		{
			get
			{
				return _Name;
			}
			set
			{
				if (_Name == value)
					return;
				_Name = value;
				RaisePropertyChanged();
			}
		}
		#endregion
	}
	public class SurfaceParamData : BaseParamData
	{
		public SurfaceParamData()
		{
			UseDarkInspection = true;
			UseAbsoluteInspection = true;
			_targetGV = new Param<int>(70, 0, 255);
			_defectSize = new Param<int>(10, 1, int.MaxValue);
			Name = "SurfaceParameter";
		}

		#region UseDarkInspection
		bool _useDarkInspection;
		public bool UseDarkInspection
		{
			get
			{
				return _useDarkInspection;
			}
			set
			{
				SetProperty(ref _useDarkInspection, value);
			}
		}
		#endregion

		#region UseAbsoluteInspection
		bool _useAbsoluteInspection;
		public bool UseAbsoluteInspection
		{
			get
			{
				return _useAbsoluteInspection;
			}
			set
			{
				if (value)
				{
					TargetLabelText = "GV";
				}
				else
				{
					TargetLabelText = "Difference(%)";
				}
				SetProperty(ref _useAbsoluteInspection, value);
			}
		}
		#endregion

	}
	public class StripParamData : BaseParamData
	{
		public StripParamData()
		{
			_targetGV = new Param<int>(70, 0, 255);
			_defectSize = new Param<int>(10, 1, int.MaxValue);
			_bandwidth = new Param<int>(10, 0, 255);
			_intensity = new Param<int>(120, 0, int.MaxValue);
			TargetLabelText = "GV";//Strip은 무조건 절대검사
			Name = "StripParameter";

			_InspAreaWidth = new Param<decimal>(5000, 0, decimal.MaxValue);
			_InspAreaHeight = new Param<decimal>(5000, 0, decimal.MaxValue);
		}

		#region Intensity
		Param<int> _intensity;
		public int Intensity
		{
			get
			{
				return _intensity._value;
			}
			set
			{
				if (_intensity._value == value)
					return;
				_intensity._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region Bandwidth
		Param<int> _bandwidth;
		public int Bandwidth
		{
			get
			{
				return _bandwidth._value;
			}
			set
			{
				if (_bandwidth._value == value)
					return;
				_bandwidth._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region InspAreaWidth
		Param<decimal> _InspAreaWidth;
		public decimal InspAreaWidth
		{
			get { return _InspAreaWidth._value; }
			set
			{
				if (_InspAreaWidth._value == value)
					return;
				_InspAreaWidth._value = value;
				RaisePropertyChanged();
			}
		}

		#endregion

		#region InspAreaHeight
		Param<decimal> _InspAreaHeight;
		public decimal InspAreaHeight
		{
			get { return _InspAreaHeight._value; }
			set
			{
				if (_InspAreaHeight._value == value)
					return;
				_InspAreaHeight._value = value;
				RaisePropertyChanged();
			}
		}

		#endregion
	}

	public class Origin
	{
		public CRect OriginRect;
		public CPoint LeftBottom;
		public RPoint Pitch;
		public Position Position;
	}
	public class EdgeBox
	{
		public EdgeBox()
		{
			EdgeList = new List<EdgeElement>();
			UseAutoGV = true;
			SearchBrightToDark = true;
			UseCustomEdgeBox = false;
		}
		public List<EdgeElement> EdgeList;
		public bool UseAutoGV;
		public bool SearchBrightToDark;
		public bool UseCustomEdgeBox;
		public int EdgeThreshold;
	}
	public class EdgeElement
	{
		public CRect Rect;
		public int SavePoint;
		public EdgeElement(int savePoint, CRect rect)
		{
			Rect = rect;
			SavePoint = savePoint;
		}
		public EdgeElement()
		{
			Rect = new CRect();
			SavePoint = -1;
		}
	}
	public class Position : ObservableObject
	{
		public List<Reference> ReferenceList = new List<Reference>();
		public List<AlignData> AlignList = new List<AlignData>();

		#region AlignSnapCount
		Param<int> _AlignSnapCount;
		public int AlignSnapCount
		{
			get { return _AlignSnapCount._value; }
			set
			{
				if (_AlignSnapCount._value == value)
					return;
				_AlignSnapCount._value = value;
				RaisePropertyChanged();
			}
		}

		#endregion

		#region AlignPositionIndex
		Param<int> _AlignPositionIndex;
		public int AlignPositionIndex
		{
			get { return _AlignPositionIndex._value; }
			set
			{
				if (_AlignPositionIndex._value == value)
					return;
				_AlignPositionIndex._value = value;
				RaisePropertyChanged();
			}
		}

		#endregion

		public Position()
		{
			_AlignSnapCount = new Param<int>(4, 0, int.MaxValue);
			_AlignPositionIndex = new Param<int>(0, 0, int.MaxValue);
		}
	}
	public class Reference : Feature
	{
		public Reference()
		{
			_PatternDistX = new Param<int>(1000, 0, int.MaxValue);
			_PatternDistY = new Param<int>(1000, 0, int.MaxValue);
		}

		#region PatternDistX
		Param<int> _PatternDistX;
		public int PatternDistX
		{
			get { return _PatternDistX._value; }
			set
			{
				if (_PatternDistX._value == value)
					return;
				_PatternDistX._value = value;
				RaisePropertyChanged();
			}
		}

		#endregion

		#region PatternDistY
		Param<int> _PatternDistY;
		public int PatternDistY
		{
			get { return _PatternDistY._value; }
			set
			{
				if (_PatternDistY._value == value)
					return;
				_PatternDistY._value = value;
				RaisePropertyChanged();
			}
		}

		#endregion
	}
	public class AlignData : Feature
	{
		public AlignData()
		{

		}
	}

	public class Feature : ObservableObject
	{
		[XmlIgnore] public ImageData m_Feature;
		public CRect RoiRect = new CRect();
		public Feature()
		{
			_FeatureFindArea = new Param<int>(1000, 0, int.MaxValue);
			_FeatureTargetScore = new Param<float>(0.4f, 0, 1);
		}

		#region FeatureFindArea
		Param<int> _FeatureFindArea;
		public int FeatureFindArea
		{
			get { return _FeatureFindArea._value; }
			set
			{
				if (_FeatureFindArea._value == value)
					return;
				_FeatureFindArea._value = value;
				RaisePropertyChanged();
			}
		}

		#endregion

		#region FeatureTargetScore
		Param<float> _FeatureTargetScore;
		public float FeatureTargetScore
		{
			get { return _FeatureTargetScore._value; }
			set
			{
				if (_FeatureTargetScore._value == value)
					return;
				_FeatureTargetScore._value = value;
				RaisePropertyChanged();
			}
		}

		#endregion

		#region Name

		/// <summary>
		/// Feature Image의 파일명. 확장자 포함
		/// </summary>
		string _name = "";
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				SetProperty(ref _name, value);
			}
		}
		#endregion
	}
	public class Roi : ObservableObject
	{
		public Roi(String name, Item item)
		{
			_name = name;
			RoiType = item;
			Origin = new Origin();
		}
		public Roi()
		{
			_name = string.Empty;
			RoiType = Item.None;
			Origin = new Origin();
		}
		string _name = "";
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				SetProperty(ref _name, value);
			}
		}
		public Origin Origin = new Origin();
		public Position Position = new Position();
		public Surface Surface = new Surface();
		public Strip Strip = new Strip();
		public Bump Bump = new Bump();
		public EdgeBox EdgeBox = new EdgeBox();
		//DeadLine m_DeadLine;


		public Item RoiType;
		public enum Item
		{
			None,
			Origin,
			Position,
			ReticlePattern,
			ReticleSide,
			ReticleBevel,
		}
	}
	public class ItemParent : ObservableObject
	{

	}

	public class Surface : ItemParent
	{
		public List<Pattern> PatternList;
		public List<NonPattern> NonPatternList;
		ObservableCollection<SurfaceParamData> _parameterList = new ObservableCollection<SurfaceParamData>();
		public ObservableCollection<SurfaceParamData> ParameterList
		{
			get
			{
				return _parameterList;
			}
			set
			{
				SetProperty(ref _parameterList, value);
			}
		}
	}
	public class Strip : ItemParent
	{
		public List<Pattern> PatternList;
		public List<NonPattern> NonPatternList;

		#region ParameterList

		ObservableCollection<StripParamData> _parameterList = new ObservableCollection<StripParamData>();
		public ObservableCollection<StripParamData> ParameterList
		{
			get
			{
				return _parameterList;
			}
			set
			{
				SetProperty(ref _parameterList, value);
			}
		}
		#endregion
	}

	public class Bump : ItemParent
	{
		public List<CPoint> BumpList;
	}
	public class Pattern
	{
		public CPoint Point;
		public int Length;
	}
	public class NonPattern
	{
		public CRect Area;
	}
	public class MapData
	{
		[XmlIgnore] public Unit[,] Map = null;
		public int ColumnCount = -1;
		public int RowCount = -1;
		public bool IsInitialized
		{
			get
			{
				if (ColumnCount > 0 && RowCount > 0)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}
		/// <summary>
		/// Map 레시피 파일명. 확장자 포함
		/// </summary>
		public string MapFileName { get; set; }

		internal void Save(string recipeDir, string fileName)
		{
			MapFileName = fileName;

			StringBuilder stbr = new StringBuilder();
			for (int h = 0; h < RowCount; h++)
			{
				for (int w = 0; w < ColumnCount; w++)
				{
					//x, y, Exist, Selected, Result, Progress
					stbr.AppendLine(Map[h, w].ToString());//저장, 로드할때 비효율적이라고 생각됨. 우선은 이렇게 구현
				}
			}
			File.WriteAllText(Path.Combine(recipeDir, MapFileName), stbr.ToString());
		}
		internal void Load(string dirPath)
		{
			//기본 Parameter Load가 되지 않은 상태에서 불러오게되면 죽을 가능성이 높음. 구조 개선 필요
			//우선 땜빵
			if (!IsInitialized)
				return;

			var lines = File.ReadAllLines(Path.Combine(dirPath, MapFileName));
			Map = new Unit[RowCount, ColumnCount];

			foreach (var line in lines)
			{
				var temp = Unit.Parse(line);
				Map[temp.Y, temp.X] = temp;
			}
		}
		public MapData(int w, int h)
		{
			Map = new Unit[w, h];
			ColumnCount = w;
			RowCount = h;
		}
		/// <summary>
		/// Serialize를 위한 생성자
		/// </summary>
		public MapData()
		{
			//Map = new Unit[1, 1];//다차원 배열은 시리얼라이즈 할 수 없습니다!
			//ColumnCount = 1;
			//RowCount = 1;
		}
		public enum DIR
		{
			D90, D180, D270
		}
		public MapData RotateCW(DIR d)
		{
			MapData m = null;
			if (d == DIR.D90)
			{
				//나중에만들어야지
			}
			else if (d == DIR.D180)
			{

			}
			else if (d == DIR.D270)
			{

			}
			return m;
		}
	}
	public class Unit
	{
		public bool Exist;
		public bool Selected;
		public UnitResult Result;
		public UnitProgress Progress;
		public int X;
		public int Y;

		/// <summary>
		/// Unit Class 기준으로 string을 Parsing합니다. 좌표 정보는 포함되어 있지 않습니다
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static Unit Parse(string input)
		{
			Unit result = new Unit();

			//X, Y, Exist, Selected, Result, Progress
			var values = input.Split(',');

			result.X = Convert.ToInt32(values[0]);
			result.Y = Convert.ToInt32(values[1]);
			result.Exist = Convert.ToBoolean(values[2]);
			result.Selected = Convert.ToBoolean(values[3]);
			result.Result = (UnitResult)Convert.ToInt32(values[4]);
			result.Progress = (UnitProgress)Convert.ToInt32(values[5]);

			return result;
		}
		public Unit()
		{

		}
		public enum UnitResult
		{
			Good = 0,
			Bad = 1
		}
		public enum UnitProgress
		{
			Ready = 0,
			Mapping = 1,
			Processing = 2,
			Done = 3,
		}
		//TODO : 손쉬운 저장/로드를 위한 ToString의 override등이 필요할 수 있음
		public override string ToString()
		{
			//X, Y, Exist, Selected, Result, Progress
			//int, int, bool, bool, int, int
			StringBuilder stbr = new StringBuilder();
			stbr.Append(X.ToString());
			stbr.Append(",");
			stbr.Append(Y.ToString());
			stbr.Append(",");
			stbr.Append(Exist.ToString());
			stbr.Append(",");
			stbr.Append(Selected.ToString());
			stbr.Append(",");
			stbr.Append((int)Result);
			stbr.Append(",");
			stbr.Append((int)Progress);

			return stbr.ToString();
		}
	}
}
