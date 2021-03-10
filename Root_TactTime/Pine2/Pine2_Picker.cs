using RootTools;
using System.Collections.Generic;

namespace Root_TactTime.Pine2
{
    public class Pine2_Picker
    {
        Loader m_loader; 
        public Pine2_Picker(TactTime tact)
        {
            List<Module> aModule = tact.m_aModule;
            m_loader = new Loader(tact, "Loader0");
            tact.m_aLoader.Add(m_loader);
            m_loader.Add(new Picker(m_loader, "Picker0", new CPoint(250, 200), new RPoint(0, -0.2)));

            m_loader = new Loader(tact, "Loader1");
            tact.m_aLoader.Add(m_loader);
            m_loader.Add(new Picker(m_loader, "Picker1", new CPoint(250, 500), new RPoint(0, 0.2)));


            Module[] module3D = new Module[2];
            module3D[0] = new Module(tact, "3D 0", 15, new CPoint(450, 100), new RPoint(1.6, 0));
            module3D[1] = new Module(tact, "3D 1", 15, new CPoint(450, 170), new RPoint(1.6, 0.4));
            module3D[0].m_moduleSync = module3D[1];
            module3D[1].m_moduleSync = module3D[0];
            aModule.Add(module3D[0]);
            aModule.Add(module3D[1]);

            Module[] moduleTop = new Module[2];
            moduleTop[0] = new Module(tact, "Top 0", 10, new CPoint(450, 400), new RPoint(1.6, 1));
            moduleTop[1] = new Module(tact, "Top 1", 10, new CPoint(450, 470), new RPoint(1.6, 1.4));
            moduleTop[0].m_moduleSync = moduleTop[1];
            moduleTop[1].m_moduleSync = moduleTop[0];
            aModule.Add(moduleTop[0]);
            aModule.Add(moduleTop[1]);

            Module[] moduleBottom = new Module[2];
            moduleBottom[0] = new Module(tact, "Bottom 0", 10, new CPoint(450, 700), new RPoint(1.6, 2));
            moduleBottom[1] = new Module(tact, "Bottom 1", 10, new CPoint(450, 770), new RPoint(1.6, 2.4));
            moduleBottom[0].m_moduleSync = moduleBottom[1];
            moduleBottom[1].m_moduleSync = moduleBottom[0];
            aModule.Add(moduleBottom[0]);
            aModule.Add(moduleBottom[1]);

            m_loader = new Loader(tact, "Loader2");
            tact.m_aLoader.Add(m_loader);
            m_loader.Add(new Picker(m_loader, "Picker2", new CPoint(650, 200), new RPoint(2, 0.2)));


            aModule.Add(new Module(tact, "TurnOver", 2, new CPoint(650, 560), new RPoint(2, 1.4)));
            m_loader = new Loader(tact, "Loader3");
            tact.m_aLoader.Add(m_loader);
            m_loader.Add(new Picker(m_loader, "Turnover Unload", new CPoint(650, 620), new RPoint(0, 0.2)));

            aModule.Add(new Module(tact, "Buffer 0", 0, new CPoint(250, 320), new RPoint(0, 0)));
            aModule.Add(new Module(tact, "Buffer 1", 0, new CPoint(250, 380), new RPoint(0, 0)));

            aModule.Add(new Module(tact, "Elevator", 0, new CPoint(50, 100), new RPoint(0, 0), Module.eType.Load));
            aModule.Add(new Module(tact, "MGZ 0", 0, new CPoint(50, 200), new RPoint(0, 0.4), Module.eType.Unload));
            aModule.Add(new Module(tact, "MGZ 1", 0, new CPoint(50, 270), new RPoint(0, 0.4), Module.eType.Unload));
            aModule.Add(new Module(tact, "MGZ 2", 0, new CPoint(50, 340), new RPoint(0, 0.4), Module.eType.Unload));
            aModule.Add(new Module(tact, "MGZ 3", 0, new CPoint(50, 410), new RPoint(0, 0.4), Module.eType.Unload));
            aModule.Add(new Module(tact, "MGZ 4", 0, new CPoint(50, 480), new RPoint(0, 0.4), Module.eType.Unload));
            aModule.Add(new Module(tact, "MGZ 5", 0, new CPoint(50, 550), new RPoint(0, 0.4), Module.eType.Unload));
            aModule.Add(new Module(tact, "MGZ 6", 0, new CPoint(50, 620), new RPoint(0, 0.4), Module.eType.Unload));
            aModule.Add(new Module(tact, "MGZ 7", 0, new CPoint(50, 690), new RPoint(0, 0.4), Module.eType.Unload));
            aModule.Add(new Module(tact, "MGZ 8", 0, new CPoint(50, 760), new RPoint(0, 0.4), Module.eType.Unload));
        }
    }
}
