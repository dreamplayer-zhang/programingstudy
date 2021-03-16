using System;

namespace RootTools.ParticleCounter
{
    public class AirnetII_310 : ParticleCounterBase
    {
        string[] _asParticleSize = { "0.3um", "0.5um", "1.0um", "5.0um" };
        public AirnetII_310(string id, Log log)
        {
            Init(id, log, _asParticleSize); 
        }
    }
}
