
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
        data.centerX = (left + right) / 2;
        data.centerY = (top + bottom) / 2;
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

// ********* Inspection *********
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
    int64 roiW = (ptRB.x - (int64)ptLT.x);
    int64 roiH = (ptRB.y - (int64)ptLT.y);

    PBYTE imgROI = new BYTE[roiW * roiH];
    for (int64 r = ptLT.y; r < ptRB.y; r++)
    {
        BYTE* pImg = &pSrc[r * nMemW + ptLT.x];
        memcpy(&imgROI[roiW * (r - (int64)ptLT.y)], pImg, roiW);
    }
    Mat imgSrc = Mat(roiH, roiW, CV_8UC1, imgROI);
    Mat imgDst = Mat(roiH, roiW, CV_8UC1, pDst);

    if (bDark)
        cv::threshold(imgSrc, imgDst, thresh, 255, CV_THRESH_BINARY_INV);
    else
        cv::threshold(imgSrc, imgDst, thresh, 255, CV_THRESH_BINARY);
}

float IP::Average(BYTE* pSrc, int nW, int nH)
{
    Mat imgSrc = Mat(nH, nW, CV_8UC1, pSrc);

    return (cv::sum(cv::sum(imgSrc)))[0] / ((uint64)nW * nH); // mean �Լ��� return Type = Scalar
}
float IP::Average(BYTE* pSrc, int nMemW, int nMemH, Point ptLT, Point ptRB)
{
    uint64 roiW = (ptRB.x - (int64)ptLT.x);
    uint64 roiH = (ptRB.y - (int64)ptLT.y);

    PBYTE imgROI = new BYTE[roiW * roiH];
    for (int64 r = ptLT.y; r < ptRB.y; r++)
    {
        BYTE* pImg = &pSrc[r * nMemW + ptLT.x];
        memcpy(&imgROI[roiW * (r - (int64)ptLT.y)], pImg, roiW);
    }

    Mat imgSrc = Mat(roiH, roiW, CV_8UC1, imgROI);

    return (cv::sum(cv::sum(imgSrc)))[0] / (roiW * roiH); // mean �Լ��� return Type = Scalar

}

void IP::Labeling(BYTE* pSrc, BYTE* pBin, std::vector<LabeledData>& vtOutLabeled, int nW, int nH, bool bDark)
{
    Mat imgSrc = Mat(nH, nW, CV_8UC1, pSrc);
    Mat imgBin = Mat(nH, nW, CV_8UC1, pBin);
    Mat imgMask;

    cv::bitwise_and(imgSrc, imgBin, imgMask);

    std::vector<std::vector<Point>> contours;
    std::vector<Vec4i> hierarchy;
    cv::findContours(imgBin, contours, hierarchy, CV_RETR_TREE, CV_CHAIN_APPROX_SIMPLE, Point(0, 0));

    Rect bounding_rect;
    for (int i = 0; i < contours.size(); i++) // iterate through each contour. 
    {
        if (hierarchy[i][3] != -1) // Parent Contour�� ���� ��� Pass!
            continue;

        bounding_rect = boundingRect(contours[i]);

        // Defect�� GV���ϴ� �κ� (�켱�� Min, Max)
        Mat defectROI = imgMask(bounding_rect);
        Mat defectMask = imgBin(bounding_rect);

        double area = countNonZero(defectMask);

        double min, max;
        minMaxIdx(defectROI, &min, &max, NULL, NULL, defectMask);

        LabeledData data;
        data.bound = { bounding_rect.x, bounding_rect.y, bounding_rect.x + bounding_rect.width, bounding_rect.y + bounding_rect.height };
        data.width = bounding_rect.width;
        data.height = bounding_rect.height;
        data.centerX = bounding_rect.x + bounding_rect.width / 2;
        data.centerY = bounding_rect.y + bounding_rect.height / 2;
        data.area = area;
        data.value = (bDark) ? min : max;
        vtOutLabeled.push_back(data);
    }
}
void IP::Labeling_SubPix(BYTE* pSrc, BYTE* pBin, std::vector<LabeledData>& vtOutLabeled, int nW, int nH, bool bDark, int thresh, float Scale)
{
    Mat imgSrc = Mat(nH, nW, CV_8UC1, pSrc);
    Mat imgBin = Mat(nH, nW, CV_8UC1, pBin);

    std::vector<std::vector<Point>> contours;
    std::vector<Vec4i> hierarchy;
    cv::findContours(imgBin, contours, hierarchy, CV_RETR_TREE, CV_CHAIN_APPROX_SIMPLE, Point(0, 0));

    Rect bounding_rect;
    Mat Up_ScaleImg;
    std::vector<std::vector<Point>> Up_contours;
    std::vector<Vec4i> Up_hierarchy;

    for (int i = 0; i < contours.size(); i++) // iterate through each contour. 
    {
        if (hierarchy[i][3] != -1) // Parent Contour�� ���� ��� Pass!
            continue;

        bounding_rect = boundingRect(contours[i]);

        Rect resize_rect = bounding_rect;
        resize_rect.x -= (resize_rect.x > 1) ? 1 : resize_rect.x;
        resize_rect.y -= (resize_rect.y > 1) ? 1 : resize_rect.y;
        resize_rect.width += (resize_rect.x + resize_rect.width + 2 > nW) ? (resize_rect.x > 1) ? 1 : 0 : (resize_rect.x > 1) ? 2 : 1;
        resize_rect.height += (resize_rect.y + resize_rect.height + 2 > nH) ? (resize_rect.y > 1) ? 1 : 0 : (resize_rect.y > 1) ? 2 : 1;

        Mat defectROI = imgSrc(resize_rect).clone();
        resize(defectROI, Up_ScaleImg, Size(defectROI.size().width * Scale, defectROI.size().height * Scale));

        Mat Up_defectMask;
        if (bDark)
            cv::threshold(Up_ScaleImg, Up_defectMask, thresh, 255, CV_THRESH_BINARY_INV);
        else
            cv::threshold(Up_ScaleImg, Up_defectMask, thresh, 255, CV_THRESH_BINARY);


        cv::findContours(Up_defectMask, Up_contours, Up_hierarchy, CV_RETR_TREE, CV_CHAIN_APPROX_SIMPLE, Point(0, 0));

        Rect Up_bounding_rect;
        for (int i = 0; i < Up_contours.size(); i++) // iterate through each contour. 
        {
            if (Up_hierarchy[i][3] != -1) // Parent Contour�� ���� ��� Pass!
                continue;

            Up_bounding_rect = boundingRect(Up_contours[i]);
        }

        double defectX = bounding_rect.x + 1 + Up_bounding_rect.x / Scale + 1;
        double defectY = bounding_rect.y + 1 + Up_bounding_rect.y / Scale + 1;
        //double area = countNonZero(defectMask); // Pixel ���� 
        double size = (Up_bounding_rect.width > Up_bounding_rect.height) ? Up_bounding_rect.width : Up_bounding_rect.height; // Ÿ ȸ����� Defect Size�� ������ ���̷� �Ѵٰ� ��...

        double min, max;
        minMaxIdx(Up_ScaleImg, &min, &max, NULL, NULL, Up_defectMask);

        LabeledData data;
        data.bound = { bounding_rect.x, bounding_rect.y, bounding_rect.x + bounding_rect.width, bounding_rect.y + bounding_rect.height };
        data.centerX = (defectX + Up_bounding_rect.width / Scale / 2);
        data.centerY = (defectY + Up_bounding_rect.height / Scale / 2);
        data.value = (bDark) ? min : max;
        // Defect GV�� ���� size�� �Ҽ��� ���
        data.width = round((Up_bounding_rect.width / Scale + abs(128 - data.value) / 255) * 100) / 100.0;
        data.height = round((Up_bounding_rect.height / Scale + abs(128 - data.value) / 255) * 100) / 100.0;
        data.area = round((size / Scale + abs(128 - data.value) / 255) * 100) / 100.0;

        vtOutLabeled.push_back(data);
    }
}

// Position
float IP::TemplateMatching(BYTE* pSrc, BYTE* pTemp, Point& outMatchPoint, int nMemW, int nMemH, int nTempW, int nTempH, Point ptLT, Point ptRB, int method, int nByteCnt)
{
    int64 roiW = (ptRB.x - (int64)ptLT.x);
    int64 roiH = (ptRB.y - (int64)ptLT.y);

    PBYTE imgROI = new BYTE[roiW * roiH];
    for (int64 r = ptLT.y; r < ptRB.y; r++)
    {
        BYTE* pImg = &pSrc[r * nMemW + ptLT.x];
        memcpy(&imgROI[roiW * (r - (int64)ptLT.y)], pImg, roiW);
    }

    Mat imgSrc = Mat(roiH, roiW, CV_8UC1, imgROI);
    Mat imgTemp;
    double chMax = 0;

    if (nByteCnt == 1)
    {
        imgTemp = Mat(nTempH, nTempW, CV_8UC1, pTemp);

        Mat result;

        Point minLoc, maxLoc;

		matchTemplate(imgSrc, imgTemp, result, method);

		minMaxLoc(result, NULL, &chMax, NULL, &outMatchPoint); // �Ϻ��ϰ� ��Ī�� ��� 1
    }
    else if (nByteCnt == 3)
    {
        imgTemp = Mat(nTempH, nTempW, CV_8UC3, pTemp);

        Mat bgr[3];
        split(imgTemp, bgr);

        Mat result;

        double maxVal;
        Point minLoc, maxLoc;

        for (int i = 0; i < 3; i++)
        {
            matchTemplate(imgSrc, bgr[i], result, method);

            minMaxLoc(result, NULL, &maxVal, NULL, &outMatchPoint); // �Ϻ��ϰ� ��Ī�� ��� 1

            if (maxVal > chMax)
                chMax = maxVal;
        }
    }


    return chMax * 100; // Matching Score
}

// D2D 
void IP::SubtractAbs(BYTE* pSrc1, BYTE* pSrc2, BYTE* pDst, int nW, int nH)
{
    Mat imgSrc1 = Mat(nH, nW, CV_8UC1, pSrc1);
    Mat imgSrc2 = Mat(nH, nW, CV_8UC1, pSrc2);

    Mat imgDst = Mat(nH, nW, CV_8UC1, pDst);

    cv::absdiff(imgSrc1, imgSrc2, imgDst);
}
void IP::SelectMinDiffinArea(BYTE* pSrc1, BYTE** pSrc2, BYTE* pDst, int imgNum, int nW, int nH, int nAreaSize)
{
    Mat imgSrc1 = Mat(nH, nW, CV_8UC1, pSrc1);
    Mat imgSrcROI1 = imgSrc1(Rect(nAreaSize, nAreaSize, nW - nAreaSize * 2, nH - nAreaSize * 2));

    Mat imgDst = Mat(nH, nW, CV_8UC1, pDst);
    Mat imgDstROI = imgDst(Rect(nAreaSize, nAreaSize, nW - nAreaSize * 2, nH - nAreaSize * 2));
    cv::add(imgDstROI, Scalar(255), imgDstROI);
    Mat Diff;
    for (int i = 0; i < imgNum; i++)
    {
        Mat imgSrc2 = Mat(nH, nW, CV_8UC1, pSrc2[i]);

        for (int r = -nAreaSize; r <= nAreaSize; r++)
        {
            for (int c = -nAreaSize; c <= nAreaSize; c++)
            {
                Mat imgSrcROI2 = imgSrc2(Rect(nAreaSize + r, nAreaSize + c, nW - nAreaSize * 2, nH - nAreaSize * 2));
                cv::absdiff(imgSrcROI1, imgSrcROI2, Diff);
                //Min�� ����
                (cv::min)(imgDstROI, Diff, imgDstROI);
            }
        }
    }
}
Point IP::FindMinDiffLoc(BYTE* pSrc, BYTE* pInOutTarget, int nTargetW, int nTargetH, int nTrigger)
{
    Mat imgSrc = Mat(nTargetH + (nTrigger * 2), nTargetW + (nTrigger * 2), CV_8UC1, pSrc);
    Mat imgTarget = Mat(nTargetH, nTargetW, CV_8UC1, pInOutTarget);
    Mat imgDiff;

    Point Trans;
    int minVal = 999;

    for (int i = -nTrigger; i <= nTrigger; i++)
    {
        for (int j = -nTrigger; j <= nTrigger; j++)
        {
            Mat roi = imgSrc(Rect((nTrigger * 2) / 2 + j, (nTrigger * 2) / 2 + i, nTargetW, nTargetH));
            cv::absdiff(roi, imgTarget, imgDiff);
            int nDiffSum = (cv::sum(cv::sum(imgDiff)))[0] / ((uint64)nTargetW * nTargetH);

            if (nDiffSum < minVal)
            {
                minVal = nDiffSum;
                Trans.x = j;
                Trans.y = i;
            }
        }
    }
    Mat roi = imgSrc(Rect((nTrigger * 2) / 2 + Trans.y, (nTrigger * 2) / 2 + Trans.x, nTargetW, nTargetH));
    imgTarget = roi.clone();

    return Trans;
}

// Create Golden Image
void IP::CreateGoldenImage_Avg(BYTE** pSrc, BYTE* pDst, int imgNum, int nW, int nH)
{
    Mat imgAccumlate = Mat::zeros(nH, nW, CV_16UC1);
    Mat imgDst = Mat(nH, nW, CV_8UC1, pDst);

    for (int i = 0; i < imgNum; i++)
    {
        Mat imgSrc = Mat(nH, nW, CV_8UC1, pSrc[i]);
        imgSrc.convertTo(imgSrc, CV_16UC1);
        imgAccumlate = imgAccumlate + imgSrc;
    }

    imgAccumlate.convertTo(imgDst, CV_8UC1, 1. / imgNum);
}
void IP::CreateGoldenImage_NearAvg(BYTE** pSrc, BYTE* pDst, int imgNum, int nW, int nH)
{
    Mat imgAccumlate = Mat::zeros(nH, nW, CV_16UC1);
    Mat imgDst = Mat(nH, nW, CV_8UC1, pDst);
    Mat imgAvg;
    for (int i = 0; i < imgNum; i++)
    {
        Mat imgSrc = Mat(nH, nW, CV_8UC1, pSrc[i]);
        imgSrc.convertTo(imgSrc, CV_16UC1);
    }

    imgAccumlate.convertTo(imgAvg, CV_8UC1, 1. / imgNum);

    Mat imgSrc = Mat(nH, nW, CV_8UC1, pSrc[0]);
    // Mean�� ���� ����� �� ����
    imgSrc.copyTo(imgDst);
    Mat diff1, diff2, minDiff;
    for (int i = 1; i < imgNum; i++) {
        // result - avgImg �� new Image - avgImg �� �� �� Diff�� �� ���� �ȼ��鸸 ������Ʈ
        imgSrc = Mat(nH, nW, CV_8UC1, pSrc[i]);
        cv::absdiff(imgAvg, imgSrc, diff1);
        cv::absdiff(imgAvg, imgDst, diff2);

        // minDiff 0 : diff1 < diff2 // 255 : diff1 > diff2

        cv::subtract(diff1, diff2, minDiff);
        cv::threshold(minDiff, minDiff, 1, 255, CV_THRESH_BINARY);
        // Get the old pixels that are still ok
        cv::bitwise_and(imgDst, minDiff, imgDst);
        // Get the new pixels
        cv::bitwise_or(imgDst, imgSrc & ~minDiff, imgDst);
    }
}
void IP::CreateGoldenImage_MedianAvg(BYTE** pSrc, BYTE* pDst, int imgNum, int nW, int nH)
{
    Mat imgAccumlate = Mat::zeros(nH, nW, CV_16UC1);
    Mat imgDst = Mat(nH, nW, CV_8UC1, pDst);
    Mat imgSrc;
    if (imgNum <= 4)
    {
        imgSrc = Mat(nH, nW, CV_8UC1, pSrc[0]);
        imgSrc.convertTo(imgSrc, CV_16UC1);

        Mat minImg = imgSrc.clone();
        Mat maxImg = imgSrc.clone();

        imgAccumlate = imgAccumlate + minImg;// pSrc[0]

        for (int i = 1; i < imgNum; i++)
        {
            imgSrc = Mat(nH, nW, CV_8UC1, pSrc[i]);
            imgSrc.convertTo(imgSrc, CV_16UC1);

            (cv::min)(imgSrc, minImg, minImg);
            (cv::max)(imgSrc, maxImg, maxImg);

            imgAccumlate = imgAccumlate + imgSrc;
        }

        cv::subtract(imgAccumlate, minImg, imgAccumlate);
        cv::subtract(imgAccumlate, maxImg, imgAccumlate);

        imgAccumlate.convertTo(imgDst, CV_8UC1, 1. / (imgNum - 2));
    }
    else
    {
        imgSrc = Mat(nH, nW, CV_8UC1, pSrc[0]);
        imgSrc.convertTo(imgSrc, CV_16UC1);

        Mat minImg = imgSrc.clone();
        Mat maxImg = imgSrc.clone();

        imgAccumlate = imgAccumlate + minImg;// pSrc[0]

        for (int cnt = 0; cnt < imgNum / 3; cnt++)
        {
            for (int i = cnt * 3; i < cnt * 3 + 3; i++)
            {
                imgSrc = Mat(nH, nW, CV_8UC1, pSrc[i]);
                imgSrc.convertTo(imgSrc, CV_16UC1);

                (cv::min)(imgSrc, minImg, minImg);
                (cv::max)(imgSrc, maxImg, maxImg);

                imgAccumlate = imgAccumlate + imgSrc;

                cv::subtract(imgAccumlate, minImg, imgAccumlate);
                cv::subtract(imgAccumlate, maxImg, imgAccumlate);
            }
        }

        imgAccumlate.convertTo(imgDst, CV_8UC1, 1. / (imgNum / 3));
    }
}
void IP::CreateGoldenImage_Median(BYTE** pSrc, BYTE* pDst, int imgNum, int nW, int nH)
{
    Mat imgAccumlate = Mat::zeros(nH, nW, CV_16UC1);
    Mat imgDst = Mat(nH, nW, CV_8UC1, pDst);
    Mat imgSrc;

    Mat minImg;
    Mat maxImg;

    for (int i = 2; i < imgNum; i++)
    {
        if (i == 2)
        {
            imgSrc = Mat(nH, nW, CV_8UC1, pSrc[0]);
            imgSrc.convertTo(imgSrc, CV_16UC1);

            minImg = imgSrc.clone();
            maxImg = imgSrc.clone();

            imgAccumlate = imgAccumlate + minImg;
        }
        else
        {
            minImg = imgAccumlate.clone();
            maxImg = imgAccumlate.clone();
        }  

        for (int j = i - 2; j < i; j++)
        {
            
            imgSrc = Mat(nH, nW, CV_8UC1, pSrc[j]);
            imgSrc.convertTo(imgSrc, CV_16UC1);

            (cv::min)(imgSrc, minImg, minImg);
            (cv::max)(imgSrc, maxImg, maxImg);

            imgAccumlate = imgAccumlate + imgSrc;
        }

        cv::subtract(imgAccumlate, minImg, imgAccumlate);
        cv::subtract(imgAccumlate, maxImg, imgAccumlate);
    }
    imgAccumlate.convertTo(imgDst, CV_8UC1);
}

// D2D 3.0
void IP::CreateDiffScaleMap(BYTE* pSrc, float* pDst, int nW, int nH, int nEdgeSuppressionLev, int nBrightSuppressionLev)
{
    Mat imgSrc = Mat(nH, nW, CV_8UC1, pSrc);
    Mat imgDst = Mat(nH, nW, CV_32FC1, pDst);
    Mat fImgSrc;
    Mat imgMax, imgMin;
    Mat imgEdgeScale, imgBrightScale;

    imgSrc.convertTo(fImgSrc, CV_32FC1, 1 / 255.0);

    if (nEdgeSuppressionLev > 0)
    {
        nEdgeSuppressionLev = (nEdgeSuppressionLev > 10) ? 10 : nEdgeSuppressionLev;
        // Create Edge Scale Map
        Mat dirElement(3, 3, CV_8U, cv::Scalar(1));
        cv::dilate(fImgSrc, imgMax, dirElement, cv::Point(-1, -1), 2);
        cv::erode(fImgSrc, imgMin, dirElement, cv::Point(-1, -1), 2);

        cv::absdiff(imgMax, imgMin, imgEdgeScale);
        cv::multiply(Scalar(1.0 / (11 - nEdgeSuppressionLev)), imgEdgeScale, imgEdgeScale);
        cv::subtract(Scalar(1.0), imgEdgeScale, imgEdgeScale);
    }

    // Create Bright Scale Map
    if (nBrightSuppressionLev > 0)
    {
        nBrightSuppressionLev = (nBrightSuppressionLev > 10) ? 10 : nBrightSuppressionLev;
        // Create Edge Scale Map
        cv::subtract(fImgSrc, Scalar(0.5), imgBrightScale);
        cv::threshold(imgBrightScale, imgBrightScale, 0, 1, CV_THRESH_TOZERO);
        cv::multiply(imgBrightScale, Scalar(1.0 / (11 - nBrightSuppressionLev)), imgBrightScale);
        cv::subtract(Scalar(1.0), imgBrightScale, imgBrightScale); // (0.5 < x) => 1 , (1 ~ 0.5) => (0.5 ~ 1)  
    }

    if (nBrightSuppressionLev <= 0)
        imgEdgeScale.copyTo(imgDst);

    else if (nEdgeSuppressionLev <= 0)
        imgEdgeScale.copyTo(imgDst);

    else // �� �̹��� �� �� ���� ����ġ�� ����
    {
        Mat minImg = Mat(nH, nW, CV_32FC1);
        (cv::min)(imgEdgeScale, imgBrightScale, minImg);
        minImg.copyTo(imgDst);
    }
}
void IP::CreateHistogramWeightMap(BYTE* pSrc, BYTE* pGolden, float* pDst, int nW, int nH, int nWeightLev)
{
    Mat imgSrc = Mat(nH, nW, CV_8UC1, pSrc);
    Mat imgGolden = Mat(nH, nW, CV_8UC1, pGolden);
    Mat imgGolden_blur;

    cv::GaussianBlur(imgGolden, imgGolden_blur, Size(7, 7), 1);

    int histSize = 256;
    float range[] = { 0, 256 }; //the upper boundary is exclusive
    const float* histRange = { range };

    bool uniform = true, accumulate = false;
    Mat src_hist, golden_hist;
    calcHist(&imgSrc, 1, 0, Mat(), src_hist, 1, &histSize, &histRange, uniform, accumulate);
    calcHist(&imgGolden, 1, 0, Mat(), golden_hist, 1, &histSize, &histRange, uniform, accumulate);

    float* gold_data = (float*)golden_hist.data;
    float* cur_data = (float*)src_hist.data;
    bool histWight[256] = { false, };

    for (int i = 0; i < 255; i++)
        if ((gold_data[i] == 0) && (cur_data[i] != 0)) // Current Image������ �����ϴ� Pixel �����鿡 ���� ����ġ �ο�
            histWight[i] = true;

    Mat imgDst = Mat(nH, nW, CV_32FC1, pDst);

    Mat weightMap = Mat::zeros(nH, nW, CV_8UC1);
    Mat temp = Mat::zeros(nH, nW, CV_8UC1);
    for (int i = 0; i < 255; i++)
    {
        if (histWight[i])
        {
            temp = (imgSrc == i);
            bitwise_or(weightMap, temp, weightMap);
        }
    }
    weightMap.convertTo(imgDst, CV_32FC1, (1 + nWeightLev / 5.0) / 255.0);
    cv::add(imgDst, Scalar(1), imgDst);
}

// Elemetwise Operation
void IP::Multiply(BYTE* pSrc1, float* pSrc2, BYTE* pDst, int nW, int nH)
{
    Mat imgSrc1 = Mat(nH, nW, CV_8UC1, pSrc1);
    Mat imgSrc2 = Mat(nH, nW, CV_32FC1, pSrc2);
    Mat imgDst = Mat(nH, nW, CV_8UC1, pDst);

    Mat fImgSrc1;
    imgSrc1.convertTo(fImgSrc1, CV_32FC1);

    cv::multiply(fImgSrc1, imgSrc2, fImgSrc1);
    fImgSrc1.convertTo(imgDst, CV_8UC1);
}

// Filtering
void IP::GaussianBlur(BYTE* pSrc, BYTE* pDst, int nW, int nH, int nSigma)
{
    Mat imgSrc = Mat(nH, nW, CV_8UC1, pSrc);
    Mat imgDst = Mat(nH, nW, CV_8UC1, pDst);

    cv::GaussianBlur(imgSrc, imgDst, Size(nSigma * 6 + 1, nSigma * 6 + 1), nSigma);
}
void IP::AverageBlur(BYTE* pSrc, BYTE* pDst, int nW, int nH)
{
    Mat imgSrc = Mat(nH, nW, CV_8UC1, pSrc);
    Mat imgDst = Mat(nH, nW, CV_8UC1, pDst);

    cv::boxFilter(imgSrc, imgDst, CV_8UC1, Size(3, 3));
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

// BackSide
std::vector<Point> IP::FindWaferEdge(BYTE* pSrc, float& inoutCenterX, float& inoutCenterY, float& inoutRadius, int nW, int nH, int downScale)
{
    Mat imgSrc = Mat(nH, nW, CV_8UC1, pSrc);
    Mat imgSubSample;

    if (inoutCenterX == 0 || inoutCenterY == 0)
    {
        inoutCenterX = nW / 2;
        inoutCenterY = nH / 2;
    }

    inoutCenterX /= downScale;
    inoutCenterY /= downScale;
    inoutRadius /= downScale;

    if (downScale != 1)
        cv::resize(imgSrc, imgSubSample, Size(nW / downScale, nH / downScale));
    else
        imgSubSample = imgSrc.clone();

    // 0. CenterPoint�� �������� nRadius �� �����Ͽ� ������ �� ����
    Mat CircleMask = Mat(nH / downScale, nW / downScale, CV_8UC1);
    CircleMask = Scalar(0);

    circle(CircleMask, Point(inoutCenterX, inoutCenterY), inoutRadius, 255, -1);

    // 1. Wafer �߽��� ���� ���� ��� ������ �ĺ� ���� ����
    Mat CenterROI(imgSubSample, Rect(inoutCenterX - 50, inoutCenterY - 50, 100, 100));
    float avg = (cv::sum(cv::sum(CenterROI)))[0] / ((uint64)100 * 100) * 0.8f;
    cv::threshold(imgSubSample, imgSubSample, avg, 255, CV_THRESH_BINARY);
    cv::bitwise_and(imgSubSample, CircleMask, imgSubSample);

    // 2. Contour�� ������ ������� Wafer�� �ش��ϴ� Contour Detect
    std::vector<std::vector<Point>> contours;
    std::vector<Vec4i> hierarchy;
    findContours(imgSubSample, contours, hierarchy, CV_RETR_TREE, CV_CHAIN_APPROX_SIMPLE, Point(0, 0));

    int largest_area = 0; // Largest_Area �׽�Ʈ �غ��� ���������� ������ r*r*PI ������ ���� ������ �������� ����
    int largest_contour_index = 0;

    for (int i = 0; i < contours.size(); i++) // iterate through each contour. 
    {
        double a = contourArea(contours[i], false);  //  Find the area of contour
        if (a > largest_area) {
            largest_area = a;
            largest_contour_index = i;                //Store the index of largest contour
        }
    }

    // 3. ������ WF Image�� 
    for (int i = 0; i < contours[largest_contour_index].size(); i++)
    {
        contours[largest_contour_index][i].x *= downScale;
        contours[largest_contour_index][i].y *= downScale;
    }

    // W, H �� ª�� ���� ���������� ��� -> Defect ���Ž� Ȱ��
    Rect bounding_rect;
    bounding_rect = boundingRect(contours[largest_contour_index]);

    inoutCenterX = bounding_rect.x + bounding_rect.width / 2;
    inoutCenterY = bounding_rect.y + bounding_rect.height / 2;

    inoutRadius = (bounding_rect.height < bounding_rect.width) ? bounding_rect.height : bounding_rect.width;
    inoutRadius = inoutRadius / 2;

    return contours[largest_contour_index];
}
std::vector<byte> IP::GenerateMapData(std::vector<Point> vtContour, float& outOriginX, float& outOriginY, float& outChipSzX, float& outChipSzY, int& outMapX, int& outMapY, int nW, int nH, int downScale, bool isIncludeMode)
{
    Mat GridMapMask = Mat(nH / downScale, nW / downScale, CV_8UC1);
    GridMapMask = Scalar(0);

    std::vector<std::vector<Point>> contours;
    contours.push_back(vtContour);

    cv::drawContours(GridMapMask, contours, 0, 255, -1);

    Rect bounding_rect;
    bounding_rect = boundingRect(contours[0]);

    int mapX = outMapX;
    int mapY = outMapY;
    int nChipSzX = (bounding_rect.width / outMapX) + 1;
    int nChipSzY = (bounding_rect.height / outMapY) + 1;
    if ((bounding_rect.width - nChipSzX * outMapX) > nChipSzX)
        outMapX = mapX = bounding_rect.width / nChipSzX;

    if ((bounding_rect.height - nChipSzY * outMapY) > nChipSzY)
        outMapY = mapY = bounding_rect.width / nChipSzY;

    outChipSzX = nChipSzX * downScale;
    outChipSzY = nChipSzY * downScale;

    std::vector<byte> pMapData;// ; [nMapX * nMapY] ;
    std::vector<byte> pMapData_Trans;

    bool isOrigin = true;
    for (int c = 0; c < mapX; c++) {
        int checkEmptyLine = 0;
        for (int r = 0; r < mapY; r++) {
            bool condition = (isIncludeMode) ? countNonZero(Mat(GridMapMask, Rect(c * nChipSzX + bounding_rect.x, r * nChipSzY + bounding_rect.y, nChipSzX, nChipSzY))) // WF ������ �����̶� ������ Map�� �߰�
                : countNonZero(Mat(GridMapMask, Rect(c * nChipSzX + bounding_rect.x, r * nChipSzY + bounding_rect.y, nChipSzX, nChipSzY))) == (nChipSzX * nChipSzY);

            if (condition)
            {
                if (isOrigin)
                {
                    //circle(GridMapMask, Point(c * nChipSzX + bounding_rect.x, r * nChipSzY + nChipSzY + bounding_rect.y), 10, 125, -1); // Debug > Image Watch
                    outOriginX = (c * nChipSzX + bounding_rect.x) * downScale;
                    outOriginY = (r * nChipSzY + nChipSzY + bounding_rect.y) * downScale; // ��'��'��
                    isOrigin = false;
                }

                pMapData.push_back(1);
                //rectangle(GridMapMask, Rect(c * nChipSzX + bounding_rect.x, r * nChipSzY + bounding_rect.y, nChipSzX, nChipSzY), 125, 1); // Debug > Image Watch
            }
            else
            {
                pMapData.push_back(0);
                checkEmptyLine++;
            }
        }
        if (checkEmptyLine == mapY) // chip�� ���� �� ������ ��� ����
        {
            pMapData.erase(pMapData.end() - mapY, pMapData.end());
            outMapX--;
        }
    }
    mapY = outMapY;
    for (int r = 0; r < mapY; r++) // Transpose
    {
        int checkEmptyLine = 0;
        for (int c = 0; c < outMapX; c++)
        {
            pMapData_Trans.push_back(pMapData[c * mapY + r]);
            if (pMapData[c * mapY + r] == 0)
                checkEmptyLine++;
        }
        if (checkEmptyLine == outMapX)
        {
            pMapData_Trans.erase(pMapData_Trans.end() - outMapX, pMapData_Trans.end());
            outMapY--;
        }
    }

    return pMapData_Trans;
}
std::vector<byte> IP::GenerateMapData(std::vector<Point> vtContour, float& outOriginX, float& outOriginY, int& outMapX, int& outMapY, int nW, int nH, int downScale, bool isIncludeMode)
{
    Mat GridMapMask = Mat(nH / downScale, nW / downScale, CV_8UC1);
    GridMapMask = Scalar(0);

    std::vector<std::vector<Point>> contours;
    contours.push_back(vtContour);

    cv::drawContours(GridMapMask, contours, 0, 255, -1);

    Rect bounding_rect;
    bounding_rect = boundingRect(contours[0]);

    int mapX = outMapX;
    int mapY = outMapY;

    float nChipSz = 1000.0 / downScale;
    outMapX = ceil(bounding_rect.width / nChipSz); // Backside Inspection Size 1000 x 1000
    outMapY = ceil(bounding_rect.height / nChipSz); // 100 * downScale = 1000

    std::vector<byte> pMapData;// ; [nMapX * nMapY] ;
    std::vector<byte> pMapData_Trans;

    bool isOrigin = true;
    for (int c = 0; c < mapX; c++) {
        for (int r = 0; r < mapY; r++) {
            bool condition = (isIncludeMode) ? countNonZero(Mat(GridMapMask, Rect(c * nChipSz + bounding_rect.x, r * nChipSz + bounding_rect.y, nChipSz, nChipSz)))
                : countNonZero(Mat(GridMapMask, Rect(c * nChipSz + bounding_rect.x, r * nChipSz + bounding_rect.y, nChipSz, nChipSz))) == (nChipSz * nChipSz);

            if (condition)
            {
                if (isOrigin)
                {
                    //circle(GridMapMask, Point(c * nChipSz, r * nChipSz + nChipSz), 10, 125, -1); // Debug > Image Watch
                    if (r != 0) // ���� ù ���� ������ Origin�� �־����
                    {
                        pMapData.erase(pMapData.begin(), pMapData.begin() + mapY * c);
                        outMapX -= c;
                    }
                    outOriginX = (c * nChipSz + bounding_rect.x) * downScale;
                    outOriginY = (r * nChipSz + nChipSz + bounding_rect.y) * downScale; // ��'��'��
                    isOrigin = false;
                }

                pMapData.push_back(1);
                //rectangle(GridMapMask, Rect(c * nChipSz, r * nChipSz, nChipSz, nChipSz), 125, 1); // Debug > Image Watch
            }
            else
                pMapData.push_back(0);
        }
    }

    for (int r = 0; r < outMapY; r++) // Transpose
        for (int c = 0; c < outMapX; c++)
            pMapData_Trans.push_back(pMapData[c * outMapY + r]);

    return pMapData_Trans;
}

// Image(Feature/Defect Image) Load/Save - (���� C#������ �÷��� �� ��) 
void IP::SaveBMP(String sFilePath, BYTE* pSrc, int nW, int nH, int nByteCnt)
{
    if (nByteCnt == 1)
    {
        Mat imgSrc = Mat(nH, nW, CV_8UC1, pSrc);
        cv::imwrite(sFilePath, imgSrc);
    }
    else if (nByteCnt == 3)
    {
        Mat imgSrc = Mat(nH, nW / 3, CV_8UC3, pSrc);
        cv::cvtColor(imgSrc, imgSrc, CV_RGB2BGR);
        cv::imwrite(sFilePath, imgSrc);
    }
}
void IP::SaveDefectListBMP(String sFilePath, BYTE* pSrc, int nW, int nH, std::vector<Rect> DefectRect)
{
    int saveSzW = 640;
    int saveSzH = 480;

    for (int i = 0; i < DefectRect.size(); i++)
    {
        std::string Path = sFilePath;
        int rectCenterX = DefectRect[i].x + DefectRect[i].width / 2;
        int rectCenterY = DefectRect[i].y + DefectRect[i].height / 2;

        if (DefectRect[i].x < saveSzW && DefectRect[i].y < saveSzH)
        {
            DefectRect[i].x = (rectCenterX - saveSzW / 2);
            DefectRect[i].y = (rectCenterY - saveSzH / 2);
            DefectRect[i].width = saveSzW;
            DefectRect[i].height = saveSzH;
        }
        else
        {
            int saveW;
            int saveH;
            if (DefectRect[i].width > saveSzW)
                saveW = DefectRect[i].width;
            else
                saveW = 640;

            if (DefectRect[i].height > saveSzH)
                saveH = DefectRect[i].height;
            else
                saveH = 480;

            DefectRect[i].x = (rectCenterX - saveW / 2);
            DefectRect[i].y = (rectCenterY - saveH / 2);
            DefectRect[i].width = saveW;
            DefectRect[i].height = saveH;


        }
        byte* pHeader = pSrc;
        byte* defectROI = new byte[DefectRect[i].width * (long)DefectRect[i].height];

        for (int r = 0; r < DefectRect[i].y; r++)
            pHeader += nW;

        int targetIdx = 0;
        for (int r = DefectRect[i].y; r < DefectRect[i].y + DefectRect[i].height; r++)
        {
            memcpy(defectROI + ((long)DefectRect[i].width * targetIdx), pHeader + ((long)DefectRect[i].x), ((long)DefectRect[i].width));
            pHeader += nW;
            targetIdx++;
        }
        Mat saveROI;
        saveROI = Mat(DefectRect[i].height, DefectRect[i].width, CV_8UC1, defectROI);

        if (DefectRect[i].width != 640 || DefectRect[i].height != 480)
            resize(saveROI, saveROI, Size(640, 480));

        Path += std::to_string(i + 1) + ".BMP";
        imwrite(Path, saveROI);
    }
}
void IP::SaveDefectListBMP_Color(String sFilePath, BYTE* pR, BYTE* pG, BYTE* pB, int nW, int nH, std::vector<Rect> DefectRect)
{
    byte pRGB[3];
    Mat BGR[3];

    int saveSzW = 640;
    int saveSzH = 480;


    for (int i = 0; i < DefectRect.size(); i++)
    {
        std::string Path = sFilePath;
        int rectCenterX = DefectRect[i].x + DefectRect[i].width / 2;
        int rectCenterY = DefectRect[i].y + DefectRect[i].height / 2;

        if (DefectRect[i].x < saveSzW && DefectRect[i].y < saveSzH)
        {
            DefectRect[i].x = (rectCenterX - saveSzW / 2);
            DefectRect[i].y = (rectCenterY - saveSzH / 2);
            DefectRect[i].width = saveSzW;
            DefectRect[i].height = saveSzH;
        }
        else
        {
            int saveW;
            int saveH;
            if (DefectRect[i].width > saveSzW)
                saveW = DefectRect[i].width;
            else
                saveW = 640;

            if (DefectRect[i].height > saveSzH)
                saveH = DefectRect[i].height;
            else
                saveH = 480;

            DefectRect[i].x = (rectCenterX - saveW / 2);
            DefectRect[i].y = (rectCenterY - saveH / 2);
            DefectRect[i].width = saveW;
            DefectRect[i].height = saveH;
        }

        for (int ch = 2; ch >= 0; ch--)
        {
            byte* pHeader = &pRGB[ch];
            byte* defectROI = new byte[DefectRect[i].width * (long)DefectRect[i].height];

            for (int r = 0; r < DefectRect[i].y; r++)
                pHeader += nW;

            int targetIdx = 0;
            for (int r = DefectRect[i].y; r < DefectRect[i].y + DefectRect[i].height; r++)
            {
                memcpy(defectROI + ((long)DefectRect[i].width * targetIdx), pHeader + ((long)DefectRect[i].x), (long)DefectRect[i].width);
                pHeader += nW;
                targetIdx++;
            }
            Mat saveROI;
            saveROI = Mat(DefectRect[i].height, DefectRect[i].width, CV_8UC1, defectROI);

            if (DefectRect[i].width != 640 || DefectRect[i].height != 480)
                resize(saveROI, saveROI, Size(640, 480));

            BGR[2 - ch] = saveROI;
        }
        Mat colorImg;
        cv::merge(BGR, 3, colorImg);

        Path += std::to_string(i + 1) + ".BMP";
        imwrite(Path, colorImg);
    }
}
void IP::LoadBMP(String sFilePath, BYTE* pOut, int nW, int nH, int nByteCnt)
{
    if (nByteCnt == 1)
    {
        Mat imgSrc = imread(sFilePath, CV_LOAD_IMAGE_GRAYSCALE).clone();
        memcpy(pOut, imgSrc.data, nW * nH);
    }
    else if (nByteCnt == 3)
    {
        Mat imgSrc = imread(sFilePath).clone();
        cv::cvtColor(imgSrc, imgSrc, CV_BGR2RGB);
        memcpy(pOut, imgSrc.data, (nW / 3) * nH);
    }
}

// ETC.
void IP::SplitColorChannel(BYTE* pSrc, BYTE* pOutImg, int nW, int nH, Point ptLT, Point ptRB, int nChIndex, int nDownSample)
{
    long nIdx = 0;

    int nByteCnt = 3;
    long nWidth = (ptRB.x - ptLT.x) / nByteCnt;
    nWidth -= nWidth % nDownSample;

    long nPitch = nW / nByteCnt;
    byte* pHeader = pSrc;

    for (int i = 0; i < ptLT.y; i++)
        pHeader += (nByteCnt * nPitch * nDownSample);

    for (long j = ptLT.y; j < ptRB.y; j += nDownSample) {
        for (long i = (ptLT.x / nByteCnt); i < (ptLT.x / nByteCnt) + nWidth; i += nDownSample)
            pOutImg[nIdx++] = *(pHeader + (long)(nChIndex + (long)nByteCnt * i));
        pHeader += (nByteCnt * nPitch * nDownSample);
    }

    Mat imgSrc = Mat((ptRB.y - ptLT.y) / nDownSample, ((ptRB.x - ptLT.x) / nDownSample) / 3, CV_8UC1, pOutImg); // Debug
}
void IP::SubSampling(BYTE* pSrc, BYTE* pOutImg, int nW, int nH, Point ptLT, Point ptRB, int nDownSample)
{
    long nIdx = 0;
    long nWidth = (ptRB.x - ptLT.x);
    nWidth -= nWidth % nDownSample;

    byte* pHeader = pSrc;
    byte* pDownSample = pOutImg;
    for (int i = 0; i < ptLT.y; i++)
        pHeader += (nW * nDownSample);

    for (long j = ptLT.y; j < ptRB.y; j += nDownSample) {
        for (long i = ptLT.x; i < ptLT.x + nWidth; i += nDownSample)
            pOutImg[nIdx++] = *(pHeader + i);
        pHeader += (nW * nDownSample);
    }

    Mat imgSrc = Mat((ptRB.y - ptLT.y) / nDownSample, (ptRB.x - ptLT.x) / nDownSample, CV_8UC1, pOutImg); // Debug
}
void IP::ConvertRGB2H(BYTE* pR, BYTE* pG, BYTE* pB, BYTE* pOutH, int nW, int nH)
{
    Mat bgr[3], hsv[3];
    Mat colorImg;
    Mat HSVImg;
    Mat pH = Mat(nH, nW, CV_8UC1, pOutH);

    bgr[0] = Mat(nH, nW, CV_8UC1, pB);
    bgr[1] = Mat(nH, nW, CV_8UC1, pG);
    bgr[2] = Mat(nH, nW, CV_8UC1, pR);

    cv::merge(bgr, 3, colorImg);
    cv::cvtColor(colorImg, HSVImg, COLOR_BGR2HSV);

    cv::split(HSVImg, hsv);
    hsv[0].copyTo(pH);
}
void IP::DrawContourMap(BYTE* pSrc, BYTE* pDst, int nW, int nH)
{
    Mat pRaw = Mat(nH, nW, CV_8UC1, pSrc);
    Mat pContourMap = Mat(nH, nW, CV_8UC3, pDst);

    // ���� ���ڶ��...
    cv::GaussianBlur(pRaw, pRaw, Size(43, 43), 7);
    equalizeHist(pRaw, pRaw);

    // Raw -> Contour Map
    cv::applyColorMap(pRaw, pContourMap, COLORMAP_JET);

    // Wafer ������� Masking
    Mat CircleMask = Mat(nH, nW, CV_8UC3);
    CircleMask = Scalar(0);
    circle(CircleMask, Point(nW / 2, nH / 2), nH / 2 - 10, Scalar(1, 1, 1), -1);

    cv::multiply(CircleMask, pContourMap, pContourMap);
}
void IP::CutOutROI(BYTE* pSrc, BYTE* pDst, int nW, int nH, Point ptLT, Point ptRB)
{
    Mat pSrcImg = Mat(nH, nW, CV_8UC1, pSrc);
    Mat pDstImg = Mat(ptRB.y - ptLT.y, ptRB.x - ptLT.x, CV_8UC1, pDst);

    Mat ROI = pSrcImg(Rect(ptLT, ptRB));
    ROI.copyTo(pDstImg);
}


void IP::SobelEdgeDetection(BYTE* pSrc, BYTE* pDst, int nW, int nH, int nDerivativeX, int nDerivativeY, int nKernelSize, int nScale, int nDelta)
{
    Mat imgSrc = Mat(nH, nW, CV_8UC1, pSrc);
    Mat imgSobel = Mat();
    Mat imgAbsGradX = Mat();

    /*int nDerivativeX = 1;
    int nDerivativeY = 0;
    int nKernelSize = 5;
    int nScale = 1;
    int nDelta = 1;*/

    Sobel(imgSrc, imgSobel, CV_8UC1, nDerivativeX, nDerivativeY, nKernelSize, nScale, nDelta);
    convertScaleAbs(imgSobel, imgAbsGradX);

    pDst = imgAbsGradX.ptr<BYTE>();
}

void IP::Histogram(BYTE* pSrc, BYTE* pDst, int nW, int nH, int channels, int dims, int histSize, float* ranges)
{
    // �̿ϼ�
    /*int channels = 0;
    int histSize = 256;
    float ranges[] = new float[] { 0, 255 };
    */
    const float* histRange = { ranges };
    
    Mat matSrc = Mat(nH, nW, CV_8UC1, pSrc);
    Mat matHist;
    calcHist(&matSrc, 1, &channels, Mat(), matHist, dims, &histSize, &histRange);
}

void IP::FitEllipse(BYTE* pSrc, BYTE* pDst, int nW, int nH)
{
    Mat imgSrc = Mat(nH, nW, CV_8UC1, pSrc);
    RotatedRect ellipses = fitEllipse(imgSrc);
}