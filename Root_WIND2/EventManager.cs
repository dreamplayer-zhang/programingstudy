using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    class WIND2EventManager
    {
        ///
        ///     SnapDone
        ///
        public static event EventHandler<SnapDoneArgs> SnapDone;

        public static void OnSnapDone(object obj, SnapDoneArgs args)
        {
            SnapDone?.Invoke(obj, args);
        }


    }
}
