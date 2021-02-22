using RootTools;
using RootTools.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace RootTools_Vision
{
    public class Tools
    {
        public static byte[] CovertImageToArray(Image img)
        {
            byte[] data;

            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

                data = new byte[ms.Length];

                data = ms.ToArray();
            }

            return data;
        }

        public static Bitmap ConvertArrayToBitmapRect(byte[] rawData, int _width, int _height, int _byteCount, CRect _rect)
        {
            try
            {
                System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                if (_byteCount == 1)
                {
                    format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                }
                else if (_byteCount == 3)
                {
                    format = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                }
                else
                {
                    System.Windows.MessageBox.Show("지원하지 않는 PixelFormat입니다.");
                    return null;
                }

                int stride = (int)Math.Ceiling((double)_width / 4) * 4;
                Bitmap bmp = new Bitmap(_width, _height, format);
                ColorPalette palette = bmp.Palette;
                if (_byteCount == 1)
                {
                    for (int i = 0; i < 256; i++)
                        palette.Entries[i] = Color.FromArgb(i, i, i);

                    bmp.Palette = palette;
                }
                else
                {
                    //Color Palette 만들줄 아는사람 넣어줘
                }

            }
            catch
            {

            }
            return null;
        }

        public static Bitmap ConvertArrayToColorBitmap(IntPtr rawDataR, IntPtr rawDataG, IntPtr rawDataB, int _memWidth, int _byteCount, Rect rect)
        {
            try
            {
                System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                if (_byteCount == 1)
                {
                    format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                }
                else if (_byteCount == 3)
                {
                    format = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                }
                else
                {
                    System.Windows.MessageBox.Show("지원하지 않는 PixelFormat입니다.");
                    return null;
                }

                int stride = (int)Math.Ceiling((double)640 / 4) * 4;
                Bitmap bmp = new Bitmap(640, 480, format);

                ColorPalette palette = bmp.Palette;

                if (_byteCount == 1)
                {
                    for (int i = 0; i < 256; i++)
                        palette.Entries[i] = Color.FromArgb(i, i, i);

                    bmp.Palette = palette;
                }
                else
                {
                    //Color Palette 만들줄 아는사람 넣어줘
                }
                //Marshal.Copy(,);


                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, 640, 480), ImageLockMode.WriteOnly, format);

                int centerX = (int)(rect.X + (rect.Width / 2));
                int centerY = (int)(rect.Y + (rect.Height / 2));

                Rect rt = new Rect();
                int saveW = 640;
                int saveH = 480;
                if(rect.Width < saveW && rect.Height < saveH)
                {
                    rt.X = centerX - (saveW / 2);
                    rt.Y = centerY - (saveH / 2);
                    rt.Width = saveW;
                    rt.Height = saveH;
                }
                else
                {
                    int nSaveW;
                    int nSaveH;
                    if (rect.Width > saveW)
                        nSaveW = (int)rect.Width;
                    else
                        nSaveW = 640;

                    if (rect.Height > saveH)
                        nSaveH = (int)rect.Height;
                    else
                        nSaveH = 480;

                    rt.X = (centerX - saveW / 2);
                    rt.Y = (centerY - saveH / 2);
                    rt.Width = nSaveW;
                    rt.Height = nSaveH;
                }
                


                IntPtr pointer = bmpData.Scan0;
                {
                    unsafe
                    {
                        byte* pPointer = (byte*)pointer.ToPointer();
                        byte* pR = (byte*)rawDataR.ToPointer();
                        byte* pG = (byte*)rawDataG.ToPointer();
                        byte* pB = (byte*)rawDataB.ToPointer();
                        for (int i = 0; i < 480; i++)
                            for (int j = 0; j < 640; j++)
                            {
                                pPointer[i * (saveW * 3) + j * _byteCount + 0] = pB[(i + (int)(rt.Y)) * _memWidth + (j + (int)(rt.X))];
                                pPointer[i * (saveW * 3) + j * _byteCount + 1] = pG[(i + (int)(rt.Y)) * _memWidth + (j + (int)(rt.X))];
                                pPointer[i * (saveW * 3) + j * _byteCount + 2] = pR[(i + (int)(rt.Y)) * _memWidth + (j + (int)(rt.X))];
                            }
                    }
                }
                bmp.UnlockBits(bmpData);

                if (rt.Width != 640 || rt.Height != 480)
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.DrawImage(bmp, 0, 0, saveW, saveH);
                    }
                }

                return bmp;
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.ToString());
            }
            return null;
        }

        public static Bitmap CovertArrayToBitmap(byte[] rawdata, int _width, int _height, int _byteCount)
        {
            try
            {
                System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                if (_byteCount == 1)
                {
                    format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                }
                else if (_byteCount == 3)
                {
                    format = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                }
                else
                {
                    System.Windows.MessageBox.Show("지원하지 않는 PixelFormat입니다.");
                    return null;
                }

                int stride = (int)Math.Ceiling((double)_width / 4) * 4;
                Bitmap bmp = new Bitmap(_width, _height, format);

                ColorPalette palette = bmp.Palette;

                if (_byteCount == 1)
                {
                    for (int i = 0; i < 256; i++)
                        palette.Entries[i] = Color.FromArgb(i, i, i);

                    bmp.Palette = palette;
                }
                else
                {
                    //Color Palette 만들줄 아는사람 넣어줘
                }
              

                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.WriteOnly, format);

                IntPtr pointer = bmpData.Scan0;

                //Marshal.Copy(rawdata, 0, pointer, rawdata.Length);

                if (_byteCount == 1)
                {
                    for (int i = 0; i < _height; i++)
                        Marshal.Copy(rawdata, i * _width, pointer + i * bmpData.Stride, _width);
                }
                else if (_byteCount == 3)
                {
                    unsafe
                    {
                        byte* pPointer = (byte*)pointer.ToPointer();
                        for (int i = 0; i < _height; i++)
                            for (int j = 0; j < _width; j++)
                            {
                                pPointer[i * bmpData.Stride + j * _byteCount + 0] = rawdata[i * _width * _byteCount + j * _byteCount + 2];
                                pPointer[i * bmpData.Stride + j * _byteCount + 1] = rawdata[i * _width * _byteCount + j * _byteCount + 1];
                                pPointer[i * bmpData.Stride + j * _byteCount + 2] = rawdata[i * _width * _byteCount + j * _byteCount + 0];
                            }
                    }
                }
                bmp.UnlockBits(bmpData);

                return bmp;
            }
            catch (Exception)
            {

            }
            return null;
        }

        public static bool SaveRawdataToBitmap(string filepath, byte[] rawdata, int _width, int _height, int _byteCount)
        {
            bool rst = true;
            try
            {
                Bitmap bmp = CovertArrayToBitmap(rawdata, _width, _height, _byteCount);
                bmp.Save(filepath);
            }
            catch (Exception)
            {
                rst = false;
            }

            return rst;
        }

        
        public unsafe static byte[] LoadBitmapToRawdata(string filepath, int* _width, int* _height)
        {
            byte[] resData = null;
            //bool res = false;
            try
            {
                int byteCount = 1;
                Bitmap bmp = new Bitmap(filepath);

                *_width = bmp.Width;
                *_height = bmp.Height;

                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, *_width, *_height), ImageLockMode.WriteOnly, bmp.PixelFormat);

                

                if (bmp.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    byteCount = 1;
                }
                else
                {
                    byteCount = 3;
                }

                resData = new byte[bmp.Width * bmp.Height * byteCount];
                if (byteCount == 1)
                {
                    //IntPtr[] ptr = new IntPtr[bmp.Width * bmp.Height * byteCount];
                    IntPtr pointer = bmpData.Scan0;
                    Parallel.For(0, *_height - 1, (i) =>
                    {
                        Marshal.Copy(pointer + bmpData.Stride * i, resData, i * *_width, *_width * byteCount);
                        //rawdata. (byte*)ptr[i].ToPointer();
                    });
                   
                    
                }
                else
                {
                    unsafe
                    {
                        IntPtr pointer = bmpData.Scan0;
                        byte* pPointer = (byte*)pointer.ToPointer();

                        Parallel.For(0, *_height - 1, (i) =>
                        {
                            for (int j = 0; j < *_width; j++)
                            {
                                resData[i * *_width * byteCount + j * byteCount + 2] = pPointer[i * bmpData.Stride + j * byteCount + 0];
                                resData[i * *_width * byteCount + j * byteCount + 1] = pPointer[i * bmpData.Stride + j * byteCount + 1];
                                resData[i * *_width * byteCount + j * byteCount + 0] = pPointer[i * bmpData.Stride + j * byteCount + 2];
                            }
                        });
                    }
                }

                
            }
            catch (Exception)
            {
                resData = null;
               // res = false;
            }
            return resData;
        }

        public static void SpliteColor(byte[] srcColor, byte[] dstR, byte[] dstG, byte[] dstB)
        {
            for(int i = 0; i < srcColor.Length/3; i++)
            {
                dstR[i] = srcColor[i * 3];
                dstG[i] = srcColor[i * 3 + 1];
                dstB[i] = srcColor[i * 3 + 2];
            }
        }
        public static void ConvertRGB2Gray(byte[] srcColor, byte[] dstGray)
        {
            for (int i = 0; i < srcColor.Length / 3; i++)
            {
                dstGray[i] = (byte)(0.299 * srcColor[i * 3] + 0.587 * srcColor[i * 3 + 1] + 0.114 * srcColor[i * 3 + 2]);
            }
        }
        public static bool LoadBitmapToRawdata(string filepath, byte[] rawdata, int _width, int _height, int _byteCount)
        {
            //StopWatch stop = new StopWatch();
            //stop.Start();
            bool rst = true;
            try
            {
                Bitmap bmp = new Bitmap(filepath);
                
                // Raw Copy
                //rawdata = new byte[width * _height * _byteCount];
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.WriteOnly, bmp.PixelFormat);


                if (_byteCount == 1)
                {
                    IntPtr pointer = bmpData.Scan0;
                    //for (int i = 0; i < _height; i++)
                    //    Marshal.Copy(pointer + bmpData.Stride * i, rawdata, i * _width, _width * _byteCount);

                    Parallel.For(0, _height - 1, (i) =>
                    {
                        Marshal.Copy(pointer + bmpData.Stride * i, rawdata, i * _width, _width * _byteCount);
                    });
                }
                else
                {
                    unsafe
                    {
                        IntPtr pointer = bmpData.Scan0;
                        byte* pPointer = (byte*)pointer.ToPointer();

                        Parallel.For(0, _height - 1, (i) =>
                        {
                            for (int j = 0; j < _width; j++)
                            {
                                rawdata[i * _width * _byteCount + j * _byteCount + 2] = pPointer[i * bmpData.Stride + j * _byteCount + 0];
                                rawdata[i * _width * _byteCount + j * _byteCount + 1] = pPointer[i * bmpData.Stride + j * _byteCount + 1];
                                rawdata[i * _width * _byteCount + j * _byteCount + 0] = pPointer[i * bmpData.Stride + j * _byteCount + 2];
                            }
                        });
                    }
                }

            }
            catch( Exception)
            {
                rst = false;
            }
            //stop.Stop();
            //MessageBox.Show(stop.ElapsedMilliseconds.ToString());

            return rst;
        }


        public static bool LoadBitmapToRawdata(string filepath, byte[] rawdata, ref int _width, ref int _height, ref int _byteCount)
        {
            bool rst = true;
            try
            {
                Bitmap bmp = new Bitmap(filepath);

                
                int width = bmp.Width;
                int height = bmp.Height;
                
                _width = width;
                _height = height;

                int byteCount = 1;
                if (bmp.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    byteCount = 1;
                }
                else
                {
                    byteCount = 3;
                }

                _byteCount = byteCount;

                // Raw Copy
                //rawdata = new byte[width * _height * _byteCount];
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.WriteOnly, bmp.PixelFormat);


                if(_byteCount == 1)
                {
                    IntPtr pointer = bmpData.Scan0;
                    //for (int i = 0; i < _height; i++)
                    //    Marshal.Copy(pointer + bmpData.Stride * i, rawdata, i * _width, _width * _byteCount);

                    Parallel.For(0, _height - 1, (i) =>
                    {
                        Marshal.Copy(pointer + bmpData.Stride * i, rawdata, i * width, width * byteCount);
                    });
                }
                else
                {
                    unsafe
                    {
                        IntPtr pointer = bmpData.Scan0;
                        byte* pPointer = (byte*)pointer.ToPointer();

                        for (int i = 0; i < _height; i++)
                        {
                            //for (int j = 0; j < _width; j++)
                            //{
                                Parallel.For(0, _width - 1, (j) =>
                                {
                                    rawdata[i * width * byteCount + j * byteCount + 2] = pPointer[i * bmpData.Stride + j * byteCount + 0];
                                    rawdata[i * width * byteCount + j * byteCount + 1] = pPointer[i * bmpData.Stride + j * byteCount + 1];
                                    rawdata[i * width * byteCount + j * byteCount + 0] = pPointer[i * bmpData.Stride + j * byteCount + 2];
                                });

                                //rawdata[i * _width * _byteCount + j * _byteCount + 2] = pPointer[i * bmpData.Stride + j * _byteCount + 0];
                                //rawdata[i * _width * _byteCount + j * _byteCount + 1] = pPointer[i * bmpData.Stride + j * _byteCount + 1];
                                //rawdata[i * _width * _byteCount + j * _byteCount + 0] = pPointer[i * bmpData.Stride + j * _byteCount + 2];
                            //}
                        }
                    }
                }

            }
            catch (Exception)
            {
                rst = false;
            }

            return rst;
        }

        public static ObservableCollection<Type> GetInheritedClasses(Type _type, string filter = "")
        {
            IEnumerable<Type> type = Assembly.GetAssembly(_type).GetTypes().Where(TheType => TheType.IsClass && !TheType.IsAbstract && TheType.IsSubclassOf(_type));
            //if you want the abstract classes drop the !TheType.IsAbstract but it is probably to instance so its a good idea to keep it.

            ObservableCollection<Type> typeList = new ObservableCollection<Type>();
            foreach (Type t in type)
            {
                if (filter != "" && t.Name.Contains(filter) == true)
                    continue;

                typeList.Add(t);
            }

            return typeList;
        }

        public static ObservableCollection<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class, IComparable<T>
        {
            ObservableCollection<T> objects = new ObservableCollection<T>();
            foreach (Type type in
                Assembly.GetAssembly(typeof(T)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                objects.Add((T)Activator.CreateInstance(type, constructorArgs));
            }
            return objects;
        }

        public static byte[] ObjectToByteArray<T>(T obj)
        {
            MemoryStream ms = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            return ms.ToArray();
        }

        public static byte[] ObejctToByteArray(object obj)
        {
            MemoryStream ms = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            return ms.ToArray();
        }

        public static T ByteArrayToObject<T>(byte[] byteArr)
        {
            if (byteArr == null) return default(T);

            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            ms.Write(byteArr, 0, byteArr.Length);
            ms.Seek(0, SeekOrigin.Begin);
            return (T)bf.Deserialize(ms);
        }

        public static object ByteArrayToObject(byte[] byteArr)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(byteArr))
                {
                    stream.Position = 0;
                    BinaryFormatter bf = new BinaryFormatter();
                    return bf.Deserialize(stream);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

            return null;
            //if (byteArr == null) return null;

            //MemoryStream ms = new MemoryStream();
            //BinaryFormatter bf = new BinaryFormatter();
            //ms.Write(byteArr, 0, byteArr.Length);
            //ms.Seek(0, SeekOrigin.Begin);
            //object obj = (object)bf.Deserialize(ms);

            //return obj;
        }

        public static T CreateInstance<T>()
        {
            return Activator.CreateInstance<T>();
        }

        public static object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }

        public static void ParallelImageCopy(IntPtr ptrSrc, int srcStride, int srcHeight, CRect roi, byte[] byteDst)
        {
            try
            {
                int top = roi.Top;
                int bottom = roi.Bottom;
                int left = roi.Left;
                int right = roi.Right;
                int width = roi.Width;
                int height = roi.Height;

                Parallel.For(top, bottom, (i) =>
                {
                    if (byteDst == null)
                        return;

                    Marshal.Copy(new IntPtr(ptrSrc.ToInt64() + (i * (Int64)srcStride + left)), byteDst, width * (i - top), width);

                });

            }
            catch(Exception)
            {
                //검사 종료할 경우 buffer 카피하다가 workplace가 reset되서 다운
            }
        }

        public static System.Windows.Media.Imaging.BitmapSource ConvertBitmapToSource(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = System.Windows.Media.Imaging.BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                System.Windows.Media.PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            return bitmapSource;
        }

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
    }
}
