#include "pch.h"
#include "Cpp_Demo.h"
//#include <opencv2\opencv.hpp>

//using namespace cv;

bool Cpp_Demo::OpenImage(std::string strFilepath)
{
	//string ImageFilePath = cv::format("%s", strFilepath);
	std::string ImageFilePath = "D:\\lena.jpg";
	//Mat image;
	//image = imread(ImageFilePath, IMREAD_COLOR);

	//if (image.empty())
	//{
	//	return false;
	//}
	//else
	//{
	///*	namedWindow("Test", WINDOW_AUTOSIZE);
	//	imshow("Test", image);*/
	//	return true;
	//}
	
	return false;
}

int Cpp_Demo::Add(int n0, int n1)
{
	m_nAdd = n0 + n1; 
	return m_nAdd;
}

void Cpp_Demo::SetMemory(int nCount, int nByte, int xSize, int ySize, __int64 nAddress)
{
	m_memory.Init(nCount, nByte, xSize, ySize, nAddress); 
}