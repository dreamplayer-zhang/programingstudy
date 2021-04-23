#pragma once
#include "..\RootTools_Cpp\\PitSizer.h"
#include "windows.h"
#include "memoryapi.h"
#include <stdio.h>
#include <list>
#include "CLR_InspConnector.h"
#include "..\RootTools_Cpp\\InspectionSurface.h"
#include "..\RootTools_Cpp\\InspectionReticle.h"
#include "DefectData.h"
#include "CDefectDataWrapper.h"

#include <vcclr.h> // for PtrToStringChars 
#include <stdio.h> // for wprintf
#include <msclr\marshal_cppstd.h>
#include <cliext/vector>

#include "MySQLDBConnector.h"

#pragma warning(disable: 4267)

namespace RootTools_CLR
{
	public ref class CLR_Inspection
	{
	protected:
		CLR_InspConnector* m_InspConn = nullptr;
		CInspectionSurface* pInspSurface = nullptr;
		CInspectionReticle* pInspReticle = nullptr;
	public:
		CLR_Inspection(int nThreadNum, int nROIWidth, int nROIHeight)
		{
			m_InspConn = new CLR_InspConnector(nThreadNum);
			pInspSurface = new CInspectionSurface(nROIWidth, nROIHeight);
			pInspReticle = new CInspectionReticle(nROIWidth, nROIHeight);
		}

		virtual ~CLR_Inspection()
		{
			delete m_InspConn;
			delete pInspSurface;
			delete pInspReticle;
		}

		array<CDefectDataWrapper^>^ SurfaceInspection(System::String^ poolName, System::String^ groupName, System::String^ memoryName, unsigned __int64  memOffset, int threadindex, int nDefectCode, int RoiLeft, int RoiTop, int RoiRight, int RoiBottom, int  memwidth, int  memHeight, int GV, int DefectSize, bool bDark, bool bAbsolute, bool bMerge, int nMergeDistance, void* ptrMemory)
		{
			RECT targetRect;
			std::vector<DefectDataStruct> vTempResult;

			targetRect.left = RoiLeft;
			targetRect.right = RoiRight;
			targetRect.top = RoiTop;
			targetRect.bottom = RoiBottom;

			msclr::interop::marshal_context context;
			std::string standardString = context.marshal_as<std::string>(poolName);

			//byte* buffer = m_InspConn->GetImagePool(standardString, memOffset, memwidth, memHeight);
			byte* buffer = (byte*)ptrMemory;
			if (buffer != NULL)
			{
				int bufferwidth = memwidth;
				int bufferheight = memHeight;

				pInspSurface->SetParams(buffer, bufferwidth, bufferheight, targetRect, 1, GV, DefectSize, bDark, threadindex);

				//TODO 여기서 이벤트를 올리는 방식으로 변경한다
				//여기 들어올때 이미 한 블럭에 대한 정보가 통째로 넘어오는 것이므로 구조 자체를 변경하여 AddDefect이 발생하는 순간을 여기서 포착하도록 수정한다
				//pInspSurface->Inspection();

				pInspSurface->CheckConditions();

				pInspSurface->CopyImageToBuffer(bDark);//opencv pitsize 가져오기 전까지는 buffer copy가 필요함
				vTempResult = pInspSurface->SurfaceInspection(bAbsolute, nDefectCode);//TODO : absolute GV 구현해야함
				//vTempResult를 Merge해서 다시 정렬해야함

				bool bResultExist = vTempResult.size() > 0;
				cliext::vector<CDefectDataWrapper^>^ local = gcnew cliext::vector<CDefectDataWrapper^>(vTempResult.size());
				cliext::vector<CDefectDataWrapper^>^ cache = gcnew cliext::vector<CDefectDataWrapper^>();
				if (bMerge)
				{
					for (int i = 0; i < vTempResult.size(); i++)
					{
						local[i] = gcnew CDefectDataWrapper();
						local[i]->nIdx = vTempResult[i].nIdx;
						local[i]->nClassifyCode = nDefectCode;//vTempResult[i].nClassifyCode;
						local[i]->fAreaSize = vTempResult[i].fAreaSize;
						local[i]->nLength = vTempResult[i].nLength;
						local[i]->nWidth = vTempResult[i].nWidth;
						local[i]->nHeight = vTempResult[i].nHeight;
						local[i]->nGV = vTempResult[i].GV;
						local[i]->nFOV = vTempResult[i].nFOV;
						local[i]->fPosX = vTempResult[i].fPosX + targetRect.left;//데이터를 던져주기 직전에 rect의 top/left 정보를 더해서 던져준다
						local[i]->fPosY = vTempResult[i].fPosY + targetRect.top;//데이터를 던져주기 직전에 rect의 top/left 정보를 더해서 던져준다
					}
					local = CDefectDataWrapper::MergeDefect(local, nMergeDistance);
					cache->assign(local->begin(), local->end());

					local->clear();
					local->assign(cache->begin(), cache->end());
;				}


				if (bResultExist)
				{
					MySQLDBConnector^ connector = gcnew MySQLDBConnector();
					unsigned int errorCode = connector->OpenDatabase();

					int count = vTempResult.size();
					if (bMerge)
						count = local->size();

					for (int i = 0; i < count; i++)
					{
						if (!bMerge)
						{
							local[i] = gcnew CDefectDataWrapper();
							local[i]->nIdx = vTempResult[i].nIdx;
							local[i]->nClassifyCode = nDefectCode;//vTempResult[i].nClassifyCode;
							local[i]->fAreaSize = vTempResult[i].fAreaSize;
							local[i]->nLength = vTempResult[i].nLength;
							local[i]->nWidth = vTempResult[i].nWidth;
							local[i]->nHeight = vTempResult[i].nHeight;
							local[i]->nFOV = vTempResult[i].nFOV;
							local[i]->nGV = vTempResult[i].GV;
							local[i]->fPosX = vTempResult[i].fPosX + targetRect.left;//데이터를 던져주기 직전에 rect의 top/left 정보를 더해서 던져준다
							local[i]->fPosY = vTempResult[i].fPosY + targetRect.top;//데이터를 던져주기 직전에 rect의 top/left 정보를 더해서 던져준다
						}

						if (errorCode == 0)
						{
							//DB Open성공

							System::String^ query;
							query = query->Format("INSERT INTO tempdata (ClassifyCode, AreaSize, Length, Width, Height, FOV, PosX, PosY, memPOOL, memGROUP, memMEMORY, GV) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}');",
								local[i]->nClassifyCode, local[i]->fAreaSize, local[i]->nLength, local[i]->nWidth, local[i]->nHeight, local[i]->nFOV, local[i]->fPosX, local[i]->fPosY,
								poolName, groupName, memoryName, local[i]->nGV);

							errorCode = connector->RunQuery(query);

							if (errorCode == 0)
							{
								//success
							}
							else if (errorCode == 1146)
							{
								//table이 없음
								//table생성 후 재시도
								query = query->Format("CREATE TABLE tempdata(idx INT NOT NULL AUTO_INCREMENT, ClassifyCode INT NULL, AreaSize DOUBLE NULL,  GV INT NULL,  Length INT NULL,  Width INT NULL, Height INT NULL, FOV INT NULL, PosX DOUBLE NULL, PosY DOUBLE NULL, memPOOL longtext DEFAULT NULL, memGROUP longtext DEFAULT NULL, memMEMORY longtext DEFAULT NULL, PRIMARY KEY (idx), UNIQUE INDEX idx_UNIQUE (idx ASC) VISIBLE);");
								errorCode = connector->RunQuery(query);
								if (errorCode == 0)
								{
									//insert재실행
									query = query->Format("INSERT INTO tempdata (ClassifyCode, AreaSize, Length, Width, Height, FOV, PosX, PosY, memPOOL, memGROUP, memMEMORY, GV) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}');",
										local[i]->nClassifyCode, local[i]->fAreaSize, local[i]->nLength, local[i]->nWidth, local[i]->nHeight, local[i]->nFOV, local[i]->fPosX, local[i]->fPosY,
										poolName, groupName, memoryName, local[i]->nGV);

									errorCode = connector->RunQuery(query);
									if (errorCode != 0)
									{
										//예외처리 진행. 될때까지 업로드 시도 시키는게 좋을것으로 보임
									}
								}
								else
								{
									//예외처리 진행. 될때까지 업로드 시도 시키는게 좋을것으로 보임
								}
							}
							else
							{
								//예외처리 진행. 될때까지 업로드 시도 시키는게 좋을것으로 보임
							}
						}
						else
						{
							//DBOpen 실패
						}
					}
				}
				array<CDefectDataWrapper^>^ arrLocal = gcnew array<CDefectDataWrapper^>(local->size());
				for (int i = 0; i < local->size(); i++)
				{
					arrLocal[i] = local[i];
				}

				return arrLocal;
			}
			else
			{
				array<CDefectDataWrapper^>^ local = gcnew array<CDefectDataWrapper^>(0);
				return local;
			}
		}
		array<CDefectDataWrapper^>^ StripInspection(System::String^ poolName, System::String^ groupName, System::String^ memoryName, unsigned __int64 memOffset, int threadindex, int nDefectCode, int RoiLeft, int RoiTop, int RoiRight, int RoiBottom, int  memwidth, int  memHeight, int GV, int DefectSize, int nIntensity, int nBandwidth, bool bMerge, int nMergeDistance, void *ptrMemory)
		{
			RECT targetRect;
			std::vector<DefectDataStruct> vTempResult;

			targetRect.left = RoiLeft;
			targetRect.right = RoiRight;
			targetRect.top = RoiTop;
			targetRect.bottom = RoiBottom;

			msclr::interop::marshal_context context;
			std::string standardString = context.marshal_as<std::string>(poolName);

			//byte* buffer = m_InspConn->GetImagePool(standardString, memOffset, memwidth, memHeight);//TODO 수정필요
			byte* buffer = (byte*)ptrMemory;
			int bufferwidth = memwidth;
			int bufferheight = memHeight;

			pInspReticle->SetParams(buffer, bufferwidth, bufferheight, targetRect, 1, threadindex, GV, DefectSize);
			pInspReticle->CheckConditions();

			pInspReticle->CopyImageToBuffer(true);//opencv pitsize 가져오기 전까지는 buffer copy가 필요함
			vTempResult = pInspReticle->StripInspection(nBandwidth, nIntensity, nDefectCode);
			//vTempResult를 Merge해서 다시 정렬해야함

			bool bResultExist = vTempResult.size() > 0;
			cliext::vector<CDefectDataWrapper^>^ local = gcnew cliext::vector<CDefectDataWrapper^>(vTempResult.size());
			if (bMerge)
			{
				for (int i = 0; i < vTempResult.size(); i++)
				{
					local[i] = gcnew CDefectDataWrapper();
					local[i]->nIdx = vTempResult[i].nIdx;
					local[i]->nClassifyCode = nDefectCode;//vTempResult[i].nClassifyCode;
					local[i]->fAreaSize = vTempResult[i].fAreaSize;
					local[i]->nLength = vTempResult[i].nLength;
					local[i]->nWidth = vTempResult[i].nWidth;
					local[i]->nHeight = vTempResult[i].nHeight;
					local[i]->nFOV = vTempResult[i].nFOV;
					local[i]->fPosX = vTempResult[i].fPosX + targetRect.left;//데이터를 던져주기 직전에 rect의 top/left 정보를 더해서 던져준다
					local[i]->fPosY = vTempResult[i].fPosY + targetRect.top;//데이터를 던져주기 직전에 rect의 top/left 정보를 더해서 던져준다
				}
				local = CDefectDataWrapper::MergeDefect(local, nMergeDistance);
			}

			if (bResultExist)
			{
				MySQLDBConnector^ connector = gcnew MySQLDBConnector();
				unsigned int errorCode = connector->OpenDatabase();


				int count = vTempResult.size();
				if (bMerge)
					count = local->size();

				for (int i = 0; i < count; i++)
				{
					if (!bMerge)
					{
						local[i] = gcnew CDefectDataWrapper();
						local[i]->nIdx = vTempResult[i].nIdx;
						local[i]->nClassifyCode = nDefectCode;//vTempResult[i].nClassifyCode;
						local[i]->fAreaSize = vTempResult[i].fAreaSize;
						local[i]->nLength = vTempResult[i].nLength;
						local[i]->nWidth = vTempResult[i].nWidth;
						local[i]->nHeight = vTempResult[i].nHeight;
						local[i]->nFOV = vTempResult[i].nFOV;
						local[i]->fPosX = vTempResult[i].fPosX + targetRect.left;//데이터를 던져주기 직전에 rect의 top/left 정보를 더해서 던져준다
						local[i]->fPosY = vTempResult[i].fPosY + targetRect.top;//데이터를 던져주기 직전에 rect의 top/left 정보를 더해서 던져준다
					}
					if (errorCode == 0)
					{
						//DB Open성공

						System::String^ query;
						query = query->Format("INSERT INTO tempdata (ClassifyCode, AreaSize, Length, Width, Height, FOV, PosX, PosY, memPOOL, memGROUP, memMEMORY) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}');",
							local[i]->nClassifyCode, local[i]->fAreaSize, local[i]->nLength, local[i]->nWidth, local[i]->nHeight, local[i]->nFOV, local[i]->fPosX, local[i]->fPosY,
							poolName, groupName, memoryName);

						errorCode = connector->RunQuery(query);

						if (errorCode == 0)
						{
							//success
						}
						else if (errorCode == 1146)
						{
							//table이 없음
							//table생성 후 재시도
							query = query->Format("CREATE TABLE tempdata(idx INT NOT NULL AUTO_INCREMENT, ClassifyCode INT NULL, AreaSize DOUBLE NULL,  Length INT NULL,  Width INT NULL, Height INT NULL, FOV INT NULL, PosX DOUBLE NULL, PosY DOUBLE NULL, memPOOL longtext DEFAULT NULL, memGROUP longtext DEFAULT NULL, memMEMORY longtext DEFAULT NULL, PRIMARY KEY (idx), UNIQUE INDEX idx_UNIQUE (idx ASC) VISIBLE);");
							errorCode = connector->RunQuery(query);
							if (errorCode == 0)
							{
								//insert재실행
								query = query->Format("INSERT INTO tempdata (ClassifyCode, AreaSize, Length, Width, Height, FOV, PosX, PosY, memPOOL, memGROUP, memMEMORY) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10})';",
									local[i]->nClassifyCode, local[i]->fAreaSize, local[i]->nLength, local[i]->nWidth, local[i]->nHeight, local[i]->nFOV, local[i]->fPosX, local[i]->fPosY,
									poolName, groupName, memoryName);

								errorCode = connector->RunQuery(query);
								if (errorCode != 0)
								{
									//예외처리 진행. 될때까지 업로드 시도 시키는게 좋을것으로 보임
								}
							}
							else
							{
								//예외처리 진행. 될때까지 업로드 시도 시키는게 좋을것으로 보임
							}
						}
						else
						{
							//예외처리 진행. 될때까지 업로드 시도 시키는게 좋을것으로 보임
						}
					}
					else
					{
						//DBOpen 실패
					}
				}

				array<CDefectDataWrapper^>^ arrLocal = gcnew array<CDefectDataWrapper^>(local->size());
				for (int i = 0; i < local->size(); i++)
				{
					arrLocal[i] = local[i];
				}

				return arrLocal;
			}
			else
			{
				array<CDefectDataWrapper^>^ local = gcnew array<CDefectDataWrapper^>(0);
				return local;
			}
		}
		void PaintOutline(int nY, int nOutline, byte* pByte, int nX)
		{
			for (int i = 0; i < nY; i++)
			{
				for (int j = 0; j < nOutline; j++)
				{
					pByte[i * nX + j] = 255;
				}
			}
			for (int i = 0; i < nY; i++)
			{
				for (int j = nX - nOutline; j < nX; j++)
				{
					pByte[i * nX + j] = 255;
				}
			}
			for (int i = 0; i < nOutline; i++)
			{
				for (int j = 0; j < nX; j++)
				{
					pByte[i * nX + j] = 255;
				}
			}
			for (int i = nY - nOutline; i < nY; i++)
			{
				for (int j = 0; j < nX; j++)
				{
					pByte[i * nX + j] = 255;
				}
			}
		}

		void PaintOutline(int startx, int starty, int endx, int endy, int nY, int nOutline, byte* pByte, int nX)
		{
			for (int i = starty; i < endy; i++)
			{
				for (int j = startx; j < startx + nOutline; j++)
				{
					pByte[i * nX + j] = 255;
				}
			}
			for (int i = starty; i < endy; i++)
			{
				for (int j = endx - nOutline; j < endx; j++)
				{
					pByte[i * nX + j] = 255;
				}
			}
			for (int i = starty; i < starty + nOutline; i++)
			{
				for (int j = startx; j < endx; j++)
				{
					pByte[i * nX + j] = 255;
				}
			}
			for (int i = endy - nOutline; i < endy; i++)
			{
				for (int j = startx; j < endx; j++)
				{
					pByte[i * nX + j] = 255;
				}
			}

		}

		int Width(RECT rt)
		{
			int width = rt.right - rt.left;

			width = abs(width);

			return width;
		}
		int Height(RECT rt)
		{
			int Height = rt.bottom - rt.top;

			Height = abs(Height);

			return Height;
		}
	};
}

