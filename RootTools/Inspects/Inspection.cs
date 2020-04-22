using RootTools_CLR;
using System;
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
		public delegate void ChangeDefectInfoEventHander(DefectData[] source, EventArgs args);
		public event ChangeDefectInfoEventHander AddDefect;
		#endregion

		int threadIndex = -1;
		int inspectionID = -1;
		public InspectionState bState = InspectionState.None;
		Thread _thread;
		CLR_Inspection clrDemo = new CLR_Inspection();
		InspectionProperty m_InspProp;
		private volatile bool shouldStop = false;

		public int ThreadIndex { get { return threadIndex; } set { threadIndex = value; } }
		public int InspectionID { get { return inspectionID; } set { inspectionID = value; } }

		public void StartInspection(InspectionProperty prop, int threadIndex)
		{
			shouldStop = false;
			ThreadIndex = threadIndex;
			m_InspProp = prop;
			InspectionID = prop.p_index;
			bState = InspectionState.Run;
			_thread = new Thread(DoInspection);
			//_thread.IsBackground = true;
			_thread.Start();
		}

		public void DoInspection(object threadId)
		{
			while (!shouldStop)
			{
				if (bState == InspectionState.Run)
				{
					Console.WriteLine(string.Format("Inspection ID : {0} Thread Index {1}", InspectionID, ThreadIndex));
					bState = InspectionState.Running;


					var arrDefects = clrDemo.Test_Inspection(
						ThreadIndex,
						m_InspProp.p_Rect.Left,
						m_InspProp.p_Rect.Top,
						m_InspProp.p_Rect.Right,
						m_InspProp.p_Rect.Bottom,
						10000,
						10000,
						m_InspProp.p_Sur_Param.p_GV,
						m_InspProp.p_Sur_Param.p_DefectSize,
						m_InspProp.p_Sur_Param.p_bDarkInspection,
						m_InspProp.p_Sur_Param.p_bAbsoluteInspection);
					if (AddDefect != null)//대리자 호출을 간단하게 만들 수 있으나 vs2013에서 호환이 안 될 가능성이 없어 보류
					{
						AddDefect(arrDefects, EventArgs.Empty);
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
