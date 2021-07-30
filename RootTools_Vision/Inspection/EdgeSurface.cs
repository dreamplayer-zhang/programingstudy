using Emgu.CV;
using Emgu.CV.Cvb;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using RootTools;
using RootTools.Database;
using RootTools.Inspects;
using RootTools_CLR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
	public class EdgeSurface : WorkBase
	{
		public override WORK_TYPE Type => WORK_TYPE.INSPECTION;

		public enum EdgeMapPositionX
		{
			Top = 0,
			Side = 1,
			Btm = 2
		}

		public EdgeSurface() : base()
		{
			m_sName = this.GetType().Name;
		}

		public override WorkBase Clone()
		{
			return (WorkBase)this.MemberwiseClone();
		}

		protected override bool Preparation()
		{
			return true;
		}

		protected override bool Execution()
		{
			DoInspection();
			return true;
		}

		public void DoInspection()
		{
			if (this.currentWorkplace.Index == 0)
				return;

			EdgeSurfaceParameter param = recipe.GetItem<EdgeSurfaceParameter>();
			EdgeSurfaceParameterBase paramTop = param.EdgeParamBaseTop;
			EdgeSurfaceParameterBase paramBottom = param.EdgeParamBaseBtm;
			EdgeSurfaceParameterBase paramSide = param.EdgeParamBaseSide;

			WorkEventManager.OnInspectionStart(this.currentWorkplace, new InspectionStartArgs());

			if (paramTop.ChR)
				DoColorInspection(paramTop, EdgeDefectCode.Top, 0);
			if (paramTop.ChG)
				DoColorInspection(paramTop, EdgeDefectCode.Top, 1);
			if (paramTop.ChB)
				DoColorInspection(paramTop, EdgeDefectCode.Top, 2);
			WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...

			if (paramBottom.ChR)
				DoColorInspection(paramBottom, EdgeDefectCode.Btm, 3);
			if (paramBottom.ChG)
				DoColorInspection(paramBottom, EdgeDefectCode.Btm, 4);
			if (paramBottom.ChB)
				DoColorInspection(paramBottom, EdgeDefectCode.Btm, 5);
			WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...

			if (paramSide.ChR)
				DoColorInspection(paramSide, EdgeDefectCode.Side, 6);
			if (paramSide.ChG)
				DoColorInspection(paramSide, EdgeDefectCode.Side, 7);
			if (paramSide.ChB)
				DoColorInspection(paramSide, EdgeDefectCode.Side, 8);
			WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...
		}

		public enum EdgeDefectCode
		{
			Top = 20000,
			Side = 20100,
			Btm = 20200,
		}
				
		private void DoColorInspection(EdgeSurfaceParameterBase param, EdgeDefectCode defectCode, int channelIndex)
		{
			if (param.UseProbabilityMethod == true)
            {
				#region [Parameter]

				double waferRadius = 150000.0;          // um
				double resolutionX = this.currentWorkplace.CameraInfo.TargetResX;       // um
				int alignTolerance = 3;			// 1-profile similar GV search range

				OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();

				int roiStartY = param.StartPosition;
				int roiEndY = param.EndPosition;

				if (roiEndY > originRecipe.OriginHeight)
                {
					roiEndY = originRecipe.OriginHeight;
				}

				int roiWidth = originRecipe.OriginWidth;
				int roiHeight = roiEndY - roiStartY;

				#endregion

				#region [Measure Edge X]

				int[] measuredEdgeXList = new int[roiHeight];

				Parallel.For(0, roiHeight, i =>
				{
					byte[] bufferRow = new byte[roiWidth];

					Marshal.Copy(this.currentWorkplace.SharedBufferInfo.PtrList[channelIndex] + this.currentWorkplace.SharedBufferInfo.Width * (roiStartY + i), bufferRow, 0, roiWidth);

					byte currentGV = 0;
					byte maxGV = 0;

					for (int j = 0; j < roiWidth; j++)
					{
						currentGV = bufferRow[j];

						if (currentGV > maxGV)
						{
							maxGV = currentGV;
						}
					}

					byte thresholdGV = (byte)((int)maxGV * param.EdgeSearchLevel / 100);

					int measuredEdgeX = 0;

					for (int j = 0; j < roiWidth; j++)
					{
						currentGV = bufferRow[j];

						if (currentGV > thresholdGV)
						{
							measuredEdgeX = j;
							break;
						}
					}

					measuredEdgeXList[i] = measuredEdgeX;
				}
				);

				#endregion

				#region [Measure Square Distance]

				double averageEdgeX = 0;

				for (int i = 0; i < roiHeight; i++)
				{
					averageEdgeX += (double)measuredEdgeXList[i];
				}

				averageEdgeX = averageEdgeX / (double)roiHeight;

				double[] measuredSquareDistanceList = new double[roiHeight];				

				Parallel.For(0, roiHeight, i =>
				{
					double measuredDistance = waferRadius + (averageEdgeX - measuredEdgeXList[i]) * resolutionX;

					measuredSquareDistanceList[i] = measuredDistance * measuredDistance;
				}
				);

				#endregion

				#region [Curve Fitting]

				double period = (double)roiHeight;
				double frequency = 2.0 * Math.PI / period;

				double constant = 0;
				double coefficiantCosine = 0;
				double coefficiantSine = 0;

				for (int i = 0; i < roiHeight; i++)
				{
					constant += measuredSquareDistanceList[i];
					coefficiantCosine += measuredSquareDistanceList[i] * Math.Cos(frequency * i);
					coefficiantSine += measuredSquareDistanceList[i] * Math.Sin(frequency * i);
				}

				constant = constant / period;
				coefficiantCosine = coefficiantCosine * 2 / period;
				coefficiantSine = coefficiantSine * 2 / period;

				double[] calculatedSquareDistanceList = new double[roiHeight];
				int[] calculatedEdgeXList = new int[roiHeight];

				Parallel.For(0, roiHeight, i =>
				{
					calculatedSquareDistanceList[i] = constant + coefficiantCosine * Math.Cos(frequency * i) + coefficiantSine * Math.Sin(frequency * i);
					calculatedEdgeXList[i] = (int)(averageEdgeX - (Math.Sqrt(calculatedSquareDistanceList[i]) - waferRadius) / resolutionX + 0.5);
				}
				);

				#endregion

				#region [Align Image]

				int minEdgeX = roiWidth;

				for (int i = 0; i < roiHeight; i++)
				{
					if (calculatedEdgeXList[i] < minEdgeX)
					{
						minEdgeX = calculatedEdgeXList[i];
					}
				}

				if (defectCode == EdgeDefectCode.Top)
                {
					Parallel.For(0, roiHeight, i =>
					{
						byte[] bufferRow = new byte[2 * roiWidth];

						Array.Clear(bufferRow, 0, bufferRow.Length);
						Marshal.Copy(this.currentWorkplace.SharedBufferInfo.PtrList[0] + this.currentWorkplace.SharedBufferInfo.Width * (roiStartY + i), bufferRow, roiWidth - calculatedEdgeXList[i] + minEdgeX, roiWidth);
						Marshal.Copy(bufferRow, roiWidth, this.currentWorkplace.SharedBufferInfo.PtrList[0] + this.currentWorkplace.SharedBufferInfo.Width * (roiStartY + i), roiWidth);

						Array.Clear(bufferRow, 0, bufferRow.Length);
						Marshal.Copy(this.currentWorkplace.SharedBufferInfo.PtrList[1] + this.currentWorkplace.SharedBufferInfo.Width * (roiStartY + i), bufferRow, roiWidth - calculatedEdgeXList[i] + minEdgeX, roiWidth);
						Marshal.Copy(bufferRow, roiWidth, this.currentWorkplace.SharedBufferInfo.PtrList[1] + this.currentWorkplace.SharedBufferInfo.Width * (roiStartY + i), roiWidth);

						Array.Clear(bufferRow, 0, bufferRow.Length);
						Marshal.Copy(this.currentWorkplace.SharedBufferInfo.PtrList[2] + this.currentWorkplace.SharedBufferInfo.Width * (roiStartY + i), bufferRow, roiWidth - calculatedEdgeXList[i] + minEdgeX, roiWidth);
						Marshal.Copy(bufferRow, roiWidth, this.currentWorkplace.SharedBufferInfo.PtrList[2] + this.currentWorkplace.SharedBufferInfo.Width * (roiStartY + i), roiWidth);
					}
					);
				}
				else if (defectCode == EdgeDefectCode.Btm)
				{
					Parallel.For(0, roiHeight, i =>
					{
						byte[] bufferRow = new byte[2 * roiWidth];

						Array.Clear(bufferRow, 0, bufferRow.Length);
						Marshal.Copy(this.currentWorkplace.SharedBufferInfo.PtrList[3] + this.currentWorkplace.SharedBufferInfo.Width * (roiStartY + i), bufferRow, roiWidth - calculatedEdgeXList[i] + minEdgeX, roiWidth);
						Marshal.Copy(bufferRow, roiWidth, this.currentWorkplace.SharedBufferInfo.PtrList[3] + this.currentWorkplace.SharedBufferInfo.Width * (roiStartY + i), roiWidth);

						Array.Clear(bufferRow, 0, bufferRow.Length);
						Marshal.Copy(this.currentWorkplace.SharedBufferInfo.PtrList[4] + this.currentWorkplace.SharedBufferInfo.Width * (roiStartY + i), bufferRow, roiWidth - calculatedEdgeXList[i] + minEdgeX, roiWidth);
						Marshal.Copy(bufferRow, roiWidth, this.currentWorkplace.SharedBufferInfo.PtrList[4] + this.currentWorkplace.SharedBufferInfo.Width * (roiStartY + i), roiWidth);

						Array.Clear(bufferRow, 0, bufferRow.Length);
						Marshal.Copy(this.currentWorkplace.SharedBufferInfo.PtrList[5] + this.currentWorkplace.SharedBufferInfo.Width * (roiStartY + i), bufferRow, roiWidth - calculatedEdgeXList[i] + minEdgeX, roiWidth);
						Marshal.Copy(bufferRow, roiWidth, this.currentWorkplace.SharedBufferInfo.PtrList[5] + this.currentWorkplace.SharedBufferInfo.Width * (roiStartY + i), roiWidth);
					}
					);
				}
				else if (defectCode == EdgeDefectCode.Side)
				{
					Parallel.For(0, roiHeight, i =>
					{
						byte[] bufferRow = new byte[2 * roiWidth];

						Array.Clear(bufferRow, 0, bufferRow.Length);
						Marshal.Copy(this.currentWorkplace.SharedBufferInfo.PtrList[6] + this.currentWorkplace.SharedBufferInfo.Width * (roiStartY + i), bufferRow, roiWidth - calculatedEdgeXList[i] + minEdgeX, roiWidth);
						Marshal.Copy(bufferRow, roiWidth, this.currentWorkplace.SharedBufferInfo.PtrList[6] + this.currentWorkplace.SharedBufferInfo.Width * (roiStartY + i), roiWidth);

						Array.Clear(bufferRow, 0, bufferRow.Length);
						Marshal.Copy(this.currentWorkplace.SharedBufferInfo.PtrList[7] + this.currentWorkplace.SharedBufferInfo.Width * (roiStartY + i), bufferRow, roiWidth - calculatedEdgeXList[i] + minEdgeX, roiWidth);
						Marshal.Copy(bufferRow, roiWidth, this.currentWorkplace.SharedBufferInfo.PtrList[7] + this.currentWorkplace.SharedBufferInfo.Width * (roiStartY + i), roiWidth);

						Array.Clear(bufferRow, 0, bufferRow.Length);
						Marshal.Copy(this.currentWorkplace.SharedBufferInfo.PtrList[8] + this.currentWorkplace.SharedBufferInfo.Width * (roiStartY + i), bufferRow, roiWidth - calculatedEdgeXList[i] + minEdgeX, roiWidth);
						Marshal.Copy(bufferRow, roiWidth, this.currentWorkplace.SharedBufferInfo.PtrList[8] + this.currentWorkplace.SharedBufferInfo.Width * (roiStartY + i), roiWidth);
					}
					);
				}

				#endregion

				#region [Create Profile]

				int inspectionWidth = roiWidth - minEdgeX;
				int[][] histogram = new int[inspectionWidth][];

				for (int i = 0; i < inspectionWidth; i++)
				{
					histogram[i] = new int[256];
				}

				for (int i = 0; i < roiHeight; i++)
				{
					int bufferWidth = roiWidth - calculatedEdgeXList[i];

					byte[] bufferRow = new byte[bufferWidth];

					Marshal.Copy(this.currentWorkplace.SharedBufferInfo.PtrList[channelIndex] + this.currentWorkplace.SharedBufferInfo.Width * (roiStartY + i) + minEdgeX, bufferRow, 0, bufferWidth);

					for (int j = 0; j < bufferWidth; j++)
					{
						histogram[j][bufferRow[j]]++;
					}
				}

				byte[] profile = new byte[inspectionWidth];

				for (int i = 0; i < inspectionWidth; i++)
				{
					int maxBin = 0;
					int maxIndex = 0;

					for (int j = 0; j < 256; j++)
					{
						int currentBin = histogram[i][j];

						if (currentBin > maxBin)
						{
							maxBin = currentBin;
							maxIndex = j;
						}
					}

					profile[i] = (byte)maxIndex;
				}

				#endregion

				#region [Create Difference Image]

				int notchOffset = (int)(((param.EndPosition - param.StartPosition) / 360.0) * param.NotchOffsetDegree);
				int inspectoinStartY = roiStartY + notchOffset;
				int inspectionEndY = roiEndY - notchOffset;
				int inspectionHeight = inspectionEndY - inspectoinStartY;

				byte[] differenceImage = new byte[inspectionWidth * inspectionHeight];
				Array.Clear(differenceImage, 0, differenceImage.Length);

				Parallel.For(0, inspectionHeight, i =>
				{
					int bufferWidth = roiWidth - calculatedEdgeXList[i + notchOffset];

					byte[] bufferRow = new byte[bufferWidth];

					Marshal.Copy(this.currentWorkplace.SharedBufferInfo.PtrList[channelIndex] + this.currentWorkplace.SharedBufferInfo.Width * (inspectoinStartY + i) + minEdgeX, bufferRow, 0, bufferWidth);

					for (int j = 0; j < bufferWidth; j++)
					{
						int startIndex = j - alignTolerance;
						int endIndex = j + alignTolerance;

						if (startIndex < 0)
						{
							startIndex = 0;
						}
						if (endIndex > inspectionWidth - 1)
						{
							endIndex = inspectionWidth - 1;
						}

						int currentAbsDifference = 0;
						int minAbsDifference = 255;
						int minIndex = startIndex;

						for (int k = startIndex; k <= endIndex; k++)
						{
							currentAbsDifference = Math.Abs(profile[k] - bufferRow[j]);

							if (currentAbsDifference < minAbsDifference)
							{
								minAbsDifference = currentAbsDifference;
								minIndex = k;
							}
						}

						int currentDifference = profile[minIndex] - bufferRow[j];

						if (currentDifference > 0)
						{
							int maxBin = histogram[minIndex][profile[minIndex]];
							int currentBin = histogram[minIndex][bufferRow[j]];

							differenceImage[i * inspectionWidth + j] = (byte)((currentDifference * (maxBin - currentBin)) / maxBin);
						}
						else
						{
							differenceImage[i * inspectionWidth + j] = 0;
						}
					}

                    //// Display Difference Image
                    //Marshal.Copy(differenceImage, i * inspectionWidth, this.currentWorkplace.SharedBufferInfo.PtrList[1] + this.currentWorkplace.SharedBufferInfo.Width * (inspectoinStartY + i) + minEdgeX, inspectionWidth);
                }
				);

				#endregion

				#region [Inspection]

				byte[] binaryImage = new byte[inspectionWidth * inspectionHeight];
				Array.Clear(binaryImage, 0, binaryImage.Length);

				CLR_IP.Cpp_Threshold(differenceImage, binaryImage, inspectionWidth, inspectionHeight, false, param.Threshold);

                //// Display Binary Image
                //Parallel.For(0, inspectionHeight, i =>
                //{
                //    Marshal.Copy(binaryImage, i * inspectionWidth, this.currentWorkplace.SharedBufferInfo.PtrList[2] + this.currentWorkplace.SharedBufferInfo.Width * (inspectoinStartY + i) + minEdgeX, inspectionWidth);
                //}
                //);

                var label = CLR_IP.Cpp_Labeling(differenceImage, binaryImage, inspectionWidth, inspectionHeight, true);

				int defectSizeMinX = param.DefectSizeMinX;
				int defectSizeMaxX = param.DefectSizeMaxX;
				int defectSizeMinY = param.DefectSizeMinY;
				int defectSizeMaxY = param.DefectSizeMaxY;

				// Add Defect
				string sInspectionID = DatabaseManager.Instance.GetInspectionID();
				for (int l = 0; l < label.Length; l++)
				{
					if (label[l].width > defectSizeMinX && label[l].width < defectSizeMaxX
							&& label[l].height > defectSizeMinY && label[l].height < defectSizeMaxY)
					{
						int defectLeft = minEdgeX + label[l].boundLeft;
						int defectTop = inspectoinStartY + label[l].boundTop;

						this.currentWorkplace.AddDefect(sInspectionID,
							(int)defectCode /*10000 + (channelIndex * 100)*/,
							(float)(label[l].area),
							label[l].value,
							0,
							0,
							defectLeft,
							defectTop,
							(float)label[l].width,
							(float)label[l].height,
							this.currentWorkplace.MapIndexX,
							this.currentWorkplace.MapIndexY
							);
					}
				}

				#endregion
			}
			else
            {
				//if (this.GetWorkplaceBufferByIndex(channelIndex) == null)
				//	return;

				OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();
				int width = originRecipe.OriginWidth;
				int height = param.ROIHeight;

				int notchOffset = (int)(((param.EndPosition - param.StartPosition) / 360.0) * param.NotchOffsetDegree);

				int startPtY = param.StartPosition + notchOffset;
				int endPtY = param.EndPosition - notchOffset;
				if (originRecipe.OriginHeight < param.EndPosition)
					endPtY = originRecipe.OriginHeight;

				int count = (int)((endPtY - startPtY) / param.ROIHeight);
				//for (int i = 1; i < 3; i++)
				Parallel.For(1, count, i =>
				{
					int ptLeft = 0;
					int ptTop = startPtY + (i * height);
					int ptBtm = ptTop + height;

					// 검사영역이 end position을 넘어가는 경우
					if (ptBtm > endPtY)
					{
						ptBtm = endPtY;
						height = ptBtm - ptTop;
					}

					int bufferLength = width * height;
					byte[] inspectionROI = new byte[bufferLength];
					for (int y = ptTop; y < ptBtm; y++)
					{
						int startIdx = this.currentWorkplace.SharedBufferInfo.Width * y;
						int dstIdx = width * (y - ptTop);

						Marshal.Copy(this.currentWorkplace.SharedBufferInfo.PtrList[channelIndex] + startIdx, inspectionROI, dstIdx, width);
					}

					// bitmap save 
					//System.Drawing.Bitmap bmp = Tools.CovertArrayToBitmap(inspectionROI, width, height, 1);
					//bmp.Save("D:\\edge " + i + ".bmp");

					#region [Inspection]

					int startPtX = 0;
					// Edge Search
					if (param.UseEdgeSearch)
					{
						int edge = CLR_IP.Cpp_FindEdge(inspectionROI, width, height, 0, 0, (width - 1), (height - 1), 0, param.EdgeSearchLevel);
						startPtX = edge;

						// edge search fail
						if (startPtX >= width)
							startPtX = 0;
					}

					// profile 생성
					List<int> temp = new List<int>();
					List<int> profile = new List<int>();
					for (long x = 0; x < width; x++)
					{
						temp.Clear();
						for (long y = 0; y < bufferLength; y += width)
						{
							temp.Add(inspectionROI[x + y]);
						}
						temp.Sort();
						profile.Add(temp[temp.Count / 2]);  // 중앙값
					}

					// Calculate diff image (original - profile)
					byte[] diff = new byte[bufferLength];
					for (int y = 0; y < height; y++)
					{
						for (int x = startPtX; x < width; x++)
						{
							diff[(y * width) + x] = (byte)(Math.Abs(inspectionROI[(y * width) + x] - profile[x]));
						}
					}

					// Threshold and Labeling
					byte[] thresh = new byte[bufferLength];
					CLR_IP.Cpp_Threshold(diff, thresh, width, height, false, param.Threshold);
					var label = CLR_IP.Cpp_Labeling(diff, thresh, width, height, true);

					int defectSizeMinX = param.DefectSizeMinX;
					int defectSizeMaxX = param.DefectSizeMaxX;
					int defectSizeMinY = param.DefectSizeMinY;
					int defectSizeMaxY = param.DefectSizeMaxY;

					// Add defect
					string sInspectionID = DatabaseManager.Instance.GetInspectionID();
					for (int l = 0; l < label.Length; l++)
					{
						if (label[l].width > defectSizeMinX && label[l].width < defectSizeMaxX
							&& label[l].height > defectSizeMinY && label[l].height < defectSizeMaxY)
						{
							int defectLeft = ptLeft + label[l].boundLeft;
							int defectTop = ptTop + label[l].boundTop;

							this.currentWorkplace.AddDefect(sInspectionID,
								(int)defectCode /*10000 + (channelIndex * 100)*/,
								(float)(label[l].area),
								label[l].value,
								0,
								0,
								defectLeft,
								defectTop,
								(float)label[l].width,
								(float)label[l].height,
								this.currentWorkplace.MapIndexX,
								this.currentWorkplace.MapIndexY
								);
						}
					}
					#endregion
				}
				);
			}
		}
	}
}
