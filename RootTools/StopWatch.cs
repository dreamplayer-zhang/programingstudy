using System.Diagnostics;

namespace RootTools
{
    public class StopWatch : Stopwatch
    {
        public StopWatch()
        {
            Start(); 
        }

        public new void Start()
        {
            Restart(); 
        }

        public string p_sTime
        { 
            get
            {
                long ms = ElapsedMilliseconds;
                if (ms < 1000) return ms.ToString() + " ms";
                return (ms / 1000.0).ToString(".0") + " sec"; 
            }
        }

        int _msTimeout = 0; 
        public int p_msTimeout
        {
            get { return _msTimeout; }
            set { _msTimeout = value; }
        }

        public double p_secTimeout
        {
            get { return _msTimeout / 1000.0; }
            set { _msTimeout = (int)(value * 1000); }
        }

        public bool IsTimeover()
        {
            return (ElapsedMilliseconds > _msTimeout); 
        }
    }
}
