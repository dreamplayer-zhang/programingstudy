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

        public static void SaveDataImage(String path, List<Data> dataList, SharedBufferInfo sharedBuffer)
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
