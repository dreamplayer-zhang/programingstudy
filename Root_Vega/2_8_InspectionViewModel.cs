using RootTools;
using RootTools.Inspects;

namespace Root_Vega
{
    public class _2_8_InspectionViewModel : ObservableObject
    {
        public Vega_Engineer m_Enginner;
        InspectionManager m_InspectionManager;
        public InspectionManager p_InspectionManager
        {
            get
            {
                return m_InspectionManager;
            }
            set
            {
                SetProperty(ref m_InspectionManager, value);
            }
        }

        public _2_8_InspectionViewModel(Vega_Engineer engineer)
        {
            m_Enginner = engineer;
            p_InspectionManager = m_Enginner.m_InspManager;
        }

        private void InspStart()
        {
            //p_InspectionManager.StartInspection(InspectionType.None, m_Image.p_Size.X, m_Image.p_Size.Y);//사용예시
        }

        public RelayCommand InspStartCommand
        {
            get
            {
                return new RelayCommand(InspStart);
            }
        }
    }
}
