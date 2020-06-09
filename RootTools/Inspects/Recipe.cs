using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace RootTools.Inspects
{
	public class Recipe : ObservableObject
	{
		public string m_Name;
		RecipeData m_RecipeData = new RecipeData();
		public RecipeData p_RecipeData
		{
			get { return m_RecipeData; }
			set { SetProperty(ref m_RecipeData, value); }
		}

		public Result m_SI;
		public MapData m_MD;
		public void Save(string filePath)
		{
			//파일에 출력하는 예
			using (StreamWriter wr = new StreamWriter(filePath))
			{
				XmlSerializer xs = new XmlSerializer(typeof(Recipe));
				xs.Serialize(wr, this);
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
		List<DefectInfo> m_DD;

		Result()
		{
			m_DD = new List<DefectInfo>();
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
			m_bDefectMerge = false;
			m_nMergeDistance = new Param<int>(10, 0, int.MaxValue);//최대치는 나중에 정하기
			m_Roi = new ObservableCollection<Roi>();
		}

		#region p_Roi

		ObservableCollection<Roi> m_Roi;
		public ObservableCollection<Roi> p_Roi
		{
			get
			{
				return m_Roi;
			}
			set
			{
				SetProperty(ref m_Roi, value);
			}
		}
		#endregion

		#region p_bDefectMerge
		bool m_bDefectMerge;
		public bool p_bDefectMerge
		{
			get { return m_bDefectMerge; }
			set
			{
				SetProperty(ref m_bDefectMerge, value);
			}
		}
		#endregion

		#region p_nMergeDistance
		Param<int> m_nMergeDistance;
		public int p_nMergeDistance
		{
			get
			{
				return m_nMergeDistance._value;
			}
			set
			{
				if (m_nMergeDistance._value == value)
					return;
				m_nMergeDistance._value = value;
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
		#region p_GV
		protected Param<int> GV;
		public int p_GV
		{
			get
			{
				return GV._value;
			}
			set
			{
				if (GV._value == value)
					return;
				GV._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region p_DefectSize
		protected Param<int> DefectSize;
		public int p_DefectSize
		{
			get
			{
				return DefectSize._value;
			}
			set
			{
				if (DefectSize._value == value)
					return;
				DefectSize._value = value;
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
			p_bDarkInspection = true;
			p_bAbsoluteInspection = true;
			GV = new Param<int>(70, 0, 255);
			DefectSize = new Param<int>(10, 1, int.MaxValue);
		}

		#region p_bDarkInspection
		bool bDarkInspection;
		public bool p_bDarkInspection
		{
			get
			{
				return bDarkInspection;
			}
			set
			{
				SetProperty(ref bDarkInspection, value);
			}
		}
		#endregion

		#region p_bAbsoluteInspection
		bool bAbsoluteInspection;
		public bool p_bAbsoluteInspection
		{
			get
			{
				return bAbsoluteInspection;
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
				SetProperty(ref bAbsoluteInspection, value);
			}
		}
		#endregion
	}
	public class StripParamData : BaseParamData
	{
		public StripParamData()
		{
			GV = new Param<int>(70, 0, 255);
			DefectSize = new Param<int>(10, 1, int.MaxValue);
			bandwidth = new Param<int>(10, 0, 255);
			intensity = new Param<int>(120, 0, int.MaxValue);
			TargetLabelText = "GV";//Strip은 무조건 절대검사
		}

		#region p_Intensity
		Param<int> intensity;
		public int p_Intensity
		{
			get
			{
				return intensity._value;
			}
			set
			{
				if (intensity._value == value)
					return;
				intensity._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region p_Bandwidth
		Param<int> bandwidth;
		public int p_Bandwidth
		{
			get
			{
				return bandwidth._value;
			}
			set
			{
				if (bandwidth._value == value)
					return;
				bandwidth._value = value;
				RaisePropertyChanged();
			}
		}
		#endregion
	}

	public class Origin
	{
		public CRect m_rtOrigin;
		public CPoint m_ptLeftBottom;
		public RPoint m_ptPitch;
		public Position m_Position;
	}
	public class Position
	{
		public List<Feature> m_ListFeature = new List<Feature>();
		public int m_nScore;
	}
	public class Feature
	{
		[XmlIgnore] public ImageData m_Feature;
		public CRect m_rtRoi = new CRect();
	}
	public class Roi : ObservableObject
	{
		public Roi(String name, Item item)
		{
			m_sName = name;
			m_Item = item;
			m_Origin = new Origin();
		}
		public Roi()
		{
			m_sName = string.Empty;
			m_Item = Item.None;
			m_Origin = new Origin();
		}
		string m_sName = "";
		public string p_sName
		{
			get
			{
				return m_sName;
			}
			set
			{
				SetProperty(ref m_sName, value);
			}
		}
		public Origin m_Origin = new Origin();
		public Position m_Position = new Position();
		public Surface m_Surface = new Surface();
		public Strip m_Strip = new Strip();
		public Bump m_Bump = new Bump();
		//DeadLine m_DeadLine;


		Item m_Item;
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
		public List<Pattern> m_Pattern;
		public List<NonPattern> m_NonPattern;
		ObservableCollection<SurfaceParamData> m_Parameter = new ObservableCollection<SurfaceParamData>();
		public ObservableCollection<SurfaceParamData> p_Parameter
		{
			get
			{
				return m_Parameter;
			}
			set
			{
				SetProperty(ref m_Parameter, value);
			}
		}
	}
	public class Strip : ItemParent
	{
		public List<Pattern> m_Pattern;
		public List<NonPattern> m_NonPattern;
		ObservableCollection<StripParamData> m_Parameter = new ObservableCollection<StripParamData>();
		public ObservableCollection<StripParamData> p_Parameter
		{
			get
			{
				return m_Parameter;
			}
			set
			{
				SetProperty(ref m_Parameter, value);
			}
		}
	}

	public class Bump : ItemParent
	{
		public List<CPoint> m_ptBump;
	}
	public class Pattern
	{
		public CPoint m_pt;
		public int m_len;
	}
	public class NonPattern
	{
		public CRect m_rt;
	}
	public class MapData
	{
		[XmlIgnore] public Unit[,] Map = null;
		public MapData(int w, int h)
		{
			Map = new Unit[w, h];
		}
		/// <summary>
		/// Serialize를 위한 생성자
		/// </summary>
		public MapData()
		{
			//Map = new Unit[1, 1];//다차원 배열은 시리얼라이즈 할 수 없습니다!
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

		public enum UnitResult
		{
			Good,
			Bad
		}
		public enum UnitProgress
		{
			Ready,
			Mapping,
			Processing,
			Done,
		}
	}
}
