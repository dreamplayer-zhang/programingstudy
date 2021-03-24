using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public enum REMOTE_PROCESS_MESSAGE_TYPE
    {
        StartSync,
        EndSync,
        StartReadyWork,
        EndReadyWork,
        StartWork,
        EndWork,
    }

    public enum REMOTE_MESSAGE_DATA
    {
        MemoryID,
        WorkManagerList,

        RecipeName,
        WorkplaceBundle,
        WorkBundle,

        DefectList,
        InspectionDone,
        IntergratedProcessDefectDone,
        WorkplaceStateChanged,
        //
        StartRemote,
        StartCreateCloneFactory,
        EndCreateCloneFactory,
        StartCreateWork,

        EndCreateWork,

        //
        Sync,
        End,
    }

    public enum REMOTE_SLAVE_MESSAGES
    {
        StartRemoteAck,
        StartCreateCloneFactoryAck,
        EndCreateCloneFactoryAck,
        StartCreateWorkAck,
        EndCreateWorkAck,
        StartWorkAck,
    }
}
