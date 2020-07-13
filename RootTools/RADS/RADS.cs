using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RootTools.RADS
{
	public class RADS : ObservableObject
	{
		Log m_log;
		IEngineer m_engineer;

		string m_strID = "RADS";
		
		public RADS()
		{
			p_limit = 100;
			p_reg = new int[16];
			//m_log = m_engineer.ClassLogView().GetLog(LogView.eLogType.ENG, "RADS");
			//m_log = LogView.GetLog(m_strID, "RADS");
			m_log = null;
			m_treeRoot = new TreeRoot(m_strID, m_log);
			
			RunTree(Tree.eMode.RegRead);
			RunTree(Tree.eMode.Init);
			m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
		}

		private void M_treeRoot_UpdateTree()
		{
			RunTree(Tree.eMode.Update);
			RunTree(Tree.eMode.Init);
			RunTree(Tree.eMode.RegWrite);
		}

		public void RunTree(Tree.eMode mode)
		{
			m_treeRoot.p_eMode = mode;
			RunTreeCamera(m_treeRoot.GetTree("Camera"));
			RunTreeParameter(m_treeRoot.GetTree("Parameter"));
			RunTreeFilter(m_treeRoot.GetTree("Filter"));
		}

		void RunTreeCamera(Tree tree)
		{
			//p_IP = tree.Set(p_IP, IPAddress.Parse("0.0.0.0"), "ADS IP", "ADS IP");
		}

		void RunTreeParameter(Tree tree)
		{
			p_ySize = tree.Set(p_ySize, 100, "Y Size", "Y Size");
			p_nThreshold = tree.Set(p_nThreshold, 100, "Threshold", "Threshold");
		}

		public enum eFilterType { Mean, Unknown };
		void RunTreeFilter(Tree tree)
		{
			p_filterType = (eFilterType)tree.Set(p_filterType, eFilterType.Mean, "Filter Type", "Filter Type");
			p_hFilter_on = tree.Set(p_hFilter_on, false, "H Filter On", "Horizontal Filter On");
			p_hFilterMin = tree.Set(p_hFilterMin, 0, "H Filter Min", "Horizontal Filter Min");
			p_hFilterMax = tree.Set(p_hFilterMax, 0, "H Filter Max", "Horizontal Filter Max");
			p_wFilter_on = tree.Set(p_wFilter_on, false, "W Filter On", "W Filter On");
			p_wFilterSize = tree.Set(p_wFilterSize, 10, "W Filter Size", "W Filter Size");
			p_yFilter_on = tree.Set(p_yFilter_on, false, "Y Filter On", "Y Filter On");
			p_limit = tree.Set(p_limit, 127, "Limit", "Limit");
		}

		TreeRoot m_treeRoot = null;
		public TreeRoot p_TreeRoot
		{
			get
			{
				return m_treeRoot;
			}
			set
			{
				SetProperty(ref m_treeRoot, value);
			}
		}

		public string ADSCP_Type { get; internal set; }
		public string ADSCP_Opcode { get; internal set; }
		public string ADSCP_Length { get; internal set; }
		public byte[] RawName { get; internal set; }
		public string Name
		{
			get
			{
				//TODO : 이름 출력 형식에 대해 나중에 수정이 필요할 수 있음
				return Encoding.Default.GetString(this.RawName).Replace('\0', ' ').Trim('\u0001');
			}
		}

		string m_MAC;
		public string p_MAC
		{
			get
			{
				return m_MAC;
			}
			set
			{
				SetProperty(ref m_MAC, value);
			}
		}
		IPAddress m_IP;
		public IPAddress p_IP
		{
			get
			{
				return m_IP;
			}
			set
			{
				SetProperty(ref m_IP, value);
			}
		}
		int m_ADS_run;
		public int p_ADS_run
		{
			get
			{
				return m_ADS_run;
			}
			set
			{
				SetProperty(ref m_ADS_run, value);
			}
		}
		eFilterType m_filterType;
		public eFilterType p_filterType
		{
			get
			{
				return m_filterType;
			}
			set
			{
				SetProperty(ref m_filterType, value);
			}
		}
		bool m_yFilter_on;
		public bool p_yFilter_on
		{
			get
			{
				return m_yFilter_on;
			}
			set
			{
				SetProperty(ref m_yFilter_on, value);
			}
		}
		bool m_hFilter_on;
		public bool p_hFilter_on
		{
			get
			{
				return m_hFilter_on;
			}
			set
			{
				SetProperty(ref m_hFilter_on, value);
			}
		}
		bool m_wFilter_on;
		public bool p_wFilter_on
		{
			get
			{
				return m_wFilter_on;
			}
			set
			{
				SetProperty(ref m_wFilter_on, value);
			}
		}
		int m_ADS_view;
		public int p_ADS_view
		{
			get
			{
				return m_ADS_view;
			}
			set
			{
				SetProperty(ref m_ADS_view, value);
			}
		}
		int[] m_reg;
		public int[] p_reg
		{
			get
			{
				return m_reg;
			}
			set
			{
				SetProperty(ref m_reg, value);
			}
		}
		int m_ySize;
		public int p_ySize
		{
			get
			{
				return m_ySize;
			}
			set
			{
				SetProperty(ref m_ySize, value);
			}
		}
		int m_nThreshold;
		public int p_nThreshold
		{
			get
			{
				return m_nThreshold;
			}
			set
			{

				SetProperty(ref m_nThreshold, value);
			}
		}
		int m_hFilterMin;
		public int p_hFilterMin
		{
			get 
			{
				return m_hFilterMin;
			}
			set
			{
				SetProperty(ref m_hFilterMin, value);
			}
		}
		int m_hFilterMax;
		public int p_hFilterMax
		{
			get
			{
				return m_hFilterMax;
			}
			set
			{
				SetProperty(ref m_hFilterMax, value);
			}
		}
		int m_wFilterSize;
		public int p_wFilterSize
		{
			get
			{
				return m_wFilterSize;
			}
			set
			{
				SetProperty(ref m_wFilterSize, value);
			}
		}
		int m_limit = 123;
		public int p_limit
		{
			get
			{
				return m_limit;
			}
			set
			{
				SetProperty(ref m_limit, value);
			}
		}
		int m_offset_value;
		public int p_offset_value
		{
			get
			{
				return m_offset_value;
			}
			set
			{
				SetProperty(ref m_offset_value, value);
			}
		}
		int m_offset_updown;
		public int p_offset_updown
		{
			get
			{
				return m_offset_updown;
			}
			set
			{
				SetProperty(ref m_offset_updown, value);
			}
		}
		int m_ADS_data;
		public int p_ADS_data
		{
			get
			{
				return m_ADS_data;
			}
			set
			{
				SetProperty(ref m_ADS_data, value);
			}
		}
		int m_ADS_up;
		public int p_ADS_up
		{
			get
			{
				return m_ADS_up;
			}
			set
			{
				SetProperty(ref m_ADS_up, value);
			}
		}
		int m_ADS_down;
		public int p_ADS_down
		{
			get
			{
				return m_ADS_down;
			}
			set
			{
				SetProperty(ref m_ADS_down, value);
			}
		}

		internal string GetInformation()
		{
			return string.Format("{0} / {1} / {2}", this.p_IP, this.p_MAC, this.Name);
		}
	}
}
