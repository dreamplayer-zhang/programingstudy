using RootTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Diagnostics;
//using RootTools_CLR;//CLR INSP 테스트
using System.Data;//DB 연동 테스트
using ATI;
using Microsoft.Win32;
using System.Threading;
using Root_Vega.Dialog;
using Root_Vega.Controls;

namespace Root_Vega

{
	class _5_ResultViewModel : ObservableObject
	{
		#region IsReticleIDSearch
		bool _IsReticleIDSearch;
		public bool IsReticleIDSearch
		{
			get { return this._IsReticleIDSearch; }
			set
			{
				if (this._IsReticleIDSearch != value)
				{
					this._IsReticleIDSearch = value;
					RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region ReticleID
		string _ReticleID;
		public string ReticleID
		{
			get { return this._ReticleID; }
			set
			{
				if (this._ReticleID != value)
				{
					this._ReticleID = value;
					RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region IsRecipeNameSearch
		bool _IsRecipeNameSearch;
		public bool IsRecipeNameSearch
		{
			get { return this._IsRecipeNameSearch; }
			set
			{
				if (this._IsRecipeNameSearch != value)
				{
					this._IsRecipeNameSearch = value;
					RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region RecipeName
		string _RecipeName;
		public string RecipeName
		{
			get { return this._RecipeName; }
			set
			{
				if (this._RecipeName != value)
				{
					this._RecipeName = value;
					RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region IsDefectCountSearch
		bool _IsDefectCountSearch;
		public bool IsDefectCountSearch
		{
			get { return this._IsDefectCountSearch; }
			set
			{
				if (this._IsDefectCountSearch != value)
				{
					this._IsDefectCountSearch = value;
					RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region IsDateSearch
		bool _IsDateSearch;
		public bool IsDateSearch
		{
			get { return this._IsDateSearch; }
			set
			{
				if (this._IsDateSearch != value)
				{
					this._IsDateSearch = value;
					RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region StartDate
		DateTime _StartDate;
		public DateTime StartDate
		{
			get { return this._StartDate; }
			set
			{
				if (this._StartDate != value)
				{
					this._StartDate = value;
					if (StartDate >= EndDate)
					{
						EndDate = StartDate.AddHours(-1);
					}
					RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region EndDate
		DateTime _EndDate;
		public DateTime EndDate
		{
			get { return this._EndDate; }
			set
			{
				if (this._EndDate != value)
				{
					this._EndDate = value;
					if (StartDate >= EndDate)
					{
						StartDate = EndDate.AddHours(1);
					}
					RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region DefectCountSelectList
		List<string> _DefectCountSelectList;
		public List<string> DefectCountSelectList
		{
			get { return this._DefectCountSelectList; }
			set
			{
				this._DefectCountSelectList = value;
			}
		}
		#endregion

		#region SelectedDefectCountFirst
		int _SelectedDefectCountFirst;
		public int SelectedDefectCountFirst
		{
			get { return this._SelectedDefectCountFirst; }
			set
			{
				if (this._SelectedDefectCountFirst != value)
				{
					this._SelectedDefectCountFirst = value;
#if DEBUG
					Debug.WriteLine(string.Format("SelectedDefectCountFirst Set : {0}", value));
#endif
					RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region SelectedDefectCountSecond
		int _SelectedDefectCountSecond;
		public int SelectedDefectCountSecond
		{
			get { return this._SelectedDefectCountSecond; }
			set
			{
				if (this._SelectedDefectCountSecond != value)
				{
					this._SelectedDefectCountSecond = value;
#if DEBUG
					Debug.WriteLine(string.Format("SelectedDefectCountSecond Set : {0}", value));
#endif
					RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region ResultDataTable
		DataTable _ResultDataTable;
		DataTable _OriginResultDataTable;
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

		#region DefectCountFirst
		string _DefectCountFirst;
		public string DefectCountFirst
		{
			get { return this._DefectCountFirst; }
			set
			{
				if (this._DefectCountFirst != value)
				{
					this._DefectCountFirst = value;
					RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region DefectCountSecond
		string _DefectCountSecond;
		public string DefectCountSecond
		{
			get { return this._DefectCountSecond; }
			set
			{
				if (this._DefectCountSecond != value)
				{
					this._DefectCountSecond = value;
					RaisePropertyChanged();
				}
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
#if DEBUG
					foreach (var item in value.Row.ItemArray)
					{
						Debug.WriteLine(item.ToString());
					}
#endif
				}
			}
		}
		#endregion

		string[] SignArray = new string[] { ">=", "<=", "=" };
		CancellationTokenSource cts;

		Vega_Engineer m_Engineer;
		private readonly IDialogService m_DialogService;
		SqliteDataDB DataIndexDB { get; set; }

		string indexFilePath = @"C:\vsdb\init\SearchIndex.sqlite";
		string dbFormatFilePath = @"C:\vsdb\init\vsdb.txt";

		public InspResultViewModel CurrentResultViewModel { get; set; }

		public _5_ResultViewModel(Vega_Engineer engineer, IDialogService dialogService)
		{
			m_Engineer = engineer;
			m_DialogService = dialogService;
			this.EndDate = DateTime.Now;
			this.StartDate = DateTime.Now.AddDays(-1);

			_DefectCountSelectList = new List<string>();
			_DefectCountSelectList.Add("Up");//0
			_DefectCountSelectList.Add("Down");//1
			_DefectCountSelectList.Add("Equal");//2
			DefectCountSelectList = new List<string>(_DefectCountSelectList);

			DefectCountFirst = string.Empty;
			DefectCountSecond = string.Empty;
			ReticleID = string.Empty;
			RecipeName = string.Empty;

			this.SelectedDefectCountFirst = 0;
			this.SelectedDefectCountSecond = 0;
			if (File.Exists(indexFilePath) && File.Exists(dbFormatFilePath))
			{
				DataIndexDB = new SqliteDataDB(indexFilePath, dbFormatFilePath);//나중에 수정해야함. 파일 경로 할당을 설정 등으로 제어하는 부분이 필요함
				if (DataIndexDB.Connect())
				{
					//SearchTable,*Idx(INTEGER),InspStartTime(TEXT),ReticleID(TEXT),RecipeName(TEXT),TotalDefectCount(INTEGER),DataFilePath(TEXT)
					_OriginResultDataTable = DataIndexDB.GetDataTable("SearchTable", "Idx", "InspStartTime", "ReticleID", "RecipeName", "TotalDefectCount");
					DataIndexDB.Disconnect();
				}
				IsDateSearch = true;
			}
			CurrentResultViewModel = new InspResultViewModel();
		}
		public void StartSearch()
		{
			if (!File.Exists(indexFilePath) || !File.Exists(dbFormatFilePath))
				return;

			string dateQuery = string.Format("InspStartTime >= CONVERT('{0}', 'System.DateTime') AND InspStartTime <= CONVERT('{1}', 'System.DateTime')",
			this.StartDate.ToString("yyyy-MM-dd HH:mm:ss"), this.EndDate.ToString("yyyy-MM-dd HH:mm:ss"));
			string reticleIdQuery = string.Format("ReticleID = '{0}'", ReticleID);
			string recipeNameQuery = string.Format("RecipeName = '{0}'", RecipeName);

			string firstSign = SignArray[SelectedDefectCountFirst];
			string secondSign = SignArray[SelectedDefectCountSecond];

			string defectCountQuery_1 = string.Format("TotalDefectCount {0} {1}", firstSign, DefectCountFirst);
			string defectCountQuery_2 = string.Format("TotalDefectCount {0} {1}", secondSign, DefectCountSecond);

			StringBuilder stbr = new StringBuilder();
			bool IsFirstQuery = true;
			if (IsDateSearch)
			{
				stbr.Append(dateQuery);
				IsFirstQuery = false;
			}
			if (IsReticleIDSearch && ReticleID != "")
			{
				if (!IsFirstQuery)
				{
					stbr.Append(" AND ");
				}
				else
				{
					IsFirstQuery = false;
				}
				stbr.Append(reticleIdQuery);
			}
			if (IsRecipeNameSearch && RecipeName != "")
			{
				if (!IsFirstQuery)
				{
					stbr.Append(" AND ");
				}
				else
				{
					IsFirstQuery = false;
				}
				stbr.Append(recipeNameQuery);
			}
			if (IsDefectCountSearch)
			{
				if (DefectCountFirst != "")
				{
					if (!IsFirstQuery)
					{
						stbr.Append(" AND ");
					}
					else
					{
						IsFirstQuery = false;
					}
					stbr.Append(defectCountQuery_1);
				}
				if (DefectCountSecond != "")
				{

					if (!IsFirstQuery)
					{
						stbr.Append(" AND ");
					}
					else
					{
						IsFirstQuery = false;
					}
					stbr.Append(defectCountQuery_2);
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
		}
		private void StartTest()
		{

		}
		public void RefreshList()
		{
			if (File.Exists(indexFilePath) && File.Exists(dbFormatFilePath))
			{
				DataIndexDB = new SqliteDataDB(indexFilePath, dbFormatFilePath);//나중에 수정해야함. 파일 경로 할당을 설정 등으로 제어하는 부분이 필요함
				if (DataIndexDB.Connect())
				{
					//SearchTable,*Idx(INTEGER),InspStartTime(TEXT),ReticleID(TEXT),RecipeName(TEXT),TotalDefectCount(INTEGER),DataFilePath(TEXT)
					_OriginResultDataTable = DataIndexDB.GetDataTable("SearchTable", "Idx", "InspStartTime", "ReticleID", "RecipeName", "TotalDefectCount");
					DataIndexDB.Disconnect();
					if (ResultDataTable != null)
						ResultDataTable.Clear();
				}
				IsDateSearch = true;
			}
		}
		public void ChangeDBFilePath()
		{
			bool dbChange = false;
			bool formatChange = false;

			OpenFileDialog dbDlg = new OpenFileDialog();
			dbDlg.Filter = "SQlite DB file (*.sqlite)|*.sqlite|All file (*.*)|*.*";
			dbDlg.FilterIndex = 0;
			dbDlg.FileName = indexFilePath;
			dbDlg.InitialDirectory = System.IO.Path.GetDirectoryName(indexFilePath);
			dbDlg.Title = "Select Index SQLite DB File";
			var dbResult = dbDlg.ShowDialog();
			if (dbResult == true)
			{
				if(indexFilePath!= dbDlg.FileName)
				{
					indexFilePath = dbDlg.FileName;
					dbChange = true;
				}
			}

			OpenFileDialog formatDlg = new OpenFileDialog();
			formatDlg.Filter = "ATI DB Format File (*.txt)|*.txt|All file (*.*)|*.*";
			formatDlg.FilterIndex = 0;
			formatDlg.FileName = dbFormatFilePath;
			formatDlg.InitialDirectory = System.IO.Path.GetDirectoryName(dbFormatFilePath);
			formatDlg.Title = "Select SQLite DB Default Format File";
			var keyResult = formatDlg.ShowDialog();
			if (keyResult == true)
			{
				if(dbFormatFilePath != formatDlg.FileName)
				{
					dbFormatFilePath = formatDlg.FileName;
					formatChange = true;
				}
			}

			if(dbChange || formatChange)
			{
				RefreshList();
			}
		}
		internal void OpenSelectedInspectionData()
		{
			if (SelectedDataTable != null)
			{
				DataIndexDB = new SqliteDataDB(indexFilePath, dbFormatFilePath);//나중에 수정해야함. 파일 경로 할당을 설정 등으로 제어하는 부분이 필요함
				if (DataIndexDB.Connect())
				{
					cts = new CancellationTokenSource();

					var idx = SelectedDataTable["Idx"];
					//SearchTable,*Idx(INTEGER),InspStartTime(TEXT),ReticleID(TEXT),RecipeName(TEXT),TotalDefectCount(INTEGER),DataFilePath(TEXT)
					var tempTable = DataIndexDB.StartQuery("SearchTable", string.Format("SELECT Idx,InspStartTime,DataFilePath FROM SearchTable WHERE Idx={0}", idx)).Rows[0];
					DataIndexDB.Disconnect();

					//var ctrlViewModel = new InspResultViewModel(tempTable["DataFilePath"].ToString(), DateTime.Parse(tempTable["InspStartTime"].ToString()));
					var windowViewModel = new Dialog_InspResultViewModel(tempTable["DataFilePath"].ToString(), DateTime.Parse(tempTable["InspStartTime"].ToString()));
					var window = new Dialog_InspResultView();
					windowViewModel.CloseWindow += () =>
					{
						cts.Cancel();
						windowViewModel.Dispose();
						window.Close();
					};

					window.DataContext = windowViewModel;
					window.Show();
                    windowViewModel.LoadTiffImage(cts.Token);
				}
			}
			else
			{
				Debug.WriteLine("OpenSelectedInspectionData() - NULL");
			}
		}

		public ICommand CommandStartSearch
		{
			get
			{
				return new RelayCommand(StartSearch);
			}
		}
		public ICommand CommandChangeDBFilePath
		{
			get
			{
				return new RelayCommand(ChangeDBFilePath);
			}
		}
		public ICommand CommandRefreshList
		{
			get
			{
				return new RelayCommand(RefreshList);
			}
		}
	}
}
