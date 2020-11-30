#include "pch.h"
#include "CDefectDataWrapper.h"

bool CDefectDataWrapper::IsCluster()
{
	if (ClusterItems == nullptr)
		return false;
	if (ClusterItems->size() > 0)
		return true;
	else
		return false;
}

int CDefectDataWrapper::GetStartPointX()
{
	if (IsCluster())
	{
		int left = INT_MAX;
		for (int i = 0; i < ClusterItems->size(); i++)
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
		for (int i = 0; i < ClusterItems->size(); i++)
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
		for (int i = 0; i < ClusterItems->size(); i++)
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
		for (int i = 0; i < ClusterItems->size(); i++)
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

	ClusterItems = gcnew cliext::vector<CDefectDataWrapper^>();
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
	if (item->ClusterItems != nullptr)
	{
		ClusterItems = gcnew cliext::vector<CDefectDataWrapper^>(item->ClusterItems);//DefectData 클래스에는 Cluster가 없음
	}
}

void CDefectDataWrapper::AddCluster(CDefectDataWrapper^ data)
{
	if (ClusterItems == nullptr)
		ClusterItems = gcnew cliext::vector<CDefectDataWrapper^>();

	ClusterItems->push_back(data);
	MergeClusterInfo();
}

void CDefectDataWrapper::AddRangeCluster(cliext::vector<CDefectDataWrapper^>^ dataList)
{
	if (ClusterItems == nullptr)
		ClusterItems = gcnew cliext::vector<CDefectDataWrapper^>();

	for (int i = 0; i < dataList->size(); i++)
	{
		ClusterItems->push_back(dataList[i]);
	}
	MergeClusterInfo();
}

void CDefectDataWrapper::RemoveCluster(int idx)
{
	if (ClusterItems->size() == 0)
		return;

	int targetIdx = 0;
	ClusterItems->erase(ClusterItems->begin() + (idx - 1));
	MergeClusterInfo();
}

void CDefectDataWrapper::MergeClusterInfo()
{
	if (ClusterItems->size() > 0)
	{
		float floatTemp = 0.0;
		int intTemp = 0;
		for (int i = 0; i < ClusterItems->size(); i++)
		{
			floatTemp += ClusterItems[i]->fPosX;
		}
		fPosX = floatTemp / ClusterItems->size();

		floatTemp = 0.0;
		for (int i = 0; i < ClusterItems->size(); i++)
		{
			floatTemp += ClusterItems[i]->fPosY;
		}
		fPosY = floatTemp / ClusterItems->size();

		floatTemp = 0.0;
		for (int i = 0; i < ClusterItems->size(); i++)
		{
			floatTemp += ClusterItems[i]->fAreaSize;
		}
		fAreaSize = floatTemp;// / ClusterItems->size();

		nClassifyCode = ClusterItems[0]->nClassifyCode;
		nFOV = ClusterItems[0]->nFOV;

		nWidth = GetDrawWidth();
		nHeight = GetDrawHeight();

		nIdx = ClusterItems[0]->nIdx;

		/*
		 = ClusterItems.Sum(x = > x.nWidth);
		nHeight = ClusterItems.Sum(x = > x.nHeight);
		nIdx = ClusterItems.First().nIdx;*/
	}
}

cliext::vector<CDefectDataWrapper^>^ CDefectDataWrapper::RearrangeCluster(CDefectDataWrapper^ data, int mergeDistance)
{
	if (data->ClusterItems == nullptr || data->ClusterItems->size() <= 1)
	{
		//Cluster가 비이있거나 하나밖에 없는 경우 자기 자신을 list로 반환
		cliext::vector<CDefectDataWrapper^>^ result = gcnew cliext::vector<CDefectDataWrapper^>();
		result->push_back(data);
		return result;
	}

	return CreateClusterList(data->ClusterItems, mergeDistance);
}

cliext::vector<CDefectDataWrapper^>^ CDefectDataWrapper::CreateClusterList(cliext::vector<CDefectDataWrapper^>^ datas, int mergeDistance)
{
	cliext::vector<CDefectDataWrapper^>^ result = gcnew cliext::vector<CDefectDataWrapper^>();
	if (datas->size() <= 1)
	{
		return datas;
	}

	for (int i = 0; i < datas->size(); i++)
	{
		bool merged = false;
		if (datas[i]->bMergeUsed)
			continue;

		CDefectDataWrapper^ cluster = gcnew CDefectDataWrapper(datas[i]);
		for (int j = i + 1; j < datas->size(); j++)
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
			result->push_back(cluster);
		}
		else
		{
			//merge에 진입하지 않은 경우, 자기 자신을 결과에 넣는다
			result->push_back(datas[i]);
		}
	}
	return result;
}

cliext::vector<CDefectDataWrapper^>^ CDefectDataWrapper::MergeDefect(cliext::vector<CDefectDataWrapper^>^ datas, int mergeDistance)
{
	cliext::vector<CDefectDataWrapper^>^ tempList = gcnew cliext::vector<CDefectDataWrapper^>();

	for (int i = 0; i < datas->size(); i++)
	{
		//받아온 모든 List의 ClusterItems 및 요소를 치환한다
		if (datas[i]->IsCluster())
		{
			for (int j = 0; j < datas[i]->ClusterItems->size(); j++)
			{
				tempList->push_back(datas[i]->ClusterItems[j]);
			}
		}
		else
		{
			tempList->push_back(datas[i]);
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
