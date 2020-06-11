using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace RootTools.Inspects
{
	public class Recipe : ObservableObject
	{
		public string RecipeName;
		RecipeData recipeData = new RecipeData();
		public RecipeData RecipeData
		{
			get { return recipeData; }
			set { SetProperty(ref recipeData, value); }
		}

		[XmlIgnore] public Result m_SI;
		public MapData MapData;
		public void Save(string filePath)
		{
			//Serialize가능한 내용을 xml로 저장
			using (StreamWriter wr = new StreamWriter(filePath))
			{
				XmlSerializer xs = new XmlSerializer(typeof(Recipe));
				xs.Serialize(wr, this);
			}
			//Feature 저장
			if (RecipeData != null)
			{
				//foreach (var roi in p_RecipeData.p_Roi)
				//{
				//	if (roi != null)
				//	{
				//		foreach (var feature in roi.m_Position.m_ListFeature)
				//		{
				//			if (feature != null)
				//			{
				//				feature.m_Feature.SaveWholeImage(feature.m_sFeaturePath);
				//			}
				//		}
				//	}
				//}
			}
			if (MapData != null)
			{
				//m_MD.Save();
			}
		}
		public static Recipe Load(string filePath)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(Recipe));
			Recipe result = new Recipe();

			using (Stream reader = new FileStream(filePath, FileMode.Open))
			{
				// Call the Deserialize method to restore the object's state.
				result = (Recipe)serializer.Deserialize(reader);
			}

			return result;
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
	}
	public class SurfaceParamData : BaseParamData
	{
		public SurfaceParamData()
		{
			UseDarkInspection = true;
			UseAbsoluteInspection = true;
			_targetGV = new Param<int>(70, 0, 255);
			_defectSize = new Param<int>(10, 1, int.MaxValue);
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
	}

	public class Origin
	{
		public CRect OriginRect;
		public CPoint LeftBottom;
		public RPoint Pitch;
		public Position Position;
	}
	public class Position
	{
		public List<Feature> FeatureList = new List<Feature>();
		public int ScoreValue;
	}
	public class Feature
	{
		[XmlIgnore] public ImageData m_Feature;
		public CRect RoiRect = new CRect();
		/// <summary>
		/// Feature Image의 경로. 파일 확장자는 bmp
		/// </summary>
		public string FeatureFilePath;
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
		int nWidthCount;
		int nHeightCount;
		/// <summary>
		/// Map 레시피의 경로. 파일 확장자는 csv
		/// </summary>
		public string MapFilePath;

		private void Save()
		{
			StringBuilder stbr = new StringBuilder();
			for (int h = 0; h < nHeightCount; h++)
			{
				for (int w = 0; w < nWidthCount; w++)
				{
					//w, h, Exist, Selected, Result, Progress
					stbr.Append(w.ToString() + "," + h.ToString() + "," + Map[h, w].ToString());
				}
			}
			File.WriteAllText(MapFilePath, stbr.ToString());
		}
		private void Load()
		{

		}
		public MapData(int w, int h)
		{
			Map = new Unit[w, h];
			nWidthCount = w;
			nHeightCount = h;
		}
		/// <summary>
		/// Serialize를 위한 생성자
		/// </summary>
		public MapData()
		{
			Map = new Unit[1, 1];//다차원 배열은 시리얼라이즈 할 수 없습니다!
			nWidthCount = 1;
			nHeightCount = 1;
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

		public Unit(string input)
		{

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
			//Exist, Selected, Result, Progress
			//bool, bool, int, int
			StringBuilder stbr = new StringBuilder();
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
