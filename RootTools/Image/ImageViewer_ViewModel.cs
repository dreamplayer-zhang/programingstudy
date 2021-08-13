using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using RootTools.Inspects;
using RootTools.Memory;
using MySql.Data.MySqlClient.Authentication;
using System.Windows.Threading;
using RootTools.Database;

namespace RootTools
{

    public delegate void LoadedDelegate();
	public delegate void DoubleClickDelegate();
	// public delegate void RedrawDelegate();
	public class ImageViewer_ViewModel : ObservableObject
	{
		public enum DrawingMode
		{
			None,
			Drawing,
			Tool,
			AttachedTool,
			Modify
		}
		#region Property
		private ObservableCollection<UIElement> m_Element = new ObservableCollection<UIElement>();
		public ObservableCollection<UIElement> p_Element
		{
			get
			{
				return m_Element;
			}
			set
			{
				SetProperty(ref m_Element, value);
			}
		}

		private ObservableCollection<UIElement> _ViewerUIelement = new ObservableCollection<UIElement>();
		public ObservableCollection<UIElement> p_ViewerUIElement
		{
			get
			{
				return _ViewerUIelement;
			}
			set
			{
				SetProperty(ref _ViewerUIelement, value);
			}
		}

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

		private List<DefectDataWrapper> m_arrDefectDataWrapper;
		public List<DefectDataWrapper> p_arrDefectDataWrapper
		{
			get
			{
				return m_arrDefectDataWrapper;
			}
			set
			{
				SetProperty(ref m_arrDefectDataWrapper, value);
			}
		}

		private System.Windows.Input.Cursor m_MouseCursor;
		public System.Windows.Input.Cursor p_MouseCursor
		{
			get
			{
				return m_MouseCursor;
			}
			set
			{
				SetProperty(ref m_MouseCursor, value);
			}
		}

		private int _CanvasWidth = 100;
		public int p_CanvasWidth
		{
			get
			{
				return _CanvasWidth;
			}
			set
			{
				if (value == 0)
					return;
				SetProperty(ref _CanvasWidth, value);
				SetRoiRect();
			}
		}
		private int _CanvasHeight = 100;
		public int p_CanvasHeight
		{
			get
			{
				return _CanvasHeight;
			}
			set
			{
				if (value == 0)
					return;
				//_CanvasHeight = value / 10 * 10;
				//RaisePropertyChanged();
				SetProperty(ref _CanvasHeight, value);
				//SetRoiRect();
			}
		}

		private int _ThumbWidth = 100;
		public int p_ThumbWidth
		{
			get
			{
				return _ThumbWidth;
			}
			set
			{
				SetProperty(ref _ThumbWidth, value);
			}
		}
		private int _ThumbHeight = 100;
		public int p_ThumbHeight
		{
			get
			{
				return _ThumbHeight;
			}
			set
			{
				//_CanvasHeight = value / 10 * 10;
				//RaisePropertyChanged();
				SetProperty(ref _ThumbHeight, value);
			}
		}

		//public bool KeyPressedState = false;
		private System.Windows.Input.KeyEventArgs _keyEvent;
		public System.Windows.Input.KeyEventArgs KeyEvent
		{
			get
			{
				return _keyEvent;
			}
			set
			{
				SetProperty(ref _keyEvent, value);
			}

		}


		private System.Windows.Input.MouseEventArgs _mouseEvent;
		public System.Windows.Input.MouseEventArgs MouseEvent
		{
			get
			{
				return _mouseEvent;
			}
			set
			{
				SetProperty(ref _mouseEvent, value);
			}
		}
		private MouseButtonState MouseState;


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


		private int _MouseX = 0;
		public int p_MouseX
		{
			get
			{
				return _MouseX;
			}
			set
			{
				if (p_ImgSource != null)
				{
					if (_CanvasWidth != 0 && _CanvasHeight != 0)
					{
						if (p_MouseX < p_ImgSource.Width && p_MouseY < p_ImgSource.Height)
						{
							if (p_ImgSource.Format.BitsPerPixel == 24)
							{
								System.Windows.Media.Color c_Pixel = GetPixelColor(p_ImgSource, p_MouseX, p_MouseY);
								p_Data = "R = " + c_Pixel.R + "G = " + c_Pixel.G + "B = " + c_Pixel.B;
							}
							else if (p_ImgSource.Format.BitsPerPixel == 8)
							{
								byte[] pixel = new byte[1];
								p_ImgSource.CopyPixels(new Int32Rect(p_MouseX, p_MouseY, 1, 1), pixel, 1, 0);
								p_Data = "GV = " + pixel[0];
							}
							p_MouseMemY = p_View_Rect.Y + p_MouseY * p_View_Rect.Height / _CanvasHeight;
							p_MouseMemX = p_View_Rect.X + p_MouseX * p_View_Rect.Width / _CanvasWidth;
						}
					}
				}
				SetProperty(ref _MouseX, value);
			}
		}
		private int _MouseY = 0;
		public int p_MouseY
		{
			get
			{
				return _MouseY;
			}
			set
			{
				SetProperty(ref _MouseY, value);
			}
		}
		private int _MouseMemX = 0;
		public int p_MouseMemX
		{
			get
			{
				return _MouseMemX;
			}
			set
			{
				SetProperty(ref _MouseMemX, value);
			}
		}
		private int _MouseMemY = 0;
		public int p_MouseMemY
		{
			get
			{
				return _MouseMemY;
			}
			set
			{
				SetProperty(ref _MouseMemY, value);
			}
		}
		private int _TumbMouseX = 0;
		public int p_TumbMouseX
		{
			get
			{
				return _TumbMouseX;
			}
			set
			{
				SetProperty(ref _TumbMouseX, value);
			}
		}

		private int _TumbMouseY = 0;
		public int p_TumbMouseY
		{
			get
			{
				return _TumbMouseY;
			}
			set
			{
				SetProperty(ref _TumbMouseY, value);
			}
		}

		private string _Data = "";
		public string p_Data
		{
			get
			{
				return _Data;
			}
			set
			{
				SetProperty(ref _Data, value);
			}
		}

		private double _Zoom = 1;
		public double p_Zoom
		{
			get
			{
				return _Zoom;
			}
			set
			{
				SetProperty(ref _Zoom, value);
				SetRoiRect();
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

		private Thickness _TumbnailImgMargin = new Thickness(0, 0, 0, 0);
		public Thickness p_TumbnailImgMargin
		{
			get
			{
				return _TumbnailImgMargin;
			}
			set
			{
				SetProperty(ref _TumbnailImgMargin, value);
			}
		}


		private System.Drawing.Rectangle _TumbnailImg_Rect = new System.Drawing.Rectangle();
		public System.Drawing.Rectangle p_TumbnailImg_Rect
		{
			get
			{
				return _TumbnailImg_Rect;
			}
			set
			{
				SetProperty(ref _TumbnailImg_Rect, value);
			}
		}

		private BitmapSource _ImageThumnail;
		public BitmapSource p_ImageThumnail
		{
			get
			{
				return _ImageThumnail;
			}
			set
			{
				SetProperty(ref _ImageThumnail, value);
			}
		}

		private bool ToolExist = false;
		bool InformationToolExist = false;
		public DrawToolVM SelectedTool;
		public DrawHistoryWorker m_HistoryWorker = new DrawHistoryWorker();
		public SimpleShapeDrawerVM m_SimpleShapeDrawerVM;
		public ModifyManager m_ModifyManager;
		public InformationDrawer informationDrawer;

		DrawingMode m_Mode = DrawingMode.None;
		public DrawingMode p_Mode
		{
			get
			{
				return m_Mode;
			}
			set
			{
				if (m_Mode != value)
				{
					switch (m_Mode)
					{
						case DrawingMode.None:
							{
								//MouseCursor = System.Windows.Input.Cursors.Arrow;
								break;
							}
						case DrawingMode.Drawing:
							{
								if (value != DrawingMode.None)
								{
									m_BasicTool.DrawEnd();
									m_BasicTool.p_State = false;
								}
								break;
							}
						case DrawingMode.AttachedTool:
							{
								if (value != DrawingMode.None)
								{
									SelectedTool.DrawEnd();
									SelectedTool.p_State = false;
								}
								break;
							}
						case DrawingMode.Modify:
							{
								if (value != DrawingMode.None)
								{
									m_ModifyManager.p_ModifyState = false;
									m_ModifyManager.DeleteModifyData();
									p_MouseCursor = System.Windows.Input.Cursors.Arrow;
								}
								break;
							}
					}
				}
				SetProperty(ref m_Mode, value);
			}
		}

		private readonly IDialogService m_DialogService;
		#endregion
		public UniquenessDrawerVM m_BasicTool;
		private UniquenessDrawerVM m_InformationTool;
		public ImageViewer_ViewModel(ImageData image = null, IDialogService dialogService = null, Dispatcher dispatcher = null)
		{
			if (dispatcher != null) dispatcher = Dispatcher.CurrentDispatcher;

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

			m_ModifyManager = new ModifyManager(this);

			m_BasicTool = new UniquenessDrawerVM(this);
			m_BasicTool.LineKeyValue = Key.LeftCtrl;
			m_BasicTool.RectangleKeyValue = Key.LeftShift;
			m_BasicTool.m_Stroke = System.Windows.Media.Brushes.Yellow;

			m_InformationTool = new UniquenessDrawerVM(this);
			m_InformationTool.m_Stroke = System.Windows.Media.Brushes.Red;

			p_Element = new ObservableCollection<UIElement>();
		}

        public void SetDrawer(DrawToolVM _SelectedTool)
		{
			SelectedTool = _SelectedTool;

			if (SelectedTool != null)
				ToolExist = true;
		}
		public void SetInformationViewer(InformationDrawer info)
		{
			informationDrawer = info;

			if (informationDrawer != null)
			{
				InformationToolExist = true;
			}
		}

		public void SetImageData(ImageData image)
		{
			p_ImageData = null;
			p_ImageData = image;
			image.OnCreateNewImage += image_NewImage;
			image.OnUpdateImage += image_OnUpdateImage;
			image.UpdateOpenProgress += image_UpdateOpenProgress;
			//InitRoiRect(p_ImageData.p_Size.X, p_ImageData.p_Size.Y);
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
			var asdf = ImageViewer.DataContextProperty;
			//((ImageViewer_ViewModel)(this.DataContext)).SetRoiRect();
			InitRoiRect(p_ImageData.p_Size.X, p_ImageData.p_Size.Y);
			SetImageSource();
		}
		public void InitRoiRect(int nWidth, int nHeight)
		{
			if (p_ImageData == null)
			{
				p_View_Rect = new System.Drawing.Rectangle(0, 0, nWidth, nHeight);
				p_Zoom = 1;
			}
			bool bRatio_WH = (double)p_View_Rect.Width / p_CanvasWidth < (double)p_View_Rect.Height / p_CanvasHeight;
			//m_View_Rect = new Rectangle(m_View_Rect.X, m_View_Rect.Y, m_View_Rect.Width, m_View_Rect.Height * ViewWidth / imgWidth);
			if (bRatio_WH)//세로가 길어
			{
				p_View_Rect = new System.Drawing.Rectangle(p_View_Rect.X, p_View_Rect.Y, p_View_Rect.Width, p_View_Rect.Width * p_CanvasHeight / p_CanvasWidth);
			}
			else
			{
				p_View_Rect = new System.Drawing.Rectangle(p_View_Rect.X, p_View_Rect.Y, p_View_Rect.Height * p_CanvasWidth / p_CanvasHeight, p_View_Rect.Height);
				//p_View_Rect = new Rectangle(p_View_Rect.X, p_View_Rect.Y, p_View_Rect.Width, p_View_Rect.Width * p_CanvasHeight / p_CanvasWidth);
			}
			SetThumNailIamge();
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
				InspectionManager.RefreshDefectDraw();

			}
		}
		public void SetThumbNailSize(int width, int height)
		{
			p_ThumbWidth = width;
			p_ThumbHeight = height;
		}
		public unsafe void SetThumNailIamge()
		{
			return;
			/*			if (p_ImageData.p_nByte == 1)
						{
							Image<Gray, byte> view = new Image<Gray, byte>(p_ThumbWidth, p_ThumbHeight);
							IntPtr ptrMem = m_ImageData.GetPtr();
							if (ptrMem == IntPtr.Zero) return;
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
							IntPtr ptrMem = p_ImageData.GetPtr();
							if (ptrMem == IntPtr.Zero) return;
							int pix_x = 0;
							int pix_y = 0;

							for (int yy = 0; yy < p_ThumbHeight; yy++)
							{
								pix_y = yy * p_ImageData.p_Size.Y / p_ThumbHeight;
								for (int xx = 0; xx < p_ThumbWidth; xx++)
								{
									pix_x = xx * p_ImageData.p_Size.X / p_ThumbWidth;
									view.Data[yy, xx, 2] = ((byte*)ptrMem)[0 + p_ImageData.p_nByte * (pix_x + (long)pix_y * p_ImageData.p_Size.X)];
									view.Data[yy, xx, 1] = ((byte*)ptrMem)[1 + p_ImageData.p_nByte * (pix_x + (long)pix_y * p_ImageData.p_Size.X)];
									view.Data[yy, xx, 0] = ((byte*)ptrMem)[2 + p_ImageData.p_nByte * (pix_x + (long)pix_y * p_ImageData.p_Size.X)];
								}
							}
							if (view.Width != 0 && view.Height != 0)
								p_ThumNailImgSource = ImageHelper.ToBitmapSource(view);
						}
			*/
		}
		public unsafe void SetImageSource()
		{
			try
			{
				if (p_ImageData != null)
				{
					if (p_ImageData.GetBytePerPixel() == 1)
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
					else if (p_ImageData.GetBytePerPixel() == 3)
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

								view.Data[yy, xx, 2] = ((byte*)ptrMem)[0 + m_ImageData.GetBytePerPixel() * ((long)pix_x + pix_rect)];
								view.Data[yy, xx, 1] = ((byte*)ptrMem)[1 + m_ImageData.GetBytePerPixel() * ((long)pix_x + pix_rect)];
								view.Data[yy, xx, 0] = ((byte*)ptrMem)[2 + m_ImageData.GetBytePerPixel() * ((long)pix_x + pix_rect)];

							}
						}

						p_ImgSource = ImageHelper.ToBitmapSource(view);

						p_TumbnailImgMargin = new Thickness(Convert.ToInt32((double)p_View_Rect.X * p_ThumbWidth / m_ImageData.p_Size.X), Convert.ToInt32((double)p_View_Rect.Y * p_ThumbHeight / m_ImageData.p_Size.Y), 0, 0);
						if (Convert.ToInt32((double)p_View_Rect.Height * p_ThumbHeight / m_ImageData.p_Size.Y) == 0)
							p_TumbnailImg_Rect = new System.Drawing.Rectangle(0, 0, Convert.ToInt32((double)p_View_Rect.Width * p_ThumbWidth / m_ImageData.p_Size.X), 2);
						else
							p_TumbnailImg_Rect = new System.Drawing.Rectangle(0, 0, Convert.ToInt32((double)p_View_Rect.Width * p_ThumbWidth / m_ImageData.p_Size.X), Convert.ToInt32((double)p_View_Rect.Height * p_ThumbHeight / m_ImageData.p_Size.Y));

					}
					//if (p_arrDefectDataWrapper != null)
					//               {
					//	for (int i = 0; i<p_arrDefectDataWrapper.Count; i++)
					//                   {

					//                   }
					//               }
				}
			}
			catch (Exception ee)
			{
				TempLogger.Write("ImageViewer", ee);
				//System.Windows.MessageBox.Show(ee.ToString());
			}

            //System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //{
            RedrawingElement();
            //});

        }

		public unsafe Image<Gray, byte> GetSamplingGrayImage()
		{
			Rect rect = new Rect(p_View_Rect.X, p_View_Rect.Y, p_View_Rect.Width, p_View_Rect.Height);
			return GetSamplingGrayImage(m_ImageData, rect, p_CanvasWidth, p_CanvasHeight);
		}
		public unsafe Image<Gray, byte> GetSamplingGrayImage(ImageData imgData, Rect rect, int canvasWidth, int canvasHeight)
		{
			IntPtr ptrMem = imgData.GetPtr();
			if (ptrMem == IntPtr.Zero)
				return null;

			Image<Gray, byte> view = new Image<Gray, byte>(canvasWidth, canvasHeight);

			byte[,,] viewPtr = view.Data;
			byte* imageptr = (byte*)imgData.GetPtr();

			Parallel.For(0, canvasHeight, new ParallelOptions { MaxDegreeOfParallelism = 12 }, (yy) =>
			{
				{
					long pix_y = (long)(rect.Y + yy * rect.Height / canvasHeight);

					for (int xx = 0; xx < canvasWidth; xx++)
					{
						long pix_x = (long)(rect.X + xx * rect.Width / canvasWidth);
						view.Data[yy, xx, 0] = imageptr[pix_x + (long)pix_y * imgData.p_Size.X];
					}
				}
			});

			return view;
		}
		public unsafe Image<Rgb, byte> GetSamplingRgbImage()
		{
			Rect rect = new Rect(p_View_Rect.X, p_View_Rect.Y, p_View_Rect.Width, p_View_Rect.Height);
			return GetSamplingRgbImage(m_ImageData, rect, p_CanvasWidth, p_CanvasHeight);
		}
		public unsafe Image<Rgb, byte> GetSamplingRgbImage(ImageData imgData, Rect rect, int canvasWidth, int canvasHeight, bool isRgbOrder = true)
		{
			IntPtr ptrMem = imgData.GetPtr();
			if (ptrMem == IntPtr.Zero)
				return null;

			Image<Rgb, byte> view = new Image<Rgb, byte>(canvasWidth, canvasHeight);

			byte[,,] viewPtr = view.Data;
			byte* imageptr = (byte*)imgData.GetPtr();
			int nBytePerPixel = imgData.GetBytePerPixel();

			int size = imgData.p_Size.X;
			Parallel.For(0, canvasHeight, new ParallelOptions { MaxDegreeOfParallelism = 12 }, (yy) =>
			{
				try
				{
					long pix_y = (long)(rect.Y + yy * rect.Height / canvasHeight);
					for (int xx = 0; xx < canvasWidth; xx++)
					{
						long pix_x = (long)(rect.X + xx * rect.Width / canvasWidth);

						if (isRgbOrder)
						{
							viewPtr[yy, xx, 0] = imageptr[(pix_x * nBytePerPixel + 0) + (long)pix_y * (imgData.p_Size.X * 3)];
							viewPtr[yy, xx, 1] = imageptr[(pix_x * nBytePerPixel + 1) + (long)pix_y * (imgData.p_Size.X * 3)];
							viewPtr[yy, xx, 2] = imageptr[(pix_x * nBytePerPixel + 2) + (long)pix_y * (imgData.p_Size.X * 3)];
						}
						else
						{
							viewPtr[yy, xx, 2] = imageptr[(pix_x * nBytePerPixel + 0) + (long)pix_y * (size * 3)];
							viewPtr[yy, xx, 1] = imageptr[(pix_x * nBytePerPixel + 1) + (long)pix_y * (size * 3)];
							viewPtr[yy, xx, 0] = imageptr[(pix_x * nBytePerPixel + 2) + (long)pix_y * (size * 3)];
						}
					}
				}
				catch (Exception ex)
				{

				}
			});

			return view;
		}

		public void SetImageSource(Image<Gray, byte> img)
		{
			try
			{
				if (img != null)
				{
					p_ImgSource = ImageHelper.ToBitmapSource(img);

					p_TumbnailImgMargin = new Thickness(Convert.ToInt32((double)p_View_Rect.X * p_ThumbWidth / m_ImageData.p_Size.X), Convert.ToInt32((double)p_View_Rect.Y * p_ThumbHeight / m_ImageData.p_Size.Y), 0, 0);
					if (Convert.ToInt32((double)p_View_Rect.Height * p_ThumbHeight / m_ImageData.p_Size.Y) == 0)
						p_TumbnailImg_Rect = new System.Drawing.Rectangle(0, 0, Convert.ToInt32((double)p_View_Rect.Width * p_ThumbWidth / m_ImageData.p_Size.X), 2);
					else
						p_TumbnailImg_Rect = new System.Drawing.Rectangle(0, 0, Convert.ToInt32((double)p_View_Rect.Width * p_ThumbWidth / m_ImageData.p_Size.X), Convert.ToInt32((double)p_View_Rect.Height * p_ThumbHeight / m_ImageData.p_Size.Y));
				}
			}
			catch (Exception ee)
			{
				System.Windows.MessageBox.Show(ee.ToString());
			}

			RedrawingElement();
		}
		public void SetImageSource(Image<Rgb, byte> img)
		{
			try
			{
				if (img != null)
				{
					p_ImgSource = ImageHelper.ToBitmapSource(img);

					p_TumbnailImgMargin = new Thickness(Convert.ToInt32((double)p_View_Rect.X * p_ThumbWidth / m_ImageData.p_Size.X), Convert.ToInt32((double)p_View_Rect.Y * p_ThumbHeight / m_ImageData.p_Size.Y), 0, 0);
					if (Convert.ToInt32((double)p_View_Rect.Height * p_ThumbHeight / m_ImageData.p_Size.Y) == 0)
						p_TumbnailImg_Rect = new System.Drawing.Rectangle(0, 0, Convert.ToInt32((double)p_View_Rect.Width * p_ThumbWidth / m_ImageData.p_Size.X), 2);
					else
						p_TumbnailImg_Rect = new System.Drawing.Rectangle(0, 0, Convert.ToInt32((double)p_View_Rect.Width * p_ThumbWidth / m_ImageData.p_Size.X), Convert.ToInt32((double)p_View_Rect.Height * p_ThumbHeight / m_ImageData.p_Size.Y));
				}
			}
			catch (Exception ee)
			{
				System.Windows.MessageBox.Show(ee.ToString());
			}

			RedrawingElement();
		}
		void TumbNailMove()
		{
			if (MouseEvent.LeftButton == MouseButtonState.Pressed)
			{
				double perX = (double)p_TumbMouseX / p_ThumbWidth;
				double perY = (double)p_TumbMouseY / p_ThumbHeight;
				CanvasMovePoint(perX, perY);
			}
		}
		void ThumNailMoveStart()
		{
			TumbNailMove();
		}
		void _btnOpenImage()
		{
			if (m_ImageData == null)
			{
				System.Windows.Forms.MessageBox.Show("Image를 열어주세요");
				return;
			}
			CancelCommand();
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "Image Files(*.bmp;*.jpg)|*.bmp;*.jpg";
			if (ofd.ShowDialog() == DialogResult.OK)
			{
				//var viewModel = new Dialog_ImageOpenViewModel(this);
				//Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
				if (true)
				{
					if (true)
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
		void _btnSaveImage()
		{
			if (m_ImageData == null)
			{
				System.Windows.Forms.MessageBox.Show("Image를 열어주세요");
				return;
			}
			CancelCommand();

			//if (m_BasicTool.m_Element.Count == 0 || m_BasicTool.m_Element[0].GetType() != typeof(System.Windows.Shapes.Rectangle))
			//	m_ImageData.SaveWholeImage();
			//else
			//{
			var temp = m_BasicTool.m_TempRect;
			int left = (int)temp.StartPos.X;
			int top = (int)temp.StartPos.Y;

			int right = (int)temp.EndPos.X;
			int bot = (int)temp.EndPos.Y;

			m_ImageData.SaveRectImage(new CRect(left, top, right, bot));
			//}
		}
		void ImageClear()
		{
			if (m_ImageData != null)
				m_ImageData.ClearImage();
		}

		void CancelCommand()
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
			InspectionManager.RefreshDefectDraw();
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

		#region Point Helper
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
			if (nX % 2 != 0) nX += 1;
			if (nY % 2 != 0) nY += 1;
			return new CPoint(nX, nY);
		}
		CPoint GetCurrentPoint()
		{
			int nX = p_View_Rect.X + p_View_Rect.Width / 2;
			int nY = p_View_Rect.Y + p_View_Rect.Height / 2;

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
				nX = p_View_Rect.X + Convert.ToInt32(p_View_Rect.Width - nImgWidth * p_Zoom) * p_MouseX / _CanvasWidth; // 마우스 커서기준으로 확대/축소
				nY = p_View_Rect.Y + Convert.ToInt32(p_View_Rect.Height - nImgWidth * p_Zoom * p_CanvasHeight / p_CanvasWidth) * p_MouseY / _CanvasHeight;
				viewrectwidth = Convert.ToInt32(nImgWidth * p_Zoom);
				viewrectheight = Convert.ToInt32(nImgWidth * p_Zoom * p_CanvasHeight / p_CanvasWidth);
			}
			else
			{
				nX = p_View_Rect.X + Convert.ToInt32(p_View_Rect.Width - nImgHeight * p_Zoom * p_CanvasWidth / p_CanvasHeight) * p_MouseX / _CanvasWidth;
				nY = p_View_Rect.Y + Convert.ToInt32(p_View_Rect.Height - nImgHeight * p_Zoom) * p_MouseY / _CanvasHeight;
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
		#endregion

		StopWatch m_swMouse = new StopWatch();
		CPoint m_ptBuffer1 = new CPoint();
		CPoint m_PtMouseBuffer = new CPoint();

		private void StartImageMove()
		{
			m_ptBuffer1 = new CPoint(p_View_Rect.X, p_View_Rect.Y);
			m_PtMouseBuffer = new CPoint(p_MouseX, p_MouseY);
			m_swMouse.Restart();
		}

		public void RedrawingElement()
		{
			p_Element.Clear();

			if (InformationToolExist)
			{
				foreach (UIElement TempElement in informationDrawer.m_Element)
					p_Element.Add(TempElement);

				informationDrawer.Redrawing();
			}

			if (ToolExist)
			{
				foreach (UIElement TempElement in SelectedTool.m_Element)
					p_Element.Add(TempElement);

				SelectedTool.Redrawing();
				if (p_Mode == DrawingMode.AttachedTool)
					SelectedTool.Drawing();
			}

			if (m_BasicTool != null)
			{
				foreach (UIElement TempElement in m_BasicTool.m_Element)
					p_Element.Add(TempElement);
				m_BasicTool.Redrawing();

				if (p_Mode == DrawingMode.Drawing)
				{
					p_ViewerUIElement.Clear();
					m_BasicTool.Drawing();
					p_ViewerUIElement.Add(m_BasicTool.DrawnTb);
				}
			}

			if (m_ModifyManager != null && m_ModifyManager.p_ModifyRect != null)
			{
				p_Element.Add(m_ModifyManager.p_ModifyRect);
				m_ModifyManager.Redrawing();
			}
		}

		private void RedrawingHistory()
		{
			p_Element.Clear();

			if (ToolExist)
			{
				foreach (UIElement TempElement in SelectedTool.m_Element)
					p_Element.Add(TempElement);

				SelectedTool.Redrawing();
				if (p_Mode == DrawingMode.AttachedTool)
					SelectedTool.Drawing();
			}

			if (m_BasicTool != null)
			{
				foreach (UIElement TempElement in m_BasicTool.m_Element)
					p_Element.Add(TempElement);
				m_BasicTool.Redrawing();
			}

			if (m_ModifyManager != null && m_ModifyManager.p_ModifyRect != null)
			{
				p_Element.Add(m_ModifyManager.p_ModifyRect);
				m_ModifyManager.Redrawing();
			}
		}

		private void KeyPressedDownFunction()
		{
			if (KeyEvent != null)
			{
				for (int i = 0; i < m_BasicTool.p_KeyList.Length; i++)
					if (m_BasicTool.p_KeyList[i] != Key.None && KeyEvent.KeyboardDevice.IsKeyDown(m_BasicTool.p_KeyList[i]))
					{
						m_BasicTool.SetShape(i);
						p_Mode = DrawingMode.Drawing;

					}
				if (ToolExist)
				{
					for (int i = 0; i < SelectedTool.p_KeyList.Length; i++)
						if (SelectedTool.p_KeyList[i] != Key.None && KeyEvent.KeyboardDevice.IsKeyDown(SelectedTool.p_KeyList[i]))
						{
							p_ViewerUIElement.Clear();
							SelectedTool.SetShape(i);
							p_ViewerUIElement.Add(SelectedTool.m_ShapeIcon);

							p_Mode = DrawingMode.AttachedTool;

						}
				}

				if (KeyEvent.KeyboardDevice.Modifiers == ModifierKeys.Control)
				{
					if (!m_BasicTool.p_State && (ToolExist == true && !SelectedTool.p_State))
					{
						ModifyManager _ModifyManager = null;
						if (p_Mode == DrawingMode.Modify)
							_ModifyManager = m_ModifyManager;
						if (KeyEvent.KeyboardDevice.IsKeyDown(Key.Z))
						{
							m_HistoryWorker.Do_CtrlZ(_ModifyManager);
							RedrawingHistory();
						}
						if (KeyEvent.KeyboardDevice.IsKeyDown(Key.Y))
						{
							m_HistoryWorker.Do_CtrlY(_ModifyManager);
							RedrawingHistory();
						}
					}
				}

			}
		}

		private void KeyPressedUpFunction()
		{
			if (KeyEvent != null)
			{

				for (int i = 0; i < m_BasicTool.p_KeyList.Length; i++)
					if (m_BasicTool.p_KeyList[i] != Key.None && KeyEvent.KeyboardDevice.IsKeyUp(m_BasicTool.p_KeyList[i]))
					{
						if (p_Mode == DrawingMode.Drawing && m_BasicTool.p_State == false)
						{
							//p_ViewerUIElement.Clear();
							m_BasicTool.SetShape(-1);
							p_Mode = DrawingMode.None;

						}
					}
				if (ToolExist)
				{
					for (int i = 0; i < SelectedTool.p_KeyList.Length; i++)
						if (SelectedTool.p_KeyList[i] != Key.None && KeyEvent.KeyboardDevice.IsKeyUp(SelectedTool.p_KeyList[i]))
						{
							if (p_Mode == DrawingMode.AttachedTool && SelectedTool.p_State == false)
							{
								p_ViewerUIElement.Clear();
								SelectedTool.SetShape(-1);
								p_Mode = DrawingMode.None;

							}
						}
				}
			}
		}

		private void LeftUp()
		{
			MouseState = MouseEvent.LeftButton;
			switch (p_Mode)
			{
				case DrawingMode.Modify:
					{

						if (!m_ModifyManager.p_SetStateDone)
							m_ModifyManager.p_SetStateDone = true;
						else
							if (m_ModifyManager.p_ModifyState && m_ModifyManager.m_MouseHitType != ModifyManager.HitType.None)
						{
							m_ModifyManager.ModifyEnd();
						}
						break;
					}
			}

		}

		private void LeftDown()
		{
			MouseState = MouseEvent.LeftButton;
			if (m_ImageData == null)
				return;
			StartImageMove();
			switch (p_Mode)
			{
				case DrawingMode.None:
					{
						break;
					}
				case DrawingMode.Drawing:
					{
						if (m_BasicTool.p_State == false)
						{
							m_BasicTool.p_State = true;
							m_BasicTool.DrawStart();
						}
						break;
					}
				case DrawingMode.AttachedTool:
					{
						if (SelectedTool.p_State == false)
						{
							SelectedTool.p_State = true;
							SelectedTool.DrawStart();
						}
						break;
					}
				case DrawingMode.Modify:
					{
						if (m_ModifyManager.m_MouseHitType != ModifyManager.HitType.None)
						{
							m_ModifyManager.p_ModifyState = true;
							m_ModifyManager.ModifyStart();
						}

						break;
					}
			}
		}

		private void MouseMove()
		{

			if (m_ImageData == null)
				return;

			int m_nMouseMoveDelay = 0;

			switch (p_Mode)
			{
				default:
					{
						if (MouseState == MouseButtonState.Pressed && m_swMouse.ElapsedMilliseconds > m_nMouseMoveDelay)
						{
							CPoint point = m_ptBuffer1;
							CanvasMovePoint_Ref(point, m_PtMouseBuffer.X - p_MouseX, m_PtMouseBuffer.Y - p_MouseY);
						}
						break;
					}
				case DrawingMode.Tool:
					{
						break;
					}
				case DrawingMode.Modify:
					{
						if (MouseState == MouseButtonState.Pressed && m_swMouse.ElapsedMilliseconds > m_nMouseMoveDelay)
						{
							if (m_ModifyManager.m_MouseHitType == ModifyManager.HitType.None)
							{
								CPoint point = m_ptBuffer1;
								CanvasMovePoint_Ref(point, m_PtMouseBuffer.X - p_MouseX, m_PtMouseBuffer.Y - p_MouseY);
							}
							else
							{
								m_ModifyManager.AdjustOrigin(new CPoint(p_MouseMemX, p_MouseMemY));
								//크기변환 혹은 위치변환.

							}
							//SelectedTool.ModifyTarget;
						}
						else
						{
							CPoint MousePoint = new CPoint(p_MouseX, p_MouseY);
							m_ModifyManager.m_MouseHitType = m_ModifyManager.SetHitType(MousePoint);
							p_MouseCursor = m_ModifyManager.SetMouseCursor(m_ModifyManager.m_MouseHitType);

						}

						break;
					}
			}

			switch (p_Mode)
			{
				case DrawingMode.None:
					{
						break;
					}
				case DrawingMode.Drawing:
					{
						if (m_BasicTool.p_State == true)
						{
							m_BasicTool.Drawing();
							RedrawingElement();
						}

						break;
					}
				case DrawingMode.AttachedTool:
					{
						if (SelectedTool.p_State == true)
						{
							SelectedTool.Drawing();
							RedrawingElement();
						}
						break;
					}
			}
		}

		private void RightDown()
		{
			if (m_ImageData == null)
				return;
			switch (p_Mode)
			{
				case DrawingMode.None:
					{
						break;
					}
				case DrawingMode.Drawing:
					{
						if (m_BasicTool.p_State == true)
						{
							m_BasicTool.DrawEnd();
							p_ViewerUIElement.Clear();
							p_ViewerUIElement.Add(m_BasicTool.DrawnTb);
							m_BasicTool.p_State = false;
							p_Mode = DrawingMode.None;
						}
						break;
					}
				case DrawingMode.AttachedTool:
					{
						if (SelectedTool.p_State == true)
						{
							SelectedTool.DrawEnd();
							SelectedTool.p_State = false;
							p_ViewerUIElement.Clear();
							p_Mode = DrawingMode.None;
						}
						break;
					}
				case DrawingMode.Modify:
					{
						if (m_ModifyManager.m_MouseHitType == ModifyManager.HitType.None)
						{
							m_ModifyManager.p_ModifyState = false;
							m_ModifyManager.DeleteModifyData();
							p_Mode = DrawingMode.None;
						}
						break;
					}
			}
		}

		public void ClearShape()
		{
			m_BasicTool.Clear();
			if (ToolExist == true)
				SelectedTool.Clear();
			p_Mode = DrawingMode.None;

		}

		public void KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			KeyEvent = e;
		}
		public void OnMouseDown(Object sender, System.Windows.Input.MouseEventArgs e)
		{
			//var viewer = (Canvas)sender;
			//viewer.Focus();
		}
		public void OnMouseEnter(Object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (MouseState == MouseButtonState.Pressed)
				MouseState = MouseEvent.LeftButton;
			var viewer = (Grid)sender;
			viewer.Focus();
		}
		public void OnMouseWheel(object sender, MouseWheelEventArgs e)
		{

			var viewer = (Grid)sender;
			viewer.Focus();
			try
			{
				int lines = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
				double zoom = _Zoom;

				if (lines < 0)
				{
					zoom *= 1.4F;
				}
				if (lines > 0)
				{
					zoom *= 0.6F;
				}

				//double nDev = 0;
				//int absDelta = Math.Abs(e.Delta);
				//if ((minMouseDelta > absDelta) && (absDelta > 0))
				//    minMouseDelta = absDelta;
				//int factor = absDelta / minMouseDelta;
				//if (factor < 1)
				//    factor = 1;
				//if (factor > 6)
				//    factor = 6;
				//if (e.Delta > 0)
				//    nDev = -0.01;
				//else 
				//    nDev = 0.01;

				//    //p_Zoom += (nDev * Factor[factor-1]);
				//   p_Zoom += nDev;
				if (zoom > 1)
				{
					zoom = 1;
				}
				else if (p_Zoom < 0.0001)
				{
					zoom = 0.0001;
				}
				p_Zoom = zoom;
				//SetRoiRect();
			}
			catch (Exception ee)
			{
				TempLogger.Write("ImageViewer", ee);
				//System.Windows.Forms.MessageBox.Show(ee.ToString());
			}
			//if (ToolExist)
			//    SelectedTool.Drawing();
		}


		#region ICommand

		public ICommand SaveImageCommand
		{
			get
			{
				return new RelayCommand(p_ImageData.SaveWholeImage);
			}
		}
		public ICommand CanvasMouseLeftUp
		{
			get
			{
				return new RelayCommand(LeftUp);
			}
		}
		public ICommand CanvasMouseLeftDown
		{
			get
			{
				return new RelayCommand(LeftDown);
			}
		}
		public ICommand CanvasMouseMove
		{
			get
			{
				return new RelayCommand(MouseMove);
			}
		}
		public ICommand btnClickOpenImage
		{
			get
			{
				return new RelayCommand(_btnOpenImage);
			}
		}
		public ICommand btnClickSaveImage
		{
			get
			{
				return new RelayCommand(_btnSaveImage);
			}
		}
		public ICommand CopyCancelCommand
		{
			get
			{
				return new RelayCommand(CancelCommand);
			}
		}
		public ICommand CommandImageClear
		{
			get
			{
				return new RelayCommand(ImageClear);
			}
		}
		public ICommand TumbNailMouseMove
		{
			get
			{
				return new RelayCommand(TumbNailMove);
			}
		}
		public ICommand TumbNailMouseLeftDown
		{
			get
			{
				return new RelayCommand(ThumNailMoveStart);
			}
		}
		public ICommand KeyDelCommand
		{
			get
			{
				return new RelayCommand(ClearShape);
			}
		}
		public ICommand KeyUpCommand
		{
			get
			{
				return new RelayCommand(delegate
				{
					CanvasMoveCanvasPoint(Convert.ToInt32(p_CanvasWidth / 2), Convert.ToInt32(p_CanvasHeight / 4));
				}
				);
			}
		}
		public ICommand KeyDownCommand
		{
			get
			{
				return new RelayCommand(delegate
				{

					CanvasMoveCanvasPoint(Convert.ToInt32(p_CanvasWidth / 2), Convert.ToInt32(p_CanvasHeight * 3 / 4));
				}
				);
			}
		}
		public ICommand KeyLeftCommand
		{
			get
			{
				return new RelayCommand(delegate
				{
					CanvasMoveCanvasPoint(Convert.ToInt32(p_CanvasWidth / 4), Convert.ToInt32(p_CanvasHeight / 2));
				}
				);
			}
		}
		public ICommand KeyPressedDownCommand
		{
			get { return new RelayCommand(KeyPressedDownFunction); }
		}
		public ICommand KeyPressedUpCommand
		{
			get { return new RelayCommand(KeyPressedUpFunction); }
		}
		public ICommand KeyRightCommand
		{
			get
			{
				return new RelayCommand(delegate
				{
					CanvasMoveCanvasPoint(Convert.ToInt32(p_CanvasWidth * 3 / 4), Convert.ToInt32(p_CanvasHeight / 2));
				}
				);
			}
		}
		public ICommand CanvasMouseRightDown
		{
			get
			{
				return new RelayCommand(RightDown);
			}
		}
		public ICommand LoadedCommand
		{
			get
			{
				return new RelayCommand(Loaded_function);
			}
		}
		public ICommand MouseDoubleClickCommand
        {
            get
            {
				return new RelayCommand(MouseDoubleClick);
            }
        }
		public event LoadedDelegate m_AfterLoaded;
		void Loaded_function()
		{
			if (p_ImageData != null)
				InitRoiRect(p_ImageData.p_Size.X, p_ImageData.p_Size.Y);
			if (m_AfterLoaded != null)
				m_AfterLoaded();
		}

		public EventHandler DoubleClicked;
		void MouseDoubleClick()
        {
			if (DoubleClicked != null)
				DoubleClicked.Invoke(new CPoint(p_MouseMemX, p_MouseMemY), null);

		}
		#endregion
	}

	public class DrawHelper
	{
		public TextBlock DrawnTb = new TextBlock();
		public System.Windows.Shapes.Rectangle DrawnRect = new System.Windows.Shapes.Rectangle();
		public CPoint Rect_StartPt = new CPoint();
		public CPoint Rect_EndPt = new CPoint();
		public CRect preRect = new CRect();
	}
}
