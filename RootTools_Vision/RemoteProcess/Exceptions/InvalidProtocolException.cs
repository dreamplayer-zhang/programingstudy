using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class InvalidProtocolException : Exception
    {
        public PIPE_MESSAGE_TYPE MsgType { get; set; }
        public string Msg { get; set; }

        public string DataType { get; set; }

        public object Data { get; set; }

        public InvalidProtocolException(PipeProtocol protocol)
        {
            this.MsgType = protocol.msgType;
            this.Msg = protocol.msg;
            this.DataType = protocol.dataType;
            this.Data = protocol.data;
        }
    }
}
