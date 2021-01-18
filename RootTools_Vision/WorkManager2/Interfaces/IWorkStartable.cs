using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public interface IWorkStartable
    {
        void Start();
        void Stop();
        void Exit();
    }
}
