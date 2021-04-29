using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    #region [Enums]
    public enum WORK_TYPE
    {
        NONE                    = 0,
        SNAP                    = 1,
        ALIGNMENT               = 2,
        INSPECTION              = 3,
        DEFECTPROCESS           = 4,
        DEFECTPROCESS_ALL       = 5,
    }

    public enum WORKMANAGER_STATE
    {
        NONE = 0,
        CREATED,
        READY,
        STOP,
        EXIT,
        CHECK,
        ASSIGN,
        DONE,
    }

    #endregion


    #region [Structs]

    [Serializable]
    public struct MemoryID
    {
        public readonly string Pool;
        public readonly string Group;
        public readonly string Data;

        public MemoryID(string pool, string group, string data)
        {
            Pool = pool;
            Group = group;
            Data = data;
        }
    }
    #endregion
}
