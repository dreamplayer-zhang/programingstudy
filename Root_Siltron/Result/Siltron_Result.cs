using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_Siltron
{
    class Siltron_Result
    {
        // All Defect or Each Defect List(Front/Back/Side)?
        public List<Defect> m_listDefect = null;
    }

    public class Defect
    {
        public Defect()
        {
        }

        public eDirection m_eDirection = eDirection.Front;
        public eIdentity m_eIdentity = eIdentity.None;
        public double m_dTheta;
        public Size m_dSize;
        public int m_dGV;
        public string m_sDefectCode;
        
    }
    public enum eDirection
    {
        Front,
        Back,
        Side,
    }
    public enum eIdentity
    {
        None,
        Crack,
        Damage,
        Particle,
        Contamination,
    }
}
