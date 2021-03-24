using System;

namespace RootTools.ParticleCounter
{
    public class LasairIII : ParticleCounterBase
    {
        string[] _asParticleSize = { "0.1um", "0.15um", "0.2um", "0.25um", "0.3um", "0.5um", "1.0um", "5.0um" };
        public LasairIII(string id, Log log)
        {
            Init(id, log, _asParticleSize); 
        }
    }
}
