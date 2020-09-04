using ATI;
using Root_Vega.Models.InspectionReview;
using RootTools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Root_Vega.Controls
{
	class InspResultViewModel : ObservableObject
	{
		#region ResultDataTable
		DataTable _OriginResultDataTable;
		DataTable _ResultDataTable;
		public DataTable ResultDataTable
		{
			get { return this._ResultDataTable; }
			set
			{
				this._ResultDataTable = value;
				this.RaisePropertyChanged();
			}
		}

		#endregion

		#region Title
		string _Title;
		public string Title
		{
			get { return this._Title; }
			set
			{
				this._Title = value;
				this.RaisePropertyChanged();
			}
		}

		#endregion

		#region TopMost
		bool _TopMost;
		public bool TopMost
		{
			get { return this._TopMost; }
			set
			{
				this._TopMost = value;
				this.RaisePropertyChanged();
			}
		}

		#endregion

		#region CurrentMapImage
		private ImageSource _CurrentMapImage;
		public ImageSource CurrentMapImage
		{
			get { return this._CurrentMapImage; }
			set
			{
				this._CurrentMapImage = value;

				this._CurrentMapImage = value;
				this.RaisePropertyChanged();
			}
		}
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

		#region SelectedVRSImage
		private ImageSource _SelectedVRSImage;
		public ImageSource SelectedVRSImage
		{
			get { return this._SelectedVRSImage; }
			set
			{
				this._SelectedVRSImage = value;
				this.RaisePropertyChanged();
			}
		}
		#endregion

		#region SelectedDataTable
		DataRowView _SelectedDataTable;
		public DataRowView SelectedDataTable
		{
			get { return this._SelectedDataTable; }
			set
			{
				if (value != null)
				{
					this._SelectedDataTable = value;
					SetData(this.SelectedDataTable, ImageType.TDI);
					SetData(this.SelectedDataTable, ImageType.VRS);
				}
			}
		}
		#endregion

		#region TotalInfoList
		List<InspectionInformation> _TotalInfoList;
		public List<InspectionInformation> TotalInfoList
		{
			get { return this._TotalInfoList; }
			set
			{
				this._TotalInfoList = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region DefectInfoList
		List<InspectionInformation> _DefectInfoList;
		public List<InspectionInformation> DefectInfoList
		{
			get { return this._DefectInfoList; }
			set
			{
				this._DefectInfoList = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region LoadStatusVisible
		Visibility _LoadStatusVisible;
		public Visibility LoadStatusVisible
		{
			get { return this._LoadStatusVisible; }
			set
			{
				this._LoadStatusVisible = value;
				this.RaisePropertyChanged();
			}
		}
		#endregion

		#region StatusText
		string _StatusText;
		public string StatusText
		{
			get { return this._StatusText; }
			set
			{
				this._StatusText = value;
				this.RaisePropertyChanged();
			}
		}
		#endregion

		#region InspModeList
		List<InspectionModeInfo> _InspModeList;
		public List<InspectionModeInfo> InspModeList
		{
			get { return this._InspModeList; }
			set
			{
				this._InspModeList = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region IsDefectSizeSearch
		bool _IsDefectSizeSearch;
		public bool IsDefectSizeSearch
		{
			get { return this._IsDefectSizeSearch; }
			set
			{
				this._IsDefectSizeSearch = value;
				this.RaisePropertyChanged();
			}
		}
		#endregion

		#region IsDefectCodeSearch
		bool _IsDefectCodeSearch;
		public bool IsDefectCodeSearch
		{
			get { return this._IsDefectCodeSearch; }
			set
			{
				this._IsDefectCodeSearch = value;
				this.RaisePropertyChanged();
			}
		}
		#endregion

		#region IsInspModeSearch
		bool _IsInspModeSearch;
		public bool IsInspModeSearch
		{
			get { return this._IsInspModeSearch; }
			set
			{
				this._IsInspModeSearch = value;
				this.RaisePropertyChanged();
			}
		}
		#endregion

		#region DefectSizeFirst
		string _DefectSizeFirst;
		public string DefectSizeFirst
		{
			get { return this._DefectSizeFirst; }
			set
			{
				this._DefectSizeFirst = value;
				this.RaisePropertyChanged();
			}
		}
		#endregion

		#region DefectSizeSecond
		string _DefectSizeSecond;
		public string DefectSizeSecond
		{
			get { return this._DefectSizeSecond; }
			set
			{
				this._DefectSizeSecond = value;
				this.RaisePropertyChanged();
			}
		}
		#endregion

		#region DefectSizeSelectList
		List<string> _DefectSizeSelectList;
		public List<string> DefectSizeSelectList
		{
			get { return this._DefectSizeSelectList; }
			set
			{
				this._DefectSizeSelectList = value;
				this.RaisePropertyChanged();
			}
		}
		#endregion

		#region SelectedDefectSizeFirst
		int _SelectedDefectSizeFirst;
		public int SelectedDefectSizeFirst
		{
			get { return this._SelectedDefectSizeFirst; }
			set
			{
				if (this._SelectedDefectSizeFirst != value)
				{
					this._SelectedDefectSizeFirst = value;
#if DEBUG
					Debug.WriteLine(string.Format("SelectedDefectSizeFirst Set : {0}", value));
#endif
					RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region SelectedDefectSizeSecond
		int _SelectedDefectSizeSecond;
		public int SelectedDefectSizeSecond
		{
			get { return this._SelectedDefectSizeSecond; }
			set
			{
				if (this._SelectedDefectSizeSecond != value)
				{
					this._SelectedDefectSizeSecond = value;
#if DEBUG
					Debug.WriteLine(string.Format("SelectedDefectSizeSecond Set : {0}", value));
#endif
					RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region DefectCode
		string _DefectCode;
		public string DefectCode
		{
			get { return this._DefectCode; }
			set
			{
				this._DefectCode = value;
				this.RaisePropertyChanged();
			}
		}
		#endregion

		#region IsInitialized
		bool _IsInitialized;
		public bool IsInitialized
		{
			get { return this._IsInitialized; }
			set
			{
				this._IsInitialized = value;
				this.RaisePropertyChanged();
			}
		}
		#endregion

		string DBDataPath { get; set; }
		string TiffDataPath
		{
			get { return this.DBDataPath.Replace(".vega_result", ".tif"); }
		}

		public string[] SignArray = new string[] { ">=", "<=", "=" };

		string dbFormatFilePath = @"C:\sqlite\db\vsdb.txt";

		Dictionary<int, Bitmap> TDIImageDictionary { get; set; }
		Dictionary<int, Bitmap> VRSImageDictionary { get; set; }
		Dictionary<int, ImageInfo> ImageInfoDictionary { get; set; }
		SqliteDataDB DataIndexDB { get; set; }
		bool ImageLoaded { get; set; }
		public InspResultViewModel()
		{
			this.LoadStatusVisible = Visibility.Collapsed;
			IsInitialized = false;
		}
		public InspResultViewModel(string dataPath, DateTime InspTime)
		{
			LoadStatusVisible = Visibility.Collapsed;

			this.DBDataPath = dataPath;
			this.ImageLoaded = false;

			this.TopMost = false;
			this.Title = string.Format("Inspection Result [{0} - {1}]", InspTime.ToString("yyyy-MM-dd HH:mm:ss"), Path.GetFileNameWithoutExtension(dataPath));

			_InspModeList = new List<InspectionModeInfo>();

			_InspModeList.Add(new InspectionModeInfo("Mode1", 0, false));
			_InspModeList.Add(new InspectionModeInfo("Mode2", 1, false));
			_InspModeList.Add(new InspectionModeInfo("Mode3", 2, false));
			_InspModeList.Add(new InspectionModeInfo("Mode4", 3, false));
			_InspModeList.Add(new InspectionModeInfo("Mode5", 4, false));

			_DefectSizeSelectList = new List<string>();
			_DefectSizeSelectList.Add("Up");//0
			_DefectSizeSelectList.Add("Down");//1
			_DefectSizeSelectList.Add("Equal");//2
			DefectSizeSelectList = new List<string>(_DefectSizeSelectList);

			this.SelectedDefectSizeFirst = 0;
			this.SelectedDefectSizeSecond = 0;

			this.IsDefectCodeSearch = false;
			this.IsDefectSizeSearch = false;
			this.IsInspModeSearch = false;

			this.DefectSizeFirst = string.Empty;
			this.DefectSizeSecond = string.Empty;
			this.DefectCode = string.Empty;

			DataIndexDB = new SqliteDataDB(dataPath, @"C:\sqlite\db\vsdb.txt");//나중에 수정해야함. 파일 경로 할당을 설정 등으로 제어하는 부분이 필요함

			if (DataIndexDB.Connect())
			{
				var tempTable = DataIndexDB.GetDataTable("Data");
				InspectionInformation totalInfo = new InspectionInformation();
				var tempTotalList = new List<InspectionInformation>();

				MakeDictionary(tempTable);
				tempTable.Dispose();
				//Data,*No(INTEGER),DCode(INTEGER),Size(INTEGER),Length(INTEGER),Width(INTEGER),Height(INTEGER),InspMode(INTEGER),FOV(INTEGER),PosX(INTEGER),PosY(INTEGER),TdiImageExist(INTEGER),VrsImageExist(INTEGER)
				_OriginResultDataTable = DataIndexDB.GetDataTable("Data", "No", "DCode", "Size", "Length", "Width", "Height", "InspMode", "PosX", "PosY");
				ResultDataTable = _OriginResultDataTable.Copy();

				//Datainfo,*LotIndexID(INTEGER),InspStartTime(TEXT),BCRID(TEXT)
				var totalDataTable = DataIndexDB.GetDataTable("Datainfo");

				if (totalDataTable.Rows.Count == 1 && totalDataTable != null)
				{
					var rowData = totalDataTable.Rows[0];

					totalInfo.Name = "Lot Index ID";
					totalInfo.Context = rowData["LotIndexID"].ToString();
					tempTotalList.Add(new InspectionInformation(totalInfo));
					totalInfo.Name = "Inspection Start Time";
					totalInfo.Context = rowData["InspStartTime"].ToString();
					tempTotalList.Add(new InspectionInformation(totalInfo));
					totalInfo.Name = "Reticle ID";
					totalInfo.Context = rowData["BCRID"].ToString();
					tempTotalList.Add(new InspectionInformation(totalInfo));
					TotalInfoList = new List<InspectionInformation>(tempTotalList);

					totalDataTable.Dispose();

					IsInitialized = true;
				}


				if (File.Exists(dataPath.Replace(".vega_result", ".png")))
				{
					var bmp = new BitmapImage(new Uri(dataPath.Replace(".vega_result", ".png"), UriKind.Absolute));
					bmp.CacheOption = BitmapCacheOption.OnLoad;
					this.CurrentMapImage = bmp.Clone();
				}
			}

		}
		//public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
		public void Dispose()
		{
			if (this.TDIImageDictionary != null)
			{
				TDIImageDictionary.Clear();
			}
			if (this.VRSImageDictionary != null)
			{
				VRSImageDictionary.Clear();
			}
			if (this.ImageInfoDictionary != null)
			{
				ImageInfoDictionary.Clear();
			}
			if (this.ResultDataTable != null)
			{
				ResultDataTable.Dispose();
			}
			if (this._OriginResultDataTable != null)
			{
				_OriginResultDataTable.Dispose();
			}
			if (DataIndexDB != null)
			{
				DataIndexDB.Disconnect();
			}
		}
		public async Task LoadTiffImage(CancellationToken ct)
		{
			this.ImageLoaded = false;
			this.TDIImageDictionary = new Dictionary<int, Bitmap>();
			this.VRSImageDictionary = new Dictionary<int, Bitmap>();
			if (File.Exists(TiffDataPath))
			{
				await Task.Factory.StartNew(() => ParseTiff(ct));
			}
		}
		public bool ParseTiff(CancellationToken token)
		{
			using (Image imageFile = Image.FromFile(TiffDataPath))
			{
				LoadStatusVisible = Visibility.Visible;
				try
				{
					FrameDimension frameDimensions = new FrameDimension(
					   imageFile.FrameDimensionsList[0]);

					// Gets the number of pages from the tiff image (if multipage) 
					int frameNum = imageFile.GetFrameCount(frameDimensions);

					for (int frame = 0; frame < frameNum; frame++)
					{
						// Selects one frame at a time and save as jpeg. 
						StatusText = string.Format("{0} / {1}", frame, frameNum);
						if (token.IsCancellationRequested)
						{
							this.Dispose();

							return false;
						}

						imageFile.SelectActiveFrame(frameDimensions, frame);
						using (Bitmap bmp = new Bitmap(imageFile))
						{
							switch (ImageInfoDictionary[frame].Type)
							{
								case ImageType.TDI:
									TDIImageDictionary.Add(ImageInfoDictionary[frame].DefectIndex, new Bitmap(bmp));
									break;
								case ImageType.VRS:
									VRSImageDictionary.Add(ImageInfoDictionary[frame].DefectIndex, new Bitmap(bmp));
									break;
								case ImageType.Mask:
									break;
							}
						}
					}
					this.ImageLoaded = true;

					LoadStatusVisible = Visibility.Collapsed;
					return true;
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
					LoadStatusVisible = Visibility.Visible;
					StatusText = string.Format("FAIL : {0}", ex.Message);
					return false;
				}
			}
		}
		private void MakeDictionary(DataTable table)
		{
			ImageInfoDictionary = new Dictionary<int, ImageInfo>();
			int idx = 0;
			foreach (DataRow item in table.Rows)
			{
				int defectIndex = Convert.ToInt32(item["No"]);
				bool IsTdiExist = Convert.ToBoolean(item["TdiImageExist"]);
				bool IsVRSExist = Convert.ToBoolean(item["VrsImageExist"]);
				if (IsTdiExist)
				{
					ImageInfo info = new ImageInfo();
					info.DefectIndex = defectIndex;
					info.Type = ImageType.TDI;
					ImageInfoDictionary.Add(idx, info);
					idx++;
				}
				if (IsVRSExist)
				{
					ImageInfo info = new ImageInfo();
					info.DefectIndex = defectIndex;
					info.Type = ImageType.VRS;
					ImageInfoDictionary.Add(idx, info);
					idx++;
				}
			}
		}


		private void SetData(DataRowView selectedDataTable, ImageType type)
		{
			int idx = Convert.ToInt32(selectedDataTable["No"]);
			int size = Convert.ToInt32(selectedDataTable["Size"]);
			int dcode = Convert.ToInt32(selectedDataTable["DCode"]);
			int posx = Convert.ToInt32(selectedDataTable["PosX"]);
			int posy = Convert.ToInt32(selectedDataTable["PosY"]);
			var tempList = new List<InspectionInformation>();
			InspectionInformation info = new InspectionInformation();
			//Data,*No(INTEGER),DCode(INTEGER),Size(INTEGER),Length(INTEGER),Width(INTEGER),Height(INTEGER),InspMode(INTEGER),FOV(INTEGER),PosX(INTEGER),PosY(INTEGER),TdiImageExist(INTEGER),VrsImageExist(INTEGER)

			info.Name = "Index";
			info.Context = idx.ToString();
			tempList.Add(new InspectionInformation(info));

			info.Name = "Size(pixel)";
			info.Context = size.ToString();
			tempList.Add(new InspectionInformation(info));

			info.Name = "Defect Code";
			info.Context = dcode.ToString();
			tempList.Add(new InspectionInformation(info));

			info.Name = "Position X";
			info.Context = posx.ToString();
			tempList.Add(new InspectionInformation(info));

			info.Name = "Position Y";
			info.Context = posy.ToString();
			tempList.Add(new InspectionInformation(info));

			this.DefectInfoList = new List<InspectionInformation>(tempList);

			if (ImageLoaded)
			{
				switch (type)
				{
					case ImageType.TDI:
						if (this.TDIImageDictionary.ContainsKey(idx))
						{
							this.SelectedTDIImage = ConvertImage(this.TDIImageDictionary[idx]);
						}
						else
						{
							this.SelectedTDIImage = null;
						}
						break;
					case ImageType.VRS:
						if (this.VRSImageDictionary.ContainsKey(idx))
						{
							this.SelectedVRSImage = ConvertImage(this.VRSImageDictionary[idx]);
						}
						else
						{
							this.SelectedVRSImage = null;
						}
						break;
					case ImageType.Mask:
						break;
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
		public ICommand CommandOnStartSearchButton
		{
			get
			{
				return new RelayCommand(OnStartSearchButton);
			}
		}
		public void OnStartSearchButton()
		{
			//Defect 탐색
			//Inspection Mode
			//Defect Size
			//Defect Code
			if (!File.Exists(this.DBDataPath) || !File.Exists(dbFormatFilePath))
				return;

			if (!IsInitialized)
				return;

			//Data,*No(INTEGER),DCode(INTEGER),Size(INTEGER),Length(INTEGER),Width(INTEGER),Height(INTEGER),InspMode(INTEGER),FOV(INTEGER),PosX(INTEGER),PosY(INTEGER),TdiImageExist(INTEGER),VrsImageExist(INTEGER)
			string dCodeQuery = string.Format("DCode = {0}", DefectCode);

			string firstSign = SignArray[SelectedDefectSizeFirst];
			string secondSign = SignArray[SelectedDefectSizeSecond];

			bool firstSearchEnable = false;
			bool secondSearchEnable = false;

			decimal FirstSizeUm;
			decimal SecondSizeUm;

			if (!decimal.TryParse(DefectSizeFirst, out FirstSizeUm))
			{
				FirstSizeUm = 1.0m;
				firstSearchEnable = false;
			}
			else
			{
				firstSearchEnable = true;
			}
			if (!decimal.TryParse(DefectSizeSecond, out SecondSizeUm))
			{
				SecondSizeUm = 1.0m;
				secondSearchEnable = false;
			}
			else
			{
				secondSearchEnable = true;
			}

			string defectSizeQuery_1 = string.Format("Size {0} {1}", firstSign, FirstSizeUm);
			string defectSizeQuery_2 = string.Format("Size {0} {1}", secondSign, SecondSizeUm);



			StringBuilder stbr = new StringBuilder();
			bool IsFirstQuery = true;
			if (IsDefectCodeSearch)
			{
				stbr.Append(dCodeQuery);
				IsFirstQuery = false;
			}
			if (IsInspModeSearch)
			{
				if (!IsFirstQuery)
				{
					stbr.Append(" AND ");
				}
				else
				{
					IsFirstQuery = false;
				}
				if (InspModeList.Count > 0)
				{
					stbr.Append("(");
					bool OrFirstFlag = true;
					for (int i = 0; i < InspModeList.Count; i++)
					{
						var item = InspModeList[i];
						if (item.IsTarget)
						{
							if (i < InspModeList.Count && !OrFirstFlag)
							{
								stbr.Append(" OR ");
							}
							else
							{
								OrFirstFlag = false;
							}
							stbr.Append(string.Format("InspMode = {0}", item.Mode));
						}
					}
					stbr.Append(")");
				}
			}
			if (IsDefectSizeSearch)
			{
				if (firstSearchEnable)
				{
					if (!IsFirstQuery)
					{
						stbr.Append(" AND ");
					}
					else
					{
						IsFirstQuery = false;
					}
					stbr.Append(defectSizeQuery_1);
				}
				if (secondSearchEnable)
				{
					if (!IsFirstQuery)
					{
						stbr.Append(" AND ");
					}
					else
					{
						IsFirstQuery = false;
					}
					stbr.Append(defectSizeQuery_2);
				}
			}

			if (!IsFirstQuery)//First Query 상태가 해소되지 않으면 명령을 수행하지 않는다
			{
				var temp = _OriginResultDataTable.Select(stbr.ToString());
				if (temp.Count() > 0)
				{
					ResultDataTable = temp.CopyToDataTable();
				}
				else
				{
					ResultDataTable = new DataTable();
				}
			}
			else
			{
				ResultDataTable = _OriginResultDataTable.Copy();//검사 조건이 없으면 원복
			}
		}
	}
}
