#include "pch.h"
#include "CD2DTest.h"
#include "math.h"
#include <immintrin.h>

#pragma warning(disable: 4101)
#pragma warning(disable: 4244)


int MyClrTest::testInt(int a, int b)
{
	return a * a + b * b;

}

void MyClrTest::testDB()
{
	/*	MySQLDBConnector^ connector = gcnew MySQLDBConnector();
		unsigned int errorCode = connector->OpenDatabase();*/
}

void MyClrTest::testTrigger(D2DPtrStruc reference, D2DPtrStruc Target)
{
	byte* pTargetPtr = Target.pByte;
	byte* pRefPtr = reference.pByte;


}

void MyClrTest::test_struc_func(D2DPtrStruc reference, D2DPtrStruc Target)
{
	byte* TargetPtr = Target.pByte;


}

void MyClrTest::SSE_MakeABS_Proto(D2DPtrStruc* target, D2DPtrStruc* reference, D2DPtrStruc* ResultABS)
{
	byte* RefPtr = reference->pByte;
	int nRefPtrWidth = reference->nPtrWidth;
	int nRefWidth = reference->nWidth;
	int nRefHeight = reference->nHeight;

	byte* TargetPtr = target->pByte;
	int nTargetPtrWidth = target->nPtrWidth;
	int nTargetWidth = target->nWidth;
	int nTargetHeight = target->nHeight;

	byte* ResultPtr = ResultABS->pByte;
	int ResultPtrWidth = ResultABS->nPtrWidth;
	int ResultWidth = ResultABS->nWidth;
	int ResultHeight = ResultABS->nHeight;

	int blockEndWidth = ceil(float(nRefWidth / 32));
	int blockEndHeight = nRefHeight;

	ULONGLONG TotalSum = 0;

	__m256i  s0 = _mm256_setzero_si256(), s1, s2, p1, p2 = _mm256_setzero_si256(), tempP2Lo, tempP2Hi, * p, sum = _mm256_setzero_si256(), zero = _mm256_setzero_si256();
	__m128i even_mask = _mm_setr_epi8(0, 129, 2, 130, 4, 131, 6, 132, 8, 133, 10, 134, 12, 135, 14, 136);
	__m128i* pp, p2_0, p2_1, p2_2, p2_3, p2Lo, p2Hi, res;

	for (int i = 0; i < blockEndHeight; i++)
	{
		p = (__m256i*)(&RefPtr[i * nRefPtrWidth]); //원본사이즈
		pp = (__m128i*)(&TargetPtr[i * nTargetPtrWidth]);//2배 증가 사이즈

		for (int j = 0; j < blockEndWidth; j++, p++, pp++)
		{
			p1 = _mm256_loadu_si256(p);

			p2_0 = _mm_shuffle_epi8(_mm_loadu_si128(pp), even_mask);
			pp++;
			p2_1 = _mm_shuffle_epi8(_mm_loadu_si128(pp), even_mask);
			pp++;
			p2_2 = _mm_shuffle_epi8(_mm_loadu_si128(pp), even_mask);
			pp++;
			p2_3 = _mm_shuffle_epi8(_mm_loadu_si128(pp), even_mask);

			p2Lo = _mm_packus_epi16(p2_0, p2_1);
			p2Hi = _mm_packus_epi16(p2_2, p2_3);

			p2 = _mm256_insertf128_si256(p2, p2Lo, 0);
			p2 = _mm256_insertf128_si256(p2, p2Hi, 1);

			s1 = _mm256_subs_epu8(p1, p2);
			s2 = _mm256_subs_epu8(p2, p1);
			s0 = _mm256_adds_epu8(s1, s2);
			//s0 = _mm256_sad_epu8(p1, p2);

		}
		byte bArray[16];
		bArray[0] = (byte)_mm256_extract_epi8(s0, 0);
		bArray[1] = (byte)_mm256_extract_epi8(s0, 1);
		bArray[2] = (byte)_mm256_extract_epi8(s0, 2);
		bArray[3] = (byte)_mm256_extract_epi8(s0, 3);
		bArray[4] = (byte)_mm256_extract_epi8(s0, 4);
		bArray[5] = (byte)_mm256_extract_epi8(s0, 5);
		bArray[6] = (byte)_mm256_extract_epi8(s0, 6);
		bArray[7] = (byte)_mm256_extract_epi8(s0, 7);
		bArray[8] = (byte)_mm256_extract_epi8(s0, 8);
		bArray[9] = (byte)_mm256_extract_epi8(s0, 9);
		bArray[10] = (byte)_mm256_extract_epi8(s0, 10);
		bArray[11] = (byte)_mm256_extract_epi8(s0, 11);
		bArray[12] = (byte)_mm256_extract_epi8(s0, 12);
		bArray[13] = (byte)_mm256_extract_epi8(s0, 13);
		bArray[14] = (byte)_mm256_extract_epi8(s0, 14);
		bArray[15] = (byte)_mm256_extract_epi8(s0, 15);

		memcpy(ResultPtr, bArray, 16);
		ResultPtr += 16;
	}
}


ULONGLONG MyClrTest::SSE_GetDiffSum4DoubleSize_256(D2DPtrStruc* Target , D2DPtrStruc* reference)
{
	byte* RefPtr = reference->pByte;
	int nRefPtrWidth = reference->nPtrWidth;
	int nRefWidth = reference->nWidth;
	int nRefHeight = reference->nHeight;

	byte* TargetPtr = Target->pByte;
	int nTargetPtrWidth = Target->nPtrWidth;
	int nTargetWidth = Target->nWidth;
	int nTargetHeight = Target->nHeight;

	int blockEndWidth = ceil(float(nRefWidth / 32));
	int blockEndHeight = nRefHeight;

	ULONGLONG TotalSum = 0;


	__m256i  s0, s1, s2, p1, p2 = _mm256_setzero_si256(), tempP2Lo, tempP2Hi, * p, sum = _mm256_setzero_si256(), zero = _mm256_setzero_si256();
	__m128i even_mask = _mm_setr_epi8(0, 129, 2, 130, 4, 131, 6, 132, 8, 133, 10, 134, 12, 135, 14, 136);
	__m128i* pp, p2_0, p2_1, p2_2, p2_3, p2Lo, p2Hi, res;

	for (int i = 0; i < blockEndHeight; i++)
	{
		p = (__m256i*)(&RefPtr[i * nRefPtrWidth]); //원본사이즈
		pp = (__m128i*)(&TargetPtr[i * nTargetPtrWidth]);//2배 증가 사이즈

		for (int j = 0; j < blockEndWidth; j++, p++, pp++)
		{
			p1 = _mm256_loadu_si256(p);

			p2_0 = _mm_shuffle_epi8(_mm_loadu_si128(pp), even_mask);
			pp++;
			p2_1 = _mm_shuffle_epi8(_mm_loadu_si128(pp), even_mask);
			pp++;
			p2_2 = _mm_shuffle_epi8(_mm_loadu_si128(pp), even_mask);
			pp++;
			p2_3 = _mm_shuffle_epi8(_mm_loadu_si128(pp), even_mask);

			p2Lo = _mm_packus_epi16(p2_0, p2_1);
			p2Hi = _mm_packus_epi16(p2_2, p2_3);

			p2 = _mm256_insertf128_si256(p2, p2Lo, 0);
			p2 = _mm256_insertf128_si256(p2, p2Hi, 1);

			//s1 = _mm256_subs_epu8(p1, p2);
			//s2 = _mm256_subs_epu8(p2, p1);
			//s0 = _mm256_adds_epu8(s1, s2);
			s0 = _mm256_sad_epu8(p1, p2);

			sum = _mm256_add_epi64(s0, sum);

		}
		res = _mm256_extractf128_si256(sum, 0);
		TotalSum += _mm_extract_epi64(res, 0) + _mm_extract_epi64(res, 1);
		res = _mm256_extractf128_si256(sum, 1);
		TotalSum += _mm_extract_epi64(res, 0) + _mm_extract_epi64(res, 1);
		sum = _mm256_setzero_si256();
	}

	return TotalSum;
}


ULONGLONG MyClrTest::SSE_GetDiffSum4DoubleSize_256_threshold(D2DPtrStruc* Target, D2DPtrStruc* reference, byte bMean)
{
	byte* RefPtr = reference->pByte;
	int nRefPtrWidth = reference->nPtrWidth;
	int nRefWidth = reference->nWidth;
	int nRefHeight = reference->nHeight;

	byte* TargetPtr = Target->pByte;
	int nTargetPtrWidth = Target->nPtrWidth;
	int nTargetWidth = Target->nWidth;
	int nTargetHeight = Target->nHeight;

	int blockEndWidth = ceil(float(nRefWidth / 32));
	int blockEndHeight = nRefHeight;

	ULONGLONG TotalSum = 0;


	__m256i  s0, s1, s2, p1, p2 = _mm256_setzero_si256(), tempP2Lo, tempP2Hi, * p, sum = _mm256_setzero_si256(), zero = _mm256_setzero_si256(), threshold = _mm256_set1_epi8(bMean);
	__m128i even_mask = _mm_setr_epi8(0, 129, 2, 130, 4, 131, 6, 132, 8, 133, 10, 134, 12, 135, 14, 136);
	__m128i* pp, p2_0, p2_1, p2_2, p2_3, p2Lo, p2Hi, res;

	for (int i = 0; i < blockEndHeight; i++)
	{
		p = (__m256i*)(&RefPtr[i * nRefPtrWidth]); //원본사이즈
		pp = (__m128i*)(&TargetPtr[i * nTargetPtrWidth]);//2배 증가 사이즈

		for (int j = 0; j < blockEndWidth; j++, p++, pp++)
		{
			p1 = _mm256_loadu_si256(p);

			p2_0 = _mm_shuffle_epi8(_mm_loadu_si128(pp), even_mask);
			pp++;
			p2_1 = _mm_shuffle_epi8(_mm_loadu_si128(pp), even_mask);
			pp++;
			p2_2 = _mm_shuffle_epi8(_mm_loadu_si128(pp), even_mask);
			pp++;
			p2_3 = _mm_shuffle_epi8(_mm_loadu_si128(pp), even_mask);

			p2Lo = _mm_packus_epi16(p2_0, p2_1);
			p2Hi = _mm_packus_epi16(p2_2, p2_3);

			p2 = _mm256_insertf128_si256(p2, p2Lo, 0);
			p2 = _mm256_insertf128_si256(p2, p2Hi, 1);

			s1 = _mm256_subs_epu8(p1, p2);
			s2 = _mm256_subs_epu8(p2, p1);
			s0 = _mm256_adds_epu8(s1, s2);
			s0 = _mm256_subs_epu8(s0, threshold);
			s0 = _mm256_sad_epu8(s0, zero);
			sum = _mm256_add_epi64(s0, sum);

		}
		res = _mm256_extractf128_si256(sum, 0);
		TotalSum += _mm_extract_epi64(res, 0) + _mm_extract_epi64(res, 1);
		res = _mm256_extractf128_si256(sum, 1);
		TotalSum += _mm_extract_epi64(res, 0) + _mm_extract_epi64(res, 1);
		sum = _mm256_setzero_si256();
	}

	return TotalSum;
}

