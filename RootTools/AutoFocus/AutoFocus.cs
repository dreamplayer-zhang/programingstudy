using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools.AutoFocus
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
    }
}
