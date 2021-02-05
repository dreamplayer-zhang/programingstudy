using RootTools;
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
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
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
                    MessageBox.Show("지원하지 않는 PixelFormat입니다.");
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

        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null) return null;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
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
                MessageBox.Show(ex.Message);
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
    }
}
