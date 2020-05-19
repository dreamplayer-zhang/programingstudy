using RootTools_CLR;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RootTools.Inspects
{
	public class Inspection
	{
		#region EventHandler
		/// <summary>
		/// 이벤트 핸들러
		/// </summary>
		public delegate void EventHandler();
		public EventHandler InspectionStart;
		public EventHandler InspectionComplete;
		public delegate void ChangeDefectInfoEventHander(DefectData[] source, InspectionType type);
		public event ChangeDefectInfoEventHander AddDefect;
		#endregion

		InspectionType inspectionType;

		int threadIndex = -1;
		int inspectionID = -1;
		public InspectionState bState = InspectionState.None;
		Thread _thread;
		CLR_Inspection clrInsp = new CLR_Inspection();
		InspectionProperty m_InspProp;
		private volatile bool shouldStop = false;

		public int ThreadIndex { get { return threadIndex; } set { threadIndex = value; } }
		public int InspectionID { get { return inspectionID; } set { inspectionID = value; } }

		int m_nWholeImageWidth;
		int m_nWholeImageHeight;
		public bool IsInitialized { get; private set; }

		public Inspection(int nWholeImageWidth, int nWholeImageHeight)
		{
			m_nWholeImageWidth = nWholeImageWidth;
			m_nWholeImageHeight = nWholeImageHeight;
			IsInitialized = true;
		}

		public bool StartInspection(InspectionProperty prop, int threadIndex)
		{
			if (!IsInitialized)
				return false;

			shouldStop = false;
			ThreadIndex = threadIndex;
			m_InspProp = prop;
			InspectionID = prop.p_index;
			bState = InspectionState.Run;
			_thread = new Thread(DoInspection);
			inspectionType = prop.p_InspType;
			//_thread.IsBackground = true;
			_thread.Start();

			return true;
		}

		public void DoInspection(object threadId)
		{
			while (!shouldStop)
			{
				if (bState == InspectionState.Run)
				{
					Console.WriteLine(string.Format("Inspection ID : {0} Thread Index {1}", InspectionID, ThreadIndex));
					bState = InspectionState.Running;


					List<DefectData> arrDefects = new List<DefectData>();
					switch (inspectionType)
					{
						case InspectionType.Surface:
							arrDefects.AddRange(clrInsp.SurfaceInspection(
								ThreadIndex,
								m_InspProp.p_Rect.Left,
								m_InspProp.p_Rect.Top,
								m_InspProp.p_Rect.Right,
								m_InspProp.p_Rect.Bottom,
								m_nWholeImageWidth,
								m_nWholeImageHeight,
								m_InspProp.p_Sur_Param.p_GV,
								m_InspProp.p_Sur_Param.p_DefectSize,
								m_InspProp.p_Sur_Param.p_bDarkInspection,
								m_InspProp.p_Sur_Param.p_bAbsoluteInspection));
							break;
						case InspectionType.Strip:
							arrDefects.AddRange(clrInsp.StripInspection(
								ThreadIndex,
								m_InspProp.p_Rect.Left,
								m_InspProp.p_Rect.Top,
								m_InspProp.p_Rect.Right,
								m_InspProp.p_Rect.Bottom,
								m_nWholeImageWidth,
								m_nWholeImageHeight,
								m_InspProp.p_StripParam.p_GV,
								m_InspProp.p_StripParam.p_DefectSize,
								m_InspProp.p_StripParam.p_Intensity,
								m_InspProp.p_StripParam.p_Bandwidth));
							break;
						case InspectionType.None:
						default:
							break;
					}

					if (AddDefect != null)//대리자 호출을 간단하게 만들 수 있으나 vs2013에서 호환이 안 될 가능성이 없어 보류
					{
						AddDefect(arrDefects.ToArray(), inspectionType);
					}
				}
				else if (bState == InspectionState.Running)
				{
					shouldStop = true;
					bState = InspectionState.Done;
					_thread.Join();
				}
			}
		}

		public enum InspectionState
		{
			None,
			Ready,
			Run,
			Running,
			Done
		};
	}
}
