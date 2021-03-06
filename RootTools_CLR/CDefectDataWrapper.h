#pragma once
#include "DefectData.h"
#include "../RootTools_Cpp/TypeDefines.h"
#include <cliext/vector>

#pragma warning(disable: 4677)

public ref class CDefectDataWrapper : DefectData
{
public:
	bool IsCluster();

	cliext::vector<CDefectDataWrapper^>^ ClusterItems;
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
	void AddRangeCluster(cliext::vector<CDefectDataWrapper^>^ dataList);
	void RemoveCluster(int idx);
	void MergeClusterInfo();
	static cliext::vector<CDefectDataWrapper^>^ RearrangeCluster(CDefectDataWrapper^ data, int mergeDistance);
	static cliext::vector<CDefectDataWrapper^>^ CreateClusterList(cliext::vector<CDefectDataWrapper^>^ data, int mergeDistance);
	static cliext::vector<CDefectDataWrapper^>^ MergeDefect(cliext::vector<CDefectDataWrapper^>^ data, int mergeDistance);
	static bool CheckMerge(CDefectDataWrapper^ data1, CDefectDataWrapper^ data2, int distance);
};

