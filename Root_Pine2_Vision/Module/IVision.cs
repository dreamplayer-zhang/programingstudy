using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;
using System.Threading;

namespace Root_Pine2_Vision.Module
{
    public enum eVision
    {
        Top3D,
        Top2D,
        Bottom
    }

    public enum eWorks
    {
        A,
        B,
    }

    public class LotInfo
    {
        public int m_nMode = 0;
        public string m_sRecipe = "";
        public string m_sLotID = "";
        public bool m_bLotMix = false;
        public bool m_bBarcode = false;
        public int m_nBarcode = 0;
        public int m_lBarcode = 0;

        public LotInfo Clone()
        {
            return new LotInfo(m_nMode, m_sRecipe, m_sLotID, m_bLotMix, m_bBarcode, m_nBarcode, m_lBarcode);
        }

        public string GetString()
        {
            return m_nMode.ToString() + "," + m_sRecipe + "," + m_sLotID + "," + (m_bBarcode ? "1," : "0,") + (m_bLotMix ? "1," : "0,") + m_nBarcode.ToString() + "," + m_lBarcode.ToString();
        }

        public void RunTree(Tree tree, bool bVisible)
        {
            m_nMode = tree.Set(m_nMode, m_nMode, "Mode", "Operation Mode (1 = Magazine, 0 = Stack)", bVisible);
            m_sRecipe = tree.Set(m_sRecipe, m_sRecipe, "Recipe", "Recipe Name", bVisible);
            m_sLotID = tree.Set(m_sLotID, m_sLotID, "LotID", "Lot Name", bVisible);
            m_bLotMix = tree.Set(m_bLotMix, m_bLotMix, "Lot Mix", "Check Lot Mix", bVisible);
            m_bBarcode = tree.Set(m_bBarcode, m_bBarcode, "Barcode", "Read Barcode", bVisible);
            m_nBarcode = tree.Set(m_nBarcode, m_nBarcode, "Barcode Start", "Read Barcode Start Pos (pixel)", bVisible);
            m_lBarcode = tree.Set(m_lBarcode, m_lBarcode, "Barcode Length", "Read Barcode Length (pixel)", bVisible);
        }

        public LotInfo(int nMode, string sRecipe, string sLotID, bool bLotMix, bool bBarcode, int nBarcode, int lBarcode)
        {
            m_nMode = nMode;
            m_sRecipe = sRecipe;
            m_sLotID = sLotID;
            m_bLotMix = bLotMix;
            m_bBarcode = bBarcode;
            m_nBarcode = nBarcode;
            m_lBarcode = lBarcode;
        }
    }

    public class SnapInfo
    {
        public eWorks m_eWorks = eWorks.A;
        public int m_nSnapMode = 0;
        public string m_sStripID = "0000";
        public int m_nLine = 0;
        public bool m_bInsp = true;

        public SnapInfo Clone()
        {
            return new SnapInfo(m_eWorks, m_nSnapMode, m_sStripID, m_nLine, m_bInsp);
        }

        public string GetString()
        {
            return m_nSnapMode.ToString() + "," + m_sStripID + "," + m_nLine.ToString() + m_bInsp.ToString();
        }

        public void RunTree(Tree tree, bool bVisible)
        {
            m_eWorks = (eWorks)tree.Set(m_eWorks, m_eWorks, "eWorks", "eWorks", bVisible);
            m_nSnapMode = tree.Set(m_nSnapMode, m_nSnapMode, "SnapMode", "Snap Mode (0 = RGB, 1 = APS, 3 = ALL)", bVisible);
            m_sStripID = tree.Set(m_sStripID, m_sStripID, "StripID", "Strip ID", bVisible);
            m_nLine = tree.Set(m_nLine, m_nLine, "SnapLine", "Snap Line Number", bVisible);
            m_bInsp = tree.Set(m_bInsp, m_bInsp, "bInsp", "True : Do Inspection (VisionWorks2)", bVisible);
        }

        public SnapInfo(eWorks eWorks, int nSnapMode, string sStripID, int nLine, bool bInsp)
        {
            m_eWorks = eWorks;
            m_nSnapMode = nSnapMode;
            m_sStripID = sStripID;
            m_nLine = nLine;
            m_bInsp = bInsp;
        }
    }
    
    public class SortInfo
    {
        public eWorks m_eWorks = eWorks.A;
        public string m_sStripID = "";
        public string m_sSortID = "";

        public SortInfo Clone()
        {
            return new SortInfo(m_eWorks, m_sStripID, m_sSortID);
        }

        public string GetString()
        {
            return m_sStripID + "," + m_sSortID;
        }

        public void RunTree(Tree tree, bool bVisible)
        {
            m_eWorks = (eWorks)tree.Set(m_eWorks, m_eWorks, "eWorks", "eWorks", bVisible);
            m_sStripID = tree.Set(m_sStripID, m_sStripID, "StripID", "StripID", bVisible);
            m_sSortID = tree.Set(m_sSortID, m_sSortID, "SortID", "SortID", bVisible);
        }

        public SortInfo(eWorks eWorks, string sStripID, string sSortID)
        {
            m_eWorks = eWorks;
            m_sStripID = sStripID;
            m_sSortID = sSortID;
        }
    }

    #region Protocol
    public enum eProtocol
    {
        SnapInfo,
        Snap,
        SnapDone,
        SnapReady,
        LotInfo,
        InspDone,
        SortingData,
        WorksConnect,
        ChangeUserset,
        Reset,
    }

    public class Protocol
    {
        public eProtocol m_eProtocol;
        public string m_sRecipe = "";
        public string m_sSend = "";
        public string m_sInfo = "";

        public void ReceiveData(string sSend)
        {
            m_sInfo = Receive(sSend);
        }

        string Receive(string sSend)
        {
            int l = m_sSend.Length;
            if (sSend.Length < l) return "Message Length Error";
            if (m_sSend.Substring(0, l - 1) != sSend.Substring(0, l - 1)) return "Message not Correct";
            return sSend.Substring(l, sSend.Length - l - 1);
        }

        public int m_iSnap = 0;          // Snap Done Line Index (0 Base)
        public Protocol(int nID, eProtocol eProtocol, string sRecipe, int iSnap)
        {
            m_eProtocol = eProtocol;
            m_sRecipe = sRecipe;
            m_iSnap = iSnap;
            m_sSend = "<" + nID.ToString("000") + "," + eProtocol.ToString() + "," + sRecipe + "," + iSnap.ToString() + ">";
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

        public Protocol(int nID, eProtocol eProtocol)
        {
            m_eProtocol = eProtocol;
            m_sSend = "<" + nID.ToString("000") + "," + eProtocol.ToString() +  ">";
        }
    }
    #endregion

    public interface IVision
    {
        eVision p_eVision { get; set; }
        List<string> p_asRecipe { get; set; }
        TreeRoot p_treeRootQueue { get; }
        ModuleBase.Remote p_remote { get; }
        string SendSnapInfo(SnapInfo snapInfo); 
        string SendLotInfo(LotInfo lotInfo);
        string SendSortInfo(SortInfo sortInfo);
    }
}
