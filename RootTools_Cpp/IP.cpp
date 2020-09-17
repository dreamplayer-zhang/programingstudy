
#include "pch.h"

#include "IP.h"

void IP::Threshold(BYTE* pSrc, int nW, int nH, int threshold, bool bDark, BYTE* pDst)
{
    Mat imgSrc = Mat(nW, nH, CV_8UC1, pSrc);
    Mat imgDst;

    if (bDark)
        cv::threshold(imgSrc, imgDst, threshold, 255, CV_THRESH_BINARY_INV);
    else
        cv::threshold(imgSrc, imgDst, threshold, 255, CV_THRESH_BINARY);

    pDst = imgDst.ptr<BYTE>();

    //namedWindow("Threshold window", WINDOW_KEEPRATIO);
    //imshow("Threshold window", imgSrc);

    //namedWindow("Threshold2 window", WINDOW_KEEPRATIO);
    //imshow("Threshold2 window", imgDst);
}

void IP::Labeling(BYTE* pSrc, BYTE* pBin, int nW, int nH, bool bDark, BYTE* pDst, std::vector<LabeledData>& vtLabeled)
{
    Mat imgSrc = Mat(nW, nH, CV_8UC1, pSrc);
    Mat imgBin = Mat(nW, nH, CV_8UC1, pBin);
    Mat imgDst;
    Mat imgMul;

    cv::multiply(imgSrc, imgBin, imgMul, 1.0, CV_8UC1);

    Mat img_labels, stats, centroids;
    
    int numOfLables = connectedComponentsWithStats(imgBin, imgDst, stats, centroids, 8, CV_32S);


    // Dark�� ��� min ���� ã��, Bright ��� Max ���� ã��
    BYTE* pValue = new BYTE[numOfLables - 1];
    memset(pValue, bDark ? 255 : 0, sizeof(BYTE) * (numOfLables - 1));

    for (int i = 0; i < nH * nW; i++)
    {
        int label = imgDst.at<int>(i);
        if (label == 0) continue;

        BYTE val = imgSrc.at<BYTE>(i);
        if (bDark)
        {
            if (val < pValue[label])
            {
                pValue[label] = val;
            }
        }
        else
        {
            if (val > pValue[label])
            {
                pValue[label] = val;
            }
        }
    }

    //ù��° ���� Background Label ��
    for (int j = 1; j < numOfLables; j++) {
        int area = stats.at<int>(j, CC_STAT_AREA);
        int left = stats.at<int>(j, CC_STAT_LEFT);
        int top = stats.at<int>(j, CC_STAT_TOP);
        int width = stats.at<int>(j, CC_STAT_WIDTH);
        int height = stats.at<int>(j, CC_STAT_HEIGHT);
        int right = left + width;
        int bottom = top + height;

        LabeledData data;
        data.bound = { left, top, right, bottom };
        data.center = { (left + right) / 2, (top + bottom) / 2 };
        data.area = area;
        data.value = pValue[j - 1];

        vtLabeled.push_back(data);
    }

    delete[] pValue;

    //Mat imgColors;

    //cvtColor(imgSrc, imgColors, COLOR_GRAY2BGR);

    //for (int i = 0; i < imgDst.rows; i++)
    //{
    //    int* label = imgDst.ptr<int>(i);
    //    Vec3b* pixel = imgColors.ptr<Vec3b>(i);

    //    for (int j = 0; j < imgDst.cols; j++)
    //    {
    //        if (label[j] != 0)
    //        {
    //            pixel[j][2] = 255;
    //            pixel[j][1] = 0;
    //            pixel[j][0] = 0;
    //        }
    //    }
    //}

    //namedWindow("Labeling window", WINDOW_KEEPRATIO);
    //imshow("Labeling window", imgColors);
}


// Vision
void IP::Threshold(BYTE* pSrc, BYTE* pDst, int nW, int nH, bool bDark, int thresh)
{
    Mat imgSrc = Mat(nH, nW, CV_8UC1, pSrc);
    Mat imgDst = Mat(nH, nW, CV_8UC1, pDst);

    
    if (bDark)
        cv::threshold(imgSrc, imgDst, thresh, 255, CV_THRESH_BINARY_INV);
    else
        cv::threshold(imgSrc, imgDst, thresh, 255, CV_THRESH_BINARY);
}
void IP::Threshold(BYTE* pSrc, BYTE* pDst, int nMemW, int nMemH, Point ptLT, Point ptRB, bool bDark, int thresh)
{
    Mat imgSrc = Mat(nMemW, nMemW, CV_8UC1, pSrc);
    Mat ROI(imgSrc, Rect(ptLT, ptRB));

    Mat imgDst = Mat((ptRB.y - ptLT.y), (ptRB.x - ptLT.x), CV_8UC1, pDst);

    if (bDark)
        cv::threshold(ROI, imgDst, thresh, 255, CV_THRESH_BINARY_INV);
    else
        cv::threshold(ROI, imgDst, thresh, 255, CV_THRESH_BINARY);
}

float IP::Average(BYTE* pSrc, int nW, int nH)
{
    Mat imgSrc = Mat(nH, nW, CV_8UC1, pSrc);

    return (cv::sum(cv::sum(imgSrc)))[0] / ((uint64)nW * nH); // mean �Լ��� return Type = Scalar
}
float IP::Average(BYTE* pSrc, int nMemW, int nMemH, Point ptLT, Point ptRB)
{
    Mat imgSrc = Mat(nMemW, nMemH, CV_8UC1, pSrc);
    Mat ROI(imgSrc, Rect(ptLT, ptRB));

    return (cv::sum(cv::sum(ROI)))[0] / ((uint64)(ptRB.x - ptLT.x) * (ptRB.y - ptLT.y)); // mean �Լ��� return Type = Scalar

}

void IP::Labeling(BYTE* pSrc, BYTE* pBin, std::vector<LabeledData>& vtOutLabeled, int nW, int nH, bool bDark)
{
    Mat imgSrc = Mat(nH, nW, CV_8UC1, pSrc);
    Mat imgBin = Mat(nH, nW, CV_8UC1, pBin);
    Mat imgDst;
    Mat imgMul;
    
    if (bDark)
        cv::bitwise_not(imgSrc, imgSrc); // Defect�� 0�� ��� connectedComponent���� Labeling ���� ����

    cv::divide(255, imgBin, imgBin); // 1, 0�� ���� ������ Mask ����
    cv::multiply(imgSrc, imgBin, imgMul, 1.0, CV_8UC1);

    Mat img_labels, stats, centroids;
    
    int numOfLables = connectedComponentsWithStats(imgMul, imgDst, stats, centroids, 8, CV_32S);

    // Dark�� ��� min ���� ã��, Bright ��� Max ���� ã��
    BYTE* pValue = new BYTE[numOfLables - 1];
    memset(pValue, bDark ? 255 : 0, sizeof(BYTE) * (numOfLables - 1));

    for (int i = 0; i < nH * nW; i++)
    {
        int label = imgDst.at<int>(i);
        if (label == 0) continue;

        BYTE val = imgSrc.at<BYTE>(i);
        if (bDark)
        {
            if (val < pValue[label])
            {
                pValue[label] = val;
            }
        }
        else
        {
            if (val > pValue[label])
            {
                pValue[label] = val;
            }
        }
    }
    //ù��° ���� Background Label ��
    for (int j = 1; j < numOfLables; j++) {
        int area = stats.at<int>(j, CC_STAT_AREA);
        int left = stats.at<int>(j, CC_STAT_LEFT);
        int top = stats.at<int>(j, CC_STAT_TOP);
        int width = stats.at<int>(j, CC_STAT_WIDTH);
        int height = stats.at<int>(j, CC_STAT_HEIGHT);
        int right = left + width;
        int bottom = top + height;

        LabeledData data;
        data.bound = { left, top, right, bottom };
        data.center = { (left + right) / 2, (top + bottom) / 2 };
        data.area = area;
        data.value = pValue[j - 1];

        vtOutLabeled.push_back(data);
    }

    //delete[] pValue;

   /* Mat imgColors;

    cvtColor(imgSrc, imgColors, COLOR_GRAY2BGR);

    for (int i = 0; i < imgDst.rows; i++)
    {
        int* label = imgDst.ptr<int>(i);
        Vec3b* pixel = imgColors.ptr<Vec3b>(i);

        for (int j = 0; j < imgDst.cols; j++)
        {
            if (label[j] != 0)
            {
                pixel[j][2] = 255;
                pixel[j][1] = 0;
                pixel[j][0] = 0;
            }
        }
    }

    namedWindow("Labeling window", WINDOW_KEEPRATIO);
    imshow("Labeling window", imgColors);
    cv::waitKey(0)*/;
}
void IP::Labeling(BYTE* pSrc, BYTE* pBin, std::vector<LabeledData>& vtOutLabeled, int nMemW, int nMemH, Point ptLT, Point ptRB, bool bDark)
{
    Mat imgSrc = Mat(nMemH, nMemW, CV_8UC1, pSrc);
    Mat ROI(imgSrc, Rect(ptLT, ptRB));
    Mat imgBin = Mat((ptRB.y - ptLT.y), (ptRB.x - ptLT.x), CV_8UC1, pBin);
    Mat imgCopy = ROI.clone(); // ROI�� ���� Image �̱� ������ ����ó���� ��� ���� �̹��� �Ѽյ�
    Mat imgDst, imgMul;

    if (bDark)
        cv::bitwise_not(ROI, imgCopy); // Defect�� GV�� 0�� ��� connectedComponent���� Labeling ���� ����

    cv::divide(255, imgBin, imgBin); // 1, 0�� ���� ������ Mask ����
    cv::multiply(imgCopy, imgBin, imgMul, 1.0, CV_8UC1);

    Mat img_labels, stats, centroids;

    int numOfLables = connectedComponentsWithStats(imgMul, imgDst, stats, centroids, 8, CV_32S);

    // Dark�� ��� min ���� ã��, Bright ��� Max ���� ã��
    BYTE* pValue = new BYTE[numOfLables - 1];
    memset(pValue, bDark ? 255 : 0, sizeof(BYTE) * (numOfLables - 1));

    for (int i = 0; i < (uint64)(ptRB.x - ptLT.x) * (ptRB.y - ptLT.y); i++)
    {
        int label = imgDst.at<int>(i);
        if (label == 0) continue;

        BYTE val = imgCopy.at<BYTE>(i);
        if (bDark)
        {
            if (val < pValue[label])
            {
                pValue[label] = val;
            }
        }
        else
        {
            if (val > pValue[label])
            {
                pValue[label] = val;
            }
        }
    }
    //ù��° ���� Background Label ��
    for (int j = 1; j < numOfLables; j++) {
        int area = stats.at<int>(j, CC_STAT_AREA);
        int left = stats.at<int>(j, CC_STAT_LEFT);
        int top = stats.at<int>(j, CC_STAT_TOP);
        int width = stats.at<int>(j, CC_STAT_WIDTH);
        int height = stats.at<int>(j, CC_STAT_HEIGHT);
        int right = left + width;
        int bottom = top + height;

        LabeledData data;
        data.bound = { left, top, right, bottom };
        data.center = { (left + right) / 2, (top + bottom) / 2 };
        data.area = area;
        data.value = pValue[j - 1];

        vtOutLabeled.push_back(data);
    }
}

float IP::TemplateMatching(BYTE* pSrc, BYTE* pTemp, Point& outMatchPoint, int nMemW, int nMemH, int nTempW, int nTempH, Point ptLT, Point ptRB, int method)
{
    Mat imgSrc = Mat(nMemH, nMemW, CV_8UC1, pSrc);
    Mat ROI(imgSrc, Rect(ptLT, ptRB));
    Mat imgTemp = Mat(nTempH, nTempW, CV_8UC1, pTemp);

    Mat result;

    double minVal, maxVal;
    Point minLoc, maxLoc;

    matchTemplate(ROI, imgTemp, result, method);

    minMaxLoc(result, NULL, &maxVal, NULL, &outMatchPoint); // �Ϻ��ϰ� ��Ī�� ��� 1

    return maxVal * 100; // Matching Score
}

void IP::GaussianBlur(BYTE* pSrc, BYTE* pDst, int nW, int nH, int nSigma)
{
    Mat imgSrc = Mat(nH, nW, CV_8UC1, pSrc);
    Mat imgDst = Mat(nH, nW, CV_8UC1, pDst);

    cv::GaussianBlur(imgSrc, imgDst, Size(nSigma * 6 + 1, nSigma * 6 + 1), nSigma);
}

void IP::MedianBlur(BYTE* pSrc, BYTE* pDst, int nW, int nH, int FilterSz)
{
    Mat imgSrc = Mat(nH, nW, CV_8UC1, pSrc);
    Mat imgDst = Mat(nH, nW, CV_8UC1, pDst);

    cv::medianBlur(imgSrc, imgDst, FilterSz);
}

void IP::Morphology(BYTE* pSrc, BYTE* pDst, int nW, int nH, int nFilterSz, std::string strMethod, int nIter)
{
    Mat dirElement(nFilterSz, nFilterSz, CV_8U, cv::Scalar(1)); // �ϴ� ������ Morph, ���� 4���� ����
    Mat imgSrc = Mat(nH, nW, CV_8UC1, pSrc);
    Mat imgDst = Mat(nH, nW, CV_8UC1, pDst);

    if (strMethod == String("erode") || strMethod == String("Erode") || strMethod == String("ERODE"))
    {
        cv::erode(imgSrc, imgDst, dirElement, cv::Point(-1, -1), nIter);
    }
    else if (strMethod == String("dilate") || strMethod == String("Dilate") || strMethod == String("DILATE"))
    {
        cv::dilate(imgSrc, imgDst, dirElement, cv::Point(-1, -1), nIter);
    }
    else if (strMethod == String("open") || strMethod == String("Open") || strMethod == String("OPEN"))
    {
        cv::morphologyEx(imgSrc, imgDst, cv::MORPH_OPEN, dirElement, cv::Point(-1, -1), nIter);
    }
    else if (strMethod == String("close") || strMethod == String("Close") || strMethod == String("CLOSE"))
    {
        cv::morphologyEx(imgSrc, imgDst, cv::MORPH_CLOSE, dirElement, cv::Point(-1, -1), nIter);
    }
}

