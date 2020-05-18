using System.ComponentModel;
using System.Threading;
using System.Collections;
using System.Data;
using ATI;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System;

namespace RootTools.Inspects
{
	public class InspectionManager : Singleton<InspectionManager>, INotifyPropertyChanged
	{
		#region EventHandler
		/// <summary>
		/// 이벤트 핸들러
		/// </summary>
		public delegate void EventHandler(InspectionType type);
		public EventHandler InspectionStart;
		public EventHandler InspectionComplete;
		/// <summary>
		/// Defect 정보 변경 시 사용할 Event Handler
		/// </summary>
		/// <param name="source">Defect List</param>
		/// <param name="args">arguments. 필요한 경우 수정해서 사용</param>
		public delegate void ChangeDefectInfoEventHanlder(DefectData[] source, InspectionType type);
		/// <summary>
		/// UI에 Defect을 추가하기 위해 발생하는 Event
		/// </summary>
		public event ChangeDefectInfoEventHanlder AddDefect;
		#endregion

		int nThreadNum = 4;
		int nInspectionCount = 0;

		InspectionType inspectionType;

		public void StartInspection(InspectionType type)
		{
			inspectionType = type;

			if (0 < GetWaitQueue())
			{
				if (GetWaitQueue() < nThreadNum)
				{
					nThreadNum = GetWaitQueue();
				}

				Thread thread = new Thread(DoInspection);
				thread.Start();
			}
		}
		public void DoInspection()
		{
			int nInspDoneNum = 0;
			Inspection[] inspection = new Inspection[nThreadNum];

			if (InspectionStart != null)
			{
				InspectionStart(inspectionType);//DB Write 준비 시작
			}

			for (int i = 0; i < nThreadNum; i++)
			{
				inspection[i] = new Inspection();
				inspection[i].AddDefect += InspectionManager_AddDefect;
			}

			while (true)
			{
				lock (lockObj)
				{
					for (int i = 0; i < nThreadNum; i++)
					{
						if (inspection[i].bState == Inspection.InspectionState.Done)
						{
							inspection[i].bState = Inspection.InspectionState.Ready;
							nInspDoneNum++;
						}
					}

					while (p_qInspection.Count == 0 && nInspDoneNum == nInspectionCount)
					{
						InspectionDone();
						Monitor.Wait(lockObj);
					}

					for (int i = 0; i < nThreadNum; i++)
					{
						if (0 < GetWaitQueue())
						{
							if (inspection[i].bState == Inspection.InspectionState.Ready ||
							inspection[i].bState == Inspection.InspectionState.None)
							{
								nInspectionCount++;
								InspectionProperty ipQueue = p_qInspection.Dequeue();
								inspection[i].StartInspection(ipQueue, i);
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
		private void InspectionManager_AddDefect(DefectData[] source, InspectionType type)
		{
			#region DEBUG

#if DEBUG
			foreach (DefectData data in source)
			{
				StringBuilder stbr = new StringBuilder();
				stbr.Append(data.nIdx);
				stbr.Append(",");
				stbr.Append(data.fSize);
				stbr.Append(",");
				stbr.Append(data.fPosX);
				stbr.Append(",");
				stbr.Append(data.fPosY);
				System.Diagnostics.Debug.WriteLine(stbr.ToString());
			}
#endif
			#endregion

			if (AddDefect != null)
			{
				AddDefect(source, inspectionType);
			}
		}

		public void InspectionDone()
		{
			//TODO : 해당 Queue로 들어온 검사가 완전 종료되었을때 발동. 여기서 DB를 닫으면 될 것으로 보임
			if (InspectionComplete != null)
			{
				InspectionComplete(inspectionType);
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

		public void AddInspection(InspectionProperty _property)//(in InspectionProperty _property) //2013 in 안됨
		{
			p_qInspection.Enqueue(_property);
		}

		public void ClearInspection()
		{
			p_qInspection.Clear();
		}
		public List<CRect> CreateInspArea(CRect WholeInspArea, int blocksize, SurFace_ParamData param)
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

			if (iw == 0 || ih == 0)
			{
				// return;
			}
			else
			{
				//insp blockd에 대한 start xy, end xy
				int sx, sy, ex, ey;
				int blockcount = 1;

				for (int i = 0; i < iw; i++)
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
						ip.p_InspType = RootTools.Inspects.InspectionType.Surface;

						CRect inspblock = new CRect(sx, sy, ex, ey);
						ip.p_Rect = inspblock;
						ip.p_Sur_Param = param;
						ip.p_index = blockcount;
						AddInspection(ip);
						blockcount++;

						inspblocklist.Add(inspblock);
					}
					//inspection offset, 모서리 영역 미구현

				}
			}

			return inspblocklist;

		}
		public List<CRect> CreateInspArea(CRect WholeInspArea, int blocksize, StripParamData param)
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

			if (iw == 0 || ih == 0)
			{
				// return;
			}
			else
			{
				//insp blockd에 대한 start xy, end xy
				int sx, sy, ex, ey;
				int blockcount = 1;

				for (int i = 0; i < iw; i++)
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
						ip.p_InspType = RootTools.Inspects.InspectionType.Surface;

						CRect inspblock = new CRect(sx, sy, ex, ey);
						ip.p_Rect = inspblock;
						ip.p_StripParam = param;
						ip.p_index = blockcount;
						AddInspection(ip);
						blockcount++;

						inspblocklist.Add(inspblock);
					}
					//inspection offset, 모서리 영역 미구현

				}
			}

			return inspblocklist;

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
		SurFace_ParamData Sur_Param;
		public SurFace_ParamData p_Sur_Param
		{
			get
			{
				return Sur_Param;
			}
			set
			{
				SetProperty(ref Sur_Param, value);
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
	}

	public enum InspectionType
	{
		None,
		Surface,
		Strip,
	};
}
