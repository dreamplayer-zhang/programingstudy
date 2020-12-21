using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision.Utility
{
	class KlarfData
	{
		public KlarfData()
		{
			//m_sTiffFileName = "";
			waferID_name = "";
			klarfType = 0;
		}
		#region [Variables]
		public int klarfType;

		public int waferID_num;
		public int slot;                                // Hynix는 WaferID와 동일하게 사용.// WaferID 0~25
		public String partID;                          // Part ID
		public String lotID;                           // Lot ID
		public String deviceID;                        // Device ID
		public String waferID_name;
		public String sampleOrientationMarkType;       // FLAT or NOTCH
		public String orientationMarkLocation;         // Flat zone 방향 // UP, DOWN, LEFT, RIGHT
		public String tiffFileName;                    // Tiff file 명

		public double diePitchX, diePitchY;          // Die Pitch
		public double dieOriginX, dieOriginY;        // 센터 기준. 무조건 0, 0 으로 보고 셋팅
		public double sampleCenterLocationX;            // 센터 Die의 Left,bottom와 실제 제품 Center간의 차이.
		public double sampleCenterLocationY;            // 센터 Die의 Left,bottom와 실제 제품 Center간의 차이.

		public String sampleTestPlan;
		public String defectInfor;
		public String DCollData;
		public String MEMMAP;

		public String areaPerTest;
		public int sampleTestCnt;
		public int defectDieCnt;
		public int centerX;
		public int centerY;

		public int klarfRow;
		public int klarfCol;

		public String tempString;
		public float resX;
		public float resY;
		#endregion

		public bool Save(StreamWriter sw)
        {		
			PutSampleOrientationMarkTypeNotch(sw);
			PutOrientationMarkLocation(sw);
			PutTiffFileName(sw);
			PutDiePitch(sw);
			PutDieOrigin(sw);
			PutWaferID(sw);
			PutSlot(sw);
			PutSampleCenterLocation(sw);

			//Put_OrientationInstruction(FILE);	
			//Put_CoordinatesMirroredYES(FILE);	
			//PutClassLookup(sw);
			PutInspectionTest(sw);
			PutSampleTestPlan(sw);
			PutTiffFileName(sw); // 170802 syyun 추가
			PutDefectInfor(sw);
			PutDCollData(sw);

			return true;
        }
		public bool SaveMEMMAP(String strFilePath)
		{
			tempString = string.Format(".P{0:2d}", slot);
			strFilePath += tempString;

			FileStream fs = new FileStream(strFilePath, FileMode.Append, FileAccess.Write);
			StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

			if (sw != null)
			{
				sw.Write(MEMMAP);
				sw.Close();
			}

			return true;
		}

		public void SetKlarfType(int mode)
		{
			klarfType = mode;
		}

		private void PutWaferID(StreamWriter sw)
		{
			// @ 없네
			//	m_sTemp.Format("WaferID %c%02d%c;\n", 34, m_nWaferID, 34);

			if (klarfType == 1)
			{
				tempString = string.Format("WaferID \"" + waferID_name + ".{0:D2}\";\n", waferID_num);
			}
			else
			{
				tempString = string.Format("WaferID \"{0:D2}\";\n", waferID_num);
			}
			sw.Write(tempString);
		}
		private void PutSlot(StreamWriter sw)
		{
			if (slot < 9)
				tempString = string.Format("Slot 0{0:d};\n", slot); // ATI: 0 base, 미래로: 1 base
			else
				tempString = string.Format("Slot {0:d};\n", slot); // ATI: 0 base, 미래로: 1 base

			sw.Write(tempString);
		}
		private void PutSampleOrientationMarkTypeNotch(StreamWriter sw)
		{
			tempString = string.Format("SampleOrientationMarkType NOTCH;\n");
			sw.Write(tempString);
		}

		private void PutOrientationMarkLocation(StreamWriter sw)
		{
			tempString = string.Format("OrientationMarkLocation " +  orientationMarkLocation + ";\n");
			sw.Write(tempString);
		}

		private void PutOrientationInstruction(StreamWriter sw)
		{
			tempString = string.Format("OrientationInstruction " + orientationMarkLocation + ";\n");
			sw.Write(tempString);
		}
		private void PutSampleCenterLocation(StreamWriter sw)
		{
			// 다른 장비와 맞쳐주기 위함.
			if (klarfRow == 90 && klarfCol == 90)
			{ // 4inch
				sampleCenterLocationX = 5.000000e+004;
				sampleCenterLocationY = 5.000000e+004;
			}
			else if (klarfRow == 70 && klarfCol == 70)
			{ // 6inch
				sampleCenterLocationX = 7.500000e+004;
				sampleCenterLocationY = 7.500000e+004;
			}

			if (klarfType == 1)
			{
				tempString = string.Format("SampleCenterLocation {0:f} 0.000000;\n", sampleCenterLocationX);    // 170802 syyun SampleCenterLocation Y를 0으로 하드코딩
			}
			else
			{
				tempString = string.Format("SampleCenterLocation {0:e} {1:e};\n", sampleCenterLocationX, sampleCenterLocationY);    // 기존
			}
			sw.Write(tempString);
		}
		private void PutDiePitch(StreamWriter sw)
		{
			if (klarfRow == 90 && klarfCol == 90)
			{ // 4inch
				diePitchX = 1.000000e+005;
				diePitchY = 1.000000e+005;
			}
			else if (klarfRow == 70 && klarfCol == 70)
			{ // 6inch
				diePitchX = 1.500000e+005;
				diePitchY = 1.500000e+005;
			}

			if (klarfType == 1)
			{
				tempString = string.Format("DiePitch {0:f} {1:f};\n", diePitchX, diePitchY);
			}
			else
			{
				tempString = string.Format("DiePitch {0:e} {1:e};\n", diePitchX, diePitchY);
			}
			sw.Write(tempString);
		}

		private void PutCoordinatesMirroredYES(StreamWriter sw)
		{
			tempString = string.Format("CoordinatesMirrored YES;\n");
			sw.Write(tempString);
		}

		private void PutDieOrigin(StreamWriter sw)
		{
			if (klarfType == 1)
			{
				tempString = string.Format("DieOrigin {0:f} {1:f};\n", dieOriginX, dieOriginY);
			}
			else
			{
				tempString = string.Format("DieOrigin {0:e} {1:e};\n", dieOriginX, dieOriginY);
			}
			sw.Write(tempString);
		}

		private void PutClassLookup(StreamWriter sw)
		{
			// 하드코딩
			tempString = string.Format("ClassLookup 2\n");
			sw.Write(tempString);
			tempString = string.Format("0 \"No Revision\"\n");
			sw.Write(tempString);
			tempString = string.Format("1 \"Good\";\n");
			sw.Write(tempString);
		}
		private void PutInspectionTest(StreamWriter sw)
		{
			tempString = string.Format("InspectionTest 1;\n");
			sw.Write(tempString);
		}

		private void PutSampleTestPlan(StreamWriter sw)
		{
			sw.Write(sampleTestPlan);
			sw.Write(areaPerTest); 
		}

		private void PutDefectInfor(StreamWriter sw)
		{
			sw.Write(defectInfor);
		}

		private void PutDCollData(StreamWriter sw)
		{
			sw.Write(DCollData);
		}

		private void PutTiffFileName(StreamWriter sw)
		{
			tempString = string.Format("TiffFilename " + tiffFileName + ";\n");
			sw.Write(tempString);
		}
		public void SetMEMMAP(RecipeType_WaferMap _mapdata)
		{
			tempString = string.Format("[P=" + partID + " ");        // Part ID : 현재 없음
			this.MEMMAP = tempString;
			tempString = string.Format("L="+ lotID +" ");      // Lot ID
			this.MEMMAP += tempString;
			tempString = string.Format("W={0:2d} ", slot);     // Slot No.
			this.MEMMAP += tempString;
			tempString = string.Format("O=00000000 ");          // Operator ID (Static)
			this.MEMMAP += tempString;
			tempString = string.Format("F=5 ");                       // Flat Zone (Static)
			this.MEMMAP += tempString;
			tempString = string.Format("D=" + deviceID + " ");   // Device ID (Setup ID)
			this.MEMMAP += tempString;
			tempString = string.Format("V=20]\n");              // Version (Static)
			this.MEMMAP += tempString;


			int[] p = _mapdata.Data;
			int nDATASStartX = 0;
			int nDATASStartY = 30;
			int cnt = 0;
			for (int j = 0; j < _mapdata.MapSizeX; j++, cnt++)
			{
				if (p[cnt] == 1)
				{
					nDATASStartX = 30 - j;
					break;
				}
			}

			cnt = 0;
			for (int i = 0; i < _mapdata.MapSizeY; i++)
			{
				for (int j = 0; j < _mapdata.MapSizeX; j++, cnt++)
				{
					if (true/*p[cnt] == WAFER_MAP_CHIP_DEFECT*/)
					{
						tempString = string.Format("X= {0:4d} Y= {1:4d} B= 91\n", j + nDATASStartX, i + nDATASStartY);
						MEMMAP += tempString;
					}
					else if (false/*p[cnt] != WAFER_MAP_CHIP_NOCHIP && p[cnt] != WAFER_MAP_CHIP_FLATZONE*/)
					{
						tempString = string.Format("X= {0:4d} Y= {1:4d} B= 01\n", j + nDATASStartX, i + nDATASStartY);
						MEMMAP += tempString;
					}
				}
			}

			tempString = string.Format("E= EOW\n");
			MEMMAP += tempString;
		}

		PointF GetDefectPos(int DCode, int inspMode, int posX, int posY, Point ptStart, int chipWidth, int chipHeight)
		{
			Point pt = new Point(posX, posY);
			PointF ptf = new PointF();
			ptf.X = (float)(pt.X - ptStart.X);
			if (ptf.X != 0) ptf.X /= chipWidth;
			ptf.Y = (float)(pt.Y - ptStart.Y);
			if (ptf.Y != 0) ptf.Y /= chipHeight;


			// 0~1안 체크

			if (ptf.X < 0.0f) ptf.X = 0.0f;
			if (ptf.X > 1.0f) ptf.X = 1.0f;
			if (ptf.Y < 0.0f) ptf.Y = 0.0f;
			if (ptf.Y > 1.0f) ptf.Y = 1.0f;

			return ptf;
		}
	}
}
