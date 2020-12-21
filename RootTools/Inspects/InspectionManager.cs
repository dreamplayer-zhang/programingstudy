using System.ComponentModel;
using System.Threading;
using System.Collections;
using System.Data;
using ATI;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System;
using System.Security.RightsManagement;
using System.Threading.Tasks;
using System.Linq;
using DPoint = System.Drawing.Point;
using System.Diagnostics;
using MySqlX.XDevAPI.Relational;
using RootTools.ToolBoxs;
using System.Drawing.Imaging;
using System.Windows;

namespace RootTools.Inspects
{
	public enum eEdgeFindDirection { LEFT, TOP, RIGHT, BOTTOM };
	public enum eBrightSide { LEFT, TOP, RIGHT, BOTTOM };
	public class InspectionManager : Singleton<InspectionManager>, INotifyPropertyChanged
	{
		#region EventHandler
		/// <summary>
		/// Defect 정보 변경 시 사용할 Event Handler
		/// </summary>
		/// <param name="item">Defect Information</param>
		/// <param name="args">arguments. 필요한 경우 수정해서 사용</param>
		public delegate void ChangeDefectInfoEventHanlder(DefectDataWrapper item);
		public delegate void EventHandler();
		/// <summary>
		/// UI에 Defect을 추가하기 위해 발생하는 Event
		/// </summary>
		public event ChangeDefectInfoEventHanlder AddChromeDefect;
		/// <summary>
		/// UI에 Defect을 추가하기 위해 발생하는 Event
		/// </summary>
		public event ChangeDefectInfoEventHanlder AddEBRDefect;
		/// <summary>
		/// UI에 Defect을 추가하기 위해 발생하는 Event
		/// </summary>
		public event ChangeDefectInfoEventHanlder AddTopSideDefect;
		/// <summary>
		/// UI에 Defect을 추가하기 위해 발생하는 Event
		/// </summary>
		public event ChangeDefectInfoEventHanlder AddLeftSideDefect;
		/// <summary>
		/// UI에 Defect을 추가하기 위해 발생하는 Event
		/// </summary>
		public event ChangeDefectInfoEventHanlder AddRightSideDefect;
		/// <summary>
		/// UI에 Defect을 추가하기 위해 발생하는 Event
		/// </summary>
		public event ChangeDefectInfoEventHanlder AddBotSideDefect;
		/// <summary>
		/// UI에 Defect을 추가하기 위해 발생하는 Event
		/// </summary>
		public event ChangeDefectInfoEventHanlder AddTopBevelDefect;
		/// <summary>
		/// UI에 Defect을 추가하기 위해 발생하는 Event
		/// </summary>
		public event ChangeDefectInfoEventHanlder AddLeftBevelDefect;
		/// <summary>
		/// UI에 Defect을 추가하기 위해 발생하는 Event
		/// </summary>
		public event ChangeDefectInfoEventHanlder AddRightBevelDefect;
		/// <summary>
		/// UI에 Defect을 추가하기 위해 발생하는 Event
		/// </summary>
		public event ChangeDefectInfoEventHanlder AddBotBevelDefect;
		/// <summary>
		/// UI에 Defect을 추가하기 위해 발생하는 Event
		/// </summary>
		public event ChangeDefectInfoEventHanlder AddD2DDefect;
		/// <summary>
		/// UI Defect을 지우기 위해 발생하는 Event
		/// </summary>
		public event EventHandler ClearDefect;
		/// <summary>
		/// UI에 추가된 Defect 정보를 새로고침 하기위한 Event
		/// </summary>
		public static event EventHandler RefreshDefect;
		#endregion

		Thread inspThread;
		Inspection[] InspectionThread;
		StopWatch sw;

		int nThreadNum = 10;
		int ImageWidth = 320;
		int ImageHeight = 240;
		public ToolBox m_toolBox;



		private string inspDefaultDir;
		private string inspFileName;
		SqliteDataDB VSDBManager;
		SqliteDataDB IndexDBManager;
		System.Data.DataTable VSDataInfoDT;
		System.Data.DataTable VSDataDT;
		System.Data.DataTable SearchDataDT;
		Dictionary<int, CPoint> refPosDictionary;
		public DateTime NowTime;
		bool m_bProgress;
		int m_nPatternInspDoneNum = 0;
		public int p_nPatternInspDoneNum
        {
            get { return m_nPatternInspDoneNum; }
            set
            {
				if (m_nPatternInspDoneNum == value) return;
				m_nPatternInspDoneNum = value;
				OnPropertyChanged("p_nPatternInspDoneNum");
            }
        }
		int m_nSideInspDoneNum = 0;
		public int p_nSideInspDoneNum
        {
            get { return m_nSideInspDoneNum; }
            set
            {
				if (m_nSideInspDoneNum == value) return;
				m_nSideInspDoneNum = value;
				OnPropertyChanged("p_nSideInspDoneNum");
            }
        }

		public int m_nTotalDefectCount = 0;
		public float[] m_farrAfineMatrix = null;

		public bool IsInitialized { get; private set; }
		public void StartInspection()
		{
			//이건 무조건 한번만 하도록 구성해야 함
			if (m_bProgress)
			{
				//강제로 리턴
				return;
			}
			NowTime = DateTime.Now;
			m_bProgress = false;
			sw = new StopWatch();
			sw.Start();

			IsInitialized = true;
			p_nPatternInspDoneNum = 0;
			p_nSideInspDoneNum = 0;
			m_nTotalDefectCount = 0;
			
			inspThread = new Thread(DoInspection);

            //if (inspThread != null)
            //{
            //	if (inspThread.IsAlive)
            //	{
            //		inspThread.Abort();
            //	}
            //}
            inspThread.Start();

        }
        public void DoInspection()
		{
			if (!IsInitialized)
				return;

			int nInspDoneNum = 0;
			InspectionThread = new Inspection[nThreadNum];
			Parallel.For(0, nThreadNum, i =>
			{
				InspectionThread[i] = new Inspection(nThreadNum);
				InspectionThread[i].AddChromeDefect += InspectionManager_AddChromeDefect;

				InspectionThread[i].AddTopSideDefect += InspectionManager_AddTopSideDefect;//InspectionManager_AddSideDefect;
				InspectionThread[i].AddLeftSideDefect += InspectionManager_AddLeftSideDefect;
				InspectionThread[i].AddRightSideDefect += InspectionManager_AddRightSideDefect;
				InspectionThread[i].AddBotSideDefect += InspectionManager_AddBotSideDefect;

				InspectionThread[i].AddTopBevelDefect += InspectionManager_AddTopBevelDefect;
				InspectionThread[i].AddLeftBevelDefect += InspectionManager_AddLeftBevelDefect;
				InspectionThread[i].AddRightBevelDefect += InspectionManager_AddRightBevelDefect;
				InspectionThread[i].AddBotBevelDefect += InspectionManager_AddBotBevelDefect;

				InspectionThread[i].AddEBRDefect += InspectionManager_AddEBRDefect;
			});
			//for (int i = 0; i < nThreadNum; i++)
			//{
			//	InsepctionThread[i] = new Inspection(nThreadNum);
			//	InsepctionThread[i].AddDefect += InspectionManager_AddDefect;
			//}

			m_bProgress = true;

			while (m_bProgress)
			{
				lock (lockObj)
				{
					for (int i = 0; i < nThreadNum; i++)
					{
						if (InspectionThread[i].bState == Inspection.InspectionState.Done)
						{
							InspectionThread[i].bState = Inspection.InspectionState.Ready;
							nInspDoneNum++;
						}
					}

					//TODO : 한번 시작하면 무한으로 돌도록 만듭시다.
					//단 결과는 나와야 하므로 일정 구간에서 완료 이벤트는 발생이 되어야 합니다
					//while (p_qInspection.Count == 0 && nInspDoneNum == nInspectionCount)//구문이 이해가 안감...
					//{
					//    InspectionDone();
					//    Monitor.Wait(lockObj);
					//    m_bProgress = false;
					//    break;
					//}

					for (int i = 0; i < nThreadNum; i++)
					{
						if (0 < GetWaitQueue())
						{
							if (InspectionThread[i].bState == Inspection.InspectionState.Ready ||
							InspectionThread[i].bState == Inspection.InspectionState.None)
							{
								InspectionProperty ipQueue = p_qInspection.Dequeue();
								if ( p_qInspection.Count == 0)
                                {
									Console.WriteLine("Queue Item Count : " + p_qInspection.Count);
									sw.Stop();
									Console.WriteLine(string.Format("Insepction End : {0}", sw.ElapsedMilliseconds / 1000.0));
									//MessageBox.Show("Inspection Complete");
								}
								InspectionThread[i].StartInspection(ipQueue, i);

								// InspectionTarget별 검사 진행 Counting
								InspectionTarget inspTarget = GetInspectionTarget(ipQueue.m_nDefectCode);
								if (inspTarget == InspectionTarget.Chrome || inspTarget == InspectionTarget.EBR) p_nPatternInspDoneNum++;
								else if (inspTarget == InspectionTarget.SideInspectionBottom || inspTarget == InspectionTarget.SideInspectionLeft
									|| inspTarget == InspectionTarget.SideInspectionTop || inspTarget == InspectionTarget.SideInspectionRight) p_nSideInspDoneNum++;
								else
								{

								}
							}
						}
					}
				}
			}
			//여기서 완료 이벤트 발생
		}

		private void InspectionManager_AddBotBevelDefect(DefectDataWrapper item)
		{
			if (AddBotBevelDefect != null)
			{
				AddBotBevelDefect(item);
				m_nTotalDefectCount++;
			}
		}

		private void InspectionManager_AddRightBevelDefect(DefectDataWrapper item)
		{
			if (AddRightBevelDefect != null)
			{
				AddRightBevelDefect(item);
				m_nTotalDefectCount++;
			}
		}

		private void InspectionManager_AddLeftBevelDefect(DefectDataWrapper item)
		{
			if (AddLeftBevelDefect != null)
			{
				AddLeftBevelDefect(item);
				m_nTotalDefectCount++;
			}
		}

		private void InspectionManager_AddTopBevelDefect(DefectDataWrapper item)
		{
			if (AddTopBevelDefect != null)
			{
				AddTopBevelDefect(item);
				m_nTotalDefectCount++;
			}
		}

		private void InspectionManager_AddBotSideDefect(DefectDataWrapper item)
		{
			if (AddBotSideDefect != null)
			{
				AddBotSideDefect(item);
				m_nTotalDefectCount++;
			}
		}

		private void InspectionManager_AddRightSideDefect(DefectDataWrapper item)
		{
			if (AddRightSideDefect != null)
			{
				AddRightSideDefect(item);
				m_nTotalDefectCount++;
			}
		}

		private void InspectionManager_AddTopSideDefect(DefectDataWrapper item)
		{
			if (AddTopSideDefect != null)
			{
				AddTopSideDefect(item);
				m_nTotalDefectCount++;
			}
		}
		private void InspectionManager_AddLeftSideDefect(DefectDataWrapper item)
		{
			if (AddLeftSideDefect != null)
			{
				AddLeftSideDefect(item);
				m_nTotalDefectCount++;
			}
		}

		private void InspectionManager_AddEBRDefect(DefectDataWrapper item)
		{
			if (AddEBRDefect != null)
			{
				AddEBRDefect(item);
				m_nTotalDefectCount++;
			}
		}
		/// <summary>
		/// Add Defect 이벤트가 발생할 때 실행될 메소드
		/// </summary>
		/// <param name="source">DefectData array</param>
		/// <param name="args">추후 arguments가 필요하면 사용할것</param>
		private void InspectionManager_AddChromeDefect(DefectDataWrapper item)
		{
			if (AddChromeDefect != null)
			{
				AddChromeDefect(item);
				m_nTotalDefectCount++;
			}
		}

		public void InspectionDone(string inspIndexFilePath)
		{
			bool testFlag = false;
			//여기서 DB관련동작 이하생략!
			DBConnector connector = new DBConnector("localhost", "Inspections", "root", "`ati5344");
			if (connector.Open())
			{
				var result = connector.SendNonQuery("SELECT COUNT(*) FROM inspections.inspstatus;");
				if (result == (int)MYSQLError.TABLE_IS_MISSING)
				{
					//상태용 테이블 작성
					result = connector.SendNonQuery("CREATE TABLE inspections.inspstatus (idx INT NOT NULL, inspStatusNum INT NULL, PRIMARY KEY (idx));");
					if (result != 0)
					{
						//에러에 대한 추가 예외처리 필요
					}
				}
				//완료 표시

				//완료. 검사결과 출력
				DataSet tempSet = connector.GetDataSet("tempdata");

				inspDefaultDir = @"C:\vsdb";
				if (!System.IO.Directory.Exists(inspDefaultDir))
				{
					System.IO.Directory.CreateDirectory(inspDefaultDir);
				}
				//var nowTime = DateTime.Now;
				inspFileName = NowTime.ToString("yyyyMMdd_HHmmss") + "_inspResult.vega_result";
				var targetVsPath = System.IO.Path.Combine(inspDefaultDir, inspFileName);
				string VSDB_configpath = @"C:/vsdb/init/vsdb.txt";

				VSDBManager = new SqliteDataDB(targetVsPath, VSDB_configpath);
				IndexDBManager = new SqliteDataDB(inspIndexFilePath, VSDB_configpath);

				if (VSDBManager.Connect())
				{
					VSDBManager.CreateTable("Datainfo");
					VSDBManager.CreateTable("Data");

					VSDataInfoDT = VSDBManager.GetDataTable("Datainfo");
					VSDataDT = VSDBManager.GetDataTable("Data");
				}
				if (IndexDBManager.Connect())
				{
					IndexDBManager.CreateTable("SearchTable");
					SearchDataDT = IndexDBManager.GetDataTable("SearchTable");
				}

				//int stride = ImageWidth / 8;
				//string target_path = System.IO.Path.Combine(inspDefaultDir, System.IO.Path.GetFileNameWithoutExtension(inspFileName) + ".tif");

				//System.Windows.Media.Imaging.BitmapPalette myPalette = System.Windows.Media.Imaging.BitmapPalettes.WebPalette;

				//System.IO.FileStream stream = new System.IO.FileStream(target_path, System.IO.FileMode.Create);
				//System.Windows.Media.Imaging.TiffBitmapEncoder encoder = new System.Windows.Media.Imaging.TiffBitmapEncoder();
				//encoder.Compression = System.Windows.Media.Imaging.TiffCompressOption.Zip;

				//Data,@No(INTEGER),DCode(INTEGER),Size(INTEGER),Length(INTEGER),Width(INTEGER),Height(INTEGER),InspMode(INTEGER),FOV(INTEGER),PosX(INTEGER),PosY(INTEGER)

				//using (FileStream fs = new FileStream(System.IO.Path.Combine(inspDefaultDir, System.IO.Path.GetFileNameWithoutExtension(inspFileName) + ".vega_image"), FileMode.Create))
				//{
					//fs.Write(BitConverter.GetBytes(tempSet.Tables["tempdata"].Rows.Count), 0, sizeof(int));//defect 개수 저장
					foreach (System.Data.DataRow item in tempSet.Tables["tempdata"].Rows)
					{
						System.Data.DataRow dataRow = VSDataDT.NewRow();

						dataRow["No"] = item["idx"];
						dataRow["DCode"] = item["ClassifyCode"];
						dataRow["AreaSize"] = item["AreaSize"];
						dataRow["Length"] = item["Length"];
						dataRow["Width"] = item["Width"];
						dataRow["Height"] = item["Height"];
						dataRow["FOV"] = item["FOV"];
						int posX = Convert.ToInt32(item["PosX"]);
						int posY = Convert.ToInt32(item["PosY"]);
						//	회전보정
						System.Drawing.PointF ptfOriginPos = new System.Drawing.PointF(posX, posY);
						System.Drawing.PointF ptfTransformedPos = new System.Drawing.PointF(posX, posY);
						if (m_farrAfineMatrix != null)
                        {
							ptfTransformedPos = AffineTransform(ptfOriginPos, m_farrAfineMatrix);
						}
						//
						if (refPosDictionary != null)
						{
							if (refPosDictionary.ContainsKey(Convert.ToInt32(item["ClassifyCode"])))
							{
								//보정 기본 좌표가 있는 경우 보정 기준 좌표를 적용한다
								ptfTransformedPos.X -= refPosDictionary[Convert.ToInt32(item["ClassifyCode"])].X;
								ptfTransformedPos.Y -= refPosDictionary[Convert.ToInt32(item["ClassifyCode"])].Y;
							}
						}
						dataRow["PosX"] = posX;
						dataRow["PosY"] = posY;

						dataRow["TransformPosX"] = ptfTransformedPos.X;
						dataRow["TransformPosY"] = ptfTransformedPos.Y;

						dataRow["TdiImageExist"] = 1;
						dataRow["VrsImageExist"] = 0;

						VSDataDT.Rows.Add(dataRow);


						//TODO 
						//  int,int,bytes 무한반복


						double fPosX = Convert.ToDouble(item["PosX"]);
						double fPosY = Convert.ToDouble(item["PosY"]);
						CRect ImageSizeBlock = new CRect(
								 (int)fPosX - ImageWidth / 2,
								 (int)fPosY - ImageHeight / 2,
								 (int)fPosX + ImageWidth / 2,
								 (int)fPosY + ImageHeight / 2);
						string pool = item["memPOOL"].ToString();
						string group = item["memGROUP"].ToString();
						string memory = item["memMEMORY"].ToString();
						var tempMem = m_toolBox.m_memoryTool.GetMemory(pool, group, memory);
						var img = new ImageData(tempMem);
					//var imageBytes = img.GetRectByteArray(ImageSizeBlock);
					img.SaveRectImage(ImageSizeBlock, System.IO.Path.Combine(inspDefaultDir, System.IO.Path.GetFileNameWithoutExtension(inspFileName) +"_"+Convert.ToInt32(item["idx"]).ToString("D8") + ".bmp"));

					//fs.Write(BitConverter.GetBytes(Convert.ToInt32(item["idx"].ToString())), 0, sizeof(int));//4
					//fs.Write(BitConverter.GetBytes(ImageWidth), 0, sizeof(int));//4
					//fs.Write(BitConverter.GetBytes(ImageHeight), 0, sizeof(int));//4
					//fs.Write(BitConverter.GetBytes(imageBytes.Length), 0, sizeof(int));//4
					//fs.Write(imageBytes, 0, imageBytes.Length);//바로직전거만큼
				}
				//}
				System.Data.DataRow searchDataRow = SearchDataDT.NewRow();
				searchDataRow["Idx"] = SearchDataDT.Rows.Count;
				searchDataRow["InspStartTime"] = NowTime.ToString("yyyy-MM-dd HH:mm:ss");//TODO 나중에 진짜 검사 시작시간(로딩 시작 시간)으로 바꿔야 함
				searchDataRow["ReticleID"] = "RETICLEID";//TODO 나중에 진짜 retilce Id를 받아와서 넣어줘야 함
				searchDataRow["RecipeName"] = "Rcp001";//TODO 나중에 진짜 recipe name을 받아와서 넣어줘야 함
				searchDataRow["TotalDefectCount"] = tempSet.Tables["tempdata"].Rows.Count;
				searchDataRow["DataFilePath"] = targetVsPath;
				SearchDataDT.Rows.Add(searchDataRow);


				//if (VSDataDT.Rows.Count > 0)
				//{
				//	encoder.Save(stream);
				//}
				//stream.Dispose();

				VSDBManager.SetDataTable(VSDataInfoDT);
				VSDBManager.SetDataTable(VSDataDT);
				VSDBManager.Disconnect();

				IndexDBManager.SetDataTable(SearchDataDT);
				IndexDBManager.Disconnect();

				VSDataDT.Clear();
				VSDataInfoDT.Clear();
				SearchDataDT.Clear();


				result = connector.SendNonQuery("INSERT INTO inspections.inspstatus (idx, inspStatusNum) VALUES ('0', '1') ON DUPLICATE KEY UPDATE idx='0', inspStatusNum='1';");
			}

			System.Drawing.PointF AffineTransform(System.Drawing.PointF ptSrc, float[] Coef)
			{
				System.Drawing.PointF ptRst = new System.Drawing.PointF();
				ptRst.X = (int)Math.Ceiling(ptSrc.X * Coef[0] + ptSrc.Y * Coef[1] + Coef[2]);
				ptRst.Y = (int)Math.Ceiling(ptSrc.X * Coef[3] + ptSrc.Y * Coef[4] + Coef[5]);
				return ptRst;
			}

			connector.Close();

			//Monitor.Wait(lockObj);
			m_bProgress = false;

			for (int i = 0; i < InspectionThread.Length; i++)
			{
				if (InspectionThread[i] != null)
				{
					InspectionThread[i].Dispose();
				}
			}

			if (inspThread != null)
			{
				if (inspThread.IsAlive)
				{
					inspThread.Interrupt();
					inspThread.Join();
				}
			}

		}

		public void ClearDefectList()
		{
			if(this.ClearDefect != null)
			{
				this.ClearDefect();
			}
		}
		public static void RefreshDefectDraw()
		{
			if(RefreshDefect != null)
			{
				RefreshDefect();
			}
		}
		public System.Windows.Media.Imaging.BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
		{
			var bitmapData = bitmap.LockBits(
				new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
				System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

			var bitmapSource = System.Windows.Media.Imaging.BitmapSource.Create(
				bitmapData.Width, bitmapData.Height,
				bitmap.HorizontalResolution, bitmap.VerticalResolution,
				System.Windows.Media.PixelFormats.Gray8, null,
				bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

			bitmap.UnlockBits(bitmapData);
			return bitmapSource;
		}
		public void Dispose()
		{
			if (!IsInitialized)
				return;

			if (InspectionThread != null)
			{
				for (int i = 0; i < InspectionThread.Length; i++)
				{
					//이벤트 핸들러 제거
					InspectionThread[i].AddChromeDefect -= InspectionManager_AddChromeDefect;
					//InsepctionThread[i].Dispose();
				}
			}
		}

		public void SetThread(int threadNum)  //(in int threadNum)  //2013 in 안됨
		{
			nThreadNum = threadNum;
		}

		public int GetWaitQueue()
		{
			return p_qInspection.Count;
		}

		public void AddInspection(InspectionProperty _property, bool bDefectMerge, int nMergeDistance)//(in InspectionProperty _property) //2013 in 안됨
		{
			_property.p_bDefectMerge = bDefectMerge;
			_property.p_nMergeDistance = nMergeDistance;

			p_qInspection.Enqueue(_property);
		}

		public void _clearInspReslut()
		{
			DBConnector connector = new DBConnector("localhost", "Inspections", "root", "`ati5344");
			if (connector.Open())
			{
				string dropQuery = "DROP TABLE Inspections.tempdata";
				var result = connector.SendNonQuery(dropQuery);
				Debug.WriteLine(string.Format("tempdata Table Drop : {0}", result));
				result = connector.SendNonQuery("INSERT INTO inspections.inspstatus (idx, inspStatusNum) VALUES ('0', '0') ON DUPLICATE KEY UPDATE idx='0', inspStatusNum='0';");
				Debug.WriteLine(string.Format("Status Clear : {0}", result));
			}
			connector.Close();

			// 검사 큐 클리어
			p_qInspection.Clear();

			// Defect Box 클리어
			ClearDefectList();
		}

		public void ClearInspection()
		{
			p_qInspection.Clear();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="WholeInspArea"></param>
		/// <param name="blocksize"></param>
		/// <param name="param"></param>
		/// <param name="bDefectMerge"></param>
		/// <param name="nMergeDistance"></param>
		/// <returns></returns>
		public List<CRect> CreateInspArea(string poolName, string groupName, string memoryName, ulong memOffset, int memWidth, int memHeight, CRect WholeInspArea, int blocksize, BaseParamData param, int dCode, bool bDefectMerge, int nMergeDistance, int nInnerRange, IntPtr ptrMemory)
		{
			List<CRect> inspblocklist = new List<CRect>();

			CRect IgnoreArea = new CRect(
				WholeInspArea.Left + nInnerRange,
				WholeInspArea.Top + nInnerRange,
				WholeInspArea.Right - nInnerRange,
				WholeInspArea.Bottom - nInnerRange);//검사영역에서 제외될 Rect

			int AreaStartX = WholeInspArea.Left;
			int AreaEndX = WholeInspArea.Right;
			int AreaStartY = WholeInspArea.Top;
			int AreaEndY = WholeInspArea.Bottom;

			int AreaWidth = WholeInspArea.Width;
			int AreaHeight = WholeInspArea.Height;

			//if (StartX + blocksize > EndX || StartY + blocksize > EndY)
			//{
			//    return;
			//}

			int iw = AreaWidth / blocksize;
			int ih = AreaHeight / blocksize;

			int wStart = 0;
			int wStop = iw;

			//if (nStart != -1 && nStop != -1)//검사영역을 제한하는 기능
			//{
			//	wStart = nStart;
			//	wStop = nStop;
			//}

			if (wStop == 0 || ih == 0)
			{
				// return;
			}
			else
			{
				//insp blockd에 대한 start xy, end xy
				int sx, sy, ex, ey;

				for (int i = wStart; i < wStop; i++)
				{
					sx = AreaStartX + i * blocksize;
					if (i < iw - 1) ex = sx + blocksize;
					else ex = AreaEndX;

					for (int j = 0; j < ih; j++)
					{
						sy = AreaStartY + j * blocksize;
						if (j < ih - 1) ey = sy + blocksize;
						else ey = AreaEndY;

						InspectionProperty ip = new InspectionProperty();
						ip.p_InspType = GetInspectionType(dCode);
						ip.m_nDefectCode = dCode;
						//ip.p_index = blockcount;
						ip.MemoryPoolName = poolName;
						ip.MemoryGroupName = groupName;
						ip.MemoryName = memoryName;
						ip.MemoryOffset = memOffset;
						ip.p_TargetMemWidth = memWidth;
						ip.p_TargetMemHeight = memHeight;
						ip.p_ptrMemory = ptrMemory;

						if (ip.p_InspType == InspectionType.Strip)
						{
							ip.p_StripParam = (StripParamData)param;
						}
						else if (ip.p_InspType == InspectionType.AbsoluteSurface || ip.p_InspType == InspectionType.AbsoluteSurface)
						{
							ip.p_surfaceParam = (SurfaceParamData)param;
						}

						CRect inspblock = new CRect(sx, sy, ex, ey);

						if (IgnoreArea.IsInside(new CPoint(inspblock.Left, inspblock.Top)) && IgnoreArea.IsInside(new CPoint(inspblock.Right, inspblock.Bottom)) && nInnerRange != 0)
						{
							//둘다 포함되는 경우엔 continue
							continue;
						}
						else if (inspblock.Right == IgnoreArea.Left && inspblock.Bottom == IgnoreArea.Top && nInnerRange != 0)
						{
							ip.p_Rect = inspblock;
							AddInspection(ip, bDefectMerge, nMergeDistance);
							inspblocklist.Add(inspblock);
							//blockcount++;
						}
						else if (
							IgnoreArea.IsInside(new CPoint(inspblock.Right, inspblock.Top)) &&
							IgnoreArea.IsInside(new CPoint(inspblock.Right, inspblock.Bottom)) &&
							nInnerRange != 0)
						{
							inspblock.Right = IgnoreArea.Left;
							ip.p_Rect = inspblock;
							AddInspection(ip, bDefectMerge, nMergeDistance);
							inspblocklist.Add(inspblock);
							//blockcount++;
						}
						else if (
							IgnoreArea.IsInside(new CPoint(inspblock.Left, inspblock.Top)) &&
							IgnoreArea.IsInside(new CPoint(inspblock.Right, inspblock.Top)) &&
							nInnerRange != 0)
						{
							inspblock.Top = IgnoreArea.Bottom;
							ip.p_Rect = inspblock;
							AddInspection(ip, bDefectMerge, nMergeDistance);
							inspblocklist.Add(inspblock);
							//blockcount++;
						}
						else if (
							IgnoreArea.IsInside(new CPoint(inspblock.Left, inspblock.Top)) &&
							IgnoreArea.IsInside(new CPoint(inspblock.Left, inspblock.Bottom)) &&
							nInnerRange != 0)
						{
							inspblock.Left = IgnoreArea.Right;
							ip.p_Rect = inspblock;
							AddInspection(ip, bDefectMerge, nMergeDistance);
							inspblocklist.Add(inspblock);
							//blockcount++;
						}
						else if (
							IgnoreArea.IsInside(new CPoint(inspblock.Left, inspblock.Bottom)) &&
							IgnoreArea.IsInside(new CPoint(inspblock.Right, inspblock.Bottom)) &&
							nInnerRange != 0)
						{
							inspblock.Bottom = IgnoreArea.Top;
							ip.p_Rect = inspblock;
							AddInspection(ip, bDefectMerge, nMergeDistance);
							inspblocklist.Add(inspblock);
							//blockcount++;
						}
						else if (IgnoreArea.IsInside(new CPoint(inspblock.Left, inspblock.Top)) &&
							!IgnoreArea.IsInside(new CPoint(inspblock.Right, inspblock.Bottom)) && 
							nInnerRange != 0)
						{
							CRect first = new CRect(inspblock);
							CRect second = new CRect(inspblock);
							CRect third = new CRect(inspblock);
							InspectionProperty ip2 = new InspectionProperty(ip);
							InspectionProperty ip3 = new InspectionProperty(ip);

							first.Left = IgnoreArea.Right;
							first.Bottom = IgnoreArea.Bottom;
							ip.p_Rect = first;

							second.Top = IgnoreArea.Bottom;
							second.Right = IgnoreArea.Right;
							ip2.p_Rect = second;

							third.Left = IgnoreArea.Right;
							third.Top = IgnoreArea.Bottom;
							ip3.p_Rect = third;

							AddInspection(ip, bDefectMerge, nMergeDistance);
							AddInspection(ip2, bDefectMerge, nMergeDistance);
							AddInspection(ip3, bDefectMerge, nMergeDistance);

							inspblocklist.Add(first);
							inspblocklist.Add(second);
							inspblocklist.Add(third);

							//blockcount += 3;
						}
						else if (IgnoreArea.IsInside(new CPoint(inspblock.Right, inspblock.Bottom)) &&
							!IgnoreArea.IsInside(new CPoint(inspblock.Left, inspblock.Top)) && 
							nInnerRange != 0)
						{
							//오른쪽 아래만 포함되는 경우. 세조각으로 자른다
							CRect first = new CRect(inspblock);
							CRect second = new CRect(inspblock);
							CRect third = new CRect(inspblock);
							InspectionProperty ip2 = new InspectionProperty(ip);
							InspectionProperty ip3 = new InspectionProperty(ip);

							first.Right = IgnoreArea.Left;
							first.Bottom = IgnoreArea.Top;
							ip.p_Rect = first;

							second.Left = IgnoreArea.Left;
							second.Bottom = IgnoreArea.Top;
							ip2.p_Rect = second;

							third.Right = IgnoreArea.Left;
							third.Top = IgnoreArea.Top;
							ip3.p_Rect = third;

							AddInspection(ip, bDefectMerge, nMergeDistance);
							AddInspection(ip2, bDefectMerge, nMergeDistance);
							AddInspection(ip3, bDefectMerge, nMergeDistance);

							inspblocklist.Add(first);
							inspblocklist.Add(second);
							inspblocklist.Add(third);

							//blockcount += 3;
						}
						else
						{
							//안겹치는 경우는 기존이랑 똑같이
							ip.p_Rect = inspblock;
							AddInspection(ip, bDefectMerge, nMergeDistance);
							inspblocklist.Add(inspblock);
							//blockcount++;
						}
					}
					//inspection offset, 모서리 영역 미구현
				}
			}

			return inspblocklist;
		}
		/// <summary>
		/// Defect Code를 int형식으로 변환하여 반환
		/// </summary>
		/// <param name="target">검사 대상</param>
		/// <param name="inspType">검사 알고리즘 종류</param>
		/// <param name="idx">ROI Index</param>
		/// <returns></returns>
		public static int MakeDefectCode(InspectionTarget target, InspectionType inspType, int idx)
		{
			int result = 0;

			result += (((int)target) * 10000);
			result += (((int)inspType) * 100);
			result += idx;

			return result;
		}
		/// <summary>
		/// Defect Code를 6자리 string 변환하여 반환 (앞에 0채움)
		/// </summary>
		/// <param name="target">검사 대상</param>
		/// <param name="inspType">검사 알고리즘 종류</param>
		/// <param name="idx">ROI Index</param>
		/// <returns></returns>
		public static string MakeDefectCodeToString(InspectionTarget target, InspectionType inspType, int idx)
		{
			int result = MakeDefectCode(target, inspType, idx);
			return result.ToString("D6");
		}
		/// <summary>
		/// Defect Code로 검사 영역을 확인하는 메소드
		/// </summary>
		/// <param name="nDefectCode"></param>
		/// <returns></returns>
		public static InspectionTarget GetInspectionTarget(int nDefectCode)
		{
			return (InspectionTarget)(nDefectCode / 10000);
		}
		/// <summary>
		/// Defect Code로 검출에 사용한 검사 알고리즘을 확인하는 메소드
		/// </summary>
		/// <param name="nDefectCode"></param>
		/// <returns></returns>
		public static InspectionType GetInspectionType(int nDefectCode)
		{
			int target = Convert.ToInt32(nDefectCode / 10000) * 10000;
			int result = (nDefectCode - target) / 100;
			return (InspectionType)result;
		}
		public static int GetROIInfo(int nDefectCode)
		{
			int target = Convert.ToInt32(nDefectCode / 10000) * 10000;
			int type = Convert.ToInt32((nDefectCode - target) / 100) * 100;
			return (nDefectCode - target - type);
		}
		//static ??
		private ObservableQueue<InspectionProperty> qInspection = new ObservableQueue<InspectionProperty>();
		public ObservableQueue<InspectionProperty> p_qInspection
		{
			get
			{
				return qInspection;
			}
			set
			{
				qInspection = value;
				OnPropertyChanged("p_qInspection");
			}
		}


		static object lockObj = new object();


		#region PropertyChanged
		public virtual event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string proertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(proertyName));
		}
		#endregion

		public static unsafe DPoint GetEdge(ImageData img, System.Windows.Rect rcROI, eEdgeFindDirection eDirection, bool bUseAutoThreshold, bool bUseB2D, int nThreshold)
		{
			// variable
			int nSum = 0;
			double dAverage = 0.0;
			int nEdgeY = 0;
			int nEdgeX = 0;

			// implement

			if (bUseAutoThreshold == true)
			{
				nThreshold = GetThresholdAverage(img, rcROI, eDirection);
			}

			switch (eDirection)
			{
				case eEdgeFindDirection.TOP:
					for (int i = 0; i < rcROI.Height; i++)
					{
						byte* bp;
						if (bUseB2D == true) bp = (byte*)(img.GetPtr((int)rcROI.Bottom - i, (int)rcROI.Left).ToPointer());
						else bp = (byte*)(img.GetPtr((int)rcROI.Top + i, (int)rcROI.Left).ToPointer());
						for (int j = 0; j < rcROI.Width; j++)
						{
							nSum += *bp;
							bp++;
						}
						dAverage = nSum / rcROI.Width;
						if (bUseB2D == true)
						{
							if (dAverage < nThreshold)
							{
								nEdgeY = (int)rcROI.Bottom - i;
								nEdgeX = (int)(rcROI.Left + (rcROI.Width / 2));
								break;
							}
						}
						else
						{
							if (dAverage > nThreshold)
							{
								nEdgeY = (int)rcROI.Top + i;
								nEdgeX = (int)(rcROI.Left + (rcROI.Width / 2));
								break;
							}
						}
						nSum = 0;
					}
					break;
				case eEdgeFindDirection.LEFT:
					for (int i = 0; i < rcROI.Width; i++)
					{
						byte* bp;
						if (bUseB2D == true) bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Right - i));
						else bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Left + i));
						for (int j = 0; j < rcROI.Height; j++)
						{
							nSum += *bp;
							bp += img.p_Stride;
						}
						dAverage = nSum / rcROI.Height;
						if (bUseB2D == true)
						{
							if (dAverage < nThreshold)
							{
								nEdgeX = (int)rcROI.Right - i;
								nEdgeY = (int)(rcROI.Top + (rcROI.Height / 2));
								break;
							}
						}
						else
						{
							if (dAverage > nThreshold)
							{
								nEdgeX = (int)rcROI.Left + i;
								nEdgeY = (int)(rcROI.Top + (rcROI.Height / 2));
								break;
							}
						}
						nSum = 0;
					}
					break;
				case eEdgeFindDirection.RIGHT:
					for (int i = 0; i < rcROI.Width; i++)
					{
						byte* bp;
						if (bUseB2D == true) bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Left + i));
						else bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Right - i));
						for (int j = 0; j < rcROI.Height; j++)
						{
							nSum += *bp;
							bp += img.p_Stride;
						}
						dAverage = nSum / rcROI.Height;
						if (bUseB2D == true)
						{
							if (dAverage < nThreshold)
							{
								nEdgeX = (int)rcROI.Left + i;
								nEdgeY = (int)(rcROI.Top + (rcROI.Height / 2));
								break;
							}
						}
						else
						{
							if (dAverage > nThreshold)
							{
								nEdgeX = (int)rcROI.Right - i;
								nEdgeY = (int)(rcROI.Top + (rcROI.Height / 2));
								break;
							}
						}
						nSum = 0;
					}
					break;
				case eEdgeFindDirection.BOTTOM:
					for (int i = 0; i < rcROI.Height; i++)
					{
						byte* bp;
						if (bUseB2D == true) bp = (byte*)(img.GetPtr((int)rcROI.Top + i, (int)rcROI.Left).ToPointer());
						else bp = (byte*)(img.GetPtr((int)rcROI.Bottom - i, (int)rcROI.Left).ToPointer());
						for (int j = 0; j < rcROI.Width; j++)
						{
							nSum += *bp;
							bp++;
						}
						dAverage = nSum / rcROI.Width;
						if (bUseB2D == true)
						{
							if (dAverage < nThreshold)
							{
								nEdgeY = (int)rcROI.Top + i;
								nEdgeX = (int)(rcROI.Left + (rcROI.Width / 2));
								break;
							}
						}
						else
						{
							if (dAverage > nThreshold)
							{
								nEdgeY = (int)rcROI.Bottom - i;
								nEdgeX = (int)(rcROI.Left + (rcROI.Width / 2));
								break;
							}
						}

						nSum = 0;
					}
					break;
			}

			return new System.Drawing.Point(nEdgeX, nEdgeY);
		}

		public void SetStandardPos(int nDefectCode, CPoint standardPos)
		{
			//DB에도 저장하고 메모리에도 올려둡시다
			//	//여기서 DB관련동작 이하생략!
			//	DBConnector connector = new DBConnector("localhost", "Inspections", "root", "`ati5344");
			//	if (connector.Open())
			//	{
			//		var result = connector.SendNonQuery("SELECT COUNT(*) FROM inspections.inspstatus;");
			//		if (result == (int)MYSQLError.TABLE_IS_MISSING)
			//		{
			//			//상태용 테이블 작성
			//			result = connector.SendNonQuery("CREATE TABLE inspections.inspstatus (idx INT NOT NULL, inspStatusNum INT NULL, PRIMARY KEY (idx));");
			//			if (result != 0)
			//			{
			//				//에러에 대한 추가 예외처리 필요
			//			}
			//		}
			//		//완료 표시
			//		result = connector.SendNonQuery("INSERT INTO inspections.inspstatus (idx, inspStatusNum) VALUES ('0', '1') ON DUPLICATE KEY UPDATE idx='0', inspStatusNum='1';");
			//	}
			if (refPosDictionary == null)
			{
				refPosDictionary = new Dictionary<int, CPoint>();//초기화
			}
			if (!refPosDictionary.ContainsKey(nDefectCode))
			{
				refPosDictionary.Add(nDefectCode, standardPos);
			}
			else
			{
				refPosDictionary[nDefectCode] = standardPos;
			}
		}

		static unsafe int GetThresholdAverage(ImageData img, System.Windows.Rect rcROI, eEdgeFindDirection eDirection)
		{
			// variable
			int nSum = 0;
			int nThreshold = 40;

			// implement

			if (eDirection == eEdgeFindDirection.TOP || eDirection == eEdgeFindDirection.BOTTOM)
			{
				double dRatio = rcROI.Height * 0.1;
				double dAverage1 = 0.0;
				double dAverage2 = 0.0;
				for (int i = 0; i < (int)dRatio; i++)
				{
					byte* bp = (byte*)(img.GetPtr((int)rcROI.Bottom - i, (int)rcROI.Left).ToPointer());
					for (int j = 0; j < rcROI.Width; j++)
					{
						nSum += *bp;
						bp++;
					}
				}
				dAverage1 = nSum / (rcROI.Width * (int)dRatio);
				nSum = 0;
				for (int i = 0; i < (int)dRatio; i++)
				{
					byte* bp = (byte*)(img.GetPtr((int)rcROI.Top + i, (int)rcROI.Left).ToPointer());
					for (int j = 0; j < rcROI.Width; j++)
					{
						nSum += *bp;
						bp++;
					}
				}
				dAverage2 = nSum / (rcROI.Width * (int)dRatio);
				nSum = 0;
				////////////////////////////////////////////////
				nThreshold = (int)(dAverage1 + dAverage2) / 2;
			}
			else
			{
				double dRatio = rcROI.Width * 0.1;
				double dAverage1 = 0.0;
				double dAverage2 = 0.0;
				for (int i = 0; i < (int)dRatio; i++)
				{
					byte* bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Right - i));
					for (int j = 0; j < rcROI.Height; j++)
					{
						nSum += *bp;
						bp += img.p_Stride;
					}
				}
				dAverage1 = nSum / (rcROI.Height * (int)dRatio);
				nSum = 0;
				for (int i = 0; i < (int)dRatio; i++)
				{
					byte* bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Left + i));
					for (int j = 0; j < rcROI.Height; j++)
					{
						nSum += *bp;
						bp += img.p_Stride;
					}
				}
				dAverage2 = nSum / (rcROI.Height * (int)dRatio);
				nSum = 0;
				////////////////////////////////////////////////
				nThreshold = (int)(dAverage1 + dAverage2) / 2;
			}

			return nThreshold;
		}
		/// <summary>
		/// ROI 영역 내의 성분 방향을 획득하여 반환한다
		/// </summary>
		/// <param name="img"></param>
		/// <param name="rcROI"></param>
		/// <returns></returns>
		public static unsafe eEdgeFindDirection GetDirection(ImageData img, System.Windows.Rect rcROI)
		{
			// variable
			double dRatio = 0.0;
			int nSum = 0;
			double dAverageTemp = 0.0;
			Dictionary<eBrightSide, double> dic = new Dictionary<eBrightSide, double>();

			// implement
			// Left
			dRatio = rcROI.Width * 0.1;
			for (int i = 0; i < (int)dRatio; i++)
			{
				byte* bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Left + i));
				for (int j = 0; j < rcROI.Height; j++)
				{
					nSum += *bp;
					bp += img.p_Stride;
				}
			}
			dAverageTemp = nSum / (rcROI.Height * (int)dRatio);
			dic.Add(eBrightSide.LEFT, dAverageTemp);
			nSum = 0;

			// Top
			dRatio = rcROI.Height * 0.1;
			for (int i = 0; i < (int)dRatio; i++)
			{
				byte* bp = (byte*)(img.GetPtr((int)rcROI.Top + i, (int)rcROI.Left).ToPointer());
				for (int j = 0; j < rcROI.Width; j++)
				{
					nSum += *bp;
					bp++;
				}
			}
			dAverageTemp = nSum / (rcROI.Width * (int)dRatio);
			dic.Add(eBrightSide.TOP, dAverageTemp);
			nSum = 0;

			// Right
			dRatio = rcROI.Width * 0.1;
			for (int i = 0; i < (int)dRatio; i++)
			{
				byte* bp = (byte*)(img.GetPtr((int)rcROI.Top, (int)rcROI.Right - i).ToPointer());
				for (int j = 0; j < rcROI.Height; j++)
				{
					nSum += *bp;
					bp += img.p_Stride;
				}
			}
			dAverageTemp = nSum / (rcROI.Height * (int)dRatio);
			dic.Add(eBrightSide.RIGHT, dAverageTemp);
			nSum = 0;

			// Bottom
			dRatio = rcROI.Height * 0.1;
			for (int i = 0; i < (int)dRatio; i++)
			{
				byte* bp = (byte*)(img.GetPtr((int)rcROI.Bottom - i, (int)rcROI.Left).ToPointer());
				for (int j = 0; j < rcROI.Width; j++)
				{
					nSum += *bp;
					bp++;
				}
			}
			dAverageTemp = nSum / (rcROI.Width * (int)dRatio);
			dic.Add(eBrightSide.BOTTOM, dAverageTemp);
			nSum = 0;

			var maxKey = dic.Keys.Max();
			var maxValue = dic.Values.Max();
			// Value값이 가장 큰 Key값 찾기
			var keyOfMaxValue = dic.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;

			if (keyOfMaxValue == eBrightSide.TOP) return eEdgeFindDirection.BOTTOM;
			else if (keyOfMaxValue == eBrightSide.BOTTOM) return eEdgeFindDirection.TOP;
			else if (keyOfMaxValue == eBrightSide.LEFT) return eEdgeFindDirection.RIGHT;
			else return eEdgeFindDirection.LEFT;
		}
	}

	public class InspectionProperty : ObservableObject
	{
		InspectionType InspType = InspectionType.None;
		public InspectionType p_InspType
		{
			get
			{
				return InspType;
			}
			set
			{
				SetProperty(ref InspType, value);
			}
		}
		int targetMemWidth;
		public int p_TargetMemWidth
		{
			get { return this.targetMemWidth; }
			set
			{
				SetProperty(ref targetMemWidth, value);
			}
		}
		int targetMemHeight;
		public int p_TargetMemHeight
		{
			get { return this.targetMemHeight; }
			set
			{
				SetProperty(ref targetMemHeight, value);
			}
		}
		CRect Rect;
		public CRect p_Rect
		{
			get
			{
				return Rect;
			}
			set
			{
				SetProperty(ref Rect, value);
			}
		}
		public int m_nDefectCode;
		bool bDefectMerge;
		public bool p_bDefectMerge
		{
			get
			{
				return bDefectMerge;
			}
			set
			{
				SetProperty(ref bDefectMerge, value);
			}
		}
		int nMergeDistance;
		public int p_nMergeDistance
		{
			get
			{
				return nMergeDistance;
			}
			set
			{
				SetProperty(ref nMergeDistance, value);
			}
		}
		SurfaceParamData surfaceParam;
		public SurfaceParamData p_surfaceParam
		{
			get
			{
				return surfaceParam;
			}
			set
			{
				SetProperty(ref surfaceParam, value);
			}
		}
		StripParamData stripParam;
		public StripParamData p_StripParam
		{
			get
			{
				return stripParam;
			}
			set
			{
				SetProperty(ref stripParam, value);
			}
		}
		int index = 0;
		public InspectionProperty()
		{

		}
		public InspectionProperty(InspectionProperty ip)
		{
			p_InspType = ip.p_InspType;
			m_nDefectCode = ip.m_nDefectCode;
			p_index = ip.p_index;
			MemoryPoolName = ip.MemoryPoolName;
			MemoryGroupName = ip.MemoryGroupName;
			MemoryName = ip.MemoryName;
			MemoryOffset = ip.MemoryOffset;
			p_TargetMemWidth = ip.p_TargetMemWidth;
			p_TargetMemHeight = ip.p_TargetMemHeight;
			p_ptrMemory = ip.p_ptrMemory;
			p_StripParam = ip.p_StripParam;
			p_surfaceParam = ip.p_surfaceParam;
		}

		public int p_index
		{
			get
			{
				return index;
			}
			set
			{
				SetProperty(ref index, value);
			}
		}
		public string MemoryPoolName { get; set; }
		public string MemoryGroupName { get; set; }
		public string MemoryName { get; set; }
		public ulong MemoryOffset { get; set; }

		public IntPtr p_ptrMemory { get; set; }
	}
	public class MemInfo
	{


	}
}
