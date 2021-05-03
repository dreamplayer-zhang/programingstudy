using RootTools;

namespace Root_Pine2.Module
{
    public class InfoStrip : NotifyProperty
    {
        public string p_id { get; set; }
        public InfoStrip(string id)
        {
            p_id = id; 
        }

        public InfoStrip(int nStrip)
        {
            p_id = "Strip " + nStrip.ToString("000"); 
        }
    }
}
