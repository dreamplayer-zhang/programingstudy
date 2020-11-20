using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RootTools.Memory;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Emgu.CV.Dnn;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Collections;
using RootTools_CLR;
using System.Security.Cryptography.X509Certificates;

namespace RootTools
{
	public class ImageData : ObservableObject
	{
		public static string MainFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
		private static System.Drawing.Imaging.ColorPalette mono;
		public enum eMode
		{
			MemoryRead,
			ImageBuffer,
			OtherPCMem,
		}
		public eMode m_eMode = eMode.MemoryRead;

		public MemoryTool m_ToolMemory;

		string m_id = "";
		public string p_id
		{
			get
			{
				return m_id;
			}
			set
			{
				SetProperty(ref m_id, value);
			}
		}

		CPoint m_Size = new CPoint();
		public CPoint p_Size
		{
			get
			{
				return m_Size;
			}
			set
			{
				if ((m_Size.X != value.X) && (m_Size.Y !=value.Y) && m_eMode == eMode.ImageBuffer)
				{
					ReAllocate(value, p_nByte);
				}
				SetProperty(ref m_Size, value);
			}
		}

		int _nByte = 1;
		public int p_nByte
		{
			get
			{
				return _nByte;
			}
			set
			{
				SetProperty(ref _nByte, value);

			}
		}
		public IntPtr m_ptrByte;

		public long p_Stride
		{
			get
			{
				return (long)p_nByte * p_Size.X;
			}
		}

		public IntPtr m_ptrImg;
		MemoryData m_MemData;
		public byte[] m_aBuf;
		byte[] m_aBufFileOpen;

		ObservableCollection<object> m_element = new ObservableCollection<object>();
		public ObservableCollection<object> p_Element
		{
			get
			{
				return m_element;
			}
			set
			{
				SetProperty(ref m_element, value);
			}
		}

		public delegate void DelegateNoParameter();
		public event DelegateNoParameter OnUpdateImage;
		public event DelegateNoParameter OnCreateNewImage;
		public delegate void DelegateOneInt(int nInt);
		public event DelegateOneInt UpdateOpenProgress;

		public BackgroundWorker Worker_MemoryCopy = new BackgroundWorker();
		public BackgroundWorker Worker_MemoryClear = new BackgroundWorker();


		int m_nProgress = 0;
		public int p_nProgress
		{
			get
			{
				return m_nProgress;
			}
			set
			{
				m_nProgress = value;

				if (UpdateOpenProgress != null)
					UpdateOpenProgress(m_nProgress);
			}
		}

		public ImageData(int Width, int Height, int nByte = 1)
		{
			m_eMode = eMode.ImageBuffer;
			p_Size = new CPoint(Width, Height);
			p_nByte = nByte;
			ReAllocate(p_Size, nByte);

			var bmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

			//Grayscale 이미지를 위한 Pallete 설정
			mono = bmp.Palette;
			System.Drawing.Color[] ent = mono.Entries;

			Parallel.For(0, 256, (j) =>
			{
				System.Drawing.Color b = new System.Drawing.Color();
				b = System.Drawing.Color.FromArgb((byte)j, (byte)j, (byte)j);
				ent[j] = b;
			});
		}

		public ImageData(string sIP, string sPool, string sGroup, string sMem, MemoryTool tool)
		{
			m_eMode = eMode.OtherPCMem;
			m_ToolMemory = tool;
		}

		public byte[] GetData(System.Drawing.Rectangle View_Rect, int CanvasWidth, int CanvasHeight)
		{
			//m_ToolMemory.GetOtherMemory(View_Rect, CanvasWidth, CanvasHeight);
			return new byte[5];
		}
		public unsafe void SetData(IntPtr ptr, CRect rect)
		{
			byte* imagePtr = (byte*)ptr;
			for (int i = rect.Height - 1; i >= 0; i--)
			{
				
			}
		}
		public unsafe void SetData(IntPtr ptr, CRect rect, int stride, int nByte = 1)
		{
			for (int i = rect.Height-1; i >= 0; i--)
			{
				Marshal.Copy((IntPtr)((long)ptr + rect.Left * nByte + ((long)i + (long)rect.Top) * stride) , m_aBuf, i * rect.Width * nByte, rect.Width * nByte);
				//Marshal.Copy((IntPtr)((long)ptr + nByte * (rect.Left / nByte + (stride / nByte / nByte) * ((long)i + (long)rect.Top))), m_aBuf , i * rect.Width , rect.Width);
			}
		}
		public byte[] GetByteArray()
		{
			byte[] aBuf = new byte[p_Size.X * p_nByte * p_Size.Y];
			int position = 0;

			for (int i = 0; i < p_Size.Y; i++)
			{
				Marshal.Copy((IntPtr)( (long)GetPtr() + (((long)i) * p_Size.X)), aBuf, position, p_Size.X * p_nByte);
				position += (p_Size.X * p_nByte);
			}
			return aBuf;
		}
		public ImageData(MemoryData data)
		{
			if (data == null) return;
			m_eMode = eMode.MemoryRead;
			m_ptrImg = data.GetPtr();
			m_MemData = data;
			p_Size = data.p_sz;
			p_nByte = data.p_nByte;
			SetBackGroundWorker();
		}

		public void SetBackGroundWorker()
		{
			Worker_MemoryCopy.DoWork += Worker_MemoryCopy_DoWork;
			Worker_MemoryCopy.RunWorkerCompleted += Worker_MemoryCopy_RunWorkerCompleted;
			Worker_MemoryCopy.WorkerSupportsCancellation = true;
			Worker_MemoryClear.DoWork += Worker_MemoryClear_DoWork;
			Worker_MemoryClear.RunWorkerCompleted += Worker_MemoryClear_RunWorkerCompleted;
			Worker_MemoryClear.WorkerSupportsCancellation = true;
		}

		public void UpdateImage()
		{
			OnUpdateImage();
		}
		public Bitmap GetBitmapToArray(int width, int height, byte[] imageData)
		{
			var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			bmp.Palette = mono;
			using (var stream = new MemoryStream(imageData))
			{
				System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0,
																bmp.Width,
																bmp.Height),
												  System.Drawing.Imaging.ImageLockMode.WriteOnly,
												  bmp.PixelFormat);
				IntPtr pNative = bmpData.Scan0;
				Marshal.Copy(imageData, 0, pNative, imageData.Length);
				bmp.UnlockBits(bmpData);
			}
			return bmp;
		}
		public Bitmap GetByteToBitmap(int width, int height, byte[] imageData)
		{
			var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

			bmp.Palette = mono;

			using (var stream = new MemoryStream(imageData))
			{

				System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0,
																bmp.Width,
																bmp.Height),
												  System.Drawing.Imaging.ImageLockMode.WriteOnly,
												  bmp.PixelFormat);

				IntPtr pNative = bmpData.Scan0;
				Marshal.Copy(imageData, 0, pNative, imageData.Length);

				bmp.UnlockBits(bmpData);
			}
			return bmp;
		}
		public byte[] GetRectByteArray(CRect rect)
		{
			int position = 0;

			if (rect.Width % 4 != 0)
			{
				rect.Right += 4 - rect.Width % 4;
			}

			byte[] aBuf = new byte[rect.Width * p_nByte * rect.Height];
			//나중에 거꾸로 나왔던것 확인해야 함. 일단 지금은 정순으로 바꿔둠
			for (int i = 0; i < rect.Height; i++)
			{
				Marshal.Copy((IntPtr)((long)GetPtr() + rect.Left + ((long)i + (long)rect.Top) * p_Size.X), aBuf, position, rect.Width * p_nByte);
				position += (rect.Width * p_nByte);
			}
			return aBuf;
		}
		public Bitmap GetRectImage(CRect rect)
		{
			#region TEMP
			//byte[] aBuf = new byte[54 + (1024 * 2) + rect.Width * rect.Height];
			//byte[] temp;
			//int position = 0;
			//temp = BitConverter.GetBytes(Convert.ToUInt16(0x4d42));
			//temp.CopyTo(aBuf, position);
			//position += temp.Length;

			//temp = BitConverter.GetBytes(Convert.ToUInt32(54 + 1024 + rect.Width * rect.Height));
			//temp.CopyTo(aBuf, position);
			//position += temp.Length;

			//temp = BitConverter.GetBytes(Convert.ToUInt16(0));
			//temp.CopyTo(aBuf, position);
			//position += temp.Length;
			//temp = BitConverter.GetBytes(Convert.ToUInt16(0));
			//temp.CopyTo(aBuf, position);
			//position += temp.Length;
			//temp = BitConverter.GetBytes(Convert.ToUInt32(1078));
			//temp.CopyTo(aBuf, position);
			//position += temp.Length;

			//temp = BitConverter.GetBytes(Convert.ToUInt32(40));
			//temp.CopyTo(aBuf, position);
			//position += temp.Length;
			//temp = BitConverter.GetBytes(Convert.ToUInt32(rect.Width));
			//temp.CopyTo(aBuf, position);
			//position += temp.Length;
			//temp = BitConverter.GetBytes(Convert.ToUInt32(rect.Height));
			//temp.CopyTo(aBuf, position);
			//position += temp.Length;

			//temp = BitConverter.GetBytes(Convert.ToUInt16(1));
			//temp.CopyTo(aBuf, position);
			//position += temp.Length;
			//temp = BitConverter.GetBytes(Convert.ToUInt16(8));
			//temp.CopyTo(aBuf, position);
			//position += temp.Length;
			//temp = BitConverter.GetBytes(Convert.ToUInt32(0));
			//temp.CopyTo(aBuf, position);
			//position += temp.Length;

			//temp = BitConverter.GetBytes(Convert.ToUInt32(rect.Width * rect.Height));
			//temp.CopyTo(aBuf, position);
			//position += temp.Length;
			//temp = BitConverter.GetBytes(Convert.ToUInt32(0));
			//temp.CopyTo(aBuf, position);
			//position += temp.Length;
			//temp = BitConverter.GetBytes(Convert.ToUInt32(0));
			//temp.CopyTo(aBuf, position);
			//position += temp.Length;
			//temp = BitConverter.GetBytes(Convert.ToUInt32(256));
			//temp.CopyTo(aBuf, position);
			//position += temp.Length;
			//temp = BitConverter.GetBytes(Convert.ToUInt32(256));
			//temp.CopyTo(aBuf, position);
			//position += temp.Length;

			//for (int i = 0; i < 256; i++)
			//{

			//    temp = BitConverter.GetBytes(Convert.ToByte(i));
			//    temp.CopyTo(aBuf, position);
			//    position += temp.Length;
			//    temp = BitConverter.GetBytes(Convert.ToByte(i));
			//    temp.CopyTo(aBuf, position);
			//    position += temp.Length;
			//    temp = BitConverter.GetBytes(Convert.ToByte(i));
			//    temp.CopyTo(aBuf, position);
			//    position += temp.Length;
			//    temp = BitConverter.GetBytes(Convert.ToByte(255));
			//    temp.CopyTo(aBuf, position);
			//    position += temp.Length;
			//}
			#endregion

			int position = 0;
			if (rect.Width % 4 != 0)
			{
				rect.Right += 4 - rect.Width % 4;
			}
			byte[] aBuf = new byte[rect.Width * rect.Height];
			//나중에 거꾸로 나왔던것 확인해야 함. 일단 지금은 정순으로 바꿔둠
			for (int i = 0; i < rect.Height; i++)
			{
				Marshal.Copy((IntPtr)((long)GetPtr() + rect.Left + ((long)i + (long)rect.Top) * p_Size.X), aBuf, position, rect.Width);
				position += rect.Width;
			}
			return GetBitmapToArray(rect.Width, rect.Height, aBuf);
		}
		public unsafe BitmapSource GetBitMapSource(int nByteCnt = 1)
		{
			if (nByteCnt == 1)
			{
				Image<Gray, byte> image = new Image<Gray, byte>(p_Size.X, p_Size.Y);
				IntPtr ptrMem = GetPtr();

				for (int y = 0; y < p_Size.Y; y++)
					for (int x = 0; x < p_Size.X; x++)
					{
						image.Data[y, x, 0] = ((byte*)ptrMem)[(long)x + (long)y * p_Size.X];
					}

				return ImageHelper.ToBitmapSource(image);
			}
			else if (nByteCnt == 3)
			{
				Image<Rgb, byte> image = new Image<Rgb, byte>(p_Size.X, p_Size.Y);
				IntPtr ptrMem = GetPtr();

				for (int y = 0; y < p_Size.Y; y++)
					for (int x = 0; x < p_Size.X; x++)
					{
						image.Data[y, x, 0] = ((byte*)ptrMem)[0 + p_nByte * (x + (long)y * p_Size.X)];
						image.Data[y, x, 1] = ((byte*)ptrMem)[1 + p_nByte * (x + (long)y * p_Size.X)];
						image.Data[y, x, 2] = ((byte*)ptrMem)[2 + p_nByte * (x + (long)y * p_Size.X)];
					}
				return ImageHelper.ToBitmapSource(image);
			}
			else if (nByteCnt == 4)
			{
				Image<Bgra, byte> image = new Image<Bgra, byte>(p_Size.X, p_Size.Y);
				IntPtr ptrMem = GetPtr();

				Parallel.For(0, p_Size.Y, y =>
				{
					Parallel.For(0, p_Size.X, x =>
					{
						image.Data[y, x, 0] = ((byte*)ptrMem)[0 + p_nByte * (x + (long)y * p_Size.X)];
						image.Data[y, x, 1] = ((byte*)ptrMem)[1 + p_nByte * (x + (long)y * p_Size.X)];
						image.Data[y, x, 2] = ((byte*)ptrMem)[2 + p_nByte * (x + (long)y * p_Size.X)];
						image.Data[y, x, 3] = ((byte*)ptrMem)[3 + p_nByte * (x + (long)y * p_Size.X)];
					});
				});
				//for (int y = 0; y < p_Size.Y; y++)
				//	for (int x = 0; x < p_Size.X; x++)
				//	{
				//		image.Data[y, x, 0] = ((byte*)ptrMem)[0 + p_nByte * (x + (long)y * p_Size.X)];
				//		image.Data[y, x, 1] = ((byte*)ptrMem)[1 + p_nByte * (x + (long)y * p_Size.X)];
				//		image.Data[y, x, 2] = ((byte*)ptrMem)[2 + p_nByte * (x + (long)y * p_Size.X)];
				//		image.Data[y, x, 3] = ((byte*)ptrMem)[3 + p_nByte * (x + (long)y * p_Size.X)];
				//	}
				return ImageHelper.ToBitmapSource(image);
			}
			return null;
		}
		public Bitmap GetRectImagePattern(CRect rect)
		{
			if (rect.Width % 4 != 0)
			{
				rect.Right += 4 - rect.Width % 4;
			}
			return GetByteToBitmap(rect.Width, rect.Height, GetRectByteArray(rect));
		}
		public void SaveRectImage(CRect memRect)
		{
			SaveFileDialog ofd = new SaveFileDialog();
			ofd.Filter = "BMP파일|*.bmp";

			if (ofd.ShowDialog() == DialogResult.OK)
			{
				List<object> arguments = new List<object>();
				arguments.Add(ofd.FileName);
				arguments.Add(memRect);

				BackgroundWorker Worker_MemorySave = new BackgroundWorker();
				Worker_MemorySave.DoWork += new DoWorkEventHandler(Worker_MemorySave_DoWork);
				Worker_MemorySave.RunWorkerAsync(arguments);
			}
		}
		public void SaveRectImage(CRect memRect, string saveTargetPath)
		{
			List<object> arguments = new List<object>();
			arguments.Add(saveTargetPath);
			arguments.Add(memRect);

			BackgroundWorker Worker_MemorySave = new BackgroundWorker();
			Worker_MemorySave.DoWork += new DoWorkEventHandler(Worker_MemorySave_DoWork);
			Worker_MemorySave.RunWorkerAsync(arguments);
		}
		public void SaveWholeImage()
		{
			SaveFileDialog ofd = new SaveFileDialog();
			ofd.Filter = "BMP파일|*.bmp";

			if (ofd.ShowDialog() == DialogResult.OK)
			{
				List<object> arguments = new List<object>();
				arguments.Add(ofd.FileName);
				arguments.Add(new CRect(0, 0, p_Size.X, p_Size.Y));

				BackgroundWorker Worker_MemorySave = new BackgroundWorker();
				Worker_MemorySave.DoWork += new DoWorkEventHandler(Worker_MemorySave_DoWork);
				Worker_MemorySave.RunWorkerAsync(arguments);
			}
		}
		public void SaveWholeImage(string targetPath)
		{
			List<object> arguments = new List<object>();
			arguments.Add(targetPath);
			arguments.Add(new CRect(0, 0, p_Size.X, p_Size.Y));

			BackgroundWorker Worker_MemorySave = new BackgroundWorker();
			Worker_MemorySave.DoWork += new DoWorkEventHandler(Worker_MemorySave_DoWork);
			Worker_MemorySave.RunWorkerAsync(arguments);
		}
		/// <summary>
		/// 비동기 이미지 Load
		/// </summary>
		public void LoadImageSync(string filePath, CPoint offset)
		{
			OpenBMPFile(filePath, null, offset);
		}
		void Worker_MemorySave_DoWork(object sender, DoWorkEventArgs e)
		{
			List<object> arguments = (List<object>)(e.Argument);

			string sPath = arguments[0].ToString();
			CRect MemRect = (CRect)arguments[1];

			FileSaveBMP(sPath, m_ptrImg, MemRect);
		}
		unsafe void FileSaveBMP(string sFile, IntPtr ptr, CRect rect)
		{
			//int width = (int)(rect.Right * 0.25);
			//if (width * 4 != rect.Right) rect.Right = (width + 1) * 4;

			FileStream fs = new FileStream(sFile, FileMode.Create, FileAccess.Write);
			BinaryWriter bw = new BinaryWriter(fs);

			bw.Write(Convert.ToUInt16(0x4d42));//ushort bfType = br.ReadUInt16();
			if (p_nByte == 1)
			{
				if ((Int64)rect.Width * (Int64)rect.Height > Int32.MaxValue) bw.Write(Convert.ToUInt32(54 + 1024 + p_nByte * 1000 * 1000));
				else bw.Write(Convert.ToUInt32(54 + 1024 + p_nByte * (Int64)rect.Width * (Int64)rect.Height));
			}
			else if (p_nByte == 3)
			{
				if ((Int64)rect.Width * (Int64)rect.Height > Int32.MaxValue) bw.Write(Convert.ToUInt32(54 + p_nByte * 1000 * 1000));//uint bfSize = br.ReadUInt32();
				else bw.Write(Convert.ToUInt32(54 + p_nByte * (Int64)rect.Width * (Int64)rect.Height));//uint bfSize = br.ReadUInt32();
			}
				
			//image 크기 bw.Write();   bmfh.bfSize = sizeof(14byte) + nSizeHdr + rect.right * rect.bottom;
			bw.Write(Convert.ToUInt16(0));   //reserved // br.ReadUInt16();
			bw.Write(Convert.ToUInt16(0));   //reserved //br.ReadUInt16();
			if (p_nByte == 1)
				bw.Write(Convert.ToUInt32(1078));
			else if (p_nByte == 3)
				bw.Write(Convert.ToUInt32(54));//uint bfOffBits = br.ReadUInt32();

			bw.Write(Convert.ToUInt32(40));// uint biSize = br.ReadUInt32();
			bw.Write(Convert.ToInt32(rect.Width));// nWidth = br.ReadInt32();
			bw.Write(Convert.ToInt32(rect.Height));// nHeight = br.ReadInt32();
			bw.Write(Convert.ToUInt16(1));// a = br.ReadUInt16();
			bw.Write(Convert.ToUInt16(8 * p_nByte));     //byte       // nByte = br.ReadUInt16() / 8;                
			bw.Write(Convert.ToUInt32(0));      //compress //b = br.ReadUInt32();
			if ((Int64)rect.Width * (Int64)rect.Height > Int32.MaxValue) bw.Write(Convert.ToUInt32(1000 * 1000));// b = br.ReadUInt32();
			else bw.Write(Convert.ToUInt32((Int64)rect.Width * (Int64)rect.Height));// b = br.ReadUInt32();
			bw.Write(Convert.ToInt32(0));//a = br.ReadInt32();
			bw.Write(Convert.ToInt32(0));// a = br.ReadInt32();
			bw.Write(Convert.ToUInt32(256));      //color //b = br.ReadUInt32();
			bw.Write(Convert.ToUInt32(256));      //import // b = br.ReadUInt32();
			if (p_nByte == 1)
			{
				for (int i = 0; i < 256; i++)
				{
					bw.Write(Convert.ToByte(i));
					bw.Write(Convert.ToByte(i));
					bw.Write(Convert.ToByte(i));
					bw.Write(Convert.ToByte(255));
				}
			}
			if (rect.Width % 4 != 0)
			{
				rect.Right += 4 - rect.Width % 4;
			}
			byte[] aBuf = new byte[p_nByte * rect.Width];
			for (int i = rect.Height - 1; i >= 0; i--)
			{
				Marshal.Copy((IntPtr)((long)ptr + rect.Left + ((long)i + (long)rect.Top) * p_Size.X * p_nByte), aBuf, 0, rect.Width * p_nByte);
				bw.Write(aBuf);
				p_nProgress = Convert.ToInt32(((double)(rect.Height - i) / rect.Height) * 100);
			}

			//byte[] pBuf = br.ReadBytes(nWidth);
			//Marshal.Copy(pBuf, 0, (IntPtr)((long)m_ptrImg + offset.X + (long)p_Size.X * ((long)offset.Y + y)), lowwidth);
			//p_nProgress = Convert.ToInt32(((double)y / lowheight) * 100);
			//for (int i = rect.Height - 1; i >= 0; i--)
			//{  
			//    Marshal.Copy(ptr + i * rect.Width, aBuf, 0, rect.Width);
			//    p_nProgress = Convert.ToInt32(((double)(rect.Height - i) / rect.Height) * 100);
			//    bw.Write(aBuf);
			//}
			bw.Close();
			fs.Close();
		}
		public void OpenFile(string sFileName, CPoint offset)
		{
			FileInfo fileInfo = new FileInfo(sFileName);
			if (fileInfo.Exists)
			{
				List<object> arguments = new List<object>();
				arguments.Add(sFileName);
				arguments.Add(offset);
				Worker_MemoryCopy.RunWorkerAsync(arguments);
			}
			else
			{
				ImageData data = new ImageData(123, 123);
				System.Windows.MessageBox.Show("OpenFile() - 파일이 존재 하지 않거나 열기에 실패하였습니다. - " + sFileName);
			}
		}

		void Worker_MemoryCopy_DoWork(object sender, DoWorkEventArgs e)
		{
			List<object> arguments = (List<object>)(e.Argument);

			string sPath = arguments[0].ToString();
			CPoint offset = (CPoint)(arguments[1]);
			if (sPath.ToLower().IndexOf(".bmp") >= 0)
			{
				OpenBMPFile(sPath, e, offset);
			}
			else if (sPath.ToLower().IndexOf(".jpg") >= 0)
			{

			}
			if (m_eMode == eMode.MemoryRead)
			{

			}
			m_aBufFileOpen = new byte[1];
		}

		void Worker_MemoryCopy_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			p_nProgress = 100;
			OnCreateNewImage();
		}


		void Worker_MemoryClear_DoWork(object sender, DoWorkEventArgs e)
		{
			byte[] pBuf = new byte[p_Size.X];
			for (int y = 0; y < p_Size.Y; y++)
			{
				if (Worker_MemoryClear.CancellationPending)
					return;
				Marshal.Copy(pBuf, 0, (IntPtr)((long)m_ptrImg + (long)p_Size.X * y), p_Size.X);
				p_nProgress = Convert.ToInt32(((double)y / p_Size.Y) * 100);
			}
		}

		public void SaveImageSync(string targetPath)
		{
			IntPtr unmanagedPointer = Marshal.AllocHGlobal(m_aBuf.Length);
			Marshal.Copy(m_aBuf, 0, unmanagedPointer, m_aBuf.Length);
			FileSaveBMP(targetPath, unmanagedPointer, new CRect(0, 0, m_Size.X, m_Size.Y));
		}

		void Worker_MemoryClear_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			p_nProgress = 100;
			OnCreateNewImage();
		}

		int nLine = 0;

		unsafe void OpenBMPFile(string sFile, DoWorkEventArgs e, CPoint offset)
		{
			int nByte;
			int nWidth = 0, nHeight = 0;
			FileStream fs = null;
			try
			{
				fs = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read, 32768, true);
			}
			catch (Exception)
			{
				return;
			}

			int a = 0;
			UInt32 b = 0;
			BinaryReader br = new BinaryReader(fs);
			ushort bfType = br.ReadUInt16();  //2 4 2 2 4 4 4 4 2  2 4 4 4 4 4 4 256*4 1024  54 
			uint bfSize = br.ReadUInt32();    
			br.ReadUInt16();                  
			br.ReadUInt16();
			uint bfOffBits = br.ReadUInt32();
			if (bfType != 0x4D42)
				return;
			uint biSize = br.ReadUInt32();
			nWidth = br.ReadInt32();
			nHeight = br.ReadInt32();
			a = br.ReadUInt16();
			nByte = br.ReadUInt16() / 8;
			b = br.ReadUInt32();
			b = br.ReadUInt32();
			a = br.ReadInt32();
			a = br.ReadInt32();
			b = br.ReadUInt32();
			b = br.ReadUInt32();

			if(bfOffBits != 54)   
				br.ReadBytes((int)bfOffBits - 54);

			int lowwidth = 0, lowheight = 0;
			//lowwidth = nWidth < p_Size.X / p_nByte - offset.X ? nWidth : p_Size.X / p_nByte - offset.X;
			lowwidth = nWidth < p_Size.X - offset.X ? nWidth : p_Size.X  - offset.X;
			lowheight = nHeight < p_Size.Y - offset.Y ? nHeight : p_Size.Y - offset.Y;


			if (m_eMode == eMode.MemoryRead)
			{
				p_nByte = nByte;
				//byte[] hRGB;
    //            if (p_nByte != 3)
    //                hRGB = br.ReadBytes(256 * 4);
				if (p_nByte == 1)
				{
					nLine = 0;
					int nNum = 2;
					Thread[] multiThread = new Thread[nNum];

                    for (int i = 0; i < nNum; i++)
                    {
                        int nStartHeight = lowheight * (nNum - i) / nNum;
                        int nEndHeight = lowheight * (nNum - i - 1) / nNum;
                        multiThread[i] = new Thread(() => RunCopyThread(sFile, nWidth, nHeight, lowwidth, lowheight, nStartHeight, nEndHeight, offset));
                        multiThread[i].Start();
                    }
                    while (true)
                    {
                        bool bEnd = true;
                        for (int i = 0; i < nNum; i++)
                        {
                            if (multiThread[i].IsAlive)
                                bEnd = false;
                        }
                        Thread.Sleep(10);
                        p_nProgress = Convert.ToInt32(((double)nLine / lowheight) * 100);
                        if (bEnd)
                            break;
                    }
                    //for (int y = lowheight - 1; y >= 0; y--)
                    //{
                    //    if (Worker_MemoryCopy.CancellationPending)
                    //        return;

                    //    byte[] pBuf = br.ReadBytes(p_nByte * nWidth);
                    //    Marshal.Copy(pBuf, 0, (IntPtr)((long)m_ptrImg + p_nByte * (offset.X + p_Size.X / p_nByte * ((long)offset.Y + y))), p_nByte * lowwidth);
                    //    p_nProgress = Convert.ToInt32(((double)(lowheight - y) / lowheight) * 100);
                    //}
                }
				else if(p_nByte == 3)
				{
					for (int y = lowheight - 1; y >= 0; y--)
					{
						if (Worker_MemoryCopy.CancellationPending)
							return;

						byte[] pBuf = br.ReadBytes(p_nByte * nWidth);
						IntPtr ptrR =  m_MemData.GetPtr(0);					
						IntPtr ptrG = m_MemData.GetPtr(1);
						IntPtr ptrB = m_MemData.GetPtr(2);
						if (ptrR == IntPtr.Zero || ptrB == IntPtr.Zero || ptrG == IntPtr.Zero)
						{
                            System.Windows.MessageBox.Show("Memory Count Error");
							return;
						}
						for (int i = 0; i < lowwidth*3; i = i + 3)
						{
							((byte*)(ptrB))[i / 3 + (long)y * p_Size.X] = pBuf[i];
                            ((byte*)(ptrG))[i / 3 + (long)y * p_Size.X] = pBuf[i+1];
							((byte*)(ptrR))[i / 3 + (long)y * p_Size.X] = pBuf[i+2];
						}
						//Marshal.Copy(pBuf, 0, (IntPtr)((long)m_ptrImg + p_nByte * (offset.X + p_Size.X / p_nByte * ((long)offset.Y + y))), p_nByte * lowwidth);
						p_nProgress = Convert.ToInt32(((double)(lowheight - y) / lowheight) * 100);
					}
				}
            }
            else
            {
				p_nByte = nByte;
				
				p_Size = new CPoint(nWidth + offset.X, nHeight + offset.Y);
				ReAllocate(p_Size, _nByte);

				byte[] pBuf = new byte[(int)nWidth * nByte];

				for (int y = p_Size.Y - 1; y >= 0; y--)
				{
					pBuf = br.ReadBytes((int)nWidth * nByte);
					Buffer.BlockCopy(pBuf, 0, m_aBuf, (int)(offset.X + (offset.Y + y) * p_Stride), (int)nWidth * nByte);
					p_nProgress = Convert.ToInt32(((double)(p_Size.Y - y) / p_Size.Y) * 100);

				}
			}
			br.Close();
		}	

		public void RunCopyThread(string sFile, int nWidth, int nHeight , int nLowWidth, int nLowHeight,int nStartHeight ,int nEndHeight ,CPoint offset)
		{
			FileStream fss = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read, 32768, true);
			byte[] buf = new byte[nWidth];
			fss.Seek(54 + 1024 + (nLowHeight - nStartHeight) * (long)nWidth , SeekOrigin.Begin);
			for (int i = nStartHeight -1 ; i >= nEndHeight; i--)
			{
				if (Worker_MemoryCopy.CancellationPending)
					return;
				fss.Read(buf,0, nWidth);
				Marshal.Copy(buf, 0, (IntPtr)((long)m_ptrImg + (offset.X + p_Size.X * ((long)offset.Y + i))), nLowWidth);
				nLine++;
				//p_nProgress = Convert.ToInt32(((double)(lowheight - y) / lowheight) * 100);
			}
			//for (int y = lowheight - 1; y >= 0; y--)
			//{
			//	if (Worker_MemoryCopy.CancellationPending)
			//		return;

			//	byte[] pBuf = br.ReadBytes(p_nByte * nWidth);
			//	Marshal.Copy(pBuf, 0, (IntPtr)((long)m_ptrImg + p_nByte * (offset.X + p_Size.X / p_nByte * ((long)offset.Y + y))), p_nByte * lowwidth);
			//	p_nProgress = Convert.ToInt32(((double)(lowheight - y) / lowheight) * 100);
			//}
			fss.Close();
		}

		public unsafe bool ReAllocate(CPoint sz, int nByte)
		{
			if (nByte <= 0)
				return false;
			if ((sz.X < 1) || (sz.Y < 1))
			return false;
			if (m_eMode == eMode.ImageBuffer)
			{
				Array.Resize(ref m_aBuf, sz.X * nByte * sz.Y);
			}

			return true;
		}

		public void ClearImage()
		{
			List<object> arguments = new List<object>();
			arguments.Add("test");
			Worker_MemoryClear.RunWorkerAsync(arguments);
		}

		public IntPtr GetPtr(int index)
		{
			IntPtr ip = (IntPtr)null;
			if (m_eMode == eMode.MemoryRead)
			{
				ip = (IntPtr)((long)m_MemData.GetPtr(index));
			}
			else if (m_eMode == eMode.ImageBuffer)
			{	
			}
			return ip;
		}


		public IntPtr GetPtr(int y = 0, int x = 0)
		{
			IntPtr ip = (IntPtr)null;
			if (m_eMode == eMode.MemoryRead)
			{
				ip = (IntPtr)((long)m_ptrImg + p_nByte * (y * p_Stride + x));
			}
			else if (m_eMode == eMode.ImageBuffer)
			{
				if (m_aBuf == null)
					return (IntPtr)null;
				try
				{
					unsafe
					{
						fixed (byte* p = &m_aBuf[p_nByte * (y * p_Stride + x)])
						{
							ip = (IntPtr)(p);
						}
					}
				}
				catch (Exception)
				{
					return (IntPtr)null;
				}
			}
			return ip;
		}
		public void Clear()
		{
			m_aBuf = null;
			_nByte = 0;
			p_Size.X = 0;
			p_Size.Y = 0;
		}

		#region 주석
		//Bitmap bitmap = new Bitmap(sFile);
		//        if (bitmap == null) return "FileOpen Error"; 
		//        string sClone = Clone(bitmap);
		//        if (sClone == "OK") _bNew = true;
		//        return sClone; 

		//     public string Clone(Bitmap bitmap)
		//    {
		//        switch (bitmap.PixelFormat)
		//        {
		//            case PixelFormat.Format8bppIndexed:
		//                _nByte = 1;
		//                break;
		//            case PixelFormat.Format24bppRgb:
		//                _nByte = 3;
		//                break;
		//            case PixelFormat.Format32bppArgb:
		//                _nByte = 4;
		//                break;
		//            default:
		//                return "Invalid Pixel Format : " + bitmap.PixelFormat.ToString();
		//        }
		//        CPoint sz = new CPoint(bitmap.Width, bitmap.Height);
		//        p_sz = new CPoint((bitmap.Width / 4) * 4, bitmap.Height);
		//        BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
		//        Marshal.Copy(bitmapData.Scan0, m_aBuf, 0, p_nByte * p_sz.X * p_sz.Y);
		//        bitmap.UnlockBits(bitmapData);
		//        InvY();
		//        return "OK";
		//    }

		//    public Mat GetMat(CPoint cp, CPoint szROI)
		//    {
		//        if (cp.X < 0)
		//            cp.X = 0;
		//        if (cp.Y < 0)
		//            cp.Y = 0;
		//        if ((cp.X + szROI.X) > p_sz.X)
		//            szROI.X = p_sz.X - cp.X;
		//        if ((cp.Y + szROI.Y) > p_sz.Y)
		//            szROI.Y = p_sz.Y - cp.Y;
		//        if (szROI.X < 0)
		//            return null;
		//        if (szROI.Y < 0)
		//            return null;
		//        System.Drawing.Size sz = new System.Drawing.Size(szROI.X, szROI.Y);
		//        return new Mat(sz, DepthType.Cv8U, p_nByte, GetIntPtr(cp.Y, cp.X), (int)W);
		//    }
		//    public void InvY()
		//    {
		//        byte[] aTemp = new byte[W];
		//        long yp0 = 0;
		//        long yp1 = W * (p_sz.Y - 1); 
		//        for (int y = 0; y < p_sz.Y / 2; y++)
		//        {
		//            Array.Copy(m_aBuf, yp0, aTemp, 0, W);
		//            Array.Copy(m_aBuf, yp1, m_aBuf, yp0, W);
		//            Array.Copy(aTemp, 0, m_aBuf, yp1, W);
		//            yp0 += W;
		//            yp1 -= W; 
		//        }
		//    }

		//    public void ChangeGray()
		//    {
		//        if (p_nByte == 1) return; 
		//        byte[] aBuf = m_aBuf;
		//        int nByte = p_nByte;
		//        p_nByte = 1;
		//        Parallel.For(0, p_sz.Y, y => ChangeGray(y, nByte, aBuf)); 
		//    }

		//    void ChangeGray(long y, int nByte, byte[] aBuf)
		//    {
		//        long ySrc = y * nByte * p_sz.X;
		//        long yDst = y * W;
		//        for (int x = 0; x < p_sz.X; x++, yDst++, ySrc += nByte)
		//        {
		//            double fGV = 0.114 * aBuf[ySrc];
		//            fGV += 0.587 * aBuf[ySrc + 1];
		//            fGV += 0.299 * aBuf[ySrc + 2];
		//            m_aBuf[yDst] = (byte)Math.Round(fGV); 
		//        }
		//    }

		//    public Bitmap GetBitmap()
		//    {
		//        if ((p_sz.X < 1) || (p_sz.Y < 1)) return null;
		//        PixelFormat pixelFormat; 
		//        switch (p_nByte)
		//        {
		//            case 1: pixelFormat = PixelFormat.Format8bppIndexed; break;
		//            case 3: pixelFormat = PixelFormat.Format24bppRgb; break;
		//            case 4: pixelFormat = PixelFormat.Format32bppRgb; break;
		//            default: return null;
		//        }
		//        Bitmap bitmap = new Bitmap(p_sz.X, p_sz.Y, (int)W, pixelFormat, GetIntPtr(0, 0));
		//        if (p_nByte == 1) SetPalette(bitmap);
		//        bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY); 
		//        return bitmap; 
		//    }

		//    void SetPalette(Bitmap bitmap)
		//    {
		//        int n;
		//        ColorPalette palette = bitmap.Palette;
		//        for (n = 0; n < 256; n++) palette.Entries[n] = Color.FromArgb(n, n, n);
		//        bitmap.Palette = palette;
		//    }

		//    public IntPtr GetIntPtr(int y, int x) 
		//    {
		//        IntPtr ip;
		//        if (m_aBuf == null) return (IntPtr)null;
		//        try
		//        {
		//            unsafe
		//            {
		//                fixed (byte* p = &m_aBuf[y * W + x * p_nByte]) 
		//                { 
		//                    ip = (IntPtr)(p); 
		//                }
		//            }
		//        }
		//        catch (Exception)
		//        {
		//            return (IntPtr)null; 
		//        }
		//        return ip;
		//    }

		//    public string FileSave()
		//    {
		//        SaveFileDialog dlg = new SaveFileDialog();
		//        dlg.Filter = "Image Files(*.bmp;*.jpg)|*.bmp;*.jpg";
		//        if (dlg.ShowDialog() == false) return "Image Save File not Found !!";
		//        return FileSave(dlg.FileName);
		//    }

		//    public string FileSave(string sFile)
		//    {
		//        m_sFile = sFile; 
		//        string[] sFiles = sFile.Split(new char[] { '.' });
		//        int l = sFiles.Length;
		//        if (l < 2) return "Invalid File Name : " + sFile;
		//        string sExt = sFiles[l - 1].ToLower();
		//        Bitmap bitmap = GetBitmap();
		//        if (bitmap == null) return "No Image !!";
		//        try
		//        {
		//            if (sExt == "bmp") bitmap.Save(sFile, ImageFormat.Bmp);
		//            if (sExt == "jpg") bitmap.Save(sFile, ImageFormat.Jpeg);
		//        }
		//        catch (Exception ex)
		//        {
		//            return "Image File Save Exception : " + sFile + " : " + ex.Message;
		//        }
		//        return "OK";
		//    }

		//    public string m_sFile = ""; 
		//    public string FileOpen(string sFile)
		//    {
		//        m_sFile = sFile; 
		//        string[] sFiles = sFile.Split(new char[] { '.' });
		//        int l = sFiles.Length;
		//        if (l < 2) return "Invalid File Name : " + sFile; 
		//        string sExt = sFiles[l-1].ToLower();
		//        try
		//        {
		//            switch (sExt)
		//            {
		//                case "bmp": return FileOpenBMP(sFile);
		//                case "jpg": return FileOpenBitmap(sFile);
		//                default: return "Invalid File Ext : " + sFile;
		//            }
		//        }
		//        catch (Exception ex)
		//        {
		//            return "Image File Open Exception : " + sFile + " : " + ex.Message; 
		//        }
		//    }

		//    string FileOpenBMP(string sFile)
		//    {
		//        int nByte;
		//        CPoint szImg = new CPoint(0, 0);
		//        FileStream fs = null;
		//        try { fs = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read, 32768, true); }
		//        catch (Exception ex) { return ex.Message; }
		//        BinaryReader br = new BinaryReader(fs);
		//        ushort bfType = br.ReadUInt16();
		//        uint bfSize = br.ReadUInt32();
		//        br.ReadUInt16(); 
		//        br.ReadUInt16();
		//        uint bfOffBits = br.ReadUInt32();
		//        if (bfType != 0x4D42) return "File is not BMP !!";
		//        uint biSize = br.ReadUInt32();
		//        szImg.X = br.ReadInt32();
		//        szImg.Y = br.ReadInt32();
		//        br.ReadUInt16();
		//        nByte = br.ReadUInt16() / 8;
		//        br.ReadUInt32();
		//        br.ReadUInt32();
		//        br.ReadInt32(); 
		//        br.ReadInt32();
		//        br.ReadUInt32(); 
		//        br.ReadUInt32();
		//        if (nByte == 1)
		//        {
		//            byte[] hRGB = br.ReadBytes(256 * 4);
		//            _nByte = nByte; 
		//            p_sz = szImg;
		//            for (int y = 0; y < p_sz.Y; y++)
		//            {
		//                byte[] pBuf = br.ReadBytes((int)W);
		//                Buffer.BlockCopy(pBuf, 0, m_aBuf, (int)(y * W), (int)W); 
		//            }
		//            br.Close();
		//        }
		//        else
		//        {
		//            br.Close();
		//            return FileOpenBitmap(sFile); 
		//        }
		//        _bNew = true;
		//        return "OK";
		//    }

		//    string FileOpenBitmap(string sFile)
		//    {
		//        Bitmap bitmap = new Bitmap(sFile);
		//        if (bitmap == null) return "FileOpen Error"; 
		//        string sClone = Clone(bitmap);
		//        if (sClone == "OK") _bNew = true;
		//        return sClone; 
		//    }

		//    public string Copy(ImageD imgSrc, CPoint cpSrc, CPoint sz, CPoint cpDst)
		//    {
		//        if (imgSrc.p_sz.IsInside(cpSrc + sz) == false) return "Image Src Area not Valid";
		//        if (p_sz.IsInside(cpDst + sz) == false) return "Image Dst Area not Valid";
		//        if (imgSrc.p_nByte != p_nByte) return "Image Byte MissMatch"; 
		//        long pSrc = cpSrc.Y * imgSrc.W + cpSrc.X * imgSrc.p_nByte;
		//        long pDst = cpDst.Y * W + cpDst.X * p_nByte;
		//        int w = p_nByte * sz.X; 
		//        for (int y = 0; y < sz.Y; y++)
		//        {
		//            Array.Copy(imgSrc.m_aBuf, pSrc, m_aBuf, pDst, w);
		//            pSrc += imgSrc.W;
		//            pDst += W; 
		//        }
		//        return "OK"; 
		//    }

		//    public bool HasImage()
		//    {
		//        return (W * p_sz.Y != 0);
		//    }

		//    public string GetGVString(CPoint cpImg)
		//    {
		//        if ((cpImg.X < 0) || (cpImg.X >= p_sz.X)) return "0ut";
		//        if ((cpImg.Y < 0) || (cpImg.Y >= p_sz.Y)) return "0ut";
		//        switch (p_nByte)
		//        {
		//            case 1: return m_aBuf[cpImg.X + W * cpImg.Y].ToString();
		//            case 3:
		//                long lAdd = 3 * cpImg.X + W * cpImg.Y;
		//                return "(" + m_aBuf[lAdd + 2].ToString() + "," + m_aBuf[lAdd + 1].ToString() + "," + m_aBuf[lAdd].ToString() + ")";
		//            default: return "Unknown";
		//        }
		//    }
		//}
		#endregion
	}


	public static class ImageHelper
	{
		/// <summary>
		/// ImageSource to bytes
		/// </summary>
		/// <param name="encoder"></param>
		/// <param name="imageSource"></param>
		/// <returns></returns>
		public static byte[] ImageSourceToBytes(BitmapEncoder encoder, ImageSource imageSource)
		{
			byte[] bytes = null;
			var bitmapSource = imageSource as BitmapSource;

			if (bitmapSource != null)
			{
				encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

				using (var stream = new MemoryStream())
				{
					encoder.Save(stream);
					bytes = stream.ToArray();
				}
			}

			return bytes;
		}

		[DllImport("gdi32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool DeleteObject(IntPtr value);

		public static Bitmap GetBitmap(BitmapSource source)
		{
			Bitmap bmp = new Bitmap
			(
			  source.PixelWidth,
			  source.PixelHeight,
			  System.Drawing.Imaging.PixelFormat.Format32bppPArgb
			);

			BitmapData data = bmp.LockBits
			(
				new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
				ImageLockMode.WriteOnly,
				System.Drawing.Imaging.PixelFormat.Format32bppPArgb
			);

			source.CopyPixels
			(
			  Int32Rect.Empty,
			  data.Scan0,
			  data.Height * data.Stride,
			  data.Stride
			);

			bmp.UnlockBits(data);

			return bmp;
		}

		public static BitmapSource GetImageSource(Image myImage)
		{
			var bitmap = new Bitmap(myImage);
			IntPtr bmpPt = bitmap.GetHbitmap();
			BitmapSource bitmapSource =
			 System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
				   bmpPt,
				   IntPtr.Zero,
				   Int32Rect.Empty,
				   BitmapSizeOptions.FromEmptyOptions());

			//freeze bitmapSource and clear memory to avoid memory leaks
			bitmapSource.Freeze();
			DeleteObject(bmpPt);

			return bitmapSource;
		}

		/// <summary>
		/// Convert String to ImageFormat
		/// </summary>
		/// <param name="format"></param>
		/// <returns></returns>
		public static System.Drawing.Imaging.ImageFormat ImageFormatFromString(string format)
		{
			if (format.Equals("Jpg"))
				format = "Jpeg";
			Type type = typeof(System.Drawing.Imaging.ImageFormat);
			BindingFlags flags = BindingFlags.GetProperty;
			object o = type.InvokeMember(format, flags, null, type, null);
			return (System.Drawing.Imaging.ImageFormat)o;
		}

		/// <summary>
		/// Read image from path
		/// </summary>
		/// <param name="imageFile"></param>
		/// <param name="imageFormat"></param>
		/// <returns></returns>
		public static byte[] BytesFromImage(String imageFile, System.Drawing.Imaging.ImageFormat imageFormat)
		{
			MemoryStream ms = new MemoryStream();
			Image img = Image.FromFile(imageFile);
			img.Save(ms, imageFormat);
			return ms.ToArray();
		}

		/// <summary>
		/// Convert image to byte array
		/// </summary>
		/// <param name="imageIn"></param>
		/// <param name="imageFormat"></param>
		/// <returns></returns>
		public static byte[] ImageToByteArray(System.Drawing.Image imageIn, System.Drawing.Imaging.ImageFormat imageFormat)
		{
			MemoryStream ms = new MemoryStream();
			imageIn.Save(ms, imageFormat);
			return ms.ToArray();
		}

		/// <summary>
		/// Byte array to photo
		/// </summary>
		/// <param name="byteArrayIn"></param>
		/// <returns></returns>
		public static Image ByteArrayToImage(byte[] byteArrayIn)
		{
			MemoryStream ms = new MemoryStream(byteArrayIn);
			//TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));

			//Bitmap b = (Bitmap)tc.ConvertFrom(ms, true, false);
			Image returnImage = Image.FromStream(ms, true, false);
			return returnImage;
		}
		public static Bitmap ToBitmap(Image<Gray, byte> image)
		{
			using (System.Drawing.Bitmap source = image.Bitmap)
			{
				var bitmapData = source.LockBits(
				new System.Drawing.Rectangle(0, 0, source.Width, source.Height),
				System.Drawing.Imaging.ImageLockMode.ReadOnly, source.PixelFormat);

				BitmapSource bitmapSource = BitmapSource.Create(
				source.Width, source.Height,
				source.HorizontalResolution, source.VerticalResolution,
				PixelFormats.Gray8, null,
				bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);



				source.UnlockBits(bitmapData);
			
				return source;
			}


		}
		public static BitmapSource ToBitmapSource(Image<Bgra, byte> image)
		{
			using (System.Drawing.Bitmap source = image.Bitmap)
			{
				var bitmapData = source.LockBits(
				new System.Drawing.Rectangle(0, 0, source.Width, source.Height),
				System.Drawing.Imaging.ImageLockMode.ReadOnly, source.PixelFormat);

				BitmapSource bitmapSource = BitmapSource.Create(
				source.Width, source.Height,
				source.HorizontalResolution, source.VerticalResolution,
				PixelFormats.Bgra32, null,
				bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

				source.UnlockBits(bitmapData);

				//DeleteObject(ptr);
				return bitmapSource;
			}
		}
		public static BitmapSource ToBitmapSource(Image<Gray, byte> image)
		{
			using (System.Drawing.Bitmap source = image.Bitmap)
			{
                var bitmapData = source.LockBits(
				new System.Drawing.Rectangle(0, 0, source.Width, source.Height),
				System.Drawing.Imaging.ImageLockMode.ReadOnly, source.PixelFormat);

                BitmapSource bitmapSource = BitmapSource.Create(
				source.Width, source.Height,
				source.HorizontalResolution, source.VerticalResolution,
				PixelFormats.Gray8, null,
				bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);


                source.UnlockBits(bitmapData);

				//DeleteObject(ptr);
				return bitmapSource;
			}
		}
		public static BitmapSource ToBitmapSource(Image<Rgb, byte> image)
		{
			using (System.Drawing.Bitmap source = image.Bitmap)
			{


				var bitmapData = source.LockBits(
				new System.Drawing.Rectangle(0, 0, source.Width, source.Height),
				System.Drawing.Imaging.ImageLockMode.ReadOnly, source.PixelFormat);

				BitmapSource bitmapSource = BitmapSource.Create(
				source.Width, source.Height,
				source.HorizontalResolution, source.VerticalResolution,
				PixelFormats.Bgr24, null,
				bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);


				source.UnlockBits(bitmapData);

				//DeleteObject(ptr);
				return bitmapSource;
			}

		}

		public static BitmapSource GetBitmapSourceFromBitmap(Bitmap bitmap)
		{
			BitmapSource bitmapSource;


			IntPtr hBitmap = bitmap.GetHbitmap();
			BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromEmptyOptions();
			bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, sizeOptions);
			bitmapSource.Freeze();


			return bitmapSource;
		}


		public static byte[] FileLoadBitmap(string sFilePath, int nW, int nH)
		{
			byte[] rawdata = new byte[nW * nH];
			CLR_IP.Cpp_LoadBMP(sFilePath, rawdata, nW, nH);
			return rawdata;
		}

		public static void FileSaveBitmap(string sFilePath, byte[] rawdata, int nW, int nH, int nByteCnt = 1)
		{
			CLR_IP.Cpp_SaveBMP(sFilePath, rawdata, nW, nH, nByteCnt);
		}	
	}
}
