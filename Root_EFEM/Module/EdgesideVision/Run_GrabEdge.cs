using RootTools;
using RootTools.Camera;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Root_EFEM.Module.EdgesideVision
{
	public class Run_GrabEdge : ModuleRunBase
	{
		Vision_Edgeside module;
		GrabModeEdge gmTop = null;
		GrabModeEdge gmSide = null;
		GrabModeEdge gmBtm = null;

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

		public Run_GrabEdge(Vision_Edgeside module)
		{
			this.module = module;
			InitModuleRun(module);
		}

		public GrabModeBase GetGrabMode(string grabModeName)
		{
			return module.GetGrabMode(grabModeName);
		}

		public override ModuleRunBase Clone()
		{
			Run_GrabEdge run = new Run_GrabEdge(module);
			run.p_sGrabModeTop = p_sGrabModeTop;
			run.p_sGrabModeSide = p_sGrabModeSide;
			run.p_sGrabModeBtm = p_sGrabModeBtm;

			return run;
		}

		public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
		{
			p_sGrabModeTop = (tree.GetTree("Grab Mode", false, bVisible).Set(p_sGrabModeTop, p_sGrabModeTop, module.p_asGrabMode, "Grab Mode : Top", "Select GrabMode", bVisible));
			p_sGrabModeSide = (tree.GetTree("Grab Mode", false, bVisible).Set(p_sGrabModeSide, p_sGrabModeSide, module.p_asGrabMode, "Grab Mode : Side", "Select GrabMode", bVisible));
			p_sGrabModeBtm = (tree.GetTree("Grab Mode", false, bVisible).Set(p_sGrabModeBtm, p_sGrabModeBtm, module.p_asGrabMode, "Grab Mode : Bottom", "Select GrabMode", bVisible));
		}

		public override string Run()
		{
			module.p_bStageVac = true;

			if (gmTop == null || gmSide == null || gmBtm == null) return "Grab Mode == null";

			try
			{
				gmTop.SetLight(true);

				Axis axisR = module.AxisRotate;
				Axis axisEdgeX = module.AxisEdgeX;

				double pulsePerDegree = module.Pulse360 / 360;
				int camHeight = module.CamEdgeTop.GetRoiSize().Y;
				int scanSpeed = Convert.ToInt32((double)gmTop.m_nMaxFrame * gmTop.m_dTrigger * camHeight * (double)gmTop.m_nScanRate / 100); //56000

				//double curr = axisR.p_posActual - axisR.p_posActual % module.Pulse360;
				//double triggerStart = curr + startDegree * pulsePerDegree;
				double triggerStart = gmTop.m_nStartDegree * pulsePerDegree;
				double triggerDest = triggerStart + gmTop.m_nScanDegree * pulsePerDegree;
				
				// 시계 방향
				//double moveStart = triggerStart - axisR.GetSpeedValue(Axis.eSpeed.Move).m_acc * scanSpeed;   //y 축 이동 시작 지점 
				//double moveEnd = triggerDest + axisR.GetSpeedValue(Axis.eSpeed.Move).m_acc * scanSpeed;  // Y 축 이동 끝 지점
				// 반시계 방향
				double moveStart = triggerDest + axisR.GetSpeedValue(Axis.eSpeed.Move).m_acc * scanSpeed;  // Y 축 이동 끝 지점
				double moveEnd = triggerStart - axisR.GetSpeedValue(Axis.eSpeed.Move).m_acc * scanSpeed;   //y 축 이동 시작 지점

                int grabCount = Convert.ToInt32((gmTop.m_nScanDegree / 360.0) * gmTop.m_nWaferSize_mm * pulsePerDegree * Math.PI / gmTop.m_dRealResX_um); // m_nWaferSize_mm = 300

				if (module.Run(axisEdgeX.StartMove(gmTop.m_nFocusX)))
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

				if (module.Run(axisR.StartMove(moveEnd, 30000, axisR.GetSpeedValue(Axis.eSpeed.Move).m_acc, axisR.GetSpeedValue(Axis.eSpeed.Move).m_acc)))
					return p_sInfo;
				if (module.Run(axisR.WaitReady()))
					return p_sInfo;

				while (gmTop.m_camera.p_nGrabProgress != 100 || gmSide.m_camera.p_nGrabProgress != 100 || gmBtm.m_camera.p_nGrabProgress != 100)
				{
					System.Threading.Thread.Sleep(10);
					//m_log.Info("Wait Camera GrabProcess");
				}

				axisR.RunTrigger(false);
				gmTop.StopGrab();
				gmSide.StopGrab();
				gmBtm.StopGrab();
				return "OK";
			}
			finally
			{
				gmTop.SetLight(false);
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

		//public void FindEdge()
		//{
		//	string path = @"D:\SKSiltron\SKSiltron_EdgeStage_210104\lifter up,down 반복성\";
		//	StreamWriter sw = new StreamWriter(path + DateTime.Now.ToString("yy-MM-dd_HH-mm-ss") + ".csv");

		//	int memW = Convert.ToInt32(module.MemoryEdgeTop.W);
		//	int memH = module.MemoryEdgeTop.p_sz.Y;
		//	int sobelSize = edgeDetectHeight;

		//	int memSize = memW * sobelSize;

		//	for (int n = 3; n < memH / sobelSize; n++)
		//	{
		//		byte[] arrSrc = new byte[memSize];
		//		byte[] arrDst = new byte[memSize];

		//		Marshal.Copy(new IntPtr(module.MemoryEdgeTop.GetPtr().ToInt64() + (n * (Int64)memSize)),
		//					arrSrc,
		//					0,
		//					memSize);

		//		Emgu.CV.Mat mat = new Emgu.CV.Mat((int)sobelSize, (int)memW, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
		//		Marshal.Copy(arrSrc, 0, mat.DataPointer, arrSrc.Length);
		//		//mat.Save(path + n.ToString() + ".bmp");
		//		Emgu.CV.Structure.MCvScalar color = new Emgu.CV.Structure.MCvScalar(20, 100, 100);
		//		//return;

		//		int nMin = 256;
		//		int nMax = 0;
		//		int nSearchLevel = 70;
		//		int prox = nMin + (int)((nMax - nMin) * nSearchLevel * 0.01);

		//		if (nSearchLevel >= 100)
		//			prox = nMax;
		//		else if (nSearchLevel <= 0)
		//			prox = nMin;

		//		int x1, x2, y1, y2;
		//		int nAvg, nAvgNext, nMinn;
		//		x1 = 500;
		//		x2 = 1500;// memW; // 3000;
		//		y1 = 0;
		//		y2 = sobelSize;

		//		nMinn = x2;
		//		nAvg = nAvgNext = 0;

		//		for (int y = y1; y < y2; y++)
		//		{
		//			nAvgNext += arrSrc[y * x2];
		//		}
		//		if (nAvgNext != 0) nAvgNext /= (y2 - y1 + 1);

		//		for (int x = x1; x < x2; x++)
		//		{
		//			nAvg = nAvgNext;
		//			nAvgNext = 0;
		//			for (int y = y1; y < memSize; y += memW)
		//			{
		//				nAvgNext += arrSrc[y + (x + 1)];
		//			}
		//			if (nAvgNext != 0) nAvgNext /= (y2 - y1 + 1);

		//			if ((nAvg >= prox && prox > nAvgNext) || (nAvg <= prox && prox < nAvgNext))
		//			{
		//				nMinn = x;
		//				x = x2 + 1;
		//			}
		//		}

		//		sw.WriteLine(nMinn);
		//		Emgu.CV.CvInvoke.Line(mat, new System.Drawing.Point(nMinn, 0), new System.Drawing.Point(nMinn, sobelSize), color, 2);
		//		mat.Save(path + n.ToString() + ".bmp");
		//	}
		//	sw.Close();
		//}
	}
}
