using RootTools.Database;
using RootTools_CLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
	public partial class Tools
	{
        public static List<Defect> MergeDefect(List<Defect> DefectList, int mergeDist)
        {
            string sInspectionID = DatabaseManager.Instance.GetInspectionID();
            List<Defect> MergeDefectList = new List<Defect>();
            int nDefectIndex = 1;

            for (int i = 0; i < DefectList.Count; i++)
            {
                if (DefectList[i].m_fSize == -123)
                    continue;

                for (int j = 0; j < DefectList.Count; j++)
                {
                    System.Windows.Rect defectRect1 = DefectList[i].p_rtDefectBox;
                    System.Windows.Rect defectRect2 = DefectList[j].p_rtDefectBox;

                    if (DefectList[j].m_fSize == -123 || (i == j))
                        continue;

                    else if (defectRect1.Contains(defectRect2))
                    {
                        DefectList[j].m_fSize = -123;
                        continue;
                    }
                    else if (defectRect2.Contains(defectRect1))
                    {
                        DefectList[i].SetDefectInfo(sInspectionID, DefectList[j].m_nDefectCode, DefectList[j].m_fSize, DefectList[j].m_fGV, DefectList[j].m_fWidth, DefectList[j].m_fHeight
                            , 0, 0, (float)DefectList[j].p_rtDefectBox.Left, (float)DefectList[j].p_rtDefectBox.Top, DefectList[j].m_nChipIndexX, DefectList[j].m_nCHipIndexY);
                        DefectList[j].m_fSize = -123;
                        continue;
                    }
                    else if (defectRect1.IntersectsWith(defectRect2))
                    {
                        System.Windows.Rect intersect = System.Windows.Rect.Intersect(defectRect1, defectRect2);
                        if (intersect.Height == 0 || intersect.Width == 0)
                        {
                            DefectList[j].m_fSize = -123;
                            continue;
                        }
                    }

                    defectRect1.Inflate(new System.Windows.Size(mergeDist, mergeDist)); // Rect 가로/세로 mergeDist 만큼 확장
                    if (defectRect1.IntersectsWith(defectRect2) && (DefectList[i].m_nDefectCode == DefectList[j].m_nDefectCode))
                    {
                        System.Windows.Rect intersect = System.Windows.Rect.Intersect(defectRect1, defectRect2);
                        if (intersect.Height == 0 || intersect.Width == 0) // Rect가 선만 겹쳐도 Intersect True가 됨! (실제 Dist보다 +1 만큼 더 되어 merge되는 것을 방지)
                            continue;

                        // Merge Defect Info
                        int nDefectCode = DefectList[j].m_nDefectCode;

                        float fDefectGV = (float)((DefectList[i].m_fGV + DefectList[j].m_fGV) / 2.0);
                        float fDefectLeft = (defectRect2.Left < defectRect1.Left + mergeDist) ? (float)defectRect2.Left : (float)defectRect1.Left + mergeDist;
                        float fDefectTop = (defectRect2.Top < defectRect1.Top + mergeDist) ? (float)defectRect2.Top : (float)defectRect1.Top + mergeDist;
                        float fDefectRight = (defectRect2.Right > defectRect1.Right - mergeDist) ? (float)defectRect2.Right : (float)defectRect1.Right - mergeDist;
                        float fDefectBottom = (defectRect2.Bottom > defectRect1.Bottom - mergeDist) ? (float)defectRect2.Bottom : (float)defectRect1.Bottom - mergeDist;

                        float fDefectWidth = fDefectRight - fDefectLeft;
                        float fDefectHeight = fDefectBottom - fDefectTop;

                        float fDefectSz = (fDefectWidth > fDefectHeight) ? fDefectWidth : fDefectHeight;

                        float fDefectRelX = 0;
                        float fDefectRelY = 0;

                        DefectList[i].SetDefectInfo(sInspectionID, nDefectCode, fDefectSz, fDefectGV, fDefectWidth, fDefectHeight
                            , fDefectRelX, fDefectRelY, fDefectLeft, fDefectTop, DefectList[j].m_nChipIndexX, DefectList[j].m_nCHipIndexY);

                        DefectList[j].m_fSize = -123; // Merge된 Defect이 중복 저장되지 않도록...
                    }
                }
            }

            for (int i = 0; i < DefectList.Count; i++)
            {
                if (DefectList[i].m_fSize != -123)
                {
                    MergeDefectList.Add(DefectList[i]);
                    MergeDefectList[nDefectIndex - 1].SetDefectIndex(nDefectIndex++);
                }
            }

            return MergeDefectList;
        }

        // 지울거야
        public static void SaveDefectImage(String Path, List<Defect> DefectList, SharedBufferInfo sharedBuffer, int nByteCnt)
        {
            Path += "\\";
            DirectoryInfo di = new DirectoryInfo(Path);
            if (!di.Exists)
                di.Create();

            if (DefectList.Count < 1)
                return;

            unsafe
            {
                Cpp_Rect[] defectArray = new Cpp_Rect[DefectList.Count];

                for (int i = 0; i < DefectList.Count; i++)
                {
                    Cpp_Rect rect = new Cpp_Rect();
                    rect.x = (int)DefectList[i].p_rtDefectBox.Left;
                    rect.y = (int)DefectList[i].p_rtDefectBox.Top;
                    rect.w = (int)DefectList[i].m_fWidth;
                    rect.h = (int)DefectList[i].m_fHeight;

                    defectArray[i] = rect;
                }

                if (nByteCnt == 1)
                {
                    CLR_IP.Cpp_SaveDefectListBMP(
                       Path,
                       (byte*)sharedBuffer.PtrR_GRAY.ToPointer(),
                       sharedBuffer.Width,
                       sharedBuffer.Height,
                       defectArray
                       );
                }

                else if (nByteCnt == 3)
                {
                    CLR_IP.Cpp_SaveDefectListBMP_Color(
                        Path,
                       (byte*)sharedBuffer.PtrR_GRAY.ToPointer(),
                       (byte*)sharedBuffer.PtrG.ToPointer(),
                       (byte*)sharedBuffer.PtrB.ToPointer(),
                       sharedBuffer.Width,
                       sharedBuffer.Height,
                       defectArray);
                }
            }
        }

        public static void SaveDefectImage(String path, List<Data> dataList, SharedBufferInfo sharedBuffer)
        {
            path += "\\";
            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.Exists)
                di.Create();

            if (dataList.Count < 1)
                return;

            unsafe
            {
                Cpp_Rect[] defectArray = new Cpp_Rect[dataList.Count];

                for (int i = 0; i < dataList.Count; i++)
                {
                    Cpp_Rect rect = new Cpp_Rect();
					rect.x = (int)dataList[i].GetRect().Left;
					rect.y = (int)dataList[i].GetRect().Top;
                    rect.w = (int)dataList[i].GetWidth();
					rect.h = (int)dataList[i].GetHeight();

					defectArray[i] = rect;
                }

                if (sharedBuffer.ByteCnt == 1)
                {
                    CLR_IP.Cpp_SaveDefectListBMP(
                       path,
                       (byte*)sharedBuffer.PtrR_GRAY.ToPointer(),
                       sharedBuffer.Width,
                       sharedBuffer.Height,
                       defectArray
                       );
                }

                else if (sharedBuffer.ByteCnt == 3)
                {
                    CLR_IP.Cpp_SaveDefectListBMP_Color(
                       path,
                       (byte*)sharedBuffer.PtrR_GRAY.ToPointer(),
                       (byte*)sharedBuffer.PtrG.ToPointer(),
                       (byte*)sharedBuffer.PtrB.ToPointer(),
                       sharedBuffer.Width,
                       sharedBuffer.Height,
                       defectArray);
                }
            }
        }

        public static void SaveDefectImage(String path, Data data, SharedBufferInfo sharedBuffer, int imageNum = 0)
		{
            path += "\\";
            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.Exists)
                di.Create();

            if (data == null)
                return;

            unsafe
            {
                Cpp_Rect rect = new Cpp_Rect();
                rect.x = (int)data.GetRect().Left;
                rect.y = (int)data.GetRect().Top;
                rect.w = (int)data.GetWidth();
                rect.h = (int)data.GetHeight();

                if (sharedBuffer.ByteCnt == 1)
                {
					CLR_IP.Cpp_SaveDefectListBMP(
					   path,
					   (byte*)sharedBuffer.PtrR_GRAY.ToPointer(),
					   sharedBuffer.Width,
					   sharedBuffer.Height,
					   rect,
					   imageNum);
				}

                else if (sharedBuffer.ByteCnt == 3)
                {
					CLR_IP.Cpp_SaveDefectListBMP_Color(
					   path,
					   (byte*)sharedBuffer.PtrR_GRAY.ToPointer(),
					   (byte*)sharedBuffer.PtrG.ToPointer(),
					   (byte*)sharedBuffer.PtrB.ToPointer(),
					   sharedBuffer.Width,
					   sharedBuffer.Height,
					   rect,
					   imageNum);
				}
            }
        }

        // 지울거야
        public static void SaveTiffImage(string Path, List<Defect> defectList, SharedBufferInfo sharedBuffer)
        {
            Path += "\\";
            DirectoryInfo di = new DirectoryInfo(Path);
            if (!di.Exists)
                di.Create();

            ArrayList inputImage = new ArrayList();
            for (int i = 0; i < defectList.Count; i++)
            {
				MemoryStream image = new MemoryStream();
                System.Drawing.Bitmap bitmap = Tools.ConvertArrayToColorBitmap(sharedBuffer.PtrR_GRAY, sharedBuffer.PtrG, sharedBuffer.PtrB, sharedBuffer.Width, sharedBuffer.ByteCnt, defectList[i].GetRect());
                //System.Drawing.Bitmap NewImg = new System.Drawing.Bitmap(bitmap);
                bitmap.Save(image, ImageFormat.Tiff);
                inputImage.Add(image);
            }

            ImageCodecInfo info = null;
            foreach (ImageCodecInfo ice in ImageCodecInfo.GetImageEncoders())
            {
                if (ice.MimeType == "image/tiff")
                {
                    info = ice;
                    break;
                }
            }

            string test = "test";
            Path += test + ".tiff";

            EncoderParameters ep = new EncoderParameters(2);

            bool firstPage = true;

            System.Drawing.Image img = null;

            for (int i = 0; i < inputImage.Count; i++)
            {
                System.Drawing.Image img_src = System.Drawing.Image.FromStream((Stream)inputImage[i]);
                Guid guid = img_src.FrameDimensionsList[0];
                System.Drawing.Imaging.FrameDimension dimension = new System.Drawing.Imaging.FrameDimension(guid);

                for (int nLoopFrame = 0; nLoopFrame < img_src.GetFrameCount(dimension); nLoopFrame++)
                {
                    img_src.SelectActiveFrame(dimension, nLoopFrame);

                    ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, Convert.ToInt32(EncoderValue.CompressionLZW));

                    if (firstPage)
                    {
                        img = img_src;

                        ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.MultiFrame));
                        img.Save(Path, info, ep);

                        firstPage = false;
                        continue;
                    }

                    ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.FrameDimensionPage));
                    img.SaveAdd(img_src, ep);
                }
            }
            if (inputImage.Count == 0)
            {
                File.Create(Path);
                return;
            }

            ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.Flush));
            img.SaveAdd(ep);
        }

        public static void SaveTiffImage(string Path, List<Data> dataList, SharedBufferInfo sharedBuffer)
        {
            Path += "\\";
            DirectoryInfo di = new DirectoryInfo(Path);
            if (!di.Exists)
                di.Create();

            ArrayList inputImage = new ArrayList();
            for (int i = 0; i < dataList.Count; i++)
            {
                MemoryStream image = new MemoryStream();
				System.Drawing.Bitmap bitmap = Tools.ConvertArrayToColorBitmap(sharedBuffer.PtrR_GRAY, sharedBuffer.PtrG, sharedBuffer.PtrB, sharedBuffer.Width, sharedBuffer.ByteCnt, dataList[i].GetRect());
				//System.Drawing.Bitmap NewImg = new System.Drawing.Bitmap(bitmap);
				bitmap.Save(image, ImageFormat.Tiff);
                inputImage.Add(image);
            }

            ImageCodecInfo info = null;
            foreach (ImageCodecInfo ice in ImageCodecInfo.GetImageEncoders())
            {
                if (ice.MimeType == "image/tiff")
                {
                    info = ice;
                    break;
                }
            }

            string test = "test";
            Path += test + ".tiff";

            EncoderParameters ep = new EncoderParameters(2);

            bool firstPage = true;

            System.Drawing.Image img = null;

            for (int i = 0; i < inputImage.Count; i++)
            {
                System.Drawing.Image img_src = System.Drawing.Image.FromStream((Stream)inputImage[i]);
                Guid guid = img_src.FrameDimensionsList[0];
                System.Drawing.Imaging.FrameDimension dimension = new System.Drawing.Imaging.FrameDimension(guid);

                for (int nLoopFrame = 0; nLoopFrame < img_src.GetFrameCount(dimension); nLoopFrame++)
                {
                    img_src.SelectActiveFrame(dimension, nLoopFrame);

                    ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, Convert.ToInt32(EncoderValue.CompressionLZW));

                    if (firstPage)
                    {
                        img = img_src;

                        ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.MultiFrame));
                        img.Save(Path, info, ep);

                        firstPage = false;
                        continue;
                    }

                    ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.FrameDimensionPage));
                    img.SaveAdd(img_src, ep);
                }
            }
            if (inputImage.Count == 0)
            {
                File.Create(Path);
                return;
            }

            ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.Flush));
            img.SaveAdd(ep);
        }

    }
}
