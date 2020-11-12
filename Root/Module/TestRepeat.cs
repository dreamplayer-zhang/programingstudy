using Root_ASIS.AOI;
using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;

namespace Root.Module
{
    public class TestRepeat : ModuleBase
    {
        #region ToolBox
        public MemoryPool m_memoryPool;
        CameraBasler m_cam;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory");
            p_sInfo = m_toolBox.Get(ref m_cam, this, "Camera");
            if (bInit)
            {
                if (m_memoryPool.p_gbPool < 1) m_memoryPool.p_gbPool = 1;
                m_cam.OnConnect += M_cam_OnConnect;
            }
        }

        private void M_cam_OnConnect()
        {
            m_memoryBasler.p_nByte = m_cam.p_nByte;
            m_memoryBasler.p_sz = m_cam.p_sz;
            m_cam.SetMemoryData(m_memoryBasler);
        }
        #endregion

        #region Memory
        MemoryGroup m_memoryGroup;
        MemoryData m_memoryBasler;
        public override void InitMemorys()
        {
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
            m_memoryBasler = m_memoryGroup.CreateMemory("Grab Basler", 1, m_cam.p_nByte, m_cam.p_sz);
            m_cam.SetMemoryData(m_memoryBasler);
        }
        #endregion

        #region ROI
        AOIData m_roi; 

        void InitROI()
        {
            m_roi = new AOIData("ROI", new CPoint(100, 100)); 
            m_memoryPool.m_viewer.OnLBD += M_viewer_OnLBD;
            m_memoryPool.m_viewer.OnMouseMove += M_viewer_OnMouseMove;
        }

        private void M_viewer_OnLBD(bool bDown, CPoint cpImg)
        {
            m_roi.LBD(bDown, cpImg);
            Draw(AOIData.eDraw.ROI);
        }

        private void M_viewer_OnMouseMove(CPoint cpImg)
        {
            m_roi.MouseMove(cpImg);
            Draw(AOIData.eDraw.ROI);
        }

        public void Draw(AOIData.eDraw eDraw)
        {
            if (m_memoryPool.m_viewer.p_memoryData == null) return; 
            MemoryDraw draw = m_memoryPool.m_viewer.p_memoryData.m_aDraw[0];
            draw.Clear();
            m_roi.Draw(draw, eDraw);
            draw.InvalidDraw();
        }
        #endregion

        #region Inspect
        Blob m_blob = new Blob();
        Blob.eSort m_eSort = Blob.eSort.Size;
        public CPoint m_mmGV = new CPoint(100, 0);
        public string Inspect(int nGV)
        {
            MemoryData memory = m_memoryPool.m_viewer.p_memoryData;
            if (memory == null) return "MemoryData not Assigned";
            m_blob.RunBlob(memory, 0, m_roi.p_cp0, m_roi.m_sz, nGV, 0, 10);
            m_blob.RunSort(m_eSort);
            if (m_blob.m_aSort.Count == 0) return "Find Fiducial Error";
            Blob.Island island = m_blob.m_aSort[0];
            m_roi.m_bInspect = true;
            m_roi.m_rpCenter = island.m_rpCenter;
            m_roi.m_sDisplay = "Size = " + island.m_nSize + ", " + island.m_sz.ToString();
            Draw(AOIData.eDraw.Inspect); 
            return "OK";
        }
        #endregion

        #region override
        public override void Reset()
        {
            base.Reset();
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            //
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate) return "OK";
            //
            p_eState = eState.Ready;
            return "OK";
        }
        #endregion

        public TestRepeat(string id, IEngineer engineer)
        {
            InitBase(id, engineer);
            InitROI(); 
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), false, "Time Delay");
            AddModuleRunList(new Run_AxisMove(this), false, "Axis Move");
            AddModuleRunList(new Run_GrabBasler(this), false, "Run Grab Basler Camera");
            AddModuleRunList(new Run_Inspect(this), false, "Inspect");
            AddModuleRunList(new Run_Repeat(this), false, "Repeat");
        }

        public class Run_Delay : ModuleRunBase
        {
            TestRepeat m_module;
            public Run_Delay(TestRepeat module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_secDelay = 2;
            public override ModuleRunBase Clone()
            {
                Run_Delay run = new Run_Delay(m_module);
                run.m_secDelay = m_secDelay;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Time Delay (sec)", bVisible);
            }

            public override string Run()
            {
                return "OK";
            }
        }

        public class Run_AxisMove : ModuleRunBase
        {
            TestRepeat m_module;
            public Run_AxisMove(TestRepeat module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public double m_pos = 0; 
            public override ModuleRunBase Clone()
            {
                Run_AxisMove run = new Run_AxisMove(m_module);
                run.m_pos = m_pos;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_pos = tree.Set(m_pos, m_pos, "Position", "Axis Position (pulse)", bVisible);
            }

            public override string Run()
            {
                if (EQ.p_bSimulate) return "OK";
                //
                return "OK";
            }
        }

        public class Run_GrabBasler : ModuleRunBase
        {
            TestRepeat m_module;
            public Run_GrabBasler(TestRepeat module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_GrabBasler run = new Run_GrabBasler(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                if (EQ.p_bSimulate) return "OK";
                if (m_module.Run(m_module.m_cam.GrabOne(0))) return p_sInfo;
                return "OK";
            }
        }

        public class Run_Inspect : ModuleRunBase
        {
            TestRepeat m_module;
            public Run_Inspect(TestRepeat module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            int m_nGV = 150;
            public override ModuleRunBase Clone()
            {
                Run_Inspect run = new Run_Inspect(m_module);
                run.m_nGV = m_nGV;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_nGV = tree.Set(m_nGV, m_nGV, "GV", "Gray Value (0 ~ 255)", bVisible);
            }

            public override string Run()
            {
                return m_module.Inspect(m_nGV);
            }
        }

        public class Run_Repeat : ModuleRunBase
        {
            TestRepeat m_module;
            public Run_Repeat(TestRepeat module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            int m_nRepeat = 10; 
            int m_nGV = 150;
            public override ModuleRunBase Clone()
            {
                Run_Repeat run = new Run_Repeat(m_module);
                run.m_nRepeat = m_nRepeat;
                run.m_nGV = m_nGV;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_nRepeat = tree.Set(m_nRepeat, m_nRepeat, "Repeat", "Repeat Count", bVisible);
                m_nGV = tree.Set(m_nGV, m_nGV, "GV", "Gray Value (0 ~ 255)", bVisible);
            }

            public override string Run()
            {
                if (EQ.p_bSimulate) return "OK";
                for (int n = 0; n < m_nRepeat; n++)
                {
                    //Axis Move; 
                    if (m_module.Run(m_module.m_cam.GrabOne(0))) return p_sInfo;
                    if (m_module.Run(m_module.Inspect(m_nGV))) return p_sInfo;
                }
                return "OK";
            }
        }
        #endregion
    }
}
