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

namespace RootTools.Inspects
{
	public class InspectionManager : Singleton<InspectionManager>, INotifyPropertyChanged
	{
		#region EventHandler
		/// <summary>
		/// Defect 정보 변경 시 사용할 Event Handler
		/// </summary>
		/// <param name="item">Defect Information</param>
		/// <param name="args">arguments. 필요한 경우 수정해서 사용</param>
		public delegate void ChangeDefectInfoEventHanlder(DefectDataWrapper item, int nDCode);
		/// <summary>
		/// UI에 Defect을 추가하기 위해 발생하는 Event
		/// </summary>
		public event ChangeDefectInfoEventHanlder AddDefect;
		#endregion

		Thread inspThread;
		Inspection[] InsepctionThread;

		int nThreadNum = 10;
		int nInspectionCount = 0;

		//int m_nDefectCode;

		//int m_nMemWidth;
		//int m_nMemHeight;

		bool m_bProgress;

		public bool IsInitialized { get; private set; }

		public void StartInspection()
		{
			//m_nDefectCode = nDefectCode;
			//m_nMemWidth = nMemWidth;
			//m_nMemHeight = nMemHegiht;
			m_bProgress = false;
			nInspectionCount = 0;

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

			//if (InspectionStart != null)
			//{
			//	InspectionStart(m_nDefectCode);//DB Write 준비 시작
			//}

			for (int i = 0; i < nThreadNum; i++)
			{
				InsepctionThread[i] = new Inspection(nThreadNum);
				InsepctionThread[i].AddDefect += InspectionManager_AddDefect;
			}

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
		private void InspectionManager_AddDefect(DefectDataWrapper item, int nDCode)
		{
			if (AddDefect != null)
			{
				AddDefect(item, nDCode);
			}
		}

		public void InspectionDone()
		{
			////TODO : 해당 Queue로 들어온 검사가 완전 종료되었을때 발동. 여기서 DB를 닫으면 될 것으로 보임
			//if (InspectionComplete != null)
			//{
			//	InspectionComplete();
			//}
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
		public List<CRect> CreateInspArea(string poolName, ulong memOffset, int memWidth, int memHeight, CRect WholeInspArea, int blocksize, BaseParamData param, RootTools.Inspects.InspectionType insptype, bool bDefectMerge, int nMergeDistance)
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
						ip.p_InspType = insptype;

						CRect inspblock = new CRect(sx, sy, ex, ey);
						ip.p_Rect = inspblock;
						if (insptype == InspectionType.Strip)
						{
							ip.p_StripParam = (StripParamData)param;
						}
						else if (insptype == InspectionType.AbsoluteSurface || insptype == InspectionType.AbsoluteSurface)
						{
							ip.p_surfaceParam = (SurfaceParamData)param;
						}
						//TODO : Memory pool 이름, offset 계산하여 AddInspection 하기 전에 InspectionProperty로 넘겨줘야 함
						ip.p_index = blockcount;
						ip.MemoryPoolName = poolName;
						ip.MemoryOffset = memOffset;
						ip.p_TargetMemWidth = memWidth;
						ip.p_TargetMemHeight = memHeight;

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
}
