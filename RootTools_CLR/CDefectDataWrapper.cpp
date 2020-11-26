#include "pch.h"
#include "CDefectDataWrapper.h"

bool CDefectDataWrapper::IsCluster()
{
	if (ClusterItems == nullptr)
		return false;

	if (ClusterItems->Length > 0)
		return true;
	else
		return false;
}

int CDefectDataWrapper::GetStartPointX()
{
	if (IsCluster())
	{
		int left = INT_MAX;
		for (int i = 0; i < ClusterItems->Length; i++)
		{
			if (left > ClusterItems[i]->GetStartPointX())
			{
				left = ClusterItems[i]->GetStartPointX();
			}
		}
		if (left != INT_MAX)
			return left;
		else
			return (int)(fPosX - nWidth / 2.0);
	}
	else
	{
		return (int)(fPosX - nWidth / 2.0);
	}
}

int CDefectDataWrapper::GetStartPointY()
{
	if (IsCluster())
	{
		int top = INT_MAX;
		for (int i = 0; i < ClusterItems->Length; i++)
		{
			if (top > ClusterItems[i]->GetStartPointY())
			{
				top = ClusterItems[i]->GetStartPointY();
			}
		}
		if (top != INT_MAX)
			return top;
		else
			return (int)(fPosY - nHeight / 2.0);
	}
	else
	{
		return (int)(fPosY - nHeight / 2.0);
	}
}

int CDefectDataWrapper::GetEndPointX()
{
	if (IsCluster())
	{
		int right = INT_MIN;
		for (int i = 0; i < ClusterItems->Length; i++)
		{
			if (right < ClusterItems[i]->GetEndPointX())
			{
				right = ClusterItems[i]->GetEndPointX();
			}
		}
		if (right != INT_MIN)
			return right;
		else
			return (int)(fPosX + nWidth / 2.0);
	}
	else
	{
		return (int)(fPosX + nWidth / 2.0);
	}
}

int CDefectDataWrapper::GetEndPointY()
{
	if (IsCluster())
	{
		int bot = INT_MIN;
		for (int i = 0; i < ClusterItems->Length; i++)
		{
			if (bot < ClusterItems[i]->GetEndPointY())
			{
				bot = ClusterItems[i]->GetEndPointY();
			}
		}
		if (bot != INT_MIN)
			return bot;
		else
			return (int)(fPosY + nHeight / 2.0);
	}
	else
	{
		return (int)(fPosY + nHeight / 2.0);
	}
}

int CDefectDataWrapper::GetDrawWidth()
{
	if (IsCluster())
	{
		return GetEndPointX() - GetStartPointX();
	}
	else
	{
		return nWidth;
	}
}

int CDefectDataWrapper::GetDrawHeight()
{
	if (IsCluster())
	{
		return GetEndPointY() - GetStartPointY();
	}
	else
	{
		return nHeight;
	}
}

CDefectDataWrapper::CDefectDataWrapper()
{

}

CDefectDataWrapper::CDefectDataWrapper(DefectData item)
{

	fPosX = item.fPosX;
	fPosY = item.fPosY;
	fAreaSize = item.fAreaSize;
	nClassifyCode = item.nClassifyCode;
	nFOV = item.nFOV;
	nWidth = item.nWidth;
	nHeight = item.nHeight;
	nIdx = item.nIdx;
	nLength = item.nLength;
	bMergeUsed = false;

	ClusterItems = gcnew array<CDefectDataWrapper^>(0);//DefectData 클래스에는 Cluster가 없음
}
CDefectDataWrapper::CDefectDataWrapper(CDefectDataWrapper^ item)
{
	fPosX = item->fPosX;
	fPosY = item->fPosY;
	fAreaSize = item->fAreaSize;
	nClassifyCode = item->nClassifyCode;
	nFOV = item->nFOV;
	nWidth = item->nWidth;
	nHeight = item->nHeight;
	nIdx = item->nIdx;
	nLength = item->nLength;
	bMergeUsed = false;

	ClusterItems = gcnew array<CDefectDataWrapper^>(item->ClusterItems->Length);//DefectData 클래스에는 Cluster가 없음
	ClusterItems = item->ClusterItems;
}

void CDefectDataWrapper::AddCluster(CDefectDataWrapper^ data)
{
	if (ClusterItems == nullptr)
		ClusterItems = gcnew array<CDefectDataWrapper^>(0);

	array< CDefectDataWrapper^>^ tempList = gcnew array<CDefectDataWrapper^>(ClusterItems->Length + 1);
	tempList = ClusterItems;
	tempList[ClusterItems->Length] = data;
	ClusterItems = gcnew array<CDefectDataWrapper^>(ClusterItems->Length + 1);
	ClusterItems = tempList;
	MergeClusterInfo();
}

void CDefectDataWrapper::AddRangeCluster(array<CDefectDataWrapper^>^ dataList)
{
	array<CDefectDataWrapper^>^ tempList = gcnew array<CDefectDataWrapper^>(ClusterItems->Length + dataList->Length);

	int targetIdx = 0;
	for (targetIdx = 0; targetIdx < ClusterItems->Length; targetIdx++)
	{
		tempList[targetIdx] = ClusterItems[targetIdx];
	}
	for (int i = 0; i < dataList->Length; i++, targetIdx++)
	{
		tempList[targetIdx] = dataList[i];
	}
	MergeClusterInfo();
}

void CDefectDataWrapper::RemoveCluster(int idx)
{
	array<CDefectDataWrapper^>^ tempList = gcnew array<CDefectDataWrapper^>(ClusterItems->Length - 1);
	int targetIdx = 0;
	for (int i = 0; i < tempList->Length; targetIdx++)
	{
		if (idx != i)
		{
			tempList[i] = ClusterItems[targetIdx];
		}
	}
	MergeClusterInfo();
}

void CDefectDataWrapper::MergeClusterInfo()
{
	if (ClusterItems->Length > 0)
	{
		float floatTemp = 0.0;
		int intTemp = 0;
		for (int i = 0; i < ClusterItems->Length; i++)
		{
			floatTemp += ClusterItems[i]->fPosX;
		}
		fPosX = floatTemp / ClusterItems->Length;

		floatTemp = 0.0;
		for (int i = 0; i < ClusterItems->Length; i++)
		{
			floatTemp += ClusterItems[i]->fPosY;
		}
		fPosY = floatTemp / ClusterItems->Length;

		floatTemp = 0.0;
		for (int i = 0; i < ClusterItems->Length; i++)
		{
			floatTemp += ClusterItems[i]->fAreaSize;
		}
		fAreaSize = floatTemp / ClusterItems->Length;

		nClassifyCode = ClusterItems[0]->nClassifyCode;
		nFOV = ClusterItems[0]->nFOV;

		intTemp = 0.0;//nWidth
		for (int i = 0; i < ClusterItems->Length; i++)
		{
			intTemp += ClusterItems[i]->fAreaSize;
		}
		fAreaSize = intTemp / ClusterItems->Length;

		/*
		 = ClusterItems.Sum(x = > x.nWidth);
		nHeight = ClusterItems.Sum(x = > x.nHeight);
		nIdx = ClusterItems.First().nIdx;*/
	}
}

array<CDefectDataWrapper^>^ CDefectDataWrapper::RearrangeCluster(CDefectDataWrapper^ data, int mergeDistance)
{
	if (data->ClusterItems == nullptr || data->ClusterItems->Length <= 1)
	{
		//Cluster가 비이있거나 하나밖에 없는 경우 자기 자신을 list로 반환
		array<CDefectDataWrapper^>^ result = gcnew array<CDefectDataWrapper^>(1);
		result[0] = data;
		return result;
	}

	return CreateClusterList(data->ClusterItems, mergeDistance);
}

array<CDefectDataWrapper^>^ CDefectDataWrapper::CreateClusterList(array<CDefectDataWrapper^>^ datas, int mergeDistance)
{
	array<CDefectDataWrapper^>^ result = gcnew array<CDefectDataWrapper^>(0);
	if (datas->Length <= 1)
	{
		return datas;
	}

	for (int i = 0; i < datas->Length - 1; i++)
	{
		bool merged = false;
		if (datas[i]->bMergeUsed)
			continue;

		CDefectDataWrapper^ cluster = gcnew CDefectDataWrapper(datas[i]);
		for (int j = i + 1; j < datas->Length; j++)
		{
			if (CheckMerge(datas[i], datas[j], mergeDistance))
			{
				merged = true;
				datas[j]->bMergeUsed = true;
				cluster->AddCluster(datas[j]);
			}
		}
		if (merged)
		{
			datas[i]->bMergeUsed = true;
			cluster->AddCluster(datas[i]);//다 끝나고 merge가 한번이라도 되었다면 자기 자신도 넣고 끝낸다
											  //merge가 안 된 경우에는 clusteritems에 defect이 하나도 없게 됨

			cluster->MergeClusterInfo();//Cluster내의 정보를 병합해서 본체에 덮어씌운다
			array<CDefectDataWrapper^>^ tempResult = gcnew array<CDefectDataWrapper^>(result->Length + 1);
			tempResult = result;
			tempResult[result->Length] = cluster;
			result = gcnew array<CDefectDataWrapper^>(tempResult->Length);
			result = tempResult;
		}
		else
		{
			//merge에 진입하지 않은 경우, 자기 자신을 결과에 넣는다
			array<CDefectDataWrapper^>^ tempResult = gcnew array<CDefectDataWrapper^>(result->Length + 1);
			tempResult = result;
			tempResult[result->Length] = datas[i];
			result = gcnew array<CDefectDataWrapper^>(tempResult->Length);
			result = tempResult;
		}
	}
	return result;
}

array<CDefectDataWrapper^>^ CDefectDataWrapper::MergeDefect(array<CDefectDataWrapper^>^ datas, int mergeDistance)
{
	array<CDefectDataWrapper^>^ tempList = gcnew array<CDefectDataWrapper^>(0);

	for (int i = 0; i < datas->Length; i++)
	{
		//받아온 모든 List의 ClusterItems 및 요소를 치환한다
		if (datas[i]->IsCluster())
		{
			//tempList->AddRange(datas[i]->ClusterItems);
			array<CDefectDataWrapper^>^ tempResult = gcnew array<CDefectDataWrapper^>(tempList->Length + datas[i]->ClusterItems->Length);
			tempResult = tempList;
			for (int i = 0; i < datas[i]->ClusterItems->Length; i++)
			{
				tempResult[i + tempList->Length - 1] = datas[i]->ClusterItems[i];
			}
			tempList = gcnew array<CDefectDataWrapper^>(tempList->Length + datas[i]->ClusterItems->Length);
			tempList = tempResult;
		}
		else
		{
			array<CDefectDataWrapper^>^ tempResult = gcnew array<CDefectDataWrapper^>(tempList->Length + 1);
			tempResult = tempList;
			tempResult[tempList->Length] = datas[i];
			tempList = gcnew array<CDefectDataWrapper^>(tempResult->Length);
			tempList = tempResult;
		}
	}

	return CreateClusterList(tempList, mergeDistance);
}

bool CDefectDataWrapper::CheckMerge(CDefectDataWrapper^ data1, CDefectDataWrapper^ data2, int distance)
{
	int data1Width = data1->nWidth + (distance * 2);
	int data1Height = data1->nHeight + (distance * 2);

	int data1Left = data1->fPosX - data1Width / 2.0;
	int data1Top = data1->fPosY - data1Height / 2.0;
	int data1Right = data1->fPosX + data1Width / 2.0;
	int data1Bot = data1->fPosY + data1Height / 2.0;

	int data2Width = data2->nWidth + (distance * 2);
	int data2Height = data2->nHeight + (distance * 2);

	int data2Left = data2->fPosX - data2Width / 2.0;
	int data2Top = data2->fPosY - data2Height / 2.0;
	int data2Right = data2->fPosX + data2Width / 2.0;
	int data2Bot = data2->fPosY + data2Height / 2.0;

	if (data1Left < data2Right && data1Right > data2Left &&
		data1Top > data2Bot && data1Bot < data2Top)
	{
		//겹침
		return true;
	}
	else 
	{
		//안겹침
		return false;
	}
}
