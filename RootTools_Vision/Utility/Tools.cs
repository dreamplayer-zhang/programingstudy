using RootTools;
using RootTools.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace RootTools_Vision
{
    public partial class Tools
    {
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

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

        public static byte[] ConvertBufferToArrayRect(SharedBufferInfo info, Rect rect)
        {
            CRect cRect = new CRect((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom);
            int size = cRect.Width * cRect.Height;//((int)rect.Width * (int)rect.Height);
            byte[] dst = new byte[size];
            Tools.ParallelImageCopy(info.PtrR_GRAY, info.Width, info.Height, cRect, dst);
            return dst;
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

                return bmp;
            }
            catch
            {
                return null;
            }
        }
              
        public unsafe static Bitmap CovertBufferToBitmap(SharedBufferInfo info, Rect rect, int outSizeX = 0, int outSizeY = 0)
        {
            try
            {
                int _byteCount = info.ByteCnt;
                int _width = info.Width;
                int _height = info.Height;

                int roiWidth = (int)rect.Width;
                int roiHeight = (int)rect.Height;

                double samplingX = 1;
                double samplingY = 1;
                if (outSizeX != 0 && outSizeY != 0)
                {
                    if (outSizeX > roiWidth) outSizeX = roiWidth;
                    if (outSizeY > roiHeight) outSizeY = roiHeight;

                    roiWidth = outSizeX;
                    roiHeight = outSizeY;

                    samplingX = Math.Floor((double)(int)rect.Width / outSizeX);
                    samplingY = Math.Floor((double)(int)rect.Height / outSizeY);
                }


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
                Bitmap bmp = new Bitmap((int)roiWidth, roiHeight, format);

                ColorPalette palette = bmp.Palette;

                if (_byteCount == 1)
                {
                    for (int i = 0; i < 256; i++)
                        palette.Entries[i] = Color.FromArgb(i, i, i);

                    bmp.Palette = palette;
                }


                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, (int)roiWidth, (int)roiHeight), ImageLockMode.WriteOnly, format);

                IntPtr pointer = bmpData.Scan0;
                if (_byteCount == 1)
                {
                    int h = (int)rect.Height;

                    byte* ptr = (byte*)pointer.ToPointer();

                    for (int i = 0; i < h; i++)
                    {
                        //Marshal.Copy(info.PtrR_GRAY + (int)((i + rect.Top) * info.Width + rect.Left), ptr, (int)(i * _width),(int)rect.Width );                        
                        CopyMemory(pointer + (i * bmpData.Stride), info.PtrR_GRAY + +(int)((i + rect.Top) * info.Width + rect.Left), (uint)rect.Width);
                    }

                    
                    //for (int i = 0; i < _height; i++)
                    //{
                    //    Buffer.MemoryCopy(info.PtrList[0].ToPointer()[i * _width], pointer.ToPointer()[i * bmpData.Stride], bmpData.Stride, bmpData.Stride);
                    //    //CopyMemory(info.PtrR_GRAY + i * _width, pointer + i * bmpData.Stride, (uint)_width);
                    //}
                }
                else if (_byteCount == 3)
                {
                    unsafe
                    {
                        byte* pDst = (byte*)pointer.ToPointer();
                        byte* pR = (byte*)info.PtrR_GRAY.ToPointer();
                        byte* pG = (byte*)info.PtrG.ToPointer();
                        byte* pB = (byte*)info.PtrB.ToPointer();

                        for (int i = 0; i < rect.Y; i++)
                        {
                            pR += info.Width;
                            pG += info.Width;
                            pB += info.Width;
                        }

                        pR += (int)rect.Left;
                        pG += (int)rect.Left;
                        pB += (int)rect.Left;

                        for (long i = 0; i <roiHeight ; i++)
                        {
                            for (long j = 0; j < roiWidth; j++)
                            {
                                pDst[(long)((long)i * (bmpData.Stride) + (long)j * _byteCount + 0)] = *(pB + (long)(j * samplingX));
                                pDst[(long)((long)i * (bmpData.Stride) + (long)j * _byteCount + 1)] = *(pG + (long)(j * samplingX));
                                pDst[(long)((long)i * (bmpData.Stride) + (long)j * _byteCount + 2)] = *(pR + (long)(j * samplingX));
                            }

                            pR += (long)(info.Width * samplingY);
                            pG += (long)(info.Width * samplingY);
                            pB += (long)(info.Width * samplingY);
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

        enum PenColor
        {
            WHITE,
            RED,
            BLUE,
            GREEN,
            YELLOW,
            ORANGE
        }

        Pen GetPen(PenColor penColor)
        {
            Pen pen;
            switch (penColor)
            {
                case PenColor.WHITE:
                    pen = new Pen(Color.White, 3);
                    break;
                case PenColor.RED:
                    pen = new Pen(Color.Red, 3);
                    break;
                case PenColor.BLUE:
                    pen = new Pen(Color.Blue, 3);
                    break;
                case PenColor.GREEN:
                    pen = new Pen(Color.Green, 3);
                    break;
                case PenColor.YELLOW:
                    pen = new Pen(Color.Yellow, 3);
                    break;
                case PenColor.ORANGE:
                default:
                    pen = new Pen(Color.Orange, 3);
                    break;
            }
            return pen;
        }

        Brush GetBrush(PenColor penColor)
        {
            Brush brush;
            switch (penColor)
            {
                case PenColor.WHITE:
                    brush = new SolidBrush(Color.White);
                    break;
                case PenColor.RED:
                    brush = new SolidBrush(Color.Red);
                    break;
                case PenColor.BLUE:
                    brush = new SolidBrush(Color.Black);
                    break;
                case PenColor.GREEN:
                    brush = new SolidBrush(Color.Green);
                    break;
                case PenColor.YELLOW:
                    brush = new SolidBrush(Color.Yellow);
                    break;
                case PenColor.ORANGE:
                default:
                    brush = new SolidBrush(Color.Orange);
                    break;
            }
            return brush;
        }
        void DrawBitmapText(ref Bitmap bit, string text, float x, float y, PenColor penColor = PenColor.ORANGE)
        {
            Graphics graphics = Graphics.FromImage(bit);
            Brush brush = GetBrush(penColor);

            System.Drawing.Font myFont = new System.Drawing.Font(FontFamily.GenericSansSerif, 20, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel);
            graphics.DrawString(text, myFont, brush, x, y);

        }
        void DrawBitmapRect(ref Bitmap bit, float x, float y, float width, float height, PenColor penColor = PenColor.ORANGE)
        {
            Graphics graphics = Graphics.FromImage(bit);
            Pen pen = GetPen(penColor);

           
            graphics.DrawRectangle(pen, x, y, width, height);
        }

        public static void DrawRuler(ref Bitmap bit, int x, int y, float resolution)
        {
            double r = resolution;
            float f10um = (float)(10.0f / r);
            float f1um = f10um / 10;

            Graphics graphics = Graphics.FromImage(bit);

            //graphics.DrawLine

            for (int i = 0; i <= 10; i++)
            {

            }
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

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        
        public static bool SaveImageJpg(SharedBufferInfo info, Rect rect, string savePath, long compressRatio, int outSizeX = 0, int outSizeY = 0)
        {
            Bitmap bmp = CovertBufferToBitmap(info, rect, outSizeX, outSizeY);

            SaveImageJpg(bmp, savePath, compressRatio);

            return true;
        }
        
        public static bool SaveImageJpg(Bitmap bmp, string savePath, long compressRatio)
        {
            try
            {
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

                System.Drawing.Imaging.Encoder myEncoder =
                    System.Drawing.Imaging.Encoder.Quality;

                EncoderParameters enconderParameters = new EncoderParameters(1);

                EncoderParameter parameter = new EncoderParameter(myEncoder, compressRatio);
                enconderParameters.Param[0] = parameter;
                bmp.Save(savePath, jpgEncoder, enconderParameters);

                return true;
            }
            catch(Exception ex)
            {
                TempLogger.Write("Error", ex);
                return false;
            }
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
                //System.Windows.MessageBox.Show(ex.Message);
                TempLogger.Write("Tools", ex);
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

        public static System.Windows.Media.Imaging.BitmapSource ConvertBitmapToSource(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);


            // Try creating a new image with a custom palette.


            System.Windows.Media.Imaging.BitmapSource bitmapSource = null;
            if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
                for (int i = 0; i < 256; i++)
                    colors.Add(System.Windows.Media.Color.FromRgb((byte)i, (byte)i, (byte)i));
                BitmapPalette palette = new BitmapPalette(colors);

                bitmapSource = System.Windows.Media.Imaging.BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                System.Windows.Media.PixelFormats.Indexed8, palette,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);
            }
            else
            {
                bitmapSource = System.Windows.Media.Imaging.BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                System.Windows.Media.PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);
            }


            bitmap.UnlockBits(bitmapData);

            return bitmapSource;
        }

        public static string GetName<T>(Expression<Func<T>> expr)
        {
            var body = ((MemberExpression)expr.Body);
            return body.Member.Name;
        }

        public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
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

                    Marshal.Copy((IntPtr)((long)ptrSrc + (long)((long)i * srcStride + (long)left)), byteDst, width * (i - top), width);

                });

            }
            catch(Exception)
            {
                //검사 종료할 경우 buffer 카피하다가 workplace가 reset되서 다운
            }
        }

        public static Bitmap FlipXImage(System.Drawing.Bitmap source)
        {
            Bitmap dest = new Bitmap(source);
            dest.RotateFlip(RotateFlipType.RotateNoneFlipX);
            return dest;
        }

        public static List<Defect> DataTableToDefectList(DataTable table)
        {
            List<Defect> defects = new List<Defect>();
            FieldInfo[] fields = typeof(Defect).GetFields();
            
            foreach(DataRow row in table.Rows)
            {
                Defect defect = new Defect();
                foreach (FieldInfo info in fields)
                {
                    info.SetValue(defect, Convert.ChangeType(row[info.Name], info.FieldType));
                }
                defects.Add(defect);
            }

            return defects;
        }

        public static Defect DataRowToDefect(DataRow row)
        {            
            FieldInfo[] fields = typeof(Defect).GetFields();

            Defect defect = new Defect();
            foreach (FieldInfo info in fields)
            {
                info.SetValue(defect, Convert.ChangeType(row[info.Name], info.FieldType));
            }

            return defect;
        }
    }
}
