using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.ExceptionServices;
using System.Security;
namespace Root_CAMELLIA.LibSR_Met
{
    public class ImageProcess
    {
        //public ImageProcess()
        //{

        //}
        public static Bitmap CropBitmap(Bitmap bm, Rectangle rt)
        {
            if (bm == null)
            {
                return null;
            }

            Bitmap bitmap = new Bitmap(rt.Width, rt.Height);

            for (int i = 0; i < rt.Width; i++)
            {
                for (int j = 0; j < rt.Height; j++)
                {
                    try
                    {
                        bitmap.SetPixel(i, j, bm.GetPixel(i + rt.Left, j + rt.Top));
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        return bitmap;
                    }
                }
            }
            return bitmap;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        unsafe public static Mat RGBToGray(Mat src)
        {
            try
            {
                Mat result = new Mat(src.Rows, src.Cols, MatType.CV_8U);
                Cv2.CvtColor(src, result, ColorConversion.RgbaToGray);

                return result;
            }
            catch (AccessViolationException)
            {
                return null;
            }
        }

        //public static List<System.Drawing.Point> FindPoint = new List<System.Drawing.Point>();

        private static void CheckPointRange(Mat image, ref int x, ref int y)
        {
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x >= image.Width) x = image.Width - 1;
            if (y >= image.Height) y = image.Height - 1;
        }

        public static void GetEdgePoint(Mat image, PointF startPoint, PointF vector, out System.Drawing.Point edgePoint, int SearchRange, int SearchLevel)
        {
            edgePoint = new System.Drawing.Point();

            double prox = TargetGV(image, startPoint, vector, SearchRange, SearchLevel);
            if (prox < 10)
            {
                edgePoint = new System.Drawing.Point();
                return;
            }
            edgePoint = GetEdgePoint(image, startPoint, vector, prox, SearchRange, SearchLevel);
        }
        private static double TargetGV(Mat image, PointF point, PointF vector, int SearchRange, int SearchLevel)
        {
            double target = 0;

            byte min;
            byte max;
            GetMinMax(image, point, vector, SearchRange, out min, out max);

            if (max - min < 50)
            {
                target = 0;
            }
            else
            {
                target = (double)(min + (max - min) * SearchLevel * 0.01);
            }

            return target;
        }
        unsafe private static void GetMinMax(Mat image, PointF point, PointF vector, int searchRange, out byte min, out byte max)
        {
            byte* pImg = image.DataPointer;

            min = Byte.MaxValue;
            max = Byte.MinValue;

            double vectorX = 0;
            double vectorY = 0;

            for (int i = 0; i < searchRange; i++)
            {
                int x = (int)(point.X + vectorX + 0.5);
                int y = (int)(point.Y + vectorY + 0.5);

                CheckPointRange(image, ref x, ref y);

                byte pix = pImg[y * image.Width + x];
                if (pix < min)
                    min = pix;
                if (pix > max)
                    max = pix;

                vectorX += vector.X;
                vectorY += vector.Y;
            }
        }
        unsafe private static System.Drawing.Point GetEdgePoint(Mat image, PointF point, PointF vector, double prox, int SearchRange, int SearchLevel)
        {
            byte* pImg = image.DataPointer;

            System.Drawing.Point result = new System.Drawing.Point((int)point.X, (int)point.Y);

            double positionX;
            double positionY;
            double vectorX;
            double vectorY;

            // 찾는 방향 설정 (In to Out / Out to In)
            SetDirection(vector, out positionX, out positionY, out vectorX, out vectorY);

            byte prev = 0;
            byte current = 0;

            int x = 0;
            int y = 0;

            x = (int)(point.X + positionX + 0.5);
            y = (int)(point.Y + positionY + 0.5);

            CheckPointRange(image, ref x, ref y);

            prev = pImg[y * image.Width + x];

            for (int i = 0; i < SearchRange; i++)
            {
                x = (int)(point.X + positionX + 0.5);
                y = (int)(point.Y + positionY + 0.5);

                CheckPointRange(image, ref x, ref y);
                //if (i == 0)
                //{
                //    FindPoint.Add(new System.Drawing.Point(x, y));
                //    FindPoint.Add(new System.Drawing.Point((int)(x + vectorX * SearchRange), (int)(y + vectorY * SearchRange)));
                //}

                current = pImg[y * image.Width + x];
                if ((current >= prox && prox > prev) ||
                   (current <= prox && prox < prev))
                {
                    result.X = x;
                    result.Y = y;
                    break;
                }

                prev = current;

                positionX += vectorX;
                positionY += vectorY;
            }

            return result;
        }
        private static void SetDirection(PointF vector, out double positionX, out double positionY, out double vectorX, out double vectorY)
        {
            positionX = 0;
            positionY = 0;
            vectorX = vector.X;
            vectorY = vector.Y;
        }
        public static void SaveTiff(ref Bitmap DestFile, Bitmap SrcImg, String Filename, bool bEnd)
        {
            EncoderParameters eps = new EncoderParameters();
            System.Drawing.Imaging.Encoder en = System.Drawing.Imaging.Encoder.SaveFlag;
            Filename += ".tif";
            if (bEnd) /// 다저장하고 한번더 호출해야함.
            {
                //Flush
                eps.Param[0] = new EncoderParameter(en, (long)EncoderValue.Flush);
                DestFile.SaveAdd(eps);
                return;
            }

            if (DestFile == null)
            {

                DestFile = new Bitmap(SrcImg);
                ImageCodecInfo Info = null;

                foreach (ImageCodecInfo i in ImageCodecInfo.GetImageEncoders())
                {
                    if (i.MimeType == "image/tiff")
                        Info = i;
                }
                eps.Param[0] = new EncoderParameter(en, (long)EncoderValue.MultiFrame);

                DestFile.Save(Filename, Info, eps);

            }
            else
            {
                eps.Param[0] = new EncoderParameter(en, (long)EncoderValue.FrameDimensionPage);
                DestFile.SaveAdd(SrcImg, eps);

            }

        }
        //public static void SaveImage(DataManager DM, String Path, String PutString = "")
        //{
        //    //SaveImage(DM, Path, DM.m_RecipeDataManager.RD.ImageSave.Ext
        //    //    , DM.m_RecipeDataManager.RD.ImageSave.nQuality
        //    //    , DM.m_RecipeDataManager.RD.ImageSave.nWidth
        //    //    , DM.m_RecipeDataManager.RD.ImageSave.nHeight
        //    //    , DM.m_RecipeDataManager.RD.ImageSave.nRoiWidth
        //    //    , DM.m_RecipeDataManager.RD.ImageSave.nRoiHeight
        //    //    , DM.m_RecipeDataManager.RD.ImageSave.UseOriginalSize
        //    //    , PutString);
        //}
        //public static void SaveImage(DataManager DM, String Path, String nExt, int nQuality, int nW, int nH, int nRoiW, int nRoiH, bool UseOriginalSize = false, String PutString = "")
        //{
        //    Mat ImgS = SapToMat(DM);
        //    OpenCvSharp.CPlusPlus.Size sz = new OpenCvSharp.CPlusPlus.Size(ImgS.Width, ImgS.Height);
        //    Mat Img = new Mat(sz.Height, sz.Width, MatType.CV_8UC4);

        //    ImgS.CopyTo(Img);

        //    OpenCvSharp.CPlusPlus.Size sz2 = new OpenCvSharp.CPlusPlus.Size(nW, nH);
        //    Mat ImgResize;
        //    Mat ImgCropRoi;

        //    if (nRoiW != 0 && nRoiH != 0)
        //    {
        //        int nCenterX = Img.Width / 2;
        //        int nCenterY = Img.Height / 2;

        //        Rect rtDst = new Rect(nCenterX - nRoiW / 2, nCenterY - nRoiH / 2, nRoiW, nRoiH);

        //        ImgCropRoi = Img.Clone(rtDst);
        //    }
        //    else
        //    {
        //        ImgCropRoi = Img;
        //    }



        //    if (UseOriginalSize)
        //    {
        //        ImgResize = ImgCropRoi;
        //    }
        //    else
        //    {
        //        ImgResize = ImgCropRoi.Resize(sz2);
        //    }
        //    double RatioX = (double)ImgCropRoi.Width / (double)ImgResize.Width;
        //    //DrawRuler(ImgResize, DM.m_Setting.CameraSet.LensRes[DM.m_Turret.GetLensIndex()] * RatioX);

        //    if (PutString != "")
        //    {
        //        ImgResize.PutText(PutString, new OpenCvSharp.CPlusPlus.Point(10, 30), FontFace.HersheyPlain, 2, Scalar.Crimson, 2);
        //    }

        //    if (nExt == "JPG")
        //    {
        //        ImageEncodingParam iep = new ImageEncodingParam(ImageEncodingID.JpegQuality, nQuality);
        //        //   Path += ".jpg";
        //        ImgResize.SaveImage(Path, iep);

        //    }
        //    else if (nExt == "BMP")
        //    {
        //        //    Path += ".bmp";
        //        ImgResize.SaveImage(Path);
        //    }
        //}
        public static void DrawRuler(Mat img, double LensRes)
        {
            Scalar Clr = Scalar.Crimson;
            int ShortLen = img.Width;// Math.Min(img.Height, img.Width);
            OpenCvSharp.CPlusPlus.Point RulerStdpt = new OpenCvSharp.CPlusPlus.Point(ShortLen / 10, img.Height - ShortLen / 10);

            int RScale = (int)((double)ShortLen * LensRes / 10);

            for (int i = 1; i < RScale; i *= 10)
            {
                if (RScale / i < 10)
                {
                    RScale = i;
                    break;
                }
            }

            int RulerLength = ShortLen - RulerStdpt.X;      // 전체 자 길이는 이미지 size 받아와서 하면 됨.
            int grad = 0;              // 이것도
            int gradNum = (int)(RulerLength * LensRes / RScale);            // 전체 자 길이/스케일
            gradNum = (gradNum / 5) * 5;
            double FontScale = (double)ShortLen / 300;
            double fixedRulerLength = RScale * gradNum / LensRes;

            int gradLong = (int)((fixedRulerLength / (double)gradNum) * 0.8);
            int gradShort = (int)((fixedRulerLength / (double)gradNum) * 0.5);

            img.PutText(RScale.ToString() + " [um]", new OpenCvSharp.CPlusPlus.Point(RulerStdpt.X - 0, RulerStdpt.Y - gradLong - gradShort), FontFace.HersheyPlain, FontScale, Clr, 1); //text는 눈금-5
            img.Line(RulerStdpt, new OpenCvSharp.CPlusPlus.Point(RulerStdpt.X + fixedRulerLength, RulerStdpt.Y), Clr, 1);

            for (int n = 0; n < gradNum + 1; n++)
            {
                string GradValue = (RScale * n).ToString();
                double XaxisX = RulerStdpt.X + (RScale / LensRes * n);
                double YaxisY = RulerStdpt.Y + (RScale / LensRes * n);

                if (n % 5 == 0)
                {
                    grad = gradLong;
                }
                else
                {
                    grad = gradShort;
                }
                img.Line(new OpenCvSharp.CPlusPlus.Point((double)XaxisX, (double)RulerStdpt.Y), new OpenCvSharp.CPlusPlus.Point((double)XaxisX, (double)RulerStdpt.Y - grad), Clr, 1);

            }
        }
        public static Bitmap PtrToBitmap(IntPtr p, int w, int h)
        {
            Mat Img = new Mat(h, w, MatType.CV_8UC3, p);
            return new Bitmap(Img.ToMemoryStream(".bmp"));
        }
        public static Image PtrToImage(IntPtr p, int w, int h)
        {
            Image i;
            try
            {
                Mat Img = new Mat(h, w, MatType.CV_8UC3, p);
                i = new Bitmap(Img.Clone().ToMemoryStream(".bmp"));
            }
            catch (Exception ex)
            {
                i = null;
            }
            return i;
        }

        public static Mat PtrToMat(IntPtr p, int w, int h)
        {
            Mat Img = new Mat(h, w, MatType.CV_8UC3, p);
            return Img;
        }
        //public static Image SapToImage(DataManager DM)
        //{
        //    //IntPtr p = DM.m_Sapera.GetSaperaBufferAddress();
        //    //System.Drawing.Size sz = DM.m_Sapera.GetImageSize();
        //    //Mat Img = new Mat(sz.Height, sz.Width, MatType.CV_8UC4, p);
        //    //Image i = new Bitmap(Img.ToMemoryStream(".bmp"));
        //    //return i;
        //    Image i = new Bitmap(0, 0);
        //    return i;
        //}
        public static Image MatToImage(Mat Img)
        {
            if (Img == null) return null;
            Image i = new Bitmap(Img.ToMemoryStream(".bmp"));
            return i;
        }
        public static Mat ImageToMat(Image i)
        {
            if (i == null) return null;
            Bitmap bf = new Bitmap(i);
            System.Drawing.Imaging.BitmapData bd = bf.LockBits(new Rectangle(0, 0, bf.Width, bf.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bf.PixelFormat);
            Mat Img = new Mat(bf.Height, bf.Width, MatType.CV_8UC4, bd.Scan0);
            return Img;
        }
        //public static Mat SapToMat(DataManager DM)
        //{
        //    //IntPtr p = DM.m_Sapera.GetSaperaBufferAddress();
        //    //System.Drawing.Size sz = DM.m_Sapera.GetImageSize();
        //    //Mat Img = new Mat(sz.Height, sz.Width, MatType.CV_8UC4, p);

        //    Mat Img = new Mat();
        //    return Img;
        //}

        public static double TemplateMatching(ref System.Drawing.Point pt, Mat FF, Mat SS)
        {
            try
            {
                if (FF == null) return 0;
                if (SS == null) return 0;

                int nWidthFeature = FF.Width;
                int nHeightFeature = FF.Height;
                int nWidthsSource = SS.Width;
                int nHeightSource = SS.Height;
                Mat F = new Mat(nHeightFeature, nWidthFeature, MatType.CV_8UC4);
                Mat S = new Mat(nHeightSource, nWidthsSource, MatType.CV_8UC4);
                FF.CopyTo(F);
                SS.CopyTo(S);
                //느리면 나중에 흑백으로 바꾸어볼라고 놔둠
                //   Mat Fg = new Mat(nHeightFeature, nWidthFeature, MatType.CV_8UC1);
                //  Mat Sg = new Mat(nHeightSource, nWidthsSource, MatType.CV_8UC1);
                //  Cv2.CvtColor(F, Fg, ColorConversion.RgbaToGray);
                //  Cv2.CvtColor(S, Sg, ColorConversion.RgbaToGray);

                int result_cols = nWidthsSource - nWidthFeature + 1;
                int result_rows = nHeightSource - nHeightFeature + 1;

                Mat result = new Mat(result_rows, result_cols, MatType.CV_32FC1);
                OpenCvSharp.CPlusPlus.Point ptMin = new OpenCvSharp.CPlusPlus.Point(0, 0);
                OpenCvSharp.CPlusPlus.Point ptMax = new OpenCvSharp.CPlusPlus.Point(0, 0);
                Cv2.MatchTemplate(S, F, result, MatchTemplateMethod.CCoeffNormed);


                double dMax = 0, dMin = 0;

                Cv2.MinMaxLoc(result, out dMin, out dMax, out ptMin, out ptMax);

                int nX = ptMax.X;
                int nY = ptMax.Y;
                pt.X = nX + nWidthFeature / 2;
                pt.Y = nY + nHeightFeature / 2;

                // for log
                Cv2.Rectangle(S, ptMax, new OpenCvSharp.CPlusPlus.Point(ptMax.X + nWidthFeature, ptMax.Y + nHeightFeature), Scalar.Red);
                //S.SaveImage(Define.File_TemplateMatchingResult);

                return dMax;
            }
            catch (AccessViolationException e)
            {
                System.Windows.MessageBox.Show(e.ToString());
                return 0;

            }
        }
        public static double CalFocusValueLocalVariance(int nCols, int nRows, IntPtr p)
        {
            Mat srcS = new Mat(nRows, nCols, MatType.CV_8UC4, p);
            Mat s = new Mat(nRows, nCols, MatType.CV_8UC4);
            srcS.CopyTo(s);// 보호된 메모리 엑세스 방지
            s = s.CvtColor(ColorConversion.RgbaToGray);

            int nGrid = 8;

            int nW = s.Width / nGrid;
            int nH = s.Height / nGrid;

            double dLocalVaiance = 0;
            for (int i = 0; i + nW < s.Width; i += nW)
            {
                for (int j = 0; j + nH < s.Height; j += nH)
                {
                    OpenCvSharp.CPlusPlus.Rect rtRoi = new OpenCvSharp.CPlusPlus.Rect(i, j, nW, nH);
                    Mat srcRoi = s.Clone(rtRoi);
                    double nAvg = srcRoi.Sum()[0] / nW / nH;

                    Mat mAvg = srcRoi.EmptyClone();
                    mAvg.SetTo(new Scalar((double)nAvg));

                    Cv2.Subtract(srcRoi, mAvg, srcRoi);
                    Cv2.Multiply(srcRoi, srcRoi, srcRoi);
                    dLocalVaiance += srcRoi.Sum()[0] / nW / nH;
                }
            }
            dLocalVaiance = dLocalVaiance / nGrid / nGrid;

            return dLocalVaiance;
        }
        public static int CalFocusValue(int nCols, int nRows, IntPtr p, int n, int nSample = 5)
        {
            unsafe
            {
                Mat srcS = new Mat(nRows, nCols, MatType.CV_8UC4, p);
                Mat src = new Mat(nRows, nCols, MatType.CV_8UC1);
                srcS.CopyTo(src);// 보호된 메모리 엑세스 방지
                src = src.CvtColor(ColorConversion.RgbaToGray);
                if (nSample != 1)
                {
                    OpenCvSharp.CPlusPlus.Size ss = new OpenCvSharp.CPlusPlus.Size(nCols / nSample, nRows / nSample);
                    src = src.Resize(ss);
                }
                Mat dstG = new Mat();
                Mat dstL = new Mat();
                Mat dstA = new Mat();

                OpenCvSharp.CPlusPlus.Size s = new OpenCvSharp.CPlusPlus.Size(3, 3);
                Cv2.GaussianBlur(src, dstG, s, 0.5);
                Cv2.Laplacian(src, dstL, MatType.CV_16S, 3);
                Cv2.ConvertScaleAbs(dstL, dstA);
                // Cv2.Threshold(src, src, 100, 0, ThresholdType.ToZero);

                int nScore = (int)Cv2.Sum(dstA)[0];// -(int)Cv2.Sum(src)[0];


                //src.SaveImage(@"C:\s" + "_" + nScore + ".bmp");
                //dstA.SaveImage(@"C:\a" + "_"+ nScore + ".bmp");

                return nScore;
            }
        }
        public static int CalFocusValue(Mat src, Rect rtRoi)
        {
            unsafe
            {
                Mat dstG = new Mat();
                Mat dstL = new Mat();
                Mat dstA = new Mat();

                Mat srcRoi = src.Clone(rtRoi).CvtColor(ColorConversion.RgbaToGray);//.Resize(new OpenCvSharp.CPlusPlus.Size(src.Width/8,src.Height/8));
                OpenCvSharp.CPlusPlus.Size s = new OpenCvSharp.CPlusPlus.Size(3, 3);

                //   Cv2.Sobel(srcRoi, dstA, MatType.CV_8U, 1, 1);
                Cv2.GaussianBlur(srcRoi, dstG, s, 0);
                Cv2.Laplacian(dstG, dstL, MatType.CV_16S, 3);
                Cv2.ConvertScaleAbs(dstL, dstA);

                return (int)Cv2.Sum(dstA);//Cv2.CountNonZero(dstA); //
            }
        }
        public static int FilterLaplacian(IntPtr ptr1, int nW, int nH, int ByteCount, int nSample, int n)
        {
            int i, j;
            int nTotalSum = 0;
            unsafe
            {
                int n1, n2, n3, n4, n5, n6, n7, n8;
                bool bIgnor;
                int nNosieLevel = 250;

                byte* p = (byte*)ptr1.ToPointer();
                //byte[] pb = new byte[nW * nH * ByteCount];

                //System.Runtime.InteropServices.Marshal.Copy(ptr1, pb, 0, nW * nH * ByteCount);

                //fixed (byte* p = pb)
                {
                    for (j = 1; j < nH - 1; j += nSample)
                    {
                        for (i = 1; i < nW - 1; i += nSample)
                        {

                            int nS = 0;
                            bIgnor = false;
                            for (int b = 0; b < ByteCount; b++)
                            {
                                if (p[j * (nW * ByteCount) + ByteCount * (i) + b] > nNosieLevel)
                                    bIgnor = true;

                                n1 = p[j * (nW * ByteCount) + ByteCount * (i) + b] - p[(j - 1) * (nW * ByteCount) + ByteCount * (i) + b];
                                n2 = p[j * (nW * ByteCount) + ByteCount * (i) + b] - p[j * (nW * ByteCount) + ByteCount * (i + 1) + b];
                                n3 = p[j * (nW * ByteCount) + ByteCount * (i) + b] - p[(j + 1) * (nW * ByteCount) + ByteCount * (i) + b];
                                n4 = p[j * (nW * ByteCount) + ByteCount * (i) + b] - p[j * (nW * ByteCount) + ByteCount * (i - 1) + b];
                                //                               n5 = p[j * (nW * ByteCount) + ByteCount * (i) + b] - p[(j - 1) * (nW * ByteCount) + ByteCount * (i - 1) + b];
                                //                               n6 = p[j * (nW * ByteCount) + ByteCount * (i) + b] - p[(j - 1) * (nW * ByteCount) + ByteCount * (i + 1) + b];
                                //                              n7 = p[j * (nW * ByteCount) + ByteCount * (i) + b] - p[(j + 1) * (nW * ByteCount) + ByteCount * (i - 1) + b];
                                //                               n8 = p[j * (nW * ByteCount) + ByteCount * (i) + b] - p[(j + 1) * (nW * ByteCount) + ByteCount * (i + 1) + b];
                                nS = nS
                                    + Math.Abs(n1)
                                    + Math.Abs(n2)
                                    + Math.Abs(n3)
                                    + Math.Abs(n4);
                                //                                + Math.Abs(n5) //* 2
                                //                                + Math.Abs(n6) //* 2
                                //                                + Math.Abs(n7) //* 2
                                //                                + Math.Abs(n8);// *2;        

                            }
                            //nS /= 0;
                            nS /= ByteCount;
                            if (bIgnor)
                                nS = 0;
                                //nS = -nNosieLevel;

                            nTotalSum += nS;
                        }
                    }
                }

            }

            return nTotalSum;
        }
        public static void Test()
        {
            unsafe
            {
                const int nImage = 70;
                Mat[] src = new Mat[nImage];
                Mat[] dstG = new Mat[nImage];
                Mat[] dstL = new Mat[nImage];
                Mat[] dstA = new Mat[nImage];

                MatOfByte3[] mat3Src = new MatOfByte3[nImage];
                MatOfByte[] matdstA = new MatOfByte[nImage];
                int nName = 10;
                long start = DateTime.Now.Ticks;
                for (int n = 0; n < nImage; n++)
                {
                    String FileName = (nName + n).ToString();
                    src[n] = new Mat(@"C:\fs3\s" + FileName + ".bmp", LoadMode.Unchanged);
                    mat3Src[n] = new MatOfByte3(src[n]);
                    dstG[n] = new Mat();
                    dstL[n] = new Mat();
                    dstA[n] = new Mat();
                    OpenCvSharp.CPlusPlus.Size s = new OpenCvSharp.CPlusPlus.Size(3, 3);
                    Cv2.GaussianBlur(src[n].CvtColor(ColorConversion.RgbaToGray), dstG[n], s, 0);
                    Cv2.Laplacian(dstG[n], dstL[n], MatType.CV_16S, 3);
                    Cv2.ConvertScaleAbs(dstL[n], dstA[n]);
                    matdstA[n] = new MatOfByte(dstA[n]);
                }
                Mat Fin = new Mat(src[0].Rows, src[0].Cols, src[0].Type());
                Mat MaxArr = new Mat(nImage, 1, MatType.CV_8UC1);

                MatOfByte3 mat3 = new MatOfByte3(Fin);

                var indexer = mat3.GetIndexer();

                int[] bbb = new int[dstA[0].Width * dstA[0].Height];

                for (int j = 0; j < dstA[0].Height; j++)
                {
                    Parallel.For(0, dstA[0].Width, i =>
                    //for (int i = 0; i < dstA[0].Width; i++)
                    {
                        int nMax = 0;
                        byte b = 0;
                        for (int n = 0; n < nImage; n++)
                        {
                            byte b2 = matdstA[n].GetIndexer()[j, i];
                            if (b2 > b)
                            {
                                b = b2;
                                nMax = n;
                            }
                        }
                        bbb[j * dstA[0].Width + i] = b;
                        indexer[j, i] = mat3Src[nMax].GetIndexer()[j, i];

                    });
                }
                long end = DateTime.Now.Ticks;
                double t = (double)(end - start) / 10000000.0F;


                //plotControl1.SurfacePlot(dstA[0].Width, dstA[0].Height, bbb, 10);
                //plotControl1.DotPlot(2, 3, bbb);

                //plotControl1.DotPlot(dstA[0].Width, dstA[0].Height, bbb,100);
                //MessageBox.Show(t.ToString());
                //  using (new Window("src image", src[0]))
                Fin.SaveImage(@"C:\f.bmp");
                //  using (new Window("Fin image", Fin))
                //   {
                //      Cv2.WaitKey();
                //  }
            }
        }

        // 중심좌표 1개 5x Version - hjhwang 181205
        public static OpenCvSharp.CPlusPlus.Point FindCircleCenter(Mat image, List<OpenCvSharp.CPlusPlus.Point> pts, int nThreshold, ref double dDiameter)
        {
            try
            {
                unsafe
                {
                    OpenCvSharp.CPlusPlus.Point CenterPoint = new OpenCvSharp.CPlusPlus.Point();

                    Mat matImg = new Mat();
                    image.CopyTo(matImg);

                    Mat matGaussianImg;
                    Mat matThresholdImg;
                    Mat matContourImg;
                    Mat matSeedPointImg;
                    Mat matEdgeImg;
                    Mat matEllipseImg;
                    Mat matOtsu;

                    matGaussianImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC1);
                    matThresholdImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC1);
                    matContourImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC3);
                    matSeedPointImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC3);
                    matEdgeImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC3);
                    matEllipseImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC3);
                    matOtsu = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC3);
                    //m_ptInitCenter.Clear();      // hjhwang 181115 - 리스트 비우고 초기화

                    int nWindowWidth = (matImg.Width) / 4;
                    int nWindowHeight = (matImg.Height) / 4;

                    //Cv2.NamedWindow("Input", WindowMode.NormalGui);
                    //Cv2.ResizeWindow("Input", nWindowWidth, nWindowHeight);
                    //Cv2.NamedWindow("Gaussian", WindowMode.NormalGui);
                    //Cv2.ResizeWindow("Gaussian", nWindowWidth, nWindowHeight);
                    //Cv2.NamedWindow("Otsu", WindowMode.NormalGui);
                    //Cv2.ResizeWindow("Otsu", nWindowWidth, nWindowHeight);
                    //Cv2.NamedWindow("Seed Point", WindowMode.NormalGui);
                    //Cv2.ResizeWindow("Seed Point", nWindowWidth, nWindowHeight);
                    //Cv2.NamedWindow("Find Edge", WindowMode.NormalGui);
                    //Cv2.ResizeWindow("Find Edge", nWindowWidth, nWindowHeight);
                    //Cv2.NamedWindow("Find Circle", WindowMode.NormalGui);
                    //Cv2.ResizeWindow("Find Circle", nWindowWidth, nWindowHeight);

                    // Input

                    Cv2.ImWrite("D:\\Temp\\SensorCenter_Input.jpg", matImg);
                    // Gaussian
                    Cv2.GaussianBlur(matImg, matGaussianImg, new OpenCvSharp.CPlusPlus.Size(3, 3), 1, 1);

                    Cv2.ImWrite("D:\\Temp\\SensorCenter_GaussBlur.jpg", matGaussianImg);
                    //// Threshold
                    //Cv2.Threshold(matGaussianImg, matThresholdImg, 100, 255, ThresholdType.Otsu);
                    //Cv2.Threshold(matGaussianImg, matThresholdImg, nThreshold, 255, ThresholdType.Binary);

                    //Cv2.ImWrite("D:\\Temp\\SensorCenter_Threshold.jpg", matThresholdImg);

                    Cv2.Threshold(matGaussianImg, matThresholdImg, nThreshold, 255, ThresholdType.Otsu);
                    Cv2.ImWrite("D:\\Temp\\SensorCenter_ThresholdOtsu.jpg", matThresholdImg);
                    // Seed Point (초기값 설정)
                    Cv2.CvtColor(matThresholdImg, matSeedPointImg, ColorConversion.GrayToBgr);
                    Cv2.ImWrite("D:\\Temp\\SensorCenter_CVtColor.jpg", matSeedPointImg);
                   // 여기서부터 안됨 다시 확인

                    List<double> ptFirst = new List<double>(new double[] { pts[0].X, pts[0].Y });


                    // Find Edge
                                                      // hjhwang 181113 - 들어갈 포인트 360개로 수정
                    Cv2.CvtColor(matThresholdImg, matEdgeImg, ColorConversion.GrayToBgr);
                    Cv2.ImWrite("D:\\Temp\\SensorCenter_ThresholdCvtColor.jpg", matEdgeImg);

                    //Cv2.Threshold(matImg, matOtsu, nThreshold,255,ThresholdType.Otsu);
                    //Cv2.ImWrite("D:\\Temp\\SensorCenter_ThresholdOtsu.jpg", matOtsu);

                    OpenCvSharp.CPlusPlus.Point2f[] ptDir = MakeSearchDir();
                    List<List<double>> ltEdgePointFirst = FindEdgePoints(matThresholdImg, ptDir, ptFirst);   // hjhwang 181113 - 각도 360도로 수정 및 ltEdgePoint 리스트에 값 담으면서 (x, y) 최대, 최소 값 구하고, 그 평균 m_ptInitCenter 에 저장

                    // 원에 대한 중심점 [ (x, y, r ) ] 쌍 담음
                    List<double> x1y1r1 = new List<double>();
                    Fitting Circle = new Fitting();

                    //m_ptInitCenter[0].Add(1000);      // 초기값으로 r = 1000 추가
                    //x1y1r1 = Circle.LineFitting(100, 3, m_ptInitCenter[0], ltEdgePointFirst, "LMA", "Circle", true);
                    ptFirst.Add(1000);
                    x1y1r1 = Circle.LineFitting(100, 3, ptFirst, ltEdgePointFirst, "LMA", "Circle", true);

                    // 초기 좌표 재설정 및 초기화
                    double dRadius = x1y1r1[2];
                    ptFirst = new List<double>(new double[] { x1y1r1[0], x1y1r1[1] });       // 초기 x, y 좌표
                    //m_ptInitCenter.Clear();
                    x1y1r1.Clear();

                    // 다시 원에 대한 중심점 [ (x, y, r ) ] 쌍 담음
                    ltEdgePointFirst = FindEdgePoints(matThresholdImg, ptDir, ptFirst);
                    x1y1r1 = Circle.LineFitting(100, 2, ptFirst, ltEdgePointFirst, "LMA", "Circle", false, dRadius);


                    CenterPoint.X = (int)x1y1r1[0];
                    CenterPoint.Y = (int)x1y1r1[1];

                    // 엣지 검출해서 빨간색 테두리 그리기
                    for (int i = 0; i < ltEdgePointFirst.Count; i++)
                    {
                        Cv2.Line(matEdgeImg, new OpenCvSharp.CPlusPlus.Point(ltEdgePointFirst[i][0] - 10, ltEdgePointFirst[i][1] - 10), new OpenCvSharp.CPlusPlus.Point(ltEdgePointFirst[i][0] + 10, ltEdgePointFirst[i][1] + 10), Scalar.Red, 10);
                        Cv2.Line(matEdgeImg, new OpenCvSharp.CPlusPlus.Point(ltEdgePointFirst[i][0] - 10, ltEdgePointFirst[i][1] + 10), new OpenCvSharp.CPlusPlus.Point(ltEdgePointFirst[i][0] + 10, ltEdgePointFirst[i][1] - 10), Scalar.Red, 10);
                    }

                    //Cv2.ImWrite("D:\\Temp\\sensorcenter.jpg", matEdgeImg);
                    Cv2.ImWrite("D:\\Temp\\SensorCenter_Align.jpg", matEdgeImg);

                    Cv2.CvtColor(matThresholdImg, matEllipseImg, ColorConversion.GrayToBgr);

                    // 원에서 최초 찍은 좌표 그리기
                    Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(ptFirst[0] - 10, ptFirst[1] - 10), new OpenCvSharp.CPlusPlus.Point(ptFirst[0] + 10, ptFirst[1] + 10), Scalar.Wheat, 10);
                    Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(ptFirst[0] - 10, ptFirst[1] + 10), new OpenCvSharp.CPlusPlus.Point(ptFirst[0] + 10, ptFirst[1] - 10), Scalar.Wheat, 10);

                    // 검출된 점들 평균 좌표 그리기
                    //Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(m_ptInitCenter[0][0] - 10, m_ptInitCenter[0][1] - 10), new OpenCvSharp.CPlusPlus.Point(m_ptInitCenter[0][0] + 10, m_ptInitCenter[0][1] + 10), Scalar.SeaGreen, 10);
                    //Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(m_ptInitCenter[0][0] - 10, m_ptInitCenter[0][1] + 10), new OpenCvSharp.CPlusPlus.Point(m_ptInitCenter[0][0] + 10, m_ptInitCenter[0][1] - 10), Scalar.SeaGreen, 10);

                    // 원의 중심 좌표 그리기
                    Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(CenterPoint.X - 10, CenterPoint.Y - 10), new OpenCvSharp.CPlusPlus.Point(CenterPoint.X + 10, CenterPoint.Y + 10), Scalar.Purple, 10);
                    Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(CenterPoint.X - 10, CenterPoint.Y + 10), new OpenCvSharp.CPlusPlus.Point(CenterPoint.X + 10, CenterPoint.Y - 10), Scalar.Purple, 10);

                    //Cv2.ImShow("Input", matImg);
                    //Cv2.ImShow("Gaussian", matGaussianImg);
                    //Cv2.ImShow("Otsu", matThresholdImg);
                    //Cv2.ImShow("Seed Point", matSeedPointImg);
                    //Cv2.ImShow("Find Edge", matEdgeImg);
                    //Cv2.ImShow("Find Circle", matEllipseImg);

                    // ??????????????????????이거 어떻게 띄우지?????????????????????? --> 얘가 값 입력해야 하는 offset
                    OpenCvSharp.CPlusPlus.Point2d ptOffset = GetOffsetPulseFromScreenCenter(CenterPoint, 1.098);

                    dDiameter = dRadius * 2;
                    return CenterPoint;
                }
            }
            catch (Exception ex)
            {
                return new OpenCvSharp.CPlusPlus.Point(0, 0);
            }
        }

        // 중심좌표 3개 10x Version - hjhwang 181205
        /*public static OpenCvSharp.CPlusPlus.Point FindCircleCenter(Mat image, List<OpenCvSharp.CPlusPlus.Point> pts, ref double dDiameter)
        {
            unsafe
            {
                OpenCvSharp.CPlusPlus.Point CenterPoint = new OpenCvSharp.CPlusPlus.Point();

                Mat matImg = new Mat();
                image.CopyTo(matImg);

                Mat matGaussianImg;
                Mat matThresholdImg;
                Mat matContourImg;
                Mat matSeedPointImg;
                Mat matEdgeImg;
                Mat matEllipseImg;

                matGaussianImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC1);
                matThresholdImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC1);
                matContourImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC3);
                matSeedPointImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC3);
                matEdgeImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC3);
                matEllipseImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC3);

                //m_ptInitCenter.Clear();      // hjhwang 181115 - 리스트 비우고 초기화

                int nWindowWidth = (matImg.Width) / 4;
                int nWindowHeight = (matImg.Height) / 4;

                Cv2.NamedWindow("Input", WindowMode.NormalGui);
                Cv2.ResizeWindow("Input", nWindowWidth, nWindowHeight);
                Cv2.NamedWindow("Gaussian", WindowMode.NormalGui);
                Cv2.ResizeWindow("Gaussian", nWindowWidth, nWindowHeight);
                Cv2.NamedWindow("Otsu", WindowMode.NormalGui);
                Cv2.ResizeWindow("Otsu", nWindowWidth, nWindowHeight);
                Cv2.NamedWindow("Seed Point", WindowMode.NormalGui);
                Cv2.ResizeWindow("Seed Point", nWindowWidth, nWindowHeight);
                Cv2.NamedWindow("Find Edge", WindowMode.NormalGui);
                Cv2.ResizeWindow("Find Edge", nWindowWidth, nWindowHeight);
                Cv2.NamedWindow("Find Circle", WindowMode.NormalGui);
                Cv2.ResizeWindow("Find Circle", nWindowWidth, nWindowHeight);

                // Input
                Cv2.ImShow("Input", matImg);

                // Gaussian
                Cv2.GaussianBlur(matImg, matGaussianImg, new OpenCvSharp.CPlusPlus.Size(3, 3), 1, 1);
                Cv2.ImShow("Gaussian", matGaussianImg);

                //// Threshold
                Cv2.Threshold(matGaussianImg, matThresholdImg, 100, 255, ThresholdType.Otsu);
                Cv2.ImShow("Otsu", matThresholdImg);

                // Seed Point (초기값 설정)
                Cv2.CvtColor(matThresholdImg, matSeedPointImg, ColorConversion.GrayToBgr);
                List<double> ptFirst = new List<double>(new double[] { pts[0].X, pts[0].Y });
                List<double> ptSecond = new List<double>(new double[] { pts[1].X, pts[1].Y });
                List<double> ptThird = new List<double>(new double[] { pts[2].X, pts[2].Y });
                Cv2.ImShow("Seed Point", matSeedPointImg);

                // Find Edge
                OpenCvSharp.CPlusPlus.Point2f[] ptDir = MakeSearchDir();                                    // hjhwang 181113 - 들어갈 포인트 360개로 수정
                Cv2.CvtColor(matThresholdImg, matEdgeImg, ColorConversion.GrayToBgr);
                List<List<double>> ltEdgePointFirst = FindEdgePoints(matThresholdImg, ptDir, ptFirst);             // hjhwang 181113 - 각도 360도로 수정 및 ltEdgePoint 리스트에 값 담음
                List<List<double>> ltEdgePointSecond = FindEdgePoints(matThresholdImg, ptDir, ptSecond);    // hjhwang 181113 - 각도 360도로 수정 및 ltEdgePoint 리스트에 값 담음
                List<List<double>> ltEdgePointThird = FindEdgePoints(matThresholdImg, ptDir, ptThird);          // hjhwang 181113 - 각도 360도로 수정 및 ltEdgePoint 리스트에 값 담음

                // 원에 대한 중심점 [ (x, y, r ) ] 쌍 담음
                List<double> x1y1r1 = new List<double>();
                List<double> x2y2r2 = new List<double>();
                List<double> x3y3r3 = new List<double>();
                Fitting Circle = new Fitting();

                //m_ptInitCenter[0].Add(1000);      // 초기값으로 r = 1000 추가
                //x1y1r1 = Circle.LineFitting(100, 3, m_ptInitCenter[0], ltEdgePointFirst, "LMA", "Circle", true);
                ptSecond.Add(1000);
                x2y2r2 = Circle.LineFitting(100, 3, ptSecond, ltEdgePointSecond, "LMA", "Circle", true);
                x1y1r1 = Circle.LineFitting(100, 2, ptFirst, ltEdgePointFirst, "LMA", "Circle", false, x2y2r2[2]);
                x3y3r3 = Circle.LineFitting(100, 2, ptThird, ltEdgePointThird, "LMA", "Circle", false, x2y2r2[2]);

                // 초기 좌표 재설정 및 초기화 - 원중심점으로 초기좌표 재설정하면, 이미지에서 잘린, (-)인 값 때문에 뻑남
                //double dRadius = x2y2r2[2];
                //ptFirst = new List<double>(new double[] { x1y1r1[0], x1y1r1[1] });       // 초기 x, y 좌표
                //ptSecond = new List<double>(new double[] { x2y2r2[0], x2y2r2[1] });       // 초기 x, y 좌표
                //ptThird = new List<double>(new double[] { x3y3r3[0], x3y3r3[1] });       // 초기 x, y 좌표
                ////m_ptInitCenter.Clear();
                //x1y1r1.Clear();
                //x2y2r2.Clear();
                //x3y3r3.Clear();

                //// 다시 원에 대한 중심점 [ (x, y, r ) ] 쌍 담음
                //ltEdgePointFirst = FindEdgePoints(matThresholdImg, ptDir, ptFirst);
                //ltEdgePointSecond = FindEdgePoints(matThresholdImg, ptDir, ptSecond);
                //ltEdgePointThird = FindEdgePoints(matThresholdImg, ptDir, ptThird);        

                //x1y1r1 = Circle.LineFitting(100, 2, ptFirst, ltEdgePointFirst, "LMA", "Circle", false, dRadius);
                //x2y2r2 = Circle.LineFitting(100, 2, ptFirst, ltEdgePointSecond, "LMA", "Circle", false, dRadius);
                //x3y3r3 = Circle.LineFitting(100, 2, ptFirst, ltEdgePointThird, "LMA", "Circle", false, dRadius);
                
                // 중심좌표 그리기
                double dx1 = x1y1r1[0];
                double dy1 = x1y1r1[1];
                double dx2 = x2y2r2[0];
                double dy2 = x2y2r2[1];
                double dx3 = x3y3r3[0];
                double dy3 = x3y3r3[1];
                double dd1 = (dx2 - dx1) / (dy2 - dy1);
                double dd2 = (dx3 - dx2) / (dy3 - dy2);
                double dcx = ((dy3 - dy1) + (dx2 + dx3) * dd2 - (dx1 + dx2) * dd1) / (2 * (dd2 - dd1));
                double dcy = -dd1 * (dcx - (dx1 + dx2) / 2) + (dy1 + dy2) / 2;


                CenterPoint.X = (int)Math.Round(dcx);
                CenterPoint.Y = (int)Math.Round(dcy);

                // 엣지 검출해서 빨간색 테두리 그리기
                for (int i = 0; i < ltEdgePointFirst.Count; i++)
                {
                    Cv2.Line(matEdgeImg, new OpenCvSharp.CPlusPlus.Point(ltEdgePointFirst[i][0] - 10, ltEdgePointFirst[i][1] - 10), new OpenCvSharp.CPlusPlus.Point(ltEdgePointFirst[i][0] + 10, ltEdgePointFirst[i][1] + 10), Scalar.Red, 10);
                    Cv2.Line(matEdgeImg, new OpenCvSharp.CPlusPlus.Point(ltEdgePointFirst[i][0] - 10, ltEdgePointFirst[i][1] + 10), new OpenCvSharp.CPlusPlus.Point(ltEdgePointFirst[i][0] + 10, ltEdgePointFirst[i][1] - 10), Scalar.Red, 10);
                }

                for (int i = 0; i < ltEdgePointSecond.Count; i++)
                {
                    Cv2.Line(matEdgeImg, new OpenCvSharp.CPlusPlus.Point(ltEdgePointSecond[i][0] - 10, ltEdgePointSecond[i][1] - 10), new OpenCvSharp.CPlusPlus.Point(ltEdgePointSecond[i][0] + 10, ltEdgePointSecond[i][1] + 10), Scalar.Red, 10);
                    Cv2.Line(matEdgeImg, new OpenCvSharp.CPlusPlus.Point(ltEdgePointSecond[i][0] - 10, ltEdgePointSecond[i][1] + 10), new OpenCvSharp.CPlusPlus.Point(ltEdgePointSecond[i][0] + 10, ltEdgePointSecond[i][1] - 10), Scalar.Red, 10);
                }

                for (int i = 0; i < ltEdgePointThird.Count; i++)
                {
                    Cv2.Line(matEdgeImg, new OpenCvSharp.CPlusPlus.Point(ltEdgePointThird[i][0] - 10, ltEdgePointThird[i][1] - 10), new OpenCvSharp.CPlusPlus.Point(ltEdgePointThird[i][0] + 10, ltEdgePointThird[i][1] + 10), Scalar.Red, 10);
                    Cv2.Line(matEdgeImg, new OpenCvSharp.CPlusPlus.Point(ltEdgePointThird[i][0] - 10, ltEdgePointThird[i][1] + 10), new OpenCvSharp.CPlusPlus.Point(ltEdgePointThird[i][0] + 10, ltEdgePointThird[i][1] - 10), Scalar.Red, 10);
                }
                Cv2.ImShow("Find Edge", matEdgeImg);
                Cv2.CvtColor(matThresholdImg, matEllipseImg, ColorConversion.GrayToBgr);

                // 원에서 최초 찍은 좌표 그리기
                Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(ptFirst[0] - 10, ptFirst[1] - 10), new OpenCvSharp.CPlusPlus.Point(ptFirst[0] + 10, ptFirst[1] + 10), Scalar.Wheat, 10);
                Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(ptFirst[0] - 10, ptFirst[1] + 10), new OpenCvSharp.CPlusPlus.Point(ptFirst[0] + 10, ptFirst[1] - 10), Scalar.Wheat, 10);

                // 검출된 점들 평균 좌표 그리기
                //Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(m_ptInitCenter[0][0] - 10, m_ptInitCenter[0][1] - 10), new OpenCvSharp.CPlusPlus.Point(m_ptInitCenter[0][0] + 10, m_ptInitCenter[0][1] + 10), Scalar.SeaGreen, 10);
                //Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(m_ptInitCenter[0][0] - 10, m_ptInitCenter[0][1] + 10), new OpenCvSharp.CPlusPlus.Point(m_ptInitCenter[0][0] + 10, m_ptInitCenter[0][1] - 10), Scalar.SeaGreen, 10);

                // 원의 중심 좌표 그리기
                Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(CenterPoint.X - 10, CenterPoint.Y - 10), new OpenCvSharp.CPlusPlus.Point(CenterPoint.X + 10, CenterPoint.Y + 10), Scalar.Purple, 10);
                Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(CenterPoint.X - 10, CenterPoint.Y + 10), new OpenCvSharp.CPlusPlus.Point(CenterPoint.X + 10, CenterPoint.Y - 10), Scalar.Purple, 10);

                Cv2.ImShow("Find Circle", matEllipseImg);

                // ??????????????????????이거 어떻게 띄우지?????????????????????? --> 얘가 값 입력해야 하는 offset
                OpenCvSharp.CPlusPlus.Point2d ptOffset = GetOffsetPulseFromScreenCenter(CenterPoint, General.dResolution * 10);

                dDiameter = dRadius * 2;
                return CenterPoint;
            }
        }*/

        // hjhwang 181205
        public static OpenCvSharp.CPlusPlus.Point2d GetOffsetPulseFromScreenCenter(OpenCvSharp.CPlusPlus.Point ptCircleCenter, double ImageResolution)
        {
            // 화면 중심 좌표 - 수정 가능
            OpenCvSharp.CPlusPlus.Point ptScreenCenter = new OpenCvSharp.CPlusPlus.Point(1000.0, 1000.0);

            // 좌표 평행이동 : (0,0) 에서 top left, 우하향으로 (+) ==> screen center 중심으로 일반 좌표계 : (x - screen center X, screen center Y - y)
            // 그 후 Resolution 곱하고 pulse 단위(1um = 10 pulse) 곱함 ==> 결과 : 축 값
            OpenCvSharp.CPlusPlus.Point2d ptOffset = new OpenCvSharp.CPlusPlus.Point2d();
            ptOffset.X = (ptScreenCenter.X - ptCircleCenter.X) * ImageResolution * 10;
            ptOffset.Y = (ptCircleCenter.Y - ptScreenCenter.Y) * ImageResolution * 10;

            return ptOffset;
        }

        // 다영찡 원본
        /*public static OpenCvSharp.CPlusPlus.Point FindCircleCenter(Mat image, List<OpenCvSharp.CPlusPlus.Point> pts, ref double dDiameter)
        {
            unsafe
            {
                if (pts.Count < 3)
                    return new OpenCvSharp.CPlusPlus.Point(0, 0);

                OpenCvSharp.CPlusPlus.Point CenterPoint = new OpenCvSharp.CPlusPlus.Point();

                Mat matImg = new Mat();
                image.CopyTo(matImg);

                Mat matGaussianImg;
                Mat matThresholdImg;
                Mat matContourImg;
                Mat matSeedPointImg;
                Mat matEdgeImg;
                Mat matEllipseImg;

                matGaussianImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC1);
                matThresholdImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC1);
                matContourImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC3);
                matSeedPointImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC3);
                matEdgeImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC3);
                matEllipseImg = new Mat(matImg.Rows, matImg.Cols, MatType.CV_8UC3);

                int nWindowWidth = (matImg.Width) / 4;
                int nWindowHeight = (matImg.Height) / 4;

                //Cv2.NamedWindow("Input", WindowMode.NormalGui);
                //Cv2.ResizeWindow("Input", nWindowWidth, nWindowHeight);
                //Cv2.NamedWindow("Gaussian", WindowMode.NormalGui);
                //Cv2.ResizeWindow("Gaussian", nWindowWidth, nWindowHeight);
                //Cv2.NamedWindow("Otsu", WindowMode.NormalGui);
                //Cv2.ResizeWindow("Otsu", nWindowWidth, nWindowHeight);
                //Cv2.NamedWindow("Seed Point", WindowMode.NormalGui);
                //Cv2.ResizeWindow("Seed Point", nWindowWidth, nWindowHeight);
                Cv2.NamedWindow("Find Edge", WindowMode.NormalGui);
                Cv2.ResizeWindow("Find Edge", nWindowWidth, nWindowHeight);
                Cv2.NamedWindow("Find Circle", WindowMode.NormalGui);
                Cv2.ResizeWindow("Find Circle", nWindowWidth, nWindowHeight);

                // Input
                //Cv2.ImShow("Input", matImg);

                // Gaussian
                Cv2.GaussianBlur(matImg, matGaussianImg, new OpenCvSharp.CPlusPlus.Size(3, 3), 1, 1);
                //Cv2.ImShow("Gaussian", matGaussianImg);

                // Threshold
                Cv2.Threshold(matGaussianImg, matThresholdImg, 100, 255, ThresholdType.Otsu);
                //Cv2.ImShow("Otsu", matThresholdImg);

                // Seed Point
                Cv2.CvtColor(matThresholdImg, matSeedPointImg, ColorConversion.GrayToBgr);
                OpenCvSharp.CPlusPlus.Point ptFirst = pts[0];//new OpenCvSharp.CPlusPlus.Point(1032, 96);
                OpenCvSharp.CPlusPlus.Point ptSecond = pts[1];//new OpenCvSharp.CPlusPlus.Point(935, 883);
                OpenCvSharp.CPlusPlus.Point ptThird = pts[2];//new OpenCvSharp.CPlusPlus.Point(290, 1783);
                Cv2.Line(matSeedPointImg, new OpenCvSharp.CPlusPlus.Point(ptFirst.X - 10, ptFirst.Y - 10), new OpenCvSharp.CPlusPlus.Point(ptFirst.X + 10, ptFirst.Y + 10), Scalar.Wheat, 10);
                Cv2.Line(matSeedPointImg, new OpenCvSharp.CPlusPlus.Point(ptFirst.X - 10, ptFirst.Y + 10), new OpenCvSharp.CPlusPlus.Point(ptFirst.X + 10, ptFirst.Y - 10), Scalar.Wheat, 10);
                Cv2.Line(matSeedPointImg, new OpenCvSharp.CPlusPlus.Point(ptSecond.X - 10, ptSecond.Y - 10), new OpenCvSharp.CPlusPlus.Point(ptSecond.X + 10, ptSecond.Y + 10), Scalar.Wheat, 10);
                Cv2.Line(matSeedPointImg, new OpenCvSharp.CPlusPlus.Point(ptSecond.X - 10, ptSecond.Y + 10), new OpenCvSharp.CPlusPlus.Point(ptSecond.X + 10, ptSecond.Y - 10), Scalar.Wheat, 10);
                Cv2.Line(matSeedPointImg, new OpenCvSharp.CPlusPlus.Point(ptThird.X - 10, ptThird.Y - 10), new OpenCvSharp.CPlusPlus.Point(ptThird.X + 10, ptThird.Y + 10), Scalar.Wheat, 10);
                Cv2.Line(matSeedPointImg, new OpenCvSharp.CPlusPlus.Point(ptThird.X - 10, ptThird.Y + 10), new OpenCvSharp.CPlusPlus.Point(ptThird.X + 10, ptThird.Y - 10), Scalar.Wheat, 10);
                //Cv2.ImShow("Seed Point", matSeedPointImg);

                // Find Edge
                OpenCvSharp.CPlusPlus.Point2f[] ptDir = MakeSearchDir();
                Cv2.CvtColor(matThresholdImg, matEdgeImg, ColorConversion.GrayToBgr);
                List<OpenCvSharp.CPlusPlus.Point> ltEdgePointFirst = new List<OpenCvSharp.CPlusPlus.Point>();
                List<OpenCvSharp.CPlusPlus.Point> ltEdgePointSecond = new List<OpenCvSharp.CPlusPlus.Point>();
                List<OpenCvSharp.CPlusPlus.Point> ltEdgePointThird = new List<OpenCvSharp.CPlusPlus.Point>();
                FindEdge(matThresholdImg, ptFirst, ptDir, ref ltEdgePointFirst);
                for (int i = 0; i < ltEdgePointFirst.Count; i++)
                {
                    Cv2.Line(matEdgeImg, new OpenCvSharp.CPlusPlus.Point(ltEdgePointFirst[i].X - 10, ltEdgePointFirst[i].Y - 10), new OpenCvSharp.CPlusPlus.Point(ltEdgePointFirst[i].X + 10, ltEdgePointFirst[i].Y + 10), Scalar.Red, 10);
                    Cv2.Line(matEdgeImg, new OpenCvSharp.CPlusPlus.Point(ltEdgePointFirst[i].X - 10, ltEdgePointFirst[i].Y + 10), new OpenCvSharp.CPlusPlus.Point(ltEdgePointFirst[i].X + 10, ltEdgePointFirst[i].Y - 10), Scalar.Red, 10);
                }
                FindEdge(matThresholdImg, ptSecond, ptDir, ref ltEdgePointSecond);
                for (int i = 0; i < ltEdgePointSecond.Count; i++)
                {
                    Cv2.Line(matEdgeImg, new OpenCvSharp.CPlusPlus.Point(ltEdgePointSecond[i].X - 10, ltEdgePointSecond[i].Y - 10), new OpenCvSharp.CPlusPlus.Point(ltEdgePointSecond[i].X + 10, ltEdgePointSecond[i].Y + 10), Scalar.Red, 10);
                    Cv2.Line(matEdgeImg, new OpenCvSharp.CPlusPlus.Point(ltEdgePointSecond[i].X - 10, ltEdgePointSecond[i].Y + 10), new OpenCvSharp.CPlusPlus.Point(ltEdgePointSecond[i].X + 10, ltEdgePointSecond[i].Y - 10), Scalar.Red, 10);
                }
                FindEdge(matThresholdImg, ptThird, ptDir, ref ltEdgePointThird);
                for (int i = 0; i < ltEdgePointThird.Count; i++)
                {
                    Cv2.Line(matEdgeImg, new OpenCvSharp.CPlusPlus.Point(ltEdgePointThird[i].X - 10, ltEdgePointThird[i].Y - 10), new OpenCvSharp.CPlusPlus.Point(ltEdgePointThird[i].X + 10, ltEdgePointThird[i].Y + 10), Scalar.Red, 10);
                    Cv2.Line(matEdgeImg, new OpenCvSharp.CPlusPlus.Point(ltEdgePointThird[i].X - 10, ltEdgePointThird[i].Y + 10), new OpenCvSharp.CPlusPlus.Point(ltEdgePointThird[i].X + 10, ltEdgePointThird[i].Y - 10), Scalar.Red, 10);
                }
                Cv2.ImShow("Find Edge", matEdgeImg);

                // Find Circle
                Cv2.CvtColor(matThresholdImg, matEllipseImg, ColorConversion.GrayToBgr);
                RotatedRect rtFitFirst = new RotatedRect();
                RotatedRect rtFitSecond = new RotatedRect();
                RotatedRect rtFitThird = new RotatedRect();
                rtFitFirst = Cv2.FitEllipse(ltEdgePointFirst);
                rtFitSecond = Cv2.FitEllipse(ltEdgePointSecond);
                rtFitThird = Cv2.FitEllipse(ltEdgePointThird);
                Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(rtFitFirst.Center.X - 10, rtFitFirst.Center.Y - 10), new OpenCvSharp.CPlusPlus.Point(rtFitFirst.Center.X + 10, rtFitFirst.Center.Y + 10), Scalar.Purple, 10);
                Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(rtFitFirst.Center.X - 10, rtFitFirst.Center.Y + 10), new OpenCvSharp.CPlusPlus.Point(rtFitFirst.Center.X + 10, rtFitFirst.Center.Y - 10), Scalar.Purple, 10);
                Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(rtFitSecond.Center.X - 10, rtFitSecond.Center.Y - 10), new OpenCvSharp.CPlusPlus.Point(rtFitSecond.Center.X + 10, rtFitSecond.Center.Y + 10), Scalar.Purple, 10);
                Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(rtFitSecond.Center.X - 10, rtFitSecond.Center.Y + 10), new OpenCvSharp.CPlusPlus.Point(rtFitSecond.Center.X + 10, rtFitSecond.Center.Y - 10), Scalar.Purple, 10);
                Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(rtFitThird.Center.X - 10, rtFitThird.Center.Y - 10), new OpenCvSharp.CPlusPlus.Point(rtFitThird.Center.X + 10, rtFitThird.Center.Y + 10), Scalar.Purple, 10);
                Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(rtFitThird.Center.X - 10, rtFitThird.Center.Y + 10), new OpenCvSharp.CPlusPlus.Point(rtFitThird.Center.X + 10, rtFitThird.Center.Y - 10), Scalar.Purple, 10);

                Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(ptFirst.X - 10, ptFirst.Y - 10), new OpenCvSharp.CPlusPlus.Point(ptFirst.X + 10, ptFirst.Y + 10), Scalar.Wheat, 10);
                Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(ptFirst.X - 10, ptFirst.Y + 10), new OpenCvSharp.CPlusPlus.Point(ptFirst.X + 10, ptFirst.Y - 10), Scalar.Wheat, 10);
                Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(ptSecond.X - 10, ptSecond.Y - 10), new OpenCvSharp.CPlusPlus.Point(ptSecond.X + 10, ptSecond.Y + 10), Scalar.Wheat, 10);
                Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(ptSecond.X - 10, ptSecond.Y + 10), new OpenCvSharp.CPlusPlus.Point(ptSecond.X + 10, ptSecond.Y - 10), Scalar.Wheat, 10);
                Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(ptThird.X - 10, ptThird.Y - 10), new OpenCvSharp.CPlusPlus.Point(ptThird.X + 10, ptThird.Y + 10), Scalar.Wheat, 10);
                Cv2.Line(matEllipseImg, new OpenCvSharp.CPlusPlus.Point(ptThird.X - 10, ptThird.Y + 10), new OpenCvSharp.CPlusPlus.Point(ptThird.X + 10, ptThird.Y - 10), Scalar.Wheat, 10);

                Cv2.ImShow("Find Circle", matEllipseImg);

                //int nOffset = 4;
                //int nPointNum = 0;
                //float fAvgX1 = 0, fAvgY1 = 0;
                //for(int n = 0; n < ltEdgePointFirst.Count; n++)
                //{
                //    if(n + nOffset + nOffset < ltEdgePointFirst.Count)
                //    {
                //        float x1 = ltEdgePointFirst[n].X;
                //        float y1 = ltEdgePointFirst[n].Y;
                //        float x2 = ltEdgePointFirst[n + nOffset].X;
                //        float y2 = ltEdgePointFirst[n + nOffset].Y;
                //        float x3 = ltEdgePointFirst[n + nOffset + nOffset].X;
                //        float y3 = ltEdgePointFirst[n + nOffset + nOffset].Y;
                //        if(y2 - y1 == 0 || y3 - y2 == 0)
                //        {
                //            continue;
                //        }

                //        float d1 = (x2 - x1) / (y2 - y1);
                //        float d2 = (x3 - x2) / (y3 - y2);
                //        float cx = ((y3 - y1) + (x2 + x3) * d2 - (x1 + x2) * d1) / (2 * (d2 - d1));
                //        float cy = -d1 * (cx - (x1 + x2) / 2) + (y1 + y2) / 2;

                //        fAvgX1 += cx;
                //        fAvgY1 += cy;
                //        nPointNum++;
                //    }
                //}
                //fAvgX1 /= nPointNum;
                //fAvgY1 /= nPointNum;

                //nPointNum = 0;
                //float fAvgX2 = 0, fAvgY2 = 0;
                //for (int n = 0; n < ltEdgePointSecond.Count; n++)
                //{
                //    if (n + nOffset + nOffset < ltEdgePointSecond.Count)
                //    {
                //        float x1 = ltEdgePointSecond[n].X;
                //        float y1 = ltEdgePointSecond[n].Y;
                //        float x2 = ltEdgePointSecond[n + nOffset].X;
                //        float y2 = ltEdgePointSecond[n + nOffset].Y;
                //        float x3 = ltEdgePointSecond[n + nOffset + nOffset].X;
                //        float y3 = ltEdgePointSecond[n + nOffset + nOffset].Y;
                //        if (y2 - y1 == 0 || y3 - y2 == 0)
                //        {
                //            continue;
                //        }

                //        float d1 = (x2 - x1) / (y2 - y1);
                //        float d2 = (x3 - x2) / (y3 - y2);
                //        float cx = ((y3 - y1) + (x2 + x3) * d2 - (x1 + x2) * d1) / (2 * (d2 - d1));
                //        float cy = -d1 * (cx - (x1 + x2) / 2) + (y1 + y2) / 2;

                //        fAvgX2 += cx;
                //        fAvgY2 += cy;
                //        nPointNum++;
                //    }
                //}
                //fAvgX2 /= nPointNum;
                //fAvgY2 /= nPointNum;

                //nPointNum = 0;
                //float fAvgX3 = 0, fAvgY3 = 0;
                //for (int n = 0; n < ltEdgePointThird.Count; n++)
                //{
                //    if (n + nOffset + nOffset < ltEdgePointThird.Count)
                //    {
                //        float x1 = ltEdgePointThird[n].X;
                //        float y1 = ltEdgePointThird[n].Y;
                //        float x2 = ltEdgePointThird[n + nOffset].X;
                //        float y2 = ltEdgePointThird[n + nOffset].Y;
                //        float x3 = ltEdgePointThird[n + nOffset + nOffset].X;
                //        float y3 = ltEdgePointThird[n + nOffset + nOffset].Y;
                //        if (y2 - y1 == 0 || y3 - y2 == 0)
                //        {
                //            continue;
                //        }

                //        float d1 = (x2 - x1) / (y2 - y1);
                //        float d2 = (x3 - x2) / (y3 - y2);
                //        float cx = ((y3 - y1) + (x2 + x3) * d2 - (x1 + x2) * d1) / (2 * (d2 - d1));
                //        float cy = -d1 * (cx - (x1 + x2) / 2) + (y1 + y2) / 2;

                //        fAvgX3 += cx;
                //        fAvgY3 += cy;
                //        nPointNum++;
                //    }
                //}
                //fAvgX3 /= nPointNum;
                //fAvgY3 /= nPointNum;

                //float fx1 = fAvgX1;
                //float fy1 = fAvgY1;
                //float fx2 = fAvgX2;
                //float fy2 = fAvgY2;
                //float fx3 = fAvgX3;
                //float fy3 = fAvgY3;
                //float fd1 = (fx2 - fx1) / (fy2 - fy1);
                //float fd2 = (fx3 - fx2) / (fy3 - fy2);
                //float fcx = ((fy3 - fy1) + (fx2 + fx3) * fd2 - (fx1 + fx2) * fd1) / (2 * (fd2 - fd1));
                //float fcy = -fd1 * (fcx - (fx1 + fx2) / 2) + (fy1 + fy2) / 2;

                float fx1 = rtFitFirst.Center.X;
                float fy1 = rtFitFirst.Center.Y;
                float fx2 = rtFitSecond.Center.X;
                float fy2 = rtFitSecond.Center.Y;
                float fx3 = rtFitThird.Center.X;
                float fy3 = rtFitThird.Center.Y;
                float fd1 = (fx2 - fx1) / (fy2 - fy1);
                float fd2 = (fx3 - fx2) / (fy3 - fy2);
                float fcx = ((fy3 - fy1) + (fx2 + fx3) * fd2 - (fx1 + fx2) * fd1) / (2 * (fd2 - fd1));
                float fcy = -fd1 * (fcx - (fx1 + fx2) / 2) + (fy1 + fy2) / 2;

                dDiameter = Math.Sqrt((Math.Abs(fcx - fx1) * Math.Abs(fcx - fx1)) + (Math.Abs(fcy - fy1) * Math.Abs(fcy - fy1)));

                CenterPoint.X = (int)Math.Round(fcx);
                CenterPoint.Y = (int)Math.Round(fcy);

                System.Windows.Forms.MessageBox.Show("pt1: " + fx1.ToString() + "," + fy1.ToString() + "\npt2: " + fx2.ToString() + "," + fy2.ToString() + "\npt3: " + fx3.ToString() + "," + fy3.ToString());

                return CenterPoint;
            }
        }*/

        private static List<List<double>> FindEdgePoints(Mat matImg, OpenCvSharp.CPlusPlus.Point2f[] ptDir, List<double> ptSeedPoint)
        {
            int nSearchLength = 0;
            int tx = 0;
            int ty = 0;
            int nGV = 0;
            float tvecx = 0;
            float tvecy = 0;

            List<List<double>> ltEdgePoint = new List<List<double>>();

            double nMinX = 99999999;
            double nMinY = 99999999;
            double nMaxX = -99999999;
            double nMaxY = -99999999;

            if (matImg.Width > matImg.Height)
            {
                nSearchLength = matImg.Width;
            }
            else
            {
                nSearchLength = matImg.Height;
            }

            //for (int i = 0; i < 72; i++)
            for (int i = 0; i < 360; i++)
            {
                tvecx = 0;
                tvecy = 0;

                for (int j = 0; j < nSearchLength; j++)
                {
                    tx = Convert.ToInt32(ptSeedPoint[0] + tvecx + 0.5);
                    ty = Convert.ToInt32(ptSeedPoint[1] + tvecy + 0.5);

                    if (tx <= 0 || tx >= matImg.Width)
                    {
                        break;
                    }

                    if (ty <= 0 || ty >= matImg.Height)
                    {
                        break;
                    }

                    nGV = matImg.Get<Byte>(ty, tx);

                    if (nGV == 0)
                    {
                        List<double> ptEdge = new List<double>();

                        ptEdge.Add(tx);
                        ptEdge.Add(ty);

                        ltEdgePoint.Add(ptEdge);

                        /*
                        // 나타난 점들 모집단으로 중심좌표 구해볼려고 - hjhwang 181114
                        if (ptEdge[0] >= nMaxX)
                            nMaxX = ptEdge[0];
                        else if (ptEdge[0] <= nMinX)
                            nMinX = ptEdge[0];

                        // 나타난 점들 모집단으로 중심좌표 구해볼려고 - hjhwang 181114
                        if (ptEdge[1] >= nMaxY)
                            nMaxY = ptEdge[1];
                        else if (ptEdge[1] <= nMinY)
                            nMinY = ptEdge[1];
                        */

                        break;
                    }

                    tvecx += ptDir[i].X;
                    tvecy += ptDir[i].Y;
                }
            }

            /*
             * 나타난 점들 모집단으로 중심좌표 구해볼려고 - hjhwang 181114
             * 
            List<double> ptCenter = new List<double>();
            ptCenter.Add((nMaxX + nMinX) * 0.5);
            ptCenter.Add((nMaxY + nMinY) * 0.5);
            m_ptInitCenter.Add(ptCenter);*/

            return ltEdgePoint;

        }

        // 다영찡 원본
        /*private static void FindEdge(Mat Img, OpenCvSharp.CPlusPlus.Point ptSeedPoint, OpenCvSharp.CPlusPlus.Point2f[] ptDir, ref List<OpenCvSharp.CPlusPlus.Point> ltEdgePoint)
        {
            int nSearchLength = 0;
            int tx = 0;
            int ty = 0;
            int nGV = 0;
            float tvecx = 0;
            float tvecy = 0;
            OpenCvSharp.CPlusPlus.Point ptEdge = new OpenCvSharp.CPlusPlus.Point();

            if (Img.Width > Img.Height)
            {
                nSearchLength = Img.Width;
            }
            else
            {
                nSearchLength = Img.Height;
            }

            for (int i = 0; i < 36; i++)
            {
                tvecx = 0;
                tvecy = 0;
                if (i == 38)
                {
                    int a = 5;
                }

                for (int j = 0; j < nSearchLength; j++)
                {
                    tx = Convert.ToInt32(ptSeedPoint.X + tvecx + 0.5);
                    ty = Convert.ToInt32(ptSeedPoint.Y + tvecy + 0.5);

                    if (tx <= 0 || tx >= Img.Width)
                    {
                        break;
                    }

                    if (ty <= 0 || ty >= Img.Height)
                    {
                        break;
                    }

                    nGV = Img.Get<Byte>(ty, tx);

                    if (nGV == 0)
                    {
                        ptEdge.X = tx;
                        ptEdge.Y = ty;

                        ltEdgePoint.Add(ptEdge);
                        break;
                    }

                    tvecx += ptDir[i].X;
                    tvecy += ptDir[i].Y;
                }
            }
        }*/

        private static OpenCvSharp.CPlusPlus.Point2f[] MakeSearchDir()  //일단 고정으로 간다 왜냐하면 급하니까 - 360으로 수정 - hjhwang 181204
        {
            double deg = 3.141592 / 180;

            //OpenCvSharp.CPlusPlus.Point2f[] ptDir = new OpenCvSharp.CPlusPlus.Point2f[72];
            //for (int i = 0; i < 72; i++)
            //{
            //    ptDir[i].X = (float)Math.Cos(deg * i * 5);
            //    ptDir[i].Y = (float)Math.Sin(deg * i * 5);
            //}

            OpenCvSharp.CPlusPlus.Point2f[] ptDir = new OpenCvSharp.CPlusPlus.Point2f[360];
            for (int i = 0; i < 360; i++)
            {
                ptDir[i].X = (float)Math.Cos(deg * i * 1);
                ptDir[i].Y = (float)Math.Sin(deg * i * 1);
            }

            return ptDir;
        }
    }
}