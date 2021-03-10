using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{

    public enum PIPE_MESSAGE_TYPE
    {
        Message = 0,
        Process,
        Command,
        Data,
        Event
    }

    [Serializable]
    public struct PipeProtocol
    {
        public PIPE_MESSAGE_TYPE msgType;
        public string msg;
        public string dataType;
        public object data;
        
        public PipeProtocol(PIPE_MESSAGE_TYPE msgType, string msg, string dataType = "", object data = null)
        {
            this.msgType = msgType;
            this.msg = msg;
            this.dataType = dataType;
            this.data = data;


            if (msg == "")
            {
                throw new InvalidProtocolException(this);
            }

            if (msgType == PIPE_MESSAGE_TYPE.Data || msgType == PIPE_MESSAGE_TYPE.Event)
            {
                if (dataType == "" || data == null)
                    throw new InvalidProtocolException(this);
            }
        }

        public PipeProtocol(PIPE_MESSAGE_TYPE msgType, REMOTE_MESSAGE_DATA msg, string dataType = "", object data = null)
        {
            this.msgType = msgType;
            this.msg = msg.ToString();
            this.dataType = dataType;
            this.data = data;


            if (this.msg == "")
            {
                throw new InvalidProtocolException(this);
            }

            if (msgType == PIPE_MESSAGE_TYPE.Data || msgType == PIPE_MESSAGE_TYPE.Event)
            {
                if (dataType == "" || data == null)
                    throw new InvalidProtocolException(this);
            }
        }

        public PipeProtocol(PIPE_MESSAGE_TYPE msgType, REMOTE_PROCESS_TYPE msg, string dataType = "", object data = null)
        {
            this.msgType = msgType;
            this.msg = msg.ToString();
            this.dataType = dataType;
            this.data = data;


            if (this.msg == "")
            {
                throw new InvalidProtocolException(this);
            }

            if (msgType == PIPE_MESSAGE_TYPE.Data || msgType == PIPE_MESSAGE_TYPE.Event)
            {
                if (dataType == "" || data == null)
                    throw new InvalidProtocolException(this);
            }
        }

        public PipeProtocol(PIPE_MESSAGE_TYPE msgType, REMOTE_SLAVE_MESSAGES msg, string dataType = "", object data = null)
        {
            this.msgType = msgType;
            this.msg = msg.ToString();
            this.dataType = dataType;
            this.data = data;


            if (this.msg == "")
            {
                throw new InvalidProtocolException(this);
            }

            if (msgType == PIPE_MESSAGE_TYPE.Data || msgType == PIPE_MESSAGE_TYPE.Event)
            {
                if (dataType == "" || data == null)
                    throw new InvalidProtocolException(this);
            }
        }

        //public PipeProtocol(PIPE_MESSAGE_TYPE msgType, REMOTE_DATA_TYPE msg, string dataType = "", object data = null)
        //{
        //    this.msgType = msgType;
        //    this.msg = msg.ToString();
        //    this.dataType = dataType;
        //    this.data = data;


        //    if (this.msg == "")
        //    {
        //        throw new InvalidProtocolException(this);
        //    }

        //    if (msgType == PIPE_MESSAGE_TYPE.Data || msgType == PIPE_MESSAGE_TYPE.Event)
        //    {
        //        if (dataType == "" || data == null)
        //            throw new InvalidProtocolException(this);
        //    }
        //}
    }
}
