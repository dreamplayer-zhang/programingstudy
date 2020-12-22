using RootTools;
using RootTools.Camera;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using RootTools_CLR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Root_WIND2.Module
{
	public class Run_GrabEdge : ModuleRunBase
	{
		EdgeSideVision module;
		GrabMode gmTop = null;
		GrabMode gmSide = null;
		GrabMode gmBtm = null;

		string m_sGrabModeTop = "";
		public string p_sGrabModeTop
		{
			get { return m_sGrabModeTop; }
			set
			{
				m_sGrabModeTop = value;
				gmTop = module.GetGrabMode(value);
			}
		}
		string m_sGrabModeSide = "";
		public string p_sGrabModeSide
		{
			get { return m_sGrabModeSide; }
			set
			{
				m_sGrabModeSide = value;
				gmSide = module.GetGrabMode(value);
			}
		}
		string m_sGrabModeBtm = "";
		public string p_sGrabModeBtm
		{
			get { return m_sGrabModeBtm; }
			set
			{
				m_sGrabModeBtm = value;
				gmBtm = module.GetGrabMode(value);
			}
		}

		public Run_GrabEdge(EdgeSideVision module)
		{
			this.module = module;
			InitModuleRun(module);
		}

		double resolution = 1.7;    // um
		double startDegree = 0;
		double scanDegree = 360;
		double scanAcc = 1;
		int scanRate = 100;         // Camera Frame Spec 사용률 ? 1~100 %
		int maxFrame = 100;
		//double triggerRatio = 1.5;	// 캠익에서 트리거 분주비

		// recipe
		int sobelHeight = 10;
		int sobelThreshold = 90;
		int sobelCnt = 10;
		int sideFocusAxis = 26809;
		int inspHeight = 200;
		int defectSize = 5;
		int mergeDist = 5;
		int inspThreshhold = 12;

		public override ModuleRunBase Clone()
		{
			Run_GrabEdge run = new Run_GrabEdge(module);
			run.p_sGrabModeTop = p_sGrabModeTop;
			run.p_sGrabModeSide = p_sGrabModeSide;
			run.p_sGrabModeBtm = p_sGrabModeBtm;

			run.resolution = resolution;
			run.startDegree = startDegree;
			run.scanDegree = scanDegree;
			run.scanAcc = scanAcc;
			run.scanRate = scanRate;
			run.maxFrame = maxFrame;
			//run.triggerRatio = triggerRatio;

			run.sobelHeight = sobelHeight;
			run.sobelThreshold = sobelThreshold;
			run.sobelCnt = sobelCnt;
			run.sideFocusAxis = sideFocusAxis;
			run.inspHeight = inspHeight;
			run.defectSize = defectSize;
			run.mergeDist = mergeDist;
			run.inspThreshhold = inspThreshhold;
			return run;
		}

		public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
		{
			startDegree = tree.Set(startDegree, startDegree, "Start Angle", "Degree", bVisible);
			scanDegree = tree.Set(scanDegree, scanDegree, "Scan Angle", "Degree", bVisible);
			scanRate = tree.Set(scanRate, scanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100%", bVisible);
			maxFrame = tree.Set(maxFrame, maxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
			scanAcc = tree.Set(scanAcc, scanAcc, "Scan Acc", "스캔 축 가속도 (sec)", bVisible);

			// recipe
			sobelHeight = (tree.GetTree("Side Focus", false, bVisible)).Set(sobelHeight, sobelHeight, "Sobel Start Height", "", bVisible);
			sobelThreshold = (tree.GetTree("Side Focus", false, bVisible)).Set(sobelThreshold, sobelThreshold, "Edge Detect GV Threshold", "Sobel Edge 검출 시 GV Threshold", bVisible);
			sobelCnt = (tree.GetTree("Side Focus", false, bVisible)).Set(sobelCnt, sobelCnt, "Edge Detect Count", "Sobel Edge 검출 시 Threshold 이상의 Count. Count 이상 발견 시 Edge", bVisible);
			sideFocusAxis = (tree.GetTree("Side Focus", false, bVisible)).Set(sideFocusAxis, sideFocusAxis, "Side Focus Axis", "Side 카메라 Focus 축 값", bVisible);

			inspHeight = tree.Set(inspHeight, inspHeight, "Inspection ROI Height", "", bVisible);
			defectSize = tree.Set(defectSize, defectSize, "Defect Size", "pixel", bVisible);
			mergeDist = tree.Set(mergeDist, mergeDist, "Merge Distance", "pixel", bVisible);
			inspThreshhold = tree.Set(inspThreshhold, inspThreshhold, "Inspection Theshold", "", bVisible);
			//

			p_sGrabModeTop = tree.Set(p_sGrabModeTop, p_sGrabModeTop, module.p_asGrabMode, "Grab Mode : Top", "Select GrabMode", bVisible);
			if (gmTop != null) gmTop.RunTree(tree.GetTree("Grab Mode : Top", false), bVisible, true);
			p_sGrabModeSide = tree.Set(p_sGrabModeSide, p_sGrabModeSide, module.p_asGrabMode, "Grab Mode : Side", "Select GrabMode", bVisible);
			if (gmSide != null) gmSide.RunTree(tree.GetTree("Grab Mode : Side", false), bVisible, true);
			p_sGrabModeBtm = tree.Set(p_sGrabModeBtm, p_sGrabModeBtm, module.p_asGrabMode, "Grab Mode : Bottom", "Select GrabMode", bVisible);
			if (gmBtm != null) gmBtm.RunTree(tree.GetTree("Grab Mode : Bottom", false), bVisible, true);
		}

		public override string Run()
		{
			if (gmTop == null || gmSide == null || gmBtm == null) return "Grab Mode == null";

			try
			{
				gmTop.SetLight(true);
				gmSide.SetLight(true);
				gmBtm.SetLight(true);

				Axis axisR = module.AxisRotate;
				Axis axisEdgeX = module.AxisEdgeX;

				double pulsePerDegree = module.Pulse360 / 360;
				double curr = axisR.p_posActual - axisR.p_posActual % module.Pulse360;
				double triggerStart = curr + startDegree * pulsePerDegree;
				double triggerDest = triggerStart + scanDegree * pulsePerDegree;
				int trigger = 1;
				int scanSpeed = Convert.ToInt32((double)maxFrame * trigger * (double)scanRate / 100);
				double moveStart = triggerStart - scanAcc * scanSpeed;   //y 축 이동 시작 지점 
				double moveEnd = triggerDest + scanAcc * scanSpeed;  // Y 축 이동 끝 지점.
				int grabCount = Convert.ToInt32(scanDegree * pulsePerDegree * module.TriggerRatio);

				axisEdgeX.StartMove(sideFocusAxis);
				if (module.Run(axisEdgeX.WaitReady()))
					return p_sInfo;

				if (module.Run(axisR.StartMove(moveStart)))
					return p_sInfo;
				if (module.Run(axisR.WaitReady()))
					return p_sInfo;
				axisR.SetTrigger(triggerStart, triggerDest, trigger, true);

				gmTop.StartGrab(gmTop.m_memoryData, new CPoint(0, 0), grabCount);
				gmTop.Grabed += m_gmTop_Grabed;
				gmSide.StartGrab(gmSide.m_memoryData, new CPoint(0, 0), grabCount);
				gmBtm.StartGrab(gmBtm.m_memoryData, new CPoint(0, 0), grabCount);

				if (module.Run(axisR.StartMove(moveEnd, scanSpeed, scanAcc, scanAcc)))
					return p_sInfo;
				if (module.Run(axisR.WaitReady()))
					return p_sInfo;

				axisR.RunTrigger(false);
				gmTop.StopGrab();
				gmSide.StopGrab();
				gmBtm.StopGrab();
				return "OK";
			}
			finally
			{
				gmTop.SetLight(false);
				gmSide.SetLight(false);
				gmBtm.SetLight(false);
			}


			/*
			string sRstCam = module.OpenCamera();
			if (sRstCam != "OK")
			{
				return sRstCam;
			}
			module.p_bStageVac = true;

			string sRst = "None";
			sRst = GrabEdge();
			if (sRst != "OK")
				return sRst;

			return sRst;
			*/
		}

		private string GrabEdge()
		{
			Axis axisR = module.AxisRotate;
			Axis axisEdgeX = module.AxisEdgeX;

			try
			{
				gmTop.SetLight(true);

				double pulsePerDegree = module.Pulse360 / 360;
				double curr = axisR.p_posActual - axisR.p_posActual % module.Pulse360;
				double triggerStart = curr + startDegree * pulsePerDegree;
				double triggerDest = triggerStart + scanDegree * pulsePerDegree;
				int trigger = 1;

				int scanSpeed = Convert.ToInt32((double)maxFrame * trigger * (double)scanRate / 100);
				double moveStart = triggerStart - scanAcc * scanSpeed;   //y 축 이동 시작 지점 
				double moveEnd = triggerDest + scanAcc * scanSpeed;  // Y 축 이동 끝 지점.
				int grabCount = Convert.ToInt32(scanDegree * pulsePerDegree * module.TriggerRatio);

				if (module.Run(axisEdgeX.StartMove(sideFocusAxis)))
					return p_sInfo;
				if (module.Run(axisEdgeX.WaitReady()))
					return p_sInfo;
				if (module.Run(axisR.StartMove(moveStart)))
					return p_sInfo;
				if (module.Run(axisR.WaitReady()))
					return p_sInfo;

				axisR.SetTrigger(triggerStart, triggerDest, trigger, true);
				gmTop.StartGrab(gmTop.m_memoryData, new CPoint(0, 0), grabCount);
				gmTop.Grabed += m_gmTop_Grabed;
				gmSide.StartGrab(gmSide.m_memoryData, new CPoint(0, 0), grabCount);
				gmBtm.StartGrab(gmBtm.m_memoryData, new CPoint(0, 0), grabCount);

				if (module.Run(axisR.StartMove(moveEnd, scanSpeed, scanAcc, scanAcc)))
					return p_sInfo;
				if (module.Run(axisR.WaitReady()))
					return p_sInfo;

				/*
				gmBevelSide.StopGrab();
				//arr = "_focus_";
				//k_side = 0;

				//CalcAxisOffset();
				//RearrangeArray((int)m_fScanDegree);
				if (module.Run(axisR.StartMove(0)))
					return p_sInfo;
				if (module.Run(axisR.WaitReady()))
					return p_sInfo;

				axisEdgeX.StartMove((int)(module.EdgeXOfffset[0] * 1.7 * 10) - 26809 - 8868);
				if (module.Run(axisEdgeX.WaitReady()))
					return p_sInfo;

				gmBevelSide.StartGrab(module.m_memoryEdgeTop, new CPoint(4000, 0), nGrabCount);
				gmBevelSide.Grabed += gmBevelSide_Grabed;
				if (module.Run(axisR.StartMove(fMoveEnd, nScanSpeed, m_fScanAcc, m_fScanAcc)))
					return p_sInfo;

				//System.IO.StreamWriter sw = new System.IO.StreamWriter(new System.IO.FileStream(@"D:\axis.txt", System.IO.FileMode.Append));
				//sw.WriteLine(time);
				//sw.WriteLine("index , pixel , 축 좌표");
				int axis = 0;
				int nAxisDiff = 0;
				for (int i = 0; i < module.EdgeXOfffset.Count; i++)
				{
					axis = (int)(module.EdgeXOfffset[i] * 1.7 * 10);
					nAxisDiff = (axis - 26809) - 8868;
					//sw.WriteLine(i.ToString() + " , " + module.EdgeXOfffset[i].ToString() + " , " + nAxisDiff.ToString());

					axisEdgeX.StartMove(nAxisDiff);
					Thread.Sleep(7);
				}
				//sw.Close();

				if (module.Run(axisR.WaitReady()))
					return p_sInfo;
				*/

				axisR.RunTrigger(false);
				gmTop.StopGrab();
				gmSide.StopGrab();
				gmBtm.StopGrab();
				gmTop.SetLight(false);

				return "OK";
			}
			finally
			{
				axisR.RunTrigger(false);
				gmTop.SetLight(false);
			}
		}

		private void m_gmTop_Grabed(object sender, EventArgs e)
		{
			GrabedArgs ga = (GrabedArgs)e;
			int memW = Convert.ToInt32(ga.mdMemoryData.W);
			int memH = ga.mdMemoryData.p_sz.Y;

			Axis axisX = module.AxisEdgeX;

			IntPtr ptrSrc = (IntPtr)((long)ga.mdMemoryData.GetPtr() + ga.rtRoi.Left + ((long)memW * ga.rtRoi.Top));

			int sobelSize = sobelHeight;
			byte[] arrSrc = new byte[sobelSize * memW];
			byte[] arrDst = new byte[sobelSize * memW];
			Marshal.Copy(arrSrc, (memH / 2 - sobelSize) * memW, ptrSrc, (memW * sobelSize * 2));

			int derivativeX = 1;
			int derivativeY = 0;
			int kernelSize = 5;
			int scale = 1;
			int delta = 1;

			CLR_IP.Cpp_SobelEdgeDetection(arrSrc, arrDst, memW, memH, derivativeX, derivativeY, kernelSize, scale, delta);

			object o = new object();
			List<int> edgeDetect = new List<int>();
			edgeDetect.Clear();
			System.Threading.Tasks.Parallel.For(0, (sobelSize * 2), i =>
			{
				//lock (o)
				//{
				CalcSobelEdge(arrDst, memW, i, edgeDetect);
				//}
			});

			if (edgeDetect.Count == 0
				|| edgeDetect.Average() == 0
				|| edgeDetect.Average() > 100)
				return;

			axisX.StartMove((int)(edgeDetect.Average() * resolution * 10) - sideFocusAxis);
			if (module.Run(axisX.WaitReady()))
				return;
		}

		public void CalcSobelEdge(byte[] arrSobel, int nWidth, int nHeight, List<int> edgeDetect)
		{
			for (int i = 0; i < nWidth; i++)
			{
				if (arrSobel[i + (i * nHeight)] > sobelThreshold)
				{
					int nGVCnt = 0;
					int nStartGV = i;
					while (arrSobel[i + (i * nHeight)] > sobelThreshold)
					{
						nGVCnt++;
						i++;
					}
					if (nGVCnt > sobelCnt)
					{
						edgeDetect.Add(nStartGV);
						break;
					}
				}
			}
		}
				
		public void FindEdge()
		{
			int memW = Convert.ToInt32(module.MemoryEdgeTop.W);
			int memH = module.MemoryEdgeTop.p_sz.Y;
			int sobelSize = sobelHeight;

			int memSize = memW * sobelSize;

			for (int n = 1; n < memH / sobelSize; n++)
			{
				byte[] arrSrc = new byte[memSize];
				byte[] arrDst = new byte[memSize];

				Marshal.Copy(new IntPtr(module.MemoryEdgeTop.GetPtr().ToInt64() + (n * (Int64)memSize)),
							arrSrc,
							0,
							memSize);

				int nMin = 256;
				int nMax = 0;
				int nSearchLevel = 20;
				int prox = nMin + (int)((nMax - nMin) * nSearchLevel * 0.01);
				
				if (nSearchLevel >= 100) 
					prox = nMax;
				else if (nSearchLevel <= 0) 
					prox = nMin;

				int x1, x2, y1, y2;
				int nAvg, nAvgNext, nMinn;
				x1 = 0; 
				x2 = memW; // 3000;
				y1 = 0;
				y2 = sobelSize;

				nMinn = x2;
				nAvg = nAvgNext = 0;

				for (int y = y1; y <= y2; y++)
				{
					nAvgNext += arrSrc[y * x2];
				}
				if (nAvgNext != 0) nAvgNext /= (y2 - y1 + 1);

				for (int x = x1; x <= x2; x++)
				{
					nAvg = nAvgNext;
					nAvgNext = 0;
					for (int y = y1; y < memSize; y += memW)
					{
						nAvgNext += arrSrc[y + (x + 1)];
					}
					if (nAvgNext != 0) nAvgNext /= (y2 - y1 + 1);

					if ((nAvg >= prox && prox > nAvgNext) || (nAvg <= prox && prox < nAvgNext))
					{
						nMinn = x;
						x = x2 + 1;
					}
				}
			}
		}

		/*
		public void CalcAxisOffset()
		{
			int memW = Convert.ToInt32(module.MemoryEdgeTop.W);
			int memH = module.MemoryEdgeTop.p_sz.Y;
			int sobelSize = sobelHeight;
			int memSize = memW * sobelSize;

			for (int n = 1; n < memH / sobelSize; n++)
			{
				byte[] arrSrc = new byte[memSize];
				byte[] arrDst = new byte[memSize];

				Marshal.Copy(new IntPtr(module.MemoryEdgeTop.GetPtr().ToInt64() + (n * (Int64)memSize)),
							arrSrc,
							0,
							memSize);

				Emgu.CV.Mat matSrc = new Emgu.CV.Mat((int)sobelSize, (int)memW, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
				Marshal.Copy(arrSrc, 0, matSrc.DataPointer, arrSrc.Length);
				
				Emgu.CV.Mat matSobel = new Mat();
				Emgu.CV.Mat matAbsGradX = new Mat();

				int nScale = 1;
				int nDelta = 1;
				MCvScalar color = new MCvScalar(20, 100, 100);

				Emgu.CV.CvInvoke.Sobel(matSrc, matSobel, Emgu.CV.CvEnum.DepthType.Cv8U, 1, 0, 5, nScale, nDelta, Emgu.CV.CvEnum.BorderType.Default);
				Emgu.CV.CvInvoke.ConvertScaleAbs(matSobel, matAbsGradX, nScale, nDelta);

				byte[] arrSobel = matAbsGradX.GetRawData();

				List<int> listPixelX = new List<int>();
				List<Rectangle> listRect = new List<Rectangle>();
				for (int j = 0; j < matAbsGradX.Height; j += 5)
				{
					for (int i = 0; i < matAbsGradX.Width; i++)
					{
						if (arrSobel[(j * matAbsGradX.Width) + i] > 90)
						{
							int nGVCnt = 0;
							int nStartGV = i;
							while (arrSobel[(j * matAbsGradX.Width) + i] > 90)
							{
								nGVCnt++;
								i++;
							}
							if (nGVCnt > 10)
							{
								listPixelX.Add(nStartGV);
								listRect.Add(new Rectangle(nStartGV, j, i - nStartGV, 30));
								//Emgu.CV.CvInvoke.Rectangle(matAbsGradX, new Rectangle(nStartGV, j, i - nStartGV, 30), color);
								break;
							}
						}
					}
				}

				int nStartGVAvg = 0;
				if (listPixelX.Count != 0)
					nStartGVAvg = (int)listPixelX.Average();

				String path = @"D:\SKSiltron\" + n.ToString() + ".bmp";

				int nLTAvg = 0;
				int nWidthAvg = 0;
				if (listRect.Count != 0)
				{
					nLTAvg = (int)listRect.Average(x => x.Left);
					nWidthAvg = (int)listRect.Average(x => x.Width);
					Emgu.CV.CvInvoke.Rectangle(matAbsGradX, new Rectangle(nLTAvg, 0, nWidthAvg, matAbsGradX.Height), color);
					CvInvoke.CvtColor(matAbsGradX, matAbsGradX, Emgu.CV.CvEnum.ColorConversion.Gray2Rgb);
					matAbsGradX.Save(path);
				}

				//if (nStartGVAvg <= 0)
				//	module.EdgeXOfffset.Add(0);
				//else
				//	module.EdgeXOfffset.Add(nStartGVAvg);
				
			}
		}

		string time = DateTime.Now.ToString("HH-mm-ss");
		public void RearrangeArray(int degree)
		{
			if (module.EdgeXOfffset.Count == 0)
				return;

			try
			{
				System.IO.StreamWriter sw = new System.IO.StreamWriter(new System.IO.FileStream(@"D:\axis_top.txt", System.IO.FileMode.Append));
				sw.WriteLine(time);
				sw.WriteLine("index , top rect left");
				for (int i = 0; i < module.EdgeXOfffset.Count; i++)
				{
					sw.WriteLine(i.ToString() + " , " + module.EdgeXOfffset[i].ToString());
				}
				sw.Close();

				int nExtraDegree = degree - 360;
				int nCntPer45Degree = module.EdgeXOfffset.Count * 45 / degree;
				int nCntExtraAdd = module.EdgeXOfffset.Count * nExtraDegree / degree;

				List<int> insertList = new List<int>();
				for (int i = module.EdgeXOfffset.Count - nCntPer45Degree - nCntExtraAdd; i < module.EdgeXOfffset.Count - nCntPer45Degree; i++)
				{
					insertList.Add(module.EdgeXOfffset[i]);
				}
				for (int i = 0; i < module.EdgeXOfffset.Count - nCntExtraAdd; i++)
				{
					insertList.Add(module.EdgeXOfffset[i]);
				}

				module.EdgeXOfffset.Clear();
				module.EdgeXOfffset = insertList;

				for (int i = 1; i < module.EdgeXOfffset.Count; i++)
				{
					if (Math.Abs(module.EdgeXOfffset[i - 1] - module.EdgeXOfffset[i]) > 2000)
						module.EdgeXOfffset[i] = module.EdgeXOfffset[i - 1];
				}
			}
			catch
			{

			}
		}
		*/
	}


}
