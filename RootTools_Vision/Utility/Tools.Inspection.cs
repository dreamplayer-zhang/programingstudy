using RootTools.Database;
using RootTools_CLR;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

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
                    int defectCode1 = DefectList[i].m_nDefectCode;
                    int defectCode2 = DefectList[j].m_nDefectCode;
                    
                    if (defectCode1 != defectCode2)
                        continue;

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
                            , 0, 0, (float)DefectList[j].p_rtDefectBox.Left, (float)DefectList[j].p_rtDefectBox.Top, DefectList[j].m_nChipIndexX, DefectList[j].m_nChipIndexY);
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
                            , fDefectRelX, fDefectRelY, fDefectLeft, fDefectTop, DefectList[j].m_nChipIndexX, DefectList[j].m_nChipIndexY);

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

        public static void SaveDefectImageParallel(String path, List<Defect> defectList, SharedBufferInfo sharedBuffer, int nByteCnt)
        {
            path += "\\";
            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.Exists)
                di.Create();

            if (defectList.Count < 1)
                return;


            int imageSizeX = 160;
            int imageSizeY = 120;

            Parallel.ForEach(defectList, defect =>
            {
                double cx = (defect.p_rtDefectBox.Left + defect.p_rtDefectBox.Right) / 2;
                double cy = (defect.p_rtDefectBox.Top + defect.p_rtDefectBox.Bottom) / 2;
                int startX = (int)cx - imageSizeX / 2;
                int startY = (int)cy - imageSizeY / 2;
                //int endX = startX + 640;
                //int endY = startY + 480;

                System.Drawing.Bitmap bitmap = CovertBufferToBitmap(sharedBuffer, new System.Windows.Rect(startX, startY, imageSizeX, imageSizeY));

                if (System.IO.File.Exists(path + defect.m_nDefectIndex + ".bmp"))
                    System.IO.File.Delete(path + defect.m_nDefectIndex + ".bmp");

                bitmap.Save(path + defect.m_nDefectIndex + ".bmp");
            });
        }
        public static void SaveDefectImageParallel(String path, List<Defect> defectList, SharedBufferInfo sharedBuffer, int nByteCnt,Point size)
        {
            path += "\\";
            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.Exists)
                di.Create();

            if (defectList.Count < 1)
                return;


            int imageSizeX = (int)size.X;
            int imageSizeY = (int)size.Y;

            int idx = 0;
            foreach(Defect defect in defectList)
            {
                double cx = (defect.p_rtDefectBox.Left + defect.p_rtDefectBox.Right) / 2;
                double cy = (defect.p_rtDefectBox.Top + defect.p_rtDefectBox.Bottom) / 2;
                int startX = (int)cx - imageSizeX / 2;
                int startY = (int)cy - imageSizeY / 2;
                //int endX = startX + 640;
                //int endY = startY + 480;

                System.Drawing.Bitmap bitmap = CovertBufferToBitmap(sharedBuffer, new System.Windows.Rect(startX, startY, imageSizeX, imageSizeY));

                if (System.IO.File.Exists(path + defect.m_nDefectIndex + ".bmp"))
                    System.IO.File.Delete(path + defect.m_nDefectIndex + ".bmp");

                bitmap.Save(path + idx++ + ".bmp");
            }

        }
        public static void SaveDefectImageParallel(String path, List<Measurement> measurementList, SharedBufferInfo sharedBuffer, int nByteCnt, Size size = new Size())
        {
			path += "\\";
			DirectoryInfo di = new DirectoryInfo(path);
			if (!di.Exists)
				di.Create();

			if (measurementList.Count < 1)
				return;

			Parallel.ForEach(measurementList, measure =>
			{
				double cx = (measure.p_rtDefectBox.Left + measure.p_rtDefectBox.Right) / 2;
				double cy = (measure.p_rtDefectBox.Top + measure.p_rtDefectBox.Bottom) / 2;

				int startX = (int)cx - 320;
				int startY = (int)cy - 240;
				int width = 640;
				int height = 480;

				System.Windows.Rect imageRect = new System.Windows.Rect(startX, startY, width, height);

				if (size != Size.Empty)
				{
					startX = (int)(cx - (size.Width / 2));
					if (startX < 0)
						startX = 0;

					startY = (int)(cy - (size.Height / 2));
					width = (int)size.Width;
					height = (int)size.Height;
				}

				System.Drawing.Bitmap bitmap = CovertBufferToBitmap(sharedBuffer, new System.Windows.Rect(startX, startY, width, height));

				lock (lockObj)
				{
					if (System.IO.File.Exists(path + measure.m_nMeasurementIndex + ".bmp"))
						System.IO.File.Delete(path + measure.m_nMeasurementIndex + ".bmp");

					bitmap.Save(path + measure.m_nMeasurementIndex + ".bmp");
				}
			});
		}

        private static object lockObj = new object();

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

        public static void WriteTextToBitmap(System.Drawing.Bitmap sourceBitmap, System.Drawing.Font font, System.Drawing.Brush brush, System.Drawing.Point location, string text)
        {
            System.Drawing.Graphics bitmapGraphics = System.Drawing.Graphics.FromImage(sourceBitmap);
            bitmapGraphics.DrawString(text, font, brush, location);
            bitmapGraphics.Dispose();
        }

        public static void WriteRectToBitmap(System.Drawing.Bitmap sourceBitmap, System.Drawing.Pen pen,System.Drawing.Rectangle rect)
        {
            System.Drawing.Graphics bitmapGraphics = System.Drawing.Graphics.FromImage(sourceBitmap);
            //bitmapGraphics.DrawString(text, font, brush, location);
            bitmapGraphics.DrawRectangle(pen, rect);
            bitmapGraphics.Dispose();
        }

        public static object lockTiffObj = new object();
        public static void SaveTiffImage(string Path, string fileName, List<Defect> defectList, SharedBufferInfo sharedBuffer, Size imageSize = default(Size))
        {
            Path += "\\";
            DirectoryInfo di = new DirectoryInfo(Path);
            if (!di.Exists)
                di.Create();

            ArrayList inputImage = new ArrayList();

            int tiffWidth = 160;
            int tiffHeight = 120;
            if (imageSize != default(Size))
            {
                tiffWidth = (int)imageSize.Width;
                tiffHeight = (int)imageSize.Height;
            }

            //Parallel.ForEach(defectList, defect =>
            foreach (Defect defect in defectList)
            {
                Rect defectRect = new Rect(
                    defect.m_fAbsX - tiffWidth / 2,
                    defect.m_fAbsY - tiffHeight / 2,
                    tiffWidth,
                    tiffHeight);

                MemoryStream image = new MemoryStream();
                System.Drawing.Bitmap bitmap = Tools.CovertBufferToBitmap(sharedBuffer, defectRect);
                WriteTextToBitmap(bitmap, new System.Drawing.Font("돋움체", 72f, System.Drawing.FontStyle.Bold), System.Drawing.Brushes.White, new System.Drawing.Point(20, 20), "테스트");
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

            Path += fileName + ".tiff";

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
                        lock (lockTiffObj) img.Save(Path, info, ep);

                        firstPage = false;
                        continue;
                    }

                    ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.FrameDimensionPage));
                    lock (lockTiffObj) img.SaveAdd(img_src, ep);
                }
            }
            if (inputImage.Count == 0)
            {
                lock (lockTiffObj) File.Create(Path);
                return;
            }

            ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.Flush));
            lock (lockTiffObj) img.SaveAdd(ep);
        }


        public static void SaveTiffImageOnlyTDI(string Path, string fileName, List<Defect> defectList, SharedBufferInfo sharedBuffer, Size imageSize = default(Size))
        {
            Path += "\\";
            DirectoryInfo di = new DirectoryInfo(Path);
            if (!di.Exists)
                di.Create();

            ArrayList inputImage = new ArrayList();

            int tiffWidth = 160;
            int tiffHeight = 120;
            if(imageSize != default(Size))
            {
                tiffWidth = (int)imageSize.Width;
                tiffHeight = (int)imageSize.Height;
            }

            //Parallel.ForEach(defectList, defect =>
            foreach (Defect defect in defectList)
            {
                Rect defectRect = new Rect(
                    defect.m_fAbsX - tiffWidth / 2,
                    defect.m_fAbsY - tiffHeight / 2,
                    tiffWidth,
                    tiffHeight);

                MemoryStream image = new MemoryStream();
                System.Drawing.Bitmap bitmap = Tools.CovertBufferToBitmap(sharedBuffer, defectRect);
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

            Path += fileName + ".tiff";

            EncoderParameters ep = new EncoderParameters(2);

            bool firstPage = true;

            System.Drawing.Image img = null;
            lock (lockTiffObj)
            {
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
                        lock (lockTiffObj) img.SaveAdd(img_src, ep);
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


        // 지울거야
        public static void SaveTiffImageBoth(string Path, string fileName, List<Defect> defectList, SharedBufferInfo sharedBuffer, Size imageSize, ConcurrentQueue<byte[]> vrsImageQueue, Size vrsImageSize)
        {
            Path += "\\";
            DirectoryInfo di = new DirectoryInfo(Path);
            if (!di.Exists)
                di.Create();

            ArrayList inputImage = new ArrayList();

            int tiffWidth = (int)imageSize.Width;
            int tiffHeight = (int)imageSize.Height;

            if (vrsImageQueue == null)
            {
                MessageBox.Show("VRS Imaage Queue == null");
                return;
            }

            if ((vrsImageQueue.Count != defectList.Count) || vrsImageSize == default(Size))
            {
                MessageBox.Show("VRS Review Image와 Defect의 수가 다릅니다.");
                return;
            }

            if (vrsImageSize == default(Size))
            {
                MessageBox.Show("VRS Review Image Size를 설정해주어야합니다.");
                return;
            }

            //Parallel.ForEach(defectList, defect =>
            foreach (Defect defect in defectList)
            {
                Rect defectRect = new Rect(
                    defect.m_fAbsX - tiffWidth / 2,
                    defect.m_fAbsY - tiffHeight / 2,
                    tiffWidth,
                    tiffHeight);

                MemoryStream image = new MemoryStream();
                System.Drawing.Bitmap bitmap = Tools.CovertBufferToBitmap(sharedBuffer, defectRect);
                //System.Drawing.Bitmap NewImg = new System.Drawing.Bitmap(bitmap);
                bitmap.Save(image, ImageFormat.Tiff);
                inputImage.Add(image);


                byte[] colorImage = null;
                if(vrsImageQueue.TryDequeue(out colorImage) == true)
                {
                    MemoryStream ms = new MemoryStream();
                    System.Drawing.Bitmap vrsBmp = Tools.CovertArrayToBitmap(colorImage, (int)vrsImageSize.Width, (int)vrsImageSize.Height, 3);

                    vrsBmp.Save(ms, ImageFormat.Tiff);
                    inputImage.Add(ms);
                }
                else
                {
                    TempLogger.Write("Error", "Save Klarf image - VRS Image Dequeue Fail!!");
                }
            }
            //for (int i = 0; i < defectList.Count; i++)
            //{
            //    Rect defectRect = new Rect(
            //        (defectList[i].p_rtDefectBox.Left + defectList[i].p_rtDefectBox.Right) / 2 - tiffWidth / 2,
            //        (defectList[i].p_rtDefectBox.Top + defectList[i].p_rtDefectBox.Bottom) / 2 - tiffHeight / 2,
            //        tiffWidth,
            //        tiffHeight);

            //    MemoryStream image = new MemoryStream();
            //    System.Drawing.Bitmap bitmap = Tools.CovertBufferToBitmap(sharedBuffer, defectRect);
            //    //System.Drawing.Bitmap NewImg = new System.Drawing.Bitmap(bitmap);
            //    bitmap.Save(image, ImageFormat.Tiff);
            //    inputImage.Add(image);
            //}

            ImageCodecInfo info = null;
            foreach (ImageCodecInfo ice in ImageCodecInfo.GetImageEncoders())
            {
                if (ice.MimeType == "image/tiff")
                {
                    info = ice;
                    break;
                }
            }
            
            Path += fileName + ".tiff";

            EncoderParameters ep = new EncoderParameters(2);

            bool firstPage = true;

            System.Drawing.Image img = null;
            lock (lockTiffObj)
            {
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

        public static void SaveTiffImageOnlyVRS(string Path, string fileName, List<Defect> defectList, ConcurrentQueue<byte[]> vrsImageQueue, Size vrsImageSize)
        {
            Path += "\\";
            DirectoryInfo di = new DirectoryInfo(Path);
            if (!di.Exists)
                di.Create();

            ArrayList inputImage = new ArrayList();

            if (vrsImageQueue == null)
            {
                MessageBox.Show("VRS Image == Null");
                return;
            }

            if ((vrsImageQueue.Count != defectList.Count) || vrsImageSize == default(Size))
            {
                MessageBox.Show("VRS Review Image와 Defect의 수가 다릅니다.");
                return;
            }

            if (vrsImageSize == default(Size))
            {
                MessageBox.Show("VRS Review Image Size를 설정해주어야합니다.");
                return;
            }

            //Parallel.ForEach(defectList, defect =>
            foreach (Defect defect in defectList)  // 이거 나중에 정보 필요할수 있음
            {
                byte[] colorImage = null;
                if(vrsImageQueue.TryDequeue(out colorImage) == true)
                {
                    MemoryStream ms = new MemoryStream();
                    System.Drawing.Bitmap vrsBmp = Tools.CovertArrayToBitmap(colorImage, (int)vrsImageSize.Width, (int)vrsImageSize.Height, 3);

                    vrsBmp.Save(ms, ImageFormat.Tiff);
                    inputImage.Add(ms);
                }
                else
                {
                    TempLogger.Write("Error", "Save Klarf image - VRS Image Dequeue Fail!!");
                }
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
            lock(lockTiffObj)
            {
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
                System.Drawing.Bitmap bitmap = Tools.CovertBufferToBitmap(sharedBuffer, dataList[i].GetRect());
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
