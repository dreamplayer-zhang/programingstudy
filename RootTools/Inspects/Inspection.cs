using RootTools_CLR;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel.Configuration;
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
		public delegate void ChangeDefectInfoEventHander(DefectDataWrapper[] source, int nDCode);
		public event ChangeDefectInfoEventHander AddDefect;
		#endregion

		InspectionType inspectionType;

		int threadIndex = -1;
		int inspectionID = -1;
		public InspectionState bState = InspectionState.None;
		Thread _thread;
		InspectionProperty m_InspProp;
		private volatile bool shouldStop = false;
		int m_nThreadNum;

		public int ThreadIndex { get { return threadIndex; } set { threadIndex = value; } }
		public int InspectionID { get { return inspectionID; } set { inspectionID = value; } }

		int m_nWholeImageWidth;
		int m_nWholeImageHeight;
		int m_nDefectCode;
		public bool IsInitialized { get; private set; }

		public Inspection(int nWholeImageWidth, int nWholeImageHeight, int nDefectCode, int nThreadNum)
		{
			m_nWholeImageWidth = nWholeImageWidth;
			m_nWholeImageHeight = nWholeImageHeight;
			m_nDefectCode = nDefectCode;
			m_nThreadNum = nThreadNum;

			IsInitialized = true;
		}
		public void Dispose()
		{
			//clrInsp.Dispose();
		}

		public bool StartInspection(InspectionProperty prop, int threadIndex)
		{
			if (!IsInitialized)
				return false;

			shouldStop = false;
			ThreadIndex = threadIndex;
			m_InspProp = prop;
			m_InspProp.m_nDefectCode = m_nDefectCode;
			InspectionID = prop.p_index;
			bState = InspectionState.Run;
			_thread = new Thread(DoInspection);
			inspectionType = InspectionManager.GetInspectionType(m_nDefectCode);
			//_thread.IsBackground = true;
			_thread.Start();

			return true;
		}

		public void DoInspection(object threadId)
		{
			if (!IsInitialized)
				return;

			while (shouldStop == false)
			{
				if (bState == InspectionState.Run)
				{
					Console.WriteLine(string.Format("Inspection ID : {0} Thread Index {1}", InspectionID, ThreadIndex));
					bState = InspectionState.Running;


					List<DefectDataWrapper> arrDefects = new List<DefectDataWrapper>();
					if (inspectionType == InspectionType.AbsoluteSurface && inspectionType == InspectionType.RelativeSurface)
					{
						using (CLR_Inspection clrInsp = new CLR_Inspection(m_nThreadNum, m_InspProp.p_Rect.Width, m_InspProp.p_Rect.Height))
						{
							var temp = clrInsp.SurfaceInspection(
								ThreadIndex,
								m_InspProp.m_nDefectCode,
								m_InspProp.p_Rect.Left,
								m_InspProp.p_Rect.Top,
								m_InspProp.p_Rect.Right,
								m_InspProp.p_Rect.Bottom,
								m_nWholeImageWidth,
								m_nWholeImageHeight,
								m_InspProp.p_surfaceParam.p_GV,
								m_InspProp.p_surfaceParam.p_DefectSize,
								m_InspProp.p_surfaceParam.p_bDarkInspection,
								m_InspProp.p_surfaceParam.p_bAbsoluteInspection);
							foreach (var item in temp)
							{
								arrDefects.Add(new DefectDataWrapper(item));
							}
						}
					}
					else if (inspectionType == InspectionType.Strip)
					{
						using (CLR_Inspection clrInsp = new CLR_Inspection(m_nThreadNum, m_InspProp.p_Rect.Width, m_InspProp.p_Rect.Height))
						{
							var temp = clrInsp.StripInspection(
									   ThreadIndex,
									   m_InspProp.m_nDefectCode,
									   m_InspProp.p_Rect.Left,
									   m_InspProp.p_Rect.Top,
									   m_InspProp.p_Rect.Right,
									   m_InspProp.p_Rect.Bottom,
									   m_nWholeImageWidth,
									   m_nWholeImageHeight,
									   m_InspProp.p_StripParam.p_GV,
									   m_InspProp.p_StripParam.p_DefectSize,
									   m_InspProp.p_StripParam.p_Intensity,
									   m_InspProp.p_StripParam.p_Bandwidth);
							foreach (var item in temp)
							{
								arrDefects.Add(new DefectDataWrapper(item));
							}
						}
					}
					if (m_InspProp.p_bDefectMerge)//TODO : 기능 개선이 필요함. UI에 표시할때의 변수가 별도로 있는 것이 좋을 것으로 보임 + Defect Clustering구현
					{
						arrDefects = DefectDataWrapper.MergeDefect(arrDefects.ToArray(), m_InspProp.p_nMergeDistance);
					}
					if (AddDefect != null)//대리자 호출을 간단하게 만들 수 있으나 vs2013에서 호환이 안 될 가능성이 없어 보류
					{
						AddDefect(arrDefects.ToArray(), m_nDefectCode);
					}
					arrDefects.Clear();
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
