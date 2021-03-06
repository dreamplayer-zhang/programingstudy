using RootTools;
using System.Collections.Generic;

namespace Root_TactTime.Mold
{
    public class Mold_Buffer
    {
        Loader m_loader;
        Loader m_turn; 
        public Mold_Buffer(TactTime tact)
        {
            List<Module> aModule = tact.m_aModule;
            aModule.Add(new Module(tact, "MGZ Load", 2, new CPoint(50, 100), new RPoint(0, 0), Module.eType.Load));
            aModule.Add(new Module(tact, "MGZ Unoad", 2, new CPoint(50, 200), new RPoint(0, 0.4), Module.eType.Unload));
            aModule.Add(new Module(tact, "Boat0", 12, new CPoint(450, 100), new RPoint(0.6, 0)));
            aModule.Add(new Module(tact, "Boat1", 12, new CPoint(450, 200), new RPoint(0.6, 0.4)));
            m_loader = new Loader(tact, "Loader");
            tact.m_aLoader.Add(m_loader);
            m_loader.Add(new Picker(m_loader, "Picker0", new CPoint(250, 130), new RPoint(0, -0.2)));
            m_loader.Add(new Picker(m_loader, "Picker1", new CPoint(250, 190), new RPoint(0, 0.2)));
            m_turn = new Loader(tact, "Turn");
            tact.m_aLoader.Add(m_turn);
            m_loader.Add(new Picker(m_turn, "Picker2", new CPoint(450, 350), new RPoint(0.6, 0.2)));
        }
    }
}
