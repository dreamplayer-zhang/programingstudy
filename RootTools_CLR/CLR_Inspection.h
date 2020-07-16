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

#include <vcclr.h> // for PtrToStringChars 
#include <stdio.h> // for wprintf
#include <msclr\marshal_cppstd.h>

#include "MySQLDBConnector.h"


namespace RootTools_CLR
{
	public ref class CLR_Inspection
	{
	protected:
		CLR_InspConnector^ m_InspConn = nullptr;
		CInspectionSurface* pInspSurface = nullptr;
		CInspectionReticle* pInspReticle = nullptr;
	public:
		CLR_Inspection(int nThreadNum, int nROIWidth, int nROIHeight)
		{
			m_InspConn = gcnew CLR_InspConnector(nThreadNum);
			pInspSurface = new CInspectionSurface(nROIWidth, nROIHeight);
			pInspReticle = new CInspectionReticle(nROIWidth, nROIHeight);
		}

		virtual ~CLR_Inspection()
		{
			delete pInspSurface;
			delete pInspReticle;
		}

		void SurfaceInspection(System::String^ poolName, System::String^ groupName, System::String^ memoryName, unsigned __int64  memOffset, int threadindex, int nDefectCode, int RoiLeft, int RoiTop, int RoiRight, int RoiBottom, int  memwidth, int  memHeight, int GV, int DefectSize, bool bDark, bool bAbsolute)
		{
			RECT targetRect;
			std::vector<DefectDataStruct> vTempResult;

			targetRect.left = RoiLeft;
			targetRect.right = RoiRight;
			targetRect.top = RoiTop;
			targetRect.bottom = RoiBottom;

			msclr::interop::marshal_context context;
			std::string standardString = context.marshal_as<std::string>(poolName);

			byte* buffer = m_InspConn->GetImagePool(standardString, memOffset, memwidth, memHeight);
			if (buffer != NULL)
			{
				int bufferwidth = memwidth;
				int bufferheight = memHeight;

				pInspSurface->SetParams(buffer, bufferwidth, bufferheight, targetRect, 1, GV, DefectSize, bDark, threadindex);

				//TODO ���⼭ �̺�Ʈ�� �ø��� ������� �����Ѵ�
				//���� ���ö� �̹� �� ���� ���� ������ ��°�� �Ѿ���� ���̹Ƿ� ���� ��ü�� �����Ͽ� AddDefect�� �߻��ϴ� ������ ���⼭ �����ϵ��� �����Ѵ�
				//pInspSurface->Inspection();

				pInspSurface->CheckConditions();

				pInspSurface->CopyImageToBuffer(bDark);//opencv pitsize �������� �������� buffer copy�� �ʿ���
				vTempResult = pInspSurface->SurfaceInspection(bAbsolute, nDefectCode);//TODO : absolute GV �����ؾ���

				bool bResultExist = vTempResult.size() > 0;

				if (bResultExist)
				{
					MySQLDBConnector^ connector = gcnew MySQLDBConnector();
					unsigned int errorCode = connector->OpenDatabase();

					for (int i = 0; i < vTempResult.size(); i++)
					{
						if (errorCode == 0)
						{
							//DB Open����

							System::String^ query;
							query = query->Format("INSERT INTO tempdata (ClassifyCode, AreaSize, Length, Width, Height, FOV, PosX, PosY, memPOOL, memGROUP, memMEMORY) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}');",
								vTempResult[i].nClassifyCode, vTempResult[i].fAreaSize, vTempResult[i].nLength, vTempResult[i].nWidth, vTempResult[i].nHeight, vTempResult[i].nFOV, vTempResult[i].fPosX + targetRect.left, vTempResult[i].fPosY + targetRect.top,
								poolName, groupName, memoryName);

							errorCode = connector->RunQuery(query);

							if (errorCode == 0)
							{
								//success
							}
							else if (errorCode == 1146)
							{
								//table�� ����
								//table���� �� ��õ�
								query = query->Format("CREATE TABLE tempdata(idx INT NOT NULL AUTO_INCREMENT, ClassifyCode INT NULL, AreaSize DOUBLE NULL,  Length INT NULL,  Width INT NULL, Height INT NULL, FOV INT NULL, PosX DOUBLE NULL, PosY DOUBLE NULL, memPOOL longtext DEFAULT NULL, memGROUP longtext DEFAULT NULL, memMEMORY longtext DEFAULT NULL, PRIMARY KEY (idx), UNIQUE INDEX idx_UNIQUE (idx ASC) VISIBLE);");								
								errorCode = connector->RunQuery(query);
								if (errorCode == 0)
								{
									//insert�����
									query = query->Format("INSERT INTO tempdata (ClassifyCode, AreaSize, Length, Width, Height, FOV, PosX, PosY, memPOOL, memGROUP, memMEMORY) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}');",
										vTempResult[i].nClassifyCode, vTempResult[i].fAreaSize, vTempResult[i].nLength, vTempResult[i].nWidth, vTempResult[i].nHeight, vTempResult[i].nFOV, vTempResult[i].fPosX + targetRect.left, vTempResult[i].fPosY + targetRect.top,
										poolName, groupName, memoryName);

									errorCode = connector->RunQuery(query);
									if (errorCode != 0)
									{
										//����ó�� ����. �ɶ����� ���ε� �õ� ��Ű�°� ���������� ����
									}
								}
								else
								{
									//����ó�� ����. �ɶ����� ���ε� �õ� ��Ű�°� ���������� ����
								}
							}
							else
							{
								//����ó�� ����. �ɶ����� ���ε� �õ� ��Ű�°� ���������� ����
							}
						}
						else
						{
							//DBOpen ����
						}
					}
				}

				//return local;
			}
			else
			{
				array<DefectData^>^ local = gcnew array<DefectData^>(0);
				//return local;
			}
		}
		void StripInspection(System::String^ poolName, System::String^ groupName, System::String^ memoryName, unsigned __int64 memOffset, int threadindex, int nDefectCode, int RoiLeft, int RoiTop, int RoiRight, int RoiBottom, int  memwidth, int  memHeight, int GV, int DefectSize, int nIntensity, int nBandwidth)
		{
			RECT targetRect;
			std::vector<DefectDataStruct> vTempResult;

			targetRect.left = RoiLeft;
			targetRect.right = RoiRight;
			targetRect.top = RoiTop;
			targetRect.bottom = RoiBottom;

			msclr::interop::marshal_context context;
			std::string standardString = context.marshal_as<std::string>(poolName);

			byte* buffer = m_InspConn->GetImagePool(standardString, memOffset, memwidth, memHeight);//TODO �����ʿ�
			int bufferwidth = memwidth;
			int bufferheight = memHeight;


			if (buffer != NULL)
			{
				int bufferwidth = memwidth;
				int bufferheight = memHeight;

				pInspReticle->SetParams(buffer, bufferwidth, bufferheight, targetRect, 1, threadindex, GV, DefectSize);
				pInspReticle->CheckConditions();
				pInspReticle->CopyImageToBuffer(true);//opencv pitsize �������� �������� buffer copy�� �ʿ���

				vTempResult = pInspReticle->StripInspection(nBandwidth, nIntensity, nDefectCode);

				bool bResultExist = vTempResult.size() > 0;


				if (bResultExist)
				{
					MySQLDBConnector^ connector = gcnew MySQLDBConnector();
					unsigned int errorCode = connector->OpenDatabase();

					for (int i = 0; i < vTempResult.size(); i++)
					{
						if (errorCode == 0)
						{
							//DB Open����

							System::String^ query;
							query = query->Format("INSERT INTO tempdata (ClassifyCode, AreaSize, Length, Width, Height, FOV, PosX, PosY, memPOOL, memGROUP, memMEMORY) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}');",
								vTempResult[i].nClassifyCode, vTempResult[i].fAreaSize, vTempResult[i].nLength, vTempResult[i].nWidth, vTempResult[i].nHeight, vTempResult[i].nFOV, vTempResult[i].fPosX + targetRect.left, vTempResult[i].fPosY + targetRect.top,
								poolName, groupName, memoryName);

							errorCode = connector->RunQuery(query);

							if (errorCode == 0)
							{
								//success
							}
							else if (errorCode == 1146)
							{
								//table�� ����
								//table���� �� ��õ�
								query = query->Format("CREATE TABLE tempdata(idx INT NOT NULL AUTO_INCREMENT, ClassifyCode INT NULL, AreaSize DOUBLE NULL,  Length INT NULL,  Width INT NULL, Height INT NULL, FOV INT NULL, PosX DOUBLE NULL, PosY DOUBLE NULL, memPOOL longtext DEFAULT NULL, memGROUP longtext DEFAULT NULL, memMEMORY longtext DEFAULT NULL, PRIMARY KEY (idx), UNIQUE INDEX idx_UNIQUE (idx ASC) VISIBLE);");
								errorCode = connector->RunQuery(query);
								if (errorCode == 0)
								{
									//insert�����
									query = query->Format("INSERT INTO tempdata (ClassifyCode, AreaSize, Length, Width, Height, FOV, PosX, PosY, memPOOL, memGROUP, memMEMORY) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10})';",
										vTempResult[i].nClassifyCode, vTempResult[i].fAreaSize, vTempResult[i].nLength, vTempResult[i].nWidth, vTempResult[i].nHeight, vTempResult[i].nFOV, vTempResult[i].fPosX + targetRect.left, vTempResult[i].fPosY + targetRect.top,
										poolName, groupName, memoryName);

									errorCode = connector->RunQuery(query);
									if (errorCode != 0)
									{
										//����ó�� ����. �ɶ����� ���ε� �õ� ��Ű�°� ���������� ����
									}
								}
								else
								{
									//����ó�� ����. �ɶ����� ���ε� �õ� ��Ű�°� ���������� ����
								}
							}
							else
							{
								//����ó�� ����. �ɶ����� ���ε� �õ� ��Ű�°� ���������� ����
							}
						}
						else
						{
							//DBOpen ����
						}
					}
				}
			}

			//return local;
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

