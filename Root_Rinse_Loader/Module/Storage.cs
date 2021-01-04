using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_Rinse_Loader.Module
{
    public class Storage : ModuleBase
    {
        #region ToolBox
        DIO_Is m_diPodCheck;
        DIO_I2O2 m_dioGuide;
        Axis m_axisDoor;
        DIO_I m_diDoorOpen;
        DIO_I m_diDoorClose;
        //OHT m_OHT;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_diPodCheck, this, "POD Check", "POD Check", 3);
            p_sInfo = m_toolBox.Get(ref m_dioGuide, this, "Guide", "Up", "Down");
            p_sInfo = m_toolBox.Get(ref m_axisDoor, this, "Door");
            p_sInfo = m_toolBox.Get(ref m_diDoorOpen, this, "Door Open");
            p_sInfo = m_toolBox.Get(ref m_diDoorClose, this, "Door Close");
            //p_sInfo = m_toolBox.Get(ref m_OHT, this, p_infoCarrier, "OHT", m_diPlaced, m_diPresent);
            if (bInit) { }
        }
        #endregion

        #region Magazine
        public class Magazine : NotifyProperty
        {
            DIO_I m_diCheck;
            DIO_IO m_dioClamp;
            public void GetTools(ToolBox toolBox)
            {
                m_storage.p_sInfo = toolBox.Get(ref m_diCheck, m_storage, m_id + ".Check");
                m_storage.p_sInfo = toolBox.Get(ref m_dioClamp, m_storage, m_id + ".Clamp");
            }

            bool _bCheck = false; 
            public bool p_bCheck
            {
                get { return _bCheck; }
                set
                {
                    if (_bCheck == value) return;
                    _bCheck = value;
                    OnPropertyChanged(); 
                } 
            }

            bool _bClamp = false; 
            public bool p_bClamp
            {
                get { return _bClamp; }
                set
                {
                    if (_bClamp == value) return;
                    _bClamp = value;
                    OnPropertyChanged(); 
                }
            }

            string m_id; 
            Storage m_storage; 
            public Magazine(string id, Storage storage)
            {
                m_id = id; 
                m_storage = storage; 
            }
        }
        #endregion

        public Storage(string id, IEngineer engineer)
        {
            p_id = id;
            InitBase(id, engineer);
            //InitGAF();
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

    }
}
