using System.ComponentModel;
using System.Threading;
using System.Collections;
using System.Data;
using ATI;
using System.IO;
using System.Collections.Generic;

namespace RootTools.Inspects
{
    public class InspectionManager : Singleton<InspectionManager>, INotifyPropertyChanged
    {
        int nThreadNum = 4;
        int nInspectionCount = 0;

        #region EventHandler
        /// <summary>
        /// 이벤트 핸들러
        /// </summary>
        public delegate void EventHandler();
        public EventHandler InspectionComplete;
        #endregion

        public void StartInspection()
        {
            if (0 < GetWaitQueue())
            {
                if (GetWaitQueue() < nThreadNum)
                {
                    nThreadNum = GetWaitQueue();
                }

                Thread thread = new Thread(DoInspection);
                thread.Start();
            }
        }

        //VSDBManager VSDBReader = new VSDBManager();
        

        /// <summary>
        /// Thread 갯수 만큼 db temp file 생성
        /// </summary>
        public void CreateVSTempDB()
        {
            for (int i = 0; i < nThreadNum; i++)
            {
                string dbtemp_path = @"C:/vsdb\VSTEMP" + i.ToString() + ".sqlite";
                string dbtemp_configpath = @"C:/vsdb/init/vsdb.txt";

                if (!File.Exists(dbtemp_path))
                {

                    SqliteDataDB VSDBTempManager = new SqliteDataDB(dbtemp_path, dbtemp_configpath);
                    VSDBTempManager.Connect();


                    //file이 있으면 현재 init coulumn이랑 비교해서 다르면 삭제 하고 새로 만들게 하자
                    // init column이랑 같으면 안에 data 비우게 하고

                    VSDBTempManager.CreateTable("Tempdata");
                    VSDBTempManager.Disconnect();
                    //Tempdata,*No(INTEGER),DCode(INTEGER),Size(INTEGER),Length(INTEGER),Width(INTEGER),Height(INTEGER),InspMode(INTEGER),FOV(INTEGER),PosX(INTEGER),PosY(INTEGER),RectL(INTEGER),RectT(INTERGER),RectR(INTEGER),RectB(INTEGER),Threadindex(INTEGER)
                }
            }

        }
        string VSDB_path;//검사 결과 db 파일
        string VSDB_configpath;//검사 결과 db 파일
        string VSIMG_path;//검사 이미지 파일(tiff) 
        /// <summary>
        /// Temp DB에 저장된 data를 꺼내 VSData DB file에 담는 매소드
        /// </summary>
        public void CollectVSTempDB()
        {
            //DATA 출력 TEST
            string time = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            VSDB_path = @"C:/vsdb/TestData" + time + ".sqlite";
            VSDB_configpath = @"C:/vsdb/init/vsdb.txt";


            SqliteDataDB VSDBManager = new SqliteDataDB(VSDB_path, VSDB_configpath);
            VSDBManager.Connect();
            VSDBManager.CreateTable("Datainfo");
            VSDBManager.CreateTable("Data");
            DataTable VSDataInfoDT = VSDBManager.GetDataTable("Datainfo");
            DataTable VSDataDT = VSDBManager.GetDataTable("Data");
            int datacount = 0;

            //VS data
            for (int i = 0; i < nThreadNum; i++)
            {
                string dbtemp_path = @"C:/vsdb/VSTEMP" + i.ToString() + ".sqlite";
                string dbtemp_configpath = @"C:/vsdb/init/vsdb.txt";

                dbtemp_path =  dbtemp_path.Replace("/", "\\");
                dbtemp_configpath = dbtemp_configpath.Replace("/", "\\");

                SqliteDataDB VSDBTempManager = new SqliteDataDB(dbtemp_path, dbtemp_configpath);

                VSDBTempManager.Connect();
                DataTable TempDT = VSDBTempManager.GetDataTable("Tempdata");

                for (int j = 0; j < TempDT.Rows.Count; j++)
                {
                    //object temp = TempDT.Rows[j].ItemArray;

                    DataRow DataRow;
                    DataRow = VSDataDT.NewRow();

                    string temp;
                    //temp = "No";
                    //DataRow[temp] = datacount;
                    temp = "DCode";
                    DataRow[temp] = ReadData(temp, TempDT.Rows[j]);
                    temp = "Size";
                    DataRow[temp] = ReadData(temp, TempDT.Rows[j]);
                    temp = "Width";
                    object w = ReadData(temp, TempDT.Rows[j]);
                    DataRow[temp] = w;
                    temp = "Height";
                    object h = ReadData(temp, TempDT.Rows[j]);
                    DataRow[temp] = h;
                    
                    int l = System.Convert.ToInt32(w);
                    //int l = (int)w;
                    if (l < System.Convert.ToInt32(h))
                        l = System.Convert.ToInt32(h);
                    temp = "Length";
                    DataRow[temp] = l;

                    /*
                    temp = "InspMode";
                    DataRow[temp] = ReadData(temp, TempDT.Rows[j]);
                    temp = "FOV";
                    DataRow[temp] = ReadData(temp, TempDT.Rows[j]);
                    temp = "PosX";
                    DataRow[temp] = ReadData(temp, TempDT.Rows[j]);
                    temp = "PosY";
                    DataRow[temp] = ReadData(temp, TempDT.Rows[j]);
                    */
                    //Data,*No(INTEGER),DCode(INTEGER),Size(INTEGER),Length(INTEGER),Width(INTEGER),Height(INTEGER),InspMode(INTEGER),FOV(INTEGER),PosX(INTEGER),PosY(INTEGER)

                    VSDataDT.Rows.Add(DataRow);
                    datacount++;
                }

                VSDBTempManager.Disconnect();
                if(File.Exists(dbtemp_path))
                {
                    File.Delete(dbtemp_path);
                }
            }
            VSDBManager.SaveDataTable(VSDataDT);


            //VSdatainfo
            DataRow DatainfoRow;
            DatainfoRow = VSDataInfoDT.NewRow();
            string infotemp;
            infotemp = "BCRID";
            DatainfoRow[infotemp] = "ABCD1234";
            infotemp = "RCPID";
            DatainfoRow[infotemp] = "TestRecipe";
            infotemp = "DataSaveTime";
            DatainfoRow[infotemp] = time;
            infotemp = "DefectCount";
            DatainfoRow[infotemp] = datacount;

            VSDataInfoDT.Rows.Add(DatainfoRow);
        }

        public object ReadData(string column, DataRow row)
        {
            object data = row[column];

            return data;
        }

        public void PreInspection()
        {
            CreateVSTempDB();




        }
        public void EndInspection()
        {
            CollectVSTempDB();




        }

        public void DoInspection()
        {
            int nInspDoneNum = 0;
            Inspection[] inspection = new Inspection[nThreadNum];

            for (int i = 0; i < nThreadNum; i++)
            {
                inspection[i] = new Inspection();
            }

            while (true)
            {
                lock (lockObj)
                {
                    for (int i = 0; i < nThreadNum; i++)
                    {
                        if (inspection[i].bState == Inspection.InspectionState.Done)
                        {
                            inspection[i].bState = Inspection.InspectionState.Ready;
                            nInspDoneNum++;
                        }
                    }

                    while (p_qInspection.Count == 0 && nInspDoneNum == nInspectionCount)
                    {
                        InspectionDone();
                        Monitor.Wait(lockObj);
                    }

                    for (int i = 0; i < nThreadNum; i++)
                    {
                        if (0 < GetWaitQueue())
                        {
                            if (inspection[i].bState == Inspection.InspectionState.Ready ||
                            inspection[i].bState == Inspection.InspectionState.None)
                            {
                                nInspectionCount++;
                                InspectionProperty ipQueue = p_qInspection.Dequeue();
                                inspection[i].StartInspection(ipQueue, i);
                            }
                        }
                    }
                }
            }
            //여기서 완료 이벤트 발생
        }

        public void InspectionDone()
        {
            if(InspectionComplete!=null)
            {
                InspectionComplete();
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

        public void AddInspection(InspectionProperty _property)//(in InspectionProperty _property) //2013 in 안됨
        {
            p_qInspection.Enqueue(_property);
        }

        public void ClearInspection()
        {
            p_qInspection.Clear();
        }
        public List<CRect> CreateInspArea(CRect WholeInspArea, int blocksize, SurFace_ParamData param)
        {
            List<CRect> inspblocklist = new List<CRect>();

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

            if (iw == 0 || ih == 0)
            {
                // return;
            }
            else
            {
                //insp blockd에 대한 start xy, end xy
                int sx, sy, ex, ey;
                int blockcount = 1;

                for (int i = 0; i < iw; i++)
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
                        ip.p_InspType = RootTools.Inspects.InspectionType.Surface;

                        CRect inspblock = new CRect(sx, sy, ex, ey);
                        ip.p_Rect = inspblock;
                        ip.p_Sur_Param = param;
                        ip.p_index = blockcount;
                        AddInspection(ip);
                        blockcount++;

                        inspblocklist.Add(inspblock);
                    }
                    //inspection offset, 모서리 영역 미구현

                }
            }

            return inspblocklist;

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
        SurFace_ParamData Sur_Param;
        public SurFace_ParamData p_Sur_Param
        {
            get
            {
                return Sur_Param;
            }
            set
            {
                SetProperty(ref Sur_Param, value);
            }
        }
        int index = 0;
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
    }

    public enum InspectionType
    {
        None,
        Surface,
        Pattern
    };
}
