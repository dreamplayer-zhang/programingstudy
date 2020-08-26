using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RootTools
{
    public class ImageToolViewer_VM : ObservableObject
    {
		public ImageToolViewer_VM(ImageData image = null, IDialogService dialogService = null)
		{
			if (image != null)
			{
				p_ImageData = image;
				image.OnCreateNewImage += image_NewImage;
				image.OnUpdateImage += image_OnUpdateImage;
				image.UpdateOpenProgress += image_UpdateOpenProgress;
				InitRoiRect(p_ImageData.p_Size.X, p_ImageData.p_Size.Y);
				SetImageSource();

			}
			if (dialogService != null)
			{
				m_DialogService = dialogService;
			}
		}

		private readonly IDialogService m_DialogService;

		bool mouseIsDown = System.Windows.Input.Mouse.LeftButton == MouseButtonState.Pressed;
		
		#region Property

		private ImageData m_ImageData;
		public ImageData p_ImageData
		{
			get
			{
				return m_ImageData;
			}
			set
			{
				SetProperty(ref m_ImageData, value);
			}
		}

		private CPoint m_CopyOffset = new CPoint(0, 0);
		public CPoint p_CopyOffset
		{
			get
			{
				return m_CopyOffset;
			}
			set
			{
				SetProperty(ref m_CopyOffset, value);
			}
		}

		private int m_nProgress = 0;
		public int p_nProgress
		{
			get
			{
				return m_nProgress;
			}
			set
			{
				SetProperty(ref m_nProgress, value);
			}
		}

		#region View
		private BitmapSource m_imgSource;
		public BitmapSource p_ImgSource
		{
			get
			{
				return m_imgSource;
			}
			set
			{
				SetProperty(ref m_imgSource, value);
			}
		}

		private System.Drawing.Rectangle _View_Rect = new System.Drawing.Rectangle();
		public System.Drawing.Rectangle p_View_Rect
		{
			get
			{
				return _View_Rect;
			}
			set
			{
				SetProperty(ref _View_Rect, value);
			}
		}

		private int m_CanvasWidth = 100;
		public int p_CanvasWidth
		{
			get
			{
				return m_CanvasWidth;
			}
			set
			{
				if (value == 0)
					return;
				SetProperty(ref m_CanvasWidth, value);
			}
		}
		private int m_CanvasHeight = 100;
		public int p_CanvasHeight
		{
			get
			{
				return m_CanvasHeight;
			}
			set
			{
				if (value == 0)
					return;
				//_CanvasHeight = value / 10 * 10;
				//RaisePropertyChanged();
				SetProperty(ref m_CanvasHeight, value);
			}
		}

		#endregion

		#region Thumbnail

		private BitmapSource m_ThumNailImgSource;
		public BitmapSource p_ThumNailImgSource
		{
			get
			{
				return m_ThumNailImgSource;
			}
			set
			{
				SetProperty(ref m_ThumNailImgSource, value);
			}
		}

		private System.Drawing.Rectangle m_TumbnailImg_Rect = new System.Drawing.Rectangle();
		public System.Drawing.Rectangle p_TumbnailImg_Rect
		{
			get
			{
				return m_TumbnailImg_Rect;
			}
			set
			{
				SetProperty(ref m_TumbnailImg_Rect, value);
			}
		}

		private Thickness m_TumbnailImgMargin = new Thickness(0, 0, 0, 0);
		public Thickness p_TumbnailImgMargin // TumbnailRect Margin
		{
			get
			{
				return m_TumbnailImgMargin;
			}
			set
			{
				SetProperty(ref m_TumbnailImgMargin, value);
			}
		}

		private int m_ThumbWidth = 100;
		public int p_ThumbWidth
		{
			get
			{
				return m_ThumbWidth;
			}
			set
			{
				SetProperty(ref m_ThumbWidth, value);
			}
		}
		private int m_ThumbHeight = 100;
		public int p_ThumbHeight
		{
			get
			{
				return m_ThumbHeight;
			}
			set
			{
				//_CanvasHeight = value / 10 * 10;
				//RaisePropertyChanged();
				SetProperty(ref m_ThumbHeight, value);
			}
		}

		#endregion

		#region Mouse
		private int m_MouseX = 0;
		public int p_MouseX
		{
			get
			{
				return m_MouseX;
			}
			set
			{
				if (p_ImgSource != null)
				{
					if (m_CanvasWidth != 0 && m_CanvasHeight != 0)
					{
                        if (p_MouseX < p_ImgSource.Width && p_MouseY < p_ImgSource.Height)
                        {
                            if (p_ImgSource.Format.BitsPerPixel == 24)
                            {
                                System.Windows.Media.Color c_Pixel = GetPixelColor(p_ImgSource, p_MouseX, p_MouseY);
								p_PixelData = "R = " + c_Pixel.R + "G = " + c_Pixel.G + "B = " + c_Pixel.B;
                            }
                            else if (p_ImgSource.Format.BitsPerPixel == 8)
                            {
                                byte[] pixel = new byte[1];
                                p_ImgSource.CopyPixels(new Int32Rect(p_MouseX, p_MouseY, 1, 1), pixel, 1, 0);
								p_PixelData = "GV = " + pixel[0];
                            }
                            p_MouseMemY = p_View_Rect.Y + p_MouseY * p_View_Rect.Height / m_CanvasHeight;
                            p_MouseMemX = p_View_Rect.X + p_MouseX * p_View_Rect.Width / m_CanvasWidth;
                        }
                    }
				}
				SetProperty(ref m_MouseX, value);
			}
		}
		private int m_MouseY = 0;
		public int p_MouseY
		{
			get
			{
				return m_MouseY;
			}
			set
			{
				SetProperty(ref m_MouseY, value);
			}
		}
		private int m_MouseMemX = 0;
		public int p_MouseMemX
		{
			get
			{
				return m_MouseMemX;
			}
			set
			{
				SetProperty(ref m_MouseMemX, value);
			}
		}
		private int m_MouseMemY = 0;
		public int p_MouseMemY
		{
			get
			{
				return m_MouseMemY;
			}
			set
			{
				SetProperty(ref m_MouseMemY, value);
			}
		}
		private int m_TumbMouseX = 0;
		public int p_TumbMouseX
		{
			get
			{
				return m_TumbMouseX;
			}
			set
			{
				SetProperty(ref m_TumbMouseX, value);
			}
		}
		private int m_TumbMouseY = 0;
		public int p_TumbMouseY
		{
			get
			{
				return m_TumbMouseY;
			}
			set
			{
				SetProperty(ref m_TumbMouseY, value);
			}
		}

		private string m_PixelData = "0";
		public string p_PixelData
		{
			get
			{
				return m_PixelData;
			}
			set
			{
				SetProperty(ref m_PixelData, value);
			}
		}
		private double m_Zoom = 1;
		public double p_Zoom
		{
			get
			{
				return m_Zoom;
			}
			set
			{
				SetProperty(ref m_Zoom, value);
				SetRoiRect();
			}
		}
		#endregion

		#endregion

		#region Method
		public void SetImageData(ImageData image)
		{
			p_ImageData = null;
			p_ImageData = image;
			image.OnCreateNewImage += image_NewImage;
			image.OnUpdateImage += image_OnUpdateImage;
			image.UpdateOpenProgress += image_UpdateOpenProgress;
			InitRoiRect(p_ImageData.p_Size.X, p_ImageData.p_Size.Y);
			SetImageSource();
		}
		void image_OnUpdateImage()
		{
			SetImageSource();
		}
		void image_UpdateOpenProgress(int nInt)
		{
			p_nProgress = nInt;
		}
		void image_NewImage()
		{
			SetRoiRect();
			//InitRoiRect(p_ImageData.p_Size.X, p_ImageData.p_Size.Y);
			//SetImageSource();
		}

		public unsafe void SetImageSource()
		{
			try
			{
				if (p_ImageData != null)
				{
					if (p_ImageData.p_nByte == 1)
					{
						Image<Gray, byte> view = new Image<Gray, byte>(p_CanvasWidth, p_CanvasHeight);
						IntPtr ptrMem = m_ImageData.GetPtr();
						if (ptrMem == IntPtr.Zero)
							return;
						int pix_x = 0;
						int pix_y = 0;

						for (int yy = 0; yy < p_CanvasHeight; yy++)
						{
							for (int xx = 0; xx < p_CanvasWidth; xx++)
							{
								pix_x = p_View_Rect.X + xx * p_View_Rect.Width / p_CanvasWidth;
								pix_y = p_View_Rect.Y + yy * p_View_Rect.Height / p_CanvasHeight;
								view.Data[yy, xx, 0] = ((byte*)ptrMem)[(long)pix_x + (long)pix_y * p_ImageData.p_Size.X];
							}
						}

						p_ImgSource = ImageHelper.ToBitmapSource(view);

						p_TumbnailImgMargin = new Thickness(Convert.ToInt32((double)p_View_Rect.X * p_ThumbWidth / m_ImageData.p_Size.X), Convert.ToInt32((double)p_View_Rect.Y * p_ThumbHeight / m_ImageData.p_Size.Y), 0, 0);
						if (Convert.ToInt32((double)p_View_Rect.Height * p_ThumbHeight / m_ImageData.p_Size.Y) == 0)
							p_TumbnailImg_Rect = new System.Drawing.Rectangle(0, 0, Convert.ToInt32((double)p_View_Rect.Width * p_ThumbWidth / m_ImageData.p_Size.X), 2);
						else
							p_TumbnailImg_Rect = new System.Drawing.Rectangle(0, 0, Convert.ToInt32((double)p_View_Rect.Width * p_ThumbWidth / m_ImageData.p_Size.X), Convert.ToInt32((double)p_View_Rect.Height * p_ThumbHeight / m_ImageData.p_Size.Y));

					}
					else if (p_ImageData.p_nByte == 3)
					{
						Image<Rgb, byte> view = new Image<Rgb, byte>(p_CanvasWidth, p_CanvasHeight);
						IntPtr ptrMem = m_ImageData.GetPtr();
						if (ptrMem == IntPtr.Zero)
							return;
						int pix_x = 0;
						int pix_y = 0;
						long pix_rect;

						for (int yy = 1; yy < p_CanvasHeight; yy++)
						{
							pix_y = p_View_Rect.Y + yy * p_View_Rect.Height / p_CanvasHeight;
							pix_rect = (long)pix_y * p_ImageData.p_Size.X;
							for (int xx = 0; xx < p_CanvasWidth; xx++)
							{
								pix_x = p_View_Rect.X + xx * p_View_Rect.Width / p_CanvasWidth;

								view.Data[yy, xx, 2] = ((byte*)ptrMem)[0 + m_ImageData.p_nByte * ((long)pix_x + pix_rect)];
								view.Data[yy, xx, 1] = ((byte*)ptrMem)[1 + m_ImageData.p_nByte * ((long)pix_x + pix_rect)];
								view.Data[yy, xx, 0] = ((byte*)ptrMem)[2 + m_ImageData.p_nByte * ((long)pix_x + pix_rect)];

							}
						}

						p_ImgSource = ImageHelper.ToBitmapSource(view);

						p_TumbnailImgMargin = new Thickness(Convert.ToInt32((double)p_View_Rect.X * p_ThumbWidth / m_ImageData.p_Size.X), Convert.ToInt32((double)p_View_Rect.Y * p_ThumbHeight / m_ImageData.p_Size.Y), 0, 0);
						if (Convert.ToInt32((double)p_View_Rect.Height * p_ThumbHeight / m_ImageData.p_Size.Y) == 0)
							p_TumbnailImg_Rect = new System.Drawing.Rectangle(0, 0, Convert.ToInt32((double)p_View_Rect.Width * p_ThumbWidth / m_ImageData.p_Size.X), 2);
						else
							p_TumbnailImg_Rect = new System.Drawing.Rectangle(0, 0, Convert.ToInt32((double)p_View_Rect.Width * p_ThumbWidth / m_ImageData.p_Size.X), Convert.ToInt32((double)p_View_Rect.Height * p_ThumbHeight / m_ImageData.p_Size.Y));

					}

				}
			}
			catch (Exception ee)
			{
				System.Windows.MessageBox.Show(ee.ToString());
			}

		}
		public void InitRoiRect(int nWidth, int nHeight)
		{
			if (p_ImageData == null)
			{
				p_View_Rect = new System.Drawing.Rectangle(0, 0, nWidth, nHeight);
				p_Zoom = 1;
			}
			bool bRatio_WH = (double)p_View_Rect.Width / p_CanvasWidth < (double)p_View_Rect.Height / p_CanvasHeight;
			if (bRatio_WH)//세로가 길어
			{
				p_View_Rect = new System.Drawing.Rectangle(p_View_Rect.X, p_View_Rect.Y, p_View_Rect.Width, p_View_Rect.Width * p_CanvasHeight / p_CanvasWidth);
			}
			else
			{
				p_View_Rect = new System.Drawing.Rectangle(p_View_Rect.X, p_View_Rect.Y, p_View_Rect.Height * p_CanvasWidth / p_CanvasHeight, p_View_Rect.Height);
			}
			SetThumNailIamgeSource();
		}
		public void SetRoiRect()
		{
			if (p_ImageData != null)
			{
				CPoint StartPt = GetStartPoint_Center(p_ImageData.p_Size.X, p_ImageData.p_Size.Y);
				bool bRatio_WH = (double)p_ImageData.p_Size.X / p_CanvasWidth < (double)p_ImageData.p_Size.Y / p_CanvasHeight;
				if (bRatio_WH)
				{ //세로가 길어
					p_View_Rect = new System.Drawing.Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(p_ImageData.p_Size.X * p_Zoom), Convert.ToInt32(p_ImageData.p_Size.X * p_Zoom * p_CanvasHeight / p_CanvasWidth));
				}
				else
				{
					p_View_Rect = new System.Drawing.Rectangle(StartPt.X, StartPt.Y, Convert.ToInt32(p_ImageData.p_Size.Y * p_Zoom * p_CanvasWidth / p_CanvasHeight), Convert.ToInt32(p_ImageData.p_Size.Y * p_Zoom));
				}
				if (p_View_Rect.Height % 2 != 0)
					_View_Rect.Height += 1;
				SetImageSource();
			}
		}
		void SetViewRect(CPoint point)      //point image의 좌상단
		{
			bool bRatio_WH = (double)p_ImageData.p_Size.X / p_CanvasWidth < (double)p_ImageData.p_Size.Y / p_CanvasHeight;
			if (bRatio_WH)
			{ //세로가 길어
				p_View_Rect = new System.Drawing.Rectangle(point.X, point.Y, Convert.ToInt32(p_ImageData.p_Size.X * p_Zoom), Convert.ToInt32(p_ImageData.p_Size.X * p_Zoom * p_CanvasHeight / p_CanvasWidth));
			}
			else
			{
				p_View_Rect = new System.Drawing.Rectangle(point.X, point.Y, Convert.ToInt32(p_ImageData.p_Size.Y * p_Zoom * p_CanvasWidth / p_CanvasHeight), Convert.ToInt32(p_ImageData.p_Size.Y * p_Zoom));
			}
		}

		public unsafe void SetThumNailIamgeSource()
		{
			if (p_ImageData.p_nByte == 1)
			{
				Image<Gray, byte> view = new Image<Gray, byte>(p_ThumbWidth, p_ThumbHeight);
				IntPtr ptrMem = m_ImageData.GetPtr();
				if (ptrMem == IntPtr.Zero)
					return;
				int pix_x = 0;
				int pix_y = 0;

				for (int yy = 0; yy < p_ThumbHeight; yy++)
				{
					for (int xx = 0; xx < p_ThumbWidth; xx++)
					{
						pix_x = xx * p_ImageData.p_Size.X / p_ThumbWidth;
						pix_y = yy * p_ImageData.p_Size.Y / p_ThumbHeight;
						view.Data[yy, xx, 0] = ((byte*)ptrMem)[pix_x + (long)pix_y * p_ImageData.p_Size.X];
					}
				}
				if (view.Width != 0 && view.Height != 0)
					p_ThumNailImgSource = ImageHelper.ToBitmapSource(view);
			}
			else if (p_ImageData.p_nByte == 3)
			{
				Image<Rgb, byte> view = new Image<Rgb, byte>(p_ThumbWidth, p_ThumbHeight);
				IntPtr ptrMem = m_ImageData.GetPtr();
				if (ptrMem == IntPtr.Zero)
					return;
				int pix_x = 0;
				int pix_y = 0;

				for (int yy = 0; yy < p_ThumbHeight; yy++)
				{
					pix_y = yy * p_ImageData.p_Size.Y / p_ThumbHeight;
					for (int xx = 0; xx < p_ThumbWidth; xx++)
					{
						pix_x = xx * p_ImageData.p_Size.X / p_ThumbWidth;
						view.Data[yy, xx, 2] = ((byte*)ptrMem)[0 + m_ImageData.p_nByte * (pix_x + (long)pix_y * p_ImageData.p_Size.X)];
						view.Data[yy, xx, 1] = ((byte*)ptrMem)[1 + m_ImageData.p_nByte * (pix_x + (long)pix_y * p_ImageData.p_Size.X)];
						view.Data[yy, xx, 0] = ((byte*)ptrMem)[2 + m_ImageData.p_nByte * (pix_x + (long)pix_y * p_ImageData.p_Size.X)];
					}
				}
				if (view.Width != 0 && view.Height != 0)
					p_ThumNailImgSource = ImageHelper.ToBitmapSource(view);
			}

		}
		void _openImage()
		{
			if (m_ImageData == null)
			{
				System.Windows.Forms.MessageBox.Show("Image를 열어주세요");
				return;
			}
			_CancelCopy();
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "Image Files(*.bmp;*.jpg)|*.bmp;*.jpg";
			if (ofd.ShowDialog() == DialogResult.OK)
			{
                var viewModel = new Dialog_ImageOpenViewModel(this);
                Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
                if (result.HasValue)
                {
                    if (result.Value)
                    {
                        m_ImageData.OpenFile(ofd.FileName, p_CopyOffset);
                    }
                    else
                    {
                        // Cancelled
                    }
                }
            }
		}
		void _saveImage()
		{
			if (m_ImageData == null)
			{
				System.Windows.Forms.MessageBox.Show("Image를 열어주세요");
				return;
			}
			_CancelCopy();

			////if (m_BasicTool.m_Element.Count == 0 || m_BasicTool.m_Element[0].GetType() != typeof(System.Windows.Shapes.Rectangle))
			////	m_ImageData.SaveWholeImage();
			////else
			////{
			//var temp = m_BasicTool.m_TempRect;
			//int left = (int)temp.StartPos.X;
			//int top = (int)temp.StartPos.Y;

			//int right = (int)temp.EndPos.X;
			//int bot = (int)temp.EndPos.Y;

			//m_ImageData.SaveRectImage(new CRect(left, top, right, bot));
			////}
		}
		void _clearImage()
		{
			if (m_ImageData != null)
				m_ImageData.ClearImage();
		}
		void _CancelCopy()
		{
			if (m_ImageData.Worker_MemoryCopy.IsBusy)
			{
				m_ImageData.Worker_MemoryCopy.CancelAsync();
			}
			if (m_ImageData.Worker_MemoryClear.IsBusy)
			{
				m_ImageData.Worker_MemoryClear.CancelAsync();
			}
		}

		void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
		{
			//CPoint StartPt = GetCurrentPoint();
			CPoint MovePoint = new CPoint();
			MovePoint.X = point.X + p_View_Rect.Width * nX / p_CanvasWidth;
			MovePoint.Y = point.Y + p_View_Rect.Height * nY / p_CanvasHeight;
			if (MovePoint.X < 0)
				MovePoint.X = 0;
			else if (MovePoint.X > p_ImageData.p_Size.X - p_View_Rect.Width)
				MovePoint.X = p_ImageData.p_Size.X - p_View_Rect.Width;
			if (MovePoint.Y < 0)
				MovePoint.Y = 0;
			else if (MovePoint.Y > p_ImageData.p_Size.Y - p_View_Rect.Height)
				MovePoint.Y = p_ImageData.p_Size.Y - p_View_Rect.Height;
			SetViewRect(MovePoint);
			SetImageSource();
		}
		void CanvasMoveMousePoint()
		{
			CPoint StartPt = GetStartPoint(p_MouseX, p_MouseY);
			SetViewRect(StartPt);
			SetImageSource();
		}
		void CanvasMoveCanvasPoint(int mouseX, int mouseY)
		{
			Thread.Sleep(50);
			CPoint StartPt = GetStartPoint(mouseX, mouseY);
			SetViewRect(StartPt);
			SetImageSource();
		}
		void CanvasMovePoint(double nPercentX, double nPercentY)        // 0.xxx
		{
			CPoint StartPt = new CPoint();
			StartPt.X = Convert.ToInt32(p_ImageData.p_Size.X * nPercentX - p_View_Rect.Width / 2);
			StartPt.Y = Convert.ToInt32(p_ImageData.p_Size.Y * nPercentY - p_View_Rect.Height / 2);
			if (StartPt.X < 0)
				StartPt.X = 0;
			else if (StartPt.X > p_ImageData.p_Size.X - p_View_Rect.Width)
				StartPt.X = p_ImageData.p_Size.X - p_View_Rect.Width;
			if (StartPt.Y < 0)
				StartPt.Y = 0;
			else if (StartPt.Y > p_ImageData.p_Size.Y - p_View_Rect.Height)
				StartPt.Y = p_ImageData.p_Size.Y - p_View_Rect.Height;
			SetViewRect(StartPt);
			SetImageSource();

		}

		CPoint GetStartPoint(int MouseX, int MouseY)
		{
			int nX = p_View_Rect.X + p_View_Rect.Width * MouseX / p_CanvasWidth - p_View_Rect.Width / 2;
			int nY = p_View_Rect.Y + p_View_Rect.Height * MouseY / p_CanvasHeight - p_View_Rect.Height / 2;

			if (nX < 0)
				nX = 0;
			else if (nX > p_ImageData.p_Size.X - p_View_Rect.Width)
				nX = p_ImageData.p_Size.X - p_View_Rect.Width;
			if (nY < 0)
				nY = 0;
			else if (nY > p_ImageData.p_Size.Y - p_View_Rect.Height)
				nY = p_ImageData.p_Size.Y - p_View_Rect.Height;
			return new CPoint(nX, nY);
		}
		CPoint GetStartPoint_Center(int nImgWidth, int nImgHeight)
		{
			bool bRatio_WH = (double)p_ImageData.p_Size.X / p_CanvasWidth < (double)p_ImageData.p_Size.Y / p_CanvasHeight;
			int viewrectwidth = 0;
			int viewrectheight = 0;
			int nX = 0;
			int nY = 0;
			if (bRatio_WH)
			{ //세로가 길어
			  //nX = p_View_Rect.X + Convert.ToInt32(p_View_Rect.Width - nImgWidth * p_Zoom) /2; 기존 중앙기준으로 확대/축소되는 코드. 
				nX = p_View_Rect.X + Convert.ToInt32(p_View_Rect.Width - nImgWidth * p_Zoom) * p_MouseX / m_CanvasWidth; // 마우스 커서기준으로 확대/축소
				nY = p_View_Rect.Y + Convert.ToInt32(p_View_Rect.Height - nImgWidth * p_Zoom * p_CanvasHeight / p_CanvasWidth) * p_MouseY / m_CanvasHeight;
				viewrectwidth = Convert.ToInt32(nImgWidth * p_Zoom);
				viewrectheight = Convert.ToInt32(nImgWidth * p_Zoom * p_CanvasHeight / p_CanvasWidth);
			}
			else
			{
				nX = p_View_Rect.X + Convert.ToInt32(p_View_Rect.Width - nImgHeight * p_Zoom * p_CanvasWidth / p_CanvasHeight) * p_MouseX / m_CanvasWidth;
				nY = p_View_Rect.Y + Convert.ToInt32(p_View_Rect.Height - nImgHeight * p_Zoom) * p_MouseY / m_CanvasHeight;
				viewrectwidth = Convert.ToInt32(nImgHeight * p_Zoom * p_CanvasWidth / p_CanvasHeight);
				viewrectheight = Convert.ToInt32(nImgHeight * p_Zoom);
			}

			if (nX < 0)
				nX = 0;
			else if (nX > nImgWidth - viewrectwidth)
				nX = nImgWidth - viewrectwidth;
			if (nY < 0)
				nY = 0;
			else if (nY > nImgHeight - viewrectheight)
				nY = nImgHeight - viewrectheight;
			return new CPoint(nX, nY);
		}
		public System.Windows.Media.Color GetPixelColor(BitmapSource source, int x, int y)
		{
			System.Windows.Media.Color c = Colors.White;
			if (source != null)
			{
				try
				{
					CroppedBitmap cb = new CroppedBitmap(source, new Int32Rect(x, y, 1, 1));
					var pixels = new byte[4];
					cb.CopyPixels(pixels, 4, 0);
					c = System.Windows.Media.Color.FromRgb(pixels[2], pixels[1], pixels[0]);
				}
				catch (Exception) { }
			}
			return c;
		}
		#endregion

		#region Command
		public ICommand OpenImage
		{
			get
			{
				return new RelayCommand(_openImage);
			}
		}
		public ICommand ClearImage
		{
			get
			{
				return new RelayCommand(_clearImage);
			}
		}
		public ICommand CancelCommand
		{
			get
			{
				return new RelayCommand(_CancelCopy);
			}
		}

		public void CanvasMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			var pt = e.GetPosition(sender as IInputElement);
			p_MouseX = (int) pt.X;
			p_MouseY = (int) pt.Y;
		}
		
		#endregion
	}
}
