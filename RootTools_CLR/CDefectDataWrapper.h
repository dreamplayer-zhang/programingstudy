#pragma once
#include "DefectData.h"
#include "../RootTools_Cpp/TypeDefines.h"
ref class CDefectDataWrapper : DefectData
{
public:
	bool IsCluster();
	array<CDefectDataWrapper^>^ ClusterItems;
	int GetStartPointX();
	int GetStartPointY();
	int GetEndPointX();
	int GetEndPointY();
	int GetDrawWidth();
	int GetDrawHeight();
	bool bMergeUsed;

	CDefectDataWrapper();
	CDefectDataWrapper(DefectData item);
	CDefectDataWrapper(CDefectDataWrapper^ item);
	void AddCluster(CDefectDataWrapper^ data);
	void AddRangeCluster(array<CDefectDataWrapper^>^ dataList);
	void RemoveCluster(int idx);
	void MergeClusterInfo();
	static array<CDefectDataWrapper^>^ RearrangeCluster(CDefectDataWrapper^ data, int mergeDistance);
	static array<CDefectDataWrapper^>^ CreateClusterList(array<CDefectDataWrapper^>^ datas, int mergeDistance);
	static array<CDefectDataWrapper^>^ MergeDefect(array<CDefectDataWrapper^>^ datas, int mergeDistance);
	static bool CheckMerge(CDefectDataWrapper^ data1, CDefectDataWrapper^ data2, int distance);
};

