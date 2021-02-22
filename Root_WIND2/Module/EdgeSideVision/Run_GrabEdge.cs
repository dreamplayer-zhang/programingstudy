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
		string m_sGrabModeSide = "";
		string m_sGrabModeBtm = "";

		#region [Getter/Setter]
		public string p_sGrabModeTop
		{
			get { return m_sGrabModeTop; }
			set
			{
				m_sGrabModeTop = value;
				gmTop = module.GetGrabMode(value);
			}
		}
		public string p_sGrabModeSide
		{
			get { return m_sGrabModeSide; }
			set
			{
				m_sGrabModeSide = value;
				gmSide = module.GetGrabMode(value);
			}
		}
		public string p_sGrabModeBtm
		{
			get { return m_sGrabModeBtm; }
			set
			{
				m_sGrabModeBtm = value;
				gmBtm = module.GetGrabMode(value);
			}
		}
		#endregion

		double startDegree = 0;
		double scanDegree = 360;

		int sideFocusAxis = 0;
		int edgeDetectHeight = 0;

		public Run_GrabEdge(EdgeSideVision module)
		{
			this.module = module;
			InitModuleRun(module);
		}

		public GrabMode GetGrabMode(string grabModeName)
		{
			return module.GetGrabMode(grabModeName);
		}

		public override ModuleRunBase Clone()
		{
			Run_GrabEdge run = new Run_GrabEdge(module);
			run.p_sGrabModeTop = p_sGrabModeTop;
			run.p_sGrabModeSide = p_sGrabModeSide;
			run.p_sGrabModeBtm = p_sGrabModeBtm;

			run.startDegree = startDegree;
			run.scanDegree = scanDegree;

			run.sideFocusAxis = sideFocusAxis;
			run.edgeDetectHeight = edgeDetectHeight;
			return run;
		}

		public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
		{
			startDegree = tree.Set(startDegree, startDegree, "Start Angle", "Degree", bVisible);
			scanDegree = tree.Set(scanDegree, scanDegree, "Scan Angle", "Degree", bVisible);

			sideFocusAxis = (tree.GetTree("Side Focus", false, bVisible)).Set(sideFocusAxis, sideFocusAxis, "Side Focus Axis", "Side 카메라 Focus 축 값", bVisible);
			edgeDetectHeight = (tree.GetTree("Side Focus", false, bVisible)).Set(edgeDetectHeight, edgeDetectHeight, "Edge Detect Height", "Top Edge 검출영역 Height", bVisible);

			p_sGrabModeTop = tree.Set(p_sGrabModeTop, p_sGrabModeTop, module.p_asGrabMode, "Grab Mode : Top", "Select GrabMode", bVisible);
			//if (gmTop != null) 
			//	gmTop.RunTree(tree.GetTree("Grab Mode : Top", false), bVisible, true);
			p_sGrabModeSide = tree.Set(p_sGrabModeSide, p_sGrabModeSide, module.p_asGrabMode, "Grab Mode : Side", "Select GrabMode", bVisible);
			//if (gmSide != null) 
			//	gmSide.RunTree(tree.GetTree("Grab Mode : Side", false), bVisible, true);
			p_sGrabModeBtm = tree.Set(p_sGrabModeBtm, p_sGrabModeBtm, module.p_asGrabMode, "Grab Mode : Bottom", "Select GrabMode", bVisible);
			//if (gmBtm != null) 
			//	gmBtm.RunTree(tree.GetTree("Grab Mode : Bottom", false), bVisible, true);
		}

		public override string Run()
		{
			module.p_bStageVac = true;

			if (gmTop == null || gmSide == null || gmBtm == null) return "Grab Mode == null";

			try
			{
				gmTop.SetLight(true);
				//gmSide.SetLight(true);
				//gmBtm.SetLight(true);

				Axis axisR = module.AxisRotate;
				Axis axisEdgeX = module.AxisEdgeX;

				double pulsePerDegree = module.Pulse360 / 360;
				int camHeight = module.CamEdgeTop.GetRoiSize().Y;
				int scanSpeed = Convert.ToInt32((double)gmTop.m_nMaxFrame * gmTop.m_dTrigger * camHeight * (double)gmTop.m_nScanRate / 100); //56000
				
				//double curr = axisR.p_posActual - axisR.p_posActual % module.Pulse360;
				//double triggerStart = curr + startDegree * pulsePerDegree;
				double triggerStart = startDegree * pulsePerDegree;
				double triggerDest = triggerStart + scanDegree * pulsePerDegree;
				//double moveStart = triggerStart - axisR.GetSpeedValue(Axis.eSpeed.Move).m_acc * scanSpeed;   //y 축 이동 시작 지점 
				//double moveEnd = triggerDest + axisR.GetSpeedValue(Axis.eSpeed.Move).m_acc * scanSpeed;  // Y 축 이동 끝 지점.
				double moveStart = triggerDest + axisR.GetSpeedValue(Axis.eSpeed.Move).m_acc * scanSpeed;  // Y 축 이동 끝 지점.
				double moveEnd = triggerStart - axisR.GetSpeedValue(Axis.eSpeed.Move).m_acc * scanSpeed;   //y 축 이동 시작 지점 
				int grabCount = Convert.ToInt32(scanDegree * pulsePerDegree * gmTop.m_dCamTriggerRatio); //module.EdgeCamTriggerRatio);

				if (module.Run(axisEdgeX.StartMove(sideFocusAxis)))
					return p_sInfo;
				if (module.Run(axisEdgeX.WaitReady()))
					return p_sInfo;
				if (module.Run(axisR.StartMove(moveStart)))
					return p_sInfo;
				if (module.Run(axisR.WaitReady()))
					return p_sInfo;
				axisR.SetTrigger(triggerStart, triggerDest, 1, true);

                gmTop.StartGrab(gmTop.m_memoryData, new CPoint(0, 0), grabCount, gmTop.m_GD, true);
                gmTop.Grabed += m_gmTop_Grabed;
                gmSide.StartGrab(gmSide.m_memoryData, new CPoint(0, 0), grabCount, gmSide.m_GD, true);
                gmBtm.StartGrab(gmBtm.m_memoryData, new CPoint(0, 0), grabCount, gmBtm.m_GD, true);

                if (module.Run(axisR.StartMove(moveEnd, scanSpeed, axisR.GetSpeedValue(Axis.eSpeed.Move).m_acc, axisR.GetSpeedValue(Axis.eSpeed.Move).m_acc)))
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
				//gmSide.SetLight(false);
				//gmBtm.SetLight(false);
			}
		}
		
		private void m_gmTop_Grabed(object sender, EventArgs e)
		{
			GrabedArgs ga = (GrabedArgs)e;
			module.p_nProgress = ga.nProgress;
			int memW = Convert.ToInt32(ga.mdMemoryData.W);
			int memH = ga.mdMemoryData.p_sz.Y;

			Axis axisX = module.AxisEdgeX;
			IntPtr ptrSrc = (IntPtr)((long)ga.mdMemoryData.GetPtr() + ga.rtRoi.Left + ((long)memW * ga.rtRoi.Top));

			/*
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
			*/
		}


		int sobelThreshold = 90;
		int sobelCnt = 10;
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
			string path = @"D:\SKSiltron\SKSiltron_EdgeStage_210104\lifter up,down 반복성\";
			StreamWriter sw = new StreamWriter(path + DateTime.Now.ToString("yy-MM-dd_HH-mm-ss") + ".csv");

			int memW = Convert.ToInt32(module.MemoryEdgeTop.W);
			int memH = module.MemoryEdgeTop.p_sz.Y;
			int sobelSize = edgeDetectHeight;

			int memSize = memW * sobelSize;

			for (int n = 3; n < memH / sobelSize; n++)
			{
				byte[] arrSrc = new byte[memSize];
				byte[] arrDst = new byte[memSize];

				Marshal.Copy(new IntPtr(module.MemoryEdgeTop.GetPtr().ToInt64() + (n * (Int64)memSize)),
							arrSrc,
							0,
							memSize);

				Emgu.CV.Mat mat = new Emgu.CV.Mat((int)sobelSize, (int)memW, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
				Marshal.Copy(arrSrc, 0, mat.DataPointer, arrSrc.Length);
				//mat.Save(path + n.ToString() + ".bmp");
				Emgu.CV.Structure.MCvScalar color = new Emgu.CV.Structure.MCvScalar(20, 100, 100);
				//return;

				int nMin = 256;
				int nMax = 0;
				int nSearchLevel = 70;
				int prox = nMin + (int)((nMax - nMin) * nSearchLevel * 0.01);
				
				if (nSearchLevel >= 100) 
					prox = nMax;
				else if (nSearchLevel <= 0) 
					prox = nMin;

				int x1, x2, y1, y2;
				int nAvg, nAvgNext, nMinn;
				x1 = 500;
				x2 = 1500;// memW; // 3000;
				y1 = 0;
				y2 = sobelSize;

				nMinn = x2;
				nAvg = nAvgNext = 0;

				for (int y = y1; y < y2; y++)
				{
					nAvgNext += arrSrc[y * x2];
				}
				if (nAvgNext != 0) nAvgNext /= (y2 - y1 + 1);

				for (int x = x1; x < x2; x++)
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

				sw.WriteLine(nMinn);
				Emgu.CV.CvInvoke.Line(mat, new System.Drawing.Point(nMinn, 0), new System.Drawing.Point(nMinn, sobelSize), color, 2);
				mat.Save(path + n.ToString() + ".bmp");
			}
			sw.Close();
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
