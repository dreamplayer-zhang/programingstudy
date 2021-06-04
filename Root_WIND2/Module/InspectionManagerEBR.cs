using Root_WIND2.Module;
using RootTools;
using RootTools.Database;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_WIND2
{
	public class InspectionManagerEBR : WorkFactory
	{
		#region [Members]
		private readonly RecipeEBR recipe;
		private readonly SharedBufferInfo sharedBufferInfo;

		private WorkplaceBundle workplaceBundle;
		#endregion

		#region [Properties]
		public RecipeEBR Recipe
		{
			get => this.recipe;
		}

		public SharedBufferInfo SharedBufferInfo
		{
			get => this.sharedBufferInfo;
		}
		#endregion

		public InspectionManagerEBR(RecipeEBR _recipe, SharedBufferInfo _bufferInfo)
		{
			this.recipe = _recipe;
			this.sharedBufferInfo = _bufferInfo;
		}

		#region [Overrides]
		protected override void Initialize()
		{
			CreateWorkManager(WORK_TYPE.INSPECTION, 5);
			CreateWorkManager(WORK_TYPE.DEFECTPROCESS_ALL, 1, true);
		}

		protected override WorkplaceBundle CreateWorkplaceBundle()
		{
			// find notch
			int firstNotch = recipe.GetItem<EBRRecipe>().FirstNotch;
			int lastNotch = recipe.GetItem<EBRRecipe>().LastNotch;

			int bufferHeight = lastNotch - firstNotch;
			int bufferHeightPerDegree = bufferHeight / 360;

			int width = 5300;// recipe.GetItem<EBRParameter>().ROIWidth;
			int height = recipe.GetItem<EBRParameter>().ROIHeight;
			double stepDegree = 10; //recipe.GetItem<EBRParameter>().StepDegree;
			int workplaceCnt = (int)(360 / stepDegree);

			workplaceBundle = new WorkplaceBundle();
			workplaceBundle.Add(new Workplace(0, 0, 0, 0, 0, 0, workplaceBundle.Count));
			
			for (int i = 1; i < workplaceCnt; i++)
			{
				int posY = (int)((bufferHeightPerDegree * stepDegree * i) - (height / 2));
				Workplace workplace = new Workplace(0, 0, 0, posY, width, height, workplaceBundle.Count);
				workplaceBundle.Add(workplace);
			}

			workplaceBundle.SetSharedBuffer(this.sharedBufferInfo);
			return workplaceBundle;
		}

		protected override WorkBundle CreateWorkBundle()
		{ 
			List<ParameterBase> paramList = recipe.ParameterItemList;
			WorkBundle workBundle = new WorkBundle();
			EBR ebr = new EBR();
			ProcessMeasurement processMeasurement = new ProcessMeasurement();
			
			// Parameters
			foreach (ParameterBase param in paramList)
				ebr.SetParameter(param);
			ebr.SetGrabMode(((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[recipe.GetItem<EBRRecipe>().GrabModeIndex]);
			
			workBundle.Add(ebr);
			workBundle.Add(processMeasurement);
			workBundle.SetRecipe(recipe);
			return workBundle;
		}

		private void TestCode()
		{
			// Parameters
			//char* sNotchImageFileName = "D:/WISVision/Image/NotchImage.bmp";    // Notch Golden Image 파일 경로
			//LPBYTE* ppImage = m_Image.GetMemPointer();          // Image Memory Pointer
			int nImageWidth = 7000;                 // Image 가로 길이, 각 Row 마다 0 ~ nImageWidth 범위 내에서 Edge X 좌표 계산함
			int nEdgeStartY = 200;                  // 검사 시작 Y 좌표
			int nEdgeEndY = 299990;                 // 검사 끝 Y 좌표
			int nSaturationThreshold = 255;         // Saturation을 판별하는 Threshold, Edge X 좌표를 Saturation 영역 좌측 끝 값으로 정의함

			// Functions
			//IplImage* pNotchImage = cvLoadImage(sNotchImageFileName, 0);	// 골든 이미지 어디?
			//float* pNotchEdges = new float[pNotchImage->height];
			System.Drawing.Image pNotchImage = System.Drawing.Image.FromFile(@"D:\test.bmp"); // 골든 이미지 어디? Image -> byte[]
			float[] pNotchEdges = new float[pNotchImage.Height];
			FindEdges(pNotchImage, nSaturationThreshold, ref pNotchEdges);

			int nNotchCenter = FindNotchCenter(pNotchEdges, pNotchImage.Height);

			int size = nImageWidth * (nEdgeEndY - nEdgeStartY);
			byte[] managedArray = new byte[size];
			System.Runtime.InteropServices.Marshal.Copy(this.sharedBufferInfo.PtrR_GRAY, managedArray, 0, size);
			CPoint[] ptNotches = FindNotches(managedArray, nImageWidth, nEdgeStartY, nEdgeEndY, nSaturationThreshold, pNotchEdges, pNotchImage.Height, nNotchCenter);
		}

		private void FindEdges(System.Drawing.Image image, int nSaturationThreshold, ref float[] pEdges)
		{
			// 각 Row에서 Edge X 좌표를 찾아 배열에 저장
			// Edge X 좌표는 Saturation 영역의 좌측 끝 값으로 정의
			// GV가 nSaturationThreshold 이상인 픽셀이 연속으로 가장 길게 나타나는 영역을 Saturation 영역으로 정의

			System.Drawing.ImageConverter _imageConverter = new System.Drawing.ImageConverter();
			byte[] pImage = (byte[])_imageConverter.ConvertTo(image, typeof(byte[]));

			bool bStart;
			int nEdge, nCount, nMaxCount, nIndex;

			for (int i = 0; i < image.Height; i++)
			{
				bStart = false;
				nEdge = 0;
				nCount = 0;
				nMaxCount = 0;
				pEdges[i] = 0;

				for (int j = 0; j < image.Width; j++)
				{
					nIndex = (i * image.Width) + j;
					if (bStart == false)
					{
						if (pImage[nIndex] >= nSaturationThreshold)
						{
							bStart = true;
							nEdge = j;
							nCount++;
						}
						else
						{
							if (pImage[nIndex] >= nSaturationThreshold)
							{
								nCount++;
								if (j == (image.Width - 1))
								{
									if (nCount > nMaxCount)
										pEdges[i] = nEdge;
								}
							}
						}
					}
					else
					{
						bStart = false;
						if (nCount > nMaxCount)
						{
							pEdges[i] = nEdge;
							nMaxCount = nCount;
						}
						nCount = 0;
					}
				}
			}
		}

		private int FindNotchCenter(float[] pEdges, int nNotchHeight)
		{
			// Edge X 좌표가 최소인 위치들의 중점으로 Notch Center 정의

			int nEdge;
			int nMinEdge = (int)pEdges[0];

			for (int i = 1; i < nNotchHeight; i++)
			{
				nEdge = (int)pEdges[i];

				if (nEdge < nMinEdge)
				{
					nMinEdge = nEdge;
				}
			}

			bool bStart = false;
			int nMinStart = 0;
			int nCenterStart = 0;
			int nCount = 0;
			int nMaxCount = 0;

			for (int i = 0; i < nNotchHeight; i++)
			{
				if (bStart == false)
				{
					if (pEdges[i] == nMinEdge)
					{
						bStart = true;
						nMinStart = i;
						nCount++;
					}
				}
				else
				{
					if (pEdges[i] == nMinEdge)
					{
						nCount++;

						if (i == (nNotchHeight - 1))
						{
							if (nCount > nMaxCount)
							{
								nCenterStart = nMinStart;
								nMaxCount = nCount;
							}
						}
					}
					else
					{
						bStart = false;

						if (nCount > nMaxCount)
						{
							nCenterStart = nMinStart;
							nMaxCount = nCount;
						}

						nCount = 0;
					}
				}
			}
			return nCenterStart + nMaxCount / 2;
		}

		CPoint[] FindNotches(byte[] pImage, int nImageWidth, int nEdgeStartY, int nEdgeEndY, int nSaturationThreshold, float[] pNotchEdges, int nNotchHeight, int nNotchCenter)
		{
			CPoint[] ptNotches = new CPoint[2];
			int nImageHeight = nEdgeEndY - nEdgeStartY;
			float[] pEdges = new float[nImageHeight];
			//IntPtr pEdges;
			FindEdges(pImage, nImageWidth, nEdgeStartY, nEdgeEndY, nSaturationThreshold, ref pEdges);

			// Notch Golden Image의 Edge X 좌표 배열과 현재 Image의 Edge X 좌표 배열을 1D Template Matching
			//Emgu.CV.Mat matEdges = new Emgu.CV.Mat(nImageHeight, nImageWidth, Emgu.CV.CvEnum.DepthType.Cv32F, 1);//(new System.Drawing.Size(1, nImageHeight), Emgu.CV.CvEnum.DepthType.Cv32F, 1);
			//System.Runtime.InteropServices.Marshal.Copy(pEdges, 0, matEdges, nImageHeight * nImageWidth);

			//Emgu.CV.Mat matNotchEdges = new Emgu.CV.Mat(new System.Drawing.Size(1, nNotchHeight), Emgu.CV.CvEnum.DepthType.Cv32F, 1, pNotchEdges);
			//Emgu.CV.Mat matResult;

			//cv::matchTemplate(matEdges, matNotchEdges, matResult, CV_TM_CCOEFF_NORMED);

			//// 1st Peak 위치 계산
			//double minVal; double maxVal; cv::Point minLoc; cv::Point maxLoc;
			//minMaxLoc(matResult, &minVal, &maxVal, &minLoc, &maxLoc, Mat());

			//ptNotches[0].X = pEdges[maxLoc.y + nNotchCenter];
			//ptNotches[0].Y = nEdgeStartY + maxLoc.y + nNotchCenter;

			//// 1st Peak 위치 주변 0 값 대입
			//int nAroundMaxStart = maxLoc.y - nNotchHeight < 0 ? 0 : maxLoc.y - nNotchHeight;
			//int nAroundMaxEnd = maxLoc.y + nNotchHeight >= matResult.rows ? matResult.rows - 1 : maxLoc.y + nNotchHeight;

			//for (int i = nAroundMaxStart; i <= nAroundMaxEnd; i++)
			//{
			//	matResult.at<float>(i, 0) = 0;
			//}

			//// 2nd Peak 위치 계산
			//minMaxLoc(matResult, &minVal, &maxVal, &minLoc, &maxLoc, Mat());

			//ptNotches[1].x = pEdges[maxLoc.y + nNotchCenter];
			//ptNotches[1].y = nEdgeStartY + maxLoc.y + nNotchCenter;

			// 메모리 해제
			//matEdges.release();
			//matNotchEdges.release();
			//matResult.release();
			//delete[] pEdges;

			return ptNotches;
		}

		void FindEdges(byte[] ppImage, int nImageWidth, int nEdgeStartY, int nEdgeEndY, int nSaturationThreshold, ref float[] pEdges)
		{
			// 각 Row에서 Edge X 좌표를 찾아 배열에 저장
			// Edge X 좌표는 Saturation 영역의 좌측 끝 값으로 정의
			// GV가 nSaturationThreshold 이상인 픽셀이 연속으로 가장 길게 나타나는 영역을 Saturation 영역으로 정의

			bool bStart;
			int nEdge, nCount, nMaxCount, nIndex;

			for (int i = nEdgeStartY; i < nEdgeEndY; i++)
			{
				bStart = false;
				nEdge = 0;
				nCount = 0;
				nMaxCount = 0;
				pEdges[i - nEdgeStartY] = 0;

				for (int j = 0; j < nImageWidth; j++)
				{
					nIndex = (i * nImageWidth) + j;
					if (bStart == false)
					{
						if (ppImage[nIndex] >= nSaturationThreshold)
						{
							bStart = true;
							nEdge = j;
							nCount++;
						}
					}
					else
					{
						if (ppImage[nIndex] >= nSaturationThreshold)
						{
							nCount++;

							if (j == (nImageWidth - 1))
							{
								if (nCount > nMaxCount)
								{
									pEdges[i] = nEdge;
								}
							}
						}
						else
						{
							bStart = false;

							if (nCount > nMaxCount)
							{
								pEdges[i - nEdgeStartY] = nEdge;
								nMaxCount = nCount;
							}

							nCount = 0;
						}
					}
				}
			}
		}

		public int GetWorkplaceCount()
		{
			if (workplaceBundle == null)
				return 1;
			return workplaceBundle.Count();
		}

		protected override bool Ready(WorkplaceBundle workplaces, WorkBundle works)
		{
			return true;
		}
		#endregion

		public new void Start()
		{
			if (this.Recipe == null)
				return;

			DateTime inspectionStart = DateTime.Now;
			DateTime inspectionEnd = DateTime.Now;
			string lotId = "Lotid";
			string partId = "Partid";
			string setupId = "SetupID";
			string cstId = "CSTid";
			string waferId = "WaferID";
			//string sRecipe = "RecipeID";
			string recipeName = recipe.Name;

			DatabaseManager.Instance.SetLotinfo(inspectionStart, inspectionEnd, lotId, partId, setupId, cstId, waferId, recipeName);

			base.Start();
		}
    }
}
