using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA.ShapeDraw
{
    class Line : IShapeManager<Line>
    {
        #region Getter Setter
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        #endregion

        #region Constructor
        public Line()
        {
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
        }

        public Line(double dX, double dY, double dWidth, double dHeight)
        {
            X = dX;
            Y = dY;
            Width = dWidth;
            Height = dHeight;
        }
        #endregion

        #region Method
        public void Set(Line line)
        {
            X = line.X;
            Y = line.Y;
            Width = line.Width;
            Height = line.Height;
        }

        public void Set(double dX, double dY, double dWidth, double dHeight)
        {
            X = dX;
            Y = dY;
            Width = dWidth;
            Height = dHeight;
        }

        public void Transform(double dScaleX, double dScaleY)
        {
            X *= dScaleX;
            Y *= dScaleY;
            Width *= dScaleX;
            Height *= dScaleY;
        }

        public void ScaleOffset(int nScale, int nOffsetX, int nOffsetY)
        {
            X = X * nScale + nOffsetX;
            Y = Y * nScale + nOffsetY;
            Width *= nScale;
            Height *= nScale;
        }

        public void ScaleOffset(double dScale, int nOffsetX, int nOffsetY)
        {
            X = X * dScale + nOffsetX;
            Y = Y * dScale + nOffsetY;
            Width *= dScale;
            Height *= dScale;
        }



        #endregion
    }
}
