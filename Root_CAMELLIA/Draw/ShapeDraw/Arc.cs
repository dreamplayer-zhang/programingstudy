using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA.ShapeDraw
{
    public class Arc : IShapeManager<Arc>
    {
        #region Getter Setter

        public PointF[] Points { get; set; }
        #endregion

        #region Constructor
        public Arc()
        {
            Points = null;
        }

        public Arc(double dX, double dY, double dRadius, double dStartAngle, double dEndAngle, int nNum, bool isAbs)
        {
            double dStepAngle;
            if (isAbs == true)
            {
                dStepAngle = Math.Abs(dEndAngle - dStartAngle) / nNum;
            }
            else
            {
                dStepAngle = (dEndAngle - dStartAngle) / nNum;
            }
            Points = new PointF[nNum + 1];

            for (int i = 0; i <= nNum; i++)
            {
                Points[i].X = (float)(dX + dRadius * Math.Cos(dStartAngle + i * dStepAngle));
                Points[i].Y = (float)(dY + dRadius * Math.Sin(dStartAngle + i * dStepAngle));
            }
        }
        #endregion

        #region Method
        public void InitArc(double dX, double dY, double dRadius, double dStartAngle, double dEndAngle, int nNum, bool bAbs)
        {
            double dStepAngle;
            if (bAbs == true)
            {
                dStepAngle = Math.Abs(dEndAngle - dStartAngle) / nNum;
            }
            else
            {
                dStepAngle = (dEndAngle - dStartAngle) / nNum;
            }

            Points = new PointF[nNum + 1];

            for (int i = 0; i <= nNum; i++)
            {
                Points[i].X = (float)(dX + dRadius * Math.Cos(dStartAngle + i * dStepAngle));
                Points[i].Y = (float)(dY + dRadius * Math.Sin(dStartAngle + i * dStepAngle));
            }
        }

        public void Set(Arc arc)
        {
            int nNum = arc.Points.Count();
            Points = new PointF[nNum];

            for (int i = 0; i < nNum; i++)
            {
                Points[i].X = arc.Points[i].X;
                Points[i].Y = arc.Points[i].Y;
            }
        }

        public void Transform(double dScaleX, double dScaleY)
        {
            int nNum = Points.Count();

            for (int i = 0; i < nNum; i++)
            {
                Points[i].X *= (float)dScaleX;
                Points[i].Y *= (float)dScaleY;
            }
        }

        public void ScaleOffset(int nScale, int nOffsetX, int nOffsetY)
        {
            int nNum = Points.Count();

            for (int i = 0; i < nNum; i++)
            {
                Points[i].X = Points[i].X * nScale + nOffsetX;
                Points[i].Y = Points[i].Y * nScale + nOffsetY;
            }
        }

        public void ScaleOffset(double dScale, int nOffsetX, int nOffsetY)
        {
            int nNum = Points.Count();

            for (int i = 0; i < nNum; i++)
            {
                Points[i].X = Points[i].X * (float)dScale + nOffsetX;
                Points[i].Y = Points[i].Y * (float)dScale + nOffsetY;
            }
        }
        #endregion
    }
}
