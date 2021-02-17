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
    }

    public enum REMOTE_SLAVE_MESSAGES
    {
        StartRemoteAck,
        StartCreateCloneFactoryAck,
    }

    public enum REMOTE_DATA_TYPE
    {
        WorkManager,
        WorkManagerList,
        SharedBuffer,
        Work,
        WorkBundle,
        Workplace,
        WorkplaceBundle,
    }
}
