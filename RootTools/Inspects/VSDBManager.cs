using System;
using System.Data;
using System.Data.SQLite;

namespace RootTools.Inspects
{
    public class VSDBManager
    {

        //선언부
        //private static string VSDBFileName = "";                //DB File 이름(확장자 포함)
        //private static string VSDBFolderPath = "";              //DB 생성 Folder 경로
        private static string VSDBFullPath = "";                //DB 생성 Full 경로(File이름까지 포함)
        //private static string VSDBConfigPath = "";              //DB Config file 경로(VSDB table 정보)
        private static SQLiteConnection VSDBconnect = null;     //SQLITE Connect class
        //private static SQLiteCommand cmd;
        //private static SQLiteDataAdapter adapter;

        public bool ConnectVSDB(string path)
        {
            //테스트용
            //VSDBFullPath = @"C:/vsdb\VSTEMP0.sqlite";
            VSDBFullPath = path;

            //DB Connect
            bool result = true;
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = VSDBFullPath;
            builder.Version = 3;

            VSDBconnect = new SQLiteConnection();
            VSDBconnect.ConnectionString = builder.ConnectionString;
            try
            {
                VSDBconnect = VSDBconnect.OpenAndReturn();
                result = true;
            }
            catch (Exception)
            {
                result = false;

            }

            return result;
        }
        public bool CreateVSDB()  //경로, 파일 이름을 받아 DB File 생성(설정에 세팅된 테이블 정보를 바탕으로 생성)
        {
            bool result = true;

            //db file 생성
            if (result)
            {
                SQLiteConnection.CreateFile(VSDBFullPath);

                //생성된 file 유무 확인
                if (!System.IO.File.Exists(VSDBFullPath))
                    result = false;
            }


            //if (result)
            //{
            //    //생성된 dbfile에 Connect
            //    if (ConnectVSDB(VSDBFullPath))
            //    {
            //        SetVSDBTable(); //db table 생성(txt config 기반)
            //    }
            //    else
            //        result = false;
            //}


            //string str;
            //for (int i = 0; i < 10; i++)
            //{
            //    str = string.Format("{0} {1}", i, i * 5);
            //    InsertDataLine("Data", str, VSDBPath, sVSDBConfig, TableCount);
            //}

            //CloseVSDB();// db 연결 해제
            return result;

        }

        public DataTable DBFillToTable(string tablename)
        {
            string cmdstr;
            cmdstr = string.Format("SELECT * FROM {0}", tablename);
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmdstr, VSDBconnect);
            DataTable DT = new DataTable();
            adapter.Fill(DT);

            VSDBconnect.Close();
            return DT;
        }
        public bool CloseVSDB()
        {
  
            VSDBconnect.Close();
            return true;
        }



    }
}
