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
		public delegate void ChangeDefectInfoEventHander(DefectData[] source, int nDCode);
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


					List<DefectData> arrDefects = new List<DefectData>();
					if (inspectionType == InspectionType.AbsoluteSurface && inspectionType == InspectionType.RelativeSurface)
					{
						using (CLR_Inspection clrInsp = new CLR_Inspection(m_nThreadNum, m_InspProp.p_Rect.Width, m_InspProp.p_Rect.Height))
						{
							arrDefects.AddRange(clrInsp.SurfaceInspection(
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
								m_InspProp.p_surfaceParam.p_bAbsoluteInspection));
						}
					}
					else if (inspectionType == InspectionType.Strip)
					{
						using (CLR_Inspection clrInsp = new CLR_Inspection(m_nThreadNum, m_InspProp.p_Rect.Width, m_InspProp.p_Rect.Height))
						{
							arrDefects.AddRange(clrInsp.StripInspection(
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
									   m_InspProp.p_StripParam.p_Bandwidth));
						}
					}
					if (m_InspProp.p_bDefectMerge)
					{
						arrDefects = MergeDefect(arrDefects, m_InspProp.p_nMergeDistance);
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

		private List<DefectData> MergeDefect(List<DefectData> arrDefects, int nMergeDistance)
		{
			//List<DefectData> resultList = new List<DefectData>();
			//return resultList;
			for (int i = 0; i < arrDefects.Count; i++)
			{
				if (arrDefects[i].bMergeUsed)
					continue;

				//0부터 전부 돈다
				//만약 merged옵션이 켜져있다면 해당 defect은 pass
				//한바퀴 다 돌면 기준 defect의 merge는 무조건 true(추후 처리를 위함)
				for (int j = i + 1; j < arrDefects.Count; j++)
				{
					if (arrDefects[i].nClassifyCode != arrDefects[j].nClassifyCode)
						continue;

					if (CheckMerge(arrDefects[i], arrDefects[j], nMergeDistance))
					{
						arrDefects[i] = MergeDefectInformation(arrDefects[i], arrDefects[j]);
						arrDefects[j].bMergeUsed = true;
					}
				}
				arrDefects[i].bMerged = true;
			}
			return arrDefects.Where(x => x.bMerged && !x.bMergeUsed).ToList();
		}
		/// <summary>
		/// Origin Data의 정보와 Target Data의 정보를 합친다. 기준은 Origin Data가 된다
		/// </summary>
		/// <param name="originData">합칠때 기준이 되는 Defect Data</param>
		/// <param name="targetData">합칠때 데이터를 보정할 정보로 쓰일 Defect Data</param>
		/// <returns></returns>
		private DefectData MergeDefectInformation(DefectData originData, DefectData targetData)
		{
			DefectData result = originData;

			result.fPosX = (originData.fPosX + targetData.fPosX) / 2.0;
			result.fPosY = (originData.fPosY + targetData.fPosY) / 2.0;

			result.nWidth += targetData.nWidth;
			result.nHeight += targetData.nHeight;
			result.fSize += targetData.fSize;

			result.nLength = result.nHeight;
			if (result.nHeight < result.nWidth)
			{
				result.nLength = result.nWidth;
			}

			return result;
		}

		private bool CheckMerge(DefectData data1, DefectData data2, int distance)
		{
			int data1Width = data1.nWidth + (distance * 2);
			int data1Height = data1.nHeight + (distance * 2);
			Rectangle data1Rect = new Rectangle(
				Convert.ToInt32(data1.fPosX - data1Width / 2.0),
				Convert.ToInt32(data1.fPosY - data1Height / 2.0),
				data1Width,
				data1Height);

			int data2Width = data2.nWidth + (distance * 2);
			int data2Height = data2.nHeight + (distance * 2);
			Rectangle data2Rect = new Rectangle(
				Convert.ToInt32(data2.fPosX - data2Width / 2.0),
				Convert.ToInt32(data2.fPosY - data2Height / 2.0),
				data2Width,
				data2Height);
			var result = Rectangle.Intersect(data1Rect, data2Rect);
			if(result.IsEmpty)
			{
				return false;
			}
			else
			{
				return true;
			}
			//return Convert.ToInt32(Math.Sqrt(Math.Pow(data1.fPosX - data2.fPosX, 2.0) + Math.Pow(data1.fPosY - data2.fPosY, 2.0)));//TODO : 사각형 두개가 겹치는지를 확인하여 return
			//return true;//그냥 Merge가 되는지 안되는지 출력하기위해 강제로 true return
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
