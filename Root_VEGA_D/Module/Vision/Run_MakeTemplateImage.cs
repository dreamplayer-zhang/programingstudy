using RootTools;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_D.Module
{
    public class Run_MakeTemplateImage : ModuleRunBase
    {
        Vision m_module;
        public CPoint m_cptTopAlignMarkCenterPos = new CPoint();
        public int m_nTopWidth = 500;
        public int m_nTopHeight = 500;
        public CPoint m_cptBottomAlignMarkCenterPos = new CPoint();
        public int m_nBottomWidth = 500;
        public int m_nBottomHeight = 500;

        public Run_MakeTemplateImage(Vision module)
        {
            m_module = module;
            InitModuleRun(module);
        }
        public override ModuleRunBase Clone()
        {
            Run_MakeTemplateImage run = new Run_MakeTemplateImage(m_module);

            run.m_cptTopAlignMarkCenterPos = m_cptTopAlignMarkCenterPos;
            run.m_nTopWidth = m_nTopWidth;
            run.m_nTopHeight = m_nTopHeight;
            run.m_cptBottomAlignMarkCenterPos = m_cptBottomAlignMarkCenterPos;
            run.m_nBottomWidth = m_nBottomWidth;
            run.m_nBottomHeight = m_nBottomHeight;

            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            m_cptTopAlignMarkCenterPos = tree.Set(m_cptTopAlignMarkCenterPos, m_cptTopAlignMarkCenterPos, "Top Align Mark Center Position", "Top Align Mark Center Position", bVisible);
            m_nTopWidth = tree.Set(m_nTopWidth, m_nTopWidth, "Top Align Mark Width", "Top Align Mark Width", bVisible);
            m_nTopHeight = tree.Set(m_nTopHeight, m_nTopHeight, "Top Align Mark Height", "Top Align Mark Height", bVisible);
            m_cptBottomAlignMarkCenterPos = tree.Set(m_cptBottomAlignMarkCenterPos, m_cptBottomAlignMarkCenterPos, "Bottom Align Mark Center Position", "Bottom Align Mark Center Position", bVisible);
            m_nBottomWidth = tree.Set(m_nBottomWidth, m_nBottomWidth, "Bottom Align Mark Width", "Bottom Align Mark Width", bVisible);
            m_nBottomHeight = tree.Set(m_nBottomHeight, m_nBottomHeight, "Bottom Align Mark Height", "Bottom Align Mark Height", bVisible);
            m_nBottomHeight = tree.Set(m_nBottomHeight, m_nBottomHeight, "Bottom Align Mark Height", "Bottom Align Mark Height", bVisible);
        }

        public override string Run()
        {
            // variable
            string strAlignMarkTemplateImagePath = "D:\\AlignMarkTemplateImage";
            string strPool = "Vision.Memory";
            string strGroup = "Vision";
            string strMemory = "Main";
            MemoryData mem = m_module.m_engineer.GetMemory(strPool, strGroup, strMemory);

            // implement
            if (!Directory.Exists(strAlignMarkTemplateImagePath))
                Directory.CreateDirectory(strAlignMarkTemplateImagePath);

            CRect crtTopAlignMarkROI = new CRect(m_cptTopAlignMarkCenterPos, m_nTopWidth, m_nTopHeight);
            CRect crtBottomAlignMarkROI = new CRect(m_cptBottomAlignMarkCenterPos, m_nBottomWidth, m_nBottomHeight);

            m_module.GetGrayByteImageFromMemory_12bit(mem, crtTopAlignMarkROI).Save(Path.Combine(strAlignMarkTemplateImagePath, "TopTemplateImage.bmp"));
            m_module.GetGrayByteImageFromMemory_12bit(mem, crtBottomAlignMarkROI).Save(Path.Combine(strAlignMarkTemplateImagePath, "BottomTemplateImage.bmp"));

            return "OK";
        }
    }
}
