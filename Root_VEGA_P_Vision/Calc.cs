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
        public static unsafe double GetAlignAngle(ImageData imageData,int nScore,int trigger)
        {
            EUVPositionRecipe positionRecipe = GlobalObjects.Instance.Get<RecipeCoverFront>().GetItem<EUVPositionRecipe>();
            
            byte* srcPtr = (byte*)imageData.GetPtr().ToPointer();

            List<Result> resli = new List<Result>();
            foreach (RecipeType_ImageData template in positionRecipe.PartsFeatureList.ListAlignFeature)
            {
                int posX = 0, posY = 0; //results
                int left = template.PositionX - trigger;
                int top = template.PositionY - trigger;
                int right = template.PositionX + template.Width + trigger;
                int bottom = template.PositionY + template.Height + trigger;
                double result = CLR_IP.Cpp_TemplateMatching(srcPtr, template.RawData, &posX, &posY,
                    imageData.p_Size.X, imageData.p_Size.Y, template.Width, template.Height,left,top,right,bottom, 5, 1, 0);
                
                //if (result >= nScore)
                    resli.Insert(0, new Result(new CPoint(template.PositionX + posX, template.PositionY + posY), result));
            }

            if (resli.Count >= 2)
            { 
                resli.Sort(CompareScore);
                return CalcAngle(resli[1].Pos, resli[0].Pos);
            }
            else
            {
                return double.MinValue;
            }
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
