using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RootTools.Database
{
    // 추가 필요 내용.
    // 1. 특정 테이블의 데이터를 Binding 통해 계속 갱신 가능하게끔.    // or DB의 내용 갱신 가능하게.
    // 2. 다른 프로젝트에 자유롭게 붙였다 땠다 할 수 있게.
    //DataViewer가 가진 DataSet자체를 갱신 및 바인딩.

    public class DataViewer_ViewModel : ObservableObject
    {
        public DataTable m_Dataset;
        public DataTable p_Dataset
        {
            get
    {
                return m_Dataset;
            }
            set
        {
                SetProperty(ref m_Dataset, value);
            }
        }
        
        public DataViewer_ViewModel(DataTable dataset)
        {
            m_Dataset = dataset;
        }

        public void SelectTable()
        {
            // 테이블 자체 매핑하게끔
            p_Dataset = DatabaseManager.Instance.m_DefectTable; ;
        }
    }
}
