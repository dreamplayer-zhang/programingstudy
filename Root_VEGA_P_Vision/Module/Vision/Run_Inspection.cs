using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools.ImageProcess;

namespace Root_VEGA_P_Vision.Module
{
    public class Run_Inspection:ModuleRunBase
    {
        Vision m_module;
        string recipeName;
        double radius, power;
        public GrabMode m_grabMode = null;
        string m_sGrabMode;

        public double Radius
        {
            get => radius;
            set => radius = value;
        }
        public double Power
        {
            get => power;
            set => power = value;
        }

        public string RecipeName
        {
            get => recipeName;
            set => recipeName = value;
        }

        public Run_Inspection(Vision vision)
        {
            m_module = vision;
            InitModuleRun(vision);
        }
        public override ModuleRunBase Clone()
        {
            Run_Inspection run = new Run_Inspection(m_module);
            run.Radius = Radius;
            run.Power = Power;
            return run;
        }
        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            Radius = tree.Set(Radius, Radius, "Radius", "radius", bVisible);
            Power = tree.Set(Power, Power, "Power", "Power", bVisible);
        }
        public unsafe override string Run()
        {
            MemoryData mem = m_module.m_mainOptic.GetMemoryData(InfoPod.ePod.EIP_Cover,Vision.MainOptic.eInsp.Stack,Vision.eUpDown.Front);

            int width = 4095;
            int height = 3000;
            VignetteFilter vignette = new VignetteFilter(width,height,Power,Radius); //필터만들어주는애
            vignette.GenerateGradient();

            byte* dstptr = (byte*)mem.GetPtr(1).ToPointer();
            byte* srcptr = (byte*)mem.GetPtr(0).ToPointer();

            //for (int cnt = 0; cnt < 10; cnt++)
            //{
            //    byte* ptr = (byte*)mem.GetPtr(cnt).ToPointer();

            //    for (int x = 0; x < 6; x++)
            //    {
            //        for (int y = 0; y < 7; y++)
            //        {
            //            for (int i = 0; i < height; i++)
            //            {
            //                for (int j = 0; j < width; j++)
            //                {
            //                    double* Maskptr = (double*)vignette.Mask.DataPointer.ToPointer();
            //                    int curY = height * y + i;
            //                    int curX = width * x + j;
            //                    int cur = curY * mem.p_sz.X + curX;
            //                    int maskcur = i * width + j;
            //                    double res = (ptr[cur] / Maskptr[maskcur]) > 255 ? 255 : (ptr[cur] / Maskptr[maskcur]);
            //                    ptr[cur] = (byte)res;
            //                }
            //            }
            //        }
            //    }

            //    mem.FileSave(@"C:\Stack\" + cnt + ".bmp", cnt, 1);
            //}


            FocusStacking_new fs = new FocusStacking_new(mem);

            for(int i=0;i<6;i++)
            {
                for(int j=0;j<7;j++)
                {
                    fs.Run(width, height,i,j);
                }
            }

            return "OK";
        }
    }
}
