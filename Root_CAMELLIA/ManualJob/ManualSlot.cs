using RootTools;
using RootTools.Gem;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Root_CAMELLIA.ManualJob
{
    public class ManualSlot : NotifyProperty
    {
        public InfoCarrier m_infoCarrier = null;
        public ManualSlot_UI m_manualSlotUI = null;

        public ManualSlot(InfoCarrier infoCarrier)
        {
            m_infoCarrier = infoCarrier;
            //Application.Current.Dispatcher.Invoke((Action)delegate
            //{
            //    Init();
            //}, DispatcherPriority.Send);
            //Init();
        }

        public void Init()
        {
            if (m_infoCarrier == null) return;
            m_manualSlotUI = new ManualSlot_UI(this);
            //Thread.Sleep(200);
            m_manualSlotUI.Init();
        }

        public void SetRecipe(string sRecipe)
        {
            m_manualSlotUI.SetRecipe(sRecipe);
        }
    }
}
