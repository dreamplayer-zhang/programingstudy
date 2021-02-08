#include "pch.h"
#include "CDefectDataWrapper.h"

#pragma warning(disable: 4244)

bool CDefectDataWrapper::IsCluster()
{
	if (ClusterItems == nullptr)
		return false;
	else if (ClusterItems->size() > 0)
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
			if (ClusterItems[i]->fPosX < left)
			{
				left = ClusterItems[i]->fPosX;
			}
		}
		return left;
	}
	else
	{
		//Cluster가 없는 경우 단순반환
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
			if (ClusterItems[i]->fPosY < top)
			{
				top = ClusterItems[i]->fPosY;
			}
		}
		return top;
	}
	else
	{
		//Cluster가 없는 경우 단순반환
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
			if (ClusterItems[i]->fPosX > right)
			{
				right = ClusterItems[i]->fPosX;
			}
		}
		return right;
	}
	else
	{
		//Cluster가 없는 경우 단순반환
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
			if (ClusterItems[i]->fPosY > bot)
			{
				bot = ClusterItems[i]->fPosY;
			}
		}
		return bot;
	}
	else
	{
		//Cluster가 없는 경우 단순반환
		return (int)(fPosY + nHeight / 2.0);
	}
}

int CDefectDataWrapper::GetDrawWidth()
{
	if (IsCluster())
	{
		//Cluster가 있는 경우 Cluster 내부의 모든 정보를 종합하여 반환
		int left = GetStartPointX();
		int right = GetEndPointX();
		return right - left;
	}
	else
	{
		//Cluster가 없는 경우 단순반환
		return nWidth;
	}
}

int CDefectDataWrapper::GetDrawHeight()
{
	if (IsCluster())
	{
		//Cluster가 있는 경우 Cluster 내부의 모든 정보를 종합하여 반환
		int top = GetStartPointY();
		int bot = GetEndPointY();
		return bot - top;
	}
	else
	{
		//Cluster가 없는 경우 단순반환
		return nHeight;
	}
}

CDefectDataWrapper::CDefectDataWrapper()
{
	ClusterItems = gcnew cliext::vector<CDefectDataWrapper^>();
}

CDefectDataWrapper::CDefectDataWrapper(DefectData item)
{
	fAreaSize = item.fAreaSize;
	fPosX = item.fPosX;
	fPosY = item.fPosY;
	nClassifyCode = item.nClassifyCode;
	nFOV = item.nFOV;
	nHeight = item.nHeight;
	nIdx = item.nIdx;
	nLength = item.nLength;
	nWidth = item.nWidth;
	bMergeUsed = false;
	ClusterItems = gcnew cliext::vector<CDefectDataWrapper^>();
}

CDefectDataWrapper::CDefectDataWrapper(CDefectDataWrapper^ item)
{
	fAreaSize = item->fAreaSize;
	fPosX = item->fPosX;
	fPosY = item->fPosY;
	nClassifyCode = item->nClassifyCode;
	nFOV = item->nFOV;
	nHeight = item->nHeight;
	nIdx = item->nIdx;
	nLength = item->nLength;
	nWidth = item->nWidth;
	bMergeUsed = item->bMergeUsed;

	ClusterItems = gcnew cliext::vector<CDefectDataWrapper^>();
	ClusterItems->assign(item->ClusterItems->begin(), item->ClusterItems->end());
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
	if (ClusterItems == nullptr)
		ClusterItems = gcnew cliext::vector<CDefectDataWrapper^>();

	int targetIdx = 0;
	ClusterItems->erase(ClusterItems->begin() + (idx - 1));
	MergeClusterInfo();
}

void CDefectDataWrapper::MergeClusterInfo()
{
	if (ClusterItems == nullptr)
		return;
	if (ClusterItems->size() == 0)
		return;
	if (!IsCluster())
		return;
	float x = 0.0f, y = 0.0f, area = 0.0f;

	for (int i = 0; i < ClusterItems->size(); i++)
	{
		x += ClusterItems[i]->fPosX;
		y += ClusterItems[i]->fPosY;
		area += ClusterItems[i]->fAreaSize;
	}
	fPosX = x / ClusterItems->size();
	fPosY = y / ClusterItems->size();
	fAreaSize = area / ClusterItems->size();

	nWidth = GetDrawWidth();
	nHeight = GetDrawHeight();
	nIdx = ClusterItems[0]->nIdx;
	nClassifyCode = ClusterItems[0]->nClassifyCode;
	nFOV = ClusterItems[0]->nFOV;
}

cliext::vector<CDefectDataWrapper^>^ CDefectDataWrapper::RearrangeCluster(CDefectDataWrapper^ data, int mergeDistance)
{
	cliext::vector<CDefectDataWrapper^>^ result = gcnew cliext::vector<CDefectDataWrapper^>();
	if (data->IsCluster())
	{
		for (int i = 0; i < data->ClusterItems->size(); i++)
		{
			result->push_back(data->ClusterItems[i]);
		}
	}
	else
	{
		result->push_back(data);
	}

	return CreateClusterList(data->ClusterItems, mergeDistance);
}

cliext::vector<CDefectDataWrapper^>^ CDefectDataWrapper::CreateClusterList(cliext::vector<CDefectDataWrapper^>^ data, int mergeDistance)
{
	cliext::vector<CDefectDataWrapper^>^ result = gcnew cliext::vector<CDefectDataWrapper^>();

	if (data->size() <= 1)
	{
		return data;
	}

	for (int i = 0; i < data->size(); i++)
	{
		bool merged = false;
		if (data[i]->bMergeUsed)
			continue;

		CDefectDataWrapper^ cluster = gcnew CDefectDataWrapper(data[i]);
		for (int j = i + 1; j < data->size(); j++)
		{
			if (CheckMerge(data[i], data[j], mergeDistance))
			{
				merged = true;
				data[j]->bMergeUsed = true;
				cluster->AddCluster(data[j]);
			}
		}
		if (merged)
		{
			data[i]->bMergeUsed = true;
			cluster->AddCluster(data[i]);//다 끝나고 merge가 한번이라도 되었다면 자기 자신도 넣고 끝낸다
											  //merge가 안 된 경우에는 clusteritems에 defect이 하나도 없게 됨

			cluster->MergeClusterInfo();//Cluster내의 정보를 병합해서 본체에 덮어씌운다
			result->push_back(cluster);
		}
		else
		{
			//merge에 진입하지 않은 경우, 자기 자신을 결과에 넣는다
			result->push_back(data[i]);
		}
	}
	return result;
}

cliext::vector<CDefectDataWrapper^>^ CDefectDataWrapper::MergeDefect(cliext::vector<CDefectDataWrapper^>^ data, int mergeDistance)
{
	cliext::vector<CDefectDataWrapper^>^ tempList = gcnew cliext::vector<CDefectDataWrapper^>();

	for (int i = 0; i < data->size(); i++)
	{
		//받아온 모든 List의 ClusterItems 및 요소를 치환한다
		if (data[i]->IsCluster())
		{
			for (int i = 0; i < data[i]->ClusterItems->size(); i++)
			{
				tempList->push_back(data[i]->ClusterItems[i]);
			}
		}
		else
		{
			tempList->push_back(data[i]);
		}
	}

	return CreateClusterList(tempList, mergeDistance);
}

bool CDefectDataWrapper::CheckMerge(CDefectDataWrapper^ data1, CDefectDataWrapper^ data2, int distance)
{
	int data1Width = data1->nWidth + (distance * 2);
	int data1Height = data1->nHeight + (distance * 2);
	System::Drawing::Rectangle^ data1Rect = gcnew System::Drawing::Rectangle(
		(int)(data1->fPosX - data1Width / 2.0),
		(int)(data1->fPosY - data1Height / 2.0),
		data1Width,
		data1Height);

	int data2Width = data2->nWidth + (distance * 2);
	int data2Height = data2->nHeight + (distance * 2);
	System::Drawing::Rectangle^ data2Rect = gcnew System::Drawing::Rectangle(
		(int)(data2->fPosX - data2Width / 2.0),
		(int)(data2->fPosY - data2Height / 2.0),
		data2Width,
		data2Height);

	System::Drawing::Rectangle^ result = System::Drawing::Rectangle::Intersect(*data1Rect, *data2Rect);

	if (result->IsEmpty)
	{
		return false;
	}
	else
	{
		return true;
	}
}
