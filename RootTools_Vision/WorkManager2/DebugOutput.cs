using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
#if DEBUG
    internal static class DebugOutput
    {

        private static string LL = " | ";

        public static void PrintWorkplaceBundle(WorkplaceBundle wb)
        {
            //int w = wb.SizeX;
            //int h = wb.SizeY;

            //int[,] map = new int[w, h];
            //foreach(Workplace wp in wb)
            //{
            //    if (wp.MapIndexX == -1 && wp.MapIndexY == -1) continue;
            //    map[wp.MapIndexX - 1, wp.MapIndexY - 1] = (int)wp.WorkState;
            //}
            //StringBuilder builder = new StringBuilder();
            
            //for(int i = 0; i < h; i++)
            //{
            //    for(int j =0; j < w; j++)
            //    {
            //        builder.Append(map[j, i]);
            //    }
            //    builder.Append("\n");
            //}

            //Debug.Write(builder.ToString());
        }


        public static void PrintWorkplaceBundleOffset(WorkplaceBundle wb)
        {
            int w = wb.SizeX;
            int h = wb.SizeY;

            string[,] map = new string[w, h];
            foreach (Workplace wp in wb)
            {
                if (wp.MapIndexX == -1 && wp.MapIndexY == -1) continue;
                map[wp.MapIndexX, wp.MapIndexY] = string.Format("[{0:D4},{1:D4}]", Math.Abs(wp.OffsetX), Math.Abs(wp.OffsetY));
            }
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    if (map[j, i] == null) 
                        builder.Append("[0000,0000]");
                    else
                        builder.Append(map[j, i]);
                }
                builder.Append("\n");
            }

            Debug.Write(builder.ToString());
            Debug.Write("\n\n");
        }

        public static void PrintWorkManagerInfo(WorkManager wm, string append = "")
        {
            string output = "[WorkManager]" + LL + wm.WorkType.ToString() + LL + wm.State.ToString() + LL + append;
            Debug.WriteLine(output);
        }

        public static void PrintWorkplaceInfo(Workplace wp, string append = "")
        {
            string output = "[Workplace(" + wp.MapIndexX +","+wp.MapIndexY+")]" + LL + wp.WorkState.ToString() + LL + wp.IsOccupied.ToString() + LL + append;
            Debug.WriteLine(output);
        }

        public static void PrintWorker(Worker wk, string append = "")
        {
            //if(wk.Workplace == null)
            //{
            //    Debug.WriteLine("[Worker]" + LL + wk.WorkType + "[" + wk.WorkerIndex + "]" + LL + "Workplace == null" + LL + append);
            //}
            //else
            //    Debug.WriteLine("[Worker]" + LL + wk.WorkType + "[" + wk.WorkerIndex + "]" + LL + wk.Workplace.MapIndexX + "," + wk.Workplace.MapIndexY + LL + append);
        }

        public static void PrintWork(WorkBase wk, string append = "")
        {
            if (wk.Workplace == null)
                Debug.WriteLine("[Work]" + LL + wk.p_sName + LL + "Workplace == null" + append);
            else
                Debug.WriteLine("[Work]" + LL + wk.p_sName + LL + wk.Workplace.MapIndexX + "," + wk.Workplace.MapIndexY + LL + append);
        }
    }
#endif
}
