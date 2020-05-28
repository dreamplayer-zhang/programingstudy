using RootTools.Memory;
using System.Threading.Tasks;

namespace RootTools.Camera
{
    public class Bayer2RGB
    {
        MemoryData m_memBayer;
        int m_iBayer = 0; 
        MemoryData m_memRGB;
        int m_iRGB = 0; 
        public string Convert(MemoryData memBayer, int nMemoryIndexBayer, MemoryData memRGB, int nMemoryIndexRGB)
        {
            if (memBayer.p_sz != memRGB.p_sz) return "ROI Size Mismatch";
            m_memBayer = memBayer;
            m_iBayer = nMemoryIndexBayer; 
            m_memRGB = memRGB;
            m_iRGB = nMemoryIndexRGB;
            ConvertR(1, 2);
            ConvertB(2, 1);
            ConvertGR(1, 1);
            ConvertGB(2, 2);
            return "OK";
        }

        unsafe void ConvertR(int ox, int oy)
        {
            int w = m_memBayer.p_sz.X; 
            Parallel.For(0, m_memBayer.p_sz.Y / 2 - 1, i =>
            {
                int y = 2 * i + oy;
                byte* pSrc = (byte*)m_memBayer.GetPtr(m_iBayer, ox, y);
                byte* pSrcT = pSrc - w;
                byte* pSrcB = pSrc + w;
                byte* pDst = (byte*)m_memRGB.GetPtr(m_iRGB, ox, y);
                for (int x = ox; x < m_memBayer.p_sz.X; x += 2, pSrc += 2, pSrcT += 2, pSrcB += 2)
                {
                    int nB = *(pSrcT - 1) + *(pSrcT + 1) + *(pSrcB - 1) + *(pSrcB + 1);
                    int nG = *pSrcT + *pSrcB + *(pSrc - 1) + *(pSrc + 1);
                    *pDst = (byte)(nB / 4);
                    pDst++;
                    *pDst = (byte)(nG / 4);
                    pDst++;
                    *pDst = *pSrc;
                    pDst += 4;
                }
            });
        }

        unsafe void ConvertB(int ox, int oy)
        {
            int w = m_memBayer.p_sz.X;
            Parallel.For(0, m_memBayer.p_sz.Y / 2 - 1, i =>
            {
                int y = 2 * i + oy;
                byte* pSrc = (byte*)m_memBayer.GetPtr(m_iBayer, ox, y);
                byte* pSrcT = pSrc - w;
                byte* pSrcB = pSrc + w;
                byte* pDst = (byte*)m_memRGB.GetPtr(m_iRGB, ox, y);
                for (int x = ox; x < m_memBayer.p_sz.X; x += 2, pSrc += 2, pSrcT += 2, pSrcB += 2)
                {
                    int nR = *(pSrcT - 1) + *(pSrcT + 1) + *(pSrcB - 1) + *(pSrcB + 1);
                    int nG = *pSrcT + *pSrcB + *(pSrc - 1) + *(pSrc + 1);
                    *pDst = *pSrc;
                    pDst++;
                    *pDst = (byte)(nG / 4);
                    pDst++;
                    *pDst = (byte)(nR / 4);
                    pDst += 4;
                }
            });
        }

        unsafe void ConvertGR(int ox, int oy)
        {
            int w = m_memBayer.p_sz.X;
            Parallel.For(0, m_memBayer.p_sz.Y / 2 - 1, i =>
            {
                int y = 2 * i + oy;
                byte* pSrc = (byte*)m_memBayer.GetPtr(m_iBayer, ox, y);
                byte* pSrcT = pSrc - w;
                byte* pSrcB = pSrc + w;
                byte* pDst = (byte*)m_memRGB.GetPtr(m_iRGB, ox, y);
                for (int x = ox; x < m_memBayer.p_sz.X; x += 2, pSrc += 2, pSrcT += 2, pSrcB += 2)
                {
                    int nB = *(pSrc - 1) + *(pSrc + 1);
                    int nR = *pSrcT + *pSrcB;
                    *pDst = (byte)(nB / 2);
                    pDst++;
                    *pDst = *pSrc;
                    pDst++;
                    *pDst = (byte)(nR / 2);
                    pDst += 4;
                }
            });
        }

        unsafe void ConvertGB(int ox, int oy)
        {
            int w = m_memBayer.p_sz.X;
            Parallel.For(0, m_memBayer.p_sz.Y / 2 - 1, i =>
            {
                int y = 2 * i + oy;
                byte* pSrc = (byte*)m_memBayer.GetPtr(m_iBayer, ox, y);
                byte* pSrcT = pSrc - w;
                byte* pSrcB = pSrc + w;
                byte* pDst = (byte*)m_memRGB.GetPtr(m_iRGB, ox, y);
                for (int x = ox; x < m_memBayer.p_sz.X; x += 2, pSrc += 2, pSrcT += 2, pSrcB += 2)
                {
                    int nR = *(pSrc - 1) + *(pSrc + 1);
                    int nB = *pSrcT + *pSrcB;
                    *pDst = (byte)(nB / 2);
                    pDst++;
                    *pDst = *pSrc;
                    pDst++;
                    *pDst = (byte)(nR / 2);
                    pDst += 4;
                }
            });
        }
    }
}
