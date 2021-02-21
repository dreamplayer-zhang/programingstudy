using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public enum REMOTE_MASTER_MESSAGES
    {
        StartRemote,
        StartCreateCloneFactory,
        MemoryID,
        WorkManagerList,
        EndCreateCloneFactory,
        StartCreateWork,
        RecipeName,
        WorkplaceBundle,
        WorkBundle,
        EndCreateWork,
    }

    public enum REMOTE_SLAVE_MESSAGES
    {
        StartRemoteAck,
        StartCreateCloneFactoryAck,
        EndCreateCloneFactoryAck,
        StartCreateWorkAck,
        EndCreateWorkAck,
    }
}
