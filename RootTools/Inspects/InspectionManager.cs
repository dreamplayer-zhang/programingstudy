using System.ComponentModel;
using System.Threading;
using System.Collections;
using System.Data;
using ATI;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System;
using System.Security.RightsManagement;
using System.Threading.Tasks;
using System.Linq;
using DPoint = System.Drawing.Point;
using System.Diagnostics;

namespace RootTools.Inspects
{
	public enum eEdgeFindDirection { LEFT, TOP, RIGHT, BOTTOM };
	public enum eBrightSide { LEFT, TOP, RIGHT, BOTTOM };
	public class InspectionManager : Singleton<InspectionManager>, INotifyPropertyChanged
	{
		#region EventHandler
		/// <summary>
		/// Defect 정보 변경 시 사용할 Event Handler
		/// </summary>
		/// <param name="item">Defect Information</param>
		/// <param name="args">arguments. 필요한 경우 수정해서 사용</param>
		public delegate void ChangeDefectInfoEventHanlder(DefectDataWrapper item);
		/// <summary>
		/// UI에 Defect을 추가하기 위해 발생하는 Event
		/// </summary>
		public event ChangeDefectInfoEventHanlder AddDefect;
		#endregion

		Thread inspThread;
		Inspection[] InsepctionThread;

		StopWatch sw;

		int nThreadNum = 10;
		public int nInspectionCount = 0;

		bool m_bProgress;

		public bool IsInitialized { get; private set; }
		public void StartInspection()
		{
			m_bProgress = false;
			nInspectionCount = 0;
			sw = new StopWatch();
			sw.Start();

			IsInitialized = true;

			if (0 < GetWaitQueue())
			{
				if (GetWaitQueue() < nThreadNum)
				{
					nThreadNum = GetWaitQueue();
				}
				inspThread = new Thread(DoInspection);

				//if (inspThread != null)
				//{
				//	if (inspThread.IsAlive)
				//	{
				//		inspThread.Abort();
				//	}
				//}
				inspThread.Start();
			}
		}
		public void DoInspection()
		{
			if (!IsInitialized)
				return;

			int nInspDoneNum = 0;
			InsepctionThread = new Inspection[nThreadNum];

			Parallel.For(0, nThreadNum, i =>
			{
				InsepctionThread[i] = new Inspection(nThreadNum);
				InsepctionThread[i].AddDefect += InspectionManager_AddDefect;
			});
			//for (int i = 0; i < nThreadNum; i++)
			//{
			//	InsepctionThread[i] = new Inspection(nThreadNum);
			//	InsepctionThread[i].AddDefect += InspectionManager_AddDefect;
			//}

			m_bProgress = true;

			while (m_bProgress)
			{
				lock (lockObj)
				{
					for (int i = 0; i < nThreadNum; i++)
					{
						if (InsepctionThread[i].bState == Inspection.InspectionState.Done)
						{
							InsepctionThread[i].bState = Inspection.InspectionState.Ready;
							nInspDoneNum++;
						}
					}

					while (p_qInspection.Count == 0 && nInspDoneNum == nInspectionCount)
					{
						InspectionDone();
						Monitor.Wait(lockObj);
						m_bProgress = false;
						break;
					}

					for (int i = 0; i < nThreadNum; i++)
					{
						if (0 < GetWaitQueue())
						{
							if (InsepctionThread[i].bState == Inspection.InspectionState.Ready ||
							InsepctionThread[i].bState == Inspection.InspectionState.None)
							{
								nInspectionCount++;
								InspectionProperty ipQueue = p_qInspection.Dequeue();
								InsepctionThread[i].StartInspection(ipQueue, i);
							}
						}
					}
				}
			}
			//여기서 완료 이벤트 발생
		}
		/// <summary>
		/// Add Defect 이벤트가 발생할 때 실행될 메소드
		/// </summary>
		/// <param name="source">DefectData array</param>
		/// <param name="args">추후 arguments가 필요하면 사용할것</param>
		private void InspectionManager_AddDefect(DefectDataWrapper item)
		{
			//여기서 DB 에 추가되는 등의 동작을 해야함!
			if (AddDefect != null)
			{
				AddDefect(item);
			}
		}

		public void InspectionDone()
		{
			//여기서 DB관련동작 이하생략!

			////TODO : 해당 Queue로 들어온 검사가 완전 종료되었을때 발동. 여기서 DB를 닫으면 될 것으로 보임
			//if (InspectionComplete != null)
			//{
			//	InspectionComplete();
			//}
			sw.Stop();
			Console.WriteLine(string.Format("Insepction End : {0}", sw.ElapsedMilliseconds / 1000.0));

		}
		public void Dispose()
		{
			if (!IsInitialized)
				return;

			if (InsepctionThread != null)
			{
				for (int i = 0; i < InsepctionThread.Length; i++)
				{
					//이벤트 핸들러 제거
					InsepctionThread[i].AddDefect -= InspectionManager_AddDefect;
					//InsepctionThread[i].Dispose();
				}
			}
		}

		public void SetThread(int threadNum)  //(in int threadNum)  //2013 in 안됨
		{
			nThreadNum = threadNum;
		}

		public int GetWaitQueue()
		{
			return p_qInspection.Count;
		}

		public void AddInspection(InspectionProperty _property, bool bDefectMerge, int nMergeDistance)//(in InspectionProperty _property) //2013 in 안됨
		{
			_property.p_bDefectMerge = bDefectMerge;
			_property.p_nMergeDistance = nMergeDistance;

			p_qInspection.Enqueue(_property);
		}

		public void ClearInspection()
		{
			p_qInspection.Clear();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="WholeInspArea"></param>
		/// <param name="blocksize"></param>
		/// <param name="param"></param>
		/// <param name="bDefectMerge"></param>
		/// <param name="nMergeDistance"></param>
		/// <returns></returns>
		public List<CRect> CreateInspArea(string poolName, ulong memOffset, int memWidth, int memHeight, CRect WholeInspArea, int blocksize, BaseParamData param, int dCode, bool bDefectMerge, int nMergeDistance)
		{
			List<CRect> inspblocklist = new List<CRect>();

			int AreaStartX = WholeInspArea.Left;
			int AreaEndX = WholeInspArea.Right;
			int AreaStartY = WholeInspArea.Top;
			int AreaEndY = WholeInspArea.Bottom;

			int AreaWidth = WholeInspArea.Width;
			int AreaHeight = WholeInspArea.Height;

			//if (StartX + blocksize > EndX || StartY + blocksize > EndY)
			//{
			//    return;
			//}

			int iw = AreaWidth / blocksize;
			int ih = AreaHeight / blocksize;

			int wStart = 0;
			int wStop = iw;

			//if (nStart != -1 && nStop != -1)//검사영역을 제한하는 기능
			//{
			//	wStart = nStart;
			//	wStop = nStop;
			//}

			if (wStop == 0 || ih == 0)
			{
				// return;
			}
			else
			{
				//insp blockd에 대한 start xy, end xy
				int sx, sy, ex, ey;
				int blockcount = 1;

				for (int i = wStart; i < wStop; i++)
				{
					sx = AreaStartX + i * blocksize;
					if (i < iw - 1) ex = sx + blocksize;
					else ex = AreaEndX;

					for (int j = 0; j < ih; j++)
					{
						sy = AreaStartY + j * blocksize;
						if (j < ih - 1) ey = sy + blocksize;
						else ey = AreaEndY;

						InspectionProperty ip = new InspectionProperty();
						ip.p_InspType = GetInspectionType(dCode);
						ip.m_nDefectCode = dCode;
						ip.p_index = blockcount;
						ip.MemoryPoolName = poolName;
						ip.MemoryOffset = memOffset;
						ip.p_TargetMemWidth = memWidth;
						ip.p_TargetMemHeight = memHeight;

						CRect inspblock = new CRect(sx, sy, ex, ey);
						ip.p_Rect = inspblock;
						if (ip.p_InspType == InspectionType.Strip)
						{
							ip.p_StripParam = (StripParamData)param;
						}
						else if (ip.p_InspType == InspectionType.AbsoluteSurface || ip.p_InspType == InspectionType.AbsoluteSurface)
						{
							ip.p_surfaceParam = (SurfaceParamData)param;
						}

						AddInspection(ip, bDefectMerge, nMergeDistance);
						blockcount++;

						inspblocklist.Add(inspblock);
					}
					//inspection offset, 모서리 영역 미구현

				}
			}

			return inspblocklist;
		}
		/// <summary>
		/// Defect Code를 int형식으로 변환하여 반환
		/// </summary>
		/// <param name="target">검사 대상</param>
		/// <param name="inspType">검사 알고리즘 종류</param>
		/// <param name="idx">ROI Index</param>
		/// <returns></returns>
		public static int MakeDefectCode(InspectionTarget target, InspectionType inspType, int idx)
		{
			int result = 0;

			result += (((int)target) * 10000);
			result += (((int)inspType) * 100);
			result += idx;

			return result;
		}
		/// <summary>
		/// Defect Code를 6자리 string 변환하여 반환 (앞에 0채움)
		/// </summary>
		/// <param name="target">검사 대상</param>
		/// <param name="inspType">검사 알고리즘 종류</param>
		/// <param name="idx">ROI Index</param>
		/// <returns></returns>
		public static string MakeDefectCodeToString(InspectionTarget target, InspectionType inspType, int idx)
		{
			int result = MakeDefectCode(target, inspType, idx);
			return result.ToString("D6");
		}
		public static InspectionTarget GetInspectionTarget(int nDefectCode)
		{
			return (InspectionTarget)(nDefectCode / 10000);
		}
		public static InspectionType GetInspectionType(int nDefectCode)
		{
			int target = Convert.ToInt32(nDefectCode / 10000) * 10000;
			int result = (nDefectCode - target) / 100;
			return (InspectionType)result;
		}
		public static int GetROIInfo(int nDefectCode)
		{
			int target = Convert.ToInt32(nDefectCode / 10000) * 10000;
			int type = Convert.ToInt32((nDefectCode - target) / 100) * 100;
			return (nDefectCode - target - type);
		}
		//static ??
		private ObservableQueue<InspectionProperty> qInspection = new ObservableQueue<InspectionProperty>();
		public ObservableQueue<InspectionProperty> p_qInspection
		{
			get
			{
				return qInspection;
			}
			set
			{
				qInspection = value;
				OnPropertyChanged("p_qInspection");
			}
		}


		static object lockObj = new object();


		#region PropertyChanged
		public virtual event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string proertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(proertyName));
		}
		#endregion

		public static unsafe DPoint GetEdge(ImageData img, System.Windows.Rect rcROI, eEdgeFindDirection eDirection, bool bUseAutoThreshold, bool bUseB2D, int nThreshold)
		{
			// variable
			int nSum = 0;
			double dAverage = 0.0;
			int nEdgeY = 0;
			int nEdgeX = 0;

			// implement

			if (bUseAutoThreshold == true)
			{
				nThreshold = GetThresholdAverage(img, rcROI, eDirection);
			}

			switch (eDirection)
			{
				case eEdgeFindDirection.TOP:
					for (int i = 0; i < rcROI.Height; i++)
					{
						byte* bp;
						if (bUseB2D == true) bp = (byte*)(img.GetPtr((int)rcROI.Bottom - i, (int)rcROI.Left).ToPointer());
						else bp = (byte*)(img.GetPtr((int)rcROI.Top + i, (int)rcROI.Left).ToPointer());
						for (int j = 0; j < rcROI.Width; j++)
						{
							nSum += *bp;
							bp++;
						}
						dAverage = nSum / rcROI.Width;
						if (bUseB2D == true)
						{
							if (dAverage < nThreshold)
							{
								nEdgeY = (int)rcROI.Bottom - i;
								nEdgeX = (int)(rcROI.Left + (rcROI.Width / 2));
								break;
							}
						}
						else
						{
							if (dAverage > nThreshold)
							{
								nEdgeY = (int)rcROI.Top + i;
								nEdgeX = (int)(rcROI.Left + (rcROI.Width / 2));
								break;
							}
						}
						nSum = 0;
					}
					break;
				case eEdgeFindDirection.LEFT:
					for (int i = 0; i < rcROI.Width; i++)
					{
						byte* bp;
						if (bUseB2D == true) bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Right - i));
						else bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Left + i));
						for (int j = 0; j < rcROI.Height; j++)
						{
							nSum += *bp;
							bp += img.p_Stride;
						}
						dAverage = nSum / rcROI.Height;
						if (bUseB2D == true)
						{
							if (dAverage < nThreshold)
							{
								nEdgeX = (int)rcROI.Right - i;
								nEdgeY = (int)(rcROI.Top + (rcROI.Height / 2));
								break;
							}
						}
						else
						{
							if (dAverage > nThreshold)
							{
								nEdgeX = (int)rcROI.Left + i;
								nEdgeY = (int)(rcROI.Top + (rcROI.Height / 2));
								break;
							}
						}
						nSum = 0;
					}
					break;
				case eEdgeFindDirection.RIGHT:
					for (int i = 0; i < rcROI.Width; i++)
					{
						byte* bp;
						if (bUseB2D == true) bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Left + i));
						else bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Right - i));
						for (int j = 0; j < rcROI.Height; j++)
						{
							nSum += *bp;
							bp += img.p_Stride;
						}
						dAverage = nSum / rcROI.Height;
						if (bUseB2D == true)
						{
							if (dAverage < nThreshold)
							{
								nEdgeX = (int)rcROI.Left + i;
								nEdgeY = (int)(rcROI.Top + (rcROI.Height / 2));
								break;
							}
						}
						else
						{
							if (dAverage > nThreshold)
							{
								nEdgeX = (int)rcROI.Right - i;
								nEdgeY = (int)(rcROI.Top + (rcROI.Height / 2));
								break;
							}
						}
						nSum = 0;
					}
					break;
				case eEdgeFindDirection.BOTTOM:
					for (int i = 0; i < rcROI.Height; i++)
					{
						byte* bp;
						if (bUseB2D == true) bp = (byte*)(img.GetPtr((int)rcROI.Top + i, (int)rcROI.Left).ToPointer());
						else bp = (byte*)(img.GetPtr((int)rcROI.Bottom - i, (int)rcROI.Left).ToPointer());
						for (int j = 0; j < rcROI.Width; j++)
						{
							nSum += *bp;
							bp++;
						}
						dAverage = nSum / rcROI.Width;
						if (bUseB2D == true)
						{
							if (dAverage < nThreshold)
							{
								nEdgeY = (int)rcROI.Top + i;
								nEdgeX = (int)(rcROI.Left + (rcROI.Width / 2));
								break;
							}
						}
						else
						{
							if (dAverage > nThreshold)
							{
								nEdgeY = (int)rcROI.Bottom - i;
								nEdgeX = (int)(rcROI.Left + (rcROI.Width / 2));
								break;
							}
						}

						nSum = 0;
					}
					break;
			}

			return new System.Drawing.Point(nEdgeX, nEdgeY);
		}

		static unsafe int GetThresholdAverage(ImageData img, System.Windows.Rect rcROI, eEdgeFindDirection eDirection)
		{
			// variable
			int nSum = 0;
			int nThreshold = 40;

			// implement

			if (eDirection == eEdgeFindDirection.TOP || eDirection == eEdgeFindDirection.BOTTOM)
			{
				double dRatio = rcROI.Height * 0.1;
				double dAverage1 = 0.0;
				double dAverage2 = 0.0;
				for (int i = 0; i < (int)dRatio; i++)
				{
					byte* bp = (byte*)(img.GetPtr((int)rcROI.Bottom - i, (int)rcROI.Left).ToPointer());
					for (int j = 0; j < rcROI.Width; j++)
					{
						nSum += *bp;
						bp++;
					}
				}
				dAverage1 = nSum / (rcROI.Width * (int)dRatio);
				nSum = 0;
				for (int i = 0; i < (int)dRatio; i++)
				{
					byte* bp = (byte*)(img.GetPtr((int)rcROI.Top + i, (int)rcROI.Left).ToPointer());
					for (int j = 0; j < rcROI.Width; j++)
					{
						nSum += *bp;
						bp++;
					}
				}
				dAverage2 = nSum / (rcROI.Width * (int)dRatio);
				nSum = 0;
				////////////////////////////////////////////////
				nThreshold = (int)(dAverage1 + dAverage2) / 2;
			}
			else
			{
				double dRatio = rcROI.Width * 0.1;
				double dAverage1 = 0.0;
				double dAverage2 = 0.0;
				for (int i = 0; i < (int)dRatio; i++)
				{
					byte* bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Right - i));
					for (int j = 0; j < rcROI.Height; j++)
					{
						nSum += *bp;
						bp += img.p_Stride;
					}
				}
				dAverage1 = nSum / (rcROI.Height * (int)dRatio);
				nSum = 0;
				for (int i = 0; i < (int)dRatio; i++)
				{
					byte* bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Left + i));
					for (int j = 0; j < rcROI.Height; j++)
					{
						nSum += *bp;
						bp += img.p_Stride;
					}
				}
				dAverage2 = nSum / (rcROI.Height * (int)dRatio);
				nSum = 0;
				////////////////////////////////////////////////
				nThreshold = (int)(dAverage1 + dAverage2) / 2;
			}

			return nThreshold;
		}
		/// <summary>
		/// ROI 영역 내의 성분 방향을 획득하여 반환한다
		/// </summary>
		/// <param name="img"></param>
		/// <param name="rcROI"></param>
		/// <returns></returns>
		public static unsafe eEdgeFindDirection GetDirection(ImageData img, System.Windows.Rect rcROI)
		{
			// variable
			double dRatio = 0.0;
			int nSum = 0;
			double dAverageTemp = 0.0;
			Dictionary<eBrightSide, double> dic = new Dictionary<eBrightSide, double>();

			// implement
			// Left
			dRatio = rcROI.Width * 0.1;
			for (int i = 0; i < (int)dRatio; i++)
			{
				byte* bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Left + i));
				for (int j = 0; j < rcROI.Height; j++)
				{
					nSum += *bp;
					bp += img.p_Stride;
				}
			}
			dAverageTemp = nSum / (rcROI.Height * (int)dRatio);
			dic.Add(eBrightSide.LEFT, dAverageTemp);
			nSum = 0;

			// Top
			dRatio = rcROI.Height * 0.1;
			for (int i = 0; i < (int)dRatio; i++)
			{
				byte* bp = (byte*)(img.GetPtr((int)rcROI.Top + i, (int)rcROI.Left).ToPointer());
				for (int j = 0; j < rcROI.Width; j++)
				{
					nSum += *bp;
					bp++;
				}
			}
			dAverageTemp = nSum / (rcROI.Width * (int)dRatio);
			dic.Add(eBrightSide.TOP, dAverageTemp);
			nSum = 0;

			// Right
			dRatio = rcROI.Width * 0.1;
			for (int i = 0; i < (int)dRatio; i++)
			{
				byte* bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Right - i).ToPointer());
				for (int j = 0; j < rcROI.Height; j++)
				{
					nSum += *bp;
					bp += img.p_Stride;
				}
			}
			dAverageTemp = nSum / (rcROI.Height * (int)dRatio);
			dic.Add(eBrightSide.RIGHT, dAverageTemp);
			nSum = 0;

			// Bottom
			dRatio = rcROI.Height * 0.1;
			for (int i = 0; i < (int)dRatio; i++)
			{
				byte* bp = (byte*)(img.GetPtr((int)rcROI.Bottom - i, (int)rcROI.Left).ToPointer());
				for (int j = 0; j < rcROI.Width; j++)
				{
					nSum += *bp;
					bp++;
				}
			}
			dAverageTemp = nSum / (rcROI.Width * (int)dRatio);
			dic.Add(eBrightSide.BOTTOM, dAverageTemp);
			nSum = 0;

			var maxKey = dic.Keys.Max();
			var maxValue = dic.Values.Max();
			// Value값이 가장 큰 Key값 찾기
			var keyOfMaxValue = dic.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;

			if (keyOfMaxValue == eBrightSide.TOP) return eEdgeFindDirection.BOTTOM;
			else if (keyOfMaxValue == eBrightSide.BOTTOM) return eEdgeFindDirection.TOP;
			else if (keyOfMaxValue == eBrightSide.LEFT) return eEdgeFindDirection.RIGHT;
			else return eEdgeFindDirection.LEFT;
		}
	}

	public class InspectionProperty : ObservableObject
	{
		InspectionType InspType = InspectionType.None;
		public InspectionType p_InspType
		{
			get
			{
				return InspType;
			}
			set
			{
				SetProperty(ref InspType, value);
			}
		}
		int targetMemWidth;
		public int p_TargetMemWidth
		{
			get { return this.targetMemWidth; }
			set
			{
				SetProperty(ref targetMemWidth, value);
			}
		}
		int targetMemHeight;
		public int p_TargetMemHeight
		{
			get { return this.targetMemHeight; }
			set
			{
				SetProperty(ref targetMemHeight, value);
			}
		}
		CRect Rect;
		public CRect p_Rect
		{
			get
			{
				return Rect;
			}
			set
			{
				SetProperty(ref Rect, value);
			}
		}
		public int m_nDefectCode;
		bool bDefectMerge;
		public bool p_bDefectMerge
		{
			get
			{
				return bDefectMerge;
			}
			set
			{
				SetProperty(ref bDefectMerge, value);
			}
		}
		int nMergeDistance;
		public int p_nMergeDistance
		{
			get
			{
				return nMergeDistance;
			}
			set
			{
				SetProperty(ref nMergeDistance, value);
			}
		}
		SurfaceParamData surfaceParam;
		public SurfaceParamData p_surfaceParam
		{
			get
			{
				return surfaceParam;
			}
			set
			{
				SetProperty(ref surfaceParam, value);
			}
		}
		StripParamData stripParam;
		public StripParamData p_StripParam
		{
			get
			{
				return stripParam;
			}
			set
			{
				SetProperty(ref stripParam, value);
			}
		}
		int index = 0;
		public int p_index
		{
			get
			{
				return index;
			}
			set
			{
				SetProperty(ref index, value);
			}
		}
		public string MemoryPoolName { get; set; }
		public ulong MemoryOffset { get; set; }
	}
	public class MemInfo
	{


	}
}
