using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace RootTools
{
	/// <summary>
	/// DefectData Class의 Wrapper Class. Defect Cluster(Merge)관련된 기능을 가지고 있습니다. 
	/// 기본적으로 Defect은 이 구조를 가지고, Merge가 진행된 경우에는 ClusterItems에 이 Defect을 구성할때 사용된 Defect의 정보가 누적됩니다. 
	/// 구조적으로는 ClusterItems를 가진 Defect을 ClusterItems에 넣는것도 가능하지만 권장되지 않습니다. 
	/// </summary>
	public class DefectDataWrapper : DefectData
	{
		#region IsCluster 프로퍼티

		/// <summary>
		/// 해당 defect 정보가 cluster 정보인지 확인
		/// </summary>
		public bool IsCluster
		{
			get
			{
				if (ClusterItems == null)
					return false;
				else if (ClusterItems.Count >= 1)
					return true;
				else
					return false;
			}
		}
		#endregion

		#region ClusterItems 프로퍼티
		/// <summary>
		/// Cluster defect의 구성요소 목록
		/// </summary>
		public List<DefectDataWrapper> ClusterItems { get; private set; }

		#endregion

		#region DrawStartPoint 프로퍼티

		public Point DrawStartPoint
		{
			get
			{
				if (this.IsCluster)
				{
					//Cluster가 있는 경우 Cluster 내부의 모든 정보를 종합하여 반환
					int left = ClusterItems.Min(x => x.DrawStartPoint.X);
					int top = ClusterItems.Min(x => x.DrawStartPoint.Y);
					return new Point(left, top);
				}
				else
				{
					//Cluster가 없는 경우 단순반환
					return new Point((int)(fPosX - nWidth / 2.0), (int)(fPosY - nHeight / 2.0));
				}
			}
		}
		#endregion

		#region DrawEndPoint 프로퍼티

		public Point DrawEndPoint
		{
			get
			{
				if (this.IsCluster)
				{
					//Cluster가 있는 경우 Cluster 내부의 모든 정보를 종합하여 반환
					int right = ClusterItems.Max(x => x.DrawEndPoint.X);
					int bottom = ClusterItems.Max(x => x.DrawEndPoint.Y);
					return new Point(right, bottom);
				}
				else
				{
					//Cluster가 없는 경우 단순반환
					return new Point((int)(fPosX + nWidth / 2.0), (int)(fPosY + nHeight / 2.0));
				}
			}
		}
		#endregion

		#region DrawWidth 프로퍼티

		public int DrawWidth
		{
			get
			{
				if (this.IsCluster)
				{
					//Cluster가 있는 경우 Cluster 내부의 모든 정보를 종합하여 반환
					int left = ClusterItems.Min(x => x.DrawStartPoint.X);
					int right = ClusterItems.Max(x => x.DrawEndPoint.X);
					return right - left;
				}
				else
				{
					//Cluster가 없는 경우 단순반환
					return nWidth;
				}
			}
		}
		#endregion

		#region DrawHeight 프로퍼티

		public int DrawHeight
		{
			get
			{
				if (this.IsCluster)
				{
					//Cluster가 있는 경우 Cluster 내부의 모든 정보를 종합하여 반환
					int top = ClusterItems.Min(x => x.DrawStartPoint.Y);
					int bottom = ClusterItems.Max(x => x.DrawEndPoint.Y);
					return bottom - top;
				}
				else
				{
					//Cluster가 없는 경우 단순반환
					return nHeight;
				}
			}
		}
		#endregion

		bool bMergeUsed;

		public DefectDataWrapper(DefectData item)
		{
			fPosX = item.fPosX;
			fPosY = item.fPosY;
			fAreaSize = item.fAreaSize;
			nClassifyCode = item.nClassifyCode;
			nFOV = item.nFOV;
			nWidth = item.nWidth;
			nHeight = item.nHeight;
			nIdx = item.nIdx;
			nLength = item.nLength;
			bMergeUsed = false;
			nGV = item.nGV;

			ClusterItems = new List<DefectDataWrapper>();
		}

		#region 클러스터 제어 메소드

		/// <summary>
		/// ClusterItems에 Data를 추가
		/// </summary>
		/// <param name="data"></param>
		public void AddCluster(DefectDataWrapper data)
		{
			if (ClusterItems == null)
				ClusterItems = new List<DefectDataWrapper>();

			ClusterItems.Add(data);
			MergeClusterInfo();
		}
		/// <summary>
		/// ClusterItems에 Data Array를 추가
		/// </summary>
		/// <param name="data"></param>
		public void AddRangeCluster(DefectDataWrapper[] data)
		{
			if (ClusterItems == null)
				ClusterItems = new List<DefectDataWrapper>();

			ClusterItems.AddRange(data);
			MergeClusterInfo();
		}
		/// <summary>
		/// ClusterItems에서 지정된 순서의 데이터를 삭제. zero-based
		/// </summary>
		/// <param name="idx"></param>
		public void RemoveCluster(int idx)
		{
			if (ClusterItems == null)
				return;
			ClusterItems.RemoveAt(idx);
			MergeClusterInfo();
		}
		/// <summary>
		/// ClusterItems에서 지정된 데이터를 삭제
		/// </summary>
		/// <param name="data"></param>
		public void RemoveCluster(DefectDataWrapper data)
		{
			if (ClusterItems == null)
				return;
			ClusterItems.Remove(data);
			MergeClusterInfo();
		}
		/// <summary>
		/// Cluster defect 내부의 정보를 종합하여 재연산. ClusterItems가 비어있으면 동작하지 않는다
		/// </summary>
		public void MergeClusterInfo()
		{
			if (ClusterItems == null || ClusterItems.Count == 0)
				return;
			/*
			 fPosX = item.fPosX;
			fPosY = item.fPosY;
			fSize = item.fSize;
			nClassifyCode = item.nClassifyCode;
			nFOV = item.nFOV;
			nWidth = item.nWidth;
			nHeight = item.nHeight;
			nIdx = item.nIdx;
			nInspMode = item.nInspMode;
			nLength = item.nLength;
			 */
			this.fPosX = ClusterItems.Average(x => x.fPosX);
			this.fPosY = ClusterItems.Average(x => x.fPosY);
			this.fAreaSize = ClusterItems.Sum(x => x.fAreaSize);
			this.nClassifyCode = ClusterItems.First().nClassifyCode;
			this.nFOV = ClusterItems.First().nFOV;
			this.nWidth = ClusterItems.Sum(x => x.nWidth);
			this.nHeight = ClusterItems.Sum(x => x.nHeight);
			this.nIdx = ClusterItems.First().nIdx;
			//this.nLength = ClusterItems.Sum(x => x.nLength);
		}
		/// <summary>
		/// ClusterItems안의 모든 항목을 지정된 조건으로 다시 조정한다
		/// </summary>
		/// <param name="data">조정할 Defect</param>
		/// <returns></returns>
		public static List<DefectDataWrapper> RearrangeCluster(DefectDataWrapper data, int mergeDistance)
		{
			List<DefectDataWrapper> result = new List<DefectDataWrapper>();

			if (data.ClusterItems == null || data.ClusterItems.Count <= 1)
			{
				//Cluster가 비이있거나 하나밖에 없는 경우 자기 자신을 list로 반환
				result.Add(data);
				return result;
			}

			return CreateClusterList(data.ClusterItems.ToArray(), mergeDistance);
		}
		/// <summary>
		/// DefectDataWrapper Array를 distance argument 에 따라 Cluster화 한다
		/// </summary>
		/// <param name="data">Defect List</param>
		/// <param name="mergeDistance"></param>
		/// <returns></returns>
		private static List<DefectDataWrapper> CreateClusterList(DefectDataWrapper[] data, int mergeDistance)
		{
			List<DefectDataWrapper> result = new List<DefectDataWrapper>();

			if(data.Count() <= 1)
			{
				return data.ToList();
			}

			for (int i = 0; i < data.Length - 1; i++)
			{
				bool merged = false;
				if (data[i].bMergeUsed)
					continue;

				DefectDataWrapper cluster = new DefectDataWrapper(data[i]);
				for (int j = i + 1; j < data.Length; j++)
				{
					if (CheckMerge(data[i], data[j], mergeDistance))
					{
						merged = true;
						data[j].bMergeUsed = true;
						cluster.AddCluster(data[j]);
					}
				}
				if (merged)
				{
					data[i].bMergeUsed = true;
					cluster.AddCluster(data[i]);//다 끝나고 merge가 한번이라도 되었다면 자기 자신도 넣고 끝낸다
													  //merge가 안 된 경우에는 clusteritems에 defect이 하나도 없게 됨

					cluster.MergeClusterInfo();//Cluster내의 정보를 병합해서 본체에 덮어씌운다
					result.Add(cluster);
				}
				else
				{
					//merge에 진입하지 않은 경우, 자기 자신을 결과에 넣는다
					result.Add(data[i]);
				}
			}
			return result;
		}
		#endregion

		/// <summary>
		/// List의 모든 Cluster를 풀어낸 후 주어진 조건으로 병합한다
		/// </summary>
		/// <param name="data">병합을 진행할 데이터 목록</param>
		/// <param name="mergeDistance">병합 거리(Pixel)</param>
		/// <returns></returns>
		public static List<DefectDataWrapper> MergeDefect(DefectDataWrapper[] data, int mergeDistance)
		{
			List<DefectDataWrapper> tempList = new List<DefectDataWrapper>();

			for (int i = 0; i < data.Length; i++)
			{
				//받아온 모든 List의 ClusterItems 및 요소를 치환한다
				if (data[i].IsCluster)
				{
					tempList.AddRange(data[i].ClusterItems);
				}
				else
				{
					tempList.Add(data[i]);
				}
			}

			return CreateClusterList(tempList.ToArray(), mergeDistance);
		}
		/// <summary>
		/// 두개의 데이터의 최외각 사각형이 서로 겹치는지에 대해 판별하여 return
		/// </summary>
		/// <param name="data1">비교할 데이터</param>
		/// <param name="data2">비교할 데이터</param>
		/// <param name="distance">병합 거리(Pixel)</param>
		/// <returns></returns>
		private static bool CheckMerge(DefectDataWrapper data1, DefectDataWrapper data2, int distance)
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
			if (result.IsEmpty)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
	}
}
