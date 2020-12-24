using RootTools;
using RootTools.Control;
using RootTools.Camera.Dalsa;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RootTools.Camera.Matrox;
using Root_EFEM.Module;
using Root_EFEM;

namespace Root_WIND2.Module
{
	public class EdgeSideVision : ModuleBase, IWTRChild
	{
		#region ToolBox
		Axis axisRotate;
		Axis axisEdgeX;
		AxisXY axisXZ;
		DIO_O doVac;
		DIO_O doBlow;

		MemoryPool memoryPool;
		MemoryGroup memoryGroup;
		MemoryData memoryEdgeTop;
		MemoryData memoryEdgeSide;
		MemoryData memoryEdgeBtm;
		MemoryData memoryEBR;

		LightSet lightSet;
		Camera_Dalsa camEdgeTop;
		Camera_Dalsa camEdgeSide;
		Camera_Dalsa camEdgeBtm;
		Camera_Matrox camEBR;

		#region Getter/Setter
		public Axis AxisRotate { get => axisRotate; private set => axisRotate = value; }
		public Axis AxisEdgeX { get => axisEdgeX; private set => axisEdgeX = value; }
		public AxisXY AxisXZ { get => axisXZ; private set => axisXZ = value; }
		public DIO_O DoVac { get => doVac; private set => doVac = value; }
		public DIO_O DoBlow { get => doBlow; private set => doBlow = value; }
		public MemoryPool MemoryPool { get => memoryPool; private set => memoryPool = value; }
		public MemoryGroup MemoryGroup { get => memoryGroup; private set => memoryGroup = value; }
		public MemoryData MemoryEdgeTop { get => memoryEdgeTop; private set => memoryEdgeTop = value; }
		public MemoryData MemoryEdgeSide { get => memoryEdgeSide; private set => memoryEdgeSide = value; }
		public MemoryData MemoryEdgeBtm { get => memoryEdgeBtm; private set => memoryEdgeBtm = value; }
		public MemoryData MemoryEBR { get => memoryEBR; private set => memoryEBR = value; }
		public LightSet LightSet { get => lightSet; private set => lightSet = value; }
		public Camera_Dalsa CamEdgeTop { get => camEdgeTop; private set => camEdgeTop = value; }
		public Camera_Dalsa CamEdgeSide { get => camEdgeSide; private set => camEdgeSide = value; }
		public Camera_Dalsa CamEdgeBtm { get => camEdgeBtm; private set => camEdgeBtm = value; }
		public Camera_Matrox CamEBR { get => CamEBR; private set => CamEBR = value; }
		#endregion

		public override void GetTools(bool bInit)
		{
			p_sInfo = m_toolBox.Get(ref axisRotate, this, "Axis Rotate");
			p_sInfo = m_toolBox.Get(ref axisEdgeX, this, "Axis EdgeSideX");
			p_sInfo = m_toolBox.Get(ref axisXZ, this, "Axis NotchXZ");
			p_sInfo = m_toolBox.Get(ref doVac, this, "Stage Vacuum");
			p_sInfo = m_toolBox.Get(ref doBlow, this, "Stage Blow");
			p_sInfo = m_toolBox.Get(ref memoryPool, this, "Memory", 1);
			p_sInfo = m_toolBox.Get(ref camEdgeTop, this, "Cam EdgeTop");
			p_sInfo = m_toolBox.Get(ref camEdgeSide, this, "Cam EdgeSide");
			p_sInfo = m_toolBox.Get(ref camEdgeBtm, this, "Cam EdgeBottom");
            p_sInfo = m_toolBox.Get(ref camEBR, this, "Cam EBR");
            p_sInfo = m_toolBox.Get(ref lightSet, this);
			memoryGroup = memoryPool.GetGroup(p_id);
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
				GrabMode grabMode = new GrabMode(id, m_cameraSet, lightSet, memoryPool);
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

		#region DIO
		public bool p_bStageVac
		{
			get { return doVac.p_bOut; }
			set
			{
				if (doVac.p_bOut == value) return;
				doVac.Write(value);
			}
		}

		public bool p_bStageBlow
		{
			get { return doBlow.p_bOut; }
			set
			{
				if (doBlow.p_bOut == value) return;
				doBlow.Write(value);
			}
		}

		public void RunBlow(int msDelay)
		{
			doBlow.DelayOff(msDelay);
		}
		#endregion

		#region override
		public override void Reset()
		{
			base.Reset();
		}

		double pulse360 = 360000;
		public double Pulse360 { get => pulse360; set => pulse360 = value; }
		double triggerRatio = 1.5; //캠익에서 트리거 분주비
		public double TriggerRatio { get => triggerRatio; set => triggerRatio = value; }
		double margin = 36000;
		public double Margin { get => margin; set => margin = value; }
		public override void InitMemorys()
		{
			int nImageY = (int)(pulse360 * triggerRatio + margin);
			int nImageX = 3000;
			memoryGroup = memoryPool.GetGroup(p_id);
			memoryEdgeTop = memoryPool.GetGroup(p_id).CreateMemory(EDGE_TYPE.EdgeTop.ToString(), 1, 1, nImageX, nImageY);
			memoryEdgeSide = memoryPool.GetGroup(p_id).CreateMemory(EDGE_TYPE.EdgeSide.ToString(), 1, 1, nImageX, nImageY);
			memoryEdgeBtm = memoryPool.GetGroup(p_id).CreateMemory(EDGE_TYPE.EdgeBottom.ToString(), 1, 1, nImageX, nImageY);
			memoryEBR = memoryPool.GetGroup(p_id).CreateMemory(EDGE_TYPE.EBR.ToString(), 1, 1, nImageX, nImageY);
		}
		#endregion

		#region Axis
		private int pulseRound = 1000;
		public int PulseRound { get => pulseRound; set => pulseRound = value; }
		void RunTreeAxis(Tree tree)
		{
			pulseRound = tree.Set(pulseRound, pulseRound, "Rotate Pulse / Round", "Rotate Axis Pulse / 1 Round (pulse)");
		}
		#endregion

		#region IWTRChild
		bool _bLock = false;
		public bool p_bLock
		{
			get
			{
				return _bLock;
			}
			set
			{
				if (_bLock == value)
					return;
				_bLock = value;
			}
		}

		bool IsLock()
		{
			for (int n = 0; n < 10; n++)
			{
				if (p_bLock == false)
					return false;
				Thread.Sleep(100);
			}
			return true;
		}

		public List<string> p_asChildSlot
		{
			get
			{
				return null;
			}
		}

		public InfoWafer GetInfoWafer(int nID)
		{
			return p_infoWafer;
		}

		public void SetInfoWafer(int nID, InfoWafer infoWafer)
		{
			p_infoWafer = infoWafer;
		}

		public string IsGetOK(int nID)
		{
			if (p_eState != eState.Ready)
				return p_id + " eState not Ready";
			if (p_infoWafer == null)
				return p_id + " IsGetOK - InfoWafer not Exist";
			return "OK";
		}

		public string IsPutOK(InfoWafer infoWafer, int nID)
		{
			if (p_eState != eState.Ready)
				return p_id + " eState not Ready";
			if (p_infoWafer != null)
				return p_id + " IsPutOK - InfoWafer Exist";
			if (m_waferSize.GetData(infoWafer.p_eSize).m_bEnable == false)
				return p_id + " not Enable Wafer Size";
			return "OK";
		}

		public int GetTeachWTR(InfoWafer infoWafer = null)
		{
			if (infoWafer == null)
				infoWafer = p_infoWafer;
			return m_waferSize.GetData(infoWafer.p_eSize).m_teachWTR;
		}

		public string BeforeGet(int nID)
		{
			//            string info = MoveReadyPos();
			//            if (info != "OK") return info;
			return "OK";
		}

		public string BeforePut(int nID)
		{
			//            string info = MoveReadyPos();
			//            if (info != "OK") return info;
			return "OK";
		}

		public string AfterGet(int nID)
		{
			return "OK";
		}

		public string AfterPut(int nID)
		{
			return "OK";
		}

		enum eCheckWafer
		{
			InfoWafer,
			Sensor
		}
		eCheckWafer m_eCheckWafer = eCheckWafer.InfoWafer;
		public bool IsWaferExist(int nID)
		{
			switch (m_eCheckWafer)
			{
				case eCheckWafer.Sensor: return false; // m_diWaferExist.p_bIn;
				default: return (p_infoWafer != null);
			}
		}

		InfoWafer.WaferSize m_waferSize;
		public void RunTreeTeach(Tree tree)
		{
			m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
		}

		string m_sInfoWafer = "";
		InfoWafer _infoWafer = null;
		public InfoWafer p_infoWafer
		{
			get
			{
				return _infoWafer;
			}
			set
			{
				m_sInfoWafer = (value == null) ? "" : value.p_id;
				_infoWafer = value;
				if (m_reg != null)
					m_reg.Write("sInfoWafer", m_sInfoWafer);
				OnPropertyChanged();
			}
		}

		Registry m_reg = null;
		public void ReadInfoWafer_Registry()
		{
			m_reg = new Registry(p_id + ".InfoWafer");
			m_sInfoWafer = m_reg.Read("sInfoWafer", m_sInfoWafer);
			p_infoWafer = m_engineer.ClassHandler().GetGemSlot(m_sInfoWafer);
		}
		#endregion

		#region State Home
		public string OpenCamera()
		{
			if (camEdgeTop.p_CamInfo.p_eState == RootTools.Camera.Dalsa.eCamState.Init)
				camEdgeTop.Connect();
			if (camEdgeSide.p_CamInfo.p_eState == RootTools.Camera.Dalsa.eCamState.Init)
				camEdgeSide.Connect();
			if (camEdgeBtm.p_CamInfo.p_eState == RootTools.Camera.Dalsa.eCamState.Init)
				camEdgeBtm.Connect();
			if (camEBR.p_CamInfo.p_eState == RootTools.Camera.Matrox.eCamState.Init)
				camEBR.Connect();
			return "OK";
		}

		public override string StateHome()
		{
			if (EQ.p_bSimulate) return "OK";
			//            p_bStageBlow = false;
			//            p_bStageVac = true;

			OpenCamera();
			p_bStageVac = true;
			axisEdgeX.StartHome();
			if (axisEdgeX.WaitReady() != "OK")
			{
				p_bStageVac = false;
				p_eState = eState.Error;
				return "OK";
			}

			Thread.Sleep(200);
			axisRotate.StartHome();
			if (axisRotate.WaitReady() == "OK")
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

		#region Tree
		public override void RunTree(Tree tree)
		{
			base.RunTree(tree);
			m_eCheckWafer = (eCheckWafer)tree.Set(m_eCheckWafer, m_eCheckWafer, "CheckWafer", "CheckWafer");

			RunTreeGrabMode(tree.GetTree("Grab Mode", false));
		}
		#endregion

		public enum EDGE_TYPE
		{
			EdgeTop,
			EdgeSide,
			EdgeBottom,
			EBR,
		}

		public EdgeSideVision(string id, IEngineer engineer)
		{ 
			base.InitBase(id, engineer);
			m_waferSize = new InfoWafer.WaferSize(id, false, false);
		}

		
		public override void ThreadStop()
		{
			base.ThreadStop();
		}

		#region ModuleRun
		protected override void InitModuleRuns()
		{
			AddModuleRunList(new Run_GrabEdge(this), false, "Run Grab Edge");
			AddModuleRunList(new Run_GrabEBR(this), false, "Run Grab EBR");
			//AddModuleRunList(new Run_GrabNotch(this), false, "Run Grab Notch");
			//AddModuleRunList(new Run_GrabAreaLine(this), false, "Run Grab Area Line");
		}

		public ImageData GetMemoryData(EDGE_TYPE data)
		{
			ImageData result = new ImageData(memoryPool.GetMemory(p_id, data.ToString()));
			return result;
		}
		#endregion

		/* TO-DO 옮길거임
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
				Axis axisR = m_module.axisRotate;
				AxisXY axisXZ = m_module.axisXZ;

				double fCurrR = axisR.p_posActual - axisR.p_posActual % m_module.pulse360;

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
				return "OK";
			}
		}
		*/
	}
}
