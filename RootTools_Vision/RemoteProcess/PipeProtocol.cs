using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{

    public enum PIPE_MESSAGE_TYPE
    { 
        String,
        Object,
        Command
    }

    //[Serializable]
    //public class PipeProtocol
    //{
    //    private PIPE_MESSAGE_TYPE msgType;
    //    private int Length;

    //    public PipeProtocol(PIPE_MESSAGE_TYPE msgType, int length, int obj)
    //    {
    //        this.msgType = msgType;
    //        Length = length;
    //    }

    //    public PipeProtocol(PIPE_MESSAGE_TYPE msgType, string msg)
    //    {
    //        this.msgType = msgType;
    //        Length = length;
    //    }
    //}
}
