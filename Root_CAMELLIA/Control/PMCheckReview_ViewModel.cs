using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace Root_CAMELLIA
{
    public class PMCheckReview_ViewModel : ObservableObject
    {
        public PMCheckReview_ViewModel()
        {
            Init();
        }

        public void Init()
        {
            //pointListItem = new DataTable();
            p_DataTable.Columns.Add(new DataColumn("TIme"));
            p_DataTable.Columns.Add(new DataColumn("List"));
            p_DataTable.Columns.Add(new DataColumn("Result"));
        }
            DataTable m_DataTable = new DataTable();
        public DataTable p_DataTable
        {
            get
            {
                return m_DataTable;
            }
            set
            {
                SetProperty(ref m_DataTable, value);
            }
        }
        public void UpdatePMList ()
        {
            p_DataTable.Clear();
            DataRow row;



        }
    }
}
