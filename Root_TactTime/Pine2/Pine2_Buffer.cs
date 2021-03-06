using RootTools;
using System.Collections.Generic;

namespace Root_TactTime.Pine2
{
    public class Pine2_Buffer
    {
        Loader m_loader;
        Loader m_buffer;
        Loader m_MGZ;
        public Pine2_Buffer(TactTime tact)
        {
            List<Module> aModule = tact.m_aModule;
            aModule.Add(new Module(tact, "MGZ Load", 2, new CPoint(50, 100), new RPoint(0, 0), Module.eType.Load));
            aModule.Add(new Module(tact, "MGZ Unoad", 2, new CPoint(50, 200), new RPoint(0, 0.4), Module.eType.Unload));
            aModule.Add(new Module(tact, "Boat0", 12, new CPoint(450, 100), new RPoint(0.6, 0)));
            aModule.Add(new Module(tact, "Boat1", 12, new CPoint(450, 200), new RPoint(0.6, 0.4)));
            aModule.Add(new Module(tact, "Buffer Pad", 0, new CPoint(450, 300), new RPoint(0.6, 0.8)));
            aModule.Add(new Module(tact, "Boat2", 12.5, new CPoint(450, 400), new RPoint(0.6, 0.8)));
            aModule.Add(new Module(tact, "Boat3", 12.5, new CPoint(450, 500), new RPoint(0.6, 1.2)));

            m_loader = new Loader(tact, "Loader");
            tact.m_aLoader.Add(m_loader);
            m_loader.Add(new Picker(m_loader, "Picker0", new CPoint(250, 130), new RPoint(0, -0.2)));
            m_loader.Add(new Picker(m_loader, "Picker1", new CPoint(250, 190), new RPoint(0, 0.2)));

            m_buffer = new Loader(tact, "Buffer Rail");
            tact.m_aLoader.Add(m_buffer);
            m_buffer.Add(new Picker(m_buffer, "Buffer", new CPoint(250, 350), new RPoint(0, 0)));

            m_MGZ = new Loader(tact, "MGZ Rail");
            tact.m_aLoader.Add(m_MGZ);
            m_MGZ.Add(new Picker(m_MGZ, "MGZ", new CPoint(50, 350), new RPoint(0, 0)));
        }
    }
}
