using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	}
	public class Result
	{
		//DateTime m_StartTime;
		//DateTime m_EndTime;
		DefectInfo[] m_DD;

		Result(int nCnt = 100000)
		{
			m_DD = new DefectInfo[nCnt];
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
		ObservableCollection<Roi> m_Roi = new ObservableCollection<Roi>();
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
		//ParamData m_PD;
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
	public class SurFace_ParamData : ObservableObject
	{
		public SurFace_ParamData()
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

		#region p_GV
		Param<int> GV;
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
		Param<int> DefectSize;
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
		string _TargetLabelText;
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
		public ImageData m_Feature;
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
		public Bump m_Bump = new Bump();
		//DeadLine m_DeadLine;


		Item m_Item;
		public enum Item
		{
			Test,
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
		ObservableCollection<SurFace_ParamData> m_Parameter = new ObservableCollection<SurFace_ParamData>();
		public ObservableCollection<SurFace_ParamData> p_Parameter
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
		public Unit[,] Map = null;
		public MapData(int w, int h)
		{
			Map = new Unit[w, h];
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
