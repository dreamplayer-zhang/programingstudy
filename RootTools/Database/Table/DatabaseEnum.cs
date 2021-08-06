using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools.Database.Table
{
	public enum ID
	{
		MainQuery = 100,
	}
	public enum DB_ERROR
	{
		CONNECT_ERROR = -2,//연결실패. 커스텀에러
		INIT_ERROR = -1,//초기화를 하지 않음. 커스텀 에러
		SUCCESS = 0, // 성공
		AUTH_FAIL = 1044, //권한 없음
		DB_NOT_FOUND = 1049,//DB 찾을 수 없음.
		QUERY_IS_NOT_CORRECT = 1064,//쿼리문이 잘못됨
		TABLE_IS_MISSING = 1146,//Table이 없음
		COLUMN_IS_MISMATCHING = 1136,//컬럼개수가 일치하지않음 
		UNKNOWN_TABLE = 1051, // 알수 없는 테이블 이름
	}
}
