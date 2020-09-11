using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RootTools.Database
{
    public class Lotinfo
    {
        protected DateTime InspectionStart;
        protected DateTime InspectionEnd;
        protected string sLotID;
        protected string sPartID;
        protected string sSetupID;
        protected string sCSTID;
        protected string sWaferID;
        protected string sRecipeID;

        public Lotinfo()
        {

        }

        public Lotinfo(string lotid, string partid , string setupid , string cstid, string waferid, string recipeid)
        {
            sLotID = lotid;
            sPartID = partid;
            sSetupID = setupid;
            sCSTID = cstid;
            sWaferID = waferid;
            sRecipeID = recipeid;
        }


        public void SetLotinfo(string lotid, string partid, string setupid, string cstid, string waferid, string recipeid)
        {
            sLotID = lotid;
            sPartID = partid;
            sSetupID = setupid;
            sCSTID = cstid;
            sWaferID = waferid;
            sRecipeID = recipeid;
        }

        public string GetLotID() { return sLotID; }
        public string GetCSTID() { return sCSTID; }
        public string GetWaferID() { return sWaferID; }
        public string GetRecipeID() { return sRecipeID; }
    }
}
