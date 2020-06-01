using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools.RADS
{
	internal class RADSControlInfo : ObservableObject
	{
		// ADSCP define
		public const int ADSCP_PORT = 9032;  // ADSCP port
		public const int ADSDP_PORT = 9456;// ADS data protocol port

		//ADSCP type 
		public const string ADSCP_TYPE_REQ = "3201";    // pc to controller
		public const string ADSCP_TYPE_RSP = "3202";// controller to pc

		// ADSCP opcode define
		public const string ADSCP_OPCODE_PING = "0080";     // ping (pc 2 controller)
		public const string ADSCP_OPCODE_PONG = "0081"; // pong (controller 2 pc)
		public const string ADSCP_OPCODE_READ_REQ = "0070"; // read register request (pc 2 controller)
		public const string ADSCP_OPCODE_READ_RSP = "0071"; // read register response 
		public const string ADSCP_OPCODE_WRITE_REQ = "0072";    // write register request
		public const string ADSCP_OPCODE_WRITE_RSP = "0073";// write register response 
		public const string ADSCP_OPCODE_SEND_DATA = "0075";// send ADS data from controller to pc
		public const string ADSCP_OPCODE_SET_BASE_REQ = "0076";// set base request
		public const string ADSCP_OPCODE_SET_BASE_RSP = "0077"; // set base response
		public const string ADSCP_OPCODE_RESET_REQ = "0078";    // reset request
		public const string ADSCP_OPCODE_RESET_RSP = "0079";// reset response
	}
}
