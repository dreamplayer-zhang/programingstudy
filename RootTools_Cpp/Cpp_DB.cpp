#include "pch.h"
#include "Cpp_DB.h"
#include "iostream"
#include "fstream"


Cpp_DB::Cpp_DB()
{
	//m_sDBPath = "";
}


Cpp_DB::~Cpp_DB()
{
}
int Cpp_DB::test()
{
	ofstream ff;

	ff.open("c:\\Log\\abc.txt");
	ff << 1;

	return 1;
}
bool Cpp_DB::SetDBPath(string path)
{

	return true;
}
bool Cpp_DB::OpenAndBegin(string path)
{
	char* dbpath = (char*)path.c_str();
	int errcode = 0;
	string str, temp;
	char *zErrMsg = 0;

	errcode = sqlite3_open(dbpath, &vsdb);

	if (errcode != SQLITE_OK)
	{
		return false;
	}

	str = "BEGIN;";
	char* cmd;
	cmd = (char*)str.c_str();

	/* Execute SQL statement */
	errcode = sqlite3_exec(vsdb, cmd, callback, 0, &zErrMsg);

	return true;
}
bool Cpp_DB::InsertData(string data)
{
	int errcode = 0;
	string str, temp;
	char *zErrMsg = 0;

	str = "INSERT INTO Tempdata (DCode,Size,Width,Height,PosX,PosY,RectL,RectT,RectR,RectB)"\
		" VALUES (" + data + "); ";
	//Tempdata, @No(INTEGER), DCode(INTEGER), Size(INTEGER), Width(INTEGER), Height(INTEGER), PosX(INTEGER), PosY(INTEGER), RectL(INTEGER), RectT(INTEGER), RectR(INTEGER), RectB(INTEGER)
	/*
	"No        INT PRIMARY KEY	AUTOINCREMENT  NOT NULL,"\
		"PosX      INT              NOT NULL,"\
		"PosY      INT              NOT NULL,"\
		"Darea     REAL             NOT NULL,"\
		"UnitX     INT              NOT NULL,"\
		"UnitY     INT              NOT NULL,"\
		"Width     INT              NOT NULL,"\
		"Height    INT              NOT NULL,"\
		"ClusterID INT              NOT NULL,"\
		"RectL     INT              NOT NULL,"\
		"RectT     INT              NOT NULL,"\
		"RectR     INT              NOT NULL,"\
		"RectB     INT              NOT NULL,"\
		"Dname     TEXT                     ,"\
		"Dcode     INT              NOT NULL,"\
		"Threadindex	INT			NOT NULL,"\
		"isthislast     INT         NOT NULL );";
	*/

	char* cmd;
	cmd = (char*)str.c_str();

	/* Execute SQL statement */
	errcode = sqlite3_exec(vsdb, cmd, callback, 0, &zErrMsg);

	if (errcode != SQLITE_OK){

		sqlite3_free(zErrMsg);
	}
	else {
	}

	return true;
}
bool Cpp_DB::Commit()
{
	int errcode = 0;
	string str;
	char* zErrMsg = 0;

	str = "COMMIT;";
	char* cmd;
	cmd = (char*)str.c_str();

	/* Execute SQL statement */
	errcode = sqlite3_exec(vsdb, cmd, callback, 0, &zErrMsg);

	if (errcode != SQLITE_OK)
	{

	}

	//sqlite3_close(vsdb);

	return true;
}
bool Cpp_DB::CommitAndClose()
{
	int errcode = 0;
	string str;
	char *zErrMsg = 0;

	str = "COMMIT;";
	char* cmd;
	cmd = (char*)str.c_str();

	/* Execute SQL statement */
	errcode = sqlite3_exec(vsdb, cmd, callback, 0, &zErrMsg);

	if (errcode != SQLITE_OK)
	{
		
	}

	sqlite3_close(vsdb);

	return true;
}
bool Cpp_DB::EraseAndClose(string path)
{
	char* dbpath = (char*)path.c_str();
	//해당 경로에 TEMP DATA DB 파일이 있는 경우 data를 전부 삭제
	//해당 경로에 없는 경우 db file을 새로 생성함
	int errcode = 0;
	sqlite3* db;
	string str = "";
	char* cmd;
	char* zErrMsg = 0;

	ifstream f(dbpath);
	if (f.good())	//file exist
	{
		f.close();
		str = "DELETE FROM TempData;";
	}
	errcode = sqlite3_open(dbpath, &db);
	if (errcode != SQLITE_OK)
	{
		sqlite3_free(zErrMsg);
		return false;
	}

	cmd = (char*)str.c_str();

	errcode = sqlite3_exec(db, cmd, callback, 0, &zErrMsg);


	if (errcode != SQLITE_OK)
	{
		sqlite3_free(zErrMsg);
		return false;
	}

	sqlite3_close(db);
	return true;
}

int Cpp_DB::DBCreateVSTemp(string path)
{
	char* dbpath = (char*)path.c_str();
	//해당 경로에 TEMP DATA DB 파일이 있는 경우 data를 전부 삭제
	//해당 경로에 없는 경우 db file을 새로 생성함
	int errcode = 0;
	sqlite3 *db;
	string str = "";
	char* cmd;
	char *zErrMsg = 0;
	
	

	// db 저장 data에 대한 세팅, ver에 대한거를 생각해봐야함
	// 현재 구조는 저장할 defect 정보 하나 더 추가하려면 복잡하게 해야함


	ifstream f(dbpath);

	if (f.good())	//file exist
	{
		//f.close();
		//str = "DELETE FROM TempData;";
	}
	else
	{
		f.close();
		
		//table column 설정 방안 논의 필요
		//C# List - Table adapter?
		
		str = "CREATE TABLE TempData("\
			"No        INTEGER PRIMARY KEY AUTOINCREMENT,"\
			"PosX      INT              NOT NULL,"\
			"PosY      INT              NOT NULL,"\
			"Darea     REAL             NOT NULL,"\
			"UnitX     INT              NOT NULL,"\
			"UnitY     INT              NOT NULL,"\
			"Width     INT              NOT NULL,"\
			"Height    INT              NOT NULL,"\
			"ClusterID INT              NOT NULL,"\
			"RectL     INT              NOT NULL,"\
			"RectT     INT              NOT NULL,"\
			"RectR     INT              NOT NULL,"\
			"RectB     INT              NOT NULL,"\
			"Dname     TEXT                     ,"\
			"Dcode     INT              NOT NULL,"\
			"Threadindex	INT			NOT NULL,"\
			"isthislast     INT         NOT NULL );";

	}

	errcode = sqlite3_open(dbpath, &db);

	if (errcode != SQLITE_OK)
	{
		return errcode;
	}

	cmd = (char*)str.c_str();

	errcode = sqlite3_exec(db, cmd, callback, 0, &zErrMsg);
	
	
	if (errcode != SQLITE_OK)
	{
		ofstream ff;

		sqlite3_free(zErrMsg);
		return errcode;
	}

	sqlite3_close(db);
	return errcode;
}
int Cpp_DB::DBOpenAndWrite(string path, string querry)
{
	char* m_sDBPath = (char*)path.c_str();
	int errcode = 0;
	sqlite3 *db;
	string str, temp;
	char *zErrMsg = 0;

	//string aa = "C:\\sqlite\\db\\VS1.sqlite";
	//char* bb = (char*)aa.c_str();
	//errcode = sqlite3_open(bb, &db);

	errcode = sqlite3_open(m_sDBPath, &db);

	if (errcode != SQLITE_OK)
	{
		return 111;
	}

	//str = "INSERT INTO " + tablename + " (" + column + ") " + "VALUES (" + value + "); ";
	str = "INSERT INTO TempData "\
		"VALUES (" + querry + "); ";

	//str = "INSERT INTO DATA (No,Size)" + "VALUES (10, 20); ";


	char* cmd;
	cmd = (char*)str.c_str();

	/* Execute SQL statement */
	errcode = sqlite3_exec(db, cmd, callback, 0, &zErrMsg);

	if (errcode != SQLITE_OK){

		ofstream ff;

		ff.open("c:\\Log\\abc.txt");
		
		ff << zErrMsg;

		sqlite3_free(zErrMsg);
	}
	else {
	}
	sqlite3_close(db);




	return errcode;
}


int Cpp_DB::DBTest(string path)
{
	char* ver = (char*)path.c_str();


	ifstream f(ver);

	f.good();	//파일이 존재하고 있으면 1



	return f.good();
}


//#define SQLITE_OK           0   /* Successful result */
///* beginning-of-error-codes */
//#define SQLITE_ERROR        1   /* Generic error */
//#define SQLITE_INTERNAL     2   /* Internal logic error in SQLite */
//#define SQLITE_PERM         3   /* Access permission denied */
//#define SQLITE_ABORT        4   /* Callback routine requested an abort */
//#define SQLITE_BUSY         5   /* The database file is locked */
//#define SQLITE_LOCKED       6   /* A table in the database is locked */
//#define SQLITE_NOMEM        7   /* A malloc() failed */
//#define SQLITE_READONLY     8   /* Attempt to write a readonly database */
//#define SQLITE_INTERRUPT    9   /* Operation terminated by sqlite3_interrupt()*/
//#define SQLITE_IOERR       10   /* Some kind of disk I/O error occurred */
//#define SQLITE_CORRUPT     11   /* The database disk image is malformed */
//#define SQLITE_NOTFOUND    12   /* Unknown opcode in sqlite3_file_control() */
//#define SQLITE_FULL        13   /* Insertion failed because database is full */
//#define SQLITE_CANTOPEN    14   /* Unable to open the database file */
//#define SQLITE_PROTOCOL    15   /* Database lock protocol error */
//#define SQLITE_EMPTY       16   /* Internal use only */
//#define SQLITE_SCHEMA      17   /* The database schema changed */
//#define SQLITE_TOOBIG      18   /* String or BLOB exceeds size limit */
//#define SQLITE_CONSTRAINT  19   /* Abort due to constraint violation */
//#define SQLITE_MISMATCH    20   /* Data type mismatch */
//#define SQLITE_MISUSE      21   /* Library used incorrectly */
//#define SQLITE_NOLFS       22   /* Uses OS features not supported on host */
//#define SQLITE_AUTH        23   /* Authorization denied */
//#define SQLITE_FORMAT      24   /* Not used */
//#define SQLITE_RANGE       25   /* 2nd parameter to sqlite3_bind out of range */
//#define SQLITE_NOTADB      26   /* File opened that is not a database file */
//#define SQLITE_NOTICE      27   /* Notifications from sqlite3_log() */
//#define SQLITE_WARNING     28   /* Warnings from sqlite3_log() */
//#define SQLITE_ROW         100  /* sqlite3_step() has another row ready */
//#define SQLITE_DONE        101  /* sqlite3_step() has finished executing */
