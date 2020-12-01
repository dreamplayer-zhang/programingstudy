using RootTools;
using RootTools.Camera.Matrox;
using RootTools.Control;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Root_WIND2.Module
{
	public class EBRVision : ModuleBase
	{
		#region ToolBox
		private DIO_O doVac;
		private Axis axisX;
		private Axis axisZ;
		private Axis axisRotate;
		private Camera_Matrox camEBR;
		private LightSet lightSet;
		private MemoryPool memoryPool;
		private MemoryGroup memoryGroup;
		private MemoryData memoryMain;

		public override void GetTools(bool bInit)
		{
			p_sInfo = m_toolBox.Get(ref doVac, this, "Stage Vaccume");
			p_sInfo = m_toolBox.Get(ref axisX, this, "Axis X");
			p_sInfo = m_toolBox.Get(ref axisZ, this, "Axis Z");
			p_sInfo = m_toolBox.Get(ref axisRotate, this, "Axis Rotate");
			p_sInfo = m_toolBox.Get(ref camEBR, this, "Cam EBR");
			p_sInfo = m_toolBox.Get(ref lightSet, this);
			p_sInfo = m_toolBox.Get(ref memoryPool, this, "Memory", 1);
		}
		#endregion

		#region DIO
		public bool StageVac
		{
			get { return doVac.p_bOut; }
			set
			{
				if (doVac.p_bOut == value) return;
				doVac.Write(value);
			}
		}
		#endregion

		#region Axis

		#endregion

		#region State Home
		public override string StateHome()
		{
			if (EQ.p_bSimulate)
				return "OK";

			OpenCamera();

			axisZ.StartHome();
			if (axisZ.WaitReady() != "OK")
			{
				p_eState = eState.Error;
				return "Home Stop";
			}

			Thread.Sleep(200);
			StageVac = true;

			axisX.StartHome();
			if (axisX.WaitReady() != "OK")
			{
				StageVac = false;
				p_eState = eState.Error;
				return "Home Stop";
			}

			Thread.Sleep(200);
			axisRotate.StartHome();
			if (axisRotate.WaitReady() != "OK")
			{
				StageVac = false;
				p_eState = eState.Error;
				return "Home Stop";
			}

			p_eState = eState.Ready;
			return "OK";
		}
		
		public string OpenCamera()
		{
			if (camEBR.p_CamInfo.p_eState == eCamState.Init)
				camEBR.Connect();
			return "OK";
		}
		#endregion

		#region Tree
		public override void RunTree(Tree tree)
		{
			base.RunTree(tree);
			RunTreeGrabMode(tree.GetTree("Grab Mode", false));
		}
		#endregion

		#region GrabMode
		private int grabModeCnt = 0;
		public ObservableCollection<GrabMode> GrabMode = new ObservableCollection<GrabMode>();
		public List<string> GrabModeName
		{
			get
			{
				List<string> GrabModeName = new List<string>();
				foreach (GrabMode grabMode in GrabMode)
					GrabModeName.Add(grabMode.p_sName);
				return GrabModeName;
			}
		}

		public GrabMode GetGrabMode(string grabModeName)
		{
			foreach (GrabMode grabMode in GrabMode)
			{
				if (grabModeName == grabMode.p_sName)
					return grabMode;
			}
			return null;
		}

		private void RunTreeGrabMode(Tree tree)
		{
			grabModeCnt = tree.Set(grabModeCnt, grabModeCnt, "Count", "Grab Mode Count");
			while (GrabMode.Count < grabModeCnt)
			{
				string id = "Mode." + GrabMode.Count.ToString("00");
				GrabMode grabMode = new GrabMode(id, m_cameraSet, lightSet, memoryPool);
				GrabMode.Add(grabMode);
			}
			while (GrabMode.Count > grabModeCnt)
				GrabMode.RemoveAt(GrabMode.Count - 1);
			foreach (GrabMode grabMode in GrabMode)
				grabMode.RunTreeName(tree.GetTree("Name", false));
			foreach (GrabMode grabMode in GrabMode)
				grabMode.RunTree(tree.GetTree(grabMode.p_sName, false), true, false);
		}
		#endregion
		
		#region override
		public override void Reset()
		{
			base.Reset();
		}

		public override void ThreadStop()
		{
			base.ThreadStop();
		}

		public override void InitMemorys()
		{
			memoryGroup = memoryPool.GetGroup(p_id);
			memoryMain = memoryGroup.CreateMemory("Main", 1, 1, 1000, 1000);
		}

		protected override void InitModuleRuns()
		{
			AddModuleRunList(new Run_Grab(this), false, "Run Grab");
		}
		#endregion

		public EBRVision(string id, IEngineer engineer)
		{
			base.InitBase(id, engineer);
			InitMemorys();
		}

		public class Run_Grab : ModuleRunBase
		{
			private EBRVision module;

			public Run_Grab(EBRVision module)
			{
				this.module = module;
				InitModuleRun(module);
			}

			public override ModuleRunBase Clone()
			{
				Run_Grab run = new Run_Grab(module);
				return run;
			}

			public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
			{
			}

			public override string Run()
			{
				string resultCam = module.OpenCamera();
				if (resultCam != "OK")
					return resultCam;

				module.StageVac = true;
				string result = "None";
				result = Grab();
				return result;
			}

			private string Grab()
			{
				Axis axisX = module.axisX;
				Axis axisZ = module.axisZ;
				Axis axisTheta = module.axisRotate;

				try
				{
					return "OK";
				}
				finally
				{
					
				}
			}
		}
	}
}
