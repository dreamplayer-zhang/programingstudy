using Emgu.CV.Structure;
using RootTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Point = System.Drawing.Point;

namespace RootTools.ImageProcess
{
    public enum eImageState { GRAY_SCALE, COLOR_RGB, COLOR_RGBA };
    public class AutoFocus
    {
        double m_dDistanceOfLeftPointToRightPoint;
        public double p_dDistanceOfLeftPointToRightPoint
        {
            get
            {
                return m_dDistanceOfLeftPointToRightPoint;
            }
            set
            {
                m_dDistanceOfLeftPointToRightPoint = value;
            }
        }
        double m_dDifferenceOfFocusDistance;
        public double p_dDifferenceOfFocusDistance
        {
            get
            {
                return m_dDifferenceOfFocusDistance;
            }
            set
            {
                m_dDifferenceOfFocusDistance = value;
            }
        }
        double m_dLeftPosMaxScore;
        public double p_dMaxScore
        {
            get
            {
                return m_dLeftPosMaxScore;
            }
            set
            {
                m_dLeftPosMaxScore = value;
            }
        }
        double m_dRightPosMaxScore;
        public double p_dRightPosMaxScore
        {
            get
            {
                return m_dRightPosMaxScore;
            }
            set
            {
                m_dRightPosMaxScore = value;
            }
        }
        double m_dLeftMaxPos;
        public double p_dLeftMaxPos
        {
            get
            {
                return m_dLeftMaxPos;
            }
            set
            {
                m_dLeftMaxPos = value;
            }
        }
        double m_dRightMaxPos;
        public double p_dRightMaxPos
        {
            get
            {
                return m_dRightMaxPos;
            }
            set
            {
                m_dRightMaxPos = value;
            }
        }


        eImageState m_eImageState = eImageState.GRAY_SCALE;
        public eImageState p_eImageState
        {
            get
            {
                return m_eImageState;
            }
            set
            {
                m_eImageState = value;
            }
        }

        public double GetThetaDegree(double dRadian)
        {
            return dRadian * (180 / Math.PI);
        }
        public double GetThetaRadian()
        {
            return Math.Atan2(m_dDifferenceOfFocusDistance, m_dDistanceOfLeftPointToRightPoint);
        }
        public double GetImageVarianceScore(ImageData img, int nVarianceSize)
        {
            // variable
            double dTemp = 0;
            int nTempCount = 0;
            int nWidth = img.p_Size.X / nVarianceSize;
            int nHeight = img.p_Size.Y / nVarianceSize;

            // implement
            for (int i = 0; i < nWidth; i++)
            {
                for (int j = 0; j < nHeight; j++)
                {
                    dTemp += GetImageLocalVariance(img, new Point(i * nVarianceSize, j * nVarianceSize), nVarianceSize);
                    nTempCount++;
                }
            }

            return dTemp / (double)nTempCount;
        }
        public double GetImageLocalVariance(ImageData img, Point pt, int nVarianceSize)
        {
            // variable
            double dTemp = 0.0;
            double dAvg = 0.0;
            int nVal = 0;

            // implement
            dAvg = GetImageLocalAverage(img, pt, nVarianceSize);

            unsafe
            {
                switch (p_eImageState)
                {
                    case eImageState.GRAY_SCALE:
                        {
                            byte* p = (byte*)img.GetPtr();
                            for (int i = 0; i < nVarianceSize; i++)
                            {
                                for (int j = 0; j < nVarianceSize; j++)
                                {
                                    nVal = p[((int)pt.Y + j) * img.p_Size.X + (int)pt.X + i];
                                    dTemp += (nVal - dAvg) * (nVal - dAvg);
                                }
                            }
                            break;
                        }
                    case eImageState.COLOR_RGB:
                        {
                            for (int i = 0; i < nVarianceSize; i++)
                            {
                                for (int j = 0; j < nVarianceSize; j++)
                                {
                                    nVal = GetRGBPixelVal(img, (int)pt.X + i, (int)pt.Y + j);
                                    dTemp += (nVal - dAvg) * (nVal - dAvg);
                                }
                            }
                            break;
                        }
                    case eImageState.COLOR_RGBA:
                        {
                            for (int i = 0; i < nVarianceSize; i++)
                            {
                                for (int j = 0; j < nVarianceSize; j++)
                                {
                                    nVal = GetRGBAPixelVal(img, (int)pt.X + i, (int)pt.Y + j);
                                    dTemp += (nVal - dAvg) * (nVal - dAvg);
                                }
                            }
                            break;
                        }
                }
            }

            return dTemp / (double)nVarianceSize / (double)nVarianceSize;
        }

        public double GetImageLocalAverage(ImageData img, Point pt, int nVarianceSize)
        {
            // variable
            double dTemp = 0.0;
            int nVal = 0;

            // implement
            unsafe
            {
                switch (p_eImageState)
                {
                    case eImageState.GRAY_SCALE:
                        {
                            byte* p = (byte*)img.GetPtr();
                            for (int i = 0; i < nVarianceSize; i++)
                            {
                                for (int j = 0; j < nVarianceSize; j++)
                                {
                                    dTemp += p[((int)pt.Y + j) * img.p_Size.X + (int)pt.X + i];
                                }
                            }
                            break;
                        }
                    case eImageState.COLOR_RGB:
                        {
                            for (int i = 0; i < nVarianceSize; i++)
                            {
                                for (int j = 0; j < nVarianceSize; j++)
                                {
                                    nVal = GetRGBPixelVal(img, (int)pt.X + i, (int)pt.Y + j);
                                    dTemp += nVal;
                                }
                            }
                            break;
                        }
                    case eImageState.COLOR_RGBA:
                        {
                            for (int i = 0; i < nVarianceSize; i++)
                            {
                                for (int j = 0; j < nVarianceSize; j++)
                                {
                                    nVal = GetRGBAPixelVal(img, (int)pt.X + i, (int)pt.Y + j);
                                    dTemp += nVal;
                                }
                            }
                            break;
                        }
                }
            }

            return dTemp / (double)nVarianceSize / (double)nVarianceSize;
        }

        public byte GetRGBPixelVal(ImageData img, int x, int y)
        {
            // variable
            int nWidth = img.p_Size.X;
            int r, g, b;

            // implement
            unsafe
            {
                byte* p = (byte*)img.GetPtr();
                r = *(p + y * nWidth * 3 + x * 3);
                g = *(p + y * nWidth * 3 + x * 3 + 1);
                b = *(p + y * nWidth * 3 + x * 3 + 2);
            }
            return (byte)((r + g + b) / 3);
        }
        public byte GetRGBAPixelVal(ImageData img, int x, int y)
        {
            // variable
            int nWidth = img.p_Size.X;
            int r, g, b;

            // implement
            unsafe
            {
                byte* p = (byte*)img.GetPtr();
                r = *(p + y * nWidth * 4 + x * 4);
                g = *(p + y * nWidth * 4 + x * 4 + 1);
                b = *(p + y * nWidth * 4 + x * 4 + 2);
            }
            return (byte)((r + g + b) / 3);
        }

        public double GetImageFocusScoreWithSobel(ImageData img, out Bitmap bmpOut)
        {
            Emgu.CV.Mat matSrc = new Emgu.CV.Mat(img.p_Size.X, img.p_Size.Y, Emgu.CV.CvEnum.DepthType.Cv8U, img.p_nByte, img.GetPtr(), (int)img.p_Stride);
            Emgu.CV.Mat matGrad = new Emgu.CV.Mat();

            int nScale = 1;
            int nDelta = 0;
            //int ddepth = (int)Emgu.CV.CvEnum.DepthType.Cv8U;
            Emgu.CV.Mat matGradX = new Emgu.CV.Mat();
            Emgu.CV.Mat matGradY = new Emgu.CV.Mat();
            Emgu.CV.Mat matAbsGradX = new Emgu.CV.Mat();
            Emgu.CV.Mat matAbsGradY = new Emgu.CV.Mat();
            ///Gradient X
            Emgu.CV.CvInvoke.Sobel(matSrc, matGradX, Emgu.CV.CvEnum.DepthType.Cv8U, 1, 0, 3, nScale, nDelta, Emgu.CV.CvEnum.BorderType.Default);
            ///Gradient Y
            Emgu.CV.CvInvoke.Sobel(matSrc, matGradY, Emgu.CV.CvEnum.DepthType.Cv8U, 0, 1, 3, nScale, nDelta, Emgu.CV.CvEnum.BorderType.Default);
            Emgu.CV.CvInvoke.ConvertScaleAbs(matGradX, matAbsGradX, nScale, nDelta);
            Emgu.CV.CvInvoke.ConvertScaleAbs(matGradY, matAbsGradY, nScale, nDelta);
            Emgu.CV.CvInvoke.AddWeighted(matAbsGradX, 0.5, matAbsGradY, 0.5, 0, matGrad);

            System.Drawing.Bitmap bmp = matGrad.Bitmap;
            bmpOut = bmp;

            Emgu.CV.Structure.MCvScalar mu = new Emgu.CV.Structure.MCvScalar();
            Emgu.CV.Structure.MCvScalar sigma = new Emgu.CV.Structure.MCvScalar();
            Emgu.CV.CvInvoke.MeanStdDev(matGrad, ref mu, ref sigma);
            double dFocusMeasure = mu.V0 * mu.V0;

            return dFocusMeasure;
        }
    }
}
