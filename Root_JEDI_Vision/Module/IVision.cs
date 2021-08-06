using Root_JEDI_Sorter.Module;
using RootTools;
using RootTools.Trees;
using System.Threading;

namespace Root_JEDI_Vision.Module
{
    public class LotInfo
    {
        public string m_sRecipe = "";
        public string m_sLotID = "";
        public double m_umThickness = 1000; 

        public LotInfo Clone()
        {
            return new LotInfo(m_sRecipe, m_sLotID, m_umThickness);
        }

        public string GetString()
        {
            return m_sRecipe + "," + m_sLotID;
        }

        public void RunTree(Tree tree, bool bVisible)
        {
            m_sRecipe = tree.Set(m_sRecipe, m_sRecipe, "Recipe", "Recipe Name", bVisible);
            m_sLotID = tree.Set(m_sLotID, m_sLotID, "LotID", "Lot Name", bVisible);
            m_umThickness = tree.Set(m_umThickness, m_umThickness, "Thickness", "Thickness (um)", bVisible);
        }

        public LotInfo(string sRecipe, string sLotID, double umThickness)
        {
            m_sRecipe = sRecipe;
            m_sLotID = sLotID;
            m_umThickness = umThickness; 
        }
    }

    public class SnapInfo
    {
        public enum eMode
        {
            RGB,
            APS,
            All
        }
        public eMode m_eMode = eMode.RGB; 
        public string m_sStripID = "0000";
        public int m_nLine = 0;

        public SnapInfo Clone()
        {
            return new SnapInfo(m_eMode, m_sStripID, m_nLine);
        }

        public string GetString()
        {
            int nMode = (m_eMode == eMode.All) ? 3 : (int)m_eMode; 
            return nMode.ToString() + "," + m_sStripID + "," + m_nLine.ToString();
        }

        public void RunTree(Tree tree, bool bVisible)
        {
            m_eMode = (eMode)tree.Set(m_eMode, m_eMode, "SnapMode", "Snap Mode", bVisible);
            m_sStripID = tree.Set(m_sStripID, m_sStripID, "StripID", "Strip ID", bVisible);
            m_nLine = tree.Set(m_nLine, m_nLine, "SnapLine", "Snap Line Number", bVisible);
        }

        public SnapInfo(eMode eMode, string sStripID, int nLine)
        {
            m_eMode = eMode;
            m_sStripID = sStripID;
            m_nLine = nLine;
        }
    }

    public class SortInfo
    {
        public string m_sTrayIn = "";
        public string m_sTrayOut = "";

        public SortInfo Clone()
        {
            return new SortInfo(m_sTrayIn, m_sTrayOut);
        }

        public string GetString()
        {
            return m_sTrayIn + "," + m_sTrayOut;
        }

        public void RunTree(Tree tree, bool bVisible)
        {
            m_sTrayIn = tree.Set(m_sTrayIn, m_sTrayIn, "Tray In", "Tray In", bVisible);
            m_sTrayOut = tree.Set(m_sTrayOut, m_sTrayOut, "Tray Out", "Tray Out", bVisible);
        }

        public SortInfo(string sTrayIn, string sTrayOut)
        {
            m_sTrayIn = sTrayIn;
            m_sTrayOut = sTrayOut;
        }

        public SortInfo(InfoTray infoTray)
        {
            m_sTrayIn = infoTray.m_sTrayIn;
            m_sTrayOut = infoTray.m_sTrayOut;
        }
    }

    #region Protocol
    public enum eProtocol
    {
        SnapInfo,
        SnapDone,
        LotInfo,
        SortingData,
    }

    public class Protocol
    {
        public eProtocol m_eProtocol;
        public string m_sRecipe = "";
        public string m_sSend = "";
        public string m_sInfo = "";

        bool m_bWait = true;
        public void ReceiveData(string sSend)
        {
            m_sInfo = Receive(sSend);
            m_bWait = false;
        }

        string Receive(string sSend)
        {
            int l = m_sSend.Length;
            if (sSend.Length < l) return "Message Length Error";
            if (m_sSend.Substring(0, l - 1) != sSend.Substring(0, l - 1)) return "Message not Correct";
            return sSend.Substring(l, sSend.Length - l - 1);
        }

        public string WaitReply(int secTimeout)
        {
            int msTimeout = 1000 * secTimeout;
            StopWatch sw = new StopWatch();
            while (m_bWait)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
                if (sw.ElapsedMilliseconds > msTimeout) return "Protocol Recieve Timeout";
            }
            return m_sInfo;
        }

        public Protocol(int nID, eProtocol eProtocol, string sRecipe)
        {
            m_eProtocol = eProtocol;
            m_sRecipe = sRecipe;
            m_sSend = "<" + nID.ToString("000") + "," + eProtocol.ToString() + "," + sRecipe + ">";
        }

        public int m_iSnap = 0;          // Snap Done Line Index (0 Base)
        public Protocol(int nID, eProtocol eProtocol, string sRecipe, int iSnap)
        {
            m_eProtocol = eProtocol;
            m_sRecipe = sRecipe;
            m_iSnap = iSnap;
            m_sSend = "<" + nID.ToString("000") + "," + eProtocol.ToString() + "," + sRecipe + "," + iSnap.ToString() + ">";
        }

        public int m_nSnapMode = 0;      // 0 : RGB 단일, 1 : PAS 단일, 2 : RGB, APS 모두
        public int m_nLineNum = 0;
        public Protocol(int nID, eProtocol eProtocol, string sRecipe, int nScanMode, int nLineNum)
        {
            m_eProtocol = eProtocol;
            m_sRecipe = sRecipe;
            m_nSnapMode = nScanMode;
            m_nLineNum = nLineNum;
            m_sSend = "<" + nID.ToString("000") + "," + eProtocol.ToString() + "," + sRecipe + "," + m_nSnapMode.ToString() + "," + m_nLineNum.ToString() + ">";
        }

        public SnapInfo m_snapInfo = null;
        public Protocol(int nID, eProtocol eProtocol, SnapInfo snapInfo)
        {
            m_eProtocol = eProtocol;
            m_snapInfo = snapInfo;
            m_sSend = "<" + nID.ToString("000") + "," + eProtocol.ToString() + "," + snapInfo.GetString() + ">";
        }

        public LotInfo m_lotInfo = null;
        public Protocol(int nID, eProtocol eProtocol, LotInfo lotInfo)
        {
            m_eProtocol = eProtocol;
            m_lotInfo = lotInfo;
            m_sSend = "<" + nID.ToString("000") + "," + eProtocol.ToString() + "," + lotInfo.GetString() + ">";
        }

        public SortInfo m_sortInfo = null;
        public Protocol(int nID, eProtocol eProtocol, SortInfo sortInfo)
        {
            m_eProtocol = eProtocol;
            m_sortInfo = sortInfo;
            m_sSend = "<" + nID.ToString("000") + "," + eProtocol.ToString() + "," + sortInfo.GetString() + ">";
        }
    }
    #endregion


    public interface IVision
    {
        string p_id { get; set; }
        eVision p_eVision { get; set; }
        string p_sInfo { get; set; }
        InfoTray p_infoTray { get; set; }
        string SendLotInfo(LotInfo lotInfo);
        string SendSortInfo(SortInfo sortInfo);
    }
}
