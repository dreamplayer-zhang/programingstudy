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
		public delegate void ChangeDefectInfoEventHander(DefectDataWrapper item);
		public event ChangeDefectInfoEventHander AddChromeDefect;
		public event ChangeDefectInfoEventHander AddEBRDefect;
		public event ChangeDefectInfoEventHander AddTopSideDefect;
		public event ChangeDefectInfoEventHander AddLeftSideDefect;
		public event ChangeDefectInfoEventHander AddRightSideDefect;
		public event ChangeDefectInfoEventHander AddBotSideDefect;
		public event ChangeDefectInfoEventHander AddTopBevelDefect;
		public event ChangeDefectInfoEventHander AddLeftBevelDefect;
		public event ChangeDefectInfoEventHander AddRightBevelDefect;
		public event ChangeDefectInfoEventHander AddBotBevelDefect;
		public event ChangeDefectInfoEventHander AddD2DDefect;
		#endregion

		int threadIndex = -1;
		int inspectionID = -1;
		public InspectionState bState = InspectionState.None;
		Thread _thread;
		InspectionProperty m_InspProp;
		private volatile bool shouldStop = false;
		int m_nThreadNum;

		public int ThreadIndex { get { return threadIndex; } set { threadIndex = value; } }
		public int InspectionID { get { return inspectionID; } set { inspectionID = value; } }

		//int m_nWholeImageWidth;
		//int m_nWholeImageHeight;
		//int m_nDefectCode;
		public bool IsInitialized { get; private set; }

		public Inspection(int nThreadNum)
		{
			//m_nWholeImageWidth = nWholeImageWidth;
			//m_nWholeImageHeight = nWholeImageHeight;
			//m_nDefectCode = nDefectCode;
			m_nThreadNum = nThreadNum;

			IsInitialized = true;
		}
		public void Dispose()
		{
			//clrInsp.Dispose();
			if (_thread.IsAlive)
			{
				//_thread.Interrupt();
				_thread.Interrupt();
				shouldStop = false;
			}
		}



		public void D2DRectInsp(CCLRD2DStructure StrucRef)
		{

			CCLRD2DModule testModule = new CCLRD2DModule();
			testModule.D2DInspProto();
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
			//_thread.IsBackground = true;
			_thread.Start();

			return true;
		}

		public void DoInspection(object threadId)
		{
			if (!IsInitialized)
				return;
			CLR_Inspection clrInsp = new CLR_Inspection(m_nThreadNum, m_InspProp.p_Rect.Width, m_InspProp.p_Rect.Height);
			while (shouldStop == false)
			{
				if (bState == InspectionState.Run)
				{
					//Console.WriteLine(string.Format("Inspection ID : {0} Thread Index {1}", InspectionID, ThreadIndex));
					bState = InspectionState.Running;


					List<DefectDataWrapper> arrDefects = new List<DefectDataWrapper>();
					if (m_InspProp.p_InspType == InspectionType.AbsoluteSurface || m_InspProp.p_InspType == InspectionType.RelativeSurface)
					{
						unsafe
						{
							var temp = clrInsp.SurfaceInspection(
							m_InspProp.MemoryPoolName,
							m_InspProp.MemoryGroupName,
							m_InspProp.MemoryName,
							m_InspProp.MemoryOffset,
							ThreadIndex,
							m_InspProp.m_nDefectCode,
							m_InspProp.p_Rect.Left,
							m_InspProp.p_Rect.Top,
							m_InspProp.p_Rect.Right,
							m_InspProp.p_Rect.Bottom,
							m_InspProp.p_TargetMemWidth,
							m_InspProp.p_TargetMemHeight,
							m_InspProp.p_surfaceParam.TargetGV,
							m_InspProp.p_surfaceParam.DefectSize,
							m_InspProp.p_surfaceParam.UseDarkInspection,
							m_InspProp.p_surfaceParam.UseAbsoluteInspection,
							m_InspProp.p_bDefectMerge,
							m_InspProp.p_nMergeDistance,
							(void*)m_InspProp.p_ptrMemory);

							foreach (var item in temp)
							{
								arrDefects.Add(new DefectDataWrapper(item));
							}
						}

					}
					else if (m_InspProp.p_InspType == InspectionType.Strip)
					{
						unsafe
						{
							var temp = clrInsp.StripInspection(
							m_InspProp.MemoryPoolName,
							m_InspProp.MemoryGroupName,
							m_InspProp.MemoryName,
							m_InspProp.MemoryOffset,
							ThreadIndex,
							m_InspProp.m_nDefectCode,
							m_InspProp.p_Rect.Left,
							m_InspProp.p_Rect.Top,
							m_InspProp.p_Rect.Right,
							m_InspProp.p_Rect.Bottom,
							m_InspProp.p_TargetMemWidth,
							m_InspProp.p_TargetMemHeight,
							m_InspProp.p_StripParam.TargetGV,
							m_InspProp.p_StripParam.DefectSize,
							m_InspProp.p_StripParam.Intensity,
							m_InspProp.p_StripParam.Bandwidth,
							m_InspProp.p_bDefectMerge,
							m_InspProp.p_nMergeDistance,
							(void*)m_InspProp.p_ptrMemory);

							foreach (var item in temp)
							{
								arrDefects.Add(new DefectDataWrapper(item));
							}
						}

					}
					//if (m_InspProp.p_bDefectMerge)//TODO : 기능 개선이 필요함. UI에 표시할때의 변수가 별도로 있는 것이 좋을 것으로 보임 + Defect Clustering구현
					//{
					//	arrDefects = DefectDataWrapper.MergeDefect(arrDefects.ToArray(), m_InspProp.p_nMergeDistance);
					//}
					if (arrDefects != null && arrDefects.Count > 0)
					{
						foreach (var item in arrDefects)
						{
							switch (InspectionManager.GetInspectionTarget(item.nClassifyCode))
							{
								case InspectionTarget.None:
									break;
								case InspectionTarget.Chrome:
									if (AddChromeDefect != null)
									{
										AddChromeDefect(item);
									}
									break;
								case InspectionTarget.Pellcile45:
									break;
								case InspectionTarget.Pellicle90:
									break;
								case InspectionTarget.FrameTop:
									break;
								case InspectionTarget.FrameLeft:
									break;
								case InspectionTarget.FrameBottom:
									break;
								case InspectionTarget.FrameRight:
									break;
								case InspectionTarget.Glass:
									break;
								case InspectionTarget.SideInspection:
									break;
								case InspectionTarget.SideInspectionTop:
									if (AddTopSideDefect != null)
									{
										AddTopSideDefect(item);

									}
									break;
								case InspectionTarget.SideInspectionLeft:
									if (AddLeftSideDefect != null)
									{
										AddLeftSideDefect(item);

									}
									break;
								case InspectionTarget.SideInspectionRight:
									if (AddRightSideDefect != null)
									{
										AddRightSideDefect(item);

									}
									break;
								case InspectionTarget.SideInspectionBottom:
									if (AddBotSideDefect != null)
									{
										AddBotSideDefect(item);

									}
									break;
								case InspectionTarget.BevelInspection:
									break;
								case InspectionTarget.BevelInspectionTop:
									if (AddTopBevelDefect != null)
									{
										AddTopBevelDefect(item);

									}
									break;
								case InspectionTarget.BevelInspectionLeft:
									if (AddLeftBevelDefect != null)
									{
										AddLeftBevelDefect(item);

									}
									break;
								case InspectionTarget.BevelInspectionRight:
									if (AddRightBevelDefect != null)
									{
										AddRightBevelDefect(item);

									}
									break;
								case InspectionTarget.BevelInspectionBottom:
									if (AddBotBevelDefect != null)
									{
										AddBotBevelDefect(item);

									}
									break;
								case InspectionTarget.D2D:
									if (AddD2DDefect != null)
									{
										AddD2DDefect(item);

									}
									break;
								case InspectionTarget.EBR:
									if (AddEBRDefect != null)
									{
										AddEBRDefect(item);

									}
									break;
							}
						}
					}
					arrDefects.Clear();
				}
				else if (bState == InspectionState.Running)
				{
					shouldStop = true;
					bState = InspectionState.Done;
					//여기서 완료이벤트?
					if (_thread.ThreadState == ThreadState.WaitSleepJoin)
					{
						_thread.Interrupt();
					}
				}
			}
			clrInsp.Dispose();
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
