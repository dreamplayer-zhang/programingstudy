using RootTools;
using RootTools.Memory;
using RootTools_CLR;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision
{
    public class Result
    {
        public CPoint Pos;
        public double Score;

        public Result(CPoint Pos, double Score)
        {
            this.Pos = Pos;
            this.Score = Score;
        }
    }
    public static class Calc
    {
        public static unsafe double GetAlignAngle(ImageData imageData,int nScore)
        {
            EUVPositionRecipe positionRecipe = GlobalObjects.Instance.Get<RecipeVision>().GetItem<EUVPositionRecipe>();
            
            byte* srcPtr = (byte*)imageData.GetPtr().ToPointer();

            List<Result> resli = new List<Result>();
            foreach (RecipeType_ImageData template in positionRecipe.EIPCoverTopFeature.ListAlignFeature)
            {
                int posX = 0, posY = 0; //results
                CPoint Abspt = new CPoint(template.PositionX, template.PositionY);
                double result = CLR_IP.Cpp_TemplateMatching(srcPtr, template.RawData, &posX, &posY,
                    imageData.p_Size.X, imageData.p_Size.Y, template.Width, template.Height, Abspt.X, Abspt.Y, Abspt.X + template.Width, Abspt.Y+template.Height, 5, 1, 0);

                if (result >= nScore)
                    resli.Insert(0, new Result(Abspt, result));
            }

            resli.Sort(CompareScore);
            return CalcAngle(resli[1].Pos, resli[0].Pos);
        }

        public static int CompareScore(Result res1, Result res2)
        {
            return Convert.ToInt32(res1.Pos.Y < res2.Pos.Y);
        }

        public static double CalcAngle(CPoint firstPos, CPoint secondPos)
        {
            double radian = Math.Atan2( secondPos.X - firstPos.X, secondPos.Y - firstPos.Y);
            double angle = radian * (180 / Math.PI);

            return angle;
        }
    }
}
