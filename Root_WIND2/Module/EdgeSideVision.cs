﻿using RootTools;
using RootTools.Control;
using RootTools.Camera;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using RootTools_CLR;
using System.Runtime.InteropServices;
using System.Activities.Statements;
using RootTools.Database;
using System.ComponentModel.Design;
using System.IO;
using RootTools_Vision;

namespace Root_WIND2.Module
{
	public class EdgeSideVision : ModuleBase
	{
		#region ToolBox
		Axis m_axisRotate;
		Axis m_axisEdgeSideX;
		AxisXY m_axisXZ;
		DIO_O m_doVac;
		DIO_O m_doBlow;
		MemoryPool m_memoryPool;
		MemoryGroup m_memoryGroup;
		LightSet m_lightSet;
		Camera_Dalsa m_camEdgeTop;
		Camera_Dalsa m_camEdgeSide;
		Camera_Dalsa m_camEdgeBottom;
		Camera_Basler m_camNotchTop;
		Camera_Basler m_camNotchSide;
		Camera_Basler m_camNotchBottom;

		public override void GetTools(bool bInit)
		{
			p_sInfo = m_toolBox.Get(ref m_axisRotate, this, "Axis Rotate");
			p_sInfo = m_toolBox.Get(ref m_axisEdgeSideX, this, "Axis EdgeSideX");
			p_sInfo = m_toolBox.Get(ref m_axisXZ, this, "Axis NotchXZ");
			p_sInfo = m_toolBox.Get(ref m_doVac, this, "Stage Vacuum");
			p_sInfo = m_toolBox.Get(ref m_doBlow, this, "Stage Blow");
			p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory", 1);
			p_sInfo = m_toolBox.Get(ref m_camEdgeTop, this, "Cam EdgeTop");
			p_sInfo = m_toolBox.Get(ref m_camEdgeSide, this, "Cam EdgeSide");
			p_sInfo = m_toolBox.Get(ref m_camEdgeBottom, this, "Cam EdgeBottom");
			p_sInfo = m_toolBox.Get(ref m_camNotchTop, this, "Cam NotchTop");
			p_sInfo = m_toolBox.Get(ref m_camNotchSide, this, "Cam NotchSide");
			p_sInfo = m_toolBox.Get(ref m_camNotchBottom, this, "Cam NotchBottom");
			p_sInfo = m_toolBox.Get(ref m_lightSet, this);
			m_memoryGroup = m_memoryPool.GetGroup(p_id);
		}
		#endregion

		#region DIO
		public bool p_bStageVac
		{
			get { return m_doVac.p_bOut; }
			set
			{
				if (m_doVac.p_bOut == value) return;
				m_doVac.Write(value);
			}
		}

		public bool p_bStageBlow
		{
			get { return m_doBlow.p_bOut; }
			set
			{
				if (m_doBlow.p_bOut == value) return;
				m_doBlow.Write(value);
			}
		}

		public void RunBlow(int msDelay)
		{
			m_doBlow.DelayOff(msDelay);
		}
		#endregion

		#region override
		public override void Reset()
		{
			base.Reset();
		}

		public override void InitMemorys()
		{
			m_memoryGroup = m_memoryPool.GetGroup(p_id);
		}
		#endregion

		#region Axis
		public int m_pulseRound = 1000;
		void RunTreeAxis(Tree tree)
		{
			m_pulseRound = tree.Set(m_pulseRound, m_pulseRound, "Rotate Pulse / Round", "Rotate Axis Pulse / 1 Round (pulse)");
		}
		#endregion

		#region State Home
		public string OpenCamera()
		{
			if (m_camEdgeTop.p_CamInfo.p_eState == eCamState.Init)
				m_camEdgeTop.Connect();
			if (m_camEdgeSide.p_CamInfo.p_eState == eCamState.Init)
				m_camEdgeSide.Connect();
			if (m_camEdgeBottom.p_CamInfo.p_eState == eCamState.Init)
				m_camEdgeBottom.Connect();
			if (m_camNotchTop.p_CamInfo._OpenStatus == false)
				m_camNotchTop.Connect();
			if (m_camNotchSide.p_CamInfo._OpenStatus == false)
				m_camNotchSide.Connect();
			if (m_camNotchBottom.p_CamInfo._OpenStatus == false)
				m_camNotchBottom.Connect();
			return "OK";
		}

		public override string StateHome()
		{
			if (EQ.p_bSimulate) return "OK";
			//            p_bStageBlow = false;
			//            p_bStageVac = true;

			OpenCamera();
			p_bStageVac = true;
			m_axisEdgeSideX.StartHome();
			if (m_axisEdgeSideX.WaitReady() != "OK")
			{
				p_bStageVac = false;
				p_eState = eState.Error;
				return "OK";
			}

			Thread.Sleep(200);
			m_axisRotate.StartHome();
			if (m_axisRotate.WaitReady() == "OK")
			{
				p_eState = eState.Ready;
				return "OK";
			}
			//p_sInfo = base.StateHome();
			//p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
			p_eState = eState.Error;
			p_bStageVac = false;
			return "Home Error";
		}
		#endregion

		#region GrabMode
		int m_lGrabMode = 0;
		public ObservableCollection<GrabMode> m_aGrabMode = new ObservableCollection<GrabMode>();

		public List<string> p_asGrabMode
		{
			get
			{
				List<string> asGrabMode = new List<string>();
				foreach (GrabMode grabMode in m_aGrabMode)
					asGrabMode.Add(grabMode.p_sName);
				return asGrabMode;
			}
		}

		public GrabMode GetGrabMode(string sGrabMode)
		{
			foreach (GrabMode grabMode in m_aGrabMode)
			{
				if (sGrabMode == grabMode.p_sName)
					return grabMode;
			}
			return null;
		}

		void RunTreeGrabMode(Tree tree)
		{
			m_lGrabMode = tree.Set(m_lGrabMode, m_lGrabMode, "Count", "Grab Mode Count");
			while (m_aGrabMode.Count < m_lGrabMode)
			{
				string id = "Mode." + m_aGrabMode.Count.ToString("00");
				GrabMode grabMode = new GrabMode(id, m_cameraSet, m_lightSet, m_memoryPool);
				m_aGrabMode.Add(grabMode);
			}
			while (m_aGrabMode.Count > m_lGrabMode)
				m_aGrabMode.RemoveAt(m_aGrabMode.Count - 1);
			foreach (GrabMode grabMode in m_aGrabMode)
				grabMode.RunTreeName(tree.GetTree("Name", false));
			foreach (GrabMode grabMode in m_aGrabMode)
				grabMode.RunTree(tree.GetTree(grabMode.p_sName, false), true, false);
		}
		#endregion

		#region Tree
		public override void RunTree(Tree tree)
		{
			base.RunTree(tree);
			RunTreeGrabMode(tree.GetTree("Grab Mode", false));
		}
		#endregion

		public EdgeSideVision(string id, IEngineer engineer)
		{ 
			base.InitBase(id, engineer);
			InitMemory();
		}

		MemoryData m_memoryEdgeTop;
		MemoryData m_memoryEdgeSide;
		MemoryData m_memoryEdgeBottom;
		MemoryData m_memoryNotch;
		MemoryData[] m_memoryNotchSide;
		MemoryData[] m_memoryNotchTop;
		MemoryData[] m_memoryNotchBottom;
		double dPulse360 = 360000;
		double dTriggerratio = 1.5; //캠익에서 트리거 분주비
		double dMargin = 36000;
		int nBaslerX = 2048;
		int nBaslerY = 1080;

		public enum eMemData
		{
			EdgeTop,
			EdgeSide,
			EdgeBottom,
		}

		void InitMemory()
		{
			int nImageY = (int)(dPulse360 * dTriggerratio + dMargin);
			int nImageX = 3000;
			m_memoryEdgeTop = m_memoryPool.GetGroup(p_id).CreateMemory(eMemData.EdgeTop.ToString(), 1, 1, nImageX, nImageY);
            m_memoryEdgeSide = m_memoryPool.GetGroup(p_id).CreateMemory(eMemData.EdgeSide.ToString(), 1, 1, nImageX, nImageY);
            m_memoryEdgeBottom = m_memoryPool.GetGroup(p_id).CreateMemory(eMemData.EdgeBottom.ToString(), 1, 1, nImageX, nImageY);
            //m_memoryNotch = m_memoryPool.GetGroup(p_id).CreateMemory("Notch", 1, 1, nBaslerX * 9, nBaslerY * 40);
			//InitStackMemory(3, 20, 3, 20, 3, 20);
		}

        public ImageData GetMemoryData(eMemData data)
		{
			ImageData result = new ImageData(m_memoryPool.GetMemory(p_id, data.ToString()));
			return result;
		}

		void InitStackMemory(int nSide, int nFrameSide, int nTop, int nFrameTop, int nBottom, int nFrameBottom)
		{
			m_memoryNotchSide = new MemoryData[nSide];
			for (int n = 0; n < nSide; n++)
			{
				m_memoryNotchSide[n] = m_memoryPool.GetGroup(p_id).CreateMemory("NotchSide" + n, 1, 1, nBaslerX, nBaslerY * nFrameSide);
			}
			m_memoryNotchTop = new MemoryData[nTop];
			for (int n = 0; n < nTop; n++)
			{
				m_memoryNotchSide[n] = m_memoryPool.GetGroup(p_id).CreateMemory("NotchTop" + n, 1, 1, nBaslerX, nBaslerY * nFrameTop);
			}
			m_memoryNotchBottom = new MemoryData[nBottom];
			for (int n = 0; n < nBottom; n++)
			{
				m_memoryNotchSide[n] = m_memoryPool.GetGroup(p_id).CreateMemory("NotchBottom" + n, 1, 1, nBaslerX, nBaslerY * nFrameBottom);
			}
		}

		public override void ThreadStop()
		{
			base.ThreadStop();
		}

		#region ModuleRun
		protected override void InitModuleRuns()
		{
			AddModuleRunList(new Run_GrabEdge(this), false, "Run Grab Edge");
			AddModuleRunList(new Run_GrabNotch(this), false, "Run Grab Notch");
			AddModuleRunList(new Run_GrabAreaLine(this), false, "Run Grab Area Line");
			//AddModuleRunList(new Run_Delay(this), false, "Time Delay");
			//AddModuleRunList(new Run_Rotate(this), false, "Rotate Axis");
		}

		public class Run_GrabEdge : ModuleRunBase
		{
			EdgeSideVision m_module;
			public GrabMode m_gmTop = null;
			public GrabMode m_gmSide = null;
			public GrabMode m_gmBtm = null;

			string _sGrabModeTop = "";
			string p_sGrabModeTop
			{
				get { return _sGrabModeTop; }
				set
				{
					_sGrabModeTop = value;
					m_gmTop = m_module.GetGrabMode(value);
				}
			}
			string _sGrabModeSide = "";
			string p_sGrabModeSide
			{
				get { return _sGrabModeSide; }
				set
				{
					_sGrabModeSide = value;
					m_gmSide = m_module.GetGrabMode(value);
				}
			}
			string _sGrabModeBtm = "";
			string p_sGrabModeBtm
			{
				get { return _sGrabModeBtm; }
				set
				{
					_sGrabModeBtm = value;
					m_gmBtm = m_module.GetGrabMode(value);
				}
			}

			public double m_fRes = 1.7; // 단위 um
			public double m_fStartDegree = 0;
			public double m_fScanDegree = 360;
			public int m_nScanRate = 100;   // Camera Frame Spec 사용률 ? 1~100 %
			public double m_fScanAcc = 1; //sec
			public int m_nMaxFrame = 100;
			public int m_nSobelHeight = 10;
			public int m_nSobelThreshold = 90;
			public int m_nSobelCnt = 10;
			public int m_nSideFocusAxis = 26809;
			public double m_nSideCamResoultion = 1.7;
			public int m_nInspHeight = 200;
			public int m_nDefectSize = 5;
			public int m_nMergeDist = 5;
			public int m_nInspThreshhold = 12;

			public Run_GrabEdge(EdgeSideVision module)
			{
				m_module = module;
				InitModuleRun(module);
			}

			public override ModuleRunBase Clone()
			{
				Run_GrabEdge run = new Run_GrabEdge(m_module);
				run.p_sGrabModeTop = p_sGrabModeTop;
				run.p_sGrabModeSide = p_sGrabModeSide;
				run.p_sGrabModeBtm = p_sGrabModeBtm;

				run.m_fRes = m_fRes;
				run.m_fStartDegree = m_fStartDegree;
				run.m_fScanDegree = m_fScanDegree;
				run.m_nScanRate = m_nScanRate;
				run.m_fScanAcc = m_fScanAcc;
				run.m_nMaxFrame = m_nMaxFrame;
				run.m_nSobelHeight = m_nSobelHeight;
				run.m_nSobelThreshold = m_nSobelThreshold;
				run.m_nSobelCnt = m_nSobelCnt;
				run.m_nSideFocusAxis = m_nSideFocusAxis;
				run.m_nSideCamResoultion = m_nSideCamResoultion;
				run.m_nInspHeight = m_nInspHeight;
				run.m_nDefectSize = m_nDefectSize;
				run.m_nMergeDist = m_nMergeDist;
				run.m_nInspThreshhold = m_nInspThreshhold;
				return run;
			}

			public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
			{
				m_fStartDegree = tree.Set(m_fStartDegree, m_fStartDegree, "Start Angle", "Degree", bVisible);
				m_fScanDegree = tree.Set(m_fScanDegree, m_fScanDegree, "Scan Angle", "Degree", bVisible);
				m_nScanRate = tree.Set(m_nScanRate, m_nScanRate, "Scan Rate", "카메라 Frame 사용률 1~ 100%", bVisible);
				m_nMaxFrame = tree.Set(m_nMaxFrame, m_nMaxFrame, "Max Frame", "Camera Max Frame Spec", bVisible);
				m_fScanAcc = tree.Set(m_fScanAcc, m_fScanAcc, "Scan Acc", "스캔 축 가속도 (sec)", bVisible);

				m_nSobelHeight = (tree.GetTree("Side Focus", false, bVisible)).Set(m_nSobelHeight, m_nSobelHeight, "Sobel Start Height", "", bVisible);
				m_nSobelThreshold = (tree.GetTree("Side Focus", false, bVisible)).Set(m_nSobelThreshold, m_nSobelThreshold, "Edge Detect GV Threshold", "Sobel Edge 검출 시 GV Threshold", bVisible);
				m_nSobelCnt = (tree.GetTree("Side Focus", false, bVisible)).Set(m_nSobelCnt, m_nSobelCnt, "Edge Detect Count", "Sobel Edge 검출 시 Threshold 이상의 Count. Count 이상 발견 시 Edge", bVisible);
				m_nSideFocusAxis = (tree.GetTree("Side Focus", false, bVisible)).Set(m_nSideFocusAxis, m_nSideFocusAxis, "Side Focus Axis", "Side 카메라 Focus 축 값", bVisible);
				m_nSideCamResoultion = (tree.GetTree("Side Focus", false, bVisible)).Set(m_nSideCamResoultion, m_nSideCamResoultion, "Side Cam Resolution", "Side 카메라 해상도", bVisible);

				m_nInspHeight = tree.Set(m_nInspHeight, m_nInspHeight, "Inspection ROI Height", "", bVisible);
				m_nDefectSize = tree.Set(m_nDefectSize, m_nDefectSize, "Defect Size", "pixel", bVisible);
				m_nMergeDist = tree.Set(m_nMergeDist, m_nMergeDist, "Merge Distance", "pixel", bVisible);
				m_nInspThreshhold = tree.Set(m_nInspThreshhold, m_nInspThreshhold, "Inspection Theshold", "", bVisible);

				p_sGrabModeTop = tree.Set(p_sGrabModeTop, p_sGrabModeTop, m_module.p_asGrabMode, "Grab Mode : Top", "Select GrabMode", bVisible);
				if (m_gmTop != null) m_gmTop.RunTree(tree.GetTree("Grab Mode : Top", false), bVisible, true);
				p_sGrabModeSide = tree.Set(p_sGrabModeSide, p_sGrabModeSide, m_module.p_asGrabMode, "Grab Mode : Side", "Select GrabMode", bVisible);
				if (m_gmSide != null) m_gmTop.RunTree(tree.GetTree("Grab Mode : Side", false), bVisible, true);
				p_sGrabModeBtm = tree.Set(p_sGrabModeBtm, p_sGrabModeBtm, m_module.p_asGrabMode, "Grab Mode : Bottom", "Select GrabMode", bVisible);
				if (m_gmBtm != null) m_gmTop.RunTree(tree.GetTree("Grab Mode : Bottom", false), bVisible, true);
			}

			public override string Run()
			{
				string sRstCam = m_module.OpenCamera();
				if (sRstCam != "OK")
				{
					return sRstCam;
				}
				m_module.p_bStageVac = true;

				string sRst = "None";
				sRst = GrabEdge();
				if (sRst != "OK")
					return sRst;

				/*
				string sRst = "None";
				if (m_module.m_bGrabEdge)
				{
					sRst = GrabEdge();
					if (sRst != "OK")
						return sRst;
				}
				if (m_module.m_bGrabNotch)
				{
					sRst = GrabNotch();
					if (sRst != "OK")
						return sRst;
					//RootTools.ImageProcess.FocusStacking FS = new RootTools.ImageProcess.FocusStacking(m_module.m_memoryNotch.p_sz);
					//FS.SaveAll(m_module.m_memoryNotch.GetPtr());
					// FS.Run(0, m_module.m_memoryNotch.GetPtr());
				}
				if (m_module.m_bGrabAreaSide)
				{
					sRst = GrabAreaSide();
					if (sRst != "OK")
						return sRst;
				}
				*/
				return sRst;
			}

			private string GrabEdge()
			{
				Axis axisR = m_module.m_axisRotate;
				Axis axisEdgeX = m_module.m_axisEdgeSideX;

				try
				{
					m_gmTop.SetLight(true);

					double PulsePerDegree = m_module.dPulse360 / 360;
					double fCurr = axisR.p_posActual - axisR.p_posActual % m_module.dPulse360;
					double fTriggerStart = fCurr + m_fStartDegree * PulsePerDegree;
					double fTriggerDest = fTriggerStart + m_fScanDegree * PulsePerDegree;
					int dTrigger = 1;
					int nScanSpeed = Convert.ToInt32((double)m_nMaxFrame * dTrigger * (double)m_nScanRate / 100);
					double fMoveStart = fTriggerStart - m_fScanAcc * nScanSpeed;   //y 축 이동 시작 지점 
					double fMoveEnd = fTriggerDest + m_fScanAcc * nScanSpeed;  // Y 축 이동 끝 지점.
					int nGrabCount = Convert.ToInt32(m_fScanDegree * PulsePerDegree * m_module.dTriggerratio);

					axisEdgeX.StartMove(m_nSideFocusAxis);
					if (m_module.Run(axisEdgeX.WaitReady()))
						return p_sInfo;

					if (m_module.Run(axisR.StartMove(fMoveStart)))
						return p_sInfo;
					if (m_module.Run(axisR.WaitReady()))
						return p_sInfo;
					axisR.SetTrigger(fTriggerStart, fTriggerDest, dTrigger, true);

					m_gmTop.StartGrab(m_gmTop.m_memoryData, new CPoint(0, 0), nGrabCount);
					m_gmTop.Grabed += m_gmTop_Grabed;
					m_gmSide.StartGrab(m_gmSide.m_memoryData, new CPoint(0, 0), nGrabCount);
					m_gmBtm.StartGrab(m_gmBtm.m_memoryData, new CPoint(0, 0), nGrabCount);

					if (m_module.Run(axisR.StartMove(fMoveEnd, nScanSpeed, m_fScanAcc, m_fScanAcc)))
						return p_sInfo;
					if (m_module.Run(axisR.WaitReady()))
						return p_sInfo;

					/*
					gmBevelSide.StopGrab();
					//arr = "_focus_";
					//k_side = 0;

					//CalcAxisOffset();
					//RearrangeArray((int)m_fScanDegree);
					if (m_module.Run(axisR.StartMove(0)))
						return p_sInfo;
					if (m_module.Run(axisR.WaitReady()))
						return p_sInfo;

					axisEdgeX.StartMove((int)(m_module.EdgeXOfffset[0] * 1.7 * 10) - 26809 - 8868);
					if (m_module.Run(axisEdgeX.WaitReady()))
						return p_sInfo;

					gmBevelSide.StartGrab(m_module.m_memoryEdgeTop, new CPoint(4000, 0), nGrabCount);
					gmBevelSide.Grabed += gmBevelSide_Grabed;
					if (m_module.Run(axisR.StartMove(fMoveEnd, nScanSpeed, m_fScanAcc, m_fScanAcc)))
						return p_sInfo;

					//System.IO.StreamWriter sw = new System.IO.StreamWriter(new System.IO.FileStream(@"D:\axis.txt", System.IO.FileMode.Append));
					//sw.WriteLine(time);
					//sw.WriteLine("index , pixel , 축 좌표");
					int axis = 0;
					int nAxisDiff = 0;
					for (int i = 0; i < m_module.EdgeXOfffset.Count; i++)
					{
						axis = (int)(m_module.EdgeXOfffset[i] * 1.7 * 10);
						nAxisDiff = (axis - 26809) - 8868;
						//sw.WriteLine(i.ToString() + " , " + m_module.EdgeXOfffset[i].ToString() + " , " + nAxisDiff.ToString());

						axisEdgeX.StartMove(nAxisDiff);
						Thread.Sleep(7);
					}
					//sw.Close();

					if (m_module.Run(axisR.WaitReady()))
						return p_sInfo;
					*/

					axisR.RunTrigger(false);
					m_gmTop.StopGrab();
					m_gmSide.StopGrab();
					m_gmBtm.StopGrab();
					m_gmTop.SetLight(false);

					return "OK";
				}
				finally
				{
					axisR.RunTrigger(false);
					m_gmTop.SetLight(false);
				}
			}

			private void m_gmTop_Grabed(object sender, EventArgs e)
			{
				GrabedArgs ga = (GrabedArgs)e;
				int memW = Convert.ToInt32(ga.mdMemoryData.W);
				int memH = ga.mdMemoryData.p_sz.Y;

				IntPtr ptrSrc = (IntPtr)((long)ga.mdMemoryData.GetPtr() + ga.rtRoi.Left + ((long)memW * ga.rtRoi.Top));

				int nSobelSize = m_nSobelHeight;
				byte[] arrSrc = new byte[nSobelSize * memW];
				byte[] arrDst = new byte[nSobelSize * memW];
				Marshal.Copy(arrSrc, (memH / 2 - nSobelSize) * memW , ptrSrc, (memW * nSobelSize * 2));

				int nDerivativeX = 1;
				int nDerivativeY = 0;
				int nKernelSize = 5;
				int nScale = 1;
				int nDelta = 1;

				CLR_IP.Cpp_SobelEdgeDetection(arrSrc, arrDst, memW, memH, nDerivativeX, nDerivativeY, nKernelSize, nScale, nDelta);
				
				object o = new object();
				List<int> edgeDetect = new List<int>();
				edgeDetect.Clear();
				System.Threading.Tasks.Parallel.For(0, (nSobelSize * 2), i =>
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

				m_module.m_axisEdgeSideX.StartMove((int)(edgeDetect.Average() * m_nSideCamResoultion * 10) - m_nSideFocusAxis);
				if (m_module.Run(m_module.m_axisEdgeSideX.WaitReady()))
					return;
			}

			public void CalcSobelEdge(byte[] arrSobel, int nWidth, int nHeight, List<int> edgeDetect)
			{
				for (int i = 0; i < nWidth; i++)
				{
					if (arrSobel[i + (i * nHeight)] > m_nSobelThreshold)
					{
						int nGVCnt = 0;
						int nStartGV = i;
						while (arrSobel[i + (i * nHeight)] > m_nSobelThreshold)
						{
							nGVCnt++;
							i++;
						}
						if (nGVCnt > m_nSobelCnt)
						{
							edgeDetect.Add(nStartGV);
							break;
						}
					}
				}
			}

			/*
            public void CalcAxisOffset()
            {
                m_module.EdgeXOfffset.Clear();
                for (int n = 0; n < k_top; n++)
                {
                    String path = n.ToString() + ".bmp";
                    Emgu.CV.Mat matSrc = new Mat(@"D:\slot7\top\" + time + "_" + path, Emgu.CV.CvEnum.ImreadModes.Grayscale);
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
                                    //Emgu.CV.CvInvoke.Rectangle(matAbsGradX, new Rectangle(nStartGV, k, i - nStartGV, 30), color);
                                    break;
                                }
                            }
                        }
                    }

                    int nStartGVAvg = 0;
                    if (listPixelX.Count != 0)
                        nStartGVAvg = (int)listPixelX.Average();

                    int nLTAvg = 0;
                    int nWidthAvg = 0;
                    if (listRect.Count != 0)
                    {
                        nLTAvg = (int)listRect.Average(x => x.Left);
                        nWidthAvg = (int)listRect.Average(x => x.Width);
                        Emgu.CV.CvInvoke.Rectangle(matAbsGradX, new Rectangle(nLTAvg, 0, nWidthAvg, matAbsGradX.Height), color);
                        matAbsGradX.Save(@"D:\slot7\sobel\" + time + "_" + path);
                    }

                    if (nStartGVAvg <= 0)
                        m_module.EdgeXOfffset.Add(0);
                    else
                        m_module.EdgeXOfffset.Add(nStartGVAvg);
                }

                // 앞에 이미지 없는거 처리
                for (int i = 0; i < 4; i++)
                {
                    m_module.EdgeXOfffset[i] = m_module.EdgeXOfffset[4];
                }
            }

            string time = DateTime.Now.ToString("HH-mm-ss");
            public void RearrangeArray(int degree)
            {
                if (m_module.EdgeXOfffset.Count == 0)
                    return;

                try
                {
                    System.IO.StreamWriter sw = new System.IO.StreamWriter(new System.IO.FileStream(@"D:\axis_top.txt", System.IO.FileMode.Append));
                    sw.WriteLine(time);
                    sw.WriteLine("index , top rect left");
                    for (int i = 0; i < m_module.EdgeXOfffset.Count; i++)
                    {
                        sw.WriteLine(i.ToString() + " , " + m_module.EdgeXOfffset[i].ToString());
                    }
                    sw.Close();

                    int nExtraDegree = degree - 360;
                    int nCntPer45Degree = m_module.EdgeXOfffset.Count * 45 / degree;
                    int nCntExtraAdd = m_module.EdgeXOfffset.Count * nExtraDegree / degree;

                    List<int> insertList = new List<int>();
                    for (int i = m_module.EdgeXOfffset.Count - nCntPer45Degree - nCntExtraAdd; i < m_module.EdgeXOfffset.Count - nCntPer45Degree; i++)
                    {
                        insertList.Add(m_module.EdgeXOfffset[i]);
                    }
                    for (int i = 0; i < m_module.EdgeXOfffset.Count - nCntExtraAdd; i++)
                    {
                        insertList.Add(m_module.EdgeXOfffset[i]);
                    }

                    m_module.EdgeXOfffset.Clear();
                    m_module.EdgeXOfffset = insertList;

                    for (int i = 1; i < m_module.EdgeXOfffset.Count; i++)
                    {
                        if (Math.Abs(m_module.EdgeXOfffset[i - 1] - m_module.EdgeXOfffset[i]) > 2000)
                            m_module.EdgeXOfffset[i] = m_module.EdgeXOfffset[i - 1];
                    }
                }
                catch
                {

                }
            }
            */
		}

		public class Run_GrabNotch : ModuleRunBase
		{
			EdgeSideVision m_module;
			public GrabMode m_gmTop = null;
			public GrabMode m_gmSide = null;
			public GrabMode m_gmBtm = null;

			string _sGrabModeTop = "";
			string p_sGrabModeTop
			{
				get { return _sGrabModeTop; }
				set
				{
					_sGrabModeTop = value;
					m_gmTop = m_module.GetGrabMode(value);
				}
			}
			string _sGrabModeSide = "";
			string p_sGrabModeSide
			{
				get { return _sGrabModeSide; }
				set
				{
					_sGrabModeSide = value;
					m_gmSide = m_module.GetGrabMode(value);
				}
			}
			string _sGrabModeBtm = "";
			string p_sGrabModeBtm
			{
				get { return _sGrabModeBtm; }
				set
				{
					_sGrabModeBtm = value;
					m_gmBtm = m_module.GetGrabMode(value);
				}
			}

			public double m_fResAxisTop = 0.1; // um
			public double m_fScanAccTop = 0.1; // sec            
			public int m_nStackSpeedTop = 1000;
			public int m_nStackRangeTop = 1000; // um
			public int m_nStackStepTop = 100;
			public double m_fCalcedFpsTop = 0;
			public double m_fRealFpsTop = 0;

			public double m_fResAxisSide = 0.1; // um
			public double m_fScanAccSide = 0.1; // sec            
			public int m_nStackSpeedSide = 1000;
			public int m_nStackRangeSide = 1000; // um
			public int m_nStackStepSide = 100;
			public double m_fCalcedFpsSide = 0;
			public double m_fRealFpsSide = 0;

			public double m_fResAxisBottom = 0.1; // um
			public double m_fScanAccBottom = 0.1; // sec            
			public int m_nStackSpeedBottom = 1000;
			public int m_nStackRangeBottom = 1000; // um
			public int m_nStackStepBottom = 100;
			public double m_fCalcedFpsBottom = 0;
			public double m_fRealFpsBottom = 0;

			public int m_nPosOffsetEdge2Side = 0; // 45' // EdgeNotch R to Notch Side offset Pulse
			public int m_nPosOffsetSide2Top = -45000; // 45'
			public int m_nPosOffsetSide2Bottom = 45000; // 45'

			public int m_nStackStepRTop = 5000; // degree
			public int m_nStackAdditionalCountTop = 1; // 면 번에 걸쳐서 찍을거냐 1하면 중심 기준 양쪽 한번씩 더 0하면 가운데만
			public int m_nStackStepRBottom = 5000; // degree
			public int m_nStackAdditionalCountBottom = 1; // 면 번에 걸쳐서 찍을거냐 1하면 중심 기준 양쪽 한번씩 더 0하면 가운데만
			public int m_nStackStepRSide = 5000; // degree
			public int m_nStackAdditionalCountSide = 1; // 면 번에 걸쳐서 찍을거냐 1하면 중심 기준 양쪽 한번씩 더 0하면 가운데만
			public int m_nStackCenterOffsetXSide = 10000; // 사이드 가운데는 들어가서 찍어야되서

			public Run_GrabNotch(EdgeSideVision module)
			{
				m_module = module;
				InitModuleRun(module);
			}

			public override ModuleRunBase Clone()
			{
				Run_GrabNotch run = new Run_GrabNotch(m_module);
				run.p_sGrabModeTop = p_sGrabModeTop;
				run.p_sGrabModeSide = p_sGrabModeSide;
				run.p_sGrabModeBtm = p_sGrabModeBtm;

				run.m_fResAxisTop = m_fResAxisTop;
				run.m_fScanAccTop = m_fScanAccTop;
				run.m_nStackSpeedTop = m_nStackSpeedTop;
				run.m_nStackRangeTop = m_nStackRangeTop;
				run.m_nStackStepTop = m_nStackStepTop;
				run.m_fCalcedFpsTop = m_fCalcedFpsTop;
				run.m_fRealFpsTop = m_fRealFpsTop;

				run.m_fResAxisSide = m_fResAxisSide;
				run.m_fScanAccSide = m_fScanAccSide;
				run.m_nStackSpeedSide = m_nStackSpeedSide;
				run.m_nStackRangeSide = m_nStackRangeSide;
				run.m_nStackStepSide = m_nStackStepSide;
				run.m_fCalcedFpsSide = m_fCalcedFpsSide;
				run.m_fRealFpsSide = m_fRealFpsSide;

				run.m_fResAxisBottom = m_fResAxisBottom;
				run.m_fScanAccBottom = m_fScanAccBottom;
				run.m_nStackSpeedBottom = m_nStackSpeedBottom;
				run.m_nStackRangeBottom = m_nStackRangeBottom;
				run.m_nStackStepBottom = m_nStackStepBottom;
				run.m_fCalcedFpsBottom = m_fCalcedFpsBottom;
				run.m_fRealFpsBottom = m_fRealFpsBottom;

				run.m_nPosOffsetEdge2Side = m_nPosOffsetEdge2Side;
				run.m_nPosOffsetSide2Top = m_nPosOffsetSide2Top;
				run.m_nPosOffsetSide2Bottom = m_nPosOffsetSide2Bottom;

				run.m_nStackStepRTop = m_nStackStepRTop;
				run.m_nStackAdditionalCountTop = m_nStackAdditionalCountTop;
				run.m_nStackStepRBottom = m_nStackStepRBottom;
				run.m_nStackAdditionalCountBottom = m_nStackAdditionalCountBottom;
				run.m_nStackStepRSide = m_nStackStepRSide;
				run.m_nStackAdditionalCountSide = m_nStackAdditionalCountSide;
				run.m_nStackCenterOffsetXSide = m_nStackCenterOffsetXSide;

				return run;
			}

			public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
			{
				#region notch top
				{
					m_nStackRangeTop = (tree.GetTree("Notch Top", false, bVisible)).Set(m_nStackRangeTop, m_nStackRangeTop, "Stack Range", "Stack Range (um)", bVisible);
					m_nStackStepTop = (tree.GetTree("Notch Top", false, bVisible)).Set(m_nStackStepTop, m_nStackStepTop, "Stack Step", "Stack 간격 (um)", bVisible);
					m_nStackSpeedTop = (tree.GetTree("Notch Top", false, bVisible)).Set(m_nStackSpeedTop, m_nStackSpeedTop, "Stack Axis Speed", "Stack Axis Speed", bVisible);

					m_fCalcedFpsTop = m_nStackSpeedTop / (m_nStackStepTop / m_fResAxisTop);
					m_fCalcedFpsTop = (tree.GetTree("Notch Top", false, bVisible)).Set(m_fCalcedFpsTop, m_fCalcedFpsTop, "Calculated FPS", "계산된 FPS", bVisible, true);

					GrabMode gmTop = m_module.GetGrabMode("NotchTop");
					if (gmTop != null)
					{
						//m_fRealFpsTop = gmTop.GetFps();
					}
					m_fRealFpsTop = (tree.GetTree("Notch Top", false, bVisible)).Set(m_fRealFpsTop, m_fRealFpsTop, "Real FPS", "실제 FPS", bVisible, true);

					m_nStackStepRTop = ((tree.GetTree("Notch Top", false, bVisible).GetTree("Setting", false))).Set(m_nStackStepRTop, m_nStackStepRTop, "Stack Step R", "Pulse", bVisible);
					m_nPosOffsetSide2Top = ((tree.GetTree("Notch Top", false, bVisible).GetTree("Setting", false))).Set(m_nPosOffsetSide2Top, m_nPosOffsetSide2Top, "Side - Top Offset", "Pulse", bVisible);
					m_fResAxisTop = ((tree.GetTree("Notch Top", false, bVisible).GetTree("Setting", false))).Set(m_fResAxisTop, m_fResAxisTop, "Axis Res", "스택 축 해상도", bVisible);
					m_fScanAccTop = ((tree.GetTree("Notch Top", false, bVisible).GetTree("Setting", false))).Set(m_fScanAccTop, m_fScanAccTop, "Scan Acc", "스택 축 가속도 sec", bVisible);
				}
				#endregion

				#region notch side
				{
					m_nStackRangeSide = (tree.GetTree("Notch Side", false, bVisible)).Set(m_nStackRangeSide, m_nStackRangeSide, "Stack Range", "Stack Range (um)", bVisible);
					m_nStackStepSide = (tree.GetTree("Notch Side", false, bVisible)).Set(m_nStackStepSide, m_nStackStepSide, "Stack Step", "Stack 간격 (um)", bVisible);
					m_nStackSpeedSide = (tree.GetTree("Notch Side", false, bVisible)).Set(m_nStackSpeedSide, m_nStackSpeedSide, "Stack Axis Speed", "Stack Axis Speed", bVisible);

					m_fCalcedFpsSide = m_nStackSpeedSide / (m_nStackStepSide / m_fResAxisSide);
					m_fCalcedFpsSide = (tree.GetTree("Notch Side", false, bVisible)).Set(m_fCalcedFpsSide, m_fCalcedFpsSide, "Calculated FPS", "계산된 FPS", bVisible, true);

					GrabMode gmSide = m_module.GetGrabMode("NotchSide");
					if (gmSide != null)
					{
						//m_fRealFpsSide = gmSide.GetFps();
					}
					m_fRealFpsSide = (tree.GetTree("Notch Side", false, bVisible)).Set(m_fRealFpsSide, m_fRealFpsSide, "Real FPS", "실제 FPS", bVisible, true);

					m_nStackStepRSide = ((tree.GetTree("Notch Side", false, bVisible).GetTree("Setting", false))).Set(m_nStackStepRSide, m_nStackStepRSide, "Stack Step R", "Pulse", bVisible);
					m_nPosOffsetEdge2Side = ((tree.GetTree("Notch Side", false, bVisible).GetTree("Setting", false))).Set(m_nPosOffsetEdge2Side, m_nPosOffsetEdge2Side, "Edge Notch - Side Offset", "Pulse", bVisible);
					m_nStackCenterOffsetXSide = ((tree.GetTree("Notch Side", false, bVisible).GetTree("Setting", false))).Set(m_nStackCenterOffsetXSide, m_nStackCenterOffsetXSide, "Center Offset X Side", "Pulse", bVisible);
					m_fResAxisSide = ((tree.GetTree("Notch Side", false, bVisible).GetTree("Setting", false))).Set(m_fResAxisSide, m_fResAxisSide, "Axis Resulotion", "스택 축 해상도", bVisible);
					m_fScanAccSide = ((tree.GetTree("Notch Side", false, bVisible).GetTree("Setting", false))).Set(m_fScanAccSide, m_fScanAccSide, "Scan Acc", "스택 축 가속도 sec", bVisible);
				}
				#endregion

				#region notch bottom
				{
					m_nStackRangeBottom = (tree.GetTree("Notch Bottom", false, bVisible)).Set(m_nStackRangeBottom, m_nStackRangeBottom, "Stack Range", "Stack Range (um)", bVisible);
					m_nStackStepBottom = (tree.GetTree("Notch Bottom", false, bVisible)).Set(m_nStackStepBottom, m_nStackStepBottom, "Stack Step", "Stack 간격 (um)", bVisible);
					m_nStackSpeedBottom = (tree.GetTree("Notch Bottom", false, bVisible)).Set(m_nStackSpeedBottom, m_nStackSpeedBottom, "Stack Axis Speed", "Stack Axis Speed", bVisible);

					m_fCalcedFpsBottom = m_nStackSpeedBottom / (m_nStackStepBottom / m_fResAxisBottom);
					m_fCalcedFpsBottom = (tree.GetTree("Notch Bottom", false, bVisible)).Set(m_fCalcedFpsBottom, m_fCalcedFpsBottom, "Calculated FPS", "계산된 FPS", bVisible, true);

					GrabMode gmBottom = m_module.GetGrabMode("NotchBottom");
					if (gmBottom != null)
					{
						//m_fRealFpsBottom = gmBottom.GetFps();
					}
					m_fRealFpsBottom = (tree.GetTree("Notch Bottom", false, bVisible)).Set(m_fRealFpsBottom, m_fRealFpsBottom, "Real FPS", "실제 FPS", bVisible, true);

					m_nStackStepRBottom = ((tree.GetTree("Notch Bottom", false, bVisible).GetTree("Setting", false))).Set(m_nStackStepRBottom, m_nStackStepRBottom, "Stack Step R", "Pulse", bVisible);
					m_nPosOffsetSide2Bottom = ((tree.GetTree("Notch Bottom", false, bVisible).GetTree("Setting", false))).Set(m_nPosOffsetSide2Bottom, m_nPosOffsetSide2Bottom, "Side - Bottom Offset", "Pulse", bVisible);
					m_fResAxisBottom = ((tree.GetTree("Notch Bottom", false, bVisible).GetTree("Setting", false))).Set(m_fResAxisBottom, m_fResAxisBottom, "Axis Res", "스택 축 해상도", bVisible);
					m_fScanAccBottom = ((tree.GetTree("Notch Bottom", false, bVisible).GetTree("Setting", false))).Set(m_fScanAccBottom, m_fScanAccBottom, "Scan Acc", "스택 축 가속도 sec", bVisible);
				}
				#endregion

				p_sGrabModeTop = tree.Set(p_sGrabModeTop, p_sGrabModeTop, m_module.p_asGrabMode, "Grab Mode : Top", "Select GrabMode", bVisible);
				if (m_gmTop != null) m_gmTop.RunTree(tree.GetTree("Grab Mode : Top", false), bVisible, true);
				p_sGrabModeSide = tree.Set(p_sGrabModeSide, p_sGrabModeSide, m_module.p_asGrabMode, "Grab Mode : Side", "Select GrabMode", bVisible);
				if (m_gmSide != null) m_gmTop.RunTree(tree.GetTree("Grab Mode : Side", false), bVisible, true);
				p_sGrabModeBtm = tree.Set(p_sGrabModeBtm, p_sGrabModeBtm, m_module.p_asGrabMode, "Grab Mode : Bottom", "Select GrabMode", bVisible);
				if (m_gmBtm != null) m_gmTop.RunTree(tree.GetTree("Grab Mode : Bottom", false), bVisible, true);
			}

			public override string Run()
			{
				string sRstCam = m_module.OpenCamera();
				if (sRstCam != "OK")
				{
					return sRstCam;
				}
				m_module.p_bStageVac = true;

				string sRst = "None";
				sRst = GrabNotch();
				if (sRst != "OK")
					return sRst;

				return "OK";
			}

			private string GrabNotch()
			{
				/*
				Axis axisR = m_module.m_axisRotate;
				AxisXY axisXZ = m_module.m_axisXZ;

				GrabMode gmTop = m_module.GetGrabMode("NotchTop");
				GrabMode gmSide = m_module.GetGrabMode("NotchSide");
				GrabMode gmSideC = m_module.GetGrabMode("NotchSideCenter");
				GrabMode gmBottom = m_module.GetGrabMode("NotchBottom");
				//m_module.m_memoryNotch.Clear();

				gmTop.SetLight(true);
				MoveStackPos(m_nPosOffsetEdge2Side + m_nPosOffsetSide2Top - m_nStackStepRTop, "Position_1");
				StackProc(axisXZ.p_axisY, gmTop, m_module.m_memoryNotch, new CPoint(m_module.nBaslerX * 3, 0), m_nStackStepTop, m_nStackRangeTop, m_fResAxisTop, m_nStackSpeedTop, m_fScanAccTop);
				MoveStackPos(m_nPosOffsetEdge2Side + m_nPosOffsetSide2Top, "Position_1");
				StackProc(axisXZ.p_axisY, gmTop, m_module.m_memoryNotch, new CPoint(m_module.nBaslerX * 4, 0), m_nStackStepTop, m_nStackRangeTop, m_fResAxisTop, m_nStackSpeedTop, m_fScanAccTop);
				MoveStackPos(m_nPosOffsetEdge2Side + m_nPosOffsetSide2Top + m_nStackStepRTop, "Position_1");
				StackProc(axisXZ.p_axisY, gmTop, m_module.m_memoryNotch, new CPoint(m_module.nBaslerX * 5, 0), m_nStackStepTop, m_nStackRangeTop, m_fResAxisTop, m_nStackSpeedTop, m_fScanAccTop);
				gmTop.SetLight(false);

				gmSide.SetLight(true);
				MoveStackPos(m_nPosOffsetEdge2Side + m_nStackStepRSide, "Position_0");
				StackProc(axisXZ.p_axisX, gmSide, m_module.m_memoryNotch, new CPoint(0, 0), m_nStackStepSide, m_nStackRangeSide, m_fResAxisSide, m_nStackSpeedSide, m_fScanAccSide);
				gmSide.SetLight(false);
				gmSideC.SetLight(true);
				MoveStackPos(m_nPosOffsetEdge2Side, "Position_0", m_nStackCenterOffsetXSide);
				StackProc(axisXZ.p_axisX, gmSide, m_module.m_memoryNotch, new CPoint(m_module.nBaslerX, 0), m_nStackStepSide, m_nStackRangeSide, m_fResAxisSide, m_nStackSpeedSide, m_fScanAccSide);
				gmSideC.SetLight(false);
				gmSide.SetLight(true);
				MoveStackPos(m_nPosOffsetEdge2Side - m_nStackStepRSide, "Position_0");
				StackProc(axisXZ.p_axisX, gmSide, m_module.m_memoryNotch, new CPoint(m_module.nBaslerX * 2, 0), m_nStackStepSide, m_nStackRangeSide, m_fResAxisSide, m_nStackSpeedSide, m_fScanAccSide);
				gmSide.SetLight(false);

				gmBottom.SetLight(true);
				MoveStackPos(m_nPosOffsetEdge2Side + m_nPosOffsetSide2Bottom + m_nStackStepRBottom, "Position_2");
				StackProc(axisXZ.p_axisY, gmBottom, m_module.m_memoryNotch, new CPoint(m_module.nBaslerX * 6, 0), m_nStackStepBottom, m_nStackRangeBottom, m_fResAxisBottom, m_nStackSpeedBottom, m_fScanAccBottom);
				MoveStackPos(m_nPosOffsetEdge2Side + m_nPosOffsetSide2Bottom, "Position_2");
				StackProc(axisXZ.p_axisY, gmBottom, m_module.m_memoryNotch, new CPoint(m_module.nBaslerX * 7, 0), m_nStackStepBottom, m_nStackRangeBottom, m_fResAxisBottom, m_nStackSpeedBottom, m_fScanAccBottom);
				MoveStackPos(m_nPosOffsetEdge2Side + m_nPosOffsetSide2Bottom - m_nStackStepRBottom, "Position_2");
				StackProc(axisXZ.p_axisY, gmBottom, m_module.m_memoryNotch, new CPoint(m_module.nBaslerX * 8, 0), m_nStackStepBottom, m_nStackRangeBottom, m_fResAxisBottom, m_nStackSpeedBottom, m_fScanAccBottom);
				gmBottom.SetLight(false);
				*/
				return "OK";
			}

			private string StackProc(Axis a, GrabMode gm, MemoryData md, CPoint ImgOffset, int step, int range, double AxisRes, int AxisSpd, double AxisAcc) //step,range um
			{
				try
				{
					int cnt = range / step;

					step = (int)(step / AxisRes);
					range = (int)(range / AxisRes);

					double fCurr = a.p_posActual;
					double fAccRange = AxisSpd * AxisAcc;
					if (m_module.Run(a.StartMove(fCurr - fAccRange, AxisSpd, AxisAcc, AxisAcc)))
						return p_sInfo;
					if (m_module.Run(a.WaitReady()))
						return p_sInfo;

					a.SetTrigger(fCurr, fCurr + range, step, true); //, true, 4);
					gm.Grabed += gm_Grabed;
					gm.StartGrab(md, ImgOffset, cnt);
					if (m_module.Run(a.StartMove(fCurr + range, AxisSpd, AxisAcc, AxisAcc)))
						return p_sInfo;
					if (m_module.Run(a.WaitReady()))
						return p_sInfo;

					gm.StopGrab();
					a.RunTrigger(false); //ResetTrigger();

					return "OK";
				}
				finally
				{
				}
			}

			void gm_Grabed(object sender, EventArgs e)
			{
				GrabedArgs ga = (GrabedArgs)e;
				//    m_module.m_log.Info(ga.nFrameCnt.ToString());
				// ga.mdMemoryData.GetPtr()
			}

			private string MoveStackPos(int nR, string strXY, int nOffsetX = 0)
			{
				Axis axisR = m_module.m_axisRotate;
				AxisXY axisXZ = m_module.m_axisXZ;

				double fCurrR = axisR.p_posActual - axisR.p_posActual % m_module.dPulse360;

				if (m_module.Run(axisXZ.p_axisY.StartMove(strXY)))
					return p_sInfo;
				if (m_module.Run(axisR.StartMove(fCurrR + nR)))
					return p_sInfo;
				if (m_module.Run(axisXZ.WaitReady()))
					return p_sInfo;
				if (m_module.Run(axisR.WaitReady()))
					return p_sInfo;
				if (m_module.Run(axisXZ.p_axisX.StartMove(strXY)))
					return p_sInfo;
				if (m_module.Run(axisXZ.WaitReady()))
					return p_sInfo;
				if (nOffsetX != 0)
				{
					double fCurr = axisXZ.p_axisX.p_posActual + nOffsetX;
					if (m_module.Run(axisXZ.p_axisX.StartMove(fCurr)))
						return p_sInfo;
					if (m_module.Run(axisXZ.WaitReady()))
						return p_sInfo;
				}

				return "OK";
			}
		}

		public class Run_GrabAreaLine : ModuleRunBase
		{
			EdgeSideVision m_module;

			public double m_fStartDegreeAL = 0;
			public double m_fScanDegreeAL = 360;
			public int m_nMaxFrameAL = 100;
			public int m_nTriggerAL = 10;
			public double m_fScanAccAL = 0.3; //sec
			public double m_fDuleAL = 5.235988; // 1pulse 당 길이 300mm기준

			public Run_GrabAreaLine(EdgeSideVision module)
			{
				m_module = module;
				InitModuleRun(module);
			}

			public override ModuleRunBase Clone()
			{
				Run_GrabAreaLine run = new Run_GrabAreaLine(m_module);
				run.m_fStartDegreeAL = m_fStartDegreeAL;
				run.m_fScanDegreeAL = m_fScanDegreeAL;
				run.m_nMaxFrameAL = m_nMaxFrameAL;
				run.m_nTriggerAL = m_nTriggerAL;
				run.m_fScanAccAL = m_fScanAccAL;
				run.m_fDuleAL = m_fDuleAL;

				return run;
			}

			public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
			{
				m_fStartDegreeAL = tree.Set(m_fStartDegreeAL, m_fStartDegreeAL, "StartAngle", "Degree", bVisible);
				m_fScanDegreeAL = tree.Set(m_fScanDegreeAL, m_fScanDegreeAL, "ScanAngle", "Degree", bVisible);
				m_nMaxFrameAL = tree.Set(m_nMaxFrameAL, m_nMaxFrameAL, "Max Frame", "Camera Max Frame Spec", bVisible);
				m_nTriggerAL = tree.Set(m_nTriggerAL, m_nTriggerAL, "Trigger", "pulse", bVisible);
				m_fScanAccAL = tree.Set(m_fScanAccAL, m_fScanAccAL, "Scan Acc", "스캔 축 가속도 sec", bVisible);
				m_fDuleAL = tree.Set(m_fDuleAL, m_fDuleAL, "1펄스길이", "1pulse 당 둘레 길이", bVisible);
			}

			public override string Run()
			{
				string sRstCam = m_module.OpenCamera();
				if (sRstCam != "OK")
				{
					return sRstCam;
				}
				m_module.p_bStageVac = true;

				string sRst = "None";
				sRst = GrabAreaSide(); 
				if (sRst != "OK")
					return sRst;

				return "OK";
			}

			private string GrabAreaSide()
			{
				/*
				Axis axisR = m_module.m_axisRotate;
				AxisXY axisXZ = m_module.m_axisXZ;
				GrabMode gmTop = m_module.GetGrabMode("NotchTop");
				GrabMode gmSide = m_module.GetGrabMode("NotchSide");
				GrabMode gmBottom = m_module.GetGrabMode("NotchBottom");

				double PulsePerDegree = m_module.dPulse360 / 360;
				double fCurr = axisR.p_posActual - axisR.p_posActual % m_module.dPulse360;

				double fTriggerStart = fCurr + m_fStartDegreeAL * PulsePerDegree;
				double fTriggerDest = fTriggerStart + m_fScanDegreeAL * PulsePerDegree;
				int dTrigger = 1;
				int nScanSpeed = Convert.ToInt32((double)m_nMaxFrameAL * dTrigger);
				double fMoveStart = fTriggerStart - m_fScanAcc * nScanSpeed;   //y 축 이동 시작 지점 
				double fMoveEnd = fTriggerDest + m_fScanAcc * nScanSpeed;  // Y 축 이동 끝 지점.

				gmSide.SetLight(true);
				MoveStackPos(m_nPosOffsetEdge2Side, "Position_0");

				int nGrabCount = Convert.ToInt32(m_fScanDegree * PulsePerDegree);
				if (m_module.Run(axisR.StartMove(fMoveStart)))
					return p_sInfo;
				if (m_module.Run(axisR.WaitReady()))
					return p_sInfo;

				axisR.SetTrigger(fTriggerStart, fTriggerDest, dTrigger, true);
				gmSide.StartGrab(m_module.m_memoryEdgeTop, new CPoint(0, 0), nGrabCount);

				if (m_module.Run(axisR.StartMove(fMoveEnd, nScanSpeed, m_fScanAcc, m_fScanAcc)))
					return p_sInfo;
				if (m_module.Run(axisR.WaitReady()))
					return p_sInfo;

				axisR.RunTrigger(false);
				gmSide.StopGrab();

				gmSide.SetLight(false);
				*/
				return "OK";

			}
		}

		public class Run_Delay : ModuleRunBase
		{
			EdgeSideVision m_module;

			public Run_Delay(EdgeSideVision module)
			{
				m_module = module;
				InitModuleRun(module);
			}

			public override ModuleRunBase Clone()
			{
				Run_Delay run = new Run_Delay(m_module);
				return run;
			}

			public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
			{
			}

			public override string Run()
			{
				return "OK";
			}
		}

		public class Run_Rotate : ModuleRunBase
		{
			EdgeSideVision m_module;
			public Run_Rotate(EdgeSideVision module)
			{
				m_module = module;
				InitModuleRun(module);
			}

			double m_fDeg = 0;
			public override ModuleRunBase Clone()
			{
				Run_Rotate run = new Run_Rotate(m_module);
				run.m_fDeg = m_fDeg;
				return run;
			}

			public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
			{
				m_fDeg = tree.Set(m_fDeg, m_fDeg, "Degree", "Rotation Degree (0 ~ 360)", bVisible);
			}

			public override string Run()
			{
				int pulseRound = m_module.m_pulseRound;
				Axis axis = m_module.m_axisRotate;
				int pulse = (int)Math.Round(m_module.m_pulseRound * m_fDeg / 360);
				while (pulse < axis.p_posCommand) pulse += pulseRound;
				{
					axis.p_posCommand -= pulseRound;
					axis.p_posActual -= pulseRound;
				}
				if (m_module.Run(axis.StartMove(pulse))) return p_sInfo;
				return axis.WaitReady();
			}
		}

		#endregion

	}
}