using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RootTools.Database
{
    /// <summary>
    /// Defect 데이터 관리 및 쿼리 전달
    /// </summary>
    public class DefectManager
    {
        /// <summary>
        /// DEFECT 관련 처리.
        /// </summary>
        public DefectManager()
        {         

        }

        public static void AddDefect(int _nThreadID, Defect _DefectData)
        {
            //string sInspectionID = DatabaseManager.Instance.GetInspectionID();
            //Lotinfo CurrentLotinfo = DatabaseManager.Instance.GetCurrentLotInfo();

            //// DB Insert
            //string sInsertQuery;
            //sInsertQuery =
            //   string.Format("INSERT INTO tempdefect(INSPECTIONID,DCODE,SIZE) values('{0}',{1},{2})"
            //    , sInspectionID
            //    , _DefectData.GetDcode()
            //    , _DefectData.GetDsize());

            //// Table Insert 쿼리

            //DatabaseManager.Instance.SendThreadQuery(_nThreadID, sInsertQuery);
        }

        public static void DoProcessDefect(int _nID, Cpp_LabelParam[] lables)
        {
            int nLength = lables.Length;
            if (nLength < 1) return;

            ////Defect defect = new Defect();
            //string sCurrentInspectionID = DatabaseManager.Instance.GetInspectionID();
            //for(int i =0; i <nLength; i++)
            //{
            //    defect.SetDefect(sCurrentInspectionID, 2002, lables[i].area);
            //    AddDefect(_nID, defect);
            //}
        }


    }
}
