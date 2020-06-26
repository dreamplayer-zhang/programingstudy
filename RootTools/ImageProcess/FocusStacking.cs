using System;
using System.Collections.Generic;
using System.Diagnostics; //stopwatch. 추후 필요없으면 삭제.
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace RootTools.ImageProcess
{

	public class DataInfo
	{
		private int m_nScale = 2;
		public int p_nScale
		{
			get
			{
				return m_nScale;
			}
			set
			{
				m_nScale = value;
			}
		}
		private int m_nSrcHeight = 1080;
		public int p_nSrcHeight
		{
			get
			{
				return m_nSrcHeight;
			}
			set
			{
				m_nSrcHeight = value;
			}
		}
		private int m_nSrcWidth = 2048;
		public int p_nSrcWidth
		{
			get
			{
				return m_nSrcWidth;
			}
			set
			{
				m_nSrcWidth = value;
			}
		}
		private int m_nHeight;
		public int p_nHeight
		{
			get
			{
				return m_nHeight;
			}
			set
			{
				m_nHeight = value;
			}
		}
		private int m_nWidth;
		public int p_nWidth
		{
			get
			{
				return m_nWidth;
			}
			set
			{
				m_nWidth = value;
			}
		}
		private int m_nFocusCount;
		public int p_nFocusCount
		{
			get
			{
				return m_nFocusCount;
			}
			set
			{
				m_nFocusCount = value;
			}
		}
		private int m_nImageCount;
		public int p_nImageCount
		{
			get
			{
				return m_nImageCount;
			}
			set
			{
				m_nImageCount = value;
			}
		}
		public Stopwatch sw;
	}
	class FocusStacking
	{
		static int size = 10;
		public DataInfo DI;

		public void Run(int nIndex, int[] TotalArray)
		{
			//2048 1080
			DI.sw = new Stopwatch();
			Stopwatch total = new Stopwatch();

			Mat[] ListOfSrc;

			DI.sw.Start();
			total.Start();
			DI.p_nFocusCount = 33;
			DI.p_nHeight = DI.p_nSrcHeight / DI.p_nScale;
			DI.p_nWidth = DI.p_nSrcWidth / DI.p_nScale;

			ListOfSrc = new Mat[DI.p_nFocusCount];
			CreateSrcData(ref ListOfSrc, ref TotalArray, nIndex);
			Mat blur = new Mat();

			//ListofMat
			Mat[] ListOfLaplacianData = new Mat[DI.p_nFocusCount];
			Mat[] ListOfGradientData = new Mat[DI.p_nFocusCount];

			//이미지 전처리. 원본 - 가우시안 - 라플라스 - 가우시안.
			for (int i = 0; i < DI.p_nFocusCount; i++)
			{
				CvInvoke.GaussianBlur(ListOfSrc[i], blur, new System.Drawing.Size(0, 0), 1, 1, BorderType.Default);
				//Cv2.GaussianBlur(ListOfSrc[i], blur, new OpenCvSharp.Size(0, 0), 1, 1, BorderTypes.Default);

				ListOfLaplacianData[i] = new Mat();
				CreateGradientData(ref blur, ref ListOfGradientData[i]);

				CvInvoke.Laplacian(blur, ListOfLaplacianData[i], DepthType.Cv32F, 3, 1, 0, BorderType.Default);
				//Cv2.Laplacian(blur, ListOfLaplacianData[i], MatType.CV_32F, 3, 1, 0, BorderTypes.Default);

				ListOfLaplacianData[i].ConvertTo(ListOfLaplacianData[i], DepthType.Cv8U, 1);
				//ListOfLaplacianData[i].ConvertTo(ListOfLaplacianData[i], MatType.CV_8UC1);
				ListOfLaplacianData[i] = ListOfLaplacianData[i] + ListOfGradientData[i];

				//save
				//ListOfLaplacianData[i].SaveImage(DI.sDirP ath + "\\" + DI.filename[i] + "_2Laplacian_" + DateTime.Now.ToString("ss_fff") + ".tif");

				CvInvoke.GaussianBlur(ListOfLaplacianData[i], ListOfLaplacianData[i], new System.Drawing.Size(0, 0), 1, 1, BorderType.Default);
				//Cv2.GaussianBlur(ListOfLaplacianData[i], ListOfLaplacianData[i], new OpenCvSharp.Size(0, 0), 1, 1, BorderTypes.Default);

				CvInvoke.Resize(ListOfLaplacianData[i], ListOfLaplacianData[i], new System.Drawing.Size(0, 0), 0.5, 0.5);
				//Cv2.Resize(ListOfLaplacianData[i], ListOfLaplacianData[i], new OpenCvSharp.Size(0, 0), 0.5, 0.5);
			}

			Console.WriteLine("전처리과정 소요시간 : " + DI.sw.ElapsedMilliseconds + "ms");

			Mat res;
			Mat[] ListOfPartImg;
			//이미지 연산.
			res = ComparePixelData(ListOfSrc, ListOfLaplacianData, out ListOfPartImg);

			//이미지 후처리
			Mat ttmp = new Mat(res.Rows, res.Cols, DepthType.Cv8U, 1);

			CvInvoke.GaussianBlur(res, ttmp, new System.Drawing.Size(0, 0), 1, 1, BorderType.Default);
			//Cv2.GaussianBlur(res, ttmp, new OpenCvSharp.Size(0, 0), 2);

			CvInvoke.AddWeighted(res, 1.5, ttmp, -0.5, 0, res);
			//Cv2.AddWeighted(res, 1.5, ttmp, -0.5, 0, res);

			CvInvoke.GaussianBlur(res, ttmp, new System.Drawing.Size(0, 0), 1, 1, BorderType.Default);
			//Cv2.GaussianBlur(res, ttmp, new OpenCvSharp.Size(0, 0), 2);

			CvInvoke.AddWeighted(res, 1.5, ttmp, -0.5, 0, res);
			//Cv2.AddWeighted(res, 1.5, ttmp, -0.5, 0, res);

			for (int i = 0; i < DI.p_nFocusCount; i++)
			{
				//save
				//ListOfPartImg[i].SaveImage(DI.sDirPath + "\\" + DI.filename[i] + "_3PartImg_" + DateTime.Now.ToString("ss_fff") + ".tif");
			}

			//save
			//res.SaveImage(DI.sDirPath + "\\" + "_Result_" + DateTime.Now.ToString("ss_fff") + ".tif");
			Console.WriteLine("총 소요시간 : " + total.ElapsedMilliseconds + "ms");
			System.Windows.Forms.MessageBox.Show("Done");

		}

		//실질적 연산 함수.
		// 

		unsafe public void CreateSrcData(ref Mat[] SrcMat, ref int[] TotalArray, int nIndex)
		{
			for (int n = 0; n < DI.p_nFocusCount; n++)
			{
				SrcMat[n] = new Mat(DI.p_nSrcWidth, DI.p_nSrcHeight, DepthType.Cv8U, 1);
				byte* ptr = (byte*)SrcMat[n].DataPointer;
				for (int y = 0; y < DI.p_nSrcHeight * DI.p_nSrcWidth; y++)
					for (int x = 0; x < DI.p_nSrcHeight * DI.p_nSrcWidth; x++)
					{
						ptr[y * DI.p_nSrcWidth + x] = (byte)TotalArray[n * DI.p_nSrcHeight * DI.p_nImageCount * DI.p_nSrcWidth + y * DI.p_nImageCount * DI.p_nSrcWidth + nIndex * DI.p_nSrcWidth + x];
					}
			}
		}

		unsafe public Mat ComparePixelData(Mat[] _ListOfSrc, Mat[] _ListOfgData, out Mat[] ListOfPartImg)
		{
			DI.sw.Restart();

			//IntPtr TempPtr = _img.Data;
			byte*[] ListDataPtr = new byte*[DI.p_nFocusCount];
			byte*[] ListSrcPtr = new byte*[DI.p_nFocusCount];
			for (int n = 0; n < DI.p_nFocusCount; n++)
			{
				ListDataPtr[n] = (byte*)_ListOfgData[n].DataPointer;
				ListSrcPtr[n] = (byte*)_ListOfSrc[n].DataPointer;
			}

			Mat Res = new Mat(new System.Drawing.Size(DI.p_nSrcWidth, DI.p_nSrcHeight), DepthType.Cv8U, 1);
			byte* ResPtr = (byte*)Res.DataPointer;

			Mat[] WeightMap = new Mat[DI.p_nFocusCount];
			byte*[] WeightMapPtr = new byte*[DI.p_nFocusCount];

			ListOfPartImg = new Mat[DI.p_nFocusCount];
			byte*[] ListPartPtr = new byte*[_ListOfgData.Count()];
			for (int n = 0; n < DI.p_nFocusCount; n++)
			{
				WeightMap[n] = new Mat(new System.Drawing.Size(DI.p_nSrcWidth, DI.p_nSrcHeight), DepthType.Cv8U, 1);
				WeightMapPtr[n] = (byte*)WeightMap[n].DataPointer;

				ListOfPartImg[n] = new Mat(new System.Drawing.Size(DI.p_nSrcWidth, DI.p_nSrcHeight), DepthType.Cv8U, 1);
				ListPartPtr[n] = (byte*)ListOfPartImg[n].DataPointer;
			}

			int[][] WeightRes;
			WeightRes = CreateWeightData(ref ListDataPtr);

			//TotalWeight 계산.
			int[] totalWeight = new int[DI.p_nWidth * DI.p_nHeight];
			for (int i = 0; i < DI.p_nWidth * DI.p_nHeight; i++)
			{
				totalWeight[i] = 0;
				for (int n = 0; n < DI.p_nFocusCount; n++)
				{
					totalWeight[i] += WeightRes[n][i];
				}
			}

			int totalValue = 0;
			for (int y = 0; y < DI.p_nSrcHeight; y++)
				for (int x = 0; x < DI.p_nSrcWidth; x++)
				{
					totalValue = 0;
					for (int n = 0; n < DI.p_nFocusCount; n++)
					{
						totalValue += WeightRes[n][(y / 2) * DI.p_nWidth + (x / 2)] * ListSrcPtr[n][y * DI.p_nSrcWidth + x];
					}
					if (totalWeight[(y / 2) * DI.p_nWidth + (x / 2)] == 0)
					{
						totalValue = 0;
						for (int n = 0; n < DI.p_nFocusCount; n++)
							totalValue += ListSrcPtr[n][y * DI.p_nSrcWidth + x];
						ResPtr[y * DI.p_nSrcWidth + x] = (byte)(totalValue / DI.p_nFocusCount);
					}
					else
					{
						ResPtr[y * DI.p_nSrcWidth + x] = (byte)(totalValue / totalWeight[(y / 2) * DI.p_nWidth + (x / 2)]);
						for (int n = 0; n < DI.p_nFocusCount; n++)
							WeightMapPtr[n][y * DI.p_nSrcWidth + x] = (byte)(255 * WeightRes[n][(y / 2) * DI.p_nWidth + (x / 2)] / totalWeight[(y / 2) * DI.p_nWidth + (x / 2)]);
					}
				}
			for (int n = 0; n < DI.p_nFocusCount; n++)
			{
				//save
				//WeightMap[n].SaveImage(DI.sDirPath + "\\" + DI.filename[n] + "_4WeightImage_" + DateTime.Now.ToString("ss_fff") + ".tif");
			}

			return Res;
		}

		//GradientData를 생성. 추후 라플라스이미지와 +연산에 사용.
		unsafe void CreateGradientData(ref Mat Src, ref Mat Res)
		{
			int _w = Src.Width;
			int _h = Src.Height;
			Res = new Mat(new System.Drawing.Size(_w, _h), DepthType.Cv8U, 1);
			//Res = new Mat(new OpenCvSharp.Size(_w, _h), MatType.CV_8UC1);
			byte* SrcPtr = (byte*)Src.DataPointer;
			byte* ResPtr = (byte*)Res.DataPointer;

			int tempSum = 0;
			int temp = 0;
			for (int y = 0; y < _h; y++)
			{
				for (int x = 0; x < _w; x++)
				{
					ResPtr[y * _w + x] = 0;
					tempSum = 0;
					temp = 0;
					for (int _c = 0; _c < 9; _c++)
					{
						if (x - 1 + _c / 3 > 0 && x - 1 + _c / 3 < _w)
							if (y - 1 + _c % 3 > 0 && y - 1 + _c % 3 < _h)
							{
								temp += SrcPtr[(y - 1 + _c % 3) * _w + x - 1 + _c / 3];
								temp -= SrcPtr[y * _w + x];
								temp = Math.Abs(temp);
								tempSum += temp;
							}
					}
					ResPtr[y * _w + x] = (byte)temp;
				}
				// ListOfPartImg[i].SaveImage(sDirPath + "\\" + i + "_" + WhileCounter + "_3PartImg_" + DateTime.Now.ToString("ss_fff") + ".tif");
			}
		}

		//ref byte*[] ListDataPtr 는 Edge Score 이미지의 리스트.
		//
		//return int[][] 각각의 Weight ScoreMap
		unsafe int[][] CreateWeightData(ref byte*[] ListDataPtr)
		{
			int[][] res = new int[DI.p_nFocusCount][];
			for (int i = 0; i < DI.p_nFocusCount; i++)
				res[i] = new int[DI.p_nHeight * DI.p_nWidth];

			double[] nAvgMap = new double[DI.p_nHeight * DI.p_nWidth];
			for (int y = 0; y < DI.p_nHeight; y++)
			{
				for (int x = 0; x < DI.p_nWidth; x++)
				{
					nAvgMap[y * DI.p_nWidth + x] = 0;
					for (int n = 0; n < DI.p_nFocusCount; n++)
					{
						nAvgMap[y * DI.p_nWidth + x] += ListDataPtr[n][y * DI.p_nWidth + x];
					}
					nAvgMap[y * DI.p_nWidth + x] /= DI.p_nFocusCount;
				}
			}

			for (int y = 0; y < DI.p_nHeight; y++)
			{
				for (int x = 0; x < DI.p_nWidth; x++)
				{
					for (int n = 0; n < DI.p_nFocusCount; n++)
					{
						res[n][y * DI.p_nWidth + x] = CaculateWeight(ref ListDataPtr[n], ref nAvgMap, x, y);
					}
				}
			}
			return res;
		}

		//ref DataInfo DI - DataInformation 기본적인 데이터가 들어있는 Structure.
		//ref byte* DataPtr - 사용될 데이터가 있는 포인터. Score Map이다.
		//ref double[] _nAvgMap - 사용될 데이터에서 각 포인트들의 합/ 이미지 갯수 즉 평균 Score Map 이다.
		//int x 계산할 좌표.
		//int y 계산할 좌표.
		//return int  (x,y)에 해당하는 Weight값.
		unsafe int CaculateWeight(ref byte* DataPtr, ref double[] _nAvgMap, int x, int y)
		{
			double res = 0;
			double dLength = 0;
			for (int _i = -size; _i < size + 1; _i++)
			{
				if (x + _i >= 0 && x + _i < DI.p_nWidth)
					for (int _j = -size; _j < size + 1; _j++)
					{
						if (y + _j >= 0 && y + _j < DI.p_nHeight)
						{
							dLength = Math.Sqrt(_i * _i + _j * _j);

							if (dLength <= size && dLength > 0)
							{
								if (DataPtr[(y + _j) * DI.p_nWidth + (x + _i)] > _nAvgMap[(y + _j) * DI.p_nWidth + (x + _i)])
									res += (DataPtr[(y + _j) * DI.p_nWidth + (x + _i)] - _nAvgMap[(y + _j) * DI.p_nWidth + (x + _i)]) / dLength;
							}
						}
					}
			}
			return (int)res;
		}
	}
}
