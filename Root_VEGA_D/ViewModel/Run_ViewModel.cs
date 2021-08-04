using Root_VEGA_D.Engineer;
using RootTools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_VEGA_D
{
	public enum ImageType
	{
		Ref,
		Dif,
		Cur,
	}
	class Run_ViewModel : ObservableObject
	{
		public MainWindow_ViewModel p_main { get; set; }
		public VEGA_D_Handler m_handler { get; set; }

		#region ResultTable
		DataTable _OriginResultTable;
		DataTable _ResultTable;
		public DataTable ResultTable
		{
			get { return this._ResultTable; }
			set
			{
				this._ResultTable = value;
				this.RaisePropertyChanged();
			}
		}

		#endregion

		#region SelectedResultTable
		DataRowView _SelectedResultTable;
		public DataRowView SelectedResultTable
		{
			get { return this._SelectedResultTable; }
			set
			{
				if (value != null)
				{
					this._SelectedResultTable = value;
					UpdateData();
					RaisePropertyChanged();
				}
			}
		}

		public Dispatcher Dispatcher { get; internal set; }
		#endregion

		#region SelectedTDIImage
		private ImageSource _SelectedTDIImage;
		public ImageSource SelectedTDIImage
		{
			get { return this._SelectedTDIImage; }
			set
			{
				this._SelectedTDIImage = value;
				this.RaisePropertyChanged();
			}
		}
		#endregion

		#region SelectedProcessImage
		private ImageSource _SelectedProcessImage;
		public ImageSource SelectedProcessImage
		{
			get { return this._SelectedProcessImage; }
			set
			{
				this._SelectedProcessImage = value;
				this.RaisePropertyChanged();
			}
		}
		#endregion

		#region SelectedCurrentImage
		private ImageSource _SelectedCurrentImage;
		public ImageSource SelectedCurrentImage
		{
			get { return this._SelectedCurrentImage; }
			set
			{
				this._SelectedCurrentImage = value;
				this.RaisePropertyChanged();
			}
		}
		#endregion

		public Run_ViewModel()
		{
			m_handler = (VEGA_D_Handler)App.m_engineer.ClassHandler();
		}

		public Run_ViewModel(MainWindow_ViewModel main)
		{
			p_main = main;
		}
		/// <summary>
		/// 선택된 Defect 정보를 Update한다
		/// </summary>
		private void UpdateData()
		{
			int idx;
			if (SelectedResultTable == null)
			{
				SelectedTDIImage = null;
				SelectedProcessImage = null;
				SelectedCurrentImage = null;

				return;
			}
			if (int.TryParse(SelectedResultTable["idx"].ToString(), out idx))
			{
				//Image파일을 불러올 Index확보
				//{0}_{1:D8}_{2}-{3}-{4}-abs.bmp", defaultFileName, currentDefectIdx, SelectedDefect->StartX, SelectedDefect->StartY, SelectedDefect->GV
				string refTargetKey = string.Format("_{0:D8}Ref.bmp", idx);
				string absTargetKey = string.Format("_{0:D8}Dif.bmp", idx);
				string curTargetKey = string.Format("_{0:D8}Cur.bmp", idx);

				//파일명을 가져오고 웹서버에서 이미지를 가져오도록 구현한다. 웹서버에서 가져온 이미지는 캐시 폴더를 사용, 캐시 폴더는 파일 생성일자를 봐서 한달이 넘은 경우 지우고 새로 받도록 구현한다
				var inspID = SelectedResultTable["InspectionID"].ToString();
				var nameKey = "Defect";
				var refFileName = nameKey + refTargetKey;
				var absFIleName = nameKey + absTargetKey;
				var curFIleName = nameKey + curTargetKey;
				var cacheDirPath = Path.Combine(App.MainFolder, "CacheFolder", inspID);

				bool refCheck = false;
				bool absCheck = false;
				bool curCheck = false;

				var refImage = DownloadImage(cacheDirPath, ImageType.Ref, inspID, idx, "Defect");
				if (refImage != null)
				{
					SelectedTDIImage = ConvertImage(refImage);
					refCheck = true;
					refImage.Dispose();
				}
				else
				{
					SelectedTDIImage = null;
				}

				var absImage = DownloadImage(cacheDirPath, ImageType.Dif, inspID, idx, "Defect");
				if (absImage != null)
				{
					SelectedProcessImage = ConvertImage(absImage);
					absCheck = true;
					absImage.Dispose();
				}
				else
				{
					SelectedProcessImage = null;
				}

				var curImage = DownloadImage(cacheDirPath, ImageType.Cur, inspID, idx, "Defect");
				if (curImage != null)
				{
					SelectedCurrentImage = ConvertImage(curImage);
					curCheck = true;
					curImage.Dispose();
				}
				else
				{
					SelectedCurrentImage = null;
				}

				if (!refCheck && !absCheck && !curCheck)
				{
					//없으면 surface이미지로 한번 더 탐색. surface는 abs와 ref가 없으므로 강제로 비운다
					curImage = DownloadImage(cacheDirPath, ImageType.Cur, inspID, idx, "Surface");
					if (curImage != null)
					{
						SelectedCurrentImage = ConvertImage(curImage);
						curImage.Dispose();
						SelectedTDIImage = null;
						SelectedProcessImage = null;
					}
					else
					{
						SelectedCurrentImage = null;
						SelectedTDIImage = null;
						SelectedProcessImage = null;
					}
				}
			}
		}
		Bitmap DownloadImage(string targetDirPath, ImageType type, string inspID, int idx, string nameKey)
		{
			if (!App.IsServerEnabled)
			{
				return null;
			}
			//Image파일을 불러올 Index확보
			//{0}_{1:D8}_{2}-{3}-{4}-abs.bmp", defaultFileName, currentDefectIdx, SelectedDefect->StartX, SelectedDefect->StartY, SelectedDefect->GV
			string targetKey = string.Format("_{0:D8}{1}.bmp", idx, type.ToString());

			//파일명을 가져오고 웹서버에서 이미지를 가져오도록 구현한다. 웹서버에서 가져온 이미지는 캐시 폴더를 사용, 캐시 폴더는 파일 생성일자를 봐서 한달이 넘은 경우 지우고 새로 받도록 구현한다
			//var inspID = SelectedDataTable["InspectionID"].ToString();
			var fileName = nameKey + targetKey;

			var fileUrl = App.ServerIP + ":" + App.ServerWebPort + "/" + inspID + "/" + fileName;//웹서버(IPU PC) 정보가 필요함

			if (!fileUrl.StartsWith("http://"))
			{
				fileUrl = "http://" + fileUrl;
			}
			if (!Directory.Exists(targetDirPath))
			{
				Directory.CreateDirectory(targetDirPath);
			}

			using (WebClient webClient = new WebClient())
			{
				try
				{
					var refImageFilePath = Path.Combine(targetDirPath, fileName);

					if (File.Exists(refImageFilePath))
					{
						//생성 날짜를 check하고 한달이 넘었으면 삭제하고 새로 받는다
						var info = new FileInfo(refImageFilePath);
						if (info.CreationTime.AddDays(30) < DateTime.Now)
						{
							File.Delete(refImageFilePath);
							webClient.DownloadFile(fileUrl, refImageFilePath);
						}
					}
					else
					{
						//파일 없으면 받는다
						webClient.DownloadFile(fileUrl, refImageFilePath);
					}
					//캐시폴더에서 이미지 로드
					if (System.IO.File.Exists(refImageFilePath))
					{
						return new Bitmap(refImageFilePath);

					}
					else
					{
						return null;
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
					return null;
				}
			}
		}
		public System.Windows.Media.ImageSource ConvertImage(System.Drawing.Image image)
		{
			try
			{
				if (image != null)
				{
					var bitmap = new System.Windows.Media.Imaging.BitmapImage();
					bitmap.BeginInit();
					System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
					image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
					memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
					bitmap.StreamSource = memoryStream;
					bitmap.EndInit();
					return bitmap;
				}
			}
			catch { }
			return null;
		}
	}
}
