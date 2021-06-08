using RootTools;
using RootTools.Database;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision.Utility
{
    public class KlarfData_Lot
    {
		public KlarfData_Lot()
		{
			klarfData = new List<KlarfData>();

			fileVer1 = 1;
			fileVer2 = 1;

			tiffSpec = "6.0 G R;";
			dieOriginX = 0.0f;
			dieOriginY = 0.0f;

			//inspectionTest = 1;

			// 초기화
			inspectionStationVender = "ATI";
			inspectionStationModel = "WIND";
			inspectionStationMachineID = "";
			stepID = "";
			setupID = "";
			timeResult = DateTime.Now;
			timeRecipe = DateTime.Now;
			sampleType = "WAFER";
			sampleOrientationMarkType = "NOTCH";
			sampleSize = 300;
			diePitchX = 0.0f;
			diePitchY = 0.0f;
			lotID = "";
			//lotNum = "";
			waferID = "";
			//mesLotID = "";
			deviceID = "";
			orientationMarkLocation = "DOWN";

			timeFile = DateTime.Now;
			klarfFileName = "";
			sampleCenterLocationX = 0.0f;
			sampleCenterLocationY = 0.0f;
			slotID = 1;

			klarfType = 0;
		}
		
		#region [Variables]
		private List<KlarfData> klarfData = null;

		private int fileVer1, fileVer2;               // Klarf Ver : 사실 우린 의미가 없음.. 1 1 넣음됨.

		private String inspectionStationVender;       // 설비 제작 업체.
		private String inspectionStationModel;        // 설비 종류.
		private String inspectionStationMachineID;    // 업체에서 관리하는 장비 번호.

		private String stepID;                        // MI Team에서 제작한 규칙. 비전에서 모델별로 수동입력 받음.
		private String setupID;                       // Recipe 관리 명. 사용자가 임의 입력.
		private String deviceID;                      // Device ID
		private String lotID;                         // Lot ID
		//private String lotNum;                        // Lot Num	현재 데이터에 안넣음 필요하면 넣지 뭐...
		private String waferID;                       // Lot Num	현재 데이터에 안넣음 필요하면 넣지 뭐...
		private String partID;                        // Part ID
		private String MEMMAP_FileName;
		private String klarf_FileName;
		private String cassetteID;


		private DateTime timeLotStart;				  //210531 Lot Start 시간

		private DateTime timeFile;                    // Klarf 생성 시간.
		private DateTime timeResult;                  // 검사 종료 시간.
		private DateTime timeRecipe;                  // Recipe 생성 시간.
		private DateTime timeResultStamp;

		private string sampleType;                    // 검사 제품의 종류. (ex WAFER)
		private string sampleOrientationMarkType;     // FLAT or NOTCH
		private string orientationMarkLocation;       // Flat zone 방향 // UP, DOWN, LEFT, RIGHT

		private double diePitchX, diePitchY;          // Die Pitch
		private double dieOriginX, dieOriginY;        // 센터 기준. 무조건 0, 0 으로 보고 셋팅
		private double sampleCenterLocationX;         // 센터 Die의 Left,bottom와 실제 제품 Center간의 차이.
		private double sampleCenterLocationY;         // 센터 Die의 Left,bottom와 실제 제품 Center간의 차이.
		private System.Windows.Size chipSize;                        // pixel 기준 칩 크기
		private int sampleSize;                       // 제품의 크기. (ex 1 300)
		private string tempString;

		private string tempLotEndResultTimeStamp;
		private double resX;
		private double resY;

		private int slotID;

		private int klarfType;

		//private String mesLotID;                        // Lot ID : Product ID가 따로 있을 경우 (Tray/PCB경우 실제 LotID와 Prod.ID가 다른경우가 있음)

		private string recipeName;
		private string tiffSpec;                        // Tiff Spec, 현재 모두 Color로 변환하여 저장. (ex 6.0 G R)
		private string klarfFileName;
														//private String areaPerTest;                      // Area Per Test (사용안함)


		//private int inspectionTest;                      // 검사 회수, ATI 검사 Mode가 1가지라서 1회만 검사하지요.

		//private int sampleTestCnt;                       // 검사한 Die 수량.
		//private int defectDieCnt;                        // 불량 Die 수량.
		//private String sampleTestPlan;                  // 검사한 Die 좌표들.
		//private int tmpSampleTestCnt;                    // 검사한 Die 수량을 임시로 저장해둠. Density 구하기 위함.


		// 210531
		string klarfPath;

		#endregion



		// 210531 New
		public bool LotStart(string klarfPath, InfoWafer infoWafer , RecipeType_WaferMap mapData, GrabModeBase grabMode)
        {
			this.klarfData.Clear();

			if (infoWafer == null)
			{
				this.cassetteID = "cassetteID";
				this.lotID = "lotID";
				this.recipeName = "recipeName";
				this.waferID = "00";
			}
			else
			{
			this.cassetteID = infoWafer.p_sCarrierID;
			this.lotID = infoWafer.p_sLotID;
			this.recipeName = infoWafer.p_sRecipe;
			this.waferID = infoWafer.p_sWaferID;

			string[] idArr = infoWafer.p_sRecipe.Split('.');
			if(idArr.Length == 1)
            {
				this.deviceID = idArr[0];
				this.partID = idArr[0];
				this.stepID = idArr[0];
			}
			else
            {
				this.deviceID = idArr[0];
				this.partID = idArr[0];
				this.stepID = idArr[1];
			}
			}

			this.resX = grabMode.m_dRealResX_um;
			this.resY = grabMode.m_dRealResY_um;

			CalcSampleCenterLoc(mapData);

			this.timeLotStart = DateTime.Now;

			this.klarfPath = klarfPath;

			return true;
		}


		// 이건 쓰지말자...
		public bool LotStart(string _recipeName/*, CRecipeData_ProductSetting* _productInfor*/, RecipeType_WaferMap _mapdata, string _lotID, DateTime _lotStart)
		{
			this.klarfData.Clear();
			SetProductInfo(/*_productInfor*/);

			this.recipeName = _recipeName;
			this.lotID = _lotID;	
			this.deviceID = "deviceID"; //_mapdata.GetDeviceID();

            switch (/*_mapdata.GetFlatZone()*/4)
            {
                //case 1: this.orientationMarkLocation = "LEFT"; break;
                //case 2: this.orientationMarkLocation = "RIGHT"; break;
                //case 3: this.orientationMarkLocation = "UP"; break;
                case 4: this.orientationMarkLocation = "DOWN"; break;
            }

			this.timeResult = _lotStart;

			CalcSampleCenterLoc(_mapdata);

			klarf_FileName = this.recipeName;
			klarf_FileName = klarf_FileName + "_" + this.lotID + this.waferID + ".001";
			MEMMAP_FileName = this.lotID + "-" + this.cassetteID;

			return true;
		}

		public void SetResultTimeStamp()
		{
			timeResultStamp = DateTime.Now;
		}



		public bool WaferStart(RecipeType_WaferMap mapdata, InfoWafer infoWafer)
        {

			this.timeResult = DateTime.Now; 
			this.timeRecipe = DateTime.Now;


			if (infoWafer == null)
				this.slotID = 0;
			else
			this.slotID = infoWafer.m_nSlot;


			this.klarfFileName = this.klarfPath + "\\" + recipeName + "_" + waferID + "_" + lotID + "_" + cassetteID;
			return true;
		}


		// 이거 쓰지말자
		public bool WaferStart(/*CRecipeData_CurrentWFInfor _waferInfor, CRecipeData_ProductSetting* _productInfor, */ RecipeType_WaferMap _mapdata, DateTime _waferStart)
		{
			//klarfData.Clear();
			SetProductInfo(/*_productInfor*/);
			SetWaferInfo(/*_waferInfor*/);

			this.timeResult = DateTime.Now; //_productInfor.m_timeLastUpdate;
			this.timeRecipe = DateTime.Now;//_productInfor.m_timeRecipe;
			
			this.deviceID = "deviceID";//pMapdata->GetDeviceID();
			switch (/*_mapdata.GetFlatZone()*/4)
			{
				//case 1: this.orientationMarkLocation = "LEFT"; break;
				//case 2: this.orientationMarkLocation = "RIGHT"; break;
				//case 3: this.orientationMarkLocation = "UP"; break;
				case 4: this.orientationMarkLocation = "DOWN"; break;
			}
			
			this.slotID = CheckSlotNo();
			
			CalcSampleCenterLoc(_mapdata);

			klarf_FileName = this.recipeName;
			klarf_FileName = klarf_FileName + "_" + this.lotID + this.waferID + ".001";
			MEMMAP_FileName = this.lotID + "-" + this.cassetteID;

			return true;
		}

        // 기존 210302
        public bool AddSlot(RecipeType_WaferMap _mapdata, List<Defect> _defectlist, OriginRecipe _origin, bool useTDIReview = false, bool useVrsReview = false)
        {
            UpdateSampleCenterLocation(_mapdata/*, pRecipe->GetProductSetting()*/);

            KlarfData data = new KlarfData();

            data.SetKlarfType(klarfType);
            data.klarfFileName = this.klarfFileName;

            data.waferID_name = string.Format("{0:2d}", this.slotID);

            //	data.m_nWaferID = AfxGetApp()->GetProfileIntA("ProductSetting", "SlotNum", data.m_nWaferID);  
            //	data.m_nSlot = AfxGetApp()->GetProfileIntA("ProductSetting", "SlotNum", data.m_nSlot); 
            data.partID = this.partID;

            //	m_sLotID = data.m_sLotID = pRecipe->GetCurrentWFInfor()->m_strLotID;    

            data.deviceID = this.deviceID;

            data.sampleOrientationMarkType = this.sampleOrientationMarkType;
            data.orientationMarkLocation = this.orientationMarkLocation;

            data.diePitchX = this.diePitchX;
            data.diePitchY = this.diePitchY;
            data.dieOriginX = this.dieOriginX;
            data.dieOriginY = this.dieOriginY;
            data.sampleCenterLocationX = this.sampleCenterLocationX;
            data.sampleCenterLocationY = this.sampleCenterLocationY;
            data.resolutionX = this.resX;
            data.resolutionX = this.resY;

            data.SetSampleTestPlan(_mapdata);
            data.SetDefectInfor_SRLine(_mapdata, _defectlist, _origin, useTDIReview, useVrsReview);
            //	data.m_nDefectDieCnt = pResultMap->GetBadDieNum();

            data.SetMEMMAP(_mapdata);

            klarfData.Add(data);
            return true;
        }

		public bool AddSlot(RecipeType_WaferMap _mapdata, List<Measurement> _list, OriginRecipe _origin)
		{
			UpdateSampleCenterLocation(_mapdata/*, pRecipe->GetProductSetting()*/);

			KlarfData data = new KlarfData();

			data.SetKlarfType(klarfType);
			data.klarfFileName = this.klarfFileName;

			data.waferID_name = string.Format("{0:2d}", 0/*pMapdata->GetWaferID()*/);

			//	data.m_nWaferID = AfxGetApp()->GetProfileIntA("ProductSetting", "SlotNum", data.m_nWaferID);  
			//	data.m_nSlot = AfxGetApp()->GetProfileIntA("ProductSetting", "SlotNum", data.m_nSlot); 
			data.partID = this.partID;

			//	m_sLotID = data.m_sLotID = pRecipe->GetCurrentWFInfor()->m_strLotID;    

			data.deviceID = this.deviceID;

			data.sampleOrientationMarkType = this.sampleOrientationMarkType;
			data.orientationMarkLocation = this.orientationMarkLocation;

			data.diePitchX = this.diePitchX;
			data.diePitchY = this.diePitchY;
			data.dieOriginX = this.dieOriginX;
			data.dieOriginY = this.dieOriginY;
			data.sampleCenterLocationX = this.sampleCenterLocationX;
			data.sampleCenterLocationY = this.sampleCenterLocationY;
			data.resolutionX = this.resX;
			data.resolutionY = this.resY;

			data.SetSampleTestPlan(_mapdata);
			data.SetDefectInfor_SRLine(_mapdata, _list, _origin);
			//	data.m_nDefectDieCnt = pResultMap->GetBadDieNum();

			data.SetMEMMAP(_mapdata);

			klarfData.Add(data);
			return true;
		}

		//public bool AddSlot(RecipeType_WaferMap _mapdata, List<string> _dataStringList, OriginRecipe _origin)
		//{
		//	UpdateSampleCenterLocation(_mapdata/*, pRecipe->GetProductSetting()*/);

		//	KlarfData data = new KlarfData();

		//	data.SetKlarfType(klarfType);
		//	data.tiffFileName = this.tiffFileName;

		//	data.waferID_name = string.Format("{0:2d}", 0/*pMapdata->GetWaferID()*/);
		//	//	data.m_nWaferID = AfxGetApp()->GetProfileIntA("ProductSetting", "SlotNum", data.m_nWaferID);  
		//	//	data.m_nSlot = AfxGetApp()->GetProfileIntA("ProductSetting", "SlotNum", data.m_nSlot); 
		//	data.partID = this.partID;
		//	//	m_sLotID = data.m_sLotID = pRecipe->GetCurrentWFInfor()->m_strLotID;    
		//	data.deviceID = this.deviceID;
		//	data.sampleOrientationMarkType = this.sampleOrientationMarkType;
		//	data.orientationMarkLocation = this.orientationMarkLocation;
		//	data.diePitchX = this.diePitchX;
		//	data.diePitchY = this.diePitchY;
		//	data.dieOriginX = this.dieOriginX;
		//	data.dieOriginY = this.dieOriginY;
		//	data.sampleCenterLocationX = this.sampleCenterLocationX;
		//	data.sampleCenterLocationY = this.sampleCenterLocationY;
		//	data.resX = this.resX;
		//	data.resY = this.resY;

		//	//data.SetSampleTestPlan(_mapdata); // map data 없는거 예외처리 필요
		//	data.SetDefectInfor_SRLine(_mapdata, _dataStringList, _origin);
		//	//	data.m_nDefectDieCnt = pResultMap->GetBadDieNum();
		//	data.SetMEMMAP(_mapdata);

		//	klarfData.Add(data);
		//	return true;
		//}

		public bool AddSlotToServer(RecipeType_WaferMap _mapdata)
		{
			UpdateSampleCenterLocation(_mapdata/*, pRecipe->GetProductSetting()*/);

			KlarfData data = new KlarfData();
			data.SetKlarfType(klarfType);
			data.klarfFileName = this.klarfFileName;

			data.waferID_name = string.Format("{0:2d}", 0/*pMapdata->GetWaferID()*/); 

		//	data.m_nWaferID = AfxGetApp()->GetProfileIntA("ProductSetting", "SlotNum", data.m_nWaferID);    
		//	data.m_nSlot = AfxGetApp()->GetProfileIntA("ProductSetting", "SlotNum", data.m_nSlot); 
			data.partID = this.partID;

		//	m_sLotID = data.m_sLotID = pRecipe->GetCurrentWFInfor()->m_strLotID; 

			data.deviceID = this.deviceID;
			data.sampleOrientationMarkType = this.sampleOrientationMarkType;
			data.orientationMarkLocation = this.orientationMarkLocation;

			data.diePitchX = this.diePitchX;
			data.diePitchY = this.diePitchY;
			data.dieOriginX = this.dieOriginX;
			data.dieOriginY = this.dieOriginY;
			data.sampleCenterLocationX = this.sampleCenterLocationX;
			data.sampleCenterLocationY = this.sampleCenterLocationY;
			data.resolutionX = this.resX;
			data.resolutionY = this.resY;

			data.SetSampleTestPlan(_mapdata);
		//	data.m_nDefectDieCnt = pResultMap->GetBadDieNum();
		//	data.SetDefectInfor_SRLine(pVSData, m_szChipSize, nMaxImgNum, pRecipe, pVRSMSG, pParam, pCS_DefectVerify);  // 170802 syyun SR Line 껄로 변경
																											//data.SetDCollData_WPLine(pMDM);
			data.SetMEMMAP(_mapdata);

			klarfData.Add(data);

			return true;
		}

		public string GetKlarfFileName(bool bCollector = false)
        {
			if (bCollector)
			{
				tempString = string.Format(lotID + "_{0:d}", slotID);
			}
			else
			{
				tempString = string.Format(recipeName + "_" + waferID + "_" + lotID + "_" + cassetteID);
			}

			tempString.Replace("\\\\", "\\");
			tempString.Replace(".rcp", "");			

			return tempString;
		}

		public bool SaveKlarf(string strFilePath ="", bool bCollector = false)
		{

			if (strFilePath == "") strFilePath = this.klarfPath;

			SetResultTimeStamp();

			timeFile = DateTime.Now;

			if (!Directory.Exists(strFilePath))
				Directory.CreateDirectory(strFilePath);

			if (bCollector)
			{
				tempString = string.Format(strFilePath + "\\" + lotID + "_{0:d}", slotID);
			}
			else
			{
				tempString = string.Format(strFilePath + "\\" + recipeName + "_" + waferID + "_" + lotID + "_" + cassetteID);
			}

			tempString.Replace("\\\\", "\\");
			tempString.Replace(".rcp", "");
			klarfFileName = tempString;
			string klarfFileFullPath = klarfFileName + ".001";

			FileStream fs = new FileStream(klarfFileFullPath, FileMode.Create, FileAccess.Write);
			StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

			if (sw != null)
			{
				SaveHeader(sw);

				for (int i = 0; i < (int)klarfData.Count; i++)
				{
					klarfData[i].SetKlarfType(klarfType);
					klarfData[i].Save(sw);
				}

				sw.Write("EndOfFile;");
				sw.Flush();
				sw.Close();
				fs.Close();
				klarfData.Clear();
			}

			return true;
		}

		public bool CreateLotEnd(string strFilePath = "")
		{
			if (strFilePath == "")
				strFilePath = this.klarfPath;

			tempString = string.Format(strFilePath + "\\LotEnd_" + recipeName + "_" + cassetteID + "_" + "00-" + waferID);

			tempString.Replace(".rcp", "");
			tempString += ".trf";

			SetResultTimeStamp();

			timeFile = DateTime.Now;

			FileStream fs = new FileStream(tempString, FileMode.Create, FileAccess.Write);
			StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

			//m_sTemp = strFilePath + "\\LotEnd_" + m_sRecipeName + "_" + m_sWaferID;	// 기존


			if (sw != null)
			{

				PutFileVersion(sw);
				PutFileTimestamp(sw);
				PutInspectionStationID(sw);
				PutResultTimestampLotEnd(sw);
				PutLotID(sw);
				sw.Write("EndOfLotInspection ;\n");
				sw.Write("EndOfFile;\n");

				sw.Flush();
				sw.Close();
				fs.Close();
			}

			return true;
		}

		//public bool SaveKlarfToServer(string strFilePath, int nError)
		//{
		//	timeFile = DateTime.Now;

		//	FileStream fs = new FileStream(strFilePath, FileMode.Append, FileAccess.Write);
		//	StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

		//	tempString = string.Format(strFilePath + "\\" + recipeName + "_" + cassetteID + "_" + "00-" + waferID);
		//	tempString.Replace(".rcp", "");
		//	klarfFileName = tempString;
		//	tempString += ".001";
			

		//	try
		//	{
		//		if (fs != null)
		//		{
		//			SaveHeader(sw);

		//			for (int i = 0; i < (int)klarfData.Count; i++)
		//			{
		//				klarfData[i].SetKlarfType(klarfType);
		//				klarfData[i].Save(sw);
		//			}
		//			sw.Write("EndOfFile;");
		//			sw.Flush();
		//			sw.Close();
		//			fs.Close();
		//			klarfData.Clear();
		//		}
		//	}
		//	catch (Exception)
		//	{
		//	}

		//	return true;
		//}

		private void SetProductInfo(/*CRecipeData_ProductSetting* _productInfor*/)
		{
			this.inspectionStationVender = "ATI";
			this.inspectionStationModel = "inspectionStationModel";//_productInfor.m_sInspectionStationModel;
			this.inspectionStationMachineID = "inspectionStationMachineID";//pProductInfor->m_sInspectionStationMachineID;

			this.stepID = "stepID"; //_productInfor.m_sStepID;
			this.setupID = "setupID"; //_productInfor.m_sSetupID;
			this.partID = "partID"; //_productInfor.m_sPartID;

			this.sampleType = "sampleType"; //_productInfor.m_sSampleType;

			sampleSize = 5; //_productInfor.m_nSampleSizeX;
			diePitchX = 14; //_productInfor.m_fDiePitchX;
			diePitchY = 14; //_productInfor.m_fDiePitchY; // 이전에 0.998 있었음 *0.998238;//klarfconveter에 참조

			resX = (float)1.5f; //_productInfor.m_fResolutionX;
			resY = (float)1.5f; //_productInfor.m_fResolutionY;

			this.chipSize.Width = (int)(this.diePitchX / 1.5);//_productInfor.m_fResolutionX);
			this.chipSize.Height = (int)(this.diePitchY / 1.5);//_productInfor.m_fResolutionY);
		}

		public void SetResolution(float resolutionX, float resolutionY)
        {
			this.resX = resolutionX;
			this.resY = resolutionY;
        }

		private void SetWaferInfo(/*CRecipeData_CurrentWFInfor _waferInfor*/)
        {
			this.lotID = "lotID";// _waferInfor.m_strLotID;
			this.recipeName = "recipeName"; //_waferInfor.m_strRecipe;

			int waferNo = 0;//_waferInfor.m_nSlotID[0];//atoi(pWaferInfor->m_strWaferID[0]);
			this.waferID = string.Format("{0:d2}", waferNo);

			cassetteID = "cassetteID";//_waferInfor->m_strCassetteID;
		}

		private void CalcSampleCenterLoc(RecipeType_WaferMap _mapdata)
        {
			// Center Die 좌표.
			int centerX = _mapdata.MapSizeX / 2 + _mapdata.MapSizeX % 2;
			int centerY = _mapdata.MapSizeY / 2 + _mapdata.MapSizeY % 2;

			// 실좌표계의 센터.
			double realCenterX = this.diePitchX * _mapdata.MapSizeX / 2;
			double realCenterY = 0.0f;

			if (_mapdata.MapSizeY % 2 == 0)
				realCenterY = this.diePitchY * _mapdata.MapSizeY / 2;
			else
				realCenterY = this.diePitchY * (_mapdata.MapSizeY / 2 + 1) + this.diePitchY / 2;

			// Center Die의 Left Bottom 좌표.
			double CDCenterX = (centerX - 1) * this.diePitchX;
			double CDCenterY = centerY * this.diePitchY;

			// SampleCenterLocation
			this.sampleCenterLocationX = realCenterX - CDCenterX;
			this.sampleCenterLocationY = -(realCenterY - CDCenterY); 
		}

		private bool SaveMEMMAP(String strFilePath)
		{
			tempString = string.Format(strFilePath + "\\" + this.MEMMAP_FileName);

			for (int i = 0; i < (int)klarfData.Count; i++)
				klarfData[i].SaveMEMMAP(tempString);

			return true;
		}

		bool SaveHeader(StreamWriter sw)
		{
			PutFileVersion(sw);
			PutResultTimestamp(sw);
			PutTiffSpec(sw);
			PutInspectionStationID(sw);
			PutSampleType(sw);
			PutFileTimestamp(sw);
			PutLotID(sw);
			PutSampleSize(sw);
			PutDeviceID(sw);
			PutSetupID(sw);
			PutStepID(sw);

			//sw.Flush();
			//sw.Close();

			return true;
		}

		private void PutFileVersion(StreamWriter sw)
		{
			tempString = string.Format("{0} {1};\n", this.fileVer1, this.fileVer2);
			tempString = "FileVersion " + tempString;

			sw.Write(tempString);
		}

		private void PutFileTimestamp(StreamWriter sw)
		{
			this.timeFile = DateTime.Now;
			tempString = string.Format(this.timeFile.ToString("yyyy-MM-dd HH:mm:ss\n"));
			tempString = "FileTimestamp " + tempString;
			sw.Write(tempString);
		}

		private void PutTiffSpec(StreamWriter sw)
		{
			this.tempString = string.Format("TiffSpec " + tiffSpec + "\n");

			sw.Write(tempString);
		}

		private void PutInspectionStationID(StreamWriter sw)
		{
			// 하드코딩, 셋팅 수정하면 될텐데 일단 그냥 둠
			tempString = string.Format("InspectionStationID \""+ this.inspectionStationVender + "\" \""+ this.inspectionStationModel + 
				"\" \""+ this.inspectionStationMachineID + "\";\n");

			sw.Write(tempString);
		}

		private void PutSampleType(StreamWriter sw)
		{
			tempString = string.Format("SampleType " + this.sampleType + ";\n");
			sw.Write(tempString);
		}

		private void PutSampleSize(StreamWriter sw)
		{
			if (this.sampleSize > 170 && this.sampleSize < 230)
				tempString = string.Format("{0:d} {1:d};\n", 1, 200);

			else if (this.sampleSize > 110 && this.sampleSize <= 170)
				tempString = string.Format("{0:d} {1:d};\n", 1, 150);

			else
				tempString = string.Format("{0:d} {1:d};\n", 1, this.sampleSize);

			tempString = "SampleSize " + tempString;
			sw.Write(tempString);
		}

		private void PutResultTimestamp(StreamWriter sw)
		{
			tempString = string.Format(this.timeResultStamp.ToString("yyyy-MM-dd HH:mm:ss;\n"));;
			tempString = "ResultTimestamp " + tempString;
			sw.Write(tempString);
			this.tempLotEndResultTimeStamp = tempString;
		}

		private void PutResultTimestampLotEnd(StreamWriter sw)
		{
			sw.Write(this.tempLotEndResultTimeStamp);
		}

		private void PutLotID(StreamWriter sw)
		{
			tempString = string.Format("LotID {0}{1}-{2}{3};\n", 34, lotID, cassetteID, 34);
			sw.Write(tempString);
		}

		private void PutSetupID(StreamWriter sw)
		{
			tempString = string.Format("SetupID \"" + setupID + "\"" + this.timeResult.ToString("yyyy-MM-dd HH:mm:ss") + ";\n");
			sw.Write(tempString);
		}

		private void PutStepID(StreamWriter sw)
		{
			tempString = string.Format("StepID \""+ stepID + "\";\n");
			sw.Write(tempString);
		}

		private void PutDeviceID(StreamWriter sw)
		{
			tempString = string.Format("DeviceID \""+ deviceID + "\";\n");
			sw.Write(tempString);
		}

		private int CheckSlotNo()
		{
			//String strWaferNo;
			//int nWaferID = -1;
			//strWaferNo = this.tiffFileName;
			//strWaferNo.Replace(".tif", "");
			//strWaferNo = strWaferNo.Right(2);
			//nWaferID = atoi(strWaferNo);
			//if (nWaferID >= 0 && nWaferID <= 25)
			//	return nWaferID;
			return 99;
		}

		private void UpdateSampleCenterLocation(/*CRecipeData_ProductSetting* _productInfor, */ RecipeType_WaferMap _mapdata)
		{
			this.diePitchX = 14;//pProductInfor->m_fDiePitchX;
			this.diePitchY = 14;//pProductInfor->m_fDiePitchY;

			int centerX = _mapdata.MapSizeX / 2 + _mapdata.MapSizeX % 2;
			int centerY = _mapdata.MapSizeY / 2 + _mapdata.MapSizeY % 2;

			double realCenterX = this.diePitchX * _mapdata.MapSizeX / 2;
			double realCenterY = this.diePitchY * _mapdata.MapSizeY / 2;

			// Center Die의 Left Bottom 좌표.
			double CDCenterX = (centerX - 1) * this.diePitchX;
			double CDCenterY = centerY * this.diePitchY;

			// SampleCenterLocation
			this.sampleCenterLocationX = realCenterX - CDCenterX;
			this.sampleCenterLocationY = -(realCenterY - CDCenterY); //_hj : +->- 로 바꿈 칩 개수가 홀수인경우에는 - 여야 함 짝숟인경우에는 어짜피 0:17.12.11
		}

		private object lockTiffObj = new object();
		public void SaveTiffImageOnlyTDI(List<Defect> defectList, SharedBufferInfo sharedBuffer, System.Windows.Size imageSize = default(System.Windows.Size))
		{
			string path = (string)klarfPath.Clone();
			path = Path.Combine(path, this.klarfFileName + ".tif");

			ArrayList inputImage = new ArrayList();

			int tiffWidth = 640;
			int tiffHeight = 480;
			if (imageSize != default(System.Windows.Size))
			{
				tiffWidth = (int)imageSize.Width;
				tiffHeight = (int)imageSize.Height;
			}

			//Parallel.ForEach(defectList, defect =>
			foreach (Defect defect in defectList)
			{
				Rect defectRect = new Rect(
					defect.m_fAbsX - tiffWidth / 2,
					defect.m_fAbsY - tiffHeight / 2,
					tiffWidth,
					tiffHeight);

				MemoryStream image = new MemoryStream();
				System.Drawing.Bitmap bitmap = Tools.CovertBufferToBitmap(sharedBuffer, defectRect, tiffWidth, tiffHeight);
				Tools.DrawBitmapRect(ref bitmap, (tiffWidth / 2) -  (defect.m_fWidth / 2), (tiffHeight / 2) - (defect.m_fHeight / 2), defect.m_fWidth, defect.m_fHeight, Tools.PenColor.RED);
				Tools.DrawRuler(ref bitmap, tiffWidth, tiffHeight, (float)resX);
				//System.Drawing.Bitmap NewImg = new System.Drawing.Bitmap(bitmap);
				bitmap.Save(image, ImageFormat.Tiff);
				inputImage.Add(image);
			}

			ImageCodecInfo info = null;
			foreach (ImageCodecInfo ice in ImageCodecInfo.GetImageEncoders())
			{
				if (ice.MimeType == "image/tiff")
				{
					info = ice;
					break;
				}
			}

			EncoderParameters ep = new EncoderParameters(2);

			bool firstPage = true;

			System.Drawing.Image img = null;
			lock (lockTiffObj)
			{
				for (int i = 0; i < inputImage.Count; i++)
				{
					System.Drawing.Image img_src = System.Drawing.Image.FromStream((Stream)inputImage[i]);
					Guid guid = img_src.FrameDimensionsList[0];
					System.Drawing.Imaging.FrameDimension dimension = new System.Drawing.Imaging.FrameDimension(guid);

					for (int nLoopFrame = 0; nLoopFrame < img_src.GetFrameCount(dimension); nLoopFrame++)
					{
						img_src.SelectActiveFrame(dimension, nLoopFrame);

						ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, Convert.ToInt32(EncoderValue.CompressionLZW));

						if (firstPage)
						{
							img = img_src;

							ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.MultiFrame));
							img.Save(path, info, ep);

							firstPage = false;
							continue;
						}

						ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.FrameDimensionPage));
						lock (lockTiffObj) img.SaveAdd(img_src, ep);
					}
				}
				if (inputImage.Count == 0)
				{
					File.Create(path);
					return;
				}

				ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.Flush));
				img.SaveAdd(ep);
			}
		}


		public void SaveTiffImageOnlyVRS(List<Defect> defectList, ConcurrentQueue<byte[]> vrsImageQueue, System.Windows.Size vrsImageSize)
		{
			string path = (string)this.klarfPath.Clone();
			path = Path.Combine(path, this.klarfFileName + ".tif");

			ArrayList inputImage = new ArrayList();

			if (vrsImageQueue == null)
			{
				MessageBox.Show("VRS Image == Null");
				return;
			}

			if ((vrsImageQueue.Count != defectList.Count) || vrsImageSize == default(System.Windows.Size))
			{
				MessageBox.Show("VRS Review Image와 Defect의 수가 다릅니다.");
				return;
			}

			if (vrsImageSize == default(System.Windows.Size))
			{
				MessageBox.Show("VRS Review Image Size를 설정해주어야합니다.");
				return;
			}

			//Parallel.ForEach(defectList, defect =>
			foreach (Defect defect in defectList)  // 이거 나중에 정보 필요할수 있음
			{
				byte[] colorImage = null;
				if (vrsImageQueue.TryDequeue(out colorImage) == true)
				{
					MemoryStream ms = new MemoryStream();
					System.Drawing.Bitmap vrsBmp = Tools.CovertArrayToBitmap(colorImage, (int)vrsImageSize.Width, (int)vrsImageSize.Height, 3);

					vrsBmp.Save(ms, ImageFormat.Tiff);
					inputImage.Add(ms);
				}
				else
				{
					TempLogger.Write("Error", "Save Klarf image - VRS Image Dequeue Fail!!");
				}
			}

			ImageCodecInfo info = null;
			foreach (ImageCodecInfo ice in ImageCodecInfo.GetImageEncoders())
			{
				if (ice.MimeType == "image/tiff")
				{
					info = ice;
					break;
				}
			}

			EncoderParameters ep = new EncoderParameters(2);

			bool firstPage = true;

			System.Drawing.Image img = null;
			lock (lockTiffObj)
			{
				for (int i = 0; i < inputImage.Count; i++)
				{
					System.Drawing.Image img_src = System.Drawing.Image.FromStream((Stream)inputImage[i]);
					Guid guid = img_src.FrameDimensionsList[0];
					System.Drawing.Imaging.FrameDimension dimension = new System.Drawing.Imaging.FrameDimension(guid);

					for (int nLoopFrame = 0; nLoopFrame < img_src.GetFrameCount(dimension); nLoopFrame++)
					{
						img_src.SelectActiveFrame(dimension, nLoopFrame);

						ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, Convert.ToInt32(EncoderValue.CompressionLZW));

						if (firstPage)
						{
							img = img_src;

							ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.MultiFrame));
							img.Save(path, info, ep);

							firstPage = false;
							continue;
						}

						ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.FrameDimensionPage));
						img.SaveAdd(img_src, ep);
					}
				}
				if (inputImage.Count == 0)
				{
					File.Create(path);
					return;
				}

				ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.Flush));
				img.SaveAdd(ep);
			}
		}

		public void SaveTiffImageBoth(List<Defect> defectList, SharedBufferInfo sharedBuffer, System.Windows.Size imageSize, ConcurrentQueue<byte[]> vrsImageQueue, System.Windows.Size vrsImageSize)
		{
			string path = (string)this.klarfPath.Clone();
			path = Path.Combine(path, this.klarfFileName + ".tif");

			ArrayList inputImage = new ArrayList();

			int tiffWidth = (int)imageSize.Width;
			int tiffHeight = (int)imageSize.Height;

			if (vrsImageQueue == null)
			{
				MessageBox.Show("VRS Imaage Queue == null");
				return;
			}

			if ((vrsImageQueue.Count != defectList.Count) || vrsImageSize == default(System.Windows.Size))
			{
				MessageBox.Show("VRS Review Image와 Defect의 수가 다릅니다.");
				return;
			}

			if (vrsImageSize == default(System.Windows.Size))
			{
				MessageBox.Show("VRS Review Image Size를 설정해주어야합니다.");
				return;
			}

			//Parallel.ForEach(defectList, defect =>
			foreach (Defect defect in defectList)
			{
				Rect defectRect = new Rect(
					defect.m_fAbsX - tiffWidth / 2,
					defect.m_fAbsY - tiffHeight / 2,
					tiffWidth,
					tiffHeight);

				MemoryStream image = new MemoryStream();
				System.Drawing.Bitmap bitmap = Tools.CovertBufferToBitmap(sharedBuffer, defectRect);
				//System.Drawing.Bitmap NewImg = new System.Drawing.Bitmap(bitmap);
				bitmap.Save(image, ImageFormat.Tiff);
				inputImage.Add(image);


				byte[] colorImage = null;
				if (vrsImageQueue.TryDequeue(out colorImage) == true)
				{
					MemoryStream ms = new MemoryStream();
					System.Drawing.Bitmap vrsBmp = Tools.CovertArrayToBitmap(colorImage, (int)vrsImageSize.Width, (int)vrsImageSize.Height, 3);

					vrsBmp.Save(ms, ImageFormat.Tiff);
					inputImage.Add(ms);
				}
				else
				{
					TempLogger.Write("Error", "Save Klarf image - VRS Image Dequeue Fail!!");
				}
			}

			ImageCodecInfo info = null;
			foreach (ImageCodecInfo ice in ImageCodecInfo.GetImageEncoders())
			{
				if (ice.MimeType == "image/tiff")
				{
					info = ice;
					break;
				}
			}

			EncoderParameters ep = new EncoderParameters(2);

			bool firstPage = true;

			System.Drawing.Image img = null;
			lock (lockTiffObj)
			{
				for (int i = 0; i < inputImage.Count; i++)
				{
					System.Drawing.Image img_src = System.Drawing.Image.FromStream((Stream)inputImage[i]);
					Guid guid = img_src.FrameDimensionsList[0];
					System.Drawing.Imaging.FrameDimension dimension = new System.Drawing.Imaging.FrameDimension(guid);

					for (int nLoopFrame = 0; nLoopFrame < img_src.GetFrameCount(dimension); nLoopFrame++)
					{
						img_src.SelectActiveFrame(dimension, nLoopFrame);

						ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, Convert.ToInt32(EncoderValue.CompressionLZW));

						if (firstPage)
						{
							img = img_src;

							ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.MultiFrame));
							img.Save(path, info, ep);

							firstPage = false;
							continue;
						}

						ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.FrameDimensionPage));
						img.SaveAdd(img_src, ep);
					}
				}
				if (inputImage.Count == 0)
				{
					File.Create(path);
					return;
				}

				ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.Flush));
				img.SaveAdd(ep);
			}

		}

		public void SaveTiffImageFromFiles(string defectImagePath)
		{
			string savePath = (string)this.klarfPath.Clone();
			savePath = Path.Combine(savePath, this.klarfFileName + ".tif");

			//savePath += "\\";
			//DirectoryInfo di = new DirectoryInfo(savePath);
			//if (!di.Exists)
			//	di.Create();

			DirectoryInfo di2 = new DirectoryInfo(defectImagePath);
			if (!di2.Exists)
				return;

			ImageCodecInfo info = null;
			info = (from ie in ImageCodecInfo.GetImageEncoders()
					where ie.MimeType == "image/tiff"
					select ie).FirstOrDefault();


			EncoderParameters ep = new EncoderParameters(2);
			bool firstPage = true;

			//var test = di2.GetFiles().OrderBy(f => f.Name);
			FileInfo[] files = di2.GetFiles();
			Array.Sort<FileInfo>(files, CompareByNumericName);

			ArrayList inputImage = new ArrayList();
			foreach (FileInfo file in files)
			{
				if (file.Extension != ".bmp" && file.Extension != ".jpg")
					continue;

				System.Drawing.Image imgFromFile = System.Drawing.Bitmap.FromFile(file.FullName);		
				inputImage.Add(imgFromFile);
			}

			System.Drawing.Image img = null;
			for (int i = 0; i < inputImage.Count; i++)
			{
				System.Drawing.Image img_src = (System.Drawing.Image)inputImage[i];
				Guid guid = img_src.FrameDimensionsList[0];
				System.Drawing.Imaging.FrameDimension dimension = new System.Drawing.Imaging.FrameDimension(guid);

				for (int nLoopFrame = 0; nLoopFrame < img_src.GetFrameCount(dimension); nLoopFrame++)
				{
					img_src.SelectActiveFrame(dimension, nLoopFrame);

					ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, Convert.ToInt32(EncoderValue.CompressionLZW));

					if (firstPage)
					{
						img = img_src;

						ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.MultiFrame));
						lock (lockTiffObj) img.Save(savePath, info, ep);

						firstPage = false;
						continue;
					}

					ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.FrameDimensionPage));
					lock (lockTiffObj) img.SaveAdd(img_src, ep);
				}
			}
			if (inputImage.Count == 0)
			{
				lock (lockTiffObj) File.Create(savePath);
				return;
			}

			ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.Flush));
			lock (lockTiffObj) img.SaveAdd(ep);
		}

		private int CompareByNumericName(FileInfo firstFile, FileInfo secondFile)
		{
			int firstFileNumericName = Int32.Parse(Path.GetFileNameWithoutExtension(firstFile.Name));
			int secondFileNumericName = Int32.Parse(Path.GetFileNameWithoutExtension(secondFile.Name));

			return firstFileNumericName.CompareTo(secondFileNumericName);
		}

		public bool SaveImageJpg(SharedBufferInfo info, Rect rect, long compressRatio, int outSizeX, int outSizeY)
		{
			Bitmap bmp = Tools.CovertBufferToBitmap(info, rect, outSizeX, outSizeY);

			Tools.SaveImageJpg(bmp, this.klarfFileName + ".jpg", compressRatio);

			return true;
		}
	}
}
