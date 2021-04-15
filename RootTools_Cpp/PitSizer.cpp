#include "pch.h"
#include "PitSizer.h"

PitSizer::PitSizer(int nMaxPixelNum, int nMaskSize)
{
	m_nMaskSize = nMaskSize;
	m_nChainLength = (2 * nMaskSize + 1) * (2 * nMaskSize + 1);
	m_nMaxDefectMapLength = nMaxPixelNum * 2;

	dir = new POINT[m_nChainLength];

	MakeDir();

	m_nMaxPixelNum = nMaxPixelNum;
	Path = new POINT[m_nMaxPixelNum];
	Pixels = new POINT[m_nMaxPixelNum];
	m_pptDefectMap = new POINT[m_nMaxDefectMapLength];
}


PitSizer::~PitSizer()
{
	delete[] Path;
	delete[] Pixels;
	delete[] dir;
	delete[] m_pptDefectMap;
}


float PitSizer::GetPitSize(byte* mem, int x, int y, int nW, int RefGV, int PitValue, int nIndex, bool bGatherDefectMap)
{	//	This function entered if pixel value is below DiscolorValue
	// 요기서 Center 및 Rect가 튀어나와야함.
	POINT* p, * d;
	int gv, px, py, dx, dy;
	bool found, out = false;

	float fSizeFraction;

	if (nIndex == 1)
	{
		m_nGV = 255;
	}
	else if (nIndex == -1)
	{
		m_nGV = 0;
	}

	m_rcPitRect.left = x;
	m_rcPitRect.right = x;
	m_rcPitRect.top = y;
	m_rcPitRect.bottom = y;

	if (RefGV == PitValue)	fSizeFraction = 0.0;
	else					fSizeFraction = (float)1.0 / (RefGV - PitValue);

	if (RefGV < 1)	return 0.0;
	if (PitValue < 1)	return 0.0;

	if (nIndex == 1) {	//	finds dark pixels
		PathNum = 1;
		PitSize = 0.0;
		m_nDefectMapLength = 1;

		gv = mem[y * nW + x];
		mem[y * nW + x] = 255;
		if (gv <= PitValue) PitSize += 1.0;
		else {
			PitSize += (float)(RefGV - gv) * fSizeFraction;
		}

		p = Path;
		px = p->x = x;
		py = p->y = y;
		m_pptDefectMap[0].x = x;
		m_pptDefectMap[0].y = y;

		do
		{
			d = dir;
			found = false;
			for (int i = 0; i < m_nChainLength; i++)
			{
				if (px < 1)	px = 1;
				if (py < 1)	py = 1;

				if (mem[(py + d->y) * nW + px + d->x] < RefGV)
				{
					dx = d->x;
					dy = d->y;
					gv = mem[(py + dy) * nW + px + dx];
					mem[(py + dy) * nW + px + dx] = 255;
					if (bGatherDefectMap) {
						if (m_nDefectMapLength < m_nMaxDefectMapLength) {
							m_pptDefectMap[m_nDefectMapLength].x = px + dx;
							m_pptDefectMap[m_nDefectMapLength].y = py + dy;
							m_nDefectMapLength++;
						}
					}

					if (gv <= PitValue)
					{
						PitSize += 1.0;
						if (m_nGV > gv)
						{
							m_nGV = gv;
						}
					}
					else
					{
						PitSize += (float)(RefGV - gv) * fSizeFraction;
					}

					px = (p + 1)->x = px + dx;
					py = (p + 1)->y = py + dy;

					if (px < 1)	px = 1;
					if (py < 1)	py = 1;

					//x, y값이 현재보다 더 큰 값으로 변하지 못해
					//defect 탐색 다 해놓고 아래 m_rcPitRect 변수에 잘못된 x,y값을 넣어줘
					//현재 메소드 나간 이후 검출된 rect 가져올때 잘못된 rect 가져옴
					//if (x < px) px = x;
					//if (y < py) py = y;

					p++;
					PathNum++;
					if (PathNum > m_nMaxPixelNum - 2)	return PitSize;
					found = true;

					i = m_nChainLength + 1;
				}
				d++;
			}

			if (!found) {
				if (PathNum == 1)    out = true;
				else {
					PathNum--;
					p--;
					px = p->x;
					py = p->y;
				}
			}
			if (px > m_rcPitRect.right) {
				m_rcPitRect.right = px;
			}
			else if (px < m_rcPitRect.left) {
				m_rcPitRect.left = px;
			}
			if (py > m_rcPitRect.bottom) {
				m_rcPitRect.bottom = py;
			}
			else if (py < m_rcPitRect.top) {
				m_rcPitRect.top = py;
			}
		} while (!out);
	}
	else if (nIndex == -1) {	//	finds brighter pixels
		PathNum = 1;
		PitSize = 0.0;
		m_nDefectMapLength = 1;

		gv = mem[y * nW + x];
		mem[y * nW + x] = 0;
		if (gv >= PitValue)
		{
			PitSize += 1.0;
			if (m_nGV < gv)
			{
				m_nGV = gv;
			}
		}
		else 
		{
			PitSize += (float)(RefGV - gv) * fSizeFraction;
		}

		p = Path;
		px = p->x = x;
		py = p->y = y;
		m_pptDefectMap[0].x = x;
		m_pptDefectMap[0].y = y;

		do {
			d = dir;
			found = false;
			for (int i = 0; i < m_nChainLength; i++)
			{
				if (px < 1)	px = 1;
				if (py < 1)	py = 1;

				if (mem[(py + d->y) * nW + px + d->x] > RefGV)
				{
					dx = d->x;
					dy = d->y;
					gv = mem[(py + dy) * nW + px + dx];
					mem[(py + dy) * nW + px + dx] = 0;
					if (bGatherDefectMap) {
						if (m_nDefectMapLength < m_nMaxDefectMapLength) {
							m_pptDefectMap[m_nDefectMapLength].x = px + dx;
							m_pptDefectMap[m_nDefectMapLength].x = py + dy;
							m_nDefectMapLength++;
						}
					}

					if (gv >= PitValue)	PitSize += 1.0;
					else					PitSize += (float)(RefGV - gv) * fSizeFraction;

					px = (p + 1)->x = px + dx;
					py = (p + 1)->y = py + dy;

					if (px < 1)	px = 1;
					if (py < 1)	py = 1;

					//x, y값이 현재보다 더 큰 값으로 변하지 못해
					//defect 탐색 다 해놓고 아래 m_rcPitRect 변수에 잘못된 x,y값을 넣어줘
					//현재 메소드 나간 이후 검출된 rect 가져올때 잘못된 rect 가져옴
					//if (x < px) px = x;
					//if (y < py) py = y;

					p++;
					PathNum++;
					if (PathNum > m_nMaxPixelNum - 2)	return PitSize;
					found = true;

					i = m_nChainLength + 1;
				}
				d++;
			}

			if (!found) {
				if (PathNum == 1)    out = true;
				else {
					PathNum--;
					p--;
					px = p->x;
					py = p->y;
				}
			}
			if (px > m_rcPitRect.right) {
				m_rcPitRect.right = px;
			}
			else if (px < m_rcPitRect.left) {
				m_rcPitRect.left = px;
			}
			if (py > m_rcPitRect.bottom) {
				m_rcPitRect.bottom = py;
			}
			else if (py < m_rcPitRect.top) {
				m_rcPitRect.top = py;
			}
		} while (!out);
	}

	return PitSize;
}

void PitSizer::MakeDir(void)
{
	int nCount = 0;

	for (int y = -m_nMaskSize; y < m_nMaskSize + 1; y++) {
		for (int x = -m_nMaskSize; x < m_nMaskSize + 1; x++) {
			dir[nCount].x = x;
			dir[nCount].y = y;
			nCount++;
		}
	}
}

int PitSizer::GetPathNum()
{
	return PathNum;
}

POINT* PitSizer::GetPath()
{
	return Path;
}

int PitSizer::GetPixelNum()
{
	return m_nPixelNum;
}

POINT* PitSizer::GetPixelPointer()
{
	return Pixels;
}

PitSizer::PitSizer()
{

}

POINT* PitSizer::GetDefectMapPath()
{
	return m_pptDefectMap;
}

int PitSizer::GetDefectMapLength()
{
	return m_nDefectMapLength;
}
RECT PitSizer::GetPitRect()
{
	return m_rcPitRect;
}

int PitSizer::GetGV()
{
	return m_nGV;
}
